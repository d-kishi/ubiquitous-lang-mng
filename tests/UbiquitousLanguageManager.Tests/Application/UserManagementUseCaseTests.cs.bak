using Microsoft.FSharp.Core;
using Microsoft.FSharp.Collections;
using Moq;
using System.Linq;
using UbiquitousLanguageManager.Application;
using UbiquitousLanguageManager.Domain;
using Xunit;

namespace UbiquitousLanguageManager.Tests.Application;

/// <summary>
/// UserManagementUseCaseの単体テスト
/// 
/// 【テスト方針】
/// Phase A2で新規追加されたUserManagementUseCaseのビジネスフロー、
/// 入力検証、UseCaseResult型の動作、F#のコマンド型との連携を検証します。
/// </summary>
public class UserManagementUseCaseTests
{
    private readonly Mock<UserApplicationService> _mockUserAppService;
    private readonly UserManagementUseCase _useCase;

    public UserManagementUseCaseTests()
    {
        _mockUserAppService = new Mock<UserApplicationService>(
            Mock.Of<IUserRepository>(),
            Mock.Of<IAuthenticationService>(),
            Mock.Of<INotificationService>(),
            Mock.Of<ILogger<UserApplicationService>>());
            
        _useCase = new UserManagementUseCase(_mockUserAppService.Object);
    }

    private User CreateTestUser(string email = "test@example.com", string name = "テストユーザー", Role? role = null, long id = 1L)
    {
        var emailResult = Email.create(email);
        var nameResult = UserName.create(name);
        
        Assert.True(emailResult.IsOk);
        Assert.True(nameResult.IsOk);

        return User.create(
            emailResult.ResultValue,
            nameResult.ResultValue,
            role ?? Role.GeneralUser,
            UserId.create(id)
        );
    }

    /// <summary>
    /// RegisterUserAsyncのテスト
    /// </summary>
    public class RegisterUserAsyncTests : UserManagementUseCaseTests
    {
        [Fact]
        public async Task RegisterUserAsync_ValidInput_ShouldReturnSuccess()
        {
            // Arrange
            var command = new UbiquitousLanguageManager.Application.RegisterUserCommand(
                "newuser@example.com",
                "新規ユーザー",
                "generaluser",
                1L
            );

            var expectedUser = CreateTestUser("newuser@example.com", "新規ユーザー", Role.GeneralUser, 2L);

            _mockUserAppService
                .Setup(x => x.CreateUserAsync(
                    It.IsAny<Email>(),
                    It.IsAny<UserName>(),
                    It.IsAny<Role>(),
                    It.IsAny<User>()))
                .ReturnsAsync(FSharpResult<User, string>.NewOk(expectedUser));

            // Act
            var result = await _useCase.RegisterUserAsync(command);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Data);
            Assert.Null(result.ErrorMessage);
            Assert.Empty(result.ValidationErrors);

            // ApplicationServiceが正しく呼ばれることを確認
            _mockUserAppService.Verify(
                x => x.CreateUserAsync(
                    It.Is<Email>(e => e.Value == "newuser@example.com"),
                    It.Is<UserName>(n => n.Value == "新規ユーザー"),
                    It.Is<Role>(r => r.IsGeneralUser),
                    It.IsAny<User>()),
                Times.Once);
        }

        [Theory]
        [InlineData("superuser", true)]
        [InlineData("SuperUser", true)]
        [InlineData("SUPERUSER", true)]
        [InlineData("projectmanager", true)]
        [InlineData("ProjectManager", true)]
        [InlineData("domainapprover", true)]
        [InlineData("DomainApprover", true)]
        [InlineData("generaluser", true)]
        [InlineData("GeneralUser", true)]
        public async Task RegisterUserAsync_ValidRoles_ShouldReturnSuccess(string roleString, bool expectedValid)
        {
            // Arrange
            var command = new UbiquitousLanguageManager.Application.RegisterUserCommand(
                "user@example.com",
                "ユーザー",
                roleString,
                1L
            );

            var expectedUser = CreateTestUser("user@example.com", "ユーザー", Role.GeneralUser);

            _mockUserAppService
                .Setup(x => x.CreateUserAsync(It.IsAny<Email>(), It.IsAny<UserName>(), It.IsAny<Role>(), It.IsAny<User>()))
                .ReturnsAsync(FSharpResult<User, string>.NewOk(expectedUser));

            // Act
            var result = await _useCase.RegisterUserAsync(command);

            // Assert
            if (expectedValid)
            {
                Assert.True(result.IsSuccess);
                _mockUserAppService.Verify(
                    x => x.CreateUserAsync(It.IsAny<Email>(), It.IsAny<UserName>(), It.IsAny<Role>(), It.IsAny<User>()),
                    Times.Once);
            }
            else
            {
                Assert.False(result.IsSuccess);
                _mockUserAppService.Verify(
                    x => x.CreateUserAsync(It.IsAny<Email>(), It.IsAny<UserName>(), It.IsAny<Role>(), It.IsAny<User>()),
                    Times.Never);
            }
        }

