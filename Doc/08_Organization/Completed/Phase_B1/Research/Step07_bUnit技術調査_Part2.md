# Step07 bUnit技術調査結果 (Part 2/3)

## 2.3 IProjectManagementServiceモック完全例

**本プロジェクトのサービスインターフェースに対応したモック実装**:

```csharp
using Moq;
using Microsoft.FSharp.Core;
using UbiquitousLanguageManager.Application.ProjectManagement;
using UbiquitousLanguageManager.Contracts.DTOs.Application;

public class ProjectManagementServiceMockBuilder
{
    private readonly Mock<IProjectManagementService> _mockService;
    
    public ProjectManagementServiceMockBuilder()
    {
        _mockService = new Mock<IProjectManagementService>();
    }
    
    /// <summary>
    /// GetProjectsAsync成功モックセットアップ
    /// </summary>
    public ProjectManagementServiceMockBuilder SetupGetProjectsSuccess(
        List<ProjectDto> projects, 
        int totalCount = 0)
    {
        var resultDto = new ProjectListResultDto
        {
            IsSuccess = true,
            Projects = projects,
            TotalCount = totalCount > 0 ? totalCount : projects.Count,
            PageNumber = 1,
            PageSize = 20,
            AuthorizationInfo = AuthorizationResultDto.Authorized()
        };
        
        var fsharpResult = FSharpResult<ProjectListResultDto, string>.NewOk(resultDto);
        
        _mockService
            .Setup(s => s.GetProjectsAsync(It.IsAny<GetProjectsQuery>()))
            .ReturnsAsync(fsharpResult);
        
        return this;
    }
    
    /// <summary>
    /// GetProjectsAsync失敗モックセットアップ
    /// </summary>
    public ProjectManagementServiceMockBuilder SetupGetProjectsFailure(string errorMessage)
    {
        var fsharpResult = FSharpResult<ProjectListResultDto, string>.NewError(errorMessage);
        
        _mockService
            .Setup(s => s.GetProjectsAsync(It.IsAny<GetProjectsQuery>()))
            .ReturnsAsync(fsharpResult);
        
        return this;
    }
    
    /// <summary>
    /// CreateProjectAsync成功モックセットアップ
    /// </summary>
    public ProjectManagementServiceMockBuilder SetupCreateProjectSuccess(ProjectDto createdProject)
    {
        var resultDto = new ProjectCreationResultDto
        {
            IsSuccess = true,
            Project = createdProject
        };
        
        var fsharpResult = FSharpResult<ProjectCreationResultDto, string>.NewOk(resultDto);
        
        _mockService
            .Setup(s => s.CreateProjectAsync(It.IsAny<CreateProjectCommand>()))
            .ReturnsAsync(fsharpResult);
        
        return this;
    }
    
    /// <summary>
    /// DeleteProjectAsync成功モックセットアップ
    /// </summary>
    public ProjectManagementServiceMockBuilder SetupDeleteProjectSuccess()
    {
        // F# Unit型は Microsoft.FSharp.Core.Unit で表現
        var unitValue = Microsoft.FSharp.Core.Unit.Default;
        var fsharpResult = FSharpResult<Microsoft.FSharp.Core.Unit, string>.NewOk(unitValue);
        
        _mockService
            .Setup(s => s.DeleteProjectAsync(It.IsAny<DeleteProjectCommand>()))
            .ReturnsAsync(fsharpResult);
        
        return this;
    }
    
    /// <summary>
    /// モックインスタンス取得
    /// </summary>
    public IProjectManagementService Build() => _mockService.Object;
    
    /// <summary>
    /// Mockオブジェクト取得（検証用）
    /// </summary>
    public Mock<IProjectManagementService> BuildMock() => _mockService;
}

// 使用例
public class ProjectListTests : TestContext
{
    [Fact]
    public void Should_Display_Projects_When_Data_Loaded_Successfully()
    {
        // Arrange
        var testProjects = new List<ProjectDto>
        {
            new ProjectDto { Id = 1, Name = "テストプロジェクト1", IsActive = true },
            new ProjectDto { Id = 2, Name = "テストプロジェクト2", IsActive = true }
        };
        
        var mockService = new ProjectManagementServiceMockBuilder()
            .SetupGetProjectsSuccess(testProjects)
            .Build();
        
        Services.AddSingleton(mockService);
        Services.AddSingleton<AuthenticationStateProvider>(new FakeAuthenticationStateProvider());
        
        // Act
        var cut = RenderComponent<ProjectList>();
        
        // Assert
        cut.FindAll("tr.project-row").Count.Should().Be(2);
        cut.Find("td").TextContent.Should().Contain("テストプロジェクト1");
    }
}
```

