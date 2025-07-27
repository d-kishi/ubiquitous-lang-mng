using System;
using System.Threading.Tasks;
using Microsoft.FSharp.Core;
using UbiquitousLanguageManager.Application;
using UbiquitousLanguageManager.Domain;
using UbiquitousLanguageManager.Infrastructure.Services;
using UbiquitousLanguageManager.Tests.Stubs;

namespace UbiquitousLanguageManager.Tests.Stubs
{
    /// <summary>
    /// Phase A3 Step5: テストインフラ修正のための一時的なスタブ
    /// Phase A4で正式なテストを再構築する際に削除予定
    /// </summary>
    public static class TemporaryStubs
    {
        // Note: Extension methods moved to top-level static classes below
    }

    /// <summary>
    /// 削除されたApplicationDbContextの代替
    /// </summary>
    public class ApplicationDbContext : UbiquitousLanguageManager.Infrastructure.Data.UbiquitousLanguageDbContext
    {
        public ApplicationDbContext(Microsoft.EntityFrameworkCore.DbContextOptions<UbiquitousLanguageManager.Infrastructure.Data.UbiquitousLanguageDbContext> options) 
            : base(options)
        {
        }
    }

    /// <summary>
    /// 削除されたUserエンティティの代替（Identityで使用）
    /// </summary>
    public class User : UbiquitousLanguageManager.Infrastructure.Data.Entities.ApplicationUser
    {
    }
}

// Unit型の名前空間衝突は他の方法で解決
// Microsoft.FSharp.Core.Unitを直接使用

/// <summary>
/// INotificationServiceの拡張メソッド（削除されたメソッドの代替）
/// </summary>
public static class NotificationServiceExtensions
{
    public static Task<FSharpResult<Microsoft.FSharp.Core.Unit, string>> SendPasswordResetEmailAsync(
        this INotificationService service, Email email, string userName, string resetUrl)
    {
        // Phase A3で削除されたメソッドの代替スタブ
        return Task.FromResult(FSharpResult<Microsoft.FSharp.Core.Unit, string>.NewOk(null!));
    }

    public static Task<FSharpResult<Microsoft.FSharp.Core.Unit, string>> SendPasswordResetConfirmationAsync(
        this INotificationService service, Email email)
    {
        // Phase A3で削除されたメソッドの代替スタブ
        return Task.FromResult(FSharpResult<Microsoft.FSharp.Core.Unit, string>.NewOk(null!));
    }
}

/// <summary>
/// AuthenticationServiceの拡張メソッド（削除されたメソッドの代替）
/// </summary>
public static class AuthenticationServiceExtensions
{
    public static Task<FSharpResult<string, string>> RequestPasswordResetAsync(
        this AuthenticationService service, Email email)
    {
        // Phase A3で削除されたメソッドの代替スタブ
        return Task.FromResult(FSharpResult<string, string>.NewError("Phase A3で削除"));
    }

    public static Task<FSharpResult<UbiquitousLanguageManager.Domain.User, string>> ResetPasswordAsync(
        this AuthenticationService service, Email email, string token, Password newPassword)
    {
        // Phase A3で削除されたメソッドの代替スタブ
        return Task.FromResult(FSharpResult<UbiquitousLanguageManager.Domain.User, string>.NewError("Phase A3で削除"));
    }

    public static Task<FSharpResult<UbiquitousLanguageManager.Domain.User, string>> AutoLoginAfterPasswordResetAsync(
        this AuthenticationService service, Email email)
    {
        // Phase A3で削除されたメソッドの代替スタブ
        return Task.FromResult(FSharpResult<UbiquitousLanguageManager.Domain.User, string>.NewError("Phase A3で削除"));
    }

    public static Task<FSharpResult<Microsoft.FSharp.Core.Unit, string>> RecordLoginAttemptAsync(
        this AuthenticationService service, Email email, bool isSuccess)
    {
        // Phase A3で削除されたメソッドの代替スタブ
        return Task.FromResult(FSharpResult<Microsoft.FSharp.Core.Unit, string>.NewError("Phase A3で削除"));
    }

    public static Task<FSharpResult<bool, string>> ValidatePasswordResetTokenAsync(
        this AuthenticationService service, Email email, string token)
    {
        // Phase A3で削除されたメソッドの代替スタブ
        return Task.FromResult(FSharpResult<bool, string>.NewError("Phase A3で削除"));
    }

    public static Task<FSharpResult<bool, string>> IsAccountLockedAsync(
        this AuthenticationService service, Email email)
    {
        // Phase A3で削除されたメソッドの代替スタブ
        return Task.FromResult(FSharpResult<bool, string>.NewOk(false));
    }
}