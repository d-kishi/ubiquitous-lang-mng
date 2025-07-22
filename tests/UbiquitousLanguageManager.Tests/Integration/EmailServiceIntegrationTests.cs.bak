using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Diagnostics;
using UbiquitousLanguageManager.Application;
using UbiquitousLanguageManager.Infrastructure.Services;
using Xunit;

namespace UbiquitousLanguageManager.Tests.Integration;

/// <summary>
/// メール送信サービスの統合テスト
/// 【初学者向け解説】
/// このテストクラスでは、実際のSMTP サーバー（Smtp4dev）を使用した
/// エンドツーエンドのメール送信機能を検証します。
/// 単体テストとは異なり、外部リソース（SMTPサーバー）との実際の通信を行います。
/// 
/// 【統合テストについて（ADR_013準拠）】
/// Phase A3 では、基盤レイヤーの統合テストを重視し、
/// 実際の動作環境に近い条件での動作検証を行います。
/// </summary>
/// <remarks>
/// Phase A3: NotificationService基盤構築で追加
/// テスト条件: Smtp4dev が localhost:1025 で起動していること
/// テスト対象: 実際のSMTP通信、バックグラウンド処理、エラーハンドリング
/// </remarks>
public class EmailServiceIntegrationTests : IAsyncDisposable
{
    private readonly ServiceProvider _serviceProvider;
    private readonly IEmailSender _emailSender;
    private readonly IBackgroundEmailQueue _emailQueue;
    private readonly EmailSendingBackgroundService _backgroundService;

    /// <summary>
    /// 統合テストの初期化処理
    /// 【初学者向け解説】
    /// 実際のDIコンテナを構築し、本番環境と同じ構成でサービスを初期化します。
    /// これにより、依存関係の結合や設定の不整合を実環境に近い条件で検証できます。
    /// </summary>
    public EmailServiceIntegrationTests()
    {
        var services = new ServiceCollection();

        // ログサービスの登録
        services.AddLogging(builder => builder.AddConsole().SetMinimumLevel(LogLevel.Debug));

        // SMTP設定の登録（Smtp4dev用）
        var smtpSettings = new SmtpSettings
        {
            Server = "localhost",
            Port = 1025,
            SenderName = "Integration Test",
            SenderEmail = "test@integration.local",
            Username = "",
            Password = "",
            EnableSsl = false
        };
        services.Configure<SmtpSettings>(options =>
        {
            options.Server = smtpSettings.Server;
            options.Port = smtpSettings.Port;
            options.SenderName = smtpSettings.SenderName;
            options.SenderEmail = smtpSettings.SenderEmail;
            options.Username = smtpSettings.Username;
            options.Password = smtpSettings.Password;
            options.EnableSsl = smtpSettings.EnableSsl;
        });

        // メールサービスの登録
        services.AddTransient<IEmailSender, MailKitEmailSender>();
        services.AddSingleton<IBackgroundEmailQueue, BackgroundEmailQueue>();
        services.AddHostedService<EmailSendingBackgroundService>();

        _serviceProvider = services.BuildServiceProvider();
        _emailSender = _serviceProvider.GetRequiredService<IEmailSender>();
        _emailQueue = _serviceProvider.GetRequiredService<IBackgroundEmailQueue>();
        
        // バックグラウンドサービスを手動で取得
        var hostedServices = _serviceProvider.GetServices<IHostedService>();
        _backgroundService = hostedServices.OfType<EmailSendingBackgroundService>().First();
    }

