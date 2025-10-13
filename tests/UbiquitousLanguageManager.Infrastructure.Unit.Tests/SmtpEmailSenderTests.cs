using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;
using UbiquitousLanguageManager.Contracts.Interfaces;
using MailKit.Security;
using MailKit;
using MailKit.Net.Smtp;
using MimeKit;
using UbiquitousLanguageManager.Infrastructure.Emailing;
using InfraISmtpClient = UbiquitousLanguageManager.Infrastructure.Emailing.ISmtpClient;

namespace UbiquitousLanguageManager.Infrastructure.Unit.Tests
{
    /// <summary>
    /// SmtpEmailSender 実装の単体テスト
    /// Phase A3 Step2: Infrastructure層のRed Phaseテスト
    /// </summary>
    public class SmtpEmailSenderTests
    {
        private readonly Mock<ILogger<SmtpEmailSender>> _loggerMock;
        private readonly Mock<IOptions<SmtpSettings>> _optionsMock;
        private readonly Mock<IConfiguration> _configurationMock;
        private readonly SmtpSettings _testSettings;

        public SmtpEmailSenderTests()
        {
            _loggerMock = new Mock<ILogger<SmtpEmailSender>>();
            _configurationMock = new Mock<IConfiguration>();
            _configurationMock.Setup(x => x["App:BaseUrl"]).Returns("https://localhost:5001");

            _testSettings = new SmtpSettings
            {
                Host = "localhost",
                Port = 2525,
                Username = "",
                Password = "",
                EnableSsl = false,
                SenderEmail = "noreply@test.com",
                SenderName = "Test System"
            };
            _optionsMock = new Mock<IOptions<SmtpSettings>>();
            _optionsMock.Setup(x => x.Value).Returns(_testSettings);
        }

