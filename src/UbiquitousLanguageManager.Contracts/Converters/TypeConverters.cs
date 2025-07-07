using UbiquitousLanguageManager.Contracts.DTOs;
using UbiquitousLanguageManager.Domain;

namespace UbiquitousLanguageManager.Contracts.Converters;

/// <summary>
/// F#ドメインエンティティとC# DTOの型変換クラス
/// Clean Architectureの境界における言語間疎結合を実現
/// 手動変換方式を採用し、AutoMapperを使用せず確実性・性能・追跡可能性を重視
/// </summary>
public static class TypeConverters
{
    #region ユーザー型変換

    /// <summary>
    /// F#ユーザーエンティティからC# DTOへの変換
    /// </summary>
    /// <param name="user">F#ユーザーエンティティ</param>
    /// <returns>C# ユーザーDTO</returns>
    public static UserDto ToDto(User user)
    {
        // 🎯 F#レコード型からC#クラスへの手動変換
        // 型安全性を保ちながら言語間境界を越える
        return new UserDto
        {
            Id = user.Id.Item,  // 🔧 F#のタグ付きユニオンからプリミティブ値を抽出
            Email = user.Email.Value,  // 📧 F#のValueObjectから値を取得
            Name = user.Name.Value,    // 👤 Value Objectの値抽出
            Role = user.Role.ToString(),  // 🎭 F#判別共用体を文字列に変換
            IsActive = user.IsActive,
            IsFirstLogin = user.IsFirstLogin,
            UpdatedAt = user.UpdatedAt,
            UpdatedBy = user.UpdatedBy.Item
        };
    }

    /// <summary>
    /// C# DTOからF#ユーザーエンティティへの変換
    /// Value Objectの作成で検証も同時実行
    /// </summary>
    /// <param name="dto">C# ユーザーDTO</param>
    /// <returns>F#ユーザーエンティティ（Result型でラップ）</returns>
    public static Result<User, string> FromDto(UserDto dto)
    {
        // 🔍 Value Object作成による入力値検証
        var emailResult = Email.create(dto.Email);
        var nameResult = UserName.create(dto.Name);
        var roleResult = ParseUserRole(dto.Role);

        // 🎯 Result型を使用した関数型エラーハンドリング
        if (emailResult.IsSuccess && nameResult.IsSuccess && roleResult.IsSuccess)
        {
            var user = new User
            {
                Id = new UserId(dto.Id),
                Email = emailResult.ResultValue,  // 🔧 F#のResult型から成功値を取得
                Name = nameResult.ResultValue,
                Role = roleResult.ResultValue,
                IsActive = dto.IsActive,
                IsFirstLogin = dto.IsFirstLogin,
                UpdatedAt = dto.UpdatedAt,
                UpdatedBy = new UserId(dto.UpdatedBy)
            };

            return Result<User, string>.NewSuccess(user);
        }

        // ❌ 検証エラーの集約
        var errors = new List<string>();
        if (emailResult.IsError) errors.Add(emailResult.ErrorValue);
        if (nameResult.IsError) errors.Add(nameResult.ErrorValue);
        if (roleResult.IsError) errors.Add(roleResult.ErrorValue);

        return Result<User, string>.NewError(string.Join(", ", errors));
    }

    /// <summary>
    /// 文字列からUserRole判別共用体への変換
    /// </summary>
    private static Result<UserRole, string> ParseUserRole(string roleString)
    {
        return roleString?.ToLower() switch
        {
            "superuser" => Result<UserRole, string>.NewSuccess(UserRole.SuperUser),
            "projectmanager" => Result<UserRole, string>.NewSuccess(UserRole.ProjectManager),
            "domainapprover" => Result<UserRole, string>.NewSuccess(UserRole.DomainApprover),
            "generaluser" => Result<UserRole, string>.NewSuccess(UserRole.GeneralUser),
            _ => Result<UserRole, string>.NewError($"無効なユーザーロール: {roleString}")
        };
    }

    #endregion

    #region プロジェクト型変換

    /// <summary>
    /// F#プロジェクトエンティティからC# DTOへの変換
    /// </summary>
    public static ProjectDto ToDto(Project project)
    {
        return new ProjectDto
        {
            Id = project.Id.Item,
            Name = project.Name.Value,
            Description = project.Description.Value,
            IsActive = project.IsActive,
            UpdatedAt = project.UpdatedAt,
            UpdatedBy = project.UpdatedBy.Item
        };
    }

    /// <summary>
    /// C# DTOからF#プロジェクトエンティティへの変換
    /// </summary>
    public static Result<Project, string> FromDto(ProjectDto dto)
    {
        var nameResult = JapaneseName.create(dto.Name);
        var descriptionResult = Description.create(dto.Description);

        if (nameResult.IsSuccess && descriptionResult.IsSuccess)
        {
            var project = new Project
            {
                Id = new ProjectId(dto.Id),
                Name = nameResult.ResultValue,
                Description = descriptionResult.ResultValue,
                IsActive = dto.IsActive,
                UpdatedAt = dto.UpdatedAt,
                UpdatedBy = new UserId(dto.UpdatedBy)
            };

            return Result<Project, string>.NewSuccess(project);
        }

        var errors = new List<string>();
        if (nameResult.IsError) errors.Add(nameResult.ErrorValue);
        if (descriptionResult.IsError) errors.Add(descriptionResult.ErrorValue);

        return Result<Project, string>.NewError(string.Join(", ", errors));
    }

    #endregion

    #region ドメイン型変換

