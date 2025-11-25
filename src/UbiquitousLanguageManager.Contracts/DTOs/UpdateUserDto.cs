using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace UbiquitousLanguageManager.Contracts.DTOs;

/// <summary>
/// ユーザー更新用DTO
/// Phase B-F3: ユーザー管理機能拡張
/// 既存ユーザー情報の更新時の入力データ構造
/// </summary>
public class UpdateUserDto
{
    /// <summary>
    /// 対象ユーザーID
    /// </summary>
    [Required(ErrorMessage = "ユーザーIDは必須です")]
    public string UserId { get; set; } = string.Empty;

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
    /// 割り当てプロジェクトID一覧
    /// ユーザーがアクセス可能なプロジェクトのIDリスト
    /// </summary>
    public List<long> AssignedProjectIds { get; set; } = new();

    /// <summary>
    /// アクティブ状態フラグ
    /// true: 有効 / false: 無効化
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// 更新者ID
    /// 変更を実行したユーザーのID（監査ログ用）
    /// </summary>
    public long UpdatedBy { get; set; }
}
