using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace UbiquitousLanguageManager.Contracts.DTOs;

/// <summary>
/// ユーザー情報のデータ転送オブジェクト
/// F#ドメインエンティティとC#プレゼンテーション層の境界で使用
/// </summary>
public class UserDto
{
    /// <summary>
    /// ユーザーID（主キー）
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// メールアドレス（ログインID）
    /// </summary>
    [Required(ErrorMessage = "メールアドレスは必須です")]
    [EmailAddress(ErrorMessage = "有効なメールアドレス形式で入力してください")]
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// ユーザー名（表示名）
    /// </summary>
    [Required(ErrorMessage = "ユーザー名は必須です")]
    [StringLength(50, ErrorMessage = "ユーザー名は50文字以内で入力してください")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// ユーザーロール
    /// SuperUser, ProjectManager, DomainApprover, GeneralUser のいずれか
    /// </summary>
    [Required(ErrorMessage = "ユーザーロールは必須です")]
    public string Role { get; set; } = string.Empty;

    /// <summary>
    /// アクティブ状態フラグ
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// 初回ログインフラグ（パスワード変更が必要な状態）
    /// </summary>
    public bool IsFirstLogin { get; set; } = true;

    /// <summary>
    /// 最終更新日時（UTC）
    /// </summary>
    public DateTime UpdatedAt { get; set; }

    /// <summary>
    /// 最終更新者ID
    /// </summary>
    public long UpdatedBy { get; set; }
}

/// <summary>
/// ユーザー登録用DTO
/// 新規ユーザー作成時の入力データ構造
/// </summary>
public class CreateUserDto
{
    /// <summary>
    /// メールアドレス（ログインID）
    /// </summary>
    [Required(ErrorMessage = "メールアドレスは必須です")]
    [EmailAddress(ErrorMessage = "有効なメールアドレス形式で入力してください")]
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// ユーザー名（表示名）
    /// </summary>
    [Required(ErrorMessage = "ユーザー名は必須です")]
    [StringLength(50, ErrorMessage = "ユーザー名は50文字以内で入力してください")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// ユーザーロール
    /// </summary>
    [Required(ErrorMessage = "ユーザーロールは必須です")]
    public string Role { get; set; } = string.Empty;

    /// <summary>
    /// 作成者ID
    /// </summary>
    public long CreatedBy { get; set; }
}

/// <summary>
/// ログイン用DTO
/// 認証情報の入力データ構造
/// </summary>
public class LoginDto
{
    /// <summary>
    /// メールアドレス（ログインID）
    /// </summary>
    [Required(ErrorMessage = "メールアドレスは必須です")]
    [EmailAddress(ErrorMessage = "有効なメールアドレス形式で入力してください")]
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// パスワード
    /// </summary>
    [Required(ErrorMessage = "パスワードは必須です")]
    public string Password { get; set; } = string.Empty;
}

/// <summary>
/// パスワード変更用DTO
/// セキュアなパスワード更新のための入力データ構造
/// </summary>
public class ChangePasswordDto
{
    /// <summary>
    /// ユーザーID
    /// </summary>
    public long UserId { get; set; }

    /// <summary>
    /// 現在のパスワード
    /// </summary>
    [Required(ErrorMessage = "現在のパスワードは必須です")]
    public string OldPassword { get; set; } = string.Empty;

    /// <summary>
    /// 新しいパスワード
    /// </summary>
    [Required(ErrorMessage = "新しいパスワードは必須です")]
    [StringLength(100, MinimumLength = 8, ErrorMessage = "パスワードは8文字以上100文字以内で入力してください")]
    public string NewPassword { get; set; } = string.Empty;

    /// <summary>
    /// 新しいパスワード（確認用）
    /// </summary>
    [Required(ErrorMessage = "パスワード確認は必須です")]
    [Compare(nameof(NewPassword), ErrorMessage = "新しいパスワードと確認パスワードが一致しません")]
    public string ConfirmPassword { get; set; } = string.Empty;
}