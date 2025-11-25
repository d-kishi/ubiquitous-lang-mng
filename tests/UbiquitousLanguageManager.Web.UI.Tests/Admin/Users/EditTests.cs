using System;
using Xunit;
using FluentAssertions;
using Bunit;
using UbiquitousLanguageManager.Web.Tests.Infrastructure;
using UbiquitousLanguageManager.Web.Components.Pages.Admin.Users;
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
            id: 1L,
            email: "test@example.com",
            name: "テストユーザー",
            role: FSharpRole.GeneralUser,
            isActive: true
        );

        // GetUserByIdAsyncモック設定
        var builder = new UserManagementServiceMockBuilder();
        var mockService = builder
            .SetupGetUserByIdSuccess(existingUser)
            .BuildMock();
        Services.AddSingleton(mockService.Object);

        // Act - Editコンポーネントレンダリング（UserId=1）
        var cut = RenderComponent<Edit>(parameters => parameters
            .Add(p => p.Id, 1L));

        // Assert - メールアドレス表示確認（読み取り専用）
        var emailInput = cut.Find("input[data-testid='input-email-readonly']");
        emailInput.GetAttribute("value").Should().Be("test@example.com");
        emailInput.HasAttribute("disabled").Should().BeTrue("メールアドレスは変更不可");

        // ユーザー名表示確認
        var nameInput = cut.Find("input[data-testid='input-name']");
        nameInput.GetAttribute("value").Should().Be("テストユーザー");

        // ロール選択状態確認（GeneralUser選択済み）
        var generalUserRadio = cut.Find("input[data-testid='radio-role-generaluser']");
        generalUserRadio.GetAttribute("checked").Should().NotBeNull("GeneralUserロールが選択されている");

        // アクティブ状態確認
        var activeCheckbox = cut.Find("input[data-testid='checkbox-is-active']");
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

        var existingUser = CreateTestUser(id: 1L, email: "test@example.com", name: "Test", role: FSharpRole.GeneralUser);
        var builder = new UserManagementServiceMockBuilder();
        var mockService = builder.SetupGetUserByIdSuccess(existingUser).BuildMock();
        Services.AddSingleton(mockService.Object);

        // Act
        var cut = RenderComponent<Edit>(parameters => parameters.Add(p => p.Id, 1L));

        // Assert - 4つのロール選択肢が存在
        var superUserRadio = cut.Find("input[data-testid='radio-role-superuser']");
        superUserRadio.Should().NotBeNull();

        var projectManagerRadio = cut.Find("input[data-testid='radio-role-projectmanager']");
        projectManagerRadio.Should().NotBeNull();

        var domainApproverRadio = cut.Find("input[data-testid='radio-role-domainapprover']");
        domainApproverRadio.Should().NotBeNull();

        var generalUserRadio = cut.Find("input[data-testid='radio-role-generaluser']");
        generalUserRadio.Should().NotBeNull();
    }

    /// <summary>
    /// 【初期表示3】プロジェクト選択肢表示（TODO Phase B-F3 Step2）
    ///
    /// 【検証内容】
    /// - プロジェクト選択領域が存在
    /// - 現在はプロジェクトなしメッセージ表示（実装予定）
    ///
    /// 【期待結果】
    /// - data-testid="project-list"が存在
    /// - 「プロジェクトがありません」メッセージ表示
    /// </summary>
    [Fact]
    public void Edit_OnInitialized_DisplaysProjectListPlaceholder()
    {
        // Arrange
        SetupSuperUser("admin@test.com");

        var existingUser = CreateTestUser(id: 1L, email: "test@example.com", name: "Test", role: FSharpRole.ProjectManager);
        var builder = new UserManagementServiceMockBuilder();
        var mockService = builder.SetupGetUserByIdSuccess(existingUser).BuildMock();
        Services.AddSingleton(mockService.Object);

        // Act
        var cut = RenderComponent<Edit>(parameters => parameters.Add(p => p.Id, 1L));

        // Assert - プロジェクト選択領域が存在
        var projectList = cut.Find("[data-testid='project-list']");
        projectList.Should().NotBeNull();

        // TODO: Phase B-F3 Step2実装後は、プロジェクト一覧表示のテストに変更
        projectList.TextContent.Should().Contain("プロジェクトがありません", "Phase B-F3 Step2でプロジェクト一覧取得実装予定");
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
            id: 1L,
            email: "inactive@example.com",
            name: "非アクティブユーザー",
            role: FSharpRole.GeneralUser,
            isActive: false
        );

        var builder = new UserManagementServiceMockBuilder();
        var mockService = builder.SetupGetUserByIdSuccess(inactiveUser).BuildMock();
        Services.AddSingleton(mockService.Object);

        // Act
        var cut = RenderComponent<Edit>(parameters => parameters.Add(p => p.Id, 1L));

        // Assert - 非アクティブ状態確認
        var activeCheckbox = cut.Find("input[data-testid='checkbox-is-active']");
        activeCheckbox.GetAttribute("checked").Should().BeNull("非アクティブ状態のためチェックなし");

        // バッジ表示確認（「無効」バッジが表示される）
        var badges = cut.FindAll(".badge");
        badges.Should().Contain(b => b.TextContent.Contains("無効"), "非アクティブ状態のバッジ表示");
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

        var existingUser = CreateTestUser(id: 1L, email: "test@example.com", name: "既存名", role: FSharpRole.GeneralUser);
        var updatedUser = CreateTestUser(id: 1L, email: "test@example.com", name: "変更後の名前", role: FSharpRole.GeneralUser);

        var builder = new UserManagementServiceMockBuilder();
        var mockService = builder
            .SetupGetUserByIdSuccess(existingUser)
            .SetupUpdateUserSuccess(updatedUser)
            .BuildMock();
        Services.AddSingleton(mockService.Object);

        // Act - コンポーネントレンダリング
        var cut = RenderComponent<Edit>(parameters => parameters.Add(p => p.Id, 1L));

        // ユーザー名変更
        var nameInput = cut.Find("input[data-testid='input-name']");
        nameInput.Change("変更後の名前");

        // フォーム送信
        var form = cut.Find("form");
        form.Submit();

        // Assert - NavigationManagerリダイレクト確認
        var navMan = Services.GetRequiredService<NavigationManager>();
        navMan.Uri.Should().EndWith("/admin/users", "更新成功時にユーザー一覧へリダイレクト");

        // UpdateUserAsync呼び出し確認
        mockService.Verify(
            s => s.UpdateUserAsync(
                It.IsAny<FSharpUserId>(),
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

        var existingUser = CreateTestUser(id: 1L, email: "test@example.com", name: "Test", role: FSharpRole.GeneralUser);
        var updatedUser = CreateTestUser(id: 1L, email: "test@example.com", name: "Test", role: FSharpRole.ProjectManager);

        var builder = new UserManagementServiceMockBuilder();
        var mockService = builder
            .SetupGetUserByIdSuccess(existingUser)
            .SetupUpdateUserSuccess(updatedUser)
            .BuildMock();
        Services.AddSingleton(mockService.Object);

        // Act
        var cut = RenderComponent<Edit>(parameters => parameters.Add(p => p.Id, 1L));

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

        mockService.Verify(
            s => s.UpdateUserAsync(
                It.IsAny<FSharpUserId>(),
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

        var existingUser = CreateTestUser(id: 1L, email: "test@example.com", name: "Test", role: FSharpRole.GeneralUser, isActive: true);
        var updatedUser = CreateTestUser(id: 1L, email: "test@example.com", name: "Test", role: FSharpRole.GeneralUser, isActive: false);

        var builder = new UserManagementServiceMockBuilder();
        var mockService = builder
            .SetupGetUserByIdSuccess(existingUser)
            .SetupUpdateUserSuccess(updatedUser)
            .BuildMock();
        Services.AddSingleton(mockService.Object);

        // Act
        var cut = RenderComponent<Edit>(parameters => parameters.Add(p => p.Id, 1L));

        // アクティブ状態変更（true → false）
        var activeCheckbox = cut.Find("input[data-testid='checkbox-is-active']");
        activeCheckbox.Change(false);

        // フォーム送信
        var form = cut.Find("form");
        form.Submit();

        // Assert
        var navMan = Services.GetRequiredService<NavigationManager>();
        navMan.Uri.Should().EndWith("/admin/users", "ステータス変更成功");

        mockService.Verify(
            s => s.UpdateUserAsync(
                It.IsAny<FSharpUserId>(),
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

        var existingUser = CreateTestUser(id: 1L, email: "test@example.com", name: "Test", role: FSharpRole.GeneralUser);
        var builder = new UserManagementServiceMockBuilder();
        var mockService = builder.SetupGetUserByIdSuccess(existingUser).BuildMock();
        Services.AddSingleton(mockService.Object);

        // Act
        var cut = RenderComponent<Edit>(parameters => parameters.Add(p => p.Id, 1L));

        // ユーザー名を空欄にする
        var nameInput = cut.Find("input[data-testid='input-name']");
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

        // UpdateUserAsyncが呼び出されない
        mockService.Verify(
            s => s.UpdateUserAsync(It.IsAny<FSharpUserId>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Microsoft.FSharp.Collections.FSharpList<long>>(), It.IsAny<bool>(), It.IsAny<string>()),
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

        var existingUser = CreateTestUser(id: 1L, email: "test@example.com", name: "Test", role: FSharpRole.GeneralUser);
        var builder = new UserManagementServiceMockBuilder();
        var mockService = builder
            .SetupGetUserByIdSuccess(existingUser)
            .SetupUpdateUserFailure("データベース接続エラー")
            .BuildMock();
        Services.AddSingleton(mockService.Object);

        // JSRuntimeモック設定（alert呼び出し確認用）
        JSInterop.SetupVoid("alert", _ => true).SetVoidResult();

        // Act
        var cut = RenderComponent<Edit>(parameters => parameters.Add(p => p.Id, 1L));

        var nameInput = cut.Find("input[data-testid='input-name']");
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
    /// </summary>
    [Fact]
    public void Edit_ShortPassword_ShowsValidationError()
    {
        // Arrange
        SetupSuperUser("admin@test.com");

        var existingUser = CreateTestUser(id: 1L, email: "test@example.com", name: "Test", role: FSharpRole.GeneralUser);
        var builder = new UserManagementServiceMockBuilder();
        var mockService = builder.SetupGetUserByIdSuccess(existingUser).BuildMock();
        Services.AddSingleton(mockService.Object);

        // Act
        var cut = RenderComponent<Edit>(parameters => parameters.Add(p => p.Id, 1L));

        // 8文字未満のパスワード入力
        var passwordInput = cut.Find("input[data-testid='input-new-password']");
        passwordInput.Change("Pass123");  // 7文字

        // フォーム送信
        var form = cut.Find("form");
        form.Submit();

        // Assert - ValidationSummaryにエラーメッセージ表示
        var validationSummary = cut.Find(".alert.alert-danger");
        validationSummary.TextContent.Should().Contain("8文字以上", "パスワード強度エラー");
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

        var targetUser = CreateTestUser(id: 999L, email: "target@example.com", name: "Target User", role: FSharpRole.GeneralUser);
        var updatedUser = CreateTestUser(id: 999L, email: "target@example.com", name: "Updated Name", role: FSharpRole.GeneralUser);

        var builder = new UserManagementServiceMockBuilder();
        var mockService = builder
            .SetupGetUserByIdSuccess(targetUser)
            .SetupUpdateUserSuccess(updatedUser)
            .BuildMock();
        Services.AddSingleton(mockService.Object);

        // Act
        var cut = RenderComponent<Edit>(parameters => parameters.Add(p => p.Id, 999L));

        var nameInput = cut.Find("input[data-testid='input-name']");
        nameInput.Change("Updated Name");

        var form = cut.Find("form");
        form.Submit();

        // Assert
        var navMan = Services.GetRequiredService<NavigationManager>();
        navMan.Uri.Should().EndWith("/admin/users", "SuperUserは任意のユーザー編集可能");
    }

    /// <summary>
    /// 【権限制御2】ProjectManager権限: 担当ユーザーのみ編集可能（TODO Phase B-F3 Step2）
    ///
    /// 【検証内容】
    /// - ProjectManager権限でアクセス
    /// - 担当プロジェクトのユーザーのみ編集可能（実装予定）
    ///
    /// 【期待結果】
    /// - TODO: Phase B-F3 Step2で権限制御ロジック実装後にテスト追加
    /// </summary>
    [Fact(Skip = "Phase B-F3 Step2でProjectManager権限制御実装後に有効化")]
    public void Edit_ProjectManagerPermission_CanEditOwnedProjectUsers()
    {
        // TODO: Phase B-F3 Step2でProjectManager権限制御実装後にテスト実装
        Assert.True(true, "ProjectManager権限制御実装待ち");
    }

    #endregion

    #region キャンセル処理テスト

    /// <summary>
    /// キャンセルボタン押下 → ユーザー一覧画面へリダイレクト
    ///
    /// 【検証内容】
    /// - キャンセルボタン押下
    /// - NavigationManager.Uri == "/admin/users"
    ///
    /// 【期待結果】
    /// - UpdateUserAsync呼び出しなし
    /// - ユーザー一覧画面へリダイレクト
    /// </summary>
    [Fact]
    public void Edit_ClickCancel_RedirectsToUserList()
    {
        // Arrange
        SetupSuperUser("admin@test.com");

        var existingUser = CreateTestUser(id: 1L, email: "test@example.com", name: "Test", role: FSharpRole.GeneralUser);
        var builder = new UserManagementServiceMockBuilder();
        var mockService = builder.SetupGetUserByIdSuccess(existingUser).BuildMock();
        Services.AddSingleton(mockService.Object);

        // Act
        var cut = RenderComponent<Edit>(parameters => parameters.Add(p => p.Id, 1L));

        var cancelButton = cut.Find("button[data-testid='btn-cancel']");
        cancelButton.Click();

        // Assert
        var navMan = Services.GetRequiredService<NavigationManager>();
        navMan.Uri.Should().EndWith("/admin/users", "キャンセル時にユーザー一覧へリダイレクト");

        // UpdateUserAsyncが呼び出されない
        mockService.Verify(
            s => s.UpdateUserAsync(It.IsAny<FSharpUserId>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Microsoft.FSharp.Collections.FSharpList<long>>(), It.IsAny<bool>(), It.IsAny<string>()),
            Times.Never,
            "キャンセル時はUpdateUserAsyncが呼び出されない"
        );
    }

    #endregion

    #region F# Domain型テストデータ生成ヘルパー

    /// <summary>
    /// F# Domain型のUserテストデータ生成
    /// </summary>
    private static FSharpDomainUser CreateTestUser(
        long id,
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
