# ユーザー管理機能 全層調査レポート

**調査日**: 2025-11-30
**調査目的**: Phase B-F3 Step1.5 Stage2以降のリファクタ計画策定
**調査方針**: 「最初から作り直すつもりで」全層を精査

---

## 1. アーキテクチャ概要

```
Web層 (C# Blazor Server)
  ↓ 呼び出し
Contracts層 (C# DTO/TypeConverter)
  ↓ 型変換
Application層 (F# ユースケース)
  ↓ 呼び出し
Domain層 (F# ドメインモデル)
  ↑ 参照
Infrastructure層 (C# EF Core/Repository)
  ↓ 永続化
Database (PostgreSQL)
```

---

## 2. Domain層（F#）- 完成度: ✅100%

### ファイル構成
```
src/UbiquitousLanguageManager.Domain/Authentication/
├── AuthenticationEntities.fs      # User Aggregate Root
├── AuthenticationValueObjects.fs  # Email, UserName, Password等
├── AuthenticationErrors.fs        # エラー型定義
└── UserDomainService.fs           # ドメインサービス

src/UbiquitousLanguageManager.Domain/Common/
└── CommonTypes.fs                 # UserId, Role, Permission等
```

### User Aggregate Root
```fsharp
type User = {
    Id: UserId
    Email: Email
    Name: UserName
    Role: Role
    IsActive: bool
    IsFirstLogin: bool
    PasswordHash: PasswordHash option
    SecurityStamp: SecurityStamp option
    Profile: UserProfile
    ProjectPermissions: ProjectPermission list
    CreatedAt: DateTime
    CreatedBy: UserId
    UpdatedAt: DateTime
    UpdatedBy: UserId
}
```

### 主要メソッド
| メソッド | 説明 | 返り値 |
|---------|------|--------|
| `User.create` | ファクトリーメソッド | User |
| `changeEmail` | メールアドレス変更 | Result<User, string> |
| `changeRole` | ロール変更 | Result<User, string> |
| `setProjectPermissions` | プロジェクト権限設定 | Result<User, string> |
| `deactivate` | ユーザー無効化 | Result<User, string> |
| `activate` | ユーザー有効化 | Result<User, string> |

### Role型（判別共用体）
```fsharp
type Role =
    | SuperUser
    | ProjectManager
    | DomainApprover
    | GeneralUser
```

### 評価
- **品質**: 高品質、Clean Architecture準拠
- **リファクタ必要性**: なし

---

## 3. Application層（F#）- 完成度: ⚠️90%

### ファイル構成
```
src/UbiquitousLanguageManager.Application/
├── AuthenticationServices.fs      # 認証ユースケース（約600行）
├── UserManagementServices.fs      # ユーザー管理ユースケース（約480行）
├── IUserManagementService.fs      # インターフェース定義
└── UseCases.fs                    # ユースケース関数型実装
```

### UserManagementApplicationService

#### 実装済み機能
| メソッド | 状態 | 備考 |
|---------|------|------|
| `GetAllUsersAsync` | ✅完了 | ProjectManager権限フィルタ実装済み |
| `GetUserByIdAsync` | ⚠️部分実装 | **ProjectManager権限フィルタ未実装** |
| `CreateUserAsync` | ⚠️部分実装 | **プロジェクト割り当て未実装** |
| `UpdateUserAsync` | ⚠️部分実装 | **プロジェクト割り当て更新未実装** |
| `DeactivateUserAsync` | ✅完了 | - |
| `ActivateUserAsync` | ✅完了 | - |

#### 問題点詳細

**問題1: GetUserByIdAsyncのProjectManager権限フィルタ未実装**
```fsharp
// 現状のコード（TODOコメントのまま）
// TODO: ProjectManagerは自分が管理するプロジェクトのユーザーのみ参照可能（Phase B-F3 Step2）
```

**問題2: プロジェクト割り当て機能未実装**
```fsharp
// CreateUserAsync内（TODOコメントのまま）
// TODO: Step 8プロジェクト割り当て（Phase B-F3 Step2）

// UpdateUserAsync内（TODOコメントのまま）
// TODO: Step 9プロジェクト割り当て更新（Phase B-F3 Step2）
```

### 評価
- **品質**: 高品質、Railway-oriented Programming採用
- **リファクタ必要性**: あり（権限フィルタ・プロジェクト割り当て）

---

## 4. Contracts層（C#）- 完成度: ✅100%

### ファイル構成
```
src/UbiquitousLanguageManager.Contracts/
├── DTOs/
│   ├── Authentication/
│   │   ├── AuthenticatedUserDto.cs
│   │   ├── LoginRequestDto.cs
│   │   └── ...
│   └── UserDto.cs
├── Converters/
│   ├── AuthenticationConverter.cs
│   └── TypeConverters.cs
└── Mappers/
    └── ResultMapper.cs
```

