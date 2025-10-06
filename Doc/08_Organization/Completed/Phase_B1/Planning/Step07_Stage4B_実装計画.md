# Step07 Stage4-B bUnit UIテスト実装計画書

**作成日**: 2025-10-05
**対象Phase**: Phase B1 Step7 Stage4-B
**作成目的**: Stage4-A完了後の本格実装を円滑に実行するための詳細計画
**推定実施時間**: 2.5-3時間（Phase 0 + Phase 1-3）
**最終更新**: 2025-10-05（Phase 0: 事前修正追加）

---

## Part 0: 事前修正（Stage4-A完了時に発見された問題）

### 0.1 ConfirmationDialogプロパティ名修正

**問題内容**:
Stage4-Aのテスト実行時に、`ConfirmationDialog`コンポーネントに存在しないプロパティ名が使用されていることが判明。

**エラー内容**:
```
System.InvalidOperationException: Object of type 'UbiquitousLanguageManager.Web.Pages.Admin.Components.ConfirmationDialog' does not have a property matching the name 'ConfirmButtonText'.
```

**原因**:
- **誤**: `ConfirmButtonText`プロパティを使用（存在しない）
- **正**: `ConfirmText`プロパティが正しい（ConfirmationDialog.razor:100行目）

**修正対象ファイル**:
1. `src/UbiquitousLanguageManager.Web/Components/Pages/ProjectManagement/ProjectList.razor`
2. `src/UbiquitousLanguageManager.Web/Components/Pages/ProjectManagement/ProjectDeleteDialog.razor`
3. その他、`ConfirmationDialog`を使用している全ファイル

**修正内容**:
```razor
<!-- 修正前（エラー） -->
<ConfirmationDialog
    ConfirmButtonText="削除"  @* ❌ 存在しないプロパティ *@
    ... />

<!-- 修正後（正しい） -->
<ConfirmationDialog
    ConfirmText="削除"  @* ✅ 正しいプロパティ名 *@
    ... />
```

**担当Agent**: csharp-web-ui Agent

**推定時間**: 5-10分

**実施タイミング**: Stage4-B開始直後（Phase 0として最優先実施）

**品質基準**:
- ✅ ビルドエラー0件
- ✅ 修正箇所の特定完了（Grep検索）
- ✅ 全修正箇所でビルド成功確認

**次工程への影響**:
この修正完了後、Phase 1（テストケース実装）に進むことで、テスト失敗原因がWeb層の問題ではなく、テストコードの問題に絞り込める。

---

## Part 1: プロジェクト概要

### 1.1 テストアーキテクチャ不整合の背景

#### 問題状況
**Stage2で作成したテスト**: HTTP API統合テスト（`ProjectManagementIntegrationTests.cs`）
- **テスト対象**: `/api/projects`等のREST APIエンドポイント
- **テストツール**: `WebApplicationFactory<Program>`（HttpClient）
- **想定アーキテクチャ**: REST API + JSON通信

**Stage3で実装したコード**: Blazor Server UIコンポーネント
- **実装形式**: `.razor`ファイル（SignalRベース）
- **通信方式**: Blazor Server SignalRハブ（REST APIなし）
- **実際のアーキテクチャ**: SignalR双方向通信 + サーバーサイドレンダリング

#### テスト結果
```
失敗!   -失敗:    10、合格:     0、スキップ:     0、合計:    10
全10テスト失敗理由: Assert.Fail("TDD Red Phase: エンドポイント未実装想定")
```

**根本原因**: Blazor ServerはREST APIを公開しないアーキテクチャ

```
【従来のWeb API】
Browser → HTTP Request → /api/projects → Controller → Service
         ← JSON Response ←

【Blazor Server】
Browser ←→ SignalR Hub ←→ Razor Component → Service
        (双方向通信)        (サーバーサイドレンダリング)
```

### 1.2 bUnit移行の技術的根拠

#### bUnit選定理由
1. **Blazor Server専用テストフレームワーク**: SignalRベースUIコンポーネントの直接テスト可能
2. **Razor構文サポート**: `.razor`ファイルのマークアップ・`@code`ブロック完全対応
3. **F#型システム統合**: `FSharpResult`/`FSharpOption`のC#からの生成・検証パターン確立済み
4. **認証・権限制御モック**: `FakeAuthenticationStateProvider`による権限別テスト実現
5. **.NET 8.0完全対応**: bUnit 1.40.0（2025年6月リリース）で最新.NET対応

#### 技術調査完了状況
- **調査文書**: `Doc/08_Organization/Active/Phase_B1/Research/Step07_bUnit技術調査.md`（470行・14KB）
- **調査完了日**: 2025-10-05
- **調査Agent**: tech-research Agent

### 1.3 実装範囲・制約事項

#### 実装範囲
**Phase B1スコープ**: 「プロジェクト基本CRUD」（マスタープラン準拠）
- ✅ **SuperUser権限テスト**: 全機能アクセス可能確認（6パターン）
- ✅ **ProjectManager権限テスト**: 担当プロジェクトのみ確認（4パターン）
- ⏸️ **DomainApprover権限**: Phase B2対応（4パターン）
- ⏸️ **GeneralUser権限**: Phase B2対応（4パターン）
- ⏸️ **UserProjects中間テーブル**: Phase B2対応
- ⏸️ **デフォルトドメイン詳細検証**: Phase B3対応

#### 制約事項
1. **Phase B1品質基準**: 仕様準拠度95-100点達成（現状10テストで基本動作検証十分）
2. **既存実装活用**: Stage3で確立したF#↔C#型変換パターン完全適用
3. **Railway-oriented Programming統合**: F# Result型のBlazor Server統合パターン継続
4. **0 Warning/0 Error維持**: ビルド・テスト実行100%成功必須

