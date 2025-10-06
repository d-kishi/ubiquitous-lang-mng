using Moq;
using Microsoft.FSharp.Core;
using UbiquitousLanguageManager.Contracts.DTOs;
// Application層のF# Record型をエイリアスで使用
using AppProjectListResult = UbiquitousLanguageManager.Application.ProjectManagement.ProjectListResultDto;
using AppCreateProjectCommand = UbiquitousLanguageManager.Application.ProjectManagement.CreateProjectCommand;
using AppUpdateProjectCommand = UbiquitousLanguageManager.Application.ProjectManagement.UpdateProjectCommand;
using AppDeleteProjectCommand = UbiquitousLanguageManager.Application.ProjectManagement.DeleteProjectCommand;
using AppGetProjectsQuery = UbiquitousLanguageManager.Application.ProjectManagement.GetProjectsQuery;
using AppIProjectManagementService = UbiquitousLanguageManager.Application.ProjectManagement.IProjectManagementService;
using AppProjectCreationResultDto = UbiquitousLanguageManager.Application.ProjectManagement.ProjectCreationResultDto;
// F# Domain型をエイリアスで使用
using FSharpDomainProject = UbiquitousLanguageManager.Domain.ProjectManagement.Project;
using FSharpDomainDomain = UbiquitousLanguageManager.Domain.ProjectManagement.Domain;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UbiquitousLanguageManager.Domain.Common;

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
    private readonly Mock<AppIProjectManagementService> _mockService;

    public ProjectManagementServiceMockBuilder()
    {
        _mockService = new Mock<AppIProjectManagementService>();
    }

    #region GetProjectsAsync モックセットアップ

    /// <summary>
    /// GetProjectsAsync成功モックセットアップ
    ///
    /// 【引数】
    /// - projects: 返却するプロジェクトリスト（F# Domain型）
    /// - totalCount: 総件数（ページング用・デフォルト: projects.Count）
    ///
    /// 【戻り値】
    /// FSharpResult&lt;ProjectListResultDto, string&gt;.NewOk
    ///
    /// 【重要】Application層はF# Domain型を使用するため、ProjectDtoではなくF# Projectを受け取ります
    /// </summary>
    public ProjectManagementServiceMockBuilder SetupGetProjectsSuccess(
        List<FSharpDomainProject> projects,
        int totalCount = 0)
    {
        // C# List<Project> → F# list<Project> への変換
        // F#のlist型はImmutableなので、ListModule.OfSeqで変換が必要
        var fsharpProjectList = Microsoft.FSharp.Collections.ListModule.OfSeq(projects);

        // F# Record型 ProjectListResultDto の正しいパラメータ名を使用
        // F#定義: type ProjectListResultDto = { Projects: Project list; TotalCount: int; ... }
        // 【重要】C#からF# Record型コンストラクタを呼ぶ際は、パラメータ名をcamelCaseで指定（F#言語仕様）
        var resultDto = new AppProjectListResult(
            projects: fsharpProjectList,  // F# list<Project>型（camelCase）
            totalCount: totalCount > 0 ? totalCount : projects.Count,
            pageNumber: 1,
            pageSize: 20,
            hasNextPage: false,
            hasPreviousPage: false
        );

        var fsharpResult = resultDto.ToOkResult();

        _mockService
            .Setup(s => s.GetProjectsAsync(It.IsAny<AppGetProjectsQuery>()))
            .ReturnsAsync(fsharpResult);

        return this;
    }

    /// <summary>
    /// GetProjectsAsync失敗モックセットアップ
    /// </summary>
    public ProjectManagementServiceMockBuilder SetupGetProjectsFailure(string errorMessage)
    {
        var fsharpResult = errorMessage.ToErrorResult<AppProjectListResult>();

        _mockService
            .Setup(s => s.GetProjectsAsync(It.IsAny<AppGetProjectsQuery>()))
            .ReturnsAsync(fsharpResult);

        return this;
    }

    #endregion

    #region CreateProjectAsync モックセットアップ

    /// <summary>
    /// CreateProjectAsync成功モックセットアップ
    ///
    /// 【引数】
    /// - createdProject: 作成されたプロジェクト（F# Domain型）
    /// - createdDomain: 自動作成されたデフォルトドメイン（F# Domain型）
    ///
    /// 【戻り値】
    /// FSharpResult&lt;ProjectCreationResultDto, string&gt;.NewOk
    ///
    /// 【重要】Application層の戻り値はProjectCreationResultDto（Project + Domain + CreatedAt）です
    /// </summary>
    public ProjectManagementServiceMockBuilder SetupCreateProjectSuccess(
        FSharpDomainProject createdProject,
        FSharpDomainDomain createdDomain)
    {
        // ProjectCreationResultDto を生成
        // F#定義: type ProjectCreationResultDto = { Project: Project; DefaultDomain: Domain; CreatedAt: DateTime }
        // 【重要】C#からF# Record型コンストラクタを呼ぶ際は、パラメータ名をcamelCaseで指定（F#言語仕様）
        var creationResult = new AppProjectCreationResultDto(
            project: createdProject,
            defaultDomain: createdDomain,
            createdAt: DateTime.UtcNow
        );

        var fsharpResult = creationResult.ToOkResult();

        _mockService
            .Setup(s => s.CreateProjectAsync(It.IsAny<AppCreateProjectCommand>()))
            .Returns(Task.FromResult(fsharpResult));  // Task.FromResult で明示的にラップ

        return this;
    }

    /// <summary>
    /// CreateProjectAsync失敗モックセットアップ
    /// </summary>
    public ProjectManagementServiceMockBuilder SetupCreateProjectFailure(string errorMessage)
    {
        var fsharpResult = errorMessage.ToErrorResult<AppProjectCreationResultDto>();

        _mockService
            .Setup(s => s.CreateProjectAsync(It.IsAny<AppCreateProjectCommand>()))
            .Returns(Task.FromResult(fsharpResult));  // Task.FromResult で明示的にラップ

        return this;
    }

    #endregion

    #region UpdateProjectAsync モックセットアップ

    /// <summary>
    /// UpdateProjectAsync成功モックセットアップ
    ///
    /// 【引数】
    /// - updatedProject: 更新されたプロジェクト（F# Domain型）
    ///
    /// 【戻り値】
    /// FSharpResult&lt;Project, string&gt;.NewOk
    ///
    /// 【重要】Application層の戻り値はF# Domain型のProjectです
    /// </summary>
    public ProjectManagementServiceMockBuilder SetupUpdateProjectSuccess(FSharpDomainProject updatedProject)
    {
        var fsharpResult = updatedProject.ToOkResult();

        _mockService
            .Setup(s => s.UpdateProjectAsync(It.IsAny<AppUpdateProjectCommand>()))
            .Returns(Task.FromResult(fsharpResult));  // Task.FromResult で明示的にラップ

        return this;
    }

    /// <summary>
    /// UpdateProjectAsync失敗モックセットアップ
    /// </summary>
    public ProjectManagementServiceMockBuilder SetupUpdateProjectFailure(string errorMessage)
    {
        var fsharpResult = errorMessage.ToErrorResult<FSharpDomainProject>();

        _mockService
            .Setup(s => s.UpdateProjectAsync(It.IsAny<AppUpdateProjectCommand>()))
            .Returns(Task.FromResult(fsharpResult));  // Task.FromResult で明示的にラップ

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
        // F# Unit型はstruct（値型）のため、default(Unit)で生成
        // 【注意】Unit.Defaultは存在しません（F# 8.0仕様）
        var unitValue = default(Microsoft.FSharp.Core.Unit);
        var fsharpResult = FSharpResult<Unit, string>.NewOk(unitValue);

        _mockService
            .Setup(s => s.DeleteProjectAsync(It.IsAny<AppDeleteProjectCommand>()))
            .ReturnsAsync(fsharpResult);

        return this;
    }

    /// <summary>
    /// DeleteProjectAsync失敗モックセットアップ
    /// </summary>
    public ProjectManagementServiceMockBuilder SetupDeleteProjectFailure(string errorMessage)
    {
        var fsharpResult = errorMessage.ToErrorResult<Unit>();

        _mockService
            .Setup(s => s.DeleteProjectAsync(It.IsAny<AppDeleteProjectCommand>()))
            .ReturnsAsync(fsharpResult);

        return this;
    }

    #endregion

    #region GetProjectDetailAsync モックセットアップ

    /// <summary>
    /// GetProjectDetailAsync成功モックセットアップ
    ///
    /// 【引数】
    /// - project: 返却するプロジェクト詳細（F# Domain型）
    ///
    /// 【戻り値】
    /// FSharpResult&lt;ProjectDetailResultDto, string&gt;.NewOk
    /// </summary>
    public ProjectManagementServiceMockBuilder SetupGetProjectDetailSuccess(FSharpDomainProject project)
    {
        // ProjectDetailResultDto を生成
        var resultDto = new UbiquitousLanguageManager.Application.ProjectManagement.ProjectDetailResultDto(
            project: project,
            userCount: 1,
            domainCount: 1,
            ubiquitousLanguageCount: 0,
            canEdit: true,
            canDelete: true
        );

        var fsharpResult = resultDto.ToOkResult();

        _mockService
            .Setup(s => s.GetProjectDetailAsync(It.IsAny<UbiquitousLanguageManager.Application.ProjectManagement.GetProjectDetailQuery>()))
            .Returns(Task.FromResult(fsharpResult));

        return this;
    }

    /// <summary>
    /// GetProjectDetailAsync失敗モックセットアップ
    /// </summary>
    public ProjectManagementServiceMockBuilder SetupGetProjectDetailFailure(string errorMessage)
    {
        var fsharpResult = errorMessage.ToErrorResult<UbiquitousLanguageManager.Application.ProjectManagement.ProjectDetailResultDto>();

        _mockService
            .Setup(s => s.GetProjectDetailAsync(It.IsAny<UbiquitousLanguageManager.Application.ProjectManagement.GetProjectDetailQuery>()))
            .Returns(Task.FromResult(fsharpResult));

        return this;
    }

    #endregion

    #region ビルド

    /// <summary>
    /// モックインスタンス取得（IProjectManagementService）
    /// </summary>
    public AppIProjectManagementService Build() => _mockService.Object;

    /// <summary>
    /// Mockオブジェクト取得（検証用）
    ///
    /// 【使用例】
    /// var mock = builder.BuildMock();
    /// mock.Verify(s => s.GetProjectsAsync(It.IsAny&lt;GetProjectsQuery&gt;()), Times.Once);
    /// </summary>
    public Mock<AppIProjectManagementService> BuildMock() => _mockService;

    #endregion
}
