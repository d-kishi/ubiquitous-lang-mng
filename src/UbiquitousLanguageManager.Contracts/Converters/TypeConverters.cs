using System;
using System.Collections.Generic;
using Microsoft.FSharp.Core;
using UbiquitousLanguageManager.Contracts.DTOs;
using UbiquitousLanguageManager.Domain;
using DomainEntity = UbiquitousLanguageManager.Domain.Domain;

namespace UbiquitousLanguageManager.Contracts.Converters;

/// <summary>
/// F#ドメインエンティティとC# DTOの型変換クラス（完全版）
/// F#の強力な型システムの恩恵を活かしつつ、C#との間で安全かつ保守性の高いデータ変換を実現
/// Blazor Server初学者・F#初学者向けに詳細なコメントを記載
/// </summary>
public static class TypeConverters
{
    // =================================================================
    // 🔄 F# → C# 変換メソッド（ドメインエンティティ → DTO）
    // =================================================================

    /// <summary>
    /// F#のUserエンティティをC#のUserDTOに変換
    /// F#のRecord型とValue Objectを適切にC#のプロパティにマッピング
    /// </summary>
    /// <param name="user">F#で定義されたUserエンティティ</param>
    /// <returns>C#のUserDTO</returns>
    public static UserDto ToDto(User user)
    {
        return new UserDto
        {
            Id = user.Id.Item,                      // F#のUserId判別共用体から値を取得
            Email = user.Email.Value,                // F#のEmail値オブジェクトから値を取得
            Name = user.Name.Value,                  // F#のUserName値オブジェクトから値を取得
            Role = UserRoleToString(user.Role),      // F#の判別共用体をstring型に変換
            IsFirstLogin = user.IsFirstLogin,
            UpdatedAt = user.UpdatedAt,
            UpdatedBy = user.UpdatedBy.Item          // F#のUserId判別共用体から値を取得
        };
    }

    /// <summary>
    /// F#のProjectエンティティをC#のProjectDTOに変換
    /// </summary>
    /// <param name="project">F#で定義されたProjectエンティティ</param>
    /// <returns>C#のProjectDTO</returns>
    public static ProjectDto ToDto(Project project)
    {
        return new ProjectDto
        {
            Id = project.Id.Item,                   // F#のProjectId判別共用体から値を取得
            Name = project.Name.Value,               // F#のJapaneseName値オブジェクトから値を取得
            Description = project.Description.Value, // F#のDescription値オブジェクトから値を取得
            UpdatedAt = project.UpdatedAt,
            UpdatedBy = project.UpdatedBy.Item       // F#のUserId判別共用体から値を取得
        };
    }

    /// <summary>
    /// F#のDomainエンティティをC#のDomainDTOに変換
    /// </summary>
    /// <param name="domain">F#で定義されたDomainエンティティ</param>
    /// <returns>C#のDomainDTO</returns>
    public static DomainDto ToDto(DomainEntity domain)
    {
        return new DomainDto
        {
            Id = domain.Id.Item,                    // F#のDomainId判別共用体から値を取得
            ProjectId = domain.ProjectId.Item,       // F#のProjectId判別共用体から値を取得
            Name = domain.Name.Value,                // F#のJapaneseName値オブジェクトから値を取得
            Description = domain.Description.Value,  // F#のDescription値オブジェクトから値を取得
            IsActive = domain.IsActive,
            UpdatedAt = domain.UpdatedAt,
            UpdatedBy = domain.UpdatedBy.Item        // F#のUserId判別共用体から値を取得
        };
    }

    /// <summary>
    /// F#のDraftUbiquitousLanguageエンティティをC#のUbiquitousLanguageDTOに変換
    /// </summary>
    /// <param name="draft">F#で定義されたDraftUbiquitousLanguageエンティティ</param>
    /// <returns>C#のUbiquitousLanguageDTO</returns>
    public static UbiquitousLanguageDto ToDto(DraftUbiquitousLanguage draft)
    {
        return new UbiquitousLanguageDto
        {
            Id = draft.Id.Item,                         // F#のUbiquitousLanguageId判別共用体から値を取得
            DomainId = draft.DomainId.Item,              // F#のDomainId判別共用体から値を取得
            JapaneseName = draft.JapaneseName.Value,     // F#のJapaneseName値オブジェクトから値を取得
            EnglishName = draft.EnglishName.Value,       // F#のEnglishName値オブジェクトから値を取得
            Description = draft.Description.Value,       // F#のDescription値オブジェクトから値を取得
            Status = ApprovalStatusToString(draft.Status), // F#の判別共用体をstring型に変換
            UpdatedAt = draft.UpdatedAt,
            UpdatedBy = draft.UpdatedBy.Item,            // F#のUserId判別共用体から値を取得
            ApprovedAt = null,                           // 下書きなので承認日時は未設定
            ApprovedBy = null                            // 下書きなので承認者は未設定
        };
    }

