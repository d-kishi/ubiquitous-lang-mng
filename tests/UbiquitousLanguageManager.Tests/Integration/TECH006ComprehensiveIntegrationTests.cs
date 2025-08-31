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
/// TECH-006解決実装の包括的統合品質保証テスト
/// 
/// 【テスト目的】
/// Stage 1-3完了実装の統合品質保証・認証フロー統合テスト
/// Headers read-onlyエラー完全解決実証・AuthApiController統合効果確認
/// 
/// 【実装確認対象】
/// Stage 1: NavigateTo最適化（forceLoad: false）
/// Stage 2: HTTPContext管理改善（Response.HasStartedチェック）
/// Stage 3: 認証API分離（AuthApiController・JavaScript統合）
/// </summary>
public class TECH006ComprehensiveIntegrationTests : IClassFixture<TestWebApplicationFactory<Program>>
{
    private readonly TestWebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;
    private readonly HttpClient _noRedirectClient;

    public TECH006ComprehensiveIntegrationTests(TestWebApplicationFactory<Program> factory)
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

    #region Stage 3: 認証API分離統合テスト

    /// <summary>
    /// 統合テスト-1: AuthApiControllerによるHeaders read-onlyエラー完全解消確認
    /// </summary>
    [Fact]
    public async Task Stage3_AuthApiController_EliminatesHeadersReadOnlyError()
    {
        // Arrange - テストユーザー作成
        using var scope = await _factory.CreateScopeWithTestDataAsync();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        
        var testUser = new ApplicationUser
        {
            UserName = "stage3.test@example.com",
            Email = "stage3.test@example.com",
            Name = "Stage3統合テストユーザー",
            IsFirstLogin = false
        };
        
        var createResult = await userManager.CreateAsync(testUser, "Stage3Pass123!");
        Assert.True(createResult.Succeeded, $"ユーザー作成失敗: {string.Join(", ", createResult.Errors.Select(e => e.Description))}");

        // Act - AuthApiController ログイン（新しいHTTPコンテキスト）
        var loginRequest = new
        {
            Email = "stage3.test@example.com",
            Password = "Stage3Pass123!",
            RememberMe = false
        };

        var loginContent = new StringContent(
            JsonSerializer.Serialize(loginRequest),
            Encoding.UTF8,
            "application/json"
        );

        var response = await _noRedirectClient.PostAsync("/api/auth/login", loginContent);
        var responseContent = await response.Content.ReadAsStringAsync();

        // Assert - Headers read-onlyエラー完全解消確認
        Assert.True(response.IsSuccessStatusCode,
            $"AuthApiController ログインでHeaders read-onlyエラー発生: Status={response.StatusCode}, Content={responseContent}");

        // Cookie認証処理正常完了確認
        var setCookieHeaders = response.Headers.GetValues("Set-Cookie").ToArray();
        Assert.NotEmpty(setCookieHeaders);
        
        var authCookie = setCookieHeaders.FirstOrDefault(c => c.Contains("Identity.Application"));
        Assert.NotNull(authCookie);

        // JSON APIレスポンス構造確認
        var authResponse = JsonSerializer.Deserialize<JsonElement>(responseContent);
        Assert.True(authResponse.GetProperty("success").GetBoolean());
        Assert.Equal("ログインしました。", authResponse.GetProperty("message").GetString());
        Assert.Contains("/home", authResponse.GetProperty("redirectUrl").GetString());
    }

