# Step 02 組織設計・実行記録

## 🎯 Step目的（Why）

**このStepで達成すべきこと**:
- Phase A仕様で定義された3つの認証補助UI画面を完全実装
- E2Eテストによる品質保証（3シナリオ追加）

**Phase全体における位置づけ**:
- **Phase全体の課題**: Phase A仕様ギャップ解消（UI実装系）
- **このStepの役割**: Phase A認証機能の完全完了（未実装3画面の実装）

**関連Issue**: なし（Phase A完了タスク）

---

## 📋 Step概要

| 項目 | 内容 |
|------|------|
| **Step名** | Step02 Phase A認証補助機能UI |
| **作業特性** | UI実装・E2Eテスト |
| **推定工数** | 5-8時間 |
| **開始日** | 2025-12-18 |
| **前提条件** | Step1.5完了 ✅ |

---

## 🏢 組織設計

### SubAgent構成

| SubAgent | 責務 | Stage |
|----------|------|-------|
| **csharp-web-ui** | Blazor Serverコンポーネント実装 | Stage 1-3, 5（修正時） |
| **e2e-test** | Playwright E2Eテスト実装 | Stage 6 |
| **spec-compliance** | 仕様準拠・品質検証 | Stage 7 |

### 並列実行計画

```
Stage 1 (Profile.razor)     ─┬─ 並列実行可能（csharp-web-ui × 3）
Stage 2 (ForgotPassword)    ─┤
Stage 3 (ResetPassword)     ─┘
          ↓
Stage 4 (旧ファイル削除)     ← シーケンシャル
          ↓
Stage 5 (ユーザー確認)       ← 🔴 ユーザー承認必須
          ↓
Stage 6 (E2Eテスト)          ← シーケンシャル
          ↓
Stage 7 (統合テスト)         ← シーケンシャル
```

### ユーザー対話結果

| 項目 | 決定内容 | 理由 |
|------|---------|------|
| **Profile.razor移行方針** | 全面書き換え | 一貫性確保・新ディレクトリへの新規作成 |
| **旧ForgotPassword/ResetPassword参照** | 参考にしない | 新規実装・UI設計書準拠 |

---

## 📚 必須参照ファイル

| ファイル | 参照目的 | 重点セクション |
|---------|---------|---------------|
| `Doc/02_Design/UI設計/01_認証・ユーザー管理画面設計.md` | UI仕様 | 3.2節、3.4節、3.5節 |
| `src/.../Contracts/Interfaces/IPasswordResetService.cs` | サービスIF | 全体 |
| `tests/.../E2E.Tests/authentication.spec.ts` | E2E実装対象 | skipped 3シナリオ |
| `.claude/rules/tests/e2e-test-data-testid-naming.md` | data-testid命名 | 全体 |

---

## 🎯 実装対象

### UI画面（3画面）

| 画面 | UI設計書 | 実装ファイル | レイアウト |
|------|---------|-------------|-----------|
| プロフィール変更 | 3.2節 | `Components/Pages/Auth/Profile.razor` | MainLayout（サイドメニューあり） |
| パスワードリセットメール送信 | 3.4節 | `Components/Pages/Auth/ForgotPassword.razor` | CenteredLayout（サイドメニューなし） |
| パスワードリセット実行 | 3.5節 | `Components/Pages/Auth/ResetPassword.razor` | CenteredLayout（サイドメニューなし） |

### E2Eテスト（3シナリオ）

| テスト名 | 内容 |
|---------|------|
| `PasswordReset_ValidEmail_ShowsSuccessMessage` | 有効メールでリセット要求→成功メッセージ |
| `PasswordReset_ValidToken_ShowsSuccessMessage` | 有効トークンでパスワード変更→成功 |
| `PasswordReset_InvalidToken_ShowsErrorMessage` | 無効トークン→エラーメッセージ |

---

## 📊 Stage構成（詳細）

### Stage 1: Profile.razor全面書き換え（1-2時間）

**目的**: UI設計書3.2節準拠のプロフィール変更画面を新規作成

**作業内容**:
1. 旧`Pages/Auth/Profile.razor`を確認（削除対象として認識）
2. UI設計書3.2節の仕様確認
3. `Components/Pages/Auth/Profile.razor`を新規作成
   - `@page "/profile"`
   - `@attribute [Authorize]`
   - UserManager統合
   - 名前編集フォーム（EditFormコンポーネント）
   - Email読み取り専用表示
   - サイドメニュー表示（MainLayout適用）
