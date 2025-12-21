---
name: github-issues-management
description: GitHub Issueを作成・管理する。「Issue作成」「技術的負債記録」「バグ報告」「課題管理」「ラベル設定」の際に使用する。
allowed-tools: Read, Bash
---

# GitHub Issues Management Skill

GitHub Issue作成時のラベル判断・運用規則の自律的適用を実現。

## 使用タイミング

1. **GitHub Issue作成時** - `gh issue create`コマンド実行前・ラベル選択時
2. **技術的負債記録時** - 技術的負債の発見・分析完了時
3. **課題管理時** - アーキテクチャ問題・品質課題の記録時

---

## 基本指針

### 1. 運用規則の必須参照

**原則**: GitHub Issue作成前に必ず運用規則を参照

**参照**: [`references/github-issues-rules.md`](./references/github-issues-rules.md)

### 2. ラベル選択の3段階判断

```
1. 種別ラベル判断（必須・1つ選択）
   ↓
2. 優先度ラベル判断（必須・1つ選択）
   ↓
3. 影響範囲ラベル判断（任意・複数選択可）
```

**詳細**: [`references/label-selection-guide.md`](./references/label-selection-guide.md)

### 3. Issue作成基準

```
1. 作成対象か確認（✅/❌判定）
   ↓
2. 作成判断基準の評価（4基準）
   ↓
3. Issue作成 or Phase計画管理
```

**詳細**: [`references/creation-criteria.md`](./references/creation-criteria.md)

### 4. テンプレート活用

**原則**: 基本テンプレートを必ず使用

**詳細**: [`references/issue-template-patterns.md`](./references/issue-template-patterns.md)

---

## 参照ファイル

| ファイル | 内容 |
|---------|------|
| [`github-issues-rules.md`](./references/github-issues-rules.md) | 運用規則（必須参照） |
| [`label-selection-guide.md`](./references/label-selection-guide.md) | ラベル判断基準・選択ロジック |
| [`label-reference.md`](./references/label-reference.md) | ラベル体系一覧（クイックリファレンス） |
| [`creation-criteria.md`](./references/creation-criteria.md) | Issue作成判断基準 |
| [`issue-template-patterns.md`](./references/issue-template-patterns.md) | テンプレート活用パターン |

## 関連リソース

- **ADR**: `Doc/07_Decisions/ADR_015_技術的負債管理のGitHub_Issues移行.md`
- **関連Skills**: `spec-compliance-auto`, `adr-knowledge-base`
- **関連Commands**: `step-end-review`, `phase-end`

---

**作成日**: 2025-11-15
**Phase**: Phase B-F2
