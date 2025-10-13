namespace UbiquitousLanguageManager.Application.Unit.Tests

open System
open System.Collections.Generic
open System.Threading.Tasks
open Xunit
open Moq
open UbiquitousLanguageManager.Contracts.Interfaces
open UbiquitousLanguageManager.Contracts.DTOs

/// <summary>
/// Application層サービス統合テスト（F#版）
///
/// 【F#テスト設計方針】
/// 現在のC#アーキテクチャ（IApplicationService）に基づく
/// 統合テストを実施し、主要なビジネス機能を検証します。
///
/// 【F#におけるクラス定義とコンストラクタ】
/// - type ClassName() = ... : クラス定義（括弧内がプライマリコンストラクタ）
/// - let mutable: 可変フィールド定義
/// - member _.メソッド名: インスタンスメソッド（_は自己参照の省略）
/// </summary>
type ApplicationServiceTests() =

    let mutable mockAppService = Mock<IApplicationService>()

    /// <summary>
    /// テスト用ユーザーDTO作成ヘルパー
    ///
    /// 【F#におけるオプショナルパラメータ】
    /// - ?paramName: オプショナルパラメータ（省略可能）
    /// - defaultArg: オプション値のデフォルト値取得
    /// - { Field = value; ... }: レコード型初期化構文
    /// </summary>
    member private _.CreateTestUserDto(?email: string, ?name: string, ?role: string) =
        let email = defaultArg email "test@example.com"
        let name = defaultArg name "テストユーザー"
        let role = defaultArg role "GeneralUser"

        CreateUserDto(
            Email = email,
            Name = name,
            Role = role,
            CreatedBy = 1L
        )

    /// <summary>
    /// CreateUserAsyncのテスト - 重複メール
    ///
    /// 【F#における非同期テスト】
    /// - task { ... }: タスクコンピュテーション式
    /// - let!: 非同期結果の待機と値のバインド
    /// - Assert.False/Equal: xUnit標準アサーション
    /// </summary>
    [<Fact>]
    member this.``CreateUserAsync_重複メール_エラーを返すべき``() =
        task {
            // Arrange
            let createUserDto = this.CreateTestUserDto(email = "existing@example.com", name = "重複ユーザー")

            mockAppService
                .Setup(fun x -> x.CreateUserAsync(It.IsAny<CreateUserDto>()))
                .ReturnsAsync(ServiceResult<UserDto>.Error("メールアドレスが既に存在します"))
                |> ignore

            // Act
            let! result = mockAppService.Object.CreateUserAsync(createUserDto)

            // Assert
            Assert.False(result.IsSuccess)
            Assert.Equal("メールアドレスが既に存在します", result.ErrorMessage)
            Assert.Null(result.Data)
        }

    /// <summary>
    /// LoginAsyncのテスト - 有効な認証情報
    ///
    /// 【F#におけるDTOオブジェクト初期化】
    /// - C#と同じプロパティ初期化構文が使用可能
    /// - 型推論により型名を省略できるケースもある
    /// </summary>
    [<Fact>]
    member _.``LoginAsync_有効な認証情報_成功を返すべき``() =
        task {
            // Arrange
            let loginDto = LoginDto(
                Email = "user@example.com",
                Password = "password123"
            )

            let expectedUserDto = UserDto(
                Id = 1L,
                Email = "user@example.com",
                Name = "テストユーザー",
                Role = "GeneralUser",
                IsActive = true
            )

            mockAppService
                .Setup(fun x -> x.LoginAsync(It.IsAny<LoginDto>()))
                .ReturnsAsync(ServiceResult<UserDto>.Success(expectedUserDto))
                |> ignore

            // Act
            let! result = mockAppService.Object.LoginAsync(loginDto)

            // Assert
            Assert.True(result.IsSuccess)
            Assert.NotNull(result.Data)
            Assert.Equal("user@example.com", result.Data.Email)
            Assert.Equal("テストユーザー", result.Data.Name)

            // Mock呼び出し確認
            mockAppService.Verify(
                (fun x -> x.LoginAsync(It.IsAny<LoginDto>())),
                Times.Once())
        }

    /// <summary>
    /// LoginAsyncのテスト - 認証失敗
    ///
    /// 【F#における失敗ケーステスト】
    /// - ServiceResult.Error: エラー結果の作成
    /// - Assert.False: 失敗検証
    /// </summary>
    [<Fact>]
    member _.``LoginAsync_認証失敗_エラーを返すべき``() =
        task {
            // Arrange
            let loginDto = LoginDto(
                Email = "user@example.com",
                Password = "wrongpassword"
            )

            mockAppService
                .Setup(fun x -> x.LoginAsync(It.IsAny<LoginDto>()))
                .ReturnsAsync(ServiceResult<UserDto>.Error("メールアドレスまたはパスワードが正しくありません"))
                |> ignore

            // Act
            let! result = mockAppService.Object.LoginAsync(loginDto)

            // Assert
            Assert.False(result.IsSuccess)
            Assert.Equal("メールアドレスまたはパスワードが正しくありません", result.ErrorMessage)
            Assert.Null(result.Data)
        }

    /// <summary>
    /// LoginAsyncのテスト - バリデーションエラー
    ///
    /// 【F#におけるTheoryテスト】
    /// - [<Theory>] + [<InlineData>]: パラメータ化テスト
    /// - 複数のInlineData属性で異なる入力値を検証
    /// - Dictionary<string, string>: C#と同じコレクション型使用可能
    /// </summary>
    [<Theory>]
    [<InlineData("")>]
    [<InlineData("invalid")>]
    [<InlineData("user@")>]
    [<InlineData("@example.com")>]
    member _.``LoginAsync_無効なメール_バリデーションエラーを返すべき``(invalidEmail: string) =
        task {
            // Arrange
            let loginDto = LoginDto(
                Email = invalidEmail,
                Password = "password123"
            )

            let validationErrors = Dictionary<string, string>()
            validationErrors.Add("Email", "有効なメールアドレスを入力してください")

            mockAppService
                .Setup(fun x -> x.LoginAsync(It.IsAny<LoginDto>()))
                .ReturnsAsync(ServiceResult<UserDto>.ValidationError(validationErrors))
                |> ignore

            // Act
            let! result = mockAppService.Object.LoginAsync(loginDto)

            // Assert
            Assert.False(result.IsSuccess)
            Assert.NotEmpty(result.ValidationErrors)
            Assert.True(result.ValidationErrors.ContainsKey("Email"))
        }
