# Phase B-F3: 前提タスク対処 + Phase B完成 Phase Summary

**作成日**: 2025-11-21
**Phase開始日**: 2025-11-21
**Phase期間**: 推定26-41.5時間 + α（8-10セッション）
**Phase目的**: Phase A完全完成（25% → 100%） + Phase B完全完成（100%） + ユーザー動作確認
**Phase構成**: Part 1（Step1-4: 前提タスク対処）+ Part 2（Step5-10: Phase B機能完成）
**参照資料**: `Doc/99_Others/PhaseB3開始前タスク実施計画.md`, `Doc/99_Others/PhaseA予定実績乖離分析.md`

---

## 📋 Phase概要

### Phase A教訓の適用

**Phase A問題点**:
- UI実装率25%（2/8画面・Login/PasswordChangeのみ動作確認済み）で完了と判断
- ゴール具体化不足・受け入れ基準曖昧
- UI設計書との明示的紐付けなし

**Phase B-F3での改善**:
- ✅ **理想状態のみ受入**（100%完成のみ）
- ✅ **UI設計書との完全紐付け**（8画面明示）
- ✅ **エンドユーザー視点の受け入れ基準**
- ✅ **定量的完成度管理**（画面数・シナリオ数）

---

## 🎯 Phase B-F3全体目標

### 最終ゴール

| 項目 | 現状 | 目標 | 達成基準 |
|------|------|------|---------|
| **Phase A UI実装** | 2/8画面 (25%) | 8/8画面 (100%) | UI設計書全画面動作 |
| **Phase A E2Eテスト** | 6/19シナリオ (31.6%) | 19/19シナリオ (100%) | 全シナリオPass |
| **Phase B UI実装** | 2/3画面 (66.7%) | 3/3画面 (100%) | UI設計書全画面動作 |
| **Phase B エンドユーザー操作** | 7/9操作 (77.8%) | 9/9操作 (100%) | 全操作可能 |
| **技術負債解消** | 7 Issues Open | 7 Issues Close | #44, #59, #57, #46, #53, #19, #52 |

### Phase B-F3完成基準（理想状態のみ受入）

**Part 1完了基準**（Step1-4完了時）:
- ✅ **Phase A完全完成**: UI設計書8画面100%実装・エンドユーザー操作100%可能
- ✅ **E2Eテスト完全化**: 認証9シナリオ + ユーザー管理10シナリオ = 19シナリオ全Pass
- ✅ **Phase B前提整備**: Issue #59, #57解決・Phase B移行準備完了
- ✅ **品質基準維持**: 0 Warning / 0 Error・Clean Architecture 97点維持
- ✅ **7 Issues Close**: #44, #59, #57, #46, #53, #19, #52完全解決

**Part 2完了基準**（Step 10完了時・Phase B-F3完了）:
- ✅ **Phase B完全完成**: UI設計書3画面100%動作・エンドユーザー操作9/9完全達成
- ✅ **サイドメニュー遷移**: プロジェクト一覧画面へのアクセス経路確立
- ✅ **統計情報表示**: プロジェクト統計情報表示機能完成
- ✅ **デフォルトドメイン**: 自動作成動作保証
- ✅ **削除時影響分析**: 関連データ確認機能完成
- ✅ **設計外実装削除**: ProjectMembers.razor削除完了
- ✅ **E2Eテスト完全化**: Phase B E2Eテスト全シナリオPass
- ✅ **品質基準維持**: 0 Warning / 0 Error・Clean Architecture 97点維持
- ✅ **ユーザー動作確認**: エンドユーザー（プロジェクトオーナー）による手動動作確認完了
- ✅ **Phase B3移行判断**: Phase B3移行前提条件を満たしていることをユーザーが確認

**Phase A教訓適用**:
- ❌ **部分完成は受け入れない**
- ✅ **100%完成のみ受入**

---

## 📊 Step構成（10 Steps）

**Part 1: 前提タスク対処**（Step1-4）
- Step 1-2: Phase A完全完成
- Step 3-4: Issue対処・システム改善

**Part 2: Phase B機能完成**（Step5-10）
- Step 5: サイドメニュー→プロジェクト一覧画面遷移
- Step 6: 統計情報表示（所属ドメイン数・ユーザー数）
- Step 7: ProjectMembers.razor削除（設計外実装）
- Step 8: デフォルトドメイン自動作成確認
- Step 9: 削除時影響分析
- Step 10: Part 2のユーザ動作確認・Phase B3移行可否判断

---

### Step 1: Phase A対応漏れ（ユーザー管理UI）

**推定時間**: 8-12時間（1-2セッション）
**優先度**: 🔴 Critical
**関連Issue**: #52
**ゴール寄与率**: +37.5%（3/8画面実装）

#### 実施内容

**重要**: `Pages/Admin/`配下に既存実装が存在するが、UI設計書と大きく乖離（1画面統合 vs 3画面分離）。削除→再作成により仕様準拠実装を実施。

**削除対象（既存実装の仕様乖離により削除）**:
- `src/UbiquitousLanguageManager.Web/Pages/Admin/UserManagement.razor`（758行・1画面統合実装）
- `src/UbiquitousLanguageManager.Web/Pages/Admin/Components/`（8コンポーネント）
  - ValidationSummary.razor
  - TooltipComponent.razor
  - ConfirmationDialog.razor
  - LoadingOverlay.razor
  - KeyboardShortcuts.razor
  - ToastNotification.razor
  - UserEditModal.razor
  - FieldValidator.razor

**仕様乖離理由**:
- UI設計書: 3画面分離（一覧・登録・編集）+ 別画面遷移
- 既存実装: 1画面統合 + モーダル表示
- URL設計: 3つのURL想定 vs 1つのURLのみ
- **判断**: 758行を3画面分割するより、仕様通り再作成の方がコスト安（8-11h vs 9-12h）

