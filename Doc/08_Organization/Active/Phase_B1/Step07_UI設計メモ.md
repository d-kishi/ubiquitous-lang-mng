# Step07 UI実装設計メモ

**作成日**: 2025-10-04
**対象**: Phase B1 Step7 Web層実装
**Stage**: Stage1完了成果物

## 📋 UI設計詳細分析結果

### プロジェクト一覧画面（ProjectList.razor）

#### UI要素詳細
**検索フィルタ**:
- プロジェクト名: 部分一致検索
- 削除済み表示: チェックボックス切替

**データテーブル**:
| 列名 | 内容 | 備考 |
|------|------|------|
| プロジェクト名 | Project.Name | ソート可能 |
| 作成日 | Project.CreatedAt | ソート可能 |
| プロジェクト管理者 | 代表者表示 | 複数の場合は1名のみ |
| ドメイン数 | Domain数カウント | - |
| ユーザー数 | 所属ユーザー数 | - |
| 操作 | 編集・削除ボタン | 権限別表示制御 |

**ページング**:
- 件数選択: 50/100/200件
- ページ番号表示: [◀ 前] 1 / 2 [次 ▶]

**権限別表示制御**:
- **SuperUser**: 全プロジェクト表示・削除ボタン表示
- **ProjectManager**: 担当プロジェクトのみ表示・削除ボタン非表示
- **DomainApprover/GeneralUser**: 画面アクセス不可（Authorize属性）

### プロジェクト登録画面（ProjectCreate.razor）

#### 入力項目
**プロジェクト名**:
- 必須入力
- 一意性チェック（サーバーサイド）
- 空文字・空白文字のみ禁止
- 最大文字数: 100文字

**説明**:
- 任意入力
- 複数行テキスト（textarea）
- 最大文字数: 1000文字

#### バリデーション実装
```csharp
// クライアントサイド（即座フィードバック）
[Required(ErrorMessage = "プロジェクト名は必須です")]
[StringLength(100, ErrorMessage = "プロジェクト名は100文字以内で入力してください")]

// サーバーサイド（Application層）
ProjectDomainService.createProjectWithDefaultDomain
→ 一意性チェック・空文字チェック実施
```

#### デフォルトドメイン自動作成通知
- 成功時Toast: "プロジェクトとデフォルトドメイン「共通」を作成しました"
- 失敗時Toast: Railway-oriented Programming Error詳細表示

**権限制御**:
- SuperUserのみアクセス可能（`@attribute [Authorize(Roles = "SuperUser")]`）

### プロジェクト編集画面（ProjectEdit.razor）

#### UI構成
**プロジェクト名**:
- readonly表示（変更禁止明示）
- `<input readonly disabled>` 属性設定
- グレーアウト表示

**説明**:
- 編集可能
- 複数行テキスト
- 既存値初期表示

**ステータス**:
- ラジオボタン: ● アクティブ ○ 非アクティブ
- `Project.IsActive` フラグ制御

#### 権限別編集制御
- **SuperUser**: 全プロジェクト編集可能
- **ProjectManager**: 担当プロジェクトのみ編集可能
- サーバーサイド権限検証（二重チェック）

## 🎯 Blazor Server実装パターン

### 既存UserManagement.razorパターン活用

#### @codeセクション標準構成
```csharp
@code {
    // 1. 状態管理変数
    private List<ProjectDto> projects = new();
    private bool isLoading = false;

    // 2. 初期化処理
    protected override async Task OnInitializedAsync() {
        await LoadProjects();
    }

    // 3. データ読み込み
    private async Task LoadProjects() {
        isLoading = true;
        StateHasChanged(); // ローディング表示更新

        var query = new GetProjectsQuery(currentUser.Id, currentUser.Role, pageNumber, pageSize);
        var result = await ProjectManagementService.GetProjectsByUserAsync(query);

        if (result.IsOk) {
            projects = result.ResultValue;
        } else {
            await ShowToast("error", result.ErrorValue);
        }

        isLoading = false;
        StateHasChanged(); // データ表示更新
    }

    // 4. CRUD操作
    private async Task CreateProject() { /* ... */ }
    private async Task UpdateProject() { /* ... */ }
    private async Task DeleteProject() { /* ... */ }
}
```

