namespace UbiquitousLanguageManager.Application

// F# Domain層namespace階層化対応: Bounded Context別にopen
open UbiquitousLanguageManager.Domain.Common                  // UserId, DomainId, Description
open UbiquitousLanguageManager.Domain.Authentication          // User, Email, UserName, Password, Role (SuperUser, ProjectManager等)
open UbiquitousLanguageManager.Domain.ProjectManagement       // (使用なし)
open UbiquitousLanguageManager.Domain.UbiquitousLanguageManagement  // JapaneseName, EnglishName, UbiquitousLanguageId
open System.Threading.Tasks

// 🎯 ユースケース: ビジネス要求を直接的に表現する機能単位
// Application Serviceを組み合わせて具体的なユーザーストーリーを実現

// 🔧 ユースケースの結果型: 操作結果の構造化表現
type UseCaseResult<'TSuccess> = {
    IsSuccess: bool
    Data: 'TSuccess option
    ErrorMessage: string option
    ValidationErrors: (string * string) list // フィールド名 × エラーメッセージ
}

// 🛠️ ユースケース結果のヘルパー関数
module UseCaseResult =
    
    // ✅ 成功結果の作成
    let success data = {
        IsSuccess = true
        Data = Some data
        ErrorMessage = None
        ValidationErrors = []
    }
    
    // ❌ エラー結果の作成
    let error message = {
        IsSuccess = false
        Data = None
        ErrorMessage = Some message
        ValidationErrors = []
    }
    
    // 🔍 検証エラー結果の作成
    let validationError errors = {
        IsSuccess = false
        Data = None
        ErrorMessage = Some "入力値に問題があります"
        ValidationErrors = errors
    }
    
    // 🔄 Result型からUseCaseResultへの変換
    let fromResult result =
        match result with
        | Ok data -> success data
        | Error message -> error message

// 🔧 コマンド定義: Application層の公開API契約
// Clean Architecture推奨：コマンドはUse Case外部に定義し、型安全性と明確性を確保

// ユーザー登録コマンド: 入力データの構造化
type RegisterUserCommand = {
    Email: string
    Name: string
    Role: string
    CreatedBy: string
}

// C#からのアクセス用ファクトリメソッド
module RegisterUserCommand =
    let create email name role createdBy = {
        Email = email
        Name = name
        Role = role
        CreatedBy = createdBy
    }

// ログインコマンド: 認証と初回ログインチェック用
type LoginCommand = {
    Email: string
    Password: string
}

// C#からのアクセス用ファクトリメソッド
module LoginCommand =
    let create email password = {
        Email = email
        Password = password
    }

// パスワード変更コマンド: セキュリティポリシー適用
type ChangePasswordCommand = {
    UserId: string
    OldPassword: string
    NewPassword: string
    ConfirmPassword: string
}

// C#からのアクセス用ファクトリメソッド
module ChangePasswordCommand =
    let create userId oldPassword newPassword confirmPassword = {
        UserId = userId
        OldPassword = oldPassword
        NewPassword = newPassword
        ConfirmPassword = confirmPassword
    }

// 用語作成コマンド: 下書き用語作成のための入力データ
type CreateUbiquitousLanguageCommand = {
    DomainId: int64
    JapaneseName: string
    EnglishName: string
    Description: string
    CreatedBy: string
}

// 承認申請コマンド: ワークフロー開始処理用
type SubmitForApprovalCommand = {
    UbiquitousLanguageId: int64
    SubmittedBy: string
    Comment: string option // 申請時のコメント（オプション）
}

// 承認処理コマンド: 承認者による最終決定用
type ApprovalCommand = {
    UbiquitousLanguageId: int64
    ApprovedBy: string
    ApprovalComment: string option // 承認時のコメント（オプション）
    IsApproved: bool // true: 承認, false: 却下
}

