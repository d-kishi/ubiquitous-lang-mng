using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Xunit;
using UbiquitousLanguageManager.Contracts.Interfaces;
using UbiquitousLanguageManager.Infrastructure.Emailing;

namespace UbiquitousLanguageManager.Tests.Integration
{
    /// <summary>
    /// メール送信基盤の統合テスト
    /// Phase A3 Step2: 統合動作確認
    /// </summary>
    public class EmailIntegrationTests : IDisposable
    {
        private readonly ServiceProvider _serviceProvider;

        public EmailIntegrationTests()
        {
            var services = new ServiceCollection();

            // 設定の構築
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new[]
                {
                    new KeyValuePair<string, string>("SmtpSettings:Host", "localhost"),
                    new KeyValuePair<string, string>("SmtpSettings:Port", "2525"),
                    new KeyValuePair<string, string>("SmtpSettings:Username", ""),
                    new KeyValuePair<string, string>("SmtpSettings:Password", ""),
                    new KeyValuePair<string, string>("SmtpSettings:EnableSsl", "false"),
                    new KeyValuePair<string, string>("SmtpSettings:SenderEmail", "noreply@test.com"),
                    new KeyValuePair<string, string>("SmtpSettings:SenderName", "Test System")
                })
                .Build();

            // サービスの登録
            services.AddSingleton<IConfiguration>(configuration);
            services.Configure<SmtpSettings>(configuration.GetSection("SmtpSettings"));
            services.AddLogging(builder => builder.AddConsole());
            services.AddScoped<IEmailSender, SmtpEmailSender>();

            _serviceProvider = services.BuildServiceProvider();
        }

        /// <summary>
        /// DI経由でIEmailSenderが正しく解決されることを確認
        /// </summary>
        [Fact]
        public void EmailSender_ShouldResolveFromDI()
        {
            // Act
            var emailSender = _serviceProvider.GetService<IEmailSender>();

            // Assert
            Assert.NotNull(emailSender);
            Assert.IsType<SmtpEmailSender>(emailSender);
        }

        /// <summary>
        /// SmtpSettingsが正しく設定されることを確認
        /// </summary>
        [Fact]
        public void SmtpSettings_ShouldBeConfiguredCorrectly()
        {
            // Act
            var options = _serviceProvider.GetService<Microsoft.Extensions.Options.IOptions<SmtpSettings>>();
            var settings = options?.Value;

            // Assert
            Assert.NotNull(settings);
            Assert.Equal("localhost", settings.Host);
            Assert.Equal(2525, settings.Port);
            Assert.Equal("noreply@test.com", settings.SenderEmail);
            Assert.False(settings.EnableSsl);
        }

        /// <summary>
        /// 統合環境でメール送信が正しく動作することを確認（モックSMTP使用）
        /// </summary>
        [Fact(Skip = "実際のSMTPサーバーが必要なため、CI環境ではスキップ")]
        public async Task SendEmailAsync_IntegrationTest_ShouldSendSuccessfully()
        {
            // Arrange
            var emailSender = _serviceProvider.GetRequiredService<IEmailSender>();

            // Act & Assert - smtp4devが起動していない場合は例外が発生する
            await emailSender.SendEmailAsync(
                "test@example.com",
                "統合テスト",
                "<h1>統合テスト</h1><p>これは統合テストのメールです。</p>",
                true);
        }

        public void Dispose()
        {
            _serviceProvider?.Dispose();
        }
    }
}