1. **ユーザー一覧画面実装**（3-4h）
   - **削除**: `Pages/Admin/UserManagement.razor` + Components/
   - **再作成**: `Components/Pages/Admin/Users/Index.razor`
   - **機能**: 一覧表示・検索・フィルタ・ページング・編集/削除ボタン（権限制御）
   - **参照**: UI設計書3.6節
   - **URL**: `/admin/users`
   - **SubAgent**: csharp-web-ui

2. **ユーザー登録画面実装**（2-3h）
   - **再作成**: `Components/Pages/Admin/Users/Create.razor`
   - **機能**: メールアドレス・氏名・初期パスワード・ロール・所属プロジェクト
   - **参照**: UI設計書3.7節
   - **URL**: `/admin/users/create`
   - **画面遷移**: 一覧画面 → 新規登録ボタン → 登録画面（別画面遷移）
   - **SubAgent**: csharp-web-ui

3. **ユーザー編集画面実装**（3-4h）
   - **再作成**: `Components/Pages/Admin/Users/Edit.razor`
   - **機能**: 氏名・ロール・所属プロジェクト・ステータス・パスワードリセット
   - **参照**: UI設計書3.8節
   - **URL**: `/admin/users/edit/{id}`
   - **画面遷移**: 一覧画面 → 編集ボタン → 編集画面（別画面遷移）
   - **SubAgent**: csharp-web-ui

4. **E2Eテスト作成**（1-2h）
   - **成果物**: `tests/E2E.Tests/user-management.spec.ts`（10シナリオ）
   - **シナリオ**:
     - ユーザー一覧表示（2シナリオ）
     - ユーザー登録機能（3シナリオ）
     - ユーザー編集機能（3シナリオ）
     - ユーザー削除機能（2シナリオ）
   - **SubAgent**: e2e-test

#### テスト要件（テストファースト原則）

**単体テスト**:
- Index/Create/Editコンポーネントテスト（3画面分離）
- バリデーションロジックテスト
- 権限制御ロジックテスト

**統合テスト**:
- User CRUD API統合テスト
- 権限別データフィルタリングテスト
- 3画面間の画面遷移統合テスト

#### 完了基準

- ✅ UI設計書3.6-3.8節100%実装
- ✅ エンドユーザーがユーザー管理操作可能（一覧・登録・編集・削除）
- ✅ E2Eテスト10シナリオ全Pass
- ✅ 単体・統合テスト80%+カバレッジ
- ✅ 0 Warning/0 Error維持
- ✅ Issue #52 Close可能状態

---

### Step 2: Phase A対応漏れ（認証補助機能UI）

**推定時間**: 5-8時間（1セッション）
**優先度**: 🟡 Medium
**関連Issue**: #44（ディレクトリ構造統一）
**ゴール寄与率**: +37.5%（3/8画面実装）

#### 実施内容

**重要**: 以下3画面は`Pages/Auth/`配下に旧実装が存在。削除→再作成により正しいディレクトリ構造に移行。

1. **プロフィール変更画面再作成**（1-2h）
   - **削除**: `src/UbiquitousLanguageManager.Web/Pages/Auth/Profile.razor`（436行）
   - **再作成**: `Components/Pages/Auth/Profile.razor`
   - **機能**: 氏名変更（メールアドレス変更不可）
   - **参照**: UI設計書3.2節
   - **既存実装**: UserManager<ApplicationUser>統合済み・高品質コード（参考可）
   - **SubAgent**: csharp-web-ui

2. **パスワードリセットメール送信画面再作成**（2-3h）
   - **削除**: `src/UbiquitousLanguageManager.Web/Pages/Auth/ForgotPassword.razor`（353行）
   - **再作成**: `Components/Pages/Auth/ForgotPassword.razor`
   - **機能**: メールアドレス入力・リセットメール送信
   - **参照**: UI設計書3.4節
   - **依存**: IPasswordResetService（実装確認済み）
   - **SubAgent**: csharp-web-ui + csharp-infrastructure

3. **パスワードリセット実行画面再作成**（2-3h）
   - **削除**: `src/UbiquitousLanguageManager.Web/Pages/Auth/ResetPassword.razor`（494行）
   - **再作成**: `Components/Pages/Auth/ResetPassword.razor`
   - **機能**: 新しいパスワード設定・リンク有効期限確認
   - **参照**: UI設計書3.5節
   - **依存**: IPasswordResetService（実装確認済み）
   - **SubAgent**: csharp-web-ui + csharp-infrastructure

4. **E2Eテスト拡張**（1-2h）
   - **成果物**: `tests/E2E.Tests/authentication.spec.ts` 拡張（3シナリオ追加）
   - **シナリオ**:
     - Password reset email send
     - Password reset execution
     - Password reset invalid token
   - **現状**: 6/9シナリオ完了（3シナリオSkip状態）
   - **SubAgent**: e2e-test

#### IPasswordResetService確認済み

**インターフェース**: `src/UbiquitousLanguageManager.Contracts/Interfaces/IPasswordResetService.cs`
**実装クラス**: `src/UbiquitousLanguageManager.Infrastructure/Services/PasswordResetService.cs`
**DI登録**: `src/UbiquitousLanguageManager.Web/Program.cs` (line 309-310)

**メソッド**:
- `RequestPasswordResetAsync` - パスワードリセット要求
- `ValidateResetTokenAsync` - トークン検証
- `ResetPasswordAsync` - パスワードリセット実行

#### 完了基準

- ✅ UI設計書3.2, 3.4, 3.5節100%実装
- ✅ パスワードリセットフロー完全動作
- ✅ E2Eテスト9/9シナリオ全Pass（認証機能完全化）
- ✅ 単体・統合テスト80%+カバレッジ
- ✅ 0 Warning/0 Error維持
- ✅ **Phase A完全完成（100%）達成**
- ✅ Issue #44 Close（ディレクトリ構造統一完了）

---

### Step 3: 設計乖離テスト削除（Issue #59対処）

