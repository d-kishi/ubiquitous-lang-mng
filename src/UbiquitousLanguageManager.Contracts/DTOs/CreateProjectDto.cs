using System.ComponentModel.DataAnnotations;

namespace UbiquitousLanguageManager.Contracts.DTOs;

/// <summary>
/// プロジェクト作成要求DTO（Phase B1）
/// UI層からApplication層へのプロジェクト作成要求を表現
/// </summary>
public record CreateProjectDto
{
    /// <summary>
    /// プロジェクト名（必須、3-100文字）
    /// </summary>
    [Required(ErrorMessage = "プロジェクト名は必須です")]
    [StringLength(100, MinimumLength = 3, ErrorMessage = "プロジェクト名は3文字以上100文字以内で入力してください")]
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// プロジェクト説明（任意、最大1000文字）
    /// </summary>
    [StringLength(1000, ErrorMessage = "プロジェクト説明は1000文字以内で入力してください")]
    public string? Description { get; init; }

    /// <summary>
    /// プロジェクト所有者ID（必須）
    /// 認証済みユーザーIDがセットされる
    /// </summary>
    [Required(ErrorMessage = "プロジェクト所有者IDは必須です")]
    public long OwnerId { get; init; }
}