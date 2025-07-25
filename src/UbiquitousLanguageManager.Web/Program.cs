using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using UbiquitousLanguageManager.Infrastructure.Data;
using UbiquitousLanguageManager.Infrastructure.Data.Entities;
using UbiquitousLanguageManager.Infrastructure.Services;
using UbiquitousLanguageManager.Web.Middleware;

/// <summary>
/// Programクラス - テストから参照可能にするためにpublicとして定義
/// 
/// 【初学者向け解説】
/// WebApplicationFactory&lt;T&gt;でテストサーバーを起動する際に、
/// このProgramクラスを型パラメータとして指定する必要があります。
/// </summary>
public partial class Program
{
    /// <summary>
    /// テスト用にMainメソッドを公開
    /// </summary>
    public static async Task Main(string[] args)
    {
        var app = await CreateApp(args);
        app.Run();
    }

    /// <summary>
    /// アプリケーション作成処理 - テストからも利用可能
    /// </summary>
    public static async Task<WebApplication> CreateApp(string[] args)
    {

var builder = WebApplication.CreateBuilder(args);

// 🔧 Blazor Server設定: サーバーサイドレンダリングとSignalR接続
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();

// 🔧 HTTP Context Accessor: Blazor ServerでHTTPコンテキストにアクセスするために必要
builder.Services.AddHttpContextAccessor();

// 🔐 Blazor認証設定
// 【Blazor Server初学者向け解説】
// AuthorizationCoreは、Blazorコンポーネントで[Authorize]属性を使用するために必要です。
// AuthenticationStateProviderは、認証状態をBlazor全体で管理するサービスです。
builder.Services.AddAuthorizationCore();
builder.Services.AddScoped<Microsoft.AspNetCore.Components.Authorization.AuthenticationStateProvider, 
    UbiquitousLanguageManager.Web.Authentication.CustomAuthenticationStateProvider>();

// 📊 データベース設定: PostgreSQL + Entity Framework Core
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<UbiquitousLanguageDbContext>(options =>
    options.UseNpgsql(connectionString));

// 🔧 DbContextFactory設定: マルチスレッド環境でのEF Core最適化
builder.Services.AddDbContextFactory<UbiquitousLanguageDbContext>(options =>
    options.UseNpgsql(connectionString));

// 🔐 認証・認可設定: ASP.NET Core Identity
// ApplicationUser を使用した Identity 統合により、
// 業務固有のユーザープロパティと認証機能を統合しています
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options => 
{
    // 🔑 パスワードポリシー設定
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequiredLength = 8;
    options.Password.RequiredUniqueChars = 1;

    // 🔒 ログイン設定（仕様書2.1.1準拠: ロックアウト機構は設けない）
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
    options.Lockout.MaxFailedAccessAttempts = 999; // 仕様書2.1.1準拠: 実質無制限
    options.Lockout.AllowedForNewUsers = false; // 仕様書2.1.1準拠: ロックアウト無効

    // 📧 ユーザー設定
    options.User.AllowedUserNameCharacters =
        "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
    options.User.RequireUniqueEmail = true;

    // 🔐 サインイン設定
    options.SignIn.RequireConfirmedAccount = false; // Phase A1では無効化
    options.SignIn.RequireConfirmedEmail = false;    // Phase A1では無効化
    options.SignIn.RequireConfirmedPhoneNumber = false;
})
.AddEntityFrameworkStores<UbiquitousLanguageDbContext>()
.AddDefaultTokenProviders(); // パスワードリセットトークン等の生成用

// 🍪 Cookie認証設定（仕様書2.1.1・10.1.1準拠）
// 【Blazor Server初学者向け解説】
// Blazor Server アプリケーションでは、Cookie ベースの認証が一般的です。
// SignalR 接続でも認証状態が維持されるため、セキュアな双方向通信が可能です。
builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest; // HTTPS環境ではSecure属性
    options.Cookie.SameSite = SameSiteMode.Lax; // CSRF攻撃対策
    
    // 仕様書10.1.1準拠: セッションタイムアウト2時間
    options.ExpireTimeSpan = TimeSpan.FromHours(2);
    
    // 仕様書2.1.1準拠: Remember Me機能（7日間有効期限）
    // isPersistent=trueの場合、ExpireTimeSpanが7日間に延長される
    
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.SlidingExpiration = true; // アクティブなユーザーは自動延長
    
    // Remember Me用の延長設定
    options.Events.OnSigningIn = context =>
    {
        // isPersistentがtrueの場合、7日間に延長
        if (context.Properties?.IsPersistent == true)
        {
            context.Properties.ExpiresUtc = DateTimeOffset.UtcNow.AddDays(7); // 仕様書2.1.1準拠: 7日間
        }
        return Task.CompletedTask;
    };
});

