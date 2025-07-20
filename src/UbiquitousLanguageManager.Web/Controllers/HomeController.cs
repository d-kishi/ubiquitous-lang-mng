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
    /// </summary>
    /// <returns>ホームページビューまたはリダイレクト</returns>
    public IActionResult Index()
    {
        // 認証済みユーザーはBlazorアプリケーションにリダイレクト
        if (User.Identity?.IsAuthenticated == true)
        {
            return Redirect("/admin/users");
        }

        // 未認証の場合はホームページを表示
        return View();
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