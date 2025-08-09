# SubAgent選択Command

**目的**: Phase・Step特性に応じた最適SubAgent組み合わせ選択  
**対象**: Phase/Step開始時  
**基準**: SubAgent組み合わせパターン.md準拠

## コマンド実行内容

### 1. 作業特性判断（1-2分）
```bash
echo "🎯 作業特性を判断します..."
```

**判断項目**:
- [ ] **新機能実装**: 新規エンティティ・機能の縦方向スライス実装
- [ ] **機能拡張**: 既存機能の拡張・関連機能追加  
- [ ] **品質改善**: 既存コードの改善・リファクタリング・負債解消
- [ ] **調査分析**: 技術調査・仕様分析・課題発見

### 2. SubAgent組み合わせパターン選択

#### Step1（調査分析）- 全Phase共通
**必須Agent（並列実行）**:
- [ ] **tech-research**（技術調査）: 技術課題・解決策調査
- [ ] **spec-analysis**（仕様分析）: 仕様詳細分析・要件抽出
- [ ] **design-review**（設計レビュー）: 既存設計との整合性確認
- [ ] **dependency-analysis**（依存関係分析）: 実装順序・依存関係分析

```yaml
実行時間: 45-60分（従来90分から短縮）
並列実行: 4Agent同時実行→MainAgentで統合
```

#### Pattern A: 新機能実装（Phase B1, C1, D1等）
**Phase1（Domain→Application）**:
- [ ] **fsharp-domain**（F#ドメイン）: ドメインモデル設計・実装
- [ ] **fsharp-application**（F#アプリケーション）: ユースケース・サービス実装  
- [ ] **unit-test**（単体テスト）: TDD実践・ドメインロジックテスト

**Phase2（境界→インフラ）**:
- [ ] **contracts-bridge**（F#↔C#境界）: F#↔C#型変換実装
- [ ] **csharp-infrastructure**（C#インフラ）: Repository・データアクセス実装
- [ ] **integration-test**（統合テスト）: データベース統合テスト

**Phase3（UI→完成）**:
- [ ] **csharp-web-ui**（C# Web UI）: Blazorコンポーネント・画面実装
- [ ] **spec-compliance**（仕様準拠監査）: 仕様準拠確認・受け入れテスト  
- [ ] **code-review**（コードレビュー）: 全体品質レビュー

#### Pattern B: 機能拡張（Phase A2, B2, C2等）
**Phase1（影響分析）**:
- [ ] **dependency-analysis**（依存関係分析）: 既存機能への影響分析
- [ ] **design-review**（設計レビュー）: アーキテクチャ整合性確認
- [ ] **spec-analysis**（仕様分析）: 追加要件と既存仕様の整合確認

**Phase2（実装・統合）**:
- [ ] 必要な実装系Agent選択（既存の拡張対象に応じて）
- [ ] **integration-test**（統合テスト）: 既存機能との統合テスト重点
- [ ] **unit-test**（単体テスト）: 追加・変更ロジックのテスト

**Phase3（品質保証）**:
- [ ] **code-review**（コードレビュー）: リファクタリング・品質改善
- [ ] **spec-compliance**（仕様準拠監査）: 既存＋新規仕様の全体準拠確認

#### Pattern C: 品質改善（技術負債解消等）
**Phase1（課題分析）**:
- [ ] **code-review**（コードレビュー）: 既存コードの問題点特定
- [ ] **tech-research**（技術調査）: 改善手法・ベストプラクティス調査
- [ ] **dependency-analysis**（依存関係分析）: 改善の影響範囲分析

**Phase2（改善実装）**:
- [ ] 対象層のAgent選択（問題箇所に応じて）
  * F#問題: **fsharp-domain**, **fsharp-application**
  * 境界問題: **contracts-bridge**  
  * C#問題: **csharp-infrastructure**, **csharp-web-ui**
- [ ] **unit-test**（単体テスト）: リファクタリング安全性確保

**Phase3（検証・完成）**:
- [ ] **integration-test**（統合テスト）: 改善後の統合動作確認
- [ ] **spec-compliance**（仕様準拠監査）: 仕様準拠の維持確認
- [ ] **code-review**（コードレビュー）: 改善効果・品質向上確認

### 3. 並列実行計画策定
```bash
echo "⚡ 並列実行計画を策定します..."
```

**計画項目**:
- [ ] 選択したSubAgentの並列実行順序決定
- [ ] 各Agentの専門成果物確認（/Doc/05_Research/Phase_XX/配下）
- [ ] Agent間の成果物継承関係確認
- [ ] MainAgentでの統合・品質確認計画

### 4. 動的調整判断基準設定

**Agent追加・変更の判断基準**:
- [ ] 想定以上の複雑性発見 → 追加Agentの並列投入
- [ ] 専門知識不足 → **tech-research**による補強
- [ ] 品質懸念 → **code-review**, **spec-compliance**の早期投入

### 5. 実行開始準備
```bash  
echo "🚀 SubAgent並列実行を開始します..."
```

**実行準備確認**:
- [ ] 選択したSubAgent定義ファイル確認（.claude/agents/配下）
- [ ] 必要な入力ファイル・前提条件確認
- [ ] 並列実行環境・Context準備完了
- [ ] SubAgent実行・統合プロセス開始

## Pattern効率性確認

### 成功基準・KPI
- [ ] **時間短縮率**: 従来プロセス比50-60%短縮維持
- [ ] **並列効果**: Step1調査分析90分→45分達成
- [ ] **管理負荷**: 組織設計時間90%削減（90分→9分）

### 品質向上効果
- [ ] **仕様準拠率**: spec-compliance により100%達成
- [ ] **コード品質**: code-review により継続的改善
- [ ] **テスト品質**: unit-test, integration-test により80%カバレッジ維持

## 実行後フォローアップ
✅ 最適Pattern選択・実行成功 → 継続実行  
🔄 Pattern調整必要 → Agent追加・変更実施  
⚠️ 効率性課題 → Pattern最適化・改善実施

## 関連文書・参考情報
- **SubAgent組み合わせパターン.md**: 詳細なPattern選択ガイド
- **ADR_013**: 組織管理サイクル運用規則
- **.claude/agents/**: 全SubAgent定義ファイル群
- **Infrastructure_Improvement_Planning.md**: SubAgent戦略詳細