# 開発ガイドライン

## 🤖 Agent Skills活用方法（2025-10-21新設・Phase 1導入完了）

### Agent Skillsとは

**定義**: プロジェクト固有の知見・パターン・判断基準をモジュール化し、Claudeが自律的に適用する仕組み

**配置場所**: `.claude/skills/`

**使用方法**: Claudeが状況に応じて自律的に判断・使用（ユーザーの明示的呼び出し不要）

### Phase 1導入済みSkills

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

### 効果測定

**測定ドキュメント**: `Doc/08_Organization/Active/AgentSkills_Phase1_効果測定.md`

**測定期間**: Phase B2 Step5 ～ Phase B3完了

**期待効果**: 作業効率20-25分/セッション削減、品質向上（ADR遵守率90%→98%）

---

## プロセス遵守絶対原則（ADR_016）
...
（既存内容を維持）
...

## 🧪 E2Eテスト実装タイミング原則（2025-10-17確立・Phase B2 Step2で発見）
...
（既存内容を維持）
...

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

（以下、既存の development_guidelines 内容を維持）

## 🎯 重要: Blazor Server・F#初学者対応
...
（既存内容省略）
...

---

**最終更新**: 2025-10-21（**Agent Skills Phase 1導入完了・ADR_019移行記録追加**）
