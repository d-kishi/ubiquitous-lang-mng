using Microsoft.Playwright;
using Xunit;

namespace UbiquitousLanguageManager.E2E.Tests;

/// <summary>
/// Phase A Authentication Feature E2E Tests
///
/// Test Scenarios: 9 scenarios (6 implemented + 3 skipped)
/// - 3 Positive scenarios: Login success, Logout success, Change password success
/// - 3 Negative scenarios: Empty fields, Invalid credentials, Wrong password
/// - 3 Skipped scenarios: Password reset (未実装機能)
///
/// Technical Requirements:
/// - ViewportSize: 1920x1080 (Full HD)
/// - data-testid selectors
/// - Blazor Server SignalR対応
/// </summary>
public class AuthenticationTests : IAsyncLifetime
{
    private IPlaywright? _playwright;
    private IBrowser? _browser;
    private const string BaseUrl = "https://localhost:5001";
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
    /// Scenario 1: 正常系 - ログイン成功
    ///
    /// 実装ステップ:
    /// 1. トップページにアクセス
    /// 2. メールアドレス・パスワード入力
    /// 3. ログインボタンクリック
    /// 4. Blazor Server SignalR接続確立待機
    /// 5. ログイン成功確認（ログアウトボタン表示）
    /// 6. ViewportSize 1920x1080でNavMenu折りたたみなし確認
    /// </summary>
    [Fact]
    public async Task Login_ValidCredentials_ShowsHomePage()
    {
        // Arrange
        var page = await _browser!.NewPageAsync(new BrowserNewPageOptions
        {
            IgnoreHTTPSErrors = true, // 自己署名証明書対応（開発環境）
            ViewportSize = new ViewportSize { Width = 1920, Height = 1080 } // Full HD
        });

        try
        {
            // Act & Assert

            await page.GotoAsync(BaseUrl);
            await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            // data-testid属性でセレクタ指定
            await page.FillAsync("[data-testid='username-input']", TestEmail);
            await page.FillAsync("[data-testid='password-input']", TestPassword);
            await page.ClickAsync("[data-testid='login-button']");

            // Blazor Server SignalR接続確立待機
            await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            // ログイン成功確認（NavMenu表示・ログアウトボタン表示）
            // 注: logout-buttonが2箇所に存在するため.Firstで最初の要素を選択
            // （NavMenu.razor と AuthDisplay.razor に重複 - 将来的に解消予定）
            var logoutButton = page.Locator("[data-testid='logout-button']").First;
            await logoutButton.WaitForAsync(new LocatorWaitForOptions
            {
                State = WaitForSelectorState.Visible,
                Timeout = 5000
            });

            // ViewportSize 1920x1080でNavMenu折りたたみなし確認
            // 実際のクラス名は .nav-scrollable（.navbar-collapseではない）
            var navMenu = page.Locator(".nav-scrollable");
            await Assertions.Expect(navMenu).ToBeVisibleAsync();
        }
        finally
        {
            await page.CloseAsync();
        }
    }

    /// <summary>
    /// Scenario 2: 正常系 - ログアウト成功
    ///
    /// 実装ステップ:
    /// 1. ログイン（前提条件）
    /// 2. ログアウトボタンクリック
    /// 3. Blazor Server SignalR処理待機
    /// 4. ログインフォーム表示確認
    /// </summary>
    [Fact]
    public async Task Logout_AfterLogin_RedirectsToLoginPage()
    {
        // Arrange
        var page = await _browser!.NewPageAsync(new BrowserNewPageOptions
        {
            IgnoreHTTPSErrors = true,
            ViewportSize = new ViewportSize { Width = 1920, Height = 1080 }
        });

        try
        {
            // Act & Assert

            // 前提条件: ログイン
            await page.GotoAsync(BaseUrl);
            await page.FillAsync("[data-testid='username-input']", TestEmail);
            await page.FillAsync("[data-testid='password-input']", TestPassword);
            await page.ClickAsync("[data-testid='login-button']");
            await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            // ログアウトボタンクリック
            var logoutButton = page.Locator("[data-testid='logout-button']").First;
            await logoutButton.WaitForAsync(new LocatorWaitForOptions
            {
                State = WaitForSelectorState.Visible,
                Timeout = 5000
            });
            await logoutButton.ClickAsync();

            // Blazor Server SignalR処理待機
            await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
            await page.WaitForTimeoutAsync(1000);

            // ログインフォーム表示確認
            var usernameInput = page.Locator("[data-testid='username-input']");
            var passwordInput = page.Locator("[data-testid='password-input']");

            await usernameInput.WaitForAsync(new LocatorWaitForOptions
            {
                State = WaitForSelectorState.Visible,
                Timeout = 5000
            });
            await Assertions.Expect(passwordInput).ToBeVisibleAsync();
        }
        finally
        {
            await page.CloseAsync();
        }
    }