        /// <summary>
        /// SendEmailAsync_有効なパラメータ_SMTPクライアントを正しく呼び出すべき
        /// </summary>
        [Fact]
        public async Task SendEmailAsync_WithValidParameters_ShouldCallSmtpClient()
        {
            // Arrange
            var mockSmtpClient = new Mock<InfraISmtpClient>();
            mockSmtpClient.Setup(x => x.ConnectAsync(
                It.IsAny<string>(),
                It.IsAny<int>(),
                It.IsAny<bool>(),
                It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
            
            mockSmtpClient.Setup(x => x.AuthenticateAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
            
            mockSmtpClient.Setup(x => x.SendAsync(
                It.IsAny<MimeMessage>(),
                It.IsAny<CancellationToken>(),
                It.IsAny<ITransferProgress>()))
                .Returns(Task.CompletedTask);
            
            mockSmtpClient.Setup(x => x.DisconnectAsync(
                It.IsAny<bool>(),
                It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var emailSender = new SmtpEmailSender(_optionsMock.Object, _loggerMock.Object, _configurationMock.Object, mockSmtpClient.Object);

            // Act
            await emailSender.SendEmailAsync(
                "recipient@example.com",
                "Test Subject",
                "<p>Test HTML Body</p>",
                true);

            // Assert
            mockSmtpClient.Verify(x => x.ConnectAsync(
                _testSettings.Host,
                _testSettings.Port,
                _testSettings.EnableSsl,
                It.IsAny<CancellationToken>()),
                Times.Once);

            mockSmtpClient.Verify(x => x.SendAsync(
                It.Is<MimeMessage>(m => 
                    m.To.ToString().Contains("recipient@example.com") &&
                    m.Subject == "Test Subject"),
                It.IsAny<CancellationToken>(),
                null),
                Times.Once);

            mockSmtpClient.Verify(x => x.DisconnectAsync(
                true,
                It.IsAny<CancellationToken>()),
                Times.Once);
        }

        /// <summary>
        /// SendEmailAsync_プレーンテキスト_正しく処理されるべき
        /// </summary>
        [Fact]
        public async Task SendEmailAsync_WithPlainText_ShouldSetTextBody()
        {
            // Arrange
            var capturedMessage = (MimeMessage)null;
            var mockSmtpClient = new Mock<InfraISmtpClient>();
            
            SetupSmtpClientMock(mockSmtpClient);
            
            mockSmtpClient.Setup(x => x.SendAsync(
                It.IsAny<MimeMessage>(),
                It.IsAny<CancellationToken>(),
                It.IsAny<ITransferProgress>()))
                .Callback<MimeMessage, CancellationToken, ITransferProgress>((msg, ct, progress) => 
                {
                    capturedMessage = msg;
                })
                .Returns(Task.CompletedTask);

            var emailSender = new SmtpEmailSender(_optionsMock.Object, _loggerMock.Object, _configurationMock.Object, mockSmtpClient.Object);

            // Act
            await emailSender.SendEmailAsync(
                "recipient@example.com",
                "Test Subject",
                "Plain text body",
                false);

            // Assert
            Assert.NotNull(capturedMessage);
            Assert.Equal("Plain text body", capturedMessage.TextBody);
            Assert.Null(capturedMessage.HtmlBody);
        }

        /// <summary>
        /// SendEmailAsync_SMTP接続エラー_例外を投げるべき
        /// </summary>
        [Fact]
        public async Task SendEmailAsync_WhenSmtpConnectionFails_ShouldThrowException()
        {
            // Arrange
            var mockSmtpClient = new Mock<InfraISmtpClient>();
            mockSmtpClient.Setup(x => x.ConnectAsync(
                It.IsAny<string>(),
                It.IsAny<int>(),
                It.IsAny<bool>(),
                It.IsAny<CancellationToken>()))
                .ThrowsAsync(new SmtpCommandException(SmtpErrorCode.UnexpectedStatusCode, SmtpStatusCode.ServiceNotAvailable, "Connection failed"));

            var emailSender = new SmtpEmailSender(_optionsMock.Object, _loggerMock.Object, _configurationMock.Object, mockSmtpClient.Object);

            // Act & Assert
            await Assert.ThrowsAsync<SmtpCommandException>(async () =>
                await emailSender.SendEmailAsync(
                    "recipient@example.com",
                    "Test Subject",
                    "Test Body",
                    true));
        }

        /// <summary>
        /// SendEmailAsync_認証エラー_適切にログ出力して例外を投げるべき
        /// </summary>
        [Fact]
        public async Task SendEmailAsync_WhenAuthenticationFails_ShouldLogAndThrow()
        {
            // Arrange
            var mockSmtpClient = new Mock<InfraISmtpClient>();
            mockSmtpClient.Setup(x => x.ConnectAsync(
                It.IsAny<string>(),
                It.IsAny<int>(),
                It.IsAny<bool>(),
                It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
            
            mockSmtpClient.Setup(x => x.AuthenticateAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
                .ThrowsAsync(new MailKit.Security.AuthenticationException("Invalid credentials"));

            var emailSender = new SmtpEmailSender(_optionsMock.Object, _loggerMock.Object, _configurationMock.Object, mockSmtpClient.Object);

            // Act & Assert
            await Assert.ThrowsAsync<MailKit.Security.AuthenticationException>(async () =>
                await emailSender.SendEmailAsync(
                    "recipient@example.com",
                    "Test Subject",
                    "Test Body",
                    true));

            // ログ出力の確認
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Failed to send email")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        /// <summary>
        /// SendEmailAsync_キャンセレーション_正しく処理されるべき
        /// </summary>
        [Fact]
        public async Task SendEmailAsync_WhenCancelled_ShouldThrowOperationCancelledException()
        {
            // Arrange
            var cts = new CancellationTokenSource();
            cts.Cancel();
            
            var mockSmtpClient = new Mock<InfraISmtpClient>();
            mockSmtpClient.Setup(x => x.ConnectAsync(
                It.IsAny<string>(),
                It.IsAny<int>(),
                It.IsAny<bool>(),
                It.IsAny<CancellationToken>()))
                .ThrowsAsync(new OperationCanceledException());

            var emailSender = new SmtpEmailSender(_optionsMock.Object, _loggerMock.Object, _configurationMock.Object, mockSmtpClient.Object);

            // Act & Assert
            await Assert.ThrowsAsync<OperationCanceledException>(async () =>
                await emailSender.SendEmailAsync(
                    "recipient@example.com",
                    "Test Subject",
                    "Test Body",
                    true,
                    cts.Token));
        }

        private void SetupSmtpClientMock(Mock<InfraISmtpClient> mockSmtpClient)
        {
            mockSmtpClient.Setup(x => x.ConnectAsync(
                It.IsAny<string>(),
                It.IsAny<int>(),
                It.IsAny<bool>(),
                It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
            
            mockSmtpClient.Setup(x => x.AuthenticateAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
            
            mockSmtpClient.Setup(x => x.SendAsync(
                It.IsAny<MimeMessage>(),
                It.IsAny<CancellationToken>(),
                It.IsAny<ITransferProgress>()))
                .Returns(Task.CompletedTask);
            
            mockSmtpClient.Setup(x => x.DisconnectAsync(
                It.IsAny<bool>(),
                It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
        }
    }
}