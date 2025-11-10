# Step 06 組織設計・実行記録

## 🎯 Step目的（Why）

**このStepで達成すべきこと**:
- GitHub Codespacesで夜間作業自動化の実証検証
- DevContainer環境での定型Command実行確認
- 時間削減効果50%以上の実測確認
- Issue #51 Phase 2本格運用開始の可否判断（Go/No-Go決定）

**Phase全体における位置づけ**:
- **Phase全体の課題**: Claude Code on the Web検証によりIssue #51の前提条件が成立しないことが判明
- **このStepの役割**: Issue #51の本質的な目的（夜間作業自動化）を達成するための代替手段検証

**関連Issue**: #51（Claude Code on the Web による開発プロセス自動化）
- **Phase 1結果**: Claude Code on the Webは.NETプロジェクトに不向き（DevContainer起動不可・dotnet実行不可・MCP Server接続不可）
- **Phase 2方針転換**: GitHub Codespacesで再検証
- **期待効果**: DevContainer環境が動作 → dotnet/MCP Server利用可能 → Issue #51の本質的な目的を達成

---

## 📋 Step概要

- **Step名**: Step 6: GitHub Codespaces 検証・夜間作業自動化実証
- **対応Issue**: #51 Phase2
- **作業特性**: 分析・技術検証（GitHub Codespaces検証・定型Command実行・効果測定）
- **推定期間**: 6-9時間
- **開始日**: 未定（Phase B-F2 Step5完了後）

## 🏢 組織設計

### Step特性判定
- **作業特性**: 分析・技術検証
- **段階種別**: 該当なし（Phase B-F2は基盤整備Phase）
- **主要作業**: GitHub Codespaces環境構築・DevContainer動作確認・定型Command実行検証・効果測定

### SubAgent構成

**推奨SubAgent**: tech-research Agent

**選定理由**:
- GitHub Codespaces環境構築には技術検証が必要
- DevContainer動作確認には実験的アプローチが必要
- 効果測定には定量的データ収集・分析が必要
- tech-research AgentはWebSearch・WebFetch・技術調査に特化

**並列実行計画**:
- Step6は単一Agentのため並列実行不要
- ただし、GitHub Codespaces内での並列タスク実行検証が主要目的の1つ

### Step5成果物必須参照

**Phase_Summary.mdのStep間成果物参照マトリックスより**:

#### 必須参照ファイル

1. **Issue_51_Phase1_検証結果.md**
   - **配置**: `/Doc/99_Others/`
   - **重点参照セクション**: 全体（特に「判明した制約事項」「方針転換の提案」）
   - **活用目的**: Claude Code on the Webの制約理解・GitHub Codespaces選定理由

2. **Step05_Web版検証・並列タスク実行.md**
   - **配置**: `/Doc/08_Organization/Active/Phase_B-F2/`
   - **重点参照セクション**: Stage 1実行記録（検証結果・総合評価）
   - **活用目的**: 前Step教訓の活用・検証アプローチの改善

3. **Web_Version_Verification_Report.md**
   - **配置**: `/Doc/08_Organization/Active/Phase_B-F2/Research/`
   - **重点参照セクション**: 全体（特に「推奨される使い方」「代替手段の提案」）
   - **活用目的**: ハイブリッド開発アプローチの理解

#### 全Step共通参照ファイル

4. **Phase_B-F2_Revised_Implementation_Plan.md**
   - **配置**: `/Doc/08_Organization/Active/Phase_B-F2/Research/`
   - **重点参照セクション**: リスク管理計画（📊セクション）
   - **活用目的**: リスク要因・対策・トリガー確認

5. **Phase_B-F2_Revised_Implementation_Plan.md**
   - **配置**: `/Doc/08_Organization/Active/Phase_B-F2/Research/`
   - **重点参照セクション**: 効果測定計画（📈セクション）
   - **活用目的**: DevContainer・GitHub Codespaces・代替手段測定方法

### Step5分析結果活用

**活用する分析結果**:
- Claude Code on the Webの制約事項（5項目）
- 適用範囲の明確化（ドキュメント作業は適・開発作業は不適）
- ハイブリッド開発アプローチの可能性

**技術検証方針**:
- GitHub Codespaces環境での実際の作業を通じた効果測定
- 定型Command実行による時間削減効果の定量的評価
- DevContainer + MCP Server環境の実用性確認

## 🎯 Step成功基準

### 機能要件
- ✅ GitHub Codespaces環境構築完了（2-3時間）
- ✅ DevContainer環境動作確認完了（dotnet build/test成功）
- ✅ MCP Server接続確認完了（Serena/Playwright動作確認）
- ✅ 定型Command実行検証完了（weekly-retrospective等）
- ✅ 並列タスク実行検証完了（4 Command同時実行）
- ✅ 効果測定完了（時間削減率測定）

### 品質要件
- ✅ 時間削減効果50%以上確認
- ✅ 品質問題なし（0 Warning/0 Error維持）
- ✅ MCP Server動作確認（Serena/Playwright）
- ✅ DevContainer環境の安定性確認

### 成果物要件
- ✅ GitHub Codespaces検証レポート作成
- ✅ 効果測定結果（時間削減率・品質・コスト）
- ✅ Phase 2実施判断材料作成（Go/No-Go判断）
- ✅ ADR_0XX作成（GitHub Codespaces統合決定 or 代替手段選定）

## 🏗️ Stage構成設計

### Stage 1: GitHub Codespaces環境構築・基本動作確認（2-3時間）

