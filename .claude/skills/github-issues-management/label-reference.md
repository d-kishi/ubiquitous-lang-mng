# ラベル体系一覧（クイックリファレンス）

**目的**: GitHub Issueラベル体系の迅速な参照

**参照元**: `.claude/rules/operations/github-issues-rules.md`

## 📊 ラベル体系全体像

### 3つのカテゴリ

1. **種別ラベル**（必須・1つ選択）
2. **優先度ラベル**（必須・1つ選択）
3. **影響範囲ラベル**（任意・複数選択可）

---

## 1️⃣ 種別ラベル（必須・1つ選択）

| ラベル | 説明 | 対象例 | 対応目安 |
|--------|------|--------|---------|
| `tech-debt` | 技術的負債 | コード重複、密結合 | Phase/Step単位 |
| `architecture` | アーキテクチャ問題 | 層間依存、設計原則違反 | Phase単位 |
| `security` | セキュリティ問題 | 脆弱性、セキュリティ要件未達 | 即座対応 |
| `performance` | パフォーマンス問題 | 処理速度、メモリ使用量 | Phase/Step単位 |
| `maintainability` | 保守性問題 | 可読性、テスト性 | Phase/Step単位 |
| `compliance` | 仕様準拠問題 | 要件・設計書からの逸脱 | Phase/Step単位 |

### 選択ガイド（クイック判断）

```
設計原則違反 → architecture
セキュリティリスク → security
処理速度問題 → performance
要件・設計書不整合 → compliance
可読性・テスト性 → maintainability
コード重複・密結合 → tech-debt
```

---

## 2️⃣ 優先度ラベル（必須・1つ選択）

| ラベル | 説明 | 対応目安 | 判断基準 |
|--------|------|---------|---------|
| `priority/critical` | 緊急対応必要 | 24時間以内 | システム停止・重大セキュリティリスク |
| `priority/high` | 高優先度 | 1週間以内 | 機能不全・ユーザー影響大 |
| `priority/medium` | 中優先度 | 1ヶ月以内 | 保守性低下・将来的リスク |
| `priority/low` | 低優先度 | 次Phase以降 | 改善余地・技術的負債 |

### 選択ガイド（クイック判断）

```
システム停止・重大脆弱性 → priority/critical
機能不全・ユーザー影響大 → priority/high
保守性低下・将来的リスク → priority/medium
改善余地・技術的負債 → priority/low
```

---

## 3️⃣ 影響範囲ラベル（任意・複数選択可）

| ラベル | 説明 | 対象ディレクトリ |
|--------|------|-----------------|
| `scope/domain` | F# ドメイン層 | `src/UbiquitousLanguageManager.Domain/` |
| `scope/application` | F# アプリケーション層 | `src/UbiquitousLanguageManager.Application/` |
| `scope/contracts` | C# Contracts層 | `src/UbiquitousLanguageManager.Contracts/` |
| `scope/infrastructure` | C# Infrastructure層 | `src/UbiquitousLanguageManager.Infrastructure/` |
| `scope/web` | C# Web層 | `src/UbiquitousLanguageManager.Web/` |
| `scope/tests` | テスト関連 | `tests/` |
| `scope/docs` | ドキュメント | `Doc/` |

### 選択ガイド（複数選択可）

```
影響ファイルのディレクトリを確認
  ↓
該当するすべての層を選択
  ↓
複数層にまたがる場合、すべて選択
```

---

## 🎯 よくあるラベル組み合わせパターン

### パターン1: Clean Architecture違反

```bash
--label "architecture,priority/medium,scope/domain,scope/application"
```

**理由**:
- 種別: 設計原則違反 → `architecture`
- 優先度: 保守性低下 → `priority/medium`
- 影響範囲: Domain層・Application層 → `scope/domain,scope/application`

### パターン2: セキュリティ脆弱性

```bash
--label "security,priority/critical,scope/web,scope/infrastructure"
```

**理由**:
- 種別: セキュリティリスク → `security`
- 優先度: 重大脆弱性 → `priority/critical`
- 影響範囲: Web層・Infrastructure層 → `scope/web,scope/infrastructure`

### パターン3: コード重複

```bash
--label "tech-debt,priority/low,scope/application"
```

**理由**:
- 種別: 技術的負債 → `tech-debt`
- 優先度: 改善余地 → `priority/low`
- 影響範囲: Application層 → `scope/application`

### パターン4: 仕様逸脱

```bash
--label "compliance,priority/high,scope/web,scope/docs"
```

