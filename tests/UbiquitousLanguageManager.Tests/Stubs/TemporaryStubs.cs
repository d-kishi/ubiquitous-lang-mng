using System;
using System.Threading.Tasks;
using Microsoft.FSharp.Core;
using UbiquitousLanguageManager.Application;
using UbiquitousLanguageManager.Domain;
using UbiquitousLanguageManager.Domain.Common;
using UbiquitousLanguageManager.Domain.Authentication;
using UbiquitousLanguageManager.Infrastructure.Services;
using UbiquitousLanguageManager.Tests.Stubs;

namespace UbiquitousLanguageManager.Tests.Stubs
{
    /// <summary>
    /// テストインフラスタブライブラリ
    /// テスト実行に必要な一時的なスタブを提供
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
        // テスト用スタブ実装
        return Task.FromResult(FSharpResult<Microsoft.FSharp.Core.Unit, string>.NewOk(null!));
    }

    public static Task<FSharpResult<Microsoft.FSharp.Core.Unit, string>> SendPasswordResetConfirmationAsync(
        this INotificationService service, Email email)
    {
        // テスト用スタブ実装
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
        // テスト用スタブ実装
        return Task.FromResult(FSharpResult<string, string>.NewError("機能不可"));
    }

    public static Task<FSharpResult<User, string>> ResetPasswordAsync(
        this AuthenticationService service, Email email, string token, Password newPassword)
    {
        // テスト用スタブ実装
        return Task.FromResult(FSharpResult<User, string>.NewError("機能不可"));
    }

    public static Task<FSharpResult<User, string>> AutoLoginAfterPasswordResetAsync(
        this AuthenticationService service, Email email)
    {
        // テスト用スタブ実装
        return Task.FromResult(FSharpResult<User, string>.NewError("機能不可"));
    }

    public static Task<FSharpResult<Microsoft.FSharp.Core.Unit, string>> RecordLoginAttemptAsync(
        this AuthenticationService service, Email email, bool isSuccess)
    {
        // テスト用スタブ実装
        return Task.FromResult(FSharpResult<Microsoft.FSharp.Core.Unit, string>.NewError("機能不可"));
    }

    public static Task<FSharpResult<bool, string>> ValidatePasswordResetTokenAsync(
        this AuthenticationService service, Email email, string token)
    {
        // テスト用スタブ実装
        return Task.FromResult(FSharpResult<bool, string>.NewError("機能不可"));
    }

    public static Task<FSharpResult<bool, string>> IsAccountLockedAsync(
        this AuthenticationService service, Email email)
    {
        // テスト用スタブ実装
        return Task.FromResult(FSharpResult<bool, string>.NewOk(false));
    }
}