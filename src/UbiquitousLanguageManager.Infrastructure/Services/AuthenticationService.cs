using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.FSharp.Core;
using UbiquitousLanguageManager.Application;
using UbiquitousLanguageManager.Domain;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using System.Text;
using System.Text.Encodings.Web;

namespace UbiquitousLanguageManager.Infrastructure.Services;

/// <summary>
/// F# Application層IAuthenticationServiceの実装（Phase A3で本格実装予定）
/// 
/// 【F#初学者向け解説】
/// このクラスは、F#のApplication層で定義されたIAuthenticationServiceインターフェースを
/// C#のInfrastructure層で実装しています。Phase A2では基本スタブ実装とし、
/// Phase A3でASP.NET Core Identityとの本格統合を行います。
/// </summary>
public class AuthenticationService : IAuthenticationService
{
    private readonly Microsoft.Extensions.Logging.ILogger<AuthenticationService> _logger;
    private readonly UserManager<IdentityUser> _userManager;
    private readonly SignInManager<IdentityUser> _signInManager;
    private readonly INotificationService _notificationService;
    private readonly IUserRepository _userRepository;

    /// <summary>
    /// AuthenticationServiceのコンストラクタ
    /// </summary>
    /// <param name="logger">ログ出力サービス</param>
    /// <param name="userManager">ASP.NET Core Identity UserManager</param>
    /// <param name="signInManager">ASP.NET Core Identity SignInManager</param>
    /// <param name="notificationService">通知サービス</param>
    /// <param name="userRepository">ユーザーリポジトリ</param>
    public AuthenticationService(
        Microsoft.Extensions.Logging.ILogger<AuthenticationService> logger,
        UserManager<IdentityUser> userManager,
        SignInManager<IdentityUser> signInManager,
        INotificationService notificationService,
        IUserRepository userRepository)
    {
        _logger = logger;
        _userManager = userManager;
        _signInManager = signInManager;
        _notificationService = notificationService;
        _userRepository = userRepository;
    }

    /// <summary>
    /// Phase A3で実装予定：ログイン機能
    /// </summary>
    public async Task<FSharpResult<User, string>> LoginAsync(Email email, string password)
    {
        await Task.CompletedTask;
        _logger.LogInformation("LoginAsync called - Phase A3で実装予定");
        return FSharpResult<User, string>.NewError("Phase A3で実装予定");
    }

    /// <summary>
    /// Phase A3で実装予定：ユーザー作成
    /// </summary>
    public async Task<FSharpResult<User, string>> CreateUserWithPasswordAsync(
        Email email, UserName name, Role role, Password password, UserId createdBy)
    {
        await Task.CompletedTask;
        _logger.LogInformation("CreateUserWithPasswordAsync called - Phase A3で実装予定");
        return FSharpResult<User, string>.NewError("Phase A3で実装予定");
    }

    /// <summary>
    /// Phase A3で実装予定：パスワード変更
    /// </summary>
    public async Task<FSharpResult<PasswordHash, string>> ChangePasswordAsync(
        UserId userId, string oldPassword, Password newPassword)
    {
        await Task.CompletedTask;
        _logger.LogInformation("ChangePasswordAsync called - Phase A3で実装予定");
        return FSharpResult<PasswordHash, string>.NewError("Phase A3で実装予定");
    }

    /// <summary>
    /// Phase A3で実装予定：パスワードハッシュ生成
    /// </summary>
    public async Task<FSharpResult<PasswordHash, string>> HashPasswordAsync(Password password)
    {
        await Task.CompletedTask;
        _logger.LogInformation("HashPasswordAsync called - Phase A3で実装予定");
        return FSharpResult<PasswordHash, string>.NewError("Phase A3で実装予定");
    }

    /// <summary>
    /// Phase A3で実装予定：パスワード検証
    /// </summary>
    public async Task<FSharpResult<bool, string>> VerifyPasswordAsync(string password, PasswordHash hash)
    {
        await Task.CompletedTask;
        _logger.LogInformation("VerifyPasswordAsync called - Phase A3で実装予定");
        return FSharpResult<bool, string>.NewError("Phase A3で実装予定");
    }