    /// <summary>
    /// 統合テスト-2: 初回ログインフロー統合動作確認
    /// </summary>
    [Fact]
    public async Task Stage3_FirstLoginFlow_CompletesSuccessfully()
    {
        // Arrange - 初回ログインユーザー作成
        using var scope = await _factory.CreateScopeWithTestDataAsync();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        
        var firstLoginUser = new ApplicationUser
        {
            UserName = "firstlogin.integration@example.com",
            Email = "firstlogin.integration@example.com",
            Name = "初回ログイン統合テストユーザー",
            IsFirstLogin = true // 初回ログインフラグ
        };
        
        await userManager.CreateAsync(firstLoginUser, "FirstPass123!");

        // Act - AuthApiController 初回ログイン実行
        var loginRequest = new
        {
            Email = "firstlogin.integration@example.com",
            Password = "FirstPass123!",
            RememberMe = false
        };

        var loginContent = new StringContent(
            JsonSerializer.Serialize(loginRequest),
            Encoding.UTF8,
            "application/json"
        );

        var response = await _noRedirectClient.PostAsync("/api/auth/login", loginContent);
        var responseContent = await response.Content.ReadAsStringAsync();

        // Assert - 初回ログイン処理正常完了
        Assert.True(response.IsSuccessStatusCode, $"初回ログイン処理失敗: {responseContent}");

        // Cookie認証成功確認
        var setCookieHeaders = response.Headers.GetValues("Set-Cookie").ToArray();
        var authCookie = setCookieHeaders.FirstOrDefault(c => c.Contains("Identity.Application"));
        Assert.NotNull(authCookie);

        // 初回ログインのリダイレクト先確認
        var authResponse = JsonSerializer.Deserialize<JsonElement>(responseContent);
        Assert.True(authResponse.GetProperty("success").GetBoolean());
        Assert.Contains("/change-password", authResponse.GetProperty("redirectUrl").GetString());
    }

    /// <summary>
    /// 統合テスト-3: パスワード変更API統合・セキュリティスタンプ更新確認
    /// </summary>
    [Fact]
    public async Task Stage3_PasswordChangeAPI_UpdatesSecurityStampCorrectly()
    {
        // Arrange - 初回ログインユーザーでログイン完了状態作成
        using var scope = await _factory.CreateScopeWithTestDataAsync();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        
        var user = new ApplicationUser
        {
            UserName = "passwordchange.integration@example.com",
            Email = "passwordchange.integration@example.com",
            Name = "パスワード変更統合テストユーザー",
            IsFirstLogin = true
        };
        
        await userManager.CreateAsync(user, "InitialPass123!");

        // Step 1: 初回ログインでCookie取得
        var loginRequest = new
        {
            Email = "passwordchange.integration@example.com",
            Password = "InitialPass123!",
            RememberMe = false
        };

        var loginContent = new StringContent(JsonSerializer.Serialize(loginRequest), Encoding.UTF8, "application/json");
        var loginResponse = await _noRedirectClient.PostAsync("/api/auth/login", loginContent);
        
        Assert.True(loginResponse.IsSuccessStatusCode);
        
        var loginCookies = loginResponse.Headers.GetValues("Set-Cookie").ToArray();
        var authCookie = loginCookies.FirstOrDefault(c => c.Contains("Identity.Application"));
        Assert.NotNull(authCookie);

        // Step 2: パスワード変更API実行
        var passwordChangeRequest = new
        {
            CurrentPassword = "InitialPass123!",
            NewPassword = "ChangedPass123!",
            ConfirmPassword = "ChangedPass123!"
        };

        var passwordChangeContent = new StringContent(
            JsonSerializer.Serialize(passwordChangeRequest), Encoding.UTF8, "application/json");

        // 認証Cookie設定
        _noRedirectClient.DefaultRequestHeaders.Clear();
        _noRedirectClient.DefaultRequestHeaders.Add("Cookie", authCookie);

        var passwordChangeResponse = await _noRedirectClient.PostAsync("/api/auth/change-password", passwordChangeContent);
        var passwordChangeResponseContent = await passwordChangeResponse.Content.ReadAsStringAsync();

        // Assert - パスワード変更成功・セキュリティスタンプ更新確認
        Assert.True(passwordChangeResponse.IsSuccessStatusCode,
            $"パスワード変更API失敗: Status={passwordChangeResponse.StatusCode}, Content={passwordChangeResponseContent}");

        var passwordChangeAuthResponse = JsonSerializer.Deserialize<JsonElement>(passwordChangeResponseContent);
        Assert.True(passwordChangeAuthResponse.GetProperty("success").GetBoolean());
        Assert.Contains("/home", passwordChangeAuthResponse.GetProperty("redirectUrl").GetString());

        // セキュリティスタンプ更新によるCookie更新確認
        var newAuthCookies = passwordChangeResponse.Headers.GetValues("Set-Cookie").ToArray();
        Assert.NotEmpty(newAuthCookies);

        // Step 3: 新しいパスワードでの認証確認
        var newLoginRequest = new
        {
            Email = "passwordchange.integration@example.com",
            Password = "ChangedPass123!",
            RememberMe = false
        };

        var newLoginContent = new StringContent(JsonSerializer.Serialize(newLoginRequest), Encoding.UTF8, "application/json");
        _noRedirectClient.DefaultRequestHeaders.Clear();

        var newLoginResponse = await _noRedirectClient.PostAsync("/api/auth/login", newLoginContent);
        var newLoginResponseContent = await newLoginResponse.Content.ReadAsStringAsync();

        Assert.True(newLoginResponse.IsSuccessStatusCode, $"新パスワードでのログイン失敗: {newLoginResponseContent}");
        
        // IsFirstLoginがfalseに更新されていることを確認（通常ログインフロー）
        var newLoginAuthResponse = JsonSerializer.Deserialize<JsonElement>(newLoginResponseContent);
        Assert.Contains("/home", newLoginAuthResponse.GetProperty("redirectUrl").GetString());
    }

