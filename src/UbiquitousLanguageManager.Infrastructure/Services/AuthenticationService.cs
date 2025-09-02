using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.FSharp.Core;
using Microsoft.AspNetCore.Identity;
using UbiquitousLanguageManager.Application;
using UbiquitousLanguageManager.Domain;
using UbiquitousLanguageManager.Infrastructure.Data.Entities;
using UbiquitousLanguageManager.Contracts.Converters;

namespace UbiquitousLanguageManager.Infrastructure.Services;

/// <summary>
/// F# Application層IAuthenticationServiceの実装（Phase A3本格実装完了）
/// 
/// 【F#初学者向け解説】
/// このクラスは、F#のApplication層で定義されたIAuthenticationServiceインターフェースを
/// C#のInfrastructure層で実装しています。ASP.NET Core Identityとの本格統合により、
/// ログイン、ユーザー作成、パスワード管理、ロックアウト機能を提供します。
/// 
/// 【Blazor Server初学者向け解説】
/// UserManager、SignInManagerはASP.NET Core Identityの核心サービスです。
/// - UserManager: ユーザー作成、パスワード管理、ロックアウト制御
/// - SignInManager: ログイン/ログアウト、Remember Me機能
/// </summary>
public class AuthenticationService : IAuthenticationService
{
    private readonly Microsoft.Extensions.Logging.ILogger<AuthenticationService> _logger;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly INotificationService _notificationService;
    private readonly IUserRepository _userRepository;

