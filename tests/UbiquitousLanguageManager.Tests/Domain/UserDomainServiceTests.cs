using UbiquitousLanguageManager.Domain;
using Microsoft.FSharp.Collections;
using Xunit;

namespace UbiquitousLanguageManager.Tests.Domain;

/// <summary>
/// UserDomainServiceの単体テスト
/// 
/// 【テスト方針】
/// Phase A2で新規追加されたUserDomainServiceのビジネスルール、
/// 権限チェック、ドメイン固有のバリデーションロジックを検証します。
/// </summary>
public class UserDomainServiceTests
{
    private Role ConvertIntToRole(int roleInt)
    {
        return roleInt switch
        {
            0 => Role.GeneralUser,
            1 => Role.DomainApprover,
            2 => Role.ProjectManager,
            3 => Role.SuperUser,
            _ => Role.GeneralUser
        };
    }

    private User CreateTestUser(string email = "test@example.com", string name = "テストユーザー", Role? role = null, long id = 1L, bool isActive = true)
    {
        var emailResult = Email.create(email);
        var nameResult = UserName.create(name);
        
        Assert.True(emailResult.IsOk);
        Assert.True(nameResult.IsOk);

        var user = User.create(
            emailResult.ResultValue,
            nameResult.ResultValue,
            role ?? Role.GeneralUser,
            UserId.create(id)
        );

        if (!isActive)
        {
            var adminUser = CreateTestUser("admin@example.com", "管理者", Role.SuperUser, 999L);
            var deactivatedResult = user.deactivate(adminUser, adminUser.Id);
            Assert.True(deactivatedResult.IsOk);
            return deactivatedResult.ResultValue;
        }

        return user;
    }

    /// <summary>
    /// validateUserCreationPermissionのテスト
    /// </summary>
    public class ValidateUserCreationPermissionTests : UserDomainServiceTests
    {
        [Fact]
        public void ValidateUserCreationPermission_SuperUserCreatingGeneralUser_ShouldReturnOk()
        {
            // Arrange
            var operatorUser = CreateTestUser("admin@example.com", "管理者", Role.SuperUser);
            var targetRole = Role.GeneralUser;

            // Act
            var result = UserDomainService.validateUserCreationPermission(operatorUser, targetRole);

            // Assert
            Assert.True(result.IsOk);
        }

        [Fact]
        public void ValidateUserCreationPermission_SuperUserCreatingSuperUser_ShouldReturnOk()
        {
            // Arrange
            var operatorUser = CreateTestUser("admin@example.com", "管理者", Role.SuperUser);
            var targetRole = Role.SuperUser;

            // Act
            var result = UserDomainService.validateUserCreationPermission(operatorUser, targetRole);

            // Assert
            Assert.True(result.IsOk);
        }

        [Fact]
        public void ValidateUserCreationPermission_ProjectManagerCreatingGeneralUser_ShouldReturnOk()
        {
            // Arrange
            var operatorUser = CreateTestUser("manager@example.com", "管理者", Role.ProjectManager);
            var targetRole = Role.GeneralUser;

            // Act
            var result = UserDomainService.validateUserCreationPermission(operatorUser, targetRole);

            // Assert
            Assert.True(result.IsOk);
        }

        [Fact]
        public void ValidateUserCreationPermission_ProjectManagerCreatingSuperUser_ShouldReturnError()
        {
            // Arrange
            var operatorUser = CreateTestUser("manager@example.com", "管理者", Role.ProjectManager);
            var targetRole = Role.SuperUser;

            // Act
            var result = UserDomainService.validateUserCreationPermission(operatorUser, targetRole);

            // Assert
            Assert.True(result.IsError);
            Assert.Equal("SuperUserの作成はSuperUserのみが実行できます", result.ErrorValue);
        }

        [Fact]
        public void ValidateUserCreationPermission_GeneralUserCreatingAnyUser_ShouldReturnError()
        {
            // Arrange
            var operatorUser = CreateTestUser("user@example.com", "一般ユーザー", Role.GeneralUser);
            var targetRole = Role.GeneralUser;

            // Act
            var result = UserDomainService.validateUserCreationPermission(operatorUser, targetRole);

            // Assert
            Assert.True(result.IsError);
            Assert.Equal("ユーザー作成の権限がありません", result.ErrorValue);
        }

        [Fact]
        public void ValidateUserCreationPermission_InactiveUserCreatingAnyUser_ShouldReturnError()
        {
            // Arrange
            var operatorUser = CreateTestUser("inactive@example.com", "無効ユーザー", Role.SuperUser, 1L, false);
            var targetRole = Role.GeneralUser;

            // Act
            var result = UserDomainService.validateUserCreationPermission(operatorUser, targetRole);

            // Assert
            Assert.True(result.IsError);
            Assert.Equal("非アクティブなユーザーは新規ユーザーを作成できません", result.ErrorValue);
        }

