using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Moq;
using UbiquitousLanguageManager.Contracts.DTOs.Authentication;
using UbiquitousLanguageManager.Infrastructure.Data.Entities;
using UbiquitousLanguageManager.Web.Authentication;
using UbiquitousLanguageManager.Web.Services;
using Xunit;

namespace UbiquitousLanguageManager.Tests.Web;

/// <summary>
/// Web層AuthenticationServiceの単体テスト
/// 
/// 【テスト方針】
/// Phase A2で実装されたWeb層AuthenticationServiceの認証状態管理、
/// ログアウト処理、現在ユーザー情報取得機能、およびPhase A3実装予定の
/// スタブメソッドの動作を検証します。
/// </summary>
public class AuthenticationServiceTests
{
    private readonly Mock<CustomAuthenticationStateProvider> _mockAuthStateProvider;
    private readonly Mock<SignInManager<ApplicationUser>> _mockSignInManager;
    private readonly Mock<UserManager<ApplicationUser>> _mockUserManager;
    private readonly Mock<ILogger<AuthenticationService>> _mockLogger;
    private readonly AuthenticationService _authService;

    public AuthenticationServiceTests()
    {
        // CustomAuthenticationStateProviderのMockにコンストラクタ引数を提供
        var mockHttpContextAccessor = new Mock<Microsoft.AspNetCore.Http.IHttpContextAccessor>();
        var mockUserStore = new Mock<IUserStore<ApplicationUser>>();
        var mockUserManager = new Mock<UserManager<ApplicationUser>>(
            mockUserStore.Object, 
            null,
            new Mock<IPasswordHasher<ApplicationUser>>().Object,
            new IUserValidator<ApplicationUser>[0],
            new IPasswordValidator<ApplicationUser>[0],
            new Mock<ILookupNormalizer>().Object,
            new Mock<IdentityErrorDescriber>().Object,
            null,
            new Mock<Microsoft.Extensions.Logging.ILogger<UserManager<ApplicationUser>>>().Object);
        var mockLogger = new Mock<ILogger<CustomAuthenticationStateProvider>>();
        
        _mockAuthStateProvider = new Mock<CustomAuthenticationStateProvider>(
            mockHttpContextAccessor.Object,
            mockUserManager.Object,
            mockLogger.Object);
        
        // SignInManagerのモックは複雑なため、必要最小限のセットアップ
        var mockUserStore2 = new Mock<IUserStore<ApplicationUser>>();
        _mockUserManager = new Mock<UserManager<ApplicationUser>>(
            mockUserStore2.Object, 
            null,
            new Mock<IPasswordHasher<ApplicationUser>>().Object,
            new IUserValidator<ApplicationUser>[0],
            new IPasswordValidator<ApplicationUser>[0],
            new Mock<ILookupNormalizer>().Object,
            new Mock<IdentityErrorDescriber>().Object,
            null,
            new Mock<Microsoft.Extensions.Logging.ILogger<UserManager<ApplicationUser>>>().Object);
        
        var mockContextAccessor = new Mock<Microsoft.AspNetCore.Http.IHttpContextAccessor>();
        var mockClaimsFactory = new Mock<IUserClaimsPrincipalFactory<ApplicationUser>>();
        _mockSignInManager = new Mock<SignInManager<ApplicationUser>>(
            _mockUserManager.Object, 
            mockContextAccessor.Object, 
            mockClaimsFactory.Object, 
            null,
            new Mock<Microsoft.Extensions.Logging.ILogger<SignInManager<ApplicationUser>>>().Object,
            null,
            null);
        
        _mockLogger = new Mock<ILogger<AuthenticationService>>();

        _authService = new AuthenticationService(
            _mockAuthStateProvider.Object,
            _mockSignInManager.Object,
            _mockUserManager.Object,
            _mockLogger.Object);
    }

