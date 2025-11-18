# テストプロジェクト参照関係原則（ADR_020準拠）

## 概要

ADR_020テストアーキテクチャ決定に基づく**テストプロジェクト参照関係原則**を定義します。参照関係違反は、Clean Architecture崩壊・ビルドエラー・循環依存の原因となるため、**厳格遵守**が必要です。

---

## 🔴 CRITICAL: 参照関係絶対原則

### 基本原則

```yaml
Unit Tests原則:
  - テスト対象レイヤーのみ参照（最小化原則）
  - 不要な参照禁止（ビルド時間増加・循環依存リスク）

Integration Tests原則:
  - 必要な依存層のみ参照
  - WebApplicationFactory使用時は全層参照

E2E Tests原則:
  - 全層参照可
  - Playwright使用
```

---

## Unit Tests参照関係

### Domain.Unit.Tests（F#）

**原則**: Domain層のみ参照

**参照設定**:
```xml
<ItemGroup>
  <!-- ADR_020準拠: Unit Tests原則 - テスト対象レイヤーのみ参照 -->
  <ProjectReference Include="..\..\src\UbiquitousLanguageManager.Domain\UbiquitousLanguageManager.Domain.fsproj" />
</ItemGroup>
```

**理由**:
- Domain層は外部依存なし（Clean Architecture最内層）
- 単体テストは単一レイヤーのみテスト

**❌ 禁止参照**:
```xml
<!-- Application層参照（違反） -->
<ProjectReference Include="..\..\src\UbiquitousLanguageManager.Application\UbiquitousLanguageManager.Application.fsproj" />

<!-- Infrastructure層参照（違反） -->
<ProjectReference Include="..\..\src\UbiquitousLanguageManager.Infrastructure\UbiquitousLanguageManager.Infrastructure.csproj" />

<!-- Web層参照（違反） -->
<ProjectReference Include="..\..\src\UbiquitousLanguageManager.Web\UbiquitousLanguageManager.Web.csproj" />
```

---

### Application.Unit.Tests（F#）

**原則**: Application + Domain参照

**参照設定**:
```xml
<ItemGroup>
  <!-- Application層はDomain層に依存するため、両方参照 -->
  <ProjectReference Include="..\..\src\UbiquitousLanguageManager.Application\UbiquitousLanguageManager.Application.fsproj" />
  <ProjectReference Include="..\..\src\UbiquitousLanguageManager.Domain\UbiquitousLanguageManager.Domain.fsproj" />
</ItemGroup>
```

**理由**:
- Application層はDomain層に依存（Use CaseがDomain Modelを使用）
- テスト実行にDomain層が必要

**❌ 禁止参照**:
```xml
<!-- Infrastructure層参照（違反） -->
<ProjectReference Include="..\..\src\UbiquitousLanguageManager.Infrastructure\UbiquitousLanguageManager.Infrastructure.csproj" />

<!-- Web層参照（違反） -->
<ProjectReference Include="..\..\src\UbiquitousLanguageManager.Web\UbiquitousLanguageManager.Web.csproj" />
```

---

### Contracts.Unit.Tests（C#）

**原則**: Contracts + Application + Domain参照

**参照設定**:
```xml
<ItemGroup>
  <!-- Contracts層はApplication・Domain層に依存するため、3層参照 -->
  <ProjectReference Include="..\..\src\UbiquitousLanguageManager.Contracts\UbiquitousLanguageManager.Contracts.csproj" />
  <ProjectReference Include="..\..\src\UbiquitousLanguageManager.Application\UbiquitousLanguageManager.Application.fsproj" />
  <ProjectReference Include="..\..\src\UbiquitousLanguageManager.Domain\UbiquitousLanguageManager.Domain.fsproj" />
</ItemGroup>
```

**理由**:
- Contracts層はF#↔C#境界（TypeConverter実装）
- F# Domain/Application型とC# DTO型の変換テストに両方必要

**❌ 禁止参照**:
```xml
<!-- Infrastructure層参照（違反） -->
<ProjectReference Include="..\..\src\UbiquitousLanguageManager.Infrastructure\UbiquitousLanguageManager.Infrastructure.csproj" />

<!-- Web層参照（違反） -->
<ProjectReference Include="..\..\src\UbiquitousLanguageManager.Web\UbiquitousLanguageManager.Web.csproj" />
```

