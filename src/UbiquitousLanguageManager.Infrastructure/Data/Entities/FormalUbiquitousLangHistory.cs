using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UbiquitousLanguageManager.Infrastructure.Data.Entities;

/// <summary>
/// 正式ユビキタス言語履歴エンティティ
/// initスキーマのFormalUbiquitousLangHistoryテーブルに対応
/// </summary>
[Table("FormalUbiquitousLangHistory")]
public class FormalUbiquitousLangHistory
{
    /// <summary>
    /// 履歴ID（主キー）
    /// </summary>
    [Key]
    public long HistoryId { get; set; }

    /// <summary>
    /// 元の正式ユビキタス言語ID
    /// </summary>
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
    /// 関連ユビキタス言語スナップショット（JSONB）
    /// </summary>
    [Column(TypeName = "jsonb")]
    public string? RelatedUbiquitousLangSnapshot { get; set; }

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
    /// この履歴レコードが所属するドメイン
    /// </summary>
    public virtual Domain Domain { get; set; } = null!;
    
    /// <summary>
    /// この履歴レコードを作成したユーザー
    /// 元のユビキタス言語を更新したユーザーの情報
    /// </summary>
    public virtual ApplicationUser UpdatedByUser { get; set; } = null!;
    
    /// <summary>
    /// この履歴レコードの元となった正式なユビキタス言語
    /// 変更前の状態を記録するための参照元エンティティ
    /// </summary>
    public virtual FormalUbiquitousLang FormalUbiquitousLang { get; set; } = null!;
}