// 🎯 Clean Architecture: 依存関係注入設定
// Repository実装の登録
builder.Services.AddScoped<UbiquitousLanguageManager.Application.IUserRepository, UbiquitousLanguageManager.Infrastructure.Repositories.UserRepository>();
// 将来の拡張用（現在は実装なし）
// builder.Services.AddScoped<IProjectRepository, ProjectRepository>();
// builder.Services.AddScoped<IDomainRepository, DomainRepository>();
// builder.Services.AddScoped<IUbiquitousLanguageRepository, UbiquitousLanguageRepository>();

// Application Service実装の登録
builder.Services.AddScoped<UbiquitousLanguageManager.Application.UserApplicationService>();
// 将来の拡張用（現在は実装なし）
// builder.Services.AddScoped<UbiquitousLanguageApplicationService>();

// 🔧 初期データサービスの登録
builder.Services.AddScoped<InitialDataService>();

// 🔐 Web層認証サービスの登録
builder.Services.AddScoped<UbiquitousLanguageManager.Web.Services.AuthenticationService>();

// 📋 設定オブジェクトの登録
builder.Services.Configure<UbiquitousLanguageManager.Infrastructure.Services.InitialSuperUserSettings>(
    builder.Configuration.GetSection("InitialSuperUser"));

// 📧 メール送信設定（Phase A3 Step2）
builder.Services.Configure<UbiquitousLanguageManager.Infrastructure.Emailing.SmtpSettings>(
    builder.Configuration.GetSection("SmtpSettings"));
builder.Services.AddScoped<UbiquitousLanguageManager.Contracts.Interfaces.IEmailSender, 
    UbiquitousLanguageManager.Infrastructure.Emailing.SmtpEmailSender>();

// 🏥 ヘルスチェック設定: アプリケーション・データベースの正常性監視
builder.Services.AddHealthChecks()
    .AddDbContextCheck<UbiquitousLanguageDbContext>(
        name: "database",
        failureStatus: Microsoft.Extensions.Diagnostics.HealthChecks.HealthStatus.Degraded,
        tags: new[] { "ready", "db" })
    .AddCheck("liveness", () => 
        Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Healthy("Application is alive"),
        tags: new[] { "live" });

var app = builder.Build();

// 🌍 開発環境設定: エラーページとHTTPS設定
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts(); // 🔒 HTTP Strict Transport Security
}

// 🚨 グローバル例外ハンドリング: 全体的な例外処理
app.UseGlobalExceptionHandling();

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// 🔐 認証・認可ミドルウェア設定
app.UseAuthentication();
app.UseAuthorization();

// 🎯 Blazor Server設定: ルーティング
app.MapRazorPages();
app.MapBlazorHub(); // 🌐 SignalR Hubマッピング（Blazor Serverの双方向通信）

// 🎯 MVC設定: 認証ページ用ルーティング
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapFallbackToPage("/_Host");

// 🏥 ヘルスチェックエンドポイント: 監視・運用のためのエンドポイント
app.MapHealthChecks("/health", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    ResponseWriter = async (context, report) =>
    {
        // JSON形式のレスポンス
        context.Response.ContentType = "application/json";
        var result = System.Text.Json.JsonSerializer.Serialize(new
        {
            status = report.Status.ToString(),
            checks = report.Entries.Select(entry => new
            {
                name = entry.Key,
                status = entry.Value.Status.ToString(),
                description = entry.Value.Description,
                duration = entry.Value.Duration.ToString()
            }),
            totalDuration = report.TotalDuration.ToString()
        });
        await context.Response.WriteAsync(result);
    }
});

// 🏥 詳細ヘルスチェック: データベース接続確認
app.MapHealthChecks("/health/ready", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("ready")
});

// 🏥 軽量ヘルスチェック: アプリケーション生存確認
app.MapHealthChecks("/health/live", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("live")
});

// 🔧 初期データ投入: アプリケーション起動時の自動実行
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        // 📊 データベース自動マイグレーション実行
        var context = services.GetRequiredService<UbiquitousLanguageDbContext>();
        await context.Database.EnsureCreatedAsync();
        
        // 👤 初期スーパーユーザー作成処理
        var initialDataService = services.GetRequiredService<InitialDataService>();
        await initialDataService.SeedInitialDataAsync();
        
        app.Logger.LogInformation("✅ 初期データ投入が完了しました");
    }
    catch (Exception ex)
    {
        app.Logger.LogError(ex, "❌ 初期データ投入中にエラーが発生しました: {Message}", ex.Message);
        throw; // 🚨 初期化失敗時はアプリケーション起動を停止
    }
}

        return app;
    }
}