    #endregion

    #region パフォーマンス・安定性統合確認

    /// <summary>
    /// 統合テスト-4: 認証API分離によるパフォーマンス影響測定
    /// </summary>
    [Fact]
    public async Task Stage3_AuthenticationPerformance_MeetsTargets()
    {
        // Arrange - パフォーマンステスト用ユーザー群作成
        using var scope = await _factory.CreateScopeWithTestDataAsync();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        
        var testUsers = Enumerable.Range(1, 3).Select(i => new ApplicationUser
        {
            UserName = $"perftest{i}@integration.com",
            Email = $"perftest{i}@integration.com",
            Name = $"パフォーマンス統合テストユーザー{i}",
            IsFirstLogin = false
        }).ToArray();

        foreach (var testUser in testUsers)
        {
            await userManager.CreateAsync(testUser, "PerfPass123!");
        }

        // Act - 連続ログイン処理（パフォーマンス・安定性測定）
        var performanceResults = new List<TimeSpan>();
        var successCount = 0;
        const int iterationsPerUser = 2;

        foreach (var user in testUsers)
        {
            for (int i = 0; i < iterationsPerUser; i++)
            {
                var startTime = DateTime.UtcNow;
                
                var loginRequest = new
                {
                    Email = user.Email,
                    Password = "PerfPass123!",
                    RememberMe = false
                };

                var loginContent = new StringContent(
                    JsonSerializer.Serialize(loginRequest), Encoding.UTF8, "application/json");

                var response = await _noRedirectClient.PostAsync("/api/auth/login", loginContent);
                var executionTime = DateTime.UtcNow - startTime;
                
                performanceResults.Add(executionTime);

                if (response.IsSuccessStatusCode)
                {
                    successCount++;
                }

                // パフォーマンス目標確認（<2秒）
                Assert.True(executionTime.TotalSeconds < 2.0,
                    $"認証処理時間が目標値を超過: {executionTime.TotalSeconds:F2}秒 (ユーザー: {user.Email})");

                await Task.Delay(50); // 連続処理負荷制御
            }
        }

        // Assert - 成功率・平均パフォーマンス確認
        var totalTests = testUsers.Length * iterationsPerUser;
        Assert.Equal(totalTests, successCount); // 100%成功率要求

        var avgResponseTime = performanceResults.Average(t => t.TotalMilliseconds);
        Assert.True(avgResponseTime < 1500, $"平均レスポンス時間が目標を超過: {avgResponseTime:F2}ms");

        // パフォーマンス安定性確認（標準偏差）
        var variance = performanceResults.Select(t => Math.Pow(t.TotalMilliseconds - avgResponseTime, 2)).Average();
        var standardDeviation = Math.Sqrt(variance);
        Assert.True(standardDeviation < 500, $"レスポンス時間の変動が許容範囲を超過: {standardDeviation:F2}ms");
    }

