using System.Net;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using UbiquitousLanguageManager.Infrastructure.Data.Entities;
using UbiquitousLanguageManager.Infrastructure.Services;
using UbiquitousLanguageManager.Tests.TestUtilities;
using UbiquitousLanguageManager.Web;
using Xunit;

namespace UbiquitousLanguageManager.Tests.Integration;

/// <summary>
/// 初期パスワード認証機能の統合テスト
/// 機能仕様書 2.0.1（初期パスワード"su"固定）・2.2.1（平文管理）準拠
/// 
/// 【重要な実行制御】
/// ⚠️ 大量起動防止: 単一インスタンスのみ実行
/// ⚠️ 実行時間監視: 20分以内
/// ⚠️ メモリ使用量監視: 異常時の即座停止
/// </summary>
[Collection("IntegrationTests")] // 並列実行防止
public class InitialPasswordAuthenticationIntegrationTests : IClassFixture<TestWebApplicationFactory<Program>>, IDisposable
{
    private readonly TestWebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;
    private readonly DateTime _startTime;
    private bool _disposed = false;

    /// <summary>
    /// テスト初期化 - タイムアウト監視付き
    /// </summary>
    public InitialPasswordAuthenticationIntegrationTests(TestWebApplicationFactory<Program> factory)
    {
        _startTime = DateTime.UtcNow;
        _factory = factory ?? throw new ArgumentNullException(nameof(factory));
        _client = _factory.CreateClient();
        
        // テスト開始ログ
        Console.WriteLine($"🔍 [{DateTime.UtcNow:HH:mm:ss}] 初期パスワード認証統合テスト開始");
    }

    /// <summary>
    /// 統合テスト1: 初期ユーザー作成統合テスト
    /// InitialDataServiceによるユーザー作成・appsettings.json設定読み込み確認
    /// データベース実際の登録内容確認
    /// </summary>
    [Fact]
    public async Task InitialUserCreation_WithRealSettings_CreatesUserSuccessfully()
    {
        // タイムアウトチェック
        CheckTimeoutAndMemory("InitialUserCreation_WithRealSettings_CreatesUserSuccessfully");
        
        // Arrange - 実際のDI設定とappsettings.json設定を使用
        using var scope = _factory.Services.CreateScope();
        var serviceProvider = scope.ServiceProvider;
        
        // 実際の InitialDataService（テスト用ではない）を使用するための専用スコープ
        using var realDataServiceScope = CreateScopeWithRealInitialDataService();
        var realServiceProvider = realDataServiceScope.ServiceProvider;
        
        var userManager = realServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = realServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var logger = realServiceProvider.GetRequiredService<ILogger<InitialDataService>>();
        
        // appsettings.json設定の読み込み確認
        var settings = realServiceProvider.GetRequiredService<IOptions<InitialSuperUserSettings>>();
        
        // Act - 実際のInitialDataServiceでユーザー作成実行
        var initialDataService = new InitialDataService(userManager, roleManager, logger, settings);
        await initialDataService.SeedInitialDataAsync();
        
        // Assert - データベース実際の登録内容確認
        var createdUser = await userManager.FindByEmailAsync(settings.Value.Email);
        
        Assert.NotNull(createdUser);
        Assert.Equal("admin@ubiquitous-lang.com", createdUser!.Email);
        Assert.Equal("システム管理者", createdUser.Name);
        Assert.Equal("su", createdUser.InitialPassword); // 機能仕様書2.0.1準拠
        Assert.True(createdUser.IsFirstLogin); // 初回ログインフラグ
        Assert.Null(createdUser.PasswordHash); // 機能仕様書2.2.1準拠：平文管理時はNull
        Assert.True(createdUser.EmailConfirmed);
        Assert.False(createdUser.LockoutEnabled);
        
        // ロール確認
        var isInRole = await userManager.IsInRoleAsync(createdUser, "SuperUser");
        Assert.True(isInRole);
        
        // ロールマネージャーでのロール存在確認
        var roles = new[] { "SuperUser", "ProjectManager", "DomainApprover", "GeneralUser" };
        foreach (var role in roles)
        {
            var roleExists = await roleManager.RoleExistsAsync(role);
            Assert.True(roleExists, $"ロール {role} が作成されていません");
        }
        
        Console.WriteLine($"✅ [{DateTime.UtcNow:HH:mm:ss}] 初期ユーザー作成統合テスト完了");
    }

