using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UbiquitousLanguageManager.Infrastructure.Data.Entities;

/// <summary>
/// 関連ユビキタス言語エンティティ
/// initスキーマのRelatedUbiquitousLangテーブルに対応
/// </summary>
[Table("RelatedUbiquitousLang")]
public class RelatedUbiquitousLang
{
    /// <summary>
    /// 関連ユビキタス言語ID（主キー）
    /// </summary>
    [Key]
    public long RelatedUbiquitousLangId { get; set; }

    /// <summary>
    /// 関連元ユビキタス言語ID
    /// </summary>
    public long SourceUbiquitousLangId { get; set; }

    /// <summary>
    /// 関連先ユビキタス言語ID
    /// </summary>
    public long TargetUbiquitousLangId { get; set; }

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
    /// 関連性の起点となる正式なユビキタス言語
    /// 関連性において主体となるユビキタス言語
    /// </summary>
    public virtual FormalUbiquitousLang SourceUbiquitousLang { get; set; } = null!;
    
    /// <summary>
    /// 関連性の対象となる正式なユビキタス言語
    /// 関連性において客体となるユビキタス言語
    /// </summary>
    public virtual FormalUbiquitousLang TargetUbiquitousLang { get; set; } = null!;
    
    /// <summary>
    /// この関連性を最後に更新したユーザー
    /// </summary>
    public virtual ApplicationUser UpdatedByUser { get; set; } = null!;
}