### 主要DTO
```csharp
public class UserDto
{
    public long Id { get; set; }
    public string Email { get; set; }
    public string Name { get; set; }
    public string Role { get; set; }
    public bool IsActive { get; set; }
    public bool IsFirstLogin { get; set; }
}
```

### 評価
- **品質**: 高品質、型変換パターン確立
- **リファクタ必要性**: なし

---

## 5. Infrastructure層（C#）- 完成度: ❌50%

### 🔴 重要発見事項（2025-11-30追記）

**IUserRepository実装が2ファイル存在**:

| ファイル | 状態 | DI登録 |
|---------|------|--------|
| **UserRepositoryAdapter.cs** (780行) | ✅ 本番使用中 | `AddScoped<IUserRepository, UserRepositoryAdapter>` |
| UserRepository.cs (1220行) | ⚠️ レガシー・未使用 | なし |

**調査根拠**:
- Program.cs 210行目: `builder.Services.AddScoped<IUserRepository, UserRepositoryAdapter>()`
- UserRepository.csはDI登録されていない

**結論**:
- **UserRepositoryAdapter.cs が本番で使用されている実装**
- UserRepository.csは削除対象（テストファイル修正後）

### ファイル構成
```
src/UbiquitousLanguageManager.Infrastructure/
├── Data/
│   ├── Entities/
│   │   ├── ApplicationUser.cs      # ASP.NET Core Identity
│   │   └── UserProject.cs          # ユーザー・プロジェクト多対多
│   └── UbiquitousLanguageDbContext.cs
├── Repositories/
│   ├── IUserRepository.cs          # F# インターフェース（Application層に定義）
│   ├── UserRepositoryAdapter.cs    # ✅ 本番使用中（780行）
│   └── UserRepository.cs           # ⚠️ レガシー・削除対象（1220行）
└── Services/
    └── AuthenticationService.cs
```

### UserRepositoryAdapter.cs - 問題点（本番使用中の実装）

#### 正常動作しているメソッド（変更不要）
- `GetByEmailAsync` (58-102行) - UserManager.FindByEmailAsync使用 ✅
- `GetByIdentityIdAsync` (167-209行) - Identity ID直接検索 ✅
- `GetAllActiveUsersAsync` (428-471行) - IsDeleted=false フィルタ ✅
- `GetAllUsersAsync` (476-519行) - 論理削除除外 ✅
- `SearchUsersAsync` (548-600行) - 部分一致検索 ✅

#### 問題1: DeleteAsync - 成功時にエラーを返すバグ（致命的） 🔴
```csharp
// 現状のコード（411行）
return FSharpResult<Unit, string>.NewError("Delete completed successfully");
// ↑ SUCCESS時に ERROR を返している！
```

**影響**:
- 論理削除は実行されるが、呼び出し元にはエラーとして返却される
- UI上でエラー表示される

#### 問題2: GetByRoleAsync - 空リスト返却 🔴
```csharp
// 現状のコード（525-542行）
await Task.Delay(1); // async警告解消
var emptyList = FSharpList<User>.Empty;
return FSharpResult<FSharpList<User>, string>.NewOk(emptyList);
```

**影響**:
- ロール別ユーザー取得が機能しない

#### 問題3: SaveAsync - ロール変更が永続化されない 🟡
```csharp
// 現状のコード（219-276行）
// ユーザー属性は更新されるが、ロール変更はAspNetUserRolesテーブルに反映されない
```

**影響**:
- ユーザーのロール変更がDBに保存されない

#### 問題4: GetUsersByProjectIdsAsync - 空リスト返却 🟡
```csharp
// 現状のコード（315-333行）
return FSharpResult<FSharpList<User>, string>.NewOk(FSharpList<User>.Empty);
```

**影響**:
- プロジェクト所属ユーザー取得が機能しない
- ProjectManager権限フィルタが正しく動作しない

#### 問題5: GetProjectIdsByUserIdAsync - 空リスト返却 🟡
```csharp
// 現状のコード（346-363行）
return FSharpResult<FSharpList<ProjectId>, string>.NewOk(FSharpList<ProjectId>.Empty);
```

**影響**:
- ユーザーの所属プロジェクト取得が機能しない

#### 問題6: ID変換 - 合成GUIDによる情報損失 🟢
```csharp
// 現状のコード（645-648行）
return new Guid((int)(userId.Item % int.MaxValue), 0, 0, 0, 0, 0, 0, 0, 0, 0, 0).ToString();
```

**影響**:
- 合成GUIDによる衝突リスク
- 将来的なDB設計変更で対応予定

---

### UserRepository.cs - レガシー実装（削除対象）

**削除理由**:
1. Program.csでDI登録されていない（未使用）
2. UserRepositoryAdapterが全機能を提供
3. 1220行のレガシーコードがメンテナンス負担
4. DependencyInjectionUnitTests.csで参照されているが、テスト修正後に削除可能

