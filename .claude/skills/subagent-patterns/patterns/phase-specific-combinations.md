# Phase特性別SubAgent組み合わせパターン

## 概要

Phase特性（新機能実装・技術基盤整備・テスト強化・仕様準拠監査）に応じた最適なSubAgent組み合わせパターンを提供します。Phase B1, B2実績に基づくベストプラクティスです。

---

## Pattern A: 新機能実装Phase

### 特徴

```yaml
目的: 新規ドメインモデル・ユースケース実装
特性:
  - ボトムアップ実装（Domain → Application → Infrastructure → Web）
  - Clean Architecture依存方向遵守
  - TDD実践・Red-Green-Refactorサイクル

適用Phase例:
  - Phase A1-A6（認証・ユーザー管理実装）
  - Phase B2（E2Eテスト基盤実装）
```

### 推奨SubAgent組み合わせ（11 Step）

```yaml
Step1: spec-analysis Agent（仕様分析）
  目的: 仕様要件抽出・仕様準拠マトリックス作成
  成果物: 仕様準拠マトリックス・テスト要件リスト
  推定: 30-40分

Step2: design-review Agent（設計確認）
  目的: 既存設計との整合性確認・レイヤー配置決定
  成果物: レイヤー配置計画・namespace設計
  推定: 20-30分

Step3: dependency-analysis Agent（依存関係分析）
  目的: 実装順序決定・並列実装判断
  成果物: 実装順序計画・依存グラフ
  推定: 20-30分

Step4: fsharp-domain + unit-test（並列）
  目的: Domain層実装・TDD実践
  成果物: ValueObjects/Entities/DomainServices + 単体テスト
  推定: 60-90分

Step5: fsharp-application + unit-test（並列）
  目的: Application層実装・TDD実践
  成果物: UseCase/ApplicationService + 単体テスト
  推定: 60-90分

Step6: contracts-bridge Agent（F#↔C#境界）
  目的: 型変換・TypeConverter実装
  成果物: DTO/TypeConverter/Mapper
  推定: 40-60分

Step7: csharp-infrastructure + integration-test（並列）
  目的: Infrastructure層実装・データベース統合テスト
  成果物: Repository/DbContext + 統合テスト
  推定: 60-90分

Step8: csharp-web-ui + integration-test（並列）
  目的: Web層実装・E2Eテスト
  成果物: Blazor/Razor + E2Eテスト
  推定: 60-90分

Step9: code-review Agent（品質確認）
  目的: コード品質・Clean Architecture準拠確認
  成果物: 品質評価レポート・改善提案
  推定: 30-40分

Step10: spec-compliance Agent（仕様準拠確認）
  目的: 仕様準拠度評価・逸脱箇所特定
  成果物: 仕様準拠度評価レポート
  推定: 30-40分

Step11: code-review + spec-compliance（並列・最終確認）
  目的: 最終品質確認・仕様準拠確認
  成果物: 最終品質評価・受け入れ基準達成確認
  推定: 20-30分
```

### 並列実行戦略

```yaml
並列実行可能:
  - Step4: fsharp-domain + unit-test
  - Step5: fsharp-application + unit-test
  - Step7: csharp-infrastructure + integration-test
  - Step8: csharp-web-ui + integration-test
  - Step11: code-review + spec-compliance

並列実行不可:
  - Step1-3: 調査分析系（順次実行）
  - Step4-8: 実装系（依存関係により順次）
```

### Phase B1実績

```yaml
適用Phase: Phase A1-A6（認証・ユーザー管理）
総所要時間: 6-8時間（11 Step）
品質達成: 97点（Clean Architecture準拠）
仕様準拠率: 95%

効果:
  - ボトムアップ実装による依存方向遵守
  - TDD実践によるテストカバレッジ向上（97%）
  - 並列実行による効率化（30-40%）
```

---

## Pattern B: 技術基盤整備Phase

### 特徴

```yaml
目的: アーキテクチャ改善・技術負債解決・パフォーマンス最適化
特性:
  - レイヤー別改善（特定層の集中改善）
  - 既存依存関係への影響分析
  - 技術調査・ベストプラクティス適用

適用Phase例:
  - Phase B-F2（技術負債解決・技術基盤刷新）
  - Phase A7（アーキテクチャ統一）
```

### 推奨SubAgent組み合わせ（8 Step）