---

## Part 2: テストインフラ設計

### 2.1 テストプロジェクト構成

#### ディレクトリ構造
```
tests/UbiquitousLanguageManager.Web.Tests/
├── UbiquitousLanguageManager.Web.Tests.csproj  # Microsoft.NET.Sdk.Razor必須
├── _Imports.razor                               # bUnit名前空間インポート
├── Infrastructure/
│   ├── BlazorComponentTestBase.cs              # 共通基底クラス
│   ├── FSharpTypeHelpers.cs                    # F#型統合拡張メソッド
│   └── ProjectManagementServiceMockBuilder.cs  # モックファクトリ
└── ProjectManagement/
    └── ProjectListTests.cs                      # Stage4-A検証用1テスト
```

#### .csprojファイル設計
```xml
<Project Sdk="Microsoft.NET.Sdk.Razor">  <!-- 重要: Razorプロジェクト -->
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
  </PropertyGroup>

  <ItemGroup>
    <!-- bUnit本体 -->
    <PackageReference Include="bunit" Version="1.40.0" />
    <PackageReference Include="bunit.web" Version="1.40.0" />

    <!-- テストフレームワーク -->
    <PackageReference Include="xunit" Version="2.9.0" />
    <PackageReference Include="xunit.runner.visualstudio" Version="2.8.2">
      <PrivateAssets>all</PrivateAssets>
      <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
    </PackageReference>
    <PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.11.0" />

    <!-- モック・アサーション -->
    <PackageReference Include="Moq" Version="4.20.72" />
    <PackageReference Include="FluentAssertions" Version="6.12.1" />
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include="..\..\src\UbiquitousLanguageManager.Web\UbiquitousLanguageManager.Web.csproj" />
  </ItemGroup>
</Project>
```

#### _Imports.razorファイル設計
```csharp
@using Bunit
@using Bunit.TestDoubles
@using Xunit
@using FluentAssertions
@using Moq
@using Microsoft.Extensions.DependencyInjection
@using Microsoft.AspNetCore.Components
@using Microsoft.AspNetCore.Components.Authorization
@using Microsoft.FSharp.Core
@using UbiquitousLanguageManager.Application.ProjectManagement
@using UbiquitousLanguageManager.Contracts.DTOs
@using UbiquitousLanguageManager.Contracts.DTOs.Application
@using UbiquitousLanguageManager.Domain.Common
@using UbiquitousLanguageManager.Web.Tests.Infrastructure
```

### 2.2 BlazorComponentTestBase設計

#### クラス概要
**目的**: 全UIテストで共通するセットアップを基底クラスに集約

**主要機能**:
1. TestContext継承（bUnit基底クラス）
2. 認証コンテキスト自動初期化（`FakeAuthenticationStateProvider`）
3. IProjectManagementServiceモック自動登録
4. 権限別ユーザー設定ヘルパーメソッド

