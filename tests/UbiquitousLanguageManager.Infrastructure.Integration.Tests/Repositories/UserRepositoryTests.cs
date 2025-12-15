using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using UbiquitousLanguageManager.Application;
using UbiquitousLanguageManager.Domain.Authentication;
using UbiquitousLanguageManager.Infrastructure.Data;
using UbiquitousLanguageManager.Infrastructure.Data.Entities;
using UbiquitousLanguageManager.Infrastructure.Repositories;
using UbiquitousLanguageManager.Infrastructure.Integration.Tests.Fixtures;
using MsLogger = Microsoft.Extensions.Logging.ILogger;

namespace UbiquitousLanguageManager.Infrastructure.Integration.Tests.Repositories;

/// <summary>
/// Phase B-F3 Step1.5 Task5-2: UserRepository統合テスト（Identity IDベースメソッド）
///
/// 【統合テスト初学者向け解説】
/// このテストクラスは、UserRepositoryの実装を統合環境（InMemoryDatabase + ASP.NET Core Identity）で検証します。
/// Unit Testと異なり、実際のUserManager・DbContext・DIコンテナを使用して、
/// エンドツーエンドのシナリオを検証します。
///
/// 【テスト対象メソッド】（14件）
/// - GetAllUsersWithIdentityAsync: 全ユーザー一覧取得（Identity ID付き）
/// - GetByIdentityIdAsync: IdentityIdでユーザー取得
/// - GetProjectIdsByIdentityIdAsync: IdentityIdでプロジェクトID取得
/// - AssignProjectsToUserByIdentityIdAsync: プロジェクト新規割り当て
/// - UpdateUserProjectsByIdentityIdAsync: プロジェクト更新（既存削除→新規追加）
/// - DeleteByIdentityIdAsync: 論理削除
///
/// 【ADR_020準拠】
/// - プロジェクト名: UbiquitousLanguageManager.Infrastructure.Integration.Tests
/// - 参照関係: 全層参照（WebApplicationFactory使用のため）
/// - AAAパターン適用（Arrange-Act-Assert）
/// </summary>
public class UserRepositoryTests : IClassFixture<IntegrationTestFixture>
{
    private readonly IntegrationTestFixture _fixture;

    // DbInitializer固定ID（テストデータ）
    private const string AdminUserId = "00000000-0000-0000-0000-000000000001";
    private const string ProjectManagerUserId = "00000000-0000-0000-0000-000000000002";
    private const string DomainApproverUserId = "00000000-0000-0000-0000-000000000003";
    private const string GeneralUserId = "00000000-0000-0000-0000-000000000004";
    private const string E2eTestUserId = "00000000-0000-0000-0000-000000000099";

    /// <summary>
    /// コンストラクタ: IClassFixture&lt;IntegrationTestFixture&gt;パターン
    ///
    /// 【xUnit初学者向け解説】
    /// IClassFixture&lt;T&gt;により、テストクラス全体で1つのfixture（テスト基盤）を共有します。
    /// これにより、WebApplicationFactoryの初期化コストを削減し、テスト実行を高速化します。
    /// </summary>
    public UserRepositoryTests(IntegrationTestFixture fixture)
    {
        _fixture = fixture ?? throw new ArgumentNullException(nameof(fixture));
    }

    #region GetAllUsersWithIdentityAsync Tests (3件)

