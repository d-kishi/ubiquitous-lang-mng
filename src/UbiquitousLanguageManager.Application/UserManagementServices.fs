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

        // 📋 ユーザー一覧取得（検索条件・権限フィルタ対応）
        // 【F#初学者向け解説】
        // task計算式（Task Computation Expression）を使用して非同期処理を記述します。
        // C#のasync/awaitと同様の機能ですが、F#では型推論により簡潔に記述できます。
        member this.GetAllUsersAsync(query: obj, operatorIdentityId: string): Task<Result<User list, string>> =
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
                                // SuperUser: 全ユーザー取得可能
                                let! usersResult = userRepository.GetAllUsersAsync()

                                match usersResult with
                                | Error err ->
                                    let! _ = logger.LogErrorAsync($"ユーザー一覧取得エラー: {err}", None)
                                    return Error err
                                | Ok users ->
                                    let! _ = logger.LogInformationAsync($"ユーザー一覧取得成功 - 件数: {users.Length}")
                                    return Ok users

                            | ProjectManager ->
                                // ProjectManager: 自分が管理するプロジェクトのユーザーのみ
                                // TODO: プロジェクト別ユーザー取得ロジック実装（Phase B-F3 Step2）
                                let! _ = logger.LogInformationAsync($"プロジェクト管理者によるユーザー一覧取得 - 操作者ASP.NET Core Identity ID: {operatorIdentityId}")
                                let! usersResult = userRepository.GetAllActiveUsersAsync()

                                match usersResult with
                                | Error err -> return Error err
                                | Ok users -> return Ok users

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
                                    // TODO: ProjectManagerは自分が管理するプロジェクトのユーザーのみ参照可能（Phase B-F3 Step2）
                                    else
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
                                                        | Ok createdUser ->

                                                            // TODO: Step 8: プロジェクト割り当て（Phase B-F3 Step2）
                                                            // assignedProjectIds を使用してUserProjectsテーブルに登録

                                                            let! _ = logger.LogInformationAsync($"ユーザー作成成功 - ユーザーID: {createdUser.Id.Value}")
                                                            return Ok createdUser

                with
                | ex ->
                    let! _ = logger.LogErrorAsync($"ユーザー作成で予期しないエラーが発生しました", Some ex)
                    return Error $"ユーザー作成で予期しないエラーが発生しました: {ex.Message}"
            }

        // ✏️ ユーザー更新
        // 【F#初学者向け解説】
        // 既存ユーザー情報の更新です。名前・ロール・プロジェクト割り当て・アクティブ状態を変更できます。
        member this.UpdateUserAsync(userId: UserId, name: string, role: string, assignedProjectIds: int64 list, isActive: bool, operatorIdentityId: string): Task<Result<User, string>> =
            task {
                try
                    let! _ = logger.LogInformationAsync($"ユーザー更新開始 - ユーザーID: {userId.Value}, 操作者ASP.NET Core Identity ID: {operatorIdentityId}")

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

                                // Step 3: 対象ユーザー取得
                                let! userResult = userRepository.GetByIdAsync(userId)

                                match userResult with
                                | Error err -> return Error err
                                | Ok userOpt ->
                                    match userOpt with
                                    | None ->
                                        let! _ = logger.LogWarningAsync($"更新対象ユーザーが見つかりません: {userId.Value}")
                                        return Error $"更新対象ユーザーが見つかりません: {userId.Value}"
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

                                                    // TODO: Step 9: プロジェクト割り当て更新（Phase B-F3 Step2）

                                                    let! _ = logger.LogInformationAsync($"ユーザー更新成功 - ユーザーID: {userId.Value}")
                                                    return Ok savedUser

                with
                | ex ->
                    let! _ = logger.LogErrorAsync($"ユーザー更新で予期しないエラーが発生しました", Some ex)
                    return Error $"ユーザー更新で予期しないエラーが発生しました: {ex.Message}"
            }

        // 🗑️ ユーザー無効化（論理削除）
        // 【F#初学者向け解説】
        // ユーザーを論理削除（IsActive = false）します。物理削除は行いません。
        member this.DeactivateUserAsync(userId: UserId, operatorIdentityId: string): Task<Result<unit, string>> =
            task {
                try
                    let! _ = logger.LogInformationAsync($"ユーザー無効化開始 - ユーザーID: {userId.Value}, 操作者ASP.NET Core Identity ID: {operatorIdentityId}")

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

                                // Step 3: リポジトリ削除処理（論理削除）
                                let! deleteResult = userRepository.DeleteAsync(userId)

                                match deleteResult with
                                | Error deleteErr ->
                                    let! _ = logger.LogErrorAsync($"ユーザー無効化エラー: {deleteErr}", None)
                                    return Error deleteErr
                                | Ok () ->
                                    let! _ = logger.LogInformationAsync($"ユーザー無効化成功 - ユーザーID: {userId.Value}")
                                    return Ok ()

                with
                | ex ->
                    let! _ = logger.LogErrorAsync($"ユーザー無効化で予期しないエラーが発生しました", Some ex)
                    return Error $"ユーザー無効化で予期しないエラーが発生しました: {ex.Message}"
            }

        // ✅ ユーザー有効化
        // 【F#初学者向け解説】
        // 無効化されたユーザーを再有効化（IsActive = true）します。
        member this.ActivateUserAsync(userId: UserId, operatorIdentityId: string): Task<Result<unit, string>> =
            task {
                try
                    let! _ = logger.LogInformationAsync($"ユーザー有効化開始 - ユーザーID: {userId.Value}, 操作者ASP.NET Core Identity ID: {operatorIdentityId}")

                    // Step 1: 操作者権限確認（Identity ID で検索）
                    let! operatorResult = userRepository.GetByIdentityIdAsync(operatorIdentityId)

                    match operatorResult with
                    | Error err -> return Error err
                    | Ok operatorOpt ->
                        match operatorOpt with
                        | None -> return Error $"操作者が見つかりません: {operatorIdentityId}"
                        | Some operator ->

                            // Step 2: ユーザー削除権限チェック（有効化も削除権限が必要）
                            if not (PermissionMappings.hasPermission operator.Role DeleteUsers) then
                                let! _ = logger.LogWarningAsync($"ユーザー有効化権限なし - ロール: {operator.Role}")
                                return Error "ユーザー有効化の権限がありません"
                            else

                                // Step 3: 対象ユーザー取得
                                let! userResult = userRepository.GetByIdAsync(userId)

                                match userResult with
                                | Error err -> return Error err
                                | Ok userOpt ->
                                    match userOpt with
                                    | None ->
                                        let! _ = logger.LogWarningAsync($"有効化対象ユーザーが見つかりません: {userId.Value}")
                                        return Error $"有効化対象ユーザーが見つかりません: {userId.Value}"
                                    | Some existingUser ->

                                        // Step 4: ドメインロジック適用（ユーザー有効化）
                                        // 【F#初学者向け解説】
                                        // with式でIsActiveをtrueに変更します。不変性を保つため新しいレコードが作成されます。
                                        let activatedUser =
                                            { existingUser with
                                                IsActive = true
                                                UpdatedAt = System.DateTime.UtcNow
                                                UpdatedBy = operator.Id }

                                        // Step 5: 永続化
                                        let! saveResult = userRepository.SaveAsync(activatedUser)

                                        match saveResult with
                                        | Error saveErr ->
                                            let! _ = logger.LogErrorAsync($"ユーザー有効化保存エラー: {saveErr}", None)
                                            return Error saveErr
                                        | Ok _ ->
                                            let! _ = logger.LogInformationAsync($"ユーザー有効化成功 - ユーザーID: {userId.Value}")
                                            return Ok ()

                with
                | ex ->
                    let! _ = logger.LogErrorAsync($"ユーザー有効化で予期しないエラーが発生しました", Some ex)
                    return Error $"ユーザー有効化で予期しないエラーが発生しました: {ex.Message}"
            }
