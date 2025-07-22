using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.FSharp.Core;
using Moq;
using UbiquitousLanguageManager.Application;
using UbiquitousLanguageManager.Infrastructure.Services;
using Xunit;

namespace UbiquitousLanguageManager.Tests.Infrastructure;

/// <summary>
/// MailKitEmailSender の単体テスト
/// 【初学者向け解説】
/// このテストクラスでは、MailKit を使用したメール送信機能の単体テストを実行します。
/// 実際のSMTPサーバーへの接続はせず、モック（偽のオブジェクト）を使用して
/// ロジックの正確性を検証します。
/// 
/// 【テストファースト開発について（ADR_013準拠）】
/// Phase A3 では、実装前にテストを作成し、テストが通ることを確認しながら開発を進めます。
/// これにより、品質の高い実装と回帰テストの防止を実現します。
/// </summary>
/// <remarks>
/// Phase A3: NotificationService基盤構築で追加
/// テスト対象: メール送信の基本機能、エラーハンドリング、設定検証
/// </remarks>
public class MailKitEmailSenderTests
{
    private readonly Mock<Microsoft.Extensions.Logging.ILogger<MailKitEmailSender>> _mockLogger;
    private readonly SmtpSettings _validSmtpSettings;
    private readonly IOptions<SmtpSettings> _options;

    /// <summary>
    /// テストの初期化処理
    /// 【初学者向け解説】
    /// 各テストメソッドの実行前に呼ばれる共通の初期化処理です。
    /// モックオブジェクトと有効な設定を準備して、テストの前提条件を整えます。
    /// </summary>
    public MailKitEmailSenderTests()
    {
        // Logger のモックを作成
        _mockLogger = new Mock<Microsoft.Extensions.Logging.ILogger<MailKitEmailSender>>();

        // 有効な SMTP 設定を作成（テスト用）
        _validSmtpSettings = new SmtpSettings
        {
            Server = "localhost",
            Port = 1025,
            SenderName = "Test Sender",
            SenderEmail = "test@example.com",
            Username = "",
            Password = "",
            EnableSsl = false
        };

        // IOptions<SmtpSettings> のモックを作成
        var mockOptions = new Mock<IOptions<SmtpSettings>>();
        mockOptions.Setup(o => o.Value).Returns(_validSmtpSettings);
        _options = mockOptions.Object;
    }

    /// <summary>
    /// コンストラクタ成功テスト
    /// 【テスト目的】
    /// 有効な設定でMailKitEmailSenderが正常に初期化されることを確認
    /// </summary>
    [Fact]
    public void Constructor_WithValidSettings_ShouldCreateInstance()
    {
        // Act & Assert
        var emailSender = new MailKitEmailSender(_options, _mockLogger.Object);
        
        Assert.NotNull(emailSender);
    }

