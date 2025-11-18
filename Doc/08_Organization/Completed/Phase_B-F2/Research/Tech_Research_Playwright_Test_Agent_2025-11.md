# Playwright Test Agent 技術調査報告

**調査日**: 2025-11-15  
**調査者**: Claude Code (tech-research Agent)  
**Phase**: Phase B-F2 Step6（Phase A機能E2Eテスト実装）

---

## エグゼクティブサマリー

### 主要な発見
- ✅ **Playwright Test Agent = 3つのClaude Code Subagents**（Planner/Generator/Healer）
- ✅ **本プロジェクトではPhase B2で既に統合完了**（ADR_021・playwright-e2e-patterns Skill）
- ✅ **93.3%効率化実証済み**（150分 → 10分/機能）
- ⚠️ **新たな導入作業は不要**（既存基盤活用のみ）

### 推奨事項
1. **新規導入は不要** - 既存のPlaywright MCP + Agent Skillsパターンを継続活用
2. **Phase B-F2 Step6では既存基盤を活用** - playwright-e2e-patterns Skillを参照
3. **Healer Agent実用評価は今後実施** - Phase B3以降で効果測定

---

## 1. 概要

### Playwright Test Agentとは

**Playwright Test Agent**は、Playwright v1.56（2025-10-10リリース）で導入された**3つのAI駆動Agentの総称**です。

| Agent名 | 役割 | 出力 |
|---------|------|------|
| **🎭 Planner** | アプリ探索・テスト計画作成 | Markdown形式テスト計画（specs/） |
| **🎭 Generator** | テスト計画→コード生成 | 実行可能Playwrightテスト（tests/） |
| **🎭 Healer** | テスト失敗時の自動修復 | セレクタ更新・待機戦略調整 |

### 技術成熟度
- **Playwright v1.56**: 2025-10-10リリース（安定版）
- **VS Code Subagents**: v1.105対応（Insiders不要）
- **実用段階**: 本プロジェクトPhase B2で実証済み

---

## 2. Playwright MCP Serverとの違い

### レイヤー構造

```
┌─────────────────────────────────────┐
│     Claude Code (AI/LLM Layer)      │
├─────────────────────────────────────┤
│  Playwright Test Agents (AI専門家)  │
│  🎭 Planner 🎭 Generator 🎭 Healer  │
├─────────────────────────────────────┤
│  Playwright MCP Server (基盤)       │
│  25種類ブラウザ操作ツール            │
└─────────────────────────────────────┘
```

### 比較表

| 項目 | Playwright MCP Server | Playwright Test Agents |
|------|----------------------|------------------------|
| **役割** | エンジン（基盤） | ドライバー（専門AI） |
| **機能** | 25種類ブラウザ操作ツール | テスト計画・生成・修復 |
| **独立性** | 単独動作可能 | MCP Server必須 |
| **統合** | MCP設定ファイル | `npx playwright init-agents` |

**相互関係**: 両者は共存・相互補完（Test AgentsはMCP Serverの機能を活用）

---

## 3. 導入手順

### 前提条件
- Node.js v18以上
- Playwright v1.56以上
- VS Code v1.105以上
- Claude Code最新版

### インストール

#### Step 1: Playwright MCP Server設定

```bash
claude mcp add playwright npx @playwright/mcp@latest
```

#### Step 2: Playwright Test Agents初期化

```bash
npx playwright init-agents --loop=claude
```

生成されるファイル構成（**Playwright v1.56仕様**）：
```
.claude/agents/          # Agent定義（⚠️ 仕様変更: .github/chatmodes/ → .claude/agents/）
  ├── 🎭 playwright-test-planner.md
  ├── 🎭 playwright-test-generator.md
  └── 🎭 playwright-test-healer.md
.mcp.json                # MCP Server設定（playwright-test）
seed.spec.ts             # 初期環境構築（Seed Test）
playwright.config.ts     # Playwright設定
```

