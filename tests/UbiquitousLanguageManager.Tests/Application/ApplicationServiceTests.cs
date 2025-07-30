using Moq;
using UbiquitousLanguageManager.Contracts.Interfaces;
using UbiquitousLanguageManager.Contracts.DTOs;
using Xunit;

namespace UbiquitousLanguageManager.Tests.Application;

/// <summary>
/// Application層サービス統合テスト
/// 
/// 【テスト方針】
/// 現在のC#アーキテクチャ（IApplicationService）に基づく
/// 統合テストを実施し、主要なビジネス機能を検証します。
/// </summary>
public class ApplicationServiceTests
{
    private readonly Mock<IApplicationService> _mockAppService;

    public ApplicationServiceTests()
    {
        _mockAppService = new Mock<IApplicationService>();
    }

    /// <summary>
    /// テスト用ユーザーDTO作成ヘルパー
    /// </summary>
    private CreateUserDto CreateTestUserDto(string email = "test@example.com", string name = "テストユーザー", string role = "GeneralUser")
    {
        return new CreateUserDto
        {
            Email = email,
            Name = name,
            Role = role,
            CreatedBy = 1L
        };
    }

    /// <summary>
    /// CreateUserAsyncのテスト - 重複メール
    /// </summary>
    [Fact]
    public async Task CreateUserAsync_DuplicateEmail_ShouldReturnError()
    {
        // Arrange
        var createUserDto = CreateTestUserDto("existing@example.com", "重複ユーザー");

        _mockAppService
            .Setup(x => x.CreateUserAsync(It.IsAny<CreateUserDto>()))
            .ReturnsAsync(ServiceResult<UserDto>.Error("メールアドレスが既に存在します"));

        // Act
        var result = await _mockAppService.Object.CreateUserAsync(createUserDto);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("メールアドレスが既に存在します", result.ErrorMessage);
        Assert.Null(result.Data);
    }

    /// <summary>
    /// LoginAsyncのテスト - 有効な認証情報
    /// </summary>
    [Fact]
    public async Task LoginAsync_ValidCredentials_ShouldReturnSuccess()
    {
        // Arrange
        var loginDto = new LoginDto
        {
            Email = "user@example.com",
            Password = "password123"
        };

        var expectedUserDto = new UserDto
        {
            Id = 1L,
            Email = "user@example.com",
            Name = "テストユーザー",
            Role = "GeneralUser",
            IsActive = true
        };

        _mockAppService
            .Setup(x => x.LoginAsync(It.IsAny<LoginDto>()))
            .ReturnsAsync(ServiceResult<UserDto>.Success(expectedUserDto));

        // Act
        var result = await _mockAppService.Object.LoginAsync(loginDto);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Equal("user@example.com", result.Data.Email);
        Assert.Equal("テストユーザー", result.Data.Name);

        // Mock呼び出し確認
        _mockAppService.Verify(
            x => x.LoginAsync(It.Is<LoginDto>(l => l.Email == "user@example.com")),
            Times.Once);
    }

    /// <summary>
    /// LoginAsyncのテスト - 認証失敗
    /// </summary>
    [Fact]
    public async Task LoginAsync_InvalidCredentials_ShouldReturnError()
    {
        // Arrange
        var loginDto = new LoginDto
        {
            Email = "user@example.com",
            Password = "wrongpassword"
        };

        _mockAppService
            .Setup(x => x.LoginAsync(It.IsAny<LoginDto>()))
            .ReturnsAsync(ServiceResult<UserDto>.Error("メールアドレスまたはパスワードが正しくありません"));

        // Act
        var result = await _mockAppService.Object.LoginAsync(loginDto);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("メールアドレスまたはパスワードが正しくありません", result.ErrorMessage);
        Assert.Null(result.Data);
    }

    /// <summary>
    /// LoginAsyncのテスト - バリデーションエラー
    /// </summary>
    [Theory]
    [InlineData("")]
    [InlineData("invalid")]
    [InlineData("user@")]
    [InlineData("@example.com")]
    public async Task LoginAsync_InvalidEmail_ShouldReturnValidationError(string invalidEmail)
    {
        // Arrange
        var loginDto = new LoginDto
        {
            Email = invalidEmail,
            Password = "password123"
        };

        var validationErrors = new Dictionary<string, string>
        {
            { "Email", "有効なメールアドレスを入力してください" }
        };

        _mockAppService
            .Setup(x => x.LoginAsync(It.IsAny<LoginDto>()))
            .ReturnsAsync(ServiceResult<UserDto>.ValidationError(validationErrors));

        // Act
        var result = await _mockAppService.Object.LoginAsync(loginDto);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.NotEmpty(result.ValidationErrors);
        Assert.True(result.ValidationErrors.ContainsKey("Email"));
    }
}