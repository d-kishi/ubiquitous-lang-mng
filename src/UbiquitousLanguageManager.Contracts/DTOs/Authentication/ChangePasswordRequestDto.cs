using System.ComponentModel.DataAnnotations;

namespace UbiquitousLanguageManager.Contracts.DTOs.Authentication;

/// <summary>
/// パスワード変更要求DTO
/// 
/// 【F#初学者向け解説】
/// F#のドメイン層のパスワード変更ロジックに渡すデータを構造化したDTOです。
/// セキュリティ要件として、現在のパスワードと新しいパスワードの確認が必要です。
/// </summary>
public class ChangePasswordRequestDto
{
    /// <summary>
    /// 現在のパスワード
    /// </summary>
    [Required(ErrorMessage = "現在のパスワードを入力してください")]
    [DataType(DataType.Password)]
    [Display(Name = "現在のパスワード")]
    public string CurrentPassword { get; set; } = string.Empty;

    /// <summary>
    /// 新しいパスワード
    /// </summary>
    [Required(ErrorMessage = "新しいパスワードを入力してください")]
    [StringLength(100, MinimumLength = 8, ErrorMessage = "パスワードは8文字以上100文字以下で入力してください")]
    [DataType(DataType.Password)]
    [Display(Name = "新しいパスワード")]
    [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).+$", 
        ErrorMessage = "パスワードは大文字、小文字、数字をそれぞれ1文字以上含む必要があります")]
    public string NewPassword { get; set; } = string.Empty;

    /// <summary>
    /// 新しいパスワード（確認用）
    /// </summary>
    [Required(ErrorMessage = "パスワード確認を入力してください")]
    [DataType(DataType.Password)]
    [Display(Name = "パスワード確認")]
    [Compare(nameof(NewPassword), ErrorMessage = "新しいパスワードとパスワード確認が一致しません")]
    public string ConfirmPassword { get; set; } = string.Empty;
}