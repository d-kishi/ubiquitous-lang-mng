using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using UbiquitousLanguageManager.Infrastructure.Data;
using UbiquitousLanguageManager.Infrastructure.Services;
using UbiquitousLanguageManager.Web.Middleware;

var builder = WebApplication.CreateBuilder(args);

// 🔧 Blazor Server設定: サーバーサイドレンダリングとSignalR接続
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();

// 📊 データベース設定: PostgreSQL + Entity Framework Core
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<UbiquitousLanguageDbContext>(options =>
    options.UseNpgsql(connectionString));

// 🔧 DbContextFactory設定: マルチスレッド環境でのEF Core最適化
builder.Services.AddDbContextFactory<UbiquitousLanguageDbContext>(options =>
    options.UseNpgsql(connectionString));

// 🔐 認証・認可設定: ASP.NET Core Identity
builder.Services.AddIdentity<IdentityUser, IdentityRole>(options => 
{
    // 🔑 パスワードポリシー設定
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequiredLength = 8;
    options.Password.RequiredUniqueChars = 1;

    // 🔒 ログイン設定
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = true;

    // 📧 ユーザー設定
    options.User.AllowedUserNameCharacters =
        "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
    options.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<UbiquitousLanguageDbContext>();

// 🎯 Clean Architecture: 依存関係注入設定
// Repository実装の登録
builder.Services.AddScoped<UbiquitousLanguageManager.Application.IUserRepository, UbiquitousLanguageManager.Infrastructure.Repositories.UserRepository>();
// 将来の拡張用（現在は実装なし）
// builder.Services.AddScoped<IProjectRepository, ProjectRepository>();
// builder.Services.AddScoped<IDomainRepository, DomainRepository>();
// builder.Services.AddScoped<IUbiquitousLanguageRepository, UbiquitousLanguageRepository>();

// Application Service実装の登録
// 将来の拡張用（現在は実装なし）
// builder.Services.AddScoped<UserApplicationService>();
// builder.Services.AddScoped<UbiquitousLanguageApplicationService>();

// 🔧 初期データサービスの登録
builder.Services.AddScoped<InitialDataService>();

// 📋 設定オブジェクトの登録
builder.Services.Configure<InitialSuperUserSettings>(
    builder.Configuration.GetSection("InitialSuperUser"));

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

app.Run();

/// <summary>
/// 初期スーパーユーザー設定クラス
/// appsettings.jsonの"InitialSuperUser"セクションにバインド
/// </summary>
public class InitialSuperUserSettings
{
    /// <summary>
    /// 初期スーパーユーザーのメールアドレス
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// 初期スーパーユーザーの名前
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 初期パスワード（"su"固定）
    /// </summary>
    public string Password { get; set; } = string.Empty;

    /// <summary>
    /// 初回ログインフラグ（パスワード変更必須）
    /// </summary>
    public bool IsFirstLogin { get; set; } = true;
}