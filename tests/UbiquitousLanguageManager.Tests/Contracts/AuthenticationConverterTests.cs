using System;
using Xunit;
using Microsoft.FSharp.Core;
using UbiquitousLanguageManager.Contracts.Converters;
using UbiquitousLanguageManager.Contracts.DTOs;
using UbiquitousLanguageManager.Domain;

namespace UbiquitousLanguageManager.Tests.Contracts;

/// <summary>
/// Phase A9: AuthenticationConverter テストクラス
/// F#↔C#境界TypeConverter拡張の完全テスト
/// Railway-oriented Programming・Result型・判別共用体変換の検証
/// 【テスト観点】
/// - 双方向変換の正確性
/// - 型安全性の保証
/// - エラーハンドリングの網羅性
/// - 境界値・null処理の安全性
/// </summary>
public class AuthenticationConverterTests
{
    // =================================================================
    // 🧪 F# → C# 変換テスト（Success Cases）
    // =================================================================

    [Fact]
    public void ToDto_SuccessfulAuthenticationResult_ShouldReturnSuccessDto()
    {
        // Arrange: F#の成功認証結果を準備
        var user = CreateTestUser();
        var successResult = FSharpResult<User, AuthenticationError>.NewOk(user);

        // Act: F# → C# 変換実行
        var resultDto = AuthenticationConverter.ToDto(successResult);

        // Assert: 成功結果の検証
        Assert.True(resultDto.IsSuccess);
        Assert.NotNull(resultDto.User);
        Assert.Null(resultDto.Error);
        Assert.Equal(user.Email.Value, resultDto.User.Email);
        Assert.Equal(user.Name.Value, resultDto.User.Name);
        Assert.Equal(user.IsFirstLogin, resultDto.RequiresPasswordChange);
    }

    [Fact]
    public void ToDto_InvalidCredentialsError_ShouldReturnFailureDto()
    {
        // Arrange: F#のInvalidCredentialsエラーを準備
        var errorResult = FSharpResult<User, AuthenticationError>.NewError(
            AuthenticationError.InvalidCredentials);

        // Act: F# → C# 変換実行
        var resultDto = AuthenticationConverter.ToDto(errorResult);

        // Assert: InvalidCredentialsエラーの検証
        Assert.False(resultDto.IsSuccess);
        Assert.Null(resultDto.User);
        Assert.NotNull(resultDto.Error);
        Assert.Equal("InvalidCredentials", resultDto.Error.Type);
        Assert.Contains("認証情報が正しくありません", resultDto.Error.Message);
    }

    [Fact]
    public void ToDto_UserNotFoundError_ShouldReturnFailureDtoWithEmail()
    {
        // Arrange: F#のUserNotFoundエラーを準備
        var testEmail = Email.create("notfound@example.com").ResultValue;
        var errorResult = FSharpResult<User, AuthenticationError>.NewError(
            AuthenticationError.NewUserNotFound(testEmail));

        // Act: F# → C# 変換実行
        var resultDto = AuthenticationConverter.ToDto(errorResult);

        // Assert: UserNotFoundエラーの検証
        Assert.False(resultDto.IsSuccess);
        Assert.NotNull(resultDto.Error);
        Assert.Equal("UserNotFound", resultDto.Error.Type);
        Assert.Equal("notfound@example.com", resultDto.Error.Email);
        Assert.Contains("ユーザーが見つかりません", resultDto.Error.Message);
    }

    [Fact]
    public void ToDto_ValidationError_ShouldReturnFailureDtoWithMessage()
    {
        // Arrange: F#のValidationErrorを準備
        var validationMessage = "メールアドレスの形式が正しくありません";
        var errorResult = FSharpResult<User, AuthenticationError>.NewError(
            AuthenticationError.NewValidationError(validationMessage));

        // Act: F# → C# 変換実行
        var resultDto = AuthenticationConverter.ToDto(errorResult);

        // Assert: ValidationErrorの検証
        Assert.False(resultDto.IsSuccess);
        Assert.NotNull(resultDto.Error);
        Assert.Equal("ValidationError", resultDto.Error.Type);
        Assert.Contains(validationMessage, resultDto.Error.Message);
    }

    [Fact]
    public void ToDto_AccountLockedError_ShouldReturnFailureDtoWithLockoutDetails()
    {
        // Arrange: F#のAccountLockedエラーを準備
        var lockedEmail = Email.create("locked@example.com").ResultValue;
        var lockoutEnd = DateTime.UtcNow.AddMinutes(15);
        var errorResult = FSharpResult<User, AuthenticationError>.NewError(
            AuthenticationError.NewAccountLocked(lockedEmail, lockoutEnd));

        // Act: F# → C# 変換実行
        var resultDto = AuthenticationConverter.ToDto(errorResult);

        // Assert: AccountLockedエラーの検証
        Assert.False(resultDto.IsSuccess);
        Assert.NotNull(resultDto.Error);
        Assert.Equal("AccountLocked", resultDto.Error.Type);
        Assert.Equal("locked@example.com", resultDto.Error.Email);
        Assert.Equal(lockoutEnd, resultDto.Error.LockoutEnd);
        Assert.Contains("アカウントがロックされています", resultDto.Error.Message);
    }