### 既存コンポーネント活用方針

#### SecureButton（権限付きボタン）
```razor
<SecureButton Text="新規プロジェクト作成"
             IconClass="fas fa-plus"
             CssClass="btn btn-primary"
             RequiredRoles='new List<string> { "SuperUser" }'
             RequiredPermission="CreateProject"
             OnClick="ShowCreateProjectModal"
             FallbackText="作成権限なし" />
```

#### PermissionGuard（権限ベース表示制御）
```razor
<PermissionGuard RequiredRoles='new List<string> { "SuperUser", "ProjectManager" }'>
    <AuthorizedContent>
        <!-- 権限あり時の表示 -->
    </AuthorizedContent>
    <NotAuthorizedContent>
        <p>この機能を利用する権限がありません</p>
    </NotAuthorizedContent>
</PermissionGuard>
```

#### ConfirmationDialog（削除確認）
```razor
<ConfirmationDialog @ref="deleteDialog"
                   Title="プロジェクト削除確認"
                   Message="@deleteConfirmMessage"
                   OnConfirm="ExecuteDelete"
                   ConfirmButtonText="削除"
                   CancelButtonText="キャンセル"
                   ConfirmButtonClass="btn-danger" />

@code {
    private string deleteConfirmMessage =>
        $"プロジェクト「{selectedProject?.Name}」を削除しますか？\n" +
        $"関連ドメイン: {domainCount}個\n" +
        $"関連ユビキタス言語: {termCount}個";
}
```

#### LoadingOverlay（ローディング表示）
```razor
<LoadingOverlay IsVisible="@isLoading"
               LoadingText="プロジェクト情報を読み込んでいます..."
               SpinnerType="LoadingOverlay.SpinnerStyle.Border"
               Size="LoadingOverlay.SpinnerSize.Medium" />
```

#### ToastNotification（操作結果通知）
```csharp
private async Task ShowToast(string type, string message) {
    await JSRuntime.InvokeVoidAsync("showToast", type, message);
}
```

### Application層統合設計

#### IProjectManagementService活用
**既存実装済みメソッド**（Phase B1 Step3完了）:
```fsharp
// F# Application層
type IProjectManagementService =
    abstract member CreateProjectAsync: CreateProjectCommand -> Async<Result<ProjectDto, string>>
    abstract member GetProjectsByUserAsync: GetProjectsQuery -> Async<Result<ProjectDto list, string>>
    abstract member UpdateProjectAsync: UpdateProjectCommand -> Async<Result<ProjectDto, string>>
    abstract member DeleteProjectAsync: DeleteProjectCommand -> Async<Result<unit, string>>
```

#### Railway-oriented Programming Result型処理
```csharp
// C# Blazor Serverでの処理パターン
var result = await ProjectManagementService.CreateProjectAsync(command);

if (FSharpResult<ProjectDto, string>.get_IsOk(result)) {
    var project = result.ResultValue;
    await ShowToast("success", "プロジェクトを作成しました");
    NavigationManager.NavigateTo("/projects");
} else {
    var error = result.ErrorValue;
    await ShowToast("error", error);
}
```

#### 権限制御統合実装
```csharp
// AuthenticationStateProvider活用
[Inject] private AuthenticationStateProvider AuthStateProvider { get; set; }

private async Task<UserRole> GetCurrentUserRole() {
    var authState = await AuthStateProvider.GetAuthenticationStateAsync();
    var roleClaim = authState.User.FindFirst(ClaimTypes.Role);
    return Enum.Parse<UserRole>(roleClaim.Value);
}

// Application層Query作成
var query = new GetProjectsQuery(
    UserId: currentUser.Id,
    UserRole: currentUserRole,
    PageNumber: pageNumber,
    PageSize: pageSize
);
```

## 🏗️ コンポーネント構成計画

### ディレクトリ構成
```
src/UbiquitousLanguageManager.Web/Components/Pages/ProjectManagement/
├── ProjectList.razor                    # 一覧画面（メイン）
├── ProjectCreate.razor                  # 登録画面
├── ProjectEdit.razor                    # 編集画面
└── Components/
    ├── ProjectDeleteDialog.razor        # 削除確認ダイアログ
    └── ProjectSearchFilter.razor        # 検索フィルタコンポーネント
```

