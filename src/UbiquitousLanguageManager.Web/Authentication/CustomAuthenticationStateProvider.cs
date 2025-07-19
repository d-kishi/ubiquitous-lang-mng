using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using System.Security.Claims;
using UbiquitousLanguageManager.Contracts.DTOs.Authentication;

namespace UbiquitousLanguageManager.Web.Authentication;

/// <summary>
/// カスタム認証状態プロバイダー
/// 
/// 【Blazor Server初学者向け解説】
/// AuthenticationStateProviderは、Blazorアプリ全体で認証状態を管理するクラスです。
/// Blazor Serverでは、SignalR接続を通じてリアルタイムに認証状態が同期されます。
/// このクラスにより、コンポーネント間で一貫した認証情報が共有されます。
/// </summary>
public class CustomAuthenticationStateProvider : AuthenticationStateProvider
{
    private readonly ProtectedSessionStorage _sessionStorage;
    private ClaimsPrincipal _currentUser = new(new ClaimsIdentity());

    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="sessionStorage">保護されたセッションストレージ</param>
    public CustomAuthenticationStateProvider(ProtectedSessionStorage sessionStorage)
    {
        _sessionStorage = sessionStorage;
    }

    /// <summary>
    /// 現在の認証状態を取得
    /// 
    /// 【Blazor Server初学者向け解説】
    /// このメソッドは、Blazorフレームワークから自動的に呼び出されます。
    /// 認証が必要なページにアクセスした際や、@context Task等で認証状態を確認する際に実行されます。
    /// </summary>
    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        try
        {
            // セッションストレージから認証情報を取得
            var userDataResult = await _sessionStorage.GetAsync<AuthenticatedUserDto>("user");
            
            if (userDataResult.Success && userDataResult.Value != null)
            {
                var userData = userDataResult.Value;
                var identity = CreateClaimsIdentity(userData);
                _currentUser = new ClaimsPrincipal(identity);
            }
            else
            {
                _currentUser = new ClaimsPrincipal(new ClaimsIdentity());
            }
        }
        catch
        {
            // セッションストレージアクセスエラー時は未認証とする
            _currentUser = new ClaimsPrincipal(new ClaimsIdentity());
        }

        return new AuthenticationState(_currentUser);
    }

    /// <summary>
    /// ログイン処理
    /// 
    /// 【セキュリティ注意】
    /// このメソッドは実際の認証処理は行いません。
    /// 認証は別途ASP.NET Core Identityで行い、成功後にこのメソッドでUI状態を更新します。
    /// </summary>
    public async Task LoginAsync(AuthenticatedUserDto userData)
    {
        // セッションストレージに認証情報を保存
        await _sessionStorage.SetAsync("user", userData);

        // Claims Identityを作成
        var identity = CreateClaimsIdentity(userData);
        _currentUser = new ClaimsPrincipal(identity);

        // 認証状態変更を通知
        // 【Blazor Server初学者向け解説】
        // NotifyAuthenticationStateChangedにより、認証状態に依存する全てのコンポーネントが自動的に再レンダリングされます。
        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(_currentUser)));
    }

    /// <summary>
    /// ログアウト処理
    /// </summary>
    public async Task LogoutAsync()
    {
        // セッションストレージから認証情報を削除
        await _sessionStorage.DeleteAsync("user");

        // 未認証状態に設定
        _currentUser = new ClaimsPrincipal(new ClaimsIdentity());

        // 認証状態変更を通知
        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(_currentUser)));
    }

    /// <summary>
    /// Claims Identityの作成
    /// 
    /// 【ASP.NET Core初学者向け解説】
    /// Claimsは、ユーザーの属性情報（名前、ロール等）を表現するKey-Valueペアです。
    /// 認証・認可システムで、ユーザーの権限判定に使用されます。
    /// </summary>
    private static ClaimsIdentity CreateClaimsIdentity(AuthenticatedUserDto userData)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userData.Id.ToString()),
            new(ClaimTypes.Name, userData.Name),
            new(ClaimTypes.Email, userData.Email),
            new(ClaimTypes.Role, userData.Role),
            new("IsFirstLogin", userData.IsFirstLogin.ToString()),
            new("IsActive", userData.IsActive.ToString())
        };

        return new ClaimsIdentity(claims, "custom");
    }

    /// <summary>
    /// 現在のユーザー情報を取得
    /// </summary>
    public async Task<AuthenticatedUserDto?> GetCurrentUserAsync()
    {
        var userDataResult = await _sessionStorage.GetAsync<AuthenticatedUserDto>("user");
        return userDataResult.Success ? userDataResult.Value : null;
    }

    /// <summary>
    /// ユーザー情報の更新
    /// 例：パスワード変更後のIsFirstLoginフラグクリア等
    /// </summary>
    public async Task UpdateUserAsync(AuthenticatedUserDto userData)
    {
        await _sessionStorage.SetAsync("user", userData);
        
        var identity = CreateClaimsIdentity(userData);
        _currentUser = new ClaimsPrincipal(identity);
        
        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(_currentUser)));
    }
}