#### 実装コード設計
```csharp
using Bunit;
using Bunit.TestDoubles;
using Moq;
using Microsoft.Extensions.DependencyInjection;
using UbiquitousLanguageManager.Application.ProjectManagement;
using UbiquitousLanguageManager.Domain.Common;

namespace UbiquitousLanguageManager.Web.Tests.Infrastructure;

/// <summary>
/// Blazor Serverコンポーネントテストの基底クラス
///
/// 【機能】
/// - TestContext継承（bUnit基盤）
/// - 認証コンテキスト自動初期化
/// - IProjectManagementServiceモック自動登録
/// - 権限別ユーザー設定ヘルパー
///
/// 【使用例】
/// public class ProjectListTests : BlazorComponentTestBase
/// {
///     [Fact]
///     public void Should_Display_Projects_For_SuperUser()
///     {
///         // Arrange
///         SetupSuperUser("admin@test.com");
///         SetupGetProjectsSuccess(testProjects);
///
///         // Act
///         var cut = RenderComponent<ProjectList>();
///
///         // Assert
///         cut.FindAll(".project-row").Should().HaveCount(2);
///     }
/// }
/// </summary>
public abstract class BlazorComponentTestBase : TestContext
{
    /// <summary>
    /// 認証コンテキスト（権限制御テスト用）
    /// </summary>
    protected TestAuthorizationContext AuthContext { get; private set; } = null!;

    /// <summary>
    /// IProjectManagementServiceモック
    /// </summary>
    protected Mock<IProjectManagementService> MockProjectService { get; private set; } = null!;

    /// <summary>
    /// コンストラクタ（共通初期化）
    ///
    /// 【初期化内容】
    /// 1. 認証コンテキスト作成（デフォルト: 未認証）
    /// 2. IProjectManagementServiceモック作成・DI登録
    /// 3. その他必要なサービスのモック登録
    /// </summary>
    protected BlazorComponentTestBase()
    {
        // 認証コンテキスト初期化（bUnit標準機能）
        AuthContext = this.AddTestAuthorization();

        // IProjectManagementServiceモック作成・登録
        MockProjectService = new Mock<IProjectManagementService>();
        Services.AddSingleton(MockProjectService.Object);

        // NavigationManager（bUnit標準FakeNavigationManager自動登録済み）
    }

    #region 権限別ユーザー設定ヘルパー

    /// <summary>
    /// SuperUser権限ユーザー設定
    /// </summary>
    /// <param name="email">ユーザーメールアドレス（デフォルト: superuser@test.com）</param>
    protected void SetupSuperUser(string email = "superuser@test.com")
    {
        AuthContext.SetAuthorized(email);
        AuthContext.SetRoles("SuperUser");
    }

    /// <summary>
    /// ProjectManager権限ユーザー設定
    /// </summary>
    /// <param name="email">ユーザーメールアドレス（デフォルト: pm@test.com）</param>
    protected void SetupProjectManager(string email = "pm@test.com")
    {
        AuthContext.SetAuthorized(email);
        AuthContext.SetRoles("ProjectManager");
    }

    /// <summary>
    /// DomainApprover権限ユーザー設定（Phase B2対応）
    /// </summary>
    /// <param name="email">ユーザーメールアドレス</param>
    protected void SetupDomainApprover(string email = "approver@test.com")
    {
        AuthContext.SetAuthorized(email);
        AuthContext.SetRoles("DomainApprover");
    }

    /// <summary>
    /// GeneralUser権限ユーザー設定（Phase B2対応）
    /// </summary>
    /// <param name="email">ユーザーメールアドレス</param>
    protected void SetupGeneralUser(string email = "user@test.com")
    {
        AuthContext.SetAuthorized(email);
        AuthContext.SetRoles("GeneralUser");
    }

    /// <summary>
    /// 未認証ユーザー設定
    /// </summary>
    protected void SetupUnauthenticatedUser()
    {
        // デフォルトで未認証のため何もしない
        // 明示的に呼び出す場合のために用意
    }

    #endregion

    #region IProjectManagementServiceモックセットアップヘルパー

    /// <summary>
    /// GetProjectsAsync成功モックセットアップ
    ///
    /// 【使用例】
    /// SetupGetProjectsSuccess(new List<ProjectDto>
    /// {
    ///     new ProjectDto { Id = 1, ProjectName = "テストPJ", IsActive = true }
    /// });
    /// </summary>
    protected void SetupGetProjectsSuccess(List<ProjectDto> projects, int totalCount = 0)
    {
        var builder = new ProjectManagementServiceMockBuilder();
        MockProjectService = builder
            .SetupGetProjectsSuccess(projects, totalCount > 0 ? totalCount : projects.Count)
            .BuildMock();

        // サービス再登録（既存のモックを置き換え）
        Services.AddSingleton(MockProjectService.Object);
    }

    /// <summary>
    /// GetProjectsAsync失敗モックセットアップ
    /// </summary>
    protected void SetupGetProjectsFailure(string errorMessage)
    {
        var builder = new ProjectManagementServiceMockBuilder();
        MockProjectService = builder
            .SetupGetProjectsFailure(errorMessage)
            .BuildMock();

        Services.AddSingleton(MockProjectService.Object);
    }

    /// <summary>
    /// CreateProjectAsync成功モックセットアップ
    /// </summary>
    protected void SetupCreateProjectSuccess(ProjectDto createdProject)
    {
        var builder = new ProjectManagementServiceMockBuilder();
        MockProjectService = builder
            .SetupCreateProjectSuccess(createdProject)
            .BuildMock();

        Services.AddSingleton(MockProjectService.Object);
    }

    /// <summary>
    /// DeleteProjectAsync成功モックセットアップ
    /// </summary>
    protected void SetupDeleteProjectSuccess()
    {
        var builder = new ProjectManagementServiceMockBuilder();
        MockProjectService = builder
            .SetupDeleteProjectSuccess()
            .BuildMock();

        Services.AddSingleton(MockProjectService.Object);
    }

    #endregion
}
```

### 2.3 FSharpTypeHelpers設計

#### クラス概要
**目的**: F# Result/Option型のC#からの生成・検証を簡潔にする拡張メソッド提供

**主要機能**:
1. `FSharpResult<T, string>`生成ヘルパー（Ok/Error）
2. `FSharpOption<T>`生成ヘルパー（Some/None）
3. Result型検証ヘルパー（IsOk/IsError判定）
4. Option型検証ヘルパー（IsSome/IsNone判定）

#### 実装コード設計
```csharp
using Microsoft.FSharp.Core;

namespace UbiquitousLanguageManager.Web.Tests.Infrastructure;

/// <summary>
/// F#型システムとC#の統合を簡潔にする拡張メソッド集
///
/// 【対象型】
/// - FSharpResult&lt;T, string&gt;: F# Result型（成功/失敗）
/// - FSharpOption&lt;T&gt;: F# Option型（値あり/なし）
///
/// 【使用例】
/// // Result型生成
/// var okResult = projectDto.ToOkResult();
/// var errorResult = "エラーメッセージ".ToErrorResult&lt;ProjectDto&gt;();
///
/// // Option型生成
/// var someValue = "検索キーワード".ToSome();
/// var noneValue = FSharpTypeHelpers.ToNone&lt;string&gt;();
///
/// // Result型検証
/// result.IsOk.Should().BeTrue();
/// result.ErrorValue.Should().Be("予期されるエラー");
/// </summary>
public static class FSharpTypeHelpers
{
    #region FSharpResult 生成ヘルパー

    /// <summary>
    /// C#オブジェクトをF# Result型のOk値に変換
    /// </summary>
    /// <typeparam name="T">成功値の型</typeparam>
    /// <param name="value">成功値</param>
    /// <returns>FSharpResult&lt;T, string&gt;.NewOk</returns>
    public static FSharpResult<T, string> ToOkResult<T>(this T value)
        => FSharpResult<T, string>.NewOk(value);

    /// <summary>
    /// エラーメッセージをF# Result型のError値に変換
    /// </summary>
    /// <typeparam name="T">成功値の型（Errorには含まれない）</typeparam>
    /// <param name="errorMessage">エラーメッセージ</param>
    /// <returns>FSharpResult&lt;T, string&gt;.NewError</returns>
    public static FSharpResult<T, string> ToErrorResult<T>(this string errorMessage)
        => FSharpResult<T, string>.NewError(errorMessage);

    #endregion

    #region FSharpOption 生成ヘルパー

    /// <summary>
    /// C#オブジェクトをF# Option型のSome値に変換
    /// </summary>
    /// <typeparam name="T">値の型</typeparam>
    /// <param name="value">値</param>
    /// <returns>FSharpOption&lt;T&gt;.Some</returns>
    public static FSharpOption<T> ToSome<T>(this T value)
        => FSharpOption<T>.Some(value);

    /// <summary>
    /// F# Option型のNone値を生成
    ///
    /// 【重要】
    /// 型推論が効かない場合は明示的に型指定必須
    /// 例: var noneValue = FSharpTypeHelpers.ToNone&lt;string&gt;();
    /// </summary>
    /// <typeparam name="T">値の型</typeparam>
    /// <returns>FSharpOption&lt;T&gt;.None</returns>
    public static FSharpOption<T> ToNone<T>()
        => FSharpOption<T>.None;

    #endregion

    #region FSharpOption 検証ヘルパー

    /// <summary>
    /// Option型がSome値か判定
    /// </summary>
    public static bool IsSome<T>(this FSharpOption<T> option)
        => FSharpOption<T>.get_IsSome(option);

    /// <summary>
    /// Option型がNone値か判定
    /// </summary>
    public static bool IsNone<T>(this FSharpOption<T> option)
        => FSharpOption<T>.get_IsNone(option);

    /// <summary>
    /// Option型から値を取得（None時はデフォルト値）
    /// </summary>
    public static T GetValueOrDefault<T>(this FSharpOption<T> option, T defaultValue = default!)
        => option.IsSome() ? option.Value : defaultValue;

    #endregion
}
```

