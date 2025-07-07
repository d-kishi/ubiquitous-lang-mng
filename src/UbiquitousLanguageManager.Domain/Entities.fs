namespace UbiquitousLanguageManager.Domain

open System

// 🎯 エンティティ: ドメイン駆動設計の核となるオブジェクト
// IDによって同一性が決まり、ライフサイクルを持つ

// 👤 ユーザーエンティティ: システム利用者の表現
type User = {
    Id: UserId
    Email: Email
    Name: UserName
    Role: UserRole
    IsActive: bool
    IsFirstLogin: bool
    UpdatedAt: DateTime
    UpdatedBy: UserId
} with
    // 🔧 ユーザー作成: 新規ユーザーのファクトリーメソッド
    static member create (email: Email) (name: UserName) (role: UserRole) (createdBy: UserId) = {
        Id = UserId 0L  // 🔄 実際のIDはInfrastructure層で設定
        Email = email
        Name = name
        Role = role
        IsActive = true
        IsFirstLogin = true
        UpdatedAt = DateTime.UtcNow
        UpdatedBy = createdBy
    }
    
    // 📧 メールアドレス変更: ビジネスルールを適用した更新
    member this.changeEmail newEmail updatedBy =
        if this.IsActive then
            Ok { this with 
                    Email = newEmail
                    UpdatedAt = DateTime.UtcNow
                    UpdatedBy = updatedBy }
        else
            Error "非アクティブなユーザーのメールアドレスは変更できません"

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