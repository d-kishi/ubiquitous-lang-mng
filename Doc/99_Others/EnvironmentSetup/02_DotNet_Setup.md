# .NET SDK インストール手順

## 1. .NET 8.0 SDK のダウンロード

1. 公式サイトにアクセス: https://dotnet.microsoft.com/download/dotnet/8.0
2. 「.NET 8.0 SDK」セクションを見つける
3. Windows x64用の「Installer」をクリックしてダウンロード

## 2. インストール

1. ダウンロードした`.exe`ファイルを実行
2. 「Install」をクリック
3. インストールが完了するまで待機
4. 「Close」をクリックして終了

## 3. インストール確認

PowerShellを**新しく開いて**以下のコマンドを実行：

```powershell
dotnet --version
```

8.0.x のバージョンが表示されれば成功です。

## 4. SDK一覧の確認

```powershell
dotnet --list-sdks
```

インストールされているSDKの一覧が表示されます。

## 5. HTTPS開発証明書の設定

開発環境でHTTPSを使用するための証明書を設定：

```powershell
dotnet dev-certs https --trust
```

確認ダイアログが表示されたら「はい」をクリック。

## 6. Entity Framework Core ツールのインストール

データベースマイグレーション用のツールをインストール：

```powershell
dotnet tool install --global dotnet-ef
```

インストール確認：

```powershell
dotnet ef --version
```

## 7. 環境変数の確認

PowerShellで以下を実行して、.NETのパスが通っていることを確認：

```powershell
$env:PATH -split ';' | Where-Object { $_ -like '*dotnet*' }
```

.NET関連のパスが表示されれば正常です。

---
作成日: 2025-08-09