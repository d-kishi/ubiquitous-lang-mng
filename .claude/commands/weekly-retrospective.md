# 週次振り返りCommand

**目的**: 週単位での振り返り・改善点抽出・次期計画策定
**対象**: 週末セッション・振り返り要求時

## コマンド実行内容

### 1. 振り返り期間・対象確認（必須）
- [ ] 対象期間の明確化（開始日〜終了日・週番号）
- [ ] 対象期間中の作業記録収集（`Doc/04_Daily/YYYY-MM/`から関連ファイル特定）
- [ ] 実施Phase・完了事項の整理（フェーズ進捗・達成状況）

### 2. 成果・マイルストーン整理
- [ ] 期間中の主要成果抽出（完了Phase・重要機能実装・技術基盤確立）
- [ ] 定量的成果測定（品質スコア・時間効率・テスト成功率等）
- [ ] 技術的マイルストーン確認（アーキテクチャ改善・技術負債解消）
- [ ] プロセス改善成果（SubAgent効果・Commands効果・効率化実績）

### 3. 課題・問題分析
- [ ] 発生課題の分類・整理（技術的課題・プロセス課題・要件課題）
- [ ] 未解決問題の継続状況確認（技術負債・継続課題・残存リスク）
- [ ] 新規発見問題の記録（潜在課題・設計問題・運用課題）
- [ ] 問題解決プロセスの効果検証（解決手法・時間効率・品質効果）

### 4. 学習事項・改善提案抽出
- [ ] 重要な技術的学習事項整理（新技術理解・ベストプラクティス発見）
- [ ] プロセス改善知見抽出（SubAgent活用法・Commands効果・効率化手法）
- [ ] 品質向上要因分析（成功要因・品質確保手法・改善効果）
- [ ] 次期改善提案策定（プロセス改善・技術改善・品質改善）

### 5. 効率・品質評価
- [ ] 時間効率測定（予定vs実績・効率化効果・時間配分最適化）
- [ ] 品質指標評価（技術品質・プロセス品質・成果物品質）
- [ ] SubAgent・Commands効果測定（並列実行効果・自動化効果）
- [ ] 組織運用効果検証（Phase適応型組織・専門役割効果）

### 6. 次期計画策定
- [ ] 次週重点事項設定（最優先課題・実施予定Phase・改善事項）
- [ ] 継続課題管理（優先度更新・対応計画・期限設定）
- [ ] リスク管理（潜在リスク・予防策・contingency計画）
- [ ] 改善アクション計画（具体的改善項目・実施方法・効果測定）

### 7. 文書化・記録
- [ ] **週次総括文書作成**: `Doc/04_Daily/YYYY-MM/週次総括_YYYY-WXX.md`
  - 対象期間: YYYY-MM-DD ～ YYYY-MM-DD
  - 週番号: YYYY-WXX形式（例: 2025-W33）
  - 内容: 主要成果・課題分析・学習事項・次期計画
- [ ] **Serenaメモリー更新（edit_memory方式）**: 4種類のメモリー更新
- [ ] 重要決定事項のADR化検討（プロセス改善・技術方針等）

### 8. Serenaメモリー更新（必須・edit_memory方式）

#### 🔴 重要：Context効率化のためedit_memoryを使用
**理由**: Context消費65.3%削減・応答性28.6%改善達成（2025-11-16検証済み）
**適用範囲**: weekly-retrospectiveコマンド実行時のみ（手動操作・他コマンドでは従来通り`write_memory`使用可能）
**禁止事項**: weekly-retrospective時は`mcp__serena__read_memory` / `mcp__serena__write_memory`使用禁止

#### 更新手順（全メモリー共通）
1. **edit_memoryツール**: regex指定で該当箇所のみ差分更新（置換・追記・削除）
2. **検証**: 必要に応じてtailコマンドで更新結果確認

#### 各メモリー更新詳細

- [ ] **project_overview更新（`.serena/memories/project_overview.md`）**:
  ```
  必須更新:
  1. edit_memoryツールでregex実行:
     - memory_file_name: project_overview.md
     - regex: (## 📅 週次振り返り実施状況\n\n)[\s\S]*?(\n\n## 次回セッション推奨範囲)
     - repl: $1[最新週次振り返り記録追加 + 次回計画更新]$2
  ```

- [ ] **development_guidelines更新（`.serena/memories/development_guidelines.md`）**:
  ```
  変更がある場合のみ:
  1. edit_memoryツールでregex実行:
     - memory_file_name: development_guidelines.md
     - regex: (該当セクション名を含むregexパターン)
     - repl: [週次振り返りで確立されたプロセス改善追記 or 既存セクション更新]
  変更がない場合: スキップ
  ```

