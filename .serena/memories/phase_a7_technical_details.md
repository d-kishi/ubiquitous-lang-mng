# Phase A7 技術詳細メモリ

## FirstLoginRedirectMiddleware統合詳細

### 現在の不整合
```csharp
// Middleware期待パス
private const string ChangePasswordPath = "/Account/ChangePassword";

// 実際の状況
- MVC版: /Account/ChangePassword (Controller未実装・404エラー)
- 期待: /change-password (Blazor Server版)
```

### Step2-4での解決手順
1. **Step2**: AccountController実装（暫定対応）
2. **Step2**: /change-password Blazor版実装  
3. **Step4**: Middleware修正（/change-passwordへ統一）
4. **Step3**: MVC版削除

### 修正後のコード
```csharp
// Step4で修正予定
private const string ChangePasswordPath = "/change-password";

public async Task InvokeAsync(HttpContext context)
{
    if (context.User.Identity?.IsAuthenticated == true)
    {
        var user = await _userManager.GetUserAsync(context.User);
        
        if (user?.RequirePasswordChange == true && 
            !context.Request.Path.StartsWithSegments(ChangePasswordPath))
        {
            context.Response.Redirect(ChangePasswordPath);
            return;
        }
    }
    await _next(context);
}
```

## AccountController実装仕様（Step2暫定）

### ファイルパス
```
src/UbiquitousLanguageManager.Web/Controllers/AccountController.cs
```

### 実装必須メソッド
```csharp
[HttpGet("ChangePassword")]
public IActionResult ChangePassword() => View();

[HttpPost("ChangePassword")]  
[ValidateAntiForgeryToken]
public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
{
    // UserManager<ApplicationUser>使用
    // 成功時: RedirectToAction("Index", "Home")
    // 失敗時: View(model)
}
```

### 依存関係
- UserManager<ApplicationUser>
- SignInManager<ApplicationUser>  
- ILogger<AccountController>

### 注意事項
- **暫定実装**: Step3でMVC削除時に同時削除予定
- Views/Account/ChangePassword.cshtml依存あり

## F#/C#境界設計詳細

### Result型変換パターン
```csharp
// Contracts/Mappers/ResultMapper.cs
public static T MapResult<T>(FSharpResult<T, string> result)
{
    if (FSharpResult.IsOk(result))
        return result.ResultValue;
    else
        throw new DomainException(result.ErrorValue);
}

public static async Task<T> MapResultAsync<T>(Async<FSharpResult<T, string>> asyncResult)
{
    var result = await FSharpAsync.StartAsTask(asyncResult);
    return MapResult(result);
}
```

### TypeConverter実装パターン
```csharp
// 基本パターン
public class EntityTypeConverter
{
    public static EntityDto ToDto(Entity domain) { /* Domain → DTO */ }
    public static Result<Entity, string> FromDto(EntityDto dto) { /* DTO → Domain */ }
}
```

### エラーハンドリング統一
```csharp
// DomainException定義
public class DomainException : Exception
{
    public DomainException(string message) : base(message) { }
}

// Blazor ErrorBoundary使用
<ErrorBoundary>
    <ChildContent>@ChildContent</ChildContent>
    <ErrorContent>
        <div class="alert alert-danger">@context.Message</div>
    </ErrorContent>
</ErrorBoundary>
```

## URL設計統一戦略

### 統一後URL設計
```
認証関連（全Blazor Server）:
/login              → Login.razor
/logout             → Logout.razor  
/change-password    → ChangePassword.razor
/profile            → Profile.razor

管理画面:
/                   → Index.razor（認証分岐）
/admin/users        → UserManagement.razor

削除されるMVC URL:
/Account/ChangePassword  → 削除（Step3）
/Home/Index              → 削除（Step3）
/Home/Error              → 削除（Step3）
```

### URL形式規則
- **Blazor形式**: 小文字・ハイフン区切り（/change-password）
- **削除MVC形式**: PascalCase・/Controller/Action（/Account/ChangePassword）

## Application層インターフェース設計

