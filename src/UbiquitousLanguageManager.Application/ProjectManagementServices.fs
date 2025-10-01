namespace UbiquitousLanguageManager.Application

open Microsoft.Extensions.Logging
open System.Threading.Tasks
// F# Domain層namespace階層化対応: Bounded Context別にopen
open UbiquitousLanguageManager.Domain.Common                  // UserId, Role, Permission, PermissionMappings
open UbiquitousLanguageManager.Domain.Authentication          // User
open UbiquitousLanguageManager.Domain.ProjectManagement       // Project, ProjectName, ProjectDescription, Domain, ProjectDomainService, ProjectError
open UbiquitousLanguageManager.Domain.UbiquitousLanguageManagement  // (使用なし)
open UbiquitousLanguageManager.Application.ProjectManagement

// 🎯 Phase B1 Step3: プロジェクト管理アプリケーションサービス実装
// TDD Green Phase: Application層実装
// 【F#初学者向け解説】
// ProjectManagementApplicationServiceは、プロジェクト管理のユースケースを実装します。
// Railway-oriented Programmingパターンと権限制御マトリックスを統合し、
// ドメインロジックとInfrastructure層を仲介します。

type ProjectManagementApplicationService(
    projectRepository: IProjectRepository,
    domainRepository: IDomainRepository,
    userRepository: IUserRepository,
    logger: ILogger<ProjectManagementApplicationService>) =

    // 📁 プロジェクト作成（デフォルトドメイン自動作成付き）
    // REQ-3.1.2準拠: 原子性保証・権限制御・重複チェック統合
    member this.CreateProjectAsync(command: CreateProjectCommand, operatorUserId: UserId) =
        task {
            logger.LogInformation("プロジェクト作成開始: {Name}", command.Name)

            // 🔐 Step 1: 操作者権限確認
            let! operatorResult = userRepository.GetByIdAsync(operatorUserId)

            match operatorResult with
            | Error err ->
                logger.LogError("操作者取得エラー: {Error}", err)
                return Error $"操作者取得エラー: {err}"
            | Ok operatorOpt ->
                match operatorOpt with
                | None ->
                    logger.LogWarning("操作者が見つかりません: {UserId}", operatorUserId)
                    return Error "操作者が見つかりません"
                | Some operatorUser ->
                    // 🔐 Step 2: プロジェクト作成権限チェック
                    if not (PermissionMappings.hasPermission operatorUser.Role CreateProjects) then
                        logger.LogWarning("権限不足 - ユーザー: {UserId}, ロール: {Role}", operatorUserId, operatorUser.Role)
                        return Error "プロジェクト作成の権限がありません"
                    else
                        // 🔧 Step 3: Command検証・Domain型変換
                        match command.toDomainTypes() with
                        | Error validationErr ->
                            logger.LogWarning("Command検証エラー: {Error}", validationErr)
                            return Error validationErr
                        | Ok (projectName, projectDescription, ownerId, _) ->
                            // 🔍 Step 4: 既存プロジェクト重複チェック
                            let! existingProjectsResult = projectRepository.GetByOwnerAsync(ownerId)

                            match existingProjectsResult with
                            | Error err ->
                                logger.LogError("既存プロジェクト取得エラー: {Error}", err)
                                return Error $"既存プロジェクト取得エラー: {err}"
                            | Ok existingProjects ->
                                // ドメインサービス: 重複チェック
                                match ProjectDomainService.validateUniqueProjectName projectName existingProjects with
                                | Error duplicateErr ->
                                    logger.LogWarning("プロジェクト名重複: {Name}", command.Name)
                                    return Error duplicateErr
                                | Ok () ->
                                    // 🔧 Step 5: プロジェクト＆デフォルトドメイン作成
                                    match ProjectDomainService.createProjectWithDefaultDomain projectName projectDescription ownerId existingProjects with
                                    | Error createErr ->
                                        logger.LogError("プロジェクト作成エラー: {Error}", createErr)
                                        return Error createErr
                                    | Ok (newProject, defaultDomain) ->
                                        // 💾 Step 6: 原子性保証での永続化
                                        let! saveResult = projectRepository.SaveProjectWithDomainAsync(newProject, defaultDomain)

                                        match saveResult with
                                        | Error saveErr ->
                                            logger.LogError("プロジェクト保存エラー: {Error}", saveErr)
                                            return Error $"プロジェクト保存エラー: {saveErr}"
                                        | Ok (savedProject, savedDomain) ->
                                            logger.LogInformation("プロジェクト作成成功: {ProjectId}, {Name}", savedProject.Id, savedProject.Name.Value)

                                            // 🎯 成功結果DTO作成
                                            let resultDto = {
                                                Project = savedProject
                                                DefaultDomain = savedDomain
                                                CreatedAt = savedProject.CreatedAt
                                            }
                                            return Ok resultDto
        }

    // 📝 プロジェクト編集（説明のみ変更可能）
    // REQ-3.1.3・PROHIBITION-3.3.1-1準拠: プロジェクト名変更禁止
    member this.UpdateProjectAsync(command: UpdateProjectCommand, operatorUserId: UserId) =
        task {
            logger.LogInformation("プロジェクト更新開始: {ProjectId}", command.ProjectId)

            // 🔐 Step 1: 操作者権限確認
            let! operatorResult = userRepository.GetByIdAsync(operatorUserId)

            match operatorResult with
            | Error err -> return Error $"操作者取得エラー: {err}"
            | Ok operatorOpt ->
                match operatorOpt with
                | None -> return Error "操作者が見つかりません"
                | Some operatorUser ->
                    // 🔧 Step 2: Command検証・Domain型変換
                    match command.toDomainTypes() with
                    | Error validationErr -> return Error validationErr
                    | Ok (projectId, newDescription, _) ->
                        // 🔍 Step 3: 対象プロジェクト取得
                        let! projectResult = projectRepository.GetByIdAsync(projectId)

                        match projectResult with
                        | Error err -> return Error $"プロジェクト取得エラー: {err}"
                        | Ok projectOpt ->
                            match projectOpt with
                            | None -> return Error "指定されたプロジェクトが見つかりません"
                            | Some existingProject ->
                                // 🔐 Step 4: 編集権限チェック（プロジェクト管理権限またはスーパーユーザー）
                                if not (PermissionMappings.hasPermission operatorUser.Role ManageProjects) then
                                    return Error "プロジェクト編集の権限がありません"
                                else
                                    // 🔧 Step 5: プロジェクト説明変更
                                    match existingProject.changeDescription newDescription operatorUserId with
                                    | Error changeErr -> return Error changeErr
                                    | Ok updatedProject ->
                                        // 💾 Step 6: 変更の永続化
                                        let! saveResult = projectRepository.SaveAsync(updatedProject)

                                        match saveResult with
                                        | Error saveErr -> return Error $"プロジェクト更新保存エラー: {saveErr}"
                                        | Ok savedProject ->
                                            logger.LogInformation("プロジェクト更新成功: {ProjectId}", savedProject.Id)
                                            return Ok savedProject
        }

    // 🔍 ユーザー別プロジェクト一覧取得（権限制御統合）
    // REQ-10.2.1準拠: 権限マトリックス適用
    member this.GetProjectsByUserAsync(requestUserId: UserId) =
        task {
            logger.LogInformation("プロジェクト一覧取得開始: {UserId}", requestUserId)

            // 🔐 Step 1: 要求ユーザー取得・権限確認
            let! userResult = userRepository.GetByIdAsync(requestUserId)

            match userResult with
            | Error err -> return Error $"ユーザー取得エラー: {err}"
            | Ok userOpt ->
                match userOpt with
                | None -> return Error "ユーザーが見つかりません"
                | Some user ->
                    // 🔐 Step 2: 権限マトリックスによる表示制御
                    match user.Role with
                    | SuperUser ->
                        // スーパーユーザー: 全プロジェクト取得
                        logger.LogInformation("スーパーユーザーによる全プロジェクト取得: {UserId}", requestUserId)
                        let! allProjectsResult = projectRepository.GetAllActiveAsync()

                        match allProjectsResult with
                        | Error err -> return Error $"全プロジェクト取得エラー: {err}"
                        | Ok projects ->
                            // プロジェクトDTO変換
                            let projectDtos =
                                projects
                                |> List.map (fun p -> {
                                    Id = p.Id.Value |> System.Guid
                                    Name = p.Name.Value
                                    Description = p.Description.Value
                                    OwnerId = p.OwnerId.Value
                                    CreatedAt = p.CreatedAt
                                    UpdatedAt = p.UpdatedAt
                                    IsActive = p.IsActive
                                })
                            return Ok projectDtos

                    | ProjectManager ->
                        // プロジェクト管理者: 自分のプロジェクトのみ
                        logger.LogInformation("プロジェクト管理者による自プロジェクト取得: {UserId}", requestUserId)
                        let! ownProjectsResult = projectRepository.GetByOwnerAsync(requestUserId)

                        match ownProjectsResult with
                        | Error err -> return Error $"所有プロジェクト取得エラー: {err}"
                        | Ok projects ->
                            let projectDtos =
                                projects
                                |> List.map (fun p -> {
                                    Id = p.Id.Value |> System.Guid
                                    Name = p.Name.Value
                                    Description = p.Description.Value
                                    OwnerId = p.OwnerId.Value
                                    CreatedAt = p.CreatedAt
                                    UpdatedAt = p.UpdatedAt
                                    IsActive = p.IsActive
                                })
                            return Ok projectDtos

                    | DomainApprover | GeneralUser ->
                        // ドメイン承認者・一般ユーザー: プロジェクト一覧参照権限なし
                        logger.LogWarning("プロジェクト一覧参照権限なし - ユーザー: {UserId}, ロール: {Role}", requestUserId, user.Role)
                        return Error "プロジェクト一覧参照の権限がありません"
        }

// 📊 プロジェクトDTO定義（Application層境界用）
// 【F#初学者向け解説】
// DTOは境界層でのデータ交換用オブジェクトです。
// Domain層のエンティティから必要な情報のみを抽出し、
// UI層やAPI層への安全なデータ転送を実現します。
type ProjectDto = {
    Id: System.Guid
    Name: string
    Description: string
    OwnerId: int64
    CreatedAt: System.DateTime
    UpdatedAt: System.DateTime option
    IsActive: bool
}

// 🎯 Application層エラー定義
// 【F#初学者向け解説】
// Application層特有のエラーを型安全に表現します。
// Domain層エラーとInfrastructure層エラーを統一し、
// UI層でのエラーハンドリングを明確にします。
type ApplicationError =
    | ValidationError of string      // Command検証エラー
    | PermissionDenied of string     // 権限不足エラー
    | ResourceNotFound of string     // リソース未存在エラー
    | DuplicateResource of string    // リソース重複エラー
    | InfrastructureError of string  // Infrastructure層エラー

// 🔄 Application層Result型エイリアス
type ApplicationResult<'T> = Result<'T, ApplicationError>