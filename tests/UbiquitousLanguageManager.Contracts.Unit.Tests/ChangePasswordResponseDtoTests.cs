using UbiquitousLanguageManager.Contracts.DTOs.Authentication;
using Xunit;

namespace UbiquitousLanguageManager.Contracts.Unit.Tests;

/// <summary>
/// ChangePasswordResponseDtoの単体テスト
/// 
/// 【テスト方針】
/// Phase A2で新規追加されたChangePasswordResponseDtoの作成メソッド、
/// プロパティ設定、レスポンス形式の妥当性を検証します。
/// </summary>
public class ChangePasswordResponseDtoTests
{
    /// <summary>
    /// 成功レスポンス作成のテスト
    /// </summary>
    public class SuccessResponseTests
    {
        [Fact]
        public void Success_WithDefaultMessage_ShouldReturnSuccessResponse()
        {
            // Act
            var response = ChangePasswordResponseDto.Success();

            // Assert
            Assert.True(response.IsSuccess);
            Assert.Equal("パスワードが正常に変更されました。", response.Message);
            Assert.Null(response.ErrorMessage);
        }

        [Fact]
        public void Success_WithCustomMessage_ShouldReturnSuccessResponseWithCustomMessage()
        {
            // Arrange
            var customMessage = "パスワードの変更が完了しました！";

            // Act
            var response = ChangePasswordResponseDto.Success(customMessage);

            // Assert
            Assert.True(response.IsSuccess);
            Assert.Equal(customMessage, response.Message);
            Assert.Null(response.ErrorMessage);
        }

        [Fact]
        public void Success_WithNullMessage_ShouldReturnSuccessResponseWithNullMessage()
        {
            // Act
            var response = ChangePasswordResponseDto.Success(null);

            // Assert
            Assert.True(response.IsSuccess);
            Assert.Null(response.Message);
            Assert.Null(response.ErrorMessage);
        }

        [Fact]
        public void Success_WithEmptyMessage_ShouldReturnSuccessResponseWithEmptyMessage()
        {
            // Act
            var response = ChangePasswordResponseDto.Success("");

            // Assert
            Assert.True(response.IsSuccess);
            Assert.Equal("", response.Message);
            Assert.Null(response.ErrorMessage);
        }

        [Theory]
        [InlineData("パスワードが正常に変更されました。")]
        [InlineData("Password changed successfully.")]
        [InlineData("変更完了")]
        [InlineData("Success! Your password has been updated.")]
        public void Success_WithVariousMessages_ShouldReturnCorrectResponse(string message)
        {
            // Act
            var response = ChangePasswordResponseDto.Success(message);

            // Assert
            Assert.True(response.IsSuccess);
            Assert.Equal(message, response.Message);
            Assert.Null(response.ErrorMessage);
        }
    }

    /// <summary>
    /// エラーレスポンス作成のテスト
    /// </summary>
    public class ErrorResponseTests
    {
        [Fact]
        public void Error_WithErrorMessage_ShouldReturnErrorResponse()
        {
            // Arrange
            var errorMessage = "現在のパスワードが正しくありません";

            // Act
            var response = ChangePasswordResponseDto.Error(errorMessage);

            // Assert
            Assert.False(response.IsSuccess);
            Assert.Equal(errorMessage, response.Message);
            Assert.Equal(errorMessage, response.ErrorMessage);
        }

        [Fact]
        public void Error_WithNullMessage_ShouldReturnErrorResponseWithNullMessages()
        {
            // Act
            var response = ChangePasswordResponseDto.Error(null!);

            // Assert
            Assert.False(response.IsSuccess);
            Assert.Null(response.Message);
            Assert.Null(response.ErrorMessage);
        }

        [Fact]
        public void Error_WithEmptyMessage_ShouldReturnErrorResponseWithEmptyMessages()
        {
            // Act
            var response = ChangePasswordResponseDto.Error("");

            // Assert
            Assert.False(response.IsSuccess);
            Assert.Equal("", response.Message);
            Assert.Equal("", response.ErrorMessage);
        }

        [Theory]
        [InlineData("現在のパスワードが正しくありません")]
        [InlineData("パスワードは8文字以上で入力してください")]
        [InlineData("新しいパスワードと確認パスワードが一致しません")]
        [InlineData("パスワードには大文字、小文字、数字を含めてください")]
        [InlineData("データベースエラーが発生しました")]
        public void Error_WithVariousErrorMessages_ShouldReturnCorrectResponse(string errorMessage)
        {
            // Act
            var response = ChangePasswordResponseDto.Error(errorMessage);

            // Assert
            Assert.False(response.IsSuccess);
            Assert.Equal(errorMessage, response.Message);
            Assert.Equal(errorMessage, response.ErrorMessage);
        }
    }

    /// <summary>
    /// プロパティ設定のテスト
    /// </summary>
    public class PropertySetTests
    {
        [Fact]
        public void Properties_CanBeSetDirectly_ShouldRetainValues()
        {
            // Arrange
            var response = new ChangePasswordResponseDto();

            // Act
            response.IsSuccess = true;
            response.Message = "テストメッセージ";
            response.ErrorMessage = "テストエラー";

            // Assert
            Assert.True(response.IsSuccess);
            Assert.Equal("テストメッセージ", response.Message);
            Assert.Equal("テストエラー", response.ErrorMessage);
        }

