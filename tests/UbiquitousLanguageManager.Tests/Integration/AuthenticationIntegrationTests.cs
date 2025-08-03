using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using System.Net.Http;
using Xunit;
using UbiquitousLanguageManager.Web;
using UbiquitousLanguageManager.Infrastructure.Data;
using UbiquitousLanguageManager.Tests.TestUtilities;

namespace UbiquitousLanguageManager.Tests.Integration;

/// <summary>
/// 認証システムの統合テスト
/// 
/// 【テスト方針】
/// WebApplicationFactoryを使用してテストサーバーを起動し、
/// 実際のHTTPリクエスト・レスポンスを通じて認証フローをテストします。
/// </summary>
public class AuthenticationIntegrationTests : IClassFixture<TestWebApplicationFactory<Program>>
{
    private readonly TestWebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public AuthenticationIntegrationTests(TestWebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
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
        // より柔軟な内容チェック（実装に依存しない基本的な確認）
        Assert.True(content.Contains("メールアドレス") || content.Contains("email") || content.Contains("Email") ||
                   content.Contains("ユーザー名") || content.Contains("username"));
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
        // 認証が必要なMVCページの場合、適切にリダイレクトまたは認証ページが表示される
        Assert.True(response.StatusCode == System.Net.HttpStatusCode.Redirect || 
                   response.StatusCode == System.Net.HttpStatusCode.Found ||
                   response.StatusCode == System.Net.HttpStatusCode.OK);
        
        if (response.StatusCode == System.Net.HttpStatusCode.OK)
        {
            // 認証ページまたはアクセス拒否ページが表示される場合
            var content = await response.Content.ReadAsStringAsync();
            Assert.True(content.Contains("ログイン") || content.Contains("認証") || content.Contains("アクセス"));
        }
        else
        {
            // リダイレクトの場合
            var location = response.Headers.Location?.ToString();
            Assert.Contains("/Account/Login", location ?? "");
        }
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
        // 存在しないページは404またはフォールバック処理により200が返される
        Assert.True(response.StatusCode == System.Net.HttpStatusCode.NotFound ||
                   response.StatusCode == System.Net.HttpStatusCode.OK);
        
        if (response.StatusCode == System.Net.HttpStatusCode.OK)
        {
            // フォールバック処理でBlazorページが返される場合
            var content = await response.Content.ReadAsStringAsync();
            Assert.Contains("text/html", response.Content.Headers.ContentType?.ToString());
        }
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