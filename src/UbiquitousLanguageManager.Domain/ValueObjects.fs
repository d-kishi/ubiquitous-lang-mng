namespace UbiquitousLanguageManager.Domain

open System

// 🎯 Value Objects: ドメイン駆動設計の基本構成要素
// 値によって同一性が決まるオブジェクト（IDを持たない）

// 📧 メールアドレス値オブジェクト
type Email = 
    private Email of string
with
    // 🔧 静的ファクトリーメソッド: 検証を伴う安全な生成
    static member create (emailStr: string) =
        if System.String.IsNullOrWhiteSpace(emailStr) then
            Error "メールアドレスが入力されていません"
        elif emailStr.Length > 254 then
            Error "メールアドレスは254文字以内で入力してください"
        elif not (emailStr.Contains("@")) then
            Error "有効なメールアドレス形式ではありません"
        elif emailStr.StartsWith("@") || emailStr.EndsWith("@") then
            Error "有効なメールアドレス形式ではありません"
        elif emailStr.IndexOf("@") <> emailStr.LastIndexOf("@") then
            Error "有効なメールアドレス形式ではありません"
        elif not (emailStr.Contains(".")) then
            Error "有効なメールアドレス形式ではありません"
        else
            let atIndex = emailStr.IndexOf("@")
            let localPart = emailStr.Substring(0, atIndex)
            let domainPart = emailStr.Substring(atIndex + 1)
            if System.String.IsNullOrWhiteSpace(localPart) || System.String.IsNullOrWhiteSpace(domainPart) then
                Error "有効なメールアドレス形式ではありません"
            elif domainPart.IndexOf(".") = -1 then
                Error "有効なメールアドレス形式ではありません"
            else
                Ok (Email emailStr)
    
    // 📤 値の取得: プライベートコンストラクタのため専用メソッドで値を取得
    member this.Value = 
        let (Email email) = this
        email

// 👤 ユーザー名値オブジェクト
type UserName = 
    private UserName of string
with
    static member create (nameStr: string) =
        if System.String.IsNullOrWhiteSpace(nameStr) then
            Error "ユーザー名が入力されていません"
        elif nameStr.Length > 50 then
            Error "ユーザー名は50文字以内で入力してください"
        else
            Ok (UserName nameStr)
    
    member this.Value = 
        let (UserName name) = this
        name

// 🌍 日本語名値オブジェクト
type JapaneseName = 
    private JapaneseName of string
with
    static member create (nameStr: string) =
        if System.String.IsNullOrWhiteSpace(nameStr) then
            Error "日本語名が入力されていません"
        elif nameStr.Length > 100 then
            Error "日本語名は100文字以内で入力してください"
        else
            Ok (JapaneseName nameStr)
    
    member this.Value = 
        let (JapaneseName name) = this
        name

// 🔤 英語名値オブジェクト
type EnglishName = 
    private EnglishName of string
with
    static member create (nameStr: string) =
        if System.String.IsNullOrWhiteSpace(nameStr) then
            Error "英語名が入力されていません"
        elif nameStr.Length > 100 then
            Error "英語名は100文字以内で入力してください"
        else
            Ok (EnglishName nameStr)
    
    member this.Value = 
        let (EnglishName name) = this
        name

// 📝 説明文値オブジェクト
type Description = 
    private Description of string
with
    static member create (descStr: string) =
        if System.String.IsNullOrWhiteSpace(descStr) then
            Error "説明が入力されていません"
        elif descStr.Length > 1000 then
            Error "説明は1000文字以内で入力してください"
        else
            Ok (Description descStr)
    
    member this.Value = 
        let (Description desc) = this
        desc

// 🎭 ユーザーロール: 権限管理のための列挙型
type UserRole =
    | SuperUser      // システム管理者
    | ProjectManager // プロジェクト管理者
    | DomainApprover // ドメイン承認者
    | GeneralUser    // 一般ユーザー

// 🔄 承認状態: ワークフロー管理のための列挙型
type ApprovalStatus =
    | Draft     // 下書き
    | Submitted // 承認申請中
    | Approved  // 承認済み
    | Rejected  // 却下

