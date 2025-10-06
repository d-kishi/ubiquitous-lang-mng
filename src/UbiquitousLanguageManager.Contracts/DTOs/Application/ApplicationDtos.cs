using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using UbiquitousLanguageManager.Contracts.DTOs;

namespace UbiquitousLanguageManager.Contracts.DTOs.Application;

// ========================================
// Application層向けCommand/Query DTOs
// F# Application層 ↔ C# Infrastructure/Web層の型変換用DTOs
// Phase B1 Step3: Application層実装対応
//
// 【F#初学者向け解説】
// F# Application層のCommand/Query型をC#で扱えるようにするためのDTOです。
// バリデーション属性により入力検証を行い、F#のvalue objectと連携します。
// ========================================

// ========================================
// プロジェクト管理Command/Query DTOs
// ========================================

/// <summary>
/// プロジェクト作成コマンドDTO
/// F# Application層のCreateProjectCommandとの双方向変換用
/// </summary>
public class CreateProjectCommandDto
{
    /// <summary>
    /// プロジェクト名
    /// F# JapaneseName値オブジェクトに変換される
    ///
    /// 【禁止事項対応】
    /// - プロジェクト名重複禁止
    /// - 日本語名制限あり（F#バリデーションで実施）
    /// </summary>
    [Required(ErrorMessage = "プロジェクト名は必須です")]
    [StringLength(100, ErrorMessage = "プロジェクト名は100文字以内で入力してください")]
    [RegularExpression(@"^[a-zA-Z0-9\s\-_\u3040-\u309F\u30A0-\u30FF\u4E00-\u9FAF]+$",
        ErrorMessage = "プロジェクト名に使用できない文字が含まれています")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// プロジェクト説明
    /// F# Description値オブジェクトに変換される
    /// </summary>
    [StringLength(1000, ErrorMessage = "説明は1000文字以内で入力してください")]
    public string? Description { get; set; }

    /// <summary>
    /// プロジェクト所有者ID
    /// F# UserId判別共用体に変換される
    /// </summary>
    [Required]
    [Range(1, long.MaxValue, ErrorMessage = "有効な所有者IDを指定してください")]
    public long OwnerId { get; set; }
}

/// <summary>
/// プロジェクト編集コマンドDTO
///
/// 【重要】プロジェクト名は変更禁止のため含まない
/// Step1成果物の禁止事項（プロジェクト名変更不可）への対応
/// </summary>
public class UpdateProjectCommandDto
{
    /// <summary>
    /// プロジェクトID
    /// F# ProjectId判別共用体に変換される
    /// </summary>
    [Required]
    [Range(1, long.MaxValue, ErrorMessage = "有効なプロジェクトIDを指定してください")]
    public long ProjectId { get; set; }

    /// <summary>
    /// プロジェクト説明（更新可能）
    /// F# Description値オブジェクトに変換される
    /// </summary>
    [StringLength(1000, ErrorMessage = "説明は1000文字以内で入力してください")]
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// 更新実行者ID（権限チェック用）
    /// F# UserId判別共用体に変換される
    /// </summary>
    [Required]
    [Range(1, long.MaxValue, ErrorMessage = "有効なユーザーIDを指定してください")]
    public long UserId { get; set; }
}

/// <summary>
/// プロジェクト削除コマンドDTO
/// 論理削除用のコマンド
/// </summary>
public class DeleteProjectCommandDto
{
    /// <summary>
    /// 削除対象プロジェクトID
    /// F# ProjectId判別共用体に変換される
    /// </summary>
    [Required]
    [Range(1, long.MaxValue, ErrorMessage = "有効なプロジェクトIDを指定してください")]
    public long ProjectId { get; set; }

    /// <summary>
    /// 削除実行者ID（権限チェック用）
    /// F# UserId判別共用体に変換される
    /// </summary>
    [Required]
    [Range(1, long.MaxValue, ErrorMessage = "有効なユーザーIDを指定してください")]
    public long UserId { get; set; }

    /// <summary>
    /// 削除理由（オプション）
    /// 監査ログ用
    /// </summary>
    [StringLength(500, ErrorMessage = "削除理由は500文字以内で入力してください")]
    public string? Reason { get; set; }
}

/// <summary>
/// プロジェクト一覧取得クエリDTO
/// 権限制御・ページング対応
///
/// 【権限マトリックス対応】
/// - SuperUser: 全プロジェクト取得
/// - ProjectManager: 担当プロジェクトのみ
/// - DomainApprover: 所属プロジェクトのみ
/// - GeneralUser: 所属プロジェクトのみ
/// </summary>
public class GetProjectsQueryDto
{
    /// <summary>
    /// 要求者ユーザーID（権限フィルタリング用）
    /// F# UserId判別共用体に変換される
    /// </summary>
    [Required]
    [Range(1, long.MaxValue, ErrorMessage = "有効なユーザーIDを指定してください")]
    public long UserId { get; set; }