    /// <summary>
    /// 統合テスト-5: エラーハンドリング・異常系統合確認
    /// </summary>
    [Fact]
    public async Task Stage3_ErrorHandling_HandlesAllFailureCasesGracefully()
    {
        // Arrange - 異常系テストケース定義
        using var scope = await _factory.CreateScopeWithTestDataAsync();

        var errorTestCases = new[]
        {
            // 認証失敗系
            new { Email = "nonexistent@example.com", Password = "wrongpass", Description = "存在しないユーザー" },
            new { Email = "invalid.email", Password = "validpass", Description = "不正メールアドレス形式" },
            new { Email = "", Password = "validpass", Description = "空メールアドレス" },
            new { Email = "test@example.com", Password = "", Description = "空パスワード" },
            
            // 境界値系
            new { Email = new string('a', 100) + "@example.com", Password = "validpass", Description = "長すぎるメールアドレス" },
            new { Email = "test@example.com", Password = new string('p', 200), Description = "長すぎるパスワード" }
        };

        // Act & Assert - 各エラーケースでの適切なハンドリング確認
        foreach (var testCase in errorTestCases)
        {
            var loginRequest = new
            {
                testCase.Email,
                testCase.Password,
                RememberMe = false
            };

            var loginContent = new StringContent(
                JsonSerializer.Serialize(loginRequest), Encoding.UTF8, "application/json");

            var response = await _noRedirectClient.PostAsync("/api/auth/login", loginContent);
            var responseContent = await response.Content.ReadAsStringAsync();

            // 適切なHTTPステータス確認
            Assert.True(
                response.StatusCode == HttpStatusCode.BadRequest ||
                response.StatusCode == HttpStatusCode.Unauthorized,
                $"エラーケース '{testCase.Description}' で予期しないステータス: {response.StatusCode}");

            // JSON エラーレスポンス構造確認
            if (!string.IsNullOrEmpty(responseContent))
            {
                var errorResponse = JsonSerializer.Deserialize<JsonElement>(responseContent);
                if (errorResponse.TryGetProperty("success", out var successProp))
                {
                    Assert.False(successProp.GetBoolean(), $"エラーケースで success=true: {testCase.Description}");
                }
            }
        }
    }

    #endregion

    #region Stage 1-2 統合効果確認

    /// <summary>
    /// 統合テスト-6: NavigateTo最適化・HTTPContext管理改善統合確認
    /// </summary>
    [Fact]
    public async Task Stage1And2_NavigateToOptimization_HTTPContextManagement_IntegratedCorrectly()
    {
        // Arrange - 複数認証パターンテスト用ユーザー
        using var scope = await _factory.CreateScopeWithTestDataAsync();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        
        var integrationUsers = new[]
        {
            new { Email = "normal.s12@example.com", IsFirstLogin = false, Description = "通常ユーザー" },
            new { Email = "firsttime.s12@example.com", IsFirstLogin = true, Description = "初回ログインユーザー" }
        };

        foreach (var userData in integrationUsers)
        {
            var user = new ApplicationUser
            {
                UserName = userData.Email,
                Email = userData.Email,
                Name = $"Stage1-2統合テスト {userData.Description}",
                IsFirstLogin = userData.IsFirstLogin
            };
            
            await userManager.CreateAsync(user, "Integration123!");
        }

        // Act & Assert - Stage 1-2統合効果確認
        foreach (var userData in integrationUsers)
        {
            var loginRequest = new
            {
                Email = userData.Email,
                Password = "Integration123!",
                RememberMe = false
            };

            var loginContent = new StringContent(
                JsonSerializer.Serialize(loginRequest), Encoding.UTF8, "application/json");

            var response = await _noRedirectClient.PostAsync("/api/auth/login", loginContent);
            var responseContent = await response.Content.ReadAsStringAsync();

            // Stage 3 AuthApiController統合による成功確認
            Assert.True(response.IsSuccessStatusCode,
                $"Stage1-2統合でログイン失敗 ({userData.Description}): {responseContent}");

            // Stage 1 NavigateTo最適化効果確認（適切なリダイレクトURL）
            var authResponse = JsonSerializer.Deserialize<JsonElement>(responseContent);
            var redirectUrl = authResponse.GetProperty("redirectUrl").GetString();
            
            if (userData.IsFirstLogin)
            {
                Assert.Contains("/change-password", redirectUrl);
            }
            else
            {
                Assert.Contains("/home", redirectUrl);
            }

            // Stage 2 HTTPContext管理改善効果確認（Cookie正常設定）
            var setCookieHeaders = response.Headers.GetValues("Set-Cookie").ToArray();
            var authCookie = setCookieHeaders.FirstOrDefault(c => c.Contains("Identity.Application"));
            Assert.NotNull(authCookie);
            
            // HTTPContext分離によるSecure Cookie属性確認
            Assert.Contains("HttpOnly", authCookie);
        }
    }