        [Theory]
        [InlineData("admin", false)]
        [InlineData("user", false)]
        [InlineData("invalid", false)]
        [InlineData("", false)]
        public async Task RegisterUserAsync_InvalidRoles_ShouldReturnValidationError(string roleString, bool expectedValid)
        {
            // Arrange
            var command = new UbiquitousLanguageManager.Application.RegisterUserCommand(
                "user@example.com",
                "ユーザー",
                roleString,
                1L
            );

            // Act
            var result = await _useCase.RegisterUserAsync(command);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.NotEmpty(result.ValidationErrors);
            Assert.Contains(result.ValidationErrors, e => e.Item1 == "Role");
            Assert.Contains("無効なユーザーロール", result.ValidationErrors.First(e => e.Item1 == "Role").Item2);

            // ApplicationServiceは呼ばれない
            _mockUserAppService.Verify(
                x => x.CreateUserAsync(It.IsAny<Email>(), It.IsAny<UserName>(), It.IsAny<Role>(), It.IsAny<User>()),
                Times.Never);
        }

        [Theory]
        [InlineData("invalid-email", true, "generaluser", "Email")]
        [InlineData("user@example.com", false, "generaluser", "Name")] // 空の名前
        [InlineData("", true, "generaluser", "Email")] // 空のメール
        public async Task RegisterUserAsync_InvalidInput_ShouldReturnValidationErrors(
            string email, bool validName, string role, string expectedErrorField)
        {
            // Arrange
            var command = new UbiquitousLanguageManager.Application.RegisterUserCommand(
                email,
                validName ? "有効なユーザー名" : "", // 空文字は無効
                role,
                1L
            );

            // Act
            var result = await _useCase.RegisterUserAsync(command);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.NotEmpty(result.ValidationErrors);
            Assert.Contains(result.ValidationErrors, e => e.Item1 == expectedErrorField);

            // ApplicationServiceは呼ばれない
            _mockUserAppService.Verify(
                x => x.CreateUserAsync(It.IsAny<Email>(), It.IsAny<UserName>(), It.IsAny<Role>(), It.IsAny<User>()),
                Times.Never);
        }

