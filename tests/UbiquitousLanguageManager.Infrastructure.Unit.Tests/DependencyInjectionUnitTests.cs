using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Xunit;
using UbiquitousLanguageManager.Application;
using UbiquitousLanguageManager.Infrastructure.Services;
using UbiquitousLanguageManager.Infrastructure.Data;
using UbiquitousLanguageManager.Infrastructure.Repositories;

namespace UbiquitousLanguageManager.Infrastructure.Unit.Tests;

/// <summary>
/// 依存関係注入設定の単体テスト
/// Phase A4 Step2: Clean Architecture基盤修正のためのTDDテスト
/// </summary>
public class DependencyInjectionUnitTests
{
    /// <summary>
    /// TDD Green Phase: IAuthenticationServiceが正しく解決できることを確認
    /// Phase A9修正: AuthenticationServiceの完全な依存関係を登録
    /// </summary>
    [Fact]
    public void IAuthenticationService_Should_Be_Resolvable()
    {
        // Arrange
        var services = new ServiceCollection();

        // 必要な依存関係を登録
        services.AddLogging();

        // DbContext設定（インメモリデータベースを使用）
        services.AddDbContext<UbiquitousLanguageDbContext>(options =>
            options.UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()));

        // Identity設定（AuthenticationServiceの依存関係）
        services.AddIdentity<UbiquitousLanguageManager.Infrastructure.Data.Entities.ApplicationUser, Microsoft.AspNetCore.Identity.IdentityRole>()
            .AddEntityFrameworkStores<UbiquitousLanguageDbContext>()
            .AddDefaultTokenProviders();

        // Repository
        services.AddScoped<IUserRepository, UserRepository>();

        // Application層のサービス
        services.AddScoped<IAuthenticationService, AuthenticationService>();
        services.AddScoped<INotificationService, NotificationService>();

        var serviceProvider = services.BuildServiceProvider();

        // Act
        var authService = serviceProvider.GetService<IAuthenticationService>();

        // Assert
        Assert.NotNull(authService);
        Assert.IsType<AuthenticationService>(authService);
    }

    /// <summary>
    /// TDD Green Phase: INotificationServiceが正しく解決できることを確認
    /// </summary>
    [Fact]
    public void INotificationService_Should_Be_Resolvable()
    {
        // Arrange
        var services = new ServiceCollection();
        
        // 必要な依存関係を登録
        services.AddLogging();
        services.AddScoped<INotificationService, NotificationService>();
        
        var serviceProvider = services.BuildServiceProvider();
        
        // Act
        var notificationService = serviceProvider.GetService<INotificationService>();
        
        // Assert
        Assert.NotNull(notificationService);
        Assert.IsType<NotificationService>(notificationService);
    }

    /// <summary>
    /// TDD Green Phase: F#用ILoggerアダプターが正しく解決できることを確認
    /// </summary>
    [Fact]
    public void FSharpLoggerAdapter_Should_Be_Resolvable()
    {
        // Arrange
        var services = new ServiceCollection();
        
        // 必要な依存関係を登録
        services.AddLogging();
        services.AddScoped(typeof(UbiquitousLanguageManager.Application.ILogger<>), typeof(FSharpLoggerAdapter<>));
        
        var serviceProvider = services.BuildServiceProvider();
        
        // Act
        var logger = serviceProvider.GetService<UbiquitousLanguageManager.Application.ILogger<UserApplicationService>>();
        
        // Assert
        Assert.NotNull(logger);
        Assert.IsType<FSharpLoggerAdapter<UserApplicationService>>(logger);
    }

    /// <summary>
    /// TDD Green Phase: UserApplicationServiceが全ての依存関係と共に解決できることを確認
    /// Phase A9修正: Identity設定を追加してAuthenticationServiceの依存関係を完全に解決
    /// </summary>
    [Fact]
    public void UserApplicationService_Should_Be_Resolvable_With_All_Dependencies()
    {
        // Arrange
        var services = new ServiceCollection();

        // 必要な依存関係を全て登録
        services.AddLogging();

        // DbContext設定（インメモリデータベースを使用）
        services.AddDbContext<UbiquitousLanguageDbContext>(options =>
            options.UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()));

        // Identity設定（AuthenticationServiceの依存関係）
        services.AddIdentity<UbiquitousLanguageManager.Infrastructure.Data.Entities.ApplicationUser, Microsoft.AspNetCore.Identity.IdentityRole>()
            .AddEntityFrameworkStores<UbiquitousLanguageDbContext>()
            .AddDefaultTokenProviders();

        // Repository
        services.AddScoped<IUserRepository, UserRepository>();

        // Application層のサービス
        services.AddScoped<IAuthenticationService, AuthenticationService>();
        services.AddScoped<INotificationService, NotificationService>();
        services.AddScoped(typeof(UbiquitousLanguageManager.Application.ILogger<>), typeof(FSharpLoggerAdapter<>));
        services.AddScoped<UserApplicationService>();

        var serviceProvider = services.BuildServiceProvider();

        // Act
        UserApplicationService? userAppService = null;
        Exception? exception = null;

        try
        {
            userAppService = serviceProvider.GetService<UserApplicationService>();
        }
        catch (Exception ex)
        {
            exception = ex;
        }

        // Assert
        Assert.Null(exception);
        Assert.NotNull(userAppService);
    }

    /// <summary>
    /// TDD Green Phase: DbContextFactoryが正しく解決できることを確認
    /// </summary>
    [Fact]
    public void DbContextFactory_Should_Be_Resolvable()
    {
        // Arrange
        var services = new ServiceCollection();
        
        // DbContext設定
        services.AddDbContext<UbiquitousLanguageDbContext>(options =>
            options.UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()));
        
        // DbContextFactory設定
        services.AddDbContextFactory<UbiquitousLanguageDbContext>(options =>
            options.UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()));
        
        var serviceProvider = services.BuildServiceProvider();
        
        // Act
        var dbContextFactory = serviceProvider.GetService<IDbContextFactory<UbiquitousLanguageDbContext>>();
        
        // Assert
        Assert.NotNull(dbContextFactory);
        
        // ファクトリーが正しく動作することを確認
        using var context = dbContextFactory.CreateDbContext();
        Assert.NotNull(context);
    }
}