```yaml
Step1: tech-research Agent（技術調査）
  目的: 新技術調査・ベストプラクティス収集
  成果物: 技術調査レポート・推奨パターン
  推定: 40-60分

Step2: dependency-analysis Agent（依存関係分析）
  目的: 既存依存関係への影響分析・リスク評価
  成果物: 依存関係影響分析・リスク評価レポート
  推定: 30-40分

Step3: design-review Agent（設計確認）
  目的: アーキテクチャ整合性確認
  成果物: 設計整合性評価・改善計画
  推定: 20-30分

Step4: 該当層Agent + unit-test（並列）
  目的: 特定層の改善実装・テスト
  成果物: 改善実装 + 単体テスト
  推定: 60-90分

Step5: integration-test Agent（統合テスト）
  目的: 改善実装の統合テスト・影響範囲確認
  成果物: 統合テスト・影響範囲評価
  推定: 40-60分

Step6: code-review Agent（品質確認）
  目的: 改善実装の品質確認・パフォーマンス評価
  成果物: 品質評価レポート・パフォーマンス評価
  推定: 30-40分

Step7: spec-compliance Agent（仕様準拠確認）
  目的: 改善実装の仕様準拠確認
  成果物: 仕様準拠度評価
  推定: 20-30分

Step8: code-review + spec-compliance（並列・最終確認）
  目的: 最終品質確認
  成果物: 最終品質評価
  推定: 20-30分
```

### レイヤー別Agent選択

```yaml
Domain層改善:
  - fsharp-domain + unit-test

Application層改善:
  - fsharp-application + unit-test

Contracts層改善:
  - contracts-bridge + unit-test

Infrastructure層改善:
  - csharp-infrastructure + integration-test

Web層改善:
  - csharp-web-ui + integration-test
```

### 並列実行戦略

```yaml
並列実行可能:
  - Step1 + Step2（tech-research + dependency-analysis）
  - Step4（該当層Agent + unit-test）
  - Step8（code-review + spec-compliance）

並列実行不可:
  - Step1-3（調査分析系・順次実行推奨）
  - Step4-5（実装→統合テスト・順次実行）
```

### Phase B-F2想定

```yaml
適用Phase: Phase B-F2（技術負債解決・技術基盤刷新）
推定所要時間: 4-6時間（8 Step）
目標品質: 97点維持

効果:
  - 技術調査による最適解適用
  - 依存関係影響分析によるリスク低減
  - レイヤー別集中改善による効率化
```

---

## Pattern C: テスト強化Phase

### 特徴

```yaml
目的: テストカバレッジ向上・E2Eテスト実装・テスト品質向上
特性:
  - テスト系Agent集中活用
  - TDDサイクル実践
  - Playwright MCP活用

適用Phase例:
  - Phase B2（E2Eテスト基盤構築）
```

### 推奨SubAgent組み合わせ（7 Step）

```yaml
Step1: spec-analysis Agent（テスト要件分析）
  目的: テストケース要件抽出・受け入れ基準明確化
  成果物: テストケース要件リスト・受け入れ基準
  推定: 30-40分

Step2: tech-research Agent（テストパターン調査）
  目的: テストフレームワーク・パターン調査
  成果物: テストパターン調査レポート
  推定: 30-40分

Step3: unit-test Agent（単体テスト拡充）
  目的: 単体テストカバレッジ向上・TDD実践
  成果物: 単体テスト拡充
  推定: 60-90分

Step4: integration-test Agent（統合テスト拡充）
  目的: 統合テストカバレッジ向上
  成果物: 統合テスト拡充
  推定: 60-90分

Step5: integration-test Agent（E2Eテスト実装）
  目的: E2Eテスト実装・Playwright MCP活用
  成果物: E2Eテスト
  推定: 90-120分

Step6: code-review Agent（テスト品質確認）
  目的: テストコード品質確認・パターン適用確認
  成果物: テスト品質評価
  推定: 30-40分

Step7: spec-compliance Agent（テスト仕様準拠確認）
  目的: テストケースの仕様準拠確認
  成果物: テスト仕様準拠度評価
  推定: 20-30分
```

### 並列実行戦略

```yaml
並列実行可能:
  - Step1 + Step2（spec-analysis + tech-research）
  - Step6 + Step7（code-review + spec-compliance）

並列実行不可:
  - Step3-5（unit-test → integration-test → E2E順次実行推奨）
```

### Phase B2実績

```yaml
適用Phase: Phase B2（E2Eテスト基盤構築）
総所要時間: 5-7時間（7 Step）
カバレッジ達成: 97%（単体）、85%（統合）
E2E効率: 93.3%効率化（Playwright MCP活用）

効果:
  - TDDサイクル実践によるテスト品質向上
  - Playwright MCP活用による93.3%効率化
  - playwright-e2e-patterns Skill確立
```

