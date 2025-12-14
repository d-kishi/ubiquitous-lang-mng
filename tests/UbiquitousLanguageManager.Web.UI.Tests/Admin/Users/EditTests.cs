using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using FluentAssertions;
using Bunit;
using UbiquitousLanguageManager.Web.Tests.Infrastructure;
using UbiquitousLanguageManager.Web.Components.Pages.Admin.Users;
using UbiquitousLanguageManager.Application.ProjectManagement;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Moq;

// F# Domain型のエイリアス
using FSharpDomainUser = UbiquitousLanguageManager.Domain.Authentication.User;
using FSharpUserId = UbiquitousLanguageManager.Domain.Common.UserId;
using FSharpUserName = UbiquitousLanguageManager.Domain.Authentication.UserName;
using FSharpEmail = UbiquitousLanguageManager.Domain.Authentication.Email;
using FSharpRole = UbiquitousLanguageManager.Domain.Common.Role;
using FSharpDomainProject = UbiquitousLanguageManager.Domain.ProjectManagement.Project;

namespace UbiquitousLanguageManager.Web.Tests.Admin.Users;

/// <summary>
/// Edit.razorコンポーネントのbUnit UIテスト（Modified Red Phase TDD）
///
/// 【テスト対象】
/// - Phase B-F3 Step1で実装したEdit.razor
/// - SuperUser権限でアクセス可能
/// - ユーザー情報編集機能（メールアドレス以外）
///
/// 【実装テストケース】
/// 1. 初期表示系（4ケース）
/// 2. 更新処理系（5ケース）
/// 3. パスワードリセット系（3ケース）
/// 4. 権限制御系（2ケース - ProjectManager権限は TODO）
///
/// 【TDDアプローチ】
/// - RED/GREEN分類実行
/// - 既存実装準拠性確認
/// - 仕様ギャップ発見・報告
/// </summary>
public class EditTests : BlazorComponentTestBase
{
    #region ヘルパーメソッド

    /// <summary>
    /// Edit.razor用の完全なモックセットアップ
    ///
    /// 【必須モック】
    /// - GetAllUsersWithIdentityAsync: LoadUserAsync内で呼び出される（Phase B-F3リファクタ対応）
    /// - GetProjectIdsByUserIdAsync: LoadUserAsync内でAssignedProjectIds復元に使用
    /// - GetProjectsAsync: LoadProjectsAsync内で呼び出される
    ///
    /// 【引数】
    /// - existingUser: 編集対象ユーザー（F# Domain型）
    /// - identityId: IdentityId（ASP.NET Core Identity ID）
    /// - projectIds: 割り当てプロジェクトIDリスト（省略時は空リスト）
    /// </summary>
    private UserManagementServiceMockBuilder SetupEditMocks(
        FSharpDomainUser existingUser,
        string identityId,
        List<long>? projectIds = null)
    {
        var builder = new UserManagementServiceMockBuilder();

        // 1. GetAllUsersWithIdentityAsync成功モック（Phase B-F3リファクタ対応）
        // Edit.razorのLoadUserAsync()で呼び出される
        var userTuple = new Tuple<FSharpDomainUser, string>(existingUser, identityId);
        builder.SetupGetAllUsersWithIdentitySuccess(new List<Tuple<FSharpDomainUser, string>> { userTuple });

        // 2. GetProjectIdsByUserIdAsync成功モック（Phase B-F3 Stage4対応）
        // AssignedProjectIds復元に使用
        builder.SetupGetProjectIdsByUserIdSuccess(projectIds ?? new List<long>());

        // 3. IProjectManagementServiceモック作成（GetProjectsAsync用）
        // Edit.razorのLoadProjectsAsync内で呼び出される
        var projectMockBuilder = new ProjectManagementServiceMockBuilder();
        projectMockBuilder.SetupGetProjectsSuccess(new List<FSharpDomainProject>(), totalCount: 0);
        Services.AddSingleton(projectMockBuilder.Build());

        return builder;
    }

    #endregion

    #region 1. 初期表示系テスト（4ケース）

