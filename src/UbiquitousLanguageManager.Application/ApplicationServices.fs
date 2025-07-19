namespace UbiquitousLanguageManager.Application

open UbiquitousLanguageManager.Domain
open System.Threading.Tasks

// 🎯 Application Service: ユースケースの実装とドメインロジックの調整
// Clean Architectureにおけるアプリケーション層の中核

// 👤 ユーザー管理サービス: ユーザー関連のユースケース実装
type UserApplicationService(
    userRepository: IUserRepository,
    authService: IAuthenticationService,
    notificationService: INotificationService) =
    
    // 👥 新規ユーザー登録: 業務フローに応じたユーザー作成
    member this.CreateUserAsync(email: Email, name: UserName, role: UserRole, createdBy: UserId) =
        task {
            // 🔍 重複チェック: 既存ユーザーとの競合確認
            let! existingUserResult = userRepository.GetByEmailAsync(email)
            
            return!
                match existingUserResult with
                | Error err -> Task.FromResult(Error err)
                | Ok existingUser ->
                    match existingUser with
                    | Some _ -> Task.FromResult(Error "指定されたメールアドレスは既に使用されています")
                    | None ->
                        // 🔧 ドメインエンティティ作成: ファクトリーメソッド使用
                        let newUser = User.create email name role createdBy
                        
                        // 💾 永続化: Infrastructureへの委譲
                        task {
                            let! saveResult = userRepository.SaveAsync(newUser)
                            return saveResult
                        }
        }
    
    // 🔐 認証用ユーザー登録: パスワード付きユーザー作成
    // 【F#初学者向け解説】
    // 通常のユーザー登録と異なり、パスワードハッシュを含む完全な認証情報を設定します。
    // Infrastructure層でASP.NET Core Identityと連携して実行されます。
    member this.RegisterUserWithAuthenticationAsync(email: Email, name: UserName, role: UserRole, password: string, createdBy: UserId) =
        task {
            // 🔍 重複チェック
            let! existingUserResult = userRepository.GetByEmailAsync(email)
            
            return!
                match existingUserResult with
                | Error err -> Task.FromResult(Error err)
                | Ok existingUser ->
                    match existingUser with
                    | Some _ -> Task.FromResult(Error "指定されたメールアドレスは既に使用されています")
                    | None ->
                        // 🔐 認証サービスでのユーザー作成（パスワードハッシュ化含む）
                        task {
                            let! createResult = authService.RegisterUserAsync(email, name, role, password, createdBy)
                            return createResult
                        }
        }
    
    // 🔑 ユーザーログイン: 認証処理と初回ログインチェック
    member this.LoginAsync(email: Email, password: string) =
        task {
            let! loginResult = authService.LoginAsync(email, password)
            
            return
                match loginResult with
                | Error err -> Error err
                | Ok user ->
                    // 🎯 初回ログインチェック: パスワード変更必須判定
                    if user.IsFirstLogin then
                        Error "初回ログインのため、パスワード変更が必要です"
                    else
                        Ok user
        }
    
    // 🔐 パスワード変更: セキュリティポリシーに準拠した更新
    member this.ChangePasswordAsync(userId: UserId, oldPassword: string, newPassword: string) =
        task {
            // 🔐 認証サービスでパスワード変更処理
            let! changeResult = authService.ChangePasswordAsync(userId, oldPassword, newPassword)
            
            match changeResult with
            | Error err -> return Error err
            | Ok () ->
                // 🔍 ユーザー情報の更新: 初回ログインフラグのクリア
                let! userResult = userRepository.GetByIdAsync(userId)
                
                return!
                    match userResult with
                    | Error err -> Task.FromResult(Error err)
                    | Ok userOpt ->
                        match userOpt with
                        | None -> Task.FromResult(Error "ユーザーが見つかりません")
                        | Some user ->
                            // 🎯 初回ログインフラグをクリア
                            let updatedUser = { user with IsFirstLogin = false }
                            task {
                                let! saveResult = userRepository.SaveAsync(updatedUser)
                                return
                                    match saveResult with
                                    | Ok _ -> Ok ()
                                    | Error err -> Error err
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
                        match DomainService.validateUserCanCreateInDomain createdBy domain with
                        | Error err -> Task.FromResult(Error err)
                        | Ok () ->
                            // 🔍 重複チェック: 同一ドメイン内での名前重複確認
                            task {
                                let! existingTermsResult = ubiquitousLanguageRepository.GetFormalsByDomainIdAsync(domainId)
                                
                                return!
                                    match existingTermsResult with
                                    | Error err -> Task.FromResult(Error err)
                                    | Ok existingTerms ->
                                        // 🎯 ドメインサービス: 重複検証
                                        match DomainService.validateUniqueNamesInDomain japaneseName englishName existingTerms with
                                        | Error err -> Task.FromResult(Error err)
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
                        | Error err -> Task.FromResult(Error err)
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
                                        match DomainService.validateApprovalAuthorization approvedBy approver.Role domain with
                                        | Error err -> Task.FromResult(Error err)
                                        | Ok () ->
                                            // 🎯 ドメインロジック: 承認処理
                                            match draft.approve approvedBy with
                                            | Error err -> Task.FromResult(Error err)
                                            | Ok approvedDraft ->
                                                // 🔄 正式版への変換
                                                match FormalUbiquitousLanguage.createFromDraft approvedDraft approvedBy with
                                                | Error err -> Task.FromResult(Error err)
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