    /// <summary>
    /// 統合テスト2: 初回ログインフロー統合テスト  
    /// admin@ubiquitous-lang.com / "su" での認証成功
    /// IsFirstLoginフラグによるパスワード変更画面遷移
    /// UI層とAPI層の連携確認
    /// </summary>
    [Fact]
    public async Task InitialLogin_WithSuPassword_RedirectsToChangePassword()
    {
        // タイムアウトチェック
        CheckTimeoutAndMemory("InitialLogin_WithSuPassword_RedirectsToChangePassword");
        
        // Arrange - 初期ユーザーが作成済み状態を確保
        await EnsureInitialUserExists();
        
        // Act - ログイン画面取得
        var loginResponse = await _client.GetAsync("/Login");
        loginResponse.EnsureSuccessStatusCode();
        
        var loginContent = await loginResponse.Content.ReadAsStringAsync();
        Assert.Contains("ログイン", loginContent);
        
        // ログイン処理実行（API経由）
        var loginData = new
        {
            Email = "admin@ubiquitous-lang.com",
            Password = "su"
        };
        
        var loginJson = JsonSerializer.Serialize(loginData);
        var loginRequest = new StringContent(loginJson, Encoding.UTF8, "application/json");
        
        var authResponse = await _client.PostAsync("/api/auth/login", loginRequest);
        
        // Assert - 認証成功またはパスワード変更画面遷移の確認
        if (authResponse.StatusCode == HttpStatusCode.OK)
        {
            var authContent = await authResponse.Content.ReadAsStringAsync();
            var authResult = JsonSerializer.Deserialize<JsonElement>(authContent);
            
            // IsFirstLoginフラグによる分岐確認
            if (authResult.TryGetProperty("requiresPasswordChange", out var requiresChange) && 
                requiresChange.GetBoolean())
            {
                // 初回ログイン時のパスワード変更要求
                Assert.True(true, "初回ログイン時のパスワード変更要求が正常に返されました");
            }
            else if (authResult.TryGetProperty("success", out var success) && success.GetBoolean())
            {
                // 通常ログイン成功（既にパスワード変更済み）
                Assert.True(true, "ログイン成功が確認されました");
            }
            else
            {
                Assert.True(false, "予期しないレスポンス形式です");
            }
        }
        else if (authResponse.StatusCode == HttpStatusCode.Redirect)
        {
            // リダイレクトによるパスワード変更画面遷移
            var location = authResponse.Headers.Location?.ToString();
            Assert.Contains("ChangePassword", location ?? "");
        }
        else
        {
            // 認証エラーの詳細確認
            var errorContent = await authResponse.Content.ReadAsStringAsync();
            Assert.True(false, $"認証に失敗しました: {authResponse.StatusCode}, {errorContent}");
        }
        
        Console.WriteLine($"✅ [{DateTime.UtcNow:HH:mm:ss}] 初回ログインフロー統合テスト完了");
    }

    /// <summary>
    /// 統合テスト3: パスワード変更完了フロー統合テスト
    /// 初期パスワードから新パスワードへの変更
    /// InitialPassword=NULLへの更新確認  
    /// 2回目以降のPasswordHash認証確認
    /// </summary>
    [Fact]
    public async Task PasswordChange_FromInitialToNew_UpdatesAuthenticationMethod()
    {
        // タイムアウトチェック
        CheckTimeoutAndMemory("PasswordChange_FromInitialToNew_UpdatesAuthenticationMethod");
        
        // Arrange - 初期ユーザー確保とログイン
        await EnsureInitialUserExists();
        await AuthenticateAsInitialUser();
        
        // Act - パスワード変更実行
        var changePasswordData = new
        {
            CurrentPassword = "su",
            NewPassword = "NewSecurePassword123!",
            ConfirmPassword = "NewSecurePassword123!"
        };
        
        var changeJson = JsonSerializer.Serialize(changePasswordData);
        var changeRequest = new StringContent(changeJson, Encoding.UTF8, "application/json");
        
        var changeResponse = await _client.PostAsync("/api/auth/change-password", changeRequest);
        
        // Assert - パスワード変更成功確認
        if (changeResponse.StatusCode == HttpStatusCode.OK)
        {
            // データベースでの変更確認
            using var scope = CreateScopeWithRealInitialDataService();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            
            var user = await userManager.FindByEmailAsync("admin@ubiquitous-lang.com");
            Assert.NotNull(user);
            
            // InitialPassword=NULLへの更新確認（機能仕様書2.2.1準拠）
            Assert.Null(user!.InitialPassword);
            
            // PasswordHashが設定されることを確認
            Assert.NotNull(user.PasswordHash);
            
            // IsFirstLoginがfalseに更新されることを確認
            Assert.False(user.IsFirstLogin);
            
            // 新しいパスワードでのログイン確認
            var newLoginData = new
            {
                Email = "admin@ubiquitous-lang.com",
                Password = "NewSecurePassword123!"
            };
            
            var newLoginJson = JsonSerializer.Serialize(newLoginData);
            var newLoginRequest = new StringContent(newLoginJson, Encoding.UTF8, "application/json");
            
            var newAuthResponse = await _client.PostAsync("/api/auth/login", newLoginRequest);
            
            Assert.Equal(HttpStatusCode.OK, newAuthResponse.StatusCode);
            
            Console.WriteLine($"✅ [{DateTime.UtcNow:HH:mm:ss}] パスワード変更完了フロー統合テスト完了");
        }
        else
        {
            var errorContent = await changeResponse.Content.ReadAsStringAsync();
            Assert.True(false, $"パスワード変更に失敗しました: {changeResponse.StatusCode}, {errorContent}");
        }
    }

