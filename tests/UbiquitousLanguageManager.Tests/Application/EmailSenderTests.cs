using System;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Xunit;
using UbiquitousLanguageManager.Contracts.Interfaces;

namespace UbiquitousLanguageManager.Tests.Application
{
    /// <summary>
    /// IEmailSender インターフェースの使用シナリオテスト
    /// メール送信基盤の動作検証とエラーハンドリングテスト
    /// </summary>
    public class EmailSenderTests
    {
        /// <summary>
        /// SendEmailAsync_基本的なメール送信_成功するべき
        /// </summary>
        [Fact]
        public async Task SendEmailAsync_WithValidParameters_ShouldSendSuccessfully()
        {
            // Arrange
            var mockEmailSender = new Mock<IEmailSender>();
            mockEmailSender
                .Setup(x => x.SendEmailAsync(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<bool>(),
                    It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var to = "test@example.com";
            var subject = "Test Subject";
            var body = "<p>Test Body</p>";
            var isBodyHtml = true;

            // Act
            await mockEmailSender.Object.SendEmailAsync(to, subject, body, isBodyHtml);

            // Assert
            mockEmailSender.Verify(
                x => x.SendEmailAsync(to, subject, body, isBodyHtml, It.IsAny<CancellationToken>()),
                Times.Once);
        }

        /// <summary>
        /// SendEmailAsync_HTMLとプレーンテキストの切り替え_正しく処理されるべき
        /// </summary>
        [Theory]
        [InlineData(true, "<p>HTML Body</p>")]
        [InlineData(false, "Plain Text Body")]
        public async Task SendEmailAsync_WithDifferentBodyTypes_ShouldHandleCorrectly(bool isHtml, string body)
        {
            // Arrange
            var mockEmailSender = new Mock<IEmailSender>();
            mockEmailSender
                .Setup(x => x.SendEmailAsync(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<bool>(),
                    It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            // Act
            await mockEmailSender.Object.SendEmailAsync("test@example.com", "Subject", body, isHtml);

            // Assert
            mockEmailSender.Verify(
                x => x.SendEmailAsync(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    body,
                    isHtml,
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        /// <summary>
        /// SendEmailAsync_キャンセレーショントークン_正しく伝播されるべき
        /// </summary>
        [Fact]
        public async Task SendEmailAsync_WithCancellationToken_ShouldPropagate()
        {
            // Arrange
            var mockEmailSender = new Mock<IEmailSender>();
            var cancellationToken = new CancellationToken();
            
            mockEmailSender
                .Setup(x => x.SendEmailAsync(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<bool>(),
                    It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            // Act
            await mockEmailSender.Object.SendEmailAsync(
                "test@example.com",
                "Subject",
                "Body",
                true,
                cancellationToken);

            // Assert
            mockEmailSender.Verify(
                x => x.SendEmailAsync(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<bool>(),
                    cancellationToken),
                Times.Once);
        }

        /// <summary>
        /// SendEmailAsync_例外発生時_適切に処理されるべき
        /// </summary>
        [Fact]
        public async Task SendEmailAsync_WhenExceptionOccurs_ShouldThrow()
        {
            // Arrange
            var mockEmailSender = new Mock<IEmailSender>();
            var expectedException = new InvalidOperationException("SMTP connection failed");
            
            mockEmailSender
                .Setup(x => x.SendEmailAsync(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<bool>(),
                    It.IsAny<CancellationToken>()))
                .ThrowsAsync(expectedException);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(async () =>
                await mockEmailSender.Object.SendEmailAsync(
                    "test@example.com",
                    "Subject",
                    "Body",
                    true));

            Assert.Equal("SMTP connection failed", exception.Message);
        }
    }
}