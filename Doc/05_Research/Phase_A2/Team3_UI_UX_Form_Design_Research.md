# チーム3: UI/UX・フォーム設計専門分析

**分析日**: 2025-07-20  
**専門領域**: Blazor Server複雑UI・フォームバリデーション・ユーザビリティ  

## 🔍 発見された技術課題

### 1. ユーザー管理DataGrid実装
**課題**: 検索・フィルタ・ソート・ページング統合UI  
**影響度**: 🟡 中（UI実装）  

**解決アプローチ**:
```razor
@page "/users"
@attribute [Authorize(Policy = "ManageUsers")]

<div class="card">
    <div class="card-header">
        <h3>👥 ユーザー一覧</h3>
    </div>
    
    <!-- 検索・フィルタセクション -->
    <div class="card-body">
        <EditForm Model="@searchCriteria" OnValidSubmit="@SearchUsers">
            <div class="row mb-3">
                <div class="col-md-4">
                    <label>👤 氏名</label>
                    <InputText @bind-Value="searchCriteria.Name" class="form-control" 
                               placeholder="部分一致検索" />
                </div>
                <div class="col-md-4">
                    <label>📁 プロジェクト</label>
                    <InputSelect @bind-Value="searchCriteria.ProjectId" class="form-control">
                        <option value="">全て</option>
                        @foreach (var project in projects)
                        {
                            <option value="@project.Id">@project.Name</option>
                        }
                    </InputSelect>
                </div>
                <div class="col-md-4">
                    <label>&nbsp;</label>
                    <div>
                        <button type="submit" class="btn btn-primary">🔍 検索</button>
                        <InputCheckbox @bind-Value="searchCriteria.IncludeDeleted" />
                        削除済み表示
                    </div>
                </div>
            </div>
        </EditForm>
    </div>
    
    <!-- データグリッド -->
    <div class="table-responsive">
        <table class="table table-hover">
            <thead>
                <tr>
                    <th @onclick="@(() => Sort("Name"))">
                        👤 氏名 @GetSortIcon("Name")
                    </th>
                    <th>📧 メールアドレス</th>
                    <th>🎭 権限</th>
                    <th>📁 プロジェクト</th>
                    <th>操作</th>
                </tr>
            </thead>
            <tbody>
                @foreach (var user in pagedUsers.Items)
                {
                    <tr class="@(user.IsDeleted ? "table-secondary" : "")">
                        <td>@user.Name</td>
                        <td>@user.Email</td>
                        <td>@user.RoleName</td>
                        <td>@string.Join(", ", user.ProjectNames)</td>
                        <td>
                            <button class="btn btn-sm btn-outline-primary" 
                                    @onclick="@(() => EditUser(user.Id))">
                                ✏️ 編集
                            </button>
                            @if (!user.IsDeleted)
                            {
                                <button class="btn btn-sm btn-outline-danger" 
                                        @onclick="@(() => DeleteUser(user.Id))">
                                    🗑️ 削除
                                </button>
                            }
                        </td>
                    </tr>
                }
            </tbody>
        </table>
    </div>
    
    <!-- ページング -->
    <Pagination TotalPages="@pagedUsers.TotalPages" 
                CurrentPage="@searchCriteria.PageIndex"
                OnPageChanged="@OnPageChanged" />
</div>
```

### 2. 複雑フォームバリデーション
**課題**: リアルタイム検証とサーバーサイド検証の統合  
**影響度**: 🟡 中（UX品質）  

**解決アプローチ**:
```csharp
// ユーザー登録モデル（DataAnnotations）
public class UserCreateModel
{
    [Required(ErrorMessage = "メールアドレスは必須です")]
    [EmailAddress(ErrorMessage = "有効なメールアドレスを入力してください")]
    [UniqueEmail] // カスタムバリデーション属性
    public string Email { get; set; }
    
    [Required(ErrorMessage = "氏名は必須です")]
    [StringLength(50, ErrorMessage = "氏名は50文字以内で入力してください")]
    public string Name { get; set; }
    
    [Required(ErrorMessage = "パスワードは必須です")]
    [PasswordStrength] // カスタムバリデーション属性
    public string Password { get; set; }
    
    [Required(ErrorMessage = "権限の選択は必須です")]
    public string RoleId { get; set; }
}

// カスタムバリデーション属性
public class PasswordStrengthAttribute : ValidationAttribute
{
    protected override ValidationResult IsValid(object value, ValidationContext context)
    {
        var password = value as string;
        if (string.IsNullOrEmpty(password))
            return ValidationResult.Success;
        
        var errors = new List<string>();
        if (password.Length < 8)
            errors.Add("8文字以上");
        if (!Regex.IsMatch(password, "[A-Z]"))
            errors.Add("大文字を含む");
        if (!Regex.IsMatch(password, "[0-9]"))
            errors.Add("数字を含む");
        
        if (errors.Any())
            return new ValidationResult($"パスワードは{string.Join("、", errors)}必要があります");
        
        return ValidationResult.Success;
    }
}
```

### 3. 権限ベースUI制御
**課題**: メニュー項目・ボタンの動的表示制御  
**影響度**: 🔴 高（セキュリティ）  

**解決アプローチ**:
```razor
<!-- 権限ベースメニュー表示 -->
<nav class="sidebar">
    <ul class="nav flex-column">
        <li class="nav-item">
            <NavLink class="nav-link" href="/">🏠 ホーム</NavLink>
        </li>
        <li class="nav-item">
            <NavLink class="nav-link" href="/ubiquitous-language">✏️ 入力・編集</NavLink>
        </li>
        
        <AuthorizeView Roles="DomainApprover,ProjectManager,SuperUser">
            <li class="nav-item">
                <NavLink class="nav-link" href="/approval">✅ 承認</NavLink>
            </li>
        </AuthorizeView>
        
        <AuthorizeView Roles="ProjectManager,SuperUser">
            <li class="nav-item">
                <NavLink class="nav-link" href="/users">👥 ユーザー管理</NavLink>
            </li>
            <li class="nav-item">
                <NavLink class="nav-link" href="/projects">📁 プロジェクト管理</NavLink>
            </li>
        </AuthorizeView>
    </ul>
</nav>
```

## 📊 Gemini技術調査結果

### 調査: Blazor Server複雑UI実装
**キーポイント**:
- EditFormによるフォーム全体の検証コンテキスト管理
- DataAnnotationsによる宣言的バリデーション
- カスタムバリデーション属性での複雑ルール実装
- AuthorizeViewによる権限ベースUI制御

## 🎯 実装推奨事項

### Web層実装優先順位
1. **共通レイアウト**: サイドメニュー・権限制御
2. **ユーザー一覧画面**: DataGrid基本機能
3. **ユーザー登録/編集**: フォームバリデーション
4. **プロファイル/パスワード変更**: 自己管理機能

### UI/UXベストプラクティス
- **Loading状態**: 非同期処理中の視覚的フィードバック
- **エラー処理**: トースター通知での成功/失敗表示
- **確認ダイアログ**: 削除等の破壊的操作前確認

### 技術的リスクと対策
- **リスク**: SignalR切断時のユーザー体験悪化
- **対策**: 再接続処理と適切なエラーメッセージ

---

**分析完了**: ユーザビリティ重視のBlazor UI実装方針確立