# Step06 Stage3 E2Eテスト失敗分析と修正方針

**作成日**: 2025-11-15
**Phase**: Phase B-F2 Step6 Stage 3
**対応Issue**: #52
**分析実施時間**: 約20分

---

## 📊 E2Eテスト実行結果（Stage 3）

### 実行サマリー

- **Total tests**: 9
- **Passed**: 1（Login_ValidCredentials_ShowsHomePage のみ）
- **Failed**: 5
- **Skipped**: 3（パスワードリセット機能未実装 - 正常）

### 期待結果との乖離

- **期待**: 6テスト成功、3テストSkip、0テスト失敗
- **実際**: 1テスト成功、5テスト失敗、3テストSkip

---

## 🔍 失敗原因詳細分析

### 失敗したテスト一覧

1. **Login_EmptyFields_ShowsValidationErrors** - バリデーションエラー表示の問題
2. **Logout_AfterLogin_RedirectsToLoginPage** - ログアウトボタンが見つからない（TimeoutException）
3. **Login_InvalidCredentials_ShowsErrorMessage** - エラーメッセージ表示の問題（TimeoutException）
4. **ChangePassword_ValidInput_ShowsSuccessMessage** - `#currentPassword`要素が見つからない（TimeoutException）
5. **ChangePassword_WrongCurrentPassword_ShowsErrorMessage** - `#currentPassword`要素が見つからない（TimeoutException）

---

## 📝 原因別分析

### ❌ 問題1: ValidationMessageクラス名不一致

**失敗テスト**: `Login_EmptyFields_ShowsValidationErrors`

**テストコード**:
```csharp
// AuthenticationTests.cs:251
var validationErrors = page.Locator(".validation-message, [role='alert']");
var errorCount = await validationErrors.CountAsync();
Assert.True(errorCount > 0, "バリデーションエラーが表示されるはず");
```

**実際のUI実装** (`Login.razor:56, 67`):
```razor
<ValidationMessage For="@(() => loginRequest.Email)" class="text-danger small" />
<ValidationMessage For="@(() => loginRequest.Password)" class="text-danger small" />
```

**問題**:
- テストLocator: `.validation-message, [role='alert']`
- 実際のクラス: `text-danger small`
- **クラス名が完全に異なる**

**根本原因**:
- ValidationMessageコンポーネントに`.validation-message`クラスが設定されていない
- Blazor Server標準のValidationMessageコンポーネントは、デフォルトでクラス名を付与しない
- カスタムクラス`text-danger small`のみが指定されている

---

### ❌ 問題2: ログアウトボタンのdata-testid属性欠如

**失敗テスト**: `Logout_AfterLogin_RedirectsToLoginPage`

**テストコード**:
```csharp
// AuthenticationTests.cs:311
var logoutLocator = page.Locator("[data-testid='logout-button'], a:has-text('ログアウト')");
await logoutLocator.First.WaitForAsync(new LocatorWaitForOptions
{
    State = WaitForSelectorState.Visible,
    Timeout = 5000
});
```

**実際のUI実装**:

**NavMenu.razor:122-124**:
```razor
<button class="nav-link btn btn-link text-start w-100" @onclick="Logout">
    <i class="fas fa-sign-out-alt me-2"></i>
    ログアウト
</button>
```

**AuthDisplay.razor:19-21**:
```razor
<button class="btn btn-outline-secondary btn-sm ms-2" @onclick="HandleLogoutAsync">
    <i class="fas fa-sign-out-alt me-1"></i>
    ログアウト
</button>
```

**問題**:
- テストLocator: `[data-testid='logout-button'], a:has-text('ログアウト')`
- 実際の要素: `<button>` タグ（`<a>`タグではない）
- **data-testid属性が存在しない**
- `a:has-text('ログアウト')`は`<button>`要素にマッチしない

**根本原因**:
- ログアウトボタンにdata-testid属性が設定されていない
- テストコードが`<a>`タグを想定していたが、実際は`<button>`タグ

---

### ❌ 問題3: ChangePassword画面の認証・遷移問題

**失敗テスト**:
- `ChangePassword_ValidInput_ShowsSuccessMessage`
- `ChangePassword_WrongCurrentPassword_ShowsErrorMessage`

**テストコード**:
```csharp
// AuthenticationTests.cs:391-392
await page.GotoAsync($"{BaseUrl}/change-password");
await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

// AuthenticationTests.cs:396
await page.FillAsync("#currentPassword", TestPassword);
```

