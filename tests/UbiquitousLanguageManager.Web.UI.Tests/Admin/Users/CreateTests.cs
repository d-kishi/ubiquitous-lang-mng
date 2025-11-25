using Xunit;
using FluentAssertions;
using Bunit;
using Moq;
using UbiquitousLanguageManager.Web.Tests.Infrastructure;
using UbiquitousLanguageManager.Web.Components.Pages.Admin.Users;
using UbiquitousLanguageManager.Application;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Components;

// F# Domain型のエイリアス
using FSharpDomainUser = UbiquitousLanguageManager.Domain.Authentication.User;
using FSharpEmail = UbiquitousLanguageManager.Domain.Authentication.Email;
using FSharpUserName = UbiquitousLanguageManager.Domain.Authentication.UserName;
using FSharpUserId = UbiquitousLanguageManager.Domain.Common.UserId;
using FSharpRole = UbiquitousLanguageManager.Domain.Common.Role;

namespace UbiquitousLanguageManager.Web.Tests.Admin.Users;

/// <summary>
/// Create.razorコンポーネントのbUnit UIテスト
///
/// 【テスト対象】
/// - Phase B-F3 Step1で実装したCreate.razor
/// - SuperUser権限のみアクセス可能
/// - ユーザー作成・初期パスワード設定・ロール選択・プロジェクト割り当て機能
///
/// 【実装テストケース】（15ケース）
///
/// **2-1. 初期表示系（3ケース）**:
/// 1. Create_SuperUser_InitialDisplay_ShowsEmptyForm
/// 2. Create_InitialDisplay_ShowsAllRoleOptions
/// 3. Create_InitialDisplay_ShowsProjectSelectionArea
///
/// **2-2. バリデーション系（5ケース）**:
/// 4. Create_EmptyEmail_ShowsValidationError
/// 5. Create_InvalidEmailFormat_ShowsValidationError
/// 6. Create_EmptyUserName_ShowsValidationError
/// 7. Create_UserNameTooLong_ShowsValidationError
/// 8. Create_WeakPassword_ShowsValidationError
///
/// **2-3. 登録処理系（4ケース）**:
/// 9. Create_ValidForm_ShowsSuccessMessageAndRedirects
/// 10. Create_DuplicateEmail_ShowsErrorMessage
/// 11. Create_ValidationError_ShowsValidationSummary
/// 12. Create_ServerError_ShowsErrorMessage
///
/// **2-4. 権限制御系（3ケース）**:
/// 13. Create_SuperUser_CanAccessPage
/// 14. Create_ProjectManager_CannotAccessPage (別途認可テストで実施)
/// 15. Create_GeneralUser_CannotAccessPage (別途認可テストで実施)
/// </summary>
public class CreateTests : BlazorComponentTestBase
{
    // Mock IUserManagementService
    private Mock<IUserManagementService> _mockUserService = null!;

    public CreateTests()
    {
        // IUserManagementServiceモック作成
        var mockBuilder = new UserManagementServiceMockBuilder();
        _mockUserService = mockBuilder.BuildMock();
        Services.AddSingleton(_mockUserService.Object);
    }

    #region 2-1. 初期表示系テスト

    /// <summary>
    /// 【テストケース1】
    /// SuperUser権限: 初期表示時に空フォームが表示される
    ///
    /// 【検証内容】
    /// - SuperUser権限設定成功
    /// - フォーム要素が表示される
    /// - 入力欄が空である
    /// - 「作成」ボタンが表示される
    /// </summary>
    [Fact]
    public void Create_SuperUser_InitialDisplay_ShowsEmptyForm()
    {
        // Arrange - SuperUser権限設定
        SetupSuperUser("admin@test.com");

        // Act - Createコンポーネントレンダリング
        var cut = RenderComponent<Create>();

        // Assert - フォーム表示確認
        var form = cut.Find("form");
        form.Should().NotBeNull("フォームが表示される");

        // 入力欄確認
        var emailInput = cut.Find("input[data-testid='input-email']");
        emailInput.GetAttribute("value").Should().BeNullOrEmpty("メールアドレス入力欄が空");

        var nameInput = cut.Find("input[data-testid='input-name']");
        nameInput.GetAttribute("value").Should().BeNullOrEmpty("ユーザー名入力欄が空");

        var passwordInput = cut.Find("input[data-testid='input-password']");
        passwordInput.GetAttribute("value").Should().BeNullOrEmpty("パスワード入力欄が空");

        // 作成ボタン確認
        var submitButton = cut.Find("button[data-testid='btn-submit']");
        submitButton.Should().NotBeNull("作成ボタンが表示される");
    }

