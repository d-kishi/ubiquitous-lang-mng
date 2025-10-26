# Playwright MCPツール活用パターン

## 概要

Playwright MCPが提供する25種類のブラウザ操作ツールの使い分けガイド。Phase B2 Step6で実証した効率的なツール選択パターン。

## 主要MCPツール（E2Eテスト頻出）

### 1. playwright_navigate

**用途**: URL遷移・ページ読み込み

**使用例**:
```plaintext
playwright_navigate url="https://localhost:5001"
```

**C# Playwrightコード**:
```csharp
await page.GotoAsync("https://localhost:5001");
await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
```

**適用場面**:
- E2Eテスト開始時のログイン画面アクセス
- 画面遷移確認
- URLベースのナビゲーション

---

### 2. playwright_snapshot

**用途**: アクセシビリティツリー取得（構造化データ・高速・正確）

**革新性**:
- ✅ **スクリーンショットより高速**（画像処理不要）
- ✅ **構造化データ**（LLM最適化）
- ✅ **決定論的**（同じ構造=同じ結果）
- ✅ **Vision API不要**（低コスト）

**使用例**:
```plaintext
playwright_snapshot
```

**適用場面**:
- ページ構造確認
- 要素存在確認
- デバッグ・トラブルシューティング

---

### 3. playwright_click

**用途**: ボタン・リンククリック

**使用例**:
```plaintext
playwright_click selector="[data-testid='member-add-button']"
```

**C# Playwrightコード**:
```csharp
await page.ClickAsync("[data-testid='member-add-button']");

// 複雑なセレクタ
var memberLink = page.Locator("[data-testid='member-management-link']").First;
await memberLink.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
await memberLink.ClickAsync();
```

**適用場面**:
- ボタンクリック
- リンククリック
- フォーム送信

---

### 4. playwright_fill

**用途**: フォーム入力

**使用例**:
```plaintext
playwright_fill selector="[data-testid='username-input']" value="e2e-test@ubiquitous-lang.local"
```

**C# Playwrightコード**:
```csharp
await page.FillAsync("[data-testid='username-input']", "e2e-test@ubiquitous-lang.local");
await page.FillAsync("[data-testid='password-input']", "E2ETest#2025!Secure");
```

**適用場面**:
- ログインフォーム入力
- テキストボックス入力
- パスワード入力

---

### 5. playwright_select

**用途**: ドロップダウン選択

**使用例**:
```plaintext
playwright_select selector="[data-testid='member-selector']" option="ユーザー名"
```

**C# Playwrightコード**:
```csharp
var memberSelector = page.Locator("[data-testid='member-selector']");
await memberSelector.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
await memberSelector.SelectOptionAsync(new SelectOptionValue { Index = 1 });
// または
await memberSelector.SelectOptionAsync(new SelectOptionValue { Label = "ユーザー名" });
```

**適用場面**:
- セレクトボックス選択
- ドロップダウンメニュー操作

---

### 6. playwright_wait_for

**用途**: 要素表示待機・時間待機

**使用例（要素待機）**:
```plaintext
playwright_wait_for selector="[data-testid='member-list']" state="visible" timeout=5000
```

**使用例（時間待機）**:
```plaintext
playwright_wait_for time=1000
```

**C# Playwrightコード**:
```csharp
// 要素表示待機
var memberList = page.Locator("[data-testid='member-list']");
await memberList.WaitForAsync(new LocatorWaitForOptions
{
    State = WaitForSelectorState.Visible,
    Timeout = 5000
});

// 時間待機（SignalR更新待機）
await page.WaitForTimeoutAsync(1000);
```

**適用場面**:
- Blazor Server SignalR接続確立待機
- 非同期UI更新待機（StateHasChanged()）
- Toast通知表示待機

---

## 使い分けパターン

### パターン1: ログイン操作

**MCPツール使用順序**:
1. `playwright_navigate` - ログイン画面アクセス
2. `playwright_fill` (×2) - ユーザー名・パスワード入力
3. `playwright_click` - ログインボタンクリック
4. `playwright_wait_for` - SignalR接続確立待機