    /// <summary>
    /// Phase A3で実装予定：トークン生成
    /// </summary>
    public async Task<FSharpResult<string, string>> GenerateTokenAsync(User user)
    {
        await Task.CompletedTask;
        _logger.LogInformation("GenerateTokenAsync called - Phase A3で実装予定");
        return FSharpResult<string, string>.NewError("Phase A3で実装予定");
    }

    /// <summary>
    /// Phase A3で実装予定：トークン検証
    /// </summary>
    public async Task<FSharpResult<User, string>> ValidateTokenAsync(string token)
    {
        await Task.CompletedTask;
        _logger.LogInformation("ValidateTokenAsync called - Phase A3で実装予定");
        return FSharpResult<User, string>.NewError("Phase A3で実装予定");
    }

    /// <summary>
    /// Phase A3で実装予定：ログイン失敗記録
    /// </summary>
    public async Task<FSharpResult<User, string>> RecordFailedLoginAsync(UserId userId)
    {
        await Task.CompletedTask;
        _logger.LogInformation("RecordFailedLoginAsync called - Phase A3で実装予定");
        return FSharpResult<User, string>.NewError("Phase A3で実装予定");
    }

    /// <summary>
    /// Phase A3で実装予定：ログイン成功記録
    /// </summary>
    public async Task<FSharpResult<User, string>> RecordSuccessfulLoginAsync(UserId userId)
    {
        await Task.CompletedTask;
        _logger.LogInformation("RecordSuccessfulLoginAsync called - Phase A3で実装予定");
        return FSharpResult<User, string>.NewError("Phase A3で実装予定");
    }

    /// <summary>
    /// Phase A3で実装予定：ユーザーロック
    /// </summary>
    public async Task<FSharpResult<Unit, string>> LockUserAsync(UserId userId, DateTime lockoutEnd)
    {
        await Task.CompletedTask;
        _logger.LogInformation("LockUserAsync called - Phase A3で実装予定");
        return FSharpResult<Unit, string>.NewError("Phase A3で実装予定");
    }

    /// <summary>
    /// Phase A3で実装予定：ユーザーロック解除
    /// </summary>
    public async Task<FSharpResult<Unit, string>> UnlockUserAsync(UserId userId)
    {
        await Task.CompletedTask;
        _logger.LogInformation("UnlockUserAsync called - Phase A3で実装予定");
        return FSharpResult<Unit, string>.NewError("Phase A3で実装予定");
    }

    /// <summary>
    /// Phase A3で実装予定：セキュリティスタンプ更新
    /// </summary>
    public async Task<FSharpResult<Unit, string>> UpdateSecurityStampAsync(UserId userId)
    {
        await Task.CompletedTask;
        _logger.LogInformation("UpdateSecurityStampAsync called - Phase A3で実装予定");
        return FSharpResult<Unit, string>.NewError("Phase A3で実装予定");
    }

    /// <summary>
    /// Phase A3で実装予定：メール確認送信
    /// </summary>
    public async Task<FSharpResult<Unit, string>> SendEmailConfirmationAsync(Email email)
    {
        await Task.CompletedTask;
        _logger.LogInformation("SendEmailConfirmationAsync called - Phase A3で実装予定");
        return FSharpResult<Unit, string>.NewError("Phase A3で実装予定");
    }

    /// <summary>
    /// Phase A3で実装予定：メール確認
    /// </summary>
    public async Task<FSharpResult<Unit, string>> ConfirmEmailAsync(UserId userId, string confirmationToken)
    {
        await Task.CompletedTask;
        _logger.LogInformation("ConfirmEmailAsync called - Phase A3で実装予定");
        return FSharpResult<Unit, string>.NewError("Phase A3で実装予定");
    }

    /// <summary>
    /// Phase A3で実装予定：二要素認証有効化
    /// </summary>
    public async Task<FSharpResult<string, string>> EnableTwoFactorAsync(UserId userId)
    {
        await Task.CompletedTask;
        _logger.LogInformation("EnableTwoFactorAsync called - Phase A3で実装予定");
        return FSharpResult<string, string>.NewError("Phase A3で実装予定");
    }

