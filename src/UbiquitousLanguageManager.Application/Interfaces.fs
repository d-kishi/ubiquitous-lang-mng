namespace UbiquitousLanguageManager.Application

// F# Domain層namespace階層化対応: Bounded Context別にopen
open UbiquitousLanguageManager.Domain.Common                  // UserId, ProjectId, DomainId, Role, Permission
open UbiquitousLanguageManager.Domain.Authentication          // User, Email, UserName, Password, PasswordHash
open UbiquitousLanguageManager.Domain.ProjectManagement       // Project, Domain
open UbiquitousLanguageManager.Domain.UbiquitousLanguageManagement  // DraftUbiquitousLanguage, FormalUbiquitousLanguage, UbiquitousLanguageId
open System.Threading.Tasks

// 🎯 Application層インターフェース: Clean Architectureの境界定義
// Infrastructure層への依存関係を抽象化し、依存関係逆転の原則を実現

// 👤 Phase A2: ユーザーリポジトリインターフェース（大幅拡張版）
// 【F#初学者向け解説】
// インターフェース定義により、Infrastructure層の具体的実装に依存しない
// 抽象的なデータアクセス契約を定義します。これにより、テスト時のモック作成や
// 将来的なデータベース変更への対応が容易になります。
type IUserRepository =
    // 🔍 ユーザー検索: メールアドレスによる一意検索
    abstract member GetByEmailAsync: email: Email -> Task<Result<User option, string>>
    
    // 🔍 ユーザー検索: IDによる一意検索
    abstract member GetByIdAsync: userId: UserId -> Task<Result<User option, string>>

    // 🔍 ユーザー検索: ASP.NET Identity ID（文字列）による一意検索
    abstract member GetByIdentityIdAsync: identityId: string -> Task<Result<User option, string>>

    // 💾 ユーザー保存: 新規作成・更新の両方に対応
    abstract member SaveAsync: user: User -> Task<Result<User, string>>
    
    // 📋 アクティブユーザー一覧: 有効なユーザーのみ取得
    abstract member GetAllActiveUsersAsync: unit -> Task<Result<User list, string>>
    
    // 📋 全ユーザー一覧: 無効化されたユーザーも含めて取得（削除済み含む制御）
    // 【F#初学者向け解説】
    // includeDeleted: trueの場合は論理削除（IsActive=false）ユーザーも含めて取得します。
    // falseの場合はアクティブなユーザーのみを返します。
    abstract member GetAllUsersAsync: includeDeleted: bool -> Task<Result<User list, string>>
    
    // 📋 ユーザー一覧: プロジェクト単位でのユーザー取得
    abstract member GetByProjectIdAsync: projectId: ProjectId -> Task<Result<User list, string>>

    // 📋 ユーザー一覧: 複数プロジェクトに所属するユーザー取得
    // 【F#初学者向け解説】
    // ProjectManager権限フィルタ用のメソッドです。
    // 操作者が管理するプロジェクトのIDリストを渡すと、
    // それらのプロジェクトに所属するユーザーの一覧を返します。
    // 重複排除された結果を返すため、同じユーザーが複数プロジェクトに所属していても1回のみ含まれます。
    //
    // 【パラメータ】
    // - projectIds: ProjectId list - 検索対象のプロジェクトIDリスト（F#のlist型）
    //
    // 【戻り値】
    // - Task<Result<User list, string>>
    //   - Ok (User list): 取得成功時、重複排除されたユーザーリスト
    //   - Error string: 取得失敗時のエラーメッセージ
    //
    // 【使用例】
    // let projectIds = [ProjectId.create "proj-1"; ProjectId.create "proj-2"]
    // let! result = userRepository.GetUsersByProjectIdsAsync(projectIds)
    // match result with
    // | Ok users -> // ユーザーリスト処理
    // | Error msg -> // エラー処理
    abstract member GetUsersByProjectIdsAsync: projectIds: ProjectId list -> Task<Result<User list, string>>

    // 📋 ユーザーが所属するプロジェクトID一覧取得
    // 【F#初学者向け解説】
    // ProjectManager権限フィルタ用のメソッドです。
    // 指定ユーザーが所属（管理）するプロジェクトのID一覧を取得します。
    // UserProjectsテーブルを参照して、ユーザーに割り当てられたプロジェクトを返します。
    //
    // 【パラメータ】
    // - userId: UserId - 検索対象のユーザーID
    //
    // 【戻り値】
    // - Task<Result<ProjectId list, string>>
    //   - Ok (ProjectId list): 取得成功時、プロジェクトIDリスト（F#のlist型）
    //   - Error string: 取得失敗時のエラーメッセージ
    //
    // 【使用例】
    // let! result = userRepository.GetProjectIdsByUserIdAsync(currentUserId)
    // match result with
    // | Ok projectIds -> // projectIds: ProjectId list を使用
    // | Error msg -> // エラー処理
    abstract member GetProjectIdsByUserIdAsync: userId: UserId -> Task<Result<ProjectId list, string>>

    // 📋 Email指定プロジェクトID一覧: EmailからIdentityIdを取得し、プロジェクト一覧取得
    // 【F#初学者向け解説】
    // GetProjectIdsByUserIdAsyncの合成GUID問題を回避するため、Emailベースで検索します。
    // 内部的にEmailでApplicationUserを検索し、そのIdentity IDでUserProjectsを検索します。
    abstract member GetProjectIdsByEmailAsync: email: Email -> Task<Result<ProjectId list, string>>

    // 📋 Identity IDベースのプロジェクトID一覧取得（PM権限フィルタ用）
    // 【F#初学者向け解説】
    // Phase B-F3 Step1.5: GetProjectIdsByUserIdAsyncの合成GUID問題を回避するため、
    // ASP.NET Core Identity ID（文字列）を直接使用してUserProjectsテーブルを検索します。
    // PMがユーザー一覧を取得する際、このメソッドで操作者のプロジェクト範囲を正確に特定できます。
    //
    // 【パラメータ】
    // - identityId: ASP.NET Core Identity ID（GUID形式の文字列）
    //
    // 【戻り値】
    // - Ok ProjectId list: 操作者が所属するプロジェクトIDのリスト
    // - Error string: 取得失敗時のエラーメッセージ
    abstract member GetProjectIdsByIdentityIdAsync:
        identityId: string -> Task<Result<ProjectId list, string>>

    // 📋 IdentityId付きユーザー一覧取得
    // 【F#初学者向け解説】
    // Phase B-F3 Step1.5 Stage5: Web層でIdentityIdを使用するため、
    // タプル(User * string)のリストを返します。
    // これにより、GetHashCode()使用による不安定なID変換を回避し、
    // Web層からの削除・更新操作で安定したID管理が可能になります。
    //
    // 【パラメータ】
    // - includeDeleted: bool - trueの場合は論理削除ユーザーも含めて取得
    //
    // 【戻り値】
    // - Task<Result<(User * string) list, string>>
    //   - Ok (User * string) list: 取得成功時、ユーザー・IdentityIdペアのリスト
    //     - User: F# Domainエンティティ
    //     - string: ASP.NET Core Identity ID（GUID文字列）
    //   - Error string: 取得失敗時のエラーメッセージ
    //
    // 【使用例】
    // let! result = userRepository.GetAllUsersWithIdentityAsync(includeDeleted = true)
    // match result with
    // | Ok userPairs ->
    //     userPairs |> List.iter (fun (user, identityId) ->
    //         printfn "User: %s, IdentityId: %s" user.Name.Value identityId)
    // | Error msg -> // エラー処理
    abstract member GetAllUsersWithIdentityAsync: includeDeleted: bool -> Task<Result<(User * string) list, string>>

    // 🗑️ IdentityIdによる削除: ASP.NET Core Identity IDでユーザーを論理削除
    // 【F#初学者向け解説】
    // Phase B-F3 Step1.5 Stage5: GetHashCode()変換を回避し、
    // Web層から渡されたIdentityIdを直接使用して削除します。
    //
    // 【パラメータ】
    // - identityId: string - 削除対象ユーザーのASP.NET Core Identity ID（GUID文字列）
    //
    // 【戻り値】
    // - Task<Result<unit, string>>
    //   - Ok unit: 削除成功
    //   - Error string: 削除失敗時のエラーメッセージ
    //
    // 【処理内容】
    // 1. IdentityIdでApplicationUserを検索
    // 2. 対応するF# Domain UserエンティティのIsActiveをfalseに設定
    // 3. 論理削除完了
    abstract member DeleteByIdentityIdAsync: identityId: string -> Task<Result<unit, string>>

    // ✏️ IdentityIdによる更新: ASP.NET Core Identity IDでユーザー情報を更新
    // 【F#初学者向け解説】
    // Phase B-F3 Step1.5 Stage5: GetHashCode()変換を回避し、
    // Web層から渡されたIdentityIdを直接使用して更新します。
    //
    // 【パラメータ】
    // - identityId: string - 更新対象ユーザーのASP.NET Core Identity ID（GUID文字列）
    // - name: string - 新しいユーザー名（バリデーション前の文字列）
    // - role: Role - 新しいロール（F# Domain Role型）
    // - isActive: bool - 新しいアクティブ状態
    //
    // 【戻り値】
    // - Task<Result<User, string>>
    //   - Ok User: 更新成功時、更新されたF# Domain Userエンティティ
    //   - Error string: 更新失敗時のエラーメッセージ
    //
    // 【処理内容】
    // 1. IdentityIdでApplicationUserを検索
    // 2. 対応するF# Domain Userエンティティを取得
    // 3. 名前・ロール・IsActiveを更新
    // 4. 更新されたUserエンティティを返す
    //
    // 【注意】
    // プロジェクト割り当ての更新は、既存のUpdateUserProjectsAsyncを使用します。
    abstract member UpdateByIdentityIdAsync: identityId: string * name: string * role: Role * isActive: bool -> Task<Result<User, string>>

    // 📋 ロール別ユーザー一覧: 特定のロールを持つユーザー取得
    abstract member GetByRoleAsync: role: Role -> Task<Result<User list, string>>
    
    // 🔍 ユーザー検索: 部分マッチング検索（名前・メールアドレス）
    abstract member SearchUsersAsync: searchTerm: string -> Task<Result<User list, string>>
    
    // 🗑️ ユーザー削除: 論理削除（IsActiveをfalseに設定）
    abstract member DeleteAsync: userId: UserId -> Task<Result<unit, string>>

    // 📊 ユーザー統計: アクティブユーザー数・ロール別統計など
    abstract member GetUserStatisticsAsync: unit -> Task<Result<obj, string>> // 具体的な統計型は後で定義

    // 📋 プロジェクト割り当て: ユーザーにプロジェクトを割り当て
    // 【F#初学者向け解説】
    // ユーザー作成時にプロジェクトを割り当てるためのメソッドです。
    // UserProjectsテーブルにレコードを追加します。
    // ProjectManager権限チェックはApplication層で実施します。
    //
    // 【パラメータ】
    // - userId: UserId - 割り当て対象のユーザーID
    // - projectIds: int64 list - 割り当てるプロジェクトIDリスト（F#のlist型）
    //
    // 【戻り値】
    // - Task<Result<unit, string>>
    //   - Ok unit: 割り当て成功
    //   - Error string: 割り当て失敗時のエラーメッセージ
    abstract member AssignProjectsToUserAsync: userId: UserId * projectIds: int64 list -> Task<Result<unit, string>>

    // 📋 プロジェクト割り当て（Identity IDベース）: ユーザー作成時のプロジェクト割り当て
    // 【F#初学者向け解説】
    // ユーザー作成直後のプロジェクト割り当てに使用します。
    // ASP.NET Core IdentityのID（GUID文字列）を直接使用してUserProjectsテーブルに挿入します。
    // UserId（F#）からGUIDへの変換による不一致問題を回避するための専用メソッドです。
    //
    // 【パラメータ】
    // - identityId: string - AspNetUsers.Id（Identity ID、GUID文字列）
    // - projectIds: int64 list - 割り当てるプロジェクトIDリスト（F#のlist型）
    //
    // 【戻り値】
    // - Task<Result<unit, string>>
    //   - Ok unit: 割り当て成功
    //   - Error string: 割り当て失敗時のエラーメッセージ
    abstract member AssignProjectsToUserByIdentityIdAsync: identityId: string * projectIds: int64 list -> Task<Result<unit, string>>

    // 📋 プロジェクト割り当て更新: ユーザーのプロジェクト割り当てを更新
    // 【F#初学者向け解説】
    // ユーザー更新時にプロジェクト割り当てを更新するためのメソッドです。
    // 既存のUserProjectsレコードを削除し、新しいレコードを追加します。
    // ProjectManager権限チェックはApplication層で実施します。
    //
    // 【パラメータ】
    // - userId: UserId - 更新対象のユーザーID
    // - projectIds: int64 list - 新しいプロジェクトIDリスト（F#のlist型）
    //
    // 【戻り値】
    // - Task<Result<unit, string>>
    //   - Ok unit: 更新成功
    //   - Error string: 更新失敗時のエラーメッセージ
    abstract member UpdateUserProjectsAsync: userId: UserId * projectIds: int64 list -> Task<Result<unit, string>>

    // 📋 Identity IDベースのプロジェクト割り当て更新: ユーザーのプロジェクト割り当てを更新（編集時用）
    // 【F#初学者向け解説】
    // AssignProjectsToUserByIdentityIdAsyncと同様の背景で追加されたメソッドです。
    // ASP.NET Core IdentityのID（GUID文字列）を直接使用してプロジェクト割り当てを更新します。
    // ConvertUserIdToGuid()による合成GUID問題を回避するため、Identity IDを直接使用します。
    //
    // 【パラメータ】
    // - identityId: string - ASP.NET Core Identity のユーザーID（GUID文字列）
    // - projectIds: int64 list - 新しいプロジェクトIDリスト（F#のlist型）
    //
    // 【戻り値】
    // - Task<Result<unit, string>>
    //   - Ok unit: 更新成功
    //   - Error string: 更新失敗時のエラーメッセージ
    abstract member UpdateUserProjectsByIdentityIdAsync: identityId: string * projectIds: int64 list -> Task<Result<unit, string>>

