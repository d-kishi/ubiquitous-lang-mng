# Step 11: InitialData対応 組織設計書

**作成日**: 2025-12-08
**Phase**: Issue79 - ID体系統一リファクタリング
**前提Step**: Step 10完了（2025-12-08, 0 Error 0 Warning）

---

## 1. Step概要

### 1.1 目的
InitialData（SQLスクリプト・C#定数）の人間可読ID形式をGUID形式に統一

### 1.2 対象ファイル
| ファイル | 修正内容 | 修正箇所数 |
|----------|----------|------------|
| `init/backup/01_create_schema.sql` | AspNetRoles IDをGUID化 | 4件 |
| `init/backup/02_initial_data.sql` | 全ID参照箇所をGUID化 | 26件以上 |
| `src/.../DbInitializer.cs` | ユーザーID/ロールID定数をGUID化 | 10件 |

### 1.3 参照資料
- `Doc/08_Organization/Active/Phase_Issue79/Research/InitialData再設計書.md`
- `Doc/08_Organization/Active/Phase_Issue79/Phase_Summary.md` Step 11セクション

---

## 2. GUID変換マッピング

### 2.1 ユーザーID
| 現在のID | 新GUID | 説明 |
|----------|--------|------|
| `admin-001` | `00000000-0000-0000-0000-000000000001` | システム管理者 |
| `pm-001` | `00000000-0000-0000-0000-000000000002` | プロジェクト管理者 |
| `da-001` | `00000000-0000-0000-0000-000000000003` | ドメイン承認者 |
| `gu-001` | `00000000-0000-0000-0000-000000000004` | 一般ユーザー |
| `e2e-test@ubiquitous-lang.local` | `00000000-0000-0000-0000-000000000099` | E2Eテストユーザー |

### 2.2 ロールID
| 現在のID | 新GUID | 説明 |
|----------|--------|------|
| `super-user` | `00000000-0000-0000-0001-000000000001` | SuperUserロール |
| `project-manager` | `00000000-0000-0000-0001-000000000002` | ProjectManagerロール |
| `domain-approver` | `00000000-0000-0000-0001-000000000003` | DomainApproverロール |
| `general-user` | `00000000-0000-0000-0001-000000000004` | GeneralUserロール |

### 2.3 GUID分類規則
- **ユーザーID**: `00000000-0000-0000-0000-00000000XXXX`
- **ロールID**: `00000000-0000-0000-0001-00000000XXXX`

---

## 3. Stage構成

### Stage 1: SQL InitialData修正
**担当**: MainAgent（単純置換作業）

#### Stage 1.1: 01_create_schema.sql修正
- AspNetRoles INSERT文のID修正（4件）
- 対象行: L468-473

#### Stage 1.2: 02_initial_data.sql修正
- AspNetUsers.Id修正（4件）
- AspNetUserRoles.UserId/RoleId修正（4件）
- UpdatedByカラム修正（25件以上）
- UserProjects.UserId修正（6件）
- DomainApprovers.ApproverId修正（3件）

### Stage 2: C# DbInitializer修正
**担当**: csharp-infrastructure Agent

- ユーザーID定数修正（5件）
- ロールID定数修正（4件）
- E2EテストユーザーID修正（1件）

### Stage 3: 動作検証
**担当**: MainAgent

1. **DB再作成**
   ```bash
   docker exec ubiquitous-lang-mng_devcontainer-devcontainer-1 dotnet ef database drop --project src/UbiquitousLanguageManager.Infrastructure --force
   docker exec ubiquitous-lang-mng_devcontainer-devcontainer-1 dotnet ef database update --project src/UbiquitousLanguageManager.Infrastructure
   ```

2. **InitialData投入**
   ```bash
   docker exec -i postgres psql -U admin -d ubiquitous_lang_db < init/backup/01_create_schema.sql
   docker exec -i postgres psql -U admin -d ubiquitous_lang_db < init/backup/02_initial_data.sql
   ```

3. **ログイン検証**
   - admin@ubiquitous-lang.com（SuperUser）
   - project.manager@ubiquitous-lang.com（ProjectManager）
   - domain.approver@ubiquitous-lang.com（DomainApprover）
   - general.user@ubiquitous-lang.com（GeneralUser）

---

## 4. 完了基準

### 4.1 必須条件
- [x] 01_create_schema.sql: 4ロールIDがGUID形式
- [x] 02_initial_data.sql: 全ID参照がGUID形式
- [x] DbInitializer.cs: 全定数がGUID形式
- [x] ビルド成功（0 Error 0 Warning）※ソースコードのみ、テストは別Step
- [x] ログイン検証成功（E2Eテストユーザー: SuperUserロール確認済み）

### 4.2 検証SQL
```sql
-- GUID形式検証
SELECT "Id" FROM "AspNetUsers" WHERE "Id" NOT LIKE '________-____-____-____-____________';
SELECT "Id" FROM "AspNetRoles" WHERE "Id" NOT LIKE '________-____-____-____-____________';
-- 結果が0件であること
```

---

## 5. リスク・注意事項

### 5.1 破壊的変更
- 既存DBデータとの互換性なし（再作成必須）
- 旧ID形式のバックアップデータは使用不可

### 5.2 対策
- Step実行前にDB状態を確認（必要に応じてバックアップ）
- 修正後は必ずビルド・検証を実施

---

## 6. 完了報告

**完了日時**: 2025-12-08
**実施者**: MainAgent

### 実施結果
| Stage | 内容 | 状態 |
|-------|------|------|
| 1.1 | 01_create_schema.sql ロールID GUID化（4件） | ✅ 完了 |
| 1.2 | 02_initial_data.sql 全ID参照 GUID化（47件） | ✅ 完了 |
| 2 | DbInitializer.cs 定数 GUID化（9件） | ✅ 完了 |
| 3 | 動作検証（ビルド・DB再作成・ログイン確認） | ✅ 完了 |

### DB投入確認
| テーブル | 件数 | ID形式 |
|---------|------|--------|
| AspNetUsers | 5件 | ✅ GUID形式 |
| AspNetRoles | 4件 | ✅ GUID形式 |
| AspNetUserRoles | 5件 | ✅ GUID形式 |

### 備考
- テストプロジェクトのビルドエラー（80件）はStep 12で対応予定

---

**作成者**: MainAgent
**ステータス**: ✅ 完了
