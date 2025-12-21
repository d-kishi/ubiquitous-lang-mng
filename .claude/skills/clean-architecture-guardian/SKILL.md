---
name: clean-architecture-guardian
description: Clean Architecture準拠性をチェックする（循環依存・namespace階層・レイヤー間参照制約）。「新規クラス作成」「namespace変更」「プロジェクト参照変更」「リファクタリング」「Step完了」「ビルドエラー」の際に使用する。
allowed-tools: Read, Grep
---

# Clean Architecture Guardian Skill

F# + C# Clean Architecture実装の準拠性を自動チェック。Phase B1で確立した97点品質基準を維持。

## 使用タイミング

1. **新規実装時** - 新規クラス・モジュール・Bounded Context
2. **リファクタリング時** - コード移動・namespace変更・プロジェクト参照変更
3. **Step/Phase完了時** - 完了チェック・統合ビルド前
4. **問題発生時** - 循環依存・namespace衝突・ビルドエラー

## チェック項目

### 1. レイヤー分離原則

- ✅ C# → F#依存は許可
- ❌ F# → C#依存は禁止
- ✅ 循環依存ゼロ

**詳細**: [`references/layer-separation.md`](./references/layer-separation.md)

### 2. namespace階層化ルール

- ✅ Bounded Context別サブnamespace使用
- ✅ テンプレート: `<ProjectName>.<Layer>.<BoundedContext>[.<Feature>]`
- ✅ 階層制限（3階層推奨、4階層許容）

**詳細**: [`references/namespace-design.md`](./references/namespace-design.md)

### 3. Bounded Context境界

| 境界 | 対象 |
|------|------|
| Common | 全境界文脈共通定義 |
| Authentication | ユーザー・認証・権限管理 |
| ProjectManagement | プロジェクト管理 |
| UbiquitousLanguageManagement | ユビキタス言語管理 |

### 4. F# Compilation Order

- ✅ Common → Authentication → ProjectManagement → UbiquitousLanguageManagement
- ✅ ValueObjects → Errors → Entities → DomainServices
- ✅ 前方参照なし

## 品質基準（Phase B1確立）

| 観点 | 目標 |
|------|------|
| **依存関係の正しさ** | 100% |
| **循環依存** | 0件 |
| **レイヤー責務分離** | 100% |
| **namespace階層化** | 100% |
| **ビルド品質** | 0 Warning / 0 Error |

## 参照ファイル

| ファイル | 内容 |
|---------|------|
| [`layer-separation.md`](./references/layer-separation.md) | レイヤー分離原則詳細 |
| [`namespace-design.md`](./references/namespace-design.md) | namespace設計規約（ADR_019準拠） |
| [`violation-patterns.md`](./references/violation-patterns.md) | 違反パターン・修正方法・チェック手順 |

## 関連Agents

- **design-review**: システム設計・Clean Architecture準拠確認
- **dependency-analysis**: 依存関係特定・実装順序決定
- **code-review**: コード品質・Clean Architecture準拠レビュー

## 参照元

- **Phase B1 Step5実装記録**: `Doc/08_Organization/Completed/Phase_B1/Step05_namespace階層化.md`
- **ADR_019**: namespace設計規約
