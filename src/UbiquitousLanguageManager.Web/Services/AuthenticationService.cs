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

            // 【TECH-006修正】Blazor Server環境での認証処理最適化
            // HTTPコンテキストがBlazor SignalRと競合しないよう、慎重にCookie操作を実行
            
            // 仕様書2.1.1準拠: Remember Me機能を含むログイン実行
            // lockoutOnFailure=false: ロックアウト機構を無効化
            
            // ApplicationUserにキャスト（初期パスワード認証対応）
            var appUser = user as ApplicationUser;
            SignInResult result;
            
            // 【Blazor Server初学者向け解説】
            // 初回ログイン判定ロジック: IsFirstLoginフラグとInitialPasswordの存在で分岐
            // - 初回ログイン: InitialPassword（平文）で直接比較認証
            // - 通常ログイン: PasswordHash（ハッシュ値）でASP.NET Core Identity標準認証
            if (appUser != null && appUser.IsFirstLogin && !string.IsNullOrEmpty(appUser.InitialPassword))
            {
                // 🔑 初回ログイン：InitialPassword（平文）で比較認証
                // 仕様書2.1.2準拠: 初回ログイン時はパスワード変更必須
                if (request.Password == appUser.InitialPassword)
                {
                    // 手動でサインイン実行（初回ログイン専用）
                    // ASP.NET Core Identityの標準認証をスキップし、直接サインイン
                    await _signInManager.SignInAsync(user, isPersistent: request.RememberMe);
                    result = SignInResult.Success;
                    _logger.LogInformation("初回ログイン成功: InitialPassword認証 - Email: {Email}", request.Email);
                }
                else
                {
                    result = SignInResult.Failed;
                    _logger.LogWarning("初回ログイン失敗: InitialPassword不一致 - Email: {Email}", request.Email);
                }
            }
            else
            {
                // 🔐 通常ログイン：PasswordHashで認証（既存処理）
                // ASP.NET Core Identity標準のハッシュベース認証を実行
                try 
                {
                    result = await _signInManager.PasswordSignInAsync(
                        user, 
                        request.Password, 
                        isPersistent: request.RememberMe, // 仕様書2.1.1準拠: ログイン状態保持
                        lockoutOnFailure: false); // 仕様書2.1.1準拠: ロックアウト機構は設けない
                }
                catch (InvalidOperationException ex) when (ex.Message.Contains("Headers are read-only"))
                {
                    // Blazor Server環境でのHeaders競合エラーをキャッチ
                    _logger.LogError(ex, "Blazor Server認証処理でHeaders競合エラーが発生: {Email}", request.Email);
                    return UbiquitousLanguageManager.Contracts.DTOs.Authentication.LoginResponseDto.Error(
                        "認証処理中にエラーが発生しました。ページを更新してから再度お試しください。");
                }
            }

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
    /// パスワード変更処理（TECH-004: 初回ログイン時パスワード変更対応）
    /// </summary>
    /// <param name="userEmail">対象ユーザーのメールアドレス</param>
    /// <param name="request">パスワード変更要求</param>
    /// <returns>パスワード変更結果</returns>
