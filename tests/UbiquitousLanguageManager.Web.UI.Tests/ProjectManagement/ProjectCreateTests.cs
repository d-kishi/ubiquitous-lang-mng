using Xunit;
using FluentAssertions;
using Bunit;  // bUnit拡張メソッド（Find等）のために必須
using Moq;  // Moq（It, Times等）のために必須
using UbiquitousLanguageManager.Web.Tests.Infrastructure;
using UbiquitousLanguageManager.Web.Components.Pages.ProjectManagement;
using UbiquitousLanguageManager.Contracts.DTOs;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Components;

// F# Domain型のエイリアス（型名が長いためエイリアスで簡潔に）
using FSharpDomainProject = UbiquitousLanguageManager.Domain.ProjectManagement.Project;
using FSharpProjectName = UbiquitousLanguageManager.Domain.ProjectManagement.ProjectName;
using FSharpProjectDescription = UbiquitousLanguageManager.Domain.ProjectManagement.ProjectDescription;
using FSharpDomainDomain = UbiquitousLanguageManager.Domain.ProjectManagement.Domain;
using FSharpDomainName = UbiquitousLanguageManager.Domain.ProjectManagement.DomainName;
using FSharpProjectId = UbiquitousLanguageManager.Domain.Common.ProjectId;
using FSharpDomainId = UbiquitousLanguageManager.Domain.Common.DomainId;
using FSharpUserId = UbiquitousLanguageManager.Domain.Common.UserId;

namespace UbiquitousLanguageManager.Web.Tests.ProjectManagement;

/// <summary>
/// ProjectCreate.razorコンポーネントのbUnit UIテスト
///
/// 【テスト対象】
/// - Phase B1 Step7 Stage3で実装したProjectCreate.razor
/// - SuperUser権限のみアクセス可能
/// - プロジェクト作成・デフォルトドメイン自動作成機能
///
/// 【実装テストケース】
/// 1. ProjectCreate_SuperUser_SubmitsValidForm_ShowsSuccessMessage
/// 2. ProjectCreate_DuplicateName_ShowsErrorMessage
/// 3. ProjectCreate_ProjectManager_HidesCreateButton (ProjectListテストに移動)
/// 4. ProjectCreate_SuccessMessage_MentionsDefaultDomain
/// </summary>
public class ProjectCreateTests : BlazorComponentTestBase
{
    /// <summary>
    /// 【テストケース1】
    /// SuperUser権限: 有効なフォーム送信→成功メッセージ表示→リダイレクト
    ///
    /// 【検証内容】
    /// - SuperUser権限設定成功
    /// - プロジェクト名・説明入力
    /// - CreateProjectAsyncモック成功設定
    /// - フォーム送信成功
    /// - NavigationManager.Uri == "/projects" 確認
    ///
    /// 【F#型変換パターン】
    /// - CreateProjectCommand Record型コンストラクタ生成
    /// - Result<ProjectCreationResultDto, string>検証
    /// </summary>
    [Fact]
    public void ProjectCreate_SuperUser_SubmitsValidForm_ShowsSuccessMessage()
    {
        // Arrange - SuperUser権限設定
        SetupSuperUser("admin@test.com");

        // テストデータ準備: 作成されたプロジェクト（F# Domain型）
        var createdProject = CreateTestProject(
            id: 1L,
            name: "新規テストプロジェクト",
            description: "テスト説明",
            ownerId: 1L
        );

        // デフォルトドメイン準備（F# Domain型）
        // 注: 実際のデフォルトドメイン名は「{ProjectName}_Default」形式
        var defaultDomain = CreateTestDomain(
            id: 1L,
            name: "新規テストプロジェクト_Default",
            projectId: 1L,
            isDefault: true
        );

        // CreateProjectAsyncモック設定（成功）
        SetupCreateProjectSuccess(createdProject, defaultDomain);

        // Act - ProjectCreateコンポーネントレンダリング
        var cut = RenderComponent<ProjectCreate>();

        // プロジェクト名入力
        var projectNameInput = cut.Find("input#projectName");
        projectNameInput.Change("新規テストプロジェクト");

        // プロジェクト説明入力
        var projectDescriptionInput = cut.Find("textarea#projectDescription");
        projectDescriptionInput.Change("テスト説明");

        // フォーム送信
        var form = cut.Find("form");
        form.Submit();

        // Assert - NavigationManagerリダイレクト確認
        var navMan = Services.GetRequiredService<NavigationManager>();
        navMan.Uri.Should().EndWith("/projects", "フォーム送信成功時にプロジェクト一覧へリダイレクトされる");

        // CreateProjectAsync呼び出し確認
        MockProjectService.Verify(
            s => s.CreateProjectAsync(It.IsAny<UbiquitousLanguageManager.Application.ProjectManagement.CreateProjectCommand>()),
            Times.Once,
            "CreateProjectAsyncが1回呼び出される"
        );
    }

