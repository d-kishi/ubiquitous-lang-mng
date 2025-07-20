using Microsoft.AspNetCore.Identity;
using UbiquitousLanguageManager.Infrastructure.Data.Entities;
using UbiquitousLanguageManager.Web.Authentication;
using System.Security.Claims;

namespace UbiquitousLanguageManager.Web.Services;

/// <summary>
/// Web層認証サービス
/// 
/// 【Blazor Server初学者向け解説】
/// このサービスは、ASP.NET Core IdentityのCookie認証を管理し、
/// Blazor Serverアプリケーションでの認証状態を提供します。
/// ApplicationUserとDomain層のユーザー情報を統合し、Claims形式で認証情報を管理します。
/// </summary>
public class AuthenticationService
{
    private readonly CustomAuthenticationStateProvider _authStateProvider;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<AuthenticationService> _logger;

    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="authStateProvider">認証状態プロバイダー</param>
    /// <param name="signInManager">サインイン管理</param>
    /// <param name="userManager">ユーザー管理</param>
    /// <param name="logger">ロガー</param>
    public AuthenticationService(
        CustomAuthenticationStateProvider authStateProvider,
        SignInManager<ApplicationUser> signInManager,
        UserManager<ApplicationUser> userManager,
        ILogger<AuthenticationService> logger)
    {
        _authStateProvider = authStateProvider;
        _signInManager = signInManager;
        _userManager = userManager;
        _logger = logger;
    }

    /// <summary>
    /// ユーザーのログアウト処理
    /// </summary>
    public async Task LogoutAsync()
    {
        try
        {
            // ASP.NET Core Identity認証解除
            await _signInManager.SignOutAsync();
            
            // Blazor認証状態の変更を通知
            _authStateProvider.NotifyUserLogout();
            
            _logger.LogInformation("ユーザーがログアウトしました");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "ログアウト処理中にエラーが発生しました");
        }
    }

    /// <summary>
    /// 現在のユーザーのDomain UserIdを取得します
    /// </summary>
    /// <returns>Domain UserIdまたはnull</returns>
    public long? GetCurrentDomainUserId()
    {
        return _authStateProvider.GetCurrentDomainUserId();
    }

    /// <summary>
    /// 現在のユーザーがアクティブかどうかを確認します
    /// </summary>
    /// <returns>アクティブフラグ</returns>
    public bool IsCurrentUserActive()
    {
        return _authStateProvider.IsCurrentUserActive();
    }

    /// <summary>
    /// 現在のユーザーが初回ログインかどうかを確認します
    /// </summary>
    /// <returns>初回ログインフラグ</returns>
    public bool IsCurrentUserFirstLogin()
    {
        return _authStateProvider.IsCurrentUserFirstLogin();
    }

    /// <summary>
    /// Phase A3で実装予定：ログイン機能（Web層版）
    /// </summary>
    public async Task<UbiquitousLanguageManager.Contracts.DTOs.Authentication.LoginResponseDto> LoginAsync(UbiquitousLanguageManager.Contracts.DTOs.Authentication.LoginRequestDto request)
    {
        await Task.CompletedTask;
        _logger.LogInformation("LoginAsync called - Phase A3で実装予定");
        return UbiquitousLanguageManager.Contracts.DTOs.Authentication.LoginResponseDto.Error("Phase A3で実装予定");
    }

    /// <summary>
    /// Phase A3で実装予定：現在ユーザー取得（Web層版）
    /// </summary>
    public async Task<UbiquitousLanguageManager.Contracts.DTOs.Authentication.AuthenticatedUserDto?> GetCurrentUserAsync()
    {
        await Task.CompletedTask;
        _logger.LogInformation("GetCurrentUserAsync called - Phase A3で実装予定");
        return null;
    }

    /// <summary>
    /// Phase A3で実装予定：パスワード変更（Web層版）
    /// </summary>
    public async Task<UbiquitousLanguageManager.Contracts.DTOs.Authentication.ChangePasswordResponseDto> ChangePasswordAsync(UbiquitousLanguageManager.Contracts.DTOs.Authentication.ChangePasswordRequestDto request)
    {
        await Task.CompletedTask;
        _logger.LogInformation("ChangePasswordAsync called - Phase A3で実装予定");
        return UbiquitousLanguageManager.Contracts.DTOs.Authentication.ChangePasswordResponseDto.Error("Phase A3で実装予定");
    }
}