    /// <summary>
    /// ログアウト機能のテスト
    /// </summary>
    public class LogoutTests : AuthenticationServiceTests
    {
        [Fact]
        public async Task LogoutAsync_Success_ShouldCallSignOutAndNotifyLogout()
        {
            // Arrange
            _mockSignInManager
                .Setup(x => x.SignOutAsync())
                .Returns(Task.CompletedTask);

            // Act
            await _authService.LogoutAsync();

            // Assert
            _mockSignInManager.Verify(x => x.SignOutAsync(), Times.Once);
            _mockAuthStateProvider.Verify(x => x.NotifyUserLogout(), Times.Once);
            
            // ログが出力されることを確認
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("ログアウト")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task LogoutAsync_SignInManagerThrowsException_ShouldLogError()
        {
            // Arrange
            var expectedException = new InvalidOperationException("ログアウトエラー");
            _mockSignInManager
                .Setup(x => x.SignOutAsync())
                .ThrowsAsync(expectedException);

            // Act
            await _authService.LogoutAsync();

            // Assert
            _mockSignInManager.Verify(x => x.SignOutAsync(), Times.Once);
            
            // NotifyUserLogoutは呼ばれない（例外が発生したため）
            _mockAuthStateProvider.Verify(x => x.NotifyUserLogout(), Times.Never);
            
            // エラーログが出力されることを確認
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("エラー")),
                    expectedException,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task LogoutAsync_AuthStateProviderThrowsException_ShouldLogError()
        {
            // Arrange
            _mockSignInManager
                .Setup(x => x.SignOutAsync())
                .Returns(Task.CompletedTask);
            
            var expectedException = new InvalidOperationException("認証状態エラー");
            _mockAuthStateProvider
                .Setup(x => x.NotifyUserLogout())
                .Throws(expectedException);

            // Act
            await _authService.LogoutAsync();

            // Assert
            _mockSignInManager.Verify(x => x.SignOutAsync(), Times.Once);
            _mockAuthStateProvider.Verify(x => x.NotifyUserLogout(), Times.Once);
            
            // エラーログが出力されることを確認
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("エラー")),
                    expectedException,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }
    }

    /// <summary>
    /// 現在ユーザー情報取得のテスト
    /// </summary>
    public class CurrentUserInfoTests : AuthenticationServiceTests
    {
        [Fact]
        public void GetCurrentDomainUserId_ShouldDelegateToAuthStateProvider()
        {
            // Arrange
            var expectedUserId = 123L;
            _mockAuthStateProvider
                .Setup(x => x.GetCurrentDomainUserId())
                .Returns(expectedUserId);

            // Act
            var result = _authService.GetCurrentDomainUserId();

            // Assert
            Assert.Equal(expectedUserId, result);
            _mockAuthStateProvider.Verify(x => x.GetCurrentDomainUserId(), Times.Once);
        }

        [Fact]
        public void GetCurrentDomainUserId_WhenNull_ShouldReturnNull()
        {
            // Arrange
            _mockAuthStateProvider
                .Setup(x => x.GetCurrentDomainUserId())
                .Returns((long?)null);

            // Act
            var result = _authService.GetCurrentDomainUserId();

            // Assert
            Assert.Null(result);
            _mockAuthStateProvider.Verify(x => x.GetCurrentDomainUserId(), Times.Once);
        }

        [Fact]
        public void IsCurrentUserActive_ShouldDelegateToAuthStateProvider()
        {
            // Arrange
            _mockAuthStateProvider
                .Setup(x => x.IsCurrentUserActive())
                .Returns(true);

            // Act
            var result = _authService.IsCurrentUserActive();

            // Assert
            Assert.True(result);
            _mockAuthStateProvider.Verify(x => x.IsCurrentUserActive(), Times.Once);
        }

        [Fact]
        public void IsCurrentUserActive_WhenInactive_ShouldReturnFalse()
        {
            // Arrange
            _mockAuthStateProvider
                .Setup(x => x.IsCurrentUserActive())
                .Returns(false);

            // Act
            var result = _authService.IsCurrentUserActive();

            // Assert
            Assert.False(result);
            _mockAuthStateProvider.Verify(x => x.IsCurrentUserActive(), Times.Once);
        }

        [Fact]
        public void IsCurrentUserFirstLogin_ShouldDelegateToAuthStateProvider()
        {
            // Arrange
            _mockAuthStateProvider
                .Setup(x => x.IsCurrentUserFirstLogin())
                .Returns(true);

            // Act
            var result = _authService.IsCurrentUserFirstLogin();

            // Assert
            Assert.True(result);
            _mockAuthStateProvider.Verify(x => x.IsCurrentUserFirstLogin(), Times.Once);
        }

        [Fact]
        public void IsCurrentUserFirstLogin_WhenNotFirstLogin_ShouldReturnFalse()
        {
            // Arrange
            _mockAuthStateProvider
                .Setup(x => x.IsCurrentUserFirstLogin())
                .Returns(false);

            // Act
            var result = _authService.IsCurrentUserFirstLogin();

            // Assert
            Assert.False(result);
            _mockAuthStateProvider.Verify(x => x.IsCurrentUserFirstLogin(), Times.Once);
        }
    }

    /// <summary>
    /// Phase A3実装予定機能のテスト（スタブ実装）
    /// </summary>
    public class Phase3StubMethodsTests : AuthenticationServiceTests
    {
        // Phase A3でLoginAsyncは完全実装されているため、このテストは不要

