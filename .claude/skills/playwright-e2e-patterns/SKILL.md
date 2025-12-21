---
name: playwright-e2e-patterns
description: TypeScript/Playwright TestでE2Eテストを作成する。「E2Eテスト実装」「data-testid設計」「Blazor Server対応」「SignalR待機」「ログインフロー」「ローディング状態テスト」「CDP Network Throttling」の際に使用する。
allowed-tools: Read, Grep
---

# Playwright E2E Test Patterns

TypeScript/Playwright Test + Generator/Healer Agents活用によるE2Eテスト作成パターン。

**Phase B2-F2移行完了**: C# E2E削除・TypeScript移行完了・**93.3%効率化達成**

## 使用タイミング

1. **E2Eテスト実装時** - 新規E2Eテストコード・Blazor Serverコンポーネントテスト
2. **data-testid属性設計時** - UI要素へのdata-testid属性付与
3. **Blazor Server SignalR対応時** - StateHasChanged待機・SignalR接続確認
4. **Playwright MCPツール選択時** - navigate/snapshot/click/fill等の使い分け

---

## 3つのE2Eテストパターン

### 1. data-testid属性設計パターン

**詳細**: [`references/data-testid-design.md`](./references/data-testid-design.md)

**命名規則**: [`.claude/rules/tests/e2e-test-data-testid-naming.md`](../../../rules/tests/e2e-test-data-testid-naming.md)

| 要素タイプ | 命名パターン | 例 |
|-----------|-------------|-----|
| ボタン | `{action}-button` | `member-add-button` |
| 入力 | `{field}-input` | `username-input` |
| リスト | `{entity}-list` | `member-list` |
| カード | `{entity}-card` | `member-card` |
| エラー | `{context}-error-message` | `member-error-message` |

---

### 2. Playwright MCPツール活用パターン

**詳細**: [`references/mcp-tools-usage.md`](./references/mcp-tools-usage.md)

| ツール | 用途 |
|--------|------|
| `playwright_navigate` | URL遷移・ページ読み込み |
| `playwright_snapshot` | アクセシビリティツリー取得（構造化・高速） |
| `playwright_click` | ボタン・リンククリック |
| `playwright_fill` | フォーム入力 |
| `playwright_wait_for` | 要素表示待機・時間待機 |

---

### 3. Blazor Server SignalR対応パターン（7パターン）

**詳細**: [`references/blazor-signalr-e2e.md`](./references/blazor-signalr-e2e.md)

| パターン | 用途 |
|---------|------|
| 1. SignalR接続確立待機 | `WaitForLoadStateAsync(NetworkIdle)` |
| 2. StateHasChanged()待機 | 非同期UI更新待機（1000ms） |
| 3. Toast通知検証 | `.toast-success`, `[role='alert']` |
| 4. confirmダイアログ処理 | `page.Dialog`イベント登録 |
| 5. URL遷移確認 | `WaitForURLAsync` |
| 6. 要素表示待機 | `WaitForSelectorState.Visible` |
| 7. CDP Network Throttling | ローディング状態テスト（SignalR対応）🆕 |

---

## 参照ファイル

| ファイル | 内容 |
|---------|------|
| [`data-testid-design.md`](./references/data-testid-design.md) | data-testid命名規則・実装例 |
| [`mcp-tools-usage.md`](./references/mcp-tools-usage.md) | MCPツール使い分け・実践例 |
| [`blazor-signalr-e2e.md`](./references/blazor-signalr-e2e.md) | SignalR対応7パターン詳細 |
| [`e2e-implementation-guide.md`](./references/e2e-implementation-guide.md) | 実証結果・横展開・次ステップ |

## 関連Agents

- **playwright-test-planner**: テスト計画作成
- **playwright-test-generator**: テストコード自動生成
- **playwright-test-healer**: テスト修復
- **e2e-test**: E2Eテスト実行統合

---

**Skill作成日**: 2025-10-26
**最終更新**: 2025-12-15（パターン7追加）
**実証結果**: 93.3%効率化達成
