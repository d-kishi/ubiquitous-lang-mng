# Step 10 Web層修正対象一覧

**作成日**: 2025-12-07
**作成目的**: Step 9.5事前調査に基づく、Step 10修正対象の網羅的リスト

---

## 調査結果サマリ

| 項目 | 数値 |
|------|------|
| **ビルドエラー数** | 22件 |
| **修正対象ファイル数** | 8ファイル |
| **修正パターン数** | 3パターン |

---

## 修正パターン分類

### パターンA: Guid → string変換（12箇所）

Command/Query引数で`Guid`型を`string`型に変換する必要がある箇所。

```csharp
// 現在
new SomeCommand(..., userId: currentUserId, ...)  // currentUserId: Guid型
// 修正後
new SomeCommand(..., userId: currentUserId.ToString(), ...)
```

### パターンB: long → string変換（3箇所）

`UserId.NewUserId()`呼び出しで`long`型を`string`型に変換する必要がある箇所。

```csharp
// 現在
UserId.NewUserId(longValue)
// 修正後
UserId.NewUserId(longValue.ToString())
```

### パターンC: int → string変換（3箇所）

DTO代入時に`int`型を`string`型に変換する必要がある箇所。

```csharp
// 現在
UpdatedBy = 0
// 修正後
UpdatedBy = "0"  // または適切なstring値
```

### パターンD: 根本的な型変更が必要（4箇所）

`Guid`型変数をそのまま使用しているため、変数宣言自体を`string`型に変更する必要がある箇所。

---

## 修正対象ファイル詳細

### 1. BlazorAuthenticationService.cs（1箇所）

**パス**: `src/UbiquitousLanguageManager.Web/Services/BlazorAuthenticationService.cs`

| 行番号 | エラー | 現在のコード | 修正方法 | パターン |
|--------|--------|-------------|---------|---------|
| 139 | CS1503: long → string | `UserId.NewUserId(domainUserId.Value)` | `UserId.NewUserId(domainUserId.Value.ToString())` | B |

**関連コード確認（行73）**:
```csharp
public long? GetCurrentDomainUserId()  // 戻り値型がlong?
```
→ この戻り値型の変更も検討が必要

---

### 2. ProjectMembers.razor（5箇所）

**パス**: `src/UbiquitousLanguageManager.Web/Components/Projects/ProjectMembers.razor`

| 行番号 | エラー | 現在のコード | 修正方法 | パターン |
|--------|--------|-------------|---------|---------|
| 284 | CS1503: Guid → string | `userId: currentUserId` | `userId: currentUserId.ToString()` | A |
| 376 | CS1503: Guid → string | `userId: selectedUserId.Value` | `userId: selectedUserId.Value.ToString()` | A |
| 377 | CS1503: Guid → string | `operatorUserId: currentUserId` | `operatorUserId: currentUserId.ToString()` | A |
| 448 | CS1503: Guid → string | `userId: userGuid` | `userId: userGuid.ToString()` | A |
| 449 | CS1503: Guid → string | `operatorUserId: currentUserId` | `operatorUserId: currentUserId.ToString()` | A |

**関連変数宣言（要確認）**:
- 行155: `private Guid? selectedUserId = null;`
- 行184: `private Guid currentUserId;`
- 行220: `Guid.TryParse(userIdClaim.Value, out var userId)`

**波及効果分析**:
- `currentUserId`を`string`型に変更する場合、Guid.TryParseのロジック変更が必要
- 代替案: 変数はGuid型のまま、使用時に`.ToString()`で変換

---

### 3. ProjectList.razor（3箇所）

**パス**: `src/UbiquitousLanguageManager.Web/Components/Pages/ProjectManagement/ProjectList.razor`

| 行番号 | エラー | 現在のコード | 修正方法 | パターン |
|--------|--------|-------------|---------|---------|
| 337 | CS1503: Guid → string | `userId: currentUserId` | `userId: currentUserId.ToString()` | A |
| 375 | CS0029: int → string | `UpdatedBy = 0` | `UpdatedBy = "0"` または型確認 | C |
| 567 | CS1503: Guid → string | `operatorUserId: currentUserId` | `operatorUserId: currentUserId.ToString()` | A |