**推定時間**: 2-4時間（1セッション）
**優先度**: 🔴 Critical
**関連Issue**: #59, #57
**ゴール寄与率**: Phase B前提条件整備

#### Issue #59対処（設計乖離テスト削除）

**背景**:
- Issue #59の最後のコメント: "失敗しているテストがある"
- テスト自体が設計から乖離
- 設計上、「プロジェクトメンバー管理画面」「ユーザープロジェクト一覧画面」は存在しない
- ユーザー・プロジェクト紐付けはユーザー管理画面で実施

**対処内容**:
1. **設計乖離テスト削除**（0.5-1h）
   - `tests/E2E.Tests/user-projects.spec.ts` の不要テスト削除
   - 設計準拠シナリオのみ残存
   - Issue #59 Close

#### Issue #57対処（e2e-test Agent運用検証）

**実施内容**:
1. **Step1-3でのe2e-test Agent活用**（並行実施）
2. **運用効果測定**（5分-1時間）
   - 実装時間削減率
   - テスト品質向上
   - Agent活用パターン確立
   - Issue #57 Close

#### 完了基準

- ✅ Issue #59 Close（設計乖離テスト削除完了）
- ✅ Issue #57 Close（e2e-test Agent運用効果レポート作成）
- ✅ Phase B前提条件整備完了

---

### Step 4: システム改善・Issue対処（並列実施）

**推定時間**: 6-8.5時間 → **最適化後: 4-5.5時間**（並列実施）
**優先度**: 🟡 Medium
**関連Issue**: #46, #19, #53

#### Issue #46: Commands更新延期判断（1-1.5h）

**実施内容**:
1. Commands刷新必要性の再評価
2. 延期判断基準の策定
3. ADR作成（必要に応じて）

**SubAgent**: design-review + tech-research

#### Issue #19: テスト戦略改善・再発防止（3-4h）

**背景**:
- Phase A8 Step4で認証テスト35件失敗
- テスト戦略ガイドの適切なタイミング適用不足

**実施内容**:
1. **step-start.md改善**
   - 品質保証準備セクションにテスト戦略ガイド必読化追加
   - TDD実践計画テンプレート組み込み
   - 仕様ベーステスト設計チェックリスト追加

2. **CLAUDE.md改善**
   - Step開始時必読リストにテスト戦略ガイド追加
   - テストファースト原則の明示的記載強化

3. **tdd-practice-check統合**
   - step-start内での明示的呼び出し実装
   - spec-compliance-checkとの連携強化

4. **spec-complianceエージェント強化**（必要に応じて）
   - TDD原則の組み込み
   - 仕様→テストケース変換支援機能追加

5. **unit-testエージェント連携**（必要に応じて）
   - Step開始時のテスト戦略確認必須化
   - 仕様ベーステスト設計との統合

**SubAgent**: design-review + spec-compliance

#### Issue #53: ADR_022作成（テスト失敗時判断プロセス）（2-3h）

**依存関係**: Issue #19完了後に実施

**実施内容**:
1. Issue #19の知見を基にADR_022作成
2. テスト失敗時の判断プロセス明文化
3. 対処優先度マトリックス策定

**SubAgent**: なし（MainAgentがドキュメント作成）

#### 並列実施計画

**タイムライン**:
- **0:00-1:30**: Issue #46（MainAgent）並行
- **0:00-4:00**: Issue #19（SubAgent 1: design-review）並行
- **1:30-4:30**: Issue #53（MainAgent、#19完了後）

**最適化効果**: 6-8.5h → **4-5.5h**（並列効率30-40%）

#### 完了基準

- ✅ Issue #46 Close（Commands更新判断レポート作成）
- ✅ Issue #19 Close（テスト戦略改善ドキュメント作成・システム改善実施）
- ✅ Issue #53 Close（ADR_022作成完了）
- ✅ Phase B1以降の大規模テスト失敗再発防止体制確立

---

### Step 5: サイドメニュー→プロジェクト一覧画面遷移

**推定時間**: 1-2時間（1セッション）
**優先度**: 🔴 Critical
**前提条件**: Step1-4完了（Phase A完全完成 + Issue対処完了）
**ゴール寄与率**: Phase B画面到達性確立

#### 実施内容

**背景**:
- Phase B2完了時点で、プロジェクト一覧画面は実装済み
- しかし、サイドメニューからの遷移リンクが未実装
- **現状**: エンドユーザーがプロジェクト画面に到達できない状態

1. **サイドメニュー更新**（0.5-1h）
   - **実装**: `Shared/NavMenu.razor` にプロジェクト一覧リンク追加
   - **URL**: `/projects`
   - **権限制御**: 認証済みユーザー全員
   - **SubAgent**: csharp-web-ui

2. **画面遷移E2Eテスト作成**（0.5-1h）
   - **成果物**: `tests/E2E.Tests/projects.spec.ts` 拡張
   - **シナリオ**: 「サイドメニューからプロジェクト一覧画面へ遷移」
   - **SubAgent**: e2e-test

#### 成果物

- ✅ `Shared/NavMenu.razor` 更新（プロジェクト一覧リンク追加）
- ✅ プロジェクト一覧画面へのアクセス経路確立
- ✅ E2Eテスト: サイドメニュー遷移シナリオ

#### 完了基準

- ✅ サイドメニューから「プロジェクト」をクリック → プロジェクト一覧画面表示
- ✅ E2Eテスト Pass
- ✅ 0 Warning/0 Error維持
- ✅ Clean Architecture 97点維持

---

### Step 6: 統計情報表示（所属ドメイン数・ユーザー数）

**推定時間**: 2-3時間（1セッション）
**優先度**: 🔴 Critical
**前提条件**: Step 5完了（プロジェクト一覧画面到達可能）
**ゴール寄与率**: Phase B機能完成度+30%

#### 実施内容

