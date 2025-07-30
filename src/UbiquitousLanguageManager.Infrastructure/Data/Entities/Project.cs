using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UbiquitousLanguageManager.Infrastructure.Data.Entities;

/// <summary>
/// プロジェクトエンティティ
/// initスキーマのProjectsテーブルに対応
/// </summary>
[Table("Projects")]
public class Project
{
    /// <summary>
    /// プロジェクトID（主キー）
    /// </summary>
    [Key]
    public long ProjectId { get; set; }

    /// <summary>
    /// プロジェクト名（システム内一意）
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string ProjectName { get; set; } = string.Empty;

    /// <summary>
    /// プロジェクト説明
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// 最終更新者ID
    /// </summary>
    [Required]
    [MaxLength(450)]
    public string UpdatedBy { get; set; } = string.Empty;

    /// <summary>
    /// 最終更新日時
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// 論理削除フラグ
    /// </summary>
    public bool IsDeleted { get; set; } = false;

    // Navigation properties
    /// <summary>
    /// このプロジェクトを最後に更新したユーザー
    /// </summary>
    public virtual ApplicationUser UpdatedByUser { get; set; } = null!;
    
    /// <summary>
    /// このプロジェクトに参加するユーザーとの関連付け一覧
    /// UserProjectエンティティを通じた多対多の関係
    /// </summary>
    public virtual ICollection<UserProject> UserProjects { get; set; } = new List<UserProject>();
    
    /// <summary>
    /// このプロジェクトに所属するドメイン一覧
    /// プロジェクト配下の業務ドメインの集合
    /// </summary>
    public virtual ICollection<Domain> Domains { get; set; } = new List<Domain>();
}