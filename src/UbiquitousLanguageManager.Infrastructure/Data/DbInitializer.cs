using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using UbiquitousLanguageManager.Infrastructure.Data.Entities;
using DomainEntity = UbiquitousLanguageManager.Infrastructure.Data.Entities.Domain;

namespace UbiquitousLanguageManager.Infrastructure.Data;

/// <summary>
/// データベース初期化クラス
/// 開発・テスト環境用の初期データ投入を担当
/// </summary>
/// <remarks>
/// バックアップSQL（init/backup/02_initial_data.sql）の内容をC#で再現
/// - ASP.NET Core Identity対応（PasswordHasher使用）
/// - 全ユーザーの初期パスワード: "su"
/// - IsFirstLogin: true（全ユーザー初回ログイン前）
/// </remarks>
public class DbInitializer
{
    private readonly UbiquitousLanguageDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly ILogger<DbInitializer> _logger;
    private readonly IPasswordHasher<ApplicationUser> _passwordHasher;

    // 初期データ投入で使用する管理者ユーザーID
    private const string AdminUserId = "admin-001";
    private const string ProjectManagerUserId = "pm-001";
    private const string DomainApproverUserId = "da-001";
    private const string GeneralUserId = "gu-001";
    private const string E2eTestUserId = "e2e-test@ubiquitous-lang.local";

    /// <summary>
    /// コンストラクタ: 依存関係の注入
    /// </summary>
    /// <param name="context">Entity Framework Core DbContext</param>
    /// <param name="userManager">ASP.NET Core Identity ユーザーマネージャー</param>
    /// <param name="roleManager">ASP.NET Core Identity ロールマネージャー</param>
    /// <param name="logger">ログ出力サービス</param>
    /// <param name="passwordHasher">パスワードハッシュ化サービス</param>
    public DbInitializer(
        UbiquitousLanguageDbContext context,
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager,
        ILogger<DbInitializer> logger,
        IPasswordHasher<ApplicationUser> passwordHasher)
    {
        _context = context;
        _userManager = userManager;
        _roleManager = roleManager;
        _logger = logger;
        _passwordHasher = passwordHasher;
    }

