using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UbiquitousLanguageManager.Infrastructure.Data.Entities;

/// <summary>
/// ドラフトユビキタス言語エンティティ
/// initスキーマのDraftUbiquitousLangテーブルに対応
/// </summary>
[Table("DraftUbiquitousLang")]
public class DraftUbiquitousLang
{
    /// <summary>
    /// ドラフトユビキタス言語ID（主キー）
    /// </summary>
    [Key]
    public long DraftUbiquitousLangId { get; set; }

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
    [MaxLength(50)]
    public string? EnglishName { get; set; }

    /// <summary>
    /// 意味・説明（改行可能）
    /// </summary>
    public string? Description { get; set; }

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
    /// ステータス
    /// </summary>
    [Required]
    [MaxLength(20)]
    public string Status { get; set; } = "Draft";

    /// <summary>
    /// 申請者ID
    /// </summary>
    [MaxLength(450)]
    public string? ApplicantId { get; set; }

    /// <summary>
    /// 申請日時
    /// </summary>
    public DateTime? ApplicationDate { get; set; }

    /// <summary>
    /// 却下理由
    /// </summary>
    public string? RejectionReason { get; set; }

    /// <summary>
    /// 編集元正式ユビキタス言語ID
    /// </summary>
    public long? SourceFormalUbiquitousLangId { get; set; }

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
    /// この下書きユビキタス言語が所属するドメイン
    /// </summary>
    public virtual Domain Domain { get; set; } = null!;
    
    /// <summary>
    /// この下書きユビキタス言語を申請したユーザー
    /// 下書き状態の場合はnull、申請済みの場合はユーザーが設定される
    /// </summary>
    public virtual ApplicationUser? Applicant { get; set; }
    
    /// <summary>
    /// この下書きユビキタス言語を最後に更新したユーザー
    /// </summary>
    public virtual ApplicationUser UpdatedByUser { get; set; } = null!;
    
    /// <summary>
    /// 編集元となった正式なユビキタス言語
    /// 既存の正式なユビキタス言語を編集する場合に設定される
    /// </summary>
    public virtual FormalUbiquitousLang? SourceFormalUbiquitousLang { get; set; }
    
    /// <summary>
    /// この下書きユビキタス言語に関連付けられた関係性一覧
    /// 他のユビキタス言語との関連性（類義語、対義語等）の下書き状態
    /// </summary>
    public virtual ICollection<DraftUbiquitousLangRelation> DraftRelations { get; set; } = new List<DraftUbiquitousLangRelation>();
}