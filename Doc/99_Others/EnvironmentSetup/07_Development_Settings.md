# 開発環境設定

## 1. プロジェクト固有の設定

### .vscode/settings.json の作成

プロジェクトルートに `.vscode` ディレクトリを作成し、`settings.json` を配置：

```json
{
  "editor.formatOnSave": true,
  "editor.tabSize": 4,
  "editor.insertSpaces": true,
  "files.trimTrailingWhitespace": true,
  "files.insertFinalNewline": true,
  "files.encoding": "utf8",
  
  "[csharp]": {
    "editor.defaultFormatter": "ms-dotnettools.csharp"
  },
  
  "[fsharp]": {
    "editor.defaultFormatter": "ionide.ionide-fsharp"
  },
  
  "[json]": {
    "editor.defaultFormatter": "vscode.json-language-features"
  },
  
  "[markdown]": {
    "editor.wordWrap": "on",
    "editor.quickSuggestions": {
      "comments": "off",
      "strings": "off",
      "other": "off"
    }
  },
  
  "omnisharp.enableRoslynAnalyzers": true,
  "omnisharp.enableEditorConfigSupport": true,
  
  "dotnet.server.useOmnisharp": false,
  
  "FSharp.suggestGitignore": false,
  "FSharp.formatOnSave": true,
  
  "terminal.integrated.defaultProfile.windows": "PowerShell",
  
  "files.exclude": {
    "**/bin": true,
    "**/obj": true,
    "**/.vs": true
  }
}
```

### .vscode/launch.json の作成

デバッグ設定ファイルを作成：

```json
{
  "version": "0.2.0",
  "configurations": [
    {
      "name": ".NET Core Launch (web)",
      "type": "coreclr",
      "request": "launch",
      "preLaunchTask": "build",
      "program": "${workspaceFolder}/src/UbiquitousLanguageManager.Web/bin/Debug/net8.0/UbiquitousLanguageManager.Web.dll",
      "args": [],
      "cwd": "${workspaceFolder}/src/UbiquitousLanguageManager.Web",
      "stopAtEntry": false,
      "serverReadyAction": {
        "action": "openExternally",
        "pattern": "\\bNow listening on:\\s+(https?://\\S+)"
      },
      "env": {
        "ASPNETCORE_ENVIRONMENT": "Development"
      },
      "sourceFileMap": {
        "/Views": "${workspaceFolder}/Views"
      }
    },
    {
      "name": ".NET Core Attach",
      "type": "coreclr",
      "request": "attach"
    }
  ]
}
```

### .vscode/tasks.json の作成

ビルドタスクの設定：

```json
{
  "version": "2.0.0",
  "tasks": [
    {
      "label": "build",
      "command": "dotnet",
      "type": "process",
      "args": [
        "build",
        "${workspaceFolder}/UbiquitousLanguageManager.sln",
        "/property:GenerateFullPaths=true",
        "/consoleloggerparameters:NoSummary"
      ],
      "problemMatcher": "$msCompile"
    },
    {
      "label": "publish",
      "command": "dotnet",
      "type": "process",
      "args": [
        "publish",
        "${workspaceFolder}/UbiquitousLanguageManager.sln",
        "/property:GenerateFullPaths=true",
        "/consoleloggerparameters:NoSummary"
      ],
      "problemMatcher": "$msCompile"
    },
    {
      "label": "watch",
      "command": "dotnet",
      "type": "process",
      "args": [
        "watch",
        "run",
        "--project",
        "${workspaceFolder}/src/UbiquitousLanguageManager.Web/UbiquitousLanguageManager.Web.csproj"
      ],
      "problemMatcher": "$msCompile"
    },
    {
      "label": "test",
      "command": "dotnet",
      "type": "process",
      "args": [
        "test",
        "${workspaceFolder}/UbiquitousLanguageManager.sln"
      ],
      "problemMatcher": "$msCompile"
    }
  ]
}
```

## 2. EditorConfig の設定

プロジェクトルートに `.editorconfig` ファイルを作成：

```ini
root = true

[*]
charset = utf-8
end_of_line = crlf
indent_style = space
indent_size = 4
insert_final_newline = true
trim_trailing_whitespace = true

[*.{cs,fs,fsx}]
indent_size = 4

[*.{json,yml,yaml}]
indent_size = 2

[*.md]
trim_trailing_whitespace = false

[*.{csproj,fsproj,props}]
indent_size = 2
```

## 3. Git設定

### .gitignore の確認

プロジェクトルートの `.gitignore` に以下が含まれているか確認：

```gitignore
## Visual Studio Code
.vscode/*
!.vscode/settings.json
!.vscode/tasks.json
!.vscode/launch.json
!.vscode/extensions.json

## .NET
bin/
obj/
*.user
*.userosscache
*.sln.docstates

## NuGet
*.nupkg
*.snupkg
**/packages/*
!**/packages/build/

## Visual Studio
.vs/
*.suo
*.bak
*.cache

## User-specific files
appsettings.Development.json
appsettings.Production.json
```

## 4. 開発用スクリプトの作成

プロジェクトルートに `scripts` ディレクトリを作成し、便利なスクリプトを配置：

### scripts/start-dev.ps1

```powershell
# 開発環境起動スクリプト
Write-Host "Starting development environment..." -ForegroundColor Green

# Docker起動
Write-Host "Starting Docker containers..." -ForegroundColor Yellow
docker-compose up -d

# 待機
Start-Sleep -Seconds 5

# 開発サーバー起動
Write-Host "Starting development server..." -ForegroundColor Yellow
dotnet run --project src/UbiquitousLanguageManager.Web
```

### scripts/reset-db.ps1

