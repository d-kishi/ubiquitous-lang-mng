using System;
using System.Collections.Generic;
using Microsoft.FSharp.Core;
using UbiquitousLanguageManager.Contracts.DTOs;
using UbiquitousLanguageManager.Domain;

namespace UbiquitousLanguageManager.Contracts.Converters;

/// <summary>
/// F#ドメインエンティティとC# DTOの型変換クラス（簡易版）
/// 段階的に機能を追加していく予定
/// </summary>
public static class TypeConverters
{
    /// <summary>
    /// 一時的な簡易変換メソッド（ビルド成功のため）
    /// </summary>
    public static UserDto ToDto(User user)
    {
        return new UserDto
        {
            Id = user.Id.Item,
            Email = user.Email.Value,
            Name = user.Name.Value,
            Role = user.Role.ToString(),
            IsFirstLogin = user.IsFirstLogin,
            UpdatedAt = user.UpdatedAt,
            UpdatedBy = user.UpdatedBy.Item
        };
    }

    /// <summary>
    /// 一時的な簡易変換メソッド（ビルド成功のため）
    /// </summary>
    public static ProjectDto ToDto(Project project)
    {
        return new ProjectDto
        {
            Id = project.Id.Item,
            Name = project.Name.Value,
            Description = project.Description.Value,
            UpdatedAt = project.UpdatedAt,
            UpdatedBy = project.UpdatedBy.Item
        };
    }
}