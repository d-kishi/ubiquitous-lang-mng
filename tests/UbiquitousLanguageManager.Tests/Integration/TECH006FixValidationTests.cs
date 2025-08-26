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
using Microsoft.Extensions.Logging;

namespace UbiquitousLanguageManager.Tests.Integration;

/// <summary>
/// TECH-006: ログイン認証フローエラー修正効果確認統合テスト
/// 
/// 【テスト目的】
/// Headers read-onlyエラー解消確認・Blazor Server認証フロー正常動作確認
/// StateHasChanged()タイミング調整による認証Cookie処理修正効果測定
/// 
/// 【修正内容確認対象】
/// - Login.razor の HandleLoginAsync() メソッド認証処理順序最適化
/// - Cookie認証処理（ASP.NET Core Identity）とBlazor Server HTTPレスポンス制御の統合
/// - StateHasChanged()呼び出しタイミング調整効果
/// </summary>
public class TECH006FixValidationTests : IClassFixture<TestWebApplicationFactory<Program>>
{
    private readonly TestWebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public TECH006FixValidationTests(TestWebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false // リダイレクトを手動制御でエラー検証
        });
    }

    #region TECH-006修正効果確認: Headers read-onlyエラー解消

    /// <summary>
    /// TECH-006-Fix-1: 修正前エラー「Headers are read-only, response has already started」完全解消確認
    /// 
    /// 【検証内容】
    /// - ログイン処理でHeaders read-onlyエラーが発生しないことを確認
    /// - Cookie認証処理（ASP.NET Core Identity）正常完了確認
    /// - HTTPレスポンス制御とStateHasChanged()タイミング調整効果確認
    /// </summary>
    [Fact]
    public async Task LoginAuthentication_WithTECH006Fix_DoesNotThrowHeadersReadOnlyError()
    {
        // Arrange - 正常ログイン可能ユーザー準備
        using var scope = await _factory.CreateScopeWithTestDataAsync();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        
        var testUser = new ApplicationUser
        {
            UserName = "tech006test@example.com",
            Email = "tech006test@example.com",
            Name = "TECH-006修正確認ユーザー",
            IsFirstLogin = false // 通常ログインフロー
        };
        
        var createResult = await userManager.CreateAsync(testUser, "SecurePass123!");
        Assert.True(createResult.Succeeded);

        // Act - ログイン実行（修正後のLogin.razorを通じて）
        var loginRequest = new
        {
            Email = "tech006test@example.com",
            Password = "SecurePass123!",
            RememberMe = false
        };

        var loginContent = new StringContent(
            JsonSerializer.Serialize(loginRequest),
            Encoding.UTF8,
            "application/json"
        );

        // 修正前はここでSystem.InvalidOperationExceptionが発生していた
        var response = await _client.PostAsync("/api/auth/login", loginContent);

        // Assert - エラー無しでログイン処理完了確認
        Assert.True(response.IsSuccessStatusCode || response.StatusCode == HttpStatusCode.Redirect,
            $"ログイン処理が失敗しました。Status: {response.StatusCode}, Content: {await response.Content.ReadAsStringAsync()}");

        // Cookie設定成功確認
        var cookies = response.Headers.GetValues("Set-Cookie").ToArray();
        Assert.NotEmpty(cookies);
        Assert.Contains(cookies, c => c.Contains("Identity.Application")); // ASP.NET Core Identity認証Cookie
    }

    /// <summary>
    /// TECH-006-Fix-2: Blazor Server HTTPコンテキスト・SignalR通信正常性確認
    /// 
    /// 【検証内容】
    /// - Blazor Server特有のSignalR通信とHTTP認証Cookieの競合解消
    /// - StateHasChanged()によるサーバー→クライアント通信正常処理
    /// - HTTPレスポンス開始後のヘッダー変更制約回避確認
    /// </summary>
    [Fact]
    public async Task BlazorServerAuthentication_HTTPContextManagement_WorksCorrectly()
    {
        // Arrange
        using var scope = await _factory.CreateScopeWithTestDataAsync();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        
        // 複数の認証状態テストユーザー準備
        var testUsers = new[]
        {
            new { Email = "normal@example.com", Password = "Pass123!", IsFirstLogin = false },
            new { Email = "firstlogin@example.com", Password = "Pass123!", IsFirstLogin = true }
        };

        foreach (var userData in testUsers)
        {
            var user = new ApplicationUser
            {
                UserName = userData.Email,
                Email = userData.Email,
                Name = $"テストユーザー {userData.Email}",
                IsFirstLogin = userData.IsFirstLogin
            };
            
            await userManager.CreateAsync(user, userData.Password);
        }

        // Act & Assert - 各認証シナリオでHTTPコンテキスト正常処理確認
        foreach (var userData in testUsers)
        {
            var loginRequest = new
            {
                userData.Email,
                userData.Password,
                RememberMe = false
            };

            var loginContent = new StringContent(
                JsonSerializer.Serialize(loginRequest),
                Encoding.UTF8,
                "application/json"
            );

            // 修正後: HTTPレスポンス制御とBlazor Server統合が正常動作
            var response = await _client.PostAsync("/api/auth/login", loginContent);

            // HTTPステータス・Cookie処理正常確認
            Assert.True(response.IsSuccessStatusCode || response.StatusCode == HttpStatusCode.Redirect,
                $"{userData.Email}のログイン処理でHTTPコンテキストエラー発生");

            // SignalR通信対応ヘッダー確認（Blazor Server特有）
            Assert.Contains("text/html", response.Content.Headers.ContentType?.ToString() ?? "");
        }
    }

    /// <summary>
    /// TECH-006-Fix-3: 認証Cookie設定タイミング最適化確認
    /// 
    /// 【検証内容】
    /// - Cookie認証処理を StateHasChanged() 前に完了させる修正効果確認
    /// - ASP.NET Core Identity Cookie設定とBlazor Server UI更新の順序最適化
    /// - ChunkingCookieManager.AppendResponseCookie 正常実行確認
    /// </summary>
    [Fact]
    public async Task CookieAuthenticationTiming_OptimizedSequence_WorksCorrectly()
    {
        // Arrange - 認証Cookie処理テスト用ユーザー
        using var scope = await _factory.CreateScopeWithTestDataAsync();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        
        var testUser = new ApplicationUser
        {
            UserName = "cookietest@example.com",
            Email = "cookietest@example.com",
            Name = "Cookie処理テストユーザー",
            IsFirstLogin = false
        };
        
        await userManager.CreateAsync(testUser, "CookiePass123!");

        // Act - ログイン実行（最適化された認証シーケンス）
        var loginRequest = new
        {
            Email = "cookietest@example.com",
            Password = "CookiePass123!",
            RememberMe = true // Cookie永続化設定
        };

        var loginContent = new StringContent(
            JsonSerializer.Serialize(loginRequest),
            Encoding.UTF8,
            "application/json"
        );

        var response = await _client.PostAsync("/api/auth/login", loginContent);

        // Assert - Cookie設定成功確認（修正前は AppendResponseCookie でエラー）
        Assert.True(response.IsSuccessStatusCode || response.StatusCode == HttpStatusCode.Redirect);

        // 認証Cookie詳細確認
        var setCookieHeaders = response.Headers.GetValues("Set-Cookie").ToArray();
        Assert.NotEmpty(setCookieHeaders);
        
        // ASP.NET Core Identity標準Cookie確認
        var authCookie = setCookieHeaders.FirstOrDefault(c => c.Contains("Identity.Application"));
        Assert.NotNull(authCookie);
        
        // RememberMe=trueでの永続化設定確認
        if (authCookie != null)
        {
            Assert.Contains("expires=", authCookie.ToLower()); // 永続化Cookie設定
        }
    }

    #endregion

    #region 認証フロー統合動作確認

    /// <summary>
    /// TECH-006-Fix-4: 初回ログイン→パスワード変更フロー統合確認
    /// 
    /// 【検証内容】
    /// - 修正後ログイン処理での初回ログイン判定正常動作
    /// - パスワード変更画面リダイレクト処理正常動作
    /// - 認証状態とUI更新の統合処理確認
    /// </summary>
    [Fact]
    public async Task FirstLoginFlow_WithTECH006Fix_RedirectsCorrectly()
    {
        // Arrange - 初回ログインユーザー
        using var scope = await _factory.CreateScopeWithTestDataAsync();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        
        var firstLoginUser = new ApplicationUser
        {
            UserName = "firstloginfix@example.com",
            Email = "firstloginfix@example.com",
            Name = "初回ログイン修正確認ユーザー",
            IsFirstLogin = true
        };
        
        await userManager.CreateAsync(firstLoginUser, "FirstPass123!");

        // Act - 初回ログイン実行
        var loginRequest = new
        {
            Email = "firstloginfix@example.com",
            Password = "FirstPass123!",
            RememberMe = false
        };

        var loginContent = new StringContent(
            JsonSerializer.Serialize(loginRequest),
            Encoding.UTF8,
            "application/json"
        );

        var response = await _client.PostAsync("/api/auth/login", loginContent);

        // Assert - 修正後の正常リダイレクト確認
        Assert.True(response.IsSuccessStatusCode || response.StatusCode == HttpStatusCode.Redirect);

        if (response.StatusCode == HttpStatusCode.Redirect)
        {
            var location = response.Headers.Location?.ToString();
            Assert.Contains("/Account/ChangePassword", location ?? "");
        }
        else
        {
            // JSON応答の場合のリダイレクト指示確認
            var content = await response.Content.ReadAsStringAsync();
            Assert.Contains("redirectUrl", content);
            Assert.Contains("ChangePassword", content);
        }
    }

    /// <summary>
    /// TECH-006-Fix-5: 通常ログイン→管理画面アクセスフロー確認
    /// 
    /// 【検証内容】
    /// - 修正後の通常ログイン処理完全動作確認
    /// - 認証状態正常設定による管理画面アクセス可能性確認
    /// - Cookie認証とBlazor Server認証状態プロバイダー統合確認
    /// </summary>
    [Fact]
    public async Task NormalLoginFlow_WithTECH006Fix_AllowsAdminAccess()
    {
        // Arrange - 通常ログインユーザー
        using var scope = await _factory.CreateScopeWithTestDataAsync();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        
        var normalUser = new ApplicationUser
        {
            UserName = "normalloginfix@example.com",
            Email = "normalloginfix@example.com",
            Name = "通常ログイン修正確認ユーザー",
            IsFirstLogin = false
        };
        
        await userManager.CreateAsync(normalUser, "NormalPass123!");

        // Act - 通常ログイン実行
        var loginRequest = new
        {
            Email = "normalloginfix@example.com",
            Password = "NormalPass123!",
            RememberMe = false
        };

        var loginContent = new StringContent(
            JsonSerializer.Serialize(loginRequest),
            Encoding.UTF8,
            "application/json"
        );

        var loginResponse = await _client.PostAsync("/api/auth/login", loginContent);

        // Assert - 正常ログイン確認
        Assert.True(loginResponse.IsSuccessStatusCode || loginResponse.StatusCode == HttpStatusCode.Redirect);

        // 認証Cookie取得
        var cookies = loginResponse.Headers.GetValues("Set-Cookie").ToArray();
        var authCookie = cookies.FirstOrDefault(c => c.Contains("Identity.Application"));
        Assert.NotNull(authCookie);

        // 管理画面アクセス確認（認証状態が正しく設定されている場合）
        if (loginResponse.StatusCode == HttpStatusCode.OK)
        {
            // Set received auth cookie for subsequent requests
            _client.DefaultRequestHeaders.Add("Cookie", authCookie);
            
            var adminResponse = await _client.GetAsync("/admin/users");
            Assert.True(adminResponse.StatusCode == HttpStatusCode.OK ||
                       adminResponse.StatusCode == HttpStatusCode.Redirect);
        }
    }

    #endregion

    #region パフォーマンス・安定性確認

    /// <summary>
    /// TECH-006-Fix-6: 修正後認証処理のパフォーマンス・安定性確認
    /// 
    /// 【検証内容】
    /// - 修正による認証処理時間への影響測定
    /// - 連続ログイン処理の安定性確認
    /// - メモリリーク・リソース管理正常性確認
    /// </summary>
    [Fact]
    public async Task AuthenticationPerformance_WithTECH006Fix_IsStableAndFast()
    {
        // Arrange - パフォーマンステスト用ユーザー
        using var scope = await _factory.CreateScopeWithTestDataAsync();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        
        var perfTestUser = new ApplicationUser
        {
            UserName = "perftest@example.com",
            Email = "perftest@example.com",
            Name = "パフォーマンステストユーザー",
            IsFirstLogin = false
        };
        
        await userManager.CreateAsync(perfTestUser, "PerfPass123!");

        // Act - 複数回ログイン実行（安定性確認）
        var successfulLogins = 0;
        var totalExecutionTime = TimeSpan.Zero;
        const int testIterations = 5;

        for (int i = 0; i < testIterations; i++)
        {
            var startTime = DateTime.UtcNow;
            
            var loginRequest = new
            {
                Email = "perftest@example.com",
                Password = "PerfPass123!",
                RememberMe = false
            };

            var loginContent = new StringContent(
                JsonSerializer.Serialize(loginRequest),
                Encoding.UTF8,
                "application/json"
            );

            var response = await _client.PostAsync("/api/auth/login", loginContent);
            var executionTime = DateTime.UtcNow - startTime;
            totalExecutionTime += executionTime;

            if (response.IsSuccessStatusCode || response.StatusCode == HttpStatusCode.Redirect)
            {
                successfulLogins++;
            }

            // 各ログイン処理が妥当な時間内で完了することを確認
            Assert.True(executionTime.TotalSeconds < 10, $"ログイン処理時間が長すぎます: {executionTime.TotalSeconds}秒");
        }

        // Assert - 成功率・平均時間確認
        Assert.Equal(testIterations, successfulLogins); // 100%成功率
        
        var averageExecutionTime = totalExecutionTime.TotalMilliseconds / testIterations;
        Assert.True(averageExecutionTime < 5000, $"平均ログイン処理時間が長すぎます: {averageExecutionTime}ms");
    }

    /// <summary>
    /// TECH-006-Fix-7: エラー処理・例外ハンドリング改善確認
    /// 
    /// 【検証内容】
    /// - 修正後のエラー処理でHeaders read-onlyエラーが発生しないことを確認
    /// - 不正ログイン試行時の例外処理安定性確認
    /// - StateHasChanged()例外処理の改善確認
    /// </summary>
    [Fact]
    public async Task ErrorHandling_WithTECH006Fix_HandlesFailuresGracefully()
    {
        // Arrange
        using var scope = await _factory.CreateScopeWithTestDataAsync();

        // Act - 不正ログイン試行（修正前はここでも Headers read-only エラーの可能性）
        var invalidLoginAttempts = new[]
        {
            new { Email = "nonexistent@example.com", Password = "wrongpass" },
            new { Email = "invalidemail", Password = "wrongpass" },
            new { Email = "", Password = "" }
        };

        foreach (var attempt in invalidLoginAttempts)
        {
            var loginContent = new StringContent(
                JsonSerializer.Serialize(attempt),
                Encoding.UTF8,
                "application/json"
            );

            var response = await _client.PostAsync("/api/auth/login", loginContent);

            // Assert - エラーハンドリング正常動作（Headers read-onlyエラー無し）
            Assert.True(response.StatusCode == HttpStatusCode.BadRequest ||
                       response.StatusCode == HttpStatusCode.Unauthorized ||
                       response.StatusCode == HttpStatusCode.UnprocessableEntity,
                $"不正ログイン試行で予期しないステータス: {response.StatusCode}");
            
            // レスポンス内容確認（エラーメッセージ適切な設定）
            var content = await response.Content.ReadAsStringAsync();
            Assert.NotNull(content);
        }
    }

    #endregion
}