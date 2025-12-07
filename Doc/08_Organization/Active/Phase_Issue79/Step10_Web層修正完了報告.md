# Step 10 Web層修正完了報告（Agent 2: Admin/Users系 + Service）

**作成日**: 2025-12-08
**担当Agent**: csharp-web-ui
**対象**: Issue #79 根本修正（UserId型string化）

---

## 修正対象ファイル（4ファイル）

### 1. Index.razor (Users) - 4箇所修正

**ファイルパス**: `src/UbiquitousLanguageManager.Web/Components/Pages/Admin/Users/Index.razor`

#### 修正1: UserId初期化（行280-283）

```csharp
// Before
private UserId currentUserId = UserId.NewUserId(0);

// After
// 【Issue #79 根本修正】UserId型はstring型に変更されました
private UserId currentUserId = UserId.NewUserId("0");
```

**根拠**: F# Domain層のUserId型が`UserId of string`に変更されたため、long型ではなくstring型で初期化

---

#### 修正2: UserID取得・TryParse削除（行342-347）

```csharp
// Before
var userIdString = user.FindFirst("UserId")?.Value ?? "0";
var userId = long.TryParse(userIdString, out var id) ? UserId.NewUserId(id) : UserId.NewUserId(0);

// After
// 【Issue #79 根本修正】UserId型はstring型に変更されたため、TryParseは不要
var userIdString = user.FindFirst("UserId")?.Value ?? "0";
var userId = UserId.NewUserId(userIdString);
```

**根拠**: UserId型がstring型のため、long.TryParseによる変換は不要（余計な処理の削除）

---

#### 修正3: Guid.TryParse削除（行358-373）

```csharp
// Before
var userGuid = Guid.TryParse(operatorIdentityId, out var guid) ? guid : Guid.Empty;
var query = new GetProjectsQuery(
    userGuid,       // UserId: Guid
    currentRole,
    1, 100, false,
    Microsoft.FSharp.Core.FSharpOption<string>.None
);

// After
// 【Issue #79 根本修正】ASP.NET Core Identity IDはstring型のまま使用
// Guid変換は不要（GetProjectsQueryがstring型Identity IDを受け取るため）
var operatorUserId = operatorIdentityId ?? string.Empty;
var query = new GetProjectsQuery(
    operatorUserId, // UserId: string（ASP.NET Core Identity ID）
    currentRole,
    1, 100, false,
    Microsoft.FSharp.Core.FSharpOption<string>.None
);
```

**根拠**: GetProjectsQueryの第1引数がstring型Identity IDを受け取るため、Guid変換は不要

---

#### 修正4: int → string代入（行399）

```csharp
// Before
UpdatedBy = 0,  // 一覧画面では不要

// After
UpdatedBy = "0",  // 【Issue #79 根本修正】string型に変更
```

**根拠**: ProjectDto.UpdatedByがstring型に変更されたため

---

### 2. Create.razor (Users) - 1箇所修正

**ファイルパス**: `src/UbiquitousLanguageManager.Web/Components/Pages/Admin/Users/Create.razor`

#### 修正1: Guid.TryParse削除（行470-495）

```csharp
// Before
var userGuid = Guid.TryParse(operatorIdentityId, out var guid) ? guid : Guid.Empty;
var query = new GetProjectsQuery(
    userGuid,       // UserId: Guid
    userRole,
    1, 100, false,
    Microsoft.FSharp.Core.FSharpOption<string>.None
);

// After
// 【Issue #79 根本修正】ASP.NET Core Identity IDはstring型のまま使用
// Guid変換は不要（GetProjectsQueryがstring型Identity IDを受け取るため）
var operatorUserId = operatorIdentityId ?? string.Empty;
var query = new GetProjectsQuery(
    operatorUserId, // UserId: string（ASP.NET Core Identity ID）
    userRole,
    1, 100, false,
    Microsoft.FSharp.Core.FSharpOption<string>.None
);
```

**根拠**: GetProjectsQueryの第1引数がstring型Identity IDを受け取るため、Guid変換は不要

---

### 3. Edit.razor (Users) - 2箇所修正

**ファイルパス**: `src/UbiquitousLanguageManager.Web/Components/Pages/Admin/Users/Edit.razor`

#### 修正1: Guid.TryParse削除（行598-623）

```csharp
// Before
var userGuid = Guid.TryParse(identityIdClaim.Value, out var guid) ? guid : Guid.Empty;
var query = new GetProjectsQuery(
    userGuid,       // UserId: Guid
    userRole,
    1, 100, false,
    Microsoft.FSharp.Core.FSharpOption<string>.None
);

// After
// 【Issue #79 根本修正】ASP.NET Core Identity IDはstring型のまま使用
// Guid変換は不要（GetProjectsQueryがstring型Identity IDを受け取るため）
var operatorUserId = identityIdClaim.Value ?? string.Empty;
var query = new GetProjectsQuery(
    operatorUserId, // UserId: string（ASP.NET Core Identity ID）
    userRole,
    1, 100, false,
    Microsoft.FSharp.Core.FSharpOption<string>.None
);
```

