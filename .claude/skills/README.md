# Agent Skills - Phase 1

## 概要

このディレクトリには、Claude Codeが自律的に使用するAgent Skillsが含まれています。Phase 1では、F#↔C#相互運用とClean Architecture準拠性の2つの重要な知見をSkill化しました。

## 導入日

**Phase 1導入日**: 2025-10-21

## Skill一覧

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

## ADRからの移行

Phase 1では、以下のADRの知見をSkills化しました：

| ADR | 移行先Skill | バックアップ場所 |
|-----|-------------|-----------------|
| ADR_010（実装規約） | clean-architecture-guardian | `Doc/07_Decisions/backup/` |
| ADR_019（namespace設計規約） | clean-architecture-guardian | `Doc/07_Decisions/backup/` |

**移行理由**: 効果測定の正確性確保（Skillsからのみ知見を参照させる）

**バックアップ**: ADRは完全削除ではなく、`backup/`ディレクトリで保持

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

## Phase 2以降の計画

### Phase 2: 本格展開（Phase B3-B4期間中・2-3時間）

**追加予定Skill**:
1. `tdd-red-green-refactor` - TDD実践ガイド
2. `spec-compliance-auto` - 仕様準拠チェック
3. `adr-knowledge-base` - ADR参照
4. （オプション）`subagent-patterns` - SubAgent活用パターン
5. （オプション）`test-architecture` - テストアーキテクチャ

**目標**: 5-7個のSkill完全実装・ADR/Rules知見の体系的Skill化

### Phase 3: Plugin化・横展開（Phase B完了後・1-2時間）

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
├── README.md                           # このファイル
├── fsharp-csharp-bridge/
│   ├── SKILL.md                        # メインSkillファイル
│   └── patterns/
│       ├── result-conversion.md        # Result型変換パターン
│       ├── option-conversion.md        # Option型変換パターン
│       ├── du-conversion.md            # Discriminated Union変換パターン
│       └── record-conversion.md        # Record型変換パターン
└── clean-architecture-guardian/
    ├── SKILL.md                        # メインSkillファイル
    └── rules/
        ├── layer-separation.md         # レイヤー分離原則
        └── namespace-design.md         # namespace設計規約
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

---

**最終更新**: 2025-10-26（Agent Skills作成判断基準追加）
