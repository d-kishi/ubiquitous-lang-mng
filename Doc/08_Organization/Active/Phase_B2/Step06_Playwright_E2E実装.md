# Step 06 組織設計・実行記録

**Step名**: Step06 Playwright E2Eテスト実装・統合効果検証
**作業特性**: E2Eテスト実装・Agents統合・効果測定・ADR作成
**推定期間**: 1.5-2時間（4 Stage構成）
**前提条件**: Step5（UI実装）完了必須
**開始予定日**: Step5完了後

---

## 📋 Step概要

### Step目的
- UserProjects機能E2Eテスト実装（UI実装完了後）
- Playwright Agents統合によるテストメンテナンス効率化（50-70%削減）
- 総合効果検証（作成効率 + メンテナンス効率）
- ADR_021作成による技術決定永続化
- **GitHub Issue #56対応**: bUnit技術的課題8件のE2Eテスト代替実装

### Step成功基準
- ✅ E2E.Tests実装完了（UserProjects 3シナリオ実装・実行成功）
- ✅ Playwright Agents統合完了（VS Code 1.105安定版動作確認）
- ✅ 総合85%効率化検証完了（実測値記録）
- ✅ ADR_021作成完了（技術決定永続化）
- ✅ GitHub Issue #56対応完了（bUnit困難範囲のE2E実証確認）

---

## 🏢 組織設計

### SubAgent構成
- **integration-test Agent**: Stage 1-3担当（E2Eテスト作成・Agents統合・効果検証）
- **tech-research Agent**: Stage 4担当（ADR_021作成）

### 並列実行計画
```yaml
並列実行:
  - integration-test: Stage 1-3（E2Eテスト作成→Agents統合→効果検証）
  - tech-research: Stage 4（ADR_021作成）

実行タイミング:
  - Stage 1-3: 順次実行（integration-test担当）
  - Stage 4: Stage 3と並行可能（tech-research担当）
```

### 🤖 Agent Skills Phase 1効果測定（Session 3）

**測定機会**: Step6はAgent Skills導入後の初E2Eテスト実装

**測定項目**:
- integration-test Agentでの自律的Skills使用確認
  - clean-architecture-guardian: E2Eテスト実装時のCA品質自動チェック
  - fsharp-csharp-bridge: F#型統合（該当時）
- Playwright E2Eテスト実装時のエラー発生率記録
- 作業効率記録（計画1.5-2時間 vs 実測時間）

**記録先**: `Doc/08_Organization/Active/AgentSkills_Phase1_効果測定.md`

---

## 📚 必須参照ファイル（Step間成果物参照マトリックス準拠）

### 1. Tech_Research_Playwright_2025-10.md
**参照目的**: Playwright Agents統合実装手順
**重点参照セクション**:
- 3章: Playwright Agents最新状況（VS Code 1.105安定版対応）
- 4章: セキュリティ・クレデンシャル管理方針

**活用内容**:
- Stage 2: Agents統合コマンド確認（行468-486）
- セキュリティ設定: テスト専用アカウント方針（行492-538）

### 2. Spec_Analysis_UserProjects.md
**参照目的**: E2Eテストシナリオ作成
**重点参照セクション**:
- 3.2節: ユーザー操作フロー（行172-195）

**活用内容**:
- メンバー追加フロー（7ステップ・行174-181）
- メンバー削除フロー（6ステップ・行183-189）
- エラーハンドリング・バリデーション（3パターン・行191-194）

### 3. Phase_B2_Implementation_Plan.md
**参照目的**: Step6詳細実装内容・リスク管理
**重点参照セクション**:
- リスク管理計画（4章）

### 4. Phase_Summary.md
**参照目的**: Phase B2全体計画・Step間引継ぎ事項
**重点参照セクション**:
- Step5完了状況（E2Eテスト準備完了確認）
- Step間成果物参照マトリックス
- GitHub Issue #56（bUnit技術的課題）記録

---

## 🎯 Step実行計画（5 Stage構成：Stage 0追加）

### Stage 0: セキュリティ準備（15分・Stage 1前に必須実施）

**重要**: E2Eテスト実行前のセキュリティ設定必須

**実施内容**:

#### 0-1. .gitignore設定追加（5分）
- [ ] .gitignore に以下を追加:
  ```gitignore
  # E2Eテスト・Playwright関連
  .env.test
  appsettings.Test.json
  /playwright/.auth/
  playwright-report/
  test-results/

  # Playwright Agents
  .playwright/agents/*.log
  .playwright/agents/cache/
  ```

#### 0-2. テスト専用アカウント作成（5分）
- [ ] E2Eテスト用アカウント作成:
  - Email: `e2e-test@ubiquitous-lang.local`
  - Password: `E2ETest#2025!Secure`
  - Role: SuperUser（全権限・テストデータ操作可）
- [ ] Docker環境内でのみ使用・外部アクセス不可確認