    /// <summary>
    /// F#ドメインエンティティからC# DTOへの変換
    /// </summary>
    public static DomainDto ToDto(Domain domain)
    {
        return new DomainDto
        {
            Id = domain.Id.Item,
            ProjectId = domain.ProjectId.Item,
            Name = domain.Name.Value,
            Description = domain.Description.Value,
            IsActive = domain.IsActive,
            UpdatedAt = domain.UpdatedAt,
            UpdatedBy = domain.UpdatedBy.Item
        };
    }

    /// <summary>
    /// C# DTOからF#ドメインエンティティへの変換
    /// </summary>
    public static Result<Domain, string> FromDto(DomainDto dto)
    {
        var nameResult = JapaneseName.create(dto.Name);
        var descriptionResult = Description.create(dto.Description);

        if (nameResult.IsSuccess && descriptionResult.IsSuccess)
        {
            var domain = new Domain
            {
                Id = new DomainId(dto.Id),
                ProjectId = new ProjectId(dto.ProjectId),
                Name = nameResult.ResultValue,
                Description = descriptionResult.ResultValue,
                IsActive = dto.IsActive,
                UpdatedAt = dto.UpdatedAt,
                UpdatedBy = new UserId(dto.UpdatedBy)
            };

            return Result<Domain, string>.NewSuccess(domain);
        }

        var errors = new List<string>();
        if (nameResult.IsError) errors.Add(nameResult.ErrorValue);
        if (descriptionResult.IsError) errors.Add(descriptionResult.ErrorValue);

        return Result<Domain, string>.NewError(string.Join(", ", errors));
    }

    #endregion

    #region ユビキタス言語型変換

    /// <summary>
    /// F#下書きユビキタス言語エンティティからC# DTOへの変換
    /// </summary>
    public static UbiquitousLanguageDto ToDto(DraftUbiquitousLanguage draft)
    {
        return new UbiquitousLanguageDto
        {
            Id = draft.Id.Item,
            DomainId = draft.DomainId.Item,
            JapaneseName = draft.JapaneseName.Value,
            EnglishName = draft.EnglishName.Value,
            Description = draft.Description.Value,
            Status = draft.Status.ToString(),
            UpdatedAt = draft.UpdatedAt,
            UpdatedBy = draft.UpdatedBy.Item,
            ApprovedAt = null,  // 下書きは未承認
            ApprovedBy = null   // 下書きは未承認
        };
    }

    /// <summary>
    /// F#正式ユビキタス言語エンティティからC# DTOへの変換
    /// </summary>
    public static UbiquitousLanguageDto ToDto(FormalUbiquitousLanguage formal)
    {
        return new UbiquitousLanguageDto
        {
            Id = formal.Id.Item,
            DomainId = formal.DomainId.Item,
            JapaneseName = formal.JapaneseName.Value,
            EnglishName = formal.EnglishName.Value,
            Description = formal.Description.Value,
            Status = "Approved",  // 正式版は常に承認済み
            UpdatedAt = formal.UpdatedAt,
            UpdatedBy = formal.UpdatedBy.Item,
            ApprovedAt = formal.ApprovedAt,
            ApprovedBy = formal.ApprovedBy.Item
        };
    }

    /// <summary>
    /// C# DTOからF#下書きユビキタス言語エンティティへの変換
    /// </summary>
    public static Result<DraftUbiquitousLanguage, string> FromDraftDto(UbiquitousLanguageDto dto)
    {
        var japaneseNameResult = JapaneseName.create(dto.JapaneseName);
        var englishNameResult = EnglishName.create(dto.EnglishName);
        var descriptionResult = Description.create(dto.Description);
        var statusResult = ParseApprovalStatus(dto.Status);

        if (japaneseNameResult.IsSuccess && englishNameResult.IsSuccess && 
            descriptionResult.IsSuccess && statusResult.IsSuccess)
        {
            var draft = new DraftUbiquitousLanguage
            {
                Id = new UbiquitousLanguageId(dto.Id),
                DomainId = new DomainId(dto.DomainId),
                JapaneseName = japaneseNameResult.ResultValue,
                EnglishName = englishNameResult.ResultValue,
                Description = descriptionResult.ResultValue,
                Status = statusResult.ResultValue,
                UpdatedAt = dto.UpdatedAt,
                UpdatedBy = new UserId(dto.UpdatedBy)
            };

            return Result<DraftUbiquitousLanguage, string>.NewSuccess(draft);
        }

        var errors = new List<string>();
        if (japaneseNameResult.IsError) errors.Add(japaneseNameResult.ErrorValue);
        if (englishNameResult.IsError) errors.Add(englishNameResult.ErrorValue);
        if (descriptionResult.IsError) errors.Add(descriptionResult.ErrorValue);
        if (statusResult.IsError) errors.Add(statusResult.ErrorValue);

        return Result<DraftUbiquitousLanguage, string>.NewError(string.Join(", ", errors));
    }

    /// <summary>
    /// 文字列からApprovalStatus判別共用体への変換
    /// </summary>
    private static Result<ApprovalStatus, string> ParseApprovalStatus(string statusString)
    {
        return statusString?.ToLower() switch
        {
            "draft" => Result<ApprovalStatus, string>.NewSuccess(ApprovalStatus.Draft),
            "submitted" => Result<ApprovalStatus, string>.NewSuccess(ApprovalStatus.Submitted),
            "approved" => Result<ApprovalStatus, string>.NewSuccess(ApprovalStatus.Approved),
            "rejected" => Result<ApprovalStatus, string>.NewSuccess(ApprovalStatus.Rejected),
            _ => Result<ApprovalStatus, string>.NewError($"無効な承認状態: {statusString}")
        };
    }

    #endregion
}