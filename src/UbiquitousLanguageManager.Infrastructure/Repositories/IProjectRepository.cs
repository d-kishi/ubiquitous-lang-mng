using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.FSharp.Core;
using Microsoft.FSharp.Collections;

// F# Domain層の型を参照（ADR_019準拠）
using UbiquitousLanguageManager.Domain.Common;
using UbiquitousLanguageManager.Domain.ProjectManagement;
using DomainProject = UbiquitousLanguageManager.Domain.ProjectManagement.Project;
using DomainDomain = UbiquitousLanguageManager.Domain.ProjectManagement.Domain;

namespace UbiquitousLanguageManager.Infrastructure.Repositories;

/// <summary>
/// プロジェクトリポジトリインターフェース
/// F# Domain層のProjectエンティティに対するデータアクセス操作を定義
///
/// 【Blazor Server・F#初学者向け解説】
/// このインターフェースはClean Architectureの依存性逆転原則（DIP）に基づいています。
/// Application層（F#）がこのインターフェースを定義し、Infrastructure層（C#）が実装します。
/// これにより、ドメインロジックがインフラストラクチャの詳細に依存しない設計を実現します。
///
/// 【Result型の理解】
/// F#のResult型は、成功時の値とエラー時のメッセージを型安全に扱う関数型プログラミングの概念です。
/// - Result&lt;T, string&gt;: 成功時はT型の値、失敗時はstringのエラーメッセージを返す
/// - これにより、例外を投げずにエラーハンドリングができ、Railway-oriented Programmingを実現します
///
/// Phase B1 Step6: プロジェクト管理機能の基盤実装
/// </summary>
public interface IProjectRepository
{
    // =================================================================
    // 🔍 基本CRUD操作
    // =================================================================

    /// <summary>
    /// プロジェクトIDによる単一プロジェクト取得
    ///
    /// 【F#初学者向け解説】
    /// F#のResult型を使用することで、データベースエラーやプロジェクト未発見を
    /// 例外ではなく型安全な値として扱えます。呼び出し側はパターンマッチで
    /// 成功（Ok）と失敗（Error）のケースを明示的に処理します。
    /// </summary>
    /// <param name="projectId">F#のProjectId型（Value Object）</param>
    /// <returns>
    /// Result&lt;Option&lt;Project&gt;, string&gt;型:
    /// - Ok(Some(project)): プロジェクトが見つかった場合
    /// - Ok(None): プロジェクトが見つからなかった場合（エラーではない）
    /// - Error(message): データベースエラー等の異常時
    /// </returns>
    Task<FSharpResult<FSharpOption<DomainProject>, string>> GetByIdAsync(ProjectId projectId);

    /// <summary>
    /// アクティブなプロジェクト全件取得
    /// 論理削除されていないプロジェクトのみを取得
    ///
    /// 【EF Core最適化ポイント】
    /// - AsNoTracking(): 読み取り専用クエリで40-60%性能向上（Technical_Research_Results.md準拠）
    /// - HasQueryFilter(): DbContextでIsDeleted=falseのグローバルフィルター適用済み
    /// </summary>
    /// <returns>Result&lt;List&lt;Project&gt;, string&gt;型</returns>
    Task<FSharpResult<FSharpList<DomainProject>, string>> GetAllAsync();

    /// <summary>
    /// プロジェクト作成
    ///
    /// 【F#との統合ポイント】
    /// - 引数のProjectはF# Domainモデル（不変オブジェクト）
    /// - Infrastructure層でC# Entityに変換してEF Coreで永続化
    /// - 保存後、自動生成されたIDを含むF# Projectを返却
    /// </summary>
    /// <param name="project">F#のProjectドメインモデル</param>
    /// <returns>保存後のProject（自動生成されたIDを含む）</returns>
    Task<FSharpResult<DomainProject, string>> CreateAsync(DomainProject project);

    /// <summary>
    /// プロジェクト更新
    ///
    /// 【楽観的ロック制御】
    /// - UpdatedAtフィールドによる楽観的ロック実装
    /// - 更新競合時はDbUpdateConcurrencyExceptionをキャッチしてError返却
    /// </summary>
    /// <param name="project">更新対象のF# Projectドメインモデル</param>
    /// <returns>更新後のProject</returns>
    Task<FSharpResult<DomainProject, string>> UpdateAsync(DomainProject project);

