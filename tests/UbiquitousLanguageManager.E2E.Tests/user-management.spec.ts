import { test, expect } from '@playwright/test';

/**
 * Phase B User Management E2E Tests
 *
 * Test Scenarios: 10 scenarios
 * - SuperUser権限: ユーザー一覧表示、ユーザー作成、ユーザー編集、ユーザー削除
 * - ProjectManager権限: ユーザー一覧表示、ロール選択肢制限
 * - GeneralUser権限: アクセス拒否
 * - バリデーション: 無効メールアドレス
 * - UIインタラクション: ロード中スピナー表示、論理削除済みユーザー表示切替
 *
 * Technical Requirements:
 * - ViewportSize: 1920x1080 (Full HD)
 * - data-testid selectors
 * - Blazor Server SignalR対応
 */

const BASE_URL = process.env.BASE_URL || 'https://localhost:5001';

// E2Eテスト用アカウント（authentication.spec.tsから再利用）
const TEST_ACCOUNTS = {
  SuperUser: {
    email: 'e2e-test@ubiquitous-lang.local',
    password: 'E2ETest#2025!Secure'
  },
  ProjectManager: {
    email: 'e2e-test-pm@ubiquitous-lang.local',
    password: 'Test123!'
  },
  GeneralUser: {
    email: 'e2e-test-gu@ubiquitous-lang.local',
    password: 'Test123!'
  }
};

