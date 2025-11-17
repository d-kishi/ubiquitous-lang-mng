# Playwright運用統合ガイドライン

**策定日**: 2025-11-17
**最終更新**: 2025-11-17
**対象Phase**: Phase B2完了以降（Phase B-F2 ~ Phase B5）
**ステータス**: 運用中（Phase B2-F2 TypeScript移行完了）

---

## 📋 Executive Summary

### 統合状況サマリー

| 項目 | 現状 | 詳細 |
|------|------|------|
| **Playwright MCP統合** | ✅ 完了（Phase B2） | 25ツール利用可能・93.3%効率化実証 |
| **E2Eテスト基盤** | ✅ TypeScript移行完了（Phase B2-F2） | TypeScript/Playwright Test + Blazor Server SignalR対応 |
| **Playwright Test Agents** | ✅ 統合完了（Phase B2-F2） | Generator/Healer/Planner統合完了（60-70%効率化） |
| **次期強化** | 📅 Phase B3以降 | 実用評価・効果測定・パターン洗練 |

### 重要な技術的発見（2025-11-18調査 + Phase B2-F2移行完了）

1. **Playwright Test AgentsはTypeScript専用** ✅ **解決**
   - generator Agent: TypeScript対応確認 → **Phase B2-F2で正式統合完了**
   - planner Agent: 言語非依存（Markdown計画生成） → **Phase B2-F2で統合完了**
   - healer Agent: TypeScript環境で実用性確認 → **Phase B2-F2で統合完了**

2. **既存e2e-test Agent実績** ✅ **TypeScript移行完了**
   - Phase B2で93.3%効率化を実証（150分 → 10分/機能）
   - playwright-e2e-patterns Skill活用（TypeScript対応完了）
   - TypeScript/Playwright Test + Blazor Server SignalR完全対応

3. **TypeScript移行戦略の成功** ✅ **Phase B2-F2完了**
   - C# E2Eテストプロジェクト完全削除
   - TypeScript/Playwright Test移行完了
   - Generator/Healer/Planner Agents統合完了
   - Phase B2実績（93.3%効率化）継続保証

---

## 🎯 技術スタック・アーキテクチャ

### Playwright MCP Server

**導入状況**: ✅ Phase B2完了（2025-10-27）

**提供機能**:
- 25種類のブラウザ操作ツール（playwright_navigate, playwright_click, playwright_fill等）
- リアルタイムフィードバック・デバッグ効率化
- アクセシビリティツリー活用
- Claude Code直接統合

**統合推奨度**: ⭐⭐⭐⭐⭐ 9/10点

**導入コマンド**:
```bash
claude mcp add playwright npx '@playwright/mcp@latest'
```

**設定ファイル**: `.mcp.json`（プロジェクトルート）

### e2e-test Agent（現行標準）

**定義ファイル**: `.claude/agents/e2e-test.md`

**責務**:
- Blazor Server E2Eテスト実装
- Playwright MCP 21ツール活用
- Blazor Server SignalR対応パターン適用
- data-testid属性設計パターン適用

**対応技術**:
- C# Playwright（Microsoft.Playwright）
- Blazor Server（StateHasChanged・SignalR接続）
- F# ↔ C# 型変換パターン
- PostgreSQL統合テストデータ

**実績**:
- Phase B2: 93.3%効率化（150分 → 10分/機能）
- AuthenticationTests.cs: 6/6テスト成功（100%）
- UserProjectsTests.cs: 3シナリオ実装完了

**Agent Skills**: `.claude/skills/playwright-e2e-patterns/`
- data-testid設計パターン
- Playwright MCPツール活用パターン
- Blazor Server SignalR対応パターン

### Playwright Test Agents（段階的評価中）

**定義ファイル**:
- `.claude/agents/playwright-test-planner.md`
- `.claude/agents/playwright-test-generator.md`
- `.claude/agents/playwright-test-healer.md`

**MCP Server**: `.mcp.json`（playwright-test）

**技術的制約**:
- **TypeScript専用**（公式仕様）
- C# Playwrightとの直接連携不可
- 本プロジェクトではgenerator Agentは使用不可

