using System;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using NSubstitute;
using UbiquitousLanguageManager.Infrastructure.Services;
using UbiquitousLanguageManager.Contracts.DTOs;
using UbiquitousLanguageManager.Contracts.DTOs.Common;
using UbiquitousLanguageManager.Contracts.Interfaces;
using UbiquitousLanguageManager.Infrastructure.Data.Entities;
using Xunit;

namespace UbiquitousLanguageManager.Tests.Unit.Application
{
    /// <summary>
    /// パスワードリセットサービスの単体テスト
    /// 仕様書2.1.3準拠: パスワードリセット機能
    /// </summary>
    public class PasswordResetServiceTests
    {
        private readonly UserManager<ApplicationUser> _mockUserManager;
        private readonly IEmailSender _mockEmailSender;
        private readonly ILogger<PasswordResetService> _mockLogger;
        private readonly PasswordResetService _service;

        public PasswordResetServiceTests()
        {
            // 🔧 モック作成: NSubstituteによる依存性のモック化
            var userStore = Substitute.For<IUserStore<ApplicationUser>>();
            _mockUserManager = Substitute.For<UserManager<ApplicationUser>>(
                userStore, null, null, null, null, null, null, null, null);
            _mockEmailSender = Substitute.For<IEmailSender>();
            _mockLogger = Substitute.For<ILogger<PasswordResetService>>();

            _service = new PasswordResetService(
                _mockUserManager, 
                _mockEmailSender, 
                _mockLogger);
        }

        [Fact]
        public async Task RequestPasswordResetAsync_正常なメールアドレスでリセットメール送信成功()
        {
            // Arrange
            var email = "test@example.com";
            var user = new ApplicationUser 
            { 
                Id = Guid.NewGuid().ToString(), 
                Email = email, 
                UserName = email 
            };
            var resetToken = "test-reset-token-12345";

            _mockUserManager.FindByEmailAsync(email).Returns(Task.FromResult(user));
            _mockUserManager.GeneratePasswordResetTokenAsync(user).Returns(Task.FromResult(resetToken));
            _mockEmailSender.SendPasswordResetEmailAsync(email, resetToken).Returns(Task.FromResult(true));

            // Act
            var result = await _service.RequestPasswordResetAsync(email);

            // Assert
            result.IsSuccess.Should().BeTrue();

            // 🔍 モック検証: 適切な順序でメソッドが呼ばれたか
            await _mockUserManager.Received(1).FindByEmailAsync(email);
            await _mockUserManager.Received(1).GeneratePasswordResetTokenAsync(user);
            await _mockEmailSender.Received(1).SendPasswordResetEmailAsync(email, resetToken);
        }

        [Fact]
        public async Task RequestPasswordResetAsync_未登録メールアドレスでエラー()
        {
            // Arrange
            var email = "notfound@example.com";
            
            _mockUserManager.FindByEmailAsync(email)
                .Returns(Task.FromResult<ApplicationUser>(null));

            // Act
            var result = await _service.RequestPasswordResetAsync(email);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error.Should().Be("メールアドレスが見つかりません");

            // 🚫 メール送信やトークン生成は実行されないことを確認
            await _mockUserManager.DidNotReceive()
                .GeneratePasswordResetTokenAsync(Arg.Any<ApplicationUser>());
            await _mockEmailSender.DidNotReceive()
                .SendPasswordResetEmailAsync(Arg.Any<string>(), Arg.Any<string>());
        }

        [Fact]
        public async Task ResetPasswordAsync_正常なトークンでパスワードリセット成功()
        {
            // Arrange
            var email = "test@example.com";
            var token = "valid-token";
            var newPassword = "NewPassword123!";
            var user = new ApplicationUser 
            { 
                Id = Guid.NewGuid().ToString(), 
                Email = email, 
                UserName = email 
            };

            _mockUserManager.FindByEmailAsync(email).Returns(Task.FromResult(user));
            _mockUserManager.ResetPasswordAsync(user, token, newPassword)
                .Returns(Task.FromResult(IdentityResult.Success));

            // Act
            var result = await _service.ResetPasswordAsync(email, token, newPassword);

            // Assert
            result.IsSuccess.Should().BeTrue();

            // 🔍 モック検証
            await _mockUserManager.Received(1).FindByEmailAsync(email);
            await _mockUserManager.Received(1).ResetPasswordAsync(user, token, newPassword);
        }

        [Fact]
        public async Task ResetPasswordAsync_無効なトークンでエラー()
        {
            // Arrange
            var email = "test@example.com";
            var invalidToken = "invalid-token";
            var newPassword = "NewPassword123!";
            var user = new ApplicationUser 
            { 
                Id = Guid.NewGuid().ToString(), 
                Email = email, 
                UserName = email 
            };

            _mockUserManager.FindByEmailAsync(email).Returns(Task.FromResult(user));
            _mockUserManager.ResetPasswordAsync(user, invalidToken, newPassword)
                .Returns(Task.FromResult(IdentityResult.Failed(
                    new IdentityError { Code = "InvalidToken", Description = "Invalid token" })));

            // Act
            var result = await _service.ResetPasswordAsync(email, invalidToken, newPassword);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error.Should().Contain("リセットトークンが無効です");
        }

        [Fact]
        public async Task ResetPasswordAsync_弱いパスワードでエラー()
        {
            // Arrange
            var email = "test@example.com";
            var token = "valid-token";
            var weakPassword = "123";
            var user = new ApplicationUser 
            { 
                Id = Guid.NewGuid().ToString(), 
                Email = email, 
                UserName = email 
            };

            _mockUserManager.FindByEmailAsync(email).Returns(Task.FromResult(user));
            _mockUserManager.ResetPasswordAsync(user, token, weakPassword)
                .Returns(Task.FromResult(IdentityResult.Failed(
                    new IdentityError { Code = "PasswordTooShort", Description = "Password too short" })));

            // Act
            var result = await _service.ResetPasswordAsync(email, token, weakPassword);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error.Should().Contain("パスワードが要件を満たしていません");
        }

        [Fact]
        public async Task ValidateResetTokenAsync_有効なトークンで検証成功()
        {
            // Arrange
            var email = "test@example.com";
            var token = "valid-token";
            var user = new ApplicationUser 
            { 
                Id = Guid.NewGuid().ToString(), 
                Email = email, 
                UserName = email 
            };

            _mockUserManager.FindByEmailAsync(email).Returns(Task.FromResult(user));
            _mockUserManager.VerifyUserTokenAsync(
                user,
                _mockUserManager.Options.Tokens.PasswordResetTokenProvider, 
                "ResetPassword",
                token).Returns(Task.FromResult(true));

            // Act
            var result = await _service.ValidateResetTokenAsync(email, token);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().BeTrue();
        }

        [Fact]
        public async Task ValidateResetTokenAsync_無効なトークンで検証失敗()
        {
            // Arrange
            var email = "test@example.com";
            var invalidToken = "invalid-token";
            var user = new ApplicationUser 
            { 
                Id = Guid.NewGuid().ToString(), 
                Email = email, 
                UserName = email 
            };

            _mockUserManager.FindByEmailAsync(email).Returns(Task.FromResult(user));
            _mockUserManager.VerifyUserTokenAsync(
                user,
                _mockUserManager.Options.Tokens.PasswordResetTokenProvider,
                "ResetPassword",
                invalidToken).Returns(Task.FromResult(false));

            // Act
            var result = await _service.ValidateResetTokenAsync(email, invalidToken);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().BeFalse();
        }
    }
}