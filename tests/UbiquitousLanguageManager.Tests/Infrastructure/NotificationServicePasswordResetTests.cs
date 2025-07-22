using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.FSharp.Core;
using Moq;
using Xunit;
using UbiquitousLanguageManager.Application;
using UbiquitousLanguageManager.Domain;
using UbiquitousLanguageManager.Infrastructure.Services;

namespace UbiquitousLanguageManager.Tests.Infrastructure;

/// <summary>
/// NotificationService パスワードリセット機能のテストクラス
/// 
/// 【F#初学者向け解説】
/// Phase A3で実装したパスワードリセット通知機能の単体テスト。
/// Step2で実装したメール送信基盤（IEmailSender）との統合を検証。
/// 
/// 📧 メールテンプレート・送信処理・エラーハンドリングを網羅的にテスト
/// 📊 ADR_008準拠: ログ出力の検証も含む
/// </summary>
public class NotificationServicePasswordResetTests
{
    private readonly Mock<Microsoft.Extensions.Logging.ILogger<NotificationService>> _loggerMock;
    private readonly Mock<IEmailSender> _emailSenderMock;
    private readonly NotificationService _notificationService;

    /// <summary>
    /// テスト初期化
    /// </summary>
    public NotificationServicePasswordResetTests()
    {
        _loggerMock = new Mock<Microsoft.Extensions.Logging.ILogger<NotificationService>>();
        _emailSenderMock = new Mock<IEmailSender>();

        _notificationService = new NotificationService(
            _loggerMock.Object,
            _emailSenderMock.Object);
    }

    #region SendPasswordResetEmailAsync Tests

    /// <summary>
    /// パスワードリセットメール送信 - 正常ケース
    /// </summary>
    [Fact]
    public async Task SendPasswordResetEmailAsync_ValidInput_ShouldSucceed()
    {
        // Arrange
        var email = Email.create("test@example.com").ResultValue;
        var resetToken = "encoded_reset_token_123";
        var resetUrl = "https://localhost:5001/account/reset-password?email=test@example.com&token=" + resetToken;

        _emailSenderMock.Setup(x => x.SendEmailAsync(
                email.Value,
                It.Is<string>(s => s.Contains("パスワードリセットのご案内")),
                It.Is<string>(s => s.Contains(resetUrl) && s.Contains("24時間"))))
            .ReturnsAsync(FSharpResult<Unit, string>.NewOk(null));

        // Act
        var result = await _notificationService.SendPasswordResetEmailAsync(email, resetToken, resetUrl);

        // Assert
        Assert.True(result.IsOk);
        
        // 📧 Step2のメール送信基盤が適切に呼ばれることを検証
        _emailSenderMock.Verify(x => x.SendEmailAsync(
            email.Value,
            It.Is<string>(s => s.Contains("パスワードリセットのご案内")),
            It.Is<string>(s => s.Contains(resetUrl))), Times.Once);
            
        // 📊 ADR_008準拠: 適切なログ出力を検証
        VerifyLogCalled(LogLevel.Information, "Sending password reset email");
        VerifyLogCalled(LogLevel.Information, "Password reset email sent successfully");
    }

