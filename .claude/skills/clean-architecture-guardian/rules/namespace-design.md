# namespace設計規約（Bounded Context別サブnamespace規約）

## 概要

Bounded Context別サブnamespace規約。Phase B1 Step5で確立・実証済み。ADR_019から抽出。

## 基本テンプレート（必須遵守）

```
<ProjectName>.<Layer>.<BoundedContext>[.<Feature>]
```

**具体例**:
```
UbiquitousLanguageManager.Domain.ProjectManagement
UbiquitousLanguageManager.Application.ProjectManagement
UbiquitousLanguageManager.Infrastructure.Repositories
```

## レイヤー別namespace規約

### Domain層（F#）

```fsharp
namespace UbiquitousLanguageManager.Domain.Common          // 共通定義
namespace UbiquitousLanguageManager.Domain.Authentication  // 認証境界文脈
namespace UbiquitousLanguageManager.Domain.ProjectManagement  // プロジェクト管理境界文脈
namespace UbiquitousLanguageManager.Domain.UbiquitousLanguageManagement  // ユビキタス言語管理境界文脈
```

**ファイル構成例（ProjectManagement境界）**:
```
Domain/
└── ProjectManagement/
    ├── ProjectValueObjects.fs    // namespace ...Domain.ProjectManagement
    ├── ProjectErrors.fs          // namespace ...Domain.ProjectManagement
    ├── ProjectEntities.fs        // namespace ...Domain.ProjectManagement
    └── ProjectDomainService.fs   // namespace ...Domain.ProjectManagement
```

### Application層（F#）

```fsharp
namespace UbiquitousLanguageManager.Application.ProjectManagement
namespace UbiquitousLanguageManager.Application.Interfaces
```

### Infrastructure層（C#）

```csharp
namespace UbiquitousLanguageManager.Infrastructure.Data
namespace UbiquitousLanguageManager.Infrastructure.Repositories
namespace UbiquitousLanguageManager.Infrastructure.Identity
```

### Contracts層（C#）

```csharp
namespace UbiquitousLanguageManager.Contracts.DTOs
namespace UbiquitousLanguageManager.Contracts.Converters
namespace UbiquitousLanguageManager.Contracts.Interfaces
```

### Web層（Blazor Server, C#）

```csharp
namespace UbiquitousLanguageManager.Web.Components
namespace UbiquitousLanguageManager.Web.Pages
namespace UbiquitousLanguageManager.Web.Services
```

## Bounded Context分離原則

### Common境界（特別扱い）

**定義**: 全Bounded Contextで使用する共通定義

**配置**: 各層のルート直下または`.Common`サブnamespace

**含まれる型**:
- ID型（UserId, ProjectId, DomainId等）
- Permission型
- Role型
- ApprovalStatus型
- Description型

**依存関係**: 他のBounded Contextに依存しない

**実装例**:
```fsharp
namespace UbiquitousLanguageManager.Domain.Common

// 共通ID型
type UserId = UserId of Guid
type ProjectId = ProjectId of Guid
type DomainId = DomainId of Guid

// 共通Permission
type Permission =
    | CreateProject
    | ReadProject
    | UpdateProject
    | DeleteProject
    // ... 17種類

// 共通Role
type Role =
    | SuperUser
    | ProjectManager
    | DomainApprover
    | GeneralUser
```

### Bounded Context一覧

| Bounded Context | 責務 | Phase |
|----------------|------|-------|
| **Common** | 全境界共通定義 | Phase A完了 |
| **Authentication** | ユーザー・認証・権限管理 | Phase A完了 |
| **ProjectManagement** | プロジェクト管理 | Phase B1完了 |
| **UbiquitousLanguageManagement** | ユビキタス言語管理 | Phase D計画中 |
| **DomainManagement** | ドメイン管理 | Phase C計画中 |

### Bounded Context境界原則

1. **凝集性**: 関連する概念を1つのBounded Contextに集約
2. **独立性**: 他Bounded Contextへの依存を最小化
3. **境界明確化**: ドメイン用語の意味が境界内で一貫

## 階層構造ルール

### 最大階層制限

- **推奨**: 3階層まで（`<Project>.<Layer>.<BoundedContext>`）
- **許容**: 4階層（`<Project>.<Layer>.<BoundedContext>.<Feature>`）
- **禁止**: 5階層以上（可読性低下・保守性悪化）

