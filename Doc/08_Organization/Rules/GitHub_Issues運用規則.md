# GitHub Issues運用規則

**作成日**: 2025-08-17  
**対象**: 技術的負債・課題管理  
**関連ADR**: ADR_014  

## 概要

本規則は、技術的負債・課題管理のGitHub Issues移行（ADR_014）に基づく具体的な運用方法を定める。

## Issue作成基準

### 対象となる事項

#### ✅ Issue作成対象
- **技術的負債**: コード品質・保守性に関わる課題
- **アーキテクチャ問題**: 設計原則・要件からの逸脱
- **設計整合性問題**: 設計書・仕様書との不整合
- **品質課題**: パフォーマンス・セキュリティ・テスト関連
- **保守性改善**: リファクタリング・コード改善要求

#### ❌ Issue作成対象外
- **日常的なバグ**: 通常のバグレポート・修正
- **新機能要求**: Phase計画で管理される機能追加
- **ドキュメント更新**: 単純な文書修正
- **設定変更**: 環境・設定ファイルの調整

### 作成判断基準

以下のいずれかを満たす場合にIssue作成：

1. **影響範囲**: 複数ファイル・モジュールに影響
2. **作業時間**: 1時間以上の作業が見込まれる
3. **品質影響**: 品質・保守性・拡張性に影響
4. **要件逸脱**: 要件定義・設計書からの逸脱

## Issue種別とラベル体系

### 種別ラベル（必須・1つ選択）

| ラベル | 説明 | 例 |
|--------|------|-----|
| `tech-debt` | 技術的負債 | コード重複、密結合 |
| `architecture` | アーキテクチャ問題 | 層間依存、設計原則違反 |
| `security` | セキュリティ問題 | 脆弱性、セキュリティ要件未達 |
| `performance` | パフォーマンス問題 | 処理速度、メモリ使用量 |
| `maintainability` | 保守性問題 | 可読性、テスト性 |
| `compliance` | 仕様準拠問題 | 要件・設計書からの逸脱 |

### 優先度ラベル（必須・1つ選択）

| ラベル | 説明 | 対応目安 |
|--------|------|---------|
| `priority/critical` | 緊急対応必要 | 24時間以内 |
| `priority/high` | 高優先度 | 1週間以内 |
| `priority/medium` | 中優先度 | 1ヶ月以内 |
| `priority/low` | 低優先度 | 次Phase以降 |

### 影響範囲ラベル（任意・複数選択可）

| ラベル | 説明 |
|--------|------|
| `scope/domain` | F# ドメイン層 |
| `scope/application` | F# アプリケーション層 |
| `scope/contracts` | C# Contracts層 |
| `scope/infrastructure` | C# Infrastructure層 |
| `scope/web` | C# Web層 |
| `scope/tests` | テスト関連 |
| `scope/docs` | ドキュメント |

### Phaseラベル（任意・1つ選択）

| ラベル | 説明 |
|--------|------|
| `phase-a7` | Phase A7で対応 |
| `phase-b1` | Phase B1で対応 |
| `phase-future` | 将来対応 |

## Issueテンプレート

### 基本テンプレート

```markdown
# [ISSUE-TYPE-XXX] 課題タイトル

## 📋 基本情報
- **発見日**: 2025-08-17
- **発見Phase**: Phase A6レビュー
- **発見者**: ユーザー/Claude Code
- **種別**: [tech-debt/architecture/security/performance/maintainability/compliance]
- **優先度**: [critical/high/medium/low]

## 🔍 問題詳細

### 現状
現在の問題状況を具体的に記述

### 期待される状態
あるべき姿・目標状態

### 発生原因
根本原因の分析結果

## 📊 影響範囲

### 影響ファイル
- `src/path/to/file1.cs`
- `src/path/to/file2.razor`

### 関連機能
- 機能A
- 機能B

### リスク
対応しない場合のリスク評価

## 🛠️ 対応方針

### 解決アプローチ
具体的な解決方法・手順

### 必要なリソース
- 作業時間見積もり
- 必要な技術調査
- 外部依存

## 📚 Claude Code対応時の必須読み込み情報

### 必須ファイル
- [ ] `/Doc/01_Requirements/要件定義書.md`
- [ ] `/Doc/02_Design/システム設計書.md`
- [ ] `/Doc/07_Decisions/ADR_XXX.md`

### 推奨ファイル
- [ ] `/Doc/02_Design/UI設計/*.md`
- [ ] `/src/UbiquitousLanguageManager.*/Program.cs`

### 調査コマンド
```bash
# 影響範囲調査
find src -name "*.cs" | xargs grep "PatternToSearch"