// 👥 ユーザー管理ユースケース: ユーザー関連の業務フロー
type UserManagementUseCase(userAppService: UserApplicationService) =
    
    // 👥 新規ユーザー登録: 入力検証からドメイン処理までの一連のフロー
    member this.RegisterUserAsync(command: RegisterUserCommand) =
        task {
            // 🔍 入力値検証: Value Objectの作成で検証実行
            let emailResult = Email.create command.Email
            let nameResult = UserName.create command.Name
            
            // 🎭 ロール文字列の解析
            let roleResult = 
                match command.Role.ToLower() with
                | "superuser" -> Ok SuperUser
                | "projectmanager" -> Ok ProjectManager
                | "domainapprover" -> Ok DomainApprover
                | "generaluser" -> Ok GeneralUser
                | _ -> Error "無効なユーザーロールです"
            
            // 🔧 検証結果の集約
            match emailResult, nameResult, roleResult with
            | Ok email, Ok name, Ok role ->
                // ✅ 検証成功: ドメイン処理実行
                // 📝 F#初学者向け解説: CreatedByはUserId型ではなくUserエンティティが必要
                // ここでは仮にシステム管理者を作成して使用（実装時には適切な方法で取得）
                let systemAdmin = User.createSystemAdmin() // 仮のシステム管理者
                let! result = userAppService.CreateUserAsync(email, name, role, systemAdmin)
                return UseCaseResult.fromResult result
                
            | _ ->
                // ❌ 検証失敗: エラー情報の収集
                let errors = [
                    if Result.isError emailResult then
                        match emailResult with Error msg -> ("Email", msg) | _ -> ("Email", "不明なエラー")
                    if Result.isError nameResult then
                        match nameResult with Error msg -> ("Name", msg) | _ -> ("Name", "不明なエラー")
                    if Result.isError roleResult then
                        match roleResult with Error msg -> ("Role", msg) | _ -> ("Role", "不明なエラー")
                ]
                return UseCaseResult.validationError errors
        }
    
    // 🔑 ログイン処理: 認証と初回ログインチェック
    member this.LoginAsync(command: LoginCommand) =
        task {
            // 📧 メールアドレス検証
            match Email.create command.Email with
            | Error msg -> return UseCaseResult.validationError [("Email", msg)]
            | Ok email ->
                // 🔐 認証処理実行
                let! result = userAppService.LoginAsync(email, command.Password)
                return UseCaseResult.fromResult result
        }
    
    // 🔐 パスワード変更: セキュリティポリシー適用
    member this.ChangePasswordAsync(command: ChangePasswordCommand) =
        task {
            // 🔒 パスワード確認チェック
            if command.NewPassword <> command.ConfirmPassword then
                return UseCaseResult.validationError [("ConfirmPassword", "新しいパスワードと確認パスワードが一致しません")]
            
            // 🔐 パスワード強度チェック（簡易版）
            elif command.NewPassword.Length < 8 then
                return UseCaseResult.validationError [("NewPassword", "パスワードは8文字以上で入力してください")]
            
            else
                // ✅ 検証通過: パスワード変更実行
                // 📝 F#初学者向け解説: ChangePasswordAsyncには4つの引数が必要
                let passwordResult = Password.create(command.NewPassword)
                match passwordResult with
                | Error err -> return UseCaseResult.validationError [("NewPassword", err)]
                | Ok password ->
                    let systemAdmin = User.createSystemAdmin() // 仮のシステム管理者
                    let! result = userAppService.ChangePasswordAsync(UserId(command.UserId), command.OldPassword, password, systemAdmin)
                    return UseCaseResult.fromResult result
        }

// 📝 ユビキタス言語管理ユースケース: 用語管理の業務フロー
type UbiquitousLanguageManagementUseCase(ubiquitousLanguageAppService: UbiquitousLanguageApplicationService) =
    
    // 📝 新規用語作成: 入力検証からドメイン処理までの完全なフロー
    member this.CreateDraftAsync(command: CreateUbiquitousLanguageCommand) =
        task {
            // 🔍 入力値検証: Value Objectの作成
            let japaneseNameResult = JapaneseName.create command.JapaneseName
            let englishNameResult = EnglishName.create command.EnglishName
            let descriptionResult = Description.create command.Description
            
            // 🔧 検証結果の集約処理
            match japaneseNameResult, englishNameResult, descriptionResult with
            | Ok japaneseName, Ok englishName, Ok description ->
                // ✅ 検証成功: ドメイン処理実行
                let! result = ubiquitousLanguageAppService.CreateDraftAsync(
                    DomainId command.DomainId,
                    japaneseName,
                    englishName,
                    description,
                    UserId(command.CreatedBy))
                return UseCaseResult.fromResult result
                
            | _ ->
                // ❌ 検証失敗: エラー情報の詳細化
                let errors = [
                    if Result.isError japaneseNameResult then
                        match japaneseNameResult with Error msg -> ("JapaneseName", msg) | _ -> ("JapaneseName", "不明なエラー")
                    if Result.isError englishNameResult then
                        match englishNameResult with Error msg -> ("EnglishName", msg) | _ -> ("EnglishName", "不明なエラー")
                    if Result.isError descriptionResult then
                        match descriptionResult with Error msg -> ("Description", msg) | _ -> ("Description", "不明なエラー")
                ]
                return UseCaseResult.validationError errors
        }
    
    // 📤 承認申請ユースケース: ワークフロー開始処理  
    member this.SubmitForApprovalAsync(command: SubmitForApprovalCommand) =
        task {
            // 🎯 承認申請の実行: ID変換とドメイン処理
            let! result = ubiquitousLanguageAppService.SubmitForApprovalAsync(
                UbiquitousLanguageId command.UbiquitousLanguageId,
                UserId(command.SubmittedBy))
            return UseCaseResult.fromResult result
        }
    
    // ✅ 承認処理ユースケース: 承認者による最終決定
    member this.ProcessApprovalAsync(command: ApprovalCommand) =
        task {
            if command.IsApproved then
                // ✅ 承認処理の実行
                let! result = ubiquitousLanguageAppService.ApproveAsync(
                    UbiquitousLanguageId command.UbiquitousLanguageId,
                    UserId(command.ApprovedBy))
                return UseCaseResult.fromResult result
            else
                // ❌ 却下処理（今後実装予定）
                return UseCaseResult.error "却下処理は現在実装中です"
        }

// 🔧 ユースケース集約クラス: 全ユースケースへの統一アクセスポイント
type ApplicationUseCases(
    userAppService: UserApplicationService,
    ubiquitousLanguageAppService: UbiquitousLanguageApplicationService) =
    
    // 👥 ユーザー管理ユースケースへのアクセス
    member this.UserManagement = UserManagementUseCase(userAppService)
    
    // 📝 ユビキタス言語管理ユースケースへのアクセス
    member this.UbiquitousLanguageManagement = UbiquitousLanguageManagementUseCase(ubiquitousLanguageAppService)