**例**:
```
✅ 推奨（3階層）
UbiquitousLanguageManager.Domain.ProjectManagement

🟡 許容（4階層）
UbiquitousLanguageManager.Domain.ProjectManagement.Specifications

❌ 禁止（5階層以上）
UbiquitousLanguageManager.Domain.ProjectManagement.Specifications.Complex
```

### 理由

- **可読性**: 深すぎる階層は理解困難
- **保守性**: 階層変更時の影響範囲拡大
- **IntelliSense**: 補完候補が増えすぎる

## F#特別考慮事項

### Module設計との関係

```fsharp
namespace UbiquitousLanguageManager.Domain.ProjectManagement

// 型定義（namespace直下）
type ProjectId = ProjectId of Guid
type ProjectName = private ProjectName of string

// Smart Constructor（module）
module ProjectName =
    let create (value: string) : Result<ProjectName, string> = ...
    let value (ProjectName name) = name

// ドメインサービス（module）
module ProjectDomainService =
    let validateProjectName name = ...
    let createProject name = ...
```

**ポイント**:
- **namespace**: Bounded Context単位
- **module**: 機能単位（型のコンパニオン・ドメインサービス）
- **Module ≠ Bounded Context**: 強制しない・保守性優先

### F# Compilation Order制約（厳格遵守必須）

F#は前方宣言不可のため、依存関係順に厳密なコンパイル順序が必要。

**Bounded Context間依存順**:
```xml
<ItemGroup>
  <!-- 1. Common Bounded Context: 最初 -->
  <Compile Include="Common\CommonTypes.fs" />
  <Compile Include="Common\CommonValueObjects.fs" />
  <Compile Include="Common\CommonSpecifications.fs" />

  <!-- 2. Authentication Bounded Context -->
  <Compile Include="Authentication\AuthenticationValueObjects.fs" />
  <Compile Include="Authentication\AuthenticationErrors.fs" />
  <Compile Include="Authentication\AuthenticationEntities.fs" />
  <Compile Include="Authentication\UserDomainService.fs" />

  <!-- 3. ProjectManagement Bounded Context -->
  <Compile Include="ProjectManagement\ProjectValueObjects.fs" />
  <Compile Include="ProjectManagement\ProjectErrors.fs" />
  <Compile Include="ProjectManagement\ProjectEntities.fs" />
  <Compile Include="ProjectManagement\ProjectDomainService.fs" />

  <!-- 4. UbiquitousLanguageManagement Bounded Context -->
  <Compile Include="UbiquitousLanguageManagement\UbiquitousLanguageValueObjects.fs" />
  <Compile Include="UbiquitousLanguageManagement\UbiquitousLanguageErrors.fs" />
  <Compile Include="UbiquitousLanguageManagement\UbiquitousLanguageEntities.fs" />
  <Compile Include="UbiquitousLanguageManagement\UbiquitousLanguageDomainService.fs" />
</ItemGroup>
```

**Bounded Context内依存順**:
```
ValueObjects.fs → Errors.fs → Entities.fs → DomainService.fs
```

**理由**: F#は宣言順にコンパイル・前方参照不可の制約

### よくあるエラー

```
Error: The type 'ProjectId' is used before it is defined
```

**原因**: Compilation Order違反

**解決**: Common を ProjectManagement より前に配置

## C#特別考慮事項

### using文推奨パターン

```csharp
// Blazor Serverコンポーネント
using UbiquitousLanguageManager.Domain.Common;
using UbiquitousLanguageManager.Domain.ProjectManagement;
using UbiquitousLanguageManager.Application.ProjectManagement;
```

**Bounded Context別にグループ化**（推奨）:
```csharp
// Domain層
using UbiquitousLanguageManager.Domain.Common;
using UbiquitousLanguageManager.Domain.Authentication;
using UbiquitousLanguageManager.Domain.ProjectManagement;

// Application層
using UbiquitousLanguageManager.Application.ProjectManagement;

// Contracts層
using UbiquitousLanguageManager.Contracts.DTOs;
```

### using alias使用（型名衝突回避）

