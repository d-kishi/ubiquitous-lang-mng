using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.FSharp.Core;
using Microsoft.FSharp.Collections;
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
    /// Phase A2: F#のUserエンティティをC#のUserDTOに変換（拡張版）
    /// F#のRecord型とValue Objectを適切にC#のプロパティにマッピング
    /// 新しい権限システム・プロフィール・プロジェクト権限に対応
    /// </summary>
    /// <param name="user">F#で定義されたUserエンティティ</param>
    /// <returns>C#のUserDTO</returns>
    /// <exception cref="ArgumentNullException">userがnullの場合</exception>
    public static UserDto ToDto(User user)
    {
        if (user == null)
            throw new ArgumentNullException(nameof(user), "Userエンティティがnullです");
            
        return new UserDto
        {
            Id = user.Id.Value,                                 // F#のUserId判別共用体から値を取得
            Email = user.Email.Value,                            // F#のEmail値オブジェクトから値を取得
            Name = user.Name.Value,                              // F#のUserName値オブジェクトから値を取得
            Role = RoleToString(user.Role),                      // F#のRole判別共用体をstring型に変換
            IsActive = user.IsActive,                            // アクティブ状態
            IsFirstLogin = user.IsFirstLogin,                    // 初回ログインフラグ
            // Phase A2: 新機能対応
            Profile = ToDto(user.Profile),                       // プロフィール情報変換
            ProjectPermissions = user.ProjectPermissions.Select(ToDto).ToList(), // プロジェクト権限変換（F#ListはIEnumerable扱い）
            EmailConfirmed = user.EmailConfirmed,                // メールアドレス確認フラグ
            PhoneNumber = user.PhoneNumber != null && FSharpOption<string>.get_IsSome(user.PhoneNumber) ? user.PhoneNumber.Value : null, // F#option型からC#null可能型へ変換
            PhoneNumberConfirmed = user.PhoneNumberConfirmed,    // 電話番号確認フラグ
            TwoFactorEnabled = user.TwoFactorEnabled,            // 二要素認証有効フラグ
            LockoutEnabled = user.LockoutEnabled,                // ロックアウト機能有効フラグ
            LockoutEnd = user.LockoutEnd != null && FSharpOption<DateTime>.get_IsSome(user.LockoutEnd) ? user.LockoutEnd.Value : (DateTime?)null, // F#option型からC#nullable型へ変換
            AccessFailedCount = user.AccessFailedCount,          // ログイン失敗回数
            // 監査情報
            CreatedAt = user.CreatedAt,                          // 作成日時
            CreatedBy = user.CreatedBy.Value,                    // 作成者ID
            UpdatedAt = user.UpdatedAt,                          // 更新日時
            UpdatedBy = user.UpdatedBy.Value                     // 更新者ID
        };
    }

    /// <summary>
    /// Phase A2: F#のUserProfileをC#のUserProfileDTOに変換
    /// F#のオプション型を適切にC#のnullable型にマッピング
    /// </summary>
    /// <param name="profile">F#で定義されたUserProfile</param>
    /// <returns>C#のUserProfileDTO</returns>
    /// <exception cref="ArgumentNullException">profileがnullの場合</exception>
    public static UserProfileDto ToDto(UserProfile profile)
    {
        if (profile == null)
            throw new ArgumentNullException(nameof(profile), "UserProfileがnullです");
            
        return new UserProfileDto
        {
            DisplayName = profile.DisplayName != null && FSharpOption<string>.get_IsSome(profile.DisplayName) ? profile.DisplayName.Value : null,    // F#のoption型からnullable stringに変換
            Department = profile.Department != null && FSharpOption<string>.get_IsSome(profile.Department) ? profile.Department.Value : null,      // F#のoption型からnullable stringに変換
            PhoneNumber = profile.PhoneNumber != null && FSharpOption<string>.get_IsSome(profile.PhoneNumber) ? profile.PhoneNumber.Value : null,    // F#のoption型からnullable stringに変換
            Notes = profile.Notes != null && FSharpOption<string>.get_IsSome(profile.Notes) ? profile.Notes.Value : null                 // F#のoption型からnullable stringに変換
        };
    }

    /// <summary>
    /// Phase A2: F#のProjectPermissionをC#のProjectPermissionDTOに変換
    /// F#のSet型をC#のList型に変換
    /// </summary>
    /// <param name="projectPermission">F#で定義されたProjectPermission</param>
    /// <returns>C#のProjectPermissionDTO</returns>
    public static ProjectPermissionDto ToDto(ProjectPermission projectPermission)
    {
        return new ProjectPermissionDto
        {
            ProjectId = projectPermission.ProjectId.Value,                   // F#のProjectId判別共用体から値を取得
            ProjectName = "プロジェクト名取得予定",                              // 実装時に適切なプロジェクト名取得ロジックを追加
            Permissions = SetModule.ToArray(projectPermission.Permissions)   // F#のSet<Permission>を配列に変換してからLINQ使用
                .Select(PermissionToString)
                .ToList()
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
            Id = project.Id.Value,                  // F#のProjectId判別共用体から値を取得
            Name = project.Name.Value,               // F#のJapaneseName値オブジェクトから値を取得
            Description = project.Description.Value, // F#のDescription値オブジェクトから値を取得
            UpdatedAt = project.UpdatedAt,
            UpdatedBy = project.UpdatedBy.Value      // F#のUserId判別共用体から値を取得
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
            Id = domain.Id.Value,                   // F#のDomainId判別共用体から値を取得
            ProjectId = domain.ProjectId.Value,      // F#のProjectId判別共用体から値を取得
            Name = domain.Name.Value,                // F#のJapaneseName値オブジェクトから値を取得
            Description = domain.Description.Value,  // F#のDescription値オブジェクトから値を取得
            IsActive = domain.IsActive,
            UpdatedAt = domain.UpdatedAt,
            UpdatedBy = domain.UpdatedBy.Value       // F#のUserId判別共用体から値を取得
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
            Id = draft.Id.Value,                        // F#のUbiquitousLanguageId判別共用体から値を取得
            DomainId = draft.DomainId.Value,             // F#のDomainId判別共用体から値を取得
            JapaneseName = draft.JapaneseName.Value,     // F#のJapaneseName値オブジェクトから値を取得
            EnglishName = draft.EnglishName.Value,       // F#のEnglishName値オブジェクトから値を取得
            Description = draft.Description.Value,       // F#のDescription値オブジェクトから値を取得
            Status = ApprovalStatusToString(draft.Status), // F#の判別共用体をstring型に変換
            UpdatedAt = draft.UpdatedAt,
            UpdatedBy = draft.UpdatedBy.Value,           // F#のUserId判別共用体から値を取得
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
            Id = formal.Id.Value,                       // F#のUbiquitousLanguageId判別共用体から値を取得
            DomainId = formal.DomainId.Value,            // F#のDomainId判別共用体から値を取得
            JapaneseName = formal.JapaneseName.Value,    // F#のJapaneseName値オブジェクトから値を取得
            EnglishName = formal.EnglishName.Value,      // F#のEnglishName値オブジェクトから値を取得
            Description = formal.Description.Value,      // F#のDescription値オブジェクトから値を取得
            Status = "Approved",                        // 正式版なので承認済み状態
            UpdatedAt = formal.UpdatedAt,
            UpdatedBy = formal.UpdatedBy.Value,          // F#のUserId判別共用体から値を取得
            ApprovedAt = formal.ApprovedAt,              // 承認日時を設定
            ApprovedBy = formal.ApprovedBy.Value         // F#のUserId判別共用体から承認者ID取得
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
        var roleResult = StringToRole(dto.Role ?? "");
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
    /// Phase A2: F#のRole判別共用体をC#のstring型に変換
    /// パターンマッチングを使用して安全に変換
    /// </summary>
    /// <param name="role">F#のRole判別共用体</param>
    /// <returns>文字列表現</returns>
    private static string RoleToString(Role role)
    {
        // F#の判別共用体は、C#からはプロパティとして各ケースにアクセス可能
        if (role.IsSuperUser) return "SuperUser";
        if (role.IsProjectManager) return "ProjectManager";
        if (role.IsDomainApprover) return "DomainApprover";
        if (role.IsGeneralUser) return "GeneralUser";
        return "Unknown"; // 予期しないケース（通常は発生しない）
    }

    /// <summary>
    /// Phase A2: C#のstring型をF#のRole判別共用体に変換
    /// 無効な値が渡された場合はResult型のErrorを返す
    /// </summary>
    /// <param name="roleString">ロールの文字列表現</param>
    /// <returns>F#のResult型（成功時はRole、失敗時はエラーメッセージ）</returns>
    private static FSharpResult<Role, string> StringToRole(string roleString)
    {
        // F#の判別共用体は、C#からはNewXxxという静的メソッドでケースを作成
        return roleString switch
        {
            "SuperUser" => FSharpResult<Role, string>.NewOk(Role.SuperUser),
            "ProjectManager" => FSharpResult<Role, string>.NewOk(Role.ProjectManager),
            "DomainApprover" => FSharpResult<Role, string>.NewOk(Role.DomainApprover),
            "GeneralUser" => FSharpResult<Role, string>.NewOk(Role.GeneralUser),
            _ => FSharpResult<Role, string>.NewError($"無効なユーザーロールです: {roleString}")
        };
    }

    /// <summary>
    /// Phase A2: F#のPermission判別共用体をC#のstring型に変換
    /// 新しい権限システムの権限種別を文字列に変換
    /// </summary>
    /// <param name="permission">F#のPermission判別共用体</param>
    /// <returns>文字列表現</returns>
    private static string PermissionToString(Permission permission)
    {
        // F#の判別共用体の各ケースをチェックして文字列に変換
        if (permission.IsViewUsers) return "ViewUsers";
        if (permission.IsCreateUsers) return "CreateUsers";
        if (permission.IsEditUsers) return "EditUsers";
        if (permission.IsDeleteUsers) return "DeleteUsers";
        if (permission.IsManageUserRoles) return "ManageUserRoles";
        if (permission.IsViewProjects) return "ViewProjects";
        if (permission.IsCreateProjects) return "CreateProjects";
        if (permission.IsManageProjects) return "ManageProjects";
        if (permission.IsDeleteProjects) return "DeleteProjects";
        if (permission.IsViewDomains) return "ViewDomains";
        if (permission.IsManageDomains) return "ManageDomains";
        if (permission.IsApproveDomains) return "ApproveDomains";
        if (permission.IsViewUbiquitousLanguages) return "ViewUbiquitousLanguages";
        if (permission.IsCreateUbiquitousLanguages) return "CreateUbiquitousLanguages";
        if (permission.IsEditUbiquitousLanguages) return "EditUbiquitousLanguages";
        if (permission.IsApproveUbiquitousLanguages) return "ApproveUbiquitousLanguages";
        if (permission.IsManageSystemSettings) return "ManageSystemSettings";
        return "Unknown"; // 予期しないケース（通常は発生しない）
    }

    /// <summary>
    /// Phase A2: C#のstring型をF#のPermission判別共用体に変換
    /// 無効な値が渡された場合はResult型のErrorを返す
    /// </summary>
    /// <param name="permissionString">権限の文字列表現</param>
    /// <returns>F#のResult型（成功時はPermission、失敗時はエラーメッセージ）</returns>
    private static FSharpResult<Permission, string> StringToPermission(string permissionString)
    {
        return permissionString switch
        {
            "ViewUsers" => FSharpResult<Permission, string>.NewOk(Permission.ViewUsers),
            "CreateUsers" => FSharpResult<Permission, string>.NewOk(Permission.CreateUsers),
            "EditUsers" => FSharpResult<Permission, string>.NewOk(Permission.EditUsers),
            "DeleteUsers" => FSharpResult<Permission, string>.NewOk(Permission.DeleteUsers),
            "ManageUserRoles" => FSharpResult<Permission, string>.NewOk(Permission.ManageUserRoles),
            "ViewProjects" => FSharpResult<Permission, string>.NewOk(Permission.ViewProjects),
            "CreateProjects" => FSharpResult<Permission, string>.NewOk(Permission.CreateProjects),
            "ManageProjects" => FSharpResult<Permission, string>.NewOk(Permission.ManageProjects),
            "DeleteProjects" => FSharpResult<Permission, string>.NewOk(Permission.DeleteProjects),
            "ViewDomains" => FSharpResult<Permission, string>.NewOk(Permission.ViewDomains),
            "ManageDomains" => FSharpResult<Permission, string>.NewOk(Permission.ManageDomains),
            "ApproveDomains" => FSharpResult<Permission, string>.NewOk(Permission.ApproveDomains),
            "ViewUbiquitousLanguages" => FSharpResult<Permission, string>.NewOk(Permission.ViewUbiquitousLanguages),
            "CreateUbiquitousLanguages" => FSharpResult<Permission, string>.NewOk(Permission.CreateUbiquitousLanguages),
            "EditUbiquitousLanguages" => FSharpResult<Permission, string>.NewOk(Permission.EditUbiquitousLanguages),
            "ApproveUbiquitousLanguages" => FSharpResult<Permission, string>.NewOk(Permission.ApproveUbiquitousLanguages),
            "ManageSystemSettings" => FSharpResult<Permission, string>.NewOk(Permission.ManageSystemSettings),
            _ => FSharpResult<Permission, string>.NewError($"無効な権限です: {permissionString}")
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

    // =================================================================
    // 🔄 Phase A2: 新規DTO変換メソッド（C# → F#）
    // =================================================================

    /// <summary>
    /// Phase A2: C#のUserProfileDTOからF#のUserProfileを作成
    /// F#のoption型による適切なnull処理を実装
    /// </summary>
    /// <param name="dto">C#のUserProfileDTO</param>
    /// <returns>F#のResult型（成功時はUserProfile、失敗時はエラーメッセージ）</returns>
    public static FSharpResult<UserProfile, string> FromDto(UserProfileDto dto)
    {
        // F#初学者向け解説: UserProfileはstring optionを使用しているため、
        // 値オブジェクトではなく、直接文字列を渡す
        var profile = UserProfile.create(dto.DisplayName, dto.Department, dto.PhoneNumber, dto.Notes);
        return FSharpResult<UserProfile, string>.NewOk(profile);
    }

    /// <summary>
    /// Phase A2: C#のProjectPermissionDTOからF#のProjectPermissionを作成
    /// 権限文字列リストをF#のSet&lt;Permission&gt;に変換
    /// </summary>
    /// <param name="dto">C#のProjectPermissionDTO</param>
    /// <returns>F#のResult型（成功時はProjectPermission、失敗時はエラーメッセージ）</returns>
    public static FSharpResult<ProjectPermission, string> FromDto(ProjectPermissionDto dto)
    {
        // ProjectId作成
        var projectIdResult = CreateProjectId(dto.ProjectId);
        if (projectIdResult.IsError)
            return FSharpResult<ProjectPermission, string>.NewError(projectIdResult.ErrorValue);

        // 権限文字列リストをF#のPermission Set型に変換
        var permissions = new List<Permission>();
        var errors = new List<string>();

        foreach (var permissionString in dto.Permissions)
        {
            var permissionResult = StringToPermission(permissionString);
            if (permissionResult.IsOk)
            {
                permissions.Add(permissionResult.ResultValue);
            }
            else
            {
                errors.Add(permissionResult.ErrorValue);
            }
        }

        if (errors.Count > 0) // List<T>.Any()の代わりにCountを使用
        {
            return FSharpResult<ProjectPermission, string>.NewError($"無効な権限が含まれています: {string.Join(", ", errors)}");
        }

        // F#のList型に変換（ProjectPermission.createはListを期待）
        var permissionList = ListModule.OfSeq(permissions);
        
        // ProjectPermission作成
        var projectPermission = ProjectPermission.create(projectIdResult.ResultValue, permissionList);
        return FSharpResult<ProjectPermission, string>.NewOk(projectPermission);
    }

    /// <summary>
    /// Phase A2: C#のChangeUserRoleDTOからF#のRoleに変換
    /// ユーザーロール変更用の安全な型変換
    /// </summary>
    /// <param name="dto">C#のChangeUserRoleDTO</param>
    /// <returns>F#のResult型（成功時はRole、失敗時はエラーメッセージ）</returns>
    public static FSharpResult<Role, string> FromDto(ChangeUserRoleDto dto)
    {
        return StringToRole(dto.NewRole);
    }

    /// <summary>
    /// Phase A2: C#のChangeEmailDTOからF#のEmailに変換
    /// メールアドレス変更用の安全な型変換
    /// </summary>
    /// <param name="dto">C#のChangeEmailDTO</param>
    /// <returns>F#のResult型（成功時はEmail、失敗時はエラーメッセージ）</returns>
    public static FSharpResult<Email, string> FromDto(ChangeEmailDto dto)
    {
        return Email.create(dto.NewEmail);
    }

    /// <summary>
    /// Phase A2: C#のChangePasswordDTOからF#のPasswordに変換
    /// パスワード変更用の安全な型変換（新しいパスワードのみ）
    /// </summary>
    /// <param name="dto">C#のChangePasswordDTO</param>
    /// <returns>F#のResult型（成功時はPassword、失敗時はエラーメッセージ）</returns>
    public static FSharpResult<Password, string> FromDto(ChangePasswordDto dto)
    {
        return Password.create(dto.NewPassword);
    }

    // =================================================================
    // 🔄 Phase A2: Value Object変換ヘルパーメソッド
    // =================================================================

    /// <summary>
    /// Phase A2: C#のstringからF#のPassword値オブジェクトを作成
    /// セキュリティポリシーに従ったパスワード検証付き
    /// </summary>
    /// <param name="passwordString">パスワード文字列</param>
    /// <returns>F#のResult型（成功時はPassword、失敗時はエラーメッセージ）</returns>
    public static FSharpResult<Password, string> CreatePassword(string passwordString)
    {
        return Password.create(passwordString);
    }

    /// <summary>
    /// Phase A2: C#のstringからF#のStrongEmail値オブジェクトを作成
    /// 強化されたメールアドレス検証付き
    /// </summary>
    /// <param name="emailString">メールアドレス文字列</param>
    /// <returns>F#のResult型（成功時はStrongEmail、失敗時はエラーメッセージ）</returns>
    public static FSharpResult<StrongEmail, string> CreateStrongEmail(string emailString)
    {
        return StrongEmail.create(emailString);
    }

    // 📝 注意: UserProfileは値オブジェクトではなくstring optionを使用しているため、
    // DisplayName、Department、PhoneNumber、Notes用の値オブジェクトは定義されていません。
    // 必要に応じて値オブジェクトとして定義することも可能です。

    // =================================================================
    // 🔐 Phase A9: 認証専用TypeConverter統合メソッド
    // =================================================================

    /// <summary>
    /// Phase A9: F# Result&lt;User, AuthenticationError&gt; を AuthenticationResultDto に変換
    /// 認証専用TypeConverterとの統合により、既存基盤で認証結果を処理
    /// AuthenticationConverter.ToDtoの統合版
    /// </summary>
    /// <param name="authResult">F#の認証結果</param>
    /// <returns>C#のAuthenticationResultDto</returns>
    public static AuthenticationResultDto ToDto(FSharpResult<User, AuthenticationError> authResult)
    {
        return AuthenticationConverter.ToDto(authResult);
    }

    /// <summary>
    /// Phase A9: F# AuthenticationError を AuthenticationErrorDto に変換
    /// 認証エラーの型安全な変換（既存基盤統合）
    /// AuthenticationConverter.ToDtoの統合版
    /// </summary>
    /// <param name="authError">F#の認証エラー</param>
    /// <returns>C#のAuthenticationErrorDto</returns>
    public static AuthenticationErrorDto ToDto(AuthenticationError authError)
    {
        return AuthenticationConverter.ToDto(authError);
    }

    /// <summary>
    /// Phase A9: LoginRequestDto を F# 認証パラメータに変換
    /// Web層からApplication層への型安全な変換（既存基盤統合）
    /// </summary>
    /// <param name="loginDto">ログインリクエストDTO</param>
    /// <returns>F#のResult型（Email*string or エラー）</returns>
    public static FSharpResult<Tuple<Email, string>, string> FromDto(LoginRequestDto loginDto)
    {
        return AuthenticationConverter.ToFSharpLoginParams(loginDto);
    }

    /// <summary>
    /// Phase A9: AuthenticationResultDto を F# Result型に変換
    /// C#からF#への逆変換（双方向変換完全対応）
    /// </summary>
    /// <param name="resultDto">C#の認証結果DTO</param>
    /// <returns>F#のResult型</returns>
    public static FSharpResult<User, AuthenticationError> ToFSharpResult(AuthenticationResultDto resultDto)
    {
        return AuthenticationConverter.ToFSharpResult(resultDto);
    }

    // =================================================================
    // 🔄 Phase A9: Infrastructure統合用変換メソッド
    // =================================================================

    // =================================================================
    // 📝 注意: Infrastructure層統合用変換メソッド
    // =================================================================
    // 【Clean Architecture遵守】
    // Contracts層からInfrastructure層への直接参照は依存方向違反のため削除
    // UserRepositoryAdapterで必要な変換は、Infrastructure層内で実装
}