using System.ComponentModel.DataAnnotations;

namespace UbiquitousLanguageManager.Contracts.DTOs;

/// <summary>
/// ユビキタス言語のデータ転送オブジェクト
/// F#ドメインエンティティとC#プレゼンテーション層の境界で使用
/// </summary>
public class UbiquitousLanguageDto
{
    /// <summary>
    /// ユビキタス言語ID（主キー）
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// 所属ドメインID
    /// </summary>
    public long DomainId { get; set; }

    /// <summary>
    /// 日本語名
    /// </summary>
    [Required(ErrorMessage = "日本語名は必須です")]
    [StringLength(100, ErrorMessage = "日本語名は100文字以内で入力してください")]
    public string JapaneseName { get; set; } = string.Empty;

    /// <summary>
    /// 英語名
    /// </summary>
    [Required(ErrorMessage = "英語名は必須です")]
    [StringLength(100, ErrorMessage = "英語名は100文字以内で入力してください")]
    public string EnglishName { get; set; } = string.Empty;

    /// <summary>
    /// 説明文
    /// </summary>
    [Required(ErrorMessage = "説明は必須です")]
    [StringLength(1000, ErrorMessage = "説明は1000文字以内で入力してください")]
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// 承認状態
    /// Draft, Submitted, Approved, Rejected のいずれか
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// 最終更新日時（UTC）
    /// </summary>
    public DateTime UpdatedAt { get; set; }

    /// <summary>
    /// 最終更新者ID
    /// </summary>
    public long UpdatedBy { get; set; }

    /// <summary>
    /// 承認日時（正式版のみ）
    /// </summary>
    public DateTime? ApprovedAt { get; set; }

    /// <summary>
    /// 承認者ID（正式版のみ）
    /// </summary>
    public long? ApprovedBy { get; set; }
}

/// <summary>
/// ユビキタス言語作成用DTO
/// 新規用語作成時の入力データ構造
/// </summary>
public class CreateUbiquitousLanguageDto
{
    /// <summary>
    /// 所属ドメインID
    /// </summary>
    [Required(ErrorMessage = "ドメインIDは必須です")]
    public long DomainId { get; set; }

    /// <summary>
    /// 日本語名
    /// </summary>
    [Required(ErrorMessage = "日本語名は必須です")]
    [StringLength(100, ErrorMessage = "日本語名は100文字以内で入力してください")]
    public string JapaneseName { get; set; } = string.Empty;

    /// <summary>
    /// 英語名
    /// </summary>
    [Required(ErrorMessage = "英語名は必須です")]
    [StringLength(100, ErrorMessage = "英語名は100文字以内で入力してください")]
    public string EnglishName { get; set; } = string.Empty;

    /// <summary>
    /// 説明文
    /// </summary>
    [Required(ErrorMessage = "説明は必須です")]
    [StringLength(1000, ErrorMessage = "説明は1000文字以内で入力してください")]
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// 作成者ID
    /// </summary>
    public long CreatedBy { get; set; }
}

/// <summary>
/// 承認申請用DTO
/// ワークフロー開始時の入力データ構造
/// </summary>
public class SubmitForApprovalDto
{
    /// <summary>
    /// ユビキタス言語ID
    /// </summary>
    [Required(ErrorMessage = "ユビキタス言語IDは必須です")]
    public long UbiquitousLanguageId { get; set; }

    /// <summary>
    /// 申請者ID
    /// </summary>
    public long SubmittedBy { get; set; }

    /// <summary>
    /// 申請時のコメント（オプション）
    /// </summary>
    [StringLength(500, ErrorMessage = "コメントは500文字以内で入力してください")]
    public string? Comment { get; set; }
}

/// <summary>
/// 承認処理用DTO
/// 承認者による最終決定のための入力データ構造
/// </summary>
public class ApprovalDto
{
    /// <summary>
    /// ユビキタス言語ID
    /// </summary>
    [Required(ErrorMessage = "ユビキタス言語IDは必須です")]
    public long UbiquitousLanguageId { get; set; }

    /// <summary>
    /// 承認者ID
    /// </summary>
    public long ApprovedBy { get; set; }

    /// <summary>
    /// 承認・却下フラグ（true: 承認, false: 却下）
    /// </summary>
    public bool IsApproved { get; set; }

    /// <summary>
    /// 承認・却下時のコメント（オプション）
    /// </summary>
    [StringLength(500, ErrorMessage = "コメントは500文字以内で入力してください")]
    public string? ApprovalComment { get; set; }
}