using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;
using MimeKit.Text;
using UbiquitousLanguageManager.Contracts.Interfaces;

namespace UbiquitousLanguageManager.Infrastructure.Emailing
{
    /// <summary>
    /// SMTP経由でメールを送信する実装クラス
    /// Phase A3 Step2: Infrastructure層のメール送信実装
    /// </summary>
    public class SmtpEmailSender : IEmailSender
    {
        private readonly SmtpSettings _settings;
        private readonly ILogger<SmtpEmailSender> _logger;
        private readonly ISmtpClient _smtpClient;

        /// <summary>
        /// コンストラクタ（本番用）
        /// </summary>
        public SmtpEmailSender(
            IOptions<SmtpSettings> options,
            ILogger<SmtpEmailSender> logger)
            : this(options, logger, new SmtpClientWrapper())
        {
        }

        /// <summary>
        /// コンストラクタ（テスト用）
        /// </summary>
        public SmtpEmailSender(
            IOptions<SmtpSettings> options,
            ILogger<SmtpEmailSender> logger,
            ISmtpClient smtpClient)
        {
            _settings = options.Value;
            _logger = logger;
            _smtpClient = smtpClient;
        }

        /// <summary>
        /// メールを非同期で送信します
        /// </summary>
        public async Task SendEmailAsync(
            string to,
            string subject,
            string body,
            bool isBodyHtml = true,
            CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Sending email to {To} with subject: {Subject}", to, subject);

                // MimeMessageの作成
                var message = new MimeMessage();
                message.From.Add(new MailboxAddress(_settings.SenderName, _settings.SenderEmail));
                message.To.Add(new MailboxAddress(string.Empty, to));
                message.Subject = subject;

                // 本文の設定（HTML or プレーンテキスト）
                if (isBodyHtml)
                {
                    message.Body = new TextPart(TextFormat.Html) { Text = body };
                }
                else
                {
                    message.Body = new TextPart(TextFormat.Plain) { Text = body };
                }

                // SMTP送信
                await _smtpClient.ConnectAsync(_settings.Host, _settings.Port, _settings.EnableSsl, cancellationToken);
                await _smtpClient.AuthenticateAsync(_settings.Username, _settings.Password, cancellationToken);
                await _smtpClient.SendAsync(message, cancellationToken);
                await _smtpClient.DisconnectAsync(true, cancellationToken);

                _logger.LogInformation("Email sent successfully to {To}", to);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send email to {To}", to);
                throw;
            }
        }

        /// <summary>
        /// パスワードリセットメールを送信します
        /// </summary>
        /// <param name="email">送信先メールアドレス</param>
        /// <param name="resetToken">リセットトークン</param>
        /// <returns>送信成功の可否</returns>
        /// <remarks>
        /// 仕様書2.1.3準拠: パスワードリセットメール送信
        /// リセットリンクの有効期限は24時間
        /// </remarks>
        public async Task<bool> SendPasswordResetEmailAsync(string email, string resetToken)
        {
            try
            {
                // 🔧 入力検証
                if (string.IsNullOrWhiteSpace(email))
                {
                    _logger.LogWarning("Invalid email address provided for password reset");
                    return false;
                }

                // 📧 リセットURLの生成
                // TODO: 実際のURLはアプリケーション設定から取得する必要があります
                var resetUrl = $"https://localhost/reset-password?token={Uri.EscapeDataString(resetToken)}&email={Uri.EscapeDataString(email)}";

                // 📝 メール本文の作成
                var subject = "パスワードリセットのお知らせ";
                var body = $@"
<html>
<body>
    <h2>パスワードリセットのお知らせ</h2>
    <p>パスワードリセットのリクエストを受け付けました。</p>
    <p>以下のリンクをクリックして、新しいパスワードを設定してください：</p>
    <p><a href='{resetUrl}'>パスワードをリセットする</a></p>
    <p>このリンクの有効期限は<strong>24時間</strong>です。</p>
    <p>心当たりがない場合は、このメールを無視してください。</p>
    <hr>
    <p>ユビキタス言語管理システム</p>
</body>
</html>";

                // 📧 メール送信
                await SendEmailAsync(email, subject, body, true);
                
                _logger.LogInformation("Password reset email sent successfully to {Email}", email);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send password reset email to {Email}", email);
                return false;
            }
        }
    }
}