### 2.4 ProjectManagementServiceMockBuilder設計

#### クラス概要
**目的**: IProjectManagementServiceのモック設定をFluent APIで簡潔に記述

**主要機能**:
1. GetProjectsAsync成功/失敗モック
2. CreateProjectAsync成功/失敗モック
3. UpdateProjectAsync成功/失敗モック
4. DeleteProjectAsync成功/失敗モック
5. Fluent API（メソッドチェーン）対応

#### 実装コード設計
```csharp
using Moq;
using Microsoft.FSharp.Core;
using UbiquitousLanguageManager.Application.ProjectManagement;
using UbiquitousLanguageManager.Contracts.DTOs;
using UbiquitousLanguageManager.Contracts.DTOs.Application;

namespace UbiquitousLanguageManager.Web.Tests.Infrastructure;

/// <summary>
/// IProjectManagementServiceモック作成ビルダー（Fluent API）
///
/// 【使用例】
/// var mockService = new ProjectManagementServiceMockBuilder()
///     .SetupGetProjectsSuccess(testProjects, totalCount: 10)
///     .SetupCreateProjectSuccess(createdProject)
///     .Build();
///
/// Services.AddSingleton(mockService);
/// </summary>
public class ProjectManagementServiceMockBuilder
{
    private readonly Mock<IProjectManagementService> _mockService;

    public ProjectManagementServiceMockBuilder()
    {
        _mockService = new Mock<IProjectManagementService>();
    }

    #region GetProjectsAsync モックセットアップ

    /// <summary>
    /// GetProjectsAsync成功モックセットアップ
    ///
    /// 【引数】
    /// - projects: 返却するプロジェクトリスト
    /// - totalCount: 総件数（ページング用・デフォルト: projects.Count）
    ///
    /// 【戻り値】
    /// FSharpResult&lt;ProjectListResultDto, string&gt;.NewOk
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

        var fsharpResult = resultDto.ToOkResult();

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
        var fsharpResult = errorMessage.ToErrorResult<ProjectListResultDto>();

        _mockService
            .Setup(s => s.GetProjectsAsync(It.IsAny<GetProjectsQuery>()))
            .ReturnsAsync(fsharpResult);

        return this;
    }

    #endregion

    #region CreateProjectAsync モックセットアップ

    /// <summary>
    /// CreateProjectAsync成功モックセットアップ
    ///
    /// 【引数】
    /// - createdProject: 作成されたプロジェクト
    ///
    /// 【戻り値】
    /// FSharpResult&lt;ProjectCreationResultDto, string&gt;.NewOk
    /// </summary>
    public ProjectManagementServiceMockBuilder SetupCreateProjectSuccess(ProjectDto createdProject)
    {
        var resultDto = new ProjectCreationResultDto
        {
            IsSuccess = true,
            Project = createdProject,
            DefaultDomain = new DomainDto
            {
                DomainId = 1,
                DomainName = "共通",
                Description = "デフォルトドメイン",
                IsDefault = true,
                IsActive = true,
                ProjectId = createdProject.ProjectId
            }
        };

        var fsharpResult = resultDto.ToOkResult();

        _mockService
            .Setup(s => s.CreateProjectAsync(It.IsAny<CreateProjectCommand>()))
            .ReturnsAsync(fsharpResult);

        return this;
    }

    /// <summary>
    /// CreateProjectAsync失敗モックセットアップ
    /// </summary>
    public ProjectManagementServiceMockBuilder SetupCreateProjectFailure(string errorMessage)
    {
        var fsharpResult = errorMessage.ToErrorResult<ProjectCreationResultDto>();

        _mockService
            .Setup(s => s.CreateProjectAsync(It.IsAny<CreateProjectCommand>()))
            .ReturnsAsync(fsharpResult);

        return this;
    }

    #endregion

    #region UpdateProjectAsync モックセットアップ

    /// <summary>
    /// UpdateProjectAsync成功モックセットアップ
    /// </summary>
    public ProjectManagementServiceMockBuilder SetupUpdateProjectSuccess(ProjectDto updatedProject)
    {
        var fsharpResult = updatedProject.ToOkResult();

        _mockService
            .Setup(s => s.UpdateProjectAsync(It.IsAny<UpdateProjectCommand>()))
            .ReturnsAsync(fsharpResult);

        return this;
    }

    /// <summary>
    /// UpdateProjectAsync失敗モックセットアップ
    /// </summary>
    public ProjectManagementServiceMockBuilder SetupUpdateProjectFailure(string errorMessage)
    {
        var fsharpResult = errorMessage.ToErrorResult<ProjectDto>();

        _mockService
            .Setup(s => s.UpdateProjectAsync(It.IsAny<UpdateProjectCommand>()))
            .ReturnsAsync(fsharpResult);

        return this;
    }

    #endregion

    #region DeleteProjectAsync モックセットアップ

    /// <summary>
    /// DeleteProjectAsync成功モックセットアップ
    ///
    /// 【戻り値】
    /// FSharpResult&lt;Unit, string&gt;.NewOk（F# Unit型）
    /// </summary>
    public ProjectManagementServiceMockBuilder SetupDeleteProjectSuccess()
    {
        var unitValue = Microsoft.FSharp.Core.Unit.Default;
        var fsharpResult = FSharpResult<Microsoft.FSharp.Core.Unit, string>.NewOk(unitValue);

        _mockService
            .Setup(s => s.DeleteProjectAsync(It.IsAny<DeleteProjectCommand>()))
            .ReturnsAsync(fsharpResult);

        return this;
    }

    /// <summary>
    /// DeleteProjectAsync失敗モックセットアップ
    /// </summary>
    public ProjectManagementServiceMockBuilder SetupDeleteProjectFailure(string errorMessage)
    {
        var fsharpResult = errorMessage.ToErrorResult<Microsoft.FSharp.Core.Unit>();

        _mockService
            .Setup(s => s.DeleteProjectAsync(It.IsAny<DeleteProjectCommand>()))
            .ReturnsAsync(fsharpResult);

        return this;
    }

    #endregion

    #region ビルド

    /// <summary>
    /// モックインスタンス取得（IProjectManagementService）
    /// </summary>
    public IProjectManagementService Build() => _mockService.Object;

    /// <summary>
    /// Mockオブジェクト取得（検証用）
    ///
    /// 【使用例】
    /// var mock = builder.BuildMock();
    /// mock.Verify(s => s.GetProjectsAsync(It.IsAny&lt;GetProjectsQuery&gt;()), Times.Once);
    /// </summary>
    public Mock<IProjectManagementService> BuildMock() => _mockService;

    #endregion
}
```

