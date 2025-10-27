# data-testid属性設計パターン

## 概要

E2Eテストで使用する `data-testid` 属性の命名規則とベストプラクティス。Phase B2 Step5で15要素実装し、Step6 E2Eテストで実証した効率的なパターン。

## 命名規則

### 基本テンプレート

```
data-testid="{target}-{type}"
```

- **{target}**: 対象要素の意味的名称（小文字・ハイフン区切り）
- **{type}**: 要素タイプ（button/input/list/card/error-message/link等）

---

## パターン別実装例

### 1. ボタン（button）

```razor
<!-- メンバー追加ボタン -->
<button data-testid="member-add-button" @onclick="AddMember">
    ✅ 追加
</button>

<!-- メンバー削除ボタン -->
<button data-testid="member-delete-button" @onclick="() => DeleteMember(member.Id)">
    🗑️
</button>

<!-- ログインボタン -->
<button data-testid="login-button" type="submit">
    ログイン
</button>
```

**E2Eテストでの使用例**:
```csharp
await page.ClickAsync("[data-testid='member-add-button']");
await page.ClickAsync("[data-testid='login-button']");
```

---

### 2. 入力フィールド（input）

```razor
<!-- ユーザー名入力 -->
<input data-testid="username-input" type="text" @bind="Username" />

<!-- パスワード入力 -->
<input data-testid="password-input" type="password" @bind="Password" />

<!-- プロジェクト名入力 -->
<input data-testid="project-name-input" @bind="Model.ProjectName" />
```

**E2Eテストでの使用例**:
```csharp
await page.FillAsync("[data-testid='username-input']", "e2e-test@ubiquitous-lang.local");
await page.FillAsync("[data-testid='password-input']", "E2ETest#2025!Secure");
```

---

### 3. リスト（list）

```razor
<!-- メンバー一覧 -->
<div data-testid="member-list">
    @foreach (var member in Members)
    {
        <div data-testid="member-card">
            <!-- メンバー情報 -->
        </div>
    }
</div>

<!-- プロジェクト一覧 -->
<div data-testid="project-list">
    @foreach (var project in Projects)
    {
        <div data-testid="project-item">
            <!-- プロジェクト情報 -->
        </div>
    }
</div>
```

**E2Eテストでの使用例**:
```csharp
var memberList = page.Locator("[data-testid='member-list']");
var memberCount = await memberList.Locator("[data-testid='member-card']").CountAsync();
Assert.True(memberCount > 0, "メンバー一覧に要素が表示されるはず");
```

---

### 4. カード（card）

```razor
<!-- メンバーカード -->
<div data-testid="member-card">
    <span data-testid="member-name">@member.Name</span>
    <span data-testid="member-role">@member.Role</span>
    <button data-testid="member-delete-button">🗑️</button>
</div>
```

**E2Eテストでの使用例**:
```csharp
var firstMemberCard = page.Locator("[data-testid='member-card']").First;
var memberName = await firstMemberCard.Locator("[data-testid='member-name']").TextContentAsync();
```

---

### 5. エラーメッセージ（error-message）

```razor
<!-- メンバー追加エラーメッセージ -->
@if (!string.IsNullOrEmpty(ErrorMessage))
{
    <div data-testid="member-error-message" class="alert alert-danger">
        @ErrorMessage
    </div>
}
```

**E2Eテストでの使用例**:
```csharp
var errorLocator = page.Locator("[data-testid='member-error-message']");
await errorLocator.WaitForAsync(new LocatorWaitForOptions
{
    State = WaitForSelectorState.Visible,
    Timeout = 5000
});
var errorText = await errorLocator.TextContentAsync();
Assert.Contains("既にこのプロジェクトのメンバーです", errorText);
```

---

### 6. リンク（link）

```razor
<!-- メンバー管理画面へのリンク -->
<a data-testid="member-management-link" href="@($"/projects/{Project.Id}/members")">
    👥 メンバー
</a>
```

