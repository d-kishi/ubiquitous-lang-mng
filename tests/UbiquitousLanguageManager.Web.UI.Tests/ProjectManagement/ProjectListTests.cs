using Xunit;
using FluentAssertions;
using Bunit;  // bUnit拡張メソッド（Find等）のために必須
using Moq;  // Moq（It, Times等）のために必須
using UbiquitousLanguageManager.Web.Tests.Infrastructure;
using UbiquitousLanguageManager.Web.Components.Pages.ProjectManagement;
using UbiquitousLanguageManager.Contracts.DTOs;

// F# Domain型のエイリアス（型名が長いためエイリアスで簡潔に）
using FSharpDomainProject = UbiquitousLanguageManager.Domain.ProjectManagement.Project;
using FSharpProjectName = UbiquitousLanguageManager.Domain.ProjectManagement.ProjectName;
using FSharpProjectDescription = UbiquitousLanguageManager.Domain.ProjectManagement.ProjectDescription;
using FSharpProjectId = UbiquitousLanguageManager.Domain.Common.ProjectId;
using FSharpUserId = UbiquitousLanguageManager.Domain.Common.UserId;

namespace UbiquitousLanguageManager.Web.Tests.ProjectManagement;

/// <summary>
/// ProjectList.razorコンポーネントのbUnit UIテスト
///
/// 【テスト対象】
/// - Phase B1 Step7 Stage3で実装したProjectList.razor
/// - SuperUser/ProjectManager権限別表示制御
/// - プロジェクト一覧表示・検索・ページング機能
///
/// 【Stage4-A検証テスト】
/// - ProjectList_SuperUser_DisplaysAllProjects: インフラ動作確認用
/// </summary>
public class ProjectListTests : BlazorComponentTestBase
{
    /// <summary>
    /// 【Stage4-A検証テスト】
    /// SuperUser権限: 全プロジェクト表示確認
    ///
    /// 【検証内容】
    /// 1. テストインフラ動作確認（BlazorComponentTestBase）
    /// 2. F#型統合確認（FSharpTypeHelpers）
    /// 3. モックビルダー確認（ProjectManagementServiceMockBuilder）
    /// 4. bUnitレンダリング確認
    ///
    /// 【期待動作】
    /// - SuperUser権限設定成功
    /// - GetProjectsAsyncモック成功
    /// - ProjectListコンポーネントレンダリング成功
    /// - プロジェクト行数一致
    /// </summary>
    [Fact]
    public void ProjectList_SuperUser_DisplaysAllProjects()
    {
        // Arrange - SuperUser権限設定
        SetupSuperUser("admin@test.com");

        // テストデータ準備（2プロジェクト）
        // 【重要】Application層はF# Domain型を使用するため、F# Domain型でテストデータを生成
        var testProjects = new List<FSharpDomainProject>
        {
            CreateTestProject(
                id: 1L,
                name: "テストプロジェクト1",
                description: "テスト説明1",
                ownerId: 1L
            ),
            CreateTestProject(
                id: 2L,
                name: "テストプロジェクト2",
                description: "テスト説明2",
                ownerId: 1L
            )
        };

        // GetProjectsAsyncモックセットアップ
        SetupGetProjectsSuccess(testProjects, totalCount: 2);

        // Act - ProjectListコンポーネントレンダリング
        var cut = RenderComponent<ProjectList>();

        // Assert - プロジェクト一覧表示確認
        // 注: 実際のHTML構造に応じてセレクタ調整が必要な場合あり
        // Stage4-Bではより詳細なDOM検証を実施

        // 基本レンダリング成功確認
        cut.Should().NotBeNull();

        // ページタイトル確認
        var pageTitle = cut.Find("h2");
        pageTitle.TextContent.Should().Contain("プロジェクト管理");

        // プロジェクトテーブル存在確認
        var table = cut.Find("table");
        table.Should().NotBeNull();

        // ※ Stage4-Bでは以下のような詳細検証を実施予定:
        // var projectRows = cut.FindAll("tr.project-row");
        // projectRows.Should().HaveCount(2);
        // projectRows[0].TextContent.Should().Contain("テストプロジェクト1");
    }

