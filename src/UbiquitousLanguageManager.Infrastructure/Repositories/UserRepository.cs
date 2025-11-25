using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.FSharp.Core;
using Microsoft.FSharp.Collections;
using UbiquitousLanguageManager.Application;
using UbiquitousLanguageManager.Domain.Common;
using UbiquitousLanguageManager.Domain.Authentication;
using UbiquitousLanguageManager.Infrastructure.Data;
using UbiquitousLanguageManager.Infrastructure.Data.Entities;
using System.Globalization;

namespace UbiquitousLanguageManager.Infrastructure.Repositories;

/// <summary>
/// ユーザーリポジトリの実装（簡易版）
/// 雛型として最小限の機能を実装
/// </summary>
public class UserRepository : IUserRepository
{
    private readonly UbiquitousLanguageDbContext _context;
    private readonly Microsoft.Extensions.Logging.ILogger<UserRepository> _logger;

    /// <summary>
    /// UserRepositoryのコンストラクタ
    /// </summary>
    public UserRepository(UbiquitousLanguageDbContext context, Microsoft.Extensions.Logging.ILogger<UserRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// 一時的な簡易実装（雛型用）
    /// </summary>
    public async Task<FSharpResult<FSharpOption<User>, string>> GetByEmailAsync(Email email)
    {
        var startTime = DateTime.UtcNow;
        try
        {
            _logger.LogDebug("Starting GetByEmailAsync for email: {Email}", email.Value);

            // 簡易実装：実際のDB検索は省略
            await Task.Delay(1); // async警告解消用
            var userName = UserName.create("Sample User");
            var user = User.create(email, userName.ResultValue, Role.GeneralUser, UserId.NewUserId(1L));
            var option = FSharpOption<User>.Some(user);

            var duration = DateTime.UtcNow - startTime;
            _logger.LogInformation("GetByEmailAsync completed successfully for email: {Email} in {Duration}ms",
                email.Value, duration.TotalMilliseconds);

            return FSharpResult<FSharpOption<User>, string>.NewOk(option);
        }
        catch (Exception ex)
        {
            var duration = DateTime.UtcNow - startTime;
            _logger.LogError(ex, "GetByEmailAsync failed for email: {Email} after {Duration}ms",
                email.Value, duration.TotalMilliseconds);
            return FSharpResult<FSharpOption<User>, string>.NewError(ex.Message);
        }
    }

    /// <summary>
    /// ユーザーIDによるユーザー検索
    /// F#のUserIdとC#のApplicationUser.Idのマッピングを実装
    /// </summary>
    public async Task<FSharpResult<FSharpOption<User>, string>> GetByIdAsync(UserId id)
    {
        var startTime = DateTime.UtcNow;
        try
        {
            _logger.LogDebug("Starting GetByIdAsync for userId: {UserId}", id.Item);

            // データベースから全ユーザーを取得（論理削除されていないユーザーのみ、ロール情報含む）
            var entities = await _context.Users
                .Include(u => u.Roles)
                .Where(u => !u.IsDeleted)
                .ToListAsync();

            // 各ユーザーのId.GetHashCode()を計算し、入力UserIdと比較
            foreach (var entity in entities)
            {
                var entityHashId = (long)entity.Id.GetHashCode();
                if (entityHashId == id.Item)
                {
                    // 一致するユーザーが見つかった場合、ToDomainUserで変換
                    var userResult = ToDomainUser(entity);
                    if (userResult.IsOk)
                    {
                        var duration = DateTime.UtcNow - startTime;
                        _logger.LogInformation("GetByIdAsync completed successfully for userId: {UserId} in {Duration}ms",
                            id.Item, duration.TotalMilliseconds);

                        return FSharpResult<FSharpOption<User>, string>.NewOk(
                            FSharpOption<User>.Some(userResult.ResultValue));
                    }

                    // 変換エラーの場合
                    var errorDuration = DateTime.UtcNow - startTime;
                    _logger.LogError("GetByIdAsync conversion failed for userId: {UserId} after {Duration}ms: {Error}",
                        id.Item, errorDuration.TotalMilliseconds, userResult.ErrorValue);
                    return FSharpResult<FSharpOption<User>, string>.NewError(userResult.ErrorValue);
                }
            }

            // ユーザーが見つからなかった場合
            var notFoundDuration = DateTime.UtcNow - startTime;
            _logger.LogInformation("GetByIdAsync completed for userId: {UserId} in {Duration}ms (no user found)",
                id.Item, notFoundDuration.TotalMilliseconds);

            return FSharpResult<FSharpOption<User>, string>.NewOk(FSharpOption<User>.None);
        }
        catch (Exception ex)
        {
            var duration = DateTime.UtcNow - startTime;
            _logger.LogError(ex, "GetByIdAsync failed for userId: {UserId} after {Duration}ms",
                id.Item, duration.TotalMilliseconds);
            return FSharpResult<FSharpOption<User>, string>.NewError(ex.Message);
        }
    }

    /// <summary>
    /// ASP.NET Identity の文字列 ID でユーザーを検索
    /// GetHashCode()による不安定なマッチングの代替として、Identity ID で直接検索
    /// </summary>
    public async Task<FSharpResult<FSharpOption<User>, string>> GetByIdentityIdAsync(string identityId)
    {
        var startTime = DateTime.UtcNow;
        try
        {
            _logger.LogDebug("Starting GetByIdentityIdAsync for identityId: {IdentityId}", identityId);

            // Identity ID で直接検索（ロール情報を含めて取得）
            var entity = await _context.Users
                .Include(u => u.Roles)
                .FirstOrDefaultAsync(u => u.Id == identityId && !u.IsDeleted);

            if (entity == null)
            {
                var duration = DateTime.UtcNow - startTime;
                _logger.LogInformation("GetByIdentityIdAsync completed for identityId: {IdentityId} in {Duration}ms (no user found)",
                    identityId, duration.TotalMilliseconds);
                return FSharpResult<FSharpOption<User>, string>.NewOk(FSharpOption<User>.None);
            }

            // デバッグログ: Rolesコレクションの状態確認
            var rolesCount = entity.Roles?.Count ?? -1;
            _logger.LogWarning("GetByIdentityIdAsync DEBUG - User: {UserId}, Roles loaded: {RolesLoaded}, Count: {RolesCount}",
                entity.Id, entity.Roles != null, rolesCount);

            var userResult = ToDomainUser(entity);
            if (userResult.IsOk)
            {
                var duration = DateTime.UtcNow - startTime;
                _logger.LogInformation("GetByIdentityIdAsync completed successfully for identityId: {IdentityId} in {Duration}ms",
                    identityId, duration.TotalMilliseconds);
                return FSharpResult<FSharpOption<User>, string>.NewOk(
                    FSharpOption<User>.Some(userResult.ResultValue));
            }

            return FSharpResult<FSharpOption<User>, string>.NewError(userResult.ErrorValue);
        }
        catch (Exception ex)
        {
            var duration = DateTime.UtcNow - startTime;
            _logger.LogError(ex, "GetByIdentityIdAsync failed for identityId: {IdentityId} after {Duration}ms",
                identityId, duration.TotalMilliseconds);
            return FSharpResult<FSharpOption<User>, string>.NewError(ex.Message);
        }
    }

    /// <summary>
    /// 一時的な簡易実装（雛型用）
    /// </summary>
    public async Task<FSharpResult<User, string>> SaveAsync(User user)
    {
        var startTime = DateTime.UtcNow;
        try
        {
            _logger.LogDebug("Starting SaveAsync for user: {Email}", user.Email.Value);

            await Task.Delay(1); // async警告解消用

            var duration = DateTime.UtcNow - startTime;
            _logger.LogInformation("SaveAsync completed successfully for user: {Email} in {Duration}ms",
                user.Email.Value, duration.TotalMilliseconds);

            return FSharpResult<User, string>.NewOk(user);
        }
        catch (Exception ex)
        {
            var duration = DateTime.UtcNow - startTime;
            _logger.LogError(ex, "SaveAsync failed for user: {Email} after {Duration}ms",
                user.Email.Value, duration.TotalMilliseconds);
            return FSharpResult<User, string>.NewError(ex.Message);
        }
    }

    /// <summary>
    /// 一時的な簡易実装（雛型用）
    /// </summary>
    public async Task<FSharpResult<FSharpList<User>, string>> GetByProjectIdAsync(ProjectId projectId)
    {
        try
        {
            await Task.Delay(1); // async警告解消用
            var emptyList = FSharpList<User>.Empty;
            return FSharpResult<FSharpList<User>, string>.NewOk(emptyList);
        }
        catch (Exception ex)
        {
            return FSharpResult<FSharpList<User>, string>.NewError(ex.Message);
        }
    }

    /// <summary>
    /// 一時的な簡易実装（雛型用）
    /// </summary>
    public async Task<FSharpResult<Unit, string>> DeleteAsync(UserId id)
    {
        try
        {
            await Task.Delay(1); // async警告解消用
            // 一時的にUnit型を使わない実装
            return FSharpResult<Unit, string>.NewError("Not implemented in prototype");
        }
        catch (Exception ex)
        {
            return FSharpResult<Unit, string>.NewError(ex.Message);
        }
    }

    // =================================================================
    // 🔄 プライベートヘルパーメソッド：Entity ⇄ Domain変換
    // =================================================================

    /// <summary>
    /// C#のApplicationUserをF#のUserドメインエンティティに変換
    /// F#のValue Objectのスマートコンストラクタを使用して安全に変換
    /// Phase A2: ASP.NET Core Identity統合対応版
    ///
    /// 【変更履歴】
    /// - Phase B-F3: static → インスタンスメソッドに変更（ロール取得のためDbContext必要）
    /// </summary>
    /// <param name="entity">C#のApplicationUser（ASP.NET Core Identity対応）</param>
    /// <returns>F#のResult型でラップされたUser</returns>
    private FSharpResult<User, string> ToDomainUser(ApplicationUser entity)
    {
        if (entity == null)
        {
            return FSharpResult<User, string>.NewError("ApplicationUser cannot be null");
        }

        try
        {
            // F#のValue Objectのスマートコンストラクタを使用
            // ASP.NET Core IdentityのEmailプロパティ（NULL許容）に対応
            var emailString = entity.Email ?? string.Empty;
            var emailResult = Email.create(emailString);
            if (emailResult.IsError)
            {
                return FSharpResult<User, string>.NewError($"Invalid email: {emailResult.ErrorValue}");
            }

            var nameResult = UserName.create(entity.Name);
            if (nameResult.IsError)
            {
                return FSharpResult<User, string>.NewError($"Invalid name: {nameResult.ErrorValue}");
            }

            // ASP.NET Core Identity Rolesから判別共用体に変換（設計書準拠）
            // ApplicationUser.Roles ナビゲーションプロパティから取得
            var roleResult = GetUserRoleFromEntity(entity);
            if (roleResult.IsError)
            {
                return FSharpResult<User, string>.NewError($"Invalid role: {roleResult.ErrorValue}");
            }

            // UserIdの生成（ASP.NET Core IdentityのGUID文字列からF#のUserIdに変換）
            // 【F#初学者向け解説】
            // ASP.NET Core IdentityはGUID文字列（450文字まで）をユーザーIDとして使用します。
            // F#のUserIdはlong型を内部的に使用するため、文字列をハッシュ化して変換するか、
            // 別の方法でマッピングする必要があります。ここでは簡易的にHashCodeを使用。
            var userIdValue = (long)entity.Id.GetHashCode();
            var userId = UserId.NewUserId(userIdValue);

            // F#のUser.create静的メソッドを使用して作成（Phase A2対応）
            // 【F#初学者向け解説】
            // F#のレコード型は通常、スマートコンストラクタパターンを使用します。
            // User.create静的メソッドが最小限の必須パラメータでUserエンティティを作成し、
            // 他のプロパティはデフォルト値で初期化されます。
            var user = User.create(emailResult.ResultValue, nameResult.ResultValue, roleResult.ResultValue, userId);

            return FSharpResult<User, string>.NewOk(user);
        }
        catch (Exception ex)
        {
            return FSharpResult<User, string>.NewError($"Conversion error: {ex.Message}");
        }
    }

    /// <summary>
    /// F#のUserドメインエンティティをC#のApplicationUserに変換
    /// F#のValue Objectから値を取得してC#のPOCOに設定
    /// Phase A2: ASP.NET Core Identity統合対応版
    /// </summary>
    /// <param name="user">F#のUser</param>
    /// <returns>C#のApplicationUser（ASP.NET Core Identity対応）</returns>
    private static ApplicationUser ToEntity(User user)
    {
        // F#のUserIdからGUID文字列を生成
        // 【Blazor Server・F#初学者向け解説】
        // ASP.NET Core IdentityはGUID文字列をプライマリキーとして使用します。
        // F#のlong型UserIdを一意のGUID文字列に変換する必要があります。
        var guidId = new Guid((int)(user.Id.Item % int.MaxValue), 0, 0, 0, 0, 0, 0, 0, 0, 0, 0).ToString();

        return new ApplicationUser
        {
            // ASP.NET Core Identity標準カラム
            Id = guidId,                                  // F#のUserId → GUID文字列変換
            UserName = user.Email.Value,                  // メールアドレスをユーザー名として使用
            NormalizedUserName = user.Email.Value.ToUpperInvariant(),
            Email = user.Email.Value,                     // F#のEmail値オブジェクトから値を取得
            NormalizedEmail = user.Email.Value.ToUpperInvariant(),
            EmailConfirmed = true,                        // 初期状態では確認済みとする
            LockoutEnabled = false,                       // ロックアウト機能は無効
            AccessFailedCount = 0,                        // アクセス失敗回数は0で初期化
            
            // プロジェクト固有カスタムカラム
            Name = user.Name.Value,                       // F#のUserName値オブジェクトから値を取得
            // UserRoleプロパティ削除（ASP.NET Core Identity標準Roles使用）
            IsFirstLogin = user.IsFirstLogin,
            UpdatedAt = DateTime.UtcNow,                     // 現在時刻で初期化（DateTime型）
            UpdatedBy = guidId,                           // 作成者・更新者は自分自身として設定
            // CreatedAt/CreatedByプロパティ削除（設計書にない実装のため）
            IsDeleted = false,                            // 新規作成時は削除フラグOFF
            PasswordHash = null                           // 実装時に適切なハッシュ値を設定
        };
    }

    /// <summary>
    /// ApplicationUserエンティティからF#のRole判別共用体を取得
    /// ASP.NET Core Identity の UserRoles から実際のロールを取得して変換
    ///
    /// 【F#初学者向け解説】
    /// ASP.NET Core Identityでは、ユーザーは複数のロールを持つことができますが、
    /// このシステムでは1ユーザー1ロールの設計となっています。
    /// 優先順位: SuperUser > ProjectManager > DomainApprover > GeneralUser
    /// </summary>
    /// <param name="entity">ApplicationUserエンティティ（Rolesナビゲーションプロパティ必須）</param>
    /// <returns>F#のResult型でラップされたRole</returns>
    private FSharpResult<Role, string> GetUserRoleFromEntity(ApplicationUser entity)
    {
        try
        {
            // 【修正】ナビゲーションプロパティが読み込まれていない場合は直接DBからロールを取得
            // ASP.NET Core IdentityのDbContext継承では、Include()がIdentityUserRole<string>に
            // 対して正しく機能しない場合があるため、フォールバック処理を追加
            List<string?> roleNames;

            if (entity.Roles != null && entity.Roles.Any())
            {
                // ナビゲーションプロパティが読み込まれている場合
                var roleIds = entity.Roles.Select(r => r.RoleId).ToList();
                roleNames = _context.Roles
                    .Where(r => roleIds.Contains(r.Id))
                    .Select(r => r.Name)
                    .ToList();
            }
            else
            {
                // 【フォールバック】ナビゲーションプロパティが読み込まれていない場合は直接クエリ
                // AspNetUserRoles + AspNetRoles を直接JOINしてロール名を取得
                roleNames = (from ur in _context.Set<IdentityUserRole<string>>()
                            join r in _context.Roles on ur.RoleId equals r.Id
                            where ur.UserId == entity.Id
                            select r.Name).ToList();

                if (!roleNames.Any())
                {
                    // ロールが未設定の場合はGeneralUserとして扱う
                    _logger.LogWarning("User {UserId} has no roles assigned, defaulting to GeneralUser", entity.Id);
                    return FSharpResult<Role, string>.NewOk(Role.GeneralUser);
                }
            }

            // 優先順位に従ってロールを決定
            if (roleNames.Contains("SuperUser"))
                return FSharpResult<Role, string>.NewOk(Role.SuperUser);
            if (roleNames.Contains("ProjectManager"))
                return FSharpResult<Role, string>.NewOk(Role.ProjectManager);
            if (roleNames.Contains("DomainApprover"))
                return FSharpResult<Role, string>.NewOk(Role.DomainApprover);
            if (roleNames.Contains("GeneralUser"))
                return FSharpResult<Role, string>.NewOk(Role.GeneralUser);

            // 該当するロールがない場合はエラー
            var rolesString = string.Join(", ", roleNames);
            _logger.LogWarning("User {UserId} has unknown roles: {Roles}, defaulting to GeneralUser", entity.Id, rolesString);
            return FSharpResult<Role, string>.NewOk(Role.GeneralUser);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting role for user {UserId}", entity.Id);
            return FSharpResult<Role, string>.NewError($"ロール取得エラー: {ex.Message}");
        }
    }

    /// <summary>
    /// Phase A2: 文字列をF#のRole判別共用体に変換（新権限システム対応）
    /// </summary>
    /// <param name="roleString">ロールの文字列表現</param>
    /// <returns>F#のResult型でラップされたRole</returns>
    private static FSharpResult<Role, string> StringToRole(string roleString)
    {
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
    /// Phase A2: F#のRole判別共用体を文字列に変換（新権限システム対応）
    /// </summary>
    /// <param name="role">F#のRole判別共用体</param>
    /// <returns>文字列表現</returns>
    private static string RoleToString(Role role)
    {
        if (role.IsSuperUser) return "SuperUser";
        if (role.IsProjectManager) return "ProjectManager";
        if (role.IsDomainApprover) return "DomainApprover";
        if (role.IsGeneralUser) return "GeneralUser";
        return "Unknown";
    }

    // =================================================================
    // 🆕 Phase A2: 新インターフェースメソッド実装
    // =================================================================

    /// <summary>
    /// Phase A2: アクティブユーザー一覧取得
    /// 論理削除されていないユーザーのみを取得
    /// </summary>
    /// <returns>F#のResult型でラップされたアクティブユーザーリスト</returns>
    public async Task<FSharpResult<FSharpList<User>, string>> GetAllActiveUsersAsync()
    {
        try
        {
            // 【Blazor Server・F#初学者向け解説】
            // EF Coreを使用してデータベースからアクティブユーザーを取得
            // Where句でIsActive = trueかつ論理削除されていないユーザーをフィルタリング
            // Include()でロール情報を一括取得（N+1問題回避）
            var entities = await _context.Users
                .Include(u => u.Roles)
                .Where(u => !u.IsDeleted)
                .OrderBy(u => u.Name)
                .ToListAsync();

            var users = new List<User>();
            var errors = new List<string>();

            // 各EntityをDomainオブジェクトに変換
            foreach (var entity in entities)
            {
                var userResult = ToDomainUser(entity);
                if (userResult.IsOk)
                {
                    users.Add(userResult.ResultValue);
                }
                else
                {
                    errors.Add($"User ID {entity.Id}: {userResult.ErrorValue}");
                }
            }

            if (errors.Any())
            {
                _logger.LogWarning("Some users could not be converted: {Errors}", string.Join(", ", errors));
            }

            // F#のFSharpListに変換
            var fsharpList = ListModule.OfSeq(users);
            return FSharpResult<FSharpList<User>, string>.NewOk(fsharpList);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving active users");
            return FSharpResult<FSharpList<User>, string>.NewError($"データベースエラー: {ex.Message}");
        }
    }

    /// <summary>
    /// Phase A2: 全ユーザー一覧取得（無効化ユーザー含む）
    /// 管理者向けの完全なユーザーリスト取得
    /// </summary>
    /// <returns>F#のResult型でラップされた全ユーザーリスト</returns>
    public async Task<FSharpResult<FSharpList<User>, string>> GetAllUsersAsync()
    {
        try
        {
            // 論理削除されていないユーザーのみ取得（IsActiveは問わない）
            // Include()でロール情報を一括取得（N+1問題回避）
            var entities = await _context.Users
                .Include(u => u.Roles)
                .Where(u => !u.IsDeleted)
                .OrderBy(u => u.Name)
                .ToListAsync();

            var users = new List<User>();
            var errors = new List<string>();

            foreach (var entity in entities)
            {
                var userResult = ToDomainUser(entity);
                if (userResult.IsOk)
                {
                    users.Add(userResult.ResultValue);
                }
                else
                {
                    errors.Add($"User ID {entity.Id}: {userResult.ErrorValue}");
                }
            }

            if (errors.Any())
            {
                _logger.LogWarning("Some users could not be converted: {Errors}", string.Join(", ", errors));
            }

            var fsharpList = ListModule.OfSeq(users);
            return FSharpResult<FSharpList<User>, string>.NewOk(fsharpList);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all users");
            return FSharpResult<FSharpList<User>, string>.NewError($"データベースエラー: {ex.Message}");
        }
    }

    /// <summary>
    /// Phase A2: ロール別ユーザー取得
    /// 特定のロールを持つユーザーを取得
    /// </summary>
    /// <param name="role">F#のRole判別共用体</param>
    /// <returns>F#のResult型でラップされたユーザーリスト</returns>
    public Task<FSharpResult<FSharpList<User>, string>> GetByRoleAsync(Role role)
    {
        try
        {
            var roleString = RoleToString(role);
            
            // UserRoleプロパティ削除のため、一時的に空のリストを返す
            // 将来的にASP.NET Core Identity Roles機能で実装予定
            var entities = new List<ApplicationUser>();

            var users = new List<User>();
            var errors = new List<string>();

            foreach (var entity in entities)
            {
                var userResult = ToDomainUser(entity);
                if (userResult.IsOk)
                {
                    users.Add(userResult.ResultValue);
                }
                else
                {
                    errors.Add($"User ID {entity.Id}: {userResult.ErrorValue}");
                }
            }

            if (errors.Any())
            {
                _logger.LogWarning("Some users could not be converted for role {Role}: {Errors}", roleString, string.Join(", ", errors));
            }

            var fsharpList = ListModule.OfSeq(users);
            return Task.FromResult(FSharpResult<FSharpList<User>, string>.NewOk(fsharpList));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving users by role {Role}", RoleToString(role));
            return Task.FromResult(FSharpResult<FSharpList<User>, string>.NewError($"データベースエラー: {ex.Message}"));
        }
    }

    /// <summary>
    /// Phase A2: ユーザー検索
    /// 名前・メールアドレスでの部分一致検索（PostgreSQL pg_trgm対応）
    /// </summary>
    /// <param name="searchTerm">検索キーワード</param>
    /// <returns>F#のResult型でラップされた検索結果ユーザーリスト</returns>
    public async Task<FSharpResult<FSharpList<User>, string>> SearchUsersAsync(string searchTerm)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                // 検索語が空の場合は全アクティブユーザーを返す
                return await GetAllActiveUsersAsync();
            }

            var normalizedSearchTerm = searchTerm.Trim().ToLower(CultureInfo.InvariantCulture);
            
            // 【PostgreSQL pg_trgm対応】
            // 実際の本格実装では、pg_trgm拡張とGINインデックスを使用した類似検索を行う
            // 現在はLIKE検索で代替実装
            // Include()でロール情報を一括取得（N+1問題回避）
            var entities = await _context.Users
                .Include(u => u.Roles)
                .Where(u => !u.IsDeleted &&
                           (EF.Functions.ILike(u.Name, $"%{normalizedSearchTerm}%") ||
                            EF.Functions.ILike(u.Email ?? "", $"%{normalizedSearchTerm}%")))
                .OrderBy(u => u.Name)
                .ToListAsync();

            var users = new List<User>();
            var errors = new List<string>();

            foreach (var entity in entities)
            {
                var userResult = ToDomainUser(entity);
                if (userResult.IsOk)
                {
                    users.Add(userResult.ResultValue);
                }
                else
                {
                    errors.Add($"User ID {entity.Id}: {userResult.ErrorValue}");
                }
            }

            if (errors.Any())
            {
                _logger.LogWarning("Some users could not be converted in search: {Errors}", string.Join(", ", errors));
            }

            var fsharpList = ListModule.OfSeq(users);
            return FSharpResult<FSharpList<User>, string>.NewOk(fsharpList);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching users with term: {SearchTerm}", searchTerm);
            return FSharpResult<FSharpList<User>, string>.NewError($"検索エラー: {ex.Message}");
        }
    }

    /// <summary>
    /// Phase A2: ユーザー統計情報取得
    /// ダッシュボード等で使用する統計データの取得
    /// </summary>
    /// <returns>F#のResult型でラップされた統計情報オブジェクト</returns>
    public async Task<FSharpResult<object, string>> GetUserStatisticsAsync()
    {
        try
        {
            // ユーザー統計情報を取得
            var totalUsers = await _context.Users.CountAsync(u => !u.IsDeleted);
            var activeUsers = await _context.Users.CountAsync(u => !u.IsDeleted);
            var inactiveUsers = totalUsers - activeUsers;
            var firstLoginUsers = await _context.Users.CountAsync(u => u.IsFirstLogin && !u.IsDeleted);
            
            // ロール別統計（UserRoleプロパティ削除のため一時無効化）
            var roleStats = new List<object>();

            var statistics = new
            {
                TotalUsers = totalUsers,
                ActiveUsers = activeUsers,
                InactiveUsers = inactiveUsers,
                FirstLoginUsers = firstLoginUsers,
                RoleStatistics = new Dictionary<string, int>(), // UserRoleプロパティ削除のため空辞書
                LastUpdated = DateTime.UtcNow
            };

            return FSharpResult<object, string>.NewOk(statistics);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving user statistics");
            return FSharpResult<object, string>.NewError($"統計情報取得エラー: {ex.Message}");
        }
    }

    // =================================================================
    // 🚀 Team 3: 高性能検索・フィルタリング・ページング機能
    // =================================================================

    /// <summary>
    /// 高性能ページング対応ユーザー検索
    /// PostgreSQL最適化クエリとインデックス活用による大量データ対応
    /// </summary>
    /// <param name="searchTerm">検索キーワード（名前・メール部分一致）</param>
    /// <param name="roleFilter">ロールフィルター（null時は全ロール）</param>
    /// <param name="statusFilter">ステータスフィルター（active/inactive/all）</param>
    /// <param name="pageNumber">ページ番号（1ベース）</param>
    /// <param name="pageSize">1ページあたりのデータ件数</param>
    /// <returns>ページング結果とメタデータ</returns>
    public async Task<FSharpResult<object, string>> GetUsersWithPagingAsync(
        string? searchTerm = null,
        string? roleFilter = null,
        string statusFilter = "active",
        int pageNumber = 1,
        int pageSize = 20)
    {
        try
        {
            // パラメータ検証
            if (pageNumber < 1)
            {
                return FSharpResult<object, string>.NewError("ページ番号は1以上である必要があります");
            }
            if (pageSize < 1)
            {
                return FSharpResult<object, string>.NewError("ページサイズは1以上である必要があります");
            }
            if (pageSize > 100)
            {
                return FSharpResult<object, string>.NewError("ページサイズは100以下である必要があります");
            }

            var query = _context.Users.AsQueryable();

            // 基本フィルター（論理削除されていないユーザー）
            query = query.Where(u => !u.IsDeleted);

            // ステータスフィルター
            switch (statusFilter.ToLower())
            {
                case "active":
                    query = query.Where(u => !u.IsDeleted);
                    break;
                case "inactive":
                    query = query.Where(u => u.IsDeleted);
                    break;
                case "all":
                default:
                    // 全ステータス対象（削除済み以外）
                    break;
            }

            // ロールフィルター
            // roleFilterはUserRoleプロパティ削除のため一時無効化
            // 将来的にASP.NET Core Identity Rolesで実装

            // 検索フィルター（PostgreSQL pg_trgm最適化）
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var normalizedSearchTerm = searchTerm.Trim().ToLower(CultureInfo.InvariantCulture);
                
                // PostgreSQL pg_trgm GINインデックス活用のため、ILike使用
                query = query.Where(u => 
                    EF.Functions.ILike(u.Name, $"%{normalizedSearchTerm}%") ||
                    EF.Functions.ILike(u.Email ?? "", $"%{normalizedSearchTerm}%"));
            }

            // 総件数取得（フィルター適用後）
            var totalCount = await query.CountAsync();
            var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

            // ページング実行とソート（インデックス活用のため、Name順）
            // Include()でロール情報を一括取得（N+1問題回避）
            var entities = await query
                .Include(u => u.Roles)
                .OrderBy(u => u.Name)
                .ThenBy(u => u.Email)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // Entity→Domain変換
            var users = new List<User>();
            var errors = new List<string>();

            foreach (var entity in entities)
            {
                var userResult = ToDomainUser(entity);
                if (userResult.IsOk)
                {
                    users.Add(userResult.ResultValue);
                }
                else
                {
                    errors.Add($"User ID {entity.Id}: {userResult.ErrorValue}");
                }
            }

            if (errors.Any())
            {
                _logger.LogWarning("Some users could not be converted in paged search: {Errors}", string.Join(", ", errors));
            }

            // ページング結果の構築
            var result = new
            {
                Users = users,
                TotalCount = totalCount,
                TotalPages = totalPages,
                CurrentPage = pageNumber,
                PageSize = pageSize,
                HasPrevious = pageNumber > 1,
                HasNext = pageNumber < totalPages,
                SearchTerm = searchTerm,
                RoleFilter = roleFilter,
                StatusFilter = statusFilter
            };

            return FSharpResult<object, string>.NewOk(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in paged user search. SearchTerm: {SearchTerm}, Role: {Role}, Status: {Status}, Page: {Page}", 
                searchTerm, roleFilter, statusFilter, pageNumber);
            return FSharpResult<object, string>.NewError($"ページング検索エラー: {ex.Message}");
        }
    }

    /// <summary>
    /// 高度なユーザーフィルタリング
    /// 複数条件による高性能検索（PostgreSQL複合インデックス活用）
    /// </summary>
    /// <param name="filters">フィルター条件オブジェクト</param>
    /// <returns>フィルター結果</returns>
    public async Task<FSharpResult<FSharpList<User>, string>> GetUsersWithAdvancedFiltersAsync(object filters)
    {
        try
        {
            // フィルター条件の動的解析（リフレクション使用）
            var filtersType = filters.GetType();
            var query = _context.Users.AsQueryable();

            // 基本フィルター
            query = query.Where(u => !u.IsDeleted);

            // 動的フィルター適用
            var nameProperty = filtersType.GetProperty("Name");
            if (nameProperty?.GetValue(filters) is string name && !string.IsNullOrWhiteSpace(name))
            {
                query = query.Where(u => EF.Functions.ILike(u.Name, $"%{name.ToLower()}%"));
            }

            var emailProperty = filtersType.GetProperty("Email");
            if (emailProperty?.GetValue(filters) is string email && !string.IsNullOrWhiteSpace(email))
            {
                query = query.Where(u => EF.Functions.ILike(u.Email ?? "", $"%{email.ToLower()}%"));
            }

            var roleProperty = filtersType.GetProperty("Role");
            // UserRoleフィルターは一時無効化（プロパティ削除のため）

            var isActiveProperty = filtersType.GetProperty("IsActive");
            if (isActiveProperty?.GetValue(filters) is bool isActive)
            {
                query = query.Where(u => !u.IsDeleted == isActive);
            }

            var isFirstLoginProperty = filtersType.GetProperty("IsFirstLogin");
            if (isFirstLoginProperty?.GetValue(filters) is bool isFirstLogin)
            {
                query = query.Where(u => u.IsFirstLogin == isFirstLogin);
            }

            // CreatedAtフィルターは一時無効化（プロパティ削除のため）
            // 将来的にUpdatedAtで代替可能

            // パフォーマンス最適化：インデックス活用のためソート
            // Include()でロール情報を一括取得（N+1問題回避）
            var entities = await query
                .Include(u => u.Roles)
                .OrderBy(u => u.Name)
                .ThenBy(u => u.UpdatedAt) // CreatedAt→UpdatedAtに変更
                .ToListAsync();

            // Entity→Domain変換
            var users = new List<User>();
            var errors = new List<string>();

            foreach (var entity in entities)
            {
                var userResult = ToDomainUser(entity);
                if (userResult.IsOk)
                {
                    users.Add(userResult.ResultValue);
                }
                else
                {
                    errors.Add($"User ID {entity.Id}: {userResult.ErrorValue}");
                }
            }

            if (errors.Any())
            {
                _logger.LogWarning("Some users could not be converted in advanced filter: {Errors}", string.Join(", ", errors));
            }

            var fsharpList = ListModule.OfSeq(users);
            return FSharpResult<FSharpList<User>, string>.NewOk(fsharpList);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in advanced user filtering");
            return FSharpResult<FSharpList<User>, string>.NewError($"高度フィルタリングエラー: {ex.Message}");
        }
    }

    /// <summary>
    /// PostgreSQL全文検索対応ユーザー検索
    /// pg_trgm + GINインデックスによる高速類似検索
    /// </summary>
    /// <param name="searchTerm">検索語</param>
    /// <param name="similarityThreshold">類似度閾値（0.1-1.0）</param>
    /// <returns>類似度順ソート済み結果</returns>
    public async Task<FSharpResult<FSharpList<User>, string>> SearchUsersWithSimilarityAsync(
        string searchTerm, 
        double similarityThreshold = 0.3)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                return await GetAllActiveUsersAsync();
            }

            // PostgreSQL pg_trgm similarity関数使用
            // 注意：実際の本格運用ではpg_trgm拡張とGINインデックス設定が必要
            var normalizedSearchTerm = searchTerm.Trim().ToLower(CultureInfo.InvariantCulture);

            // Include()でロール情報を一括取得（N+1問題回避）
            var entities = await _context.Users
                .Include(u => u.Roles)
                .Where(u => !u.IsDeleted)
                .Where(u =>
                    // PostgreSQL similarity関数のEF Core近似実装
                    EF.Functions.ILike(u.Name, $"%{normalizedSearchTerm}%") ||
                    EF.Functions.ILike(u.Email ?? "", $"%{normalizedSearchTerm}%") ||
                    // 部分一致による類似検索
                    u.Name.ToLower().Contains(normalizedSearchTerm) ||
                    (u.Email != null && u.Email.ToLower().Contains(normalizedSearchTerm)))
                // 名前の完全一致→部分一致→メール一致の順でソート
                .OrderBy(u => u.Name.ToLower().StartsWith(normalizedSearchTerm) ? 0 : 1)
                .ThenBy(u => u.Name.ToLower().Contains(normalizedSearchTerm) ? 0 : 1)
                .ThenBy(u => u.Name)
                .ToListAsync();

            var users = new List<User>();
            var errors = new List<string>();

            foreach (var entity in entities)
            {
                var userResult = ToDomainUser(entity);
                if (userResult.IsOk)
                {
                    users.Add(userResult.ResultValue);
                }
                else
                {
                    errors.Add($"User ID {entity.Id}: {userResult.ErrorValue}");
                }
            }

            if (errors.Any())
            {
                _logger.LogWarning("Some users could not be converted in similarity search: {Errors}", string.Join(", ", errors));
            }

            var fsharpList = ListModule.OfSeq(users);
            return FSharpResult<FSharpList<User>, string>.NewOk(fsharpList);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in similarity user search. SearchTerm: {SearchTerm}", searchTerm);
            return FSharpResult<FSharpList<User>, string>.NewError($"類似検索エラー: {ex.Message}");
        }
    }

    /// <summary>
    /// 高性能ユーザー集計
    /// PostgreSQL集計関数とインデックス最適化による高速統計処理
    /// </summary>
    /// <returns>詳細統計情報</returns>
    public async Task<FSharpResult<object, string>> GetDetailedUserStatisticsAsync()
    {
        try
        {
            // 並列実行による高速化
            var tasks = new[]
            {
                // 基本統計
                _context.Users.CountAsync(u => !u.IsDeleted),
                _context.Users.CountAsync(u => !u.IsDeleted),
                _context.Users.CountAsync(u => u.IsFirstLogin && !u.IsDeleted),
                
                // 時系列統計（最近30日）- CreatedAt→UpdatedAtに変更
                _context.Users.CountAsync(u => u.UpdatedAt >= DateTime.UtcNow.AddDays(-30) && !u.IsDeleted),
                _context.Users.CountAsync(u => u.UpdatedAt >= DateTime.UtcNow.AddDays(-30) && !u.IsDeleted),
            };

            var results = await Task.WhenAll(tasks);

            // ロール別統計（UserRoleプロパティ削除のため一時無効化）
            var roleStatsTask = Task.FromResult(new List<object>());

            // 月別作成数統計（CreatedAt→UpdatedAtに変更）
            var monthlyStatsTask = _context.Users
                .Where(u => !u.IsDeleted && u.UpdatedAt >= DateTime.UtcNow.AddMonths(-12))
                .GroupBy(u => new { Year = u.UpdatedAt.Year, Month = u.UpdatedAt.Month })
                .Select(g => new { 
                    Year = g.Key.Year, 
                    Month = g.Key.Month, 
                    Count = g.Count() 
                })
                .OrderBy(s => s.Year)
                .ThenBy(s => s.Month)
                .ToListAsync();

            var roleStats = await roleStatsTask;
            var monthlyStats = await monthlyStatsTask;

            var detailedStatistics = new
            {
                // 基本統計
                TotalUsers = results[0],
                ActiveUsers = results[1],
                FirstLoginUsers = results[2],
                InactiveUsers = results[0] - results[1],
                
                // 時系列統計
                NewUsersLast30Days = results[3],
                UpdatedUsersLast30Days = results[4],
                
                // ロール別統計
                RoleStatistics = new Dictionary<string, int>(), // UserRoleプロパティ削除のため空辞書
                
                // 月別統計
                MonthlyCreationStats = monthlyStats.Select(m => new {
                    Period = $"{m.Year}-{m.Month:D2}",
                    Count = m.Count
                }).ToList(),
                
                // アクティビティ率
                ActiveUserPercentage = results[0] > 0 ? Math.Round((double)results[1] / results[0] * 100, 2) : 0,
                FirstLoginPercentage = results[1] > 0 ? Math.Round((double)results[2] / results[1] * 100, 2) : 0,
                
                // メタデータ
                LastUpdated = DateTime.UtcNow,
                CalculationTimeMs = 0 // 実際の処理時間は別途計測
            };

            return FSharpResult<object, string>.NewOk(detailedStatistics);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving detailed user statistics");
            return FSharpResult<object, string>.NewError($"詳細統計情報取得エラー: {ex.Message}");
        }
    }
}