using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.FSharp.Core;
using UbiquitousLanguageManager.Contracts.DTOs;
using UbiquitousLanguageManager.Contracts.DTOs.Application;
using UbiquitousLanguageManager.Domain.ProjectManagement;
// UbiquitousLanguageManager.Domain.ProjectManagement.Domain型は使用箇所で直接指定
// FSharpResult型は使用箇所で直接指定

namespace UbiquitousLanguageManager.Contracts.Converters;

/// <summary>
/// Application層Result型変換専用クラス
/// F# Application層のResult型、Task&lt;Result&lt;T, TError&gt;&gt;パターンとC# DTOの変換
///
/// 【F#初学者向け解説】
/// F# Application層では、Railway-oriented Programmingパターンを採用し、
/// 処理結果をResult&lt;Success, Error&gt;型で表現します。このクラスは、
/// その結果を非同期処理と組み合わせてC#で扱えるDTOsに変換します。
///
/// 【Blazor Server初学者向け解説】
/// Blazor Serverページでは、F#の非同期処理結果をawaitして受け取り、
/// ページ表示用のDTOとして活用します。エラーハンドリングも包含します。
/// </summary>
public static class ApplicationResultConverters
{
    private static ILogger? _logger;

    /// <summary>
    /// ロガーを設定します（依存性注入で設定）
    /// </summary>
    /// <param name="logger">ILoggerインスタンス</param>
    public static void SetLogger(ILogger logger)
    {
        _logger = logger;
    }

    // ========================================
    // F# Task<Result<T, string>> → C# DTO変換（汎用）
    // ========================================

