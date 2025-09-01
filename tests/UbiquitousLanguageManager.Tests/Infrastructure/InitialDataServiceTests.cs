using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using UbiquitousLanguageManager.Infrastructure.Data.Entities;
using UbiquitousLanguageManager.Infrastructure.Services;
using Microsoft.AspNetCore.Identity;
using Xunit;

namespace UbiquitousLanguageManager.Tests.Infrastructure;

/// <summary>
/// InitialDataService単体テスト
/// TECH-002対応: 初期パスワード"su"での初期ユーザー作成テスト
/// TDD実践：Red-Green-Refactorサイクルによる品質向上
/// </summary>
public class InitialDataServiceTests
{
    private readonly Mock<ILogger<InitialDataService>> _mockLogger;
    private readonly Mock<UserManager<ApplicationUser>> _mockUserManager;
    private readonly Mock<RoleManager<IdentityRole>> _mockRoleManager;
    private readonly Mock<IOptions<InitialSuperUserSettings>> _mockSettings;
    private readonly InitialDataService _service;

    public InitialDataServiceTests()
    {
        // Logger のモック作成
        _mockLogger = new Mock<ILogger<InitialDataService>>();

        // UserManager のモック作成（既存のAuthenticationServiceTestsと同様の構造）
        var userStore = new Mock<IUserStore<ApplicationUser>>();
        _mockUserManager = new Mock<UserManager<ApplicationUser>>(
            userStore.Object, null, null, null, null, null, null, null, null);

        // RoleManager のモック作成
        var roleStore = new Mock<IRoleStore<IdentityRole>>();
        _mockRoleManager = new Mock<RoleManager<IdentityRole>>(
            roleStore.Object, null, null, null, null);

        // Settings のモック作成
        _mockSettings = new Mock<IOptions<InitialSuperUserSettings>>();
        _mockSettings.Setup(s => s.Value).Returns(new InitialSuperUserSettings
        {
            Email = "admin@ubiquitous-lang.com",
            Name = "システム管理者",
            Password = "su", // TECH-002: 仕様準拠の初期パスワード
            IsFirstLogin = true
        });

        // InitialDataService インスタンス作成
        _service = new InitialDataService(
            _mockUserManager.Object,
            _mockRoleManager.Object,
            _mockLogger.Object,
            _mockSettings.Object);
    }

    [Fact]
    public async Task SeedInitialDataAsync_CreatesSuperUserWithInitialPassword_ShouldSucceed()
    {
        // Arrange - RED: まだ期待するInitialPasswordロジックが実装されていないので失敗する
        // TECH-002対応: 初期ユーザーがPasswordHash=NULL、InitialPassword="su"で作成されることをテスト
        
        // ロール存在確認のモック（存在しない前提）
        _mockRoleManager.Setup(x => x.RoleExistsAsync("SuperUser")).ReturnsAsync(false);
        _mockRoleManager.Setup(x => x.RoleExistsAsync("ProjectManager")).ReturnsAsync(false);
        _mockRoleManager.Setup(x => x.RoleExistsAsync("DomainApprover")).ReturnsAsync(false);
        _mockRoleManager.Setup(x => x.RoleExistsAsync("GeneralUser")).ReturnsAsync(false);
        
        // ロール作成成功のモック
        _mockRoleManager.Setup(x => x.CreateAsync(It.IsAny<IdentityRole>()))
                      .ReturnsAsync(IdentityResult.Success);

        // ユーザー存在確認のモック（存在しない前提）
        _mockUserManager.Setup(x => x.FindByEmailAsync("admin@ubiquitous-lang.com"))
                      .ReturnsAsync((ApplicationUser)null);

        // ユーザー作成成功のモック - InitialPassword設定確認
        ApplicationUser capturedUser = null;
        _mockUserManager.Setup(x => x.CreateAsync(It.IsAny<ApplicationUser>()))
                      .ReturnsAsync(IdentityResult.Success)
                      .Callback<ApplicationUser>(user => {
                          capturedUser = user; // テスト後の検証のために保存
                      });

        // ロール付与成功のモック
        _mockUserManager.Setup(x => x.AddToRoleAsync(It.IsAny<ApplicationUser>(), "SuperUser"))
                      .ReturnsAsync(IdentityResult.Success);

        // Act
        await _service.SeedInitialDataAsync();

        // Assert - 初期データ作成処理の実行確認
        _mockRoleManager.Verify(x => x.CreateAsync(It.Is<IdentityRole>(r => r.Name == "SuperUser")), Times.Once);
        _mockRoleManager.Verify(x => x.CreateAsync(It.Is<IdentityRole>(r => r.Name == "GeneralUser")), Times.Once);
        
        // CRITICAL: InitialPassword="su"、PasswordHash=null確認
        _mockUserManager.Verify(x => x.CreateAsync(It.IsAny<ApplicationUser>()), Times.Once);
        
        // キャプチャしたユーザーの詳細検証
        Assert.NotNull(capturedUser);
        Assert.Equal("admin@ubiquitous-lang.com", capturedUser.Email);
        Assert.Equal("システム管理者", capturedUser.Name);
        Assert.True(capturedUser.IsFirstLogin);
        
        // TECH-002 重要検証: 初期パスワード設定とPasswordHashがNULL
        Assert.Equal("su", capturedUser.InitialPassword);
        Assert.Null(capturedUser.PasswordHash); // 平文初期パスワード管理仕様
        
        _mockUserManager.Verify(x => x.AddToRoleAsync(It.IsAny<ApplicationUser>(), "SuperUser"), Times.Once);
    }

    [Fact]
    public async Task SeedInitialDataAsync_ExistingSuperUser_ShouldSkipCreation()
    {
        // Arrange
        var existingUser = new ApplicationUser
        {
            Id = "1",
            Email = "admin@ubiquitous-lang.com",
            UserName = "admin@ubiquitous-lang.com",
            Name = "システム管理者",
            IsFirstLogin = false, // 既存ユーザーはパスワード変更済み
            PasswordHash = "existing_hash", // 既存ユーザーはハッシュ済み
            InitialPassword = null // パスワード変更後はnull
        };

        _mockUserManager.Setup(x => x.FindByEmailAsync("admin@ubiquitous-lang.com"))
                      .ReturnsAsync(existingUser);

        // Act
        await _service.SeedInitialDataAsync();

        // Assert - 既存ユーザーの場合は作成処理をスキップ
        _mockUserManager.Verify(x => x.CreateAsync(It.IsAny<ApplicationUser>()), Times.Never);
    }

    [Fact]
    public async Task SeedInitialDataAsync_UserCreationFailed_ShouldThrowException()
    {
        // Arrange
        _mockRoleManager.Setup(x => x.RoleExistsAsync(It.IsAny<string>()))
                      .ReturnsAsync(true); // ロールは存在する前提
        
        _mockUserManager.Setup(x => x.FindByEmailAsync("admin@ubiquitous-lang.com"))
                      .ReturnsAsync((ApplicationUser)null);

        // ユーザー作成失敗のモック
        var failedResult = IdentityResult.Failed(new IdentityError { 
            Code = "DuplicateEmail", 
            Description = "Email already exists" 
        });
        _mockUserManager.Setup(x => x.CreateAsync(It.IsAny<ApplicationUser>()))
                      .ReturnsAsync(failedResult);

        // Act & Assert - 例外が発生することを確認
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.SeedInitialDataAsync());
        
        Assert.Contains("ユーザー作成に失敗しました", exception.Message);
    }
}