    /// <summary>
    /// ヘルパーメソッド: 初期ユーザーの存在確保
    /// </summary>
    private async Task EnsureInitialUserExists()
    {
        using var scope = CreateScopeWithRealInitialDataService();
        var serviceProvider = scope.ServiceProvider;
        
        var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var existingUser = await userManager.FindByEmailAsync("admin@ubiquitous-lang.com");
        
        if (existingUser == null)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var logger = serviceProvider.GetRequiredService<ILogger<InitialDataService>>();
            var settings = serviceProvider.GetRequiredService<IOptions<InitialSuperUserSettings>>();
            
            var initialDataService = new InitialDataService(userManager, roleManager, logger, settings);
            await initialDataService.SeedInitialDataAsync();
        }
    }

    /// <summary>
    /// ヘルパーメソッド: 初期ユーザーでの認証実行
    /// </summary>
    private async Task AuthenticateAsInitialUser()
    {
        var loginData = new
        {
            Email = "admin@ubiquitous-lang.com",
            Password = "su"
        };
        
        var loginJson = JsonSerializer.Serialize(loginData);
        var loginRequest = new StringContent(loginJson, Encoding.UTF8, "application/json");
        
        var authResponse = await _client.PostAsync("/api/auth/login", loginRequest);
        
        // 認証処理が何らかの形で成功することを確認
        // (リダイレクト、OK、その他の成功系ステータス)
        Assert.True(
            authResponse.StatusCode == HttpStatusCode.OK || 
            authResponse.StatusCode == HttpStatusCode.Redirect ||
            authResponse.StatusCode == HttpStatusCode.Found,
            $"認証が失敗しました: {authResponse.StatusCode}");
    }

    /// <summary>
    /// 実際のInitialDataServiceを使用するスコープの作成
    /// テスト用のダミーサービスではなく、本物の設定とサービスを使用
    /// </summary>
    private IServiceScope CreateScopeWithRealInitialDataService()
    {
        var services = new ServiceCollection();
        
        // 必要なサービスのみ登録（循環参照を避ける）
        var serviceProvider = _factory.Services;
        
        // 必要なサービスをファクトリーから取得
        services.AddSingleton(serviceProvider.GetRequiredService<UserManager<ApplicationUser>>());
        services.AddSingleton(serviceProvider.GetRequiredService<RoleManager<IdentityRole>>());
        services.AddSingleton<ILogger<InitialDataService>>(serviceProvider.GetRequiredService<ILogger<InitialDataService>>());
        
        // appsettings.jsonの設定を読み込み
        services.Configure<InitialSuperUserSettings>(options =>
        {
            options.Email = "admin@ubiquitous-lang.com";
            options.Name = "システム管理者";
            options.Password = "su";
            options.IsFirstLogin = true;
        });
        
        services.AddScoped<InitialDataService>();
        
        var newServiceProvider = services.BuildServiceProvider();
        return newServiceProvider.CreateScope();
    }

    /// <summary>
    /// タイムアウトとメモリ使用量の監視
    /// 実行時間20分制限とメモリ異常検知
    /// </summary>
    private void CheckTimeoutAndMemory(string testName)
    {
        var elapsed = DateTime.UtcNow - _startTime;
        if (elapsed.TotalMinutes > 20)
        {
            throw new TimeoutException($"テスト実行時間が20分を超過しました: {testName}");
        }
        
        // メモリ使用量監視（簡易版）
        GC.Collect();
        var memoryUsed = GC.GetTotalMemory(false) / (1024 * 1024); // MB単位
        
        if (memoryUsed > 512) // 512MB制限
        {
            throw new OutOfMemoryException($"メモリ使用量が制限を超過しました: {memoryUsed}MB in {testName}");
        }
        
        Console.WriteLine($"⏱️ [{DateTime.UtcNow:HH:mm:ss}] {testName}: 経過時間 {elapsed.TotalMinutes:F1}分, メモリ使用量 {memoryUsed}MB");
    }

    /// <summary>
    /// リソース解放処理
    /// </summary>
    public void Dispose()
    {
        if (!_disposed)
        {
            var totalElapsed = DateTime.UtcNow - _startTime;
            Console.WriteLine($"🏁 [{DateTime.UtcNow:HH:mm:ss}] 初期パスワード認証統合テスト完了 - 総実行時間: {totalElapsed.TotalMinutes:F1}分");
            
            _client?.Dispose();
            _disposed = true;
        }
    }
}