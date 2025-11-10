# Step5 Stage3: 次回セッション実施手順

**実施予定**: 次回セッション
**所要時間**: 2-3時間
**実施環境**: GitHub Codespaces

---

## 🎯 次回セッションで実施すること

**GitHub Codespaces技術調査（5項目）**

---

## 📋 事前準備（2025-11-11完了）

- [x] 技術調査計画書テンプレート作成完了（`Doc/08_Organization/Active/Phase_B-F2/Research/Codespaces技術調査結果.md`）
- [x] 次回セッション実施手順書作成完了（本ファイル）
- [x] **Dockerfile修正完了**（Claude Code CLIインストール追加）
- [x] **構築手順ドキュメント作成完了**（`Doc/99_Others/GitHub_Codespaces_DevContainer構築手順.md`）
- [x] **ローカルDevContainer検証完了**（Claude Code CLI動作確認済み）
- [x] **GitHub Secrets設定完了**（`ANTHROPIC_API_KEY`）
- [x] **Codespaces再ビルド・動作確認完了**
- [x] Git commit/push完了（commit: 46c5e62）
- [x] セッション終了処理実施（`/session-end`）

**重要な追加作業**:
- 当初想定していなかったClaude Code CLI統合作業を実施
- ローカル検証 → Codespaces検証の2段階検証を完了
- 調査項目1の一部（Claude CLI動作確認）を完了

---

## 🚀 次回セッション開始手順（Codespaces環境）

### 前提条件（✅ 完了済み）

- ✅ Dockerfile修正済み（Claude Code CLIインストール）
- ✅ GitHub Secrets設定済み（ANTHROPIC_API_KEY）
- ✅ Codespaces環境構築済み（再ビルド完了）
- ✅ Claude Code CLI動作確認済み

### Step 1: 既存Codespacesを開く

**ユーザー操作**:

1. **GitHubリポジトリにアクセス**
   ```
   https://github.com/d-kishi/ubiquitous-lang-mng
   ```

2. **既存Codespacesを開く**
   - 「Code」ボタン → 「Codespaces」タブ
   - 既存のCodespace（feature/PhaseB-F2）を選択
   - クリックして起動

3. **Codespaces環境が起動**（30秒～1分）
   - VS Code Web版が開く

### Step 2: Codespaces内でClaude Code CLIを起動

**ユーザー操作**:

1. **Codespacesターミナルを開く**
   - Ctrl+` または View → Terminal

2. **Claude Code CLIを起動**
   ```bash
   claude
   ```

3. **セッション開始**
   ```
   セッションを開始します
   ```
   - これにより`/session-start` Commandが自動実行されます

### Step 3: 技術調査継続を指示

**ユーザー指示（そのまま貼り付け可能）**:

```
Step5 Stage3（GitHub Codespaces技術調査）の調査項目2から開始してください。

調査項目1（Codespaces環境構築・Claude CLI動作確認）は前回セッションで完了済みです。

以下のファイルを参照してください：
- Doc/08_Organization/Active/Phase_B-F2/Research/Codespaces技術調査結果.md

調査項目2: MCP Server接続確認
調査項目3: 開発環境動作確認（dotnet build/test）
調査項目4: 基本Command実行確認
調査項目5: バックグラウンド実行検証

