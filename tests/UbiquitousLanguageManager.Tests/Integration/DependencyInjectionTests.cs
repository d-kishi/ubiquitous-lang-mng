using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Xunit;
using UbiquitousLanguageManager.Application;
using UbiquitousLanguageManager.Infrastructure.Services;
using System.Linq;
using UbiquitousLanguageManager.Tests.TestUtilities;

namespace UbiquitousLanguageManager.Tests.Integration;

/// <summary>
/// 依存関係注入設定の統合テスト
/// Phase A4 Step2: Clean Architecture基盤修正のためのTDDテスト
/// Phase A4 Step4: WebApplicationFactory DI競合解決
/// </summary>
public class DependencyInjectionTests : IClassFixture<TestWebApplicationFactory<Program>>
{
    private readonly TestWebApplicationFactory<Program> _factory;

    public DependencyInjectionTests(TestWebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    /// <summary>
    /// TDD Red Phase: IAuthenticationServiceがDIコンテナに登録されていることを確認
    /// 
    /// 【現在の状態】
    /// このテストは失敗することが期待されています。
    /// IAuthenticationServiceの実装がProgram.csでDI登録されていないため、
    /// サービス解決時に例外が発生します。
    /// </summary>
    [Fact]
    public async Task IAuthenticationService_Should_Be_Registered_In_DI_Container()
    {
        // Arrange
        await using var scope = _factory.Services.CreateAsyncScope();
        var serviceProvider = scope.ServiceProvider;

        // Act & Assert
        // このテストは現在失敗することが期待される（Red Phase）
        var authService = serviceProvider.GetService<IAuthenticationService>();
        
        Assert.NotNull(authService);
        Assert.IsType<AuthenticationService>(authService);
    }

    /// <summary>
    /// TDD Red Phase: UserApplicationServiceがDIコンテナで正しく解決できることを確認
    /// 
    /// 【現在の状態】
    /// UserApplicationServiceはIAuthenticationServiceに依存しているため、
    /// IAuthenticationServiceが登録されていない現状では、
    /// このサービスも解決できません。
    /// </summary>
    [Fact]
    public async Task UserApplicationService_Should_Be_Resolvable_From_DI_Container()
    {
        // Arrange
        await using var scope = _factory.Services.CreateAsyncScope();
        var serviceProvider = scope.ServiceProvider;

        // Act & Assert
        // このテストも現在失敗することが期待される（Red Phase）
        var userAppService = serviceProvider.GetService<UserApplicationService>();
        
        Assert.NotNull(userAppService);
    }

    /// <summary>
    /// TDD Red Phase: DbContextFactoryが正しく機能することを確認
    /// 
    /// 【現在の状態】
    /// DbContextFactoryは現在コメントアウトされているため、
    /// このテストも失敗します。
    /// </summary>
    [Fact]
    public async Task DbContextFactory_Should_Be_Available_For_Blazor_Server()
    {
        // Arrange
        await using var scope = _factory.Services.CreateAsyncScope();
        var serviceProvider = scope.ServiceProvider;

        // Act & Assert
        // 現在コメントアウトされているため失敗する（Red Phase）
        var dbContextFactory = serviceProvider.GetService<IDbContextFactory<UbiquitousLanguageManager.Infrastructure.Data.UbiquitousLanguageDbContext>>();
        
        Assert.NotNull(dbContextFactory);
        
        // ファクトリーが正しく動作することを確認
        using var context = dbContextFactory.CreateDbContext();
        Assert.NotNull(context);
    }

    /// <summary>
    /// Web層のAuthenticationServiceが登録されていることを確認
    /// これは現在正常に動作するはず
    /// </summary>
    [Fact]
    public async Task WebLayer_AuthenticationService_Should_Be_Registered()
    {
        // Arrange
        await using var scope = _factory.Services.CreateAsyncScope();
        var serviceProvider = scope.ServiceProvider;

        // Act
        var webAuthService = serviceProvider.GetService<UbiquitousLanguageManager.Web.Services.AuthenticationService>();
        
        // Assert
        Assert.NotNull(webAuthService);
    }
}