    /// <summary>
    /// F#のFormalUbiquitousLanguageエンティティをC#のUbiquitousLanguageDTOに変換
    /// </summary>
    /// <param name="formal">F#で定義されたFormalUbiquitousLanguageエンティティ</param>
    /// <returns>C#のUbiquitousLanguageDTO</returns>
    public static UbiquitousLanguageDto ToDto(FormalUbiquitousLanguage formal)
    {
        return new UbiquitousLanguageDto
        {
            Id = formal.Id.Item,                        // F#のUbiquitousLanguageId判別共用体から値を取得
            DomainId = formal.DomainId.Item,             // F#のDomainId判別共用体から値を取得
            JapaneseName = formal.JapaneseName.Value,    // F#のJapaneseName値オブジェクトから値を取得
            EnglishName = formal.EnglishName.Value,      // F#のEnglishName値オブジェクトから値を取得
            Description = formal.Description.Value,      // F#のDescription値オブジェクトから値を取得
            Status = "Approved",                        // 正式版なので承認済み状態
            UpdatedAt = formal.UpdatedAt,
            UpdatedBy = formal.UpdatedBy.Item,           // F#のUserId判別共用体から値を取得
            ApprovedAt = formal.ApprovedAt,              // 承認日時を設定
            ApprovedBy = formal.ApprovedBy.Item          // F#のUserId判別共用体から承認者ID取得
        };
    }

    // =================================================================
    // 🔄 C# → F# 変換メソッド（DTO → ドメインエンティティ）
    // =================================================================

    /// <summary>
    /// C#のCreateUserDTOからF#のUserエンティティを作成
    /// バリデーションを実施し、Result型で成功/失敗を返す
    /// </summary>
    /// <param name="dto">C#のCreateUserDTO</param>
    /// <returns>F#のResult型（成功時はUser、失敗時はエラーメッセージ）</returns>
    public static FSharpResult<User, string> FromCreateDto(CreateUserDto dto)
    {
        // F#のValue Objectを使用してバリデーションを実施
        // 各Value Objectのcreateメソッドは検証を含む
        var emailResult = Email.create(dto.Email ?? "");
        var nameResult = UserName.create(dto.Name ?? "");
        var roleResult = StringToUserRole(dto.Role ?? "");
        var createdByResult = CreateUserId(dto.CreatedBy);

        // 複数のResult型をチェック（すべて成功した場合のみUserエンティティを作成）
        if (emailResult.IsOk && nameResult.IsOk && roleResult.IsOk && createdByResult.IsOk)
        {
            var user = User.create(emailResult.ResultValue, nameResult.ResultValue, roleResult.ResultValue, createdByResult.ResultValue);
            return FSharpResult<User, string>.NewOk(user);
        }
        else
        {
            // エラー収集：どのフィールドでエラーが発生したかを特定
            var errors = new List<string>();
            if (emailResult.IsError) errors.Add($"Email: {emailResult.ErrorValue}");
            if (nameResult.IsError) errors.Add($"Name: {nameResult.ErrorValue}");
            if (roleResult.IsError) errors.Add($"Role: {roleResult.ErrorValue}");
            if (createdByResult.IsError) errors.Add($"CreatedBy: {createdByResult.ErrorValue}");
            return FSharpResult<User, string>.NewError(string.Join(", ", errors));
        }
    }