    /// <summary>
    /// Phase A3で実装予定：二要素認証無効化
    /// </summary>
    public async Task<FSharpResult<Unit, string>> DisableTwoFactorAsync(UserId userId)
    {
        await Task.CompletedTask;
        _logger.LogInformation("DisableTwoFactorAsync called - Phase A3で実装予定");
        return FSharpResult<Unit, string>.NewError("Phase A3で実装予定");
    }

    /// <summary>
    /// Phase A3で実装予定：二要素認証コード検証
    /// </summary>
    public async Task<FSharpResult<bool, string>> VerifyTwoFactorCodeAsync(UserId userId, string code)
    {
        await Task.CompletedTask;
        _logger.LogInformation("VerifyTwoFactorCodeAsync called - Phase A3で実装予定");
        return FSharpResult<bool, string>.NewError("Phase A3で実装予定");
    }

    /// <summary>
    /// Phase A3で実装予定：現在ユーザー取得
    /// </summary>
    public async Task<FSharpResult<FSharpOption<User>, string>> GetCurrentUserAsync()
    {
        await Task.CompletedTask;
        _logger.LogInformation("GetCurrentUserAsync called - Phase A3で実装予定");
        return FSharpResult<FSharpOption<User>, string>.NewError("Phase A3で実装予定");
    }

    // 🔄 Phase A3: パスワードリセット機能実装
    // 【F#初学者向け解説】
    // ASP.NET Core Identity の UserManager を使用して、セキュアなパスワードリセット機能を実装します。
    // トークンベースの認証により、メールアドレスの所有者のみがパスワードを変更できる仕組みです。

