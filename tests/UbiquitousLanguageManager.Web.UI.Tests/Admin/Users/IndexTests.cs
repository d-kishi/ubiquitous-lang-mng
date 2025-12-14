using Xunit;
using FluentAssertions;
using Bunit;
using Moq;
using UbiquitousLanguageManager.Web.Tests.Infrastructure;
using UbiquitousLanguageManager.Web.Components.Pages.Admin.Users;

// F# Domain型のエイリアス
using FSharpDomainUser = UbiquitousLanguageManager.Domain.Authentication.User;
using FSharpEmail = UbiquitousLanguageManager.Domain.Authentication.Email;
using FSharpUserName = UbiquitousLanguageManager.Domain.Authentication.UserName;
using FSharpUserId = UbiquitousLanguageManager.Domain.Common.UserId;
using FSharpRole = UbiquitousLanguageManager.Domain.Common.Role;

namespace UbiquitousLanguageManager.Web.Tests.Admin.Users;

/// <summary>
/// Index.razorコンポーネントのbUnit UIテスト（Phase B-F3 Step1 Part 2）
///
/// 【テスト対象】
/// - ユーザー一覧画面（/admin/users）
/// - SuperUser権限専用画面（@attribute [Authorize(Roles = "SuperUser")]）
/// - 一覧表示・検索・フィルタ・ソート・ページング機能
/// - ユーザー有効化・無効化操作
///
/// 【Modified Red Phase TDD】
/// - 仕様ベースのテストケース作成
/// - Pass（Green）: 既存実装が仕様準拠 → 成功として記録
/// - Fail（Red）: 仕様ギャップ発見 → 失敗テスト一覧を報告
/// </summary>
public class IndexTests : BlazorComponentTestBase
{
    #region 1. 一覧表示系テスト（5ケース）

    /// <summary>
    /// 【テストケース1-1】
    /// SuperUser権限: 全ユーザー表示
    ///
    /// 【検証内容】
    /// - SuperUser権限設定
    /// - GetAllUsersAsyncモック成功（3ユーザー）
    /// - テーブルに3行表示されることを確認
    /// - ユーザー情報（Email, Name, Role, Status）表示確認
    /// </summary>
    [Fact]
    public void Index_SuperUser_DisplaysAllUsers()
    {
        // Arrange - SuperUser権限設定
        SetupSuperUser("admin@test.com");

        // テストデータ準備（3ユーザー）
        var testUsers = new List<FSharpDomainUser>
        {
            CreateTestUser(
                id: "00000000-0000-0000-0000-000000000001",
                email: "user1@test.com",
                name: "ユーザー1",
                role: FSharpRole.GeneralUser,
                isActive: true
            ),
            CreateTestUser(
                id: "00000000-0000-0000-0000-000000000002",
                email: "user2@test.com",
                name: "ユーザー2",
                role: FSharpRole.DomainApprover,
                isActive: true
            ),
            CreateTestUser(
                id: "00000000-0000-0000-0000-000000000003",
                email: "user3@test.com",
                name: "ユーザー3",
                role: FSharpRole.ProjectManager,
                isActive: false
            )
        };

        // GetAllUsersWithIdentityAsyncモックセットアップ（Phase B-F3リファクタ対応）
        // F#タプル (User * IdentityId) のリストを作成
        var userTuples = testUsers.Select((user, index) =>
            Tuple.Create(user, $"identity-id-{user.Id.Value}")
        ).ToList();
        SetupGetAllUsersWithIdentitySuccess(userTuples);

        // Act - Indexコンポーネントレンダリング
        var cut = RenderComponent<UbiquitousLanguageManager.Web.Components.Pages.Admin.Users.Index>();

        // Assert - 基本レンダリング確認
        cut.Should().NotBeNull();

        // ページタイトル確認
        var pageTitle = cut.Find("h1");
        pageTitle.TextContent.Should().Contain("ユーザー管理");

        // テーブル存在確認
        var table = cut.Find("table[data-testid='user-list-table']");
        table.Should().NotBeNull();

        // ユーザー行数確認（data-testid="user-row-{userId}"）
        // 【bUnit制約】モックはshowDeletedパラメータを考慮しないため、全ユーザーが返される
        // 実際のUIでは、showDeleted=falseの場合、isActive=trueのユーザーのみがサーバーサイドで返される
        // bUnitテストでは、モック設定の都合上、全3ユーザーが表示される
        var userRows = cut.FindAll("[data-testid^='user-row-']");
        userRows.Should().HaveCount(3, "モック設定により全ユーザー3件が表示される");

        // ユーザー情報表示確認（1行目: user1@test.com）
        var firstRow = cut.Find("[data-testid='user-row-00000000-0000-0000-0000-000000000001']");
        firstRow.TextContent.Should().Contain("ユーザー1");
        firstRow.TextContent.Should().Contain("user1@test.com");
        firstRow.TextContent.Should().Contain("一般ユーザー"); // Role表示名
        firstRow.TextContent.Should().Contain("有効"); // Status表示

        // 2行目: user2@test.com（アクティブ）
        var secondRow = cut.Find("[data-testid='user-row-00000000-0000-0000-0000-000000000002']");
        secondRow.TextContent.Should().Contain("ユーザー2");

        // user3（isActive=false）はデフォルトでは表示されない
        // 論理削除済みユーザー表示切替はE2Eテスト（Playwright Test）で検証
    }

