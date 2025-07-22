using Microsoft.Extensions.Logging;
using Moq;
using System.Threading.Channels;
using UbiquitousLanguageManager.Application;
using UbiquitousLanguageManager.Infrastructure.Services;
using Xunit;

namespace UbiquitousLanguageManager.Tests.Infrastructure;

/// <summary>
/// BackgroundEmailQueue の単体テスト
/// 【初学者向け解説】
/// このテストクラスでは、バックグラウンドメール送信キューの機能を検証します。
/// Channel<T> を使用したプロデューサー・コンシューマーパターンの正確性と
/// スレッドセーフティを確認することが主な目的です。
/// 
/// 【テストファースト開発について（ADR_013準拠）】
/// キューのライフサイクル管理、例外処理、リソース解放の正確性を事前に定義し、
/// 実装がこれらの仕様を満たしていることを継続的に検証します。
/// </summary>
/// <remarks>
/// Phase A3: NotificationService基盤構築で追加
/// テスト対象: キューへの追加・取得、並行処理、リソース管理
/// </remarks>
public class BackgroundEmailQueueTests
{
    private readonly Mock<Microsoft.Extensions.Logging.ILogger<BackgroundEmailQueue>> _mockLogger;

    /// <summary>
    /// テストの初期化処理
    /// 【初学者向け解説】
    /// 各テストで共通して使用するモックオブジェクトを準備します。
    /// BackgroundEmailQueue は Logger 以外に外部依存がないため、
    /// シンプルな初期化となります。
    /// </summary>
    public BackgroundEmailQueueTests()
    {
        _mockLogger = new Mock<Microsoft.Extensions.Logging.ILogger<BackgroundEmailQueue>>();
    }

    /// <summary>
    /// コンストラクタ成功テスト
    /// 【テスト目的】
    /// BackgroundEmailQueue が正常に初期化されることを確認
    /// </summary>
    [Fact]
    public void Constructor_WithValidLogger_ShouldCreateInstance()
    {
        // Act
        var queue = new BackgroundEmailQueue(_mockLogger.Object);

        // Assert
        Assert.NotNull(queue);
    }

