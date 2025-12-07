using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.FSharp.Core;
using Microsoft.FSharp.Collections;

// F# Domain層の型を参照（ADR_019準拠）
using UbiquitousLanguageManager.Domain.Common;
using UbiquitousLanguageManager.Domain.ProjectManagement;
using DomainDomain = UbiquitousLanguageManager.Domain.ProjectManagement.Domain;

// F# Application層の型参照（ILogger競合対策のため型のみエイリアス使用）
using IDomainRepository = UbiquitousLanguageManager.Application.IDomainRepository;

// Application.ProjectManagement層のインターフェース参照（Phase B-F3）
using PmIDomainRepository = UbiquitousLanguageManager.Application.ProjectManagement.IDomainRepository;

// Infrastructure層の参照
using UbiquitousLanguageManager.Infrastructure.Data;
using EntityDomain = UbiquitousLanguageManager.Infrastructure.Data.Entities.Domain;

namespace UbiquitousLanguageManager.Infrastructure.Repositories;

/// <summary>
/// ドメインリポジトリの実装
/// F# Domain層のDomainエンティティに対するデータアクセス操作を実装
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
///
/// 【インターフェース実装】
/// - Application.IDomainRepository: 4メソッド（既存）
/// - Application.ProjectManagement.IDomainRepository: 2メソッド（Phase B-F3追加）
///
/// Phase B-F3: ドメイン管理機能の基盤実装
/// </summary>
public class DomainRepository :
    IDomainRepository, // Application.IDomainRepository（既存・4メソッド）
    PmIDomainRepository // Application.ProjectManagement.IDomainRepository（追加・2メソッド）
{
    private readonly UbiquitousLanguageDbContext _context;
    private readonly ILogger<DomainRepository> _logger;

    /// <summary>
    /// DomainRepositoryのコンストラクタ
    /// DIコンテナからDbContextとLoggerを注入
    ///
    /// 【Blazor Server初学者向け解説】
    /// ASP.NET CoreのDI（Dependency Injection）により、
    /// DbContextとLoggerが自動的に注入されます。
    /// これによりテスト時にモックを注入しやすく、疎結合な設計を実現します。
    /// </summary>
    /// <param name="context">Entity Framework Core DbContext</param>
    /// <param name="logger">ASP.NET Core標準のLogger</param>
    public DomainRepository(
        UbiquitousLanguageDbContext context,
        ILogger<DomainRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    // =================================================================
    // 🔍 基本CRUD操作
    // =================================================================

    /// <summary>
    /// ドメインIDによる単一ドメイン取得
    ///
    /// 【EF Core最適化ポイント】
    /// - AsNoTracking(): 読み取り専用クエリで40-60%性能向上
    /// - FirstOrDefaultAsync(): 単一レコード取得に最適化
    /// </summary>
    public async Task<FSharpResult<FSharpOption<DomainDomain>, string>> GetByIdAsync(DomainId domainId)
    {
        try
        {
            _logger.LogDebug("ドメイン取得開始: DomainId={DomainId}", domainId.Value);

            // 【F#初学者向け解説】
            // DomainId は F# の判別共用体型で、.Value プロパティで内部の long 値にアクセスします。
            var domainEntity = await _context.Domains
                .AsNoTracking()  // 読み取り専用クエリで性能向上
                .FirstOrDefaultAsync(d => d.DomainId == domainId.Value);

            if (domainEntity == null)
            {
                _logger.LogDebug("ドメインが見つかりません: DomainId={DomainId}", domainId.Value);
                return FSharpResult<FSharpOption<DomainDomain>, string>.NewOk(
                    FSharpOption<DomainDomain>.None);
            }

            // C# Entity → F# Domain型変換
            var domain = ConvertToFSharpDomain(domainEntity);
            _logger.LogInformation("ドメイン取得成功: DomainId={DomainId}, Name={Name}",
                domainId.Value, domain.Name.Value);

            return FSharpResult<FSharpOption<DomainDomain>, string>.NewOk(
                FSharpOption<DomainDomain>.Some(domain));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "ドメイン取得でエラー発生: DomainId={DomainId}", domainId.Value);
            return FSharpResult<FSharpOption<DomainDomain>, string>.NewError(
                $"ドメイン取得に失敗しました: {ex.Message}");
        }
    }

    /// <summary>
    /// プロジェクトIDによるドメイン一覧取得
    /// プロジェクトに属するアクティブなドメインのみを取得
    ///
    /// 【EF Core最適化ポイント】
    /// - HasQueryFilter(): DbContextでIsDeleted=falseのグローバルフィルター適用済み
    /// - OrderBy(): DomainNameでソートして一貫した順序を保証
    /// </summary>
    public async Task<FSharpResult<FSharpList<DomainDomain>, string>> GetByProjectIdAsync(ProjectId projectId)
    {
        try
        {
            _logger.LogDebug("プロジェクト別ドメイン一覧取得開始: ProjectId={ProjectId}", projectId.Value);

            var domainEntities = await _context.Domains
                .AsNoTracking()
                .Where(d => d.ProjectId == projectId.Value)
                .OrderBy(d => d.DomainName)  // ドメイン名順でソート
                .ToListAsync();

            var domains = domainEntities.Select(ConvertToFSharpDomain).ToList();

            _logger.LogInformation("プロジェクト別ドメイン一覧取得成功: ProjectId={ProjectId}, Count={Count}",
                projectId.Value, domains.Count);

            return FSharpResult<FSharpList<DomainDomain>, string>.NewOk(
                ListModule.OfSeq(domains));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "プロジェクト別ドメイン一覧取得でエラー発生: ProjectId={ProjectId}",
                projectId.Value);
            return FSharpResult<FSharpList<DomainDomain>, string>.NewError(
                $"ドメイン一覧取得に失敗しました: {ex.Message}");
        }
    }

    /// <summary>
    /// ドメイン保存（作成・更新の両方に対応）
    ///
    /// 【F#との統合ポイント】
    /// - F# Domainモデル（不変オブジェクト）をC# Entityに変換
    /// - SaveChangesAsync()でデータベースに永続化
    /// - 自動生成されたIDを含むF# Domainを返却
    ///
    /// 【作成・更新の判断】
    /// - DomainId.Value == 0L の場合: 新規作成
    /// - DomainId.Value &gt; 0L の場合: 更新
    /// </summary>
    public async Task<FSharpResult<DomainDomain, string>> SaveAsync(DomainDomain domain)
    {
        try
        {
            var domainName = domain.Name.Value;
            var isNewDomain = domain.Id.Value == 0L;

            if (isNewDomain)
            {
                // 新規作成
                _logger.LogDebug("ドメイン作成開始: Name={Name}, ProjectId={ProjectId}",
                    domainName, domain.ProjectId.Value);

                // 1. 重複チェック（プロジェクト内でのドメイン名一意性）
                var existingDomain = await _context.Domains
                    .FirstOrDefaultAsync(d =>
                        d.ProjectId == domain.ProjectId.Value &&
                        d.DomainName == domainName);

                if (existingDomain != null)
                {
                    _logger.LogWarning("ドメイン名重複: Name={Name}, ProjectId={ProjectId}",
                        domainName, domain.ProjectId.Value);
                    return FSharpResult<DomainDomain, string>.NewError(
                        $"ドメイン名'{domainName}'は既に使用されています");
                }

                // 2. F# Domain型 → C# Entity変換
                var domainEntity = new EntityDomain
                {
                    DomainName = domainName,
                    ProjectId = domain.ProjectId.Value,
                    Description = FSharpOption<string>.get_IsSome(domain.Description.Value)
                        ? domain.Description.Value.Value
                        : null,
                    OwnerId = long.Parse(domain.OwnerId.Value),  // string → long変換（DB Entity型はlong）
                    IsDefault = domain.IsDefault,    // Phase B1で追加されたIsDefaultフラグ
                    CreatedAt = domain.CreatedAt,    // Phase B1で追加されたCreatedAt
                    UpdatedBy = domain.OwnerId.Value,  // stringのまま使用（DB UpdatedBy型はstring）
                    UpdatedAt = DateTime.UtcNow,
                    IsActive = domain.IsActive,      // Phase B1で追加されたIsActive
                    IsDeleted = false
                };

                // 3. 保存
                _context.Domains.Add(domainEntity);
                await _context.SaveChangesAsync();

                _logger.LogInformation("ドメイン作成成功: DomainId={DomainId}, Name={Name}",
                    domainEntity.DomainId, domainEntity.DomainName);

                // 4. C# Entity → F# Domain型変換（自動生成されたIDを含む）
                var resultDomain = ConvertToFSharpDomain(domainEntity);
                return FSharpResult<DomainDomain, string>.NewOk(resultDomain);
            }
            else
            {
                // 更新
                _logger.LogDebug("ドメイン更新開始: DomainId={DomainId}", domain.Id.Value);

                var domainEntity = await _context.Domains
                    .FirstOrDefaultAsync(d => d.DomainId == domain.Id.Value);

                if (domainEntity == null)
                {
                    _logger.LogWarning("ドメインが見つかりません: DomainId={DomainId}", domain.Id.Value);
                    return FSharpResult<DomainDomain, string>.NewError(
                        $"ドメインID'{domain.Id.Value}'が見つかりません");
                }

                // 楽観的ロック（UpdatedAtによる競合検出）
                // 【Blazor Server初学者向け解説】
                // 楽観的ロックは、更新時に元の更新日時と現在のデータベースの更新日時を比較し、
                // 異なる場合は他のユーザーが更新したと判断してエラーとします。
                if (FSharpOption<DateTime>.get_IsSome(domain.UpdatedAt))
                {
                    var expectedUpdatedAt = domain.UpdatedAt.Value;
                    if (domainEntity.UpdatedAt != expectedUpdatedAt)
                    {
                        _logger.LogWarning(
                            "ドメイン更新競合: DomainId={DomainId}, Expected={Expected}, Actual={Actual}",
                            domain.Id.Value, expectedUpdatedAt, domainEntity.UpdatedAt);
                        return FSharpResult<DomainDomain, string>.NewError(
                            "ドメインが他のユーザーによって更新されています（競合）。再度取得してください。");
                    }
                }

                // 更新（名前・説明・IsActiveのみ変更可能）
                domainEntity.DomainName = domainName;
                domainEntity.Description = FSharpOption<string>.get_IsSome(domain.Description.Value)
                    ? domain.Description.Value.Value
                    : null;
                domainEntity.IsActive = domain.IsActive;
                domainEntity.UpdatedAt = DateTime.UtcNow;
                domainEntity.UpdatedBy = domain.OwnerId.Value.ToString();

                await _context.SaveChangesAsync();

                _logger.LogInformation("ドメイン更新成功: DomainId={DomainId}", domain.Id.Value);

                var resultDomain = ConvertToFSharpDomain(domainEntity);
                return FSharpResult<DomainDomain, string>.NewOk(resultDomain);
            }
        }
        catch (DbUpdateConcurrencyException ex)
        {
            _logger.LogError(ex, "ドメイン保存で競合発生: DomainId={DomainId}", domain.Id.Value);
            return FSharpResult<DomainDomain, string>.NewError(
                "ドメインが他のユーザーによって更新されています（競合）。再度取得してください。");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "ドメイン保存でエラー発生: DomainId={DomainId}", domain.Id.Value);
            return FSharpResult<DomainDomain, string>.NewError(
                $"ドメイン保存に失敗しました: {ex.Message}");
        }
    }

    /// <summary>
    /// ドメイン削除（論理削除）
    ///
    /// 【論理削除の理由】
    /// - ドメインに紐づくユビキタス言語の履歴保持
    /// - データベース設計書準拠
    /// - IsActive=false, IsDeleted=trueに設定、物理削除は行わない
    ///
    /// 【F#初学者向け解説】
    /// F# の Unit 型は「返り値なし」を表す型です。C# の void に相当しますが、
    /// Result型では値が必要なため null! を使用します。
    /// </summary>
    public async Task<FSharpResult<Unit, string>> DeleteAsync(DomainId domainId)
    {
        try
        {
            _logger.LogDebug("ドメイン削除開始: DomainId={DomainId}", domainId.Value);

            var domainEntity = await _context.Domains
                .IgnoreQueryFilters()  // 論理削除フィルターを無効化
                .FirstOrDefaultAsync(d => d.DomainId == domainId.Value);

            if (domainEntity == null)
            {
                _logger.LogWarning("ドメインが見つかりません: DomainId={DomainId}", domainId.Value);
                return FSharpResult<Unit, string>.NewError(
                    $"ドメインID'{domainId.Value}'が見つかりません");
            }

            // 論理削除
            domainEntity.IsActive = false;  // Phase B1で追加されたIsActive
            domainEntity.IsDeleted = true;
            domainEntity.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation("ドメイン削除成功: DomainId={DomainId}", domainId.Value);

            return FSharpResult<Unit, string>.NewOk(null!);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "ドメイン削除でエラー発生: DomainId={DomainId}", domainId.Value);
            return FSharpResult<Unit, string>.NewError(
                $"ドメイン削除に失敗しました: {ex.Message}");
        }
    }

    // =================================================================
    // 🔄 プライベートヘルパーメソッド：Entity ⇄ Domain変換
    // =================================================================

    /// <summary>
    /// C# Entity → F# Domain型変換（Domain）
    ///
    /// 【F#初学者向け解説】
    /// Infrastructure層で取得したC# Entityを、Domain層で使用するF# 型に変換します。
    /// F# の Smart Constructor（create メソッド）を使用して型安全に変換します。
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

        // UserId: long → F# UserId（string型に変更）
        var ownerId = UserId.create(entity.OwnerId.ToString());

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
    // 🎯 Application.ProjectManagement.IDomainRepository 明示的実装
    // ProjectManagementService (F#) からの依存解決用（Phase B-F3）
    // =================================================================

    /// <summary>
    /// ドメイン保存（ProjectManagement用）
    /// 既存のSaveAsyncに委譲
    ///
    /// 【F#初学者向け解説】
    /// 明示的インターフェース実装により、Application.ProjectManagement.IDomainRepositoryの
    /// SaveAsyncメソッドを実装します。これにより、ProjectManagementServiceが
    /// このRepositoryを依存性注入で使用可能になります。
    ///
    /// 既存のSaveAsyncメソッドと同一のシグネチャを持つため、
    /// 実装を委譲することでコード重複を避けます。
    /// </summary>
    Task<FSharpResult<DomainDomain, string>> PmIDomainRepository.SaveAsync(DomainDomain domain)
        => SaveAsync(domain);

    /// <summary>
    /// プロジェクト別ドメイン一覧取得（ProjectManagement用）
    /// 既存のGetByProjectIdAsyncに委譲
    ///
    /// 【F#初学者向け解説】
    /// 明示的インターフェース実装により、Application.ProjectManagement.IDomainRepositoryの
    /// GetByProjectIdAsyncメソッドを実装します。
    ///
    /// 既存のGetByProjectIdAsyncメソッドと同一のシグネチャを持つため、
    /// 実装を委譲することでコード重複を避けます。
    /// </summary>
    Task<FSharpResult<FSharpList<DomainDomain>, string>> PmIDomainRepository.GetByProjectIdAsync(ProjectId projectId)
        => GetByProjectIdAsync(projectId);
}