// 📁 プロジェクトリポジトリインターフェース: プロジェクトデータの永続化抽象化  
type IProjectRepository =
    // 🔍 プロジェクト検索: IDによる一意検索
    abstract member GetByIdAsync: projectId: ProjectId -> Task<Result<Project option, string>>
    
    // 📋 プロジェクト一覧: アクティブなプロジェクトのみ取得
    abstract member GetActiveProjectsAsync: unit -> Task<Result<Project list, string>>
    
    // 💾 プロジェクト保存: 新規作成・更新の両方に対応
    abstract member SaveAsync: project: Project -> Task<Result<Project, string>>
    
    // 🗑️ プロジェクト削除: 論理削除（IsActiveをfalseに設定）
    abstract member DeleteAsync: projectId: ProjectId -> Task<Result<unit, string>>

// 🏷️ ドメインリポジトリインターフェース: ドメインデータの永続化抽象化
type IDomainRepository =
    // 🔍 ドメイン検索: IDによる一意検索
    abstract member GetByIdAsync: domainId: DomainId -> Task<Result<Domain option, string>>
    
    // 📋 ドメイン一覧: プロジェクト単位でのドメイン取得
    abstract member GetByProjectIdAsync: projectId: ProjectId -> Task<Result<Domain list, string>>
    
    // 💾 ドメイン保存: 新規作成・更新の両方に対応
    abstract member SaveAsync: domain: Domain -> Task<Result<Domain, string>>
    
    // 🗑️ ドメイン削除: 論理削除（IsActiveをfalseに設定）
    abstract member DeleteAsync: domainId: DomainId -> Task<Result<unit, string>>