    [Fact]
    public void ToDto_SystemError_ShouldReturnFailureDtoWithExceptionDetails()
    {
        // Arrange: F#のSystemErrorを準備
        var testException = new InvalidOperationException("データベース接続エラー");
        var errorResult = FSharpResult<User, AuthenticationError>.NewError(
            AuthenticationError.NewSystemError(testException));

        // Act: F# → C# 変換実行
        var resultDto = AuthenticationConverter.ToDto(errorResult);

        // Assert: SystemErrorの検証
        Assert.False(resultDto.IsSuccess);
        Assert.NotNull(resultDto.Error);
        Assert.Equal("SystemError", resultDto.Error.Type);
        Assert.Contains("システムエラーが発生しました", resultDto.Error.Message);
        Assert.True(resultDto.Error.SystemDetails.ContainsKey("ExceptionType"));
        Assert.Equal("InvalidOperationException", resultDto.Error.SystemDetails["ExceptionType"]);
    }

    [Fact]
    public void ToDto_TwoFactorRequired_ShouldReturnFailureDtoWithTwoFactorFlag()
    {
        // Arrange: F#のTwoFactorRequiredエラーを準備
        var userEmail = Email.create("2fa@example.com").ResultValue;
        var errorResult = FSharpResult<User, AuthenticationError>.NewError(
            AuthenticationError.NewTwoFactorRequired(userEmail));

        // Act: F# → C# 変換実行
        var resultDto = AuthenticationConverter.ToDto(errorResult);

        // Assert: TwoFactorRequiredの検証
        Assert.False(resultDto.IsSuccess);
        Assert.True(resultDto.RequiresTwoFactor);
        Assert.NotNull(resultDto.Error);
        Assert.Equal("TwoFactorRequired", resultDto.Error.Type);
        Assert.Equal("2fa@example.com", resultDto.Error.Email);
    }

    // =================================================================
    // 🧪 C# → F# 変換テスト（LoginRequestDto）
    // =================================================================

    [Fact]
    public void ToFSharpLoginParams_ValidLoginRequest_ShouldReturnSuccessResult()
    {
        // Arrange: 有効なログインリクエストを準備
        var loginDto = new LoginRequestDto
        {
            Email = "test@example.com",
            Password = "SecurePassword123!",
            RememberMe = true
        };

        // Act: C# → F# 変換実行
        var result = AuthenticationConverter.ToFSharpLoginParams(loginDto);

        // Assert: 成功結果の検証
        Assert.True(result.IsOk);
        var (email, password) = result.ResultValue;
        Assert.Equal("test@example.com", email.Value);
        Assert.Equal("SecurePassword123!", password);
    }

    [Fact]
    public void ToFSharpLoginParams_InvalidEmail_ShouldReturnValidationError()
    {
        // Arrange: 無効なメールアドレスのログインリクエスト
        var loginDto = new LoginRequestDto
        {
            Email = "invalid-email",
            Password = "password"
        };

        // Act: C# → F# 変換実行
        var result = AuthenticationConverter.ToFSharpLoginParams(loginDto);

        // Assert: バリデーションエラーの検証
        Assert.True(result.IsError);
        Assert.Contains("メールアドレス", result.ErrorValue);
    }

    [Fact]
    public void ToFSharpLoginParams_EmptyPassword_ShouldReturnValidationError()
    {
        // Arrange: 空のパスワードのログインリクエスト
        var loginDto = new LoginRequestDto
        {
            Email = "test@example.com",
            Password = ""
        };

        // Act: C# → F# 変換実行
        var result = AuthenticationConverter.ToFSharpLoginParams(loginDto);

        // Assert: バリデーションエラーの検証
        Assert.True(result.IsError);
        Assert.Contains("パスワードが入力されていません", result.ErrorValue);
    }

    [Fact]
    public void ToFSharpLoginParams_NullLoginRequest_ShouldReturnError()
    {
        // Arrange: nullリクエスト
        LoginRequestDto? loginDto = null;

        // Act: C# → F# 変換実行
        var result = AuthenticationConverter.ToFSharpLoginParams(loginDto!);

        // Assert: nullエラーの検証
        Assert.True(result.IsError);
        Assert.Contains("nullです", result.ErrorValue);
    }

    // =================================================================
    // 🧪 エラーハンドリング・境界値テスト
    // =================================================================

    [Fact]
    public void ToDto_NullAuthenticationResult_ShouldThrowArgumentNullException()
    {
        // Arrange: null認証結果
        FSharpResult<User, AuthenticationError>? nullResult = null;

        // Act & Assert: ArgumentNullException の検証
        var exception = Assert.Throws<ArgumentNullException>(
            () => AuthenticationConverter.ToDto(nullResult!));
        Assert.Contains("認証結果がnullです", exception.Message);
    }

