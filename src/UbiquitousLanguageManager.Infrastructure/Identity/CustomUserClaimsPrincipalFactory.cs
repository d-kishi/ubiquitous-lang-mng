using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Threading.Tasks;
using UbiquitousLanguageManager.Infrastructure.Data.Entities;

namespace UbiquitousLanguageManager.Infrastructure.Identity;

/// <summary>
/// カスタムUserClaimsPrincipalFactory
/// ユーザー情報から必要なClaimを生成
/// 
/// 【設計説明】
/// Claimテーブルを使用せず、ApplicationUserの情報から
/// 直接必要なClaimを生成する
/// </summary>
public class CustomUserClaimsPrincipalFactory : UserClaimsPrincipalFactory<ApplicationUser, IdentityRole>
{
    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="userManager">ユーザーマネージャー</param>
    /// <param name="roleManager">ロールマネージャー</param>
    /// <param name="optionsAccessor">Identityオプション</param>
    public CustomUserClaimsPrincipalFactory(
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager,
        IOptions<IdentityOptions> optionsAccessor)
        : base(userManager, roleManager, optionsAccessor)
    {
    }

    /// <summary>
    /// ユーザー情報からClaimを生成
    /// </summary>
    protected override async Task<ClaimsIdentity> GenerateClaimsAsync(ApplicationUser user)
    {
        var id = await base.GenerateClaimsAsync(user);

        // カスタムClaimを追加
        if (!string.IsNullOrEmpty(user.Name))
        {
            id.AddClaim(new Claim("name", user.Name));
        }

        // 初回ログインフラグ
        if (user.IsFirstLogin)
        {
            id.AddClaim(new Claim("IsFirstLogin", "true"));
        }

        // 削除フラグ（論理削除されたユーザーの識別用）
        if (user.IsDeleted)
        {
            id.AddClaim(new Claim("IsDeleted", "true"));
        }

        return id;
    }
}