using System;

namespace UbiquitousLanguageManager.Contracts.DTOs;

/// <summary>
/// Phase A9: 認証結果DTO
/// F# Application層のResult&lt;User, AuthenticationError&gt;をC#境界で表現
/// Railway-oriented Programmingの結果を型安全に伝達
/// 【Blazor Server初学者向け解説】
/// このDTOは、F#の強力な型システムで表現された認証結果を、
/// C#のWeb層・Infrastructure層で安全に扱うためのデータ転送オブジェクトです。
/// </summary>
public class AuthenticationResultDto
{
    /// <summary>
    /// 認証成功フラグ
    /// F#のResult型のSuccess/Failureを表現
    /// </summary>
    public bool IsSuccess { get; set; }

    /// <summary>
    /// 認証成功時のユーザー情報
    /// IsSuccess=trueの場合のみ設定される
    /// </summary>
    public UserDto? User { get; set; }

    /// <summary>
    /// 認証失敗時のエラー情報
    /// IsSuccess=falseの場合のみ設定される
    /// </summary>
    public AuthenticationErrorDto? Error { get; set; }

    /// <summary>
    /// 認証トークン（ログイン成功時）
    /// セッション管理・JWT等のトークン情報
    /// </summary>
    public string? Token { get; set; }

    /// <summary>
    /// トークン有効期限（ログイン成功時）
    /// セキュリティ管理のための有効期限情報
    /// </summary>
    public DateTime? TokenExpires { get; set; }

    /// <summary>
    /// 二要素認証が必要かどうか
    /// TwoFactorRequiredエラーの場合に設定
    /// </summary>
    public bool RequiresTwoFactor { get; set; }

    /// <summary>
    /// パスワード変更が必要かどうか
    /// 初回ログイン・パスワード期限切れの場合に設定
    /// </summary>
    public bool RequiresPasswordChange { get; set; }

    /// <summary>
    /// 認証結果の詳細ステータス
    /// F#のAuthenticationResult拡張型に対応
    /// </summary>
    public string? DetailedStatus { get; set; }

    /// <summary>
    /// 成功結果を作成するファクトリーメソッド
    /// F# Success caseに対応
    /// </summary>
    /// <param name="user">認証されたユーザー情報</param>
    /// <param name="token">認証トークン（オプション）</param>
    /// <param name="tokenExpires">トークン有効期限（オプション）</param>
    /// <param name="detailedStatus">詳細ステータス（オプション）</param>
    /// <returns>成功を表すAuthenticationResultDto</returns>
    public static AuthenticationResultDto Success(UserDto user, string? token = null, DateTime? tokenExpires = null, string? detailedStatus = null)
    {
        return new AuthenticationResultDto
        {
            IsSuccess = true,
            User = user,
            Error = null,
            Token = token,
            TokenExpires = tokenExpires,
            RequiresTwoFactor = false,
            RequiresPasswordChange = user.IsFirstLogin,
            DetailedStatus = detailedStatus ?? "Success"
        };
    }

    // =================================================================
    // Phase A9: 拡張認証結果ファクトリーメソッド（将来のF#拡張対応）
    // =================================================================

    /// <summary>
    /// パスワード変更要求結果を作成するファクトリーメソッド
    /// F# PasswordChangeRequired caseに対応
    /// </summary>
    /// <param name="user">認証されたユーザー情報</param>
    /// <param name="reason">パスワード変更が必要な理由</param>
    /// <returns>パスワード変更要求を表すAuthenticationResultDto</returns>
    public static AuthenticationResultDto PasswordChangeRequired(UserDto user, string? reason = null)
    {
        return new AuthenticationResultDto
        {
            IsSuccess = true,
            User = user,
            Error = null,
            Token = null,
            TokenExpires = null,
            RequiresTwoFactor = false,
            RequiresPasswordChange = true,
            DetailedStatus = $"PasswordChangeRequired: {reason ?? "初回ログインまたはパスワード期限切れ"}"
        };
    }

    /// <summary>
    /// 二要素認証要求結果を作成するファクトリーメソッド
    /// F# TwoFactorRequired caseに対応
    /// </summary>
    /// <param name="user">認証されたユーザー情報（部分的）</param>
    /// <returns>二要素認証要求を表すAuthenticationResultDto</returns>
    public static AuthenticationResultDto TwoFactorRequired(UserDto user)
    {
        return new AuthenticationResultDto
        {
            IsSuccess = false,  // 二要素認証完了まで認証未完了扱い
            User = user,
            Error = AuthenticationErrorDto.TwoFactorRequired(user.Email),
            Token = null,
            TokenExpires = null,
            RequiresTwoFactor = true,
            RequiresPasswordChange = false,
            DetailedStatus = "TwoFactorRequired"
        };
    }

    /// <summary>
    /// セキュリティ警告付き成功結果を作成するファクトリーメソッド
    /// F# SecurityWarning caseに対応
    /// </summary>
    /// <param name="user">認証されたユーザー情報</param>
    /// <param name="warningMessage">警告メッセージ</param>
    /// <param name="token">認証トークン（オプション）</param>
    /// <param name="tokenExpires">トークン有効期限（オプション）</param>
    /// <returns>セキュリティ警告付き成功を表すAuthenticationResultDto</returns>
    public static AuthenticationResultDto SecurityWarning(UserDto user, string warningMessage, string? token = null, DateTime? tokenExpires = null)
    {
        return new AuthenticationResultDto
        {
            IsSuccess = true,
            User = user,
            Error = null,
            Token = token,
            TokenExpires = tokenExpires,
            RequiresTwoFactor = false,
            RequiresPasswordChange = false,
            DetailedStatus = $"SecurityWarning: {warningMessage}"
        };
    }

    /// <summary>
    /// 一時的ロックアウト結果を作成するファクトリーメソッド
    /// F# TemporaryLockout caseに対応
    /// </summary>
    /// <param name="email">ロックされたユーザーのメールアドレス</param>
    /// <param name="lockoutEnd">ロックアウト終了時刻</param>
    /// <param name="remainingAttempts">残り試行回数</param>
    /// <returns>一時的ロックアウトを表すAuthenticationResultDto</returns>
    public static AuthenticationResultDto TemporaryLockout(string email, DateTime lockoutEnd, int remainingAttempts = 0)
    {
        return new AuthenticationResultDto
        {
            IsSuccess = false,
            User = null,
            Error = AuthenticationErrorDto.AccountLocked(email, lockoutEnd),
            Token = null,
            TokenExpires = null,
            RequiresTwoFactor = false,
            RequiresPasswordChange = false,
            DetailedStatus = $"TemporaryLockout: 残り試行回数 {remainingAttempts}"
        };
    }

    /// <summary>
    /// 失敗結果を作成するファクトリーメソッド
    /// F# Error caseに対応
    /// </summary>
    /// <param name="error">エラー情報</param>
    /// <returns>失敗を表すAuthenticationResultDto</returns>
    public static AuthenticationResultDto Failure(AuthenticationErrorDto error)
    {
        return new AuthenticationResultDto
        {
            IsSuccess = false,
            User = null,
            Error = error,
            Token = null,
            TokenExpires = null,
            RequiresTwoFactor = error.Type == "TwoFactorRequired",
            RequiresPasswordChange = error.Type == "PasswordExpired"
        };
    }
}