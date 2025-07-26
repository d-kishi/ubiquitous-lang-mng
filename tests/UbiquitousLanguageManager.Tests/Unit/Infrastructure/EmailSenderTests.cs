using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;
using NSubstitute;
using UbiquitousLanguageManager.Contracts.Interfaces;
using SmtpClient = UbiquitousLanguageManager.Infrastructure.Emailing.ISmtpClient;
using UbiquitousLanguageManager.Infrastructure.Emailing;
using Xunit;

namespace UbiquitousLanguageManager.Tests.Unit.Infrastructure
{
    /// <summary>
    /// メール送信サービスの単体テスト
    /// 仕様書2.1.3準拠: パスワードリセットメール送信機能
    /// </summary>
    public class EmailSenderTests
    {
        private readonly SmtpClient _mockSmtpClient;
        private readonly IOptions<SmtpSettings> _mockOptions;
        private readonly ILogger<SmtpEmailSender> _mockLogger;
        private readonly SmtpEmailSender _emailSender;
        private readonly SmtpSettings _smtpSettings;

        public EmailSenderTests()
        {
            // 🔧 テスト用SMTP設定
            _smtpSettings = new SmtpSettings
            {
                Host = "localhost",
                Port = 1025,
                EnableSsl = false,
                SenderEmail = "noreply@example.com",
                SenderName = "ユビキタス言語管理システム"
            };

            _mockSmtpClient = Substitute.For<SmtpClient>();
            _mockOptions = Substitute.For<IOptions<SmtpSettings>>();
            _mockOptions.Value.Returns(_smtpSettings);
            _mockLogger = Substitute.For<ILogger<SmtpEmailSender>>();

            _emailSender = new SmtpEmailSender(
                _mockOptions,
                _mockLogger,
                _mockSmtpClient);
        }

        [Fact]
        public async Task SendPasswordResetEmailAsync_正常なパラメータでメール送信成功()
        {
            // Arrange
            var toEmail = "user@example.com";
            var resetToken = "test-reset-token-12345";
            var expectedResetUrl = $"https://localhost/reset-password?token={Uri.EscapeDataString(resetToken)}&email={Uri.EscapeDataString(toEmail)}";

            // 🎯 モック設定: SMTP接続・送信成功
            _mockSmtpClient.IsConnected.Returns(false);
            _mockSmtpClient.ConnectAsync(
                _smtpSettings.Host, 
                _smtpSettings.Port, 
                _smtpSettings.EnableSsl, 
                Arg.Any<CancellationToken>())
                .Returns(Task.CompletedTask);
            
            MimeMessage capturedMessage = null;
            _mockSmtpClient.SendAsync(
                Arg.Do<MimeMessage>(msg => capturedMessage = msg), 
                Arg.Any<CancellationToken>())
                .Returns(Task.CompletedTask);

            // Act
            var result = await _emailSender.SendPasswordResetEmailAsync(toEmail, resetToken);

            // Assert
            // ✅ 送信成功確認
            result.Should().BeTrue();

            // 🔍 メール内容検証
            capturedMessage.Should().NotBeNull();
            capturedMessage.From.ToString().Should().Contain(_smtpSettings.SenderEmail);
            capturedMessage.To.ToString().Should().Contain(toEmail);
            capturedMessage.Subject.Should().Be("パスワードリセットのお知らせ");
            
            // 📧 本文にリセットURLが含まれているか確認
            var textBody = capturedMessage.TextBody;
            textBody.Should().Contain(expectedResetUrl);
            textBody.Should().Contain("24時間");
            
            // 🔍 モック検証: 接続・送信・切断の順序
            Received.InOrder(() =>
            {
                _mockSmtpClient.ConnectAsync(
                    _smtpSettings.Host, 
                    _smtpSettings.Port, 
                    SecureSocketOptions.None, 
                    Arg.Any<CancellationToken>());
                _mockSmtpClient.SendAsync(
                    Arg.Any<MimeMessage>(), 
                    Arg.Any<CancellationToken>());
                _mockSmtpClient.DisconnectAsync(true, Arg.Any<CancellationToken>());
            });
        }