        [Fact]
        public void Properties_DefaultValues_ShouldBeCorrect()
        {
            // Act
            var response = new ChangePasswordResponseDto();

            // Assert
            Assert.False(response.IsSuccess); // bool default is false
            Assert.Null(response.Message);
            Assert.Null(response.ErrorMessage);
        }

        [Fact]
        public void Properties_CanHandleNullValues_ShouldNotThrowExceptions()
        {
            // Arrange
            var response = new ChangePasswordResponseDto();

            // Act & Assert (no exceptions should be thrown)
            response.Message = null;
            response.ErrorMessage = null;
            
            Assert.Null(response.Message);
            Assert.Null(response.ErrorMessage);
        }
    }

    /// <summary>
    /// 互換性テスト（ErrorMessageプロパティの互換性確認）
    /// </summary>
    public class CompatibilityTests
    {
        [Fact]
        public void Error_Response_ShouldHaveBothMessageAndErrorMessage()
        {
            // Arrange
            var errorMessage = "パスワード変更エラー";

            // Act
            var response = ChangePasswordResponseDto.Error(errorMessage);

            // Assert
            // Messageプロパティとしてもアクセス可能
            Assert.Equal(errorMessage, response.Message);
            
            // 互換性のためのErrorMessageプロパティとしてもアクセス可能
            Assert.Equal(errorMessage, response.ErrorMessage);
            
            // 両方のプロパティが同じ値を持つ
            Assert.Equal(response.Message, response.ErrorMessage);
        }

        [Fact]
        public void Success_Response_ShouldHaveMessageButNotErrorMessage()
        {
            // Arrange
            var successMessage = "パスワード変更成功";

            // Act
            var response = ChangePasswordResponseDto.Success(successMessage);

            // Assert
            Assert.Equal(successMessage, response.Message);
            Assert.Null(response.ErrorMessage); // エラーメッセージは設定されない
        }

        [Fact]
        public void Response_CanBeUsedWithOldErrorMessageProperty()
        {
            // Arrange - 古いコードがErrorMessageプロパティを使用する場合
            var errorMessage = "旧コード用エラーメッセージ";
            var response = ChangePasswordResponseDto.Error(errorMessage);

            // Act - 古いコードスタイルでの使用
            if (!response.IsSuccess && !string.IsNullOrEmpty(response.ErrorMessage))
            {
                var displayMessage = response.ErrorMessage;
                
                // Assert
                Assert.Equal(errorMessage, displayMessage);
            }
            else
            {
                Assert.Fail("エラーレスポンスのErrorMessageが期待通りに設定されていません");
            }
        }

        [Fact]
        public void Response_CanBeUsedWithNewMessageProperty()
        {
            // Arrange - 新しいコードがMessageプロパティを使用する場合
            var errorMessage = "新コード用エラーメッセージ";
            var response = ChangePasswordResponseDto.Error(errorMessage);

            // Act - 新しいコードスタイルでの使用
            if (!response.IsSuccess && !string.IsNullOrEmpty(response.Message))
            {
                var displayMessage = response.Message;
                
                // Assert
                Assert.Equal(errorMessage, displayMessage);
            }
            else
            {
                Assert.Fail("エラーレスポンスのMessageが期待通りに設定されていません");
            }
        }
    }

    /// <summary>
    /// 実用的なシナリオテスト
    /// </summary>
    public class PracticalScenarioTests
    {
        [Fact]
        public void CreateResponseChain_SuccessAndError_ShouldWorkCorrectly()
        {
            // Arrange & Act
            var successResponse = ChangePasswordResponseDto.Success("パスワード変更完了");
            var errorResponse = ChangePasswordResponseDto.Error("バリデーションエラー");

            // Assert
            Assert.True(successResponse.IsSuccess);
            Assert.False(errorResponse.IsSuccess);
            
            Assert.NotNull(successResponse.Message);
            Assert.NotNull(errorResponse.Message);
            Assert.NotNull(errorResponse.ErrorMessage);
            
            Assert.Null(successResponse.ErrorMessage);
        }

        [Fact]
        public void Response_CanBeUsedInControllerPattern()
        {
            // Arrange - コントローラーでの使用パターンをシミュレート
            var responses = new List<ChangePasswordResponseDto>
            {
                ChangePasswordResponseDto.Success(),
                ChangePasswordResponseDto.Error("無効なパスワード"),
                ChangePasswordResponseDto.Success("パスワード更新完了"),
                ChangePasswordResponseDto.Error("データベース接続エラー")
            };

            // Act & Assert
            var successCount = responses.Count(r => r.IsSuccess);
            var errorCount = responses.Count(r => !r.IsSuccess);

            Assert.Equal(2, successCount);
            Assert.Equal(2, errorCount);
            
            // 全てのエラーレスポンスにはメッセージが設定されている
            var errorResponses = responses.Where(r => !r.IsSuccess);
            Assert.All(errorResponses, r => Assert.NotNull(r.Message));
            Assert.All(errorResponses, r => Assert.NotNull(r.ErrorMessage));
        }
    }
}