# Claude Code セットアップ手順

## 1. 前提条件

- Node.js 18 以降がインストールされていること
- npm または yarn が使用可能であること

### Node.js のインストール（未インストールの場合）

1. 公式サイトにアクセス: https://nodejs.org/
2. LTS 版をダウンロード
3. インストーラーを実行（デフォルト設定で OK）
4. PowerShell で確認：
   ```powershell
   node --version
   npm --version
   ```

## 2. Claude Code CLI のインストール

PowerShell（管理者権限）で以下を実行：

```powershell
# Claude Code CLIのグローバルインストール
npm install -g @anthropic-ai/claude-code

# インストール確認
claude --version
```

## 3. 認証設定

### API キーの取得

1. https://console.anthropic.com/ にアクセス
2. ログインまたはアカウント作成
3. API Keys セクションで API キーを生成

### API キーの設定

```powershell
# APIキーを環境変数に設定
[System.Environment]::SetEnvironmentVariable("ANTHROPIC_API_KEY", "your-api-key-here", "User")

# 設定確認
$env:ANTHROPIC_API_KEY
```

## 4. Claude Code の初期設定

プロジェクトルートで以下を実行：

```powershell
# Claude Code初期化
claude init
```

設定プロンプトで以下を選択：

- Model: claude-3-opus（または claude-3.5-sonnet）
- Context window: 200000
- Output format: markdown

## 5. プロジェクト固有の設定

### .claude ディレクトリの作成

```powershell
# .claudeディレクトリ作成
New-Item -ItemType Directory -Path ".claude" -Force
```

### .claude/settings.local.json の作成

既存の設定ファイルをコピーまたは以下の内容で作成：

```json
{
  "permissions": {
    "allow": [
      "Bash(mkdir:*)",
      "WebFetch(domain:docs.anthropic.com)",
      "Bash(rg:*)",
      "Bash(find:*)",
      "Bash(mv:*)",
      "Bash(gemini:*)",
      "Bash(grep:*)",
      "Bash(docker:*)",
      "Bash(ls:*)",
      "Bash(diff:*)",
      "Bash(rm:*)",
      "Bash(dotnet new:*)",
      "Bash(dotnet sln add:*)",
      "Bash(dotnet build:*)",
      "Bash(dotnet run:*)",
      "Bash(dotnet restore:*)",
      "Bash(dotnet test:*)",
      "Bash(dotnet ef:*)",
      "Bash(dotnet tool:*)",
      "Bash(dotnet add:*)",
      "Bash(git:*)",
      "Bash(code:*)",
      "Bash(powershell:*)",
      "Bash(curl:*)",
      "Bash(echo:*)",
      "WebFetch(domain:github.com)",
      "mcp__serena__*"
    ],
    "deny": []
  }
}
```

### .claude/agents ディレクトリの作成と設定

```powershell
# agentsディレクトリ作成
New-Item -ItemType Directory -Path ".claude/agents" -Force

# commandsディレクトリ作成
New-Item -ItemType Directory -Path ".claude/commands" -Force
```

## 6. CLAUDE.md の確認

プロジェクトルートの `CLAUDE.md` ファイルが存在することを確認。
このファイルには Claude Code への指示が記載されています。

## 7. 使用方法

### 基本的な使用

```powershell
# Claude Codeを起動
claude

# 特定のファイルを開いて起動
claude --file src/UbiquitousLanguageManager.Web/Program.cs

# プロジェクト全体のコンテキストで起動
claude --context .
```

### セッション管理

Claude Code とのセッション開始時：

1. 「セッションを開始します」と宣言
2. 自動的にセッション開始プロセスが実行される

セッション終了時：

1. 「セッション終了」と宣言
2. 自動的にセッション終了プロセスが実行される

## 8. トラブルシューティング

### API キーが認識されない場合

```powershell
# 環境変数を再読み込み
$env:ANTHROPIC_API_KEY = [System.Environment]::GetEnvironmentVariable("ANTHROPIC_API_KEY", "User")
```

### コマンドが見つからない場合

```powershell
# npmのパスを確認
npm config get prefix

# パスを環境変数に追加
$npmPath = npm config get prefix
[System.Environment]::SetEnvironmentVariable("PATH", "$env:PATH;$npmPath", "User")
```

### PowerShell を再起動して確認

```powershell
claude --help
```

---

作成日: 2025-08-09