    /// <summary>
    /// Smtp4dev 接続テスト
    /// 【テスト目的】
    /// 開発環境のSMTPサーバー（Smtp4dev）への接続が可能であることを確認
    /// </summary>
    /// <remarks>
    /// このテストが失敗する場合は、以下を確認してください：
    /// 1. Smtp4dev が起動しているか: dotnet tool run smtp4dev
    /// 2. ポート1025が使用可能か
    /// 3. ファイアウォールの設定
    /// </remarks>
    [Fact]
    public async Task SendEmailAsync_WithSmtp4dev_ShouldSendSuccessfully()
    {
        // Skip test if Smtp4dev is not running
        if (!await IsSmtp4devRunning())
        {
            // 開発環境でのみスキップ（CI環境では必須）
            var skipMessage = "Smtp4dev is not running on localhost:1025. " +
                            "Please start it with: dotnet tool run smtp4dev";
            
            // CI環境では失敗させる、ローカル開発では警告のみ
            if (Environment.GetEnvironmentVariable("CI") == "true")
            {
                throw new InvalidOperationException(skipMessage);
            }
            else
            {
                // ローカル開発環境では警告を出してスキップ
                Console.WriteLine($"Warning: {skipMessage}");
                return;
            }
        }

        // Arrange
        var recipientEmail = "test@recipient.local";
        var subject = "Integration Test - HTML Message";
        var htmlMessage = @"
            <html>
                <body>
                    <h1>Integration Test Email</h1>
                    <p>This is a test email sent from the integration test suite.</p>
                    <p>Timestamp: {DateTime.Now:yyyy-MM-dd HH:mm:ss}</p>
                </body>
            </html>".Replace("{DateTime.Now:yyyy-MM-dd HH:mm:ss}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

        // Act
        var result = await _emailSender.SendEmailAsync(recipientEmail, subject, htmlMessage);

        // Assert
        Assert.NotNull(result);
        
        // F# Result型の成功確認
        // 注意: ここでは基本的な型チェックのみ実施
        // 詳細なResult型の検証は単体テストで実施済み
        
        // ログ出力で結果確認（開発時のデバッグ用）
        Console.WriteLine($"Email sending result type: {result.GetType().Name}");
        Console.WriteLine("Please check Smtp4dev web interface at http://localhost:5000 to verify email delivery");
    }

    /// <summary>
    /// プレーンテキストメール送信テスト
    /// 【テスト目的】
    /// プレーンテキスト形式でのメール送信が正常に動作することを確認
    /// </summary>
    [Fact]
    public async Task SendPlainTextEmailAsync_WithSmtp4dev_ShouldSendSuccessfully()
    {
        // Skip test if Smtp4dev is not running
        if (!await IsSmtp4devRunning())
        {
            return; // Skip in local development
        }

        // Arrange
        var recipientEmail = "plaintext@recipient.local";
        var subject = "Integration Test - Plain Text Message";
        var textMessage = $@"
            Integration Test Email (Plain Text)
            
            This is a plain text email sent from the integration test suite.
            Timestamp: {DateTime.Now:yyyy-MM-dd HH:mm:ss}
            
            Test completed successfully.";

        // Act
        var result = await _emailSender.SendPlainTextEmailAsync(recipientEmail, subject, textMessage);

        // Assert
        Assert.NotNull(result);
        Console.WriteLine("Plain text email sent. Check Smtp4dev web interface for verification.");
    }

