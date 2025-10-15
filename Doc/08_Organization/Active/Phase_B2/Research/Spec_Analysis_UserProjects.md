# Phase B2 要件詳細分析レポート

**作成日**: 2025-10-15
**分析対象**: Phase B2 (ユーザー・プロジェクト関連管理)
**分析担当**: spec-analysis Agent
**参照仕様書**: 機能仕様書・データベース設計書・UI設計書

---

## 📋 原典仕様書読み込み確認

- ✅ 機能仕様書 - 118項目抽出（3章・4章・10章）
- ✅ データベース設計書 - 27項目抽出（5.3節・5.8節・3.1節・3.2節）
- ✅ UI設計書（プロジェクト・ドメイン管理） - 12項目抽出（3.1節・3.4節）
- ✅ Phase B1 Phase_Summary.md - 基盤技術17項目確認
- ✅ Phase B2 Phase_Summary.md - 実施計画5項目確認
- ✅ 要件定義書 - 制約事項10項目確認
- ✅ ユーザーストーリー - US-009～US-014確認
- ✅ システム設計書 - Clean Architecture設計原則確認

---

## 1. UserProjects多対多関連の要件詳細

### 1.1 テーブル設計仕様（データベース設計書 5.3節）

#### UserProjectsテーブル構造
| カラム名 | データ型 | 制約 | 説明 |
|---------|---------|------|------|
| UserProjectId | BIGSERIAL | PK | ユーザープロジェクトID（主キー） |
| UserId | VARCHAR(450) | NOT NULL, FK | ユーザーID |
| ProjectId | BIGINT | NOT NULL, FK | プロジェクトID |
| UpdatedBy | VARCHAR(450) | NOT NULL | 最終更新者ID |
| UpdatedAt | TIMESTAMPTZ | NOT NULL, DEFAULT NOW() | 最終更新日時 |

#### 制約・インデックス設計
- **複合一意制約**: `(UserId, ProjectId)` - ユーザー・プロジェクト組み合わせ一意
- **外部キー制約**:
  - `UserId → AspNetUsers.Id` (CASCADE DELETE)
  - `ProjectId → Projects.ProjectId` (CASCADE DELETE)
- **インデックス**:
  - PRIMARY KEY (UserProjectId)
  - UNIQUE INDEX IX_UserProjects_UserProject (UserId, ProjectId)
  - INDEX IX_UserProjects_ProjectId (ProjectId)
  - INDEX IX_UserProjects_UserId (UserId)

### 1.2 ビジネスルール・制約（機能仕様書 4.2.1節）

#### 多対多関連の実装方式
- **実装パターン**: 中間テーブル方式（UserProjects）
- **カスケード削除設計**:
  - ユーザー削除時: UserProjects関連レコード自動削除
  - プロジェクト削除時: UserProjects関連レコード自動削除
- **重複登録防止**: 複合一意制約による同一ユーザー・プロジェクト組み合わせ禁止

#### プロジェクトメンバー管理ルール
- **1ユーザーが複数プロジェクト所属可能**: 多対多関連の基本要件
- **1プロジェクトに複数ユーザー所属可能**: 多対多関連の基本要件
- **プロジェクト管理者判定**:
  - `AspNetUserRoles.RoleId = 'ProjectManager'` AND `UserProjects.ProjectId = 対象プロジェクトID`

### 1.3 データ整合性の要件

#### トランザクション境界
- **メンバー追加**: UserProjectsレコード単一INSERT（トランザクション単位）
- **メンバー削除**: UserProjectsレコード単一DELETE（トランザクション単位）
- **プロジェクト作成時**: ProjectsレコードINSERT + UserProjects初期レコードINSERT（同一トランザクション）

#### 同時実行制御の要件
- **楽観ロック**: `UpdatedAt`カラムによる競合制御
- **競合検知時の処理**: エラーメッセージ表示「他のユーザーが同時編集中です。画面を更新してください。」
- **デッドロック対策**: 楽観ロック優先、必要時のみ悲観ロック

---

## 2. 権限制御マトリックス拡張計画（6→16パターン）

### 2.1 Phase B1実装済み6パターン

| ロール | 操作 | プロジェクト範囲 | 実装状況 |
|--------|------|-----------------|---------|
| SuperUser | プロジェクト作成 | 全プロジェクト | ✅ 完了 |
| SuperUser | プロジェクト編集 | 全プロジェクト | ✅ 完了 |
| SuperUser | プロジェクト削除 | 全プロジェクト | ✅ 完了 |
| SuperUser | プロジェクト一覧表示 | 全プロジェクト | ✅ 完了 |
| ProjectManager | プロジェクト編集 | 担当プロジェクトのみ | ✅ 完了 |
| ProjectManager | プロジェクト一覧表示 | 担当プロジェクトのみ | ✅ 完了 |