**🔴 重要: 配置に関する注意**:
- **Playwright v1.56仕様変更**: 古い仕様（`.github/chatmodes/`）から新仕様（`.claude/agents/`）に変更
- **Claude Code検索パス**: プロジェクトルートの`.claude/agents/`のみ認識（サブディレクトリ非対応）
- **推奨配置**: `npx playwright init-agents`をプロジェクトルートで実行、または生成後にプロジェクトルートの`.claude/agents/`へ移動

#### Step 3: Seed Test作成

```typescript
// tests/seed.spec.ts - テスト環境初期状態定義
test('seed', async ({ page }) => {
  await page.goto('https://localhost:5001');
  await page.fill('[data-testid="username-input"]', 'admin@example.com');
  await page.fill('[data-testid="password-input"]', 'Admin123!');
  await page.click('[data-testid="login-button"]');
});
```

---

## 4. Claude Code統合方法

### Agent呼び出し

```
🎭 planner, explore the user management feature and create a test plan.
🎭 generator, generate Playwright tests from specs/user-management.md.
🎭 healer, fix the failing login test.
```

### 既存Playwright MCP Serverとの共存

**結論**: **完全共存可能**（異なるレイヤーで動作・干渉なし）

- **MCP Server**: システムレベル（IDE/MCP設定）
- **Test Agents**: プロジェクトレベル（.github/chatmodes/）

---

## 5. E2Eテスト活用方法

### テストコード生成フロー

```
1. Planner → specs/login.md（テスト計画）
2. Generator → tests/login.spec.ts（実行可能テスト）
3. Test実行 → npx playwright test
4. Healer → 失敗時の自動修復
```

### data-testid属性の自動検出

Playwright MCP Serverの`playwright_snapshot`ツールは、以下の優先順位でセレクタ検出：

1. **data-testid属性**（最優先）
2. aria-label属性
3. role属性
4. テキストコンテンツ
5. CSS/XPathセレクタ（最終手段）

**本プロジェクト**: Phase B2でdata-testid設計パターン確立済み

| UI要素 | 命名規則 | 例 |
|--------|----------|-----|
| ボタン | `{action}-button` | `member-add-button` |
| 入力 | `{field}-input` | `username-input` |
| リスト | `{entity}-list` | `member-list` |

**参照**: `.claude/skills/playwright-e2e-patterns/patterns/data-testid-design.md`

### Blazor Server / SignalR対応

Phase B2確立済みパターン：

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

---

## 6. 制約・注意点

### Windows DevContainer環境

#### Display/GUI制約
- **問題**: "Unable to open X display"エラー
- **対策**:
  1. VcXsrv使用（X転送）
  2. **Headlessモード**（推奨・本プロジェクト採用）
  3. ホスト環境でCodegen実行

#### 本プロジェクト対応
- Sandboxモード（DevContainer）でHeadless実行
- Codegen: ホスト環境実行
- **問題なし**: E2Eテスト正常動作

### パフォーマンス

#### テスト生成速度（Phase B2実測）
- **Playwright MCP**: 約10分/機能（3シナリオ）
- **従来手法**: 150-180分/機能
- **削減率**: **93.3%**

#### Healer自動修復（期待値・未実証）
- 成功率: 80-85%
- 修復時間: 1-3分/失敗テスト
- **Phase B3で効果測定予定**

### セキュリティ

```bash
# .gitignore設定（Phase B2完了）
.env.test
playwright/.auth/
test-results/
playwright-report/
```

- テスト専用PostgreSQLコンテナ使用
- テスト専用ユーザー: `e2e-test@ubiquitous-lang.local`

---

## 7. 推奨事項

### Phase B-F2 Step6（現在）

