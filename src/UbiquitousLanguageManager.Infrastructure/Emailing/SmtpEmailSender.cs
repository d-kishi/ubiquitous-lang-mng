using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
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
        private readonly IConfiguration _configuration;

        /// <summary>
        /// コンストラクタ（本番用）
        /// </summary>
        public SmtpEmailSender(
            IOptions<SmtpSettings> options,
            ILogger<SmtpEmailSender> logger,
            IConfiguration configuration)
            : this(options, logger, configuration, new SmtpClientWrapper())
        {
        }

        /// <summary>
        /// コンストラクタ（テスト用）
        /// </summary>
        public SmtpEmailSender(
            IOptions<SmtpSettings> options,
            ILogger<SmtpEmailSender> logger,
            IConfiguration configuration,
            ISmtpClient smtpClient)
        {
            _settings = options.Value;
            _logger = logger;
            _configuration = configuration;
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
            var startTime = DateTime.UtcNow;
            try
            {
                _logger.LogInformation("Starting email send to {To} with subject: {Subject}, HTML: {IsHtml}",
                    to, subject, isBodyHtml);
                _logger.LogDebug("SMTP settings: Host={Host}, Port={Port}, SSL={EnableSsl}",
                    _settings.Host, _settings.Port, _settings.EnableSsl);

                // MimeMessageの作成
                _logger.LogDebug("Creating MimeMessage for {To}", to);
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
                _logger.LogDebug("Connecting to SMTP server {Host}:{Port}", _settings.Host, _settings.Port);
                await _smtpClient.ConnectAsync(_settings.Host, _settings.Port, _settings.EnableSsl, cancellationToken);

                _logger.LogDebug("Authenticating with SMTP server as {Username}", _settings.Username);
                await _smtpClient.AuthenticateAsync(_settings.Username, _settings.Password, cancellationToken);

                _logger.LogDebug("Sending message to {To}", to);
                await _smtpClient.SendAsync(message, cancellationToken);

                _logger.LogDebug("Disconnecting from SMTP server");
                await _smtpClient.DisconnectAsync(true, cancellationToken);

                var duration = DateTime.UtcNow - startTime;
                _logger.LogInformation("Email sent successfully to {To} in {Duration}ms", to, duration.TotalMilliseconds);
            }
            catch (Exception ex)
            {
                var duration = DateTime.UtcNow - startTime;
                _logger.LogError(ex, "Failed to send email to {To} after {Duration}ms - Host: {Host}, Port: {Port}",
                    to, duration.TotalMilliseconds, _settings.Host, _settings.Port);
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
            var startTime = DateTime.UtcNow;
            try
            {
                _logger.LogInformation("Starting password reset email send to {Email}", email);

                // 🔧 入力検証
                if (string.IsNullOrWhiteSpace(email))
                {
                    _logger.LogWarning("Invalid email address provided for password reset");
                    return false;
                }

                if (string.IsNullOrWhiteSpace(resetToken))
                {
                    _logger.LogWarning("Invalid reset token provided for password reset email to {Email}", email);
                    return false;
                }

                // 📧 リセットURLの生成
                // Phase A8 Step6 Stage1: URL設定外部化対応
                var baseUrl = _configuration["App:BaseUrl"] ?? "https://localhost:5001";
                var resetUrl = $"{baseUrl}/reset-password?token={Uri.EscapeDataString(resetToken)}&email={Uri.EscapeDataString(email)}";

                _logger.LogDebug("Generated password reset URL for {Email} with base URL: {BaseUrl}", email, baseUrl);

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

                _logger.LogDebug("Prepared password reset email with subject: {Subject}", subject);

                // 📧 メール送信
                await SendEmailAsync(email, subject, body, true);

                var duration = DateTime.UtcNow - startTime;
                _logger.LogInformation("Password reset email sent successfully to {Email} in {Duration}ms",
                    email, duration.TotalMilliseconds);
                return true;
            }
            catch (Exception ex)
            {
                var duration = DateTime.UtcNow - startTime;
                _logger.LogError(ex, "Failed to send password reset email to {Email} after {Duration}ms",
                    email, duration.TotalMilliseconds);
                return false;
            }
        }
    }
}