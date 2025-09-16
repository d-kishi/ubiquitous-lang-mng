namespace UbiquitousLanguageManager.Application

open UbiquitousLanguageManager.Domain
open System.Threading.Tasks

// 🎯 認証Application層サービス: F# Railway-oriented Programming実装
// Phase A9 Step 1-1: Clean Architectureに完全準拠したF#認証ビジネスロジック

// 🚦 拡張版認証エラー型: より包括的なエラー分類（Phase A9拡張）
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
    // 🔐 パスワードリセット関連エラー (Phase A9追加)
    | PasswordResetTokenExpired of Email        // パスワードリセットトークン期限切れ
    | PasswordResetTokenInvalid of Email        // 無効なパスワードリセットトークン
    | PasswordResetNotRequested of Email        // パスワードリセット要求なし
    | PasswordResetAlreadyUsed of Email         // 既に使用済みのリセットトークン
    // 🔒 トークン関連エラー (Phase A9追加)
    | TokenGenerationFailed of string           // トークン生成失敗
    | TokenValidationFailed of string           // トークン検証失敗
    | TokenExpired of string                    // トークン期限切れ
    | TokenRevoked of string                    // 取り消し済みトークン
    // 👮 管理者操作関連エラー (Phase A9追加)
    | InsufficientPermissions of Role * Permission // 権限不足（現在ロール * 必要権限）
    | OperationNotAllowed of string             // 操作が許可されていない
    | ConcurrentOperationDetected of string     // 並行操作検出
    // 🔮 将来拡張用エラー (Phase A9準備)
    | TwoFactorAuthRequired of Email            // 2要素認証必須
    | TwoFactorAuthFailed of Email              // 2要素認証失敗
    | ExternalAuthenticationFailed of string    // 外部認証失敗（OAuth/OIDC等）
    | AuditLogError of string                   // 監査ログエラー

// 🎯 認証リクエスト型: 入力データの型安全な表現
// 【F#初学者向解説】
// レコード型を使用して認証リクエストを構造化します。
// 不変性により、データの整合性が保証されます。
type AuthenticationRequest = {
    Email: string                              // メールアドレス（文字列）
    Password: string                           // パスワード（平文）
    RememberMe: bool                           // ログイン状態保持フラグ
}

