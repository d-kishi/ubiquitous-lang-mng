using Microsoft.FSharp.Core;
using Microsoft.FSharp.Collections;
using Moq;
using UbiquitousLanguageManager.Application;
using UbiquitousLanguageManager.Domain;
using Xunit;

namespace UbiquitousLanguageManager.Tests.Application;

/// <summary>
/// UserApplicationServiceの単体テスト
/// 
/// 【テスト方針】
/// Phase A2で大幅に拡張されたUserApplicationServiceのビジネスロジック、
/// ドメインサービス連携、権限チェック、外部サービス呼び出しを検証します。
/// F#のtask計算式とResult型の動作も含めて検証を行います。
/// </summary>
public class UserApplicationServiceTests
{
    private readonly Mock<IUserRepository> _mockUserRepository;
    private readonly Mock<IAuthenticationService> _mockAuthService;
    private readonly Mock<INotificationService> _mockNotificationService;
    private readonly Mock<ILogger<UserApplicationService>> _mockLogger;
    private readonly UserApplicationService _service;

    public UserApplicationServiceTests()
    {
        _mockUserRepository = new Mock<IUserRepository>();
        _mockAuthService = new Mock<IAuthenticationService>();
        _mockNotificationService = new Mock<INotificationService>();
        _mockLogger = new Mock<ILogger<UserApplicationService>>();
        
        _service = new UserApplicationService(
            _mockUserRepository.Object,
            _mockAuthService.Object,
            _mockNotificationService.Object,
            _mockLogger.Object);
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
    /// CreateUserAsyncのテスト
    /// </summary>
    public class CreateUserAsyncTests : UserApplicationServiceTests
    {
        [Fact]
        public async Task CreateUserAsync_ValidInput_ShouldCreateUser_AndSendWelcomeEmail()
        {
            // Arrange
            var emailResult = Email.create("newuser@example.com");
            var nameResult = UserName.create("新規ユーザー");
            Assert.True(emailResult.IsOk);
            Assert.True(nameResult.IsOk);
            
            var email = emailResult.ResultValue;
            var name = nameResult.ResultValue;
            var role = Role.GeneralUser;
            var operatorUser = CreateTestUser("admin@example.com", "管理者", Role.SuperUser);
            
            var newUser = CreateTestUser("newuser@example.com", "新規ユーザー", Role.GeneralUser, 2L);
            var existingUsers = new List<User> { operatorUser };

            // モックの設定
            _mockUserRepository
                .Setup(x => x.GetAllActiveUsersAsync())
                .ReturnsAsync(FSharpResult<FSharpList<User>, string>.NewOk(ListModule.OfSeq(existingUsers)));
                
            _mockUserRepository
                .Setup(x => x.SaveAsync(It.IsAny<User>()))
                .ReturnsAsync(FSharpResult<User, string>.NewOk(newUser));
                
            _mockNotificationService
                .Setup(x => x.SendWelcomeEmailAsync(It.IsAny<Email>()))
                .ReturnsAsync(FSharpResult<Microsoft.FSharp.Core.Unit, string>.NewOk(null!));

            // Act
            var result = await _service.CreateUserAsync(email, name, role, operatorUser);

            // Assert
            Assert.True(result.IsOk);
            
            // リポジトリが呼ばれることを確認
            _mockUserRepository.Verify(x => x.GetAllActiveUsersAsync(), Times.Once);
            _mockUserRepository.Verify(x => x.SaveAsync(It.IsAny<User>()), Times.Once);
            
            // 通知サービスが呼ばれることを確認
            _mockNotificationService.Verify(x => x.SendWelcomeEmailAsync(email), Times.Once);
        }

        [Fact]
        public async Task CreateUserAsync_DuplicateEmail_ShouldReturnError()
        {
            // Arrange
            var emailResult = Email.create("existing@example.com");
            var nameResult = UserName.create("重複ユーザー");
            Assert.True(emailResult.IsOk);
            Assert.True(nameResult.IsOk);
            
            var email = emailResult.ResultValue;
            var name = nameResult.ResultValue;
            var role = Role.GeneralUser;
            var operatorUser = CreateTestUser("admin@example.com", "管理者", Role.SuperUser);
            
            // 既存ユーザーに同じメールアドレスが存在
            var existingUser = CreateTestUser("existing@example.com", "既存ユーザー");
            var existingUsers = new List<User> { operatorUser, existingUser };

            _mockUserRepository
                .Setup(x => x.GetAllActiveUsersAsync())
                .ReturnsAsync(FSharpResult<FSharpList<User>, string>.NewOk(ListModule.OfSeq(existingUsers)));

            // Act
            var result = await _service.CreateUserAsync(email, name, role, operatorUser);

            // Assert
            Assert.True(result.IsError);
            Assert.Contains("重複", result.ErrorValue);
            
            // Saveは呼ばれないことを確認
            _mockUserRepository.Verify(x => x.SaveAsync(It.IsAny<User>()), Times.Never);
            _mockNotificationService.Verify(x => x.SendWelcomeEmailAsync(It.IsAny<Email>()), Times.Never);
        }

        [Fact]
        public async Task CreateUserAsync_InsufficientPermission_ShouldReturnError()
        {
            // Arrange
            var emailResult = Email.create("newuser@example.com");
            var nameResult = UserName.create("新規ユーザー");
            Assert.True(emailResult.IsOk);
            Assert.True(nameResult.IsOk);
            
            var email = emailResult.ResultValue;
            var name = nameResult.ResultValue;
            var role = Role.SuperUser; // 上位ロール
            var operatorUser = CreateTestUser("user@example.com", "一般ユーザー", Role.GeneralUser); // 権限不足

            // Act
            var result = await _service.CreateUserAsync(email, name, role, operatorUser);

            // Assert
            Assert.True(result.IsError);
            
            // 権限チェックで止まるため、リポジトリアクセスは行われない
            _mockUserRepository.Verify(x => x.GetAllActiveUsersAsync(), Times.Never);
            _mockUserRepository.Verify(x => x.SaveAsync(It.IsAny<User>()), Times.Never);
        }
    }

    /// <summary>
    /// LoginAsyncのテスト
    /// </summary>
    public class LoginAsyncTests : UserApplicationServiceTests
    {
        [Fact]
        public async Task LoginAsync_ValidCredentials_ActiveUser_ShouldReturnUser()
        {
            // Arrange
            var emailResult = Email.create("user@example.com");
            Assert.True(emailResult.IsOk);
            var email = emailResult.ResultValue;
            var password = "password123";
            
            var user = CreateTestUser("user@example.com", "ユーザー", Role.GeneralUser);
            
            _mockAuthService
                .Setup(x => x.LoginAsync(email, password))
                .ReturnsAsync(FSharpResult<User, string>.NewOk(user));
                
            _mockUserRepository
                .Setup(x => x.SaveAsync(It.IsAny<User>()))
                .ReturnsAsync(FSharpResult<User, string>.NewOk(user));

            // Act
            var result = await _service.LoginAsync(email, password);

            // Assert
            Assert.True(result.IsOk);
            
            // 認証サービスが呼ばれることを確認
            _mockAuthService.Verify(x => x.LoginAsync(email, password), Times.Once);
            
            // ログイン成功記録のため、Saveが呼ばれることを確認
            _mockUserRepository.Verify(x => x.SaveAsync(It.IsAny<User>()), Times.Once);
        }

        [Fact]
        public async Task LoginAsync_InvalidCredentials_ShouldReturnError()
        {
            // Arrange
            var emailResult = Email.create("user@example.com");
            Assert.True(emailResult.IsOk);
            var email = emailResult.ResultValue;
            var password = "wrongpassword";
            
            _mockAuthService
                .Setup(x => x.LoginAsync(email, password))
                .ReturnsAsync(FSharpResult<User, string>.NewError("認証に失敗しました"));

            // Act
            var result = await _service.LoginAsync(email, password);

            // Assert
            Assert.True(result.IsError);
            Assert.Equal("認証に失敗しました", result.ErrorValue);
            
            // 認証失敗のため、Saveは呼ばれない
            _mockUserRepository.Verify(x => x.SaveAsync(It.IsAny<User>()), Times.Never);
        }

        [Fact]
        public async Task LoginAsync_InactiveUser_ShouldReturnError()
        {
            // Arrange
            var emailResult = Email.create("inactive@example.com");
            Assert.True(emailResult.IsOk);
            var email = emailResult.ResultValue;
            var password = "password123";
            
            // 無効化されたユーザーを作成
            var user = CreateTestUser("inactive@example.com", "無効ユーザー", Role.GeneralUser);
            var operatorUser = CreateTestUser("admin@example.com", "管理者", Role.SuperUser);
            var deactivatedUser = user.deactivate(operatorUser, operatorUser.Id);
            Assert.True(deactivatedUser.IsOk);
            
            _mockAuthService
                .Setup(x => x.LoginAsync(email, password))
                .ReturnsAsync(FSharpResult<User, string>.NewOk(deactivatedUser.ResultValue));

            // Act
            var result = await _service.LoginAsync(email, password);

            // Assert
            Assert.True(result.IsError);
            Assert.Contains("無効化", result.ErrorValue);
            
            // 無効ユーザーのため、Saveは呼ばれない
            _mockUserRepository.Verify(x => x.SaveAsync(It.IsAny<User>()), Times.Never);
        }
    }

    /// <summary>
    /// ChangePasswordAsyncのテスト
    /// </summary>
    public class ChangePasswordAsyncTests : UserApplicationServiceTests
    {
        [Fact]
        public async Task ChangePasswordAsync_ValidInput_ShouldChangePassword_AndSendNotification()
        {
            // Arrange
            var userId = UserId.create(1L);
            var currentPassword = "oldPassword";
            var newPasswordResult = Password.create("NewPassword123!");
            Assert.True(newPasswordResult.IsOk);
            var newPassword = newPasswordResult.ResultValue;
            var operatorUser = CreateTestUser("user@example.com", "ユーザー", Role.GeneralUser, 1L);
            
            var targetUser = CreateTestUser("user@example.com", "ユーザー", Role.GeneralUser, 1L);
            var passwordHashResult = PasswordHash.create("hashedNewPassword");
            Assert.True(passwordHashResult.IsOk);
            var passwordHash = passwordHashResult.ResultValue;

            _mockUserRepository
                .Setup(x => x.GetByIdAsync(userId))
                .ReturnsAsync(FSharpResult<FSharpOption<User>, string>.NewOk(FSharpOption<User>.Some(targetUser)));
                
            _mockAuthService
                .Setup(x => x.ChangePasswordAsync(userId, currentPassword, newPassword))
                .ReturnsAsync(FSharpResult<PasswordHash, string>.NewOk(passwordHash));
                
            _mockUserRepository
                .Setup(x => x.SaveAsync(It.IsAny<User>()))
                .ReturnsAsync(FSharpResult<User, string>.NewOk(targetUser));
                
            _mockNotificationService
                .Setup(x => x.SendPasswordChangeNotificationAsync(It.IsAny<Email>()))
                .ReturnsAsync(FSharpResult<Microsoft.FSharp.Core.Unit, string>.NewOk(null!));

            // Act
            var result = await _service.ChangePasswordAsync(userId, currentPassword, newPassword, operatorUser);

            // Assert
            Assert.True(result.IsOk);
            
            // 各サービスが正しく呼ばれることを確認
            _mockUserRepository.Verify(x => x.GetByIdAsync(userId), Times.Once);
            _mockAuthService.Verify(x => x.ChangePasswordAsync(userId, currentPassword, newPassword), Times.Once);
            _mockUserRepository.Verify(x => x.SaveAsync(It.IsAny<User>()), Times.Once);
            _mockNotificationService.Verify(x => x.SendPasswordChangeNotificationAsync(It.IsAny<Email>()), Times.Once);
        }

        [Fact]
        public async Task ChangePasswordAsync_UserNotFound_ShouldReturnError()
        {
            // Arrange
            var userId = UserId.create(999L); // 存在しないユーザー
            var currentPassword = "oldPassword";
            var newPasswordResult = Password.create("NewPassword123!");
            Assert.True(newPasswordResult.IsOk);
            var newPassword = newPasswordResult.ResultValue;
            var operatorUser = CreateTestUser("admin@example.com", "管理者", Role.SuperUser);

            _mockUserRepository
                .Setup(x => x.GetByIdAsync(userId))
                .ReturnsAsync(FSharpResult<FSharpOption<User>, string>.NewOk(FSharpOption<User>.None));

            // Act
            var result = await _service.ChangePasswordAsync(userId, currentPassword, newPassword, operatorUser);

            // Assert
            Assert.True(result.IsError);
            Assert.Contains("見つかりません", result.ErrorValue);
            
            // ユーザーが見つからないため、後続処理は実行されない
            _mockAuthService.Verify(x => x.ChangePasswordAsync(It.IsAny<UserId>(), It.IsAny<string>(), It.IsAny<Password>()), Times.Never);
            _mockNotificationService.Verify(x => x.SendPasswordChangeNotificationAsync(It.IsAny<Email>()), Times.Never);
        }

        [Fact]
        public async Task ChangePasswordAsync_AuthServiceFailure_ShouldReturnError()
        {
            // Arrange
            var userId = UserId.create(1L);
            var currentPassword = "wrongPassword"; // 間違ったパスワード
            var newPasswordResult = Password.create("NewPassword123!");
            Assert.True(newPasswordResult.IsOk);
            var newPassword = newPasswordResult.ResultValue;
            var operatorUser = CreateTestUser("user@example.com", "ユーザー", Role.GeneralUser, 1L);
            
            var targetUser = CreateTestUser("user@example.com", "ユーザー", Role.GeneralUser, 1L);

            _mockUserRepository
                .Setup(x => x.GetByIdAsync(userId))
                .ReturnsAsync(FSharpResult<FSharpOption<User>, string>.NewOk(FSharpOption<User>.Some(targetUser)));
                
            _mockAuthService
                .Setup(x => x.ChangePasswordAsync(userId, currentPassword, newPassword))
                .ReturnsAsync(FSharpResult<PasswordHash, string>.NewError("現在のパスワードが正しくありません"));

            // Act
            var result = await _service.ChangePasswordAsync(userId, currentPassword, newPassword, operatorUser);

            // Assert
            Assert.True(result.IsError);
            Assert.Equal("現在のパスワードが正しくありません", result.ErrorValue);
            
            // パスワード変更失敗のため、保存と通知は実行されない
            _mockUserRepository.Verify(x => x.SaveAsync(It.IsAny<User>()), Times.Never);
            _mockNotificationService.Verify(x => x.SendPasswordChangeNotificationAsync(It.IsAny<Email>()), Times.Never);
        }
    }

    /// <summary>
    /// ChangeUserRoleAsyncのテスト
    /// </summary>
    public class ChangeUserRoleAsyncTests : UserApplicationServiceTests
    {
        [Fact]
        public async Task ChangeUserRoleAsync_ValidInput_ShouldChangeRole_AndSendNotification()
        {
            // Arrange
            var userId = UserId.create(2L);
            var newRole = Role.ProjectManager;
            var operatorUser = CreateTestUser("admin@example.com", "管理者", Role.SuperUser, 1L);
            var targetUser = CreateTestUser("user@example.com", "ユーザー", Role.GeneralUser, 2L);

            _mockUserRepository
                .Setup(x => x.GetByIdAsync(userId))
                .ReturnsAsync(FSharpResult<FSharpOption<User>, string>.NewOk(FSharpOption<User>.Some(targetUser)));
                
            _mockUserRepository
                .Setup(x => x.SaveAsync(It.IsAny<User>()))
                .ReturnsAsync(FSharpResult<User, string>.NewOk(targetUser));
                
            _mockNotificationService
                .Setup(x => x.SendRoleChangeNotificationAsync(It.IsAny<Email>(), newRole))
                .ReturnsAsync(FSharpResult<Microsoft.FSharp.Core.Unit, string>.NewOk(null!));

            // Act
            var result = await _service.ChangeUserRoleAsync(userId, newRole, operatorUser);

            // Assert
            Assert.True(result.IsOk);
            
            // 各サービスが正しく呼ばれることを確認
            _mockUserRepository.Verify(x => x.GetByIdAsync(userId), Times.Once);
            _mockUserRepository.Verify(x => x.SaveAsync(It.IsAny<User>()), Times.Once);
            _mockNotificationService.Verify(x => x.SendRoleChangeNotificationAsync(It.IsAny<Email>(), newRole), Times.Once);
        }

        [Fact]
        public async Task ChangeUserRoleAsync_InsufficientPermission_ShouldReturnError()
        {
            // Arrange
            var userId = UserId.create(2L);
            var newRole = Role.SuperUser; // 上位ロール
            var operatorUser = CreateTestUser("user@example.com", "一般ユーザー", Role.GeneralUser, 1L); // 権限不足
            var targetUser = CreateTestUser("target@example.com", "対象ユーザー", Role.GeneralUser, 2L);

            _mockUserRepository
                .Setup(x => x.GetByIdAsync(userId))
                .ReturnsAsync(FSharpResult<FSharpOption<User>, string>.NewOk(FSharpOption<User>.Some(targetUser)));

            // Act
            var result = await _service.ChangeUserRoleAsync(userId, newRole, operatorUser);

            // Assert
            Assert.True(result.IsError);
            
            // 権限不足で処理が停止するため、保存と通知は実行されない
            _mockUserRepository.Verify(x => x.SaveAsync(It.IsAny<User>()), Times.Never);
            _mockNotificationService.Verify(x => x.SendRoleChangeNotificationAsync(It.IsAny<Email>(), It.IsAny<Role>()), Times.Never);
        }
    }

    /// <summary>
    /// DeactivateUserAsyncのテスト
    /// </summary>
    public class DeactivateUserAsyncTests : UserApplicationServiceTests
    {
        [Fact]
        public async Task DeactivateUserAsync_ValidInput_ShouldDeactivateUser_AndSendNotification()
        {
            // Arrange
            var userId = UserId.create(2L);
            var operatorUser = CreateTestUser("admin@example.com", "管理者", Role.SuperUser, 1L);
            var targetUser = CreateTestUser("user@example.com", "ユーザー", Role.GeneralUser, 2L);

            _mockUserRepository
                .Setup(x => x.GetByIdAsync(userId))
                .ReturnsAsync(FSharpResult<FSharpOption<User>, string>.NewOk(FSharpOption<User>.Some(targetUser)));
                
            _mockUserRepository
                .Setup(x => x.SaveAsync(It.IsAny<User>()))
                .ReturnsAsync(FSharpResult<User, string>.NewOk(targetUser));
                
            _mockNotificationService
                .Setup(x => x.SendAccountDeactivationNotificationAsync(It.IsAny<Email>()))
                .ReturnsAsync(FSharpResult<Microsoft.FSharp.Core.Unit, string>.NewOk(null!));

            // Act
            var result = await _service.DeactivateUserAsync(userId, operatorUser);

            // Assert
            Assert.True(result.IsOk);
            
            // 各サービスが正しく呼ばれることを確認
            _mockUserRepository.Verify(x => x.GetByIdAsync(userId), Times.Once);
            _mockUserRepository.Verify(x => x.SaveAsync(It.IsAny<User>()), Times.Once);
            _mockNotificationService.Verify(x => x.SendAccountDeactivationNotificationAsync(It.IsAny<Email>()), Times.Once);
        }
    }

    /// <summary>
    /// ActivateUserAsyncのテスト
    /// </summary>
    public class ActivateUserAsyncTests : UserApplicationServiceTests
    {
        [Fact]
        public async Task ActivateUserAsync_ValidInput_ShouldActivateUser_AndSendNotification()
        {
            // Arrange
            var userId = UserId.create(2L);
            var operatorUser = CreateTestUser("admin@example.com", "管理者", Role.SuperUser, 1L);
            var targetUser = CreateTestUser("user@example.com", "ユーザー", Role.GeneralUser, 2L);

            _mockUserRepository
                .Setup(x => x.GetByIdAsync(userId))
                .ReturnsAsync(FSharpResult<FSharpOption<User>, string>.NewOk(FSharpOption<User>.Some(targetUser)));
                
            _mockUserRepository
                .Setup(x => x.SaveAsync(It.IsAny<User>()))
                .ReturnsAsync(FSharpResult<User, string>.NewOk(targetUser));
                
            _mockNotificationService
                .Setup(x => x.SendAccountActivationNotificationAsync(It.IsAny<Email>()))
                .ReturnsAsync(FSharpResult<Microsoft.FSharp.Core.Unit, string>.NewOk(null!));

            // Act
            var result = await _service.ActivateUserAsync(userId, operatorUser);

            // Assert
            Assert.True(result.IsOk);
            
            // 各サービスが正しく呼ばれることを確認
            _mockUserRepository.Verify(x => x.GetByIdAsync(userId), Times.Once);
            _mockUserRepository.Verify(x => x.SaveAsync(It.IsAny<User>()), Times.Once);
            _mockNotificationService.Verify(x => x.SendAccountActivationNotificationAsync(It.IsAny<Email>()), Times.Once);
        }
    }

    /// <summary>
    /// GetUsersAsyncのテスト
    /// </summary>
    public class GetUsersAsyncTests : UserApplicationServiceTests
    {
        [Fact]
        public async Task GetUsersAsync_IncludeActive_ShouldReturnActiveUsers()
        {
            // Arrange
            var operatorUser = CreateTestUser("admin@example.com", "管理者", Role.SuperUser);
            var users = new List<User> { operatorUser };

            _mockUserRepository
                .Setup(x => x.GetAllActiveUsersAsync())
                .ReturnsAsync(FSharpResult<FSharpList<User>, string>.NewOk(ListModule.OfSeq(users)));

            // Act
            var result = await _service.GetUsersAsync(operatorUser, includeInactive: false);

            // Assert
            Assert.True(result.IsOk);
            _mockUserRepository.Verify(x => x.GetAllActiveUsersAsync(), Times.Once);
            _mockUserRepository.Verify(x => x.GetAllUsersAsync(), Times.Never);
        }

        [Fact]
        public async Task GetUsersAsync_IncludeInactive_ShouldReturnAllUsers()
        {
            // Arrange
            var operatorUser = CreateTestUser("admin@example.com", "管理者", Role.SuperUser);
            var users = new List<User> { operatorUser };

            _mockUserRepository
                .Setup(x => x.GetAllUsersAsync())
                .ReturnsAsync(FSharpResult<FSharpList<User>, string>.NewOk(ListModule.OfSeq(users)));

            // Act
            var result = await _service.GetUsersAsync(operatorUser, includeInactive: true);

            // Assert
            Assert.True(result.IsOk);
            _mockUserRepository.Verify(x => x.GetAllUsersAsync(), Times.Once);
            _mockUserRepository.Verify(x => x.GetAllActiveUsersAsync(), Times.Never);
        }

        [Fact]
        public async Task GetUsersAsync_InsufficientPermission_ShouldReturnError()
        {
            // Arrange
            var operatorUser = CreateTestUser("user@example.com", "一般ユーザー", Role.GeneralUser); // 権限不足

            // Act
            var result = await _service.GetUsersAsync(operatorUser, includeInactive: false);

            // Assert
            Assert.True(result.IsError);
            
            // 権限不足のため、リポジトリアクセスは行われない
            _mockUserRepository.Verify(x => x.GetAllActiveUsersAsync(), Times.Never);
            _mockUserRepository.Verify(x => x.GetAllUsersAsync(), Times.Never);
        }
    }

    /// <summary>
    /// GetUserByIdAsyncのテスト
    /// </summary>
    public class GetUserByIdAsyncTests : UserApplicationServiceTests
    {
        [Fact]
        public async Task GetUserByIdAsync_ValidUser_ShouldReturnUser()
        {
            // Arrange
            var userId = UserId.create(2L);
            var operatorUser = CreateTestUser("admin@example.com", "管理者", Role.SuperUser);
            var targetUser = CreateTestUser("user@example.com", "ユーザー", Role.GeneralUser, 2L);

            _mockUserRepository
                .Setup(x => x.GetByIdAsync(userId))
                .ReturnsAsync(FSharpResult<FSharpOption<User>, string>.NewOk(FSharpOption<User>.Some(targetUser)));

            // Act
            var result = await _service.GetUserByIdAsync(userId, operatorUser);

            // Assert
            Assert.True(result.IsOk);
            _mockUserRepository.Verify(x => x.GetByIdAsync(userId), Times.Once);
        }

        [Fact]
        public async Task GetUserByIdAsync_InsufficientPermission_ShouldReturnError()
        {
            // Arrange
            var userId = UserId.create(2L);
            var operatorUser = CreateTestUser("user@example.com", "一般ユーザー", Role.GeneralUser); // 権限不足

            // Act
            var result = await _service.GetUserByIdAsync(userId, operatorUser);

            // Assert
            Assert.True(result.IsError);
            
            // 権限不足のため、リポジトリアクセスは行われない
            _mockUserRepository.Verify(x => x.GetByIdAsync(It.IsAny<UserId>()), Times.Never);
        }
    }
}