    /// <summary>
    /// C#のCreateDomainDTOからF#のDomainエンティティを作成
    /// </summary>
    /// <param name="dto">C#のCreateDomainDTO</param>
    /// <returns>F#のResult型（成功時はDomain、失敗時はエラーメッセージ）</returns>
    public static FSharpResult<DomainEntity, string> FromCreateDto(CreateDomainDto dto)
    {
        var projectIdResult = CreateProjectId(dto.ProjectId);
        var nameResult = JapaneseName.create(dto.Name ?? "");
        var descriptionResult = Description.create(dto.Description ?? "");
        var createdByResult = CreateUserId(dto.CreatedBy);

        if (projectIdResult.IsOk && nameResult.IsOk && descriptionResult.IsOk && createdByResult.IsOk)
        {
            var domain = DomainEntity.create(projectIdResult.ResultValue, nameResult.ResultValue, descriptionResult.ResultValue, createdByResult.ResultValue);
            return FSharpResult<DomainEntity, string>.NewOk(domain);
        }
        else
        {
            var errors = new List<string>();
            if (projectIdResult.IsError) errors.Add($"ProjectId: {projectIdResult.ErrorValue}");
            if (nameResult.IsError) errors.Add($"Name: {nameResult.ErrorValue}");
            if (descriptionResult.IsError) errors.Add($"Description: {descriptionResult.ErrorValue}");
            if (createdByResult.IsError) errors.Add($"CreatedBy: {createdByResult.ErrorValue}");
            return FSharpResult<DomainEntity, string>.NewError(string.Join(", ", errors));
        }
    }

    /// <summary>
    /// C#のCreateUbiquitousLanguageDTOからF#のDraftUbiquitousLanguageエンティティを作成
    /// </summary>
    /// <param name="dto">C#のCreateUbiquitousLanguageDTO</param>
    /// <returns>F#のResult型（成功時はDraftUbiquitousLanguage、失敗時はエラーメッセージ）</returns>
    public static FSharpResult<DraftUbiquitousLanguage, string> FromCreateDto(CreateUbiquitousLanguageDto dto)
    {
        var domainIdResult = CreateDomainId(dto.DomainId);
        var japaneseNameResult = JapaneseName.create(dto.JapaneseName ?? "");
        var englishNameResult = EnglishName.create(dto.EnglishName ?? "");
        var descriptionResult = Description.create(dto.Description ?? "");
        var createdByResult = CreateUserId(dto.CreatedBy);

        if (domainIdResult.IsOk && japaneseNameResult.IsOk && englishNameResult.IsOk && descriptionResult.IsOk && createdByResult.IsOk)
        {
            var draft = DraftUbiquitousLanguage.create(domainIdResult.ResultValue, japaneseNameResult.ResultValue, englishNameResult.ResultValue, descriptionResult.ResultValue, createdByResult.ResultValue);
            return FSharpResult<DraftUbiquitousLanguage, string>.NewOk(draft);
        }
        else
        {
            var errors = new List<string>();
            if (domainIdResult.IsError) errors.Add($"DomainId: {domainIdResult.ErrorValue}");
            if (japaneseNameResult.IsError) errors.Add($"JapaneseName: {japaneseNameResult.ErrorValue}");
            if (englishNameResult.IsError) errors.Add($"EnglishName: {englishNameResult.ErrorValue}");
            if (descriptionResult.IsError) errors.Add($"Description: {descriptionResult.ErrorValue}");
            if (createdByResult.IsError) errors.Add($"CreatedBy: {createdByResult.ErrorValue}");
            return FSharpResult<DraftUbiquitousLanguage, string>.NewError(string.Join(", ", errors));
        }
    }

    // =================================================================
    // 🔄 判別共用体変換ヘルパーメソッド
    // =================================================================

    /// <summary>
    /// F#のUserRole判別共用体をC#のstring型に変換
    /// パターンマッチングを使用して安全に変換
    /// </summary>
    /// <param name="role">F#のUserRole判別共用体</param>
    /// <returns>文字列表現</returns>
    private static string UserRoleToString(UserRole role)
    {
        // F#の判別共用体は、C#からはプロパティとして各ケースにアクセス可能
        if (role.IsSuperUser) return "SuperUser";
        if (role.IsProjectManager) return "ProjectManager";
        if (role.IsDomainApprover) return "DomainApprover";
        if (role.IsGeneralUser) return "GeneralUser";
        return "Unknown"; // 予期しないケース（通常は発生しない）
    }

    /// <summary>
    /// C#のstring型をF#のUserRole判別共用体に変換
    /// 無効な値が渡された場合はResult型のErrorを返す
    /// </summary>
    /// <param name="roleString">ロールの文字列表現</param>
    /// <returns>F#のResult型（成功時はUserRole、失敗時はエラーメッセージ）</returns>
    private static FSharpResult<UserRole, string> StringToUserRole(string roleString)
    {
        // F#の判別共用体は、C#からはNewXxxという静的メソッドでケースを作成
        return roleString switch
        {
            "SuperUser" => FSharpResult<UserRole, string>.NewOk(UserRole.SuperUser),
            "ProjectManager" => FSharpResult<UserRole, string>.NewOk(UserRole.ProjectManager),
            "DomainApprover" => FSharpResult<UserRole, string>.NewOk(UserRole.DomainApprover),
            "GeneralUser" => FSharpResult<UserRole, string>.NewOk(UserRole.GeneralUser),
            _ => FSharpResult<UserRole, string>.NewError($"無効なユーザーロールです: {roleString}")
        };
    }