# 依存関係確認
dotnet list package --include-transitive
```

## ✅ 完了チェックリスト

### 調査・分析
- [ ] 問題の根本原因特定
- [ ] 影響範囲の詳細調査
- [ ] 解決方針の検討・決定

### 実装
- [ ] コード修正・リファクタリング
- [ ] テストケース追加・修正
- [ ] ドキュメント更新

### 検証
- [ ] 修正内容の動作確認
- [ ] 回帰テスト実行
- [ ] コードレビュー完了

### 完了処理
- [ ] 関連PRのマージ
- [ ] ADR更新（必要に応じて）
- [ ] 次Phaseへの申し送り事項整理
```

## Claude Code操作手順

### Issue作成

```bash
# Issue作成
gh issue create \
  --title "[ARCH-001] MVC/Blazor混在アーキテクチャの要件逸脱" \
  --body "$(cat issue_template.md)" \
  --label "architecture,priority/high,scope/web,phase-a7"
```

### Issue確認・読み込み

```bash
# Issue一覧取得
gh issue list --label "tech-debt" --state "open"

# 特定Issue詳細確認
gh issue view 123

# APIでの詳細取得（プログラム処理用）
gh api repos/:owner/:repo/issues/123
```

### 進捗更新

```bash
# 調査開始報告
gh issue comment 123 --body "調査開始: MVC関連ファイル確認中"

# 進捗報告
gh issue comment 123 --body "進捗: 影響ファイル10個特定完了"

# 完了報告・クローズ
gh issue comment 123 --body "対応完了: 全ファイルをBlazor化"
gh issue close 123
```

### フィルタリング・検索

```bash
# 優先度別フィルタ
gh issue list --label "priority/high"

# Phase別フィルタ
gh issue list --label "phase-a7"

# 複合条件検索
gh issue list --label "architecture,priority/high" --state "open"
```

## 既存TECH-XXXからの移行手順

### Phase 1: 重要課題の移行（Phase A7開始時）

1. **対象選定**
   - TECH-001, TECH-002, TECH-003, TECH-004の移行状況確認
   - 未完了・部分完了項目の抽出

2. **Issue作成**
   - 既存Markdownファイルの内容をIssueテンプレートに転記
   - 適切なラベル・優先度の設定
   - 元ファイルへの相互参照リンク設定

3. **元ファイル更新**
   - 移行完了マークの追加
   - GitHub Issue番号の記載

### Phase 2: 全面移行（Phase A7完了後）

1. **残り課題の移行**
   - 全TECH-XXXファイルの順次移行
   - 類似課題の統合・整理

2. **アーカイブ化**
   - `/Doc/10_Debt/Archive/`への移動
   - READMEでのGitHub Issues参照案内

## 運用監視・改善

### 効果測定指標

| 指標 | 目標 | 測定方法 |
|------|------|---------|
| 課題解決時間 | 30%短縮 | Issue作成〜クローズまでの時間 |
| 管理作業時間 | 50%削減 | Issue作成・更新にかかる時間 |
| 進捗可視性 | 100%追跡可能 | Open/Closed比率、ラベル分布 |

### 見直しタイミング

- **Phase A7完了時**: 初回効果測定・プロセス改善
- **Phase B1開始前**: 運用ルールの最適化
- **月次**: Issue統計・トレンド分析

### 改善プロセス

1. **問題の特定**
   - Issue作成・管理の課題収集
   - Claude Code操作の問題点整理

2. **ルール見直し**
   - テンプレートの改善
   - ラベル体系の最適化
   - 作成基準の調整

3. **ツール改善**
   - GitHub CLI活用方法の改善
   - 自動化スクリプトの開発

## 関連文書

- **決定記録**: `/Doc/07_Decisions/ADR_014_技術的負債管理のGitHub_Issues移行.md`
- **移行対象**: `/Doc/10_Debt/Technical/TECH-*.md`
- **参考**: 
  - GitHub Issues Documentation
  - GitHub CLI Reference

---

**策定者**: Claude Code  
**承認日**: 2025-08-17  
**次回見直し**: Phase A7完了時