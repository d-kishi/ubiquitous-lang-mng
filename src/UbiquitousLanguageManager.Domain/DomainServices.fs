namespace UbiquitousLanguageManager.Domain

// 🎯 ドメインサービス: 複数のエンティティにまたがるビジネスロジック
// 単一のエンティティでは表現できない複雑なドメインルールを実装

module DomainService =
    
    // 🔐 ユーザー権限検証: ユーザーが特定のドメインでユビキタス言語を作成可能か判定
    let validateUserCanCreateInDomain (userId: UserId) (domain: Domain) =
        // 🎭 ビジネスルール: アクティブなドメインでのみ作成可能
        if not domain.IsActive then
            Error "非アクティブなドメインではユビキタス言語を作成できません"
        else
            Ok () // 👍 基本的な検証は通過（詳細な権限チェックはApplication層で実施）
    
    // 🔍 重複チェック: 同一ドメイン内での用語名重複を防止
    let validateUniqueNamesInDomain (japaneseName: JapaneseName) (englishName: EnglishName) 
                                   (existingTerms: FormalUbiquitousLanguage list) =
        // 🎯 日本語名の重複チェック
        let japaneseNameExists = 
            existingTerms 
            |> List.exists (fun term -> term.JapaneseName.Value = japaneseName.Value)
        
        // 🎯 英語名の重複チェック    
        let englishNameExists = 
            existingTerms 
            |> List.exists (fun term -> term.EnglishName.Value = englishName.Value)
        
        // 🚫 重複エラーの判定
        match japaneseNameExists, englishNameExists with
        | true, true -> Error "日本語名・英語名ともに既に使用されています"
        | true, false -> Error "日本語名が既に使用されています"
        | false, true -> Error "英語名が既に使用されています"
        | false, false -> Ok () // ✅ 重複なし
    
    // 🔄 承認ワークフロー検証: 承認者の権限チェック
    let validateApprovalAuthorization (approverId: UserId) (approverRole: Role) (domain: Domain) =
        match approverRole with
        | SuperUser -> Ok () // 🎖️ スーパーユーザーは全ドメイン承認可能
        | ProjectManager -> Ok () // 👨‍💼 プロジェクト管理者も承認可能
        | DomainApprover -> Ok () // ✅ ドメイン承認者は担当ドメインの承認可能
        | GeneralUser -> Error "一般ユーザーは承認権限がありません" // ❌ 一般ユーザーは承認不可
    
    // 📊 ステータス遷移検証: 正しいワークフローでのステータス変更を保証
    let validateStatusTransition (currentStatus: ApprovalStatus) (targetStatus: ApprovalStatus) =
        match currentStatus, targetStatus with
        | Draft, Submitted -> Ok () // 下書き → 申請
        | Submitted, Approved -> Ok () // 申請 → 承認
        | Submitted, Rejected -> Ok () // 申請 → 却下
        | Rejected, Draft -> Ok () // 却下 → 下書き（修正のため）
        | _ -> Error $"ステータス '{currentStatus}' から '{targetStatus}' への変更は許可されていません"

