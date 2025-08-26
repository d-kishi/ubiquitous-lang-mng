# Phase A7 Step3: アーキテクチャ完全統一 - 詳細実装カード

## 📋 Step3概要
- **Step名**: アーキテクチャ完全統一
- **所要時間**: 120-150分
- **重要度**: 最高（Pure Blazor Server要件実現）
- **SubAgent**: csharp-web-ui・fsharp-application・code-review

## 🎯 対応課題一覧

### 課題1: [ARCH-001] MVC/Blazor混在アーキテクチャ
- **問題**: 要件定義4.2.1「Pure Blazor Server」に対しMVC Controller併存
- **影響**: アーキテクチャ全体・将来拡張性・保守性
- **証跡**: HomeController.cs:12、Views/Home/Index.cshtml等

### 課題2: Pure Blazor Server要件違反
- **問題**: システム設計書「Pure Blazor Server」要件に対しMVC混在
- **影響**: アーキテクチャ統一性欠如・保守性低下・拡張性制約
- **証跡**: Web層品質45/100（MVC混在による）

### 課題3: URL設計統一性課題・不備
- **問題**: MVC(/Account/ChangePassword)とBlazor(/admin/users)形式混在
- **影響**: ユーザビリティ・開発効率・設計一貫性
- **証跡**: 複数URL形式併存

### 課題4: エラーハンドリング統一不足
- **問題**: F# Result型とC#例外処理の統一不足
- **影響**: エラー処理一貫性・デバッグ効率
- **証跡**: 層間でのエラーハンドリング方式不統一

## 🗂️ MVC要素完全削除タスク

### タスク1: Controllers完全削除

#### 削除対象ファイル
```
1. src/UbiquitousLanguageManager.Web/Controllers/HomeController.cs
2. src/UbiquitousLanguageManager.Web/Controllers/AccountController.cs (Step2で作成・Step3で削除)
```

#### HomeController削除による影響と対応
```csharp
// 削除前の機能（HomeController.cs）
public class HomeController : Controller
{
    public IActionResult Index()
    {
        if (User.Identity.IsAuthenticated)
            return Redirect("/admin/users");
        else
            return Redirect("/login");
    }
}
```

**代替実装**: App.razorでの認証分岐制御

### タスク2: Views完全削除

#### 削除対象ディレクトリ・ファイル一覧
```
src/UbiquitousLanguageManager.Web/Views/
├── Home/
│   ├── Index.cshtml           ← 削除
│   └── Error.cshtml           ← 削除
├── Account/
│   ├── ChangePassword.cshtml  ← 削除
│   └── AccessDenied.cshtml    ← 削除
├── Shared/
│   ├── _Layout.cshtml         ← 削除
│   └── _ValidationScriptsPartial.cshtml ← 削除
├── _ViewImports.cshtml        ← 削除
└── _ViewStart.cshtml          ← 削除
```

### タスク3: Program.cs MVC設定削除

#### 削除対象設定（具体的行番号は実装時確認）
```csharp
// 削除対象
builder.Services.AddControllersWithViews();

// 削除対象
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
```

#### 保持する設定
```csharp
// 保持：Blazor Server設定
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
app.MapRazorPages();
app.MapBlazorHub();
app.MapFallbackToPage("/_Host");
```

## 🔄 Blazor統一実装タスク

### タスク4: App.razor認証分岐実装

#### ファイル修正
```
パス: src/UbiquitousLanguageManager.Web/App.razor
```

#### 実装内容（ルート「/」の認証分岐制御）
```razor
@using Microsoft.AspNetCore.Components.Authorization

<CascadingAuthenticationState>
    <Router AppAssembly="@typeof(App).Assembly">
        <Found Context="routeData">
            <AuthorizeRouteView RouteData="@routeData" DefaultLayout="@typeof(MainLayout)">
                <NotAuthorized>
                    @if (context.User.Identity?.IsAuthenticated != true)
                    {
                        @* 未認証時は自動的にログイン画面へ *@
                        <RedirectToLogin />
                    }
                    else
                    {
                        @* 認証済みだが権限不足 *@
                        <UnauthorizedAccess />
                    }
                </NotAuthorized>
            </AuthorizeRouteView>
        </Found>
        <NotFound>
            <PageTitle>Page not found</PageTitle>
            <LayoutView Layout="@typeof(MainLayout)">
                <NotFoundPage />
            </LayoutView>
        </NotFound>
    </Router>
</CascadingAuthenticationState>
```

### タスク5: ルート（/）ページ実装

#### 新規ファイル作成
```
パス: src/UbiquitousLanguageManager.Web/Pages/Index.razor
```

#### 実装内容（認証分岐）
```razor
@page "/"
@using Microsoft.AspNetCore.Components.Authorization
@inject AuthenticationStateProvider AuthenticationStateProvider
@inject NavigationManager Navigation

<PageTitle>ユビキタス言語管理システム</PageTitle>

@code {
    protected override async Task OnInitializedAsync()
    {
        var authState = await AuthenticationStateProvider.GetAuthenticationStateAsync();
        
        if (authState.User.Identity?.IsAuthenticated == true)
        {
            // 認証済みユーザーは管理画面へリダイレクト
            Navigation.NavigateTo("/admin/users");
        }
        else
        {
            // 未認証ユーザーはログイン画面へリダイレクト
            Navigation.NavigateTo("/login");
        }
    }
}
```