        [Fact]
        public async Task GetCurrentUserAsync_ShouldReturnPhaseA3Error_AndLogMessage()
        {
            // Act
            var result = await _authService.GetCurrentUserAsync();

            // Assert
            Assert.Null(result);
            
            // ログが出力されることを確認
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("GetCurrentUserAsync called - Phase A3で実装予定")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task ChangePasswordAsync_ShouldReturnPhaseA3Error_AndLogMessage()
        {
            // Arrange
            var changePasswordRequest = new ChangePasswordRequestDto
            {
                CurrentPassword = "oldPassword",
                NewPassword = "NewPassword123!",
                ConfirmPassword = "NewPassword123!"
            };

            // Act
            var result = await _authService.ChangePasswordAsync(changePasswordRequest);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal("Phase A3で実装予定", result.ErrorMessage);
            
            // ログが出力されることを確認
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("ChangePasswordAsync called - Phase A3で実装予定")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }
    }

    /// <summary>
    /// 統合シナリオのテスト
    /// </summary>
    public class IntegrationScenarioTests : AuthenticationServiceTests
    {
        [Fact]
        public async Task UserSession_CompleteFlow_ShouldWorkCorrectly()
        {
            // Arrange - ログイン済みユーザーのシミュレート
            var userId = 123L;
            _mockAuthStateProvider.Setup(x => x.GetCurrentDomainUserId()).Returns(userId);
            _mockAuthStateProvider.Setup(x => x.IsCurrentUserActive()).Returns(true);
            _mockAuthStateProvider.Setup(x => x.IsCurrentUserFirstLogin()).Returns(false);

            _mockSignInManager.Setup(x => x.SignOutAsync()).Returns(Task.CompletedTask);

            // Act & Assert - 現在ユーザー情報の取得
            var currentUserId = _authService.GetCurrentDomainUserId();
            var isActive = _authService.IsCurrentUserActive();
            var isFirstLogin = _authService.IsCurrentUserFirstLogin();

            Assert.Equal(userId, currentUserId);
            Assert.True(isActive);
            Assert.False(isFirstLogin);

            // Act & Assert - ログアウト
            await _authService.LogoutAsync();

            // 全ての期待されるメソッドが呼ばれることを確認
            _mockAuthStateProvider.Verify(x => x.GetCurrentDomainUserId(), Times.Once);
            _mockAuthStateProvider.Verify(x => x.IsCurrentUserActive(), Times.Once);
            _mockAuthStateProvider.Verify(x => x.IsCurrentUserFirstLogin(), Times.Once);
            _mockSignInManager.Verify(x => x.SignOutAsync(), Times.Once);
            _mockAuthStateProvider.Verify(x => x.NotifyUserLogout(), Times.Once);
        }

        [Fact]
        public async Task GuestUser_AccessAttempt_ShouldReturnExpectedResults()
        {
            // Arrange - ゲストユーザー（ログインしていない）
            _mockAuthStateProvider.Setup(x => x.GetCurrentDomainUserId()).Returns((long?)null);
            _mockAuthStateProvider.Setup(x => x.IsCurrentUserActive()).Returns(false);
            _mockAuthStateProvider.Setup(x => x.IsCurrentUserFirstLogin()).Returns(false);

            // Act
            var currentUserId = _authService.GetCurrentDomainUserId();
            var isActive = _authService.IsCurrentUserActive();
            var isFirstLogin = _authService.IsCurrentUserFirstLogin();

            // Assert
            Assert.Null(currentUserId);
            Assert.False(isActive);
            Assert.False(isFirstLogin);

            // ログアウトも正常に動作する（ゲストユーザーでも）
            await _authService.LogoutAsync();
            _mockSignInManager.Verify(x => x.SignOutAsync(), Times.Once);
        }

        [Fact]
        public async Task FirstTimeUser_AccessPattern_ShouldReturnCorrectFlags()
        {
            // Arrange - 初回ログインユーザー
            var userId = 456L;
            _mockAuthStateProvider.Setup(x => x.GetCurrentDomainUserId()).Returns(userId);
            _mockAuthStateProvider.Setup(x => x.IsCurrentUserActive()).Returns(true);
            _mockAuthStateProvider.Setup(x => x.IsCurrentUserFirstLogin()).Returns(true);

            // Act
            var currentUserId = _authService.GetCurrentDomainUserId();
            var isActive = _authService.IsCurrentUserActive();
            var isFirstLogin = _authService.IsCurrentUserFirstLogin();

            // Assert
            Assert.Equal(userId, currentUserId);
            Assert.True(isActive);
            Assert.True(isFirstLogin); // 初回ログインユーザー
        }
    }
}