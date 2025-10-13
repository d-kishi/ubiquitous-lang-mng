# SubAgent選択Command

**目的**: Phase・Step特性に応じた最適SubAgent組み合わせ選択  
**対象**: Phase/Step開始時  
**基準**: SubAgent組み合わせパターン.md準拠

## コマンド実行内容

### 1. 作業特性・段階判断（1-2分）
```bash
echo "🎯 作業特性・段階を判断します..."
```

**作業特性判断項目**:
- [ ] **新機能実装**: 新規エンティティ・機能の縦方向スライス実装
- [ ] **機能拡張**: 既存機能の拡張・関連機能追加
- [ ] **品質改善**: 既存コードの改善・リファクタリング・負債解消
- [ ] **調査分析**: 技術調査・仕様分析・課題発見

**段階判断項目（Phase B/C/D対応）**:
- [ ] **基本実装段階（1-3）**: 基本CRUD・関連機能・機能完成
- [ ] **品質保証段階（4-6）**: 技術負債解消・UI/UX最適化・統合テスト
- [ ] **拡張段階（7-8）**: 高度機能・外部連携・運用最適化

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
**対象**: 基本実装段階（段階1-3）・新規エンティティ・基本CRUD
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

#### Pattern B: 機能拡張（Phase B2-B3, C2-C4, D2-D6等）
**対象**: 基本実装段階（段階2-3）・関連機能・業務ロジック拡張
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

#### Pattern D: 品質保証段階（Phase B4-B5, C5-C6, D7等）
**対象**: 技術負債解消・UI/UX最適化・統合テスト段階
**特徴**: 既存実装の品質向上・保守性改善・運用品質確保

**Phase1（技術負債特定・分析）**:
- [ ] **code-review**（コードレビュー）: 既存コードの問題点・改善箇所特定
- [ ] **dependency-analysis**（依存関係分析）: リファクタリング影響範囲分析
- [ ] **tech-research**（技術調査）: 最新ベストプラクティス・改善手法調査

**Phase2（品質改善実装）**:
- [ ] 対象層に応じた実装系Agent選択（リファクタリング・最適化）
  * F#品質改善: **fsharp-domain**, **fsharp-application**
  * 境界最適化: **contracts-bridge**
  * C#品質改善: **csharp-infrastructure**, **csharp-web-ui**
- [ ] **unit-test**（単体テスト）: リファクタリング安全性確保・テスト充実
- [ ] **code-review**（コードレビュー）: 改善効果確認・コード品質向上

**Phase3（統合検証・品質確認）**:
- [ ] **integration-test**（統合テスト）: E2E動作確認・パフォーマンステスト
- [ ] **spec-compliance**（仕様準拠監査）: 仕様準拠維持確認・品質基準達成確認

#### Pattern E: 拡張段階（Phase D7-D8等）
**対象**: 高度機能・外部連携・運用最適化段階
**特徴**: Claude Code連携・エクスポート機能・監視・ログ・保守機能

**Phase1（外部連携設計・調査）**:
- [ ] **tech-research**（技術調査）: 外部API調査・連携手法・セキュリティ要件
- [ ] **design-review**（設計レビュー）: アーキテクチャ影響確認・統合設計
- [ ] **spec-analysis**（仕様分析）: 外部連携要件・エクスポート仕様分析

**Phase2（拡張機能実装）**:
- [ ] **csharp-infrastructure**（C#インフラ）: 外部API連携・データエクスポート実装
- [ ] **contracts-bridge**（F#↔C#境界）: 外部連携用データ変換・フォーマット変換
- [ ] **csharp-web-ui**（C# Web UI）: エクスポートUI・外部連携画面実装

**Phase3（運用準備・統合確認）**:
- [ ] **integration-test**（統合テスト）: 外部連携テスト・エクスポート機能テスト
- [ ] **code-review**（コードレビュー）: セキュリティ・性能・保守性確認
- [ ] **spec-compliance**（仕様準拠監査）: 外部連携仕様準拠・運用要件達成確認

### 3. 並列実行計画策定
```bash
echo "⚡ 並列実行計画を策定します..."
```

**計画項目**:
- [ ] 選択したSubAgentの並列実行順序決定
- [ ] 各Agentの専門成果物確認（/Doc/08_Organization/Active/Phase_XX/Research/配下）
- [ ] Agent間の成果物継承関係確認
- [ ] MainAgentでの統合・品質確認計画

## 🔧 F#コード実装時の専門SubAgent選択ガイドライン

### F#専門性活用の必須ルール
```markdown
## F#実装対象別SubAgent選択
- F# Domain層（エンティティ・値オブジェクト） → **fsharp-domain**
- F# Application層（サービス・インターフェース） → **fsharp-application**  
- F#/C#境界（TypeConverter・DTO変換） → **contracts-bridge**
- C# Infrastructure層のみ → **csharp-infrastructure**
- C# Web UI層のみ → **csharp-web-ui**
```

### Application層インターフェース実装時の注意
**対象**: `IUbiquitousLanguageService.fs`・`IProjectService.fs`・`IDomainService.fs`
**正しい選択**: **fsharp-application**
**誤った選択**: csharp-infrastructure（C#専門のためF#慣習を見落とす可能性）

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
- [ ] **🔴 並列実行確認**: 同一メッセージ内で複数Task tool呼び出し必須
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