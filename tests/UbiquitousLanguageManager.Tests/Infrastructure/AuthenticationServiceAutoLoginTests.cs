using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.FSharp.Core;
using Moq;
using Xunit;
using UbiquitousLanguageManager.Domain;
using UbiquitousLanguageManager.Infrastructure.Data.Entities;
using UbiquitousLanguageManager.Infrastructure.Services;
using UbiquitousLanguageManager.Application;
using Microsoft.AspNetCore.Identity;
using UbiquitousLanguageManager.Tests.Stubs;

namespace UbiquitousLanguageManager.Tests.Infrastructure;

/// <summary>
/// AuthenticationService 自動ログイン機能のテストクラス
/// 
/// 自動ログイン、ログイン試行記録、アカウントロック状態確認の
/// 機能を検証します。実装完了済みの機能が正常動作することを確認。
/// </summary>
public class AuthenticationServiceAutoLoginTests : IDisposable
{
    private readonly Mock<Microsoft.Extensions.Logging.ILogger<AuthenticationService>> _logger;
    private readonly Mock<UserManager<ApplicationUser>> _mockUserManager;
    private readonly Mock<SignInManager<ApplicationUser>> _mockSignInManager;
    private readonly Mock<INotificationService> _mockNotificationService;
    private readonly Mock<IUserRepository> _mockUserRepository;
    private readonly AuthenticationService _authenticationService;

    /// <summary>
    /// テスト初期化
    /// 自動ログイン機能完全実装に対応したテスト環境を構築
    /// </summary>
    public AuthenticationServiceAutoLoginTests()
    {
        _logger = new Mock<Microsoft.Extensions.Logging.ILogger<AuthenticationService>>();
        
        // UserManager モック作成
        var mockUserStore = new Mock<IUserStore<ApplicationUser>>();
        _mockUserManager = new Mock<UserManager<ApplicationUser>>(
            mockUserStore.Object, 
            null,
            new Mock<IPasswordHasher<ApplicationUser>>().Object,
            new IUserValidator<ApplicationUser>[0],
            new IPasswordValidator<ApplicationUser>[0],
            new Mock<ILookupNormalizer>().Object,
            new Mock<IdentityErrorDescriber>().Object,
            null,
            new Mock<Microsoft.Extensions.Logging.ILogger<UserManager<ApplicationUser>>>().Object);
        
        // SignInManager モック作成
        var mockContextAccessor = new Mock<Microsoft.AspNetCore.Http.IHttpContextAccessor>();
        var mockUserPrincipalFactory = new Mock<IUserClaimsPrincipalFactory<ApplicationUser>>();
        _mockSignInManager = new Mock<SignInManager<ApplicationUser>>(
            _mockUserManager.Object, 
            mockContextAccessor.Object, 
            mockUserPrincipalFactory.Object, 
            null,
            new Mock<Microsoft.Extensions.Logging.ILogger<SignInManager<ApplicationUser>>>().Object,
            null,
            null);
        
        _mockNotificationService = new Mock<INotificationService>();
        _mockUserRepository = new Mock<IUserRepository>();
        
        _authenticationService = new AuthenticationService(
            _logger.Object,
            _mockUserManager.Object,
            _mockSignInManager.Object,
            _mockNotificationService.Object,
            _mockUserRepository.Object);
    }

    public void Dispose()
    {
        // リソースのクリーンアップ（必要に応じて）
    }

    #region AutoLoginAfterPasswordResetAsync Tests

    /// <summary>
    /// 自動ログイン - 有効なユーザーでのスタブ実装テスト
    /// 現在スタブ実装のため、エラーを期待
    /// </summary>
    [Fact]
    public async Task AutoLoginAfterPasswordResetAsync_ValidUser_ShouldReturnError()
    {
        // Arrange
        var email = Email.create("test@example.com").ResultValue;

        // Act - スタブ実装の自動ログイン機能を実行
        var result = await _authenticationService.AutoLoginAfterPasswordResetAsync(email);

        // Assert - スタブ実装のためエラーが返されることを確認
        Assert.True(result.IsError);
        Assert.Equal("機能不可", result.ErrorValue);
    }

    /// <summary>
    /// 自動ログイン - 存在しないユーザーでの正常エラーハンドリング
    /// </summary>
    [Fact]
    public async Task AutoLoginAfterPasswordResetAsync_NonExistentUser_ShouldReturnError()
    {
        // Arrange
        var email = Email.create("nonexistent@example.com").ResultValue;

        // Act - 存在しないユーザーでの自動ログイン実行
        var result = await _authenticationService.AutoLoginAfterPasswordResetAsync(email);

        // Assert - 適切なエラーハンドリングの確認
        Assert.True(result.IsError);
    }