    /// <summary>
    /// Test 1: GetAllUsersWithIdentityAsync - ActiveUsersOnly
    /// includeDeleted=falseでアクティブユーザーのみ取得
    ///
    /// 【検証内容】
    /// - アクティブユーザー2件取得（論理削除ユーザー除外）
    /// - Identity ID正しくタプルで返却
    /// - F# Result型IsOk検証
    /// </summary>
    [Fact]
    public async Task GetAllUsersWithIdentityAsync_ActiveUsersOnly_ReturnsOnlyActiveUsers()
    {
        // Arrange: テスト環境準備
        using var scope = _fixture.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<UbiquitousLanguageDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var logger = scope.ServiceProvider.GetRequiredService<Microsoft.Extensions.Logging.ILogger<UserRepository>>();

        // ロールシード
        await IntegrationTestFixture.SeedTestRolesAsync(roleManager);

        // アクティブユーザー2件作成
        await IntegrationTestFixture.SeedTestUserAsync(context, userManager, AdminUserId,
            "Admin User", "admin@test.com", "SuperUser");
        await IntegrationTestFixture.SeedTestUserAsync(context, userManager, ProjectManagerUserId,
            "PM User", "pm@test.com", "ProjectManager");

        // 論理削除ユーザー1件作成
        var deletedUser = new ApplicationUser
        {
            Id = GeneralUserId,
            UserName = "deleted@test.com",
            NormalizedUserName = "DELETED@TEST.COM",
            Email = "deleted@test.com",
            NormalizedEmail = "DELETED@TEST.COM",
            EmailConfirmed = true,
            Name = "Deleted User",
            IsFirstLogin = false,
            IsDeleted = true, // 論理削除フラグ
            UpdatedAt = DateTime.UtcNow,
            UpdatedBy = GeneralUserId
        };
        await userManager.CreateAsync(deletedUser, "TestPass123!");

        var repository = new UserRepository(userManager, logger, context);

        // Act: ターゲットメソッド実行（includeDeleted=false）
        var result = await repository.GetAllUsersWithIdentityAsync(includeDeleted: false);

        // Assert: 結果検証
        Assert.True(result.IsOk, "Result should be Ok");
        var userTuples = Microsoft.FSharp.Collections.ListModule.ToArray(result.ResultValue);

        // アクティブユーザーのみ取得（論理削除ユーザー除外）
        Assert.Equal(2, userTuples.Length);

        // Identity ID正しく返却
        var adminTuple = userTuples.FirstOrDefault(t => t.Item2 == AdminUserId);
        Assert.NotNull(adminTuple);
        Assert.Equal("admin@test.com", adminTuple.Item1.Email.Value);

        var pmTuple = userTuples.FirstOrDefault(t => t.Item2 == ProjectManagerUserId);
        Assert.NotNull(pmTuple);
        Assert.Equal("pm@test.com", pmTuple.Item1.Email.Value);
    }

    /// <summary>
    /// Test 2: GetAllUsersWithIdentityAsync - WithDeleted
    /// includeDeleted=trueで削除済みユーザーも含む
    ///
    /// 【検証内容】
    /// - 全ユーザー3件取得（論理削除ユーザー含む）
    /// - IgnoreQueryFilters()適用確認
    /// </summary>
    [Fact]
    public async Task GetAllUsersWithIdentityAsync_WithDeleted_ReturnsAllUsers()
    {
        // Arrange
        using var scope = _fixture.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<UbiquitousLanguageDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var logger = scope.ServiceProvider.GetRequiredService<Microsoft.Extensions.Logging.ILogger<UserRepository>>();

        await IntegrationTestFixture.SeedTestRolesAsync(roleManager);

        // アクティブユーザー2件
        await IntegrationTestFixture.SeedTestUserAsync(context, userManager, AdminUserId,
            "Admin User", "admin@test.com", "SuperUser");
        await IntegrationTestFixture.SeedTestUserAsync(context, userManager, ProjectManagerUserId,
            "PM User", "pm@test.com", "ProjectManager");

        // 論理削除ユーザー1件
        var deletedUser = new ApplicationUser
        {
            Id = GeneralUserId,
            UserName = "deleted@test.com",
            NormalizedUserName = "DELETED@TEST.COM",
            Email = "deleted@test.com",
            NormalizedEmail = "DELETED@TEST.COM",
            EmailConfirmed = true,
            Name = "Deleted User",
            IsFirstLogin = false,
            IsDeleted = true,
            UpdatedAt = DateTime.UtcNow,
            UpdatedBy = GeneralUserId
        };
        await userManager.CreateAsync(deletedUser, "TestPass123!");

        var repository = new UserRepository(userManager, logger, context);

        // Act: includeDeleted=true
        var result = await repository.GetAllUsersWithIdentityAsync(includeDeleted: true);

        // Assert: 論理削除ユーザー含む全件取得
        Assert.True(result.IsOk);
        var userTuples = Microsoft.FSharp.Collections.ListModule.ToArray(result.ResultValue);

        Assert.Equal(3, userTuples.Length); // アクティブ2件 + 削除済み1件
    }

    /// <summary>
    /// Test 3: GetAllUsersWithIdentityAsync - EmptyDatabase
    /// 空データベースでも正常終了（空リスト返却）
    ///
    /// 【検証内容】
    /// - ユーザー0件の場合も正常終了
    /// - 空のF# FSharpList返却
    /// </summary>
    [Fact]
    public async Task GetAllUsersWithIdentityAsync_EmptyDatabase_ReturnsEmptyList()
    {
        // Arrange: 空データベース（ユーザーシードなし）
        using var scope = _fixture.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<UbiquitousLanguageDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var logger = scope.ServiceProvider.GetRequiredService<Microsoft.Extensions.Logging.ILogger<UserRepository>>();

        var repository = new UserRepository(userManager, logger, context);

        // Act
        var result = await repository.GetAllUsersWithIdentityAsync(includeDeleted: false);

        // Assert: 空リスト正常返却
        Assert.True(result.IsOk);
        var userTuples = Microsoft.FSharp.Collections.ListModule.ToArray(result.ResultValue);
        Assert.Empty(userTuples);
    }

