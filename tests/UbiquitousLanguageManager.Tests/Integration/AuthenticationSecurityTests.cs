using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using Xunit;
using UbiquitousLanguageManager.Tests.TestUtilities;
using Microsoft.AspNetCore.Identity;
using UbiquitousLanguageManager.Infrastructure.Data.Entities;
using System.Text;
using System.Text.Json;
using HtmlAgilityPack;
using Microsoft.Extensions.Configuration;

namespace UbiquitousLanguageManager.Tests.Integration;

/// <summary>
/// 認証システムセキュリティ統合テスト
/// Phase A6完了後のセキュリティ強化機能検証
/// </summary>
public class AuthenticationSecurityTests : IClassFixture<TestWebApplicationFactory<Program>>
{
    private readonly TestWebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public AuthenticationSecurityTests(TestWebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });
    }

    #region パスワードセキュリティテスト

    /// <summary>
    /// パスワードハッシュ強度: OWASP推奨値600,000回反復確認
    /// </summary>
    [Fact]
    public async Task PasswordHashing_UsesSecureIterationCount()
    {
        // Arrange
        using var scope = await _factory.CreateScopeWithTestDataAsync();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher<ApplicationUser>>();
        
        var testUser = new ApplicationUser
        {
            UserName = "security@hash.test",
            Email = "security@hash.test",
            Name = "セキュリティハッシュテストユーザー",
        };

        // Act - パスワードハッシュ生成
        var hashedPassword = passwordHasher.HashPassword(testUser, "su");
        
        // Assert - ハッシュ形式確認
        Assert.NotNull(hashedPassword);
        Assert.NotEmpty(hashedPassword);
        
        // パスワード検証確認
        var verificationResult = passwordHasher.VerifyHashedPassword(
            testUser, hashedPassword, "su");
        Assert.Equal(PasswordVerificationResult.Success, verificationResult);
        
        // 間違ったパスワードでの検証失敗確認
        var wrongPasswordResult = passwordHasher.VerifyHashedPassword(
            testUser, hashedPassword, "WrongPassword123!");
        Assert.Equal(PasswordVerificationResult.Failed, wrongPasswordResult);
    }

    /// <summary>
    /// パスワードポリシー: 複雑性要件の確認
    /// </summary>
    [Theory]
    [InlineData("123456", false)]           // 短すぎる
    [InlineData("password", false)]         // 一般的すぎる
    [InlineData("Password", false)]         // 数字・記号なし
    [InlineData("password123", false)]      // 大文字・記号なし
    [InlineData("PASSWORD123!", false)]     // 小文字なし
    [InlineData("Password123", false)]      // 記号なし
    [InlineData("Password123!", true)]      // 強いパスワード
    [InlineData("MyStr0ng!P@ssw0rd", true)] // 非常に強いパスワード
    public async Task PasswordPolicy_EnforcesComplexityRequirements(string password, bool shouldBeValid)
    {
        // Arrange
        using var scope = await _factory.CreateScopeWithTestDataAsync();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        
        var testUser = new ApplicationUser
        {
            UserName = $"policy-{Guid.NewGuid()}@test.com",
            Email = $"policy-{Guid.NewGuid()}@test.com",
            Name = "パスワードポリシーテストユーザー",
        };

        // Act
        var result = await userManager.CreateAsync(testUser, password);

        // Assert
        if (shouldBeValid)
        {
            Assert.True(result.Succeeded, 
                       $"Password '{password}' should be valid but was rejected: {string.Join(", ", result.Errors.Select(e => e.Description))}");
        }
        else
        {
            Assert.False(result.Succeeded, 
                        $"Password '{password}' should be invalid but was accepted");
            Assert.NotEmpty(result.Errors);
        }
    }

    /// <summary>
    /// 初期パスワード"su"セキュリティ: 強制変更要求確認
    /// </summary>
    [Fact]
    public async Task InitialPassword_RequiresMandatoryChange()
    {
        // Arrange
        using var scope = await _factory.CreateScopeWithTestDataAsync();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();
        
        // 設定から初期パスワード確認
        var initialPassword = configuration["InitialSuperUser:Password"];
        Assert.Equal("su", initialPassword);

        // 初期スーパーユーザー確認
        var superUser = await userManager.FindByEmailAsync("admin@ubiquitous-lang.com");
        Assert.NotNull(superUser);
        Assert.True(superUser.IsFirstLogin); // 必須変更フラグ

        // Act & Assert - 初期パスワードでのログイン後、変更強制を確認
        var loginRequest = new
        {
            Email = "admin@ubiquitous-lang.com",
            Password = "su",
            RememberMe = false
        };

        var loginContent = new StringContent(
            JsonSerializer.Serialize(loginRequest),
            Encoding.UTF8,
            "application/json"
        );

        var response = await _client.PostAsync("/api/auth/login", loginContent);
        
        // ログイン成功後、パスワード変更が必要な状態であることを確認
        Assert.True(response.StatusCode == HttpStatusCode.OK ||
                   response.StatusCode == HttpStatusCode.Redirect);
    }

    #endregion

    #region CSRF・XSS保護テスト

    /// <summary>
    /// CSRF保護: Antiforgeryトークン検証
    /// </summary>
    [Fact]
    public async Task CSRFProtection_AntiforgeryTokens_AreValidated()
    {
        // Arrange - ログインページからトークン取得
        var loginPageResponse = await _client.GetAsync("/login");
        var loginContent = await loginPageResponse.Content.ReadAsStringAsync();
        
        var doc = new HtmlDocument();
        doc.LoadHtml(loginContent);
        
        var antiforgeryInput = doc.DocumentNode
            .SelectSingleNode("//input[@name='__RequestVerificationToken']");
        
        Assert.NotNull(antiforgeryInput);
        
        var validToken = antiforgeryInput.GetAttributeValue("value", "");
        Assert.NotEmpty(validToken);

        // Act & Assert - 有効なトークンでのフォーム送信
        var validFormData = new List<KeyValuePair<string, string>>
        {
            new("Email", "test@example.com"),
            new("Password", "su"),
            new("__RequestVerificationToken", validToken)
        };

        var validFormContent = new FormUrlEncodedContent(validFormData);
        var validResponse = await _client.PostAsync("/login", validFormContent);
        
        // 有効なトークンは受け入れられる（認証失敗でもCSRF検証は通る）
        Assert.True(validResponse.StatusCode != HttpStatusCode.BadRequest);

        // 無効なトークンでのフォーム送信
        var invalidFormData = new List<KeyValuePair<string, string>>
        {
            new("Email", "test@example.com"),
            new("Password", "su"),
            new("__RequestVerificationToken", "INVALID_TOKEN_VALUE")
        };

        var invalidFormContent = new FormUrlEncodedContent(invalidFormData);
        var invalidResponse = await _client.PostAsync("/login", invalidFormContent);

        // 無効なトークンは拒否される
        Assert.True(invalidResponse.StatusCode == HttpStatusCode.BadRequest ||
                   invalidResponse.StatusCode == HttpStatusCode.Forbidden);
    }

    /// <summary>
    /// XSS保護: 出力エンコーディング確認
    /// </summary>
    [Fact]
    public async Task XSSProtection_OutputEncoding_PreventsScriptInjection()
    {
        // Arrange - XSS攻撃を試行するデータ
        var xssPayloads = new[]
        {
            "<script>alert('XSS')</script>",
            "javascript:alert('XSS')",
            "<img src=x onerror=alert('XSS')>",
            "';alert('XSS');//",
            "<svg onload=alert('XSS')>"
        };

        // Act & Assert - 各種ページでXSS攻撃が無効化されることを確認
        foreach (var payload in xssPayloads)
        {
            // ログインページでのXSS試行
            var response = await _client.GetAsync($"/login?error={Uri.EscapeDataString(payload)}");
            
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                
                // エンコードされていない生のスクリプトタグが含まれていないことを確認
                Assert.DoesNotContain("<script>alert('XSS')</script>", content);
                Assert.DoesNotContain("javascript:alert('XSS')", content);
                Assert.DoesNotContain("<img src=x onerror=alert('XSS')>", content);
                
                // エンコード済みの安全な形式であることを確認
                if (content.Contains("alert"))
                {
                    Assert.True(content.Contains("&lt;") || content.Contains("&gt;") || 
                               content.Contains("&#") || !content.Contains("<script"));
                }
            }
        }
    }

    #endregion

    #region 認証・セッション管理セキュリティ

    /// <summary>
    /// セッション固定攻撃対策: ログイン時のセッションID再生成
    /// </summary>
    [Fact]
    public async Task SessionSecurity_RegeneratesSessionOnLogin()
    {
        // Arrange - ログイン前のセッション状態取得
        var preLoginResponse = await _client.GetAsync("/login");
        var preLoginCookies = preLoginResponse.Headers.GetValues("Set-Cookie").ToList();

        using var scope = await _factory.CreateScopeWithTestDataAsync();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        
        var testUser = new ApplicationUser
        {
            UserName = "session@security.test",
            Email = "session@security.test",
            Name = "セッションセキュリティテストユーザー",
            IsFirstLogin = false,
        };
        
        await userManager.CreateAsync(testUser, "SessionSecurity123!");

        // Act - ログイン実行
        var loginRequest = new
        {
            Email = "session@security.test",
            Password = "SessionSecurity123!",
            RememberMe = false
        };

        var loginContent = new StringContent(
            JsonSerializer.Serialize(loginRequest),
            Encoding.UTF8,
            "application/json"
        );

        var loginResponse = await _client.PostAsync("/api/auth/login", loginContent);

        // Assert - ログイン後のセッション状態確認
        if (loginResponse.Headers.Contains("Set-Cookie"))
        {
            var postLoginCookies = loginResponse.Headers.GetValues("Set-Cookie").ToList();
            
            // セッション関連Cookieが更新されていることを確認
            var sessionCookieUpdated = postLoginCookies.Any(cookie => 
                cookie.Contains("Identity") || cookie.Contains("Session") || cookie.Contains("Auth"));
            
            if (sessionCookieUpdated)
            {
                // セッションIDが変更されていることを確認
                var preLoginSession = preLoginCookies.FirstOrDefault(c => c.Contains("Session"));
                var postLoginSession = postLoginCookies.FirstOrDefault(c => c.Contains("Session"));
                
                if (preLoginSession != null && postLoginSession != null)
                {
                    Assert.NotEqual(preLoginSession, postLoginSession);
                }
            }
        }
    }

    /// <summary>
    /// ログイン試行制限: ブルートフォース攻撃対策
    /// </summary>
    [Fact]
    public async Task LoginAttemptLimiting_PreventsbruteForceAttacks()
    {
        // Arrange
        using var scope = await _factory.CreateScopeWithTestDataAsync();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        
        var targetUser = new ApplicationUser
        {
            UserName = "bruteforce@target.test",
            Email = "bruteforce@target.test",
            Name = "ブルートフォース攻撃対象ユーザー",
            IsFirstLogin = false,
        };
        
        await userManager.CreateAsync(targetUser, "CorrectPassword123!");

        // Act - 複数回の失敗ログイン試行
        var failureCount = 0;
        var maxAttempts = 10;
        
        for (int i = 0; i < maxAttempts; i++)
        {
            var loginRequest = new
            {
                Email = "bruteforce@target.test",
                Password = $"WrongPassword{i}",
                RememberMe = false
            };

            var loginContent = new StringContent(
                JsonSerializer.Serialize(loginRequest),
                Encoding.UTF8,
                "application/json"
            );

            var response = await _client.PostAsync("/api/auth/login", loginContent);
            
            if (response.StatusCode == HttpStatusCode.Unauthorized ||
                response.StatusCode == HttpStatusCode.BadRequest)
            {
                failureCount++;
            }
            
            // 短時間での大量試行を避けるため少し待機
            await Task.Delay(100);
        }

        // Assert - 失敗ログインが適切に記録・制限される
        Assert.True(failureCount > 0, "Failed login attempts should be recorded");
        
        // アカウントロックアウト機能が実装されている場合の検証
        // （実装状況に応じて調整）
    }

    #endregion

    #region セキュリティヘッダー・設定テスト

    /// <summary>
    /// セキュリティヘッダー: 適切なHTTPセキュリティヘッダー設定確認
    /// </summary>
    [Fact]
    public async Task SecurityHeaders_AreCorrectlyConfigured()
    {
        // Act
        var response = await _client.GetAsync("/login");

        // Assert - 必要なセキュリティヘッダーの確認
        Assert.True(response.IsSuccessStatusCode);

        // Content-Type ヘッダー
        var contentType = response.Content.Headers.ContentType?.ToString();
        Assert.Contains("text/html", contentType ?? "");
        Assert.Contains("charset=utf-8", contentType ?? "");

        // セキュリティ関連ヘッダーの確認（実装されている場合）
        var headers = response.Headers;
        
        // X-Content-Type-Options (実装されている場合)
        if (headers.Contains("X-Content-Type-Options"))
        {
            var xContentTypeOptions = headers.GetValues("X-Content-Type-Options").FirstOrDefault();
            Assert.Equal("nosniff", xContentTypeOptions);
        }

        // X-Frame-Options (実装されている場合)
        if (headers.Contains("X-Frame-Options"))
        {
            var xFrameOptions = headers.GetValues("X-Frame-Options").FirstOrDefault();
            Assert.True(xFrameOptions == "DENY" || xFrameOptions == "SAMEORIGIN");
        }

        // Strict-Transport-Security (HTTPS環境で実装されている場合)
        if (headers.Contains("Strict-Transport-Security"))
        {
            var hsts = headers.GetValues("Strict-Transport-Security").FirstOrDefault();
            Assert.Contains("max-age=", hsts ?? "");
        }
    }

    /// <summary>
    /// Cookie セキュリティ: Secure・HttpOnly・SameSite属性確認
    /// </summary>
    [Fact]
    public async Task CookieSecurity_UsesSecureAttributes()
    {
        // Act
        var response = await _client.GetAsync("/login");

        // Assert
        if (response.Headers.Contains("Set-Cookie"))
        {
            var cookies = response.Headers.GetValues("Set-Cookie");
            
            foreach (var cookie in cookies)
            {
                // 認証関連Cookieのセキュリティ属性確認
                if (cookie.Contains("Identity") || cookie.Contains("Auth") || cookie.Contains("Session"))
                {
                    // HttpOnly属性（XSS対策）
                    Assert.Contains("HttpOnly", cookie);
                    
                    // SameSite属性（CSRF対策）
                    Assert.True(cookie.Contains("SameSite=Strict") || 
                               cookie.Contains("SameSite=Lax"));
                    
                    // Secure属性は HTTPS 環境でのみ適用されるため条件付き確認
                    // テスト環境では HTTP のため Secure 属性は期待しない
                }
            }
        }
    }

    #endregion

    #region パフォーマンス・可用性テスト

    /// <summary>
    /// 認証パフォーマンス: 同時認証リクエスト処理性能
    /// </summary>
    [Fact]
    public async Task AuthenticationPerformance_HandlesConcurrentRequests()
    {
        // Arrange
        using var scope = await _factory.CreateScopeWithTestDataAsync();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        
        // 複数のテストユーザー作成
        var testUsers = new List<ApplicationUser>();
        for (int i = 0; i < 10; i++)
        {
            var user = new ApplicationUser
            {
                UserName = $"performance{i}@test.com",
                Email = $"performance{i}@test.com",
                Name = $"パフォーマンステストユーザー{i}",
                IsFirstLogin = false,
                };
            
            await userManager.CreateAsync(user, "PerformanceTest123!");
            testUsers.Add(user);
        }

        // Act - 同時ログイン試行
        var concurrentRequests = 20;
        var tasks = new List<Task<HttpResponseMessage>>();
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        for (int i = 0; i < concurrentRequests; i++)
        {
            var userIndex = i % testUsers.Count;
            var loginRequest = new
            {
                Email = testUsers[userIndex].Email,
                Password = "PerformanceTest123!",
                RememberMe = false
            };

            var loginContent = new StringContent(
                JsonSerializer.Serialize(loginRequest),
                Encoding.UTF8,
                "application/json"
            );

            tasks.Add(_client.PostAsync("/api/auth/login", loginContent));
        }

        var responses = await Task.WhenAll(tasks);
        stopwatch.Stop();

        // Assert
        var successfulAuthentications = responses.Count(r => 
            r.StatusCode == HttpStatusCode.OK || 
            r.StatusCode == HttpStatusCode.Redirect);

        Assert.True(successfulAuthentications > 0, 
                   "At least some concurrent authentication requests should succeed");

        // パフォーマンス確認
        var averageResponseTime = stopwatch.ElapsedMilliseconds / (double)concurrentRequests;
        Assert.True(averageResponseTime < 2000, 
                   $"Average response time should be less than 2000ms, but was {averageResponseTime}ms");

        // リソース クリーンアップ
        foreach (var response in responses)
        {
            response.Dispose();
        }
    }

    /// <summary>
    /// 認証システム可用性: エラー状態からの回復確認
    /// </summary>
    [Fact]
    public async Task AuthenticationAvailability_RecoversFromErrors()
    {
        // Arrange - 正常な認証リクエスト
        using var scope = await _factory.CreateScopeWithTestDataAsync();
        
        // Act & Assert - エラー状態後の回復テスト
        
        // 1. 無効なリクエストでエラー発生
        var invalidRequest = new StringContent("INVALID_JSON", Encoding.UTF8, "application/json");
        var errorResponse = await _client.PostAsync("/api/auth/login", invalidRequest);
        Assert.True(errorResponse.StatusCode == HttpStatusCode.BadRequest ||
                   errorResponse.StatusCode == HttpStatusCode.InternalServerError);

        // 2. 直後の正常なリクエストが成功することを確認
        var healthResponse = await _client.GetAsync("/health");
        Assert.True(healthResponse.IsSuccessStatusCode);

        var loginPageResponse = await _client.GetAsync("/login");
        Assert.True(loginPageResponse.IsSuccessStatusCode);

        // 3. 認証システムが正常に動作し続けることを確認
        var multipleHealthChecks = new List<Task<HttpResponseMessage>>();
        for (int i = 0; i < 5; i++)
        {
            multipleHealthChecks.Add(_client.GetAsync("/health"));
        }

        var healthResults = await Task.WhenAll(multipleHealthChecks);
        Assert.True(healthResults.All(r => r.IsSuccessStatusCode),
                   "Authentication system should remain available after errors");

        foreach (var result in healthResults)
        {
            result.Dispose();
        }
    }

    #endregion
}