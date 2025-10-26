# 手動SQL追加パターン

## パターン1: GINインデックス（JSONB列・全文検索）

### Migration作成
```bash
dotnet ef migrations add AddGinIndexForSearch
```

### Migration手動編集
```csharp
protected override void Up(MigrationBuilder migrationBuilder)
{
    // GINインデックス追加（JSONB列）
    migrationBuilder.Sql(
        @"CREATE INDEX ""IX_TableName_JsonColumn_GIN""
          ON ""TableName""
          USING GIN (""JsonColumn"")");

    // GINインデックス追加（全文検索）
    migrationBuilder.Sql(
        @"CREATE INDEX ""IX_DraftUbiquitousLang_SearchText_GIN""
          ON ""DraftUbiquitousLang""
          USING GIN (to_tsvector('japanese', ""JapaneseName"" || ' ' || ""EnglishName""))");
}

protected override void Down(MigrationBuilder migrationBuilder)
{
    migrationBuilder.Sql(
        @"DROP INDEX IF EXISTS ""IX_TableName_JsonColumn_GIN""");

    migrationBuilder.Sql(
        @"DROP INDEX IF EXISTS ""IX_DraftUbiquitousLang_SearchText_GIN""");
}
```

## パターン2: BRINインデックス（時系列データ）

### Migration手動編集
```csharp
protected override void Up(MigrationBuilder migrationBuilder)
{
    migrationBuilder.Sql(
        @"CREATE INDEX ""IX_TableName_CreatedAt_BRIN""
          ON ""TableName""
          USING BRIN (""CreatedAt"")");
}

protected override void Down(MigrationBuilder migrationBuilder)
{
    migrationBuilder.Sql(
        @"DROP INDEX IF EXISTS ""IX_TableName_CreatedAt_BRIN""");
}
```

## パターン3: COMMENT文追加（手動）

### Migration手動編集
```csharp
protected override void Up(MigrationBuilder migrationBuilder)
{
    migrationBuilder.Sql(
        @"COMMENT ON TABLE ""TableName"" IS 'テーブルの説明'");

    migrationBuilder.Sql(
        @"COMMENT ON COLUMN ""TableName"".""ColumnName"" IS '列の説明'");
}
```

**注意**: COMMENT文は通常、Entity定義の`comment:`パラメータで指定推奨。手動SQLは既存テーブルへの追加時のみ使用。

## 適用対象Phase

- **Phase C-D**: GINインデックス（ユビキタス言語検索機能）
- **Phase E以降**: BRINインデックス（監査ログ等の時系列データ）