    /// <summary>
    /// プロジェクト削除（論理削除）
    ///
    /// 【論理削除の理由】
    /// - プロジェクトに紐づくドメイン・ユビキタス言語の履歴保持
    /// - データベース設計書（行563-585）準拠
    /// - IsDeleted=trueに設定、物理削除は行わない
    /// </summary>
    /// <param name="projectId">削除対象のProjectId</param>
    /// <returns>
    /// Result&lt;Unit, string&gt;型:
    /// - Ok(unit): 削除成功（F#のUnit型は「値なし」を表す）
    /// - Error(message): 削除失敗（外部キー制約違反等）
    /// </returns>
    Task<FSharpResult<Unit, string>> DeleteAsync(ProjectId projectId);

    // =================================================================
    // 🔐 権限フィルタリング機能
    // =================================================================

    /// <summary>
    /// ユーザーがアクセス可能なプロジェクト一覧取得
    ///
    /// 【権限制御の実装】
    /// - SuperUser: 全プロジェクト取得
    /// - ProjectManager: 担当プロジェクトのみ取得（UserProjectsテーブル結合）
    /// - DomainApprover/GeneralUser: 所属プロジェクトのみ取得
    ///
    /// 【EF Core最適化】
    /// - Include()でUserProjectsを結合（Eager Loading）
    /// - ロールによる条件分岐でN+1問題回避
    /// </summary>
    /// <param name="userId">F#のUserId型</param>
    /// <param name="role">F#のRole判別共用体</param>
    /// <returns>ユーザーがアクセス可能なプロジェクトリスト</returns>
    Task<FSharpResult<FSharpList<DomainProject>, string>> GetProjectsByUserAsync(UserId userId, Role role);

    // =================================================================
    // ⚛️ 原子性保証：プロジェクト+デフォルトドメイン同時作成
    // =================================================================

    /// <summary>
    /// プロジェクトとデフォルトドメインの同時作成（トランザクション保証）
    ///
    /// 【原子性保証の実装】（Technical_Research_Results.md 行176-236準拠）
    /// 1. BeginTransactionAsync()で手動トランザクション開始
    /// 2. プロジェクト作成 → SaveChangesAsync()
    /// 3. 自動生成されたProjectIdを使用してデフォルトドメイン作成
    /// 4. ドメイン作成 → SaveChangesAsync()
    /// 5. 両方成功時のみCommitAsync()、エラー時は自動ロールバック
    ///
    /// 【データベース設計書準拠】（行563-638）
    /// - プロジェクト作成時に「共通」ドメイン自動作成（行579）
    /// - ドメインはプロジェクトに必須の依存関係（外部キー制約）
    ///
    /// 【F# Railway-oriented Programming統合】
    /// - Application層のProjectDomainServiceから呼び出される
    /// - Result型による成功/失敗の明示的な伝播
    /// - トランザクション失敗時はロールバックしてError返却
    ///
    /// 【Blazor Server初学者向け解説】
    /// Entity Framework Coreのトランザクション機能を使用することで、
    /// 複数のデータベース操作を「全て成功」または「全て失敗」のいずれかに保証します。
    /// 途中でエラーが発生した場合、それまでの変更は全て取り消されます。
    /// これにより、プロジェクトだけ作成されてドメインが作成されない不整合を防ぎます。
    /// </summary>
    /// <param name="project">作成するF# Projectドメインモデル</param>
    /// <param name="domain">作成するF# Domainドメインモデル（デフォルトドメイン）</param>
    /// <returns>
    /// Result&lt;(Project, Domain), string&gt;型（F#のタプル型）:
    /// - Ok((project, domain)): 両方の作成成功時、自動生成されたIDを含むタプル
    /// - Error(message): トランザクション失敗時のエラーメッセージ
    /// </returns>
    Task<FSharpResult<Tuple<DomainProject, DomainDomain>, string>> CreateProjectWithDefaultDomainAsync(
        DomainProject project,
        DomainDomain domain);

    // =================================================================
    // 🔍 検索・フィルタリング機能
    // =================================================================

