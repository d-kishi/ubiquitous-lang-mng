using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UbiquitousLanguageManager.Infrastructure.Data.Entities;

/// <summary>
/// ドラフトユビキタス言語関連エンティティ
/// initスキーマのDraftUbiquitousLangRelationsテーブルに対応
/// </summary>
[Table("DraftUbiquitousLangRelations")]
public class DraftUbiquitousLangRelation
{
    /// <summary>
    /// ドラフト関連ID（主キー）
    /// </summary>
    [Key]
    public long DraftUbiquitousLangRelationId { get; set; }

    /// <summary>
    /// ドラフトユビキタス言語ID
    /// </summary>
    public long DraftUbiquitousLangId { get; set; }

    /// <summary>
    /// 関連正式ユビキタス言語ID
    /// </summary>
    public long FormalUbiquitousLangId { get; set; }

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
    /// 関連付けの主体となる下書きユビキタス言語
    /// </summary>
    public virtual DraftUbiquitousLang DraftUbiquitousLang { get; set; } = null!;
    
    /// <summary>
    /// 関連付けの対象となる正式なユビキタス言語
    /// 下書きユビキタス言語と関連性を持つ既存の正式なユビキタス言語
    /// </summary>
    public virtual FormalUbiquitousLang FormalUbiquitousLang { get; set; } = null!;
    
    /// <summary>
    /// この関連付けを最後に更新したユーザー
    /// </summary>
    public virtual ApplicationUser UpdatedByUser { get; set; } = null!;
}