// 📝 ユビキタス言語リポジトリインターフェース: 用語データの永続化抽象化
type IUbiquitousLanguageRepository =
    // 🔍 下書き検索: IDによる一意検索
    abstract member GetDraftByIdAsync: id: UbiquitousLanguageId -> Task<Result<DraftUbiquitousLanguage option, string>>
    
    // 🔍 正式版検索: IDによる一意検索
    abstract member GetFormalByIdAsync: id: UbiquitousLanguageId -> Task<Result<FormalUbiquitousLanguage option, string>>
    
    // 📋 下書き一覧: ドメイン単位での下書き取得
    abstract member GetDraftsByDomainIdAsync: domainId: DomainId -> Task<Result<DraftUbiquitousLanguage list, string>>
    
    // 📋 正式版一覧: ドメイン単位での正式版取得
    abstract member GetFormalsByDomainIdAsync: domainId: DomainId -> Task<Result<FormalUbiquitousLanguage list, string>>
    
    // 💾 下書き保存: 新規作成・更新の両方に対応
    abstract member SaveDraftAsync: draft: DraftUbiquitousLanguage -> Task<Result<DraftUbiquitousLanguage, string>>
    
    // 💾 正式版保存: 承認済み用語の永続化
    abstract member SaveFormalAsync: formal: FormalUbiquitousLanguage -> Task<Result<FormalUbiquitousLanguage, string>>
    
    // 🗑️ 下書き削除: 物理削除（下書きのため完全削除）
    abstract member DeleteDraftAsync: id: UbiquitousLanguageId -> Task<Result<unit, string>>

