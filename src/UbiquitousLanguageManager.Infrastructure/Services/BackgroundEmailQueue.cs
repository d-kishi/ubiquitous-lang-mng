using System;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using UbiquitousLanguageManager.Application;

namespace UbiquitousLanguageManager.Infrastructure.Services;

/// <summary>
/// バックグラウンドメール送信キューの実装
/// 【初学者向け解説】
/// メール送信は外部SMTP サーバーへの通信が必要で時間がかかるため、
/// HTTPリクエストの応答時間に影響しないよう、バックグラウンドで非同期処理を行います。
/// 
/// 【Channel&lt;T&gt; について】
/// .NET のプロデューサー・コンシューマーパターン実装のためのスレッドセーフなキュー
/// メモリ上でのみ動作するため、アプリケーション再起動時にキュー内容は失われます
/// 高度な要求がある場合は RabbitMQ や Azure Service Bus などの永続キューの使用も検討
/// </summary>
/// <remarks>
/// Phase A3: NotificationService基盤構築で導入
/// Clean Architecture の依存関係逆転により、Application層のインターフェースを実装
/// </remarks>
public class BackgroundEmailQueue : IBackgroundEmailQueue
{
    private readonly Channel<System.Func<System.Threading.CancellationToken, Task>> _queue;
    private readonly Microsoft.Extensions.Logging.ILogger<BackgroundEmailQueue> _logger;

    /// <summary>
    /// コンストラクタ: キューの初期化
    /// 【初学者向け解説】
    /// Channel.CreateUnbounded(): 無制限キューを作成（メモリが許す限りタスクを蓄積可能）
    /// 実運用では CreateBounded() でキュー上限を設定することも検討
    /// </summary>
    public BackgroundEmailQueue(Microsoft.Extensions.Logging.ILogger<BackgroundEmailQueue> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        // 無制限キューを作成
        var options = new BoundedChannelOptions(capacity: 1000) // 最大1000個のタスクをキュー
        {
            FullMode = BoundedChannelFullMode.Wait, // キューが満杯の場合は待機
            SingleReader = true, // 1つのBackgroundServiceでのみ読み取り
            SingleWriter = false // 複数のスレッドから書き込み可能
        };

        _queue = Channel.CreateBounded<System.Func<System.Threading.CancellationToken, Task>>(options);
        
        _logger.LogInformation("BackgroundEmailQueue initialized with capacity: {Capacity}", 1000);
    }

    /// <summary>
    /// メール送信タスクをキューに追加
    /// 【初学者向け解説】
    /// workItem: 実際のメール送信処理を表すデリゲート（関数オブジェクト）
    /// 呼び出し元のスレッドは即座に制御を返すため、HTTPレスポンスが速くなります
    /// </summary>
    /// <param name="workItem">実行するメール送信処理</param>
    /// <returns>キューへの追加完了を表すTask</returns>
    public async Task QueueBackgroundWorkItemAsync(System.Func<System.Threading.CancellationToken, Task> workItem)
    {
        if (workItem == null)
        {
            throw new ArgumentNullException(nameof(workItem));
        }

        try
        {
            await _queue.Writer.WriteAsync(workItem);
            _logger.LogDebug("Email work item queued successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to queue email work item");
            throw;
        }
    }

    /// <summary>
    /// キューからメール送信タスクを取得
    /// 【初学者向け解説】
    /// BackgroundService がこのメソッドを継続的に呼び出してタスクを処理します
    /// WaitToReadAsync(): タスクがキューに追加されるまで非同期待機
    /// </summary>
    /// <param name="cancellationToken">キャンセル要求を伝播するためのトークン</param>
    /// <returns>実行すべきメール送信処理</returns>
    public async Task<System.Func<System.Threading.CancellationToken, Task>> DequeueAsync(CancellationToken cancellationToken)
    {
        // キューにタスクが追加されるまで待機
        await _queue.Reader.WaitToReadAsync(cancellationToken);

        // タスクを取得して返す
        _queue.Reader.TryRead(out var workItem);
        
        if (workItem != null)
        {
            _logger.LogDebug("Email work item dequeued successfully");
        }
        
        return workItem ?? throw new InvalidOperationException("Unable to dequeue work item");
    }

    /// <summary>
    /// キューの統計情報を取得（監視・デバッグ用）
    /// </summary>
    /// <returns>キュー内の未処理タスク数</returns>
    public int GetQueueCount()
    {
        // ChannelReader には直接的な Count プロパティがないため、
        // 実装では概算値を返すか、独自の카ウン터를 維持する必要があります
        // ここでは簡単のため、利用可能な場合のみカウントを返します
        var count = _queue.Reader.CanCount ? _queue.Reader.Count : -1;
        return count;
    }

    /// <summary>
    /// リソースの適切な解放
    /// 【初学者向解説】
    /// IDisposable パターンに従い、使用済みリソースを適切に解放します
    /// Channel の Writer を完了状態にして、BackgroundService に終了を通知
    /// </summary>
    public void Dispose()
    {
        try
        {
            // Writer を完了状態にして新しいタスクの追加を停止
            _queue.Writer.Complete();
            _logger.LogInformation("BackgroundEmailQueue disposed");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while disposing BackgroundEmailQueue");
        }
    }
}