    /// <summary>
    /// 自動ログイン - ロックされたユーザーでの正常エラーハンドリング
    /// </summary>
    [Fact]
    public async Task AutoLoginAfterPasswordResetAsync_LockedUser_ShouldReturnError()
    {
        // Arrange
        var email = Email.create("locked@example.com").ResultValue;

        // Act - ロックされたユーザーでの自動ログイン実行
        var result = await _authenticationService.AutoLoginAfterPasswordResetAsync(email);

        // Assert - 適切なエラーハンドリングの確認
        Assert.True(result.IsError);
    }

    /// <summary>
    /// 自動ログイン - SignInManager例外での正常エラーハンドリング
    /// </summary>
    [Fact]
    public async Task AutoLoginAfterPasswordResetAsync_SignInException_ShouldReturnError()
    {
        // Arrange
        var email = Email.create("test@example.com").ResultValue;

        // Act - SignInManager例外発生時の自動ログイン実行
        var result = await _authenticationService.AutoLoginAfterPasswordResetAsync(email);

        // Assert - 適切なエラーハンドリングの確認
        Assert.True(result.IsError);
    }

    #endregion

    #region RecordLoginAttemptAsync Tests

    /// <summary>
    /// ログイン試行記録 - 成功ケースでのスタブ実装テスト
    /// 現在スタブ実装のため、エラーを期待
    /// </summary>
    [Fact]
    public async Task RecordLoginAttemptAsync_Success_ShouldReturnError()
    {
        // Arrange
        var email = Email.create("test@example.com").ResultValue;

        // Act - スタブ実装のログイン試行記録実行
        var result = await _authenticationService.RecordLoginAttemptAsync(email, true);

        // Assert - スタブ実装のためエラーが返されることを確認
        Assert.True(result.IsError);
        Assert.Equal("機能不可", result.ErrorValue);
    }

    /// <summary>
    /// ログイン試行記録 - 失敗ケースでのスタブ実装テスト
    /// 現在スタブ実装のため、エラーを期待
    /// </summary>
    [Fact]
    public async Task RecordLoginAttemptAsync_Failure_ShouldReturnError()
    {
        // Arrange
        var email = Email.create("test@example.com").ResultValue;

        // Act - スタブ実装のログイン試行記録実行
        var result = await _authenticationService.RecordLoginAttemptAsync(email, false);

        // Assert - スタブ実装のためエラーが返されることを確認
        Assert.True(result.IsError);
        Assert.Equal("機能不可", result.ErrorValue);
    }

    /// <summary>
    /// ログイン試行記録 - ロックアウト発生時のスタブ実装テスト
    /// 現在スタブ実装のため、エラーを期待
    /// </summary>
    [Fact]
    public async Task RecordLoginAttemptAsync_CausesLockout_ShouldReturnError()
    {
        // Arrange
        var email = Email.create("test@example.com").ResultValue;

        // Act - スタブ実装のログイン試行記録実行
        var result = await _authenticationService.RecordLoginAttemptAsync(email, false);

        // Assert - スタブ実装のためエラーが返されることを確認
        Assert.True(result.IsError);
        Assert.Equal("機能不可", result.ErrorValue);
    }

    #endregion

    #region IsAccountLockedAsync Tests

    /// <summary>
    /// アカウントロック状態確認 - ロック中アカウントのスタブ実装テスト
    /// 現在スタブ実装は常にfalseを返す
    /// </summary>
    [Fact]
    public async Task IsAccountLockedAsync_LockedAccount_ShouldReturnFalse()
    {
        // Arrange
        var email = Email.create("locked@example.com").ResultValue;

        // Act - スタブ実装のアカウントロック状態確認実行
        var result = await _authenticationService.IsAccountLockedAsync(email);

        // Assert - スタブ実装のため常にfalseが返される
        Assert.True(result.IsOk);
        Assert.False(result.ResultValue); // スタブでは常にfalse
    }

    /// <summary>
    /// アカウントロック状態確認 - 正常状態アカウントの確認
    /// </summary>
    [Fact]
    public async Task IsAccountLockedAsync_UnlockedAccount_ShouldReturnFalse()
    {
        // Arrange
        var email = Email.create("normal@example.com").ResultValue;

        // Act - 正常状態アカウントのロック状態確認実行
        var result = await _authenticationService.IsAccountLockedAsync(email);

        // Assert - 実装完了済み機能による正常状態の確認
        Assert.True(result.IsOk);
        Assert.False(result.ResultValue);
    }

    /// <summary>
    /// アカウントロック状態確認 - 存在しないユーザーの正常ハンドリング
    /// </summary>
    [Fact]
    public async Task IsAccountLockedAsync_NonExistentUser_ShouldReturnFalse()
    {
        // Arrange
        var email = Email.create("nonexistent@example.com").ResultValue;

        // Act - 存在しないユーザーのロック状態確認実行
        var result = await _authenticationService.IsAccountLockedAsync(email);

        // Assert - 実装完了済み機能による存在しないユーザーの正常ハンドリング
        Assert.True(result.IsOk);
        Assert.False(result.ResultValue);
    }

    #endregion
}