**評価対象Agent**:
1. **healer Agent**:
   - テスト失敗時の自動修復
   - C#環境での実用性検証必要
   - Phase B3評価予定（修復成功率≥50%基準）

2. **planner Agent**:
   - Markdown形式テスト計画生成（言語非依存）
   - 手動計画との統合・補完
   - Phase B4評価予定（計画作成時間≥30%削減基準）

---

## 🔄 段階的移行計画（2025-11-18確定）

### Phase B-F2（現在・2025-11）

**実施内容**:
- ✅ e2e-test Agent専任継続（現行運用維持）
- ✅ Playwright Test Agents保持（削除しない・将来評価用）
- ✅ 組織管理運用マニュアル更新不要
- ✅ `.mcp.json`統合完了（playwright-test MCP Server追加）

**理由**:
- Phase A機能E2Eテスト実装に集中
- Phase B2実績パターン（93.3%効率化）活用
- リスク最小化（実用評価前の統合回避）
- Playwright Test AgentsはTypeScript専用（C# Playwright非対応）

### Phase B3（次期・2025-12予定）

**評価Step**: Phase B3 Step6（E2E拡充）

#### healer Agent実用評価

**評価方法**:
1. 既存E2Eテスト1件を意図的に失敗させる（セレクタ変更）
2. `🎭 healer, fix the failing test in tests/E2E.Tests/UserProjectsTests.cs`
3. 修復成功率・修復時間・提案内容を測定

**成功基準**: 50%以上の修復成功率

**評価結果に基づく対応**:
- **成功時**: healer Agent正式統合・ADR作成・Commands更新
- **失敗時**: 実用性不足として保留・Phase B4で再評価検討

**統合時の更新ドキュメント**（healer Agent評価成功時のみ）:
1. **組織管理運用マニュアル**（`Doc/08_Organization/Rules/組織管理運用マニュアル.md`）
   - 追加セクション: 「E2Eテスト失敗時の自動修復フロー」

2. **step-end-review Command**（`.claude/commands/step-end-review.md`）
   - 追加項目: E2Eテスト失敗時のhealer活用確認

3. **weekly-retrospective Command**（`.claude/commands/weekly-retrospective.md`）
   - 追加項目: healer Agent実績レポート

4. **ADR作成**
   - ADR_XXX: Playwright Healer Agent実用評価結果

### Phase B4（将来・2026-01予定）

**評価Step**: Phase B4 Step1（新機能分析）

#### planner Agent実用評価

**評価方法**:
1. `🎭 planner, explore the [new feature] and create a test plan`
2. 生成されたMarkdown計画と手動計画の比較
3. カバレッジ・計画作成時間を測定

**成功基準**: 30%以上の計画作成時間削減

**評価結果に基づく対応**:
- **成功時**: planner Agent正式統合・Agent Skills拡張
- **失敗時**: 補助的活用継続・必須化せず

**統合時の更新ドキュメント**（planner Agent評価成功時のみ）:
1. **step-start Command**（`.claude/commands/step-start.md`）
   - 追加項目: planner Agent活用

2. **playwright-e2e-patterns Skill**（`.claude/skills/playwright-e2e-patterns/SKILL.md`）
   - 新規パターン追加: `patterns/planner-usage.md`

3. **ADR作成**
   - ADR_XXX: Playwright Planner Agent実用評価結果

---

## 📊 効果測定実績・期待値

### Phase B2実績（2025-10-27完了）

| 指標 | 実績値 | 詳細 |
|------|-------|------|
| **E2Eテスト作成効率** | 93.3%削減 | 150分 → 10分/機能（3シナリオ） |
| **Generator Agent効果** | ⭐⭐⭐⭐⭐ | 60-70%時間削減（1-2時間削減） |
| **Healer Agent効果** | ⭐ | 0%成功率（複雑な状態管理問題検出不可） |
| **総合時間削減** | 40-50% | 人間-AI協調の重要性確認 |

### Phase B3期待値（healer Agent統合後）

