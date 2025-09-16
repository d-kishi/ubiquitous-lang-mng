using System;
using System.ComponentModel.DataAnnotations;

namespace UbiquitousLanguageManager.Contracts.DTOs.Authentication;

/// <summary>
/// Phase A9: パスワードリセットトークンDTO
/// パスワードリセットトークンの情報を型安全に表現
/// F#のトークン情報をC#境界で扱うためのDTO
/// </summary>
public class PasswordResetTokenDto
{
    /// <summary>
    /// パスワードリセットトークン
    /// セキュアランダム文字列・URL安全な形式
    /// </summary>
    [Required(ErrorMessage = "リセットトークンは必須です")]
    public string Token { get; set; } = string.Empty;

    /// <summary>
    /// トークンの対象ユーザーメールアドレス
    /// F#のEmail値オブジェクトとの変換に使用
    /// </summary>
    [Required(ErrorMessage = "メールアドレスは必須です")]
    [EmailAddress(ErrorMessage = "有効なメールアドレス形式で入力してください")]
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// トークンの有効期限
    /// セキュリティ上の理由により短期間（通常15-30分）
    /// </summary>
    public DateTime ExpiresAt { get; set; }

    /// <summary>
    /// 新しいパスワード
    /// リセット実行時にのみ設定される
    /// </summary>
    [MinLength(8, ErrorMessage = "パスワードは8文字以上である必要があります")]
    public string? NewPassword { get; set; }

    /// <summary>
    /// トークン生成日時（監査用）
    /// セキュリティログ・トレーサビリティ確保
    /// </summary>
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// トークンが使用済みかどうか
    /// 一度使用されたトークンの再利用防止
    /// </summary>
    public bool IsUsed { get; set; } = false;
}