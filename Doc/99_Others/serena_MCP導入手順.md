# Serena MCP Server 導入手順

## 概要

Serena（https://github.com/oraios/serena）は、IDE統合型のAIアシスタントで、Claude CodeでMCP（Model Context Protocol）サーバーとして利用できます。

## 導入手順

### 1. 前提条件確認

```bash
# Python環境確認（必要に応じてインストール）
python --version

# Git確認
git --version
```

### 2. uv（Python パッケージマネージャー）のインストール

#### Windowsの場合

**PowerShellでの方法（推奨）：**
```powershell
iwr https://astral.sh/uv/install.ps1 -useb | iex
```

**Bash（Git Bash等）での方法：**
```bash
curl -LsSf https://astral.sh/uv/install.sh | sh
```

### 3. PATH設定（Git Bash利用時）

Claude CodeはGit Bashを使用するため、以下の設定が必要：

```bash
# ~/.bashrcに追加（恒久的解決）
echo 'export PATH="$HOME/.local/bin:$PATH"' >> ~/.bashrc

# 現在のセッションに適用
source ~/.bashrc
```

### 4. Claude CodeへのMCP追加

プロジェクトディレクトリで以下のコマンドを実行：

```bash
# プロジェクトディレクトリに移動
cd /path/to/your/project

# Claude CodeにSerena MCPサーバーを追加
claude mcp add serena -- uvx --from git+https://github.com/oraios/serena serena start-mcp-server --context ide-assistant --project $(pwd)
```

### 5. インストール確認

```bash
# uvx が利用可能か確認
uvx --version

# MCP接続状態確認
claude mcp list
```

### 6. Serenaの自動設定

Serena MCPサーバーは、プロジェクト初回接続時に以下を自動作成：

- `.serena/project.yml` - プロジェクト設定ファイル
- プロジェクト固有の設定（言語：C#、GitIgnore連携等）

## トラブルシューティング

### 接続失敗時の対処法

1. **uvxが見つからない**
   ```bash
   which uvx  # パスを確認
   source $HOME/.local/bin/env  # 一時的解決
   ```

2. **PowerShellとGit Bashの環境差異**
   - PowerShellでインストール成功でも、Git Bashで利用不可の場合
   - `~/.bashrc`への PATH 追加が必要

3. **プロセス起動状態確認**
   ```bash
   # serenaプロセス確認
   powershell -Command "Get-Process '*serena*' -ErrorAction SilentlyContinue"
   ```

### MCP接続確認コマンド

```bash
# 接続状態確認
claude mcp list

# 正常な出力例：
# serena: uvx --from git+https://github.com/oraios/serena serena start-mcp-server --context ide-assistant --project [PROJECT_PATH] - ✓ Connected
```

## Serenaの主要機能

- **コード解析**: シンボル検索、参照検索
- **ファイル操作**: 読み込み、書き込み、編集
- **プロジェクト管理**: 設定管理、メモリ機能
- **シェルコマンド実行**: 統合された環境での実行

## 設定ファイル（.serena/project.yml）の説明

```yaml
language: csharp                      # プロジェクト言語
ignore_all_files_in_gitignore: true  # GitIgnore連携
read_only: false                      # 編集可能モード
excluded_tools: []                    # 除外ツール（通常は空）
project_name: "ubiquitous-lang-mng"   # プロジェクト名
```

## メンテナンス

### 設定リセット

```bash
# 設定ファイル削除（再生成される）
rm -rf .serena/
```

### アップデート

uvxを通じて自動的に最新版が利用されるため、手動アップデートは不要です。

## 参考情報

- Serena GitHub: https://github.com/oraios/serena
- uv 公式サイト: https://astral.sh/uv/
- Claude Code MCP 設定: 環境に応じて自動設定

---

**作成日**: 2025-08-08  
**最終更新**: 2025-08-08  
**対象環境**: Windows 10/11, Git Bash, Claude Code