    /// <summary>
    /// データベース初期化（マイグレーション適用 + 初期データ投入）
    /// </summary>
    public async Task InitializeAsync()
    {
        try
        {
            _logger.LogInformation("データベース初期化を開始します");

            // マイグレーション適用
            await _context.Database.MigrateAsync();
            _logger.LogInformation("マイグレーション適用完了");

            // 初期データ投入
            await SeedDataAsync();
            _logger.LogInformation("データベース初期化が完了しました");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "データベース初期化中にエラーが発生しました");
            throw;
        }
    }

    /// <summary>
    /// 初期データ投入処理
    /// バックアップSQL（init/backup/02_initial_data.sql）の内容を再現
    /// </summary>
    /// <remarks>
    /// Phase B2実装方針: InitialDataServiceと役割分担
    /// - DbInitializer: 開発環境専用の全データ投入（ユーザー・ロール・プロジェクト・ドメイン・UserProjects）
    /// - InitialDataService: 本番環境用のスーパーユーザー作成のみ
    ///
    /// 既存データチェック: 各データタイプごとに個別チェック（細分化方式）
    /// - ロール、ユーザー、プロジェクト、ドメイン、UserProjectsをそれぞれ個別に確認
    /// - 既存データがある場合は該当部分のみスキップし、他のデータ投入は継続
    /// </remarks>
    private async Task SeedDataAsync()
    {
        _logger.LogInformation("初期データ投入を開始します");

        // 1. ロール作成（個別チェック）
        if (!await _roleManager.Roles.AnyAsync())
        {
            await SeedRolesAsync();
        }
        else
        {
            _logger.LogInformation("ロールが既に存在するため、ロール投入をスキップします");
        }

        // 2. ユーザー作成（個別チェック）
        if (!await _userManager.Users.AnyAsync())
        {
            await SeedUsersAsync();
        }
        else
        {
            _logger.LogInformation("ユーザーが既に存在するため、ユーザー投入をスキップします");
        }

        // 3. プロジェクト作成（個別チェック）
        if (!await _context.Projects.AnyAsync())
        {
            await SeedProjectsAsync();
        }
        else
        {
            _logger.LogInformation("プロジェクトが既に存在するため、プロジェクト投入をスキップします");
        }

        // 4. ドメイン作成（個別チェック）
        if (!await _context.Domains.AnyAsync())
        {
            await SeedDomainsAsync();
        }
        else
        {
            _logger.LogInformation("ドメインが既に存在するため、ドメイン投入をスキップします");
        }

        // 5. UserProjects関連設定（個別チェック）
        if (!await _context.UserProjects.AnyAsync())
        {
            await SeedUserProjectsAsync();
        }
        else
        {
            _logger.LogInformation("UserProjectsが既に存在するため、UserProjects投入をスキップします");
        }

        // 6. DomainApprovers設定
        await SeedDomainApproversAsync();

        // 7. E2Eテストドラフト用語作成（個別チェック）
        // Note: グローバルクエリフィルタを無視して全データ確認
        if (!await _context.DraftUbiquitousLanguages.IgnoreQueryFilters().AnyAsync(d => d.JapaneseName == "テスト用語"))
        {
            await SeedDraftUbiquitousLanguagesAsync();
        }
        else
        {
            _logger.LogInformation("E2Eテストドラフト用語が既に存在するため、投入をスキップします");
        }

        await _context.SaveChangesAsync();
        _logger.LogInformation("初期データ投入が完了しました");
        LogInitialDataSummary();
    }

    /// <summary>
    /// ロール初期データ投入（4件）
    /// バックアップSQL準拠: super-user, project-manager, domain-approver, general-user
    /// </summary>
    private async Task SeedRolesAsync()
    {
        var roles = new[]
        {
            new { Id = "super-user", Name = "super-user", NormalizedName = "SUPER-USER" },
            new { Id = "project-manager", Name = "project-manager", NormalizedName = "PROJECT-MANAGER" },
            new { Id = "domain-approver", Name = "domain-approver", NormalizedName = "DOMAIN-APPROVER" },
            new { Id = "general-user", Name = "general-user", NormalizedName = "GENERAL-USER" }
        };

        foreach (var role in roles)
        {
            // 既存ロール確認
            if (await _roleManager.RoleExistsAsync(role.Name))
            {
                _logger.LogInformation("ロール既存: {RoleName}", role.Name);
                continue;
            }

            var identityRole = new IdentityRole
            {
                Id = role.Id,
                Name = role.Name,
                NormalizedName = role.NormalizedName
            };

            var result = await _roleManager.CreateAsync(identityRole);
            if (result.Succeeded)
            {
                _logger.LogInformation("ロール作成: {RoleName}", role.Name);
            }
            else
            {
                _logger.LogError("ロール作成失敗: {RoleName}, Errors: {Errors}",
                    role.Name, string.Join(", ", result.Errors.Select(e => e.Description)));
            }
        }
    }

    /// <summary>
    /// ユーザー初期データ投入（5件）
    /// 全ユーザーの初期パスワード: "su"
    /// E2Eテストユーザー: "E2ETest#2025!Secure"
    /// </summary>
    private async Task SeedUsersAsync()
    {
        var users = new[]
        {
            new { Id = "admin-001", Email = "admin@ubiquitous-lang.com", Name = "システム管理者", Role = "super-user", Password = "su", IsFirstLogin = true },
            new { Id = "pm-001", Email = "project.manager@ubiquitous-lang.com", Name = "プロジェクト管理者", Role = "project-manager", Password = "su", IsFirstLogin = true },
            new { Id = "da-001", Email = "domain.approver@ubiquitous-lang.com", Name = "ドメイン承認者", Role = "domain-approver", Password = "su", IsFirstLogin = true },
            new { Id = "gu-001", Email = "general.user@ubiquitous-lang.com", Name = "一般ユーザー", Role = "general-user", Password = "su", IsFirstLogin = true },
            new { Id = E2eTestUserId, Email = E2eTestUserId, Name = "E2Eテストユーザー", Role = "super-user", Password = "E2ETest#2025!Secure", IsFirstLogin = false }
        };

        foreach (var userData in users)
        {
            var user = new ApplicationUser
            {
                Id = userData.Id,
                UserName = userData.Email,
                Email = userData.Email,
                EmailConfirmed = true,
                Name = userData.Name,
                IsFirstLogin = userData.IsFirstLogin, // E2Eテストユーザーは初回ログイン済み
                InitialPassword = userData.Password, // 機能仕様書2.2.1準拠：平文管理
                UpdatedBy = "admin-001",
                UpdatedAt = DateTime.UtcNow
            };

            // ASP.NET Core Identity PasswordHasher でハッシュ化
            user.PasswordHash = _passwordHasher.HashPassword(user, userData.Password);

            await _userManager.CreateAsync(user);
            await _userManager.AddToRoleAsync(user, userData.Role);

            _logger.LogInformation("ユーザー作成: {Email} (Role: {Role}, IsFirstLogin: {IsFirstLogin})", user.Email, userData.Role, userData.IsFirstLogin);
        }
    }

    /// <summary>
    /// プロジェクト初期データ投入（3件）
    /// バックアップSQL準拠: ECサイト構築プロジェクト、顧客管理システム、E2Eテストプロジェクト
    /// </summary>
    /// <remarks>
    /// Phase B2暫定仕様: OwnerIdは仮値（1）を設定
    /// 理由: ApplicationUser.UserId（long型）プロパティが未実装のため
    /// TODO: 後続Phaseでユーザー管理機能実装時にOwnerIdマッピング機能を追加
    /// </remarks>
    private async Task SeedProjectsAsync()
    {
        const long temporaryOwnerId = 1; // Phase B2暫定仕様: 仮のOwnerID

        var projects = new[]
        {
            new Project
            {
                ProjectName = "ECサイト構築プロジェクト",
                Description = "オンライン販売システムの構築プロジェクト",
                OwnerId = temporaryOwnerId,
                CreatedAt = DateTime.UtcNow,
                UpdatedBy = AdminUserId,
                UpdatedAt = DateTime.UtcNow,
                IsActive = true
            },
            new Project
            {
                ProjectName = "顧客管理システム",
                Description = "CRM機能を持つ顧客管理システムの開発",
                OwnerId = temporaryOwnerId,
                CreatedAt = DateTime.UtcNow,
                UpdatedBy = AdminUserId,
                UpdatedAt = DateTime.UtcNow,
                IsActive = true
            },
            new Project
            {
                ProjectName = "E2Eテストプロジェクト",
                Description = "Playwright E2Eテスト専用プロジェクト",
                OwnerId = temporaryOwnerId,
                CreatedAt = DateTime.UtcNow,
                UpdatedBy = E2eTestUserId,
                UpdatedAt = DateTime.UtcNow,
                IsActive = true
            }
        };

        await _context.Projects.AddRangeAsync(projects);
        _logger.LogInformation("プロジェクト初期データ投入完了: {Count}件（OwnerId暫定値={OwnerId}）", projects.Length, temporaryOwnerId);
    }

    /// <summary>
    /// ドメイン初期データ投入（4件）
    /// バックアップSQL準拠: 商品管理、注文管理、顧客情報管理、E2Eテストドメイン
    /// </summary>
    /// <remarks>
    /// Phase B2暫定仕様: OwnerIdは仮値（1）を設定
    /// 理由: ApplicationUser.UserId（long型）プロパティが未実装のため
    /// TODO: 後続Phaseでユーザー管理機能実装時にOwnerIdマッピング機能を追加
    /// </remarks>
    private async Task SeedDomainsAsync()
    {
        const long temporaryOwnerId = 1; // Phase B2暫定仕様: 仮のOwnerID

        // プロジェクトIDを取得（バックアップSQL: ProjectId 1, 2, 3に対応）
        var projects = await _context.Projects
            .OrderBy(p => p.ProjectId)
            .ToListAsync();

        if (projects.Count < 3)
        {
            _logger.LogError("プロジェクトが3件未満です。E2Eテストドメイン投入をスキップします。");
            return;
        }

        var domains = new[]
        {
            new DomainEntity
            {
                ProjectId = projects[0].ProjectId, // ECサイト構築プロジェクト
                DomainName = "商品管理",
                Description = "商品カタログ、在庫管理、価格設定に関するドメイン",
                OwnerId = temporaryOwnerId,
                IsDefault = false,
                CreatedAt = DateTime.UtcNow,
                UpdatedBy = AdminUserId,
                UpdatedAt = DateTime.UtcNow,
                IsActive = true
            },
            new DomainEntity
            {
                ProjectId = projects[0].ProjectId, // ECサイト構築プロジェクト
                DomainName = "注文管理",
                Description = "注文処理、決済、配送に関するドメイン",
                OwnerId = temporaryOwnerId,
                IsDefault = false,
                CreatedAt = DateTime.UtcNow,
                UpdatedBy = AdminUserId,
                UpdatedAt = DateTime.UtcNow,
                IsActive = true
            },
            new DomainEntity
            {
                ProjectId = projects[1].ProjectId, // 顧客管理システム
                DomainName = "顧客情報管理",
                Description = "顧客の基本情報、連絡先、履歴管理に関するドメイン",
                OwnerId = temporaryOwnerId,
                IsDefault = false,
                CreatedAt = DateTime.UtcNow,
                UpdatedBy = AdminUserId,
                UpdatedAt = DateTime.UtcNow,
                IsActive = true
            },
            new DomainEntity
            {
                ProjectId = projects[2].ProjectId, // E2Eテストプロジェクト
                DomainName = "E2Eテストドメイン",
                Description = "Playwright E2Eテスト専用ドメイン",
                OwnerId = temporaryOwnerId,
                IsDefault = false,
                CreatedAt = DateTime.UtcNow,
                UpdatedBy = E2eTestUserId,
                UpdatedAt = DateTime.UtcNow,
                IsActive = true
            }
        };

        await _context.Domains.AddRangeAsync(domains);
        _logger.LogInformation("ドメイン初期データ投入完了: {Count}件（OwnerId暫定値={OwnerId}）", domains.Length, temporaryOwnerId);
    }

    /// <summary>
    /// UserProjects関連設定
    /// pm-001, da-001, gu-001をプロジェクト1, 2に割り当て
    /// e2e-test@ubiquitous-lang.localをE2Eテストプロジェクトに割り当て
    /// </summary>
    private async Task SeedUserProjectsAsync()
    {
        // プロジェクトIDを取得
        var projects = await _context.Projects
            .OrderBy(p => p.ProjectId)
            .ToListAsync();

        if (projects.Count < 3)
        {
            _logger.LogError("プロジェクトが3件未満です。E2EテストUserProjects投入をスキップします。");
            return;
        }

        var userProjects = new List<UserProject>();

        // プロジェクト管理者をプロジェクト1, 2に割り当て
        userProjects.Add(new UserProject
        {
            UserId = ProjectManagerUserId,
            ProjectId = projects[0].ProjectId,
            UpdatedBy = AdminUserId,
            UpdatedAt = DateTime.UtcNow
        });
        userProjects.Add(new UserProject
        {
            UserId = ProjectManagerUserId,
            ProjectId = projects[1].ProjectId,
            UpdatedBy = AdminUserId,
            UpdatedAt = DateTime.UtcNow
        });

        // ドメイン承認者をプロジェクト1, 2に割り当て
        userProjects.Add(new UserProject
        {
            UserId = DomainApproverUserId,
            ProjectId = projects[0].ProjectId,
            UpdatedBy = AdminUserId,
            UpdatedAt = DateTime.UtcNow
        });
        userProjects.Add(new UserProject
        {
            UserId = DomainApproverUserId,
            ProjectId = projects[1].ProjectId,
            UpdatedBy = AdminUserId,
            UpdatedAt = DateTime.UtcNow
        });

        // 一般ユーザーをプロジェクト1, 2に割り当て
        userProjects.Add(new UserProject
        {
            UserId = GeneralUserId,
            ProjectId = projects[0].ProjectId,
            UpdatedBy = AdminUserId,
            UpdatedAt = DateTime.UtcNow
        });
        userProjects.Add(new UserProject
        {
            UserId = GeneralUserId,
            ProjectId = projects[1].ProjectId,
            UpdatedBy = AdminUserId,
            UpdatedAt = DateTime.UtcNow
        });

        // E2EテストユーザーをE2Eテストプロジェクトに割り当て
        userProjects.Add(new UserProject
        {
            UserId = E2eTestUserId,
            ProjectId = projects[2].ProjectId,
            UpdatedBy = E2eTestUserId,
            UpdatedAt = DateTime.UtcNow
        });

        await _context.UserProjects.AddRangeAsync(userProjects);
        _logger.LogInformation("UserProjects関連設定完了: {Count}件", userProjects.Count);
    }

    /// <summary>
    /// DomainApprovers設定
    /// da-001をドメイン1, 2, 3に割り当て
    /// </summary>
    private Task SeedDomainApproversAsync()
    {
        // TODO: DomainApproverエンティティ定義後に実装
        _logger.LogInformation("DomainApprovers設定（後続Stepで実装予定）");
        return Task.CompletedTask;
    }

    /// <summary>
    /// E2Eテストドラフトユビキタス言語作成
    /// Playwright E2Eテスト専用のサンプルドラフト用語を作成
    /// </summary>
    /// <remarks>
    /// IgnoreQueryFilters使用: グローバルクエリフィルタ（IsDeleted）を無視してドメイン検索
    /// 理由: 初期データ投入時はIsDeletedフラグがfalseのデータのみ対象だが、明示的に全データを検索するため
    /// </remarks>
    private async Task SeedDraftUbiquitousLanguagesAsync()
    {
        // E2Eテストドメインを取得（グローバルクエリフィルタを無視）
        var e2eTestDomain = await _context.Domains
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(d => d.DomainName == "E2Eテストドメイン");

        if (e2eTestDomain == null)
        {
            _logger.LogError("E2Eテストドメインが見つかりません。ドラフト用語投入をスキップします。");
            return;
        }

        var draftTerms = new[]
        {
            new DraftUbiquitousLang
            {
                DomainId = e2eTestDomain.DomainId,
                JapaneseName = "テスト用語",
                EnglishName = "TestTerm",
                Description = "E2Eテスト用のサンプル用語",
                OccurrenceContext = "テストシナリオ実行時",
                Remarks = "Playwright E2Eテスト専用データ",
                Status = "Draft",
                UpdatedBy = E2eTestUserId,
                UpdatedAt = DateTime.UtcNow
            }
        };

        await _context.DraftUbiquitousLanguages.AddRangeAsync(draftTerms);
        _logger.LogInformation("E2Eテストドラフト用語投入完了: {Count}件", draftTerms.Length);
    }

    /// <summary>
    /// 初期データ投入結果サマリーをログ出力
    /// </summary>
    private void LogInitialDataSummary()
    {
        _logger.LogInformation("===== 初期データ投入完了サマリー =====");
        _logger.LogInformation("作成ユーザー数: 5（開発用4件 + E2Eテスト用1件）");
        _logger.LogInformation("作成ロール数: 4");
        _logger.LogInformation("作成プロジェクト数: 3（開発用2件 + E2Eテスト用1件）");
        _logger.LogInformation("作成ドメイン数: 4（開発用3件 + E2Eテスト用1件）");
        _logger.LogInformation("UserProjects関連設定: 7件（開発用6件 + E2Eテスト用1件）");
        _logger.LogInformation("DomainApprovers設定: 後続Stepで実装予定");
        _logger.LogInformation("E2Eテストドラフト用語: 1件");
        _logger.LogInformation("デフォルトパスワード: su（機能仕様書2.0.1準拠）");
        _logger.LogInformation("E2Eテストパスワード: E2ETest#2025!Secure（IsFirstLogin=false）");
        _logger.LogInformation("認証システム: ASP.NET Core Identity");
        _logger.LogInformation("用語統一: ADR_003準拠（UbiquitousLang表記）");
        _logger.LogInformation("初期パスワード管理: US-005準拠（InitialPassword保存）");
        _logger.LogInformation("=====================================");
    }
}
