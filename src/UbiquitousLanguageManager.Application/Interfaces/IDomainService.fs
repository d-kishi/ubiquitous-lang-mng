namespace UbiquitousLanguageManager.Application.Interfaces

// F# Domain層namespace階層化対応: Bounded Context別にopen
open UbiquitousLanguageManager.Domain.Common                  // UserId, ProjectId, DomainId
open UbiquitousLanguageManager.Domain.Authentication          // User
open UbiquitousLanguageManager.Domain.ProjectManagement       // Domain
open UbiquitousLanguageManager.Domain.UbiquitousLanguageManagement  // (使用なし)
open System.Threading.Tasks

/// <summary>
/// ドメインサービスインターフェース（Application層サービス基盤）
/// 
/// 【F#初学者向け解説】
/// このインターフェースは、ドメイン境界とその承認者管理のビジネスロジックを抽象化します。
/// プロジェクト内でのドメイン管理・承認者の設定・ドメイン内のユビキタス言語管理等の
/// 機能を提供します。
/// 
/// ドメインはDDD（ドメイン駆動設計）における境界を表現し、その中でユビキタス言語が
/// 定義・管理される単位となります。
/// </summary>
type IDomainService =
    
    /// <summary>
    /// ドメインの新規作成
    /// </summary>
    /// <param name="domain">作成するドメイン</param>
    /// <returns>作成されたドメインまたはエラーメッセージ</returns>
    abstract member CreateAsync: domain: Domain -> Task<Result<Domain, string>>
    
    /// <summary>
    /// ドメインの更新
    /// </summary>
    /// <param name="domain">更新するドメイン</param>
    /// <returns>更新されたドメインまたはエラーメッセージ</returns>
    abstract member UpdateAsync: domain: Domain -> Task<Result<Domain, string>>
    
    /// <summary>
    /// ドメインの削除（論理削除）
    /// </summary>
    /// <param name="id">削除するドメインのID</param>
    /// <returns>削除成功時はunit、失敗時はエラーメッセージ</returns>
    abstract member DeleteAsync: id: DomainId -> Task<Result<unit, string>>
    
    /// <summary>
    /// IDによるドメイン取得
    /// </summary>
    /// <param name="id">取得するドメインのID</param>
    /// <returns>見つかったドメイン（option型）またはエラーメッセージ</returns>
    abstract member GetByIdAsync: id: DomainId -> Task<Result<Domain option, string>>
    
    /// <summary>
    /// プロジェクト単位でのドメイン一覧取得
    /// </summary>
    /// <param name="projectId">プロジェクトID</param>
    /// <returns>ドメインリストまたはエラーメッセージ</returns>
    abstract member GetByProjectIdAsync: projectId: ProjectId -> Task<Result<Domain list, string>>
    
    /// <summary>
    /// アクティブなドメイン一覧取得
    /// 論理削除されていないドメインのみを返します
    /// </summary>
    /// <returns>ドメインリストまたはエラーメッセージ</returns>
    abstract member GetActiveDomainsAsync: unit -> Task<Result<Domain list, string>>
    
    /// <summary>
    /// ドメインに承認者を追加
    /// </summary>
    /// <param name="domainId">ドメインID</param>
    /// <param name="approverId">承認者として追加するユーザーID</param>
    /// <param name="assignedBy">割り当て実行者のユーザーID</param>
    /// <returns>追加成功時はunit、失敗時はエラーメッセージ</returns>
    abstract member AddApproverAsync: domainId: DomainId * approverId: UserId * assignedBy: UserId -> Task<Result<unit, string>>
    
    /// <summary>
    /// ドメインから承認者を削除
    /// </summary>
    /// <param name="domainId">ドメインID</param>
    /// <param name="approverId">削除する承認者のユーザーID</param>
    /// <param name="removedBy">削除実行者のユーザーID</param>
    /// <returns>削除成功時はunit、失敗時はエラーメッセージ</returns>
    abstract member RemoveApproverAsync: domainId: DomainId * approverId: UserId * removedBy: UserId -> Task<Result<unit, string>>
    
    /// <summary>
    /// ドメインの承認者一覧取得
    /// </summary>
    /// <param name="domainId">ドメインID</param>
    /// <returns>承認者（ユーザー）リストまたはエラーメッセージ</returns>
    abstract member GetApproversAsync: domainId: DomainId -> Task<Result<User list, string>>
    
    /// <summary>
    /// ユーザーが承認者として設定されているドメイン一覧取得
    /// </summary>
    /// <param name="userId">ユーザーID</param>
    /// <returns>ドメインリストまたはエラーメッセージ</returns>
    abstract member GetDomainsByApproverAsync: userId: UserId -> Task<Result<Domain list, string>>
    
    /// <summary>
    /// ドメイン内のユビキタス言語統計取得
    /// ドメイン内の下書き数・承認済み数・承認待ち数等の統計情報
    /// </summary>
    /// <param name="domainId">ドメインID</param>
    /// <returns>統計情報またはエラーメッセージ</returns>
    abstract member GetDomainStatisticsAsync: domainId: DomainId -> Task<Result<obj, string>> // 具体的な統計型は後で定義
    
    /// <summary>
    /// ドメイン承認権限チェック
    /// 指定されたユーザーが指定されたドメインの承認権限を持つかを確認
    /// </summary>
    /// <param name="domainId">ドメインID</param>
    /// <param name="userId">チェック対象のユーザーID</param>
    /// <returns>承認権限有無またはエラーメッセージ</returns>
    abstract member HasApprovalPermissionAsync: domainId: DomainId * userId: UserId -> Task<Result<bool, string>>