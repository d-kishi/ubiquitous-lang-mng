using System;
using System.IO;
using System.Threading.Tasks;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;
using UbiquitousLanguageManager.Application;
using Microsoft.FSharp.Core;

namespace UbiquitousLanguageManager.Infrastructure.Services;

/// <summary>
/// MailKit を使用したメール送信サービス実装
/// 【初学者向け解説】
/// F# Application層で定義された IEmailSender インターフェースの具体的実装です。
/// Clean Architectureの依存関係逆転の原則に従い、Application層のインターフェースを
/// Infrastructure層で実装することで、外部ライブラリ（MailKit）への依存を抽象化しています。
/// 
/// 【MailKit について】
/// .NET で推奨されるメール送信ライブラリ（System.Net.Mail は非推奨）
/// SMTP、IMAP、POP3 などの豊富な機能を持ち、セキュリティに配慮した実装が可能
/// </summary>
/// <remarks>
/// Phase A3: NotificationService基盤構築で導入
/// 開発環境では Smtp4dev、本番環境では実際のSMTPサーバーとの通信を担当
/// </remarks>
public class MailKitEmailSender : IEmailSender
{
    private readonly SmtpSettings _smtpSettings;
    private readonly Microsoft.Extensions.Logging.ILogger<MailKitEmailSender> _logger;

    /// <summary>
    /// コンストラクタ: 依存性注入によるサービス初期化
    /// 【初学者向け解説】
    /// IOptions&lt;SmtpSettings&gt;: ASP.NET Core の Options パターン
    /// appsettings.json から設定を読み込み、型安全に使用できます
    /// ILogger: 構造化ログ出力のためのインターフェース（ADR_008準拠）
    /// </summary>
    public MailKitEmailSender(IOptions<SmtpSettings> smtpSettings, Microsoft.Extensions.Logging.ILogger<MailKitEmailSender> logger)
    {
        _smtpSettings = smtpSettings.Value ?? throw new ArgumentNullException(nameof(smtpSettings));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        // 設定の妥当性を初期化時にチェック
        if (!_smtpSettings.IsValid())
        {
            throw new InvalidOperationException($"Invalid SMTP settings: {_smtpSettings}");
        }

        _logger.LogInformation("MailKitEmailSender initialized with settings: {SmtpSettings}", 
            _smtpSettings.ToString());
    }

    /// <summary>
    /// HTML メール送信（基本メソッド）
    /// 【初学者向け解説】
    /// async/await: 非同期処理により、メール送信中でもアプリケーションがブロックされません
    /// Result&lt;unit, string&gt;: F#のResult型を使用した関数型エラーハンドリング
    /// </summary>
    public async Task<FSharpResult<Unit, string>> SendEmailAsync(string email, string subject, string htmlMessage)
    {
        try
        {
            _logger.LogDebug("Starting to send email to: {Email}, Subject: {Subject}", email, subject);

            // MimeMessage: RFC準拠のメールメッセージオブジェクト
            var message = new MimeMessage();
            
            // 送信者情報設定
            message.From.Add(new MailboxAddress(_smtpSettings.SenderName, _smtpSettings.SenderEmail));
            
            // 受信者情報設定
            message.To.Add(new MailboxAddress("", email));
            
            // 件名設定
            message.Subject = subject;

            // HTMLボディ設定（BodyBuilder使用でHTMLとプレーンテキストの両方を設定可能）
            var bodyBuilder = new BodyBuilder
            {
                HtmlBody = htmlMessage
            };
            message.Body = bodyBuilder.ToMessageBody();

            // SMTP クライアントでメール送信実行
            using var client = new SmtpClient();
            
            // SMTP サーバーに接続
            await ConnectToSmtpServerAsync(client);
            
            // 認証が必要な場合は認証実行
            if (_smtpSettings.RequireAuthentication)
            {
                await client.AuthenticateAsync(_smtpSettings.Username, _smtpSettings.Password);
                _logger.LogDebug("SMTP authentication successful");
            }
            
            // メール送信実行
            await client.SendAsync(message);
            _logger.LogInformation("Email sent successfully to: {Email}", email);
            
            // SMTP サーバーから切断
            await client.DisconnectAsync(true);

            return FSharpResult<Unit, string>.NewOk(default(Unit)!);
        }
        catch (Exception ex)
        {
            var errorMessage = $"Failed to send email to {email}: {ex.Message}";
            _logger.LogError(ex, "Email sending failed for: {Email}", email);
            return FSharpResult<Unit, string>.NewError(errorMessage);
        }
    }

