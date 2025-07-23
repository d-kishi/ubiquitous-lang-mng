namespace UbiquitousLanguageManager.Application

open UbiquitousLanguageManager.Domain
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
    
    // 💾 ユーザー保存: 新規作成・更新の両方に対応
    abstract member SaveAsync: user: User -> Task<Result<User, string>>
    
    // 📋 アクティブユーザー一覧: 有効なユーザーのみ取得
    abstract member GetAllActiveUsersAsync: unit -> Task<Result<User list, string>>
    
    // 📋 全ユーザー一覧: 無効化されたユーザーも含めて取得
    abstract member GetAllUsersAsync: unit -> Task<Result<User list, string>>
    
    // 📋 ユーザー一覧: プロジェクト単位でのユーザー取得
    abstract member GetByProjectIdAsync: projectId: ProjectId -> Task<Result<User list, string>>
    
    // 📋 ロール別ユーザー一覧: 特定のロールを持つユーザー取得
    abstract member GetByRoleAsync: role: Role -> Task<Result<User list, string>>
    
    // 🔍 ユーザー検索: 部分マッチング検索（名前・メールアドレス）
    abstract member SearchUsersAsync: searchTerm: string -> Task<Result<User list, string>>
    
    // 🗑️ ユーザー削除: 論理削除（IsActiveをfalseに設定）
    abstract member DeleteAsync: userId: UserId -> Task<Result<unit, string>>
    
    // 📊 ユーザー統計: アクティブユーザー数・ロール別統計など
    abstract member GetUserStatisticsAsync: unit -> Task<Result<obj, string>> // 具体的な統計型は後で定義

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
    abstract member CreateUserWithPasswordAsync: email: Email * name: UserName * role: Role * password: Password * createdBy: UserId -> Task<Result<User, string>>
    
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