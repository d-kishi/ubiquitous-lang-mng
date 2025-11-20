# GitHub Open Issue全35件分析レポート

**調査完了日**: 2025-11-19
**調査対象**: GitHub Open Issues全35件（#70～#8）
**調査目的**: Phase B3開始前に対処すべきタスクの整理・優先順位付け

---

## エグゼクティブサマリ

### 重要判断

- **Phase B3開始前必須対処**: 2件（#59, #57）- Phase B-F2積み残しUI実装
- **Phase B3前推奨**: 3件（#46, #52, #53）- 組織基盤・E2E基盤
- **Phase C前推奨**: 5件（#67, #55 Phase2, #54 Phase3, #19, #44）
- **懸案事項**:
  - Issue #65（.NET 10移行）: Phase B3-B5期間中またはPhase C前推奨
  - Issue #61（Cursor Ver2移行）: Phase C期間中検証推奨
- **Phase B-F3新設**: **不要**（Phase B3前必須タスクは3-5時間で完了可能）

### 全体統計

- **合計Open Issues**: 35件
- **Phase B3前必須**: 2件（3-5時間）
- **Phase B3前推奨**: 3件（6-8.5時間）
- **Phase C前推奨**: 5件（40-50時間）
- **懸案事項**: 2件（#65: 8-12h, #61: 9-13h）
- **後回し可**: 23件（Phase D以降/監視中/低優先度）

---

## 1. Phase B3開始前対処必須（高優先度）

### Issue #59: E2Eテストシナリオ再設計（GitHub Issue #57, #53解決後）

**GitHub**: https://github.com/d-kishi/ubiquitous-lang-mng/issues/59

**現在状況**:
- Phase B-F2 Step7で中断（UserProjects UI実装待ち）
- TypeScript E2Eテスト（`user-projects.spec.ts`）は既に作成済み（136行・3シナリオ）
- 問題発見: `member-management-link`要素が`ProjectList.razor`に未実装

**Phase B3への影響**: **高**

**推奨タイミング**: **Phase B3開始直後（UI実装と同時）**

**理由**:
1. Phase B-F2 Step7で発見：`member-management-link`要素が未実装
2. TypeScript E2Eテストは既に作成済み（実装済み率100%・実行可能率0%）
3. Phase B3でProjectList.razorにリンク実装が必要（数行のUI追加のみ）
4. UI実装完了後、E2Eテスト再実行で即座に検証完了

**実装内容**:
- `ProjectList.razor`にプロジェクト一覧→メンバー管理画面への遷移リンク追加
- `data-testid`属性（`member-management-link`）設定
- `ProjectMembers.razor`は既存（`/projects/{ProjectId:guid}/members`）

**推定時間**: 2-3時間
- UI実装: 1時間
- E2Eテスト再実行・検証: 1-2時間

**前提条件**: なし（即座実施可能）

**成功基準**:
- `user-projects.spec.ts` 3シナリオ全成功（100%）
- Issue #59 Close準備完了

---

### Issue #57: Playwright E2Eテスト実装責任の明確化（Phase B3開始前対応）

**GitHub**: https://github.com/d-kishi/ubiquitous-lang-mng/issues/57

**現在状況**:
- Phase B-F2完了（e2e-test Agent新設・ADR_024完成・subagent-patterns Skills更新）
- 完了条件6項目中5項目達成済み
- 残り1項目「動作確認成功」のみ

**Phase B3への影響**: **中**

**推奨タイミング**: **Phase B3開始直後（Issue #59と同時）**

**理由**:
1. e2e-test Agent既に作成済み（14種類目SubAgent）
2. Issue #59完了後の運用検証が必要
3. Phase B3以降のE2Eテスト作成プロセス確立のため

**実装内容**:
- Issue #59完了後、e2e-test Agent動作確認
- 動作確認成功確認
- Issue #57 Close

