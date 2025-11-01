# 新規テストプロジェクト作成チェックリスト

## 概要

新規テストプロジェクト作成時の必須確認事項を段階別にチェックリスト化したものです。Issue #40（テストアーキテクチャ再構成）の教訓を組み込み、ADR_020準拠の高品質なテストプロジェクト作成を保証します。

---

## 🎯 このチェックリストの目的

### 目的
- テストアーキテクチャ整合性維持（ADR_020準拠）
- Issue #40類似問題の再発防止
- 新規テストプロジェクト作成の標準化
- unit-test/integration-test Agentの自律的作業支援

### 適用タイミング
- **新規テストプロジェクト作成前**（必須）
- unit-test/integration-test Agent選択時
- tests/配下に新規ディレクトリ・プロジェクトファイル作成前

---

## Phase 1: 事前確認チェックリスト

### 必須ドキュメント確認

- [ ] **ADR_020テストアーキテクチャ決定確認**
  - 場所: `/Doc/07_Decisions/ADR_020_テストアーキテクチャ決定.md`
  - 確認内容: レイヤー×テストタイプ分離方式の理解
  - 所要時間: 5分

- [ ] **テストアーキテクチャ設計書確認**
  - 場所: `/Doc/02_Design/テストアーキテクチャ設計書.md`
  - 確認内容: プロジェクト構成図・命名規則・参照関係原則
  - 所要時間: 10分

- [ ] **新規プロジェクト作成ガイドライン確認**
  - 場所: `/Doc/08_Organization/Rules/新規テストプロジェクト作成ガイドライン.md`
  - 確認内容: 詳細手順・標準パッケージ・確認コマンド
  - 所要時間: 10分

### 既存プロジェクト重複確認

- [ ] **同一レイヤー・同一テストタイプのプロジェクト存在確認**
  - 方法: `dotnet sln list` または `tests/` ディレクトリ一覧確認
  - 確認観点: 重複プロジェクトがないことを確認
  - 例: Domain.Unit.Tests作成前に既存Domain.Unit.Testsがないか確認

- [ ] **類似プロジェクト確認**
  - 方法: テストアーキテクチャ設計書のプロジェクト一覧表確認
  - 確認観点: 作成予定プロジェクトとの整合性確認

### レイヤー・テストタイプ分類明確化

- [ ] **Layer選択明確化**
  - 選択肢: Domain / Application / Contracts / Infrastructure / Web
  - 確認: 作成するテストプロジェクトの対象レイヤーを明確化

- [ ] **TestType選択明確化**
  - 選択肢: Unit / Integration / UI / E2E
  - 確認: 作成するテストプロジェクトのテストタイプを明確化

**Phase 1完了判定**: すべてのチェック項目が✅

---

## Phase 2: プロジェクト作成チェックリスト

### Step 1: プロジェクト作成コマンド実行

- [ ] **F#プロジェクト作成（Domain/Application層）**
  ```bash
  dotnet new xunit -lang F# -n UbiquitousLanguageManager.{Layer}.{TestType}.Tests -o tests/UbiquitousLanguageManager.{Layer}.{TestType}.Tests
  ```
  - 例: `dotnet new xunit -lang F# -n UbiquitousLanguageManager.Domain.Unit.Tests -o tests/UbiquitousLanguageManager.Domain.Unit.Tests`

- [ ] **C#プロジェクト作成（Contracts/Infrastructure/Web層）**
  ```bash
  dotnet new xunit -n UbiquitousLanguageManager.{Layer}.{TestType}.Tests -o tests/UbiquitousLanguageManager.{Layer}.{TestType}.Tests
  ```
  - 例: `dotnet new xunit -n UbiquitousLanguageManager.Infrastructure.Integration.Tests -o tests/UbiquitousLanguageManager.Infrastructure.Integration.Tests`

### Step 2: 命名規則確認

- [ ] **`{ProjectName}` = `UbiquitousLanguageManager` 確認**
- [ ] **`{Layer}` = Domain/Application/Contracts/Infrastructure/Web のいずれか**
- [ ] **`{TestType}` = Unit/Integration/UI/E2E のいずれか**
- [ ] **`.Tests` サフィックス確認**
- [ ] **ディレクトリ名とプロジェクト名の一致確認**