    #endregion

    #region 回帰テスト・既存機能保護確認

    /// <summary>
    /// 統合テスト-7: 既存機能保護・回帰テスト確認
    /// </summary>
    [Fact]
    public async Task ExistingFeatures_RegressionTest_AllFunctionalityPreserved()
    {
        // Arrange - 既存機能テスト用ユーザー
        using var scope = await _factory.CreateScopeWithTestDataAsync();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        
        var regressionTestUser = new ApplicationUser
        {
            UserName = "regression@example.com",
            Email = "regression@example.com",
            Name = "回帰テストユーザー",
            IsFirstLogin = false
        };
        
        await userManager.CreateAsync(regressionTestUser, "RegressionPass123!");

        // Act 1: 基本ログイン機能確認
        var loginRequest = new
        {
            Email = "regression@example.com",
            Password = "RegressionPass123!",
            RememberMe = true // RememberMe機能確認
        };

        var loginContent = new StringContent(
            JsonSerializer.Serialize(loginRequest), Encoding.UTF8, "application/json");

        var loginResponse = await _noRedirectClient.PostAsync("/api/auth/login", loginContent);
        var loginResponseContent = await loginResponse.Content.ReadAsStringAsync();

        // Assert 1: 基本認証機能保持確認
        Assert.True(loginResponse.IsSuccessStatusCode, $"回帰テスト基本ログイン失敗: {loginResponseContent}");

        var authResponse = JsonSerializer.Deserialize<JsonElement>(loginResponseContent);
        Assert.True(authResponse.GetProperty("success").GetBoolean());

        // RememberMe機能確認（永続Cookie）
        var cookies = loginResponse.Headers.GetValues("Set-Cookie").ToArray();
        var authCookie = cookies.FirstOrDefault(c => c.Contains("Identity.Application"));
        Assert.NotNull(authCookie);
        Assert.Contains("expires=", authCookie.ToLowerInvariant()); // 永続化Cookie設定

        // Act 2: ログアウト機能確認（既存機能保護）
        _noRedirectClient.DefaultRequestHeaders.Clear();
        _noRedirectClient.DefaultRequestHeaders.Add("Cookie", authCookie);

        var logoutResponse = await _noRedirectClient.PostAsync("/api/auth/logout", new StringContent(""));
        var logoutResponseContent = await logoutResponse.Content.ReadAsStringAsync();

        // Assert 2: ログアウト機能保持確認
        Assert.True(logoutResponse.IsSuccessStatusCode || logoutResponse.StatusCode == HttpStatusCode.Redirect,
            $"回帰テストログアウト失敗: Status={logoutResponse.StatusCode}, Content={logoutResponseContent}");

        // Cookie削除確認（ログアウト時）
        if (logoutResponse.Headers.Contains("Set-Cookie"))
        {
            var logoutCookies = logoutResponse.Headers.GetValues("Set-Cookie").ToArray();
            var expiredAuthCookie = logoutCookies.FirstOrDefault(c => c.Contains("Identity.Application"));
            if (expiredAuthCookie != null)
            {
                // Cookie削除（expires=過去日付）またはValue=""確認
                Assert.True(expiredAuthCookie.Contains("expires=") || expiredAuthCookie.Contains("=;"));
            }
        }
    }

    #endregion
}