#### 0-3. 環境設定ファイル作成（5分）
- [ ] `appsettings.Test.json` 作成（テストDB接続文字列）
  - テストデータベース専用接続文字列設定
  - 本番データとの完全分離確認
- [ ] `.env.test` 作成（E2Eテストクレデンシャル）
  - `E2E_TEST_EMAIL=e2e-test@ubiquitous-lang.local`
  - `E2E_TEST_PASSWORD=E2ETest#2025!Secure`

**セキュリティ確認**:
- [ ] .gitignore設定完了（機密情報コミット防止）
- [ ] テスト専用アカウント・本番データ分離確認
- [ ] 環境変数管理方式確認

---

### Stage 1: E2Eテスト作成（30分・MCPツール活用）

**前提条件確認**:
- [ ] Stage 0完了確認（セキュリティ準備完了）
- [ ] Step5完了確認（UI実装完了・2025-10-23完了）
- [ ] data-testid属性実装確認（15要素実装済み）
  - ProjectMembers.razor: 7要素
  - ProjectMemberSelector.razor: 1要素
  - ProjectMemberCard.razor: 3要素
  - Login.razor: 3要素
  - ProjectEdit.razor: 1要素
- [ ] https://localhost:5001 アクセス可能確認
- [ ] テスト専用アカウント準備確認

**実施内容**:
E2E.Testsプロジェクトに以下3シナリオのE2Eテスト作成

**GitHub Issue #56対応**: 以下のbUnit困難範囲をE2Eテストで実証
- フォーム送信ロジック（EditForm.Submit()問題回避）
- 子コンポーネント連携（ProjectMemberCard/ProjectMemberSelector統合）
- Blazor Server SignalR接続・StateHasChanged動作確認

#### 1-1. メンバー追加E2Eテスト（10分）
**テストシナリオ**（Spec_Analysis 3.2節 行174-181準拠）:
1. https://localhost:5001 にアクセス（playwright_navigate）
2. ログイン（テスト専用アカウント）
3. プロジェクト一覧画面で「👥 メンバー」ボタンをクリック（playwright_click）
4. メンバー管理画面遷移確認（playwright_snapshot）
5. ユーザー選択ドロップダウン操作（playwright_select）
6. 「✅ 追加」ボタンをクリック（playwright_click）
7. 成功メッセージ検証「{ユーザー名}をプロジェクトに追加しました」
8. メンバー一覧自動更新確認

#### 1-2. メンバー削除E2Eテスト（10分）
**テストシナリオ**（Spec_Analysis 3.2節 行183-189準拠）:
1. メンバー管理画面で「🗑️」ボタンをクリック（playwright_click）
2. 削除確認ダイアログ検証（playwright_snapshot）
3. 「確認」ボタンをクリック（playwright_click）
4. 成功メッセージ検証
5. メンバー一覧自動更新確認

#### 1-3. エラーハンドリングE2Eテスト（10分）
**テストシナリオ**（Spec_Analysis 3.2節 行191-194準拠）:
1. 重複追加エラー検証「{ユーザー名}は既にこのプロジェクトのメンバーです」
2. 最後の管理者削除エラー検証「プロジェクトには最低1名のプロジェクト管理者が必要です」
3. 権限不足エラー検証「プロジェクトメンバー管理権限がありません」

**MCPツール活用方針**:
- playwright_navigate: https://localhost:5001へのアクセス
- playwright_snapshot: アクセシビリティツリー取得
- playwright_click: ボタン・リンククリック
- playwright_fill: フォーム入力
- playwright_select: ドロップダウン選択
- playwright_console_messages: エラーメッセージ検証

**確認事項**:
- [ ] 3シナリオ全テスト作成完了
- [ ] テスト実行成功（3/3成功）
- [ ] 0 Warning / 0 Error維持

**成果物**:
- tests/UbiquitousLanguageManager.E2E.Tests/UserProjectsTests.cs
- テストコード: メンバー追加・削除・エラーハンドリング3シナリオ

---

### Stage 2: Playwright Agents統合（15分）

**重要**: VS Code 1.105安定版対応完了（2025-10-10リリース）
- **推奨度**: **9/10点（強く推奨）** ← 従来7/10点から格上げ
- **Insiders依存リスク**: 完全解消（安定版で完全動作）
- **プロダクション環境対応**: 準備完了

**実施内容**:
```bash
# Agents初期化
npx playwright init-agents --loop=vscode

# VS Code設定確認
# .playwright/agents/設定ファイル確認
```

**確認事項**:
- [ ] VS Code 1.105以上確認（安定版対応）
- [ ] npx playwright init-agents実行成功
- [ ] .playwright/agents/ディレクトリ生成確認
- [ ] Planner/Generator/Healer設定ファイル確認
- [ ] VS Code再起動

**セキュリティ設定**（申し送り事項準拠）:
- [ ] .gitignore設定追加: `.env.test`, `/playwright/.auth/`, `test-results/`
- [ ] テスト専用アカウント作成: e2e-test@ubiquitous-lang.local
- [ ] appsettings.Test.json作成（gitignore追加）

