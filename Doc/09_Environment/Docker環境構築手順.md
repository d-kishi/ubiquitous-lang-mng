# Docker環境構築手順

**作成日**: 2025-07-06  
**最終更新**: 2025-07-06  
**対象環境**: Windows 11 + WSL2  

## 前提条件

### 必須条件
- ✅ Windows 11（WSL2対応版）
- ✅ WSL2インストール済み
- ✅ Ubuntu等のLinuxディストリビューション導入済み

### 事前確認・設定

#### Windows機能の有効化確認
1. **「Windowsの機能の有効化または無効化」**を開く
   - スタートメニュー検索で「Windows機能」で検索
2. 以下の項目が**チェック済み**であることを確認：
   - ✅ **「Hyper-V」**（すべてのサブ項目含む）
   - ✅ **「仮想マシンプラットフォーム」**
   - ✅ **「Linux用Windowsサブシステム」**
3. チェックが外れている場合はチェックして**再起動**

#### WSL2バージョン確認
```bash
# PowerShellまたはコマンドプロンプトで実行
wsl -l -v

# 期待される結果例
#   NAME            STATE           VERSION
# * Ubuntu          Running         2
```

#### BIOS仮想化設定確認
- CPUの仮想化機能（Intel VT-x/AMD-V）が有効になっている必要があります
- BIOS/UEFI設定で「Virtualization Technology」が**Enabled**であることを確認

## 1. Docker Desktop インストール

### 1.1 Microsoft Store からのインストール
1. **Microsoft Store** を開く
2. **「Docker Desktop」** で検索
3. **「Docker Desktop」** をクリック
4. **「インストール」** をクリック
5. インストール完了まで待機（約5-10分）

### 1.2 インストール確認
- スタートメニューに「Docker Desktop」が表示されることを確認
- アプリケーション一覧に「Docker Desktop」が追加されることを確認

## 2. Docker Desktop 初期設定

### 2.1 Docker Desktop 起動
1. **Docker Desktop** を起動
2. 利用規約に同意（Accept）
3. 初期セットアップウィザードを完了
4. システム再起動が要求された場合は再起動

### 2.2 WSL2統合設定
1. Docker Desktop の **「Settings」**（⚙️アイコン）をクリック
2. 左メニューから **「Resources」** → **「WSL Integration」** を選択
3. **「Enable integration with my default WSL distro」** にチェック
4. 使用中のWSL2ディストリビューション（Ubuntu等）を **「ON」** に設定
5. **「Apply & Restart」** をクリック
6. Docker Desktop の再起動完了まで待機

## 3. 動作確認

### 3.1 WSL2ターミナルでの確認
WSL2環境（Ubuntu等）のターミナルを開き、以下のコマンドを実行：

```bash
# Dockerバージョン確認
docker --version

# Docker Composeバージョン確認
docker-compose --version

# Docker動作テスト
docker run hello-world
```

### 3.2 期待される結果
```
Docker version 28.2.2, build e6534b4
Docker Compose version v2.37.1-desktop.1
Hello from Docker!
```

## 4. プロジェクト固有の設定

### 4.1 ポート番号設定
**重要**: アプリケーションがデフォルトポート5000を使用するため、smtp4devのポートを変更しています。

- **Webアプリケーション**: http://localhost:5000
- **smtp4dev（メール確認）**: http://localhost:5080
- **pgAdmin（DB管理）**: http://localhost:8080
- **PostgreSQL**: localhost:5432

### 4.2 docker-compose.yml の設定
smtp4devのポート設定は以下のとおりです：
```yaml
smtp4dev:
  ports:
    - "5080:80"    # Web UI（5000から変更）
    - "2525:25"    # SMTP Port
```

## 5. 基本的なDockerコマンド

### 5.1 コンテナ管理
```bash
# コンテナ一覧表示（実行中）
docker ps

# コンテナ一覧表示（全て）
docker ps -a

# コンテナ停止
docker stop <コンテナ名またはID>

# コンテナ削除
docker rm <コンテナ名またはID>

# コンテナ内でコマンド実行
docker exec <コンテナ名> <コマンド>

# コンテナ内に入る
docker exec -it <コンテナ名> bash
```

### 4.2 イメージ管理
```bash
# イメージ一覧表示
docker images

# イメージ削除
docker rmi <イメージ名:タグ>

# イメージのダウンロード
docker pull <イメージ名:タグ>

# 未使用イメージの削除
docker image prune
```