// 🔐 Phase A2: 認証サービスインターフェース（大幅拡張版）
// 【F#初学者向け解説】
// このインターフェースは、ASP.NET Core Identity の機能を F# のドメインモデルに適合させるためのアダプタです。
// Infrastructure層で実装され、Application層では抽象的に使用されます。新しい権限システムに対応しています。
type IAuthenticationService =
    // 🔑 ログイン: メールアドレス・パスワードによる認証
    abstract member LoginAsync: email: Email * password: string -> Task<Result<User, string>>
    
    // 👥 認証ユーザー作成: パスワードハッシュ化を含む完全なユーザー作成（新権限システム対応）
    // 戻り値: (User, IdentityId) - IdentityIdはプロジェクト割り当て等で使用
    abstract member CreateUserWithPasswordAsync: email: Email * name: UserName * role: Role * password: Password * createdBy: UserId -> Task<Result<User * string, string>>
    
    // 🔐 パスワード変更: セキュアなパスワード更新（Password値オブジェクト対応）
    abstract member ChangePasswordAsync: userId: UserId * oldPassword: string * newPassword: Password -> Task<Result<PasswordHash, string>>
    
    // 🔒 パスワードハッシュ生成: Password値オブジェクトからの安全なハッシュ化
    abstract member HashPasswordAsync: password: Password -> Task<Result<PasswordHash, string>>
    
    // ✅ パスワード検証: ハッシュとの照合
    abstract member VerifyPasswordAsync: password: string * hash: PasswordHash -> Task<Result<bool, string>>
    
    // 🆔 トークン生成: セッション管理用トークンの発行
    abstract member GenerateTokenAsync: user: User -> Task<Result<string, string>>
    
    // ✅ トークン検証: セッション有効性の確認
    abstract member ValidateTokenAsync: token: string -> Task<Result<User, string>>
    
    // 🔓 ログイン失敗記録: ロックアウト機能のサポート
    abstract member RecordFailedLoginAsync: userId: UserId -> Task<Result<User, string>>
    
    // ✅ ログイン成功記録: 失敗カウントのリセット
    abstract member RecordSuccessfulLoginAsync: userId: UserId -> Task<Result<User, string>>
    
    // 🔓 ロックアウト管理: アカウントロックアウトの設定・解除
    abstract member LockUserAsync: userId: UserId * lockoutEnd: System.DateTime -> Task<Result<unit, string>>
    abstract member UnlockUserAsync: userId: UserId -> Task<Result<unit, string>>
    
    // 🔄 セキュリティスタンプ更新: 認証状態の無効化
    abstract member UpdateSecurityStampAsync: userId: UserId -> Task<Result<unit, string>>
    
    // 📧 メールアドレス確認: メールアドレス確認機能
    abstract member SendEmailConfirmationAsync: email: Email -> Task<Result<unit, string>>
    abstract member ConfirmEmailAsync: userId: UserId * confirmationToken: string -> Task<Result<unit, string>>
    
    // 📱 二要素認証: 2FA管理
    abstract member EnableTwoFactorAsync: userId: UserId -> Task<Result<string, string>> // バックアップコードを返す
    abstract member DisableTwoFactorAsync: userId: UserId -> Task<Result<unit, string>>
    abstract member VerifyTwoFactorCodeAsync: userId: UserId * code: string -> Task<Result<bool, string>>
    
    // 👤 現在ユーザー取得: セッション・認証状態からの現在ユーザー情報取得
    abstract member GetCurrentUserAsync: unit -> Task<Result<User option, string>>

    // 🔓 統一ログアウト: パラメータなしログアウト処理
    abstract member LogoutAsync: unit -> Task<unit>

    // 🔐 Phase A9: パスワードリセット機能拡張
    // パスワードリセットトークン生成: 時間制限付きトークンの生成
    abstract member GeneratePasswordResetTokenAsync: userId: UserId * validFor: System.TimeSpan -> Task<Result<string, string>>

    // パスワードリセットトークン検証: トークンの有効性確認とユーザー取得
    abstract member ValidatePasswordResetTokenAsync: email: Email * token: string -> Task<Result<User, string>>

    // パスワードリセットトークン無効化: 使用済みトークンの無効化
    abstract member InvalidatePasswordResetTokenAsync: email: Email * token: string -> Task<unit>

    // 🔐 Phase B-F3 Step1.5 Stage4: 管理者によるパスワードリセット機能追加
    // 管理者リセット: SuperUserが他ユーザーのパスワードを強制変更
    // 【F#初学者向け解説】
    // このメソッドは、管理者がユーザーのパスワードを強制的にリセットするためのものです。
    // 通常のパスワード変更と異なり、現在のパスワードを知らなくても新しいパスワードを設定できます。
    // Infrastructure層でASP.NET Core IdentityのRemovePasswordAsync + AddPasswordAsyncを使用します。
    //
    // 【パラメータ】
    // - identityId: string - 対象ユーザーのASP.NET Core Identity ID（GUID文字列）
    // - newPassword: Password - 新しいパスワード（Password値オブジェクト）
    //
    // 【戻り値】
    // - Task<Result<unit, string>>
    //   - Ok unit: パスワードリセット成功
    //   - Error string: リセット失敗時のエラーメッセージ（ユーザー不存在、Identityエラー等）
    //
    // 【使用例】
    // let! result = authService.AdminResetPasswordAsync(targetIdentityId, newPassword)
    // match result with
    // | Ok () -> // リセット成功
    // | Error msg -> // エラー処理
    //
    // 【セキュリティ考慮事項】
    // - Application層で権限チェック（SuperUserのみ実行可能）を必ず実施すること
    // - パスワードリセット実行ログを必ず記録すること（監査証跡）
    // - 対象ユーザーへの通知メール送信を推奨（セキュリティ通知）
    abstract member AdminResetPasswordAsync: identityId: string * newPassword: Password -> Task<Result<unit, string>>

