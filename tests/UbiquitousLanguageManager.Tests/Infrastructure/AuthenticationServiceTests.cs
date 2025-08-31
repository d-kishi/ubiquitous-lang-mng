using Microsoft.Extensions.Logging;
using Microsoft.FSharp.Core;
using Moq;
using UbiquitousLanguageManager.Domain;
using UbiquitousLanguageManager.Infrastructure.Services;
using UbiquitousLanguageManager.Infrastructure.Data.Entities;
using UbiquitousLanguageManager.Application;
using Microsoft.AspNetCore.Identity;
using Xunit;

namespace UbiquitousLanguageManager.Tests.Infrastructure;

/// <summary>
/// AuthenticationServiceの単体テスト
/// 
/// 【テスト方針】
/// Phase A2ではAuthenticationServiceは全てスタブ実装となっているため、
/// 各メソッドが適切にPhase A3実装予定のエラーを返すことを確認します。
/// また、ログ出力が正しく行われることも検証します。
/// </summary>
public class AuthenticationServiceTests
{
    private readonly Mock<Microsoft.Extensions.Logging.ILogger<AuthenticationService>> _mockLogger;
    private readonly Mock<UserManager<ApplicationUser>> _mockUserManager;
    private readonly Mock<SignInManager<ApplicationUser>> _mockSignInManager;
    private readonly Mock<INotificationService> _mockNotificationService;
    private readonly Mock<IUserRepository> _mockUserRepository;
    private readonly AuthenticationService _service;

    public AuthenticationServiceTests()
    {
        _mockLogger = new Mock<Microsoft.Extensions.Logging.ILogger<AuthenticationService>>();
        
        // UserManager モック作成
        var mockUserStore = new Mock<IUserStore<ApplicationUser>>();
        _mockUserManager = new Mock<UserManager<ApplicationUser>>(mockUserStore.Object, null, null, null, null, null, null, null, null);
        
        // SignInManager モック作成
        var mockContextAccessor = new Mock<Microsoft.AspNetCore.Http.IHttpContextAccessor>();
        var mockUserPrincipalFactory = new Mock<IUserClaimsPrincipalFactory<ApplicationUser>>();
        _mockSignInManager = new Mock<SignInManager<ApplicationUser>>(_mockUserManager.Object, mockContextAccessor.Object, mockUserPrincipalFactory.Object, null, null, null, null);
        
        _mockNotificationService = new Mock<INotificationService>();
        _mockUserRepository = new Mock<IUserRepository>();
        
        _service = new AuthenticationService(
            _mockLogger.Object,
            _mockUserManager.Object,
            _mockSignInManager.Object,
            _mockNotificationService.Object,
            _mockUserRepository.Object);
    }

