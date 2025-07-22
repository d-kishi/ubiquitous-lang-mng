using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.FSharp.Core;
using Moq;
using Xunit;
using UbiquitousLanguageManager.Application;
using UbiquitousLanguageManager.Domain;
using UbiquitousLanguageManager.Infrastructure.Services;

namespace UbiquitousLanguageManager.Tests.Infrastructure;

/// <summary>
/// AuthenticationService 自動ログイン機能のテストクラス
/// 
/// 【F#初学者向け解説】
/// Phase A3 Step4で実装した自動ログイン機能の単体テスト。
/// SignInManager統合、Identity Lockout連携、基本監査ログ機能を検証。
/// 
/// 🔐 自動ログイン・セキュリティ機能の包括的テスト
/// 📊 ADR_008準拠: ログ出力の検証も含む
/// </summary>
public class AuthenticationServiceAutoLoginTests
{
    private readonly Mock<Microsoft.Extensions.Logging.ILogger<AuthenticationService>> _loggerMock;
    private readonly Mock<UserManager<IdentityUser>> _userManagerMock;
    private readonly Mock<SignInManager<IdentityUser>> _signInManagerMock;
    private readonly Mock<INotificationService> _notificationServiceMock;
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly AuthenticationService _authenticationService;

    /// <summary>
    /// テスト初期化
    /// </summary>
    public AuthenticationServiceAutoLoginTests()
    {
        _loggerMock = new Mock<Microsoft.Extensions.Logging.ILogger<AuthenticationService>>();
        _userManagerMock = CreateUserManagerMock();
        _signInManagerMock = CreateSignInManagerMock();
        _notificationServiceMock = new Mock<INotificationService>();
        _userRepositoryMock = new Mock<IUserRepository>();

        _authenticationService = new AuthenticationService(
            _loggerMock.Object,
            _userManagerMock.Object,
            _signInManagerMock.Object,
            _notificationServiceMock.Object,
            _userRepositoryMock.Object);
    }

    #region AutoLoginAfterPasswordResetAsync Tests

    /// <summary>
    /// 自動ログイン - 正常ケース
    /// </summary>
    [Fact]
    public async Task AutoLoginAfterPasswordResetAsync_ValidUser_ShouldSucceed()
    {
        // Arrange
        var email = Email.create("test@example.com").ResultValue;
        var identityUser = new IdentityUser { Email = email.Value, EmailConfirmed = true };

        _userManagerMock.Setup(x => x.FindByEmailAsync(email.Value))
            .ReturnsAsync(identityUser);
        _userManagerMock.Setup(x => x.IsLockedOutAsync(identityUser))
            .ReturnsAsync(false);
        _signInManagerMock.Setup(x => x.SignInAsync(identityUser, false, null))
            .Returns(Task.CompletedTask);

        // RecordLoginAttemptAsyncのモック（内部で呼ばれる）
        _userManagerMock.Setup(x => x.ResetAccessFailedCountAsync(identityUser))
            .ReturnsAsync(IdentityResult.Success);

        // Act
        var result = await _authenticationService.AutoLoginAfterPasswordResetAsync(email);

        // Assert
        Assert.True(result.IsOk);

        // 🔐 SignInManager.SignInAsyncが適切に呼ばれることを検証
        _signInManagerMock.Verify(x => x.SignInAsync(identityUser, false, null), Times.Once);

        // 📊 ADR_008準拠: 適切なログ出力を検証
        VerifyLogCalled(LogLevel.Information, "Auto login after password reset initiated");
        VerifyLogCalled(LogLevel.Information, "Auto login completed successfully");
    }

    /// <summary>
    /// 自動ログイン - 存在しないユーザー
    /// </summary>
    [Fact]
    public async Task AutoLoginAfterPasswordResetAsync_NonExistentUser_ShouldFail()
    {
        // Arrange
        var email = Email.create("nonexistent@example.com").ResultValue;

        _userManagerMock.Setup(x => x.FindByEmailAsync(email.Value))
            .ReturnsAsync((IdentityUser)null);

        // Act
        var result = await _authenticationService.AutoLoginAfterPasswordResetAsync(email);

        // Assert
        Assert.True(result.IsError);
        Assert.Contains("ユーザーが見つかりません", result.ErrorValue);

        // 🔐 SignInAsyncが呼ばれないことを検証
        _signInManagerMock.Verify(x => x.SignInAsync(It.IsAny<IdentityUser>(), It.IsAny<bool>(), It.IsAny<string>()), Times.Never);

        // 📊 警告ログが出力されることを検証
        VerifyLogCalled(LogLevel.Warning, "Auto login attempted for non-existent user");
    }

