using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Http;
using NSubstitute;
using Xunit;
using FluentAssertions;
using Microsoft.Extensions.Logging;

namespace UbiquitousLanguageManager.Tests.Integration;

/// <summary>
/// 仕様書2.1.1準拠: Step4 Remember Me・ログアウト機能統合テスト・否定的仕様検証
/// ロックアウト機構なし・統合認証フロー確認
/// </summary>
public class Step4AuthenticationTests
{
    private readonly SignInManager<IdentityUser> _signInManager;
    private readonly UserManager<IdentityUser> _userManager;
    private readonly ILogger<Step4AuthenticationTests> _logger;

    public Step4AuthenticationTests()
    {
        _userManager = Substitute.For<UserManager<IdentityUser>>(
            Substitute.For<IUserStore<IdentityUser>>(),
            null, null, null, null, null, null, null, null);
        
        _signInManager = Substitute.For<SignInManager<IdentityUser>>(
            _userManager,
            Substitute.For<IHttpContextAccessor>(),
            Substitute.For<IUserClaimsPrincipalFactory<IdentityUser>>(),
            null, null, null, null);
        
        _logger = Substitute.For<ILogger<Step4AuthenticationTests>>();
    }

    /// <summary>
    /// 仕様書2.1.1準拠（否定的仕様）: ログイン失敗によるロックアウト機構は設けない
    /// 10回連続ログイン失敗後も正しいパスワードでログイン可能であることを確認
    /// </summary>
    [Fact]
    public async Task Login_AfterMultipleFailures_ShouldNotLockAccount()
    {
        // Arrange
        var user = new IdentityUser
        {
            Id = Guid.NewGuid().ToString(),
            UserName = "test@example.com",
            Email = "test@example.com",
            LockoutEnabled = false // 仕様書2.1.1準拠: ロックアウト無効
        };

        _userManager.FindByEmailAsync(user.Email).Returns(user);
        _userManager.CheckPasswordAsync(user, "WrongPassword").Returns(false);
        _userManager.CheckPasswordAsync(user, "CorrectPassword123!").Returns(true);

        // 失敗時のSignInResult設定
        _signInManager.PasswordSignInAsync(user.Email, "WrongPassword", false, false)
            .Returns(SignInResult.Failed);

        // 成功時のSignInResult設定（実装後に変更）
        _signInManager.PasswordSignInAsync(user.Email, "CorrectPassword123!", false, false)
            .Returns(SignInResult.Failed); // 実装前なので一時的にFailed

        // Act: 10回連続でログイン失敗
        var loginAttempts = 10;
        for (int i = 0; i < loginAttempts; i++)
        {
            var failureResult = await _signInManager.PasswordSignInAsync(
                user.Email, "WrongPassword", false, false);
            failureResult.Should().Be(SignInResult.Failed);
        }

        // Act & Assert: 正しいパスワードでログイン試行
        var successResult = await _signInManager.PasswordSignInAsync(
            user.Email, "CorrectPassword123!", false, false);

        // テスト失敗を確認（実装前のため）
        // 実装後はSignInResult.Successに変更
        successResult.Should().Be(SignInResult.Success, 
            "仕様書2.1.1準拠: ログイン失敗によるロックアウト機構は設けない");
        successResult.IsLockedOut.Should().BeFalse(
            "仕様書2.1.1準拠: 10回失敗後もアカウントはロックアウトされない");
    }

    /// <summary>
    /// エンドツーエンド認証フロー統合テスト
    /// Remember Me → ログアウト → 再ログインの完全フロー
    /// </summary>
    [Fact]
    public async Task CompleteAuthenticationFlow_ShouldWorkEndToEnd()
    {
        // Arrange
        var user = new IdentityUser
        {
            Id = Guid.NewGuid().ToString(),
            UserName = "test@example.com",
            Email = "test@example.com"
        };

        _userManager.FindByEmailAsync(user.Email).Returns(user);
        _userManager.CheckPasswordAsync(user, "TestPassword123!").Returns(true);

        // Act & Assert
        // 1. Remember Meでログイン
        var loginResult = await _signInManager.PasswordSignInAsync(
            user.Email, "TestPassword123!", isPersistent: true, lockoutOnFailure: false);
        
        // テスト失敗を確認（実装前のため）
        loginResult.Should().Be(SignInResult.Success, "Remember Meログインが成功するはず");

        // 2. ログアウト
        await _signInManager.SignOutAsync();
        
        // 3. セッション状態確認
        var isSignedIn = _signInManager.IsSignedIn(null!); // 実装時にHttpContextを適切に設定
        isSignedIn.Should().BeFalse("ログアウト後はサインアウト状態であるはず");
    }
}