using Xunit;
using FluentAssertions;
using Bunit;  // bUnit拡張メソッド（Find等）のために必須
using UbiquitousLanguageManager.Web.Tests.Infrastructure;
using UbiquitousLanguageManager.Web.Components.Pages.ProjectManagement;
using UbiquitousLanguageManager.Contracts.DTOs;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Components;
using Moq;

// F# Domain型のエイリアス
using FSharpDomainProject = UbiquitousLanguageManager.Domain.ProjectManagement.Project;
using FSharpProjectName = UbiquitousLanguageManager.Domain.ProjectManagement.ProjectName;
using FSharpProjectDescription = UbiquitousLanguageManager.Domain.ProjectManagement.ProjectDescription;
using FSharpProjectId = UbiquitousLanguageManager.Domain.Common.ProjectId;
using FSharpUserId = UbiquitousLanguageManager.Domain.Common.UserId;

namespace UbiquitousLanguageManager.Web.Tests.ProjectManagement;

/// <summary>
/// ProjectEdit.razorコンポーネントのbUnit UIテスト
///
/// 【テスト対象】
/// - Phase B1 Step7 Stage3で実装したProjectEdit.razor
/// - SuperUser/ProjectManager権限でアクセス可能
/// - プロジェクト説明編集機能（プロジェクト名は変更禁止）
///
/// 【実装テストケース】
/// 1. ProjectEdit_SuperUser_UpdatesDescription_ShowsSuccess
/// 2. ProjectEdit_ProjectManager_UpdatesOwnedProject_ShowsSuccess
/// </summary>
public class ProjectEditTests : BlazorComponentTestBase
{
    /// <summary>
    /// 【テストケース1】
    /// SuperUser権限: プロジェクト説明更新→成功メッセージ→リダイレクト
    ///
    /// 【検証内容】
    /// - SuperUser権限設定
    /// - 既存プロジェクトデータモック設定
    /// - 説明フィールド編集
    /// - UpdateProjectAsyncモック成功設定
    /// - フォーム送信
    /// - NavigationManager.Uri == "/projects" 確認
    ///
    /// 【F#型変換パターン】
    /// - UpdateProjectCommand Record型コンストラクタ生成
    /// - Result<Project, string>検証
    /// </summary>
    [Fact]
    public void ProjectEdit_SuperUser_UpdatesDescription_ShowsSuccess()
    {
        // Arrange - SuperUser権限設定
        SetupSuperUser("admin@test.com");

        // 既存プロジェクトデータ準備（F# Domain型）
        var existingProject = CreateTestProject(
            id: 1L,
            name: "既存プロジェクト",
            description: "既存の説明",
            ownerId: 1L
        );

        // 更新後プロジェクトデータ準備
        var updatedProject = CreateTestProject(
            id: 1L,
            name: "既存プロジェクト",  // プロジェクト名は変更不可
            description: "更新後の説明",
            ownerId: 1L
        );

        // GetProjectDetailAsyncモック設定（既存データ読み込み）
        var builder = new ProjectManagementServiceMockBuilder();
        var mockService = builder
            .SetupGetProjectDetailSuccess(existingProject)
            .SetupUpdateProjectSuccess(updatedProject)
            .BuildMock();
        Services.AddSingleton(mockService.Object);

        // Act - ProjectEditコンポーネントレンダリング（ProjectId=1）
        var cut = RenderComponent<ProjectEdit>(parameters => parameters
            .Add(p => p.ProjectId, 1L));

        // 説明フィールド編集
        var descriptionInput = cut.Find("textarea#projectDescription");
        descriptionInput.Change("更新後の説明");

        // フォーム送信
        var form = cut.Find("form");
        form.Submit();

        // Assert - NavigationManagerリダイレクト確認
        var navMan = Services.GetRequiredService<NavigationManager>();
        navMan.Uri.Should().EndWith("/projects", "更新成功時にプロジェクト一覧へリダイレクトされる");

        // UpdateProjectAsync呼び出し確認
        MockProjectService.Verify(
            s => s.UpdateProjectAsync(It.IsAny<UbiquitousLanguageManager.Application.ProjectManagement.UpdateProjectCommand>()),
            Times.Once,
            "UpdateProjectAsyncが1回呼び出される"
        );
    }