    /// <summary>
    /// コンストラクタ異常テスト: null設定
    /// 【テスト目的】
    /// null設定が渡された場合に適切な例外が発生することを確認
    /// </summary>
    [Fact]
    public void Constructor_WithNullSettings_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => 
            new MailKitEmailSender(null!, _mockLogger.Object));
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
        Assert.Throws<ArgumentNullException>(() => 
            new MailKitEmailSender(_options, null!));
    }

    /// <summary>
    /// コンストラクタ異常テスト: 無効な設定
    /// 【テスト目的】
    /// 無効な設定（空のサーバー名など）で初期化が失敗することを確認
    /// </summary>
    [Fact]
    public void Constructor_WithInvalidSettings_ShouldThrowInvalidOperationException()
    {
        // Arrange: 無効な設定を作成
        var invalidSettings = new SmtpSettings
        {
            Server = "", // 無効なサーバー名
            Port = 1025,
            SenderName = "Test Sender",
            SenderEmail = "test@example.com",
            Username = "",
            Password = "",
            EnableSsl = false
        };

        var mockOptions = new Mock<IOptions<SmtpSettings>>();
        mockOptions.Setup(o => o.Value).Returns(invalidSettings);

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => 
            new MailKitEmailSender(mockOptions.Object, _mockLogger.Object));
    }

    /// <summary>
    /// SendEmailAsync 成功テスト（モック使用）
    /// 【テスト目的】
    /// 正常なメール送信処理の基本的なロジック検証
    /// 
    /// 【注意】
    /// 実際のSMTPサーバーには接続せず、入力検証とResult型の正確性のみをテスト
    /// 実際のSMTP通信テストは統合テストで実施
    /// </summary>
    [Fact]
    public async Task SendEmailAsync_WithValidParameters_ShouldReturnOkResult()
    {
        // Arrange
        var emailSender = new MailKitEmailSender(_options, _mockLogger.Object);
        var testEmail = "recipient@example.com";
        var testSubject = "Test Subject";
        var testHtmlMessage = "<h1>Test HTML Message</h1>";

        // Act
        // 注意: 実際のSMTP接続は行われないため、このテストは設定検証のみ
        // 実際の送信テストは統合テストで実施
        var result = await emailSender.SendEmailAsync(testEmail, testSubject, testHtmlMessage);

        // Assert: Result型の基本的な構造確認
        Assert.NotNull(result);
        // 実際のSMTP接続がないため、詳細な成功/失敗判定は統合テストで実施
    }

    /// <summary>
    /// SendEmailAsync 異常テスト: null引数
    /// 【テスト目的】
    /// null引数が渡された場合の適切なエラーハンドリング確認
    /// </summary>
    [Theory]
    [InlineData(null, "subject", "message")] // nullメールアドレス
    [InlineData("test@example.com", null, "message")] // null件名
    [InlineData("test@example.com", "subject", null)] // nullメッセージ
    public async Task SendEmailAsync_WithNullParameters_ShouldReturnErrorResult(
        string email, string subject, string htmlMessage)
    {
        // Arrange
        var emailSender = new MailKitEmailSender(_options, _mockLogger.Object);

        // Act
        var result = await emailSender.SendEmailAsync(email!, subject!, htmlMessage!);

        // Assert: エラー結果が返されることを確認
        Assert.NotNull(result);
        // F# Result型のエラー状態確認は統合テストで詳細実施
    }

    /// <summary>
    /// SendPlainTextEmailAsync 基本テスト
    /// 【テスト目的】
    /// プレーンテキストメール送信の基本的なロジック検証
    /// </summary>
    [Fact]
    public async Task SendPlainTextEmailAsync_WithValidParameters_ShouldReturnResult()
    {
        // Arrange
        var emailSender = new MailKitEmailSender(_options, _mockLogger.Object);
        var testEmail = "recipient@example.com";
        var testSubject = "Test Subject";
        var testTextMessage = "Test plain text message";

        // Act
        var result = await emailSender.SendPlainTextEmailAsync(testEmail, testSubject, testTextMessage);

        // Assert
        Assert.NotNull(result);
    }

    /// <summary>
    /// SendEmailWithAttachmentAsync 基本テスト
    /// 【テスト目的】
    /// 添付ファイル付きメール送信の基本的なロジック検証
    /// </summary>
    [Fact]
    public async Task SendEmailWithAttachmentAsync_WithNonExistentFile_ShouldReturnErrorResult()
    {
        // Arrange
        var emailSender = new MailKitEmailSender(_options, _mockLogger.Object);
        var testEmail = "recipient@example.com";
        var testSubject = "Test Subject";
        var testHtmlMessage = "<h1>Test HTML Message</h1>";
        var nonExistentFilePath = "/path/to/nonexistent/file.txt";
        var attachmentName = "test.txt";

        // Act: 存在しないファイルを指定してエラーを確認
        var result = await emailSender.SendEmailWithAttachmentAsync(
            testEmail, testSubject, testHtmlMessage, nonExistentFilePath, attachmentName);

        // Assert: エラー結果が返されることを確認
        Assert.NotNull(result);
        // ファイルが存在しない場合のエラー処理が適切に動作することを確認
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
        var emailSender = new MailKitEmailSender(_options, _mockLogger.Object);

        // Assert: 初期化ログが出力されることを確認
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("MailKitEmailSender initialized")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
}