### 2.2 Phase B2拡張対象10パターン

#### 新規追加パターン（DomainApprover権限）
| ロール | 操作 | プロジェクト範囲 | 優先度 |
|--------|------|-----------------|--------|
| DomainApprover | プロジェクト一覧表示 | 所属プロジェクトのみ | Must have |
| DomainApprover | プロジェクトメンバー一覧表示 | 所属プロジェクトのみ | Must have |
| DomainApprover | プロジェクト詳細表示 | 所属プロジェクトのみ | Should have |

#### 新規追加パターン（GeneralUser権限）
| ロール | 操作 | プロジェクト範囲 | 優先度 |
|--------|------|-----------------|--------|
| GeneralUser | プロジェクト一覧表示 | 所属プロジェクトのみ | Must have |
| GeneralUser | プロジェクトメンバー一覧表示 | 所属プロジェクトのみ | Must have |
| GeneralUser | プロジェクト詳細表示 | 所属プロジェクトのみ | Should have |

#### メンバー管理パターン（SuperUser/ProjectManager専用）
| ロール | 操作 | プロジェクト範囲 | 優先度 |
|--------|------|-----------------|--------|
| SuperUser | メンバー追加 | 全プロジェクト | Must have |
| SuperUser | メンバー削除 | 全プロジェクト | Must have |
| ProjectManager | メンバー追加 | 担当プロジェクトのみ | Must have |
| ProjectManager | メンバー削除 | 担当プロジェクトのみ | Must have |

### 2.3 権限判定ロジックの複雑度分析

#### プロジェクトメンバー判定（2段階チェック）
```csharp
// 1段階: ロール判定
bool isSuperUser = user.Role == "SuperUser";
bool isProjectManager = user.Role == "ProjectManager";
bool isDomainApprover = user.Role == "DomainApprover";
bool isGeneralUser = user.Role == "GeneralUser";

// 2段階: プロジェクトメンバー判定（SuperUser以外）
bool isMemberOfProject = await context.UserProjects
    .AnyAsync(up => up.UserId == userId && up.ProjectId == projectId);
```

---

## 3. プロジェクトメンバー管理UI仕様

### 3.1 UI設計詳細

#### プロジェクト一覧画面拡張
```
┌─────────────────────────────────────────────────────────┐
│                📁 プロジェクト一覧                        │
│                                                         │
│ ┌─────────────────────────────────────────────────────┐ │
│ │📁名称      │📅作成日│👤管理者│👥メンバー数│🏷️ドメイン数│ │
│ ├───────────┼───────┼───────┼──────────┼──────────┤ │
│ │ECサイト    │2025/01│田中    │5名       │5個       │ │
│ │在庫管理    │2025/02│佐藤    │3名       │3個       │ │
│ │            │       │       │✏️[編集]🗑️[削除]👥[メンバー]│ │
│ └─────────────────────────────────────────────────────┘ │
└─────────────────────────────────────────────────────────┘
```

#### メンバー管理画面（新規追加）
```
┌─────────────────────────────────────────────────────────┐
│            👥 プロジェクトメンバー管理                    │
│            プロジェクト: ECサイト                         │
│                                                         │
│ ┌─────────────────────────────────────────────────────┐ │
│ │ 📊 メンバー一覧 (5名)                                 │ │
│ ├─────────────────────────────────────────────────────┤ │
│ │ 👤氏名       │📧メール          │🎭ロール       │     │ │
│ ├─────────────┼─────────────────┼──────────────┤     │ │
│ │ 田中太郎     │tanaka@example.com│ProjectManager│🗑️  │ │
│ │ 佐藤花子     │sato@example.com  │DomainApprover│🗑️  │ │
│ └─────────────────────────────────────────────────────┘ │
│                                                         │
│ ┌─────────────────────────────────────────────────────┐ │
│ │ ➕ メンバー追加                                       │ │
│ │ 👤 ユーザー選択: [全ユーザー一覧から選択 ▼]           │ │
│ │ ✅ [追加] ❌ [キャンセル]                             │ │
│ └─────────────────────────────────────────────────────┘ │
└─────────────────────────────────────────────────────────┘
```

### 3.2 ユーザー操作フロー