### ProjectList.razor 詳細設計

#### 責務
- プロジェクト一覧表示（権限フィルタリング適用済みデータ）
- 検索・ページング制御
- Create/Edit画面への遷移
- 削除確認ダイアログ呼び出し

#### 主要メソッド
```csharp
@code {
    // 状態管理
    private List<ProjectDto> projects = new();
    private List<ProjectDto> filteredProjects = new();
    private string searchTerm = "";
    private bool showDeleted = false;
    private int currentPage = 1;
    private int pageSize = 50;
    private bool isLoading = false;

    // 初期化
    protected override async Task OnInitializedAsync() {
        await LoadProjects();
    }

    // データ読み込み
    private async Task LoadProjects() {
        isLoading = true;
        var query = new GetProjectsQuery(currentUser.Id, currentUserRole, currentPage, pageSize);
        var result = await ProjectManagementService.GetProjectsByUserAsync(query);
        // Result型処理
        isLoading = false;
        StateHasChanged();
    }

    // 検索処理
    private void SearchProjects() {
        filteredProjects = projects
            .Where(p => string.IsNullOrEmpty(searchTerm) ||
                       p.Name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
            .Where(p => showDeleted || p.IsActive)
            .ToList();
        StateHasChanged();
    }

    // ページング処理
    private void NextPage() {
        currentPage++;
        await LoadProjects();
    }

    // 削除確認
    private async Task ShowDeleteConfirmation(ProjectDto project) {
        selectedProject = project;
        // 関連データ数取得
        domainCount = await GetRelatedDomainCount(project.Id);
        await deleteDialog.Show();
    }

    // 削除実行
    private async Task ExecuteDelete() {
        var command = new DeleteProjectCommand(selectedProject.Id, currentUser.Id);
        var result = await ProjectManagementService.DeleteProjectAsync(command);
        // Result型処理・UI更新
    }
}
```

### ProjectCreate.razor 詳細設計

#### 責務
- 新規プロジェクト登録フォーム
- バリデーション（一意性・必須チェック）
- デフォルトドメイン自動作成通知

#### フォームバリデーション
```razor
<EditForm Model="@model" OnValidSubmit="CreateProject">
    <DataAnnotationsValidator />
    <ValidationSummary />

    <div class="mb-3">
        <label class="form-label">プロジェクト名（必須）</label>
        <InputText @bind-Value="model.Name" class="form-control" />
        <ValidationMessage For="@(() => model.Name)" />
    </div>

    <div class="mb-3">
        <label class="form-label">説明</label>
        <InputTextArea @bind-Value="model.Description" class="form-control" rows="5" />
    </div>

    <button type="submit" class="btn btn-primary">
        <i class="fas fa-check me-2"></i>登録
    </button>
</EditForm>

@code {
    private CreateProjectModel model = new();

    private async Task CreateProject() {
        var command = new CreateProjectCommand(
            model.Name,
            model.Description,
            currentUser.Id
        );

        var result = await ProjectManagementService.CreateProjectAsync(command);

        if (FSharpResult<ProjectDto, string>.get_IsOk(result)) {
            await ShowToast("success", "プロジェクトとデフォルトドメイン「共通」を作成しました");
            NavigationManager.NavigateTo("/projects");
        } else {
            await ShowToast("error", result.ErrorValue);
        }
    }
}
```

### ProjectEdit.razor 詳細設計

#### 責務
- プロジェクト編集フォーム（説明のみ編集可能）
- プロジェクト名readonly表示
- ステータス変更

