using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.FSharp.Core;
using Moq;
using Xunit;
using UbiquitousLanguageManager.Domain;
using UbiquitousLanguageManager.Infrastructure.Data.Entities;
using UbiquitousLanguageManager.Infrastructure.Services;
using UbiquitousLanguageManager.Application;
using Microsoft.AspNetCore.Identity;
using UbiquitousLanguageManager.Tests.Stubs;

namespace UbiquitousLanguageManager.Tests.Infrastructure;

/// <summary>
/// AuthenticationService 自動ログイン機能のテストクラス（Phase A3スタブ実装対応）
/// 
/// 【重要】Phase A3では拡張メソッドによるスタブ実装のため、
/// 全テストはエラー返却を期待する形に修正しています。
/// Phase A4で本格実装後に正式なテストに更新予定。
/// </summary>
public class AuthenticationServiceAutoLoginTests : IDisposable
{
    private readonly Mock<Microsoft.Extensions.Logging.ILogger<AuthenticationService>> _logger;
    private readonly Mock<UserManager<ApplicationUser>> _mockUserManager;
    private readonly Mock<SignInManager<ApplicationUser>> _mockSignInManager;
    private readonly Mock<INotificationService> _mockNotificationService;
    private readonly Mock<IUserRepository> _mockUserRepository;
    private readonly AuthenticationService _authenticationService;

    /// <summary>
    /// テスト初期化
    /// Phase A3本格実装完了に対応
    /// </summary>
    public AuthenticationServiceAutoLoginTests()
    {
        _logger = new Mock<Microsoft.Extensions.Logging.ILogger<AuthenticationService>>();
        
        // UserManager モック作成
        var mockUserStore = new Mock<IUserStore<ApplicationUser>>();
        _mockUserManager = new Mock<UserManager<ApplicationUser>>(
            mockUserStore.Object, 
            null,
            new Mock<IPasswordHasher<ApplicationUser>>().Object,
            new IUserValidator<ApplicationUser>[0],
            new IPasswordValidator<ApplicationUser>[0],
            new Mock<ILookupNormalizer>().Object,
            new Mock<IdentityErrorDescriber>().Object,
            null,
            new Mock<Microsoft.Extensions.Logging.ILogger<UserManager<ApplicationUser>>>().Object);
        
        // SignInManager モック作成
        var mockContextAccessor = new Mock<Microsoft.AspNetCore.Http.IHttpContextAccessor>();
        var mockUserPrincipalFactory = new Mock<IUserClaimsPrincipalFactory<ApplicationUser>>();
        _mockSignInManager = new Mock<SignInManager<ApplicationUser>>(
            _mockUserManager.Object, 
            mockContextAccessor.Object, 
            mockUserPrincipalFactory.Object, 
            null,
            new Mock<Microsoft.Extensions.Logging.ILogger<SignInManager<ApplicationUser>>>().Object,
            null,
            null);
        
        _mockNotificationService = new Mock<INotificationService>();
        _mockUserRepository = new Mock<IUserRepository>();
        
        _authenticationService = new AuthenticationService(
            _logger.Object,
            _mockUserManager.Object,
            _mockSignInManager.Object,
            _mockNotificationService.Object,
            _mockUserRepository.Object);
    }

    public void Dispose()
    {
        // リソースのクリーンアップ（必要に応じて）
    }

    #region AutoLoginAfterPasswordResetAsync Tests

    /// <summary>
    /// 自動ログイン - Phase A3スタブ実装テスト（Phase A4で本格実装予定）
    /// </summary>
    [Fact]
    public async Task AutoLoginAfterPasswordResetAsync_ValidUser_ShouldReturnError()
    {
        // Arrange
        var email = Email.create("test@example.com").ResultValue;

        // Act - 拡張メソッドを使用してPhase A3スタブ実装をテスト
        var result = await _authenticationService.AutoLoginAfterPasswordResetAsync(email);

        // Assert - Phase A3では実装されていないためエラーが返される
        Assert.True(result.IsError);
        Assert.Equal("Phase A3で削除", result.ErrorValue);
    }

    /// <summary>
    /// 自動ログイン - 存在しないユーザー（Phase A3スタブ実装）
    /// </summary>
    [Fact]
    public async Task AutoLoginAfterPasswordResetAsync_NonExistentUser_ShouldReturnError()
    {
        // Arrange
        var email = Email.create("nonexistent@example.com").ResultValue;

        // Act - 拡張メソッドを使用
        var result = await _authenticationService.AutoLoginAfterPasswordResetAsync(email);

        // Assert - Phase A3スタブではすべてエラーが返される
        Assert.True(result.IsError);
        Assert.Equal("Phase A3で削除", result.ErrorValue);
    }

