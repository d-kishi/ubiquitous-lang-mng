# Stage3.5 着手前提条件調査レポート

**作成日**: 2025-12-01
**対象**: Phase B-F3 Step1.5 Stage3.5
**目的**: Stage4（Web層リファクタ）着手前提条件の整備

---

## 1. 調査背景

UI設計書3.7-3.8章の「所属プロジェクト」チェックボックスを実装するにあたり、
プロジェクト一覧取得APIの実装状況を調査した結果、重大な問題が発見された。

---

## 2. 発見した問題

### 2.1 IProjectRepository 二重定義問題

| 場所 | メソッド数 | 詳細 |
|------|------|------|
| **Application層（Interfaces.fs Line 129-141）** | 4メソッド | GetByIdAsync, GetActiveProjectsAsync, SaveAsync, DeleteAsync |
| **Infrastructure層（IProjectRepository.cs）** | 16メソッド | 上記 + GetProjectsByUserAsync, GetAllAsync, AddUserToProjectAsync等 |

**問題点**: 同名だが異なるインターフェース。Application層には`GetProjectsByUserAsync`がない。

### 2.2 DomainRepository.cs 未実装

| コンポーネント | Application層 | Infrastructure層 | 状態 |
|---|---|---|---|
| `IProjectRepository` | ✅ 定義（4メソッド） | ✅ 定義（16メソッド） | ⚠️ 不一致 |
| `ProjectRepository` | - | ✅ 1361行実装済み | ✅ |
| `IDomainRepository` | ✅ 定義（4メソッド） | - | ❌ 未実装 |
| `DomainRepository` | - | ❌ **ファイルなし** | ❌ 未実装 |
| `ProjectManagementService` | ✅ 実装済み | - | ⚠️ DI登録不可 |

### 2.3 ProjectRepository がApplication層インターフェース未実装

`ProjectRepository.cs` Line 42:
```csharp
public class ProjectRepository : IProjectRepository
```

この`IProjectRepository`は**Infrastructure層のインターフェース**（同じnamespace内）。
Application層の`IProjectRepository`は実装されていない。

### 2.4 Program.cs DI登録状況

```
✅ 登録済み:
- IUserManagementService → UserManagementApplicationService
- IUserRepository → UserRepository
- IAuthenticationService → AuthenticationService

❌ コメントアウト中（登録しても動作しない）:
- Line 227: IProjectRepository → ProjectRepository
  → Application層とInfrastructure層のインターフェース不一致
- Line 228: IDomainRepository → DomainRepository
  → DomainRepository未実装
- Line 298: ProjectManagementService
  → IDomainRepository依存のためDI解決失敗
```

---

## 3. 影響分析

### 3.1 Stage4への影響

UI設計書3.7-3.8章「所属プロジェクト」チェックボックス実装に必要：
- `GetProjectsByUserAsync(UserId, Role)` - 権限フィルタ付きプロジェクト一覧

このメソッドはInfrastructure層のIProjectRepositoryにのみ存在。
Application層経由で呼び出すためには、以下の解決が必要：

1. DomainRepository.cs の新規実装
2. ProjectManagementService のDI登録有効化
3. ProjectRepository のApplication層インターフェース実装追加

### 3.2 依存関係図

```
ProjectManagementService (F#)
    ├─ IProjectRepository (Application層 4メソッド) → ProjectRepository (Infrastructure層)
    ├─ IDomainRepository (Application層 4メソッド) → DomainRepository (❌ 未実装)
    └─ IUserRepository (Application層) → UserRepository (✅ 実装済み)
```

---

## 4. 解決策

### 4.1 選択されたアプローチ: 案C（長期対応・本来の姿）

**作業量見積もり**: 6-8時間

### 4.2 実装タスク

| Task | 内容 | 時間 | 担当Agent |
|---|---|---|---|
| C-1 | DomainRepository.cs新規実装 | 3-4h | csharp-infrastructure |
| C-2 | Program.cs DI登録 + ProjectRepository修正 | 1-2h | csharp-infrastructure |
| C-3 | ビルド確認・DI解決検証 | 30min | MainAgent |
| C-4 | Create/Edit.razorプロジェクト一覧取得 | 1-2h | csharp-web-ui |
| C-5 | 動作検証 | 30min | MainAgent |

---

## 5. 実装詳細

### 5.1 Task C-1: DomainRepository.cs新規実装

**参照ファイル**:
- Application層インターフェース: `Interfaces.fs` Line 143-154
- C# Entity: `Infrastructure/Data/Entities/Domain.cs`
- F#ドメインモデル: `Domain/ProjectManagement/ProjectEntities.fs` Line 169-179
- 実装参考: `ProjectRepository.cs`