// 🔐 パスワードハッシュ値オブジェクト
// 【F#初学者向け解説】
// パスワードは平文で保存せず、必ずハッシュ化して保存します。
// このValue Objectは、ハッシュ化されたパスワードのみを受け入れ、
// ドメイン層でのパスワード処理の安全性を保証します。
type PasswordHash = 
    private PasswordHash of string
with
    // パスワードハッシュは外部（Infrastructure層）で生成されたものを受け取る
    static member create (hashStr: string) =
        if System.String.IsNullOrWhiteSpace(hashStr) then
            Error "パスワードハッシュが入力されていません"
        else
            Ok (PasswordHash hashStr)
    
    member this.Value = 
        let (PasswordHash hash) = this
        hash

// 🔒 セキュリティスタンプ値オブジェクト
// ASP.NET Core Identityで使用される、ユーザーの認証状態変更を追跡する値
type SecurityStamp = 
    private SecurityStamp of string
with
    static member create (stampStr: string) =
        if System.String.IsNullOrWhiteSpace(stampStr) then
            // 新規作成時は自動生成
            Ok (SecurityStamp (System.Guid.NewGuid().ToString("N")))
        else
            Ok (SecurityStamp stampStr)
    
    static member createNew () =
        SecurityStamp (System.Guid.NewGuid().ToString("N"))
    
    member this.Value = 
        let (SecurityStamp stamp) = this
        stamp

// 🔄 並行性スタンプ値オブジェクト
// Entity Frameworkの楽観的並行性制御で使用
type ConcurrencyStamp = 
    private ConcurrencyStamp of string
with
    static member create (stampStr: string) =
        if System.String.IsNullOrWhiteSpace(stampStr) then
            Ok (ConcurrencyStamp (System.Guid.NewGuid().ToString()))
        else
            Ok (ConcurrencyStamp stampStr)
    
    static member createNew () =
        ConcurrencyStamp (System.Guid.NewGuid().ToString())
    
    member this.Value = 
        let (ConcurrencyStamp stamp) = this
        stamp

// 🆔 識別子型: 型安全なIDの実装
type UserId = 
    | UserId of int64
with
    member this.Value = 
        let (UserId id) = this
        id
    static member create(id: int64) = UserId id

type ProjectId = 
    | ProjectId of int64
with
    member this.Value = 
        let (ProjectId id) = this
        id
    static member create(id: int64) = ProjectId id

type DomainId = 
    | DomainId of int64
with
    member this.Value = 
        let (DomainId id) = this
        id
    static member create(id: int64) = DomainId id

type UbiquitousLanguageId = 
    | UbiquitousLanguageId of int64
with
    member this.Value = 
        let (UbiquitousLanguageId id) = this
        id
    static member create(id: int64) = UbiquitousLanguageId id

// 🔐 パスワード値オブジェクト（平文パスワード用）
// 【F#初学者向け解説】
// これは平文パスワードを一時的に保持するためのValue Objectです。
// セキュリティの観点から、パスワードは即座にハッシュ化され、
// この値オブジェクトはInfrastructure層でのハッシュ化処理のみで使用されます。
type Password = 
    private Password of string
with
    // パスワード強度バリデーションを含む安全な生成
    static member create (passwordStr: string) =
        if System.String.IsNullOrWhiteSpace(passwordStr) then
            Error "パスワードが入力されていません"
        elif passwordStr.Length < 8 then
            Error "パスワードは8文字以上で入力してください"
        elif passwordStr.Length > 100 then
            Error "パスワードは100文字以内で入力してください"
        elif not (System.Text.RegularExpressions.Regex.IsMatch(passwordStr, "[A-Z]")) then
            Error "パスワードには大文字を含めてください"
        elif not (System.Text.RegularExpressions.Regex.IsMatch(passwordStr, "[a-z]")) then
            Error "パスワードには小文字を含めてください"
        elif not (System.Text.RegularExpressions.Regex.IsMatch(passwordStr, "[0-9]")) then
            Error "パスワードには数字を含めてください"
        else
            Ok (Password passwordStr)
    
    member this.Value = 
        let (Password pwd) = this
        pwd

