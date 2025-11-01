# Agent Skills

## 概要

このディレクトリには、Claude Codeが自律的に使用するAgent Skillsが含まれています。Phase 1（2025-10-21）でF#↔C#相互運用とClean Architecture準拠性、Phase 2（2025-11-01）でTDD実践・仕様準拠・ADR知見・SubAgent組み合わせ・テストアーキテクチャの計7個のSkillsを導入しました。

## 導入日

- **Phase 1導入日**: 2025-10-21
- **Phase 2導入日**: 2025-11-01

## Skill一覧（全7個）

### Phase 1 Skills（2個）

### 1. fsharp-csharp-bridge

**目的**: F# Domain/Application層とC# Infrastructure/Web層の型変換パターンを自律的に適用

**使用タイミング**:
- F#↔C#境界コード実装時
- 型変換エラー発生時
- contracts-bridge Agent作業時

**提供パターン**:
1. F# Result型 ↔ C# 統合パターン
2. F# Option型 ↔ C# 統合パターン
3. F# Discriminated Union ↔ C# 統合パターン
4. F# Record型 ↔ C# 統合パターン

**Phase B1実証結果**:
- 実装箇所: 36ファイル
- エラー修正: 36件完全解決
- 成功率: 100%（0 Warning/0 Error）

**詳細**: [fsharp-csharp-bridge/SKILL.md](./fsharp-csharp-bridge/SKILL.md)

---

### 2. clean-architecture-guardian

**目的**: Clean Architecture準拠性を自動チェック

**使用タイミング**:
- 新規実装時
- リファクタリング時
- Step/Phase完了時
- 問題発生時（循環依存・namespace衝突）

**チェック項目**:
1. レイヤー分離原則（C#→F#許可、F#→C#禁止）
2. namespace階層化ルール（Bounded Context別サブnamespace）
3. Bounded Context境界（Common/Authentication/ProjectManagement等）
4. F# Compilation Order（依存関係順）

**Phase B1品質基準**:
- Clean Architecture スコア: 97/100点
- 循環依存: 0件
- ビルド: 0 Warning/0 Error

**詳細**: [clean-architecture-guardian/SKILL.md](./clean-architecture-guardian/SKILL.md)

---

### Phase 2 Skills（5個）

### 3. tdd-red-green-refactor

**目的**: TDD Red-Green-Refactorサイクル実践パターンの自律的適用

**使用タイミング**:
- unit-test Agent作業時
- TDDサイクル実践時
- テスタブルコード設計時

**提供パターン**:
1. Red Phase詳細手順（失敗するテスト作成）
2. Green Phase詳細手順（最小限の実装で合格）
3. Refactor Phase詳細手順（コード改善・リファクタリング）
4. テスタブルコード設計原則
5. テストカバレッジ管理方法（97%目標）

**Phase B1実証結果**:
- テストカバレッジ: 97%達成
- TDDサイクル実践: 全Step適用
- 品質スコア: 97/100点

**詳細**: [tdd-red-green-refactor/SKILL.md](./tdd-red-green-refactor/SKILL.md)

---

### 4. spec-compliance-auto

**目的**: 仕様準拠チェックの自律的適用（spec-compliance-check Command活用）

**使用タイミング**:
- Step/Phase完了時
- 仕様準拠確認時
- spec-compliance Agent作業時

**提供パターン**:
1. 機能要件確認パターン
2. 非機能要件確認パターン
3. データ整合性確認パターン
4. UI/UX要件確認パターン

**Phase B1実証結果**:
- 仕様準拠率: 95%達成
- 仕様逸脱リスク: 早期特定・対策実施
- 受け入れ基準: 100%達成

**詳細**: [spec-compliance-auto/SKILL.md](./spec-compliance-auto/SKILL.md)

---

### 5. adr-knowledge-base

**目的**: ADR知見の体系的参照・適用（主要ADR抜粋提供）

**使用タイミング**:
- 実装規約確認時
- プロセス遵守確認時
- namespace設計時
- テストアーキテクチャ設計時

**提供ADR抜粋**:
1. ADR_010_実装規約（Blazor Server・F#初学者対応）
2. ADR_016_プロセス遵守（絶対原則・禁止行為）
3. ADR_019_namespace設計（Bounded Context別サブnamespace）
4. ADR_020_テストアーキテクチャ（レイヤー×テストタイプ分離）

**Phase B1実証結果**:
- ADR参照効率化: 抜粋による高速参照
- プロセス遵守: 違反防止
- 品質スコア: 97/100点

**詳細**: [adr-knowledge-base/SKILL.md](./adr-knowledge-base/SKILL.md)

---

### 6. subagent-patterns

**目的**: 13種類のSubAgent組み合わせパターン・選択ロジック提供

**使用タイミング**:
- Step開始時（最重要）
- SubAgent選択迷い時
- エラー修正時（Fix-Mode活用）
- Phase計画時

