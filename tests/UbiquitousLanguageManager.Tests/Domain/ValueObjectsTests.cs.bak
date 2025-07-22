using UbiquitousLanguageManager.Domain;
using Xunit;

namespace UbiquitousLanguageManager.Tests.Domain;

/// <summary>
/// Value Objectsの単体テスト
/// 
/// 【テスト方針】
/// F#のValue Objectsが正しく動作することを確認します。
/// 特にスマートコンストラクタによる検証ロジックをテストします。
/// </summary>
public class ValueObjectsTests
{
    /// <summary>
    /// Email Value Object のテスト
    /// </summary>
    public class EmailTests
    {
        [Fact]
        public void Email_ValidFormat_ShouldCreateSuccessfully()
        {
            // Arrange
            var validEmail = "test@example.com";

            // Act
            var result = Email.create(validEmail);

            // Assert
            Assert.True(result.IsOk);
        }

        [Theory]
        [InlineData("")]
        [InlineData("invalid-email")]
        [InlineData("@example.com")]
        [InlineData("test@")]
        [InlineData("test.example.com")]
        public void Email_InvalidFormat_ShouldReturnError(string invalidEmail)
        {
            // Arrange & Act
            var result = Email.create(invalidEmail);

            // Assert
            Assert.True(result.IsError);
        }

        [Fact]
        public void Email_TooLong_ShouldReturnError()
        {
            // Arrange
            var longEmail = new string('a', 250) + "@example.com";

            // Act
            var result = Email.create(longEmail);

            // Assert
            Assert.True(result.IsError);
        }
    }

    /// <summary>
    /// UserName Value Object のテスト
    /// </summary>
    public class UserNameTests
    {
        [Fact]
        public void UserName_ValidName_ShouldCreateSuccessfully()
        {
            // Arrange
            var validName = "田中太郎";

            // Act
            var result = UserName.create(validName);

            // Assert
            Assert.True(result.IsOk);
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        public void UserName_EmptyOrWhitespace_ShouldReturnError(string invalidName)
        {
            // Arrange & Act
            var result = UserName.create(invalidName);

            // Assert
            Assert.True(result.IsError);
        }

        [Fact]
        public void UserName_TooLong_ShouldReturnError()
        {
            // Arrange
            var longName = new string('あ', 51);

            // Act
            var result = UserName.create(longName);

            // Assert
            Assert.True(result.IsError);
        }
    }

    /// <summary>
    /// PasswordHash Value Object のテスト
    /// </summary>
    public class PasswordHashTests
    {
        [Fact]
        public void PasswordHash_ValidHash_ShouldCreateSuccessfully()
        {
            // Arrange
            var validHash = "$2a$11$example.hash.string.for.testing";

            // Act
            var result = PasswordHash.create(validHash);

            // Assert
            Assert.True(result.IsOk);
        }

        [Fact]
        public void UserName_Null_ShouldReturnError()
        {
            // Arrange & Act
            var result = UserName.create(null);

            // Assert
            Assert.True(result.IsError);
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        public void PasswordHash_EmptyOrWhitespace_ShouldReturnError(string invalidHash)
        {
            // Arrange & Act
            var result = PasswordHash.create(invalidHash);

            // Assert
            Assert.True(result.IsError);
        }

        [Fact]
        public void PasswordHash_Null_ShouldReturnError()
        {
            // Arrange & Act
            var result = PasswordHash.create(null);

            // Assert
            Assert.True(result.IsError);
        }
    }
}