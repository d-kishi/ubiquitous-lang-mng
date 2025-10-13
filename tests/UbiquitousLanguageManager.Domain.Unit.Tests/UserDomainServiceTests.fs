namespace UbiquitousLanguageManager.Domain.Unit.Tests

open System
open Xunit
open UbiquitousLanguageManager.Domain.Common
open UbiquitousLanguageManager.Domain.Authentication

// 🧪 UserDomainServiceの単体テスト（C#→F#変換）
//
// 【テスト方針】
// Phase A2で新規追加されたUserDomainServiceのビジネスルール、
// 権限チェック、ドメイン固有のバリデーションロジックを検証します。

// 【F#初学者向け解説】
// F#でxUnitテストを書く際の基本パターン：
// 1. type TestClass() = でテストクラスを定義
// 2. [<Fact>] 属性で単一テストメソッド定義
// 3. [<Theory>] [<InlineData(...)>] で複数パラメータテスト定義
// 4. match式でResult型の検証（Ok/Error）
// 5. Assert.True/Assert.Equal/Assert.Containsでアサーション

type UserDomainServiceTests() =

    // 🔧 テスト用ヘルパー関数
    // 【F#初学者向け解説】
    // F#では関数をmember thisとして定義し、テストクラス内で共有します。
    // Result型のパターンマッチングで安全に値を取り出します。

    /// Role型のint表現を変換
    member this.ConvertIntToRole(roleInt: int) : Role =
        match roleInt with
        | 0 -> Role.GeneralUser
        | 1 -> Role.DomainApprover
        | 2 -> Role.ProjectManager
        | 3 -> Role.SuperUser
        | _ -> Role.GeneralUser

    /// テスト用ユーザー作成ヘルパー
    member this.CreateTestUser(email: string, name: string, role: Role, id: int64, isActive: bool) : User =
        let emailValue =
            match Email.create email with
            | Ok e -> e
            | Error _ -> failwith "Email作成失敗"

        let nameValue =
            match UserName.create name with
            | Ok n -> n
            | Error _ -> failwith "UserName作成失敗"

        let user = User.create emailValue nameValue role (UserId.create id)

        if not isActive then
            let adminUser = this.CreateTestUser("admin@example.com", "管理者", Role.SuperUser, 999L, true)
            match user.deactivate adminUser adminUser.Id with
            | Ok deactivatedUser -> deactivatedUser
            | Error _ -> failwith "ユーザー無効化失敗"
        else
            user

    /// テスト用ユーザー作成（デフォルトパラメータ）
    member this.CreateTestUserDefault(?email: string, ?name: string, ?role: Role, ?id: int64, ?isActive: bool) : User =
        let defaultEmail = defaultArg email "test@example.com"
        let defaultName = defaultArg name "テストユーザー"
        let defaultRole = defaultArg role Role.GeneralUser
        let defaultId = defaultArg id 1L
        let defaultIsActive = defaultArg isActive true
        this.CreateTestUser(defaultEmail, defaultName, defaultRole, defaultId, defaultIsActive)


