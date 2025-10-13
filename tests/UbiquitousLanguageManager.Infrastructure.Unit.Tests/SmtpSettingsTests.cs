using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Xunit;
using UbiquitousLanguageManager.Infrastructure.Emailing;

namespace UbiquitousLanguageManager.Infrastructure.Unit.Tests
{
    /// <summary>
    /// SmtpSettings 設定管理のテスト
    /// Phase A3 Step2: 設定管理のRed Phaseテスト
    /// </summary>
    public class SmtpSettingsTests
    {
        /// <summary>
        /// SmtpSettings_設定ファイルから読み込み_正しくバインドされるべき
        /// </summary>
        [Fact]
        public void SmtpSettings_FromConfiguration_ShouldBindCorrectly()
        {
            // Arrange
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new[]
                {
                    new KeyValuePair<string, string>("SmtpSettings:Host", "smtp.example.com"),
                    new KeyValuePair<string, string>("SmtpSettings:Port", "587"),
                    new KeyValuePair<string, string>("SmtpSettings:Username", "testuser"),
                    new KeyValuePair<string, string>("SmtpSettings:Password", "testpass"),
                    new KeyValuePair<string, string>("SmtpSettings:EnableSsl", "true"),
                    new KeyValuePair<string, string>("SmtpSettings:SenderEmail", "noreply@example.com"),
                    new KeyValuePair<string, string>("SmtpSettings:SenderName", "Test System")
                })
                .Build();

            var services = new ServiceCollection();
            services.Configure<SmtpSettings>(configuration.GetSection("SmtpSettings"));
            var serviceProvider = services.BuildServiceProvider();
            var options = serviceProvider.GetRequiredService<IOptions<SmtpSettings>>();

            // Act
            var settings = options.Value;

            // Assert
            Assert.Equal("smtp.example.com", settings.Host);
            Assert.Equal(587, settings.Port);
            Assert.Equal("testuser", settings.Username);
            Assert.Equal("testpass", settings.Password);
            Assert.True(settings.EnableSsl);
            Assert.Equal("noreply@example.com", settings.SenderEmail);
            Assert.Equal("Test System", settings.SenderName);
        }

        /// <summary>
        /// SmtpSettings_デフォルト値_適切に設定されるべき
        /// </summary>
        [Fact]
        public void SmtpSettings_DefaultValues_ShouldBeSet()
        {
            // Arrange & Act
            var settings = new SmtpSettings();

            // Assert
            Assert.Equal(string.Empty, settings.Host);
            Assert.Equal(0, settings.Port);
            Assert.Equal(string.Empty, settings.Username);
            Assert.Equal(string.Empty, settings.Password);
            Assert.False(settings.EnableSsl);
            Assert.Equal(string.Empty, settings.SenderEmail);
            Assert.Equal(string.Empty, settings.SenderName);
        }

        /// <summary>
        /// SmtpSettings_開発環境設定_smtp4dev用に設定されるべき
        /// </summary>
        [Fact]
        public void SmtpSettings_DevelopmentEnvironment_ShouldConfigureForSmtp4dev()
        {
            // Arrange
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new[]
                {
                    new KeyValuePair<string, string>("SmtpSettings:Host", "localhost"),
                    new KeyValuePair<string, string>("SmtpSettings:Port", "2525"),
                    new KeyValuePair<string, string>("SmtpSettings:Username", ""),
                    new KeyValuePair<string, string>("SmtpSettings:Password", ""),
                    new KeyValuePair<string, string>("SmtpSettings:EnableSsl", "false"),
                    new KeyValuePair<string, string>("SmtpSettings:SenderEmail", "noreply@ubiquitous-lang-mng.local"),
                    new KeyValuePair<string, string>("SmtpSettings:SenderName", "Ubiquitous Language Manager (Dev)")
                })
                .Build();

            var services = new ServiceCollection();
            services.Configure<SmtpSettings>(configuration.GetSection("SmtpSettings"));
            var serviceProvider = services.BuildServiceProvider();
            var options = serviceProvider.GetRequiredService<IOptions<SmtpSettings>>();

            // Act
            var settings = options.Value;

            // Assert
            Assert.Equal("localhost", settings.Host);
            Assert.Equal(2525, settings.Port);
            Assert.Equal(string.Empty, settings.Username);
            Assert.Equal(string.Empty, settings.Password);
            Assert.False(settings.EnableSsl);
            Assert.Equal("noreply@ubiquitous-lang-mng.local", settings.SenderEmail);
            Assert.Equal("Ubiquitous Language Manager (Dev)", settings.SenderName);
        }

        /// <summary>
        /// SmtpSettings_環境変数から読み込み_正しく上書きされるべき
        /// </summary>
        [Fact]
        public void SmtpSettings_FromEnvironmentVariables_ShouldOverrideConfiguration()
        {
            // Arrange
            Environment.SetEnvironmentVariable("SmtpSettings__Username", "env_user");
            Environment.SetEnvironmentVariable("SmtpSettings__Password", "env_pass");

            try
            {
                var configuration = new ConfigurationBuilder()
                    .AddInMemoryCollection(new[]
                    {
                        new KeyValuePair<string, string>("SmtpSettings:Host", "smtp.example.com"),
                        new KeyValuePair<string, string>("SmtpSettings:Port", "587"),
                        new KeyValuePair<string, string>("SmtpSettings:Username", "config_user"),
                        new KeyValuePair<string, string>("SmtpSettings:Password", "config_pass"),
                        new KeyValuePair<string, string>("SmtpSettings:EnableSsl", "true"),
                        new KeyValuePair<string, string>("SmtpSettings:SenderEmail", "noreply@example.com"),
                        new KeyValuePair<string, string>("SmtpSettings:SenderName", "Test System")
                    })
                    .AddEnvironmentVariables()
                    .Build();

                var services = new ServiceCollection();
                services.Configure<SmtpSettings>(configuration.GetSection("SmtpSettings"));
                var serviceProvider = services.BuildServiceProvider();
                var options = serviceProvider.GetRequiredService<IOptions<SmtpSettings>>();

                // Act
                var settings = options.Value;

                // Assert
                Assert.Equal("smtp.example.com", settings.Host);
                Assert.Equal(587, settings.Port);
                Assert.Equal("env_user", settings.Username); // 環境変数で上書き
                Assert.Equal("env_pass", settings.Password); // 環境変数で上書き
                Assert.True(settings.EnableSsl);
                Assert.Equal("noreply@example.com", settings.SenderEmail);
                Assert.Equal("Test System", settings.SenderName);
            }
            finally
            {
                // Cleanup
                Environment.SetEnvironmentVariable("SmtpSettings__Username", null);
                Environment.SetEnvironmentVariable("SmtpSettings__Password", null);
            }
        }
    }
}