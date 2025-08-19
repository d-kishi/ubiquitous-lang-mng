# Step開始準備Command

**目的**: Step開始時の必須準備プロセス・組織設計を自動実行  
**対象**: 全Step開始時（Step1, Step2, Step3...）  
**実行タイミング**: ユーザーが「Step開始」「StepXX開始」「次Step開始」宣言時

## コマンド実行内容

### 1. Step情報収集・確認（必須）
- [ ] **Phase状況確認**: `/Doc/08_Organization/Active/Phase_XX/`配下の現在Phase状況確認
- [ ] **前Step完了確認**: 前Step完了レビュー結果・成果物出力状況確認
- [ ] **Phase計画確認**: `Phase_Summary.md`から全Step構成・当該Step位置確認
- [ ] **次Step判定**: Phase計画から実行すべき次Stepの自動判定

### 2. Step組織設計（コア機能）
- [ ] **Step特性判定**: 当該Stepの作業特性判断（分析/実装/テスト/統合/品質保証）
- [ ] **🔧 subagent-selection Command実行**: [subagent-selection Command](./.claude/commands/subagent-selection.md)でStep作業特性に基づくSubAgent組み合わせ選択
- [ ] **SubAgent選択結果確認**: subagent-selection実行結果の確認・選択SubAgent組み合わせの取得
- [ ] **並列実行計画策定**: 選択SubAgentの並列実行計画・効率化戦略
- [ ] **StepXX_[内容].md作成**: Step組織設計記録ファイル作成（Step1の場合はStep01_Analysis.md）

### 3. Step固有準備
- [ ] **Step1特化準備**（Step1の場合）:
  - 分析範囲・調査項目確定
  - 技術検証ポイント特定
  - 成果物出力先準備（`/Doc/05_Research/Phase_XX/`）
- [ ] **Step2以降準備**（Step2以降の場合）:
  - 前Step成果物確認・分析結果読み込み
    - `/Doc/05_Research/Phase_XX/SpecAnalysis_*.md` - 仕様分析結果
    - `/Doc/05_Research/Phase_XX/TechResearch_*.md` - 技術調査結果
    - `/Doc/05_Research/Phase_XX/DependencyAnalysis_*.md` - 依存関係分析
    - `/Doc/05_Research/Phase_XX/Spec_Compliance_Matrix.md` - 仕様準拠マトリックス
  - Step1分析結果活用計画策定
  - 実装対象機能・範囲確定
- [ ] **仕様書該当セクション特定**: Step対象機能の機能仕様書セクション確認
- [ ] **仕様準拠マトリックス準備**: 実装すべき仕様・制約の整理

### 4. 技術的前提条件確認
- [ ] **開発環境確認**: 必要な開発環境・ツール・依存関係の確認
- [ ] **技術基盤継承**: 前Stepで確立された技術基盤・パターンの継承確認
- [ ] **データベース状況確認**: マイグレーション・テストデータの状況確認
- [ ] **ビルド・テスト状況確認**: 0エラー0警告状態・テスト成功率確認

### 5. 品質保証準備
- [ ] **TDD実践計画**: Red-Green-Refactorサイクルの適用計画
- [ ] **テスト戦略設定**: 単体テスト・統合テスト・E2Eテストの計画
- [ ] **品質確認基準設定**: Step完了時の品質確認基準・成功基準設定
- [ ] **カバレッジ目標設定**: テストカバレッジ目標・測定方法確認

### 6. Step開始承認・実行準備
- [ ] **Step目的明確化**: 当該Stepの具体的目的・期待成果の明確化
- [ ] **作業計画提示**: 推定所要時間・主要作業項目・マイルストーンの提示
- [ ] **SubAgent実行計画提示**: 選択SubAgent・並列実行計画・効率化戦略をユーザーに提示
- [ ] **リスク・制約確認**: 技術リスク・時間制約・依存関係リスクの確認
- [ ] **ユーザー承認取得**: Step開始・組織設計・SubAgent実行計画の最終承認

## StepXX.md作成テンプレート

### Step1（分析）の場合
```markdown
# Step 01 組織設計・実行記録

## 📋 Step概要
- Step名: Step01 Analysis
- 作業特性: 分析・技術調査・計画詳細化
- 推定期間: XX セッション
- 開始日: YYYY-MM-DD

## 🏢 組織設計
### SubAgent構成
[subagent-selection Command実行結果に基づく]

### 並列実行計画
[subagent-selection Command実行結果に基づく並列実行戦略]

## 🎯 Step成功基準
- 包括的分析完了・技術検証完了
- 実装計画詳細化・次Step準備完了
- 成果物品質確認・活用準備完了

## 📊 Step実行記録（随時更新）
[実施中に更新]

## ✅ Step終了時レビュー
[Step完了時に更新]
```

### Step2以降（実装）の場合
```markdown
# Step XX 組織設計・実行記録

## 📋 Step概要
- Step名: Step02 [実装内容]
- 作業特性: 実装・テスト・統合
- 推定期間: XX セッション
- 開始日: YYYY-MM-DD

## 🏢 組織設計
### SubAgent構成
[Step作業特性に応じた構成]

### Step1分析結果活用
- 活用する分析結果・技術調査結果
- 実装方針・技術選択の根拠

## 🎯 Step成功基準
[Step固有の成功基準]

## 📊 Step実行記録（随時更新）
[実施中に更新]

## ✅ Step終了時レビュー
[Step完了時に更新]
```

## 実行後アクション

### Step準備完了時
✅ **Step開始準備完了** → ユーザー承認取得後にSubAgent並列実行・Step作業開始  
⚠️ **準備不足項目あり** → 不足項目完了・再確認実施  
❌ **前Step未完了** → 前Step完了確認・完了後再実行

### SubAgent実行開始条件
- **必須**: ユーザーによるSubAgent実行計画承認
- **確認事項**: 選択SubAgent・並列実行戦略・推定所要時間の最終承認
- **実行開始**: 承認取得後にSubAgent並列実行開始

### 次プロセス連携
- **SubAgent並列実行**: ユーザー承認後の専門作業実行
- **step-end-review**: Step完了時の包括的品質確認・レビュー
- **次step-start**: 当Step完了後の次Step準備

## 実行トリガー
- ユーザー発言: "Step開始"、"次Step開始"
- ユーザー発言: "StepXX開始"、"Step XX開始してください"
- ユーザー発言: "Step開始準備"、"次のStep準備"
- Phase実行中の自然な次Step移行時

## 関連Command・プロセス
- **前提**: phase-start完了または前Step完了
- **連携**: SubAgent並列実行 → step-end-review → 次step-start
- **参考**: 組織管理運用マニュアル・ファイル管理規約準拠