// ========================================
// validateUserCreationPermissionのテスト
// ========================================
type ValidateUserCreationPermissionTests() =
    inherit UserDomainServiceTests()

    [<Fact>]
    member this.``ValidateUserCreationPermission_SuperUserCreatingGeneralUser_ShouldReturnOk``() =
        // Arrange
        let operatorUser = this.CreateTestUser("admin@example.com", "管理者", Role.SuperUser, 1L, true)
        let targetRole = Role.GeneralUser

        // Act
        let result = UserDomainService.validateUserCreationPermission operatorUser targetRole

        // Assert
        match result with
        | Ok _ -> Assert.True(true)
        | Error msg -> Assert.True(false, $"Expected Ok but got Error: {msg}")

    [<Fact>]
    member this.``ValidateUserCreationPermission_SuperUserCreatingSuperUser_ShouldReturnOk``() =
        // Arrange
        let operatorUser = this.CreateTestUser("admin@example.com", "管理者", Role.SuperUser, 1L, true)
        let targetRole = Role.SuperUser

        // Act
        let result = UserDomainService.validateUserCreationPermission operatorUser targetRole

        // Assert
        match result with
        | Ok _ -> Assert.True(true)
        | Error msg -> Assert.True(false, $"Expected Ok but got Error: {msg}")

    [<Fact>]
    member this.``ValidateUserCreationPermission_ProjectManagerCreatingGeneralUser_ShouldReturnOk``() =
        // Arrange
        let operatorUser = this.CreateTestUser("manager@example.com", "管理者", Role.ProjectManager, 1L, true)
        let targetRole = Role.GeneralUser

        // Act
        let result = UserDomainService.validateUserCreationPermission operatorUser targetRole

        // Assert
        match result with
        | Ok _ -> Assert.True(true)
        | Error msg -> Assert.True(false, $"Expected Ok but got Error: {msg}")

    [<Fact>]
    member this.``ValidateUserCreationPermission_ProjectManagerCreatingSuperUser_ShouldReturnError``() =
        // Arrange
        let operatorUser = this.CreateTestUser("manager@example.com", "管理者", Role.ProjectManager, 1L, true)
        let targetRole = Role.SuperUser

        // Act
        let result = UserDomainService.validateUserCreationPermission operatorUser targetRole

        // Assert
        match result with
        | Error msg -> Assert.Equal("SuperUserの作成はSuperUserのみが実行できます", msg)
        | Ok _ -> Assert.True(false, "Expected Error but got Ok")

    [<Fact>]
    member this.``ValidateUserCreationPermission_GeneralUserCreatingAnyUser_ShouldReturnError``() =
        // Arrange
        let operatorUser = this.CreateTestUser("user@example.com", "一般ユーザー", Role.GeneralUser, 1L, true)
        let targetRole = Role.GeneralUser

        // Act
        let result = UserDomainService.validateUserCreationPermission operatorUser targetRole

        // Assert
        match result with
        | Error msg -> Assert.Equal("ユーザー作成の権限がありません", msg)
        | Ok _ -> Assert.True(false, "Expected Error but got Ok")

    [<Fact>]
    member this.``ValidateUserCreationPermission_InactiveUserCreatingAnyUser_ShouldReturnError``() =
        // Arrange
        let operatorUser = this.CreateTestUser("inactive@example.com", "無効ユーザー", Role.SuperUser, 1L, false)
        let targetRole = Role.GeneralUser

        // Act
        let result = UserDomainService.validateUserCreationPermission operatorUser targetRole

        // Assert
        match result with
        | Error msg -> Assert.Equal("非アクティブなユーザーは新規ユーザーを作成できません", msg)
        | Ok _ -> Assert.True(false, "Expected Error but got Ok")

    [<Theory>]
    [<InlineData(3, 3, true)>]      // SuperUser can create SuperUser
    [<InlineData(3, 2, true)>]      // SuperUser can create ProjectManager
    [<InlineData(3, 1, true)>]      // SuperUser can create DomainApprover
    [<InlineData(3, 0, true)>]      // SuperUser can create GeneralUser
    [<InlineData(2, 3, false)>]     // ProjectManager cannot create SuperUser
    [<InlineData(2, 2, true)>]      // ProjectManager can create ProjectManager
    [<InlineData(2, 1, true)>]      // ProjectManager can create DomainApprover
    [<InlineData(2, 0, true)>]      // ProjectManager can create GeneralUser
    [<InlineData(1, 3, false)>]     // DomainApprover cannot create SuperUser
    [<InlineData(1, 2, false)>]     // DomainApprover cannot create ProjectManager
    [<InlineData(1, 1, false)>]     // DomainApprover cannot create users
    [<InlineData(1, 0, false)>]     // DomainApprover cannot create users
    [<InlineData(0, 3, false)>]     // GeneralUser cannot create SuperUser
    [<InlineData(0, 2, false)>]     // GeneralUser cannot create ProjectManager
    [<InlineData(0, 1, false)>]     // GeneralUser cannot create DomainApprover
    [<InlineData(0, 0, false)>]     // GeneralUser cannot create GeneralUser
    member this.``ValidateUserCreationPermission_VariousRoleCombinations_ShouldReturnExpectedResult``
        (operatorRoleInt: int, targetRoleInt: int, expectedSuccess: bool) =
        // Arrange
        let operatorRole = this.ConvertIntToRole operatorRoleInt
        let targetRole = this.ConvertIntToRole targetRoleInt
        let operatorUser = this.CreateTestUser("operator@example.com", "操作者", operatorRole, 1L, true)

        // Act
        let result = UserDomainService.validateUserCreationPermission operatorUser targetRole

        // Assert
        match result, expectedSuccess with
        | Ok _, true -> Assert.True(true)
        | Error _, false -> Assert.True(true)
        | Ok _, false -> Assert.True(false, "Expected Error but got Ok")
        | Error msg, true -> Assert.True(false, $"Expected Ok but got Error: {msg}")