public async Task<UbiquitousLanguageManager.Contracts.DTOs.Authentication.ChangePasswordResponseDto> ChangePasswordAsync(string userEmail, UbiquitousLanguageManager.Contracts.DTOs.Authentication.ChangePasswordRequestDto request)
{
    try
    {
        _logger.LogInformation("パスワード変更処理開始: ユーザー={Email}", userEmail);

        // ユーザー検索
        var user = await _userManager.FindByEmailAsync(userEmail);
        if (user == null)
        {
            _logger.LogWarning("パスワード変更失敗: ユーザーが見つかりません {Email}", userEmail);
            return UbiquitousLanguageManager.Contracts.DTOs.Authentication.ChangePasswordResponseDto.Error("ユーザーが見つかりません。");
        }

        // ApplicationUserにキャスト
        if (user is not ApplicationUser appUser)
        {
            _logger.LogError("パスワード変更失敗: ApplicationUserキャストエラー {Email}", userEmail);
            return UbiquitousLanguageManager.Contracts.DTOs.Authentication.ChangePasswordResponseDto.Error("システムエラーが発生しました。");
        }

        // パスワード変更実行
        var result = await _userManager.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword);
        
        if (result.Succeeded)
        {
            // 初回ログインフラグを更新（true → false）
            if (appUser.IsFirstLogin)
            {
                appUser.IsFirstLogin = false;
                appUser.UpdatedAt = DateTime.UtcNow;
                appUser.UpdatedBy = user.Email ?? "System";
                
                // 初期パスワードをクリア（セキュリティ強化）
                appUser.InitialPassword = null;
                
                var updateResult = await _userManager.UpdateAsync(appUser);
                if (!updateResult.Succeeded)
                {
                    _logger.LogError("パスワード変更後のフラグ更新失敗: {Email}, エラー: {Errors}", 
                        userEmail, string.Join(", ", updateResult.Errors.Select(e => e.Description)));
                    return UbiquitousLanguageManager.Contracts.DTOs.Authentication.ChangePasswordResponseDto.Error("パスワード変更は成功しましたが、システム更新でエラーが発生しました。");
                }
                
                _logger.LogInformation("初回ログインパスワード変更完了: {Email}", userEmail);
            }
            
            // Blazor認証状態の更新通知
            _authStateProvider.NotifyUserAuthentication(_authStateProvider.GetAuthenticationStateAsync());
            
            _logger.LogInformation("パスワード変更成功: {Email}", userEmail);
            return UbiquitousLanguageManager.Contracts.DTOs.Authentication.ChangePasswordResponseDto.Success("パスワードを変更しました。");
        }
        else
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            _logger.LogWarning("パスワード変更失敗: {Email}, エラー: {Errors}", userEmail, errors);
            return UbiquitousLanguageManager.Contracts.DTOs.Authentication.ChangePasswordResponseDto.Error($"パスワード変更に失敗しました: {errors}");
        }
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "パスワード変更処理中にエラーが発生しました: {Email}", userEmail);
        return UbiquitousLanguageManager.Contracts.DTOs.Authentication.ChangePasswordResponseDto.Error("パスワード変更処理中にエラーが発生しました。管理者にお問い合わせください。");
    }
}

    /// <summary>
    /// Blazor用パスワード変更処理（現在のユーザー対象）
    /// </summary>
    /// <param name="currentPassword">現在のパスワード</param>
    /// <param name="newPassword">新しいパスワード</param>
    /// <returns>Result型でのパスワード変更結果</returns>
    public async Task<Microsoft.FSharp.Core.FSharpResult<string, string>> ChangePasswordAsync(string currentPassword, string newPassword)
    {
        try
        {
            // 現在の認証状態から現在のユーザーを取得
            var authState = await _authStateProvider.GetAuthenticationStateAsync();
            var userEmail = authState.User.Identity?.Name;

            if (string.IsNullOrEmpty(userEmail))
            {
                _logger.LogWarning("パスワード変更失敗: 現在のユーザーが特定できません");
                return Microsoft.FSharp.Core.FSharpResult<string, string>.NewError("認証情報の取得に失敗しました。再度ログインしてください。");
            }

            _logger.LogInformation("Blazor用パスワード変更処理開始: ユーザー={Email}", userEmail);

            // ユーザー検索
            var user = await _userManager.FindByEmailAsync(userEmail);
            if (user == null)
            {
                _logger.LogWarning("パスワード変更失敗: ユーザーが見つかりません {Email}", userEmail);
                return Microsoft.FSharp.Core.FSharpResult<string, string>.NewError("ユーザーが見つかりません。");
            }

            // ApplicationUserにキャスト
            if (user is not ApplicationUser appUser)
            {
                _logger.LogError("パスワード変更失敗: ApplicationUserキャストエラー {Email}", userEmail);
                return Microsoft.FSharp.Core.FSharpResult<string, string>.NewError("システムエラーが発生しました。");
            }

            // パスワード変更実行
            var result = await _userManager.ChangePasswordAsync(user, currentPassword, newPassword);
            
            if (result.Succeeded)
            {
                // 初回ログインフラグを更新（true → false）
                if (appUser.IsFirstLogin)
                {
                    appUser.IsFirstLogin = false;
                    appUser.UpdatedAt = DateTime.UtcNow;
                    appUser.UpdatedBy = userEmail;
                    
                    // 初期パスワードをクリア（セキュリティ強化）
                    appUser.InitialPassword = null;
                    
                    var updateResult = await _userManager.UpdateAsync(appUser);
                    if (!updateResult.Succeeded)
                    {
                        _logger.LogError("パスワード変更後のフラグ更新失敗: {Email}, エラー: {Errors}", 
                            userEmail, string.Join(", ", updateResult.Errors.Select(e => e.Description)));
                        return Microsoft.FSharp.Core.FSharpResult<string, string>.NewError("パスワード変更は成功しましたが、システム更新でエラーが発生しました。");
                    }
                    
                    _logger.LogInformation("初回ログインパスワード変更完了: {Email}", userEmail);
                }
                
                _logger.LogInformation("Blazor用パスワード変更成功: {Email}", userEmail);
                return Microsoft.FSharp.Core.FSharpResult<string, string>.NewOk("パスワードを変更しました。");
            }
            else
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                _logger.LogWarning("パスワード変更失敗: {Email}, エラー: {Errors}", userEmail, errors);
                return Microsoft.FSharp.Core.FSharpResult<string, string>.NewError($"パスワード変更に失敗しました: {errors}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Blazor用パスワード変更処理中にエラーが発生しました");
            return Microsoft.FSharp.Core.FSharpResult<string, string>.NewError("パスワード変更処理中にエラーが発生しました。管理者にお問い合わせください。");
        }
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
    /// <summary>
/// ApplicationUser（カスタムIdentityUser）での初回ログイン判定
/// </summary>
private static bool IsUserFirstLogin(IdentityUser user)
{
    // ApplicationUserにキャストしてIsFirstLoginフィールドを確認
    if (user is ApplicationUser appUser)
    {
        return appUser.IsFirstLogin;
    }
    
    // キャストに失敗した場合はfalse（安全側に倒す）
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