**根拠**: GetProjectsQueryの第1引数がstring型Identity IDを受け取るため、Guid変換は不要

---

#### 修正2: long.TryParse削除（行803-812）

```csharp
// Before
if (!long.TryParse(model.UserId, out var userId))
{
    passwordResetError = "ユーザーIDが無効です";
    isResettingPassword = false;
    StateHasChanged();
    return;
}
var targetId = UserId.NewUserId(userId);

// After
// 【Issue #79 根本修正】UserId型はstring型に変更されたため、TryParseは不要
if (string.IsNullOrEmpty(model.UserId))
{
    passwordResetError = "ユーザーIDが無効です";
    isResettingPassword = false;
    StateHasChanged();
    return;
}
var targetId = UserId.NewUserId(model.UserId);
```

**根拠**: UserId型がstring型のため、long.TryParseによる変換は不要（余計な処理の削除）

---

### 4. BlazorAuthenticationService.cs - 1箇所修正

**ファイルパス**: `src/UbiquitousLanguageManager.Web/Services/BlazorAuthenticationService.cs`

#### 修正1: UserId.NewUserId引数修正（行139-140）

```csharp
// Before
var userId = UserId.NewUserId(domainUserId.Value);

// After
// 【Issue #79 根本修正】UserId型はstring型に変更されたため、ToString()で変換
var userId = UserId.NewUserId(domainUserId.Value.ToString());
```

**根拠**: UserId型がstring型のため、long?型のValueをToString()で変換

---

## 修正パターンサマリ

### パターンA: Guid.TryParse削除（3箇所）

- **Index.razor (Users)** 行357
- **Create.razor (Users)** 行471
- **Edit.razor (Users)** 行599

**修正内容**: ASP.NET Core Identity IDをGuidに変換していた処理を削除し、string型のまま使用

```csharp
// Before
var userGuid = Guid.TryParse(operatorIdentityId, out var guid) ? guid : Guid.Empty;

// After
var operatorUserId = operatorIdentityId ?? string.Empty;
```

---

### パターンB: long.TryParse削除（2箇所）

- **Index.razor (Users)** 行343
- **Edit.razor (Users)** 行802

**修正内容**: UserIdをlongに変換していた処理を削除し、string型のまま使用

```csharp
// Before
var userId = long.TryParse(userIdString, out var id) ? UserId.NewUserId(id) : UserId.NewUserId(0);

// After
var userId = UserId.NewUserId(userIdString);
```

---

### パターンC: int → string代入（1箇所）

- **Index.razor (Users)** 行399

**修正内容**: DTO代入時にint型からstring型に変更

```csharp
// Before
UpdatedBy = 0

// After
UpdatedBy = "0"
```

---

### パターンD: ToString()変換追加（1箇所）

- **BlazorAuthenticationService.cs** 行139

**修正内容**: long?型をstring型に変換してUserId.NewUserIdに渡す

```csharp
// Before
var userId = UserId.NewUserId(domainUserId.Value);

// After
var userId = UserId.NewUserId(domainUserId.Value.ToString());
```

---

## 完了基準チェックリスト

- [x] 4ファイル全ての修正完了
  - [x] Index.razor (Users) - 4箇所
  - [x] Create.razor (Users) - 1箇所
  - [x] Edit.razor (Users) - 2箇所
  - [x] BlazorAuthenticationService.cs - 1箇所
- [x] Guid.TryParse 3箇所削除完了
- [x] long.TryParse 2箇所削除完了
- [x] UserId.NewUserId引数をstring型に統一
- [x] 初学者向けコメント追加（Issue #79根本修正の説明）
- [ ] 各ファイルのビルドエラー解消（ビルド確認必要）

---

## 波及効果・注意事項

### 1. GetCurrentDomainUserId()の戻り値型

**現状**: `long?`型
**問題**: UserId型がstring型に変更されたため、BlazorAuthenticationService.csでToString()変換が必要

**推奨対応**（別Issue）:
```csharp
// 現在
public long? GetCurrentDomainUserId() { ... }

// 推奨
public string? GetCurrentDomainUserId() { ... }
```

この変更は`CustomAuthenticationStateProvider.cs`の修正が必要なため、別Issueで対応推奨

---

### 2. 削除された「余計な処理」一覧

Issue #79の本質に従い、以下の「余計な処理」を削除しました：

1. **Guid.TryParse**: Identity IDをGuidに変換（3箇所削除）
2. **long.TryParse**: UserIdをlongに変換（2箇所削除）
3. **Guid.Empty代入**: Guid変換失敗時のフォールバック（3箇所削除）

これらは全て、「AspNetUsers.Id（string/GUID文字列）をアプリケーション全体でstringのまま扱う」設計に反する処理でした。

---

## 次のステップ

1. **ビルド確認**: DevContainer環境で`dotnet build`実行
2. **エラー修正**: 残存エラーがあれば対応
3. **他Agent担当ファイルの修正状況確認**:
   - Agent 1: ProjectMembers/ProjectList/ProjectCreate/ProjectEdit（5ファイル）
   - Step 10全体の完了確認

---

**作成完了**: 2025-12-08
**修正方針**: Issue #79根本修正アプローチ準拠
