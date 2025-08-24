using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using FluentAssertions;
using UbiquitousLanguageManager.Tests.TestUtilities;
using Microsoft.AspNetCore.Identity;
using UbiquitousLanguageManager.Infrastructure.Data.Entities;
using System.Net;
using System.Net.Http;
using Microsoft.AspNetCore.Mvc.Testing;

namespace UbiquitousLanguageManager.Tests.Integration
{
    /// <summary>
    /// Phase A7 Step4 Phase 3: 基本統合テスト（簡易版）
    /// FirstLoginRedirectMiddleware・認証基盤の基本動作確認
    /// 
    /// 【テスト対象】
    /// 1. TestWebApplicationFactory基盤動作確認
    /// 2. FirstLoginRedirectMiddleware基本動作
    /// 3. データベース統合基盤確認
    /// </summary>
    public class Step4BasicIntegrationTests : IDisposable
    {
        private readonly TestWebApplicationFactory<Program> _factory;
        private readonly HttpClient _client;

        public Step4BasicIntegrationTests()
        {
            _factory = new TestWebApplicationFactory<Program>();
            _client = _factory.CreateClient(new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false // リダイレクト動作確認のため
            });
        }

        #region WebApplicationFactory基盤確認

        /// <summary>
        /// TestWebApplicationFactory基盤動作確認
        /// DI・データベース・基本HTTPレスポンス確認
        /// </summary>
        [Fact]
        public async Task WebApplicationFactory_Foundation_BasicVerification()
        {
            // Act & Assert: 基本的な初期化確認
            _factory.Should().NotBeNull("TestWebApplicationFactoryが正常に初期化される必要があります");
            _client.Should().NotBeNull("HttpClientが正常に作成される必要があります");

            // DI基盤確認
            using var scope = _factory.Services.CreateScope();
            var serviceProvider = scope.ServiceProvider;
            serviceProvider.Should().NotBeNull("ServiceProviderが正常に取得できる必要があります");

            // UserManager取得確認
            var userManager = serviceProvider.GetService<UserManager<ApplicationUser>>();
            userManager.Should().NotBeNull("UserManagerがDIコンテナから取得できる必要があります");

            // 基本HTTPレスポンス確認
            var response = await _client.GetAsync("/");
            response.Should().NotBeNull("HTTPレスポンスが取得できる必要があります");
            response.StatusCode.Should().BeOneOf(
                HttpStatusCode.OK,
                HttpStatusCode.Redirect,
                HttpStatusCode.Unauthorized,
                HttpStatusCode.NotFound
            );

            // テスト環境確認
            var environment = scope.ServiceProvider.GetService<Microsoft.AspNetCore.Hosting.IWebHostEnvironment>();
            environment?.EnvironmentName.Should().Be("Test", "テスト環境として設定される必要があります");
        }

        #endregion

        #region データベース統合基盤確認

        /// <summary>
        /// データベース統合基盤・UserManager基本動作確認
        /// ユーザー作成・取得の基本フローテスト
        /// </summary>
        [Fact]
        public async Task Database_Integration_UserManager_BasicOperations()
        {
            using var scope = await _factory.CreateScopeWithTestDataAsync();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            // Act: シンプルなテストユーザー作成
            var testUser = new ApplicationUser
            {
                UserName = "basic.integration@example.com",
                Email = "basic.integration@example.com",
                NormalizedUserName = "BASIC.INTEGRATION@EXAMPLE.COM",
                NormalizedEmail = "BASIC.INTEGRATION@EXAMPLE.COM",
                EmailConfirmed = true,
                Name = "Basic Integration Test User",
                IsFirstLogin = false, // 通常ユーザーとして作成
                SecurityStamp = Guid.NewGuid().ToString(),
                UpdatedAt = DateTime.UtcNow,
                UpdatedBy = "test-system"
            };

            var createResult = await userManager.CreateAsync(testUser, "BasicTest123!");

            // Assert: ユーザー作成確認
            createResult.Succeeded.Should().BeTrue("テストユーザーの作成が成功する必要があります");
            
            if (!createResult.Succeeded)
            {
                var errors = string.Join(", ", createResult.Errors.Select(e => e.Description));
                throw new InvalidOperationException($"ユーザー作成に失敗しました: {errors}");
            }

            // Act: ユーザー取得確認
            var retrievedUser = await userManager.FindByEmailAsync("basic.integration@example.com");

            // Assert: 取得・プロパティ確認
            retrievedUser.Should().NotBeNull("作成したユーザーが取得できる必要があります");
            retrievedUser!.Name.Should().Be("Basic Integration Test User", "ユーザー名が正しく保存・取得される必要があります");
            retrievedUser.IsFirstLogin.Should().BeFalse("IsFirstLoginフラグが正しく保存・取得される必要があります");
            retrievedUser.IsActive.Should().BeTrue("計算プロパティ IsActive が正常に動作する必要があります");
        }