**推定時間**: 5分-1時間
- 動作確認: 5分
- ドキュメント更新（必要に応じて）: 30分-1時間

**前提条件**: Issue #59完了

**成功基準**: Issue #57 Close完了

---

## 2. Phase B3開始前推奨（中優先度）

### Issue #46: Playwright統合後のCommands/SubAgent刷新

**GitHub**: https://github.com/d-kishi/ubiquitous-lang-mng/issues/46

**現在状況**:
- 当初計画（Phase B2完了後即座統合）から変更
- Phase B-F2 Step6で方針変更: TypeScript専用判明・段階的採用へ変更（2025-11-18）
- 新方針: Phase B-F2（現状維持）→ Phase B3（healer評価）→ Phase B4（planner評価）

**Phase B3への影響**: **中**

**推奨タイミング**: **Phase B3開始前**

**理由**:
1. Commands更新は当初計画から延期済み
2. Phase B3開始前に整理推奨（Commands更新延期の正式決定）
3. Phase B3以降のPlaywright運用方針明確化

**実装内容**:
- Commands更新延期の正式決定
- Phase B3-B4での段階的評価計画確認
- Issue #46更新（方針変更記録）

**推定時間**: 1-1.5時間

**前提条件**: なし（即座実施可能）

**成功基準**: Commands更新延期判断完了・Issue #46更新完了

---

### Issue #52: Phase A（認証・ユーザー管理）機能のE2Eテスト実装

**GitHub**: https://github.com/d-kishi/ubiquitous-lang-mng/issues/52

**現在状況**:
- 6/19シナリオ完了（31.6%）
- 10シナリオがUI未実装でブロック中（UserManagement機能未実装）

**Phase B3への影響**: **低**（Phase A機能）

**推奨タイミング**: **Phase B3期間中（並行作業）**

**理由**:
1. Phase A残存作業（UserManagement UI実装）
2. Phase B3機能とは独立
3. 完了により全E2E基盤確立（Phase A+B2+B3）

**実装内容**:
- UserManagement UI実装（10画面）
- UserManagement E2Eテスト実装（10シナリオ）

**推定時間**: 8-12時間
- UI実装: 5-8時間
- E2Eテスト実装: 3-4時間

**前提条件**: なし（Phase B3実装と独立）

**成功基準**: UserManagement E2Eテスト 10/10シナリオ成功

---

### Issue #53: [Process Improvement] テスト失敗時の判断プロセス確立（ADR_022）

**GitHub**: https://github.com/d-kishi/ubiquitous-lang-mng/issues/53

**現在状況**: 未着手（Phase B2 Step4で問題発見）

**Phase B3への影響**: **中**

**推奨タイミング**: **Phase B3開始前**

**理由**:
1. Phase B2で「テスト失敗→仕様確認せず修正」の問題発生
2. Phase B3でテスト失敗時の正しいプロセス適用必須
3. ADR_022作成・CLAUDE.md更新が必要

**実装内容**:
- テスト失敗時の判断フロー策定
- ADR_022作成（テスト失敗時判断プロセス決定）
- CLAUDE.md更新（プロセス明記）
- step-start Command更新（テスト戦略確認追加）

**推定時間**: 2-3時間

**前提条件**: なし（即座実施可能）

**成功基準**: ADR_022作成完了・CLAUDE.md更新完了

---

## 3. Phase C開始前推奨（中優先度）

### Issue #67: Phase C準備: ドメインエキスパートSubAgent導入（ビジネス観点検証体制確立）

**GitHub**: https://github.com/d-kishi/ubiquitous-lang-mng/issues/67

**現在状況**: 未着手、Phase B-F2完了後即実施推奨

**Phase B3への影響**: **低**

**推奨タイミング**: **Phase B-F2完了後即座（Phase B3前または並行）**

**理由**:
1. Phase C-D（ドメイン管理・ユビキタス言語）でビジネス観点検証必須
2. 現在14 SubAgentすべて技術観点、ビジネス観点なし
3. Phase B3実装中に準備完了が理想