    /// <summary>
    /// F# Task&lt;Result&lt;TDomain, string&gt;&gt; を C# DTO に非同期変換
    /// 汎用的なResult型変換メソッド（型安全）
    ///
    /// 【使用例】
    /// - F#: Task&lt;Result&lt;Project, string&gt;&gt;
    /// - C#: Task&lt;ProjectResultDto&gt;
    /// </summary>
    /// <typeparam name="TDomain">F#ドメイン型</typeparam>
    /// <typeparam name="TDto">C# DTO型</typeparam>
    /// <param name="domainResultTask">F#の非同期Result</param>
    /// <param name="converter">ドメイン型→DTO変換関数</param>
    /// <param name="createSuccessResult">成功結果作成関数</param>
    /// <param name="createFailureResult">失敗結果作成関数</param>
    /// <returns>C#のResult DTO</returns>
    public static async Task<TDto> ConvertAsync<TDomain, TDto>(
        Task<Microsoft.FSharp.Core.FSharpResult<TDomain, string>> domainResultTask,
        Func<TDomain, object> converter,
        Func<object, TDto> createSuccessResult,
        Func<string, TDto> createFailureResult)
        where TDto : class
    {
        try
        {
            _logger?.LogDebug("F# Task<Result<{DomainType}, string>>→{DtoType} 変換開始",
                typeof(TDomain).Name, typeof(TDto).Name);

            var domainResult = await domainResultTask;

            if (domainResult.IsOk)
            {
                var domain = domainResult.ResultValue;
                var dto = converter(domain);

                _logger?.LogInformation("F# Task<Result<{DomainType}, string>>→{DtoType} 変換成功",
                    typeof(TDomain).Name, typeof(TDto).Name);

                return createSuccessResult(dto);
            }
            else
            {
                var error = domainResult.ErrorValue;
                _logger?.LogWarning("F#処理エラー→{DtoType} 変換: {Error}",
                    typeof(TDto).Name, error);

                return createFailureResult(error);
            }
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "F# Task<Result<{DomainType}, string>>→{DtoType} 変換で予期しないエラーが発生",
                typeof(TDomain).Name, typeof(TDto).Name);

            return createFailureResult($"変換処理でエラーが発生しました: {ex.Message}");
        }
    }

    // ========================================
    // プロジェクト専用変換メソッド
    // ========================================

    /// <summary>
    /// F# プロジェクト作成結果 → C# ProjectResultDto 変換
    /// Task&lt;Result&lt;Project, string&gt;&gt; の専用変換
    /// </summary>
    /// <param name="createProjectTask">F#のプロジェクト作成Task</param>
    /// <returns>C#のProjectResultDto</returns>
    public static async Task<ProjectResultDto> ConvertProjectCreationAsync(
        Task<Microsoft.FSharp.Core.FSharpResult<Project, string>> createProjectTask)
    {
        return await ConvertAsync(
            createProjectTask,
            domain => TypeConverters.ToDto((Project)domain),
            dto => ProjectResultDto.Success((ProjectDto)dto),
            error => ProjectQueryConverters.CategorizeProjectError(error)
        );
    }

    /// <summary>
    /// F# プロジェクト更新結果 → C# ProjectResultDto 変換
    /// Task&lt;Result&lt;Project, string&gt;&gt; の専用変換
    /// </summary>
    /// <param name="updateProjectTask">F#のプロジェクト更新Task</param>
    /// <returns>C#のProjectResultDto</returns>
    public static async Task<ProjectResultDto> ConvertProjectUpdateAsync(
        Task<Microsoft.FSharp.Core.FSharpResult<Project, string>> updateProjectTask)
    {
        return await ConvertAsync(
            updateProjectTask,
            domain => TypeConverters.ToDto((Project)domain),
            dto => ProjectResultDto.Success((ProjectDto)dto),
            error => ProjectQueryConverters.CategorizeProjectError(error)
        );
    }

    /// <summary>
    /// F# プロジェクト削除結果 → C# ProjectResultDto 変換
    /// Task&lt;Result&lt;unit, string&gt;&gt; の専用変換（削除は削除対象Projectを返す）
    /// </summary>
    /// <param name="deleteProjectTask">F#のプロジェクト削除Task</param>
    /// <param name="deletedProject">削除されたプロジェクト情報</param>
    /// <returns>C#のProjectResultDto</returns>
    public static async Task<ProjectResultDto> ConvertProjectDeletionAsync(
        Task<Microsoft.FSharp.Core.FSharpResult<Microsoft.FSharp.Core.Unit, string>> deleteProjectTask,
        ProjectDto deletedProject)
    {
        try
        {
            _logger?.LogDebug("F# プロジェクト削除結果→ProjectResultDto 変換開始 ProjectId: {ProjectId}",
                deletedProject.Id);

            var domainResult = await deleteProjectTask;

            if (domainResult.IsOk)
            {
                _logger?.LogInformation("F# プロジェクト削除成功→ProjectResultDto 変換完了 ProjectId: {ProjectId}",
                    deletedProject.Id);

                // 削除成功時は削除されたプロジェクト情報を返す
                return ProjectResultDto.Success(deletedProject);
            }
            else
            {
                var error = domainResult.ErrorValue;
                _logger?.LogWarning("F# プロジェクト削除エラー→ProjectResultDto 変換: {Error}", error);

                return ProjectQueryConverters.CategorizeProjectError(error);
            }
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "F# プロジェクト削除結果→ProjectResultDto 変換で予期しないエラーが発生");
            return ProjectResultDto.Failure($"削除結果変換処理でエラーが発生しました: {ex.Message}");
        }
    }

    /// <summary>
    /// F# プロジェクト取得結果 → C# ProjectResultDto 変換
    /// Task&lt;Result&lt;Project option, string&gt;&gt; の専用変換
    /// </summary>
    /// <param name="getProjectTask">F#のプロジェクト取得Task</param>
    /// <returns>C#のProjectResultDto</returns>
    public static async Task<ProjectResultDto> ConvertProjectRetrievalAsync(
        Task<Microsoft.FSharp.Core.FSharpResult<FSharpOption<Project>, string>> getProjectTask)
    {
        try
        {
            _logger?.LogDebug("F# プロジェクト取得結果→ProjectResultDto 変換開始");

            var domainResult = await getProjectTask;

            if (domainResult.IsOk)
            {
                var projectOption = domainResult.ResultValue;

                if (FSharpOption<Project>.get_IsSome(projectOption))
                {
                    var project = projectOption.Value;
                    var projectDto = TypeConverters.ToDto(project);

                    _logger?.LogInformation("F# プロジェクト取得成功→ProjectResultDto 変換完了 ProjectId: {ProjectId}",
                        project.Id.Value);

                    return ProjectResultDto.Success(projectDto);
                }
                else
                {
                    _logger?.LogInformation("F# プロジェクト取得結果: プロジェクトが見つかりません");
                    return ProjectResultDto.Failure("指定されたプロジェクトが見つかりません");
                }
            }
            else
            {
                var error = domainResult.ErrorValue;
                _logger?.LogWarning("F# プロジェクト取得エラー→ProjectResultDto 変換: {Error}", error);

                return ProjectQueryConverters.CategorizeProjectError(error);
            }
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "F# プロジェクト取得結果→ProjectResultDto 変換で予期しないエラーが発生");
            return ProjectResultDto.Failure($"取得結果変換処理でエラーが発生しました: {ex.Message}");
        }
    }

    /// <summary>
    /// F# プロジェクト一覧取得結果 → C# ProjectListResultDto 変換
    /// Task&lt;Result&lt;Project list, string&gt;&gt; の専用変換（ページング対応）
    /// </summary>
    /// <param name="getProjectsTask">F#のプロジェクト一覧取得Task</param>
    /// <param name="totalCount">総件数</param>
    /// <param name="pageNumber">ページ番号</param>
    /// <param name="pageSize">ページサイズ</param>
    /// <param name="userRole">要求者ユーザーロール</param>
    /// <returns>C#のProjectListResultDto</returns>
    public static async Task<ProjectListResultDto> ConvertProjectListAsync(
        Task<Microsoft.FSharp.Core.FSharpResult<Microsoft.FSharp.Collections.FSharpList<Project>, string>> getProjectsTask,
        int totalCount,
        int pageNumber,
        int pageSize,
        string userRole)
    {
        try
        {
            _logger?.LogDebug("F# プロジェクト一覧取得結果→ProjectListResultDto 変換開始 Role: {Role}", userRole);

            var domainResult = await getProjectsTask;

            if (domainResult.IsOk)
            {
                return ProjectQueryConverters.ToProjectListResultDto(
                    domainResult,
                    totalCount,
                    pageNumber,
                    pageSize,
                    userRole);
            }
            else
            {
                var error = domainResult.ErrorValue;
                _logger?.LogWarning("F# プロジェクト一覧取得エラー→ProjectListResultDto 変換: {Error}", error);

                return ProjectListResultDto.Failure(error);
            }
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "F# プロジェクト一覧取得結果→ProjectListResultDto 変換で予期しないエラーが発生");
            return ProjectListResultDto.Failure($"一覧取得変換処理でエラーが発生しました: {ex.Message}");
        }
    }

    // ========================================
    // 複合操作結果変換メソッド（Phase B1拡張）
    // ========================================

    /// <summary>
    /// F# プロジェクト作成結果（プロジェクト＋デフォルトドメイン）→ C# ProjectResultDto 変換
    /// Task&lt;Result&lt;Project * Domain, string&gt;&gt; の専用変換
    ///
    /// 【Phase B1対応】プロジェクト作成時の自動デフォルトドメイン作成結果
    /// </summary>
    /// <param name="createProjectWithDomainTask">F#のプロジェクト＋ドメイン作成Task</param>
    /// <returns>C#のProjectResultDto</returns>
    public static async Task<ProjectResultDto> ConvertProjectCreationWithDomainAsync(
        Task<Microsoft.FSharp.Core.FSharpResult<Tuple<Project, UbiquitousLanguageManager.Domain.ProjectManagement.Domain>, string>> createProjectWithDomainTask)
    {
        try
        {
            _logger?.LogDebug("F# プロジェクト＋ドメイン作成結果→ProjectResultDto 変換開始");

            var domainResult = await createProjectWithDomainTask;

            if (domainResult.IsOk)
            {
                var (project, defaultDomain) = domainResult.ResultValue;
                var projectDto = TypeConverters.ToDto(project);

                _logger?.LogInformation("F# プロジェクト＋ドメイン作成成功→ProjectResultDto 変換完了 ProjectId: {ProjectId}, DefaultDomain: {DomainName}",
                    project.Id.Value, defaultDomain.Name.Value);

                return ProjectResultDto.Success(projectDto);
            }
            else
            {
                var error = domainResult.ErrorValue;
                _logger?.LogWarning("F# プロジェクト＋ドメイン作成エラー→ProjectResultDto 変換: {Error}", error);

                return ProjectQueryConverters.CategorizeProjectError(error);
            }
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "F# プロジェクト＋ドメイン作成結果→ProjectResultDto 変換で予期しないエラーが発生");
            return ProjectResultDto.Failure($"プロジェクト＋ドメイン作成変換処理でエラーが発生しました: {ex.Message}");
        }
    }

    // ========================================
    // バッチ処理結果変換メソッド
    // ========================================

    /// <summary>
    /// F# バッチ処理結果 → C# 統合結果DTO 変換
    /// 複数の操作結果をまとめて処理する場合の変換
    ///
    /// 【将来拡張用】複数プロジェクトの一括操作時に使用予定
    /// </summary>
    /// <param name="batchResults">F#のバッチ処理結果リスト</param>
    /// <returns>統合されたProjectListResultDto</returns>
    public static async Task<ProjectListResultDto> ConvertBatchOperationAsync(
        List<Task<Microsoft.FSharp.Core.FSharpResult<Project, string>>> batchResults)
    {
        try
        {
            _logger?.LogDebug("F# バッチ処理結果→ProjectListResultDto 変換開始 BatchCount: {Count}",
                batchResults.Count);

            var successfulProjects = new List<ProjectDto>();
            var errors = new List<string>();

            foreach (var resultTask in batchResults)
            {
                var result = await resultTask;

                if (result.IsOk)
                {
                    var project = result.ResultValue;
                    var projectDto = TypeConverters.ToDto(project);
                    successfulProjects.Add(projectDto);
                }
                else
                {
                    errors.Add(result.ErrorValue);
                }
            }

            if (errors.Any())
            {
                var combinedError = string.Join("; ", errors);
                _logger?.LogWarning("F# バッチ処理部分失敗→ProjectListResultDto 変換: 成功={Success}件, エラー={Error}件",
                    successfulProjects.Count, errors.Count);

                // 部分成功の場合でも成功として扱い、エラー情報を含める
                var result = ProjectListResultDto.Success(successfulProjects, successfulProjects.Count, 1, successfulProjects.Count);
                result.ErrorMessage = $"一部の操作が失敗しました: {combinedError}";
                return result;
            }
            else
            {
                _logger?.LogInformation("F# バッチ処理全成功→ProjectListResultDto 変換完了 SuccessCount: {Count}",
                    successfulProjects.Count);

                return ProjectListResultDto.Success(successfulProjects, successfulProjects.Count, 1, successfulProjects.Count);
            }
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "F# バッチ処理結果→ProjectListResultDto 変換で予期しないエラーが発生");
            return ProjectListResultDto.Failure($"バッチ処理変換でエラーが発生しました: {ex.Message}");
        }
    }

    // ========================================
    // パフォーマンス最適化用変換メソッド
    // ========================================

    /// <summary>
    /// 大量プロジェクトリストの効率的変換
    /// メモリ使用量を抑えた変換処理
    ///
    /// 【パフォーマンス対応】1000件以上のプロジェクト一覧処理時に使用
    /// </summary>
    /// <param name="largeProjectListTask">F#の大量プロジェクトリスト取得Task</param>
    /// <param name="batchSize">バッチサイズ</param>
    /// <param name="pageNumber">ページ番号</param>
    /// <param name="pageSize">ページサイズ</param>
    /// <param name="userRole">要求者ユーザーロール</param>
    /// <returns>C#のProjectListResultDto</returns>
    public static async Task<ProjectListResultDto> ConvertLargeProjectListAsync(
        Task<Microsoft.FSharp.Core.FSharpResult<Microsoft.FSharp.Collections.FSharpList<Project>, string>> largeProjectListTask,
        int batchSize,
        int pageNumber,
        int pageSize,
        string userRole)
    {
        try
        {
            _logger?.LogDebug("F# 大量プロジェクトリスト→ProjectListResultDto 変換開始 BatchSize: {BatchSize}",
                batchSize);

            var domainResult = await largeProjectListTask;

            if (domainResult.IsOk)
            {
                var fsharpProjectList = domainResult.ResultValue;
                var projectArray = Microsoft.FSharp.Collections.ListModule.ToArray(fsharpProjectList);

                // バッチ処理で変換（メモリ効率化）
                var projects = new List<ProjectDto>();
                var totalCount = projectArray.Length;

                for (int i = 0; i < projectArray.Length; i += batchSize)
                {
                    var batch = projectArray.Skip(i).Take(batchSize);
                    var batchDtos = batch.Select(TypeConverters.ToDto).ToList();
                    projects.AddRange(batchDtos);

                    // バッチごとのGC実行（メモリ圧迫時）
                    if (i % (batchSize * 10) == 0)
                    {
                        GC.Collect();
                    }
                }

                // 権限情報を設定
                var authInfo = ProjectQueryConverters.ToAuthorizationResultDto(true, "ViewProject", userRole);

                _logger?.LogInformation("F# 大量プロジェクトリスト→ProjectListResultDto 変換成功 TotalCount: {Count}",
                    totalCount);

                var result = ProjectListResultDto.Success(projects, totalCount, pageNumber, pageSize);
                result.AuthorizationInfo = authInfo;

                return result;
            }
            else
            {
                var error = domainResult.ErrorValue;
                _logger?.LogWarning("F# 大量プロジェクトリスト取得エラー→ProjectListResultDto 変換: {Error}", error);

                return ProjectListResultDto.Failure(error);
            }
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "F# 大量プロジェクトリスト→ProjectListResultDto 変換で予期しないエラーが発生");
            return ProjectListResultDto.Failure($"大量リスト変換処理でエラーが発生しました: {ex.Message}");
        }
    }
}