**背景**:
- UI設計書3.1節（プロジェクト一覧画面）で統計情報表示が定義済み
- 表示項目: 所属ドメイン数・所属ユーザー数
- Phase B2実装では未対応

1. **統計情報取得クエリ実装**（1-1.5h）
   - **実装**: `Infrastructure/Repositories/ProjectRepository.cs` 拡張
   - **機能**: プロジェクト別ドメイン数・ユーザー数集計
   - **SubAgent**: csharp-infrastructure

2. **UI表示実装**（1-1.5h）
   - **実装**: `Components/Pages/Projects/Index.razor` 更新
   - **表示位置**: プロジェクトカード内（各プロジェクト行）
   - **表示例**: 「所属ドメイン: 3 / 所属ユーザー: 5」
   - **参照**: UI設計書3.1節
   - **SubAgent**: csharp-web-ui

3. **E2Eテスト作成**（0.5h）
   - **成果物**: `tests/E2E.Tests/projects.spec.ts` 拡張
   - **シナリオ**: 「プロジェクト一覧で統計情報表示確認」
   - **SubAgent**: e2e-test

#### 成果物

- ✅ `Infrastructure/Repositories/ProjectRepository.cs` 拡張（統計情報取得クエリ）
- ✅ `Components/Pages/Projects/Index.razor` 更新（統計情報表示）
- ✅ E2Eテスト: 統計情報表示確認シナリオ

#### 完了基準

- ✅ プロジェクト一覧画面で各プロジェクトの統計情報が表示される
- ✅ 統計情報（ドメイン数・ユーザー数）が正しく集計される
- ✅ E2Eテスト Pass
- ✅ 0 Warning/0 Error維持
- ✅ Clean Architecture 97点維持

---

### Step 7: ProjectMembers.razor削除（設計外実装）

**推定時間**: 1時間（1セッション）
**優先度**: 🟡 Medium
**前提条件**: Step 6完了
**ゴール寄与率**: Phase B設計準拠性確保

#### 実施内容

**背景**:
- Phase B2 Step5でProjectMembers.razorが実装された
- しかし、UI設計書には「プロジェクトメンバー管理画面」が存在しない
- **ユーザー・プロジェクト紐付けはユーザー管理画面で実施**する設計
- Phase B2 Step8 E2Eテスト失敗の原因の1つ

1. **設計外実装削除**（0.5-1h）
   - **削除対象**:
     - `Components/Pages/Projects/ProjectMembers.razor`
     - `Components/Pages/Projects/ProjectMemberSelector.razor`
     - `Components/Pages/Projects/ProjectMemberCard.razor`
   - **SubAgent**: なし（MainAgentが直接実行）

2. **E2Eテスト修正**（0h）
   - **確認**: `tests/E2E.Tests/user-projects.spec.ts` の関連シナリオ削除は Step 3で実施済み
   - 本Stepでは追加作業なし

#### 成果物

- ✅ ProjectMembers.razor等3ファイル削除完了
- ✅ Phase B設計準拠性確保

#### 完了基準

- ✅ ProjectMembers.razor等3ファイルが存在しない
- ✅ ビルド成功・0 Warning/0 Error維持
- ✅ Clean Architecture 97点維持

---

### Step 8: デフォルトドメイン自動作成確認

**推定時間**: 1-2時間（1セッション）
**優先度**: 🔴 Critical
**前提条件**: Step 7完了
**ゴール寄与率**: Phase B機能完成度+20%

#### 実施内容

**背景**:
- プロジェクト作成時にデフォルトドメインが自動作成される設計
- Application層（ProjectDomainService.fs）で実装済みの可能性あり
- E2Eテストで動作保証が必要

1. **デフォルトドメイン自動作成ロジック確認**（0.5-1h）
   - **確認**: `src/UbiquitousLanguageManager.Application/ProjectDomainService.fs`
   - **機能**: CreateProject関数でのデフォルトドメイン作成処理確認
   - **未実装の場合**: Application層に実装追加
   - **SubAgent**: fsharp-application（未実装の場合）

2. **E2Eテスト作成**（0.5-1h）
   - **成果物**: `tests/E2E.Tests/projects.spec.ts` 拡張
   - **シナリオ**: 「プロジェクト作成時デフォルトドメイン自動作成確認」
   - **検証内容**: プロジェクト作成 → ドメイン一覧画面 → デフォルトドメイン存在確認
   - **SubAgent**: e2e-test

#### 成果物

- ✅ ProjectDomainService.fs デフォルトドメイン自動作成実装確認（または実装追加）
- ✅ E2Eテスト: デフォルトドメイン自動作成確認シナリオ

#### 完了基準

- ✅ プロジェクト作成時に「デフォルト」ドメインが自動作成される
- ✅ E2Eテストで動作保証
- ✅ 0 Warning/0 Error維持
- ✅ Clean Architecture 97点維持

---

### Step 9: 削除時影響分析

**推定時間**: 2-4時間（1セッション）
**優先度**: 🔴 Critical
**前提条件**: Step 8完了
**ゴール寄与率**: Phase B機能完成度+40%

#### 実施内容

**背景**:
- UI設計書3.1節（プロジェクト削除機能）で削除時影響分析が定義済み
- 削除前の確認ダイアログで関連データ数を表示
- ユーザーが削除実行/キャンセルを選択できる方式

**仕様（UI設計書 line 510-512）**:
- **削除制約**: 関連データ（ドメイン・用語）がある場合の警告表示
- **警告表示**: 削除前の確認ダイアログで関連データ数を表示
- **メッセージ例**: 「このプロジェクトには3つのドメインと15の用語が含まれています。削除してもよろしいですか？」

1. **関連データカウント取得実装**（1-1.5h）
   - **実装**: `Infrastructure/Repositories/ProjectRepository.cs` 拡張
   - **機能**: プロジェクトIDから関連ドメイン数・用語数を集計
   - **SubAgent**: csharp-infrastructure

