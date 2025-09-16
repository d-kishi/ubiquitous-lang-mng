using System;

namespace UbiquitousLanguageManager.Contracts.DTOs.Authentication;

/// <summary>
/// Phase A9: パスワードリセット結果DTO
/// F# Result&lt;パスワードリセット結果, AuthenticationError&gt;をC#境界で表現
/// Railway-oriented Programmingの結果を型安全に伝達
/// </summary>
public class PasswordResetResultDto
{
    /// <summary>
    /// パスワードリセット成功フラグ
    /// F#のResult型のSuccess/Failureを表現
    /// </summary>
    public bool IsSuccess { get; set; }

    /// <summary>
    /// リセット成功時のメッセージ
    /// ユーザーへの表示用確認メッセージ
    /// </summary>
    public string? SuccessMessage { get; set; }

    /// <summary>
    /// パスワードリセット失敗時のエラー情報
    /// IsSuccess=falseの場合のみ設定される
    /// </summary>
    public AuthenticationErrorDto? Error { get; set; }

    /// <summary>
    /// リセット実行日時（成功時）
    /// 監査・セキュリティログ用のタイムスタンプ
    /// </summary>
    public DateTime? ResetCompletedAt { get; set; }

    /// <summary>
    /// 対象ユーザーのメールアドレス（成功時）
    /// 確認・監査用の情報
    /// </summary>
    public string? UserEmail { get; set; }

    /// <summary>
    /// パスワード変更が必要かどうか
    /// リセット後の初回ログイン処理判定用
    /// </summary>
    public bool RequiresPasswordChange { get; set; } = true;

    /// <summary>
    /// 成功結果を作成するファクトリーメソッド
    /// F# Success caseに対応
    /// </summary>
    /// <param name="userEmail">リセット対象ユーザーのメールアドレス</param>
    /// <param name="message">成功メッセージ（オプション）</param>
    /// <returns>成功を表すPasswordResetResultDto</returns>
    public static PasswordResetResultDto Success(string userEmail, string? message = null)
    {
        return new PasswordResetResultDto
        {
            IsSuccess = true,
            UserEmail = userEmail,
            SuccessMessage = message ?? "パスワードが正常にリセットされました。新しいパスワードでログインしてください。",
            Error = null,
            ResetCompletedAt = DateTime.UtcNow,
            RequiresPasswordChange = true
        };
    }

    /// <summary>
    /// 失敗結果を作成するファクトリーメソッド
    /// F# Error caseに対応
    /// </summary>
    /// <param name="error">エラー情報</param>
    /// <returns>失敗を表すPasswordResetResultDto</returns>
    public static PasswordResetResultDto Failure(AuthenticationErrorDto error)
    {
        return new PasswordResetResultDto
        {
            IsSuccess = false,
            SuccessMessage = null,
            Error = error,
            ResetCompletedAt = null,
            UserEmail = error.Email,
            RequiresPasswordChange = false
        };
    }
}