| 推奨度 | アクション | 理由 |
|--------|-----------|------|
| ✅ 推奨 | 既存Playwright MCP + Agent Skillsパターン継続 | 93.3%効率化実証済み |
| ✅ 推奨 | playwright-e2e-patterns Skill参照 | パターン確立済み |
| ⚠️ 保留 | Test Agents（Planner/Generator/Healer）統合 | .github/chatmodes/生成済み・実用評価は今後 |
| ❌ 不要 | 新規MCP Server設定 | ADR_021完了済み |

### Phase B3以降（将来）

| アクション | 時期 |
|-----------|------|
| Healer Agent実用評価 | Phase B3（UI変更時自動修復測定） |
| Planner/Generator実用評価 | Phase B4（自動生成測定） |
| Agent Skills拡張 | GitHub Issue #56完全解決時 |

---

## 8. 本プロジェクト既存実装

### Phase B2完了済み

#### ADR_021: Playwright MCP + Agents統合戦略
- Status: Accepted（2025-10-26）
- 実証: 93.3%効率化達成

#### Agent Skills: playwright-e2e-patterns
- Status: Phase 1完了（2025-10-26）
- 内容: 3つのE2Eパターン
  1. data-testid属性設計
  2. Playwright MCPツール活用
  3. Blazor Server SignalR対応

#### E2Eテスト実装
- プロジェクト: `tests/UbiquitousLanguageManager.E2E.Tests/`
- テスト: `UserProjectsTests.cs`（3シナリオ実装済み）

### 未実装

- Healer Agent実用評価（Phase B3予定）
- Planner/Generator実用評価（Phase B4予定）

---

## 9. 参考資料

### 本プロジェクト内
- ADR_021: Playwright MCP + Agents統合戦略
- ADR_020: テストアーキテクチャ決定
- Agent Skills: `.claude/skills/playwright-e2e-patterns/`
- Phase B2 Step6: Playwright E2E実装
- Phase B-F1評価レポート: Playwright MCP/Agents評価

### 外部リソース
- [Playwright Test Agents公式](https://playwright.dev/docs/test-agents)
- [Playwright MCP Server (GitHub)](https://github.com/microsoft/playwright-mcp)
- [Shipyard: Playwright Agents with Claude Code](https://shipyard.build/blog/playwright-agents-claude-code/)
- [Medium: Complete Guide to Playwright Agents](https://medium.com/@ismailsobhy/ai-powered-test-automation-part-4-complete-guide-to-playwright-agents-planner-generator-healer-d418166afe34)

---

## 10. 結論

### 技術的評価

| 項目 | 評価 | 備考 |
|------|------|------|
| 技術成熟度 | ✅ 実用段階 | Playwright v1.56安定版 |
| Claude Code統合 | ✅ 完璧 | MCP + Subagents完了 |
| 本プロジェクト適合性 | ✅ 最適 | Blazor Server・F#+C#に最適 |
| 効率化実証 | ✅ 達成（93.3%） | Phase B2実証完了 |
| セキュリティ | ✅ 安全 | .gitignore設定・専用アカウント |

### 最終推奨

#### Phase B-F2 Step6（現在）
1. **新規導入は不要** - Playwright MCP Server統合済み
2. **既存基盤活用** - playwright-e2e-patterns Skill参照
3. **E2Eテスト作成** - Playwright MCP直接活用継続

#### Phase B3以降（将来）
1. Healer Agent実用評価（UI変更自動修復測定）
2. Planner/Generator実用評価（自動生成測定）
3. Agent Skills拡張（bUnit代替パターン追加）

### 期待効果

| 効果 | Phase B2実証値 | Phase B3期待値 |
|------|----------------|---------------|
| E2Eテスト作成効率 | 93.3%削減 | 95%削減 |
| テストメンテナンス効率 | - | 50-70%削減 |
| テストカバレッジ | 3シナリオ | 3-5倍拡大 |

---

**調査完了日**: 2025-11-15  
**次回評価予定**: Phase B3（Healer Agent実用評価）  
**技術負債**: なし（既存基盤で十分対応可能）
