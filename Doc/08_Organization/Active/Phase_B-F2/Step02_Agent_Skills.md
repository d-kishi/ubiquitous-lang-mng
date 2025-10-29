# Step 02 組織設計・実行記録

**Phase**: Phase B-F2（技術負債解決・E2Eテスト基盤強化・技術基盤刷新）
**Step名**: Step 2 - Agent Skills Phase 2展開 + CLAUDE.mdルール強化
**作業特性**: Agent Skills拡充・プロセス遵守ルール強化・ADR/Rules知見体系化

---

## 📋 Step概要

### 基本情報

- **Step名**: Step 2 - Agent Skills Phase 2展開 + CLAUDE.mdルール強化
- **作業特性**: Agent Skills拡充（5-7個）・CLAUDE.mdプロセス遵守ルール強化
- **推定期間**: 7-11時間（Agent Skills 2-3時間 + CLAUDE.mdルール強化 5-8時間）
- **開始予定日**: Phase B-F2 Step1完了後
- **完了予定日**: 未定

### 対応Issue

- **GitHub Issue #54 Phase2**: Agent Skills Phase 2展開
- **Issue #55代替手段**: CLAUDE.mdプロセス遵守ルール強化（Agent SDK No-Go判断による代替手段）

### Step目的

1. **Agent Skills Phase 2展開**: 5-7個のSkill作成・ADR/Rules知見体系化・Claudeの自律適用確認
2. **CLAUDE.mdルール強化**: Agent SDK代替手段実施・ADR_016違反率0-2%目標・プロセス遵守チェック実行率100%目標

---

## 🏢 組織設計

### チーム構成

**MainAgent**: 全体統括・Skill作成・CLAUDE.md更新
- 役割: Agent Skills作成・CLAUDE.mdルール強化実施・統合調整
- 責務範囲: Phase全体統括・成果物統合・品質確認

**必要に応じてSubAgent活用**:
- **fsharp-domain / fsharp-application**: F#関連Skill作成時（該当する場合）
- **contracts-bridge**: F#↔C#境界Skill作成時（該当する場合）
- **code-review**: CLAUDE.mdルール内容レビュー時（該当する場合）

### 専門領域

**Agent Skills Phase 2展開**:
1. **tdd-red-green-refactor Skill作成**（TDD実践パターン）
2. **spec-compliance-auto Skill作成**（仕様準拠自動チェック）
3. **adr-knowledge-base Skill作成**（ADR知見体系化）
4. **database-design-patterns Skill作成**（データベース設計パターン）
5. **phase-step-management Skill作成**（Phase/Step管理パターン）
6. **補助ファイル充実**（ADR抜粋・パターン集・判定基準詳細）
7. **`.claude/skills/README.md`更新**

**CLAUDE.mdルール強化**（Agent SDK代替手段）:
1. **プロセス遵守ルール詳細化**（ADR_016絶対遵守原則強化）
2. **SubAgent選択チェックリスト**（並列実行判断・責務境界明確化）
3. **実体確認手順具体化**（SubAgent成果物実体確認必須手順）

### 実施内容詳細

#### Part 1: Agent Skills Phase 2展開（2-3時間）

**Skill作成（5-7個）**:

1. **tdd-red-green-refactor.md** (新規作成)
   - 目的: TDD Red-Green-Refactorサイクル実践パターン提供
   - 内容:
     - Red-Green-Refactorサイクル詳細手順
     - unit-test Agent活用パターン
     - テスタブルコード設計原則
     - テストカバレッジ管理方法
   - 補助ファイル: `patterns/tdd-cycle.md`, `patterns/testable-design.md`

2. **spec-compliance-auto.md** (新規作成)
   - 目的: 仕様準拠自動チェック機能の自律適用
   - 内容:
     - spec-compliance-check Command活用パターン
     - 仕様書（Doc/01_Requirements/）参照方法
     - 仕様準拠率95%維持手順
     - 仕様逸脱リスク特定方法
   - 補助ファイル: `patterns/spec-compliance-checklist.md`

