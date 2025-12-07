# GetHashCode使用箇所一覧

**調査日**: 2025-12-07
**調査目的**: GitHub Issue #79「ID体系統一リファクタリング」準備

---

## 調査結果サマリー

| 項目 | 数値 |
|------|------|
| **GetHashCode()使用箇所総数** | 30箇所 |
| **対象ファイル数** | 4ファイル |
| **C#ファイル** | 2ファイル（3箇所） |
| **F#ファイル** | 2ファイル（27箇所） |

---

## 1. Infrastructure層（C#）- 3箇所

### 1.1 AuthenticationService.cs

| # | 行番号 | コード | 目的 | 優先度 |
|---|--------|--------|------|--------|
| 1 | 233 | `Id = user.Id.GetHashCode()` | ログイン成功時、IdentityUserのGUID IDをintに変換して認証済みユーザーDTOに設定 | **高** |
| 2 | 1333 | `var userId = UserId.NewUserId((long)identityUser.Id.GetHashCode());` | ApplicationUserのGUID IDをlong型のUserIdに変換 | **高** |

**ファイルパス**: `src/UbiquitousLanguageManager.Infrastructure/Services/AuthenticationService.cs`

### 1.2 UserRepository.cs

| # | 行番号 | コード | 目的 | 優先度 |
|---|--------|--------|------|--------|
| 3 | 1530 | `var userIdValue = (long)appUser.Id.GetHashCode();` | ApplicationUserをF# Userエンティティに変換時、GUID IDをlong型に変換 | **高** |

**ファイルパス**: `src/UbiquitousLanguageManager.Infrastructure/Repositories/UserRepository.cs`

---

## 2. Application層（F#）- 27箇所

### 2.1 Queries.fs - 13箇所

**ファイルパス**: `src/UbiquitousLanguageManager.Application/ProjectManagement/Queries.fs`

| # | 行番号 | 型名 | コードパターン | 優先度 |
|---|--------|------|----------------|--------|
| 4 | 34 | `GetUserProjectsQuery` | `let userId = UserId(int64(this.UserId.GetHashCode()))` | 中 |
| 5 | 45 | `GetProjectDetailQuery` | `ProjectId(int64(this.ProjectId.GetHashCode()))` | 中 |
| 6 | 45 | `GetProjectDetailQuery` | `UserId(int64(this.UserId.GetHashCode()))` | 中 |
| 7 | 55 | `GetProjectUsersQuery` | `ProjectId(int64(this.ProjectId.GetHashCode()))` | 中 |
| 8 | 55 | `GetProjectUsersQuery` | `UserId(int64(this.UserId.GetHashCode()))` | 中 |
| 9 | 67 | `GetProjectMembersQuery` | `ProjectId(int64(this.ProjectId.GetHashCode()))` | 中 |
| 10 | 67 | `GetProjectMembersQuery` | `UserId(int64(this.UserId.GetHashCode()))` | 中 |
| 11 | 78 | `GetProjectDomainsQuery` | `ProjectId(int64(this.ProjectId.GetHashCode()))` | 中 |
| 12 | 78 | `GetProjectDomainsQuery` | `UserId(int64(this.UserId.GetHashCode()))` | 中 |
| 13 | 89 | `GetUserProjectsQuery` | `UserId(int64(this.TargetUserId.GetHashCode()))` | 中 |
| 14 | 90 | `GetUserProjectsQuery` | `UserId(int64(this.RequestUserId.GetHashCode()))` | 中 |
| 15 | 112 | `SearchProjectsQuery` | `UserId(int64(this.UserId.GetHashCode()))` | 中 |
| 16 | 113 | `SearchProjectsQuery` | `Option.map (fun guid -> UserId(int64(guid.GetHashCode())))` | 中 |
| 17 | 124 | `GetProjectStatisticsQuery` | `UserId(int64(this.UserId.GetHashCode()))` | 中 |
| 18 | 125 | `GetProjectStatisticsQuery` | `Option.map (fun guid -> ProjectId(int64(guid.GetHashCode())))` | 中 |

### 2.2 Commands.fs - 14箇所

**ファイルパス**: `src/UbiquitousLanguageManager.Application/ProjectManagement/Commands.fs`

