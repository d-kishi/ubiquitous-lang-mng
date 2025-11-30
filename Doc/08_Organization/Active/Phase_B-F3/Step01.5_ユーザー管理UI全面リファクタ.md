# Step 01.5 組織設計・実行記録

## 🎯 Step目的（Why）

**このStepで達成すべきこと**:
- Step1 Stage3で発生した7件の品質問題を根本解決
- Application層のセキュリティ問題2件の即座修正 ← ✅Stage1完了
- **Infrastructure層UserRepositoryの完全実装**（簡易実装からの脱却）← 🆕追加
- **Application層の権限フィルタ・プロジェクト割り当て完全実装** ← 🆕追加
- 仕様書ベースでのユーザー管理UI全面書き換え（Index/Create/Edit.razor）
- Phase A初期実装（Claude Code v1時代）の品質問題を完全解消

**Phase全体における位置づけ**:
- **Phase全体の課題**: Phase A完全完成（25% → 100%）+ Phase B完全完成（100%）
- **このStepの役割**: 全層を通したユーザー管理機能の完全リファクタ
- **次Stepへの影響**: Step2（認証補助機能UI）の品質基盤確立

**Step計画改訂の経緯**（2025-11-30）:
- Stage2（Web層再実装のみ）を過去2回実施したが、実行エラーが続いた
- 調査の結果、**Infrastructure層（UserRepository）が簡易実装のまま**であることが判明
- **UIだけ書き換えても、下位層が動作しないため効果がない**と判断
- 方針転換: **「最初から作り直すつもりで」下位層から積み上げ実装**

---

## 📋 Step概要

- **Step名**: Step01.5 ユーザー管理機能全面リファクタ
- **作業特性**: 品質改善・セキュリティ修正・全層再実装
- **推定期間**: 11-15時間（2-3セッション）← 🆕改訂
- **開始日**: 2025-11-27
- **Stage構成**: 5 Stages ← 🆕改訂（4→5）

---

## 📊 全層調査結果（2025-11-30実施）

### 層別完成度サマリ

| 層 | 完成度 | 判定 | リファクタ必要性 |
|----|--------|------|-----------------|
| **Domain** | ✅100% | 完成 | なし |
| **Application** | ⚠️90% | 要改善 | あり（権限フィルタ・プロジェクト割り当て） |
| **Contracts** | ✅100% | 完成 | なし |
| **Infrastructure** | ❌50% | 重大問題 | **必須**（全メソッド実装） |
| **Web** | ❌20% | 重大問題 | **必須**（全画面書き換え） |

### 🔴 重大問題（Infrastructure層）

| 問題 | 影響度 | 詳細 |
|------|--------|------|
| `GetHashCode()`によるID変換 | 🔴致命的 | ハッシュ衝突リスク、実行環境依存で不安定 |
| `GetByEmailAsync`ハードコード | 🔴致命的 | DB検索なし、ダミーユーザー返却 |
| `SaveAsync`永続化なし | 🔴致命的 | `Task.Delay(1)`のみ、DBに保存されない |
| `DeleteAsync`未実装 | 🟡高 | 常にエラー返却 |

### ⚠️ 未実装機能（Application層）

| 問題 | 影響度 | 詳細 |
|------|--------|------|
| `GetUserByIdAsync`ProjectManager権限フィルタ | 🟡高 | TODOコメントのまま |
| プロジェクト割り当て機能 | 🟡高 | `assignedProjectIds`パラメータ未使用 |

**詳細**: `Research/UserManagement_全層調査レポート.md` 参照

---

## 🏢 組織設計

### 技術調査判断結果

**判断**: ✅ **技術調査完了**（全層品質確認として実施済み）

**実施内容**:
1. 全層品質確認（Domain/Application/Contracts/Infrastructure/Web）
2. UI設計書3.6-3.8章との整合性確認
3. ProjectManagement参考パターン評価
4. 問題点・リスク特定

**結果**: 調査レポート2件を`Research/`に出力
- `Research/UserManagement_全層調査レポート.md`
- `Research/UserManagement_リファクタ計画.md`

---

## Stage構成（🆕改訂版・2025-11-30）

```
Stage 1: セキュリティ問題修正 ← ✅完了
Stage 2: Infrastructure層 UserRepository完全実装 ← 🆕追加
Stage 3: Application層 権限フィルタ・プロジェクト割り当て ← 🆕追加
Stage 4: Web層 全画面リファクタ ← 元Stage 2
Stage 5: テスト（単体/統合/E2E） ← 元Stage 3-4統合
```

### 推定時間サマリ（🆕改訂版）

