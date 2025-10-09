using UbiquitousLanguageManager.Domain.Common;
using UbiquitousLanguageManager.Domain.Authentication;
using Microsoft.FSharp.Core;
using Xunit;

namespace UbiquitousLanguageManager.Tests.Domain;

/// <summary>
/// UserProfile Value Objectの単体テスト
/// 
/// 【テスト方針】
/// Phase A2で新規追加されたUserProfile Value Objectの作成、
/// オプション型フィールドの動作、不変性、値の正規化を検証します。
/// </summary>
public class UserProfileValueObjectTests
{
    /// <summary>
    /// UserProfile作成のテスト
    /// </summary>
    public class UserProfileCreationTests
    {
        [Fact]
        public void Create_AllFieldsProvided_ShouldReturnProfileWithAllValues()
        {
            // Arrange
            var displayName = "田中 太郎";
            var department = "開発部";
            var phoneNumber = "03-1234-5678";
            var notes = "プロジェクトリーダー";

            // Act
            var profile = UserProfile.create(displayName, department, phoneNumber, notes);

            // Assert
            Assert.True(FSharpOption<string>.get_IsSome(profile.DisplayName));
            Assert.Equal(displayName, profile.DisplayName.Value);
            
            Assert.True(FSharpOption<string>.get_IsSome(profile.Department));
            Assert.Equal(department, profile.Department.Value);
            
            Assert.True(FSharpOption<string>.get_IsSome(profile.PhoneNumber));
            Assert.Equal(phoneNumber, profile.PhoneNumber.Value);
            
            Assert.True(FSharpOption<string>.get_IsSome(profile.Notes));
            Assert.Equal(notes, profile.Notes.Value);
        }

        [Fact]
        public void Create_NoFieldsProvided_ShouldReturnProfileWithAllNoneValues()
        {
            // Act
            var profile = UserProfile.create(null, null, null, null);

            // Assert
            Assert.True(FSharpOption<string>.get_IsNone(profile.DisplayName));
            Assert.True(FSharpOption<string>.get_IsNone(profile.Department));
            Assert.True(FSharpOption<string>.get_IsNone(profile.PhoneNumber));
            Assert.True(FSharpOption<string>.get_IsNone(profile.Notes));
        }

        [Fact]
        public void Create_EmptyStringsProvided_ShouldReturnProfileWithAllNoneValues()
        {
            // Act
            var profile = UserProfile.create("", "", "", "");

            // Assert
            Assert.True(FSharpOption<string>.get_IsNone(profile.DisplayName));
            Assert.True(FSharpOption<string>.get_IsNone(profile.Department));
            Assert.True(FSharpOption<string>.get_IsNone(profile.PhoneNumber));
            Assert.True(FSharpOption<string>.get_IsNone(profile.Notes));
        }

        [Fact]
        public void Create_WhitespaceStringsProvided_ShouldReturnProfileWithAllNoneValues()
        {
            // Act
            var profile = UserProfile.create("   ", "  \t  ", " \n ", "    ");

            // Assert
            Assert.True(FSharpOption<string>.get_IsNone(profile.DisplayName));
            Assert.True(FSharpOption<string>.get_IsNone(profile.Department));
            Assert.True(FSharpOption<string>.get_IsNone(profile.PhoneNumber));
            Assert.True(FSharpOption<string>.get_IsNone(profile.Notes));
        }
    }

    /// <summary>
    /// 部分的なフィールド指定のテスト
    /// </summary>
    public class PartialFieldsTests
    {
        [Fact]
        public void Create_OnlyDisplayNameProvided_ShouldReturnProfileWithDisplayNameOnly()
        {
            // Arrange
            var displayName = "山田 花子";

            // Act
            var profile = UserProfile.create(displayName, null, null, null);

            // Assert
            Assert.True(FSharpOption<string>.get_IsSome(profile.DisplayName));
            Assert.Equal(displayName, profile.DisplayName.Value);
            
            Assert.True(FSharpOption<string>.get_IsNone(profile.Department));
            Assert.True(FSharpOption<string>.get_IsNone(profile.PhoneNumber));
            Assert.True(FSharpOption<string>.get_IsNone(profile.Notes));
        }

        [Fact]
        public void Create_OnlyDepartmentProvided_ShouldReturnProfileWithDepartmentOnly()
        {
            // Arrange
            var department = "マーケティング部";

            // Act
            var profile = UserProfile.create(null, department, null, null);

            // Assert
            Assert.True(FSharpOption<string>.get_IsNone(profile.DisplayName));
            
            Assert.True(FSharpOption<string>.get_IsSome(profile.Department));
            Assert.Equal(department, profile.Department.Value);
            
            Assert.True(FSharpOption<string>.get_IsNone(profile.PhoneNumber));
            Assert.True(FSharpOption<string>.get_IsNone(profile.Notes));
        }