| 指標 | 期待値 | 条件 |
|------|-------|------|
| **E2Eテスト作成効率** | 93.3%削減（維持） | e2e-test Agent継続 |
| **テスト失敗修復効率** | 50-70%削減 | healer成功率≥50% |
| **全体生産性向上** | 95%削減 | healer統合成功時 |

### Phase B4期待値（planner Agent統合後）

| 指標 | 期待値 | 条件 |
|------|-------|------|
| **テスト計画作成効率** | 30%削減 | planner時間削減≥30% |
| **全体生産性向上** | 97%削減 | planner統合成功時 |

---

## 🛠️ 運用ガイドライン

### E2Eテスト作成時の原則

#### 1. e2e-test Agent活用（現行標準）

**呼び出しパターン**:
```
e2e-test Agent, create E2E tests for [feature name]

実施内容:
- [機能概要]
- [テストシナリオ概要]
```

**SubAgent責務**:
- Blazor Server E2Eテスト実装
- Playwright MCP 21ツール活用
- data-testid属性設計パターン適用
- Blazor Server SignalR対応パターン適用

#### 2. Playwright MCPツール優先活用

**主要ツール**:
- `playwright_navigate`: ページ遷移
- `playwright_click`: 要素クリック
- `playwright_fill`: フォーム入力
- `playwright_screenshot`: スクリーンショット取得
- `playwright_evaluate`: JavaScript実行
- `playwright_console`: コンソールログ確認

**活用パターン**:
1. リアルタイム検証（即座フィードバック）
2. デバッグ効率化（スクリーンショット・コンソールログ）
3. アクセシビリティツリー活用（セレクタ最適化）

#### 3. data-testid属性設計パターン適用

**命名規則**:
- ボタン: `{action}-button`（例: `member-add-button`）
- 入力: `{field}-input`（例: `username-input`）
- リスト: `{entity}-list`（例: `member-list`）

**参照**: `.claude/skills/playwright-e2e-patterns/patterns/data-testid-design.md`

#### 4. Blazor Server SignalR対応パターン適用

**主要パターン**:
```csharp
// StateHasChanged()待機
await page.WaitForTimeoutAsync(1000);

// SignalR接続確立
await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

// Toast通知検証
var toast = page.Locator(".toast-success");
await Expect(toast).ToBeVisibleAsync();

// confirmダイアログ処理
page.Dialog += (_, dialog) => dialog.AcceptAsync();
```

**参照**: `.claude/skills/playwright-e2e-patterns/patterns/blazor-signalr-e2e.md`

### E2Eテスト実行時の原則

#### 自動実行スクリプト活用（推奨）

**一括実行**:
```bash
# DevContainer内で実行（VS Code統合ターミナル）
bash tests/run-e2e-tests.sh

# 特定テストクラスのみ実行
bash tests/run-e2e-tests.sh AuthenticationTests
```

**ホスト環境から実行**（Claude Code用）:
```bash
# Docker経由でDevContainer内実行
docker exec ubiquitous-lang-mng_devcontainer-devcontainer-1 bash tests/run-e2e-tests.sh
```

**効率化効果**: 83-93%削減（手動3-5分 → 自動30秒）

**参照**: `CLAUDE.md` - E2Eテスト自動実行セクション

### E2Eテスト失敗時の対応（Phase B3以降）

#### 現行対応（Phase B-F2）

1. エラーログ確認
2. e2e-test Agent Fix-Mode活用
3. 手動デバッグ・修正
4. 失敗パターン記録

#### 将来対応（Phase B3: healer統合後）

1. Playwright Agents自動修復確認（healer起動・修復試行）
2. 修復成功確認（3試行以内で成功）
3. 修復失敗時の手動介入（e2e-test SubAgent Fix-Mode活用）
4. 失敗パターン分析・記録

---

## ⚠️ リスク管理

### 技術的リスク

| リスク | 発生確率 | 影響度 | 対策 |
|--------|---------|--------|------|
| healer AgentがC# Playwright非対応 | 中（50%） | 中 | Phase B3実用評価で早期検証 |
| planner AgentのMarkdown計画品質不足 | 中（40%） | 低 | 手動計画との統合・補完 |
| Playwright Test Agents MCP更新でツール変更 | 低（20%） | 低 | 週次振り返りでバージョン確認 |