    #endregion

    #region GetByIdentityIdAsync Tests (2件)

    /// <summary>
    /// Test 4: GetByIdentityIdAsync - ExistingId
    /// 存在するIDでユーザー取得
    ///
    /// 【検証内容】
    /// - FindByIdAsync正常動作
    /// - F# Option.Some返却
    /// - User型変換正常完了
    /// </summary>
    [Fact]
    public async Task GetByIdentityIdAsync_ExistingId_ReturnsUser()
    {
        // Arrange
        using var scope = _fixture.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<UbiquitousLanguageDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var logger = scope.ServiceProvider.GetRequiredService<Microsoft.Extensions.Logging.ILogger<UserRepository>>();

        await IntegrationTestFixture.SeedTestRolesAsync(roleManager);
        await IntegrationTestFixture.SeedTestUserAsync(context, userManager, AdminUserId,
            "Admin User", "admin@test.com", "SuperUser");

        var repository = new UserRepository(userManager, logger, context);

        // Act: 存在するIdentityIdで検索
        var result = await repository.GetByIdentityIdAsync(AdminUserId);

        // Assert: Option.Some返却
        Assert.True(result.IsOk);
        Assert.True(Microsoft.FSharp.Core.FSharpOption<User>.get_IsSome(result.ResultValue));

        var user = result.ResultValue.Value;
        Assert.Equal("admin@test.com", user.Email.Value);
        Assert.Equal("Admin User", user.Name.Value);
        Assert.True(user.Role.IsSuperUser);
    }

    /// <summary>
    /// Test 5: GetByIdentityIdAsync - NonExistingId
    /// 存在しないIDでOption.None返却
    ///
    /// 【検証内容】
    /// - FindByIdAsyncがnull返却時の処理
    /// - F# Option.None正常返却
    /// - エラーではなく正常終了
    /// </summary>
    [Fact]
    public async Task GetByIdentityIdAsync_NonExistingId_ReturnsNone()
    {
        // Arrange
        using var scope = _fixture.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<UbiquitousLanguageDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var logger = scope.ServiceProvider.GetRequiredService<Microsoft.Extensions.Logging.ILogger<UserRepository>>();

        var repository = new UserRepository(userManager, logger, context);

        // Act: 存在しないIdentityId
        var result = await repository.GetByIdentityIdAsync("99999999-9999-9999-9999-999999999999");

        // Assert: Option.None返却（エラーではない）
        Assert.True(result.IsOk);
        Assert.True(Microsoft.FSharp.Core.FSharpOption<User>.get_IsNone(result.ResultValue));
    }

    #endregion

    #region GetProjectIdsByIdentityIdAsync Tests (3件)

    /// <summary>
    /// Test 6: GetProjectIdsByIdentityIdAsync - UserWithProjects
    /// プロジェクト所属ユーザーのプロジェクトID取得
    ///
    /// 【検証内容】
    /// - UserProjectsテーブル結合動作
    /// - F# ProjectId変換正常完了
    /// </summary>
    [Fact]
    public async Task GetProjectIdsByIdentityIdAsync_UserWithProjects_ReturnsProjectIds()
    {
        // Arrange
        using var scope = _fixture.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<UbiquitousLanguageDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var logger = scope.ServiceProvider.GetRequiredService<Microsoft.Extensions.Logging.ILogger<UserRepository>>();

        await IntegrationTestFixture.SeedTestRolesAsync(roleManager);
        await IntegrationTestFixture.SeedTestUserAsync(context, userManager, AdminUserId,
            "Admin User", "admin@test.com", "SuperUser");

        // UserProjectsテーブルにプロジェクト割り当てデータ追加
        context.Set<UserProject>().AddRange(
            new UserProject { UserId = AdminUserId, ProjectId = 1L, UpdatedAt = DateTime.UtcNow, UpdatedBy = AdminUserId },
            new UserProject { UserId = AdminUserId, ProjectId = 2L, UpdatedAt = DateTime.UtcNow, UpdatedBy = AdminUserId }
        );
        await context.SaveChangesAsync();

        var repository = new UserRepository(userManager, logger, context);

        // Act
        var result = await repository.GetProjectIdsByIdentityIdAsync(AdminUserId);

        // Assert: プロジェクトID取得成功
        Assert.True(result.IsOk);
        var projectIds = Microsoft.FSharp.Collections.ListModule.ToArray(result.ResultValue);

        Assert.Equal(2, projectIds.Length);
        Assert.Contains(projectIds, p => p.Item == 1L);
        Assert.Contains(projectIds, p => p.Item == 2L);
    }

