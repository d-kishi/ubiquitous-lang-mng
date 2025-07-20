using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using UbiquitousLanguageManager.Infrastructure.Data.Entities;

namespace UbiquitousLanguageManager.Web.Authentication;

/// <summary>
/// カスタム認証状態プロバイダー
/// 
/// 【Blazor Server初学者向け解説】
/// AuthenticationStateProviderは、Blazor Server全体で認証状態を管理するサービスです。
/// ASP.NET Core IdentityのCookie認証と連携し、ApplicationUserからDomain層の情報を
/// ClaimsとしてBlazorコンポーネントに提供します。
/// </summary>
public class CustomAuthenticationStateProvider : AuthenticationStateProvider
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<CustomAuthenticationStateProvider> _logger;

    /// <summary>
    /// CustomAuthenticationStateProviderのコンストラクタ
    /// </summary>
    /// <param name="httpContextAccessor">HTTPコンテキストアクセサー</param>
    /// <param name="userManager">ASP.NET Core Identity ユーザー管理</param>
    /// <param name="logger">ログ出力サービス</param>
    public CustomAuthenticationStateProvider(
        IHttpContextAccessor httpContextAccessor,
        UserManager<ApplicationUser> userManager,
        ILogger<CustomAuthenticationStateProvider> logger)
    {
        _httpContextAccessor = httpContextAccessor;
        _userManager = userManager;
        _logger = logger;
    }

    /// <summary>
    /// 現在の認証状態を取得します
    /// </summary>
    /// <returns>認証状態</returns>
    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        try
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext?.User?.Identity?.IsAuthenticated != true)
            {
                // 未認証状態
                return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
            }

            var user = await _userManager.GetUserAsync(httpContext.User);
            if (user == null)
            {
                _logger.LogWarning("認証済みユーザーが見つかりません: {UserName}", httpContext.User.Identity.Name);
                return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
            }

            // ASP.NET Core IdentityのClaimsを取得
            var claims = new List<Claim>(httpContext.User.Claims);

            // ApplicationUserからDomain固有のClaimsを追加
            await AddDomainSpecificClaims(claims, user);

            // ロール情報を追加
            var roles = await _userManager.GetRolesAsync(user);
            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var identity = new ClaimsIdentity(claims, "Identity.Application");
            var principal = new ClaimsPrincipal(identity);

            return new AuthenticationState(principal);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "認証状態の取得中にエラーが発生しました");
            return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
        }
    }

    /// <summary>
    /// ユーザー情報の変更を通知します
    /// </summary>
    /// <param name="task">新しい認証状態のタスク</param>
    public void NotifyUserAuthentication(Task<AuthenticationState> task)
    {
        NotifyAuthenticationStateChanged(task);
    }

    /// <summary>
    /// ログアウトを通知します
    /// </summary>
    public void NotifyUserLogout()
    {
        var anonymousUser = new ClaimsPrincipal(new ClaimsIdentity());
        var authState = Task.FromResult(new AuthenticationState(anonymousUser));
        NotifyAuthenticationStateChanged(authState);
    }

    /// <summary>
    /// Domain固有のClaimsを追加します
    /// </summary>
    /// <param name="claims">Claims一覧</param>
    /// <param name="user">ApplicationUser</param>
    private async Task AddDomainSpecificClaims(List<Claim> claims, ApplicationUser user)
    {
        try
        {
            // F# Domain層のUserIdに相当するクレームを追加
            if (user.DomainUserId.HasValue)
            {
                claims.Add(new Claim("DomainUserId", user.DomainUserId.Value.ToString()));
            }

            // ユーザーの状態情報をクレームとして追加
            claims.Add(new Claim("IsActive", (!user.IsDeleted).ToString()));
            claims.Add(new Claim("IsFirstLogin", user.IsFirstLogin.ToString()));
            claims.Add(new Claim("UserRole", user.UserRole));
            claims.Add(new Claim("UpdatedAt", user.UpdatedAt.ToString("O")));

            // 所属プロジェクト情報をクレームとして追加（Phase A3で拡張予定）
            // var projectIds = await GetUserProjectIds(user.Id);
            // foreach (var projectId in projectIds)
            // {
            //     claims.Add(new Claim("ProjectId", projectId));
            // }

            await Task.CompletedTask; // 将来の非同期処理のための仮実装
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Domain固有のClaims追加中にエラーが発生しました: UserId={UserId}", user.Id);
        }
    }

    /// <summary>
    /// 現在のユーザーのDomain UserIdを取得します
    /// </summary>
    /// <returns>Domain UserIdまたはnull</returns>
    public long? GetCurrentDomainUserId()
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext?.User?.Identity?.IsAuthenticated != true)
        {
            return null;
        }

        var domainUserIdClaim = httpContext.User.FindFirst("DomainUserId");
        if (domainUserIdClaim != null && long.TryParse(domainUserIdClaim.Value, out var domainUserId))
        {
            return domainUserId;
        }

        return null;
    }

    /// <summary>
    /// 現在のユーザーがアクティブかどうかを確認します
    /// </summary>
    /// <returns>アクティブフラグ</returns>
    public bool IsCurrentUserActive()
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext?.User?.Identity?.IsAuthenticated != true)
        {
            return false;
        }

        var isActiveClaim = httpContext.User.FindFirst("IsActive");
        return isActiveClaim != null && bool.TryParse(isActiveClaim.Value, out var isActive) && isActive;
    }

    /// <summary>
    /// 現在のユーザーが初回ログインかどうかを確認します
    /// </summary>
    /// <returns>初回ログインフラグ</returns>
    public bool IsCurrentUserFirstLogin()
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext?.User?.Identity?.IsAuthenticated != true)
        {
            return false;
        }

        var isFirstLoginClaim = httpContext.User.FindFirst("IsFirstLogin");
        return isFirstLoginClaim != null && bool.TryParse(isFirstLoginClaim.Value, out var isFirstLogin) && isFirstLogin;
    }
}