**必要メソッド（IDomainRepository準拠）**:
1. `GetByIdAsync(DomainId)` - ID検索
2. `GetByProjectIdAsync(ProjectId)` - プロジェクト単位取得
3. `SaveAsync(Domain)` - 保存（Create/Update）
4. `DeleteAsync(DomainId)` - 論理削除

**型変換要件**:
- F# `Domain` ↔ C# `Domain` Entity
- F# `DomainId`/`ProjectId`/`UserId` ↔ C# `long`
- F# `DomainName` ↔ C# `string`
- F# `DateTime option` ↔ C# `DateTime?`

**ファイル作成先**: `src/UbiquitousLanguageManager.Infrastructure/Repositories/DomainRepository.cs`

### 5.2 Task C-2: ProjectRepository + Program.cs修正

**ProjectRepository.csへの追加**:
Application層`IProjectRepository`の明示的実装

```csharp
// クラス宣言を変更
public class ProjectRepository :
    Infrastructure.Repositories.IProjectRepository,
    Application.IProjectRepository  // 追加

// Application層インターフェースの明示的実装を追加
Task<Result<Project option, string>> Application.IProjectRepository.GetByIdAsync(ProjectId projectId)
    => this.GetByIdAsync(projectId);

Task<Result<Project list, string>> Application.IProjectRepository.GetActiveProjectsAsync()
    => this.GetAllAsync();

Task<Result<Project, string>> Application.IProjectRepository.SaveAsync(Project project)
    => this.CreateAsync(project);

Task<Result<unit, string>> Application.IProjectRepository.DeleteAsync(ProjectId projectId)
    => this.DeleteAsync(projectId);
```

**Program.cs変更箇所**:
```csharp
// Line 227: コメント解除
builder.Services.AddScoped<UbiquitousLanguageManager.Application.IProjectRepository,
    UbiquitousLanguageManager.Infrastructure.Repositories.ProjectRepository>();

// Line 228: コメント解除
builder.Services.AddScoped<UbiquitousLanguageManager.Application.IDomainRepository,
    UbiquitousLanguageManager.Infrastructure.Repositories.DomainRepository>();

// Line 298: コメント解除
builder.Services.AddScoped<UbiquitousLanguageManager.Application.ProjectManagement.ProjectManagementService>();
```

### 5.3 Task C-4: Create/Edit.razorプロジェクト一覧取得

**使用サービス**: `ProjectManagementService`（F#）

**呼び出しメソッド**:
```fsharp
member this.GetProjectsByUserAsync(requestUserId: UserId) =
    // 権限フィルタ適用済みプロジェクト一覧取得
```

**Razor実装例**:
```razor
@inject UbiquitousLanguageManager.Application.ProjectManagement.ProjectManagementService ProjectService

@code {
    private List<Project> availableProjects = new();

    protected override async Task OnInitializedAsync()
    {
        var result = await ProjectService.GetProjectsByUserAsync(currentUserId);
        if (result.IsOk)
            availableProjects = result.ResultValue.ToList();
    }
}
```

---

## 6. Critical Files

**新規作成**:
- `src/UbiquitousLanguageManager.Infrastructure/Repositories/DomainRepository.cs`

**修正対象**:
- `src/UbiquitousLanguageManager.Infrastructure/Repositories/ProjectRepository.cs`
- `src/UbiquitousLanguageManager.Web/Program.cs`
- `src/UbiquitousLanguageManager.Web/Components/Pages/Admin/Users/Create.razor`
- `src/UbiquitousLanguageManager.Web/Components/Pages/Admin/Users/Edit.razor`

**参照ファイル**:
- `src/UbiquitousLanguageManager.Application/Interfaces.fs`（IDomainRepository定義 Line 143-154）
- `src/UbiquitousLanguageManager.Infrastructure/Data/Entities/Domain.cs`（C# Entity）
- `src/UbiquitousLanguageManager.Domain/ProjectManagement/ProjectEntities.fs`（F# Domain Line 169-179）
- `src/UbiquitousLanguageManager.Infrastructure/Repositories/ProjectRepository.cs`（実装参考）

---

## 7. 完了基準

- [ ] DomainRepository.cs: IDomainRepository全4メソッド実装
- [ ] ProjectRepository.cs: Application層IProjectRepository明示的実装追加
- [ ] Program.cs: DI登録有効化（Line 227, 228, 298）
- [ ] ビルド: 0 Error
- [ ] Create/Edit.razor: プロジェクト一覧取得実装
- [ ] 動作確認: プロジェクト一覧チェックボックス表示