// 👤 Phase A2: ユーザー管理ドメインサービス
// 【F#初学者向け解説】
// ユーザー管理に関する複雑なビジネスルールを集約したドメインサービスです。
// 単一のUserエンティティでは表現できない、複数のユーザー間の関係性や
// システム全体にまたがるビジネスルールを実装します。
module UserDomainService =
    
    // 🔐 ユーザー作成権限検証: 新規ユーザー作成権限の詳細チェック
    // 【F#初学者向け解説】
    // この関数はResult型を返します。成功時はOk()、失敗時はError文字列を返し、
    // エラー処理をコンパイル時に強制することで、権限チェック漏れを防ぎます。
    let validateUserCreationPermission (operatorUser: User) (targetRole: Role) : Result<unit, string> =
        // 操作者自身がアクティブである必要
        if not operatorUser.IsActive then
            Error "非アクティブなユーザーは新規ユーザーを作成できません"
        // SuperUser作成はSuperUserのみ可能
        elif targetRole = SuperUser && operatorUser.Role <> SuperUser then
            Error "SuperUserの作成はSuperUserのみが実行できます"
        // ユーザー作成権限の確認
        elif not (PermissionMappings.hasPermission operatorUser.Role CreateUsers) then
            Error "ユーザー作成の権限がありません"
        else
            Ok ()
    
    // 📧 メールアドレス重複チェック: システム全体での一意性保証
    let validateUniqueEmail (email: Email) (existingUsers: User list) : Result<unit, string> =
        let isDuplicate = 
            existingUsers 
            |> List.exists (fun user -> 
                System.String.Equals(user.Email.Value, email.Value, System.StringComparison.OrdinalIgnoreCase) 
                && user.IsActive)
        
        if isDuplicate then
            Error "このメールアドレスは既に使用されています"
        else
            Ok ()
    
    // 🎭 ロール変更権限検証: 複雑なロール変更ルールの実装
    // 【F#初学者向け解説】
    // パターンマッチングを使用して、すべてのロール変更パターンを網羅的にチェックします。
    // F#のコンパイラは、すべてのケースが処理されているかを確認するため、
    // ビジネスルールの漏れを防ぐことができます。
    let validateRoleChangeAuthorization (operatorUser: User) (targetUser: User) (newRole: Role) : Result<unit, string> =
        match operatorUser.Role, targetUser.Role, newRole with
        // SuperUser関連の制限
        | SuperUser, _, _ -> Ok () // SuperUserはすべての変更が可能
        | _, SuperUser, _ -> Error "SuperUserのロール変更はSuperUserのみが実行できます"
        | _, _, SuperUser -> Error "SuperUserへの昇格はSuperUserのみが実行できます"
        
        // ProjectManager以下の権限チェック
        | ProjectManager, targetRole, newRole when targetRole <> SuperUser && newRole <> SuperUser -> 
            Ok () // ProjectManagerはSuperUser以外の変更が可能
        | DomainApprover, targetRole, newRole when targetRole = GeneralUser && newRole = DomainApprover -> 
            Ok () // DomainApproverはGeneralUserをDomainApproverに昇格可能
        | DomainApprover, targetRole, newRole when targetRole = DomainApprover && newRole = GeneralUser -> 
            Ok () // DomainApproverはDomainApproverをGeneralUserに降格可能
        
        // その他は権限不足
        | _ -> Error "このロール変更を実行する権限がありません"
    
    // 🏢 プロジェクト権限整合性チェック: ユーザーのロールとプロジェクト権限の整合性検証
    let validateProjectPermissionsConsistency (user: User) : Result<unit, string> =
        // GlobalRoleで既に権限を持っている場合、重複するProjectPermissionは不要
        let globalPermissions = PermissionMappings.getPermissionsForRole user.Role
        
        let redundantPermissions = 
            user.ProjectPermissions
            |> List.collect (fun projectPerm -> Set.toList projectPerm.Permissions)
            |> List.filter (fun permission -> Set.contains permission globalPermissions)
        
        if not (List.isEmpty redundantPermissions) then
            // Warning: 重複権限があるが、システム動作には影響しない
            Error $"グローバルロールで既に持っている権限が重複しています: {redundantPermissions}"
        else
            Ok ()
    
    // 🔒 アカウント無効化権限検証: 無効化対象と操作者の関係性チェック
    let validateUserDeactivationPermission (operatorUser: User) (targetUser: User) : Result<unit, string> =
        // 自分自身の無効化は禁止
        if operatorUser.Id = targetUser.Id then
            Error "自分自身のアカウントを無効化することはできません"
        // 非アクティブユーザーの無効化は無意味
        elif not targetUser.IsActive then
            Error "既に無効化されているユーザーです"
        // SuperUserの無効化はSuperUserのみ可能
        elif targetUser.Role = SuperUser && operatorUser.Role <> SuperUser then
            Error "SuperUserの無効化はSuperUserのみが実行できます"
        // 削除権限の確認
        elif not (PermissionMappings.hasPermission operatorUser.Role DeleteUsers) then
            Error "ユーザー無効化の権限がありません"
        else
            Ok ()
    
    // 🔐 パスワード変更権限検証: 自分・他人のパスワード変更権限チェック
    let validatePasswordChangePermission (operatorUser: User) (targetUser: User) : Result<unit, string> =
        // 自分のパスワード変更は常に許可（アクティブユーザーのみ）
        if operatorUser.Id = targetUser.Id && operatorUser.IsActive then
            Ok ()
        // 他人のパスワード変更には管理者権限が必要
        elif operatorUser.Id <> targetUser.Id then
            if PermissionMappings.hasPermission operatorUser.Role ManageUserRoles then
                Ok ()
            else
                Error "他のユーザーのパスワードを変更する権限がありません"
        else
            Error "非アクティブなユーザーはパスワードを変更できません"
    
    // 👥 同時ログインユーザー数制限チェック: システムリソース保護
    // 【F#初学者向け解説】
    // List.filterを使用してアクティブユーザーをフィルタリングし、
    // List.lengthで数を取得します。F#のパイプライン演算子(|>)により、
    // データの流れが左から右に明確に表現されています。
    let validateConcurrentUserLimit (currentActiveUsers: User list) (maxConcurrentUsers: int) : Result<unit, string> =
        let activeUserCount = 
            currentActiveUsers 
            |> List.filter (fun user -> user.IsActive)
            |> List.length
        
        if activeUserCount >= maxConcurrentUsers then
            Error $"同時ログイン可能なユーザー数の上限（{maxConcurrentUsers}人）に達しています"
        else
            Ok ()
    
    // 🎯 ユーザー管理業務検証: 複数の検証を組み合わせた総合チェック
    // 【F#初学者向け解説】
    // 複数のResult型を連鎖的に処理します。
    // エラーが発生した時点で処理が停止し、最初のエラーが返されます。
    let validateUserManagementOperation (operatorUser: User) (targetUser: User option) (operation: string) : Result<unit, string> =
        // 操作者のアクティブ状態確認
        if not operatorUser.IsActive then
            Error "非アクティブなユーザーはユーザー管理操作を実行できません"
        // 基本的なユーザー管理権限確認
        elif not (PermissionMappings.hasPermission operatorUser.Role ViewUsers) then
            Error "ユーザー管理機能へのアクセス権限がありません"
        else
            // 対象ユーザーが指定されている場合の追加検証
            match targetUser with
            | Some target ->
                // SuperUser関連の制限チェック
                if target.Role = SuperUser && operatorUser.Role <> SuperUser then
                    Error "SuperUserに対する操作はSuperUserのみが実行できます"
                else
                    Ok () // すべての検証をパス
            | None ->
                Ok () // 対象ユーザーなしの操作（一覧表示等）は追加チェック不要

