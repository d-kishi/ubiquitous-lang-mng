namespace UbiquitousLanguageManager.Application

// F# Domain層namespace階層化対応: Bounded Context別にopen
open UbiquitousLanguageManager.Domain.Common                  // UserId, ProjectId, DomainId, Role, Permission, PermissionMappings, Description
open UbiquitousLanguageManager.Domain.Authentication          // User, Email, UserName, Password, PasswordHash, UserDomainService, UserProfile, ProjectPermission
open UbiquitousLanguageManager.Domain.ProjectManagement       // Project, Domain, ProjectError
open UbiquitousLanguageManager.Domain.UbiquitousLanguageManagement  // DraftUbiquitousLanguage, FormalUbiquitousLanguage, UbiquitousLanguageId, JapaneseName, EnglishName, UbiquitousLanguageDomainService
open System.Threading.Tasks

// 🎯 Application Service: ユースケースの実装とドメインロジックの調整
// Clean Architectureにおけるアプリケーション層の中核

// 👤 Phase A2: ユーザー管理アプリケーションサービス（大幅拡張版）
// 【F#初学者向け解説】
// Application Serviceは、ドメインロジックをUIから呼び出し可能にするサービス層です。
// ビジネスルールの実行順序やエラーハンドリング、外部サービスとの連携を管理します。
type UserApplicationService(
    userRepository: IUserRepository,
    authService: IAuthenticationService,
    notificationService: INotificationService,
    logger: ILogger<UserApplicationService>) =
    
    // 👥 新規ユーザー作成: 権限チェック・重複確認・ドメインサービス活用
    // 【F#初学者向け解説】
    // task計算式を使用して非同期処理を行います。F#のtask{}は、
    // C#のasync/awaitと同様の機能を提供し、データベースアクセス等の
    // 非同期操作を効率的に処理できます。
    member this.CreateUserAsync(email: Email, name: UserName, role: Role, operatorUser: User) =
        task {
            // 🔐 ドメインサービス: ユーザー作成権限の検証
            match UserDomainService.validateUserCreationPermission operatorUser role with
            | Error err -> return Error err
            | Ok () ->
                // 🔍 既存ユーザー取得: 重複チェック用
                let! allUsersResult = userRepository.GetAllActiveUsersAsync()
                
                return!
                    match allUsersResult with
                    | Error err -> Task.FromResult(Error err)
                    | Ok existingUsers ->
                        // 🔐 ドメインサービス: メールアドレス重複チェック
                        match UserDomainService.validateUniqueEmail email existingUsers with
                        | Error err -> Task.FromResult(Error err)
                        | Ok () ->
                            // 🔧 ドメインエンティティ作成: ファクトリーメソッド使用
                            let newUser = User.create email name role operatorUser.Id
                            
                            // 💾 永続化: Infrastructure層への委譲
                            task {
                                let! saveResult = userRepository.SaveAsync(newUser)
                                
                                match saveResult with
                                | Ok savedUser ->
                                    // 📧 ウェルカムメール送信（非同期）
                                    let! _ = notificationService.SendWelcomeEmailAsync(savedUser.Email)
                                    return Ok savedUser
                                | Error err -> return Error err
                            }
        }
    
    // 🔐 認証付きユーザー作成: パスワード設定を含む完全なユーザー登録
    member this.CreateUserWithPasswordAsync(email: Email, name: UserName, role: Role, password: Password, operatorUser: User) =
        task {
            // 🔐 ドメインサービス: ユーザー作成権限の検証
            match UserDomainService.validateUserCreationPermission operatorUser role with
            | Error err -> return Error err
            | Ok () ->
                // 🔍 既存ユーザー取得: 重複チェック用
                let! allUsersResult = userRepository.GetAllActiveUsersAsync()
                
                return!
                    match allUsersResult with
                    | Error err -> Task.FromResult(Error err)
                    | Ok existingUsers ->
                        // 🔐 ドメインサービス: メールアドレス重複チェック
                        match UserDomainService.validateUniqueEmail email existingUsers with
                        | Error err -> Task.FromResult(Error err)
                        | Ok () ->
                            // 🔐 認証サービス: パスワードハッシュ化とユーザー作成
                            task {
                                let! createResult = authService.CreateUserWithPasswordAsync(email, name, role, password, operatorUser.Id)
                                
                                match createResult with
                                | Ok (createdUser, _identityId) ->
                                    // 📧 ウェルカムメール送信（非同期）
                                    // _identityId はプロジェクト割り当てが不要なこのフローでは使用しない
                                    let! _ = notificationService.SendWelcomeEmailAsync(createdUser.Email)
                                    return Ok createdUser
                                | Error err -> return Error err
                            }
        }
    
    // 👤 ユーザープロフィール更新: プロフィール情報の更新
    member this.UpdateUserProfileAsync(userId: UserId, newProfile: UserProfile, operatorUser: User) =
        task {
            // 🔍 対象ユーザー取得
            let! userResult = userRepository.GetByIdAsync(userId)
            
            return!
                match userResult with
                | Error err -> Task.FromResult(Error err)
                | Ok userOpt ->
                    match userOpt with
                    | None -> Task.FromResult(Error "指定されたユーザーが見つかりません")
                    | Some targetUser ->
                        // 🔐 ドメインサービス: ユーザー管理権限の検証
                        match UserDomainService.validateUserManagementOperation operatorUser (Some targetUser) "profile_update" with
                        | Error err -> Task.FromResult(Error err)
                        | Ok () ->
                            // 🎯 ドメインロジック: プロフィール更新
                            match targetUser.updateProfile newProfile operatorUser.Id with
                            | Error err -> Task.FromResult(Error err)
                            | Ok updatedUser ->
                                // 💾 永続化
                                task {
                                    let! saveResult = userRepository.SaveAsync(updatedUser)
                                    return saveResult
                                }
        }
    
    // 🎭 ユーザーロール変更: 権限システムでのロール変更管理
    member this.ChangeUserRoleAsync(userId: UserId, newRole: Role, operatorUser: User) =
        task {
            // 🔍 対象ユーザー取得
            let! userResult = userRepository.GetByIdAsync(userId)
            
            return!
                match userResult with
                | Error err -> Task.FromResult(Error err)
                | Ok userOpt ->
                    match userOpt with
                    | None -> Task.FromResult(Error "指定されたユーザーが見つかりません")
                    | Some targetUser ->
                        // 🔐 ドメインサービス: ロール変更権限の検証
                        match UserDomainService.validateRoleChangeAuthorization operatorUser targetUser newRole with
                        | Error err -> Task.FromResult(Error err)
                        | Ok () ->
                            // 🎯 ドメインロジック: ロール変更
                            match targetUser.changeRole newRole operatorUser operatorUser.Id with
                            | Error err -> Task.FromResult(Error err)
                            | Ok updatedUser ->
                                // 💾 永続化
                                task {
                                    let! saveResult = userRepository.SaveAsync(updatedUser)
                                    
                                    match saveResult with
                                    | Ok savedUser ->
                                        // 📧 ロール変更通知メール
                                        let! _ = notificationService.SendRoleChangeNotificationAsync(savedUser.Email, newRole)
                                        return Ok savedUser
                                    | Error err -> return Error err
                                }
        }
    
    // 🏢 プロジェクト権限設定: プロジェクトスコープ権限の管理
    member this.SetProjectPermissionsAsync(userId: UserId, projectPermissions: ProjectPermission list, operatorUser: User) =
        task {
            // 🔍 対象ユーザー取得
            let! userResult = userRepository.GetByIdAsync(userId)
            
            return!
                match userResult with
                | Error err -> Task.FromResult(Error err)
                | Ok userOpt ->
                    match userOpt with
                    | None -> Task.FromResult(Error "指定されたユーザーが見つかりません")
                    | Some targetUser ->
                        // 🎯 ドメインロジック: プロジェクト権限設定
                        match targetUser.setProjectPermissions projectPermissions operatorUser operatorUser.Id with
                        | Error err -> Task.FromResult(Error err)
                        | Ok updatedUser ->
                            // 🔐 ドメインサービス: 権限整合性チェック
                            task {
                                match UserDomainService.validateProjectPermissionsConsistency updatedUser with
                                | Error warning ->
                                    // Warning: 権限重複があるが、動作に影響なし
                                    let! _ = logger.LogWarningAsync($"権限重複警告: {warning}")
                                    
                                    // 💾 永続化（警告があっても保存）
                                    let! saveResult = userRepository.SaveAsync(updatedUser)
                                    return saveResult
                                | Ok () ->
                                    // 💾 永続化
                                    let! saveResult = userRepository.SaveAsync(updatedUser)
                                    return saveResult
                            }
        }
    
    // 🔒 ユーザー無効化: 論理削除による無効化
    member this.DeactivateUserAsync(userId: UserId, operatorUser: User) =
        task {
            // 🔍 対象ユーザー取得
            let! userResult = userRepository.GetByIdAsync(userId)
            
            return!
                match userResult with
                | Error err -> Task.FromResult(Error err)
                | Ok userOpt ->
                    match userOpt with
                    | None -> Task.FromResult(Error "指定されたユーザーが見つかりません")
                    | Some targetUser ->
                        // 🔐 ドメインサービス: 無効化権限の検証
                        match UserDomainService.validateUserDeactivationPermission operatorUser targetUser with
                        | Error err -> Task.FromResult(Error err)
                        | Ok () ->
                            // 🎯 ドメインロジック: ユーザー無効化
                            match targetUser.deactivate operatorUser operatorUser.Id with
                            | Error err -> Task.FromResult(Error err)
                            | Ok deactivatedUser ->
                                // 💾 永続化
                                task {
                                    let! saveResult = userRepository.SaveAsync(deactivatedUser)
                                    
                                    match saveResult with
                                    | Ok savedUser ->
                                        // 📧 無効化通知メール
                                        let! _ = notificationService.SendAccountDeactivationNotificationAsync(savedUser.Email)
                                        return Ok savedUser
                                    | Error err -> return Error err
                                }
        }
    
    // ✅ ユーザー有効化: 無効化されたユーザーの再有効化
    member this.ActivateUserAsync(userId: UserId, operatorUser: User) =
        task {
            // 🔍 対象ユーザー取得
            let! userResult = userRepository.GetByIdAsync(userId)
            
            return!
                match userResult with
                | Error err -> Task.FromResult(Error err)
                | Ok userOpt ->
                    match userOpt with
                    | None -> Task.FromResult(Error "指定されたユーザーが見つかりません")
                    | Some targetUser ->
                        // 🎯 ドメインロジック: ユーザー有効化
                        match targetUser.activate operatorUser operatorUser.Id with
                        | Error err -> Task.FromResult(Error err)
                        | Ok activatedUser ->
                            // 💾 永続化
                            task {
                                let! saveResult = userRepository.SaveAsync(activatedUser)
                                
                                match saveResult with
                                | Ok savedUser ->
                                    // 📧 有効化通知メール
                                    let! _ = notificationService.SendAccountActivationNotificationAsync(savedUser.Email)
                                    return Ok savedUser
                                | Error err -> return Error err
                            }
        }
    
    // 🔑 ユーザーログイン: 認証処理と初回ログインチェック
    member this.LoginAsync(email: Email, password: string) =
        task {
            let! loginResult = authService.LoginAsync(email, password)
            
            return!
                match loginResult with
                | Error err -> Task.FromResult(Error err)
                | Ok user ->
                    // 🎯 ユーザー状態チェック
                    if not user.IsActive then
                        Task.FromResult(Error "アカウントが無効化されています。管理者にお問い合わせください")
                    elif user.isLockedOut() then
                        Task.FromResult(Error "アカウントがロックされています。しばらく待ってから再試行してください")
                    elif user.IsFirstLogin then
                        Task.FromResult(Error "初回ログインのため、パスワード変更が必要です")
                    else
                        // ✅ ログイン成功の記録
                        let successUser = user.recordSuccessfulAccess()
                        task {
                            let! saveResult = userRepository.SaveAsync(successUser)
                            return
                                match saveResult with
                                | Ok savedUser -> Ok savedUser
                                | Error err -> Error err
                        }
        }
    
    // 🔐 パスワード変更: セキュリティポリシーに準拠した更新
    member this.ChangePasswordAsync(userId: UserId, currentPassword: string, newPassword: Password, operatorUser: User) =
        task {
            // 🔍 対象ユーザー取得
            let! userResult = userRepository.GetByIdAsync(userId)
            
            return!
                match userResult with
                | Error err -> Task.FromResult(Error err)
                | Ok userOpt ->
                    match userOpt with
                    | None -> Task.FromResult(Error "指定されたユーザーが見つかりません")
                    | Some targetUser ->
                        // 🔐 ドメインサービス: パスワード変更権限の検証
                        match UserDomainService.validatePasswordChangePermission operatorUser targetUser with
                        | Error err -> Task.FromResult(Error err)
                        | Ok () ->
                            // 🔐 認証サービス: パスワード変更処理（現在のパスワード確認含む）
                            task {
                                let! changeResult = authService.ChangePasswordAsync(userId, currentPassword, newPassword)
                                
                                match changeResult with
                                | Error err -> return Error err
                                | Ok passwordHash ->
                                    // 🎯 ドメインロジック: パスワード変更
                                    match targetUser.changePassword passwordHash operatorUser.Id with
                                    | Error err -> return Error err
                                    | Ok updatedUser ->
                                        // 💾 永続化
                                        let! saveResult = userRepository.SaveAsync(updatedUser)
                                        
                                        match saveResult with
                                        | Ok savedUser ->
                                            // 📧 パスワード変更通知メール
                                            let! _ = notificationService.SendPasswordChangeNotificationAsync(savedUser.Email)
                                            return Ok savedUser
                                        | Error err -> return Error err
                            }
        }
    
    // 🔍 ユーザー検索・一覧取得: 権限に応じたユーザー情報取得
    member this.GetUsersAsync(operatorUser: User, includeInactive: bool) =
        task {
            // 🔐 ドメインサービス: ユーザー管理権限の検証
            match UserDomainService.validateUserManagementOperation operatorUser None "view_users" with
            | Error err -> return Error err
            | Ok () ->
                // 🔍 ユーザー一覧取得（削除済みユーザー制御対応）
                // 【F#初学者向け解説】
                // includeInactiveパラメータをGetAllUsersAsyncに渡します。
                // trueの場合は論理削除ユーザーも含め、falseの場合はアクティブユーザーのみを取得します。
                if includeInactive then
                    return! userRepository.GetAllUsersAsync(true)  // 削除済み含む
                else
                    return! userRepository.GetAllActiveUsersAsync()
        }
    
    // 🔍 ユーザー詳細取得: 特定ユーザーの詳細情報取得
    member this.GetUserByIdAsync(userId: UserId, operatorUser: User) =
        task {
            // 🔐 ドメインサービス: ユーザー管理権限の検証
            match UserDomainService.validateUserManagementOperation operatorUser None "view_user_details" with
            | Error err -> return Error err
            | Ok () ->
                // 🔍 ユーザー詳細取得
                return! userRepository.GetByIdAsync(userId)
        }
    
    // 📧 メールアドレス変更: メールアドレス更新と確認プロセス
    member this.ChangeEmailAsync(userId: UserId, newEmail: Email, operatorUser: User) =
        task {
            // 🔍 対象ユーザー取得
            let! userResult = userRepository.GetByIdAsync(userId)
            
            return!
                match userResult with
                | Error err -> Task.FromResult(Error err)
                | Ok userOpt ->
                    match userOpt with
                    | None -> Task.FromResult(Error "指定されたユーザーが見つかりません")
                    | Some targetUser ->
                        // 🔐 権限チェック: 自分または管理者権限が必要
                        if targetUser.Id <> operatorUser.Id && not (PermissionMappings.hasPermission operatorUser.Role EditUsers) then
                            Task.FromResult(Error "メールアドレス変更の権限がありません")
                        else
                            // 🔍 既存ユーザー取得: 重複チェック用
                            task {
                                let! allUsersResult = userRepository.GetAllActiveUsersAsync()
                                
                                return!
                                    match allUsersResult with
                                    | Error err -> Task.FromResult(Error err)
                                    | Ok existingUsers ->
                                        // 🔐 ドメインサービス: メールアドレス重複チェック
                                        match UserDomainService.validateUniqueEmail newEmail existingUsers with
                                        | Error err -> Task.FromResult(Error err)
                                        | Ok () ->
                                            // 🎯 ドメインロジック: メールアドレス変更
                                            match targetUser.changeEmail newEmail operatorUser.Id with
                                            | Error err -> Task.FromResult(Error err)
                                            | Ok updatedUser ->
                                                // 💾 永続化
                                                task {
                                                    let! saveResult = userRepository.SaveAsync(updatedUser)
                                                    
                                                    match saveResult with
                                                    | Ok savedUser ->
                                                        // 📧 メールアドレス変更確認メール（新旧両方のアドレスに送信）
                                                        let! _ = notificationService.SendEmailChangeConfirmationAsync(targetUser.Email, newEmail)
                                                        return Ok savedUser
                                                    | Error err -> return Error err
                                                }
                            }
        }