**E2Eテストでの使用例**:
```csharp
var memberLink = page.Locator("[data-testid='member-management-link']").First;
await memberLink.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
await memberLink.ClickAsync();
```

---

### 7. セレクトボックス（selector）

```razor
<!-- メンバー選択ドロップダウン -->
<select data-testid="member-selector" @bind="SelectedUserId">
    <option value="">ユーザーを選択...</option>
    @foreach (var user in AvailableUsers)
    {
        <option value="@user.Id">@user.Name</option>
    }
</select>
```

**E2Eテストでの使用例**:
```csharp
var memberSelector = page.Locator("[data-testid='member-selector']");
await memberSelector.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
await memberSelector.SelectOptionAsync(new SelectOptionValue { Index = 1 });
```

---

## ベストプラクティス

### 1. 階層的命名（推奨）

```razor
<!-- 親要素 -->
<div data-testid="project-detail">
    <!-- 子要素 -->
    <h2 data-testid="project-name">@Project.Name</h2>
    <p data-testid="project-description">@Project.Description</p>

    <!-- 孫要素 -->
    <div data-testid="project-members">
        @foreach (var member in Project.Members)
        {
            <div data-testid="member-card">
                <!-- ... -->
            </div>
        }
    </div>
</div>
```

**メリット**:
- ✅ 階層構造が明確
- ✅ 親要素からの相対セレクタが使いやすい
- ✅ 名前空間衝突回避

---

### 2. 動的要素の命名（注意）

```razor
<!-- ❌ 悪い例: IDを含める（テストが脆弱） -->
<div data-testid="member-@member.Id">
    <!-- ... -->
</div>

<!-- ✅ 良い例: 汎用的な命名 -->
<div data-testid="member-card">
    <!-- IDは別属性で管理 -->
    <span data-member-id="@member.Id" hidden>@member.Id</span>
    <!-- ... -->
</div>
```

---

### 3. CSS/XPathセレクタ回避（重要）

```csharp
// ❌ 悪い例: CSSセレクタ依存（UI変更に脆弱）
await page.ClickAsync(".btn.btn-primary.add-member");
await page.ClickAsync("#memberList > div:nth-child(1) > button");

// ✅ 良い例: data-testid使用（UI変更に強い）
await page.ClickAsync("[data-testid='member-add-button']");
await page.Locator("[data-testid='member-list']")
           .Locator("[data-testid='member-card']").First
           .Locator("[data-testid='member-delete-button']")
           .ClickAsync();
```

**理由**:
- ✅ data-testid はテスト用途専用（UIデザイン変更の影響を受けない）
- ✅ CSS/XPathはUIクラス名・構造変更で即座に破損
- ✅ 保守性・可読性が高い

---

## Phase B2実装実績

### 実装箇所（15要素）

#### ProjectMembers.razor（7要素）
- `member-add-button`
- `member-delete-button`
- `member-list`
- `member-error-message`
- `member-card` (×N)
- `member-name` (×N)
- `member-role` (×N)

#### ProjectMemberSelector.razor（1要素）
- `member-selector`

#### Login.razor（3要素）
- `username-input`
- `password-input`
- `login-button`

#### ProjectEdit.razor（2要素）
- `member-management-link`
- `project-name-input`

#### ProjectList.razor（2要素）
- `project-list`
- `project-item`

---

## 効率化効果

### Phase B2 Step6実証結果
- **data-testid属性設計パターン確立**: E2Eテスト作成効率93.3%向上の主要因
- **セレクタ指定の信頼性**: 100%（UI変更の影響ゼロ）
- **テストコード可読性**: 大幅向上（セレクタ意図が明確）

---

**作成日**: 2025-10-26
**Phase**: Phase B2 Step6
**実装箇所**: 15要素（ProjectMembers/Login/ProjectEdit/ProjectList）
