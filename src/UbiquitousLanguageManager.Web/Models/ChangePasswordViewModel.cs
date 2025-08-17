using System.ComponentModel.DataAnnotations;

namespace UbiquitousLanguageManager.Web.Models;

/// <summary>
/// パスワード変更用ビューモデル（TECH-004: 初回ログイン時パスワード変更）
/// 
/// 【Blazor Server初学者向け解説】
/// このクラスは、MVCパターンでのパスワード変更フォームのデータバインディング用です。
/// ASP.NET Core MVCのModel Bindingとデータアノテーション（DataAnnotations）による
/// サーバーサイドバリデーションを提供します。
/// </summary>
public class ChangePasswordViewModel
{
    /// <summary>
    /// 現在のパスワード
    /// 初回ログイン時は管理者が設定した初期パスワードを入力
    /// </summary>
    [Required(ErrorMessage = "現在のパスワードは必須です。")]
    [Display(Name = "現在のパスワード")]
    [DataType(DataType.Password)]
    public string CurrentPassword { get; set; } = string.Empty;

    /// <summary>
    /// 新しいパスワード
    /// 仕様書のパスワード要件に準拠する必要があります
    /// </summary>
    [Required(ErrorMessage = "新しいパスワードは必須です。")]
    [Display(Name = "新しいパスワード")]
    [DataType(DataType.Password)]
    [StringLength(100, MinimumLength = 8, ErrorMessage = "パスワードは8文字以上100文字以下で入力してください。")]
    [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&].*$", 
        ErrorMessage = "パスワードは大文字・小文字・数字・特殊文字をそれぞれ1つ以上含む必要があります。")]
    public string NewPassword { get; set; } = string.Empty;

    /// <summary>
    /// 新しいパスワード確認
    /// NewPasswordと同じ値である必要があります
    /// </summary>
    [Required(ErrorMessage = "パスワードの確認入力は必須です。")]
    [Display(Name = "新しいパスワード（確認）")]
    [DataType(DataType.Password)]
    [Compare("NewPassword", ErrorMessage = "パスワードとパスワード確認が一致しません。")]
    public string ConfirmPassword { get; set; } = string.Empty;
}