**関連変数宣言**:
- 行280: `private Guid currentUserId;`
- 行297: `Guid.TryParse(userIdClaim, out var userId)`

---

### 4. Index.razor - Users（5箇所）

**パス**: `src/UbiquitousLanguageManager.Web/Components/Pages/Admin/Users/Index.razor`

| 行番号 | エラー | 現在のコード | 修正方法 | パターン |
|--------|--------|-------------|---------|---------|
| 280 | CS1503: int → string | `UserId.NewUserId(id)` | `UserId.NewUserId(id.ToString())` | B |
| 343 | CS1503: long → string | `UserId.NewUserId(id)` | `UserId.NewUserId(id.ToString())` | B |
| 343 | CS1503: int → string | 同上 | 同上 | B |
| 364 | CS1503: Guid → string | `userGuid` | `userGuid.ToString()` | A |
| 396 | CS0029: int → string | 代入式 | 型確認後修正 | C |

**関連コード（行343）**:
```csharp
var userId = long.TryParse(userIdString, out var id) ? UserId.NewUserId(id) : UserId.NewUserId(0);
```
→ `UserId.NewUserId(id.ToString())` に変更必要

**関連コード（行357）**:
```csharp
var userGuid = Guid.TryParse(operatorIdentityId, out var guid) ? guid : Guid.Empty;
```
→ このGuid変換自体が不要になる可能性

---

### 5. ProjectCreate.razor（2箇所）

**パス**: `src/UbiquitousLanguageManager.Web/Components/Pages/ProjectManagement/ProjectCreate.razor`

| 行番号 | エラー | 現在のコード | 修正方法 | パターン |
|--------|--------|-------------|---------|---------|
| 215 | CS1503: Guid → string | `ownerId: currentUserId` | `ownerId: currentUserId.ToString()` | A |
| 216 | CS1503: Guid → string | `operatorUserId: currentUserId` | `operatorUserId: currentUserId.ToString()` | A |

**関連変数宣言**:
- 行172: `private Guid currentUserId;`
- 行185: `Guid.TryParse(userIdClaim, out var userId)`

---

### 6. ProjectEdit.razor（3箇所）

**パス**: `src/UbiquitousLanguageManager.Web/Components/Pages/ProjectManagement/ProjectEdit.razor`

| 行番号 | エラー | 現在のコード | 修正方法 | パターン |
|--------|--------|-------------|---------|---------|
| 314 | CS1503: Guid → string | `userId: currentUserId` | `userId: currentUserId.ToString()` | A |
| 344 | CS0029: int → string | `UpdatedBy = 0` | `UpdatedBy = "0"` または型確認 | C |
| 397 | CS1503: Guid → string | `operatorUserId: currentUserId` | `operatorUserId: currentUserId.ToString()` | A |

**関連変数宣言**:
- 行241: `private Guid currentUserId;`
- 行275: `Guid.TryParse(userIdClaim, out var userId)`

---

### 7. Create.razor - Users（1箇所）

**パス**: `src/UbiquitousLanguageManager.Web/Components/Pages/Admin/Users/Create.razor`

| 行番号 | エラー | 現在のコード | 修正方法 | パターン |
|--------|--------|-------------|---------|---------|
| 488 | CS1503: Guid → string | `userGuid` | `userGuid.ToString()` | A |

**関連コード（行471）**:
```csharp
var userGuid = Guid.TryParse(operatorIdentityId, out var guid) ? guid : Guid.Empty;
```

---

### 8. Edit.razor - Users（2箇所）

**パス**: `src/UbiquitousLanguageManager.Web/Components/Pages/Admin/Users/Edit.razor`

| 行番号 | エラー | 現在のコード | 修正方法 | パターン |
|--------|--------|-------------|---------|---------|
| 616 | CS1503: Guid → string | `userGuid` | `userGuid.ToString()` | A |
| 810 | CS1503: long → string | `UserId.NewUserId(userId)` | `UserId.NewUserId(userId.ToString())` | B |

**関連コード（行599）**:
```csharp
var userGuid = Guid.TryParse(identityIdClaim.Value, out var guid) ? guid : Guid.Empty;
```

