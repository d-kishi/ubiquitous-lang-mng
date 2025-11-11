# MCP設定メンテナンスガイド

**作成日**: 2025-11-11
**最終更新**: 2025-11-11
**対象**: GitHub Codespaces + DevContainer環境におけるMCP設定の継続的メンテナンス

## 📋 目次

1. [概要](#概要)
2. [MCPサーバー追加手順](#mcpサーバー追加手順)
3. [MCPサーバー削除手順](#mcpサーバー削除手順)
4. [MCPサーバー更新手順](#mcpサーバー更新手順)
5. [週次振り返り時の確認項目](#週次振り返り時の確認項目)
6. [トラブルシューティング](#トラブルシューティング)
7. [関連ドキュメント](#関連ドキュメント)

---

## 概要

### MCP設定の構成要素

本プロジェクトでは、以下のファイルでMCP環境を管理しています：

| ファイル | 役割 | Git管理 | 更新頻度 |
|---------|------|---------|---------|
| `.mcp.json` | MCPサーバー設定（チーム共有） | ✅ 対象 | MCPサーバー追加/削除/更新時 |
| `.devcontainer/scripts/setup-mcp.sh` | 自動セットアップスクリプト | ✅ 対象 | 依存ツール変更時 |
| `.devcontainer/Dockerfile` | 依存ツールインストール | ✅ 対象 | 依存ツール追加時 |
| `.serena/project.yml` | Serenaプロジェクト設定 | ✅ 対象 | Serena設定変更時 |
| `~/.serena/serena_config.yml` | Serenaユーザー設定 | ❌ 除外 | ユーザー個別設定 |

### 自動設定の仕組み

```
DevContainer起動
    ↓
postCreateCommand実行
    ↓
setup-https.sh実行
    ↓
setup-mcp.sh実行 ← ここでMCP環境を自動構築
    ↓
dotnet restore実行
    ↓
Claude Code CLI起動可能（/mcpでSerena/Playwright利用可能）
```

---

## MCPサーバー追加手順

### Step 1: MCPサーバーの調査

**必要な情報を収集**:
1. **コマンド**: MCPサーバー起動コマンド（例: `npx`, `uvx`, `node`）
2. **引数**: コマンドライン引数（例: `--from git+https://...`）
3. **依存ツール**: 必要なツール（例: Node.js, Python, uv）
4. **環境変数**: 必要な環境変数（例: APIキー）

### Step 2: `.mcp.json`に設定追加

**編集ファイル**: `.mcp.json`

**追加例**:
```json
{
  "mcpServers": {
    "serena": {
      "command": "uvx",
      "args": ["--from", "git+https://github.com/oraios/serena", "serena-mcp-server", "--context", "ide-assistant", "--project", "/workspace"]
    },
    "playwright": {
      "command": "npx",
      "args": ["-y", "@playwright/mcp@latest"]
    },
    "new-mcp-server": {
      "command": "コマンド",
      "args": ["引数1", "引数2"],
      "env": {
        "ENV_VAR": "${環境変数名}"
      }
    }
  }
}
```

**注意事項**:
- `command`: 実行可能なコマンド（PATH通っている必要あり）
- `args`: 配列形式で引数を指定
- `env`: 環境変数が必要な場合のみ指定
- 環境変数展開: `${VAR}` 形式で記述可能

### Step 3: 依存ツールのインストール（必要な場合）

#### Dockerfileに追加が必要な場合

**編集ファイル**: `.devcontainer/Dockerfile`

**例: 新しいnpmパッケージが必要な場合**:
```dockerfile
# グローバルnpmパッケージインストール
RUN npm install -g \
    # Playwright（E2Eテスト）
    @playwright/test \
    playwright \
    # 新しいパッケージ
    new-mcp-package
```

**例: 新しいPythonツールが必要な場合**:
```dockerfile
# Python uv インストール（Serena MCP用）
# 注: グローバルインストール（全ユーザーから利用可能）
RUN curl -LsSf https://astral.sh/uv/install.sh | sh \
    && mv /root/.local/bin/uv /usr/local/bin/uv \
    && mv /root/.local/bin/uvx /usr/local/bin/uvx
```

#### setup-mcp.shに検証ロジック追加（推奨）

**編集ファイル**: `.devcontainer/scripts/setup-mcp.sh`

**追加場所**: Step 3（npxコマンド確認）の後

**例**:
```bash
# 4. 新しいツールのコマンド確認
echo ""
echo "🔧 Checking new-tool..."
if command -v new-tool &> /dev/null; then
  success "new-tool is installed"
  new-tool --version
else
  error_exit "new-tool is not installed. Please install it."
fi
```

### Step 4: 動作確認

#### ローカル環境での確認

1. **DevContainer再ビルド**:
   ```
   VS Code: Cmd/Ctrl + Shift + P → "Dev Containers: Rebuild Container"
   ```

2. **スクリプトログ確認**:
   ```
   🔌 MCP Server Setup for Claude Code CLI
   ✅ MCP configuration found: /workspace/.mcp.json
   ✅ uv is installed
   ✅ npx is installed
   ✅ new-tool is installed ← 新しいツール確認
   ✅ MCP Server Setup Complete!
   ```

3. **Claude Code CLI起動・確認**:
   ```bash
   cd /workspace
   claude
   # Claude Code CLI内で
   /mcp
   # → new-mcp-serverが表示されることを確認
   ```

#### GitHub Codespaces環境での確認

1. **Codespaces再ビルド** or **新規Codespace作成**
2. 同様の手順で動作確認

### Step 5: Git commit・Push

```bash
git add .mcp.json
git add .devcontainer/Dockerfile  # 依存ツール追加した場合
git add .devcontainer/scripts/setup-mcp.sh  # 検証ロジック追加した場合
git commit -m "feat: 新MCPサーバー追加（new-mcp-server）"
git push
```

---

## MCPサーバー削除手順

### Step 1: `.mcp.json`から設定削除

**編集ファイル**: `.mcp.json`

該当するMCPサーバー設定を削除:
```json
{
  "mcpServers": {
    "serena": { ... },
    "playwright": { ... }
    // "削除するMCPサーバー": { ... } ← 削除
  }
}
```

### Step 2: setup-mcp.shから検証ロジック削除（追加していた場合）

**編集ファイル**: `.devcontainer/scripts/setup-mcp.sh`

該当するツール確認ロジックを削除（オプション）

### Step 3: 依存ツール削除検討

**注意**: 他のMCPサーバーが同じツールを使用していないか確認してから削除

**編集ファイル**: `.devcontainer/Dockerfile`

不要になった依存ツールを削除（慎重に実施）

### Step 4: 動作確認・Git commit

ローカル/Codespaces環境で動作確認後、Git commit・Push

---

## MCPサーバー更新手順

### パターンA: バージョン固定なし（`@latest`使用）

**対象**: Playwright MCP等、常に最新版を使用するMCPサーバー

**設定例**:
```json
{
  "mcpServers": {
    "playwright": {
      "command": "npx",
      "args": ["-y", "@playwright/mcp@latest"]
    }
  }
}
```

**更新方法**: 特別な作業不要（DevContainer再ビルド時に最新版取得）

### パターンB: Gitリポジトリ参照

**対象**: Serena MCP等、Gitリポジトリから直接取得するMCPサーバー

**設定例**:
```json
{
  "mcpServers": {
    "serena": {
      "command": "uvx",
      "args": [
        "--from",
        "git+https://github.com/oraios/serena",
        "serena-mcp-server",
        ...
      ]
    }
  }
}
```

**更新方法**: 特別な作業不要（DevContainer再ビルド時に最新版取得）

**特定バージョン固定したい場合**:
```json
"args": [
  "--from",
  "git+https://github.com/oraios/serena@v1.2.3",  // ← タグ/コミットハッシュ指定
  "serena-mcp-server",
  ...
]
```

### パターンC: バージョン固定あり

**設定例**:
```json
{
  "mcpServers": {
    "example": {
      "command": "npx",
      "args": ["-y", "example-mcp@1.2.3"]  // ← バージョン固定
    }
  }
}
```

**更新方法**:
1. `.mcp.json`のバージョン番号を更新
2. Git commit・Push
3. DevContainer再ビルド

---

## 週次振り返り時の確認項目

### 確認タイミング

`/weekly-retrospective` Command実行時（週次振り返り時）

### 確認手順

#### 1. Playwright MCPバージョン確認

```bash
# 最新バージョン確認
npm view @playwright/mcp version

# 現在使用中のバージョン確認（DevContainer内で実行）
npx @playwright/mcp@latest --version
```

**結果記録例**:
```markdown
## MCP更新確認

### Playwright MCP
- **現在バージョン**: 1.48.2
- **最新バージョン**: 1.48.2
- **更新の要否**: 不要（最新）
```

#### 2. Serena MCPバージョン確認

```bash
# GitHubリリース確認
gh api repos/oraios/serena/releases/latest | jq -r '.tag_name'
```

**結果記録例**:
```markdown
### Serena MCP
- **現在バージョン**: main branch（最新コミット参照）
- **最新リリース**: v0.8.5
- **更新の要否**: 不要（Gitリポジトリから常に最新取得）
```

#### 3. ツール変更検出

```bash
# Playwright MCPツール一覧取得
echo '{"jsonrpc": "2.0", "id": 1, "method": "tools/list"}' \
  | npx @playwright/mcp@latest \
  | jq '.result.tools[].name'
```

**結果記録例**:
```markdown
### ツール変更検出
- **Playwright MCP**: 21ツール（前回から変更なし）
- **新規ツール**: なし
- **廃止ツール**: なし
- **非推奨ツール**: なし
```

#### 4. 更新判断

**更新が必要な場合**:
- 重要なバグ修正
- セキュリティパッチ
- 新機能追加（必要な場合）

**更新不要な場合**:
- マイナーアップデート（破壊的変更なし）
- 現在のバージョンで問題なく動作

### 週次振り返りドキュメントへの記録

**記録場所**: `Doc/04_Daily/YYYY-MM/週次総括_YYYY-WXX.md`

**記録テンプレート**:
```markdown
## 技術基盤メンテナンス

### MCP設定更新確認

#### Playwright MCP
- **現在バージョン**: X.XX.X
- **最新バージョン**: X.XX.X
- **更新の要否**: 不要/必要
- **更新実施**: 実施済み/次週実施予定

#### Serena MCP
- **現在設定**: main branch（最新コミット参照）
- **最新リリース**: vX.X.X
- **更新の要否**: 不要/必要
- **更新実施**: 実施済み/次週実施予定

#### ツール変更検出
- **Playwright MCP**: XXツール（前回からXX追加、XX廃止）
- **影響範囲**: なし/SubAgent定義更新必要
```

---

## トラブルシューティング

### エラー: `uvx: command not found`

**原因**: `uv`がインストールされていない

**解決策**:
```bash
# DevContainer内で実行
curl -LsSf https://astral.sh/uv/install.sh | sh
export PATH="$HOME/.local/bin:$PATH"

# または、DevContainer再ビルド（Dockerfileで自動インストール）
```

### エラー: `spawn npx ENOENT`

**原因**: `.vscode/settings.json`にMCP設定が記述されている

**解決策**: `.mcp.json`に移行し、`.vscode/settings.json`からMCP設定を削除

### エラー: `Invalid JSON syntax in .mcp.json`

**原因**: `.mcp.json`のJSON構文エラー

**解決策**:
```bash
# JSON構文チェック
jq empty .mcp.json

# エラー箇所特定
jq . .mcp.json
```

**よくあるJSON構文エラー**:
- 末尾のカンマ（Trailing comma）
- クォートの不一致
- 括弧の不一致

### Serena MCPが動作しない

**確認項目**:
1. `.serena/project.yml`が存在するか
2. `/workspace`パスが正しいか（DevContainer内のパス）
3. Serenaのログを確認: `~/.serena/logs/`

**解決策**:
```bash
# Serena設定確認
cat /workspace/.serena/project.yml

# Serenaログ確認
ls -la ~/.serena/logs/
cat ~/.serena/logs/latest.log
```

### Claude Code CLI認証エラー

**原因**: Codespaces初回起動時、認証が必要

**解決策**:
```bash
claude
# → ブラウザで認証リンクを開いてログイン
```

### ⚠️ ブラウザ版VSCodeでのSerena起動問題（未検証）

**症状**: Claude Code CLI起動時、プロンプトが表示されない（5分以上待機）

**推定原因**:
- Serena MCPはダッシュボード表示のためブラウザウィンドウを開く
- ブラウザ版VSCodeではポップアップブロック等で新しいブラウザウィンドウを開けない
- ダッシュボード表示失敗 → Serena初期化が完了しない → プロンプト表示されない

**回避策（検証済み）**:
1. VS Code（デスクトップ版）を使用
   ```
   Command Palette (Ctrl+Shift+P) → "Codespaces: Connect to Codespace"
   ```
2. VS Code（デスクトップ版）では正常動作（1-3分で起動完了）

**検証状況**:
- ✅ VS Code（デスクトップ版）: 正常動作確認済み
- ❌ ブラウザ版VSCode: 未検証（要追加調査）

**追加検証が必要な場合**:
- ブラウザのポップアップブロック設定確認
- Serenaダッシュボード表示の無効化オプション調査
- ブラウザ開発者コンソールでエラー確認

**関連情報**: `Doc/08_Organization/Active/Phase_B-F2/Research/Codespaces技術調査結果.md` 調査項目2

---

## 関連ドキュメント

### 技術調査レポート

- **Phase B-F2 調査レポート**: `Doc/08_Organization/Active/Phase_B-F2/Research/Codespaces技術調査結果.md`
  - 調査項目1: DevContainer + Claude Code CLI統合
  - 調査項目2: MCP Server接続確認
  - 実装方法の比較検討

### ADR

- **ADR_024**: Playwright専用SubAgent新設決定
  - MCPメンテナンス手順の詳細
  - 週次振り返り連携方法

### 既存パターン

- **HTTPS証明書設定**: `.devcontainer/scripts/setup-https.sh`
  - 同様のアプローチでMCP設定を実装
  - postCreateCommand連携パターン

---

**最終更新日**: 2025-11-11
**メンテナンス担当**: プロジェクトチーム全員
**質問・問題報告**: GitHub Issues
