using System.ComponentModel.DataAnnotations;

namespace UbiquitousLanguageManager.Contracts.DTOs.Authentication;

/// <summary>
/// ログイン要求DTO
/// 
/// 【Blazor Server初学者向け解説】
/// このDTOは、ログインフォームからの入力データを受け取るために使用されます。
/// DataAnnotationsによって、クライアントサイドとサーバーサイドの両方で検証が行われます。
/// </summary>
public class LoginRequestDto
{
    /// <summary>
    /// メールアドレス（ログインID）
    /// </summary>
    [Required(ErrorMessage = "メールアドレスを入力してください")]
    [EmailAddress(ErrorMessage = "有効なメールアドレスを入力してください")]
    [Display(Name = "メールアドレス")]
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// パスワード
    /// </summary>
    [Required(ErrorMessage = "パスワードを入力してください")]
    [DataType(DataType.Password)]
    [Display(Name = "パスワード")]
    public string Password { get; set; } = string.Empty;

    /// <summary>
    /// ログイン状態を保持するかどうか
    /// </summary>
    [Display(Name = "ログイン状態を保持する")]
    public bool RememberMe { get; set; } = false;
}