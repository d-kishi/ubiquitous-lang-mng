# Phase B-F3 Part 1: 前提タスク対処 Phase Summary（ドラフト版）

**作成日**: 2025-11-21
**Phase期間**: 未定（phase-start実行後に確定）
**推定工数**: 19-28時間（3-4セッション）
**Phase目的**: Phase A完全完成（37.5% → 100%） + Phase B前提条件整備
**参照資料**: `Doc/99_Others/PhaseB3開始前タスク実施計画.md`, `Doc/99_Others/PhaseA予定実績乖離分析.md`

---

## 📋 Phase概要

### Phase A教訓の適用

**Phase A問題点**:
- UI実装率37.5%（3/8画面）で完了と判断
- ゴール具体化不足・受け入れ基準曖昧
- UI設計書との明示的紐付けなし

**Phase B-F3での改善**:
- ✅ **理想状態のみ受入**（100%完成のみ）
- ✅ **UI設計書との完全紐付け**（8画面明示）
- ✅ **エンドユーザー視点の受け入れ基準**
- ✅ **定量的完成度管理**（画面数・シナリオ数）

---

## 🎯 Phase B-F3 Part 1 目標

### 最終ゴール

| 項目 | 現状 | 目標 | 達成基準 |
|------|------|------|---------|
| **Phase A UI実装** | 3/8画面 (37.5%) | 8/8画面 (100%) | UI設計書全画面動作 |
| **Phase A E2Eテスト** | 6/19シナリオ (31.6%) | 19/19シナリオ (100%) | 全シナリオPass |
| **Phase B前提条件** | 未整備 | 整備完了 | Issue #59, #57解決 |
| **技術負債解消** | 7 Issues Open | 7 Issues Close | #44, #59, #57, #46, #53, #19, #52 |

### Phase B-F3 Part 1 完成基準（理想状態のみ受入）

**必須達成基準**:
- ✅ **Phase A完全完成**: UI設計書8画面100%実装・エンドユーザー操作100%可能
- ✅ **E2Eテスト完全化**: 認証9シナリオ + ユーザー管理10シナリオ = 19シナリオ全Pass
- ✅ **Phase B前提整備**: Issue #59, #57解決・Phase B移行準備完了
- ✅ **品質基準維持**: 0 Warning / 0 Error・Clean Architecture 97点維持
- ✅ **7 Issues Close**: #44, #59, #57, #46, #53, #19, #52完全解決

**Phase A教訓適用**:
- ❌ **部分完成は受け入れない**
- ✅ **100%完成のみ受入**

---

## 📊 Step構成（4 Steps）

### Step 1: Phase A対応漏れ（ユーザー管理UI）

**推定時間**: 8-12時間（1-2セッション）
**優先度**: 🔴 Critical
**関連Issue**: #52
**ゴール寄与率**: +37.5%（3/8画面実装）

#### 実施内容

1. **ユーザー一覧画面実装**（3-4h）
   - **成果物**: `Components/Pages/Admin/UserManagement/UserList.razor`
   - **機能**: 一覧表示・検索・フィルタ・ページング・編集/削除ボタン（権限制御）
   - **参照**: UI設計書3.6節
   - **SubAgent**: csharp-web-ui

2. **ユーザー登録画面実装**（2-3h）
   - **成果物**: `Components/Pages/Admin/UserManagement/UserCreate.razor`
   - **機能**: メールアドレス・氏名・初期パスワード・ロール・所属プロジェクト
   - **参照**: UI設計書3.7節
   - **SubAgent**: csharp-web-ui

3. **ユーザー編集画面実装**（3-4h）
   - **成果物**: `Components/Pages/Admin/UserManagement/UserEdit.razor`
   - **機能**: 氏名・ロール・所属プロジェクト・ステータス・パスワードリセット
   - **参照**: UI設計書3.8節
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
- UserList/UserCreate/UserEditコンポーネントテスト
- バリデーションロジックテスト
- 権限制御ロジックテスト

**統合テスト**:
- User CRUD API統合テスト
- 権限別データフィルタリングテスト

**既存テスト調査**:
- ⚠️ Pages/Admin/配下の既存テスト確認
- 移行 or 新設判断必要

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

### Step 3: Phase B UI拡張 + Agent検証

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

## 📊 Phase B-F3 Part 1 全体スケジュール

### タイムライン

| Step | タスク内容 | 推定時間 | セッション | 累積時間 |
|------|-----------|---------|-----------|---------|
| **Step 1** | Phase A対応漏れ（ユーザー管理UI） | 8-12h | 1-2 | 8-12h |
| **Step 2** | Phase A対応漏れ（認証補助機能UI） | 5-8h | 1 | 13-20h |
| **Step 3** | Phase B UI拡張 + Agent検証 | 2-4h | 1 | 15-24h |
| **Step 4** | システム改善・Issue対処（並列） | 4-5.5h | 1 | 19-29.5h |
| **合計** | Phase B-F3 Part 1 | **19-29.5h** | **3-5** | - |

### マイルストーン

| マイルストーン | 達成基準 | 完了予定 |
|---------------|---------|---------|
| **MS1**: Phase A 100%完成 | UI設計書8画面100%実装・エンドユーザー操作100% | Step 1-2完了時 |
| **MS2**: Phase B Issue対処完了 | Issue #59, #57, #46, #53, #19完全解決 | Step 3-4完了時 |

---

## 🎯 Phase B-F3 Part 1 完成基準（詳細）

### Phase A完了基準（MS1達成時）

| 項目 | 達成基準 | 検証方法 |
|------|---------|---------|
| **UI実装** | UI設計書8画面100%実装 | 全画面動作確認 |
| **エンドユーザー操作** | 100%操作可能（37.5% → 100%） | 受け入れテスト実施 |
| **E2Eテスト** | 19シナリオ全Pass（6/19 → 19/19） | dotnet test実行 |
| **品質** | 0 Warning/0 Error | dotnet build確認 |
| **アーキテクチャ** | Clean Architecture 97点維持 | clean-architecture-guardian確認 |

### Phase B前提条件整備（MS2達成時）

| 項目 | 達成基準 | 検証方法 |
|------|---------|---------|
| **Issue解決** | 7 Issues Close (#44, #59, #57, #46, #53, #19, #52) | GitHub Issue確認 |
| **システム改善** | テスト戦略改善実施完了 | step-start.md等更新確認 |
| **Agent検証** | e2e-test Agent運用効果測定完了 | レポート作成確認 |

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
| ❌ UI実装率37.5%で完了 | ✅ 100%完成のみ受入 |
| ❌ ゴール具体化不足 | ✅ UI設計書8画面明示 |
| ❌ 受け入れ基準曖昧 | ✅ エンドユーザー視点100%操作可能 |
| ❌ Phase完了判定甘い | ✅ 理想状態のみ受入 |
| ❌ 縦スライス実装違反 | ✅ 機能単位全層貫通実装 |

---

## 📝 備考

### Context圧縮対策

**本ドラフト作成理由**:
- Context圧縮により重要情報が失われるリスク回避
- 昨日作成資料（PhaseB3開始前タスク実施計画.md等）の理解保持
- Phase B-F3実行時の参照ドキュメントとして活用

### 次のアクション

**phase-start Command実行時**:
1. 本ドラフトを基にPhase_Summary.md正式版作成
2. `Doc/08_Organization/Active/Phase_B-F3/Phase_Summary.md` に配置
3. Section 2以降の実行継続

---

**ドラフト作成日**: 2025-11-21
**作成者**: Claude Code
**ステータス**: ドラフト版（phase-start実行前）
**正式版作成予定**: phase-start Command実行時（Section 2）