---

## Part 3: 10テストケース移行マッピング

### 3.1 移行対応表

| # | API統合テスト名 | bUnit UIテスト名 | 画面 | 移行難易度 | F#型変換パターン | 推定時間 |
|---|----------------|-----------------|------|-----------|----------------|---------|
| 1 | CreateProject_SuperUserWithValidData_Returns201 | ProjectCreate_SuperUser_SubmitsValidForm_ShowsSuccessMessage | ProjectCreate.razor | 中 | CreateProjectCommand Record型・Result型検証 | 20分 |
| 2 | CreateProject_SuperUserWithDuplicateName_Returns400 | ProjectCreate_DuplicateName_ShowsErrorMessage | ProjectCreate.razor | 中 | Result Error型検証 | 15分 |
| 3 | CreateProject_ProjectManager_Returns403 | ProjectCreate_ProjectManager_HidesCreateButton | ProjectCreate.razor | 低 | 権限制御テスト（SecureButton非表示確認） | 10分 |
| 4 | GetProjects_SuperUser_ReturnsAllProjects | ProjectList_SuperUser_DisplaysAllProjects | ProjectList.razor | 高 | GetProjectsQuery Record型・FSharpOption検索キーワード | 25分 |
| 5 | GetProjects_ProjectManager_ReturnsOnlyAssignedProjects | ProjectList_ProjectManager_DisplaysFilteredProjects | ProjectList.razor | 高 | GetProjectsQuery Record型・権限フィルタリング検証 | 25分 |
| 6 | UpdateProject_SuperUserUpdatesDescription_Returns200 | ProjectEdit_SuperUser_UpdatesDescription_ShowsSuccess | ProjectEdit.razor | 中 | UpdateProjectCommand Record型・NavigationManager検証 | 20分 |
| 7 | UpdateProject_ProjectManagerUpdatesAssignedProject_Returns200 | ProjectEdit_ProjectManager_UpdatesOwnedProject_ShowsSuccess | ProjectEdit.razor | 中 | UpdateProjectCommand Record型・権限制御統合 | 20分 |
| 8 | DeleteProject_SuperUser_Returns200AndSetsIsActiveFalse | ProjectList_SuperUser_DeleteConfirm_ShowsSuccessToast | ProjectList.razor | 低 | DeleteProjectCommand Record型・Toast表示検証 | 15分 |
| 9 | DeleteProject_ProjectManager_Returns403 | ProjectList_ProjectManager_NoDeleteButton | ProjectList.razor | 低 | 権限制御テスト（削除ボタン非表示確認） | 10分 |
| 10 | CreateProject_AutomaticallyCreatesDefaultDomain | ProjectCreate_SuccessMessage_MentionsDefaultDomain | ProjectCreate.razor | 中 | ProjectCreationResultDto検証・DefaultDomain確認 | 15分 |

**合計推定時間**: 175分（約3時間）

### 3.2 テストケース分類

#### グループA: ProjectCreate系（3件・45分）
**対象画面**: `ProjectCreate.razor`

1. **ProjectCreate_SuperUser_SubmitsValidForm_ShowsSuccessMessage**
   - **検証内容**: フォーム入力→送信→成功メッセージ表示→リダイレクト
   - **F#型変換**: `CreateProjectCommand` Record型コンストラクタ生成
   - **Assert内容**:
     - CreateProjectAsync呼び出し確認
     - NavigationManager.Uri == "/projects" 確認
     - Toast表示確認（JSRuntimeモック必要）