    /// <summary>
    /// 【初期表示1】フォーム初期値表示（既存ユーザーデータ）
    ///
    /// 【検証内容】
    /// - ユーザーID=1のデータ取得成功
    /// - メールアドレス表示（読み取り専用）
    /// - ユーザー名表示
    /// - ロール選択状態表示
    /// - アクティブ状態表示
    ///
    /// 【期待結果】
    /// - 全フィールドに既存データ表示
    /// - メールアドレス入力欄がdisabled属性あり
    /// </summary>
    [Fact]
    public void Edit_OnInitialized_DisplaysExistingUserData()
    {
        // Arrange - SuperUser権限設定
        SetupSuperUser("admin@test.com");

        // 既存ユーザーデータ準備（F# Domain型）
        var existingUser = CreateTestUser(
            id: "00000000-0000-0000-0000-000000000001",
            email: "test@example.com",
            name: "テストユーザー",
            role: FSharpRole.GeneralUser,
            isActive: true
        );

        // 完全なモックセットアップ（GetAllUsersWithIdentityAsync + GetProjectIdsByUserIdAsync + GetProjectsAsync）
        var identityId = "identity-id-1";
        var builder = SetupEditMocks(existingUser, identityId);
        var mockService = builder.BuildMock();
        Services.AddSingleton(mockService.Object);

        // Act - Editコンポーネントレンダリング（IdentityId指定）
        // Phase B-F3リファクタ対応: Id (UserId) → IdentityId (string)
        var cut = RenderComponent<Edit>(parameters => parameters
            .Add(p => p.IdentityId, identityId));

        // Assert - メールアドレス表示確認（読み取り専用）
        var emailInput = cut.Find("input[data-testid='email-display']");
        emailInput.GetAttribute("value").Should().Be("test@example.com");
        emailInput.HasAttribute("disabled").Should().BeTrue("メールアドレスは変更不可");

        // ユーザー名表示確認
        var nameInput = cut.Find("input[data-testid='name-input']");
        nameInput.GetAttribute("value").Should().Be("テストユーザー");

        // ロール選択状態確認（GeneralUser選択済み）
        var generalUserRadio = cut.Find("input[data-testid='role-dropdown-generaluser']");
        generalUserRadio.GetAttribute("checked").Should().NotBeNull("GeneralUserロールが選択されている");

        // アクティブ状態確認
        var activeCheckbox = cut.Find("input[data-testid='status-toggle']");
        activeCheckbox.GetAttribute("checked").Should().NotBeNull("アクティブ状態が有効");
    }

    /// <summary>
    /// 【初期表示2】ロール選択肢表示（4種類）
    ///
    /// 【検証内容】
    /// - SuperUser
    /// - ProjectManager
    /// - DomainApprover
    /// - GeneralUser
    ///
    /// 【期待結果】
    /// - 4つのラジオボタン表示確認
    /// </summary>
    [Fact]
    public void Edit_OnInitialized_DisplaysAllRoleOptions()
    {
        // Arrange
        SetupSuperUser("admin@test.com");

        var existingUser = CreateTestUser(id: "00000000-0000-0000-0000-000000000001", email: "test@example.com", name: "Test", role: FSharpRole.GeneralUser);
        var identityId = "identity-id-1";
        var builder = SetupEditMocks(existingUser, identityId);
        var mockService = builder.BuildMock();
        Services.AddSingleton(mockService.Object);

        // Act（Phase B-F3リファクタ対応: IdentityId指定）
        var cut = RenderComponent<Edit>(parameters => parameters.Add(p => p.IdentityId, identityId));

        // Assert - 4つのロール選択肢が存在
        var superUserRadio = cut.Find("input[data-testid='role-dropdown-superuser']");
        superUserRadio.Should().NotBeNull();

        var projectManagerRadio = cut.Find("input[data-testid='role-dropdown-projectmanager']");
        projectManagerRadio.Should().NotBeNull();

        var domainApproverRadio = cut.Find("input[data-testid='role-dropdown-domainapprover']");
        domainApproverRadio.Should().NotBeNull();

        var generalUserRadio = cut.Find("input[data-testid='role-dropdown-generaluser']");
        generalUserRadio.Should().NotBeNull();
    }