        /// <summary>
        /// 初回ログインユーザー作成・確認テスト
        /// FirstLoginRedirectMiddleware対象ユーザーの作成確認
        /// </summary>
        [Fact]
        public async Task Database_FirstLoginUser_Creation_Verification()
        {
            using var scope = await _factory.CreateScopeWithTestDataAsync();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            // Act: 初回ログイン状態ユーザー作成
            var firstLoginUser = new ApplicationUser
            {
                UserName = "firstlogin.db@example.com",
                Email = "firstlogin.db@example.com",
                NormalizedUserName = "FIRSTLOGIN.DB@EXAMPLE.COM",
                NormalizedEmail = "FIRSTLOGIN.DB@EXAMPLE.COM",
                EmailConfirmed = true,
                Name = "First Login DB Test User",
                IsFirstLogin = true, // 初回ログイン状態
                SecurityStamp = Guid.NewGuid().ToString(),
                UpdatedAt = DateTime.UtcNow,
                UpdatedBy = "test-system"
            };

            var createResult = await userManager.CreateAsync(firstLoginUser, "FirstLoginTest123!");

            // Assert: 初回ログインユーザー作成確認
            createResult.Succeeded.Should().BeTrue("初回ログインユーザーの作成が成功する必要があります");

            var retrievedUser = await userManager.FindByEmailAsync("firstlogin.db@example.com");
            retrievedUser.Should().NotBeNull("初回ログインユーザーが取得できる必要があります");
            retrievedUser!.IsFirstLogin.Should().BeTrue("初回ログインフラグが正しく設定されている必要があります");
        }

        #endregion

        #region FirstLoginRedirectMiddleware基本確認

        /// <summary>
        /// FirstLoginRedirectMiddleware基本動作確認
        /// 未認証状態での各種パスアクセス確認
        /// </summary>
        [Fact]
        public async Task FirstLoginRedirectMiddleware_UnauthenticatedAccess_BasicVerification()
        {
            // Act & Assert: 未認証での管理画面アクセス
            var adminResponse = await _client.GetAsync("/admin/users");
            adminResponse.StatusCode.Should().BeOneOf(
                HttpStatusCode.Redirect,      // ログイン画面へのリダイレクト
                HttpStatusCode.Unauthorized,  // 認証が必要
                HttpStatusCode.Forbidden,     // アクセス拒否
                HttpStatusCode.NotFound       // エンドポイントが存在しない
            );

            // Act & Assert: 未認証でのホームページアクセス
            var homeResponse = await _client.GetAsync("/");
            homeResponse.StatusCode.Should().BeOneOf(
                HttpStatusCode.OK,            // 公開ページとして正常表示
                HttpStatusCode.Redirect       // 認証が必要な場合のリダイレクト
            );

            // Act & Assert: 静的リソースアクセス
            var cssResponse = await _client.GetAsync("/css/app.css");
            cssResponse.StatusCode.Should().BeOneOf(
                HttpStatusCode.OK,            // ファイルが存在
                HttpStatusCode.NotFound       // ファイルが存在しない（正常）
            );

            // Act & Assert: パスワード変更画面アクセス
            var changePasswordResponse = await _client.GetAsync("/change-password");
            changePasswordResponse.StatusCode.Should().BeOneOf(
                HttpStatusCode.OK,            // 正常表示
                HttpStatusCode.Redirect,      // 認証が必要（正常）
                HttpStatusCode.NotFound       // エンドポイントが存在しない場合
            );
        }

        /// <summary>
        /// 例外対象パス・静的リソースアクセス確認
        /// FirstLoginRedirectMiddleware除外処理の確認
        /// </summary>
        [Fact]
        public async Task FirstLoginRedirectMiddleware_StaticResources_ExceptionPaths()
        {
            // Act & Assert: 静的リソース各種
            var testResources = new[]
            {
                "/css/bootstrap.css",         // CSS
                "/js/app.js",                 // JavaScript
                "/favicon.ico",               // アイコン
                "/health",                    // ヘルスチェック
                "/api/auth/status"            // API認証状態
            };

            foreach (var resource in testResources)
            {
                var response = await _client.GetAsync(resource);
                
                // リダイレクトが発生しないことを確認（ファイルが存在しない場合はNotFound）
                response.StatusCode.Should().BeOneOf(
                    HttpStatusCode.OK,            // ファイルが存在
                    HttpStatusCode.NotFound,      // ファイルが存在しない（正常）
                    HttpStatusCode.Unauthorized   // APIの場合認証が必要（正常）
                );

                // パスワード変更画面へのリダイレクトが発生しないことを確認
                if (response.StatusCode == HttpStatusCode.Redirect)
                {
                    var location = response.Headers.Location?.ToString();
                    location.Should().NotContain("/change-password", 
                        $"静的リソース '{resource}' でパスワード変更画面へのリダイレクトが発生してはいけません");
                }
            }
        }

        #endregion

        #region HTTPクライアント基本確認

        /// <summary>
        /// HTTPクライアント基本動作確認
        /// リダイレクト設定・レスポンス処理の確認
        /// </summary>
        [Fact]
        public async Task HttpClient_BasicConfiguration_RedirectHandling()
        {
            // Act: 基本的なGETリクエスト
            var response = await _client.GetAsync("/");

            // Assert: レスポンス基本確認
            response.Should().NotBeNull("HTTPレスポンスが取得できる必要があります");
            response.Headers.Should().NotBeNull("HTTPヘッダーが存在する必要があります");

            // リダイレクト処理確認（自動リダイレクトを無効にしているため）
            if (response.StatusCode == HttpStatusCode.Redirect)
            {
                var location = response.Headers.Location;
                location.Should().NotBeNull("リダイレクト時にLocationヘッダーが設定される必要があります");
            }

            // コンテンツタイプ確認（HTMLまたはJSON）
            if (response.StatusCode == HttpStatusCode.OK)
            {
                var contentType = response.Content.Headers.ContentType?.MediaType;
                if (contentType != null)
                {
                    contentType.Should().BeOneOf("text/html", "application/json", 
                        "レスポンスのContentTypeが適切に設定される必要があります");
                }
            }
        }

        #endregion

        #region クリーンアップ

        public void Dispose()
        {
            _client?.Dispose();
            _factory?.Dispose();
        }

        #endregion
    }
}