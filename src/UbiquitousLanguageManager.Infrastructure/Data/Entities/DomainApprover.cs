using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UbiquitousLanguageManager.Infrastructure.Data.Entities;

/// <summary>
/// ドメイン承認者エンティティ
/// initスキーマのDomainApproversテーブルに対応
/// </summary>
[Table("DomainApprovers")]
public class DomainApprover
{
    /// <summary>
    /// ドメイン承認者ID（主キー）
    /// </summary>
    [Key]
    public long DomainApproverId { get; set; }

    /// <summary>
    /// ドメインID
    /// </summary>
    public long DomainId { get; set; }

    /// <summary>
    /// 承認者ユーザーID
    /// </summary>
    [Required]
    [MaxLength(450)]
    public string ApproverId { get; set; } = string.Empty;

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
    /// 承認対象のドメイン
    /// </summary>
    public virtual Domain Domain { get; set; } = null!;
    
    /// <summary>
    /// 承認者ユーザー
    /// このドメインに対する承認権限を持つユーザー
    /// </summary>
    public virtual ApplicationUser Approver { get; set; } = null!;
    
    /// <summary>
    /// この承認者情報を最後に更新したユーザー
    /// </summary>
    public virtual ApplicationUser UpdatedByUser { get; set; } = null!;
}