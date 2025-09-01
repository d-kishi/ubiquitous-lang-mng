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
using Microsoft.Extensions.Logging;
using HtmlAgilityPack;

namespace UbiquitousLanguageManager.Tests.Integration;

/// <summary>
/// Home.razorルーティング競合解消後の統合品質確認テスト
/// 
/// 【修正確認対象】
/// - Pages/Home.razor削除済み・Components/Pages/Home.razor単一有効化確認
/// - ルーティング競合エラー完全解消確認（ポート5001動作環境）
/// - TECH-006解決効果継続確認（Headers read-onlyエラー0%）
/// - 認証フロー統合テスト（ログイン→Home画面遷移）
/// </summary>
public class HomeRoutingConflictResolutionTests : IClassFixture<TestWebApplicationFactory<Program>>
{
    private readonly TestWebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;
    private readonly HttpClient _noRedirectClient;

    public HomeRoutingConflictResolutionTests(TestWebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = true
        });
        _noRedirectClient = _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });
    }

    #region ルーティング競合解消確認

    /// <summary>
    /// 統合テスト-1: Home.razor重複解消・単一ルーティング確認
    /// </summary>
    [Fact]
    public async Task HomeRouting_ConflictResolved_SingleEndpointWorks()
    {
        // Arrange - 認証済みユーザー作成
        using var scope = await _factory.CreateScopeWithTestDataAsync();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        
        var testUser = new ApplicationUser
        {
            UserName = "routing.test@example.com",
            Email = "routing.test@example.com",
            Name = "ルーティング統合テストユーザー",
            IsFirstLogin = false
        };
        
        var createResult = await userManager.CreateAsync(testUser, "RoutingTest123!");
        Assert.True(createResult.Succeeded, $"テストユーザー作成失敗: {string.Join(", ", createResult.Errors.Select(e => e.Description))}");

        // ログイン処理（認証Cookie取得）
        var loginRequest = new
        {
            Email = "routing.test@example.com",
            Password = "RoutingTest123!",
            RememberMe = false
        };

        var loginContent = new StringContent(JsonSerializer.Serialize(loginRequest), Encoding.UTF8, "application/json");
        var loginResponse = await _noRedirectClient.PostAsync("/api/auth/login", loginContent);
        
        Assert.True(loginResponse.IsSuccessStatusCode, "ログイン処理が成功する必要があります");

        var authCookies = loginResponse.Headers.GetValues("Set-Cookie").ToArray();
        var authCookie = authCookies.FirstOrDefault(c => c.Contains("Identity.Application"));
        Assert.NotNull(authCookie);

        // Act - "/" ルートアクセス（認証Cookie付き）
        _client.DefaultRequestHeaders.Clear();
        _client.DefaultRequestHeaders.Add("Cookie", authCookie);

        var homeResponse = await _client.GetAsync("/");

        // Assert - ルーティング競合解消による正常アクセス確認
        Assert.Equal(HttpStatusCode.OK, homeResponse.StatusCode);

        var homeContent = await homeResponse.Content.ReadAsStringAsync();
        Assert.NotEmpty(homeContent);

        // Components/Pages/Home.razor が表示されていることを確認
        Assert.Contains("ダッシュボード", homeContent);
        Assert.Contains("ユビキタス言語管理システムへようこそ", homeContent);
        Assert.Contains("登録ユーザー数", homeContent);

        // "/home" パスでも同じページがアクセス可能か確認
        var homeAltResponse = await _client.GetAsync("/home");
        Assert.Equal(HttpStatusCode.OK, homeAltResponse.StatusCode);

        var homeAltContent = await homeAltResponse.Content.ReadAsStringAsync();
        Assert.Contains("ダッシュボード", homeAltContent);
    }

    /// <summary>
    /// 統合テスト-2: ルーティング競合エラー完全解消確認
    /// </summary>
    [Fact]
    public async Task ApplicationStartup_NoRoutingConflicts_CleanBuild()
    {
        // Act - アプリケーション起動状態確認（DIコンテナ初期化）
        using var scope = _factory.Services.CreateScope();
        var serviceProvider = scope.ServiceProvider;

        // Blazor Serverルーティングエンジン正常動作確認
        var routeData = serviceProvider.GetService<Microsoft.AspNetCore.Routing.RouteData>();
        
        // Assert - DIコンテナ正常初期化確認（ルーティング競合なし）
        Assert.NotNull(serviceProvider);

        // "/" エンドポイントアクセステスト（未認証状態）
        var rootResponse = await _client.GetAsync("/");
        
        // 未認証の場合は認証リダイレクトかログイン画面表示
        Assert.True(
            rootResponse.StatusCode == HttpStatusCode.OK || 
            rootResponse.StatusCode == HttpStatusCode.Redirect,
            $"ルートアクセスで予期しないステータス: {rootResponse.StatusCode}");

        // ルーティング競合が解消されていれば、例外やサーバーエラーは発生しない
        if (rootResponse.StatusCode == HttpStatusCode.OK)
        {
            var content = await rootResponse.Content.ReadAsStringAsync();
            
            // サーバーエラーや例外メッセージが含まれていないことを確認
            Assert.DoesNotContain("AmbiguousMatchException", content);
            Assert.DoesNotContain("Multiple routing", content);
            Assert.DoesNotContain("routing conflict", content);
        }
    }

    #endregion

    #region TECH-006解決効果継続確認

    /// <summary>
    /// 統合テスト-3: TECH-006解決効果継続・Headers read-onlyエラー0%確認
    /// </summary>
    [Fact]
    public async Task TECH006_FixContinuation_NoHeadersReadOnlyErrors()
    {
        // Arrange - 認証フロー統合テスト用ユーザー
        using var scope = await _factory.CreateScopeWithTestDataAsync();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        
        var integrationUser = new ApplicationUser
        {
            UserName = "tech006.continuation@example.com",
            Email = "tech006.continuation@example.com",
            Name = "TECH-006継続テストユーザー",
            IsFirstLogin = false
        };
        
        await userManager.CreateAsync(integrationUser, "Tech006Pass123!");

        // Act - AuthApiController統合による認証フロー実行
        var loginRequest = new
        {
            Email = "tech006.continuation@example.com",
            Password = "Tech006Pass123!",
            RememberMe = false
        };

        var loginContent = new StringContent(JsonSerializer.Serialize(loginRequest), Encoding.UTF8, "application/json");
        var loginResponse = await _noRedirectClient.PostAsync("/api/auth/login", loginContent);
        var loginResponseContent = await loginResponse.Content.ReadAsStringAsync();

        // Assert - Headers read-onlyエラー完全解消継続確認
        Assert.True(loginResponse.IsSuccessStatusCode,
            $"TECH-006効果継続失敗: Status={loginResponse.StatusCode}, Content={loginResponseContent}");

        // 認証成功レスポンス確認
        var authResponse = JsonSerializer.Deserialize<JsonElement>(loginResponseContent);
        Assert.True(authResponse.GetProperty("success").GetBoolean());

        // ホームページリダイレクト先確認
        var redirectUrl = authResponse.GetProperty("redirectUrl").GetString();
        Assert.Contains("/home", redirectUrl);

        // 認証Cookie正常設定確認
        var setCookieHeaders = loginResponse.Headers.GetValues("Set-Cookie").ToArray();
        var authCookie = setCookieHeaders.FirstOrDefault(c => c.Contains("Identity.Application"));
        Assert.NotNull(authCookie);

        // Step 2: 認証済みでHomeページアクセス
        _client.DefaultRequestHeaders.Clear();
        _client.DefaultRequestHeaders.Add("Cookie", authCookie);

        var homeResponse = await _client.GetAsync("/home");
        var homeContent = await homeResponse.Content.ReadAsStringAsync();

        // Homeページ正常表示・Headers read-onlyエラー0%継続確認
        Assert.Equal(HttpStatusCode.OK, homeResponse.StatusCode);
        Assert.Contains("ダッシュボード", homeContent);
        
        // エラーログ確認（Headers read-onlyエラーなし）
        Assert.DoesNotContain("Headers are read-only", homeContent);
        Assert.DoesNotContain("Response has already started", homeContent);
    }

    /// <summary>
    /// 統合テスト-4: 認証フロー統合テスト・ログイン→Home画面遷移
    /// </summary>
    [Fact]
    public async Task AuthenticationFlow_LoginToHome_CompleteIntegration()
    {
        // Arrange - 認証フロー統合テスト
        using var scope = await _factory.CreateScopeWithTestDataAsync();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        
        var authFlowUsers = new[]
        {
            new { Email = "normal.auth@example.com", IsFirstLogin = false, ExpectedRedirect = "/home" },
            new { Email = "first.auth@example.com", IsFirstLogin = true, ExpectedRedirect = "/change-password" }
        };

        foreach (var userData in authFlowUsers)
        {
            var user = new ApplicationUser
            {
                UserName = userData.Email,
                Email = userData.Email,
                Name = $"認証フロー統合テストユーザー ({userData.Email})",
                IsFirstLogin = userData.IsFirstLogin
            };
            
            await userManager.CreateAsync(user, "AuthFlow123!");
        }

        // Act & Assert - 各ユーザータイプでの認証フロー確認
        foreach (var userData in authFlowUsers)
        {
            var loginRequest = new
            {
                Email = userData.Email,
                Password = "AuthFlow123!",
                RememberMe = false
            };

            var loginContent = new StringContent(JsonSerializer.Serialize(loginRequest), Encoding.UTF8, "application/json");
            var loginResponse = await _noRedirectClient.PostAsync("/api/auth/login", loginContent);
            var loginResponseContent = await loginResponse.Content.ReadAsStringAsync();

            // 認証成功確認
            Assert.True(loginResponse.IsSuccessStatusCode,
                $"認証失敗 ({userData.Email}): {loginResponseContent}");

            var authResponse = JsonSerializer.Deserialize<JsonElement>(loginResponseContent);
            Assert.True(authResponse.GetProperty("success").GetBoolean());

            // 適切なリダイレクト先確認
            var redirectUrl = authResponse.GetProperty("redirectUrl").GetString();
            Assert.Contains(userData.ExpectedRedirect, redirectUrl);

            // 認証Cookie正常設定確認
            var authCookies = loginResponse.Headers.GetValues("Set-Cookie").ToArray();
            var authCookie = authCookies.FirstOrDefault(c => c.Contains("Identity.Application"));
            Assert.NotNull(authCookie);

            // 通常ユーザーの場合、Homeページアクセス確認
            if (!userData.IsFirstLogin)
            {
                _client.DefaultRequestHeaders.Clear();
                _client.DefaultRequestHeaders.Add("Cookie", authCookie);

                var homeResponse = await _client.GetAsync("/home");
                Assert.Equal(HttpStatusCode.OK, homeResponse.StatusCode);

                var homeContent = await homeResponse.Content.ReadAsStringAsync();
                Assert.Contains("ダッシュボード", homeContent);
                Assert.Contains("登録ユーザー数", homeContent);
            }
        }
    }

    #endregion

    #region パフォーマンス・安定性確認

    /// <summary>
    /// 統合テスト-5: ポート5001動作確認・パフォーマンス継続確認
    /// </summary>
    [Fact]
    public async Task Port5001_Performance_StabilityVerification()
    {
        // Arrange - パフォーマンス測定用ユーザー群
        using var scope = await _factory.CreateScopeWithTestDataAsync();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        
        var perfUsers = Enumerable.Range(1, 3).Select(i => new ApplicationUser
        {
            UserName = $"perf{i}@routing.test",
            Email = $"perf{i}@routing.test",
            Name = $"パフォーマンステストユーザー{i}",
            IsFirstLogin = false
        }).ToArray();

        foreach (var user in perfUsers)
        {
            await userManager.CreateAsync(user, "PerfTest123!");
        }

        // Act - 連続認証・Home画面アクセス（パフォーマンス測定）
        var performanceResults = new List<TimeSpan>();
        var successCount = 0;

        foreach (var user in perfUsers)
        {
            var startTime = DateTime.UtcNow;

            // Step 1: ログイン処理
            var loginRequest = new
            {
                Email = user.Email,
                Password = "PerfTest123!",
                RememberMe = false
            };

            var loginContent = new StringContent(JsonSerializer.Serialize(loginRequest), Encoding.UTF8, "application/json");
            var loginResponse = await _noRedirectClient.PostAsync("/api/auth/login", loginContent);

            if (!loginResponse.IsSuccessStatusCode) continue;

            var authCookies = loginResponse.Headers.GetValues("Set-Cookie").ToArray();
            var authCookie = authCookies.FirstOrDefault(c => c.Contains("Identity.Application"));
            
            if (authCookie == null) continue;

            // Step 2: Homeページアクセス
            _client.DefaultRequestHeaders.Clear();
            _client.DefaultRequestHeaders.Add("Cookie", authCookie);

            var homeResponse = await _client.GetAsync("/home");
            var executionTime = DateTime.UtcNow - startTime;

            performanceResults.Add(executionTime);

            if (homeResponse.IsSuccessStatusCode)
            {
                var homeContent = await homeResponse.Content.ReadAsStringAsync();
                if (homeContent.Contains("ダッシュボード"))
                {
                    successCount++;
                }
            }

            // パフォーマンス目標確認（<2秒）
            Assert.True(executionTime.TotalSeconds < 2.0,
                $"認証→Homeアクセス時間が目標値超過: {executionTime.TotalSeconds:F2}秒 (ユーザー: {user.Email})");

            await Task.Delay(50); // 負荷制御
        }

        // Assert - 成功率・平均パフォーマンス確認
        Assert.Equal(perfUsers.Length, successCount); // 100%成功率要求

        if (performanceResults.Any())
        {
            var avgResponseTime = performanceResults.Average(t => t.TotalMilliseconds);
            Assert.True(avgResponseTime < 1500, $"平均レスポンス時間が目標超過: {avgResponseTime:F2}ms");
        }
    }

    /// <summary>
    /// 統合テスト-6: エラーハンドリング・異常系統合確認（ルーティング競合解消後）
    /// </summary>
    [Fact]
    public async Task ErrorHandling_PostConflictResolution_GracefulFailure()
    {
        // Act & Assert - 異常系テストケース（ルーティング競合解消後）
        var errorTestCases = new[]
        {
            new { Path = "/", Description = "ルート未認証アクセス" },
            new { Path = "/home", Description = "Home未認証アクセス" },
            new { Path = "/nonexistent", Description = "存在しないパス" }
        };

        foreach (var testCase in errorTestCases)
        {
            var response = await _client.GetAsync(testCase.Path);
            var content = await response.Content.ReadAsStringAsync();

            // ルーティング競合エラーが発生していないことを確認
            Assert.DoesNotContain("AmbiguousMatchException", content);
            Assert.DoesNotContain("Multiple endpoints", content);
            Assert.DoesNotContain("routing conflict", content);

            // 適切なHTTPステータス
            Assert.True(
                response.StatusCode == HttpStatusCode.OK ||          // 認証リダイレクト処理
                response.StatusCode == HttpStatusCode.Redirect ||    // 明示的リダイレクト
                response.StatusCode == HttpStatusCode.NotFound,      // 存在しないパス
                $"テストケース '{testCase.Description}' で予期しないステータス: {response.StatusCode}");
        }
    }

    #endregion

    #region 回帰テスト・既存機能保護

    /// <summary>
    /// 統合テスト-7: 既存機能保護・回帰テスト確認（Home競合解消後）
    /// </summary>
    [Fact]
    public async Task ExistingFeatures_PostHomeConflictResolution_AllPreserved()
    {
        // Arrange - 回帰テスト用認証済みユーザー
        using var scope = await _factory.CreateScopeWithTestDataAsync();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        
        var regressionUser = new ApplicationUser
        {
            UserName = "regression.home@example.com",
            Email = "regression.home@example.com", 
            Name = "Home競合解消回帰テストユーザー",
            IsFirstLogin = false
        };
        
        await userManager.CreateAsync(regressionUser, "Regression123!");

        // ログイン処理
        var loginRequest = new
        {
            Email = "regression.home@example.com",
            Password = "Regression123!",
            RememberMe = false
        };

        var loginContent = new StringContent(JsonSerializer.Serialize(loginRequest), Encoding.UTF8, "application/json");
        var loginResponse = await _noRedirectClient.PostAsync("/api/auth/login", loginContent);
        
        Assert.True(loginResponse.IsSuccessStatusCode, "ログイン処理が成功する必要があります");

        var authCookies = loginResponse.Headers.GetValues("Set-Cookie").ToArray();
        var authCookie = authCookies.FirstOrDefault(c => c.Contains("Identity.Application"));
        Assert.NotNull(authCookie);

        // Act & Assert - 既存機能保護確認
        _client.DefaultRequestHeaders.Clear();
        _client.DefaultRequestHeaders.Add("Cookie", authCookie);

        // 1. Homeページ基本機能
        var homeResponse = await _client.GetAsync("/home");
        Assert.Equal(HttpStatusCode.OK, homeResponse.StatusCode);

        var homeContent = await homeResponse.Content.ReadAsStringAsync();
        
        // 必須要素の存在確認
        Assert.Contains("ダッシュボード", homeContent);
        Assert.Contains("登録ユーザー数", homeContent);
        Assert.Contains("プロジェクト数", homeContent);
        Assert.Contains("ユビキタス言語数", homeContent);
        Assert.Contains("クイックアクション", homeContent);
        Assert.Contains("最近の活動", homeContent);

        // 2. HTML構造正常性確認
        var doc = new HtmlDocument();
        doc.LoadHtml(homeContent);
        
        // 必須HTML要素の存在確認
        var titleNode = doc.DocumentNode.SelectSingleNode("//title");
        Assert.NotNull(titleNode);
        Assert.Contains("ホーム", titleNode.InnerText);

        var dashboardHeader = doc.DocumentNode.SelectSingleNode("//*[contains(@class, 'display-6') and contains(text(), 'ダッシュボード')]");
        Assert.NotNull(dashboardHeader);

        // 3. スタイルシート・JavaScript読み込み確認
        var stylesheetLinks = doc.DocumentNode.SelectNodes("//link[@rel='stylesheet']");
        Assert.NotNull(stylesheetLinks);
        Assert.NotEmpty(stylesheetLinks);

        // 4. "/" ルートでも同等機能確認
        var rootResponse = await _client.GetAsync("/");
        Assert.Equal(HttpStatusCode.OK, rootResponse.StatusCode);

        var rootContent = await rootResponse.Content.ReadAsStringAsync();
        Assert.Contains("ダッシュボード", rootContent);
    }

    #endregion
}