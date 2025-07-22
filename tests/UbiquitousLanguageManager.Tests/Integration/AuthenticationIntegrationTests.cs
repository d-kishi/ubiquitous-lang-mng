using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using System.Net.Http;
using Xunit;
using UbiquitousLanguageManager.Web;
using UbiquitousLanguageManager.Infrastructure.Data;

namespace UbiquitousLanguageManager.Tests.Integration;

/// <summary>
/// 認証システムの統合テスト
/// 
/// 【テスト方針】
/// WebApplicationFactoryを使用してテストサーバーを起動し、
/// 実際のHTTPリクエスト・レスポンスを通じて認証フローをテストします。
/// </summary>
public class AuthenticationIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public AuthenticationIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                // テスト用の設定をオーバーライド
                
                // 既存のDbContextを削除
                var dbContextDescriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<UbiquitousLanguageDbContext>));
                if (dbContextDescriptor != null)
                {
                    services.Remove(dbContextDescriptor);
                }
                
                var dbContextFactoryDescriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(IDbContextFactory<UbiquitousLanguageDbContext>));
                if (dbContextFactoryDescriptor != null)
                {
                    services.Remove(dbContextFactoryDescriptor);
                }
                
                // In-Memoryデータベースを使用
                services.AddDbContext<UbiquitousLanguageDbContext>(options =>
                {
                    options.UseInMemoryDatabase("TestDatabase_" + Guid.NewGuid().ToString());
                });
                
                services.AddDbContextFactory<UbiquitousLanguageDbContext>(options =>
                {
                    options.UseInMemoryDatabase("TestDatabase_" + Guid.NewGuid().ToString());
                });
                
                // InitialDataServiceをモックに置き換えて初期データ作成をスキップ
                var initialDataDescriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(UbiquitousLanguageManager.Infrastructure.Services.InitialDataService));
                if (initialDataDescriptor != null)
                {
                    services.Remove(initialDataDescriptor);
                }
                
                // モックのInitialDataServiceを登録
                services.AddScoped<UbiquitousLanguageManager.Infrastructure.Services.InitialDataService>(provider =>
                {
                    return new TestInitialDataService();
                });
            });
            
            builder.UseEnvironment("Testing");
        });
        
        _client = _factory.CreateClient();
    }

    /// <summary>
    /// テスト用のInitialDataService実装
    /// 初期データ作成をスキップします
    /// 
    /// 【統合テスト初学者向け解説】
    /// WebApplicationFactoryではDIコンテナが完全に作成されるため、
    /// 依存関係のないシンプルなサービス実装を提供します。
    /// これにより初期データ投入をスキップしてテストを高速化します。
    /// </summary>
    private class TestInitialDataService : UbiquitousLanguageManager.Infrastructure.Services.InitialDataService
    {
        /// <summary>
        /// テスト専用コンストラクタ
        /// MockのILoggerとNullObjectパターンでSettings設定を提供
        /// </summary>
        public TestInitialDataService() 
            : base(
                userManager: null!, 
                roleManager: null!, 
                logger: Microsoft.Extensions.Logging.Abstractions.NullLogger<UbiquitousLanguageManager.Infrastructure.Services.InitialDataService>.Instance,
                settings: Microsoft.Extensions.Options.Options.Create(new UbiquitousLanguageManager.Infrastructure.Services.InitialSuperUserSettings
                {
                    Email = "test@example.com",
                    Name = "テストユーザー",
                    Password = "TestPassword123!",
                    IsFirstLogin = false
                }))
        {
        }

        /// <summary>
        /// テスト用データ投入スキップ実装
        /// </summary>
        public override async Task SeedInitialDataAsync()
        {
            // テスト時は初期データ作成をスキップ
            await Task.CompletedTask;
        }
    }

    /// <summary>
    /// ホーム画面のアクセステスト
    /// </summary>
    [Fact]
    public async Task Home_Get_ReturnsSuccessAndCorrectContentType()
    {
        // Act
        var response = await _client.GetAsync("/");

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal("text/html; charset=utf-8", response.Content.Headers.ContentType?.ToString());
        
        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("ユビキタス言語管理システム", content);
    }

    /// <summary>
    /// ログイン画面のアクセステスト
    /// </summary>
    [Fact]
    public async Task Login_Get_ReturnsSuccessAndCorrectContent()
    {
        // Act
        var response = await _client.GetAsync("/Account/Login");

        // Assert
        response.EnsureSuccessStatusCode();
        
        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("ログイン", content);
        Assert.Contains("メールアドレス", content);
        Assert.Contains("パスワード", content);
    }

    /// <summary>
    /// パスワード変更画面の認証チェック
    /// </summary>
    [Fact]
    public async Task ChangePassword_Get_WithoutAuth_RedirectsToLogin()
    {
        // Act
        var response = await _client.GetAsync("/Account/ChangePassword");

        // Assert
        // 未認証の場合、ログイン画面にリダイレクトされることを確認
        Assert.True(response.StatusCode == System.Net.HttpStatusCode.Redirect || 
                   response.StatusCode == System.Net.HttpStatusCode.Found);
        
        var location = response.Headers.Location?.ToString();
        Assert.Contains("/Account/Login", location ?? "");
    }

    /// <summary>
    /// 存在しないページのアクセステスト
    /// </summary>
    [Fact]
    public async Task NonExistentPage_Get_ReturnsNotFound()
    {
        // Act
        var response = await _client.GetAsync("/NonExistent");

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.NotFound, response.StatusCode);
    }

    /// <summary>
    /// ヘルスチェックエンドポイントのテスト
    /// </summary>
    [Fact]
    public async Task HealthCheck_Get_ReturnsHealthy()
    {
        // Act
        var response = await _client.GetAsync("/health");

        // Assert
        response.EnsureSuccessStatusCode();
        
        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("Healthy", content);
    }

    /// <summary>
    /// CSS・JSファイルのアクセステスト
    /// </summary>
    [Theory]
    [InlineData("/css/app.css")]
    public async Task StaticFiles_Get_ReturnsSuccess(string url)
    {
        // Act
        var response = await _client.GetAsync(url);

        // Assert
        response.EnsureSuccessStatusCode();
    }

    /// <summary>
    /// APIエンドポイントの基本テスト
    /// </summary>
    [Fact]
    public async Task HealthLiveness_Get_ReturnsHealthy()
    {
        // Act
        var response = await _client.GetAsync("/health/live");

        // Assert
        response.EnsureSuccessStatusCode();
        
        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("Healthy", content);
    }

}