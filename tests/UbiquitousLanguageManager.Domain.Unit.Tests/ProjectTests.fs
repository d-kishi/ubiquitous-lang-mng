namespace UbiquitousLanguageManager.Domain.Unit.Tests

open System
open Xunit
open UbiquitousLanguageManager.Domain.Common
open UbiquitousLanguageManager.Domain.ProjectManagement

// 🧪 F# Domain層プロジェクトテスト（Phase B1 TDD実装）
// Railway-oriented Programming・Result型・Smart Constructorのテスト

// 📝 【F#初学者向け解説】
// F#でxUnitテストを書く方法：
// 1. [<Fact>]属性でテストメソッドを定義
// 2. Assert.Equal、Assert.Trueなどでアサーション
// 3. パターンマッチングでResult型・Option型の検証
// 4. type Test() = とクラス型で複数テストをグループ化

type ProjectTests() =

    // 🔧 テスト用データ作成ヘルパー
    // 【F#初学者向け解説】
    // F#では関数を使ってテストデータを作成します。let bindingにより、
    // 関数やデータを定義し、テスト間で再利用できます。
    let createValidProjectName() =
        ProjectName.create "Valid Project Name"
        |> function
           | Ok name -> name
           | Error _ -> failwith "テストデータエラー"

    let createValidProjectDescription() =
        ProjectDescription.create (Some "Valid project description")
        |> function
           | Ok desc -> desc
           | Error _ -> failwith "テストデータエラー"

    let createTestUserId() = UserId.create "00000000-0000-0000-0000-000000000001"

    // ✅ ProjectName Smart Constructor テスト（正常系）
    [<Fact>]
    member this.``ProjectName_create_ValidName_ReturnsOk``() =
        // Arrange
        let validName = "Valid Project Name"

        // Act
        let result = ProjectName.create validName

        // Assert
        // 【F#初学者向け解説】
        // パターンマッチングを使用してResult型の内容を検査します。
        // match式により、Ok・Error両方のケースを明示的に処理します。
        match result with
        | Ok projectName ->
            Assert.Equal(validName, projectName.Value)
        | Error _ ->
            Assert.True(false, "正常なプロジェクト名でエラーが発生しました")

    // ❌ ProjectName Smart Constructor テスト（異常系：空文字列）
    [<Fact>]
    member this.``ProjectName_create_EmptyName_ReturnsError``() =
        // Arrange
        let emptyName = ""

        // Act
        let result = ProjectName.create emptyName

        // Assert
        match result with
        | Error msg ->
            Assert.Equal("プロジェクト名は必須です", msg)
        | Ok _ ->
            Assert.True(false, "空文字列でProjectNameが作成されました")

    // ❌ ProjectName Smart Constructor テスト（異常系：null）
    [<Fact>]
    member this.``ProjectName_create_NullName_ReturnsError``() =
        // Arrange
        let nullName = null

        // Act
        let result = ProjectName.create nullName

        // Assert
        match result with
        | Error msg ->
            Assert.Equal("プロジェクト名は必須です", msg)
        | Ok _ ->
            Assert.True(false, "nullでProjectNameが作成されました")

    // ❌ ProjectName Smart Constructor テスト（異常系：長すぎる）
    [<Fact>]
    member this.``ProjectName_create_TooLongName_ReturnsError``() =
        // Arrange
        let longName = String.replicate 101 "a"  // 101文字

        // Act
        let result = ProjectName.create longName

        // Assert
        match result with
        | Error msg ->
            Assert.Equal("プロジェクト名は100文字以内で入力してください", msg)
        | Ok _ ->
            Assert.True(false, "101文字でProjectNameが作成されました")

    // ✅ ProjectName Smart Constructor テスト（境界値：最大文字数）
    [<Fact>]
    member this.``ProjectName_create_ExactlyMaxLength_ReturnsOk``() =
        // Arrange
        let maxLengthName = String.replicate 100 "a"  // 100文字

        // Act
        let result = ProjectName.create maxLengthName

        // Assert
        match result with
        | Ok projectName ->
            Assert.Equal(maxLengthName, projectName.Value)
        | Error _ ->
            Assert.True(false, "100文字でエラーが発生しました")

    // ❌ ProjectName Smart Constructor テスト（異常系：短すぎる）
    [<Fact>]
    member this.``ProjectName_create_TooShortName_ReturnsError``() =
        // Arrange
        let shortName = "ab"  // 2文字（3文字未満）

        // Act
        let result = ProjectName.create shortName

        // Assert
        match result with
        | Error msg ->
            Assert.Equal("プロジェクト名は3文字以上で入力してください", msg)
        | Ok _ ->
            Assert.True(false, "2文字でProjectNameが作成されました")

    // ✅ ProjectDescription Smart Constructor テスト（正常系：有効な説明）
    [<Fact>]
    member this.``ProjectDescription_create_ValidDescription_ReturnsOk``() =
        // Arrange
        let validDescription = Some "Valid project description"

        // Act
        let result = ProjectDescription.create validDescription

        // Assert
        match result with
        | Ok projectDesc ->
            Assert.Equal(validDescription, projectDesc.Value)
        | Error _ ->
            Assert.True(false, "正常な説明でエラーが発生しました")

    // ✅ ProjectDescription Smart Constructor テスト（正常系：None）
    [<Fact>]
    member this.``ProjectDescription_create_None_ReturnsOk``() =
        // Arrange
        let noneDescription = None

        // Act
        let result = ProjectDescription.create noneDescription

        // Assert
        match result with
        | Ok projectDesc ->
            Assert.Equal(None, projectDesc.Value)
        | Error _ ->
            Assert.True(false, "Noneでエラーが発生しました")

    // ❌ ProjectDescription Smart Constructor テスト（異常系：長すぎる）
    [<Fact>]
    member this.``ProjectDescription_create_TooLongDescription_ReturnsError``() =
        // Arrange
        let longDescription = Some (String.replicate 1001 "a")  // 1001文字

        // Act
        let result = ProjectDescription.create longDescription

        // Assert
        match result with
        | Error msg ->
            Assert.Equal("プロジェクト説明は1000文字以内で入力してください", msg)
        | Ok _ ->
            Assert.True(false, "1001文字でProjectDescriptionが作成されました")

    // ✅ Project作成テスト（正常系）
    [<Fact>]
    member this.``Project_create_ValidInput_ReturnsProjectWithCorrectProperties``() =
        // Arrange
        let name = createValidProjectName()
        let description = createValidProjectDescription()
        let ownerId = createTestUserId()
        let beforeCreation = DateTime.UtcNow

        // Act
        let project = Project.create name description ownerId
        let afterCreation = DateTime.UtcNow

        // Assert
        Assert.Equal(name, project.Name)
        Assert.Equal(description, project.Description)
        Assert.Equal(ownerId, project.OwnerId)
        Assert.True(project.IsActive)
        Assert.Equal(None, project.UpdatedAt)

        // 作成日時の検証（範囲チェック）
        // 【F#初学者向け解説】
        // DateTime型の比較では、テスト実行時間のわずかなズレを考慮して
        // 範囲チェックを行います。
        Assert.True(project.CreatedAt >= beforeCreation)
        Assert.True(project.CreatedAt <= afterCreation)

        // IDが設定されていることを確認（0Lは仮値）
        match project.Id with
        | ProjectId id -> Assert.NotEqual(0L, id)

    // ✅ ProjectId生成テスト（一意性確認）
    [<Fact>]
    member this.``Project_create_MultipleTimes_GeneratesUniqueIds``() =
        // Arrange
        let name = createValidProjectName()
        let description = createValidProjectDescription()
        let ownerId = createTestUserId()

        // Act
        let project1 = Project.create name description ownerId
        let project2 = Project.create name description ownerId
        let project3 = Project.create name description ownerId

        // Assert
        // 【F#初学者向け解説】
        // F#では<>演算子で非等価を確認します。
        // IDが確実に異なることを確認し、一意性を保証します。
        Assert.NotEqual(project1.Id, project2.Id)
        Assert.NotEqual(project2.Id, project3.Id)
        Assert.NotEqual(project1.Id, project3.Id)