**命名規則テンプレート**: `UbiquitousLanguageManager.{Layer}.{TestType}.Tests`

### Step 3: 言語・SDK選択確認

- [ ] **レイヤー別言語選択確認**
  | Layer | 言語 | 確認方法 |
  |-------|------|---------|
  | Domain | F# | .fsproj拡張子確認 |
  | Application | F# | .fsproj拡張子確認 |
  | Contracts | C# | .csproj拡張子確認 |
  | Infrastructure | C# | .csproj拡張子確認 |
  | Web | C# | .csproj拡張子確認 |

- [ ] **SDK選択確認（UI Testsのみ手動変更必要）**
  | TestType | SDK | 確認方法 |
  |----------|-----|---------|
  | Unit | `Microsoft.NET.Sdk` | .csproj/.fsproj内確認 |
  | Integration | `Microsoft.NET.Sdk` | .csproj/.fsproj内確認 |
  | UI (bUnit) | `Microsoft.NET.Sdk.Razor` | **手動変更必須** |
  | E2E | `Microsoft.NET.Sdk` | .csproj/.fsproj内確認 |

**Phase 2完了判定**: すべてのチェック項目が✅

---

## Phase 3: 参照関係設定チェックリスト

### Unit Tests参照関係設定

- [ ] **Domain.Unit.Tests: Domain層のみ参照**
  ```xml
  <ItemGroup>
    <ProjectReference Include="..\..\src\UbiquitousLanguageManager.Domain\UbiquitousLanguageManager.Domain.fsproj" />
  </ItemGroup>
  ```

- [ ] **Application.Unit.Tests: Application + Domain参照**
  ```xml
  <ItemGroup>
    <ProjectReference Include="..\..\src\UbiquitousLanguageManager.Application\UbiquitousLanguageManager.Application.fsproj" />
    <ProjectReference Include="..\..\src\UbiquitousLanguageManager.Domain\UbiquitousLanguageManager.Domain.fsproj" />
  </ItemGroup>
  ```

- [ ] **Contracts.Unit.Tests: Contracts + Application + Domain参照**
  ```xml
  <ItemGroup>
    <ProjectReference Include="..\..\src\UbiquitousLanguageManager.Contracts\UbiquitousLanguageManager.Contracts.csproj" />
    <ProjectReference Include="..\..\src\UbiquitousLanguageManager.Application\UbiquitousLanguageManager.Application.fsproj" />
    <ProjectReference Include="..\..\src\UbiquitousLanguageManager.Domain\UbiquitousLanguageManager.Domain.fsproj" />
  </ItemGroup>
  ```

- [ ] **Infrastructure.Unit.Tests: Infrastructure + Domain参照**
- [ ] **Web.Unit.Tests: Web + 必要な依存層参照**

### Integration Tests参照関係設定

- [ ] **Infrastructure.Integration.Tests: 全層参照（WebApplicationFactory使用）**
  ```xml
  <ItemGroup>
    <ProjectReference Include="..\..\src\UbiquitousLanguageManager.Infrastructure\UbiquitousLanguageManager.Infrastructure.csproj" />
    <ProjectReference Include="..\..\src\UbiquitousLanguageManager.Application\UbiquitousLanguageManager.Application.fsproj" />
    <ProjectReference Include="..\..\src\UbiquitousLanguageManager.Domain\UbiquitousLanguageManager.Domain.fsproj" />
    <ProjectReference Include="..\..\src\UbiquitousLanguageManager.Web\UbiquitousLanguageManager.Web.csproj" />
    <ProjectReference Include="..\..\src\UbiquitousLanguageManager.Contracts\UbiquitousLanguageManager.Contracts.csproj" />
  </ItemGroup>
  ```

### E2E Tests参照関係設定

- [ ] **Web.E2E.Tests: 全層参照**
  ```xml
  <ItemGroup>
    <ProjectReference Include="..\..\src\UbiquitousLanguageManager.Web\UbiquitousLanguageManager.Web.csproj" />
    <ProjectReference Include="..\..\src\UbiquitousLanguageManager.Infrastructure\UbiquitousLanguageManager.Infrastructure.csproj" />
    <ProjectReference Include="..\..\src\UbiquitousLanguageManager.Application\UbiquitousLanguageManager.Application.fsproj" />
    <ProjectReference Include="..\..\src\UbiquitousLanguageManager.Domain\UbiquitousLanguageManager.Domain.fsproj" />
    <ProjectReference Include="..\..\src\UbiquitousLanguageManager.Contracts\UbiquitousLanguageManager.Contracts.csproj" />
  </ItemGroup>
  ```

