namespace UbiquitousLanguageManager.Domain.UbiquitousLanguageManagement

open UbiquitousLanguageManager.Domain.Common

// 🎯 UbiquitousLanguageDomainService.fs: ユビキタス言語管理ドメインサービス
// Bounded Context: UbiquitousLanguageManagement
// このファイルはユビキタス言語管理に関するドメインサービスを定義します
//
// 【F#初学者向け解説】
// ドメインサービスは、単一のエンティティや値オブジェクトに属さない
// ビジネスロジックを実装する場所です。複数のエンティティにまたがる
// ビジネスルールや、ドメイン全体に関わる知識をここに集約します。

// 📚 Phase B1完了後: ユビキタス言語管理ドメインサービス
// 【F#初学者向け解説】
// 複数のエンティティにまたがるビジネスロジックや、
// ドメイン全体に関わる複雑なビジネスルールを実装します。
//
// 責務:
// - ユーザー権限検証（ドメイン単位）
// - 用語名重複チェック（ドメイン単位）
// - 承認ワークフロー権限検証
// - ステータス遷移妥当性検証
module UbiquitousLanguageDomainService =

    // 🔐 ユーザー権限検証: ユーザーが特定のドメインでユビキタス言語を作成可能か判定
    // 【F#初学者向け解説】
    // Result型を使用することで、エラーハンドリングをコンパイル時に強制します。
    // 成功時はOk()、失敗時はUbiquitousLanguageError型を返し、権限チェック漏れを防ぎます。
    //
    // ※ Phase C/D実装時にProjectManagement境界文脈のDomain型を参照
    let validateUserCanCreateInDomain (userId: UserId) (domainIsActive: bool) : Result<unit, UbiquitousLanguageError> =
        // 🎭 ビジネスルール: アクティブなドメインでのみ作成可能
        if not domainIsActive then
            Error (InactiveDomainError "非アクティブなドメインではユビキタス言語を作成できません")
        else
            Ok () // 👍 基本的な検証は通過（詳細な権限チェックはApplication層で実施）

    // 🔍 重複チェック: 同一ドメイン内での用語名重複を防止
    // 【F#初学者向け解説】
    // List.exists関数を使用して、既存用語の中に同名のものがないかチェックします。
    // パターンマッチングにより、すべてのエラーケースを明示的に処理します。
    // UbiquitousLanguageError型により、エラーの種類を明確に区別できます。
    let validateUniqueNamesInDomain (japaneseName: JapaneseName) (englishName: EnglishName)
                                   (existingTerms: FormalUbiquitousLanguage list) : Result<unit, UbiquitousLanguageError> =
        // 🎯 日本語名の重複チェック
        let japaneseNameExists =
            existingTerms
            |> List.exists (fun term -> term.JapaneseName.Value = japaneseName.Value)

        // 🎯 英語名の重複チェック
        let englishNameExists =
            existingTerms
            |> List.exists (fun term -> term.EnglishName.Value = englishName.Value)

        // 🚫 重複エラーの判定
        // 【F#初学者向け解説】
        // タプルパターンマッチングにより、複数の条件を組み合わせた処理を簡潔に記述できます。
        // UbiquitousLanguageError型の具体的なケースを返すことで、エラー内容を明確にします。
        match japaneseNameExists, englishNameExists with
        | true, true -> Error (DuplicateBothNames (japaneseName.Value, englishName.Value))
        | true, false -> Error (DuplicateJapaneseName japaneseName.Value)
        | false, true -> Error (DuplicateEnglishName englishName.Value)
        | false, false -> Ok () // ✅ 重複なし

    // 🔄 承認ワークフロー検証: 承認者の権限チェック
    // 【F#初学者向け解説】
    // パターンマッチングを使用して、ロールごとの承認権限を明確に定義します。
    // F#のコンパイラがすべてのケースをチェックするため、権限設定漏れを防ぎます。
    // UbiquitousLanguageError型により、権限エラーの種類を型安全に表現します。
    //
    // ※ Phase C/D実装時にAuthentication境界文脈のRole型を参照
    let validateApprovalAuthorization (approverId: UserId) (approverRole: Role) : Result<unit, UbiquitousLanguageError> =
        match approverRole with
        | SuperUser -> Ok () // 🎖️ スーパーユーザーは全ドメイン承認可能
        | ProjectManager -> Ok () // 👨‍💼 プロジェクト管理者も承認可能
        | DomainApprover -> Ok () // ✅ ドメイン承認者は担当ドメインの承認可能
        | GeneralUser -> Error (UnauthorizedApproval "一般ユーザーは承認権限がありません") // ❌ 一般ユーザーは承認不可

    // 📊 ステータス遷移検証: 正しいワークフローでのステータス変更を保証
    // 【F#初学者向け解説】
    // 承認ワークフローのステータス遷移ルールを実装します。
    // 不正なステータス遷移を防ぐことで、データの整合性を保ちます。
    // UbiquitousLanguageError型により、不正な遷移の詳細情報を提供します。
    let validateStatusTransition (currentStatus: ApprovalStatus) (targetStatus: ApprovalStatus) : Result<unit, UbiquitousLanguageError> =
        match currentStatus, targetStatus with
        | Draft, Submitted -> Ok () // 下書き → 申請
        | Submitted, Approved -> Ok () // 申請 → 承認
        | Submitted, Rejected -> Ok () // 申請 → 却下
        | Rejected, Draft -> Ok () // 却下 → 下書き（修正のため）
        | _ -> Error (InvalidStatusTransition ($"{currentStatus}", $"{targetStatus}"))