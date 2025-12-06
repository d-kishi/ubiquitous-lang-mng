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
    /// ユーザー一覧取得（検索条件・権限フィルタ・削除済みユーザー表示対応）
    /// 【F#初学者向け解説】
    /// - SuperUser: 全ユーザー取得可能
    /// - ProjectManager: 自分が管理するプロジェクトのユーザーのみ
    /// - その他のロール: 権限エラー（PermissionDenied）
    /// includeDeletedパラメータにより、論理削除ユーザーの表示を制御します。
    /// タプル形式（引数をカンマ区切り）で定義することで、C#インターフェースと互換性を持ちます。
    /// Result&lt;T, string&gt;型により、エラーメッセージを文字列で返します。
    /// </summary>
    /// <param name="query">検索条件（obj型、現在未使用）</param>
    /// <param name="operatorIdentityId">操作者のASP.NET Core Identity ID（文字列）</param>
    /// <param name="includeDeleted">trueの場合、論理削除ユーザーも含めて取得</param>
    /// <returns>ユーザー一覧またはエラーメッセージ（Railway-oriented Programming）</returns>
    abstract member GetAllUsersAsync:
        query: obj * operatorIdentityId: string * includeDeleted: bool ->
        Task<Result<User list, string>>

    /// <summary>
    /// ユーザー一覧取得（IdentityId付き）
    /// 【F#初学者向け解説】
    /// Web層でASP.NET Core Identity IDを使用するため、
    /// タプル(User * string)のリストを返します。
    /// - User: F# Domainエンティティ
    /// - string: ASP.NET Core Identity ID（GUID文字列）
    /// これにより、Web層からの削除・更新操作で安定したID変換が可能になります。
    /// </summary>
    /// <param name="query">検索条件（obj型、現在未使用）</param>
    /// <param name="operatorIdentityId">操作者のASP.NET Core Identity ID（文字列）</param>
    /// <param name="includeDeleted">trueの場合、論理削除ユーザーも含めて取得</param>
    /// <returns>ユーザー・IdentityIdペアのリストまたはエラーメッセージ</returns>
    abstract member GetAllUsersWithIdentityAsync:
        query: obj * operatorIdentityId: string * includeDeleted: bool ->
        Task<Result<(User * string) list, string>>

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
    ///
    /// Phase B-F3 Step1.5 Stage5: UserId → IdentityId変更
    /// GetHashCode()使用のUserId変換が不安定だったため、
    /// ASP.NET Core Identity IDを直接使用する方式に変更しました。
    /// </summary>
    /// <param name="targetIdentityId">更新対象ユーザーのASP.NET Core Identity ID（GUID文字列）</param>
    /// <param name="name">新しいユーザー名</param>
    /// <param name="role">新しいロール文字列</param>
    /// <param name="assignedProjectIds">新しいプロジェクト割り当て（現在未使用）</param>
    /// <param name="isActive">アクティブ状態</param>
    /// <param name="operatorIdentityId">操作者のASP.NET Core Identity ID（文字列）</param>
    /// <returns>更新されたユーザーまたはエラーメッセージ</returns>
    abstract member UpdateUserAsync:
        targetIdentityId: string * name: string * role: string * assignedProjectIds: int64 list * isActive: bool * operatorIdentityId: string ->
        Task<Result<User, string>>

    /// <summary>
    /// ユーザー削除（論理削除）
    /// 【F#初学者向け解説】
    /// ユーザーを論理削除（IsActive = false）します。物理削除は行いません。
    /// DeleteUsers権限が必要です。
    /// Task&lt;Result&lt;unit, string&gt;&gt;の返却値は、
    /// 成功時に意味のある値を返さないことを示します（unit型 = C#のvoid相当）。
    ///
    /// Phase B-F3 Step1.5 Stage5: UserId → IdentityId変更
    /// GetHashCode()使用のUserId変換が不安定だったため、
    /// ASP.NET Core Identity IDを直接使用する方式に変更しました。
    /// </summary>
    /// <param name="targetIdentityId">削除対象ユーザーのASP.NET Core Identity ID（GUID文字列）</param>
    /// <param name="operatorIdentityId">操作者のASP.NET Core Identity ID（文字列）</param>
    /// <returns>成功時はunit、失敗時はエラーメッセージ</returns>
    abstract member DeleteUserAsync:
        targetIdentityId: string * operatorIdentityId: string ->
        Task<Result<unit, string>>

    /// <summary>
    /// パスワードリセット（管理者による強制変更）
    /// 【F#初学者向け解説】
    /// SuperUserが他ユーザーのパスワードを強制的に変更するためのメソッドです。
    /// 通常のパスワード変更（ChangePasswordAsync）と異なり、
    /// 現在のパスワードを知らなくても新しいパスワードを設定できます。
    ///
    /// 以下の処理を順次実行します（Railway-oriented Programming）：
    /// 1. 操作者権限確認（SuperUserのみ実行可能）
    /// 2. 対象ユーザー確認（UserId → Identity ID取得）
    /// 3. パスワードバリデーション（Smart Constructor Pattern）
    /// 4. Infrastructure層AuthenticationService.AdminResetPasswordAsync呼び出し
    /// 5. セキュリティログ記録（監査証跡）
    ///
    /// 【セキュリティ考慮事項】
    /// - SuperUser権限必須（権限チェックは本メソッド内で実施）
    /// - パスワードリセット実行ログを必ず記録（監査証跡）
    /// - 将来的には対象ユーザーへの通知メール送信を推奨
    /// </summary>
    /// <param name="targetUserId">対象ユーザーID（F# Domain UserId型）</param>
    /// <param name="newPassword">新しいパスワード（バリデーション前の文字列）</param>
    /// <param name="operatorIdentityId">操作者のASP.NET Core Identity ID（文字列）</param>
    /// <returns>成功時はunit、失敗時はエラーメッセージ</returns>
    abstract member ResetPasswordAsync:
        targetUserId: UserId * newPassword: string * operatorIdentityId: string ->
        Task<Result<unit, string>>

    /// <summary>
    /// ユーザーが所属するプロジェクトID一覧取得
    /// 【F#初学者向け解説】
    /// Phase B-F3 Step1.5 Stage4: Edit.razorでプロジェクト割り当てチェックボックスの初期状態を復元するために使用します。
    /// IUserRepositoryのGetProjectIdsByUserIdAsyncを薄くラップし、Clean Architectureを維持します。
    ///
    /// 【使用場面】
    /// - ユーザー編集画面でのプロジェクト割り当て状態復元（チェックボックス初期化）
    ///
    /// 【権限制御】
    /// - このメソッド自体には権限チェックを含みません（GetUserByIdAsyncで権限確認済み前提）
    /// </summary>
    /// <param name="userId">対象ユーザーID</param>
    /// <returns>プロジェクトID一覧（long list）またはエラーメッセージ</returns>
    abstract member GetProjectIdsByUserIdAsync:
        userId: UserId ->
        Task<Result<int64 list, string>>

    /// <summary>
    /// ユーザーのEmailから所属プロジェクトID一覧を取得
    /// 【F#初学者向け解説】
    /// Phase B-F3 Step1.5 Stage4修正: Index.razorで所属プロジェクトを表示するために使用します。
    /// GetProjectIdsByUserIdAsyncの合成GUID問題を回避するため、Emailベースで検索します。
    /// </summary>
    /// <param name="email">対象ユーザーのメールアドレス</param>
    /// <returns>プロジェクトID一覧（int64 list）またはエラーメッセージ</returns>
    abstract member GetProjectIdsByEmailAsync:
        email: string ->
        Task<Result<int64 list, string>>