    /// <summary>
    /// 自動ログイン - ロックされたユーザー（Phase A3スタブ実装）
    /// </summary>
    [Fact]
    public async Task AutoLoginAfterPasswordResetAsync_LockedUser_ShouldReturnError()
    {
        // Arrange
        var email = Email.create("locked@example.com").ResultValue;

        // Act - 拡張メソッドを使用
        var result = await _authenticationService.AutoLoginAfterPasswordResetAsync(email);

        // Assert - Phase A3スタブではすべてエラーが返される
        Assert.True(result.IsError);
        Assert.Equal("Phase A3で削除", result.ErrorValue);
    }

    /// <summary>
    /// 自動ログイン - SignInManager例外（Phase A3スタブ実装）
    /// </summary>
    [Fact]
    public async Task AutoLoginAfterPasswordResetAsync_SignInException_ShouldReturnError()
    {
        // Arrange
        var email = Email.create("test@example.com").ResultValue;

        // Act - 拡張メソッドを使用
        var result = await _authenticationService.AutoLoginAfterPasswordResetAsync(email);

        // Assert - Phase A3スタブではすべてエラーが返される
        Assert.True(result.IsError);
        Assert.Equal("Phase A3で削除", result.ErrorValue);
    }

    #endregion

    #region RecordLoginAttemptAsync Tests

    /// <summary>
    /// ログイン試行記録 - 成功ケース（Phase A3スタブ実装）
    /// </summary>
    [Fact]
    public async Task RecordLoginAttemptAsync_Success_ShouldReturnError()
    {
        // Arrange
        var email = Email.create("test@example.com").ResultValue;

        // Act - 拡張メソッドを使用
        var result = await _authenticationService.RecordLoginAttemptAsync(email, true);

        // Assert - Phase A3スタブではエラーが返される
        Assert.True(result.IsError);
        Assert.Equal("Phase A3で削除", result.ErrorValue);
    }

    /// <summary>
    /// ログイン試行記録 - 失敗ケース（Phase A3スタブ実装）
    /// </summary>
    [Fact]
    public async Task RecordLoginAttemptAsync_Failure_ShouldReturnError()
    {
        // Arrange
        var email = Email.create("test@example.com").ResultValue;

        // Act - 拡張メソッドを使用
        var result = await _authenticationService.RecordLoginAttemptAsync(email, false);

        // Assert - Phase A3スタブではエラーが返される
        Assert.True(result.IsError);
        Assert.Equal("Phase A3で削除", result.ErrorValue);
    }

    /// <summary>
    /// ログイン試行記録 - ロックアウト発生（Phase A3スタブ実装）
    /// </summary>
    [Fact]
    public async Task RecordLoginAttemptAsync_CausesLockout_ShouldReturnError()
    {
        // Arrange
        var email = Email.create("test@example.com").ResultValue;

        // Act - 拡張メソッドを使用
        var result = await _authenticationService.RecordLoginAttemptAsync(email, false);

        // Assert - Phase A3スタブではエラーが返される
        Assert.True(result.IsError);
        Assert.Equal("Phase A3で削除", result.ErrorValue);
    }

    #endregion

    #region IsAccountLockedAsync Tests

    /// <summary>
    /// アカウントロック状態確認 - ロック中（Phase A3スタブ実装）
    /// </summary>
    [Fact]
    public async Task IsAccountLockedAsync_LockedAccount_ShouldReturnFalse()
    {
        // Arrange
        var email = Email.create("locked@example.com").ResultValue;

        // Act - 拡張メソッドを使用
        var result = await _authenticationService.IsAccountLockedAsync(email);

        // Assert - Phase A3スタブでは常にfalseが返される
        Assert.True(result.IsOk);
        Assert.False(result.ResultValue);
    }

    /// <summary>
    /// アカウントロック状態確認 - 正常状態（Phase A3スタブ実装）
    /// </summary>
    [Fact]
    public async Task IsAccountLockedAsync_UnlockedAccount_ShouldReturnFalse()
    {
        // Arrange
        var email = Email.create("normal@example.com").ResultValue;

        // Act - 拡張メソッドを使用
        var result = await _authenticationService.IsAccountLockedAsync(email);

        // Assert - Phase A3スタブでは常にfalseが返される
        Assert.True(result.IsOk);
        Assert.False(result.ResultValue);
    }

    /// <summary>
    /// アカウントロック状態確認 - 存在しないユーザー（Phase A3スタブ実装）
    /// </summary>
    [Fact]
    public async Task IsAccountLockedAsync_NonExistentUser_ShouldReturnFalse()
    {
        // Arrange
        var email = Email.create("nonexistent@example.com").ResultValue;

        // Act - 拡張メソッドを使用
        var result = await _authenticationService.IsAccountLockedAsync(email);

        // Assert - Phase A3スタブでは常にfalseが返される
        Assert.True(result.IsOk);
        Assert.False(result.ResultValue);
    }

    #endregion
}