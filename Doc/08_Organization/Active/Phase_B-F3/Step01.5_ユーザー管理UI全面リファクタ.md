# Step 01.5 組織設計・実行記録

## 🎯 Step目的（Why）

**このStepで達成すべきこと**:
- Step1 Stage3で発生した7件の品質問題を根本解決
- Application層のセキュリティ問題2件の即座修正
- 仕様書ベースでのユーザー管理UI全面書き換え（Index/Create/Edit.razor）
- Phase A初期実装（Claude Code v1時代）の品質問題を完全解消

**Phase全体における位置づけ**:
- **Phase全体の課題**: Phase A完全完成（25% → 100%）+ Phase B完全完成（100%）
- **このStepの役割**: Step1で発見された品質問題の根本解決
- **次Stepへの影響**: Step2（認証補助機能UI）の品質基盤確立

**Step追加の経緯**:
- Step1 Stage3（ユーザー動作確認）でIndex.razor初期表示だけで7件Issue発生
- 根本原因: Phase A初期実装の品質問題
- 判断: デバッグ継続より全面リファクタの方が効率的
- 採用方針: **Option A: 既存コード不信用・仕様書ベース再実装**

---

## 📋 Step概要

- **Step名**: Step01.5 ユーザー管理UI全面リファクタ
- **作業特性**: 品質改善・セキュリティ修正・再実装
- **推定期間**: 5-9時間（並列実行による効率化）
- **開始日**: 2025-11-27
- **Stage構成**: 4 Stages

---

## 📊 全層品質確認結果（2025-11-27実施）

### 品質スコア一覧

| 層 | スコア | 判定 | 再実装必要 |
|----|--------|------|-----------|
| **Domain** | 82/100 | 良好 | 3箇所（中優先度） |
| **Application** | 72/100 | 要改善 | **2箇所（🔴最優先セキュリティ）** |
| **Contracts/Infrastructure** | 82/100 | 良好 | 4箇所（優先度1-2） |
| **Web** | - | 未確認 | 全面書き換え対象 |

### 🔴 最優先対応（セキュリティ問題）

| 問題 | 層 | 影響度 | 詳細 |
|------|-----|--------|------|
| ProjectManager権限フィルタ未実装 | Application | 🔴 Critical | GetAllUsersAsync()で全ユーザー返却。権限漏洩リスク |
| 自己ロール変更禁止チェック未実装 | Application | 🔴 Critical | UpdateUserAsyncで自分のロール変更可能。権限昇格リスク |

**詳細**: `Research/00_品質確認サマリ.md` および各層レポート参照

---

## 🏢 組織設計

### 技術調査判断結果

**判断**: ✅ **技術調査完了**（品質確認として実施済み）

**実施内容**:
1. 全層品質確認（Domain/Application/Contracts/Infrastructure）
2. 機能仕様書2.2節との整合性確認
3. Clean Architecture準拠確認
4. セキュリティリスク特定

**結果**: 品質確認レポート4件を`Research/`に出力済み

### SubAgent構成（確定版）

**Stage構成方式**: セキュリティ修正→Web層並列再実装→テスト→ユーザー確認

---

## Stage構成

### Stage 1: Application層セキュリティ修正（🔴最優先）

**目的**: セキュリティ問題2件の即座修正

**SubAgent**: **fsharp-application Agent**

**推定時間**: 1-2時間

**Task 1**: ProjectManager権限フィルタ実装
- 対象: `UserManagementServices.fs` GetAllUsersAsync()
- 現状: 全ユーザーを返却（Line 79-87）
- 修正: 操作者が管理するプロジェクトのユーザーのみフィルタ

**Task 2**: 自己ロール変更禁止チェック追加
- 対象: `UserManagementServices.fs` UpdateUserAsync()
- 現状: 自分のロールを変更可能
- 修正: `userId = operator.Id` かつ `newRole <> existingUser.Role` の場合エラー

