using UbiquitousLanguageManager.Domain;
using Xunit;

namespace UbiquitousLanguageManager.Tests.Domain;

/// <summary>
/// Password Value Objectの単体テスト
/// 
/// 【テスト方針】
/// Phase A2で新規追加されたPassword Value Objectのパスワード強度バリデーション、
/// セキュリティ要件、エラーメッセージの妥当性を検証します。
/// </summary>
public class PasswordValueObjectTests
{
    /// <summary>
    /// 有効なパスワードのテスト
    /// </summary>
    public class ValidPasswordTests
    {
        [Theory]
        [InlineData("Password123!")]  // 標準的な強いパスワード
        [InlineData("MySecure1")]     // 最小要件を満たすパスワード
        [InlineData("Complex123A")]   // 複雑なパスワード
        [InlineData("TestPass1")]     // 基本的なパスワード
        [InlineData("Strong999Z")]    // 数字多めのパスワード
        public void Create_ValidPassword_ShouldReturnOk(string validPassword)
        {
            // Act
            var result = Password.create(validPassword);

            // Assert
            Assert.True(result.IsOk);
            Assert.Equal(validPassword, result.ResultValue.Value);
        }

        [Fact]
        public void Create_MinimumLengthPassword_ShouldReturnOk()
        {
            // Arrange: 8文字ちょうどで条件を満たすパスワード
            var password = "Pass123A";

            // Act
            var result = Password.create(password);

            // Assert
            Assert.True(result.IsOk);
            Assert.Equal(8, result.ResultValue.Value.Length);
        }

        [Fact]
        public void Create_MaximumLengthPassword_ShouldReturnOk()
        {
            // Arrange: 100文字のパスワード（最大長）
            var password = new string('A', 50) + new string('a', 40) + "123456789Z";
            Assert.Equal(100, password.Length);

            // Act
            var result = Password.create(password);

            // Assert
            Assert.True(result.IsOk);
            Assert.Equal(100, result.ResultValue.Value.Length);
        }
    }

    /// <summary>
    /// 無効なパスワード - 長さ制限のテスト
    /// </summary>
    public class LengthValidationTests
    {
        [Fact]
        public void Create_EmptyPassword_ShouldReturnError()
        {
            // Act
            var result = Password.create("");

            // Assert
            Assert.True(result.IsError);
            Assert.Equal("パスワードが入力されていません", result.ErrorValue);
        }

        [Fact]
        public void Create_NullPassword_ShouldReturnError()
        {
            // Act
            var result = Password.create(null!);

            // Assert
            Assert.True(result.IsError);
            Assert.Equal("パスワードが入力されていません", result.ErrorValue);
        }

        [Fact]
        public void Create_WhitespaceOnlyPassword_ShouldReturnError()
        {
            // Act
            var result = Password.create("   ");

            // Assert
            Assert.True(result.IsError);
            Assert.Equal("パスワードが入力されていません", result.ErrorValue);
        }

        [Theory]
        [InlineData("Pass1A")]     // 6文字
        [InlineData("Pwd1A")]      // 5文字
        [InlineData("Ab1")]        // 3文字
        [InlineData("A1a")]        // 3文字
        public void Create_TooShortPassword_ShouldReturnError(string shortPassword)
        {
            // Act
            var result = Password.create(shortPassword);

            // Assert
            Assert.True(result.IsError);
            Assert.Equal("パスワードは8文字以上で入力してください", result.ErrorValue);
        }

        [Fact]
        public void Create_TooLongPassword_ShouldReturnError()
        {
            // Arrange: 101文字のパスワード
            var password = new string('A', 50) + new string('a', 40) + "123456789Z" + "X";
            Assert.Equal(101, password.Length);

            // Act
            var result = Password.create(password);

            // Assert
            Assert.True(result.IsError);
            Assert.Equal("パスワードは100文字以内で入力してください", result.ErrorValue);
        }
    }

    /// <summary>
    /// パスワード強度バリデーションのテスト
    /// </summary>
    public class StrengthValidationTests
    {
        [Theory]
        [InlineData("password123")]  // 大文字なし
        [InlineData("mypassword1")]  // 大文字なし
        [InlineData("testpass123")]  // 大文字なし
        public void Create_NoUpperCase_ShouldReturnError(string passwordWithoutUpper)
        {
            // Act
            var result = Password.create(passwordWithoutUpper);

            // Assert
            Assert.True(result.IsError);
            Assert.Equal("パスワードには大文字を含めてください", result.ErrorValue);
        }

        [Theory]
        [InlineData("PASSWORD123")]  // 小文字なし
        [InlineData("MYPASSWORD1")]  // 小文字なし
        [InlineData("TESTPASS123")]  // 小文字なし
        public void Create_NoLowerCase_ShouldReturnError(string passwordWithoutLower)
        {
            // Act
            var result = Password.create(passwordWithoutLower);

            // Assert
            Assert.True(result.IsError);
            Assert.Equal("パスワードには小文字を含めてください", result.ErrorValue);
        }

