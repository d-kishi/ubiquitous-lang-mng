# ADR_024: E2E専用SubAgent新設決定

**ステータス**: Accepted
**策定日**: 2025-11-02
**決定者**: プロジェクトオーナー
**関連ADR**: ADR_020（テストアーキテクチャ決定）、ADR_018（SubAgent指示改善とFix-Mode活用）

---

## 📋 決定事項

**E2E専用SubAgent（e2e-test Agent）を新設する**

### SubAgent構成変更

**変更前**: 13種類のSubAgent構成
- 品質保証系（4Agent）: unit-test, integration-test, code-review, spec-compliance

**変更後**: 14種類のSubAgent構成
- 品質保証系（5Agent）: unit-test, **integration-test（責務再定義）**, **e2e-test（新設）**, code-review, spec-compliance

### 責務境界定義

#### e2e-test Agent（新設）
- **責務**: Playwright E2Eテスト実装・UIインタラクション・エンドツーエンドシナリオテスト
- **実行範囲**: `tests/E2E.Tests/` 専任
- **使用Skill**: playwright-e2e-patterns Skill（93.3%効率化パターン）
- **使用ツール**: Playwright MCP（21ツール）
- **禁止範囲**: `src/` 配下の実装コード修正・`tests/Infrastructure.Integration.Tests/` 配下の実装

#### integration-test Agent（責務再定義）
- **責務**: WebApplicationFactory統合テスト・データベース統合テスト
- **実行範囲**: `tests/Infrastructure.Integration.Tests/` 専任
- **使用ツール**: Testcontainers.PostgreSql + WebApplicationFactory
- **禁止範囲**: `src/` 配下の実装コード修正・`tests/E2E.Tests/` 配下の実装（e2e-test Agent責務）

---

## 🎯 決定の背景

### Phase B2実績

Phase B2にて以下の成果を達成：
- **Playwright MCP + Agents統合完了**（統合推奨度 10/10点）
- **93.3%効率化実証**（playwright-e2e-patterns Skill確立）
- 3つのE2Eテストパターン確立（data-testid/MCP/SignalR）

しかし、Playwright実装責任の明確化が不十分であり、以下の課題が存在：
- integration-test AgentとE2E Testの責務境界が不明確
- ADR_020設計思想（レイヤー別×テストタイプ別分離方式）との整合性未確保

### 現状課題

Phase B-F2 Step2完了時点の課題：
1. **SubAgent責務の不明確さ**: integration-test Agentが「WebApplicationFactory・E2Eテスト・データベース統合テスト」を担当しているが、E2EとIntegrationは本質的に異なるレイヤー
2. **ADR_020との不整合**: ADR_020は「レイヤー別×テストタイプ別分離方式」を採用しており、Integration TestsとE2E Testsを明確に分離する設計思想
3. **技術スタックの混在**: Testcontainers（Integration）とPlaywright MCP（E2E）が同一Agent責務内に混在

---

## 💡 判断根拠（5点）

### ① ADR_020設計思想との整合性

ADR_020では「**レイヤー別×テストタイプ別分離方式**」を採用：

```
tests/
├── UbiquitousLanguageManager.Infrastructure.Integration.Tests/  ← Integration Test
└── UbiquitousLanguageManager.E2E.Tests/                       ← E2E Test
```

**プロジェクト名**: `{ProjectName}.{Layer}.{TestType}.Tests`
- **Layer**: Domain / Application / Contracts / Infrastructure / Web
- **TestType**: Unit / Integration / UI / **E2E**

**重要**: ADR_020では**E2E**を独立した**TestType**として扱っており、Integration TestsとE2E Testsを明確に分離する設計思想が示されている。

SubAgent設計もこの分離原則に従うべきである。

### ② Integration/E2Eのレイヤー分離

テストアーキテクチャ設計書に基づく3つの本質的な違い：

| 項目 | Integration Test | E2E Test |
|-----|-----------------|---------|
| **実行時間** | 1-10秒/test | 10-60秒/test |
| **使用フレームワーク** | Testcontainers | **Playwright** |
| **テスト対象** | バックエンド統合 | **UI/ブラウザインタラクション** |
| **レイヤー特性** | Infrastructure層 | **横断的テスト（特定レイヤーに属さない）** |

