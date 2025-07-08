using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace UbiquitousLanguageManager.Contracts.DTOs;

/// <summary>
/// プロジェクト情報のデータ転送オブジェクト
/// F#ドメインエンティティとC#プレゼンテーション層の境界で使用
/// </summary>
public class ProjectDto
{
    /// <summary>
    /// プロジェクトID（主キー）
    /// </summary>
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
    /// 最終更新日時（UTC）
    /// </summary>
    public DateTime UpdatedAt { get; set; }

    /// <summary>
    /// 最終更新者ID
    /// </summary>
    public long UpdatedBy { get; set; }

    /// <summary>
    /// 関連するドメイン一覧（参照用）
    /// </summary>
    public List<DomainDto> Domains { get; set; } = new();

    /// <summary>
    /// プロジェクト参加ユーザー数（参照用）
    /// </summary>
    public int MemberCount { get; set; }
}

/// <summary>
/// プロジェクト作成用DTO
/// 新規プロジェクト作成時の入力データ構造
/// </summary>
public class CreateProjectDto
{
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
    /// 作成者ID
    /// </summary>
    public long CreatedBy { get; set; }
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