### 実装予定インターフェース（Step2）
```fsharp
// IUbiquitousLanguageService
type IUbiquitousLanguageService =
    abstract CreateUbiquitousLanguageAsync : UbiquitousLanguage -> Async<Result<UbiquitousLanguage, string>>
    abstract UpdateUbiquitousLanguageAsync : UbiquitousLanguage -> Async<Result<UbiquitousLanguage, string>>
    abstract DeleteUbiquitousLanguageAsync : UbiquitousLanguageId -> Async<Result<unit, string>>
    abstract GetUbiquitousLanguageByIdAsync : UbiquitousLanguageId -> Async<Result<UbiquitousLanguage option, string>>
    abstract GetUbiquitousLanguagesByProjectAsync : ProjectId -> Async<Result<UbiquitousLanguage list, string>>

// IProjectService
type IProjectService =
    abstract CreateProjectAsync : Project -> Async<Result<Project, string>>
    abstract UpdateProjectAsync : Project -> Async<Result<Project, string>>
    abstract DeleteProjectAsync : ProjectId -> Async<Result<unit, string>>
    abstract GetProjectByIdAsync : ProjectId -> Async<Result<Project option, string>>
    abstract GetProjectsByUserAsync : UserId -> Async<Result<Project list, string>>

// IDomainService  
type IDomainService =
    abstract CreateDomainAsync : Domain -> Async<Result<Domain, string>>
    abstract UpdateDomainAsync : Domain -> Async<Result<Domain, string>>
    abstract DeleteDomainAsync : DomainId -> Async<Result<unit, string>>
    abstract GetDomainByIdAsync : DomainId -> Async<Result<Domain option, string>>
    abstract GetDomainsByProjectAsync : ProjectId -> Async<Result<Domain list, string>>
```

## MVC削除安全手順

### 削除順序（厳守）
1. **Controllers削除**: HomeController.cs → AccountController.cs  
2. **Views削除**: Home/* → Account/* → Shared/*
3. **Program.cs設定削除**: AddControllersWithViews → MapControllerRoute
4. **確認**: 各段階でビルド・動作確認

### 削除前確認事項
- [ ] Step2完了: AccountController・/change-password実装済み
- [ ] 代替Blazor実装確認: 各削除対象の代替機能動作確認
- [ ] テスト更新: MVC依存テスト削除・Blazor版テスト実装

### 削除後確認必須項目
- [ ] 全URL正常動作: /・/login・/change-password・/profile・/admin/users
- [ ] 認証フロー完全動作: 初回ログイン→パスワード変更→管理画面アクセス
- [ ] エラーハンドリング: F#→C#境界エラーの統一表示

## 用語統一実施対象

### 置換対象
```bash
"用語" → "ユビキタス言語"
"Term" → "UbiquitousLanguage" (適切な文脈で)  
"語彙" → "ユビキタス言語"
"専門用語" → "ユビキタス言語"
```

### 対象ファイル群
- src/UbiquitousLanguageManager.Web/Pages/*.razor
- src/UbiquitousLanguageManager.Web/Shared/*.razor
- src/UbiquitousLanguageManager.Contracts/DTOs/*.cs
- src/UbiquitousLanguageManager.Application/*.fs
- README.md, CLAUDE.md等ドキュメント

## プロフィール変更画面実装詳細

### 実装ファイル
```
src/UbiquitousLanguageManager.Web/Pages/Auth/Profile.razor
src/UbiquitousLanguageManager.Contracts/DTOs/Authentication/ProfileUpdateDto.cs
```

### 実装機能
- ユーザー名・姓・名の変更
- メールアドレス表示（変更不可）
- ロール・作成日・最終ログイン表示
- パスワード変更画面へのリンク

### バリデーション
- Required属性: ユーザー名・メールアドレス
- StringLength属性: 各フィールド50文字制限
- EmailAddress属性: メールアドレス形式チェック

## セッション跨ぎ重要チェックポイント

### Step2完了判定
- [ ] /Account/ChangePassword 404エラー解消
- [ ] /change-password 正常表示
- [ ] Application層インターフェース実装完了

### Step3完了判定  
- [ ] Controllers/Views完全削除
- [ ] Pure Blazor Server実現
- [ ] 全URL正常動作

### Step4完了判定
- [ ] TypeConverter完全実装
- [ ] FirstLoginRedirectMiddleware統合完了
- [ ] 初回ログインフロー完全動作

### 最終確認（Step6）
- [ ] GitHub Issues #5・#6完全解決
- [ ] 要件準拠度90%以上達成
- [ ] アーキテクチャ品質85/100以上達成