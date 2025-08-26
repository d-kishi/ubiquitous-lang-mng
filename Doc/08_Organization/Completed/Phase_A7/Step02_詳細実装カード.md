# Phase A7 Step2: 緊急対応・基盤整備 - 詳細実装カード

## 📋 Step2概要
- **Step名**: 緊急対応・基盤整備
- **所要時間**: 90-120分
- **緊急度**: 最高（404エラー解消・機能停止回避）
- **SubAgent**: csharp-infrastructure・csharp-web-ui

## 🚨 対応課題一覧

### 課題1: [CTRL-001] AccountController未実装（CRITICAL）
- **問題**: Views/Account/ChangePassword.cshtmlが参照するController未実装
- **影響**: 404エラー・認証システム機能停止
- **証跡**: Views/Account/ChangePassword.cshtml:1参照、Controllerファイル不存在

### 課題2: Application層インターフェース未実装
- **問題**: 設計書定義の主要サービスインターフェース未実装
- **影響**: 機能拡張時の技術的制約・設計意図との乖離
- **証跡**: IUbiquitousLanguageService等設計書定義未実装

### 課題3: Blazorパスワード変更画面未実装
- **問題**: FirstLoginRedirectMiddleware期待パス（/change-password）未実装
- **影響**: 初回ログインフロー不整合
- **証跡**: RedirectUrl = "/change-password" vs 実装なし

## 🛠️ 詳細実装タスク

### タスク1: AccountController緊急実装

#### ファイル作成
```
パス: src/UbiquitousLanguageManager.Web/Controllers/AccountController.cs
```

#### 実装内容
```csharp
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using UbiquitousLanguageManager.Web.Models;

[Authorize]
[Route("Account")]
public class AccountController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly ILogger<AccountController> _logger;

    public AccountController(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        ILogger<AccountController> logger)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _logger = logger;
    }

    [HttpGet("ChangePassword")]
    public IActionResult ChangePassword()
    {
        return View();
    }

    [HttpPost("ChangePassword")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var user = await _userManager.GetUserAsync(User);
        if (user == null)
            return RedirectToAction("Login", "Home");

        var result = await _userManager.ChangePasswordAsync(user, 
            model.CurrentPassword, model.NewPassword);

        if (result.Succeeded)
        {
            await _signInManager.RefreshSignInAsync(user);
            _logger.LogInformation("User changed their password successfully.");
            return RedirectToAction("Index", "Home");
        }

        foreach (var error in result.Errors)
            ModelState.AddModelError(string.Empty, error.Description);

        return View(model);
    }
}
```

#### ViewModelクラス作成
```
パス: src/UbiquitousLanguageManager.Web/Models/ChangePasswordViewModel.cs
```

```csharp
using System.ComponentModel.DataAnnotations;

public class ChangePasswordViewModel
{
    [Required]
    [DataType(DataType.Password)]
    [Display(Name = "現在のパスワード")]
    public string CurrentPassword { get; set; } = string.Empty;

    [Required]
    [StringLength(100, ErrorMessage = "パスワードは {2} 文字以上である必要があります。", MinimumLength = 6)]
    [DataType(DataType.Password)]
    [Display(Name = "新しいパスワード")]
    public string NewPassword { get; set; } = string.Empty;

    [DataType(DataType.Password)]
    [Display(Name = "新しいパスワード（確認）")]
    [Compare("NewPassword", ErrorMessage = "新しいパスワードと確認用パスワードが一致しません。")]
    public string ConfirmPassword { get; set; } = string.Empty;
}
```

### タスク2: Blazorパスワード変更画面実装

#### ファイル作成
```
パス: src/UbiquitousLanguageManager.Web/Pages/Auth/ChangePassword.razor
```