        [Fact]
        public async Task RegisterUserAsync_ApplicationServiceError_ShouldReturnError()
        {
            // Arrange
            var command = new UbiquitousLanguageManager.Application.RegisterUserCommand(
                "user@example.com",
                "ユーザー",
                "generaluser",
                1L
            );

            _mockUserAppService
                .Setup(x => x.CreateUserAsync(It.IsAny<Email>(), It.IsAny<UserName>(), It.IsAny<Role>(), It.IsAny<User>()))
                .ReturnsAsync(FSharpResult<User, string>.NewError("メールアドレスが既に存在します"));

            // Act
            var result = await _useCase.RegisterUserAsync(command);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal("メールアドレスが既に存在します", result.ErrorMessage);
            Assert.Null(result.Data);
            Assert.Empty(result.ValidationErrors);
        }
    }

    /// <summary>
    /// LoginAsyncのテスト
    /// </summary>
    public class LoginAsyncTests : UserManagementUseCaseTests
    {
        [Fact]
        public async Task LoginAsync_ValidCredentials_ShouldReturnSuccess()
        {
            // Arrange
            var command = new UbiquitousLanguageManager.Application.LoginCommand(
                "user@example.com",
                "password123"
            );

            var expectedUser = CreateTestUser("user@example.com", "ユーザー", Role.GeneralUser);

            _mockUserAppService
                .Setup(x => x.LoginAsync(It.IsAny<Email>(), command.Password))
                .ReturnsAsync(FSharpResult<User, string>.NewOk(expectedUser));

            // Act
            var result = await _useCase.LoginAsync(command);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Data);
            Assert.Null(result.ErrorMessage);
            Assert.Empty(result.ValidationErrors);

            // ApplicationServiceが正しく呼ばれることを確認
            _mockUserAppService.Verify(
                x => x.LoginAsync(
                    It.Is<Email>(e => e.Value == "user@example.com"),
                    command.Password),
                Times.Once);
        }

        [Theory]
        [InlineData("invalid-email")]
        [InlineData("")]
        [InlineData("user@")]
        [InlineData("@example.com")]
        public async Task LoginAsync_InvalidEmail_ShouldReturnValidationError(string invalidEmail)
        {
            // Arrange
            var command = new UbiquitousLanguageManager.Application.LoginCommand(
                invalidEmail,
                "password123"
            );

            // Act
            var result = await _useCase.LoginAsync(command);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.NotEmpty(result.ValidationErrors);
            Assert.Contains(result.ValidationErrors, e => e.Item1 == "Email");

            // ApplicationServiceは呼ばれない
            _mockUserAppService.Verify(
                x => x.LoginAsync(It.IsAny<Email>(), It.IsAny<string>()),
                Times.Never);
        }

        [Fact]
        public async Task LoginAsync_AuthenticationFailure_ShouldReturnError()
        {
            // Arrange
            var command = new UbiquitousLanguageManager.Application.LoginCommand(
                "user@example.com",
                "wrongpassword"
            );

            _mockUserAppService
                .Setup(x => x.LoginAsync(It.IsAny<Email>(), command.Password))
                .ReturnsAsync(FSharpResult<User, string>.NewError("認証に失敗しました"));

            // Act
            var result = await _useCase.LoginAsync(command);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal("認証に失敗しました", result.ErrorMessage);
            Assert.Null(result.Data);
            Assert.Empty(result.ValidationErrors);
        }
    }

    /// <summary>
    /// ChangePasswordAsyncのテスト
    /// </summary>
    public class ChangePasswordAsyncTests : UserManagementUseCaseTests
    {
        [Fact]
        public async Task ChangePasswordAsync_ValidInput_ShouldReturnSuccess()
        {
            // Arrange
            var command = new UbiquitousLanguageManager.Application.ChangePasswordCommand(
                1L,
                "oldPassword",
                "NewPassword123!",
                "NewPassword123!"
            );

            var expectedUser = CreateTestUser("user@example.com", "ユーザー", Role.GeneralUser);

            _mockUserAppService
                .Setup(x => x.ChangePasswordAsync(
                    It.IsAny<UserId>(),
                    command.OldPassword,
                    It.IsAny<Password>(),
                    It.IsAny<User>()))
                .ReturnsAsync(FSharpResult<User, string>.NewOk(expectedUser));

            // Act
            var result = await _useCase.ChangePasswordAsync(command);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Data);
            Assert.Null(result.ErrorMessage);
            Assert.Empty(result.ValidationErrors);

            // ApplicationServiceが正しく呼ばれることを確認
            _mockUserAppService.Verify(
                x => x.ChangePasswordAsync(
                    It.Is<UserId>(u => u.Value == 1L),
                    command.OldPassword,
                    It.IsAny<Password>(),
                    It.IsAny<User>()),
                Times.Once);
        }

        [Fact]
        public async Task ChangePasswordAsync_PasswordMismatch_ShouldReturnValidationError()
        {
            // Arrange
            var command = new UbiquitousLanguageManager.Application.ChangePasswordCommand(
                1L,
                "oldPassword",
                "NewPassword123!",
                "DifferentPassword123!" // 確認パスワードが異なる
            );

            // Act
            var result = await _useCase.ChangePasswordAsync(command);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.NotEmpty(result.ValidationErrors);
            Assert.Contains(result.ValidationErrors, e => e.Item1 == "ConfirmPassword");
            Assert.Contains("一致しません", result.ValidationErrors.First(e => e.Item1 == "ConfirmPassword").Item2);

            // ApplicationServiceは呼ばれない
            _mockUserAppService.Verify(
                x => x.ChangePasswordAsync(It.IsAny<UserId>(), It.IsAny<string>(), It.IsAny<Password>(), It.IsAny<User>()),
                Times.Never);
        }

        [Theory]
        [InlineData("short", "8文字以上")]
        [InlineData("1234567", "8文字以上")]
        [InlineData("", "8文字以上")]
        public async Task ChangePasswordAsync_ShortPassword_ShouldReturnValidationError(string shortPassword, string expectedErrorMessage)
        {
            // Arrange
            var command = new UbiquitousLanguageManager.Application.ChangePasswordCommand(
                1L,
                "oldPassword",
                shortPassword,
                shortPassword
            );

            // Act
            var result = await _useCase.ChangePasswordAsync(command);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.NotEmpty(result.ValidationErrors);
            Assert.Contains(result.ValidationErrors, e => e.Item1 == "NewPassword");
            Assert.Contains(expectedErrorMessage, result.ValidationErrors.First(e => e.Item1 == "NewPassword").Item2);

            // ApplicationServiceは呼ばれない
            _mockUserAppService.Verify(
                x => x.ChangePasswordAsync(It.IsAny<UserId>(), It.IsAny<string>(), It.IsAny<Password>(), It.IsAny<User>()),
                Times.Never);
        }

        [Fact]
        public async Task ChangePasswordAsync_InvalidPasswordFormat_ShouldReturnValidationError()
        {
            // Arrange
            var command = new UbiquitousLanguageManager.Application.ChangePasswordCommand(
                1L,
                "oldPassword",
                "weakpassword", // 8文字以上だが強度が不足
                "weakpassword"
            );

            // Act
            var result = await _useCase.ChangePasswordAsync(command);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.NotEmpty(result.ValidationErrors);
            Assert.Contains(result.ValidationErrors, e => e.Item1 == "NewPassword");

            // ApplicationServiceは呼ばれない
            _mockUserAppService.Verify(
                x => x.ChangePasswordAsync(It.IsAny<UserId>(), It.IsAny<string>(), It.IsAny<Password>(), It.IsAny<User>()),
                Times.Never);
        }

        [Fact]
        public async Task ChangePasswordAsync_ApplicationServiceError_ShouldReturnError()
        {
            // Arrange
            var command = new UbiquitousLanguageManager.Application.ChangePasswordCommand(
                1L,
                "wrongOldPassword", // 間違った現在のパスワード
                "NewPassword123!",
                "NewPassword123!"
            );

            _mockUserAppService
                .Setup(x => x.ChangePasswordAsync(It.IsAny<UserId>(), It.IsAny<string>(), It.IsAny<Password>(), It.IsAny<User>()))
                .ReturnsAsync(FSharpResult<User, string>.NewError("現在のパスワードが正しくありません"));

            // Act
            var result = await _useCase.ChangePasswordAsync(command);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal("現在のパスワードが正しくありません", result.ErrorMessage);
            Assert.Null(result.Data);
            Assert.Empty(result.ValidationErrors);
        }
    }

    /// <summary>
    /// UseCaseResultヘルパーメソッドのテスト
    /// </summary>
    public class UseCaseResultHelpersTests
    {
        [Fact]
        public void UseCaseResult_Success_ShouldCreateValidSuccessResult()
        {
            // Arrange
            var testData = "テストデータ";

            // Act
            var result = UseCaseResult.success(testData);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Data);
            Assert.Equal(testData, result.Data);
            Assert.Null(result.ErrorMessage);
            Assert.Empty(result.ValidationErrors);
        }

        [Fact]
        public void UseCaseResult_Error_ShouldCreateValidErrorResult()
        {
            // Arrange
            var errorMessage = "エラーメッセージ";

            // Act
            var result = UseCaseResult.error<string>(errorMessage);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Null(result.Data);
            Assert.Equal(errorMessage, result.ErrorMessage);
            Assert.Empty(result.ValidationErrors);
        }

        [Fact]
        public void UseCaseResult_ValidationError_ShouldCreateValidValidationErrorResult()
        {
            // Arrange
            var validationErrors = new List<(string, string)>
            {
                ("Field1", "エラー1"),
                ("Field2", "エラー2")
            };

            // Convert to F# list
            var fsharpErrors = ListModule.OfSeq(validationErrors.Select(x => new Tuple<string, string>(x.Item1, x.Item2)));

            // Act
            var result = UseCaseResult.validationError<string>(fsharpErrors);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Null(result.Data);
            Assert.Equal("入力値に問題があります", result.ErrorMessage);
            Assert.Equal(2, result.ValidationErrors.Length);
            Assert.Contains(result.ValidationErrors, e => e.Item1 == "Field1" && e.Item2 == "エラー1");
            Assert.Contains(result.ValidationErrors, e => e.Item1 == "Field2" && e.Item2 == "エラー2");
        }

        [Fact]
        public void UseCaseResult_FromResult_Success_ShouldConvertCorrectly()
        {
            // Arrange
            var testData = "テストデータ";
            var fsharpResult = FSharpResult<string, string>.NewOk(testData);

            // Act
            var result = UseCaseResult.fromResult(fsharpResult);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(testData, result.Data);
            Assert.Null(result.ErrorMessage);
            Assert.Empty(result.ValidationErrors);
        }

        [Fact]
        public void UseCaseResult_FromResult_Error_ShouldConvertCorrectly()
        {
            // Arrange
            var errorMessage = "エラーメッセージ";
            var fsharpResult = FSharpResult<string, string>.NewError(errorMessage);

            // Act
            var result = UseCaseResult.fromResult(fsharpResult);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Null(result.Data);
            Assert.Equal(errorMessage, result.ErrorMessage);
            Assert.Empty(result.ValidationErrors);
        }
    }
}