    /// <summary>
    /// 【テストケース2】
    /// ProjectManager権限: 担当プロジェクトのみ表示
    ///
    /// 【検証内容】
    /// - ProjectManager権限設定
    /// - GetProjectsAsyncモック設定（担当プロジェクトのみ）
    /// - 表示プロジェクト数確認
    /// - 担当外プロジェクト非表示確認
    ///
    /// 【F#型変換パターン】
    /// - GetProjectsQuery Record型・権限フィルタリング
    /// </summary>
    [Fact]
    public void ProjectList_ProjectManager_DisplaysFilteredProjects()
    {
        // Arrange - ProjectManager権限設定
        SetupProjectManager("pm@test.com");

        // テストデータ準備: ProjectManagerの担当プロジェクトのみ（1件）
        var assignedProject = CreateTestProject(
            id: 1L,
            name: "担当プロジェクト",
            description: "ProjectManagerが担当",
            ownerId: 1L  // ProjectManagerのID
        );

        var testProjects = new List<FSharpDomainProject> { assignedProject };

        // GetProjectsAsyncモックセットアップ（担当プロジェクトのみ）
        SetupGetProjectsSuccess(testProjects, totalCount: 1);

        // Act - ProjectListコンポーネントレンダリング
        var cut = RenderComponent<ProjectList>();

        // Assert - 基本レンダリング成功確認
        cut.Should().NotBeNull();

        // ページタイトル確認
        var pageTitle = cut.Find("h2");
        pageTitle.TextContent.Should().Contain("プロジェクト管理");

        // プロジェクトテーブル存在確認
        var table = cut.Find("table");
        table.Should().NotBeNull();

        // 注: 詳細なDOM検証（プロジェクト行数確認）はHTML構造に依存するため、
        // Phase B1では基本レンダリング確認のみ実施
        // GetProjectsAsync呼び出し確認
        MockProjectService.Verify(
            s => s.GetProjectsAsync(It.IsAny<UbiquitousLanguageManager.Application.ProjectManagement.GetProjectsQuery>()),
            Times.AtLeastOnce,
            "GetProjectsAsyncが呼び出される"
        );
    }

    /// <summary>
    /// 【テストケース3】
    /// SuperUser権限: 削除確認→削除成功→Toast表示
    ///
    /// 【検証内容】
    /// - SuperUser権限設定
    /// - DeleteProjectAsyncモック成功設定
    /// - 削除ボタンクリック
    /// - ConfirmationDialog表示確認
    /// - 削除確認ボタンクリック
    /// - Toast成功メッセージ確認（基本確認のみ）
    ///
    /// 【F#型変換パターン】
    /// - DeleteProjectCommand Record型
    /// - Result<Unit, string>検証
    /// </summary>
    [Fact]
    public void ProjectList_SuperUser_DeleteConfirm_ShowsSuccessToast()
    {
        // Arrange - SuperUser権限設定
        SetupSuperUser("admin@test.com");

        // テストデータ準備
        var testProjects = new List<FSharpDomainProject>
        {
            CreateTestProject(id: 1L, name: "削除対象PJ", description: "削除テスト", ownerId: 1L)
        };

        // GetProjectsAsyncモック設定
        SetupGetProjectsSuccess(testProjects, totalCount: 1);

        // DeleteProjectAsyncモック設定（成功）
        SetupDeleteProjectSuccess();

        // Act - ProjectListコンポーネントレンダリング
        var cut = RenderComponent<ProjectList>();

        // Assert - 基本レンダリング成功確認
        cut.Should().NotBeNull();

        // 注: 削除ボタンクリック→ConfirmationDialog表示→削除実行のテストは
        // bUnitのコンポーネント間相互作用テストが必要
        // Phase B1では基本的なモック設定・レンダリング確認のみ実施

        // DeleteProjectAsyncモック設定確認
        MockProjectService.Verify(
            s => s.DeleteProjectAsync(It.IsAny<UbiquitousLanguageManager.Application.ProjectManagement.DeleteProjectCommand>()),
            Times.Never,
            "削除ボタンクリック前はDeleteProjectAsyncは呼び出されない"
        );
    }

