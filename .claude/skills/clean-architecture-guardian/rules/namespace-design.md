# namespace設計規約（Bounded Context別）

## 概要

Bounded Context別サブnamespace規約。Phase B1 Step5で確立・実証済み。ADR_019から抽出。

---

## 🔴 CRITICAL: 基本テンプレート

```
<ProjectName>.<Layer>.<BoundedContext>[.<Feature>]
```

**例**: `UbiquitousLanguageManager.Domain.ProjectManagement`

---

## レイヤー別namespace規約

| 層 | 言語 | namespace例 |
|----|------|------------|
| **Domain** | F# | `...Domain.Common`, `...Domain.Authentication`, `...Domain.ProjectManagement` |
| **Application** | F# | `...Application.ProjectManagement`, `...Application.Interfaces` |
| **Infrastructure** | C# | `...Infrastructure.Data`, `...Infrastructure.Repositories`, `...Infrastructure.Identity` |
| **Contracts** | C# | `...Contracts.DTOs`, `...Contracts.Converters`, `...Contracts.Interfaces` |
| **Web** | C# | `...Web.Components`, `...Web.Pages`, `...Web.Services` |

---

## Bounded Context一覧

| Bounded Context | 責務 | Phase |
|----------------|------|-------|
| **Common** | 全境界共通定義（ID型・Permission・Role） | Phase A完了 |
| **Authentication** | ユーザー・認証・権限管理 | Phase A完了 |
| **ProjectManagement** | プロジェクト管理 | Phase B1完了 |
| **UbiquitousLanguageManagement** | ユビキタス言語管理 | Phase D計画中 |
| **DomainManagement** | ドメイン管理 | Phase C計画中 |

### Common境界（特別扱い）

**定義**: 全Bounded Contextで使用する共通定義
**配置**: 各層のルート直下または`.Common`サブnamespace
**含まれる型**: UserId, ProjectId, DomainId, Permission型(17種類), Role型(4種類)

---

## 階層構造ルール

| 階層数 | 判定 | 例 |
|-------|------|-----|
| **3階層** | ✅ 推奨 | `...Domain.ProjectManagement` |
| **4階層** | 🟡 許容 | `...Domain.ProjectManagement.Specifications` |
| **5階層以上** | ❌ 禁止 | 可読性低下・保守性悪化 |

---

## 🔴 CRITICAL: F# Compilation Order制約

F#は前方宣言不可のため、依存関係順に厳密なコンパイル順序が必要。

### Bounded Context間依存順

```xml
<ItemGroup>
  <!-- 1. Common: 最初 -->
  <Compile Include="Common\CommonTypes.fs" />
  <Compile Include="Common\CommonValueObjects.fs" />

  <!-- 2. Authentication -->
  <Compile Include="Authentication\AuthenticationValueObjects.fs" />
  <Compile Include="Authentication\AuthenticationEntities.fs" />

  <!-- 3. ProjectManagement -->
  <Compile Include="ProjectManagement\ProjectValueObjects.fs" />
  <Compile Include="ProjectManagement\ProjectEntities.fs" />
</ItemGroup>
```

### Bounded Context内依存順

```
ValueObjects.fs → Errors.fs → Entities.fs → DomainService.fs
```

### よくあるエラー

```
Error: The type 'ProjectId' is used before it is defined
```
**原因**: Compilation Order違反
**解決**: Common を ProjectManagement より前に配置

---

## C#特別考慮事項

### using文パターン

```csharp
// Bounded Context別グループ化（推奨）
using UbiquitousLanguageManager.Domain.Common;
using UbiquitousLanguageManager.Domain.ProjectManagement;
using UbiquitousLanguageManager.Application.ProjectManagement;
```

### using alias（型名衝突回避）

```csharp
using DomainModel = UbiquitousLanguageManager.Domain.ProjectManagement.Domain;
using DomainEntity = UbiquitousLanguageManager.Infrastructure.Data.Entities.Domain;
```

---

## 検証チェックリスト

### Step開始時

- [ ] namespace構造レビュー実施
- [ ] Bounded Context境界確認
- [ ] 循環依存なし確認

### Phase完了時

- [ ] 全層namespace整合性確認
- [ ] 基本テンプレート準拠確認
- [ ] Clean Architecture 97点以上維持確認

---

## 参考情報

- **DDD**: Eric Evans著 "Domain-Driven Design"
- **F# for fun and profit**: Scott Wlaschin
- **Phase B1 Step5記録**: `Doc/08_Organization/Completed/Phase_B1/Step05_namespace階層化.md`