    /// <summary>
    /// F#のApprovalStatus判別共用体をC#のstring型に変換
    /// </summary>
    /// <param name="status">F#のApprovalStatus判別共用体</param>
    /// <returns>文字列表現</returns>
    private static string ApprovalStatusToString(ApprovalStatus status)
    {
        if (status.IsDraft) return "Draft";
        if (status.IsSubmitted) return "Submitted";
        if (status.IsApproved) return "Approved";
        if (status.IsRejected) return "Rejected";
        return "Unknown";
    }

    /// <summary>
    /// C#のstring型をF#のApprovalStatus判別共用体に変換
    /// </summary>
    /// <param name="statusString">ステータスの文字列表現</param>
    /// <returns>F#のResult型（成功時はApprovalStatus、失敗時はエラーメッセージ）</returns>
    private static FSharpResult<ApprovalStatus, string> StringToApprovalStatus(string statusString)
    {
        return statusString switch
        {
            "Draft" => FSharpResult<ApprovalStatus, string>.NewOk(ApprovalStatus.Draft),
            "Submitted" => FSharpResult<ApprovalStatus, string>.NewOk(ApprovalStatus.Submitted),
            "Approved" => FSharpResult<ApprovalStatus, string>.NewOk(ApprovalStatus.Approved),
            "Rejected" => FSharpResult<ApprovalStatus, string>.NewOk(ApprovalStatus.Rejected),
            _ => FSharpResult<ApprovalStatus, string>.NewError($"無効な承認ステータスです: {statusString}")
        };
    }

    // =================================================================
    // 🔄 IDヘルパーメソッド
    // =================================================================

    /// <summary>
    /// long型からF#のUserId判別共用体を作成
    /// </summary>
    /// <param name="id">ユーザーID</param>
    /// <returns>F#のResult型（成功時はUserId、失敗時はエラーメッセージ）</returns>
    private static FSharpResult<UserId, string> CreateUserId(long id)
    {
        if (id <= 0)
            return FSharpResult<UserId, string>.NewError("ユーザーIDは正の値である必要があります");
        return FSharpResult<UserId, string>.NewOk(UserId.NewUserId(id));
    }

    /// <summary>
    /// long型からF#のProjectId判別共用体を作成
    /// </summary>
    /// <param name="id">プロジェクトID</param>
    /// <returns>F#のResult型（成功時はProjectId、失敗時はエラーメッセージ）</returns>
    private static FSharpResult<ProjectId, string> CreateProjectId(long id)
    {
        if (id <= 0)
            return FSharpResult<ProjectId, string>.NewError("プロジェクトIDは正の値である必要があります");
        return FSharpResult<ProjectId, string>.NewOk(ProjectId.NewProjectId(id));
    }

    /// <summary>
    /// long型からF#のDomainId判別共用体を作成
    /// </summary>
    /// <param name="id">ドメインID</param>
    /// <returns>F#のResult型（成功時はDomainId、失敗時はエラーメッセージ）</returns>
    private static FSharpResult<DomainId, string> CreateDomainId(long id)
    {
        if (id <= 0)
            return FSharpResult<DomainId, string>.NewError("ドメインIDは正の値である必要があります");
        return FSharpResult<DomainId, string>.NewOk(DomainId.NewDomainId(id));
    }

    /// <summary>
    /// long型からF#のUbiquitousLanguageId判別共用体を作成
    /// </summary>
    /// <param name="id">ユビキタス言語ID</param>
    /// <returns>F#のResult型（成功時はUbiquitousLanguageId、失敗時はエラーメッセージ）</returns>
    private static FSharpResult<UbiquitousLanguageId, string> CreateUbiquitousLanguageId(long id)
    {
        if (id <= 0)
            return FSharpResult<UbiquitousLanguageId, string>.NewError("ユビキタス言語IDは正の値である必要があります");
        return FSharpResult<UbiquitousLanguageId, string>.NewOk(UbiquitousLanguageId.NewUbiquitousLanguageId(id));
    }
}