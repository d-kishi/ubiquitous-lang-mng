# Serena MCP セットアップ手順

## 1. 前提条件

- Python 3.8以降がインストールされていること
- pipが使用可能であること
- Claude Codeがインストール済みであること

### Python のインストール（未インストールの場合）

1. 公式サイトにアクセス: https://www.python.org/downloads/
2. Windows用の最新版をダウンロード
3. インストーラーを実行
   - ☑ Add Python to PATH にチェック
   - Install Now をクリック
4. PowerShellで確認：
   ```powershell
   python --version
   pip --version
   ```

## 2. uvx のインストール

uvxはPythonパッケージを独立した環境で実行するツールです：

```powershell
# pipをアップグレード
python -m pip install --upgrade pip

# uvをインストール
pip install uv

# インストール確認
uv --version
```

## 3. Serena MCP のインストール

```powershell
# Serena MCPをインストール（GitHubから直接）
uv tool install git+https://github.com/oraios/serena

# インストール確認
uvx --from git+https://github.com/oraios/serena serena --version
```

## 4. Claude Desktop 設定

### 設定ファイルの作成

ユーザーディレクトリに設定ファイルを作成：

```powershell
# .claudeディレクトリ作成
New-Item -ItemType Directory -Path "$env:USERPROFILE\.claude" -Force

# 設定ファイル作成
New-Item -ItemType File -Path "$env:USERPROFILE\.claude\claude_desktop_config.json" -Force
```

### claude_desktop_config.json の内容

`%USERPROFILE%\.claude\claude_desktop_config.json` を以下の内容で編集：

```json
{
  "mcpServers": {
    "serena": {
      "command": "uvx",
      "args": [
        "--from",
        "git+https://github.com/oraios/serena",
        "serena",
        "start-mcp-server",
        "--context",
        "ide-assistant",
        "--project",
        "C:\\Develop\\ubiquitous-lang-mng"
      ],
      "env": {}
    }
  }
}
```

**注意**: `"project"` のパスは実際のプロジェクトパスに合わせて変更してください。

## 5. Serena プロジェクト設定

プロジェクトルートに `.serena` ディレクトリを作成：

```powershell
# プロジェクトルートに移動
cd C:\Develop\ubiquitous-lang-mng

# .serenaディレクトリ作成
New-Item -ItemType Directory -Path ".serena" -Force
```

### .serena/config.json の作成

```json
{
  "project_name": "ユビキタス言語管理システム",
  "language": "csharp",
  "frameworks": ["dotnet", "blazor", "fsharp"],
  "context": "ide-assistant",
  "features": {
    "code_analysis": true,
    "refactoring": true,
    "test_generation": true,
    "documentation": true
  }
}
```

## 6. 接続確認

### Claude Code から確認

```powershell
# Claude Codeで MCP サーバーリストを確認
claude mcp list
```

以下のような出力が表示されれば成功：
```
serena: uvx --from git+https://github.com/oraios/serena serena start-mcp-server --context ide-assistant --project C:\Develop\ubiquitous-lang-mng - ✓ Connected
```

### 手動テスト

```powershell
# Serena MCPサーバーを手動起動してテスト
uvx --from git+https://github.com/oraios/serena serena start-mcp-server --context ide-assistant --project .
```

`Ctrl+C` で停止。

## 7. Serena MCP の主な機能

### 利用可能なツール

Serena MCPは以下のツールを提供：

- `mcp__serena__find_symbol`: シンボル検索
- `mcp__serena__replace_symbol_body`: シンボル本体の置換
- `mcp__serena__get_symbols_overview`: シンボル概要取得
- `mcp__serena__find_referencing_symbols`: 参照シンボル検索
- `mcp__serena__search_for_pattern`: パターン検索
- `mcp__serena__initial_instructions`: 初期指示取得

### 使用例

Claude Codeセッション内で：

```
「mcp__serena__find_symbol でUserRepositoryクラスを検索してください」
「mcp__serena__get_symbols_overview でプロジェクト全体の構造を確認してください」
```

## 8. トラブルシューティング

### uvxが見つからない場合

```powershell
# Pythonスクリプトディレクトリをパスに追加
$pythonScripts = python -c "import site; print(site.USER_BASE + '\\Scripts')"
[System.Environment]::SetEnvironmentVariable("PATH", "$env:PATH;$pythonScripts", "User")

# PowerShellを再起動
```

### Serena MCPが接続できない場合

1. ファイアウォール設定を確認
2. ウイルス対策ソフトの除外設定を確認
3. プロジェクトパスが正しいか確認

### ログの確認

```powershell
# Claude Codeのログを確認
claude logs

# Serenaの詳細ログを有効化
$env:SERENA_DEBUG = "true"
```

## 9. アップデート方法

定期的にSerena MCPをアップデート：

```powershell
# Serena MCPを最新版に更新
uv tool upgrade serena

# または再インストール
uv tool uninstall serena
uv tool install git+https://github.com/oraios/serena
```

---
作成日: 2025-08-09