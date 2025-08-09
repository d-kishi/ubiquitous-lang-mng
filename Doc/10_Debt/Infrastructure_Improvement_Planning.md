# インフラ・基盤改善計画

**管理方針**: [ADR_013: 組織管理サイクル運用規則](/Doc/07_Decisions/ADR_013_組織管理サイクル運用規則.md#技術負債管理運用規則)

## 📊 目的

開発基盤・インフラストラクチャの改善・最新化により、開発効率と品質の長期的向上を図る。

---

## Gemini連携のMCP移行

### 実装理由・背景
- **現在**: bash経由でのGemini連携（シェルインジェクションリスク・非構造化データ交換）
- **課題**: セキュリティリスク・エラー処理の複雑性・拡張性の制約
- **解決策**: Model Context Protocol (MCP) による標準化された統合

### 優先度とユーザー価値
- 🟡 **中優先度**: 開発者体験・開発効率向上
- **価値**: 40-60%の開発効率向上・エラー削減・セキュリティ強化
- **影響範囲**: 技術調査・コードレビュー・設計検証の全プロセス

### 技術的依存関係・制約
- **前提条件**: MCP プロトコル理解・SDK習得（40-60時間学習）
- **依存技術**: @anthropic/mcp-server-gemini、JSON-RPC 2.0
- **制約**: エコシステム発展途上・Anthropic仕様依存
- **互換性**: 既存bash方式との並行運用期間必要

### 工数見積もり・実装計画
- **学習・検証フェーズ**: 2-3週間
  - MCPプロトコル理解・基本環境構築
  - Gemini連携テスト・性能比較測定
- **段階的移行フェーズ**: 4-6週間
  - 基本Gemini連携のMCP化
  - データベース・GitHub統合追加
  - 全面MCP環境への移行
- **推奨実装時期**: **Phase B1開始前**（Phase A3完了後）

### 期待効果
- **セキュリティ**: シェルインジェクション等のリスク排除
- **開発効率**: 構造化データ交換による精密な情報共有
- **拡張性**: 標準プロトコルによる新ツール統合の低コスト化
- **品質**: 型安全性によるエラー削減・予期しない不具合防止

### 実装方針
1. **Phase A3**: 現行bash方式継続（メール送信基盤優先）
2. **Phase B1開始前**: MCP Gemini サーバー検証・効果測定
3. **効果確認後**: データベース・GitHub統合等の段階的MCP化
4. **Phase C移行時**: 全面MCP統合環境完成

### 関連ドキュメント
- **詳細調査結果**: 2025-07-24 セッション記録
- **現行連携ガイド**: `/Doc/09_Environment/Gemini連携実践ガイド.md`

---

## SubAgent活用組織化戦略（改訂版）

### 実装理由・背景
- **現在**: 単一Agent内での順次専門役割実行（90-120分）・Phase毎の組織再設計負荷
- **課題**: Context圧迫・AutoCompact頻発・役割切り替え効率低下・組織パターンの再利用性欠如
- **解決策**: 事前定義SubAgentプール方式による真の並行組織化・専門性特化・再利用性向上

### 🎯 事前定義SubAgentプール戦略

#### **実装方式：プロジェクトサブエージェント採用**
- **選択理由**: プロジェクト固有知識の共有・チーム開発での一貫性・Git管理による改善履歴追跡
- **配置場所**: `.claude/agents/` ディレクトリ（プロジェクトルート）
- **管理方法**: Gitによるバージョン管理・チーム共有・継続的改善
- **利点**: プロジェクトコンテキスト自動継承・環境設定共有・標準化された開発アプローチ

#### **標準SubAgentパターン定義**

##### 🔷 調査分析系Agent群
1. **技術調査Agent** - Gemini連携・最新情報収集・ベストプラクティス調査
2. **仕様分析Agent** - 要件定義書・仕様書の詳細分析・仕様準拠マトリックス作成
3. **設計レビューAgent** - データベース設計・システム設計の整合性確認
4. **依存関係分析Agent** - 技術的依存関係・実装順序の特定

##### 🔶 実装系Agent群
5. **F# Domain層Agent** - ドメインモデル・ビジネスロジック実装
6. **F# Application層Agent** - ユースケース・アプリケーションサービス実装
7. **Contracts層Agent** - F#↔C#型変換・DTO設計・TypeConverter実装（★重要）
8. **C# Infrastructure層Agent** - Repository・Entity Framework実装
9. **C# Web UI Agent** - Blazor Server・Razorページ実装

##### 🔵 品質保証系Agent群
10. **単体テストAgent** - TDD実践・Red-Green-Refactorサイクル
11. **統合テストAgent** - WebApplicationFactory・E2Eテスト
12. **コードレビューAgent** - 品質確認・リファクタリング提案
13. **仕様準拠監査Agent** - 仕様違反検出・修正提案

#### **Contracts層専門Agentの特別な重要性**
- **責務**: F#とC#の架け橋として双方向型変換を担当
- **協調**: F# Domain層Agent・C# Infrastructure層Agent両方と密接に連携
- **品質保証**: 型安全性・null安全性の境界での保証
- **変更管理**: 両側の変更に対する影響分析と適応

### 🔄 新組織運用体系

```yaml
SubAgent Pool（事前定義・常設）:
  調査分析系: [技術調査, 仕様分析, 設計レビュー, 依存関係分析]
  実装系: [F# Domain, F# Application, Contracts, C# Infrastructure, C# Web UI]  
  品質保証系: [単体テスト, 統合テスト, コードレビュー, 仕様準拠監査]

Phase実行パターン:
  Step1（調査分析）:
    使用Agent: 調査分析系（4並列）+ MainAgent（統合）
    所要時間: 45-60分（現在90分から短縮）
  
  Step2-N（実装）:
    使用Agent: 実装系（必要な層を選択）+ テストAgent
    並列パターン:
      - 層別並列: 各層Agent同時実行
      - 機能別並列: 同一機能を全層で並列実装
    Contracts層Agent: F#/C#境界で常時稼働
  
  Step終了時:
    使用Agent: 品質保証系による総合レビュー
    成果物統合: MainAgentによる最終調整

Phase間での再利用:
  - 全SubAgentは常設・学習効果蓄積
  - Phase特性に応じた組み合わせ選択
  - 組織設計の簡略化（選択のみ）
```

### 🚀 革新的メリット

1. **組織設計の簡略化**
   - Phase毎の組織再設計不要
   - 標準パターンから選択するだけ
   - 組織管理オーバーヘッド削減

2. **Agent学習効果の蓄積**
   - 各SubAgentが専門知識を蓄積
   - プロジェクト全体の文脈理解向上
   - パターン認識能力の継続的向上

3. **柔軟な編成**
   - Phase特性に応じた最適な組み合わせ
   - 必要なAgentのみ活用
   - 動的な役割調整可能

4. **Context効率の最大化**
   - 専門特化によるContext最適化
   - AutoCompact大幅削減
   - 必要情報の集約化

### 優先度とユーザー価値
- 🔴 **最高優先度**: 組織管理簡略化・開発効率劇的向上
- **価値**: 組織化効率50-60%向上・品質向上・管理負荷90%削減
- **影響範囲**: 全Phase・全Step・ADR_013組織管理サイクル全面刷新

### 実装計画
#### **Phase 1: 基盤整備**（即座実施可能）
- `.claude/agents/` ディレクトリ構造作成
- プロジェクトサブエージェント定義ファイル作成
- 各Agent役割・プロンプトテンプレート明確化
- 技術負債解消作業での初回適用準備

#### **Phase 2: 技術負債解消での初回適用**（Phase B1着手前）
- **TECH-001～004修正作業**: SubAgent初回実証実験
  - 実装系Agent並列活用（Contracts層・Infrastructure層・Web UI層）
  - 品質保証系Agent活用（テスト・仕様準拠監査）
- 効果測定・調整・改善点抽出
- Phase B1に向けた最適化

#### **Phase 3: Phase B1以降での本格展開**
- Phase B1 Step1: 調査分析系4並列実行
- Step2以降: 実装系並列適用（技術負債解消での経験活用）
- 全Phase標準化・継続的改善サイクル確立

### 期待効果
- **開発時間**: 50-60%短縮（並列化・再利用効果）
- **品質向上**: 専門性向上・仕様準拠確実性向上
- **管理負荷**: 90%削減（組織設計不要化）
- **Context効率**: AutoCompact 80%削減

### 実装方針
1. **即座**: `.claude/agents/`配下にプロジェクトサブエージェント定義作成
2. **技術負債解消時**: 初回実証実験・効果測定（TECH-001～004修正）
3. **Phase B1開始時**: 本格展開・調査分析系並列化
4. **継続的**: Git管理による改善・最適化・パターン蓄積

### ディレクトリ構造
```
.claude/
  agents/                    # SubAgent直下配置（仕様準拠）
    tech-research.md         # 技術調査Agent
    spec-analysis.md         # 仕様分析Agent
    design-review.md         # 設計レビューAgent  
    dependency-analysis.md   # 依存関係分析Agent
    fsharp-domain.md         # F# Domain層Agent
    fsharp-application.md    # F# Application層Agent
    contracts-bridge.md      # F#↔C#境界Agent（重要）
    csharp-infrastructure.md # C# Infrastructure層Agent
    csharp-web-ui.md         # C# Web UI層Agent
    unit-test.md             # 単体テストAgent
    integration-test.md      # 統合テストAgent
    code-review.md           # コードレビューAgent
    spec-compliance.md       # 仕様準拠監査Agent
```

### SerenaMCPとSubAgent組み合わせ戦略

#### **制約と対策**
- **SerenaMCP制約**: F#非対応（.fs/.fsx/.fsi ファイル）・シンボリック操作不可
- **対策**: Agent別の明示的ツール使用指示による適切な使い分け

#### **Agent別ツール使用方針**
```yaml
F#系Agent（Domain・Application）:
  禁止ツール:
    - mcp__serena__* のシンボリック操作ツール全般
  推奨ツール:
    - Read/Write/Edit/MultiEdit（標準ツール）
    - Grep/Glob（ファイル検索）
  
Contracts層Agent:
  使い分け:
    - F#側コード: 標準ツールのみ
    - C#側コード: SerenaMCP活用可能
    - TypeConverter実装: 標準ツール推奨（両言語境界のため）

C#系Agent（Infrastructure・Web UI）:
  推奨ツール:
    - mcp__serena__* 全機能積極活用
    - find_symbol, replace_symbol_body等のシンボリック操作
    - 特にBlazorコンポーネント・Entity Framework操作に最適

品質保証系Agent:
  使い分け:
    - C#テストコード: SerenaMCP活用
    - F#テストコード: 標準ツール使用
```

#### **SubAgent定義ファイルでの明示指示**
各Agent定義ファイルに以下を記載：
- 使用可能ツール・禁止ツール一覧
- ファイル種別による使い分け指示
- エラー回避のための注意事項

### 関連技術課題と対策
- **Context管理**: 統合Agent（Main）による情報集約・配分
- **品質保証**: 各SubAgent標準出力フォーマット定義
- **ツール使用最適化**: 言語別・Agent別の適切なツール選択指示
- **エラーハンドリング**: SubAgent失敗時のフォールバック戦略確立
- **協調プロトコル**: SubAgent間通信・同期方法の標準化

---

**記録日**: 2025-07-24（基盤改善）・2025-08-01（SubAgent戦略追加）・2025-08-08（事前定義プール戦略改訂）  
**記録者**: Claude Code  
**ステータス**: 🔴 優先実施（SubAgentプール戦略により即座実施可能）