# 重要なコマンド一覧

## ビルド・実行コマンド

### ビルド
```bash
dotnet build                                           # 全体ビルド
dotnet build src/UbiquitousLanguageManager.Web        # Web層のみ
```

### 実行
```bash
dotnet run --project src/UbiquitousLanguageManager.Web # アプリ起動（http://localhost:5000）
```

### Docker環境
```bash
docker-compose up -d                                   # PostgreSQL/PgAdmin/Smtp4dev起動
docker-compose down                                    # 停止
```

## テストコマンド

### テスト実行
```bash
dotnet test                                            # 全テスト
dotnet test --filter "FullyQualifiedName~UserTests"   # 特定テストのみ
dotnet test --logger "console;verbosity=detailed"     # 詳細出力
```

### カバレッジ測定
```bash
dotnet test --collect:"XPlat Code Coverage"
```

## データベース関連

### Entity Framework
```bash
dotnet ef migrations add MigrationName --project src/UbiquitousLanguageManager.Infrastructure
dotnet ef database update --project src/UbiquitousLanguageManager.Infrastructure
```

### PostgreSQL接続
```bash
psql -h localhost -U ubiquitous_lang_user -d ubiquitous_lang_db
```

## Windows系システムコマンド
- `dir` - ファイル一覧表示
- `cd` - ディレクトリ移動
- `findstr` - テキスト検索 (grep相当)
- `where` - ファイル場所検索 (which相当)
- `type` - ファイル内容表示 (cat相当)

## 開発ツールURL
- **アプリ**: http://localhost:5000
- **PgAdmin**: http://localhost:8080 (admin@ubiquitous-lang.com / admin123)
- **Smtp4dev**: http://localhost:5080

## 品質チェックコマンド
タスク完了時に必ず実行：
1. `dotnet build` - ビルドエラーチェック
2. `dotnet test` - テスト実行
3. 動作確認 - アプリ起動して主要機能確認