3. **adr-knowledge-base.md** (新規作成)
   - 目的: ADR知見の体系的参照・適用
   - 内容:
     - 主要ADR抜粋（ADR_010, 013, 016, 019, 020, 021）
     - ADR検索・参照方法
     - 技術決定パターン集
     - ADR作成判断基準（or Agent Skills判断フロー）
   - 補助ファイル: `adr-excerpts/ADR_010_実装規約.md`, `adr-excerpts/ADR_016_プロセス遵守.md`

4. **database-design-patterns.md** (新規作成)
   - 目的: PostgreSQL + EF Core設計パターン提供
   - 内容:
     - PostgreSQL識別子クォート規約（全識別子クォート必須）
     - EF Core Migrations主体方式（ADR_023）
     - テーブル設計原則（正規化・制約・インデックス）
     - データベース設計書参照方法
   - 補助ファイル: `patterns/postgresql-ef-core.md`, `patterns/db-design-checklist.md`

5. **phase-step-management.md** (新規作成)
   - 目的: Phase/Step管理プロセスパターン提供
   - 内容:
     - phase-start / step-start Command活用
     - Step終了時レビュー（ADR_013準拠）
     - SubAgent成果物実体確認必須手順
     - Context管理・セッション継続判断
   - 補助ファイル: `patterns/phase-step-checklist.md`

**補助ファイル充実**:
- ADR抜粋（adr-excerpts/配下）: ADR_010, 016, 019, 020, 021
- パターン集（patterns/配下）: tdd-cycle, testable-design, spec-compliance-checklist, postgresql-ef-core, db-design-checklist, phase-step-checklist
- 判定基準詳細: ADRとAgent Skills判断フロー（30秒チェック）

**`.claude/skills/README.md`更新**:
- Phase 2追加Skill（5-7個）の概要・目的・活用シーン追加
- Phase 1 Skills（3個）との統合説明
- 合計8-10個のSkills一覧

**効果測定ドキュメント更新**:
- `Doc/08_Organization/Active/AgentSkills_Phase1_効果測定.md`
- Phase 2展開効果（5-7個Skill追加）記録
- Claudeの自律適用率測定

#### Part 2: CLAUDE.mdルール強化（5-8時間）

**1. プロセス遵守ルール詳細化**（2-3時間）:

```markdown
## 🔴 CRITICAL: プロセス遵守絶対原則（ADR_016）強化版

### 絶対遵守原則（詳細化）
- **コマンド = 契約**: 一字一句を法的契約として遵守・例外なし
  - phase-start/step-start/step-end-reviewの手順厳守
  - チェックリスト全項目実行（省略禁止）
  - 承認待ち時間を理由とした手順飛ばし禁止
- **承認 = 必須**: 「ユーザー承認」表記は例外なく取得・勝手な判断禁止
  - 承認なしの作業開始は重大違反
  - 「効率化」を理由とした独断禁止
  - 承認取得までの待機必須
- **手順 = 聖域**: 定められた順序の変更禁止・先回り作業禁止
  - Step1完了前のStep2着手禁止
  - SubAgent成果物実体確認必須
  - 成果物未確認での次Step着手禁止

### SubAgent選択時チェックリスト
1. ✅ 作業内容の責務判定（MainAgent/SubAgent境界明確化）
2. ✅ 並列実行可能性判断（依存関係確認）
3. ✅ SubAgent選択根拠記録（subagent-selection.md参照）
4. ✅ 成果物期待値明示（物理的存在確認項目定義）

### 実体確認手順（具体化）
1. ✅ ファイルパス確認（絶対パス明示）
2. ✅ ファイル存在確認（Readツール実行またはls -lh実行）
3. ✅ ファイルサイズ確認（空ファイル検出・目標サイズ範囲確認）
4. ✅ 内容品質確認（具体性・実用性・完全性）
   - 具体性: 抽象的記述なし・実装可能レベル
   - 実用性: Phase2以降で活用可能
   - 完全性: 必須項目全て記載

### 禁止行為（重大違反）詳細化
- ❌ **承認前の作業開始**: いかなる理由でも禁止
  - 「時間短縮のため」「効率化のため」等の理由での独断禁止
  - 承認取得まで待機必須
- ❌ **独断での判断**: 「効率化」を理由とした勝手な作業
  - ユーザー確認必須事項の独断判断禁止
  - 不明点は必ず質問・承認取得
- ❌ **成果物の虚偽報告**: 実体のない成果物の報告
  - 「作成しました」のみの報告禁止（ファイルパス明示必須）
  - SubAgent報告の無検証信頼禁止（必ず実体確認）
- ❌ **コマンド手順の無視**: phase-start/step-start等の手順飛ばし
  - 「前回やったから省略」等の理由での手順飛ばし禁止
  - 毎回必ず実行

### 必須実行事項詳細化
- ✅ **実体確認**: SubAgent成果物の物理的存在確認
  - Readツールまたはls -lhコマンド実行必須
  - ファイルサイズ確認（空ファイル検出）
  - 内容品質確認（具体性・実用性・完全性）
- ✅ **承認記録**: 取得した承認の明示的記録
  - 承認取得日時・承認内容の記録
  - 承認範囲の明確化
- ✅ **チェックリスト実行**: 組織管理運用マニュアルのプロセス遵守チェック
  - phase-start / step-start / step-end-review チェックリスト全項目実行
  - 実行記録を組織設計ファイルに記載
```