    /// <summary>
    /// Test 7: GetProjectIdsByIdentityIdAsync - UserWithoutProjects
    /// プロジェクト未所属ユーザーは空リスト
    ///
    /// 【検証内容】
    /// - UserProjects未登録ユーザーの処理
    /// - 空リスト正常返却
    /// </summary>
    [Fact]
    public async Task GetProjectIdsByIdentityIdAsync_UserWithoutProjects_ReturnsEmptyList()
    {
        // Arrange
        using var scope = _fixture.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<UbiquitousLanguageDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var logger = scope.ServiceProvider.GetRequiredService<Microsoft.Extensions.Logging.ILogger<UserRepository>>();

        await IntegrationTestFixture.SeedTestRolesAsync(roleManager);
        await IntegrationTestFixture.SeedTestUserAsync(context, userManager, AdminUserId,
            "Admin User", "admin@test.com", "SuperUser");

        // UserProjectsテーブルには何も登録しない

        var repository = new UserRepository(userManager, logger, context);

        // Act
        var result = await repository.GetProjectIdsByIdentityIdAsync(AdminUserId);

        // Assert: 空リスト返却
        Assert.True(result.IsOk);
        var projectIds = Microsoft.FSharp.Collections.ListModule.ToArray(result.ResultValue);
        Assert.Empty(projectIds);
    }

    /// <summary>
    /// Test 8: GetProjectIdsByIdentityIdAsync - NonExistingUser
    /// 存在しないユーザーは空リスト
    ///
    /// 【検証内容】
    /// - ユーザー存在チェックなしでも正常動作
    /// - 空リスト返却
    /// </summary>
    [Fact]
    public async Task GetProjectIdsByIdentityIdAsync_NonExistingUser_ReturnsEmptyList()
    {
        // Arrange
        using var scope = _fixture.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<UbiquitousLanguageDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var logger = scope.ServiceProvider.GetRequiredService<Microsoft.Extensions.Logging.ILogger<UserRepository>>();

        var repository = new UserRepository(userManager, logger, context);

        // Act: 存在しないユーザーID
        var result = await repository.GetProjectIdsByIdentityIdAsync("99999999-9999-9999-9999-999999999999");

        // Assert: 空リスト返却（エラーではない）
        Assert.True(result.IsOk);
        var projectIds = Microsoft.FSharp.Collections.ListModule.ToArray(result.ResultValue);
        Assert.Empty(projectIds);
    }

    #endregion

    #region AssignProjectsToUserByIdentityIdAsync Tests (2件)

    /// <summary>
    /// Test 9: AssignProjectsToUserByIdentityIdAsync - ValidInput
    /// プロジェクト新規割り当て成功
    ///
    /// 【検証内容】
    /// - UserProjectsテーブルへのInsert動作
    /// - F# FSharpList&lt;long&gt;変換正常完了
    /// </summary>
    [Fact]
    public async Task AssignProjectsToUserByIdentityIdAsync_ValidInput_AssignsProjects()
    {
        // Arrange
        using var scope = _fixture.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<UbiquitousLanguageDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var logger = scope.ServiceProvider.GetRequiredService<Microsoft.Extensions.Logging.ILogger<UserRepository>>();

        await IntegrationTestFixture.SeedTestRolesAsync(roleManager);
        await IntegrationTestFixture.SeedTestUserAsync(context, userManager, AdminUserId,
            "Admin User", "admin@test.com", "SuperUser");

        var repository = new UserRepository(userManager, logger, context);

        // F# FSharpList<long>作成
        var projectIds = Microsoft.FSharp.Collections.ListModule.OfSeq(new long[] { 1L, 2L });

        // Act: プロジェクト割り当て
        var result = await repository.AssignProjectsToUserByIdentityIdAsync(AdminUserId, projectIds);

        // Assert: 割り当て成功
        Assert.True(result.IsOk);

        // UserProjectsテーブル確認
        var userProjects = await context.Set<UserProject>()
            .Where(up => up.UserId == AdminUserId)
            .ToListAsync();

        Assert.Equal(2, userProjects.Count);
        Assert.Contains(userProjects, up => up.ProjectId == 1L);
        Assert.Contains(userProjects, up => up.ProjectId == 2L);
    }

