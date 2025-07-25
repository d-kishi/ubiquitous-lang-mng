using Microsoft.FSharp.Core;
using UbiquitousLanguageManager.Contracts.DTOs.Authentication;
using UbiquitousLanguageManager.Contracts.DTOs.Common;
using UbiquitousLanguageManager.Domain;

namespace UbiquitousLanguageManager.Contracts.Converters;

/// <summary>
/// 認証関連のF#↔C#マッピング
/// 
/// 【アーキテクチャ説明】
/// Clean Architectureの境界において、F#のドメインモデルとC#のDTOを相互変換します。
/// 手動マッピングを採用することで、型安全性と制御性を確保しています。
/// </summary>
public static class AuthenticationMapper
{
    /// <summary>
    /// F#のUserをAuthenticatedUserDtoに変換
    /// </summary>
    public static AuthenticatedUserDto ToAuthenticatedUserDto(User user)
    {
        // UserId、UserRoleの値を取得
        var userIdValue = user.Id.Value;
        var roleString = RoleToString(user.Role);

        return new AuthenticatedUserDto
        {
            Id = userIdValue,
            Email = user.Email.Value,
            Name = user.Name.Value,
            Role = roleString,
            IsActive = user.IsActive,
            IsFirstLogin = user.IsFirstLogin,
            UpdatedAt = user.UpdatedAt
        };
    }

    /// <summary>
    /// F#のUseCaseResultをLoginResponseDtoに変換
    /// </summary>
    public static LoginResponseDto ToLoginResponseDto(UbiquitousLanguageManager.Application.UseCaseResult<User> result)
    {
        if (result.IsSuccess && FSharpOption<User>.get_IsSome(result.Data))
        {
            var user = ToAuthenticatedUserDto(result.Data.Value);
            return LoginResponseDto.Success(user, user.IsFirstLogin);
        }
        else
        {
            var errorMessage = FSharpOption<string>.get_IsSome(result.ErrorMessage) ? result.ErrorMessage.Value : "ログインに失敗しました";
            return LoginResponseDto.Error(errorMessage);
        }
    }

    /// <summary>
    /// F#のUseCaseResult&lt;unit&gt;をResultDtoに変換
    /// </summary>
    public static ResultDto ToResultDto(UbiquitousLanguageManager.Application.UseCaseResult<Unit> result)
    {
        if (result.IsSuccess)
        {
            return ResultDto.Success();
        }
        else
        {
            var errorMessage = FSharpOption<string>.get_IsSome(result.ErrorMessage) ? result.ErrorMessage.Value : "処理に失敗しました";
            return ResultDto.Failure(errorMessage);
        }
    }

    /// <summary>
    /// LoginRequestDtoをF#のLoginCommandに変換
    /// </summary>
    public static UbiquitousLanguageManager.Application.LoginCommand ToLoginCommand(LoginRequestDto dto)
    {
        return new UbiquitousLanguageManager.Application.LoginCommand(
            dto.Email,
            dto.Password
        );
    }

    /// <summary>
    /// ChangePasswordRequestDtoをF#のChangePasswordCommandに変換
    /// </summary>
    public static UbiquitousLanguageManager.Application.ChangePasswordCommand ToChangePasswordCommand(
        ChangePasswordRequestDto dto, long userId)
    {
        return new UbiquitousLanguageManager.Application.ChangePasswordCommand(
            userId,
            dto.CurrentPassword,
            dto.NewPassword,
            dto.ConfirmPassword
        );
    }

    /// <summary>
    /// F#のRoleを文字列に変換（Phase A2: 新しい権限システム対応）
    /// </summary>
    private static string RoleToString(Role role)
    {
        if (role.IsSuperUser) return "SuperUser";
        if (role.IsProjectManager) return "ProjectManager";
        if (role.IsDomainApprover) return "DomainApprover";
        if (role.IsGeneralUser) return "GeneralUser";
        return "GeneralUser";
    }

    /// <summary>
    /// 文字列をF#のRoleに変換（Phase A2: 新しい権限システム対応）
    /// </summary>
    public static FSharpResult<Role, string> StringToRole(string roleString)
    {
        var normalizedRole = roleString?.ToLower()?.Trim();
        
        return normalizedRole switch
        {
            "superuser" => FSharpResult<Role, string>.NewOk(Role.SuperUser),
            "projectmanager" => FSharpResult<Role, string>.NewOk(Role.ProjectManager),
            "domainapprover" => FSharpResult<Role, string>.NewOk(Role.DomainApprover),
            "generaluser" => FSharpResult<Role, string>.NewOk(Role.GeneralUser),
            _ => FSharpResult<Role, string>.NewError($"無効なユーザーロール: {roleString}")
        };
    }
}