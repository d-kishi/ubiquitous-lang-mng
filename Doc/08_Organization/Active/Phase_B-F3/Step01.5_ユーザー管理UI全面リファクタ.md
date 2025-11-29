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

### Stage 1: セキュリティ問題完全修正（🔴最優先）

**目的**: セキュリティ問題2件の完全修正（Application層 + Infrastructure層）

**SubAgent**:
- **fsharp-application Agent**（Task 1-1, Task 1-3, Task 2）
- **csharp-infrastructure Agent**（Task 1-2）

**推定時間**: 1.5-2時間（並列実行による20-30%短縮）

**変更経緯**:
- 当初計画ではApplication層のみの修正を想定
- 調査の結果、Task1完全実装にはInfrastructure層拡張が必要と判明
- セキュリティ問題の「即座修正」目的を達成するため、Stage1範囲を拡張
- Task 2, Task 1-1, Task 1-2は並列実行可能と判断（品質リスク低）

---

#### 並列実行グループ（Task 2 + Task 1-1 + Task 1-2）

**並列実行の前提条件**: メソッドシグネチャを事前確定

**確定シグネチャ**:
```fsharp
// F# (IUserRepository)
abstract member GetUsersByProjectIdsAsync: ProjectId list -> Task<Result<User list, string>>
```
```csharp
// C# (UserRepository)
Task<FSharpResult<FSharpList<User>, string>> GetUsersByProjectIdsAsync(FSharpList<ProjectId> projectIds)
```

**Task 1-1**: IUserRepositoryメソッド追加（Application層）【並列】
- 対象: `Interfaces.fs` IUserRepository
- 追加: 上記シグネチャのメソッド定義
- SubAgent: fsharp-application Agent

**Task 1-2**: UserRepository実装追加（Infrastructure層）【並列】
- 対象: `UserRepository.cs`
- 実装: 指定プロジェクトIDリストに所属するユーザー一覧を取得
- ロジック: UserProjectsテーブルをJOINし、重複排除してユーザー一覧返却
- SubAgent: csharp-infrastructure Agent

**Task 2**: 自己ロール変更禁止チェック追加【並列】
- 対象: `UserManagementServices.fs` UpdateUserAsync() Line 302-306
- 修正: Step 6の後に自己ロール変更禁止チェック追加
- 仕様根拠: 機能仕様書2.2.2節「自分自身のロール変更不可（権限昇格防止）」
- SubAgent: fsharp-application Agent

---

#### 順次実行（Task 1-3）

**Task 1-3**: GetAllUsersAsync権限フィルタ適用（Application層）【Task 1-1, 1-2完了後】
- 対象: `UserManagementServices.fs` GetAllUsersAsync() Line 79-87
- 修正内容:
  1. 操作者のUserProjectsからプロジェクトID一覧を取得
  2. GetUsersByProjectIdsAsyncで該当ユーザーを取得
  3. フィルタ済みユーザー一覧を返却
- SubAgent: fsharp-application Agent

---

**完了基準**:
- [x] Task 2: 自己ロール変更時にエラーが返却される
- [x] Task 1: ProjectManager権限フィルタが正しく動作
  - [x] Task 1-1: IUserRepository.GetUsersByProjectIdsAsync追加
  - [x] Task 1-2: UserRepository.GetUsersByProjectIdsAsync実装
  - [x] Task 1-3: GetAllUsersAsync権限フィルタ適用
- [x] 既存テスト全Pass（Application層 32/32）
- [x] ビルド 0 Error / 0 Warning（既存Warning除く）

---

### Stage 2: Web層再実装（並列実行）【PhaseB2成果活用版】

**目的**: Index/Create/Edit.razorの仕様書ベース全面書き換え

**SubAgent**: **csharp-web-ui Agent × 3（並列実行）**

**推定時間**: 2-3時間（並列化により6-9h→2-3hに短縮）

**更新経緯**（2025-11-29）:
- PhaseB2完了成果を踏まえた計画最適化
- 高品質なPhaseB2成果物（ProjectList.razor等）を参考パターンとして活用
- data-testid属性を必須要件に格上げ

---

#### 🆕 PhaseB2成果活用（参考可否マトリックス）

| コンポーネント | 作成時期 | 品質 | 参考可否 |
|--------------|---------|------|---------|
| `ProjectList.razor` | PhaseB2 | 高品質 | ✅ 参考可 |
| `ProjectCreate.razor` | PhaseB2 | 高品質 | ✅ 参考可 |
| `ProjectEdit.razor` | PhaseB2 | 高品質 | ✅ 参考可 |
| `Index.razor`（Users） | Step1 | 品質問題 | ❌ 参考不可（書き換え対象） |
| `Create.razor`（Users） | Step1 | 品質問題 | ❌ 参考不可（書き換え対象） |
| `Edit.razor`（Users） | Step1 | 品質問題 | ❌ 参考不可（書き換え対象） |
| 旧`UserManagement.razor` | PhaseA | 低品質 | ❌ 参考不可（削除済み） |

