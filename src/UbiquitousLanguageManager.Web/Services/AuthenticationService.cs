using Microsoft.AspNetCore.Identity;
using Microsoft.FSharp.Core;
using UbiquitousLanguageManager.Application;
using UbiquitousLanguageManager.Contracts.DTOs.Authentication;
using UbiquitousLanguageManager.Contracts.Converters;
using UbiquitousLanguageManager.Web.Authentication;

namespace UbiquitousLanguageManager.Web.Services;

/// <summary>
/// Web層認証サービス
/// 
/// 【アーキテクチャ説明】
/// このサービスは、Blazor ServerコンポーネントとApplication層の間の橋渡しを行います。
/// ASP.NET Core IdentityとF#のドメインロジックを統合し、一貫した認証フローを提供します。
/// </summary>
public class AuthenticationService
{
    private readonly ApplicationUseCases _applicationUseCases;
    private readonly CustomAuthenticationStateProvider _authStateProvider;
    private readonly SignInManager<Infrastructure.Data.Entities.ApplicationUser> _signInManager;
    private readonly UserManager<Infrastructure.Data.Entities.ApplicationUser> _userManager;
    private readonly ILogger<AuthenticationService> _logger;

    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="applicationUseCases">アプリケーションユースケース</param>
    /// <param name="authStateProvider">認証状態プロバイダー</param>
    /// <param name="signInManager">サインイン管理</param>
    /// <param name="userManager">ユーザー管理</param>
    /// <param name="logger">ロガー</param>
    public AuthenticationService(
        ApplicationUseCases applicationUseCases,
        CustomAuthenticationStateProvider authStateProvider,
        SignInManager<Infrastructure.Data.Entities.ApplicationUser> signInManager,
        UserManager<Infrastructure.Data.Entities.ApplicationUser> userManager,
        ILogger<AuthenticationService> logger)
    {
        _applicationUseCases = applicationUseCases;
        _authStateProvider = authStateProvider;
        _signInManager = signInManager;
        _userManager = userManager;
        _logger = logger;
    }

    /// <summary>
    /// ログイン処理
    /// 
    /// 【Blazor Server初学者向け解説】
    /// 1. F#のApplication層で認証ロジックを実行
    /// 2. 成功時はASP.NET Core IdentityでCookie認証を設定
    /// 3. CustomAuthenticationStateProviderで認証状態をBlazorに通知
    /// このフローにより、サーバーサイドとクライアントサイドの認証状態が同期されます。
    /// </summary>
    public async Task<LoginResponseDto> LoginAsync(LoginRequestDto request)
    {
        try
        {
            // F#のApplication層でドメインロジック実行
            var loginCommand = AuthenticationMapper.ToLoginCommand(request);
            var result = await _applicationUseCases.UserManagement.LoginAsync(loginCommand);

            if (result.IsSuccess && FSharpOption<Domain.User>.get_IsSome(result.Data))
            {
                var user = result.Data.Value;
                
                // ASP.NET Core IdentityでCookie認証設定
                var applicationUser = await _userManager.FindByEmailAsync(user.Email.Value);
                if (applicationUser != null)
                {
                    await _signInManager.SignInAsync(applicationUser, request.RememberMe);
                    
                    // Blazor認証状態更新
                    var userDto = AuthenticationMapper.ToAuthenticatedUserDto(user);
                    await _authStateProvider.LoginAsync(userDto);

                    _logger.LogInformation("ユーザーがログインしました: {Email}", user.Email.Value);
                    
                    return LoginResponseDto.Success(userDto, user.IsFirstLogin);
                }
                else
                {
                    _logger.LogWarning("F#ドメインとIdentityユーザーの同期エラー: {Email}", user.Email.Value);
                    return LoginResponseDto.Error("認証システムエラーが発生しました");
                }
            }
            else
            {
                var errorMessage = FSharpOption<string>.get_IsSome(result.ErrorMessage) ? result.ErrorMessage.Value : "ログインに失敗しました";
                _logger.LogWarning("ログイン失敗: {Email}, {Error}", request.Email, errorMessage);
                return LoginResponseDto.Error(errorMessage);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "ログイン処理中にエラーが発生しました: {Email}", request.Email);
            return LoginResponseDto.Error("システムエラーが発生しました");
        }
    }

    /// <summary>
    /// ログアウト処理
    /// </summary>
    public async Task LogoutAsync()
    {
        try
        {
            // ASP.NET Core Identity認証解除
            await _signInManager.SignOutAsync();
            
            // Blazor認証状態クリア
            await _authStateProvider.LogoutAsync();
            
            _logger.LogInformation("ユーザーがログアウトしました");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "ログアウト処理中にエラーが発生しました");
        }
    }

    /// <summary>
    /// パスワード変更処理
    /// </summary>
    public async Task<LoginResponseDto> ChangePasswordAsync(ChangePasswordRequestDto request)
    {
        try
        {
            // 現在のユーザー情報取得
            var currentUser = await _authStateProvider.GetCurrentUserAsync();
            if (currentUser == null)
            {
                return LoginResponseDto.Error("認証されていません");
            }

            // F#のApplication層でパスワード変更実行
            var changePasswordCommand = AuthenticationMapper.ToChangePasswordCommand(request, currentUser.Id);
            var result = await _applicationUseCases.UserManagement.ChangePasswordAsync(changePasswordCommand);

            if (result.IsSuccess)
            {
                // 初回ログインフラグをクリア
                var updatedUser = new AuthenticatedUserDto
                {
                    Id = currentUser.Id,
                    Email = currentUser.Email,
                    Name = currentUser.Name,
                    Role = currentUser.Role,
                    IsActive = currentUser.IsActive,
                    IsFirstLogin = false,
                    UpdatedAt = currentUser.UpdatedAt
                };
                await _authStateProvider.UpdateUserAsync(updatedUser);

                _logger.LogInformation("パスワードが変更されました: {UserId}", currentUser.Id);
                
                return LoginResponseDto.Success(updatedUser);
            }
            else
            {
                var errorMessage = FSharpOption<string>.get_IsSome(result.ErrorMessage) ? result.ErrorMessage.Value : "パスワード変更に失敗しました";
                _logger.LogWarning("パスワード変更失敗: {UserId}, {Error}", currentUser.Id, errorMessage);
                return LoginResponseDto.Error(errorMessage);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "パスワード変更処理中にエラーが発生しました");
            return LoginResponseDto.Error("システムエラーが発生しました");
        }
    }

    /// <summary>
    /// 現在のユーザー情報取得
    /// </summary>
    public async Task<AuthenticatedUserDto?> GetCurrentUserAsync()
    {
        return await _authStateProvider.GetCurrentUserAsync();
    }
}