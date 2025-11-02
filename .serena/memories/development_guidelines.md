# 開発ガイドライン

## 🤖 Agent Skills活用方法（2025-11-01更新・Phase 2展開完了）

### Agent Skillsとは

**定義**: プロジェクト固有の知見・パターン・判断基準をモジュール化し、Claudeが自律的に適用する仕組み

**配置場所**: `.claude/skills/`

**使用方法**: Claudeが状況に応じて自律的に判断・使用（ユーザーの明示的呼び出し不要）

### Phase 1 Skills（2個・2025-10-21導入）

#### 1. fsharp-csharp-bridge

**目的**: F#↔C#型変換パターンの自律的適用

**使用タイミング**:
- F#↔C#境界コード実装時
- 型変換エラー発生時
- contracts-bridge Agent作業時

**提供パターン**: Result型・Option型・Discriminated Union・Record型の4パターン

**詳細**: `.claude/skills/fsharp-csharp-bridge/SKILL.md`

#### 2. clean-architecture-guardian

**目的**: Clean Architecture準拠性の自動チェック

**使用タイミング**:
- 新規実装時
- リファクタリング時
- Step/Phase完了時

**チェック項目**: レイヤー分離・namespace階層・BC境界・F# Compilation Order

**詳細**: `.claude/skills/clean-architecture-guardian/SKILL.md`

### Phase 2 Skills（5個・2025-11-01導入）

#### 3. tdd-red-green-refactor

**目的**: TDD Red-Green-Refactorサイクル実践ガイド

**使用タイミング**:
- 新規機能実装開始時
- unit-test/integration-test Agent作業時
- TDD実践時の各フェーズ切り替え時

**提供パターン**: Red Phase・Green Phase・Refactor Phaseの3パターン

**詳細**: `.claude/skills/tdd-red-green-refactor/SKILL.md`

#### 4. spec-compliance-auto

**目的**: 仕様準拠確認の自律的適用

**使用タイミング**:
- spec-compliance-check Command実行時
- Step/Phase完了時の品質確認時
- 新規機能実装完了時

**提供ルール**: 原典仕様書参照・仕様準拠マトリックス・スコアリング・重複実装検出の4ルール

**詳細**: `.claude/skills/spec-compliance-auto/SKILL.md`

#### 5. adr-knowledge-base

**目的**: ADR知見抜粋による技術決定理由提供

**使用タイミング**:
- 設計判断時
- プロセス遵守確認時
- SubAgent選択・Fix-Mode活用時
- テストアーキテクチャ設計時

**提供ADR抜粋**: ADR_016（プロセス遵守）・ADR_018（Fix-Mode）・ADR_020（テストアーキテクチャ）・ADR_023（DB初期化方針）

**詳細**: `.claude/skills/adr-knowledge-base/SKILL.md`

#### 6. subagent-patterns

**目的**: SubAgent選択・組み合わせパターンの自律的適用

**使用タイミング**:
- step-start Command実行時
- SubAgent選択時
- 並列実行判断時
- 責務境界判定時

**提供パターン**: 13種類のAgent定義・選択原則・Phase特性別組み合わせパターン・責務境界判定・並列実行判断

**詳細**: `.claude/skills/subagent-patterns/SKILL.md`

#### 7. test-architecture

**目的**: テストアーキテクチャ自律適用（ADR_020準拠）

**使用タイミング**:
- 新規テストプロジェクト作成時
- unit-test/integration-test Agent選択時
- テストアーキテクチャ設計時

**提供ルール**: 新規テストプロジェクト作成チェックリスト・命名規則・参照関係原則の3ルール

**詳細**: `.claude/skills/test-architecture/SKILL.md`

### Phase 1 Skills（Phase B2 Step7導入）

#### 8. db-schema-management（2025-10-27新設・Phase B2 Step7）

**目的**: EF Core Migrationsによるスキーマ変更ガイドライン・パターン提供

**使用タイミング**:
- スキーマ変更時（テーブル・列追加、制約追加等）
- CHECK制約・COMMENT文追加時
- データベース設計書更新時
- Phase B3以降のスキーマ変更全般

**提供パターン**: 5種類
- ef-migrations-workflow.md: 5段階スキーマ変更ワークフロー
- check-constraint-pattern.md: CHECK制約実装パターン
- manual-sql-pattern.md: GIN/BRIN/COMMENT手動SQLパターン
- db-doc-sync-checklist.md: DB設計書同期チェックリスト