### タスク6: エラーハンドリング統一実装

#### 統一エラーハンドリング戦略

##### F#側Result型→C#側例外変換
```
パス: src/UbiquitousLanguageManager.Contracts/Mappers/ResultMapper.cs
```

```csharp
public static class ResultMapper
{
    /// F# Resultを C# 例外に変換する
    public static T MapResult<T>(FSharpResult<T, string> result)
    {
        if (FSharpResult.IsOk(result))
        {
            return result.ResultValue;
        }
        else
        {
            throw new DomainException(result.ErrorValue);
        }
    }

    /// F# Async<Result>を C# Task に変換する
    public static async Task<T> MapResultAsync<T>(Async<FSharpResult<T, string>> asyncResult)
    {
        var result = await FSharpAsync.StartAsTask(asyncResult);
        return MapResult(result);
    }
}
```

##### DomainException定義
```
パス: src/UbiquitousLanguageManager.Contracts/Exceptions/DomainException.cs
```

```csharp
public class DomainException : Exception
{
    public DomainException(string message) : base(message) { }
    public DomainException(string message, Exception innerException) : base(message, innerException) { }
}
```

##### Blazorコンポーネントでの統一エラー表示
```
パス: src/UbiquitousLanguageManager.Web/Shared/ErrorBoundary.razor
```

```razor
<ErrorBoundary>
    <ChildContent>
        @ChildContent
    </ChildContent>
    <ErrorContent>
        <div class="alert alert-danger">
            <h4>エラーが発生しました</h4>
            <p>@context.Message</p>
            <button type="button" class="btn btn-primary" @onclick="@(() => Recovery())">
                再試行
            </button>
        </div>
    </ErrorContent>
</ErrorBoundary>

@code {
    [Parameter] public RenderFragment ChildContent { get; set; }
    
    private void Recovery()
    {
        // ErrorBoundaryの回復処理
        StateHasChanged();
    }
}
```

## 📋 URL設計統一タスク

### タスク7: Blazor形式URL統一

#### 統一後のURL設計
```
認証関連:
✅ /login              (Login.razor)
✅ /logout             (Logout.razor)
✅ /change-password    (ChangePassword.razor)

管理画面:
✅ /admin/users        (UserManagement.razor)
✅ /                   (Index.razor → 認証分岐)

エラー・アクセス拒否:
✅ /error              (ErrorPage.razor)
✅ /unauthorized       (UnauthorizedAccess.razor)

削除されるURL:
❌ /Account/ChangePassword  (MVC削除により)
❌ /Home/Index              (MVC削除により)
❌ /Home/Error              (MVC削除により)
```

#### 実装確認項目
- [ ] 全URLがBlazor形式（小文字・ハイフン区切り）
- [ ] MVC形式URL（PascalCase・/Controller/Action）完全削除
- [ ] 既存機能への影響なし

## 🎯 Step3完了確認項目

### MVC削除確認
- [ ] Controllers/ディレクトリ完全削除
- [ ] Views/ディレクトリ完全削除
- [ ] Program.cs内AddControllersWithViews削除
- [ ] Program.cs内MapControllerRoute削除

### Blazor統一確認
- [ ] / アクセス時の認証分岐正常動作
- [ ] /login 未認証ユーザー正常アクセス
- [ ] /admin/users 認証済みユーザー正常アクセス
- [ ] /change-password 認証済みユーザー正常アクセス

### エラーハンドリング確認
- [ ] F# Result型エラーがBlazorで適切表示
- [ ] DomainExceptionが統一フォーマットで表示
- [ ] ErrorBoundaryによる回復機能正常動作

### ビルド・品質確認
- [ ] `dotnet build` 成功（0 Warning, 0 Error）
- [ ] `dotnet test` 全テスト成功
- [ ] 認証フロー完全動作確認（ログイン→管理画面→ログアウト）

### 設計準拠確認
- [ ] Pure Blazor Serverアーキテクチャ実現
- [ ] システム設計書「Pure Blazor Server」要件100%準拠
- [ ] URL設計完全統一（Blazor形式）

## 📋 Step4への引き継ぎ事項

### 重要な統一完了事項
1. **Pure Blazor Serverアーキテクチャ実現**
   - MVC要素完全削除済み
   - FirstLoginRedirectMiddleware統合準備完了

2. **URL設計完全統一**
   - 全URLがBlazor形式で統一済み
   - /change-password 実装完了（Middleware統合可能）

3. **エラーハンドリング統一基盤完成**
   - F#↔C#境界でのエラー変換メカニズム確立
   - Step4でのContracts層拡張時の基盤利用可能

### Step4での作業準備完了
- FirstLoginRedirectMiddlewareのパス統合基盤整備済み
- TypeConverter拡張時のエラーハンドリング基盤整備済み
- Contracts層拡張に必要なBlazor統一基盤確立

---

**実装担当**: csharp-web-ui・fsharp-application・code-review SubAgents  
**完了予定**: Step3実施セッション内  
**次工程**: Step4（Contracts層・型変換完全実装）