#### メンバー追加フロー
1. プロジェクト一覧画面で「👥 メンバー」ボタンをクリック
2. メンバー管理画面に遷移
3. 「➕ メンバー追加」セクションでユーザーをドロップダウンから選択
4. 「✅ 追加」ボタンをクリック
5. UserProjectsテーブルにレコードINSERT
6. 成功メッセージ表示「{ユーザー名}をプロジェクトに追加しました」
7. メンバー一覧を自動更新

#### メンバー削除フロー
1. メンバー管理画面で削除対象ユーザーの「🗑️」ボタンをクリック
2. 削除確認ダイアログ表示「{ユーザー名}をプロジェクトから削除してもよろしいですか？」
3. 「確認」ボタンをクリック
4. UserProjectsテーブルからレコードDELETE
5. 成功メッセージ表示
6. メンバー一覧を自動更新

#### エラーハンドリング・バリデーション
- **重複追加エラー**: 「{ユーザー名}は既にこのプロジェクトのメンバーです」
- **最後の管理者削除エラー**: 「プロジェクトには最低1名のプロジェクト管理者が必要です」
- **権限不足エラー**: 「プロジェクトメンバー管理権限がありません」

---

## 4. Phase B1技術負債4件の解消計画

### 4.1 InputRadioGroup制約（2件）

#### 問題内容
Blazor Server/bUnit既知の制約により、InputRadioGroupコンポーネントのテストが困難

#### Phase B2対応方針（Step5実施）
- **Option 2推奨**: カスタムラジオボタンコンポーネント実装
- **理由**: UI/UX一貫性維持・bUnitテスト基盤活用

#### Step5実装計画
- Phase 1: カスタムラジオボタンコンポーネント実装（30分）
- Phase 2: 既存画面への適用（20分）
- Phase 3: bUnitテスト実装（30分）
- Phase 4: 統合テスト・品質確認（20分）

### 4.2 フォーム送信詳細テスト（1件）

#### 問題内容
フォーム送信ロジック未トリガー・bUnitテストでのフォームSubmitイベント検証困難

#### Phase B2対応方針（Step5実施）
- **Option 2推奨**: Playwright E2Eテスト + bUnit部分統合
- **理由**: Phase B2でPlaywright統合実装予定

#### Step5実装計画
- Phase 1: Playwright E2Eテスト実装（40分）
- Phase 2: bUnit部分統合テスト実装（30分）
- Phase 3: 統合テスト・品質確認（20分）

### 4.3 Null参照警告（1件）

#### 問題内容
ProjectManagementServiceMockBuilder.cs:206のNull参照警告

#### Phase B2対応方針（Step5実施）
- **Option 1 + Option 2推奨**: Null許容参照型 + デフォルト値明示

#### Step5実装計画
- Phase 1: Null許容参照型有効化（10分）
- Phase 2: Null警告解消（20分）
- Phase 3: ビルド・テスト実行（10分）

---

## 5. 実装推奨事項

### 5.1 優先実装項目（Must have）

1. **UserProjects中間テーブル実装**（Step3: Domain層）
2. **権限制御マトリックス拡張**（Step3: Domain層）
3. **ProjectRepository拡張**（Step4: Infrastructure層）
4. **メンバー管理UI実装**（Step5: Web層）

### 5.2 リスク要因

- **技術的リスク**: 複合一意制約・カスケード削除・権限制御ロジック
- **プロセスリスク**: Phase B1技術負債解消・Playwright MCP統合効率

### 5.3 実装時の注意点

#### Clean Architecture境界の維持
- Domain層: F# UserProject Aggregate（外部依存なし）
- Application層: F# IProjectManagementService拡張
- Contracts層: F#↔C# TypeConverter拡張
- Infrastructure層: C# ProjectRepository拡張
- Web層: C# Blazor Serverコンポーネント

---

## 6. Step2-5への申し送り事項

### Step2実装時の参照事項
- Playwright MCP統合: Phase B2 Phase_Summary.md Step2実施内容準拠
- E2Eテスト作成: UserProjectsシナリオE2Eテスト実装

### Step3実装時の参照事項
- 本レポート 1章: UserProjects多対多関連の要件詳細
- 本レポート 2章: 権限制御マトリックス拡張計画

### Step4実装時の参照事項
- 本レポート 1.1節: UserProjectsテーブル設計仕様
- 本レポート 1.2節: ビジネスルール・制約

### Step5実装時の参照事項
- 本レポート 3章: プロジェクトメンバー管理UI仕様
- 本レポート 4章: Phase B1技術負債4件の解消計画

---

**分析完了日**: 2025-10-15
**分析実施者**: spec-analysis Agent
**次工程**: Step2 Playwright MCP + Agents統合実装