```csharp
// Infrastructure層のDomain Entity vs Domain層のDomain型
using DomainModel = UbiquitousLanguageManager.Domain.ProjectManagement.Domain;
using DomainEntity = UbiquitousLanguageManager.Infrastructure.Data.Entities.Domain;

// 使用時
DomainModel domain = domainService.CreateDefault(projectId);
DomainEntity entity = mapper.ToEntity(domain);
```

**Phase B1で発生した型衝突**:
```csharp
// ProjectCreationError.DuplicateProjectName vs ProjectUpdateError.DuplicateProjectName
// テストコードで完全修飾名使用（12箇所修正）
var error = ProjectCreationError.DuplicateProjectName("test");
```

## 検証プロセス（必須実行）

### Step開始時検証

- [ ] namespace構造レビュー実施
- [ ] Bounded Context境界確認
- [ ] 循環依存なし確認
- [ ] 基本テンプレート準拠確認

### Phase完了時検証

- [ ] 全層namespace整合性確認
- [ ] 基本テンプレート準拠確認
- [ ] F#/C#ベストプラクティス準拠確認
- [ ] Clean Architecture 97点以上維持確認

## Phase B1 Step5実装記録（2025-10-01）

### 実施内容

1. **Domain層namespace階層化**: 15ファイル・4境界文脈（Common/Authentication/ProjectManagement/UbiquitousLanguageManagement）
2. **Application層open文修正**: 12ファイル・Bounded Context別open文追加
3. **Contracts層using文修正**: 7ファイル・C#境界参照更新
4. **Infrastructure層using文修正**: 4ファイル・認証系中心
5. **Web層using文修正**: 2ファイル・@using形式対応
6. **Tests層修正**: 2ファイル・型衝突解決（完全修飾名使用）
7. **統合ビルド・テスト**: 全層0 Warning/0 Error・32テスト100%成功

### 発見された課題と対応

**課題**: `ProjectCreationError.DuplicateProjectName` と `ProjectUpdateError.DuplicateProjectName` の型名衝突

**対応**: テストコードで完全修飾名使用（12箇所修正）

**教訓**: 同一namespace内で同名コンストラクタを持つ判別共用体は型衝突リスクあり

### 確立したパターン

1. **Bounded Context分離**: 4境界文脈確立（Common/Authentication/ProjectManagement/UbiquitousLanguageManagement）
2. **F# Compilation Order最適化**: 依存関係順厳格管理・前方参照制約対応
3. **42ファイル移行**: モノリシック構造から境界文脈分離
4. **0 Warning/0 Error達成**: 全ビルド成功・既存テスト100%維持

## よくある違反パターンと修正方法

### 違反1: フラットnamespace

```fsharp
❌ 誤り: Bounded Contextなし
namespace UbiquitousLanguageManager.Domain

type Project = ...
type User = ...  // 異なる境界文脈が混在
```

**修正**:
```fsharp
✅ 正しい: Bounded Context別に分離
namespace UbiquitousLanguageManager.Domain.ProjectManagement
type Project = ...

namespace UbiquitousLanguageManager.Domain.Authentication
type User = ...
```

### 違反2: 深すぎる階層

```csharp
❌ 誤り: 5階層
namespace UbiquitousLanguageManager.Domain.ProjectManagement.ValueObjects.Specifications
```

**修正**:
```csharp
✅ 正しい: 3-4階層
namespace UbiquitousLanguageManager.Domain.ProjectManagement
// または
namespace UbiquitousLanguageManager.Domain.ProjectManagement.Specifications
```

### 違反3: F# Compilation Order違反

```xml
❌ 誤り: ProjectManagementがCommonより前
<Compile Include="ProjectManagement\ProjectEntities.fs" />
<Compile Include="Common\CommonTypes.fs" />
```

**修正**:
```xml
✅ 正しい: Commonを最初に配置
<Compile Include="Common\CommonTypes.fs" />
<Compile Include="ProjectManagement\ProjectEntities.fs" />
```

## 参考情報

- **DDD**: Eric Evans著 "Domain-Driven Design"
- **F# for fun and profit**: Scott Wlaschin
- **Microsoft Learn**: C# namespace規約
- **Phase B1 Step5記録**: `Doc/08_Organization/Completed/Phase_B1/Step05_namespace階層化.md`
