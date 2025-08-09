# リポジトリのクローンと初期設定

## 1. 作業ディレクトリの作成

PowerShellで以下を実行：

```powershell
# 開発用ディレクトリの作成
New-Item -ItemType Directory -Path "C:\Develop" -Force
cd C:\Develop
```

## 2. リポジトリのクローン

```powershell
# リポジトリをクローン（URLは実際のリポジトリに置き換えてください）
git clone [リポジトリURL] ubiquitous-lang-mng
cd ubiquitous-lang-mng
```

## 3. VSCodeでプロジェクトを開く

```powershell
code .
```

## 4. NuGetパッケージの復元

VSCodeのターミナル（`Ctrl + `` ` ）で以下を実行：

```powershell
dotnet restore
```

## 5. 環境設定ファイルの作成

### appsettings.Development.json の設定

`src/UbiquitousLanguageManager.Web/appsettings.Development.json` を作成または編集：

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Debug",
      "Microsoft.AspNetCore": "Warning",
      "Microsoft.EntityFrameworkCore.Database.Command": "Information"
    }
  },
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=UbiquitousLanguageDB;Username=postgres;Password=postgres"
  },
  "EmailSettings": {
    "SmtpHost": "localhost",
    "SmtpPort": 5025,
    "SmtpUserName": "",
    "SmtpPassword": "",
    "SenderEmail": "noreply@example.com",
    "SenderName": "ユビキタス言語管理システム",
    "EnableSsl": false
  },
  "ApplicationSettings": {
    "DefaultAdminEmail": "admin@example.com",
    "DefaultAdminPassword": "Admin123!",
    "RequireEmailConfirmation": false,
    "MaxLoginAttempts": 5,
    "LockoutDurationMinutes": 15
  }
}
```

## 6. 環境変数の設定

PowerShellで以下を実行：

```powershell
# 開発環境の設定
[System.Environment]::SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Development", "User")

# 環境変数の確認
$env:ASPNETCORE_ENVIRONMENT
```

## 7. データベース初期化

プロジェクトルートで以下を実行：

```powershell
# データベースの作成とマイグレーション適用
dotnet ef database update --project src/UbiquitousLanguageManager.Infrastructure --startup-project src/UbiquitousLanguageManager.Web
```

## 8. ビルド確認

```powershell
# ソリューション全体のビルド
dotnet build
```

エラーが表示されなければ成功です。

## 9. 開発サーバーの起動確認

```powershell
# Webアプリケーションの起動
dotnet run --project src/UbiquitousLanguageManager.Web
```

以下のようなメッセージが表示されれば成功：
```
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: https://localhost:7001
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: http://localhost:5000
```

ブラウザで https://localhost:7001 にアクセスして動作確認。

`Ctrl + C` で開発サーバーを停止。

---
作成日: 2025-08-09