// 📁 Phase B1: プロジェクト管理ドメインサービス
// 【F#初学者向け解説】
// プロジェクトとデフォルトドメインの同時作成など、複数のエンティティにまたがる
// 複雑なビジネスロジックを実装します。Railway-oriented Programmingパターンを適用し、
// エラーハンドリングを型安全に実行します。
module ProjectDomainService =

    // 📁 プロジェクト作成エラー判別共用体（Phase B1 完全版）
    // 【F#初学者向け解説】
    // プロジェクト作成で発生する可能性のあるエラーを型安全に表現します。
    // パターンマッチングにより、全てのエラーケースの処理を強制し、エラー処理漏れを防ぎます。
    type ProjectCreationError =
        | DuplicateProjectName of string           // プロジェクト名重複
        | InvalidProjectName of string             // 無効なプロジェクト名
        | InvalidProjectDescription of string      // 無効なプロジェクト説明
        | InsufficientPermissions of string        // 権限不足（要求仕様追加）
        | DomainCreationFailed of string          // デフォルトドメイン作成失敗
        | DatabaseError of string                  // データベースエラー
        | SystemError of exn                       // システムエラー（例外情報付き）

    // 🔧 Railway-oriented Programming バインド演算子（要求仕様準拠）
    // 【F#初学者向け解説】
    // F#のカスタム演算子により、Result型のバインド操作を簡潔に記述できます。
    // >>= は Result.bind の中置記法で、成功時は次の処理を実行し、失敗時はエラーを伝播します。
    let (>>=) result func = Result.bind func result
    let (<!>) result func = Result.map func result

    // 🔍 プロジェクト名重複チェック: アクティブなプロジェクトでの名前重複を検証
    // 【F#初学者向け解説】
    // List.exists関数を使用して、既存のアクティブなプロジェクトで同名のものがないかチェックします。
    // 大文字小文字を区別しない比較により、ユーザビリティを向上させています。
    let validateUniqueProjectName (name: ProjectName) (existingProjects: Project list) : Result<unit, ProjectCreationError> =
        let isDuplicate =
            existingProjects
            |> List.exists (fun project ->
                project.IsActive &&
                System.String.Equals(project.Name.Value, name.Value, System.StringComparison.OrdinalIgnoreCase))

        if isDuplicate then
            Error (DuplicateProjectName "指定されたプロジェクト名は既に使用されています")
        else
            Ok ()

    // 🔧 プロジェクトとデフォルトドメインの同時作成: Railway-oriented Programming実装
    // 【F#初学者向け解説】
    // Result型を使用したRailway-oriented Programmingパターンの実装例です。
    // 各ステップが成功した場合のみ次のステップに進み、エラーが発生した場合は即座に処理を中断します。
    // タプル型(Project * Domain)により、作成されたプロジェクトとドメインの両方を返します。
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
            | Error err -> Error (DomainCreationFailed err)
            | Ok domain ->
                // Step 4: 成功時は両方を返す
                Ok (project, domain)

    // 🔧 Railway-oriented Programming を使った関数合成版
    // 【F#初学者向け解説】
    // Result.bindを使用した関数型プログラミングスタイルの実装です。
    // パイプライン演算子(|>)とResult.bindの組み合わせにより、
    // エラーハンドリングを含む処理の流れを読みやすく表現できます。
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
            |> Result.mapError DomainCreationFailed  // エラー型の変換
            |> Result.map (fun domain -> (project, domain))  // 成功時はタプルを作成
        )

    // 🔐 プロジェクト作成権限検証（要求仕様準拠版）
    // 【F#初学者向け解説】
    // 権限チェックを専用の関数として分離し、適切なエラー型を使用します。
    let validateProjectCreationPermission (operatorUser: User) : Result<unit, ProjectCreationError> =
        if not operatorUser.IsActive then
            Error (InsufficientPermissions "非アクティブなユーザーはプロジェクトを作成できません")
        elif not (PermissionMappings.hasPermission operatorUser.Role CreateProjects) then
            Error (InsufficientPermissions "プロジェクト作成の権限がありません")
        else
            Ok ()

    // 🔧 完全版プロジェクト作成（権限チェック統合・要求仕様完全準拠）
    // 【F#初学者向け解説】
    // Railway-oriented Programmingパターンを使用した完全版のプロジェクト作成関数です。
    // 権限チェック、重複チェック、プロジェクト作成、デフォルトドメイン作成を統合的に実行し、
    // バインド演算子（>>=）を使用してエラーハンドリングを簡潔に表現します。
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
            |> Result.mapError DomainCreationFailed
            |> Result.map (fun domain -> (project, domain))

    // 🔄 バインド操作による関数合成ヘルパー（要求仕様準拠）
    // 【F#初学者向け解説】
    // 複数のバリデーション関数を合成し、すべてが成功した場合のみ処理を継続します。
    // 関数型プログラミングの関数合成により、複雑な処理フローを簡潔に表現できます。
    let combineValidations (validations: (unit -> Result<unit, ProjectCreationError>) list) : Result<unit, ProjectCreationError> =
        validations
        |> List.fold (fun acc validation ->
            acc >>= fun _ -> validation ()
        ) (Ok ())

    // 📊 プロジェクト統計情報計算: ビジネスインテリジェンス機能
    // 【F#初学者向け解説】
    // List.filter、List.lengthなどのリスト操作関数を組み合わせて、
    // プロジェクトの統計情報を効率的に計算します。
    type ProjectStatistics = {
        TotalProjects: int
        ActiveProjects: int
        InactiveProjects: int
        ProjectsWithDomains: int
        AverageDomainsPerProject: float
    }

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
    let validateProjectNameChange
        (project: Project)
        (newName: ProjectName)
        (existingProjects: Project list)
        (operatorUser: User) : Result<unit, ProjectCreationError> =

        // 権限チェック
        if not (PermissionMappings.hasPermission operatorUser.Role ManageProjects) then
            Error (DatabaseError "プロジェクト管理の権限がありません")
        // 同名チェック（自分以外）
        elif existingProjects
            |> List.exists (fun p ->
                p.Id <> project.Id &&
                p.IsActive &&
                System.String.Equals(p.Name.Value, newName.Value, System.StringComparison.OrdinalIgnoreCase)) then
            Error (DuplicateProjectName "指定されたプロジェクト名は既に使用されています")
        else
            Ok ()

    // 🗑️ プロジェクト削除ビジネスルール: 関連データ確認
    // 【F#初学者向け解説】
    // プロジェクト削除前に関連するドメインやユビキタス言語の存在を確認し、
    // データの整合性を保つためのビジネスルールを実装します。
    let validateProjectDeletion
        (project: Project)
        (relatedDomains: Domain list)
        (operatorUser: User) : Result<unit, ProjectCreationError> =

        // 権限チェック
        if not (PermissionMappings.hasPermission operatorUser.Role DeleteProjects) then
            Error (DatabaseError "プロジェクト削除の権限がありません")
        // 関連ドメインの存在チェック
        elif relatedDomains |> List.exists (fun d -> d.IsActive) then
            Error (DatabaseError "アクティブなドメインが存在するプロジェクトは削除できません")
        else
            Ok ()