// ========================================
// validateUniqueEmailのテスト
// ========================================
type ValidateUniqueEmailTests() =
    inherit UserDomainServiceTests()

    [<Fact>]
    member this.``ValidateUniqueEmail_NewUniqueEmail_ShouldReturnOk``() =
        // Arrange
        let email =
            match Email.create "newuser@example.com" with
            | Ok e -> e
            | Error _ -> failwith "Email作成失敗"

        let existingUsers =
            [
                this.CreateTestUser("user1@example.com", "ユーザー1", Role.GeneralUser, 1L, true)
                this.CreateTestUser("user2@example.com", "ユーザー2", Role.GeneralUser, 2L, true)
            ]

        // Act
        let result = UserDomainService.validateUniqueEmail email existingUsers

        // Assert
        match result with
        | Ok _ -> Assert.True(true)
        | Error msg -> Assert.True(false, $"Expected Ok but got Error: {msg}")

    [<Fact>]
    member this.``ValidateUniqueEmail_DuplicateEmailActiveUser_ShouldReturnError``() =
        // Arrange
        let email =
            match Email.create "duplicate@example.com" with
            | Ok e -> e
            | Error _ -> failwith "Email作成失敗"

        let existingUsers =
            [
                this.CreateTestUser("user1@example.com", "ユーザー1", Role.GeneralUser, 1L, true)
                this.CreateTestUser("duplicate@example.com", "重複ユーザー", Role.GeneralUser, 2L, true)
                this.CreateTestUser("user2@example.com", "ユーザー2", Role.GeneralUser, 3L, true)
            ]

        // Act
        let result = UserDomainService.validateUniqueEmail email existingUsers

        // Assert
        match result with
        | Error msg -> Assert.Equal("このメールアドレスは既に使用されています", msg)
        | Ok _ -> Assert.True(false, "Expected Error but got Ok")

    [<Fact>]
    member this.``ValidateUniqueEmail_DuplicateEmailInactiveUser_ShouldReturnOk``() =
        // Arrange
        let email =
            match Email.create "inactive@example.com" with
            | Ok e -> e
            | Error _ -> failwith "Email作成失敗"

        let existingUsers =
            [
                this.CreateTestUser("user1@example.com", "ユーザー1", Role.GeneralUser, 1L, true)
                this.CreateTestUser("inactive@example.com", "無効ユーザー", Role.GeneralUser, 2L, false)
                this.CreateTestUser("user2@example.com", "ユーザー2", Role.GeneralUser, 3L, true)
            ]

        // Act
        let result = UserDomainService.validateUniqueEmail email existingUsers

        // Assert
        match result with
        | Ok _ -> Assert.True(true) // 非アクティブユーザーとの重複は許可
        | Error msg -> Assert.True(false, $"Expected Ok but got Error: {msg}")

    [<Fact>]
    member this.``ValidateUniqueEmail_EmptyUserList_ShouldReturnOk``() =
        // Arrange
        let email =
            match Email.create "unique@example.com" with
            | Ok e -> e
            | Error _ -> failwith "Email作成失敗"

        let existingUsers = []

        // Act
        let result = UserDomainService.validateUniqueEmail email existingUsers

        // Assert
        match result with
        | Ok _ -> Assert.True(true)
        | Error msg -> Assert.True(false, $"Expected Ok but got Error: {msg}")

    [<Fact>]
    member this.``ValidateUniqueEmail_CaseInsensitiveCheck_ShouldReturnError``() =
        // Arrange
        let email =
            match Email.create "User@Example.Com" with
            | Ok e -> e
            | Error _ -> failwith "Email作成失敗"

        let existingUsers =
            [
                this.CreateTestUser("user@example.com", "既存ユーザー", Role.GeneralUser, 1L, true)
            ]

        // Act
        let result = UserDomainService.validateUniqueEmail email existingUsers

        // Assert
        match result with
        | Error msg -> Assert.Equal("このメールアドレスは既に使用されています", msg)
        | Ok _ -> Assert.True(false, "Expected Error but got Ok")


