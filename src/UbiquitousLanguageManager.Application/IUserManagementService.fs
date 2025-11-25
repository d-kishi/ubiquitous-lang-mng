namespace UbiquitousLanguageManager.Application

open System.Threading.Tasks
// F# Domain層namespace階層化対応: Bounded Context別にopen
open UbiquitousLanguageManager.Domain.Common         // UserId, Role
open UbiquitousLanguageManager.Domain.Authentication // User, Email

// 🎯 Phase B-F3 Step1 Stage2: ユーザー管理サービスインターフェース
// Clean Architecture Application層のインターフェース定義
// 【F#初学者向け解説】
// IUserManagementServiceは、ユーザー管理の全ユースケースを抽象化し、
// Infrastructure層への依存を排除した純粋なApplication層契約を提供します。
// このInterfaceを導入することで、bUnitテストでMoqによるMock化が可能になります。

/// <summary>
/// ユーザー管理サービスインターフェース
/// 【F#初学者向け解説】
/// F#のInterfaceは、C#と同様にabstract memberで定義します。
/// すべてのメソッドはTask&lt;Result&lt;T, string&gt;&gt;を返し、
/// 非同期処理と型安全なエラーハンドリングを両立します。
/// エラー型はstringとし、IProjectManagementServiceと統一しています。
/// </summary>
type IUserManagementService =

    /// <summary>
    /// ユーザー一覧取得（検索条件・権限フィルタ対応）
    /// 【F#初学者向け解説】
    /// - SuperUser: 全ユーザー取得可能
    /// - ProjectManager: 自分が管理するプロジェクトのユーザーのみ
    /// - その他のロール: 権限エラー（PermissionDenied）
    /// タプル形式（引数をカンマ区切り）で定義することで、C#インターフェースと互換性を持ちます。
    /// Result&lt;T, string&gt;型により、エラーメッセージを文字列で返します。
    /// </summary>
    /// <param name="query">検索条件（obj型、現在未使用）</param>
    /// <param name="operatorIdentityId">操作者のASP.NET Core Identity ID（文字列）</param>
    /// <returns>ユーザー一覧またはエラーメッセージ（Railway-oriented Programming）</returns>
    abstract member GetAllUsersAsync:
        query: obj * operatorIdentityId: string ->
        Task<Result<User list, string>>

    /// <summary>
    /// ユーザー詳細取得（権限制御統合）
    /// 【F#初学者向け解説】
    /// SuperUserまたは該当ユーザーの管理者のみ取得可能です。
    /// Result&lt;T, string&gt;型により、ユーザー未存在エラーと権限不足エラーを
    /// エラーメッセージとして明示的に返します。
    /// </summary>
    /// <param name="userId">取得対象ユーザーID</param>
    /// <param name="operatorIdentityId">操作者のASP.NET Core Identity ID（文字列）</param>
    /// <returns>ユーザー詳細情報またはエラーメッセージ</returns>
    abstract member GetUserByIdAsync:
        userId: UserId * operatorIdentityId: string ->
        Task<Result<User, string>>

    /// <summary>
    /// ユーザー作成（手動入力パスワード対応）
    /// 【F#初学者向け解説】
    /// 以下の処理を順次実行します（Railway-oriented Programming）：
    /// 1. 操作者権限確認（CreateUsers権限必須）
    /// 2. 入力値バリデーション（Smart Constructor Pattern）
    /// 3. メールアドレス重複チェック
    /// 4. パスワードハッシュ化・ユーザー作成
    /// 5. プロジェクト割り当て（Phase B-F3 Step2で実装予定）
    /// </summary>
    /// <param name="email">メールアドレス（バリデーション前の文字列）</param>
    /// <param name="name">ユーザー名（バリデーション前の文字列）</param>
    /// <param name="password">パスワード（バリデーション前の文字列）</param>
    /// <param name="role">ロール文字列（"SuperUser", "ProjectManager", "DomainApprover", "GeneralUser"）</param>
    /// <param name="assignedProjectIds">割り当てプロジェクトID一覧（現在未使用・Phase B-F3 Step2で実装）</param>
    /// <param name="operatorIdentityId">操作者のASP.NET Core Identity ID（文字列）</param>
    /// <returns>作成されたユーザーまたはエラーメッセージ</returns>
    abstract member CreateUserAsync:
        email: string * name: string * password: string * role: string * assignedProjectIds: int64 list * operatorIdentityId: string ->
        Task<Result<User, string>>

    /// <summary>
    /// ユーザー更新（名前・ロール・プロジェクト割り当て・アクティブ状態変更）
    /// 【F#初学者向け解説】
    /// 既存ユーザーの情報を更新します。以下の項目が変更可能です：
    /// - 名前（UserName型バリデーション適用）
    /// - ロール（権限チェック統合：ProjectManagerはSuperUser/ProjectManagerに変更不可）
    /// - プロジェクト割り当て（Phase B-F3 Step2で実装予定）
    /// - アクティブ状態（IsActive）
    /// </summary>
    /// <param name="userId">更新対象ユーザーID</param>
    /// <param name="name">新しいユーザー名</param>
    /// <param name="role">新しいロール文字列</param>
    /// <param name="assignedProjectIds">新しいプロジェクト割り当て（現在未使用）</param>
    /// <param name="isActive">アクティブ状態</param>
    /// <param name="operatorIdentityId">操作者のASP.NET Core Identity ID（文字列）</param>
    /// <returns>更新されたユーザーまたはエラーメッセージ</returns>
    abstract member UpdateUserAsync:
        userId: UserId * name: string * role: string * assignedProjectIds: int64 list * isActive: bool * operatorIdentityId: string ->
        Task<Result<User, string>>

    /// <summary>
    /// ユーザー無効化（論理削除）
    /// 【F#初学者向け解説】
    /// ユーザーを論理削除（IsActive = false）します。物理削除は行いません。
    /// DeleteUsers権限が必要です。
    /// Task&lt;Result&lt;unit, string&gt;&gt;の返却値は、
    /// 成功時に意味のある値を返さないことを示します（unit型 = C#のvoid相当）。
    /// </summary>
    /// <param name="userId">無効化対象ユーザーID</param>
    /// <param name="operatorIdentityId">操作者のASP.NET Core Identity ID（文字列）</param>
    /// <returns>成功時はunit、失敗時はエラーメッセージ</returns>
    abstract member DeactivateUserAsync:
        userId: UserId * operatorIdentityId: string ->
        Task<Result<unit, string>>

    /// <summary>
    /// ユーザー有効化（論理削除の取り消し）
    /// 【F#初学者向け解説】
    /// 無効化されたユーザーを再有効化（IsActive = true）します。
    /// DeleteUsers権限が必要です（有効化も削除権限で管理）。
    /// </summary>
    /// <param name="userId">有効化対象ユーザーID</param>
    /// <param name="operatorIdentityId">操作者のASP.NET Core Identity ID（文字列）</param>
    /// <returns>成功時はunit、失敗時はエラーメッセージ</returns>
    abstract member ActivateUserAsync:
        userId: UserId * operatorIdentityId: string ->
        Task<Result<unit, string>>