#### UI実装
```razor
<EditForm Model="@model" OnValidSubmit="UpdateProject">
    <div class="mb-3">
        <label class="form-label">プロジェクト名（変更不可）</label>
        <input type="text" class="form-control" value="@model.Name" readonly disabled />
        <small class="form-text text-muted">
            プロジェクト名は変更できません
        </small>
    </div>

    <div class="mb-3">
        <label class="form-label">説明</label>
        <InputTextArea @bind-Value="model.Description" class="form-control" rows="5" />
    </div>

    <div class="mb-3">
        <label class="form-label">ステータス</label>
        <div>
            <div class="form-check form-check-inline">
                <InputRadio @bind-Value="model.IsActive" Value="true" class="form-check-input" id="statusActive" />
                <label class="form-check-label" for="statusActive">アクティブ</label>
            </div>
            <div class="form-check form-check-inline">
                <InputRadio @bind-Value="model.IsActive" Value="false" class="form-check-input" id="statusInactive" />
                <label class="form-check-label" for="statusInactive">非アクティブ</label>
            </div>
        </div>
    </div>

    <button type="submit" class="btn btn-primary">
        <i class="fas fa-save me-2"></i>更新
    </button>
</EditForm>

@code {
    [Parameter] public Guid ProjectId { get; set; }
    private UpdateProjectModel model = new();

    protected override async Task OnInitializedAsync() {
        // 既存データ読み込み
        var result = await ProjectManagementService.GetProjectByIdAsync(ProjectId);
        if (result.IsOk) {
            var project = result.ResultValue;
            model = new UpdateProjectModel {
                Id = project.Id,
                Name = project.Name, // readonly表示用
                Description = project.Description,
                IsActive = project.IsActive
            };
        }
    }

    private async Task UpdateProject() {
        var command = new UpdateProjectCommand(
            model.Id,
            model.Description,
            model.IsActive,
            currentUser.Id
        );

        var result = await ProjectManagementService.UpdateProjectAsync(command);
        // Result型処理・UI更新
    }
}
```

### ProjectDeleteDialog.razor 詳細設計

#### 責務
- 削除確認ダイアログ表示
- 関連データ数表示（ドメイン・ユビキタス言語）
- 論理削除実行

#### ConfirmationDialog拡張
```razor
<ConfirmationDialog @ref="dialog"
                   Title="プロジェクト削除確認"
                   Message="@confirmMessage"
                   OnConfirm="OnConfirm"
                   ConfirmButtonText="削除"
                   CancelButtonText="キャンセル"
                   ConfirmButtonClass="btn-danger" />

@code {
    private ConfirmationDialog dialog;
    private ProjectDto selectedProject;
    private int domainCount;
    private int termCount;

    private string confirmMessage =>
        $"プロジェクト「{selectedProject?.Name}」を削除しますか？\n\n" +
        $"関連データ:\n" +
        $"・ドメイン: {domainCount}個\n" +
        $"・ユビキタス言語: {termCount}個\n\n" +
        $"※論理削除のため、後から復元可能です。";

    public async Task Show(ProjectDto project) {
        selectedProject = project;
        // 関連データ数取得
        domainCount = await GetRelatedDomainCount(project.Id);
        termCount = await GetRelatedTermCount(project.Id);
        StateHasChanged();
        await dialog.Show();
    }

    [Parameter] public EventCallback<Guid> OnConfirm { get; set; }
}
```

### ProjectSearchFilter.razor 詳細設計

#### 責務
- 検索条件入力UI
- フィルタ適用・クリア

#### UI実装
```razor
<div class="card mb-3">
    <div class="card-body">
        <div class="row g-3">
            <div class="col-md-6">
                <label class="form-label">プロジェクト名</label>
                <input type="text" class="form-control"
                       @bind="SearchTerm" @bind:event="oninput"
                       placeholder="プロジェクト名で検索" />
            </div>
            <div class="col-md-3">
                <label class="form-label">&nbsp;</label>
                <div class="form-check">
                    <input type="checkbox" class="form-check-input"
                           @bind="ShowDeleted" id="showDeleted" />
                    <label class="form-check-label" for="showDeleted">
                        削除済みを表示
                    </label>
                </div>
            </div>
            <div class="col-md-3 d-flex align-items-end">
                <button class="btn btn-secondary w-100" @onclick="OnClear">
                    <i class="fas fa-times me-1"></i>クリア
                </button>
            </div>
        </div>
    </div>
</div>

@code {
    [Parameter] public string SearchTerm { get; set; } = "";
    [Parameter] public EventCallback<string> SearchTermChanged { get; set; }

    [Parameter] public bool ShowDeleted { get; set; } = false;
    [Parameter] public EventCallback<bool> ShowDeletedChanged { get; set; }

    [Parameter] public EventCallback OnSearch { get; set; }
    [Parameter] public EventCallback OnClear { get; set; }
}
```

