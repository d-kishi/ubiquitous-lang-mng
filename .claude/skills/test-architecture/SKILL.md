---
name: test-architecture
description: テストアーキテクチャ規約を適用する。「テストプロジェクト作成」「テスト命名」「テスト参照関係」「テスト配置」の際に使用する。
allowed-tools: Read, mcp__serena__find_symbol, mcp__serena__get_symbols_overview, Grep
---

# Test Architecture Skill

ADR_020テストアーキテクチャ決定に基づく**レイヤー×テストタイプ分離方式**の自律的適用を提供する。

## 使用タイミング

1. **新規テストプロジェクト作成時**（最重要）
2. **unit-test/integration-test Agent作業開始時**
3. **テストアーキテクチャ違反検出時**

## 核心原則（ADR_020）

### 命名規則（厳守）

```
UbiquitousLanguageManager.{Layer}.{TestType}.Tests
```

- **Layer**: Domain / Application / Contracts / Infrastructure / Web
- **TestType**: Unit / Integration / UI / E2E

### 参照関係原則

| TestType | 参照範囲 |
|----------|---------|
| Unit | テスト対象レイヤーのみ |
| Integration | 必要な依存層（WebApplicationFactory使用時は全層） |
| E2E | 全層参照可 |

**詳細**: [`./references/reference-relationships.md`](./references/reference-relationships.md)

### Issue #40再発防止

- 1プロジェクト = 1レイヤー × 1テストタイプ
- F#/C#混在回避
- テストタイプ/レイヤー混在回避

## 参照ファイル

| ファイル | 内容 |
|---------|------|
| [`new-test-project-checklist.md`](./references/new-test-project-checklist.md) | 新規プロジェクト作成チェックリスト |
| [`test-project-architecture.md`](./references/test-project-architecture.md) | テストプロジェクトアーキテクチャ詳細 |
| [`reference-relationships.md`](./references/reference-relationships.md) | 参照関係詳細（XML例） |
| [`conventions-and-patterns.md`](./references/conventions-and-patterns.md) | 言語選択・NuGet・違反パターン・品質目標 |

## 関連Skills

- **tdd-red-green-refactor**: TDD実践パターン
- **playwright-e2e-patterns**: E2Eテスト作成パターン
- **subagent-patterns**: Agent組み合わせパターン

## 参照元ADR

- **ADR_020**: `/Doc/07_Decisions/ADR_020_テストアーキテクチャ決定.md`
