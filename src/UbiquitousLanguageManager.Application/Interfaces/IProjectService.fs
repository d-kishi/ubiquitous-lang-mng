namespace UbiquitousLanguageManager.Application.Interfaces

// F# Domain層namespace階層化対応: Bounded Context別にopen
open UbiquitousLanguageManager.Domain.Common                  // UserId, ProjectId
open UbiquitousLanguageManager.Domain.Authentication          // User
open UbiquitousLanguageManager.Domain.ProjectManagement       // Project, Domain
open UbiquitousLanguageManager.Domain.UbiquitousLanguageManagement  // (使用なし)
open System.Threading.Tasks

/// <summary>
/// プロジェクトサービスインターフェース（Application層サービス基盤）
/// 
/// 【F#初学者向け解説】
/// このインターフェースは、プロジェクト管理のビジネスロジックを抽象化します。
/// プロジェクトの作成・更新・削除・検索機能を提供し、ユーザーとプロジェクトの関連付けや
/// プロジェクト内ドメインの管理機能も含みます。
/// 
/// Task<Result<T, string>>により、非同期処理と型安全なエラーハンドリングを実現し、
/// Infrastructure層の具体的実装から独立したインターフェースを提供します。
/// </summary>
type IProjectService =
    
    /// <summary>
    /// プロジェクトの新規作成
    /// </summary>
    /// <param name="project">作成するプロジェクト</param>
    /// <returns>作成されたプロジェクトまたはエラーメッセージ</returns>
    abstract member CreateAsync: project: Project -> Task<Result<Project, string>>
    
    /// <summary>
    /// プロジェクトの更新
    /// </summary>
    /// <param name="project">更新するプロジェクト</param>
    /// <returns>更新されたプロジェクトまたはエラーメッセージ</returns>
    abstract member UpdateAsync: project: Project -> Task<Result<Project, string>>
    
    /// <summary>
    /// プロジェクトの削除（論理削除）
    /// </summary>
    /// <param name="id">削除するプロジェクトのID</param>
    /// <returns>削除成功時はunit、失敗時はエラーメッセージ</returns>
    abstract member DeleteAsync: id: ProjectId -> Task<Result<unit, string>>
    
    /// <summary>
    /// IDによるプロジェクト取得
    /// </summary>
    /// <param name="id">取得するプロジェクトのID</param>
    /// <returns>見つかったプロジェクト（option型）またはエラーメッセージ</returns>
    abstract member GetByIdAsync: id: ProjectId -> Task<Result<Project option, string>>
    
    /// <summary>
    /// アクティブなプロジェクト一覧取得
    /// 論理削除されていないプロジェクトのみを返します
    /// </summary>
    /// <returns>プロジェクトリストまたはエラーメッセージ</returns>
    abstract member GetActiveProjectsAsync: unit -> Task<Result<Project list, string>>
    
    /// <summary>
    /// ユーザーが参加しているプロジェクト一覧取得
    /// </summary>
    /// <param name="userId">ユーザーID</param>
    /// <returns>プロジェクトリストまたはエラーメッセージ</returns>
    abstract member GetProjectsByUserAsync: userId: UserId -> Task<Result<Project list, string>>
    
    /// <summary>
    /// プロジェクトにユーザーを追加
    /// </summary>
    /// <param name="projectId">プロジェクトID</param>
    /// <param name="userId">追加するユーザーID</param>
    /// <param name="assignedBy">割り当て実行者のユーザーID</param>
    /// <returns>追加成功時はunit、失敗時はエラーメッセージ</returns>
    abstract member AddUserToProjectAsync: projectId: ProjectId * userId: UserId * assignedBy: UserId -> Task<Result<unit, string>>
    
    /// <summary>
    /// プロジェクトからユーザーを削除
    /// </summary>
    /// <param name="projectId">プロジェクトID</param>
    /// <param name="userId">削除するユーザーID</param>
    /// <param name="removedBy">削除実行者のユーザーID</param>
    /// <returns>削除成功時はunit、失敗時はエラーメッセージ</returns>
    abstract member RemoveUserFromProjectAsync: projectId: ProjectId * userId: UserId * removedBy: UserId -> Task<Result<unit, string>>
    
    /// <summary>
    /// プロジェクトに参加しているユーザー一覧取得
    /// </summary>
    /// <param name="projectId">プロジェクトID</param>
    /// <returns>ユーザーリストまたはエラーメッセージ</returns>
    abstract member GetProjectUsersAsync: projectId: ProjectId -> Task<Result<User list, string>>
    
    /// <summary>
    /// プロジェクトのドメイン一覧取得
    /// </summary>
    /// <param name="projectId">プロジェクトID</param>
    /// <returns>ドメインリストまたはエラーメッセージ</returns>
    abstract member GetProjectDomainsAsync: projectId: ProjectId -> Task<Result<Domain list, string>>
    
    /// <summary>
    /// プロジェクトの統計情報取得
    /// プロジェクト内のドメイン数・ユビキタス言語数・参加ユーザー数等の統計データ
    /// </summary>
    /// <param name="projectId">プロジェクトID</param>
    /// <returns>統計情報またはエラーメッセージ</returns>
    abstract member GetProjectStatisticsAsync: projectId: ProjectId -> Task<Result<obj, string>> // 具体的な統計型は後で定義