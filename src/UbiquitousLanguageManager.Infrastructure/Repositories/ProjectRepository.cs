using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Logging;
using Microsoft.FSharp.Core;
using Microsoft.FSharp.Collections;

// F# Domain層の型を参照（ADR_019準拠）
using UbiquitousLanguageManager.Domain.Common;
using UbiquitousLanguageManager.Domain.ProjectManagement;
using DomainProject = UbiquitousLanguageManager.Domain.ProjectManagement.Project;
using DomainDomain = UbiquitousLanguageManager.Domain.ProjectManagement.Domain;

// Infrastructure層の参照
using UbiquitousLanguageManager.Infrastructure.Data;
using EntityProject = UbiquitousLanguageManager.Infrastructure.Data.Entities.Project;
using EntityDomain = UbiquitousLanguageManager.Infrastructure.Data.Entities.Domain;
using EntityUserProject = UbiquitousLanguageManager.Infrastructure.Data.Entities.UserProject;

// Application.ProjectManagement層の型参照（Phase B-F3）
using ProjectListResultDto = UbiquitousLanguageManager.Application.ProjectManagement.ProjectListResultDto;
using SearchProjectsQuery = UbiquitousLanguageManager.Application.ProjectManagement.SearchProjectsQuery;

namespace UbiquitousLanguageManager.Infrastructure.Repositories;