### 参照関係確認

- [ ] **テスト対象レイヤー参照確認**: Unit Testsはテスト対象レイヤーのみ（最小化原則）
- [ ] **不要な参照の削除確認**: 循環依存防止・ビルド時間最小化
- [ ] **ADR_020参照関係原則準拠確認**: テストタイプ別の参照関係原則に準拠

**Phase 3完了判定**: すべてのチェック項目が✅

---

## Phase 4: NuGetパッケージ追加チェックリスト

### F# Unit Tests標準パッケージ

- [ ] **xUnit本体**
  ```bash
  dotnet add package xunit
  dotnet add package xunit.runner.visualstudio
  dotnet add package Microsoft.NET.Test.Sdk
  ```

- [ ] **F#テストユーティリティ**
  ```bash
  dotnet add package FsUnit.xUnit
  ```

- [ ] **カバレッジ測定**
  ```bash
  dotnet add package coverlet.collector
  ```

### C# Unit Tests標準パッケージ

- [ ] **xUnit本体**
  ```bash
  dotnet add package xunit
  dotnet add package xunit.runner.visualstudio
  dotnet add package Microsoft.NET.Test.Sdk
  ```

- [ ] **アサーション・モック**
  ```bash
  dotnet add package FluentAssertions
  dotnet add package Moq
  ```

- [ ] **カバレッジ測定**
  ```bash
  dotnet add package coverlet.collector
  ```

### Integration Tests標準パッケージ

- [ ] **xUnit本体**（上記と同様）

- [ ] **統合テスト用パッケージ**
  ```bash
  dotnet add package Microsoft.AspNetCore.Mvc.Testing
  dotnet add package Microsoft.EntityFrameworkCore.InMemory
  dotnet add package Testcontainers.PostgreSql
  ```

### E2E Tests（Playwright）標準パッケージ

- [ ] **xUnit本体**（上記と同様）

- [ ] **Playwright**
  ```bash
  dotnet add package Microsoft.Playwright
  dotnet add package Microsoft.AspNetCore.Mvc.Testing
  ```

**Phase 4完了判定**: すべてのチェック項目が✅

---

## Phase 5: ビルド・実行確認チェックリスト

### ソリューションファイル更新

- [ ] **`dotnet sln add` 実行完了**
  ```bash
  dotnet sln add tests/{ProjectName}.{Layer}.{TestType}.Tests
  ```

- [ ] **`dotnet sln list` で新規プロジェクト確認**
  - 期待結果: 新規プロジェクトがソリューション一覧に表示される

### ビルド確認

- [ ] **新規プロジェクト個別ビルド成功（0 Warning/0 Error）**
  ```bash
  dotnet build tests/{ProjectName}.{Layer}.{TestType}.Tests
  ```

- [ ] **ソリューション全体ビルド成功（0 Warning/0 Error）**
  ```bash
  dotnet build
  ```
  - 重要: 既存プロジェクトへの影響がないこと確認

### テスト実行確認

- [ ] **新規プロジェクト個別テスト実行成功**
  ```bash
  dotnet test tests/{ProjectName}.{Layer}.{TestType}.Tests
  ```
  - 期待結果: テスト実行成功（0件でもOK・サンプルテスト追加推奨）

- [ ] **ソリューション全体テスト実行成功（100%維持）**
  ```bash
  dotnet test
  ```
  - 期待結果: 全テスト100%成功

**Phase 5完了判定**: すべてのチェック項目が✅

---

## Phase 6: Issue #40再発防止チェックリスト

### 技術負債回避

- [ ] **EnableDefaultCompileItems=false設定禁止**
  - 確認: .csproj/.fsprojに `<EnableDefaultCompileItems>false</EnableDefaultCompileItems>` が存在しないこと
  - 理由: Issue #40でF#/C#混在環境でのビルドエラー暫定対応として使用され、技術負債化

