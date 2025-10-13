using UbiquitousLanguageManager.Contracts.Converters;
using UbiquitousLanguageManager.Contracts.DTOs.Authentication;
using UbiquitousLanguageManager.Domain.Common;
using UbiquitousLanguageManager.Domain.Authentication;
using Microsoft.FSharp.Core;
using Xunit;

namespace UbiquitousLanguageManager.Contracts.Unit.Tests;

/// <summary>
/// AuthenticationMapperの単体テスト
/// 
/// 【テスト方針】
/// F#とC#間の型変換が正しく動作することを確認します。
/// 特にOption型の扱いとエラーハンドリングをテストします。
/// </summary>
public class AuthenticationMapperTests
{
    /// <summary>
    /// LoginRequestDto → LoginCommand 変換テスト
    /// </summary>
    public class ToLoginCommandTests
    {
        [Fact]
        public void ToLoginCommand_ValidRequest_ShouldConvertCorrectly()
        {
            // Arrange
            var dto = new LoginRequestDto
            {
                Email = "test@example.com",
                Password = "password123",
                RememberMe = true
            };

            // Act
            var command = AuthenticationMapper.ToLoginCommand(dto);

            // Assert
            Assert.Equal(dto.Email, command.Email);
            Assert.Equal(dto.Password, command.Password);
        }

        [Fact]
        public void ToLoginCommand_EmptyValues_ShouldHandleCorrectly()
        {
            // Arrange
            var dto = new LoginRequestDto
            {
                Email = "",
                Password = "",
                RememberMe = false
            };

            // Act
            var command = AuthenticationMapper.ToLoginCommand(dto);

            // Assert
            Assert.Equal("", command.Email);
            Assert.Equal("", command.Password);
        }
    }

    /// <summary>
    /// ChangePasswordRequestDto → ChangePasswordCommand 変換テスト
    /// </summary>
    public class ToChangePasswordCommandTests
    {
        [Fact]
        public void ToChangePasswordCommand_ValidRequest_ShouldConvertCorrectly()
        {
            // Arrange
            var dto = new ChangePasswordRequestDto
            {
                CurrentPassword = "oldPassword",
                NewPassword = "newPassword123",
                ConfirmPassword = "newPassword123"
            };
            var userId = 123L;

            // Act
            var command = AuthenticationMapper.ToChangePasswordCommand(dto, userId);

            // Assert
            Assert.Equal(userId, command.UserId);
            Assert.Equal(dto.CurrentPassword, command.OldPassword);
            Assert.Equal(dto.NewPassword, command.NewPassword);
            Assert.Equal(dto.ConfirmPassword, command.ConfirmPassword);
        }
    }

    /// <summary>
    /// User → AuthenticatedUserDto 変換テスト
    /// </summary>
    public class ToAuthenticatedUserDtoTests
    {
        [Fact]
        public void ToAuthenticatedUserDto_ValidUser_ShouldConvertCorrectly()
        {
            // Arrange
            var emailResult = Email.create("test@example.com");
            var nameResult = UserName.create("テストユーザー");
            
            Assert.True(emailResult.IsOk);
            Assert.True(nameResult.IsOk);

            var user = User.create(
                emailResult.ResultValue,
                nameResult.ResultValue,
                Role.GeneralUser,
                UserId.create(1L)
            );

            // Act
            var dto = AuthenticationMapper.ToAuthenticatedUserDto(user);

            // Assert
            Assert.Equal(0L, dto.Id); // User.createはID=0を設定する
            Assert.Equal("test@example.com", dto.Email);
            Assert.Equal("テストユーザー", dto.Name);
            Assert.Equal("GeneralUser", dto.Role);
            Assert.True(dto.IsActive);
        }
    }

    /// <summary>
    /// UserRole変換テスト
    /// </summary>
    public class UserRoleConversionTests
    {
        [Fact]
        public void StringToRole_SuperUser_ShouldConvertCorrectly()
        {
            // Act
            var result = AuthenticationMapper.StringToRole("superuser");

            // Assert
            Assert.True(result.IsOk);
            Assert.True(result.ResultValue.IsSuperUser);
        }

        [Fact]
        public void StringToRole_ProjectManager_ShouldConvertCorrectly()
        {
            // Act
            var result = AuthenticationMapper.StringToRole("projectmanager");

            // Assert
            Assert.True(result.IsOk);
            Assert.True(result.ResultValue.IsProjectManager);
        }

        [Fact]
        public void StringToRole_DomainApprover_ShouldConvertCorrectly()
        {
            // Act
            var result = AuthenticationMapper.StringToRole("domainapprover");

            // Assert
            Assert.True(result.IsOk);
            Assert.True(result.ResultValue.IsDomainApprover);
        }

        [Fact]
        public void StringToRole_GeneralUser_ShouldConvertCorrectly()
        {
            // Act
            var result = AuthenticationMapper.StringToRole("generaluser");

            // Assert
            Assert.True(result.IsOk);
            Assert.True(result.ResultValue.IsGeneralUser);
        }

        [Fact]
        public void StringToRole_CaseInsensitive_ShouldWork()
        {
            // Act
            var result1 = AuthenticationMapper.StringToRole("SUPERUSER");
            var result2 = AuthenticationMapper.StringToRole("ProjectManager");

            // Assert
            Assert.True(result1.IsOk);
            Assert.True(result1.ResultValue.IsSuperUser);
            Assert.True(result2.IsOk);
            Assert.True(result2.ResultValue.IsProjectManager);
        }

        [Theory]
        [InlineData("invalid")]
        [InlineData("")]
        [InlineData("admin")]
        public void StringToRole_InvalidRoles_ShouldReturnError(string roleString)
        {
            // Act
            var result = AuthenticationMapper.StringToRole(roleString);

            // Assert
            Assert.True(result.IsError);
        }

        [Fact]
        public void StringToRole_Null_ShouldReturnError()
        {
            // Act
            var result = AuthenticationMapper.StringToRole(null!);

            // Assert
            Assert.True(result.IsError);
        }
    }
}