using Microsoft.FSharp.Core;
using UbiquitousLanguageManager.Contracts.Converters;
using UbiquitousLanguageManager.Domain.Common;
using UbiquitousLanguageManager.Domain.Authentication;
using UbiquitousLanguageManager.Domain.ProjectManagement;
using Xunit;

namespace UbiquitousLanguageManager.Contracts.Unit.Tests;

/// <summary>
/// TypeConvertersの拡張機能テスト
/// 
/// 【テスト方針】
/// Phase A2で拡張されたTypeConvertersの新機能、
/// F#のValue Objects・Option型・判別共用体のC# DTOへの変換、
/// 型安全性とnull安全性を検証します。
/// </summary>
public class TypeConvertersExtensionsTests
{
    private Role ConvertIntToRole(int roleInt)
    {
        return roleInt switch
        {
            0 => Role.GeneralUser,
            1 => Role.DomainApprover,
            2 => Role.ProjectManager,
            3 => Role.SuperUser,
            _ => Role.GeneralUser
        };
    }

    private User CreateTestUser(string email = "test@example.com", string name = "テストユーザー", Role? role = null, string id = "00000000-0000-0000-0000-000000000001")
    {
        var emailResult = Email.create(email);
        var nameResult = UserName.create(name);

        Assert.True(emailResult.IsOk);
        Assert.True(nameResult.IsOk);

        return User.createWithId(
            emailResult.ResultValue,
            nameResult.ResultValue,
            role ?? Role.GeneralUser,
            UserId.create(id)
        );
    }

    /// <summary>
    /// UserProfile変換のテスト
    /// </summary>
    public class UserProfileConversionTests : TypeConvertersExtensionsTests
    {
        [Fact]
        public void ToDto_UserProfileWithAllFields_ShouldConvertCorrectly()
        {
            // Arrange
            var profile = UserProfile.create(
                "田中 太郎",
                "開発部",
                "03-1234-5678",
                "プロジェクトリーダー"
            );

            // Act
            var dto = TypeConverters.ToDto(profile);

            // Assert
            Assert.Equal("田中 太郎", dto.DisplayName);
            Assert.Equal("開発部", dto.Department);
            Assert.Equal("03-1234-5678", dto.PhoneNumber);
            Assert.Equal("プロジェクトリーダー", dto.Notes);
        }

        [Fact]
        public void ToDto_UserProfileWithSomeFields_ShouldConvertWithNulls()
        {
            // Arrange
            var profile = UserProfile.create(
                "佐藤 花子",
                null, // Department is null
                "090-1234-5678",
                null  // Notes is null
            );

            // Act
            var dto = TypeConverters.ToDto(profile);

            // Assert
            Assert.Equal("佐藤 花子", dto.DisplayName);
            Assert.Null(dto.Department);  // F# None → C# null
            Assert.Equal("090-1234-5678", dto.PhoneNumber);
            Assert.Null(dto.Notes);       // F# None → C# null
        }

        [Fact]
        public void ToDto_EmptyUserProfile_ShouldConvertWithAllNulls()
        {
            // Arrange
            var profile = UserProfile.empty;

            // Act
            var dto = TypeConverters.ToDto(profile);

            // Assert
            Assert.Null(dto.DisplayName);
            Assert.Null(dto.Department);
            Assert.Null(dto.PhoneNumber);
            Assert.Null(dto.Notes);
        }

        [Fact]
        public void ToDto_UserProfileWithWhitespaceValues_ShouldTrimAndConvert()
        {
            // Arrange
            var profile = UserProfile.create(
                "  山田 次郎  ",
                "\t営業部\t",
                "  080-1234-5678  ",
                "\n  備考情報  \n"
            );

            // Act
            var dto = TypeConverters.ToDto(profile);

            // Assert
            Assert.Equal("山田 次郎", dto.DisplayName);
            Assert.Equal("営業部", dto.Department);
            Assert.Equal("080-1234-5678", dto.PhoneNumber);
            Assert.Equal("備考情報", dto.Notes);
        }
    }

