namespace UbiquitousLanguageManager.Application

open UbiquitousLanguageManager.Domain
open System.Threading.Tasks

// 🎯 Application層インターフェース: Clean Architectureの境界定義
// Infrastructure層への依存関係を抽象化し、依存関係逆転の原則を実現

// 👤 ユーザーリポジトリインターフェース: ユーザーデータの永続化抽象化
type IUserRepository =
    // 🔍 ユーザー検索: メールアドレスによる一意検索
    abstract member GetByEmailAsync: email: Email -> Task<Result<User option, string>>
    
    // 🔍 ユーザー検索: IDによる一意検索
    abstract member GetByIdAsync: userId: UserId -> Task<Result<User option, string>>
    
    // 💾 ユーザー保存: 新規作成・更新の両方に対応
    abstract member SaveAsync: user: User -> Task<Result<User, string>>
    
    // 📋 ユーザー一覧: プロジェクト単位でのユーザー取得
    abstract member GetByProjectIdAsync: projectId: ProjectId -> Task<Result<User list, string>>
    
    // 🗑️ ユーザー削除: 論理削除（IsActiveをfalseに設定）
    abstract member DeleteAsync: userId: UserId -> Task<Result<unit, string>>

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

// 🔐 認証サービスインターフェース: ユーザー認証・認可の抽象化
type IAuthenticationService =
    // 🔑 ログイン: メールアドレス・パスワードによる認証
    abstract member LoginAsync: email: Email * password: string -> Task<Result<User, string>>
    
    // 🔐 パスワード変更: セキュアなパスワード更新
    abstract member ChangePasswordAsync: userId: UserId * oldPassword: string * newPassword: string -> Task<Result<unit, string>>
    
    // 🆔 トークン生成: セッション管理用トークンの発行
    abstract member GenerateTokenAsync: user: User -> Task<Result<string, string>>
    
    // ✅ トークン検証: セッション有効性の確認
    abstract member ValidateTokenAsync: token: string -> Task<Result<User, string>>

// 📧 通知サービスインターフェース: 外部通知システムの抽象化
type INotificationService =
    // 📤 承認通知: 承認者への通知送信
    abstract member SendApprovalRequestAsync: approver: User * ubiquitousLanguage: DraftUbiquitousLanguage -> Task<Result<unit, string>>
    
    // ✅ 承認完了通知: 申請者への結果通知
    abstract member SendApprovalResultAsync: requester: User * ubiquitousLanguage: FormalUbiquitousLanguage * isApproved: bool -> Task<Result<unit, string>>

// 📊 レポートサービスインターフェース: 分析・レポート機能の抽象化
type IReportService =
    // 📈 統計情報: プロジェクト単位での用語使用状況
    abstract member GetProjectStatisticsAsync: projectId: ProjectId -> Task<Result<obj, string>> // 🔧 具体的な統計型は後で定義
    
    // 📋 エクスポート: 用語一覧のファイル出力
    abstract member ExportUbiquitousLanguagesAsync: domainId: DomainId * format: string -> Task<Result<byte[], string>>