        [Theory]
        [InlineData("PasswordABC")]  // 数字なし
        [InlineData("MyPasswordZ")]  // 数字なし
        [InlineData("TestPassXYZ")]  // 数字なし
        public void Create_NoDigit_ShouldReturnError(string passwordWithoutDigit)
        {
            // Act
            var result = Password.create(passwordWithoutDigit);

            // Assert
            Assert.True(result.IsError);
            Assert.Equal("パスワードには数字を含めてください", result.ErrorValue);
        }

        [Fact]
        public void Create_MultipleValidationFailures_ShouldReturnFirstError()
        {
            // Arrange: 大文字・小文字・数字すべてなし
            var password = "password";

            // Act
            var result = Password.create(password);

            // Assert
            Assert.True(result.IsError);
            // 最初に検出されるエラー（大文字なし）が返される
            Assert.Equal("パスワードには大文字を含めてください", result.ErrorValue);
        }
    }

    /// <summary>
    /// パスワード強度の境界値テスト
    /// </summary>
    public class BoundaryValueTests
    {
        [Fact]
        public void Create_ExactlyMinimumRequirements_ShouldReturnOk()
        {
            // Arrange: 最小要件を満たすパスワード（8文字、大文字・小文字・数字を各1文字）
            var password = "Abcdef12";

            // Act
            var result = Password.create(password);

            // Assert
            Assert.True(result.IsOk);
            Assert.Equal(password, result.ResultValue.Value);
        }

        [Fact]
        public void Create_JustBelowMinimumLength_ShouldReturnError()
        {
            // Arrange: 7文字（大文字・小文字・数字を含むが文字数不足）
            var password = "Abcde12";

            // Act
            var result = Password.create(password);

            // Assert
            Assert.True(result.IsError);
            Assert.Equal("パスワードは8文字以上で入力してください", result.ErrorValue);
        }

        [Fact]
        public void Create_ComplexValidPassword_ShouldReturnOk()
        {
            // Arrange: 複雑で強力なパスワード
            var password = "MyVerySecurePassword123WithSpecialChars!";

            // Act
            var result = Password.create(password);

            // Assert
            Assert.True(result.IsOk);
            Assert.Equal(password, result.ResultValue.Value);
        }
    }

    /// <summary>
    /// パスワードの特殊文字テスト
    /// </summary>
    public class SpecialCharacterTests
    {
        [Theory]
        [InlineData("Password123!")]  // 感嘆符
        [InlineData("Password123@")]  // アットマーク
        [InlineData("Password123#")]  // ハッシュ
        [InlineData("Password123$")]  // ドル記号
        [InlineData("Password123%")]  // パーセント
        [InlineData("Password123^")]  // キャレット
        [InlineData("Password123&")]  // アンパサンド
        [InlineData("Password123*")]  // アスタリスク
        public void Create_WithSpecialCharacters_ShouldReturnOk(string passwordWithSpecial)
        {
            // Act
            var result = Password.create(passwordWithSpecial);

            // Assert
            Assert.True(result.IsOk);
            Assert.Equal(passwordWithSpecial, result.ResultValue.Value);
        }

        [Fact]
        public void Create_UnicodeCharacters_ShouldReturnOk()
        {
            // Arrange: Unicode文字を含むパスワード
            var password = "パスワード123A";

            // Act
            var result = Password.create(password);

            // Assert
            Assert.True(result.IsOk);
            Assert.Equal(password, result.ResultValue.Value);
        }
    }

    /// <summary>
    /// Value Objectの不変性テスト
    /// </summary>
    public class ImmutabilityTests
    {
        [Fact]
        public void Value_PropertyAccess_ShouldReturnOriginalValue()
        {
            // Arrange
            var originalPassword = "SecurePass123";
            var result = Password.create(originalPassword);
            Assert.True(result.IsOk);
            var password = result.ResultValue;

            // Act
            var retrievedValue = password.Value;

            // Assert
            Assert.Equal(originalPassword, retrievedValue);
        }

        [Fact]
        public void Create_SamePassword_ShouldCreateIndependentInstances()
        {
            // Arrange
            var passwordText = "TestPassword123";

            // Act
            var result1 = Password.create(passwordText);
            var result2 = Password.create(passwordText);

            // Assert
            Assert.True(result1.IsOk);
            Assert.True(result2.IsOk);
            
            var password1 = result1.ResultValue;
            var password2 = result2.ResultValue;
            
            // 値は同じだが、異なるインスタンス
            Assert.Equal(password1.Value, password2.Value);
            Assert.NotSame(password1, password2);
        }
    }
}