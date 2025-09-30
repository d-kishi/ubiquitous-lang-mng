namespace UbiquitousLanguageManager.Domain

// 🎯 ProjectDomainService.fs: プロジェクト管理ドメインサービス
// Bounded Context: ProjectManagement
// このファイルはプロジェクト管理に関するドメインサービスを定義します
//
// 【F#初学者向け解説】
// プロジェクトとデフォルトドメインの同時作成など、複数のエンティティにまたがる
// 複雑なビジネスロジックを実装します。Railway-oriented Programmingパターンを適用し、
// エラーハンドリングを型安全に実行します。

// 📁 Phase B1: プロジェクト管理ドメインサービス
// 【F#初学者向け解説】
// プロジェクトとデフォルトドメインの同時作成など、複数のエンティティにまたがる
// 複雑なビジネスロジックを実装します。Railway-oriented Programmingパターンを適用し、
// エラーハンドリングを型安全に実行します。
//
// 責務:
// - プロジェクト作成時のデフォルトドメイン自動作成
// - プロジェクト名重複チェック（全プロジェクトを横断）
// - プロジェクト作成権限チェック
// - プロジェクト統計情報計算
// - プロジェクト削除時の依存関係チェック
module ProjectDomainService =

    // 🔧 Railway-oriented Programming バインド演算子（要求仕様準拠）
    // 【F#初学者向け解説】
    // F#のカスタム演算子により、Result型のバインド操作を簡潔に記述できます。
    // >>= は Result.bind の中置記法で、成功時は次の処理を実行し、失敗時はエラーを伝播します。
    //
    // 使用例:
    // ```fsharp
    // validatePermission user
    // >>= fun _ -> validateProjectName name
    // >>= fun _ -> createProject name description
    // ```
    // この記法により、エラーハンドリングのネストを回避し、処理の流れを読みやすく表現できます。
    let (>>=) result func = Result.bind func result
    let (<!>) result func = Result.map func result

    // 🔍 プロジェクト名重複チェック: アクティブなプロジェクトでの名前重複を検証
    // 【F#初学者向け解説】
    // List.exists関数を使用して、既存のアクティブなプロジェクトで同名のものがないかチェックします。
    // 大文字小文字を区別しない比較により、ユーザビリティを向上させています。
    //
    // ビジネスルール:
    // - アクティブなプロジェクト間でのみ重複チェック（非アクティブは除外）
    // - 大文字小文字を区別しない比較（StringComparison.OrdinalIgnoreCase）
    // - 自分自身は除外（プロジェクト更新時）
    let validateUniqueProjectName (name: ProjectName) (existingProjects: Project list) : Result<unit, ProjectCreationError> =
        let isDuplicate =
            existingProjects
            |> List.exists (fun project ->
                project.IsActive &&
                System.String.Equals(project.Name.Value, name.Value, System.StringComparison.OrdinalIgnoreCase))

        if isDuplicate then
            Error (ProjectCreationError.DuplicateProjectName "指定されたプロジェクト名は既に使用されています")
        else
            Ok ()

    // 🔧 プロジェクトとデフォルトドメインの同時作成: Railway-oriented Programming実装
    // 【F#初学者向け解説】
    // Result型を使用したRailway-oriented Programmingパターンの実装例です。
    // 各ステップが成功した場合のみ次のステップに進み、エラーが発生した場合は即座に処理を中断します。
    // タプル型(Project * Domain)により、作成されたプロジェクトとドメインの両方を返します。
    //
    // 処理フロー:
    // 1. プロジェクト名の重複チェック
    // 2. プロジェクト作成（Project.create）
    // 3. デフォルトドメイン作成（Domain.createDefault）
    // 4. 成功時は(Project * Domain)タプルを返却
    //
    // Railway-oriented Programming:
    // - エラー発生時は即座に処理を中断し、エラーを返す
    // - 成功時は次の処理へ進む
    // - Result<'T, ProjectCreationError>型により型安全なエラーハンドリング
    let createProjectWithDefaultDomain
        (name: ProjectName)
        (description: ProjectDescription)
        (ownerId: UserId)
        (existingProjects: Project list) : Result<Project * Domain, ProjectCreationError> =

        // Step 1: プロジェクト名の重複チェック
        match validateUniqueProjectName name existingProjects with
        | Error err -> Error err
        | Ok () ->
            // Step 2: プロジェクト作成
            let project = Project.create name description ownerId

            // Step 3: デフォルトドメイン作成
            match Domain.createDefault project.Id name ownerId with
            | Error err -> Error (ProjectCreationError.DomainCreationFailed err)
            | Ok domain ->
                // Step 4: 成功時は両方を返す
                Ok (project, domain)

    // 🔧 Railway-oriented Programming を使った関数合成版
    // 【F#初学者向け解説】
    // Result.bindを使用した関数型プログラミングスタイルの実装です。
    // パイプライン演算子(|>)とResult.bindの組み合わせにより、
    // エラーハンドリングを含む処理の流れを読みやすく表現できます。
    //
    // この実装は上記のcreateProjectWithDefaultDomainと同等の処理を、
    // より関数型プログラミング的なスタイルで記述した例です。
    let createProjectWithDefaultDomainPipeline
        (name: ProjectName)
        (description: ProjectDescription)
        (ownerId: UserId)
        (existingProjects: Project list) : Result<Project * Domain, ProjectCreationError> =

        validateUniqueProjectName name existingProjects
        |> Result.bind (fun () ->
            // プロジェクト作成成功
            let project = Project.create name description ownerId

            // デフォルトドメイン作成
            Domain.createDefault project.Id name ownerId
            |> Result.mapError ProjectCreationError.DomainCreationFailed  // エラー型の変換
            |> Result.map (fun domain -> (project, domain))  // 成功時はタプルを作成
        )

    // 🔐 プロジェクト作成権限検証（要求仕様準拠版）
    // 【F#初学者向け解説】
    // 権限チェックを専用の関数として分離し、適切なエラー型を使用します。
    //
    // ビジネスルール:
    // - アクティブなユーザーのみプロジェクト作成可能
    // - CreateProjects権限を持つユーザーのみ作成可能
    // - SuperUser、ProjectManager、DomainApproverが該当
    let validateProjectCreationPermission (operatorUser: User) : Result<unit, ProjectCreationError> =
        if not operatorUser.IsActive then
            Error (ProjectCreationError.InsufficientPermissions "非アクティブなユーザーはプロジェクトを作成できません")
        elif not (PermissionMappings.hasPermission operatorUser.Role CreateProjects) then
            Error (ProjectCreationError.InsufficientPermissions "プロジェクト作成の権限がありません")
        else
            Ok ()

    // 🔧 完全版プロジェクト作成（権限チェック統合・要求仕様完全準拠）
    // 【F#初学者向け解説】
    // Railway-oriented Programmingパターンを使用した完全版のプロジェクト作成関数です。
    // 権限チェック、重複チェック、プロジェクト作成、デフォルトドメイン作成を統合的に実行し、
    // バインド演算子（>>=）を使用してエラーハンドリングを簡潔に表現します。
    //
    // 処理フロー（Railway-oriented Programming）:
    // 1. 権限チェック（validateProjectCreationPermission）
    // 2. ↓（成功時のみ次へ）
    // 3. プロジェクト名重複チェック（validateUniqueProjectName）
    // 4. ↓（成功時のみ次へ）
    // 5. プロジェクト作成（Project.create）
    // 6. ↓（成功時のみ次へ）
    // 7. デフォルトドメイン作成（Domain.createDefault）
    // 8. ↓（成功時のみ次へ）
    // 9. (Project * Domain)タプル返却
    //
    // エラー時はどの段階でも即座に処理を中断し、エラーを返します。
    let createProjectWithPermissionCheck
        (name: ProjectName)
        (description: ProjectDescription)
        (ownerId: UserId)
        (operatorUser: User)
        (existingProjects: Project list) : Result<Project * Domain, ProjectCreationError> =

        // Railway-oriented Programming パイプライン実行
        validateProjectCreationPermission operatorUser
        >>= fun _ -> validateUniqueProjectName name existingProjects
        >>= fun _ ->
            let project = Project.create name description ownerId
            Domain.createDefault project.Id name ownerId
            |> Result.mapError ProjectCreationError.DomainCreationFailed
            |> Result.map (fun domain -> (project, domain))

    // 🔄 バインド操作による関数合成ヘルパー（要求仕様準拠）
    // 【F#初学者向け解説】
    // 複数のバリデーション関数を合成し、すべてが成功した場合のみ処理を継続します。
    // 関数型プログラミングの関数合成により、複雑な処理フローを簡潔に表現できます。
    //
    // 使用例:
    // ```fsharp
    // let validations = [
    //     (fun () -> validatePermission user)
    //     (fun () -> validateProjectName name)
    //     (fun () -> validateOwner owner)
    // ]
    // combineValidations validations
    // ```
    let combineValidations (validations: (unit -> Result<unit, ProjectCreationError>) list) : Result<unit, ProjectCreationError> =
        validations
        |> List.fold (fun acc validation ->
            acc >>= fun _ -> validation ()
        ) (Ok ())

    // 📊 プロジェクト統計情報計算: ビジネスインテリジェンス機能
    // 【F#初学者向け解説】
    // List.filter、List.lengthなどのリスト操作関数を組み合わせて、
    // プロジェクトの統計情報を効率的に計算します。
    //
    // 統計情報:
    // - TotalProjects: 全プロジェクト数
    // - ActiveProjects: アクティブなプロジェクト数
    // - InactiveProjects: 非アクティブなプロジェクト数
    // - ProjectsWithDomains: ドメインを持つプロジェクト数
    // - AverageDomainsPerProject: プロジェクトあたりの平均ドメイン数
    type ProjectStatistics = {
        TotalProjects: int
        ActiveProjects: int
        InactiveProjects: int
        ProjectsWithDomains: int
        AverageDomainsPerProject: float
    }

    // 統計情報計算関数
    // 【F#初学者向け解説】
    // パイプライン演算子(|>)を使用して、リスト操作を連鎖的に実行します。
    // 例: projects |> List.filter (fun p -> p.IsActive) |> List.length
    // これは「projectsをフィルタリングして、その結果の長さを取得」という処理の流れを表現します。
    let calculateProjectStatistics (projects: Project list) (domains: Domain list) : ProjectStatistics =
        let totalProjects = List.length projects
        let activeProjects = projects |> List.filter (fun p -> p.IsActive) |> List.length
        let inactiveProjects = totalProjects - activeProjects

        let projectsWithDomains =
            projects
            |> List.filter (fun project ->
                domains |> List.exists (fun domain -> domain.ProjectId = project.Id))
            |> List.length

        let averageDomainsPerProject =
            if totalProjects > 0 then
                float (List.length domains) / float totalProjects
            else
                0.0

        {
            TotalProjects = totalProjects
            ActiveProjects = activeProjects
            InactiveProjects = inactiveProjects
            ProjectsWithDomains = projectsWithDomains
            AverageDomainsPerProject = averageDomainsPerProject
        }

    // 🔄 プロジェクト名前変更ビジネスルール: 複雑な制約チェック
    // 【F#初学者向け解説】
    // プロジェクト名変更の際の複雑なビジネスルールをドメインサービスで集約します。
    // 単一のProjectエンティティでは表現できない、全プロジェクトにまたがる制約を実装します。
    //
    // ビジネスルール:
    // - プロジェクト管理権限が必要
    // - 自分以外のアクティブなプロジェクトと名前が重複しないこと
    // - 大文字小文字を区別しない比較
    let validateProjectNameChange
        (project: Project)
        (newName: ProjectName)
        (existingProjects: Project list)
        (operatorUser: User) : Result<unit, ProjectCreationError> =

        // 権限チェック
        if not (PermissionMappings.hasPermission operatorUser.Role ManageProjects) then
            Error (ProjectCreationError.InsufficientPermissions "プロジェクト管理の権限がありません")
        // 同名チェック（自分以外）
        elif existingProjects
            |> List.exists (fun p ->
                p.Id <> project.Id &&
                p.IsActive &&
                System.String.Equals(p.Name.Value, newName.Value, System.StringComparison.OrdinalIgnoreCase)) then
            Error (ProjectCreationError.DuplicateProjectName "指定されたプロジェクト名は既に使用されています")
        else
            Ok ()

    // 🗑️ プロジェクト削除ビジネスルール: 関連データ確認
    // 【F#初学者向け解説】
    // プロジェクト削除前に関連するドメインやユビキタス言語の存在を確認し、
    // データの整合性を保つためのビジネスルールを実装します。
    //
    // ビジネスルール:
    // - プロジェクト削除権限が必要
    // - アクティブなドメインが存在するプロジェクトは削除不可
    // - 非アクティブなドメインのみのプロジェクトは削除可能
    let validateProjectDeletion
        (project: Project)
        (relatedDomains: Domain list)
        (operatorUser: User) : Result<unit, ProjectCreationError> =

        // 権限チェック
        if not (PermissionMappings.hasPermission operatorUser.Role DeleteProjects) then
            Error (ProjectCreationError.InsufficientPermissions "プロジェクト削除の権限がありません")
        // 関連ドメインの存在チェック
        elif relatedDomains |> List.exists (fun d -> d.IsActive) then
            Error (ProjectCreationError.DatabaseError "アクティブなドメインが存在するプロジェクトは削除できません")
        else
            Ok ()

// 🚨 Note: DomainServiceモジュール（ユビキタス言語管理関連）は
// Phase 5以降で別の境界文脈として移行します。
// Phase 4ではProjectManagement境界文脈のProjectDomainServiceのみを移行対象とします。