    /// <summary>
    /// 【初期表示3】プロジェクト選択肢表示（ロールに応じて表示制御）
    ///
    /// 【検証内容】
    /// - SuperUserロールの場合は非表示（UI設計書3.8章準拠）
    /// - ProjectManager/DomainApprover/GeneralUserの場合は表示
    ///
    /// 【期待結果】
    /// - ProjectManagerでは data-testid="project-list" が存在
    /// </summary>
    [Fact]
    public void Edit_OnInitialized_DisplaysProjectListPlaceholder()
    {
        // Arrange
        SetupSuperUser("admin@test.com");

        // ProjectManagerロールのユーザー編集時はプロジェクト選択表示
        var existingUser = CreateTestUser(id: "00000000-0000-0000-0000-000000000001", email: "test@example.com", name: "Test", role: FSharpRole.ProjectManager);
        var identityId = "identity-id-1";
        var builder = SetupEditMocks(existingUser, identityId);
        var mockService = builder.BuildMock();
        Services.AddSingleton(mockService.Object);

        // Act（Phase B-F3リファクタ対応: IdentityId指定）
        var cut = RenderComponent<Edit>(parameters => parameters.Add(p => p.IdentityId, identityId));

        // 非同期初期化完了を待機（loading=false状態）
        cut.WaitForState(() => !cut.Markup.Contains("読み込み中"), timeout: TimeSpan.FromSeconds(5));

        // Assert - ProjectManagerロール編集時はプロジェクト選択領域が表示される
        // model.Role != "SuperUser" の場合に表示（Edit.razor line 218）
        // Edit.razorの実装では、プロジェクト選択エリアに data-testid は設定されていないが、
        // "所属プロジェクト（任意）" のラベルテキストで確認可能
        var projectLabels = cut.FindAll("label.form-label");
        projectLabels.Should().Contain(l => l.TextContent.Contains("所属プロジェクト"), "ProjectManagerロールの場合はプロジェクト選択エリアが表示される");
    }

    /// <summary>
    /// 【初期表示4】ステータス表示（アクティブ/非アクティブ）
    ///
    /// 【検証内容】
    /// - アクティブ状態: チェックボックスがON、バッジが「有効」
    /// - 非アクティブ状態: チェックボックスがOFF、バッジが「無効」
    ///
    /// 【期待結果】
    /// - チェックボックス状態と表示バッジが連動
    /// </summary>
    [Fact]
    public void Edit_OnInitialized_DisplaysCorrectActiveStatus()
    {
        // Arrange - 非アクティブユーザー
        SetupSuperUser("admin@test.com");

        var inactiveUser = CreateTestUser(
            id: "00000000-0000-0000-0000-000000000001",
            email: "inactive@example.com",
            name: "非アクティブユーザー",
            role: FSharpRole.GeneralUser,
            isActive: false
        );

        var identityId = "identity-id-1";
        var builder = SetupEditMocks(inactiveUser, identityId);
        var mockService = builder.BuildMock();
        Services.AddSingleton(mockService.Object);

        // Act（Phase B-F3リファクタ対応: IdentityId指定）
        var cut = RenderComponent<Edit>(parameters => parameters.Add(p => p.IdentityId, identityId));

        // 非同期初期化完了を待機（loading=false状態）
        cut.WaitForState(() => !cut.Markup.Contains("読み込み中"), timeout: TimeSpan.FromSeconds(5));

        // Assert - 非アクティブ状態確認
        var activeCheckbox = cut.Find("input[data-testid='status-toggle']");
        activeCheckbox.GetAttribute("checked").Should().BeNull("非アクティブ状態のためチェックなし");

        // バッジ表示確認（「無効」バッジが表示される）
        // 【補足】Edit.razorではIsActive=falseの場合「無効」バッジを表示
        var badges = cut.FindAll(".badge");
        badges.Should().Contain(b => b.TextContent.Contains("無効"), "非アクティブ状態（無効）のバッジ表示");
    }

    #endregion

    #region 2. 更新処理系テスト（5ケース）

