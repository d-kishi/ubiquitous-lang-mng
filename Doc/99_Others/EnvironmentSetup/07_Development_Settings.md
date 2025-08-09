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

## 5. 使用方法

開発を開始する際の手順：

1. PowerShellを開く
2. プロジェクトディレクトリに移動
3. 開発環境を起動：
   ```powershell
   .\scripts\start-dev.ps1
   ```

---
作成日: 2025-08-09