/// <summary>
/// プロジェクトリポジトリの実装
/// F# Domain層のProjectエンティティに対するデータアクセス操作を実装
///
/// 【Blazor Server・F#初学者向け解説】
/// このRepositoryはClean ArchitectureのInfrastructure層に位置し、
/// Entity Framework Core（EF Core）を使用してPostgreSQLデータベースへアクセスします。
/// F# Domain層で定義された型を、C# EntityとしてデータベースにORM（Object-Relational Mapping）します。
///
/// 【重要な設計パターン】
/// - Repository Pattern: データアクセスロジックをカプセル化
/// - Smart Constructor: F#値オブジェクトのバリデーションを活用
/// - Railway-oriented Programming: Result型による明示的なエラーハンドリング
/// - BeginTransaction: 原子性保証（複数操作の全成功/全失敗）
///
/// Phase B1 Step6: プロジェクト管理機能の基盤実装
/// Phase B-F3 Step1.5: ProjectManagementService用インターフェース実装追加
/// </summary>
public class ProjectRepository :
    IProjectRepository, // Infrastructure層のインターフェース
    Application.IProjectRepository, // Application層のインターフェース（明示的実装）
    Application.ProjectManagement.IProjectRepository // ProjectManagementService用（Phase B-F3）
{
    private readonly UbiquitousLanguageDbContext _context;
    private readonly ILogger<ProjectRepository> _logger;

    /// <summary>
    /// ProjectRepositoryのコンストラクタ
    /// DIコンテナからDbContextとLoggerを注入
    ///
    /// 【Blazor Server初学者向け解説】
    /// ASP.NET CoreのDI（Dependency Injection）により、
    /// DbContextとLoggerが自動的に注入されます。
    /// これによりテスト時にモックを注入しやすく、疎結合な設計を実現します。
    /// </summary>
    /// <param name="context">Entity Framework Core DbContext</param>
    /// <param name="logger">ASP.NET Core標準のLogger</param>
    public ProjectRepository(
        UbiquitousLanguageDbContext context,
        ILogger<ProjectRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    // =================================================================
    // 🔍 基本CRUD操作
    // =================================================================

    /// <summary>
    /// プロジェクトIDによる単一プロジェクト取得
    ///
    /// 【EF Core最適化ポイント】
    /// - AsNoTracking(): 読み取り専用クエリで40-60%性能向上
    /// - FirstOrDefaultAsync(): 単一レコード取得に最適化
    /// </summary>
    public async Task<FSharpResult<FSharpOption<DomainProject>, string>> GetByIdAsync(ProjectId projectId)
    {
        try
        {
            _logger.LogDebug("プロジェクト取得開始: ProjectId={ProjectId}", projectId.Item);

            // 【F#初学者向け解説】
            // ProjectId は F# の判別共用体型で、.Item プロパティで内部の long 値にアクセスします。
            var projectEntity = await _context.Projects
                .AsNoTracking()  // 読み取り専用クエリで性能向上
                .FirstOrDefaultAsync(p => p.ProjectId == projectId.Item);

            if (projectEntity == null)
            {
                _logger.LogDebug("プロジェクトが見つかりません: ProjectId={ProjectId}", projectId.Item);
                return FSharpResult<FSharpOption<DomainProject>, string>.NewOk(
                    FSharpOption<DomainProject>.None);
            }

            // C# Entity → F# Domain型変換
            var project = ConvertToFSharpProject(projectEntity);
            _logger.LogInformation("プロジェクト取得成功: ProjectId={ProjectId}, Name={Name}",
                projectId.Item, project.Name.Value);

            return FSharpResult<FSharpOption<DomainProject>, string>.NewOk(
                FSharpOption<DomainProject>.Some(project));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "プロジェクト取得でエラー発生: ProjectId={ProjectId}", projectId.Item);
            return FSharpResult<FSharpOption<DomainProject>, string>.NewError(
                $"プロジェクト取得に失敗しました: {ex.Message}");
        }
    }

    /// <summary>
    /// アクティブなプロジェクト全件取得
    /// 論理削除されていないプロジェクトのみを取得
    ///
    /// 【EF Core最適化ポイント】
    /// - HasQueryFilter(): DbContextでIsDeleted=falseのグローバルフィルター適用済み
    /// - OrderBy(): ProjectNameでソートして一貫した順序を保証
    /// </summary>
    public async Task<FSharpResult<FSharpList<DomainProject>, string>> GetAllAsync()
    {
        try
        {
            _logger.LogDebug("全プロジェクト取得開始");

            var projectEntities = await _context.Projects
                .AsNoTracking()
                .OrderBy(p => p.ProjectName)  // プロジェクト名順でソート
                .ToListAsync();

            var projects = projectEntities.Select(ConvertToFSharpProject).ToList();

            _logger.LogInformation("全プロジェクト取得成功: Count={Count}", projects.Count);

            return FSharpResult<FSharpList<DomainProject>, string>.NewOk(
                ListModule.OfSeq(projects));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "全プロジェクト取得でエラー発生");
            return FSharpResult<FSharpList<DomainProject>, string>.NewError(
                $"プロジェクト取得に失敗しました: {ex.Message}");
        }
    }

    /// <summary>
    /// プロジェクト作成
    ///
    /// 【F#との統合ポイント】
    /// - F# Domainモデル（不変オブジェクト）をC# Entityに変換
    /// - SaveChangesAsync()でデータベースに永続化
    /// - 自動生成されたIDを含むF# Projectを返却
    ///
    /// 【重複チェック】
    /// - プロジェクト名の一意性を保証
    /// - 既存プロジェクト存在時はエラー返却
    /// </summary>
    public async Task<FSharpResult<DomainProject, string>> CreateAsync(DomainProject project)
    {
        try
        {
            var projectName = project.Name.Value;
            _logger.LogDebug("プロジェクト作成開始: Name={Name}", projectName);

            // 1. 重複チェック
            var existingProject = await _context.Projects
                .FirstOrDefaultAsync(p => p.ProjectName == projectName);

            if (existingProject != null)
            {
                _logger.LogWarning("プロジェクト名重複: Name={Name}", projectName);
                return FSharpResult<DomainProject, string>.NewError(
                    $"プロジェクト名'{projectName}'は既に使用されています");
            }

            // 2. F# Domain型 → C# Entity変換
            // 【F#初学者向け解説】
            // F# の Option型は get_IsSome() で Some かどうか判定し、Value で内部値を取得します。
            var projectEntity = new EntityProject
            {
                ProjectName = projectName,
                Description = FSharpOption<string>.get_IsSome(project.Description.Value)
                    ? project.Description.Value.Value
                    : null,
                UpdatedBy = project.OwnerId.Item.ToString(),  // long → string変換
                UpdatedAt = DateTime.UtcNow,
                IsDeleted = false
            };

            // 3. 保存
            _context.Projects.Add(projectEntity);
            await _context.SaveChangesAsync();

            _logger.LogInformation("プロジェクト作成成功: ProjectId={ProjectId}, Name={Name}",
                projectEntity.ProjectId, projectEntity.ProjectName);

            // 4. C# Entity → F# Domain型変換（自動生成されたIDを含む）
            var resultProject = ConvertToFSharpProject(projectEntity);

            return FSharpResult<DomainProject, string>.NewOk(resultProject);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "プロジェクト作成でエラー発生: Name={Name}",
                project.Name.Value);
            return FSharpResult<DomainProject, string>.NewError(
                $"プロジェクト作成に失敗しました: {ex.Message}");
        }
    }

    /// <summary>
    /// プロジェクト更新
    ///
    /// 【楽観的ロック制御】
    /// - UpdatedAtフィールドによる楽観的ロック実装
    /// - 更新競合時はDbUpdateConcurrencyExceptionをキャッチしてError返却
    ///
    /// 【否定的仕様遵守】
    /// - プロジェクト名変更は禁止（データベース設計書準拠）
    /// - 名前変更試行時はエラー返却
    /// </summary>
    public async Task<FSharpResult<DomainProject, string>> UpdateAsync(DomainProject project)
    {
        try
        {
            _logger.LogDebug("プロジェクト更新開始: ProjectId={ProjectId}", project.Id.Item);

            var projectEntity = await _context.Projects
                .FirstOrDefaultAsync(p => p.ProjectId == project.Id.Item);

            if (projectEntity == null)
            {
                _logger.LogWarning("プロジェクトが見つかりません: ProjectId={ProjectId}", project.Id.Item);
                return FSharpResult<DomainProject, string>.NewError(
                    $"プロジェクトID'{project.Id.Item}'が見つかりません");
            }

            // 楽観的ロック（UpdatedAtによる競合検出）
            // 【Blazor Server初学者向け解説】
            // 楽観的ロックは、更新時に元の更新日時と現在のデータベースの更新日時を比較し、
            // 異なる場合は他のユーザーが更新したと判断してエラーとします。
            // これにより、後勝ち（Last Write Wins）による更新の上書きを防ぎます。
            //
            // 【F#初学者向け解説】
            // F# の Option型で Some の場合のみチェックを実行します。
            // None の場合は楽観的ロックをスキップ（新規作成直後のオブジェクト等）
            if (FSharpOption<DateTime>.get_IsSome(project.UpdatedAt))
            {
                var expectedUpdatedAt = project.UpdatedAt.Value;
                if (projectEntity.UpdatedAt != expectedUpdatedAt)
                {
                    _logger.LogWarning(
                        "プロジェクト更新競合: ProjectId={ProjectId}, Expected={Expected}, Actual={Actual}",
                        project.Id.Item, expectedUpdatedAt, projectEntity.UpdatedAt);
                    return FSharpResult<DomainProject, string>.NewError(
                        "プロジェクトが他のユーザーによって更新されています（競合）。再度取得してください。");
                }
            }
            else
            {
                // UpdatedAtがNoneの場合は楽観的ロックチェックをスキップ
                _logger.LogDebug("楽観的ロックチェックスキップ（UpdatedAt=None）: ProjectId={ProjectId}",
                    project.Id.Item);
            }

            // 否定的仕様: プロジェクト名変更は禁止
            // 【F#初学者向け解説】
            // データベース設計書において、プロジェクト名はシステム内で一意であり、
            // 変更を許可すると関連エンティティとの整合性維持が困難になるため禁止されています。
            var newProjectName = project.Name.Value;
            if (newProjectName != projectEntity.ProjectName)
            {
                _logger.LogWarning("プロジェクト名変更試行: ProjectId={ProjectId}, OldName={OldName}, NewName={NewName}",
                    project.Id.Item, projectEntity.ProjectName, newProjectName);
                return FSharpResult<DomainProject, string>.NewError(
                    "プロジェクト名の変更は許可されていません");
            }

            // 更新（説明のみ）
            projectEntity.Description = FSharpOption<string>.get_IsSome(project.Description.Value)
                ? project.Description.Value.Value
                : null;
            projectEntity.UpdatedAt = DateTime.UtcNow;
            projectEntity.UpdatedBy = project.OwnerId.Item.ToString();

            await _context.SaveChangesAsync();

            _logger.LogInformation("プロジェクト更新成功: ProjectId={ProjectId}", project.Id.Item);

            var resultProject = ConvertToFSharpProject(projectEntity);
            return FSharpResult<DomainProject, string>.NewOk(resultProject);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            _logger.LogError(ex, "プロジェクト更新で競合発生: ProjectId={ProjectId}", project.Id.Item);
            return FSharpResult<DomainProject, string>.NewError(
                "プロジェクトが他のユーザーによって更新されています（競合）。再度取得してください。");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "プロジェクト更新でエラー発生: ProjectId={ProjectId}", project.Id.Item);
            return FSharpResult<DomainProject, string>.NewError(
                $"プロジェクト更新に失敗しました: {ex.Message}");
        }
    }

    /// <summary>
    /// プロジェクト削除（論理削除）
    ///
    /// 【論理削除の理由】
    /// - プロジェクトに紐づくドメイン・ユビキタス言語の履歴保持
    /// - データベース設計書（行563-585）準拠
    /// - IsDeleted=trueに設定、物理削除は行わない
    /// </summary>
    public async Task<FSharpResult<Unit, string>> DeleteAsync(ProjectId projectId)
    {
        try
        {
            _logger.LogDebug("プロジェクト削除開始: ProjectId={ProjectId}", projectId.Item);

            var projectEntity = await _context.Projects
                .IgnoreQueryFilters()  // 論理削除フィルターを無効化
                .FirstOrDefaultAsync(p => p.ProjectId == projectId.Item);

            if (projectEntity == null)
            {
                _logger.LogWarning("プロジェクトが見つかりません: ProjectId={ProjectId}", projectId.Item);
                return FSharpResult<Unit, string>.NewError(
                    $"プロジェクトID'{projectId.Item}'が見つかりません");
            }

            // 論理削除
            projectEntity.IsDeleted = true;
            projectEntity.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation("プロジェクト削除成功: ProjectId={ProjectId}", projectId.Item);

            // 【F#初学者向け解説】
            // F# の Unit 型は「返り値なし」を表す型です。C# の void に相当しますが、
            // Result型では値が必要なため null! を使用します。
            return FSharpResult<Unit, string>.NewOk(null!);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "プロジェクト削除でエラー発生: ProjectId={ProjectId}", projectId.Item);
            return FSharpResult<Unit, string>.NewError(
                $"プロジェクト削除に失敗しました: {ex.Message}");
        }
    }

    // =================================================================
    // 🔐 権限フィルタリング機能
    // =================================================================

    /// <summary>
    /// ユーザーがアクセス可能なプロジェクト一覧取得
    ///
    /// 【権限制御の実装】
    /// - SuperUser: 全プロジェクト取得
    /// - ProjectManager: 全プロジェクト取得
    /// - DomainApprover: 割り当てられたプロジェクトのみ取得（UserProjectsテーブル結合）
    /// - GeneralUser: 所有プロジェクトのみ取得
    ///
    /// 【EF Core最適化】
    /// - Include()でUserProjectsを結合（Eager Loading）
    /// - ロールによる条件分岐でN+1問題回避
    /// </summary>
    public async Task<FSharpResult<FSharpList<DomainProject>, string>> GetProjectsByUserAsync(
        UserId userId, Role role)
    {
        try
        {
            _logger.LogDebug("ユーザー別プロジェクト取得開始: UserId={UserId}, Role={Role}",
                userId.Item, RoleToString(role));

            IQueryable<EntityProject> query = _context.Projects.AsNoTracking();

            // ロール別フィルタリング
            // 【F#初学者向け解説】
            // F# の判別共用体（Role型）は、IsSuperUser、IsProjectManager等のプロパティで
            // 各ケースを判定できます。これはパターンマッチングの代替として使用できます。
            if (role.IsSuperUser)
            {
                // SuperUser: 全プロジェクト取得
                // フィルタなし
                _logger.LogDebug("全プロジェクトアクセス権限: Role={Role}", RoleToString(role));
            }
            else if (role.IsProjectManager || role.IsDomainApprover)
            {
                // ProjectManager・DomainApprover: 割り当てられたプロジェクトのみ
                // 【UI設計書3.6章準拠】PMは担当プロジェクトのみ取得
                // 【EF Core最適化ポイント】
                // Include()でUserProjectsを明示的に読み込み、N+1問題を回避
                _logger.LogDebug("割り当てプロジェクトフィルタ適用: UserId={UserId}", userId.Item);
                query = query.Include(p => p.UserProjects)
                             .Where(p => p.UserProjects.Any(up => up.UserId == userId.Item.ToString()));
            }
            else // GeneralUser
            {
                // GeneralUser: 自分が所有するプロジェクトのみ
                _logger.LogDebug("所有プロジェクトフィルタ適用: UserId={UserId}", userId.Item);
                query = query.Where(p => p.UpdatedBy == userId.Item.ToString());
            }

            var projectEntities = await query
                .OrderBy(p => p.ProjectName)
                .ToListAsync();

            var projects = projectEntities.Select(ConvertToFSharpProject).ToList();

            _logger.LogInformation("ユーザー別プロジェクト取得成功: UserId={UserId}, Count={Count}",
                userId.Item, projects.Count);

            return FSharpResult<FSharpList<DomainProject>, string>.NewOk(
                ListModule.OfSeq(projects));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "ユーザー別プロジェクト取得でエラー発生: UserId={UserId}",
                userId.Item);
            return FSharpResult<FSharpList<DomainProject>, string>.NewError(
                $"プロジェクト取得に失敗しました: {ex.Message}");
        }
    }

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
    /// 【Blazor Server初学者向け解説】
    /// Entity Framework Coreのトランザクション機能を使用することで、
    /// 複数のデータベース操作を「全て成功」または「全て失敗」のいずれかに保証します。
    /// 途中でエラーが発生した場合、それまでの変更は全て取り消されます。
    /// これにより、プロジェクトだけ作成されてドメインが作成されない不整合を防ぎます。
    /// </summary>
    public async Task<FSharpResult<Tuple<DomainProject, DomainDomain>, string>>
        CreateProjectWithDefaultDomainAsync(DomainProject project, DomainDomain domain)
    {
        // InMemory Database判定（テスト実行環境対応）
        // 【Blazor Server初学者向け解説】
        // InMemory Databaseはテスト用の軽量データベースで、トランザクション機能をサポートしていません。
        // 本番環境（PostgreSQL）とテスト環境で異なる処理を行う必要があります。
        var isInMemory = _context.Database.ProviderName == "Microsoft.EntityFrameworkCore.InMemory";

        if (isInMemory)
        {
            // InMemory Database: トランザクションなしで実行
            return await CreateProjectWithDefaultDomainInMemoryAsync(project, domain);
        }
        else
        {
            // 通常のDB: トランザクション使用（Technical_Research_Results.md準拠）
            return await CreateProjectWithDefaultDomainWithTransactionAsync(project, domain);
        }
    }

    /// <summary>
    /// プロジェクト・デフォルトドメイン同時作成（InMemory Database用）
    /// トランザクションを使用せず、連続したSaveChangesAsync()で実行
    /// </summary>
    private async Task<FSharpResult<Tuple<DomainProject, DomainDomain>, string>>
        CreateProjectWithDefaultDomainInMemoryAsync(DomainProject project, DomainDomain domain)
    {
        try
        {
            var projectName = project.Name.Value;
            _logger.LogDebug("プロジェクト・デフォルトドメイン同時作成開始（InMemory）: ProjectName={ProjectName}",
                projectName);

            // 1. プロジェクト重複チェック
            var existingProject = await _context.Projects
                .FirstOrDefaultAsync(p => p.ProjectName == projectName);

            if (existingProject != null)
            {
                _logger.LogWarning("プロジェクト名重複: Name={Name}", projectName);
                return FSharpResult<Tuple<DomainProject, DomainDomain>, string>.NewError(
                    $"プロジェクト名'{projectName}'は既に使用されています");
            }

            // 2. プロジェクト作成
            var projectEntity = new EntityProject
            {
                ProjectName = projectName,
                Description = FSharpOption<string>.get_IsSome(project.Description.Value)
                    ? project.Description.Value.Value
                    : null,
                UpdatedBy = project.OwnerId.Item.ToString(),
                UpdatedAt = DateTime.UtcNow,
                IsDeleted = false
            };

            _context.Projects.Add(projectEntity);
            await _context.SaveChangesAsync(); // プロジェクトID確定

            _logger.LogDebug("プロジェクト作成完了: ProjectId={ProjectId}", projectEntity.ProjectId);

            // 3. デフォルトドメイン作成（プロジェクトID必須）
            var domainEntity = new EntityDomain
            {
                DomainName = domain.Name.Value,
                ProjectId = projectEntity.ProjectId, // 自動生成されたIDを使用
                Description = FSharpOption<string>.get_IsSome(domain.Description.Value)
                    ? domain.Description.Value.Value
                    : null,
                UpdatedBy = domain.OwnerId.Item.ToString(),
                UpdatedAt = DateTime.UtcNow,
                IsDeleted = false,
                IsDefault = true  // デフォルトドメインフラグを設定
            };

            _context.Domains.Add(domainEntity);
            await _context.SaveChangesAsync();

            _logger.LogDebug("デフォルトドメイン作成完了: DomainId={DomainId}", domainEntity.DomainId);

            _logger.LogInformation(
                "プロジェクト・デフォルトドメイン同時作成成功（InMemory）: ProjectId={ProjectId}, DomainId={DomainId}",
                projectEntity.ProjectId, domainEntity.DomainId);

            // 4. F# Domain型に変換して返却
            var resultProject = ConvertToFSharpProject(projectEntity);
            var resultDomain = ConvertToFSharpDomain(domainEntity);

            return FSharpResult<Tuple<DomainProject, DomainDomain>, string>.NewOk(
                Tuple.Create(resultProject, resultDomain));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "プロジェクト・デフォルトドメイン作成でエラー発生（InMemory）: ProjectName={ProjectName}",
                project.Name.Value);

            return FSharpResult<Tuple<DomainProject, DomainDomain>, string>.NewError(
                $"作成処理でエラーが発生しました: {ex.Message}");
        }
    }

    /// <summary>
    /// プロジェクト・デフォルトドメイン同時作成（トランザクション使用）
    /// 通常のデータベース（PostgreSQL等）用の実装
    /// </summary>
    private async Task<FSharpResult<Tuple<DomainProject, DomainDomain>, string>>
        CreateProjectWithDefaultDomainWithTransactionAsync(DomainProject project, DomainDomain domain)
    {
        // BeginTransactionを使用した手動トランザクション制御
        // 【Blazor Server初学者向け解説】
        // using宣言により、メソッド終了時に自動的にトランザクションが破棄されます。
        // CommitAsync()が呼ばれない場合は自動的にロールバックされます。
        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            var projectName = project.Name.Value;
            _logger.LogDebug("プロジェクト・デフォルトドメイン同時作成開始: ProjectName={ProjectName}",
                projectName);

            // 1. プロジェクト重複チェック
            var existingProject = await _context.Projects
                .FirstOrDefaultAsync(p => p.ProjectName == projectName);

            if (existingProject != null)
            {
                _logger.LogWarning("プロジェクト名重複: Name={Name}", projectName);
                return FSharpResult<Tuple<DomainProject, DomainDomain>, string>.NewError(
                    $"プロジェクト名'{projectName}'は既に使用されています");
            }

            // 2. プロジェクト作成
            var projectEntity = new EntityProject
            {
                ProjectName = projectName,
                Description = FSharpOption<string>.get_IsSome(project.Description.Value)
                    ? project.Description.Value.Value
                    : null,
                UpdatedBy = project.OwnerId.Item.ToString(),
                UpdatedAt = DateTime.UtcNow,
                IsDeleted = false
            };

            _context.Projects.Add(projectEntity);
            await _context.SaveChangesAsync(); // プロジェクトID確定

            _logger.LogDebug("プロジェクト作成完了: ProjectId={ProjectId}", projectEntity.ProjectId);

            // 3. デフォルトドメイン作成（プロジェクトID必須）
            // 【F#初学者向け解説】
            // 自動生成されたProjectIdを使用してドメインを作成します。
            // これがトランザクションの重要性を示す部分で、プロジェクトIDが確定しないと
            // ドメインの外部キー制約を満たせません。
            var domainEntity = new EntityDomain
            {
                DomainName = domain.Name.Value,
                ProjectId = projectEntity.ProjectId, // 自動生成されたIDを使用
                Description = FSharpOption<string>.get_IsSome(domain.Description.Value)
                    ? domain.Description.Value.Value
                    : null,
                UpdatedBy = domain.OwnerId.Item.ToString(),
                UpdatedAt = DateTime.UtcNow,
                IsDeleted = false,
                IsDefault = true  // デフォルトドメインフラグを設定
            };

            _context.Domains.Add(domainEntity);
            await _context.SaveChangesAsync();

            _logger.LogDebug("デフォルトドメイン作成完了: DomainId={DomainId}", domainEntity.DomainId);

            // 4. トランザクションコミット
            // 【Blazor Server初学者向け解説】
            // CommitAsync()を呼び出すことで、これまでの変更を確定します。
            // この呼び出しがない場合、usingブロック終了時に自動的にロールバックされます。
            await transaction.CommitAsync();

            _logger.LogInformation(
                "プロジェクト・デフォルトドメイン同時作成成功: ProjectId={ProjectId}, DomainId={DomainId}",
                projectEntity.ProjectId, domainEntity.DomainId);

            // 5. F# Domain型に変換して返却
            var resultProject = ConvertToFSharpProject(projectEntity);
            var resultDomain = ConvertToFSharpDomain(domainEntity);

            return FSharpResult<Tuple<DomainProject, DomainDomain>, string>.NewOk(
                Tuple.Create(resultProject, resultDomain));
        }
        catch (Exception ex)
        {
            // エラー発生時は自動ロールバック（using宣言により）
            // 【Blazor Server初学者向け解説】
            // try-catchでエラーをキャッチした時点で、usingブロックを抜けるため
            // トランザクションは自動的にロールバックされます。
            // 明示的にRollbackAsync()を呼ぶ必要はありませんが、ログには記録します。
            _logger.LogError(ex, "プロジェクト・デフォルトドメイン作成でエラー発生: ProjectName={ProjectName}",
                project.Name.Value);

            return FSharpResult<Tuple<DomainProject, DomainDomain>, string>.NewError(
                $"作成処理でエラーが発生しました: {ex.Message}");
        }
    }

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
    public async Task<FSharpResult<FSharpOption<DomainProject>, string>> GetByNameAsync(
        ProjectName projectName)
    {
        try
        {
            var nameStr = projectName.Value;
            _logger.LogDebug("プロジェクト名検索開始: Name={Name}", nameStr);

            var projectEntity = await _context.Projects
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.ProjectName == nameStr);

            if (projectEntity == null)
            {
                _logger.LogDebug("プロジェクト名が見つかりません: Name={Name}", nameStr);
                return FSharpResult<FSharpOption<DomainProject>, string>.NewOk(
                    FSharpOption<DomainProject>.None);
            }

            var project = ConvertToFSharpProject(projectEntity);
            _logger.LogInformation("プロジェクト名検索成功: Name={Name}", nameStr);

            return FSharpResult<FSharpOption<DomainProject>, string>.NewOk(
                FSharpOption<DomainProject>.Some(project));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "プロジェクト名検索でエラー発生: Name={Name}",
                projectName.Value);
            return FSharpResult<FSharpOption<DomainProject>, string>.NewError(
                $"プロジェクト検索に失敗しました: {ex.Message}");
        }
    }

    /// <summary>
    /// ユーザーが所有するプロジェクト一覧取得
    /// UserProjects.UserIdでフィルタリング
    ///
    /// 【EF Core最適化】
    /// - Include(p => p.UserProjects)でEager Loading
    /// - Where句でUserIdフィルタリング
    /// </summary>
    public async Task<FSharpResult<FSharpList<DomainProject>, string>> GetByOwnerAsync(UserId ownerId)
    {
        try
        {
            _logger.LogDebug("所有者別プロジェクト取得開始: OwnerId={OwnerId}", ownerId.Item);

            var projectEntities = await _context.Projects
                .AsNoTracking()
                .Where(p => p.UpdatedBy == ownerId.Item.ToString())
                .OrderBy(p => p.ProjectName)
                .ToListAsync();

            var projects = projectEntities.Select(ConvertToFSharpProject).ToList();

            _logger.LogInformation("所有者別プロジェクト取得成功: OwnerId={OwnerId}, Count={Count}",
                ownerId.Item, projects.Count);

            return FSharpResult<FSharpList<DomainProject>, string>.NewOk(
                ListModule.OfSeq(projects));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "所有者別プロジェクト取得でエラー発生: OwnerId={OwnerId}",
                ownerId.Item);
            return FSharpResult<FSharpList<DomainProject>, string>.NewError(
                $"プロジェクト取得に失敗しました: {ex.Message}");
        }
    }

    // =================================================================
    // 🔄 プライベートヘルパーメソッド：Entity ⇄ Domain変換
    // =================================================================

    /// <summary>
    /// C# Entity → F# Domain型変換（Project）
    ///
    /// 【F#初学者向け解説】
    /// Infrastructure層で取得したC# Entityを、Domain層で使用するF# 型に変換します。
    /// F# の Smart Constructor（create メソッド）を使用して型安全に変換します。
    /// </summary>
    /// <param name="entity">C# Project Entity</param>
    /// <returns>F# Project Domain型</returns>
    private DomainProject ConvertToFSharpProject(EntityProject entity)
    {
        // ProjectId: long → F# ProjectId
        var projectId = ProjectId.NewProjectId(entity.ProjectId);

        // ProjectName: string → F# ProjectName (Smart Constructor)
        var projectNameResult = ProjectName.create(entity.ProjectName);
        if (projectNameResult.IsError)
        {
            throw new InvalidOperationException(
                $"Invalid project name in database: {projectNameResult.ErrorValue}");
        }

        // ProjectDescription: string? → F# ProjectDescription (Option型)
        var descriptionResult = ProjectDescription.create(
            string.IsNullOrWhiteSpace(entity.Description)
                ? FSharpOption<string>.None
                : FSharpOption<string>.Some(entity.Description));
        if (descriptionResult.IsError)
        {
            throw new InvalidOperationException(
                $"Invalid project description in database: {descriptionResult.ErrorValue}");
        }

        // UserId: string → long → F# UserId
        // 【Blazor Server初学者向け解説】
        // データベースでは UpdatedBy が string型（ASP.NET Core Identity互換）ですが、
        // F# Domain層では long型の UserId を使用するため変換が必要です。
        var ownerId = long.TryParse(entity.UpdatedBy, out var ownerIdLong)
            ? UserId.NewUserId(ownerIdLong)
            : UserId.NewUserId(1L); // パース失敗時はデフォルト値

        // ✅ 修正: F# Projectレコード型を直接生成（UpdatedAtを正しく反映）
        // createWithIdメソッドはUpdatedAt=Noneで固定されてしまうため、
        // データベースから取得したUpdatedAtを反映するため、コンストラクタを直接使用
        return new DomainProject(
            projectId,
            projectNameResult.ResultValue,
            descriptionResult.ResultValue,
            ownerId,
            entity.CreatedAt,  // データベースのCreatedAtを使用
            entity.UpdatedAt.HasValue 
                ? FSharpOption<DateTime>.Some(entity.UpdatedAt.Value) 
                : FSharpOption<DateTime>.None,  // データベースのUpdatedAtを使用
            entity.IsActive);
    }

    /// <summary>
    /// C# Entity → F# Domain型変換（Domain）
    ///
    /// 【F#初学者向け解説】
    /// プロジェクトと同様に、ドメインエンティティもF# Domain型に変換します。
    /// IsDefaultフラグを含む完全なドメイン情報をF#レコード型で直接構築します。
    /// </summary>
    /// <param name="entity">C# Domain Entity</param>
    /// <returns>F# Domain Domain型</returns>
    private DomainDomain ConvertToFSharpDomain(EntityDomain entity)
    {
        // DomainId: long → F# DomainId
        var domainId = DomainId.NewDomainId(entity.DomainId);

        // DomainName: string → F# DomainName (Smart Constructor)
        var domainNameResult = DomainName.create(entity.DomainName);
        if (domainNameResult.IsError)
        {
            throw new InvalidOperationException(
                $"Invalid domain name in database: {domainNameResult.ErrorValue}");
        }

        // ProjectDescription: string? → F# ProjectDescription (Option型)
        var descriptionResult = ProjectDescription.create(
            string.IsNullOrWhiteSpace(entity.Description)
                ? FSharpOption<string>.None
                : FSharpOption<string>.Some(entity.Description));
        if (descriptionResult.IsError)
        {
            throw new InvalidOperationException(
                $"Invalid domain description in database: {descriptionResult.ErrorValue}");
        }

        // ProjectId: long → F# ProjectId
        var projectId = ProjectId.NewProjectId(entity.ProjectId);

        // UserId: string → long → F# UserId
        var ownerId = long.TryParse(entity.UpdatedBy, out var ownerIdLong)
            ? UserId.NewUserId(ownerIdLong)
            : UserId.NewUserId(1L);

        // F# Domain レコード型を直接構築（IsDefaultフラグを正しく設定）
        // 【F#初学者向け解説】
        // createWithIdメソッドではIsDefaultが常にfalseになるため、
        // F#レコード型の直接構築構文を使用してEntityの値を正確に反映します。
        return new DomainDomain(
            domainId,
            projectId,
            domainNameResult.ResultValue,
            descriptionResult.ResultValue,
            ownerId,
            entity.IsDefault,                  // IsDefault: Entityから取得
            entity.CreatedAt,
            entity.UpdatedAt.HasValue
                ? FSharpOption<DateTime>.Some(entity.UpdatedAt.Value)
                : FSharpOption<DateTime>.None,  // UpdatedAtをOption型に変換
            entity.IsActive                    // IsActive: Entityから直接取得
        );
    }

    // =================================================================
    // 👥 UserProjects多対多関連管理（Phase B2拡張）
    // =================================================================

    /// <summary>
    /// UserProjectsレコード追加（プロジェクトメンバー追加）
    ///
    /// 【Phase B2: ユーザー・プロジェクト関連管理】
    /// - 複合一意制約違反チェック（UserId + ProjectId）
    /// - CASCADE DELETE設定済み（ユーザー削除・プロジェクト削除時に自動削除）
    /// </summary>
    public async Task<FSharpResult<Unit, string>> AddUserToProjectAsync(
        UserId userId, ProjectId projectId, UserId updatedBy)
    {
        try
        {
            _logger.LogDebug("プロジェクトメンバー追加開始: UserId={UserId}, ProjectId={ProjectId}",
                userId.Item, projectId.Item);

            // 1. 重複チェック（複合一意制約）
            var existingUserProject = await _context.Set<EntityUserProject>()
                .FirstOrDefaultAsync(up => up.UserId == userId.Item.ToString() && up.ProjectId == projectId.Item);

            if (existingUserProject != null)
            {
                _logger.LogWarning("UserProjects重複: UserId={UserId}, ProjectId={ProjectId}",
                    userId.Item, projectId.Item);
                return FSharpResult<Unit, string>.NewError(
                    "このユーザーは既にプロジェクトのメンバーです");
            }

            // 2. UserProjectsレコード作成
            var userProject = new EntityUserProject
            {
                UserId = userId.Item.ToString(),
                ProjectId = projectId.Item,
                UpdatedBy = updatedBy.Item.ToString(),
                UpdatedAt = DateTime.UtcNow
            };

            _context.Set<EntityUserProject>().Add(userProject);
            await _context.SaveChangesAsync();

            _logger.LogInformation("プロジェクトメンバー追加成功: UserProjectId={UserProjectId}, UserId={UserId}, ProjectId={ProjectId}",
                userProject.UserProjectId, userId.Item, projectId.Item);

            return FSharpResult<Unit, string>.NewOk(null!);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "プロジェクトメンバー追加でエラー発生: UserId={UserId}, ProjectId={ProjectId}",
                userId.Item, projectId.Item);
            return FSharpResult<Unit, string>.NewError(
                $"プロジェクトメンバー追加に失敗しました: {ex.Message}");
        }
    }

    /// <summary>
    /// UserProjectsレコード削除（プロジェクトメンバー削除）
    ///
    /// 【Phase B2: ユーザー・プロジェクト関連管理】
    /// - 物理削除（UserProjectsテーブルには論理削除フラグなし）
    /// - 最後の管理者削除防止チェックはApplication層で実施
    /// </summary>
    public async Task<FSharpResult<Unit, string>> RemoveUserFromProjectAsync(
        UserId userId, ProjectId projectId)
    {
        try
        {
            _logger.LogDebug("プロジェクトメンバー削除開始: UserId={UserId}, ProjectId={ProjectId}",
                userId.Item, projectId.Item);

            // UserProjectsレコード取得
            var userProject = await _context.Set<EntityUserProject>()
                .FirstOrDefaultAsync(up => up.UserId == userId.Item.ToString() && up.ProjectId == projectId.Item);

            if (userProject == null)
            {
                _logger.LogWarning("UserProjectsレコードが見つかりません: UserId={UserId}, ProjectId={ProjectId}",
                    userId.Item, projectId.Item);
                return FSharpResult<Unit, string>.NewError(
                    "指定されたユーザーはこのプロジェクトのメンバーではありません");
            }

            // 物理削除
            _context.Set<EntityUserProject>().Remove(userProject);
            await _context.SaveChangesAsync();

            _logger.LogInformation("プロジェクトメンバー削除成功: UserProjectId={UserProjectId}, UserId={UserId}, ProjectId={ProjectId}",
                userProject.UserProjectId, userId.Item, projectId.Item);

            return FSharpResult<Unit, string>.NewOk(null!);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "プロジェクトメンバー削除でエラー発生: UserId={UserId}, ProjectId={ProjectId}",
                userId.Item, projectId.Item);
            return FSharpResult<Unit, string>.NewError(
                $"プロジェクトメンバー削除に失敗しました: {ex.Message}");
        }
    }

    /// <summary>
    /// プロジェクトメンバー一覧取得（UserProjects JOIN）
    ///
    /// 【EF Core最適化】
    /// - AsNoTracking()で読み取り専用最適化
    /// - Eager Loading不要（UserIdのみ取得）
    /// </summary>
    public async Task<FSharpResult<FSharpList<UserId>, string>> GetProjectMembersAsync(ProjectId projectId)
    {
        try
        {
            _logger.LogDebug("プロジェクトメンバー一覧取得開始: ProjectId={ProjectId}", projectId.Item);

            var userIds = await _context.Set<EntityUserProject>()
                .AsNoTracking()
                .Where(up => up.ProjectId == projectId.Item)
                .Select(up => up.UserId)
                .ToListAsync();

            // string → long → F# UserId変換
            var fsharpUserIds = userIds
                .Select(userId =>
                {
                    if (long.TryParse(userId, out var userIdLong))
                    {
                        return UserId.NewUserId(userIdLong);
                    }
                    else
                    {
                        _logger.LogWarning("UserIdのlong変換失敗: UserId={UserId}", userId);
                        return UserId.NewUserId(1L); // デフォルト値
                    }
                })
                .ToList();

            _logger.LogInformation("プロジェクトメンバー一覧取得成功: ProjectId={ProjectId}, Count={Count}",
                projectId.Item, fsharpUserIds.Count);

            return FSharpResult<FSharpList<UserId>, string>.NewOk(
                ListModule.OfSeq(fsharpUserIds));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "プロジェクトメンバー一覧取得でエラー発生: ProjectId={ProjectId}", projectId.Item);
            return FSharpResult<FSharpList<UserId>, string>.NewError(
                $"プロジェクトメンバー一覧取得に失敗しました: {ex.Message}");
        }
    }

    /// <summary>
    /// プロジェクトメンバー判定（UserProjects存在チェック）
    ///
    /// 【EF Core最適化】
    /// - AnyAsync()で効率的な存在チェック（COUNT不要）
    /// </summary>
    public async Task<FSharpResult<bool, string>> IsUserProjectMemberAsync(
        UserId userId, ProjectId projectId)
    {
        try
        {
            _logger.LogDebug("プロジェクトメンバー判定開始: UserId={UserId}, ProjectId={ProjectId}",
                userId.Item, projectId.Item);

            var isMember = await _context.Set<EntityUserProject>()
                .AsNoTracking()
                .AnyAsync(up => up.UserId == userId.Item.ToString() && up.ProjectId == projectId.Item);

            _logger.LogInformation("プロジェクトメンバー判定完了: UserId={UserId}, ProjectId={ProjectId}, IsMember={IsMember}",
                userId.Item, projectId.Item, isMember);

            return FSharpResult<bool, string>.NewOk(isMember);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "プロジェクトメンバー判定でエラー発生: UserId={UserId}, ProjectId={ProjectId}",
                userId.Item, projectId.Item);
            return FSharpResult<bool, string>.NewError(
                $"プロジェクトメンバー判定に失敗しました: {ex.Message}");
        }
    }

    /// <summary>
    /// プロジェクトメンバー数取得（UserProjects COUNT）
    ///
    /// 【EF Core最適化】
    /// - CountAsync()で効率的なCOUNT集計
    /// </summary>
    public async Task<FSharpResult<int, string>> GetProjectMemberCountAsync(ProjectId projectId)
    {
        try
        {
            _logger.LogDebug("プロジェクトメンバー数取得開始: ProjectId={ProjectId}", projectId.Item);

            var count = await _context.Set<EntityUserProject>()
                .AsNoTracking()
                .CountAsync(up => up.ProjectId == projectId.Item);

            _logger.LogInformation("プロジェクトメンバー数取得成功: ProjectId={ProjectId}, Count={Count}",
                projectId.Item, count);

            return FSharpResult<int, string>.NewOk(count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "プロジェクトメンバー数取得でエラー発生: ProjectId={ProjectId}", projectId.Item);
            return FSharpResult<int, string>.NewError(
                $"プロジェクトメンバー数取得に失敗しました: {ex.Message}");
        }
    }

    /// <summary>
    /// プロジェクト作成 + デフォルトドメイン作成 + Owner自動追加（トランザクション保証）
    ///
    /// 【Phase B2: Phase B1トランザクションパターン拡張】
    /// - ProjectsレコードINSERT
    /// - DomainsレコードINSERT（デフォルトドメイン）
    /// - UserProjectsレコードINSERT（Owner自動追加）
    /// - トランザクション境界（同一トランザクション）
    /// </summary>
    public async Task<FSharpResult<Tuple<DomainProject, DomainDomain>, string>>
        SaveProjectWithDefaultDomainAndOwnerAsync(DomainProject project, DomainDomain domain, UserId ownerId)
    {
        // InMemory Database判定（テスト実行環境対応）
        var isInMemory = _context.Database.ProviderName == "Microsoft.EntityFrameworkCore.InMemory";

        if (isInMemory)
        {
            // InMemory Database: トランザクションなしで実行
            return await SaveProjectWithDefaultDomainAndOwnerInMemoryAsync(project, domain, ownerId);
        }
        else
        {
            // 通常のDB: トランザクション使用
            return await SaveProjectWithDefaultDomainAndOwnerWithTransactionAsync(project, domain, ownerId);
        }
    }

    /// <summary>
    /// プロジェクト・デフォルトドメイン・Owner同時作成（InMemory Database用）
    /// </summary>
    private async Task<FSharpResult<Tuple<DomainProject, DomainDomain>, string>>
        SaveProjectWithDefaultDomainAndOwnerInMemoryAsync(DomainProject project, DomainDomain domain, UserId ownerId)
    {
        try
        {
            var projectName = project.Name.Value;
            _logger.LogDebug("プロジェクト・デフォルトドメイン・Owner同時作成開始（InMemory）: ProjectName={ProjectName}",
                projectName);

            // 1. プロジェクト重複チェック
            var existingProject = await _context.Projects
                .FirstOrDefaultAsync(p => p.ProjectName == projectName);

            if (existingProject != null)
            {
                _logger.LogWarning("プロジェクト名重複: Name={Name}", projectName);
                return FSharpResult<Tuple<DomainProject, DomainDomain>, string>.NewError(
                    $"プロジェクト名'{projectName}'は既に使用されています");
            }

            // 2. プロジェクト作成
            var projectEntity = new EntityProject
            {
                ProjectName = projectName,
                Description = FSharpOption<string>.get_IsSome(project.Description.Value)
                    ? project.Description.Value.Value
                    : null,
                UpdatedBy = ownerId.Item.ToString(),
                UpdatedAt = DateTime.UtcNow,
                IsDeleted = false
            };

            _context.Projects.Add(projectEntity);
            await _context.SaveChangesAsync(); // プロジェクトID確定

            _logger.LogDebug("プロジェクト作成完了: ProjectId={ProjectId}", projectEntity.ProjectId);

            // 3. デフォルトドメイン作成
            var domainEntity = new EntityDomain
            {
                DomainName = domain.Name.Value,
                ProjectId = projectEntity.ProjectId,
                Description = FSharpOption<string>.get_IsSome(domain.Description.Value)
                    ? domain.Description.Value.Value
                    : null,
                UpdatedBy = ownerId.Item.ToString(),
                UpdatedAt = DateTime.UtcNow,
                IsDeleted = false,
                IsDefault = true
            };

            _context.Domains.Add(domainEntity);
            await _context.SaveChangesAsync();

            _logger.LogDebug("デフォルトドメイン作成完了: DomainId={DomainId}", domainEntity.DomainId);

            // 4. UserProjects作成（Owner追加）
            var userProject = new EntityUserProject
            {
                UserId = ownerId.Item.ToString(),
                ProjectId = projectEntity.ProjectId,
                UpdatedBy = ownerId.Item.ToString(),
                UpdatedAt = DateTime.UtcNow
            };

            _context.Set<EntityUserProject>().Add(userProject);
            await _context.SaveChangesAsync();

            _logger.LogDebug("UserProjects作成完了: UserProjectId={UserProjectId}", userProject.UserProjectId);

            _logger.LogInformation(
                "プロジェクト・デフォルトドメイン・Owner同時作成成功（InMemory）: ProjectId={ProjectId}, DomainId={DomainId}, UserProjectId={UserProjectId}",
                projectEntity.ProjectId, domainEntity.DomainId, userProject.UserProjectId);

            // 5. F# Domain型に変換して返却
            var resultProject = ConvertToFSharpProject(projectEntity);
            var resultDomain = ConvertToFSharpDomain(domainEntity);

            return FSharpResult<Tuple<DomainProject, DomainDomain>, string>.NewOk(
                Tuple.Create(resultProject, resultDomain));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "プロジェクト・デフォルトドメイン・Owner作成でエラー発生（InMemory）: ProjectName={ProjectName}",
                project.Name.Value);

            return FSharpResult<Tuple<DomainProject, DomainDomain>, string>.NewError(
                $"作成処理でエラーが発生しました: {ex.Message}");
        }
    }

    /// <summary>
    /// プロジェクト・デフォルトドメイン・Owner同時作成（トランザクション使用）
    /// </summary>
    private async Task<FSharpResult<Tuple<DomainProject, DomainDomain>, string>>
        SaveProjectWithDefaultDomainAndOwnerWithTransactionAsync(DomainProject project, DomainDomain domain, UserId ownerId)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            var projectName = project.Name.Value;
            _logger.LogDebug("プロジェクト・デフォルトドメイン・Owner同時作成開始: ProjectName={ProjectName}",
                projectName);

            // 1. プロジェクト重複チェック
            var existingProject = await _context.Projects
                .FirstOrDefaultAsync(p => p.ProjectName == projectName);

            if (existingProject != null)
            {
                _logger.LogWarning("プロジェクト名重複: Name={Name}", projectName);
                return FSharpResult<Tuple<DomainProject, DomainDomain>, string>.NewError(
                    $"プロジェクト名'{projectName}'は既に使用されています");
            }

            // 2. プロジェクト作成
            var projectEntity = new EntityProject
            {
                ProjectName = projectName,
                Description = FSharpOption<string>.get_IsSome(project.Description.Value)
                    ? project.Description.Value.Value
                    : null,
                UpdatedBy = ownerId.Item.ToString(),
                UpdatedAt = DateTime.UtcNow,
                IsDeleted = false
            };

            _context.Projects.Add(projectEntity);
            await _context.SaveChangesAsync(); // プロジェクトID確定

            _logger.LogDebug("プロジェクト作成完了: ProjectId={ProjectId}", projectEntity.ProjectId);

            // 3. デフォルトドメイン作成
            var domainEntity = new EntityDomain
            {
                DomainName = domain.Name.Value,
                ProjectId = projectEntity.ProjectId,
                Description = FSharpOption<string>.get_IsSome(domain.Description.Value)
                    ? domain.Description.Value.Value
                    : null,
                UpdatedBy = ownerId.Item.ToString(),
                UpdatedAt = DateTime.UtcNow,
                IsDeleted = false,
                IsDefault = true
            };

            _context.Domains.Add(domainEntity);
            await _context.SaveChangesAsync();

            _logger.LogDebug("デフォルトドメイン作成完了: DomainId={DomainId}", domainEntity.DomainId);

            // 4. UserProjects作成（Owner追加）
            var userProject = new EntityUserProject
            {
                UserId = ownerId.Item.ToString(),
                ProjectId = projectEntity.ProjectId,
                UpdatedBy = ownerId.Item.ToString(),
                UpdatedAt = DateTime.UtcNow
            };

            _context.Set<EntityUserProject>().Add(userProject);
            await _context.SaveChangesAsync();

            _logger.LogDebug("UserProjects作成完了: UserProjectId={UserProjectId}", userProject.UserProjectId);

            // 5. トランザクションコミット
            await transaction.CommitAsync();

            _logger.LogInformation(
                "プロジェクト・デフォルトドメイン・Owner同時作成成功: ProjectId={ProjectId}, DomainId={DomainId}, UserProjectId={UserProjectId}",
                projectEntity.ProjectId, domainEntity.DomainId, userProject.UserProjectId);

            // 6. F# Domain型に変換して返却
            var resultProject = ConvertToFSharpProject(projectEntity);
            var resultDomain = ConvertToFSharpDomain(domainEntity);

            return FSharpResult<Tuple<DomainProject, DomainDomain>, string>.NewOk(
                Tuple.Create(resultProject, resultDomain));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "プロジェクト・デフォルトドメイン・Owner作成でエラー発生: ProjectName={ProjectName}",
                project.Name.Value);

            return FSharpResult<Tuple<DomainProject, DomainDomain>, string>.NewError(
                $"作成処理でエラーが発生しました: {ex.Message}");
        }
    }

    /// <summary>
    /// プロジェクト関連データ件数取得（削除確認画面用）
    ///
    /// 【Phase B2拡張】
    /// - UserProjectsカウント追加
    /// </summary>
    public async Task<FSharpResult<Tuple<int, int, int>, string>> GetRelatedDataCountAsync(ProjectId projectId)
    {
        try
        {
            _logger.LogDebug("プロジェクト関連データ件数取得開始: ProjectId={ProjectId}", projectId.Item);

            // 並列実行でパフォーマンス最適化
            var domainCountTask = _context.Domains
                .AsNoTracking()
                .CountAsync(d => d.ProjectId == projectId.Item);

            // ユビキタス言語カウント（FormalとDraftを合算）
            var formalLanguageCountTask = _context.FormalUbiquitousLanguages
                .AsNoTracking()
                .Join(_context.Domains, ful => ful.DomainId, d => d.DomainId, (ful, d) => d)
                .CountAsync(d => d.ProjectId == projectId.Item);

            var draftLanguageCountTask = _context.DraftUbiquitousLanguages
                .AsNoTracking()
                .Join(_context.Domains, dul => dul.DomainId, d => d.DomainId, (dul, d) => d)
                .CountAsync(d => d.ProjectId == projectId.Item);

            // 【Phase B2拡張】UserProjectsカウント追加（メンバー数取得）
            var memberCountTask = _context.Set<EntityUserProject>()
                .AsNoTracking()
                .CountAsync(up => up.ProjectId == projectId.Item);

            await Task.WhenAll(domainCountTask, formalLanguageCountTask, draftLanguageCountTask, memberCountTask);

            var domainCount = await domainCountTask;
            var formalLanguageCount = await formalLanguageCountTask;
            var draftLanguageCount = await draftLanguageCountTask;
            var languageCount = formalLanguageCount + draftLanguageCount;
            var memberCount = await memberCountTask;

            _logger.LogInformation(
                "プロジェクト関連データ件数取得成功: ProjectId={ProjectId}, Domains={Domains}, Languages={Languages}, Members={Members}",
                projectId.Item, domainCount, languageCount, memberCount);

            return FSharpResult<Tuple<int, int, int>, string>.NewOk(
                Tuple.Create(domainCount, languageCount, memberCount));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "プロジェクト関連データ件数取得でエラー発生: ProjectId={ProjectId}", projectId.Item);
            return FSharpResult<Tuple<int, int, int>, string>.NewError(
                $"関連データ件数取得に失敗しました: {ex.Message}");
        }
    }

    // =================================================================
    // 🔄 プライベートヘルパーメソッド：Role判別共用体変換
    // =================================================================

    /// <summary>
    /// F# Role判別共用体を文字列に変換（ログ出力用）
    /// </summary>
    private string RoleToString(Role role)
    {
        if (role.IsSuperUser) return "SuperUser";
        if (role.IsProjectManager) return "ProjectManager";
        if (role.IsDomainApprover) return "DomainApprover";
        if (role.IsGeneralUser) return "GeneralUser";
        return "Unknown";
    }

    // =================================================================
    // 🔌 Application層IProjectRepository明示的インターフェース実装
    // =================================================================

    /// <summary>
    /// Application層IProjectRepository.GetByIdAsync明示的実装
    /// Infrastructure層の既存メソッドへ委譲
    /// </summary>
    async Task<FSharpResult<FSharpOption<DomainProject>, string>> Application.IProjectRepository.GetByIdAsync(ProjectId projectId)
        => await this.GetByIdAsync(projectId);

    /// <summary>
    /// Application層IProjectRepository.GetActiveProjectsAsync明示的実装
    /// Infrastructure層のGetAllAsync()へ委譲（論理削除フィルター適用済み）
    /// </summary>
    async Task<FSharpResult<FSharpList<DomainProject>, string>> Application.IProjectRepository.GetActiveProjectsAsync()
        => await this.GetAllAsync();

    /// <summary>
    /// Application層IProjectRepository.SaveAsync明示的実装
    /// Infrastructure層のCreateAsync()へ委譲
    ///
    /// 【設計判断】
    /// Application層のSaveAsyncは「新規作成・更新の両方に対応」する仕様ですが、
    /// 現在のInfrastructure層はCreateAsync/UpdateAsyncを分離しています。
    /// ここではCreateAsyncに委譲し、更新処理は別途対応する方針とします。
    /// </summary>
    async Task<FSharpResult<DomainProject, string>> Application.IProjectRepository.SaveAsync(DomainProject project)
        => await this.CreateAsync(project);

    /// <summary>
    /// Application層IProjectRepository.DeleteAsync明示的実装
    /// Infrastructure層の既存メソッドへ委譲
    /// </summary>
    async Task<FSharpResult<Unit, string>> Application.IProjectRepository.DeleteAsync(ProjectId projectId)
        => await this.DeleteAsync(projectId);

    // =================================================================
    // 🎯 Application.ProjectManagement.IProjectRepository 明示的実装
    // ProjectManagementService (F#) からの依存解決用（Phase B-F3 Step1.5）
    // =================================================================

    /// <summary>
    /// SaveAsync: 既存のCreateAsyncに委譲
    /// 【設計判断】
    /// Application層のSaveAsyncは「新規作成・更新の両方に対応」する仕様ですが、
    /// 現在のInfrastructure層はCreateAsync/UpdateAsyncを分離しています。
    /// ここではCreateAsyncに委譲し、更新処理は別途対応する方針とします。
    /// </summary>
    async Task<FSharpResult<DomainProject, string>> Application.ProjectManagement.IProjectRepository.SaveAsync(DomainProject project)
        => await this.CreateAsync(project);

    /// <summary>
    /// SaveProjectWithDefaultDomainAsync: 既存メソッドに委譲
    /// </summary>
    async Task<FSharpResult<Tuple<DomainProject, DomainDomain>, string>> Application.ProjectManagement.IProjectRepository.SaveProjectWithDefaultDomainAsync(DomainProject project, DomainDomain defaultDomain)
        => await this.CreateProjectWithDefaultDomainAsync(project, defaultDomain);

    /// <summary>
    /// GetByIdAsync: 既存メソッドに委譲（Option版）
    /// </summary>
    async Task<FSharpResult<FSharpOption<DomainProject>, string>> Application.ProjectManagement.IProjectRepository.GetByIdAsync(ProjectId id)
        => await this.GetByIdAsync(id);

    /// <summary>
    /// GetByOwnerAsync: 既存メソッドに委譲
    /// </summary>
    async Task<FSharpResult<FSharpList<DomainProject>, string>> Application.ProjectManagement.IProjectRepository.GetByOwnerAsync(UserId ownerId)
        => await this.GetByOwnerAsync(ownerId);

    /// <summary>
    /// GetProjectsWithPermissionAsync: 簡易実装
    /// 【TODO Phase B-F3 Stage2】
    /// 本格的なページング・検索実装は後続Stageで対応
    /// 現時点では既存GetProjectsByUserAsyncを活用した簡易実装
    /// </summary>
    async Task<FSharpResult<ProjectListResultDto, string>> Application.ProjectManagement.IProjectRepository.GetProjectsWithPermissionAsync(
        UserId userId, Role userRole, int pageNumber, int pageSize, bool includeInactive)
    {
        try
        {
            _logger.LogDebug("GetProjectsWithPermissionAsync開始: UserId={UserId}, Role={Role}, Page={PageNumber}, Size={PageSize}",
                userId.Item, RoleToString(userRole), pageNumber, pageSize);

            // 既存GetProjectsByUserAsyncを活用（権限フィルタリング済み）
            var projectsResult = await this.GetProjectsByUserAsync(userId, userRole);

            if (projectsResult.IsError)
            {
                return FSharpResult<ProjectListResultDto, string>.NewError(projectsResult.ErrorValue);
            }

            // F# list → C# List変換
            var projects = ListModule.ToArray(projectsResult.ResultValue).ToList();

            // 簡易ページング処理
            var totalCount = projects.Count;
            var pagedProjects = projects
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            // F# レコード型の構築（名前付き引数不可のため、位置指定で構築）
            var resultDto = new ProjectListResultDto(
                ListModule.OfSeq(pagedProjects),  // Projects: Project list
                totalCount,                       // TotalCount: int
                pageNumber,                       // PageNumber: int
                pageSize,                         // PageSize: int
                (pageNumber * pageSize) < totalCount,  // HasNextPage: bool
                pageNumber > 1                    // HasPreviousPage: bool
            );

            _logger.LogInformation("GetProjectsWithPermissionAsync成功: TotalCount={TotalCount}, PagedCount={PagedCount}",
                totalCount, pagedProjects.Count);

            return FSharpResult<ProjectListResultDto, string>.NewOk(resultDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetProjectsWithPermissionAsyncでエラー発生");
            return FSharpResult<ProjectListResultDto, string>.NewError($"プロジェクト一覧取得に失敗しました: {ex.Message}");
        }
    }

    /// <summary>
    /// GetAllActiveAsync: 既存GetAllAsyncに委譲
    /// </summary>
    async Task<FSharpResult<FSharpList<DomainProject>, string>> Application.ProjectManagement.IProjectRepository.GetAllActiveAsync()
        => await this.GetAllAsync();

    /// <summary>
    /// GetRelatedDataCountAsync: 既存メソッド活用
    /// 【Phase B2拡張版】
    /// GetRelatedDataCountAsync(ProjectId)は(Domains, Languages, Members)のTuple3を返すため、
    /// 単純なint合計値に変換して返却
    /// </summary>
    async Task<FSharpResult<int, string>> Application.ProjectManagement.IProjectRepository.GetRelatedDataCountAsync(ProjectId projectId)
    {
        try
        {
            var result = await this.GetRelatedDataCountAsync(projectId);

            if (result.IsError)
            {
                return FSharpResult<int, string>.NewError(result.ErrorValue);
            }

            var (domainCount, languageCount, memberCount) = result.ResultValue;
            var totalCount = domainCount + languageCount + memberCount;

            return FSharpResult<int, string>.NewOk(totalCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetRelatedDataCountAsyncでエラー発生: ProjectId={ProjectId}", projectId.Item);
            return FSharpResult<int, string>.NewError($"関連データ件数取得に失敗しました: {ex.Message}");
        }
    }

    /// <summary>
    /// SearchProjectsAsync: 未実装（Phase B-F3 Stage2以降で対応）
    /// 【TODO Phase B-F3 Stage2】
    /// 高度な検索機能は後続Stageで実装予定
    /// </summary>
    Task<FSharpResult<ProjectListResultDto, string>> Application.ProjectManagement.IProjectRepository.SearchProjectsAsync(SearchProjectsQuery searchQuery)
    {
        _logger.LogWarning("SearchProjectsAsync未実装: Phase B-F3 Stage2以降で対応予定");
        return Task.FromResult(FSharpResult<ProjectListResultDto, string>.NewError("SearchProjectsAsync機能は未実装です"));
    }

    /// <summary>
    /// AddUserToProjectAsync: 既存メソッドに委譲
    /// </summary>
    async Task<FSharpResult<Unit, string>> Application.ProjectManagement.IProjectRepository.AddUserToProjectAsync(UserId userId, ProjectId projectId, UserId updatedBy)
        => await this.AddUserToProjectAsync(userId, projectId, updatedBy);

    /// <summary>
    /// RemoveUserFromProjectAsync: 既存メソッドに委譲
    /// </summary>
    async Task<FSharpResult<Unit, string>> Application.ProjectManagement.IProjectRepository.RemoveUserFromProjectAsync(UserId userId, ProjectId projectId)
        => await this.RemoveUserFromProjectAsync(userId, projectId);

    /// <summary>
    /// GetProjectMembersAsync: 既存メソッドに委譲
    /// </summary>
    async Task<FSharpResult<FSharpList<UserId>, string>> Application.ProjectManagement.IProjectRepository.GetProjectMembersAsync(ProjectId projectId)
        => await this.GetProjectMembersAsync(projectId);

    /// <summary>
    /// IsUserProjectMemberAsync: 既存メソッドに委譲
    /// </summary>
    async Task<FSharpResult<bool, string>> Application.ProjectManagement.IProjectRepository.IsUserProjectMemberAsync(UserId userId, ProjectId projectId)
        => await this.IsUserProjectMemberAsync(userId, projectId);

    /// <summary>
    /// GetProjectMemberCountAsync: 既存メソッドに委譲
    /// </summary>
    async Task<FSharpResult<int, string>> Application.ProjectManagement.IProjectRepository.GetProjectMemberCountAsync(ProjectId projectId)
        => await this.GetProjectMemberCountAsync(projectId);

    /// <summary>
    /// SaveProjectWithDefaultDomainAndOwnerAsync: 既存メソッドに委譲
    /// </summary>
    async Task<FSharpResult<Tuple<DomainProject, DomainDomain>, string>> Application.ProjectManagement.IProjectRepository.SaveProjectWithDefaultDomainAndOwnerAsync(DomainProject project, DomainDomain defaultDomain, UserId ownerId)
        => await this.SaveProjectWithDefaultDomainAndOwnerAsync(project, defaultDomain, ownerId);
}
