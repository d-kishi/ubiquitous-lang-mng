using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Identity;
using Microsoft.FSharp.Core;
using Moq;
using Xunit;
using UbiquitousLanguageManager.Application;
using UbiquitousLanguageManager.Domain.Common;
using UbiquitousLanguageManager.Domain.Authentication;
using UbiquitousLanguageManager.Infrastructure.Data.Entities;
using UbiquitousLanguageManager.Infrastructure.Services;

namespace UbiquitousLanguageManager.Tests.Infrastructure;

/// <summary>
/// Phase A8 対応: AuthenticationService統合テスト（実装済みメソッド対応版）
/// 
/// 【テスト方針】
/// - 実装済みメソッド: 実際の動作を検証
/// - 未実装メソッド: 適切なエラーメッセージを返すことを検証
/// - ログ出力の検証も含む
/// </summary>
public class AuthenticationServiceTests
{
    private readonly Mock<Microsoft.Extensions.Logging.ILogger<UbiquitousLanguageManager.Infrastructure.Services.AuthenticationService>> _mockLogger;
    private readonly Mock<UserManager<ApplicationUser>> _mockUserManager;
    private readonly Mock<SignInManager<ApplicationUser>> _mockSignInManager;
    private readonly Mock<INotificationService> _mockNotificationService;
    private readonly Mock<IUserRepository> _mockUserRepository;
    private readonly UbiquitousLanguageManager.Infrastructure.Services.AuthenticationService _service;

    public AuthenticationServiceTests()
    {
        _mockLogger = new Mock<Microsoft.Extensions.Logging.ILogger<UbiquitousLanguageManager.Infrastructure.Services.AuthenticationService>>();
        
        // UserManager モック作成（適切なコンストラクタ引数付き）
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
        
        _service = new UbiquitousLanguageManager.Infrastructure.Services.AuthenticationService(
            _mockLogger.Object,
            _mockUserManager.Object,
            _mockSignInManager.Object,
            _mockNotificationService.Object,
            _mockUserRepository.Object);
    }

    /// <summary>
    /// 実装済みメソッド: HashPasswordAsyncのテスト
    /// </summary>
    [Fact]
    public async Task HashPasswordAsync_ValidPassword_ShouldReturnHashedPassword()
    {
        // Arrange
        var passwordResult = Password.create("su");
        Assert.True(passwordResult.IsOk);
        var password = passwordResult.ResultValue;

        // Act
        var result = await _service.HashPasswordAsync(password);

        // Assert
        Assert.True(result.IsOk);
        Assert.NotNull(result.ResultValue);
        Assert.NotEmpty(result.ResultValue.Value);
    }

    /// <summary>
    /// 実装済みメソッド: VerifyPasswordAsyncのテスト
    /// </summary>
    [Fact]
    public async Task VerifyPasswordAsync_ValidPasswordAndHash_ShouldReturnVerificationResult()
    {
        // Arrange
        var password = "testPassword";
        var hashResult = PasswordHash.create("hashedPassword");
        Assert.True(hashResult.IsOk);
        var hash = hashResult.ResultValue;

        // Act
        var result = await _service.VerifyPasswordAsync(password, hash);

        // Assert
        Assert.True(result.IsOk);
        Assert.IsType<bool>(result.ResultValue);
    }

    /// <summary>
    /// 実装済みメソッド: GenerateTokenAsyncのテスト
    /// </summary>
    [Fact]
    public async Task GenerateTokenAsync_ValidUser_ShouldReturnToken()
    {
        // Arrange
        var emailResult = Email.create("test@example.com");
        var nameResult = UserName.create("テストユーザー");
        
        Assert.True(emailResult.IsOk);
        Assert.True(nameResult.IsOk);

        var user = User.create(
            emailResult.ResultValue,
            nameResult.ResultValue,
            Role.GeneralUser,
            UserId.create(1L)
        );

        var identityUser = new ApplicationUser
        {
            Id = "1",
            Email = "test@example.com",
            UserName = "test@example.com"
        };

        _mockUserManager.Setup(x => x.FindByIdAsync("1"))
                       .ReturnsAsync(identityUser);
        _mockUserManager.Setup(x => x.GeneratePasswordResetTokenAsync(identityUser))
                       .ReturnsAsync("generated-token");

        // Act
        var result = await _service.GenerateTokenAsync(user);

        // Assert
        Assert.True(result.IsOk);
        Assert.Equal("generated-token", result.ResultValue);
    }