## 🔧 技術実装方針

### Railway-oriented Programming Result型処理パターン
```csharp
// F# Result型 → C# 処理パターン
public async Task<bool> ProcessResult<T>(FSharpResult<T, string> result,
                                         Action<T> onSuccess) {
    if (FSharpResult<T, string>.get_IsOk(result)) {
        onSuccess(result.ResultValue);
        return true;
    } else {
        await ShowToast("error", result.ErrorValue);
        return false;
    }
}

// 使用例
var result = await ProjectManagementService.CreateProjectAsync(command);
await ProcessResult(result, async (project) => {
    await ShowToast("success", "プロジェクトを作成しました");
    NavigationManager.NavigateTo("/projects");
});
```

### 権限制御UI実装パターン

#### 4ロール対応実装
```csharp
// Authorize属性（画面アクセス制御）
@attribute [Authorize(Roles = "SuperUser,ProjectManager")]

// 条件付きレンダリング（ボタン表示制御）
@if (currentUserRole == UserRole.SuperUser) {
    <button @onclick="DeleteProject" class="btn btn-danger">
        <i class="fas fa-trash"></i> 削除
    </button>
}

// SecureButton活用（宣言的権限制御）
<SecureButton RequiredRoles='new List<string> { "SuperUser" }'
             RequiredPermission="CreateProject"
             OnClick="ShowCreateModal" />
```

#### サーバーサイド権限検証（二重チェック）
```csharp
// Blazor Server側（第1チェック）
if (currentUserRole != UserRole.SuperUser) {
    await ShowToast("error", "この操作を実行する権限がありません");
    return;
}

// Application層側（第2チェック）
// IProjectManagementService内で権限検証実施済み
var result = await ProjectManagementService.DeleteProjectAsync(command);
```

### エラーハンドリング・通知統合方針

#### Railway-oriented Programming統合
```csharp
// Domain層エラー → Application層 → Web層通知
try {
    var result = await ProjectManagementService.CreateProjectAsync(command);

    if (result.IsOk) {
        await ShowToast("success", "プロジェクトを作成しました");
    } else {
        // Domain層エラーメッセージをそのまま表示
        await ShowToast("error", result.ErrorValue);
    }
} catch (Exception ex) {
    // 予期しないエラー
    await ShowToast("error", "システムエラーが発生しました");
    Logger.LogError(ex, "Project creation failed");
}
```

#### ToastNotification種別
- **success**: CRUD操作成功時
- **error**: バリデーションエラー・Domain層エラー・システムエラー
- **warning**: 警告メッセージ（削除確認等）
- **info**: 情報メッセージ（デフォルトドメイン作成通知等）

## 📊 次Stage準備状況

### Stage2（TDD Red・テスト作成）実施準備完了

#### 必要な技術情報収集完了
- ✅ UI設計書詳細確認（3画面・権限制御マトリックス）
- ✅ Blazor Server実装パターン適用準備
- ✅ コンポーネント構成計画策定（4コンポーネント）
- ✅ 既存基盤活用方針確立

#### SubAgent指示準備完了
**integration-test Agent**:
- WebApplicationFactory統合テスト作成
- UI権限制御テスト作成
- E2E基本動作テスト作成

**参照情報**:
- UI実装設計メモ（本ファイル）
- Application層完全実装済み（IProjectManagementService）
- 権限制御マトリックス（Step01_Requirements_Analysis.md）

## 📝 備考

### 既存基盤活用による効率化
- UserManagement.razorパターン完全再利用
- PermissionGuard/SecureButton等の既存コンポーネント活用
- Application層100点品質基盤統合

### Phase B1最終Step特性
- Web層実装完了でPhase B1完全完了（7/7 Step・100%進捗）
- プロジェクト管理機能完全実装
- Phase C（ドメイン管理機能）準備完了

### AutoCompact対策
- Stage別推定時間明確化（1h・1h・2h・1h・30min）
- 各Stage完了時ユーザー判断（継続 or 次セッション送り）
- コンテキスト使用状況監視
