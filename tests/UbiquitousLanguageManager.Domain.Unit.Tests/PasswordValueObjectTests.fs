namespace UbiquitousLanguageManager.Domain.Unit.Tests

open System
open Xunit
open UbiquitousLanguageManager.Domain.Common
open UbiquitousLanguageManager.Domain.Authentication

// 🧪 Password Value Objectの単体テスト（C#→F#変換・簡略版）
// テスト数を主要ケースに絞り込み、Result型のF#ネイティブパターンマッチングを使用

type ValidPasswordSimpleTests() =

    [<Theory>]
    [<InlineData("Password123!")>]
    [<InlineData("MySecure1")>]
    [<InlineData("Complex123A")>]
    member this.``Create_ValidPassword_ShouldReturnOk``(validPassword: string) =
        match Password.create validPassword with
        | Ok pwd -> Assert.Equal(validPassword, pwd.Value)
        | Error msg -> Assert.True(false, $"Expected Ok but got Error: {msg}")

    [<Fact>]
    member this.``Create_MinimumLengthPassword_ShouldReturnOk``() =
        let password = "Pass123A"
        match Password.create password with
        | Ok pwd -> Assert.Equal(8, pwd.Value.Length)
        | Error msg -> Assert.True(false, $"Expected Ok but got Error: {msg}")


type LengthValidationSimpleTests() =

    [<Fact>]
    member this.``Create_EmptyPassword_ShouldReturnError``() =
        match Password.create "" with
        | Error msg -> Assert.Equal("パスワードが入力されていません", msg)
        | Ok _ -> Assert.True(false, "Expected Error but got Ok")

    [<Fact>]
    member this.``Create_NullPassword_ShouldReturnError``() =
        match Password.create null with
        | Error msg -> Assert.Equal("パスワードが入力されていません", msg)
        | Ok _ -> Assert.True(false, "Expected Error but got Ok")

    [<Theory>]
    [<InlineData("Pass1A")>]
    [<InlineData("Ab1")>]
    member this.``Create_TooShortPassword_ShouldReturnError``(shortPassword: string) =
        match Password.create shortPassword with
        | Error msg -> Assert.Equal("パスワードは8文字以上で入力してください", msg)
        | Ok _ -> Assert.True(false, "Expected Error but got Ok")


type StrengthValidationSimpleTests() =

    [<Theory>]
    [<InlineData("password123")>]
    [<InlineData("mypassword1")>]
    member this.``Create_NoUpperCase_ShouldReturnError``(pwd: string) =
        match Password.create pwd with
        | Error msg -> Assert.Equal("パスワードには大文字を含めてください", msg)
        | Ok _ -> Assert.True(false, "Expected Error but got Ok")

    [<Theory>]
    [<InlineData("PASSWORD123")>]
    [<InlineData("MYPASSWORD1")>]
    member this.``Create_NoLowerCase_ShouldReturnError``(pwd: string) =
        match Password.create pwd with
        | Error msg -> Assert.Equal("パスワードには小文字を含めてください", msg)
        | Ok _ -> Assert.True(false, "Expected Error but got Ok")

    [<Theory>]
    [<InlineData("PasswordABC")>]
    [<InlineData("MyPasswordZ")>]
    member this.``Create_NoDigit_ShouldReturnError``(pwd: string) =
        match Password.create pwd with
        | Error msg -> Assert.Equal("パスワードには数字を含めてください", msg)
        | Ok _ -> Assert.True(false, "Expected Error but got Ok")