---

## 3. Blazor Server認証・権限制御テスト

### 3.1 AuthenticationStateProviderモック

**bUnit組み込みのFakeAuthenticationStateProvider使用**:

```csharp
using Bunit;
using Bunit.TestDoubles;
using Xunit;

public class AuthenticationTests : TestContext
{
    [Fact]
    public void Unauthenticated_User_Should_See_Login_Prompt()
    {
        // Arrange
        var authContext = this.AddTestAuthorization();  // デフォルトは未認証
        
        // Act
        var cut = RenderComponent<ProjectList>();
        
        // Assert - 未認証時はAuthorize属性によりリダイレクトされる想定
        cut.Markup.Should().NotContain("プロジェクト管理");
    }
    
    [Fact]
    public void Authenticated_User_Should_See_ProjectList()
    {
        // Arrange
        var authContext = this.AddTestAuthorization();
        authContext.SetAuthorized("testuser@example.com");
        
        // Act
        var cut = RenderComponent<ProjectList>();
        
        // Assert
        cut.Find("h2").TextContent.Should().Contain("プロジェクト管理");
    }
}
```

### 3.2 権限別テストパターン

**SuperUser権限テスト**:
```csharp
[Fact]
public void SuperUser_Should_See_CreateButton()
{
    // Arrange
    var authContext = this.AddTestAuthorization();
    authContext.SetAuthorized("superuser@example.com");
    authContext.SetRoles("SuperUser");  // SuperUserロール設定
    
    var mockService = new ProjectManagementServiceMockBuilder()
        .SetupGetProjectsSuccess(new List<ProjectDto>())
        .Build();
    Services.AddSingleton(mockService);
    
    // Act
    var cut = RenderComponent<ProjectList>();
    
    // Assert
    var createButton = cut.Find("button.btn-primary");
    createButton.TextContent.Should().Contain("新規プロジェクト作成");
}
```

**ProjectManager権限テスト**:
```csharp
[Fact]
public void ProjectManager_Should_See_Edit_But_Not_Delete()
{
    // Arrange
    var authContext = this.AddTestAuthorization();
    authContext.SetAuthorized("pm@example.com");
    authContext.SetRoles("ProjectManager");
    
    var testProject = new ProjectDto 
    { 
        Id = 1, 
        Name = "テストPJ", 
        IsActive = true 
    };
    
    var mockService = new ProjectManagementServiceMockBuilder()
        .SetupGetProjectsSuccess(new List<ProjectDto> { testProject })
        .Build();
    Services.AddSingleton(mockService);
    
    // Act
    var cut = RenderComponent<ProjectList>();
    
    // Assert
    var editButtons = cut.FindAll("button.btn-outline-primary");
    editButtons.Should().HaveCount(1);  // 編集ボタンあり
    
    var deleteButtons = cut.FindAll("button.btn-outline-danger");
    deleteButtons.Should().BeEmpty();  // 削除ボタンなし（SuperUserのみ）
}
```

### 3.3 NavigationManagerモック（リダイレクトテスト用）

**bUnit組み込みFakeNavigationManager使用**:

```csharp
using Bunit;
using Bunit.TestDoubles;
using Xunit;

[Fact]
public void CreateButton_Should_Navigate_To_CreatePage()
{
    // Arrange
    var authContext = this.AddTestAuthorization();
    authContext.SetAuthorized("testuser");
    authContext.SetRoles("SuperUser");
    
    var navMan = Services.GetRequiredService<FakeNavigationManager>();
    var mockService = new ProjectManagementServiceMockBuilder()
        .SetupGetProjectsSuccess(new List<ProjectDto>())
        .Build();
    Services.AddSingleton(mockService);
    
    // Act
    var cut = RenderComponent<ProjectList>();
    var createButton = cut.Find("button.create-project");
    createButton.Click();
    
    // Assert - 遷移先URL確認
    navMan.Uri.Should().EndWith("/projects/create");
}
```