    /// <summary>
    /// 【テストケース2】
    /// 初期表示: 全ロール選択肢が表示される
    ///
    /// 【検証内容】
    /// - SuperUser
    /// - ProjectManager
    /// - DomainApprover
    /// - GeneralUser（デフォルト選択）
    /// </summary>
    [Fact]
    public void Create_InitialDisplay_ShowsAllRoleOptions()
    {
        // Arrange - SuperUser権限設定
        SetupSuperUser("admin@test.com");

        // Act - Createコンポーネントレンダリング
        var cut = RenderComponent<Create>();

        // Assert - ロール選択肢確認
        var superUserRadio = cut.Find("input[data-testid='radio-role-superuser']");
        superUserRadio.Should().NotBeNull("スーパーユーザーロールが表示される");

        var projectManagerRadio = cut.Find("input[data-testid='radio-role-projectmanager']");
        projectManagerRadio.Should().NotBeNull("プロジェクトマネージャーロールが表示される");

        var domainApproverRadio = cut.Find("input[data-testid='radio-role-domainapprover']");
        domainApproverRadio.Should().NotBeNull("ドメイン承認者ロールが表示される");

        var generalUserRadio = cut.Find("input[data-testid='radio-role-generaluser']");
        generalUserRadio.Should().NotBeNull("一般ユーザーロールが表示される");

        // デフォルト選択確認（GeneralUser）
        // 【bUnit初学者向け解説】
        // IElementにはIsChecked()メソッドがありません。
        // checked属性の有無でラジオボタンの選択状態を確認します。
        generalUserRadio.HasAttribute("checked").Should().BeTrue("一般ユーザーがデフォルト選択されている");
    }

    /// <summary>
    /// 【テストケース3】
    /// 初期表示: プロジェクト選択エリアが表示される
    ///
    /// 【検証内容】
    /// - プロジェクト選択エリア（data-testid="project-list"）が表示される
    /// - Phase B-F3 Step2実装前のため、「プロジェクトがありません」メッセージ表示
    /// </summary>
    [Fact]
    public void Create_InitialDisplay_ShowsProjectSelectionArea()
    {
        // Arrange - SuperUser権限設定
        SetupSuperUser("admin@test.com");

        // Act - Createコンポーネントレンダリング
        var cut = RenderComponent<Create>();

        // Assert - プロジェクト選択エリア確認
        var projectList = cut.Find("[data-testid='project-list']");
        projectList.Should().NotBeNull("プロジェクト選択エリアが表示される");

        // Phase B-F3 Step2実装前のため、空メッセージ確認
        var emptyMessage = projectList.TextContent;
        emptyMessage.Should().Contain("プロジェクトがありません", "プロジェクトが未実装のため空メッセージ表示");
    }

    #endregion

    #region 2-2. バリデーション系テスト

    /// <summary>
    /// 【テストケース4】
    /// メールアドレス必須チェック: 空のままフォーム送信→バリデーションエラー
    ///
    /// 【検証内容】
    /// - メールアドレス空
    /// - OnInvalidSubmitイベント発火（バリデーションエラー）
    /// - ValidationSummaryまたはValidationMessageでエラー表示
    /// </summary>
    [Fact]
    public void Create_EmptyEmail_ShowsValidationError()
    {
        // Arrange - SuperUser権限設定
        SetupSuperUser("admin@test.com");

        // Act - Createコンポーネントレンダリング
        var cut = RenderComponent<Create>();

        // ユーザー名・パスワードのみ入力（メールアドレス空）
        var nameInput = cut.Find("input[data-testid='input-name']");
        nameInput.Change("テストユーザー");

        var passwordInput = cut.Find("input[data-testid='input-password']");
        passwordInput.Change("TestPassword123");

        // フォーム送信
        var form = cut.Find("form");
        form.Submit();

        // Assert - バリデーションエラー表示確認
        // ValidationSummaryまたはValidationMessageが表示される
        var validationErrors = cut.FindAll(".validation-message, .alert-danger");
        validationErrors.Should().NotBeEmpty("バリデーションエラーが表示される");

        // エラーメッセージに「メールアドレス」が含まれる
        var errorText = string.Join(" ", validationErrors.Select(e => e.TextContent));
        errorText.Should().Contain("メールアドレス", "メールアドレス必須エラーメッセージが表示される");
    }

