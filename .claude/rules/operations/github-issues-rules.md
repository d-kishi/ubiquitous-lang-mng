---
paths:
  - Doc/10_Debt/**
  - .github/**
---

# GitHub Issues管理規則

## 概要

技術的負債・課題管理のGitHub Issues運用ルール。

## 対象範囲

### 管理対象

- 新規発見の技術的負債・課題
- 既存のTECH-XXX（段階的移行）
- アーキテクチャ・設計に関わる課題
- 品質・保守性に関わる改善項目

### 除外範囲

- 日常的なバグ報告（従来のGitHubフロー継続）
- ADR・設計決定記録（従来のMarkdown継続）

## Claude Code操作コマンド

### Issue作成

```bash
gh issue create --title "タイトル" --body "本文" --label "tech-debt"
```

### Issue読み込み

```bash
gh issue view <番号>
gh api repos/{owner}/{repo}/issues/{number}
```

### Issue更新

```bash
gh issue comment <番号> --body "進捗報告"
gh issue close <番号>
```

## GitHub Issues機能の活用

| 機能 | 用途 |
|------|------|
| **ラベル** | 種別・優先度・影響範囲の分類 |
| **マイルストーン** | Phaseとの連携 |
| **Projects** | ダッシュボード・カンバンボード |
| **Assignees** | 担当者（Claude Code）明示 |
| **Templates** | 標準化されたIssue作成 |

## 期待効果

### 追跡可能性向上

- Issue番号による一意識別
- コミット・PR・Issue間の相互参照
- タイムライン・履歴の自動記録

### 管理効率向上

- フィルタリング・検索・ソート機能
- ダッシュボードによる進捗可視化
- 自動化・通知機能

### ドキュメント軽量化

- `/Doc/10_Debt/`の簡素化
- 参照すべき情報の集約
- 重複記述の排除

### Claude Code統合強化

- 対応時の自動情報取得
- 構造化された必須読み込みファイル一覧
- 進捗の自動更新

## 見直し基準

以下の条件で本決定の見直しを検討する：

1. **効率性の問題**: 管理作業時間が30%以上増加
2. **技術的制約**: GitHub APIの制限による機能不足
3. **組織的制約**: プロジェクト規模拡大による複雑化

---

**抽出元ADR**: ADR_015_技術的負債管理のGitHub_Issues移行.md
**関連Skills**: `.claude/skills/github-issues-management/`
**作成日**: 2025-12-17
**Phase**: Issue #83 ルール管理基盤改善