#### 実装内容
```razor
@page "/change-password"
@layout MainLayout
@using UbiquitousLanguageManager.Contracts.DTOs.Authentication
@using Microsoft.AspNetCore.Identity
@inject UserManager<ApplicationUser> UserManager
@inject SignInManager<ApplicationUser> SignInManager
@inject NavigationManager Navigation
@inject AuthenticationStateProvider AuthenticationStateProvider
@attribute [Authorize]

<PageTitle>パスワード変更</PageTitle>

<div class="container">
    <div class="row justify-content-center">
        <div class="col-md-6">
            <div class="card">
                <div class="card-header">
                    <h3 class="mb-0">パスワード変更</h3>
                </div>
                <div class="card-body">
                    <EditForm Model="@model" OnValidSubmit="@HandleSubmit">
                        <DataAnnotationsValidator />
                        <ValidationSummary class="text-danger" />

                        <div class="mb-3">
                            <label for="currentPassword" class="form-label">現在のパスワード</label>
                            <InputText id="currentPassword" class="form-control" 
                                      @bind-Value="model.CurrentPassword" type="password" />
                            <ValidationMessage For="@(() => model.CurrentPassword)" />
                        </div>

                        <div class="mb-3">
                            <label for="newPassword" class="form-label">新しいパスワード</label>
                            <InputText id="newPassword" class="form-control" 
                                      @bind-Value="model.NewPassword" type="password" />
                            <ValidationMessage For="@(() => model.NewPassword)" />
                        </div>

                        <div class="mb-3">
                            <label for="confirmPassword" class="form-label">新しいパスワード（確認）</label>
                            <InputText id="confirmPassword" class="form-control" 
                                      @bind-Value="model.ConfirmPassword" type="password" />
                            <ValidationMessage For="@(() => model.ConfirmPassword)" />
                        </div>

                        @if (!string.IsNullOrEmpty(errorMessage))
                        {
                            <div class="alert alert-danger">@errorMessage</div>
                        }

                        @if (!string.IsNullOrEmpty(successMessage))
                        {
                            <div class="alert alert-success">@successMessage</div>
                        }

                        <div class="d-grid gap-2">
                            <button type="submit" class="btn btn-primary" disabled="@isProcessing">
                                @if (isProcessing)
                                {
                                    <span class="spinner-border spinner-border-sm me-2"></span>
                                }
                                パスワードを変更
                            </button>
                        </div>
                    </EditForm>
                </div>
            </div>
        </div>
    </div>
</div>

@code {
    private ChangePasswordRequestDto model = new();
    private string errorMessage = string.Empty;
    private string successMessage = string.Empty;
    private bool isProcessing = false;

    private async Task HandleSubmit()
    {
        isProcessing = true;
        errorMessage = string.Empty;
        successMessage = string.Empty;

        try
        {
            var authState = await AuthenticationStateProvider.GetAuthenticationStateAsync();
            var user = await UserManager.GetUserAsync(authState.User);

            if (user == null)
            {
                Navigation.NavigateTo("/login");
                return;
            }

            var result = await UserManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);

            if (result.Succeeded)
            {
                await SignInManager.RefreshSignInAsync(user);
                successMessage = "パスワードが正常に変更されました。";
                model = new ChangePasswordRequestDto(); // フォームリセット

                // 2秒後に管理画面にリダイレクト
                await Task.Delay(2000);
                Navigation.NavigateTo("/admin/users");
            }
            else
            {
                errorMessage = string.Join(", ", result.Errors.Select(e => e.Description));
            }
        }
        catch (Exception ex)
        {
            errorMessage = "パスワード変更中にエラーが発生しました。";
        }
        finally
        {
            isProcessing = false;
            StateHasChanged();
        }
    }
}
```

### タスク3: Application層インターフェース基盤実装

#### IUbiquitousLanguageService実装
```
パス: src/UbiquitousLanguageManager.Application/Interfaces/IUbiquitousLanguageService.cs
```

```fsharp
namespace UbiquitousLanguageManager.Application.Interfaces

open UbiquitousLanguageManager.Domain.Entities
open UbiquitousLanguageManager.Domain.ValueObjects

type IUbiquitousLanguageService =
    /// ユビキタス言語を作成する
    abstract CreateUbiquitousLanguageAsync : UbiquitousLanguage -> Async<Result<UbiquitousLanguage, string>>
    
    /// ユビキタス言語を更新する
    abstract UpdateUbiquitousLanguageAsync : UbiquitousLanguage -> Async<Result<UbiquitousLanguage, string>>
    
    /// ユビキタス言語を削除する
    abstract DeleteUbiquitousLanguageAsync : UbiquitousLanguageId -> Async<Result<unit, string>>
    
    /// IDでユビキタス言語を取得する
    abstract GetUbiquitousLanguageByIdAsync : UbiquitousLanguageId -> Async<Result<UbiquitousLanguage option, string>>
    
    /// プロジェクト内のユビキタス言語一覧を取得する
    abstract GetUbiquitousLanguagesByProjectAsync : ProjectId -> Async<Result<UbiquitousLanguage list, string>>
```

