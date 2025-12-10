import { test, expect } from '@playwright/test';

/**
 * Phase A Authentication Feature E2E Tests
 *
 * Test Scenarios: 9 scenarios (6 implemented + 3 skipped)
 * - 3 Positive scenarios: Login success, Logout success, Change password success
 * - 3 Negative scenarios: Empty fields, Invalid credentials, Wrong password
 * - 3 Skipped scenarios: Password reset (未実装機能)
 *
 * Technical Requirements:
 * - ViewportSize: 1920x1080 (Full HD)
 * - data-testid selectors
 * - Blazor Server SignalR対応
 */

const BASE_URL = process.env.BASE_URL || 'https://localhost:5001';

// E2Eテスト用アカウント（Stage 1で作成済み）
const TEST_ACCOUNTS = {
  SuperUser: {
    email: 'e2e-test@ubiquitous-lang.local',
    password: 'E2ETest#2025!Secure'
  },
  ProjectManager: {
    email: 'e2e-test-pm@ubiquitous-lang.local',
    password: 'Test123!'
  },
  DomainApprover: {
    email: 'e2e-test-da@ubiquitous-lang.local',
    password: 'Test123!'
  },
  GeneralUser: {
    email: 'e2e-test-gu@ubiquitous-lang.local',
    password: 'Test123!'
  }
};

// 後方互換性のために維持
const TEST_EMAIL = TEST_ACCOUNTS.SuperUser.email;
const TEST_PASSWORD = TEST_ACCOUNTS.SuperUser.password;

