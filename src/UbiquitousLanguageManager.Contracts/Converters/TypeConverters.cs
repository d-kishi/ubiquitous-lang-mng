using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.FSharp.Core;
using Microsoft.FSharp.Collections;
using UbiquitousLanguageManager.Contracts.DTOs;
using UbiquitousLanguageManager.Contracts.DTOs.Authentication;
using UbiquitousLanguageManager.Contracts.DTOs.Application;
using UbiquitousLanguageManager.Domain.Common;
using UbiquitousLanguageManager.Domain.Authentication;
using UbiquitousLanguageManager.Domain.ProjectManagement;
using UbiquitousLanguageManager.Domain.UbiquitousLanguageManagement;
// UbiquitousLanguageManager.Domain.ProjectManagement.Domain型は使用箇所で直接指定
// Microsoft.FSharp.Core.FSharpResult型は使用箇所で直接指定

namespace UbiquitousLanguageManager.Contracts.Converters;

/// <summary>
/// F#ドメインエンティティとC# DTOの型変換クラス（完全版）
/// F#の強力な型システムの恩恵を活かしつつ、C#との間で安全かつ保守性の高いデータ変換を実現
/// Blazor Server初学者・F#初学者向けに詳細なコメントを記載
/// </summary>
public static class TypeConverters
{
    private static ILogger? _logger;

    /// <summary>
    /// ロガーを設定します（依存性注入で設定）
    /// TypeConverters でのログ出力を有効化
    /// </summary>
    /// <param name="logger">ILoggerインスタンス</param>
    public static void SetLogger(ILogger logger)
    {
        _logger = logger;
    }

    // =================================================================
    // 🔧 F# Option型変換ヘルパーメソッド（Phase B1 新規追加）
    // =================================================================

    /// <summary>
    /// F# Option&lt;string&gt; を C# string? に変換
    /// </summary>
    private static string? ConvertOptionString(FSharpOption<string> optionValue)
    {
        return FSharpOption<string>.get_IsNone(optionValue) ? null : optionValue.Value;
    }

    /// <summary>
    /// F# Option&lt;DateTime&gt; を C# DateTime? に変換
    /// </summary>
    private static DateTime? ConvertOptionDateTime(FSharpOption<DateTime> optionValue)
    {
        return FSharpOption<DateTime>.get_IsNone(optionValue) ? null : optionValue.Value;
    }

    /// <summary>
    /// F# string option を C# string に変換（null時は空文字列）
    /// </summary>
    private static string ConvertOptionStringToString(FSharpOption<string> optionValue)
    {
        return FSharpOption<string>.get_IsNone(optionValue) ? string.Empty : optionValue.Value ?? string.Empty;
    }
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
        var stopwatch = Stopwatch.StartNew();

        try
        {
            if (user == null)
            {
                _logger?.LogError("F# User→C# UserDTO変換失敗: Userエンティティがnull");
                throw new ArgumentNullException(nameof(user), "Userエンティティがnullです");
            }

            _logger?.LogDebug("F# User→C# UserDTO変換開始 UserId: {UserId}, Email: {Email}",
                user.Id.Value, user.Email.Value);

            var result = new UserDto
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

            _logger?.LogInformation("F# User→C# UserDTO変換成功 UserId: {UserId}, ConversionTime: {ConversionTime}ms",
                user.Id.Value, stopwatch.ElapsedMilliseconds);

            return result;
        }
        catch (Exception ex) when (!(ex is ArgumentNullException))
        {
            _logger?.LogError(ex, "F# User→C# UserDTO変換で予期しないエラーが発生 UserId: {UserId}, ConversionTime: {ConversionTime}ms",
                user?.Id?.Value ?? -1, stopwatch.ElapsedMilliseconds);
            throw;
        }
        finally
        {
            stopwatch.Stop();
        }
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
    /// Phase B1: F#のProjectエンティティをC#のProjectDTOに変換（拡張版）
    /// F#ドメインエンティティの完全な情報をC# DTOに型安全に変換
    /// CreatedAt、OwnerId等の新フィールドに対応
    /// </summary>
    /// <param name="project">F#で定義されたProjectエンティティ</param>
    /// <returns>C#のProjectDTO</returns>
    /// <exception cref="ArgumentNullException">projectがnullの場合</exception>
    public static ProjectDto ToDto(Project project)
    {
        var stopwatch = Stopwatch.StartNew();

        try
        {
            if (project == null)
            {
                _logger?.LogError("F# Project→C# ProjectDTO変換失敗: Projectエンティティがnull");
                throw new ArgumentNullException(nameof(project), "Projectエンティティがnullです");
            }

            _logger?.LogDebug("F# Project→C# ProjectDTO変換開始 ProjectId: {ProjectId}, Name: {Name}",
                project.Id.Value, project.Name.Value);

            // F#のProject型から新しいProjectDto仕様への完全変換
            var result = new ProjectDto
            {
                Id = project.Id.Value,                      // F#のProjectId判別共用体から値を取得
                Name = project.Name.Value,                   // F#のJapaneseName値オブジェクトから値を取得
                Description = ConvertOptionStringToString(project.Description.Value), // F# ProjectDescription Option型変換
                OwnerId = project.OwnerId.Value,             // F# UserId判別共用体（Phase B1で追加）
                CreatedAt = project.CreatedAt,               // F# DateTime（Phase B1で追加）
                UpdatedAt = ConvertOptionDateTime(project.UpdatedAt), // F# DateTime option変換
                UpdatedBy = project.OwnerId.Value,           // 暫定: 所有者をUpdatedByとして設定
                IsActive = project.IsActive,                // F#のIsActive（bool）
                Domains = new List<DomainDto>(),            // 関連ドメインは別途取得・設定
                MemberCount = 0                             // プロジェクト参加者数は別途算出・設定
            };

            _logger?.LogInformation("F# Project→C# ProjectDTO変換成功 ProjectId: {ProjectId}, ConversionTime: {ConversionTime}ms",
                project.Id.Value, stopwatch.ElapsedMilliseconds);

            return result;
        }
        catch (Exception ex) when (!(ex is ArgumentNullException))
        {
            _logger?.LogError(ex, "F# Project→C# ProjectDTO変換で予期しないエラーが発生 ProjectId: {ProjectId}, ConversionTime: {ConversionTime}ms",
                project?.Id?.Value ?? 0L, stopwatch.ElapsedMilliseconds);
            throw;
        }
        finally
        {
            stopwatch.Stop();
        }
    }