```powershell
# データベースリセットスクリプト
Write-Host "Resetting database..." -ForegroundColor Yellow

# データベース削除
dotnet ef database drop --project src/UbiquitousLanguageManager.Infrastructure --startup-project src/UbiquitousLanguageManager.Web --force

# マイグレーション適用
dotnet ef database update --project src/UbiquitousLanguageManager.Infrastructure --startup-project src/UbiquitousLanguageManager.Web

Write-Host "Database reset completed!" -ForegroundColor Green
```

### scripts/run-tests.ps1

```powershell
# テスト実行スクリプト
Write-Host "Running all tests..." -ForegroundColor Green

# テスト実行
dotnet test --logger "console;verbosity=normal"

# カバレッジレポート（必要に応じて）
# dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=opencover
```

## 5. HTTPS開発証明書のセットアップ

**対象**: Phase B-F2以降（DevContainer環境）
**関連ADR**: ADR_026（DevContainer HTTPS証明書管理方針）
**詳細ガイド**: `Doc/99_Others/DevContainer使用ガイド.md`

### 概要

DevContainer環境でHTTPS通信を有効にするため、ホスト環境で開発用SSL証明書を生成します。この証明書はDevContainerにボリュームマウントで共有され、DevContainer再構築後も永続化されます。

### セットアップ手順（初回のみ）

#### Windows環境

PowerShellまたはGit Bashで実行：

```bash
# 証明書保存ディレクトリ作成
mkdir -p $USERPROFILE/.aspnet/https

# 既存証明書クリーンアップ
dotnet dev-certs https --clean

# 証明書生成（PFX形式、パスワード付き）
dotnet dev-certs https -ep $USERPROFILE/.aspnet/https/aspnetapp.pfx -p DevPassword123

# ホスト環境での信頼設定（ブラウザ証明書警告回避）
dotnet dev-certs https --trust
```

#### macOS環境

Terminalで実行：

```bash
# 証明書保存ディレクトリ作成
mkdir -p ~/.aspnet/https

# 既存証明書クリーンアップ
dotnet dev-certs https --clean

# 証明書生成（PFX形式、パスワード付き）
dotnet dev-certs https -ep ~/.aspnet/https/aspnetapp.pfx -p DevPassword123

# ホスト環境での信頼設定
dotnet dev-certs https --trust
```

#### Linux環境

Bashで実行：

```bash
# 証明書保存ディレクトリ作成
mkdir -p ~/.aspnet/https

# 既存証明書クリーンアップ
dotnet dev-certs https --clean

# 証明書生成（PFX形式、パスワード付き）
dotnet dev-certs https -ep ~/.aspnet/https/aspnetapp.pfx -p DevPassword123

# Linuxでは --trust オプション非対応
# ブラウザで手動承認が必要（初回アクセス時）
```

### 証明書情報

- **ファイルパス**:
  - Windows: `C:\Users\<username>\.aspnet\https\aspnetapp.pfx`
  - macOS/Linux: `~/.aspnet/https/aspnetapp.pfx`
- **ファイルサイズ**: 約2.6KB
- **証明書パスワード**: `DevPassword123`（開発環境専用）
- **有効期限**: 1年間（生成日から365日）
- **証明書用途**: localhost専用（https://localhost:5001）
- **本番環境使用**: 禁止（別の証明書管理方式を使用）

### DevContainerでの利用

**仕組み**: ボリュームマウント + 環境変数方式（Microsoft公式推奨）

DevContainerは以下の設定により、ホスト環境の証明書を自動的に利用します：

1. **ボリュームマウント**（`.devcontainer/devcontainer.json`）:
   - ホスト環境の証明書ディレクトリをDevContainerにマウント
   - 読み取り専用で共有（誤って証明書削除防止）

2. **環境変数設定**（`.devcontainer/devcontainer.json`）:
   - `ASPNETCORE_Kestrel__Certificates__Default__Path`: 証明書ファイルパス
   - `ASPNETCORE_Kestrel__Certificates__Default__Password`: 証明書パスワード

3. **証明書検証スクリプト**（`.devcontainer/scripts/setup-https.sh`）:
   - DevContainer起動時（`postCreateCommand`）に自動実行
   - 証明書存在チェック
   - 証明書未作成時のわかりやすいエラーメッセージ表示

**メリット**:
- ✅ DevContainer再構築で証明書が失われない（永続化）
- ✅ 環境再現性の確保（新規開発者も同じ手順）
- ✅ 自動化（postCreateCommandで検証）

### 証明書有効期限と更新

**有効期限**: 1年間（365日）

**有効期限切れ時の症状**:
```
System.InvalidOperationException: 'Unable to configure HTTPS endpoint. The certificate is expired.'
```

**更新手順**（2-3分）:

1. ホスト環境で証明書再生成（上記のセットアップ手順を再実行）
2. DevContainer再起動またはアプリ再起動で証明書再読み込み

### トラブルシューティング

#### 問題1: 証明書エラー（Unable to configure HTTPS endpoint）

**原因**: ホスト環境で証明書未作成

**対処法**: 上記のセットアップ手順を実施し、DevContainer再構築

#### 問題2: ブラウザ証明書警告

**原因**: ホスト環境で証明書の信頼設定未実施

**対処法**:
```bash
# ホスト環境で実行
dotnet dev-certs https --trust
```

**詳細なトラブルシューティング**: `Doc/99_Others/DevContainer使用ガイド.md` → [6. トラブルシューティング](DevContainer使用ガイド.md#6-トラブルシューティング) 参照

---

## 6. 使用方法

開発を開始する際の手順：

1. PowerShellを開く
2. プロジェクトディレクトリに移動
3. 開発環境を起動：
   ```powershell
   .\scripts\start-dev.ps1
   ```

---
作成日: 2025-08-09
最終更新: 2025-11-04（HTTPS開発証明書セクション追加）