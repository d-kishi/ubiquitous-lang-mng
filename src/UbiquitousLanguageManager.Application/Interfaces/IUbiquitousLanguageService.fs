namespace UbiquitousLanguageManager.Application.Interfaces

// F# Domain層namespace階層化対応: Bounded Context別にopen
open UbiquitousLanguageManager.Domain.Common                  // UserId, ProjectId
open UbiquitousLanguageManager.Domain.Authentication          // (使用なし)
open UbiquitousLanguageManager.Domain.ProjectManagement       // (使用なし)
open UbiquitousLanguageManager.Domain.UbiquitousLanguageManagement  // DraftUbiquitousLanguage, FormalUbiquitousLanguage, UbiquitousLanguageId
open System.Threading.Tasks

/// <summary>
/// ユビキタス言語サービスインターフェース（Application層サービス基盤）
/// 
/// 【F#初学者向け解説】
/// このインターフェースは、ユビキタス言語のビジネスロジックを抽象化します。
/// Clean Architectureにおいて、Application層のユースケースから呼び出される
/// サービス層のインターフェースとして機能します。
/// 
/// Result<T, string>型により、成功時の結果と失敗時のエラーメッセージを
/// 型安全に表現し、例外に頼らないエラーハンドリングを実現します。
/// </summary>
type IUbiquitousLanguageService =
    
    /// <summary>
    /// ユビキタス言語の新規作成
    /// </summary>
    /// <param name="ubiquitousLanguage">作成するユビキタス言語（下書き状態）</param>
    /// <returns>作成されたユビキタス言語または エラーメッセージ</returns>
    abstract member CreateAsync: ubiquitousLanguage: DraftUbiquitousLanguage -> Task<Result<DraftUbiquitousLanguage, string>>
    
    /// <summary>
    /// ユビキタス言語の更新
    /// </summary>
    /// <param name="ubiquitousLanguage">更新するユビキタス言語（下書き状態）</param>
    /// <returns>更新されたユビキタス言語またはエラーメッセージ</returns>
    abstract member UpdateAsync: ubiquitousLanguage: DraftUbiquitousLanguage -> Task<Result<DraftUbiquitousLanguage, string>>
    
    /// <summary>
    /// ユビキタス言語の削除（下書きのみ・物理削除）
    /// </summary>
    /// <param name="id">削除するユビキタス言語のID</param>
    /// <returns>削除成功時はunit、失敗時はエラーメッセージ</returns>
    abstract member DeleteAsync: id: UbiquitousLanguageId -> Task<Result<unit, string>>
    
    /// <summary>
    /// IDによるユビキタス言語取得（下書き）
    /// </summary>
    /// <param name="id">取得するユビキタス言語のID</param>
    /// <returns>見つかったユビキタス言語（option型）またはエラーメッセージ</returns>
    abstract member GetDraftByIdAsync: id: UbiquitousLanguageId -> Task<Result<DraftUbiquitousLanguage option, string>>
    
    /// <summary>
    /// IDによるユビキタス言語取得（正式版）
    /// </summary>
    /// <param name="id">取得するユビキタス言語のID</param>
    /// <returns>見つかったユビキタス言語（option型）またはエラーメッセージ</returns>
    abstract member GetFormalByIdAsync: id: UbiquitousLanguageId -> Task<Result<FormalUbiquitousLanguage option, string>>
    
    /// <summary>
    /// プロジェクト単位でのユビキタス言語一覧取得（下書き）
    /// </summary>
    /// <param name="projectId">プロジェクトID</param>
    /// <returns>ユビキタス言語リストまたはエラーメッセージ</returns>
    abstract member GetDraftsByProjectAsync: projectId: ProjectId -> Task<Result<DraftUbiquitousLanguage list, string>>
    
    /// <summary>
    /// プロジェクト単位でのユビキタス言語一覧取得（正式版）
    /// </summary>
    /// <param name="projectId">プロジェクトID</param>
    /// <returns>ユビキタス言語リストまたはエラーメッセージ</returns>
    abstract member GetFormalsByProjectAsync: projectId: ProjectId -> Task<Result<FormalUbiquitousLanguage list, string>>
    
    /// <summary>
    /// ドメイン単位でのユビキタス言語一覧取得（下書き）
    /// </summary>
    /// <param name="domainId">ドメインID</param>
    /// <returns>ユビキタス言語リストまたはエラーメッセージ</returns>
    abstract member GetDraftsByDomainAsync: domainId: DomainId -> Task<Result<DraftUbiquitousLanguage list, string>>
    
    /// <summary>
    /// ドメイン単位でのユビキタス言語一覧取得（正式版）
    /// </summary>
    /// <param name="domainId">ドメインID</param>
    /// <returns>ユビキタス言語リストまたはエラーメッセージ</returns>
    abstract member GetFormalsByDomainAsync: domainId: DomainId -> Task<Result<FormalUbiquitousLanguage list, string>>
    
    /// <summary>
    /// ユビキタス言語の承認処理（下書き→正式版）
    /// </summary>
    /// <param name="id">承認するユビキタス言語のID</param>
    /// <param name="approverId">承認者のユーザーID</param>
    /// <returns>作成された正式版ユビキタス言語またはエラーメッセージ</returns>
    abstract member ApproveAsync: id: UbiquitousLanguageId * approverId: UserId -> Task<Result<FormalUbiquitousLanguage, string>>
    
    /// <summary>
    /// ユビキタス言語の承認却下処理
    /// </summary>
    /// <param name="id">却下するユビキタス言語のID</param>
    /// <param name="approverId">承認者のユーザーID</param>
    /// <param name="reason">却下理由</param>
    /// <returns>却下処理成功時はunit、失敗時はエラーメッセージ</returns>
    abstract member RejectAsync: id: UbiquitousLanguageId * approverId: UserId * reason: string -> Task<Result<unit, string>>