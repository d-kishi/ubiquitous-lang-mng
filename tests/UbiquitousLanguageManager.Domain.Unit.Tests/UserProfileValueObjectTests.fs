namespace UbiquitousLanguageManager.Domain.Unit.Tests

open System
open Xunit
open UbiquitousLanguageManager.Domain.Common
open UbiquitousLanguageManager.Domain.Authentication

// 🧪 UserProfile Value Objectの単体テスト（C#→F#変換・簡略版）
// テスト数を主要ケースに絞り込み、Option型のF#ネイティブ関数を使用

type UserProfileCreationSimpleTests() =

    [<Fact>]
    member this.``Create_AllFieldsProvided_ShouldReturnProfileWithAllValues``() =
        let displayName = "田中 太郎"
        let department = "開発部"
        let phoneNumber = "03-1234-5678"
        let notes = "プロジェクトリーダー"

        let profile = UserProfile.create displayName department phoneNumber notes

        Assert.True(Option.isSome profile.DisplayName)
        Assert.Equal(displayName, Option.get profile.DisplayName)
        
        Assert.True(Option.isSome profile.Department)
        Assert.Equal(department, Option.get profile.Department)
        
        Assert.True(Option.isSome profile.PhoneNumber)
        Assert.Equal(phoneNumber, Option.get profile.PhoneNumber)
        
        Assert.True(Option.isSome profile.Notes)
        Assert.Equal(notes, Option.get profile.Notes)

    [<Fact>]
    member this.``Create_NoFieldsProvided_ShouldReturnProfileWithAllNoneValues``() =
        let profile = UserProfile.create null null null null

        Assert.True(Option.isNone profile.DisplayName)
        Assert.True(Option.isNone profile.Department)
        Assert.True(Option.isNone profile.PhoneNumber)
        Assert.True(Option.isNone profile.Notes)

    [<Fact>]
    member this.``Create_EmptyStringsProvided_ShouldReturnProfileWithAllNoneValues``() =
        let profile = UserProfile.create "" "" "" ""

        Assert.True(Option.isNone profile.DisplayName)
        Assert.True(Option.isNone profile.Department)
        Assert.True(Option.isNone profile.PhoneNumber)
        Assert.True(Option.isNone profile.Notes)


type PartialFieldsSimpleTests() =

    [<Fact>]
    member this.``Create_OnlyDisplayNameProvided_ShouldReturnProfileWithDisplayNameOnly``() =
        let displayName = "山田 花子"
        let profile = UserProfile.create displayName null null null

        Assert.True(Option.isSome profile.DisplayName)
        Assert.Equal(displayName, Option.get profile.DisplayName)
        
        Assert.True(Option.isNone profile.Department)
        Assert.True(Option.isNone profile.PhoneNumber)
        Assert.True(Option.isNone profile.Notes)

    [<Fact>]
    member this.``Create_OnlyDepartmentProvided_ShouldReturnProfileWithDepartmentOnly``() =
        let department = "マーケティング部"
        let profile = UserProfile.create null department null null

        Assert.True(Option.isNone profile.DisplayName)
        
        Assert.True(Option.isSome profile.Department)
        Assert.Equal(department, Option.get profile.Department)
        
        Assert.True(Option.isNone profile.PhoneNumber)
        Assert.True(Option.isNone profile.Notes)


type ValueNormalizationSimpleTests() =

    [<Fact>]
    member this.``Create_StringsWithLeadingTrailingWhitespace_ShouldTrimValues``() =
        let displayName = "  田中 三郎  "
        let department = " \t 人事部 \t "
        let profile = UserProfile.create displayName department null null

        Assert.True(Option.isSome profile.DisplayName)
        Assert.Equal("田中 三郎", Option.get profile.DisplayName)
        
        Assert.True(Option.isSome profile.Department)
        Assert.Equal("人事部", Option.get profile.Department)

    [<Fact>]
    member this.``Create_OnlyWhitespaceAfterTrim_ShouldReturnNoneValues``() =
        let displayName = "   "
        let department = "\t\t"
        let profile = UserProfile.create displayName department null null

        Assert.True(Option.isNone profile.DisplayName)
        Assert.True(Option.isNone profile.Department)