**実装内容**:
1. SubAgent定義作成（`.claude/subagents/domain-expert.json`）
2. Agent Skill作成（`domain-expert-patterns/` ディレクトリ・5ファイル）
3. 既存Skills・Commands更新（subagent-patterns, step-start等）
4. 既存Commands更新（開発プロセス統合）
5. ADR_027作成・ドキュメント整備

**推定時間**: 3.5-4時間

**前提条件**: なし（即座実施可能）

**成功基準**: domain-expert SubAgent定義完了・Phase C開始準備完了

---

### Issue #55: Claude Agent SDK導入（Phase 2実装）

**GitHub**: https://github.com/d-kishi/ubiquitous-lang-mng/issues/55

**現在状況**: Phase 1技術検証完了（2025-11-18）、Phase 2実装Go判断

**Phase B3への影響**: **低**

**推奨タイミング**: **Phase C期間中並行実施**

**理由**:
1. Phase 1完了：TypeScript SDK学習・Hooks実装成功・実現可能性確認済み
2. Phase 2推定：25-35時間（Phase C期間中並行）
3. ADR_016プロセス遵守違反率0%達成（構造的強制）

**実装内容**:
- Phase 2実装（ADR_016違反検出・SubAgent成果物実体確認・並列実行信頼性向上）
- 3機能すべてFEASIBLE確認済み

**推定時間**: 25-35時間（Phase C期間中並行）

**前提条件**: Phase 1完了（2025-11-18）

**成功基準**: Issue #55 Phase 2完了・ADR_016違反率0%達成

---

### Issue #54: Agent Skills導入提案（Phase 3実装）

**GitHub**: https://github.com/d-kishi/ubiquitous-lang-mng/issues/54

**現在状況**: Phase 1-2完了（8個Skills確立）、Phase 3未着手

**Phase B3への影響**: **低**

**推奨タイミング**: **Phase C開始前または並行**

**理由**:
1. Phase 1-2完了：8個Skills確立（fsharp-csharp-bridge, clean-architecture-guardian等）
2. Phase 3：Claude Code Plugin配布準備（Issue #47連携）
3. Phase C以降の開発効率向上に貢献

**実装内容**:
- Plugin化準備（汎用化・パッケージング）
- GitHub公開・Claude Code Marketplace申請検討

**推定時間**: 1-2時間

**前提条件**: Phase 1-2完了（2025-11-18）

**成功基準**: Issue #54 Phase 3完了

---

### Issue #19: テスト戦略改善・再発防止体制確立

**GitHub**: https://github.com/d-kishi/ubiquitous-lang-mng/issues/19

**現在状況**: 未着手（Phase A8 Step4で35件テスト失敗経験）

**Phase B3への影響**: **中**

**推奨タイミング**: **Phase B3開始前**

**理由**:
1. Phase A8でテスト戦略適用タイミング不適切による大量失敗
2. step-start Command・CLAUDE.md・spec-compliance Agent強化が必要
3. Phase B3以降のテスト失敗防止

**実装内容**:
- テスト戦略適用タイミング明確化
- step-start Command更新（テスト戦略確認追加）
- CLAUDE.md更新（テスト戦略ルール明記）
- spec-compliance Agent定義強化

**推定時間**: 3-4時間

**前提条件**: なし（即座実施可能）

**成功基準**: テスト戦略改善完了・Phase B3以降テスト失敗率0%

---

### Issue #44: Web層ディレクトリ構造統一

**GitHub**: https://github.com/d-kishi/ubiquitous-lang-mng/issues/44

**現在状況**: 未着手、Phase B完了後実施予定

**Phase B3への影響**: **低**

**推奨タイミング**: **Phase B5完了後（Phase C開始前）**

