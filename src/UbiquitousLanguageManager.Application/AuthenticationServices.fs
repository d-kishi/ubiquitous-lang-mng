namespace UbiquitousLanguageManager.Application

open UbiquitousLanguageManager.Domain
open System.Threading.Tasks

// 🎯 認証Application層サービス: F# Railway-oriented Programming実装
// Phase A9 Step 1-1: Clean Architectureに完全準拠したF#認証ビジネスロジック

// 🚦 認証エラー型: 詳細なエラー分類（判別共用体による型安全性）
// 【F#初学者向け解説】
// Discriminated Union（判別共用体）を使用してエラーを型安全に表現します。
// これにより、コンパイル時にすべてのエラーケースを確実に処理できます。
// C#の例外処理と比較して、エラーが明示的で予測可能になります。
type AuthenticationError =
    | InvalidCredentials of string              // 認証情報不正（理由付き）
    | UserNotFound of Email                     // ユーザー未存在
    | AccountLocked of lockoutEnd: System.DateTime // アカウントロック（解除時刻付き）
    | AccountDeactivated                        // アカウント無効化
    | ValidationError of string                 // バリデーションエラー
    | SystemError of exn                       // システムエラー（例外情報付き）

// 🎯 認証リクエスト型: 入力データの型安全な表現
// 【F#初学者向解説】
// レコード型を使用して認証リクエストを構造化します。
// 不変性により、データの整合性が保証されます。
type AuthenticationRequest = {
    Email: string                              // メールアドレス（文字列）
    Password: string                           // パスワード（平文）
    RememberMe: bool                           // ログイン状態保持フラグ
}

// 🎯 認証結果型: 成功時の結果データ（判別共用体による型安全性）
// 【F#初学者向け解説】
// 認証成功時の情報を構造化して表現します。
// パターンマッチングで各結果タイプを明示的に処理できます。
type AuthenticationResult =
    | AuthenticationSuccess of User * token: string    // 認証成功（ユーザー + トークン）
    | FirstLoginRequired of User                       // 初回ログイン（パスワード変更要求）
    | EmailConfirmationRequired of User                // メール確認要求

