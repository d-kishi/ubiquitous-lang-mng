# テストプロジェクトアーキテクチャ（ADR_020準拠）

## 概要

ADR_020テストアーキテクチャ決定に基づく**命名規則・参照関係原則**の統合ルール。

---

## 🔴 CRITICAL: 命名規則

### 基本テンプレート

```
UbiquitousLanguageManager.{Layer}.{TestType}.Tests
```

| 要素 | 選択肢 |
|------|--------|
| **Layer** | Domain / Application / Contracts / Infrastructure / Web |
| **TestType** | Unit / Integration / UI / E2E |

### Layer×TestType組み合わせ

| Layer | 言語 | Unit | Integration | UI | E2E |
|-------|------|------|-------------|----|----|
| **Domain** | F# | ✅ | - | - | - |
| **Application** | F# | ✅ | ✅ | - | - |
| **Contracts** | C# | ✅ | - | - | - |
| **Infrastructure** | C# | ✅ | ✅ | - | - |
| **Web** | C# | ✅ | - | ✅ | ✅ |

### 命名規則違反パターン（禁止）

| 違反例 | 問題 |
|--------|------|
| `UbiquitousLanguageManager.Tests` | Layer/TestType欠落 |
| `UbiquitousLanguageManager.Unit.Tests` | Layer欠落 |
| `UbiquitousLanguageManager.Domain.Tests` | TestType欠落 |
| `UbiquitousLanguageManager.Unit.Domain.Tests` | 順序違反 |

---

## 🔴 CRITICAL: 参照関係原則

### 基本原則

```yaml
Unit Tests: テスト対象レイヤーのみ参照（最小化原則）
Integration Tests: 全層参照（WebApplicationFactory使用）
UI Tests: Web層のみ参照（bUnitベストプラクティス）
E2E Tests: 全層参照（Playwright使用）
```

### 参照関係マトリックス

| TestProject | Domain | Application | Contracts | Infrastructure | Web |
|-------------|:------:|:-----------:|:---------:|:--------------:|:---:|
| **Domain.Unit** | ✅ | ❌ | ❌ | ❌ | ❌ |
| **Application.Unit** | ✅ | ✅ | ❌ | ❌ | ❌ |
| **Contracts.Unit** | ✅ | ✅ | ✅ | ❌ | ❌ |
| **Infrastructure.Unit** | ✅ | ⚠️ | ❌ | ✅ | ❌ |
| **Web.Unit** | ✅ | ✅ | ✅ | ✅ | ✅ |
| **Application.Integration** | ✅ | ✅ | ✅ | ✅ | ✅ |
| **Infrastructure.Integration** | ✅ | ✅ | ✅ | ✅ | ✅ |
| **Web.UI** | ❌ | ❌ | ❌ | ❌ | ✅ |
| **Web.E2E** | ✅ | ✅ | ✅ | ✅ | ✅ |

**凡例**: ✅許可 / ⚠️非推奨 / ❌禁止

---

## ディレクトリ構造

```
tests/
├── UbiquitousLanguageManager.Domain.Unit.Tests/          (F#)
├── UbiquitousLanguageManager.Application.Unit.Tests/     (F#)
├── UbiquitousLanguageManager.Contracts.Unit.Tests/       (C#)
├── UbiquitousLanguageManager.Infrastructure.Unit.Tests/  (C#)
├── UbiquitousLanguageManager.Infrastructure.Integration.Tests/ (C#)
├── UbiquitousLanguageManager.Web.Unit.Tests/             (C#)
├── UbiquitousLanguageManager.Web.UI.Tests/               (C#, bUnit)
└── UbiquitousLanguageManager.Web.E2E.Tests/              (C#, Playwright)
```

**重要**: ディレクトリ名とプロジェクト名は完全一致

---

## Unit Tests参照設定例

### Domain.Unit.Tests（F#）

```xml
<ItemGroup>
  <ProjectReference Include="..\..\src\UbiquitousLanguageManager.Domain\UbiquitousLanguageManager.Domain.fsproj" />
</ItemGroup>
```

### Application.Unit.Tests（F#）

```xml
<ItemGroup>
  <ProjectReference Include="..\..\src\UbiquitousLanguageManager.Application\UbiquitousLanguageManager.Application.fsproj" />
  <ProjectReference Include="..\..\src\UbiquitousLanguageManager.Domain\UbiquitousLanguageManager.Domain.fsproj" />
</ItemGroup>
```

### Contracts.Unit.Tests（C#）

```xml
<ItemGroup>
  <ProjectReference Include="..\..\src\UbiquitousLanguageManager.Contracts\UbiquitousLanguageManager.Contracts.csproj" />
  <ProjectReference Include="..\..\src\UbiquitousLanguageManager.Application\UbiquitousLanguageManager.Application.fsproj" />
  <ProjectReference Include="..\..\src\UbiquitousLanguageManager.Domain\UbiquitousLanguageManager.Domain.fsproj" />
</ItemGroup>
```

### Infrastructure.Unit.Tests（C#）

```xml
<ItemGroup>
  <ProjectReference Include="..\..\src\UbiquitousLanguageManager.Infrastructure\UbiquitousLanguageManager.Infrastructure.csproj" />
  <ProjectReference Include="..\..\src\UbiquitousLanguageManager.Domain\UbiquitousLanguageManager.Domain.fsproj" />
</ItemGroup>
```

---

## Integration/E2E Tests参照設定

### 全層参照パターン（Integration/E2E共通）

```xml
<ItemGroup>
  <ProjectReference Include="..\..\src\UbiquitousLanguageManager.Web\UbiquitousLanguageManager.Web.csproj" />
  <ProjectReference Include="..\..\src\UbiquitousLanguageManager.Infrastructure\UbiquitousLanguageManager.Infrastructure.csproj" />
  <ProjectReference Include="..\..\src\UbiquitousLanguageManager.Application\UbiquitousLanguageManager.Application.fsproj" />
  <ProjectReference Include="..\..\src\UbiquitousLanguageManager.Domain\UbiquitousLanguageManager.Domain.fsproj" />
  <ProjectReference Include="..\..\src\UbiquitousLanguageManager.Contracts\UbiquitousLanguageManager.Contracts.csproj" />
</ItemGroup>
```

### Web.UI.Tests（bUnit・Web層のみ）

```xml
<ItemGroup>
  <ProjectReference Include="..\..\src\UbiquitousLanguageManager.Web\UbiquitousLanguageManager.Web.csproj" />
</ItemGroup>
```

**注意**: UI TestsはSDKを`Microsoft.NET.Sdk.Razor`に手動変更必須

---

## 確認チェックリスト

### 命名規則確認

- [ ] `{Layer}` = Domain/Application/Contracts/Infrastructure/Web
- [ ] `{TestType}` = Unit/Integration/UI/E2E
- [ ] `.Tests`サフィックス付与
- [ ] ディレクトリ名とプロジェクト名の一致

### 参照関係確認

- [ ] Unit Tests: テスト対象レイヤーのみ参照
- [ ] Integration/E2E Tests: 全層参照
- [ ] UI Tests: Web層のみ参照
- [ ] 循環依存なし確認

---

## 関連ドキュメント

- **ADR_020**: テストアーキテクチャ決定
- **new-test-project-checklist.md**: 新規プロジェクト作成チェックリスト

---

**作成日**: 2025-12-18
**統合元**: test-project-naming-convention.md, test-project-reference-rules.md