// 📧 Phase A2: 通知サービスインターフェース（ユーザー管理通知対応）
// 【F#初学者向け解説】
// ユーザー管理機能の拡張に伴い、様々な通知が必要になります。
// メール送信、システム内通知、管理者アラートなど、外部システムとの連携を抽象化します。
type INotificationService =
    // 👥 ユーザー管理関連通知
    // 🎉 ウェルカムメール: 新規ユーザー作成時の歓迎メール
    abstract member SendWelcomeEmailAsync: email: Email -> Task<Result<unit, string>>
    
    // 🎭 ロール変更通知: ユーザーロール変更の通知
    abstract member SendRoleChangeNotificationAsync: email: Email * newRole: Role -> Task<Result<unit, string>>
    
    // 🔐 パスワード変更通知: パスワード変更完了の通知
    abstract member SendPasswordChangeNotificationAsync: email: Email -> Task<Result<unit, string>>
    
    // 📧 メールアドレス変更確認: 新旧両方のアドレスへの確認通知
    abstract member SendEmailChangeConfirmationAsync: oldEmail: Email * newEmail: Email -> Task<Result<unit, string>>
    
    // 🔒 アカウント無効化通知: アカウント無効化の通知
    abstract member SendAccountDeactivationNotificationAsync: email: Email -> Task<Result<unit, string>>
    
    // ✅ アカウント有効化通知: アカウント再有効化の通知  
    abstract member SendAccountActivationNotificationAsync: email: Email -> Task<Result<unit, string>>
    
    // 🚨 セキュリティアラート: 異常なアクセス・ロックアウト等の通知
    abstract member SendSecurityAlertAsync: email: Email * alertType: string * details: string -> Task<Result<unit, string>>
    
    // ユビキタス言語管理関連通知（既存）
    // 📤 承認通知: 承認者への通知送信
    abstract member SendApprovalRequestAsync: approver: User * ubiquitousLanguage: DraftUbiquitousLanguage -> Task<Result<unit, string>>
    
    // ✅ 承認完了通知: 申請者への結果通知
    abstract member SendApprovalResultAsync: requester: User * ubiquitousLanguage: FormalUbiquitousLanguage * isApproved: bool -> Task<Result<unit, string>>