    /// <summary>
    /// F#のDomainエンティティをC#のDomainDTOに変換
    /// </summary>
    /// <param name="domain">F#で定義されたDomainエンティティ</param>
    /// <returns>C#のDomainDTO</returns>
    public static DomainDto ToDto(UbiquitousLanguageManager.Domain.ProjectManagement.Domain domain)
    {
        return new DomainDto
        {
            Id = domain.Id.Value,                   // F#のDomainId判別共用体から値を取得
            ProjectId = domain.ProjectId.Value,      // F#のProjectId判別共用体から値を取得
            Name = domain.Name.Value,                // F#のDomainName値オブジェクトから値を取得
            Description = ConvertOptionStringToString(domain.Description.Value),  // F#のProjectDescription Option型変換
            IsActive = domain.IsActive,
            UpdatedAt = ConvertOptionDateTime(domain.UpdatedAt) ?? domain.CreatedAt, // F# DateTime option変換（null時はCreatedAt使用）
            UpdatedBy = domain.OwnerId.Value         // 暫定: 所有者をUpdatedByとして設定
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
    public static Microsoft.FSharp.Core.FSharpResult<User, string> FromCreateDto(CreateUserDto dto)
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
            return Microsoft.FSharp.Core.FSharpResult<User, string>.NewOk(user);
        }
        else
        {
            // エラー収集：どのフィールドでエラーが発生したかを特定
            var errors = new List<string>();
            if (emailResult.IsError) errors.Add($"Email: {emailResult.ErrorValue}");
            if (nameResult.IsError) errors.Add($"Name: {nameResult.ErrorValue}");
            if (roleResult.IsError) errors.Add($"Role: {roleResult.ErrorValue}");
            if (createdByResult.IsError) errors.Add($"CreatedBy: {createdByResult.ErrorValue}");
            return Microsoft.FSharp.Core.FSharpResult<User, string>.NewError(string.Join(", ", errors));
        }
    }

    /// <summary>
    /// C#のCreateDomainDTOからF#のDomainエンティティを作成
    /// </summary>
    /// <param name="dto">C#のCreateDomainDTO</param>
    /// <returns>F#のResult型（成功時はDomain、失敗時はエラーメッセージ）</returns>
    public static Microsoft.FSharp.Core.FSharpResult<UbiquitousLanguageManager.Domain.ProjectManagement.Domain, string> FromCreateDto(CreateDomainDto dto)
    {
        var projectIdResult = CreateProjectId(dto.ProjectId);
        var nameResult = DomainName.create(dto.Name ?? "");
        var descriptionResult = Description.create(dto.Description ?? "");
        var createdByResult = CreateUserId(dto.CreatedBy);

        if (projectIdResult.IsOk && nameResult.IsOk && descriptionResult.IsOk && createdByResult.IsOk)
        {
            var domain = UbiquitousLanguageManager.Domain.ProjectManagement.Domain.create(nameResult.ResultValue, projectIdResult.ResultValue, createdByResult.ResultValue);
            return Microsoft.FSharp.Core.FSharpResult<UbiquitousLanguageManager.Domain.ProjectManagement.Domain, string>.NewOk(domain);
        }
        else
        {
            var errors = new List<string>();
            if (projectIdResult.IsError) errors.Add($"ProjectId: {projectIdResult.ErrorValue}");
            if (nameResult.IsError) errors.Add($"Name: {nameResult.ErrorValue}");
            if (descriptionResult.IsError) errors.Add($"Description: {descriptionResult.ErrorValue}");
            if (createdByResult.IsError) errors.Add($"CreatedBy: {createdByResult.ErrorValue}");
            return Microsoft.FSharp.Core.FSharpResult<UbiquitousLanguageManager.Domain.ProjectManagement.Domain, string>.NewError(string.Join(", ", errors));
        }
    }

