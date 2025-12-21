# Claude Code Agent Skills ベストプラクティス実践ガイド

## はじめに

Claude Code（Anthropic公式CLI）には「Agent Skills」という機能があります。これは、特定のドメインやワークフローに特化した知識をClaudeに与え、より効果的なコーディング支援を実現する仕組みです。

本記事では、実プロジェクトで13個のSkillsを公式ベストプラクティスに基づいて改善した経験をもとに、効果的なSkills設計・運用のポイントを解説します。

## Agent Skillsとは

Agent Skillsは、Claude Codeの能力を拡張するためのモジュール化された知識パッケージです。

### Skillsの構成要素

```
skill-name/
├── SKILL.md (必須)
│   ├── YAML frontmatter (name, description)
│   └── Markdown本文 (使い方・ガイドライン)
└── references/ (オプション)
    ├── pattern-a.md
    └── pattern-b.md
```

### Skillsが提供するもの

1. **専門的なワークフロー** - 特定ドメインの手順・パターン
2. **ツール統合** - 特定フォーマットやAPIとの連携知識
3. **ドメイン知識** - プロジェクト固有のルール・制約
4. **再利用可能なリソース** - スクリプト、テンプレート、参照資料

## 公式ベストプラクティス 12項目チェックリスト

Anthropic公式ドキュメント（[Best Practices](https://platform.claude.com/docs/en/agents-and-tools/agent-skills/best-practices)）には、効果的なSkills作成のためのチェックリストが記載されています。

| # | チェック項目 | 説明 |
|---|-------------|------|
| 1 | Description: specific + key terms | 具体的で、キーとなる用語を含む |
| 2 | Description: what + when | 何をするか、いつ使うかの両方を含む |
| 3 | SKILL.md: under 500 lines | 本文は500行以下 |
| 4 | Details in separate files | 詳細情報は別ファイルに分離 |
| 5 | No time-sensitive info | 時間依存の情報を含まない |
| 6 | Consistent terminology | 用語が統一されている |
| 7 | Concrete examples | 抽象的でなく具体的な例 |
| 8 | References one level deep | 参照は1階層まで |
| 9 | Progressive disclosure | 段階的な情報開示 |
| 10 | Clear workflow steps | ワークフローが明確 |
| 11 | No Windows paths | Windowsパス形式を使わない |
| 12 | 3+ evaluations | 3つ以上の評価シナリオ |

### 重要な原則: Progressive Disclosure

Skillsは3段階でContext Windowに読み込まれます：

```
1. メタデータのみ (~100ワード) - 常にContext内
   └─ name + description

2. SKILL.md本文 (<5kワード推奨) - Skill発動時に読み込み
   └─ 基本ガイドライン・使用タイミング

3. 参照ファイル (必要時のみ) - 必要に応じて読み込み
   └─ 詳細パターン・チェックリスト
```

この仕組みを活かすには、**SKILL.mdをリーンに保ち、詳細情報は`references/`に分離する**ことが重要です。

## 実践: 13 Skillsの改善プロセス

### 改善前の課題

実プロジェクトで運用していた13個のSkillsを分析したところ、以下の課題が見つかりました：

| 課題 | 影響 |
|------|------|
| SKILL.mdが肥大化（最大426行） | Context Window圧迫 |
| ディレクトリ名が不統一（`rules/`, `patterns/`, `adr-excerpts/`） | 構造の分かりにくさ |
| 詳細情報がSKILL.mdに混在 | Progressive Disclosure未活用 |

### 改善計画

4フェーズで改善を実施しました：

| Phase | 内容 |
|-------|------|
| Phase 1 | ディレクトリ構成標準化（`references/`統一） |
| Phase 2 | SKILL.mdスリム化（essentials only） |
| Phase 3 | 参照ファイルにTOC追加 |
| Phase 4 | 品質検証・評価シナリオ作成 |

## Before/After比較

### 例1: error-logging-patterns

**Before（110行）**:
```markdown
# Error Logging Patterns Skill

## 概要

このSkillは、Clean ArchitectureとF#関数型プログラミングを
活かした層別エラーハンドリング・ログ出力パターンの自律的適用を
提供します。ADR_007（エラーハンドリング）・ADR_008（ログ出力指針）
に基づく実装パターンを定義します。

## 使用タイミング

Claudeは以下の状況でこのSkillを自律的に使用すべきです：

### 1. エラー処理実装時（最重要）

**タイミング**:
- Domain層でのビジネスエラー定義
- Application層でのエラー変換・統合
- Infrastructure層での例外処理実装
- Presentation層でのユーザー向けエラー表示

**必須確認事項**:
- 層別エラー処理原則の遵守
- Result型・Option型の適切な使用
- 例外使用禁止原則（Domain層）

### 2. ログ出力設計時
...（以下省略、110行続く）
```

**After（64行）**:
```markdown
# Error Logging Patterns Skill

Clean ArchitectureとF#関数型プログラミングを活かした
層別エラーハンドリング・ログ出力パターン。ADR_007・ADR_008に基づく。

## 使用タイミング

1. **エラー処理実装時** - Domain/Application/Infrastructure/Presentation層でのエラー処理
2. **ログ出力設計時** - 新規機能・既存機能へのログ追加・ログレベル判断
3. **F#↔C#境界実装時** - Result型↔Exception型変換

## 層別エラー処理・ログ原則（概要）

| 層 | エラー処理 | ログ出力 |
|----|-----------|---------|
| **Domain (F#)** | Result型必須・例外禁止 | **禁止**（純粋関数維持） |
| **Application (F#)** | エラー伝播・変換 | ユースケース開始・終了 |
| **Infrastructure (C#)** | 例外キャッチ・変換 | パフォーマンス・エラー詳細 |
| **Presentation (C#)** | ユーザー向け表示 | 操作・認証状態 |

## 詳細ルール

- **エラーハンドリング**: [`./references/error-handling.md`](./references/error-handling.md)
- **ログ出力指針**: [`./references/logging-guidelines.md`](./references/logging-guidelines.md)
```

**改善ポイント**:
- 110行 → 64行（42%削減）
- 冗長な説明を削除し、テーブルで簡潔に表現
- 詳細情報は`references/`に分離
- Progressive Disclosure原則を適用

### 例2: clean-architecture-guardian

**Before**: 240行
**After**: 79行（67%削減）

主な変更：
- 「典型的な問題」セクションを`references/violation-patterns.md`に移動
- チェックポイントをテーブル形式に圧縮
- 各項目の詳細説明を参照ファイルに分離

### 改善効果サマリー

| 指標 | 改善前 | 改善後 | 削減率 |
|------|--------|--------|--------|
| 平均行数 | 約200行 | 72.9行 | **64%** |
| 最長Skill | 426行 | 91行 | **79%** |
| ディレクトリ種類 | 4種類 | 1種類 | **75%** |

## 評価シナリオの作成

公式ベストプラクティスでは「3つ以上の評価シナリオ」が推奨されています。各Skillに対して3種類のシナリオを作成しました：

### シナリオ構成

1. **ポジティブシナリオ** - 基本トリガーによる正常発動
2. **エッジケースシナリオ** - 複合トリガー・境界条件
3. **ネガティブシナリオ** - 類似だが発動すべきでないケース

### 例: error-logging-patterns

```json
{
  "skill": "error-logging-patterns",
  "scenarios": [
    {
      "type": "positive",
      "query": "Infrastructure層でデータベース接続エラーをキャッチして適切にログ出力したい",
      "expected": "error-logging-patterns Skill発動、層別ログ原則適用"
    },
    {
      "type": "edge",
      "query": "F#のResult型エラーをC#のExceptionに変換する際のログ出力タイミングは？",
      "expected": "error-logging-patterns + fsharp-csharp-bridge 両Skill参照"
    },
    {
      "type": "negative",
      "query": "コンソールにデバッグメッセージを出力するにはどうすればいい？",
      "expected": "一般的なログ出力知識で対応、Skill発動不要"
    }
  ]
}
```

## 日本語プロジェクトでの考慮事項

公式ドキュメントでは「Always write in third person」（常に第三人称で書く）と推奨されていますが、これは英語の人称代名詞（I/You）による視点の混乱を避けるためです。

日本語の「〜する」「〜を適用する」形式は主語を持たないため、この問題は発生しません。日本語プロジェクトでは、自然な日本語表現を維持して問題ありません。

```yaml
# 英語での推奨
description: "Processes Excel files and generates reports"

# 日本語での許容例
description: "エラーハンドリング・ログ出力パターンを適用する。「エラー処理実装」「ログ設計」の際に使用する。"
```

## descriptionのトリガー設計

Skillの発動条件は`description`フィールドで決まります。効果的なトリガー設計のポイント：

### 1. キーワードを明示する

```yaml
# 「」でトリガーキーワードを明示
description: エラーハンドリング・ログ出力パターンを適用する。「エラー処理実装」「ログ設計」「例外処理」「Result型活用」の際に使用する。
```

### 2. What + Whenを含める

```yaml
# What: 何をするか
# When: いつ使うか
description: Clean Architecture準拠性をチェックする（What）。「新規クラス作成」「namespace変更」「ビルドエラー」の際に使用する（When）。
```

## まとめ

Agent Skillsを効果的に活用するためのポイント：

1. **SKILL.mdはリーンに** - エッセンスのみ、詳細は`references/`へ
2. **Progressive Disclosure** - 段階的な情報開示を意識
3. **ディレクトリ構成を統一** - `references/`を標準使用
4. **descriptionでトリガー明示** - 発動条件を明確に
5. **評価シナリオで検証** - 3種類のシナリオで発動条件確認

Skillsは「Claudeへのオンボーディングガイド」です。人間の新メンバーに説明するように、必要十分な情報を適切な粒度で提供することが重要です。

---

## 参考リンク

- [Agent Skills Best Practices](https://platform.claude.com/docs/en/agents-and-tools/agent-skills/best-practices)
- [skill-creator（公式ツール）](https://github.com/anthropics/skills/tree/main/example-skills/skill-creator)

---

**タグ案**: `Claude`, `Claude Code`, `Agent Skills`, `AI`, `開発効率化`