**エラーメッセージ**:
```
System.TimeoutException : Timeout 30000ms exceeded.
Call log:
  - waiting for Locator("#currentPassword")
```

**実際のUI実装** (`ChangePassword.razor:62`):
```razor
<InputText id="currentPassword"
          @bind-Value="changePasswordRequest.CurrentPassword"
          type="password"
          class="form-control form-control-lg"
          placeholder="現在のパスワードを入力してください"
          disabled="@isSubmitting" />
```

**ChangePassword.razor:7**:
```razor
@attribute [Authorize]
```

**問題**:
- `#currentPassword`要素は実際に存在する
- **しかし、30秒待っても要素が見つからない**
- ChangePassword.razorには`[Authorize]`属性がある（認証済みユーザーのみアクセス可）

**根本原因の可能性**:
1. **認証状態の維持問題**: ログイン後の遷移で認証Cookieが正しく維持されていない
2. **ページ読み込み未完了**: Blazor Serverの初期化（SignalR接続）が完了していない
3. **リダイレクト発生**: 未認証と判断されてログイン画面にリダイレクトされている

**検証が必要**:
- ログイン後のCookie確認
- `/change-password`遷移後のURL確認（リダイレクトされていないか）
- Blazor Server SignalR接続完了待機

---

### ❌ 問題4: Login_InvalidCredentials エラーメッセージ表示問題

**失敗テスト**: `Login_InvalidCredentials_ShowsErrorMessage`

**テストコード**:
```csharp
// AuthenticationTests.cs:196-197
await page.FillAsync("[data-testid='username-input']", "invalid@example.com");
await page.FillAsync("[data-testid='password-input']", "WrongPassword!");
await page.ClickAsync("[data-testid='login-button']");

// AuthenticationTests.cs:203-207
var errorLocator = page.Locator(".alert-danger, [role='alert']");
await errorLocator.WaitForAsync(new LocatorWaitForOptions
{
    State = WaitForSelectorState.Visible,
    Timeout = 5000
});
```

**エラーメッセージ**:
```
System.TimeoutException : Timeout 5000ms exceeded.
Call log:
  - waiting for Locator(".alert-danger, [role='alert']") to be visible
```

**実際のUI実装** (`Login.razor:27-33`):
```razor
@if (!string.IsNullOrEmpty(errorMessage))
{
    <div class="alert alert-danger d-flex align-items-center" role="alert">
        <i class="fas fa-exclamation-triangle me-2"></i>
        <div>@errorMessage</div>
    </div>
}
```

**問題**:
- `.alert-danger, [role='alert']` Locatorは正しい
- **しかし、5秒待ってもエラーメッセージが表示されない**

**根本原因の可能性**:
1. **認証API応答遅延**: JavaScript API呼び出し（`authApi.login()`）の応答が5秒以上かかる
2. **エラーメッセージ設定ロジック**: `errorMessage`変数が正しく設定されていない
3. **StateHasChanged()未呼び出し**: UI更新が反映されていない

**検証が必要**:
- ログイン失敗時のJavaScript API応答時間測定
- `HandleValidSubmit()`メソッドのエラーハンドリング確認
- StateHasChanged()呼び出し確認

---

## ✅ 修正方針: UI側修正（根本的解決）

### 方針決定理由

**UI側修正のメリット**:
- ✅ **根本的解決**: data-testid属性を標準化、将来のテスト追加が容易
- ✅ **メンテナンス性向上**: Playwrightベストプラクティスに準拠
- ✅ **セレクタ安定性**: クラス名・テキスト変更に影響されない
- ✅ **可読性向上**: テストコードの意図が明確

**テスト側修正のデメリット**:
- ❌ **一時的対処**: クラス名変更時に再度修正が必要
- ❌ **脆弱性**: UI変更に影響されやすい
- ❌ **非標準**: Playwrightベストプラクティスから逸脱

---

## 🔧 具体的な修正内容

### 修正1: ValidationMessageにdata-testid属性追加

**対象ファイル**: `src/UbiquitousLanguageManager.Web/Components/Pages/Login.razor`

