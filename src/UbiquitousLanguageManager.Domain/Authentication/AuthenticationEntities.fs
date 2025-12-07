namespace UbiquitousLanguageManager.Domain.Authentication

open System
open UbiquitousLanguageManager.Domain.Common

// 🎯 AuthenticationEntities.fs: 認証関連エンティティ定義
// Bounded Context: Authentication
// このファイルは認証・ユーザー管理に関するエンティティを定義します
//
// 【F#初学者向け解説】
// Entity（エンティティ）は、IDによって同一性が決まり、ライフサイクルを持つオブジェクトです。
// Aggregate Root（集約ルート）は、関連するエンティティ群の一貫性を保つ責任を持ちます。

// 👤 ユーザーエンティティ: システム利用者の表現（Aggregate Root）
// 【F#初学者向け解説】
// option型を使用することで、認証機能の段階的追加を実現しています。
// 既存データとの互換性を保ちながら、新しい認証属性を追加できます。
// Phase A2拡張: 権限システム・プロフィール管理・プロジェクトスコープ権限対応
//
// レコード型の不変性:
// F#のレコード型は不変（イミュータブル）です。値を変更する際は、
// { user with Email = newEmail } という構文で新しいインスタンスを作成します。
// これにより、データの整合性を保ちながら安全に状態変更ができます。
type User = {
    Id: UserId
    Email: Email
    Name: UserName
    Role: Role                    // 新しい権限システムのRole型に変更
    IsActive: bool
    IsFirstLogin: bool
    // 認証関連の拡張プロパティ（option型で後方互換性を確保）
    PasswordHash: PasswordHash option
    SecurityStamp: SecurityStamp option
    ConcurrencyStamp: ConcurrencyStamp option
    LockoutEnd: DateTime option  // アカウントロックアウト終了時刻
    AccessFailedCount: int       // ログイン失敗回数
    // Phase A2 新規追加: ユーザー管理機能拡張
    Profile: UserProfile         // ユーザープロフィール情報
    ProjectPermissions: ProjectPermission list  // プロジェクトスコープ権限
    EmailConfirmed: bool         // メールアドレス確認フラグ
    PhoneNumber: string option   // 電話番号（ASP.NET Core Identity連携）
    PhoneNumberConfirmed: bool   // 電話番号確認フラグ
    TwoFactorEnabled: bool       // 二要素認証有効フラグ
    LockoutEnabled: bool         // ロックアウト機能有効フラグ
    // 監査・管理情報
    CreatedAt: DateTime          // 作成日時
    CreatedBy: UserId            // 作成者
    UpdatedAt: DateTime
    UpdatedBy: UserId
} with
    // 🔧 ユーザー作成: 新規ユーザーのファクトリーメソッド（Phase A2拡張版）
    // 【F#初学者向け解説】
    // static member create は C# の static メソッドに相当します。
    // ファクトリーメソッドパターンにより、適切に初期化されたUserを作成します。
    static member create (email: Email) (name: UserName) (role: Role) (createdBy: UserId) : User = {
        Id = UserId.create ""  // 🔄 実際のIDはInfrastructure層で設定（空文字列は仮のID）
        Email = email
        Name = name
        Role = role
        IsActive = true
        IsFirstLogin = true
        PasswordHash = None
        SecurityStamp = Some (SecurityStamp.createNew())
        ConcurrencyStamp = Some (ConcurrencyStamp.createNew())
        LockoutEnd = None
        AccessFailedCount = 0
        // Phase A2 新規項目の初期値
        Profile = UserProfile.empty
        ProjectPermissions = []
        EmailConfirmed = false
        PhoneNumber = None
        PhoneNumberConfirmed = false
        TwoFactorEnabled = false
        LockoutEnabled = true
        // 監査情報
        CreatedAt = DateTime.UtcNow
        CreatedBy = createdBy
        UpdatedAt = DateTime.UtcNow
        UpdatedBy = createdBy
    }

    /// 🔧 データベースからの復元用ファクトリーメソッド
    /// 【F#初学者向け解説】
    /// データベースから読み込んだユーザー情報をF# Userレコードに変換する際に使用します。
    /// IsActive値を明示的に受け取ることで、データベースのIsDeletedフラグを正しく反映できます。
    /// 通常のcreateはIsActive=trueをデフォルトにしますが、このメソッドはDB状態を忠実に復元します。
    static member createFromDatabase
        (email: Email)
        (name: UserName)
        (role: Role)
        (createdBy: UserId)
        (isActive: bool) : User =
        { User.create email name role createdBy with IsActive = isActive }

    // 🔧 ID付きユーザー作成: テスト用・完全指定のファクトリーメソッド（Phase A2拡張版）
    // 【F#初学者向け解説】
    // TypeConvertersのテストで使用するため、IDを明示的に指定できるメソッドを追加
    // C#からF#の境界での型変換テストでIDの整合性を確保するために必要
    // F#のメソッドオーバーロード制限により異なる名前を使用
    static member createWithId (email: Email) (name: UserName) (role: Role) (id: UserId) : User = {
        Id = id  // 🔄 テスト用にIDを明示的に設定
        Email = email
        Name = name
        Role = role
        IsActive = true
        IsFirstLogin = true
        PasswordHash = None
        SecurityStamp = Some (SecurityStamp.createNew())
        ConcurrencyStamp = Some (ConcurrencyStamp.createNew())
        LockoutEnd = None
        AccessFailedCount = 0
        // Phase A2 新規項目の初期値
        Profile = UserProfile.empty
        ProjectPermissions = []
        EmailConfirmed = false
        PhoneNumber = None
        PhoneNumberConfirmed = false
        TwoFactorEnabled = false
        LockoutEnabled = true
        // 監査情報
        CreatedAt = DateTime.UtcNow
        CreatedBy = id  // 作成者は自分自身
        UpdatedAt = DateTime.UtcNow
        UpdatedBy = id
    }

    // 🔐 認証用ユーザー作成: パスワードハッシュを含む完全な作成（Phase A2拡張版）
    static member createWithAuthentication (email: Email) (name: UserName) (role: Role)
                                         (passwordHash: PasswordHash) (createdBy: UserId) : User = {
        Id = UserId.create ""  // 🔄 実際のIDはInfrastructure層で設定（空文字列は仮のID）
        Email = email
        Name = name
        Role = role
        IsActive = true
        IsFirstLogin = true
        PasswordHash = Some passwordHash
        SecurityStamp = Some (SecurityStamp.createNew())
        ConcurrencyStamp = Some (ConcurrencyStamp.createNew())
        LockoutEnd = None
        AccessFailedCount = 0
        // Phase A2 新規項目の初期値
        Profile = UserProfile.empty
        ProjectPermissions = []
        EmailConfirmed = false
        PhoneNumber = None
        PhoneNumberConfirmed = false
        TwoFactorEnabled = false
        LockoutEnabled = true
        // 監査情報
        CreatedAt = DateTime.UtcNow
        CreatedBy = createdBy
        UpdatedAt = DateTime.UtcNow
        UpdatedBy = createdBy
    }

    // 📧 メールアドレス変更: ビジネスルールを適用した更新（Phase A2拡張）
    // 【F#初学者向け解説】
    // Result型を返すことで、エラーハンドリングを型安全に行います。
    // Ok(newUser)は成功、Error(errorMessage)は失敗を表します。
    // レコード更新構文 { this with ... } により、一部のフィールドのみを変更した
    // 新しいUserインスタンスを作成します（不変性を保つため）。
    member this.changeEmail (newEmail: Email) (updatedBy: UserId) : Result<User, string> =
        if this.IsActive then
            Ok { this with
                    Email = newEmail
                    EmailConfirmed = false  // メール変更時は確認をリセット
                    SecurityStamp = Some (SecurityStamp.createNew()) // メール変更時はセキュリティスタンプも更新
                    UpdatedAt = DateTime.UtcNow
                    UpdatedBy = updatedBy }
        else
            Error "非アクティブなユーザーのメールアドレスは変更できません"

    // 🔑 パスワード変更: セキュリティルールに従った更新（Phase A2拡張）
    member this.changePassword (newPasswordHash: PasswordHash) (updatedBy: UserId) : Result<User, string> =
        if this.IsActive then
            Ok { this with
                    PasswordHash = Some newPasswordHash
                    SecurityStamp = Some (SecurityStamp.createNew()) // パスワード変更時もセキュリティスタンプ更新
                    IsFirstLogin = false  // パスワード変更後は初回ログインフラグをオフ
                    AccessFailedCount = 0  // パスワード変更時は失敗カウントリセット
                    LockoutEnd = None      // ロックアウト状態もリセット
                    UpdatedAt = DateTime.UtcNow
                    UpdatedBy = updatedBy }
        else
            Error "非アクティブなユーザーのパスワードは変更できません"

    // 🔐 Phase A9: パスワードリセット機能（管理者・リセットトークン用）
    // 【F#初学者向け解説】
    // パスワードリセット時は通常のパスワード変更とは異なり、現在のパスワード確認が不要です。
    // セキュリティスタンプ更新により、既存のセッションを無効化し、セキュリティを確保します。
    member this.resetPassword (newPasswordHash: PasswordHash) (updatedBy: UserId) : Result<User, string> =
        if this.IsActive then
            Ok { this with
                    PasswordHash = Some newPasswordHash
                    SecurityStamp = Some (SecurityStamp.createNew()) // 全セッション無効化
                    IsFirstLogin = false  // リセット後は初回ログインフラグをオフ
                    AccessFailedCount = 0  // 失敗カウントリセット
                    LockoutEnd = None      // ロックアウト状態リセット
                    UpdatedAt = DateTime.UtcNow
                    UpdatedBy = updatedBy }
        else
            Error "非アクティブなユーザーのパスワードはリセットできません"

    // 👤 プロフィール更新: ユーザープロフィール情報の更新
    // 【F#初学者向け解説】
    // レコード型の不変性により、updateProfileは新しいUserインスタンスを返します。
    // これにより、データの整合性を保ちながら安全にプロフィール更新ができます。
    member this.updateProfile (newProfile: UserProfile) (updatedBy: UserId) : Result<User, string> =
        if this.IsActive then
            Ok { this with
                    Profile = newProfile
                    UpdatedAt = DateTime.UtcNow
                    UpdatedBy = updatedBy }
        else
            Error "非アクティブなユーザーのプロフィールは変更できません"

    // 🎭 ロール変更: 権限管理のビジネスルール適用
    // 【F#初学者向け解説】
    // ロール変更には厳格なビジネスルールを適用します。
    // SuperUserのロール変更やロール降格の制限など、セキュリティ重要な操作です。
    member this.changeRole (newRole: Role) (operatorUser: User) (updatedBy: UserId) : Result<User, string> =
        // 操作者の権限チェック
        if not (PermissionMappings.hasPermission operatorUser.Role ManageUserRoles) then
            Error "ユーザーロール変更の権限がありません"
        elif not this.IsActive then
            Error "非アクティブなユーザーのロールは変更できません"
        elif this.Role = SuperUser && operatorUser.Role <> SuperUser then
            Error "SuperUserのロール変更はSuperUserのみが実行できます"
        elif newRole = SuperUser && operatorUser.Role <> SuperUser then
            Error "SuperUserへの昇格はSuperUserのみが実行できます"
        else
            Ok { this with
                    Role = newRole
                    SecurityStamp = Some (SecurityStamp.createNew()) // ロール変更時はセキュリティスタンプ更新
                    UpdatedAt = DateTime.UtcNow
                    UpdatedBy = updatedBy }

    // 📱 電話番号設定: ASP.NET Core Identity連携
    member this.setPhoneNumber (phoneNumber: string option) (phoneNumberConfirmed: bool) (updatedBy: UserId) : Result<User, string> =
        if this.IsActive then
            Ok { this with
                    PhoneNumber = phoneNumber
                    PhoneNumberConfirmed = phoneNumberConfirmed
                    UpdatedAt = DateTime.UtcNow
                    UpdatedBy = updatedBy }
        else
            Error "非アクティブなユーザーの電話番号は変更できません"

    // 🔐 二要素認証設定: セキュリティ機能管理
    member this.setTwoFactorEnabled (enabled: bool) (updatedBy: UserId) : Result<User, string> =
        if this.IsActive then
            Ok { this with
                    TwoFactorEnabled = enabled
                    SecurityStamp = Some (SecurityStamp.createNew()) // セキュリティ設定変更時はスタンプ更新
                    UpdatedAt = DateTime.UtcNow
                    UpdatedBy = updatedBy }
        else
            Error "非アクティブなユーザーの二要素認証設定は変更できません"

    // 🏢 プロジェクト権限管理: プロジェクトスコープ権限の設定
    // 【F#初学者向け解説】
    // リスト操作で既存のプロジェクト権限を更新または追加します。
    // List.filter で既存権限を除外し、新しい権限を追加することで重複を防ぎます。
    member this.setProjectPermissions (projectPermissions: ProjectPermission list) (operatorUser: User) (updatedBy: UserId) : Result<User, string> =
        if not (PermissionMappings.hasPermission operatorUser.Role ManageUserRoles) then
            Error "プロジェクト権限設定の権限がありません"
        elif not this.IsActive then
            Error "非アクティブなユーザーのプロジェクト権限は変更できません"
        else
            Ok { this with
                    ProjectPermissions = projectPermissions
                    UpdatedAt = DateTime.UtcNow
                    UpdatedBy = updatedBy }

    // 🏢 単一プロジェクト権限追加
    // 【F#初学者向け解説】
    // :: 演算子はリストの先頭に要素を追加します（cons演算子）。
    // これはF#の基本的なリスト操作で、非常に効率的です。
    member this.addProjectPermission (projectPermission: ProjectPermission) (operatorUser: User) (updatedBy: UserId) : Result<User, string> =
        if not (PermissionMappings.hasPermission operatorUser.Role ManageUserRoles) then
            Error "プロジェクト権限追加の権限がありません"
        elif not this.IsActive then
            Error "非アクティブなユーザーのプロジェクト権限は変更できません"
        else
            // 既存の同一プロジェクト権限を除外して新しい権限を追加
            let updatedPermissions =
                this.ProjectPermissions
                |> List.filter (fun p -> p.ProjectId <> projectPermission.ProjectId)
                |> fun perms -> projectPermission :: perms
            Ok { this with
                    ProjectPermissions = updatedPermissions
                    UpdatedAt = DateTime.UtcNow
                    UpdatedBy = updatedBy }

    // 🔒 ログイン失敗記録: アカウントロックアウト機能のサポート
    // 【F#初学者向け解説】
    // この関数はResult型ではなく、直接Userを返します（失敗しない操作のため）。
    // ログイン失敗回数が上限に達すると自動的にロックアウトします。
    member this.recordFailedAccess (maxFailedAttempts: int) (lockoutDuration: TimeSpan) : User =
        let newFailedCount = this.AccessFailedCount + 1
        if newFailedCount >= maxFailedAttempts then
            // ロックアウト発動
            { this with
                AccessFailedCount = newFailedCount
                LockoutEnd = Some (DateTime.UtcNow.Add(lockoutDuration))
                UpdatedAt = DateTime.UtcNow }
        else
            // 失敗回数のみ増加
            { this with
                AccessFailedCount = newFailedCount
                UpdatedAt = DateTime.UtcNow }

    // ✅ ログイン成功記録: 失敗カウントのリセット
    member this.recordSuccessfulAccess () : User =
        { this with
            AccessFailedCount = 0
            LockoutEnd = None
            UpdatedAt = DateTime.UtcNow }

    // 🔓 ロックアウト状態確認
    // 【F#初学者向け解説】
    // match式でOption型を処理します。
    // Some lockoutEnd -> 値が存在する場合
    // None -> 値が存在しない場合
    member this.isLockedOut () : bool =
        match this.LockoutEnd with
        | Some lockoutEnd -> DateTime.UtcNow < lockoutEnd
        | None -> false

    // 🚫 ユーザー無効化: 論理削除によるアカウント無効化
    // 【F#初学者向け解説】
    // ユーザーアカウントの削除は物理削除ではなく論理削除を行います。
    // これにより、作成したデータとの関連を保ちながら、ログイン不可にできます。
    member this.deactivate (operatorUser: User) (updatedBy: UserId) : Result<User, string> =
        if not (PermissionMappings.hasPermission operatorUser.Role DeleteUsers) then
            Error "ユーザー無効化の権限がありません"
        elif this.Role = SuperUser && operatorUser.Role <> SuperUser then
            Error "SuperUserの無効化はSuperUserのみが実行できます"
        elif not this.IsActive then
            Error "既に無効化されているユーザーです"
        else
            Ok { this with
                    IsActive = false
                    SecurityStamp = Some (SecurityStamp.createNew()) // 無効化時はセキュリティスタンプ更新
                    UpdatedAt = DateTime.UtcNow
                    UpdatedBy = updatedBy }

    // ✅ ユーザー有効化: 無効化されたアカウントの再有効化
    member this.activate (operatorUser: User) (updatedBy: UserId) : Result<User, string> =
        if not (PermissionMappings.hasPermission operatorUser.Role ManageUserRoles) then
            Error "ユーザー有効化の権限がありません"
        elif this.IsActive then
            Error "既に有効化されているユーザーです"
        else
            Ok { this with
                    IsActive = true
                    AccessFailedCount = 0      // 有効化時は失敗カウントリセット
                    LockoutEnd = None          // ロックアウト状態もリセット
                    SecurityStamp = Some (SecurityStamp.createNew())
                    UpdatedAt = DateTime.UtcNow
                    UpdatedBy = updatedBy }

    // 🔓 Phase A9: アカウントロック解除機能
    // 【F#初学者向け解説】
    // 管理者によるアカウントロック解除機能です。権限チェックは上位レイヤーで実行し、
    // ドメインレイヤーではビジネスルールに専念します。
    member this.unlockAccount () : User =
        { this with
            AccessFailedCount = 0
            LockoutEnd = None
            UpdatedAt = DateTime.UtcNow
            UpdatedBy = this.Id }  // 自動解除の場合、自分自身を更新者とする

    // 🔍 権限チェックヘルパーメソッド: 特定権限の保有確認
    member this.hasPermission (permission: Permission) : bool =
        PermissionMappings.hasPermission this.Role permission

    // 🔄 Phase A9: Application層互換性プロパティ
    // 【F#初学者向け解説】
    // Application層では`FailedAccessAttempts`として参照されるため、
    // 互換性のためのcomputed propertyを提供します。
    member this.FailedAccessAttempts = this.AccessFailedCount

    // 🔍 プロジェクトスコープ権限チェック: プロジェクト内での権限確認
    // 【F#初学者向け解説】
    // List.existsは「リストの中に条件を満たす要素が存在するか」をチェックします。
    // fun p -> ... はラムダ式（無名関数）です。
    member this.hasProjectPermission (projectId: ProjectId) (permission: Permission) : bool =
        // グローバル権限チェック
        if this.hasPermission permission then true
        else
            // プロジェクトスコープ権限チェック
            this.ProjectPermissions
            |> List.exists (fun p -> p.ProjectId = projectId && p.hasPermission permission)

    // 📋 アクセス可能プロジェクト一覧取得
    // 【F#初学者向け解説】
    // List.mapは「リストの各要素を変換」する操作です。
    // ここではProjectPermissionからProjectIdのリストに変換しています。
    member this.getAccessibleProjectIds () : ProjectId list =
        this.ProjectPermissions |> List.map (fun p -> p.ProjectId)

    // 🆔 ASP.NET Core Identity連携用ID取得: string形式での ID 取得
    // 【F#初学者向け解説】
    // ASP.NET Core Identityではユーザー ID を string として扱います。
    // UserId型はすでにstring型を内包しているため、.Valueで直接取得できます。
    member this.getIdentityId () : string =
        this.Id.Value

    // 🔧 システム管理者作成: UseCase層での仮の管理者ユーザー作成
    // 【F#初学者向け解説】
    // UseCase層でユーザー作成・パスワード変更等の操作を行う際、
    // 操作者（operatorUser）が必要ですが、実際のログイン済みユーザーが取得できない場合の
    // 仮のシステム管理者を作成するメソッドです。実装時にはセッション情報から取得するように変更予定。
    static member createSystemAdmin () =
        // F#のResult型を適切に処理して値を取得
        let systemEmail =
            match Email.create("system@admin.local") with
            | Ok email -> email
            | Error _ -> failwith "システム管理者メール作成に失敗"

        let systemName =
            match UserName.create("System Administrator") with
            | Ok name -> name
            | Error _ -> failwith "システム管理者名作成に失敗"

        {
            Id = UserId.create "system"  // システム管理者用の固定ID（GUID文字列ではなく識別子）
            Email = systemEmail
            Name = systemName
            Role = SuperUser
            IsActive = true
            IsFirstLogin = false
            PasswordHash = None
            SecurityStamp = Some (SecurityStamp.createNew())
            ConcurrencyStamp = Some (ConcurrencyStamp.createNew())
            LockoutEnd = None
            AccessFailedCount = 0
            Profile = UserProfile.empty
            ProjectPermissions = []
            EmailConfirmed = true
            PhoneNumber = None
            PhoneNumberConfirmed = false
            TwoFactorEnabled = false
            LockoutEnabled = false  // システム管理者はロックアウト無効
            CreatedAt = DateTime.UtcNow
            CreatedBy = UserId.create "system"  // 自己参照
            UpdatedAt = DateTime.UtcNow
            UpdatedBy = UserId.create "system"
        }