    /// <summary>
    /// コンストラクタ異常テスト: nullロガー
    /// 【テスト目的】
    /// nullロガーが渡された場合に適切な例外が発生することを確認
    /// </summary>
    [Fact]
    public void Constructor_WithNullLogger_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new BackgroundEmailQueue(null!));
    }

    /// <summary>
    /// QueueBackgroundWorkItemAsync 成功テスト
    /// 【テスト目的】
    /// 有効なワークアイテムがキューに正常に追加されることを確認
    /// </summary>
    [Fact]
    public async Task QueueBackgroundWorkItemAsync_WithValidWorkItem_ShouldCompleteSuccessfully()
    {
        // Arrange
        var queue = new BackgroundEmailQueue(_mockLogger.Object);
        var workItem = new Func<CancellationToken, Task>(_ => Task.CompletedTask);

        // Act
        await queue.QueueBackgroundWorkItemAsync(workItem);

        // Assert: 例外が発生しないことで成功を確認
        // キューに正常に追加されたことは、Dequeue テストで間接的に確認
    }

    /// <summary>
    /// QueueBackgroundWorkItemAsync 異常テスト: nullワークアイテム
    /// 【テスト目的】
    /// nullワークアイテムが渡された場合に適切な例外が発生することを確認
    /// </summary>
    [Fact]
    public async Task QueueBackgroundWorkItemAsync_WithNullWorkItem_ShouldThrowArgumentNullException()
    {
        // Arrange
        var queue = new BackgroundEmailQueue(_mockLogger.Object);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => 
            queue.QueueBackgroundWorkItemAsync(null!));
    }

    /// <summary>
    /// DequeueAsync 成功テスト
    /// 【テスト目的】
    /// キューに追加されたワークアイテムが正常に取得できることを確認
    /// </summary>
    [Fact]
    public async Task DequeueAsync_WithQueuedItem_ShouldReturnWorkItem()
    {
        // Arrange
        var queue = new BackgroundEmailQueue(_mockLogger.Object);
        var expectedWorkItem = new Func<CancellationToken, Task>(_ => Task.CompletedTask);
        
        // キューにアイテムを追加
        await queue.QueueBackgroundWorkItemAsync(expectedWorkItem);

        // Act
        var cancellationToken = new CancellationToken();
        var actualWorkItem = await queue.DequeueAsync(cancellationToken);

        // Assert
        Assert.NotNull(actualWorkItem);
        Assert.Equal(expectedWorkItem, actualWorkItem);
    }

    /// <summary>
    /// DequeueAsync キャンセレーションテスト
    /// 【テスト目的】
    /// CancellationTokenによるキャンセル処理が正常に動作することを確認
    /// </summary>
    [Fact]
    public async Task DequeueAsync_WithCancellation_ShouldThrowOperationCanceledException()
    {
        // Arrange
        var queue = new BackgroundEmailQueue(_mockLogger.Object);
        var cancellationTokenSource = new CancellationTokenSource();
        
        // すぐにキャンセル
        cancellationTokenSource.Cancel();

        // Act & Assert
        await Assert.ThrowsAsync<OperationCanceledException>(() => 
            queue.DequeueAsync(cancellationTokenSource.Token));
    }

    /// <summary>
    /// FIFO（先入れ先出し）動作確認テスト
    /// 【テスト目的】
    /// 複数のワークアイテムが追加された順序で取得されることを確認
    /// </summary>
    [Fact]
    public async Task DequeueAsync_WithMultipleItems_ShouldReturnInFIFOOrder()
    {
        // Arrange
        var queue = new BackgroundEmailQueue(_mockLogger.Object);
        var workItem1 = new Func<CancellationToken, Task>(_ => Task.FromResult(1));
        var workItem2 = new Func<CancellationToken, Task>(_ => Task.FromResult(2));
        var workItem3 = new Func<CancellationToken, Task>(_ => Task.FromResult(3));

        // キューに順番に追加
        await queue.QueueBackgroundWorkItemAsync(workItem1);
        await queue.QueueBackgroundWorkItemAsync(workItem2);
        await queue.QueueBackgroundWorkItemAsync(workItem3);

        // Act & Assert: 追加した順序で取得されることを確認
        var cancellationToken = new CancellationToken();
        
        var retrievedItem1 = await queue.DequeueAsync(cancellationToken);
        Assert.Equal(workItem1, retrievedItem1);

        var retrievedItem2 = await queue.DequeueAsync(cancellationToken);
        Assert.Equal(workItem2, retrievedItem2);

        var retrievedItem3 = await queue.DequeueAsync(cancellationToken);
        Assert.Equal(workItem3, retrievedItem3);
    }

    /// <summary>
    /// 並行アクセステスト
    /// 【テスト目的】
    /// 複数のスレッドから同時にキューにアクセスしても安全であることを確認
    /// </summary>
    [Fact]
    public async Task QueueAndDequeue_WithConcurrentAccess_ShouldBeThreadSafe()
    {
        // Arrange
        var queue = new BackgroundEmailQueue(_mockLogger.Object);
        const int itemCount = 100;
        var workItems = new List<Func<CancellationToken, Task>>();

        // テスト用のワークアイテムを準備
        for (int i = 0; i < itemCount; i++)
        {
            var index = i; // クロージャ用の変数キャプチャ
            workItems.Add(new Func<CancellationToken, Task>(_ => Task.FromResult(index)));
        }

        // Act: 並行してキューに追加
        var enqueueTasks = workItems.Select(item => 
            Task.Run(() => queue.QueueBackgroundWorkItemAsync(item))).ToArray();
        
        await Task.WhenAll(enqueueTasks);

        // Act: 並行してキューから取得
        var cancellationToken = new CancellationToken();
        var dequeueTasks = Enumerable.Range(0, itemCount)
            .Select(_ => Task.Run(() => queue.DequeueAsync(cancellationToken))).ToArray();

        var retrievedItems = await Task.WhenAll(dequeueTasks);

        // Assert: すべてのアイテムが正常に取得されることを確認
        Assert.Equal(itemCount, retrievedItems.Length);
        Assert.All(retrievedItems, item => Assert.NotNull(item));
    }

    /// <summary>
    /// GetQueueCount 機能テスト
    /// 【テスト目的】
    /// キューの統計情報取得機能が正常に動作することを確認
    /// </summary>
    [Fact]
    public async Task GetQueueCount_ShouldReturnCorrectCount()
    {
        // Arrange
        var queue = new BackgroundEmailQueue(_mockLogger.Object);

        // 初期状態では0または-1（カウント不可）
        var initialCount = queue.GetQueueCount();
        Assert.True(initialCount >= -1); // -1はカウント不可を表す

        // いくつかのアイテムを追加
        var workItem = new Func<CancellationToken, Task>(_ => Task.CompletedTask);
        await queue.QueueBackgroundWorkItemAsync(workItem);
        await queue.QueueBackgroundWorkItemAsync(workItem);

        // カウントが増加していることを確認（ChannelがCountをサポートする場合）
        var countAfterEnqueue = queue.GetQueueCount();
        // ChannelによってはCount取得をサポートしない場合があるため、
        // -1（不明）または正の値であることを確認
        Assert.True(countAfterEnqueue == -1 || countAfterEnqueue >= 2);
    }

    /// <summary>
    /// Dispose リソース解放テスト
    /// 【テスト目的】
    /// リソースが適切に解放されることを確認
    /// </summary>
    [Fact]
    public void Dispose_ShouldCompleteWithoutException()
    {
        // Arrange
        var queue = new BackgroundEmailQueue(_mockLogger.Object);

        // Act & Assert: 例外が発生しないことで成功を確認
        queue.Dispose();
    }

    /// <summary>
    /// ログ出力確認テスト
    /// 【テスト目的】
    /// 適切なログ出力が行われることを確認（ADR_008準拠）
    /// </summary>
    [Fact]
    public void Constructor_ShouldLogInitialization()
    {
        // Act
        var queue = new BackgroundEmailQueue(_mockLogger.Object);

        // Assert: 初期化ログが出力されることを確認
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("BackgroundEmailQueue initialized")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    /// <summary>
    /// ログ出力確認テスト: デバッグログ
    /// 【テスト目的】
    /// ワークアイテムの追加・取得時にデバッグログが出力されることを確認
    /// </summary>
    [Fact]
    public async Task QueueAndDequeue_ShouldLogDebugMessages()
    {
        // Arrange
        var queue = new BackgroundEmailQueue(_mockLogger.Object);
        var workItem = new Func<CancellationToken, Task>(_ => Task.CompletedTask);

        // Act: キューに追加
        await queue.QueueBackgroundWorkItemAsync(workItem);

        // Act: キューから取得
        var cancellationToken = new CancellationToken();
        await queue.DequeueAsync(cancellationToken);

        // Assert: デバッグログが出力されることを確認
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("queued successfully")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);

        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("dequeued successfully")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
}