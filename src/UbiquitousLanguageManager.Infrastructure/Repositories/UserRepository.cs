using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using UbiquitousLanguageManager.Application;
using UbiquitousLanguageManager.Domain;
using UbiquitousLanguageManager.Infrastructure.Data;
using UbiquitousLanguageManager.Infrastructure.Data.Entities;

namespace UbiquitousLanguageManager.Infrastructure.Repositories;

/// <summary>
/// ユーザーリポジトリの実装
/// F# Application層のインターフェースを C# Infrastructure層で実装
/// Entity Framework Core を使用したデータアクセス
/// </summary>
public class UserRepository : IUserRepository
{
    private readonly UbiquitousLanguageDbContext _context;
    private readonly IDbContextFactory<UbiquitousLanguageDbContext> _contextFactory;
    private readonly ILogger<UserRepository> _logger;

    /// <summary>
    /// コンストラクタ: 依存関係の注入
    /// </summary>
    /// <param name="context">データベースコンテキスト</param>
    /// <param name="contextFactory">DbContext ファクトリー（マルチスレッド対応）</param>
    /// <param name="logger">ログ出力</param>
    public UserRepository(
        UbiquitousLanguageDbContext context,
        IDbContextFactory<UbiquitousLanguageDbContext> contextFactory,
        ILogger<UserRepository> logger)
    {
        _context = context;
        _contextFactory = contextFactory;
        _logger = logger;
    }

    /// <summary>
    /// メールアドレスによるユーザー検索
    /// F# の Result型を使用したエラーハンドリング
    /// </summary>
    /// <param name="email">検索対象のメールアドレス</param>
    /// <returns>ユーザー情報（存在しない場合は None）</returns>
    public async Task<Result<Microsoft.FSharp.Core.FSharpOption<User>, string>> GetByEmailAsync(Email email)
    {
        try
        {
            _logger.LogDebug("📧 ユーザー検索開始: メールアドレス = {Email}", email.Value);

            // 🔍 Entity Framework による検索実行
            var userEntity = await _context.Users
                .Where(u => u.Email == email.Value && u.IsActive)
                .FirstOrDefaultAsync();

            if (userEntity == null)
            {
                _logger.LogDebug("🔍 ユーザーが見つかりませんでした: {Email}", email.Value);
                return Result<Microsoft.FSharp.Core.FSharpOption<User>, string>
                    .NewSuccess(Microsoft.FSharp.Core.FSharpOption<User>.None);
            }

            // 🔄 Entity から F# Domain エンティティへの変換
            var domainUser = ConvertToDomainUser(userEntity);
            
            if (domainUser.IsSuccess)
            {
                _logger.LogDebug("✅ ユーザー検索成功: {Email}", email.Value);
                return Result<Microsoft.FSharp.Core.FSharpOption<User>, string>
                    .NewSuccess(Microsoft.FSharp.Core.FSharpOption<User>.Some(domainUser.ResultValue));
            }

            _logger.LogWarning("⚠️ ユーザーデータの変換に失敗: {Email}, エラー: {Error}", 
                email.Value, domainUser.ErrorValue);
            return Result<Microsoft.FSharp.Core.FSharpOption<User>, string>
                .NewError($"ユーザーデータの変換エラー: {domainUser.ErrorValue}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ ユーザー検索中にエラーが発生: {Email}", email.Value);
            return Result<Microsoft.FSharp.Core.FSharpOption<User>, string>
                .NewError($"データベースエラー: {ex.Message}");
        }
    }