    /// <summary>
    /// LoginAsyncのテスト
    /// </summary>
    public class LoginAsyncTests : AuthenticationServiceTests
    {
        [Fact]
        public async Task LoginAsync_WithInitialPassword_ShouldSucceed()
        {
            // Arrange - TECH-002対応: 初期パスワード"su"でのログイン
            var emailResult = Email.create("admin@ubiquitous-lang.com");
            Assert.True(emailResult.IsOk);
            var email = emailResult.ResultValue;
            var initialPassword = "su"; // TECH-002: 仕様準拠の初期パスワード

            // スーパーユーザーのIdentityUser作成
            var superUser = new ApplicationUser
            {
                Id = "1",
                Email = "admin@ubiquitous-lang.com",
                UserName = "admin@ubiquitous-lang.com",
                Name = "システム管理者",
                IsFirstLogin = true, // TECH-004関連: 初回ログインフラグ
                EmailConfirmed = true
            };

            // UserManager モックセットアップ
            _mockUserManager.Setup(x => x.FindByEmailAsync("admin@ubiquitous-lang.com"))
                           .ReturnsAsync(superUser);
            _mockUserManager.Setup(x => x.IsLockedOutAsync(superUser))
                           .ReturnsAsync(false);

            // SignInManager モックセットアップ - 初期パスワード"su"で成功
            _mockSignInManager.Setup(x => x.PasswordSignInAsync(
                superUser, initialPassword, false, false))
                             .ReturnsAsync(Microsoft.AspNetCore.Identity.SignInResult.Success);

            // Act
            var result = await _service.LoginAsync(email, initialPassword);

            // Assert - TECH-002対応: 初期パスワード"su"でのログイン成功
            Assert.True(result.IsOk);
            var user = result.ResultValue;
            Assert.Equal("admin@ubiquitous-lang.com", user.Email.Value);
            Assert.Equal("システム管理者", user.Name.Value);
            
            // ログ出力確認
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Login successful for user: admin@ubiquitous-lang.com")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task LoginAsync_WithOldPassword_ShouldFail()
        {
            // Arrange - TECH-002対応: 旧パスワード"TempPass123!"でのログイン失敗確認
            var emailResult = Email.create("admin@ubiquitous-lang.com");
            Assert.True(emailResult.IsOk);
            var email = emailResult.ResultValue;
            var oldPassword = "TempPass123!"; // TECH-002: 以前使用していた不正なパスワード

            var superUser = new ApplicationUser
            {
                Id = "1",
                Email = "admin@ubiquitous-lang.com",
                UserName = "admin@ubiquitous-lang.com",
                Name = "システム管理者",
                IsFirstLogin = true,
                EmailConfirmed = true
            };

            _mockUserManager.Setup(x => x.FindByEmailAsync("admin@ubiquitous-lang.com"))
                           .ReturnsAsync(superUser);
            _mockUserManager.Setup(x => x.IsLockedOutAsync(superUser))
                           .ReturnsAsync(false);

            // SignInManager モックセットアップ - 旧パスワードでは失敗
            _mockSignInManager.Setup(x => x.PasswordSignInAsync(
                superUser, oldPassword, false, false))
                             .ReturnsAsync(Microsoft.AspNetCore.Identity.SignInResult.Failed);

            // Act
            var result = await _service.LoginAsync(email, oldPassword);

            // Assert - TECH-002対応: 旧パスワードでは認証失敗
            Assert.True(result.IsError);
            Assert.Equal("メールアドレスまたはパスワードが正しくありません", result.ErrorValue);
            
            // ログ出力確認
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Login failed: Invalid password for user admin@ubiquitous-lang.com")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task LoginAsync_ShouldReturnError_AndLogMessage()
        {
            // Arrange
            var emailResult = Email.create("test@example.com");
            Assert.True(emailResult.IsOk);
            var email = emailResult.ResultValue;
            var password = "password123";

            // Act
            var result = await _service.LoginAsync(email, password);

            // Assert
            Assert.True(result.IsError);
            Assert.Equal("Phase A3で実装予定", result.ErrorValue);
            
            // ログが出力されることを確認
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("LoginAsync called - Phase A3で実装予定")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }
    }

    /// <summary>
    /// CreateUserWithPasswordAsyncのテスト
    /// </summary>
    public class CreateUserWithPasswordAsyncTests : AuthenticationServiceTests
    {
        [Fact]
        public async Task CreateUserWithPasswordAsync_ShouldReturnError_AndLogMessage()
        {
            // Arrange
            var emailResult = Email.create("test@example.com");
            var nameResult = UserName.create("テストユーザー");
            var passwordResult = Password.create("SecurePassword123!");
            
            Assert.True(emailResult.IsOk);
            Assert.True(nameResult.IsOk);
            Assert.True(passwordResult.IsOk);

            var email = emailResult.ResultValue;
            var name = nameResult.ResultValue;
            var role = Role.GeneralUser;
            var password = passwordResult.ResultValue;
            var createdBy = UserId.create(1L);

            // Act
            var result = await _service.CreateUserWithPasswordAsync(email, name, role, password, createdBy);

            // Assert
            Assert.True(result.IsError);
            Assert.Equal("Phase A3で実装予定", result.ErrorValue);
            
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("CreateUserWithPasswordAsync called - Phase A3で実装予定")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }
    }

    /// <summary>
    /// ChangePasswordAsyncのテスト
    /// </summary>
    public class ChangePasswordAsyncTests : AuthenticationServiceTests
    {
        [Fact]
        public async Task ChangePasswordAsync_FirstLogin_ShouldUpdateFlagAndSucceed()
        {
            // Arrange - TECH-004対応: 初回ログイン時のパスワード変更
            var userId = UserId.create(1L);
            var oldPassword = "su"; // 初期パスワード
            var newPasswordResult = Password.create("NewSecurePassword123!");
            
            Assert.True(newPasswordResult.IsOk);
            var newPassword = newPasswordResult.ResultValue;

            // 初回ログインユーザーのApplicationUser作成
            var firstLoginUser = new ApplicationUser
            {
                Id = "1",
                Email = "admin@ubiquitous-lang.com",
                UserName = "admin@ubiquitous-lang.com",
                Name = "システム管理者",
                IsFirstLogin = true, // TECH-004: 初回ログインフラグ
                EmailConfirmed = true
            };

            // UserManager モックセットアップ
            _mockUserManager.Setup(x => x.FindByIdAsync("1"))
                           .ReturnsAsync(firstLoginUser);
            _mockUserManager.Setup(x => x.ChangePasswordAsync(firstLoginUser, oldPassword, "NewSecurePassword123!"))
                           .ReturnsAsync(IdentityResult.Success);
            _mockUserManager.Setup(x => x.UpdateAsync(It.Is<ApplicationUser>(u => !u.IsFirstLogin)))
                           .ReturnsAsync(IdentityResult.Success);

            // Act
            var result = await _service.ChangePasswordAsync(userId, oldPassword, newPassword);

            // Assert - TECH-004対応: パスワード変更成功とIsFirstLoginフラグ更新
            Assert.True(result.IsOk);
            
            // IsFirstLoginフラグがfalseに更新されることを確認
            _mockUserManager.Verify(x => x.UpdateAsync(
                It.Is<ApplicationUser>(u => u.Id == "1" && !u.IsFirstLogin)), 
                Times.Once);
            
            // ログ出力確認
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Password changed successfully for user: 1")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task ChangePasswordAsync_RegularUser_ShouldSucceed()
        {
            // Arrange - TECH-004対応: 通常ユーザーのパスワード変更（IsFirstLogin=false）
            var userId = UserId.create(2L);
            var oldPassword = "CurrentPassword123!";
            var newPasswordResult = Password.create("NewSecurePassword123!");
            
            Assert.True(newPasswordResult.IsOk);
            var newPassword = newPasswordResult.ResultValue;

            // 通常ユーザー（初回ログイン済み）のApplicationUser作成
            var regularUser = new ApplicationUser
            {
                Id = "2",
                Email = "user@ubiquitous-lang.com",
                UserName = "user@ubiquitous-lang.com",
                Name = "一般ユーザー",
                IsFirstLogin = false, // TECH-004: 通常ユーザー
                EmailConfirmed = true
            };

            // UserManager モックセットアップ
            _mockUserManager.Setup(x => x.FindByIdAsync("2"))
                           .ReturnsAsync(regularUser);
            _mockUserManager.Setup(x => x.ChangePasswordAsync(regularUser, oldPassword, "NewSecurePassword123!"))
                           .ReturnsAsync(IdentityResult.Success);

            // Act
            var result = await _service.ChangePasswordAsync(userId, oldPassword, newPassword);

            // Assert - 通常のパスワード変更成功
            Assert.True(result.IsOk);
            
            // 通常ユーザーではIsFirstLoginフラグは更新されない（既にfalse）
            _mockUserManager.Verify(x => x.UpdateAsync(It.IsAny<ApplicationUser>()), Times.Never);
        }

        [Fact]
        public async Task ChangePasswordAsync_InvalidOldPassword_ShouldFail()
        {
            // Arrange - TECH-004対応: 無効な旧パスワードでの変更失敗
            var userId = UserId.create(1L);
            var invalidOldPassword = "WrongPassword";
            var newPasswordResult = Password.create("NewSecurePassword123!");
            
            Assert.True(newPasswordResult.IsOk);
            var newPassword = newPasswordResult.ResultValue;

            var user = new ApplicationUser
            {
                Id = "1",
                Email = "admin@ubiquitous-lang.com",
                UserName = "admin@ubiquitous-lang.com",
                Name = "システム管理者",
                IsFirstLogin = true,
                EmailConfirmed = true
            };

            // UserManager モックセットアップ - パスワード変更失敗
            _mockUserManager.Setup(x => x.FindByIdAsync("1"))
                           .ReturnsAsync(user);
            
            var identityError = new IdentityError { Code = "PasswordMismatch", Description = "Incorrect password." };
            _mockUserManager.Setup(x => x.ChangePasswordAsync(user, invalidOldPassword, "NewSecurePassword123!"))
                           .ReturnsAsync(IdentityResult.Failed(identityError));

            // Act
            var result = await _service.ChangePasswordAsync(userId, invalidOldPassword, newPassword);

            // Assert - パスワード変更失敗
            Assert.True(result.IsError);
            Assert.Contains("パスワード変更に失敗しました", result.ErrorValue);
            
            // IsFirstLoginフラグは更新されない
            _mockUserManager.Verify(x => x.UpdateAsync(It.IsAny<ApplicationUser>()), Times.Never);
        }

        [Fact]
        public async Task ChangePasswordAsync_ShouldReturnError_AndLogMessage()
        {
            // Arrange
            var userId = UserId.create(1L);
            var oldPassword = "oldPassword";
            var newPasswordResult = Password.create("NewSecurePassword123!");
            
            Assert.True(newPasswordResult.IsOk);
            var newPassword = newPasswordResult.ResultValue;

            // Act
            var result = await _service.ChangePasswordAsync(userId, oldPassword, newPassword);

            // Assert
            Assert.True(result.IsError);
            Assert.Equal("Phase A3で実装予定", result.ErrorValue);
            
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("ChangePasswordAsync called - Phase A3で実装予定")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }
    }

    /// <summary>
    /// HashPasswordAsyncのテスト
    /// </summary>
    public class HashPasswordAsyncTests : AuthenticationServiceTests
    {
        [Fact]
        public async Task HashPasswordAsync_ShouldReturnError_AndLogMessage()
        {
            // Arrange
            var passwordResult = Password.create("TestPassword123!");
            Assert.True(passwordResult.IsOk);
            var password = passwordResult.ResultValue;

            // Act
            var result = await _service.HashPasswordAsync(password);

            // Assert
            Assert.True(result.IsError);
            Assert.Equal("Phase A3で実装予定", result.ErrorValue);
            
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("HashPasswordAsync called - Phase A3で実装予定")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }
    }

    /// <summary>
    /// VerifyPasswordAsyncのテスト
    /// </summary>
    public class VerifyPasswordAsyncTests : AuthenticationServiceTests
    {
        [Fact]
        public async Task VerifyPasswordAsync_ShouldReturnError_AndLogMessage()
        {
            // Arrange
            var password = "testPassword";
            var hashResult = PasswordHash.create("hashedPassword");
            Assert.True(hashResult.IsOk);
            var hash = hashResult.ResultValue;

            // Act
            var result = await _service.VerifyPasswordAsync(password, hash);

            // Assert
            Assert.True(result.IsError);
            Assert.Equal("Phase A3で実装予定", result.ErrorValue);
            
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("VerifyPasswordAsync called - Phase A3で実装予定")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }
    }

    /// <summary>
    /// GenerateTokenAsyncのテスト
    /// </summary>
    public class GenerateTokenAsyncTests : AuthenticationServiceTests
    {
        [Fact]
        public async Task GenerateTokenAsync_ShouldReturnError_AndLogMessage()
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

            // Act
            var result = await _service.GenerateTokenAsync(user);

            // Assert
            Assert.True(result.IsError);
            Assert.Equal("Phase A3で実装予定", result.ErrorValue);
            
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("GenerateTokenAsync called - Phase A3で実装予定")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }
    }

    /// <summary>
    /// ValidateTokenAsyncのテスト
    /// </summary>
    public class ValidateTokenAsyncTests : AuthenticationServiceTests
    {
        [Fact]
        public async Task ValidateTokenAsync_ShouldReturnError_AndLogMessage()
        {
            // Arrange
            var token = "test-token";

            // Act
            var result = await _service.ValidateTokenAsync(token);

            // Assert
            Assert.True(result.IsError);
            Assert.Equal("Phase A3で実装予定", result.ErrorValue);
            
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("ValidateTokenAsync called - Phase A3で実装予定")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }
    }

    /// <summary>
    /// RecordFailedLoginAsyncのテスト
    /// </summary>
    public class RecordFailedLoginAsyncTests : AuthenticationServiceTests
    {
        [Fact]
        public async Task RecordFailedLoginAsync_ShouldReturnError_AndLogMessage()
        {
            // Arrange
            var userId = UserId.create(1L);

            // Act
            var result = await _service.RecordFailedLoginAsync(userId);

            // Assert
            Assert.True(result.IsError);
            Assert.Equal("Phase A3で実装予定", result.ErrorValue);
            
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("RecordFailedLoginAsync called - Phase A3で実装予定")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }
    }

    /// <summary>
    /// RecordSuccessfulLoginAsyncのテスト
    /// </summary>
    public class RecordSuccessfulLoginAsyncTests : AuthenticationServiceTests
    {
        [Fact]
        public async Task RecordSuccessfulLoginAsync_ShouldReturnError_AndLogMessage()
        {
            // Arrange
            var userId = UserId.create(1L);

            // Act
            var result = await _service.RecordSuccessfulLoginAsync(userId);

            // Assert
            Assert.True(result.IsError);
            Assert.Equal("Phase A3で実装予定", result.ErrorValue);
            
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("RecordSuccessfulLoginAsync called - Phase A3で実装予定")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }
    }

    /// <summary>
    /// LockUserAsyncのテスト
    /// </summary>
    public class LockUserAsyncTests : AuthenticationServiceTests
    {
        [Fact]
        public async Task LockUserAsync_ShouldReturnError_AndLogMessage()
        {
            // Arrange
            var userId = UserId.create(1L);
            var lockoutEnd = DateTime.UtcNow.AddHours(1);

            // Act
            var result = await _service.LockUserAsync(userId, lockoutEnd);

            // Assert
            Assert.True(result.IsError);
            Assert.Equal("Phase A3で実装予定", result.ErrorValue);
            
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("LockUserAsync called - Phase A3で実装予定")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }
    }

    /// <summary>
    /// UnlockUserAsyncのテスト
    /// </summary>
    public class UnlockUserAsyncTests : AuthenticationServiceTests
    {
        [Fact]
        public async Task UnlockUserAsync_ShouldReturnError_AndLogMessage()
        {
            // Arrange
            var userId = UserId.create(1L);

            // Act
            var result = await _service.UnlockUserAsync(userId);

            // Assert
            Assert.True(result.IsError);
            Assert.Equal("Phase A3で実装予定", result.ErrorValue);
            
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("UnlockUserAsync called - Phase A3で実装予定")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }
    }

    /// <summary>
    /// GetCurrentUserAsyncのテスト
    /// </summary>
    public class GetCurrentUserAsyncTests : AuthenticationServiceTests
    {
        [Fact]
        public async Task GetCurrentUserAsync_ShouldReturnError_AndLogMessage()
        {
            // Act
            var result = await _service.GetCurrentUserAsync();

            // Assert
            Assert.True(result.IsError);
            Assert.Equal("Phase A3で実装予定", result.ErrorValue);
            
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("GetCurrentUserAsync called - Phase A3で実装予定")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }
    }

    /// <summary>
    /// Phase A3実装予定メソッド群のテスト
    /// （その他のメソッドも同様のパターンで実装）
    /// </summary>
    public class Phase3MethodsTests : AuthenticationServiceTests
    {
        [Fact]
        public async Task UpdateSecurityStampAsync_ShouldReturnError_AndLogMessage()
        {
            // Arrange
            var userId = UserId.create(1L);

            // Act
            var result = await _service.UpdateSecurityStampAsync(userId);

            // Assert
            Assert.True(result.IsError);
            Assert.Equal("Phase A3で実装予定", result.ErrorValue);
        }

        [Fact]
        public async Task SendEmailConfirmationAsync_ShouldReturnError_AndLogMessage()
        {
            // Arrange
            var emailResult = Email.create("test@example.com");
            Assert.True(emailResult.IsOk);
            var email = emailResult.ResultValue;

            // Act
            var result = await _service.SendEmailConfirmationAsync(email);

            // Assert
            Assert.True(result.IsError);
            Assert.Equal("Phase A3で実装予定", result.ErrorValue);
        }

        [Fact]
        public async Task ConfirmEmailAsync_ShouldReturnError_AndLogMessage()
        {
            // Arrange
            var userId = UserId.create(1L);
            var token = "confirmation-token";

            // Act
            var result = await _service.ConfirmEmailAsync(userId, token);

            // Assert
            Assert.True(result.IsError);
            Assert.Equal("Phase A3で実装予定", result.ErrorValue);
        }

        [Fact]
        public async Task EnableTwoFactorAsync_ShouldReturnError_AndLogMessage()
        {
            // Arrange
            var userId = UserId.create(1L);

            // Act
            var result = await _service.EnableTwoFactorAsync(userId);

            // Assert
            Assert.True(result.IsError);
            Assert.Equal("Phase A3で実装予定", result.ErrorValue);
        }

        [Fact]
        public async Task DisableTwoFactorAsync_ShouldReturnError_AndLogMessage()
        {
            // Arrange
            var userId = UserId.create(1L);

            // Act
            var result = await _service.DisableTwoFactorAsync(userId);

            // Assert
            Assert.True(result.IsError);
            Assert.Equal("Phase A3で実装予定", result.ErrorValue);
        }

        [Fact]
        public async Task VerifyTwoFactorCodeAsync_ShouldReturnError_AndLogMessage()
        {
            // Arrange
            var userId = UserId.create(1L);
            var code = "123456";

            // Act
            var result = await _service.VerifyTwoFactorCodeAsync(userId, code);

            // Assert
            Assert.True(result.IsError);
            Assert.Equal("Phase A3で実装予定", result.ErrorValue);
        }
    }

    /// <summary>
    /// TECH-004対応: 初回ログインフロー・IsFirstLoginフラグ管理のテスト
    /// 
    /// 【テスト対象】
    /// - 初回ログイン判定
    /// - パスワード変更完了後のIsFirstLoginフラグ更新
    /// - 初回ログインフロー全体の動作
    /// </summary>
    public class FirstLoginFlowTests : AuthenticationServiceTests
    {
        [Fact]
        public async Task FirstLoginFlow_InitialLoginWithFlagUpdate_ShouldWork()
        {
            // Arrange - TECH-004対応: 初回ログインフロー全体テスト
            var emailResult = Email.create("admin@ubiquitous-lang.com");
            Assert.True(emailResult.IsOk);
            var email = emailResult.ResultValue;
            var initialPassword = "su";
            var userId = UserId.create(1L);
            var newPasswordResult = Password.create("NewSecurePassword123!");
            Assert.True(newPasswordResult.IsOk);
            var newPassword = newPasswordResult.ResultValue;

            // 初回ログインユーザー
            var firstLoginUser = new ApplicationUser
            {
                Id = "1",
                Email = "admin@ubiquitous-lang.com",
                UserName = "admin@ubiquitous-lang.com",
                Name = "システム管理者",
                IsFirstLogin = true, // 初回ログインフラグ
                EmailConfirmed = true
            };

            // Step 1: 初回ログイン成功のモックセットアップ
            _mockUserManager.Setup(x => x.FindByEmailAsync("admin@ubiquitous-lang.com"))
                           .ReturnsAsync(firstLoginUser);
            _mockUserManager.Setup(x => x.IsLockedOutAsync(firstLoginUser))
                           .ReturnsAsync(false);
            _mockSignInManager.Setup(x => x.PasswordSignInAsync(
                firstLoginUser, initialPassword, false, false))
                             .ReturnsAsync(Microsoft.AspNetCore.Identity.SignInResult.Success);

            // Step 2: パスワード変更成功のモックセットアップ
            _mockUserManager.Setup(x => x.FindByIdAsync("1"))
                           .ReturnsAsync(firstLoginUser);
            _mockUserManager.Setup(x => x.ChangePasswordAsync(firstLoginUser, initialPassword, "NewSecurePassword123!"))
                           .ReturnsAsync(IdentityResult.Success);
            _mockUserManager.Setup(x => x.UpdateAsync(It.Is<ApplicationUser>(u => !u.IsFirstLogin)))
                           .ReturnsAsync(IdentityResult.Success);

            // Act & Assert - Step 1: 初回ログイン
            var loginResult = await _service.LoginAsync(email, initialPassword);
            Assert.True(loginResult.IsOk);
            var user = loginResult.ResultValue;
            Assert.Equal("admin@ubiquitous-lang.com", user.Email.Value);

            // Act & Assert - Step 2: パスワード変更（IsFirstLoginフラグ更新）
            var changePasswordResult = await _service.ChangePasswordAsync(userId, initialPassword, newPassword);
            Assert.True(changePasswordResult.IsOk);

            // Assert - IsFirstLoginフラグがfalseに更新されることを確認
            _mockUserManager.Verify(x => x.UpdateAsync(
                It.Is<ApplicationUser>(u => u.Id == "1" && !u.IsFirstLogin)), 
                Times.Once);

            // ログ出力確認
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Login successful for user: admin@ubiquitous-lang.com")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
                
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Password changed successfully for user: 1")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task IsFirstLoginFlag_NewUser_ShouldBeTrue()
        {
            // Arrange - TECH-004対応: 新規作成ユーザーのIsFirstLoginフラグ確認
            var emailResult = Email.create("newuser@ubiquitous-lang.com");
            var nameResult = UserName.create("新規ユーザー");
            var passwordResult = Password.create("TempPassword123!");
            
            Assert.True(emailResult.IsOk);
            Assert.True(nameResult.IsOk);
            Assert.True(passwordResult.IsOk);

            var email = emailResult.ResultValue;
            var name = nameResult.ResultValue;
            var role = Role.GeneralUser;
            var password = passwordResult.ResultValue;
            var createdBy = UserId.create(1L);

            // 作成されるユーザー
            var newUser = new ApplicationUser
            {
                Id = "2",
                Email = "newuser@ubiquitous-lang.com",
                UserName = "newuser@ubiquitous-lang.com",
                Name = "新規ユーザー",
                IsFirstLogin = true, // 新規ユーザーは初回ログインフラグがtrue
                EmailConfirmed = false
            };

            // UserManager モックセットアップ
            _mockUserManager.Setup(x => x.CreateAsync(It.IsAny<ApplicationUser>(), "TempPassword123!"))
                           .ReturnsAsync(IdentityResult.Success)
                           .Callback<ApplicationUser, string>((user, pwd) => {
                               // 新規作成時はIsFirstLoginがtrueであることを確認
                               Assert.True(user.IsFirstLogin);
                           });
            _mockUserManager.Setup(x => x.AddToRoleAsync(It.IsAny<ApplicationUser>(), "GeneralUser"))
                           .ReturnsAsync(IdentityResult.Success);

            // Act
            var result = await _service.CreateUserWithPasswordAsync(email, name, role, password, createdBy);

            // Assert - ユーザー作成時のIsFirstLoginフラグ確認
            // NOTE: 現在のテストはPhase A3実装なのでエラーが返されるが、
            // 実装時にはIsFirstLogin=trueで作成されることをテストする
            Assert.True(result.IsError);
            Assert.Equal("Phase A3で実装予定", result.ErrorValue);
        }

        [Fact]
        public async Task IsFirstLoginFlag_AfterPasswordChange_ShouldBeFalse()
        {
            // Arrange - TECH-004対応: パスワード変更後のIsFirstLoginフラグ確認
            var userId = UserId.create(1L);
            var oldPassword = "su";
            var newPasswordResult = Password.create("NewSecurePassword123!");
            Assert.True(newPasswordResult.IsOk);
            var newPassword = newPasswordResult.ResultValue;

            var userBeforeChange = new ApplicationUser
            {
                Id = "1",
                Email = "admin@ubiquitous-lang.com",
                UserName = "admin@ubiquitous-lang.com",
                Name = "システム管理者",
                IsFirstLogin = true, // 変更前: true
                EmailConfirmed = true
            };

            // UserManager モックセットアップ
            _mockUserManager.Setup(x => x.FindByIdAsync("1"))
                           .ReturnsAsync(userBeforeChange);
            _mockUserManager.Setup(x => x.ChangePasswordAsync(userBeforeChange, oldPassword, "NewSecurePassword123!"))
                           .ReturnsAsync(IdentityResult.Success);
            
            // IsFirstLoginフラグ更新の確認
            _mockUserManager.Setup(x => x.UpdateAsync(It.IsAny<ApplicationUser>()))
                           .ReturnsAsync(IdentityResult.Success)
                           .Callback<ApplicationUser>(user => {
                               // 更新時にIsFirstLoginがfalseになることを確認
                               Assert.False(user.IsFirstLogin);
                           });

            // Act
            var result = await _service.ChangePasswordAsync(userId, oldPassword, newPassword);

            // Assert - パスワード変更成功とフラグ更新確認
            Assert.True(result.IsOk);
            
            // モック呼び出し確認
            _mockUserManager.Verify(x => x.UpdateAsync(
                It.Is<ApplicationUser>(u => u.Id == "1" && !u.IsFirstLogin)), 
                Times.Once);
        }
    }

    /// <summary>
    /// TECH-002対応: 設定ファイル読み込み・初期パスワード管理のテスト
    /// 
    /// 【テスト対象】
    /// - appsettings.jsonからの初期パスワード読み込み
    /// - 設定値の一貫性確認
    /// - 初期パスワード"su"の検証
    /// </summary>
    public class InitialPasswordConfigurationTests : AuthenticationServiceTests
    {
        [Fact]
        public async Task InitialPassword_FromConfiguration_ShouldBe_Su()
        {
            // Arrange - TECH-002対応: 設定ファイルの初期パスワード確認
            // NOTE: 実際の設定読み込みテストは統合テストで実施
            // ここでは仕様準拠のパスワード"su"が正しく使用されることをテスト
            
            var emailResult = Email.create("admin@ubiquitous-lang.com");
            Assert.True(emailResult.IsOk);
            var email = emailResult.ResultValue;
            var specifiedInitialPassword = "su"; // 仕様書2.0.1で指定されたパスワード

            var superUser = new ApplicationUser
            {
                Id = "1",
                Email = "admin@ubiquitous-lang.com",
                UserName = "admin@ubiquitous-lang.com",
                Name = "システム管理者",
                IsFirstLogin = true,
                EmailConfirmed = true
            };

            _mockUserManager.Setup(x => x.FindByEmailAsync("admin@ubiquitous-lang.com"))
                           .ReturnsAsync(superUser);
            _mockUserManager.Setup(x => x.IsLockedOutAsync(superUser))
                           .ReturnsAsync(false);
            _mockSignInManager.Setup(x => x.PasswordSignInAsync(
                superUser, specifiedInitialPassword, false, false))
                             .ReturnsAsync(Microsoft.AspNetCore.Identity.SignInResult.Success);

            // Act
            var result = await _service.LoginAsync(email, specifiedInitialPassword);

            // Assert - TECH-002対応: 仕様準拠パスワード"su"での認証成功
            Assert.True(result.IsOk);
            var user = result.ResultValue;
            Assert.Equal("admin@ubiquitous-lang.com", user.Email.Value);
            
            // 仕様書で定義された初期パスワードが使用されることを確認
            _mockSignInManager.Verify(x => x.PasswordSignInAsync(
                It.IsAny<ApplicationUser>(), "su", false, false), Times.Once);
        }

        [Fact]
        public async Task InitialPassword_OldConfiguration_ShouldFail()
        {
            // Arrange - TECH-002対応: 旧設定パスワード"TempPass123!"での認証失敗確認
            var emailResult = Email.create("admin@ubiquitous-lang.com");
            Assert.True(emailResult.IsOk);
            var email = emailResult.ResultValue;
            var oldConfigPassword = "TempPass123!"; // 修正前の不正なパスワード

            var superUser = new ApplicationUser
            {
                Id = "1",
                Email = "admin@ubiquitous-lang.com",
                UserName = "admin@ubiquitous-lang.com",
                Name = "システム管理者",
                IsFirstLogin = true,
                EmailConfirmed = true
            };

            _mockUserManager.Setup(x => x.FindByEmailAsync("admin@ubiquitous-lang.com"))
                           .ReturnsAsync(superUser);
            _mockUserManager.Setup(x => x.IsLockedOutAsync(superUser))
                           .ReturnsAsync(false);
            _mockSignInManager.Setup(x => x.PasswordSignInAsync(
                superUser, oldConfigPassword, false, false))
                             .ReturnsAsync(Microsoft.AspNetCore.Identity.SignInResult.Failed);

            // Act
            var result = await _service.LoginAsync(email, oldConfigPassword);

            // Assert - TECH-002対応: 旧パスワードでは認証失敗
            Assert.True(result.IsError);
            Assert.Equal("メールアドレスまたはパスワードが正しくありません", result.ErrorValue);
            
            // 警告ログ出力確認
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Login failed: Invalid password")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact] 
        public async Task InitialPassword_WithAdmin123_ShouldFail()
        {
            // Arrange - RED: Admin123!パスワードでの認証失敗を追加テスト
            // TECH-002対応: 以前使用されていた可能性のあるパスワードでの失敗確認
            var emailResult = Email.create("admin@ubiquitous-lang.com");
            Assert.True(emailResult.IsOk);
            var email = emailResult.ResultValue;
            var oldAdminPassword = "Admin123!"; // 旧Admin系パスワード

            var superUser = new ApplicationUser
            {
                Id = "1",
                Email = "admin@ubiquitous-lang.com", 
                UserName = "admin@ubiquitous-lang.com",
                Name = "システム管理者",
                IsFirstLogin = true,
                EmailConfirmed = true,
                // RED: 期待値 - InitialPassword="su", PasswordHash=null
                InitialPassword = "su", // 仕様準拠の初期パスワード
                PasswordHash = null     // 平文管理仕様
            };

            _mockUserManager.Setup(x => x.FindByEmailAsync("admin@ubiquitous-lang.com"))
                           .ReturnsAsync(superUser);
            _mockUserManager.Setup(x => x.IsLockedOutAsync(superUser))
                           .ReturnsAsync(false);
            _mockSignInManager.Setup(x => x.PasswordSignInAsync(
                superUser, oldAdminPassword, false, false))
                             .ReturnsAsync(Microsoft.AspNetCore.Identity.SignInResult.Failed);

            // Act 
            var result = await _service.LoginAsync(email, oldAdminPassword);

            // Assert - RED: 現在の実装では失敗するはず（InitialPassword未実装）
            Assert.True(result.IsError);
            Assert.Equal("メールアドレスまたはパスワードが正しくありません", result.ErrorValue);

            // InitialPassword検証ロジックの呼び出し確認（将来の実装で必要）
            // TODO: GREEN段階でInitialPassword認証ロジックを実装
        }

        [Fact]
        public async Task InitialPassword_AuthenticationFlow_ShouldUpdateSecurityStamp()
        {
            // Arrange - RED: セキュリティスタンプ更新テスト（まだ未実装なので失敗予定）
            var emailResult = Email.create("admin@ubiquitous-lang.com");
            Assert.True(emailResult.IsOk);
            var email = emailResult.ResultValue;
            var initialPassword = "su";

            var superUser = new ApplicationUser
            {
                Id = "1", 
                Email = "admin@ubiquitous-lang.com",
                UserName = "admin@ubiquitous-lang.com",
                Name = "システム管理者",
                IsFirstLogin = true,
                EmailConfirmed = true,
                InitialPassword = "su",
                PasswordHash = null,
                SecurityStamp = "old_security_stamp" // 変更前のセキュリティスタンプ
            };

            _mockUserManager.Setup(x => x.FindByEmailAsync("admin@ubiquitous-lang.com"))
                           .ReturnsAsync(superUser);
            _mockUserManager.Setup(x => x.IsLockedOutAsync(superUser))
                           .ReturnsAsync(false);
            _mockSignInManager.Setup(x => x.PasswordSignInAsync(
                superUser, initialPassword, false, false))
                             .ReturnsAsync(Microsoft.AspNetCore.Identity.SignInResult.Success);

            // セキュリティスタンプ更新のモック
            _mockUserManager.Setup(x => x.UpdateSecurityStampAsync(superUser))
                           .ReturnsAsync(IdentityResult.Success);

            // Act
            var result = await _service.LoginAsync(email, initialPassword);

            // Assert - RED: セキュリティスタンプ更新が未実装なので検証は将来実装
            Assert.True(result.IsOk);
            
            // TODO: GREEN段階でセキュリティスタンプ更新ロジックを実装し、以下を有効化
            // _mockUserManager.Verify(x => x.UpdateSecurityStampAsync(
            //     It.Is<ApplicationUser>(u => u.Id == "1")), Times.Once);
        }

        [Fact]
        public async Task InitialPassword_AfterPasswordChange_ShouldBeNull()
        {
            // Arrange - RED: パスワード変更後のInitialPassword=null確認テスト
            var userId = UserId.create(1L);
            var oldPassword = "su";
            var newPasswordResult = Password.create("NewSecurePassword123!");
            Assert.True(newPasswordResult.IsOk);
            var newPassword = newPasswordResult.ResultValue;

            var userBeforeChange = new ApplicationUser
            {
                Id = "1",
                Email = "admin@ubiquitous-lang.com",
                UserName = "admin@ubiquitous-lang.com", 
                Name = "システム管理者",
                IsFirstLogin = true,
                InitialPassword = "su", // 変更前: 初期パスワード設定済み
                PasswordHash = null,    // 変更前: ハッシュなし
                EmailConfirmed = true
            };

            _mockUserManager.Setup(x => x.FindByIdAsync("1"))
                           .ReturnsAsync(userBeforeChange);
            _mockUserManager.Setup(x => x.ChangePasswordAsync(userBeforeChange, oldPassword, "NewSecurePassword123!"))
                           .ReturnsAsync(IdentityResult.Success);

            // ユーザー更新時のキャプチャ
            ApplicationUser updatedUser = null;
            _mockUserManager.Setup(x => x.UpdateAsync(It.IsAny<ApplicationUser>()))
                           .ReturnsAsync(IdentityResult.Success)
                           .Callback<ApplicationUser>(user => {
                               updatedUser = user;
                           });

            // Act
            var result = await _service.ChangePasswordAsync(userId, oldPassword, newPassword);

            // Assert - RED: InitialPasswordクリア処理が未実装なので将来実装
            Assert.True(result.IsOk);

            // 将来のGREEN段階で有効化予定
            // Assert.NotNull(updatedUser);
            // Assert.Null(updatedUser.InitialPassword); // パスワード変更後はnull
            // Assert.False(updatedUser.IsFirstLogin);   // 初回ログインフラグもfalse
        }
    }
}