### 4.3 Docker Compose
```bash
# サービス起動（バックグラウンド）
docker-compose up -d

# サービス停止
docker-compose down

# サービス停止（ボリューム削除）
docker-compose down -v

# ログ確認
docker-compose logs

# 特定サービスのログ確認
docker-compose logs <サービス名>

# サービス再起動
docker-compose restart
```

### 4.4 システム管理
```bash
# Docker システム情報
docker system info

# Docker 使用量確認
docker system df

# 未使用リソース削除
docker system prune

# 全ての未使用リソース削除（注意）
docker system prune -a
```

## 5. プロジェクト固有の操作

### 5.1 ユビキタス言語管理システム環境
```bash
# プロジェクトディレクトリに移動
cd /mnt/c/Develop/ubiquitous-lang-mng

# PostgreSQL環境起動
docker-compose up -d

# 環境停止
docker-compose down

# データベース直接接続
docker exec ubiquitous-lang-postgres psql -U ubiquitous_lang_user -d ubiquitous_lang_db

# PostgreSQLコンテナのログ確認
docker-compose logs postgres

# pgAdminのログ確認
docker-compose logs pgadmin
```

### 5.2 pgAdminでのデータベース接続設定

コンテナを起動したら、以下の手順でWeb管理ツールpgAdminからデータベースに接続します。

1. **Webブラウザで `http://localhost:8080` を開きます**
2. **ログイン画面で以下の情報を入力**：
   - **Email Address**: `admin@ubiquitous-lang.com`
   - **Password**: `admin123`
3. **ログイン後、ダッシュボードで "Add New Server" をクリック**
4. **"General" タブで "Name" に分かりやすい名前を入力**（例: `ubiquitous-lang-db`）
5. **"Connection" タブに切り替え、以下の情報を入力**：
   - **Host name/address**: `postgres`  
     ⚠️ **重要**: `localhost`ではありません。Dockerコンテナ間の通信ではサービス名を指定します
   - **Port**: `5432`
   - **Maintenance database**: `ubiquitous_lang_db`
   - **Username**: `ubiquitous_lang_user`
   - **Password**: `ubiquitous_lang_password`
6. **"Save" をクリックして接続設定を保存**
7. **左側のツリーに作成したサーバーが表示され、テーブル一覧などが確認できれば成功**

### 5.3 データベース操作
```bash
# テーブル一覧確認
docker exec ubiquitous-lang-postgres psql -U ubiquitous_lang_user -d ubiquitous_lang_db -c "\dt"

# ユーザー一覧確認
docker exec ubiquitous-lang-postgres psql -U ubiquitous_lang_user -d ubiquitous_lang_db -c "SELECT email, name, userrole FROM users;"

# データベースサイズ確認
docker exec ubiquitous-lang-postgres psql -U ubiquitous_lang_user -d ubiquitous_lang_db -c "SELECT pg_size_pretty(pg_database_size('ubiquitous_lang_db'));"
```

## 6. トラブルシューティング

### 6.1 段階的問題診断手順

#### ステップ1: 基本状態確認
```bash
# 1. WSL2状態確認
wsl -l -v

# 2. Docker Desktop状態確認
docker --version

# 3. Docker サービス状態確認
docker system info
```

#### ステップ2: 統合設定確認
```bash
# WSL2内でDocker利用可能か確認
docker run hello-world

# 期待される結果: "Hello from Docker!" メッセージ
```

### 6.2 よくあるエラーと対処法

#### エラー1: "WSL Distribution docker-desktop is missing"
**原因**: WSL2統合設定の問題
**対処法**:
1. Docker Desktop を完全に終了
2. PowerShellで実行: `wsl --shutdown`
3. Docker Desktop を再起動
4. Settings → Resources → WSL Integration で再設定

#### エラー2: "docker: command not found"
**原因**: WSL2統合が正しく設定されていない
**対処法**:
1. Docker Desktop Settings → Resources → WSL Integration
2. 「Enable integration with my default WSL distro」をチェック
3. 使用するディストリビューション（Ubuntu等）をONに設定
4. Apply & Restart

#### エラー3: "bind: address already in use"
**原因**: ポート競合
**診断コマンド**:
```bash
# Windows（コマンドプロンプト）
netstat -ano | findstr :5432
netstat -ano | findstr :8080

# WSL2（Linux）
sudo netstat -tlnp | grep 5432
```
**対処法**: 使用中のプロセスを停止するか、別ポートを使用