---

### Infrastructure.Unit.Tests（C#）

**原則**: Infrastructure + Domain参照（Application参照は推奨しない）

**参照設定**:
```xml
<ItemGroup>
  <!-- Infrastructure層はDomain層に依存（Repository実装） -->
  <ProjectReference Include="..\..\src\UbiquitousLanguageManager.Infrastructure\UbiquitousLanguageManager.Infrastructure.csproj" />
  <ProjectReference Include="..\..\src\UbiquitousLanguageManager.Domain\UbiquitousLanguageManager.Domain.fsproj" />
</ItemGroup>
```

**理由**:
- Infrastructure層はDomain層に依存（Repository実装がDomain Entityを使用）
- 単体テストはRepository単独テスト（Application層不要）

**⚠️ Application層参照は推奨しない**:
```xml
<!-- Application層参照（非推奨・必要な場合のみ） -->
<ProjectReference Include="..\..\src\UbiquitousLanguageManager.Application\UbiquitousLanguageManager.Application.fsproj" />
```

**❌ 禁止参照**:
```xml
<!-- Web層参照（違反） -->
<ProjectReference Include="..\..\src\UbiquitousLanguageManager.Web\UbiquitousLanguageManager.Web.csproj" />
```

---

### Web.Unit.Tests（C#）

**原則**: Web + 必要な依存層参照

**参照設定**:
```xml
<ItemGroup>
  <!-- Web層は全層に依存可能 -->
  <ProjectReference Include="..\..\src\UbiquitousLanguageManager.Web\UbiquitousLanguageManager.Web.csproj" />
  <!-- 必要に応じて追加 -->
  <ProjectReference Include="..\..\src\UbiquitousLanguageManager.Infrastructure\UbiquitousLanguageManager.Infrastructure.csproj" />
  <ProjectReference Include="..\..\src\UbiquitousLanguageManager.Application\UbiquitousLanguageManager.Application.fsproj" />
  <ProjectReference Include="..\..\src\UbiquitousLanguageManager.Domain\UbiquitousLanguageManager.Domain.fsproj" />
  <ProjectReference Include="..\..\src\UbiquitousLanguageManager.Contracts\UbiquitousLanguageManager.Contracts.csproj" />
</ItemGroup>
```

**理由**:
- Web層はClean Architecture最外層（全層参照許可）
- 単体テストでもUIロジックテストに依存層が必要な場合あり

---

## Integration Tests参照関係

### Application.Integration.Tests（F#）

**原則**: 全層参照（WebApplicationFactory使用）

**参照設定**:
```xml
<ItemGroup>
  <!-- 統合テスト: 全層参照（WebApplicationFactory使用のため） -->
  <ProjectReference Include="..\..\src\UbiquitousLanguageManager.Application\UbiquitousLanguageManager.Application.fsproj" />
  <ProjectReference Include="..\..\src\UbiquitousLanguageManager.Domain\UbiquitousLanguageManager.Domain.fsproj" />
  <ProjectReference Include="..\..\src\UbiquitousLanguageManager.Infrastructure\UbiquitousLanguageManager.Infrastructure.csproj" />
  <ProjectReference Include="..\..\src\UbiquitousLanguageManager.Web\UbiquitousLanguageManager.Web.csproj" />
  <ProjectReference Include="..\..\src\UbiquitousLanguageManager.Contracts\UbiquitousLanguageManager.Contracts.csproj" />
</ItemGroup>
```

**理由**:
- WebApplicationFactory使用時は全層が必要
- Use Case統合テストでも依存層の実装が必要

---

### Infrastructure.Integration.Tests（C#）

**原則**: 全層参照（WebApplicationFactory使用）