2. **削除確認ダイアログ拡張**（1-1.5h）
   - **実装**: `Components/Pages/Projects/Index.razor` 更新
   - **機能**: 削除ボタンクリック → 関連データ取得 → 確認ダイアログ表示
   - **UI**: Bootstrapモーダル or Blazor Confirm
   - **参照**: UI設計書3.1節
   - **SubAgent**: csharp-web-ui

3. **E2Eテスト作成**（0.5-1h）
   - **成果物**: `tests/E2E.Tests/projects.spec.ts` 拡張
   - **シナリオ**:
     - 「プロジェクト削除時影響分析確認（関連データあり）」
     - 「プロジェクト削除時影響分析確認（関連データなし）」
   - **SubAgent**: e2e-test

#### 成果物

- ✅ `Infrastructure/Repositories/ProjectRepository.cs` 拡張（関連データカウント取得）
- ✅ `Components/Pages/Projects/Index.razor` 更新（削除確認ダイアログ拡張）
- ✅ E2Eテスト: 削除時影響分析確認シナリオ

#### 完了基準

- ✅ プロジェクト削除時に関連データ数が表示される
- ✅ ユーザーが削除実行/キャンセルを選択できる
- ✅ E2Eテスト Pass
- ✅ 0 Warning/0 Error維持
- ✅ Clean Architecture 97点維持
- ✅ **Phase B機能実装100%完成**

---

### Step 10: Part 2のユーザ動作確認・Phase B3移行可否判断

**推定時間**: 時間不明（ユーザー手動確認）
**優先度**: 🔴 Critical
**前提条件**: Step 5-9完了（Phase B機能実装100%完成）
**ゴール寄与率**: Phase B-F3完了・Phase B3移行判断

#### 実施内容

**目的**:
- エンドユーザー（プロジェクトオーナー）による手動動作確認
- Phase B3移行前提条件を満たしているか最終判断
- **Phase完了時イメージの検証**（phase-start.md教訓適用）

1. **ユーザー手動動作確認項目**
   - ✅ サイドメニューから「プロジェクト」をクリック → プロジェクト一覧画面に到達できる
   - ✅ プロジェクト一覧画面で統計情報（所属ドメイン数・所属ユーザー数）が正しく表示される
   - ✅ プロジェクト作成時にデフォルトドメインが自動作成される
   - ✅ プロジェクト削除時に関連データ数（ドメイン数・用語数）が表示され、削除実行/キャンセルを選択できる

2. **E2Eテスト全Pass確認**
   - ✅ Phase A E2Eテスト19シナリオ全Pass
   - ✅ Phase B E2Eテスト全シナリオPass
   - ✅ `bash tests/run-e2e-tests.sh` 実行結果確認

3. **品質基準確認**
   - ✅ `dotnet build` → 0 Warning/0 Error
   - ✅ Clean Architecture 97点維持（clean-architecture-guardian確認）

#### Phase B3移行判断基準

| 項目 | 判断基準 | 確認方法 |
|------|---------|---------|
| **プロジェクト画面到達** | サイドメニューからプロジェクト一覧画面に到達できる | 手動確認 |
| **統計情報正常表示** | 所属ドメイン数・ユーザー数が正しく表示される | 手動確認 |
| **デフォルトドメイン自動作成** | プロジェクト作成時に「デフォルト」ドメインが作成される | 手動確認 + E2Eテスト |
| **削除影響分析正常動作** | プロジェクト削除時に関連データ数が表示される | 手動確認 + E2Eテスト |
| **E2Eテスト全Pass** | Phase A + Phase B E2Eテスト全Pass | bash tests/run-e2e-tests.sh |
| **品質基準** | 0 Warning/0 Error・Clean Architecture 97点 | dotnet build + guardian |

#### 判断結果

**✅ Phase B3移行OK条件**:
- 上記6項目すべて合格
- ユーザーが「Phase B完全完成」と判断

**❌ Phase B3移行NG条件**:
- いずれか1項目でも不合格
- ユーザーが「まだ不足している」と判断
- → 追加Step（Step 11以降）で不足箇所を実装

#### 完了基準

- ✅ ユーザー手動動作確認完了
- ✅ Phase B3移行判断完了
- ✅ **Phase B-F3完了** または **追加Step着手判断**

---

## 📊 Phase B-F3全体スケジュール

### タイムライン

| Step | タスク内容 | 推定時間 | セッション | 累積時間 |
|------|-----------|---------|-----------|---------|
| **Step 1** | Phase A対応漏れ（ユーザー管理UI） | 8-12h | 1-2 | 8-12h |
| **Step 2** | Phase A対応漏れ（認証補助機能UI） | 5-8h | 1 | 13-20h |
| **Step 3** | Phase B UI拡張 + Agent検証 | 2-4h | 1 | 15-24h |
| **Step 4** | システム改善・Issue対処（並列） | 4-5.5h | 1 | 19-29.5h |
| **合計（Part 1）** | 前提タスク対処 | **19-29.5h** | **3-5** | - |
| **Step 5** | サイドメニュー→プロジェクト一覧画面遷移 | 1-2h | 1 | 20-31.5h |
| **Step 6** | 統計情報表示（所属ドメイン数・ユーザー数） | 2-3h | 1 | 22-34.5h |
| **Step 7** | ProjectMembers.razor削除（設計外実装） | 1h | 1 | 23-35.5h |
| **Step 8** | デフォルトドメイン自動作成確認 | 1-2h | 1 | 24-37.5h |
| **Step 9** | 削除時影響分析 | 2-4h | 1 | 26-41.5h |
| **Step 10** | ユーザ動作確認・Phase B3移行可否判断 | 時間不明 | 1 | 26-41.5h + α |
| **合計（Part 2）** | Phase B機能完成 | **7-12h + α** | **5** | - |
| **総合計（Phase B-F3全体）** | Part 1 + Part 2 | **26-41.5h + α** | **8-10** | - |

### マイルストーン

