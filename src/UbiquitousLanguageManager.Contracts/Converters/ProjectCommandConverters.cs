using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.FSharp.Core;
using UbiquitousLanguageManager.Contracts.DTOs;
using UbiquitousLanguageManager.Contracts.DTOs.Application;
using UbiquitousLanguageManager.Domain.Common;
using UbiquitousLanguageManager.Domain.ProjectManagement;
// UbiquitousLanguageManager.Domain.ProjectManagement.Domain型は使用箇所で直接指定
// Microsoft.FSharp.Core.FSharpResult型は使用箇所で直接指定

namespace UbiquitousLanguageManager.Contracts.Converters;

/// <summary>
/// プロジェクト関連Command/Query型変換クラス
/// F# Application層 ↔ C# Infrastructure/Web層の双方向変換
///
/// 【F#初学者向け解説】
/// このクラスは、C#のDTOsをF# Application層のCommand/Query型に変換し、
/// 逆にF#の処理結果をC#で扱えるDTOsに変換します。
/// F#のvalue objectやResult型との型安全な変換を行います。
///
/// 【Blazor Server初学者向け解説】
/// Blazor ServerのページコンポーネントからF# Application層への
/// データ受け渡しを安全に行うための変換層です。
/// </summary>
public static class ProjectCommandConverters
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
    // C# DTO → F# Command/Query 変換メソッド
    // ========================================

    /// <summary>
    /// CreateProjectCommandDto を F#のCreateProjectCommand パラメータタプルに変換
    /// F#のvalue objectバリデーションを活用した型安全な変換
    ///
    /// 【重要】F# Application層のCreateProjectCommandは、
    /// すでに存在するためパラメータタプルとして変換
    /// </summary>
    /// <param name="dto">C#のCreateProjectCommandDto</param>
    /// <returns>F#のResult型（成功時はタプル、失敗時はエラーメッセージ）</returns>
    public static Microsoft.FSharp.Core.FSharpResult<Tuple<string, string?, long>, string> ToFSharpCreateProjectParams(
        CreateProjectCommandDto dto)
    {
        try
        {
            if (dto == null)
            {
                _logger?.LogError("CreateProjectCommandDto→F#パラメータ変換失敗: DTOがnull");
                return Microsoft.FSharp.Core.FSharpResult<Tuple<string, string?, long>, string>.NewError("プロジェクト作成データがnullです");
            }

            _logger?.LogDebug("CreateProjectCommandDto→F#パラメータ変換開始 Name: {Name}, OwnerId: {OwnerId}",
                dto.Name, dto.OwnerId);

            // 入力値の基本検証
            if (string.IsNullOrWhiteSpace(dto.Name))
            {
                return Microsoft.FSharp.Core.FSharpResult<Tuple<string, string?, long>, string>.NewError("プロジェクト名は必須です");
            }

            if (dto.OwnerId <= 0)
            {
                return Microsoft.FSharp.Core.FSharpResult<Tuple<string, string?, long>, string>.NewError("有効な所有者IDを指定してください");
            }

            // F#パラメータタプルを作成
            var parameters = Tuple.Create(
                dto.Name.Trim(),
                string.IsNullOrWhiteSpace(dto.Description) ? null : dto.Description.Trim(),
                dto.OwnerId
            );

            _logger?.LogInformation("CreateProjectCommandDto→F#パラメータ変換成功 Name: {Name}",
                dto.Name);

            return Microsoft.FSharp.Core.FSharpResult<Tuple<string, string?, long>, string>.NewOk(parameters);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "CreateProjectCommandDto→F#パラメータ変換で予期しないエラーが発生");
            return Microsoft.FSharp.Core.FSharpResult<Tuple<string, string?, long>, string>.NewError($"変換処理でエラーが発生しました: {ex.Message}");
        }
    }

    /// <summary>
    /// UpdateProjectCommandDto を F#パラメータタプルに変換
    ///
    /// 【重要】プロジェクト名は変更禁止のため含まない
    /// Step1成果物の禁止事項（プロジェクト名変更不可）への対応
    /// </summary>
    /// <param name="dto">C#のUpdateProjectCommandDto</param>
    /// <returns>F#のResult型（成功時はタプル、失敗時はエラーメッセージ）</returns>
    public static Microsoft.FSharp.Core.FSharpResult<Tuple<long, string, long>, string> ToFSharpUpdateProjectParams(
        UpdateProjectCommandDto dto)
    {
        try
        {
            if (dto == null)
            {
                _logger?.LogError("UpdateProjectCommandDto→F#パラメータ変換失敗: DTOがnull");
                return Microsoft.FSharp.Core.FSharpResult<Tuple<long, string, long>, string>.NewError("プロジェクト更新データがnullです");
            }

            _logger?.LogDebug("UpdateProjectCommandDto→F#パラメータ変換開始 ProjectId: {ProjectId}, UserId: {UserId}",
                dto.ProjectId, dto.UserId);

            // 入力値の基本検証
            if (dto.ProjectId <= 0)
            {
                return Microsoft.FSharp.Core.FSharpResult<Tuple<long, string, long>, string>.NewError("有効なプロジェクトIDを指定してください");
            }

            if (dto.UserId <= 0)
            {
                return Microsoft.FSharp.Core.FSharpResult<Tuple<long, string, long>, string>.NewError("有効なユーザーIDを指定してください");
            }

            // F#パラメータタプルを作成
            var parameters = Tuple.Create(
                dto.ProjectId,
                dto.Description?.Trim() ?? string.Empty,
                dto.UserId
            );

            _logger?.LogInformation("UpdateProjectCommandDto→F#パラメータ変換成功 ProjectId: {ProjectId}",
                dto.ProjectId);

            return Microsoft.FSharp.Core.FSharpResult<Tuple<long, string, long>, string>.NewOk(parameters);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "UpdateProjectCommandDto→F#パラメータ変換で予期しないエラーが発生");
            return Microsoft.FSharp.Core.FSharpResult<Tuple<long, string, long>, string>.NewError($"変換処理でエラーが発生しました: {ex.Message}");
        }
    }

    /// <summary>
    /// DeleteProjectCommandDto を F#パラメータタプルに変換
    /// 論理削除用パラメータの変換
    /// </summary>
    /// <param name="dto">C#のDeleteProjectCommandDto</param>
    /// <returns>F#のResult型（成功時はタプル、失敗時はエラーメッセージ）</returns>
    public static Microsoft.FSharp.Core.FSharpResult<Tuple<long, long, string?>, string> ToFSharpDeleteProjectParams(
        DeleteProjectCommandDto dto)
    {
        try
        {
            if (dto == null)
            {
                _logger?.LogError("DeleteProjectCommandDto→F#パラメータ変換失敗: DTOがnull");
                return Microsoft.FSharp.Core.FSharpResult<Tuple<long, long, string?>, string>.NewError("プロジェクト削除データがnullです");
            }

            _logger?.LogDebug("DeleteProjectCommandDto→F#パラメータ変換開始 ProjectId: {ProjectId}, UserId: {UserId}",
                dto.ProjectId, dto.UserId);

            // 入力値の基本検証
            if (dto.ProjectId <= 0)
            {
                return Microsoft.FSharp.Core.FSharpResult<Tuple<long, long, string?>, string>.NewError("有効なプロジェクトIDを指定してください");
            }

            if (dto.UserId <= 0)
            {
                return Microsoft.FSharp.Core.FSharpResult<Tuple<long, long, string?>, string>.NewError("有効なユーザーIDを指定してください");
            }

            // F#パラメータタプルを作成
            var parameters = Tuple.Create(
                dto.ProjectId,
                dto.UserId,
                string.IsNullOrWhiteSpace(dto.Reason) ? null : dto.Reason.Trim()
            );

            _logger?.LogInformation("DeleteProjectCommandDto→F#パラメータ変換成功 ProjectId: {ProjectId}",
                dto.ProjectId);

            return Microsoft.FSharp.Core.FSharpResult<Tuple<long, long, string?>, string>.NewOk(parameters);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "DeleteProjectCommandDto→F#パラメータ変換で予期しないエラーが発生");
            return Microsoft.FSharp.Core.FSharpResult<Tuple<long, long, string?>, string>.NewError($"変換処理でエラーが発生しました: {ex.Message}");
        }
    }

    /// <summary>
    /// GetProjectsQueryDto を F#パラメータタプルに変換
    /// 権限制御・ページング対応クエリの変換
    ///
    /// 【権限マトリックス対応】
    /// - SuperUser: 全プロジェクト対象
    /// - ProjectManager: 担当プロジェクトのみ
    /// - DomainApprover: 所属プロジェクトのみ
    /// - GeneralUser: 所属プロジェクトのみ
    /// </summary>
    /// <param name="dto">C#のGetProjectsQueryDto</param>
    /// <returns>F#のResult型（成功時はタプル、失敗時はエラーメッセージ）</returns>
    public static Microsoft.FSharp.Core.FSharpResult<Tuple<long, string, int, int, bool, string?>, string> ToFSharpGetProjectsParams(
        GetProjectsQueryDto dto)
    {
        try
        {
            if (dto == null)
            {
                _logger?.LogError("GetProjectsQueryDto→F#パラメータ変換失敗: DTOがnull");
                return Microsoft.FSharp.Core.FSharpResult<Tuple<long, string, int, int, bool, string?>, string>.NewError("プロジェクト一覧取得データがnullです");
            }

            _logger?.LogDebug("GetProjectsQueryDto→F#パラメータ変換開始 UserId: {UserId}, UserRole: {UserRole}",
                dto.UserId, dto.UserRole);

            // 入力値の基本検証
            if (dto.UserId <= 0)
            {
                return Microsoft.FSharp.Core.FSharpResult<Tuple<long, string, int, int, bool, string?>, string>.NewError("有効なユーザーIDを指定してください");
            }

            if (string.IsNullOrWhiteSpace(dto.UserRole))
            {
                return Microsoft.FSharp.Core.FSharpResult<Tuple<long, string, int, int, bool, string?>, string>.NewError("ユーザーロールは必須です");
            }

            // ロール値の検証
            var validRoles = new[] { "SuperUser", "ProjectManager", "DomainApprover", "GeneralUser" };
            if (!validRoles.Contains(dto.UserRole))
            {
                return Microsoft.FSharp.Core.FSharpResult<Tuple<long, string, int, int, bool, string?>, string>.NewError($"無効なユーザーロールです: {dto.UserRole}");
            }

            // ページング値の検証
            if (dto.PageNumber < 1)
            {
                return Microsoft.FSharp.Core.FSharpResult<Tuple<long, string, int, int, bool, string?>, string>.NewError("ページ番号は1以上を指定してください");
            }

            if (dto.PageSize < 1 || dto.PageSize > 100)
            {
                return Microsoft.FSharp.Core.FSharpResult<Tuple<long, string, int, int, bool, string?>, string>.NewError("ページサイズは1-100の範囲で指定してください");
            }

            // SuperUser以外でIncludeInactive=trueは権限エラー
            if (dto.IncludeInactive && dto.UserRole != "SuperUser")
            {
                _logger?.LogWarning("非SuperUserによる非アクティブプロジェクト取得要求 UserId: {UserId}, Role: {UserRole}",
                    dto.UserId, dto.UserRole);
                return Microsoft.FSharp.Core.FSharpResult<Tuple<long, string, int, int, bool, string?>, string>.NewError("非アクティブなプロジェクトを含める権限がありません");
            }

            // F#パラメータタプルを作成
            var parameters = Tuple.Create(
                dto.UserId,
                dto.UserRole.Trim(),
                dto.PageNumber,
                dto.PageSize,
                dto.IncludeInactive,
                string.IsNullOrWhiteSpace(dto.SearchKeyword) ? null : dto.SearchKeyword.Trim()
            );

            _logger?.LogInformation("GetProjectsQueryDto→F#パラメータ変換成功 UserId: {UserId}, Role: {UserRole}",
                dto.UserId, dto.UserRole);

            return Microsoft.FSharp.Core.FSharpResult<Tuple<long, string, int, int, bool, string?>, string>.NewOk(parameters);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "GetProjectsQueryDto→F#パラメータ変換で予期しないエラーが発生");
            return Microsoft.FSharp.Core.FSharpResult<Tuple<long, string, int, int, bool, string?>, string>.NewError($"変換処理でエラーが発生しました: {ex.Message}");
        }
    }

    // ========================================
    // F# Command/Query → C# DTO 変換メソッド（逆変換）
    // ========================================

    /// <summary>
    /// F#のCreateProjectCommandパラメータからC# DTOに変換
    /// 双方向変換のための逆変換メソッド
    /// </summary>
    /// <param name="name">プロジェクト名</param>
    /// <param name="description">説明</param>
    /// <param name="ownerId">所有者ID</param>
    /// <returns>C#のCreateProjectCommandDto</returns>
    public static CreateProjectCommandDto FromFSharpCreateProjectParams(string name, string? description, long ownerId)
    {
        return new CreateProjectCommandDto
        {
            Name = name?.Trim() ?? string.Empty,
            Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim(),
            OwnerId = ownerId
        };
    }

    /// <summary>
    /// F#のUpdateProjectCommandパラメータからC# DTOに変換
    /// </summary>
    /// <param name="projectId">プロジェクトID</param>
    /// <param name="description">説明</param>
    /// <param name="userId">実行ユーザーID</param>
    /// <returns>C#のUpdateProjectCommandDto</returns>
    public static UpdateProjectCommandDto FromFSharpUpdateProjectParams(long projectId, string description, long userId)
    {
        return new UpdateProjectCommandDto
        {
            ProjectId = projectId,
            Description = description?.Trim() ?? string.Empty,
            UserId = userId
        };
    }

    /// <summary>
    /// F#のDeleteProjectCommandパラメータからC# DTOに変換
    /// </summary>
    /// <param name="projectId">プロジェクトID</param>
    /// <param name="userId">実行ユーザーID</param>
    /// <param name="reason">削除理由</param>
    /// <returns>C#のDeleteProjectCommandDto</returns>
    public static DeleteProjectCommandDto FromFSharpDeleteProjectParams(long projectId, long userId, string? reason)
    {
        return new DeleteProjectCommandDto
        {
            ProjectId = projectId,
            UserId = userId,
            Reason = string.IsNullOrWhiteSpace(reason) ? null : reason.Trim()
        };
    }

    /// <summary>
    /// F#のGetProjectsQueryパラメータからC# DTOに変換
    /// </summary>
    /// <param name="userId">ユーザーID</param>
    /// <param name="userRole">ユーザーロール</param>
    /// <param name="pageNumber">ページ番号</param>
    /// <param name="pageSize">ページサイズ</param>
    /// <param name="includeInactive">非アクティブ含む</param>
    /// <param name="searchKeyword">検索キーワード</param>
    /// <returns>C#のGetProjectsQueryDto</returns>
    public static GetProjectsQueryDto FromFSharpGetProjectsParams(
        long userId, string userRole, int pageNumber, int pageSize, bool includeInactive, string? searchKeyword)
    {
        return new GetProjectsQueryDto
        {
            UserId = userId,
            UserRole = userRole?.Trim() ?? string.Empty,
            PageNumber = pageNumber,
            PageSize = pageSize,
            IncludeInactive = includeInactive,
            SearchKeyword = string.IsNullOrWhiteSpace(searchKeyword) ? null : searchKeyword.Trim()
        };
    }

    // ========================================
    // バリデーションヘルパーメソッド
    // ========================================

    /// <summary>
    /// プロジェクト作成用DTOの詳細バリデーション
    /// F#のvalue objectバリデーション前の事前チェック
    /// </summary>
    /// <param name="dto">検証対象DTO</param>
    /// <returns>バリデーションエラーリスト（空=OK）</returns>
    public static List<ValidationErrorDto> ValidateCreateProjectCommandDto(CreateProjectCommandDto dto)
    {
        var errors = new List<ValidationErrorDto>();

        if (dto == null)
        {
            errors.Add(new ValidationErrorDto("", "プロジェクト作成データがnullです"));
            return errors;
        }

        // プロジェクト名検証
        if (string.IsNullOrWhiteSpace(dto.Name))
        {
            errors.Add(new ValidationErrorDto(nameof(dto.Name), "プロジェクト名は必須です"));
        }
        else if (dto.Name.Length > 100)
        {
            errors.Add(new ValidationErrorDto(nameof(dto.Name), "プロジェクト名は100文字以内で入力してください"));
        }

        // 説明検証
        if (!string.IsNullOrEmpty(dto.Description) && dto.Description.Length > 1000)
        {
            errors.Add(new ValidationErrorDto(nameof(dto.Description), "説明は1000文字以内で入力してください"));
        }

        // 所有者ID検証
        if (dto.OwnerId <= 0)
        {
            errors.Add(new ValidationErrorDto(nameof(dto.OwnerId), "有効な所有者IDを指定してください"));
        }

        return errors;
    }

    /// <summary>
    /// プロジェクト一覧取得用DTOの詳細バリデーション
    /// 権限制御・ページング値の検証
    /// </summary>
    /// <param name="dto">検証対象DTO</param>
    /// <returns>バリデーションエラーリスト（空=OK）</returns>
    public static List<ValidationErrorDto> ValidateGetProjectsQueryDto(GetProjectsQueryDto dto)
    {
        var errors = new List<ValidationErrorDto>();

        if (dto == null)
        {
            errors.Add(new ValidationErrorDto("", "プロジェクト一覧取得データがnullです"));
            return errors;
        }

        // ユーザーID検証
        if (dto.UserId <= 0)
        {
            errors.Add(new ValidationErrorDto(nameof(dto.UserId), "有効なユーザーIDを指定してください"));
        }

        // ユーザーロール検証
        if (string.IsNullOrWhiteSpace(dto.UserRole))
        {
            errors.Add(new ValidationErrorDto(nameof(dto.UserRole), "ユーザーロールは必須です"));
        }
        else
        {
            var validRoles = new[] { "SuperUser", "ProjectManager", "DomainApprover", "GeneralUser" };
            if (!validRoles.Contains(dto.UserRole))
            {
                errors.Add(new ValidationErrorDto(nameof(dto.UserRole), $"無効なユーザーロールです: {dto.UserRole}"));
            }
        }

        // ページング値検証
        if (dto.PageNumber < 1)
        {
            errors.Add(new ValidationErrorDto(nameof(dto.PageNumber), "ページ番号は1以上を指定してください"));
        }

        if (dto.PageSize < 1 || dto.PageSize > 100)
        {
            errors.Add(new ValidationErrorDto(nameof(dto.PageSize), "ページサイズは1-100の範囲で指定してください"));
        }

        // 権限制御検証
        if (dto.IncludeInactive && dto.UserRole != "SuperUser")
        {
            errors.Add(new ValidationErrorDto(nameof(dto.IncludeInactive), "非アクティブなプロジェクトを含める権限がありません"));
        }

        // 検索キーワード検証
        if (!string.IsNullOrEmpty(dto.SearchKeyword) && dto.SearchKeyword.Length > 100)
        {
            errors.Add(new ValidationErrorDto(nameof(dto.SearchKeyword), "検索キーワードは100文字以内で入力してください"));
        }

        return errors;
    }
}