# CHECK制約追加パターン

## パターン1: HasCheckConstraint（推奨）

### Entity定義
```csharp
public class DraftUbiquitousLang
{
    public string Status { get; set; } = "Draft"; // 'Draft' or 'PendingApproval'
}
```

### DbContext OnModelCreating
```csharp
modelBuilder.Entity<DraftUbiquitousLang>()
    .HasCheckConstraint("CK_DraftUbiquitousLang_Status",
        "\"Status\" IN ('Draft', 'PendingApproval')");
```

### Migration作成
```bash
dotnet ef migrations add AddDraftStatusCheckConstraint
```

**自動生成される内容**:
```csharp
migrationBuilder.AddCheckConstraint(
    name: "CK_DraftUbiquitousLang_Status",
    table: "DraftUbiquitousLang",
    sql: "\"Status\" IN ('Draft', 'PendingApproval')");
```

## パターン2: 手動SQL（複雑な制約）

### Migration手動編集
```csharp
protected override void Up(MigrationBuilder migrationBuilder)
{
    migrationBuilder.Sql(
        @"ALTER TABLE ""TableName""
          ADD CONSTRAINT ""CK_TableName_ColumnName""
          CHECK (""ColumnName"" > 0 AND ""ColumnName"" < 100)");
}

protected override void Down(MigrationBuilder migrationBuilder)
{
    migrationBuilder.Sql(
        @"ALTER TABLE ""TableName""
          DROP CONSTRAINT IF EXISTS ""CK_TableName_ColumnName""");
}
```

## 命名規則

```
CK_{TableName}_{ColumnName}
```

**例**:
- `CK_DraftUbiquitousLang_Status`
- `CK_Projects_OwnerId`

## 動作確認

### 無効値テスト
```sql
INSERT INTO "TableName" ("ColumnName") VALUES ('InvalidValue');
-- 期待結果: CHECK制約違反エラー
```

### 有効値テスト
```sql
INSERT INTO "TableName" ("ColumnName") VALUES ('ValidValue');
-- 期待結果: INSERT成功
```