E2E.Testsの命名理由（テストアーキテクチャ設計書より）：
> 「E2Eテストは全層をまたがるため、特定のLayer名を付与しない」
> 「エンドツーエンドシナリオテストは特定レイヤーに属さない横断的なテスト」

### ③ 技術スタックの違い

**Integration Test**:
- xUnit + WebApplicationFactory + Testcontainers.PostgreSql
- バックエンドAPI・DB統合確認
- ブラウザ不要・高速実行

**E2E Test**:
- xUnit + Microsoft.Playwright（1.48.0）
- **Playwright MCP（21ツール）**
- ブラウザ自動化・UIインタラクション・エンドツーエンドシナリオ
- SignalR接続・Toast通知・JavaScript confirmダイアログ処理

**重要**: E2E TestはPlaywright MCPツールに全面的に依存しており、Integration Testとは完全に異なる技術スタックを使用する。

### ④ playwright-e2e-patterns Skill参照の必要性

Phase B2で確立した3つのE2Eテストパターン（93.3%効率化実証済み）：

1. **data-testid属性設計パターン**
   - ボタン: `{action}-button`
   - 入力: `{field}-input`
   - リスト: `{entity}-list`
   - E2Eテスト専用のセレクタ設計

2. **Playwright MCPツール活用パターン**
   - playwright_navigate, playwright_snapshot, playwright_click等
   - アクセシビリティツリー取得
   - E2Eテスト特有の技術スタック

3. **Blazor Server SignalR対応パターン**
   - StateHasChanged()待機: `await page.WaitForTimeoutAsync(1000)`
   - SignalR接続確立確認: `LoadState.NetworkIdle`
   - Toast通知検証: `.toast-success`, `[role='alert']`

**重要**: これらのパターンは**E2Eテスト特有**であり、Integration Testでは使用されない。

### ⑤ Playwright MCP連携の必要性

**Playwright MCP 21ツール**（主要ツールのみ抜粋）:
- `playwright_navigate` - URL遷移・ページ読み込み
- `playwright_snapshot` - アクセシビリティツリー取得
- `playwright_click` - ボタン・リンククリック
- `playwright_fill` - フォーム入力
- `playwright_select` - ドロップダウン選択
- `playwright_wait_for` - 要素表示待機・SignalR更新待機
- `playwright_take_screenshot` - スクリーンショット取得
- `playwright_evaluate` - JavaScript評価
- `playwright_handle_dialog` - JavaScript confirmダイアログ処理
- 他16ツール

**E2E Test実装での活用方法**:
```csharp
// 1. playwright_navigate でURL遷移
await page.GotoAsync("https://localhost:5001/projects/123");

// 2. playwright_snapshot でアクセシビリティツリー取得（構造化データ・高速）
var snapshot = await page.Accessibility.SnapshotAsync();

// 3. playwright_click でボタンクリック
await page.ClickAsync("[data-testid='member-add-button']");

// 4. playwright_fill でフォーム入力
await page.FillAsync("[data-testid='username-input']", "testuser");

// 5. playwright_wait_for でSignalR更新待機
await page.WaitForTimeoutAsync(1000); // StateHasChanged()待機
```

**重要**: これらのPlaywright MCPツールはE2Eテスト専用であり、Integration Testでは使用されない。

---

## ❌ 代替案不採用

### integration-test Agent拡張（不採用）

**提案内容**: integration-test Agentの定義説明を拡張し、E2E責務を明記する

**不採用理由**:
1. **ADR_020設計思想と矛盾**: レイヤー別×テストタイプ別分離方式に反する
2. **責務混在**: Integration Test（Infrastructure層）とE2E Test（横断的）の責務が混在
3. **技術スタック混在**: Testcontainers（Integration）とPlaywright MCP（E2E）が同一Agent責務内に混在
4. **SubAgent責務境界不明確化**: どちらのテストタイプを担当すべきか判断が曖昧になる

---

## 📊 影響範囲

### 更新が必要なファイル