        [Theory]
        [InlineData(3, 3, true)]      // SuperUser can create SuperUser
        [InlineData(3, 2, true)]      // SuperUser can create ProjectManager
        [InlineData(3, 1, true)]      // SuperUser can create DomainApprover
        [InlineData(3, 0, true)]      // SuperUser can create GeneralUser
        [InlineData(2, 3, false)]     // ProjectManager cannot create SuperUser
        [InlineData(2, 2, true)]      // ProjectManager can create ProjectManager
        [InlineData(2, 1, true)]      // ProjectManager can create DomainApprover
        [InlineData(2, 0, true)]      // ProjectManager can create GeneralUser
        [InlineData(1, 3, false)]     // DomainApprover cannot create SuperUser
        [InlineData(1, 2, false)]     // DomainApprover cannot create ProjectManager
        [InlineData(1, 1, false)]     // DomainApprover cannot create users
        [InlineData(1, 0, false)]     // DomainApprover cannot create users
        [InlineData(0, 3, false)]     // GeneralUser cannot create SuperUser
        [InlineData(0, 2, false)]     // GeneralUser cannot create ProjectManager
        [InlineData(0, 1, false)]     // GeneralUser cannot create DomainApprover
        [InlineData(0, 0, false)]     // GeneralUser cannot create GeneralUser
        public void ValidateUserCreationPermission_VariousRoleCombinations_ShouldReturnExpectedResult(
            int operatorRoleInt, int targetRoleInt, bool expectedSuccess)
        {
            // Arrange
            var operatorRole = ConvertIntToRole(operatorRoleInt);
            var targetRole = ConvertIntToRole(targetRoleInt);
            var operatorUser = CreateTestUser("operator@example.com", "操作者", operatorRole);

            // Act
            var result = UserDomainService.validateUserCreationPermission(operatorUser, targetRole);

            // Assert
            Assert.Equal(expectedSuccess, result.IsOk);
            if (!expectedSuccess)
            {
                Assert.True(result.IsError);
                Assert.NotEmpty(result.ErrorValue);
            }
        }
    }

    /// <summary>
    /// validateUniqueEmailのテスト
    /// </summary>
    public class ValidateUniqueEmailTests : UserDomainServiceTests
    {
        [Fact]
        public void ValidateUniqueEmail_NewUniqueEmail_ShouldReturnOk()
        {
            // Arrange
            var emailResult = Email.create("newuser@example.com");
            Assert.True(emailResult.IsOk);
            var email = emailResult.ResultValue;
            
            var existingUsers = new List<User>
            {
                CreateTestUser("user1@example.com", "ユーザー1"),
                CreateTestUser("user2@example.com", "ユーザー2")
            };

            // Act
            var result = UserDomainService.validateUniqueEmail(email, ListModule.OfSeq(existingUsers));

            // Assert
            Assert.True(result.IsOk);
        }

        [Fact]
        public void ValidateUniqueEmail_DuplicateEmailActiveUser_ShouldReturnError()
        {
            // Arrange
            var emailResult = Email.create("duplicate@example.com");
            Assert.True(emailResult.IsOk);
            var email = emailResult.ResultValue;
            
            var existingUsers = new List<User>
            {
                CreateTestUser("user1@example.com", "ユーザー1"),
                CreateTestUser("duplicate@example.com", "重複ユーザー"), // 重複するアクティブユーザー
                CreateTestUser("user2@example.com", "ユーザー2")
            };

            // Act
            var result = UserDomainService.validateUniqueEmail(email, ListModule.OfSeq(existingUsers));

            // Assert
            Assert.True(result.IsError);
            Assert.Equal("このメールアドレスは既に使用されています", result.ErrorValue);
        }

        [Fact]
        public void ValidateUniqueEmail_DuplicateEmailInactiveUser_ShouldReturnOk()
        {
            // Arrange
            var emailResult = Email.create("inactive@example.com");
            Assert.True(emailResult.IsOk);
            var email = emailResult.ResultValue;
            
            var existingUsers = new List<User>
            {
                CreateTestUser("user1@example.com", "ユーザー1"),
                CreateTestUser("inactive@example.com", "無効ユーザー", Role.GeneralUser, 2L, false), // 非アクティブユーザー
                CreateTestUser("user2@example.com", "ユーザー2")
            };

            // Act
            var result = UserDomainService.validateUniqueEmail(email, ListModule.OfSeq(existingUsers));

            // Assert
            Assert.True(result.IsOk); // 非アクティブユーザーとの重複は許可
        }

