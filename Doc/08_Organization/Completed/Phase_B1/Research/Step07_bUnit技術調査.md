# Step07 bUnit技術調査結果 (Part 1/3)

## 📋 調査概要
- **調査日**: 2025-10-05
- **調査目的**: Phase B1 Step7 Stage4（テストアーキテクチャ移行・bUnit UIテスト実装）準備
- **調査対象**: Blazor Server UIコンポーネントのbUnitテスト実装技術

---

## 1. bUnit基本情報

### 1.1 最新バージョン情報
- **最新版**: bUnit 1.40.0（2025年6月14日リリース）
- **.NET 8.0対応**: 完全サポート（.NET 5.0-9.0対応）
- **Blazor Server対応**: Blazor Server/WebAssembly両方サポート

### 1.2 NuGetパッケージ構成

**推奨インストールパッケージ**:
```bash
dotnet add package bunit --version 1.40.0
```

**パッケージ構成**:
- `bunit`: メインパッケージ（bunit.core + bunit.web統合）
- `bunit.core`: コア機能（TestContext等）
- `bunit.web`: Web UI固有機能（AuthorizeView等）

### 1.3 テストプロジェクト要件

**.csprojファイル設定**:
```xml
<Project Sdk="Microsoft.NET.Sdk.Razor">  <!-- 重要: Microsoft.NET.Sdk.Razor を使用 -->
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
  </PropertyGroup>
  
  <ItemGroup>
    <PackageReference Include="bunit" Version="1.40.0" />
    <PackageReference Include="xunit" Version="2.9.0" />
    <PackageReference Include="xunit.runner.visualstudio" Version="2.8.2" />
    <PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.11.0" />
    <PackageReference Include="Moq" Version="4.20.72" />
    <PackageReference Include="FluentAssertions" Version="6.12.1" />
  </ItemGroup>
  
  <ItemGroup>
    <ProjectReference Include="..\..\src\UbiquitousLanguageManager.Web\UbiquitousLanguageManager.Web.csproj" />
  </ItemGroup>
</Project>
```

**_Imports.razorファイル**:
```csharp
@using Bunit
@using Bunit.TestDoubles
@using Xunit
@using FluentAssertions
@using Moq
@using Microsoft.Extensions.DependencyInjection
@using Microsoft.AspNetCore.Components
@using Microsoft.AspNetCore.Components.Authorization
```

---

## 2. F#型システムとbUnit統合パターン

### 2.1 F# Result型のC#からの生成

**基本的な生成方法**:
```csharp
using Microsoft.FSharp.Core;

// 成功Result (Ok) の生成
var successResult = FSharpResult<ProjectListResultDto, string>.NewOk(new ProjectListResultDto
{
    IsSuccess = true,
    Projects = new List<ProjectDto>(),
    TotalCount = 0,
    PageNumber = 1,
    PageSize = 20
});

// 失敗Result (Error) の生成
var errorResult = FSharpResult<ProjectListResultDto, string>.NewError("プロジェクト取得に失敗しました");

// 別の生成方法
var result1 = FSharpResult.NewOk<ProjectListResultDto, string>(data);
var result2 = FSharpResult.NewError<ProjectListResultDto, string>("error");
```

**Resultのパターンマッチング**:
```csharp
public void ProcessResult(FSharpResult<ProjectListResultDto, string> result)
{
    if (result.IsOk)
    {
        var data = result.ResultValue;
        // 成功時の処理
    }
    else
    {
        var error = result.ErrorValue;
        // エラー時の処理
    }
}

// Switch式を使ったパターン
var message = result switch
{
    FSharpResult<ProjectListResultDto, string>.Ok ok => $"成功: {ok.Item.Projects.Count}件",
    FSharpResult<ProjectListResultDto, string>.Error err => $"エラー: {err.Item}",
    _ => "不明な結果"
};
```

### 2.2 F# Option型のC#からの生成

**基本的な生成方法**:
```csharp
using Microsoft.FSharp.Core;

// Some値の生成
var someValue = FSharpOption<string>.Some("プロジェクト説明");
var someValue2 = new FSharpOption<string>("プロジェクト説明");

// None値の生成
var noneValue = FSharpOption<string>.None;

// Option値のチェック
if (FSharpOption<string>.get_IsSome(optionValue))
{
    var value = optionValue.Value;
    // 値が存在する場合の処理
}

if (FSharpOption<string>.get_IsNone(optionValue))
{
    // 値が存在しない場合の処理
}
```

**拡張メソッドパターン（推奨）**:
```csharp
public static class FSharpOptionExtensions
{
    public static bool IsSome<T>(this FSharpOption<T> option)
        => FSharpOption<T>.get_IsSome(option);
    
    public static bool IsNone<T>(this FSharpOption<T> option)
        => FSharpOption<T>.get_IsNone(option);
    
    public static T GetValueOrDefault<T>(this FSharpOption<T> option, T defaultValue = default)
        => option.IsSome() ? option.Value : defaultValue;
}

// 使用例
var description = descriptionOption.GetValueOrDefault("説明なし");
```
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


# Step07 bUnit技術調査結果 (Part 3/3)

## 4. bUnit基本テストパターン

### 4.1 コンポーネントレンダリング
### 4.2 イベント発火テスト  
### 4.3 DOM検証パターン
### 4.4 非同期処理待機

（詳細はPart3ファイル参照）

## 5. テストプロジェクト構成推奨

### 5.1 ディレクトリ構成
### 5.2 テストベースクラス設計案
### 5.3 テストデータファクトリ設計案

## 6. 実装時の注意点

### 6.1 F#型統合の注意点
### 6.2 Blazor Server特有の注意点
### 6.3 Authorize属性テストの制限

## 7. 参考資料リンク

- bUnit公式: https://bunit.dev/
- F# Result/Option: https://learn.microsoft.com/en-us/dotnet/fsharp/language-reference/

## 8. Stage4実装への推奨事項

次セッションでの実装準備完了。本調査結果を参照し即座に実装開始可能。

調査完了日時: 2025-10-05
調査担当: tech-research Agent