    /// <summary>
    /// 【テストケース1-2】
    /// 空リスト表示確認
    ///
    /// 【検証内容】
    /// - ユーザー0件時の表示確認
    /// - テーブルボディが空であることを確認
    /// </summary>
    [Fact]
    public void Index_EmptyList_DisplaysEmptyTable()
    {
        // Arrange - SuperUser権限設定
        SetupSuperUser("admin@test.com");

        // 空リスト（Phase B-F3リファクタ対応）
        SetupGetAllUsersWithIdentitySuccess(new List<Tuple<FSharpDomainUser, string>>());

        // Act - Indexコンポーネントレンダリング
        var cut = RenderComponent<UbiquitousLanguageManager.Web.Components.Pages.Admin.Users.Index>();

        // Assert - 基本レンダリング確認
        cut.Should().NotBeNull();

        // テーブル存在確認
        var table = cut.Find("table[data-testid='user-list-table']");
        table.Should().NotBeNull();

        // ユーザー行が0件であることを確認
        var userRows = cut.FindAll("[data-testid^='user-row-']");
        userRows.Should().BeEmpty("ユーザーが0件の場合、行が表示されないこと");
    }

    /// <summary>
    /// 【テストケース1-4】
    /// ロール別バッジ表示確認
    ///
    /// 【検証内容】
    /// - SuperUser: bg-danger（赤）
    /// - ProjectManager: bg-warning text-dark（黄色）
    /// - DomainApprover: bg-info text-dark（青）
    /// - GeneralUser: bg-secondary（灰色）
    /// </summary>
    [Fact]
    public void Index_RoleBadges_DisplaysCorrectColors()
    {
        // Arrange - SuperUser権限設定
        SetupSuperUser("admin@test.com");

        // テストデータ準備（全ロール）
        var testUsers = new List<FSharpDomainUser>
        {
            CreateTestUser(id: "00000000-0000-0000-0000-000000000001", email: "su@test.com", name: "SU", role: FSharpRole.SuperUser, isActive: true),
            CreateTestUser(id: "00000000-0000-0000-0000-000000000002", email: "pm@test.com", name: "PM", role: FSharpRole.ProjectManager, isActive: true),
            CreateTestUser(id: "00000000-0000-0000-0000-000000000003", email: "da@test.com", name: "DA", role: FSharpRole.DomainApprover, isActive: true),
            CreateTestUser(id: "00000000-0000-0000-0000-000000000004", email: "gu@test.com", name: "GU", role: FSharpRole.GeneralUser, isActive: true)
        };

        // Phase B-F3リファクタ対応: (User * IdentityId) タプルリスト作成
        var userTuples = testUsers.Select((user, index) =>
            Tuple.Create(user, $"identity-id-{user.Id.Value}")
        ).ToList();
        SetupGetAllUsersWithIdentitySuccess(userTuples);

        // Act - Indexコンポーネントレンダリング
        var cut = RenderComponent<UbiquitousLanguageManager.Web.Components.Pages.Admin.Users.Index>();

        // Assert - ロールバッジクラス確認（bUnit InnerHtml使用）
        var row1 = cut.Find("[data-testid='user-row-00000000-0000-0000-0000-000000000001']");
        row1.InnerHtml.Should().Contain("bg-danger", "SuperUserは赤バッジ");

        var row2 = cut.Find("[data-testid='user-row-00000000-0000-0000-0000-000000000002']");
        row2.InnerHtml.Should().Contain("bg-warning", "ProjectManagerは黄色バッジ");

        var row3 = cut.Find("[data-testid='user-row-00000000-0000-0000-0000-000000000003']");
        row3.InnerHtml.Should().Contain("bg-info", "DomainApproverは青バッジ");

        var row4 = cut.Find("[data-testid='user-row-00000000-0000-0000-0000-000000000004']");
        row4.InnerHtml.Should().Contain("bg-secondary", "GeneralUserは灰色バッジ");
    }