// ========================================
// validateRoleChangeAuthorizationのテスト
// ========================================
type ValidateRoleChangeAuthorizationTests() =
    inherit UserDomainServiceTests()

    [<Fact>]
    member this.``ValidateRoleChangeAuthorization_SuperUserChangingAnyRole_ShouldReturnOk``() =
        // Arrange
        let operatorUser = this.CreateTestUser("admin@example.com", "管理者", Role.SuperUser, 1L, true)
        let targetUser = this.CreateTestUser("user@example.com", "ユーザー", Role.GeneralUser, 2L, true)
        let newRole = Role.ProjectManager

        // Act
        let result = UserDomainService.validateRoleChangeAuthorization operatorUser targetUser newRole

        // Assert
        match result with
        | Ok _ -> Assert.True(true)
        | Error msg -> Assert.True(false, $"Expected Ok but got Error: {msg}")

    [<Fact>]
    member this.``ValidateRoleChangeAuthorization_SuperUserChangingSuperUser_ShouldReturnOk``() =
        // Arrange
        let operatorUser = this.CreateTestUser("admin1@example.com", "管理者1", Role.SuperUser, 1L, true)
        let targetUser = this.CreateTestUser("admin2@example.com", "管理者2", Role.SuperUser, 2L, true)
        let newRole = Role.ProjectManager

        // Act
        let result = UserDomainService.validateRoleChangeAuthorization operatorUser targetUser newRole

        // Assert
        match result with
        | Ok _ -> Assert.True(true)
        | Error msg -> Assert.True(false, $"Expected Ok but got Error: {msg}")

    [<Fact>]
    member this.``ValidateRoleChangeAuthorization_NonSuperUserChangingSuperUser_ShouldReturnError``() =
        // Arrange
        let operatorUser = this.CreateTestUser("manager@example.com", "管理者", Role.ProjectManager, 1L, true)
        let targetUser = this.CreateTestUser("admin@example.com", "スーパーユーザー", Role.SuperUser, 2L, true)
        let newRole = Role.ProjectManager

        // Act
        let result = UserDomainService.validateRoleChangeAuthorization operatorUser targetUser newRole

        // Assert
        match result with
        | Error msg -> Assert.Equal("SuperUserのロール変更はSuperUserのみが実行できます", msg)
        | Ok _ -> Assert.True(false, "Expected Error but got Ok")

    [<Fact>]
    member this.``ValidateRoleChangeAuthorization_NonSuperUserPromotingToSuperUser_ShouldReturnError``() =
        // Arrange
        let operatorUser = this.CreateTestUser("manager@example.com", "管理者", Role.ProjectManager, 1L, true)
        let targetUser = this.CreateTestUser("user@example.com", "ユーザー", Role.GeneralUser, 2L, true)
        let newRole = Role.SuperUser

        // Act
        let result = UserDomainService.validateRoleChangeAuthorization operatorUser targetUser newRole

        // Assert
        match result with
        | Error msg -> Assert.Equal("SuperUserへの昇格はSuperUserのみが実行できます", msg)
        | Ok _ -> Assert.True(false, "Expected Error but got Ok")

    [<Fact>]
    member this.``ValidateRoleChangeAuthorization_ProjectManagerChangingLowerRoles_ShouldReturnOk``() =
        // Arrange
        let operatorUser = this.CreateTestUser("manager@example.com", "管理者", Role.ProjectManager, 1L, true)
        let targetUser = this.CreateTestUser("user@example.com", "ユーザー", Role.GeneralUser, 2L, true)
        let newRole = Role.DomainApprover

        // Act
        let result = UserDomainService.validateRoleChangeAuthorization operatorUser targetUser newRole

        // Assert
        match result with
        | Ok _ -> Assert.True(true)
        | Error msg -> Assert.True(false, $"Expected Ok but got Error: {msg}")

    [<Fact>]
    member this.``ValidateRoleChangeAuthorization_InsufficientPermission_ShouldReturnError``() =
        // Arrange
        let operatorUser = this.CreateTestUser("approver@example.com", "承認者", Role.DomainApprover, 1L, true)
        let targetUser = this.CreateTestUser("user@example.com", "ユーザー", Role.GeneralUser, 2L, true)
        let newRole = Role.ProjectManager

        // Act
        let result = UserDomainService.validateRoleChangeAuthorization operatorUser targetUser newRole

        // Assert
        match result with
        | Error msg -> Assert.Contains("権限がありません", msg)
        | Ok _ -> Assert.True(false, "Expected Error but got Ok")

    [<Theory>]
    [<InlineData(3, 0, 2, true)>]     // SuperUser can change any
    [<InlineData(3, 3, 2, true)>]     // SuperUser can change SuperUser
    [<InlineData(3, 2, 3, true)>]     // SuperUser can promote to SuperUser
    [<InlineData(2, 3, 0, false)>]    // Cannot change SuperUser
    [<InlineData(2, 0, 3, false)>]    // Cannot promote to SuperUser
    [<InlineData(2, 0, 2, true)>]     // Can promote within scope
    [<InlineData(2, 1, 0, true)>]     // Can demote within scope
    [<InlineData(1, 0, 2, false)>]    // Cannot promote above own level
    [<InlineData(0, 0, 1, false)>]    // Cannot promote others
    member this.``ValidateRoleChangeAuthorization_VariousRoleCombinations_ShouldReturnExpectedResult``
        (operatorRoleInt: int, targetCurrentRoleInt: int, targetNewRoleInt: int, expectedSuccess: bool) =
        // Arrange
        let operatorRole = this.ConvertIntToRole operatorRoleInt
        let targetCurrentRole = this.ConvertIntToRole targetCurrentRoleInt
        let targetNewRole = this.ConvertIntToRole targetNewRoleInt
        let operatorUser = this.CreateTestUser("operator@example.com", "操作者", operatorRole, 1L, true)
        let targetUser = this.CreateTestUser("target@example.com", "対象ユーザー", targetCurrentRole, 2L, true)

        // Act
        let result = UserDomainService.validateRoleChangeAuthorization operatorUser targetUser targetNewRole

        // Assert
        match result, expectedSuccess with
        | Ok _, true -> Assert.True(true)
        | Error _, false -> Assert.True(true)
        | Ok _, false -> Assert.True(false, "Expected Error but got Ok")
        | Error msg, true -> Assert.True(false, $"Expected Ok but got Error: {msg}")