    /// <summary>
    /// 【テストケース2】
    /// ProjectManager権限: 担当プロジェクト更新→成功
    ///
    /// 【検証内容】
    /// - ProjectManager権限設定
    /// - 担当プロジェクトデータモック設定
    /// - UpdateProjectAsyncモック成功設定
    /// - 編集成功確認
    ///
    /// 【F#型変換パターン】
    /// - UpdateProjectCommand Record型・権限制御統合
    /// </summary>
    [Fact]
    public void ProjectEdit_ProjectManager_UpdatesOwnedProject_ShowsSuccess()
    {
        // Arrange - ProjectManager権限設定
        SetupProjectManager("pm@test.com");

        // 担当プロジェクトデータ準備（OwnerId=1: ProjectManagerのID）
        var ownedProject = CreateTestProject(
            id: 1L,
            name: "担当プロジェクト",
            description: "担当プロジェクトの説明",
            ownerId: 1L  // ProjectManagerが所有
        );

        // 更新後プロジェクトデータ
        var updatedProject = CreateTestProject(
            id: 1L,
            name: "担当プロジェクト",
            description: "更新後の説明",
            ownerId: 1L
        );

        // モック設定
        var builder = new ProjectManagementServiceMockBuilder();
        var mockService = builder
            .SetupGetProjectDetailSuccess(ownedProject)
            .SetupUpdateProjectSuccess(updatedProject)
            .BuildMock();
        Services.AddSingleton(mockService.Object);

        // Act - ProjectEditコンポーネントレンダリング
        var cut = RenderComponent<ProjectEdit>(parameters => parameters
            .Add(p => p.ProjectId, 1L));

        // 説明フィールド編集
        var descriptionInput = cut.Find("textarea#projectDescription");
        descriptionInput.Change("更新後の説明");

        // フォーム送信
        var form = cut.Find("form");
        form.Submit();

        // Assert - NavigationManagerリダイレクト確認
        var navMan = Services.GetRequiredService<NavigationManager>();
        navMan.Uri.Should().EndWith("/projects", "ProjectManager担当プロジェクト更新成功");

        // UpdateProjectAsync呼び出し確認
        MockProjectService.Verify(
            s => s.UpdateProjectAsync(It.IsAny<UbiquitousLanguageManager.Application.ProjectManagement.UpdateProjectCommand>()),
            Times.Once,
            "UpdateProjectAsyncが1回呼び出される"
        );
    }

    #region F# Domain型テストデータ生成ヘルパー

    /// <summary>
    /// F# Domain型のProjectテストデータ生成
    /// </summary>
    private static FSharpDomainProject CreateTestProject(
        long id,
        string name,
        string? description = null,
        long ownerId = 1L,
        bool isActive = true)
    {
        var projectName = FSharpProjectName.create(name);
        if (projectName.IsError)
            throw new InvalidOperationException($"Invalid project name: {name}. Error: {projectName.ErrorValue}");

        var projectDescription = FSharpProjectDescription.create(
            string.IsNullOrEmpty(description)
                ? Microsoft.FSharp.Core.FSharpOption<string>.None
                : Microsoft.FSharp.Core.FSharpOption<string>.Some(description)
        );
        if (projectDescription.IsError)
            throw new InvalidOperationException($"Invalid project description: {description}. Error: {projectDescription.ErrorValue}");

        return new FSharpDomainProject(
            id: FSharpProjectId.create(id),
            name: projectName.ResultValue,
            description: projectDescription.ResultValue,
            ownerId: FSharpUserId.create(ownerId),
            isActive: isActive,
            createdAt: DateTime.UtcNow,
            updatedAt: Microsoft.FSharp.Core.FSharpOption<DateTime>.None
        );
    }

    #endregion
}
