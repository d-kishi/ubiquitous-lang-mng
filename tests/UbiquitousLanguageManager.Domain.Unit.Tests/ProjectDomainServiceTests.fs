namespace UbiquitousLanguageManager.Domain.Unit.Tests

open System
open Xunit
open UbiquitousLanguageManager.Domain.Common
open UbiquitousLanguageManager.Domain.ProjectManagement
open UbiquitousLanguageManager.Domain.ProjectManagement.ProjectDomainService

// 🧪 F# Domain層ProjectDomainServiceテスト（Phase B1 TDD実装）
// Railway-oriented Programming・Result型・ドメインサービスのテスト

// 📝 【F#初学者向け解説】
// ドメインサービスのテストでは、複数のエンティティにまたがる
// 複雑なビジネスロジックを検証します。エラーケースの網羅的テストにより、
// 型安全性とビジネスルール遵守を確認します。

type ProjectDomainServiceTests() =

    // 🔧 テスト用データ作成ヘルパー
    // 【F#初学者向け解説】
    // F#では|> function パターンでResult型から値を取り出します。
    // failwithにより、テストデータ作成に失敗した場合は即座にテストを中断します。
    let createValidProjectName name =
        ProjectName.create name
        |> function
           | Ok projectName -> projectName
           | Error _ -> failwith $"テストデータエラー: {name}"

    let createValidProjectDescription desc =
        ProjectDescription.create desc
        |> function
           | Ok projectDesc -> projectDesc
           | Error _ -> failwith "テストデータエラー"

    let createTestUserId (id: int) =
        UserId.create (sprintf "00000000-0000-0000-0000-%012d" id)

    let createExistingProject name description ownerId isActive =
        let projectName = createValidProjectName name
        let projectDesc = createValidProjectDescription description
        {
            Id = ProjectId (System.Guid.NewGuid().GetHashCode() |> int64 |> abs)
            Name = projectName
            Description = projectDesc
            OwnerId = ownerId
            CreatedAt = DateTime.UtcNow.AddDays(-1.0)  // 1日前に作成
            UpdatedAt = None
            IsActive = isActive
        }

    // ✅ createProjectWithDefaultDomain テスト（正常系）
    [<Fact>]
    member this.``createProjectWithDefaultDomain_ValidInput_ReturnsProjectAndDomain``() =
        // Arrange
        let name = createValidProjectName "Test Project"
        let description = createValidProjectDescription (Some "Test Description")
        let ownerId = createTestUserId 1
        let existingProjects = []  // 重複なし

        // Act
        let result = createProjectWithDefaultDomain name description ownerId existingProjects

        // Assert
        // 【F#初学者向け解説】
        // タプル型 (Project * Domain) の分解でプロジェクトとドメインを検証します。
        // パターンマッチングにより、Result型の内容を安全に取得できます。
        match result with
        | Ok (project, domain) ->
            // Project検証
            Assert.Equal(name, project.Name)
            Assert.Equal(description, project.Description)
            Assert.Equal(ownerId, project.OwnerId)
            Assert.True(project.IsActive)

            // Domain検証
            Assert.Equal(project.Id, domain.ProjectId)
            Assert.Equal(ownerId, domain.OwnerId)
            Assert.True(domain.IsDefault)
            Assert.Equal("Test Project_Default", domain.Name.Value)
        | Error err ->
            Assert.True(false, $"Expected Ok but got Error: {err}")

    // ❌ createProjectWithDefaultDomain テスト（異常系：重複名）
    [<Fact>]
    member this.``createProjectWithDefaultDomain_DuplicateName_ReturnsDuplicateError``() =
        // Arrange
        let name = createValidProjectName "Duplicate Project"
        let description = createValidProjectDescription None
        let ownerId = createTestUserId 1

        // 既存プロジェクト（同名・アクティブ）
        let existingProject = createExistingProject "Duplicate Project" None ownerId true
        let existingProjects = [existingProject]

        // Act
        let result = createProjectWithDefaultDomain name description ownerId existingProjects

        // Assert
        match result with
        | Error (ProjectCreationError.DuplicateProjectName msg) ->
            Assert.Equal("指定されたプロジェクト名は既に使用されています", msg)
        | Ok _ ->
            Assert.True(false, "Expected DuplicateProjectName error but got Ok")
        | Error other ->
            Assert.True(false, $"Expected DuplicateProjectName but got: {other}")

    // ✅ createProjectWithDefaultDomain テスト（正常系：非アクティブ重複は許可）
    [<Fact>]
    member this.``createProjectWithDefaultDomain_DuplicateNameButInactive_ReturnsOk``() =
        // Arrange
        let name = createValidProjectName "Inactive Project"
        let description = createValidProjectDescription None
        let ownerId = createTestUserId 1

        // 既存プロジェクト（同名だが非アクティブ）
        let inactiveProject = createExistingProject "Inactive Project" None ownerId false
        let existingProjects = [inactiveProject]

        // Act
        let result = createProjectWithDefaultDomain name description ownerId existingProjects

        // Assert
        match result with
        | Ok (project, domain) ->
            Assert.Equal(name, project.Name)
            Assert.True(domain.IsDefault)
        | Error err ->
            Assert.True(false, $"Expected Ok but got Error: {err}")

    // ✅ createProjectWithDefaultDomain テスト（正常系：複数既存プロジェクト）
    [<Fact>]
    member this.``createProjectWithDefaultDomain_MultipleExistingProjects_ChecksAllForDuplicates``() =
        // Arrange
        let name = createValidProjectName "Test Project"
        let description = createValidProjectDescription None
        let ownerId = createTestUserId 1

        // 複数の既存プロジェクト（重複なし）
        let project1 = createExistingProject "Project 1" None ownerId true
        let project2 = createExistingProject "Project 2" None ownerId true
        let existingProjects = [project1; project2]

        // Act
        let result = createProjectWithDefaultDomain name description ownerId existingProjects

        // Assert
        match result with
        | Ok (project, domain) ->
            Assert.Equal(name, project.Name)
            Assert.Equal(project.Id, domain.ProjectId)
        | Error err ->
            Assert.True(false, $"Expected Ok but got Error: {err}")

    // ✅ createProjectWithDefaultDomainPipeline テスト（Railway-oriented Programming）
    [<Fact>]
    member this.``createProjectWithDefaultDomainPipeline_ValidInput_ReturnsProjectAndDomain``() =
        // Arrange
        let name = createValidProjectName "Pipeline Test Project"
        let description = createValidProjectDescription (Some "Pipeline test")
        let ownerId = createTestUserId 2
        let existingProjects = []

        // Act
        let result = createProjectWithDefaultDomainPipeline name description ownerId existingProjects

        // Assert
        match result with
        | Ok (project, domain) ->
            Assert.Equal(name, project.Name)
            Assert.Equal("Pipeline Test Project_Default", domain.Name.Value)
            Assert.True(domain.IsDefault)
        | Error err ->
            Assert.True(false, $"Expected Ok but got Error: {err}")

    // ❌ createProjectWithDefaultDomainPipeline テスト（Railway-oriented Programming エラー伝播）
    [<Fact>]
    member this.``createProjectWithDefaultDomainPipeline_DuplicateName_PropagatesError``() =
        // Arrange
        let name = createValidProjectName "Pipeline Duplicate"
        let description = createValidProjectDescription None
        let ownerId = createTestUserId 2

        let existingProject = createExistingProject "Pipeline Duplicate" None ownerId true
        let existingProjects = [existingProject]

        // Act
        let result = createProjectWithDefaultDomainPipeline name description ownerId existingProjects

        // Assert
        // 【F#初学者向け解説】
        // Railway-oriented Programmingでは、エラーが自動的に伝播され、
        // 後続の処理は実行されません。この動作をテストで確認します。
        match result with
        | Error (ProjectCreationError.DuplicateProjectName _) ->
            Assert.True(true)  // 期待通りのエラー
        | Ok _ ->
            Assert.True(false, "Expected error propagation but got Ok")
        | Error other ->
            Assert.True(false, $"Expected DuplicateProjectName but got: {other}")

    // 🔍 validateUniqueProjectName テスト（正常系）
    [<Fact>]
    member this.``validateUniqueProjectName_UniqueName_ReturnsOk``() =
        // Arrange
        let name = createValidProjectName "Unique Project"
        let existingProjects = [
            createExistingProject "Existing Project 1" None (createTestUserId 1) true
            createExistingProject "Existing Project 2" None (createTestUserId 2) true
        ]

        // Act
        let result = validateUniqueProjectName name existingProjects

        // Assert
        match result with
        | Ok () ->
            Assert.True(true)  // 期待通りの成功
        | Error err ->
            Assert.True(false, $"Expected Ok but got Error: {err}")

    // ❌ validateUniqueProjectName テスト（異常系：重複名）
    [<Fact>]
    member this.``validateUniqueProjectName_DuplicateName_ReturnsError``() =
        // Arrange
        let name = createValidProjectName "Existing Project"
        let existingProjects = [
            createExistingProject "Existing Project" None (createTestUserId 1) true
        ]

        // Act
        let result = validateUniqueProjectName name existingProjects

        // Assert
        match result with
        | Error (ProjectCreationError.DuplicateProjectName msg) ->
            Assert.Equal("指定されたプロジェクト名は既に使用されています", msg)
        | Ok () ->
            Assert.True(false, "Expected DuplicateProjectName error but got Ok")
        | Error other ->
            Assert.True(false, $"Expected DuplicateProjectName but got: {other}")

    // ✅ validateUniqueProjectName テスト（正常系：大文字小文字の違いは重複扱い）
    [<Fact>]
    member this.``validateUniqueProjectName_CaseInsensitiveDuplicate_ReturnsError``() =
        // Arrange
        let name = createValidProjectName "existing project"  // 小文字
        let existingProjects = [
            createExistingProject "Existing Project" None (createTestUserId 1) true  // 大文字
        ]

        // Act
        let result = validateUniqueProjectName name existingProjects

        // Assert
        // 【F#初学者向け解説】
        // StringComparison.OrdinalIgnoreCaseにより、大文字小文字を区別しない
        // 比較が行われることをテストで確認します。
        match result with
        | Error (ProjectCreationError.DuplicateProjectName _) ->
            Assert.True(true)  // 期待通りのエラー
        | Ok () ->
            Assert.True(false, "Expected case-insensitive duplicate error but got Ok")
        | Error other ->
            Assert.True(false, $"Expected DuplicateProjectName but got: {other}")

    // ✅ validateUniqueProjectName テスト（正常系：非アクティブプロジェクトは重複チェック対象外）
    [<Fact>]
    member this.``validateUniqueProjectName_InactiveProjectSameName_ReturnsOk``() =
        // Arrange
        let name = createValidProjectName "Inactive Project Name"
        let existingProjects = [
            createExistingProject "Inactive Project Name" None (createTestUserId 1) false  // 非アクティブ
        ]

        // Act
        let result = validateUniqueProjectName name existingProjects

        // Assert
        match result with
        | Ok () ->
            Assert.True(true)  // 非アクティブなので重複扱いしない
        | Error err ->
            Assert.True(false, $"Expected Ok for inactive project but got Error: {err}")