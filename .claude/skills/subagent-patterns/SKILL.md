---
name: subagent-patterns
description: SubAgentを選択・組み合わせる。「Step開始」「Agent選択」「SubAgent責務判定」「並列実行判断」の際に使用する。
allowed-tools: Read, Grep
---

# SubAgent Patterns Skill

ADR_013 SubAgentプール方式に基づく14種類のAgentの最適組み合わせを選択する。

## 使用タイミング

1. **Step開始時**（最重要）- SubAgent組み合わせ選択
2. **SubAgent選択迷い時** - 責務境界確認
3. **エラー修正時** - Fix-Mode活用
4. **Phase計画時** - Agent構成検討

## SubAgentプール（14種類）

### 調査分析系（4Agent）
- **tech-research**: 技術調査・最新情報収集
- **spec-analysis**: 仕様分析・要件抽出
- **design-review**: 設計整合性確認
- **dependency-analysis**: 依存関係特定・実装順序決定

**詳細**: [`./references/research-agents-selection.md`](./references/research-agents-selection.md)

### 実装系（5Agent）
- **fsharp-domain**: F#ドメインモデル・ビジネスロジック
- **fsharp-application**: F#アプリケーションサービス・ユースケース
- **contracts-bridge**: F#↔C#型変換・TypeConverter
- **csharp-infrastructure**: Repository・EF Core・外部連携
- **csharp-web-ui**: Blazor Server・Razor・UI

**詳細**: [`./references/implementation-agents-selection.md`](./references/implementation-agents-selection.md)

### 品質保証系（5Agent）
- **unit-test**: TDD実践・単体テスト
- **integration-test**: WebApplicationFactory統合テスト
- **e2e-test**: Playwright E2Eテスト
- **code-review**: コード品質・保守性レビュー
- **spec-compliance**: 仕様準拠監査

**詳細**: [`./references/qa-agents-selection.md`](./references/qa-agents-selection.md)

## 並列実行判断

### 並列実行可能
- 実装系 + テスト系（例: fsharp-domain + unit-test）
- 品質保証系同士（例: code-review + spec-compliance）

### 並列実行不可
- 実装系同士（同一ファイル操作リスク）
- テスト系同士（テストプロジェクト競合リスク）

## 参照ファイル

| ファイル | 内容 |
|---------|------|
| [`agent-responsibility-boundary.md`](./references/agent-responsibility-boundary.md) | Agent責務境界詳細 |
| [`phase-specific-combinations.md`](./references/phase-specific-combinations.md) | Phase特性別組み合わせパターン |
| [`subagent-guidelines.md`](./references/subagent-guidelines.md) | SubAgent実行ガイドライン |

## 参照元ADR

- **ADR_013**: SubAgentプール方式採用
- **ADR_018**: SubAgent指示改善・Fix-Mode活用
- **ADR_024**: Playwright Test Agents統合
