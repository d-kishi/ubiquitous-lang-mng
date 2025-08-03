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
}