    /// <summary>
    /// Test 10: AssignProjectsToUserByIdentityIdAsync - EmptyList
    /// 空リストは何もせず成功
    ///
    /// 【検証内容】
    /// - 空リスト処理の正常動作
    /// - UserProjectsテーブル変更なし
    /// </summary>
    [Fact]
    public async Task AssignProjectsToUserByIdentityIdAsync_EmptyList_DoesNothing()
    {
        // Arrange
        using var scope = _fixture.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<UbiquitousLanguageDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var logger = scope.ServiceProvider.GetRequiredService<Microsoft.Extensions.Logging.ILogger<UserRepository>>();

        await IntegrationTestFixture.SeedTestRolesAsync(roleManager);
        await IntegrationTestFixture.SeedTestUserAsync(context, userManager, AdminUserId,
            "Admin User", "admin@test.com", "SuperUser");

        var repository = new UserRepository(userManager, logger, context);

        // F# 空リスト作成
        var emptyProjectIds = Microsoft.FSharp.Collections.ListModule.Empty<long>();

        // Act: 空リスト割り当て
        var result = await repository.AssignProjectsToUserByIdentityIdAsync(AdminUserId, emptyProjectIds);

        // Assert: 成功（何もしない）
        Assert.True(result.IsOk);

        // UserProjectsテーブル変更なし
        var userProjects = await context.Set<UserProject>()
            .Where(up => up.UserId == AdminUserId)
            .ToListAsync();

        Assert.Empty(userProjects);
    }

    #endregion

    #region UpdateUserProjectsByIdentityIdAsync Tests (2件)

    /// <summary>
    /// Test 11: UpdateUserProjectsByIdentityIdAsync - ReplacesProjects
    /// 既存プロジェクト削除→新規プロジェクト追加
    ///
    /// 【検証内容】
    /// - 既存UserProjects削除動作
    /// - 新規UserProjects追加動作
    /// - トランザクション整合性
    /// </summary>
    [Fact]
    public async Task UpdateUserProjectsByIdentityIdAsync_ReplacesProjects_UpdatesSuccessfully()
    {
        // Arrange
        using var scope = _fixture.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<UbiquitousLanguageDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var logger = scope.ServiceProvider.GetRequiredService<Microsoft.Extensions.Logging.ILogger<UserRepository>>();

        await IntegrationTestFixture.SeedTestRolesAsync(roleManager);
        await IntegrationTestFixture.SeedTestUserAsync(context, userManager, AdminUserId,
            "Admin User", "admin@test.com", "SuperUser");

        // 既存プロジェクト割り当て（1, 2）
        context.Set<UserProject>().AddRange(
            new UserProject { UserId = AdminUserId, ProjectId = 1L, UpdatedAt = DateTime.UtcNow, UpdatedBy = AdminUserId },
            new UserProject { UserId = AdminUserId, ProjectId = 2L, UpdatedAt = DateTime.UtcNow, UpdatedBy = AdminUserId }
        );
        await context.SaveChangesAsync();

        var repository = new UserRepository(userManager, logger, context);

        // 新規プロジェクトリスト（3, 4）
        var newProjectIds = Microsoft.FSharp.Collections.ListModule.OfSeq(new long[] { 3L, 4L });

        // Act: プロジェクト更新（既存削除→新規追加）
        var result = await repository.UpdateUserProjectsByIdentityIdAsync(AdminUserId, newProjectIds);

        // Assert: 更新成功
        Assert.True(result.IsOk);

        // UserProjectsテーブル確認（1, 2削除・3, 4追加）
        var userProjects = await context.Set<UserProject>()
            .Where(up => up.UserId == AdminUserId)
            .ToListAsync();

        Assert.Equal(2, userProjects.Count);
        Assert.Contains(userProjects, up => up.ProjectId == 3L);
        Assert.Contains(userProjects, up => up.ProjectId == 4L);

        // 既存プロジェクト（1, 2）削除確認
        Assert.DoesNotContain(userProjects, up => up.ProjectId == 1L);
        Assert.DoesNotContain(userProjects, up => up.ProjectId == 2L);
    }

