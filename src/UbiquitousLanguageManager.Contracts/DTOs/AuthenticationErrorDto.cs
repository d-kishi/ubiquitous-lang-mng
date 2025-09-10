using System;
using System.Collections.Generic;

namespace UbiquitousLanguageManager.Contracts.DTOs;

/// <summary>
/// Phase A9: 認証エラーDTO
/// F#のAuthenticationError判別共用体をC#境界で表現
/// 型安全なエラーハンドリングとパターンマッチング対応
/// 【Blazor Server初学者向け解説】
/// F#の判別共用体は、エラーの種類を型レベルで区別できる強力な機能です。
/// このDTOは、その型安全性をC#側でも活用できるよう設計されています。
/// </summary>
public class AuthenticationErrorDto
{
    /// <summary>
    /// エラータイプ（F#判別共用体のケースに対応）
    /// パターンマッチングのための識別子
    /// </summary>
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// エラーメッセージ
    /// ユーザーへの表示・ログ出力用の説明文
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// 関連するメールアドレス（該当する場合）
    /// UserNotFound・AccountLocked等で設定
    /// </summary>
    public string? Email { get; set; }

    /// <summary>
    /// ロックアウト終了時刻（AccountLockedの場合）
    /// アカウントロック関連エラーでの詳細情報
    /// </summary>
    public DateTime? LockoutEnd { get; set; }

    /// <summary>
    /// システムエラーの詳細情報（SystemErrorの場合）
    /// 例外情報・スタックトレース等の技術詳細
    /// </summary>
    public Dictionary<string, string> SystemDetails { get; set; } = new();

    /// <summary>
    /// エラーが発生した日時
    /// 監査・デバッグ用のタイムスタンプ
    /// </summary>
    public DateTime OccurredAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// InvalidCredentialsエラーを作成
    /// F#のInvalidCredentialsケースに対応
    /// </summary>
    /// <returns>認証情報不正エラーDTO</returns>
    public static AuthenticationErrorDto InvalidCredentials()
    {
        return new AuthenticationErrorDto
        {
            Type = "InvalidCredentials",
            Message = "認証情報が正しくありません。メールアドレスとパスワードを確認してください。"
        };
    }

    /// <summary>
    /// UserNotFoundエラーを作成
    /// F#のUserNotFound of Emailケースに対応
    /// </summary>
    /// <param name="email">見つからなかったユーザーのメールアドレス</param>
    /// <returns>ユーザー不存在エラーDTO</returns>
    public static AuthenticationErrorDto UserNotFound(string email)
    {
        return new AuthenticationErrorDto
        {
            Type = "UserNotFound",
            Message = "指定されたメールアドレスのユーザーが見つかりません。",
            Email = email
        };
    }

    /// <summary>
    /// ValidationErrorエラーを作成
    /// F#のValidationError of stringケースに対応
    /// </summary>
    /// <param name="validationMessage">バリデーション失敗の詳細</param>
    /// <returns>バリデーションエラーDTO</returns>
    public static AuthenticationErrorDto ValidationError(string validationMessage)
    {
        return new AuthenticationErrorDto
        {
            Type = "ValidationError",
            Message = $"入力データの検証に失敗しました: {validationMessage}"
        };
    }

    /// <summary>
    /// AccountLockedエラーを作成
    /// F#のAccountLocked of Email * DateTimeケースに対応
    /// </summary>
    /// <param name="email">ロックされたアカウントのメールアドレス</param>
    /// <param name="lockoutEnd">ロックアウト終了時刻</param>
    /// <returns>アカウントロックエラーDTO</returns>
    public static AuthenticationErrorDto AccountLocked(string email, DateTime lockoutEnd)
    {
        return new AuthenticationErrorDto
        {
            Type = "AccountLocked",
            Message = $"アカウントがロックされています。{lockoutEnd:yyyy/MM/dd HH:mm} まで待ってから再試行してください。",
            Email = email,
            LockoutEnd = lockoutEnd
        };
    }

    /// <summary>
    /// SystemErrorエラーを作成
    /// F#のSystemError of exnケースに対応
    /// </summary>
    /// <param name="exception">発生したシステム例外</param>
    /// <returns>システムエラーDTO</returns>
    public static AuthenticationErrorDto SystemError(Exception exception)
    {
        var systemDetails = new Dictionary<string, string>
        {
            ["ExceptionType"] = exception.GetType().Name,
            ["StackTrace"] = exception.StackTrace ?? string.Empty,
            ["Source"] = exception.Source ?? string.Empty
        };

        if (exception.InnerException != null)
        {
            systemDetails["InnerExceptionType"] = exception.InnerException.GetType().Name;
            systemDetails["InnerExceptionMessage"] = exception.InnerException.Message;
        }

        return new AuthenticationErrorDto
        {
            Type = "SystemError",
            Message = "システムエラーが発生しました。管理者にお問い合わせください。",
            SystemDetails = systemDetails
        };
    }

    /// <summary>
    /// PasswordExpiredエラーを作成
    /// F#のPasswordExpired of Emailケースに対応
    /// </summary>
    /// <param name="email">パスワード期限切れのユーザーメールアドレス</param>
    /// <returns>パスワード期限切れエラーDTO</returns>
    public static AuthenticationErrorDto PasswordExpired(string email)
    {
        return new AuthenticationErrorDto
        {
            Type = "PasswordExpired",
            Message = "パスワードの有効期限が切れています。パスワードを変更してください。",
            Email = email
        };
    }

    /// <summary>
    /// TwoFactorRequiredエラーを作成
    /// F#のTwoFactorRequired of Emailケースに対応
    /// </summary>
    /// <param name="email">二要素認証が必要なユーザーのメールアドレス</param>
    /// <returns>二要素認証必要エラーDTO</returns>
    public static AuthenticationErrorDto TwoFactorRequired(string email)
    {
        return new AuthenticationErrorDto
        {
            Type = "TwoFactorRequired",
            Message = "二要素認証コードを入力してください。",
            Email = email
        };
    }
}