test.describe('Phase A Authentication Feature', () => {
  // Scenario 1: 正常系 - ログイン成功
  test('Login_ValidCredentials_ShowsHomePage', async ({ page }) => {
    await page.goto(BASE_URL);
    await page.waitForLoadState('networkidle');

    // data-testid属性でセレクタ指定
    await page.fill('[data-testid="username-input"]', TEST_EMAIL);
    await page.fill('[data-testid="password-input"]', TEST_PASSWORD);
    await page.click('[data-testid="login-button"]');

    // Blazor Server SignalR接続確立待機
    await page.waitForLoadState('networkidle');

    // ログイン成功確認（NavMenu表示・ログアウトボタン表示）
    await expect(page.locator('[data-testid="logout-button"]').first()).toBeVisible({ timeout: 5000 });

    // ViewportSize 1920x1080でNavMenu折りたたみなし確認
    const navMenu = page.locator('.nav-scrollable');
    await expect(navMenu).toBeVisible();
  });

  // Scenario 2: 正常系 - ログアウト成功
  test('Logout_AfterLogin_RedirectsToLoginPage', async ({ page }) => {
    // 前提条件: ログイン
    await page.goto(BASE_URL);
    await page.fill('[data-testid="username-input"]', TEST_EMAIL);
    await page.fill('[data-testid="password-input"]', TEST_PASSWORD);
    await page.click('[data-testid="login-button"]');
    await page.waitForLoadState('networkidle');

    // ログアウトボタンクリック
    const logoutButton = page.locator('[data-testid="logout-button"]').first();
    await logoutButton.waitFor({ state: 'visible', timeout: 5000 });
    await logoutButton.click();

    // Blazor Server SignalR処理待機
    await page.waitForLoadState('networkidle');
    await page.waitForTimeout(1000);

    // ログインフォーム表示確認
    await expect(page.locator('[data-testid="username-input"]')).toBeVisible({ timeout: 5000 });
    await expect(page.locator('[data-testid="password-input"]')).toBeVisible();
  });

  // Scenario 3: 異常系 - メールアドレス・パスワード未入力
  test('Login_EmptyFields_ShowsValidationErrors', async ({ page }) => {
    await page.goto(BASE_URL);
    await page.waitForLoadState('networkidle');

    // 空のまま送信
    await page.click('[data-testid="login-button"]');
    await page.waitForTimeout(500);

    // HTML5バリデーションまたはBlazorバリデーションにより送信が阻止されることを確認
    // URLが変わっていない（ログインページに留まる）ことで検証
    const currentUrl = page.url();
    expect(currentUrl).toContain('/login');
  });

  // Scenario 4: 異常系 - 存在しないユーザー・パスワード不一致
  test('Login_InvalidCredentials_ShowsErrorMessage', async ({ page }) => {
    await page.goto(BASE_URL);
    await page.waitForLoadState('networkidle');

    await page.fill('[data-testid="username-input"]', 'invalid@example.com');
    await page.fill('[data-testid="password-input"]', 'WrongPassword!');
    await page.click('[data-testid="login-button"]');

    // エラーメッセージ表示確認（API応答待機）
    const errorLocator = page.locator('.alert-danger, [role="alert"]');
    await errorLocator.waitFor({ state: 'visible', timeout: 10000 });

    const errorText = await errorLocator.textContent();
    expect(errorText).toContain('メールアドレスまたはパスワードが正しくありません');
  });

  // Scenario 5: 正常系 - パスワード変更成功
  test('ChangePassword_ValidInput_ShowsSuccessMessage', async ({ page }) => {
    // 前提条件: ログイン
    await page.goto(BASE_URL);
    await page.fill('[data-testid="username-input"]', TEST_EMAIL);
    await page.fill('[data-testid="password-input"]', TEST_PASSWORD);
    await page.click('[data-testid="login-button"]');
    await page.waitForLoadState('networkidle');

    // パスワード変更画面遷移
    await page.goto(`${BASE_URL}/change-password`);
    await page.waitForLoadState('networkidle');
    await page.waitForTimeout(1000); // Blazor Server SignalR接続完了待機

    // URL確認（リダイレクトされていないか）
    const currentUrl = page.url();
    expect(currentUrl).toContain('/change-password');

    // パスワード変更フォーム入力
    await page.fill('#currentPassword', TEST_PASSWORD);
    await page.fill('#newPassword', 'NewAdmin123!');
    await page.fill('#confirmPassword', 'NewAdmin123!');
    await page.click('button[type="submit"]');

    // 成功メッセージ表示確認
    await page.waitForLoadState('networkidle');
    const successMessage = page.locator('.alert-success, [role="alert"]');
    await expect(successMessage).toBeVisible({ timeout: 5000 });

    // パスワードを元に戻す（テストデータ整合性維持）
    await page.goto(`${BASE_URL}/change-password`);
    await page.waitForLoadState('networkidle');
    await page.waitForTimeout(1000); // Blazor Server SignalR接続完了待機

    await page.fill('#currentPassword', 'NewAdmin123!');
    await page.fill('#newPassword', TEST_PASSWORD);
    await page.fill('#confirmPassword', TEST_PASSWORD);
    await page.click('button[type="submit"]');

    // パスワード復元成功確認（重要: これがないと後続テストが失敗する）
    await page.waitForLoadState('networkidle');
    const revertSuccessMessage = page.locator('.alert-success, [role="alert"]');
    await expect(revertSuccessMessage).toBeVisible({ timeout: 5000 });
  });

  // Scenario 6: 異常系 - 現在のパスワード不一致
  test('ChangePassword_WrongCurrentPassword_ShowsErrorMessage', async ({ page }) => {
    // 前提条件: ログイン
    await page.goto(BASE_URL);
    await page.fill('[data-testid="username-input"]', TEST_EMAIL);
    await page.fill('[data-testid="password-input"]', TEST_PASSWORD);
    await page.click('[data-testid="login-button"]');
    await page.waitForLoadState('networkidle');

    // ログイン成功確認（認証セッション確立待機）
    await expect(page.locator('[data-testid="logout-button"]').first()).toBeVisible({ timeout: 5000 });

    // パスワード変更画面遷移
    await page.goto(`${BASE_URL}/change-password`);
    await page.waitForLoadState('networkidle');
    await page.waitForTimeout(1000); // Blazor Server SignalR接続完了待機

    // 誤った現在のパスワード入力
    await page.fill('#currentPassword', 'WrongPassword!');
    await page.fill('#newPassword', 'NewAdmin123!');
    await page.fill('#confirmPassword', 'NewAdmin123!');
    await page.click('button[type="submit"]');

    // エラーメッセージ表示確認
    const errorMessage = page.locator('.alert-danger, [role="alert"]');
    await expect(errorMessage).toBeVisible({ timeout: 5000 });
  });

  // Scenario 7-9: Skipped - パスワードリセット機能未実装
  test.skip('PasswordReset_ValidEmail_ShowsSuccessMessage', async ({ page }) => {
    // ForgotPassword.razorページ未実装
  });

  test.skip('PasswordReset_ValidToken_ShowsSuccessMessage', async ({ page }) => {
    // ResetPassword.razorページ未実装
  });

  test.skip('PasswordReset_InvalidToken_ShowsErrorMessage', async ({ page }) => {
    // ResetPassword.razorページ未実装
  });
});