**詳細**: `.claude/skills/db-schema-management/SKILL.md`

**Phase B2 Step7成果**: GitHub Issue #58完全解決・ADR_023作成・EF Migrations主体方式確立

### 効果測定

**測定ドキュメント**: `Doc/08_Organization/Active/AgentSkills_Phase1_効果測定.md`

**Phase 1測定期間**: Phase B2 Step5 ～ Phase B3完了（推定2-3週間）

**Phase 2測定期間**: Phase B-F2 Step3以降 ～ Phase B-F2完了（推定1-2週間）

**期待効果**: 作業効率30-40分/セッション削減、品質向上（ADR遵守率90%→98%、SubAgent選択精度85%→95%）

---

## 🎊 Phase B2完了成果（2025-10-27）

### 技術負債管理ベストプラクティス確立

**3回修正試行ルール**:
- 3回の修正試行で解決しない場合、根本的な設計問題と判断
- 戦略的延期判断を実施（効率性より技術負債解決を優先）
- GitHub Issueに技術負債として明確に記録

**Phase B2 Step8適用例**:
- E2Eテスト実装で3回修正試行→すべて失敗
- 根本原因分析完了（E2Eテストシナリオと画面遷移フロー不一致）
- 戦略的延期判断（GitHub Issue #59記録）

**学んだ教訓**:
- E2Eテスト設計時、実際のアプリケーション動作を事前確認すべき
- 技術負債の依存関係を事前に整理すべき（Issue #57 → #53 → #59）
- 戦略的延期判断時、前提条件・再実装計画を明確に記録すべき

---

## 📋 ADR vs Agent Skills 判断基準（2025-10-26新設・30秒チェック）

### 判断フロー（4つの質問）

1. **歴史的記録が必要か？**（なぜこの決定をしたか）
   → YES: **ADR作成**

2. **Claudeが自律的に適用すべきか？**（実装時に自動適用）
   → YES: **Agent Skills作成**

3. **技術選定の根拠か？**（代替案との比較・リスク評価）
   → YES: **ADR作成**

4. **実装パターン・チェックリストか？**（繰り返し使うパターン）
   → YES: **Agent Skills作成**

### 簡潔な定義

**ADR（Architectural Decision Record）**:
- 目的: 「**なぜ**その技術決定をしたか」を記録
- 性質: 歴史的文書・技術選定の根拠
- 参照: 技術選定の振り返り・新メンバーのオンボーディング・将来の技術変更時

**Agent Skills**:
- 目的: 「**どう**実装すべきか」をガイド
- 性質: 実行可能な知見・自律的適用
- 参照: Claudeが自律的に判断して使用（明示的呼び出し不要）

### 詳細ガイドライン

**迷った時の参照**: `Doc/08_Organization/Rules/ADRとAgent_Skills判断ガイドライン.md`

**提供内容**:
- 判断フローチャート
- 判断基準マトリックス（5W1H）
- ADR適用例・Skills適用例
- 移行事例（ADR→Skills）
- 迷った時のチェックリスト

**重要**: 技術的知見が発生した際は、必ず上記ガイドラインを参照して適切な記録方法を選択すること

---

## プロセス遵守絶対原則（ADR_016）

### 絶対遵守原則
- **コマンド = 契約**: 一字一句を法的契約として遵守・例外なし
- **承認 = 必須**: 「ユーザー承認」表記は例外なく取得・勝手な判断禁止
- **手順 = 聖域**: 定められた順序の変更禁止・先回り作業禁止

### 禁止行為（重大違反）
- ❌ **承認前の作業開始**: いかなる理由でも禁止
- ❌ **独断での判断**: 「効率化」を理由とした勝手な作業
- ❌ **成果物の虚偽報告**: 実体のない成果物の報告
- ❌ **コマンド手順の無視**: phase-start/step-start等の手順飛ばし

### 必須実行事項
- ✅ **実体確認**: SubAgent成果物の物理的存在確認
- ✅ **承認記録**: 取得した承認の明示的記録  
- ✅ **チェックリスト実行**: 組織管理運用マニュアルのプロセス遵守チェック

**詳細**: `/Doc/07_Decisions/ADR_016_プロセス遵守違反防止策.md`

---

## 🧪 E2Eテスト実装タイミング原則（2025-10-17確立・Phase B2 Step2で発見）

### 原則: E2Eテストは機能完成後に実装