**理由**:
1. `Pages/Admin/` vs `Components/Pages/ProjectManagement/` 混在
2. Phase B3-B5で新規ページ追加時に影響
3. Phase B完了後の統一が効率的

**実装内容**:
- ディレクトリ移動（`Pages/Admin/` → `Components/Pages/Admin/`）
- 参照更新（全Razorファイル・C#ファイル）
- ビルド確認・テスト確認

**推定時間**: 1-2時間

**前提条件**: Phase B5完了

**成功基準**: ディレクトリ構造統一完了・0 Warning/0 Error維持

---

## 4. 懸案事項詳細分析

### Issue #65: .NET 10への移行

**GitHub**: https://github.com/d-kishi/ubiquitous-lang-mng/issues/65

#### 内容詳細

**現在状況**:
- **現在**: .NET 8.0（src/）、.NET 8.0/9.0混在（tests/）
- **利用可能SDK**: .NET 10.0.100
- **メリット**: パフォーマンス向上・セキュリティ更新・C# 13/F# 9機能・LTS対応（該当する場合）
- **リスク**: 破壊的変更・NuGetパッケージ互換性・EF Core/Identity対応・F#コンパイラ対応・Blazor Server変更

**移行作業内容**（5 Phase構成）:
1. Phase 1: 調査・準備（破壊的変更調査・パッケージ互換性確認）
2. Phase 2: テスト環境移行（Dockerイメージ更新・global.json作成・テストプロジェクト更新）
3. Phase 3: 本体プロジェクト移行（src/配下更新・NuGetパッケージ更新）
4. Phase 4: CI/CD・デプロイ環境更新（GitHub Actions・DevContainer・本番環境）
5. Phase 5: ドキュメント更新（README.md・CLAUDE.md・環境構築ドキュメント・ADR作成）

#### 実施タイミング推奨: **Phase B3-B5期間中またはPhase C開始前**

#### 理由

1. **Phase B3影響最小化**: Phase B3は既存Phase B機能完成（新規技術依存少ない）
2. **Phase C準備**: Phase C-Dはドメイン管理・ユビキタス言語（新規機能大量追加）
3. **技術基盤安定化**: Phase C開始前に技術基盤を最新化・Phase C-D期間の技術負債回避
4. **5段階移行計画**: 調査→テスト移行→本体移行→CI/CD更新→ドキュメント（体系的）

#### 推定時間: 8-12時間（5 Phase構成）

#### Phase B-F3対象判断: **条件付きYes**
- Phase B3開始前実施なら含める
- Phase C前実施なら別Phase

#### リスク評価

**技術的リスク**:
- 破壊的変更によるコンパイルエラー・実行時エラー
- NuGetパッケージ互換性問題（EF Core・Identity・Blazor Server）
- F#コンパイラ対応遅延（F# 9リリースタイミング）

**緩和策**:
- Phase 1で破壊的変更を事前調査
- global.jsonでSDKバージョン固定
- 段階的移行（テスト→本体）によるリスク分散

---

### Issue #61: Cursor Ver2への移行検討・検証（AI開発IDE最新技術導入）

**GitHub**: https://github.com/d-kishi/ubiquitous-lang-mng/issues/61

#### 内容詳細

**Cursor 2.0概要**:
- **リリース日**: 2025-10-29
- **主要機能**:
  - Composer（独自AIモデル・4倍高速・30秒以内タスク完了）
  - Multi-Agent（最大8並列実行・隔離環境・モデル選択）
  - 複数モデル対応（GPT-4o/Claude Sonnet 4/Gemini 2.5 Pro/Composer）
  - MCP対応（Serena MCPサーバー継続利用可能）

**期待効果**:
- **効率50-70%改善**（現40-50%からさらに向上）
- **SubAgent作業30-40%削減**
- **品質10-15%向上**（タスク別最適モデル選択）

**懸案点**:
- F#サポート不明（.NET全般サポートは確認済み）
- 既存ワークフロー再現性（SlashCommands・SubAgent専門化・運用マニュアル）
- 学習コスト（2025-10-29リリース直後・情報少ない）