**2. SubAgent選択チェックリスト明記**（1.5-2時間）:

```markdown
## 🔴 SubAgent選択チェックリスト（ADR_016準拠）

### 並列実行判断ロジック

**並列実行可能条件**:
✅ 依存関係なし（各タスク独立実行可能）
✅ 共有リソース競合なし（同一ファイル編集なし）
✅ SubAgent役割明確（責務境界重複なし）
✅ 統合手順明確（完了後の統合方法定義済み）

**並列実行不可条件**:
❌ 逐次依存関係あり（Task A完了 → Task B実施）
❌ 同一ファイル編集（競合リスク）
❌ 共有リソース依存（DB Migrations等）
❌ コンテキスト共有必要（Phase全体理解が前提）

### MainAgent責務定義（具体化）

**MainAgent実行可能作業**:
✅ 全体調整・オーケストレーション
✅ SubAgentへの作業委託・指示
✅ 品質確認・統合テスト実行
✅ プロセス管理・進捗管理
✅ ドキュメント統合・レポート作成
✅ 例外（直接修正可能）:
   - 単純なtypo（1-2文字）
   - import文の追加のみ
   - コメントの追加・修正
   - 空白・インデントの調整

**MainAgent禁止事項（例外なし）**:
❌ 実装コードの直接修正（例外除く）
❌ ビジネスロジックの追加・変更
❌ 型変換ロジックの実装
❌ テストコードの作成・修正
❌ データベーススキーマの変更

### Fix-Mode活用ガイドライン

**エラー発生時の対応原則**:
1. エラー内容で責務判定（発生場所・タイミング問わず）
2. 責務マッピングでSubAgent選定
3. Fix-Mode活用：`"[SubAgent名] Agent, Fix-Mode: [修正内容]"`
4. 効率性より責務遵守を優先

**責務マッピング**:
- F# Domain/Application層 → fsharp-domain/fsharp-application
- F#↔C#境界・型変換 → contracts-bridge
- C# Infrastructure/Web層 → csharp-infrastructure/csharp-web-ui
- テストエラー → unit-test/integration-test
```

**3. 実体確認手順具体化**（1.5-2時間）:

```markdown
## 🔴 SubAgent成果物実体確認手順（ADR_016必須）

### 物理的存在確認（必須）

**手順1: ファイルパス明示**
- 絶対パス記載必須（相対パスは不可）
- 例: `/Doc/08_Organization/Active/Phase_XX/Research/Report.md`

**手順2: ファイル存在確認**
- Readツール実行またはls -lhコマンド実行必須
- 存在しない場合はエラー報告・SubAgent再実行

**手順3: ファイルサイズ確認**
- 空ファイル検出（0 bytes → エラー）
- 目標サイズ範囲確認（例: 10,000-20,000 bytes）
- サイズ不足の場合は内容不足として再実行

**手順4: 内容品質確認**
- 具体性確認: 抽象的記述なし・実装可能レベル
- 実用性確認: Phase2以降で活用可能
- 完全性確認: 必須項目全て記載
- 品質不足の場合はSubAgent再実行

### 虚偽報告防止（重大違反）

❌ **禁止事項**:
- 実体のない成果物の報告禁止
- 「作成しました」のみの報告禁止（ファイルパス明示必須）
- SubAgent報告の無検証信頼禁止（必ず実体確認）
- 内容未確認での成果物報告禁止

✅ **必須実行事項**:
- 全成果物の物理的存在確認（Readまたはls -lh実行）
- ファイルサイズ確認（空ファイル検出）
- 内容品質確認（具体性・実用性・完全性）
- 確認記録を組織設計ファイルに記載

### 実体確認チェックリスト（Step終了時必須）

□ 全成果物のファイルパス明示（絶対パス）
□ Readツールまたはls -lhコマンド実行完了
□ ファイルサイズ確認完了（空ファイル検出）
□ 内容品質確認完了（具体性・実用性・完全性）
□ 成果物リスト完全一致（計画と実績の照合）
□ 確認記録を組織設計ファイルに記載
```

---

## 🎯 Step成功基準

### 達成目標

**Agent Skills Phase 2展開**:
- ✅ 5-7個のSkill作成完了（tdd-red-green-refactor、spec-compliance-auto、adr-knowledge-base等）
- ✅ 補助ファイル充実完了（ADR抜粋・パターン集・判定基準詳細）
- ✅ `.claude/skills/README.md`更新完了
- ✅ Claudeが自律的にSkillを参照・適用していることを確認
- ✅ ADR/Rules知見の体系的Skill化完了

**CLAUDE.mdルール強化**（Agent SDK代替手段）:
- ✅ プロセス遵守ルール詳細化完了（ADR_016絶対遵守原則強化）
- ✅ SubAgent選択チェックリスト明記完了
- ✅ 実体確認手順具体化完了
- ✅ 期待効果: ADR_016違反率5-10% → 0-2%
- ✅ 期待効果: プロセス遵守チェック実行率80-90% → 100%
- ✅ 期待効果: SubAgent並列実行成功率70-80% → 95%

### 品質基準

**Skills品質**:
- ✅ 具体性: 実行可能なパターン・チェックリスト提供
- ✅ 実用性: Phase C以降で即座に活用可能
- ✅ 完全性: 必要な補助ファイル全て揃っている
- ✅ 自律適用性: Claudeが自律的に参照・適用可能

**CLAUDE.mdルール品質**:
- ✅ 明確性: 曖昧性なし・例外なし・明示的指示
- ✅ 実効性: Phase C以降の違反率0-2%達成可能
- ✅ 検証可能性: 遵守状況の測定可能
- ✅ 継続的改善: Phase C-D実績に基づく改善可能

### 完了準備

**Step3開始準備**:
- ✅ Playwright統合基盤刷新内容確定（Commands/SubAgent更新）
- ✅ Commands改善内容確定（phase-start/step-start/step-end-review チェックリスト）
- ⏳ Step03_Playwright_Commands.md作成（次Step組織設計）

**Phase継続準備**:
- ✅ CLAUDE.mdルール強化完了（Agent SDK代替手段実施）
- ✅ Agent Skills Phase 2展開完了（8-10個Skillsへ拡充）
- ✅ ADR/Rules知見体系化完了（自律適用可能なガイド確立）

---

## 📊 Step実行記録

（Step2実施時に記録予定）

---

## ✅ Step終了時レビュー（ADR_013準拠）

（Step2完了時に記録予定）

---

**作成日**: 2025-10-29
**最終更新**: 2025-10-29（Step1完了・Step2組織設計作成時）