**参考パターン（ProjectList.razor等から抽出）**:

1. **F# Result型ハンドリング**:
```csharp
var result = await service.MethodAsync(...);
if (result.IsOk) {
    var data = result.ResultValue;
    // 処理
} else {
    errorMessage = result.ErrorValue;
}
```

2. **権限制御UIパターン（SecureButton活用）**:
```razor
<SecureButton Text="新規作成"
             RequiredRoles='new List<string> { "SuperUser", "ProjectManager" }'
             RequiredPermission="CreateUser"
             OnClick="NavigateToCreate" />
```

3. **ローディング/エラー状態表示**:
```razor
@if (isLoading) {
    <div class="spinner-border text-primary">...</div>
} else if (!string.IsNullOrEmpty(errorMessage)) {
    <div class="alert alert-danger">@errorMessage</div>
} else {
    <!-- コンテンツ -->
}
```

4. **data-testid属性付与（🔴必須）**:
```razor
<button data-testid="user-create-button">新規作成</button>
<table data-testid="user-list-table">...</table>
```

---

#### 必須参照ファイル（Stage2開始前）

| ファイル | 目的 |
|---------|------|
| `Doc/02_Design/UI設計/01_認証・ユーザー管理画面設計.md` 3.6-3.8節 | 実装仕様確認（必須） |
| `Components/Pages/ProjectManagement/ProjectList.razor` | 参考パターン（PhaseB2成果） |
| `Components/Pages/ProjectManagement/ProjectCreate.razor` | フォーム実装パターン参考 |
| `Components/Pages/ProjectManagement/ProjectEdit.razor` | 編集画面パターン参考 |

---

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

---

#### Task 2-1: Index.razor再実装

**参照仕様**: UI設計書3.6節（ユーザー一覧画面）

**実装チェックリスト**:
- [ ] ページヘッダー（タイトル、パンくずリスト）
- [ ] 検索バー（メールアドレス、ユーザー名検索）
- [ ] フィルター（ロール、ステータス）
- [ ] ユーザー一覧テーブル
- [ ] ページネーション
- [ ] 新規作成ボタン（SecureButton活用）
- [ ] 各行の編集・削除ボタン（権限制御）
- [ ] ローディング/エラー状態表示
- [ ] **data-testid属性（🔴必須・E2Eテスト用）**

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
- [ ] **data-testid属性（🔴必須）**

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
- [ ] ロール変更（InputRadioGroup）※自己変更禁止対応済み（Stage1）
- [ ] ステータス変更
- [ ] パスワードリセットボタン
- [ ] プロジェクト割り当て変更
- [ ] 更新/キャンセルボタン
- [ ] ユーザー未発見エラー表示
- [ ] **data-testid属性（🔴必須）**

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
- [ ] **data-testid属性が全コンポーネントに付与されている**
- [ ] 初期表示エラー0件
- [ ] ビルド 0 Error

---

### Stage 3: bUnitテスト作成【PhaseB2成果活用版】

**目的**: 再実装したコンポーネントのテスト

**SubAgent**: **unit-test Agent**

**推定時間**: 1-2時間

**更新経緯**（2025-11-29）:
- PhaseB2で確立されたテストアーキテクチャ基盤を活用

---

#### 🆕 PhaseB2テストアーキテクチャ基盤活用

**活用する基盤**:
- `BlazorComponentTestBase`: テストベースクラス
- `FSharpTypeHelpers`: F#型変換ヘルパー
- `ProjectManagementServiceMockBuilder`: モック構築パターン

**参考可能テストファイル**:
- `tests/UbiquitousLanguageManager.Web.UI.Tests/ProjectManagement/` 配下のテスト

---

**テストファイル**:
- `IndexTests.cs`: 一覧表示・検索・フィルタ・エラー状態
- `CreateTests.cs`: バリデーション・作成成功・作成失敗
- `EditTests.cs`: ユーザー読み込み・未発見エラー・更新成功

**完了基準**:
- [ ] 各コンポーネントの主要シナリオをカバー
- [ ] テスト全件Pass
- [ ] **ADR_020テストアーキテクチャ準拠**

---

### Stage 4: 統合テスト・ユーザー確認【PhaseB2成果活用版】

**目的**: E2Eテスト実行とユーザー動作確認

**SubAgent**: **e2e-test Agent** + **MainAgent**

**推定時間**: 1-2時間

**更新経緯**（2025-11-29）:
- PhaseB-F2で完了したTypeScript/Playwright Test環境を活用
- Playwright Test Generator/Healer Agents活用を明記

---

#### 🆕 PhaseB2/B-F2成果活用

