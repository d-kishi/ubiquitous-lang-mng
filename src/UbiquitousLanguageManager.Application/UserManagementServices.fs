namespace UbiquitousLanguageManager.Application

// F# Domain層namespace階層化対応: Bounded Context別にopen
open UbiquitousLanguageManager.Domain.Common                  // UserId, Role, Permission, PermissionMappings
open UbiquitousLanguageManager.Domain.Authentication          // User, Email, UserName, Password, PasswordHash
open UbiquitousLanguageManager.Domain.ProjectManagement       // ProjectId
open System.Threading.Tasks
open System.Linq

// 🎯 Phase B-F3: ユーザー管理アプリケーションサービス実装
// Clean Architecture Application層: ユーザー管理のユースケースを実装
// 【F#初学者向け解説】
// UserManagementApplicationServiceは、ユーザー管理機能のビジネスロジックを
// オーケストレーションします。Railway-oriented Programmingパターンにより、
// エラー処理を明示的かつ型安全に実行します。
// UserManagementError型はIUserManagementService.fsで定義されています。

/// <summary>
/// F#ユーザー管理アプリケーションサービス実装
/// 【F#初学者向け解説】
/// F#のクラス定義では、主コンストラクターでDIされる依存関係を直接受け取ります。
/// これにより、コンストラクターインジェクションが自動的に実現されます。
/// IUserManagementServiceインターフェースを実装することで、Moqによるテストが可能になります。
/// </summary>
type UserManagementApplicationService(
    userRepository: IUserRepository,
    authService: IAuthenticationService,
    logger: ILogger<UserManagementApplicationService>) =

    // 🎯 Phase B-F3 Step1 Stage2: IUserManagementServiceインターフェース実装
    // 【F#初学者向け解説】
    // "interface IUserManagementService with" によりインターフェースを実装します。
    // 既存のメンバーメソッドと同じシグネチャを持つため、インターフェース実装として明示するだけで完了します。
    interface IUserManagementService with

        // 📋 ユーザー一覧取得（IdentityId付き）
        // 【F#初学者向け解説】
        // Phase B-F3 Step1.5 Stage5: Web層でIdentityIdを使用するため、
        // タプル(User * string)のリストを返す新しいメソッドを追加しました。
        member this.GetAllUsersWithIdentityAsync(query: obj, operatorIdentityId: string, includeDeleted: bool): Task<Result<(User * string) list, string>> =
            task {
                try
                    let! _ = logger.LogInformationAsync($"ユーザー一覧取得開始（IdentityId付き）- 操作者ASP.NET Core Identity ID: {operatorIdentityId}")

                    // Step 1: 操作者権限確認（Identity ID で検索）
                    let! operatorResult = userRepository.GetByIdentityIdAsync(operatorIdentityId)

                    match operatorResult with
                    | Error err ->
                        let! _ = logger.LogErrorAsync($"操作者取得エラー: {err}", None)
                        return Error err
                    | Ok operatorOpt ->
                        match operatorOpt with
                        | None ->
                            let! _ = logger.LogWarningAsync($"操作者が見つかりません: {operatorIdentityId}")
                            return Error $"操作者が見つかりません: {operatorIdentityId}"
                        | Some operator ->

                            // Step 2: 権限による表示制御
                            match operator.Role with
                            | SuperUser ->
                                // SuperUser: 全ユーザー取得可能（削除済み制御対応）
                                // 【F#初学者向け解説】
                                // IUserRepository.GetAllUsersWithIdentityAsyncを呼び出し、
                                // ユーザー・IdentityIdペアのリストを取得します。
                                let! usersResult = userRepository.GetAllUsersWithIdentityAsync(includeDeleted)

                                match usersResult with
                                | Error err ->
                                    let! _ = logger.LogErrorAsync($"ユーザー一覧取得エラー: {err}", None)
                                    return Error err
                                | Ok userPairs ->
                                    let! _ = logger.LogInformationAsync($"ユーザー一覧取得成功（IdentityId付き）- 件数: {userPairs.Length}")
                                    return Ok userPairs

                            | ProjectManager ->
                                // ProjectManager: 自分が管理するプロジェクトのユーザーのみ
                                // 【F#初学者向け解説】
                                // Phase B-F3 Step1.5: GetProjectIdsByUserIdAsyncの合成GUID問題を回避するため、
                                // Identity IDベースの新メソッドを使用します。
                                // 操作者が管理するプロジェクトに所属するユーザーでフィルタリングします。
                                let! _ = logger.LogInformationAsync($"プロジェクト管理者によるユーザー一覧取得（IdentityId付き）- 操作者ID: {operator.Id.Value}")

                                // Step 2a: 操作者のプロジェクトID一覧を取得（Identity IDベース）
                                // 【Phase B-F3 Step1.5】合成GUID問題回避: operatorIdentityIdを直接使用
                                let! projectIdsResult = userRepository.GetProjectIdsByIdentityIdAsync(operatorIdentityId)

                                match projectIdsResult with
                                | Error err ->
                                    let! _ = logger.LogErrorAsync($"プロジェクトID取得エラー: {err}", None)
                                    return Error err
                                | Ok projectIds ->
                                    // Step 2b: プロジェクトにユーザーが割り当てられていない場合
                                    if projectIds.IsEmpty then
                                        let! _ = logger.LogInformationAsync("操作者にプロジェクトが割り当てられていません - 空リスト返却")
                                        return Ok []
                                    else
                                        // Step 2c: プロジェクトに所属するユーザーを取得
                                        let! usersResult = userRepository.GetUsersByProjectIdsAsync(projectIds)

                                        match usersResult with
                                        | Error err ->
                                            let! _ = logger.LogErrorAsync($"プロジェクトユーザー取得エラー: {err}", None)
                                            return Error err
                                        | Ok users ->
                                            // Step 2d: ユーザーIDセットを作成し、GetAllUsersWithIdentityAsyncでフィルタリング
                                            // 【F#初学者向け解説】
                                            // 1. ProjectManagerが管理するプロジェクトに所属するユーザーのUserIdセットを作成
                                            // 2. GetAllUsersWithIdentityAsyncで全ユーザー・IdentityIdペアを取得
                                            // 3. UserIdセットに含まれるユーザーのみをフィルタリング
                                            let allowedUserIds = users |> List.map (fun u -> u.Id) |> Set.ofList

                                            let! allUsersResult = userRepository.GetAllUsersWithIdentityAsync(includeDeleted)

                                            match allUsersResult with
                                            | Error err ->
                                                let! _ = logger.LogErrorAsync($"全ユーザー取得エラー: {err}", None)
                                                return Error err
                                            | Ok allUserPairs ->
                                                // allowedUserIdsに含まれるユーザーのみフィルタリング
                                                let filteredUserPairs =
                                                    allUserPairs
                                                    |> List.filter (fun (user, _) -> allowedUserIds.Contains(user.Id))

                                                let! _ = logger.LogInformationAsync($"プロジェクトユーザー一覧取得成功（IdentityId付き）- 件数: {filteredUserPairs.Length}")
                                                return Ok filteredUserPairs

                            | _ ->
                                // DomainApprover / GeneralUser: ユーザー一覧参照権限なし
                                let! _ = logger.LogWarningAsync($"ユーザー一覧参照権限なし - ロール: {operator.Role}")
                                return Error "ユーザー一覧参照の権限がありません"

                with
                | ex ->
                    let! _ = logger.LogErrorAsync($"ユーザー一覧取得（IdentityId付き）で予期しないエラーが発生しました", Some ex)
                    return Error $"ユーザー一覧取得で予期しないエラーが発生しました: {ex.Message}"
            }

        // 📋 ユーザー一覧取得（検索条件・権限フィルタ・削除済みユーザー表示対応）
        // 【F#初学者向け解説】
        // task計算式（Task Computation Expression）を使用して非同期処理を記述します。
        // C#のasync/awaitと同様の機能ですが、F#では型推論により簡潔に記述できます。
        // includeDeletedパラメータにより、論理削除ユーザーの表示を制御します。
        member this.GetAllUsersAsync(query: obj, operatorIdentityId: string, includeDeleted: bool): Task<Result<User list, string>> =
            task {
                try
                    let! _ = logger.LogInformationAsync($"ユーザー一覧取得開始 - 操作者ASP.NET Core Identity ID: {operatorIdentityId}")

                    // Step 1: 操作者権限確認（Identity ID で検索）
                    // 【F#初学者向け解説】
                    // GetByIdentityIdAsyncは、ASP.NET Core IdentityのユーザーIDからドメインユーザーを検索します。
                    // GetHashCode()によるUserId変換は不安定なため、Identity IDを直接使用します。
                    let! operatorResult = userRepository.GetByIdentityIdAsync(operatorIdentityId)

                    match operatorResult with
                    | Error err ->
                        let! _ = logger.LogErrorAsync($"操作者取得エラー: {err}", None)
                        return Error err
                    | Ok operatorOpt ->
                        match operatorOpt with
                        | None ->
                            let! _ = logger.LogWarningAsync($"操作者が見つかりません: {operatorIdentityId}")
                            return Error $"操作者が見つかりません: {operatorIdentityId}"
                        | Some operator ->

                            // Step 2: 権限による表示制御
                            // 【F#初学者向け解説】
                            // パターンマッチングでロール別の処理を実装します。
                            // SuperUserは全ユーザー、ProjectManagerは自分が管理するプロジェクトのユーザーのみ
                            match operator.Role with
                            | SuperUser ->
                                // SuperUser: 全ユーザー取得可能（削除済み制御対応）
                                // 【F#初学者向け解説】
                                // includeDeletedパラメータをリポジトリに渡します。
                                // trueの場合は論理削除ユーザーも含め、falseの場合はアクティブユーザーのみを取得します。
                                let! usersResult = userRepository.GetAllUsersAsync(includeDeleted)

                                match usersResult with
                                | Error err ->
                                    let! _ = logger.LogErrorAsync($"ユーザー一覧取得エラー: {err}", None)
                                    return Error err
                                | Ok users ->
                                    let! _ = logger.LogInformationAsync($"ユーザー一覧取得成功 - 件数: {users.Length}")
                                    return Ok users

                            | ProjectManager ->
                                // ProjectManager: 自分が管理するプロジェクトのユーザーのみ
                                // 【F#初学者向け解説】
                                // ProjectManagerは全ユーザーを参照する権限がないため、
                                // 自分が管理するプロジェクトに所属するユーザーのみを返します。
                                // 1. 操作者のプロジェクトID一覧を取得
                                // 2. そのプロジェクトに所属するユーザーを取得
                                // 仕様根拠: 機能仕様書2.2節「ProjectManager: 担当プロジェクトのユーザーのみ参照可能」
                                let! _ = logger.LogInformationAsync($"プロジェクト管理者によるユーザー一覧取得 - 操作者ID: {operator.Id.Value}")

                                // Step 2a: 操作者のプロジェクトID一覧を取得
                                let! projectIdsResult = userRepository.GetProjectIdsByUserIdAsync(operator.Id)

                                match projectIdsResult with
                                | Error err ->
                                    let! _ = logger.LogErrorAsync($"プロジェクトID取得エラー: {err}", None)
                                    return Error err
                                | Ok projectIds ->
                                    // Step 2b: プロジェクトにユーザーが割り当てられていない場合
                                    if projectIds.IsEmpty then
                                        let! _ = logger.LogInformationAsync("操作者にプロジェクトが割り当てられていません - 空リスト返却")
                                        return Ok []
                                    else
                                        // Step 2c: プロジェクトに所属するユーザーを取得
                                        let! usersResult = userRepository.GetUsersByProjectIdsAsync(projectIds)

                                        match usersResult with
                                        | Error err ->
                                            let! _ = logger.LogErrorAsync($"プロジェクトユーザー取得エラー: {err}", None)
                                            return Error err
                                        | Ok users ->
                                            let! _ = logger.LogInformationAsync($"プロジェクトユーザー一覧取得成功 - 件数: {users.Length}")
                                            return Ok users

                            | _ ->
                                // DomainApprover / GeneralUser: ユーザー一覧参照権限なし
                                let! _ = logger.LogWarningAsync($"ユーザー一覧参照権限なし - ロール: {operator.Role}")
                                return Error "ユーザー一覧参照の権限がありません"

                with
                | ex ->
                    let! _ = logger.LogErrorAsync($"ユーザー一覧取得で予期しないエラーが発生しました", Some ex)
                    return Error $"ユーザー一覧取得で予期しないエラーが発生しました: {ex.Message}"
            }

        // 🔍 ユーザー詳細取得
        // 【F#初学者向け解説】
        // 権限制御を統合したユーザー詳細取得です。SuperUserまたは該当ユーザーの管理者のみ取得可能。
        member this.GetUserByIdAsync(userId: UserId, operatorIdentityId: string): Task<Result<User, string>> =
            task {
                try
                    let! _ = logger.LogInformationAsync($"ユーザー詳細取得開始 - ユーザーID: {userId.Value}, 操作者ASP.NET Core Identity ID: {operatorIdentityId}")

                    // Step 1: 対象ユーザー取得
                    let! userResult = userRepository.GetByIdAsync(userId)

                    match userResult with
                    | Error err ->
                        let! _ = logger.LogErrorAsync($"ユーザー取得エラー: {err}", None)
                        return Error err
                    | Ok userOpt ->
                        match userOpt with
                        | None ->
                            let! _ = logger.LogWarningAsync($"ユーザーが見つかりません: {userId.Value}")
                            return Error $"ユーザーが見つかりません: {userId.Value}"
                        | Some user ->

                            // Step 2: 操作者権限確認（Identity ID で検索）
                            let! operatorResult = userRepository.GetByIdentityIdAsync(operatorIdentityId)

                            match operatorResult with
                            | Error err -> return Error err
                            | Ok operatorOpt ->
                                match operatorOpt with
                                | None -> return Error $"操作者が見つかりません: {operatorIdentityId}"
                                | Some operator ->

                                    // SuperUserは全ユーザー参照可能
                                    if operator.Role = SuperUser then
                                        let! _ = logger.LogInformationAsync($"ユーザー詳細取得成功 - ユーザーID: {userId.Value}")
                                        return Ok user
                                    else
                                        match operator.Role with
                                        | SuperUser ->
                                            // 上記if文で処理済みのため、この分岐には到達しない
                                            let! _ = logger.LogInformationAsync($"ユーザー詳細取得成功 - ユーザーID: {userId.Value}")
                                            return Ok user

                                        | ProjectManager ->
                                            // ProjectManager: 担当プロジェクトのユーザーのみ参照可能
                                            // 【F#初学者向け解説】
                                            // ProjectManagerは全ユーザーを参照する権限がないため、
                                            // 自分が管理するプロジェクトに所属するユーザーのみを返します。
                                            // 仕様根拠: 機能仕様書2.2節「ProjectManager: 担当プロジェクトのユーザーのみ参照可能」
                                            let! projectIdsResult = userRepository.GetProjectIdsByUserIdAsync(operator.Id)
                                            match projectIdsResult with
                                            | Error err -> return Error err
                                            | Ok projectIds ->
                                                if projectIds.IsEmpty then
                                                    // 操作者にプロジェクトが割り当てられていない場合
                                                    return Error "このユーザーを参照する権限がありません"
                                                else
                                                    let! usersResult = userRepository.GetUsersByProjectIdsAsync(projectIds)
                                                    match usersResult with
                                                    | Error err -> return Error err
                                                    | Ok users ->
                                                        // 対象ユーザーが操作者の担当プロジェクトに所属しているか確認
                                                        match users |> List.tryFind (fun u -> u.Id = userId) with
                                                        | Some foundUser -> return Ok foundUser
                                                        | None -> return Error "このユーザーを参照する権限がありません"

                                        | _ ->
                                            // DomainApprover / GeneralUser: ユーザー詳細参照権限なし
                                            let! _ = logger.LogWarningAsync($"ユーザー詳細参照権限なし - ロール: {operator.Role}")
                                            return Error "ユーザー詳細参照の権限がありません"

                with
                | ex ->
                    let! _ = logger.LogErrorAsync($"ユーザー詳細取得で予期しないエラーが発生しました", Some ex)
                    return Error $"ユーザー詳細取得で予期しないエラーが発生しました: {ex.Message}"
            }

        // 👤 ユーザー作成
        // 【F#初学者向け解説】
        // 手動入力パスワードによる新規ユーザー作成です。
        // バリデーション→重複チェック→ドメインロジック実行→永続化の順で処理します。
        member this.CreateUserAsync(email: string, name: string, password: string, role: string, assignedProjectIds: int64 list, operatorIdentityId: string): Task<Result<User, string>> =
            task {
                try
                    let! _ = logger.LogInformationAsync($"ユーザー作成開始 - メール: {email}, 操作者ASP.NET Core Identity ID: {operatorIdentityId}")

                    // Step 1: 操作者権限確認（Identity ID で検索）
                    let! operatorResult = userRepository.GetByIdentityIdAsync(operatorIdentityId)

                    match operatorResult with
                    | Error err -> return Error err
                    | Ok operatorOpt ->
                        match operatorOpt with
                        | None -> return Error $"操作者が見つかりません: {operatorIdentityId}"
                        | Some operator ->

                            // Step 2: ユーザー作成権限チェック
                            // 【F#初学者向け解説】
                            // PermissionMappings.hasPermissionは、ロールと権限のマッピングを確認する関数です。
                            // Domain層で定義された権限マトリックスを使用します。
                            if not (PermissionMappings.hasPermission operator.Role CreateUsers) then
                                let! _ = logger.LogWarningAsync($"ユーザー作成権限なし - ロール: {operator.Role}")
                                return Error "ユーザー作成の権限がありません"
                            else

                                // Step 3: 入力値バリデーション（Smart Constructor Pattern）
                                // 【F#初学者向け解説】
                                // Email.create・UserName.create・Password.createは、バリデーションを含む安全なコンストラクターです。
                                // Result型を返すため、エラー時は即座に短絡評価されます。
                                match Email.create email with
                                | Error emailErr ->
                                    let! _ = logger.LogWarningAsync($"メールアドレスバリデーションエラー: {emailErr}")
                                    return Error emailErr
                                | Ok validEmail ->

                                    match UserName.create name with
                                    | Error nameErr ->
                                        let! _ = logger.LogWarningAsync($"ユーザー名バリデーションエラー: {nameErr}")
                                        return Error nameErr
                                    | Ok validName ->

                                        match Password.create password with
                                        | Error passwordErr ->
                                            let! _ = logger.LogWarningAsync($"パスワードバリデーションエラー: {passwordErr}")
                                            return Error passwordErr
                                        | Ok validPassword ->

                                            // Step 4: ロール解析
                                            // 【F#初学者向け解説】
                                            // パターンマッチングでロール文字列を適切なRole型に変換します。
                                            let userRole =
                                                match role.ToLowerInvariant() with
                                                | "superuser" -> SuperUser
                                                | "projectmanager" -> ProjectManager
                                                | "domainapprover" -> DomainApprover
                                                | _ -> GeneralUser

                                            // Step 5: ロール作成権限チェック
                                            // ProjectManagerは、SuperUser・ProjectManagerを作成できない
                                            if operator.Role = ProjectManager && (userRole = SuperUser || userRole = ProjectManager) then
                                                let! _ = logger.LogWarningAsync($"ロール作成権限不足 - 操作者: ProjectManager, 作成対象ロール: {userRole}")
                                                return Error "このロールのユーザーを作成する権限がありません"
                                            else

                                                // Step 6: メールアドレス重複チェック
                                                let! existingResult = userRepository.GetByEmailAsync(validEmail)

                                                match existingResult with
                                                | Error err -> return Error err
                                                | Ok existingOpt ->
                                                    match existingOpt with
                                                    | Some _ ->
                                                        let! _ = logger.LogWarningAsync($"メールアドレス重複: {email}")
                                                        return Error $"メールアドレスは既に使用されています: {email}"
                                                    | None ->

                                                        // Step 7: Infrastructure層での認証付きユーザー作成
                                                        // 【F#初学者向け解説】
                                                        // パスワードハッシュ化などのInfrastructure特有の処理は、
                                                        // IAuthenticationServiceに委譲します。
                                                        // operatorのUserIdを使用（operatorIdentityIdではなく）
                                                        let! createResult = authService.CreateUserWithPasswordAsync(validEmail, validName, userRole, validPassword, operator.Id)

                                                        match createResult with
                                                        | Error createErr ->
                                                            let! _ = logger.LogErrorAsync($"ユーザー作成失敗: {createErr}", None)
                                                            return Error createErr
                                                        | Ok (createdUser, identityId) ->
                                                            // identityId: ASP.NET Core IdentityのID（GUID文字列）
                                                            // プロジェクト割り当て時に使用

                                                            // Step 8: プロジェクト割り当て
                                                            // 【F#初学者向け解説】
                                                            // ユーザー作成後、指定されたプロジェクトに割り当てます。
                                                            // ProjectManagerの場合、自分の担当プロジェクトのみ割り当て可能です。
                                                            if not assignedProjectIds.IsEmpty then
                                                                // ProjectManager権限チェック: 自分の担当プロジェクトのみ割り当て可能
                                                                if operator.Role = ProjectManager then
                                                                    let! operatorProjectIdsResult = userRepository.GetProjectIdsByUserIdAsync(operator.Id)
                                                                    match operatorProjectIdsResult with
                                                                    | Error err ->
                                                                        let! _ = logger.LogErrorAsync($"操作者プロジェクト取得エラー: {err}", None)
                                                                        return Error err
                                                                    | Ok operatorProjectIds ->
                                                                        let operatorProjectIdValues = operatorProjectIds |> List.map (fun p -> p.Value)
                                                                        let invalidProjectIds = assignedProjectIds |> List.filter (fun id -> not (operatorProjectIdValues |> List.contains id))
                                                                        if not invalidProjectIds.IsEmpty then
                                                                            let! _ = logger.LogWarningAsync($"権限外プロジェクト割り当て試行: {invalidProjectIds}")
                                                                            return Error "割り当て権限のないプロジェクトが含まれています"
                                                                        else
                                                                            // 割り当て実行（Identity IDベースの新メソッドを使用）
                                                                            let! assignResult = userRepository.AssignProjectsToUserByIdentityIdAsync(identityId, assignedProjectIds)
                                                                            match assignResult with
                                                                            | Error assignErr ->
                                                                                let! _ = logger.LogErrorAsync($"プロジェクト割り当てエラー: {assignErr}", None)
                                                                                return Error assignErr
                                                                            | Ok () ->
                                                                                let! _ = logger.LogInformationAsync($"ユーザー作成成功 - ユーザーID: {createdUser.Id.Value}, プロジェクト割り当て: {assignedProjectIds.Length}件")
                                                                                return Ok createdUser
                                                                else
                                                                    // SuperUser: 全プロジェクト割り当て可能（Identity IDベースの新メソッドを使用）
                                                                    let! assignResult = userRepository.AssignProjectsToUserByIdentityIdAsync(identityId, assignedProjectIds)
                                                                    match assignResult with
                                                                    | Error assignErr ->
                                                                        let! _ = logger.LogErrorAsync($"プロジェクト割り当てエラー: {assignErr}", None)
                                                                        return Error assignErr
                                                                    | Ok () ->
                                                                        let! _ = logger.LogInformationAsync($"ユーザー作成成功 - ユーザーID: {createdUser.Id.Value}, プロジェクト割り当て: {assignedProjectIds.Length}件")
                                                                        return Ok createdUser
                                                            else
                                                                let! _ = logger.LogInformationAsync($"ユーザー作成成功 - ユーザーID: {createdUser.Id.Value}")
                                                                return Ok createdUser

                with
                | ex ->
                    let! _ = logger.LogErrorAsync($"ユーザー作成で予期しないエラーが発生しました", Some ex)
                    return Error $"ユーザー作成で予期しないエラーが発生しました: {ex.Message}"
            }

        // ✏️ ユーザー更新
        // 【F#初学者向け解説】
        // Phase B-F3 Step1.5 Stage5: IdentityIdベース更新に変更
        // GetHashCode()使用のUserId変換が不安定だったため、
        // ASP.NET Core Identity IDを直接使用する方式に変更しました。
        member this.UpdateUserAsync(targetIdentityId: string, name: string, role: string, assignedProjectIds: int64 list, isActive: bool, operatorIdentityId: string): Task<Result<User, string>> =
            task {
                try
                    let! _ = logger.LogInformationAsync($"ユーザー更新開始 - 対象IdentityId: {targetIdentityId}, 操作者ASP.NET Core Identity ID: {operatorIdentityId}")

                    // Step 1: 操作者権限確認（Identity ID で検索）
                    let! operatorResult = userRepository.GetByIdentityIdAsync(operatorIdentityId)

                    match operatorResult with
                    | Error err -> return Error err
                    | Ok operatorOpt ->
                        match operatorOpt with
                        | None -> return Error $"操作者が見つかりません: {operatorIdentityId}"
                        | Some operator ->

                            // Step 2: ユーザー編集権限チェック
                            if not (PermissionMappings.hasPermission operator.Role EditUsers) then
                                let! _ = logger.LogWarningAsync($"ユーザー編集権限なし - ロール: {operator.Role}")
                                return Error "ユーザー編集の権限がありません"
                            else

                                // Step 3: 対象ユーザー取得（IdentityIdで検索）
                                // 【F#初学者向け解説】
                                // GetHashCode()変換を回避し、IdentityIdを直接使用して検索します。
                                let! userResult = userRepository.GetByIdentityIdAsync(targetIdentityId)

                                match userResult with
                                | Error err -> return Error err
                                | Ok userOpt ->
                                    match userOpt with
                                    | None ->
                                        let! _ = logger.LogWarningAsync($"更新対象ユーザーが見つかりません: {targetIdentityId}")
                                        return Error $"更新対象ユーザーが見つかりません: {targetIdentityId}"
                                    | Some existingUser ->

                                        // Step 4: 名前バリデーション
                                        match UserName.create name with
                                        | Error nameErr ->
                                            let! _ = logger.LogWarningAsync($"ユーザー名バリデーションエラー: {nameErr}")
                                            return Error nameErr
                                        | Ok validName ->

                                            // Step 5: ロール解析
                                            let newRole =
                                                match role.ToLowerInvariant() with
                                                | "superuser" -> SuperUser
                                                | "projectmanager" -> ProjectManager
                                                | "domainapprover" -> DomainApprover
                                                | _ -> GeneralUser

                                            // Step 6: ロール変更権限チェック
                                            // ProjectManagerは、SuperUser・ProjectManagerに変更できない
                                            if operator.Role = ProjectManager && (newRole = SuperUser || newRole = ProjectManager) then
                                                let! _ = logger.LogWarningAsync($"ロール変更権限不足 - 操作者: ProjectManager, 変更対象ロール: {newRole}")
                                                return Error "このロールに変更する権限がありません"
                                            else

                                                // Step 6.5: 自己ロール変更禁止チェック
                                                // 【F#初学者向け解説】
                                                // セキュリティ対策として、ユーザーが自分自身のロールを変更できないようにします。
                                                // これにより、権限昇格攻撃（一般ユーザーが自分を管理者に変更する等）を防ぎます。
                                                // 仕様根拠: 機能仕様書2.2.2節「自分自身のロール変更不可（権限昇格防止）」
                                                //
                                                // Phase B-F3 Step1.5 Stage5: IdentityIdベース比較に変更
                                                // - targetIdentityId: 更新対象ユーザーのIdentityId（string）
                                                // - operatorIdentityId: 操作者のIdentityId（string）
                                                // - 文字列比較で自己更新を検出し、かつロールが変更されている場合はエラー
                                                if targetIdentityId = operatorIdentityId && newRole <> existingUser.Role then
                                                    let! _ = logger.LogWarningAsync($"自己ロール変更試行 - IdentityId: {targetIdentityId}")
                                                    return Error "自分自身のロールを変更することはできません"
                                                else

                                                    // Step 7: ドメインロジック適用（ユーザー更新）
                                                    // 【F#初学者向け解説】
                                                    // ドメインエンティティのメソッドを呼び出してビジネスルールを適用します。
                                                    // F#のレコード型更新構文（with式）を使用して、不変性を保ちながら更新します。
                                                    let updatedUser =
                                                        { existingUser with
                                                            Name = validName
                                                            Role = newRole
                                                            IsActive = isActive
                                                            UpdatedAt = System.DateTime.UtcNow
                                                            UpdatedBy = operator.Id }

                                                    // Step 8: 永続化
                                                    let! saveResult = userRepository.SaveAsync(updatedUser)

                                                    match saveResult with
                                                    | Error saveErr ->
                                                        let! _ = logger.LogErrorAsync($"ユーザー更新保存エラー: {saveErr}", None)
                                                        return Error saveErr
                                                    | Ok savedUser ->

                                                        // Step 9: プロジェクト割り当て更新
                                                        // 【F#初学者向け解説】
                                                        // ユーザー更新後、プロジェクト割り当てを更新します。
                                                        // ProjectManagerの場合、自分の担当プロジェクトのみ割り当て可能です。
                                                        // ProjectManager権限チェック: 自分の担当プロジェクトのみ割り当て可能
                                                        if operator.Role = ProjectManager then
                                                            let! operatorProjectIdsResult = userRepository.GetProjectIdsByUserIdAsync(operator.Id)
                                                            match operatorProjectIdsResult with
                                                            | Error err ->
                                                                let! _ = logger.LogErrorAsync($"操作者プロジェクト取得エラー: {err}", None)
                                                                return Error err
                                                            | Ok operatorProjectIds ->
                                                                let operatorProjectIdValues = operatorProjectIds |> List.map (fun p -> p.Value)
                                                                let invalidProjectIds = assignedProjectIds |> List.filter (fun id -> not (operatorProjectIdValues |> List.contains id))
                                                                if not invalidProjectIds.IsEmpty then
                                                                    let! _ = logger.LogWarningAsync($"権限外プロジェクト割り当て試行: {invalidProjectIds}")
                                                                    return Error "割り当て権限のないプロジェクトが含まれています"
                                                                else
                                                                    // 割り当て更新実行（Identity IDベース）
                                                                    // Phase B-F3 Step1.5: ConvertUserIdToGuid問題回避のため、Identity IDを直接使用
                                                                    let! updateResult = userRepository.UpdateUserProjectsByIdentityIdAsync(targetIdentityId, assignedProjectIds)
                                                                    match updateResult with
                                                                    | Error updateErr ->
                                                                        let! _ = logger.LogErrorAsync($"プロジェクト割り当て更新エラー: {updateErr}", None)
                                                                        return Error updateErr
                                                                    | Ok () ->
                                                                        let! _ = logger.LogInformationAsync($"ユーザー更新成功 - ユーザーID: {savedUser.Id.Value}, プロジェクト割り当て更新完了")
                                                                        return Ok savedUser
                                                        else
                                                            // SuperUser: 全プロジェクト割り当て可能（Identity IDベース）
                                                            // Phase B-F3 Step1.5: ConvertUserIdToGuid問題回避のため、Identity IDを直接使用
                                                            let! updateResult = userRepository.UpdateUserProjectsByIdentityIdAsync(targetIdentityId, assignedProjectIds)
                                                            match updateResult with
                                                            | Error updateErr ->
                                                                let! _ = logger.LogErrorAsync($"プロジェクト割り当て更新エラー: {updateErr}", None)
                                                                return Error updateErr
                                                            | Ok () ->
                                                                let! _ = logger.LogInformationAsync($"ユーザー更新成功 - ユーザーID: {savedUser.Id.Value}, プロジェクト割り当て更新完了")
                                                                return Ok savedUser

                with
                | ex ->
                    let! _ = logger.LogErrorAsync($"ユーザー更新で予期しないエラーが発生しました", Some ex)
                    return Error $"ユーザー更新で予期しないエラーが発生しました: {ex.Message}"
            }

        // 🗑️ ユーザー削除（論理削除）
        // 【F#初学者向け解説】
        // Phase B-F3 Step1.5 Stage5: IdentityIdベース削除に変更
        // GetHashCode()使用のUserId変換が不安定だったため、
        // ASP.NET Core Identity IDを直接使用する方式に変更しました。
        member this.DeleteUserAsync(targetIdentityId: string, operatorIdentityId: string): Task<Result<unit, string>> =
            task {
                try
                    let! _ = logger.LogInformationAsync($"ユーザー削除開始 - 対象IdentityId: {targetIdentityId}, 操作者ASP.NET Core Identity ID: {operatorIdentityId}")

                    // Step 1: 操作者権限確認（Identity ID で検索）
                    let! operatorResult = userRepository.GetByIdentityIdAsync(operatorIdentityId)

                    match operatorResult with
                    | Error err -> return Error err
                    | Ok operatorOpt ->
                        match operatorOpt with
                        | None -> return Error $"操作者が見つかりません: {operatorIdentityId}"
                        | Some operator ->

                            // Step 2: ユーザー削除権限チェック
                            if not (PermissionMappings.hasPermission operator.Role DeleteUsers) then
                                let! _ = logger.LogWarningAsync($"ユーザー削除権限なし - ロール: {operator.Role}")
                                return Error "ユーザー削除の権限がありません"
                            else

                                // Step 3: リポジトリ削除処理（論理削除・IdentityIdベース）
                                // 【F#初学者向け解説】
                                // IUserRepository.DeleteByIdentityIdAsyncを呼び出し、
                                // GetHashCode()変換を回避してIdentityIdで直接削除します。
                                let! deleteResult = userRepository.DeleteByIdentityIdAsync(targetIdentityId)

                                match deleteResult with
                                | Error deleteErr ->
                                    let! _ = logger.LogErrorAsync($"ユーザー削除エラー: {deleteErr}", None)
                                    return Error deleteErr
                                | Ok () ->
                                    let! _ = logger.LogInformationAsync($"ユーザー削除成功 - IdentityId: {targetIdentityId}")
                                    return Ok ()

                with
                | ex ->
                    let! _ = logger.LogErrorAsync($"ユーザー削除で予期しないエラーが発生しました", Some ex)
                    return Error $"ユーザー削除で予期しないエラーが発生しました: {ex.Message}"
            }

        // 🔐 パスワードリセット（管理者による強制変更）
        // 【F#初学者向け解説】
        // SuperUserが他ユーザーのパスワードを強制的に変更する機能です。
        // Railway-oriented Programmingパターンにより、各ステップのエラーを明示的に処理します。
        member this.ResetPasswordAsync(targetUserId: UserId, newPassword: string, operatorIdentityId: string): Task<Result<unit, string>> =
            task {
                try
                    let! _ = logger.LogInformationAsync($"パスワードリセット開始 - 対象ユーザーID: {targetUserId.Value}, 操作者ASP.NET Core Identity ID: {operatorIdentityId}")

                    // Step 1: 操作者権限確認（Identity ID で検索）
                    // 【F#初学者向け解説】
                    // GetByIdentityIdAsyncは、ASP.NET Core IdentityのユーザーIDからドメインユーザーを検索します。
                    let! operatorResult = userRepository.GetByIdentityIdAsync(operatorIdentityId)

                    match operatorResult with
                    | Error err -> return Error err
                    | Ok operatorOpt ->
                        match operatorOpt with
                        | None -> return Error $"操作者が見つかりません: {operatorIdentityId}"
                        | Some operator ->

                            // Step 2: SuperUser権限チェック（パスワードリセットはSuperUserのみ実行可能）
                            // 【F#初学者向け解説】
                            // セキュリティ要件: 他ユーザーのパスワードを強制変更できる権限は、
                            // システム最高権限であるSuperUserに限定されます。
                            // ProjectManagerなどの他のロールは実行できません。
                            if operator.Role <> SuperUser then
                                let! _ = logger.LogWarningAsync($"パスワードリセット権限なし - ロール: {operator.Role}")
                                return Error "パスワードリセットの権限がありません。SuperUserのみ実行可能です。"
                            else

                                // Step 3: 対象ユーザー確認
                                // 【F#初学者向け解説】
                                // targetUserIdからドメインユーザーを取得し、
                                // ASP.NET Core Identity IDを取得します（Infrastructure層で使用）。
                                let! targetUserResult = userRepository.GetByIdAsync(targetUserId)

                                match targetUserResult with
                                | Error err -> return Error err
                                | Ok targetUserOpt ->
                                    match targetUserOpt with
                                    | None ->
                                        let! _ = logger.LogWarningAsync($"パスワードリセット対象ユーザーが見つかりません: {targetUserId.Value}")
                                        return Error $"パスワードリセット対象ユーザーが見つかりません: {targetUserId.Value}"
                                    | Some targetUser ->

                                        // Step 4: パスワードバリデーション（Smart Constructor Pattern）
                                        // 【F#初学者向け解説】
                                        // Password.createは、パスワードポリシーを満たすかバリデーションします。
                                        // Result型を返すため、エラー時は即座に短絡評価されます。
                                        match Password.create newPassword with
                                        | Error passwordErr ->
                                            let! _ = logger.LogWarningAsync($"パスワードバリデーションエラー: {passwordErr}")
                                            return Error passwordErr
                                        | Ok validPassword ->

                                            // Step 5: Identity ID取得
                                            // 【F#初学者向け解説】
                                            // targetUser.getIdentityId()は、ASP.NET Core IdentityのユーザーID（GUID文字列）を返します。
                                            // Infrastructure層のAuthenticationService.AdminResetPasswordAsyncに渡すために必要です。
                                            let targetIdentityId = targetUser.getIdentityId()

                                            // Step 6: Infrastructure層でパスワードリセット実行
                                            // 【F#初学者向け解説】
                                            // authService.AdminResetPasswordAsyncは、
                                            // ASP.NET Core Identityのパスワード管理機能を使用して
                                            // 現在のパスワードを知らなくても新しいパスワードを設定します。
                                            let! resetResult = authService.AdminResetPasswordAsync(targetIdentityId, validPassword)

                                            match resetResult with
                                            | Error resetErr ->
                                                let! _ = logger.LogErrorAsync($"パスワードリセット失敗: {resetErr}", None)
                                                return Error resetErr
                                            | Ok () ->
                                                // Step 7: セキュリティログ記録（監査証跡）
                                                // 【F#初学者向け解説】
                                                // パスワードリセットはセキュリティクリティカルな操作のため、
                                                // 操作者・対象ユーザー・実行日時を必ずログに記録します。
                                                let! _ = logger.LogInformationAsync($"パスワードリセット成功 - 対象ユーザーID: {targetUserId.Value}, 操作者: {operator.Email.Value}, 実行日時: {System.DateTime.UtcNow}")
                                                return Ok ()

                with
                | ex ->
                    let! _ = logger.LogErrorAsync($"パスワードリセットで予期しないエラーが発生しました", Some ex)
                    return Error $"パスワードリセットで予期しないエラーが発生しました: {ex.Message}"
            }

        // 📋 ユーザーが所属するプロジェクトID一覧取得
        // 【F#初学者向け解説】
        // Phase B-F3 Step1.5 Stage4: Edit.razorでプロジェクト割り当てチェックボックスの初期状態を復元するために使用します。
        // IUserRepositoryのGetProjectIdsByUserIdAsyncを薄くラップし、Clean Architectureを維持します。
        member this.GetProjectIdsByUserIdAsync(userId: UserId): Task<Result<int64 list, string>> =
            task {
                try
                    let! _ = logger.LogInformationAsync($"プロジェクトID一覧取得開始 - ユーザーID: {userId.Value}")

                    // IUserRepository.GetProjectIdsByUserIdAsyncを呼び出し
                    // 【F#初学者向け解説】
                    // 戻り値はResult<ProjectId list, string>なので、
                    // ProjectId型からint64（long）に変換します。
                    let! result = userRepository.GetProjectIdsByUserIdAsync(userId)

                    match result with
                    | Error err ->
                        let! _ = logger.LogErrorAsync($"プロジェクトID一覧取得エラー: {err}", None)
                        return Error err
                    | Ok projectIds ->
                        // 【F#初学者向け解説】
                        // F# ProjectId型をC# long型に変換します。
                        // ProjectId.Valueで内部のint64値を取り出します。
                        let projectIdValues = projectIds |> List.map (fun pid -> pid.Value)
                        let! _ = logger.LogInformationAsync($"プロジェクトID一覧取得成功 - ユーザーID: {userId.Value}, 件数: {projectIdValues.Length}")
                        return Ok projectIdValues

                with
                | ex ->
                    let! _ = logger.LogErrorAsync($"プロジェクトID一覧取得で予期しないエラーが発生しました", Some ex)
                    return Error $"プロジェクトID一覧取得で予期しないエラーが発生しました: {ex.Message}"
            }

        // 📋 Emailベースでプロジェクト一覧取得
        // 【F#初学者向け解説】
        // GetProjectIdsByUserIdAsyncの合成GUID問題を回避するため、Emailベースで検索します。
        member this.GetProjectIdsByEmailAsync(email: string): Task<Result<int64 list, string>> =
            task {
                try
                    let! _ = logger.LogInformationAsync($"プロジェクトID一覧取得開始（Email指定）- Email: {email}")

                    // Email型に変換
                    let emailResult = Email.create email
                    match emailResult with
                    | Error err ->
                        let! _ = logger.LogErrorAsync($"無効なメールアドレス: {err}", None)
                        return Error err
                    | Ok validEmail ->
                        // IUserRepository.GetProjectIdsByEmailAsyncを呼び出し
                        let! result = userRepository.GetProjectIdsByEmailAsync(validEmail)

                        match result with
                        | Ok projectIds ->
                            // F# ProjectId list → int64 list に変換
                            let idValues = projectIds |> List.map (fun pid -> pid.Value)
                            let! _ = logger.LogInformationAsync($"プロジェクトID一覧取得成功 - Email: {email}, 件数: {idValues.Length}")
                            return Ok idValues
                        | Error err ->
                            let! _ = logger.LogErrorAsync($"プロジェクトID取得エラー: {err}", None)
                            return Error err
                with
                | ex ->
                    let! _ = logger.LogErrorAsync($"プロジェクトID一覧取得で例外発生: {ex.Message}", Some ex)
                    return Error $"プロジェクトID一覧取得エラー: {ex.Message}"
            }