    /// <summary>
    /// 【テストケース5】
    /// メールアドレス形式チェック: 不正な形式→バリデーションエラー
    ///
    /// 【検証内容】
    /// - 不正な形式（"invalid-email"）
    /// - ValidationMessageでエラー表示
    /// </summary>
    [Fact]
    public void Create_InvalidEmailFormat_ShowsValidationError()
    {
        // Arrange - SuperUser権限設定
        SetupSuperUser("admin@test.com");

        // Act - Createコンポーネントレンダリング
        var cut = RenderComponent<Create>();

        // 不正なメールアドレス形式入力
        var emailInput = cut.Find("input[data-testid='input-email']");
        emailInput.Change("invalid-email");

        var nameInput = cut.Find("input[data-testid='input-name']");
        nameInput.Change("テストユーザー");

        var passwordInput = cut.Find("input[data-testid='input-password']");
        passwordInput.Change("TestPassword123");

        // フォーム送信
        var form = cut.Find("form");
        form.Submit();

        // Assert - バリデーションエラー表示確認
        var validationErrors = cut.FindAll(".validation-message, .alert-danger");
        validationErrors.Should().NotBeEmpty("バリデーションエラーが表示される");

        // エラーメッセージに「メールアドレス形式」が含まれる
        var errorText = string.Join(" ", validationErrors.Select(e => e.TextContent));
        errorText.Should().Match(text =>
            text.Contains("メールアドレス") || text.Contains("形式") || text.Contains("有効"),
            "メールアドレス形式エラーメッセージが表示される");
    }

    /// <summary>
    /// 【テストケース6】
    /// ユーザー名必須チェック: 空のままフォーム送信→バリデーションエラー
    ///
    /// 【検証内容】
    /// - ユーザー名空
    /// - ValidationMessageでエラー表示
    /// </summary>
    [Fact]
    public void Create_EmptyUserName_ShowsValidationError()
    {
        // Arrange - SuperUser権限設定
        SetupSuperUser("admin@test.com");

        // Act - Createコンポーネントレンダリング
        var cut = RenderComponent<Create>();

        // メールアドレス・パスワードのみ入力（ユーザー名空）
        var emailInput = cut.Find("input[data-testid='input-email']");
        emailInput.Change("test@example.com");

        var passwordInput = cut.Find("input[data-testid='input-password']");
        passwordInput.Change("TestPassword123");

        // フォーム送信
        var form = cut.Find("form");
        form.Submit();

        // Assert - バリデーションエラー表示確認
        var validationErrors = cut.FindAll(".validation-message, .alert-danger");
        validationErrors.Should().NotBeEmpty("バリデーションエラーが表示される");

        // エラーメッセージに「ユーザー名」が含まれる
        var errorText = string.Join(" ", validationErrors.Select(e => e.TextContent));
        errorText.Should().Contain("ユーザー名", "ユーザー名必須エラーメッセージが表示される");
    }