    /// <summary>
    /// プレーンテキストメール送信
    /// HTMLメールが表示できない環境や、シンプルなテキストメールを送信する場合に使用
    /// </summary>
    public async Task<FSharpResult<Unit, string>> SendPlainTextEmailAsync(string email, string subject, string textMessage)
    {
        try
        {
            _logger.LogDebug("Starting to send plain text email to: {Email}", email);

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(_smtpSettings.SenderName, _smtpSettings.SenderEmail));
            message.To.Add(new MailboxAddress("", email));
            message.Subject = subject;

            // プレーンテキストのみのボディ設定
            var bodyBuilder = new BodyBuilder
            {
                TextBody = textMessage
            };
            message.Body = bodyBuilder.ToMessageBody();

            using var client = new SmtpClient();
            await ConnectToSmtpServerAsync(client);
            
            if (_smtpSettings.RequireAuthentication)
            {
                await client.AuthenticateAsync(_smtpSettings.Username, _smtpSettings.Password);
            }
            
            await client.SendAsync(message);
            await client.DisconnectAsync(true);

            _logger.LogInformation("Plain text email sent successfully to: {Email}", email);
            return FSharpResult<Unit, string>.NewOk(default(Unit)!);
        }
        catch (Exception ex)
        {
            var errorMessage = $"Failed to send plain text email to {email}: {ex.Message}";
            _logger.LogError(ex, "Plain text email sending failed for: {Email}", email);
            return FSharpResult<Unit, string>.NewError(errorMessage);
        }
    }

    /// <summary>
    /// 添付ファイル付きメール送信
    /// パスワードリセットのPDFや、レポート送信などで使用
    /// </summary>
    public async Task<FSharpResult<Unit, string>> SendEmailWithAttachmentAsync(string email, string subject, string htmlMessage, string attachmentPath, string attachmentName)
    {
        try
        {
            _logger.LogDebug("Starting to send email with attachment to: {Email}", email);

            // 添付ファイルの存在チェック
            if (!File.Exists(attachmentPath))
            {
                var errorMessage = $"Attachment file not found: {attachmentPath}";
                _logger.LogError(errorMessage);
                return FSharpResult<Unit, string>.NewError(errorMessage);
            }

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(_smtpSettings.SenderName, _smtpSettings.SenderEmail));
            message.To.Add(new MailboxAddress("", email));
            message.Subject = subject;

            var bodyBuilder = new BodyBuilder
            {
                HtmlBody = htmlMessage
            };

            // 添付ファイル追加
            await bodyBuilder.Attachments.AddAsync(attachmentPath);
            message.Body = bodyBuilder.ToMessageBody();

            using var client = new SmtpClient();
            await ConnectToSmtpServerAsync(client);
            
            if (_smtpSettings.RequireAuthentication)
            {
                await client.AuthenticateAsync(_smtpSettings.Username, _smtpSettings.Password);
            }
            
            await client.SendAsync(message);
            await client.DisconnectAsync(true);

            _logger.LogInformation("Email with attachment sent successfully to: {Email}", email);
            return FSharpResult<Unit, string>.NewOk(default(Unit)!);
        }
        catch (Exception ex)
        {
            var errorMessage = $"Failed to send email with attachment to {email}: {ex.Message}";
            _logger.LogError(ex, "Email with attachment sending failed for: {Email}", email);
            return FSharpResult<Unit, string>.NewError(errorMessage);
        }
    }

    /// <summary>
    /// SMTP サーバーへの接続処理（プライベートメソッド）
    /// 【初学者向け解説】
    /// 開発環境（Smtp4dev）と本番環境でのSSL/TLS設定の違いを吸収
    /// SecureSocketOptions: メール送信のセキュリティレベル設定
    /// </summary>
    private async Task ConnectToSmtpServerAsync(SmtpClient client)
    {
        SecureSocketOptions secureOptions;
        
        if (_smtpSettings.EnableSsl)
        {
            // 本番環境: SSL/TLS使用
            secureOptions = SecureSocketOptions.StartTlsWhenAvailable;
        }
        else
        {
            // 開発環境（Smtp4dev等）: 暗号化なし
            secureOptions = SecureSocketOptions.None;
        }

        await client.ConnectAsync(_smtpSettings.Server, _smtpSettings.Port, secureOptions);
        _logger.LogDebug("Connected to SMTP server: {Server}:{Port} (SSL: {EnableSsl})", 
            _smtpSettings.Server, _smtpSettings.Port, _smtpSettings.EnableSsl);
    }
}