**提供パターン**:
1. 調査分析系Agent選択（tech-research, spec-analysis, design-review, dependency-analysis）
2. 実装系Agent選択（fsharp-domain, fsharp-application, contracts-bridge, csharp-infrastructure, csharp-web-ui）
3. 品質保証系Agent選択（unit-test, integration-test, code-review, spec-compliance）
4. Phase特性別組み合わせパターン（Pattern A/B/C/D）
5. 並列実行判断ロジック

**Phase B1, B2実証結果**:
- SubAgent責務境界違反: 0件
- 並列実行効率化: 30-40%
- 品質スコア: 97/100点

**詳細**: [subagent-patterns/SKILL.md](./subagent-patterns/SKILL.md)

---

### 7. test-architecture

**目的**: テストアーキテクチャ自律適用（ADR_020レイヤー×テストタイプ分離方式）

**使用タイミング**:
- 新規テストプロジェクト作成時（最重要）
- unit-test/integration-test Agent作業開始時
- テストアーキテクチャ違反検出時

**提供パターン**:
1. 新規テストプロジェクト作成7 Phaseチェックリスト
2. 命名規則厳守（`UbiquitousLanguageManager.{Layer}.{TestType}.Tests`）
3. 参照関係原則（Unit/Integration/UI/E2E別）
4. Issue #40再発防止チェックリスト

**Phase B2, Issue #40実証結果**:
- テストアーキテクチャ再構成: 7プロジェクト分割成功
- Issue #40再発防止: チェックリスト適用
- ビルド品質: 0 Warning/0 Error

**詳細**: [test-architecture/SKILL.md](./test-architecture/SKILL.md)

---

## ADR・Rulesからの移行

### Phase 1移行（2025-10-21）

| ADR/Rules | 移行先Skill | バックアップ場所 |
|-----------|-------------|-----------------|
| ADR_010（実装規約） | clean-architecture-guardian | `Doc/07_Decisions/backup/` |
| ADR_019（namespace設計規約） | clean-architecture-guardian | `Doc/07_Decisions/backup/` |

### Phase 2移行（2025-11-01）

| ADR/Rules | 移行先Skill | バックアップ場所 |
|-----------|-------------|-----------------|
| TDD実践ガイド（暗黙知） | tdd-red-green-refactor | - |
| 仕様準拠ガイド.md | spec-compliance-auto | `Doc/08_Organization/Rules/backup/` |
| ADR_010/016/019/020（抜粋） | adr-knowledge-base | - |
| SubAgent組み合わせパターン.md | subagent-patterns | `Doc/08_Organization/Rules/backup/` |
| 新規テストプロジェクト作成ガイドライン（参照） | test-architecture | - |

**移行理由**: 効果測定の正確性確保（Skillsからのみ知見を参照させる）

**バックアップ**: ADR・Rulesは完全削除ではなく、`backup/`ディレクトリで保持

---

## Agent Skills作成判断基準（2025-10-26新設）

### ADR vs Skills: いつSkillsを作成すべきか

技術的知見が発生した際、ADR（Architectural Decision Record）とAgent Skillsのどちらを作成すべきか迷った場合は、以下の判断基準を参照してください。

#### 簡潔な判断フロー（30秒チェック）

1. **歴史的記録が必要か？**（なぜこの決定をしたか）
   → YES: **ADR作成**

2. **Claudeが自律的に適用すべきか？**（実装時に自動適用）
   → YES: **Agent Skills作成**

3. **技術選定の根拠か？**（代替案との比較・リスク評価）
   → YES: **ADR作成**

4. **実装パターン・チェックリストか？**（繰り返し使うパターン）
   → YES: **Agent Skills作成**

#### ADRとSkillsの違い

| 観点 | ADR | Agent Skills |
|-----|-----|--------------|
| **目的** | 「**なぜ**その技術決定をしたか」を記録 | 「**どう**実装すべきか」をガイド |
| **性質** | 歴史的文書・技術選定の根拠 | 実行可能な知見・自律的適用 |
| **内容** | Context・Decision・Consequences・Risks | パターン・判断基準・チェックリスト |
| **参照** | 技術選定の振り返り・将来の技術変更時 | Claudeが自律的に判断して使用 |

#### 詳細ガイドライン

**迷った時の詳細参照**: `Doc/08_Organization/Rules/ADRとAgent_Skills判断ガイドライン.md`

**提供内容**:
- 判断フローチャート
- 判断基準マトリックス（5W1H）
- ADR適用例・Skills適用例
- 移行事例（ADR→Skills）
- 迷った時のチェックリスト（10問）

#### Skills作成時の必須確認事項

新しいSkillを作成する前に、以下を確認してください：

- [ ] この知見は**繰り返し使う実装パターン**ですか？
- [ ] この知見は**自動チェック・自動適用可能**ですか？
- [ ] この知見は**Claudeが状況に応じて自律判断すべき**ものですか？
- [ ] この知見は**パターン・チェックリストとして表現可能**ですか？

