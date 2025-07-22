using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.FSharp.Core;
using UbiquitousLanguageManager.Application;
using UbiquitousLanguageManager.Domain;

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
    private readonly IEmailSender _emailSender;

    /// <summary>
    /// NotificationServiceのコンストラクタ
    /// </summary>
    public NotificationService(
        Microsoft.Extensions.Logging.ILogger<NotificationService> logger,
        IEmailSender emailSender)
    {
        _logger = logger;
        _emailSender = emailSender;
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

    // 🔄 Phase A3: パスワードリセット関連通知実装
    // 【F#初学者向け解説】
    // Step2で実装したメール送信基盤（IEmailSender）を活用して、
    // パスワードリセット関連の通知メールを送信します。
    // セキュリティ上重要な機能のため、適切なログ出力とエラーハンドリングを行います。

    /// <summary>
    /// パスワードリセット要求メール送信
    /// </summary>
    /// <param name="email">送信先メールアドレス</param>
    /// <param name="resetToken">パスワードリセットトークン</param>
    /// <param name="resetUrl">パスワード再設定URL</param>
    /// <returns>送信結果（成功時はunit、失敗時はエラーメッセージ）</returns>
    /// <remarks>
    /// 📧 Step2のIEmailSender基盤を活用したメール送信
    /// 🎨 HTMLテンプレートによる美しいメール表示
    /// 📊 ADR_008準拠: セキュリティログ出力
    /// </remarks>
    public async Task<FSharpResult<Unit, string>> SendPasswordResetEmailAsync(Email email, string resetToken, string resetUrl)
    {
        try
        {
            // 📊 ADR_008準拠: セキュリティイベントログ（Information レベル）
            _logger.LogInformation("Sending password reset email. Email: {Email}", email.Value);

            // 🎨 HTMLメールテンプレート作成
            var subject = "【ユビキタス言語管理システム】パスワードリセットのご案内";
            var htmlContent = CreatePasswordResetEmailTemplate(email.Value, resetUrl);

            // 📮 Step2で実装したメール送信基盤を使用
            var sendResult = await _emailSender.SendEmailAsync(email.Value, subject, htmlContent);

            if (sendResult.IsError)
            {
                // ❌ メール送信失敗
                _logger.LogError("Failed to send password reset email. Email: {Email}, Error: {Error}", 
                    email.Value, sendResult.ErrorValue);
                return FSharpResult<Unit, string>.NewError($"メール送信に失敗しました: {sendResult.ErrorValue}");
            }

            // ✅ 送信成功ログ: セキュリティ監査用
            _logger.LogInformation("Password reset email sent successfully. Email: {Email}", email.Value);

            return FSharpResult<Unit, string>.NewOk(null!);
        }
        catch (Exception ex)
        {
            // ❌ 予期しないエラー: ADR_008準拠（Error レベル）
            _logger.LogError(ex, "Unexpected error while sending password reset email. Email: {Email}", email.Value);
            return FSharpResult<Unit, string>.NewError($"メール送信中にシステムエラーが発生しました: {ex.Message}");
        }
    }

    /// <summary>
    /// パスワードリセット完了通知メール送信
    /// </summary>
    /// <param name="email">送信先メールアドレス</param>
    /// <returns>送信結果（成功時はunit、失敗時はエラーメッセージ）</returns>
    /// <remarks>
    /// 🔐 セキュリティ完了通知: パスワード変更の確認メール
    /// 📊 ADR_008準拠: セキュリティログ出力
    /// </remarks>
    public async Task<FSharpResult<Unit, string>> SendPasswordResetConfirmationAsync(Email email)
    {
        try
        {
            // 📊 ADR_008準拠: セキュリティイベントログ（Information レベル）
            _logger.LogInformation("Sending password reset confirmation email. Email: {Email}", email.Value);

            // 🎨 パスワード変更完了通知メールテンプレート
            var subject = "【ユビキタス言語管理システム】パスワード変更完了のお知らせ";
            var htmlContent = CreatePasswordResetConfirmationEmailTemplate(email.Value);

            // 📮 Step2で実装したメール送信基盤を使用
            var sendResult = await _emailSender.SendEmailAsync(email.Value, subject, htmlContent);

            if (sendResult.IsError)
            {
                // ⚠️ 通知メール送信失敗（パスワード変更自体は成功のため Warning レベル）
                _logger.LogWarning("Failed to send password reset confirmation email. Email: {Email}, Error: {Error}", 
                    email.Value, sendResult.ErrorValue);
                return FSharpResult<Unit, string>.NewError($"確認メール送信に失敗しました: {sendResult.ErrorValue}");
            }

            // ✅ 送信成功ログ: セキュリティ監査用
            _logger.LogInformation("Password reset confirmation email sent successfully. Email: {Email}", email.Value);

            return FSharpResult<Unit, string>.NewOk(null!);
        }
        catch (Exception ex)
        {
            // ⚠️ 確認メール送信エラー: Warning レベル（主要処理は完了済みのため）
            _logger.LogWarning(ex, "Unexpected error while sending password reset confirmation email. Email: {Email}", email.Value);
            return FSharpResult<Unit, string>.NewError($"確認メール送信中にエラーが発生しました: {ex.Message}");
        }
    }

    /// <summary>
    /// パスワードリセット要求メールのHTMLテンプレート作成
    /// </summary>
    /// <param name="email">ユーザーのメールアドレス</param>
    /// <param name="resetUrl">パスワード再設定URL</param>
    /// <returns>HTML形式のメール本文</returns>
    /// <remarks>
    /// 🎨 美しい表示のHTMLメールテンプレート
    /// 🔐 セキュリティ情報とリンク有効期限の明記
    /// </remarks>
    private static string CreatePasswordResetEmailTemplate(string email, string resetUrl)
    {
        return $@"
<!DOCTYPE html>
<html lang=""ja"">
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>パスワードリセット</title>
    <style>
        body {{
            font-family: 'Hiragino Sans', 'Yu Gothic', Arial, sans-serif;
            line-height: 1.6;
            color: #333;
            max-width: 600px;
            margin: 0 auto;
            padding: 20px;
        }}
        .header {{
            background-color: #007bff;
            color: white;
            text-align: center;
            padding: 20px;
            border-radius: 8px 8px 0 0;
        }}
        .content {{
            background-color: #f8f9fa;
            padding: 30px;
            border-radius: 0 0 8px 8px;
            border: 1px solid #dee2e6;
        }}
        .button {{
            display: inline-block;
            background-color: #28a745;
            color: white;
            text-decoration: none;
            padding: 12px 30px;
            border-radius: 5px;
            font-weight: bold;
            margin: 20px 0;
            text-align: center;
        }}
        .warning {{
            background-color: #fff3cd;
            color: #856404;
            border: 1px solid #ffeaa7;
            padding: 15px;
            border-radius: 5px;
            margin: 20px 0;
        }}
        .footer {{
            text-align: center;
            color: #6c757d;
            font-size: 12px;
            margin-top: 20px;
            padding-top: 20px;
            border-top: 1px solid #dee2e6;
        }}
    </style>
</head>
<body>
    <div class=""header"">
        <h1>🔑 パスワードリセット</h1>
        <p>ユビキタス言語管理システム</p>
    </div>
    
    <div class=""content"">
        <h2>パスワードリセットのご案内</h2>
        
        <p>こんにちは、</p>
        
        <p>アカウント <strong>{email}</strong> のパスワードリセット要求を受信しました。</p>
        
        <p>以下のボタンをクリックして、新しいパスワードを設定してください：</p>
        
        <div style=""text-align: center;"">
            <a href=""{resetUrl}"" class=""button"">
                🔐 パスワードを再設定する
            </a>
        </div>
        
        <div class=""warning"">
            <h3>⚠️ 重要な注意事項</h3>
            <ul>
                <li>このリンクの有効期限は <strong>24時間</strong> です</li>
                <li>リンクは一度のみ使用可能です</li>
                <li>心当たりがない場合は、このメールを無視してください</li>
                <li>不審なアクセスがある場合は、管理者にお問い合わせください</li>
            </ul>
        </div>
        
        <p>リンクがクリックできない場合は、以下のURLをブラウザのアドレスバーにコピーしてください：</p>
        <p style=""word-break: break-all; background-color: #e9ecef; padding: 10px; border-radius: 3px; font-family: monospace; font-size: 12px;"">
            {resetUrl}
        </p>
    </div>
    
    <div class=""footer"">
        <p>ユビキタス言語管理システム 自動送信メール</p>
        <p>このメールに返信しないでください</p>
    </div>
</body>
</html>";
    }

    /// <summary>
    /// パスワード変更完了通知メールのHTMLテンプレート作成
    /// </summary>
    /// <param name="email">ユーザーのメールアドレス</param>
    /// <returns>HTML形式のメール本文</returns>
    /// <remarks>
    /// 🔐 セキュリティ完了確認とセキュリティ注意事項の提供
    /// </remarks>
    private static string CreatePasswordResetConfirmationEmailTemplate(string email)
    {
        var timestamp = DateTimeOffset.Now.ToString("yyyy年MM月dd日 HH:mm:ss (JST)");
        
        return $@"
<!DOCTYPE html>
<html lang=""ja"">
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>パスワード変更完了</title>
    <style>
        body {{
            font-family: 'Hiragino Sans', 'Yu Gothic', Arial, sans-serif;
            line-height: 1.6;
            color: #333;
            max-width: 600px;
            margin: 0 auto;
            padding: 20px;
        }}
        .header {{
            background-color: #28a745;
            color: white;
            text-align: center;
            padding: 20px;
            border-radius: 8px 8px 0 0;
        }}
        .content {{
            background-color: #f8f9fa;
            padding: 30px;
            border-radius: 0 0 8px 8px;
            border: 1px solid #dee2e6;
        }}
        .success {{
            background-color: #d4edda;
            color: #155724;
            border: 1px solid #c3e6cb;
            padding: 15px;
            border-radius: 5px;
            margin: 20px 0;
        }}
        .security {{
            background-color: #d1ecf1;
            color: #0c5460;
            border: 1px solid #bee5eb;
            padding: 15px;
            border-radius: 5px;
            margin: 20px 0;
        }}
        .footer {{
            text-align: center;
            color: #6c757d;
            font-size: 12px;
            margin-top: 20px;
            padding-top: 20px;
            border-top: 1px solid #dee2e6;
        }}
    </style>
</head>
<body>
    <div class=""header"">
        <h1>✅ パスワード変更完了</h1>
        <p>ユビキタス言語管理システム</p>
    </div>
    
    <div class=""content"">
        <div class=""success"">
            <h2>🎉 パスワードの変更が完了しました</h2>
        </div>
        
        <p>アカウント <strong>{email}</strong> のパスワードが正常に変更されました。</p>
        
        <p><strong>変更日時:</strong> {timestamp}</p>
        
        <div class=""security"">
            <h3>🔐 セキュリティに関する重要なお知らせ</h3>
            <ul>
                <li>新しいパスワードで安全にログインできます</li>
                <li>既存の全てのセッションは自動的に無効化されました</li>
                <li>他のデバイスでログインし直す必要があります</li>
                <li>心当たりのない変更の場合は、直ちに管理者にお問い合わせください</li>
            </ul>
        </div>
        
        <p>今後ともユビキタス言語管理システムをよろしくお願いいたします。</p>
    </div>
    
    <div class=""footer"">
        <p>ユビキタス言語管理システム 自動送信メール</p>
        <p>このメールに返信しないでください</p>
    </div>
</body>
</html>";
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