---
name: playwright-e2e-patterns
description: TypeScript/Playwright TestでE2Eテストを作成する。「E2Eテスト実装」「data-testid設計」「Blazor Server対応」「SignalR待機」「ログインフロー」「ローディング状態テスト」「CDP Network Throttling」の際に使用する。
allowed-tools: Read, Grep
---

# Playwright E2E Test Patterns

## 概要

このSkillは、TypeScript/Playwright Test + Generator/Healer Agents活用によるE2Eテスト作成パターンを自律的に適用します。

**Phase B2-F2移行完了**:
- ✅ C# E2Eテストプロジェクト削除完了
- ✅ TypeScript/Playwright Test移行完了
- ✅ Playwright Test Generator Agent統合（60-70%時間削減）
- ✅ Playwright Test Healer Agent統合（50-70%成功率）
- ✅ Phase B2で確立した**93.3%効率化パターン**継続適用

## 使用タイミング

Claudeは以下の状況でこのSkillを自律的に使用すべきです：

1. **E2Eテスト実装時**
   - 新規E2Eテストコード作成時
   - Blazor Serverコンポーネントのテスト作成時
   - UserProjects/ProjectMembers等の機能テスト作成時

2. **data-testid属性設計時**
   - Blazor Serverコンポーネント実装時
   - UI要素への data-testid 属性付与時
   - E2Eテスト準備時

3. **Blazor Server SignalR対応時**
   - StateHasChanged()待機処理実装時
   - SignalR接続確立確認実装時
   - Toast通知検証実装時

4. **Playwright MCPツール選択時**
   - playwright_navigate/snapshot/click/fill等の使い分け判断時
   - アクセシビリティツリー取得時
   - ブラウザ操作パターン選択時

## 3つのE2Eテストパターン

### 1. data-testid属性設計パターン

**詳細**: [`patterns/data-testid-design.md`](./patterns/data-testid-design.md)

**概要**:
- **ボタン**: `{action}-button` (例: `member-add-button`, `member-delete-button`)
- **入力フィールド**: `{field}-input` (例: `username-input`, `password-input`)
- **リスト**: `{entity}-list` (例: `member-list`, `project-list`)
- **カード**: `{entity}-card` (例: `member-card`, `project-card`)
- **エラー**: `{context}-error-message` (例: `member-error-message`)
- **リンク**: `{target}-link` (例: `member-management-link`)

**適用場面**:
- Blazor Serverコンポーネント実装時に自律的に適用
- E2Eテスト作成時のセレクタ選択に自律的に適用

---

### 2. Playwright MCPツール活用パターン

**詳細**: [`patterns/mcp-tools-usage.md`](./patterns/mcp-tools-usage.md)

**概要**:
- **playwright_navigate**: URL遷移・ページ読み込み
- **playwright_snapshot**: アクセシビリティツリー取得（構造化データ・高速・正確）
- **playwright_click**: ボタン・リンククリック
- **playwright_fill**: フォーム入力
- **playwright_select**: ドロップダウン選択
- **playwright_wait_for**: 要素表示待機・時間待機

**適用場面**:
- E2Eテスト実装時に最適なMCPツールを自律的に選択
- ブラウザ操作パターンに応じた使い分け判断

---

### 3. Blazor Server SignalR対応パターン（7パターン）

**詳細**: [`patterns/blazor-signalr-e2e.md`](./patterns/blazor-signalr-e2e.md)

**概要（7パターン）**:
- **パターン1 SignalR接続確立待機**: `await page.WaitForLoadStateAsync(LoadState.NetworkIdle);`
- **パターン2 StateHasChanged()待機**: `await page.WaitForTimeoutAsync(1000);` による非同期UI更新待機
- **パターン3 Toast通知検証**: `.toast-success`, `[role='alert']` セレクタ使用
- **パターン4 JavaScript confirmダイアログ処理**: `page.Dialog` イベントハンドラ登録
- **パターン5 URL遷移確認**: `await page.WaitForURLAsync("**/path");`
- **パターン6 要素表示待機**: `WaitForSelectorState.Visible` 明示指定
- **パターン7 CDP Network Throttling**: ローディング状態テスト（SignalR対応） 🆕

