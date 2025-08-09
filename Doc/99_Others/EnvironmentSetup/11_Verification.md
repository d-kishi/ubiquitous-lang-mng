# 環境構築の動作確認

## 1. 基本ツールの確認

PowerShellを開いて、各ツールがインストールされているか確認：

```powershell
# Gitバージョン確認
git --version
# 期待値: git version 2.x.x

# .NET SDKバージョン確認
dotnet --version
# 期待値: 8.0.x

# Dockerバージョン確認
docker --version
# 期待値: Docker version 2x.x.x

# Docker Composeバージョン確認
docker-compose --version
# 期待値: Docker Compose version v2.x.x

# VSCodeバージョン確認
code --version
# 期待値: 1.x.x

# Node.jsバージョン確認
node --version
# 期待値: v18.x.x以上

# Pythonバージョン確認
python --version
# 期待値: Python 3.x.x
```

## 2. リポジトリの確認

```powershell
# プロジェクトディレクトリに移動
cd C:\Develop\ubiquitous-lang-mng

# Gitステータス確認
git status

# ブランチ確認
git branch -a
```

## 3. Docker環境の確認

```powershell
# Docker起動確認
docker ps

# コンテナ起動
docker-compose up -d

# 起動したコンテナの確認
docker ps
# postgresql と smtp4dev のコンテナが表示されるはず

# PostgreSQL接続テスト
docker exec -it ubiquitous-lang-postgres psql -U postgres -d UbiquitousLanguageDB -c "\dt"

# SMTP4Dev Web UI確認
Start-Process "http://localhost:5080"
```

## 4. .NET プロジェクトのビルド確認

```powershell
# パッケージ復元
dotnet restore

# ビルド実行
dotnet build
# 期待値: Build succeeded. 0 Warning(s) 0 Error(s)

# テスト実行
dotnet test
# 期待値: すべてのテストが成功
```

## 5. 開発サーバーの起動確認

```powershell
# 開発サーバー起動
dotnet run --project src/UbiquitousLanguageManager.Web
```

確認項目：
- ✅ `Now listening on: https://localhost:7001` が表示される
- ✅ ブラウザで https://localhost:7001 にアクセスできる
- ✅ ログイン画面が表示される

`Ctrl + C` でサーバーを停止。

## 6. VSCode拡張機能の確認

```powershell
# インストール済み拡張機能の確認
code --list-extensions
```

以下の拡張機能が含まれているか確認：
- ✅ ms-dotnettools.csdevkit
- ✅ ms-dotnettools.csharp
- ✅ ionide.ionide-fsharp
- ✅ eamodio.gitlens
- ✅ anthropic.claude-code

## 7. Claude Code の確認

```powershell
# Claude CLIバージョン確認
claude --version

# MCPサーバー確認
claude mcp list
# 期待値: serena が Connected と表示される
```

## 8. Serena MCP の確認

```powershell
# Serena動作テスト
uvx --from git+https://github.com/oraios/serena serena --version

# MCPサーバー手動起動テスト
uvx --from git+https://github.com/oraios/serena serena start-mcp-server --context ide-assistant --project . --test
```

## 9. Gemini連携の確認

```powershell
# Pythonスクリプト実行テスト
python scripts/gemini-search.py "test query"
# APIキーが正しければ結果が返される

# 環境変数確認
$env:GEMINI_API_KEY
# APIキーが表示される
```

## 10. 統合動作確認

### データベースマイグレーション確認

```powershell
# マイグレーション状態確認
dotnet ef migrations list --project src/UbiquitousLanguageManager.Infrastructure --startup-project src/UbiquitousLanguageManager.Web
```

### ログイン機能確認

1. 開発サーバーを起動
2. https://localhost:7001 にアクセス
3. 初期管理者アカウントでログイン：
   - Email: admin@example.com
   - Password: Admin123!

### メール送信確認

1. SMTP4Dev Web UIを開く: http://localhost:5080
2. アプリケーションでパスワードリセットなどを実行
3. SMTP4Dev でメールが受信されることを確認

## 11. チェックリスト

すべての項目にチェックが付けば環境構築完了：

```
□ Git インストール済み
□ .NET 8.0 SDK インストール済み
□ Docker Desktop インストール・起動済み
□ VSCode インストール済み
□ リポジトリクローン完了
□ Docker コンテナ起動確認
□ プロジェクトビルド成功
□ 開発サーバー起動確認
□ VSCode拡張機能インストール済み
□ Claude Code 設定完了
□ Serena MCP 接続確認
□ Gemini API 設定完了
□ ログイン機能動作確認
□ メール送信機能動作確認
```

## 12. トラブルシューティング

### ビルドエラーの場合

```powershell
# キャッシュクリア
dotnet clean
dotnet restore --force
dotnet build --no-incremental
```

### Dockerコンテナが起動しない場合

```powershell
# コンテナ停止・削除
docker-compose down
docker-compose rm -f

# ボリューム削除（データも削除される）
docker volume prune

# 再起動
docker-compose up -d
```

### ポート競合の場合

他のアプリケーションがポートを使用している場合は、`docker-compose.yml` でポート番号を変更：

```yaml
ports:
  - "5433:5432"  # PostgreSQL用に5433を使用
  - "5026:25"    # SMTP用に5026を使用
```

---
作成日: 2025-08-09