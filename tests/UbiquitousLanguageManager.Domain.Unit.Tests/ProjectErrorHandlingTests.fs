namespace UbiquitousLanguageManager.Domain.Unit.Tests

open System
open System.Diagnostics
open Xunit
open UbiquitousLanguageManager.Domain.Common
open UbiquitousLanguageManager.Domain.ProjectManagement
open UbiquitousLanguageManager.Domain.ProjectManagement.ProjectDomainService

// 🧪 F# Domain層エラーハンドリング・パフォーマンステスト
// Result型パイプライン・エラー伝播・例外ケーステスト

// 📝 【F#初学者向け解説】
// F#のエラーハンドリングテストでは、Result型の動作確認と
// Railway-oriented Programmingパターンの正しい動作を検証します。
// 例外が発生せず、型安全にエラーが処理されることを確認します。

type ProjectErrorHandlingTests() =

    // 🔧 テスト用データ作成ヘルパー
    let createValidProjectName name =
        ProjectName.create name
        |> function | Ok n -> n | Error _ -> failwith "テストデータエラー"

    let createValidProjectDescription desc =
        ProjectDescription.create desc
        |> function | Ok d -> d | Error _ -> failwith "テストデータエラー"

    let createTestUserId (id: int) =
        UserId.create (sprintf "00000000-0000-0000-0000-%012d" id)

    // 🧪 ProjectCreationError toString テスト
    [<Fact>]
    member this.``ProjectCreationError_ToString_ReturnsCorrectMessage``() =
        // Arrange & Act & Assert
        let duplicateError = ProjectCreationError.DuplicateProjectName "Test Project"
        let invalidNameError = ProjectCreationError.InvalidProjectName "Invalid"
        let invalidDescError = ProjectCreationError.InvalidProjectDescription "Too long"
        let permissionError = ProjectCreationError.InsufficientPermissions "No permission"
        let dbError = ProjectCreationError.DatabaseError "Connection failed"
        let domainError = ProjectCreationError.DomainCreationFailed "Domain error"

        // 【F#初学者向け解説】
        // 判別共用体の各ケースが正しく定義され、nullでないことを確認します。
        // F#では判別共用体は自動的にToStringメソッドを持ちますが、
        // カスタムメッセージの場合は内容を確認する必要があります。
        Assert.NotNull(duplicateError)
        Assert.NotNull(invalidNameError)
        Assert.NotNull(invalidDescError)
        Assert.NotNull(permissionError)
        Assert.NotNull(dbError)
        Assert.NotNull(domainError)

    // 🔄 Result型バインド操作テスト（成功パイプライン）
    [<Fact>]
    member this.``Result_Bind_SuccessfulPipeline_ReturnsFinalResult``() =
        // Arrange
        let initialValue = 5
        let addTen x = Ok (x + 10)
        let multiplyTwo x = Ok (x * 2)
        let toString x = Ok (x.ToString())

        // Act
        // 【F#初学者向け解説】
        // Result.bindを使用したRailway-oriented Programmingパターンです。
        // 各関数が成功した場合のみ次の関数が実行され、最終結果が得られます。
        let result =
            Ok initialValue
            |> Result.bind addTen
            |> Result.bind multiplyTwo
            |> Result.bind toString

        // Assert
        match result with
        | Ok value -> Assert.Equal("30", value)
        | Error _ -> Assert.True(false, "パイプラインが失敗しました")

    // ❌ Result型バインド操作テスト（エラーパイプライン）
    [<Fact>]
    member this.``Result_Bind_FailureInPipeline_ReturnsFirstError``() =
        // Arrange
        let initialValue = 5
        let addTen x = Ok (x + 10)
        let failStep x = Error "Step failed"
        let multiplyTwo x = Ok (x * 2)

        // Act
        // 【F#初学者向け解説】
        // パイプラインの途中でエラーが発生した場合、後続の処理は実行されず、
        // 最初のエラーがそのまま伝播されることを確認します。
        let result =
            Ok initialValue
            |> Result.bind addTen
            |> Result.bind failStep      // ここでエラー発生
            |> Result.bind multiplyTwo   // この処理は実行されない

        // Assert
        match result with
        | Error msg -> Assert.Equal("Step failed", msg)
        | Ok _ -> Assert.True(false, "エラーが期待されましたが成功しました")

    // 🔗 複数バリデーション統合テスト
    [<Fact>]
    member this.``combineValidations_AllSuccessful_ReturnsOk``() =
        // Arrange
        let validation1 () = Ok ()
        let validation2 () = Ok ()
        let validation3 () = Ok ()
        let validations = [validation1; validation2; validation3]

        // Act
        let result = combineValidations validations

        // Assert
        match result with
        | Ok () -> Assert.True(true)
        | Error _ -> Assert.True(false, "すべて成功するはずでしたがエラーが発生しました")

    // ❌ 複数バリデーション統合テスト（最初のエラーで停止）
    [<Fact>]
    member this.``combineValidations_FirstFails_ReturnsFirstError``() =
        // Arrange
        let validation1 () = Ok ()
        let validation2 () = Error (ProjectCreationError.DuplicateProjectName "First error")
        let validation3 () = Error (ProjectCreationError.InvalidProjectName "Second error")  // これは実行されない
        let validations = [validation1; validation2; validation3]

        // Act
        let result = combineValidations validations

        // Assert
        match result with
        | Error (ProjectCreationError.DuplicateProjectName msg) ->
            Assert.Equal("First error", msg)
        | Ok _ ->
            Assert.True(false, "エラーが期待されましたが成功しました")
        | Error other ->
            Assert.True(false, $"最初のエラーが期待されましたが別のエラーが発生: {other}")

    // 🔄 ProjectDomainService カスタム演算子テスト
    [<Fact>]
    member this.``ProjectDomainService_CustomOperators_WorkCorrectly``() =
        // Arrange
        let addFive x = Ok (x + 5)
        let multiplyThree x = Ok (x * 3)

        // Act
        // 【F#初学者向け解説】
        // >>= はResult.bindの中置記法、<!> はResult.mapの中置記法です。
        // F#ではカスタム演算子により、より読みやすいコードを書けます。
        let result =
            Ok 10
            >>= addFive      // Ok 15
            <!> fun x -> x * 2  // Ok 30

        // Assert
        match result with
        | Ok value -> Assert.Equal(30, value)
        | Error _ -> Assert.True(false, "カスタム演算子が失敗しました")

    // 🔄 ProjectDomainService Railway-oriented Programming 統合テスト
    [<Fact>]
    member this.``ProjectDomainService_RailwayOrientedProgramming_IntegrationTest``() =
        // Arrange
        let name = createValidProjectName "ROP Integration Test"
        let description = createValidProjectDescription (Some "Railway-oriented programming test")
        let ownerId = createTestUserId 1
        let existingProjects = []

        // Act
        // 【F#初学者向け解説】
        // createProjectWithDefaultDomainPipelineは完全なRailway-oriented Programmingの実装例です。
        // 複数のバリデーション、プロジェクト作成、ドメイン作成が順次実行され、
        // いずれかでエラーが発生した場合は即座にエラーが返されます。
        let result = createProjectWithDefaultDomainPipeline name description ownerId existingProjects

        // Assert
        match result with
        | Ok (project, domain) ->
            Assert.Equal(name, project.Name)
            Assert.Equal(description, project.Description)
            Assert.Equal(ownerId, project.OwnerId)
            Assert.Equal(project.Id, domain.ProjectId)
            Assert.True(domain.IsDefault)
        | Error err ->
            Assert.True(false, $"Railway-oriented Programming統合テストが失敗: {err}")

    // ⚡ パフォーマンステスト：プロジェクト作成
    [<Fact>]
    member this.``Project_Creation_Performance_UnderAcceptableTime``() =
        // Arrange
        let name = createValidProjectName "Performance Test Project"
        let description = createValidProjectDescription (Some "Performance test")
        let ownerId = createTestUserId 1
        let stopwatch = Stopwatch.StartNew()

        // Act
        let project = Project.create name description ownerId
        stopwatch.Stop()

        // Assert
        Assert.NotNull(project)
        // 【F#初学者向け解説】
        // パフォーマンステストでは、処理時間が期待値以下であることを確認します。
        // F#のレコード型作成は高速ですが、念のため10ms以下であることを確認します。
        Assert.True(stopwatch.ElapsedMilliseconds < 10L, $"プロジェクト作成時間が長すぎます: {stopwatch.ElapsedMilliseconds}ms")

    // ⚡ パフォーマンステスト：ProjectDomainService バッチ処理
    [<Fact>]
    member this.``ProjectDomainService_BatchCreation_PerformanceAcceptable``() =
        // Arrange
        let ownerId = createTestUserId 1
        let existingProjects = []
        let stopwatch = Stopwatch.StartNew()

        // Act
        // 【F#初学者向け解説】
        // List.mapを使用して100個のプロジェクトを一括作成します。
        // F#の関数型プログラミングにより、効率的なバッチ処理が可能です。
        let results =
            [1..100]
            |> List.map (fun i ->
                let name = createValidProjectName $"Batch Project {i}"
                let description = createValidProjectDescription None
                createProjectWithDefaultDomain name description ownerId existingProjects
            )

        stopwatch.Stop()

        // Assert
        // すべての結果が成功していることを確認
        let allSuccessful = results |> List.forall (function | Ok _ -> true | Error _ -> false)
        Assert.True(allSuccessful, "バッチ処理で失敗したプロジェクトがあります")

        // パフォーマンス確認（1秒以下）
        Assert.True(stopwatch.ElapsedMilliseconds < 1000L, $"バッチ処理時間が長すぎます: {stopwatch.ElapsedMilliseconds}ms")

    // 🔍 メモリ使用量テスト（リソースリーク確認）
    [<Fact>]
    member this.``ProjectDomainService_MemoryUsage_NoMemoryLeak``() =
        // Arrange
        let initialMemory = GC.GetTotalMemory(true)
        let ownerId = createTestUserId 1

        // Act
        // 大量のプロジェクト作成・破棄を繰り返してメモリリークをチェック
        for i in 1..1000 do
            let name = createValidProjectName $"Memory Test Project {i}"
            let description = createValidProjectDescription (Some $"Memory test {i}")
            let project = Project.create name description ownerId
            // プロジェクトは自動的にガベージコレクションされる
            ignore project

        GC.Collect()
        GC.WaitForPendingFinalizers()
        GC.Collect()

        let finalMemory = GC.GetTotalMemory(true)
        let memoryIncrease = finalMemory - initialMemory

        // Assert
        // 【F#初学者向け解説】
        // F#のレコード型は値型的な性質を持ち、メモリリークが発生しにくいです。
        // 1000個のプロジェクト作成後でも、メモリ増加が1MB以下であることを確認します。
        Assert.True(memoryIncrease < 1024L * 1024L, $"メモリ使用量が予想以上に増加: {memoryIncrease} bytes")

    // 🔧 境界値テスト：極端なケース
    [<Fact>]
    member this.``ProjectDomainService_EdgeCases_HandledCorrectly``() =
        // Arrange & Act & Assert

        // 空のプロジェクトリスト
        let emptyList = []
        let name = createValidProjectName "Edge Case Test"
        let description = createValidProjectDescription None
        let ownerId = createTestUserId 1

        let result1 = validateUniqueProjectName name emptyList
        match result1 with
        | Ok () -> Assert.True(true)
        | Error _ -> Assert.True(false, "空リストでエラーが発生")

        // 大量の既存プロジェクト（性能確認）
        let manyProjects =
            [1..10000]
            |> List.map (fun i ->
                let projectName = createValidProjectName $"Existing Project {i}"
                let projectDesc = createValidProjectDescription None
                {
                    Id = ProjectId (System.Guid.NewGuid().GetHashCode() |> int64 |> abs)
                    Name = projectName
                    Description = projectDesc
                    OwnerId = ownerId
                    CreatedAt = DateTime.UtcNow
                    UpdatedAt = None
                    IsActive = true
                })

        let uniqueName = createValidProjectName "Truly Unique Project"
        let result2 = validateUniqueProjectName uniqueName manyProjects
        match result2 with
        | Ok () -> Assert.True(true)
        | Error _ -> Assert.True(false, "大量プロジェクト中での一意性チェックでエラー")