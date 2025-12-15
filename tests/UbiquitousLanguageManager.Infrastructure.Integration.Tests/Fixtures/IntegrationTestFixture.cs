using System;
using System.Linq;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using UbiquitousLanguageManager.Infrastructure.Data;
using UbiquitousLanguageManager.Infrastructure.Data.Entities;

namespace UbiquitousLanguageManager.Infrastructure.Integration.Tests.Fixtures;

/// <summary>
/// WebApplicationFactory統合テスト基盤
///
/// 【統合テスト初学者向け解説】
/// このクラスは、実際のASP.NET Core Webアプリケーション環境をテスト用に構築します。
/// WebApplicationFactory&lt;Program&gt;を継承することで、本番環境とほぼ同じDI・ミドルウェア構成で
/// 統合テストを実行できます。
///
/// 【Phase B-F3 Step1.5】
/// UserRepositoryの統合テストでは、InMemoryDatabaseを使用してテストデータを準備し、
/// 実際のUserRepository実装を検証します。
/// </summary>
public class IntegrationTestFixture : WebApplicationFactory<Program>
{
    /// <summary>
    /// テスト用WebHostBuilder設定オーバーライド
    ///
    /// 【統合テスト設計パターン】
    /// ConfigureWebHostメソッドをオーバーライドして、テスト用の設定（InMemoryDatabase等）を適用します。
    /// これにより、PostgreSQL実データベースに影響を与えずにテストを実行できます。
    /// </summary>
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // 既存のDbContext登録を削除
            // 【DI初学者向け解説】
            // 本番環境ではPostgreSQLを使用しますが、テストではInMemoryDatabaseを使用します。
            // そのため、既存のDbContext登録を削除し、テスト用の登録に置き換えます。
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<UbiquitousLanguageDbContext>));

            if (descriptor != null)
            {
                services.Remove(descriptor);
            }

            // テスト用InMemoryDatabase設定
            // 【InMemoryDatabase特性】
            // - GUID生成により、テストごとに独立したデータベースを作成
            // - テスト間のデータ競合を防止
            // - 高速なテスト実行（ディスクI/O不要）
            services.AddDbContext<UbiquitousLanguageDbContext>(options =>
            {
                options.UseInMemoryDatabase($"IntegrationTestDb_{Guid.NewGuid()}");
                options.EnableSensitiveDataLogging(); // テスト時の詳細ログ出力
            });

            // 【重要】ASP.NET Core Identity設定は削除
            // Program.csで既に設定されているため、ここで再度AddIdentityを呼ぶと
            // "Scheme already exists: Identity.Application" エラーが発生します。
            // WebApplicationFactoryパターンでは、Program.csの設定を再利用します。

            // ただし、IdentityOptionsのパスワードポリシーはテスト用に緩和
            // 【テストデータ作成の簡素化】
            // InitialDataServiceが"su"パスワードでユーザー作成を試みるため、
            // パスワードポリシーを緩和しないとテスト失敗します。
            services.Configure<IdentityOptions>(options =>
            {
                // パスワードポリシー緩和（テストデータ作成容易化）
                options.Password.RequireDigit = false;
                options.Password.RequiredLength = 1;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;
                options.Password.RequireLowercase = false;

                // ユーザー設定
                options.User.RequireUniqueEmail = true;
            });

            // テスト用ログ出力強化（デバッグ時に有用）
            services.AddLogging(loggingBuilder =>
            {
                loggingBuilder.ClearProviders();
                loggingBuilder.AddConsole();
                loggingBuilder.SetMinimumLevel(LogLevel.Debug);
            });
        });

        // テスト環境設定
        builder.UseEnvironment("Testing");
    }

    /// <summary>
    /// テスト用サービススコープ作成ヘルパー
    ///
    /// 【使用パターン】
    /// using var scope = fixture.CreateScope();
    /// var context = scope.GetRequiredService&lt;UbiquitousLanguageDbContext&gt;();
    ///
    /// 【スコープの重要性】
    /// Entity Framework CoreのDbContextはスコープドサービスです。
    /// テストごとに独立したスコープを作成することで、データの分離を保証します。
    /// </summary>
    public IServiceScope CreateScope()
    {
        return Services.CreateScope();
    }

    /// <summary>
    /// テスト用ApplicationUserデータシード（固定ID使用）
    ///
    /// 【DbInitializer固定IDとの整合性】
    /// DbInitializer.csで定義された固定IDを使用して、テストデータの一貫性を保証します。
    ///
    /// 【用途】
    /// - GetByIdentityIdAsync: 既存ユーザー検索テスト
    /// - DeleteByIdentityIdAsync: 論理削除テスト
    /// - UpdateByIdentityIdAsync: ユーザー更新テスト
    /// </summary>
    public static async Task SeedTestUserAsync(
        UbiquitousLanguageDbContext context,
        UserManager<ApplicationUser> userManager,
        string identityId,
        string name,
        string email,
        string roleName)
    {
        var user = new ApplicationUser
        {
            Id = identityId,
            UserName = email,
            NormalizedUserName = email.ToUpperInvariant(),
            Email = email,
            NormalizedEmail = email.ToUpperInvariant(),
            EmailConfirmed = true,
            Name = name,
            IsFirstLogin = false,
            IsDeleted = false,
            UpdatedAt = DateTime.UtcNow,
            UpdatedBy = identityId
        };

        // ASP.NET Core Identity UserManager経由でユーザー作成
        // 【重要】UserManager.CreateAsyncを使用することで、
        // Identity基盤のバリデーション・正規化処理が適用されます。
        var result = await userManager.CreateAsync(user, "TestPass123!");
        if (!result.Succeeded)
        {
            throw new InvalidOperationException(
                $"Failed to create test user {email}: {string.Join(", ", result.Errors.Select(e => e.Description))}");
        }

        // ロール割り当て
        await userManager.AddToRoleAsync(user, roleName);
    }

    /// <summary>
    /// テスト用ロールシード
    ///
    /// 【ロール管理の重要性】
    /// ASP.NET Core Identityでは、ロール情報はAspNetRolesテーブルで管理されます。
    /// UserRepository統合テストでは、ロール同期処理の検証も行うため、
    /// 事前に標準ロールを登録する必要があります。
    /// </summary>
    public static async Task SeedTestRolesAsync(RoleManager<IdentityRole> roleManager)
    {
        var roles = new[] { "SuperUser", "ProjectManager", "DomainApprover", "GeneralUser" };

        foreach (var roleName in roles)
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                await roleManager.CreateAsync(new IdentityRole(roleName));
            }
        }
    }
}