// 📝 ユビキタス言語管理サービス: 用語管理のユースケース実装
type UbiquitousLanguageApplicationService(
    ubiquitousLanguageRepository: IUbiquitousLanguageRepository,
    userRepository: IUserRepository,
    domainRepository: IDomainRepository,
    notificationService: INotificationService) =
    
    // 📝 下書き用語作成: 新規用語の作成とビジネスルール適用
    member this.CreateDraftAsync(domainId: DomainId, japaneseName: JapaneseName, 
                                 englishName: EnglishName, description: Description, createdBy: UserId) =
        task {
            // 🏷️ ドメイン存在確認: 作成対象ドメインの検証
            let! domainResult = domainRepository.GetByIdAsync(domainId)
            
            return!
                match domainResult with
                | Error err -> Task.FromResult(Error err)
                | Ok domainOpt ->
                    match domainOpt with
                    | None -> Task.FromResult(Error "指定されたドメインが見つかりません")
                    | Some domain ->
                        // 🔐 ドメインサービス: 作成権限の検証
                        match UbiquitousLanguageDomainService.validateUserCanCreateInDomain createdBy domain.IsActive with
                        | Error err -> Task.FromResult(Error (err.ToMessage()))
                        | Ok () ->
                            // 🔍 重複チェック: 同一ドメイン内での名前重複確認
                            task {
                                let! existingTermsResult = ubiquitousLanguageRepository.GetFormalsByDomainIdAsync(domainId)
                                
                                return!
                                    match existingTermsResult with
                                    | Error err -> Task.FromResult(Error err)
                                    | Ok existingTerms ->
                                        // 🎯 ドメインサービス: 重複検証
                                        match UbiquitousLanguageDomainService.validateUniqueNamesInDomain japaneseName englishName existingTerms with
                                        | Error err -> Task.FromResult(Error (err.ToMessage()))
                                        | Ok () ->
                                            // 🔧 ドメインエンティティ作成
                                            let draft = DraftUbiquitousLanguage.create domainId japaneseName englishName description createdBy
                                            
                                            // 💾 永続化
                                            task {
                                                let! saveResult = ubiquitousLanguageRepository.SaveDraftAsync(draft)
                                                return saveResult
                                            }
                            }
        }
    
    // 📤 承認申請: 下書きから承認フローへの移行
    member this.SubmitForApprovalAsync(ubiquitousLanguageId: UbiquitousLanguageId, submittedBy: UserId) =
        task {
            // 🔍 下書き取得: 対象用語の存在確認
            let! draftResult = ubiquitousLanguageRepository.GetDraftByIdAsync(ubiquitousLanguageId)
            
            return!
                match draftResult with
                | Error err -> Task.FromResult(Error err)
                | Ok draftOpt ->
                    match draftOpt with
                    | None -> Task.FromResult(Error "指定された下書き用語が見つかりません")
                    | Some draft ->
                        // 🎯 ドメインロジック: 承認申請処理
                        match draft.submitForApproval submittedBy with
                        | Error err -> Task.FromResult(Error (err.ToMessage()))
                        | Ok updatedDraft ->
                            // 💾 更新の永続化
                            task {
                                let! saveResult = ubiquitousLanguageRepository.SaveDraftAsync(updatedDraft)
                                
                                // 📧 承認者への通知（非同期処理）
                                // 🔧 注意: 実際の実装では承認者の決定ロジックが必要
                                // ここでは簡略化して通知のみ実行
                                
                                return saveResult
                            }
        }
    
    // ✅ 承認処理: 申請された用語の承認・正式版への変換
    member this.ApproveAsync(ubiquitousLanguageId: UbiquitousLanguageId, approvedBy: UserId) =
        task {
            // 🔍 下書き取得と承認者情報取得
            let! draftResult = ubiquitousLanguageRepository.GetDraftByIdAsync(ubiquitousLanguageId)
            let! approverResult = userRepository.GetByIdAsync(approvedBy)
            
            return!
                match draftResult, approverResult with
                | Error err, _ | _, Error err -> Task.FromResult(Error err)
                | Ok draftOpt, Ok approverOpt ->
                    match draftOpt, approverOpt with
                    | None, _ -> Task.FromResult(Error "指定された下書き用語が見つかりません")
                    | _, None -> Task.FromResult(Error "承認者の情報が見つかりません")
                    | Some draft, Some approver ->
                        // 🏷️ ドメイン情報取得（権限チェック用）
                        task {
                            let! domainResult = domainRepository.GetByIdAsync(draft.DomainId)
                            
                            return!
                                match domainResult with
                                | Error err -> Task.FromResult(Error err)
                                | Ok domainOpt ->
                                    match domainOpt with
                                    | None -> Task.FromResult(Error "関連するドメインが見つかりません")
                                    | Some domain ->
                                        // 🔐 承認権限の検証
                                        match UbiquitousLanguageDomainService.validateApprovalAuthorization approvedBy approver.Role with
                                        | Error err -> Task.FromResult(Error (err.ToMessage()))
                                        | Ok () ->
                                            // 🎯 ドメインロジック: 承認処理
                                            match draft.approve approvedBy with
                                            | Error err -> Task.FromResult(Error (err.ToMessage()))
                                            | Ok approvedDraft ->
                                                // 🔄 正式版への変換
                                                match FormalUbiquitousLanguage.createFromDraft approvedDraft approvedBy with
                                                | Error err -> Task.FromResult(Error (err.ToMessage()))
                                                | Ok formalVersion ->
                                                    // 💾 正式版の永続化
                                                    task {
                                                        let! saveResult = ubiquitousLanguageRepository.SaveFormalAsync(formalVersion)
                                                        
                                                        // 🔧 承認済み下書きの削除（正式版に移行したため）
                                                        // 実際の実装では下書きの削除も検討
                                                        
                                                        return saveResult
                                                    }
                        }
        }