**完了基準**:
- [ ] ProjectManager権限フィルタが正しく動作
- [ ] 自己ロール変更時にエラーが返却される
- [ ] 既存テスト全Pass
- [ ] ビルド 0 Error / 0 Warning（既存Warning除く）

---

### Stage 2: Web層再実装（並列実行）

**目的**: Index/Create/Edit.razorの仕様書ベース全面書き換え

**SubAgent**: **csharp-web-ui Agent × 3（並列実行）**

**推定時間**: 2-3時間（並列化により6-9h→2-3hに短縮）

#### 🔴 並列実行の実施方法（必須遵守）

**Step1での教訓**: 1つのAgentに3つの指示を送ると順次実行になってしまう

**正しい並列実行方法**:
- **1つのメッセージ内で3つの独立したTask呼び出しを送信**
- 各TaskはそれぞれのSubAgentインスタンスとして独立実行される
- 1つのAgentに複数ファイルの実装指示を含めない

```
❌ 誤り（順次実行になる）:
Task(csharp-web-ui) → "Index.razor, Create.razor, Edit.razorを実装して"

✅ 正しい（並列実行）:
同一メッセージ内で以下を送信:
├─ Task(csharp-web-ui) → "Index.razorを実装して"
├─ Task(csharp-web-ui) → "Create.razorを実装して"
└─ Task(csharp-web-ui) → "Edit.razorを実装して"
```

**MainAgent責務**: 3つのTask呼び出しを必ず**単一のメッセージで同時に送信**すること

#### Task 2-1: Index.razor再実装

**参照仕様**: UI設計書3.6節（ユーザー一覧画面）

**実装チェックリスト**:
- [ ] ページヘッダー（タイトル、パンくずリスト）
- [ ] 検索バー（メールアドレス、ユーザー名検索）
- [ ] フィルター（ロール、ステータス）
- [ ] ユーザー一覧テーブル
- [ ] ページネーション
- [ ] 新規作成ボタン
- [ ] 各行の編集・削除ボタン
- [ ] ローディング/エラー状態表示
- [ ] data-testid属性（E2Eテスト用）

**F#↔C#境界パターン**:
```csharp
// Result型ハンドリング
var result = await UserManagementService.GetAllUsersAsync(...);
if (result.IsOk) { /* 処理 */ }
else { /* エラーハンドリング */ }
```

#### Task 2-2: Create.razor再実装

**参照仕様**: UI設計書3.7節（ユーザー登録画面）

**実装チェックリスト**:
- [ ] EditForm + DataAnnotationsValidator
- [ ] メールアドレス入力フィールド
- [ ] ユーザー名入力フィールド
- [ ] 初期パスワード入力フィールド
- [ ] ロール選択（InputRadioGroup）
- [ ] プロジェクト割り当て（チェックボックスリスト）
- [ ] 作成/キャンセルボタン
- [ ] バリデーションエラー表示
- [ ] data-testid属性

**F#↔C#境界パターン**:
```csharp
// F# list変換
Microsoft.FSharp.Collections.ListModule.OfSeq(model.AssignedProjectIds)
```

#### Task 2-3: Edit.razor再実装

**参照仕様**: UI設計書3.8節（ユーザー編集画面）

**実装チェックリスト**:
- [ ] ユーザーID読み込み（OnParametersSetAsync）
- [ ] メールアドレス表示（readonly）
- [ ] ユーザー名入力フィールド
- [ ] ロール変更（InputRadioGroup）
- [ ] ステータス変更
- [ ] パスワードリセットボタン
- [ ] プロジェクト割り当て変更
- [ ] 更新/キャンセルボタン
- [ ] ユーザー未発見エラー表示
- [ ] data-testid属性

**F#↔C#境界パターン**:
```csharp
// Option型ハンドリング
if (Microsoft.FSharp.Core.OptionModule.IsSome(userOption)) {
    var user = userOption.Value;
}
```

**並列実行**: ✅ 可能（別ファイル、.csproj競合なし）

**完了基準**:
- [ ] UI設計書3.6-3.8節の全要素実装完了
- [ ] F# Result型/Option型ハンドリング正常動作
- [ ] 初期表示エラー0件
- [ ] ビルド 0 Error