    /// <summary>
    /// プロジェクト名による検索
    /// 完全一致検索（重複チェック用）
    ///
    /// 【使用目的】
    /// - プロジェクト作成時の重複チェック（Railway-oriented Programming）
    /// - ProjectDomainService.validateProjectNameで使用
    /// </summary>
    /// <param name="projectName">F#のProjectName型（Value Object）</param>
    /// <returns>一致するプロジェクト（存在しない場合はNone）</returns>
    Task<FSharpResult<FSharpOption<DomainProject>, string>> GetByNameAsync(ProjectName projectName);

    /// <summary>
    /// ユーザーが所有するプロジェクト一覧取得
    /// UserProjects.UserIdでフィルタリング
    ///
    /// 【EF Core最適化】
    /// - Include(p => p.UserProjects)でEager Loading
    /// - Where句でUserIdフィルタリング
    /// </summary>
    /// <param name="ownerId">所有者のF# UserId型</param>
    /// <returns>所有プロジェクトリスト</returns>
    Task<FSharpResult<FSharpList<DomainProject>, string>> GetByOwnerAsync(UserId ownerId);

    // =================================================================
    // 👥 UserProjects多対多関連管理（Phase B2拡張）
    // =================================================================

    /// <summary>
    /// UserProjectsレコード追加（プロジェクトメンバー追加）
    ///
    /// 【Phase B2: ユーザー・プロジェクト関連管理】
    /// - UserProjectsテーブルへのレコードINSERT
    /// - 複合一意制約違反チェック（UserId + ProjectId）
    /// - CASCADE DELETE設定済み（ユーザー削除・プロジェクト削除時に自動削除）
    ///
    /// 【EF Core実装ポイント】
    /// - SaveChangesAsync()による永続化
    /// - 楽観ロック（UpdatedAt）による競合制御
    /// </summary>
    /// <param name="userId">追加するユーザーのF# UserId型</param>
    /// <param name="projectId">プロジェクトのF# ProjectId型</param>
    /// <param name="updatedBy">最終更新者のF# UserId型</param>
    /// <returns>
    /// Result&lt;Unit, string&gt;型:
    /// - Ok(unit): 追加成功
    /// - Error(message): 重複エラー・データベースエラー等
    /// </returns>
    Task<FSharpResult<Unit, string>> AddUserToProjectAsync(UserId userId, ProjectId projectId, UserId updatedBy);

    /// <summary>
    /// UserProjectsレコード削除（プロジェクトメンバー削除）
    ///
    /// 【Phase B2: ユーザー・プロジェクト関連管理】
    /// - UserProjectsテーブルからのレコードDELETE
    /// - 最後の管理者削除防止チェック（ビジネスルール）
    ///
    /// 【注意事項】
    /// - 物理削除（UserProjectsテーブルには論理削除フラグなし）
    /// - プロジェクトに最低1名のProjectManager必須
    /// </summary>
    /// <param name="userId">削除するユーザーのF# UserId型</param>
    /// <param name="projectId">プロジェクトのF# ProjectId型</param>
    /// <returns>
    /// Result&lt;Unit, string&gt;型:
    /// - Ok(unit): 削除成功
    /// - Error(message): 最後の管理者削除エラー・レコード未発見等
    /// </returns>
    Task<FSharpResult<Unit, string>> RemoveUserFromProjectAsync(UserId userId, ProjectId projectId);

    /// <summary>
    /// プロジェクトメンバー一覧取得（UserProjects JOIN）
    ///
    /// 【Phase B2: ユーザー・プロジェクト関連管理】
    /// - UserProjects JOIN取得
    /// - Eager Loading徹底でN+1問題防止
    ///
    /// 【EF Core最適化】
    /// - Include(up => up.User).Include(up => up.Project)でEager Loading
    /// - AsNoTracking()で読み取り専用最適化
    /// </summary>
    /// <param name="projectId">プロジェクトのF# ProjectId型</param>
    /// <returns>
    /// Result&lt;List&lt;UserId&gt;, string&gt;型:
    /// - Ok(userIds): プロジェクトメンバーのUserIdリスト
    /// - Error(message): データベースエラー等
    /// </returns>
    Task<FSharpResult<FSharpList<UserId>, string>> GetProjectMembersAsync(ProjectId projectId);