**3問以上YESなら Skills作成推奨**

---

## Skillsの使い方

### Claudeによる自律的使用

Claudeは以下の状況で自動的にSkillsを参照します：

1. **F#↔C#境界コード実装時**: `fsharp-csharp-bridge`を自動参照
2. **新規クラス・モジュール作成時**: `clean-architecture-guardian`を自動参照
3. **型変換エラー発生時**: `fsharp-csharp-bridge`を自動参照
4. **namespace変更時**: `clean-architecture-guardian`を自動参照

**ユーザーによる明示的呼び出しは不要**です。Claudeが状況に応じて自律的に判断します。

### 効果測定

Phase 1の効果は以下のドキュメントで測定されます：

**測定ドキュメント**: `Doc/08_Organization/Active/AgentSkills_Phase1_効果測定.md`

**測定項目**:
- Claudeの自律的Skill使用頻度
- 判定精度（型変換パターン適合率・Clean Architecture準拠判定精度）
- 作業効率改善度（質問回数減少・エラー発生率減少）

---

## Phase 2完了（2025-11-01）

### 完了Skill（5個）

1. ✅ `tdd-red-green-refactor` - TDD実践ガイド
2. ✅ `spec-compliance-auto` - 仕様準拠チェック
3. ✅ `adr-knowledge-base` - ADR参照
4. ✅ `subagent-patterns` - SubAgent活用パターン
5. ✅ `test-architecture` - テストアーキテクチャ

**達成**: 計7個のSkill完全実装・ADR/Rules知見の体系的Skill化

### Phase 3計画: Plugin化・横展開（Phase B完了後・1-2時間）

**目標**: 他プロジェクトへの知見共有

**実施内容**:
1. `ubiquitous-language-manager-skills` Plugin作成
2. GitHub公開・コミュニティ貢献
3. Claude Code Marketplace申請検討
4. ADR_021作成（Agent Skills導入決定）

---

## ディレクトリ構造

```
.claude/skills/
├── README.md                                    # このファイル
│
├── fsharp-csharp-bridge/                        # Phase 1
│   ├── SKILL.md
│   └── patterns/
│       ├── result-conversion.md
│       ├── option-conversion.md
│       ├── du-conversion.md
│       └── record-conversion.md
│
├── clean-architecture-guardian/                 # Phase 1
│   ├── SKILL.md
│   └── rules/
│       ├── layer-separation.md
│       └── namespace-design.md
│
├── tdd-red-green-refactor/                      # Phase 2
│   ├── SKILL.md
│   └── patterns/
│       ├── red-phase-pattern.md
│       ├── green-phase-pattern.md
│       └── refactor-phase-pattern.md
│
├── spec-compliance-auto/                        # Phase 2
│   ├── SKILL.md
│   └── rules/
│       ├── functional-requirements-check.md
│       ├── non-functional-requirements-check.md
│       ├── data-integrity-check.md
│       └── ui-ux-requirements-check.md
│
├── adr-knowledge-base/                          # Phase 2
│   ├── SKILL.md
│   └── adr-excerpts/
│       ├── ADR_010_実装規約.md
│       ├── ADR_016_プロセス遵守.md
│       ├── ADR_019_namespace設計.md
│       └── ADR_020_テストアーキテクチャ.md
│
├── subagent-patterns/                           # Phase 2
│   ├── SKILL.md
│   ├── patterns/
│   │   ├── research-agents-selection.md
│   │   ├── implementation-agents-selection.md
│   │   ├── qa-agents-selection.md
│   │   └── phase-specific-combinations.md
│   └── rules/
│       └── agent-responsibility-boundary.md
│
└── test-architecture/                           # Phase 2
    ├── SKILL.md
    └── rules/
        ├── new-test-project-checklist.md
        ├── test-project-naming-convention.md
        └── test-project-reference-rules.md
```

---

## 関連ドキュメント

- **GitHub Issue #54**: Agent Skills導入提案
- **効果測定**: `Doc/08_Organization/Active/AgentSkills_Phase1_効果測定.md`
- **ADRバックアップ**: `Doc/07_Decisions/backup/README.md`
- **Phase B1実装記録**: `Doc/08_Organization/Completed/Phase_B1/Phase_Summary.md`

---

## 更新履歴

| 日付 | Phase | 内容 | 担当 |
|------|-------|------|------|
| 2025-10-21 | Phase 1 | 初版作成・fsharp-csharp-bridge + clean-architecture-guardian | Claude Code |
| 2025-10-26 | Phase B2 | Agent Skills作成判断基準セクション追加・ADR vs Skills使い分けガイド統合 | Claude Code |
| 2025-11-01 | Phase 2 | Phase 2完了・5つのSkills追加（tdd/spec-compliance/adr-knowledge/subagent/test-architecture）・計7個のSkills体系完成 | Claude Code |

---

**最終更新**: 2025-11-01（Phase 2完了・5 Skills追加・計7個）