**検証計画**（Phase 1-4構成）:
1. Phase 1: F#サポート品質検証（必須・1-2時間）
2. Phase 2: 小規模タスク実施（2-3時間）
3. Phase 3: 既存ワークフロー再現性検証（2-3時間）
4. Phase 4: 本格移行判断（1時間）

#### 現在状況

- Phase 1-4検証計画策定済み（Phase B-F2完了後実施予定）
- 未着手（検証未開始）

#### 実施タイミング推奨: **Phase C期間中検証（Phase B3開始影響なし）**

#### 理由

1. **Phase B-F2完了**: 2025-11-18完了、Phase B3開始前の検証タイミング適切
2. **検証期間**: 9-13時間（Phase 1-4）、Phase C期間中並行実施可能
3. **リスク管理**: 検証失敗時も現環境（Claude Code）継続可能
4. **Phase B3影響**: なし（検証のみ、移行はGo判断後）

#### 推定時間: 9-13時間
- Phase 1: 1-2時間
- Phase 2: 2-3時間
- Phase 3: 2-3時間
- Phase 4: 1時間
- Phase 3-Extra（新追加・詳細検証）: 3-4時間

#### Phase B-F3対象判断: **No**（Phase C期間中検証推奨）

#### 判断基準（Phase 4）

**Go判断基準**:
- F#サポート品質: Claude Code同等以上
- 作業効率: 30%以上向上
- 既存ワークフロー再現率: 80%以上
- 学習コスト: 許容範囲内（1-2日）

**結果**:
- 全基準クリア → 本格移行Go
- 1つでも未達 → 継続検証または移行見送り

---

## 5. 後回し可能（低優先度）

### プロセス改善・長期テーマ（Phase D以降推奨）

#### Issue #69: メンテナンス対象改善機会検知の仕組み構築
- **推定**: Phase 1-3計画・3-4週間
- **推奨**: Phase D以降（長期改善テーマ）

#### Issue #51: Claude Code on the Web開発プロセス自動化
- **Phase 1検証**: 完了（No-Go判断・Fire-and-forget失敗）
- **代替案**: Claude Code for GitHub Actions検証（2-3h）
- **推奨**: Phase C以降

#### Issue #47: Claude Code Plugin Package作成
- **連携**: Issue #54 Phase3
- **推定**: 7-11時間（Commands/Agents/Skills汎用化）
- **推奨**: Phase B完了後（Phase C前または並行）

#### Issue #40: テストアーキテクチャ全面見直し
- **Phase 1-3**: 完了（2025-10-13）
- **Phase 4**: E2Eテスト基盤追加（Phase B2で完了）
- **状態**: 完了扱い（Close候補）

#### Issue #39: 仕様駆動開発強化計画 Phase 2-3実装
- **Phase 1**: 完了
- **Phase 2-3**: spec-analysis Agent拡張・Living Documentation（長期改善）
- **推奨**: Phase D以降

#### Issue #33: メトリクス収集・分析機能
- **親Issue**: #12（スクラム開発完全実現）
- **推定**: Phase C1以降（長期改善フェーズ）

#### Issue #32: 自動レポート生成機能
- **親Issue**: #12（スクラム開発完全実現）
- **推定**: Phase B2以降（中期改善フェーズ）

#### Issue #31: プロセス遵守メカニズムの強化
- **親Issue**: #12（スクラム開発完全実現）
- **備考**: Issue #55（Agent SDK Phase 2）で構造的実現予定
- **推奨**: Issue #55完了後にClose検討

#### Issue #30: データベーステーブルコメント論理名改善
- **優先度**: Low
- **推定**: 60-90分
- **推奨**: Phase B以降

#### Issue #28: ASP.NET Core Identity設計見直し
- **優先度**: Medium
- **推定**: 120-180分
- **推奨**: Phase B3前に再検討（Claim機能必要性の確認）

