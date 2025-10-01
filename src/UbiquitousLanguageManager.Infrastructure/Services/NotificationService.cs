using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.FSharp.Core;
using UbiquitousLanguageManager.Application;
using UbiquitousLanguageManager.Domain.Common;
using UbiquitousLanguageManager.Domain.Authentication;
using UbiquitousLanguageManager.Domain.UbiquitousLanguageManagement;

namespace UbiquitousLanguageManager.Infrastructure.Services;

/// <summary>
/// 通知サービスの実装
/// Phase A2: ユーザー管理機能向け通知システム基本実装
/// 
/// 【Blazor Server・F#初学者向け解説】
/// 通知サービスは、ユーザー関連の重要なイベント（アカウント作成・パスワード変更等）を
/// メール・ログ・その他の方法でユーザーや管理者に通知する機能を提供します。
/// Phase A2では基本的なログ出力機能を実装し、将来的にメール送信機能を拡張予定です。
/// </summary>
public class NotificationService : INotificationService
{
    private readonly Microsoft.Extensions.Logging.ILogger<NotificationService> _logger;

    /// <summary>
    /// NotificationServiceのコンストラクタ
    /// </summary>
    public NotificationService(Microsoft.Extensions.Logging.ILogger<NotificationService> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Phase A2: ユーザー作成通知
    /// 新規ユーザーアカウント作成時の通知処理
    /// </summary>
    /// <param name="user">作成されたユーザー</param>
    /// <param name="temporaryPassword">一時パスワード（初回ログイン用）</param>
    /// <returns>F#のResult型でラップされた通知結果</returns>
    /// <summary>Phase A3以降で実装予定</summary>
    public async Task<FSharpResult<Unit, string>> NotifyUserCreatedAsync(User user, string temporaryPassword)
    {
        try
        {
            // 【F#初学者向け解説】
            // F#のValue Objectから値を取得して通知処理に使用します。
            // User.Email.Value：バリデーション済みのメールアドレス
            // User.Name.Value：バリデーション済みのユーザー名
            var email = user.Email.Value;
            var name = user.Name.Value;
            var roleString = RoleToString(user.Role);

            // Phase A2: ログ出力による通知（将来的にメール送信に拡張予定）
            // ADR_008のログ出力規約に準拠した構造化ログ
            _logger.LogInformation(
                "User account created successfully. " +
                "Email: {Email}, Name: {Name}, Role: {Role}, " +
                "IsFirstLogin: {IsFirstLogin}, TemporaryPassword: [REDACTED]",
                email, name, roleString, user.IsFirstLogin);

            // 管理者向け通知ログ
            _logger.LogInformation(
                "ADMIN_NOTIFICATION: New user account requires initial setup. " +
                "User: {Name} ({Email}), Role: {Role}",
                name, email, roleString);

            // 将来のメール送信機能のプレースホルダー
            // TODO: Phase A3以降でメール送信機能を実装予定
            // await SendWelcomeEmailAsync(email, name, temporaryPassword);

            await Task.Delay(1); // async警告解消用（実際のメール送信処理の代替）

            return FSharpResult<Unit, string>.NewOk(null!);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send user creation notification for {Email}", user.Email.Value);
            return FSharpResult<Unit, string>.NewError($"通知送信エラー: {ex.Message}");
        }
    }

    /// <summary>
    /// Phase A2: パスワード変更通知
    /// ユーザーのパスワード変更時の通知処理
    /// </summary>
    /// <param name="user">パスワードを変更したユーザー</param>
    /// <param name="isFirstLogin">初回ログイン時の変更かどうか</param>
    /// <returns>F#のResult型でラップされた通知結果</returns>
    /// <summary>Phase A3以降で実装予定</summary>
    public async Task<FSharpResult<Unit, string>> NotifyPasswordChangedAsync(User user, bool isFirstLogin)
    {
        try
        {
            var email = user.Email.Value;
            var name = user.Name.Value;
            var changeType = isFirstLogin ? "初回ログイン時パスワード設定" : "パスワード変更";

            // パスワード変更ログ（セキュリティ監査用）
            _logger.LogInformation(
                "Password changed successfully. " +
                "Email: {Email}, Name: {Name}, ChangeType: {ChangeType}, " +
                "Timestamp: {Timestamp}",
                email, name, changeType, DateTimeOffset.UtcNow);

            // セキュリティ監査ログ
            _logger.LogInformation(
                "SECURITY_AUDIT: Password change event. " +
                "User: {Name} ({Email}), Type: {ChangeType}",
                name, email, changeType);

            // 将来のセキュリティ通知メール機能のプレースホルダー
            // TODO: Phase A3以降でセキュリティ通知メール機能を実装予定
            // await SendPasswordChangeNotificationAsync(email, name, isFirstLogin);

            await Task.Delay(1); // async警告解消用

            return FSharpResult<Unit, string>.NewOk(null!);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send password change notification for {Email}", user.Email.Value);
            return FSharpResult<Unit, string>.NewError($"パスワード変更通知エラー: {ex.Message}");
        }
    }

    /// <summary>
    /// Phase A2: ユーザーロール変更通知
    /// ユーザーのロール（権限）変更時の通知処理
    /// </summary>
    /// <param name="user">ロールが変更されたユーザー</param>
    /// <param name="previousRole">変更前のロール</param>
    /// <param name="changedBy">変更を実行したユーザー</param>
    /// <returns>F#のResult型でラップされた通知結果</returns>
    /// <summary>Phase A3以降で実装予定</summary>
    public async Task<FSharpResult<Unit, string>> NotifyRoleChangedAsync(User user, Role previousRole, User changedBy)
    {
        try
        {
            var email = user.Email.Value;
            var name = user.Name.Value;
            var currentRole = RoleToString(user.Role);
            var previousRoleString = RoleToString(previousRole);
            var changedByName = changedBy.Name.Value;
            var changedByEmail = changedBy.Email.Value;

            // ロール変更ログ（権限管理監査用）
            _logger.LogInformation(
                "User role changed successfully. " +
                "User: {Name} ({Email}), PreviousRole: {PreviousRole}, " +
                "CurrentRole: {CurrentRole}, ChangedBy: {ChangedByName} ({ChangedByEmail}), " +
                "Timestamp: {Timestamp}",
                name, email, previousRoleString, currentRole, changedByName, changedByEmail, DateTimeOffset.UtcNow);

            // セキュリティ監査ログ（特に権限昇格の場合）
            var isPrivilegeEscalation = IsPrivilegeEscalation(previousRole, user.Role);
            if (isPrivilegeEscalation)
            {
                _logger.LogWarning(
                    "SECURITY_AUDIT: Privilege escalation detected. " +
                    "User: {Name} ({Email}), {PreviousRole} → {CurrentRole}, " +
                    "ChangedBy: {ChangedByName} ({ChangedByEmail})",
                    name, email, previousRoleString, currentRole, changedByName, changedByEmail);
            }

            // 管理者向け通知ログ
            _logger.LogInformation(
                "ADMIN_NOTIFICATION: User role change requires attention. " +
                "User: {Name} ({Email}), Role: {PreviousRole} → {CurrentRole}",
                name, email, previousRoleString, currentRole);

            // 将来のロール変更通知メール機能のプレースホルダー
            // TODO: Phase A3以降で権限変更通知メール機能を実装予定
            // await SendRoleChangeNotificationAsync(email, name, previousRoleString, currentRole);

            await Task.Delay(1); // async警告解消用

            return FSharpResult<Unit, string>.NewOk(null!);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send role change notification for {Email}", user.Email.Value);
            return FSharpResult<Unit, string>.NewError($"ロール変更通知エラー: {ex.Message}");
        }
    }

    /// <summary>
    /// Phase A2: システム通知
    /// システム全体に関わる重要な通知処理
    /// </summary>
    /// <param name="message">通知メッセージ</param>
    /// <param name="level">通知レベル（Info/Warning/Error）</param>
    /// <returns>F#のResult型でラップされた通知結果</returns>
    /// <summary>Phase A3以降で実装予定</summary>
    public async Task<FSharpResult<Unit, string>> NotifySystemEventAsync(string message, string level)
    {
        try
        {
            // システムイベントログ
            switch (level.ToLowerInvariant())
            {
                case "info":
                    _logger.LogInformation("SYSTEM_NOTIFICATION: {Message}", message);
                    break;
                case "warning":
                    _logger.LogWarning("SYSTEM_NOTIFICATION: {Message}", message);
                    break;
                case "error":
                    _logger.LogError("SYSTEM_NOTIFICATION: {Message}", message);
                    break;
                default:
                    _logger.LogInformation("SYSTEM_NOTIFICATION: {Message} (Level: {Level})", message, level);
                    break;
            }

            await Task.Delay(1); // async警告解消用

            return FSharpResult<Unit, string>.NewOk(null!);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send system notification: {Message}", message);
            return FSharpResult<Unit, string>.NewError($"システム通知エラー: {ex.Message}");
        }
    }

    // =================================================================
    // 🚧 Phase A3以降実装予定: 将来実装メソッド（スタブ）
    // =================================================================

    /// <summary>Phase A3以降で実装予定</summary>
    public async Task<FSharpResult<Unit, string>> SendWelcomeEmailAsync(Email email)
    {
        await Task.Delay(1);
        return FSharpResult<Unit, string>.NewError("SendWelcomeEmailAsync: Phase A3以降で実装予定");
    }

    /// <summary>Phase A3以降で実装予定</summary>
    public async Task<FSharpResult<Unit, string>> SendRoleChangeNotificationAsync(Email email, Role newRole)
    {
        await Task.Delay(1);
        return FSharpResult<Unit, string>.NewError("SendRoleChangeNotificationAsync: Phase A3以降で実装予定");
    }

    /// <summary>Phase A3以降で実装予定</summary>
    public async Task<FSharpResult<Unit, string>> SendPasswordChangeNotificationAsync(Email email)
    {
        await Task.Delay(1);
        return FSharpResult<Unit, string>.NewError("SendPasswordChangeNotificationAsync: Phase A3以降で実装予定");
    }

    /// <summary>Phase A3以降で実装予定</summary>
    public async Task<FSharpResult<Unit, string>> SendEmailChangeConfirmationAsync(Email oldEmail, Email newEmail)
    {
        await Task.Delay(1);
        return FSharpResult<Unit, string>.NewError("SendEmailChangeConfirmationAsync: Phase A3以降で実装予定");
    }

    /// <summary>Phase A3以降で実装予定</summary>
    public async Task<FSharpResult<Unit, string>> SendAccountDeactivationNotificationAsync(Email email)
    {
        await Task.Delay(1);
        return FSharpResult<Unit, string>.NewError("SendAccountDeactivationNotificationAsync: Phase A3以降で実装予定");
    }

    /// <summary>Phase A3以降で実装予定</summary>
    public async Task<FSharpResult<Unit, string>> SendAccountActivationNotificationAsync(Email email)
    {
        await Task.Delay(1);
        return FSharpResult<Unit, string>.NewError("SendAccountActivationNotificationAsync: Phase A3以降で実装予定");
    }

    /// <summary>Phase A3以降で実装予定</summary>
    public async Task<FSharpResult<Unit, string>> SendSecurityAlertAsync(Email email, string alertType, string details)
    {
        await Task.Delay(1);
        return FSharpResult<Unit, string>.NewError("SendSecurityAlertAsync: Phase A3以降で実装予定");
    }

    /// <summary>Phase A3以降で実装予定</summary>
    public async Task<FSharpResult<Unit, string>> SendApprovalRequestAsync(User approver, DraftUbiquitousLanguage ubiquitousLanguage)
    {
        await Task.Delay(1);
        return FSharpResult<Unit, string>.NewError("SendApprovalRequestAsync: Phase A3以降で実装予定");
    }

    /// <summary>Phase A3以降で実装予定</summary>
    public async Task<FSharpResult<Unit, string>> SendApprovalResultAsync(User requester, FormalUbiquitousLanguage ubiquitousLanguage, bool isApproved)
    {
        await Task.Delay(1);
        return FSharpResult<Unit, string>.NewError("SendApprovalResultAsync: Phase A3以降で実装予定");
    }

    // =================================================================
    // 🔄 プライベートヘルパーメソッド
    // =================================================================

    /// <summary>
    /// F#のRole判別共用体を文字列に変換
    /// </summary>
    private static string RoleToString(Role role)
    {
        if (role.IsSuperUser) return "SuperUser";
        if (role.IsProjectManager) return "ProjectManager";
        if (role.IsDomainApprover) return "DomainApprover";
        if (role.IsGeneralUser) return "GeneralUser";
        return "Unknown";
    }

    /// <summary>
    /// 権限昇格かどうかを判定
    /// セキュリティ監査用の判定ロジック
    /// </summary>
    private static bool IsPrivilegeEscalation(Role previousRole, Role currentRole)
    {
        // Phase A2: 簡易的な権限レベル定義
        var previousLevel = GetRoleLevel(previousRole);
        var currentLevel = GetRoleLevel(currentRole);
        
        return currentLevel > previousLevel;
    }

    /// <summary>
    /// ロールの権限レベルを数値で取得
    /// 数値が大きいほど高い権限
    /// </summary>
    private static int GetRoleLevel(Role role)
    {
        if (role.IsSuperUser) return 4;
        if (role.IsProjectManager) return 3;
        if (role.IsDomainApprover) return 2;
        if (role.IsGeneralUser) return 1;
        return 0;
    }
}