| マイルストーン | 達成基準 | 完了予定 |
|---------------|---------|---------|
| **MS1**: Phase A 100%完成 | UI設計書8画面100%実装・エンドユーザー操作100% | Step 1-2完了時（Part 1） |
| **MS2**: Phase B Issue対処完了 | Issue #59, #57, #46, #53, #19完全解決 | Step 3-4完了時（Part 1） |
| **MS3**: Phase B 100%完成 | UI設計書3画面100%動作・エンドユーザー操作9/9完全達成・ユーザー動作確認完了 | Step 10完了時（Part 2・Phase B-F3完了） |

---

## 🎯 Phase B-F3完成基準（詳細）

### Part 1完了基準（Step1-4完了時）

#### Phase A完了基準（MS1達成時）

| 項目 | 達成基準 | 検証方法 |
|------|---------|---------|
| **UI実装** | UI設計書8画面100%実装 | 全画面動作確認 |
| **エンドユーザー操作** | 100%操作可能（25% → 100%） | 受け入れテスト実施 |
| **E2Eテスト** | 19シナリオ全Pass（6/19 → 19/19） | dotnet test実行 |
| **品質** | 0 Warning/0 Error | dotnet build確認 |
| **アーキテクチャ** | Clean Architecture 97点維持 | clean-architecture-guardian確認 |

#### Phase B前提条件整備（MS2達成時）