**背景**: Phase B2 Step2でE2Eテスト先行実装により以下の問題発生
- Playwright実装に2時間要したが機能未完成のため実行不可
- 仕様変更時のテストメンテナンス工数発生
- 作業効率30-40%低下

### 推奨フロー

```
1. 機能実装完了（Unit/Integration Test完了）
   ↓
2. 手動E2E確認（Playwright MCP使用・5-10分）
   ↓
3. E2E自動テスト実装（安定した仕様に対して）
```

### 例外: E2Eテスト先行実装が許容されるケース
- ユーザー明示的指示
- 仕様確定済み・変更リスク極小
- E2E駆動開発（BDD/ATDD）採用時

**詳細**: `Doc/08_Organization/Active/E2Eテスト実装タイミング原則.md`

---

## 🔧 namespace設計原則（ADR_019準拠・2025-10-01確立）

**重要**: ADR_019は`.claude/skills/clean-architecture-guardian/rules/namespace-design.md`に移行

### 必須遵守事項
**基本テンプレート**: `<ProjectName>.<Layer>.<BoundedContext>[.<Feature>]`

#### 具体的namespace規約
- **Domain層**: `UbiquitousLanguageManager.Domain.<BoundedContext>`
- **Application層**: `UbiquitousLanguageManager.Application.<BoundedContext>`
- **Infrastructure層**: `UbiquitousLanguageManager.Infrastructure.<Feature>`
- **Contracts層**: `UbiquitousLanguageManager.Contracts.<Feature>`
- **Web層**: `UbiquitousLanguageManager.Web.<Feature>`

**詳細**: `.claude/skills/clean-architecture-guardian/rules/namespace-design.md`（ADR_019から移行）

---

## 🎯 重要: Blazor Server・F#初学者対応

プロジェクトオーナーが初学者のため、**詳細なコメント必須**（ADR_010参照）

### 必須コメント対象
- **Blazor Server**: ライフサイクル・StateHasChanged・SignalR接続の説明
- **F#**: パターンマッチング・Option型・Result型の概念説明
- **型変換**: F#↔C#境界での変換ロジック説明

### コメントテンプレート例

```csharp
// Blazor Server: StateHasChanged()を呼び出してUIを更新
// StateHasChangedはBlazor Serverのライフサイクルメソッドで、
// コンポーネントの状態が変更されたことをBlazorランタイムに通知する。
// これにより、SignalR経由でクライアントに変更が通知され、UIが再レンダリングされる。
StateHasChanged();
```

```fsharp
// F# Pattern Matching: Result型の成功/失敗を分岐処理
// match式はF#の強力な構文で、Result<T, E>型の成功(Ok)と失敗(Error)を
// 網羅的にチェックし、それぞれの場合の処理を記述できる。
match result with
| Ok value -> // 成功時の処理
| Error err -> // 失敗時の処理
```

---

## 🔴 メインエージェント必須遵守事項（ADR_016・ADR_018準拠）

### エラー修正時の責務分担原則（タイミング問わず適用）

#### MainAgent責務定義

✅ **実行可能な作業**:
- 全体調整・オーケストレーション
- SubAgentへの作業委託・指示
- 品質確認・統合テスト実行
- プロセス管理・進捗管理
- ドキュメント統合・レポート作成

❌ **禁止事項（例外を除く）**:
- 実装コードの直接修正
- ビジネスロジックの追加・変更
- 型変換ロジックの実装
- テストコードの作成・修正
- データベーススキーマの変更

#### エラー発生時の必須対応原則

1. **エラー内容で責務判定**（発生場所・タイミング問わず）
2. **責務マッピングでSubAgent選定**：
   - F# Domain/Application層 → fsharp-domain/fsharp-application
   - F#↔C#境界・型変換 → contracts-bridge
   - C# Infrastructure/Web層 → csharp-infrastructure/csharp-web-ui
   - テストエラー → unit-test/integration-test
3. **Fix-Mode活用**：`"[SubAgent名] Agent, Fix-Mode: [修正内容]"`
4. **効率性より責務遵守を優先**

#### 例外（直接修正可能）

- 単純なtypo（1-2文字）
- import文の追加のみ
- コメントの追加・修正
- 空白・インデントの調整

**詳細**: `Doc/07_Decisions/ADR_018_SubAgent指示改善とFix-Mode活用.md`

---

## 🧪 新規テストプロジェクト作成時の必須確認事項

