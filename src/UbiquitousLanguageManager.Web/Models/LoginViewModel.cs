using System.ComponentModel.DataAnnotations;

namespace UbiquitousLanguageManager.Web.Models;

/// <summary>
/// ログインフォーム用のビューモデル
/// 
/// 【Blazor Server初学者向け解説】
/// MVCパターンでのフォーム処理では、ViewModelを使用してデータバインディングと
/// バリデーションを行います。DataAnnotationsを使用してサーバーサイドバリデーションを実装。
/// </summary>
public class LoginViewModel
{
    /// <summary>
    /// メールアドレス
    /// </summary>
    [Required(ErrorMessage = "メールアドレスは必須です")]
    [EmailAddress(ErrorMessage = "正しいメールアドレス形式で入力してください")]
    [Display(Name = "メールアドレス")]
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// パスワード
    /// </summary>
    [Required(ErrorMessage = "パスワードは必須です")]
    [DataType(DataType.Password)]
    [Display(Name = "パスワード")]
    public string Password { get; set; } = string.Empty;

    /// <summary>
    /// ログイン状態を記録するかどうか
    /// </summary>
    [Display(Name = "ログイン状態を記録する")]
    public bool RememberMe { get; set; }
}