**理由**:
- 種別: 要件・設計書不整合 → `compliance`
- 優先度: ユーザー影響大 → `priority/high`
- 影響範囲: Web層・ドキュメント → `scope/web,scope/docs`

### パターン5: パフォーマンス問題

```bash
--label "performance,priority/high,scope/infrastructure"
```

**理由**:
- 種別: 処理速度問題 → `performance`
- 優先度: ユーザー影響大 → `priority/high`
- 影響範囲: Infrastructure層 → `scope/infrastructure`

### パターン6: 保守性問題（複数層）

```bash
--label "maintainability,priority/medium,scope/domain,scope/application,scope/contracts"
```

**理由**:
- 種別: 可読性・テスト性 → `maintainability`
- 優先度: 保守性低下 → `priority/medium`
- 影響範囲: Domain層・Application層・Contracts層 → `scope/domain,scope/application,scope/contracts`

---

## 🔧 標準ラベル（GitHubデフォルト）

### 運用規則対象外の場合に使用

| ラベル | 説明 | 使用例 |
|--------|------|--------|
| `bug` | Something isn't working | 外部依存のバグ（VSCode拡張機能等） |
| `documentation` | Improvements or additions to documentation | ドキュメント改善（運用規則対象外） |
| `enhancement` | New feature or request | 軽微な機能追加（Phase計画外） |
| `question` | Further information is requested | 技術的な質問・相談 |

### プロジェクト固有ラベル

| ラベル | 説明 | 使用例 |
|--------|------|--------|
| `organization` | 組織管理運用・プロセス改善 | プロセス改善・Commands改善 |
| `test-architecture` | テストアーキテクチャ・基盤改善 | テスト基盤改善・ADR_020関連 |
| `clean-architecture` | Clean Architecture・設計改善 | Clean Architecture準拠性向上 |
| `playwright` | Playwright MCP/Agents関連 | E2Eテスト・Playwright MCP活用 |
| `phase-management` | Phase/Step管理・Commands改善 | Phase/Step管理プロセス改善 |

---

## 📋 ラベル選択チェックリスト

### Issue作成前の確認

- [ ] **種別ラベル**: 1つ選択（必須）
- [ ] **優先度ラベル**: 1つ選択（必須）
- [ ] **影響範囲ラベル**: 該当するすべて選択（任意）
- [ ] **標準ラベル**: 運用規則対象外の場合のみ使用

### ラベル選択後の確認

- [ ] 種別ラベルは1つのみ（複数選択していないか）
- [ ] 優先度ラベルは1つのみ（複数選択していないか）
- [ ] 影響範囲ラベルは該当するすべて選択（漏れがないか）

---

## 🔍 ラベル検索・フィルタリング

### GitHub CLI活用

```bash
# 優先度別フィルタ
gh issue list --label "priority/high"

# 種別別フィルタ
gh issue list --label "architecture"

# 影響範囲別フィルタ
gh issue list --label "scope/domain"

# 複合条件検索
gh issue list --label "architecture,priority/high" --state "open"

# 特定Phaseのみ
gh issue list --label "phase-a7"
```

### GitHub Web UI活用

```
# URLフィルタ形式
https://github.com/owner/repo/issues?q=is:issue+is:open+label:architecture+label:priority/high
```

---

## ⚠️ よくある間違い

### 間違い1: 種別ラベルの複数選択

❌ **誤**: `--label "tech-debt,architecture"`

✅ **正**: `--label "architecture"`（最も適切な1つ）

### 間違い2: 優先度ラベルの複数選択

❌ **誤**: `--label "priority/high,priority/medium"`

✅ **正**: `--label "priority/high"`（最も適切な1つ）

### 間違い3: 影響範囲ラベルの未選択

❌ **誤**: `--label "architecture,priority/medium"`（影響範囲なし）

✅ **正**: `--label "architecture,priority/medium,scope/domain,scope/application"`

### 間違い4: 存在しないラベルの使用

❌ **誤**: `--label "infrastructure"`（存在しない）

✅ **正**: `--label "scope/infrastructure"`（正しいラベル）

---

## 📚 利用可能なラベル一覧（gh label list）

**確認コマンド**:
```bash
gh label list
```

**出力例**（抜粋）:
```
bug                      Something isn't working
documentation            Improvements or additions to documentation
tech-debt                技術負債
architecture             アーキテクチャ問題
priority/critical        緊急対応必要
priority/high            高優先度
scope/domain             F# ドメイン層
scope/application        F# アプリケーション層
...
```

---

**最終更新**: 2025-11-15