        [Fact]
        public void ValidateUniqueEmail_EmptyUserList_ShouldReturnOk()
        {
            // Arrange
            var emailResult = Email.create("unique@example.com");
            Assert.True(emailResult.IsOk);
            var email = emailResult.ResultValue;
            
            var existingUsers = new List<User>();

            // Act
            var result = UserDomainService.validateUniqueEmail(email, ListModule.OfSeq(existingUsers));

            // Assert
            Assert.True(result.IsOk);
        }

        [Fact]
        public void ValidateUniqueEmail_CaseInsensitiveCheck_ShouldReturnError()
        {
            // Arrange
            var emailResult = Email.create("User@Example.Com");
            Assert.True(emailResult.IsOk);
            var email = emailResult.ResultValue;
            
            var existingUsers = new List<User>
            {
                CreateTestUser("user@example.com", "既存ユーザー") // 小文字で登録済み
            };

            // Act
            var result = UserDomainService.validateUniqueEmail(email, ListModule.OfSeq(existingUsers));

            // Assert
            Assert.True(result.IsError);
            Assert.Equal("このメールアドレスは既に使用されています", result.ErrorValue);
        }
    }

    /// <summary>
    /// validateRoleChangeAuthorizationのテスト
    /// </summary>
    public class ValidateRoleChangeAuthorizationTests : UserDomainServiceTests
    {
        [Fact]
        public void ValidateRoleChangeAuthorization_SuperUserChangingAnyRole_ShouldReturnOk()
        {
            // Arrange
            var operatorUser = CreateTestUser("admin@example.com", "管理者", Role.SuperUser, 1L);
            var targetUser = CreateTestUser("user@example.com", "ユーザー", Role.GeneralUser, 2L);
            var newRole = Role.ProjectManager;

            // Act
            var result = UserDomainService.validateRoleChangeAuthorization(operatorUser, targetUser, newRole);

            // Assert
            Assert.True(result.IsOk);
        }

        [Fact]
        public void ValidateRoleChangeAuthorization_SuperUserChangingSuperUser_ShouldReturnOk()
        {
            // Arrange
            var operatorUser = CreateTestUser("admin1@example.com", "管理者1", Role.SuperUser, 1L);
            var targetUser = CreateTestUser("admin2@example.com", "管理者2", Role.SuperUser, 2L);
            var newRole = Role.ProjectManager;

            // Act
            var result = UserDomainService.validateRoleChangeAuthorization(operatorUser, targetUser, newRole);

            // Assert
            Assert.True(result.IsOk);
        }

        [Fact]
        public void ValidateRoleChangeAuthorization_NonSuperUserChangingSuperUser_ShouldReturnError()
        {
            // Arrange
            var operatorUser = CreateTestUser("manager@example.com", "管理者", Role.ProjectManager, 1L);
            var targetUser = CreateTestUser("admin@example.com", "スーパーユーザー", Role.SuperUser, 2L);
            var newRole = Role.ProjectManager;

            // Act
            var result = UserDomainService.validateRoleChangeAuthorization(operatorUser, targetUser, newRole);

            // Assert
            Assert.True(result.IsError);
            Assert.Equal("SuperUserのロール変更はSuperUserのみが実行できます", result.ErrorValue);
        }

        [Fact]
        public void ValidateRoleChangeAuthorization_NonSuperUserPromotingToSuperUser_ShouldReturnError()
        {
            // Arrange
            var operatorUser = CreateTestUser("manager@example.com", "管理者", Role.ProjectManager, 1L);
            var targetUser = CreateTestUser("user@example.com", "ユーザー", Role.GeneralUser, 2L);
            var newRole = Role.SuperUser;

            // Act
            var result = UserDomainService.validateRoleChangeAuthorization(operatorUser, targetUser, newRole);

            // Assert
            Assert.True(result.IsError);
            Assert.Equal("SuperUserへの昇格はSuperUserのみが実行できます", result.ErrorValue);
        }

        [Fact]
        public void ValidateRoleChangeAuthorization_ProjectManagerChangingLowerRoles_ShouldReturnOk()
        {
            // Arrange
            var operatorUser = CreateTestUser("manager@example.com", "管理者", Role.ProjectManager, 1L);
            var targetUser = CreateTestUser("user@example.com", "ユーザー", Role.GeneralUser, 2L);
            var newRole = Role.DomainApprover;

            // Act
            var result = UserDomainService.validateRoleChangeAuthorization(operatorUser, targetUser, newRole);

            // Assert
            Assert.True(result.IsOk);
        }