#### エラー4: "permission denied while trying to connect to the Docker daemon socket"
**原因**: 権限不足
**対処法**:
```bash
# WSL2内で実行
sudo usermod -aG docker $USER

# 重要: WSL2セッションを完全に再開
exit
# 新しいターミナルを開く
```

#### エラー5: Docker Desktop が起動しない
**原因**: Windows機能が無効またはBIOS設定
**確認事項**:
1. Windows機能で仮想化関連がすべて有効か
2. BIOS/UEFIで仮想化技術が有効か
3. セキュリティソフトの仮想化ブロック確認

### 6.3 診断用コマンド集

#### システム情報確認
```bash
# Docker システム詳細情報
docker system info

# Docker 使用量とエラー情報
docker system df

# Docker events監視（別ターミナルで実行）
docker system events
```

#### ログ確認手順
```bash
# プロジェクト全体のログ
docker-compose logs

# 特定サービスのログ（詳細表示）
docker-compose logs -f postgres
docker-compose logs -f pgadmin

# コンテナの詳細情報
docker inspect ubiquitous-lang-postgres
```

### 6.4 段階的復旧手順

#### レベル1: 軽微な問題
```bash
# サービス再起動
docker-compose restart

# 特定コンテナ再起動
docker-compose restart postgres
```

#### レベル2: 中程度の問題
```bash
# 環境完全再起動
docker-compose down
docker-compose up -d

# イメージ更新
docker-compose pull
docker-compose up -d
```

#### レベル3: 重大な問題
```bash
# 注意: データが削除される可能性があります
docker-compose down -v
docker system prune -f
docker-compose up -d
```

#### レベル4: 完全リセット
```bash
# ⚠️ 重大警告: 全てのDockerデータが削除されます
# このPC上の、停止中のコンテナ、ネットワーク、イメージ、そして最も重要な「ボリューム（データ）」がすべて削除されます
# 他のプロジェクトのDBデータなども消えるため、実行前に影響範囲を必ず理解してください
docker system prune -a --volumes
wsl --shutdown
# Docker Desktop 再起動
```

### 6.5 予防的メンテナンス

#### 週次メンテナンス
```bash
# 未使用イメージ削除
docker image prune

# 停止コンテナ削除
docker container prune

# 未使用ネットワーク削除
docker network prune
```

#### 月次メンテナンス
```bash
# システム全体クリーンアップ
docker system prune

# ログローテーション確認
docker-compose logs --tail=100 postgres
```

## 7. パフォーマンス最適化

### 7.1 WSL2メモリ制限設定
ファイル: `%USERPROFILE%\.wslconfig`
```ini
[wsl2]
memory=8GB
processors=4
```

### 7.2 Docker Desktop リソース設定
- **Memory**: 4GB以上推奨
- **CPU**: 2コア以上推奨
- **Disk image size**: 64GB以上推奨

## 8. セキュリティ考慮事項

### 8.1 本番環境との分離
- 開発環境専用の認証情報使用
- 本番データの使用禁止
- ポート公開の最小化

### 8.2 定期メンテナンス
```bash
# 週次実行推奨
docker system prune
docker image prune
```

## 9. 関連ファイル

- `/docker-compose.yml`: サービス定義
- `/init/01_create_schema.sql`: データベーススキーマ
- `/init/02_initial_data.sql`: 初期データ
- `/Doc/09_Environment/DB接続情報.md`: データベース接続情報

## 10. サポート情報

### 10.1 公式ドキュメント
- [Docker Desktop for Windows](https://docs.docker.com/desktop/windows/)
- [Docker Compose](https://docs.docker.com/compose/)
- [PostgreSQL Docker Image](https://hub.docker.com/_/postgres)

### 10.2 よくある質問
**Q: Docker Desktopが起動しない**
A: Windows機能のHyper-VとWSL2が有効になっているか確認

**Q: コンテナが起動しない**
A: ログを確認: `docker-compose logs`

**Q: データが消えた**
A: Dockerボリュームが削除された可能性。`docker volume ls`で確認

---

**作成者**: Claude Code  
**検証環境**: Windows 11 + WSL2 + Ubuntu  
**更新責任者**: プロジェクトオーナー