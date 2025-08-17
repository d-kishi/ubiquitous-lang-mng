using Microsoft.AspNetCore.Identity;
using UbiquitousLanguageManager.Infrastructure.Data.Entities;

namespace UbiquitousLanguageManager.Web.Middleware;

/// <summary>
/// 初回ログインアクセス制限ミドルウェア
/// 
/// 【機能概要】
/// 初回ログイン状態（IsFirstLogin=true）のユーザーを対象に、
/// パスワード変更画面以外へのアクセスを制限し、セキュリティを強化します。
/// 
/// 【TECH-004仕様対応】
/// - 全ユーザー（スーパーユーザー含む）の初回ログイン時必須パスワード変更
/// - パスワード変更完了まで他画面アクセス制限
/// - 適切なログ記録・監査機能
/// 
/// 【セキュリティポリシー】
/// - 制限対象: /admin/*, /projects/*, /domains/* 等の業務画面
/// - 例外対象: /Account/ChangePassword, /Account/Logout, 静的リソース
/// - アクセス拒否時は強制的にパスワード変更画面へリダイレクト
/// </summary>
public class FirstLoginRedirectMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<FirstLoginRedirectMiddleware> _logger;

    /// <summary>
    /// 制限対象パス（パスワード変更画面以外へのアクセスを制限）
    /// </summary>
    private static readonly string[] RestrictedPaths = new[]
    {
        "/admin",           // 管理画面全体
        "/projects",        // プロジェクト管理
        "/domains",         // ドメイン管理
        "/ubiquitous",      // ユビキタス言語管理
        "/",                // ホーム画面
        "/Home"             // MVC ホーム画面
    };

    /// <summary>
    /// 例外対象パス（初回ログイン状態でもアクセス許可）
    /// TECH-003対応: Blazor版認証システムに更新
    /// </summary>
    private static readonly string[] AllowedPaths = new[]
    {
        "/change-password",         // Blazor版パスワード変更画面
        "/logout",                  // Blazor版ログアウト機能
        "/login",                   // Blazor版ログイン画面（念のため）
        "/access-denied",           // Blazor版アクセス拒否画面
        "/health",                  // ヘルスチェック
        "/api/auth",               // 認証API
        "/_blazor",                // Blazor SignalR Hub
        "/_vs/browserLink"         // Visual Studio BrowserLink
    };

    /// <summary>
    /// 静的リソース拡張子（アクセス制限対象外）
    /// </summary>
    private static readonly string[] StaticResourceExtensions = new[]
    {
        ".css", ".js", ".png", ".jpg", ".jpeg", ".gif", ".ico", 
        ".svg", ".woff", ".woff2", ".ttf", ".eot", ".map"
    };

    /// <summary>
    /// FirstLoginRedirectMiddleware コンストラクタ
    /// </summary>
    /// <param name="next">次のミドルウェア</param>
    /// <param name="logger">ログ出力インスタンス</param>
    public FirstLoginRedirectMiddleware(RequestDelegate next, ILogger<FirstLoginRedirectMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    /// <summary>
    /// ミドルウェアメイン処理
    /// 
    /// 【処理フロー】
    /// 1. 認証状態確認
    /// 2. IsFirstLoginフラグ確認
    /// 3. アクセス対象パス判定
    /// 4. 制限適用またはリダイレクト実行
    /// 5. ログ記録・監査情報出力
    /// </summary>
    public async Task InvokeAsync(HttpContext context)
    {
        // 🔍 認証状態確認
        if (context.User.Identity?.IsAuthenticated != true)
        {
            // 未認証ユーザーは制限対象外
            await _next(context);
            return;
        }

        try
        {
            // 🔍 IsFirstLoginフラグ確認
            var userManager = context.RequestServices.GetRequiredService<UserManager<ApplicationUser>>();
            var user = await userManager.GetUserAsync(context.User);
            
            if (user == null)
            {
                _logger.LogWarning("認証済みユーザーがUserManagerで取得できませんでした。UserId: {UserId}", 
                    context.User.Identity.Name);
                await _next(context);
                return;
            }

            // 🔍 初回ログイン状態確認
            if (!user.IsFirstLogin)
            {
                // 初回ログイン完了済み → 制限なし
                await _next(context);
                return;
            }

            // 🎯 初回ログイン状態 → アクセス制限適用
            var requestPath = context.Request.Path.Value ?? string.Empty;
            
            // 📊 監査ログ記録
            _logger.LogInformation("初回ログイン状態のユーザーアクセス検知: {UserId}, Path: {Path}", 
                user.Id, requestPath);

            // 🟢 例外対象パス確認
            if (IsAllowedPath(requestPath))
            {
                _logger.LogDebug("初回ログイン状態ですが例外対象パスです: {Path}", requestPath);
                await _next(context);
                return;
            }

            // 🟢 静的リソース確認
            if (IsStaticResource(requestPath))
            {
                await _next(context);
                return;
            }

            // 🔴 制限対象パス確認
            if (IsRestrictedPath(requestPath))
            {
                // 🚨 アクセス制限実行 → パスワード変更画面へリダイレクト
                _logger.LogWarning("初回ログイン状態のユーザーの制限対象パスアクセスを拒否: {UserId}, Path: {Path}", 
                    user.Id, requestPath);

                // セキュリティポリシー適用ログ
                _logger.LogInformation("セキュリティポリシー適用: 初回ログイン時アクセス制限 - UserId: {UserId}, AccessPath: {Path}, RedirectTo: /change-password", 
                    user.Id, requestPath);

                context.Response.Redirect("/change-password");
                return;
            }

            // 🔍 その他のパス → デフォルトでリダイレクト（安全側に倒す）
            _logger.LogInformation("初回ログイン状態で未定義パスアクセス → パスワード変更画面へリダイレクト: {UserId}, Path: {Path}", 
                user.Id, requestPath);
            
            context.Response.Redirect("/change-password");
        }
        catch (Exception ex)
        {
            // 🚨 例外発生時はログ記録後、次のミドルウェアへ処理を移行
            _logger.LogError(ex, "FirstLoginRedirectMiddleware処理中にエラーが発生しました: {Message}", ex.Message);
            await _next(context);
        }
    }

    /// <summary>
    /// 例外対象パス判定
    /// 初回ログイン状態でもアクセスを許可するパス
    /// </summary>
    private static bool IsAllowedPath(string path)
    {
        return AllowedPaths.Any(allowedPath => 
            path.StartsWith(allowedPath, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// 制限対象パス判定
    /// 初回ログイン状態でアクセスを制限するパス
    /// </summary>
    private static bool IsRestrictedPath(string path)
    {
        return RestrictedPaths.Any(restrictedPath => 
            path.StartsWith(restrictedPath, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// 静的リソース判定
    /// CSS・JS・画像等のリソースファイル
    /// </summary>
    private static bool IsStaticResource(string path)
    {
        return StaticResourceExtensions.Any(extension => 
            path.EndsWith(extension, StringComparison.OrdinalIgnoreCase));
    }
}

/// <summary>
/// FirstLoginRedirectMiddleware拡張メソッド
/// 
/// 【使用方法】
/// Program.cs の適切な位置に以下を追加:
/// app.UseFirstLoginRedirect();
/// 
/// 【ミドルウェアパイプライン配置】
/// 認証ミドルウェア（UseAuthentication()）の後、
/// 認可ミドルウェア（UseAuthorization()）の前に配置を推奨
/// </summary>
public static class FirstLoginRedirectMiddlewareExtensions
{
    /// <summary>
    /// FirstLoginRedirectMiddlewareをミドルウェアパイプラインに追加
    /// </summary>
    public static IApplicationBuilder UseFirstLoginRedirect(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<FirstLoginRedirectMiddleware>();
    }
}