**参照設定**:
```xml
<ItemGroup>
  <!-- 統合テスト: 全層参照（WebApplicationFactory使用のため） -->
  <ProjectReference Include="..\..\src\UbiquitousLanguageManager.Infrastructure\UbiquitousLanguageManager.Infrastructure.csproj" />
  <ProjectReference Include="..\..\src\UbiquitousLanguageManager.Application\UbiquitousLanguageManager.Application.fsproj" />
  <ProjectReference Include="..\..\src\UbiquitousLanguageManager.Domain\UbiquitousLanguageManager.Domain.fsproj" />
  <ProjectReference Include="..\..\src\UbiquitousLanguageManager.Web\UbiquitousLanguageManager.Web.csproj" />
  <ProjectReference Include="..\..\src\UbiquitousLanguageManager.Contracts\UbiquitousLanguageManager.Contracts.csproj" />
</ItemGroup>
```

**理由**:
- Repository統合テスト（データベース接続テスト）
- WebApplicationFactory使用時は全層が必要

---

## UI Tests参照関係

### Web.UI.Tests（C#・bUnit使用）

**原則**: Web層のみ参照（bUnitベストプラクティス）

**参照設定**:
```xml
<ItemGroup>
  <!-- UIテスト: Web層のみ参照（bUnitベストプラクティス） -->
  <ProjectReference Include="..\..\src\UbiquitousLanguageManager.Web\UbiquitousLanguageManager.Web.csproj" />
</ItemGroup>
```

**理由**:
- bUnitはBlazor Componentのみテスト
- 依存層のモック化推奨（bUnitベストプラクティス）

**⚠️ 依存層参照は非推奨**:
```xml
<!-- 依存層参照（非推奨・モック化推奨） -->
<ProjectReference Include="..\..\src\UbiquitousLanguageManager.Infrastructure\UbiquitousLanguageManager.Infrastructure.csproj" />
```

---

## E2E Tests参照関係

### Web.E2E.Tests（C#・Playwright使用）

**原則**: 全層参照

**参照設定**:
```xml
<ItemGroup>
  <!-- E2Eテスト: 全層参照 -->
  <ProjectReference Include="..\..\src\UbiquitousLanguageManager.Web\UbiquitousLanguageManager.Web.csproj" />
  <ProjectReference Include="..\..\src\UbiquitousLanguageManager.Infrastructure\UbiquitousLanguageManager.Infrastructure.csproj" />
  <ProjectReference Include="..\..\src\UbiquitousLanguageManager.Application\UbiquitousLanguageManager.Application.fsproj" />
  <ProjectReference Include="..\..\src\UbiquitousLanguageManager.Domain\UbiquitousLanguageManager.Domain.fsproj" />
  <ProjectReference Include="..\..\src\UbiquitousLanguageManager.Contracts\UbiquitousLanguageManager.Contracts.csproj" />
</ItemGroup>
```

**理由**:
- E2Eテストはアプリケーション全体のテスト
- Playwright使用時も全層が必要（WebApplicationFactory併用）

---

## 参照関係マトリックス

| TestProject | Domain | Application | Contracts | Infrastructure | Web |
|-------------|--------|-------------|-----------|----------------|-----|
| **Domain.Unit.Tests** | ✅ | ❌ | ❌ | ❌ | ❌ |
| **Application.Unit.Tests** | ✅ | ✅ | ❌ | ❌ | ❌ |
| **Contracts.Unit.Tests** | ✅ | ✅ | ✅ | ❌ | ❌ |
| **Infrastructure.Unit.Tests** | ✅ | ⚠️ | ❌ | ✅ | ❌ |
| **Web.Unit.Tests** | ✅ | ✅ | ✅ | ✅ | ✅ |
| **Application.Integration.Tests** | ✅ | ✅ | ✅ | ✅ | ✅ |
| **Infrastructure.Integration.Tests** | ✅ | ✅ | ✅ | ✅ | ✅ |
| **Web.UI.Tests** | ❌ | ❌ | ❌ | ❌ | ✅ |
| **Web.E2E.Tests** | ✅ | ✅ | ✅ | ✅ | ✅ |

**凡例**:
- ✅: 参照許可
- ⚠️: 非推奨（必要な場合のみ）
- ❌: 参照禁止

---

## 参照関係確認チェックリスト

### Unit Tests確認