| # | 行番号 | 型名 | コードパターン | 優先度 |
|---|--------|------|----------------|--------|
| 19 | 39 | `CreateProjectCommand` | `UserId(int64(this.OwnerId.GetHashCode()))` | 高 |
| 20 | 40 | `CreateProjectCommand` | `UserId(int64(this.OperatorUserId.GetHashCode()))` | 高 |
| 21 | 52 | `UpdateProjectCommand` | `ProjectId(int64(this.ProjectId.GetHashCode()))` | 高 |
| 22 | 56 | `UpdateProjectCommand` | `UserId(int64(this.OperatorUserId.GetHashCode()))` | 高 |
| 23 | 67 | `DeleteProjectCommand` | `ProjectId(int64(this.ProjectId.GetHashCode()))` | 高 |
| 24 | 67 | `DeleteProjectCommand` | `UserId(int64(this.OperatorUserId.GetHashCode()))` | 高 |
| 25 | 77 | `ChangeProjectOwnerCommand` | `ProjectId(int64(this.ProjectId.GetHashCode()))` | 高 |
| 26 | 77 | `ChangeProjectOwnerCommand` | `UserId(int64(this.NewOwnerId.GetHashCode()))` | 高 |
| 27 | 77 | `ChangeProjectOwnerCommand` | `UserId(int64(this.OperatorUserId.GetHashCode()))` | 高 |
| 28 | 86 | `ActivateProjectCommand` | `ProjectId(int64(this.ProjectId.GetHashCode()))` | 高 |
| 29 | 86 | `ActivateProjectCommand` | `UserId(int64(this.OperatorUserId.GetHashCode()))` | 高 |
| 30 | 118 | `GetProjectStatisticsCommand` | `UserId(int64(this.OperatorUserId.GetHashCode()))` | 高 |
| 31 | 138 | `AddMemberToProjectCommand` | `ProjectId(int64(this.ProjectId.GetHashCode()))` | 高 |
| 32 | 138 | `AddMemberToProjectCommand` | `UserId(int64(this.UserId.GetHashCode()))` | 高 |
| 33 | 139 | `AddMemberToProjectCommand` | `UserId(int64(this.OperatorUserId.GetHashCode()))` | 高 |
| 34 | 156 | `RemoveMemberFromProjectCommand` | `ProjectId(int64(this.ProjectId.GetHashCode()))` | 高 |
| 35 | 157 | `RemoveMemberFromProjectCommand` | `UserId(int64(this.UserId.GetHashCode()))` | 高 |
| 36 | 158 | `RemoveMemberFromProjectCommand` | `UserId(int64(this.OperatorUserId.GetHashCode()))` | 高 |

---

## 3. 変換パターン分析

### 3.1 共通パターン

```
Guid (C#) → GetHashCode() → int → int64 → IdType(int64) (F#)
```

### 3.2 ID型定義（参考）

**ファイル**: `src/UbiquitousLanguageManager.Domain/Common/CommonTypes.fs`

```fsharp
type UserId = UserId of int64
type ProjectId = ProjectId of int64
type DomainId = DomainId of int64
type UbiquitousLanguageId = UbiquitousLanguageId of int64
```

---

## 4. 修正優先度マトリックス

| 優先度 | 箇所数 | 対象 | 理由 |
|--------|--------|------|------|
| **高** | 17 | Infrastructure層（3）+ Commands.fs（14） | 認証・データ更新に直結 |
| **中** | 13 | Queries.fs | 読み取り専用だがデータ整合性に影響 |

---

## 5. 既知の問題点（コード内コメント）

**UserManagementServices.fs**で既に問題が認識されており、回避策が導入済み:

| 行番号 | コメント内容 |
|--------|-------------|
| 152 | `GetHashCode()によるUserId変換は不安定なため、Identity IDを直接使用します。` |
| 453 | `GetHashCode()使用のUserId変換が不安定だったため、ASP.NET Core Identity IDを直接使用する方式に変更しました。` |
| 478 | `GetHashCode()変換を回避し、IdentityIdを直接使用して検索します。` |
| 597 | `GetHashCode()使用のUserId変換が不安定だったため、ASP.NET Core Identity IDを直接使用する方式に変更しました。` |
| 623 | `GetHashCode()変換を回避してIdentityIdで直接削除します。` |

---

## 6. リスク評価

### 6.1 GetHashCode()使用のリスク

1. **ハッシュ衝突**: 異なるGUID同士が同じハッシュコードを生成する可能性
2. **非決定性**: .NETランタイム・バージョン・プラットフォーム間での不一致
3. **デバッグ困難**: ID値の逆算が困難
4. **型安全性低下**: 多段階変換による誤り可能性

### 6.2 最高リスク箇所

- **AuthenticationService.cs L233**: ログイン直後のユーザーID割り当て
- **AuthenticationService.cs L1333**: ユーザー作成時のID変換

---

**作成日**: 2025-12-07
**作成者**: Explore Agent調査結果に基づく
