# InitialData再設計書

**作成日**: 2025-12-07
**目的**: GitHub Issue #79「ID体系統一リファクタリング」- InitialDataのGUID化

---

## 1. 現状分析

### 1.1 現在のID体系

**ファイル**: `init/backup/02_initial_data.sql`

| エンティティ | 現在のID | 形式 | 問題点 |
|-------------|----------|------|--------|
| システム管理者 | `admin-001` | 人間可読 | Guid.TryParse失敗 |
| プロジェクト管理者 | `pm-001` | 人間可読 | Guid.TryParse失敗 |
| ドメイン承認者 | `da-001` | 人間可読 | Guid.TryParse失敗 |
| 一般ユーザー | `gu-001` | 人間可読 | Guid.TryParse失敗 |
| ロールID | `super-user`, `project-manager`等 | 人間可読 | 同上 |

### 1.2 関連テーブル影響

| テーブル | ID参照カラム | 影響 |
|----------|-------------|------|
| `AspNetUsers` | `Id`, `UpdatedBy` | 直接影響 |
| `AspNetUserRoles` | `UserId`, `RoleId` | 直接影響 |
| `AspNetRoles` | `Id` | 直接影響 |
| `UserProjects` | `UserId` | 直接影響 |
| `DomainApprovers` | `ApproverId` | 直接影響 |
| `Projects` | `UpdatedBy` | 直接影響 |
| `Domains` | `UpdatedBy` | 直接影響 |
| `DraftUbiquitousLang` | `UpdatedBy` | 直接影響 |
| `FormalUbiquitousLang` | `UpdatedBy` | 直接影響 |

---

## 2. GUID変換マッピング

### 2.1 ユーザーID変換

| 現在のID | 新GUID | 説明 |
|----------|--------|------|
| `admin-001` | `00000000-0000-0000-0000-000000000001` | システム管理者 |
| `pm-001` | `00000000-0000-0000-0000-000000000002` | プロジェクト管理者 |
| `da-001` | `00000000-0000-0000-0000-000000000003` | ドメイン承認者 |
| `gu-001` | `00000000-0000-0000-0000-000000000004` | 一般ユーザー |

**GUID形式選択理由**:
- **パターン1（採用）**: `00000000-0000-0000-0000-00000000000X` - 開発用に識別しやすい連番形式
- **パターン2**: ランダムGUID - 本番相当だがデバッグ困難

### 2.2 ロールID変換

| 現在のID | 新GUID | 説明 |
|----------|--------|------|
| `super-user` | `00000000-0000-0000-0001-000000000001` | SuperUserロール |
| `project-manager` | `00000000-0000-0000-0001-000000000002` | ProjectManagerロール |
| `domain-approver` | `00000000-0000-0000-0001-000000000003` | DomainApproverロール |
| `general-user` | `00000000-0000-0000-0001-000000000004` | GeneralUserロール |

**GUID分類規則**:
- ユーザーID: `00000000-0000-0000-0000-00000000XXXX`
- ロールID: `00000000-0000-0000-0001-00000000XXXX`

---

## 3. 修正対象SQL一覧

### 3.1 AspNetUsers

```sql
-- 修正前
INSERT INTO "AspNetUsers" ("Id", ...) VALUES ('admin-001', ...);

-- 修正後
INSERT INTO "AspNetUsers" ("Id", ...) VALUES ('00000000-0000-0000-0000-000000000001', ...);
```

**修正箇所**: 4件（4ユーザー）

### 3.2 AspNetRoles

```sql
-- 修正前（01_initial_schema.sql内）
INSERT INTO "AspNetRoles" ("Id", ...) VALUES ('super-user', ...);

-- 修正後
INSERT INTO "AspNetRoles" ("Id", ...) VALUES ('00000000-0000-0000-0001-000000000001', ...);
```

**修正箇所**: 4件（4ロール）

### 3.3 AspNetUserRoles

```sql
-- 修正前
INSERT INTO "AspNetUserRoles" ("UserId", "RoleId") VALUES ('admin-001', 'super-user');

-- 修正後
INSERT INTO "AspNetUserRoles" ("UserId", "RoleId") VALUES
    ('00000000-0000-0000-0000-000000000001', '00000000-0000-0000-0001-000000000001');
```

**修正箇所**: 4件（4ユーザー×ロール）

### 3.4 UserProjects

```sql
-- 修正前
INSERT INTO "UserProjects" ("UserId", ...) VALUES ('pm-001', 1, 'admin-001');

-- 修正後
INSERT INTO "UserProjects" ("UserId", ...) VALUES
    ('00000000-0000-0000-0000-000000000002', 1, '00000000-0000-0000-0000-000000000001');
```

**修正箇所**: 6件

### 3.5 DomainApprovers

```sql
-- 修正前
INSERT INTO "DomainApprovers" (..., "ApproverId", "UpdatedBy") VALUES (1, 'da-001', 'admin-001');

-- 修正後
INSERT INTO "DomainApprovers" (..., "ApproverId", "UpdatedBy") VALUES
    (1, '00000000-0000-0000-0000-000000000003', '00000000-0000-0000-0000-000000000001');
```

**修正箇所**: 3件

### 3.6 UpdatedByカラム（全テーブル）

```sql
-- 修正前
"UpdatedBy" = 'admin-001'

-- 修正後
"UpdatedBy" = '00000000-0000-0000-0000-000000000001'
```

