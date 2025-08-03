using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;
using UbiquitousLanguageManager.Infrastructure.Data;
using UbiquitousLanguageManager.Infrastructure.Services;

namespace UbiquitousLanguageManager.Tests.TestUtilities;

/// <summary>
/// テスト用のWebApplicationFactory
/// WebApplicationFactory統合テスト環境でのDI競合を解決
/// </summary>
public class TestWebApplicationFactory<TProgram> : WebApplicationFactory<TProgram>
    where TProgram : class
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // 既存のDbContext関連サービスを削除
            RemoveDbContextServices(services);
            
            // テスト用のインメモリデータベースを設定
            ConfigureTestDatabase(services);
            
            // テスト用のサービスを追加
            ConfigureTestServices(services);
        });
        
        builder.UseEnvironment("Test");
    }
    
    /// <summary>
    /// 既存のDbContext関連サービスを削除
    /// </summary>
    private void RemoveDbContextServices(IServiceCollection services)
    {
        // DbContext関連のすべてのサービスを削除
        var descriptorsToRemove = services
            .Where(d => 
                // DbContext自体
                d.ServiceType == typeof(UbiquitousLanguageDbContext) ||
                // DbContextOptions
                (d.ServiceType.IsGenericType && 
                 d.ServiceType.GetGenericTypeDefinition() == typeof(DbContextOptions<>) &&
                 d.ServiceType.GetGenericArguments()[0] == typeof(UbiquitousLanguageDbContext)) ||
                // IDbContextFactory
                (d.ServiceType.IsGenericType && 
                 d.ServiceType.GetGenericTypeDefinition() == typeof(IDbContextFactory<>) &&
                 d.ServiceType.GetGenericArguments()[0] == typeof(UbiquitousLanguageDbContext)) ||
                // その他のDbContext関連サービス
                d.ImplementationType == typeof(UbiquitousLanguageDbContext))
            .ToList();
        
        foreach (var descriptor in descriptorsToRemove)
        {
            services.Remove(descriptor);
        }
    }
    
    /// <summary>
    /// テスト用のインメモリデータベースを設定
    /// </summary>
    private void ConfigureTestDatabase(IServiceCollection services)
    {
        // テスト用のDbContextOptionsを作成（Singleton/Scoped競合を回避）
        var dbName = $"TestDb_{Guid.NewGuid()}";
        
        // DbContextFactoryの設定（Program.csと同じ方式で、IServiceProviderを使用）
        services.AddDbContextFactory<UbiquitousLanguageDbContext>((serviceProvider, options) =>
        {
            options.UseInMemoryDatabase(dbName);
            options.EnableSensitiveDataLogging(); // テスト時のデバッグ用
        });
        
        // DbContextも登録（テスト用）
        services.AddDbContext<UbiquitousLanguageDbContext>(options =>
        {
            options.UseInMemoryDatabase(dbName);
            options.EnableSensitiveDataLogging();
        });
    }
    
    /// <summary>
    /// テスト用のサービスを追加設定
    /// </summary>
    private void ConfigureTestServices(IServiceCollection services)
    {
        // ログ出力を抑制（テスト実行時のノイズを削減）
        services.AddLogging(builder =>
        {
            builder.ClearProviders();
            builder.AddFilter("Microsoft.EntityFrameworkCore", LogLevel.Warning);
            builder.AddFilter("Microsoft.AspNetCore", LogLevel.Warning);
        });
        
        // テスト用の初期データサービスを置き換え
        RemoveService<UbiquitousLanguageManager.Infrastructure.Services.InitialDataService>(services);
        services.AddScoped<UbiquitousLanguageManager.Infrastructure.Services.InitialDataService, TestInitialDataService>();
    }
    
    /// <summary>
    /// 指定されたサービスをDIコンテナから削除
    /// </summary>
    private void RemoveService<T>(IServiceCollection services)
    {
        var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(T));
        if (descriptor != null)
        {
            services.Remove(descriptor);
        }
    }
    
    /// <summary>
    /// テスト用のスコープを作成（初期データ設定付き）
    /// </summary>
    public async Task<IServiceScope> CreateScopeWithTestDataAsync()
    {
        var scope = Services.CreateScope();
        
        try
        {
            // テスト用初期データを設定
            var initialDataService = scope.ServiceProvider.GetRequiredService<UbiquitousLanguageManager.Infrastructure.Services.InitialDataService>();
            await initialDataService.SeedInitialDataAsync();
            
            return scope;
        }
        catch
        {
            scope.Dispose();
            throw;
        }
    }
}

/// <summary>
/// テスト用初期データサービス
/// Program.csの初期データ投入処理をスキップするためのテスト実装
/// </summary>
public class TestInitialDataService : UbiquitousLanguageManager.Infrastructure.Services.InitialDataService
{
    /// <summary>
    /// テスト専用コンストラクタ - 依存関係を最小化
    /// </summary>
    public TestInitialDataService() : base(
        userManager: null!,
        roleManager: null!,
        logger: Microsoft.Extensions.Logging.Abstractions.NullLogger<UbiquitousLanguageManager.Infrastructure.Services.InitialDataService>.Instance,
        settings: Microsoft.Extensions.Options.Options.Create(new UbiquitousLanguageManager.Infrastructure.Services.InitialSuperUserSettings
        {
            Email = "test@example.com",
            Name = "Test User",
            Password = "TestPassword123!",
            IsFirstLogin = false
        }))
    {
    }
    
    /// <summary>
    /// テスト時は初期データ投入をスキップ
    /// </summary>
    public override async Task SeedInitialDataAsync()
    {
        // テスト環境では初期データ投入をスキップ
        await Task.CompletedTask;
    }
}