# テストプロジェクト命名規則（ADR_020準拠）

## 概要

ADR_020テストアーキテクチャ決定に基づく**テストプロジェクト命名規則**を定義します。命名規則違反は、テストアーキテクチャ整合性崩壊・ビルドエラー・保守性低下の原因となるため、**厳格遵守**が必要です。

---

## 🔴 CRITICAL: 命名規則絶対原則

### 基本テンプレート

```
UbiquitousLanguageManager.{Layer}.{TestType}.Tests
```

**すべてのテストプロジェクトはこのテンプレートに従う（例外なし）**

---

## Layer選択（5種類）

### Domain層

**対象**: ドメインモデル・Value Objects・Domain Services

**命名例**:
```
UbiquitousLanguageManager.Domain.Unit.Tests
```

**言語**: F#（ドメインロジックはF#で実装）

**参照範囲**: Domain層のみ（Unit Tests）

---

### Application層

**対象**: Use Cases・Application Services

**命名例**:
```
UbiquitousLanguageManager.Application.Unit.Tests
UbiquitousLanguageManager.Application.Integration.Tests
```

**言語**: F#（ユースケースはF#で実装）

**参照範囲**:
- Unit Tests: Application + Domain
- Integration Tests: 全層

---

### Contracts層

**対象**: DTOs・Type Converters（F#↔C#境界）

**命名例**:
```
UbiquitousLanguageManager.Contracts.Unit.Tests
```

**言語**: C#（DTOs・Type ConvertersはC#で実装）

**参照範囲**: Contracts + Application + Domain

---

### Infrastructure層

**対象**: Repositories・EF Core・外部連携

**命名例**:
```
UbiquitousLanguageManager.Infrastructure.Unit.Tests
UbiquitousLanguageManager.Infrastructure.Integration.Tests
```

**言語**: C#（EF Core・RepositoriesはC#で実装）

**参照範囲**:
- Unit Tests: Infrastructure + Domain
- Integration Tests: 全層

---

### Web層

**対象**: Blazor Components・Pages・Web Services

**命名例**:
```
UbiquitousLanguageManager.Web.Unit.Tests
UbiquitousLanguageManager.Web.UI.Tests
UbiquitousLanguageManager.Web.E2E.Tests
```

**言語**: C#（Blazor ServerはC#で実装）

**参照範囲**:
- Unit Tests: Web + 必要な依存層
- UI Tests: Web層のみ（bUnit推奨）
- E2E Tests: 全層

---

## TestType選択（4種類）

### Unit Tests

**目的**: 単体テスト（テスト対象レイヤーのみ参照）

**命名サフィックス**: `.Unit.Tests`

**例**:
```
UbiquitousLanguageManager.Domain.Unit.Tests
UbiquitousLanguageManager.Application.Unit.Tests
UbiquitousLanguageManager.Contracts.Unit.Tests
UbiquitousLanguageManager.Infrastructure.Unit.Tests
UbiquitousLanguageManager.Web.Unit.Tests
```

**参照原則**: テスト対象レイヤーのみ参照（最小化原則）

---

### Integration Tests

**目的**: 統合テスト（複数レイヤー・DB・外部連携）

**命名サフィックス**: `.Integration.Tests`

**例**:
```
UbiquitousLanguageManager.Application.Integration.Tests
UbiquitousLanguageManager.Infrastructure.Integration.Tests
```

**参照原則**: 必要な依存層のみ参照（WebApplicationFactory使用時は全層参照）

---

### UI Tests

**目的**: UIコンポーネントテスト（Blazor bUnit使用）

**命名サフィックス**: `.UI.Tests`

**例**:
```
UbiquitousLanguageManager.Web.UI.Tests
```

**参照原則**: Web層のみ参照（bUnitベストプラクティス）

**SDK**: `Microsoft.NET.Sdk.Razor`（**手動変更必須**）

---

### E2E Tests

**目的**: エンドツーエンドテスト（Playwright使用）

**命名サフィックス**: `.E2E.Tests`

**例**:
```
UbiquitousLanguageManager.Web.E2E.Tests
```

**参照原則**: 全層参照

**SDK**: `Microsoft.NET.Sdk`

---

## 命名規則確認チェックリスト

### 基本テンプレート確認

- [ ] **`{ProjectName}` = `UbiquitousLanguageManager` 確認**
- [ ] **`{Layer}` = Domain/Application/Contracts/Infrastructure/Web のいずれか**
- [ ] **`{TestType}` = Unit/Integration/UI/E2E のいずれか**
- [ ] **`.Tests` サフィックス確認**
- [ ] **ディレクトリ名とプロジェクト名の一致確認**

### 命名規則違反パターン（絶対禁止）

❌ **プロジェクト名のみ（Layer/TestType欠落）**:
```
UbiquitousLanguageManager.Tests
```

❌ **Layer欠落**:
```
UbiquitousLanguageManager.Unit.Tests
```

❌ **TestType欠落**:
```
UbiquitousLanguageManager.Domain.Tests
```

❌ **順序違反（TestType → Layer）**:
```
UbiquitousLanguageManager.Unit.Domain.Tests
```

❌ **サフィックス欠落**:
```
UbiquitousLanguageManager.Domain.Unit
```

❌ **短縮形使用**:
```
ULM.Domain.Unit.Tests
UbiquitousLanguageManager.Dom.Unit.Tests
```

---