---

### Stage 3: bUnitテスト作成

**目的**: 再実装したコンポーネントのテスト

**SubAgent**: **unit-test Agent**

**推定時間**: 1-2時間

**テストファイル**:
- `IndexTests.cs`: 一覧表示・検索・フィルタ・エラー状態
- `CreateTests.cs`: バリデーション・作成成功・作成失敗
- `EditTests.cs`: ユーザー読み込み・未発見エラー・更新成功

**完了基準**:
- [ ] 各コンポーネントの主要シナリオをカバー
- [ ] テスト全件Pass

---

### Stage 4: 統合テスト・ユーザー確認

**目的**: E2Eテスト実行とユーザー動作確認

**SubAgent**: **e2e-test Agent** + **MainAgent**

**推定時間**: 1-2時間

**E2Eテスト実行**:
```bash
docker exec ubiquitous-lang-mng_devcontainer-devcontainer-1 bash tests/run-e2e-tests.sh
```

**ユーザー手動確認**:
1. **Index.razor**: 一覧表示・検索・ページネーション
2. **Create.razor**: フォーム・バリデーション・ユーザー作成
3. **Edit.razor**: ユーザー情報読み込み・更新

**完了基準**:
- [ ] E2Eテスト全件Pass
- [ ] ユーザー手動確認完了
- [ ] Step1 Stage3で発見された7件の問題解消確認

---

## オプション: 他層改善（Stage 4結果に基づき判断）

### Domain層改善（必要に応じて）
- User.createSystemAdmin移動（Infrastructure層へ）
- パスワード変更必須チェック追加
- プロジェクト所属必須チェック追加

### Infrastructure層改善（必要に応じて）
- GetByRoleAsync()完全実装
- DeleteAsync()戻り値修正

**判断基準**: Stage 4の動作確認で問題が発生した場合に実施

---

## 推定時間サマリ

| Stage | 内容 | 推定時間 |
|-------|------|----------|
| Stage 1 | Application層セキュリティ修正 | 1-2h |
| Stage 2 | Web層再実装（並列） | 2-3h |
| Stage 3 | bUnitテスト作成 | 1-2h |
| Stage 4 | 統合テスト・ユーザー確認 | 1-2h |
| **合計** | | **5-9h** |

---

## 関連ファイル

### 品質確認レポート
- `Research/00_品質確認サマリ.md`
- `Research/01_Domain層品質確認レポート.md`
- `Research/02_Application層品質確認レポート.md`
- `Research/03_Contracts_Infrastructure層品質確認レポート.md`

### 参照仕様書
- `Doc/01_Requirements/機能仕様書.md` - 2.2節
- `Doc/02_Design/UI設計/01_認証・ユーザー管理画面設計.md` - 3.6-3.8節

### Skills
- `.claude/skills/fsharp-csharp-bridge/SKILL.md`
- `.claude/skills/clean-architecture-guardian/SKILL.md`
- `.claude/skills/tdd-red-green-refactor/SKILL.md`

---

## 実行記録

### Stage 1 実行記録

**開始日時**:
**終了日時**:
**実行SubAgent**:
**結果**:

---

### Stage 2 実行記録

**開始日時**:
**終了日時**:
**実行SubAgent**:
**結果**:

---

### Stage 3 実行記録

**開始日時**:
**終了日時**:
**実行SubAgent**:
**結果**:

---

### Stage 4 実行記録

**開始日時**:
**終了日時**:
**実行SubAgent**:
**結果**:

---

## Step完了チェックリスト

- [ ] Stage 1完了: Application層セキュリティ問題2件修正
- [ ] Stage 2完了: Web層3画面再実装
- [ ] Stage 3完了: bUnitテスト作成
- [ ] Stage 4完了: 統合テスト・ユーザー確認
- [ ] ビルド: 0 Error
- [ ] テスト: 全Pass
- [ ] Step1 Stage3の7件問題: 解消確認