    /// <summary>
    /// 拡張されたUser変換のテスト（Phase A2新機能）
    /// </summary>
    public class ExtendedUserConversionTests : TypeConvertersExtensionsTests
    {
        [Fact]
        public void ToDto_UserWithCompleteProfile_ShouldConvertAllFields()
        {
            // Arrange
            var user = CreateTestUser("user@example.com", "ユーザー", Role.ProjectManager, "00000000-0000-0000-0000-000000000001");

            // Act
            var dto = TypeConverters.ToDto(user);

            // Assert
            Assert.Equal("00000000-0000-0000-0000-000000000001", dto.Id);
            Assert.Equal("user@example.com", dto.Email);
            Assert.Equal("ユーザー", dto.Name);
            Assert.Equal("ProjectManager", dto.Role);
            Assert.True(dto.IsActive);
            Assert.True(dto.IsFirstLogin);
            
            // プロフィール情報の確認
            Assert.NotNull(dto.Profile);
            
            // 監査情報の確認
            Assert.True(dto.CreatedAt <= DateTime.UtcNow);
            Assert.Equal("00000000-0000-0000-0000-000000000001", dto.CreatedBy);
            Assert.True(dto.UpdatedAt <= DateTime.UtcNow);
            Assert.Equal("00000000-0000-0000-0000-000000000001", dto.UpdatedBy);
        }

        [Theory]
        [InlineData(3, "SuperUser")]
        [InlineData(2, "ProjectManager")]
        [InlineData(1, "DomainApprover")]
        [InlineData(0, "GeneralUser")]
        public void ToDto_UserWithDifferentRoles_ShouldConvertRoleCorrectly(int roleInt, string expectedRoleString)
        {
            // Arrange
            var role = ConvertIntToRole(roleInt);
            var user = CreateTestUser("user@example.com", "ユーザー", role);

            // Act
            var dto = TypeConverters.ToDto(user);

            // Assert
            Assert.Equal(expectedRoleString, dto.Role);
        }

        [Fact]
        public void ToDto_UserWithPhoneNumber_ShouldConvertPhoneNumberCorrectly()
        {
            // Arrange
            var user = CreateTestUser("user@example.com", "ユーザー", Role.GeneralUser);
            // 注意: このテストはUserエンティティにPhoneNumberプロパティが実装されている前提

            // Act
            var dto = TypeConverters.ToDto(user);

            // Assert
            // F# Option型からC# nullable型への変換テスト
            // 実際のPhoneNumberプロパティの実装に依存
            if (user.PhoneNumber != null && FSharpOption<string>.get_IsSome(user.PhoneNumber))
            {
                Assert.Equal(user.PhoneNumber.Value, dto.PhoneNumber);
            }
            else
            {
                Assert.Null(dto.PhoneNumber);
            }
        }

        [Fact]
        public void ToDto_UserWithLockoutEnd_ShouldConvertLockoutEndCorrectly()
        {
            // Arrange
            var user = CreateTestUser("user@example.com", "ユーザー", Role.GeneralUser);
            // 注意: このテストはUserエンティティにLockoutEndプロパティが実装されている前提

            // Act
            var dto = TypeConverters.ToDto(user);

            // Assert
            // F# Option<DateTime>からC# DateTime?への変換テスト
            if (user.LockoutEnd != null && FSharpOption<DateTime>.get_IsSome(user.LockoutEnd))
            {
                Assert.Equal(user.LockoutEnd.Value, dto.LockoutEnd);
            }
            else
            {
                Assert.Null(dto.LockoutEnd);
            }
        }
    }

    /// <summary>
    /// F# Option型からC# nullable型への変換テスト
    /// </summary>
    public class OptionTypeConversionTests : TypeConvertersExtensionsTests
    {
        [Fact]
        public void OptionTypeConversion_SomeValue_ShouldConvertToValue()
        {
            // Arrange
            var someStringOption = FSharpOption<string>.Some("テスト値");

            // Act & Assert
            if (FSharpOption<string>.get_IsSome(someStringOption))
            {
                var value = someStringOption.Value;
                Assert.Equal("テスト値", value);
            }
            else
            {
                Assert.Fail("Some値が正しく変換されませんでした");
            }
        }

        [Fact]
        public void OptionTypeConversion_NoneValue_ShouldConvertToNull()
        {
            // Arrange
            var noneStringOption = FSharpOption<string>.None;

            // Act & Assert
            Assert.True(FSharpOption<string>.get_IsNone(noneStringOption));
            Assert.False(FSharpOption<string>.get_IsSome(noneStringOption));
        }