### 運用リスク

| リスク | 発生確率 | 影響度 | 対策 |
|--------|---------|--------|------|
| e2e-test Agent + healer併用による複雑性増加 | 中（50%） | 低 | 明確な使い分け基準策定 |
| Phase B3評価作業の工数超過 | 中（40%） | 低 | 評価Stepを独立設定・時間バッファ確保 |
| Playwright公式Agents非推奨化 | 低（10%） | 中 | 公式ロードマップ定期確認 |

---

## 📚 関連ドキュメント

### 本プロジェクト内

#### 現行参照ドキュメント

- **ADR_021**: Playwright MCP + Agents統合戦略（Phase B2決定）
- **ADR_020**: テストアーキテクチャ決定
- **Agent Skills**: `.claude/skills/playwright-e2e-patterns/`
  - data-testid設計パターン
  - Playwright MCPツール活用パターン
  - Blazor Server SignalR対応パターン
- **Tech Research**: `Doc/08_Organization/Active/Phase_B-F2/Research/Tech_Research_Playwright_Test_Agent_2025-11.md`
- **Phase Summary**: `Doc/08_Organization/Active/Phase_B-F2/Phase_Summary.md`
- **CLAUDE.md**: E2Eテスト自動実行セクション

#### アーカイブドキュメント

- `Doc/08_Organization/Rules/backup/Phase_B2_Playwright_Agents_導入計画.md`
  - Phase B2向け導入計画（実行前・2025-10-11策定）
  - 歴史的記録として保持
- `Doc/08_Organization/Rules/backup/Phase_B2_Playwright_統合戦略.md`
  - MCP + Agents統合戦略（実行前・2025-10-11策定）
  - 歴史的記録として保持

#### GitHub Issues

- **Issue #46**: Playwright統合後のCommands/SubAgent刷新
  - 段階的移行計画確立（2025-11-18更新）
  - Phase B3/B4評価計画記録

### 外部リソース

#### Playwright公式

- [Playwright Test Agents公式](https://playwright.dev/docs/test-agents)
- [Playwright MCP Server (GitHub)](https://github.com/microsoft/playwright-mcp)
- [Playwright for .NET](https://playwright.dev/dotnet/)

#### コミュニティ・事例

- [Shipyard: Playwright Agents with Claude Code](https://shipyard.build/blog/playwright-agents-claude-code/)
- [Medium: Complete Guide to Playwright Agents](https://medium.com/@ismailsobhy/ai-powered-test-automation-part-4-complete-guide-to-playwright-agents-planner-generator-healer-d418166afe34)
- [Ministry of Testing: Self-Healing Tests](https://www.ministryoftesting.com/articles/creating-self-healing-automated-tests-with-ai-and-playwright)

---

## 🔄 継続改善計画

### Phase B3（2025-12予定）

**healer Agent実用評価・統合**（評価成功時）:
- E2Eテスト失敗時の自動修復フロー確立
- 修復パターン分析・最適化
- 組織管理運用マニュアル更新
- step-end-review/weekly-retrospective Commands更新

### Phase B4（2026-01予定）

**planner Agent実用評価・統合**（評価成功時）:
- テスト計画作成の自動化・効率化
- Markdown計画と手動計画の統合パターン確立
- playwright-e2e-patterns Skill拡張
- step-start Command更新

### Phase B5以降

**コミュニティ貢献検討**:
- .NET + Blazor Server + Playwright MCP + Agents実績の記事執筆
- GitHub Issues/Discussionsへのフィードバック
- コミュニティイベントでの発表
- ベストプラクティスの共有

---

## 📝 更新履歴

| 日付 | 更新内容 | 更新者 |
|------|---------|--------|
| 2025-11-17 | 初版作成（統合ドキュメント）・Rulesの2ファイルをアーカイブ | Claude Code |

---

**策定者**: Claude Code
**承認**: プロジェクトオーナー
**次回更新予定**: Phase B3（healer Agent実用評価完了時）
