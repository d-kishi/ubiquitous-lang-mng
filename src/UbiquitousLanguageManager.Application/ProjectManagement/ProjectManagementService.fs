namespace UbiquitousLanguageManager.Application.ProjectManagement

open System
open System.Threading.Tasks
// F# Domain層namespace階層化対応: Bounded Context別にopen
open UbiquitousLanguageManager.Domain.Common                  // UserId, ProjectId, Role
open UbiquitousLanguageManager.Domain.Authentication          // User
open UbiquitousLanguageManager.Domain.ProjectManagement       // Project, ProjectName, ProjectDescription, Domain, ProjectDomainService, ProjectCreationError, ErrorConversions
open UbiquitousLanguageManager.Domain.UbiquitousLanguageManagement  // (使用なし)

// 🎯 Phase B1 Step3: プロジェクト管理サービス実装
// IProjectManagementService・Command/Query分離・Railway-oriented Programming統合
// Step2 ProjectDomainService基盤活用・権限制御マトリックス完全実装
// 【F#初学者向け解説】
// このサービスは、Clean Architecture Application層の核心実装です。
// ドメインロジックとInfrastructure層を協調させ、ビジネス要件を満たす
// ユースケースを実行します。F#のRailway-oriented Programmingパターンにより、
// エラーハンドリングを型安全かつ明示的に処理します。