// 📊 ログサービスインターフェース: 構造化ログ出力の抽象化（ADR_008準拠）
// 【F#初学者向け解説】
// ADR_008のログ出力指針に従い、Application層での適切なログ出力を抽象化します。
// Infrastructure層でSerilogを使用した具体的実装が行われます。
type ILogger<'T> =
    // 📊 Information: 正常な業務処理の記録
    abstract member LogInformationAsync: message: string -> Task<unit>
    
    // ⚠️ Warning: ビジネスルール違反等の警告
    abstract member LogWarningAsync: message: string -> Task<unit>
    
    // ❌ Error: システムエラーの記録
    abstract member LogErrorAsync: message: string * ``exception``: System.Exception option -> Task<unit>
    
    // 🔍 Debug: 開発時のデバッグ情報
    abstract member LogDebugAsync: message: string -> Task<unit>

// 📊 レポートサービスインターフェース: 分析・レポート機能の抽象化
type IReportService =
    // 📈 統計情報: プロジェクト単位での用語使用状況
    abstract member GetProjectStatisticsAsync: projectId: ProjectId -> Task<Result<obj, string>> // 🔧 具体的な統計型は後で定義
    
    // 📋 エクスポート: 用語一覧のファイル出力
    abstract member ExportUbiquitousLanguagesAsync: domainId: DomainId * format: string -> Task<Result<byte[], string>>