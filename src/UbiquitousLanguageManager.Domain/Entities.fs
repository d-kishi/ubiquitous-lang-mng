namespace UbiquitousLanguageManager.Domain

open System

// 🎯 エンティティ: ドメイン駆動設計の核となるオブジェクト
// IDによって同一性が決まり、ライフサイクルを持つ

// 👤 ユーザーエンティティ: システム利用者の表現
// 【F#初学者向け解説】
// option型を使用することで、認証機能の段階的追加を実現しています。
// 既存データとの互換性を保ちながら、新しい認証属性を追加できます。
type User = {
    Id: UserId
    Email: Email
    Name: UserName
    Role: UserRole
    IsActive: bool
    IsFirstLogin: bool
    // 認証関連の拡張プロパティ（option型で後方互換性を確保）
    PasswordHash: PasswordHash option
    SecurityStamp: SecurityStamp option
    ConcurrencyStamp: ConcurrencyStamp option
    LockoutEnd: DateTime option  // アカウントロックアウト終了時刻
    AccessFailedCount: int       // ログイン失敗回数
    UpdatedAt: DateTime
    UpdatedBy: UserId
} with
    // 🔧 ユーザー作成: 新規ユーザーのファクトリーメソッド
    static member create (email: Email) (name: UserName) (role: UserRole) (createdBy: UserId) = {
        Id = UserId.create 0L  // 🔄 実際のIDはInfrastructure層で設定
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
        UpdatedAt = DateTime.UtcNow
        UpdatedBy = createdBy
    }
    
    // 🔐 認証用ユーザー作成: パスワードハッシュを含む完全な作成
    static member createWithAuthentication (email: Email) (name: UserName) (role: UserRole) 
                                         (passwordHash: PasswordHash) (createdBy: UserId) = {
        Id = UserId.create 0L
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
        UpdatedAt = DateTime.UtcNow
        UpdatedBy = createdBy
    }
    
    // 📧 メールアドレス変更: ビジネスルールを適用した更新
    member this.changeEmail newEmail updatedBy =
        if this.IsActive then
            Ok { this with 
                    Email = newEmail
                    SecurityStamp = Some (SecurityStamp.createNew()) // メール変更時はセキュリティスタンプも更新
                    UpdatedAt = DateTime.UtcNow
                    UpdatedBy = updatedBy }
        else
            Error "非アクティブなユーザーのメールアドレスは変更できません"
    
    // 🔑 パスワード変更: セキュリティルールに従った更新
    member this.changePassword (newPasswordHash: PasswordHash) updatedBy =
        if this.IsActive then
            Ok { this with 
                    PasswordHash = Some newPasswordHash
                    SecurityStamp = Some (SecurityStamp.createNew()) // パスワード変更時もセキュリティスタンプ更新
                    IsFirstLogin = false  // パスワード変更後は初回ログインフラグをオフ
                    UpdatedAt = DateTime.UtcNow
                    UpdatedBy = updatedBy }
        else
            Error "非アクティブなユーザーのパスワードは変更できません"
    
    // 🔒 ログイン失敗記録: アカウントロックアウト機能のサポート
    member this.recordFailedAccess maxFailedAttempts lockoutDuration =
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
    member this.recordSuccessfulAccess () =
        { this with 
            AccessFailedCount = 0
            LockoutEnd = None
            UpdatedAt = DateTime.UtcNow }
    
    // 🔓 ロックアウト状態確認
    member this.isLockedOut () =
        match this.LockoutEnd with
        | Some lockoutEnd -> DateTime.UtcNow < lockoutEnd
        | None -> false

// 📁 プロジェクトエンティティ: ドメイン領域の管理単位
type Project = {
    Id: ProjectId
    Name: JapaneseName
    Description: Description
    IsActive: bool
    UpdatedAt: DateTime
    UpdatedBy: UserId
} with
    // 🔧 プロジェクト作成
    static member create (name: JapaneseName) (description: Description) (createdBy: UserId) = {
        Id = ProjectId 0L
        Name = name
        Description = description
        IsActive = true
        UpdatedAt = DateTime.UtcNow
        UpdatedBy = createdBy
    }

// 🏷️ ドメインエンティティ: プロジェクト内の特定領域
type Domain = {
    Id: DomainId
    ProjectId: ProjectId
    Name: JapaneseName
    Description: Description
    IsActive: bool
    UpdatedAt: DateTime
    UpdatedBy: UserId
} with
    // 🔧 ドメイン作成
    static member create (projectId: ProjectId) (name: JapaneseName) (description: Description) (createdBy: UserId) = {
        Id = DomainId 0L
        ProjectId = projectId
        Name = name
        Description = description
        IsActive = true
        UpdatedAt = DateTime.UtcNow
        UpdatedBy = createdBy
    }

// 📝 下書きユビキタス言語エンティティ: 承認前の用語定義
type DraftUbiquitousLanguage = {
    Id: UbiquitousLanguageId
    DomainId: DomainId
    JapaneseName: JapaneseName
    EnglishName: EnglishName
    Description: Description
    Status: ApprovalStatus
    UpdatedAt: DateTime
    UpdatedBy: UserId
} with
    // 🔧 下書き作成
    static member create (domainId: DomainId) (japaneseName: JapaneseName) 
                         (englishName: EnglishName) (description: Description) (createdBy: UserId) = {
        Id = UbiquitousLanguageId 0L
        DomainId = domainId
        JapaneseName = japaneseName
        EnglishName = englishName
        Description = description
        Status = Draft
        UpdatedAt = DateTime.UtcNow
        UpdatedBy = createdBy
    }
    
    // 📤 承認申請: ステータス変更のビジネスロジック
    member this.submitForApproval submittedBy =
        match this.Status with
        | Draft -> 
            Ok { this with 
                    Status = Submitted
                    UpdatedAt = DateTime.UtcNow
                    UpdatedBy = submittedBy }
        | _ -> Error "下書き状態でない用語は承認申請できません"
    
    // ✅ 承認処理
    member this.approve approvedBy =
        match this.Status with
        | Submitted -> 
            Ok { this with 
                    Status = Approved
                    UpdatedAt = DateTime.UtcNow
                    UpdatedBy = approvedBy }
        | _ -> Error "申請中でない用語は承認できません"

// ✅ 正式ユビキタス言語エンティティ: 承認済みの確定用語
type FormalUbiquitousLanguage = {
    Id: UbiquitousLanguageId
    DomainId: DomainId
    JapaneseName: JapaneseName
    EnglishName: EnglishName
    Description: Description
    ApprovedAt: DateTime
    ApprovedBy: UserId
    UpdatedAt: DateTime
    UpdatedBy: UserId
} with
    // 🔧 下書きから正式版への変換
    static member createFromDraft (draft: DraftUbiquitousLanguage) (approvedBy: UserId) =
        if draft.Status = Approved then
            Ok {
                Id = draft.Id
                DomainId = draft.DomainId
                JapaneseName = draft.JapaneseName
                EnglishName = draft.EnglishName
                Description = draft.Description
                ApprovedAt = DateTime.UtcNow
                ApprovedBy = approvedBy
                UpdatedAt = DateTime.UtcNow
                UpdatedBy = approvedBy
            }
        else
            Error "承認済みでない下書きから正式版は作成できません"