- [ ] **Domain.Unit.Tests: Domain層のみ参照**
- [ ] **Application.Unit.Tests: Application + Domain参照**
- [ ] **Contracts.Unit.Tests: Contracts + Application + Domain参照**
- [ ] **Infrastructure.Unit.Tests: Infrastructure + Domain参照**
- [ ] **Web.Unit.Tests: Web + 必要な依存層参照**

### Integration Tests確認

- [ ] **Application.Integration.Tests: 全層参照**
- [ ] **Infrastructure.Integration.Tests: 全層参照**

### UI Tests確認

- [ ] **Web.UI.Tests: Web層のみ参照**

### E2E Tests確認

- [ ] **Web.E2E.Tests: 全層参照**

---

## 参照関係違反検出方法

### 方法1: .csproj/.fsproj手動確認

```bash
cat tests/{ProjectName}/*.csproj
cat tests/{ProjectName}/*.fsproj
```

**確認観点**: `<ProjectReference>` タグが参照関係原則に準拠

### 方法2: ビルドエラー確認

```bash
dotnet build
```

**エラーパターン**:
```
error CS0012: The type 'SomeType' is defined in an assembly that is not referenced.
```

**原因**: 参照関係不足

### 方法3: 循環依存検出

```bash
dotnet build
```

**エラーパターン**:
```
error CS0234: Circular dependency detected
```

**原因**: 参照関係循環

---

## 参照関係違反修正方法

### Step 1: .csproj/.fsproj編集

```xml
<!-- 修正前（違反） -->
<ItemGroup>
  <ProjectReference Include="..\..\src\UbiquitousLanguageManager.Domain\UbiquitousLanguageManager.Domain.fsproj" />
  <ProjectReference Include="..\..\src\UbiquitousLanguageManager.Application\UbiquitousLanguageManager.Application.fsproj" />
  <!-- Domain.Unit.Testsなのに Application層参照（違反） -->
</ItemGroup>

<!-- 修正後（準拠） -->
<ItemGroup>
  <ProjectReference Include="..\..\src\UbiquitousLanguageManager.Domain\UbiquitousLanguageManager.Domain.fsproj" />
  <!-- Domain層のみ参照（ADR_020準拠） -->
</ItemGroup>
```

### Step 2: ビルド確認

```bash
dotnet build
```

**期待結果**: 0 Warning / 0 Error

---

## WebApplicationFactory使用時の参照関係

### 原則

**WebApplicationFactory使用時は全層参照が必要**

**理由**:
- WebApplicationFactoryはASP.NET Core統合テスト用フレームワーク
- アプリケーション全体をホスティングするため全層が必要

**適用対象**:
- Application.Integration.Tests
- Infrastructure.Integration.Tests
- Web.E2E.Tests

**参照設定例**:
```xml
<ItemGroup>
  <!-- WebApplicationFactory使用: 全層参照 -->
  <ProjectReference Include="..\..\src\UbiquitousLanguageManager.Web\UbiquitousLanguageManager.Web.csproj" />
  <ProjectReference Include="..\..\src\UbiquitousLanguageManager.Infrastructure\UbiquitousLanguageManager.Infrastructure.csproj" />
  <ProjectReference Include="..\..\src\UbiquitousLanguageManager.Application\UbiquitousLanguageManager.Application.fsproj" />
  <ProjectReference Include="..\..\src\UbiquitousLanguageManager.Domain\UbiquitousLanguageManager.Domain.fsproj" />
  <ProjectReference Include="..\..\src\UbiquitousLanguageManager.Contracts\UbiquitousLanguageManager.Contracts.csproj" />
</ItemGroup>

<ItemGroup>
  <!-- WebApplicationFactory パッケージ -->
  <PackageReference Include="Microsoft.AspNetCore.Mvc.Testing" />
</ItemGroup>
```

---

## 関連ルール

- **test-project-naming-convention.md**: テストプロジェクト命名規則
- **new-test-project-checklist.md**: 新規テストプロジェクト作成チェックリスト

---

**作成日**: 2025-11-01
**Phase B-F2 Step2**: Agent Skills Phase 2展開
**参照**: ADR_020, 新規テストプロジェクト作成ガイドライン, Clean Architecture原則
