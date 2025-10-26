# EF Migrations スキーマ変更ワークフロー

## 5ステップ手順

### Step 1: Entity定義変更（C#コード）

**ファイル**: `src/UbiquitousLanguageManager.Infrastructure/Data/Entities/{EntityName}.cs`

```csharp
public class NewTable
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
```

### Step 2: Migration作成

```bash
dotnet ef migrations add {MigrationName} --project src/UbiquitousLanguageManager.Infrastructure
```

**命名規則**:
- 機能追加: `Add{Feature}` (例: `AddCheckConstraints`)
- 列追加: `Add{TableName}{ColumnName}` (例: `AddProjectDescription`)
- テーブル追加: `Create{TableName}Table` (例: `CreateNotificationTable`)

### Step 3: Migrationファイル確認・必要に応じて手動編集

**確認項目**:
- [ ] PostgreSQL識別子Quote（`"TableName"`, `"ColumnName"`）
- [ ] TIMESTAMPTZ型: `type: "timestamptz"`
- [ ] COMMENT文: `comment: "日本語コメント"`
- [ ] CHECK制約: `HasCheckConstraint()` or `migrationBuilder.Sql()`
- [ ] GINインデックス等: `migrationBuilder.Sql()`

**手動編集が必要なケース**:
- CHECK制約追加
- GINインデックス追加
- PostgreSQL固有インデックス（BRIN等）

### Step 4: Migration適用

```bash
dotnet ef database update --project src/UbiquitousLanguageManager.Infrastructure
```

**確認コマンド**:
```bash
# Migrations履歴確認
docker-compose exec postgres psql -U ubiquitous_lang_user -d ubiquitous_lang_db -c "SELECT \"MigrationId\" FROM \"__EFMigrationsHistory\" ORDER BY \"MigrationId\";"

# テーブル構造確認
docker-compose exec postgres psql -U ubiquitous_lang_user -d ubiquitous_lang_db -c "\d+ \"TableName\""
```

### Step 5: データベース設計書更新

**ファイル**: `Doc/02_Design/データベース設計書.md`

**更新内容**:
- テーブル定義追加・変更
- 列定義追加・変更
- 制約追加・変更
- **重要**: PostgreSQL標準型名を使用（`character varying`, `text`, `timestamp with time zone`）

**チェックリスト**: `db-doc-sync-checklist.md`を参照

## ビルド確認

```bash
dotnet build
```

**期待結果**: 0 Warning, 0 Error
