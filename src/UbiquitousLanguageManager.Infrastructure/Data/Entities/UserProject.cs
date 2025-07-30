using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UbiquitousLanguageManager.Infrastructure.Data.Entities;

/// <summary>
/// ユーザー・プロジェクト関連エンティティ
/// initスキーマのUserProjectsテーブルに対応
/// </summary>
[Table("UserProjects")]
public class UserProject
{
    /// <summary>
    /// ユーザープロジェクトID（主キー）
    /// </summary>
    [Key]
    public long UserProjectId { get; set; }

    /// <summary>
    /// ユーザーID
    /// </summary>
    [Required]
    [MaxLength(450)]
    public string UserId { get; set; } = string.Empty;

    /// <summary>
    /// プロジェクトID
    /// </summary>
    public long ProjectId { get; set; }

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

    // Navigation properties
    /// <summary>
    /// プロジェクトに参加するユーザー
    /// </summary>
    public virtual ApplicationUser User { get; set; } = null!;
    
    /// <summary>
    /// ユーザーが参加するプロジェクト
    /// </summary>
    public virtual Project Project { get; set; } = null!;
    
    /// <summary>
    /// このユーザー・プロジェクト関連付けを最後に更新したユーザー
    /// </summary>
    public virtual ApplicationUser UpdatedByUser { get; set; } = null!;
}