using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.FSharp.Core;
using UbiquitousLanguageManager.Contracts.DTOs;
using UbiquitousLanguageManager.Contracts.DTOs.Application;
using UbiquitousLanguageManager.Domain;
// UbiquitousLanguageManager.Domain.Domain型は使用箇所で直接指定
// FSharpResult型は使用箇所で直接指定

namespace UbiquitousLanguageManager.Contracts.Converters;

/// <summary>
/// プロジェクト関連Query結果変換クラス
/// F# Application層のクエリ結果 → C# DTO変換専用
///
/// 【F#初学者向け解説】
/// このクラスは、F# Application層で処理されたクエリ結果を
/// C#で扱えるDTOsに変換します。特にResult型の変換と、
/// 権限制御による結果フィルタリングに対応しています。
///
/// 【Blazor Server初学者向け解説】
/// F#の処理結果をBlazor Serverページで表示できる形に変換し、
/// 権限に応じた表示制御情報も提供します。
/// </summary>
public static class ProjectQueryConverters
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
    // F# Result型 → C# Result DTO 変換メソッド
    // ========================================

    /// <summary>
    /// F#のプロジェクト操作結果をC# ProjectResultDtoに変換
    /// Railway-oriented Programming結果の型安全な変換
    ///
    /// 【重要】F# Result&lt;Project, string&gt; → C# ProjectResultDto
    /// </summary>
    /// <param name="fsharpResult">F#のプロジェクト操作結果</param>
    /// <returns>C#のProjectResultDto</returns>
    public static ProjectResultDto ToProjectResultDto(Microsoft.FSharp.Core.FSharpResult<Project, string> fsharpResult)
    {
        try
        {
            if (fsharpResult.IsOk)
            {
                var project = fsharpResult.ResultValue;
                var projectDto = TypeConverters.ToDto(project);

                _logger?.LogInformation("F#プロジェクト操作結果→ProjectResultDto変換成功 ProjectId: {ProjectId}",
                    project.Id.Value);

                return ProjectResultDto.Success(projectDto);
            }
            else
            {
                var error = fsharpResult.ErrorValue;
                _logger?.LogWarning("F#プロジェクト操作エラー→ProjectResultDto変換: {Error}", error);

                return ProjectResultDto.Failure(error);
            }
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "F#プロジェクト操作結果→ProjectResultDto変換で予期しないエラーが発生");
            return ProjectResultDto.Failure($"結果変換処理でエラーが発生しました: {ex.Message}");
        }
    }

    /// <summary>
    /// F#のプロジェクト作成結果をC# ProjectResultDtoに変換
    /// 作成結果とデフォルトドメイン情報の統合変換
    ///
    /// 【Phase B1対応】プロジェクト作成時の自動デフォルトドメイン作成結果も含む
    /// </summary>
    /// <param name="fsharpResult">F#のプロジェクト作成結果（Project * Domain）</param>
    /// <returns>C#のProjectResultDto</returns>
    public static ProjectResultDto ToProjectCreationResultDto(
        Microsoft.FSharp.Core.FSharpResult<Tuple<Project, UbiquitousLanguageManager.Domain.Domain>, string> fsharpResult)
    {
        try
        {
            if (fsharpResult.IsOk)
            {
                var (project, defaultDomain) = fsharpResult.ResultValue;
                var projectDto = TypeConverters.ToDto(project);

                // プロジェクト作成成功ログ
                _logger?.LogInformation("F#プロジェクト作成結果→ProjectResultDto変換成功 ProjectId: {ProjectId}, DefaultDomain: {DomainName}",
                    project.Id.Value, defaultDomain.Name.Value);

                return ProjectResultDto.Success(projectDto);
            }
            else
            {
                var error = fsharpResult.ErrorValue;
                _logger?.LogWarning("F#プロジェクト作成エラー→ProjectResultDto変換: {Error}", error);

                return ProjectResultDto.Failure(error);
            }
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "F#プロジェクト作成結果→ProjectResultDto変換で予期しないエラーが発生");
            return ProjectResultDto.Failure($"作成結果変換処理でエラーが発生しました: {ex.Message}");
        }
    }

    /// <summary>
    /// F#のプロジェクトリスト取得結果をC# ProjectListResultDtoに変換
    /// 権限フィルタリング・ページング情報の統合変換
    ///
    /// 【権限マトリックス対応】
    /// 各ユーザーロールに応じてフィルタリング済みの結果を変換
    /// </summary>
    /// <param name="fsharpResult">F#のプロジェクトリスト取得結果</param>
    /// <param name="totalCount">総件数</param>
    /// <param name="pageNumber">ページ番号</param>
    /// <param name="pageSize">ページサイズ</param>
    /// <param name="userRole">要求者ユーザーロール</param>
    /// <returns>C#のProjectListResultDto</returns>
    public static ProjectListResultDto ToProjectListResultDto(
        Microsoft.FSharp.Core.FSharpResult<Microsoft.FSharp.Collections.FSharpList<Project>, string> fsharpResult,
        int totalCount,
        int pageNumber,
        int pageSize,
        string userRole)
    {
        try
        {
            if (fsharpResult.IsOk)
            {
                var fsharpProjectList = fsharpResult.ResultValue;

                // F# list を C# List に変換
                var projects = Microsoft.FSharp.Collections.ListModule.ToArray(fsharpProjectList)
                    .Select(TypeConverters.ToDto)
                    .ToList();

                // 権限情報を設定
                var authInfo = CreateAuthorizationInfo(userRole, projects.Count);

                _logger?.LogInformation("F#プロジェクトリスト→ProjectListResultDto変換成功 Count: {Count}, Role: {Role}",
                    projects.Count, userRole);

                var result = ProjectListResultDto.Success(projects, totalCount, pageNumber, pageSize);
                result.AuthorizationInfo = authInfo;

                return result;
            }
            else
            {
                var error = fsharpResult.ErrorValue;
                _logger?.LogWarning("F#プロジェクトリスト取得エラー→ProjectListResultDto変換: {Error}", error);

                return ProjectListResultDto.Failure(error);
            }
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "F#プロジェクトリスト→ProjectListResultDto変換で予期しないエラーが発生");
            return ProjectListResultDto.Failure($"リスト変換処理でエラーが発生しました: {ex.Message}");
        }
    }

    /// <summary>
    /// F#の単一プロジェクト取得結果をC# ProjectResultDtoに変換
    /// Option型対応（見つからない場合の処理を含む）
    ///
    /// 【F#初学者向け解説】
    /// F#のOption型（Some/None）をC#のnull可能型として扱います
    /// </summary>
    /// <param name="fsharpResult">F#のプロジェクト取得結果（Option型）</param>
    /// <returns>C#のProjectResultDto</returns>
    public static ProjectResultDto ToProjectResultDto(Microsoft.FSharp.Core.FSharpResult<FSharpOption<Project>, string> fsharpResult)
    {
        try
        {
            if (fsharpResult.IsOk)
            {
                var projectOption = fsharpResult.ResultValue;

                if (FSharpOption<Project>.get_IsSome(projectOption))
                {
                    var project = projectOption.Value;
                    var projectDto = TypeConverters.ToDto(project);

                    _logger?.LogInformation("F#プロジェクト取得結果→ProjectResultDto変換成功 ProjectId: {ProjectId}",
                        project.Id.Value);

                    return ProjectResultDto.Success(projectDto);
                }
                else
                {
                    _logger?.LogInformation("F#プロジェクト取得結果: プロジェクトが見つかりません");
                    return ProjectResultDto.Failure("指定されたプロジェクトが見つかりません");
                }
            }
            else
            {
                var error = fsharpResult.ErrorValue;
                _logger?.LogWarning("F#プロジェクト取得エラー→ProjectResultDto変換: {Error}", error);

                return ProjectResultDto.Failure(error);
            }
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "F#プロジェクト取得結果→ProjectResultDto変換で予期しないエラーが発生");
            return ProjectResultDto.Failure($"取得結果変換処理でエラーが発生しました: {ex.Message}");
        }
    }

    // ========================================
    // 権限制御結果変換メソッド
    // ========================================

    /// <summary>
    /// ユーザーロールに応じた権限情報を作成
    /// Step1成果物の権限マトリックスに基づく情報生成
    /// </summary>
    /// <param name="userRole">ユーザーロール</param>
    /// <param name="projectCount">取得できたプロジェクト数</param>
    /// <returns>権限情報DTO</returns>
    private static AuthorizationResultDto CreateAuthorizationInfo(string userRole, int projectCount)
    {
        var authInfo = AuthorizationResultDto.Authorized();

        // ロール別の実行可能操作を設定
        switch (userRole)
        {
            case "SuperUser":
                authInfo.AllowedActions = new List<string>
                {
                    "CreateProject", "EditProject", "DeleteProject", "ViewProject"
                };
                break;

            case "ProjectManager":
                authInfo.AllowedActions = new List<string>
                {
                    "EditProject", "ViewProject" // 担当プロジェクトのみ
                };
                break;

            case "DomainApprover":
            case "GeneralUser":
                authInfo.AllowedActions = new List<string>
                {
                    "ViewProject" // 所属プロジェクトのみ
                };
                break;

            default:
                authInfo.AllowedActions = new List<string>();
                break;
        }

        return authInfo;
    }

    /// <summary>
    /// 操作権限チェック結果をAuthorizationResultDtoに変換
    /// F#ドメインサービスの権限チェック結果変換
    /// </summary>
    /// <param name="isAuthorized">権限許可フラグ</param>
    /// <param name="operation">操作種別</param>
    /// <param name="userRole">ユーザーロール</param>
    /// <param name="reason">権限拒否理由</param>
    /// <returns>権限チェック結果DTO</returns>
    public static AuthorizationResultDto ToAuthorizationResultDto(
        bool isAuthorized,
        string operation,
        string userRole,
        string? reason = null)
    {
        if (isAuthorized)
        {
            return AuthorizationResultDto.Authorized();
        }
        else
        {
            var requiredRoles = GetRequiredRolesForOperation(operation);
            var errorReason = reason ?? $"操作「{operation}」を実行する権限がありません";

            _logger?.LogWarning("権限チェック失敗: Operation={Operation}, UserRole={UserRole}, Reason={Reason}",
                operation, userRole, errorReason);

            return AuthorizationResultDto.Denied(errorReason, requiredRoles);
        }
    }

    /// <summary>
    /// 操作に必要なロール一覧を取得
    /// Step1成果物の権限マトリックスに基づく
    /// </summary>
    /// <param name="operation">操作種別</param>
    /// <returns>必要なロール一覧</returns>
    private static List<string> GetRequiredRolesForOperation(string operation)
    {
        return operation switch
        {
            "CreateProject" => new List<string> { "SuperUser" },
            "EditProject" => new List<string> { "SuperUser", "ProjectManager" },
            "DeleteProject" => new List<string> { "SuperUser" },
            "ViewProject" => new List<string> { "SuperUser", "ProjectManager", "DomainApprover", "GeneralUser" },
            _ => new List<string>()
        };
    }

    // ========================================
    // エラー変換ヘルパーメソッド
    // ========================================

    /// <summary>
    /// F#のDomainErrorをValidationErrorDtoリストに変換
    /// F#ドメイン層のバリデーションエラーをC#向けに変換
    /// </summary>
    /// <param name="domainErrors">F#のドメインエラー文字列</param>
    /// <returns>バリデーションエラーDTOリスト</returns>
    public static List<ValidationErrorDto> ToValidationErrors(string domainErrors)
    {
        var errors = new List<ValidationErrorDto>();

        if (string.IsNullOrWhiteSpace(domainErrors))
            return errors;

        try
        {
            // F#のエラーメッセージを解析（例: "Name: 無効な名前, Description: 説明が長すぎます"）
            var errorPairs = domainErrors.Split(',', StringSplitOptions.RemoveEmptyEntries);

            foreach (var errorPair in errorPairs)
            {
                var parts = errorPair.Split(':', StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length == 2)
                {
                    var fieldName = parts[0].Trim();
                    var errorMessage = parts[1].Trim();
                    errors.Add(new ValidationErrorDto(fieldName, errorMessage));
                }
                else
                {
                    // フィールド名が特定できない場合は汎用エラー
                    errors.Add(new ValidationErrorDto("General", errorPair.Trim()));
                }
            }
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "F#ドメインエラーの解析に失敗: {DomainErrors}", domainErrors);
            errors.Add(new ValidationErrorDto("General", domainErrors));
        }

        return errors;
    }

    /// <summary>
    /// F#のプロジェクト関連エラーを分類・変換
    /// エラー種別に応じた適切なProjectResultDtoを作成
    /// </summary>
    /// <param name="error">F#のエラーメッセージ</param>
    /// <returns>適切に分類されたProjectResultDto</returns>
    public static ProjectResultDto CategorizeProjectError(string error)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(error))
            {
                return ProjectResultDto.Failure("不明なエラーが発生しました");
            }

            // エラー種別による分類
            if (error.Contains("重複") || error.Contains("Duplicate"))
            {
                var validationErrors = new List<ValidationErrorDto>
                {
                    new ValidationErrorDto("Name", "指定されたプロジェクト名は既に使用されています")
                };
                return ProjectResultDto.ValidationFailure(validationErrors);
            }
            else if (error.Contains("権限") || error.Contains("Permission") || error.Contains("Unauthorized"))
            {
                var authError = AuthorizationResultDto.Denied(error, new List<string>());
                return ProjectResultDto.AuthorizationFailure(authError);
            }
            else if (error.Contains("見つかりません") || error.Contains("Not Found"))
            {
                return ProjectResultDto.Failure("指定されたプロジェクトが見つかりません");
            }
            else if (error.Contains(":"))
            {
                // バリデーションエラー形式の場合
                var validationErrors = ToValidationErrors(error);
                return ProjectResultDto.ValidationFailure(validationErrors);
            }
            else
            {
                return ProjectResultDto.Failure(error);
            }
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "プロジェクトエラー分類処理でエラーが発生: {Error}", error);
            return ProjectResultDto.Failure($"エラー処理中に問題が発生しました: {error}");
        }
    }

    // ========================================
    // 統計・集計情報変換メソッド
    // ========================================

    /// <summary>
    /// プロジェクト統計情報の変換
    /// F#ドメインサービスで算出された統計情報をDTOに変換
    ///
    /// 【将来拡張用】プロジェクト詳細画面での統計表示用
    /// </summary>
    /// <param name="project">対象プロジェクト</param>
    /// <param name="domainCount">ドメイン数</param>
    /// <param name="memberCount">メンバー数</param>
    /// <param name="termCount">用語数</param>
    /// <returns>統計情報付きProjectDto</returns>
    public static ProjectDto ToProjectDtoWithStatistics(
        Project project,
        int domainCount,
        int memberCount,
        int termCount)
    {
        var projectDto = TypeConverters.ToDto(project);

        // 統計情報を設定
        projectDto.MemberCount = memberCount;

        // 将来の拡張: 統計情報専用プロパティの追加予定
        // projectDto.DomainCount = domainCount;
        // projectDto.TermCount = termCount;

        return projectDto;
    }
}