    /// <summary>
    /// パスワードリセット要求処理
    /// </summary>
    /// <param name="email">リセット要求するメールアドレス</param>
    /// <returns>処理結果（成功時はunit、失敗時はエラーメッセージ）</returns>
    /// <remarks>
    /// 🔐 セキュリティ考慮: 存在しないユーザーでも成功レスポンスを返すことで
    /// アカウント列挙攻撃を防止します。
    /// 📊 ADR_008準拠: セキュリティログ出力
    /// </remarks>
    public async Task<FSharpResult<Unit, string>> RequestPasswordResetAsync(Email email)
    {
        try
        {
            // 📊 ADR_008準拠: セキュリティイベントログ（Information レベル）
            _logger.LogInformation("Password reset requested. Email: {Email}", email.Value);

            // 🔍 ASP.NET Core Identity: メールアドレスでユーザー検索
            var identityUser = await _userManager.FindByEmailAsync(email.Value);
            
            if (identityUser == null)
            {
                // 🔐 セキュリティ: 存在しないユーザーでも成功として扱う（アカウント列挙攻撃対策）
                _logger.LogWarning("Password reset requested for non-existent user. Email: {Email}", email.Value);
                
                // セキュリティ上、成功として返すが実際のメール送信は行わない
                return FSharpResult<Unit, string>.NewOk(null!);
            }

            // 🎯 メール確認済みユーザーのみリセット許可
            if (!await _userManager.IsEmailConfirmedAsync(identityUser))
            {
                _logger.LogWarning("Password reset requested for unconfirmed email. Email: {Email}", email.Value);
                // セキュリティ上、詳細は返さず一般的なメッセージ
                return FSharpResult<Unit, string>.NewError("メール確認が完了していないアカウントです。");
            }

            // 🔑 ASP.NET Core Identity: パスワードリセットトークン生成
            // TokenProvider（デフォルト24時間有効）を使用
            var resetToken = await _userManager.GeneratePasswordResetTokenAsync(identityUser);

            // 📧 URL安全なトークン: Base64UrlEncode で安全にエンコード
            var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(resetToken));
            
            // 🌐 リセットURL生成
            var resetUrl = $"https://localhost:5001/account/reset-password?email={Uri.EscapeDataString(email.Value)}&token={encodedToken}";

            // 📮 Step2で実装したメール送信基盤を活用
            var notificationResult = await _notificationService.SendPasswordResetEmailAsync(email, encodedToken, resetUrl);

            if (notificationResult.IsError)
            {
                // ❌ メール送信失敗
                _logger.LogError("Failed to send password reset email. Email: {Email}, Error: {Error}", 
                    email.Value, notificationResult.ErrorValue);
                return FSharpResult<Unit, string>.NewError("メール送信中にエラーが発生しました。しばらく待ってから再試行してください。");
            }

            // ✅ 成功ログ: セキュリティ監査用
            _logger.LogInformation("Password reset email sent successfully. Email: {Email}", email.Value);

            return FSharpResult<Unit, string>.NewOk(null!);
        }
        catch (Exception ex)
        {
            // ❌ 予期しないエラー: ADR_008準拠（Error レベル）
            _logger.LogError(ex, "Unexpected error during password reset request. Email: {Email}", email.Value);
            return FSharpResult<Unit, string>.NewError("システムエラーが発生しました。管理者にお問い合わせください。");
        }
    }

    /// <summary>
    /// パスワードリセット実行処理
    /// </summary>
    /// <param name="email">対象ユーザーのメールアドレス</param>
    /// <param name="token">パスワードリセットトークン</param>
    /// <param name="newPassword">新しいパスワード</param>
    /// <returns>処理結果（成功時はunit、失敗時はエラーメッセージ）</returns>
    /// <remarks>
    /// 🔍 トークン検証・有効期限チェック・パスワード複雑性チェックを実施
    /// 📊 ADR_008準拠: セキュリティログ出力
    /// </remarks>
    public async Task<FSharpResult<Unit, string>> ResetPasswordAsync(Email email, string token, Password newPassword)
    {
        try
        {
            // 📊 ADR_008準拠: セキュリティイベントログ（Information レベル）
            _logger.LogInformation("Password reset attempt. Email: {Email}", email.Value);

            // 🔍 ユーザー存在確認
            var identityUser = await _userManager.FindByEmailAsync(email.Value);
            if (identityUser == null)
            {
                _logger.LogWarning("Password reset attempted for non-existent user. Email: {Email}", email.Value);
                return FSharpResult<Unit, string>.NewError("無効なリセット要求です。");
            }

            // 🔐 トークンデコード: Base64UrlDecode
            string decodedToken;
            try
            {
                decodedToken = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(token));
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Invalid token format in password reset. Email: {Email}", email.Value);
                return FSharpResult<Unit, string>.NewError("無効または期限切れのリセットトークンです。");
            }

            // 🔑 ASP.NET Core Identity: パスワードリセット実行
            // UserManager が自動的にトークン有効性・有効期限をチェック
            var resetResult = await _userManager.ResetPasswordAsync(identityUser, decodedToken, newPassword.Value);

            if (!resetResult.Succeeded)
            {
                // ❌ リセット失敗: トークン無効・期限切れ・パスワード要件不適合等
                var errors = string.Join(", ", resetResult.Errors.Select(e => e.Description));
                _logger.LogWarning("Password reset failed. Email: {Email}, Errors: {Errors}", email.Value, errors);
                
                // 🎯 ADR_007準拠: ユーザーフレンドリーなエラーメッセージ変換
                var userFriendlyError = resetResult.Errors.Any(e => e.Code.Contains("InvalidToken")) 
                    ? "リセットトークンが無効または期限切れです。新しいリセット要求を行ってください。"
                    : "パスワード設定中にエラーが発生しました。パスワード要件を確認してください。";

                return FSharpResult<Unit, string>.NewError(userFriendlyError);
            }

            // 🔄 セキュリティスタンプ更新: 既存セッションの無効化
            await _userManager.UpdateSecurityStampAsync(identityUser);

            // 📮 パスワード変更完了通知メール送信
            var notificationResult = await _notificationService.SendPasswordResetConfirmationAsync(email);
            if (notificationResult.IsError)
            {
                // ⚠️ 通知失敗は警告レベル（パスワード変更自体は成功）
                _logger.LogWarning("Failed to send password reset confirmation email. Email: {Email}", email.Value);
            }

            // ✅ 成功ログ: セキュリティ監査用
            _logger.LogInformation("Password reset completed successfully. Email: {Email}", email.Value);

            return FSharpResult<Unit, string>.NewOk(null!);
        }
        catch (Exception ex)
        {
            // ❌ 予期しないエラー: ADR_008準拠（Error レベル）
            _logger.LogError(ex, "Unexpected error during password reset. Email: {Email}", email.Value);
            return FSharpResult<Unit, string>.NewError("システムエラーが発生しました。管理者にお問い合わせください。");
        }
    }

    /// <summary>
    /// パスワードリセットトークン検証処理
    /// </summary>
    /// <param name="email">対象ユーザーのメールアドレス</param>
    /// <param name="token">検証するトークン</param>
    /// <returns>検証結果（true: 有効, false: 無効）</returns>
    /// <remarks>
    /// 🌐 UI側でのトークン有効性確認用メソッド
    /// フォーム表示前にトークンの妥当性をチェック
    /// </remarks>
    public async Task<FSharpResult<bool, string>> ValidatePasswordResetTokenAsync(Email email, string token)
    {
        try
        {
            // 📊 Debug レベルログ（頻繁に呼ばれる可能性があるため）
            _logger.LogDebug("Password reset token validation. Email: {Email}", email.Value);

            // 🔍 ユーザー存在確認
            var identityUser = await _userManager.FindByEmailAsync(email.Value);
            if (identityUser == null)
            {
                return FSharpResult<bool, string>.NewOk(false);
            }

            // 🔐 トークンデコード
            string decodedToken;
            try
            {
                decodedToken = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(token));
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex, "Token decode failed in validation. Email: {Email}", email.Value);
                return FSharpResult<bool, string>.NewOk(false);
            }

            // 🔍 ASP.NET Core Identity: トークン有効性検証
            // 実際のパスワード変更は行わず、トークンの妥当性のみチェック
            var isValid = await _userManager.VerifyUserTokenAsync(
                identityUser, 
                _userManager.Options.Tokens.PasswordResetTokenProvider, 
                "ResetPassword", 
                decodedToken);

            return FSharpResult<bool, string>.NewOk(isValid);
        }
        catch (Exception ex)
        {
            // ❌ 検証エラー: Debug レベル（UI用の補助メソッドのため）
            _logger.LogDebug(ex, "Error during token validation. Email: {Email}", email.Value);
            return FSharpResult<bool, string>.NewOk(false);
        }
    }

    // 🔄 Phase A3: Step4 - 自動ログイン・基本セキュリティ機能実装
    // 【F#初学者向け解説】
    // Step4で実装する基本的なセキュリティ機能とユーザビリティ向上機能です。
    // SignInManagerを活用したセキュアな自動ログイン、基本監査ログ、Identity Lockout統合を提供します。

    /// <summary>
    /// パスワードリセット完了後の自動ログイン実行
    /// </summary>
    /// <param name="email">自動ログインするユーザーのメールアドレス</param>
    /// <returns>処理結果（成功時はunit、失敗時はエラーメッセージ）</returns>
    /// <remarks>
    /// 🔐 SignInManager.SignInAsync使用による安全な自動ログイン
    /// 📊 ADR_008準拠: 自動ログイン成功/失敗ログ出力
    /// </remarks>
    public async Task<FSharpResult<Unit, string>> AutoLoginAfterPasswordResetAsync(Email email)
    {
        try
        {
            // 📊 ADR_008準拠: 自動ログイン開始ログ（Information レベル）
            _logger.LogInformation("Auto login after password reset initiated. Email: {Email}", email.Value);

            // 🔍 ユーザー存在確認・ロック状態確認
            var identityUser = await _userManager.FindByEmailAsync(email.Value);
            if (identityUser == null)
            {
                _logger.LogWarning("Auto login attempted for non-existent user. Email: {Email}", email.Value);
                return FSharpResult<Unit, string>.NewError("ユーザーが見つかりません。");
            }

            // 🔒 Identity Lockout状態確認
            if (await _userManager.IsLockedOutAsync(identityUser))
            {
                _logger.LogWarning("Auto login attempted for locked user. Email: {Email}", email.Value);
                return FSharpResult<Unit, string>.NewError("アカウントがロックされています。");
            }

            // 🔐 SignInManager使用による安全な自動ログイン実行
            // isPersistent: false（セッションベース、Remember Meは別途処理）
            await _signInManager.SignInAsync(identityUser, isPersistent: false);

            // 📊 ログイン成功記録
            await RecordLoginAttemptAsync(email, true);

            // ✅ 成功ログ: セキュリティ監査用
            _logger.LogInformation("Auto login completed successfully. Email: {Email}", email.Value);

            return FSharpResult<Unit, string>.NewOk(null!);
        }
        catch (Exception ex)
        {
            // ❌ 予期しないエラー: ADR_008準拠（Error レベル）
            _logger.LogError(ex, "Unexpected error during auto login. Email: {Email}", email.Value);
            return FSharpResult<Unit, string>.NewError("自動ログインでシステムエラーが発生しました。");
        }
    }

    /// <summary>
    /// ログイン試行記録処理
    /// </summary>
    /// <param name="email">対象ユーザーのメールアドレス</param>
    /// <param name="isSuccess">ログイン成功/失敗フラグ</param>
    /// <returns>処理結果（成功時はunit、失敗時はエラーメッセージ）</returns>
    /// <remarks>
    /// 📊 基本監査ログ出力とIdentity Lockout連携
    /// 📋 ADR_008準拠: 構造化ログ出力
    /// </remarks>
    public async Task<FSharpResult<Unit, string>> RecordLoginAttemptAsync(Email email, bool isSuccess)
    {
        try
        {
            var statusText = isSuccess ? "Success" : "Failed";
            var logLevel = isSuccess ? LogLevel.Information : LogLevel.Warning;

            // 📊 ADR_008準拠: 基本監査ログ出力
            _logger.Log(logLevel, "Login attempt recorded. Email: {Email}, Status: {Status}, Timestamp: {Timestamp}", 
                email.Value, statusText, DateTimeOffset.UtcNow);

            // 🔒 失敗時のIdentity Lockout処理
            if (!isSuccess)
            {
                var identityUser = await _userManager.FindByEmailAsync(email.Value);
                if (identityUser != null)
                {
                    // ASP.NET Core IdentityのAccessFailedAsync使用でLockout自動処理
                    await _userManager.AccessFailedAsync(identityUser);
                    
                    // ロック状態確認・ログ出力
                    if (await _userManager.IsLockedOutAsync(identityUser))
                    {
                        var lockoutEnd = await _userManager.GetLockoutEndDateAsync(identityUser);
                        _logger.LogWarning("User account locked due to failed login attempts. Email: {Email}, LockoutEnd: {LockoutEnd}", 
                            email.Value, lockoutEnd);
                    }
                }
            }
            else
            {
                // ✅ 成功時の失敗カウントリセット
                var identityUser = await _userManager.FindByEmailAsync(email.Value);
                if (identityUser != null)
                {
                    await _userManager.ResetAccessFailedCountAsync(identityUser);
                }
            }

            return FSharpResult<Unit, string>.NewOk(null!);
        }
        catch (Exception ex)
        {
            // ❌ 予期しないエラー: ADR_008準拠（Error レベル）
            _logger.LogError(ex, "Error during login attempt recording. Email: {Email}", email.Value);
            return FSharpResult<Unit, string>.NewError($"ログイン試行記録中にエラーが発生しました: {ex.Message}");
        }
    }

    /// <summary>
    /// アカウントロック状態確認
    /// </summary>
    /// <param name="email">確認対象ユーザーのメールアドレス</param>
    /// <returns>ロック状態（true: ロック中, false: 正常）</returns>
    /// <remarks>
    /// 🔒 UI表示用のアカウントロック状態取得
    /// 📊 Identity Lockout統合
    /// </remarks>
    public async Task<FSharpResult<bool, string>> IsAccountLockedAsync(Email email)
    {
        try
        {
            // 📊 Debug レベルログ（頻繁に呼ばれる可能性があるため）
            _logger.LogDebug("Account lock status check. Email: {Email}", email.Value);

            // 🔍 ユーザー存在確認
            var identityUser = await _userManager.FindByEmailAsync(email.Value);
            if (identityUser == null)
            {
                return FSharpResult<bool, string>.NewOk(false);
            }

            // 🔒 Identity Lockout状態確認
            var isLocked = await _userManager.IsLockedOutAsync(identityUser);

            return FSharpResult<bool, string>.NewOk(isLocked);
        }
        catch (Exception ex)
        {
            // ❌ エラー: Debug レベル（UI用の補助メソッドのため）
            _logger.LogDebug(ex, "Error during account lock status check. Email: {Email}", email.Value);
            return FSharpResult<bool, string>.NewOk(false);
        }
    }
}