    /// <summary>
    /// 自動ログイン - ロックされたユーザー
    /// </summary>
    [Fact]
    public async Task AutoLoginAfterPasswordResetAsync_LockedUser_ShouldFail()
    {
        // Arrange
        var email = Email.create("locked@example.com").ResultValue;
        var identityUser = new IdentityUser { Email = email.Value, EmailConfirmed = true };

        _userManagerMock.Setup(x => x.FindByEmailAsync(email.Value))
            .ReturnsAsync(identityUser);
        _userManagerMock.Setup(x => x.IsLockedOutAsync(identityUser))
            .ReturnsAsync(true);

        // Act
        var result = await _authenticationService.AutoLoginAfterPasswordResetAsync(email);

        // Assert
        Assert.True(result.IsError);
        Assert.Contains("アカウントがロックされています", result.ErrorValue);

        // 🔐 SignInAsyncが呼ばれないことを検証
        _signInManagerMock.Verify(x => x.SignInAsync(It.IsAny<IdentityUser>(), It.IsAny<bool>(), It.IsAny<string>()), Times.Never);

        // 📊 警告ログが出力されることを検証
        VerifyLogCalled(LogLevel.Warning, "Auto login attempted for locked user");
    }

    /// <summary>
    /// 自動ログイン - SignInManager例外
    /// </summary>
    [Fact]
    public async Task AutoLoginAfterPasswordResetAsync_SignInException_ShouldReturnError()
    {
        // Arrange
        var email = Email.create("test@example.com").ResultValue;
        var identityUser = new IdentityUser { Email = email.Value, EmailConfirmed = true };

        _userManagerMock.Setup(x => x.FindByEmailAsync(email.Value))
            .ReturnsAsync(identityUser);
        _userManagerMock.Setup(x => x.IsLockedOutAsync(identityUser))
            .ReturnsAsync(false);
        _signInManagerMock.Setup(x => x.SignInAsync(identityUser, false, null))
            .ThrowsAsync(new InvalidOperationException("SignIn failed"));

        // Act
        var result = await _authenticationService.AutoLoginAfterPasswordResetAsync(email);

        // Assert
        Assert.True(result.IsError);
        Assert.Contains("自動ログインでシステムエラー", result.ErrorValue);

        // 📊 エラーログが出力されることを検証
        VerifyLogCalled(LogLevel.Error, "Unexpected error during auto login");
    }

    #endregion

    #region RecordLoginAttemptAsync Tests

    /// <summary>
    /// ログイン試行記録 - 成功ケース
    /// </summary>
    [Fact]
    public async Task RecordLoginAttemptAsync_Success_ShouldResetFailedCount()
    {
        // Arrange
        var email = Email.create("test@example.com").ResultValue;
        var identityUser = new IdentityUser { Email = email.Value };

        _userManagerMock.Setup(x => x.FindByEmailAsync(email.Value))
            .ReturnsAsync(identityUser);
        _userManagerMock.Setup(x => x.ResetAccessFailedCountAsync(identityUser))
            .ReturnsAsync(IdentityResult.Success);

        // Act
        var result = await _authenticationService.RecordLoginAttemptAsync(email, true);

        // Assert
        Assert.True(result.IsOk);

        // 🔄 成功時の失敗カウントリセットが呼ばれることを検証
        _userManagerMock.Verify(x => x.ResetAccessFailedCountAsync(identityUser), Times.Once);

        // 📊 成功ログが出力されることを検証
        VerifyLogCalled(LogLevel.Information, "Login attempt recorded");
    }

    /// <summary>
    /// ログイン試行記録 - 失敗ケース
    /// </summary>
    [Fact]
    public async Task RecordLoginAttemptAsync_Failure_ShouldIncrementFailedCount()
    {
        // Arrange
        var email = Email.create("test@example.com").ResultValue;
        var identityUser = new IdentityUser { Email = email.Value };

        _userManagerMock.Setup(x => x.FindByEmailAsync(email.Value))
            .ReturnsAsync(identityUser);
        _userManagerMock.Setup(x => x.AccessFailedAsync(identityUser))
            .ReturnsAsync(IdentityResult.Success);
        _userManagerMock.Setup(x => x.IsLockedOutAsync(identityUser))
            .ReturnsAsync(false);

        // Act
        var result = await _authenticationService.RecordLoginAttemptAsync(email, false);

        // Assert
        Assert.True(result.IsOk);

        // 🔒 失敗時のAccessFailedAsyncが呼ばれることを検証
        _userManagerMock.Verify(x => x.AccessFailedAsync(identityUser), Times.Once);

        // 📊 警告ログが出力されることを検証
        VerifyLogCalled(LogLevel.Warning, "Login attempt recorded");
    }

