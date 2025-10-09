using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;
using UbiquitousLanguageManager.Application;
using UbiquitousLanguageManager.Domain.Common;
using UbiquitousLanguageManager.Domain.Authentication;
using UbiquitousLanguageManager.Infrastructure.Repositories;

namespace UbiquitousLanguageManager.Tests.Integration;

/// <summary>
/// Phase A9 Step 1-2: F# Authentication Application層統合テスト
/// Infrastructure層 ↔ F# Application層 完全統合動作確認
/// Clean Architecture依存関係・Railway-oriented Programming検証
/// 【統合テスト初学者向け解説】
/// このテストクラスでは、F#で実装された認証サービスが、
/// C# Infrastructure層と完全に統合されて動作することを検証します。
/// WebApplicationFactoryを使用して、本物のDIコンテナ環境でテストを実行します。
/// </summary>
public class FSharpAuthenticationIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly IServiceProvider _serviceProvider;

    public FSharpAuthenticationIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory ?? throw new ArgumentNullException(nameof(factory));
        _serviceProvider = _factory.Services;
    }

    // =================================================================
    // 🧪 E2Eテストシナリオ1: 初回ログイン→パスワード変更完了フロー
    // =================================================================

    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Phase", "A9")]
    public async Task Scenario1_FirstLogin_PasswordChange_Flow_ShouldWork()
    {
        // Arrange: テスト環境セットアップ
        using var scope = _serviceProvider.CreateScope();
        var authService = scope.ServiceProvider.GetRequiredService<AuthenticationApplicationService>();
        var userRepository = scope.ServiceProvider.GetRequiredService<IUserRepository>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<FSharpAuthenticationIntegrationTests>>();

        logger.LogInformation("=== E2Eテストシナリオ1: 初回ログイン→パスワード変更フロー開始 ===");

        // Step 1: 初回ログイン実行（admin@ubiquitous-lang.com / su）
        var loginRequest = new AuthenticationRequest
        {
            Email = "admin@ubiquitous-lang.com",
            Password = "su",
            RememberMe = false
        };

        logger.LogInformation("Step 1: 初回ログイン実行 - {Email}", loginRequest.Email);
        var loginResult = await authService.AuthenticateUserAsync(loginRequest);

        // Assert: 認証成功・初回ログイン検知
        Assert.True(loginResult.IsOk, $"初回ログイン失敗: {(loginResult.IsError ? loginResult.ErrorValue.ToString() : "不明")}");
        
        var authResult = loginResult.ResultValue;
        logger.LogInformation("Step 1結果: 認証成功 - タイプ: {ResultType}", authResult.GetType().Name);

        // F#判別共用体のパターンマッチング（C#版）
        if (authResult is FirstLoginRequired firstLoginUser)
        {
            logger.LogInformation("Step 1検証: 初回ログイン検知成功 - {Email}", firstLoginUser.Item.Email.Value);
            Assert.True(firstLoginUser.Item.IsFirstLogin, "IsFirstLoginフラグが正しく設定されていません");
        }
        else
        {
            Assert.True(false, $"期待される結果: FirstLoginRequired, 実際の結果: {authResult.GetType().Name}");
        }

        // Step 2: パスワード変更実行（su → NewSecurePassword123!）
        logger.LogInformation("Step 2: パスワード変更実行開始");
        var userId = ((FirstLoginRequired)authResult).Item.Id;
        var changeResult = await authService.ChangeUserPasswordAsync(
            userId, 
            "su", 
            Password.create("NewSecurePassword123!").ResultValue);

        // Assert: パスワード変更成功
        Assert.True(changeResult.IsOk, $"パスワード変更失敗: {(changeResult.IsError ? changeResult.ErrorValue.ToString() : "不明")}");
        
        var updatedUser = changeResult.ResultValue;
        logger.LogInformation("Step 2検証: パスワード変更成功 - IsFirstLogin: {IsFirstLogin}", updatedUser.IsFirstLogin);
        
        // 重要: パスワード変更後はIsFirstLogin = false になること
        Assert.False(updatedUser.IsFirstLogin, "パスワード変更後にIsFirstLoginがfalseになっていません");

        logger.LogInformation("=== シナリオ1完了: 初回ログイン→パスワード変更フロー成功 ===");
    }

    // =================================================================
    // 🧪 E2Eテストシナリオ2: パスワード変更完了後の通常ログイン
    // =================================================================

    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Phase", "A9")]
    public async Task Scenario2_NormalLogin_AfterPasswordChange_ShouldWork()
    {
        // Arrange: テスト環境セットアップ
        using var scope = _serviceProvider.CreateScope();
        var authService = scope.ServiceProvider.GetRequiredService<AuthenticationApplicationService>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<FSharpAuthenticationIntegrationTests>>();

        logger.LogInformation("=== E2Eテストシナリオ2: パスワード変更完了後の通常ログイン開始 ===");

        // 前提条件: シナリオ1でパスワード変更済みと仮定
        // 実際の統合テストでは、テストデータベースの状態管理が重要

        // Step 1: 新パスワードでのログイン実行
        var loginRequest = new AuthenticationRequest
        {
            Email = "admin@ubiquitous-lang.com",
            Password = "NewSecurePassword123!",  // 変更後パスワード
            RememberMe = false
        };

        logger.LogInformation("Step 1: 新パスワードでログイン実行 - {Email}", loginRequest.Email);
        var loginResult = await authService.AuthenticateUserAsync(loginRequest);

        // Assert: 認証成功・通常ログイン
        Assert.True(loginResult.IsOk, $"通常ログイン失敗: {(loginResult.IsError ? loginResult.ErrorValue.ToString() : "不明")}");
        
        var authResult = loginResult.ResultValue;
        logger.LogInformation("Step 1結果: 認証成功 - タイプ: {ResultType}", authResult.GetType().Name);

        // パスワード変更完了後は、AuthenticationSuccessまたはEmailConfirmationRequiredになる
        if (authResult is AuthenticationSuccess successResult)
        {
            logger.LogInformation("Step 1検証: 通常ログイン成功 - Token: {HasToken}", !string.IsNullOrEmpty(successResult.Item2));
            Assert.False(successResult.Item1.IsFirstLogin, "通常ログイン時にIsFirstLoginがtrueです");
        }
        else if (authResult is EmailConfirmationRequired emailConfirmUser)
        {
            logger.LogInformation("Step 1検証: メール確認要求 - {Email}", emailConfirmUser.Item.Email.Value);
            Assert.False(emailConfirmUser.Item.IsFirstLogin, "メール確認要求時にIsFirstLoginがtrueです");
        }
        else
        {
            Assert.True(false, $"期待される結果: AuthenticationSuccess または EmailConfirmationRequired, 実際の結果: {authResult.GetType().Name}");
        }

        logger.LogInformation("=== シナリオ2完了: パスワード変更完了後の通常ログイン成功 ===");
    }

    // =================================================================
    // 🧪 E2Eテストシナリオ3: F# Authentication Service統合確認
    // =================================================================

    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Phase", "A9")]
    public async Task Scenario3_FSharp_Authentication_Service_Integration_ShouldWork()
    {
        // Arrange: F#サービス依存関係解決確認
        using var scope = _serviceProvider.CreateScope();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<FSharpAuthenticationIntegrationTests>>();

        logger.LogInformation("=== E2Eテストシナリオ3: F#認証サービス統合確認開始 ===");

        // Step 1: F# AuthenticationApplicationService解決確認
        var authService = scope.ServiceProvider.GetService<AuthenticationApplicationService>();
        Assert.NotNull(authService);
        logger.LogInformation("Step 1検証: F# AuthenticationApplicationService解決成功");

        // Step 2: F# IUserRepository（UserRepositoryAdapter）解決確認
        var userRepository = scope.ServiceProvider.GetService<IUserRepository>();
        Assert.NotNull(userRepository);
        Assert.IsType<UserRepositoryAdapter>(userRepository);
        logger.LogInformation("Step 2検証: UserRepositoryAdapter解決成功");

        // Step 3: F# IAuthenticationService（Infrastructure実装）解決確認
        var infraAuthService = scope.ServiceProvider.GetService<IAuthenticationService>();
        Assert.NotNull(infraAuthService);
        logger.LogInformation("Step 3検証: Infrastructure AuthenticationService解決成功");

        // Step 4: Railway-oriented Programming動作確認
        // 不正なメールアドレスでのバリデーションエラー確認
        var invalidLoginRequest = new AuthenticationRequest
        {
            Email = "invalid-email-format",  // 不正なメールアドレス
            Password = "anypassword",
            RememberMe = false
        };

        logger.LogInformation("Step 4: Railway-oriented Programming検証 - 不正メールアドレス");
        var invalidResult = await authService.AuthenticateUserAsync(invalidLoginRequest);

        // Assert: バリデーションエラーが適切に返される
        Assert.True(invalidResult.IsError, "不正なメールアドレスでエラーが返されませんでした");
        
        var error = invalidResult.ErrorValue;
        Assert.True(error.IsValidationError, "ValidationErrorタイプが返されませんでした");
        logger.LogInformation("Step 4検証: バリデーションエラー正常動作 - {ErrorType}", error.GetType().Name);

        // Step 5: 存在しないユーザーでのUserNotFoundエラー確認
        var nonExistentUserRequest = new AuthenticationRequest
        {
            Email = "nonexistent@example.com",
            Password = "anypassword",
            RememberMe = false
        };

        logger.LogInformation("Step 5: UserNotFoundエラー検証");
        var notFoundResult = await authService.AuthenticateUserAsync(nonExistentUserRequest);

        // Assert: UserNotFoundエラーが適切に返される
        Assert.True(notFoundResult.IsError, "存在しないユーザーでエラーが返されませんでした");
        
        var notFoundError = notFoundResult.ErrorValue;
        Assert.True(notFoundError.IsUserNotFound, "UserNotFoundタイプが返されませんでした");
        logger.LogInformation("Step 5検証: UserNotFoundエラー正常動作");

        logger.LogInformation("=== シナリオ3完了: F#認証サービス統合確認成功 ===");
    }

    // =================================================================
    // 🧪 追加テスト: Clean Architecture依存方向確認
    // =================================================================

    [Fact]
    [Trait("Category", "Architecture")]
    [Trait("Phase", "A9")]
    public void Architecture_Dependency_Direction_ShouldBeCorrect()
    {
        // Arrange & Act: 依存方向確認
        using var scope = _serviceProvider.CreateScope();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<FSharpAuthenticationIntegrationTests>>();

        logger.LogInformation("=== Clean Architecture依存方向確認開始 ===");

        // Infrastructure → Application の依存確認
        var userRepositoryType = typeof(UserRepositoryAdapter);
        var applicationInterface = typeof(IUserRepository);

        // Assert: UserRepositoryAdapterがF# IUserRepositoryを実装
        Assert.True(applicationInterface.IsAssignableFrom(userRepositoryType), 
            "UserRepositoryAdapterがIUserRepositoryを実装していません");

        // F# Application層インターフェースの名前空間確認
        Assert.Equal("UbiquitousLanguageManager.Application", applicationInterface.Namespace);
        logger.LogInformation("依存方向確認: Infrastructure → Application ✓");

        // F# AuthenticationApplicationServiceがApplication層に存在確認
        var authServiceType = typeof(AuthenticationApplicationService);
        Assert.Equal("UbiquitousLanguageManager.Application", authServiceType.Namespace);
        logger.LogInformation("依存方向確認: F# AuthenticationApplicationService配置 ✓");

        logger.LogInformation("=== Clean Architecture依存方向確認完了 ===");
    }

    // =================================================================
    // 🧪 性能テスト: 認証処理パフォーマンス確認
    // =================================================================

    [Fact]
    [Trait("Category", "Performance")]
    [Trait("Phase", "A9")]
    public async Task Performance_Authentication_ShouldBeUnder1Second()
    {
        // Arrange
        using var scope = _serviceProvider.CreateScope();
        var authService = scope.ServiceProvider.GetRequiredService<AuthenticationApplicationService>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<FSharpAuthenticationIntegrationTests>>();

        var loginRequest = new AuthenticationRequest
        {
            Email = "admin@ubiquitous-lang.com",
            Password = "su",
            RememberMe = false
        };

        logger.LogInformation("=== 認証パフォーマンステスト開始 ===");

        // Act: 認証処理時間測定
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        var result = await authService.AuthenticateUserAsync(loginRequest);
        stopwatch.Stop();

        // Assert: 1秒以内での処理完了
        Assert.True(stopwatch.ElapsedMilliseconds < 1000, 
            $"認証処理が1秒を超えました: {stopwatch.ElapsedMilliseconds}ms");

        logger.LogInformation("認証処理時間: {ElapsedMs}ms", stopwatch.ElapsedMilliseconds);
        logger.LogInformation("=== 認証パフォーマンステスト完了 ===");
    }
}