// ========================================
// その他のUserDomainServiceメソッドのテスト
// ========================================
type AdditionalDomainServiceTests() =
    inherit UserDomainServiceTests()

    [<Fact>]
    member this.``UserDomainService_MultipleValidations_ShouldWorkTogether``() =
        // Arrange
        let operatorUser = this.CreateTestUser("admin@example.com", "管理者", Role.SuperUser, 1L, true)
        let newEmail =
            match Email.create "newuser@example.com" with
            | Ok e -> e
            | Error _ -> failwith "Email作成失敗"

        let existingUsers =
            [
                this.CreateTestUser("user1@example.com", "ユーザー1", Role.GeneralUser, 2L, true)
                this.CreateTestUser("user2@example.com", "ユーザー2", Role.GeneralUser, 3L, true)
            ]

        // Act - ユーザー作成権限チェック
        let permissionResult = UserDomainService.validateUserCreationPermission operatorUser Role.GeneralUser

        // Act - メールアドレス重複チェック
        let emailResult = UserDomainService.validateUniqueEmail newEmail existingUsers

        // Assert
        match permissionResult with
        | Ok _ -> Assert.True(true)
        | Error msg -> Assert.True(false, $"Permission check failed: {msg}")

        match emailResult with
        | Ok _ -> Assert.True(true)
        | Error msg -> Assert.True(false, $"Email uniqueness check failed: {msg}")

    [<Fact>]
    member this.``UserDomainService_ComplexScenario_ShouldHandleBusinessRules``() =
        // Arrange - 複雑なシナリオ：ProjectManagerが新しいDomainApproverを作成しようとする
        let operatorUser = this.CreateTestUser("manager@example.com", "プロジェクト管理者", Role.ProjectManager, 1L, true)
        let newEmail =
            match Email.create "approver@example.com" with
            | Ok e -> e
            | Error _ -> failwith "Email作成失敗"

        let existingUsers =
            [
                operatorUser
                this.CreateTestUser("existing@example.com", "既存ユーザー", Role.GeneralUser, 2L, true)
            ]

        // Act
        let creationPermissionResult = UserDomainService.validateUserCreationPermission operatorUser Role.DomainApprover
        let emailUniquenessResult = UserDomainService.validateUniqueEmail newEmail existingUsers

        // Assert
        match creationPermissionResult with
        | Ok _ -> Assert.True(true) // ProjectManagerはDomainApproverを作成可能
        | Error msg -> Assert.True(false, $"Creation permission failed: {msg}")

        match emailUniquenessResult with
        | Ok _ -> Assert.True(true) // メールアドレスは重複していない
        | Error msg -> Assert.True(false, $"Email uniqueness failed: {msg}")
