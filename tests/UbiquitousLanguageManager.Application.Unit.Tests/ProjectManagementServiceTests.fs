namespace UbiquitousLanguageManager.Application.Unit.Tests

open System
open System.Threading.Tasks
open Xunit
open Moq
open Microsoft.FSharp.Core
open Microsoft.FSharp.Collections

// F# Domain層namespace階層化対応
open UbiquitousLanguageManager.Domain.Common
open UbiquitousLanguageManager.Domain.Authentication
open UbiquitousLanguageManager.Domain.ProjectManagement

// Application層参照
open UbiquitousLanguageManager.Application.ProjectManagement

/// <summary>
/// ProjectManagementService Phase B2 単体テスト
///
/// 【Phase B2: UserProjects多対多関連管理テスト】
/// - AddMemberToProjectAsync: プロジェクトメンバー追加（権限制御・重複チェック）
/// - RemoveMemberFromProjectAsync: プロジェクトメンバー削除（最後の管理者削除防止）
/// - GetProjectMembersAsync: プロジェクトメンバー一覧取得（権限制御マトリックス）
/// - IsUserProjectMemberAsync: プロジェクトメンバー判定
/// - GetProjectDetailAsync修正: UserCount実装
///
/// 【F#初学者向け解説】
/// このテストクラスは、F#のxUnit.netテストフレームワークを使用した単体テストです。
/// - [<Fact>]: 単一テストケース定義
/// - [<Theory>]: パラメータ化テスト定義
/// - task { ... }: タスクコンピュテーション式（非同期テスト）
/// - Mock<T>: Moqライブラリによるモック作成
/// </summary>
type ProjectManagementServiceTests() =

    let mutable mockProjectRepository = Mock<IProjectRepository>()
    let mutable mockDomainRepository = Mock<IDomainRepository>()
    let mutable mockUserRepository = Mock<IUserRepository>()

    /// <summary>
    /// テスト用Project作成ヘルパー
    /// </summary>
    member private _.CreateTestProject(projectId: int64, ownerId: int64) : Project =
        let projectName =
            match ProjectName.create("テストプロジェクト") with
            | Ok name -> name
            | Error err -> failwith $"ProjectName作成失敗: {err}"
        let description =
            match ProjectDescription.create(Some("テスト用プロジェクト説明")) with
            | Ok desc -> desc
            | Error err -> failwith $"ProjectDescription作成失敗: {err}"
        Project.createWithId (ProjectId(projectId)) projectName description (UserId(ownerId))

    /// <summary>
    /// テスト用User作成ヘルパー
    /// </summary>
    member private _.CreateTestUser(userId: int64, role: Role) : User =
        let email =
            match Email.create("test@example.com") with
            | Ok e -> e
            | Error err -> failwith $"Email作成失敗: {err}"
        let userName =
            match UserName.create("テストユーザー") with
            | Ok name -> name
            | Error err -> failwith $"UserName作成失敗: {err}"
        User.createWithId email userName role (UserId(userId))

    /// <summary>
    /// テスト用Service作成ヘルパー（Interface型で返却）
    /// 【F#初学者向け解説】
    /// F#では明示的なインターフェース実装を使用する場合、インターフェースメソッドを呼び出すには
    /// キャスト演算子 `:>` でインターフェース型にアップキャストする必要があります。
    /// ただし、task{}ブロック内ではキャスト演算子が使用できないため、
    /// ヘルパーメソッドで事前にキャストして返却します。
    /// </summary>
    member private _.CreateService() : IProjectManagementService =
        ProjectManagementService(
            mockProjectRepository.Object,
            mockDomainRepository.Object,
            mockUserRepository.Object
        ) :> IProjectManagementService

    // =================================================================
    // ✅ AddMemberToProjectAsyncテスト（3件）
    // =================================================================

    /// <summary>
    /// AddMemberToProjectAsync正常系テスト（SuperUser権限）
    /// </summary>
    [<Fact>]
    member this.``AddMemberToProjectAsync_正常系_SuperUser権限_成功を返すべき``() =
        task {
            // Arrange
            let project = this.CreateTestProject(1L, 1L)
            let user = this.CreateTestUser(2L, Role.GeneralUser)

            // Mock設定
            mockProjectRepository <- Mock<IProjectRepository>()
            mockUserRepository <- Mock<IUserRepository>()

            mockProjectRepository
                .Setup(fun x -> x.GetByIdAsync(It.IsAny<ProjectId>()))
                .ReturnsAsync(Ok (Some project))
                |> ignore

            mockUserRepository
                .Setup(fun x -> x.GetByIdAsync(It.IsAny<UserId>()))
                .ReturnsAsync(Ok (Some user))
                |> ignore

            mockProjectRepository
                .Setup(fun x -> x.AddUserToProjectAsync(It.IsAny<UserId>(), It.IsAny<ProjectId>(), It.IsAny<UserId>()))
                .ReturnsAsync(Ok ())
                |> ignore

            let service = this.CreateService()

            let command: AddMemberToProjectCommand = {
                ProjectId = Guid.NewGuid()
                UserId = Guid.NewGuid()
                OperatorUserId = Guid.NewGuid()
                OperatorRole = Role.SuperUser
            }

            // Act
            let! result = service.AddMemberToProjectAsync(command)

            // Assert
            match result with
            | Ok _ ->
                Assert.True(true, "成功を期待")
            | Error msg ->
                Assert.Fail($"エラーが返されました: {msg}")
        }

    /// <summary>
    /// AddMemberToProjectAsync重複エラーテスト
    /// </summary>
    [<Fact>]
    member this.``AddMemberToProjectAsync_重複エラー_既存メンバー追加時エラーを返すべき``() =
        task {
            // Arrange
            let project = this.CreateTestProject(1L, 1L)
            let user = this.CreateTestUser(2L, Role.GeneralUser)

            // Mock設定
            mockProjectRepository <- Mock<IProjectRepository>()
            mockUserRepository <- Mock<IUserRepository>()

            mockProjectRepository
                .Setup(fun x -> x.GetByIdAsync(It.IsAny<ProjectId>()))
                .ReturnsAsync(Ok (Some project))
                |> ignore

            mockUserRepository
                .Setup(fun x -> x.GetByIdAsync(It.IsAny<UserId>()))
                .ReturnsAsync(Ok (Some user))
                |> ignore

            mockProjectRepository
                .Setup(fun x -> x.AddUserToProjectAsync(It.IsAny<UserId>(), It.IsAny<ProjectId>(), It.IsAny<UserId>()))
                .ReturnsAsync(Error "このユーザーは既にプロジェクトのメンバーです")
                |> ignore

            let service = this.CreateService()

            let command: AddMemberToProjectCommand = {
                ProjectId = Guid.NewGuid()
                UserId = Guid.NewGuid()
                OperatorUserId = Guid.NewGuid()
                OperatorRole = Role.SuperUser
            }

            // Act
            let! result = service.AddMemberToProjectAsync(command)

            // Assert
            match result with
            | Ok _ ->
                Assert.Fail("エラーを期待しましたが成功しました")
            | Error msg ->
                Assert.Contains("既にプロジェクトのメンバーです", msg)
        }

    /// <summary>
    /// AddMemberToProjectAsync権限エラーテスト（DomainApprover権限）
    /// </summary>
    [<Fact>]
    member this.``AddMemberToProjectAsync_権限エラー_DomainApprover権限時エラーを返すべき``() =
        task {
            // Arrange
            let project = this.CreateTestProject(1L, 1L)

            // Mock設定
            mockProjectRepository <- Mock<IProjectRepository>()

            mockProjectRepository
                .Setup(fun x -> x.GetByIdAsync(It.IsAny<ProjectId>()))
                .ReturnsAsync(Ok (Some project))
                |> ignore

            let service = this.CreateService()

            let command: AddMemberToProjectCommand = {
                ProjectId = Guid.NewGuid()
                UserId = Guid.NewGuid()
                OperatorUserId = Guid.NewGuid()
                OperatorRole = Role.DomainApprover
            }

            // Act
            let! result = service.AddMemberToProjectAsync(command)

            // Assert
            match result with
            | Ok _ ->
                Assert.Fail("エラーを期待しましたが成功しました")
            | Error msg ->
                Assert.Contains("権限がありません", msg)
        }

    // =================================================================
    // ✅ RemoveMemberFromProjectAsyncテスト（2件）
    // =================================================================

    /// <summary>
    /// RemoveMemberFromProjectAsync正常系テスト
    /// </summary>
    [<Fact>]
    member this.``RemoveMemberFromProjectAsync_正常系_成功を返すべき``() =
        task {
            // Arrange
            let project = this.CreateTestProject(1L, 1L)
            let user = this.CreateTestUser(2L, Role.GeneralUser)

            // Mock設定
            mockProjectRepository <- Mock<IProjectRepository>()
            mockUserRepository <- Mock<IUserRepository>()

            mockProjectRepository
                .Setup(fun x -> x.GetByIdAsync(It.IsAny<ProjectId>()))
                .ReturnsAsync(Ok (Some project))
                |> ignore

            mockUserRepository
                .Setup(fun x -> x.GetByIdAsync(It.IsAny<UserId>()))
                .ReturnsAsync(Ok (Some user))
                |> ignore

            mockProjectRepository
                .Setup(fun x -> x.RemoveUserFromProjectAsync(It.IsAny<UserId>(), It.IsAny<ProjectId>()))
                .ReturnsAsync(Ok ())
                |> ignore

            let service = this.CreateService()

            let command: RemoveMemberFromProjectCommand = {
                ProjectId = Guid.NewGuid()
                UserId = Guid.NewGuid()
                OperatorUserId = Guid.NewGuid()
                OperatorRole = Role.SuperUser
            }

            // Act
            let! result = service.RemoveMemberFromProjectAsync(command)

            // Assert
            match result with
            | Ok _ ->
                Assert.True(true, "成功を期待")
            | Error msg ->
                Assert.Fail($"エラーが返されました: {msg}")
        }

    /// <summary>
    /// RemoveMemberFromProjectAsync最後の管理者削除エラーテスト
    /// </summary>
    [<Fact>]
    member this.``RemoveMemberFromProjectAsync_最後の管理者削除エラー_エラーを返すべき``() =
        task {
            // Arrange
            let project = this.CreateTestProject(1L, 1L)
            let user = this.CreateTestUser(2L, Role.ProjectManager)

            // Mock設定: 最後のProjectManager（メンバー1名のみ）
            mockProjectRepository <- Mock<IProjectRepository>()
            mockUserRepository <- Mock<IUserRepository>()

            mockProjectRepository
                .Setup(fun x -> x.GetByIdAsync(It.IsAny<ProjectId>()))
                .ReturnsAsync(Ok (Some project))
                |> ignore

            mockUserRepository
                .Setup(fun x -> x.GetByIdAsync(It.IsAny<UserId>()))
                .ReturnsAsync(Ok (Some user))
                |> ignore

            // メンバー一覧取得（1名のみ）
            mockProjectRepository
                .Setup(fun x -> x.GetProjectMembersAsync(It.IsAny<ProjectId>()))
                .ReturnsAsync(Ok [UserId(2L)])
                |> ignore

            let service = this.CreateService()

            let command: RemoveMemberFromProjectCommand = {
                ProjectId = Guid.NewGuid()
                UserId = Guid.NewGuid()
                OperatorUserId = Guid.NewGuid()
                OperatorRole = Role.SuperUser
            }

            // Act
            let! result = service.RemoveMemberFromProjectAsync(command)

            // Assert
            match result with
            | Ok _ ->
                Assert.Fail("エラーを期待しましたが成功しました")
            | Error msg ->
                Assert.Contains("最低1名のプロジェクト管理者が必要です", msg)
        }

    // =================================================================
    // ✅ GetProjectMembersAsyncテスト（2件）
    // =================================================================

    /// <summary>
    /// GetProjectMembersAsync SuperUser権限テスト（全メンバー取得）
    /// </summary>
    [<Fact>]
    member this.``GetProjectMembersAsync_SuperUser権限_全メンバー取得成功``() =
        task {
            // Arrange
            let project = this.CreateTestProject(1L, 1L)

            // Mock設定
            mockProjectRepository <- Mock<IProjectRepository>()

            mockProjectRepository
                .Setup(fun x -> x.GetByIdAsync(It.IsAny<ProjectId>()))
                .ReturnsAsync(Ok (Some project))
                |> ignore

            mockProjectRepository
                .Setup(fun x -> x.IsUserProjectMemberAsync(It.IsAny<UserId>(), It.IsAny<ProjectId>()))
                .ReturnsAsync(Ok false)
                |> ignore

            let memberList = [
                UserId(1L)
                UserId(2L)
                UserId(3L)
            ]

            mockProjectRepository
                .Setup(fun x -> x.GetProjectMembersAsync(It.IsAny<ProjectId>()))
                .ReturnsAsync(Ok memberList)
                |> ignore

            let service = this.CreateService()

            let query: GetProjectMembersQuery = {
                ProjectId = Guid.NewGuid()
                UserId = Guid.NewGuid()
                UserRole = Role.SuperUser
            }

            // Act
            let! result = service.GetProjectMembersAsync(query)

            // Assert
            match result with
            | Ok members ->
                Assert.Equal(3, List.length members)
            | Error msg ->
                Assert.Fail($"エラーが返されました: {msg}")
        }

    /// <summary>
    /// GetProjectMembersAsync DomainApprover権限非メンバーテスト
    /// </summary>
    [<Fact>]
    member this.``GetProjectMembersAsync_DomainApprover権限非メンバー_エラーを返すべき``() =
        task {
            // Arrange
            let project = this.CreateTestProject(1L, 1L)

            // Mock設定: 非メンバー
            mockProjectRepository <- Mock<IProjectRepository>()

            mockProjectRepository
                .Setup(fun x -> x.GetByIdAsync(It.IsAny<ProjectId>()))
                .ReturnsAsync(Ok (Some project))
                |> ignore

            mockProjectRepository
                .Setup(fun x -> x.IsUserProjectMemberAsync(It.IsAny<UserId>(), It.IsAny<ProjectId>()))
                .ReturnsAsync(Ok false)
                |> ignore

            let service = this.CreateService()

            let query: GetProjectMembersQuery = {
                ProjectId = Guid.NewGuid()
                UserId = Guid.NewGuid()
                UserRole = Role.DomainApprover
            }

            // Act
            let! result = service.GetProjectMembersAsync(query)

            // Assert
            match result with
            | Ok _ ->
                Assert.Fail("エラーを期待しましたが成功しました")
            | Error msg ->
                Assert.Contains("権限がありません", msg)
        }

    // =================================================================
    // ✅ IsUserProjectMemberAsyncテスト（1件）
    // =================================================================

    /// <summary>
    /// IsUserProjectMemberAsyncメンバー判定テスト
    /// </summary>
    [<Fact>]
    member _.``IsUserProjectMemberAsync_メンバー判定_正常動作``() =
        task {
            // Arrange
            let projectId = ProjectId(1L)
            let userId = UserId(2L)

            // Mock設定
            mockProjectRepository <- Mock<IProjectRepository>()

            mockProjectRepository
                .Setup(fun x -> x.IsUserProjectMemberAsync(It.IsAny<UserId>(), It.IsAny<ProjectId>()))
                .ReturnsAsync(Ok true)
                |> ignore

            let service =
                ProjectManagementService(
                    mockProjectRepository.Object,
                    mockDomainRepository.Object,
                    mockUserRepository.Object
                ) :> IProjectManagementService

            // Act
            let! result = service.IsUserProjectMemberAsync(userId, projectId)

            // Assert
            match result with
            | Ok isMember ->
                Assert.True(isMember, "メンバー判定はtrueであるべき")
            | Error msg ->
                Assert.Fail($"エラーが返されました: {msg}")
        }

    // =================================================================
    // ✅ GetProjectDetailAsync UserCount実装テスト（1件）
    // =================================================================

    /// <summary>
    /// GetProjectDetailAsync UserCount実装テスト
    /// </summary>
    [<Fact>]
    member this.``GetProjectDetailAsync_UserCount実装_成功を返すべき``() =
        task {
            // Arrange
            let project = this.CreateTestProject(1L, 1L)

            // Mock設定
            mockProjectRepository <- Mock<IProjectRepository>()
            mockDomainRepository <- Mock<IDomainRepository>()

            mockProjectRepository
                .Setup(fun x -> x.GetByIdAsync(It.IsAny<ProjectId>()))
                .ReturnsAsync(Ok (Some project))
                |> ignore

            mockProjectRepository
                .Setup(fun x -> x.IsUserProjectMemberAsync(It.IsAny<UserId>(), It.IsAny<ProjectId>()))
                .ReturnsAsync(Ok false)
                |> ignore

            // GetRelatedDataCountAsync: (DomainCount, LanguageCount, MemberCount)の合計
            mockProjectRepository
                .Setup(fun x -> x.GetRelatedDataCountAsync(It.IsAny<ProjectId>()))
                .ReturnsAsync(Ok 12)  // 合計値
                |> ignore

            mockDomainRepository
                .Setup(fun x -> x.GetByProjectIdAsync(It.IsAny<ProjectId>()))
                .ReturnsAsync(Ok [])
                |> ignore

            mockProjectRepository
                .Setup(fun x -> x.GetProjectMemberCountAsync(It.IsAny<ProjectId>()))
                .ReturnsAsync(Ok 5)
                |> ignore

            let service = this.CreateService()

            let query: GetProjectDetailQuery = {
                ProjectId = Guid.NewGuid()
                UserId = Guid.NewGuid()
                UserRole = Role.SuperUser
            }

            // Act
            let! result = service.GetProjectDetailAsync(query)

            // Assert
            match result with
            | Ok detail ->
                Assert.Equal(5, detail.UserCount)
            | Error msg ->
                Assert.Fail($"エラーが返されました: {msg}")
        }

    // =================================================================
    // ✅ 権限制御マトリックステスト（Theory・2-4件）
    // =================================================================

    /// <summary>
    /// AddMemberToProjectAsync権限制御マトリックステスト
    /// </summary>
    [<Theory>]
    [<InlineData("SuperUser", true)>]
    [<InlineData("ProjectManager", true)>]
    [<InlineData("DomainApprover", false)>]
    [<InlineData("GeneralUser", false)>]
    member this.``AddMemberToProjectAsync_権限制御マトリックス_正常動作``(roleStr: string, expectedSuccess: bool) =
        task {
            // Arrange
            let role =
                match roleStr with
                | "SuperUser" -> Role.SuperUser
                | "ProjectManager" -> Role.ProjectManager
                | "DomainApprover" -> Role.DomainApprover
                | "GeneralUser" -> Role.GeneralUser
                | _ -> Role.GeneralUser

            let project = this.CreateTestProject(1L, 3L) // ProjectManager = operatorId
            let user = this.CreateTestUser(2L, Role.GeneralUser)

            // Mock設定
            mockProjectRepository <- Mock<IProjectRepository>()
            mockUserRepository <- Mock<IUserRepository>()

            mockProjectRepository
                .Setup(fun x -> x.GetByIdAsync(It.IsAny<ProjectId>()))
                .ReturnsAsync(Ok (Some project))
                |> ignore

            mockUserRepository
                .Setup(fun x -> x.GetByIdAsync(It.IsAny<UserId>()))
                .ReturnsAsync(Ok (Some user))
                |> ignore

            // Phase B2: ProjectManagerの場合、IsUserProjectMemberAsyncのMock設定追加
            mockProjectRepository
                .Setup(fun x -> x.IsUserProjectMemberAsync(It.IsAny<UserId>(), It.IsAny<ProjectId>()))
                .ReturnsAsync(Ok true)  // ProjectManagerは担当プロジェクトのメンバー
                |> ignore

            mockProjectRepository
                .Setup(fun x -> x.AddUserToProjectAsync(It.IsAny<UserId>(), It.IsAny<ProjectId>(), It.IsAny<UserId>()))
                .ReturnsAsync(Ok ())
                |> ignore

            let service = this.CreateService()

            let command: AddMemberToProjectCommand = {
                ProjectId = Guid.NewGuid()
                UserId = Guid.NewGuid()
                OperatorUserId = Guid.NewGuid()
                OperatorRole = role
            }

            // Act
            let! result = service.AddMemberToProjectAsync(command)

            // Assert
            match result, expectedSuccess with
            | Ok _, true ->
                Assert.True(true, "成功を期待")
            | Ok _, false ->
                Assert.Fail("エラーを期待しましたが成功しました")
            | Error msg, false ->
                Assert.Contains("権限がありません", msg)
            | Error msg, true ->
                Assert.Fail($"成功を期待しましたがエラーが返されました: {msg}")
        }