    /// <summary>
    /// 【更新処理1】氏名変更 → 更新成功
    ///
    /// 【検証内容】
    /// - ユーザー名フィールド編集
    /// - UpdateUserAsync成功
    /// - NavigationManager.Uri == "/admin/users" 確認
    ///
    /// 【期待結果】
    /// - ユーザー一覧画面へリダイレクト
    /// - UpdateUserAsync呼び出し確認
    /// </summary>
    [Fact]
    public void Edit_UpdateName_Success_RedirectsToUserList()
    {
        // Arrange
        SetupSuperUser("admin@test.com");

        var existingUser = CreateTestUser(id: "00000000-0000-0000-0000-000000000001", email: "test@example.com", name: "既存名", role: FSharpRole.GeneralUser);
        var updatedUser = CreateTestUser(id: "00000000-0000-0000-0000-000000000001", email: "test@example.com", name: "変更後の名前", role: FSharpRole.GeneralUser);

        var identityId = "identity-id-1";
        var builder = SetupEditMocks(existingUser, identityId);
        builder.SetupUpdateUserSuccess(updatedUser);
        var mockService = builder.BuildMock();
        Services.AddSingleton(mockService.Object);

        // Act - コンポーネントレンダリング（Phase B-F3リファクタ対応: IdentityId指定）
        var cut = RenderComponent<Edit>(parameters => parameters.Add(p => p.IdentityId, identityId));

        // ユーザー名変更
        var nameInput = cut.Find("input[data-testid='name-input']");
        nameInput.Change("変更後の名前");

        // フォーム送信
        var form = cut.Find("form");
        form.Submit();

        // Assert - NavigationManagerリダイレクト確認
        var navMan = Services.GetRequiredService<NavigationManager>();
        navMan.Uri.Should().EndWith("/admin/users", "更新成功時にユーザー一覧へリダイレクト");

        // UpdateUserAsync呼び出し確認（Phase B-F3リファクタ対応: 第1引数がstring型に変更）
        mockService.Verify(
            s => s.UpdateUserAsync(
                It.IsAny<string>(),  // targetIdentityId (変更: UserId → string)
                "変更後の名前",
                It.IsAny<string>(),
                It.IsAny<Microsoft.FSharp.Collections.FSharpList<long>>(),
                It.IsAny<bool>(),
                It.IsAny<string>()  // operatorIdentityId
            ),
            Times.Once,
            "UpdateUserAsyncが1回呼び出される"
        );
    }

    /// <summary>
    /// 【更新処理2】ロール変更 → 更新成功
    ///
    /// 【検証内容】
    /// - ロール選択変更（GeneralUser → ProjectManager）
    /// - UpdateUserAsync成功
    ///
    /// 【期待結果】
    /// - ユーザー一覧画面へリダイレクト
    /// </summary>
    [Fact]
    public async Task Edit_UpdateRole_Success_RedirectsToUserList()
    {
        // Arrange
        SetupSuperUser("admin@test.com");

        var existingUser = CreateTestUser(id: "00000000-0000-0000-0000-000000000001", email: "test@example.com", name: "Test", role: FSharpRole.GeneralUser);
        var updatedUser = CreateTestUser(id: "00000000-0000-0000-0000-000000000001", email: "test@example.com", name: "Test", role: FSharpRole.ProjectManager);

        var identityId = "identity-id-1";
        var builder = SetupEditMocks(existingUser, identityId);
        builder.SetupUpdateUserSuccess(updatedUser);
        var mockService = builder.BuildMock();
        Services.AddSingleton(mockService.Object);

        // Act（Phase B-F3リファクタ対応: IdentityId指定）
        var cut = RenderComponent<Edit>(parameters => parameters.Add(p => p.IdentityId, identityId));

        // ロール変更（GeneralUser → ProjectManager）
        // 【bUnitでInputRadioGroup操作の正しい方法】
        // InputRadioGroupの@bind-Valueを更新するには、ValueChangedイベントを明示的に呼び出す
        var inputRadioGroup = cut.FindComponent<InputRadioGroup<string>>();

        // ValueChangedイベントコールバックをBlazor Dispatcherコンテキストで実行
        await cut.InvokeAsync(async () =>
        {
            await inputRadioGroup.Instance.ValueChanged.InvokeAsync("ProjectManager");
        });

        // フォーム送信
        var form = cut.Find("form");
        form.Submit();

        // Assert
        var navMan = Services.GetRequiredService<NavigationManager>();
        navMan.Uri.Should().EndWith("/admin/users", "ロール変更成功");

        // Phase B-F3リファクタ対応: 第1引数がstring型に変更
        mockService.Verify(
            s => s.UpdateUserAsync(
                It.IsAny<string>(),  // targetIdentityId (変更: UserId → string)
                It.IsAny<string>(),
                "ProjectManager",
                It.IsAny<Microsoft.FSharp.Collections.FSharpList<long>>(),
                It.IsAny<bool>(),
                It.IsAny<string>()  // operatorIdentityId
            ),
            Times.Once,
            "ロールがProjectManagerで更新される"
        );
    }