    /// <summary>
    /// Scenario 3: 異常系 - メールアドレス・パスワード未入力
    ///
    /// 実装ステップ:
    /// 1. トップページにアクセス
    /// 2. 空のまま送信
    /// 3. バリデーションエラー表示確認
    /// </summary>
    [Fact]
    public async Task Login_EmptyFields_ShowsValidationErrors()
    {
        // Arrange
        var page = await _browser!.NewPageAsync(new BrowserNewPageOptions
        {
            IgnoreHTTPSErrors = true,
            ViewportSize = new ViewportSize { Width = 1920, Height = 1080 }
        });

        try
        {
            // Act & Assert

            await page.GotoAsync(BaseUrl);
            await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            // 空のまま送信
            await page.ClickAsync("[data-testid='login-button']");
            await page.WaitForTimeoutAsync(500);

            // バリデーションエラー表示確認
            // 実際のクラス名は .text-danger.small（.validation-messageではない）
            var validationErrors = page.Locator(".text-danger.small");
            var errorCount = await validationErrors.CountAsync();
            Assert.True(errorCount > 0, "バリデーションエラーが表示されるはず");
        }
        finally
        {
            await page.CloseAsync();
        }
    }

    /// <summary>
    /// Scenario 4: 異常系 - 存在しないユーザー・パスワード不一致
    ///
    /// 実装ステップ:
    /// 1. トップページにアクセス
    /// 2. 誤った認証情報入力
    /// 3. ログインボタンクリック
    /// 4. エラーメッセージ表示確認（API応答待機）
    /// </summary>
    [Fact]
    public async Task Login_InvalidCredentials_ShowsErrorMessage()
    {
        // Arrange
        var page = await _browser!.NewPageAsync(new BrowserNewPageOptions
        {
            IgnoreHTTPSErrors = true,
            ViewportSize = new ViewportSize { Width = 1920, Height = 1080 }
        });

        try
        {
            // Act & Assert

            await page.GotoAsync(BaseUrl);
            await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            await page.FillAsync("[data-testid='username-input']", "invalid@example.com");
            await page.FillAsync("[data-testid='password-input']", "WrongPassword!");
            await page.ClickAsync("[data-testid='login-button']");

            // エラーメッセージ表示確認（API応答待機）
            var errorLocator = page.Locator(".alert-danger, [role='alert']");
            await errorLocator.WaitForAsync(new LocatorWaitForOptions
            {
                State = WaitForSelectorState.Visible,
                Timeout = 10000
            });

            var errorText = await errorLocator.TextContentAsync();
            Assert.Contains("メールアドレスまたはパスワードが正しくありません", errorText);
        }
        finally
        {
            await page.CloseAsync();
        }
    }

    /// <summary>
    /// Scenario 5: 正常系 - パスワード変更成功
    ///
    /// 実装ステップ:
    /// 1. ログイン（前提条件）
    /// 2. パスワード変更画面遷移
    /// 3. URL確認（リダイレクトされていないか）
    /// 4. パスワード変更フォーム入力
    /// 5. 成功メッセージ表示確認
    /// 6. パスワードを元に戻す（テストデータ整合性維持）
    /// </summary>
    [Fact]
    public async Task ChangePassword_ValidInput_ShowsSuccessMessage()
    {
        // Arrange
        var page = await _browser!.NewPageAsync(new BrowserNewPageOptions
        {
            IgnoreHTTPSErrors = true,
            ViewportSize = new ViewportSize { Width = 1920, Height = 1080 }
        });

        try
        {
            // Act & Assert

            // 前提条件: ログイン
            await page.GotoAsync(BaseUrl);
            await page.FillAsync("[data-testid='username-input']", TestEmail);
            await page.FillAsync("[data-testid='password-input']", TestPassword);
            await page.ClickAsync("[data-testid='login-button']");
            await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            // パスワード変更画面遷移
            await page.GotoAsync($"{BaseUrl}/change-password");
            await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
            await page.WaitForTimeoutAsync(1000); // Blazor Server SignalR接続完了待機

            // URL確認（リダイレクトされていないか）
            var currentUrl = page.Url;
            Assert.Contains("/change-password", currentUrl);

            // パスワード変更フォーム入力
            await page.FillAsync("#currentPassword", TestPassword);
            await page.FillAsync("#newPassword", "NewAdmin123!");
            await page.FillAsync("#confirmPassword", "NewAdmin123!");
            await page.ClickAsync("button[type='submit']");

            // 成功メッセージ表示確認
            await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
            var successMessage = page.Locator(".alert-success, [role='alert']");
            await successMessage.WaitForAsync(new LocatorWaitForOptions
            {
                State = WaitForSelectorState.Visible,
                Timeout = 5000
            });

            // パスワードを元に戻す（テストデータ整合性維持）
            // パスワード変更成功後はトップページ "/" にリダイレクトされるため、
            // 再度パスワード変更画面へ遷移してから元に戻す処理を実行
            await page.GotoAsync($"{BaseUrl}/change-password");
            await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
            await page.WaitForTimeoutAsync(1000); // Blazor Server SignalR接続完了待機

            await page.FillAsync("#currentPassword", "NewAdmin123!");
            await page.FillAsync("#newPassword", TestPassword);
            await page.FillAsync("#confirmPassword", TestPassword);
            await page.ClickAsync("button[type='submit']");
            await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        }
        finally
        {
            await page.CloseAsync();
        }
    }

