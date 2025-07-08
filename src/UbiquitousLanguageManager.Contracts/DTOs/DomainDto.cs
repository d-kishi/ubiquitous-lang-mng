using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace UbiquitousLanguageManager.Contracts.DTOs;

/// <summary>
/// ドメイン情報のデータ転送オブジェクト
/// F#ドメインエンティティとC#プレゼンテーション層の境界で使用
/// </summary>
public class DomainDto
{
    /// <summary>
    /// ドメインID（主キー）
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// 所属プロジェクトID
    /// </summary>
    public long ProjectId { get; set; }

    /// <summary>
    /// ドメイン名
    /// </summary>
    [Required(ErrorMessage = "ドメイン名は必須です")]
    [StringLength(100, ErrorMessage = "ドメイン名は100文字以内で入力してください")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// ドメイン説明
    /// </summary>
    [Required(ErrorMessage = "ドメイン説明は必須です")]
    [StringLength(1000, ErrorMessage = "ドメイン説明は1000文字以内で入力してください")]
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
    /// 所属プロジェクト情報（参照用）
    /// </summary>
    public ProjectDto? Project { get; set; }

    /// <summary>
    /// ドメイン内のユビキタス言語数（参照用）
    /// </summary>
    public int UbiquitousLanguageCount { get; set; }

    /// <summary>
    /// 承認待ちユビキタス言語数（参照用）
    /// </summary>
    public int PendingApprovalCount { get; set; }
}

/// <summary>
/// ドメイン作成用DTO
/// 新規ドメイン作成時の入力データ構造
/// </summary>
public class CreateDomainDto
{
    /// <summary>
    /// 所属プロジェクトID
    /// </summary>
    [Required(ErrorMessage = "プロジェクトIDは必須です")]
    public long ProjectId { get; set; }

    /// <summary>
    /// ドメイン名
    /// </summary>
    [Required(ErrorMessage = "ドメイン名は必須です")]
    [StringLength(100, ErrorMessage = "ドメイン名は100文字以内で入力してください")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// ドメイン説明
    /// </summary>
    [Required(ErrorMessage = "ドメイン説明は必須です")]
    [StringLength(1000, ErrorMessage = "ドメイン説明は1000文字以内で入力してください")]
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// 作成者ID
    /// </summary>
    public long CreatedBy { get; set; }
}

/// <summary>
/// ドメイン更新用DTO
/// 既存ドメイン更新時の入力データ構造
/// </summary>
public class UpdateDomainDto
{
    /// <summary>
    /// ドメインID
    /// </summary>
    [Required(ErrorMessage = "ドメインIDは必須です")]
    public long Id { get; set; }

    /// <summary>
    /// ドメイン名
    /// </summary>
    [Required(ErrorMessage = "ドメイン名は必須です")]
    [StringLength(100, ErrorMessage = "ドメイン名は100文字以内で入力してください")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// ドメイン説明
    /// </summary>
    [Required(ErrorMessage = "ドメイン説明は必須です")]
    [StringLength(1000, ErrorMessage = "ドメイン説明は1000文字以内で入力してください")]
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