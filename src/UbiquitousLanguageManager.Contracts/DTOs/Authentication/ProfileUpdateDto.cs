using System;
using System.ComponentModel.DataAnnotations;

namespace UbiquitousLanguageManager.Contracts.DTOs.Authentication;

/// <summary>
/// プロフィール更新リクエストDTO
/// UI設計書3.2節「プロフィール変更画面」準拠
/// </summary>
public class ProfileUpdateDto
{
    /// <summary>
    /// メールアドレス（表示のみ・変更不可）
    /// </summary>
    [Display(Name = "メールアドレス")]
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// ユーザー名（表示のみ・変更不可）
    /// </summary>
    [Display(Name = "ユーザー名")]
    public string UserName { get; set; } = string.Empty;

    /// <summary>
    /// 氏名（必須項目）
    /// UI設計書3.2節準拠：氏名は1フィールドで管理
    /// ApplicationUser.Nameフィールドと対応
    /// </summary>
    [Required(ErrorMessage = "氏名を入力してください")]
    [StringLength(100, ErrorMessage = "氏名は100文字以内で入力してください")]
    [Display(Name = "氏名")]
    public string Name { get; set; } = string.Empty;
}