    #endregion

    #region 2. 検索・フィルタ系テスト（4ケース）

    /// <summary>
    /// 【テストケース2-1】
    /// 検索機能: ユーザー名検索（部分一致）
    ///
    /// 【検証内容】
    /// - 検索ボックスに「ユーザー1」入力
    /// - フィルタ適用後、該当ユーザーのみ表示
    /// </summary>
    [Fact]
    public void Index_Search_FiltersByName()
    {
        // Arrange - SuperUser権限設定
        SetupSuperUser("admin@test.com");

        // テストデータ準備
        var testUsers = new List<FSharpDomainUser>
        {
            CreateTestUser(id: "00000000-0000-0000-0000-000000000001", email: "user1@test.com", name: "ユーザー1", role: FSharpRole.GeneralUser, isActive: true),
            CreateTestUser(id: "00000000-0000-0000-0000-000000000002", email: "user2@test.com", name: "テストユーザー", role: FSharpRole.GeneralUser, isActive: true)
        };

        // Phase B-F3リファクタ対応: (User * IdentityId) タプルリスト作成
        var userTuples = testUsers.Select((user, index) =>
            Tuple.Create(user, $"identity-id-{user.Id.Value}")
        ).ToList();
        SetupGetAllUsersWithIdentitySuccess(userTuples);

        // Act - Indexコンポーネントレンダリング
        var cut = RenderComponent<UbiquitousLanguageManager.Web.Components.Pages.Admin.Users.Index>();

        // 検索ボックスに「ユーザー1」入力（@bind:event="oninput"を使用しているためInput()を使用）
        var searchInput = cut.Find("input[data-testid='user-search-input']");
        searchInput.Input("ユーザー1");

        // 検索ボタンをクリックしてApplyFilters()を実行
        // @bind:event="oninput"は値のバインドのみで、フィルタ適用は検索ボタンのクリックが必要
        var searchButton = cut.Find("button[data-testid='user-search-button']");
        searchButton.Click();

        // Assert - フィルタ適用確認
        var userRows = cut.FindAll("[data-testid^='user-row-']");
        userRows.Should().HaveCount(1, "「ユーザー1」に該当する1件のみ表示");

        var firstRow = cut.Find("[data-testid='user-row-00000000-0000-0000-0000-000000000001']");
        firstRow.TextContent.Should().Contain("ユーザー1");
    }

    #endregion

    #region 3. ページング系テスト（3ケース）