    /// <summary>
    /// 要求者ユーザーロール（権限フィルタリング用）
    /// F# Role判別共用体に変換される
    /// </summary>
    [Required(ErrorMessage = "ユーザーロールは必須です")]
    [RegularExpression(@"^(SuperUser|ProjectManager|DomainApprover|GeneralUser)$",
        ErrorMessage = "無効なユーザーロールです")]
    public string UserRole { get; set; } = string.Empty;

    /// <summary>
    /// ページ番号（1から開始）
    /// </summary>
    [Range(1, int.MaxValue, ErrorMessage = "ページ番号は1以上を指定してください")]
    public int PageNumber { get; set; } = 1;

    /// <summary>
    /// ページサイズ
    /// </summary>
    [Range(1, 100, ErrorMessage = "ページサイズは1-100の範囲で指定してください")]
    public int PageSize { get; set; } = 20;

    /// <summary>
    /// 非アクティブなプロジェクトも含めるかどうか
    /// SuperUserのみtrue指定可能
    /// </summary>
    public bool IncludeInactive { get; set; } = false;

    /// <summary>
    /// プロジェクト名による部分検索（オプション）
    /// </summary>
    [StringLength(100, ErrorMessage = "検索キーワードは100文字以内で入力してください")]
    public string? SearchKeyword { get; set; }
}

// ========================================
// Application層結果DTOs
// ========================================

/// <summary>
/// プロジェクト操作結果DTO
/// F# Application層のResult型をC#で扱えるようにするためのDTO
///
/// 【Railway-oriented Programming対応】
/// F#のResult&lt;T, TError&gt;型との双方向変換を行います
/// </summary>
public class ProjectResultDto
{
    /// <summary>
    /// 操作成功フラグ
    /// F# Result型のisOk相当
    /// </summary>
    public bool IsSuccess { get; set; }

    /// <summary>
    /// 操作対象プロジェクト情報
    /// 成功時のみ設定される
    /// </summary>
    public ProjectDto? Project { get; set; }

    /// <summary>
    /// エラーメッセージ
    /// 失敗時のみ設定される
    /// F# Result型のError値から変換
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// バリデーションエラー詳細
    /// 入力検証エラー時の詳細情報
    /// </summary>
    public List<ValidationErrorDto> ValidationErrors { get; set; } = new();

    /// <summary>
    /// 権限チェック結果
    /// 権限関連エラーの詳細情報
    /// </summary>
    public AuthorizationResultDto AuthorizationInfo { get; set; } = new();

    /// <summary>
    /// 成功結果を作成
    /// </summary>
    /// <param name="project">操作結果プロジェクト</param>
    /// <returns>成功結果</returns>
    public static ProjectResultDto Success(ProjectDto project)
    {
        return new ProjectResultDto
        {
            IsSuccess = true,
            Project = project,
            ErrorMessage = null,
            ValidationErrors = new(),
            AuthorizationInfo = AuthorizationResultDto.Authorized()
        };
    }

    /// <summary>
    /// 失敗結果を作成
    /// </summary>
    /// <param name="errorMessage">エラーメッセージ</param>
    /// <returns>失敗結果</returns>
    public static ProjectResultDto Failure(string errorMessage)
    {
        return new ProjectResultDto
        {
            IsSuccess = false,
            Project = null,
            ErrorMessage = errorMessage,
            ValidationErrors = new(),
            AuthorizationInfo = new()
        };
    }

    /// <summary>
    /// バリデーションエラー結果を作成
    /// </summary>
    /// <param name="validationErrors">バリデーションエラーリスト</param>
    /// <returns>バリデーションエラー結果</returns>
    public static ProjectResultDto ValidationFailure(List<ValidationErrorDto> validationErrors)
    {
        return new ProjectResultDto
        {
            IsSuccess = false,
            Project = null,
            ErrorMessage = "入力値に問題があります",
            ValidationErrors = validationErrors,
            AuthorizationInfo = new()
        };
    }

    /// <summary>
    /// 権限エラー結果を作成
    /// </summary>
    /// <param name="authError">権限エラー情報</param>
    /// <returns>権限エラー結果</returns>
    public static ProjectResultDto AuthorizationFailure(AuthorizationResultDto authError)
    {
        return new ProjectResultDto
        {
            IsSuccess = false,
            Project = null,
            ErrorMessage = authError.Reason,
            ValidationErrors = new(),
            AuthorizationInfo = authError
        };
    }
}

/// <summary>
/// プロジェクト一覧取得結果DTO
/// ページング・権限フィルタリング対応
/// </summary>
public class ProjectListResultDto
{
    /// <summary>
    /// 取得成功フラグ
    /// </summary>
    public bool IsSuccess { get; set; }

