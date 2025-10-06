using Microsoft.AspNetCore.Components.Authorization;
using UbiquitousLanguageManager.Application;
using UbiquitousLanguageManager.Web.Authentication;
using UbiquitousLanguageManager.Domain.Common;
using UbiquitousLanguageManager.Domain.Authentication;

namespace UbiquitousLanguageManager.Web.Services;

/// <summary>
/// Blazor Server専用認証サービス（Phase A9 薄いラッパー層）
/// 
/// 【Phase A9重複実装統一効果】
/// - Infrastructure層委譲：認証基盤サービス統一・重複削除
/// - 薄いラッパー層設計：Blazor Server固有機能の最小実装
/// - 保守負荷削減：重複実装解消による50%削減効果
/// 
/// 【Blazor Server初学者向け解説】
/// Infrastructure層の統一認証サービスをBlazor Server環境に適応
/// - Infrastructure層委譲：ASP.NET Core Identity完全統合・InitialPassword対応
/// - Blazor Server適応：認証状態プロバイダー統合・UI固有処理
/// - 薄いラッパー設計：最小限のBlazor固有機能のみ実装
/// </summary>
public class BlazorAuthenticationService
{
    private readonly IAuthenticationService _authenticationService;
    private readonly CustomAuthenticationStateProvider _authStateProvider;
    private readonly Microsoft.Extensions.Logging.ILogger<BlazorAuthenticationService> _logger;

    /// <summary>
    /// コンストラクタ - Infrastructure層委譲・認証状態プロバイダー統合
    /// </summary>
    /// <param name="authenticationService">Application層認証サービスインターフェース</param>
    /// <param name="authStateProvider">Blazor Server認証状態プロバイダー</param>
    /// <param name="logger">ロガー</param>
    public BlazorAuthenticationService(
        IAuthenticationService authenticationService,
        CustomAuthenticationStateProvider authStateProvider,
        Microsoft.Extensions.Logging.ILogger<BlazorAuthenticationService> logger)
    {
        _authenticationService = authenticationService;
        _authStateProvider = authStateProvider;
        _logger = logger;
    }

    /// <summary>
    /// Blazor Server用ログアウト処理（Infrastructure層委譲・認証状態通知）
    /// </summary>
    public async Task LogoutAsync()
    {
        try
        {
            _logger.LogInformation("Blazor Server ログアウト処理開始（Infrastructure層委譲）");

            // Application層認証サービスへ委譲（Infrastructure層統一認証処理）
            await _authenticationService.LogoutAsync();

            // Blazor認証状態の変更を通知
            _authStateProvider.NotifyUserLogout();
            
            _logger.LogInformation("Blazor Server ログアウト処理完了");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Blazor Server ログアウト処理中にエラーが発生");
            throw; // 呼び出し元でエラーハンドリングできるよう再throw
        }
    }

    /// <summary>
    /// 現在のドメインユーザーID取得（認証状態プロバイダー経由）
    /// </summary>
    /// <returns>Domain UserId またはNull</returns>
    public long? GetCurrentDomainUserId()
    {
        return _authStateProvider.GetCurrentDomainUserId();
    }

    /// <summary>
    /// 現在のユーザーがアクティブかどうかを確認
    /// </summary>
    /// <returns>アクティブフラグ</returns>
    public bool IsCurrentUserActive()
    {
        return _authStateProvider.IsCurrentUserActive();
    }

    /// <summary>
    /// 現在のユーザーが初回ログインかどうかを確認
    /// </summary>
    /// <returns>初回ログインフラグ</returns>
    public bool IsCurrentUserFirstLogin()
    {
        return _authStateProvider.IsCurrentUserFirstLogin();
    }

    /// <summary>
    /// Blazor Server用パスワード変更処理（Infrastructure層委譲）
    /// </summary>
    /// <param name="currentPassword">現在のパスワード</param>
    /// <param name="newPassword">新しいパスワード</param>
    /// <returns>Result型でのパスワード変更結果</returns>
    public async Task<Microsoft.FSharp.Core.FSharpResult<string, string>> ChangePasswordAsync(
        string currentPassword, string newPassword)
    {
        try
        {
            // 現在の認証状態から現在のユーザーを取得
            var authState = await _authStateProvider.GetAuthenticationStateAsync();
            var userEmail = authState.User.Identity?.Name;

            if (string.IsNullOrEmpty(userEmail))
            {
                _logger.LogWarning("Blazor Server パスワード変更失敗: 現在のユーザーが特定できません");
                return Microsoft.FSharp.Core.FSharpResult<string, string>.NewError(
                    "認証情報の取得に失敗しました。再度ログインしてください。");
            }

            _logger.LogInformation("Blazor Server パスワード変更処理開始（Infrastructure層委譲）: {Email}", userEmail);

            // 現在のドメインユーザーIDを取得
            var domainUserId = GetCurrentDomainUserId();

            if (!domainUserId.HasValue)
            {
                _logger.LogWarning("Blazor Server パスワード変更失敗: ドメインユーザーIDが取得できません");
                return Microsoft.FSharp.Core.FSharpResult<string, string>.NewError(
                    "認証情報の取得に失敗しました。再度ログインしてください。");
            }

            // newPasswordをPassword値オブジェクトに変換
            var passwordResult = Password.create(newPassword);
            if (passwordResult.IsError)
            {
                _logger.LogWarning("Blazor Server パスワード変更失敗: パスワード形式不正 - {Email}", userEmail);
                return Microsoft.FSharp.Core.FSharpResult<string, string>.NewError(
                    passwordResult.ErrorValue);
            }

            var userId = UserId.NewUserId(domainUserId.Value);

            // Application層認証サービスへ委譲
            var result = await _authenticationService.ChangePasswordAsync(
                userId, currentPassword, passwordResult.ResultValue);

            // ChangePasswordAsyncはPasswordHashを返すため、文字列結果に変換
            return result.IsOk
                ? Microsoft.FSharp.Core.FSharpResult<string, string>.NewOk("パスワードが正常に変更されました。")
                : Microsoft.FSharp.Core.FSharpResult<string, string>.NewError(result.ErrorValue);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Blazor Server パスワード変更処理中にエラーが発生");
            return Microsoft.FSharp.Core.FSharpResult<string, string>.NewError(
                "パスワード変更処理中にエラーが発生しました。管理者にお問い合わせください。");
        }
    }
}