    /// <summary>
    /// パスワードリセットメール送信 - メール送信失敗
    /// </summary>
    [Fact]
    public async Task SendPasswordResetEmailAsync_EmailSendFails_ShouldReturnError()
    {
        // Arrange
        var email = Email.create("test@example.com").ResultValue;
        var resetToken = "encoded_reset_token_123";
        var resetUrl = "https://localhost:5001/account/reset-password?email=test@example.com&token=" + resetToken;

        _emailSenderMock.Setup(x => x.SendEmailAsync(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(FSharpResult<Unit, string>.NewError("SMTP server connection timeout"));

        // Act
        var result = await _notificationService.SendPasswordResetEmailAsync(email, resetToken, resetUrl);

        // Assert
        Assert.True(result.IsError);
        Assert.Contains("メール送信に失敗しました", result.ErrorValue);
        Assert.Contains("SMTP server connection timeout", result.ErrorValue);
        
        // 📊 エラーログが出力されることを検証
        VerifyLogCalled(LogLevel.Error, "Failed to send password reset email");
    }

    /// <summary>
    /// パスワードリセットメール送信 - HTMLテンプレート検証
    /// </summary>
    [Fact]
    public async Task SendPasswordResetEmailAsync_ShouldCreateProperHtmlTemplate()
    {
        // Arrange
        var email = Email.create("user@company.com").ResultValue;
        var resetToken = "secure_token_xyz789";
        var resetUrl = "https://production.com/account/reset-password?email=user@company.com&token=" + resetToken;

        string capturedHtmlContent = null;
        _emailSenderMock.Setup(x => x.SendEmailAsync(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .Callback<string, string, string>((emailAddr, subject, htmlContent) => {
                capturedHtmlContent = htmlContent;
            })
            .ReturnsAsync(FSharpResult<Unit, string>.NewOk(null));

        // Act
        var result = await _notificationService.SendPasswordResetEmailAsync(email, resetToken, resetUrl);

        // Assert
        Assert.True(result.IsOk);
        Assert.NotNull(capturedHtmlContent);
        
        // 🎨 HTMLテンプレートの必須要素検証
        Assert.Contains("パスワードリセット", capturedHtmlContent);
        Assert.Contains("user@company.com", capturedHtmlContent);
        Assert.Contains(resetUrl, capturedHtmlContent);
        Assert.Contains("24時間", capturedHtmlContent);
        Assert.Contains("パスワードを再設定する", capturedHtmlContent);
        Assert.Contains("重要な注意事項", capturedHtmlContent);
        Assert.Contains("リンクは一度のみ使用可能", capturedHtmlContent);
        Assert.Contains("ユビキタス言語管理システム", capturedHtmlContent);
        
        // 🔐 セキュリティ注意事項の確認
        Assert.Contains("心当たりがない場合", capturedHtmlContent);
        Assert.Contains("不審なアクセス", capturedHtmlContent);
    }

    /// <summary>
    /// パスワードリセットメール送信 - 例外発生
    /// </summary>
    [Fact]
    public async Task SendPasswordResetEmailAsync_ExceptionThrown_ShouldReturnError()
    {
        // Arrange
        var email = Email.create("test@example.com").ResultValue;
        var resetToken = "token";
        var resetUrl = "https://example.com/reset";

        _emailSenderMock.Setup(x => x.SendEmailAsync(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .ThrowsAsync(new InvalidOperationException("Network connection error"));

        // Act
        var result = await _notificationService.SendPasswordResetEmailAsync(email, resetToken, resetUrl);

        // Assert
        Assert.True(result.IsError);
        Assert.Contains("メール送信中にシステムエラー", result.ErrorValue);
        Assert.Contains("Network connection error", result.ErrorValue);
        
        // 📊 エラーログが出力されることを検証
        VerifyLogCalled(LogLevel.Error, "Unexpected error while sending password reset email");
    }

    #endregion

    #region SendPasswordResetConfirmationAsync Tests

    /// <summary>
    /// パスワードリセット完了通知メール送信 - 正常ケース
    /// </summary>
    [Fact]
    public async Task SendPasswordResetConfirmationAsync_ValidInput_ShouldSucceed()
    {
        // Arrange
        var email = Email.create("test@example.com").ResultValue;

        _emailSenderMock.Setup(x => x.SendEmailAsync(
                email.Value,
                It.Is<string>(s => s.Contains("パスワード変更完了のお知らせ")),
                It.Is<string>(s => s.Contains("パスワードの変更が完了しました") && 
                                  s.Contains("セキュリティに関する重要なお知らせ"))))
            .ReturnsAsync(FSharpResult<Unit, string>.NewOk(null));

        // Act
        var result = await _notificationService.SendPasswordResetConfirmationAsync(email);

        // Assert
        Assert.True(result.IsOk);
        
        // 📧 確認メール送信が適切に呼ばれることを検証
        _emailSenderMock.Verify(x => x.SendEmailAsync(
            email.Value,
            It.Is<string>(s => s.Contains("パスワード変更完了のお知らせ")),
            It.IsAny<string>()), Times.Once);
            
        // 📊 ADR_008準拠: 適切なログ出力を検証
        VerifyLogCalled(LogLevel.Information, "Sending password reset confirmation email");
        VerifyLogCalled(LogLevel.Information, "Password reset confirmation email sent successfully");
    }

    /// <summary>
    /// パスワードリセット完了通知メール送信 - メール送信失敗
    /// </summary>
    [Fact]
    public async Task SendPasswordResetConfirmationAsync_EmailSendFails_ShouldReturnError()
    {
        // Arrange
        var email = Email.create("test@example.com").ResultValue;

        _emailSenderMock.Setup(x => x.SendEmailAsync(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(FSharpResult<Unit, string>.NewError("Mail server unavailable"));

        // Act
        var result = await _notificationService.SendPasswordResetConfirmationAsync(email);

        // Assert
        Assert.True(result.IsError);
        Assert.Contains("確認メール送信に失敗しました", result.ErrorValue);
        
        // ⚠️ 確認メール送信失敗は Warning レベル（主要処理は完了済みのため）
        VerifyLogCalled(LogLevel.Warning, "Failed to send password reset confirmation email");
    }

    /// <summary>
    /// パスワードリセット完了通知メール - HTMLテンプレート検証
    /// </summary>
    [Fact]
    public async Task SendPasswordResetConfirmationAsync_ShouldCreateProperHtmlTemplate()
    {
        // Arrange
        var email = Email.create("secure@example.com").ResultValue;

        string capturedHtmlContent = null;
        _emailSenderMock.Setup(x => x.SendEmailAsync(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .Callback<string, string, string>((emailAddr, subject, htmlContent) => {
                capturedHtmlContent = htmlContent;
            })
            .ReturnsAsync(FSharpResult<Unit, string>.NewOk(null));

        // Act
        var result = await _notificationService.SendPasswordResetConfirmationAsync(email);

        // Assert
        Assert.True(result.IsOk);
        Assert.NotNull(capturedHtmlContent);
        
        // 🎨 HTMLテンプレートの必須要素検証
        Assert.Contains("パスワード変更完了", capturedHtmlContent);
        Assert.Contains("secure@example.com", capturedHtmlContent);
        Assert.Contains("パスワードの変更が完了しました", capturedHtmlContent);
        Assert.Contains("セキュリティに関する重要なお知らせ", capturedHtmlContent);
        Assert.Contains("既存の全てのセッションは自動的に無効化", capturedHtmlContent);
        Assert.Contains("他のデバイスでログインし直す", capturedHtmlContent);
        Assert.Contains("心当たりのない変更の場合", capturedHtmlContent);
        
        // 📅 タイムスタンプが含まれることを確認
        Assert.Contains("変更日時:", capturedHtmlContent);
        Assert.Contains("年", capturedHtmlContent); // 日本語の日付形式
    }

    /// <summary>
    /// パスワードリセット完了通知メール送信 - 例外発生
    /// </summary>
    [Fact]
    public async Task SendPasswordResetConfirmationAsync_ExceptionThrown_ShouldReturnError()
    {
        // Arrange
        var email = Email.create("test@example.com").ResultValue;

        _emailSenderMock.Setup(x => x.SendEmailAsync(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .ThrowsAsync(new TimeoutException("Email server timeout"));

        // Act
        var result = await _notificationService.SendPasswordResetConfirmationAsync(email);

        // Assert
        Assert.True(result.IsError);
        Assert.Contains("確認メール送信中にエラー", result.ErrorValue);
        
        // ⚠️ 確認メール例外は Warning レベル（主要処理は完了済みのため）
        VerifyLogCalled(LogLevel.Warning, "Unexpected error while sending password reset confirmation email");
    }

    #endregion

    #region Edge Cases and Security Tests

    /// <summary>
    /// 特殊文字を含むメールアドレス - エスケープ処理確認
    /// </summary>
    [Fact]
    public async Task SendPasswordResetEmailAsync_SpecialCharactersInEmail_ShouldHandleCorrectly()
    {
        // Arrange
        var email = Email.create("test+user@example-domain.co.jp").ResultValue;
        var resetToken = "token";
        var resetUrl = "https://localhost:5001/reset?email=" + Uri.EscapeDataString(email.Value) + "&token=" + resetToken;

        _emailSenderMock.Setup(x => x.SendEmailAsync(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(FSharpResult<Unit, string>.NewOk(null));

        // Act
        var result = await _notificationService.SendPasswordResetEmailAsync(email, resetToken, resetUrl);

        // Assert
        Assert.True(result.IsOk);
        
        // 📧 特殊文字を含むメールアドレスでも正常に処理されることを確認
        _emailSenderMock.Verify(x => x.SendEmailAsync(
            "test+user@example-domain.co.jp",
            It.IsAny<string>(),
            It.Is<string>(content => content.Contains("test+user@example-domain.co.jp"))), Times.Once);
    }

    /// <summary>
    /// 長いリセットURL - テンプレート表示確認
    /// </summary>
    [Fact]
    public async Task SendPasswordResetEmailAsync_LongResetUrl_ShouldHandleCorrectly()
    {
        // Arrange
        var email = Email.create("test@example.com").ResultValue;
        var resetToken = "very_long_reset_token_with_lots_of_characters_0123456789abcdef";
        var longUrl = "https://very-long-domain-name.example.com/account/reset-password?email=test@example.com&token=" + resetToken + "&extra=parameter";

        string capturedHtmlContent = null;
        _emailSenderMock.Setup(x => x.SendEmailAsync(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .Callback<string, string, string>((emailAddr, subject, htmlContent) => {
                capturedHtmlContent = htmlContent;
            })
            .ReturnsAsync(FSharpResult<Unit, string>.NewOk(null));

        // Act
        var result = await _notificationService.SendPasswordResetEmailAsync(email, resetToken, longUrl);

        // Assert
        Assert.True(result.IsOk);
        Assert.NotNull(capturedHtmlContent);
        
        // 🔗 長いURLが適切にテンプレートに含まれることを確認
        Assert.Contains(longUrl, capturedHtmlContent);
        
        // 🎨 word-break: break-all スタイルが適用されていることを確認
        Assert.Contains("word-break: break-all", capturedHtmlContent);
    }

    #endregion

    #region Helper Methods

    /// <summary>
    /// ログ出力の検証ヘルパーメソッド
    /// </summary>
    /// <param name="expectedLevel">期待するログレベル</param>
    /// <param name="expectedMessage">期待するメッセージの一部</param>
    /// <remarks>
    /// 📊 ADR_008準拠: ログ出力の検証
    /// </remarks>
    private void VerifyLogCalled(LogLevel expectedLevel, string expectedMessage)
    {
        _loggerMock.Verify(
            x => x.Log(
                expectedLevel,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains(expectedMessage)),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.AtLeastOnce);
    }

    #endregion
}