各項目の結果を技術調査レポートに記録してください。
```

---

## 📝 技術調査の実施順序（Claude Code実施）

### 調査項目1: Codespaces環境構築（✅ 完了）

**実施日**: 2025-11-11（前回セッション）

**実施内容**:
- ✅ Dockerfile修正（Claude Code CLIインストール）
- ✅ ローカルDevContainer検証成功
- ✅ GitHub Secrets設定（ANTHROPIC_API_KEY）
- ✅ Codespaces再ビルド成功
- ✅ Claude Code CLI動作確認成功

**記録先**: `Codespaces技術調査結果.md` - 調査項目1セクション（記録済み）

### 調査項目2: MCP Server接続確認（30分）⚙️ 次回実施

**実施内容**:
1. `claude mcp list` 実行
2. Serena MCP、Playwright MCP認識確認
3. Serenaメモリー読み込みテスト（project_overview）

**記録先**: `Codespaces技術調査結果.md` - 調査項目2セクション

**次回セッション開始位置**: ここから開始

### 調査項目3: 開発環境動作確認（30分）

**実施内容**:
1. `dotnet --version` 確認
2. `dotnet restore` 実行
3. `dotnet build` 実行（0 Warning, 0 Error確認）
4. `dotnet test` 実行（全テスト成功確認）

**記録先**: `Codespaces技術調査結果.md` - 調査項目3セクション

### 調査項目4: 基本Command実行確認（30分）

**実施内容**:
1. `/session-start` 実行（既に実施済みの場合はスキップ）
2. `/spec-compliance-check` 実行
3. SubAgent・Skills動作確認

**記録先**: `Codespaces技術調査結果.md` - 調査項目4セクション

### 調査項目5: バックグラウンド実行検証（30分）

**実施内容**:
1. タスク投入（例：weekly-retrospective）
2. ブラウザを閉じる（またはCodespacesタブを閉じる）
3. 30分待機
4. Codespaces再接続
5. タスク継続実行確認

**記録先**: `Codespaces技術調査結果.md` - 調査項目5セクション

---

## 🎯 Go/No-Go判断（Claude Code実施）

### 判断基準

**Go条件**: 5項目すべて成功

**実施内容**:
1. 各調査項目の結果を総合評価
2. 必須要件充足度を計算（XX/20項目）
3. Go/No-Go判断を記録
4. 次のアクションを決定

**記録先**: `Codespaces技術調査結果.md` - Go/No-Go判断セクション

---

## 📊 Go判断時の次アクション（Claude Code実施）

### 1. Issue #51更新（30分）

**更新内容**:
- Phase1セクション更新
  - Claude Code on the Web検証結果（既存）
  - GitHub Codespaces検証結果（新規追加）
  - Phase2実施判断（Go決定）
- Phase2セクション更新
  - Stage4-5の実施計画詳細化

### 2. Step05組織設計のStage4以降詳細化（30分）

**更新ファイル**: `Doc/08_Organization/Active/Phase_B-F2/Step05_Web版検証・並列タスク実行.md`

**更新内容**:
- Stage3完了記録
- Stage4詳細計画（技術調査結果を基に確定）
- Stage5詳細計画（技術調査結果を基に確定）

### 3. Stage4実施判断

**判断ポイント**:
- 同一セッション内で継続可能か？（Context使用率確認）
- 次回セッションで実施すべきか？

---

## 🚨 No-Go判断時の次アクション（Claude Code実施）

### 1. Issue #51更新（No-Go記録）

**更新内容**:
- GitHub Codespaces検証結果（No-Go判断）
- No-Go理由の詳細記録
- 代替案の提案（C案: Self-hosted Runner等）

### 2. 代替案検討の開始

**検討対象**:
- C案（Self-hosted Runner）の詳細評価
- D案（Windows Server）の詳細評価
- Issue #51目標の見直し（スコープ縮小）

---

## 📝 セッション終了時の処理（Claude Code実施）

### 必須実施事項

1. **技術調査レポート完成確認**
   - `Codespaces技術調査結果.md` の全セクション完成
   - Go/No-Go判断完了

2. **Serenaメモリー更新**
   - `daily_sessions.md` 更新
   - `project_overview.md` 更新（Step5 Stage3完了記録）

3. **セッション終了Command実行**
   ```
   セッション終了
   ```
   - これにより`/session-end` Commandが自動実行されます

---

## 💡 Tips・注意事項

### Codespaces環境特有の注意点

1. **タイムアウト設定**
   - デフォルト30分は短すぎる
   - Settings → Codespaces → Default timeoutで240分（4時間）に変更推奨

2. **バックグラウンド実行の確認方法**
   - Codespacesタブを閉じる = ブラウザタブを閉じる
   - 再接続時に実行中のプロセスが継続していることを確認

3. **Context管理**
   - Codespaces環境でもContext管理は重要
   - `/context` コマンドで定期的に確認

### トラブルシューティング

**問題1: DevContainerが構築されない**
- 対処: `.devcontainer/devcontainer.json` の内容確認
- 対処: Codespacesログ確認（View → Output → Codespaces）

**問題2: MCP Serverが接続できない**
- 対処: `claude mcp list` で接続状況確認
- 対処: MCP Server設定ファイル確認（`.claude/mcp.json`）

**問題3: dotnet buildが失敗する**
- 対処: `dotnet restore` を先に実行
- 対処: エラーメッセージを詳細に記録

---

## ✅ チェックリスト

### 事前準備（このセッション）
- [x] 技術調査計画書テンプレート作成
- [x] 次回セッション実施手順書作成
- [ ] セッション終了処理（`/session-end`）

### 次回セッション開始時
- [ ] GitHub Codespaces起動
- [ ] Claude Code CLI起動
- [ ] セッション開始（`/session-start`）
- [ ] 技術調査開始指示

### 技術調査実施
- [ ] 調査項目1完了
- [ ] 調査項目2完了
- [ ] 調査項目3完了
- [ ] 調査項目4完了
- [ ] 調査項目5完了

### Go/No-Go判断
- [ ] 必須要件充足度評価完了
- [ ] Go/No-Go判断完了
- [ ] 次アクション決定

### Go判断時
- [ ] Issue #51更新完了
- [ ] Step05組織設計Stage4以降詳細化完了
- [ ] Stage4実施判断完了

### セッション終了
- [ ] 技術調査レポート完成
- [ ] Serenaメモリー更新完了
- [ ] セッション終了Command実行

---

**作成日**: 2025-11-10
**実施予定日**: 2025-11-XX（次回セッション）
**推定所要時間**: 2-3時間（技術調査のみ）、3-4時間（Go判断時の後続作業含む）