    /// <summary>
    /// Scenario 6: 異常系 - 現在のパスワード不一致
    ///
    /// 実装ステップ:
    /// 1. ログイン（前提条件）
    /// 2. パスワード変更画面遷移
    /// 3. URL確認
    /// 4. 誤った現在のパスワード入力
    /// 5. エラーメッセージ表示確認
    /// </summary>
    [Fact]
    public async Task ChangePassword_WrongCurrentPassword_ShowsErrorMessage()
    {
        // Arrange
        var page = await _browser!.NewPageAsync(new BrowserNewPageOptions
        {
            IgnoreHTTPSErrors = true,
            ViewportSize = new ViewportSize { Width = 1920, Height = 1080 }
        });

        try
        {
            // Act & Assert

            // 前提条件: ログイン
            await page.GotoAsync(BaseUrl);
            await page.FillAsync("[data-testid='username-input']", TestEmail);
            await page.FillAsync("[data-testid='password-input']", TestPassword);
            await page.ClickAsync("[data-testid='login-button']");
            await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            // パスワード変更画面遷移
            await page.GotoAsync($"{BaseUrl}/change-password");
            await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
            await page.WaitForTimeoutAsync(1000);

            // URL確認
            var currentUrl = page.Url;
            Assert.Contains("/change-password", currentUrl);

            // 誤った現在のパスワード入力
            await page.FillAsync("#currentPassword", "WrongPassword!");
            await page.FillAsync("#newPassword", "NewAdmin123!");
            await page.FillAsync("#confirmPassword", "NewAdmin123!");
            await page.ClickAsync("button[type='submit']");

            // エラーメッセージ表示確認
            var errorMessage = page.Locator(".alert-danger, [role='alert']");
            await errorMessage.WaitForAsync(new LocatorWaitForOptions
            {
                State = WaitForSelectorState.Visible,
                Timeout = 5000
            });
        }
        finally
        {
            await page.CloseAsync();
        }
    }

    /// <summary>
    /// Scenario 7: Skipped - パスワードリセット機能未実装
    /// ForgotPassword.razorページ未実装
    /// </summary>
    [Fact(Skip = "ForgotPassword.razorページ未実装")]
    public async Task PasswordReset_ValidEmail_ShowsSuccessMessage()
    {
        // ForgotPassword.razorページ未実装
        await Task.CompletedTask;
    }

    /// <summary>
    /// Scenario 8: Skipped - パスワードリセット機能未実装
    /// ResetPassword.razorページ未実装
    /// </summary>
    [Fact(Skip = "ResetPassword.razorページ未実装")]
    public async Task PasswordReset_ValidToken_ShowsSuccessMessage()
    {
        // ResetPassword.razorページ未実装
        await Task.CompletedTask;
    }

    /// <summary>
    /// Scenario 9: Skipped - パスワードリセット機能未実装
    /// ResetPassword.razorページ未実装
    /// </summary>
    [Fact(Skip = "ResetPassword.razorページ未実装")]
    public async Task PasswordReset_InvalidToken_ShowsErrorMessage()
    {
        // ResetPassword.razorページ未実装
        await Task.CompletedTask;
    }
}
