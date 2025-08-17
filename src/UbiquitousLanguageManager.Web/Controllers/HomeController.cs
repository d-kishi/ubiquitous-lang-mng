using Microsoft.AspNetCore.Mvc;

namespace UbiquitousLanguageManager.Web.Controllers;

/// <summary>
/// ホームページコントローラー
/// 
/// 【Blazor Server初学者向け解説】
/// MVCパターンでのルーティングとBlazor Serverアプリケーションの連携を行います。
/// このコントローラーは主にMVCページ（ログイン等）からBlazorページへのブリッジとして機能します。
/// </summary>
public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;

    /// <summary>
    /// HomeControllerのコンストラクタ
    /// </summary>
    /// <param name="logger">ログ出力サービス</param>
    public HomeController(ILogger<HomeController> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// ホームページ表示
    /// 認証済みの場合はBlazorアプリケーションにリダイレクト
    /// 
    /// 【MVC/Blazor統合設計】
    /// [AllowAnonymous]属性により未認証ユーザーのアクセスを許可し、
    /// 認証状態に応じて適切な画面（MVC/Blazor）にルーティングします。
    /// 
    /// 【認証状態動的ルーティング】
    /// - 未認証ユーザー: MVCビュー表示（ログインページへの誘導）
    /// - 認証済みユーザー: Blazor Server管理画面へリダイレクト
    /// </summary>
    /// <returns>ホームページビューまたはリダイレクト</returns>
    [Microsoft.AspNetCore.Authorization.AllowAnonymous]
    public IActionResult Index()
    {
        try
        {
            _logger.LogInformation("HomeController.Index accessed by user: {IsAuthenticated}", 
                User.Identity?.IsAuthenticated ?? false);

            // 認証済みユーザーはBlazor Server管理画面にリダイレクト
            if (User.Identity?.IsAuthenticated == true)
            {
                _logger.LogInformation("Authenticated user detected, redirecting to Blazor Server admin");
                return Redirect("/admin/users");
            }

            // 未認証ユーザーは強制的にログイン画面へリダイレクト（仕様準拠）
            _logger.LogInformation("Unauthenticated user, redirecting to login page");
            return Redirect("/login");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in HomeController.Index: {Message}", ex.Message);
            // エラー時は安全にログイン画面へリダイレクト
            return Redirect("/login");
        }
    }

    /// <summary>
    /// エラーページ表示
    /// </summary>
    /// <returns>エラーページビュー</returns>
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View();
    }
}