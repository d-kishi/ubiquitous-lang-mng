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
    /// コンストラクタ（Phase A5標準Identity移行対応）
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
    /// 仕様書10.3.1準拠: ユーザーのログアウト処理
    /// セッション無効化・Cookie削除・クリーンアップ実行
    /// </summary>
    public async Task LogoutAsync()
    {
        try
        {
            _logger.LogInformation("ログアウト処理開始");

            // ASP.NET Core Identity認証解除
            // 仕様書10.3.1準拠: セッション無効化・Cookie削除
            await _signInManager.SignOutAsync();
            
            // Blazor認証状態の変更を通知
            _authStateProvider.NotifyUserLogout();
            
            _logger.LogInformation("ログアウト処理完了: セッション無効化・Cookie削除・状態クリーンアップ");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "ログアウト処理中にエラーが発生しました");
            throw; // 呼び出し元でエラーハンドリングできるよう再throw
        }
    }

    /// <summary>
    /// 現在のユーザーのDomain UserIdを取得します
    /// </summary>
    /// <returns>Domain UserIdまたはNull</returns>
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
    /// 仕様書2.1.1準拠: ログイン機能（Remember Me対応・ロックアウト機構なし）
    /// Phase A5標準Identity移行対応
    /// </summary>
    /// <param name="request">ログイン要求情報</param>
    /// <returns>ログイン結果</returns>
    public async Task<UbiquitousLanguageManager.Contracts.DTOs.Authentication.LoginResponseDto> LoginAsync(UbiquitousLanguageManager.Contracts.DTOs.Authentication.LoginRequestDto request)
    {
        try
        {
            _logger.LogInformation("ログイン試行: {Email}", request.Email);

            // ユーザー検索
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null)
            {
                _logger.LogWarning("ログイン失敗: ユーザーが見つかりません {Email}", request.Email);
                return UbiquitousLanguageManager.Contracts.DTOs.Authentication.LoginResponseDto.Error("メールアドレスまたはパスワードが正しくありません。");
            }

            // ログインロックアウト機構確認（仕様書2.1.1準拠: 無効化）
            if (await _userManager.IsLockedOutAsync(user))
            {
                _logger.LogWarning("ログイン失敗: アカウントロックアウト {Email}", request.Email);
                // 仕様書2.1.1準拠では本来発生しないが、念のため対応
                return UbiquitousLanguageManager.Contracts.DTOs.Authentication.LoginResponseDto.Error("アカウントが一時的に無効になっています。");
            }

            // 仕様書2.1.1準拠: Remember Me機能を含むログイン実行
            // lockoutOnFailure=false: ロックアウト機構を無効化
            var result = await _signInManager.PasswordSignInAsync(
                user, 
                request.Password, 
                isPersistent: request.RememberMe, // 仕様書2.1.1準拠: ログイン状態保持
                lockoutOnFailure: false); // 仕様書2.1.1準拠: ロックアウト機構は設けない

            if (result.Succeeded)
            {
                _logger.LogInformation("ログイン成功: {Email}, RememberMe: {RememberMe}", request.Email, request.RememberMe);
                
                // Blazor認証状態の更新
                _authStateProvider.NotifyUserAuthentication(_authStateProvider.GetAuthenticationStateAsync());

                // 仕様書2.1.1準拠: ログイン成功時のユーザー情報DTO作成
                // 標準IdentityUserに対応
                var authenticatedUser = new UbiquitousLanguageManager.Contracts.DTOs.Authentication.AuthenticatedUserDto
                {
                    Id = user.Id.GetHashCode(), // 標準IdentityUser.Idのハッシュ値を使用
                    Email = user.Email ?? string.Empty,
                    Name = GetNameFromUser(user), // 標準IdentityUserからName取得
                    Role = "GeneralUser", // 標準実装：ロール情報の簡易取得
                    IsActive = !IsUserDeleted(user), // 標準IdentityUserでの削除判定
                    IsFirstLogin = IsUserFirstLogin(user), // 標準IdentityUserでの初回ログイン判定
                    UpdatedAt = GetUserUpdatedAt(user) // 標準IdentityUserでの更新日時取得
                };

                return UbiquitousLanguageManager.Contracts.DTOs.Authentication.LoginResponseDto.Success(
                    authenticatedUser, 
                    IsUserFirstLogin(user), // 初回ログインフラグを渡す
                    null); // リダイレクトURLは現在未使用
            }
            else if (result.IsNotAllowed)
            {
                _logger.LogWarning("ログイン失敗: アカウント未承認 {Email}", request.Email);
                return UbiquitousLanguageManager.Contracts.DTOs.Authentication.LoginResponseDto.Error("アカウントが有効化されていません。");
            }
            else
            {
                _logger.LogWarning("ログイン失敗: 認証失敗 {Email}", request.Email);
                return UbiquitousLanguageManager.Contracts.DTOs.Authentication.LoginResponseDto.Error("メールアドレスまたはパスワードが正しくありません。");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "ログイン処理中にエラーが発生しました: {Email}", request.Email);
            return UbiquitousLanguageManager.Contracts.DTOs.Authentication.LoginResponseDto.Error("ログイン処理中にエラーが発生しました。管理者にお問い合わせください。");
        }
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

    // ===== Phase A5標準Identity移行対応ヘルパーメソッド =====

    /// <summary>
    /// 標準IdentityUserからName情報を取得（カスタム実装）
    /// </summary>
    private static string GetNameFromUser(IdentityUser user)
    {
        // 標準IdentityUserにはName属性がないため、EmailのLocalPart部分を使用
        return user.Email?.Split('@')[0] ?? "Unknown";
    }

    /// <summary>
    /// 標準IdentityUserでの削除判定（カスタム実装）
    /// </summary>
    private static bool IsUserDeleted(IdentityUser user)
    {
        // 標準IdentityUserには削除フラグがないため、常にfalse（アクティブ）
        return false;
    }

    /// <summary>
    /// 標準IdentityUserでの初回ログイン判定（カスタム実装）
    /// </summary>
    private static bool IsUserFirstLogin(IdentityUser user)
    {
        // 標準IdentityUserには初回ログインフラグがないため、常にfalse
        // 実際の実装ではクレームやカスタムフィールドで管理が必要
        return false;
    }

    /// <summary>
    /// 標準IdentityUserでの更新日時取得（カスタム実装）
    /// </summary>
    private static DateTime GetUserUpdatedAt(IdentityUser user)
    {
        // 標準IdentityUserには更新日時がないため、現在時刻を返す
        // 実際の実装では追加のトラッキングが必要
        return DateTime.UtcNow;
    }
}