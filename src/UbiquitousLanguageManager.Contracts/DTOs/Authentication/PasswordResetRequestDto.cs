using System.ComponentModel.DataAnnotations;

namespace UbiquitousLanguageManager.Contracts.DTOs.Authentication;

/// <summary>
/// Phase A9: パスワードリセット要求DTO
/// Web層からのパスワードリセット要求を型安全に表現
/// F#↔C#境界でのデータ転送に使用
/// </summary>
public class PasswordResetRequestDto
{
    /// <summary>
    /// パスワードリセット要求ユーザーのメールアドレス
    /// F#のEmail値オブジェクトにマッピングされる
    /// </summary>
    [Required(ErrorMessage = "メールアドレスは必須です")]
    [EmailAddress(ErrorMessage = "有効なメールアドレス形式で入力してください")]
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// リセット要求の理由（オプション）
    /// パスワード忘失・管理者要求等の記録用
    /// </summary>
    public string? Reason { get; set; }

    /// <summary>
    /// 要求者のIPアドレス（セキュリティ監査用）
    /// 不正なリセット要求の検知に使用
    /// </summary>
    public string? RequestorIP { get; set; }
}