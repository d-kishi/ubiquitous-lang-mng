namespace UbiquitousLanguageManager.Application.Unit.Tests

open System
open System.Threading.Tasks
open Xunit
open NSubstitute
open FluentAssertions
open Microsoft.AspNetCore.Identity
open Microsoft.Extensions.Logging
open UbiquitousLanguageManager.Infrastructure.Services
open UbiquitousLanguageManager.Contracts.DTOs
open UbiquitousLanguageManager.Contracts.DTOs.Common
open UbiquitousLanguageManager.Contracts.Interfaces
open UbiquitousLanguageManager.Infrastructure.Data.Entities

/// <summary>
/// パスワードリセットサービスの単体テスト（F#版）
/// 仕様書2.1.3準拠: パスワードリセット機能
///
/// 【F#におけるNSubstituteモック】
/// - Substitute.For<'T>(): モックオブジェクト作成
/// - Returns(): モックメソッドの戻り値設定
/// - Received(n): メソッド呼び出し回数検証
/// - DidNotReceive(): メソッドが呼ばれていないことを検証
///
/// 【F#におけるFluentAssertions】
/// - .Should().BeTrue(): 真値検証
/// - .Should().BeFalse(): 偽値検証
/// - .Should().Be(expected): 等値検証
/// </summary>
type PasswordResetServiceTests() =

    let mockUserManager =
        // 🔧 UserManagerのモック作成: NSubstituteでの複雑な型のモック化
        let userStore = Substitute.For<IUserStore<ApplicationUser>>()
        Substitute.For<UserManager<ApplicationUser>>(
            userStore, null, null, null, null, null, null, null, null)

    let mockEmailSender = Substitute.For<IEmailSender>()
    let mockLogger = Substitute.For<ILogger<PasswordResetService>>()

    let service = PasswordResetService(mockUserManager, mockEmailSender, mockLogger)

    /// <summary>
    /// RequestPasswordResetAsync_正常なメールアドレスでリセットメール送信成功
    ///
    /// 【F#における非同期モック設定】
    /// - Task.FromResult: 同期値をTaskでラップ
    /// - do!: 非同期処理の実行（戻り値不要）
    /// - Received(1): 1回呼び出されたことを検証
    /// </summary>
    [<Fact>]
    member _.``RequestPasswordResetAsync_正常なメールアドレスでリセットメール送信成功``() =
        task {
            // Arrange
            let email = "test@example.com"
            let user = ApplicationUser(
                Id = Guid.NewGuid().ToString(),
                Email = email,
                UserName = email
            )
            let resetToken = "test-reset-token-12345"

            mockUserManager.FindByEmailAsync(email).Returns(Task.FromResult(user)) |> ignore
            mockUserManager.GeneratePasswordResetTokenAsync(user).Returns(Task.FromResult(resetToken)) |> ignore
            mockEmailSender.SendPasswordResetEmailAsync(email, resetToken).Returns(Task.FromResult(true)) |> ignore

            // Act
            let! result = service.RequestPasswordResetAsync(email)

            // Assert
            result.IsSuccess.Should().BeTrue() |> ignore

            // 🔍 モック検証: 適切な順序でメソッドが呼ばれたか
            mockUserManager.Received(1).FindByEmailAsync(email) |> ignore
            mockUserManager.Received(1).GeneratePasswordResetTokenAsync(user) |> ignore
            mockEmailSender.Received(1).SendPasswordResetEmailAsync(email, resetToken) |> ignore
        }

    /// <summary>
    /// RequestPasswordResetAsync_未登録メールアドレスでエラー
    ///
    /// 【F#におけるnull処理】
    /// - Task.FromResult<ApplicationUser>(null): null値を含むTask
    /// - .Should().Be("message"): エラーメッセージ検証
    /// - DidNotReceive(): メソッドが呼ばれていないことを確認
    /// </summary>
    [<Fact>]
    member _.``RequestPasswordResetAsync_未登録メールアドレスでエラー``() =
        task {
            // Arrange
            let email = "notfound@example.com"

            mockUserManager.FindByEmailAsync(email)
                .Returns(Task.FromResult<ApplicationUser>(null))
                |> ignore

            // Act
            let! result = service.RequestPasswordResetAsync(email)

            // Assert
            result.IsSuccess.Should().BeFalse() |> ignore
            result.Error.Should().Be("メールアドレスが見つかりません") |> ignore

            // 🚫 メール送信やトークン生成は実行されないことを確認
            mockUserManager.DidNotReceive()
                .GeneratePasswordResetTokenAsync(Arg.Any<ApplicationUser>())
                |> ignore
            mockEmailSender.DidNotReceive()
                .SendPasswordResetEmailAsync(Arg.Any<string>(), Arg.Any<string>())
                |> ignore
        }

    /// <summary>
    /// ResetPasswordAsync_正常なトークンでパスワードリセット成功
    ///
    /// 【F#におけるIdentityResult処理】
    /// - IdentityResult.Success: 成功結果
    /// - ResetPasswordAsync: パスワードリセット実行
    /// </summary>
    [<Fact>]
    member _.``ResetPasswordAsync_正常なトークンでパスワードリセット成功``() =
        task {
            // Arrange
            let email = "test@example.com"
            let token = "valid-token"
            let newPassword = "NewPassword123!"
            let user = ApplicationUser(
                Id = Guid.NewGuid().ToString(),
                Email = email,
                UserName = email
            )

            mockUserManager.FindByEmailAsync(email).Returns(Task.FromResult(user)) |> ignore
            mockUserManager.ResetPasswordAsync(user, token, newPassword)
                .Returns(Task.FromResult(IdentityResult.Success))
                |> ignore

            // Act
            let! result = service.ResetPasswordAsync(email, token, newPassword)

            // Assert
            result.IsSuccess.Should().BeTrue() |> ignore

            // 🔍 モック検証
            mockUserManager.Received(1).FindByEmailAsync(email) |> ignore
            mockUserManager.Received(1).ResetPasswordAsync(user, token, newPassword) |> ignore
        }

    /// <summary>
    /// ResetPasswordAsync_無効なトークンでエラー
    ///
    /// 【F#におけるIdentityError処理】
    /// - IdentityError: エラー情報オブジェクト
    /// - IdentityResult.Failed: 失敗結果
    /// - .Should().Contain("text"): 部分一致検証
    /// </summary>
    [<Fact>]
    member _.``ResetPasswordAsync_無効なトークンでエラー``() =
        task {
            // Arrange
            let email = "test@example.com"
            let invalidToken = "invalid-token"
            let newPassword = "NewPassword123!"
            let user = ApplicationUser(
                Id = Guid.NewGuid().ToString(),
                Email = email,
                UserName = email
            )

            mockUserManager.FindByEmailAsync(email).Returns(Task.FromResult(user)) |> ignore
            mockUserManager.ResetPasswordAsync(user, invalidToken, newPassword)
                .Returns(Task.FromResult(
                    IdentityResult.Failed(
                        IdentityError(Code = "InvalidToken", Description = "Invalid token"))))
                |> ignore

            // Act
            let! result = service.ResetPasswordAsync(email, invalidToken, newPassword)

            // Assert
            result.IsSuccess.Should().BeFalse() |> ignore
            result.Error.Should().Contain("リセットトークンが無効です") |> ignore
        }

    /// <summary>
    /// ResetPasswordAsync_弱いパスワードでエラー
    ///
    /// 【F#におけるパスワードバリデーション】
    /// - "PasswordTooShort": パスワード長エラー
    /// - エラーメッセージの検証
    /// </summary>
    [<Fact>]
    member _.``ResetPasswordAsync_弱いパスワードでエラー``() =
        task {
            // Arrange
            let email = "test@example.com"
            let token = "valid-token"
            let weakPassword = "123"
            let user = ApplicationUser(
                Id = Guid.NewGuid().ToString(),
                Email = email,
                UserName = email
            )

            mockUserManager.FindByEmailAsync(email).Returns(Task.FromResult(user)) |> ignore
            mockUserManager.ResetPasswordAsync(user, token, weakPassword)
                .Returns(Task.FromResult(
                    IdentityResult.Failed(
                        IdentityError(Code = "PasswordTooShort", Description = "Password too short"))))
                |> ignore

            // Act
            let! result = service.ResetPasswordAsync(email, token, weakPassword)

            // Assert
            result.IsSuccess.Should().BeFalse() |> ignore
            result.Error.Should().Contain("パスワードが要件を満たしていません") |> ignore
        }

    /// <summary>
    /// ValidateResetTokenAsync_有効なトークンで検証成功
    ///
    /// 【F#におけるトークン検証】
    /// - VerifyUserTokenAsync: トークン有効性検証
    /// - .Value: Result値の取得
    /// - .Should().BeTrue(): 真値検証
    /// </summary>
    [<Fact>]
    member _.``ValidateResetTokenAsync_有効なトークンで検証成功``() =
        task {
            // Arrange
            let email = "test@example.com"
            let token = "valid-token"
            let user = ApplicationUser(
                Id = Guid.NewGuid().ToString(),
                Email = email,
                UserName = email
            )

            mockUserManager.FindByEmailAsync(email).Returns(Task.FromResult(user)) |> ignore
            mockUserManager.VerifyUserTokenAsync(
                user,
                mockUserManager.Options.Tokens.PasswordResetTokenProvider,
                "ResetPassword",
                token).Returns(Task.FromResult(true)) |> ignore

            // Act
            let! result = service.ValidateResetTokenAsync(email, token)

            // Assert
            result.IsSuccess.Should().BeTrue() |> ignore
            result.Value.Should().BeTrue() |> ignore
        }

    /// <summary>
    /// ValidateResetTokenAsync_無効なトークンで検証失敗
    ///
    /// 【F#における検証失敗テスト】
    /// - .Should().BeFalse(): 偽値検証
    /// - トークン無効時の適切な処理確認
    /// </summary>
    [<Fact>]
    member _.``ValidateResetTokenAsync_無効なトークンで検証失敗``() =
        task {
            // Arrange
            let email = "test@example.com"
            let invalidToken = "invalid-token"
            let user = ApplicationUser(
                Id = Guid.NewGuid().ToString(),
                Email = email,
                UserName = email
            )

            mockUserManager.FindByEmailAsync(email).Returns(Task.FromResult(user)) |> ignore
            mockUserManager.VerifyUserTokenAsync(
                user,
                mockUserManager.Options.Tokens.PasswordResetTokenProvider,
                "ResetPassword",
                invalidToken).Returns(Task.FromResult(false)) |> ignore

            // Act
            let! result = service.ValidateResetTokenAsync(email, invalidToken)

            // Assert
            result.IsSuccess.Should().BeTrue() |> ignore
            result.Value.Should().BeFalse() |> ignore
        }
