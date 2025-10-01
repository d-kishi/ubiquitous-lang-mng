namespace UbiquitousLanguageManager.Domain.UbiquitousLanguageManagement

open System
open UbiquitousLanguageManager.Domain.Common

// 🎯 UbiquitousLanguageEntities.fs: ユビキタス言語管理エンティティ
// Bounded Context: UbiquitousLanguageManagement
// このファイルはユビキタス言語管理に関するエンティティを定義します
//
// 【F#初学者向け解説】
// エンティティはIDによって同一性が決まり、ライフサイクルを持つドメインオブジェクトです。
// 値オブジェクトと異なり、状態が変化する可能性があります（ただしF#では不変性を維持）。

// 📝 下書きユビキタス言語エンティティ: 承認前の用語定義
// 【F#初学者向け解説】
// ユビキタス言語の下書きを表現するエンティティです。
// Result型を使用することで、ビジネスルールの違反をコンパイル時に検出できます。
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
    // 【F#初学者向け解説】
    // ファクトリーメソッドパターンにより、適切に初期化された下書きを作成します。
    // Draftステータスから開始し、承認ワークフローに沿って進行します。
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
    // 【F#初学者向け解説】
    // パターンマッチングを使用して、ステータス遷移のビジネスルールを実装します。
    // Result型により、不正なステータス遷移を防ぎます。
    // UbiquitousLanguageError型を使用することで、エラーの種類を明確にします。
    member this.submitForApproval submittedBy =
        match this.Status with
        | Draft ->
            Ok { this with
                    Status = Submitted
                    UpdatedAt = DateTime.UtcNow
                    UpdatedBy = submittedBy }
        | _ -> Error (InvalidStatusTransition ("下書き以外", "申請中"))

    // ✅ 承認処理
    // 【F#初学者向け解説】
    // 申請中ステータスからのみ承認可能というビジネスルールを実装します。
    // UbiquitousLanguageError型により、エラー情報を明確に伝達します。
    member this.approve approvedBy =
        match this.Status with
        | Submitted ->
            Ok { this with
                    Status = Approved
                    UpdatedAt = DateTime.UtcNow
                    UpdatedBy = approvedBy }
        | _ -> Error (InvalidStatusTransition ("申請中以外", "承認済み"))

    // ❌ 却下処理
    // 【F#初学者向け解説】
    // 申請中ステータスからのみ却下可能というビジネスルールを実装します。
    member this.reject rejectedBy =
        match this.Status with
        | Submitted ->
            Ok { this with
                    Status = Rejected
                    UpdatedAt = DateTime.UtcNow
                    UpdatedBy = rejectedBy }
        | _ -> Error (InvalidStatusTransition ("申請中以外", "却下"))

// ✅ 正式ユビキタス言語エンティティ: 承認済みの確定用語
// 【F#初学者向け解説】
// 承認されたユビキタス言語の正式版を表現するエンティティです。
// 下書きから正式版への変換により、データの整合性を保ちます。
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
    // 【F#初学者向け解説】
    // 承認済みの下書きからのみ正式版を作成できるビジネスルールを実装します。
    // Result型により、承認前の下書きから正式版を作成することを防ぎます。
    // UbiquitousLanguageError型を使用して、エラー内容を明確に伝達します。
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
            Error (InvalidApprovalStatus "承認済みでない下書きから正式版は作成できません")