**🎯 検証目的**: GitHub Codespaces + DevContainer環境の構築・動作確認

**実施内容**:

**1-1: GitHub Codespaces起動・DevContainer環境構築**（1時間）
- GitHub Codespaces作成（feature/PhaseB-F2ブランチ）
- DevContainer環境起動確認（.devcontainer/devcontainer.json認識）
- VS Code拡張機能自動インストール確認（15個）
- 環境変数設定確認（PostgreSQL接続・SMTP設定等）

**1-2: .NET SDK・ビルド環境確認**（30分）
- dotnetコマンド動作確認
- `dotnet build` 実行確認（0 Warning / 0 Error達成）
- `dotnet test` 実行確認（341テスト想定）
- ビルド時間測定（ベンチマーク）

**1-3: MCP Server接続確認**（30分）
- Serena MCP動作確認（mcp__serena__check_onboarding_performed実行）
- Playwright MCP動作確認（browser_snapshot等）
- MCP Serverツール一覧確認
- 権限設定確認（settings.local.json）

**1-4: Claude Code CLI統合確認**（30分）
- GitHub Codespaces内でClaude Code CLI起動
- 基本的なファイル操作確認（Read/Write/Edit）
- Bashコマンド実行確認
- Context管理確認（AutoCompact動作）

**成果物**:
- GitHub Codespaces環境構築レポート（Section 1）
- DevContainer動作確認結果
- ビルド・テスト実行結果（0 Warning/0 Error）
- MCP Server動作確認結果

**完了条件**:
- ✅ GitHub Codespaces環境が正常に起動
- ✅ DevContainer環境が正常に動作（dotnet build/test成功）
- ✅ MCP Server接続が正常に動作（Serena/Playwright）
- ✅ 品質問題なし（0 Warning/0 Error維持）

---

### Stage 2: 定型Command実行検証・非同期実行確認（2-3時間）

**🎯 検証目的**: Issue #51の3大特徴（定型Command・非同期実行・PR自動作成）の検証

**実施内容**:

**2-1: 定型Command実行検証**（1時間）
- `weekly-retrospective` Command実行
- `spec-compliance-check` Command実行
- `session-start` / `session-end` Command実行
- 実行時間測定（各Command）

**2-2: 非同期実行確認**（30分）
- 時間のかかるタスクを投入（5分以上）
- GitHub Codespaces接続を切断
- 5-10分待機
- 再接続してタスク継続確認
- 完了後、結果確認

**2-3: PR自動作成機能確認**（30分）
- タスク完了後、PRが作成されるか確認
- PR内容の妥当性確認
- 手動PR作成との比較

**2-4: Claude Code on the Webとの比較**（30分）
- 同一タスクの実行時間比較
- 機能差異の確認
- 使い分け基準の整理

**成果物**:
- 定型Command実行検証レポート（Section 2）
- 各Command実行時間測定結果
- 非同期実行確認結果
- PR自動作成確認結果
- Claude Code on the Webとの比較表

**完了条件**:
- ✅ 定型Command（weekly-retrospective等）が正常に実行
- ✅ 非同期実行が正常に動作（接続切断後も継続）
- ✅ 品質問題なし（0 Warning/0 Error維持）

---

### Stage 3: 並列タスク実行検証・効果測定・Go/No-Go判断（2-3時間）

**🎯 検証目的**: Issue #51の本質的な目的（夜間作業自動化・時間削減50%以上）の達成確認

**実施内容**:

**3-1: 並列タスク実行検証**（1時間）
- 4つの定型Commandを並列実行:
  - `weekly-retrospective`（30分想定）
  - `spec-compliance-check`（20分想定）
  - ドキュメント更新タスク（15分想定）
  - ADR作成タスク（25分想定）
- 逐次実行時間: 90分（30+20+15+25）
- 並列実行時間: 30分（最長タスク基準）
- 時間削減率: 67%（60分削減）

**3-2: 効果測定**（30分）
- 時間削減率の測定（目標: 50%以上）
- 品質維持確認（0 Warning/0 Error）
- コスト評価（月60時間無料枠内か、追加料金発生か）
- Claude Code on the Webとの比較

**3-3: Go/No-Go判断**（30分）
- 判断基準の確認:
  - ✅ 時間削減効果50%以上
  - ✅ 品質問題なし（0 Warning/0 Error維持）
  - ✅ コスト許容範囲内
  - ✅ MCP Server動作確認
- Go/No-Go決定
- Phase 2本格運用開始の可否判断

**3-4: ADR作成**（30分）
- 技術決定の記録:
  - ADR_0XX: GitHub Codespaces統合決定（Goの場合）
  - または: 代替手段選定（No-Goの場合）
- 判断根拠の記録
- リスク・制約の記録

**成果物**:
- 並列タスク実行検証レポート（Section 3）
- 効果測定結果（時間削減率・品質・コスト）
- Go/No-Go判断結果
- ADR_0XX（GitHub Codespaces統合決定 or 代替手段選定）
- GitHub Codespaces検証レポート統合版

**完了条件**:
- ✅ 並列タスク実行検証完了（4 Command同時実行成功）
- ✅ 時間削減効果50%以上確認
- ✅ Go/No-Go判断完了（Phase 2実施判断明確化）
- ✅ ADR作成完了（技術決定の記録）

---

## 📊 Step実行記録

（実施中に更新）

---

## ✅ Step終了時レビュー

（Step完了時に更新）

---

**作成日**: 2025-11-07
**最終更新**: 2025-11-07（Step6計画作成時）
