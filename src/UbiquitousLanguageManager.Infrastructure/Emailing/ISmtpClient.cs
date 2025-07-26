using System;
using System.Threading;
using System.Threading.Tasks;
using MailKit;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace UbiquitousLanguageManager.Infrastructure.Emailing
{
    /// <summary>
    /// SMTPクライアントのインターフェース（テスト可能性のため）
    /// </summary>
    public interface ISmtpClient : IDisposable
    {
        /// <summary>
        /// 接続状態を取得します
        /// </summary>
        bool IsConnected { get; }
        /// <summary>
        /// SMTPサーバーに接続します
        /// </summary>
        Task ConnectAsync(string host, int port, bool useSsl, CancellationToken cancellationToken = default);

        /// <summary>
        /// SMTP認証を行います
        /// </summary>
        Task AuthenticateAsync(string userName, string password, CancellationToken cancellationToken = default);

        /// <summary>
        /// メールを送信します
        /// </summary>
        Task SendAsync(MimeMessage message, CancellationToken cancellationToken = default, ITransferProgress? progress = null);

        /// <summary>
        /// SMTPサーバーから切断します
        /// </summary>
        Task DisconnectAsync(bool quit, CancellationToken cancellationToken = default);
    }

    /// <summary>
    /// MailKitのSmtpClientのラッパー実装
    /// </summary>
    public class SmtpClientWrapper : ISmtpClient
    {
        private readonly SmtpClient _smtpClient;

        /// <summary>
        /// SmtpClientWrapperのコンストラクタ
        /// </summary>
        public SmtpClientWrapper()
        {
            _smtpClient = new SmtpClient();
        }

        /// <inheritdoc/>
        public bool IsConnected => _smtpClient.IsConnected;

        /// <inheritdoc/>
        public async Task ConnectAsync(string host, int port, bool useSsl, CancellationToken cancellationToken = default)
        {
            await _smtpClient.ConnectAsync(host, port, useSsl ? SecureSocketOptions.StartTls : SecureSocketOptions.None, cancellationToken);
        }

        /// <inheritdoc/>
        public async Task AuthenticateAsync(string userName, string password, CancellationToken cancellationToken = default)
        {
            if (!string.IsNullOrEmpty(userName) && !string.IsNullOrEmpty(password))
            {
                await _smtpClient.AuthenticateAsync(userName, password, cancellationToken);
            }
        }

        /// <inheritdoc/>
        public async Task SendAsync(MimeMessage message, CancellationToken cancellationToken = default, ITransferProgress? progress = null)
        {
            await _smtpClient.SendAsync(message, cancellationToken, progress);
        }

        /// <inheritdoc/>
        public async Task DisconnectAsync(bool quit, CancellationToken cancellationToken = default)
        {
            await _smtpClient.DisconnectAsync(quit, cancellationToken);
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            _smtpClient?.Dispose();
        }
    }
}