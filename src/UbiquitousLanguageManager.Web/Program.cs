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

        // 🎯 ログ設定強化（ADR_017準拠）
        // 【ログ管理実装戦略】
        // Microsoft.Extensions.Logging基盤活用・環境別最適化・構造化ログ準備
        builder.Logging.ClearProviders();
        builder.Logging.AddConsole();

        // 開発環境でのログ詳細化・本番環境での適切なレベル管理
        if (builder.Environment.IsDevelopment())
        {
            builder.Logging.AddDebug();
            // Entity Framework Core詳細ログを開発環境でのみ有効化
            builder.Logging.SetMinimumLevel(LogLevel.Debug);
        }
        else
        {
            // 本番環境ではInformation以上のみ出力（パフォーマンス最適化）
            builder.Logging.SetMinimumLevel(LogLevel.Information);
        }

        // 🔧 Blazor Server設定: サーバーサイドレンダリングとSignalR接続
        builder.Services.AddRazorPages();
        builder.Services.AddServerSideBlazor(options =>
        {
            // 【TECH-006修正】Blazor ServerでのSignalR設定最適化
            // DisconnectedCircuitRetentionPeriod: 切断されたサーキットの保持期間
            options.DisconnectedCircuitRetentionPeriod = TimeSpan.FromMinutes(3);

            // DisconnectedCircuitMaxRetained: 切断されたサーキットの最大保持数
            options.DisconnectedCircuitMaxRetained = 100;

            // JSInteropDefaultCallTimeout: JavaScript相互運用のデフォルトタイムアウト
            options.JSInteropDefaultCallTimeout = TimeSpan.FromMinutes(1);
        });

        // 🔧 【TECH-006修正】API Controller設定: Headers read-onlyエラー解決
        // 【HTTPコンテキスト分離戦略】
        // API Controllerは独立したHTTPコンテキストで動作し、
        // Blazor SignalR接続とは分離された認証処理を提供します。
        builder.Services.AddControllers(options =>
        {
            // CSRF保護: ValidateAntiForgeryToken自動適用
            options.Filters.Add(new Microsoft.AspNetCore.Mvc.AutoValidateAntiforgeryTokenAttribute());
        })
        .ConfigureApiBehaviorOptions(options =>
        {
            // モデルバリデーションエラー時の自動レスポンス無効化（カスタムエラーハンドリング優先）
            options.SuppressModelStateInvalidFilter = true;
        });

        // 🎯 JSON設定全体共通化（技術負債予防・DRY原則準拠）
        // 【JavaScript ↔ C# 統合標準化】
        // PropertyNameCaseInsensitive: JavaScript {success: true} ↔ C# {Success: true} 統一
        // PropertyNamingPolicy.CamelCase: JSON出力時のcamelCase統一
        builder.Services.ConfigureHttpJsonOptions(options =>
        {
            options.SerializerOptions.PropertyNameCaseInsensitive = true;
            options.SerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
        });

        // 🎯 Blazor Server用共通JSONサービス（技術負債予防・DRY原則準拠）
        // 【Blazor Component内JsonSerializer一括管理】
        // ConfigureHttpJsonOptionsはWeb API専用のため、Blazor Component内での
        // 直接JsonSerializer使用には適用されない。共通サービスで統一設定を提供。
        // 
        // 【Blazor Server初学者向け解説】
        // このサービスにより、全Blazor Componentで統一されたJSON処理設定が適用され、
        // 設定の重複・不整合を防止し、保守性を向上させます。
        builder.Services.AddScoped<UbiquitousLanguageManager.Web.Services.IJsonSerializerService, UbiquitousLanguageManager.Web.Services.JsonSerializerService>();

        // Antiforgery設定: API呼び出しでのCSRF保護
        builder.Services.AddAntiforgery(options =>
        {
            options.HeaderName = "X-CSRF-TOKEN";
            options.Cookie.Name = "__RequestVerificationToken";
            options.Cookie.HttpOnly = true;
            options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
            options.Cookie.SameSite = SameSiteMode.Strict;
        });

        // 🔧 HTTP Context Accessor: Blazor ServerでHTTPコンテキストにアクセスするために必要
        builder.Services.AddHttpContextAccessor();

        // 🔐 Blazor認証設定
        // 【Blazor Server初学者向け解説】
        // AuthorizationCoreは、Blazorコンポーネントで[Authorize]属性を使用するために必要です。
        // AuthenticationStateProviderは、認証状態をBlazor全体で管理するサービスです。
        builder.Services.AddAuthorizationCore();

        // CustomAuthenticationStateProviderを具体型としても登録（Web層AuthenticationServiceの依存関係解決用）
        builder.Services.AddScoped<UbiquitousLanguageManager.Web.Authentication.CustomAuthenticationStateProvider>();
        // さらに、AuthenticationStateProviderとしても登録（Blazor認証システム用）
        builder.Services.AddScoped<Microsoft.AspNetCore.Components.Authorization.AuthenticationStateProvider>(provider =>
            provider.GetRequiredService<UbiquitousLanguageManager.Web.Authentication.CustomAuthenticationStateProvider>());

        // 📊 データベース設定: PostgreSQL + Entity Framework Core
        var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

        // 🔧 DbContextFactory設定: マルチスレッド環境でのEF Core最適化
        // 【Blazor Server初学者向け解説】
        // Blazor Serverはマルチスレッド環境で動作するため、DbContextの同時実行を防ぐために
        // DbContextFactoryを使用します。これにより、各操作ごとに新しいDbContextインスタンスが作成されます。
        // 
        // 設計書推奨: AddDbContextFactoryのみ使用（AddDbContextは内部で自動登録される）
        builder.Services.AddDbContextFactory<UbiquitousLanguageDbContext>(options =>
            options.UseNpgsql(connectionString));

        // 🔐 認証・認可設定: ASP.NET Core Identity（Phase A5標準Identity移行）
        // 標準IdentityUser を使用したIdentity統合
        // Phase A5でカスタムApplicationUserから標準Identity実装に移行
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

            options.LoginPath = "/login";
            options.LogoutPath = "/logout";
            options.AccessDeniedPath = "/access-denied";
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
        // Phase A9: UserRepositoryAdapterに更新（ASP.NET Core Identity統合）
        builder.Services.AddScoped<UbiquitousLanguageManager.Application.IUserRepository, UbiquitousLanguageManager.Infrastructure.Repositories.UserRepositoryAdapter>();
        // 将来の拡張用（現在は実装なし）
        // builder.Services.AddScoped<IProjectRepository, ProjectRepository>();
        // builder.Services.AddScoped<IDomainRepository, DomainRepository>();
        // builder.Services.AddScoped<IUbiquitousLanguageRepository, UbiquitousLanguageRepository>();

        // 🔐 Application層の認証サービス実装の登録（Phase A4 Step2で追加）
        // 【F#初学者向け解説】
        // F# Application層で定義されたIAuthenticationServiceインターフェースを
        // C# Infrastructure層のAuthenticationServiceクラスで実装し、DIコンテナに登録します。
        // これにより、F#のUserApplicationServiceがC#の実装を利用できるようになります。
        // 🎯 Phase A9: Infrastructure層認証サービス実装（循環依存回避版）
        // Infrastructure.Services.AuthenticationServiceを軽量化し、F# AuthenticationApplicationServiceへの依存を削除
        // これにより循環依存を解決し、Clean Architecture の依存関係を適正化
        builder.Services.AddScoped<UbiquitousLanguageManager.Application.IAuthenticationService, UbiquitousLanguageManager.Infrastructure.Services.AuthenticationService>();

        // 🎯 F# AuthenticationApplicationService: 高次ビジネスロジック層として独立運用
        // F# Domain層活用率80%達成のため、AuthenticationApplicationServiceを独立サービスとして登録
        // IAuthenticationServiceを利用してビジネスロジック層の処理を担当
        builder.Services.AddScoped<UbiquitousLanguageManager.Application.AuthenticationApplicationService>();

        // 🔧 AuthApiController用の具象クラス登録
        // API層の実用性確保（DTOオーバーロード活用）のため、具象クラスも併せて登録
        // F#統合には影響なし（IAuthenticationService登録は維持）
        builder.Services.AddScoped<UbiquitousLanguageManager.Infrastructure.Services.AuthenticationService>();

        // 🔧 F# AuthenticationApplicationService用ロガー登録は不要
        // 【F#初学者向け解説】
        // ASP.NET CoreのDIコンテナは、ILogger<T>を自動的に解決するため、
        // 明示的な登録は不要です。F#のクラスでもC#と同様に使用できます。

        // 🔐 Phase A9: Infrastructure層AuthenticationServiceの具象クラス登録削除
        // 修正理由: BlazorAuthenticationServiceがIAuthenticationServiceインターフェース依存に変更されたため、
        // 重複するDI登録を削除。AuthApiControllerは引き続き201行目のインターフェース登録経由で解決される。
        // これにより、Clean Architecture依存関係の整理とDI設定の一貫性を確保。

        // 📧 Application層の通知サービス実装の登録（Phase A4 Step2で追加）
        builder.Services.AddScoped<UbiquitousLanguageManager.Application.INotificationService, UbiquitousLanguageManager.Infrastructure.Services.NotificationService>();

        // 📊 Application層のロガーアダプター登録（Phase A4 Step2で追加）
        // F#のILogger<T>インターフェースをMicrosoft.Extensions.LoggingのILogger<T>にアダプト
        builder.Services.AddScoped(typeof(UbiquitousLanguageManager.Application.ILogger<>), typeof(UbiquitousLanguageManager.Infrastructure.Services.FSharpLoggerAdapter<>));

        // Application Service実装の登録
        builder.Services.AddScoped<UbiquitousLanguageManager.Application.UserApplicationService>();
        
        // 🚀 Phase A9: F# 認証Application層サービスの登録
        // 【F#初学者向け解説】
        // Step 1-1で実装されたF#のAuthenticationApplicationServiceを登録します。
        // これにより、Railway-oriented Programmingによる型安全な認証処理が利用可能になります。
        builder.Services.AddScoped<UbiquitousLanguageManager.Application.AuthenticationApplicationService>();
        // 将来の拡張用（現在は実装なし）
        // builder.Services.AddScoped<UbiquitousLanguageApplicationService>();

        // 🔧 初期データサービスの登録
        builder.Services.AddScoped<InitialDataService>();

        // 🔐 Blazor認証サービスの登録（Phase A9 統一認証効果: Infrastructure層委譲・薄いラッパー層）
        builder.Services.AddScoped<UbiquitousLanguageManager.Web.Services.BlazorAuthenticationService>();

        // 🔑 パスワードリセットサービスの登録（Phase A3）
        builder.Services.AddScoped<UbiquitousLanguageManager.Contracts.Interfaces.IPasswordResetService,
            UbiquitousLanguageManager.Infrastructure.Services.PasswordResetService>();

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

        // 🔒 初回ログインアクセス制限ミドルウェア（TECH-004対応）
        // 【セキュリティ強化】
        // 初回ログイン状態（IsFirstLogin=true）のユーザーを対象に、
        // パスワード変更画面以外へのアクセスを制限し、セキュリティを強化します。
        // 
        // 【ミドルウェア配置理由】
        // UseAuthentication()の後: 認証済みユーザーの情報が取得可能
        // UseAuthorization()の前: 認可処理前にアクセス制限を適用
        app.UseFirstLoginRedirect();

        app.UseAuthorization();

        // 🔧 【TECH-006修正】API Controller ルーティング設定
        // 【HTTPコンテキスト分離戦略】
        // API Controllerエンドポイントを優先的にマッピングし、
        // Blazor Serverのフォールバック処理より先に処理されるようにします。
        app.MapControllers();

        // 🔒 CSRF トークン取得エンドポイント: AuthApiController.csで統一管理
        // 【重複削除】以前はMinimal APIで実装していましたが、
        // AmbiguousMatchException回避のため、Controller統一アーキテクチャに変更。
        // CSRFトークン取得は /Controllers/AuthApiController.cs の GetCsrfToken() で提供。

        // 🎯 Blazor Server設定: ルーティング
        app.MapRazorPages();
        app.MapBlazorHub(options =>
        {
            // 【TECH-006修正】SignalR Hubの設定最適化
            // Blazor ServerとASP.NET Core Identityの競合を軽減
            options.CloseOnAuthenticationExpiration = true;
        }); // 🌐 SignalR Hubマッピング（Blazor Serverの双方向通信）


        // 🎯 Pure Blazor Server ルーティング設定
        // 【アーキテクチャ統一設計】
        // 全ページをBlazor Serverで処理する統一アーキテクチャ実装
        // 
        // ルーティング設定:
        // 1. ルートパス → Blazor Server Pages/Index.razorで認証分岐処理
        // 2. 管理画面パス（/admin/* → Blazor Server）  
        // 3. フォールバック（全ページ → Blazor Server _host）
        app.MapFallbackToPage("/admin/{**path}", "/_host");
        app.MapFallbackToPage("/_host");

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

                app.Logger.LogInformation("✅ 初期データ投入が完了しました StartupTime: {StartupTime}", DateTime.UtcNow);
            }
            catch (Exception ex)
            {
                app.Logger.LogCritical(ex, "❌ 初期データ投入中にエラーが発生しました StartupFailure: {Message} Time: {Time}",
                    ex.Message, DateTime.UtcNow);
                throw; // 🚨 初期化失敗時はアプリケーション起動を停止
            }
        }

        return app;
    }
}

