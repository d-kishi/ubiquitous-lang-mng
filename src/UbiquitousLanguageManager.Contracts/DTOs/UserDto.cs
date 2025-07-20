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
    /// Phase A2: ユーザープロフィール情報
    /// </summary>
    public UserProfileDto? Profile { get; set; }

    /// <summary>
    /// Phase A2: プロジェクトスコープ権限
    /// </summary>
    public List<ProjectPermissionDto> ProjectPermissions { get; set; } = new();

    /// <summary>
    /// Phase A2: メールアドレス確認フラグ
    /// </summary>
    public bool EmailConfirmed { get; set; }

    /// <summary>
    /// Phase A2: 電話番号
    /// </summary>
    public string? PhoneNumber { get; set; }

    /// <summary>
    /// Phase A2: 電話番号確認フラグ
    /// </summary>
    public bool PhoneNumberConfirmed { get; set; }

    /// <summary>
    /// Phase A2: 二要素認証有効フラグ
    /// </summary>
    public bool TwoFactorEnabled { get; set; }

    /// <summary>
    /// Phase A2: ロックアウト機能有効フラグ
    /// </summary>
    public bool LockoutEnabled { get; set; }

    /// <summary>
    /// Phase A2: ロックアウト終了時刻
    /// </summary>
    public DateTime? LockoutEnd { get; set; }

    /// <summary>
    /// Phase A2: ログイン失敗回数
    /// </summary>
    public int AccessFailedCount { get; set; }

    /// <summary>
    /// 作成日時（UTC）
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// 作成者ID
    /// </summary>
    public long CreatedBy { get; set; }

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

/// <summary>
/// Phase A2: ユーザープロフィール情報DTO
/// ユーザーの詳細プロフィール情報の転送オブジェクト
/// </summary>
public class UserProfileDto
{
    /// <summary>
    /// 表示名（任意）
    /// </summary>
    [StringLength(100, ErrorMessage = "表示名は100文字以内で入力してください")]
    public string? DisplayName { get; set; }

    /// <summary>
    /// 所属部署（任意）
    /// </summary>
    [StringLength(100, ErrorMessage = "所属部署は100文字以内で入力してください")]
    public string? Department { get; set; }

    /// <summary>
    /// 電話番号（任意）
    /// </summary>
    [Phone(ErrorMessage = "有効な電話番号形式で入力してください")]
    public string? PhoneNumber { get; set; }

    /// <summary>
    /// 備考（任意）
    /// </summary>
    [StringLength(500, ErrorMessage = "備考は500文字以内で入力してください")]
    public string? Notes { get; set; }
}

/// <summary>
/// Phase A2: プロジェクトスコープ権限DTO
/// プロジェクト単位での細かな権限制御のための転送オブジェクト
/// </summary>
public class ProjectPermissionDto
{
    /// <summary>
    /// プロジェクトID
    /// </summary>
    public long ProjectId { get; set; }

    /// <summary>
    /// プロジェクト名（表示用）
    /// </summary>
    public string ProjectName { get; set; } = string.Empty;

    /// <summary>
    /// 権限一覧（文字列配列）
    /// ViewUsers, CreateUsers, EditUsers, etc.
    /// </summary>
    public List<string> Permissions { get; set; } = new();
}

/// <summary>
/// Phase A2: ユーザーロール変更用DTO
/// ユーザーロール変更時の入力データ構造
/// </summary>
public class ChangeUserRoleDto
{
    /// <summary>
    /// 対象ユーザーID
    /// </summary>
    public long UserId { get; set; }

    /// <summary>
    /// 新しいロール
    /// </summary>
    [Required(ErrorMessage = "新しいロールは必須です")]
    public string NewRole { get; set; } = string.Empty;

    /// <summary>
    /// 変更理由（任意）
    /// </summary>
    [StringLength(500, ErrorMessage = "変更理由は500文字以内で入力してください")]
    public string? Reason { get; set; }
}

/// <summary>
/// Phase A2: メールアドレス変更用DTO
/// メールアドレス変更時の入力データ構造
/// </summary>
public class ChangeEmailDto
{
    /// <summary>
    /// 対象ユーザーID
    /// </summary>
    public long UserId { get; set; }

    /// <summary>
    /// 新しいメールアドレス
    /// </summary>
    [Required(ErrorMessage = "新しいメールアドレスは必須です")]
    [EmailAddress(ErrorMessage = "有効なメールアドレス形式で入力してください")]
    public string NewEmail { get; set; } = string.Empty;

    /// <summary>
    /// 現在のパスワード（本人確認用）
    /// </summary>
    [Required(ErrorMessage = "現在のパスワードによる確認が必要です")]
    public string CurrentPassword { get; set; } = string.Empty;
}