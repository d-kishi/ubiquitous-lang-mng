using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.FSharp.Core;
using Moq;
using UbiquitousLanguageManager.Tests.Stubs;
// Extension methods are now top-level static classes
using Xunit;
using UbiquitousLanguageManager.Application;
using UbiquitousLanguageManager.Domain;
using UbiquitousLanguageManager.Infrastructure.Services;
using UbiquitousLanguageManager.Infrastructure.Data.Entities;

namespace UbiquitousLanguageManager.Tests.Integration;

/// <summary>
/// 自動ログイン統合テストクラス
/// 
/// 【F#初学者向け解説】
/// Phase A3 Step4で実装した自動ログイン機能の統合テスト。
/// パスワードリセット完了から自動ログイン、Remember Me、ロックアウト確認まで
/// 複数コンポーネント間の連携を検証。
/// 
/// 🔗 自動ログイン統合機能の包括的テスト
/// 📊 ADR_008準拠: エンドツーエンドログ出力検証
/// </summary>
public class AutoLoginIntegrationTests
{
    private readonly Mock<Microsoft.Extensions.Logging.ILogger<AuthenticationService>> _loggerMock;
    private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;
    private readonly Mock<SignInManager<ApplicationUser>> _signInManagerMock;
    private readonly Mock<INotificationService> _notificationServiceMock;
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly AuthenticationService _authenticationService;

    /// <summary>
    /// テスト初期化
    /// </summary>
    public AutoLoginIntegrationTests()
    {
        _loggerMock = new Mock<Microsoft.Extensions.Logging.ILogger<AuthenticationService>>();
        
        // UserManager モック作成
        var mockUserStore = new Mock<IUserStore<ApplicationUser>>();
        _userManagerMock = new Mock<UserManager<ApplicationUser>>(mockUserStore.Object, null, null, null, null, null, null, null, null);
        
        // SignInManager モック作成
        var mockContextAccessor = new Mock<Microsoft.AspNetCore.Http.IHttpContextAccessor>();
        var mockUserPrincipalFactory = new Mock<IUserClaimsPrincipalFactory<ApplicationUser>>();
        _signInManagerMock = new Mock<SignInManager<ApplicationUser>>(_userManagerMock.Object, mockContextAccessor.Object, mockUserPrincipalFactory.Object, null, null, null, null);
        
        _notificationServiceMock = new Mock<INotificationService>();
        _userRepositoryMock = new Mock<IUserRepository>();

        // Phase A3本格実装完了に対応
        _authenticationService = new AuthenticationService(
            _loggerMock.Object,
            _userManagerMock.Object,
            _signInManagerMock.Object,
            _notificationServiceMock.Object,
            _userRepositoryMock.Object);
    }

    #region Complete Password Reset Flow Tests

