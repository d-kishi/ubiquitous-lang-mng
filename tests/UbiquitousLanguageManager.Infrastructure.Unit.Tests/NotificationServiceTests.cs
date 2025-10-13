using Microsoft.Extensions.Logging;
using Microsoft.FSharp.Core;
using Moq;
using UbiquitousLanguageManager.Domain.Common;
using UbiquitousLanguageManager.Domain.Authentication;
using UbiquitousLanguageManager.Infrastructure.Services;
using Xunit;
using DomainUser = UbiquitousLanguageManager.Domain.Authentication.User;

namespace UbiquitousLanguageManager.Infrastructure.Unit.Tests;

/// <summary>
/// NotificationServiceの単体テスト
/// 
/// 【テスト方針】
/// Phase A2で実装済みの通知機能（ログ出力ベース）の動作確認と、
/// Phase A3で実装予定のスタブメソッドのエラー応答を検証します。
/// ログ出力が正しく行われることも重要な検証項目です。
/// </summary>
public class NotificationServiceTests
{
    private readonly Mock<ILogger<NotificationService>> _mockLogger;
    private readonly NotificationService _service;

    protected DomainUser CreateTestUser(string email = "test@example.com", string name = "テストユーザー", Role? role = null)
    {
        var emailResult = Email.create(email);
        var nameResult = UserName.create(name);

        Assert.True(emailResult.IsOk);
        Assert.True(nameResult.IsOk);

        return DomainUser.createWithId(
            emailResult.ResultValue,
            nameResult.ResultValue,
            role ?? Role.GeneralUser,
            UserId.create(1L)
        );
    }

    public NotificationServiceTests()
    {
        _mockLogger = new Mock<ILogger<NotificationService>>();
        _service = new NotificationService(_mockLogger.Object);
    }

    /// <summary>
    /// NotifyUserCreatedAsyncのテスト（Phase A2実装済み）
    /// </summary>
    public class NotifyUserCreatedAsyncTests : NotificationServiceTests
    {
        [Fact]
        public async Task NotifyUserCreatedAsync_ValidUser_ShouldReturnSuccess_AndLogMessages()
        {
            // Arrange
            var user = CreateTestUser("newuser@example.com", "新規ユーザー", Role.GeneralUser);
            var temporaryPassword = "su";

            // Act
            var result = await _service.NotifyUserCreatedAsync(user, temporaryPassword);

            // Assert
            Assert.True(result.IsOk);
            
            // ユーザー作成成功ログの確認
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("User account created successfully") && 
                                                 v.ToString()!.Contains("newuser@example.com") && 
                                                 v.ToString()!.Contains("新規ユーザー")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
            
            // 管理者向け通知ログの確認
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("ADMIN_NOTIFICATION") && 
                                                 v.ToString()!.Contains("New user account requires initial setup")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task NotifyUserCreatedAsync_SuperUser_ShouldLogCorrectRole()
        {
            // Arrange
            var user = CreateTestUser("admin@example.com", "管理者", Role.SuperUser);
            var temporaryPassword = "AdminPass123!";

            // Act
            var result = await _service.NotifyUserCreatedAsync(user, temporaryPassword);

            // Assert
            Assert.True(result.IsOk);

            // SuperUserロールのログ出力確認（2回：ユーザー作成ログ＋管理者通知ログ）
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("SuperUser")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Exactly(2)); // 2回のログ出力を期待
        }

        [Fact]
        public async Task NotifyUserCreatedAsync_AllRoles_ShouldLogCorrectRoleStrings()
        {
            // Arrange & Act & Assert
            var testCases = new[]
            {
                (Role.SuperUser, "SuperUser"),
                (Role.ProjectManager, "ProjectManager"),
                (Role.DomainApprover, "DomainApprover"),
                (Role.GeneralUser, "GeneralUser")
            };

            foreach (var (role, expectedRoleString) in testCases)
            {
                var user = CreateTestUser($"test{expectedRoleString}@example.com", $"テスト{expectedRoleString}", role);
                var result = await _service.NotifyUserCreatedAsync(user, "su");
                
                Assert.True(result.IsOk);
                
                _mockLogger.Verify(
                    x => x.Log(
                        LogLevel.Information,
                        It.IsAny<EventId>(),
                        It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains($"Role: {expectedRoleString}")),
                        It.IsAny<Exception>(),
                        It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                    Times.AtLeastOnce);
                
                _mockLogger.Reset(); // 次のテストケース用にリセット
            }
        }
    }