**確認タイミング**（以下のいずれか）:
1. **unit-test/integration-test Agent選択時**（step-start Command実行時）
2. **新規テストプロジェクト作成指示を受けた時**（MainAgent/SubAgent問わず）
3. **tests/配下に新規ディレクトリ・プロジェクトファイル作成前**

**新規テストプロジェクト作成前に以下を必ず確認すること**（GitHub Issue #40再発防止策）：

1. **ADR_020**: テストアーキテクチャ決定
   - `/Doc/07_Decisions/ADR_020_テストアーキテクチャ決定.md`
   - レイヤー×テストタイプ分離方式の理解

2. **テストアーキテクチャ設計書**: `/Doc/02_Design/テストアーキテクチャ設計書.md`
   - プロジェクト構成図・命名規則・参照関係原則の確認

3. **新規プロジェクト作成チェックリスト**: `/Doc/08_Organization/Rules/新規テストプロジェクト作成ガイドライン.md`
   - 事前確認・プロジェクト作成・参照関係設定・ビルド確認・ドキュメント更新の全手順実施

**命名規則（厳守）**: `UbiquitousLanguageManager.{Layer}.{TestType}.Tests`
- **Layer**: Domain / Application / Contracts / Infrastructure / Web
- **TestType**: Unit / Integration / UI / E2E

**参照関係原則**:
- **Unit Tests**: テスト対象レイヤーのみ参照
- **Integration Tests**: 必要な依存層のみ参照
- **E2E Tests**: 全層参照可

---

## 🔄 Step再実行プロセス（設計判断誤り発見時）

**確立日**: 2025-11-02（Phase B-F2 Step3実施時）

### トリガー

- **ユーザー指摘**: Step実行中の設計判断・方針決定に誤りがあることを指摘された時
- **自己発見**: Step実行中に当初の設計判断が不適切であることを発見した時

### Step再実行プロセス

1. **現状確認**:
   - 現在のStage位置確認（どのStageまで完了したか）
   - 誤った判断がどのStageで行われたか特定
   - 既に作成された成果物の確認

2. **戻り先Stage決定**:
   - **原則**: 誤った判断が行われたStageの直前に戻る
   - **例**: Stage2で判断誤り発見 → Stage1完了直後に戻る

3. **作業のロールバック**:
   ```bash
   # git restore で変更を破棄
   git restore [誤ったStageで変更したファイル群]
   ```
   - 誤ったStage以降で作成・更新したファイルを全て元に戻す
   - 新規作成ファイルは削除、既存ファイルは元のバージョンに戻す

4. **Step組織設計書の更新**:
   - 戻り先Stageの実行記録を「完了」状態に保持
   - 誤ったStageの実行記録を削除 or 「やり直し」として記録
   - 新しい正しい判断内容を記録（「結論ファースト」アプローチ）

5. **再実行**:
   - 正しい判断に基づいて、戻り先Stageの次Stageから再実行
   - Step組織設計書に再実行の経緯・判断変更の理由を明記

### Phase B-F2 Step3実施例

**発生状況**:
- **Stage2判断**: integration-test Agent拡張（E2E責務追加）
- **ユーザー指摘**: Integration TestとE2E Testは異なるレイヤー、専用SubAgent新設すべき

**実施プロセス**:
1. Stage1完了直後に戻る（Stage2-7の作業を破棄）
2. git restore でStage2以降の変更を全て元に戻す
3. Step組織設計書のStage2セクションを書き直し
   - **新判断**: E2E専用SubAgent新設（推奨度 10/10点）
   - **判断根拠**: 5点（ADR_020整合性・レイヤー分離・技術スタック・Skill参照・MCP連携）
4. Stage2から再実行（正しい判断に基づく実装）

### 効果

- ✅ **設計判断の正確性向上**: 誤った判断を早期に修正、正しい方向性で実装
- ✅ **プロセス透明性確保**: やり直しの経緯・理由を明確に記録、学習機会
- ✅ **品質向上**: 誤った設計判断による技術負債の作り込みを防止

### 注意事項

- **速やかな対応**: 誤り発見後、速やかにロールバック・再実行を実施
- **記録の明確化**: なぜ判断を変更したか、Step組織設計書に明記
- **学習機会**: 誤った判断の原因分析、次回以降の改善に活用

**詳細実施例**: `Doc/08_Organization/Active/Phase_B-F2/Step03_Playwright統合基盤刷新.md` Stage2実行記録

---

**最終更新**: 2025-11-02（**Step再実行プロセス確立・Phase B-F2 Step3実施時に確立**）
