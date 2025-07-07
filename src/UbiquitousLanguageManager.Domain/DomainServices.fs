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
    let validateApprovalAuthorization (approverId: UserId) (approverRole: UserRole) (domain: Domain) =
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