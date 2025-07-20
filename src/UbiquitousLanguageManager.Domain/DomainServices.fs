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
    let validateApprovalAuthorization (approverId: UserId) (approverRole: Role) (domain: Domain) =
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

// 👤 Phase A2: ユーザー管理ドメインサービス
// 【F#初学者向け解説】
// ユーザー管理に関する複雑なビジネスルールを集約したドメインサービスです。
// 単一のUserエンティティでは表現できない、複数のユーザー間の関係性や
// システム全体にまたがるビジネスルールを実装します。
module UserDomainService =
    
    // 🔐 ユーザー作成権限検証: 新規ユーザー作成権限の詳細チェック
    // 【F#初学者向け解説】
    // この関数はResult型を返します。成功時はOk()、失敗時はError文字列を返し、
    // エラー処理をコンパイル時に強制することで、権限チェック漏れを防ぎます。
    let validateUserCreationPermission (operatorUser: User) (targetRole: Role) : Result<unit, string> =
        // 操作者自身がアクティブである必要
        if not operatorUser.IsActive then
            Error "非アクティブなユーザーは新規ユーザーを作成できません"
        // SuperUser作成はSuperUserのみ可能
        elif targetRole = SuperUser && operatorUser.Role <> SuperUser then
            Error "SuperUserの作成はSuperUserのみが実行できます"
        // ユーザー作成権限の確認
        elif not (PermissionMappings.hasPermission operatorUser.Role CreateUsers) then
            Error "ユーザー作成の権限がありません"
        else
            Ok ()
    
    // 📧 メールアドレス重複チェック: システム全体での一意性保証
    let validateUniqueEmail (email: Email) (existingUsers: User list) : Result<unit, string> =
        let isDuplicate = 
            existingUsers 
            |> List.exists (fun user -> user.Email.Value = email.Value && user.IsActive)
        
        if isDuplicate then
            Error "このメールアドレスは既に使用されています"
        else
            Ok ()
    
    // 🎭 ロール変更権限検証: 複雑なロール変更ルールの実装
    // 【F#初学者向け解説】
    // パターンマッチングを使用して、すべてのロール変更パターンを網羅的にチェックします。
    // F#のコンパイラは、すべてのケースが処理されているかを確認するため、
    // ビジネスルールの漏れを防ぐことができます。
    let validateRoleChangeAuthorization (operatorUser: User) (targetUser: User) (newRole: Role) : Result<unit, string> =
        match operatorUser.Role, targetUser.Role, newRole with
        // SuperUser関連の制限
        | SuperUser, _, _ -> Ok () // SuperUserはすべての変更が可能
        | _, SuperUser, _ -> Error "SuperUserのロール変更はSuperUserのみが実行できます"
        | _, _, SuperUser -> Error "SuperUserへの昇格はSuperUserのみが実行できます"
        
        // ProjectManager以下の権限チェック
        | ProjectManager, targetRole, newRole when targetRole <> SuperUser && newRole <> SuperUser -> 
            Ok () // ProjectManagerはSuperUser以外の変更が可能
        | DomainApprover, targetRole, newRole when targetRole = GeneralUser && newRole = DomainApprover -> 
            Ok () // DomainApproverはGeneralUserをDomainApproverに昇格可能
        | DomainApprover, targetRole, newRole when targetRole = DomainApprover && newRole = GeneralUser -> 
            Ok () // DomainApproverはDomainApproverをGeneralUserに降格可能
        
        // その他は権限不足
        | _ -> Error "このロール変更を実行する権限がありません"
    
    // 🏢 プロジェクト権限整合性チェック: ユーザーのロールとプロジェクト権限の整合性検証
    let validateProjectPermissionsConsistency (user: User) : Result<unit, string> =
        // GlobalRoleで既に権限を持っている場合、重複するProjectPermissionは不要
        let globalPermissions = PermissionMappings.getPermissionsForRole user.Role
        
        let redundantPermissions = 
            user.ProjectPermissions
            |> List.collect (fun projectPerm -> Set.toList projectPerm.Permissions)
            |> List.filter (fun permission -> Set.contains permission globalPermissions)
        
        if not (List.isEmpty redundantPermissions) then
            // Warning: 重複権限があるが、システム動作には影響しない
            Error $"グローバルロールで既に持っている権限が重複しています: {redundantPermissions}"
        else
            Ok ()
    
    // 🔒 アカウント無効化権限検証: 無効化対象と操作者の関係性チェック
    let validateUserDeactivationPermission (operatorUser: User) (targetUser: User) : Result<unit, string> =
        // 自分自身の無効化は禁止
        if operatorUser.Id = targetUser.Id then
            Error "自分自身のアカウントを無効化することはできません"
        // 非アクティブユーザーの無効化は無意味
        elif not targetUser.IsActive then
            Error "既に無効化されているユーザーです"
        // SuperUserの無効化はSuperUserのみ可能
        elif targetUser.Role = SuperUser && operatorUser.Role <> SuperUser then
            Error "SuperUserの無効化はSuperUserのみが実行できます"
        // 削除権限の確認
        elif not (PermissionMappings.hasPermission operatorUser.Role DeleteUsers) then
            Error "ユーザー無効化の権限がありません"
        else
            Ok ()
    
    // 🔐 パスワード変更権限検証: 自分・他人のパスワード変更権限チェック
    let validatePasswordChangePermission (operatorUser: User) (targetUser: User) : Result<unit, string> =
        // 自分のパスワード変更は常に許可（アクティブユーザーのみ）
        if operatorUser.Id = targetUser.Id && operatorUser.IsActive then
            Ok ()
        // 他人のパスワード変更には管理者権限が必要
        elif operatorUser.Id <> targetUser.Id then
            if PermissionMappings.hasPermission operatorUser.Role ManageUserRoles then
                Ok ()
            else
                Error "他のユーザーのパスワードを変更する権限がありません"
        else
            Error "非アクティブなユーザーはパスワードを変更できません"
    
    // 👥 同時ログインユーザー数制限チェック: システムリソース保護
    // 【F#初学者向け解説】
    // List.filterを使用してアクティブユーザーをフィルタリングし、
    // List.lengthで数を取得します。F#のパイプライン演算子(|>)により、
    // データの流れが左から右に明確に表現されています。
    let validateConcurrentUserLimit (currentActiveUsers: User list) (maxConcurrentUsers: int) : Result<unit, string> =
        let activeUserCount = 
            currentActiveUsers 
            |> List.filter (fun user -> user.IsActive)
            |> List.length
        
        if activeUserCount >= maxConcurrentUsers then
            Error $"同時ログイン可能なユーザー数の上限（{maxConcurrentUsers}人）に達しています"
        else
            Ok ()
    
    // 🎯 ユーザー管理業務検証: 複数の検証を組み合わせた総合チェック
    // 【F#初学者向け解説】
    // 複数のResult型を連鎖的に処理します。
    // エラーが発生した時点で処理が停止し、最初のエラーが返されます。
    let validateUserManagementOperation (operatorUser: User) (targetUser: User option) (operation: string) : Result<unit, string> =
        // 操作者のアクティブ状態確認
        if not operatorUser.IsActive then
            Error "非アクティブなユーザーはユーザー管理操作を実行できません"
        // 基本的なユーザー管理権限確認
        elif not (PermissionMappings.hasPermission operatorUser.Role ViewUsers) then
            Error "ユーザー管理機能へのアクセス権限がありません"
        else
            // 対象ユーザーが指定されている場合の追加検証
            match targetUser with
            | Some target ->
                // SuperUser関連の制限チェック
                if target.Role = SuperUser && operatorUser.Role <> SuperUser then
                    Error "SuperUserに対する操作はSuperUserのみが実行できます"
                else
                    Ok () // すべての検証をパス
            | None -> 
                Ok () // 対象ユーザーなしの操作（一覧表示等）は追加チェック不要