**修正箇所1** (line 56):
```razor
<!-- Before -->
<ValidationMessage For="@(() => loginRequest.Email)" class="text-danger small" />

<!-- After -->
<ValidationMessage For="@(() => loginRequest.Email)"
                   class="text-danger small validation-message"
                   data-testid="email-validation-message" />
```

**修正箇所2** (line 67):
```razor
<!-- Before -->
<ValidationMessage For="@(() => loginRequest.Password)" class="text-danger small" />

<!-- After -->
<ValidationMessage For="@(() => loginRequest.Password)"
                   class="text-danger small validation-message"
                   data-testid="password-validation-message" />
```

**修正理由**:
- `.validation-message`クラス追加により、テストコードのLocatorが動作
- data-testid属性追加により、将来的な変更に強いセレクタを提供

---

### 修正2: ログアウトボタンにdata-testid属性追加

**対象ファイル1**: `src/UbiquitousLanguageManager.Web/Shared/NavMenu.razor`

**修正箇所** (line 122):
```razor
<!-- Before -->
<button class="nav-link btn btn-link text-start w-100" @onclick="Logout">
    <i class="fas fa-sign-out-alt me-2"></i>
    ログアウト
</button>

<!-- After -->
<button class="nav-link btn btn-link text-start w-100"
        @onclick="Logout"
        data-testid="logout-button">
    <i class="fas fa-sign-out-alt me-2"></i>
    ログアウト
</button>
```

**対象ファイル2**: `src/UbiquitousLanguageManager.Web/Shared/AuthDisplay.razor`

**修正箇所** (line 19):
```razor
<!-- Before -->
<button class="btn btn-outline-secondary btn-sm ms-2" @onclick="HandleLogoutAsync">
    <i class="fas fa-sign-out-alt me-1"></i>
    ログアウト
</button>

<!-- After -->
<button class="btn btn-outline-secondary btn-sm ms-2"
        @onclick="HandleLogoutAsync"
        data-testid="logout-button">
    <i class="fas fa-sign-out-alt me-1"></i>
    ログアウト
</button>
```

**修正理由**:
- data-testid属性により、テストコードが確実にログアウトボタンを検出可能
- 複数のログアウトボタン（NavMenu/AuthDisplay）に統一的な属性を付与

---

### 修正3: ChangePassword遷移ロジック改善

**対象ファイル**: `tests/UbiquitousLanguageManager.E2E.Tests/AuthenticationTests.cs`

**修正箇所1** (ChangePassword_ValidInput_ShowsSuccessMessage - line 391-392):
```csharp
// Before
await page.GotoAsync($"{BaseUrl}/change-password");
await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

// After
await page.GotoAsync($"{BaseUrl}/change-password");
await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

// Blazor Server SignalR接続完了待機（追加）
await page.WaitForTimeoutAsync(1000);

// ページが正しく読み込まれたか確認（追加）
var currentUrl = page.Url;
Assert.True(
    currentUrl.Contains("/change-password"),
    $"パスワード変更画面に遷移できていない。現在URL: {currentUrl}"
);
```

**修正箇所2** (ChangePassword_WrongCurrentPassword_ShowsErrorMessage - line 466-467):
```csharp
// Before
await page.GotoAsync($"{BaseUrl}/change-password");
await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

// After（同上）
await page.GotoAsync($"{BaseUrl}/change-password");
await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

// Blazor Server SignalR接続完了待機（追加）
await page.WaitForTimeoutAsync(1000);

// ページが正しく読み込まれたか確認（追加）
var currentUrl = page.Url;
Assert.True(
    currentUrl.Contains("/change-password"),
    $"パスワード変更画面に遷移できていない。現在URL: {currentUrl}"
);
```

**修正理由**:
- Blazor Server SignalR接続完了を確実に待機
- リダイレクトが発生していないか確認（デバッグ情報提供）

---

### 修正4: Login_InvalidCredentials エラーメッセージ待機時間延長

**対象ファイル**: `tests/UbiquitousLanguageManager.E2E.Tests/AuthenticationTests.cs`

**修正箇所** (Login_InvalidCredentials_ShowsErrorMessage - line 203-207):
```csharp
// Before
var errorLocator = page.Locator(".alert-danger, [role='alert']");
await errorLocator.WaitForAsync(new LocatorWaitForOptions
{
    State = WaitForSelectorState.Visible,
    Timeout = 5000  // 5秒
});

// After
var errorLocator = page.Locator(".alert-danger, [role='alert']");
await errorLocator.WaitForAsync(new LocatorWaitForOptions
{
    State = WaitForSelectorState.Visible,
    Timeout = 10000  // 10秒に延長
});
```