/**
 * Phase Issue79 E2E Tests
 *
 * Test Scenarios: 6 scenarios
 * - 4 Role Login Tests: SuperUser, ProjectManager, DomainApprover, GeneralUser
 * - 1 PM User List Access Test: Issue #79 本質的問題検証
 * - 1 PM Project Filter Test: プロジェクトフィルタ動作確認
 */
test.describe('Phase Issue79 - User Management Feature', () => {
  // Scenario 1: SuperUserアカウントログイン確認
  test('Login_SuperUser_ShowsHomePage', async ({ page }) => {
    await page.goto(BASE_URL);
    await page.waitForLoadState('networkidle');

    await page.fill('[data-testid="username-input"]', TEST_ACCOUNTS.SuperUser.email);
    await page.fill('[data-testid="password-input"]', TEST_ACCOUNTS.SuperUser.password);
    await page.click('[data-testid="login-button"]');

    await page.waitForLoadState('networkidle');

    // ログイン成功確認
    await expect(page.locator('[data-testid="logout-button"]').first()).toBeVisible({ timeout: 5000 });
  });

  // Scenario 2: ProjectManagerアカウントログイン確認
  test('Login_ProjectManager_ShowsHomePage', async ({ page }) => {
    await page.goto(BASE_URL);
    await page.waitForLoadState('networkidle');

    await page.fill('[data-testid="username-input"]', TEST_ACCOUNTS.ProjectManager.email);
    await page.fill('[data-testid="password-input"]', TEST_ACCOUNTS.ProjectManager.password);
    await page.click('[data-testid="login-button"]');

    await page.waitForLoadState('networkidle');

    // ログイン成功確認
    await expect(page.locator('[data-testid="logout-button"]').first()).toBeVisible({ timeout: 5000 });
  });

  // Scenario 3: DomainApproverアカウントログイン確認
  test('Login_DomainApprover_ShowsHomePage', async ({ page }) => {
    await page.goto(BASE_URL);
    await page.waitForLoadState('networkidle');

    await page.fill('[data-testid="username-input"]', TEST_ACCOUNTS.DomainApprover.email);
    await page.fill('[data-testid="password-input"]', TEST_ACCOUNTS.DomainApprover.password);
    await page.click('[data-testid="login-button"]');

    await page.waitForLoadState('networkidle');

    // ログイン成功確認
    await expect(page.locator('[data-testid="logout-button"]').first()).toBeVisible({ timeout: 5000 });
  });

  // Scenario 4: GeneralUserアカウントログイン確認
  test('Login_GeneralUser_ShowsHomePage', async ({ page }) => {
    await page.goto(BASE_URL);
    await page.waitForLoadState('networkidle');

    await page.fill('[data-testid="username-input"]', TEST_ACCOUNTS.GeneralUser.email);
    await page.fill('[data-testid="password-input"]', TEST_ACCOUNTS.GeneralUser.password);
    await page.click('[data-testid="login-button"]');

    await page.waitForLoadState('networkidle');

    // ログイン成功確認
    await expect(page.locator('[data-testid="logout-button"]').first()).toBeVisible({ timeout: 5000 });
  });

  // Scenario 5: PM権限ユーザー一覧表示確認（Issue #79 本質的問題）
  test('UserList_ProjectManager_ShowsUserTable', async ({ page }) => {
    // PMアカウントでログイン
    await page.goto(BASE_URL);
    await page.waitForLoadState('networkidle');

    await page.fill('[data-testid="username-input"]', TEST_ACCOUNTS.ProjectManager.email);
    await page.fill('[data-testid="password-input"]', TEST_ACCOUNTS.ProjectManager.password);
    await page.click('[data-testid="login-button"]');

    await page.waitForLoadState('networkidle');

    // ログイン成功確認
    await expect(page.locator('[data-testid="logout-button"]').first()).toBeVisible({ timeout: 5000 });

    // ユーザー一覧画面に遷移
    await page.goto(`${BASE_URL}/admin/users`);
    await page.waitForLoadState('networkidle');
    await page.waitForTimeout(2000); // Blazor Server SignalR接続・データ取得待機

    // ユーザーテーブルが表示されることを確認
    const userTable = page.locator('[data-testid="user-list-table"]');
    await expect(userTable).toBeVisible({ timeout: 10000 });

    // テーブル内にユーザー行が少なくとも1件存在することを確認
    const userRows = page.locator('[data-testid^="user-row-"]');
    const rowCount = await userRows.count();
    expect(rowCount).toBeGreaterThan(0);

    // ユーザー件数バッジが表示されることを確認（ヘッダー内の件数バッジのみ）
    const countBadge = page.locator('.card-header .badge.bg-secondary');
    await expect(countBadge).toBeVisible();
  });

  // Scenario 6: SuperUser権限プロジェクトフィルタ確認（プロジェクトデータがある場合のみ）
  test.skip('UserList_SuperUser_ProjectFilterWorks', async ({ page }) => {
    // SuperUserアカウントでログイン
    await page.goto(BASE_URL);
    await page.waitForLoadState('networkidle');

    await page.fill('[data-testid="username-input"]', TEST_ACCOUNTS.SuperUser.email);
    await page.fill('[data-testid="password-input"]', TEST_ACCOUNTS.SuperUser.password);
    await page.click('[data-testid="login-button"]');

    await page.waitForLoadState('networkidle');

    // ユーザー一覧画面に遷移
    await page.goto(`${BASE_URL}/admin/users`);
    await page.waitForLoadState('networkidle');
    await page.waitForTimeout(2000); // Blazor Server SignalR接続・データ取得待機

    // プロジェクトフィルタドロップダウンが表示されるか確認（プロジェクトがある場合のみ）
    const projectFilter = page.locator('[data-testid="project-filter-dropdown"]');
    const filterVisible = await projectFilter.isVisible({ timeout: 2000 }).catch(() => false);

    if (filterVisible) {
      // プロジェクトがある場合、フィルタ機能をテスト
      const options = await projectFilter.locator('option').all();
      if (options.length > 1) {
        const secondOption = await options[1].getAttribute('value');
        if (secondOption) {
          await projectFilter.selectOption(secondOption);
          await page.waitForTimeout(1000); // フィルタ適用待機

          // フィルタ適用後もユーザーテーブルが表示されることを確認
          const userTable = page.locator('[data-testid="user-list-table"]');
          await expect(userTable).toBeVisible({ timeout: 5000 });

          // ユーザー件数バッジが表示されることを確認（ヘッダー内の件数バッジのみ）
          const countBadge = page.locator('.card-header .badge.bg-secondary');
          await expect(countBadge).toBeVisible();
        }
      }
    } else {
      // プロジェクトがない場合、ユーザーテーブルが表示されることを確認
      const userTable = page.locator('[data-testid="user-list-table"]');
      await expect(userTable).toBeVisible({ timeout: 5000 });

      // ユーザー件数バッジが表示されることを確認
      const countBadge = page.locator('.card-header .badge.bg-secondary');
      await expect(countBadge).toBeVisible();
    }
  });
});
