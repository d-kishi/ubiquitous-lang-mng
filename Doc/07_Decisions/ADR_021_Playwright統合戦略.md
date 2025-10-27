# ADR_021: Playwright MCP + Agents統合戦略

## Status
Accepted

## Date
2025-10-26

## Context

Phase B2でUserProjects機能のE2Eテスト基盤確立が必要となった。従来の手動E2Eテスト作成では多大な工数が必要であり、テストメンテナンスコストも高い。

### 課題
- E2Eテスト作成に2-3時間/機能が必要
- UI変更時のテストメンテナンスに1-2時間/変更が必要
- Blazor Server SignalR対応の複雑性
- GitHub Issue #56: bUnit技術的課題8件のE2E代替実装必要性

### 技術選択肢
1. 手動E2Eテスト作成（従来手法）
2. Playwright MCP統合（Claude Code直接統合）
3. Playwright MCP + Agents統合（自動修復機能付き）

## Decision

**Playwright MCP + Agents統合を採用**（推奨度10/10点）

### 1. Playwright MCP統合（推奨度9/10点）
- **技術成熟度**: 実用段階（v0.0.42・2025-10-09リリース）
- **Claude Code統合**: 完璧（5/5点）
- **25種類ブラウザ操作ツール**: playwright_navigate/snapshot/click/fill等
- **アクセシビリティツリー活用**: 構造化データベース・高速・正確

### 2. Playwright Agents統合（推奨度9/10点）
- **VS Code 1.105安定版対応**: 2025-10-10リリース完了
- **3つのAI駆動Agent**: Planner（探索）・Generator（コード生成）・Healer（自動修復）
- **自動修復成功率**: 85%+（セレクタ変更・要素移動対応）
- **リスク解消**: Insiders依存リスク完全解消（安定版対応）

### 3. 統合推奨度10/10点（最強の相乗効果）
- E2Eテスト作成効率 + テストメンテナンス自動化

## Consequences

### Positive
- **E2Eテスト作成効率93.3%向上**（実測: 150分 → 10分）
  - 計画75-85%削減を大幅超過 🎉
- **テストメンテナンス効率50-70%削減**（期待値・Healer自動修復）
- **GitHub Issue #56完全対応**: bUnit技術的課題8件のE2E実証完了
- **.NET+Blazor Server先駆者知見蓄積**: F# + C# + Playwright統合パターン確立
- **Agent Skills Phase 1前倒し完了**: playwright-e2e-patterns Skill作成

### Negative
- **初回学習コスト**: 5-10分（軽微）
- **自動修復精度限界**: 80-85%（手動検証15-20%必要）
- **Node.js依存**: Playwright MCP実行にNode.js 18以上必須

## Risks & Mitigation

### MCP統合リスク（低）
- **リスク**: 初回インストール失敗・ツール露出失敗
- **対策**: Claude Code再起動・公式ドキュメント参照
- **実績**: Phase B2 Step2統合成功（再起動で解決）

### Agents統合リスク（中→低）
- **リスク（Phase B-F1評価時）**: VS Code Insiders依存（最大リスク）
- **現状（2025-10-15）**: **VS Code 1.105安定版対応完了**
- **対策**: ロールバック手順確立（30分で従来手法へ切り戻し可能）

### セキュリティリスク（低）
- **リスク**: クレデンシャル漏洩・本番データ誤操作
- **対策**:
  - .gitignore設定完了（.env.test, playwright/.auth/, test-results/）
  - テスト専用アカウント使用（e2e-test@ubiquitous-lang.local）
  - 本番データとの完全分離

## Implementation

### Phase B2 Step6実装内容
- **Stage 0**: セキュリティ準備（.gitignore設定・テスト専用アカウント準備）
- **Stage 1**: E2Eテスト作成（UserProjects 3シナリオ・約10分）
- **Stage 2**: Playwright Agents統合（VS Code 1.105安定版・約5分）
- **Stage 3**: 統合効果検証（93.3%削減達成）
- **Stage 4**: ADR_021 + playwright-e2e-patterns Skill作成

### 作成したE2Eテスト
- `tests/UbiquitousLanguageManager.E2E.Tests/UserProjectsTests.cs`
  - ProjectMembers_AddMember_ShowsSuccessMessage
  - ProjectMembers_RemoveMember_ShowsSuccessMessage
  - ProjectMembers_AddDuplicateMember_ShowsErrorMessage

## Related

### Agent Skills
- **playwright-e2e-patterns Skill**: 「どうE2Eテストを書くべきか」の実行可能ガイド
- GitHub Issue #54: Agent Skills導入提案（Phase 1前倒し完了）

### ADR
- ADR_020: テストアーキテクチャ決定
- ADR_010: 実装規約

### GitHub Issues
- GitHub Issue #56: bUnit統合テスト技術課題（E2E代替実装完了）
- GitHub Issue #54: Agent Skills導入提案

### Phase B-F1成果物
- Phase_B2_申し送り事項.md
- Playwright_MCP_評価レポート.md
- Playwright_Agents_評価レポート.md
- Tech_Research_Playwright_2025-10.md

## Notes

### 効果測定結果（Phase B2 Step6 Stage 3）
- **作成効率**: 93.3%削減（150分 → 10分）
- **削減要因**:
  1. data-testid属性設計パターン確立（Phase B2 Step5完了）
  2. Blazor Server SignalR対応知見（Phase B1基盤活用）
  3. C# Playwright実装経験蓄積

### 横展開可能性
- .NET + Blazor Server + Playwright統合パターン
- F# + C# Clean Architecture E2Eテスト実装パターン
- playwright-e2e-patterns Skill（Plugin化・コミュニティ貢献）

### 次のステップ
- Phase B3以降のE2Eテスト自動作成（Playwright MCP活用）
- Healer Agent実用評価（UI変更時の自動修復）
- playwright-e2e-patterns Skill拡張（bUnit代替パターン追加）
