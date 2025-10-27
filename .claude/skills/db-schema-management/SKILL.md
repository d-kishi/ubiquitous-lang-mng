# db-schema-management Skill

**目的**: EF Migrationsによるデータベーススキーマ変更の実装パターンをガイド

## 自律適用条件

Claudeは以下の状況で**このSkillを自律的に参照**します：

1. **新規テーブル追加時**: Entity定義作成後、Migration作成前
2. **列追加・変更時**: Entity定義変更後、Migration作成前
3. **CHECK制約追加時**: データ整合性制約が必要な場合
4. **データベース設計書更新時**: スキーマ変更後のドキュメント同期
5. **PostgreSQL固有機能使用時**: GINインデックス、BRIN等の手動SQL追加

## 提供パターン

1. **ef-migrations-workflow.md**: スキーマ変更5ステップ手順
2. **check-constraint-pattern.md**: CHECK制約追加パターン
3. **manual-sql-pattern.md**: 手動SQL追加パターン（GINインデックス等）
4. **db-doc-sync-checklist.md**: データベース設計書同期チェックリスト

## Skillの使い方

### 基本フロー
```
Entity定義変更
  ↓
ef-migrations-workflow.mdを参照
  ↓
Migration作成（dotnet ef migrations add）
  ↓
（必要に応じて）check-constraint-pattern.md参照
  ↓
（必要に応じて）manual-sql-pattern.md参照
  ↓
Migration適用（dotnet ef database update）
  ↓
db-doc-sync-checklist.mdを参照
  ↓
データベース設計書更新
```

## Phase B2 Step7で確立した知見

- PostgreSQL固有機能（TIMESTAMPTZ、JSONB、COMMENT文）の完全サポート
- CHECK制約の実装パターン（HasCheckConstraint）
- 手動SQL追加パターン（migrationBuilder.Sql）
- データベース設計書とEF Migrationsの継続的整合性維持

## 適用対象Phase

- **Phase B3以降**: 全スキーマ変更で適用
- **Phase C-D**: GINインデックス等の手動SQL追加で重点適用