4. data-testid付与（E2Eテスト対応）
   - `profile-name-input`
   - `profile-email-display`
   - `profile-save-button`
   - `profile-success-message`
   - `profile-error-message`

**UI設計書3.2節仕様**:
- 画面タイトル: 「プロフィール設定」
- 編集可能項目: 名前のみ
- 読み取り専用: メールアドレス
- 保存ボタン: 「保存」
- 成功メッセージ: 「プロフィールを更新しました」

**SubAgent**: csharp-web-ui
**依存関係**: なし（独立実行可能）

---

### Stage 2: ForgotPassword.razor新規作成（2-3時間）

**目的**: UI設計書3.4節準拠のパスワードリセットメール送信画面を新規作成

**作業内容**:
1. UI設計書3.4節の仕様確認
2. `Components/Pages/Auth/ForgotPassword.razor`を新規作成
   - `@page "/forgot-password"`
   - `@attribute [AllowAnonymous]`
   - サイドメニュー非表示（CenteredLayout適用）
   - Email入力フォーム
   - IPasswordResetService.RequestPasswordResetAsync呼び出し
   - 成功メッセージ（「リセットリンクを送信しました」）
   - エラーメッセージ表示
3. data-testid付与
   - `forgot-password-email-input`
   - `forgot-password-submit-button`
   - `forgot-password-success-message`
   - `forgot-password-error-message`

**UI設計書3.4節仕様**:
- 画面タイトル: 「パスワードリセット」
- 説明文: 「登録されたメールアドレスにリセットリンクを送信します」
- 入力項目: メールアドレス
- 送信ボタン: 「リセットリンクを送信」
- 成功メッセージ: 「パスワードリセットのリンクをメールで送信しました。」
- ログインリンク: 「ログイン画面に戻る」

**SubAgent**: csharp-web-ui
**依存関係**: IPasswordResetService（実装済み）

---

### Stage 3: ResetPassword.razor新規作成（2-3時間）

**目的**: UI設計書3.5節準拠のパスワードリセット実行画面を新規作成

**作業内容**:
1. UI設計書3.5節の仕様確認
2. `Components/Pages/Auth/ResetPassword.razor`を新規作成
   - `@page "/reset-password"`
   - `@attribute [AllowAnonymous]`
   - クエリパラメータ: `email`, `token`
   - サイドメニュー非表示（CenteredLayout適用）
   - トークン検証（ValidateResetTokenAsync）
   - 新パスワード入力フォーム（確認入力含む）
   - パスワードリセット実行（ResetPasswordAsync）
   - 成功時ログイン画面へリダイレクト
3. data-testid付与
   - `reset-password-new-input`
   - `reset-password-confirm-input`
   - `reset-password-submit-button`
   - `reset-password-success-message`
   - `reset-password-error-message`
   - `reset-password-invalid-token-message`

**UI設計書3.5節仕様**:
- 画面タイトル: 「新しいパスワードの設定」
- 入力項目: 新しいパスワード、パスワード確認
- 実行ボタン: 「パスワードを変更」
- 成功メッセージ: 「パスワードを変更しました。ログイン画面に移動します。」
- 無効トークン: 「リセットリンクが無効または期限切れです」
- バリデーション: パスワード一致確認、強度要件

**SubAgent**: csharp-web-ui
**依存関係**: IPasswordResetService（実装済み）

---

### Stage 4: 旧ファイル削除・ディレクトリ整理（30分）

**目的**: Issue #44（Web層ディレクトリ構造統一）対応・旧ファイル削除

**作業内容**:
1. `Pages/Auth/`配下の旧ファイル削除
   - `Profile.razor`（移行完了後）
   - `ForgotPassword.razor`（存在する場合）
   - `ResetPassword.razor`（存在する場合）
2. Issue #44対応確認
   - 認証関連画面が`Components/Pages/Auth/`に統一されていること
3. ルーティング整合性確認
   - `/profile`, `/forgot-password`, `/reset-password`が正常動作

**SubAgent**: MainAgent直接実行（軽量作業）
**依存関係**: Stage 1-3完了後

---

### Stage 5: ユーザー確認・UIフィードバック対応（時間可変）

