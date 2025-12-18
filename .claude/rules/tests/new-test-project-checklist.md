---
paths:
  - tests/**
---

# 新規テストプロジェクト作成チェックリスト

## 概要

新規テストプロジェクト作成時の必須確認事項。Issue #40教訓を組み込み、ADR_020準拠を保証。

**適用タイミング**: 新規テストプロジェクト作成前 / unit-test・integration-test Agent選択時

---

## Phase 1: 事前確認

### 必須ドキュメント確認

- [ ] ADR_020テストアーキテクチャ決定（`/Doc/07_Decisions/ADR_020_*.md`）
- [ ] テストアーキテクチャ設計書（`/Doc/02_Design/テストアーキテクチャ設計書.md`）
- [ ] 本チェックリスト・test-project-architecture.md

### 既存プロジェクト確認

- [ ] `dotnet sln list`で重複プロジェクトなし確認
- [ ] Layer選択明確化: Domain / Application / Contracts / Infrastructure / Web
- [ ] TestType選択明確化: Unit / Integration / UI / E2E

---

## Phase 2: プロジェクト作成

### プロジェクト作成コマンド

| Layer | 言語 | コマンド |
|-------|------|---------|
| Domain / Application | F# | `dotnet new xunit -lang F# -n UbiquitousLanguageManager.{Layer}.{TestType}.Tests -o tests/...` |
| Contracts / Infrastructure / Web | C# | `dotnet new xunit -n UbiquitousLanguageManager.{Layer}.{TestType}.Tests -o tests/...` |

### 命名規則確認

- [ ] テンプレート: `UbiquitousLanguageManager.{Layer}.{TestType}.Tests`
- [ ] `.Tests`サフィックス付与
- [ ] ディレクトリ名とプロジェクト名一致

### SDK選択（UI Testsのみ手動変更）

| TestType | SDK |
|----------|-----|
| Unit / Integration / E2E | `Microsoft.NET.Sdk` |
| UI (bUnit) | `Microsoft.NET.Sdk.Razor` **手動変更必須** |

---

## Phase 3: 参照関係設定

**詳細**: `test-project-architecture.md` 参照

### 基本原則

- [ ] Unit Tests: テスト対象レイヤーのみ参照（最小化原則）
- [ ] Integration/E2E Tests: 全層参照
- [ ] UI Tests: Web層のみ参照
- [ ] 循環依存なし確認

---

## Phase 4: NuGetパッケージ追加

### 共通パッケージ（全テストタイプ）

```bash
dotnet add package xunit xunit.runner.visualstudio Microsoft.NET.Test.Sdk coverlet.collector
```

### テストタイプ別追加パッケージ

| TestType | 追加パッケージ |
|----------|---------------|
| F# Unit | `FsUnit.xUnit` |
| C# Unit | `FluentAssertions`, `Moq` |
| Integration | `Microsoft.AspNetCore.Mvc.Testing`, `Testcontainers.PostgreSql` |
| E2E | `Microsoft.Playwright`, `Microsoft.AspNetCore.Mvc.Testing` |

---

## Phase 5: ビルド・実行確認

### ソリューション更新

```bash
dotnet sln add tests/UbiquitousLanguageManager.{Layer}.{TestType}.Tests
```

### 確認コマンド

- [ ] 個別ビルド: `dotnet build tests/{ProjectName}` → 0 Warning/0 Error
- [ ] 全体ビルド: `dotnet build` → 0 Warning/0 Error
- [ ] 個別テスト: `dotnet test tests/{ProjectName}` → 成功
- [ ] 全体テスト: `dotnet test` → 100%成功

---

## Phase 6: Issue #40再発防止

### 技術負債回避（絶対禁止）

| 項目 | 禁止理由 |
|------|---------|
| `EnableDefaultCompileItems=false` | F#/C#混在時の暫定対応が技術負債化 |
| F#/C#混在 | F#コンパイラはC#コンパイル不可 |
| テストタイプ混在 | テスト実行粒度制御不可 |
| レイヤー混在 | 責務分離違反 |

### 設計原則確認

- [ ] 1プロジェクト = 1レイヤー × 1テストタイプ
- [ ] `{ProjectName}.{Layer}.{TestType}.Tests` 形式厳守
- [ ] 不要な参照追加なし

---

## Phase 7: ドキュメント更新

- [ ] テストアーキテクチャ設計書（プロジェクト一覧表更新）
- [ ] README.md（テスト実行手順追記）

---

## 最終確認

- [ ] Phase 1〜7 すべて完了
- [ ] 0 Warning / 0 Error
- [ ] 全テスト100%成功
- [ ] ADR_020準拠確認

---

## クイックリファレンス

### F# Unit Tests作成（例: Domain.Unit.Tests）

```bash
# プロジェクト作成・パッケージ追加・ソリューション追加
dotnet new xunit -lang F# -n UbiquitousLanguageManager.Domain.Unit.Tests -o tests/UbiquitousLanguageManager.Domain.Unit.Tests
cd tests/UbiquitousLanguageManager.Domain.Unit.Tests
dotnet add package xunit xunit.runner.visualstudio Microsoft.NET.Test.Sdk FsUnit.xUnit coverlet.collector
cd ../..
dotnet sln add tests/UbiquitousLanguageManager.Domain.Unit.Tests

# 確認
dotnet build && dotnet test
```

---

**作成日**: 2025-12-18
**参照**: ADR_020, test-project-architecture.md, Issue #40教訓
