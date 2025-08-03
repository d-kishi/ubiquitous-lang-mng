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
using UbiquitousLanguageManager.Infrastructure.Data.Entities;
using UbiquitousLanguageManager.Tests.Stubs;

namespace UbiquitousLanguageManager.Tests.Infrastructure;

/// <summary>
/// 監査ログ機能のテストクラス
/// 
/// 【F#初学者向け解説】
/// Phase A3 Step4で実装した基本監査ログ機能の単体テスト。
/// ログイン成功/失敗、セキュリティイベント、構造化ログ出力を検証。
/// 
/// 📊 基本監査ログ機能の包括的テスト
/// 📝 ADR_008準拠: 構造化ログ出力の検証
/// </summary>
public class AuditLoggingTests
{
    private readonly Mock<Microsoft.Extensions.Logging.ILogger<AuthenticationService>> _logger;
    private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;
    private readonly Mock<SignInManager<ApplicationUser>> _signInManagerMock;
    private readonly Mock<INotificationService> _notificationServiceMock;
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly AuthenticationService _authenticationService;

    /// <summary>
    /// テスト初期化
    /// </summary>
    public AuditLoggingTests()
    {
        _logger = new Mock<Microsoft.Extensions.Logging.ILogger<AuthenticationService>>();
        
        // UserManager モック作成
        var mockUserStore = new Mock<IUserStore<ApplicationUser>>();
        _userManagerMock = new Mock<UserManager<ApplicationUser>>(mockUserStore.Object, null, null, null, null, null, null, null, null);
        
        // SignInManager モック作成
        var mockContextAccessor = new Mock<Microsoft.AspNetCore.Http.IHttpContextAccessor>();
        var mockUserPrincipalFactory = new Mock<IUserClaimsPrincipalFactory<ApplicationUser>>();
        _signInManagerMock = new Mock<SignInManager<ApplicationUser>>(_userManagerMock.Object, mockContextAccessor.Object, mockUserPrincipalFactory.Object, null, null, null, null);
        
        _notificationServiceMock = new Mock<INotificationService>();
        _userRepositoryMock = new Mock<IUserRepository>();

        _authenticationService = new AuthenticationService(
            _logger.Object,
            _userManagerMock.Object,
            _signInManagerMock.Object,
            _notificationServiceMock.Object,
            _userRepositoryMock.Object);
    }

    #region Login Success Audit Tests

    /// <summary>
    /// ログイン成功監査 - 情報レベルログ出力
    /// </summary>
    [Fact]
    public async Task LoginSuccess_ShouldGenerateInfoAuditLog()
    {
        // Arrange
        var email = Email.create("user@example.com").ResultValue;
        var identityUser = new IdentityUser { Email = email.Value };

        _userManagerMock.Setup(x => x.FindByEmailAsync(email.Value))
            .ReturnsAsync(identityUser);
        _userManagerMock.Setup(x => x.ResetAccessFailedCountAsync(identityUser))
            .ReturnsAsync(IdentityResult.Success);

        // Act
        var result = await _authenticationService.RecordLoginAttemptAsync(email, true);

        // Assert
        Assert.True(result.IsOk);

        // 📊 ADR_008準拠: 構造化ログ出力確認（Information レベル）
        VerifyLogCalled(LogLevel.Information, "Login attempt recorded");
        VerifyLogStructuredData(email.Value, "Success");
    }

    /// <summary>
    /// 自動ログイン成功監査 - 開始と完了ログ
    /// </summary>
    [Fact]
    public async Task AutoLoginSuccess_ShouldGenerateStartAndCompleteAuditLogs()
    {
        // Arrange
        var email = Email.create("user@example.com").ResultValue;
        var identityUser = new IdentityUser { Email = email.Value, EmailConfirmed = true };

        _userManagerMock.Setup(x => x.FindByEmailAsync(email.Value))
            .ReturnsAsync(identityUser);
        _userManagerMock.Setup(x => x.IsLockedOutAsync(identityUser))
            .ReturnsAsync(false);
        _signInManagerMock.Setup(x => x.SignInAsync(identityUser, false, null))
            .Returns(Task.CompletedTask);
        _userManagerMock.Setup(x => x.ResetAccessFailedCountAsync(identityUser))
            .ReturnsAsync(IdentityResult.Success);

        // Act
        var result = await _authenticationService.AutoLoginAfterPasswordResetAsync(email);

        // Assert
        Assert.True(result.IsOk);

        // 📊 自動ログイン開始ログ
        VerifyLogCalled(LogLevel.Information, "Auto login after password reset initiated");
        
        // 📊 自動ログイン完了ログ
        VerifyLogCalled(LogLevel.Information, "Auto login completed successfully");
        
        // 📊 ログイン試行記録ログ（内部で呼ばれる）
        VerifyLogCalled(LogLevel.Information, "Login attempt recorded");
    }

    #endregion

    #region Login Failure Audit Tests