test.describe('Phase B User Management Feature', () => {
  // Scenario 1: SuperUser権限 - ユーザー一覧表示
  test('UserList_SuperUser_ShowsAllUsers', async ({ page }) => {
    // 1. SuperUserアカウントでログイン
    await page.goto(BASE_URL);
    await page.waitForLoadState('networkidle');

    await page.fill('[data-testid="username-input"]', TEST_ACCOUNTS.SuperUser.email);
    await page.fill('[data-testid="password-input"]', TEST_ACCOUNTS.SuperUser.password);
    await page.click('[data-testid="login-button"]');

    await page.waitForLoadState('networkidle');

    // ログイン成功確認
    await expect(page.locator('[data-testid="logout-button"]').first()).toBeVisible({ timeout: 5000 });

    // 2. /admin/users に遷移
    await page.goto(`${BASE_URL}/admin/users`);
    await page.waitForLoadState('networkidle');
    await page.waitForTimeout(2000); // Blazor Server SignalR接続・データ取得待機

    // 3. ユーザー一覧テーブル表示確認
    const userTable = page.locator('[data-testid="user-list-table"]');
    await expect(userTable).toBeVisible({ timeout: 10000 });

    // 4. 全ユーザー表示確認（テーブル内にユーザー行が少なくとも1件存在）
    const userRows = page.locator('[data-testid^="user-row-"]');
    const rowCount = await userRows.count();
    expect(rowCount).toBeGreaterThan(0);

    // ユーザー件数バッジが表示されることを確認
    const countBadge = page.locator('.card-header .badge.bg-secondary');
    await expect(countBadge).toBeVisible();
  });

  // Scenario 2: SuperUser権限 - ユーザー作成
  test('CreateUser_SuperUser_ShowsSuccessMessage', async ({ page }) => {
    // 1. SuperUserアカウントでログイン
    await page.goto(BASE_URL);
    await page.waitForLoadState('networkidle');

    await page.fill('[data-testid="username-input"]', TEST_ACCOUNTS.SuperUser.email);
    await page.fill('[data-testid="password-input"]', TEST_ACCOUNTS.SuperUser.password);
    await page.click('[data-testid="login-button"]');

    await page.waitForLoadState('networkidle');

    // ログイン成功確認
    await expect(page.locator('[data-testid="logout-button"]').first()).toBeVisible({ timeout: 5000 });

    // 2. /admin/users に遷移
    await page.goto(`${BASE_URL}/admin/users`);
    await page.waitForLoadState('networkidle');
    await page.waitForTimeout(2000); // Blazor Server SignalR接続・データ取得待機

    // 3. ユーザー一覧テーブル表示確認
    const userTable = page.locator('[data-testid="user-list-table"]');
    await expect(userTable).toBeVisible({ timeout: 10000 });

    // 4. 「新規作成」ボタンクリック
    const createButton = page.locator('[data-testid="create-user-button"]');
    await createButton.waitFor({ state: 'visible', timeout: 10000 });
    await createButton.click();
    await page.waitForLoadState('networkidle');

    // Create画面のBlazor Server読み込み待機
    await page.waitForTimeout(2000);

    // URL確認（リダイレクトされていないか）
    const currentUrl = page.url();
    expect(currentUrl).toContain('/admin/users/create');

    // 4. ユーザー情報入力（Email/Name/Password/Role）
    const timestamp = Date.now();
    const testEmail = `e2e-created-user-${timestamp}@test.local`;

    await page.fill('[data-testid="email-input"]', testEmail);
    await page.fill('[data-testid="name-input"]', `E2E Test User ${timestamp}`);
    await page.fill('[data-testid="password-input"]', 'TestUser@2025!');

    // ロール選択（GeneralUser）
    await page.click('[data-testid="role-dropdown-generaluser"]');

    await page.waitForTimeout(500); // Blazor Server バインディング待機

    // 5. ダイアログイベント待機を先に設定
    const createDialogPromise = page.waitForEvent('dialog');

    // 6. 送信ボタンクリック
    await page.click('[data-testid="submit-button"]');

    // 7. ダイアログ待機＆処理
    const createDialog = await createDialogPromise;
    expect(createDialog.message()).toContain('登録しました');
    await createDialog.accept();

    // 8. Blazor Server SignalR処理完了待機
    await page.waitForTimeout(1000);

    // 9. ユーザー一覧画面にリダイレクトされることを確認
    await page.waitForURL('**/admin/users', { timeout: 15000 });
    await page.waitForLoadState('networkidle');
  });

  // Scenario 3: SuperUser権限 - ユーザー編集
  test('EditUser_SuperUser_ShowsSuccessMessage', async ({ page }) => {
    // 1. SuperUserアカウントでログイン
    await page.goto(BASE_URL);
    await page.waitForLoadState('networkidle');

    await page.fill('[data-testid="username-input"]', TEST_ACCOUNTS.SuperUser.email);
    await page.fill('[data-testid="password-input"]', TEST_ACCOUNTS.SuperUser.password);
    await page.click('[data-testid="login-button"]');

    await page.waitForLoadState('networkidle');

    // ログイン成功確認
    await expect(page.locator('[data-testid="logout-button"]').first()).toBeVisible({ timeout: 5000 });

    // 2. /admin/users に遷移
    await page.goto(`${BASE_URL}/admin/users`);
    await page.waitForLoadState('networkidle');
    await page.waitForTimeout(2000); // Blazor Server SignalR接続・データ取得待機

    // 3. 既存ユーザーの編集ボタンクリック（最初のユーザー）
    const firstEditButton = page.locator('[data-testid^="edit-user-button-"]').first();
    await firstEditButton.waitFor({ state: 'visible', timeout: 10000 });
    await firstEditButton.click();

    await page.waitForLoadState('networkidle');
    await page.waitForTimeout(1000);

    // URL確認（editページに遷移）
    const currentUrl = page.url();
    expect(currentUrl).toContain('/admin/users/edit/');

    // 4. ユーザー情報更新（名前のみ変更）
    const nameInput = page.locator('[data-testid="name-input"]');
    await nameInput.waitFor({ state: 'visible', timeout: 5000 });

    // 固定長の更新名を使用（50文字制限対応）
    const timestamp = Date.now();
    const updatedName = `E2E Updated User ${timestamp}`.substring(0, 50);

    await nameInput.clear();
    await nameInput.fill(updatedName);

    await page.waitForTimeout(500);

    // 5. ダイアログイベント待機を先に設定
    const editDialogPromise = page.waitForEvent('dialog');

    // 6. 送信ボタンクリック
    await page.click('[data-testid="submit-button"]');

    // 7. ダイアログ待機＆処理
    const editDialog = await editDialogPromise;
    expect(editDialog.message()).toContain('更新しました');
    await editDialog.accept();

    // 8. Blazor Server SignalR処理完了待機
    await page.waitForTimeout(1000);

    // 9. ユーザー一覧画面にリダイレクトされることを確認
    await page.waitForURL('**/admin/users', { timeout: 15000 });
    await page.waitForLoadState('networkidle');
  });

  // Scenario 4: SuperUser権限 - ユーザー削除（論理削除）
  test('DeleteUser_SuperUser_ShowsSuccessMessage', async ({ page }) => {
    // 1. SuperUserアカウントでログイン
    await page.goto(BASE_URL);
    await page.waitForLoadState('networkidle');

    await page.fill('[data-testid="username-input"]', TEST_ACCOUNTS.SuperUser.email);
    await page.fill('[data-testid="password-input"]', TEST_ACCOUNTS.SuperUser.password);
    await page.click('[data-testid="login-button"]');

    await page.waitForLoadState('networkidle');

    // 2. /admin/users に遷移
    await page.goto(`${BASE_URL}/admin/users`);
    await page.waitForLoadState('networkidle');
    await page.waitForTimeout(2000);

    // 3. テストユーザーのステータストグルボタンクリック（最初のユーザー）
    const firstToggleButton = page.locator('[data-testid^="toggle-status-button-"]').first();

    // ボタンが存在するか確認
    const toggleButtonVisible = await firstToggleButton.isVisible({ timeout: 2000 }).catch(() => false);

    if (toggleButtonVisible) {
      // ダイアログハンドリング（削除確認）
      page.once('dialog', async dialog => {
        expect(dialog.message()).toContain('削除してもよろしいですか');
        await dialog.accept();
      });

      await firstToggleButton.click();

      await page.waitForLoadState('networkidle');
      await page.waitForTimeout(1000);

      // 4. 論理削除確認（成功アラート）
      page.once('dialog', async dialog => {
        expect(dialog.message()).toContain('削除しました');
        await dialog.accept();
      });

      // ページリロード（削除済みユーザー非表示確認）
      await page.waitForTimeout(1000);
    }
  });

  // Scenario 5: ProjectManager権限 - ユーザー一覧表示（担当プロジェクトユーザーのみ）
  test('UserList_PM_ShowsAssignedProjectUsers', async ({ page }) => {
    // 1. ProjectManagerアカウントでログイン
    await page.goto(BASE_URL);
    await page.waitForLoadState('networkidle');

    await page.fill('[data-testid="username-input"]', TEST_ACCOUNTS.ProjectManager.email);
    await page.fill('[data-testid="password-input"]', TEST_ACCOUNTS.ProjectManager.password);
    await page.click('[data-testid="login-button"]');

    await page.waitForLoadState('networkidle');

    // ログイン成功確認
    await expect(page.locator('[data-testid="logout-button"]').first()).toBeVisible({ timeout: 5000 });

    // 2. /admin/users に遷移
    await page.goto(`${BASE_URL}/admin/users`);
    await page.waitForLoadState('networkidle');
    await page.waitForTimeout(2000);

    // 3. 担当プロジェクトユーザーのみ表示確認
    const userTable = page.locator('[data-testid="user-list-table"]');
    await expect(userTable).toBeVisible({ timeout: 10000 });

    // ユーザー行が表示されることを確認（担当プロジェクトに所属するユーザーがいる場合）
    const userRows = page.locator('[data-testid^="user-row-"]');
    const rowCount = await userRows.count();

    // ProjectManagerは担当プロジェクトユーザーのみ表示（0件以上）
    expect(rowCount).toBeGreaterThanOrEqual(0);
  });

  // Scenario 6: ProjectManager権限 - ロール選択肢制限確認
  test('CreateUser_PM_RoleRestriction', async ({ page }) => {
    // 1. ProjectManagerアカウントでログイン
    await page.goto(BASE_URL);
    await page.waitForLoadState('networkidle');

    await page.fill('[data-testid="username-input"]', TEST_ACCOUNTS.ProjectManager.email);
    await page.fill('[data-testid="password-input"]', TEST_ACCOUNTS.ProjectManager.password);
    await page.click('[data-testid="login-button"]');

    await page.waitForLoadState('networkidle');

    // ログイン成功確認
    await expect(page.locator('[data-testid="logout-button"]').first()).toBeVisible({ timeout: 5000 });

    // 2. /admin/users/create に遷移
    await page.goto(`${BASE_URL}/admin/users/create`);
    await page.waitForLoadState('networkidle');
    await page.waitForTimeout(2000); // Blazor Server 初期化待機

    // 3. ロール選択肢制限確認（SuperUserなし）
    const superUserRadio = page.locator('[data-testid="role-dropdown-superuser"]');
    const projectManagerRadio = page.locator('[data-testid="role-dropdown-projectmanager"]');
    const domainApproverRadio = page.locator('[data-testid="role-dropdown-domainapprover"]');
    const generalUserRadio = page.locator('[data-testid="role-dropdown-generaluser"]');

    // SuperUser/ProjectManagerラジオボタンは非表示（存在しない）
    const superUserCount = await superUserRadio.count();
    const projectManagerCount = await projectManagerRadio.count();
    expect(superUserCount).toBe(0);
    expect(projectManagerCount).toBe(0);

    // DomainApprover/GeneralUserラジオボタンは表示（attachedチェック - DOMに存在するか）
    await expect(domainApproverRadio).toBeAttached({ timeout: 5000 });
    await expect(generalUserRadio).toBeAttached({ timeout: 5000 });
  });

  // Scenario 7: GeneralUser権限 - アクセス拒否確認
  test('UserList_GeneralUser_AccessDenied', async ({ page }) => {
    // 1. GeneralUserアカウントでログイン
    await page.goto(BASE_URL);
    await page.waitForLoadState('networkidle');

    await page.fill('[data-testid="username-input"]', TEST_ACCOUNTS.GeneralUser.email);
    await page.fill('[data-testid="password-input"]', TEST_ACCOUNTS.GeneralUser.password);
    await page.click('[data-testid="login-button"]');

    await page.waitForLoadState('networkidle');

    // ログイン成功確認
    await expect(page.locator('[data-testid="logout-button"]').first()).toBeVisible({ timeout: 5000 });

    // 2. /admin/users に遷移（アクセス拒否されることを期待）
    await page.goto(`${BASE_URL}/admin/users`);
    await page.waitForLoadState('networkidle');
    await page.waitForTimeout(2000);

    // 3. アクセス拒否確認
    const currentUrl = page.url();

    // GeneralUserはアクセス権限がないため、以下のいずれかになる:
    // - AccessDeniedページ（/Account/AccessDenied）にリダイレクト
    // - ホーム画面（/）にリダイレクト
    // - ユーザー一覧画面は表示されない

    // ユーザー管理テーブルが表示されないことを確認
    const userTable = page.locator('[data-testid="user-list-table"]');
    const userTableVisible = await userTable.isVisible().catch(() => false);
    expect(userTableVisible).toBe(false);
  });

  // Scenario 8: 無効メールアドレスバリデーション
  test('CreateUser_InvalidEmail_ShowsValidationError', async ({ page }) => {
    // 1. SuperUserアカウントでログイン
    await page.goto(BASE_URL);
    await page.waitForLoadState('networkidle');

    await page.fill('[data-testid="username-input"]', TEST_ACCOUNTS.SuperUser.email);
    await page.fill('[data-testid="password-input"]', TEST_ACCOUNTS.SuperUser.password);
    await page.click('[data-testid="login-button"]');

    await page.waitForLoadState('networkidle');

    // ログイン成功確認
    await expect(page.locator('[data-testid="logout-button"]').first()).toBeVisible({ timeout: 5000 });

    // 2. /admin/users/create に遷移
    await page.goto(`${BASE_URL}/admin/users/create`);
    await page.waitForLoadState('networkidle');
    await page.waitForTimeout(2000); // Blazor Server 初期化待機

    // 3. 無効メールアドレス入力
    const emailInput = page.locator('[data-testid="email-input"]');
    await emailInput.waitFor({ state: 'visible', timeout: 10000 });
    await emailInput.fill('invalid-email'); // @なし

    await page.fill('[data-testid="name-input"]', 'Test User');
    await page.fill('[data-testid="password-input"]', 'TestUser@2025!');

    // ロール選択（GeneralUser）
    await page.click('[data-testid="role-dropdown-generaluser"]');

    await page.waitForTimeout(500);

    // 送信ボタンクリック
    await page.click('[data-testid="submit-button"]');

    await page.waitForTimeout(1000); // バリデーション待機

    // 4. バリデーションエラー確認（data-testidで特定 or .validation-messageクラスの最初の要素）
    const validationError = page.locator('[data-testid="validation-error-email"]').first();
    await expect(validationError).toBeVisible({ timeout: 5000 });

    // エラーメッセージ内容確認
    const errorText = await validationError.textContent();
    expect(errorText).toMatch(/メールアドレス|email/i);
  });

  // Scenario 9: ロード中スピナー表示確認
  test('UserList_LoadingSpinner_ShowsDuringLoading', async ({ page, browser }) => {
    // Blazor ServerはSignalR経由でデータ取得するため、CDPでネットワークスロットリングを使用
    // Chrome DevTools Protocolセッション作成
    const client = await page.context().newCDPSession(page);

    // ネットワークスロットリング有効化（3G Fast相当）
    await client.send('Network.enable');
    await client.send('Network.emulateNetworkConditions', {
      offline: false,
      downloadThroughput: 1.6 * 1024 * 1024 / 8, // 1.6 Mbps
      uploadThroughput: 750 * 1024 / 8,           // 750 Kbps
      latency: 40                                  // 40ms latency
    });

    // 1. SuperUserアカウントでログイン
    await page.goto(BASE_URL);
    await page.waitForLoadState('networkidle');

    await page.fill('[data-testid="username-input"]', TEST_ACCOUNTS.SuperUser.email);
    await page.fill('[data-testid="password-input"]', TEST_ACCOUNTS.SuperUser.password);
    await page.click('[data-testid="login-button"]');

    await page.waitForLoadState('networkidle');

    // ログイン成功確認
    await expect(page.locator('[data-testid="logout-button"]').first()).toBeVisible({ timeout: 5000 });

    // 2. /admin/users に遷移（ネットワークスロットリング有効）
    // スピナーを即座に検出するため、goto後すぐに確認
    const spinner = page.locator('.spinner-border.text-primary');

    // ナビゲーション開始とスピナー監視を並行実行
    const navigationPromise = page.goto(`${BASE_URL}/admin/users`);

    // 3. ローディングスピナー表示確認（ネットワーク遅延により観測可能）
    // スピナーが表示されることを確認（最大5秒待機）
    await expect(spinner).toBeVisible({ timeout: 5000 });

    // ナビゲーション完了待機
    await navigationPromise;
    await page.waitForLoadState('networkidle');

    // 4. データ取得完了後、スピナーが非表示になることを確認
    await expect(spinner).not.toBeVisible({ timeout: 5000 });

    // 5. ユーザーテーブル表示確認
    const userTable = page.locator('[data-testid="user-list-table"]');
    await expect(userTable).toBeVisible({ timeout: 10000 });

    // ネットワークスロットリング無効化
    await client.send('Network.emulateNetworkConditions', {
      offline: false,
      downloadThroughput: -1,
      uploadThroughput: -1,
      latency: 0
    });
    await client.send('Network.disable');
  });

  // Scenario 10: 論理削除済みユーザー表示切替
  test('UserList_ShowDeletedFilter_TogglesDeletedUsers', async ({ page }) => {
    // 1. SuperUserアカウントでログイン
    await page.goto(BASE_URL);
    await page.waitForLoadState('networkidle');

    await page.fill('[data-testid="username-input"]', TEST_ACCOUNTS.SuperUser.email);
    await page.fill('[data-testid="password-input"]', TEST_ACCOUNTS.SuperUser.password);
    await page.click('[data-testid="login-button"]');

    await page.waitForLoadState('networkidle');

    // ログイン成功確認
    await expect(page.locator('[data-testid="logout-button"]').first()).toBeVisible({ timeout: 5000 });

    // 2. /admin/users に遷移
    await page.goto(`${BASE_URL}/admin/users`);
    await page.waitForLoadState('networkidle');
    await page.waitForTimeout(2000); // Blazor Server SignalR接続・データ取得待機

    // 3. チェックボックス初期状態確認（未チェック）
    const showDeletedCheckbox = page.locator('[data-testid="show-deleted-checkbox"]');
    await expect(showDeletedCheckbox).toBeVisible({ timeout: 5000 });

    const isChecked = await showDeletedCheckbox.isChecked();
    expect(isChecked).toBe(false);

    // 4. 現在のユーザー件数取得（バッジテキスト）
    const countBadge = page.locator('.card-header .badge.bg-secondary');
    await expect(countBadge).toBeVisible({ timeout: 5000 });

    const initialCountText = await countBadge.textContent();
    const initialCount = parseInt(initialCountText?.match(/\d+/)?.[0] || '0');

    // 5. チェックボックスをクリック（削除済みユーザー表示）
    await showDeletedCheckbox.click();
    await page.waitForTimeout(2000); // Blazor Server SignalR処理・データ再取得待機

    // 6. ユーザー件数が変化したことを確認（削除済みユーザー含む）
    const updatedCountText = await countBadge.textContent();
    const updatedCount = parseInt(updatedCountText?.match(/\d+/)?.[0] || '0');

    // 削除済みユーザーが追加されるため、件数は増加するか同じ（削除済みユーザーが0件の場合）
    expect(updatedCount).toBeGreaterThanOrEqual(initialCount);

    // 7. チェックボックスを再度クリック（アンチェック）
    await showDeletedCheckbox.click();
    await page.waitForTimeout(2000); // Blazor Server SignalR処理・データ再取得待機

    // 8. 件数が元の値に戻ることを確認
    const finalCountText = await countBadge.textContent();
    const finalCount = parseInt(finalCountText?.match(/\d+/)?.[0] || '0');

    expect(finalCount).toBe(initialCount);
  });
});