1. **`.claude/skills/subagent-patterns/SKILL.md`**
   - SubAgentプール定義更新（13種類 → 14種類）
   - e2e-test Agent新設定義追加
   - integration-test Agent責務再定義

2. **`.claude/commands/phase-start.md`**
   - e2e-test Agent追加

3. **`.claude/commands/step-start.md`**
   - e2e-test Agent追加

4. **`.claude/commands/subagent-selection.md`**
   - e2e-test Agent選択ロジック追加

5. **`Doc/08_Organization/Rules/組織管理運用マニュアル.md`**
   - e2e-test Agent運用ガイドライン追加
   - Playwright MCP連携手順追加

---

## 🔧 MCPツール更新時のメンテナンス手順

### 目的

Playwright MCPサーバーが更新された際に、`.claude/agents/e2e-test.md`のtools定義を適切にメンテナンスし、SubAgent機能を最新状態に保つ。

### メンテナンストリガー

以下のタイミングでメンテナンスを実施：
1. **週次振り返り時の自動チェック**（推奨）
   - 週次振り返りコマンド実行時に自動でMCP更新を確認
   - 新規バージョン・ツール変更があればユーザーにレポート

2. **手動チェック**
   - Playwright MCPの新規リリースを検知した際
   - E2Eテスト実行時にツールエラーが発生した際

### メンテナンス手順

#### 1. Playwright MCP最新版確認

```bash
# 現在のバージョン確認
npx @playwright/mcp@latest --version

# npm最新版確認
npm view @playwright/mcp version

# GitHub最新リリース確認
gh api repos/microsoft/playwright-mcp/releases/latest
```

#### 2. 利用可能なツール一覧取得

```bash
# JSON-RPC経由でツール一覧取得
echo '{"jsonrpc": "2.0", "id": 1, "method": "tools/list"}' \
  | npx @playwright/mcp@latest \
  | jq '.result.tools[].name'
```

**期待される出力**（21ツール）:
```
browser_close
browser_resize
browser_console_messages
browser_handle_dialog
browser_evaluate
browser_file_upload
browser_fill_form
browser_install
browser_press_key
browser_type
browser_navigate
browser_navigate_back
browser_network_requests
browser_take_screenshot
browser_snapshot
browser_click
browser_drag
browser_hover
browser_select_option
browser_tabs
browser_wait_for
```

#### 3. e2e-test Agent定義との差分確認

`.claude/agents/e2e-test.md`のtools行と上記ツール一覧を比較：

```bash
# 現在のtools行から Playwright MCP ツールのみ抽出
grep "^tools:" .claude/agents/e2e-test.md \
  | grep -o 'mcp__playwright__[a-z_]*' \
  | sort

# JSON-RPC結果と比較
comm -3 <(上記抽出結果) <(JSON-RPC結果)
```

#### 4. ツール追加・削除の判断

**新規ツール追加時**:
1. リリースノートでツールの用途・影響を確認
2. `.claude/agents/e2e-test.md`のtools行に追加
3. 必要に応じてAgent定義本文にツール説明を追記

**ツール廃止・削除時**:
1. リリースノートで代替手段を確認
2. 既存E2Eテストでの使用有無を確認（Grep検索）
3. 影響があれば移行対応を実施
4. `.claude/agents/e2e-test.md`のtools行から削除

**ツール名変更時**:
1. リリースノートで変更理由・移行方法を確認
2. 既存E2Eテストでの使用箇所を全て更新
3. `.claude/agents/e2e-test.md`のtools行を更新

#### 5. 更新後の検証

```bash
# e2e-test Agent定義のYAML frontmatter検証
head -n 5 .claude/agents/e2e-test.md

# tools行の書式確認（カンマ区切り・余分な空白なし）
grep "^tools:" .claude/agents/e2e-test.md
```

### 週次振り返り時の自動レポート

週次振り返りコマンド（`.claude/commands/weekly-retrospective.md`）の「### 12. MCP更新確認」セクションにて以下を自動実行：

1. Playwright MCP / Serena MCPの最新バージョン確認
2. 直近1週間のリリース取得（GitHub API）
3. ツール変更レポート生成（新規追加・廃止・非推奨）
4. ユーザーへのレポート提示・更新判断支援