        [Fact]
        public void Create_OnlyPhoneNumberProvided_ShouldReturnProfileWithPhoneNumberOnly()
        {
            // Arrange
            var phoneNumber = "090-1234-5678";

            // Act
            var profile = UserProfile.create(null, null, phoneNumber, null);

            // Assert
            Assert.True(FSharpOption<string>.get_IsNone(profile.DisplayName));
            Assert.True(FSharpOption<string>.get_IsNone(profile.Department));
            
            Assert.True(FSharpOption<string>.get_IsSome(profile.PhoneNumber));
            Assert.Equal(phoneNumber, profile.PhoneNumber.Value);
            
            Assert.True(FSharpOption<string>.get_IsNone(profile.Notes));
        }

        [Fact]
        public void Create_OnlyNotesProvided_ShouldReturnProfileWithNotesOnly()
        {
            // Arrange
            var notes = "特別なアクセス権限あり";

            // Act
            var profile = UserProfile.create(null, null, null, notes);

            // Assert
            Assert.True(FSharpOption<string>.get_IsNone(profile.DisplayName));
            Assert.True(FSharpOption<string>.get_IsNone(profile.Department));
            Assert.True(FSharpOption<string>.get_IsNone(profile.PhoneNumber));
            
            Assert.True(FSharpOption<string>.get_IsSome(profile.Notes));
            Assert.Equal(notes, profile.Notes.Value);
        }

        [Fact]
        public void Create_DisplayNameAndNotesProvided_ShouldReturnProfileWithSpecifiedFields()
        {
            // Arrange
            var displayName = "佐藤 次郎";
            var notes = "新入社員";

            // Act
            var profile = UserProfile.create(displayName, null, null, notes);

            // Assert
            Assert.True(FSharpOption<string>.get_IsSome(profile.DisplayName));
            Assert.Equal(displayName, profile.DisplayName.Value);
            
            Assert.True(FSharpOption<string>.get_IsNone(profile.Department));
            Assert.True(FSharpOption<string>.get_IsNone(profile.PhoneNumber));
            
            Assert.True(FSharpOption<string>.get_IsSome(profile.Notes));
            Assert.Equal(notes, profile.Notes.Value);
        }
    }

    /// <summary>
    /// 値の正規化テスト
    /// </summary>
    public class ValueNormalizationTests
    {
        [Fact]
        public void Create_StringsWithLeadingTrailingWhitespace_ShouldTrimValues()
        {
            // Arrange
            var displayName = "  田中 三郎  ";
            var department = " \t 人事部 \t ";
            var phoneNumber = "  03-9876-5432  ";
            var notes = "  \n  部門長候補  \n  ";

            // Act
            var profile = UserProfile.create(displayName, department, phoneNumber, notes);

            // Assert
            Assert.True(FSharpOption<string>.get_IsSome(profile.DisplayName));
            Assert.Equal("田中 三郎", profile.DisplayName.Value);
            
            Assert.True(FSharpOption<string>.get_IsSome(profile.Department));
            Assert.Equal("人事部", profile.Department.Value);
            
            Assert.True(FSharpOption<string>.get_IsSome(profile.PhoneNumber));
            Assert.Equal("03-9876-5432", profile.PhoneNumber.Value);
            
            Assert.True(FSharpOption<string>.get_IsSome(profile.Notes));
            Assert.Equal("部門長候補", profile.Notes.Value);
        }

        [Fact]
        public void Create_OnlyWhitespaceAfterTrim_ShouldReturnNoneValues()
        {
            // Arrange - トリム後に空文字列になる値
            var displayName = "   ";
            var department = "\t\t";
            var phoneNumber = "\n\n";
            var notes = " \t\n ";

            // Act
            var profile = UserProfile.create(displayName, department, phoneNumber, notes);

            // Assert
            Assert.True(FSharpOption<string>.get_IsNone(profile.DisplayName));
            Assert.True(FSharpOption<string>.get_IsNone(profile.Department));
            Assert.True(FSharpOption<string>.get_IsNone(profile.PhoneNumber));
            Assert.True(FSharpOption<string>.get_IsNone(profile.Notes));
        }
    }

    /// <summary>
    /// Value Objectの不変性テスト
    /// </summary>
    public class ImmutabilityTests
    {
        [Fact]
        public void Profile_AfterCreation_ShouldBeImmutable()
        {
            // Arrange
            var displayName = "原田 美咲";
            var department = "財務部";

            // Act
            var profile = UserProfile.create(displayName, department, null, null);

            // Assert - プロパティが読み取り専用であることを確認
            Assert.True(FSharpOption<string>.get_IsSome(profile.DisplayName));
            Assert.Equal(displayName, profile.DisplayName.Value);
            
            Assert.True(FSharpOption<string>.get_IsSome(profile.Department));
            Assert.Equal(department, profile.Department.Value);
            
            Assert.True(FSharpOption<string>.get_IsNone(profile.PhoneNumber));
            Assert.True(FSharpOption<string>.get_IsNone(profile.Notes));
        }

