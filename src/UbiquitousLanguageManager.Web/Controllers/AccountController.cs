using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using UbiquitousLanguageManager.Infrastructure.Data.Entities;
using UbiquitousLanguageManager.Web.Models;

namespace UbiquitousLanguageManager.Web.Controllers;

/// <summary>
/// アカウント認証コントローラー
/// 
/// 【Blazor Server初学者向け解説】
/// Blazor ServerアプリケーションでCookie認証を実装する場合、
/// ログイン・ログアウト処理はMVCコントローラーで実装し、
/// 認証後にBlazorページにリダイレクトする方式が一般的です。
/// </summary>
public class AccountController : Controller
{
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<AccountController> _logger;

    /// <summary>
    /// AccountControllerのコンストラクタ
    /// </summary>
    /// <param name="signInManager">ASP.NET Core Identity のサインイン管理</param>
    /// <param name="userManager">ASP.NET Core Identity のユーザー管理</param>
    /// <param name="logger">ログ出力サービス</param>
    public AccountController(
        SignInManager<ApplicationUser> signInManager,
        UserManager<ApplicationUser> userManager,
        ILogger<AccountController> logger)
    {
        _signInManager = signInManager;
        _userManager = userManager;
        _logger = logger;
    }

    /// <summary>
    /// ログインページ表示
    /// </summary>
    /// <param name="returnUrl">ログイン後のリダイレクト先URL</param>
    /// <returns>ログインページビュー</returns>
    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        return View(new LoginViewModel());
    }

    /// <summary>
    /// ログイン処理実行
    /// </summary>
    /// <param name="model">ログイン情報</param>
    /// <param name="returnUrl">ログイン後のリダイレクト先URL</param>
    /// <returns>ログイン結果とリダイレクト</returns>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            // ASP.NET Core Identityでログイン試行
            var result = await _signInManager.PasswordSignInAsync(
                model.Email, 
                model.Password, 
                model.RememberMe, 
                lockoutOnFailure: true);

            if (result.Succeeded)
            {
                _logger.LogInformation("ユーザー {Email} がログインしました", model.Email);
                
                // ログイン成功時のリダイレクト処理
                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                {
                    return Redirect(returnUrl);
                }
                
                return RedirectToAction("Index", "Home");
            }

            if (result.IsLockedOut)
            {
                _logger.LogWarning("ユーザー {Email} のアカウントがロックアウトされました", model.Email);
                ModelState.AddModelError(string.Empty, "アカウントがロックされています。しばらく時間をおいてから再度お試しください。");
                return View(model);
            }

            if (result.IsNotAllowed)
            {
                _logger.LogWarning("ユーザー {Email} のログインが許可されていません", model.Email);
                ModelState.AddModelError(string.Empty, "ログインが許可されていません。");
                return View(model);
            }

            // ログイン失敗
            _logger.LogWarning("ユーザー {Email} のログインに失敗しました", model.Email);
            ModelState.AddModelError(string.Empty, "メールアドレスまたはパスワードが正しくありません。");
            return View(model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "ログイン処理中にエラーが発生しました: {Email}", model.Email);
            ModelState.AddModelError(string.Empty, "ログイン処理中にエラーが発生しました。しばらく時間をおいてから再度お試しください。");
            return View(model);
        }
    }

    /// <summary>
    /// ログアウト処理実行
    /// </summary>
    /// <returns>ログアウト後のリダイレクト</returns>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        try
        {
            var userEmail = User?.Identity?.Name ?? "不明";
            await _signInManager.SignOutAsync();
            
            _logger.LogInformation("ユーザー {Email} がログアウトしました", userEmail);
            
            return RedirectToAction("Index", "Home");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "ログアウト処理中にエラーが発生しました");
            return RedirectToAction("Index", "Home");
        }
    }

    /// <summary>
    /// アクセス拒否ページ表示
    /// </summary>
    /// <returns>アクセス拒否ページビュー</returns>
    [HttpGet]
    public IActionResult AccessDenied()
    {
        return View();
    }
}