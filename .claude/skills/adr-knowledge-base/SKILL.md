---
name: adr-knowledge-base
description: ADR知見を参照・適用する。「技術決定」「設計判断」「ADR確認」「アーキテクチャ選定」の際、または新規ADR作成要否の判断時に使用する。
allowed-tools: Read, Grep
---

# ADR Knowledge Base Skill

プロジェクトで蓄積されたADR（Architecture Decision Record）知見を体系的に参照・適用。

## 使用タイミング

1. **技術決定時** - 新技術選定・アーキテクチャ設計・ライブラリ選定
2. **問題発生時** - 既存ADRに解決策がないか確認
3. **ADR作成判断時** - 新規ADR作成が必要か判断
4. **実装時** - ADR準拠の実装方法確認

## 主要ADR一覧

| ADR | 重要ポイント | 適用シーン |
|-----|-------------|-----------|
| **ADR_010** | Blazor Server・F#初学者対応、MainAgent責務分担 | コード作成・コメント記載・エラー修正 |
| **ADR_013** | 14種類のSubAgent、並列実行 | Step開始時・SubAgent選択 |
| **ADR_016** | コマンド=契約、承認必須 | Command実行・承認取得 |
| **ADR_019** | Bounded Context別サブnamespace | 新規クラス・モジュール作成 |
| **ADR_020** | レイヤー×テストタイプ分離 | テストプロジェクト作成 |
| **ADR_021** | Playwright MCP + Agents統合 | E2Eテスト実装 |

## ADR抜粋詳細

**詳細**: 各ADR抜粋は `references/` 配下

| ファイル | 内容 |
|---------|------|
| [`ADR_010_実装規約.md`](./references/ADR_010_実装規約.md) | 実装規約・MainAgent責務 |
| [`ADR_016_プロセス遵守.md`](./references/ADR_016_プロセス遵守.md) | プロセス遵守絶対原則 |
| [`ADR_019_namespace設計.md`](./references/ADR_019_namespace設計.md) | namespace設計規約 |
| [`ADR_020_テストアーキテクチャ.md`](./references/ADR_020_テストアーキテクチャ.md) | テストアーキテクチャ決定 |
| [`decision-patterns.md`](./references/decision-patterns.md) | 技術決定パターン・ADR vs Skills判断基準・チェックリスト |

## ADR作成 vs Skills作成（30秒判断）

| 質問 | Yes → |
|------|-------|
| 歴史的記録が必要か？（なぜこの決定をしたか） | **ADR作成** |
| Claudeが自律的に適用すべきか？（実装時に自動適用） | **Skills作成** |
| 技術選定の根拠か？（代替案との比較・リスク評価） | **ADR作成** |
| 実装パターン・チェックリストか？（繰り返し使うパターン） | **Skills作成** |

## 関連Skills

- **clean-architecture-guardian**: Clean Architecture準拠性チェック
- **test-architecture**: テストプロジェクトアーキテクチャ

## 参照元

- **ADRとAgent_Skills判断ガイドライン.md**: `Doc/08_Organization/Guide/`
- **ADR配置場所**: `Doc/07_Decisions/ADR_*.md`