**パターン7詳細（2025-12-15追加）**:
- **課題**: Blazor ServerはSignalR通信のため `page.route()` によるAPIインターセプトが効かない
- **解決**: CDP (Chrome DevTools Protocol) でネットワーク層に遅延を挿入
- **用途**: ローディングスピナー・スケルトンスクリーン・プログレスバー表示確認

**適用場面**:
- Blazor Server E2Eテスト実装時に自律的に適用
- 非同期UI更新・SignalR接続の考慮が必要な場面
- **ローディング状態のE2Eテスト実装時**（パターン7）

---

## Phase B2 Step6実証結果

### 効率化実績
- **従来手法推定時間**: 2-3時間/機能（150-180分）
- **Playwright MCP活用実測時間**: 約10分/機能
- **削減率**: **93.3%**（計画75-85%を大幅超過） 🎉

### 削減要因
1. ✅ data-testid属性設計パターン確立（Phase B2 Step5完了）
2. ✅ Blazor Server SignalR対応知見（Phase B1基盤活用）
3. ✅ C# Playwright実装経験蓄積

### 作成したE2Eテスト（実証例）
- `tests/UbiquitousLanguageManager.E2E.Tests/UserProjectsTests.cs`
  - ProjectMembers_AddMember_ShowsSuccessMessage
  - ProjectMembers_RemoveMember_ShowsSuccessMessage
  - ProjectMembers_AddDuplicateMember_ShowsErrorMessage

---

## GitHub Issue #56対応

このSkillは、bUnit統合テスト技術的課題8件のE2E代替実装パターンを提供します：

### bUnitで困難な範囲（E2Eテストで実証）
1. **EditForm送信ロジック**: `OnValidSubmit`イベントトリガー
2. **子コンポーネント連携**: ProjectMemberSelector/ProjectMemberCard統合
3. **Blazor Server SignalR接続**: StateHasChanged()動作確認
4. **JavaScript confirmダイアログ**: 削除確認ダイアログ処理
5. **Toast通知表示**: 非同期通知検証
6. **非同期UI更新**: SignalR経由の自動更新確認

---

## 関連ADR・GitHub Issues

### ADR
- **ADR_021**: Playwright MCP + Agents統合戦略（技術決定の歴史的記録）
- ADR_020: テストアーキテクチャ決定
- ADR_010: 実装規約

### GitHub Issues
- **GitHub Issue #56**: bUnit統合テスト技術課題（E2E代替実装完了）
- **GitHub Issue #54**: Agent Skills導入提案（本Skillで Phase 1前倒し完了）

---

## 横展開可能性

このSkillは、以下のプロジェクトに高い横展開価値を提供します：

### 対象プロジェクト
- .NET + Blazor Server プロジェクト全般
- F# + C# Clean Architecture プロジェクト
- SignalR を使用するリアルタイムWeb アプリケーション
- Playwright for .NET 採用プロジェクト

### Plugin化構想
- **ubiquitous-language-manager-skills** Pluginの一部として配布予定
- Claude Code Marketplace申請検討（Phase B完了後）
- コミュニティ貢献・横展開基盤構築

---

## 次のステップ

### Phase B3以降での活用
- Claudeが自律的にこのSkillを使用してE2Eテスト作成
- Playwright Agents（Planner/Generator/Healer）との統合活用
- 新規機能実装時の自動E2Eテスト生成パターン確立

### Skill拡張
- bUnit代替パターンの追加（GitHub Issue #56完全解決）
- Playwright Healer Agent実用評価結果の反映
- UI変更時の自動修復パターン追加

---

**Skill作成日**: 2025-10-26
**最終更新**: 2025-12-15（パターン7 CDP Network Throttling追加）
**Phase**: Phase B2 Step6 Stage 4 / Phase B-F3 Step1.5
**実証結果**: 93.3%効率化達成
**GitHub Issue #54**: Phase 1実験的導入 前倒し完了 🎉
