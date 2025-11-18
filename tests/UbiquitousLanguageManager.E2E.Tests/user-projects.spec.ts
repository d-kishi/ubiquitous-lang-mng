import { test, expect } from '@playwright/test';

/**
 * User Projects Feature E2E Tests
 *
 * Test Scenarios: 3 scenarios
 * - Positive scenarios: Add member success, Remove member success
 * - Negative scenarios: Duplicate member error
 *
 * Technical Requirements:
 * - ViewportSize: 1920x1080 (Full HD)
 * - data-testid selectors
 * - Blazor Server SignalR対応
 * - JavaScript Dialog handling
 *
 * GitHub Issue #56対応: bUnit技術的課題のE2E代替実装
 */

const BASE_URL = 'https://localhost:5001';
const TEST_EMAIL = 'e2e-test@ubiquitous-lang.local';
const TEST_PASSWORD = 'E2ETest#2025!Secure';

test.describe('User Projects Feature', () => {
  /**
   * 共通ヘルパー: ログイン・メンバー管理画面遷移
   */
  async function navigateToMemberManagement(page) {
    // ログイン
    await page.goto(BASE_URL);
    await page.waitForLoadState('networkidle');

    await page.fill('[data-testid="username-input"]', TEST_EMAIL);
    await page.fill('[data-testid="password-input"]', TEST_PASSWORD);
    await page.click('[data-testid="login-button"]');
    await page.waitForLoadState('networkidle');

    // メンバー管理画面遷移
    const memberLink = page.locator('[data-testid="member-management-link"]').first();
    await memberLink.waitFor({ state: 'visible', timeout: 5000 });
    await memberLink.click();

    // URL確認
    await page.waitForURL('**/projects/*/members');
    await page.waitForLoadState('networkidle');
  }

  // Scenario 1: 正常系 - メンバー追加成功
  test('AddMember_ValidInput_ShowsSuccessMessage', async ({ page }) => {
    // 前提条件: ログイン・メンバー管理画面遷移
    await navigateToMemberManagement(page);

    // ユーザー選択ドロップダウン操作
    const memberSelector = page.locator('[data-testid="member-selector"]');
    await memberSelector.waitFor({ state: 'visible', timeout: 5000 });

    // ドロップダウンから最初のユーザーを選択（index 0 = 空白、index 1 = 最初のユーザー）
    await memberSelector.selectOption({ index: 1 });

    // 「追加」ボタンをクリック
    await page.click('[data-testid="member-add-button"]');

    // 成功メッセージ検証
    const toastLocator = page.locator('.toast-success, [role="alert"]');
    await toastLocator.waitFor({ state: 'visible', timeout: 5000 });

    const toastText = await toastLocator.textContent();
    expect(toastText).toContain('プロジェクトに追加しました');

    // メンバー一覧自動更新確認（StateHasChanged()による非同期UI更新待機）
    await page.waitForTimeout(1000); // SignalR更新待機

    const memberList = page.locator('[data-testid="member-list"]');
    const memberCount = await memberList.locator('[data-testid="member-card"]').count();
    expect(memberCount).toBeGreaterThan(0);
  });

  // Scenario 2: 正常系 - メンバー削除成功
  test('RemoveMember_ValidInput_ShowsSuccessMessage', async ({ page }) => {
    // 前提条件: ログイン・メンバー管理画面遷移
    await navigateToMemberManagement(page);

    // 削除前のメンバー数記録
    const memberList = page.locator('[data-testid="member-list"]');
    const initialMemberCount = await memberList.locator('[data-testid="member-card"]').count();

    // JavaScript confirmダイアログを自動承認
    page.on('dialog', async dialog => {
      expect(dialog.type()).toBe('confirm');
      expect(dialog.message()).toContain('削除');
      await dialog.accept();
    });

    // 既存メンバーの「削除」ボタンをクリック
    const deleteButton = page.locator('[data-testid="member-delete-button"]').first();
    await deleteButton.waitFor({ state: 'visible', timeout: 5000 });
    await deleteButton.click();

    // 成功メッセージ検証
    const toastLocator = page.locator('.toast-success, [role="alert"]');
    await toastLocator.waitFor({ state: 'visible', timeout: 5000 });

    const toastText = await toastLocator.textContent();
    expect(toastText).toContain('削除しました');

    // メンバー一覧自動更新確認
    await page.waitForTimeout(1000); // SignalR更新待機

    const finalMemberCount = await memberList.locator('[data-testid="member-card"]').count();
    expect(finalMemberCount).toBeLessThan(initialMemberCount);
  });

  // Scenario 3: 異常系 - 重複メンバー追加エラー
  test('AddDuplicateMember_ShowsErrorMessage', async ({ page }) => {
    // 前提条件: ログイン・メンバー管理画面遷移
    await navigateToMemberManagement(page);

    // 既存メンバーのユーザー名取得（最初のメンバーカードから）
    const firstMemberCard = page.locator('[data-testid="member-card"]').first();
    await firstMemberCard.waitFor({ state: 'visible', timeout: 5000 });

    const memberName = await firstMemberCard.locator('[data-testid="member-name"]').textContent();

    // 同じユーザーを選択して追加を試みる
    const memberSelector = page.locator('[data-testid="member-selector"]');
    await memberSelector.waitFor({ state: 'visible', timeout: 5000 });
    await memberSelector.selectOption({ label: memberName || '' });
    await page.click('[data-testid="member-add-button"]');

    // エラーメッセージ検証
    const errorLocator = page.locator('[data-testid="member-error-message"]');
    await errorLocator.waitFor({ state: 'visible', timeout: 5000 });

    const errorText = await errorLocator.textContent();
    expect(errorText).toContain('既にこのプロジェクトのメンバーです');
  });
});