type ProjectManagementService(
    projectRepository: IProjectRepository,
    domainRepository: IDomainRepository,
    userRepository: IUserRepository) =

    // 🔧 Railway-oriented Programming バインド演算子定義
    // 【F#初学者向け解説】
    // F#のカスタム演算子により、Result型の連鎖処理を簡潔に記述できます。
    // >>= は Result.bind の中置記法で、Technical_Research_Results.md の推奨パターンです。
    let (>>=) result func = Result.bind func result
    let (<!>) result func = Result.map func result

    interface IProjectManagementService with

        // 📁 プロジェクト作成: デフォルトドメイン自動作成・原子性保証実装
        // REQ-3.1.2・REQ-3.1.2-1・REQ-3.1.2-2準拠
        member this.CreateProjectAsync(command: CreateProjectCommand) =
            task {
                // Step 1: Command値のDomain型変換（Smart Constructor適用）
                match command.toDomainTypes() with
                | Error err -> return Error err
                | Ok (projectName, projectDescription, ownerId, operatorId) ->

                    // Step 2: 操作者権限確認（権限チェック統合）
                    let! operatorResult = userRepository.GetByIdAsync(operatorId)
                    match operatorResult with
                    | Error err -> return Error err
                    | Ok operatorOpt ->
                        match operatorOpt with
                        | None -> return Error "操作者が見つかりません"
                        | Some operatorUser ->

                            // Step 3: 既存プロジェクト取得（重複チェック用）
                            let! existingResult = projectRepository.GetByOwnerAsync(ownerId)
                            match existingResult with
                            | Error err -> return Error err
                            | Ok existingProjects ->

                                // Step 4: ProjectDomainService活用（Step2基盤統合）
                                // Railway-oriented Programming実装・Technical_Research_Results準拠
                                let domainResult =
                                    ProjectDomainService.createProjectWithPermissionCheck
                                        projectName
                                        projectDescription
                                        ownerId
                                        operatorUser
                                        existingProjects

                                match domainResult with
                                | Error (ProjectCreationError.InsufficientPermissions msg) ->
                                    return Error $"権限エラー: {msg}"
                                | Error (ProjectCreationError.DuplicateProjectName msg) ->
                                    return Error $"重複エラー: {msg}"
                                | Error (ProjectCreationError.DomainCreationFailed msg) ->
                                    return Error $"デフォルトドメイン作成エラー: {msg}"
                                | Error (ProjectCreationError.SystemError ex) ->
                                    return Error $"システムエラー: {ex.Message}"
                                | Error err ->
                                    return Error (ErrorConversions.getProjectCreationErrorMessage err)
                                | Ok (project, domain) ->

                                    // Step 5: 原子性保証による永続化（REQ-3.1.2-2準拠）
                                    // 【Phase B2拡張】Owner自動UserProjects追加統合
                                    // SaveProjectWithDefaultDomainAndOwnerAsync:
                                    //   - ProjectsレコードINSERT
                                    //   - DomainsレコードINSERT（デフォルトドメイン）
                                    //   - UserProjectsレコードINSERT（Owner自動追加）
                                    //   - トランザクション境界（同一トランザクション）
                                    let! saveResult = projectRepository.SaveProjectWithDefaultDomainAndOwnerAsync(project, domain, ownerId)
                                    match saveResult with
                                    | Error err -> return Error err
                                    | Ok (savedProject, savedDomain) ->

                                        // Step 6: 成功結果のDTO作成
                                        let resultDto = {
                                            Project = savedProject
                                            DefaultDomain = savedDomain
                                            CreatedAt = DateTime.UtcNow
                                        }
                                        return Ok resultDto
            }

        // 📝 プロジェクト更新: 説明のみ編集可能（プロジェクト名変更禁止）
        // REQ-3.1.3・PROHIBITION-3.3.1-1準拠
        member this.UpdateProjectAsync(command: UpdateProjectCommand) =
            task {
                // Step 1: Command値のDomain型変換
                match command.toDomainTypes() with
                | Error err -> return Error err
                | Ok (projectId, newDescription, operatorId) ->

                    // Step 2: 更新対象プロジェクト取得
                    let! projectResult = projectRepository.GetByIdAsync(projectId)
                    match projectResult with
                    | Error err -> return Error err
                    | Ok projectOpt ->
                        match projectOpt with
                        | None -> return Error "指定されたプロジェクトが見つかりません"
                        | Some project ->

                            // Step 3: 操作者権限確認
                            let! operatorResult = userRepository.GetByIdAsync(operatorId)
                            match operatorResult with
                            | Error err -> return Error err
                            | Ok operatorOpt ->
                                match operatorOpt with
                                | None -> return Error "操作者が見つかりません"
                                | Some operatorUser ->

                                    // Step 4: 権限チェック（権限制御マトリックス適用）
                                    if not (ProjectQueryPermissions.canEditProject operatorUser.Role operatorId project) then
                                        return Error "プロジェクト編集の権限がありません"
                                    else

                                        // Step 5: ドメインロジック実行（説明変更）
                                        match project.changeDescription newDescription operatorId with
                                        | Error err -> return Error err
                                        | Ok updatedProject ->

                                            // Step 6: 永続化
                                            let! saveResult = projectRepository.SaveAsync(updatedProject)
                                            return saveResult
            }

        // 🗑️ プロジェクト削除: 論理削除・関連データ影響確認・スーパーユーザーのみ
        // REQ-3.1.4・PROHIBITION-3.3.1-2準拠
        member this.DeleteProjectAsync(command: DeleteProjectCommand) =
            task {
                let (projectId, operatorId) = command.toDomainTypes()

                // Step 1: 削除対象プロジェクト取得
                let! projectResult = projectRepository.GetByIdAsync(projectId)
                match projectResult with
                | Error err -> return Error err
                | Ok projectOpt ->
                    match projectOpt with
                    | None -> return Error "指定されたプロジェクトが見つかりません"
                    | Some project ->

                        // Step 2: 操作者権限確認（スーパーユーザーのみ）
                        let! operatorResult = userRepository.GetByIdAsync(operatorId)
                        match operatorResult with
                        | Error err -> return Error err
                        | Ok operatorOpt ->
                            match operatorOpt with
                            | None -> return Error "操作者が見つかりません"
                            | Some operatorUser ->

                                // Step 3: 削除権限チェック（スーパーユーザーのみ）
                                if not (ProjectQueryPermissions.canDeleteProject operatorUser.Role) then
                                    return Error "プロジェクト削除はスーパーユーザーのみが実行できます"
                                else

                                    // Step 4: 関連データ確認（PROHIBITION-3.3.1-2準拠）
                                    let! relatedDataResult = projectRepository.GetRelatedDataCountAsync(projectId)
                                    match relatedDataResult with
                                    | Error err -> return Error err
                                    | Ok relatedDataCount ->

                                        if relatedDataCount > 0 && not command.ConfirmRelatedDataDeletion then
                                            return Error $"このプロジェクトには{relatedDataCount}件の関連データが存在します。削除を実行する場合は確認フラグを設定してください。"
                                        else

                                            // Step 5: 関連ドメイン取得・削除前検証
                                            let! domainsResult = domainRepository.GetByProjectIdAsync(projectId)
                                            match domainsResult with
                                            | Error err -> return Error err
                                            | Ok relatedDomains ->

                                                // Step 6: ProjectDomainService削除検証活用
                                                match ProjectDomainService.validateProjectDeletion project relatedDomains operatorUser with
                                                | Error (ProjectCreationError.DatabaseError msg) -> return Error msg
                                                | Error err -> return Error (ErrorConversions.getProjectCreationErrorMessage err)
                                                | Ok () ->

                                                    // Step 7: ドメインロジック実行（論理削除）
                                                    match project.deactivate operatorUser operatorId with
                                                    | Error err -> return Error err
                                                    | Ok deactivatedProject ->

                                                        // Step 8: 永続化
                                                        let! saveResult = projectRepository.SaveAsync(deactivatedProject)
                                                        match saveResult with
                                                        | Ok _ -> return Ok ()
                                                        | Error err -> return Error err
            }

        // 👤 プロジェクト所有者変更
        member this.ChangeProjectOwnerAsync(command: ChangeProjectOwnerCommand) =
            task {
                let (projectId, newOwnerId, operatorId) = command.toDomainTypes()

                // Step 1: プロジェクト取得
                let! projectResult = projectRepository.GetByIdAsync(projectId)
                match projectResult with
                | Error err -> return Error err
                | Ok projectOpt ->
                    match projectOpt with
                    | None -> return Error "指定されたプロジェクトが見つかりません"
                    | Some project ->

                        // Step 2: 操作者・新所有者取得
                        let! operatorResult = userRepository.GetByIdAsync(operatorId)
                        let! newOwnerResult = userRepository.GetByIdAsync(newOwnerId)

                        match operatorResult, newOwnerResult with
                        | Error err, _ | _, Error err -> return Error err
                        | Ok operatorOpt, Ok newOwnerOpt ->
                            match operatorOpt, newOwnerOpt with
                            | None, _ -> return Error "操作者が見つかりません"
                            | _, None -> return Error "新しい所有者が見つかりません"
                            | Some operatorUser, Some newOwner ->

                                // Step 3: ドメインロジック実行
                                match project.changeOwner newOwnerId operatorUser operatorId with
                                | Error err -> return Error err
                                | Ok updatedProject ->

                                    // Step 4: 永続化
                                    let! saveResult = projectRepository.SaveAsync(updatedProject)
                                    return saveResult
            }

        // ✅ プロジェクト有効化
        member this.ActivateProjectAsync(command: ActivateProjectCommand) =
            task {
                let (projectId, operatorId) = command.toDomainTypes()

                // Step 1: プロジェクト取得
                let! projectResult = projectRepository.GetByIdAsync(projectId)
                match projectResult with
                | Error err -> return Error err
                | Ok projectOpt ->
                    match projectOpt with
                    | None -> return Error "指定されたプロジェクトが見つかりません"
                    | Some project ->

                        // Step 2: 操作者権限確認
                        let! operatorResult = userRepository.GetByIdAsync(operatorId)
                        match operatorResult with
                        | Error err -> return Error err
                        | Ok operatorOpt ->
                            match operatorOpt with
                            | None -> return Error "操作者が見つかりません"
                            | Some operatorUser ->

                                // Step 3: ドメインロジック実行
                                match project.activate operatorUser operatorId with
                                | Error err -> return Error err
                                | Ok activatedProject ->

                                    // Step 4: 永続化
                                    let! saveResult = projectRepository.SaveAsync(activatedProject)
                                    return saveResult
            }

        // 📋 Query側実装: プロジェクト一覧取得（権限制御統合）
        // REQ-3.1.1・REQ-10.2.1準拠
        member this.GetProjectsAsync(query: GetProjectsQuery) =
            task {
                // Step 1: Query値のDomain型変換・バリデーション
                match query.toDomainTypes() with
                | Error err -> return Error err
                | Ok (userId, userRole) ->

                    // Step 2: 権限チェック（権限制御マトリックス適用）
                    if not (ProjectQueryPermissions.canViewProjectList userRole) then
                        return Error "プロジェクト一覧の表示権限がありません"
                    else

                        // Step 3: 権限制御済み一覧取得（Repository委譲）
                        let! result = projectRepository.GetProjectsWithPermissionAsync(
                            userId, userRole, query.PageNumber, query.PageSize, query.IncludeInactive)
                        return result
            }

        // 🔍 プロジェクト詳細取得（権限チェック統合）
        // 【Phase B2拡張】UserCount実装・メンバー判定追加
        member this.GetProjectDetailAsync(query: GetProjectDetailQuery) =
            task {
                let (projectId, userId, userRole) = query.toDomainTypes()

                // Step 1: プロジェクト取得
                let! projectResult = projectRepository.GetByIdAsync(projectId)
                match projectResult with
                | Error err -> return Error err
                | Ok projectOpt ->
                    match projectOpt with
                    | None -> return Error "指定されたプロジェクトが見つかりません"
                    | Some project ->

                        // Step 2: メンバー判定（Phase B2拡張: DomainApprover/GeneralUser権限チェック用）
                        let! isMemberResult = projectRepository.IsUserProjectMemberAsync(userId, projectId)
                        match isMemberResult with
                        | Error err -> return Error err
                        | Ok isMember ->

                            // Step 3: 権限チェック（Phase B2拡張: メンバー判定統合）
                            if not (ProjectQueryPermissions.canViewProjectDetail userRole userId project isMember) then
                                return Error "プロジェクト詳細の表示権限がありません"
                            else

                                // Step 4: 関連データ数取得（Phase B2拡張: UserCount追加）
                                let! relatedDataResult = projectRepository.GetRelatedDataCountAsync(projectId)
                                let! domainsResult = domainRepository.GetByProjectIdAsync(projectId)
                                let! memberCountResult = projectRepository.GetProjectMemberCountAsync(projectId)

                                match relatedDataResult, domainsResult, memberCountResult with
                                | Error err, _, _ | _, Error err, _ | _, _, Error err -> return Error err
                                | Ok relatedCount, Ok domains, Ok memberCount ->

                                    // Step 5: 権限フラグ設定
                                    let canEdit = ProjectQueryPermissions.canEditProject userRole userId project
                                    let canDelete = ProjectQueryPermissions.canDeleteProject userRole

                                    // Step 6: 詳細結果DTO作成（Phase B2拡張: UserCount実装）
                                    let detailResult = {
                                        Project = project
                                        UserCount = memberCount  // Phase B2拡張: GetProjectMemberCountAsync統合
                                        DomainCount = List.length domains
                                        UbiquitousLanguageCount = relatedCount
                                        CanEdit = canEdit
                                        CanDelete = canDelete
                                    }
                                    return Ok detailResult
            }

        // 👥 プロジェクトユーザー一覧取得（実装簡略化）
        member this.GetProjectUsersAsync(query: GetProjectUsersQuery) =
            task {
                return Error "プロジェクトユーザー管理機能は今後のPhaseで実装予定です"
            }

        // 🏷️ プロジェクトドメイン一覧取得
        // 【Phase B2拡張】メンバー判定追加
        member this.GetProjectDomainsAsync(query: GetProjectDomainsQuery) =
            task {
                let (projectId, userId, userRole) = query.toDomainTypes()

                // Step 1: プロジェクト存在確認
                let! projectResult = projectRepository.GetByIdAsync(projectId)
                match projectResult with
                | Error err -> return Error err
                | Ok projectOpt ->
                    match projectOpt with
                    | None -> return Error "指定されたプロジェクトが見つかりません"
                    | Some project ->

                        // Step 2: メンバー判定（Phase B2拡張: DomainApprover/GeneralUser権限チェック用）
                        let! isMemberResult = projectRepository.IsUserProjectMemberAsync(userId, projectId)
                        match isMemberResult with
                        | Error err -> return Error err
                        | Ok isMember ->

                            // Step 3: 権限チェック（Phase B2拡張: メンバー判定統合）
                            if not (ProjectQueryPermissions.canViewProjectDetail userRole userId project isMember) then
                                return Error "プロジェクトドメイン一覧の表示権限がありません"
                            else

                                // Step 4: ドメイン一覧取得
                                let! domainsResult = domainRepository.GetByProjectIdAsync(projectId)
                                match domainsResult with
                                | Error err -> return Error err
                                | Ok domains ->
                                    // アクティブドメインのみフィルタリング（必要に応じて）
                                    let filteredDomains =
                                        if query.IncludeInactive then domains
                                        else domains |> List.filter (fun d -> d.IsActive)
                                    return Ok filteredDomains
            }

        // 📊 ユーザー別プロジェクト一覧取得（実装簡略化）
        member this.GetUserProjectsAsync(query: GetUserProjectsQuery) =
            task {
                return Error "ユーザー別プロジェクト管理機能は今後のPhaseで実装予定です"
            }

        // 🔍 プロジェクト検索
        member this.SearchProjectsAsync(query: SearchProjectsQuery) =
            task {
                // Step 1: Query値のDomain型変換・バリデーション
                match query.toDomainTypes() with
                | Error err -> return Error err
                | Ok (userId, userRole, ownerIdOpt) ->

                    // Step 2: 権限チェック
                    if not (ProjectQueryPermissions.canViewProjectList userRole) then
                        return Error "プロジェクト検索の権限がありません"
                    else

                        // Step 3: Repository委譲検索
                        let! searchResult = projectRepository.SearchProjectsAsync(query)
                        return searchResult
            }

        // 📊 プロジェクト統計情報取得
        member this.GetProjectStatisticsAsync(query: GetProjectStatisticsQuery) : Task<ProjectStatisticsResult> =
            task {
                let (userId, userRole, projectIdOpt) = query.toDomainTypes()

                // Step 1: 権限チェック（管理者系ロールのみ統計情報アクセス可能）
                if userRole = DomainApprover || userRole = GeneralUser then
                    return Error "統計情報の表示権限がありません"
                else
                    // Step 2: 統計データ計算（ProjectDomainService活用）
                    // 実装簡略化: 基本統計のみ
                    let! allProjectsResult = projectRepository.GetByOwnerAsync(userId)
                    match allProjectsResult with
                    | Error err -> return Error err
                    | Ok projects ->

                        let! allDomainsResult =
                            task {
                                let mutable allDomains = []
                                for project in projects do
                                    let! domains = domainRepository.GetByProjectIdAsync(project.Id)
                                    match domains with
                                    | Ok domainList -> allDomains <- List.append allDomains domainList
                                    | Error _ -> ()
                                return Ok allDomains
                            }

                        match allDomainsResult with
                        | Error err -> return Error err
                        | Ok domains ->

                            // ProjectDomainService統計計算活用
                            let statistics = ProjectDomainService.calculateProjectStatistics projects domains

                            let statisticsDto = {
                                TotalProjects = statistics.TotalProjects
                                ActiveProjects = statistics.ActiveProjects
                                InactiveProjects = statistics.InactiveProjects
                                ProjectsWithDomains = statistics.ProjectsWithDomains
                                AverageDomainsPerProject = statistics.AverageDomainsPerProject
                                TotalUbiquitousLanguages = 0  // 実装簡略化
                                ProjectsByOwner = []          // 実装簡略化
                            }
                            return Ok statisticsDto
            }

        // 👥 Phase B2: UserProjects多対多関連管理メソッド実装

        // メンバー追加: Railway-oriented Programming適用・権限制御・重複チェック統合
        member this.AddMemberToProjectAsync(command: AddMemberToProjectCommand) =
            task {
                // Step 1: Command値のDomain型変換
                match command.toDomainTypes() with
                | Error err -> return Error err
                | Ok (projectId, userId, operatorId, operatorRole) ->

                    // Step 2: プロジェクト取得（権限チェック用）
                    let! projectResult = projectRepository.GetByIdAsync(projectId)
                    match projectResult with
                    | Error err -> return Error err
                    | Ok projectOpt ->
                        match projectOpt with
                        | None -> return Error "指定されたプロジェクトが見つかりません"
                        | Some project ->

                            // Step 3: 権限チェック（権限制御マトリックス適用）
                            // 【F#初学者向け解説】
                            // Phase B2権限制御マトリックス拡張:
                            // - SuperUser: 全プロジェクトに対してメンバー追加可能
                            // - ProjectManager: 担当プロジェクトのみメンバー追加可能（UserProjects判定必須）
                            // - DomainApprover/GeneralUser: メンバー追加権限なし

                            // Step 3-1: 基本権限チェック（ロールベース）
                            if not (ProjectQueryPermissions.canAddMember operatorRole operatorId project) then
                                return Error "プロジェクトメンバー追加の権限がありません"
                            else
                                // Step 3-2: ProjectManager権限の場合、メンバー判定追加
                                // 【Phase B2拡張】ProjectManagerは担当プロジェクト（UserProjectsメンバー）のみ操作可能
                                let! hasPermission =
                                    task {
                                        match operatorRole with
                                        | ProjectManager ->
                                            // UserProjectsテーブルでメンバー判定
                                            let! isMemberResult = projectRepository.IsUserProjectMemberAsync(operatorId, projectId)
                                            match isMemberResult with
                                            | Error err -> return Error err
                                            | Ok isMember -> return Ok isMember
                                        | SuperUser -> return Ok true  // SuperUserは全プロジェクト操作可能
                                        | _ -> return Ok false  // その他ロールは権限なし
                                    }

                                match hasPermission with
                                | Error err -> return Error err
                                | Ok false -> return Error "プロジェクトメンバー追加の権限がありません"
                                | Ok true ->

                                    // Step 4: 追加対象ユーザー存在確認
                                    let! userResult = userRepository.GetByIdAsync(userId)
                                    match userResult with
                                    | Error err -> return Error err
                                    | Ok userOpt ->
                                        match userOpt with
                                        | None -> return Error "追加対象のユーザーが見つかりません"
                                        | Some _user ->

                                            // Step 5: Repository委譲（重複チェック・永続化）
                                            // 【F#初学者向け解説】
                                            // Infrastructure層AddUserToProjectAsyncで重複チェック・
                                            // 複合一意制約違反エラーハンドリングを実施済み
                                            let! addResult = projectRepository.AddUserToProjectAsync(userId, projectId, operatorId)
                                            return addResult
            }

        // メンバー削除: Railway-oriented Programming適用・最後の管理者削除防止チェック
        member this.RemoveMemberFromProjectAsync(command: RemoveMemberFromProjectCommand) =
            task {
                // Step 1: Command値のDomain型変換
                match command.toDomainTypes() with
                | Error err -> return Error err
                | Ok (projectId, userId, operatorId, operatorRole) ->

                    // Step 2: プロジェクト取得（権限チェック用）
                    let! projectResult = projectRepository.GetByIdAsync(projectId)
                    match projectResult with
                    | Error err -> return Error err
                    | Ok projectOpt ->
                        match projectOpt with
                        | None -> return Error "指定されたプロジェクトが見つかりません"
                        | Some project ->

                            // Step 3: 権限チェック（権限制御マトリックス適用）
                            // Step 3-1: 基本権限チェック（ロールベース）
                            if not (ProjectQueryPermissions.canRemoveMember operatorRole operatorId project) then
                                return Error "プロジェクトメンバー削除の権限がありません"
                            else
                                // Step 3-2: ProjectManager権限の場合、メンバー判定追加
                                // 【Phase B2拡張】ProjectManagerは担当プロジェクト（UserProjectsメンバー）のみ操作可能
                                let! hasPermission =
                                    task {
                                        match operatorRole with
                                        | ProjectManager ->
                                            // UserProjectsテーブルでメンバー判定
                                            let! isMemberResult = projectRepository.IsUserProjectMemberAsync(operatorId, projectId)
                                            match isMemberResult with
                                            | Error err -> return Error err
                                            | Ok isMember -> return Ok isMember
                                        | SuperUser -> return Ok true  // SuperUserは全プロジェクト操作可能
                                        | _ -> return Ok false  // その他ロールは権限なし
                                    }

                                match hasPermission with
                                | Error err -> return Error err
                                | Ok false -> return Error "プロジェクトメンバー削除の権限がありません"
                                | Ok true ->

                                    // Step 4: 削除対象ユーザー存在確認
                                    let! userResult = userRepository.GetByIdAsync(userId)
                                    match userResult with
                                    | Error err -> return Error err
                                    | Ok userOpt ->
                                        match userOpt with
                                        | None -> return Error "削除対象のユーザーが見つかりません"
                                        | Some targetUser ->

                                            // Step 5: 最後の管理者削除防止チェック
                                            // 【F#初学者向け解説】
                                            // Infrastructure層申し送り事項:
                                            // AspNetUserRoles参照でProjectManager判定
                                            // （Infrastructure層実装により、ここではロールチェックを簡略化）
                                            // 実装簡略化: 削除対象ユーザーがProjectManagerの場合のみチェック
                                            if targetUser.Role = ProjectManager then
                                                // プロジェクトメンバー一覧取得
                                                let! membersResult = projectRepository.GetProjectMembersAsync(projectId)
                                                match membersResult with
                                                | Error err -> return Error err
                                                | Ok members ->
                                                    // メンバー数が1名の場合は削除禁止
                                                    if List.length members = 1 then
                                                        return Error "プロジェクトには最低1名のプロジェクト管理者が必要です"
                                                    else
                                                        // Step 6: Repository委譲（永続化）
                                                        let! removeResult = projectRepository.RemoveUserFromProjectAsync(userId, projectId)
                                                        return removeResult
                                            else
                                                // Step 6: Repository委譲（永続化）
                                                let! removeResult = projectRepository.RemoveUserFromProjectAsync(userId, projectId)
                                                return removeResult
            }

        // プロジェクトメンバー一覧取得: 権限制御マトリックス統合
        member this.GetProjectMembersAsync(query: GetProjectMembersQuery) =
            task {
                let (projectId, userId, userRole) = query.toDomainTypes()

                // Step 1: プロジェクト存在確認
                let! projectResult = projectRepository.GetByIdAsync(projectId)
                match projectResult with
                | Error err -> return Error err
                | Ok projectOpt ->
                    match projectOpt with
                    | None -> return Error "指定されたプロジェクトが見つかりません"
                    | Some project ->

                        // Step 2: メンバー判定（権限チェック用）
                        let! isMemberResult = projectRepository.IsUserProjectMemberAsync(userId, projectId)
                        match isMemberResult with
                        | Error err -> return Error err
                        | Ok isMember ->

                            // Step 3: 権限チェック（Phase B2拡張: DomainApprover/GeneralUser対応）
                            // 【F#初学者向け解説】
                            // 権限制御マトリックス:
                            // - SuperUser: 全プロジェクトのメンバー一覧表示可能
                            // - ProjectManager: 担当プロジェクトのみメンバー一覧表示可能
                            // - DomainApprover/GeneralUser: 所属プロジェクトのみメンバー一覧表示可能
                            if not (ProjectQueryPermissions.canViewProjectMembers userRole userId project isMember) then
                                return Error "プロジェクトメンバー一覧の表示権限がありません"
                            else

                                // Step 4: Repository委譲（メンバー一覧取得）
                                let! membersResult = projectRepository.GetProjectMembersAsync(projectId)
                                return membersResult
            }

        // プロジェクトメンバー判定: Infrastructure層委譲
        member this.IsUserProjectMemberAsync(userId: UserId, projectId: ProjectId) =
            task {
                // Infrastructure層IsUserProjectMemberAsync直接委譲
                // 【F#初学者向け解説】
                // 権限制御マトリックス統合用のヘルパーメソッドです。
                // GetProjectsAsync・GetProjectDetailAsyncなどでメンバー判定が必要な場合に活用します。
                let! result = projectRepository.IsUserProjectMemberAsync(userId, projectId)
                return result
            }