    /// <summary>
    /// 実装済みメソッド: RecordLoginAttemptAsyncのテスト（成功時）
    /// </summary>
    [Fact]
    public async Task RecordLoginAttemptAsync_SuccessfulLogin_ShouldResetFailedCount()
    {
        // Arrange
        var emailResult = Email.create("test@example.com");
        Assert.True(emailResult.IsOk);
        var email = emailResult.ResultValue;

        var identityUser = new ApplicationUser { Email = email.Value };

        _mockUserManager.Setup(x => x.FindByEmailAsync(email.Value))
                       .ReturnsAsync(identityUser);
        _mockUserManager.Setup(x => x.ResetAccessFailedCountAsync(identityUser))
                       .ReturnsAsync(IdentityResult.Success);

        // Act
        var result = await _service.RecordLoginAttemptAsync(email, true);

        // Assert
        Assert.True(result.IsOk);
        _mockUserManager.Verify(x => x.ResetAccessFailedCountAsync(identityUser), Times.Once);
    }

    /// <summary>
    /// 実装済みメソッド: RecordLoginAttemptAsyncのテスト（失敗時）
    /// </summary>
    [Fact]
    public async Task RecordLoginAttemptAsync_FailedLogin_ShouldIncrementFailedCount()
    {
        // Arrange
        var emailResult = Email.create("test@example.com");
        Assert.True(emailResult.IsOk);
        var email = emailResult.ResultValue;

        var identityUser = new ApplicationUser { Email = email.Value };

        _mockUserManager.Setup(x => x.FindByEmailAsync(email.Value))
                       .ReturnsAsync(identityUser);
        _mockUserManager.Setup(x => x.AccessFailedAsync(identityUser))
                       .ReturnsAsync(IdentityResult.Success);
        _mockUserManager.Setup(x => x.IsLockedOutAsync(identityUser))
                       .ReturnsAsync(false);

        // Act
        var result = await _service.RecordLoginAttemptAsync(email, false);

        // Assert
        Assert.True(result.IsOk);
        _mockUserManager.Verify(x => x.AccessFailedAsync(identityUser), Times.Once);
    }

    /// <summary>
    /// 実装済みメソッド: IsAccountLockedAsyncのテスト
    /// </summary>
    [Fact]
    public async Task IsAccountLockedAsync_LockedUser_ShouldReturnTrue()
    {
        // Arrange
        var emailResult = Email.create("locked@example.com");
        Assert.True(emailResult.IsOk);
        var email = emailResult.ResultValue;

        var identityUser = new ApplicationUser { Email = email.Value };

        _mockUserManager.Setup(x => x.FindByEmailAsync(email.Value))
                       .ReturnsAsync(identityUser);
        _mockUserManager.Setup(x => x.IsLockedOutAsync(identityUser))
                       .ReturnsAsync(true);

        // Act
        var result = await _service.IsAccountLockedAsync(email);

        // Assert
        Assert.True(result.IsOk);
        Assert.True(result.ResultValue);
    }

    /// <summary>
    /// 実装済みメソッド: IsAccountLockedAsyncのテスト（存在しないユーザー）
    /// </summary>
    [Fact]
    public async Task IsAccountLockedAsync_NonExistentUser_ShouldReturnFalse()
    {
        // Arrange
        var emailResult = Email.create("nonexistent@example.com");
        Assert.True(emailResult.IsOk);
        var email = emailResult.ResultValue;

        _mockUserManager.Setup(x => x.FindByEmailAsync(email.Value))
                       .ReturnsAsync((ApplicationUser)null);

        // Act
        var result = await _service.IsAccountLockedAsync(email);

        // Assert
        Assert.True(result.IsOk);
        Assert.False(result.ResultValue);
    }

    /// <summary>
    /// 実装済みメソッド: AutoLoginAfterPasswordResetAsyncのテスト
    /// </summary>
    [Fact]
    public async Task AutoLoginAfterPasswordResetAsync_ValidUser_ShouldSucceed()
    {
        // Arrange
        var emailResult = Email.create("test@example.com");
        Assert.True(emailResult.IsOk);
        var email = emailResult.ResultValue;

        var identityUser = new ApplicationUser 
        { 
            Email = email.Value,
            Name = "テストユーザー",
            Id = "1"
        };

        _mockUserManager.Setup(x => x.FindByEmailAsync(email.Value))
                       .ReturnsAsync(identityUser);
        _mockUserManager.Setup(x => x.IsLockedOutAsync(identityUser))
                       .ReturnsAsync(false);
        _mockSignInManager.Setup(x => x.SignInAsync(identityUser, false, null))
                         .Returns(Task.CompletedTask);

        // Act
        var result = await _service.AutoLoginAfterPasswordResetAsync(email);

        // Assert
        Assert.True(result.IsOk);
        Assert.NotNull(result.ResultValue);
        _mockSignInManager.Verify(x => x.SignInAsync(identityUser, false, null), Times.Once);
    }

