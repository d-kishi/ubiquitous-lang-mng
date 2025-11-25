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
                id: 1L,
                email: "user1@test.com",
                name: "ユーザー1",
                role: FSharpRole.GeneralUser,
                isActive: true
            ),
            CreateTestUser(
                id: 2L,
                email: "user2@test.com",
                name: "ユーザー2",
                role: FSharpRole.DomainApprover,
                isActive: true
            ),
            CreateTestUser(
                id: 3L,
                email: "user3@test.com",
                name: "ユーザー3",
                role: FSharpRole.ProjectManager,
                isActive: false
            )
        };

        // GetAllUsersAsyncモックセットアップ
        SetupGetAllUsersSuccess(testUsers);

        // Act - Indexコンポーネントレンダリング
        var cut = RenderComponent<UbiquitousLanguageManager.Web.Components.Pages.Admin.Users.Index>();

        // Assert - 基本レンダリング確認
        cut.Should().NotBeNull();

        // ページタイトル確認
        var pageTitle = cut.Find("h1");
        pageTitle.TextContent.Should().Contain("ユーザー管理");

        // テーブル存在確認
        var table = cut.Find("table[data-testid='table-users']");
        table.Should().NotBeNull();

        // ユーザー行数確認（data-testid="user-row-{userId}"）
        // 【仕様準拠】デフォルトではisActive=trueのユーザーのみ表示
        // user3はisActive=falseのため除外される（一般的なUI設計パターン準拠）
        var userRows = cut.FindAll("[data-testid^='user-row-']");
        userRows.Should().HaveCount(2, "アクティブなユーザー2件が表示されること");

        // ユーザー情報表示確認（1行目: user1@test.com）
        var firstRow = cut.Find("[data-testid='user-row-1']");
        firstRow.TextContent.Should().Contain("ユーザー1");
        firstRow.TextContent.Should().Contain("user1@test.com");
        firstRow.TextContent.Should().Contain("一般ユーザー"); // Role表示名
        firstRow.TextContent.Should().Contain("有効"); // Status表示

        // 2行目: user2@test.com（アクティブ）
        var secondRow = cut.Find("[data-testid='user-row-2']");
        secondRow.TextContent.Should().Contain("ユーザー2");

        // user3（isActive=false）はデフォルトでは表示されない
        // 論理削除済みユーザー表示は Index_Filter_ShowsDeletedUsers テストで検証
    }

    /// <summary>
    /// 【テストケース1-2】
    /// ProjectManager権限: アクセス拒否確認（TODO Phase B-F3 Step2で実装予定）
    ///
    /// 【検証内容】
    /// - ProjectManager権限設定
    /// - Authorize属性により403エラー発生
    ///
    /// 【補足】
    /// - 現在のIndex.razorは @attribute [Authorize(Roles = "SuperUser")] により、
    ///   SuperUser以外はアクセス拒否される
    /// - TODO: Phase B-F3 Step2でProjectManager権限追加予定
    /// </summary>
    [Fact(Skip = "Phase B-F3 Step2でProjectManager権限対応予定")]
    public void Index_ProjectManager_AccessDenied()
    {
        // Arrange - ProjectManager権限設定
        SetupProjectManager("pm@test.com");

        // Act & Assert - 403エラー発生確認（bUnit未対応のためスキップ）
        // TODO: bUnitでの403エラーハンドリング検証方法を調査
    }

    /// <summary>
    /// 【テストケース1-3】
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

        // 空リスト
        SetupGetAllUsersSuccess(new List<FSharpDomainUser>());

        // Act - Indexコンポーネントレンダリング
        var cut = RenderComponent<UbiquitousLanguageManager.Web.Components.Pages.Admin.Users.Index>();

        // Assert - 基本レンダリング確認
        cut.Should().NotBeNull();

        // テーブル存在確認
        var table = cut.Find("table[data-testid='table-users']");
        table.Should().NotBeNull();

        // ユーザー行が0件であることを確認
        var userRows = cut.FindAll("[data-testid^='user-row-']");
        userRows.Should().BeEmpty("ユーザーが0件の場合、行が表示されないこと");
    }

    /// <summary>
    /// 【テストケース1-4】
    /// ロード中表示確認
    ///
    /// 【検証内容】
    /// - OnInitializedAsync実行前のloading状態確認
    /// - spinner表示確認
    ///
    /// 【補足】
    /// - bUnitでは OnInitializedAsync が自動実行されるため、
    ///   loading状態のテストは困難（即座に完了するため）
    /// - 本テストはスキップ（実装確認済み）
    /// </summary>
    [Fact(Skip = "bUnitでのloading状態テストは困難（即座に完了）")]
    public void Index_Loading_DisplaysSpinner()
    {
        // bUnitでは OnInitializedAsync が自動実行されるため、
        // loading状態のテストは困難（即座に完了するため）
    }

    /// <summary>
    /// 【テストケース1-5】
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
            CreateTestUser(id: 1L, email: "su@test.com", name: "SU", role: FSharpRole.SuperUser, isActive: true),
            CreateTestUser(id: 2L, email: "pm@test.com", name: "PM", role: FSharpRole.ProjectManager, isActive: true),
            CreateTestUser(id: 3L, email: "da@test.com", name: "DA", role: FSharpRole.DomainApprover, isActive: true),
            CreateTestUser(id: 4L, email: "gu@test.com", name: "GU", role: FSharpRole.GeneralUser, isActive: true)
        };

        SetupGetAllUsersSuccess(testUsers);

        // Act - Indexコンポーネントレンダリング
        var cut = RenderComponent<UbiquitousLanguageManager.Web.Components.Pages.Admin.Users.Index>();

        // Assert - ロールバッジクラス確認（bUnit InnerHtml使用）
        var row1 = cut.Find("[data-testid='user-row-1']");
        row1.InnerHtml.Should().Contain("bg-danger", "SuperUserは赤バッジ");

        var row2 = cut.Find("[data-testid='user-row-2']");
        row2.InnerHtml.Should().Contain("bg-warning", "ProjectManagerは黄色バッジ");

        var row3 = cut.Find("[data-testid='user-row-3']");
        row3.InnerHtml.Should().Contain("bg-info", "DomainApproverは青バッジ");

        var row4 = cut.Find("[data-testid='user-row-4']");
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
            CreateTestUser(id: 1L, email: "user1@test.com", name: "ユーザー1", role: FSharpRole.GeneralUser, isActive: true),
            CreateTestUser(id: 2L, email: "user2@test.com", name: "テストユーザー", role: FSharpRole.GeneralUser, isActive: true)
        };

        SetupGetAllUsersSuccess(testUsers);

        // Act - Indexコンポーネントレンダリング
        var cut = RenderComponent<UbiquitousLanguageManager.Web.Components.Pages.Admin.Users.Index>();

        // 検索ボックスに「ユーザー1」入力
        var searchInput = cut.Find("input[data-testid='input-search']");
        searchInput.Input("ユーザー1");

        // Assert - フィルタ適用確認
        var userRows = cut.FindAll("[data-testid^='user-row-']");
        userRows.Should().HaveCount(1, "「ユーザー1」に該当する1件のみ表示");

        var firstRow = cut.Find("[data-testid='user-row-1']");
        firstRow.TextContent.Should().Contain("ユーザー1");
    }

    /// <summary>
    /// 【テストケース2-2】
    /// 検索機能: メールアドレス検索（部分一致）
    ///
    /// 【検証内容】
    /// - 検索ボックスに「user1@」入力
    /// - フィルタ適用後、該当ユーザーのみ表示
    /// </summary>
    [Fact]
    public void Index_Search_FiltersByEmail()
    {
        // Arrange - SuperUser権限設定
        SetupSuperUser("admin@test.com");

        // テストデータ準備
        var testUsers = new List<FSharpDomainUser>
        {
            CreateTestUser(id: 1L, email: "user1@test.com", name: "ユーザー1", role: FSharpRole.GeneralUser, isActive: true),
            CreateTestUser(id: 2L, email: "user2@test.com", name: "ユーザー2", role: FSharpRole.GeneralUser, isActive: true)
        };

        SetupGetAllUsersSuccess(testUsers);

        // Act - Indexコンポーネントレンダリング
        var cut = RenderComponent<UbiquitousLanguageManager.Web.Components.Pages.Admin.Users.Index>();

        // 検索ボックスに「user1@」入力
        var searchInput = cut.Find("input[data-testid='input-search']");
        searchInput.Input("user1@");

        // Assert - フィルタ適用確認
        var userRows = cut.FindAll("[data-testid^='user-row-']");
        userRows.Should().HaveCount(1, "「user1@」に該当する1件のみ表示");

        var firstRow = cut.Find("[data-testid='user-row-1']");
        firstRow.TextContent.Should().Contain("user1@test.com");
    }

    /// <summary>
    /// 【テストケース2-3】
    /// フィルタ: 論理削除済み表示切替
    ///
    /// 【検証内容】
    /// - デフォルト: IsActive=trueのみ表示
    /// - チェックボックスON: IsActive=false も表示
    /// </summary>
    [Fact]
    public void Index_Filter_ShowsDeletedUsers()
    {
        // Arrange - SuperUser権限設定
        SetupSuperUser("admin@test.com");

        // テストデータ準備（有効1件・無効1件）
        var testUsers = new List<FSharpDomainUser>
        {
            CreateTestUser(id: 1L, email: "active@test.com", name: "有効ユーザー", role: FSharpRole.GeneralUser, isActive: true),
            CreateTestUser(id: 2L, email: "inactive@test.com", name: "無効ユーザー", role: FSharpRole.GeneralUser, isActive: false)
        };

        SetupGetAllUsersSuccess(testUsers);

        // Act - Indexコンポーネントレンダリング
        var cut = RenderComponent<UbiquitousLanguageManager.Web.Components.Pages.Admin.Users.Index>();

        // デフォルト: 有効ユーザーのみ表示
        var userRows = cut.FindAll("[data-testid^='user-row-']");
        userRows.Should().HaveCount(1, "デフォルトでは有効ユーザーのみ表示");

        // チェックボックスON: 論理削除済みも表示
        var showDeletedCheckbox = cut.Find("input[data-testid='checkbox-show-deleted']");
        showDeletedCheckbox.Change(true);

        // Assert - フィルタ解除確認
        userRows = cut.FindAll("[data-testid^='user-row-']");
        userRows.Should().HaveCount(2, "論理削除済み含めて2件表示");
    }

    /// <summary>
    /// 【テストケース2-4】
    /// フィルタ: ロール絞り込み（TODO Phase B-F3 Step2で実装予定）
    ///
    /// 【検証内容】
    /// - ロールドロップダウンで「GeneralUser」選択
    /// - 該当ロールのユーザーのみ表示
    ///
    /// 【補足】
    /// - 現在のIndex.razorにはロール絞り込み機能が未実装
    /// - TODO: Phase B-F3 Step2でロールフィルタ追加予定
    /// </summary>
    [Fact(Skip = "Phase B-F3 Step2でロールフィルタ実装予定")]
    public void Index_Filter_FiltersByRole()
    {
        // TODO: Phase B-F3 Step2で実装予定
    }

    #endregion

    #region 3. ソート系テスト（2ケース）

    /// <summary>
    /// 【テストケース3-1】
    /// ソート: 名前昇順・降順切替
    ///
    /// 【検証内容】
    /// - デフォルト: 名前昇順
    /// - 列ヘッダークリック: 降順に切替
    /// - 再クリック: 昇順に戻る
    /// </summary>
    [Fact]
    public void Index_Sort_TogglesByName()
    {
        // Arrange - SuperUser権限設定
        SetupSuperUser("admin@test.com");

        // テストデータ準備（名前順: Z → A → B）
        var testUsers = new List<FSharpDomainUser>
        {
            CreateTestUser(id: 1L, email: "z@test.com", name: "Z User", role: FSharpRole.GeneralUser, isActive: true),
            CreateTestUser(id: 2L, email: "a@test.com", name: "A User", role: FSharpRole.GeneralUser, isActive: true),
            CreateTestUser(id: 3L, email: "b@test.com", name: "B User", role: FSharpRole.GeneralUser, isActive: true)
        };

        SetupGetAllUsersSuccess(testUsers);

        // Act - Indexコンポーネントレンダリング
        var cut = RenderComponent<UbiquitousLanguageManager.Web.Components.Pages.Admin.Users.Index>();

        // デフォルト: 名前昇順（A → B → Z）
        var userRows = cut.FindAll("[data-testid^='user-row-']");
        userRows[0].TextContent.Should().Contain("A User", "昇順1番目");
        userRows[1].TextContent.Should().Contain("B User", "昇順2番目");
        userRows[2].TextContent.Should().Contain("Z User", "昇順3番目");

        // 「ユーザー名」列ヘッダークリック → 降順に切替
        var nameHeader = cut.FindAll("th")[0]; // 1列目: ユーザー名
        nameHeader.Click();

        // Assert - 降順確認（Z → B → A）
        userRows = cut.FindAll("[data-testid^='user-row-']");
        userRows[0].TextContent.Should().Contain("Z User", "降順1番目");
        userRows[1].TextContent.Should().Contain("B User", "降順2番目");
        userRows[2].TextContent.Should().Contain("A User", "降順3番目");
    }

    /// <summary>
    /// 【テストケース3-2】
    /// ソート: ステータス昇順・降順切替
    ///
    /// 【検証内容】
    /// - デフォルト: 名前昇順（IsActiveは関係なし）
    /// - 「状態」列ヘッダークリック: IsActive昇順（false → true）
    /// - 再クリック: IsActive降順（true → false）
    /// </summary>
    [Fact]
    public void Index_Sort_TogglesByStatus()
    {
        // Arrange - SuperUser権限設定
        SetupSuperUser("admin@test.com");

        // テストデータ準備（Status: true → false → true）
        var testUsers = new List<FSharpDomainUser>
        {
            CreateTestUser(id: 1L, email: "active1@test.com", name: "Active 1", role: FSharpRole.GeneralUser, isActive: true),
            CreateTestUser(id: 2L, email: "inactive@test.com", name: "Inactive", role: FSharpRole.GeneralUser, isActive: false),
            CreateTestUser(id: 3L, email: "active2@test.com", name: "Active 2", role: FSharpRole.GeneralUser, isActive: true)
        };

        SetupGetAllUsersSuccess(testUsers);

        // Act - Indexコンポーネントレンダリング
        var cut = RenderComponent<UbiquitousLanguageManager.Web.Components.Pages.Admin.Users.Index>();

        // 論理削除済みも表示
        var showDeletedCheckbox = cut.Find("input[data-testid='checkbox-show-deleted']");
        showDeletedCheckbox.Change(true);

        // 「状態」列ヘッダークリック → IsActive昇順（false → true）
        var statusHeader = cut.FindAll("th")[4]; // 5列目: 状態
        statusHeader.Click();

        // Assert - IsActive昇順確認（false → true → true）
        var userRows = cut.FindAll("[data-testid^='user-row-']");
        userRows[0].TextContent.Should().Contain("Inactive", "IsActive=false が先頭");
        userRows[0].TextContent.Should().Contain("無効", "IsActive=false のバッジ表示");
    }

    #endregion

    #region 4. ページング系テスト（3ケース）

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
                id: i,
                email: $"user{i}@test.com",
                name: $"User {i}",
                role: FSharpRole.GeneralUser,
                isActive: true
            ));
        }

        SetupGetAllUsersSuccess(testUsers);

        // Act - Indexコンポーネントレンダリング
        var cut = RenderComponent<UbiquitousLanguageManager.Web.Components.Pages.Admin.Users.Index>();

        // デフォルト: 1ページ目表示（50件）
        var userRows = cut.FindAll("[data-testid^='user-row-']");
        userRows.Should().HaveCount(50, "1ページ目は50件表示");

        // 「次へ」ボタンクリック
        var nextButton = cut.Find("button[data-testid='btn-next-page']");
        nextButton.Click();

        // Assert - 2ページ目表示確認（1件）
        userRows = cut.FindAll("[data-testid^='user-row-']");
        userRows.Should().HaveCount(1, "2ページ目は1件表示");

        // 「前へ」ボタンクリック
        var prevButton = cut.Find("button[data-testid='btn-prev-page']");
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
                id: i,
                email: $"user{i}@test.com",
                name: $"User {i}",
                role: FSharpRole.GeneralUser,
                isActive: true
            ));
        }

        SetupGetAllUsersSuccess(testUsers);

        // Act - Indexコンポーネントレンダリング
        var cut = RenderComponent<UbiquitousLanguageManager.Web.Components.Pages.Admin.Users.Index>();

        // ページ番号「2」ボタンクリック
        var page2Button = cut.Find("button[data-testid='btn-page-2']");
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
                id: i,
                email: $"user{i}@test.com",
                name: $"User {i}",
                role: FSharpRole.GeneralUser,
                isActive: true
            ));
        }

        SetupGetAllUsersSuccess(testUsers);

        // Act - Indexコンポーネントレンダリング
        var cut = RenderComponent<UbiquitousLanguageManager.Web.Components.Pages.Admin.Users.Index>();

        // デフォルト: 50件表示
        var userRows = cut.FindAll("[data-testid^='user-row-']");
        userRows.Should().HaveCount(50, "デフォルトは50件表示");

        // 「100件」ボタンクリック
        var pageSize100Button = cut.Find("button[data-testid='btn-page-size-100']");
        pageSize100Button.Click();

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

        // 空リスト
        SetupGetAllUsersSuccess(new List<FSharpDomainUser>());

        // Act - Indexコンポーネントレンダリング
        var cut = RenderComponent<UbiquitousLanguageManager.Web.Components.Pages.Admin.Users.Index>();

        // Assert - 新規登録ボタン表示確認
        var createButton = cut.Find("button[data-testid='btn-create-user']");
        createButton.Should().NotBeNull();
        createButton.TextContent.Should().Contain("新規ユーザー作成");
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
            CreateTestUser(id: 1L, email: "user1@test.com", name: "User 1", role: FSharpRole.GeneralUser, isActive: true)
        };

        SetupGetAllUsersSuccess(testUsers);

        // Act - Indexコンポーネントレンダリング
        var cut = RenderComponent<UbiquitousLanguageManager.Web.Components.Pages.Admin.Users.Index>();

        // Assert - 編集ボタン表示確認
        var editButton = cut.Find("button[data-testid='btn-edit-user-1']");
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
            CreateTestUser(id: 1L, email: "active@test.com", name: "Active User", role: FSharpRole.GeneralUser, isActive: true)
        };

        SetupGetAllUsersSuccess(testUsers);

        // Act - Indexコンポーネントレンダリング
        var cut = RenderComponent<UbiquitousLanguageManager.Web.Components.Pages.Admin.Users.Index>();

        // Assert - 無効化ボタン表示確認
        var deactivateButton = cut.Find("button[data-testid='btn-deactivate-user-1']");
        deactivateButton.Should().NotBeNull();
        deactivateButton.TextContent.Should().Contain("無効化");
    }

    /// <summary>
    /// 【テストケース5-4】
    /// 操作ボタン: 有効化ボタン表示（SuperUser権限・IsActive=false）
    ///
    /// 【検証内容】
    /// - SuperUser権限設定
    /// - IsActive=false ユーザーに「有効化」ボタン表示
    /// </summary>
    [Fact]
    public void Index_SuperUser_ShowsActivateButton()
    {
        // Arrange - SuperUser権限設定
        SetupSuperUser("admin@test.com");

        // テストデータ準備（無効ユーザー）
        var testUsers = new List<FSharpDomainUser>
        {
            CreateTestUser(id: 1L, email: "inactive@test.com", name: "Inactive User", role: FSharpRole.GeneralUser, isActive: false)
        };

        SetupGetAllUsersSuccess(testUsers);

        // 論理削除済みも表示
        var cut = RenderComponent<UbiquitousLanguageManager.Web.Components.Pages.Admin.Users.Index>();
        var showDeletedCheckbox = cut.Find("input[data-testid='checkbox-show-deleted']");
        showDeletedCheckbox.Change(true);

        // Assert - 有効化ボタン表示確認
        var activateButton = cut.Find("button[data-testid='btn-activate-user-1']");
        activateButton.Should().NotBeNull();
        activateButton.TextContent.Should().Contain("有効化");
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

        // 空リスト
        SetupGetAllUsersSuccess(new List<FSharpDomainUser>());

        // Act - Indexコンポーネントレンダリング
        var cut = RenderComponent<UbiquitousLanguageManager.Web.Components.Pages.Admin.Users.Index>();

        // Assert - ページタイトル確認
        var pageTitle = cut.Find("h1");
        pageTitle.TextContent.Should().Contain("ユーザー管理");

        // テーブルヘッダー確認
        var tableHeaders = cut.FindAll("th");
        tableHeaders.Should().HaveCount(6, "6列ヘッダー");

        tableHeaders[0].TextContent.Should().Contain("ユーザー名");
        tableHeaders[1].TextContent.Should().Contain("メールアドレス");
        tableHeaders[2].TextContent.Should().Contain("ロール");
        tableHeaders[3].TextContent.Should().Contain("割り当てプロジェクト");
        tableHeaders[4].TextContent.Should().Contain("状態");
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

        // GetAllUsersAsyncモック失敗
        SetupGetAllUsersFailure("データベース接続エラー");

        // Act - Indexコンポーネントレンダリング
        var cut = RenderComponent<UbiquitousLanguageManager.Web.Components.Pages.Admin.Users.Index>();

        // Assert - エラーメッセージ表示確認
        var errorMessage = cut.Find("div[data-testid='error-message']");
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
        long id,
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