2. **ProjectCreate_DuplicateName_ShowsErrorMessage**
   - **検証内容**: 重複名入力→送信→エラーメッセージ表示
   - **F#型変換**: Result Error型検証
   - **Assert内容**:
     - エラーメッセージ表示確認
     - リダイレクトなし確認

3. **ProjectCreate_ProjectManager_HidesCreateButton**
   - **検証内容**: ProjectManager権限時に作成ボタン非表示
   - **F#型変換**: なし（権限制御のみ）
   - **Assert内容**: SecureButtonコンポーネント非表示確認

#### グループB: ProjectList系（4件・70分）
**対象画面**: `ProjectList.razor`

4. **ProjectList_SuperUser_DisplaysAllProjects**
   - **検証内容**: SuperUser権限→全プロジェクト表示
   - **F#型変換**: `GetProjectsQuery` Record型コンストラクタ・`FSharpOption<string>.None`検索キーワード
   - **Assert内容**:
     - テーブル行数確認
     - プロジェクト名表示確認

5. **ProjectList_ProjectManager_DisplaysFilteredProjects**
   - **検証内容**: ProjectManager権限→担当プロジェクトのみ表示
   - **F#型変換**: `GetProjectsQuery` Record型・権限フィルタリング
   - **Assert内容**:
     - 表示プロジェクト数確認
     - 担当外プロジェクト非表示確認

8. **ProjectList_SuperUser_DeleteConfirm_ShowsSuccessToast**
   - **検証内容**: 削除ボタンクリック→確認ダイアログ→削除成功→Toast表示
   - **F#型変換**: `DeleteProjectCommand` Record型
   - **Assert内容**:
     - DeleteProjectAsync呼び出し確認
     - Toast表示確認（成功メッセージ）

9. **ProjectList_ProjectManager_NoDeleteButton**
   - **検証内容**: ProjectManager権限時に削除ボタン非表示
   - **F#型変換**: なし（権限制御のみ）
   - **Assert内容**: 削除ボタン非存在確認

#### グループC: ProjectEdit系（2件・40分）
**対象画面**: `ProjectEdit.razor`

6. **ProjectEdit_SuperUser_UpdatesDescription_ShowsSuccess**
   - **検証内容**: 説明フィールド編集→保存→成功メッセージ→リダイレクト
   - **F#型変換**: `UpdateProjectCommand` Record型コンストラクタ
   - **Assert内容**:
     - UpdateProjectAsync呼び出し確認
     - NavigationManager.Uri == "/projects" 確認

7. **ProjectEdit_ProjectManager_UpdatesOwnedProject_ShowsSuccess**
   - **検証内容**: ProjectManager権限→担当プロジェクト編集成功
   - **F#型変換**: `UpdateProjectCommand` Record型・権限制御統合
   - **Assert内容**: 同上

#### グループD: デフォルトドメイン（1件・15分）

10. **ProjectCreate_SuccessMessage_MentionsDefaultDomain**
    - **検証内容**: プロジェクト作成成功時に「デフォルトドメイン『共通』を作成しました」メッセージ表示
    - **F#型変換**: `ProjectCreationResultDto`検証・DefaultDomain確認
    - **Assert内容**: Toast/通知メッセージに「共通」含まれる確認

### 3.3 F#型変換パターン詳細

#### パターン1: F# Record型コンストラクタ生成（重要）
**対象**: CreateProjectCommand, GetProjectsQuery, UpdateProjectCommand, DeleteProjectCommand

**問題**: F# Recordは不変型のため、C#のオブジェクト初期化子が使えない
```csharp
// ❌ 誤ったパターン（Stage3で36件エラー発生）
var query = new GetProjectsQuery
{
    UserId = currentUser.Id,  // Error: Read-only property
    UserRole = currentUserRole,
    // ...
};
```

**解決**: コンストラクタベース初期化（Stage3で確立したパターン）
```csharp
// ✅ 正しいパターン
var query = new GetProjectsQuery(
    userId: currentUser.Id,
    userRole: currentUserRole,
    pageNumber: 1,
    pageSize: 20,
    includeInactive: false,
    searchKeyword: FSharpOption<string>.None  // Option型はNone明示
);
```

#### パターン2: FSharpOption型生成（重要）
**対象**: searchKeyword, description等の省略可能フィールド

**None生成**: 型推論が効かないため明示的型指定必須
```csharp
// ✅ 正しいパターン
FSharpOption<string>.None

// または拡張メソッド活用
FSharpTypeHelpers.ToNone<string>()
```

**Some生成**:
```csharp
// ✅ 正しいパターン
FSharpOption<string>.Some("検索キーワード")

// または拡張メソッド活用
"検索キーワード".ToSome()
```

#### パターン3: F# Result型検証
**対象**: 全Service呼び出し結果

**IsOkプロパティ直接アクセス**（Stage3で確立したパターン）:
```csharp
var result = await ProjectManagementService.GetProjectsAsync(query);

// ✅ 正しいパターン
if (result.IsOk)
{
    var listResult = result.ResultValue;
    projects = listResult.Projects.ToList();
}
else
{
    errorMessage = result.ErrorValue;
}
```

#### パターン4: F# Discriminated Union（Role型）
**対象**: UserRole権限判定

**switch式パターンマッチング**（Stage3で確立したパターン）:
```csharp
currentUserRole = roleClaim.Value switch
{
    "SuperUser" => Role.SuperUser,
    "ProjectManager" => Role.ProjectManager,
    "DomainApprover" => Role.DomainApprover,
    "GeneralUser" => Role.GeneralUser,
    _ => Role.GeneralUser  // デフォルト値
};
```

