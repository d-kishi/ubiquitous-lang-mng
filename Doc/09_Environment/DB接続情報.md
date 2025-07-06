# データベース接続情報

**作成日**: 2025-07-06  
**最終更新**: 2025-07-06  
**環境**: 開発環境（Docker Container）  

## PostgreSQL接続情報

### Docker Container環境
- **データベース種別**: PostgreSQL 16-alpine
- **ホスト名**: 
  - `localhost` （PC上のDBツールから接続する場合）
  - `postgres` （pgAdminコンテナから接続する場合）
- **ポート番号**: 5432
- **データベース名**: ubiquitous_lang_db
- **ユーザー名**: ubiquitous_lang_user
- **パスワード**: ubiquitous_lang_password

### A5:SQL Mk-2 接続設定
```
データベース種別: PostgreSQL
ホスト名: localhost
ポート番号: 5432
データベース名: ubiquitous_lang_db
ユーザー名: ubiquitous_lang_user
パスワード: ubiquitous_lang_password
```

### pgAdmin 4 管理ツール
- **URL**: http://localhost:8080
- **管理者Email**: admin@ubiquitous-lang.com
- **管理者パスワード**: admin123

## 初期ユーザー情報

### システムユーザー（アプリケーション内）
| Email | 氏名 | ロール | パスワード |
|-------|------|--------|-----------|
| admin@ubiquitous-lang.com | システム管理者 | SuperUser | admin123 |
| project.manager@ubiquitous-lang.com | プロジェクト管理者 | ProjectManager | admin123 |
| domain.approver@ubiquitous-lang.com | ドメイン承認者 | DomainApprover | admin123 |
| general.user@ubiquitous-lang.com | 一般ユーザー | GeneralUser | admin123 |

## Docker操作コマンド

### 環境起動
```bash
docker-compose up -d
```

### 環境停止
```bash
docker-compose down
```

### ログ確認
```bash
docker-compose logs postgres
docker-compose logs pgadmin
```

### PostgreSQL直接接続
```bash
docker exec ubiquitous-lang-postgres psql -U ubiquitous_lang_user -d ubiquitous_lang_db
```

## データベース構成

### テーブル一覧（11テーブル）
1. **users** - ユーザー管理
2. **projects** - プロジェクト管理
3. **domains** - ドメイン管理
4. **draftubiquitouslang** - ドラフトユビキタス言語
5. **formalubiquitouslang** - 正式ユビキタス言語
6. **userprojects** - ユーザー・プロジェクト関連
7. **domainapprovers** - ドメイン承認者
8. **relatedubiquitouslang** - 関連ユビキタス言語
9. **draftubiquitouslangrelations** - ドラフト関連
10. **formalubiquitouslanghistory** - 正式用語履歴
11. **relatedubiquitouslanghistory** - 関連用語履歴

## セキュリティ注意事項

⚠️ **重要**: この情報は開発環境用です。本番環境では以下を必ず変更してください：
- データベースパスワード
- pgAdmin管理者パスワード
- 初期ユーザーパスワード

## トラブルシューティング

### 接続エラーの場合
1. Dockerコンテナの起動状態確認: `docker ps`
2. ポート使用状況確認: `netstat -an | grep 5432`
3. ログ確認: `docker-compose logs postgres`

### データリセット
```bash
# 注意: 全データが削除されます
docker-compose down -v
docker-compose up -d
```

---

**管理者**: プロジェクトオーナー  
**更新責任者**: Claude Code  
**関連ファイル**: 
- `/docker-compose.yml`
- `/init/01_create_schema.sql`
- `/init/02_initial_data.sql`