using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using UbiquitousLanguageManager.Application;

namespace UbiquitousLanguageManager.Infrastructure.Services;

/// <summary>
/// バックグラウンドでメール送信を実行するホステッドサービス
/// 【初学者向け解説】
/// BackgroundService を継承することで、アプリケーションの開始と共に
/// バックグラウンドスレッドで継続的にメール送信キューを監視し、
/// キューに追加されたメール送信要求を順次処理します。
/// 
/// 【ホステッドサービスについて】
/// ASP.NET Core の IHostedService インターフェースを実装したサービスで、
/// アプリケーションのライフサイクルと連動して自動的に開始・停止されます
/// </summary>
/// <remarks>
/// Phase A3: NotificationService基盤構築で導入
/// アプリケーションの応答性向上のため、重いメール送信処理をバックグラウンドで実行
/// </remarks>
public class EmailSendingBackgroundService : BackgroundService
{
    private readonly IBackgroundEmailQueue _taskQueue;
    private readonly IServiceProvider _serviceProvider;
    private readonly Microsoft.Extensions.Logging.ILogger<EmailSendingBackgroundService> _logger;

    /// <summary>
    /// コンストラクタ: 依存サービスの注入
    /// 【初学者向け解説】
    /// IServiceProvider: DIコンテナからスコープ付きサービス（IEmailSender）を取得するために必要
    /// メール送信は長時間実行される可能性があるため、スコープを適切に管理する必要があります
    /// </summary>
    public EmailSendingBackgroundService(
        IBackgroundEmailQueue taskQueue,
        IServiceProvider serviceProvider,
        Microsoft.Extensions.Logging.ILogger<EmailSendingBackgroundService> logger)
    {
        _taskQueue = taskQueue ?? throw new ArgumentNullException(nameof(taskQueue));
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// バックグラウンドサービスのメイン実行ループ
    /// 【初学者向け解説】
    /// アプリケーションが実行中の間、このメソッドが継続的に実行されます
    /// キューからタスクを取得し、スコープ内でメール送信サービスを使用してメールを送信
    /// </summary>
    /// <param name="stoppingToken">アプリケーション終了時のキャンセルトークン</param>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("EmailSendingBackgroundService started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                // キューからメール送信タスクを取得（タスクが追加されるまで待機）
                var workItem = await _taskQueue.DequeueAsync(stoppingToken);

                if (workItem != null)
                {
                    // スコープを作成してメール送信を実行
                    await ExecuteEmailSendingTaskAsync(workItem, stoppingToken);
                }
            }
            catch (OperationCanceledException)
            {
                // アプリケーション終了による正常なキャンセル
                _logger.LogInformation("EmailSendingBackgroundService is stopping due to cancellation");
                break;
            }
            catch (Exception ex)
            {
                // 予期しないエラーが発生した場合でもサービスは継続
                _logger.LogError(ex, "Unhandled error occurred in EmailSendingBackgroundService");
                
                // 短時間待機してからループを続行（エラー連発を防ぐ）
                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            }
        }

        _logger.LogInformation("EmailSendingBackgroundService stopped");
    }

    /// <summary>
    /// 個別メール送信タスクの実行
    /// 【初学者向け解説】
    /// スコープを作成してスコープ付きサービス（IEmailSender）を取得し、
    /// 実際のメール送信処理を実行します。スコープにより、
    /// EF Core の DbContext などが適切にライフサイクル管理されます
    /// </summary>
    /// <param name="workItem">実行するメール送信タスク</param>
    /// <param name="cancellationToken">キャンセルトークン</param>
    private async Task ExecuteEmailSendingTaskAsync(
        System.Func<CancellationToken, Task> workItem,
        CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        
        try
        {
            _logger.LogDebug("Executing email sending task");
            
            // スコープ内でメール送信タスクを実行
            await workItem(cancellationToken);
            
            _logger.LogDebug("Email sending task completed successfully");
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Email sending task was cancelled");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Email sending task failed");
        }
    }

    /// <summary>
    /// サービス停止時の処理
    /// 【初学者向け解説】
    /// アプリケーション終了時に呼ばれるメソッドです
    /// 実行中のタスクがある場合は、適切に完了を待機します
    /// </summary>
    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("EmailSendingBackgroundService is stopping");
        
        // 基底クラスのStopAsyncを呼び出して正常に終了処理を実行
        await base.StopAsync(cancellationToken);
        
        _logger.LogInformation("EmailSendingBackgroundService has stopped");
    }

    /// <summary>
    /// リソースの解放
    /// </summary>
    public override void Dispose()
    {
        _logger.LogInformation("EmailSendingBackgroundService disposed");
        base.Dispose();
    }
}