    /// <summary>
    /// ログイン失敗監査 - 警告レベルログ出力
    /// </summary>
    [Fact]
    public async Task LoginFailure_ShouldGenerateWarningAuditLog()
    {
        // Arrange
        var email = Email.create("user@example.com").ResultValue;
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

        // 📊 ADR_008準拠: 構造化ログ出力確認（Warning レベル）
        VerifyLogCalled(LogLevel.Warning, "Login attempt recorded");
        VerifyLogStructuredData(email.Value, "Failed");
    }

    /// <summary>
    /// 存在しないユーザーでのログイン試行 - セキュリティ警告ログ
    /// </summary>
    [Fact]
    public async Task NonExistentUserLogin_ShouldGenerateSecurityWarningLog()
    {
        // Arrange
        var email = Email.create("nonexistent@example.com").ResultValue;

        _userManagerMock.Setup(x => x.FindByEmailAsync(email.Value))
            .ReturnsAsync((IdentityUser)null);

        // Act
        var result = await _authenticationService.AutoLoginAfterPasswordResetAsync(email);

        // Assert
        Assert.True(result.IsError);

        // 🚨 セキュリティ警告ログが出力されることを検証
        VerifyLogCalled(LogLevel.Warning, "Auto login attempted for non-existent user");
    }

    #endregion

    #region Security Event Audit Tests

    /// <summary>
    /// ロックアウト発生監査 - セキュリティイベントログ
    /// </summary>
    [Fact]
    public async Task AccountLockout_ShouldGenerateSecurityEventLog()
    {
        // Arrange
        var email = Email.create("locked@example.com").ResultValue;
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

        // 🔒 ロックアウトセキュリティイベントログ
        VerifyLogCalled(LogLevel.Warning, "User account locked due to failed login attempts");
        
        // 📊 構造化データ確認（メールアドレス、ロックアウト終了時刻）
        VerifyStructuredLogWithLockoutEnd(email.Value);
    }

    /// <summary>
    /// ロック済みアカウントでのログイン試行 - セキュリティ警告
    /// </summary>
    [Fact]
    public async Task LockedAccountLoginAttempt_ShouldGenerateSecurityWarning()
    {
        // Arrange
        var email = Email.create("locked@example.com").ResultValue;
        var identityUser = new IdentityUser { Email = email.Value };

        _userManagerMock.Setup(x => x.FindByEmailAsync(email.Value))
            .ReturnsAsync(identityUser);
        _userManagerMock.Setup(x => x.IsLockedOutAsync(identityUser))
            .ReturnsAsync(true);

        // Act
        var result = await _authenticationService.AutoLoginAfterPasswordResetAsync(email);

        // Assert
        Assert.True(result.IsError);

        // 🚨 ロック済みアカウントへのアクセス警告ログ
        VerifyLogCalled(LogLevel.Warning, "Auto login attempted for locked user");
    }

    #endregion

    #region Password Reset Audit Tests

