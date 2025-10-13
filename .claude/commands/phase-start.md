# Phase開始準備Command

**目的**: Phase開始前の必須準備プロセスを自動実行  
**対象**: 全Phase開始時  
**実行タイミング**: ユーザーが「Phase開始準備」「新Phase準備」「PhaseXXの実行を開始してください」「PhaseXXを開始してください」宣言時

## コマンド実行内容

### 0. Phase情報自動収集（最初に実行）
- [ ] **プロジェクト現状確認**: Serenaメモリー`project_overview`から現在Phase状況・次期Phase実施予定確認
- [ ] **実装マスタープラン取得**: `/Doc/08_Organization/Rules/縦方向スライス実装マスタープラン.md`から実装順序・Phase特性・基本計画取得
- [ ] **前Phase継承事項確認**: `/Doc/08_Organization/Completed/Phase_XX/Phase_Summary.md`から技術基盤・継承事項・申し送り事項確認
- [ ] **GitHub Issues確認**: Phase A7等の課題対応Phaseの場合、関連Issue（#5, #6等）の内容確認

### 0.5. Phase実行確認（ユーザー承認必須）
- [ ] **推奨Phase提示**: 収集情報から判定した次期実行推奨Phaseをユーザーに提示
- [ ] **Phase選択確認**: ユーザーが実行するPhaseの最終確認・承認取得
  - 推奨Phaseで実行する場合：「はい」で継続
  - 異なるPhaseを実行する場合：希望Phase番号指定（例：「Phase B1」）
  - 実行を取りやめる場合：「キャンセル」で中止
- [ ] **実行Phase確定**: ユーザー承認を得たPhaseで以降の処理を実行

### 1. Phase基本情報確認（必須）
- [ ] **確定Phase情報取得**: ユーザー承認を得たPhaseの詳細情報をマスタープランから取得
- [ ] **Phase目的確定**: マスタープランから該当Phaseの目的・成果目標を取得・ユーザー最終確認
- [ ] **Phase規模自動判定**: 実装マスタープランから該当Phaseの規模（🟢中規模/🟡大規模/🔴超大規模）確認
- [ ] **段階数自動取得**: Phase B（5段階）、C（6段階）、D（7-8段階）を正確に取得
- [ ] **期間予測精度向上**: 規模に応じた5-7/7-9/10-12セッション予測・段階構成の詳細提示

### 2. Phase開始前ディレクトリ・ファイル準備（必須）
- [ ] **Phaseディレクトリ作成**: `/Doc/08_Organization/Active/Phase_XX/` ディレクトリ作成
- [ ] **Phase_Summary.md作成**: Phase概要・成功基準・基本組織方針記録
  ```markdown
  # Phase XX 組織設計・総括

  ## 📊 Phase概要
  - Phase名: Phase XX (具体的名称)
  - Phase規模: [🟢中規模/🟡大規模/🔴超大規模]（マスタープランから自動取得）
  - Phase段階数: XX段階（B:5段階/C:6段階/D:7-8段階）
  - Phase特性: [新機能実装/機能拡張/品質改善/調査分析]
  - 推定期間: XX-XXセッション（規模に応じた予測）
  - 開始予定日: YYYY-MM-DD
  - 完了予定日: YYYY-MM-DD

  ## 🎯 Phase成功基準
  - 機能要件: [具体的な実装目標]
  - 品質要件: [品質基準・テスト基準]
  - 技術基盤: [技術的達成目標]

  ## 📋 段階構成詳細（マスタープランから取得）
  ### 基本実装段階（1-3）
  - 段階1: [基本CRUD・基本機能]
  - 段階2: [関連機能・業務ロジック]
  - 段階3: [機能完成・高度機能]

  ### 品質保証段階（4-6）
  - 段階4: [技術負債解消・品質改善]
  - 段階5: [UI/UX最適化・パフォーマンス]
  - 段階6: [統合テスト・E2E検証]

  ### 拡張段階（7-8・該当Phase時）
  - 段階7: [高度機能・外部連携]
  - 段階8: [運用最適化・監視・保守]

  ## 🏢 Phase組織設計方針
  - 基本方針: [SubAgentプール活用方針]
  - Step別組織構成概要: [予定Step構成]

  ## 📋 全Step実行プロセス（Step1詳細計画時記録）
  [Step1完了時に更新予定]

  ## 📊 Phase総括レポート（Phase完了時記録）
  [Phase完了時に更新予定]
  ```

### 3. Phase固有情報準備
- [ ] **関連仕様書特定**: Phase対象機能の機能仕様書セクション特定（実装マスタープラン参照）
- [ ] **技術基盤継承確認**: 前Phase完了で確立された技術基盤・検証済み項目の継承活用確認
- [ ] **技術的前提条件整理**: Phase固有の開発環境・依存関係・設定要件確認

### 4. 品質保証準備
- [ ] **仕様準拠基準設定**: 機能仕様書準拠マトリックス作成準備
- [ ] **TDD実践計画**: Red-Green-Refactorサイクル適用方針確認
- [ ] **品質確認Command準備**: step-end-review・spec-compliance-check実行準備

### 5. Phase開始前確認・承認
- [ ] **準備完了確認**: 全必須項目の準備完了確認
- [ ] **ユーザー承認**: Phase開始準備完了・Phase実行開始の承認取得
- [ ] **次Action提示**: ユーザーによるStep1開始決定・「Step開始」宣言でstep-start実行

## 実行後アクション

### Phase準備完了時
✅ **Phase開始準備完了** → Phase枠組み確立・Step1開始可能状態  
⚠️ **準備不足項目あり** → 不足項目の完了・再確認実施  
❌ **Phase目的不明確** → ユーザーとのPhase目的再確認・要件明確化

### Step1開始への移行
- **ユーザー承認取得**: Step1実行開始の最終承認
- **step-start実行**: ユーザーが「Step開始」宣言時にstep-start Command自動実行
- **独立実行**: step-startはユーザーの意思決定による独立したプロセス

### 次Command連携
- **step-start Command**: Step1組織設計・SubAgent選択・実行準備
- **step-end-review Command**: Step1完了時の包括的品質確認

## ファイル出力仕様

### ディレクトリ構造確認
```
/Doc/08_Organization/Active/Phase_XX/
├── Phase_Summary.md         # Phase全体概要・総括（phase-start作成）
└── [全Stepファイルは各Step開始時にstep-startが作成]
```

### 成果物出力場所
```
/Doc/08_Organization/Active/Phase_XX/Research/   # SubAgent分析結果出力場所（step-start実行時準備）
```

## 実行トリガー
- ユーザー発言: "Phase開始準備"、"新Phase準備"
- ユーザー発言: "Phase XX開始準備"、"Phase準備実施"
- ユーザー発言: "PhaseXXの実行を開始してください"、"PhaseXXを開始してください"
- ユーザー発言: "Phase XX実行開始"、"Phase XXを実行してください"
- Phase完了後の新Phase移行時の提案実行

## 関連Command・プロセス
- **前提**: Phase完了または初回Phase開始
- **連携**: step-start → subagent-selection → SubAgent並列実行 → step-end-review
- **参考**: ファイル管理規約・組織管理運用マニュアル準拠