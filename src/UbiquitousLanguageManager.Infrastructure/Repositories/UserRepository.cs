using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.FSharp.Core;
using UbiquitousLanguageManager.Application;
using UbiquitousLanguageManager.Domain.Common;
using UbiquitousLanguageManager.Domain.Authentication;
using UbiquitousLanguageManager.Infrastructure.Data;
using UbiquitousLanguageManager.Infrastructure.Data.Entities;
// Application.ProjectManagement層のインターフェース参照（Phase B-F3）
using PmIUserRepository = UbiquitousLanguageManager.Application.ProjectManagement.IUserRepository;

namespace UbiquitousLanguageManager.Infrastructure.Repositories;

/// <summary>
/// Phase A9: F# Application層認証サービス統合用UserRepository
/// ASP.NET Core Identity → F# Domain型 完全変換Adapter
/// Clean Architecture依存方向遵守: Infrastructure → Application
/// 【Infrastructure層初学者向け解説】
/// このAdapterは、ASP.NET Core Identityの複雑なUserManager機能を、
/// F# Application層のシンプルなIUserRepositoryインターフェースに適応させます。
/// Railway-oriented ProgrammingのResult型を使用して、エラーハンドリングを一元化します。
/// </summary>
public class UserRepository :
    IUserRepository, // Application.IUserRepository（既存）
    PmIUserRepository // Application.ProjectManagement.IUserRepository（追加）
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly Microsoft.Extensions.Logging.ILogger<UserRepository> _logger;
    private readonly UbiquitousLanguageDbContext _context;

    /// <summary>
    /// UserRepositoryコンストラクタ
    /// ASP.NET Core Identity UserManager依存注入
    /// 【DI初学者向け解説】
    /// UserManagerは、ASP.NET Core Identityの中核機能です。
    /// パスワードハッシュ化、ユーザー検索、ロックアウト管理など、
    /// 認証関連の全機能を提供します。
    /// </summary>
    public UserRepository(
        UserManager<ApplicationUser> userManager,
        Microsoft.Extensions.Logging.ILogger<UserRepository> logger,
        UbiquitousLanguageDbContext context)
    {
        _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    // =================================================================
    // 🔍 F# IUserRepository実装メソッド
    // =================================================================

    /// <summary>
    /// F#メールアドレス検索: ASP.NET Core Identity統合版
    /// Email値オブジェクト → ApplicationUser → F# User変換
    /// 【F#初学者向け解説】
    /// F#のEmail値オブジェクトから文字列を取得し、
    /// ASP.NET Core IdentityのFindByEmailAsyncで検索を行い、
    /// 結果をF#のUser型とOption型で安全に返します。
    /// </summary>
    public async Task<FSharpResult<FSharpOption<User>, string>> GetByEmailAsync(Email email)
    {
        if (email == null)
        {
            _logger.LogWarning("GetByEmailAsync called with null email");
            return FSharpResult<FSharpOption<User>, string>.NewError("メールアドレスがnullです");
        }

        try
        {
            _logger.LogDebug("Searching user by email: {Email}", email.Value);
            
            // ASP.NET Core Identity検索実行
            var appUser = await _userManager.FindByEmailAsync(email.Value);
            
            if (appUser == null)
            {
                _logger.LogInformation("User not found for email: {Email}", email.Value);
                // F# Option.None: ユーザー未存在の安全な表現
                return FSharpResult<FSharpOption<User>, string>.NewOk(FSharpOption<User>.None);
            }

            // ApplicationUser → F# User変換（Infrastructure層内変換・非同期）
            var userConversionResult = await ConvertToFSharpUserAsync(appUser);

            if (userConversionResult.IsError)
            {
                _logger.LogError("User conversion failed for email {Email}: {Error}",
                    email.Value, userConversionResult.ErrorValue);
                return FSharpResult<FSharpOption<User>, string>.NewError(
                    $"ユーザー変換エラー: {userConversionResult.ErrorValue}");
            }

            _logger.LogInformation("User successfully retrieved for email: {Email}", email.Value);
            // F# Option.Some: ユーザー存在の型安全な表現
            var userOption = FSharpOption<User>.Some(userConversionResult.ResultValue);
            return FSharpResult<FSharpOption<User>, string>.NewOk(userOption);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception in GetByEmailAsync for email: {Email}", email.Value);
            return FSharpResult<FSharpOption<User>, string>.NewError(
                $"データベース検索エラー: {ex.Message}");
        }
    }

    /// <summary>
    /// F#ユーザーID検索: ASP.NET Core Identity統合版
    /// F# UserId → GUID文字列変換 → ApplicationUser検索
    /// 【F#初学者向け解説】
    /// F#のUserId型は、long値を内包した値オブジェクトです。
    /// ASP.NET Core IdentityはGUID文字列をキーとして使用するため、
    /// 適切な変換処理を行って検索します。
    /// 【注意】GetHashCode()を使用した合成GUID変換は衝突リスクがあります。
    /// 可能であればGetByIdentityIdAsync()の使用を推奨します。
    /// </summary>
    public async Task<FSharpResult<FSharpOption<User>, string>> GetByIdAsync(UserId userId)
    {
        if (userId == null)
        {
            _logger.LogWarning("GetByIdAsync called with null userId");
            return FSharpResult<FSharpOption<User>, string>.NewError("ユーザーIDがnullです");
        }

        try
        {
            _logger.LogDebug("Searching user by ID: {UserId}", userId.Item);
            _logger.LogWarning("GetByIdAsync uses synthetic GUID conversion which may cause collisions. Consider using GetByIdentityIdAsync instead.");

            // F# UserId → GUID文字列変換
            // 【重要】この変換は、UserRepositoryのToEntity/ToDomainUserメソッドと整合性が必要
            // 【警告】GetHashCode()ベースの変換は衝突リスクあり
            var guidId = ConvertUserIdToGuid(userId);
            
            // ASP.NET Core Identity検索実行
            var appUser = await _userManager.FindByIdAsync(guidId);
            
            if (appUser == null)
            {
                _logger.LogInformation("User not found for ID: {UserId}", userId.Item);
                return FSharpResult<FSharpOption<User>, string>.NewOk(FSharpOption<User>.None);
            }

            // ApplicationUser → F# User変換（非同期）
            var userConversionResult = await ConvertToFSharpUserAsync(appUser);

            if (userConversionResult.IsError)
            {
                _logger.LogError("User conversion failed for ID {UserId}: {Error}",
                    userId.Item, userConversionResult.ErrorValue);
                return FSharpResult<FSharpOption<User>, string>.NewError(
                    $"ユーザー変換エラー: {userConversionResult.ErrorValue}");
            }

            _logger.LogInformation("User successfully retrieved for ID: {UserId}", userId.Item);
            var userOption = FSharpOption<User>.Some(userConversionResult.ResultValue);
            return FSharpResult<FSharpOption<User>, string>.NewOk(userOption);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception in GetByIdAsync for ID: {UserId}", userId.Item);
            return FSharpResult<FSharpOption<User>, string>.NewError(
                $"データベース検索エラー: {ex.Message}");
        }
    }

    /// <summary>
    /// F#ユーザーID検索（Identity文字列ID版）: ASP.NET Core Identity統合版
    /// ASP.NET Identity の文字列 ID で直接検索（GetHashCode()不安定問題の回避）
    /// 【F#初学者向け解説】
    /// GetHashCode()は.NET Coreでプロセスごとにランダム化されるため、
    /// ASP.NET IdentityのGUID文字列IDで直接検索する方が安全です。
    /// </summary>
    public async Task<FSharpResult<FSharpOption<User>, string>> GetByIdentityIdAsync(string identityId)
    {
        if (string.IsNullOrWhiteSpace(identityId))
        {
            _logger.LogWarning("GetByIdentityIdAsync called with null or empty identityId");
            return FSharpResult<FSharpOption<User>, string>.NewError("Identity IDが無効です");
        }

        try
        {
            _logger.LogDebug("Searching user by Identity ID: {IdentityId}", identityId);

            // ASP.NET Core Identity直接検索実行
            var appUser = await _userManager.FindByIdAsync(identityId);

            if (appUser == null)
            {
                _logger.LogInformation("User not found for Identity ID: {IdentityId}", identityId);
                return FSharpResult<FSharpOption<User>, string>.NewOk(FSharpOption<User>.None);
            }

            // ApplicationUser → F# User変換（非同期）
            var userConversionResult = await ConvertToFSharpUserAsync(appUser);

            if (userConversionResult.IsError)
            {
                _logger.LogError("User conversion failed for Identity ID {IdentityId}: {Error}",
                    identityId, userConversionResult.ErrorValue);
                return FSharpResult<FSharpOption<User>, string>.NewError(
                    $"ユーザー変換エラー: {userConversionResult.ErrorValue}");
            }

            _logger.LogInformation("User successfully retrieved for Identity ID: {IdentityId}", identityId);
            var userOption = FSharpOption<User>.Some(userConversionResult.ResultValue);
            return FSharpResult<FSharpOption<User>, string>.NewOk(userOption);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception in GetByIdentityIdAsync for Identity ID: {IdentityId}", identityId);
            return FSharpResult<FSharpOption<User>, string>.NewError(
                $"データベース検索エラー: {ex.Message}");
        }
    }

    /// <summary>
    /// F#ユーザー保存: ASP.NET Core Identity統合版
    /// F# User → ApplicationUser変換 → Identity更新
    /// 【F#初学者向け解説】
    /// F#のUser型をApplicationUserに変換し、
    /// ASP.NET Core IdentityのUpdateAsyncで永続化します。
    /// 関連するセキュリティ情報（SecurityStamp等）も適切に更新されます。
    /// </summary>
    public async Task<FSharpResult<User, string>> SaveAsync(User user)
    {
        if (user == null)
        {
            _logger.LogWarning("SaveAsync called with null user");
            return FSharpResult<User, string>.NewError("ユーザーがnullです");
        }

        try
        {
            _logger.LogDebug("Saving user: {Email}", user.Email.Value);
            
            // F# User → ApplicationUser変換
            var appUser = ConvertToApplicationUser(user);
            
            // 既存ユーザーの検索（更新の場合）
            var existingUser = await _userManager.FindByEmailAsync(user.Email.Value);
            
            IdentityResult result;
            
            if (existingUser == null)
            {
                // 新規作成
                _logger.LogInformation("Creating new user: {Email}", user.Email.Value);
                result = await _userManager.CreateAsync(appUser);
            }
            else
            {
                // 更新処理：重要なプロパティのみコピー
                _logger.LogInformation("Updating existing user: {Email}", user.Email.Value);
                
                // 更新可能フィールドのコピー
                existingUser.Name = appUser.Name;
                existingUser.IsFirstLogin = appUser.IsFirstLogin;
                existingUser.UpdatedAt = DateTime.UtcNow;
                existingUser.UpdatedBy = appUser.UpdatedBy;
                existingUser.AccessFailedCount = appUser.AccessFailedCount;
                existingUser.LockoutEnd = appUser.LockoutEnd;
                existingUser.IsDeleted = appUser.IsDeleted;
                
                result = await _userManager.UpdateAsync(existingUser);
            }

            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                _logger.LogError("User save failed for {Email}: {Errors}", user.Email.Value, errors);
                return FSharpResult<User, string>.NewError($"ユーザー保存エラー: {errors}");
            }

            // ロール同期処理（Phase B-F3: ロール変更対応）
            var targetUser = existingUser ?? appUser;
            var currentRoles = await _userManager.GetRolesAsync(targetUser);
            var newRoleName = ConvertRoleToString(user.Role);

            if (!currentRoles.Contains(newRoleName))
            {
                _logger.LogInformation("Role change detected for {Email}: {OldRoles} -> {NewRole}",
                    user.Email.Value, string.Join(", ", currentRoles), newRoleName);

                // 既存ロール削除
                if (currentRoles.Any())
                {
                    var removeResult = await _userManager.RemoveFromRolesAsync(targetUser, currentRoles);
                    if (!removeResult.Succeeded)
                    {
                        var errors = string.Join(", ", removeResult.Errors.Select(e => e.Description));
                        _logger.LogWarning("Failed to remove old roles for {Email}: {Errors}", user.Email.Value, errors);
                    }
                }

                // 新ロール追加
                var addResult = await _userManager.AddToRoleAsync(targetUser, newRoleName);
                if (!addResult.Succeeded)
                {
                    var errors = string.Join(", ", addResult.Errors.Select(e => e.Description));
                    _logger.LogWarning("Failed to add new role {Role} for {Email}: {Errors}",
                        newRoleName, user.Email.Value, errors);
                }
                else
                {
                    _logger.LogInformation("Role updated successfully for {Email}: {NewRole}",
                        user.Email.Value, newRoleName);
                }
            }

            _logger.LogInformation("User successfully saved: {Email}", user.Email.Value);
            return FSharpResult<User, string>.NewOk(user);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception in SaveAsync for user: {Email}", user.Email.Value);
            return FSharpResult<User, string>.NewError($"保存処理エラー: {ex.Message}");
        }
    }

    /// <summary>
    /// プロジェクト別ユーザー一覧取得
    /// 【注意】現在のバージョンでは、プロジェクト・ユーザー関連付けが未実装のため、
    /// 空のリストを返します。将来的なプロジェクト管理機能実装時に対応予定です。
    /// </summary>
    public async Task<FSharpResult<Microsoft.FSharp.Collections.FSharpList<User>, string>> GetByProjectIdAsync(ProjectId projectId)
    {
        try
        {
            _logger.LogDebug("Getting users by project ID: {ProjectId} (not implemented)", projectId.Item);

            // プロジェクト機能未実装のため、空リストを返す
            await Task.Delay(1); // async警告解消
            var emptyList = Microsoft.FSharp.Collections.FSharpList<User>.Empty;

            return FSharpResult<Microsoft.FSharp.Collections.FSharpList<User>, string>.NewOk(emptyList);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception in GetByProjectIdAsync for project: {ProjectId}", projectId.Item);
            return FSharpResult<Microsoft.FSharp.Collections.FSharpList<User>, string>.NewError(
                $"プロジェクト別ユーザー検索エラー: {ex.Message}");
        }
    }

    /// <summary>
    /// Phase B-F3 Step1.5: 複数プロジェクトに所属するユーザー取得
    /// ProjectManager権限フィルタ用のメソッド
    /// 【F#初学者向け解説】
    /// F#のProjectId listをC#で受け取り、UserProjectsテーブルを使用して
    /// 指定プロジェクトに所属するユーザー一覧を取得します。
    /// </summary>
    /// <param name="projectIds">F#のProjectIdリスト</param>
    /// <returns>F#のResult型でラップされたユーザーリスト</returns>
    public async Task<FSharpResult<Microsoft.FSharp.Collections.FSharpList<User>, string>> GetUsersByProjectIdsAsync(Microsoft.FSharp.Collections.FSharpList<ProjectId> projectIds)
    {
        try
        {
            _logger.LogDebug("Getting users by project IDs");

            // F# ProjectIdリストからC# long値リストに変換
            var projectIdValues = projectIds.Select(p => p.Item).ToList();

            if (!projectIdValues.Any())
            {
                _logger.LogDebug("No project IDs provided, returning empty list");
                return FSharpResult<Microsoft.FSharp.Collections.FSharpList<User>, string>.NewOk(
                    Microsoft.FSharp.Collections.FSharpList<User>.Empty);
            }

            // UserProjectsテーブルから該当ユーザーID一覧を取得
            var userIds = await _context.Set<UserProject>()
                .Where(up => projectIdValues.Contains(up.ProjectId))
                .Select(up => up.UserId)
                .Distinct()
                .ToListAsync();

            _logger.LogDebug("Found {Count} users in projects", userIds.Count);

            // ユーザーID一覧からApplicationUserを取得してF# Userに変換
            var users = new List<User>();
            var errors = new List<string>();

            foreach (var userId in userIds)
            {
                var appUser = await _userManager.FindByIdAsync(userId);
                if (appUser != null && !appUser.IsDeleted)
                {
                    var userResult = await ConvertToFSharpUserAsync(appUser);
                    if (userResult.IsOk)
                    {
                        users.Add(userResult.ResultValue);
                    }
                    else
                    {
                        errors.Add($"User {userId}: {userResult.ErrorValue}");
                    }
                }
            }

            if (errors.Any())
            {
                _logger.LogWarning("Some users could not be converted: {Errors}", string.Join(", ", errors));
            }

            var fsharpList = Microsoft.FSharp.Collections.ListModule.OfSeq(users);
            _logger.LogInformation("Retrieved {Count} users for {ProjectCount} projects",
                users.Count, projectIdValues.Count);

            return FSharpResult<Microsoft.FSharp.Collections.FSharpList<User>, string>.NewOk(fsharpList);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception in GetUsersByProjectIdsAsync");
            return FSharpResult<Microsoft.FSharp.Collections.FSharpList<User>, string>.NewError(
                $"プロジェクト別ユーザー検索エラー: {ex.Message}");
        }
    }

    /// <summary>
    /// Phase B-F3 Step1.5: ユーザーが所属するプロジェクトID一覧取得
    /// ProjectManager権限フィルタ用のメソッド
    /// 【F#初学者向け解説】
    /// F#のUserIdからC#のApplicationUser.Idをマッピングし、
    /// UserProjectsテーブルから該当ユーザーのプロジェクトID一覧を取得します。
    /// </summary>
    /// <param name="userId">F#のUserId</param>
    /// <returns>F#のResult型でラップされたProjectIdリスト</returns>
    public async Task<FSharpResult<Microsoft.FSharp.Collections.FSharpList<ProjectId>, string>> GetProjectIdsByUserIdAsync(UserId userId)
    {
        try
        {
            _logger.LogDebug("Getting project IDs by user ID: {UserId}", userId.Item);

            // F# UserId → ASP.NET Core Identity ID文字列に変換
            var identityId = ConvertUserIdToGuid(userId);

            // UserProjectsテーブルから該当プロジェクトID一覧を取得
            var projectIds = await _context.Set<UserProject>()
                .Where(up => up.UserId == identityId)
                .Select(up => up.ProjectId)
                .Distinct()
                .ToListAsync();

            _logger.LogDebug("Found {Count} projects for user {UserId}", projectIds.Count, userId.Item);

            // C# long値をF# ProjectIdに変換
            var fsharpProjectIds = projectIds
                .Select(id => ProjectId.NewProjectId(id))
                .ToList();

            var fsharpList = Microsoft.FSharp.Collections.ListModule.OfSeq(fsharpProjectIds);
            _logger.LogInformation("Retrieved {Count} projects for user {UserId}", fsharpProjectIds.Count, userId.Item);

            return FSharpResult<Microsoft.FSharp.Collections.FSharpList<ProjectId>, string>.NewOk(fsharpList);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception in GetProjectIdsByUserIdAsync for user: {UserId}", userId.Item);
            return FSharpResult<Microsoft.FSharp.Collections.FSharpList<ProjectId>, string>.NewError(
                $"プロジェクトID取得エラー: {ex.Message}");
        }
    }

    /// <summary>
    /// Phase B-F3 Step1.5: Emailベースでプロジェクト一覧取得
    /// GetProjectIdsByUserIdAsyncの合成GUID問題を回避するため、Emailベースで検索します。
    /// 内部的にEmailでApplicationUserを検索し、そのIdentity IDでUserProjectsを検索します。
    /// </summary>
    /// <param name="email">F#のEmail型</param>
    /// <returns>F#のResult型でラップされたProjectIdリスト</returns>
    public async Task<FSharpResult<Microsoft.FSharp.Collections.FSharpList<ProjectId>, string>> GetProjectIdsByEmailAsync(Email email)
    {
        try
        {
            _logger.LogDebug("Getting project IDs by email: {Email}", email.Value);

            // EmailでApplicationUserを検索
            var appUser = await _userManager.FindByEmailAsync(email.Value);
            if (appUser == null)
            {
                _logger.LogWarning("User not found for email: {Email}", email.Value);
                // ユーザーが存在しない場合は空リストを返す
                return FSharpResult<Microsoft.FSharp.Collections.FSharpList<ProjectId>, string>.NewOk(
                    Microsoft.FSharp.Collections.ListModule.Empty<ProjectId>());
            }

            // ApplicationUser.IdでUserProjectsテーブルを検索
            var projectIds = await _context.Set<UserProject>()
                .Where(up => up.UserId == appUser.Id)
                .Select(up => up.ProjectId)
                .Distinct()
                .ToListAsync();

            _logger.LogDebug("Found {Count} projects for email {Email}", projectIds.Count, email.Value);

            // C# long値をF# ProjectIdに変換
            var fsharpProjectIds = projectIds
                .Select(id => ProjectId.NewProjectId(id))
                .ToList();

            var fsharpList = Microsoft.FSharp.Collections.ListModule.OfSeq(fsharpProjectIds);
            _logger.LogInformation("Retrieved {Count} projects for email {Email}", fsharpProjectIds.Count, email.Value);

            return FSharpResult<Microsoft.FSharp.Collections.FSharpList<ProjectId>, string>.NewOk(fsharpList);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception in GetProjectIdsByEmailAsync for email: {Email}", email.Value);
            return FSharpResult<Microsoft.FSharp.Collections.FSharpList<ProjectId>, string>.NewError(
                $"プロジェクトID取得エラー: {ex.Message}");
        }
    }

    /// <summary>
    /// Phase B-F3 Step1.5: Identity IDベースでプロジェクト一覧取得
    /// GetProjectIdsByUserIdAsyncの合成GUID問題を回避するため、
    /// Identity IDを直接使用してUserProjectsテーブルを検索します。
    /// PM権限フィルタで使用します。
    /// </summary>
    /// <param name="identityId">ASP.NET Core Identity ID（GUID文字列）</param>
    /// <returns>F#のResult型でラップされたProjectIdリスト</returns>
    public async Task<FSharpResult<Microsoft.FSharp.Collections.FSharpList<ProjectId>, string>> GetProjectIdsByIdentityIdAsync(
        string identityId)
    {
        try
        {
            _logger.LogDebug("Getting project IDs by identity ID: {IdentityId}", identityId);

            // Identity IDを直接使用してUserProjectsテーブルを検索
            var projectIds = await _context.Set<UserProject>()
                .Where(up => up.UserId == identityId)
                .Select(up => up.ProjectId)
                .Distinct()
                .ToListAsync();

            _logger.LogDebug("Found {Count} projects for identity: {IdentityId}", projectIds.Count, identityId);

            // C# long値をF# ProjectIdに変換
            var fsharpProjectIds = projectIds
                .Select(id => ProjectId.NewProjectId(id))
                .ToList();

            var fsharpList = Microsoft.FSharp.Collections.ListModule.OfSeq(fsharpProjectIds);
            _logger.LogInformation("Retrieved {Count} projects for identity {IdentityId}", fsharpProjectIds.Count, identityId);

            return FSharpResult<Microsoft.FSharp.Collections.FSharpList<ProjectId>, string>.NewOk(fsharpList);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception in GetProjectIdsByIdentityIdAsync for identity: {IdentityId}", identityId);
            return FSharpResult<Microsoft.FSharp.Collections.FSharpList<ProjectId>, string>.NewError(
                $"プロジェクトID取得エラー: {ex.Message}");
        }
    }

    /// <summary>
    /// ユーザー削除（論理削除）
    /// ASP.NET Core Identity統合版では、ApplicationUserのIsDeletedフラグを使用
    /// 【セキュリティ考慮】
    /// 物理削除ではなく論理削除を実装することで、監査証跡を保持します。
    /// 将来的な復旧要求や、関連データの整合性維持が可能です。
    /// </summary>
    public async Task<FSharpResult<Microsoft.FSharp.Core.Unit, string>> DeleteAsync(UserId userId)
    {
        if (userId == null)
        {
            _logger.LogWarning("DeleteAsync called with null userId");
            return FSharpResult<Microsoft.FSharp.Core.Unit, string>.NewError("ユーザーIDがnullです");
        }

        try
        {
            _logger.LogDebug("Deleting user: {UserId}", userId.Item);

            var guidId = ConvertUserIdToGuid(userId);
            var appUser = await _userManager.FindByIdAsync(guidId);

            if (appUser == null)
            {
                _logger.LogWarning("User not found for deletion: {UserId}", userId.Item);
                return FSharpResult<Microsoft.FSharp.Core.Unit, string>.NewError("削除対象のユーザーが見つかりません");
            }

            // 論理削除実行
            appUser.IsDeleted = true;
            appUser.UpdatedAt = DateTime.UtcNow;
            appUser.UpdatedBy = guidId; // 削除者として自分自身を記録

            var result = await _userManager.UpdateAsync(appUser);

            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                _logger.LogError("User deletion failed for {UserId}: {Errors}", userId.Item, errors);
                return FSharpResult<Microsoft.FSharp.Core.Unit, string>.NewError($"ユーザー削除エラー: {errors}");
            }

            _logger.LogInformation("User successfully deleted (logical): {UserId}", userId.Item);
            // F# Unit型の正しい返却パターン
            return FSharpResult<Microsoft.FSharp.Core.Unit, string>.NewOk(null!);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception in DeleteAsync for user: {UserId}", userId.Item);
            return FSharpResult<Microsoft.FSharp.Core.Unit, string>.NewError($"削除処理エラー: {ex.Message}");
        }
    }

    /// <summary>
    /// Phase B-F3 Step1.5: ユーザーへプロジェクト割り当て
    /// 指定したプロジェクトIDリストをユーザーに割り当てます（新規追加のみ）
    /// 【F#初学者向け解説】
    /// F#のint64 listをC#で受け取り、UserProjectsテーブルに新規レコードを追加します。
    /// 既存の割り当ては保持したまま、新しい割り当てのみを追加する仕様です。
    /// 【Entity Framework Core初学者向け解説】
    /// AddRangeメソッドで複数のエンティティを一括登録できます。
    /// SaveChangesAsyncを呼び出すまでデータベースには反映されません。
    /// </summary>
    /// <param name="userId">F#のUserId型（割り当て対象ユーザー）</param>
    /// <param name="projectIds">F#のint64 list型（割り当てるプロジェクトIDリスト）</param>
    /// <returns>F#のResult&lt;unit, string&gt;型（成功/エラー）</returns>
    public async Task<FSharpResult<Microsoft.FSharp.Core.Unit, string>> AssignProjectsToUserAsync(
        UserId userId,
        Microsoft.FSharp.Collections.FSharpList<long> projectIds)
    {
        if (userId == null)
        {
            _logger.LogWarning("AssignProjectsToUserAsync called with null userId");
            return FSharpResult<Microsoft.FSharp.Core.Unit, string>.NewError("ユーザーIDがnullです");
        }

        if (projectIds == null)
        {
            _logger.LogWarning("AssignProjectsToUserAsync called with null projectIds");
            return FSharpResult<Microsoft.FSharp.Core.Unit, string>.NewError("プロジェクトIDリストがnullです");
        }

        try
        {
            _logger.LogDebug("Assigning projects to user: {UserId}", userId.Item);

            // F# UserId → ASP.NET Core Identity ID文字列に変換
            var identityId = ConvertUserIdToGuid(userId);

            // ユーザーが存在するか確認
            var appUser = await _userManager.FindByIdAsync(identityId);
            if (appUser == null)
            {
                _logger.LogWarning("User not found for project assignment: {UserId}", userId.Item);
                return FSharpResult<Microsoft.FSharp.Core.Unit, string>.NewError("ユーザーが見つかりません");
            }

            // F# FSharpList<long>をC# List<long>に変換
            var projectIdValues = Microsoft.FSharp.Collections.ListModule.ToSeq(projectIds).ToList();

            if (!projectIdValues.Any())
            {
                _logger.LogDebug("No projects to assign for user {UserId}", userId.Item);
                return FSharpResult<Microsoft.FSharp.Core.Unit, string>.NewOk(null!);
            }

            // UserProjectsテーブルに新規レコードを追加
            var userProjects = projectIdValues.Select(projectId => new UserProject
            {
                UserId = identityId,
                ProjectId = projectId,
                UpdatedBy = identityId,
                UpdatedAt = DateTime.UtcNow
            }).ToList();

            await _context.Set<UserProject>().AddRangeAsync(userProjects);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Successfully assigned {Count} projects to user {UserId}",
                projectIdValues.Count, userId.Item);

            return FSharpResult<Microsoft.FSharp.Core.Unit, string>.NewOk(null!);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception in AssignProjectsToUserAsync for user: {UserId}", userId.Item);
            return FSharpResult<Microsoft.FSharp.Core.Unit, string>.NewError(
                $"プロジェクト割り当てエラー: {ex.Message}");
        }
    }

    /// <summary>
    /// Phase B-F3 Step1.5: Identity IDベースのプロジェクト割り当て（ユーザー作成時用）
    /// ASP.NET Core IdentityのID（GUID文字列）を直接使用してUserProjectsテーブルに挿入します。
    /// 【F#初学者向け解説】
    /// ユーザー作成直後は、F# UserId（GetHashCode由来）からGUIDへの逆変換ができないため、
    /// Identity IDを直接受け取って使用します。
    /// 【Entity Framework Core初学者向け解説】
    /// UserProjects.UserIdはAspNetUsers.Idとの外部キー制約があるため、
    /// 正確なIdentity IDを使用する必要があります。
    /// </summary>
    /// <param name="identityId">AspNetUsers.Id（Identity ID、GUID文字列）</param>
    /// <param name="projectIds">F#のint64 list型（割り当てるプロジェクトIDリスト）</param>
    /// <returns>F#のResult&lt;unit, string&gt;型（成功/エラー）</returns>
    public async Task<FSharpResult<Microsoft.FSharp.Core.Unit, string>> AssignProjectsToUserByIdentityIdAsync(
        string identityId,
        Microsoft.FSharp.Collections.FSharpList<long> projectIds)
    {
        if (string.IsNullOrWhiteSpace(identityId))
        {
            _logger.LogWarning("AssignProjectsToUserByIdentityIdAsync called with null or empty identityId");
            return FSharpResult<Microsoft.FSharp.Core.Unit, string>.NewError("Identity IDが空です");
        }

        if (projectIds == null)
        {
            _logger.LogWarning("AssignProjectsToUserByIdentityIdAsync called with null projectIds");
            return FSharpResult<Microsoft.FSharp.Core.Unit, string>.NewError("プロジェクトIDリストがnullです");
        }

        try
        {
            _logger.LogDebug("Assigning projects to user by Identity ID: {IdentityId}", identityId);

            // F# FSharpList<long>をC# List<long>に変換
            var projectIdValues = Microsoft.FSharp.Collections.ListModule.ToSeq(projectIds).ToList();

            if (!projectIdValues.Any())
            {
                _logger.LogDebug("No projects to assign for Identity ID {IdentityId}", identityId);
                return FSharpResult<Microsoft.FSharp.Core.Unit, string>.NewOk(null!);
            }

            // UserProjectsテーブルに新規レコードを追加
            // Identity IDを直接使用（ConvertUserIdToGuidを使用しない）
            var userProjects = projectIdValues.Select(projectId => new UserProject
            {
                UserId = identityId,
                ProjectId = projectId,
                UpdatedBy = identityId,
                UpdatedAt = DateTime.UtcNow
            }).ToList();

            await _context.Set<UserProject>().AddRangeAsync(userProjects);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Successfully assigned {Count} projects to user by Identity ID {IdentityId}",
                projectIdValues.Count, identityId);

            return FSharpResult<Microsoft.FSharp.Core.Unit, string>.NewOk(null!);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception in AssignProjectsToUserByIdentityIdAsync for Identity ID: {IdentityId}", identityId);
            return FSharpResult<Microsoft.FSharp.Core.Unit, string>.NewError(
                $"プロジェクト割り当てエラー: {ex.Message}");
        }
    }

    /// <summary>
    /// Phase B-F3 Step1.5: ユーザーのプロジェクト割り当て更新
    /// 既存のUserProjectsレコードを削除し、新しいプロジェクトIDリストで置き換えます
    /// 【F#初学者向け解説】
    /// F#のint64 listをC#で受け取り、UserProjectsテーブルを更新します。
    /// "更新"とは、既存の割り当てを全て削除してから新しい割り当てを追加する処理です。
    /// 【Entity Framework Core初学者向け解説】
    /// RemoveRangeで既存レコードを削除、AddRangeで新規レコードを追加します。
    /// SaveChangesAsyncで両方の変更を一括でデータベースに反映します（トランザクション保証）。
    /// </summary>
    /// <param name="userId">F#のUserId型（更新対象ユーザー）</param>
    /// <param name="projectIds">F#のint64 list型（新しいプロジェクトIDリスト）</param>
    /// <returns>F#のResult&lt;unit, string&gt;型（成功/エラー）</returns>
    public async Task<FSharpResult<Microsoft.FSharp.Core.Unit, string>> UpdateUserProjectsAsync(
        UserId userId,
        Microsoft.FSharp.Collections.FSharpList<long> projectIds)
    {
        if (userId == null)
        {
            _logger.LogWarning("UpdateUserProjectsAsync called with null userId");
            return FSharpResult<Microsoft.FSharp.Core.Unit, string>.NewError("ユーザーIDがnullです");
        }

        if (projectIds == null)
        {
            _logger.LogWarning("UpdateUserProjectsAsync called with null projectIds");
            return FSharpResult<Microsoft.FSharp.Core.Unit, string>.NewError("プロジェクトIDリストがnullです");
        }

        try
        {
            _logger.LogDebug("Updating user projects for user: {UserId}", userId.Item);

            // F# UserId → ASP.NET Core Identity ID文字列に変換
            var identityId = ConvertUserIdToGuid(userId);

            // ユーザーが存在するか確認
            var appUser = await _userManager.FindByIdAsync(identityId);
            if (appUser == null)
            {
                _logger.LogWarning("User not found for project update: {UserId}", userId.Item);
                return FSharpResult<Microsoft.FSharp.Core.Unit, string>.NewError("ユーザーが見つかりません");
            }

            // F# FSharpList<long>をC# List<long>に変換
            var projectIdValues = Microsoft.FSharp.Collections.ListModule.ToSeq(projectIds).ToList();

            // 既存のUserProjectsレコードを取得
            var existingUserProjects = await _context.Set<UserProject>()
                .Where(up => up.UserId == identityId)
                .ToListAsync();

            _logger.LogDebug("Found {Count} existing projects for user {UserId}",
                existingUserProjects.Count, userId.Item);

            // 既存レコードを削除
            _context.Set<UserProject>().RemoveRange(existingUserProjects);

            // 新しいUserProjectsレコードを追加
            if (projectIdValues.Any())
            {
                var newUserProjects = projectIdValues.Select(projectId => new UserProject
                {
                    UserId = identityId,
                    ProjectId = projectId,
                    UpdatedBy = identityId,
                    UpdatedAt = DateTime.UtcNow
                }).ToList();

                await _context.Set<UserProject>().AddRangeAsync(newUserProjects);
            }

            // データベースに変更を保存
            await _context.SaveChangesAsync();

            _logger.LogInformation("Successfully updated user projects: removed {RemovedCount}, added {AddedCount} for user {UserId}",
                existingUserProjects.Count, projectIdValues.Count, userId.Item);

            return FSharpResult<Microsoft.FSharp.Core.Unit, string>.NewOk(null!);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception in UpdateUserProjectsAsync for user: {UserId}", userId.Item);
            return FSharpResult<Microsoft.FSharp.Core.Unit, string>.NewError(
                $"プロジェクト更新エラー: {ex.Message}");
        }
    }

    /// <summary>
    /// Phase B-F3 Step1.5: Identity IDベースのプロジェクト割り当て更新
    /// 既存のUserProjectsレコードを削除し、新しいプロジェクトIDリストで置き換えます
    /// 【F#初学者向け解説】
    /// AssignProjectsToUserByIdentityIdAsyncと同様の背景で追加されたメソッドです。
    /// ASP.NET Core IdentityのID（GUID文字列）を直接使用してプロジェクト割り当てを更新します。
    /// ConvertUserIdToGuid()による合成GUID問題を回避するため、Identity IDを直接使用します。
    /// 【Entity Framework Core初学者向け解説】
    /// RemoveRangeで既存レコードを削除、AddRangeで新規レコードを追加します。
    /// SaveChangesAsyncで両方の変更を一括でデータベースに反映します（トランザクション保証）。
    /// </summary>
    /// <param name="identityId">AspNetUsers.Id（Identity ID、GUID文字列）</param>
    /// <param name="projectIds">F#のint64 list型（新しいプロジェクトIDリスト）</param>
    /// <returns>F#のResult&lt;unit, string&gt;型（成功/エラー）</returns>
    public async Task<FSharpResult<Microsoft.FSharp.Core.Unit, string>> UpdateUserProjectsByIdentityIdAsync(
        string identityId,
        Microsoft.FSharp.Collections.FSharpList<long> projectIds)
    {
        if (string.IsNullOrWhiteSpace(identityId))
        {
            _logger.LogWarning("UpdateUserProjectsByIdentityIdAsync called with null or empty identityId");
            return FSharpResult<Microsoft.FSharp.Core.Unit, string>.NewError("Identity IDが空です");
        }

        if (projectIds == null)
        {
            _logger.LogWarning("UpdateUserProjectsByIdentityIdAsync called with null projectIds");
            return FSharpResult<Microsoft.FSharp.Core.Unit, string>.NewError("プロジェクトIDリストがnullです");
        }

        try
        {
            _logger.LogDebug("Updating user projects by Identity ID: {IdentityId}", identityId);

            // ユーザーが存在するか確認（Identity IDを直接使用）
            var appUser = await _userManager.FindByIdAsync(identityId);
            if (appUser == null)
            {
                _logger.LogWarning("User not found for project update by Identity ID: {IdentityId}", identityId);
                return FSharpResult<Microsoft.FSharp.Core.Unit, string>.NewError("ユーザーが見つかりません");
            }

            // F# FSharpList<long>をC# List<long>に変換
            var projectIdValues = Microsoft.FSharp.Collections.ListModule.ToSeq(projectIds).ToList();

            // 既存のUserProjectsレコードを取得（Identity IDを直接使用）
            var existingUserProjects = await _context.Set<UserProject>()
                .Where(up => up.UserId == identityId)
                .ToListAsync();

            _logger.LogDebug("Found {Count} existing projects for Identity ID {IdentityId}",
                existingUserProjects.Count, identityId);

            // 既存レコードを削除
            _context.Set<UserProject>().RemoveRange(existingUserProjects);

            // 新しいUserProjectsレコードを追加
            if (projectIdValues.Any())
            {
                var newUserProjects = projectIdValues.Select(projectId => new UserProject
                {
                    UserId = identityId,
                    ProjectId = projectId,
                    UpdatedBy = identityId,
                    UpdatedAt = DateTime.UtcNow
                }).ToList();

                await _context.Set<UserProject>().AddRangeAsync(newUserProjects);
            }

            // データベースに変更を保存
            await _context.SaveChangesAsync();

            _logger.LogInformation("Successfully updated user projects by Identity ID: removed {RemovedCount}, added {AddedCount} for Identity ID {IdentityId}",
                existingUserProjects.Count, projectIdValues.Count, identityId);

            return FSharpResult<Microsoft.FSharp.Core.Unit, string>.NewOk(null!);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception in UpdateUserProjectsByIdentityIdAsync for Identity ID: {IdentityId}", identityId);
            return FSharpResult<Microsoft.FSharp.Core.Unit, string>.NewError(
                $"プロジェクト更新エラー: {ex.Message}");
        }
    }

    // =================================================================
    // 🔍 F# IUserRepository拡張メソッド実装（Phase A9対応）
    // =================================================================

    /// <summary>
    /// アクティブユーザー一覧取得
    /// ASP.NET Core Identity統合版では、IsDeleted = false のユーザーを取得
    /// </summary>
    public async Task<FSharpResult<Microsoft.FSharp.Collections.FSharpList<User>, string>> GetAllActiveUsersAsync()
    {
        try
        {
            _logger.LogDebug("Getting all active users");
            
            var activeUsers = await _userManager.Users
                .Where(u => !u.IsDeleted)
                .OrderBy(u => u.Name)
                .ToListAsync();
            
            var userList = new List<User>();
            var errors = new List<string>();
            
            foreach (var appUser in activeUsers)
            {
                var userResult = await ConvertToFSharpUserAsync(appUser);
                if (userResult.IsOk)
                {
                    userList.Add(userResult.ResultValue);
                }
                else
                {
                    errors.Add($"User {appUser.Email}: {userResult.ErrorValue}");
                }
            }
            
            if (errors.Any())
            {
                _logger.LogWarning("Some users could not be converted: {Errors}", string.Join(", ", errors));
            }
            
            var fsharpList = Microsoft.FSharp.Collections.ListModule.OfSeq(userList);
            _logger.LogInformation("Retrieved {Count} active users", userList.Count);
            
            return FSharpResult<Microsoft.FSharp.Collections.FSharpList<User>, string>.NewOk(fsharpList);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving active users");
            return FSharpResult<Microsoft.FSharp.Collections.FSharpList<User>, string>.NewError(
                $"アクティブユーザー取得エラー: {ex.Message}");
        }
    }

    /// <summary>
    /// 全ユーザー一覧取得
    /// </summary>
    /// <param name="includeDeleted">trueの場合、論理削除ユーザーも含める</param>
    /// <returns>ユーザー一覧またはエラー</returns>
    public async Task<FSharpResult<Microsoft.FSharp.Collections.FSharpList<User>, string>> GetAllUsersAsync(bool includeDeleted)
    {
        try
        {
            // 【C#初学者向け解説】
            // includeDeletedパラメータに応じて、論理削除ユーザーを含めるかどうかを制御します。
            // 【重要】DbContextにグローバルクエリフィルター（HasQueryFilter(e => !e.IsDeleted)）が
            // UbiquitousLanguageDbContext.csで設定されているため、削除済みユーザーを含める場合は
            // IgnoreQueryFilters()を使用してグローバルフィルターを無視する必要があります。
            IQueryable<ApplicationUser> query;
            if (includeDeleted)
            {
                // グローバルクエリフィルターを無視して削除済みユーザーも含める
                query = _userManager.Users.IgnoreQueryFilters();
            }
            else
            {
                // グローバルクエリフィルターが適用されるため、削除済みユーザーは自動的に除外される
                query = _userManager.Users.AsQueryable();
            }

            var allUsers = await query
                .OrderBy(u => u.Name)
                .ToListAsync();

            var userList = new List<User>();
            var errors = new List<string>();

            foreach (var appUser in allUsers)
            {
                var userResult = await ConvertToFSharpUserAsync(appUser);
                if (userResult.IsOk)
                {
                    userList.Add(userResult.ResultValue);
                }
                else
                {
                    errors.Add($"User {appUser.Email}: {userResult.ErrorValue}");
                }
            }

            if (errors.Any())
            {
                _logger.LogWarning("Some users could not be converted: {Errors}", string.Join(", ", errors));
            }

            var fsharpList = Microsoft.FSharp.Collections.ListModule.OfSeq(userList);
            return FSharpResult<Microsoft.FSharp.Collections.FSharpList<User>, string>.NewOk(fsharpList);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all users");
            return FSharpResult<Microsoft.FSharp.Collections.FSharpList<User>, string>.NewError(
                $"全ユーザー取得エラー: {ex.Message}");
        }
    }

    /// <summary>
    /// ロール別ユーザー取得
    /// ASP.NET Core Identity Rolesからユーザー一覧を取得
    /// </summary>
    public async Task<FSharpResult<Microsoft.FSharp.Collections.FSharpList<User>, string>> GetByRoleAsync(Role role)
    {
        try
        {
            var roleName = ConvertRoleToString(role);
            _logger.LogDebug("Getting users by role: {Role}", roleName);

            // ASP.NET Core Identity Roles検索
            var usersInRole = await _userManager.GetUsersInRoleAsync(roleName);

            // 論理削除フィルタ適用
            var activeUsers = usersInRole.Where(u => !u.IsDeleted).ToList();

            var userList = new List<User>();
            var errors = new List<string>();

            foreach (var appUser in activeUsers)
            {
                var userResult = await ConvertToFSharpUserAsync(appUser);
                if (userResult.IsOk)
                {
                    userList.Add(userResult.ResultValue);
                }
                else
                {
                    errors.Add($"User {appUser.Email}: {userResult.ErrorValue}");
                }
            }

            if (errors.Any())
            {
                _logger.LogWarning("Some users could not be converted for role {Role}: {Errors}",
                    roleName, string.Join(", ", errors));
            }

            var fsharpList = Microsoft.FSharp.Collections.ListModule.OfSeq(userList);
            _logger.LogInformation("Retrieved {Count} users for role: {Role}", userList.Count, roleName);

            return FSharpResult<Microsoft.FSharp.Collections.FSharpList<User>, string>.NewOk(fsharpList);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving users by role: {Role}", role.ToString());
            return FSharpResult<Microsoft.FSharp.Collections.FSharpList<User>, string>.NewError(
                $"ロール別ユーザー取得エラー: {ex.Message}");
        }
    }

    /// <summary>
    /// ユーザー検索（名前・メールアドレス部分一致）
    /// </summary>
    public async Task<FSharpResult<Microsoft.FSharp.Collections.FSharpList<User>, string>> SearchUsersAsync(string searchTerm)
    {
        try
        {
            _logger.LogDebug("Searching users with term: {SearchTerm}", searchTerm);
            
            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                return await GetAllActiveUsersAsync();
            }

            var normalizedSearchTerm = searchTerm.Trim().ToLower();
            
            var searchResults = await _userManager.Users
                .Where(u => !u.IsDeleted)
                .Where(u => u.Name.ToLower().Contains(normalizedSearchTerm) ||
                           (u.Email != null && u.Email.ToLower().Contains(normalizedSearchTerm)))
                .OrderBy(u => u.Name)
                .ToListAsync();
            
            var userList = new List<User>();
            var errors = new List<string>();
            
            foreach (var appUser in searchResults)
            {
                var userResult = await ConvertToFSharpUserAsync(appUser);
                if (userResult.IsOk)
                {
                    userList.Add(userResult.ResultValue);
                }
                else
                {
                    errors.Add($"User {appUser.Email}: {userResult.ErrorValue}");
                }
            }
            
            if (errors.Any())
            {
                _logger.LogWarning("Some users could not be converted in search: {Errors}", string.Join(", ", errors));
            }
            
            var fsharpList = Microsoft.FSharp.Collections.ListModule.OfSeq(userList);
            _logger.LogInformation("Search returned {Count} users for term: {SearchTerm}", userList.Count, searchTerm);
            
            return FSharpResult<Microsoft.FSharp.Collections.FSharpList<User>, string>.NewOk(fsharpList);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching users with term: {SearchTerm}", searchTerm);
            return FSharpResult<Microsoft.FSharp.Collections.FSharpList<User>, string>.NewError(
                $"ユーザー検索エラー: {ex.Message}");
        }
    }

    /// <summary>
    /// ユーザー統計情報取得
    /// </summary>
    public async Task<FSharpResult<object, string>> GetUserStatisticsAsync()
    {
        try
        {
            _logger.LogDebug("Getting user statistics");

            var totalUsers = await _userManager.Users.CountAsync(u => !u.IsDeleted);
            var activeUsers = await _userManager.Users.CountAsync(u => !u.IsDeleted);
            var firstLoginUsers = await _userManager.Users.CountAsync(u => u.IsFirstLogin && !u.IsDeleted);

            var statistics = new
            {
                TotalUsers = totalUsers,
                ActiveUsers = activeUsers,
                InactiveUsers = totalUsers - activeUsers,
                FirstLoginUsers = firstLoginUsers,
                LastUpdated = DateTime.UtcNow
            };

            _logger.LogInformation("User statistics: Total={Total}, Active={Active}, FirstLogin={FirstLogin}",
                totalUsers, activeUsers, firstLoginUsers);

            return FSharpResult<object, string>.NewOk(statistics);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving user statistics");
            return FSharpResult<object, string>.NewError($"統計情報取得エラー: {ex.Message}");
        }
    }

    // =================================================================
    // 🔍 Phase B-F3 Step1.5: Identity ID直接操作メソッド
    // GetHashCode()による不安定な変換を回避し、ASP.NET Identity IDで直接操作
    // =================================================================

    /// <summary>
    /// Phase B-F3 Step1.5: 全ユーザー一覧取得（Identity ID付き）
    /// GetAllUsersAsyncと同様のロジックですが、Tuple&lt;User, string&gt;（User, IdentityId）のリストを返します。
    /// 【F#初学者向け解説】
    /// このメソッドは、F#のUserエンティティとASP.NET Core IdentityのGUID文字列IDをタプルで返すことで、
    /// GetHashCode()による合成GUID変換の不安定性を回避します。
    /// 【用途】ユーザー一覧画面でのIdentityIdベース操作（編集・削除）に使用します。
    /// </summary>
    /// <param name="includeDeleted">trueの場合、論理削除ユーザーも含める</param>
    /// <returns>F#のResult型でラップされた(User, IdentityId)タプルのリスト</returns>
    public async Task<FSharpResult<Microsoft.FSharp.Collections.FSharpList<Tuple<User, string>>, string>> GetAllUsersWithIdentityAsync(bool includeDeleted)
    {
        try
        {
            _logger.LogDebug("Getting all users with Identity IDs (includeDeleted: {IncludeDeleted})", includeDeleted);

            // ユーザー取得クエリ構築
            // 【Entity Framework Core初学者向け解説】
            // AsQueryable()でIQueryable<ApplicationUser>を作成し、
            // includeDeletedパラメータに応じてIgnoreQueryFilters()を適用します。
            IQueryable<ApplicationUser> query;
            if (includeDeleted)
            {
                // グローバルクエリフィルターを無視して削除済みユーザーも含める
                query = _userManager.Users.IgnoreQueryFilters();
            }
            else
            {
                // グローバルクエリフィルターが適用されるため、削除済みユーザーは自動的に除外される
                query = _userManager.Users.AsQueryable();
            }

            var appUsers = await query
                .OrderBy(u => u.Name)
                .ToListAsync();

            _logger.LogDebug("Retrieved {Count} users from database", appUsers.Count);

            // ApplicationUser → (User, IdentityId) タプル変換
            var userTuples = new List<Tuple<User, string>>();
            var errors = new List<string>();

            foreach (var appUser in appUsers)
            {
                // ApplicationUser → F# User変換（既存ヘルパーメソッド再利用）
                var userResult = await ConvertToFSharpUserAsync(appUser);
                if (userResult.IsOk)
                {
                    // タプル作成: (User, IdentityId)
                    // appUser.IdがASP.NET Core IdentityのGUID文字列ID
                    userTuples.Add(Tuple.Create(userResult.ResultValue, appUser.Id));
                }
                else
                {
                    errors.Add($"User {appUser.Email}: {userResult.ErrorValue}");
                }
            }

            if (errors.Any())
            {
                _logger.LogWarning("Some users could not be converted: {Errors}", string.Join(", ", errors));
            }

            // C# List<Tuple> → F# FSharpList変換
            var fsharpList = Microsoft.FSharp.Collections.ListModule.OfSeq(userTuples);
            _logger.LogInformation("Retrieved {Count} users with Identity IDs", userTuples.Count);

            return FSharpResult<Microsoft.FSharp.Collections.FSharpList<Tuple<User, string>>, string>.NewOk(fsharpList);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception in GetAllUsersWithIdentityAsync");
            return FSharpResult<Microsoft.FSharp.Collections.FSharpList<Tuple<User, string>>, string>.NewError(
                $"ユーザー一覧（IdentityId付き）の取得に失敗しました: {ex.Message}");
        }
    }

    /// <summary>
    /// Phase B-F3 Step1.5: IdentityIdでユーザー削除（論理削除）
    /// GetHashCode()による合成GUID変換を使用せず、IdentityId文字列で直接ユーザーを検索・削除します。
    /// 【F#初学者向け解説】
    /// ASP.NET Core IdentityのFindByIdAsyncを使用して、GUID文字列IDから直接ユーザーを取得します。
    /// 論理削除（IsDeleted = true）を実施し、監査証跡を保持します。
    /// 【用途】ユーザー一覧画面での削除操作に使用します。
    /// </summary>
    /// <param name="identityId">ASP.NET Core IdentityのGUID文字列ID</param>
    /// <returns>F#のResult型（成功/エラー）</returns>
    public async Task<FSharpResult<Microsoft.FSharp.Core.Unit, string>> DeleteByIdentityIdAsync(string identityId)
    {
        if (string.IsNullOrWhiteSpace(identityId))
        {
            _logger.LogWarning("DeleteByIdentityIdAsync called with null or empty identityId");
            return FSharpResult<Microsoft.FSharp.Core.Unit, string>.NewError("Identity IDが無効です");
        }

        try
        {
            _logger.LogDebug("Deleting user by Identity ID: {IdentityId}", identityId);

            // ASP.NET Core Identity直接検索実行
            // 【ASP.NET Core Identity初学者向け解説】
            // FindByIdAsyncはUserManager<TUser>のメソッドで、主キー（GUID文字列）でユーザーを検索します。
            var appUser = await _userManager.FindByIdAsync(identityId);
            if (appUser == null)
            {
                _logger.LogWarning("User not found for deletion - Identity ID: {IdentityId}", identityId);
                return FSharpResult<Microsoft.FSharp.Core.Unit, string>.NewError("削除対象のユーザーが見つかりません");
            }

            // 論理削除実行
            // 【セキュリティ考慮初学者向け解説】
            // 物理削除（データベースから完全削除）ではなく、論理削除（IsDeletedフラグ設定）を実施します。
            // これにより、監査証跡の保持・誤削除時の復旧・関連データの整合性維持が可能になります。
            appUser.IsDeleted = true;
            appUser.UpdatedAt = DateTime.UtcNow;
            appUser.UpdatedBy = identityId; // 削除者として自分自身を記録

            var result = await _userManager.UpdateAsync(appUser);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                _logger.LogError("User deletion failed - Identity ID: {IdentityId}, Errors: {Errors}", identityId, errors);
                return FSharpResult<Microsoft.FSharp.Core.Unit, string>.NewError($"ユーザー削除に失敗しました: {errors}");
            }

            _logger.LogInformation("User successfully deleted (logical) - Identity ID: {IdentityId}, Email: {Email}",
                identityId, appUser.Email);

            // F# Unit型の正しい返却パターン
            return FSharpResult<Microsoft.FSharp.Core.Unit, string>.NewOk(null!);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception in DeleteByIdentityIdAsync for Identity ID: {IdentityId}", identityId);
            return FSharpResult<Microsoft.FSharp.Core.Unit, string>.NewError(
                $"ユーザー削除処理でエラーが発生しました: {ex.Message}");
        }
    }

    /// <summary>
    /// Phase B-F3 Step1.5: IdentityIdでユーザー更新
    /// GetHashCode()による合成GUID変換を使用せず、IdentityId文字列で直接ユーザーを検索・更新します。
    /// 【F#初学者向け解説】
    /// ASP.NET Core IdentityのFindByIdAsyncを使用して、GUID文字列IDから直接ユーザーを取得し、
    /// Name・Role・IsActiveの3つのプロパティを更新します。
    /// 【用途】ユーザー編集画面での更新操作に使用します。
    /// </summary>
    /// <param name="identityId">ASP.NET Core IdentityのGUID文字列ID</param>
    /// <param name="name">更新後のユーザー名</param>
    /// <param name="role">更新後のF# Role型</param>
    /// <param name="isActive">更新後のアクティブ状態（true: 有効, false: 無効）</param>
    /// <returns>F#のResult型でラップされた更新後のUser</returns>
    public async Task<FSharpResult<User, string>> UpdateByIdentityIdAsync(
        string identityId,
        string name,
        Role role,
        bool isActive)
    {
        if (string.IsNullOrWhiteSpace(identityId))
        {
            _logger.LogWarning("UpdateByIdentityIdAsync called with null or empty identityId");
            return FSharpResult<User, string>.NewError("Identity IDが無効です");
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            _logger.LogWarning("UpdateByIdentityIdAsync called with null or empty name");
            return FSharpResult<User, string>.NewError("ユーザー名が無効です");
        }

        try
        {
            _logger.LogDebug("Updating user by Identity ID: {IdentityId}, Name: {Name}, Role: {Role}, IsActive: {IsActive}",
                identityId, name, role, isActive);

            // ASP.NET Core Identity直接検索実行
            var appUser = await _userManager.FindByIdAsync(identityId);
            if (appUser == null)
            {
                _logger.LogWarning("User not found for update - Identity ID: {IdentityId}", identityId);
                return FSharpResult<User, string>.NewError("更新対象のユーザーが見つかりません");
            }

            // ユーザー情報更新
            // 【注意】Emailは更新しません（主キー扱い・変更不可）
            appUser.Name = name;
            // ロール情報はAspNetUserRolesテーブルで管理されるため、UserManagerのロール管理メソッドで同期（下記ロール同期処理参照）
            appUser.IsDeleted = !isActive; // IsActive → IsDeleted変換（論理否定）
            appUser.UpdatedAt = DateTime.UtcNow;
            appUser.UpdatedBy = identityId; // 更新者として自分自身を記録

            var result = await _userManager.UpdateAsync(appUser);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                _logger.LogError("User update failed - Identity ID: {IdentityId}, Errors: {Errors}", identityId, errors);
                return FSharpResult<User, string>.NewError($"ユーザー更新に失敗しました: {errors}");
            }

            // ロール同期処理
            // 【ASP.NET Core Identity初学者向け解説】
            // ASP.NET Core Identityでは、ユーザーのロール情報はUserテーブルではなく、
            // AspNetUserRolesテーブル（多対多）で管理されます。
            // そのため、ApplicationUser.Roleプロパティ更新に加えて、UserManagerのロール管理メソッドで同期が必要です。
            var currentRoles = await _userManager.GetRolesAsync(appUser);
            var newRoleName = ConvertRoleToString(role);

            if (!currentRoles.Contains(newRoleName))
            {
                _logger.LogInformation("Role change detected for Identity ID {IdentityId}: {OldRoles} -> {NewRole}",
                    identityId, string.Join(", ", currentRoles), newRoleName);

                // 既存ロール削除
                if (currentRoles.Any())
                {
                    var removeResult = await _userManager.RemoveFromRolesAsync(appUser, currentRoles);
                    if (!removeResult.Succeeded)
                    {
                        var errors = string.Join(", ", removeResult.Errors.Select(e => e.Description));
                        _logger.LogWarning("Failed to remove old roles for Identity ID {IdentityId}: {Errors}",
                            identityId, errors);
                    }
                }

                // 新ロール追加
                var addResult = await _userManager.AddToRoleAsync(appUser, newRoleName);
                if (!addResult.Succeeded)
                {
                    var errors = string.Join(", ", addResult.Errors.Select(e => e.Description));
                    _logger.LogWarning("Failed to add new role {Role} for Identity ID {IdentityId}: {Errors}",
                        newRoleName, identityId, errors);
                }
                else
                {
                    _logger.LogInformation("Role updated successfully for Identity ID {IdentityId}: {NewRole}",
                        identityId, newRoleName);
                }
            }

            _logger.LogInformation("User successfully updated - Identity ID: {IdentityId}, Name: {Name}, Email: {Email}",
                identityId, name, appUser.Email);

            // 更新後のApplicationUser → F# User変換（既存ヘルパーメソッド再利用）
            var updatedUserResult = await ConvertToFSharpUserAsync(appUser);
            if (updatedUserResult.IsError)
            {
                _logger.LogError("User conversion failed after update - Identity ID: {IdentityId}, Error: {Error}",
                    identityId, updatedUserResult.ErrorValue);
                return FSharpResult<User, string>.NewError(
                    $"ユーザー更新後の変換に失敗しました: {updatedUserResult.ErrorValue}");
            }

            return FSharpResult<User, string>.NewOk(updatedUserResult.ResultValue);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception in UpdateByIdentityIdAsync for Identity ID: {IdentityId}", identityId);
            return FSharpResult<User, string>.NewError(
                $"ユーザー更新処理でエラーが発生しました: {ex.Message}");
        }
    }

    // =================================================================
    // 🔧 プライベートヘルパーメソッド
    // =================================================================

    /// <summary>
    /// F# UserId → GUID文字列変換
    /// UserRepositoryのロジックと整合性を保つ変換処理
    /// 【重要】この変換ロジックは、既存のToEntity/ToDomainUserメソッドと同一である必要があります。
    /// 【警告】GetHashCode()ベースの合成GUID変換は衝突リスクあり。
    /// 新規実装ではGetByIdentityIdAsync()の使用を推奨します。
    /// </summary>
    private static string ConvertUserIdToGuid(UserId userId)
    {
        // UserIdがstring型に変更されたため、GUID形式であればそのまま使用
        var userIdString = userId.Value;

        // GUID形式のチェック
        if (Guid.TryParse(userIdString, out var guidValue))
        {
            return guidValue.ToString();
        }

        // GUID形式でない場合はHashCodeから生成（後方互換性のため）
        // 注意: この変換は衝突リスクがあります
        var hashCode = userIdString.GetHashCode();
        return new Guid(hashCode, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0).ToString();
    }

    /// <summary>
    /// F# Role → ASP.NET Core Identity Role名変換
    /// ロール同期で使用するヘルパーメソッド
    /// </summary>
    private static string ConvertRoleToString(Role role)
    {
        if (role.IsSuperUser) return "SuperUser";
        if (role.IsProjectManager) return "ProjectManager";
        if (role.IsDomainApprover) return "DomainApprover";
        return "GeneralUser";
    }

    /// <summary>
    /// Infrastructure層専用 - ApplicationUser → F# User変換（非同期版）
    /// Clean Architecture遵守：Infrastructure層内でのみ使用
    /// 【Issue #7修正】デッドロック問題解決のため非同期化
    /// </summary>
    private async Task<FSharpResult<User, string>> ConvertToFSharpUserAsync(ApplicationUser appUser)
    {
        if (appUser == null)
        {
            return FSharpResult<User, string>.NewError("ApplicationUserがnullです");
        }

        try
        {
            // Email変換
            var emailString = appUser.Email ?? string.Empty;
            var emailResult = Email.create(emailString);
            if (emailResult.IsError)
            {
                return FSharpResult<User, string>.NewError($"無効なメールアドレス: {emailResult.ErrorValue}");
            }

            // UserName変換
            var nameResult = UserName.create(appUser.Name ?? string.Empty);
            if (nameResult.IsError)
            {
                return FSharpResult<User, string>.NewError($"無効なユーザー名: {nameResult.ErrorValue}");
            }

            // Role変換: ASP.NET Core Identity Rolesから取得（非同期await使用）
            var roleResult = await GetUserRoleFromIdentityAsync(appUser);
            if (roleResult.IsError)
            {
                _logger.LogWarning("Role conversion failed for user {Email}: {Error}, defaulting to GeneralUser",
                    appUser.Email, roleResult.ErrorValue);
            }
            var role = roleResult.IsOk ? roleResult.ResultValue : Role.GeneralUser;

            // UserId変換（GUID文字列 → F# UserId）
            var userId = UserId.create(appUser.Id);

            // 【C#初学者向け解説】
            // データベースのIsDeletedフラグをF# User.IsActiveに変換します。
            // IsDeleted=true（削除済み）→ IsActive=false（非アクティブ）
            // IsDeleted=false（有効）→ IsActive=true（アクティブ）
            var isActive = !appUser.IsDeleted;

            // F# Userエンティティ作成（データベースから復元）
            var user = User.createFromDatabase(emailResult.ResultValue, nameResult.ResultValue, role, userId, isActive);

            // 追加プロパティの設定（F# Userはimmutableのため、直接プロパティ設定はできない）
            // F#のUserレコードはcreate時に必要なプロパティを設定する必要がある

            return FSharpResult<User, string>.NewOk(user);
        }
        catch (Exception ex)
        {
            return FSharpResult<User, string>.NewError($"ユーザー変換エラー: {ex.Message}");
        }
    }

    /// <summary>
    /// ASP.NET Core Identity からロールを取得してF# Roleに変換
    /// 優先順位: SuperUser > ProjectManager > DomainApprover > GeneralUser
    /// </summary>
    private async Task<FSharpResult<Role, string>> GetUserRoleFromIdentityAsync(ApplicationUser appUser)
    {
        try
        {
            var roles = await _userManager.GetRolesAsync(appUser);

            if (roles == null || !roles.Any())
            {
                _logger.LogWarning("User {UserId} has no roles assigned", appUser.Id);
                return FSharpResult<Role, string>.NewOk(Role.GeneralUser);
            }

            // 優先順位に従ってロールを決定
            if (roles.Contains("SuperUser"))
                return FSharpResult<Role, string>.NewOk(Role.SuperUser);
            if (roles.Contains("ProjectManager"))
                return FSharpResult<Role, string>.NewOk(Role.ProjectManager);
            if (roles.Contains("DomainApprover"))
                return FSharpResult<Role, string>.NewOk(Role.DomainApprover);
            if (roles.Contains("GeneralUser"))
                return FSharpResult<Role, string>.NewOk(Role.GeneralUser);

            _logger.LogWarning("User {UserId} has unknown roles: {Roles}, defaulting to GeneralUser",
                appUser.Id, string.Join(", ", roles));
            return FSharpResult<Role, string>.NewOk(Role.GeneralUser);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting roles for user {UserId}", appUser.Id);
            return FSharpResult<Role, string>.NewError($"ロール取得エラー: {ex.Message}");
        }
    }

    /// <summary>
    /// Infrastructure層専用 - F# User → ApplicationUser変換
    /// Clean Architecture遵守：Infrastructure層内でのみ使用
    /// </summary>
    private static ApplicationUser ConvertToApplicationUser(User user)
    {
        if (user == null)
        {
            throw new ArgumentNullException(nameof(user), "F# Userエンティティがnullです");
        }

        // F# UserId → GUID文字列変換
        var userIdString = user.Id.Value;

        // GUID形式であればそのまま使用、そうでなければHashCodeから生成
        string guidId;
        if (Guid.TryParse(userIdString, out var guidValue))
        {
            guidId = guidValue.ToString();
        }
        else
        {
            var hashCode = userIdString.GetHashCode();
            guidId = new Guid(hashCode, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0).ToString();
        }

        return new ApplicationUser
        {
            // ASP.NET Core Identity標準カラム
            Id = guidId,
            UserName = user.Email.Value,
            NormalizedUserName = user.Email.Value.ToUpperInvariant(),
            Email = user.Email.Value,
            NormalizedEmail = user.Email.Value.ToUpperInvariant(),
            EmailConfirmed = user.EmailConfirmed,
            LockoutEnabled = true,
            AccessFailedCount = 0,
            LockoutEnd = null, // F# User.LockoutEndをC# DateTimeOffset?に変換は複雑なため一時的にnull

            // プロジェクト固有カスタムカラム
            Name = user.Name.Value,
            IsFirstLogin = user.IsFirstLogin,
            UpdatedAt = DateTime.UtcNow,
            UpdatedBy = guidId,
            IsDeleted = !user.IsActive,
            PasswordHash = null
        };
    }

    // =================================================================
    // 🎯 Application.ProjectManagement.IUserRepository 明示的実装
    // ProjectManagementService (F#) からの依存解決用（Phase B-F3）
    // =================================================================

    /// <summary>
    /// ID によるユーザー取得（ProjectManagement用）
    /// 既存のGetByIdAsyncに委譲
    /// </summary>
    Task<FSharpResult<FSharpOption<User>, string>> PmIUserRepository.GetByIdAsync(UserId userId)
        => GetByIdAsync(userId);
}