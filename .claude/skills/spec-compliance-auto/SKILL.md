---
name: spec-compliance-auto
description: 仕様準拠をチェックする。「新機能実装」「仕様確認」「仕様書参照」「要件検証」「仕様逸脱リスク確認」の際に使用する。
allowed-tools: Read, Grep, WebFetch
---

# Spec Compliance Auto Skill

実装が仕様書に準拠しているかを自動チェックし、仕様逸脱リスクを特定する。仕様準拠率95%以上を維持。

## 使用タイミング

1. **新機能実装前** - 仕様書の該当セクション特定・要件理解
2. **実装中** - 仕様書参照・ビジネスルール実装時
3. **実装後** - Step/Phase完了時の仕様準拠確認
4. **仕様変更時** - 影響範囲確認・既存実装の再確認

## 仕様準拠率目標値

| 要件カテゴリ | 目標値 |
|-------------|--------|
| **全体** | 95%以上（必達） |
| **機能要件** | 100%（最優先） |
| **データ整合性** | 100%（最優先） |
| **非機能要件** | 90%以上 |
| **UI/UX要件** | 85%以上 |

## 仕様準拠チェック項目（概要）

### 1. 機能要件準拠チェック

- 実装すべき機能が全て実装されている
- 実装してはいけない機能が実装されていない
- ビジネスルールが正確に実装されている

**詳細**: [`references/functional-requirements-check.md`](./references/functional-requirements-check.md)

### 2. 非機能要件準拠チェック

- パフォーマンス・セキュリティ・可用性・保守性

**詳細**: [`references/non-functional-requirements-check.md`](./references/non-functional-requirements-check.md)

### 3. データ整合性準拠チェック

- 主キー・外部キー・NULL制約・一意制約・データ型

**詳細**: [`references/data-integrity-check.md`](./references/data-integrity-check.md)

### 4. UI/UX要件準拠チェック

- 画面レイアウト・バリデーションメッセージ・エラーメッセージ

**詳細**: [`references/ui-ux-requirements-check.md`](./references/ui-ux-requirements-check.md)

## 参照ファイル

| ファイル | 内容 |
|---------|------|
| [`compliance-workflow.md`](./references/compliance-workflow.md) | Command活用・仕様書参照・維持手順・逸脱パターン・チェックリスト |
| [`functional-requirements-check.md`](./references/functional-requirements-check.md) | 機能要件チェック詳細 |
| [`non-functional-requirements-check.md`](./references/non-functional-requirements-check.md) | 非機能要件チェック詳細 |
| [`data-integrity-check.md`](./references/data-integrity-check.md) | データ整合性チェック詳細 |
| [`ui-ux-requirements-check.md`](./references/ui-ux-requirements-check.md) | UI/UXチェック詳細 |

## 関連Skills

- **tdd-red-green-refactor**: TDDサイクルでの仕様準拠テスト設計
- **spec-compliance SubAgent**: 仕様準拠監査（SubAgent）

## 参照元

- **spec-compliance-check Command**: 仕様準拠自動チェック機能
