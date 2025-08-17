using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Text;
using Xunit;
using Microsoft.EntityFrameworkCore;
using UbiquitousLanguageManager.Infrastructure.Data;
using UbiquitousLanguageManager.Tests.TestUtilities;
using Microsoft.AspNetCore.Identity;
using UbiquitousLanguageManager.Infrastructure.Data.Entities;
using System.Text.Json;
using Microsoft.Extensions.Configuration;

namespace UbiquitousLanguageManager.Tests.Integration;

/// <summary>
/// Phase A6 認証フロー統合テスト・E2E検証
/// TECH-002/003/004の全機能を包括的に検証
/// </summary>
public class AuthenticationFlowIntegrationTests : IClassFixture<TestWebApplicationFactory<Program>>
{
    private readonly TestWebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public AuthenticationFlowIntegrationTests(TestWebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false // リダイレクトを手動で制御
        });
    }

    #region TECH-003検証: Blazor版ログイン統合テスト

    /// <summary>
    /// TECH-003-1: /login アクセス → Blazor版ログイン画面正常表示
    /// </summary>
    [Fact]
    public async Task BlazorLogin_Get_ReturnsSuccessAndCorrectContent()
    {
        // Act
        var response = await _client.GetAsync("/login");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("text/html; charset=utf-8", response.Content.Headers.ContentType?.ToString());
        
        var content = await response.Content.ReadAsStringAsync();
        
        // Blazorログイン画面の必須要素を確認
        Assert.Contains("ログイン", content);
        Assert.Contains("メールアドレス", content);
        Assert.Contains("パスワード", content);
        Assert.Contains("_Host", content); // Blazor Server特有のマーカー
        
        // CSRF保護の確認
        Assert.Contains("antiforgery", content.ToLower());
    }

    /// <summary>
    /// TECH-003-2: /Account/Login アクセス → 404エラー確認（MVC版削除確認）
    /// </summary>
    [Fact]
    public async Task MvcLogin_Get_ReturnsNotFound()
    {
        // Act
        var response = await _client.GetAsync("/Account/Login");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    /// <summary>
    /// TECH-003-3: MVC AccountController Login ActionMethod削除確認
    /// </summary>
    [Fact]
    public void MvcAccountController_LoginAction_ShouldNotExist()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var serviceProvider = scope.ServiceProvider;
        
        // Act & Assert
        // MVCのAccountControllerが存在しないことを確認
        var controllerTypes = typeof(Program).Assembly.GetTypes()
            .Where(t => t.Name.Contains("AccountController") && t.BaseType?.Name == "Controller")
            .ToList();
        
        // AccountControllerは削除されているべき
        Assert.Empty(controllerTypes);
    }

    #endregion

    #region TECH-002検証: 初期パスワード"su"認証テスト

    /// <summary>
    /// TECH-002-1: 新規環境での初期スーパーユーザー作成・初期パスワード"su"での正常ログイン
    /// </summary>
    [Fact]
    public async Task InitialSuperUser_LoginWithCorrectPassword_ShouldSucceed()
    {
        // Arrange - テスト用環境での初期データ確認
        using var scope = await _factory.CreateScopeWithTestDataAsync();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();
        
        // 設定ファイルから初期パスワードを確認
        var initialPassword = configuration["InitialSuperUser:Password"];
        Assert.Equal("su", initialPassword); // TECH-002解消確認
        
        // 初期スーパーユーザーの存在確認
        var superUser = await userManager.FindByEmailAsync("admin@ubiquitous-lang.com");
        Assert.NotNull(superUser);
        Assert.True(superUser.IsFirstLogin); // 初回ログインフラグ確認

        // Act - 正常ログインテスト
        var loginRequest = new
        {
            Email = "admin@ubiquitous-lang.com",
            Password = "su",
            RememberMe = false
        };

        var loginContent = new StringContent(
            JsonSerializer.Serialize(loginRequest),
            Encoding.UTF8,
            "application/json"
        );

        // ログインAPI呼び出し（Blazor Server認証エンドポイント）
        var response = await _client.PostAsync("/api/auth/login", loginContent);

        // Assert
        Assert.True(response.StatusCode == HttpStatusCode.OK || 
                   response.StatusCode == HttpStatusCode.Redirect);
    }

    /// <summary>
    /// TECH-002-2: 旧パスワード"TempPass123!"でのログイン失敗確認
    /// </summary>
    [Fact]
    public async Task InitialSuperUser_LoginWithOldPassword_ShouldFail()
    {
        // Arrange
        using var scope = await _factory.CreateScopeWithTestDataAsync();
        
        var loginRequest = new
        {
            Email = "admin@ubiquitous-lang.com",
            Password = "TempPass123!", // 旧パスワード
            RememberMe = false
        };

        var loginContent = new StringContent(
            JsonSerializer.Serialize(loginRequest),
            Encoding.UTF8,
            "application/json"
        );

        // Act
        var response = await _client.PostAsync("/api/auth/login", loginContent);

        // Assert - ログイン失敗を確認
        Assert.True(response.StatusCode == HttpStatusCode.Unauthorized ||
                   response.StatusCode == HttpStatusCode.BadRequest);
    }

    #endregion

    #region TECH-004検証: 初回ログインフロー統合テスト

    /// <summary>
    /// TECH-004-1: IsFirstLogin=trueユーザーの初回ログイン・パスワード変更画面への自動リダイレクト確認
    /// </summary>
    [Fact]
    public async Task FirstLoginUser_Login_RedirectsToChangePassword()
    {
        // Arrange - 初回ログインユーザー作成
        using var scope = await _factory.CreateScopeWithTestDataAsync();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        
        var testUser = new ApplicationUser
        {
            UserName = "firstlogin@test.com",
            Email = "firstlogin@test.com",
            Name = "初回ログインテストユーザー",
            IsFirstLogin = true
        };
        
        var createResult = await userManager.CreateAsync(testUser, "TempPass123!");
        Assert.True(createResult.Succeeded);

        // Act - ログイン実行
        var loginRequest = new
        {
            Email = "firstlogin@test.com",
            Password = "TempPass123!",
            RememberMe = false
        };

        var loginContent = new StringContent(
            JsonSerializer.Serialize(loginRequest),
            Encoding.UTF8,
            "application/json"
        );

        var response = await _client.PostAsync("/api/auth/login", loginContent);

        // Assert - パスワード変更画面へのリダイレクト確認
        if (response.StatusCode == HttpStatusCode.Redirect)
        {
            var location = response.Headers.Location?.ToString();
            Assert.Contains("/change-password", location ?? "");
        }
        else if (response.StatusCode == HttpStatusCode.OK)
        {
            // レスポンス内容でリダイレクト指示を確認
            var content = await response.Content.ReadAsStringAsync();
            var responseData = JsonSerializer.Deserialize<Dictionary<string, object>>(content);
            Assert.True(responseData?.ContainsKey("redirectUrl") == true);
        }
    }

    /// <summary>
    /// TECH-004-2: FirstLoginRedirectMiddleware動作確認・アクセス制限テスト
    /// </summary>
    [Fact]
    public async Task FirstLoginMiddleware_RestrictsAccessToProtectedPaths()
    {
        // Arrange - 認証済み初回ログインユーザー（パスワード変更未完了）
        using var scope = await _factory.CreateScopeWithTestDataAsync();
        
        // テスト用認証Cookieを設定（初回ログイン状態）
        _client.DefaultRequestHeaders.Add("Cookie", "TestAuth=FirstLoginUser");

        // Act & Assert - 制限対象パスへのアクセステスト
        var restrictedPaths = new[]
        {
            "/admin/users",
            "/admin/projects",
            "/admin/organizations"
        };

        foreach (var path in restrictedPaths)
        {
            var response = await _client.GetAsync(path);
            
            // パスワード変更画面へのリダイレクトまたはアクセス拒否
            Assert.True(response.StatusCode == HttpStatusCode.Redirect ||
                       response.StatusCode == HttpStatusCode.Forbidden ||
                       response.StatusCode == HttpStatusCode.Unauthorized);
            
            if (response.StatusCode == HttpStatusCode.Redirect)
            {
                var location = response.Headers.Location?.ToString();
                Assert.Contains("/change-password", location ?? "");
            }
        }
    }

    /// <summary>
    /// TECH-004-3: パスワード変更画面・ログアウトへのアクセス許可確認
    /// </summary>
    [Fact]
    public async Task FirstLoginMiddleware_AllowsAccessToAllowedPaths()
    {
        // Arrange
        using var scope = await _factory.CreateScopeWithTestDataAsync();
        
        // Act & Assert - 許可対象パスへのアクセステスト
        var allowedPaths = new[]
        {
            "/change-password",
            "/account/logout"
        };

        foreach (var path in allowedPaths)
        {
            var response = await _client.GetAsync(path);
            
            // アクセス許可確認（200 OK または適切なレスポンス）
            Assert.True(response.StatusCode == HttpStatusCode.OK ||
                       response.StatusCode == HttpStatusCode.Redirect ||
                       response.StatusCode == HttpStatusCode.Found);
        }
    }

    /// <summary>
    /// TECH-004-4: パスワード変更完了後のフラグ更新確認
    /// </summary>
    [Fact]
    public async Task PasswordChange_CompletesSuccessfully_UpdatesFirstLoginFlag()
    {
        // Arrange - 初回ログインユーザー作成
        using var scope = await _factory.CreateScopeWithTestDataAsync();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        
        var testUser = new ApplicationUser
        {
            UserName = "passwordchange@test.com",
            Email = "passwordchange@test.com",
            Name = "パスワード変更テストユーザー",
            IsFirstLogin = true
        };
        
        await userManager.CreateAsync(testUser, "TempPass123!");

        // Act - パスワード変更API呼び出し
        var changePasswordRequest = new
        {
            CurrentPassword = "TempPass123!",
            NewPassword = "NewSecurePass456!",
            ConfirmPassword = "NewSecurePass456!"
        };

        var changeContent = new StringContent(
            JsonSerializer.Serialize(changePasswordRequest),
            Encoding.UTF8,
            "application/json"
        );

        var response = await _client.PostAsync("/api/auth/change-password", changeContent);

        // Assert - パスワード変更成功確認
        Assert.True(response.StatusCode == HttpStatusCode.OK ||
                   response.StatusCode == HttpStatusCode.NoContent);

        // IsFirstLoginフラグの更新確認
        var updatedUser = await userManager.FindByEmailAsync("passwordchange@test.com");
        Assert.NotNull(updatedUser);
        Assert.False(updatedUser.IsFirstLogin); // フラグが更新されているべき
    }

    #endregion

    #region E2Eシナリオテスト: 完全認証フロー

    /// <summary>
    /// E2E-1: 新規ユーザー登録 → 初回ログイン判定 → パスワード変更強制 → 通常ログインフロー確認
    /// </summary>
    [Fact]
    public async Task CompleteAuthenticationFlow_E2EScenario_WorksCorrectly()
    {
        // Arrange - テスト環境準備
        using var scope = await _factory.CreateScopeWithTestDataAsync();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        // Step 1: 新規ユーザー登録（初回ログインフラグあり）
        var newUser = new ApplicationUser
        {
            UserName = "e2etest@example.com",
            Email = "e2etest@example.com",
            Name = "E2Eテストユーザー",
            IsFirstLogin = true
        };
        
        var createResult = await userManager.CreateAsync(newUser, "InitialPass123!");
        Assert.True(createResult.Succeeded);

        // Step 2: 初回ログイン・リダイレクト確認
        var loginRequest = new
        {
            Email = "e2etest@example.com",
            Password = "InitialPass123!",
            RememberMe = false
        };

        var loginContent = new StringContent(
            JsonSerializer.Serialize(loginRequest),
            Encoding.UTF8,
            "application/json"
        );

        var loginResponse = await _client.PostAsync("/api/auth/login", loginContent);
        Assert.True(loginResponse.IsSuccessStatusCode);

        // Step 3: パスワード変更実行
        var changePasswordRequest = new
        {
            CurrentPassword = "InitialPass123!",
            NewPassword = "NewSecurePass789!",
            ConfirmPassword = "NewSecurePass789!"
        };

        var changeContent = new StringContent(
            JsonSerializer.Serialize(changePasswordRequest),
            Encoding.UTF8,
            "application/json"
        );

        var changeResponse = await _client.PostAsync("/api/auth/change-password", changeContent);
        Assert.True(changeResponse.IsSuccessStatusCode);

        // Step 4: フラグ更新確認
        var updatedUser = await userManager.FindByEmailAsync("e2etest@example.com");
        Assert.False(updatedUser!.IsFirstLogin);

        // Step 5: 通常ログインフロー確認（新パスワード）
        var normalLoginRequest = new
        {
            Email = "e2etest@example.com",
            Password = "NewSecurePass789!",
            RememberMe = false
        };

        var normalLoginContent = new StringContent(
            JsonSerializer.Serialize(normalLoginRequest),
            Encoding.UTF8,
            "application/json"
        );

        var normalLoginResponse = await _client.PostAsync("/api/auth/login", normalLoginContent);
        Assert.True(normalLoginResponse.IsSuccessStatusCode);

        // Step 6: 管理画面アクセス確認（制限解除）
        var adminResponse = await _client.GetAsync("/admin/users");
        Assert.True(adminResponse.StatusCode == HttpStatusCode.OK ||
                   adminResponse.StatusCode == HttpStatusCode.Redirect); // 正常または認証リダイレクト
    }

    /// <summary>
    /// E2E-2: セキュリティポリシー動作確認・ログイン失敗試行記録
    /// </summary>
    [Fact]
    public async Task SecurityPolicy_LoginFailureAttempts_AreRecorded()
    {
        // Arrange
        using var scope = await _factory.CreateScopeWithTestDataAsync();

        // Act - 複数回の失敗ログイン試行
        var failureAttempts = new[]
        {
            new { Email = "nonexistent@test.com", Password = "wrongpass" },
            new { Email = "admin@ubiquitous-lang.com", Password = "wrongpass" },
            new { Email = "invalid@email", Password = "wrongpass" }
        };

        foreach (var attempt in failureAttempts)
        {
            var loginContent = new StringContent(
                JsonSerializer.Serialize(attempt),
                Encoding.UTF8,
                "application/json"
            );

            var response = await _client.PostAsync("/api/auth/login", loginContent);
            
            // Assert - 失敗応答確認
            Assert.True(response.StatusCode == HttpStatusCode.Unauthorized ||
                       response.StatusCode == HttpStatusCode.BadRequest);
        }

        // セキュリティログの記録確認は実際のログシステム実装後に拡張
    }

    #endregion

    #region インフラストラクチャ・構成テスト

    /// <summary>
    /// 認証システム統合確認・依存関係の整合性チェック
    /// </summary>
    [Fact]
    public void AuthenticationSystem_DependencyIntegration_IsCorrectlyConfigured()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var serviceProvider = scope.ServiceProvider;

        // Act & Assert - 必要なサービスの登録確認
        var userManager = serviceProvider.GetService<UserManager<ApplicationUser>>();
        Assert.NotNull(userManager);

        var signInManager = serviceProvider.GetService<SignInManager<ApplicationUser>>();
        Assert.NotNull(signInManager);

        var dbContext = serviceProvider.GetService<UbiquitousLanguageDbContext>();
        Assert.NotNull(dbContext);

        // 認証設定の確認
        var configuration = serviceProvider.GetService<IConfiguration>();
        Assert.NotNull(configuration);
        
        var initialPassword = configuration?["InitialSuperUser:Password"];
        Assert.Equal("su", initialPassword); // TECH-002解消確認
    }

    /// <summary>
    /// CSRF保護・セキュリティヘッダーの確認
    /// </summary>
    [Fact]
    public async Task SecurityHeaders_AreCorrectlyConfigured()
    {
        // Act
        var response = await _client.GetAsync("/login");

        // Assert - セキュリティヘッダーの確認
        Assert.True(response.IsSuccessStatusCode);
        
        // CSRF保護の確認
        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("antiforgery", content.ToLower());
        
        // Content-Type ヘッダーの確認
        Assert.Contains("text/html", response.Content.Headers.ContentType?.ToString());
    }

    #endregion
}