- [ ] **F#/C#混在回避**
  - 確認: F#プロジェクトにC#ファイルを含めない・C#プロジェクトにF#ファイルを含めない
  - 理由: F#コンパイラはC#ファイルをコンパイルできない（過去に7件のC#→F#変換が必要だった）

- [ ] **テストタイプ混在回避**
  - 確認: 1プロジェクト内に複数テストタイプを混在させない
  - 理由: テスト実行粒度制御不可・CI/CD最適化困難

- [ ] **レイヤー混在回避**
  - 確認: 1プロジェクト内に複数レイヤーのテストを混在させない
  - 理由: 責務分離の原則違反・影響範囲不明確・保守性低下

### 設計原則確認

- [ ] **レイヤー別分離確認**
  - 確認: 1プロジェクト = 1レイヤー × 1テストタイプ
  - 確認: プロジェクト名が `{ProjectName}.{Layer}.{TestType}.Tests` 形式

- [ ] **参照関係最小化**
  - 確認: Unit Testsはテスト対象レイヤーのみ参照
  - 確認: 不要な参照を追加しない（ビルド時間増加・循環依存リスク）

- [ ] **命名規則準拠**
  - 確認: `{ProjectName}.{Layer}.{TestType}.Tests` 形式厳守
  - Layer: Domain/Application/Contracts/Infrastructure/Web
  - TestType: Unit/Integration/UI/E2E

**Phase 6完了判定**: すべてのチェック項目が✅

---

## Phase 7: ドキュメント更新チェックリスト

### テストアーキテクチャ設計書更新

- [ ] **プロジェクト構成図更新**
  - 場所: `/Doc/02_Design/テストアーキテクチャ設計書.md`
  - 更新内容: 新規プロジェクト追加
  - 所要時間: 10分

- [ ] **プロジェクト一覧表更新**
  - 更新項目: プロジェクト名 / Layer / TestType / 言語 / 主要NuGetパッケージ / 参照関係
  - 所要時間: 5分

### README.md更新

- [ ] **テスト実行手順追記**
  - 場所: `/README.md`（プロジェクトルート）
  - 更新内容: 新規プロジェクト個別実行コマンド追加
  - 所要時間: 5分

**Phase 7完了判定**: すべてのチェック項目が✅

---

## 最終確認チェックリスト

### 全Phase完了確認

- [ ] **Phase 1: 事前確認完了**
- [ ] **Phase 2: プロジェクト作成完了**
- [ ] **Phase 3: 参照関係設定完了**
- [ ] **Phase 4: NuGetパッケージ追加完了**
- [ ] **Phase 5: ビルド・実行確認完了**
- [ ] **Phase 6: Issue #40再発防止確認完了**
- [ ] **Phase 7: ドキュメント更新完了**

### 品質確認

- [ ] **0 Warning / 0 Error（厳守）**
- [ ] **全テスト100%成功**
- [ ] **ADR_020準拠確認**
- [ ] **テストアーキテクチャ設計書との整合性確認**

---

## クイックリファレンス

### Domain.Unit.Tests作成コマンド一覧

```bash
# プロジェクト作成
dotnet new xunit -lang F# -n UbiquitousLanguageManager.Domain.Unit.Tests -o tests/UbiquitousLanguageManager.Domain.Unit.Tests

# NuGetパッケージ追加
cd tests/UbiquitousLanguageManager.Domain.Unit.Tests
dotnet add package xunit && dotnet add package xunit.runner.visualstudio && dotnet add package Microsoft.NET.Test.Sdk && dotnet add package FsUnit.xUnit && dotnet add package coverlet.collector

# ソリューション追加
cd ../..
dotnet sln add tests/UbiquitousLanguageManager.Domain.Unit.Tests

# ビルド確認
dotnet build tests/UbiquitousLanguageManager.Domain.Unit.Tests
dotnet build

# テスト実行確認
dotnet test tests/UbiquitousLanguageManager.Domain.Unit.Tests
dotnet test
```

---

**作成日**: 2025-11-01
**Phase B-F2 Step2**: Agent Skills Phase 2展開
**参照**: ADR_020, 新規テストプロジェクト作成ガイドライン, Issue #40教訓