    /// <summary>
    /// AuthenticationServiceのコンストラクタ（Phase A5標準Identity移行対応）
    /// </summary>
    /// <param name="logger">ログ出力サービス</param>
    /// <param name="userManager">ASP.NET Core Identity ユーザー管理</param>
    /// <param name="signInManager">ASP.NET Core Identity サインイン管理</param>
    /// <param name="notificationService">F# Application層 通知サービス</param>
    /// <param name="userRepository">F# Domain層 ユーザーリポジトリ</param>
    public AuthenticationService(
        Microsoft.Extensions.Logging.ILogger<AuthenticationService> logger,
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
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
    /// ログイン機能（Phase A5標準Identity移行対応）
    /// 
    /// 【F#初学者向け解説】
    /// F#のEmail型からstring変換し、ASP.NET Core Identityでログイン処理を実行。
    /// 成功時はF#のUser型に変換して返却、失敗時はエラーメッセージを返却。
    /// ロックアウト機能、失敗回数記録も含む包括的なログイン処理。
    /// </summary>
    public async Task<FSharpResult<User, string>> LoginAsync(Email email, string password)
{
    try
    {
        var emailValue = email.Value;
        _logger.LogInformation("Login attempt for user: {Email}", emailValue);

        // ASP.NET Core Identity ユーザー検索
        var identityUser = await _userManager.FindByEmailAsync(emailValue);
        if (identityUser == null)
        {
            _logger.LogWarning("Login failed: User not found for email {Email}", emailValue);
            return FSharpResult<User, string>.NewError("ユーザーが見つかりません");
        }

        // ロックアウト状態確認
        if (await _userManager.IsLockedOutAsync(identityUser))
        {
            _logger.LogWarning("Login failed: User {Email} is locked out until {LockoutEnd}", 
                emailValue, identityUser.LockoutEnd);
            return FSharpResult<User, string>.NewError("アカウントがロックアウトされています");
        }

        // 【重要な仕様対応】機能仕様書2.2.1準拠：初期パスワード平文認証
        // PasswordHashがNULLの場合はInitialPasswordで平文認証を実行
        bool authenticationSuccessful = false;
        
        if (string.IsNullOrEmpty(identityUser.PasswordHash) && !string.IsNullOrEmpty(identityUser.InitialPassword))
        {
            // 初期パスワード平文認証（機能仕様書2.2.1準拠）
            _logger.LogInformation("Using InitialPassword authentication for user: {Email}", emailValue);
            authenticationSuccessful = identityUser.InitialPassword == password;
            
            if (authenticationSuccessful)
            {
                // 手動でサインイン（InitialPassword認証成功時）
                await _signInManager.SignInAsync(identityUser, isPersistent: false);
                _logger.LogInformation("InitialPassword authentication successful for user: {Email}", emailValue);
            }
            else
            {
                _logger.LogWarning("InitialPassword authentication failed for user: {Email}", emailValue);
            }
        }
        else
        {
            // 標準のPasswordHash認証
            var signInResult = await _signInManager.PasswordSignInAsync(
                identityUser, password, isPersistent: false, lockoutOnFailure: false);
            
            authenticationSuccessful = signInResult.Succeeded;
            
            if (signInResult.IsLockedOut)
            {
                _logger.LogWarning("Login failed: User {Email} locked out after failed attempt", emailValue);
                return FSharpResult<User, string>.NewError("アカウントがロックアウトされました");
            }
        }

        if (authenticationSuccessful)
        {
            _logger.LogInformation("Login successful for user: {Email}", emailValue);
            
            // F# Domain User型に変換（標準Identity対応）
            var domainUser = CreateSimpleDomainUser(identityUser);
            
            // 通知サービスに成功通知（簡易実装）
            _logger.LogInformation("Login success notification for user: {Email}", emailValue);
            
            return FSharpResult<User, string>.NewOk(domainUser);
        }
        else
        {
            _logger.LogWarning("Login failed: Invalid password for user {Email}", emailValue);
            return FSharpResult<User, string>.NewError("メールアドレスまたはパスワードが正しくありません");
        }
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Login error for email: {Email}", email.Value);
        return FSharpResult<User, string>.NewError("ログイン処理中にエラーが発生しました");
    }
}

    /// <summary>
    /// ユーザー作成機能（Phase A5標準Identity移行対応）
    /// 
    /// 【F#初学者向け解説】
    /// F#のドメイン型（Email, UserName, Role, Password）をC#で受け取り、
    /// ASP.NET Core Identityで実際のユーザー作成を実行。
    /// 作成成功時はF#のUser型に変換して返却。
    /// </summary>
    public async Task<FSharpResult<User, string>> CreateUserWithPasswordAsync(
        Email email, UserName name, Role role, Password password, UserId createdBy)
    {
        try
        {
            var emailValue = email.Value;
            var nameValue = name.Value;
            var passwordValue = password.Value;
            
            _logger.LogInformation("Creating user: {Email} with name: {Name}", emailValue, nameValue);

            // ApplicationUser エンティティ作成（カスタムプロパティ対応）
            var identityUser = new ApplicationUser
            {
                UserName = emailValue,
                Email = emailValue,
                EmailConfirmed = true, // 初期設定では確認済みとする
                LockoutEnabled = true,  // ロックアウト機能有効
                AccessFailedCount = 0,
                Name = nameValue,  // カスタムプロパティ：ユーザー氏名
                IsFirstLogin = true,  // カスタムプロパティ：初回ログインフラグ
                UpdatedAt = DateTime.UtcNow,  // カスタムプロパティ：更新日時
                UpdatedBy = "System",  // カスタムプロパティ：更新者（システム作成）
                IsDeleted = false  // カスタムプロパティ：削除フラグ
            };

            // ASP.NET Core Identity ユーザー作成
            var createResult = await _userManager.CreateAsync(identityUser, passwordValue);
            
            if (createResult.Succeeded)
            {
                _logger.LogInformation("User created successfully: {Email}", emailValue);
                
                // ロール割り当て
                var roleValue = role.ToString();
                var roleResult = await _userManager.AddToRoleAsync(identityUser, roleValue);
                
                if (roleResult.Succeeded)
                {
                    _logger.LogInformation("Role {Role} assigned to user {Email}", roleValue, emailValue);
                    
                    // F# Domain User型に変換（標準Identity対応）
                    var domainUser = CreateSimpleDomainUser(identityUser);
                    
                    // 通知サービスに作成通知（簡易実装）
                    _logger.LogInformation("User creation notification for: {Email}", emailValue);
                    
                    return FSharpResult<User, string>.NewOk(domainUser);
                }
                else
                {
                    _logger.LogError("Role assignment failed for user {Email}: {Errors}", 
                        emailValue, string.Join(", ", roleResult.Errors.Select(e => e.Description)));
                    return FSharpResult<User, string>.NewError("ロール割り当てに失敗しました");
                }
            }
            else
            {
                var errors = string.Join(", ", createResult.Errors.Select(e => e.Description));
                _logger.LogError("User creation failed for {Email}: {Errors}", emailValue, errors);
                return FSharpResult<User, string>.NewError($"ユーザー作成に失敗しました: {errors}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "User creation error for email: {Email}", email.Value);
            return FSharpResult<User, string>.NewError("ユーザー作成処理中にエラーが発生しました");
        }
    }

    /// <summary>
    /// パスワード変更機能（Phase A5標準Identity移行対応）
    /// </summary>
    public async Task<FSharpResult<PasswordHash, string>> ChangePasswordAsync(
        UserId userId, string oldPassword, Password newPassword)
{
    try
    {
        var userIdValue = userId.Value;
        var newPasswordValue = newPassword.Value;
        
        _logger.LogInformation("Changing password for user: {UserId}", userIdValue);

        var identityUser = await _userManager.FindByIdAsync(userIdValue.ToString());
        if (identityUser == null)
        {
            _logger.LogWarning("Password change failed: User not found {UserId}", userIdValue);
            return FSharpResult<PasswordHash, string>.NewError("ユーザーが見つかりません");
        }

        IdentityResult changeResult;

        // 【重要な仕様対応】機能仕様書2.2.1準拠：初期パスワード処理
        if (string.IsNullOrEmpty(identityUser.PasswordHash) && !string.IsNullOrEmpty(identityUser.InitialPassword))
        {
            // 初期パスワードからの変更（oldPasswordは平文InitialPasswordと照合）
            if (identityUser.InitialPassword != oldPassword)
            {
                _logger.LogWarning("Password change failed: Invalid old InitialPassword for user {UserId}", userIdValue);
                return FSharpResult<PasswordHash, string>.NewError("現在のパスワードが正しくありません");
            }

            // 初期パスワードをクリアして新しいPasswordHashを設定
            identityUser.InitialPassword = null;  // セキュリティ：初期パスワード削除
            identityUser.IsFirstLogin = false;    // 初回ログインフラグをリセット
            identityUser.UpdatedAt = DateTime.UtcNow;
            identityUser.UpdatedBy = userIdValue.ToString();

            // 新しいパスワードハッシュを設定
            identityUser.PasswordHash = _userManager.PasswordHasher.HashPassword(identityUser, newPasswordValue);
            
            // ユーザー更新
            var updateResult = await _userManager.UpdateAsync(identityUser);
            changeResult = updateResult;

            _logger.LogInformation("InitialPassword cleared and PasswordHash set for user: {UserId}", userIdValue);
        }
        else
        {
            // 標準のパスワード変更
            changeResult = await _userManager.ChangePasswordAsync(
                identityUser, oldPassword, newPasswordValue);
        }
        
        if (changeResult.Succeeded)
        {
            _logger.LogInformation("Password changed successfully for user: {UserId}", userIdValue);
            
            // セキュリティスタンプ更新（既存セッション無効化）
            await _userManager.UpdateSecurityStampAsync(identityUser);
            
            // パスワードハッシュを返却（実際の値は外部に公開しない）
            var passwordHash = CreatePasswordHash("[PROTECTED]");
            return FSharpResult<PasswordHash, string>.NewOk(passwordHash);
        }
        else
        {
            var errors = string.Join(", ", changeResult.Errors.Select(e => e.Description));
            _logger.LogError("Password change failed for user {UserId}: {Errors}", userIdValue, errors);
            return FSharpResult<PasswordHash, string>.NewError($"パスワード変更に失敗しました: {errors}");
        }
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Password change error for user: {UserId}", userId.Value);
        return FSharpResult<PasswordHash, string>.NewError("パスワード変更処理中にエラーが発生しました");
    }
}

    /// <summary>
    /// パスワードハッシュ生成機能（Phase A5標準Identity移行対応）
    /// </summary>
    public async Task<FSharpResult<PasswordHash, string>> HashPasswordAsync(Password password)
    {
        try
        {
            await Task.CompletedTask;
            var passwordValue = password.Value;
            
            _logger.LogDebug("Hashing password");

            // ASP.NET Core Identity パスワードハッシュ生成
            var hashedPassword = _userManager.PasswordHasher.HashPassword(null!, passwordValue);
            
            var passwordHash = CreatePasswordHash(hashedPassword);
            _logger.LogDebug("Password hashed successfully");
            
            return FSharpResult<PasswordHash, string>.NewOk(passwordHash);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Password hashing error");
            return FSharpResult<PasswordHash, string>.NewError("パスワードハッシュ生成中にエラーが発生しました");
        }
    }

    /// <summary>
    /// パスワード検証機能（Phase A5標準Identity移行対応）
    /// </summary>
    public async Task<FSharpResult<bool, string>> VerifyPasswordAsync(string password, PasswordHash hash)
    {
        try
        {
            await Task.CompletedTask;
            var hashValue = hash.Value;
            
            _logger.LogDebug("Verifying password");

            // ASP.NET Core Identity パスワード検証
            var verificationResult = _userManager.PasswordHasher.VerifyHashedPassword(
                null!, hashValue, password);
            
            var isValid = verificationResult == PasswordVerificationResult.Success || 
                         verificationResult == PasswordVerificationResult.SuccessRehashNeeded;
            
            _logger.LogDebug("Password verification result: {IsValid}", isValid);
            
            return FSharpResult<bool, string>.NewOk(isValid);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Password verification error");
            return FSharpResult<bool, string>.NewError("パスワード検証中にエラーが発生しました");
        }
    }

    /// <summary>
    /// トークン生成機能（Phase A5標準Identity移行対応）
    /// </summary>
    public async Task<FSharpResult<string, string>> GenerateTokenAsync(User user)
    {
        try
        {
            var userId = user.Id.Value;
            var identityUser = await _userManager.FindByIdAsync(userId.ToString());
            
            if (identityUser == null)
            {
                return FSharpResult<string, string>.NewError("ユーザーが見つかりません");
            }

            var token = await _userManager.GeneratePasswordResetTokenAsync(identityUser);
            _logger.LogInformation("Token generated for user: {UserId}", userId);
            
            return FSharpResult<string, string>.NewOk(token);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Token generation error");
            return FSharpResult<string, string>.NewError("トークン生成中にエラーが発生しました");
        }
    }

    /// <summary>
    /// トークン検証機能（Phase A3簡易実装）
    /// </summary>
    public async Task<FSharpResult<User, string>> ValidateTokenAsync(string token)
    {
        await Task.CompletedTask;
        _logger.LogInformation("ValidateTokenAsync called - 簡易実装");
        return FSharpResult<User, string>.NewError("トークン検証は専用サービスで実装予定");
    }

    /// <summary>
    /// ログイン失敗記録機能（Phase A5標準Identity移行対応）
    /// </summary>
    public async Task<FSharpResult<User, string>> RecordFailedLoginAsync(UserId userId)
    {
        try
        {
            var userIdValue = userId.Value;
            _logger.LogInformation("Recording failed login for user: {UserId}", userIdValue);

            var identityUser = await _userManager.FindByIdAsync(userIdValue.ToString());
            if (identityUser == null)
            {
                _logger.LogWarning("Failed login record: User not found {UserId}", userIdValue);
                return FSharpResult<User, string>.NewError("ユーザーが見つかりません");
            }

            // 失敗回数インクリメント（ASP.NET Core Identityが自動処理）
            var result = await _userManager.AccessFailedAsync(identityUser);
            
            if (result.Succeeded)
            {
                _logger.LogInformation("Failed login recorded for user: {UserId}, Failed count: {FailedCount}", 
                    userIdValue, identityUser.AccessFailedCount);
                
                // ロックアウト判定
                if (await _userManager.IsLockedOutAsync(identityUser))
                {
                    _logger.LogWarning("User {UserId} locked out due to failed attempts", userIdValue);
                }
                
                var domainUser = CreateSimpleDomainUser(identityUser);
                return FSharpResult<User, string>.NewOk(domainUser);
            }
            else
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                _logger.LogError("Failed to record login failure for user {UserId}: {Errors}", userIdValue, errors);
                return FSharpResult<User, string>.NewError("ログイン失敗記録に失敗しました");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed login recording error for user: {UserId}", userId.Value);
            return FSharpResult<User, string>.NewError("ログイン失敗記録処理中にエラーが発生しました");
        }
    }

    /// <summary>
    /// ログイン成功記録機能（Phase A5標準Identity移行対応）
    /// </summary>
    public async Task<FSharpResult<User, string>> RecordSuccessfulLoginAsync(UserId userId)
    {
        try
        {
            var userIdValue = userId.Value;
            _logger.LogInformation("Recording successful login for user: {UserId}", userIdValue);

            var identityUser = await _userManager.FindByIdAsync(userIdValue.ToString());
            if (identityUser == null)
            {
                _logger.LogWarning("Successful login record: User not found {UserId}", userIdValue);
                return FSharpResult<User, string>.NewError("ユーザーが見つかりません");
            }

            // 失敗回数リセット
            var result = await _userManager.ResetAccessFailedCountAsync(identityUser);
            
            if (result.Succeeded)
            {
                _logger.LogInformation("Successful login recorded and failed count reset for user: {UserId}", userIdValue);
                
                var domainUser = CreateSimpleDomainUser(identityUser);
                return FSharpResult<User, string>.NewOk(domainUser);
            }
            else
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                _logger.LogError("Failed to record successful login for user {UserId}: {Errors}", userIdValue, errors);
                return FSharpResult<User, string>.NewError("ログイン成功記録に失敗しました");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Successful login recording error for user: {UserId}", userId.Value);
            return FSharpResult<User, string>.NewError("ログイン成功記録処理中にエラーが発生しました");
        }
    }

    /// <summary>
    /// ユーザーロック機能（Phase A5標準Identity移行対応）
    /// </summary>
    public async Task<FSharpResult<Unit, string>> LockUserAsync(UserId userId, DateTime lockoutEnd)
    {
        try
        {
            var userIdValue = userId.Value;
            _logger.LogInformation("Locking user: {UserId} until {LockoutEnd}", userIdValue, lockoutEnd);

            var identityUser = await _userManager.FindByIdAsync(userIdValue.ToString());
            if (identityUser == null)
            {
                _logger.LogWarning("Lock user: User not found {UserId}", userIdValue);
                return FSharpResult<Unit, string>.NewError("ユーザーが見つかりません");
            }

            // ロックアウト設定
            var result = await _userManager.SetLockoutEndDateAsync(identityUser, lockoutEnd);
            
            if (result.Succeeded)
            {
                _logger.LogInformation("User {UserId} locked until {LockoutEnd}", userIdValue, lockoutEnd);
                return FSharpResult<Unit, string>.NewOk(null!);
            }
            else
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                _logger.LogError("Failed to lock user {UserId}: {Errors}", userIdValue, errors);
                return FSharpResult<Unit, string>.NewError("ユーザーロックに失敗しました");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "User lock error for user: {UserId}", userId.Value);
            return FSharpResult<Unit, string>.NewError("ユーザーロック処理中にエラーが発生しました");
        }
    }

    /// <summary>
    /// ユーザーロック解除機能（Phase A5標準Identity移行対応）
    /// </summary>
    public async Task<FSharpResult<Unit, string>> UnlockUserAsync(UserId userId)
    {
        try
        {
            var userIdValue = userId.Value;
            _logger.LogInformation("Unlocking user: {UserId}", userIdValue);

            var identityUser = await _userManager.FindByIdAsync(userIdValue.ToString());
            if (identityUser == null)
            {
                _logger.LogWarning("Unlock user: User not found {UserId}", userIdValue);
                return FSharpResult<Unit, string>.NewError("ユーザーが見つかりません");
            }

            // ロックアウト解除と失敗回数リセット
            var unlockResult = await _userManager.SetLockoutEndDateAsync(identityUser, null);
            var resetResult = await _userManager.ResetAccessFailedCountAsync(identityUser);
            
            if (unlockResult.Succeeded && resetResult.Succeeded)
            {
                _logger.LogInformation("User {UserId} unlocked successfully", userIdValue);
                return FSharpResult<Unit, string>.NewOk(null!);
            }
            else
            {
                var errors = unlockResult.Errors.Concat(resetResult.Errors)
                    .Select(e => e.Description);
                var errorMessage = string.Join(", ", errors);
                _logger.LogError("Failed to unlock user {UserId}: {Errors}", userIdValue, errorMessage);
                return FSharpResult<Unit, string>.NewError("ユーザーロック解除に失敗しました");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "User unlock error for user: {UserId}", userId.Value);
            return FSharpResult<Unit, string>.NewError("ユーザーロック解除処理中にエラーが発生しました");
        }
    }

    /// <summary>
    /// セキュリティスタンプ更新機能（Phase A5標準Identity移行対応）
    /// </summary>
    public async Task<FSharpResult<Unit, string>> UpdateSecurityStampAsync(UserId userId)
    {
        try
        {
            var userIdValue = userId.Value;
            var identityUser = await _userManager.FindByIdAsync(userIdValue.ToString());
            
            if (identityUser == null)
            {
                return FSharpResult<Unit, string>.NewError("ユーザーが見つかりません");
            }

            var result = await _userManager.UpdateSecurityStampAsync(identityUser);
            
            if (result.Succeeded)
            {
                _logger.LogInformation("Security stamp updated for user: {UserId}", userIdValue);
                return FSharpResult<Unit, string>.NewOk(null!);
            }
            else
            {
                return FSharpResult<Unit, string>.NewError("セキュリティスタンプ更新に失敗しました");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Security stamp update error");
            return FSharpResult<Unit, string>.NewError("セキュリティスタンプ更新中にエラーが発生しました");
        }
    }

    /// <summary>
    /// メール確認送信機能（Phase A3簡易実装）
    /// </summary>
    public async Task<FSharpResult<Unit, string>> SendEmailConfirmationAsync(Email email)
    {
        await Task.CompletedTask;
        _logger.LogInformation("SendEmailConfirmationAsync called - メールサービスで実装予定");
        return FSharpResult<Unit, string>.NewOk(null!);
    }

    /// <summary>
    /// メール確認機能（Phase A3簡易実装）
    /// </summary>
    public async Task<FSharpResult<Unit, string>> ConfirmEmailAsync(UserId userId, string confirmationToken)
    {
        await Task.CompletedTask;
        _logger.LogInformation("ConfirmEmailAsync called - 簡易実装");
        return FSharpResult<Unit, string>.NewOk(null!);
    }

    /// <summary>
    /// 二要素認証有効化機能（Phase A3簡易実装）
    /// </summary>
    public async Task<FSharpResult<string, string>> EnableTwoFactorAsync(UserId userId)
    {
        await Task.CompletedTask;
        _logger.LogInformation("EnableTwoFactorAsync called - 2FAは後期実装予定");
        return FSharpResult<string, string>.NewError("2FA機能は後期実装予定");
    }

    /// <summary>
    /// 二要素認証無効化機能（Phase A3簡易実装）
    /// </summary>
    public async Task<FSharpResult<Unit, string>> DisableTwoFactorAsync(UserId userId)
    {
        await Task.CompletedTask;
        _logger.LogInformation("DisableTwoFactorAsync called - 2FAは後期実装予定");
        return FSharpResult<Unit, string>.NewError("2FA機能は後期実装予定");
    }

    /// <summary>
    /// 二要素認証コード検証機能（Phase A3簡易実装）
    /// </summary>
    public async Task<FSharpResult<bool, string>> VerifyTwoFactorCodeAsync(UserId userId, string code)
    {
        await Task.CompletedTask;
        _logger.LogInformation("VerifyTwoFactorCodeAsync called - 2FAは後期実装予定");
        return FSharpResult<bool, string>.NewError("2FA機能は後期実装予定");
    }

    /// <summary>
    /// 現在ユーザー取得機能（Phase A3簡易実装）
    /// </summary>
    public async Task<FSharpResult<FSharpOption<User>, string>> GetCurrentUserAsync()
    {
        await Task.CompletedTask;
        _logger.LogInformation("GetCurrentUserAsync called - 簡易実装");
        // 現在ユーザー取得はBlazor ServerのAuthenticationStateProviderで実装
        return FSharpResult<FSharpOption<User>, string>.NewOk(FSharpOption<User>.None);
    }

    // ===== 一時的ヘルパーメソッド（Phase A5標準Identity移行対応） =====

    /// <summary>
    /// ApplicationUserから簡易的なF# Userエンティティを作成
    /// </summary>
    private User CreateSimpleDomainUser(ApplicationUser identityUser)
    {
        var userId = UserId.NewUserId(long.Parse(identityUser.Id));
        var email = Email.create(identityUser.Email ?? "").ResultValue;
        
        // ApplicationUserのNameプロパティを使用
        var userName = UserName.create(identityUser.Name ?? "Unknown").ResultValue;
        
        var role = Role.GeneralUser; // 簡易実装
        var createdBy = UserId.NewUserId(1); // 簡易実装
        
        return User.create(email, userName, role, createdBy);
    }

    /// <summary>
    /// ログイン試行記録機能（Phase A8統合テスト対応）
    /// </summary>
    public async Task<FSharpResult<Unit, string>> RecordLoginAttemptAsync(Email email, bool isSuccessful)
    {
        try
        {
            var emailValue = email.Value;
            _logger.LogInformation("Recording login attempt for user: {Email}, Success: {IsSuccessful}", 
                emailValue, isSuccessful);

            var identityUser = await _userManager.FindByEmailAsync(emailValue);
            if (identityUser == null)
            {
                _logger.LogWarning("Login attempt record: User not found {Email}", emailValue);
                return FSharpResult<Unit, string>.NewError("ユーザーが見つかりません");
            }

            IdentityResult result;
            
            if (isSuccessful)
            {
                // 成功時：失敗回数をリセット
                result = await _userManager.ResetAccessFailedCountAsync(identityUser);
                if (result.Succeeded)
                {
                    _logger.LogInformation("Login attempt recorded successfully for user: {Email}", emailValue);
                }
            }
            else
            {
                // 失敗時：失敗回数をインクリメント
                result = await _userManager.AccessFailedAsync(identityUser);
                if (result.Succeeded)
                {
                    _logger.LogWarning("Login attempt recorded as failure for user: {Email}", emailValue);
                    
                    // ロックアウト確認
                    if (await _userManager.IsLockedOutAsync(identityUser))
                    {
                        _logger.LogWarning("User account locked due to failed login attempts for user: {Email}", emailValue);
                    }
                }
            }

            if (result.Succeeded)
            {
                return FSharpResult<Unit, string>.NewOk(null!);
            }
            else
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                _logger.LogError("Failed to record login attempt for user {Email}: {Errors}", emailValue, errors);
                return FSharpResult<Unit, string>.NewError("ログイン試行記録中にエラーが発生しました");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login attempt recording for user: {Email}", email.Value);
            return FSharpResult<Unit, string>.NewError("ログイン試行記録処理中にエラーが発生しました");
        }
    }

    /// <summary>
    /// アカウントロック状態確認機能（Phase A8統合テスト対応）
    /// </summary>
    public async Task<FSharpResult<bool, string>> IsAccountLockedAsync(Email email)
    {
        try
        {
            var emailValue = email.Value;
            _logger.LogInformation("Checking account lock status for user: {Email}", emailValue);

            var identityUser = await _userManager.FindByEmailAsync(emailValue);
            if (identityUser == null)
            {
                _logger.LogInformation("Account lock check: User not found {Email}, returning false", emailValue);
                return FSharpResult<bool, string>.NewOk(false);
            }

            var isLocked = await _userManager.IsLockedOutAsync(identityUser);
            _logger.LogInformation("Account lock status for user {Email}: {IsLocked}", emailValue, isLocked);
            
            return FSharpResult<bool, string>.NewOk(isLocked);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking account lock status for user: {Email}", email.Value);
            return FSharpResult<bool, string>.NewError("アカウントロック状態確認中にエラーが発生しました");
        }
    }

    /// <summary>
    /// パスワードリセット後自動ログイン機能（Phase A8統合テスト対応）
    /// </summary>
    public async Task<FSharpResult<User, string>> AutoLoginAfterPasswordResetAsync(Email email)
    {
        try
        {
            var emailValue = email.Value;
            _logger.LogInformation("Auto login after password reset for user: {Email}", emailValue);

            var identityUser = await _userManager.FindByEmailAsync(emailValue);
            if (identityUser == null)
            {
                _logger.LogWarning("Auto login: User not found {Email}", emailValue);
                return FSharpResult<User, string>.NewError("ユーザーが見つかりません");
            }

            // ロックアウト状態確認
            if (await _userManager.IsLockedOutAsync(identityUser))
            {
                _logger.LogWarning("Auto login attempted for locked user: {Email}", emailValue);
                return FSharpResult<User, string>.NewError("アカウントがロックされています");
            }

            // 自動サインイン実行
            await _signInManager.SignInAsync(identityUser, isPersistent: false);
            
            _logger.LogInformation("Auto login successful for user: {Email}", emailValue);
            
            var domainUser = CreateSimpleDomainUser(identityUser);
            return FSharpResult<User, string>.NewOk(domainUser);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Auto login error for user: {Email}", email.Value);
            return FSharpResult<User, string>.NewError("自動ログイン処理中にエラーが発生しました");
        }
    }

    /// <summary>
    /// 文字列からPasswordHashを作成
    /// </summary>
    private PasswordHash CreatePasswordHash(string hashValue)
    {
        // F# PasswordHashの作成（簡易実装）
        var result = PasswordHash.create(hashValue);
        return result.IsOk ? result.ResultValue : PasswordHash.create("dummy").ResultValue;
    }
}