**目的**: UI実装成果物のユーザー確認・フィードバック反映

**作業内容**:

1. **ユーザー確認依頼**
   - Stage 1-4完了報告
   - 3画面の実装完了を通知
   - 確認観点の提示:
     - レイアウト・配置
     - ボタン・入力フィールドの位置
     - メッセージ文言
     - 画面遷移フロー

2. **確認方法の選択肢提示**
   - Option A: Playwright UI Verification Skillでスクリーンショット取得
   - Option B: ユーザー自身でブラウザ確認（https://localhost:5001）
   - Option C: 両方併用

3. **フィードバック対応**（ユーザー指摘があった場合）
   - UIレイアウト修正
   - 文言修正
   - スタイル調整
   - 再確認

**ユーザー確認チェックリスト**:
- [ ] Profile.razor（プロフィール変更画面）
  - [ ] レイアウト・配置OK
  - [ ] 名前入力フィールドOK
  - [ ] メールアドレス表示OK
  - [ ] 保存ボタンOK
- [ ] ForgotPassword.razor（パスワードリセットメール送信画面）
  - [ ] 中央配置OK
  - [ ] メールアドレス入力OK
  - [ ] 送信ボタンOK
  - [ ] ログインリンクOK
- [ ] ResetPassword.razor（パスワードリセット実行画面）
  - [ ] 中央配置OK
  - [ ] パスワード入力フィールドOK
  - [ ] 確認入力フィールドOK
  - [ ] 変更ボタンOK

**SubAgent**: csharp-web-ui（修正が必要な場合）
**依存関係**: Stage 4完了後
**🔴 重要**: ユーザー承認取得後にStage 6へ進行

---

### Stage 6: E2Eテスト実装（1-2時間）

**目的**: パスワードリセットフローのE2Eテスト3シナリオを実装

**作業内容**:
1. `authentication.spec.ts`の3つのskippedテストを実装

**テストシナリオ詳細**:

```typescript
// 1. PasswordReset_ValidEmail_ShowsSuccessMessage
test('PasswordReset_ValidEmail_ShowsSuccessMessage', async ({ page }) => {
  // ForgotPassword画面にアクセス
  await page.goto('/forgot-password');
  // メールアドレス入力
  await page.fill('[data-testid="forgot-password-email-input"]', 'test@example.com');
  // 送信ボタンクリック
  await page.click('[data-testid="forgot-password-submit-button"]');
  // 成功メッセージ確認
  await expect(page.locator('[data-testid="forgot-password-success-message"]')).toBeVisible();
});

// 2. PasswordReset_ValidToken_ShowsSuccessMessage
test('PasswordReset_ValidToken_ShowsSuccessMessage', async ({ page }) => {
  // 有効なトークンでResetPassword画面にアクセス
  await page.goto('/reset-password?email=test@example.com&token=valid-token');
  // 新パスワード入力
  await page.fill('[data-testid="reset-password-new-input"]', 'NewPassword123!');
  await page.fill('[data-testid="reset-password-confirm-input"]', 'NewPassword123!');
  // 送信ボタンクリック
  await page.click('[data-testid="reset-password-submit-button"]');
  // 成功メッセージ確認
  await expect(page.locator('[data-testid="reset-password-success-message"]')).toBeVisible();
});

// 3. PasswordReset_InvalidToken_ShowsErrorMessage
test('PasswordReset_InvalidToken_ShowsErrorMessage', async ({ page }) => {
  // 無効なトークンでResetPassword画面にアクセス
  await page.goto('/reset-password?email=test@example.com&token=invalid-token');
  // エラーメッセージ確認
  await expect(page.locator('[data-testid="reset-password-invalid-token-message"]')).toBeVisible();
});
```

2. Profile画面のE2Eテスト追加（必要に応じて）
3. data-testidセレクタ使用確認

**SubAgent**: e2e-test
**依存関係**: Stage 5完了（ユーザー承認取得）後

---

### Stage 7: 統合テスト・品質検証（1時間）

**目的**: 全体品質確認・仕様準拠検証

**作業内容**:
1. 全体ビルド確認
   ```bash
   docker exec ubiquitous-lang-mng_devcontainer-devcontainer-1 dotnet build
   ```
   - 0 Warning/0 Error確認

