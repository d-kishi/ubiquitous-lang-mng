using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UbiquitousLanguageManager.Infrastructure.Data.Entities;

/// <summary>
/// ユーザーテーブルのEntity Framework エンティティ
/// PostgreSQL データベースのUsersテーブルにマッピング
/// </summary>
[Table("Users")]
public class UserEntity
{
    /// <summary>
    /// ユーザーID（主キー）
    /// PostgreSQL の BIGINT 型（BIGSERIAL）
    /// </summary>
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Id { get; set; }

    /// <summary>
    /// メールアドレス（ログインID）
    /// ユニーク制約あり
    /// </summary>
    [Required]
    [StringLength(254)]  // RFC5321 準拠の最大長
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// パスワードハッシュ
    /// BCrypt でハッシュ化されたパスワード
    /// </summary>
    [Required]
    [StringLength(255)]
    public string PasswordHash { get; set; } = string.Empty;

    /// <summary>
    /// ユーザー名（表示名）
    /// </summary>
    [Required]
    [StringLength(50)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// ユーザーロール
    /// SuperUser, ProjectManager, DomainApprover, GeneralUser
    /// </summary>
    [Required]
    [StringLength(20)]
    public string UserRole { get; set; } = string.Empty;

    /// <summary>
    /// アクティブ状態フラグ
    /// 論理削除に使用
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// 初回ログインフラグ
    /// パスワード変更が必要な状態を示す
    /// </summary>
    public bool IsFirstLogin { get; set; } = true;

    /// <summary>
    /// 最終更新日時（UTC）
    /// PostgreSQL の TIMESTAMPTZ 型
    /// </summary>
    [Required]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// 最終更新者ID
    /// 外部キー制約なし（循環参照回避）
    /// </summary>
    [Required]
    public long UpdatedBy { get; set; }
}