    /// <summary>
    /// プロジェクトメンバー判定（UserProjects存在チェック）
    ///
    /// 【Phase B2: 権限制御マトリックス統合用】
    /// - UserProjects存在チェック
    /// - DomainApprover/GeneralUser権限判定に使用
    ///
    /// 【EF Core最適化】
    /// - AnyAsync()で効率的な存在チェック（COUNT不要）
    /// </summary>
    /// <param name="userId">ユーザーのF# UserId型</param>
    /// <param name="projectId">プロジェクトのF# ProjectId型</param>
    /// <returns>
    /// Result&lt;bool, string&gt;型:
    /// - Ok(true): メンバー
    /// - Ok(false): 非メンバー
    /// - Error(message): データベースエラー等
    /// </returns>
    Task<FSharpResult<bool, string>> IsUserProjectMemberAsync(UserId userId, ProjectId projectId);

    /// <summary>
    /// プロジェクトメンバー数取得（UserProjects COUNT）
    ///
    /// 【Phase B2: プロジェクト詳細画面表示用】
    /// - UserProjects COUNT集計
    /// - プロジェクト詳細画面の「メンバー数」表示に使用
    ///
    /// 【EF Core最適化】
    /// - CountAsync()で効率的なCOUNT集計
    /// </summary>
    /// <param name="projectId">プロジェクトのF# ProjectId型</param>
    /// <returns>
    /// Result&lt;int, string&gt;型:
    /// - Ok(count): メンバー数
    /// - Error(message): データベースエラー等
    /// </returns>
    Task<FSharpResult<int, string>> GetProjectMemberCountAsync(ProjectId projectId);

    /// <summary>
    /// プロジェクト作成 + デフォルトドメイン作成 + Owner自動追加（トランザクション保証）
    ///
    /// 【Phase B2: Phase B1トランザクションパターン拡張】
    /// - ProjectsレコードINSERT
    /// - DomainsレコードINSERT（デフォルトドメイン）
    /// - UserProjectsレコードINSERT（Owner自動追加）
    /// - トランザクション境界（同一トランザクション）
    ///
    /// 【原子性保証の実装】
    /// 1. BeginTransactionAsync()で手動トランザクション開始
    /// 2. プロジェクト作成 → SaveChangesAsync()
    /// 3. デフォルトドメイン作成 → SaveChangesAsync()
    /// 4. UserProjects作成 → SaveChangesAsync()
    /// 5. 全て成功時のみCommitAsync()、エラー時は自動ロールバック
    ///
    /// 【Blazor Server初学者向け解説】
    /// Phase B1のCreateProjectWithDefaultDomainAsyncを拡張し、
    /// プロジェクト作成者を自動的にプロジェクトメンバーとして追加します。
    /// これにより、作成直後からプロジェクトへのアクセス権が保証されます。
    /// </summary>
    /// <param name="project">作成するF# Projectドメインモデル</param>
    /// <param name="domain">作成するF# Domainドメインモデル（デフォルトドメイン）</param>
    /// <param name="ownerId">プロジェクト作成者のF# UserId型（Owner）</param>
    /// <returns>
    /// Result&lt;(Project, Domain), string&gt;型（F#のタプル型）:
    /// - Ok((project, domain)): 全ての作成成功時、自動生成されたIDを含むタプル
    /// - Error(message): トランザクション失敗時のエラーメッセージ
    /// </returns>
    Task<FSharpResult<Tuple<DomainProject, DomainDomain>, string>> SaveProjectWithDefaultDomainAndOwnerAsync(
        DomainProject project,
        DomainDomain domain,
        UserId ownerId);

    /// <summary>
    /// プロジェクト関連データ件数取得（削除確認画面用）
    ///
    /// 【Phase B2: GetRelatedDataCountAsync拡張】
    /// - UserProjectsカウント追加
    /// - プロジェクト削除確認画面で「削除対象の関連データ」表示に使用
    ///
    /// 【既存実装との統合】
    /// - Phase B1で実装済みのDomainsカウント・UbiquitousLanguagesカウントに追加
    /// </summary>
    /// <param name="projectId">プロジェクトのF# ProjectId型</param>
    /// <returns>
    /// Result&lt;(int domainCount, int languageCount, int memberCount), string&gt;型:
    /// - Ok((domains, languages, members)): 関連データ件数のタプル
    /// - Error(message): データベースエラー等
    /// </returns>
    Task<FSharpResult<Tuple<int, int, int>, string>> GetRelatedDataCountAsync(ProjectId projectId);
}