// 🎯 拡張版認証結果型: より包括的な認証フロー対応（Phase A9拡張）
// 【F#初学者向け解説】
// 認証成功時の情報を構造化して表現します。
// パターンマッチングで各結果タイプを明示的に処理できます。
type AuthenticationResult =
    | AuthenticationSuccess of User * token: string    // 認証成功（ユーザー + トークン）
    | FirstLoginRequired of User                       // 初回ログイン（パスワード変更要求）
    | EmailConfirmationRequired of User                // メール確認要求
    // 🔐 パスワード関連結果 (Phase A9追加)
    | PasswordChangeRequired of User * reason: string  // パスワード変更必須（理由付き）
    | PasswordExpired of User                          // パスワード期限切れ
    // 🔒 多段階認証結果 (Phase A9準備)
    | TwoFactorRequired of User * method: string       // 2要素認証必須（方法指定）
    | TwoFactorSetupRequired of User                   // 2要素認証設定必須
    // 🚨 セキュリティ関連結果 (Phase A9追加)
    | SecurityWarning of User * warning: string        // セキュリティ警告
    | TemporaryLockout of User * retryAfter: System.DateTime // 一時ロックアウト

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

    // 🔐 Phase A9: パスワードリセットトークン生成機能
    // 【F#初学者向け解説】
    // セキュアなパスワードリセット機能を実装します。
    // 時間制限付きトークンを生成し、セキュリティログと統合します。
    member this.RequestPasswordResetAsync(email: Email): Task<Result<string, AuthenticationError>> =
        task {
            try
                let! _ = logger.LogInformationAsync($"パスワードリセット要求開始 - {email.Value}")

                // ユーザー存在確認
                let! userResult = userRepository.GetByEmailAsync(email)

                match userResult with
                | Error dbError ->
                    let! _ = logger.LogErrorAsync($"パスワードリセット失敗: データベースアクセスエラー - {dbError}", None)
                    return Error (SystemError (System.Exception(dbError)))
                | Ok userOption ->

                    match userOption with
                    | None ->
                        // セキュリティ: 存在しないユーザーでも成功レスポンスを返す（情報漏洩防止）
                        let! _ = logger.LogWarningAsync($"パスワードリセット要求: 存在しないユーザー - {email.Value}")
                        return Ok "パスワードリセット要求を受け付けました。指定されたメールアドレスに案内を送信しました。"
                    | Some user ->

                        if not user.IsActive then
                            let! _ = logger.LogWarningAsync($"パスワードリセット失敗: 無効化されたアカウント - {email.Value}")
                            return Error (AccountDeactivated)
                        else

                            // Infrastructure層でのトークン生成（30分有効期限）
                            // 【F#初学者向け解説】
                            // トークンの生成・有効期限管理はInfrastructure層に委譲します。
                            // Application層はビジネスルールに専念し、技術的詳細は抽象化します。
                            let! tokenResult = authService.GeneratePasswordResetTokenAsync(user.Id, System.TimeSpan.FromMinutes(30.0))

                            match tokenResult with
                            | Error tokenError ->
                                let! _ = logger.LogErrorAsync($"パスワードリセットトークン生成失敗 - {tokenError}", None)
                                return Error (TokenGenerationFailed tokenError)
                            | Ok resetToken ->

                                // セキュリティログ記録
                                let! _ = logger.LogInformationAsync($"パスワードリセットトークン生成成功 - {email.Value}")

                                return Ok "パスワードリセット要求を受け付けました。指定されたメールアドレスに案内を送信しました。"

            with
            | ex ->
                let! _ = logger.LogErrorAsync($"パスワードリセット要求で予期しないエラーが発生しました", Some ex)
                return Error (SystemError ex)
        }

    // 🔓 Phase A9: パスワードリセット実行機能
    // 【F#初学者向け解説】
    // トークン検証とパスワード更新を安全に実行します。
    // ワンタイムトークンの使用済みマークとセキュリティスタンプ更新も行います。
    member this.ResetPasswordAsync(email: Email, resetToken: string, newPassword: Password): Task<Result<string, AuthenticationError>> =
        task {
            try
                let! _ = logger.LogInformationAsync($"パスワードリセット実行開始 - {email.Value}")

                // Step 1: トークン検証
                let! tokenValidationResult = authService.ValidatePasswordResetTokenAsync(email, resetToken)

                match tokenValidationResult with
                | Error validationError ->
                    let! _ = logger.LogWarningAsync($"パスワードリセット失敗: トークン検証エラー - {validationError}")
                    match validationError with
                    | "Token expired" -> return Error (PasswordResetTokenExpired email)
                    | "Token invalid" -> return Error (PasswordResetTokenInvalid email)
                    | "Token already used" -> return Error (PasswordResetAlreadyUsed email)
                    | "No reset request" -> return Error (PasswordResetNotRequested email)
                    | _ -> return Error (TokenValidationFailed validationError)
                | Ok user ->

                    // Step 2: 新パスワードハッシュ化
                    let! hashResult = authService.HashPasswordAsync(newPassword)

                    match hashResult with
                    | Error hashError ->
                        let! _ = logger.LogErrorAsync($"パスワードハッシュ化失敗 - {hashError}", None)
                        return Error (SystemError (System.Exception(hashError)))
                    | Ok passwordHash ->

                        // Step 3: ドメインロジック適用（パスワード更新・セキュリティスタンプ更新）
                        match user.resetPassword passwordHash user.Id with
                        | Error domainError ->
                            let! _ = logger.LogErrorAsync($"パスワードリセット失敗: ドメインエラー - {domainError}", None)
                            return Error (ValidationError domainError)
                        | Ok updatedUser ->

                            // Step 4: 永続化
                            let! saveResult = userRepository.SaveAsync(updatedUser)

                            match saveResult with
                            | Error saveError ->
                                let! _ = logger.LogErrorAsync($"パスワードリセット失敗: 保存エラー - {saveError}", None)
                                return Error (SystemError (System.Exception(saveError)))
                            | Ok savedUser ->

                                // Step 5: トークン無効化
                                let! _ = authService.InvalidatePasswordResetTokenAsync(email, resetToken)

                                let! _ = logger.LogInformationAsync($"パスワードリセット成功 - {email.Value}")
                                return Ok "パスワードが正常にリセットされました。新しいパスワードでログインしてください。"

            with
            | ex ->
                let! _ = logger.LogErrorAsync($"パスワードリセット実行で予期しないエラーが発生しました", Some ex)
                return Error (SystemError ex)
        }

    // 🔒 Phase A9: 強化版アカウントロック機能
    // 【F#初学者向け解説】
    // より詳細なロック条件設定と段階的ロック機能を実装します。
    // 警告→一時ロック→永続ロックの段階的なセキュリティ強化を行います。
    member this.ProcessFailedLoginAttemptAsync(email: Email): Task<Result<AuthenticationResult, AuthenticationError>> =
        task {
            try
                let! _ = logger.LogInformationAsync($"ログイン失敗処理開始 - {email.Value}")

                let! userResult = userRepository.GetByEmailAsync(email)

                match userResult with
                | Error dbError ->
                    let! _ = logger.LogErrorAsync($"ログイン失敗処理エラー: データベースアクセス失敗 - {dbError}", None)
                    return Error (SystemError (System.Exception(dbError)))
                | Ok userOption ->

                    match userOption with
                    | None ->
                        // ユーザーが存在しない場合でもセキュリティログを記録
                        let! _ = logger.LogWarningAsync($"ログイン失敗: 存在しないユーザー - {email.Value}")
                        return Error (UserNotFound email)
                    | Some user ->

                        // 段階的ロック機能
                        let maxAttempts = 5
                        let warningThreshold = 3
                        let shortLockoutMinutes = 15.0
                        let longLockoutMinutes = 60.0

                        let currentAttempts = user.FailedAccessAttempts
                        let updatedUser = user.recordFailedAccess maxAttempts (System.TimeSpan.FromMinutes(longLockoutMinutes))

                        let! _ = userRepository.SaveAsync(updatedUser)

                        // 段階的な対応処理
                        // 【F#初学者向け解説】
                        // パターンマッチングを使用して、失敗回数に応じた適切な対応を実行します。
                        // これにより、セキュリティレベルを段階的に強化できます。
                        match currentAttempts + 1 with
                        | attempts when attempts = warningThreshold ->
                            let! _ = logger.LogWarningAsync($"ログイン失敗警告 - {email.Value} (失敗回数: {attempts})")
                            return Ok (SecurityWarning (updatedUser, $"ログイン失敗が{attempts}回発生しました。あと{maxAttempts - attempts}回失敗するとアカウントがロックされます。"))
                        | attempts when attempts < maxAttempts ->
                            let! _ = logger.LogWarningAsync($"ログイン失敗 - {email.Value} (失敗回数: {attempts})")
                            return Error (InvalidCredentials $"メールアドレスまたはパスワードが正しくありません（残り試行回数: {maxAttempts - attempts}回）")
                        | attempts when attempts = maxAttempts ->
                            let retryAfter = System.DateTime.UtcNow.AddMinutes(shortLockoutMinutes)
                            let! _ = logger.LogWarningAsync($"アカウント一時ロック - {email.Value} (失敗回数: {attempts})")
                            return Ok (TemporaryLockout (updatedUser, retryAfter))
                        | _ ->
                            let! _ = logger.LogErrorAsync($"アカウント永続ロック - {email.Value} (失敗回数: {currentAttempts + 1})", None)
                            return Error (AccountLocked updatedUser.LockoutEnd.Value)

            with
            | ex ->
                let! _ = logger.LogErrorAsync($"ログイン失敗処理で予期しないエラーが発生しました", Some ex)
                return Error (SystemError ex)
        }

    // 🔓 Phase A9: アカウントロック解除機能
    // 【F#初学者向け解説】
    // 管理者によるアカウントロック解除機能を実装します。
    // 権限チェックとセキュリティログの記録も含みます。
    member this.UnlockUserAccountAsync(targetUserId: UserId, adminUserId: UserId): Task<Result<string, AuthenticationError>> =
        task {
            try
                let! _ = logger.LogInformationAsync($"アカウントロック解除開始 - 対象ユーザーID: {targetUserId.Value}, 実行者ID: {adminUserId.Value}")

                // 管理者権限チェック
                let! adminResult = userRepository.GetByIdAsync(adminUserId)

                match adminResult with
                | Error dbError ->
                    return Error (SystemError (System.Exception(dbError)))
                | Ok adminOption ->

                    match adminOption with
                    | None ->
                        return Error (ValidationError "実行者ユーザーが見つかりません")
                    | Some admin ->

                        // 権限確認（プロジェクト管理者以上の権限が必要）
                        if not (PermissionMappings.hasPermission admin.Role ManageUserRoles) then
                            let! _ = logger.LogWarningAsync($"アカウントロック解除失敗: 権限不足 - 実行者ID: {adminUserId.Value}")
                            return Error (InsufficientPermissions (admin.Role, ManageUserRoles))
                        else

                            // 対象ユーザー取得
                            let! targetResult = userRepository.GetByIdAsync(targetUserId)

                            match targetResult with
                            | Error dbError ->
                                return Error (SystemError (System.Exception(dbError)))
                            | Ok targetOption ->

                                match targetOption with
                                | None ->
                                    return Error (ValidationError "対象ユーザーが見つかりません")
                                | Some targetUser ->

                                    // ロック解除処理
                                    let unlockedUser = targetUser.unlockAccount()

                                    let! saveResult = userRepository.SaveAsync(unlockedUser)

                                    match saveResult with
                                    | Error saveError ->
                                        return Error (SystemError (System.Exception(saveError)))
                                    | Ok savedUser ->

                                        let! _ = logger.LogInformationAsync($"アカウントロック解除成功 - 対象ユーザーID: {targetUserId.Value}, 実行者ID: {adminUserId.Value}")
                                        return Ok "アカウントのロックが正常に解除されました。"

            with
            | ex ->
                let! _ = logger.LogErrorAsync($"アカウントロック解除で予期しないエラーが発生しました", Some ex)
                return Error (SystemError ex)
        }

    // 🔮 Phase A9: 将来拡張用 - 2要素認証設定準備
    // 【F#初学者向け解説】
    // 将来の2要素認証機能のための基盤を準備します。
    // 現在は基本的な設定管理のみ実装し、実際の認証処理は将来実装予定です。
    member this.Prepare2FASetupAsync(userId: UserId): Task<Result<string, AuthenticationError>> =
        task {
            try
                let! _ = logger.LogInformationAsync($"2要素認証設定準備開始 - ユーザーID: {userId.Value}")

                let! userResult = userRepository.GetByIdAsync(userId)

                match userResult with
                | Error dbError ->
                    return Error (SystemError (System.Exception(dbError)))
                | Ok userOption ->

                    match userOption with
                    | None ->
                        return Error (ValidationError "ユーザーが見つかりません")
                    | Some user ->

                        if not user.IsActive then
                            return Error AccountDeactivated
                        else

                            // 将来実装: バックアップコード生成、QRコード生成など
                            let! _ = logger.LogInformationAsync($"2要素認証設定準備完了 - ユーザーID: {userId.Value}")
                            return Ok "2要素認証の設定準備が完了しました。（機能は将来実装予定）"

            with
            | ex ->
                let! _ = logger.LogErrorAsync($"2要素認証設定準備で予期しないエラーが発生しました", Some ex)
                return Error (SystemError ex)
        }

    // 🔍 Phase A9: 監査ログ統合準備
    // 【F#初学者向け解説】
    // 将来の監査ログ機能のための基盤を準備します。
    // 認証関連の重要な操作を構造化されたログとして記録します。
    member this.LogAuditEventAsync(userId: UserId, eventType: string, details: string): Task<Result<unit, AuthenticationError>> =
        task {
            try
                // 将来実装: 構造化監査ログの出力
                // JSON形式での詳細ログ、外部監査システム連携など
                let! _ = logger.LogInformationAsync($"[AUDIT] {eventType} - ユーザーID: {userId.Value}, 詳細: {details}")
                return Ok ()

            with
            | ex ->
                let! _ = logger.LogErrorAsync($"監査ログ記録で予期しないエラーが発生しました", Some ex)
                return Error (AuditLogError ex.Message)
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
                        {| User = user; Token = Some token; RequiresPasswordChange = false; RequiresEmailConfirmation = false; Warning = None |}
                    | FirstLoginRequired user ->
                        {| User = user; Token = None; RequiresPasswordChange = true; RequiresEmailConfirmation = false; Warning = None |}
                    | EmailConfirmationRequired user ->
                        {| User = user; Token = None; RequiresPasswordChange = false; RequiresEmailConfirmation = true; Warning = None |}
                    // Phase A9追加: 新しい認証結果パターン
                    | PasswordChangeRequired (user, reason) ->
                        {| User = user; Token = None; RequiresPasswordChange = true; RequiresEmailConfirmation = false; Warning = Some reason |}
                    | PasswordExpired user ->
                        {| User = user; Token = None; RequiresPasswordChange = true; RequiresEmailConfirmation = false; Warning = Some "パスワードの有効期限が切れています" |}
                    | TwoFactorRequired (user, method) ->
                        {| User = user; Token = None; RequiresPasswordChange = false; RequiresEmailConfirmation = false; Warning = Some (sprintf "2要素認証が必要です(%s)" method) |}
                    | TwoFactorSetupRequired user ->
                        {| User = user; Token = None; RequiresPasswordChange = false; RequiresEmailConfirmation = false; Warning = Some "2要素認証の設定が必要です" |}
                    | SecurityWarning (user, warning) ->
                        {| User = user; Token = None; RequiresPasswordChange = false; RequiresEmailConfirmation = false; Warning = Some warning |}
                    | TemporaryLockout (user, retryAfter) ->
                        {| User = user; Token = None; RequiresPasswordChange = false; RequiresEmailConfirmation = false; Warning = Some (sprintf "一時的にロックアウトされています。%s以降に再試行してください" (retryAfter.ToString("yyyy/MM/dd HH:mm"))) |})
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