**関連コード（行802）**:
```csharp
if (!long.TryParse(model.UserId, out var userId))
```
→ このlong.TryParseのロジック自体を見直す必要あり

---

## 波及効果分析

### 1. Guid.TryParse使用箇所（7箇所）

以下の箇所でGuid.TryParseが使用されているが、UserIdがstring型になった今、Guid変換自体が不要になる可能性がある：

| ファイル | 行番号 | 対応方針 |
|---------|--------|---------|
| ProjectMembers.razor | 220 | Guid.TryParse削除検討 |
| ProjectMemberSelector.razor | 146 | Guid.TryParse削除検討 |
| ProjectCreate.razor | 185 | Guid.TryParse削除検討 |
| ProjectList.razor | 297 | Guid.TryParse削除検討 |
| ProjectEdit.razor | 275 | Guid.TryParse削除検討 |
| Create.razor (Users) | 471 | Guid.TryParse削除検討 |
| Edit.razor (Users) | 599 | Guid.TryParse削除検討 |

### 2. long.TryParse使用箇所（3箇所）

| ファイル | 行番号 | 対応方針 |
|---------|--------|---------|
| Index.razor (Users) | 343 | long.TryParse削除、stringのまま使用 |
| Edit.razor (Users) | 802 | long.TryParse削除、stringのまま使用 |
| CustomAuthenticationStateProvider.cs | 169, 176 | GetCurrentDomainUserId戻り値型の見直し |

### 3. Guid型変数宣言（5箇所）

以下の変数は`Guid`型で宣言されているが、`string`型に変更するか使用時に`.ToString()`で変換するか判断が必要：

| ファイル | 変数名 | 対応方針 |
|---------|-------|---------|
| ProjectMembers.razor | `currentUserId: Guid` | 使用時に.ToString()（最小変更） |
| ProjectList.razor | `currentUserId: Guid` | 使用時に.ToString()（最小変更） |
| ProjectCreate.razor | `currentUserId: Guid` | 使用時に.ToString()（最小変更） |
| ProjectEdit.razor | `currentUserId: Guid` | 使用時に.ToString()（最小変更） |
| ProjectMembers.razor | `selectedUserId: Guid?` | 使用時に.ToString()（最小変更） |

---

## 推奨修正方針

### 方針1: 最小変更アプローチ（推奨）

- Guid/long型変数はそのまま維持
- Command/Query引数で`.ToString()`を追加
- int → string代入箇所は文字列リテラルに変更

**メリット**: 変更範囲が限定的、リスク低
**デメリット**: Guid.TryParse等の冗長なロジックが残る

### 方針2: 根本修正アプローチ

- `currentUserId`等を`string`型に変更
- `Guid.TryParse`を削除し、Identity IDをそのまま使用
- 認証周りのロジックを簡素化

**メリット**: コードの簡素化、Issue #79の本質に沿う
**デメリット**: 変更範囲が拡大、テスト工数増

---

## 追加確認事項

### ProjectMemberSelector.razor

調査時に発見したが、ビルドエラーには含まれていない。今後の波及効果として注視：

- 行116: `Guid.Parse(u.Id)` → `u.Id`はstring型になったため、Parseは不要になる可能性
- 行146: `Guid.TryParse(selectedUserId, out var userId)` → 削除検討

---

## チェックリスト（Step 10実行時使用）

- [ ] BlazorAuthenticationService.cs 行139 修正
- [ ] ProjectMembers.razor 5箇所 修正
- [ ] ProjectList.razor 3箇所 修正
- [ ] Index.razor (Users) 5箇所 修正
- [ ] ProjectCreate.razor 2箇所 修正
- [ ] ProjectEdit.razor 3箇所 修正
- [ ] Create.razor (Users) 1箇所 修正
- [ ] Edit.razor (Users) 2箇所 修正
- [ ] ビルド確認（0 Error）
- [ ] 波及効果確認（Guid.TryParse等）

---

**作成完了**: 2025-12-07
**調査基準**: Phase_Summary.md「Step 10開始前の具体的調査コマンド」準拠