        [Fact]
        public void ValidateRoleChangeAuthorization_InsufficientPermission_ShouldReturnError()
        {
            // Arrange
            var operatorUser = CreateTestUser("approver@example.com", "承認者", Role.DomainApprover, 1L);
            var targetUser = CreateTestUser("user@example.com", "ユーザー", Role.GeneralUser, 2L);
            var newRole = Role.ProjectManager;

            // Act
            var result = UserDomainService.validateRoleChangeAuthorization(operatorUser, targetUser, newRole);

            // Assert
            Assert.True(result.IsError);
            Assert.Contains("権限がありません", result.ErrorValue);
        }

        [Theory]
        [InlineData(3, 0, 2, true)]     // SuperUser can change any
        [InlineData(3, 3, 2, true)]     // SuperUser can change SuperUser
        [InlineData(3, 2, 3, true)]     // SuperUser can promote to SuperUser
        [InlineData(2, 3, 0, false)]    // Cannot change SuperUser
        [InlineData(2, 0, 3, false)]    // Cannot promote to SuperUser
        [InlineData(2, 0, 2, true)]     // Can promote within scope
        [InlineData(2, 1, 0, true)]     // Can demote within scope
        [InlineData(1, 0, 2, false)]    // Cannot promote above own level
        [InlineData(0, 0, 1, false)]    // Cannot promote others
        public void ValidateRoleChangeAuthorization_VariousRoleCombinations_ShouldReturnExpectedResult(
            int operatorRoleInt, int targetCurrentRoleInt, int targetNewRoleInt, bool expectedSuccess)
        {
            // Arrange
            var operatorRole = ConvertIntToRole(operatorRoleInt);
            var targetCurrentRole = ConvertIntToRole(targetCurrentRoleInt);
            var targetNewRole = ConvertIntToRole(targetNewRoleInt);
            var operatorUser = CreateTestUser("operator@example.com", "操作者", operatorRole, 1L);
            var targetUser = CreateTestUser("target@example.com", "対象ユーザー", targetCurrentRole, 2L);

            // Act
            var result = UserDomainService.validateRoleChangeAuthorization(operatorUser, targetUser, targetNewRole);

            // Assert
            Assert.Equal(expectedSuccess, result.IsOk);
            if (!expectedSuccess)
            {
                Assert.True(result.IsError);
                Assert.NotEmpty(result.ErrorValue);
            }
        }
    }

    /// <summary>
    /// その他のUserDomainServiceメソッドのテスト（Phase A2で拡張された機能）
    /// </summary>
    public class AdditionalDomainServiceTests : UserDomainServiceTests
    {
        [Fact]
        public void UserDomainService_MultipleValidations_ShouldWorkTogether()
        {
            // Arrange
            var operatorUser = CreateTestUser("admin@example.com", "管理者", Role.SuperUser, 1L);
            var newEmailResult = Email.create("newuser@example.com");
            Assert.True(newEmailResult.IsOk);
            var newEmail = newEmailResult.ResultValue;
            
            var existingUsers = new List<User>
            {
                CreateTestUser("user1@example.com", "ユーザー1", Role.GeneralUser, 2L),
                CreateTestUser("user2@example.com", "ユーザー2", Role.GeneralUser, 3L)
            };

            // Act - ユーザー作成権限チェック
            var permissionResult = UserDomainService.validateUserCreationPermission(operatorUser, Role.GeneralUser);
            
            // Act - メールアドレス重複チェック
            var emailResult = UserDomainService.validateUniqueEmail(newEmail, ListModule.OfSeq(existingUsers));

            // Assert
            Assert.True(permissionResult.IsOk);
            Assert.True(emailResult.IsOk);
        }

        [Fact]
        public void UserDomainService_ComplexScenario_ShouldHandleBusinessRules()
        {
            // Arrange - 複雑なシナリオ：ProjectManagerが新しいDomainApproverを作成しようとする
            var operatorUser = CreateTestUser("manager@example.com", "プロジェクト管理者", Role.ProjectManager, 1L);
            var newEmailResult = Email.create("approver@example.com");
            Assert.True(newEmailResult.IsOk);
            var newEmail = newEmailResult.ResultValue;
            
            var existingUsers = new List<User>
            {
                operatorUser,
                CreateTestUser("existing@example.com", "既存ユーザー", Role.GeneralUser, 2L)
            };

            // Act
            var creationPermissionResult = UserDomainService.validateUserCreationPermission(operatorUser, Role.DomainApprover);
            var emailUniquenessResult = UserDomainService.validateUniqueEmail(newEmail, ListModule.OfSeq(existingUsers));

            // Assert
            Assert.True(creationPermissionResult.IsOk); // ProjectManagerはDomainApproverを作成可能
            Assert.True(emailUniquenessResult.IsOk);    // メールアドレスは重複していない
        }
    }
}