    [Fact]
    public void ToDto_NullAuthenticationError_ShouldThrowArgumentNullException()
    {
        // Arrange: null認証エラー
        AuthenticationError? nullError = null;

        // Act & Assert: ArgumentNullException の検証
        var exception = Assert.Throws<ArgumentNullException>(
            () => AuthenticationConverter.ToDto(nullError!));
        Assert.Contains("認証エラーがnullです", exception.Message);
    }

    // =================================================================
    // 🧪 統合テスト：TypeConverters.cs経由
    // =================================================================

    [Fact]
    public void TypeConverters_AuthenticationResult_ShouldUseAuthenticationConverter()
    {
        // Arrange: F#成功認証結果
        var user = CreateTestUser();
        var successResult = FSharpResult<User, AuthenticationError>.NewOk(user);

        // Act: TypeConverters.cs経由で変換
        var resultDto = TypeConverters.ToDto(successResult);

        // Assert: AuthenticationConverterと同じ結果の検証
        Assert.True(resultDto.IsSuccess);
        Assert.NotNull(resultDto.User);
        Assert.Equal(user.Email.Value, resultDto.User.Email);
    }

    [Fact]
    public void TypeConverters_LoginRequestDto_ShouldUseAuthenticationConverter()
    {
        // Arrange: 有効なログインリクエスト
        var loginDto = new LoginRequestDto
        {
            Email = "integration@example.com",
            Password = "TestPass123!"
        };

        // Act: TypeConverters.cs経由で変換
        var result = TypeConverters.FromDto(loginDto);

        // Assert: AuthenticationConverterと同じ結果の検証
        Assert.True(result.IsOk);
        var (email, password) = result.ResultValue;
        Assert.Equal("integration@example.com", email.Value);
    }

    // =================================================================
    // 🛠️ テストヘルパーメソッド
    // =================================================================

    /// <summary>
    /// テスト用Userエンティティの作成
    /// F# Domain層のUserエンティティを構築
    /// </summary>
    /// <returns>テスト用Userエンティティ</returns>
    private static User CreateTestUser()
    {
        var email = Email.create("test@example.com").ResultValue;
        var userName = UserName.create("Test User").ResultValue;
        var role = Role.GeneralUser;
        var createdBy = UserId.create(1L);

        return User.create(email, userName, role, createdBy);
    }
}

/// <summary>
/// Phase A9: 認証専用エラーDTO統合テスト
/// AuthenticationErrorDtoファクトリーメソッドの完全検証
/// </summary>
public class AuthenticationErrorDtoTests
{
    [Fact]
    public void InvalidCredentials_ShouldCreateCorrectErrorDto()
    {
        // Act: InvalidCredentialsエラーDTO作成
        var error = AuthenticationErrorDto.InvalidCredentials();

        // Assert: 作成結果の検証
        Assert.Equal("InvalidCredentials", error.Type);
        Assert.Contains("認証情報が正しくありません", error.Message);
        Assert.Null(error.Email);
        Assert.Null(error.LockoutEnd);
        Assert.True((DateTime.UtcNow - error.OccurredAt).TotalSeconds < 1);
    }

    [Fact]
    public void UserNotFound_ShouldCreateCorrectErrorDto()
    {
        // Act: UserNotFoundエラーDTO作成
        var email = "missing@example.com";
        var error = AuthenticationErrorDto.UserNotFound(email);

        // Assert: 作成結果の検証
        Assert.Equal("UserNotFound", error.Type);
        Assert.Equal(email, error.Email);
        Assert.Contains("ユーザーが見つかりません", error.Message);
    }

    [Fact]
    public void AccountLocked_ShouldCreateCorrectErrorDto()
    {
        // Act: AccountLockedエラーDTO作成
        var email = "locked@example.com";
        var lockoutEnd = DateTime.UtcNow.AddMinutes(30);
        var error = AuthenticationErrorDto.AccountLocked(email, lockoutEnd);

        // Assert: 作成結果の検証
        Assert.Equal("AccountLocked", error.Type);
        Assert.Equal(email, error.Email);
        Assert.Equal(lockoutEnd, error.LockoutEnd);
        Assert.Contains("アカウントがロックされています", error.Message);
        Assert.Contains(lockoutEnd.ToString("yyyy/MM/dd HH:mm"), error.Message);
    }

    [Fact]
    public void SystemError_ShouldCreateCorrectErrorDtoWithExceptionDetails()
    {
        // Arrange: テスト用例外
        var innerException = new ArgumentException("Invalid argument");
        var mainException = new InvalidOperationException("Operation failed", innerException);

        // Act: SystemErrorエラーDTO作成
        var error = AuthenticationErrorDto.SystemError(mainException);

        // Assert: 作成結果の検証
        Assert.Equal("SystemError", error.Type);
        Assert.Contains("システムエラーが発生しました", error.Message);
        Assert.Equal("InvalidOperationException", error.SystemDetails["ExceptionType"]);
        Assert.Equal("ArgumentException", error.SystemDetails["InnerExceptionType"]);
        Assert.Equal("Invalid argument", error.SystemDetails["InnerExceptionMessage"]);
    }
}