- [ ] **tech_stack_and_conventions更新（`.serena/memories/tech_stack_and_conventions.md`）**:
  ```
  変更がある場合のみ:
  1. edit_memoryツールでregex実行:
     - memory_file_name: tech_stack_and_conventions.md
     - regex: (該当セクション名を含むregexパターン)
     - repl: [週次で確立された技術パターン・ベストプラクティス追記]
  変更がない場合: スキップ
  ```

- [ ] **task_completion_checklist更新（`.serena/memories/task_completion_checklist.md`）**:
  ```
  必須更新:
  1. edit_memoryツールでregex実行:
     - memory_file_name: task_completion_checklist.md
     - regex: (## 🔄 次回セッション継続タスク\n\n)[\s\S]*?(\n\n## )
     - repl: $1[最終更新日時更新 + 週次完了マーク + 次週継続タスク更新]$2
  ```

- [ ] **メモリー更新品質確認**:
  - 各メモリーの差分更新が適切に実行されたか確認
  - 既存重要情報が維持されているか確認（破壊的変更なし）
  - 次回セッションで参照可能な状態か確認

### 9. daily_sessions統合・削除処理（必須）

#### 🔴 重要：振り返り対象期間のセッション記録を統合後削除

**目的**: daily_sessionsメモリーの肥大化防止・Context効率化

**処理手順**:

- [ ] **Step 1: 対象期間の日付セクション特定**
  ```
  Grep "^## 📅" で全日付セクション取得
  振り返り対象期間の日付を特定（例: 2025-10-01 ～ 2025-10-07）
  ```

- [ ] **Step 2: 対象期間のセッション記録読み込み**
  ```
  tailコマンドで直近セッション記録確認（削除前の最終確認）
  ```

- [ ] **Step 3: weekly_retrospectives.mdに統合**
  ```
  edit_memoryツールでregex実行:
  - memory_file_name: weekly_retrospectives.md
  - regex: (^# 週次振り返り実施記録\n\n)
  - repl: $1## 📅 YYYY年第XX週（MM/DD-MM/DD）\n\n[対象期間の主要情報を抽出・要約]\n\n
  ```

- [ ] **Step 4: daily_sessionsから対象期間を削除**
  ```
  edit_memoryツールでregex実行:
  - memory_file_name: daily_sessions.md
  - regex: (## 📅 YYYY-MM-DD.*?\n\n---\n\n){複数回マッチ}
  - repl: （空文字列で削除）
  注意: 最新1週間分のみ保持・対象期間全体を削除
  ```

**削除対象期間**: 振り返り対象期間全体（通常1週間）

**保持期間**: 最新1週間分のみdaily_sessionsに保持

**アーカイブ**: weekly_retrospectives.mdに要約統合・永続化

### 10. 振り返り品質確認
- [ ] 文書完成度確認（必要情報の網羅性・具体性・実用性）
- [ ] 改善提案の実行可能性評価（具体性・優先度・期待効果）
- [ ] 次期アクション明確性確認（実施内容・担当・期限・成功基準）
- [ ] ユーザーへの振り返り結果報告・次期方針確認

### 11. MCP更新確認（自動レポート）

#### 🔴 目的：MCPツール変更の見逃し防止・SubAgent定義の陳腐化防止

**実行内容**:

- [ ] **Playwright MCP更新確認**:
  ```bash
  # 1. 現在のバージョン確認
  npx @playwright/mcp@latest --version

  # 2. npm最新版確認
  npm view @playwright/mcp version

  # 3. 直近1週間のリリース取得（GitHub API）
  gh api repos/microsoft/playwright-mcp/releases?per_page=5 \
    | grep -E '"tag_name"|"created_at"|"body"'

  # 4. 現在利用可能なツール一覧取得（JSON-RPC）
  echo '{"jsonrpc": "2.0", "id": 1, "method": "tools/list"}' \
    | npx @playwright/mcp@latest \
    | jq '.result.tools[].name'
  ```

- [ ] **Serena MCP更新確認**:
  ```bash
  # 1. 最新リリース確認（GitHub API）
  gh api repos/oraios/serena/releases/latest

  # 2. 直近1週間のリリース取得
  gh api repos/oraios/serena/releases?per_page=5 \
    | grep -E '"tag_name"|"created_at"|"body"'
  ```

