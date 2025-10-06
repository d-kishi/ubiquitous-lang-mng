using Bunit;
using Bunit.TestDoubles;
using Moq;
using Microsoft.Extensions.DependencyInjection;
using AppIProjectManagementService = UbiquitousLanguageManager.Application.ProjectManagement.IProjectManagementService;
using UbiquitousLanguageManager.Domain.Common;
using UbiquitousLanguageManager.Contracts.DTOs;
// F# Domain型をエイリアスで使用
using FSharpDomainProject = UbiquitousLanguageManager.Domain.ProjectManagement.Project;
using FSharpDomainDomain = UbiquitousLanguageManager.Domain.ProjectManagement.Domain;

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
    protected Mock<AppIProjectManagementService> MockProjectService { get; private set; } = null!;

    /// <summary>
    /// コンストラクタ（共通初期化）
    ///
    /// 【初期化内容】
    /// 1. 認証コンテキスト作成（デフォルト: 未認証）
    /// 2. IProjectManagementServiceモック作成・DI登録
    /// 3. JSRuntimeモック設定（Toast表示等のJavaScript相互運用対応）
    /// 4. その他必要なサービスのモック登録
    /// </summary>
    protected BlazorComponentTestBase()
    {
        // 認証コンテキスト初期化（bUnit標準機能）
        AuthContext = this.AddTestAuthorization();

        // IProjectManagementServiceモック作成・登録
        MockProjectService = new Mock<AppIProjectManagementService>();
        Services.AddSingleton(MockProjectService.Object);

        // NavigationManager（bUnit標準FakeNavigationManager自動登録済み）

        // JSRuntimeモック設定（Toast表示対応）
        // 【Blazor Server初学者向け解説】
        // bUnitでは、JSRuntimeの呼び出しを明示的にモック設定する必要があります。
        // SetupVoid: 戻り値のないJavaScript関数呼び出し（InvokeVoidAsync）のモック
        // "showToast": JavaScript関数名（ProjectList.razor/ProjectCreate.razorで使用）
        // _ => true: すべての引数パターンを受け入れる（引数検証不要）
        JSInterop.SetupVoid("showToast", _ => true).SetVoidResult();
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
    /// SetupGetProjectsSuccess(new List<FSharpDomainProject>
    /// {
    ///     TestDataBuilder.CreateProject(projectId: 1, projectName: "テストPJ", isActive: true)
    /// });
    ///
    /// 【重要】Application層はF# Domain型を使用するため、ProjectDtoではなくF# Projectを受け取ります
    /// </summary>
    protected void SetupGetProjectsSuccess(List<FSharpDomainProject> projects, int totalCount = 0)
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
    ///
    /// 【引数】
    /// - createdProject: 作成されたプロジェクト（F# Domain型）
    /// - createdDomain: 自動作成されたデフォルトドメイン（F# Domain型）
    ///
    /// 【重要】Application層の戻り値はProjectCreationResultDto（Project + Domain + CreatedAt）です
    /// </summary>
    protected void SetupCreateProjectSuccess(FSharpDomainProject createdProject, FSharpDomainDomain createdDomain)
    {
        var builder = new ProjectManagementServiceMockBuilder();
        MockProjectService = builder
            .SetupCreateProjectSuccess(createdProject, createdDomain)
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