---

## Part 4: Phase別実装ステップ

### Phase 1: テストインフラ実装（推定1時間）

#### 作業内容
1. **テストプロジェクト作成**（10分）
   - `.csproj`ファイル作成（Microsoft.NET.Sdk.Razor）
   - NuGetパッケージ導入（bunit, Moq, FluentAssertions）
   - `_Imports.razor`作成

2. **BlazorComponentTestBase実装**（20分）
   - TestContext継承
   - 認証コンテキスト初期化
   - モックサービス登録
   - 権限別ユーザー設定ヘルパーメソッド

3. **FSharpTypeHelpers実装**（15分）
   - Result型生成・検証拡張メソッド
   - Option型生成・検証拡張メソッド

4. **ProjectManagementServiceMockBuilder実装**（15分）
   - GetProjectsAsyncモックセットアップ
   - CreateProjectAsyncモックセットアップ
   - UpdateProjectAsyncモックセットアップ
   - DeleteProjectAsyncモックセットアップ

#### 成果物
- `/tests/UbiquitousLanguageManager.Web.Tests/UbiquitousLanguageManager.Web.Tests.csproj`
- `/tests/UbiquitousLanguageManager.Web.Tests/_Imports.razor`
- `/tests/UbiquitousLanguageManager.Web.Tests/Infrastructure/BlazorComponentTestBase.cs`
- `/tests/UbiquitousLanguageManager.Web.Tests/Infrastructure/FSharpTypeHelpers.cs`
- `/tests/UbiquitousLanguageManager.Web.Tests/Infrastructure/ProjectManagementServiceMockBuilder.cs`

#### 品質基準
- ビルド成功（0 Warning/0 Error）
- インフラクラス単体コンパイル成功

### Phase 2: 基本テスト実装（推定1時間）

#### 作業内容（グループA+B前半）
1. **ProjectCreate系3件実装**（45分）
   - ProjectCreate_SuperUser_SubmitsValidForm_ShowsSuccessMessage（20分）
   - ProjectCreate_DuplicateName_ShowsErrorMessage（15分）
   - ProjectCreate_ProjectManager_HidesCreateButton（10分）

2. **ProjectList系2件実装**（25分）
   - ProjectList_SuperUser_DisplaysAllProjects（15分）
   - ProjectList_ProjectManager_DisplaysFilteredProjects（10分）

#### 成果物
- `/tests/UbiquitousLanguageManager.Web.Tests/ProjectManagement/ProjectCreateTests.cs`（3テスト）
- `/tests/UbiquitousLanguageManager.Web.Tests/ProjectManagement/ProjectListTests.cs`（2テスト）

#### 品質基準
- 5テスト成功（Green）
- カバレッジ: Create系機能60%以上

### Phase 3: 拡張テスト実装（推定0.5-1時間）

#### 作業内容（グループB後半+C+D）
1. **ProjectList系2件追加実装**（25分）
   - ProjectList_SuperUser_DeleteConfirm_ShowsSuccessToast（15分）
   - ProjectList_ProjectManager_NoDeleteButton（10分）

2. **ProjectEdit系2件実装**（40分）
   - ProjectEdit_SuperUser_UpdatesDescription_ShowsSuccess（20分）
   - ProjectEdit_ProjectManager_UpdatesOwnedProject_ShowsSuccess（20分）

3. **デフォルトドメイン系1件実装**（15分）
   - ProjectCreate_SuccessMessage_MentionsDefaultDomain（15分）

#### 成果物
- `/tests/UbiquitousLanguageManager.Web.Tests/ProjectManagement/ProjectListTests.cs`（+2テスト・合計4テスト）
- `/tests/UbiquitousLanguageManager.Web.Tests/ProjectManagement/ProjectEditTests.cs`（2テスト）
- `/tests/UbiquitousLanguageManager.Web.Tests/ProjectManagement/ProjectCreateTests.cs`（+1テスト・合計4テスト）

#### 品質基準
- 全10テスト成功（Green）
- カバレッジ: CRUD機能80%以上
- 権限制御マトリックス: SuperUser/ProjectManager（10パターン）完全カバー

---

## Part 5: リスク分析・対応策

### 5.1 技術的リスク

#### リスク1: F# Record型コンストラクタパターン複雑性
**影響度**: 高
**発生確率**: 高

**問題内容**:
- Stage3で36件の型変換エラー発生実績あり
- GetProjectsQuery等のRecord型生成でコンストラクタ引数順序間違い可能性
- FSharpOption型の型推論失敗可能性

**対応策**:
1. **標準パターン厳守**:
   ```csharp
   var query = new GetProjectsQuery(
       userId: value1,         // 名前付き引数必須
       userRole: value2,
       pageNumber: value3,
       // ...
   );
   ```

2. **FSharpTypeHelpers活用**:
   ```csharp
   searchKeyword: string.IsNullOrEmpty(searchTerm)
       ? FSharpTypeHelpers.ToNone<string>()  // 型明示
       : searchTerm.ToSome()
   ```

3. **段階的ビルド確認**: 1テスト実装ごとにビルド・実行確認

#### リスク2: Blazor Server非同期処理タイミング
**影響度**: 中
**発生確率**: 中

**問題内容**:
- Blazor ServerはSignalR非同期通信のためレンダリングタイミング不確定
- `StateHasChanged`呼び出し後のDOM更新待機が必要な場合あり
- bUnit `WaitForState`/`WaitForAssertion`の適切な使用が必要