    /// <summary>
    /// プロジェクトリスト
    /// 権限に応じてフィルタリング済み
    /// </summary>
    public List<ProjectDto> Projects { get; set; } = new();

    /// <summary>
    /// 総件数
    /// フィルタリング適用後の総数
    /// </summary>
    public int TotalCount { get; set; }

    /// <summary>
    /// 現在ページ番号
    /// </summary>
    public int PageNumber { get; set; }

    /// <summary>
    /// ページサイズ
    /// </summary>
    public int PageSize { get; set; }

    /// <summary>
    /// 次ページの有無
    /// </summary>
    public bool HasNextPage => PageNumber * PageSize < TotalCount;

    /// <summary>
    /// 前ページの有無
    /// </summary>
    public bool HasPreviousPage => PageNumber > 1;

    /// <summary>
    /// 権限チェック結果
    /// </summary>
    public AuthorizationResultDto AuthorizationInfo { get; set; } = new();

    /// <summary>
    /// エラーメッセージ
    /// 取得失敗時のみ設定
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// 成功結果を作成
    /// </summary>
    /// <param name="projects">プロジェクトリスト</param>
    /// <param name="totalCount">総件数</param>
    /// <param name="pageNumber">ページ番号</param>
    /// <param name="pageSize">ページサイズ</param>
    /// <returns>成功結果</returns>
    public static ProjectListResultDto Success(
        List<ProjectDto> projects,
        int totalCount,
        int pageNumber,
        int pageSize)
    {
        return new ProjectListResultDto
        {
            IsSuccess = true,
            Projects = projects,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize,
            AuthorizationInfo = AuthorizationResultDto.Authorized(),
            ErrorMessage = null
        };
    }

    /// <summary>
    /// 失敗結果を作成
    /// </summary>
    /// <param name="errorMessage">エラーメッセージ</param>
    /// <returns>失敗結果</returns>
    public static ProjectListResultDto Failure(string errorMessage)
    {
        return new ProjectListResultDto
        {
            IsSuccess = false,
            Projects = new(),
            TotalCount = 0,
            PageNumber = 1,
            PageSize = 20,
            AuthorizationInfo = new(),
            ErrorMessage = errorMessage
        };
    }
}

// ========================================
// 共通ヘルパーDTOs
// ========================================

/// <summary>
/// バリデーションエラー詳細DTO
/// F# Application層のvalidationErrorsとの対応
/// </summary>
public class ValidationErrorDto
{
    /// <summary>
    /// エラー対象フィールド名
    /// </summary>
    public string FieldName { get; set; } = string.Empty;

    /// <summary>
    /// エラーメッセージ
    /// </summary>
    public string ErrorMessage { get; set; } = string.Empty;

    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="fieldName">フィールド名</param>
    /// <param name="errorMessage">エラーメッセージ</param>
    public ValidationErrorDto(string fieldName, string errorMessage)
    {
        FieldName = fieldName;
        ErrorMessage = errorMessage;
    }
}

/// <summary>
/// 権限制御結果DTO
/// Step1成果物の権限マトリックス対応
/// </summary>
public class AuthorizationResultDto
{
    /// <summary>
    /// 権限許可フラグ
    /// </summary>
    public bool IsAuthorized { get; set; }

    /// <summary>
    /// 権限拒否理由
    /// 権限エラー時の詳細メッセージ
    /// </summary>
    public string Reason { get; set; } = string.Empty;

    /// <summary>
    /// 必要な権限ロールリスト
    /// 権限エラー時のガイダンス用
    /// </summary>
    public List<string> RequiredRoles { get; set; } = new();

    /// <summary>
    /// 実行可能な操作リスト
    /// 現在の権限で実行可能な操作を通知
    /// </summary>
    public List<string> AllowedActions { get; set; } = new();

    /// <summary>
    /// 権限許可結果を作成
    /// </summary>
    /// <returns>許可結果</returns>
    public static AuthorizationResultDto Authorized()
    {
        return new AuthorizationResultDto
        {
            IsAuthorized = true,
            Reason = string.Empty,
            RequiredRoles = new(),
            AllowedActions = new()
        };
    }

    /// <summary>
    /// 権限拒否結果を作成
    /// </summary>
    /// <param name="reason">拒否理由</param>
    /// <param name="requiredRoles">必要なロール</param>
    /// <returns>拒否結果</returns>
    public static AuthorizationResultDto Denied(string reason, List<string> requiredRoles)
    {
        return new AuthorizationResultDto
        {
            IsAuthorized = false,
            Reason = reason,
            RequiredRoles = requiredRoles,
            AllowedActions = new()
        };
    }
}