using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using Xunit;
using UbiquitousLanguageManager.Tests.TestUtilities;
using Microsoft.AspNetCore.Identity;
using UbiquitousLanguageManager.Infrastructure.Data.Entities;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace UbiquitousLanguageManager.Tests.Integration;

/// <summary>
/// FirstLoginRedirectMiddleware統合テスト
/// TECH-004: 初回ログイン時パスワード変更機能の完全検証
/// </summary>
public class FirstLoginRedirectMiddlewareTests : IClassFixture<TestWebApplicationFactory<Program>>
{
    private readonly TestWebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public FirstLoginRedirectMiddlewareTests(TestWebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false // リダイレクト動作を詳細に検証
        });
    }

    #region 初回ログインミドルウェア基本動作テスト

    /// <summary>
    /// 初回ログインユーザー: 制限対象パスアクセス時のリダイレクト確認
    /// </summary>
    [Theory]
    [InlineData("/admin/users")]
    [InlineData("/admin/organizations")]
    [InlineData("/admin/projects")]
    [InlineData("/user/profile")]
    [InlineData("/dashboard")]
    [InlineData("/settings")]
    public async Task FirstLoginUser_AccessRestrictedPath_RedirectsToChangePassword(string restrictedPath)
    {
        // Arrange - 初回ログインユーザーでの認証状態をシミュレート
        using var scope = await _factory.CreateScopeWithTestDataAsync();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        
        var firstLoginUser = new ApplicationUser
        {
            UserName = $"firstlogin-{Guid.NewGuid()}@test.com",
            Email = $"firstlogin-{Guid.NewGuid()}@test.com",
            Name = "初回ログインテストユーザー",
            IsFirstLogin = true,
        };
        
        var createResult = await userManager.CreateAsync(firstLoginUser, "su");
        Assert.True(createResult.Succeeded);

        // 認証状態のシミュレート（テスト環境での認証Cookie設定）
        await SimulateAuthenticatedUser(firstLoginUser);

        // Act
        var response = await _client.GetAsync(restrictedPath);

        // Assert
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        
        var location = response.Headers.Location?.ToString();
        Assert.NotNull(location);
        Assert.Contains("/change-password", location);
    }

    /// <summary>
    /// 初回ログインユーザー: 許可パスへのアクセス成功確認
    /// </summary>
    [Theory]
    [InlineData("/change-password")]
    [InlineData("/account/logout")]
    [InlineData("/login")]
    [InlineData("/")]
    [InlineData("/health")]
    [InlineData("/health/live")]
    [InlineData("/health/ready")]
    public async Task FirstLoginUser_AccessAllowedPath_Succeeds(string allowedPath)
    {
        // Arrange - 初回ログインユーザー認証状態
        using var scope = await _factory.CreateScopeWithTestDataAsync();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        
        var firstLoginUser = new ApplicationUser
        {
            UserName = $"allowed-{Guid.NewGuid()}@test.com",
            Email = $"allowed-{Guid.NewGuid()}@test.com",
            Name = "許可パステストユーザー",
            IsFirstLogin = true,
        };
        
        await userManager.CreateAsync(firstLoginUser, "su");
        await SimulateAuthenticatedUser(firstLoginUser);

        // Act
        var response = await _client.GetAsync(allowedPath);

        // Assert
        Assert.True(response.StatusCode == HttpStatusCode.OK ||
                   response.StatusCode == HttpStatusCode.Redirect ||
                   response.StatusCode == HttpStatusCode.Found,
                   $"Path {allowedPath} should be accessible for first login users");
                   
        // パスワード変更画面以外へのリダイレクトでないことを確認
        if (response.StatusCode == HttpStatusCode.Redirect && allowedPath != "/change-password")
        {
            var location = response.Headers.Location?.ToString();
            Assert.DoesNotContain("/change-password", location ?? "");
        }
    }

    /// <summary>
    /// 通常ユーザー（IsFirstLogin=false）: 全パスアクセス可能確認
    /// </summary>
    [Theory]
    [InlineData("/admin/users")]
    [InlineData("/user/profile")]
    [InlineData("/dashboard")]
    [InlineData("/change-password")]
    public async Task NormalUser_AccessAnyPath_NotRestricted(string path)
    {
        // Arrange - 通常ユーザー（初回ログイン完了済み）
        using var scope = await _factory.CreateScopeWithTestDataAsync();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        
        var normalUser = new ApplicationUser
        {
            UserName = $"normal-{Guid.NewGuid()}@test.com",
            Email = $"normal-{Guid.NewGuid()}@test.com",
            Name = "通常ユーザー",
            IsFirstLogin = false, // 初回ログイン完了済み
        };
        
        await userManager.CreateAsync(normalUser, "Password123!");
        await SimulateAuthenticatedUser(normalUser);

        // Act
        var response = await _client.GetAsync(path);

        // Assert
        // 通常ユーザーはFirstLoginRedirectMiddlewareによる制限を受けない
        Assert.True(response.StatusCode == HttpStatusCode.OK ||
                   response.StatusCode == HttpStatusCode.Redirect ||
                   response.StatusCode == HttpStatusCode.Found ||
                   response.StatusCode == HttpStatusCode.Unauthorized, // 認証以外の理由による制限は許可
                   $"Normal user should not be restricted by FirstLoginRedirectMiddleware for path {path}");
                   
        // パスワード変更画面への強制リダイレクトでないことを確認
        if (response.StatusCode == HttpStatusCode.Redirect)
        {
            var location = response.Headers.Location?.ToString();
            // 認証に関するリダイレクトの場合、パスワード変更画面ではないはず
            if (location?.Contains("/login") != true)
            {
                Assert.DoesNotContain("/change-password", location ?? "");
            }
        }
    }

    #endregion

    #region 認証状態・セッション管理テスト

    /// <summary>
    /// 未認証ユーザー: ミドルウェアによる制限を受けない確認
    /// </summary>
    [Theory]
    [InlineData("/admin/users")]
    [InlineData("/user/profile")]
    [InlineData("/dashboard")]
    public async Task UnauthenticatedUser_NotAffectedByFirstLoginMiddleware(string path)
    {
        // Arrange - 認証されていない状態

        // Act
        var response = await _client.GetAsync(path);

        // Assert
        // 未認証の場合、FirstLoginRedirectMiddlewareではなく通常の認証システムが動作
        Assert.True(response.StatusCode == HttpStatusCode.Redirect ||
                   response.StatusCode == HttpStatusCode.Unauthorized ||
                   response.StatusCode == HttpStatusCode.Forbidden,
                   "Unauthenticated user should be handled by normal authentication, not FirstLoginRedirectMiddleware");

        if (response.StatusCode == HttpStatusCode.Redirect)
        {
            var location = response.Headers.Location?.ToString();
            // 通常の認証リダイレクト（ログイン画面）であるべき
            Assert.True(location?.Contains("/login") == true || 
                       location?.Contains("/Account/Login") == true,
                       "Should redirect to login page, not change-password");
        }
    }

    /// <summary>
    /// セッション有効期限: 認証状態の適切な管理確認
    /// </summary>
    [Fact]
    public async Task AuthenticatedSession_MaintainsFirstLoginState()
    {
        // Arrange - 初回ログインユーザーでセッション開始
        using var scope = await _factory.CreateScopeWithTestDataAsync();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        
        var sessionUser = new ApplicationUser
        {
            UserName = "session@test.com",
            Email = "session@test.com",
            Name = "セッションテストユーザー",
            IsFirstLogin = true,
        };
        
        await userManager.CreateAsync(sessionUser, "SessionPass123!");
        await SimulateAuthenticatedUser(sessionUser);

        // Act & Assert - 複数リクエストでの一貫した制限動作
        var restrictedPaths = new[] { "/admin/users", "/dashboard", "/settings" };
        
        foreach (var path in restrictedPaths)
        {
            var response = await _client.GetAsync(path);
            
            Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
            var location = response.Headers.Location?.ToString();
            Assert.Contains("/change-password", location ?? "");
        }

        // 許可パスは継続してアクセス可能
        var allowedResponse = await _client.GetAsync("/change-password");
        Assert.True(allowedResponse.StatusCode == HttpStatusCode.OK ||
                   allowedResponse.StatusCode == HttpStatusCode.Redirect);
    }

    #endregion

    #region エラーハンドリング・境界値テスト

    /// <summary>
    /// 無効なパス: ミドルウェアが適切にスキップすることを確認
    /// </summary>
    [Theory]
    [InlineData("/nonexistent")]
    [InlineData("/api/nonexistent")]
    [InlineData("/static/nonexistent.css")]
    public async Task InvalidPaths_MiddlewareSkipsGracefully(string invalidPath)
    {
        // Arrange - 初回ログインユーザー
        using var scope = await _factory.CreateScopeWithTestDataAsync();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        
        var testUser = new ApplicationUser
        {
            UserName = "invalid-path@test.com",
            Email = "invalid-path@test.com",
            Name = "無効パステストユーザー",
            IsFirstLogin = true,
        };
        
        await userManager.CreateAsync(testUser, "InvalidPath123!");
        await SimulateAuthenticatedUser(testUser);

        // Act
        var response = await _client.GetAsync(invalidPath);

        // Assert
        // ミドルウェアは無効なパスに対しても正常に動作し、404などの適切なステータスを返す
        Assert.True(response.StatusCode == HttpStatusCode.NotFound ||
                   response.StatusCode == HttpStatusCode.Redirect ||
                   response.StatusCode == HttpStatusCode.OK);
                   
        // 無効なパスでもパスワード変更画面への不適切なリダイレクトは発生しない
        if (response.StatusCode == HttpStatusCode.Redirect)
        {
            var location = response.Headers.Location?.ToString();
            // 静的ファイルやAPIパスの場合、パスワード変更画面にリダイレクトすべきでない
            if (invalidPath.StartsWith("/static") || invalidPath.StartsWith("/api"))
            {
                Assert.DoesNotContain("/change-password", location ?? "");
            }
        }
    }

    /// <summary>
    /// 大量リクエスト: ミドルウェアのパフォーマンス確認
    /// </summary>
    [Fact]
    public async Task HighVolumeRequests_MiddlewarePerformsWell()
    {
        // Arrange - 初回ログインユーザー
        using var scope = await _factory.CreateScopeWithTestDataAsync();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        
        var performanceUser = new ApplicationUser
        {
            UserName = "performance@test.com",
            Email = "performance@test.com",
            Name = "パフォーマンステストユーザー",
            IsFirstLogin = true,
        };
        
        await userManager.CreateAsync(performanceUser, "Performance123!");
        await SimulateAuthenticatedUser(performanceUser);

        // Act - 大量の同時リクエスト
        var requestCount = 100;
        var tasks = new List<Task<HttpResponseMessage>>();
        
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        
        for (int i = 0; i < requestCount; i++)
        {
            tasks.Add(_client.GetAsync("/admin/users"));
        }
        
        var responses = await Task.WhenAll(tasks);
        stopwatch.Stop();

        // Assert
        foreach (var response in responses)
        {
            Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
            var location = response.Headers.Location?.ToString();
            Assert.Contains("/change-password", location ?? "");
            response.Dispose();
        }

        // パフォーマンス確認（平均応答時間1秒以下）
        var averageResponseTime = stopwatch.ElapsedMilliseconds / (double)requestCount;
        Assert.True(averageResponseTime < 1000, 
                   $"Average response time should be less than 1000ms, but was {averageResponseTime}ms");
    }

    #endregion

    #region 設定・カスタマイズテスト

    /// <summary>
    /// 設定値: 制限パス・許可パスの設定が正しく適用されることを確認
    /// </summary>
    [Fact]
    public async Task MiddlewareConfiguration_RestrictedAndAllowedPaths_AreRespected()
    {
        // Arrange
        using var scope = await _factory.CreateScopeWithTestDataAsync();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        
        var configUser = new ApplicationUser
        {
            UserName = "config@test.com",
            Email = "config@test.com",
            Name = "設定テストユーザー",
            IsFirstLogin = true,
        };
        
        await userManager.CreateAsync(configUser, "Config123!");
        await SimulateAuthenticatedUser(configUser);

        // Act & Assert - 設定による制限パスの確認
        var expectedRestrictedPaths = new[]
        {
            "/admin/users",
            "/admin/organizations",
            "/admin/projects",
            "/user/profile",
            "/dashboard",
            "/settings"
        };

        foreach (var path in expectedRestrictedPaths)
        {
            var response = await _client.GetAsync(path);
            Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
            
            var location = response.Headers.Location?.ToString();
            Assert.Contains("/change-password", location ?? "");
        }

        // 設定による許可パスの確認
        var expectedAllowedPaths = new[]
        {
            "/change-password",
            "/account/logout",
            "/login",
            "/",
            "/health"
        };

        foreach (var path in expectedAllowedPaths)
        {
            var response = await _client.GetAsync(path);
            Assert.True(response.StatusCode == HttpStatusCode.OK ||
                       response.StatusCode == HttpStatusCode.Redirect ||
                       response.StatusCode == HttpStatusCode.Found);
                       
            if (response.StatusCode == HttpStatusCode.Redirect)
            {
                var location = response.Headers.Location?.ToString();
                // 許可パスからパスワード変更画面への強制リダイレクトは発生しない
                if (path != "/change-password")
                {
                    Assert.DoesNotContain("/change-password", location ?? "");
                }
            }
        }
    }

    #endregion

    #region ヘルパーメソッド

    /// <summary>
    /// テスト用認証状態シミュレーション
    /// </summary>
    private async Task SimulateAuthenticatedUser(ApplicationUser user)
    {
        // テスト環境での認証状態設定
        // 実際の実装では、テスト用の認証Cookieまたはトークンを設定
        
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id),
            new(ClaimTypes.Name, user.UserName ?? ""),
            new(ClaimTypes.Email, user.Email ?? ""),
            new("IsFirstLogin", user.IsFirstLogin.ToString())
        };

        // テスト用認証ヘッダー設定（実装依存）
        var testAuthValue = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(
            $"{user.Email}:{user.IsFirstLogin}"));
        
        _client.DefaultRequestHeaders.Add("X-Test-Auth", testAuthValue);
        
        await Task.CompletedTask; // 非同期操作のプレースホルダー
    }

    #endregion
}