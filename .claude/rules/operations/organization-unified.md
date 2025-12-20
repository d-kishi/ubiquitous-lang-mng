# 組織運用補足規約（統合版）

## SubAgentプール概要（ADR_013）

14種類の事前定義SubAgentによる並列問題解決体系。
- **調査分析系（4）**: tech-research, spec-analysis, design-review, dependency-analysis
- **実装系（5）**: fsharp-domain, fsharp-application, contracts-bridge, csharp-infrastructure, csharp-web-ui
- **品質保証系（5）**: unit-test, integration-test, e2e-test, code-review, spec-compliance

**詳細**: `.claude/agents/`, `Doc/07_Decisions/ADR_013*.md`, `.claude/skills/subagent-patterns/`

---

## 開発手法ガイド

### 修正試行回数制限
3回失敗で戦略的延期（GitHub Issue登録→次Step進行）

### Step再実行プロセス
設計判断誤り発見時: git restore → Step組織設計書更新 → 再実行

### トレードオフ判断
| Phase | 原則 |
|-------|------|
| A-B（基盤確立期） | 品質優先 |
| C-D（機能実装期） | バランス |
| E-F（最適化期） | 効率優先 |

---

## ファイル管理規約

### ディレクトリ構造
```
Doc/08_Organization/
├── Active/Phase_XX/     # 実行中Phase（Phase_Summary.md + StepXX_*.md）
├── Completed/           # 完了Phase
└── Templates/           # テンプレート
```

### Phase/Stepファイル運用
- **Phase開始**: phase-start → ディレクトリ作成 → Phase_Summary.md作成
- **Step終了**: StepXX_*.md更新 → 次Step組織設計ファイル作成
- **Phase完了**: Phase総括記録 → Active→Completed移動

### セッション継続時必須読込
| 対象 | ファイル |
|------|---------|
| Research | `/Doc/05_Research/Phase_XX/` |
| Organization | `/Doc/08_Organization/Active/Phase_XX/` |

---

**統合元**: organization-cycle.md, development-methodology.md, file-management.md
**作成日**: 2025-12-20