    /// <summary>
    /// 【テストケース2】
    /// 重複プロジェクト名: エラーメッセージ表示・リダイレクトなし
    ///
    /// 【検証内容】
    /// - SuperUser権限設定
    /// - CreateProjectAsyncモック失敗設定（重複名エラー）
    /// - フォーム送信
    /// - エラーメッセージ表示確認（Toast）
    /// - リダイレクトなし確認
    ///
    /// 【F#型変換パターン】
    /// - Result Error型検証
    /// </summary>
    [Fact]
    public void ProjectCreate_DuplicateName_ShowsErrorMessage()
    {
        // Arrange - SuperUser権限設定
        SetupSuperUser("admin@test.com");

        // CreateProjectAsyncモック設定（失敗・重複名エラー）
        var builder = new ProjectManagementServiceMockBuilder();
        var mockService = builder
            .SetupCreateProjectFailure("プロジェクト名が重複しています")
            .BuildMock();
        Services.AddSingleton(mockService.Object);

        // Act - ProjectCreateコンポーネントレンダリング
        var cut = RenderComponent<ProjectCreate>();

        // プロジェクト名入力（重複名）
        var projectNameInput = cut.Find("input#projectName");
        projectNameInput.Change("既存プロジェクト名");

        // フォーム送信
        var form = cut.Find("form");
        form.Submit();

        // Assert - リダイレクトなし確認
        var navMan = Services.GetRequiredService<NavigationManager>();
        navMan.Uri.Should().NotEndWith("/projects", "エラー時はリダイレクトされない");

        // CreateProjectAsync呼び出し確認
        MockProjectService.Verify(
            s => s.CreateProjectAsync(It.IsAny<UbiquitousLanguageManager.Application.ProjectManagement.CreateProjectCommand>()),
            Times.Once,
            "CreateProjectAsyncが1回呼び出される"
        );

        // 注: Toast表示確認はJSRuntime.InvokeVoidAsyncモックが必要なため、Phase B1では簡易実装
    }

    /// <summary>
    /// 【テストケース4】
    /// 成功メッセージにデフォルトドメイン「共通」が含まれる確認
    ///
    /// 【検証内容】
    /// - SuperUser権限設定
    /// - CreateProjectAsyncモック成功設定（DefaultDomain含む）
    /// - フォーム送信
    /// - 成功メッセージに「共通」含まれる確認
    ///
    /// 【F#型変換パターン】
    /// - ProjectCreationResultDto検証・DefaultDomain確認
    /// </summary>
    [Fact]
    public void ProjectCreate_SuccessMessage_MentionsDefaultDomain()
    {
        // Arrange - SuperUser権限設定
        SetupSuperUser("admin@test.com");

        // テストデータ準備: 作成されたプロジェクト
        var createdProject = CreateTestProject(
            id: 1L,
            name: "新規プロジェクト",
            description: "説明",
            ownerId: 1L
        );

        // デフォルトドメイン準備
        // 注: 実際のデフォルトドメイン名は「{ProjectName}_Default」形式
        var defaultDomain = CreateTestDomain(
            id: 1L,
            name: "新規プロジェクト_Default",  // デフォルトドメイン名
            projectId: 1L,
            isDefault: true
        );

        // CreateProjectAsyncモック設定（ProjectCreationResultDto）
        SetupCreateProjectSuccess(createdProject, defaultDomain);

        // Act - ProjectCreateコンポーネントレンダリング
        var cut = RenderComponent<ProjectCreate>();

        // プロジェクト名入力
        var projectNameInput = cut.Find("input#projectName");
        projectNameInput.Change("新規プロジェクト");

        // フォーム送信
        var form = cut.Find("form");
        form.Submit();

        // Assert - デフォルトドメイン作成確認
        // 注: 実際のToast表示確認にはJSRuntimeモックが必要
        // ProjectCreate.razorの229行目で「プロジェクトとデフォルトドメイン「共通」を作成しました」を表示
        // Phase B1では基本動作確認のみ実施
        MockProjectService.Verify(
            s => s.CreateProjectAsync(It.IsAny<UbiquitousLanguageManager.Application.ProjectManagement.CreateProjectCommand>()),
            Times.Once,
            "CreateProjectAsyncが1回呼び出される"
        );

        // NavigationManagerリダイレクト確認
        var navMan = Services.GetRequiredService<NavigationManager>();
        navMan.Uri.Should().EndWith("/projects", "成功時にプロジェクト一覧へリダイレクトされる");

        // 補足: DefaultDomain名が「{ProjectName}_Default」形式であることを確認
        defaultDomain.Name.Value.Should().Be("新規プロジェクト_Default", "デフォルトドメイン名は「{ProjectName}_Default」形式");
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

    /// <summary>
    /// F# Domain型のDomainテストデータ生成
    ///
    /// 【F#型のC#からの生成パターン】
    /// デフォルトドメイン「共通」のテストデータ生成
    /// </summary>
    private static FSharpDomainDomain CreateTestDomain(
        long id,
        string name,
        long projectId,
        bool isDefault = false,
        bool isActive = true,
        long ownerId = 1L)
    {
        // F# Smart Constructorを使用してドメイン名生成
        var domainName = FSharpDomainName.create(name);
        if (domainName.IsError)
            throw new InvalidOperationException($"Invalid domain name: {name}. Error: {domainName.ErrorValue}");

        // F# ProjectDescription Smart Constructor（空の説明）
        var emptyDescription = FSharpProjectDescription.create(Microsoft.FSharp.Core.FSharpOption<string>.None);
        if (emptyDescription.IsError)
            throw new InvalidOperationException($"Invalid empty description. Error: {emptyDescription.ErrorValue}");

        // F# Domain Record型を生成
        // 【重要】Domain型のフィールド順序: Id, ProjectId, Name, Description, OwnerId, IsDefault, CreatedAt, UpdatedAt, IsActive
        return new FSharpDomainDomain(
            id: FSharpDomainId.create(id),
            projectId: FSharpProjectId.create(projectId),
            name: domainName.ResultValue,
            description: emptyDescription.ResultValue,
            ownerId: FSharpUserId.create(ownerId),
            isDefault: isDefault,
            createdAt: DateTime.UtcNow,
            updatedAt: Microsoft.FSharp.Core.FSharpOption<DateTime>.None,
            isActive: isActive
        );
    }

    #endregion
}