**C# Playwrightコード**:
```csharp
await page.GotoAsync("https://localhost:5001");
await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
await page.FillAsync("[data-testid='username-input']", TestEmail);
await page.FillAsync("[data-testid='password-input']", TestPassword);
await page.ClickAsync("[data-testid='login-button']");
await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
```

---

### パターン2: メンバー追加操作

**MCPツール使用順序**:
1. `playwright_click` - メンバー管理リンククリック
2. `playwright_wait_for` - URL遷移確認
3. `playwright_select` - ユーザー選択
4. `playwright_click` - 追加ボタンクリック
5. `playwright_wait_for` - Toast通知表示待機
6. `playwright_snapshot` - 成功メッセージ検証

**C# Playwrightコード**:
```csharp
var memberLink = page.Locator("[data-testid='member-management-link']").First;
await memberLink.ClickAsync();
await page.WaitForURLAsync("**/projects/*/members");

var memberSelector = page.Locator("[data-testid='member-selector']");
await memberSelector.SelectOptionAsync(new SelectOptionValue { Index = 1 });
await page.ClickAsync("[data-testid='member-add-button']");

var toastLocator = page.Locator(".toast-success, [role='alert']");
await toastLocator.WaitForAsync(new LocatorWaitForOptions { Timeout = 5000 });
var toastText = await toastLocator.TextContentAsync();
Assert.Contains("プロジェクトに追加しました", toastText);
```

---

### パターン3: エラーハンドリング検証

**MCPツール使用順序**:
1. `playwright_select` - 既存メンバー選択
2. `playwright_click` - 追加ボタンクリック
3. `playwright_wait_for` - エラーメッセージ表示待機
4. `playwright_snapshot` - エラー内容検証

**C# Playwrightコード**:
```csharp
var memberSelector = page.Locator("[data-testid='member-selector']");
await memberSelector.SelectOptionAsync(new SelectOptionValue { Label = memberName });
await page.ClickAsync("[data-testid='member-add-button']");

var errorLocator = page.Locator("[data-testid='member-error-message']");
await errorLocator.WaitForAsync(new LocatorWaitForOptions { Timeout = 5000 });
var errorText = await errorLocator.TextContentAsync();
Assert.Contains("既にこのプロジェクトのメンバーです", errorText);
```

---

## その他MCPツール（参考）

### playwright_console_messages

**用途**: コンソールログ・エラーメッセージ取得

**適用場面**:
- JavaScriptエラー検出
- デバッグログ確認

---

### playwright_evaluate

**用途**: JavaScript実行

**適用場面**:
- 複雑なDOM操作
- カスタム検証ロジック実行

---

### playwright_hover

**用途**: マウスホバー

**適用場面**:
- ツールチップ表示確認
- ホバーメニュー操作

---

### playwright_upload_file

**用途**: ファイルアップロード

**適用場面**:
- ファイル選択ダイアログ操作

---

## Phase B2 Step6実装実績

### UserProjectsTests.cs で使用したMCPツール

1. **playwright_navigate** - ログイン画面アクセス
2. **playwright_fill** - ユーザー名・パスワード入力
3. **playwright_click** - ボタンクリック（ログイン・メンバー追加・削除）
4. **playwright_select** - ユーザー選択ドロップダウン操作
5. **playwright_wait_for** - 要素表示待機・SignalR更新待機

### 使用頻度
- **playwright_click**: 最頻出（ボタン・リンククリック）
- **playwright_wait_for**: 次点（Blazor Server非同期処理待機）
- **playwright_fill**: ログイン時必須
- **playwright_select**: ドロップダウン操作時必須

---

## アクセシビリティツリーの革新性

### 従来のスクリーンショットベース

❌ **問題点**:
- 遅い（画像処理時間）
- 不正確（誤認識リスク）
- コスト高（Vision API使用）
- 非決定論的（画像の微妙な差異で結果変動）

### Playwright MCPのアクセシビリティツリー

✅ **革新性**:
- **高速**: APIアクセスのみ（画像処理不要）
- **正確**: 構造化データ（誤認識ゼロ）
- **低コスト**: Vision API不要
- **決定論的**: 同じ構造=同じ結果（再現性100%）
- **LLM最適化**: テキストデータ（Claude最適）

---

**作成日**: 2025-10-26
**Phase**: Phase B2 Step6
**実装実績**: UserProjectsTests.cs（3シナリオ）
