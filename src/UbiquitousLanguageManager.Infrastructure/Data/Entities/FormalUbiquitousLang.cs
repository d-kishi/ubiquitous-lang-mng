using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UbiquitousLanguageManager.Infrastructure.Data.Entities;

/// <summary>
/// 正式ユビキタス言語エンティティ
/// initスキーマのFormalUbiquitousLangテーブルに対応
/// </summary>
[Table("FormalUbiquitousLang")]
public class FormalUbiquitousLang
{
    /// <summary>
    /// 正式ユビキタス言語ID（主キー）
    /// </summary>
    [Key]
    public long FormalUbiquitousLangId { get; set; }

    /// <summary>
    /// 所属ドメインID
    /// </summary>
    public long DomainId { get; set; }

    /// <summary>
    /// 和名
    /// </summary>
    [Required]
    [MaxLength(30)]
    public string JapaneseName { get; set; } = string.Empty;

    /// <summary>
    /// 英名
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string EnglishName { get; set; } = string.Empty;

    /// <summary>
    /// 意味・説明（改行可能）
    /// </summary>
    [Required]
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// 発生機会
    /// </summary>
    [MaxLength(50)]
    public string? OccurrenceContext { get; set; }

    /// <summary>
    /// 備考（改行可能）
    /// </summary>
    public string? Remarks { get; set; }

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
    /// この正式なユビキタス言語が所属するドメイン
    /// </summary>
    public virtual Domain Domain { get; set; } = null!;
    
    /// <summary>
    /// この正式なユビキタス言語を最後に更新したユーザー
    /// </summary>
    public virtual ApplicationUser UpdatedByUser { get; set; } = null!;
    
    /// <summary>
    /// この正式なユビキタス言語が関係の起点となる関連性一覧
    /// 他のユビキタス言語との関連性において、このユビキタス言語が起点（Source）となる関係の集合
    /// </summary>
    public virtual ICollection<RelatedUbiquitousLang> SourceRelations { get; set; } = new List<RelatedUbiquitousLang>();
    
    /// <summary>
    /// この正式なユビキタス言語が関係の対象となる関連性一覧
    /// 他のユビキタス言語との関連性において、このユビキタス言語が対象（Target）となる関係の集合
    /// </summary>
    public virtual ICollection<RelatedUbiquitousLang> TargetRelations { get; set; } = new List<RelatedUbiquitousLang>();
    
    /// <summary>
    /// この正式なユビキタス言語と関連付けられた下書きユビキタス言語の関係一覧
    /// 下書き状態のユビキタス言語がこの正式なユビキタス言語と関連性を持つ場合の関係の集合
    /// </summary>
    public virtual ICollection<DraftUbiquitousLangRelation> DraftRelations { get; set; } = new List<DraftUbiquitousLangRelation>();
    
    /// <summary>
    /// この正式なユビキタス言語の変更履歴一覧
    /// データ変更時の履歴管理用のエンティティ集合
    /// </summary>
    public virtual ICollection<FormalUbiquitousLangHistory> Histories { get; set; } = new List<FormalUbiquitousLangHistory>();
}