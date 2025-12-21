# Clean Architecture違反パターンと修正方法

## 概要

よくある違反パターンと修正方法・自動チェック手順・Phase B1実証結果。

## 目次

- [自動チェック手順](#自動チェック手順)
- [よくある違反パターンと修正方法](#よくある違反パターンと修正方法)
- [Phase B1での実証結果](#phase-b1での実証結果)

---

## 自動チェック手順

### 1. 依存関係チェック

```
# プロジェクト参照を確認
Grep "ProjectReference" *.csproj *.fsproj

# 不正な依存（Domain → Infrastructure等）を検出
```

### 2. namespace階層チェック

```
# namespace宣言を確認
Grep "namespace UbiquitousLanguageManager" *.cs *.fs

# 基本テンプレート準拠を検証
# 期待: <ProjectName>.<Layer>.<BoundedContext>[.<Feature>]
```

### 3. Bounded Context境界チェック

```
# using/open文を確認
Grep "using UbiquitousLanguageManager" *.cs
Grep "open UbiquitousLanguageManager" *.fs

# 境界を越える不適切な参照を検出
```

### 4. F# Compilation Orderチェック

```
# .fsprojファイルのCompile順序を確認
Read src/UbiquitousLanguageManager.Domain/UbiquitousLanguageManager.Domain.fsproj

# Common → Authentication → ProjectManagement順序を検証
```

---

## よくある違反パターンと修正方法

### 違反1: F# → C#依存

```
❌ 誤り: Domain層からInfrastructure層への参照
<ProjectReference Include="..\UbiquitousLanguageManager.Infrastructure\..." />
```

**修正**: 依存関係逆転の原則（DIP）適用
```
✅ 正しい: Domain層でInterface定義、Infrastructure層で実装
// Domain層
type IProjectRepository = ...

// Infrastructure層（C#）
public class ProjectRepository : IProjectRepository { ... }
```

### 違反2: フラットnamespace

```
❌ 誤り: Bounded Contextなしのフラットnamespace
namespace UbiquitousLanguageManager.Domain

type Project = ...
type User = ...  // 異なる境界文脈が混在
```

**修正**: Bounded Context別サブnamespace
```
✅ 正しい: 境界文脈別に分離
namespace UbiquitousLanguageManager.Domain.ProjectManagement
type Project = ...

namespace UbiquitousLanguageManager.Domain.Authentication
type User = ...
```

### 違反3: 循環依存

```
❌ 誤り: Application ⇄ Infrastructure 循環参照
```

**修正**: Clean Architecture依存方向遵守
```
✅ 正しい: Infrastructure → Application → Domain（一方向のみ）
```

### 違反4: F# Compilation Order違反

```
❌ 誤り: ProjectManagementがCommonより前
<Compile Include="ProjectManagement\ProjectEntities.fs" />
<Compile Include="Common\CommonTypes.fs" />
```

**修正**: 依存順序に並べ替え
```
✅ 正しい: Commonを最初に配置
<Compile Include="Common\CommonTypes.fs" />
<Compile Include="ProjectManagement\ProjectEntities.fs" />
```

---

## Phase B1での実証結果

### 実装影響（Phase B1 Step5）

| 項目 | 結果 |
|------|------|
| **修正ファイル数** | 42ファイル |
| **修正時間** | 3.5-4.5時間 |
| **エラー種別** | namespace階層化・型衝突解決 |
| **最終品質** | 0 Warning/0 Error・全32テスト成功 |

### 確立した知見

1. **事前検証の重要性**: Step開始前にnamespace構造レビュー実施
2. **段階的移行**: プロジェクト単位での段階的移行
3. **ロールバック準備**: Step単位でのgit commit

---

**作成日**: 2025-12-21
**抽出元**: SKILL.md Phase B1 Step5実績
