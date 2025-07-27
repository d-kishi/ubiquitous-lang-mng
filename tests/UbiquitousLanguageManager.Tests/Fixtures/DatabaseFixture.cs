using System;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using UbiquitousLanguageManager.Infrastructure.Services;
using UbiquitousLanguageManager.Contracts.Interfaces;
using UbiquitousLanguageManager.Infrastructure.Data.Entities;
using UbiquitousLanguageManager.Infrastructure.Data;
using UbiquitousLanguageManager.Infrastructure.Emailing;

namespace UbiquitousLanguageManager.Tests.Fixtures
{
    /// <summary>
    /// 統合テスト用のデータベースフィクスチャ
    /// テスト間のデータ分離とクリーンアップを管理
    /// </summary>
    public class DatabaseFixture : IDisposable
    {
        public IServiceProvider ServiceProvider { get; }
        
        public DatabaseFixture()
        {
            var services = new ServiceCollection();
            
            // 🔧 テスト用In-Memory Database設定
            services.AddDbContext<UbiquitousLanguageDbContext>(options =>
                options.UseInMemoryDatabase($"TestDb_{Guid.NewGuid()}"));
            
            // 🔐 ASP.NET Core Identity設定
            services.AddIdentity<ApplicationUser, IdentityRole>(options =>
            {
                // テスト用にパスワード要件を緩和
                options.Password.RequireDigit = false;
                options.Password.RequireLowercase = false;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;
                options.Password.RequiredLength = 4;
                
                // トークン有効期限設定（仕様書2.1.3: 24時間）
                options.Tokens.PasswordResetTokenProvider = TokenOptions.DefaultProvider;
            })
            .AddEntityFrameworkStores<UbiquitousLanguageDbContext>()
            .AddDefaultTokenProviders();
            
            // 📧 メール送信サービス（テスト用モック）
            services.AddSingleton<ISmtpClient, TestSmtpClient>();
            services.AddSingleton<IEmailSender, SmtpEmailSender>();
            
            // 🎯 アプリケーションサービス
            services.AddScoped<IPasswordResetService, PasswordResetService>();
            
            // 📊 ロギング
            services.AddLogging(builder =>
            {
                builder.AddConsole();
                builder.SetMinimumLevel(LogLevel.Debug);
            });
            
            // SMTP設定（テスト用）
            services.Configure<SmtpSettings>(options =>
            {
                options.Host = "localhost";
                options.Port = 1025;
                options.EnableSsl = false;
                options.SenderEmail = "test@example.com";
                options.SenderName = "Test System";
            });
            
            ServiceProvider = services.BuildServiceProvider();
            
            // データベース初期化
            using var scope = ServiceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<UbiquitousLanguageDbContext>();
            context.Database.EnsureCreated();
        }
        
        public void Dispose()
        {
            // 🧹 テスト後のクリーンアップ
            using var scope = ServiceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<UbiquitousLanguageDbContext>();
            context.Database.EnsureDeleted();
            
            if (ServiceProvider is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }
    }
    
    /// <summary>
    /// テスト用のSMTPクライアント実装
    /// 実際にはメールを送信せず、メモリに保存
    /// </summary>
    public class TestSmtpClient : ISmtpClient
    {
        public bool IsConnected { get; private set; }
        
        public Task ConnectAsync(string host, int port, bool useSsl, System.Threading.CancellationToken cancellationToken = default)
        {
            IsConnected = true;
            return Task.CompletedTask;
        }
        
        public Task AuthenticateAsync(string userName, string password, System.Threading.CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }
        
        public Task DisconnectAsync(bool quit, System.Threading.CancellationToken cancellationToken = default)
        {
            IsConnected = false;
            return Task.CompletedTask;
        }
        
        public Task SendAsync(MimeKit.MimeMessage message, System.Threading.CancellationToken cancellationToken = default, MailKit.ITransferProgress? progress = null)
        {
            // テスト用: 実際には送信しない
            return Task.CompletedTask;
        }
        
        public void Dispose()
        {
            IsConnected = false;
        }
    }
}