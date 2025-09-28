using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace UbiquitousLanguageManager.Contracts.DTOs;

/// <summary>
/// プロジェクト情報のデータ転送オブジェクト
/// F#ドメインエンティティとC#プレゼンテーション層の境界で使用
/// Phase B1: F# Projectエンティティとの完全互換性確保
/// </summary>
public class ProjectDto
{
    /// <summary>
    /// プロジェクトID（主キー）
    /// F# ProjectId判別共用体からの変換値
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// プロジェクト名
    /// F# JapaneseName値オブジェクトからの変換値
    /// </summary>
    [Required(ErrorMessage = "プロジェクト名は必須です")]
    [StringLength(100, ErrorMessage = "プロジェクト名は100文字以内で入力してください")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// プロジェクト説明
    /// F# Description値オブジェクトからの変換値
    /// </summary>
    [Required(ErrorMessage = "プロジェクト説明は必須です")]
    [StringLength(1000, ErrorMessage = "プロジェクト説明は1000文字以内で入力してください")]
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// プロジェクト所有者ID
    /// F# UserId判別共用体からの変換値（Phase B1で追加）
    /// </summary>
    public long OwnerId { get; set; }

    /// <summary>
    /// 作成日時（UTC）
    /// F#ドメインエンティティの作成日時
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// 最終更新日時（UTC）
    /// F#ドメインエンティティの更新日時
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// 最終更新者ID
    /// F# UserId判別共用体からの変換値
    /// </summary>
    public long UpdatedBy { get; set; }

    /// <summary>
    /// アクティブ状態フラグ
    /// プロジェクトの有効/無効状態
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// 関連するドメイン一覧（参照用）
    /// プロジェクトに属するドメインのリスト
    /// </summary>
    public List<DomainDto> Domains { get; set; } = new();

    /// <summary>
    /// プロジェクト参加ユーザー数（参照用）
    /// このプロジェクトに権限を持つユーザー数
    /// </summary>
    public int MemberCount { get; set; }
}

/// <summary>
/// プロジェクト作成用コマンドDTO
/// 新規プロジェクト作成時の入力データ構造
/// Phase B1: F#ドメインサービスとの型安全な連携
/// </summary>
public class CreateProjectCommand
{
    /// <summary>
    /// プロジェクト名
    /// F# JapaneseName値オブジェクトに変換される
    /// </summary>
    [Required(ErrorMessage = "プロジェクト名は必須です")]
    [StringLength(100, ErrorMessage = "プロジェクト名は100文字以内で入力してください")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// プロジェクト説明
    /// F# Description値オブジェクトに変換される
    /// </summary>
    [StringLength(1000, ErrorMessage = "プロジェクト説明は1000文字以内で入力してください")]
    public string? Description { get; set; }

    /// <summary>
    /// プロジェクト所有者ID
    /// F# UserId判別共用体に変換される
    /// </summary>
    public long OwnerId { get; set; }
}

/// <summary>
/// プロジェクト更新用DTO
/// 既存プロジェクト更新時の入力データ構造
/// </summary>
public class UpdateProjectDto
{
    /// <summary>
    /// プロジェクトID
    /// </summary>
    [Required(ErrorMessage = "プロジェクトIDは必須です")]
    public long Id { get; set; }

    /// <summary>
    /// プロジェクト名
    /// </summary>
    [Required(ErrorMessage = "プロジェクト名は必須です")]
    [StringLength(100, ErrorMessage = "プロジェクト名は100文字以内で入力してください")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// プロジェクト説明
    /// </summary>
    [Required(ErrorMessage = "プロジェクト説明は必須です")]
    [StringLength(1000, ErrorMessage = "プロジェクト説明は1000文字以内で入力してください")]
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// アクティブ状態フラグ
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// 更新者ID
    /// </summary>
    public long UpdatedBy { get; set; }
}

/// <summary>
/// プロジェクト作成結果DTO
/// F#ドメインサービスの結果をC#に安全に変換
/// Phase B1: Railway-oriented Programming結果の型安全な変換
/// </summary>
public class ProjectCreationResult
{
    /// <summary>
    /// 作成成功フラグ
    /// </summary>
    public bool IsSuccess { get; set; }

    /// <summary>
    /// 作成されたプロジェクト情報
    /// 成功時のみ設定される
    /// </summary>
    public ProjectDto? Project { get; set; }

    /// <summary>
    /// 自動作成されたデフォルトドメイン情報
    /// 成功時のみ設定される
    /// </summary>
    public DomainDto? DefaultDomain { get; set; }

    /// <summary>
    /// エラーメッセージ
    /// 失敗時のみ設定される
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// エラータイプ
    /// 失敗時の詳細なエラー分類
    /// </summary>
    public ProjectCreationErrorType ErrorType { get; set; }

    /// <summary>
    /// 成功結果を作成
    /// </summary>
    /// <param name="project">作成されたプロジェクト</param>
    /// <param name="defaultDomain">自動作成されたデフォルトドメイン</param>
    /// <returns>成功結果</returns>
    public static ProjectCreationResult Success(ProjectDto project, DomainDto defaultDomain)
    {
        return new ProjectCreationResult
        {
            IsSuccess = true,
            Project = project,
            DefaultDomain = defaultDomain,
            ErrorMessage = null,
            ErrorType = ProjectCreationErrorType.None
        };
    }

    /// <summary>
    /// 失敗結果を作成
    /// </summary>
    /// <param name="errorType">エラータイプ</param>
    /// <param name="errorMessage">エラーメッセージ</param>
    /// <returns>失敗結果</returns>
    public static ProjectCreationResult Failure(ProjectCreationErrorType errorType, string errorMessage)
    {
        return new ProjectCreationResult
        {
            IsSuccess = false,
            Project = null,
            DefaultDomain = null,
            ErrorMessage = errorMessage,
            ErrorType = errorType
        };
    }
}

/// <summary>
/// プロジェクト作成エラータイプ列挙型
/// F#の判別共用体ProjectCreationErrorとの対応関係を保持
/// Phase B1: F#↔C#境界での型安全なエラーハンドリング
/// </summary>
public enum ProjectCreationErrorType
{
    /// <summary>
    /// エラーなし（成功時）
    /// </summary>
    None = 0,

    /// <summary>
    /// プロジェクト名重複エラー
    /// F# DuplicateProjectNameに対応
    /// </summary>
    DuplicateProjectName = 1,

    /// <summary>
    /// 無効なプロジェクト名エラー
    /// F# InvalidProjectNameに対応
    /// </summary>
    InvalidProjectName = 2,

    /// <summary>
    /// 無効なプロジェクト説明エラー
    /// F# InvalidProjectDescriptionに対応
    /// </summary>
    InvalidProjectDescription = 3,

    /// <summary>
    /// データベースエラー
    /// F# DatabaseErrorに対応
    /// </summary>
    DatabaseError = 4,

    /// <summary>
    /// デフォルトドメイン作成失敗エラー
    /// F# DomainCreationFailedに対応
    /// </summary>
    DomainCreationFailed = 5
}