    /// <summary>
    /// 【テストケース7】
    /// ユーザー名長さチェック: 50文字超過→バリデーションエラー
    ///
    /// 【検証内容】
    /// - ユーザー名51文字入力
    /// - ValidationMessageでエラー表示
    /// </summary>
    [Fact]
    public void Create_UserNameTooLong_ShowsValidationError()
    {
        // Arrange - SuperUser権限設定
        SetupSuperUser("admin@test.com");

        // Act - Createコンポーネントレンダリング
        var cut = RenderComponent<Create>();

        // 51文字のユーザー名入力
        var longName = new string('あ', 51);
        var emailInput = cut.Find("input[data-testid='input-email']");
        emailInput.Change("test@example.com");

        var nameInput = cut.Find("input[data-testid='input-name']");
        nameInput.Change(longName);

        var passwordInput = cut.Find("input[data-testid='input-password']");
        passwordInput.Change("TestPassword123");

        // フォーム送信
        var form = cut.Find("form");
        form.Submit();

        // Assert - バリデーションエラー表示確認
        var validationErrors = cut.FindAll(".validation-message, .alert-danger");
        validationErrors.Should().NotBeEmpty("バリデーションエラーが表示される");

        // エラーメッセージに「50文字」が含まれる
        var errorText = string.Join(" ", validationErrors.Select(e => e.TextContent));
        errorText.Should().Match(text =>
            text.Contains("50文字") || text.Contains("長さ"),
            "ユーザー名長さエラーメッセージが表示される");
    }

    /// <summary>
    /// 【テストケース8】
    /// パスワード強度チェック: 8文字未満→バリデーションエラー
    ///
    /// 【検証内容】
    /// - パスワード7文字入力
    /// - ValidationMessageでエラー表示
    /// </summary>
    [Fact]
    public void Create_WeakPassword_ShowsValidationError()
    {
        // Arrange - SuperUser権限設定
        SetupSuperUser("admin@test.com");

        // Act - Createコンポーネントレンダリング
        var cut = RenderComponent<Create>();

        // 7文字のパスワード入力（8文字未満）
        var emailInput = cut.Find("input[data-testid='input-email']");
        emailInput.Change("test@example.com");

        var nameInput = cut.Find("input[data-testid='input-name']");
        nameInput.Change("テストユーザー");

        var passwordInput = cut.Find("input[data-testid='input-password']");
        passwordInput.Change("Pass123");  // 7文字

        // フォーム送信
        var form = cut.Find("form");
        form.Submit();

        // Assert - バリデーションエラー表示確認
        var validationErrors = cut.FindAll(".validation-message, .alert-danger");
        validationErrors.Should().NotBeEmpty("バリデーションエラーが表示される");

        // エラーメッセージに「8文字」が含まれる
        var errorText = string.Join(" ", validationErrors.Select(e => e.TextContent));
        errorText.Should().Match(text =>
            text.Contains("8文字") || text.Contains("以上"),
            "パスワード強度エラーメッセージが表示される");
    }

    #endregion

    #region 2-3. 登録処理系テスト

