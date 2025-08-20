using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using UbiquitousLanguageManager.Infrastructure.Data.Entities;
using UbiquitousLanguageManager.Web.Models;

namespace UbiquitousLanguageManager.Web.Controllers;

/// <summary>
/// アカウント管理コントローラー（CTRL-001: 404エラー解消）
/// 
/// 【Blazor Server初学者向け解説】
/// このコントローラーは、ASP.NET Core MVCパターンを使用してアカウント関連の機能を提供します。
/// ユーザー認証（ログイン）はBlazor Serverで実装されていますが、パスワード変更等の
/// 複雑なフォーム処理は、MVCコントローラーとRazorビューの組み合わせで実装します。
/// 
/// 【セキュリティ設計】
/// - [Authorize]属性による認証必須
/// - [ValidateAntiForgeryToken]によるCSRF攻撃防止
/// - UserManager&lt;ApplicationUser&gt;による安全なパスワード変更
/// </summary>
[Authorize]
[Route("Account")]
public class AccountController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly ILogger<AccountController> _logger;

    /// <summary>
    /// AccountControllerのコンストラクタ
    /// </summary>
    /// <param name="userManager">ASP.NET Core Identity ユーザー管理サービス</param>
    /// <param name="signInManager">ASP.NET Core Identity サインイン管理サービス</param>
    /// <param name="logger">ログ出力サービス</param>
    public AccountController(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        ILogger<AccountController> logger)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _logger = logger;
    }

    /// <summary>
    /// パスワード変更画面表示（GET）
    /// 
    /// 【TECH-004対応】初回ログイン時パスワード変更機能
    /// 初回ログインユーザー（IsFirstLogin=true）は強制的にここにリダイレクトされます。
    /// 
    /// 【セキュリティ考慮事項】
    /// - 認証済みユーザーのみアクセス可能（[Authorize]属性）
    /// - 初回ログイン状態の確認とUI表示の調整
    /// </summary>
    /// <returns>パスワード変更画面ビュー</returns>
    [HttpGet("change-password")]
    public async Task<IActionResult> ChangePassword()
    {
        try
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                _logger.LogWarning("Change password accessed by unauthenticated user");
                return Challenge(); // 認証チャレンジを発生させる
            }

            _logger.LogInformation("Change password page accessed by user {Email} (FirstLogin: {IsFirstLogin})", 
                user.Email, user.IsFirstLogin);

            var model = new ChangePasswordViewModel();
            
            // 初回ログインフラグをViewBagで渡してUI調整
            ViewBag.IsFirstLogin = user.IsFirstLogin;
            
            return View(model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error displaying change password page: {Message}", ex.Message);
            return RedirectToAction("Index", "Home");
        }
    }

    /// <summary>
    /// パスワード変更処理（POST）
    /// 
    /// 【セキュリティ実装】
    /// - [ValidateAntiForgeryToken]によるCSRF攻撃防止
    /// - UserManager.ChangePasswordAsyncによる安全なパスワード更新
    /// - 初回ログインフラグの解除（IsFirstLogin = false）
    /// - セキュリティスタンプ更新による他セッションの無効化
    /// 
    /// 【TECH-004完了条件】
    /// パスワード変更成功時に IsFirstLogin を false に更新し、
    /// 以降のログインで通常のアプリケーション機能にアクセス可能にします。
    /// </summary>
    /// <param name="model">パスワード変更フォームデータ</param>
    /// <returns>成功時は管理画面へリダイレクト、失敗時は同画面を再表示</returns>
    [HttpPost("change-password")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Change password form validation failed");
                return View(model);
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                _logger.LogWarning("Change password attempted by unauthenticated user");
                return Challenge();
            }

            _logger.LogInformation("Password change attempt for user {Email}", user.Email);

            // ASP.NET Core Identity の安全なパスワード変更
            var result = await _userManager.ChangePasswordAsync(
                user, model.CurrentPassword, model.NewPassword);

            if (result.Succeeded)
            {
                // 初回ログインフラグを解除（TECH-004完了条件）
                if (user.IsFirstLogin)
                {
                    user.IsFirstLogin = false;
                    user.UpdatedAt = DateTime.UtcNow;
                    await _userManager.UpdateAsync(user);
                    
                    _logger.LogInformation("First login completed for user {Email}", user.Email);
                }

                // セキュリティスタンプ更新（他セッションの無効化）
                await _userManager.UpdateSecurityStampAsync(user);

                // サインイン状態を更新（新しいセキュリティスタンプでの再認証）
                await _signInManager.RefreshSignInAsync(user);

                _logger.LogInformation("Password changed successfully for user {Email}", user.Email);

                TempData["SuccessMessage"] = "パスワードが正常に変更されました。";
                
                // 成功時は管理画面（ユーザー一覧）にリダイレクト
                return RedirectToAction("Index", "Home");
            }
            else
            {
                // パスワード変更エラーをModelStateに追加
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                    _logger.LogWarning("Password change error for user {Email}: {Error}", 
                        user.Email, error.Description);
                }

                // 初回ログイン状態を再設定してView表示
                ViewBag.IsFirstLogin = user.IsFirstLogin;
                return View(model);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during password change: {Message}", ex.Message);
            ModelState.AddModelError(string.Empty, "パスワード変更中にエラーが発生しました。");
            
            // エラー時も初回ログイン状態を保持
            var user = await _userManager.GetUserAsync(User);
            ViewBag.IsFirstLogin = user?.IsFirstLogin ?? false;
            
            return View(model);
        }
    }
}