    /// <summary>
    /// パスワードリセット完全フロー - 要求から自動ログインまで
    /// </summary>
    [Fact]
    public async Task CompletePasswordResetFlow_ShouldSucceedWithAutoLogin()
    {
        // Arrange
        var email = Email.create("user@example.com").ResultValue;
        var newPassword = Password.create("NewPassword123!").ResultValue;
        var resetToken = "valid-reset-token";
        var identityUser = new IdentityUser { Email = email.Value, EmailConfirmed = true };

        // パスワードリセット要求のセットアップ
        _userManagerMock.Setup(x => x.FindByEmailAsync(email.Value))
            .ReturnsAsync(identityUser);
        _userManagerMock.Setup(x => x.IsEmailConfirmedAsync(identityUser))
            .ReturnsAsync(true);
        _userManagerMock.Setup(x => x.GeneratePasswordResetTokenAsync(identityUser))
            .ReturnsAsync(resetToken);
        _notificationServiceMock.Setup(x => x.SendPasswordResetEmailAsync(
                It.IsAny<Email>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(FSharpResult<Microsoft.FSharp.Core.Unit, string>.NewOk(null!));

        // パスワードリセット実行のセットアップ
        _userManagerMock.Setup(x => x.ResetPasswordAsync(identityUser, It.IsAny<string>(), newPassword.Value))
            .ReturnsAsync(IdentityResult.Success);
        _userManagerMock.Setup(x => x.UpdateSecurityStampAsync(identityUser))
            .ReturnsAsync(IdentityResult.Success);
        _notificationServiceMock.Setup(x => x.SendPasswordResetConfirmationAsync(email))
            .ReturnsAsync(FSharpResult<Microsoft.FSharp.Core.Unit, string>.NewOk(null!));

        // 自動ログインのセットアップ
        _userManagerMock.Setup(x => x.IsLockedOutAsync(identityUser))
            .ReturnsAsync(false);
        _signInManagerMock.Setup(x => x.SignInAsync(identityUser, false, null))
            .Returns(Task.CompletedTask);
        _userManagerMock.Setup(x => x.ResetAccessFailedCountAsync(identityUser))
            .ReturnsAsync(IdentityResult.Success);

        // Act - 完全フロー実行

        // Step 1: パスワードリセット要求
        var requestResult = await _authenticationService.RequestPasswordResetAsync(email);

        // Step 2: パスワードリセット実行
        var resetResult = await _authenticationService.ResetPasswordAsync(email, resetToken, newPassword);

        // Step 3: 自動ログイン実行
        var autoLoginResult = await _authenticationService.AutoLoginAfterPasswordResetAsync(email);

        // Assert
        Assert.True(requestResult.IsOk);
        Assert.True(resetResult.IsOk);
        Assert.True(autoLoginResult.IsOk);

        // 🔗 完全フローの各段階検証
        
        // パスワードリセット要求ログ
        VerifyLogCalled(LogLevel.Information, "Password reset requested");
        VerifyLogCalled(LogLevel.Information, "Password reset email sent successfully");
        
        // パスワードリセット実行ログ
        VerifyLogCalled(LogLevel.Information, "Password reset attempt");
        VerifyLogCalled(LogLevel.Information, "Password reset completed successfully");
        
        // 自動ログインログ
        VerifyLogCalled(LogLevel.Information, "Auto login after password reset initiated");
        VerifyLogCalled(LogLevel.Information, "Auto login completed successfully");

        // SignInManagerの呼び出し確認
        _signInManagerMock.Verify(x => x.SignInAsync(identityUser, false, null), Times.Once);
        
        // セキュリティスタンプ更新確認
        _userManagerMock.Verify(x => x.UpdateSecurityStampAsync(identityUser), Times.Once);
    }

    /// <summary>
    /// パスワードリセット後自動ログイン失敗 - ロックアウト状態
    /// </summary>
    [Fact]
    public async Task PasswordResetSuccessButAutoLoginFails_LockedAccount_ShouldLogWarning()
    {
        // Arrange
        var email = Email.create("locked@example.com").ResultValue;
        var newPassword = Password.create("NewPassword123!").ResultValue;
        var resetToken = "valid-reset-token";
        var identityUser = new IdentityUser { Email = email.Value, EmailConfirmed = true };

        // パスワードリセット成功のセットアップ
        _userManagerMock.Setup(x => x.FindByEmailAsync(email.Value))
            .ReturnsAsync(identityUser);
        _userManagerMock.Setup(x => x.ResetPasswordAsync(identityUser, It.IsAny<string>(), newPassword.Value))
            .ReturnsAsync(IdentityResult.Success);
        _userManagerMock.Setup(x => x.UpdateSecurityStampAsync(identityUser))
            .ReturnsAsync(IdentityResult.Success);
        _notificationServiceMock.Setup(x => x.SendPasswordResetConfirmationAsync(email))
            .ReturnsAsync(FSharpResult<Microsoft.FSharp.Core.Unit, string>.NewOk(null!));

        // 自動ログイン失敗のセットアップ（ロックアウト状態）
        _userManagerMock.Setup(x => x.IsLockedOutAsync(identityUser))
            .ReturnsAsync(true);

        // Act
        var resetResult = await _authenticationService.ResetPasswordAsync(email, resetToken, newPassword);
        var autoLoginResult = await _authenticationService.AutoLoginAfterPasswordResetAsync(email);

        // Assert
        Assert.True(resetResult.IsOk); // パスワードリセット成功
        Assert.True(autoLoginResult.IsError); // 自動ログイン失敗
        Assert.Contains("アカウントがロックされています", autoLoginResult.ErrorValue);

        // 🔗 混合結果の適切なログ出力確認
        
        // パスワードリセット成功ログ
        VerifyLogCalled(LogLevel.Information, "Password reset completed successfully");
        
        // 自動ログイン失敗ログ
        VerifyLogCalled(LogLevel.Warning, "Auto login attempted for locked user");

        // SignInAsyncが呼ばれないことを確認
        _signInManagerMock.Verify(x => x.SignInAsync(It.IsAny<IdentityUser>(), It.IsAny<bool>(), It.IsAny<string>()), Times.Never);
    }

    #endregion

    #region Multi-Component Interaction Tests

    /// <summary>
    /// 複数ログイン試行からロックアウトまでの統合テスト
    /// </summary>
    [Fact]
    public async Task MultipleFailedLoginAttempts_ShouldTriggerLockout()
    {
        // Arrange
        var email = Email.create("user@example.com").ResultValue;
        var identityUser = new IdentityUser { Email = email.Value };

        _userManagerMock.Setup(x => x.FindByEmailAsync(email.Value))
            .ReturnsAsync(identityUser);
        _userManagerMock.Setup(x => x.AccessFailedAsync(identityUser))
            .ReturnsAsync(IdentityResult.Success);

        // 5回目でロックアウト発生
        var lockoutCallCount = 0;
        _userManagerMock.Setup(x => x.IsLockedOutAsync(identityUser))
            .ReturnsAsync(() => ++lockoutCallCount >= 5);
        
        var lockoutEnd = DateTimeOffset.UtcNow.AddMinutes(15);
        _userManagerMock.Setup(x => x.GetLockoutEndDateAsync(identityUser))
            .ReturnsAsync(lockoutEnd);

        // Act - 5回のログイン失敗を記録
        for (int i = 0; i < 5; i++)
        {
            await _authenticationService.RecordLoginAttemptAsync(email, false);
        }

        // ロックアウト後の自動ログイン試行
        var autoLoginResult = await _authenticationService.AutoLoginAfterPasswordResetAsync(email);

        // Assert
        Assert.True(autoLoginResult.IsError);
        Assert.Contains("アカウントがロックされています", autoLoginResult.ErrorValue);

        // 🔗 統合的なログ出力確認
        
        // 5回のログイン失敗記録
        VerifyLogCalledTimes(LogLevel.Warning, "Login attempt recorded", 5);
        
        // ロックアウト発生ログ
        VerifyLogCalled(LogLevel.Warning, "User account locked due to failed login attempts");
        
        // ロックアウト後のログイン試行警告
        VerifyLogCalled(LogLevel.Warning, "Auto login attempted for locked user");

        // AccessFailedAsyncが5回呼ばれることを確認
        _userManagerMock.Verify(x => x.AccessFailedAsync(identityUser), Times.Exactly(5));
    }

    /// <summary>
    /// ログイン成功によるロックアウト解除統合テスト
    /// </summary>
    [Fact]
    public async Task LoginSuccessAfterFailures_ShouldResetLockoutState()
    {
        // Arrange
        var email = Email.create("recovery@example.com").ResultValue;
        var identityUser = new IdentityUser { Email = email.Value };

        _userManagerMock.Setup(x => x.FindByEmailAsync(email.Value))
            .ReturnsAsync(identityUser);
        
        // 最初は失敗カウントあり、後でリセット
        _userManagerMock.Setup(x => x.AccessFailedAsync(identityUser))
            .ReturnsAsync(IdentityResult.Success);
        _userManagerMock.Setup(x => x.ResetAccessFailedCountAsync(identityUser))
            .ReturnsAsync(IdentityResult.Success);
        _userManagerMock.Setup(x => x.IsLockedOutAsync(identityUser))
            .ReturnsAsync(false);

        // 自動ログインのセットアップ
        _signInManagerMock.Setup(x => x.SignInAsync(identityUser, false, null))
            .Returns(Task.CompletedTask);

        // Act
        
        // Step 1: ログイン失敗記録
        await _authenticationService.RecordLoginAttemptAsync(email, false);
        await _authenticationService.RecordLoginAttemptAsync(email, false);
        
        // Step 2: ログイン成功（自動ログイン）
        var autoLoginResult = await _authenticationService.AutoLoginAfterPasswordResetAsync(email);

        // Assert
        Assert.True(autoLoginResult.IsOk);

        // 🔗 ロックアウト回復フローの統合確認
        
        // 失敗記録ログ
        VerifyLogCalledTimes(LogLevel.Warning, "Login attempt recorded", 2);
        
        // 自動ログイン成功ログ
        VerifyLogCalled(LogLevel.Information, "Auto login completed successfully");
        
        // 成功による失敗カウントリセットログ（内部で呼ばれる）
        VerifyLogCalled(LogLevel.Information, "Login attempt recorded");

        // 失敗カウントリセット確認
        _userManagerMock.Verify(x => x.ResetAccessFailedCountAsync(identityUser), Times.Once);
        
        // 自動ログイン実行確認
        _signInManagerMock.Verify(x => x.SignInAsync(identityUser, false, null), Times.Once);
    }

    #endregion

    #region Error Recovery Integration Tests

    /// <summary>
    /// メール送信失敗時のパスワードリセット統合エラーハンドリング
    /// </summary>
    [Fact]
    public async Task PasswordResetEmailFailure_ShouldReturnErrorGracefully()
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
        
        // メール送信失敗のシミュレーション
        _notificationServiceMock.Setup(x => x.SendPasswordResetEmailAsync(
                It.IsAny<Email>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(FSharpResult<Microsoft.FSharp.Core.Unit, string>.NewError("SMTP server unavailable"));

        // Act
        var result = await _authenticationService.RequestPasswordResetAsync(email);

        // Assert
        Assert.True(result.IsError);
        Assert.Contains("メール送信中にエラーが発生しました", result.ErrorValue);

        // 🔗 エラー統合処理の確認
        
        // 要求開始ログ
        VerifyLogCalled(LogLevel.Information, "Password reset requested");
        
        // メール送信失敗エラーログ
        VerifyLogCalled(LogLevel.Error, "Failed to send password reset email");

        // トークンは生成されたが、メール送信で失敗
        _userManagerMock.Verify(x => x.GeneratePasswordResetTokenAsync(identityUser), Times.Once);
    }

    /// <summary>
    /// 通知サービス例外時の自動ログイン統合テスト
    /// </summary>
    [Fact]
    public async Task AutoLoginWithNotificationFailure_ShouldCompleteLoginButLogWarning()
    {
        // Arrange
        var email = Email.create("user@example.com").ResultValue;
        var newPassword = Password.create("NewPassword123!").ResultValue;
        var resetToken = "valid-reset-token";
        var identityUser = new IdentityUser { Email = email.Value, EmailConfirmed = true };

        // パスワードリセット成功のセットアップ
        _userManagerMock.Setup(x => x.FindByEmailAsync(email.Value))
            .ReturnsAsync(identityUser);
        _userManagerMock.Setup(x => x.ResetPasswordAsync(identityUser, It.IsAny<string>(), newPassword.Value))
            .ReturnsAsync(IdentityResult.Success);
        _userManagerMock.Setup(x => x.UpdateSecurityStampAsync(identityUser))
            .ReturnsAsync(IdentityResult.Success);
        
        // 確認メール送信失敗のシミュレーション
        _notificationServiceMock.Setup(x => x.SendPasswordResetConfirmationAsync(email))
            .ReturnsAsync(FSharpResult<Microsoft.FSharp.Core.Unit, string>.NewError("Notification service down"));

        // 自動ログイン成功のセットアップ
        _userManagerMock.Setup(x => x.IsLockedOutAsync(identityUser))
            .ReturnsAsync(false);
        _signInManagerMock.Setup(x => x.SignInAsync(identityUser, false, null))
            .Returns(Task.CompletedTask);
        _userManagerMock.Setup(x => x.ResetAccessFailedCountAsync(identityUser))
            .ReturnsAsync(IdentityResult.Success);

        // Act
        var resetResult = await _authenticationService.ResetPasswordAsync(email, resetToken, newPassword);
        var autoLoginResult = await _authenticationService.AutoLoginAfterPasswordResetAsync(email);

        // Assert
        Assert.True(resetResult.IsOk); // パスワードリセット成功
        Assert.True(autoLoginResult.IsOk); // 自動ログイン成功

        // 🔗 部分的失敗の統合処理確認
        
        // パスワードリセット成功ログ
        VerifyLogCalled(LogLevel.Information, "Password reset completed successfully");
        
        // 確認メール送信失敗警告ログ
        VerifyLogCalled(LogLevel.Warning, "Failed to send password reset confirmation email");
        
        // 自動ログイン成功ログ
        VerifyLogCalled(LogLevel.Information, "Auto login completed successfully");

        // コア機能は正常動作確認
        _userManagerMock.Verify(x => x.ResetPasswordAsync(identityUser, It.IsAny<string>(), newPassword.Value), Times.Once);
        _signInManagerMock.Verify(x => x.SignInAsync(identityUser, false, null), Times.Once);
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

    /// <summary>
    /// 指定回数のログ出力検証ヘルパーメソッド
    /// </summary>
    private void VerifyLogCalledTimes(LogLevel expectedLevel, string expectedMessage, int expectedTimes)
    {
        _loggerMock.Verify(
            x => x.Log(
                expectedLevel,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains(expectedMessage)),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Exactly(expectedTimes));
    }

    #endregion
}