## 正しい命名例（全パターン）

### Domain層

```
✅ UbiquitousLanguageManager.Domain.Unit.Tests
```

### Application層

```
✅ UbiquitousLanguageManager.Application.Unit.Tests
✅ UbiquitousLanguageManager.Application.Integration.Tests
```

### Contracts層

```
✅ UbiquitousLanguageManager.Contracts.Unit.Tests
```

### Infrastructure層

```
✅ UbiquitousLanguageManager.Infrastructure.Unit.Tests
✅ UbiquitousLanguageManager.Infrastructure.Integration.Tests
```

### Web層

```
✅ UbiquitousLanguageManager.Web.Unit.Tests
✅ UbiquitousLanguageManager.Web.UI.Tests
✅ UbiquitousLanguageManager.Web.E2E.Tests
```

---

## ディレクトリ構造との対応

### 基本構造

```
tests/
├── UbiquitousLanguageManager.Domain.Unit.Tests/
│   ├── UbiquitousLanguageManager.Domain.Unit.Tests.fsproj
│   └── （F#テストファイル）
├── UbiquitousLanguageManager.Application.Unit.Tests/
│   ├── UbiquitousLanguageManager.Application.Unit.Tests.fsproj
│   └── （F#テストファイル）
├── UbiquitousLanguageManager.Contracts.Unit.Tests/
│   ├── UbiquitousLanguageManager.Contracts.Unit.Tests.csproj
│   └── （C#テストファイル）
├── UbiquitousLanguageManager.Infrastructure.Unit.Tests/
│   ├── UbiquitousLanguageManager.Infrastructure.Unit.Tests.csproj
│   └── （C#テストファイル）
├── UbiquitousLanguageManager.Infrastructure.Integration.Tests/
│   ├── UbiquitousLanguageManager.Infrastructure.Integration.Tests.csproj
│   └── （C#テストファイル）
├── UbiquitousLanguageManager.Web.Unit.Tests/
│   ├── UbiquitousLanguageManager.Web.Unit.Tests.csproj
│   └── （C#テストファイル）
├── UbiquitousLanguageManager.Web.UI.Tests/
│   ├── UbiquitousLanguageManager.Web.UI.Tests.csproj
│   └── （C#テストファイル）
└── UbiquitousLanguageManager.Web.E2E.Tests/
    ├── UbiquitousLanguageManager.Web.E2E.Tests.csproj
    └── （C#テストファイル）
```

**重要**: ディレクトリ名とプロジェクト名は完全一致（大文字小文字含む）

---

## プロジェクト作成コマンドテンプレート

### F#プロジェクト（Domain/Application層）

```bash
dotnet new xunit -lang F# -n UbiquitousLanguageManager.{Layer}.{TestType}.Tests -o tests/UbiquitousLanguageManager.{Layer}.{TestType}.Tests
```

**例（Domain.Unit.Tests）**:
```bash
dotnet new xunit -lang F# -n UbiquitousLanguageManager.Domain.Unit.Tests -o tests/UbiquitousLanguageManager.Domain.Unit.Tests
```

### C#プロジェクト（Contracts/Infrastructure/Web層）

```bash
dotnet new xunit -n UbiquitousLanguageManager.{Layer}.{TestType}.Tests -o tests/UbiquitousLanguageManager.{Layer}.{TestType}.Tests
```

**例（Infrastructure.Integration.Tests）**:
```bash
dotnet new xunit -n UbiquitousLanguageManager.Infrastructure.Integration.Tests -o tests/UbiquitousLanguageManager.Infrastructure.Integration.Tests
```

---

## 命名規則違反検出方法

### 方法1: ソリューション一覧確認

```bash
dotnet sln list
```

**確認観点**: すべてのテストプロジェクトが `UbiquitousLanguageManager.{Layer}.{TestType}.Tests` 形式

### 方法2: tests/ディレクトリ一覧確認

```bash
ls tests/
```

**確認観点**: すべてのディレクトリ名が命名規則に準拠

### 方法3: .csproj/.fsprojファイル名確認

```bash
find tests/ -name "*.csproj" -o -name "*.fsproj"
```

**確認観点**: すべてのプロジェクトファイル名が命名規則に準拠

---

## 命名規則違反修正方法

### Step 1: プロジェクト名変更

```bash
# 古いプロジェクト削除
dotnet sln remove tests/{OldProjectName}
rm -rf tests/{OldProjectName}

# 新しいプロジェクト作成（正しい命名規則）
dotnet new xunit -n UbiquitousLanguageManager.{Layer}.{TestType}.Tests -o tests/UbiquitousLanguageManager.{Layer}.{TestType}.Tests

# ソリューション追加
dotnet sln add tests/UbiquitousLanguageManager.{Layer}.{TestType}.Tests
```

### Step 2: 参照関係再設定

**詳細**: [`test-project-reference-rules.md`](./test-project-reference-rules.md)

### Step 3: ビルド確認

```bash
dotnet build
```

**期待結果**: 0 Warning / 0 Error

---

## 関連ルール

- **test-project-reference-rules.md**: テストプロジェクト参照関係原則
- **new-test-project-checklist.md**: 新規テストプロジェクト作成チェックリスト

---

**作成日**: 2025-11-01
**Phase B-F2 Step2**: Agent Skills Phase 2展開
**参照**: ADR_020, 新規テストプロジェクト作成ガイドライン