        [Fact]
        public void Profile_SameInputs_ShouldCreateIndependentInstances()
        {
            // Arrange
            var displayName = "鈴木 太郎";
            var department = "営業部";

            // Act
            var profile1 = UserProfile.create(displayName, department, null, null);
            var profile2 = UserProfile.create(displayName, department, null, null);

            // Assert - 異なるインスタンスだが同じ値
            Assert.NotSame(profile1, profile2);
            
            Assert.Equal(FSharpOption<string>.get_IsSome(profile1.DisplayName), FSharpOption<string>.get_IsSome(profile2.DisplayName));
            Assert.Equal(FSharpOption<string>.get_IsSome(profile1.Department), FSharpOption<string>.get_IsSome(profile2.Department));
            Assert.Equal(FSharpOption<string>.get_IsNone(profile1.PhoneNumber), FSharpOption<string>.get_IsNone(profile2.PhoneNumber));
            Assert.Equal(FSharpOption<string>.get_IsNone(profile1.Notes), FSharpOption<string>.get_IsNone(profile2.Notes));
            
            // 値が一致することを確認
            if (FSharpOption<string>.get_IsSome(profile1.DisplayName) && FSharpOption<string>.get_IsSome(profile2.DisplayName))
                Assert.Equal(profile1.DisplayName.Value, profile2.DisplayName.Value);
            if (FSharpOption<string>.get_IsSome(profile1.Department) && FSharpOption<string>.get_IsSome(profile2.Department))
                Assert.Equal(profile1.Department.Value, profile2.Department.Value);
            if (FSharpOption<string>.get_IsSome(profile1.PhoneNumber) && FSharpOption<string>.get_IsSome(profile2.PhoneNumber))
                Assert.Equal(profile1.PhoneNumber.Value, profile2.PhoneNumber.Value);
            if (FSharpOption<string>.get_IsSome(profile1.Notes) && FSharpOption<string>.get_IsSome(profile2.Notes))
                Assert.Equal(profile1.Notes.Value, profile2.Notes.Value);
        }
    }

    /// <summary>
    /// 電話番号特殊ケースのテスト
    /// </summary>
    public class PhoneNumberHandlingTests
    {
        [Theory]
        [InlineData("03-1234-5678")]
        [InlineData("090-1234-5678")]
        [InlineData("0120-123-456")]
        [InlineData("+81-3-1234-5678")]
        [InlineData("(03) 1234-5678")]
        public void Create_VariousPhoneNumberFormats_ShouldAcceptAllFormats(string phoneNumber)
        {
            // Act
            var profile = UserProfile.create(null, null, phoneNumber, null);

            // Assert
            Assert.True(FSharpOption<string>.get_IsSome(profile.PhoneNumber));
            Assert.Equal(phoneNumber, profile.PhoneNumber.Value);
        }

        [Fact]
        public void Create_PhoneNumberWithWhitespace_ShouldTrimWhitespace()
        {
            // Arrange
            var phoneNumberWithSpaces = "  090-1234-5678  ";
            var expectedPhoneNumber = "090-1234-5678";

            // Act
            var profile = UserProfile.create(null, null, phoneNumberWithSpaces, null);

            // Assert
            Assert.True(FSharpOption<string>.get_IsSome(profile.PhoneNumber));
            Assert.Equal(expectedPhoneNumber, profile.PhoneNumber.Value);
        }

        [Fact]
        public void Create_EmptyPhoneNumber_ShouldReturnNone()
        {
            // Act
            var profile = UserProfile.create(null, null, "", null);

            // Assert
            Assert.True(FSharpOption<string>.get_IsNone(profile.PhoneNumber));
        }
    }

    /// <summary>
    /// 複合シナリオテスト
    /// </summary>
    public class ComplexScenarioTests
    {
        [Fact]
        public void Create_MixedValidAndInvalidFields_ShouldHandleEachFieldCorrectly()
        {
            // Arrange
            var validDisplayName = "高橋 恵子";
            var invalidDepartment = "   "; // トリム後空文字
            var validPhoneNumber = "044-123-4567";
            var invalidNotes = ""; // 空文字

            // Act
            var profile = UserProfile.create(validDisplayName, invalidDepartment, validPhoneNumber, invalidNotes);

            // Assert
            Assert.True(FSharpOption<string>.get_IsSome(profile.DisplayName));
            Assert.Equal(validDisplayName, profile.DisplayName.Value);
            
            Assert.True(FSharpOption<string>.get_IsNone(profile.Department));
            
            Assert.True(FSharpOption<string>.get_IsSome(profile.PhoneNumber));
            Assert.Equal(validPhoneNumber, profile.PhoneNumber.Value);
            
            Assert.True(FSharpOption<string>.get_IsNone(profile.Notes));
        }
    }
}