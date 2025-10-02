using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UbiquitousLanguageManager.Infrastructure.Data.Entities;

/// <summary>
/// ドメインエンティティ
/// initスキーマのDomainsテーブルに対応
/// </summary>
[Table("Domains")]
public class Domain
{
    /// <summary>
    /// ドメインID（主キー）
    /// </summary>
    [Key]
    public long DomainId { get; set; }

    /// <summary>
    /// 所属プロジェクトID
    /// </summary>
    public long ProjectId { get; set; }

    /// <summary>
    /// ドメイン名
    /// </summary>
    [Required]
    [MaxLength(30)]
    public string DomainName { get; set; } = string.Empty;

    /// <summary>
    /// ドメイン説明
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// ドメイン所有者ID（Phase B1追加）
    /// F# Domain層のUserId（long型）に対応
    /// </summary>
    [Required]
    public long OwnerId { get; set; }

    /// <summary>
    /// デフォルトドメインフラグ（Phase B1追加）
    /// プロジェクト作成時に自動生成されるデフォルトドメインの場合true
    /// </summary>
    public bool IsDefault { get; set; } = false;

    /// <summary>
    /// 作成日時（Phase B1追加）
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// 最終更新者ID
    /// </summary>
    [Required]
    [MaxLength(450)]
    public string UpdatedBy { get; set; } = string.Empty;

    /// <summary>
    /// 最終更新日時
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// アクティブフラグ（Phase B1追加）
    /// F# Domain層のIsActiveに対応（論理削除の代わり）
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// 論理削除フラグ（旧設計・互換性のため保持）
    /// </summary>
    public bool IsDeleted { get; set; } = false;

    // Navigation properties
    /// <summary>
    /// このドメインが所属するプロジェクト
    /// </summary>
    public virtual Project Project { get; set; } = null!;
    
    /// <summary>
    /// このドメインを最後に更新したユーザー
    /// </summary>
    public virtual ApplicationUser UpdatedByUser { get; set; } = null!;
    
    /// <summary>
    /// このドメインの承認者一覧
    /// ドメインに対する変更承認権限を持つユーザーとの関連付け
    /// </summary>
    public virtual ICollection<DomainApprover> DomainApprovers { get; set; } = new List<DomainApprover>();
    
    /// <summary>
    /// このドメインに属する正式なユビキタス言語一覧
    /// 承認済みのユビキタス言語の集合
    /// </summary>
    public virtual ICollection<FormalUbiquitousLang> FormalUbiquitousLangs { get; set; } = new List<FormalUbiquitousLang>();
    
    /// <summary>
    /// このドメインに属する下書き状態のユビキタス言語一覧
    /// 申請中・レビュー中のユビキタス言語の集合
    /// </summary>
    public virtual ICollection<DraftUbiquitousLang> DraftUbiquitousLangs { get; set; } = new List<DraftUbiquitousLang>();
}