| Stage | 内容 | 推定時間 | セッション |
|-------|------|----------|-----------|
| Stage 1 | セキュリティ問題修正 | ✅完了 | 完了 |
| Stage 2 | Infrastructure層 | 3-4h | 次回 |
| Stage 3 | Application層 | 2-3h | 次々回前半 |
| Stage 4 | Web層 | 4-5h | 次々回後半〜3回目 |
| Stage 5 | テスト | 2-3h | 3回目 |
| **合計** | | **11-15h** | **2-3セッション** |

---

## Stage 1: セキュリティ問題完全修正 ✅完了

**目的**: セキュリティ問題2件の完全修正（Application層 + Infrastructure層）

**SubAgent**:
- fsharp-application Agent（Task 2, Task 1-1, Task 1-3a, Task 1-3c）
- csharp-infrastructure Agent（Task 1-2, Task 1-3b）

**完了日**: 2025-11-29

**完了基準**: ✅全達成
- [x] Task 2: 自己ロール変更時にエラーが返却される
- [x] Task 1: ProjectManager権限フィルタが正しく動作
- [x] 既存テスト全Pass
- [x] ビルド 0 Error

---

## Stage 2: Infrastructure層 UserRepository完全実装 🆕

**目的**: データアクセス層の土台を完成させる

**SubAgent**: `csharp-infrastructure`

**推定時間**: 3-4時間

### Task構成

| Task | 内容 | 詳細 |
|------|------|------|
| 2-1 | ID変換問題解決 | `GetHashCode()`廃止、`GetByIdentityIdAsync`ベースに統一 |
| 2-2 | `GetByEmailAsync`完全実装 | EF Core経由DB検索、Email正規化、Eager Loading |
| 2-3 | `SaveAsync`完全実装 | UserManager.CreateAsync/UpdateAsync、ロール割り当て |
| 2-4 | `DeleteAsync`実装 | 論理削除（IsDeleted = true） |
| 2-5 | `GetByRoleAsync`実装 | ASP.NET Identity Roles経由フィルタ |

### 対象ファイル
- `src/UbiquitousLanguageManager.Infrastructure/Repositories/UserRepository.cs`
- `src/UbiquitousLanguageManager.Infrastructure/Repositories/IUserRepository.cs`（必要に応じて）

### 完了基準
- [ ] 全メソッドがDB操作を正しく実行
- [ ] GetByEmailAsync: 既存メールで正しくユーザー取得
- [ ] SaveAsync: INSERT/UPDATE動作確認
- [ ] DeleteAsync: 論理削除動作確認
- [ ] dotnet build成功（0 Error）

---

## Stage 3: Application層 権限フィルタ・プロジェクト割り当て 🆕

**目的**: ビジネスロジックを完成させる

**SubAgent**: `fsharp-application` + `csharp-infrastructure`

**推定時間**: 2-3時間

### Task構成

| Task | 内容 | 詳細 |
|------|------|------|
| 3-1 | `GetUserByIdAsync`権限フィルタ | ProjectManager: 担当プロジェクトユーザーのみ参照可能 |
| 3-2 | `CreateUserAsync`プロジェクト割り当て | UserProjectsテーブルへのINSERT |
| 3-3 | `UpdateUserAsync`プロジェクト割り当て更新 | 既存削除→新規INSERT |

### 対象ファイル
- `src/UbiquitousLanguageManager.Application/UserManagementServices.fs`
- `src/UbiquitousLanguageManager.Infrastructure/Repositories/UserRepository.cs`（プロジェクト割り当て用メソッド追加時）

### 完了基準
- [ ] SuperUser: 全ユーザー参照可能
- [ ] ProjectManager: 担当プロジェクトユーザーのみ参照可能
- [ ] プロジェクト割り当てがDB永続化
- [ ] dotnet build成功（0 Error）

---

## Stage 4: Web層 全画面リファクタ

**目的**: UI設計書3.6-3.8章完全準拠

**SubAgent**: `csharp-web-ui` × 3（並列実行）

**推定時間**: 4-5時間

### 必須参照ファイル

| ファイル | 目的 |
|---------|------|
| `Doc/02_Design/UI設計/01_認証・ユーザー管理画面設計.md` 3.6-3.8節 | 実装仕様（必須） |
| `Components/Pages/ProjectManagement/ProjectList.razor` | 参考パターン |

### Task構成

#### Task 4-1: Index.razor（UI設計書3.6章準拠）

**実装要件**:
- 権限制御: `[Authorize(Roles = "SuperUser,ProjectManager")]`
- 表示項目: 氏名、メールアドレス、権限レベル、所属プロジェクト
- 検索: 氏名部分一致
- フィルタ: プロジェクト別ドロップダウン
- 削除済み表示: チェックボックス切替
- ページング: 50/100/200件選択
- レイアウト: FullHD対応（Bootstrap 5）
- **data-testid属性: 全要素に付与**

