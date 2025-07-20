namespace UbiquitousLanguageManager.Contracts.DTOs.Authentication;

/// <summary>
/// パスワード変更応答DTO
/// 
/// 【F#初学者向け解説】
/// F#のResult型の情報をC#のDTOに変換して、
/// Blazor Serverコンポーネントで扱いやすい形式にします。
/// </summary>
public class ChangePasswordResponseDto
{
    /// <summary>
    /// パスワード変更成功フラグ
    /// </summary>
    public bool IsSuccess { get; set; }

    /// <summary>
    /// メッセージ（成功・エラー両方）
    /// </summary>
    public string? Message { get; set; }

    /// <summary>
    /// エラーメッセージ（互換性のため）
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// 成功結果の作成
    /// </summary>
    public static ChangePasswordResponseDto Success(string message = "パスワードが正常に変更されました。")
    {
        return new ChangePasswordResponseDto
        {
            IsSuccess = true,
            Message = message
        };
    }

    /// <summary>
    /// エラー結果の作成
    /// </summary>
    public static ChangePasswordResponseDto Error(string errorMessage)
    {
        return new ChangePasswordResponseDto
        {
            IsSuccess = false,
            Message = errorMessage,
            ErrorMessage = errorMessage
        };
    }
}