---
paths:
  - .claude/commands/**
  - Doc/08_Organization/**
---

# 組織管理サイクル運用規則

## 概要

SubAgentプール方式による標準化された組織運用体系。

## SubAgentプール方式

### 採用背景

従来のPhase毎組織設計方式の課題：
- **組織設計負荷**: 各Phase開始時に毎回90分の組織設計が必要
- **Context圧迫**: 単一Agent内での順次専門役割実行によるAutoCompact頻発
- **再利用性欠如**: Phase毎の個別組織設計による知識・パターンの蓄積困難
- **効率性低下**: 類似作業の繰り返しによる時間効率の悪化

### 決定事項

事前定義SubAgent（14種類）による並列問題解決体系への移行。

## SubAgentプール構成（14種類）

### 調査分析系（4Agent）

| Agent | 責務 |
|-------|------|
| **tech-research** | 技術調査・最新情報収集・ベストプラクティス調査 |
| **spec-analysis** | 仕様分析・要件抽出・仕様準拠マトリックス作成 |
| **design-review** | 設計整合性確認・Clean Architecture準拠確認 |
| **dependency-analysis** | 依存関係特定・実装順序決定・制約リスク分析 |

### 実装系（5Agent）

| Agent | 責務 |
|-------|------|
| **fsharp-domain** | F#ドメインモデル・ビジネスロジック実装 |
| **fsharp-application** | F#アプリケーションサービス・ユースケース実装 |
| **contracts-bridge** | F#↔C#型変換・TypeConverter実装 |
| **csharp-infrastructure** | Repository・Entity Framework・外部サービス連携 |
| **csharp-web-ui** | Blazor Server・Razor・フロントエンドUI実装 |

### 品質保証系（5Agent）

| Agent | 責務 |
|-------|------|
| **unit-test** | TDD実践・単体テスト設計実装・Red-Green-Refactorサイクル |
| **integration-test** | WebApplicationFactory統合テスト・データベース統合テスト |
| **e2e-test** | Playwright E2Eテスト実装・UIインタラクション |
| **code-review** | コード品質・保守性・Clean Architecture準拠レビュー |
| **spec-compliance** | 仕様準拠監査・受け入れ基準確認 |

## 期待効果

| 効果 | 内容 |
|------|------|
| **開発効率** | 50-60%向上 |
| **管理負荷** | 90%削減 |
| **品質** | 専門性活用による向上 |

## 関連文書

### SubAgent定義

- `.claude/agents/` - プロジェクトサブエージェント定義ファイル群

### 運用規則

- `.claude/rules/operations/organization-manual.md` - 組織管理運用マニュアル
- `.claude/rules/operations/subagent-guidelines.md` - SubAgent実行ガイドライン
- `.claude/skills/subagent-patterns/` - SubAgent組み合わせパターンSkill

---

**抽出元ADR**: ADR_013_組織管理サイクル運用規則.md
**作成日**: 2025-12-17
**Phase**: Issue #83 ルール管理基盤改善