// 🔐 F#認証Application層サービス実装
// 【F#初学者向け解説】
// F#のクラス定義では、主コンストラクターでDIされる依存関係を直接受け取ります。
// これにより、コンストラクターインジェクションが自動的に実現されます。
type AuthenticationApplicationService(
    userRepository: IUserRepository,
    authService: IAuthenticationService,
    logger: ILogger<AuthenticationApplicationService>) =
    
    // 🔑 メイン認証メソッド: Railway-oriented Programming実装
    // 【F#初学者向け解説】
    // task計算式（Task Computation Expression）を使用して非同期処理を記述します。
    // C#のasync/awaitと同様の機能ですが、F#では型推論により簡潔に記述できます。
    // Result型とOption型を組み合わせて、エラーハンドリングを明示的に行います。
    member this.AuthenticateUserAsync(request: AuthenticationRequest): Task<Result<AuthenticationResult, AuthenticationError>> =
        task {
            try
                // Step 1: 入力バリデーション（Smart Constructor Pattern適用）
                // 【F#初学者向け解説】
                // Email.create・Password.createは、バリデーションを含む安全なコンストラクターです。
                // Result型を返すため、エラー時は即座に短絡評価（Railway-oriented Programming）されます。
                match Email.create request.Email with
                | Error emailError -> 
                    let! _ = logger.LogWarningAsync($"認証失敗: 不正なメールアドレス形式 - {request.Email}")
                    return Error (ValidationError emailError)
                | Ok validEmail ->
                    
                    match Password.create request.Password with
                    | Error passwordError ->
                        let! _ = logger.LogWarningAsync($"認証失敗: 不正なパスワード形式 - {validEmail.Value}")
                        return Error (ValidationError passwordError)
                    | Ok validPassword ->
                        
                        // Step 2: ユーザー存在確認
                        // 【F#初学者向け解説】
                        // let!は非同期処理の結果を待機します（C#のawaitと同じ）。
                        // Option型のパターンマッチングで、ユーザーの存在/非存在を明示的に処理します。
                        let! userResult = userRepository.GetByEmailAsync(validEmail)
                        
                        match userResult with
                        | Error dbError ->
                            let! _ = logger.LogErrorAsync($"認証エラー: データベースアクセス失敗 - {dbError}", None)
                            return Error (SystemError (System.Exception(dbError)))
                        | Ok userOption ->
                            
                            match userOption with
                            | None ->
                                let! _ = logger.LogWarningAsync($"認証失敗: ユーザー未存在 - {validEmail.Value}")
                                return Error (UserNotFound validEmail)
                            | Some user ->
                                
                                // Step 3: アカウント状態確認
                                // 【F#初学者向け解説】
                                // ガード条件（if-elif-else）でビジネスルールを順次チェックします。
                                // 各条件で異なるエラー型を返すことで、呼び出し側で適切な処理が可能です。
                                if not user.IsActive then
                                    let! _ = logger.LogWarningAsync($"認証失敗: アカウント無効化 - {validEmail.Value}")
                                    return Error AccountDeactivated
                                elif user.isLockedOut() then
                                    let lockoutEnd = user.LockoutEnd.Value
                                    let! _ = logger.LogWarningAsync($"認証失敗: アカウントロック中 - {validEmail.Value}, 解除時刻: {lockoutEnd}")
                                    return Error (AccountLocked lockoutEnd)
                                else
                                    
                                    // Step 4: パスワード認証
                                    // 【F#初学者向け解説】
                                    // Infrastructure層のIAuthenticationServiceを呼び出してパスワード検証を行います。
                                    // Clean Architectureに従い、Application層では具体的な認証メカニズムを知りません。
                                    let! loginResult = authService.LoginAsync(validEmail, request.Password)
                                    
                                    match loginResult with
                                    | Error authError ->
                                        // Step 5: 認証失敗時の処理（失敗回数記録）
                                        let failedUser = user.recordFailedAccess 5 (System.TimeSpan.FromMinutes(30.0))
                                        let! _ = userRepository.SaveAsync(failedUser)
                                        let! _ = logger.LogWarningAsync($"認証失敗: パスワード不一致 - {validEmail.Value}")
                                        return Error (InvalidCredentials "メールアドレスまたはパスワードが正しくありません")
                                    | Ok authenticatedUser ->
                                        
                                        // Step 6: 認証成功時の処理
                                        let successUser = authenticatedUser.recordSuccessfulAccess()
                                        let! _ = userRepository.SaveAsync(successUser)
                                        let! _ = logger.LogInformationAsync($"認証成功 - {validEmail.Value}")
                                        
                                        // Step 7: 認証後の状態判定（判別共用体による型安全な結果）
                                        // 【F#初学者向け解説】
                                        // ビジネスルールに基づいて、適切な認証結果を返します。
                                        // 各結果タイプは判別共用体で表現され、呼び出し側で明示的に処理されます。
                                        if successUser.IsFirstLogin then
                                            return Ok (FirstLoginRequired successUser)
                                        elif not successUser.EmailConfirmed then
                                            return Ok (EmailConfirmationRequired successUser)
                                        else
                                            // トークン生成
                                            let! tokenResult = authService.GenerateTokenAsync(successUser)
                                            match tokenResult with
                                            | Error tokenError ->
                                                let! _ = logger.LogErrorAsync($"トークン生成失敗 - {tokenError}", None)
                                                return Error (SystemError (System.Exception(tokenError)))
                                            | Ok token ->
                                                return Ok (AuthenticationSuccess (successUser, token))
            
            with
            | ex ->
                // 予期しない例外の処理
                // 【F#初学者向け解説】
                // F#では例外よりもResult型を推奨しますが、Infrastructure層からの例外は
                // try-withブロックで捕捉し、Result型のError側に変換します。
                let! _ = logger.LogErrorAsync($"認証処理で予期しないエラーが発生しました", Some ex)
                return Error (SystemError ex)
        }
    
    // 🔐 ユーザー作成（認証付き）: パスワードハッシュ化を含む完全なユーザー登録
    // 【F#初学者向け解説】
    // 複数のバリデーションを順次実行し、すべて成功した場合のみユーザーを作成します。
    // Railway-oriented Programmingにより、途中でエラーが発生すると処理が短絡評価されます。
    member this.CreateUserWithPasswordAsync(email: Email, name: UserName, role: Role, password: Password, createdBy: UserId): Task<Result<User, AuthenticationError>> =
        task {
            try
                let! _ = logger.LogInformationAsync($"ユーザー作成開始 - {email.Value}")
                
                // 既存ユーザー重複チェック
                let! existingUserResult = userRepository.GetByEmailAsync(email)
                
                match existingUserResult with
                | Error dbError ->
                    let! _ = logger.LogErrorAsync($"ユーザー作成失敗: データベースアクセスエラー - {dbError}", None)
                    return Error (SystemError (System.Exception(dbError)))
                | Ok existingUserOption ->
                    
                    match existingUserOption with
                    | Some _ ->
                        let! _ = logger.LogWarningAsync($"ユーザー作成失敗: メールアドレス重複 - {email.Value}")
                        return Error (ValidationError "指定されたメールアドレスは既に使用されています")
                    | None ->
                        
                        // Infrastructure層での認証付きユーザー作成
                        // 【F#初学者向け解説】
                        // パスワードハッシュ化などのInfrastructure特有の処理は、
                        // IAuthenticationServiceに委譲します。これにより、Application層は
                        // ビジネスロジックに専念できます。
                        let! creationResult = authService.CreateUserWithPasswordAsync(email, name, role, password, createdBy)
                        
                        match creationResult with
                        | Error creationError ->
                            let! _ = logger.LogErrorAsync($"ユーザー作成失敗: 認証サービスエラー - {creationError}", None)
                            return Error (SystemError (System.Exception(creationError)))
                        | Ok createdUser ->
                            let! _ = logger.LogInformationAsync($"ユーザー作成成功 - {email.Value}")
                            return Ok createdUser
                            
            with
            | ex ->
                let! _ = logger.LogErrorAsync($"ユーザー作成で予期しないエラーが発生しました", Some ex)
                return Error (SystemError ex)
        }
    
    // 🔐 パスワード変更: セキュリティポリシーに準拠した更新
    // 【F#初学者向け解説】
    // 現在のパスワード確認、新パスワードバリデーション、ハッシュ化、保存の
    // 一連の処理をトランザクション的に実行します。
    member this.ChangeUserPasswordAsync(userId: UserId, currentPassword: string, newPassword: Password): Task<Result<User, AuthenticationError>> =
        task {
            try
                let! _ = logger.LogInformationAsync($"パスワード変更開始 - ユーザーID: {userId.Value}")
                
                // 対象ユーザー取得
                let! userResult = userRepository.GetByIdAsync(userId)
                
                match userResult with
                | Error dbError ->
                    let! _ = logger.LogErrorAsync($"パスワード変更失敗: データベースアクセスエラー - {dbError}", None)
                    return Error (SystemError (System.Exception(dbError)))
                | Ok userOption ->
                    
                    match userOption with
                    | None ->
                        let! _ = logger.LogWarningAsync($"パスワード変更失敗: ユーザー未存在 - ユーザーID: {userId.Value}")
                        return Error (ValidationError "指定されたユーザーが見つかりません")
                    | Some user ->
                        
                        if not user.IsActive then
                            let! _ = logger.LogWarningAsync($"パスワード変更失敗: アカウント無効化 - ユーザーID: {userId.Value}")
                            return Error AccountDeactivated
                        else
                            
                            // Infrastructure層でのパスワード変更処理
                            let! changeResult = authService.ChangePasswordAsync(userId, currentPassword, newPassword)
                            
                            match changeResult with
                            | Error changeError ->
                                let! _ = logger.LogWarningAsync($"パスワード変更失敗: {changeError} - ユーザーID: {userId.Value}")
                                return Error (ValidationError changeError)
                            | Ok passwordHash ->
                                
                                // ドメインロジック適用（パスワード変更）
                                // 【F#初学者向け解説】
                                // ドメインエンティティのメソッドを呼び出してビジネスルールを適用します。
                                // パスワード変更時のセキュリティスタンプ更新なども自動的に処理されます。
                                match user.changePassword passwordHash userId with
                                | Error domainError ->
                                    let! _ = logger.LogErrorAsync($"パスワード変更失敗: ドメインエラー - {domainError}", None)
                                    return Error (ValidationError domainError)
                                | Ok updatedUser ->
                                    
                                    // 永続化
                                    let! saveResult = userRepository.SaveAsync(updatedUser)
                                    
                                    match saveResult with
                                    | Error saveError ->
                                        let! _ = logger.LogErrorAsync($"パスワード変更失敗: 保存エラー - {saveError}", None)
                                        return Error (SystemError (System.Exception(saveError)))
                                    | Ok savedUser ->
                                        let! _ = logger.LogInformationAsync($"パスワード変更成功 - ユーザーID: {userId.Value}")
                                        return Ok savedUser
                                        
            with
            | ex ->
                let! _ = logger.LogErrorAsync($"パスワード変更で予期しないエラーが発生しました", Some ex)
                return Error (SystemError ex)
        }
    
    // 🔍 現在ユーザー取得: セッション情報からの現在ユーザー情報取得
    // 【F#初学者向け解説】
    // Infrastructure層のIAuthenticationServiceから現在のログインユーザーを取得します。
    // セッション管理やHTTPコンテキストアクセスは、Infrastructure層に委譲します。
    member this.GetCurrentUserAsync(): Task<Result<User option, AuthenticationError>> =
        task {
            try
                let! currentUserResult = authService.GetCurrentUserAsync()
                
                match currentUserResult with
                | Error authError ->
                    let! _ = logger.LogWarningAsync($"現在ユーザー取得失敗 - {authError}")
                    return Error (SystemError (System.Exception(authError)))
                | Ok userOption ->
                    return Ok userOption
                    
            with
            | ex ->
                let! _ = logger.LogErrorAsync($"現在ユーザー取得で予期しないエラーが発生しました", Some ex)
                return Error (SystemError ex)
        }

