# データベース設計書同期チェックリスト

## 実施タイミング
- EF Migrations適用後（`dotnet ef database update`実行後）
- スキーマ変更手順Step 5で実施

## チェック項目

### 1. PostgreSQL標準型名確認
- [ ] `VARCHAR(n)` → `character varying(n)`
- [ ] `TEXT` → `text`
- [ ] `BOOLEAN` → `boolean`
- [ ] `BIGINT` → `bigint`
- [ ] `INTEGER` → `integer`
- [ ] `TIMESTAMP` → `timestamp without time zone`
- [ ] `TIMESTAMPTZ` → `timestamp with time zone`

### 2. 列定義の同期
- [ ] 列名が実際のDBと一致
- [ ] データ型が実際のDBと一致
- [ ] NULL許可/NOT NULLが実際のDBと一致
- [ ] デフォルト値が実際のDBと一致

### 3. 制約の同期
- [ ] PRIMARY KEY制約が記載されている
- [ ] FOREIGN KEY制約が記載されている
- [ ] CHECK制約が記載されている（例: `CK_DraftUbiquitousLang_Status`）
- [ ] UNIQUE制約が記載されている

### 4. インデックスの同期
- [ ] B-Treeインデックスが記載されている
- [ ] GINインデックスが記載されている（該当する場合）
- [ ] BRINインデックスが記載されている（該当する場合）

### 5. COMMENT文の同期
- [ ] テーブルCOMMENTが記載されている
- [ ] 列COMMENTが記載されている

## 確認方法

### 実際のDBスキーマ確認
```bash
# テーブル構造確認
docker-compose exec postgres psql -U ubiquitous_lang_user -d ubiquitous_lang_db -c "\d+ \"TableName\""

# COMMENT確認（テーブル）
docker-compose exec postgres psql -U ubiquitous_lang_user -d ubiquitous_lang_db -c "SELECT obj_description('\"TableName\"'::regclass, 'pg_class');"

# COMMENT確認（列）
docker-compose exec postgres psql -U ubiquitous_lang_user -d ubiquitous_lang_db -c "SELECT a.attname, col_description(a.attrelid, a.attnum) FROM pg_attribute a WHERE a.attrelid = '\"TableName\"'::regclass AND a.attnum > 0 AND NOT a.attisdropped;"
```

### データベース設計書更新
**ファイル**: `Doc/02_Design/データベース設計書.md`

**必須更新箇所**:
1. 対象テーブルの定義セクション
2. ER図（必要に応じて）
3. 「1.3 DB初期化方針」セクション（初回のみ）

## 不一致時の対応

**原則**: 実際のDBスキーマ（PostgreSQL）に合わせてドキュメントを修正

**例外**: 実際のDBが明らかに誤っている場合
→ Entity定義修正 → Migration作成 → データベース設計書更新