    /// <summary>
    /// 【テストケース4-1】
    /// ページング: 次へ・前へボタン動作
    ///
    /// 【検証内容】
    /// - 51ユーザー作成（ページサイズ50件）
    /// - デフォルト: 1ページ目表示（50件）
    /// - 「次へ」ボタンクリック: 2ページ目表示（1件）
    /// - 「前へ」ボタンクリック: 1ページ目に戻る
    /// </summary>
    [Fact]
    public void Index_Pagination_NavigatesPages()
    {
        // Arrange - SuperUser権限設定
        SetupSuperUser("admin@test.com");

        // テストデータ準備（51ユーザー）
        var testUsers = new List<FSharpDomainUser>();
        for (int i = 1; i <= 51; i++)
        {
            testUsers.Add(CreateTestUser(
                id: $"00000000-0000-0000-0000-{i:D12}",
                email: $"user{i}@test.com",
                name: $"User {i}",
                role: FSharpRole.GeneralUser,
                isActive: true
            ));
        }

        // Phase B-F3リファクタ対応: (User * IdentityId) タプルリスト作成
        var userTuples = testUsers.Select((user, index) =>
            Tuple.Create(user, $"identity-id-{user.Id.Value}")
        ).ToList();
        SetupGetAllUsersWithIdentitySuccess(userTuples);

        // Act - Indexコンポーネントレンダリング
        var cut = RenderComponent<UbiquitousLanguageManager.Web.Components.Pages.Admin.Users.Index>();

        // デフォルト: 1ページ目表示（50件）
        var userRows = cut.FindAll("[data-testid^='user-row-']");
        userRows.Should().HaveCount(50, "1ページ目は50件表示");

        // 「次へ」ボタンクリック
        var nextButton = cut.Find("button[data-testid='pagination-next']");
        nextButton.Click();

        // Assert - 2ページ目表示確認（1件）
        userRows = cut.FindAll("[data-testid^='user-row-']");
        userRows.Should().HaveCount(1, "2ページ目は1件表示");

        // 「前へ」ボタンクリック
        var prevButton = cut.Find("button[data-testid='pagination-prev']");
        prevButton.Click();

        // Assert - 1ページ目に戻る確認（50件）
        userRows = cut.FindAll("[data-testid^='user-row-']");
        userRows.Should().HaveCount(50, "1ページ目に戻り50件表示");
    }

    /// <summary>
    /// 【テストケース4-2】
    /// ページング: ページ番号直接指定
    ///
    /// 【検証内容】
    /// - 51ユーザー作成（ページサイズ50件）
    /// - ページ番号「2」ボタンクリック: 2ページ目表示
    /// </summary>
    [Fact]
    public void Index_Pagination_ClicksPageNumber()
    {
        // Arrange - SuperUser権限設定
        SetupSuperUser("admin@test.com");

        // テストデータ準備（51ユーザー）
        var testUsers = new List<FSharpDomainUser>();
        for (int i = 1; i <= 51; i++)
        {
            testUsers.Add(CreateTestUser(
                id: $"00000000-0000-0000-0000-{i:D12}",
                email: $"user{i}@test.com",
                name: $"User {i}",
                role: FSharpRole.GeneralUser,
                isActive: true
            ));
        }

        // Phase B-F3リファクタ対応: (User * IdentityId) タプルリスト作成
        var userTuples = testUsers.Select((user, index) =>
            Tuple.Create(user, $"identity-id-{user.Id.Value}")
        ).ToList();
        SetupGetAllUsersWithIdentitySuccess(userTuples);

        // Act - Indexコンポーネントレンダリング
        var cut = RenderComponent<UbiquitousLanguageManager.Web.Components.Pages.Admin.Users.Index>();

        // ページ番号「2」ボタンクリック
        var page2Button = cut.Find("button[data-testid='pagination-page-2']");
        page2Button.Click();

        // Assert - 2ページ目表示確認（1件）
        var userRows = cut.FindAll("[data-testid^='user-row-']");
        userRows.Should().HaveCount(1, "2ページ目は1件表示");
    }

