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

    // =================================================================
    // Phase A9: パスワードリセット関連エラー（将来のF#拡張対応）
    // =================================================================

    /// <summary>
    /// PasswordResetTokenExpiredエラーを作成
    /// パスワードリセットトークンが期限切れ
    /// </summary>
    /// <param name="email">対象ユーザーのメールアドレス</param>
    /// <returns>トークン期限切れエラーDTO</returns>
    public static AuthenticationErrorDto PasswordResetTokenExpired(string email)
    {
        return new AuthenticationErrorDto
        {
            Type = "PasswordResetTokenExpired",
            Message = "パスワードリセットトークンの有効期限が切れています。再度リセット要求を行ってください。",
            Email = email
        };
    }

    /// <summary>
    /// PasswordResetTokenInvalidエラーを作成
    /// 無効なパスワードリセットトークン
    /// </summary>
    /// <param name="email">対象ユーザーのメールアドレス</param>
    /// <returns>無効トークンエラーDTO</returns>
    public static AuthenticationErrorDto PasswordResetTokenInvalid(string email)
    {
        return new AuthenticationErrorDto
        {
            Type = "PasswordResetTokenInvalid",
            Message = "無効なパスワードリセットトークンです。正しいリンクをクリックしてください。",
            Email = email
        };
    }

    /// <summary>
    /// PasswordResetNotRequestedエラーを作成
    /// パスワードリセットが要求されていない
    /// </summary>
    /// <param name="email">対象ユーザーのメールアドレス</param>
    /// <returns>リセット未要求エラーDTO</returns>
    public static AuthenticationErrorDto PasswordResetNotRequested(string email)
    {
        return new AuthenticationErrorDto
        {
            Type = "PasswordResetNotRequested",
            Message = "このユーザーはパスワードリセットを要求していません。",
            Email = email
        };
    }

    /// <summary>
    /// PasswordResetAlreadyUsedエラーを作成
    /// パスワードリセットトークンが既に使用済み
    /// </summary>
    /// <param name="email">対象ユーザーのメールアドレス</param>
    /// <returns>トークン使用済みエラーDTO</returns>
    public static AuthenticationErrorDto PasswordResetAlreadyUsed(string email)
    {
        return new AuthenticationErrorDto
        {
            Type = "PasswordResetAlreadyUsed",
            Message = "このパスワードリセットトークンは既に使用されています。新しいリセット要求を行ってください。",
            Email = email
        };
    }

    // =================================================================
    // Phase A9: トークン関連エラー（将来のF#拡張対応）
    // =================================================================

    /// <summary>
    /// TokenGenerationFailedエラーを作成
    /// トークン生成処理の失敗
    /// </summary>
    /// <param name="reason">生成失敗の理由</param>
    /// <returns>トークン生成失敗エラーDTO</returns>
    public static AuthenticationErrorDto TokenGenerationFailed(string? reason = null)
    {
        return new AuthenticationErrorDto
        {
            Type = "TokenGenerationFailed",
            Message = $"認証トークンの生成に失敗しました。{reason ?? "システム管理者にお問い合わせください。"}"
        };
    }

    /// <summary>
    /// TokenValidationFailedエラーを作成
    /// トークン検証処理の失敗
    /// </summary>
    /// <param name="reason">検証失敗の理由</param>
    /// <returns>トークン検証失敗エラーDTO</returns>
    public static AuthenticationErrorDto TokenValidationFailed(string? reason = null)
    {
        return new AuthenticationErrorDto
        {
            Type = "TokenValidationFailed",
            Message = $"認証トークンの検証に失敗しました。{reason ?? "再度ログインしてください。"}"
        };
    }

    /// <summary>
    /// TokenExpiredエラーを作成
    /// 認証トークンの期限切れ
    /// </summary>
    /// <param name="expiredAt">期限切れ日時</param>
    /// <returns>トークン期限切れエラーDTO</returns>
    public static AuthenticationErrorDto TokenExpired(DateTime? expiredAt = null)
    {
        var expiredTime = expiredAt?.ToString("yyyy/MM/dd HH:mm") ?? "不明";
        return new AuthenticationErrorDto
        {
            Type = "TokenExpired",
            Message = $"認証トークンが期限切れです（期限: {expiredTime}）。再度ログインしてください。"
        };
    }

    /// <summary>
    /// TokenRevokedエラーを作成
    /// トークンが無効化されている
    /// </summary>
    /// <param name="reason">無効化の理由</param>
    /// <returns>トークン無効化エラーDTO</returns>
    public static AuthenticationErrorDto TokenRevoked(string? reason = null)
    {
        return new AuthenticationErrorDto
        {
            Type = "TokenRevoked",
            Message = $"認証トークンが無効化されています。{reason ?? "再度ログインしてください。"}"
        };
    }

    // =================================================================
    // Phase A9: 管理者操作関連エラー（将来のF#拡張対応）
    // =================================================================

    /// <summary>
    /// InsufficientPermissionsエラーを作成
    /// 権限不足による操作拒否
    /// </summary>
    /// <param name="requiredPermission">必要な権限</param>
    /// <param name="userEmail">操作ユーザーのメールアドレス</param>
    /// <returns>権限不足エラーDTO</returns>
    public static AuthenticationErrorDto InsufficientPermissions(string requiredPermission, string? userEmail = null)
    {
        return new AuthenticationErrorDto
        {
            Type = "InsufficientPermissions",
            Message = $"この操作には {requiredPermission} 権限が必要です。",
            Email = userEmail
        };
    }

    /// <summary>
    /// OperationNotAllowedエラーを作成
    /// 操作が許可されていない
    /// </summary>
    /// <param name="operation">実行しようとした操作</param>
    /// <param name="reason">許可されない理由</param>
    /// <returns>操作不許可エラーDTO</returns>
    public static AuthenticationErrorDto OperationNotAllowed(string operation, string? reason = null)
    {
        return new AuthenticationErrorDto
        {
            Type = "OperationNotAllowed",
            Message = $"操作 '{operation}' は許可されていません。{reason ?? ""}"
        };
    }

    /// <summary>
    /// ConcurrentOperationDetectedエラーを作成
    /// 同時操作の競合検出
    /// </summary>
    /// <param name="conflictingOperation">競合する操作</param>
    /// <returns>同時操作競合エラーDTO</returns>
    public static AuthenticationErrorDto ConcurrentOperationDetected(string? conflictingOperation = null)
    {
        return new AuthenticationErrorDto
        {
            Type = "ConcurrentOperationDetected",
            Message = $"同時操作が検出されました。{conflictingOperation ?? ""} しばらく待ってから再試行してください。"
        };
    }

    // =================================================================
    // Phase A9: 将来拡張用エラー（二要素認証・外部認証対応）
    // =================================================================

    /// <summary>
    /// AccountDeactivatedエラーを作成
    /// F#のAccountDeactivatedケースに対応
    /// </summary>
    /// <returns>アカウント無効化エラーDTO</returns>
    public static AuthenticationErrorDto AccountDeactivated()
    {
        return new AuthenticationErrorDto
        {
            Type = "AccountDeactivated",
            Message = "このアカウントは無効化されています。管理者にお問い合わせください。"
        };
    }

    /// <summary>
    /// TwoFactorAuthRequiredエラーを作成
    /// 二要素認証が必要（将来実装予定）
    /// </summary>
    /// <param name="email">対象ユーザーのメールアドレス</param>
    /// <returns>二要素認証必要エラーDTO</returns>
    public static AuthenticationErrorDto TwoFactorAuthRequired(string email)
    {
        return new AuthenticationErrorDto
        {
            Type = "TwoFactorAuthRequired",
            Message = "二要素認証が必要です。認証コードを入力してください。",
            Email = email
        };
    }

    /// <summary>
    /// TwoFactorAuthFailedエラーを作成
    /// 二要素認証の失敗
    /// </summary>
    /// <param name="email">対象ユーザーのメールアドレス</param>
    /// <returns>二要素認証失敗エラーDTO</returns>
    public static AuthenticationErrorDto TwoFactorAuthFailed(string email)
    {
        return new AuthenticationErrorDto
        {
            Type = "TwoFactorAuthFailed",
            Message = "二要素認証コードが正しくありません。再入力してください。",
            Email = email
        };
    }

    /// <summary>
    /// ExternalAuthenticationFailedエラーを作成
    /// 外部認証（OAuth/OIDC等）の失敗
    /// </summary>
    /// <param name="provider">認証プロバイダー名</param>
    /// <param name="reason">失敗理由</param>
    /// <returns>外部認証失敗エラーDTO</returns>
    public static AuthenticationErrorDto ExternalAuthenticationFailed(string provider, string? reason = null)
    {
        return new AuthenticationErrorDto
        {
            Type = "ExternalAuthenticationFailed",
            Message = $"{provider} 認証に失敗しました。{reason ?? "再試行してください。"}"
        };
    }

    /// <summary>
    /// AuditLogErrorエラーを作成
    /// 監査ログの記録失敗
    /// </summary>
    /// <param name="operation">監査対象の操作</param>
    /// <param name="reason">記録失敗の理由</param>
    /// <returns>監査ログエラーDTO</returns>
    public static AuthenticationErrorDto AuditLogError(string operation, string? reason = null)
    {
        return new AuthenticationErrorDto
        {
            Type = "AuditLogError",
            Message = $"監査ログの記録に失敗しました。操作: {operation} {reason ?? ""}"
        };
    }

    // =================================================================
    // Phase A9: メソッドオーバーロード調整（AuthenticationConverterとの互換性確保）
    // =================================================================

    /// <summary>
    /// TokenExpiredエラーを作成（文字列パラメータ版）
    /// AuthenticationConverterとの互換性確保
    /// </summary>
    /// <param name="message">期限切れメッセージ</param>
    /// <returns>トークン期限切れエラーDTO</returns>
    public static AuthenticationErrorDto TokenExpired(string message)
    {
        return new AuthenticationErrorDto
        {
            Type = "TokenExpired",
            Message = message
        };
    }


    /// <summary>
    /// ExternalAuthenticationFailedエラーを作成（文字列パラメータ版）
    /// AuthenticationConverterとの互換性確保
    /// </summary>
    /// <param name="message">外部認証失敗メッセージ</param>
    /// <returns>外部認証失敗エラーDTO</returns>
    public static AuthenticationErrorDto ExternalAuthenticationFailed(string message)
    {
        return new AuthenticationErrorDto
        {
            Type = "ExternalAuthenticationFailed",
            Message = message
        };
    }

    /// <summary>
    /// AuditLogErrorエラーを作成（文字列パラメータ版）
    /// AuthenticationConverterとの互換性確保
    /// </summary>
    /// <param name="message">監査ログエラーメッセージ</param>
    /// <returns>監査ログエラーDTO</returns>
    public static AuthenticationErrorDto AuditLogError(string message)
    {
        return new AuthenticationErrorDto
        {
            Type = "AuditLogError",
            Message = message
        };
    }

}