    /// <summary>
    /// IDによるユーザー検索
    /// </summary>
    /// <param name="userId">ユーザーID</param>
    /// <returns>ユーザー情報（存在しない場合は None）</returns>
    public async Task<Result<Microsoft.FSharp.Core.FSharpOption<User>, string>> GetByIdAsync(UserId userId)
    {
        try
        {
            _logger.LogDebug("🔍 ユーザー検索開始: ID = {UserId}", userId.Item);

            var userEntity = await _context.Users
                .Where(u => u.Id == userId.Item && u.IsActive)
                .FirstOrDefaultAsync();

            if (userEntity == null)
            {
                _logger.LogDebug("🔍 ユーザーが見つかりませんでした: ID = {UserId}", userId.Item);
                return Result<Microsoft.FSharp.Core.FSharpOption<User>, string>
                    .NewSuccess(Microsoft.FSharp.Core.FSharpOption<User>.None);
            }

            var domainUser = ConvertToDomainUser(userEntity);
            
            if (domainUser.IsSuccess)
            {
                _logger.LogDebug("✅ ユーザー検索成功: ID = {UserId}", userId.Item);
                return Result<Microsoft.FSharp.Core.FSharpOption<User>, string>
                    .NewSuccess(Microsoft.FSharp.Core.FSharpOption<User>.Some(domainUser.ResultValue));
            }

            return Result<Microsoft.FSharp.Core.FSharpOption<User>, string>
                .NewError($"ユーザーデータの変換エラー: {domainUser.ErrorValue}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ ユーザー検索中にエラーが発生: ID = {UserId}", userId.Item);
            return Result<Microsoft.FSharp.Core.FSharpOption<User>, string>
                .NewError($"データベースエラー: {ex.Message}");
        }
    }

    /// <summary>
    /// ユーザー保存（新規作成・更新）
    /// F# Domain エンティティの永続化
    /// </summary>
    /// <param name="user">保存するユーザー情報</param>
    /// <returns>保存されたユーザー情報</returns>
    public async Task<Result<User, string>> SaveAsync(User user)
    {
        try
        {
            _logger.LogDebug("💾 ユーザー保存開始: {Email}", user.Email.Value);

            // 🔄 F# Domain エンティティから Entity への変換
            var userEntity = ConvertToUserEntity(user);

            if (user.Id.Item == 0)
            {
                // 🆕 新規作成
                _context.Users.Add(userEntity);
                _logger.LogDebug("🆕 新規ユーザー作成: {Email}", user.Email.Value);
            }
            else
            {
                // 🔄 更新
                _context.Users.Update(userEntity);
                _logger.LogDebug("🔄 ユーザー更新: ID = {UserId}", user.Id.Item);
            }

            await _context.SaveChangesAsync();

            // 🔄 保存後のエンティティを Domain エンティティに再変換
            var savedDomainUser = ConvertToDomainUser(userEntity);
            
            if (savedDomainUser.IsSuccess)
            {
                _logger.LogInformation("✅ ユーザー保存成功: {Email}", user.Email.Value);
                return savedDomainUser;
            }

            return Result<User, string>.NewError($"保存後の変換エラー: {savedDomainUser.ErrorValue}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ ユーザー保存中にエラーが発生: {Email}", user.Email.Value);
            return Result<User, string>.NewError($"データベースエラー: {ex.Message}");
        }
    }

    /// <summary>
    /// プロジェクト別ユーザー一覧取得
    /// 🔧 現在は簡易実装（今後、UserProjects テーブル連携で拡張予定）
    /// </summary>
    public async Task<Result<Microsoft.FSharp.Collections.FSharpList<User>, string>> GetByProjectIdAsync(ProjectId projectId)
    {
        try
        {
            _logger.LogDebug("📋 プロジェクト別ユーザー一覧取得: ProjectId = {ProjectId}", projectId.Item);

            // 🔧 現在は全アクティブユーザーを返す（今後実装予定）
            var userEntities = await _context.Users
                .Where(u => u.IsActive)
                .ToListAsync();

            var domainUsers = new List<User>();
            
            foreach (var entity in userEntities)
            {
                var domainUser = ConvertToDomainUser(entity);
                if (domainUser.IsSuccess)
                {
                    domainUsers.Add(domainUser.ResultValue);
                }
            }

            var fsharpList = Microsoft.FSharp.Collections.ListModule.OfSeq(domainUsers);
            
            _logger.LogDebug("✅ ユーザー一覧取得成功: {Count}件", domainUsers.Count);
            return Result<Microsoft.FSharp.Collections.FSharpList<User>, string>
                .NewSuccess(fsharpList);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ ユーザー一覧取得中にエラーが発生: ProjectId = {ProjectId}", projectId.Item);
            return Result<Microsoft.FSharp.Collections.FSharpList<User>, string>
                .NewError($"データベースエラー: {ex.Message}");
        }
    }