// 📧 強化版Email値オブジェクト（より厳密なバリデーション）
// 【F#初学者向け解説】
// 既存のEmailを拡張し、より厳密なメールアドレス検証を行います。
// System.Net.Mail.MailAddressを使用してRFC準拠の検証を実行します。
type StrongEmail = 
    private StrongEmail of string
with
    static member create (emailStr: string) =
        if System.String.IsNullOrWhiteSpace(emailStr) then
            Error "メールアドレスが入力されていません"
        elif emailStr.Length > 254 then
            Error "メールアドレスは254文字以内で入力してください"
        else
            try
                // RFC準拠のメールアドレス検証
                let mailAddress = System.Net.Mail.MailAddress(emailStr)
                if mailAddress.Address = emailStr then
                    Ok (StrongEmail emailStr)
                else
                    Error "有効なメールアドレス形式ではありません"
            with
            | :? System.FormatException -> Error "有効なメールアドレス形式ではありません"
            | :? System.ArgumentException -> Error "有効なメールアドレス形式ではありません"
    
    member this.Value = 
        let (StrongEmail email) = this
        email

// 👤 ユーザープロフィール値オブジェクト
// 【F#初学者向け解説】
// ユーザーの詳細プロフィール情報を表現するValue Objectです。
// レコード型として不変な構造で定義し、プロフィール更新時は
// 新しいインスタンスを作成することで不変性を保ちます。
type UserProfile = {
    DisplayName: string option        // 表示名（任意）
    Department: string option         // 所属部署（任意）
    PhoneNumber: string option        // 電話番号（任意）
    Notes: string option              // 備考（任意）
} with
    static member create displayName department phoneNumber notes = {
        DisplayName = 
            if System.String.IsNullOrWhiteSpace(displayName) then None 
            else Some (displayName.Trim())
        Department = 
            if System.String.IsNullOrWhiteSpace(department) then None 
            else Some (department.Trim())
        PhoneNumber = 
            if System.String.IsNullOrWhiteSpace(phoneNumber) then None 
            else Some (phoneNumber.Trim())
        Notes = 
            if System.String.IsNullOrWhiteSpace(notes) then None 
            else Some (notes.Trim())
    }
    
    static member empty = {
        DisplayName = None
        Department = None
        PhoneNumber = None
        Notes = None
    }

// 🎭 権限システム：階層的権限とプロジェクトスコープ権限
// 【F#初学者向け解説】
// Discriminated Unionを使用して権限を型安全に表現します。
// これにより、コンパイル時に権限チェックの漏れを防ぎ、
// パターンマッチングで全ての権限ケースを確実に処理できます。

// 個別権限定義（最小権限単位）
type Permission =
    | ViewUsers                       // ユーザー閲覧
    | CreateUsers                     // ユーザー作成
    | EditUsers                       // ユーザー編集
    | DeleteUsers                     // ユーザー削除（無効化）
    | ManageUserRoles                 // ユーザーロール管理
    | ViewProjects                    // プロジェクト閲覧
    | CreateProjects                  // プロジェクト作成
    | ManageProjects                  // プロジェクト管理
    | DeleteProjects                  // プロジェクト削除
    | ViewDomains                     // ドメイン閲覧
    | ManageDomains                   // ドメイン管理
    | ApproveDomains                  // ドメイン承認
    | ViewUbiquitousLanguages         // ユビキタス言語閲覧
    | CreateUbiquitousLanguages       // ユビキタス言語作成
    | EditUbiquitousLanguages         // ユビキタス言語編集
    | ApproveUbiquitousLanguages      // ユビキタス言語承認
    | ManageSystemSettings            // システム設定管理

// ロール定義（権限の集合）
type Role =
    | SuperUser                       // システム管理者（全権限）
    | ProjectManager                  // プロジェクト管理者
    | DomainApprover                  // ドメイン承認者
    | GeneralUser                     // 一般ユーザー

