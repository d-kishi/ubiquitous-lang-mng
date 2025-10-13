namespace UbiquitousLanguageManager.Domain.Unit.Tests

open System
open Xunit
open UbiquitousLanguageManager.Domain.Common
open UbiquitousLanguageManager.Domain.Authentication

// 🧪 Value Objectsの単体テスト（C#→F#変換）
//
// 【テスト方針】
// F#のValue Objectsが正しく動作することを確認します。
// 特にスマートコンストラクタによる検証ロジックをテストします。

// 【F#初学者向け解説】
// F#でのValue Objectテスト：
// 1. Smart Constructorパターン：create関数でResult<T, string>を返す
// 2. match式によるResult型の検証
// 3. [<Theory>]と[<InlineData>]でパラメータ化テスト
// 4. String.replicateで繰り返し文字列生成


// ========================================
// Email Value Object のテスト
// ========================================
type EmailTests() =

    [<Fact>]
    member this.``Email_ValidFormat_ShouldCreateSuccessfully``() =
        // Arrange
        let validEmail = "test@example.com"

        // Act
        let result = Email.create validEmail

        // Assert
        match result with
        | Ok email -> Assert.Equal(validEmail, email.Value)
        | Error msg -> Assert.True(false, $"Expected Ok but got Error: {msg}")

    [<Theory>]
    [<InlineData("")>]
    [<InlineData("invalid-email")>]
    [<InlineData("@example.com")>]
    [<InlineData("test@")>]
    [<InlineData("test.example.com")>]
    member this.``Email_InvalidFormat_ShouldReturnError``(invalidEmail: string) =
        // Arrange & Act
        let result = Email.create invalidEmail

        // Assert
        match result with
        | Error _ -> Assert.True(true)
        | Ok _ -> Assert.True(false, "Expected Error but got Ok")

    [<Fact>]
    member this.``Email_TooLong_ShouldReturnError``() =
        // Arrange
        let longEmail = (String.replicate 250 "a") + "@example.com"

        // Act
        let result = Email.create longEmail

        // Assert
        match result with
        | Error _ -> Assert.True(true)
        | Ok _ -> Assert.True(false, "Expected Error but got Ok")


// ========================================
// UserName Value Object のテスト
// ========================================
type UserNameTests() =

    [<Fact>]
    member this.``UserName_ValidName_ShouldCreateSuccessfully``() =
        // Arrange
        let validName = "田中太郎"

        // Act
        let result = UserName.create validName

        // Assert
        match result with
        | Ok userName -> Assert.Equal(validName, userName.Value)
        | Error msg -> Assert.True(false, $"Expected Ok but got Error: {msg}")

    [<Theory>]
    [<InlineData("")>]
    [<InlineData("   ")>]
    member this.``UserName_EmptyOrWhitespace_ShouldReturnError``(invalidName: string) =
        // Arrange & Act
        let result = UserName.create invalidName

        // Assert
        match result with
        | Error _ -> Assert.True(true)
        | Ok _ -> Assert.True(false, "Expected Error but got Ok")

    [<Fact>]
    member this.``UserName_TooLong_ShouldReturnError``() =
        // Arrange
        let longName = String.replicate 51 "あ"  // 51文字

        // Act
        let result = UserName.create longName

        // Assert
        match result with
        | Error _ -> Assert.True(true)
        | Ok _ -> Assert.True(false, "Expected Error but got Ok")

    [<Fact>]
    member this.``UserName_Null_ShouldReturnError``() =
        // Arrange & Act
        let result = UserName.create null

        // Assert
        match result with
        | Error _ -> Assert.True(true)
        | Ok _ -> Assert.True(false, "Expected Error but got Ok")


// ========================================
// PasswordHash Value Object のテスト
// ========================================
type PasswordHashTests() =

    [<Fact>]
    member this.``PasswordHash_ValidHash_ShouldCreateSuccessfully``() =
        // Arrange
        let validHash = "$2a$11$example.hash.string.for.testing"

        // Act
        let result = PasswordHash.create validHash

        // Assert
        match result with
        | Ok passwordHash -> Assert.Equal(validHash, passwordHash.Value)
        | Error msg -> Assert.True(false, $"Expected Ok but got Error: {msg}")

    [<Theory>]
    [<InlineData("")>]
    [<InlineData("   ")>]
    member this.``PasswordHash_EmptyOrWhitespace_ShouldReturnError``(invalidHash: string) =
        // Arrange & Act
        let result = PasswordHash.create invalidHash

        // Assert
        match result with
        | Error _ -> Assert.True(true)
        | Ok _ -> Assert.True(false, "Expected Error but got Ok")

    [<Fact>]
    member this.``PasswordHash_Null_ShouldReturnError``() =
        // Arrange & Act
        let result = PasswordHash.create null

        // Assert
        match result with
        | Error _ -> Assert.True(true)
        | Ok _ -> Assert.True(false, "Expected Error but got Ok")