    /// <summary>
    /// ユーザー削除（論理削除）
    /// IsActive フラグを false に設定
    /// </summary>
    public async Task<Result<Microsoft.FSharp.Core.FSharpUnit, string>> DeleteAsync(UserId userId)
    {
        try
        {
            _logger.LogDebug("🗑️ ユーザー削除開始: ID = {UserId}", userId.Item);

            var userEntity = await _context.Users
                .Where(u => u.Id == userId.Item)
                .FirstOrDefaultAsync();

            if (userEntity == null)
            {
                return Result<Microsoft.FSharp.Core.FSharpUnit, string>
                    .NewError("削除対象のユーザーが見つかりません");
            }

            // 🔒 論理削除の実行
            userEntity.IsActive = false;
            userEntity.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            
            _logger.LogInformation("✅ ユーザー削除成功: ID = {UserId}", userId.Item);
            return Result<Microsoft.FSharp.Core.FSharpUnit, string>
                .NewSuccess(Microsoft.FSharp.Core.FSharpUnit.Default);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ ユーザー削除中にエラーが発生: ID = {UserId}", userId.Item);
            return Result<Microsoft.FSharp.Core.FSharpUnit, string>
                .NewError($"データベースエラー: {ex.Message}");
        }
    }

    #region プライベートヘルパーメソッド

    /// <summary>
    /// UserEntity から F# Domain User への変換
    /// </summary>
    private Result<User, string> ConvertToDomainUser(UserEntity entity)
    {
        // 🔧 Value Object の作成（検証も同時実行）
        var emailResult = Email.create(entity.Email);
        var nameResult = UserName.create(entity.Name);
        var roleResult = ParseUserRole(entity.UserRole);

        if (emailResult.IsSuccess && nameResult.IsSuccess && roleResult.IsSuccess)
        {
            var user = new User
            {
                Id = new UserId(entity.Id),
                Email = emailResult.ResultValue,
                Name = nameResult.ResultValue,
                Role = roleResult.ResultValue,
                IsActive = entity.IsActive,
                IsFirstLogin = entity.IsFirstLogin,
                UpdatedAt = entity.UpdatedAt,
                UpdatedBy = new UserId(entity.UpdatedBy)
            };

            return Result<User, string>.NewSuccess(user);
        }

        var errors = new List<string>();
        if (emailResult.IsError) errors.Add(emailResult.ErrorValue);
        if (nameResult.IsError) errors.Add(nameResult.ErrorValue);
        if (roleResult.IsError) errors.Add(roleResult.ErrorValue);

        return Result<User, string>.NewError(string.Join(", ", errors));
    }

    /// <summary>
    /// F# Domain User から UserEntity への変換
    /// </summary>
    private UserEntity ConvertToUserEntity(User user)
    {
        return new UserEntity
        {
            Id = user.Id.Item,
            Email = user.Email.Value,
            Name = user.Name.Value,
            UserRole = user.Role.ToString(),
            IsActive = user.IsActive,
            IsFirstLogin = user.IsFirstLogin,
            UpdatedAt = user.UpdatedAt,
            UpdatedBy = user.UpdatedBy.Item,
            PasswordHash = "" // 🔧 実装時に適切な値を設定
        };
    }

    /// <summary>
    /// 文字列から UserRole への変換
    /// </summary>
    private Result<UserRole, string> ParseUserRole(string roleString)
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
}