#### Issue #26: Phase A2/A3 未実装スタブメソッド（27メソッド）
- **優先度**: Medium
- **推定**: 180-300分
- **推奨**: Phase C以降に延期（Phase B3影響なし）

#### Issue #25: スクラム開発プロセスのClaudeCode実装
- **親Issue**: #12（スクラム開発完全実現）
- **推定**: Phase 1-4段階展開
- **推奨**: Phase B1以降（実験・検証）

#### Issue #23: デバッグログ整理・クリーンアップ
- **タイミング**: Phase D完了後（第一次製造完了時）
- **推定**: Medium優先度

#### Issue #22: 週次総括による重要情報記録プロセス確立
- **Phase 1**: W36実験完了
- **Phase 2-3**: 継続中（W37-38以降）
- **推奨**: Phase D完了まで継続

#### Issue #12: スクラム開発完全実現に向けた開発プロセス改善
- **Phase 1-3**: 全12タスク未着手
- **推定**: 3-4 Sprints
- **推奨**: Phase B2以降段階展開

#### Issue #10: SubAgent並列実行・専門性活用の改善
- **現在**: Claude Code Ver.2で再発なし（監視中）
- **推奨**: 再発なければClose検討

#### Issue #8: Gemini MCP統合の実現
- **優先度**: Low
- **状態**: 2025-08-19試行失敗・中断
- **推奨**: 将来検証

---

### 技術負債・品質改善（Phase B4-B5/Phase C以降推奨）

#### Issue #70: Claude コミュニケーション品質改善（継続的改善ルートチケット）
- **Issue #1**: 完了（CLAUDE.md更新・2025-11-18）
- **Phase 2以降**: 効果測定・継続改善

#### Issue #68: C# Dev Kit v1.80.2/v1.81.7バグ - プロジェクト重複読み込みエラー
- **暫定対応**: 完了（v1.70.3へダウングレード）
- **監視**: Microsoft Issue #2492週次監視中
- **状態**: 暫定解決済み（Close候補）

#### Issue #66: Claude Code実行環境の自動判定とコマンド形式自動切り替え実装
- **推定**: 1.5-2.5時間
- **推奨**: Phase C以降（Windows Sandbox対応準備）

#### Issue #63: Windows環境でのClaude Code Sandboxモード非対応対応
- **状態**: 暫定対応完了（docker exec形式）
- **監視**: Windows Sandbox対応待ち（四半期チェック）
- **推奨**: Windows対応後に対処

#### Issue #62: テストコードのNullable参照型警告78件解消
- **状態**: 解決済み（.gitattributes追加・78→0件）
- **監視**: 再発なければClose推奨

#### Issue #56: bUnit統合テスト技術課題（Phase B2 Step5）
- **問題**: EditForm OnValidSubmit未トリガー・子コンポーネント依存複雑化
- **現在**: Playwright E2Eテスト代替中
- **推奨**: Phase B以降（bUnitパターン確立）

---

## 6. Phase B-F3新設の必要性判断

### 結論: **不要**

### 理由

#### Phase B3前必須タスク分析

**真の必須タスク**（Phase B3開始を阻害）:
- Issue #59: UI実装 + E2Eテスト再実行（2-3時間）
- Issue #57: e2e-test Agent運用検証（5分-1時間）

**合計**: 3-5時間

#### Phase B3前推奨タスク分析

**組織基盤整備**（Phase B3品質向上）:
- Issue #46: Commands更新延期判断（1-1.5時間）
- Issue #53: ADR_022作成・プロセス確立（2-3時間）
- Issue #19: テスト戦略改善（3-4時間）

**合計**: 6-8.5時間

#### Phase B-F3新設判断基準

**新設が必要な場合**:
- 推定合計時間: 20時間以上
- 技術基盤大型変更（ADR複数作成）
- Phase本体実装開始の明確なブロッカー