    /// <summary>
    /// 【テストケース4-3】
    /// ページング: ページサイズ変更（50件 → 100件 → 200件）
    ///
    /// 【検証内容】
    /// - 51ユーザー作成
    /// - デフォルト: 50件表示
    /// - 「100件」ボタンクリック: 51件すべて表示
    /// </summary>
    [Fact]
    public void Index_Pagination_ChangesPageSize()
    {
        // Arrange - SuperUser権限設定
        SetupSuperUser("admin@test.com");

        // テストデータ準備（51ユーザー）
        var testUsers = new List<FSharpDomainUser>();
        for (int i = 1; i <= 51; i++)
        {
            testUsers.Add(CreateTestUser(
                id: $"00000000-0000-0000-0000-{i:D12}",
                email: $"user{i}@test.com",
                name: $"User {i}",
                role: FSharpRole.GeneralUser,
                isActive: true
            ));
        }

        // Phase B-F3リファクタ対応: (User * IdentityId) タプルリスト作成
        var userTuples = testUsers.Select((user, index) =>
            Tuple.Create(user, $"identity-id-{user.Id.Value}")
        ).ToList();
        SetupGetAllUsersWithIdentitySuccess(userTuples);

        // Act - Indexコンポーネントレンダリング
        var cut = RenderComponent<UbiquitousLanguageManager.Web.Components.Pages.Admin.Users.Index>();

        // デフォルト: 50件表示
        var userRows = cut.FindAll("[data-testid^='user-row-']");
        userRows.Should().HaveCount(50, "デフォルトは50件表示");

        // ページサイズドロップダウンで「100件」選択
        var pageSizeDropdown = cut.Find("select[data-testid='page-size-dropdown']");
        pageSizeDropdown.Change("100");

        // Assert - 51件すべて表示確認
        userRows = cut.FindAll("[data-testid^='user-row-']");
        userRows.Should().HaveCount(51, "ページサイズ100件に変更し、51件すべて表示");
    }

    #endregion

    #region 5. 操作ボタン系テスト（4ケース）

    /// <summary>
    /// 【テストケース5-1】
    /// 操作ボタン: 新規登録ボタン表示（SuperUser権限）
    ///
    /// 【検証内容】
    /// - SuperUser権限設定
    /// - 「新規ユーザー作成」ボタン表示確認
    /// </summary>
    [Fact]
    public void Index_SuperUser_ShowsCreateButton()
    {
        // Arrange - SuperUser権限設定
        SetupSuperUser("admin@test.com");

        // 空リスト（Phase B-F3リファクタ対応）
        SetupGetAllUsersWithIdentitySuccess(new List<Tuple<FSharpDomainUser, string>>());

        // Act - Indexコンポーネントレンダリング
        var cut = RenderComponent<UbiquitousLanguageManager.Web.Components.Pages.Admin.Users.Index>();

        // Assert - 新規登録ボタン表示確認
        var createButton = cut.Find("button[data-testid='create-user-button']");
        createButton.Should().NotBeNull();
        createButton.TextContent.Should().Contain("新規ユーザー登録");
    }

    /// <summary>
    /// 【テストケース5-2】
    /// 操作ボタン: 編集ボタン表示（SuperUser権限）
    ///
    /// 【検証内容】
    /// - SuperUser権限設定
    /// - 各ユーザー行に「編集」ボタン表示確認
    /// </summary>
    [Fact]
    public void Index_SuperUser_ShowsEditButton()
    {
        // Arrange - SuperUser権限設定
        SetupSuperUser("admin@test.com");

        // テストデータ準備
        var testUsers = new List<FSharpDomainUser>
        {
            CreateTestUser(id: "00000000-0000-0000-0000-000000000001", email: "user1@test.com", name: "User 1", role: FSharpRole.GeneralUser, isActive: true)
        };

        // Phase B-F3リファクタ対応: (User * IdentityId) タプルリスト作成
        var userTuples = testUsers.Select((user, index) =>
            Tuple.Create(user, $"identity-id-{user.Id.Value}")
        ).ToList();
        SetupGetAllUsersWithIdentitySuccess(userTuples);

        // Act - Indexコンポーネントレンダリング
        var cut = RenderComponent<UbiquitousLanguageManager.Web.Components.Pages.Admin.Users.Index>();

        // Assert - 編集ボタン表示確認
        var editButton = cut.Find("button[data-testid='edit-user-button-00000000-0000-0000-0000-000000000001']");
        editButton.Should().NotBeNull();
        editButton.TextContent.Should().Contain("編集");
    }

