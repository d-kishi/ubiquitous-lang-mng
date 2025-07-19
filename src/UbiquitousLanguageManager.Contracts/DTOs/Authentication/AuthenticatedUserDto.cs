using System;

namespace UbiquitousLanguageManager.Contracts.DTOs.Authentication;

/// <summary>
/// 認証済みユーザーDTO
/// 
/// 【Blazor Server初学者向け解説】
/// ログイン後のユーザー情報を表現するDTOです。
/// セキュリティ上、パスワードハッシュなどの機密情報は含まれません。
/// AuthenticationStateProviderで認証状態として管理されます。
/// </summary>
public class AuthenticatedUserDto
{
    /// <summary>
    /// ユーザーID
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// メールアドレス
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// ユーザー名（表示名）
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// ユーザーロール
    /// </summary>
    public string Role { get; set; } = string.Empty;

    /// <summary>
    /// アクティブ状態
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// 初回ログインフラグ
    /// </summary>
    public bool IsFirstLogin { get; set; }

    /// <summary>
    /// 最終更新日時
    /// </summary>
    public DateTime UpdatedAt { get; set; }
}