    /// <summary>
    /// 【更新処理3】ステータス変更 → 更新成功
    ///
    /// 【検証内容】
    /// - アクティブ状態チェックボックス変更
    /// - UpdateUserAsync成功
    ///
    /// 【期待結果】
    /// - isActive=falseで更新される
    /// </summary>
    [Fact]
    public void Edit_UpdateActiveStatus_Success_RedirectsToUserList()
    {
        // Arrange
        SetupSuperUser("admin@test.com");

        var existingUser = CreateTestUser(id: "00000000-0000-0000-0000-000000000001", email: "test@example.com", name: "Test", role: FSharpRole.GeneralUser, isActive: true);
        var updatedUser = CreateTestUser(id: "00000000-0000-0000-0000-000000000001", email: "test@example.com", name: "Test", role: FSharpRole.GeneralUser, isActive: false);

        var identityId = "identity-id-1";
        var builder = SetupEditMocks(existingUser, identityId);
        builder.SetupUpdateUserSuccess(updatedUser);
        var mockService = builder.BuildMock();
        Services.AddSingleton(mockService.Object);

        // Act（Phase B-F3リファクタ対応: IdentityId指定）
        var cut = RenderComponent<Edit>(parameters => parameters.Add(p => p.IdentityId, identityId));

        // アクティブ状態変更（true → false）
        var activeCheckbox = cut.Find("input[data-testid='status-toggle']");
        activeCheckbox.Change(false);

        // フォーム送信
        var form = cut.Find("form");
        form.Submit();

        // Assert
        var navMan = Services.GetRequiredService<NavigationManager>();
        navMan.Uri.Should().EndWith("/admin/users", "ステータス変更成功");

        // Phase B-F3リファクタ対応: 第1引数がstring型に変更
        mockService.Verify(
            s => s.UpdateUserAsync(
                It.IsAny<string>(),  // targetIdentityId (変更: UserId → string)
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<Microsoft.FSharp.Collections.FSharpList<long>>(),
                false,  // isActive = false
                It.IsAny<string>()  // operatorIdentityId
            ),
            Times.Once,
            "isActive=falseで更新される"
        );
    }