        [Fact]
        public async Task SendPasswordResetEmailAsync_SMTP接続エラーで失敗()
        {
            // Arrange
            var toEmail = "user@example.com";
            var resetToken = "test-token";

            // 🎯 モック設定: SMTP接続失敗
            _mockSmtpClient.IsConnected.Returns(false);
            _mockSmtpClient.ConnectAsync(
                Arg.Any<string>(), 
                Arg.Any<int>(), 
                Arg.Any<bool>(), 
                Arg.Any<CancellationToken>())
                .ThrowsAsync(new Exception("Connection refused"));

            // Act
            var result = await _emailSender.SendPasswordResetEmailAsync(toEmail, resetToken);

            // Assert
            // ❌ 送信失敗確認
            result.Should().BeFalse();
            
            // 🚫 送信メソッドは呼ばれないことを確認
            await _mockSmtpClient.DidNotReceive()
                .SendAsync(Arg.Any<MimeMessage>(), Arg.Any<CancellationToken>());
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public async Task SendPasswordResetEmailAsync_無効なメールアドレスで失敗(string invalidEmail)
        {
            // Arrange
            var resetToken = "test-token";

            // Act
            var result = await _emailSender.SendPasswordResetEmailAsync(invalidEmail, resetToken);

            // Assert
            // ❌ バリデーションエラー
            result.Should().BeFalse();
            
            // 🚫 SMTP接続すら試みないことを確認
            await _mockSmtpClient.DidNotReceive()
                .ConnectAsync(
                    Arg.Any<string>(), 
                    Arg.Any<int>(), 
                    Arg.Any<bool>(), 
                    Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task SendPasswordResetEmailAsync_既に接続済みの場合は再接続しない()
        {
            // Arrange
            var toEmail = "user@example.com";
            var resetToken = "test-token";

            // 🎯 モック設定: 既に接続済み
            _mockSmtpClient.IsConnected.Returns(true);
            _mockSmtpClient.SendAsync(
                Arg.Any<MimeMessage>(), 
                Arg.Any<CancellationToken>())
                .Returns(Task.CompletedTask);

            // Act
            var result = await _emailSender.SendPasswordResetEmailAsync(toEmail, resetToken);

            // Assert
            // ✅ 送信成功
            result.Should().BeTrue();
            
            // 🚫 再接続しないことを確認
            await _mockSmtpClient.DidNotReceive()
                .ConnectAsync(
                    Arg.Any<string>(), 
                    Arg.Any<int>(), 
                    Arg.Any<bool>(), 
                    Arg.Any<CancellationToken>());
            
            // ✅ 送信は実行される
            await _mockSmtpClient.Received(1)
                .SendAsync(Arg.Any<MimeMessage>(), Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task SendEmailAsync_汎用メール送信メソッドのテスト()
        {
            // Arrange
            var toEmail = "recipient@example.com";
            var subject = "テストメール";
            var body = "これはテストメールです。";

            _mockSmtpClient.IsConnected.Returns(false);
            _mockSmtpClient.ConnectAsync(
                Arg.Any<string>(), 
                Arg.Any<int>(), 
                Arg.Any<bool>(), 
                Arg.Any<CancellationToken>())
                .Returns(Task.CompletedTask);
            
            MimeMessage capturedMessage = null;
            _mockSmtpClient.SendAsync(
                Arg.Do<MimeMessage>(msg => capturedMessage = msg), 
                Arg.Any<CancellationToken>())
                .Returns(Task.CompletedTask);

            // Act
            await _emailSender.SendEmailAsync(toEmail, subject, body);

            // Assert
            // 🔍 メール内容検証
            capturedMessage.Should().NotBeNull();
            capturedMessage.Subject.Should().Be(subject);
            capturedMessage.TextBody.Should().Be(body);
        }

        [Fact]
        public async Task SendEmailAsync_送信タイムアウトで例外発生()
        {
            // Arrange
            var toEmail = "user@example.com";
            var subject = "テスト";
            var body = "本文";

            _mockSmtpClient.IsConnected.Returns(true);
            _mockSmtpClient.SendAsync(
                Arg.Any<MimeMessage>(), 
                Arg.Any<CancellationToken>())
                .ThrowsAsync(new TimeoutException("Send timeout"));

            // Act & Assert
            // ⏱️ タイムアウト例外がそのまま伝播することを確認
            await Assert.ThrowsAsync<TimeoutException>(
                () => _emailSender.SendEmailAsync(toEmail, subject, body));
        }
    }
}