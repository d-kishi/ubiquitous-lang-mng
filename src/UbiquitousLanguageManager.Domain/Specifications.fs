namespace UbiquitousLanguageManager.Domain

// 🎯 仕様パターン (Specification Pattern): 複雑なビジネスルールを型安全に表現
// 複雑な条件分岐を再利用可能で組み合わせ可能な形で実装

// 🔧 仕様の基底型: すべての仕様が実装すべきインターフェース
type ISpecification<'T> =
    abstract member IsSatisfiedBy: 'T -> bool
    abstract member GetReasonForFailure: 'T -> string option

// 👤 ユーザー関連の仕様
module UserSpecifications =
    
    // 🔐 アクティブユーザー仕様: ユーザーがアクティブ状態か判定
    type ActiveUserSpec() =
        interface ISpecification<User> with
            member _.IsSatisfiedBy(user: User) = user.IsActive
            member _.GetReasonForFailure(user: User) = 
                if user.IsActive then None
                else Some "ユーザーが非アクティブ状態です"
    
    // 🎖️ 管理者権限仕様: ユーザーが管理者権限を持つか判定
    type AdminRoleSpec() =
        interface ISpecification<User> with
            member _.IsSatisfiedBy(user: User) = 
                match user.Role with
                | SuperUser | ProjectManager -> true
                | _ -> false
            member _.GetReasonForFailure(user: User) = 
                match user.Role with
                | SuperUser | ProjectManager -> None
                | _ -> Some "管理者権限が必要です"
    
    // 🔑 初回ログイン完了仕様: パスワード変更が完了しているか判定
    type FirstLoginCompletedSpec() =
        interface ISpecification<User> with
            member _.IsSatisfiedBy(user: User) = not user.IsFirstLogin
            member _.GetReasonForFailure(user: User) = 
                if user.IsFirstLogin then Some "初回ログイン時のパスワード変更が完了していません"
                else None

// 📝 ユビキタス言語関連の仕様
module UbiquitousLanguageSpecifications =
    
    // ✅ 承認可能状態仕様: ユビキタス言語が承認可能な状態か判定
    type ApprovableSpec() =
        interface ISpecification<DraftUbiquitousLanguage> with
            member _.IsSatisfiedBy(draft: DraftUbiquitousLanguage) = 
                draft.Status = Submitted
            member _.GetReasonForFailure(draft: DraftUbiquitousLanguage) = 
                if draft.Status = Submitted then None
                else Some $"申請中でない用語は承認できません（現在のステータス: {draft.Status}）"
    
    // 📝 編集可能状態仕様: ユビキタス言語が編集可能な状態か判定
    type EditableSpec() =
        interface ISpecification<DraftUbiquitousLanguage> with
            member _.IsSatisfiedBy(draft: DraftUbiquitousLanguage) = 
                draft.Status = Draft || draft.Status = Rejected
            member _.GetReasonForFailure(draft: DraftUbiquitousLanguage) = 
                match draft.Status with
                | Draft | Rejected -> None
                | _ -> Some $"申請中または承認済みの用語は編集できません（現在のステータス: {draft.Status}）"

// 🔧 仕様の組み合わせ演算子: 複数の仕様を論理演算で結合
module SpecificationCombiners =
    
    // 🤝 AND仕様: 両方の仕様を満たす必要がある
    type AndSpec<'T>(left: ISpecification<'T>, right: ISpecification<'T>) =
        interface ISpecification<'T> with
            member _.IsSatisfiedBy(item: 'T) = 
                left.IsSatisfiedBy(item) && right.IsSatisfiedBy(item)
            member _.GetReasonForFailure(item: 'T) = 
                match left.GetReasonForFailure(item), right.GetReasonForFailure(item) with
                | None, None -> None
                | Some reason, None | None, Some reason -> Some reason
                | Some leftReason, Some rightReason -> Some $"{leftReason}、{rightReason}"
    
    // 🔀 OR仕様: どちらかの仕様を満たせば良い
    type OrSpec<'T>(left: ISpecification<'T>, right: ISpecification<'T>) =
        interface ISpecification<'T> with
            member _.IsSatisfiedBy(item: 'T) = 
                left.IsSatisfiedBy(item) || right.IsSatisfiedBy(item)
            member _.GetReasonForFailure(item: 'T) = 
                if left.IsSatisfiedBy(item) || right.IsSatisfiedBy(item) then None
                else Some $"いずれの条件も満たしていません"
    
    // 🚫 NOT仕様: 仕様を満たさない場合に成功
    type NotSpec<'T>(spec: ISpecification<'T>) =
        interface ISpecification<'T> with
            member _.IsSatisfiedBy(item: 'T) = not (spec.IsSatisfiedBy(item))
            member _.GetReasonForFailure(item: 'T) = 
                if spec.IsSatisfiedBy(item) then Some "条件を満たすべきではありません"
                else None

// 🎯 仕様の利用例とヘルパー関数
module SpecificationHelpers =
    
    // 🔧 仕様を適用してResult型で結果を返すヘルパー
    let applySpecification<'T> (spec: ISpecification<'T>) (item: 'T) =
        if spec.IsSatisfiedBy(item) then
            Success item
        else
            match spec.GetReasonForFailure(item) with
            | Some reason -> Error reason
            | None -> Error "仕様を満たしていません"
    
    // 🤝 複数の仕様をすべて満たすかチェック
    let satisfiesAll<'T> (specs: ISpecification<'T> list) (item: 'T) =
        specs
        |> List.fold (fun acc spec ->
            match acc with
            | Error _ -> acc
            | Success _ -> applySpecification spec item
        ) (Success item)