#### Task 4-2: Create.razor（UI設計書3.7章準拠）

**実装要件**:
- 権限制御: `[Authorize(Roles = "SuperUser,ProjectManager")]`
- 入力項目: メールアドレス、氏名、初期パスワード、ロール、所属プロジェクト
- ロール制限: ProjectManagerは一般ユーザー/ドメイン承認者のみ選択可
- プロジェクト制限: ProjectManagerは担当プロジェクトのみ表示
- レイアウト: FullHD対応
- **data-testid属性: 全要素に付与**

#### Task 4-3: Edit.razor（UI設計書3.8章準拠）

**実装要件**:
- 権限制御: `[Authorize(Roles = "SuperUser,ProjectManager")]`
- メールアドレス: 表示のみ（変更不可）
- ステータス変更: アクティブ/非アクティブ
- パスワードリセット: 管理者による新パスワード設定
- ロール・プロジェクト: Create.razorと同様の制限
- レイアウト: FullHD対応
- **data-testid属性: 全要素に付与**

### 並列実行方法（必須遵守）

```
✅ 正しい（並列実行）:
同一メッセージ内で以下を送信:
├─ Task(csharp-web-ui) → "Index.razorを実装して"
├─ Task(csharp-web-ui) → "Create.razorを実装して"
└─ Task(csharp-web-ui) → "Edit.razorを実装して"
```

### 完了基準
- [ ] UI設計書3.6-3.8章全要件満足
- [ ] 全画面data-testid付与
- [ ] FullHDレイアウト確認（1920x1080）
- [ ] dotnet build成功（0 Error）

---

## Stage 5: テスト

**目的**: 全層を通した動作確認

**推定時間**: 2-3時間

### Task構成

| Task | 内容 | SubAgent |
|------|------|----------|
| 5-1 | 単体テスト | `unit-test` |
| 5-2 | 統合テスト | `integration-test` |
| 5-3 | E2Eテスト | `e2e-test` |

### E2Eテスト環境（TypeScript/Playwright Test）

```bash
# 一括実行（推奨）
docker exec ubiquitous-lang-mng_devcontainer-devcontainer-1 bash tests/run-e2e-tests.sh

# 特定テストファイル
docker exec ubiquitous-lang-mng_devcontainer-devcontainer-1 bash tests/run-e2e-tests.sh user-management.spec.ts
```

### 完了基準
- [ ] 単体テスト全件Pass
- [ ] 統合テスト全件Pass
- [ ] E2Eテスト全件Pass
- [ ] ユーザー手動確認完了
- [ ] Step1 Stage3で発見された7件の問題解消確認

---

## 関連ファイル

### 調査レポート（2025-11-30作成）
- `Research/UserManagement_全層調査レポート.md` - 各層の現状分析・問題点
- `Research/UserManagement_リファクタ計画.md` - Stage別詳細計画・Critical Files

### 品質確認レポート（2025-11-27作成）
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
- `.claude/skills/playwright-e2e-patterns/SKILL.md`

---

## 実行記録

### Stage 1 実行記録 ✅完了

**開始日時**: 2025-11-29
**終了日時**: 2025-11-29
**実行SubAgent**:
- fsharp-application Agent × 4（Task 2, Task 1-1, Task 1-3a, Task 1-3c）
- csharp-infrastructure Agent × 2（Task 1-2, Task 1-3b）

**実施Task**:
- ✅ Task 2: 自己ロール変更禁止チェック追加
- ✅ Task 1-1: IUserRepository.GetUsersByProjectIdsAsync追加
- ✅ Task 1-2: UserRepository.GetUsersByProjectIdsAsync実装
- ✅ Task 1-3a: IUserRepository.GetProjectIdsByUserIdAsync追加
- ✅ Task 1-3b: UserRepository.GetProjectIdsByUserIdAsync実装
- ✅ Task 1-3c: GetAllUsersAsync権限フィルタ適用

**結果**: ✅ 完了
- ビルド: 0 Error
- テスト: 全Pass（Domain 113、Application 32、Contracts 98、Infrastructure 98）

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

### Stage 5 実行記録

**開始日時**:
**終了日時**:
**実行SubAgent**:
**結果**:

---

## Step完了チェックリスト

- [x] Stage 1完了: セキュリティ問題2件修正（2025-11-29）
- [ ] Stage 2完了: Infrastructure層UserRepository完全実装
- [ ] Stage 3完了: Application層権限フィルタ・プロジェクト割り当て
- [ ] Stage 4完了: Web層3画面再実装
- [ ] Stage 5完了: テスト（単体/統合/E2E）
- [x] ビルド: 0 Error
- [ ] テスト: 全Pass
- [ ] Step1 Stage3の7件問題: 解消確認