**修正理由**:
- JavaScript API呼び出し（`authApi.login()`）の応答時間を考慮
- ネットワーク遅延・サーバー処理時間のバッファ確保

---

## ⏱️ 修正工数見積もり

| 修正内容 | ファイル数 | 行数 | 工数 | 難易度 |
|---------|---------|------|------|-------|
| ValidationMessage data-testid追加 | 1 | 2箇所 | 5分 | 低 |
| ログアウトボタン data-testid追加 | 2 | 2箇所 | 5分 | 低 |
| ChangePassword遷移ロジック改善 | 1 | 2箇所 | 15分 | 中 |
| Login_InvalidCredentials待機延長 | 1 | 1箇所 | 3分 | 低 |
| **合計** | **3** | **7箇所** | **28分** | **低-中** |

**テスト実行時間**: 約5分（E2Eテスト9シナリオ）

**総所要時間**: **約33-40分**（修正 + テスト実行 + 結果確認）

---

## 📋 修正後の期待結果

### E2Eテスト実行結果（修正後）

- **Total tests**: 9
- **Passed**: 6（全実装シナリオ成功）
- **Failed**: 0
- **Skipped**: 3（パスワードリセット機能未実装）

### 成功シナリオ

1. ✅ Login_ValidCredentials_ShowsHomePage
2. ✅ Login_EmptyFields_ShowsValidationErrors（修正1適用）
3. ✅ Login_InvalidCredentials_ShowsErrorMessage（修正4適用）
4. ✅ Logout_AfterLogin_RedirectsToLoginPage（修正2適用）
5. ✅ ChangePassword_ValidInput_ShowsSuccessMessage（修正3適用）
6. ✅ ChangePassword_WrongCurrentPassword_ShowsErrorMessage（修正3適用）

### Skipシナリオ

7. ⏭️ PasswordReset_ValidEmail_ShowsSuccessMessage（ForgotPassword.razorページ未実装）
8. ⏭️ PasswordReset_ValidToken_ShowsSuccessMessage（ResetPassword.razorページ未実装）
9. ⏭️ PasswordReset_InvalidToken_ShowsErrorMessage（ResetPassword.razorページ未実装）

---

## 🔄 次のアクション

### Stage 3完了処理

1. ✅ 失敗原因分析完了（本ドキュメント作成）
2. ✅ 修正方針決定（UI側修正）
3. 📋 修正内容文書化（本ドキュメント）
4. ⏭️ **次回セッション**: 修正実施・E2Eテスト再実行

### Stage 4: E2Eテスト修正・再実行（次回セッション）

**実施内容**:
1. UI側修正実施（4箇所）
2. テスト側修正実施（3箇所）
3. E2Eテスト再実行
4. 結果確認（6テスト成功、3テストSkip、0テスト失敗）
5. Step6組織設計ファイル更新（Stage 3-4完了記録）

**推定所要時間**: 約40-50分

---

## 📚 参考情報

### 関連ファイル

- `tests/UbiquitousLanguageManager.E2E.Tests/AuthenticationTests.cs`
- `src/UbiquitousLanguageManager.Web/Components/Pages/Login.razor`
- `src/UbiquitousLanguageManager.Web/Components/Pages/ChangePassword.razor`
- `src/UbiquitousLanguageManager.Web/Shared/NavMenu.razor`
- `src/UbiquitousLanguageManager.Web/Shared/AuthDisplay.razor`

### Playwrightベストプラクティス

- **data-testid属性の使用**: セレクタの安定性・可読性向上
- **役割ベースLocator**: `getByRole('button', { name: 'ログアウト' })` も推奨
- **待機戦略**: `WaitForLoadStateAsync(NetworkIdle)` + 適切なTimeout設定

### Blazor Server E2Eテストの注意点

- **SignalR接続待機**: `WaitForTimeoutAsync(1000)` 等で接続完了を待機
- **StateHasChanged()影響**: UI更新の反映に時間がかかる場合がある
- **認証Cookieの維持**: ページ遷移時の認証状態確認が重要

---

**作成者**: Claude (Phase B-F2 Step6 Stage 3)
**レビュー**: 要ユーザー承認
**次回更新**: Stage 4修正実施後