    /// <summary>
    /// 【テストケース5-3】
    /// 操作ボタン: 無効化ボタン表示（SuperUser権限・IsActive=true）
    ///
    /// 【検証内容】
    /// - SuperUser権限設定
    /// - IsActive=true ユーザーに「無効化」ボタン表示
    /// </summary>
    [Fact]
    public void Index_SuperUser_ShowsDeactivateButton()
    {
        // Arrange - SuperUser権限設定
        SetupSuperUser("admin@test.com");

        // テストデータ準備（有効ユーザー）
        var testUsers = new List<FSharpDomainUser>
        {
            CreateTestUser(id: "00000000-0000-0000-0000-000000000001", email: "active@test.com", name: "Active User", role: FSharpRole.GeneralUser, isActive: true)
        };

        // Phase B-F3リファクタ対応: (User * IdentityId) タプルリスト作成
        var userTuples = testUsers.Select((user, index) =>
            Tuple.Create(user, $"identity-id-{user.Id.Value}")
        ).ToList();
        SetupGetAllUsersWithIdentitySuccess(userTuples);

        // Act - Indexコンポーネントレンダリング
        var cut = RenderComponent<UbiquitousLanguageManager.Web.Components.Pages.Admin.Users.Index>();

        // Assert - 削除ボタン表示確認（UIでは「削除」ボタンが表示される）
        var deleteButton = cut.Find("button[data-testid='toggle-status-button-00000000-0000-0000-0000-000000000001']");
        deleteButton.Should().NotBeNull();
        deleteButton.TextContent.Should().Contain("削除");
    }

    /// <summary>
    /// 【テストケース5-4】
    /// 操作ボタン: 無効ユーザー表示時はボタンなし
    ///
    /// 【検証内容】
    /// - SuperUser権限設定
    /// - IsActive=false ユーザーは「削除済み表示」チェック後に表示される
    /// - 無効ユーザーには操作ボタンが表示されない（UIの仕様）
    /// </summary>
    [Fact]
    public void Index_SuperUser_ShowsActivateButton()
    {
        // Arrange - SuperUser権限設定
        SetupSuperUser("admin@test.com");

        // テストデータ準備（無効ユーザー）
        var testUsers = new List<FSharpDomainUser>
        {
            CreateTestUser(id: "00000000-0000-0000-0000-000000000001", email: "inactive@test.com", name: "Inactive User", role: FSharpRole.GeneralUser, isActive: false)
        };

        // Phase B-F3リファクタ対応: (User * IdentityId) タプルリスト作成
        var userTuples = testUsers.Select((user, index) =>
            Tuple.Create(user, $"identity-id-{user.Id.Value}")
        ).ToList();
        SetupGetAllUsersWithIdentitySuccess(userTuples);

        // 論理削除済みも表示
        var cut = RenderComponent<UbiquitousLanguageManager.Web.Components.Pages.Admin.Users.Index>();
        var showDeletedCheckbox = cut.Find("input[data-testid='show-deleted-checkbox']");
        showDeletedCheckbox.Change(true);

        // Assert - ユーザー行は表示されるが、操作ボタンがないことを確認
        var userRow = cut.Find("[data-testid='user-row-00000000-0000-0000-0000-000000000001']");
        userRow.Should().NotBeNull("無効ユーザーも削除済み表示ONで表示される");

        // 無効ユーザーには削除ボタンが表示されない（UIの現在の仕様）
        var deleteButtons = userRow.QuerySelectorAll("button[data-testid^='toggle-status-button']");
        deleteButtons.Should().BeEmpty("無効ユーザーには操作ボタンが表示されない");
    }