**E2Eテスト環境（TypeScript/Playwright Test移行完了済み）**:
```bash
# 一括実行（推奨）
docker exec ubiquitous-lang-mng_devcontainer-devcontainer-1 bash tests/run-e2e-tests.sh

# 特定テストファイル
docker exec ubiquitous-lang-mng_devcontainer-devcontainer-1 bash tests/run-e2e-tests.sh user-management.spec.ts
```

**既存E2Eテスト構造（参考）**:
- `tests/UbiquitousLanguageManager.E2E.Tests/authentication.spec.ts`
- `tests/UbiquitousLanguageManager.E2E.Tests/user-projects.spec.ts`
- `tests/UbiquitousLanguageManager.E2E.Tests/seed.spec.ts`

**🆕 user-management.spec.ts新規作成**:
- ユーザー一覧表示（2シナリオ）
- ユーザー登録機能（3シナリオ）
- ユーザー編集機能（3シナリオ）
- ユーザー削除機能（2シナリオ）

**Playwright Test Generator/Healer Agents活用**:
- Generator Agent: テスト作成時間40-50%削減
- Healer Agent: テスト失敗時のデバッグ支援
- **playwright-e2e-patterns SKILL適用**

---

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
- `.claude/skills/playwright-e2e-patterns/SKILL.md`（Stage4 E2Eテスト用）

### PhaseB2参考コンポーネント（Stage2用）
- `src/UbiquitousLanguageManager.Web/Components/Pages/ProjectManagement/ProjectList.razor`
- `src/UbiquitousLanguageManager.Web/Components/Pages/ProjectManagement/ProjectCreate.razor`
- `src/UbiquitousLanguageManager.Web/Components/Pages/ProjectManagement/ProjectEdit.razor`

---

## 実行記録

### Stage 1 実行記録

**開始日時**: 2025-11-29
**終了日時**: 2025-11-29
**実行SubAgent**:
- fsharp-application Agent × 4（Task 2, Task 1-1, Task 1-3a, Task 1-3c）
- csharp-infrastructure Agent × 2（Task 1-2, Task 1-3b）

**実施Task**:
- ✅ Task 2: 自己ロール変更禁止チェック追加（UserManagementServices.fs Line 309-323）
- ✅ Task 1-1: IUserRepository.GetUsersByProjectIdsAsync追加（Interfaces.fs Line 39-61）
- ✅ Task 1-2: UserRepository.GetUsersByProjectIdsAsync実装（UserRepository.cs Line 221-304, UserRepositoryAdapter.cs Line 304-333）
- ✅ Task 1-3a: IUserRepository.GetProjectIdsByUserIdAsync追加（Interfaces.fs Line 63-82）
- ✅ Task 1-3b: UserRepository.GetProjectIdsByUserIdAsync実装（UserRepository.cs Line 316-373, UserRepositoryAdapter.cs Line 335-364）
- ✅ Task 1-3c: GetAllUsersAsync権限フィルタ適用（UserManagementServices.fs Line 79-111）

**修正ファイル一覧**:
| ファイル | 修正内容 |
|---------|---------|
| `src/UbiquitousLanguageManager.Application/Interfaces.fs` | GetUsersByProjectIdsAsync, GetProjectIdsByUserIdAsync追加 |
| `src/UbiquitousLanguageManager.Application/UserManagementServices.fs` | ProjectManager権限フィルタ適用、自己ロール変更禁止チェック追加 |
| `src/UbiquitousLanguageManager.Infrastructure/Repositories/UserRepository.cs` | GetUsersByProjectIdsAsync, GetProjectIdsByUserIdAsync実装 |
| `src/UbiquitousLanguageManager.Infrastructure/Repositories/UserRepositoryAdapter.cs` | GetUsersByProjectIdsAsync, GetProjectIdsByUserIdAsync実装 |

**結果**: ✅ 完了
- ビルド: 0 Error（既存Warning除く）
- テスト: Domain層 113/113 Pass、Application層 32/32 Pass、Contracts層 98/98 Pass、Infrastructure層 98/98 Pass
- セキュリティ問題2件修正完了
  - 自己ロール変更禁止チェック実装（機能仕様書2.2.2節準拠）
  - ProjectManager権限フィルタ実装（機能仕様書2.2節準拠）

**備考**:
- 当初計画ではTask 1-1/1-2/Task 2の並列実行のみだったが、Task 1-3実装時にGetProjectIdsByUserIdAsyncが必要と判明
- Task 1-3をTask 1-3a（Interface追加）、Task 1-3b（Repository実装）、Task 1-3c（フィルタ適用）に分割して実施

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

- [x] Stage 1完了: Application層セキュリティ問題2件修正（2025-11-29）
- [ ] Stage 2完了: Web層3画面再実装
- [ ] Stage 3完了: bUnitテスト作成
- [ ] Stage 4完了: 統合テスト・ユーザー確認
- [x] ビルド: 0 Error（既存Warning除く）
- [ ] テスト: 全Pass
- [ ] Step1 Stage3の7件問題: 解消確認