**修正対象テーブル**:
- AspNetUsers: 4件
- Projects: 2件
- Domains: 3件
- UserProjects: 6件
- DomainApprovers: 3件
- DraftUbiquitousLang: 5件
- FormalUbiquitousLang: 2件
- RelatedUbiquitousLang: 1件

---

## 4. マイグレーション手順

### 4.1 開発環境マイグレーション

**前提条件**: 開発環境DBは再作成可能

```bash
# Step 1: 既存DBを削除
docker exec ubiquitous-lang-mng_devcontainer-devcontainer-1 \
  dotnet ef database drop --project src/UbiquitousLanguageManager.Infrastructure --force

# Step 2: 新InitialDataでDB再作成
docker exec ubiquitous-lang-mng_devcontainer-devcontainer-1 \
  dotnet ef database update --project src/UbiquitousLanguageManager.Infrastructure

# Step 3: InitialData投入
docker exec -i postgres-container psql -U admin -d ubiquitous_lang_db < init/backup/02_initial_data.sql
```

### 4.2 データ移行クエリ（参考）

既存データがある場合の移行用UPDATE文:

```sql
-- ユーザーID更新
UPDATE "AspNetUsers" SET "Id" = '00000000-0000-0000-0000-000000000001' WHERE "Id" = 'admin-001';
UPDATE "AspNetUsers" SET "Id" = '00000000-0000-0000-0000-000000000002' WHERE "Id" = 'pm-001';
UPDATE "AspNetUsers" SET "Id" = '00000000-0000-0000-0000-000000000003' WHERE "Id" = 'da-001';
UPDATE "AspNetUsers" SET "Id" = '00000000-0000-0000-0000-000000000004' WHERE "Id" = 'gu-001';

-- ロールID更新
UPDATE "AspNetRoles" SET "Id" = '00000000-0000-0000-0001-000000000001' WHERE "Id" = 'super-user';
UPDATE "AspNetRoles" SET "Id" = '00000000-0000-0000-0001-000000000002' WHERE "Id" = 'project-manager';
UPDATE "AspNetRoles" SET "Id" = '00000000-0000-0000-0001-000000000003' WHERE "Id" = 'domain-approver';
UPDATE "AspNetRoles" SET "Id" = '00000000-0000-0000-0001-000000000004' WHERE "Id" = 'general-user';

-- 関連テーブルのFK更新
UPDATE "AspNetUserRoles" SET "UserId" = '00000000-0000-0000-0000-000000000001' WHERE "UserId" = 'admin-001';
-- ... 以下同様
```

---

## 5. 検証チェックリスト

### 5.1 GUID形式検証

- [ ] 全AspNetUsers.IdがGUID形式
- [ ] 全AspNetRoles.IdがGUID形式
- [ ] 全UpdatedByカラムがGUID形式
- [ ] 全UserProjects.UserIdがGUID形式
- [ ] 全DomainApprovers.ApproverIdがGUID形式

### 5.2 機能検証

- [ ] ログイン成功（全4ユーザー）
- [ ] ロール判定正常（SuperUser/PM/DA/GU）
- [ ] プロジェクト一覧表示（PM/DA/GU）
- [ ] ユーザー一覧表示（SuperUser/PM）
- [ ] プロジェクトフィルタ動作（PM）

---

## 6. 01_initial_schema.sql修正

### 6.1 ロール定義の修正

**ファイル**: `init/backup/01_initial_schema.sql`（存在確認必要）

```sql
-- 修正前
INSERT INTO "AspNetRoles" ("Id", "Name", "NormalizedName") VALUES
    ('super-user', 'SuperUser', 'SUPERUSER'),
    ('project-manager', 'ProjectManager', 'PROJECTMANAGER'),
    ('domain-approver', 'DomainApprover', 'DOMAINAPPROVER'),
    ('general-user', 'GeneralUser', 'GENERALUSER');

-- 修正後
INSERT INTO "AspNetRoles" ("Id", "Name", "NormalizedName") VALUES
    ('00000000-0000-0000-0001-000000000001', 'SuperUser', 'SUPERUSER'),
    ('00000000-0000-0000-0001-000000000002', 'ProjectManager', 'PROJECTMANAGER'),
    ('00000000-0000-0000-0001-000000000003', 'DomainApprover', 'DOMAINAPPROVER'),
    ('00000000-0000-0000-0001-000000000004', 'GeneralUser', 'GENERALUSER');
```

---

## 7. リスク・注意事項

### 7.1 破壊的変更

- **外部連携**: 現在のIDを外部システムが参照している場合、連携が破綻
- **バックアップデータ**: 旧IDで取得したバックアップとの互換性なし
- **ログ・監査**: 旧IDで記録されたログとの紐付け不可

### 7.2 対策

1. **事前バックアップ**: 移行前に全テーブルバックアップ
2. **ID対応表保存**: 旧ID↔新GUIDの対応表を永続化
3. **段階的移行**: 開発環境 → ステージング → 本番の順

---

## 8. 実装スケジュール（Step 5）

| # | 作業 | 担当 | 推定時間 |
|---|------|------|----------|
| 1 | 01_initial_schema.sql修正 | MainAgent | 30分 |
| 2 | 02_initial_data.sql修正 | MainAgent | 1時間 |
| 3 | DB再作成・InitialData投入 | MainAgent | 30分 |
| 4 | 動作確認（全4ロール） | MainAgent | 30分 |

**合計**: 約2.5時間

---

**作成日**: 2025-12-07
**作成者**: MainAgent