---

## Pattern D: 仕様準拠監査Phase

### 特徴

```yaml
目的: 仕様準拠度評価・逸脱箇所特定対策・受け入れ基準達成確認
特性:
  - spec-compliance Agent集中活用
  - 仕様準拠マトリックス検証
  - 改善提案作成

適用Phase例:
  - Phase A7（要件準拠・アーキテクチャ統一）
```

### 推奨SubAgent組み合わせ（6 Step）

```yaml
Step1: spec-analysis Agent（仕様要件抽出）
  目的: 仕様要件抽出・仕様準拠マトリックス作成
  成果物: 仕様準拠マトリックス
  推定: 30-40分

Step2: spec-compliance Agent（仕様準拠度評価）
  目的: 実装の仕様準拠度評価・逸脱箇所特定
  成果物: 仕様準拠度評価レポート・逸脱箇所リスト
  推定: 40-60分

Step3: code-review Agent（コード品質確認）
  目的: 逸脱箇所のコード品質確認
  成果物: コード品質評価
  推定: 30-40分

Step4: 該当Agent（逸脱箇所修正）
  目的: 仕様逸脱箇所の修正実装
  成果物: 修正実装
  推定: 60-90分

Step5: spec-compliance Agent（再評価）
  目的: 修正実装の仕様準拠度再評価
  成果物: 仕様準拠度再評価レポート
  推定: 20-30分

Step6: code-review + spec-compliance（並列・最終確認）
  目的: 最終仕様準拠度確認・受け入れ基準達成確認
  成果物: 最終仕様準拠度評価・受け入れ基準達成確認
  推定: 20-30分
```

### 並列実行戦略

```yaml
並列実行可能:
  - Step6（code-review + spec-compliance）

並列実行不可:
  - Step1-5（順次実行・評価→修正→再評価サイクル）
```

### Phase A7想定

```yaml
適用Phase: Phase A7（要件準拠・アーキテクチャ統一）
推定所要時間: 3-5時間（6 Step）
目標準拠率: 95%以上

効果:
  - 仕様準拠マトリックスによる網羅的確認
  - 逸脱箇所の体系的修正
  - 受け入れ基準達成確認
```

---

## Pattern選択チェックリスト

### Phase開始時

```yaml
新機能実装Phase:
  - [ ] 新規ドメインモデル・ユースケース実装が主目的
  - [ ] ボトムアップ実装が適切
  - [ ] TDD実践が必要
  → Pattern A選択

技術基盤整備Phase:
  - [ ] アーキテクチャ改善・技術負債解決が主目的
  - [ ] 特定レイヤーの集中改善が適切
  - [ ] 技術調査・ベストプラクティス適用が必要
  → Pattern B選択

テスト強化Phase:
  - [ ] テストカバレッジ向上が主目的
  - [ ] E2Eテスト実装が必要
  - [ ] テスト品質向上が必要
  → Pattern C選択

仕様準拠監査Phase:
  - [ ] 仕様準拠度評価・逸脱箇所特定が主目的
  - [ ] 受け入れ基準達成確認が必要
  - [ ] 仕様準拠マトリックス検証が必要
  → Pattern D選択
```

### Pattern組み合わせ判断

```yaml
複合Phase（Pattern A + Pattern C）:
  - 新機能実装 + テスト強化
  - 例: Phase B2（E2Eテスト基盤実装 + E2Eテスト強化）

複合Phase（Pattern B + Pattern D）:
  - 技術基盤整備 + 仕様準拠監査
  - 例: Phase A7（アーキテクチャ統一 + 要件準拠確認）
```

---

## 効果測定

### Pattern A実績（Phase B1）

```yaml
所要時間: 6-8時間（11 Step）
品質達成: 97点
仕様準拠率: 95%
テストカバレッジ: 97%
効率化: 30-40%（並列実行）
```

### Pattern C実績（Phase B2）

```yaml
所要時間: 5-7時間（7 Step）
カバレッジ: 97%（単体）、85%（統合）
E2E効率化: 93.3%（Playwright MCP活用）
効率化: 40-50%（Playwright MCP）
```

---

**作成日**: 2025-11-01
**Phase B-F2 Step2**: Agent Skills Phase 2展開
**参照**: SubAgent組み合わせパターン.md、Phase B1 Summary、Phase B2 Summary
