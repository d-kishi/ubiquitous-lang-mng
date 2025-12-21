# テストアーキテクチャ規約・パターン詳細

## 目次

- [言語選択原則](#言語選択原則)
- [NuGetパッケージ標準セット](#nugetパッケージ標準セット)
- [違反検出・修正パターン](#違反検出修正パターン)
- [品質目標](#品質目標)

---

## 言語選択原則

### レイヤー別言語選択

| Layer | 言語 | 理由 |
|-------|------|------|
| Domain | F# | ドメインロジックはF#で実装 |
| Application | F# | ユースケースはF#で実装 |
| Contracts | C# | DTOs・Type ConvertersはC#で実装 |
| Infrastructure | C# | EF Core・RepositoriesはC#で実装 |
| Web | C# | Blazor ServerはC#で実装 |

### SDK選択原則

| TestType | SDK | 理由 |
|----------|-----|------|
| Unit | `Microsoft.NET.Sdk` | 標準テスト（デフォルト） |
| Integration | `Microsoft.NET.Sdk` | 標準テスト |
| UI (bUnit) | `Microsoft.NET.Sdk.Razor` | **Blazor Componentテストのため必須** |
| E2E (Playwright) | `Microsoft.NET.Sdk` | 標準テスト |

---

## NuGetパッケージ標準セット

### F# Unit Tests

```bash
dotnet add package xunit
dotnet add package xunit.runner.visualstudio
dotnet add package Microsoft.NET.Test.Sdk
dotnet add package FsUnit.xUnit
dotnet add package coverlet.collector
```

### C# Unit Tests

```bash
dotnet add package xunit
dotnet add package xunit.runner.visualstudio
dotnet add package Microsoft.NET.Test.Sdk
dotnet add package FluentAssertions
dotnet add package Moq
dotnet add package coverlet.collector
```

### Integration Tests

```bash
dotnet add package Microsoft.AspNetCore.Mvc.Testing
dotnet add package Microsoft.EntityFrameworkCore.InMemory
dotnet add package Testcontainers.PostgreSql
```

### E2E Tests（Playwright）

```bash
dotnet add package Microsoft.Playwright
dotnet add package Microsoft.AspNetCore.Mvc.Testing
```

---

## 違反検出・修正パターン

### 違反例1: 命名規則違反

**検出**:
```bash
# NG例
UbiquitousLanguageManager.DomainTests
UbiquitousLanguageManager.Tests
Domain.Unit.Tests
```

**修正**:
```bash
# OK例
UbiquitousLanguageManager.Domain.Unit.Tests
```

**理由**: ADR_020命名規則 `{ProjectName}.{Layer}.{TestType}.Tests` 厳守

### 違反例2: 参照関係違反

**検出**:
```xml
<!-- Domain.Unit.Tests が Application層を参照（違反） -->
<ItemGroup>
  <ProjectReference Include="..\..\src\UbiquitousLanguageManager.Domain\..." />
  <ProjectReference Include="..\..\src\UbiquitousLanguageManager.Application\..." />
</ItemGroup>
```

**修正**:
```xml
<!-- Domain.Unit.Tests は Domain層のみ参照 -->
<ItemGroup>
  <ProjectReference Include="..\..\src\UbiquitousLanguageManager.Domain\..." />
</ItemGroup>
```

### 違反例3: レイヤー混在

**検出**:
```
tests/UbiquitousLanguageManager.Domain.Unit.Tests/
  ├── DomainTests/
  └── ApplicationTests/  # 混在
```

**修正**:
```
tests/UbiquitousLanguageManager.Domain.Unit.Tests/
  └── DomainTests/

tests/UbiquitousLanguageManager.Application.Unit.Tests/
  └── ApplicationTests/
```

---

## 品質目標

### テストカバレッジ目標

```yaml
目標カバレッジ:
  - 単体テスト: 97%以上
  - 統合テスト: 85%以上

測定方法:
  - dotnet test --collect:"XPlat Code Coverage"
```

### ビルド品質目標

```yaml
目標品質:
  - 0 Warning / 0 Error（厳守）
  - ビルド時間最小化（不要な参照削除）
```
