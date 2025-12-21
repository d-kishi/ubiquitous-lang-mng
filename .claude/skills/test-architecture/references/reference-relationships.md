# 参照関係原則詳細（ADR_020準拠）

## 目次

- [Unit Tests参照関係](#unit-tests参照関係)
- [Integration Tests参照関係](#integration-tests参照関係)
- [E2E Tests参照関係](#e2e-tests参照関係)

---

## Unit Tests参照関係

**原則**: テスト対象レイヤーのみ参照（最小化原則）

### Domain.Unit.Tests（F#）

```xml
<ItemGroup>
  <!-- ADR_020準拠: Unit Tests原則 - テスト対象レイヤーのみ参照 -->
  <ProjectReference Include="..\..\src\UbiquitousLanguageManager.Domain\UbiquitousLanguageManager.Domain.fsproj" />
</ItemGroup>
```

### Application.Unit.Tests（F#）

```xml
<ItemGroup>
  <!-- Application層はDomain層に依存するため、両方参照 -->
  <ProjectReference Include="..\..\src\UbiquitousLanguageManager.Application\UbiquitousLanguageManager.Application.fsproj" />
  <ProjectReference Include="..\..\src\UbiquitousLanguageManager.Domain\UbiquitousLanguageManager.Domain.fsproj" />
</ItemGroup>
```

### Contracts.Unit.Tests（C#）

```xml
<ItemGroup>
  <!-- Contracts層はApplication・Domain層に依存するため、3層参照 -->
  <ProjectReference Include="..\..\src\UbiquitousLanguageManager.Contracts\UbiquitousLanguageManager.Contracts.csproj" />
  <ProjectReference Include="..\..\src\UbiquitousLanguageManager.Application\UbiquitousLanguageManager.Application.fsproj" />
  <ProjectReference Include="..\..\src\UbiquitousLanguageManager.Domain\UbiquitousLanguageManager.Domain.fsproj" />
</ItemGroup>
```

---

## Integration Tests参照関係

**原則**: 必要な依存層のみ参照（WebApplicationFactory使用時は全層参照）

### Infrastructure.Integration.Tests（C#）

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

---

## E2E Tests参照関係

**原則**: 全層参照可・Playwright使用

### Web.E2E.Tests（C#）

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