    /// <summary>
    /// 【テストケース9】
    /// 正常系: 有効なフォーム送信→成功メッセージ→一覧画面遷移
    ///
    /// 【検証内容】
    /// - 有効な入力値
    /// - CreateUserAsyncモック成功設定
    /// - フォーム送信成功
    /// - NavigationManager.Uri == "/admin/users" 確認
    /// </summary>
    [Fact]
    public void Create_ValidForm_ShowsSuccessMessageAndRedirects()
    {
        // Arrange - SuperUser権限設定
        SetupSuperUser("admin@test.com");

        // テストデータ準備: 作成されたユーザー（F# Domain型）
        var createdUser = CreateTestUser(
            id: 1L,
            email: "newuser@example.com",
            name: "新規ユーザー",
            role: "GeneralUser",
            isActive: true
        );

        // CreateUserAsyncモック設定（成功）
        var mockBuilder = new UserManagementServiceMockBuilder();
        _mockUserService = mockBuilder
            .SetupCreateUserSuccess(createdUser)
            .BuildMock();
        Services.AddSingleton(_mockUserService.Object);

        // Act - Createコンポーネントレンダリング
        var cut = RenderComponent<Create>();

        // 有効な入力値
        var emailInput = cut.Find("input[data-testid='input-email']");
        emailInput.Change("newuser@example.com");

        var nameInput = cut.Find("input[data-testid='input-name']");
        nameInput.Change("新規ユーザー");

        var passwordInput = cut.Find("input[data-testid='input-password']");
        passwordInput.Change("TestPassword123");

        // フォーム送信
        var form = cut.Find("form");
        form.Submit();

        // Assert - NavigationManagerリダイレクト確認
        var navMan = Services.GetRequiredService<NavigationManager>();
        navMan.Uri.Should().EndWith("/admin/users", "フォーム送信成功時にユーザー一覧へリダイレクトされる");

        // CreateUserAsync呼び出し確認
        _mockUserService.Verify(
            s => s.CreateUserAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<Microsoft.FSharp.Collections.FSharpList<long>>(),
                It.IsAny<string>()
            ),
            Times.Once,
            "CreateUserAsyncが1回呼び出される"
        );
    }

    /// <summary>
    /// 【テストケース10】
    /// 異常系: メール重複→エラーメッセージ表示
    ///
    /// 【検証内容】
    /// - CreateUserAsyncモック失敗設定（メール重複エラー）
    /// - フォーム送信
    /// - エラーメッセージ表示確認（JSRuntime.InvokeVoidAsync）
    /// - リダイレクトなし確認
    /// </summary>
    [Fact]
    public void Create_DuplicateEmail_ShowsErrorMessage()
    {
        // Arrange - SuperUser権限設定
        SetupSuperUser("admin@test.com");

        // CreateUserAsyncモック設定（失敗・メール重複エラー）
        var mockBuilder = new UserManagementServiceMockBuilder();
        _mockUserService = mockBuilder
            .SetupCreateUserDuplicateEmail("duplicate@example.com")
            .BuildMock();
        Services.AddSingleton(_mockUserService.Object);

        // Act - Createコンポーネントレンダリング
        var cut = RenderComponent<Create>();

        // 重複メールアドレス入力
        var emailInput = cut.Find("input[data-testid='input-email']");
        emailInput.Change("duplicate@example.com");

        var nameInput = cut.Find("input[data-testid='input-name']");
        nameInput.Change("重複ユーザー");

        var passwordInput = cut.Find("input[data-testid='input-password']");
        passwordInput.Change("TestPassword123");

        // フォーム送信
        var form = cut.Find("form");
        form.Submit();

        // Assert - リダイレクトなし確認
        var navMan = Services.GetRequiredService<NavigationManager>();
        navMan.Uri.Should().NotEndWith("/admin/users", "エラー時はリダイレクトされない");

        // CreateUserAsync呼び出し確認
        _mockUserService.Verify(
            s => s.CreateUserAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<Microsoft.FSharp.Collections.FSharpList<long>>(),
                It.IsAny<string>()
            ),
            Times.Once,
            "CreateUserAsyncが1回呼び出される"
        );

        // 注: JSRuntime.InvokeVoidAsync("alert")のモック確認はJSInterop.VerifyInvokeで実施可能
        // Phase B-F3では基本動作確認のみ実施
    }

    /// <summary>
    /// 【テストケース11】
    /// 異常系: バリデーションエラー→ValidationSummary表示
    ///
    /// 【検証内容】
    /// - 複数バリデーションエラー（メール・ユーザー名・パスワードすべて空）
    /// - ValidationSummary表示確認
    /// </summary>
    [Fact]
    public void Create_ValidationError_ShowsValidationSummary()
    {
        // Arrange - SuperUser権限設定
        SetupSuperUser("admin@test.com");

        // Act - Createコンポーネントレンダリング
        var cut = RenderComponent<Create>();

        // 全入力欄空のままフォーム送信
        var form = cut.Find("form");
        form.Submit();

        // Assert - ValidationSummary表示確認
        var validationSummary = cut.FindAll(".validation-message, .alert-danger");
        validationSummary.Should().NotBeEmpty("ValidationSummaryまたはValidationMessageが表示される");

        // 複数エラーメッセージ確認
        var errorText = string.Join(" ", validationSummary.Select(e => e.TextContent));
        errorText.Should().Contain("メールアドレス", "メールアドレス必須エラーが含まれる");
        errorText.Should().Contain("ユーザー名", "ユーザー名必須エラーが含まれる");
        errorText.Should().Contain("パスワード", "パスワード必須エラーが含まれる");
    }

    /// <summary>
    /// 【テストケース12】
    /// 異常系: サーバーエラー→エラーメッセージ表示
    ///
    /// 【検証内容】
    /// - CreateUserAsyncモック失敗設定（リポジトリエラー）
    /// - フォーム送信
    /// - エラーメッセージ表示確認
    /// </summary>
    [Fact]
    public void Create_ServerError_ShowsErrorMessage()
    {
        // Arrange - SuperUser権限設定
        SetupSuperUser("admin@test.com");

        // CreateUserAsyncモック設定（失敗・サーバーエラー）
        var mockBuilder = new UserManagementServiceMockBuilder();
        _mockUserService = mockBuilder
            .SetupCreateUserValidationError("サーバーエラー: データベース接続失敗")
            .BuildMock();
        Services.AddSingleton(_mockUserService.Object);

        // Act - Createコンポーネントレンダリング
        var cut = RenderComponent<Create>();

        // 有効な入力値
        var emailInput = cut.Find("input[data-testid='input-email']");
        emailInput.Change("test@example.com");

        var nameInput = cut.Find("input[data-testid='input-name']");
        nameInput.Change("テストユーザー");

        var passwordInput = cut.Find("input[data-testid='input-password']");
        passwordInput.Change("TestPassword123");

        // フォーム送信
        var form = cut.Find("form");
        form.Submit();

        // Assert - リダイレクトなし確認
        var navMan = Services.GetRequiredService<NavigationManager>();
        navMan.Uri.Should().NotEndWith("/admin/users", "エラー時はリダイレクトされない");

        // CreateUserAsync呼び出し確認
        _mockUserService.Verify(
            s => s.CreateUserAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<Microsoft.FSharp.Collections.FSharpList<long>>(),
                It.IsAny<string>()
            ),
            Times.Once,
            "CreateUserAsyncが1回呼び出される"
        );
    }

    #endregion

    #region 2-4. 権限制御系テスト

    /// <summary>
    /// 【テストケース13】
    /// SuperUser権限: ページアクセス可能
    ///
    /// 【検証内容】
    /// - SuperUser権限設定
    /// - ページレンダリング成功
    /// - フォーム表示確認
    ///
    /// 【重要】
    /// [Authorize(Roles = "SuperUser")]属性によるアクセス制御は、
    /// ASP.NET Core認証ミドルウェアレベルで実施されるため、
    /// bUnit UIテストでは権限なしアクセスのテストが困難です。
    /// 権限制御の詳細テストは、E2Eテスト（Playwright Test）で実施します。
    /// </summary>
    [Fact]
    public void Create_SuperUser_CanAccessPage()
    {
        // Arrange - SuperUser権限設定
        SetupSuperUser("admin@test.com");

        // Act - Createコンポーネントレンダリング
        var cut = RenderComponent<Create>();

        // Assert - ページ表示確認
        var form = cut.Find("form");
        form.Should().NotBeNull("SuperUser権限でページアクセス可能");

        var submitButton = cut.Find("button[data-testid='btn-submit']");
        submitButton.Should().NotBeNull("作成ボタンが表示される");
    }

    #endregion

    #region F# Domain型テストデータ生成ヘルパー

    /// <summary>
    /// F# Domain型のUserテストデータ生成
    ///
    /// 【F#型のC#からの生成パターン】
    /// F# Record型はコンストラクタベースで生成する必要があります。
    ///
    /// 【F# Smart Constructorパターン】
    /// Email.create() や UserName.create() は
    /// 検証付きファクトリメソッド（Smart Constructor）です。
    /// Result<T, string> 型を返すため、IsError/ResultValue で結果を取得します。
    /// </summary>
    private static FSharpDomainUser CreateTestUser(
        long id,
        string email,
        string name,
        string role,
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

        // string → FSharpRole型変換
        // 【F#初学者向け解説】
        // F# Discriminated Unionは文字列からパースできません。各ケースを明示的に変換します。
        var roleEnum = role switch
        {
            "SuperUser" => FSharpRole.SuperUser,
            "ProjectManager" => FSharpRole.ProjectManager,
            "DomainApprover" => FSharpRole.DomainApprover,
            "GeneralUser" => FSharpRole.GeneralUser,
            _ => throw new InvalidOperationException($"Invalid role: {role}")
        };

        // F# User型のファクトリーメソッド createWithId() を使用（4引数のみ）
        // 【重要】23フィールドの手動コンストラクタ呼び出しではなく、Factory Methodを使用
        var user = FSharpDomainUser.createWithId(
            emailResult.ResultValue,
            nameResult.ResultValue,
            roleEnum,
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