2. 全テスト実行・Pass確認
   ```bash
   docker exec ubiquitous-lang-mng_devcontainer-devcontainer-1 dotnet test
   ```
   - 100% Pass確認

3. E2Eテスト一括実行
   ```bash
   docker exec ubiquitous-lang-mng_devcontainer-devcontainer-1 bash tests/run-e2e-tests.sh
   ```
   - 全シナリオPass確認

4. 手動動作確認（Playwright UI Verification Skill活用）
   - プロフィール変更フロー
   - パスワードリセットフロー（メール送信→リセット実行）

5. spec-compliance-check実行

**SubAgent**: spec-compliance
**依存関係**: Stage 1-6完了後

---

## 🎯 Step成功基準

### 成果物チェックリスト

- [ ] `Components/Pages/Auth/Profile.razor` 新規作成・動作確認
- [ ] `Components/Pages/Auth/ForgotPassword.razor` 新規作成・動作確認
- [ ] `Components/Pages/Auth/ResetPassword.razor` 新規作成・動作確認
- [ ] `Pages/Auth/` 配下の旧ファイル削除
- [ ] **ユーザーUI確認・承認取得完了**
- [ ] E2Eテスト3シナリオ実装・Pass
- [ ] 全体ビルド 0 Warning/0 Error
- [ ] 全テスト100% Pass

### 品質基準

- [ ] UI設計書3.2/3.4/3.5節仕様準拠
- [ ] data-testid命名規則準拠
- [ ] Clean Architecture準拠（Web層責務のみ）
- [ ] IPasswordResetService正常統合

---

## 📊 Step実行記録（随時更新）

### Stage 1: Profile.razor全面書き換え
**開始**: - | **完了**: -

#### 実行内容
- [実行した作業内容]
- [使用したSubAgent]

#### Skills使用報告（効果測定・Issue #81）
| 使用者 | Skill名 | 参照タイミング | 判断・適用内容 |
|--------|---------|---------------|---------------|
| - | - | - | - |

#### Stage結果
- [成果物・完了事項]
- [次Stageへの申し送り（該当時）]

---

### Stage 2: ForgotPassword.razor新規作成
**開始**: - | **完了**: -

#### 実行内容
- [実行した作業内容]

#### Skills使用報告（効果測定・Issue #81）
| 使用者 | Skill名 | 参照タイミング | 判断・適用内容 |
|--------|---------|---------------|---------------|
| - | - | - | - |

#### Stage結果
- [成果物・完了事項]

---

### Stage 3: ResetPassword.razor新規作成
**開始**: - | **完了**: -

#### 実行内容
- [実行した作業内容]

#### Skills使用報告（効果測定・Issue #81）
| 使用者 | Skill名 | 参照タイミング | 判断・適用内容 |
|--------|---------|---------------|---------------|
| - | - | - | - |

#### Stage結果
- [成果物・完了事項]

---

### Stage 4: 旧ファイル削除・ディレクトリ整理
**開始**: - | **完了**: -

#### 実行内容
- [実行した作業内容]

#### Stage結果
- [成果物・完了事項]

---

### Stage 5: ユーザー確認・UIフィードバック対応
**開始**: - | **完了**: -

#### ユーザー確認結果
- [ ] Profile.razor確認完了
- [ ] ForgotPassword.razor確認完了
- [ ] ResetPassword.razor確認完了

#### フィードバック対応（該当時）
| 画面 | 指摘内容 | 対応内容 |
|------|---------|---------|
| - | - | - |

#### Stage結果
- [ユーザー承認取得日時]
- [修正有無・内容]

---

### Stage 6: E2Eテスト実装
**開始**: - | **完了**: -

#### 実行内容
- [実行した作業内容]

#### Skills使用報告（効果測定・Issue #81）
| 使用者 | Skill名 | 参照タイミング | 判断・適用内容 |
|--------|---------|---------------|---------------|
| - | - | - | - |

#### Stage結果
- [成果物・完了事項]

---

### Stage 7: 統合テスト・品質検証
**開始**: - | **完了**: -

#### 実行内容
- [実行した作業内容]

#### Stage結果
- [成果物・完了事項]

---

## ✅ Step終了時レビュー

[Step完了時に更新]

---

**作成日**: 2025-12-18
**作成者**: MainAgent
**テンプレート**: step-start Command Step2以降テンプレート