    /// <summary>
    /// C#のCreateUbiquitousLanguageDTOからF#のDraftUbiquitousLanguageエンティティを作成
    /// </summary>
    /// <param name="dto">C#のCreateUbiquitousLanguageDTO</param>
    /// <returns>F#のResult型（成功時はDraftUbiquitousLanguage、失敗時はエラーメッセージ）</returns>
    public static Microsoft.FSharp.Core.FSharpResult<DraftUbiquitousLanguage, string> FromCreateDto(CreateUbiquitousLanguageDto dto)
    {
        var domainIdResult = CreateDomainId(dto.DomainId);
        var japaneseNameResult = JapaneseName.create(dto.JapaneseName ?? "");
        var englishNameResult = EnglishName.create(dto.EnglishName ?? "");
        var descriptionResult = Description.create(dto.Description ?? "");
        var createdByResult = CreateUserId(dto.CreatedBy);

        if (domainIdResult.IsOk && japaneseNameResult.IsOk && englishNameResult.IsOk && descriptionResult.IsOk && createdByResult.IsOk)
        {
            var draft = DraftUbiquitousLanguage.create(domainIdResult.ResultValue, japaneseNameResult.ResultValue, englishNameResult.ResultValue, descriptionResult.ResultValue, createdByResult.ResultValue);
            return Microsoft.FSharp.Core.FSharpResult<DraftUbiquitousLanguage, string>.NewOk(draft);
        }
        else
        {
            var errors = new List<string>();
            if (domainIdResult.IsError) errors.Add($"DomainId: {domainIdResult.ErrorValue}");
            if (japaneseNameResult.IsError) errors.Add($"JapaneseName: {japaneseNameResult.ErrorValue}");
            if (englishNameResult.IsError) errors.Add($"EnglishName: {englishNameResult.ErrorValue}");
            if (descriptionResult.IsError) errors.Add($"Description: {descriptionResult.ErrorValue}");
            if (createdByResult.IsError) errors.Add($"CreatedBy: {createdByResult.ErrorValue}");
            return Microsoft.FSharp.Core.FSharpResult<DraftUbiquitousLanguage, string>.NewError(string.Join(", ", errors));
        }
    }

    /// <summary>
    /// Phase B1: C#のCreateProjectCommandからF#ドメイン型への変換
    /// TypeConverter基盤拡張: プロジェクト作成コマンドの型安全変換
    /// F#のJapaneseName・Description・UserIdへの適切なマッピング
    /// </summary>
    /// <param name="command">C#のCreateProjectCommand</param>
    /// <returns>F#のResult型（成功時は変換済みパラメータタプル、失敗時はエラーメッセージ）</returns>
    public static Microsoft.FSharp.Core.FSharpResult<Tuple<JapaneseName, Description, UserId>, string> FromCreateDto(CreateProjectCommand command)
    {
        if (command == null)
            return Microsoft.FSharp.Core.FSharpResult<Tuple<JapaneseName, Description, UserId>, string>.NewError("CreateProjectCommandがnullです");

        // F#の値オブジェクトを使用してバリデーションを実施
        var nameResult = JapaneseName.create(command.Name ?? "");
        var descriptionResult = Description.create(command.Description ?? "");  // nullチェック対応
        var ownerIdResult = CreateUserId(command.OwnerId);

        // 複数のResult型をチェック（すべて成功した場合のみタプルを作成）
        if (nameResult.IsOk && descriptionResult.IsOk && ownerIdResult.IsOk)
        {
            var parameters = Tuple.Create(
                nameResult.ResultValue,
                descriptionResult.ResultValue,
                ownerIdResult.ResultValue
            );
            return Microsoft.FSharp.Core.FSharpResult<Tuple<JapaneseName, Description, UserId>, string>.NewOk(parameters);
        }
        else
        {
            // エラー収集：どのフィールドでエラーが発生したかを特定
            var errors = new List<string>();
            if (nameResult.IsError) errors.Add($"Name: {nameResult.ErrorValue}");
            if (descriptionResult.IsError) errors.Add($"Description: {descriptionResult.ErrorValue}");
            if (ownerIdResult.IsError) errors.Add($"OwnerId: {ownerIdResult.ErrorValue}");
            return Microsoft.FSharp.Core.FSharpResult<Tuple<JapaneseName, Description, UserId>, string>.NewError(string.Join(", ", errors));
        }
    }