    /// <summary>
    /// パスワードリセット要求監査 - 情報ログ
    /// </summary>
    [Fact]
    public async Task PasswordResetRequest_ShouldGenerateInfoAuditLog()
    {
        // Arrange
        var email = Email.create("user@example.com").ResultValue;
        var identityUser = new IdentityUser { Email = email.Value, EmailConfirmed = true };

        _userManagerMock.Setup(x => x.FindByEmailAsync(email.Value))
            .ReturnsAsync(identityUser);
        _userManagerMock.Setup(x => x.IsEmailConfirmedAsync(identityUser))
            .ReturnsAsync(true);
        _userManagerMock.Setup(x => x.GeneratePasswordResetTokenAsync(identityUser))
            .ReturnsAsync("reset-token");
        _notificationServiceMock.Setup(x => x.SendPasswordResetEmailAsync(
                It.IsAny<Email>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(FSharpResult<Microsoft.FSharp.Core.Unit, string>.NewOk(null!));

        // Act
        var result = await _authenticationService.RequestPasswordResetAsync(email);

        // Assert
        Assert.True(result.IsOk);

        // 📊 パスワードリセット要求ログ
        VerifyLogCalled(LogLevel.Information, "Password reset requested");
        
        // 📊 パスワードリセットメール送信成功ログ
        VerifyLogCalled(LogLevel.Information, "Password reset email sent successfully");
    }

    /// <summary>
    /// パスワードリセット完了監査 - 成功ログ
    /// </summary>
    [Fact]
    public async Task PasswordResetComplete_ShouldGenerateSuccessAuditLog()
    {
        // Arrange
        var email = Email.create("user@example.com").ResultValue;
        var password = Password.create("NewPassword123!").ResultValue;
        var token = "valid-token";
        var identityUser = new IdentityUser { Email = email.Value };

        _userManagerMock.Setup(x => x.FindByEmailAsync(email.Value))
            .ReturnsAsync(identityUser);
        _userManagerMock.Setup(x => x.ResetPasswordAsync(identityUser, It.IsAny<string>(), password.Value))
            .ReturnsAsync(IdentityResult.Success);
        _userManagerMock.Setup(x => x.UpdateSecurityStampAsync(identityUser))
            .ReturnsAsync(IdentityResult.Success);
        _notificationServiceMock.Setup(x => x.SendPasswordResetConfirmationAsync(email))
            .ReturnsAsync(FSharpResult<Microsoft.FSharp.Core.Unit, string>.NewOk(null!));

        // Act
        var result = await _authenticationService.ResetPasswordAsync(email, token, password);

        // Assert
        Assert.True(result.IsOk);

        // 📊 パスワードリセット開始ログ
        VerifyLogCalled(LogLevel.Information, "Password reset attempt");
        
        // 📊 パスワードリセット完了ログ
        VerifyLogCalled(LogLevel.Information, "Password reset completed successfully");
    }

    #endregion

    #region Error Audit Tests

    /// <summary>
    /// システムエラー監査 - エラーレベルログ
    /// </summary>
    [Fact]
    public async Task SystemError_ShouldGenerateErrorAuditLog()
    {
        // Arrange
        var email = Email.create("user@example.com").ResultValue;

        _userManagerMock.Setup(x => x.FindByEmailAsync(email.Value))
            .ThrowsAsync(new InvalidOperationException("Database connection error"));

        // Act
        var result = await _authenticationService.AutoLoginAfterPasswordResetAsync(email);

        // Assert
        Assert.True(result.IsError);

        // ❌ システムエラー監査ログ（Error レベル）
        VerifyLogCalled(LogLevel.Error, "Unexpected error during auto login");
    }

    /// <summary>
    /// 無効なトークン監査 - 警告ログ
    /// </summary>
    [Fact]
    public async Task InvalidTokenUsage_ShouldGenerateWarningAuditLog()
    {
        // Arrange
        var email = Email.create("user@example.com").ResultValue;
        var password = Password.create("NewPassword123!").ResultValue;
        var invalidToken = "invalid-token";
        var identityUser = new IdentityUser { Email = email.Value };

        _userManagerMock.Setup(x => x.FindByEmailAsync(email.Value))
            .ReturnsAsync(identityUser);
        _userManagerMock.Setup(x => x.ResetPasswordAsync(identityUser, It.IsAny<string>(), password.Value))
            .ReturnsAsync(IdentityResult.Failed(new IdentityError { Code = "InvalidToken", Description = "Invalid token" }));

        // Act
        var result = await _authenticationService.ResetPasswordAsync(email, invalidToken, password);

        // Assert
        Assert.True(result.IsError);

        // ⚠️ 無効トークン使用警告ログ
        VerifyLogCalled(LogLevel.Warning, "Password reset failed");
    }

    #endregion

    #region Log Structure Verification Tests

    /// <summary>
    /// ログ構造確認 - タイムスタンプ含有
    /// </summary>
    [Fact]
    public async Task AuditLog_ShouldContainTimestamp()
    {
        // Arrange
        var email = Email.create("user@example.com").ResultValue;
        var identityUser = new IdentityUser { Email = email.Value };

        _userManagerMock.Setup(x => x.FindByEmailAsync(email.Value))
            .ReturnsAsync(identityUser);
        _userManagerMock.Setup(x => x.ResetAccessFailedCountAsync(identityUser))
            .ReturnsAsync(IdentityResult.Success);

        // Act
        var result = await _authenticationService.RecordLoginAttemptAsync(email, true);

        // Assert
        Assert.True(result.IsOk);

        // 📅 タイムスタンプがログに含まれることを検証
        VerifyLogContainsTimestamp();
    }

    /// <summary>
    /// ログ構造確認 - メールアドレス含有
    /// </summary>
    [Fact]
    public async Task AuditLog_ShouldContainEmailAddress()
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

        // 📧 メールアドレスがログに含まれることを検証
        VerifyLogContainsEmail(email.Value);
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
        _logger.Verify(
            x => x.Log(
                expectedLevel,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains(expectedMessage)),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.AtLeastOnce);
    }

    /// <summary>
    /// 構造化ログデータの検証（メールアドレス・ステータス含有確認）
    /// </summary>
    private void VerifyLogStructuredData(string expectedEmail, string expectedStatus)
    {
        _logger.Verify(
            x => x.Log(
                It.IsAny<LogLevel>(),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => 
                    v.ToString().Contains(expectedEmail) && 
                    v.ToString().Contains(expectedStatus)),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.AtLeastOnce);
    }

    /// <summary>
    /// ロックアウト終了時刻を含む構造化ログの検証
    /// </summary>
    private void VerifyStructuredLogWithLockoutEnd(string expectedEmail)
    {
        _logger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => 
                    v.ToString().Contains(expectedEmail) && 
                    v.ToString().Contains("LockoutEnd")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.AtLeastOnce);
    }

    /// <summary>
    /// タイムスタンプ含有確認
    /// </summary>
    private void VerifyLogContainsTimestamp()
    {
        _logger.Verify(
            x => x.Log(
                It.IsAny<LogLevel>(),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Timestamp")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.AtLeastOnce);
    }

    /// <summary>
    /// メールアドレス含有確認
    /// </summary>
    private void VerifyLogContainsEmail(string expectedEmail)
    {
        _logger.Verify(
            x => x.Log(
                It.IsAny<LogLevel>(),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains(expectedEmail)),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.AtLeastOnce);
    }

    #endregion
}