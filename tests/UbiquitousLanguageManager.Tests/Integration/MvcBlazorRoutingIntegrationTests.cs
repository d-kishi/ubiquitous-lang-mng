using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Net;
using Xunit;

namespace UbiquitousLanguageManager.Tests.Integration;

/// <summary>
/// MVC/Blazor Server統合ルーティングテスト
/// Phase A4 Step3で実装したエントリーポイント分離パターンの検証
/// 
/// 【テスト戦略】
/// - MVC（/ ルート）とBlazor Server（/admin/*）の適切なルーティング確認
/// - 認証状態に応じた動的リダイレクト確認
/// - 認証エラーハンドリング統合確認
/// </summary>
public class MvcBlazorRoutingIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public MvcBlazorRoutingIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");
            builder.ConfigureServices(services =>
            {
                // テスト用設定のカスタマイズ
                // 本格的なテスト環境設定は将来のPhaseで実装
            });
        });
        _client = _factory.CreateClient();
    }

    /// <summary>
    /// MVCルートアクセステスト
    /// 【テスト内容】未認証ユーザーの/アクセス時にMVCビューが表示されることを確認
    /// </summary>
    [Fact]
    public async Task Get_Root_ReturnsSuccess_ForUnauthenticatedUser()
    {
        // Act - ルート（/）にアクセス
        var response = await _client.GetAsync("/");

        // Assert - MVCビューの正常表示確認
        response.EnsureSuccessStatusCode(); // HTTP 200 OK
        var content = await response.Content.ReadAsStringAsync();
        
        // HTMLコンテンツの基本的な確認
        Assert.Contains("text/html", response.Content.Headers.ContentType?.ToString());
    }

    /// <summary>
    /// Blazor Server管理画面アクセステスト（未認証）
    /// 【テスト内容】未認証ユーザーの/admin/*アクセス時に認証リダイレクトが発生することを確認
    /// </summary>
    [Fact]
    public async Task Get_AdminUsers_RedirectsToLogin_ForUnauthenticatedUser()
    {
        // Act - Blazor Server管理画面にアクセス
        var response = await _client.GetAsync("/admin/users");

        // Assert - 認証リダイレクトの確認
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        
        // ログインページへのリダイレクト確認
        var location = response.Headers.Location?.ToString();
        Assert.NotNull(location);
        Assert.Contains("/Account/Login", location);
    }

    /// <summary>
    /// エントリーポイント分離確認テスト
    /// 【テスト内容】/と/admin/*が適切に分離されてルーティングされることを確認
    /// </summary>
    [Theory]
    [InlineData("/")]
    [InlineData("/admin/users")]
    [InlineData("/admin/projects")]
    [InlineData("/admin/domains")]
    public async Task EntryPoint_Separation_WorksCorrectly(string path)
    {
        // Act
        var response = await _client.GetAsync(path);

        // Assert - すべてのパスで適切なレスポンスが返されることを確認
        Assert.True(response.StatusCode == HttpStatusCode.OK || 
                   response.StatusCode == HttpStatusCode.Redirect);

        // /admin/*は認証リダイレクト、/は直接表示
        if (path.StartsWith("/admin/"))
        {
            Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        }
        else if (path == "/")
        {
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }
    }

    /// <summary>
    /// HomeController認証制御テスト
    /// 【テスト内容】AllowAnonymous属性により未認証アクセスが許可されることを確認
    /// </summary>
    [Fact]
    public async Task HomeController_AllowsAnonymousAccess()
    {
        // Act
        var response = await _client.GetAsync("/");

        // Assert - 認証なしでアクセス可能
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        // Content-Typeがtext/htmlであることを確認（MVCビュー）
        var contentType = response.Content.Headers.ContentType;
        Assert.NotNull(contentType);
        Assert.Contains("text/html", contentType.ToString());
    }

    /// <summary>
    /// エラーハンドリング統合テスト
    /// 【テスト内容】存在しないパスへのアクセス時の適切なエラーハンドリング確認
    /// </summary>
    [Fact]
    public async Task NonExistent_Path_HandledGracefully()
    {
        // Act
        var response = await _client.GetAsync("/nonexistent");

        // Assert - 404エラーまたは適切なフォールバック処理
        Assert.True(response.StatusCode == HttpStatusCode.NotFound || 
                   response.StatusCode == HttpStatusCode.OK); // フォールバックによる処理
    }

    /// <summary>
    /// セキュリティヘッダー確認テスト
    /// 【テスト内容】適切なセキュリティヘッダーが設定されていることを確認
    /// </summary>
    [Fact]
    public async Task Security_Headers_ArePresent()
    {
        // Act
        var response = await _client.GetAsync("/");

        // Assert - 基本的なセキュリティ確認
        Assert.True(response.Headers.Contains("Cache-Control") || 
                   response.Content.Headers.Contains("Cache-Control"));
    }
}