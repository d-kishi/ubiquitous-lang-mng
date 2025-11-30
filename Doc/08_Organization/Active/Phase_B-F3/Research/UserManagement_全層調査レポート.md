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

### ファイル構成
```
src/UbiquitousLanguageManager.Infrastructure/
├── Data/
│   ├── Entities/
│   │   ├── ApplicationUser.cs      # ASP.NET Core Identity
│   │   └── UserProject.cs          # ユーザー・プロジェクト多対多
│   └── UbiquitousLanguageDbContext.cs
├── Repositories/
│   ├── IUserRepository.cs
│   └── UserRepository.cs           # ⚠️簡易実装のまま
└── Services/
    └── AuthenticationService.cs
```

### UserRepository.cs - 重大問題

#### 問題1: GetHashCode()によるID変換（致命的）
```csharp
// 現状のコード
var entityHashId = (long)entity.Id.GetHashCode();  // ⚠️ハッシュ衝突リスク
if (entityHashId == id.Item) { ... }
```

**影響**:
- ハッシュ衝突により誤ったユーザーを返却するリスク
- 実行環境依存で不安定な動作

#### 問題2: GetByEmailAsync - ハードコード（致命的）
```csharp
// 現状のコード（概要）
public async Task<...> GetByEmailAsync(Email email)
{
    await Task.Delay(1);  // DB検索なし
    var user = User.create(...);  // ダミーユーザー返却
    return ...;
}
```

**影響**:
- 実データがDBから取得されない
- 常に同じダミーユーザーが返却される

#### 問題3: SaveAsync - 永続化なし（致命的）
```csharp
// 現状のコード（概要）
public async Task<...> SaveAsync(User user)
{
    await Task.Delay(1);  // 実際の保存処理なし
    return FSharpResult<User, string>.NewOk(user);
}
```

**影響**:
- ユーザー作成・更新がDBに保存されない
- アプリケーション再起動でデータ消失

#### 問題4: DeleteAsync - 未実装
```csharp
// 現状のコード
public async Task<...> DeleteAsync(UserId id)
{
    return FSharpResult<Unit, string>.NewError("Not implemented");
}
```

#### 問題5: GetByRoleAsync - 空リスト返却
```csharp
// 現状のコード
public async Task<...> GetByRoleAsync(Role role)
{
    await Task.Delay(1);
    return FSharpResult<List<User>, string>.NewOk(new List<User>());
}
```

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