// 🎯 認証ユースケース実装（関数型スタイル）
// 【F#初学者向け解説】
// F#では、クラスベースのサービスに加えて、関数型スタイルでのユースケース実装も可能です。
// モジュール内の関数として定義することで、より軽量で関数型らしい実装ができます。
module AuthenticationUseCases =
    
    // 🔑 ログインユースケース: Railway-oriented Programmingによる実装
    // 【F#初学者向け解説】
    // 複数の処理を連鎖させ、途中でエラーが発生すると短絡評価される関数型パターンです。
    // パイプライン演算子（|>）を使用して、データフローを明確に表現できます。
    let loginUser (authService: AuthenticationApplicationService) (request: AuthenticationRequest) =
        task {
            let! result = authService.AuthenticateUserAsync(request)
            
            return
                result
                |> Result.map (fun authResult ->
                    match authResult with
                    | AuthenticationSuccess (user, token) ->
                        {| User = user; Token = Some token; RequiresPasswordChange = false; RequiresEmailConfirmation = false |}
                    | FirstLoginRequired user ->
                        {| User = user; Token = None; RequiresPasswordChange = true; RequiresEmailConfirmation = false |}
                    | EmailConfirmationRequired user ->
                        {| User = user; Token = None; RequiresPasswordChange = false; RequiresEmailConfirmation = true |})
        }
    
    // 🔐 パスワード変更ユースケース: 型安全なパスワード更新
    let changePassword (authService: AuthenticationApplicationService) (userId: UserId) (currentPassword: string) (newPasswordStr: string) =
        task {
            // 新パスワードのバリデーション
            match Password.create newPasswordStr with
            | Error passwordError ->
                return Error (ValidationError passwordError)
            | Ok validNewPassword ->
                
                return! authService.ChangeUserPasswordAsync(userId, currentPassword, validNewPassword)
        }
    
    // 👤 ユーザー作成ユースケース: 完全なユーザー登録フロー
    let createUser (authService: AuthenticationApplicationService) (emailStr: string) (nameStr: string) (roleStr: string) (passwordStr: string) (createdBy: UserId) =
        task {
            // すべてのパラメータのバリデーション
            let emailResult = Email.create emailStr
            let nameResult = UserName.create nameStr
            let passwordResult = Password.create passwordStr
            
            // ロールの解析（文字列からRole型への変換）
            // 【F#初学者向け解説】
            // パターンマッチングでロール文字列を適切なRole型に変換します。
            // デフォルトケースを設けることで、未知のロール文字列に対する安全性を確保します。
            let role = 
                match roleStr.ToLowerInvariant() with
                | "superuser" -> SuperUser
                | "projectmanager" -> ProjectManager
                | "domainapprover" -> DomainApprover
                | _ -> GeneralUser
            
            match emailResult, nameResult, passwordResult with
            | Ok email, Ok name, Ok password ->
                return! authService.CreateUserWithPasswordAsync(email, name, role, password, createdBy)
            | Error emailErr, _, _ ->
                return Error (ValidationError emailErr)
            | _, Error nameErr, _ ->
                return Error (ValidationError nameErr)
            | _, _, Error passwordErr ->
                return Error (ValidationError passwordErr)
        }