### IUserRepository インターフェース
```csharp
public interface IUserRepository
{
    Task<FSharpResult<FSharpOption<User>, string>> GetByIdAsync(UserId id);
    Task<FSharpResult<FSharpOption<User>, string>> GetByEmailAsync(Email email);
    Task<FSharpResult<FSharpOption<User>, string>> GetByIdentityIdAsync(string identityId);
    Task<FSharpResult<List<User>, string>> GetAllUsersAsync();
    Task<FSharpResult<List<ProjectId>, string>> GetProjectIdsByUserIdAsync(UserId userId);
    Task<FSharpResult<List<User>, string>> GetUsersByProjectIdsAsync(List<ProjectId> projectIds);
    Task<FSharpResult<User, string>> SaveAsync(User user);
    Task<FSharpResult<Unit, string>> DeleteAsync(UserId id);
}
```

### 評価
- **品質**: 低品質、簡易実装のまま
- **リファクタ必要性**: **必須**（全メソッド完全実装が必要）

---

## 6. Web層（C# Blazor Server）- 完成度: ❌20%

### ファイル構成
```
src/UbiquitousLanguageManager.Web/Components/Pages/Admin/Users/
├── Index.razor       # ユーザー一覧
├── Create.razor      # ユーザー作成
└── Edit.razor        # ユーザー編集
```

### Index.razor - 問題点

| 問題 | 詳細 |
|------|------|
| 権限制御不足 | `[Authorize(Roles = "SuperUser")]`のみ、ProjectManager未対応 |
| レイアウト | スマホサイズ（FullHD非対応） |
| プロジェクト別フィルタ | 未実装 |
| data-testid属性 | 0個 |

### Create.razor - 問題点

| 問題 | 詳細 |
|------|------|
| プロジェクト一覧 | 常に空リスト（`availableProjects = new List<>()`) |
| ロール選択制限 | ProjectManager制約なし |
| レイアウト | スマホサイズ |
| data-testid属性 | 0個 |

### Edit.razor - 問題点

| 問題 | 詳細 |
|------|------|
| プロジェクト一覧 | 常に空リスト |
| パスワードリセット | 未実装 |
| ステータス変更 | 未実装 |
| レイアウト | スマホサイズ |
| data-testid属性 | 0個 |

### 評価
- **品質**: 低品質、仕様未準拠
- **リファクタ必要性**: **必須**（全画面書き換え）

---

## 7. UI設計書要件（3.6-3.8章）

### 3.6 ユーザー一覧画面
- 表示項目: 氏名、メールアドレス、権限レベル、所属プロジェクト
- 検索: 氏名部分一致
- フィルタ: プロジェクト別
- 削除済み表示切替
- ページング: 50/100/200件
- 権限: SuperUser=全ユーザー、ProjectManager=担当プロジェクトのみ

### 3.7 ユーザー登録画面
- 入力: メールアドレス、氏名、初期パスワード、ロール、所属プロジェクト
- ロール制限: ProjectManagerは一般ユーザー/ドメイン承認者のみ選択可
- プロジェクト制限: ProjectManagerは担当プロジェクトのみ表示

### 3.8 ユーザー編集画面
- メールアドレス: 表示のみ（変更不可）
- ステータス変更: アクティブ/非アクティブ
- パスワードリセット機能
- ロール・プロジェクト制限: 3.7と同様

---

## 8. ProjectManagement参考パターン評価

### 採用推奨パターン
| パターン | 評価 |
|----------|------|
| 状態管理（OnInitializedAsync） | ✅採用推奨 |
| Railway-oriented Programming | ✅採用推奨 |
| EditForm + DataAnnotationsValidator | ✅採用推奨 |
| CustomRadioGroup + RadioOption | ✅採用推奨 |

### 注意が必要なパターン
| パターン | 問題点 |
|----------|--------|
| F# Option型変換 | `FSharpOption<T>.get_IsSome()`が冗長 |
| 権限フィルタ | ProjectManager制約が不完全 |
| data-testid属性 | **0個（未実装）** |

---

## 9. 層別完成度サマリ

| 層 | 完成度 | リファクタ必要性 |
|---|--------|-----------------|
| Domain層 | ✅100% | なし |
| Application層 | ⚠️90% | あり（権限フィルタ・プロジェクト割り当て） |
| Contracts層 | ✅100% | なし |
| Infrastructure層 | ❌50% | **必須**（全メソッド実装） |
| Web層 | ❌20% | **必須**（全画面書き換え） |

---

## 10. 結論

**UIだけ書き換えても動作しない理由**:
1. Infrastructure層（UserRepository）がDB操作を行っていない
2. Application層の権限フィルタ・プロジェクト割り当てが未実装
3. Web層がこれらの層に依存しているため、上位層だけ修正しても効果がない

**推奨アプローチ**:
Infrastructure層 → Application層 → Web層 の順で下から積み上げて実装

---

**調査完了**: 2025-11-30