**成果物**:
- Playwright Agents統合完了状態
- .playwright/agents/設定ファイル
- セキュリティ設定完了

---

### Stage 3: 統合効果検証（30分）

**実施内容**:

#### 3-1. 作成効率測定（MCP使用 vs 従来手法）（15分）
**測定項目**:
- 従来手法推定時間: 2-3時間（手動テストコード作成）
- MCP活用実測時間: Stage 1実測時間記録
- 削減率算出: ((従来 - MCP) / 従来) × 100%
- 目標達成確認: 削減率 ≥ 75%

#### 3-2. メンテナンス効率測定（Agents活用）（15分）
**測定項目**:
- UI変更シミュレーション実施
- Healer自動修復動作確認
- 修復成功率測定
- 目標達成確認: メンテナンス効率 50-70%削減

#### 3-3. 総合効果記録
**記録項目**:
- 作成効率測定結果
- メンテナンス効率測定結果
- 総合85%効率化検証結果
- Phase B2-B5予測効果更新

**成果物**:
- 効果測定レポート（本Step記録に記載）

---

### Stage 4: ADR記録作成（20分）

**重要**: ADR_021（Playwright MCP + Agents統合戦略）新規作成
- **現状**: 未作成（Step6で初めて作成）
- **Status**: Accepted予定
- **実測効果記録**: Stage 3効果測定結果を反映

**実施内容**:
ADR_021: Playwright MCP + Agents統合戦略 作成

**記録内容**（Tech_Research技術決定根拠準拠）:
```markdown
# ADR_021: Playwright MCP + Agents統合戦略

## Status
Accepted

## Context
Phase B2でE2Eテスト基盤確立・UserProjects機能検証が必要。
従来手法ではテスト作成・メンテナンスに多大な工数が必要。
Playwright MCP + Agents統合により85%効率化の可能性。

## Decision
1. Playwright MCP統合採用（推奨度9/10点）
   - プロダクション準備完了・Anthropic公式
   - 25種類ブラウザ操作ツール活用
   - Claude Code直接統合

2. Playwright Agents統合採用（推奨度9/10点）
   - VS Code 1.105安定版対応確認済み
   - Planner/Generator/Healer自動化
   - テストメンテナンス自動化

3. 統合推奨度10/10点（最強相乗効果）
   - 作成効率75-85%向上 + メンテナンス効率50-70%削減

## Consequences
### Positive
- E2Eテスト作成効率75-85%向上（実測結果: [記録]）
- テストメンテナンス効率50-70%削減（実測結果: [記録]）
- 総合85%効率化達成
- Phase B全体で10-15時間削減
- .NET+Blazor Server先駆者知見蓄積

### Negative
- Playwright MCP: 初回学習コスト（5-10分）
- Playwright Agents: 自動修復精度80-85%（手動検証15-20%必要）

## Risks & Mitigation
### MCP統合リスク（低）
- リスク: 初回統合失敗
- 対策: Claude Code再起動・公式ドキュメント参照

### Agents統合リスク（中→低）
- リスク: VS Code Insiders依存（Phase B-F1評価時）
- 現状: VS Code 1.105安定版対応完了（2025-10-10）
- 対策: ロールバック手順確立（30分で従来手法へ切り戻し）

## Related
- Phase B-F1 Phase_Summary.md
- Phase_B2_申し送り事項.md
- Tech_Research_Playwright_2025-10.md

## Notes
- 効果測定結果: [Stage 3実測値記録]
- Phase B2-B5予測効果: [更新]
```

**成果物**:
- /Doc/07_Decisions/ADR_021_Playwright統合戦略.md

---

## 📊 Step実行記録（随時更新）

### Stage 1実行記録
⏳ **未実施**（Step5完了後に実施）

### Stage 2実行記録
⏳ **未実施**（Step5完了後に実施）

### Stage 3実行記録
⏳ **未実施**（Step5完了後に実施）

### Stage 4実行記録
⏳ **未実施**（Step5完了後に実施）

---

## ✅ Step終了時レビュー

### 成果物確認
- [ ] E2E.Testsプロジェクト初期実装（3シナリオ）
- [ ] Playwright Agents統合完了状態
- [ ] ADR_021作成完了
- [ ] 効果測定レポート完成

### 品質確認
- [ ] E2Eテスト実行成功（3シナリオ全て成功）
- [ ] Playwright Agents動作確認（自動修復機能確認）
- [ ] ビルド品質維持（0 Warning / 0 Error）
- [ ] セキュリティ設定完了（.gitignore・テスト専用アカウント）

### 次Step準備確認
- [ ] Phase B2完了確認
- [ ] Phase_Summary.md更新（Step6完了記録）

---

**Step作成日**: 2025-10-17
**Step責任者**: Claude Code
**Step監督**: プロジェクトオーナー