**現状**:
- 推定合計時間: 9-13.5時間（必須3-5h + 推奨6-8.5h）
- 技術基盤変更: なし（組織整備のみ）
- ブロッカー: 真の必須は2件のみ（3-5h）

**判断**: Phase B3開始直後に組み込み可能（Phase B3 Step1-2で実施）

---

## 7. 全体サマリ

### Issue分類統計

- **合計Open Issues**: 35件
- **Phase B3前必須**: 2件（3-5時間）
- **Phase B3前推奨**: 3件（6-8.5時間）
- **Phase C前推奨**: 5件（40-50時間）
- **懸案事項**: 2件（#65: 8-12h Phase B3-B5期間中またはPhase C前、#61: 9-13h Phase C期間中）
- **後回し可**: 23件（Phase D以降/監視中/低優先度）

### Phase B3開始可能時期

**即座開始可能**（2025-11-19以降）

**理由**:
1. Phase B-F2完了済み（2025-11-18）
2. 真の必須タスク（#59, #57）はPhase B3 Step1-2で実施可能（3-5時間）
3. 推奨タスク（#46, #53, #19）もPhase B3並行実施可能（6-8.5時間）

### Phase B3実装計画推奨構成

**Phase B3 Step1-2**: 技術負債・組織基盤整備（10-14時間）
- Issue #59: UserProjects UI実装 + E2Eテスト（2-3h）
- Issue #57: e2e-test Agent運用検証（5分-1h）
- Issue #46: Commands更新延期判断（1-1.5h）
- Issue #53: ADR_022作成（2-3h）
- Issue #19: テスト戦略改善（3-4h）

**Phase B3 Step3以降**: プロジェクト機能完成実装
- 統計情報表示
- デフォルトドメイン自動作成
- 削除時影響分析
- その他Phase B3機能

### 懸案事項最終推奨

**Issue #65（.NET 10移行）**:
- **推奨タイミング**: Phase B3-B5期間中またはPhase C開始前
- **理由**: Phase C新機能大量追加前に技術基盤最新化
- **推定時間**: 8-12時間

**Issue #61（Cursor Ver2移行）**:
- **推奨タイミング**: Phase C期間中検証（Phase B3開始影響なし）
- **理由**: 9-13時間検証・Phase C並行実施可能・リスク管理
- **推定時間**: 9-13時間（検証のみ）

---

## 8. 推奨アクションプラン

### 最優先（Phase B3開始前・即座実施）

1. **Issue #59 + #57**: UserProjects UI実装・E2Eテスト検証（3-5時間）
2. **Issue #53**: テスト失敗時判断プロセス確立（ADR_022）（2-3時間）
3. **Issue #19**: テスト戦略改善・再発防止体制（3-4時間）

**合計**: 8-12時間 → **Phase B3 Step1-2で実施**

### Phase B3並行実施推奨

4. **Issue #52**: Phase A UserManagement UI実装（8-12時間・Phase B3期間中）
5. **Issue #67**: ドメインエキスパートSubAgent導入（3.5-4時間・Phase B3期間中）

### Phase B3-B5期間中またはPhase C前実施

6. **Issue #65**: .NET 10移行（8-12時間・Phase C前推奨）
7. **Issue #44**: Web層ディレクトリ構造統一（1-2時間・Phase B5完了後）

### Phase C期間中実施

8. **Issue #61**: Cursor Ver2検証（9-13時間・Phase C並行）
9. **Issue #55**: Agent SDK Phase 2実装（25-35時間・Phase C並行）
10. **Issue #54**: Agent Skills Phase 3実装（1-2時間・Phase C並行）

### Phase D以降実施

- その他23件（プロセス改善・長期テーマ・技術負債・監視中Issue）

---

**分析完了日**: 2025-11-19
**次回更新推奨**: Phase B3完了後（Phase B3で新規Issue・状況変化確認）
