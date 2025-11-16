import { test, expect } from '@playwright/test';

/**
 * Seed Test: 認証環境初期状態定義
 *
 * 目的: テスト環境の前提条件確認
 * - アプリケーション起動確認
 * - ログインフォーム表示確認
 * - テスト用管理者ログイン動作確認
 * - ViewportSize 1920x1080でNavMenu折りたたみなし確認
 */
test.describe('Seed - Authentication Setup', () => {
  test('seed - authentication environment', async ({ page }) => {
    // アプリケーション起動確認
    await page.goto('https://localhost:5001');

    // ログインフォーム表示確認
    await expect(page.locator('[data-testid="username-input"]')).toBeVisible();
    await expect(page.locator('[data-testid="password-input"]')).toBeVisible();
    await expect(page.locator('[data-testid="login-button"]')).toBeVisible();

    // テスト用管理者ログイン
    await page.fill('[data-testid="username-input"]', 'admin@example.com');
    await page.fill('[data-testid="password-input"]', 'Admin123!');
    await page.click('[data-testid="login-button"]');

    // Blazor Server SignalR接続確立待機
    await page.waitForLoadState('networkidle');

    // ログイン成功確認（NavMenu表示）
    await expect(page.locator('[data-testid="logout-button"]')).toBeVisible({ timeout: 5000 });

    // ViewportSize 1920x1080でNavMenu折りたたみなし確認
    const navMenu = page.locator('.navbar-collapse');
    await expect(navMenu).toBeVisible();

    console.log('✅ Seed Test: Authentication environment is ready');
  });
});