// 階層的権限マッピング関数
// 【F#初学者向け解説】
// Set<Permission>を使用することで、権限の重複を自動的に排除し、
// 効率的な権限チェックを実現します。Set.unionで権限を合成し、
// 階層的な権限継承を表現しています。
module PermissionMappings =
    let rec getPermissionsForRole (role: Role) : Set<Permission> =
        match role with
        | GeneralUser -> 
            set [
                ViewProjects; ViewDomains; ViewUbiquitousLanguages
                CreateUbiquitousLanguages; EditUbiquitousLanguages
            ]
        | DomainApprover -> 
            let basePermissions = getPermissionsForRole GeneralUser
            Set.union basePermissions (set [
                ApproveDomains; ApproveUbiquitousLanguages
            ])
        | ProjectManager ->
            let basePermissions = getPermissionsForRole DomainApprover
            Set.union basePermissions (set [
                ViewUsers; CreateUsers; EditUsers; ManageUserRoles
                CreateProjects; ManageProjects; ManageDomains
            ])
        | SuperUser ->
            let basePermissions = getPermissionsForRole ProjectManager
            Set.union basePermissions (set [
                DeleteUsers; DeleteProjects; ManageSystemSettings
            ])
    
    // 特定権限の保有確認
    let hasPermission (role: Role) (permission: Permission) : bool =
        let permissions = getPermissionsForRole role
        Set.contains permission permissions
    
    // 複数権限の一括確認
    let hasAllPermissions (role: Role) (requiredPermissions: Permission list) : bool =
        let userPermissions = getPermissionsForRole role
        requiredPermissions |> List.forall (fun p -> Set.contains p userPermissions)

// プロジェクトスコープ権限
// 【F#初学者向け解説】
// ユーザーが特定のプロジェクトに対して持つ権限を表現します。
// プロジェクトIDと権限の組み合わせで、細かな権限制御を実現します。
type ProjectPermission = {
    ProjectId: ProjectId
    Permissions: Set<Permission>
} with
    static member create projectId permissions = {
        ProjectId = projectId
        Permissions = Set.ofList permissions
    }
    
    member this.hasPermission permission =
        Set.contains permission this.Permissions

// 🔐 Phase A9: 認証エラー判別共用体
// Railway-oriented Programming対応・F#↔C#境界TypeConverter拡張
// 【F#初学者向け解説】
// 判別共用体により、認証エラーの種類を型安全に表現します。
// パターンマッチングによる網羅的なエラーハンドリングが可能になります。
type AuthenticationError =
    | InvalidCredentials                           // 認証情報が正しくない
    | UserNotFound of Email                        // ユーザーが見つからない
    | ValidationError of string                    // バリデーションエラー（型変換エラー等）
    | AccountLocked of Email * DateTime            // アカウントロックアウト
    | SystemError of exn                           // システムエラー（例外情報付き）
    | PasswordExpired of Email                     // パスワード期限切れ
    | TwoFactorRequired of Email                   // 二要素認証が必要
    // 🔐 Phase A9: パスワードリセット関連エラー（4種類）
    | PasswordResetTokenExpired of Email           // パスワードリセットトークン期限切れ
    | PasswordResetTokenInvalid of Email           // 無効なパスワードリセットトークン
    | PasswordResetNotRequested of Email           // パスワードリセット未要求
    | PasswordResetAlreadyUsed of Email            // パスワードリセットトークン使用済み
    // 🔒 Phase A9: トークン関連エラー（4種類）
    | TokenGenerationFailed of string              // トークン生成失敗
    | TokenValidationFailed of string              // トークン検証失敗
    | TokenExpired of string                       // トークン期限切れ
    | TokenRevoked of string                       // トークン無効化
    // 👮 Phase A9: 管理者操作関連エラー（3種類）
    | InsufficientPermissions of string            // 権限不足（ロール・権限情報）
    | OperationNotAllowed of string                // 操作不許可
    | ConcurrentOperationDetected of string        // 並行操作検出
    // 🔮 Phase A9: 将来拡張用エラー（4種類）
    | TwoFactorAuthFailed of Email                 // 二要素認証失敗
    | ExternalAuthenticationFailed of string       // 外部認証失敗
    | AuditLogError of string                      // 監査ログエラー
    | AccountDeactivated                           // アカウント無効化