- [ ] **変更レポート作成**:
  - 新規バージョンがある場合: リリースノートのツール変更箇所を抽出
  - ツール追加: `.claude/agents/e2e-test.md`への追加推奨
  - ツール廃止: 影響範囲の調査（SubAgent定義で使用中か確認）
  - ツール非推奨: 代替手段の検討・移行計画策定

- [ ] **SubAgent定義更新判断**:
  - ユーザーにレポート提示
  - 更新が必要な場合: `.claude/agents/e2e-test.md`手動編集
  - 更新不要の場合: スキップ

**出力例**:
```markdown
## MCP更新レポート（YYYY-MM-DD）

### Playwright MCP
- **現在のバージョン**: v0.0.45
- **最新バージョン**: v0.0.45（最新）
- **直近1週間の変更**: なし

### Serena MCP
- **現在のバージョン**: v0.1.4
- **最新バージョン**: v0.1.4（最新）
- **直近1週間の変更**: なし

### 推奨アクション
- ✅ 現在最新版を使用中 - 対応不要
```

**メンテナンス詳細参照**:
- **ADR_024**: MCPツール更新時のメンテナンス手順（完全版）
- **期待運用コスト**: 5-10分/週（レポート確認・判断）

---

### 12. Command実行完了確認・継続判断

#### 🔴 必須実行確認（自己チェック）
**実行証跡必須確認**:
- [ ] **Write/Editツール実行結果確認**: 以下の必須ファイル作成・更新が実際に実行されたか
  - [ ] `週次総括_YYYY-WXX.md` 作成済み（週次総括文書完成）

- [ ] **edit_memoryツール実行確認**: regex指定で4種類のメモリー更新
  - [ ] project_overview差分更新実行済み（週次振り返り記録・次週計画）
  - [ ] development_guidelines差分追記実行済み（変更がある場合のみ・スキップ可）
  - [ ] tech_stack_and_conventions差分追記実行済み（変更がある場合のみ・スキップ可）
  - [ ] task_completion_checklist状態更新実行済み（週次完了マーク・次週継続タスク）

- [ ] **daily_sessions統合・削除確認**: 振り返り対象期間のセッション記録をweekly_retrospectives.mdに統合後、daily_sessionsから削除実行済み
  - [ ] tailコマンド実行済み（対象期間セッション記録確認）
  - [ ] weekly_retrospectives.md統合実行済み（edit_memory実行・最新振り返りセクション作成）
  - [ ] daily_sessions削除実行済み（edit_memory実行・対象期間の全日付セクション削除）

- [ ] **write_memory不使用確認**: weekly-retrospective時は`mcp__serena__write_memory`使用していないことを確認

- [ ] **実行内容品質確認**: 各更新内容が具体的・実用的で次回セッションで参照可能か

#### 🔴 実行漏れ防止チェック
- [ ] **抽象的報告の排除**: 「実施しました」ではなく実際のツール呼び出し実行済み
- [ ] **段階的実行**: 各項目を順次実行し実行結果確認してから次項目へ進行済み
- [ ] **自己検証完了**: 上記全項目の実行証跡確認済み

#### 継続・終了判断
- [ ] **Command実行完了確認**: 上記自己チェック全項目が実行完了していることを確認
- [ ] **次Action提示**: 次週作業開始 or Phase開始 or セッション終了
- [ ] **終了処理**: 週次振り返り全完了確認・次週準備完了宣言

## 文書出力仕様

### ファイル命名規則
- **ファイル名**: `週次総括_YYYY-WXX.md`
- **配置場所**: `Doc/04_Daily/YYYY-MM/`
- **週番号**: ISO 8601準拠（月曜始まり）

### 文書構造標準
```markdown
# 週次総括 YYYY年第XX週（MM/DD-MM/DD）

## 📊 週次概要
### 週の主要テーマ・セッション実績

## 🎯 主要成果・マイルストーン
### Phase別達成状況・技術基盤確立・品質向上

## 📈 技術的成果・改善効果
### 定量的評価・プロセス改善・効率化成果

## 🔍 継続課題・次期対応事項
### 未解決課題・次期重点事項・リスク管理

## 💡 学習事項・改善提案
### 重要発見・ベストプラクティス・改善案

## 🎯 次週重点事項
### 最優先事項・継続改善・品質保証
```

## 実行トリガー
- ユーザー発言: "週次振り返り"、"振り返り実施"
- ユーザー発言: "今週の振り返り"、"週次レビュー"
- 週末セッション時の自動提案（金曜・土曜・日曜セッション時）

## 関連Command連携
- **session-end**: 振り返り要素の集約・継続課題管理
- **step-end-review**: Phase完了時評価の統合
- **spec-compliance-check**: 仕様準拠状況の週次評価