    /// <summary>
    /// NotifyPasswordChangedAsyncのテスト（Phase A2実装済み）
    /// </summary>
    public class NotifyPasswordChangedAsyncTests : NotificationServiceTests
    {
        [Fact]
        public async Task NotifyPasswordChangedAsync_FirstLogin_ShouldReturnSuccess_AndLogCorrectType()
        {
            // Arrange
            var user = CreateTestUser("user@example.com", "ユーザー", Role.GeneralUser);
            var isFirstLogin = true;

            // Act
            var result = await _service.NotifyPasswordChangedAsync(user, isFirstLogin);

            // Assert
            Assert.True(result.IsOk);
            
            // 初回ログイン時パスワード設定のログ確認
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Password changed successfully") && 
                                                 v.ToString()!.Contains("初回ログイン時パスワード設定")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
            
            // セキュリティ監査ログの確認
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("SECURITY_AUDIT") && 
                                                 v.ToString()!.Contains("Password change event")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task NotifyPasswordChangedAsync_RegularChange_ShouldReturnSuccess_AndLogCorrectType()
        {
            // Arrange
            var user = CreateTestUser("user@example.com", "ユーザー", Role.GeneralUser);
            var isFirstLogin = false;

            // Act
            var result = await _service.NotifyPasswordChangedAsync(user, isFirstLogin);

            // Assert
            Assert.True(result.IsOk);
            
            // 通常のパスワード変更のログ確認
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Password changed successfully") && 
                                                 v.ToString()!.Contains("パスワード変更")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }
    }