| 項目 | 達成基準 | 検証方法 |
|------|---------|---------|
| **Issue解決** | 7 Issues Close (#44, #59, #57, #46, #53, #19, #52) | GitHub Issue確認 |
| **システム改善** | テスト戦略改善実施完了 | step-start.md等更新確認 |
| **Agent検証** | e2e-test Agent運用効果測定完了 | レポート作成確認 |

---

### Part 2完了基準（Step5完了時・Phase B-F3完了）

#### Phase B完全完成（MS3達成時）

| 項目 | 達成基準 | 検証方法 |
|------|---------|---------|
| **UI実装** | UI設計書3画面100%動作 | 全画面動作確認 |
| **エンドユーザー操作** | 9/9操作完全達成（77.8% → 100%） | 受け入れテスト実施 |
| **統計情報** | プロジェクト統計情報正常表示 | 手動確認 |
| **デフォルトドメイン** | 自動作成動作保証 | E2Eテスト確認 |
| **削除時影響分析** | 関連データ確認正常動作 | 手動確認 + E2Eテスト |
| **E2Eテスト** | Phase B全シナリオPass | bash tests/run-e2e-tests.sh |
| **品質** | 0 Warning/0 Error | dotnet build確認 |
| **アーキテクチャ** | Clean Architecture 97点維持 | clean-architecture-guardian確認 |

---

## 📚 参照ドキュメント

### 計画資料
- `Doc/99_Others/PhaseB3開始前タスク実施計画.md`（詳細実施計画）
- `Doc/99_Others/PhaseA予定実績乖離分析.md`（Phase A問題分析）
- `Doc/99_Others/PhaseB全体ゴール具体化.md`（Phase B全体像）
- `Doc/08_Organization/Rules/縦方向スライス実装マスタープラン.md`（マスタープラン）

### 設計書
- `Doc/02_Design/UI設計/01_認証・ユーザー管理画面設計.md`（UI設計書）
- `Doc/02_Design/権限制御テストマトリックス.md`（権限制御仕様）

### テスト戦略
- `Doc/08_Organization/Rules/テスト戦略ガイド.md`（テストファースト原則）

### GitHub Issues
- Issue #52: Phase A（認証・ユーザー管理）機能のE2Eテスト実装
- Issue #59: Phase A E2Eテストシナリオ再設計
- Issue #57: Playwright実装責任Agent明確化
- Issue #46: Commands/SubAgent刷新（Phase B2後延期判断）
- Issue #53: テスト失敗時判断プロセス（ADR_022）
- Issue #19: テスト戦略改善・再発防止体制確立
- Issue #44: Web層ディレクトリ構造統一

---

## 🎓 Phase A教訓の適用

### 問題点と改善策

| Phase A問題点 | Phase B-F3改善策 |
|-------------|----------------|
| ❌ UI実装率25%で完了 | ✅ 100%完成のみ受入 |
| ❌ ゴール具体化不足 | ✅ UI設計書8画面明示 |
| ❌ 受け入れ基準曖昧 | ✅ エンドユーザー視点100%操作可能 |
| ❌ Phase完了判定甘い | ✅ 理想状態のみ受入 |
| ❌ 縦スライス実装違反 | ✅ 機能単位全層貫通実装 |

---

## 📊 Step間成果物参照マトリックス

### 各Step必須参照ドキュメント一覧

| Step | 作業内容 | 必須参照ドキュメント | 重点参照セクション | 活用目的 |
|------|---------|-------------------|-------------------|---------|
| **Step 1** | ユーザー一覧画面実装 | `Doc/02_Design/UI設計/01_認証・ユーザー管理画面設計.md` | 3.6節（ユーザー一覧画面） | UI実装仕様確認 |
| **Step 1** | ユーザー登録画面実装 | `Doc/02_Design/UI設計/01_認証・ユーザー管理画面設計.md` | 3.7節（ユーザー登録画面） | UI実装仕様確認 |
| **Step 1** | ユーザー編集画面実装 | `Doc/02_Design/UI設計/01_認証・ユーザー管理画面設計.md` | 3.8節（ユーザー編集画面） | UI実装仕様確認 |
| **Step 1** | 権限制御実装 | `Doc/02_Design/権限制御テストマトリックス.md` | 全体（4ロール×4機能） | 権限判定ロジック・テストケース設計 |
| **Step 1** | テスト設計・実装 | `Doc/08_Organization/Rules/テスト戦略ガイド.md` | TDD実践セクション | Red-Green-Refactorサイクル適用 |
| **Step 1** | E2Eテスト作成 | GitHub Issue #52 | E2Eテストシナリオ | ユーザー管理10シナリオ実装指針 |
| **Step 1** | 既存実装削除 | `src/UbiquitousLanguageManager.Web/Pages/Admin/UserManagement.razor` | 全体（758行） | 仕様乖離確認・削除対象特定 |
| **Step 2** | プロフィール変更画面 | `Doc/02_Design/UI設計/01_認証・ユーザー管理画面設計.md` | 3.2節（プロフィール変更画面） | UI実装仕様確認 |
| **Step 2** | パスワードリセットメール送信 | `Doc/02_Design/UI設計/01_認証・ユーザー管理画面設計.md` | 3.4節（パスワードリセットメール送信） | UI実装仕様確認 |
| **Step 2** | パスワードリセット実行 | `Doc/02_Design/UI設計/01_認証・ユーザー管理画面設計.md` | 3.5節（パスワードリセット実行） | UI実装仕様確認 |
| **Step 2** | IPasswordResetService統合 | `src/UbiquitousLanguageManager.Contracts/Interfaces/IPasswordResetService.cs` | 全メソッド | インターフェース仕様確認 |
| **Step 2** | IPasswordResetService実装確認 | `src/UbiquitousLanguageManager.Infrastructure/Services/PasswordResetService.cs` | 全実装 | 実装詳細・依存関係確認 |
| **Step 2** | 旧実装削除対象確認 | `src/UbiquitousLanguageManager.Web/Pages/Auth/Profile.razor` | 全体（436行） | 削除前の実装内容確認（参考可） |
| **Step 2** | 旧実装削除対象確認 | `src/UbiquitousLanguageManager.Web/Pages/Auth/ForgotPassword.razor` | 全体（353行） | 削除前の実装内容確認（参考可） |
| **Step 2** | 旧実装削除対象確認 | `src/UbiquitousLanguageManager.Web/Pages/Auth/ResetPassword.razor` | 全体（494行） | 削除前の実装内容確認（参考可） |
| **Step 2** | ディレクトリ構造統一 | GitHub Issue #44 | 全体 | Pages/ → Components/Pages/ 移行方針 |
| **Step 2** | テスト設計・実装 | `Doc/08_Organization/Rules/テスト戦略ガイド.md` | TDD実践セクション | Red-Green-Refactorサイクル適用 |
| **Step 2** | E2Eテスト拡張 | `tests/UbiquitousLanguageManager.E2E.Tests/authentication.spec.ts` | 既存6シナリオ | 3シナリオ追加（9/9完成） |
| **Step 3** | 設計乖離テスト削除 | GitHub Issue #59 | 問題背景・対処方針 | 設計乖離理由・削除対象確認 |
| **Step 3** | 削除対象テスト確認 | `tests/UbiquitousLanguageManager.E2E.Tests/user-projects.spec.ts` | 不要シナリオ | 設計準拠シナリオとの区分 |
| **Step 3** | Agent運用検証 | GitHub Issue #57 | e2e-test Agent責任範囲 | Agent効果測定方法確認 |
| **Step 3** | Agent運用効果レポート作成 | `.claude/agents/e2e-test.md` | Agent定義・ツール | Agent活用パターン確立 |
| **Step 4** | Commands更新延期判断 | GitHub Issue #46 | 問題背景・判断基準 | 延期妥当性評価 |
| **Step 4** | テスト戦略改善 | GitHub Issue #19 | Phase A8 Step4失敗分析 | 再発防止策策定 |
| **Step 4** | step-start.md改善 | `.claude/commands/step-start.md` | 品質保証準備セクション | テスト戦略ガイド必読化追加 |
| **Step 4** | CLAUDE.md改善 | `CLAUDE.md` | テスト戦略セクション | テストファースト原則強化 |
| **Step 4** | テスト戦略ガイド参照 | `Doc/08_Organization/Rules/テスト戦略ガイド.md` | 全体 | 改善内容の元ネタ確認 |
| **Step 4** | ADR_022作成 | GitHub Issue #53 | テスト失敗時判断プロセス | ADR作成要件確認 |
| **Step 4** | ADR_022作成 | Issue #19成果物 | テスト戦略改善知見 | ADR_022の根拠情報 |
| **Step 5** | サイドメニュー更新 | `src/UbiquitousLanguageManager.Web/Shared/NavMenu.razor` | 全体 | プロジェクト一覧リンク追加箇所特定 |
| **Step 5** | UI設計書参照 | `Doc/02_Design/UI設計/02_プロジェクト・ドメイン管理画面設計.md` | 3.1節（プロジェクト一覧画面） | 画面仕様・遷移フロー確認 |
| **Step 5** | E2Eテスト作成 | `tests/E2E.Tests/projects.spec.ts` | 既存シナリオ | サイドメニュー遷移シナリオ追加 |
| **Step 6** | 統計情報UI仕様確認 | `Doc/02_Design/UI設計/02_プロジェクト・ドメイン管理画面設計.md` | 3.1節（プロジェクト一覧画面） | 統計情報表示UI仕様確認 |
| **Step 6** | ProjectList.razor確認 | `src/UbiquitousLanguageManager.Web/Components/Pages/Projects/Index.razor` | 全体 | 既存実装確認・統計情報追加箇所特定 |
| **Step 6** | 統計情報取得実装 | `src/UbiquitousLanguageManager.Infrastructure/Repositories/ProjectRepository.cs` | 既存メソッド | 統計情報取得クエリ実装参考 |
| **Step 6** | E2Eテスト作成 | `tests/E2E.Tests/projects.spec.ts` | 既存シナリオ | 統計情報表示確認シナリオ追加 |
| **Step 6** | テスト設計・実装 | `Doc/08_Organization/Rules/テスト戦略ガイド.md` | TDD実践セクション | Red-Green-Refactorサイクル適用 |
| **Step 7** | 削除対象ファイル確認 | `src/UbiquitousLanguageManager.Web/Components/Pages/Projects/ProjectMembers.razor` | 全体 | 設計外実装確認・削除対象特定 |
| **Step 7** | 削除対象ファイル確認 | `src/UbiquitousLanguageManager.Web/Components/Pages/Projects/ProjectMemberSelector.razor` | 全体 | 設計外実装確認・削除対象特定 |
| **Step 7** | 削除対象ファイル確認 | `src/UbiquitousLanguageManager.Web/Components/Pages/Projects/ProjectMemberCard.razor` | 全体 | 設計外実装確認・削除対象特定 |
| **Step 7** | Phase B2実装確認 | `Doc/08_Organization/Completed/Phase_B2/Phase_Summary.md` | Step5実装内容 | ProjectMembers.razor実装経緯確認 |
| **Step 8** | デフォルトドメインロジック確認 | `src/UbiquitousLanguageManager.Application/ProjectDomainService.fs` | CreateProject関数 | デフォルトドメイン自動作成ロジック確認 |
| **Step 8** | UI設計書参照 | `Doc/02_Design/UI設計/02_プロジェクト・ドメイン管理画面設計.md` | プロジェクト作成機能 | デフォルトドメイン自動作成仕様確認 |
| **Step 8** | E2Eテスト作成 | `tests/E2E.Tests/projects.spec.ts` | 既存シナリオ | デフォルトドメイン自動作成確認シナリオ追加 |
| **Step 8** | テスト設計・実装 | `Doc/08_Organization/Rules/テスト戦略ガイド.md` | TDD実践セクション | Red-Green-Refactorサイクル適用 |
| **Step 9** | 削除機能UI仕様確認 | `Doc/02_Design/UI設計/02_プロジェクト・ドメイン管理画面設計.md` | 3.1節（削除機能） | 削除確認ダイアログ仕様確認 |
| **Step 9** | ProjectList.razor確認 | `src/UbiquitousLanguageManager.Web/Components/Pages/Projects/Index.razor` | 削除ボタン実装 | 既存削除機能確認・拡張箇所特定 |
| **Step 9** | 関連データカウント実装 | `src/UbiquitousLanguageManager.Infrastructure/Repositories/ProjectRepository.cs` | 既存メソッド | 関連データカウント取得クエリ実装参考 |
| **Step 9** | E2Eテスト作成 | `tests/E2E.Tests/projects.spec.ts` | 既存シナリオ | 削除時影響分析確認シナリオ追加 |
| **Step 9** | テスト設計・実装 | `Doc/08_Organization/Rules/テスト戦略ガイド.md` | TDD実践セクション | Red-Green-Refactorサイクル適用 |
| **Step 10** | Phase B3移行判断基準 | `.claude/commands/phase-start.md` | Section 1.5 | Phase完了時イメージの検証方法 |
| **Step 10** | E2Eテスト全実行 | `tests/run-e2e-tests.sh` | 全シナリオ | Phase A + Phase B E2Eテスト全Pass確認 |
| **Step 10** | 品質基準確認 | `CLAUDE.md` | 開発コマンドセクション | dotnet build / clean-architecture-guardian実行方法 |
| **Step 10** | ユーザー動作確認手順 | Phase_Summary.md（本ドキュメント） | Step 10実施内容 | ユーザー手動動作確認項目一覧 |
| **全Step** | 計画資料参照 | `Doc/99_Others/PhaseB3開始前タスク実施計画.md` | 全体計画・リスク管理 | Phase全体像・工数確認 |
| **全Step** | Phase A問題分析 | `Doc/99_Others/PhaseA予定実績乖離分析.md` | 対応漏れ詳細・教訓 | Phase A教訓適用 |

### 主要ドキュメント所在

#### 設計書
- `Doc/02_Design/UI設計/01_認証・ユーザー管理画面設計.md` - Phase A UI設計（8画面仕様）
- `Doc/02_Design/UI設計/02_プロジェクト管理画面設計.md` - Phase B UI設計（3画面仕様）
- `Doc/02_Design/権限制御テストマトリックス.md` - 権限制御仕様（4ロール×4機能=16パターン）

#### 計画資料
- `Doc/99_Others/PhaseB3開始前タスク実施計画.md` - Phase B-F3詳細実施計画（昨日作成）
- `Doc/99_Others/PhaseA予定実績乖離分析.md` - Phase A問題分析（対応漏れ原因・教訓）

#### テスト戦略
- `Doc/08_Organization/Rules/テスト戦略ガイド.md` - TDD実践ガイド（Red-Green-Refactorサイクル）

#### GitHub Issues（7件）
- Issue #52: Phase A E2Eテスト実装（6/19シナリオ → 19/19シナリオ）
- Issue #59: E2Eテストシナリオ再設計（設計乖離テスト削除）
- Issue #57: e2e-test Agent運用検証
- Issue #46: Commands更新延期判断
- Issue #53: ADR_022作成（テスト失敗時判断プロセス）
- Issue #19: テスト戦略改善・再発防止体制確立
- Issue #44: Web層ディレクトリ構造統一（Pages/ → Components/Pages/）

#### 実装コード参照
- `src/UbiquitousLanguageManager.Contracts/Interfaces/IPasswordResetService.cs` - パスワードリセットサービスIF
- `src/UbiquitousLanguageManager.Infrastructure/Services/PasswordResetService.cs` - パスワードリセット実装
- `src/UbiquitousLanguageManager.Web/Pages/Auth/*.razor` - 旧実装（削除対象・参考可）
- `src/UbiquitousLanguageManager.Web/Pages/Admin/*.razor` - 既存実装（移行対象）

#### システムファイル改善対象
- `.claude/commands/step-start.md` - Step開始プロセス（Issue #19改善対象）
- `CLAUDE.md` - プロジェクト指示書（Issue #19改善対象）

---

**作成日**: 2025-11-21
**作成者**: Claude Code
**ステータス**: 正式版
