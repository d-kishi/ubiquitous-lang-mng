namespace UbiquitousLanguageManager.Contracts.DTOs.Authentication;

/// <summary>
/// ログイン応答DTO
/// 
/// 【F#初学者向け解説】
/// F#のResult型やOption型の情報をC#のDTOに変換して、
/// Blazor Serverコンポーネントで扱いやすい形式にします。
/// </summary>
public class LoginResponseDto
{
    /// <summary>
    /// ログイン成功フラグ
    /// </summary>
    public bool IsSuccess { get; set; }

    /// <summary>
    /// エラーメッセージ（ログイン失敗時）
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// ユーザー情報（ログイン成功時）
    /// </summary>
    public AuthenticatedUserDto? User { get; set; }

    /// <summary>
    /// 初回ログインフラグ
    /// trueの場合、パスワード変更画面にリダイレクトする必要がある
    /// </summary>
    public bool IsFirstLogin { get; set; }

    /// <summary>
    /// リダイレクト先URL
    /// </summary>
    public string? RedirectUrl { get; set; }

    /// <summary>
    /// 成功結果の作成
    /// </summary>
    public static LoginResponseDto Success(AuthenticatedUserDto user, bool isFirstLogin = false, string? redirectUrl = null)
    {
        return new LoginResponseDto
        {
            IsSuccess = true,
            User = user,
            IsFirstLogin = isFirstLogin,
            RedirectUrl = redirectUrl
        };
    }

    /// <summary>
    /// エラー結果の作成
    /// </summary>
    public static LoginResponseDto Error(string errorMessage)
    {
        return new LoginResponseDto
        {
            IsSuccess = false,
            ErrorMessage = errorMessage
        };
    }
}