**対応策**:
1. **非同期待機パターン活用**:
   ```csharp
   var cut = RenderComponent<ProjectList>();

   // DOM更新待機（最大5秒）
   cut.WaitForState(() => cut.FindAll(".project-row").Count == 2, timeout: TimeSpan.FromSeconds(5));

   // または
   cut.WaitForAssertion(() =>
       cut.FindAll(".project-row").Should().HaveCount(2),
       timeout: TimeSpan.FromSeconds(5)
   );
   ```

2. **InvokeAsync明示的呼び出し**:
   ```csharp
   await cut.InvokeAsync(async () =>
   {
       // 非同期処理トリガー
   });
   ```

#### リスク3: AuthenticationStateProviderモック不整合
**影響度**: 中
**発生確率**: 低

**問題内容**:
- 権限制御テストで認証コンテキストが正しく設定されない可能性
- `[Authorize]`属性のテスト制限（bUnitは属性自体の動作検証不可）

**対応策**:
1. **FakeAuthenticationStateProvider標準活用**:
   ```csharp
   AuthContext.SetAuthorized("user@test.com");
   AuthContext.SetRoles("SuperUser");
   ```

2. **権限制御ロジック分離テスト**:
   - `[Authorize]`属性存在確認は別途メタデータテスト実施
   - UIレベルでは権限別表示制御ロジックのテストに集中

3. **SecureButtonコンポーネント活用**:
   - 既存の権限制御コンポーネント活用により実装実績あり

#### リスク4: NavigationManagerモック・リダイレクト検証
**影響度**: 低
**発生確率**: 低

**問題内容**:
- 作成/編集成功後の`NavigationManager.NavigateTo("/projects")`呼び出し検証
- bUnit FakeNavigationManagerのUri変更確認方法

**対応策**:
1. **FakeNavigationManager活用**（bUnit標準機能）:
   ```csharp
   var navMan = Services.GetRequiredService<NavigationManager>();

   // ボタンクリック等のアクション実行
   var saveButton = cut.Find("button.save");
   saveButton.Click();

   // リダイレクト先確認
   navMan.Uri.Should().EndWith("/projects");
   ```

### 5.2 プロセス的リスク

#### リスク5: コンテキスト消費超過（AutoCompact発生）
**影響度**: 高
**発生確率**: 低

**推定コンテキスト消費**:
- Phase 1: +8,000トークン
- Phase 2: +10,000トークン
- Phase 3: +7,000トークン
- **合計**: +25,000トークン（現在106,000 → 完了時131,000・65.5%）

**対応策**:
1. **Phase完了時の中間確認**: ユーザーに進捗報告・次Phase継続判断
2. **段階的コミット**: Phase 1完了時点でビルド成功確認・一旦記録
3. **次セッション送り判断**: コンテキスト70%超過時は残りPhase次セッション送り

#### リスク6: テスト実装時の予期しないエラー
**影響度**: 中
**発生確率**: 中

**想定エラー**:
- Razor構文エラー（bUnit側の制約）
- F#型変換の新たなパターン発見
- Blazor Serverライフサイクル理解不足

**対応策**:
1. **1テストずつ段階的実装**: ビルド・実行成功確認後に次テスト着手
2. **エラー発生時のSubAgent活用**:
   - unit-test Agent (Fix-Mode): 構文エラー修正
   - integration-test Agent: テスト実行エラー分析
3. **技術調査文書参照**: `Step07_bUnit技術調査.md`の実装パターン活用

---

## Part 6: 次セッション引き継ぎ事項

### 6.1 Stage4-A完了状態（本計画書作成完了時）

#### 完了事項
- ✅ 詳細実装計画書作成完了（本ファイル）
- ✅ テストプロジェクト初期構築完了
- ✅ テストインフラ3クラス実装完了
- ✅ 1テストケース実装・動作確認完了

#### 成果物
- `/Doc/08_Organization/Active/Phase_B1/Planning/Step07_Stage4B_実装計画.md`（本ファイル）
- `/tests/UbiquitousLanguageManager.Web.Tests/`配下の全ファイル
- ビルド成功・1テスト成功確認済み

### 6.2 Stage4-B実施時の必須参照

#### 必須参照ファイル
1. **本計画書**: `/Doc/08_Organization/Active/Phase_B1/Planning/Step07_Stage4B_実装計画.md`
   - Part 3: 10テストケース移行マッピング（実装優先順位）
   - Part 4: Phase別実装ステップ（Phase 2-3詳細）
   - Part 5: リスク分析・対応策（エラー対応パターン）

2. **技術調査結果**: `/Doc/08_Organization/Active/Phase_B1/Research/Step07_bUnit技術調査.md`
   - bUnit基本パターン
   - F#型統合詳細
   - 認証・権限制御テストパターン

3. **テスト対象実装**: `/src/UbiquitousLanguageManager.Web/Components/Pages/ProjectManagement/*.razor`
   - ProjectList.razor（一覧画面）
   - ProjectCreate.razor（作成画面）
   - ProjectEdit.razor（編集画面）

#### 実装優先順位
1. **Phase 2**: グループA+B前半（5テスト・1時間）
2. **Phase 3**: グループB後半+C+D（5テスト・1時間）

### 6.3 品質基準

#### Stage4-B完了基準
- ✅ 全10テスト成功（Green）
- ✅ ビルド: 0 Warning/0 Error
- ✅ テストカバレッジ: CRUD機能80%以上
- ✅ 権限制御マトリックス: SuperUser/ProjectManager（10パターン）完全カバー

#### Phase B1完了判断
- Stage4-B完了 → Stage5（品質チェック）→ Stage6（統合確認）
- 仕様準拠度: 95-100点達成維持
- Clean Architecture: 97点品質維持

---

**計画書作成日**: 2025-10-05
**計画策定Agent**: MainAgent
**次セッション実施予定**: Stage4-B本格実装（Phase 2-3）
**推定所要時間**: 2.5-3時間
