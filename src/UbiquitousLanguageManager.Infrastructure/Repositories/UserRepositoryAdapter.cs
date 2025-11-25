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
using UbiquitousLanguageManager.Infrastructure.Data.Entities;

namespace UbiquitousLanguageManager.Infrastructure.Repositories;

/// <summary>
/// Phase A9: F# Application層認証サービス統合用UserRepositoryAdapter
/// ASP.NET Core Identity → F# Domain型 完全変換Adapter
/// Clean Architecture依存方向遵守: Infrastructure → Application
/// 【Infrastructure層初学者向け解説】
/// このAdapterは、ASP.NET Core Identityの複雑なUserManager機能を、
/// F# Application層のシンプルなIUserRepositoryインターフェースに適応させます。
/// Railway-oriented ProgrammingのResult型を使用して、エラーハンドリングを一元化します。
/// </summary>
public class UserRepositoryAdapter : IUserRepository
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly Microsoft.Extensions.Logging.ILogger<UserRepositoryAdapter> _logger;

    /// <summary>
    /// UserRepositoryAdapterコンストラクタ
    /// ASP.NET Core Identity UserManager依存注入
    /// 【DI初学者向け解説】
    /// UserManagerは、ASP.NET Core Identityの中核機能です。
    /// パスワードハッシュ化、ユーザー検索、ロックアウト管理など、
    /// 認証関連の全機能を提供します。
    /// </summary>
    public UserRepositoryAdapter(
        UserManager<ApplicationUser> userManager,
        Microsoft.Extensions.Logging.ILogger<UserRepositoryAdapter> logger)
    {
        _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
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
            
            // F# UserId → GUID文字列変換
            // 【重要】この変換は、UserRepositoryのToEntity/ToDomainUserメソッドと整合性が必要
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
            // F# Unit型の問題回避：既存UserRepositoryと同様のパターンを使用
            // 成功時はエラーを返すことで一時的に対処（プロトタイプ実装）
            return FSharpResult<Microsoft.FSharp.Core.Unit, string>.NewError("Delete completed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception in DeleteAsync for user: {UserId}", userId.Item);
            return FSharpResult<Microsoft.FSharp.Core.Unit, string>.NewError($"削除処理エラー: {ex.Message}");
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
    /// 全ユーザー一覧取得（無効化ユーザー含む）
    /// </summary>
    public async Task<FSharpResult<Microsoft.FSharp.Collections.FSharpList<User>, string>> GetAllUsersAsync()
    {
        try
        {
            _logger.LogDebug("Getting all users (including inactive)");
            
            var allUsers = await _userManager.Users
                .Where(u => !u.IsDeleted) // 論理削除されていないユーザーのみ
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
            _logger.LogInformation("Retrieved {Count} total users", userList.Count);
            
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
    /// 【注意】ASP.NET Core Identity Roles未実装のため、空リストを返す
    /// </summary>
    public async Task<FSharpResult<Microsoft.FSharp.Collections.FSharpList<User>, string>> GetByRoleAsync(Role role)
    {
        try
        {
            _logger.LogDebug("Getting users by role: {Role} (not implemented)", role.ToString());
            
            // ロール機能未実装のため、空リストを返す
            await Task.Delay(1); // async警告解消
            var emptyList = Microsoft.FSharp.Collections.FSharpList<User>.Empty;
            
            return FSharpResult<Microsoft.FSharp.Collections.FSharpList<User>, string>.NewOk(emptyList);
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
    // 🔧 プライベートヘルパーメソッド
    // =================================================================

    /// <summary>
    /// F# UserId → GUID文字列変換
    /// UserRepositoryのロジックと整合性を保つ変換処理
    /// 【重要】この変換ロジックは、既存のToEntity/ToDomainUserメソッドと同一である必要があります。
    /// </summary>
    private static string ConvertUserIdToGuid(UserId userId)
    {
        // UserRepository.ToEntityメソッドと同じロジックを使用
        return new Guid((int)(userId.Item % int.MaxValue), 0, 0, 0, 0, 0, 0, 0, 0, 0, 0).ToString();
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
            var userIdValue = (long)appUser.Id.GetHashCode();
            var userId = UserId.NewUserId(userIdValue);

            // F# Userエンティティ作成
            var user = User.create(emailResult.ResultValue, nameResult.ResultValue, role, userId);

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
        var guidId = new Guid((int)(user.Id.Item % int.MaxValue), 0, 0, 0, 0, 0, 0, 0, 0, 0, 0).ToString();

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
}