    /// <summary>
    /// Phase B1 Application層拡張: CreateProjectCommandDtoからF#ドメイン型への変換
    /// 新しいApplication向けDTOsとの統合メソッド
    /// </summary>
    /// <param name="dto">Application向けCreateProjectCommandDto</param>
    /// <returns>F#のResult型（成功時は変換済みパラメータタプル、失敗時はエラーメッセージ）</returns>
    public static Microsoft.FSharp.Core.FSharpResult<Tuple<JapaneseName, Description, UserId>, string> FromCreateDto(CreateProjectCommandDto dto)
    {
        if (dto == null)
            return Microsoft.FSharp.Core.FSharpResult<Tuple<JapaneseName, Description, UserId>, string>.NewError("CreateProjectCommandDtoがnullです");

        // F#の値オブジェクトを使用してバリデーションを実施
        var nameResult = JapaneseName.create(dto.Name ?? "");
        var descriptionResult = Description.create(dto.Description ?? "");  // nullチェック対応
        var ownerIdResult = CreateUserId(dto.OwnerId);

        // 複数のResult型をチェック（すべて成功した場合のみタプルを作成）
        if (nameResult.IsOk && descriptionResult.IsOk && ownerIdResult.IsOk)
        {
            var parameters = Tuple.Create(
                nameResult.ResultValue,
                descriptionResult.ResultValue,
                ownerIdResult.ResultValue
            );
            return Microsoft.FSharp.Core.FSharpResult<Tuple<JapaneseName, Description, UserId>, string>.NewOk(parameters);
        }
        else
        {
            // エラー収集：どのフィールドでエラーが発生したかを特定
            var errors = new List<string>();
            if (nameResult.IsError) errors.Add($"Name: {nameResult.ErrorValue}");
            if (descriptionResult.IsError) errors.Add($"Description: {descriptionResult.ErrorValue}");
            if (ownerIdResult.IsError) errors.Add($"OwnerId: {ownerIdResult.ErrorValue}");
            return Microsoft.FSharp.Core.FSharpResult<Tuple<JapaneseName, Description, UserId>, string>.NewError(string.Join(", ", errors));
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
    private static Microsoft.FSharp.Core.FSharpResult<Role, string> StringToRole(string roleString)
    {
        // F#の判別共用体は、C#からはNewXxxという静的メソッドでケースを作成
        return roleString switch
        {
            "SuperUser" => Microsoft.FSharp.Core.FSharpResult<Role, string>.NewOk(Role.SuperUser),
            "ProjectManager" => Microsoft.FSharp.Core.FSharpResult<Role, string>.NewOk(Role.ProjectManager),
            "DomainApprover" => Microsoft.FSharp.Core.FSharpResult<Role, string>.NewOk(Role.DomainApprover),
            "GeneralUser" => Microsoft.FSharp.Core.FSharpResult<Role, string>.NewOk(Role.GeneralUser),
            _ => Microsoft.FSharp.Core.FSharpResult<Role, string>.NewError($"無効なユーザーロールです: {roleString}")
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
    private static Microsoft.FSharp.Core.FSharpResult<Permission, string> StringToPermission(string permissionString)
    {
        return permissionString switch
        {
            "ViewUsers" => Microsoft.FSharp.Core.FSharpResult<Permission, string>.NewOk(Permission.ViewUsers),
            "CreateUsers" => Microsoft.FSharp.Core.FSharpResult<Permission, string>.NewOk(Permission.CreateUsers),
            "EditUsers" => Microsoft.FSharp.Core.FSharpResult<Permission, string>.NewOk(Permission.EditUsers),
            "DeleteUsers" => Microsoft.FSharp.Core.FSharpResult<Permission, string>.NewOk(Permission.DeleteUsers),
            "ManageUserRoles" => Microsoft.FSharp.Core.FSharpResult<Permission, string>.NewOk(Permission.ManageUserRoles),
            "ViewProjects" => Microsoft.FSharp.Core.FSharpResult<Permission, string>.NewOk(Permission.ViewProjects),
            "CreateProjects" => Microsoft.FSharp.Core.FSharpResult<Permission, string>.NewOk(Permission.CreateProjects),
            "ManageProjects" => Microsoft.FSharp.Core.FSharpResult<Permission, string>.NewOk(Permission.ManageProjects),
            "DeleteProjects" => Microsoft.FSharp.Core.FSharpResult<Permission, string>.NewOk(Permission.DeleteProjects),
            "ViewDomains" => Microsoft.FSharp.Core.FSharpResult<Permission, string>.NewOk(Permission.ViewDomains),
            "ManageDomains" => Microsoft.FSharp.Core.FSharpResult<Permission, string>.NewOk(Permission.ManageDomains),
            "ApproveDomains" => Microsoft.FSharp.Core.FSharpResult<Permission, string>.NewOk(Permission.ApproveDomains),
            "ViewUbiquitousLanguages" => Microsoft.FSharp.Core.FSharpResult<Permission, string>.NewOk(Permission.ViewUbiquitousLanguages),
            "CreateUbiquitousLanguages" => Microsoft.FSharp.Core.FSharpResult<Permission, string>.NewOk(Permission.CreateUbiquitousLanguages),
            "EditUbiquitousLanguages" => Microsoft.FSharp.Core.FSharpResult<Permission, string>.NewOk(Permission.EditUbiquitousLanguages),
            "ApproveUbiquitousLanguages" => Microsoft.FSharp.Core.FSharpResult<Permission, string>.NewOk(Permission.ApproveUbiquitousLanguages),
            "ManageSystemSettings" => Microsoft.FSharp.Core.FSharpResult<Permission, string>.NewOk(Permission.ManageSystemSettings),
            _ => Microsoft.FSharp.Core.FSharpResult<Permission, string>.NewError($"無効な権限です: {permissionString}")
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
    private static Microsoft.FSharp.Core.FSharpResult<ApprovalStatus, string> StringToApprovalStatus(string statusString)
    {
        return statusString switch
        {
            "Draft" => Microsoft.FSharp.Core.FSharpResult<ApprovalStatus, string>.NewOk(ApprovalStatus.Draft),
            "Submitted" => Microsoft.FSharp.Core.FSharpResult<ApprovalStatus, string>.NewOk(ApprovalStatus.Submitted),
            "Approved" => Microsoft.FSharp.Core.FSharpResult<ApprovalStatus, string>.NewOk(ApprovalStatus.Approved),
            "Rejected" => Microsoft.FSharp.Core.FSharpResult<ApprovalStatus, string>.NewOk(ApprovalStatus.Rejected),
            _ => Microsoft.FSharp.Core.FSharpResult<ApprovalStatus, string>.NewError($"無効な承認ステータスです: {statusString}")
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
    private static Microsoft.FSharp.Core.FSharpResult<UserId, string> CreateUserId(long id)
    {
        if (id <= 0)
            return Microsoft.FSharp.Core.FSharpResult<UserId, string>.NewError("ユーザーIDは正の値である必要があります");
        return Microsoft.FSharp.Core.FSharpResult<UserId, string>.NewOk(UserId.NewUserId(id));
    }

    /// <summary>
    /// long型からF#のProjectId判別共用体を作成
    /// </summary>
    /// <param name="id">プロジェクトID</param>
    /// <returns>F#のResult型（成功時はProjectId、失敗時はエラーメッセージ）</returns>
    private static Microsoft.FSharp.Core.FSharpResult<ProjectId, string> CreateProjectId(long id)
    {
        if (id <= 0)
            return Microsoft.FSharp.Core.FSharpResult<ProjectId, string>.NewError("プロジェクトIDは正の値である必要があります");
        return Microsoft.FSharp.Core.FSharpResult<ProjectId, string>.NewOk(ProjectId.NewProjectId(id));
    }

    /// <summary>
    /// long型からF#のDomainId判別共用体を作成
    /// </summary>
    /// <param name="id">ドメインID</param>
    /// <returns>F#のResult型（成功時はDomainId、失敗時はエラーメッセージ）</returns>
    private static Microsoft.FSharp.Core.FSharpResult<DomainId, string> CreateDomainId(long id)
    {
        if (id <= 0)
            return Microsoft.FSharp.Core.FSharpResult<DomainId, string>.NewError("ドメインIDは正の値である必要があります");
        return Microsoft.FSharp.Core.FSharpResult<DomainId, string>.NewOk(DomainId.NewDomainId(id));
    }

    /// <summary>
    /// long型からF#のUbiquitousLanguageId判別共用体を作成
    /// </summary>
    /// <param name="id">ユビキタス言語ID</param>
    /// <returns>F#のResult型（成功時はUbiquitousLanguageId、失敗時はエラーメッセージ）</returns>
    private static Microsoft.FSharp.Core.FSharpResult<UbiquitousLanguageId, string> CreateUbiquitousLanguageId(long id)
    {
        if (id <= 0)
            return Microsoft.FSharp.Core.FSharpResult<UbiquitousLanguageId, string>.NewError("ユビキタス言語IDは正の値である必要があります");
        return Microsoft.FSharp.Core.FSharpResult<UbiquitousLanguageId, string>.NewOk(UbiquitousLanguageId.NewUbiquitousLanguageId(id));
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
    public static Microsoft.FSharp.Core.FSharpResult<UserProfile, string> FromDto(UserProfileDto dto)
    {
        // F#初学者向け解説: UserProfileはstring optionを使用しているため、
        // 値オブジェクトではなく、直接文字列を渡す
        var profile = UserProfile.create(dto.DisplayName, dto.Department, dto.PhoneNumber, dto.Notes);
        return Microsoft.FSharp.Core.FSharpResult<UserProfile, string>.NewOk(profile);
    }

    /// <summary>
    /// Phase A2: C#のProjectPermissionDTOからF#のProjectPermissionを作成
    /// 権限文字列リストをF#のSet&lt;Permission&gt;に変換
    /// </summary>
    /// <param name="dto">C#のProjectPermissionDTO</param>
    /// <returns>F#のResult型（成功時はProjectPermission、失敗時はエラーメッセージ）</returns>
    public static Microsoft.FSharp.Core.FSharpResult<ProjectPermission, string> FromDto(ProjectPermissionDto dto)
    {
        // ProjectId作成
        var projectIdResult = CreateProjectId(dto.ProjectId);
        if (projectIdResult.IsError)
            return Microsoft.FSharp.Core.FSharpResult<ProjectPermission, string>.NewError(projectIdResult.ErrorValue);

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
            return Microsoft.FSharp.Core.FSharpResult<ProjectPermission, string>.NewError($"無効な権限が含まれています: {string.Join(", ", errors)}");
        }

        // F#のList型に変換（ProjectPermission.createはListを期待）
        var permissionList = ListModule.OfSeq(permissions);
        
        // ProjectPermission作成
        var projectPermission = ProjectPermission.create(projectIdResult.ResultValue, permissionList);
        return Microsoft.FSharp.Core.FSharpResult<ProjectPermission, string>.NewOk(projectPermission);
    }

    /// <summary>
    /// Phase A2: C#のChangeUserRoleDTOからF#のRoleに変換
    /// ユーザーロール変更用の安全な型変換
    /// </summary>
    /// <param name="dto">C#のChangeUserRoleDTO</param>
    /// <returns>F#のResult型（成功時はRole、失敗時はエラーメッセージ）</returns>
    public static Microsoft.FSharp.Core.FSharpResult<Role, string> FromDto(ChangeUserRoleDto dto)
    {
        return StringToRole(dto.NewRole);
    }

    /// <summary>
    /// Phase A2: C#のChangeEmailDTOからF#のEmailに変換
    /// メールアドレス変更用の安全な型変換
    /// </summary>
    /// <param name="dto">C#のChangeEmailDTO</param>
    /// <returns>F#のResult型（成功時はEmail、失敗時はエラーメッセージ）</returns>
    public static Microsoft.FSharp.Core.FSharpResult<Email, string> FromDto(ChangeEmailDto dto)
    {
        return Email.create(dto.NewEmail);
    }

    /// <summary>
    /// Phase A2: C#のChangePasswordDTOからF#のPasswordに変換
    /// パスワード変更用の安全な型変換（新しいパスワードのみ）
    /// </summary>
    /// <param name="dto">C#のChangePasswordDTO</param>
    /// <returns>F#のResult型（成功時はPassword、失敗時はエラーメッセージ）</returns>
    public static Microsoft.FSharp.Core.FSharpResult<Password, string> FromDto(ChangePasswordDto dto)
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
    public static Microsoft.FSharp.Core.FSharpResult<Password, string> CreatePassword(string passwordString)
    {
        return Password.create(passwordString);
    }

    /// <summary>
    /// Phase A2: C#のstringからF#のStrongEmail値オブジェクトを作成
    /// 強化されたメールアドレス検証付き
    /// </summary>
    /// <param name="emailString">メールアドレス文字列</param>
    /// <returns>F#のResult型（成功時はStrongEmail、失敗時はエラーメッセージ）</returns>
    public static Microsoft.FSharp.Core.FSharpResult<StrongEmail, string> CreateStrongEmail(string emailString)
    {
        return StrongEmail.create(emailString);
    }

    // 📝 注意: UserProfileは値オブジェクトではなくstring optionを使用しているため、
    // DisplayName、Department、PhoneNumber、Notes用の値オブジェクトは定義されていません。
    // 必要に応じて値オブジェクトとして定義することも可能です。

    // =================================================================
    // 🏗️ Phase B1: プロジェクト関連Result型変換メソッド
    // =================================================================

    /// <summary>
    /// Phase B1: F# Project作成結果をC# ProjectCreationResultに変換
    /// Railway-oriented Programming結果の型安全な変換
    /// TypeConverter基盤拡張: F# Result&lt;Project * Domain, ProjectCreationError&gt; → C# ProjectCreationResult
    /// </summary>
    /// <param name="domainResult">F#のプロジェクト作成結果（Project * Domain のペア）</param>
    /// <returns>C#のProjectCreationResult</returns>
    public static ProjectCreationResult ToProjectCreationResult<TError>(
        Microsoft.FSharp.Core.FSharpResult<Tuple<Project, UbiquitousLanguageManager.Domain.ProjectManagement.Domain>, TError> domainResult)
        where TError : class
    {
        if (domainResult.IsOk)
        {
            var (project, domain) = domainResult.ResultValue;

            return ProjectCreationResult.Success(
                ToDto(project),
                ToDto(domain)
            );
        }
        else
        {
            var error = domainResult.ErrorValue;
            var (errorType, errorMessage) = ConvertProjectCreationError(error);

            return ProjectCreationResult.Failure(errorType, errorMessage);
        }
    }

    /// <summary>
    /// Phase B1: F#プロジェクト作成エラーをC#エラータイプ・メッセージに変換
    /// F#判別共用体エラーの型安全な変換ヘルパー
    /// </summary>
    /// <param name="error">F#のプロジェクト作成エラー</param>
    /// <returns>C#のエラータイプとメッセージのタプル</returns>
    private static (ProjectCreationErrorType ErrorType, string ErrorMessage) ConvertProjectCreationError<TError>(TError error)
        where TError : class
    {
        // F#の判別共用体エラー型に応じた変換
        // 注意: F#の判別共用体は実行時型チェックが必要
        var errorString = error?.ToString() ?? "不明なエラー";

        // F#の判別共用体パターンマッチングをC#で模倣
        // 実際のF#判別共用体の実装に応じて調整が必要
        if (errorString.Contains("DuplicateProjectName"))
        {
            return (ProjectCreationErrorType.DuplicateProjectName,
                   $"プロジェクト名が重複しています: {ExtractErrorValue(errorString)}");
        }
        else if (errorString.Contains("InvalidProjectName"))
        {
            return (ProjectCreationErrorType.InvalidProjectName,
                   $"無効なプロジェクト名: {ExtractErrorValue(errorString)}");
        }
        else if (errorString.Contains("InvalidProjectDescription"))
        {
            return (ProjectCreationErrorType.InvalidProjectDescription,
                   $"無効なプロジェクト説明: {ExtractErrorValue(errorString)}");
        }
        else if (errorString.Contains("DatabaseError"))
        {
            return (ProjectCreationErrorType.DatabaseError,
                   $"データベースエラー: {ExtractErrorValue(errorString)}");
        }
        else if (errorString.Contains("DomainCreationFailed"))
        {
            return (ProjectCreationErrorType.DomainCreationFailed,
                   $"デフォルトドメイン作成エラー: {ExtractErrorValue(errorString)}");
        }
        else
        {
            return (ProjectCreationErrorType.DatabaseError, $"不明なエラーが発生しました: {errorString}");
        }
    }

    /// <summary>
    /// Phase B1: F#エラー文字列から値部分を抽出
    /// F#判別共用体の値を含むエラーメッセージから実際の値を取得
    /// </summary>
    /// <param name="errorString">F#エラーの文字列表現</param>
    /// <returns>抽出された値</returns>
    private static string ExtractErrorValue(string errorString)
    {
        // F#判別共用体の文字列表現から値部分を抽出
        // 例: "DuplicateProjectName(\"既存プロジェクト名\")" → "既存プロジェクト名"
        var match = System.Text.RegularExpressions.Regex.Match(errorString, @"\""([^""]*)\""");
        return match.Success ? match.Groups[1].Value : errorString;
    }

    /// <summary>
    /// Phase B1: Project関連の便利メソッド
    /// 既存TypeConverterとの統合パターン
    /// </summary>
    /// <param name="project">F#プロジェクトエンティティ</param>
    /// <param name="owner">プロジェクト所有者</param>
    /// <returns>所有者情報付きProjectDto</returns>
    public static ProjectDto ToProjectDtoWithOwner(Project project, User owner)
    {
        var projectDto = ToDto(project);
        // 所有者情報を適切に設定（F#のProject型にOwnerIdがない場合の対応）
        projectDto.OwnerId = owner.Id.Value;
        return projectDto;
    }

    /// <summary>
    /// プロジェクトリストの一括変換（パフォーマンス最適化）
    /// </summary>
    /// <param name="projects">F#プロジェクトエンティティリスト</param>
    /// <returns>ProjectDtoリスト</returns>
    public static List<ProjectDto> ToProjectDtos(IEnumerable<Project> projects)
    {
        return projects.Select(ToDto).ToList();
    }

    /// <summary>
    /// アクティブなプロジェクトのみをフィルタリングして変換
    /// </summary>
    /// <param name="projects">F#プロジェクトエンティティリスト</param>
    /// <returns>アクティブなProjectDtoリスト</returns>
    public static List<ProjectDto> ToActiveProjectDtos(IEnumerable<Project> projects)
    {
        return projects
            .Where(p => p.IsActive)
            .Select(ToDto)
            .ToList();
    }

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
    public static AuthenticationResultDto ToDto(Microsoft.FSharp.Core.FSharpResult<User, AuthenticationError> authResult)
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
    public static Microsoft.FSharp.Core.FSharpResult<Tuple<Email, string>, string> FromDto(LoginRequestDto loginDto)
    {
        return AuthenticationConverter.ToFSharpLoginParams(loginDto);
    }

    /// <summary>
    /// Phase A9: AuthenticationResultDto を F# Result型に変換
    /// C#からF#への逆変換（双方向変換完全対応）
    /// </summary>
    /// <param name="resultDto">C#の認証結果DTO</param>
    /// <returns>F#のResult型</returns>
    public static Microsoft.FSharp.Core.FSharpResult<User, AuthenticationError> ToFSharpResult(AuthenticationResultDto resultDto)
    {
        return AuthenticationConverter.ToFSharpResult(resultDto);
    }

    // =================================================================
    // 🔄 Phase A9: パスワードリセット関連TypeConverter統合
    // =================================================================

    /// <summary>
    /// Phase A9: PasswordResetRequestDto を F# パラメータに変換
    /// TypeConverter統合によりパスワードリセット要求を型安全に変換
    /// AuthenticationConverter.ToFSharpPasswordResetParamsの統合版
    /// </summary>
    /// <param name="resetDto">パスワードリセット要求DTO</param>
    /// <returns>F#のResult型（Email or エラー）</returns>
    public static Microsoft.FSharp.Core.FSharpResult<Email, string> FromDto(PasswordResetRequestDto resetDto)
    {
        return AuthenticationConverter.ToFSharpPasswordResetParams(resetDto);
    }

    /// <summary>
    /// Phase A9: PasswordResetTokenDto を F# パラメータに変換
    /// TypeConverter統合によりパスワードリセット実行を型安全に変換
    /// AuthenticationConverter.ToFSharpPasswordResetExecuteParamsの統合版
    /// </summary>
    /// <param name="tokenDto">パスワードリセットトークンDTO</param>
    /// <returns>F#のResult型（Email*Token*Password or エラー）</returns>
    public static Microsoft.FSharp.Core.FSharpResult<Tuple<Email, string, string>, string> FromDto(PasswordResetTokenDto tokenDto)
    {
        return AuthenticationConverter.ToFSharpPasswordResetExecuteParams(tokenDto);
    }

    /// <summary>
    /// Phase A9: F# パスワードリセット結果 を PasswordResetResultDto に変換
    /// TypeConverter統合によりパスワードリセット結果を型安全に変換
    /// AuthenticationConverter.ToPasswordResetResultDtoの統合版
    /// </summary>
    /// <param name="result">F#のパスワードリセット結果</param>
    /// <param name="userEmail">対象ユーザーのメールアドレス</param>
    /// <returns>C#のPasswordResetResultDto</returns>
    public static PasswordResetResultDto ToDto<T>(Microsoft.FSharp.Core.FSharpResult<T, AuthenticationError> result, string userEmail)
    {
        return AuthenticationConverter.ToPasswordResetResultDto(result, userEmail);
    }

    // =================================================================
    // 🔄 Phase A9: 拡張認証エラー・結果変換（将来のF#拡張対応）
    // =================================================================

    /// <summary>
    /// Phase A9: 拡張AuthenticationErrorDto を F# AuthenticationError に変換
    /// 新規追加エラー型対応・将来のF#拡張に備えた型安全変換
    /// </summary>
    /// <param name="errorDto">拡張版AuthenticationErrorDto</param>
    /// <returns>F#のAuthenticationError判別共用体</returns>
    public static AuthenticationError ToFSharpAuthenticationError(AuthenticationErrorDto errorDto)
    {
        return AuthenticationConverter.ToFSharpAuthenticationErrorExtended(errorDto);
    }

    /// <summary>
    /// Phase A9: F# Result型の高度な変換
    /// Railway-oriented Programming結果の包括的変換
    /// 複数の結果型に対応した汎用変換メソッド
    /// </summary>
    /// <typeparam name="TSuccess">F#の成功型</typeparam>
    /// <param name="result">F#のResult&lt;TSuccess, AuthenticationError&gt;</param>
    /// <param name="successConverter">成功時の変換関数</param>
    /// <returns>C#のAuthenticationResultDto</returns>
    public static AuthenticationResultDto ToDto<TSuccess>(
        Microsoft.FSharp.Core.FSharpResult<TSuccess, AuthenticationError> result,
        Func<TSuccess, UserDto> successConverter)
    {
        if (result.IsOk)
        {
            var successValue = result.ResultValue;
            var userDto = successConverter(successValue);
            return AuthenticationResultDto.Success(userDto);
        }
        else
        {
            var error = result.ErrorValue;
            var errorDto = ToDto(error);
            return AuthenticationResultDto.Failure(errorDto);
        }
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