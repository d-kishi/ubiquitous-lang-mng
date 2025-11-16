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

const BASE_URL = 'https://localhost:5001';
const TEST_EMAIL = 'admin@example.com';
const TEST_PASSWORD = 'Admin123!';

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
    await expect(page.locator('[data-testid="logout-button"]')).toBeVisible({ timeout: 5000 });

    // ViewportSize 1920x1080でNavMenu折りたたみなし確認
    const navMenu = page.locator('.navbar-collapse');
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

    // バリデーションエラー表示確認
    const validationErrors = page.locator('.validation-message, [data-testid*="validation"]');
    const errorCount = await validationErrors.count();
    expect(errorCount).toBeGreaterThan(0);
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
    await page.fill('#currentPassword', 'NewAdmin123!');
    await page.fill('#newPassword', TEST_PASSWORD);
    await page.fill('#confirmPassword', TEST_PASSWORD);
    await page.click('button[type="submit"]');
    await page.waitForLoadState('networkidle');
  });

  // Scenario 6: 異常系 - 現在のパスワード不一致
  test('ChangePassword_WrongCurrentPassword_ShowsErrorMessage', async ({ page }) => {
    // 前提条件: ログイン
    await page.goto(BASE_URL);
    await page.fill('[data-testid="username-input"]', TEST_EMAIL);
    await page.fill('[data-testid="password-input"]', TEST_PASSWORD);
    await page.click('[data-testid="login-button"]');
    await page.waitForLoadState('networkidle');

    // パスワード変更画面遷移
    await page.goto(`${BASE_URL}/change-password`);
    await page.waitForLoadState('networkidle');
    await page.waitForTimeout(1000);

    // URL確認
    const currentUrl = page.url();
    expect(currentUrl).toContain('/change-password');

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