        [Fact]
        public void OptionTypeConversion_DateTimeOption_ShouldHandleCorrectly()
        {
            // Arrange
            var someDateOption = FSharpOption<DateTime>.Some(new DateTime(2023, 12, 25));
            var noneDateOption = FSharpOption<DateTime>.None;

            // Act & Assert - Some値
            if (FSharpOption<DateTime>.get_IsSome(someDateOption))
            {
                var dateValue = someDateOption.Value;
                Assert.Equal(new DateTime(2023, 12, 25), dateValue);
            }

            // Act & Assert - None値
            Assert.True(FSharpOption<DateTime>.get_IsNone(noneDateOption));
        }
    }

    /// <summary>
    /// エラー処理とエッジケースのテスト
    /// </summary>
    public class ErrorHandlingTests : TypeConvertersExtensionsTests
    {
        [Fact]
        public void ToDto_NullUser_ShouldThrowArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => TypeConverters.ToDto((User)null!));
        }

        [Fact]
        public void ToDto_NullUserProfile_ShouldThrowArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => TypeConverters.ToDto((UserProfile)null!));
        }

        [Fact]
        public void ToDto_UserWithMinimumValidData_ShouldConvertSuccessfully()
        {
            // Arrange
            var user = CreateTestUser("min@example.com", "最小ユーザー", Role.GeneralUser, "00000000-0000-0000-0000-000000000001");

            // Act
            var dto = TypeConverters.ToDto(user);

            // Assert
            Assert.Equal("00000000-0000-0000-0000-000000000001", dto.Id);
            Assert.Equal("min@example.com", dto.Email);
            Assert.Equal("最小ユーザー", dto.Name);
            Assert.Equal("GeneralUser", dto.Role);
            Assert.NotNull(dto.Profile); // 空のプロフィールでも null ではない
        }

        [Fact]
        public void ToDto_UserWithUnicodeCharacters_ShouldConvertCorrectly()
        {
            // Arrange
            var user = CreateTestUser("unicode@例え.com", "田中太郎🌟", Role.GeneralUser, "00000000-0000-0000-0000-000000000001");

            // Act
            var dto = TypeConverters.ToDto(user);

            // Assert
            Assert.Equal("unicode@例え.com", dto.Email);
            Assert.Equal("田中太郎🌟", dto.Name);
        }
    }

    /// <summary>
    /// パフォーマンステスト
    /// </summary>
    public class PerformanceTests : TypeConvertersExtensionsTests
    {
        [Fact]
        public void ToDto_MultipleUsers_ShouldConvertEfficiently()
        {
            // Arrange
            var users = Enumerable.Range(1, 100).Select(i =>
                CreateTestUser($"user{i}@example.com", $"ユーザー{i}", Role.GeneralUser, $"00000000-0000-0000-0000-{i:D12}")
            ).ToList();

            // Act
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            var dtos = users.Select(TypeConverters.ToDto).ToList();
            stopwatch.Stop();

            // Assert
            Assert.Equal(100, dtos.Count);
            Assert.True(stopwatch.ElapsedMilliseconds < 1000, "100ユーザーの変換が1秒以内に完了する必要があります");
            
            // 全てのDTOが正しく変換されていることを確認
            for (int i = 0; i < 100; i++)
            {
                Assert.Equal($"user{i + 1}@example.com", dtos[i].Email);
                Assert.Equal($"ユーザー{i + 1}", dtos[i].Name);
            }
        }

        [Fact]
        public void ToDto_UserProfile_MultipleConversions_ShouldBeEfficient()
        {
            // Arrange
            var profiles = Enumerable.Range(1, 50).Select(i =>
                UserProfile.create($"ユーザー{i}", $"部署{i}", $"080-1234-{i:D4}", $"備考{i}")
            ).ToList();

            // Act
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            var dtos = profiles.Select(TypeConverters.ToDto).ToList();
            stopwatch.Stop();

            // Assert
            Assert.Equal(50, dtos.Count);
            Assert.True(stopwatch.ElapsedMilliseconds < 500, "50プロフィールの変換が500ms以内に完了する必要があります");
            
            // 変換結果の確認
            for (int i = 0; i < 50; i++)
            {
                Assert.Equal($"ユーザー{i + 1}", dtos[i].DisplayName);
                Assert.Equal($"部署{i + 1}", dtos[i].Department);
            }
        }
    }
}