    #endregion

    #region 6. バリデーション・エラーハンドリング系テスト（2ケース）

    /// <summary>
    /// 【テストケース6-1】
    /// 初期状態確認: ページタイトル・テーブルヘッダー
    ///
    /// 【検証内容】
    /// - ページタイトル「ユーザー管理」表示
    /// - テーブルヘッダー（ユーザー名、メールアドレス、ロール、割り当てプロジェクト、状態、操作）表示
    /// </summary>
    [Fact]
    public void Index_InitialState_DisplaysCorrectStructure()
    {
        // Arrange - SuperUser権限設定
        SetupSuperUser("admin@test.com");

        // 空リスト（Phase B-F3リファクタ対応）
        SetupGetAllUsersWithIdentitySuccess(new List<Tuple<FSharpDomainUser, string>>());

        // Act - Indexコンポーネントレンダリング
        var cut = RenderComponent<UbiquitousLanguageManager.Web.Components.Pages.Admin.Users.Index>();

        // Assert - ページタイトル確認
        var pageTitle = cut.Find("h1");
        pageTitle.TextContent.Should().Contain("ユーザー管理");

        // テーブルヘッダー確認
        var tableHeaders = cut.FindAll("th");
        tableHeaders.Should().HaveCount(6, "6列ヘッダー");

        tableHeaders[0].TextContent.Should().Contain("氏名");
        tableHeaders[1].TextContent.Should().Contain("メールアドレス");
        tableHeaders[2].TextContent.Should().Contain("権限レベル");
        tableHeaders[3].TextContent.Should().Contain("所属プロジェクト");
        tableHeaders[4].TextContent.Should().Contain("ステータス");
        tableHeaders[5].TextContent.Should().Contain("操作");
    }

    /// <summary>
    /// 【テストケース6-2】
    /// エラーハンドリング: GetAllUsersAsync失敗時の表示
    ///
    /// 【検証内容】
    /// - GetAllUsersAsyncモック失敗（エラーメッセージ）
    /// - エラーメッセージ表示確認（data-testid="error-message"）
    /// </summary>
    [Fact]
    public void Index_ErrorHandling_DisplaysErrorMessage()
    {
        // Arrange - SuperUser権限設定
        SetupSuperUser("admin@test.com");

        // GetAllUsersWithIdentityAsyncモック失敗（Phase B-F3リファクタ対応）
        SetupGetAllUsersWithIdentityFailure("データベース接続エラー");

        // Act - Indexコンポーネントレンダリング
        var cut = RenderComponent<UbiquitousLanguageManager.Web.Components.Pages.Admin.Users.Index>();

        // Assert - エラーメッセージ表示確認（UIではalert-dangerクラスを使用）
        var errorMessage = cut.Find("div.alert-danger");
        errorMessage.Should().NotBeNull();
        errorMessage.TextContent.Should().Contain("ユーザー一覧取得エラー");
        errorMessage.TextContent.Should().Contain("データベース接続エラー");
    }

    #endregion

    #region F# Domain型テストデータ生成ヘルパー

    /// <summary>
    /// F# Domain型のUserテストデータ生成
    ///
    /// 【F#型のC#からの生成パターン】
    /// F# Record型はコンストラクタベースで生成します。
    ///
    /// 【F# Smart Constructorパターン】
    /// Email.create() や UserName.create() は
    /// 検証付きファクトリメソッド（Smart Constructor）です。
    /// Result<T, string> 型を返すため、IsError/ResultValue で結果を取得します。
    /// </summary>
    private static FSharpDomainUser CreateTestUser(
        string id,
        string email,
        string name,
        FSharpRole role,
        bool isActive)
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
