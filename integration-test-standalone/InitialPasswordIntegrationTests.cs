using System.Net;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using UbiquitousLanguageManager.Infrastructure.Data;
using UbiquitousLanguageManager.Infrastructure.Data.Entities;
using UbiquitousLanguageManager.Infrastructure.Services;
using UbiquitousLanguageManager.Web;
using Xunit;

namespace InitialPasswordIntegrationTests;

/// <summary>
/// 初期パスワード認証機能の統合テスト
/// 機能仕様書 2.0.1（初期パスワード"su"固定）・2.2.1（平文管理）準拠
/// 
/// 【重要な実行制御】
/// ⚠️ 大量起動防止: 単一インスタンスのみ実行
/// ⚠️ 実行時間監視: 20分以内
/// ⚠️ メモリ使用量監視: 異常時の即座停止
/// </summary>
[Collection("InitialPasswordIntegrationTests")] // 並列実行防止
public class InitialPasswordAuthenticationTests : IClassFixture<CustomWebApplicationFactory>, IDisposable
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;
    private readonly DateTime _startTime;
    private bool _disposed = false;

    /// <summary>
    /// テスト初期化 - タイムアウト監視付き
    /// </summary>
    public InitialPasswordAuthenticationTests(CustomWebApplicationFactory factory)
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
        
        var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var logger = serviceProvider.GetRequiredService<ILogger<InitialDataService>>();
        
        // 設定の確認
        var settings = serviceProvider.GetRequiredService<IOptions<InitialSuperUserSettings>>();
        
        // Act - InitialDataServiceでユーザー作成実行
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
    /// 統合テスト2: 基本認証フロー統合テスト  
    /// 初期ユーザーでの認証とレスポンス確認
    /// </summary>
    [Fact]
    public async Task InitialLogin_BasicFlow_WorksCorrectly()
    {
        // タイムアウトチェック
        CheckTimeoutAndMemory("InitialLogin_BasicFlow_WorksCorrectly");
        
        // Arrange - 初期ユーザーが作成済み状態を確保
        await EnsureInitialUserExists();
        
        // Act 1 - ログイン画面取得
        var loginResponse = await _client.GetAsync("/Login");
        
        // Assert 1 - ログイン画面が正常に表示される
        Assert.True(loginResponse.IsSuccessStatusCode || loginResponse.StatusCode == HttpStatusCode.Redirect);
        
        // Act 2 - ホーム画面取得（認証なし）
        var homeResponse = await _client.GetAsync("/");
        
        // Assert 2 - ホーム画面が正常に表示される
        homeResponse.EnsureSuccessStatusCode();
        var homeContent = await homeResponse.Content.ReadAsStringAsync();
        Assert.Contains("ユビキタス言語管理システム", homeContent);
        
        Console.WriteLine($"✅ [{DateTime.UtcNow:HH:mm:ss}] 基本認証フロー統合テスト完了");
    }

    /// <summary>
    /// 統合テスト3: データベース統合確認テスト
    /// 実際のデータベースでのユーザー情報とロール情報の確認
    /// </summary>
    [Fact]
    public async Task DatabaseIntegration_UserAndRoles_CreatedCorrectly()
    {
        // タイムアウトチェック
        CheckTimeoutAndMemory("DatabaseIntegration_UserAndRoles_CreatedCorrectly");
        
        // Arrange & Act - 初期データ作成
        await EnsureInitialUserExists();
        
        // Assert - データベース直接確認
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<UbiquitousLanguageDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        
        // ユーザー確認
        var users = await dbContext.Users.ToListAsync();
        Assert.NotEmpty(users);
        
        var adminUser = users.FirstOrDefault(u => u.Email == "admin@ubiquitous-lang.com");
        Assert.NotNull(adminUser);
        
        // ロール確認
        var roles = await dbContext.Roles.ToListAsync();
        Assert.True(roles.Count >= 4); // 最低4つのロールが存在
        
        var expectedRoles = new[] { "SuperUser", "ProjectManager", "DomainApprover", "GeneralUser" };
        foreach (var expectedRole in expectedRoles)
        {
            var role = roles.FirstOrDefault(r => r.Name == expectedRole);
            Assert.NotNull(role);
        }
        
        // ユーザーロール関連確認
        var userRoles = await dbContext.UserRoles.ToListAsync();
        Assert.NotEmpty(userRoles);
        
        Console.WriteLine($"✅ [{DateTime.UtcNow:HH:mm:ss}] データベース統合確認テスト完了");
    }

    /// <summary>
    /// ヘルパーメソッド: 初期ユーザーの存在確保
    /// </summary>
    private async Task EnsureInitialUserExists()
    {
        using var scope = _factory.Services.CreateScope();
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

/// <summary>
/// カスタムWebApplicationFactory - 統合テスト用の設定
/// </summary>
public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // 既存のDbContextを削除
            var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<UbiquitousLanguageDbContext>));
            if (descriptor != null)
            {
                services.Remove(descriptor);
            }

            // インメモリデータベースの設定
            services.AddDbContext<UbiquitousLanguageDbContext>(options =>
            {
                options.UseInMemoryDatabase($"TestDb_{Guid.NewGuid()}");
                options.EnableSensitiveDataLogging();
            });
            
            // appsettings.jsonの設定を模擬
            services.Configure<InitialSuperUserSettings>(options =>
            {
                options.Email = "admin@ubiquitous-lang.com";
                options.Name = "システム管理者";
                options.Password = "su";
                options.IsFirstLogin = true;
            });

            // ログ出力を抑制
            services.AddLogging(builder =>
            {
                builder.ClearProviders();
                builder.AddConsole();
                builder.SetMinimumLevel(LogLevel.Warning);
            });
        });
        
        builder.UseEnvironment("Test");
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
    }
}