    /// <summary>
    /// 実装済みメソッド: AutoLoginAfterPasswordResetAsyncのテスト（ロック中ユーザー）
    /// </summary>
    [Fact]
    public async Task AutoLoginAfterPasswordResetAsync_LockedUser_ShouldFail()
    {
        // Arrange
        var emailResult = Email.create("locked@example.com");
        Assert.True(emailResult.IsOk);
        var email = emailResult.ResultValue;

        var identityUser = new ApplicationUser { Email = email.Value };

        _mockUserManager.Setup(x => x.FindByEmailAsync(email.Value))
                       .ReturnsAsync(identityUser);
        _mockUserManager.Setup(x => x.IsLockedOutAsync(identityUser))
                       .ReturnsAsync(true);

        // Act
        var result = await _service.AutoLoginAfterPasswordResetAsync(email);

        // Assert
        Assert.True(result.IsError);
        Assert.Contains("アカウントがロックされています", result.ErrorValue);
        _mockSignInManager.Verify(x => x.SignInAsync(It.IsAny<ApplicationUser>(), It.IsAny<bool>(), It.IsAny<string>()), Times.Never);
    }

    /// <summary>
    /// 簡易実装メソッド: ValidateTokenAsyncのテスト
    /// </summary>
    [Fact]
    public async Task ValidateTokenAsync_ShouldReturnNotImplementedError()
    {
        // Arrange
        var token = "test-token";

        // Act
        var result = await _service.ValidateTokenAsync(token);

        // Assert
        Assert.True(result.IsError);
        Assert.Contains("トークン検証は専用サービスで実装予定", result.ErrorValue);
    }

    /// <summary>
    /// 簡易実装メソッド: GetCurrentUserAsyncのテスト
    /// </summary>
    [Fact]
    public async Task GetCurrentUserAsync_ShouldReturnNone()
    {
        // Act
        var result = await _service.GetCurrentUserAsync();

        // Assert
        Assert.True(result.IsOk);
        Assert.True(FSharpOption<User>.get_IsNone(result.ResultValue));
    }

    /// <summary>
    /// 簡易実装メソッド: 2FA関連メソッドのテスト
    /// </summary>
    [Fact]
    public async Task EnableTwoFactorAsync_ShouldReturnNotImplementedError()
    {
        // Arrange
        var userId = UserId.create(1L);

        // Act
        var result = await _service.EnableTwoFactorAsync(userId);

        // Assert
        Assert.True(result.IsError);
        Assert.Contains("2FA機能は後期実装予定", result.ErrorValue);
    }

    [Fact]
    public async Task DisableTwoFactorAsync_ShouldReturnNotImplementedError()
    {
        // Arrange
        var userId = UserId.create(1L);

        // Act
        var result = await _service.DisableTwoFactorAsync(userId);

        // Assert
        Assert.True(result.IsError);
        Assert.Contains("2FA機能は後期実装予定", result.ErrorValue);
    }

    [Fact]
    public async Task VerifyTwoFactorCodeAsync_ShouldReturnNotImplementedError()
    {
        // Arrange
        var userId = UserId.create(1L);
        var code = "123456";

        // Act
        var result = await _service.VerifyTwoFactorCodeAsync(userId, code);

        // Assert
        Assert.True(result.IsError);
        Assert.Contains("2FA機能は後期実装予定", result.ErrorValue);
    }

    /// <summary>
    /// 簡易実装メソッド: メール確認関連メソッドのテスト
    /// </summary>
    [Fact]
    public async Task SendEmailConfirmationAsync_ShouldReturnOk()
    {
        // Arrange
        var emailResult = Email.create("test@example.com");
        Assert.True(emailResult.IsOk);
        var email = emailResult.ResultValue;

        // Act
        var result = await _service.SendEmailConfirmationAsync(email);

        // Assert
        Assert.True(result.IsOk);
    }

    [Fact]
    public async Task ConfirmEmailAsync_ShouldReturnOk()
    {
        // Arrange
        var userId = UserId.create(1L);
        var token = "confirmation-token";

        // Act
        var result = await _service.ConfirmEmailAsync(userId, token);

        // Assert
        Assert.True(result.IsOk);
    }
}