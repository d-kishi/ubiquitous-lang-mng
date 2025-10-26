using Microsoft.Playwright;
using Xunit;

namespace UbiquitousLanguageManager.E2E.Tests;

/// <summary>
/// UserProjects機能のE2Eテスト
/// GitHub Issue #56対応: bUnit技術的課題のE2E代替実装
/// </summary>
public class UserProjectsTests : IAsyncLifetime
{
    private IPlaywright? _playwright;
    private IBrowser? _browser;
    private const string BaseUrl = "https://localhost:5001";

    // テスト専用アカウント（Phase B2 Step6 Stage 0で定義）
    private const string TestEmail = "e2e-test@ubiquitous-lang.local";
    private const string TestPassword = "E2ETest#2025!Secure";

    public async Task InitializeAsync()
    {
        // Playwright初期化
        _playwright = await Playwright.CreateAsync();

        // Chromiumブラウザ起動（ヘッドレスモード）
        _browser = await _playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
        {
            Headless = true, // CI/CD環境対応
            SlowMo = 100 // デバッグ時の視認性向上（ミリ秒）
        });
    }

    public async Task DisposeAsync()
    {
        // リソース解放
        if (_browser != null)
        {
            await _browser.CloseAsync();
        }
        _playwright?.Dispose();
    }

    /// <summary>
    /// シナリオ1: メンバー追加E2Eテスト
    ///
    /// 実装ステップ:
    /// 1. ログイン
    /// 2. プロジェクト一覧画面で「メンバー」ボタンをクリック
    /// 3. メンバー管理画面遷移確認
    /// 4. ユーザー選択ドロップダウン操作
    /// 5. 「追加」ボタンをクリック
    /// 6. 成功メッセージ検証
    /// 7. メンバー一覧自動更新確認
    ///
    /// GitHub Issue #56対応:
    /// - EditForm送信ロジック（bUnitでは困難）
    /// - 子コンポーネント連携（ProjectMemberSelector/ProjectMemberCard）
    /// - Blazor Server SignalR接続・StateHasChanged動作確認
    /// </summary>
    [Fact]
    public async Task ProjectMembers_AddMember_ShowsSuccessMessage()
    {
        // Arrange
        var page = await _browser!.NewPageAsync(new BrowserNewPageOptions
        {
            IgnoreHTTPSErrors = true // 自己署名証明書対応（開発環境）
        });

        try
        {
            // Act & Assert

            // Step 1: ログイン
            await page.GotoAsync(BaseUrl);
            await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            // data-testid属性でセレクタ指定（Phase B2 Step5で実装）
            await page.FillAsync("[data-testid='username-input']", TestEmail);
            await page.FillAsync("[data-testid='password-input']", TestPassword);
            await page.ClickAsync("[data-testid='login-button']");

            // Blazor Server SignalR接続確立待機
            await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            // Step 2-3: メンバー管理画面遷移
            // プロジェクト一覧から最初のプロジェクトのメンバー管理リンクをクリック
            var memberLink = page.Locator("[data-testid='member-management-link']").First;
            await memberLink.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
            await memberLink.ClickAsync();

            // メンバー管理画面遷移確認
            await page.WaitForURLAsync("**/projects/*/members");

            // Step 4: ユーザー選択ドロップダウン操作
            var memberSelector = page.Locator("[data-testid='member-selector']");
            await memberSelector.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });

            // ドロップダウンから最初のユーザーを選択
            await memberSelector.SelectOptionAsync(new SelectOptionValue { Index = 1 });

            // Step 5: 「追加」ボタンをクリック
            await page.ClickAsync("[data-testid='member-add-button']");

            // Step 6: 成功メッセージ検証
            // Toast通知表示待機（最大5秒）
            var toastLocator = page.Locator(".toast-success, [role='alert']");
            await toastLocator.WaitForAsync(new LocatorWaitForOptions
            {
                State = WaitForSelectorState.Visible,
                Timeout = 5000
            });

            var toastText = await toastLocator.TextContentAsync();
            Assert.Contains("プロジェクトに追加しました", toastText);

            // Step 7: メンバー一覧自動更新確認
            // StateHasChanged()による非同期UI更新待機
            await page.WaitForTimeoutAsync(1000); // SignalR更新待機

            var memberList = page.Locator("[data-testid='member-list']");
            var memberCount = await memberList.Locator("[data-testid='member-card']").CountAsync();
            Assert.True(memberCount > 0, "メンバー一覧に追加されたメンバーが表示されるはず");
        }
        finally
        {
            await page.CloseAsync();
        }
    }

    /// <summary>
    /// シナリオ2: メンバー削除E2Eテスト
    ///
    /// 実装ステップ:
    /// 1. ログイン・メンバー管理画面遷移
    /// 2. 既存メンバーの「削除」ボタンをクリック
    /// 3. 削除確認ダイアログ検証（JavaScript confirm）
    /// 4. 「確認」ボタンをクリック
    /// 5. 成功メッセージ検証
    /// 6. メンバー一覧自動更新確認
    ///
    /// GitHub Issue #56対応:
    /// - JavaScript confirmダイアログ処理（bUnitでは困難）
    /// - StateHasChanged()による非同期削除確認
    /// </summary>
    [Fact]
    public async Task ProjectMembers_RemoveMember_ShowsSuccessMessage()
    {
        // Arrange
        var page = await _browser!.NewPageAsync(new BrowserNewPageOptions
        {
            IgnoreHTTPSErrors = true
        });

        try
        {
            // Act & Assert

            // Step 1: ログイン・メンバー管理画面遷移（シナリオ1と同様）
            await page.GotoAsync(BaseUrl);
            await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            await page.FillAsync("[data-testid='username-input']", TestEmail);
            await page.FillAsync("[data-testid='password-input']", TestPassword);
            await page.ClickAsync("[data-testid='login-button']");
            await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            var memberLink = page.Locator("[data-testid='member-management-link']").First;
            await memberLink.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
            await memberLink.ClickAsync();
            await page.WaitForURLAsync("**/projects/*/members");

            // 削除前のメンバー数記録
            var memberList = page.Locator("[data-testid='member-list']");
            var initialMemberCount = await memberList.Locator("[data-testid='member-card']").CountAsync();

            // Step 2: 既存メンバーの「削除」ボタンをクリック
            // JavaScript confirmダイアログを自動承認
            page.Dialog += async (_, dialog) =>
            {
                Assert.Equal("confirm", dialog.Type);
                Assert.Contains("削除", dialog.Message);
                await dialog.AcceptAsync();
            };

            var deleteButton = page.Locator("[data-testid='member-delete-button']").First;
            await deleteButton.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
            await deleteButton.ClickAsync();

            // Step 5: 成功メッセージ検証
            var toastLocator = page.Locator(".toast-success, [role='alert']");
            await toastLocator.WaitForAsync(new LocatorWaitForOptions
            {
                State = WaitForSelectorState.Visible,
                Timeout = 5000
            });

            var toastText = await toastLocator.TextContentAsync();
            Assert.Contains("削除しました", toastText);

            // Step 6: メンバー一覧自動更新確認
            await page.WaitForTimeoutAsync(1000); // SignalR更新待機

            var finalMemberCount = await memberList.Locator("[data-testid='member-card']").CountAsync();
            Assert.True(finalMemberCount < initialMemberCount, "メンバー一覧から削除されたメンバーが消えているはず");
        }
        finally
        {
            await page.CloseAsync();
        }
    }

    /// <summary>
    /// シナリオ3: エラーハンドリングE2Eテスト（重複追加）
    ///
    /// 実装ステップ:
    /// 1. ログイン・メンバー管理画面遷移
    /// 2. 既に追加済みのメンバーを再度追加しようとする
    /// 3. エラーメッセージ検証
    ///
    /// GitHub Issue #56対応:
    /// - エラーハンドリングフロー検証（bUnitでは困難）
    /// - data-testid="member-error-message"要素の動的表示確認
    /// </summary>
    [Fact]
    public async Task ProjectMembers_AddDuplicateMember_ShowsErrorMessage()
    {
        // Arrange
        var page = await _browser!.NewPageAsync(new BrowserNewPageOptions
        {
            IgnoreHTTPSErrors = true
        });

        try
        {
            // Act & Assert

            // Step 1: ログイン・メンバー管理画面遷移
            await page.GotoAsync(BaseUrl);
            await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            await page.FillAsync("[data-testid='username-input']", TestEmail);
            await page.FillAsync("[data-testid='password-input']", TestPassword);
            await page.ClickAsync("[data-testid='login-button']");
            await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            var memberLink = page.Locator("[data-testid='member-management-link']").First;
            await memberLink.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
            await memberLink.ClickAsync();
            await page.WaitForURLAsync("**/projects/*/members");

            // Step 2: 既存メンバーと同じユーザーを選択
            var memberSelector = page.Locator("[data-testid='member-selector']");
            await memberSelector.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });

            // 既存メンバーのユーザーIDを取得（最初のメンバーカードから）
            var firstMemberCard = page.Locator("[data-testid='member-card']").First;
            var memberName = await firstMemberCard.Locator("[data-testid='member-name']").TextContentAsync();

            // 同じユーザーを選択して追加を試みる
            await memberSelector.SelectOptionAsync(new SelectOptionValue { Label = memberName });
            await page.ClickAsync("[data-testid='member-add-button']");

            // Step 3: エラーメッセージ検証
            var errorLocator = page.Locator("[data-testid='member-error-message']");
            await errorLocator.WaitForAsync(new LocatorWaitForOptions
            {
                State = WaitForSelectorState.Visible,
                Timeout = 5000
            });

            var errorText = await errorLocator.TextContentAsync();
            Assert.Contains("既にこのプロジェクトのメンバーです", errorText);
        }
        finally
        {
            await page.CloseAsync();
        }
    }
}