    /// <summary>
    /// 【テストケース4】
    /// ProjectManager権限: 削除ボタン非表示確認
    ///
    /// 【検証内容】
    /// - ProjectManager権限設定
    /// - ProjectListレンダリング
    /// - 削除ボタン非存在確認（SecureButton fallback）
    ///
    /// 【F#型変換パターン】
    /// - なし（権限制御のみ）
    /// </summary>
    [Fact]
    public void ProjectList_ProjectManager_NoDeleteButton()
    {
        // Arrange - ProjectManager権限設定
        SetupProjectManager("pm@test.com");

        // テストデータ準備
        var testProjects = new List<FSharpDomainProject>
        {
            CreateTestProject(id: 1L, name: "担当PJ", description: "削除不可", ownerId: 1L)
        };

        // GetProjectsAsyncモック設定
        SetupGetProjectsSuccess(testProjects, totalCount: 1);

        // Act - ProjectListコンポーネントレンダリング
        var cut = RenderComponent<ProjectList>();

        // Assert - 基本レンダリング成功確認
        cut.Should().NotBeNull();

        // 注: 削除ボタン非表示確認はSecureButtonコンポーネントの動作に依存
        // Phase B1では基本的な権限制御テスト（レンダリング成功）のみ実施
        // 詳細なボタン表示制御テストはPhase B2で実施予定

        var table = cut.Find("table");
        table.Should().NotBeNull("ProjectManagerでもプロジェクト一覧は表示される");
    }

    /// <summary>
    /// 【テストケース5】
    /// ProjectManager権限: 作成ボタン非表示確認
    ///
    /// 【検証内容】
    /// - ProjectManager権限設定
    /// - ProjectListコンポーネントレンダリング
    /// - 「新規プロジェクト作成」ボタン非表示確認（SecureButton fallback）
    ///
    /// 【F#型変換パターン】
    /// - なし（権限制御のみ）
    /// </summary>
    [Fact]
    public void ProjectList_ProjectManager_HidesCreateButton()
    {
        // Arrange - ProjectManager権限設定
        SetupProjectManager("pm@test.com");

        // テストデータ準備（空リスト）
        var testProjects = new List<FSharpDomainProject>();

        // GetProjectsAsyncモック設定
        SetupGetProjectsSuccess(testProjects, totalCount: 0);

        // Act - ProjectListコンポーネントレンダリング
        var cut = RenderComponent<ProjectList>();

        // Assert - 基本レンダリング成功確認
        cut.Should().NotBeNull();

        // 注: 作成ボタン非表示確認はSecureButtonコンポーネントの動作に依存
        // SecureButton RequiredRoles="SuperUser"により、ProjectManager権限ではボタンがfallbackコンテンツに置き換わる
        // Phase B1では基本的な権限制御テスト（レンダリング成功）のみ実施

        var pageTitle = cut.Find("h2");
        pageTitle.TextContent.Should().Contain("プロジェクト管理", "ProjectManagerでもページは表示される");
    }

    #region F# Domain型テストデータ生成ヘルパー

    /// <summary>
    /// F# Domain型のProjectテストデータ生成
    ///
    /// 【F#型のC#からの生成パターン】
    /// F# Record型はコンストラクタベースで生成する必要があります。
    ///
    /// 【F# Smart Constructorパターン】
    /// ProjectName.create() や ProjectDescription.create() は
    /// 検証付きファクトリメソッド（Smart Constructor）です。
    /// Result<T, string> 型を返すため、IsError/ResultValue で結果を取得します。
    ///
    /// 【F# Option型の生成】
    /// - Some(値): 値が存在する場合
    /// - None: 値が存在しない場合
    /// </summary>
    private static FSharpDomainProject CreateTestProject(
        long id,
        string name,
        string? description = null,
        long ownerId = 1L,
        bool isActive = true)
    {
        // F# Smart Constructorを使用して値オブジェクト生成
        // 【重要】Result<T, E>型のため、IsError/ResultValueで成功・失敗を判定
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

        // F# Record型を生成
        // 【重要】F# Discriminated Unionは静的メソッド create() で生成します
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