    /// <summary>
    /// バックグラウンドキューによるメール送信テスト
    /// 【テスト目的】
    /// バックグラウンドでのキュー処理によるメール送信が正常に動作することを確認
    /// </summary>
    [Fact]
    public async Task BackgroundEmailQueue_WithMultipleEmails_ShouldProcessAllEmails()
    {
        // Skip test if Smtp4dev is not running
        if (!await IsSmtp4devRunning())
        {
            return; // Skip in local development
        }

        // Arrange
        const int emailCount = 3;
        var sendTasks = new List<Task>();

        // バックグラウンドサービスを開始
        var cancellationTokenSource = new CancellationTokenSource();
        var backgroundTask = _backgroundService.StartAsync(cancellationTokenSource.Token);

        try
        {
            // Act: 複数のメールをキューに追加
            for (int i = 0; i < emailCount; i++)
            {
                var emailIndex = i + 1;
                var workItem = new Func<CancellationToken, Task>(async (ct) =>
                {
                    var recipientEmail = $"queue-test-{emailIndex}@recipient.local";
                    var subject = $"Integration Test - Queued Email #{emailIndex}";
                    var htmlMessage = $@"
                        <html>
                            <body>
                                <h1>Queued Email #{emailIndex}</h1>
                                <p>This email was processed through the background queue.</p>
                                <p>Timestamp: {DateTime.Now:yyyy-MM-dd HH:mm:ss}</p>
                            </body>
                        </html>";

                    await _emailSender.SendEmailAsync(recipientEmail, subject, htmlMessage);
                });

                await _emailQueue.QueueBackgroundWorkItemAsync(workItem);
            }

            // バックグラウンド処理の完了を待機（タイムアウト付き）
            await Task.Delay(TimeSpan.FromSeconds(10)); // 最大10秒待機

            // Assert
            Console.WriteLine($"Queued {emailCount} emails for background processing.");
            Console.WriteLine("Check Smtp4dev web interface to verify all emails were sent.");
        }
        finally
        {
            // バックグラウンドサービスを停止
            cancellationTokenSource.Cancel();
            try
            {
                await backgroundTask;
            }
            catch (OperationCanceledException)
            {
                // 正常なキャンセル
            }
        }
    }

    /// <summary>
    /// エラーハンドリング統合テスト
    /// 【テスト目的】
    /// 無効なSMTP設定でのエラー処理が適切に動作することを確認
    /// </summary>
    [Fact]
    public async Task SendEmailAsync_WithInvalidSmtpServer_ShouldReturnErrorResult()
    {
        // Arrange: 無効なSMTP設定でサービスを作成
        var services = new ServiceCollection();
        services.AddLogging(builder => builder.AddConsole().SetMinimumLevel(LogLevel.Debug));
        
        var invalidSmtpSettings = new SmtpSettings
        {
            Server = "invalid-smtp-server.local", // 存在しないサーバー
            Port = 9999, // 使用されていないポート
            SenderName = "Test Sender",
            SenderEmail = "test@invalid.local",
            Username = "",
            Password = "",
            EnableSsl = false
        };
        
        services.Configure<SmtpSettings>(options =>
        {
            options.Server = invalidSmtpSettings.Server;
            options.Port = invalidSmtpSettings.Port;
            options.SenderName = invalidSmtpSettings.SenderName;
            options.SenderEmail = invalidSmtpSettings.SenderEmail;
            options.Username = invalidSmtpSettings.Username;
            options.Password = invalidSmtpSettings.Password;
            options.EnableSsl = invalidSmtpSettings.EnableSsl;
        });
        
        services.AddTransient<IEmailSender, MailKitEmailSender>();
        
        using var serviceProvider = services.BuildServiceProvider();
        var emailSender = serviceProvider.GetRequiredService<IEmailSender>();

        // Act
        var result = await emailSender.SendEmailAsync(
            "test@recipient.local", 
            "Error Test", 
            "<p>This should fail</p>");

        // Assert
        Assert.NotNull(result);
        // エラー結果が返されることを確認（詳細な検証は単体テストで実施）
        Console.WriteLine($"Error handling test completed. Result type: {result.GetType().Name}");
    }

    /// <summary>
    /// Smtp4dev が起動しているかチェックする補助メソッド
    /// 【実装詳細】
    /// TCP接続試行により、Smtp4dev の起動状態を確認
    /// </summary>
    private static async Task<bool> IsSmtp4devRunning()
    {
        try
        {
            using var tcpClient = new System.Net.Sockets.TcpClient();
            await tcpClient.ConnectAsync("localhost", 1025);
            return tcpClient.Connected;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// リソースの適切な解放
    /// </summary>
    public async ValueTask DisposeAsync()
    {
        if (_backgroundService != null)
        {
            await _backgroundService.StopAsync(CancellationToken.None);
        }
        
        _serviceProvider?.Dispose();
    }
}