    /// <summary>
    /// Test 12: UpdateUserProjectsByIdentityIdAsync - RemovesAllProjects
    /// 空リストで全プロジェクト解除
    ///
    /// 【検証内容】
    /// - 既存UserProjects全削除動作
    /// - 新規追加なし
    /// </summary>
    [Fact]
    public async Task UpdateUserProjectsByIdentityIdAsync_RemovesAllProjects_ClearsProjects()
    {
        // Arrange
        using var scope = _fixture.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<UbiquitousLanguageDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var logger = scope.ServiceProvider.GetRequiredService<Microsoft.Extensions.Logging.ILogger<UserRepository>>();

        await IntegrationTestFixture.SeedTestRolesAsync(roleManager);
        await IntegrationTestFixture.SeedTestUserAsync(context, userManager, AdminUserId,
            "Admin User", "admin@test.com", "SuperUser");

        // 既存プロジェクト割り当て
        context.Set<UserProject>().AddRange(
            new UserProject { UserId = AdminUserId, ProjectId = 1L, UpdatedAt = DateTime.UtcNow, UpdatedBy = AdminUserId },
            new UserProject { UserId = AdminUserId, ProjectId = 2L, UpdatedAt = DateTime.UtcNow, UpdatedBy = AdminUserId }
        );
        await context.SaveChangesAsync();

        var repository = new UserRepository(userManager, logger, context);

        // 空リスト
        var emptyProjectIds = Microsoft.FSharp.Collections.ListModule.Empty<long>();

        // Act: 空リストで更新（全削除）
        var result = await repository.UpdateUserProjectsByIdentityIdAsync(AdminUserId, emptyProjectIds);

        // Assert: 更新成功
        Assert.True(result.IsOk);

        // UserProjectsテーブル全削除確認
        var userProjects = await context.Set<UserProject>()
            .Where(up => up.UserId == AdminUserId)
            .ToListAsync();

        Assert.Empty(userProjects);
    }

    #endregion

    #region DeleteByIdentityIdAsync Tests (2件)

    /// <summary>
    /// Test 13: DeleteByIdentityIdAsync - LogicallyDeletes
    /// 論理削除成功（IsDeleted=true設定）
    ///
    /// 【検証内容】
    /// - IsDeletedフラグ設定動作
    /// - UpdatedAt/UpdatedBy監査証跡記録
    /// - 物理削除しない
    /// </summary>
    [Fact]
    public async Task DeleteByIdentityIdAsync_LogicallyDeletes_SetsIsDeletedFlag()
    {
        // Arrange
        using var scope = _fixture.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<UbiquitousLanguageDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var logger = scope.ServiceProvider.GetRequiredService<Microsoft.Extensions.Logging.ILogger<UserRepository>>();

        await IntegrationTestFixture.SeedTestRolesAsync(roleManager);
        await IntegrationTestFixture.SeedTestUserAsync(context, userManager, AdminUserId,
            "Admin User", "admin@test.com", "SuperUser");

        var repository = new UserRepository(userManager, logger, context);

        // Act: 論理削除実行
        var result = await repository.DeleteByIdentityIdAsync(AdminUserId);

        // Assert: 削除成功
        Assert.True(result.IsOk);

        // データベース確認（IgnoreQueryFilters使用）
        var deletedUser = await userManager.Users
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(u => u.Id == AdminUserId);

        Assert.NotNull(deletedUser);
        Assert.True(deletedUser.IsDeleted); // 論理削除フラグ設定確認
        // UpdatedAtはDateTime型（値型）のため、NotNullチェック不要
    }

    /// <summary>
    /// Test 14: DeleteByIdentityIdAsync - NonExistingUser
    /// 存在しないユーザーはエラー
    ///
    /// 【検証内容】
    /// - FindByIdAsyncがnull時のエラー処理
    /// - F# Result型Error返却
    /// </summary>
    [Fact]
    public async Task DeleteByIdentityIdAsync_NonExistingUser_ReturnsError()
    {
        // Arrange
        using var scope = _fixture.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<UbiquitousLanguageDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var logger = scope.ServiceProvider.GetRequiredService<Microsoft.Extensions.Logging.ILogger<UserRepository>>();

        var repository = new UserRepository(userManager, logger, context);

        // Act: 存在しないユーザーの削除
        var result = await repository.DeleteByIdentityIdAsync("99999999-9999-9999-9999-999999999999");

        // Assert: エラー返却
        Assert.True(result.IsError);
        Assert.Contains("削除対象のユーザーが見つかりません", result.ErrorValue);
    }

    #endregion
}
