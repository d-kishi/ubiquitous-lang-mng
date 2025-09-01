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
    /// CustomAuthenticationStateProviderのコンストラクタ（Phase A5標準Identity移行対応）
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

            // 標準IdentityUserからDomain固有のClaimsを追加
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
    public virtual void NotifyUserLogout()
    {
        var anonymousUser = new ClaimsPrincipal(new ClaimsIdentity());
        var authState = Task.FromResult(new AuthenticationState(anonymousUser));
        NotifyAuthenticationStateChanged(authState);
    }

    /// <summary>
    /// Domain固有のClaimsを追加します（Phase A5標準Identity移行対応）
    /// </summary>
    /// <param name="claims">Claims一覧</param>
    /// <param name="user">標準IdentityUser</param>
    private async Task AddDomainSpecificClaims(List<Claim> claims, IdentityUser user)
    {
        try
        {
            // DomainUserIdプロパティは標準IdentityUserにないため、Identity.Idを使用
            // 将来的にDomainとIdentity間のマッピングテーブルで管理予定
            claims.Add(new Claim("DomainUserId", user.Id));

            // ユーザーの状態情報をクレームとして追加
            // 標準IdentityUserには削除フラグがないため、常にアクティブとして設定
            claims.Add(new Claim("IsActive", "true"));

            // 標準IdentityUserには初回ログインフラグがないため、カスタム実装必要
            // 現在は常にfalse（実装済み扱い）として設定
            claims.Add(new Claim("IsFirstLogin", "false"));

            // 標準IdentityUserには更新日時がないため、現在時刻を使用
            claims.Add(new Claim("UpdatedAt", DateTime.UtcNow.ToString("O")));

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
    public virtual long? GetCurrentDomainUserId()
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

        // 標準IdentityUser.Idを一時的にDomainUserIdとして使用
        var userIdClaim = httpContext.User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim != null && long.TryParse(userIdClaim.Value, out var userId))
        {
            return userId;
        }

        return null;
    }

    /// <summary>
    /// 現在のユーザーがアクティブかどうかを確認します
    /// </summary>
    /// <returns>アクティブフラグ</returns>
    public virtual bool IsCurrentUserActive()
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
    public virtual bool IsCurrentUserFirstLogin()
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