#### IProjectService実装
```
パス: src/UbiquitousLanguageManager.Application/Interfaces/IProjectService.cs
```

```fsharp
namespace UbiquitousLanguageManager.Application.Interfaces

open UbiquitousLanguageManager.Domain.Entities
open UbiquitousLanguageManager.Domain.ValueObjects

type IProjectService =
    /// プロジェクトを作成する
    abstract CreateProjectAsync : Project -> Async<Result<Project, string>>
    
    /// プロジェクトを更新する
    abstract UpdateProjectAsync : Project -> Async<Result<Project, string>>
    
    /// プロジェクトを削除する
    abstract DeleteProjectAsync : ProjectId -> Async<Result<unit, string>>
    
    /// IDでプロジェクトを取得する
    abstract GetProjectByIdAsync : ProjectId -> Async<Result<Project option, string>>
    
    /// ユーザーがアクセス可能なプロジェクト一覧を取得する
    abstract GetProjectsByUserAsync : UserId -> Async<Result<Project list, string>>
```

#### IDomainService実装
```
パス: src/UbiquitousLanguageManager.Application/Interfaces/IDomainService.cs
```

```fsharp
namespace UbiquitousLanguageManager.Application.Interfaces

open UbiquitousLanguageManager.Domain.Entities
open UbiquitousLanguageManager.Domain.ValueObjects

type IDomainService =
    /// ドメインを作成する
    abstract CreateDomainAsync : Domain -> Async<Result<Domain, string>>
    
    /// ドメインを更新する
    abstract UpdateDomainAsync : Domain -> Async<Result<Domain, string>>
    
    /// ドメインを削除する
    abstract DeleteDomainAsync : DomainId -> Async<Result<unit, string>>
    
    /// IDでドメインを取得する
    abstract GetDomainByIdAsync : DomainId -> Async<Result<Domain option, string>>
    
    /// プロジェクト内のドメイン一覧を取得する
    abstract GetDomainsByProjectAsync : ProjectId -> Async<Result<Domain list, string>>
```

## 🎯 Step2完了確認項目

### 機能確認
- [ ] `/Account/ChangePassword` にアクセス時404エラーが発生しない
- [ ] `/change-password` が正常表示される
- [ ] MVC版パスワード変更フォームが正常送信できる
- [ ] Blazor版パスワード変更フォームが正常送信できる
- [ ] パスワード変更後にSignInManager.RefreshSignInAsyncが正常動作

### ビルド確認
- [ ] `dotnet build` 成功（0 Warning, 0 Error）
- [ ] `dotnet run` でアプリケーション正常起動
- [ ] 認証状態でのページアクセス正常

### セキュリティ確認
- [ ] [Authorize]属性による認証チェック動作
- [ ] ValidateAntiForgeryToken正常動作
- [ ] パスワード要件（最小6文字等）正常チェック

## 📋 Step3への引き継ぎ事項

### 重要な移行準備
1. **AccountController暫定実装完了**
   - Step3でMVC削除時にこのControllerも削除予定
   - 削除前に/change-passwordへの完全移行確認必要

2. **Blazor版パスワード変更画面実装完了**
   - FirstLoginRedirectMiddleware統合準備完了
   - Step4でMiddleware修正時の動作確認基盤整備済み

3. **Application層インターフェース基盤完成**
   - Step4でContracts層拡張時の基盤準備完了
   - Step5でUI機能拡張時のサービス基盤利用可能

### 注意事項
- AccountControllerは**暫定実装**（Step3削除予定）
- Views/Account/ChangePassword.cshtmlは残存（Step3削除予定）
- MVC/Blazor双方のパスワード変更機能が併存状態

---

**実装担当**: csharp-infrastructure・csharp-web-ui SubAgents  
**完了予定**: Step2実施セッション内  
**次工程**: Step3（アーキテクチャ完全統一）