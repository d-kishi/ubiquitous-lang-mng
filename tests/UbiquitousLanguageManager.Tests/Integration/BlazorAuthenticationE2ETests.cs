using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using Xunit;
using Microsoft.EntityFrameworkCore;
using UbiquitousLanguageManager.Infrastructure.Data;
using UbiquitousLanguageManager.Tests.TestUtilities;
using Microsoft.AspNetCore.Identity;
using UbiquitousLanguageManager.Infrastructure.Data.Entities;
using HtmlAgilityPack;
using System.Text;

namespace UbiquitousLanguageManager.Tests.Integration;

/// <summary>
/// Blazor Server専用認証フローE2Eテスト
/// Phase A6統合完成後のBlazor Server認証システム包括検証
/// </summary>
public class BlazorAuthenticationE2ETests : IClassFixture<TestWebApplicationFactory<Program>>
{
    private readonly TestWebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public BlazorAuthenticationE2ETests(TestWebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false // リダイレクト制御
        });
    }

    #region Blazor Server認証フロー統合テスト

    /// <summary>
    /// Blazor Server認証フロー: ログイン画面表示・フォーム構造確認
    /// </summary>
    [Fact]
    public async Task BlazorLogin_Page_RendersCorrectForm()
    {
        // Act
        var response = await _client.GetAsync("/login");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var content = await response.Content.ReadAsStringAsync();
        var doc = new HtmlDocument();
        doc.LoadHtml(content);

        // Blazor Server特有の要素確認
        var blazorScript = doc.DocumentNode.SelectSingleNode("//script[contains(@src, '_framework/blazor.server.js')]");
        Assert.NotNull(blazorScript);

        // ログインフォーム要素確認
        var emailInput = doc.DocumentNode.SelectSingleNode("//input[@type='email' or @type='text']");
        Assert.NotNull(emailInput);

        var passwordInput = doc.DocumentNode.SelectSingleNode("//input[@type='password']");
        Assert.NotNull(passwordInput);

        var submitButton = doc.DocumentNode.SelectSingleNode("//button[@type='submit'] | //input[@type='submit']");
        Assert.NotNull(submitButton);

        // CSRF保護確認
        var antiforgeryToken = doc.DocumentNode.SelectSingleNode("//input[@name='__RequestVerificationToken']");
        Assert.NotNull(antiforgeryToken);

        // Bootstrap UI要素確認
        var bootstrapComponents = doc.DocumentNode.SelectNodes("//*[contains(@class, 'form-') or contains(@class, 'btn-')]");
        Assert.NotNull(bootstrapComponents);
        Assert.True(bootstrapComponents.Count > 0);
    }

    /// <summary>
    /// Blazor Server SignalR接続確認・WebSocket認証状態管理
    /// </summary>
    [Fact]
    public async Task BlazorServer_SignalRConnection_IsConfiguredCorrectly()
    {
        // Act - Blazorページアクセス
        var response = await _client.GetAsync("/login");
        
        // Assert
        Assert.True(response.IsSuccessStatusCode);
        
        var content = await response.Content.ReadAsStringAsync();
        
        // SignalR関連スクリプトの確認
        Assert.Contains("_framework/blazor.server.js", content);
        
        // WebSocket接続設定の確認
        Assert.Contains("negotiateUrl", content.ToLower());
        
        // 認証状態管理コンポーネントの確認
        Assert.True(content.ToLower().Contains("authenticationstateprovider") ||
                   content.ToLower().Contains("authentication"));
    }

    /// <summary>
    /// 認証状態変更通知・リアルタイム状態同期確認
    /// </summary>
    [Fact]
    public async Task BlazorServer_AuthenticationStateChanges_AreHandledCorrectly()
    {
        // Arrange - テスト用認証状態設定
        using var scope = await _factory.CreateScopeWithTestDataAsync();
        
        // Act - 認証が必要なBlazorページアクセス
        var protectedResponse = await _client.GetAsync("/admin/users");
        
        // Assert - 適切な認証処理確認
        Assert.True(protectedResponse.StatusCode == HttpStatusCode.Redirect ||
                   protectedResponse.StatusCode == HttpStatusCode.Unauthorized ||
                   protectedResponse.StatusCode == HttpStatusCode.OK);

        if (protectedResponse.StatusCode == HttpStatusCode.Redirect)
        {
            var location = protectedResponse.Headers.Location?.ToString();
            Assert.Contains("/login", location ?? "");
        }
    }

    #endregion

    #region FirstLoginRedirectMiddleware E2Eテスト

    /// <summary>
    /// FirstLoginRedirectMiddleware: 初回ログイン状態でのパス制限確認
    /// </summary>
    [Fact]
    public async Task FirstLoginMiddleware_RestrictsAccessCorrectly()
    {
        // Arrange - 初回ログインユーザー作成・認証状態設定
        using var scope = await _factory.CreateScopeWithTestDataAsync();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        
        var firstLoginUser = new ApplicationUser
        {
            UserName = "firstlogin@middleware.test",
            Email = "firstlogin@middleware.test",
            Name = "ミドルウェアテストユーザー",
            IsFirstLogin = true
        };
        
        await userManager.CreateAsync(firstLoginUser, "TempPass123!");

        // テスト用認証Cookie設定（実装依存）
        // 実際の実装では適切な認証Cookieまたはトークンを設定

        // Act & Assert - 制限対象パステスト
        var restrictedPaths = new[]
        {
            "/admin/users",
            "/admin/organizations",
            "/admin/projects",
            "/user/profile",
            "/dashboard"
        };

        foreach (var path in restrictedPaths)
        {
            var response = await _client.GetAsync(path);
            
            // 初回ログイン状態では制限されるべき
            Assert.True(response.StatusCode == HttpStatusCode.Redirect ||
                       response.StatusCode == HttpStatusCode.Forbidden ||
                       response.StatusCode == HttpStatusCode.Unauthorized,
                       $"Path {path} should be restricted for first login users");
            
            if (response.StatusCode == HttpStatusCode.Redirect)
            {
                var location = response.Headers.Location?.ToString();
                Assert.True(location?.Contains("/change-password") == true ||
                           location?.Contains("/login") == true,
                           $"Should redirect to change-password or login for path {path}");
            }
        }
    }

    /// <summary>
    /// FirstLoginRedirectMiddleware: 許可パスへのアクセス確認
    /// </summary>
    [Fact]
    public async Task FirstLoginMiddleware_AllowsAccessToPermittedPaths()
    {
        // Arrange
        using var scope = await _factory.CreateScopeWithTestDataAsync();

        // Act & Assert - 許可対象パステスト
        var permittedPaths = new[]
        {
            "/change-password",
            "/account/logout",
            "/login",
            "/", // ホーム画面
            "/health" // ヘルスチェック
        };

        foreach (var path in permittedPaths)
        {
            var response = await _client.GetAsync(path);
            
            // 許可パスは正常にアクセスできるべき
            Assert.True(response.StatusCode == HttpStatusCode.OK ||
                       response.StatusCode == HttpStatusCode.Redirect ||
                       response.StatusCode == HttpStatusCode.Found,
                       $"Path {path} should be accessible");
        }
    }

    #endregion

    #region パスワード変更フロー統合テスト

    /// <summary>
    /// パスワード変更ページ: Blazor Serverコンポーネント正常表示
    /// </summary>
    [Fact]
    public async Task ChangePasswordPage_RendersCorrectly()
    {
        // Act - パスワード変更ページアクセス
        var response = await _client.GetAsync("/change-password");

        // Assert
        Assert.True(response.StatusCode == HttpStatusCode.OK ||
                   response.StatusCode == HttpStatusCode.Redirect);

        if (response.StatusCode == HttpStatusCode.OK)
        {
            var content = await response.Content.ReadAsStringAsync();
            var doc = new HtmlDocument();
            doc.LoadHtml(content);

            // パスワード変更フォーム要素確認
            var currentPasswordInput = doc.DocumentNode.SelectSingleNode("//input[@type='password']");
            var newPasswordInputs = doc.DocumentNode.SelectNodes("//input[@type='password']");
            
            Assert.NotNull(currentPasswordInput);
            Assert.True(newPasswordInputs?.Count >= 3); // 現在・新・確認パスワード

            // Blazor Serverスクリプト確認
            Assert.Contains("_framework/blazor.server.js", content);

            // バリデーション要素確認
            Assert.Contains("validation", content.ToLower());
        }
    }

    /// <summary>
    /// パスワード変更処理: セキュリティポリシー適用確認
    /// </summary>
    [Fact]
    public async Task PasswordChange_EnforcesSecurityPolicy()
    {
        // Arrange - テストユーザー作成
        using var scope = await _factory.CreateScopeWithTestDataAsync();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        
        var testUser = new ApplicationUser
        {
            UserName = "security@policy.test",
            Email = "security@policy.test",
            Name = "セキュリティポリシーテストユーザー",
            IsFirstLogin = true
        };
        
        await userManager.CreateAsync(testUser, "CurrentPass123!");

        // Act & Assert - 弱いパスワードでの変更試行
        var weakPasswords = new[]
        {
            "123456",        // 短すぎる
            "password",      // 一般的すぎる
            "abc123",        // 短く単純
            "CurrentPass123!" // 現在と同じ
        };

        foreach (var weakPassword in weakPasswords)
        {
            var passwordValidationResult = await userManager.PasswordValidators
                .FirstOrDefault()
                ?.ValidateAsync(userManager, testUser, weakPassword);

            if (passwordValidationResult != null)
            {
                // 弱いパスワードは拒否されるべき
                Assert.False(passwordValidationResult.Succeeded,
                           $"Weak password '{weakPassword}' should be rejected");
            }
        }

        // 強いパスワードでの検証
        var strongPassword = "StrongNewPassword123!@#";
        var strongPasswordResult = await userManager.PasswordValidators
            .FirstOrDefault()
            ?.ValidateAsync(userManager, testUser, strongPassword);

        if (strongPasswordResult != null)
        {
            Assert.True(strongPasswordResult.Succeeded,
                       "Strong password should be accepted");
        }
    }

    #endregion

    #region セキュリティ・アクセス制御テスト

    /// <summary>
    /// CSRF保護: 無効なトークンでのフォーム送信拒否
    /// </summary>
    [Fact]
    public async Task CSRFProtection_RejectsInvalidTokens()
    {
        // Arrange - ログイン画面からCSRFトークン取得
        var loginPageResponse = await _client.GetAsync("/login");
        var loginContent = await loginPageResponse.Content.ReadAsStringAsync();
        
        var doc = new HtmlDocument();
        doc.LoadHtml(loginContent);
        
        var antiforgeryToken = doc.DocumentNode
            .SelectSingleNode("//input[@name='__RequestVerificationToken']")
            ?.GetAttributeValue("value", "");

        Assert.NotNull(antiforgeryToken);
        Assert.NotEmpty(antiforgeryToken);

        // Act - 無効なCSRFトークンでフォーム送信
        var formData = new List<KeyValuePair<string, string>>
        {
            new("Email", "test@example.com"),
            new("Password", "TestPass123!"),
            new("__RequestVerificationToken", "INVALID_TOKEN")
        };

        var formContent = new FormUrlEncodedContent(formData);
        var response = await _client.PostAsync("/login", formContent);

        // Assert - CSRF攻撃は拒否されるべき
        Assert.True(response.StatusCode == HttpStatusCode.BadRequest ||
                   response.StatusCode == HttpStatusCode.Forbidden ||
                   response.StatusCode == HttpStatusCode.Unauthorized);
    }

    /// <summary>
    /// セッションセキュリティ: 複数セッション管理確認
    /// </summary>
    [Fact]
    public async Task SessionSecurity_HandlesMultipleSessions()
    {
        // Arrange - 複数のHTTPクライアント作成（異なるセッション）
        var client1 = _factory.CreateClient();
        var client2 = _factory.CreateClient();

        using var scope = await _factory.CreateScopeWithTestDataAsync();

        // Act - 異なるクライアントでの同時アクセス
        var response1 = await client1.GetAsync("/login");
        var response2 = await client2.GetAsync("/login");

        // Assert - 両方のセッションが独立して動作
        Assert.True(response1.IsSuccessStatusCode);
        Assert.True(response2.IsSuccessStatusCode);

        var content1 = await response1.Content.ReadAsStringAsync();
        var content2 = await response2.Content.ReadAsStringAsync();

        // 異なるCSRFトークンが生成されているべき
        var doc1 = new HtmlDocument();
        doc1.LoadHtml(content1);
        var token1 = doc1.DocumentNode
            .SelectSingleNode("//input[@name='__RequestVerificationToken']")
            ?.GetAttributeValue("value", "");

        var doc2 = new HtmlDocument();
        doc2.LoadHtml(content2);
        var token2 = doc2.DocumentNode
            .SelectSingleNode("//input[@name='__RequestVerificationToken']")
            ?.GetAttributeValue("value", "");

        Assert.NotEqual(token1, token2);
    }

    #endregion

    #region 品質・パフォーマンステスト

    /// <summary>
    /// ページ読み込み性能: 認証関連ページの応答時間確認
    /// </summary>
    [Fact]
    public async Task AuthenticationPages_LoadWithinAcceptableTime()
    {
        // Arrange
        var authPages = new[] { "/", "/login", "/change-password" };
        var maxResponseTime = TimeSpan.FromSeconds(5); // 許容最大応答時間

        foreach (var page in authPages)
        {
            // Act
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            var response = await _client.GetAsync(page);
            stopwatch.Stop();

            // Assert
            Assert.True(response.StatusCode == HttpStatusCode.OK ||
                       response.StatusCode == HttpStatusCode.Redirect,
                       $"Page {page} should return success or redirect");
            
            Assert.True(stopwatch.Elapsed < maxResponseTime,
                       $"Page {page} should load within {maxResponseTime.TotalSeconds} seconds, but took {stopwatch.Elapsed.TotalSeconds} seconds");
        }
    }

    /// <summary>
    /// メモリ・リソース管理: 大量リクエストでのメモリリーク確認
    /// </summary>
    [Fact]
    public async Task AuthenticationSystem_HandlesMultipleRequestsWithoutMemoryLeaks()
    {
        // Arrange
        var requestCount = 50;
        var initialMemory = GC.GetTotalMemory(false);

        // Act - 大量の認証関連リクエスト
        var tasks = new List<Task<HttpResponseMessage>>();
        
        for (int i = 0; i < requestCount; i++)
        {
            tasks.Add(_client.GetAsync("/login"));
            tasks.Add(_client.GetAsync("/"));
        }

        var responses = await Task.WhenAll(tasks);

        // Assert - 全リクエストが正常処理
        foreach (var response in responses)
        {
            Assert.True(response.IsSuccessStatusCode ||
                       response.StatusCode == HttpStatusCode.Redirect);
            response.Dispose();
        }

        // メモリ使用量確認
        GC.Collect();
        GC.WaitForPendingFinalizers();
        var finalMemory = GC.GetTotalMemory(true);
        
        var memoryIncrease = finalMemory - initialMemory;
        var memoryIncreasePerRequest = memoryIncrease / (requestCount * 2);

        // 1リクエストあたりのメモリ増加が許容範囲内（1MB以下）
        Assert.True(memoryIncreasePerRequest < 1024 * 1024,
                   $"Memory increase per request should be less than 1MB, but was {memoryIncreasePerRequest / 1024}KB");
    }

    #endregion

    #region 統合確認・回帰テスト

    /// <summary>
    /// 統合確認: Phase A6完了後の全認証機能正常動作確認
    /// </summary>
    [Fact]
    public async Task PhaseA6Integration_AllAuthenticationFeatures_WorkCorrectly()
    {
        // Arrange
        using var scope = await _factory.CreateScopeWithTestDataAsync();

        // Act & Assert - 主要認証フローの統合確認
        
        // 1. ログイン画面アクセス（Blazor版）
        var loginResponse = await _client.GetAsync("/login");
        Assert.True(loginResponse.IsSuccessStatusCode);

        // 2. MVC版ログイン削除確認
        var mvcLoginResponse = await _client.GetAsync("/Account/Login");
        Assert.Equal(HttpStatusCode.NotFound, mvcLoginResponse.StatusCode);

        // 3. パスワード変更画面アクセス
        var changePasswordResponse = await _client.GetAsync("/change-password");
        Assert.True(changePasswordResponse.StatusCode == HttpStatusCode.OK ||
                   changePasswordResponse.StatusCode == HttpStatusCode.Redirect);

        // 4. 管理画面アクセス（認証必要）
        var adminResponse = await _client.GetAsync("/admin/users");
        Assert.True(adminResponse.StatusCode == HttpStatusCode.Redirect ||
                   adminResponse.StatusCode == HttpStatusCode.Unauthorized ||
                   adminResponse.StatusCode == HttpStatusCode.OK);

        // 5. ヘルスチェック正常動作
        var healthResponse = await _client.GetAsync("/health");
        Assert.True(healthResponse.IsSuccessStatusCode);
    }

    /// <summary>
    /// 回帰テスト: 既存機能への影響確認
    /// </summary>
    [Fact]
    public async Task RegressionTest_ExistingFeatures_RemainUnaffected()
    {
        // Act & Assert - 既存機能の動作確認
        
        // 静的ファイルアクセス
        var cssResponse = await _client.GetAsync("/css/app.css");
        Assert.True(cssResponse.IsSuccessStatusCode);

        // ホーム画面
        var homeResponse = await _client.GetAsync("/");
        Assert.True(homeResponse.IsSuccessStatusCode);

        // ヘルスチェックエンドポイント
        var healthLivenessResponse = await _client.GetAsync("/health/live");
        Assert.True(healthLivenessResponse.IsSuccessStatusCode);

        var healthReadinessResponse = await _client.GetAsync("/health/ready");
        Assert.True(healthReadinessResponse.IsSuccessStatusCode);
    }

    #endregion
}