namespace UbiquitousLanguageManager.Domain.Authentication

open UbiquitousLanguageManager.Domain.Common

// 🎯 UserDomainService.fs: ユーザー管理ドメインサービス
// Bounded Context: Authentication
// このファイルはユーザー管理に関するドメインサービスを定義します
//
// 【F#初学者向け解説】
// ドメインサービスは、複数のエンティティにまたがるビジネスロジックを実装します。
// 単一のUserエンティティでは表現できない、システム全体にまたがるビジネスルールを実装します。

// 👤 Phase A2: ユーザー管理ドメインサービス
// 【F#初学者向け解説】
// ユーザー管理に関する複雑なビジネスルールを集約したドメインサービスです。
// 単一のUserエンティティでは表現できない、複数のユーザー間の関係性や
// システム全体にまたがるビジネスルールを実装します。
//
// F#のmoduleについて:
// moduleはC#のstatic classに似ていますが、より強力な機能を持ちます。
// 関数、型、値をグループ化し、名前空間のように使用できます。
module UserDomainService =

    // 🔐 ユーザー作成権限検証: 新規ユーザー作成権限の詳細チェック
    // 【F#初学者向け解説】
    // この関数はResult<unit, string>型を返します。
    // unitは「値を返さない」ことを示す型で、C#のvoidに相当します。
    // 成功時はOk()、失敗時はError文字列を返し、
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
    // 【F#初学者向け解説】
    // List.existsは「リストの中に条件を満たす要素が存在するか」をチェックします。
    // System.String.Equalsの第3引数でOrdinalIgnoreCaseを指定することで、
    // 大文字小文字を区別しない比較を行います（user@example.comとUSER@EXAMPLE.COMを同一視）。
    let validateUniqueEmail (email: Email) (existingUsers: User list) : Result<unit, string> =
        let isDuplicate =
            existingUsers
            |> List.exists (fun user ->
                System.String.Equals(user.Email.Value, email.Value, System.StringComparison.OrdinalIgnoreCase)
                && user.IsActive)

        if isDuplicate then
            Error "このメールアドレスは既に使用されています"
        else
            Ok ()

    // 🎭 ロール変更権限検証: 複雑なロール変更ルールの実装
    // 【F#初学者向け解説】
    // パターンマッチングを使用して、すべてのロール変更パターンを網羅的にチェックします。
    // F#のコンパイラは、すべてのケースが処理されているかを確認するため、
    // ビジネスルールの漏れを防ぐことができます。
    //
    // タプルのパターンマッチング:
    // match operatorUser.Role, targetUser.Role, newRole with ...
    // このように複数の値を同時にパターンマッチングできます。
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
    // 【F#初学者向け解説】
    // List.collectは「リストの各要素を変換してフラット化」する操作です。
    // List.filterは「条件を満たす要素のみを抽出」します。
    // パイプライン演算子(|>)により、データの流れが左から右に明確に表現されています。
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
    // 【F#初学者向け解説】
    // この関数は複数のビジネスルールを順次チェックします。
    // elif を使用することで、最初にマッチしたエラー条件でエラーを返します。
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
    // 【F#初学者向け解説】
    // この関数は「自分のパスワード変更」と「他人のパスワード変更」で
    // 異なる権限チェックロジックを適用します。
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
    //
    // パイプライン演算子の読み方:
    // currentActiveUsers |> List.filter (...) |> List.length
    // 「currentActiveUsersを取り、フィルタリングし、長さを取得する」
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
    //
    // Option型のパターンマッチング:
    // Some target -> 値が存在する場合
    // None -> 値が存在しない場合（対象ユーザーなしの操作）
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