**レポート例**:
```markdown
## MCP更新レポート（2025-11-09）

### Playwright MCP
- **現在のバージョン**: v0.0.45
- **最新バージョン**: v0.0.46（更新あり）
- **直近1週間の変更**:
  - 新規ツール追加: `browser_context_menu`（右クリックメニュー操作）
  - 廃止ツール: なし
  - 非推奨ツール: `browser_goto`（`browser_navigate`を推奨）

### 推奨アクション
- ⚠️ バージョン更新推奨: v0.0.45 → v0.0.46
- ✅ `.claude/agents/e2e-test.md`にツール追加推奨: `browser_context_menu`
```

### リリースノート確認方法

```bash
# Playwright MCP最新リリースの詳細取得
gh api repos/microsoft/playwright-mcp/releases/latest \
  | jq '{tag_name, created_at, body}'

# 直近5リリースの一覧
gh api repos/microsoft/playwright-mcp/releases?per_page=5 \
  | jq '.[] | {tag_name, created_at, name}'
```

### メンテナンス履歴記録

週次総括文書（`Doc/04_Daily/YYYY-MM/週次総括_YYYY-WXX.md`）に以下を記録：

```markdown
## MCP更新履歴

### YYYY-MM-DD: Playwright MCP v0.0.45 → v0.0.46
- **変更内容**: `browser_context_menu`ツール追加
- **影響範囲**: `.claude/agents/e2e-test.md`のtools行更新
- **対応者**: ユーザー（手動更新）
- **対応時間**: 5分
```

### トラブルシューティング

**問題1: JSON-RPCツール一覧取得に失敗**
```bash
# 原因: MCP サーバー起動失敗
# 対処: MCP接続状態確認
claude mcp list
claude mcp get playwright

# 再起動試行
claude mcp remove playwright
claude mcp add playwright npx @playwright/mcp@latest
```

**問題2: ツール数が想定と異なる**
```bash
# 原因: バージョンミスマッチ
# 対処: 明示的に最新版を指定
npx @playwright/mcp@latest --version
npm cache clean --force
npx @playwright/mcp@latest
```

**問題3: E2Eテスト実行時にツールエラー**
```bash
# 原因: 廃止されたツールを使用中
# 対処: エラーメッセージからツール名を特定
grep -r "mcp__playwright__{tool_name}" tests/E2E.Tests/
# 代替ツールへ移行（リリースノート参照）
```

---

## 🔗 関連情報

### 詳細実装パターン

E2E Test実装の詳細パターンは以下を参照：
- **`.claude/skills/playwright-e2e-patterns/SKILL.md`** - 3つのE2Eテストパターン詳細
- **`Doc/02_Design/テストアーキテクチャ設計書.md`** - Integration/E2E Testのレイヤー分離設計

### 関連ADR

- **ADR_020**: テストアーキテクチャ決定（レイヤー別×テストタイプ別分離方式）
- **ADR_018**: SubAgent指示改善とFix-Mode活用（SubAgent責務分担原則）

---

## ✅ 期待される効果

### 短期効果（Phase B-F2以降）

1. **SubAgent責務境界の明確化**: E2E TestとIntegration Testの責務が明確に分離
2. **ADR_020設計思想との整合性確保**: レイヤー別×テストタイプ別分離方式の徹底
3. **技術スタック管理の効率化**: Testcontainers（Integration）とPlaywright MCP（E2E）が独立管理
4. **playwright-e2e-patterns Skill適用範囲明確化**: e2e-test Agent専用Skillとして明確化

### 中長期効果（Phase B3以降）

1. **E2Eテスト拡充時の作業効率向上**: e2e-test Agent専任により、Playwright実装効率20-30%向上
2. **SubAgent選択精度向上**: E2E Test実装時、e2e-test Agent選択が自明
3. **保守性向上**: 責務分離により、SubAgent定義のメンテナンスコスト削減

---

**策定者**: Claude Code
**承認者**: プロジェクトオーナー
**最終更新**: 2025-11-02