    /// <summary>
    /// 【更新処理4】バリデーションエラー表示
    ///
    /// 【検証内容】
    /// - ユーザー名を空欄にして送信
    /// - ValidationSummaryでエラー表示
    ///
    /// 【期待結果】
    /// - 「ユーザー名は必須です」エラーメッセージ表示
    /// - リダイレクトされない
    /// </summary>
    [Fact]
    public void Edit_EmptyName_ShowsValidationError()
    {
        // Arrange
        SetupSuperUser("admin@test.com");

        var existingUser = CreateTestUser(id: "00000000-0000-0000-0000-000000000001", email: "test@example.com", name: "Test", role: FSharpRole.GeneralUser);
        var identityId = "identity-id-1";
        var builder = SetupEditMocks(existingUser, identityId);
        var mockService = builder.BuildMock();
        Services.AddSingleton(mockService.Object);

        // Act（Phase B-F3リファクタ対応: IdentityId指定）
        var cut = RenderComponent<Edit>(parameters => parameters.Add(p => p.IdentityId, identityId));

        // ユーザー名を空欄にする
        var nameInput = cut.Find("input[data-testid='name-input']");
        nameInput.Change("");

        // フォーム送信
        var form = cut.Find("form");
        form.Submit();

        // Assert - ValidationSummaryにエラーメッセージ表示
        var validationSummary = cut.Find(".alert.alert-danger");
        validationSummary.TextContent.Should().Contain("ユーザー名は必須です", "必須フィールド未入力エラー");

        // NavigationManagerリダイレクトされない
        var navMan = Services.GetRequiredService<NavigationManager>();
        navMan.Uri.Should().NotEndWith("/admin/users", "バリデーションエラー時はリダイレクトされない");

        // UpdateUserAsyncが呼び出されない（Phase B-F3リファクタ対応: 第1引数がstring型に変更）
        mockService.Verify(
            s => s.UpdateUserAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Microsoft.FSharp.Collections.FSharpList<long>>(), It.IsAny<bool>(), It.IsAny<string>()),
            Times.Never,
            "バリデーションエラー時はUpdateUserAsyncが呼び出されない"
        );
    }

    /// <summary>
    /// 【更新処理5】サーバーエラー表示
    ///
    /// 【検証内容】
    /// - UpdateUserAsync失敗（リポジトリエラー）
    /// - alert()でエラーメッセージ表示
    ///
    /// 【期待結果】
    /// - JSRuntime.InvokeVoidAsync("alert", "更新エラー: ...") 呼び出し確認
    /// </summary>
    [Fact]
    public void Edit_UpdateUser_ServerError_ShowsAlert()
    {
        // Arrange
        SetupSuperUser("admin@test.com");

        var existingUser = CreateTestUser(id: "00000000-0000-0000-0000-000000000001", email: "test@example.com", name: "Test", role: FSharpRole.GeneralUser);
        var identityId = "identity-id-1";
        var builder = SetupEditMocks(existingUser, identityId);
        builder.SetupUpdateUserFailure("データベース接続エラー");
        var mockService = builder.BuildMock();
        Services.AddSingleton(mockService.Object);

        // JSRuntimeモック設定（alert呼び出し確認用）
        JSInterop.SetupVoid("alert", _ => true).SetVoidResult();

        // Act（Phase B-F3リファクタ対応: IdentityId指定）
        var cut = RenderComponent<Edit>(parameters => parameters.Add(p => p.IdentityId, identityId));

        var nameInput = cut.Find("input[data-testid='name-input']");
        nameInput.Change("変更後の名前");

        var form = cut.Find("form");
        form.Submit();

        // Assert - alert()呼び出し確認
        // bUnitのJSInteropでは、alert呼び出しは記録されるが、
        // 詳細な引数検証はJSRuntimeInvocation履歴から確認する
        var alertInvocations = JSInterop.Invocations["alert"];
        alertInvocations.Should().NotBeEmpty("エラー時にalert()が呼び出される");
        alertInvocations[0].Arguments[0].ToString().Should().Contain("更新エラー", "エラーメッセージが表示される");
    }

    #endregion

    #region 3. パスワードリセット系テスト（3ケース）

    /// <summary>
    /// 【パスワードリセット1】正常系: パスワードリセット成功（TODO Phase B-F3 Step2）
    ///
    /// 【検証内容】
    /// - 新パスワード入力
    /// - パスワード変更API呼び出し（実装予定）
    ///
    /// 【期待結果】
    /// - 現在はパスワード変更処理がスキップされる
    /// - TODO: Phase B-F3 Step2でパスワード変更API実装後にテスト追加
    /// </summary>
    [Fact(Skip = "Phase B-F3 Step2でパスワード変更API実装後に有効化")]
    public void Edit_UpdatePassword_Success_RedirectsToUserList()
    {
        // TODO: Phase B-F3 Step2でパスワード変更API実装後にテスト実装
        Assert.True(true, "パスワード変更API実装待ち");
    }

    /// <summary>
    /// 【パスワードリセット2】異常系: 強度不足エラー（8文字未満）
    ///
    /// 【検証内容】
    /// - 新パスワード入力（7文字）
    /// - ValidationMessageでエラー表示
    ///
    /// 【期待結果】
    /// - 「パスワードは8文字以上100文字以内で入力してください」エラー表示
    ///
    /// 【Skip理由】
    /// パスワードリセット機能はSuperUser権限のみ表示されるため、
    /// 複雑なテストセットアップが必要。Phase B-F3 Step2で詳細テスト追加予定。
    /// </summary>
    [Fact(Skip = "パスワードリセット機能のテストは複雑なため、Phase B-F3 Step2で実装")]
    public void Edit_ShortPassword_ShowsValidationError()
    {
        // Arrange
        SetupSuperUser("admin@test.com");

        var existingUser = CreateTestUser(id: "00000000-0000-0000-0000-000000000001", email: "test@example.com", name: "Test", role: FSharpRole.GeneralUser);
        var identityId = "identity-id-1";
        var builder = SetupEditMocks(existingUser, identityId);
        var mockService = builder.BuildMock();
        Services.AddSingleton(mockService.Object);

        // Act（Phase B-F3リファクタ対応: IdentityId指定）
        var cut = RenderComponent<Edit>(parameters => parameters.Add(p => p.IdentityId, identityId));

        // パスワードリセットモーダルを開く
        var resetButton = cut.Find("button[data-testid='password-reset-button']");
        resetButton.Click();

        // 8文字未満のパスワード入力
        var passwordInput = cut.Find("input[data-testid='new-password-input']");
        passwordInput.Change("Pass123");  // 7文字

        // モーダル内のフォーム送信
        // パスワードリセットモーダル内のフォームは data-testid がないため、
        // モーダル内の EditForm を検索して Submit
        var modalForms = cut.FindAll("form");
        var passwordResetForm = modalForms.Last(); // 最後のフォームがモーダル内フォーム
        passwordResetForm.Submit();

        // Assert - ValidationSummaryにエラーメッセージ表示
        // モーダル内のバリデーションエラーは .alert.alert-danger ではなく、
        // ValidationMessage で表示されるため、.text-danger を検索
        var validationMessages = cut.FindAll(".text-danger");
        validationMessages.Should().NotBeEmpty("パスワード強度バリデーションエラーが表示される");

        var errorText = string.Join(" ", validationMessages.Select(e => e.TextContent));
        errorText.Should().Contain("8文字以上", "パスワード強度エラー");
    }

    /// <summary>
    /// 【パスワードリセット3】異常系: サーバーエラー（TODO Phase B-F3 Step2）
    ///
    /// 【検証内容】
    /// - パスワード変更API失敗（実装予定）
    ///
    /// 【期待結果】
    /// - TODO: Phase B-F3 Step2でパスワード変更API実装後にテスト追加
    /// </summary>
    [Fact(Skip = "Phase B-F3 Step2でパスワード変更API実装後に有効化")]
    public void Edit_UpdatePassword_ServerError_ShowsAlert()
    {
        // TODO: Phase B-F3 Step2でパスワード変更API実装後にテスト実装
        Assert.True(true, "パスワード変更API実装待ち");
    }

    #endregion

    #region 4. 権限制御系テスト（2ケース）

    /// <summary>
    /// 【権限制御1】SuperUser権限: 全ユーザー編集可能
    ///
    /// 【検証内容】
    /// - SuperUser権限でアクセス
    /// - 任意のユーザー編集成功
    ///
    /// 【期待結果】
    /// - 編集画面表示
    /// - 更新処理成功
    /// </summary>
    [Fact]
    public void Edit_SuperUserPermission_CanEditAnyUser()
    {
        // Arrange
        SetupSuperUser("admin@test.com");

        var targetUser = CreateTestUser(id: "00000000-0000-0000-0000-000000000999", email: "target@example.com", name: "Target User", role: FSharpRole.GeneralUser);
        var updatedUser = CreateTestUser(id: "00000000-0000-0000-0000-000000000999", email: "target@example.com", name: "Updated Name", role: FSharpRole.GeneralUser);

        var identityId = "identity-id-999";
        var builder = SetupEditMocks(targetUser, identityId);
        builder.SetupUpdateUserSuccess(updatedUser);
        var mockService = builder.BuildMock();
        Services.AddSingleton(mockService.Object);

        // Act（Phase B-F3リファクタ対応: IdentityId指定）
        var cut = RenderComponent<Edit>(parameters => parameters.Add(p => p.IdentityId, identityId));

        var nameInput = cut.Find("input[data-testid='name-input']");
        nameInput.Change("Updated Name");

        var form = cut.Find("form");
        form.Submit();

        // Assert
        var navMan = Services.GetRequiredService<NavigationManager>();
        navMan.Uri.Should().EndWith("/admin/users", "SuperUserは任意のユーザー編集可能");
    }

    #endregion

    #region 戻るボタン処理テスト

    /// <summary>
    /// 戻るボタン押下 → ユーザー一覧画面へリダイレクト
    ///
    /// 【検証内容】
    /// - 戻るボタン押下
    /// - NavigationManager.Uri == "/admin/users"
    ///
    /// 【期待結果】
    /// - UpdateUserAsync呼び出しなし
    /// - ユーザー一覧画面へリダイレクト
    /// </summary>
    [Fact]
    public void Edit_ClickGoBack_RedirectsToUserList()
    {
        // Arrange
        SetupSuperUser("admin@test.com");

        var existingUser = CreateTestUser(id: "00000000-0000-0000-0000-000000000001", email: "test@example.com", name: "Test", role: FSharpRole.GeneralUser);
        var identityId = "identity-id-1";
        var builder = SetupEditMocks(existingUser, identityId);
        var mockService = builder.BuildMock();
        Services.AddSingleton(mockService.Object);

        // Act（Phase B-F3リファクタ対応: IdentityId指定）
        var cut = RenderComponent<Edit>(parameters => parameters.Add(p => p.IdentityId, identityId));

        var backButton = cut.Find("button[data-testid='back-button']");
        backButton.Click();

        // Assert
        var navMan = Services.GetRequiredService<NavigationManager>();
        navMan.Uri.Should().EndWith("/admin/users", "戻るボタン押下時にユーザー一覧へリダイレクト");

        // UpdateUserAsyncが呼び出されない（Phase B-F3リファクタ対応: 第1引数がstring型に変更）
        mockService.Verify(
            s => s.UpdateUserAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Microsoft.FSharp.Collections.FSharpList<long>>(), It.IsAny<bool>(), It.IsAny<string>()),
            Times.Never,
            "戻るボタン押下時はUpdateUserAsyncが呼び出されない"
        );
    }

    #endregion

    #region F# Domain型テストデータ生成ヘルパー

    /// <summary>
    /// F# Domain型のUserテストデータ生成
    /// </summary>
    private static FSharpDomainUser CreateTestUser(
        string id,
        string email,
        string name,
        FSharpRole role,
        bool isActive = true)
    {
        // F# Smart Constructorを使用して値オブジェクト生成
        var emailResult = FSharpEmail.create(email);
        if (emailResult.IsError)
            throw new InvalidOperationException($"Invalid email: {email}. Error: {emailResult.ErrorValue}");

        var nameResult = FSharpUserName.create(name);
        if (nameResult.IsError)
            throw new InvalidOperationException($"Invalid name: {name}. Error: {nameResult.ErrorValue}");

        var userId = FSharpUserId.create(id);

        // F# User型のファクトリーメソッド createWithId() を使用（4引数のみ）
        // 【重要】23フィールドの手動コンストラクタ呼び出しではなく、Factory Methodを使用
        var user = FSharpDomainUser.createWithId(
            emailResult.ResultValue,
            nameResult.ResultValue,
            role,
            userId
        );

        // isActiveが異なる場合は with式で更新（F# Record Copy Expression）
        // 【F#初学者向け解説】
        // F# Record型は不変（immutable）のため、フィールド更新は"コピー with 変更"で実現します
        if (user.IsActive != isActive)
        {
            // C#からF# Record Copyを実行するには、明示的なコンストラクタ呼び出しが必要
            // （F#の "with" 式はC#から直接使用不可）
            user = new FSharpDomainUser(
                id: user.Id,
                email: user.Email,
                name: user.Name,
                role: user.Role,
                isActive: isActive,  // 変更するフィールドのみ上書き
                isFirstLogin: user.IsFirstLogin,
                passwordHash: user.PasswordHash,
                securityStamp: user.SecurityStamp,
                concurrencyStamp: user.ConcurrencyStamp,
                lockoutEnd: user.LockoutEnd,
                accessFailedCount: user.AccessFailedCount,
                profile: user.Profile,
                projectPermissions: user.ProjectPermissions,
                emailConfirmed: user.EmailConfirmed,
                phoneNumber: user.PhoneNumber,
                phoneNumberConfirmed: user.PhoneNumberConfirmed,
                twoFactorEnabled: user.TwoFactorEnabled,
                lockoutEnabled: user.LockoutEnabled,
                createdAt: user.CreatedAt,
                createdBy: user.CreatedBy,
                updatedAt: user.UpdatedAt,
                updatedBy: user.UpdatedBy
            );
        }

        return user;
    }

    #endregion
}