    /// <summary>
    /// NotifyRoleChangedAsyncのテスト（Phase A2実装済み）
    /// </summary>
    public class NotifyRoleChangedAsyncTests : NotificationServiceTests
    {
        [Fact]
        public async Task NotifyRoleChangedAsync_RegularRoleChange_ShouldReturnSuccess_AndLogChange()
        {
            // Arrange
            var user = CreateTestUser("user@example.com", "ユーザー", Role.ProjectManager);
            var previousRole = Role.GeneralUser;
            var changedBy = CreateTestUser("admin@example.com", "管理者", Role.SuperUser);

            // Act
            var result = await _service.NotifyRoleChangedAsync(user, previousRole, changedBy);

            // Assert
            Assert.True(result.IsOk);
            
            // ロール変更ログの確認
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("User role changed successfully") && 
                                                 v.ToString()!.Contains("PreviousRole: GeneralUser") && 
                                                 v.ToString()!.Contains("CurrentRole: ProjectManager")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
            
            // 管理者向け通知ログの確認
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("ADMIN_NOTIFICATION") && 
                                                 v.ToString()!.Contains("User role change requires attention")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task NotifyRoleChangedAsync_PrivilegeEscalation_ShouldLogWarning()
        {
            // Arrange: GeneralUser → SuperUser（権限昇格）
            var user = CreateTestUser("user@example.com", "ユーザー", Role.SuperUser);
            var previousRole = Role.GeneralUser;
            var changedBy = CreateTestUser("admin@example.com", "管理者", Role.SuperUser);

            // Act
            var result = await _service.NotifyRoleChangedAsync(user, previousRole, changedBy);

            // Assert
            Assert.True(result.IsOk);
            
            // 権限昇格の警告ログ確認
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("SECURITY_AUDIT") && 
                                                 v.ToString()!.Contains("Privilege escalation detected") && 
                                                 v.ToString()!.Contains("GeneralUser → SuperUser")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Theory]
        [InlineData("GeneralUser", "DomainApprover", true)]    // 1→2: 権限昇格
        [InlineData("DomainApprover", "ProjectManager", true)] // 2→3: 権限昇格
        [InlineData("ProjectManager", "SuperUser", true)]      // 3→4: 権限昇格
        [InlineData("SuperUser", "ProjectManager", false)]     // 4→3: 権限降格
        [InlineData("ProjectManager", "DomainApprover", false)] // 3→2: 権限降格
        [InlineData("DomainApprover", "GeneralUser", false)]   // 2→1: 権限降格
        [InlineData("GeneralUser", "GeneralUser", false)]      // 1→1: 変更なし
        public async Task NotifyRoleChangedAsync_VariousRoleChanges_ShouldDetectPrivilegeEscalation(
            string previousRoleStr, string currentRoleStr, bool expectedEscalation)
        {
            // Arrange
            var previousRole = StringToRole(previousRoleStr);
            var currentRole = StringToRole(currentRoleStr);
            var user = CreateTestUser("user@example.com", "ユーザー", currentRole);
            var changedBy = CreateTestUser("admin@example.com", "管理者", Role.SuperUser);

            // Act
            var result = await _service.NotifyRoleChangedAsync(user, previousRole, changedBy);

            // Assert
            Assert.True(result.IsOk);
            
            if (expectedEscalation)
            {
                // 権限昇格の場合はWarningログが出力される
                _mockLogger.Verify(
                    x => x.Log(
                        LogLevel.Warning,
                        It.IsAny<EventId>(),
                        It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Privilege escalation detected")),
                        It.IsAny<Exception>(),
                        It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                    Times.Once);
            }
            else
            {
                // 権限昇格でない場合はWarningログは出力されない
                _mockLogger.Verify(
                    x => x.Log(
                        LogLevel.Warning,
                        It.IsAny<EventId>(),
                        It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Privilege escalation detected")),
                        It.IsAny<Exception>(),
                        It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                    Times.Never);
            }
        }

        private static Role StringToRole(string roleStr) => roleStr switch
        {
            "SuperUser" => Role.SuperUser,
            "ProjectManager" => Role.ProjectManager,
            "DomainApprover" => Role.DomainApprover,
            "GeneralUser" => Role.GeneralUser,
            _ => throw new ArgumentException($"Invalid role: {roleStr}")
        };
    }

    /// <summary>
    /// NotifySystemEventAsyncのテスト（Phase A2実装済み）
    /// </summary>
    public class NotifySystemEventAsyncTests : NotificationServiceTests
    {
        [Theory]
        [InlineData("info", LogLevel.Information)]
        [InlineData("Info", LogLevel.Information)]
        [InlineData("INFO", LogLevel.Information)]
        [InlineData("warning", LogLevel.Warning)]
        [InlineData("Warning", LogLevel.Warning)]
        [InlineData("WARNING", LogLevel.Warning)]
        [InlineData("error", LogLevel.Error)]
        [InlineData("Error", LogLevel.Error)]
        [InlineData("ERROR", LogLevel.Error)]
        public async Task NotifySystemEventAsync_ValidLevels_ShouldLogWithCorrectLevel(string level, LogLevel expectedLogLevel)
        {
            // Arrange
            var message = $"テストシステムイベント - レベル: {level}";

            // Act
            var result = await _service.NotifySystemEventAsync(message, level);

            // Assert
            Assert.True(result.IsOk);
            
            _mockLogger.Verify(
                x => x.Log(
                    expectedLogLevel,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("SYSTEM_NOTIFICATION") && 
                                                 v.ToString()!.Contains(message)),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task NotifySystemEventAsync_InvalidLevel_ShouldLogAsInfo()
        {
            // Arrange
            var message = "カスタムレベルのメッセージ";
            var customLevel = "custom";

            // Act
            var result = await _service.NotifySystemEventAsync(message, customLevel);

            // Assert
            Assert.True(result.IsOk);
            
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("SYSTEM_NOTIFICATION") && 
                                                 v.ToString()!.Contains(message) && 
                                                 v.ToString()!.Contains("Level: custom")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }
    }

    /// <summary>
    /// Phase A3実装予定メソッドのテスト（スタブ実装）
    /// </summary>
    public class Phase3StubMethodsTests : NotificationServiceTests
    {
        [Fact]
        public async Task SendWelcomeEmailAsync_ShouldReturnError()
        {
            // Arrange
            var emailResult = Email.create("test@example.com");
            Assert.True(emailResult.IsOk);
            var email = emailResult.ResultValue;

            // Act
            var result = await _service.SendWelcomeEmailAsync(email);

            // Assert
            Assert.True(result.IsError);
            Assert.Contains("Phase A3以降で実装予定", result.ErrorValue);
        }

        [Fact]
        public async Task SendRoleChangeNotificationAsync_ShouldReturnError()
        {
            // Arrange
            var emailResult = Email.create("test@example.com");
            Assert.True(emailResult.IsOk);
            var email = emailResult.ResultValue;
            var role = Role.ProjectManager;

            // Act
            var result = await _service.SendRoleChangeNotificationAsync(email, role);

            // Assert
            Assert.True(result.IsError);
            Assert.Contains("Phase A3以降で実装予定", result.ErrorValue);
        }

        [Fact]
        public async Task SendPasswordChangeNotificationAsync_ShouldReturnError()
        {
            // Arrange
            var emailResult = Email.create("test@example.com");
            Assert.True(emailResult.IsOk);
            var email = emailResult.ResultValue;

            // Act
            var result = await _service.SendPasswordChangeNotificationAsync(email);

            // Assert
            Assert.True(result.IsError);
            Assert.Contains("Phase A3以降で実装予定", result.ErrorValue);
        }

        [Fact]
        public async Task SendEmailChangeConfirmationAsync_ShouldReturnError()
        {
            // Arrange
            var oldEmailResult = Email.create("old@example.com");
            var newEmailResult = Email.create("new@example.com");
            Assert.True(oldEmailResult.IsOk);
            Assert.True(newEmailResult.IsOk);
            var oldEmail = oldEmailResult.ResultValue;
            var newEmail = newEmailResult.ResultValue;

            // Act
            var result = await _service.SendEmailChangeConfirmationAsync(oldEmail, newEmail);

            // Assert
            Assert.True(result.IsError);
            Assert.Contains("Phase A3以降で実装予定", result.ErrorValue);
        }

        [Fact]
        public async Task SendAccountDeactivationNotificationAsync_ShouldReturnError()
        {
            // Arrange
            var emailResult = Email.create("test@example.com");
            Assert.True(emailResult.IsOk);
            var email = emailResult.ResultValue;

            // Act
            var result = await _service.SendAccountDeactivationNotificationAsync(email);

            // Assert
            Assert.True(result.IsError);
            Assert.Contains("Phase A3以降で実装予定", result.ErrorValue);
        }

        [Fact]
        public async Task SendAccountActivationNotificationAsync_ShouldReturnError()
        {
            // Arrange
            var emailResult = Email.create("test@example.com");
            Assert.True(emailResult.IsOk);
            var email = emailResult.ResultValue;

            // Act
            var result = await _service.SendAccountActivationNotificationAsync(email);

            // Assert
            Assert.True(result.IsError);
            Assert.Contains("Phase A3以降で実装予定", result.ErrorValue);
        }

        [Fact]
        public async Task SendSecurityAlertAsync_ShouldReturnError()
        {
            // Arrange
            var emailResult = Email.create("test@example.com");
            Assert.True(emailResult.IsOk);
            var email = emailResult.ResultValue;
            var alertType = "login_attempt";
            var details = "不正なログイン試行が検出されました";

            // Act
            var result = await _service.SendSecurityAlertAsync(email, alertType, details);

            // Assert
            Assert.True(result.IsError);
            Assert.Contains("Phase A3以降で実装予定", result.ErrorValue);
        }
    }
}