    /// <summary>
    /// ログイン試行記録 - ロックアウト発生
    /// </summary>
    [Fact]
    public async Task RecordLoginAttemptAsync_CausesLockout_ShouldLogLockout()
    {
        // Arrange
        var email = Email.create("test@example.com").ResultValue;
        var identityUser = new IdentityUser { Email = email.Value };
        var lockoutEnd = DateTimeOffset.UtcNow.AddMinutes(15);

        _userManagerMock.Setup(x => x.FindByEmailAsync(email.Value))
            .ReturnsAsync(identityUser);
        _userManagerMock.Setup(x => x.AccessFailedAsync(identityUser))
            .ReturnsAsync(IdentityResult.Success);
        _userManagerMock.Setup(x => x.IsLockedOutAsync(identityUser))
            .ReturnsAsync(true);
        _userManagerMock.Setup(x => x.GetLockoutEndDateAsync(identityUser))
            .ReturnsAsync(lockoutEnd);

        // Act
        var result = await _authenticationService.RecordLoginAttemptAsync(email, false);

        // Assert
        Assert.True(result.IsOk);

        // 📊 ロックアウトログが出力されることを検証
        VerifyLogCalled(LogLevel.Warning, "User account locked due to failed login attempts");
    }

    #endregion

    #region IsAccountLockedAsync Tests

    /// <summary>
    /// アカウントロック状態確認 - ロック中
    /// </summary>
    [Fact]
    public async Task IsAccountLockedAsync_LockedAccount_ShouldReturnTrue()
    {
        // Arrange
        var email = Email.create("locked@example.com").ResultValue;
        var identityUser = new IdentityUser { Email = email.Value };

        _userManagerMock.Setup(x => x.FindByEmailAsync(email.Value))
            .ReturnsAsync(identityUser);
        _userManagerMock.Setup(x => x.IsLockedOutAsync(identityUser))
            .ReturnsAsync(true);

        // Act
        var result = await _authenticationService.IsAccountLockedAsync(email);

        // Assert
        Assert.True(result.IsOk);
        Assert.True(result.ResultValue);
    }

    /// <summary>
    /// アカウントロック状態確認 - 正常状態
    /// </summary>
    [Fact]
    public async Task IsAccountLockedAsync_UnlockedAccount_ShouldReturnFalse()
    {
        // Arrange
        var email = Email.create("normal@example.com").ResultValue;
        var identityUser = new IdentityUser { Email = email.Value };

        _userManagerMock.Setup(x => x.FindByEmailAsync(email.Value))
            .ReturnsAsync(identityUser);
        _userManagerMock.Setup(x => x.IsLockedOutAsync(identityUser))
            .ReturnsAsync(false);

        // Act
        var result = await _authenticationService.IsAccountLockedAsync(email);

        // Assert
        Assert.True(result.IsOk);
        Assert.False(result.ResultValue);
    }

    /// <summary>
    /// アカウントロック状態確認 - 存在しないユーザー
    /// </summary>
    [Fact]
    public async Task IsAccountLockedAsync_NonExistentUser_ShouldReturnFalse()
    {
        // Arrange
        var email = Email.create("nonexistent@example.com").ResultValue;

        _userManagerMock.Setup(x => x.FindByEmailAsync(email.Value))
            .ReturnsAsync((IdentityUser)null);

        // Act
        var result = await _authenticationService.IsAccountLockedAsync(email);

        // Assert
        Assert.True(result.IsOk);
        Assert.False(result.ResultValue);
    }

    #endregion

    #region Helper Methods

    /// <summary>
    /// UserManager<IdentityUser> のモックを作成
    /// </summary>
    private static Mock<UserManager<IdentityUser>> CreateUserManagerMock()
    {
        var store = new Mock<IUserStore<IdentityUser>>();
        var mgr = new Mock<UserManager<IdentityUser>>(
            store.Object, null, null, null, null, null, null, null, null);
        mgr.Object.UserValidators.Add(new UserValidator<IdentityUser>());
        mgr.Object.PasswordValidators.Add(new PasswordValidator<IdentityUser>());
        return mgr;
    }

    /// <summary>
    /// SignInManager<IdentityUser> のモックを作成
    /// </summary>
    private static Mock<SignInManager<IdentityUser>> CreateSignInManagerMock()
    {
        var userManager = CreateUserManagerMock();
        var contextAccessor = new Mock<Microsoft.AspNetCore.Http.IHttpContextAccessor>();
        var claimsFactory = new Mock<Microsoft.AspNetCore.Identity.IUserClaimsPrincipalFactory<IdentityUser>>();

        return new Mock<SignInManager<IdentityUser>>(
            userManager.Object,
            contextAccessor.Object,
            claimsFactory.Object,
            null, null, null, null);
    }

    /// <summary>
    /// ログ出力の検証ヘルパーメソッド
    /// </summary>
    private void VerifyLogCalled(LogLevel expectedLevel, string expectedMessage)
    {
        _loggerMock.Verify(
            x => x.Log(
                expectedLevel,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains(expectedMessage)),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.AtLeastOnce);
    }

    #endregion
}