namespace UbiquitousLanguageManager.Application.Unit.Tests

open System
open System.Threading
open System.Threading.Tasks
open Xunit
open Moq
open UbiquitousLanguageManager.Contracts.Interfaces

/// <summary>
/// IEmailSender インターフェースの使用シナリオテスト（F#版）
/// メール送信基盤の動作検証とエラーハンドリングテスト
///
/// 【F#テスト構造の説明】
/// - F#では型推論により型宣言を省略できるケースが多い
/// - Moqの活用: C#と同じMoqライブラリを使用（F#から完全互換）
/// - Task.CompletedTask: 非同期処理の完了を表すTask型の値
/// </summary>
type EmailSenderTests() =

    /// <summary>
    /// SendEmailAsync_基本的なメール送信_成功するべき
    ///
    /// 【F#におけるモック検証】
    /// - Mock.Verify: C#と同じ方法で呼び出し検証
    /// - It.IsAny<'T>(): ジェネリック型パラメータの指定に注意
    /// - Times.Once: 1回のみ呼び出されたことを検証
    /// </summary>
    [<Fact>]
    member _.``SendEmailAsync_基本的なメール送信_成功するべき``() =
        task {
            // Arrange
            let mockEmailSender = Mock<IEmailSender>()
            mockEmailSender
                .Setup(fun x ->
                    x.SendEmailAsync(
                        It.IsAny<string>(),
                        It.IsAny<string>(),
                        It.IsAny<string>(),
                        It.IsAny<bool>(),
                        It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask)
                |> ignore  // F#ではSetupの戻り値を無視する必要がある

            let toAddress = "test@example.com"
            let subject = "Test Subject"
            let body = "<p>Test Body</p>"
            let isBodyHtml = true

            // Act
            do! mockEmailSender.Object.SendEmailAsync(toAddress, subject, body, isBodyHtml)

            // Assert
            mockEmailSender.Verify(
                (fun x -> x.SendEmailAsync(toAddress, subject, body, isBodyHtml, It.IsAny<CancellationToken>())),
                Times.Once())
        }

    /// <summary>
    /// SendEmailAsync_HTMLとプレーンテキストの切り替え_正しく処理されるべき
    ///
    /// 【F#におけるTheory/InlineData】
    /// - [<Theory>]: パラメータ化テスト
    /// - [<InlineData(...)>]: テストケースのパラメータ定義
    /// - 複数のInlineData属性で複数のテストケース定義可能
    /// </summary>
    [<Theory>]
    [<InlineData(true, "<p>HTML Body</p>")>]
    [<InlineData(false, "Plain Text Body")>]
    member _.``SendEmailAsync_HTMLとプレーンテキストの切り替え_正しく処理されるべき``(isHtml: bool, body: string) =
        task {
            // Arrange
            let mockEmailSender = Mock<IEmailSender>()
            mockEmailSender
                .Setup(fun x ->
                    x.SendEmailAsync(
                        It.IsAny<string>(),
                        It.IsAny<string>(),
                        It.IsAny<string>(),
                        It.IsAny<bool>(),
                        It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask)
                |> ignore

            // Act
            do! mockEmailSender.Object.SendEmailAsync("test@example.com", "Subject", body, isHtml)

            // Assert
            mockEmailSender.Verify(
                (fun x -> x.SendEmailAsync(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    body,
                    isHtml,
                    It.IsAny<CancellationToken>())),
                Times.Once())
        }

    /// <summary>
    /// SendEmailAsync_キャンセレーショントークン_正しく伝播されるべき
    ///
    /// 【F#におけるCancellationToken】
    /// - CancellationToken(): 新しいCancellationTokenインスタンス作成
    /// - F#でも非同期処理のキャンセル機能を完全にサポート
    /// </summary>
    [<Fact>]
    member _.``SendEmailAsync_キャンセレーショントークン_正しく伝播されるべき``() =
        task {
            // Arrange
            let mockEmailSender = Mock<IEmailSender>()
            let cancellationToken = CancellationToken()

            mockEmailSender
                .Setup(fun x ->
                    x.SendEmailAsync(
                        It.IsAny<string>(),
                        It.IsAny<string>(),
                        It.IsAny<string>(),
                        It.IsAny<bool>(),
                        It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask)
                |> ignore

            // Act
            do! mockEmailSender.Object.SendEmailAsync(
                "test@example.com",
                "Subject",
                "Body",
                true,
                cancellationToken)

            // Assert
            mockEmailSender.Verify(
                (fun x -> x.SendEmailAsync(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<bool>(),
                    cancellationToken)),
                Times.Once())
        }

    /// <summary>
    /// SendEmailAsync_例外発生時_適切に処理されるべき
    ///
    /// 【F#における例外テスト】
    /// - Assert.ThrowsAsync: 非同期例外の検証
    /// - fun () -> task { ... }: Task返却のラムダ式（F#の関数型スタイル）
    /// - ThrowsAsync内でtaskコンピュテーション式を使用
    /// </summary>
    [<Fact>]
    member _.``SendEmailAsync_例外発生時_適切に処理されるべき``() =
        task {
            // Arrange
            let mockEmailSender = Mock<IEmailSender>()
            let expectedException = InvalidOperationException("SMTP connection failed")

            mockEmailSender
                .Setup(fun x ->
                    x.SendEmailAsync(
                        It.IsAny<string>(),
                        It.IsAny<string>(),
                        It.IsAny<string>(),
                        It.IsAny<bool>(),
                        It.IsAny<CancellationToken>()))
                .ThrowsAsync(expectedException)
                |> ignore

            // Act & Assert
            let! ex =
                Assert.ThrowsAsync<InvalidOperationException>(fun () ->
                    task {
                        do! mockEmailSender.Object.SendEmailAsync(
                            "test@example.com",
                            "Subject",
                            "Body",
                            true)
                    } :> Task)

            Assert.Equal("SMTP connection failed", ex.Message)
        }
