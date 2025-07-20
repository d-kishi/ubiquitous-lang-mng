# Phase A2 データベース設計書確認結果

**確認日**: 2025-07-20  
**対象**: ユーザー管理機能関連テーブル  

## 📊 確認したテーブル構造

### AspNetUsers（ユーザー管理）
```sql
- Id: VARCHAR(450) PK -- ユーザーID（主キー、GUID文字列）
- UserName: VARCHAR(256) NULL -- ユーザー名
- NormalizedUserName: VARCHAR(256) NULL -- 正規化ユーザー名
- Email: VARCHAR(256) NULL -- メールアドレス
- NormalizedEmail: VARCHAR(256) NULL -- 正規化メールアドレス
- EmailConfirmed: BOOLEAN NOT NULL DEFAULT false -- メールアドレス確認フラグ
- PasswordHash: TEXT NULL -- パスワードハッシュ値
- SecurityStamp: TEXT NULL -- セキュリティスタンプ
- ConcurrencyStamp: TEXT NULL -- 同時実行制御スタンプ
- LockoutEnd: TIMESTAMPTZ NULL -- ロックアウト終了日時
- LockoutEnabled: BOOLEAN NOT NULL DEFAULT false -- ロックアウト有効フラグ
- AccessFailedCount: INTEGER NOT NULL DEFAULT false -- アクセス失敗回数
- Name: VARCHAR(50) NOT NULL -- ユーザー氏名
- IsFirstLogin: BOOLEAN NOT NULL DEFAULT true -- 初回ログインフラグ
- UpdatedAt: TIMESTAMPTZ NOT NULL DEFAULT NOW() -- 最終更新日時
- IsDeleted: BOOLEAN NOT NULL DEFAULT false -- 論理削除フラグ
- InitialPassword: VARCHAR(100) NULL -- 初期パスワード（平文・一時的）
- PasswordResetToken: TEXT NULL -- パスワードリセットトークン
- PasswordResetExpiry: TIMESTAMPTZ NULL -- リセットトークン有効期限
```

**重要な制約**：
- 一意制約: NormalizedUserName, NormalizedEmail
- インデックス: IX_AspNetUsers_NormalizedEmail, IX_AspNetUsers_IsDeleted, IX_AspNetUsers_IsFirstLogin
- ASP.NET Core Identity標準カラム + カスタムプロパティ

### AspNetRoles（ロール管理）
```sql
- Id: VARCHAR(450) PK -- ロールID（主キー）
- Name: VARCHAR(256) NULL -- ロール名
- NormalizedName: VARCHAR(256) NULL -- 正規化ロール名
- ConcurrencyStamp: TEXT NULL -- 同時実行制御スタンプ
```

**制約・初期データ**：
- 一意制約: NormalizedName
- 初期データ: SuperUser, ProjectManager, DomainApprover, GeneralUser

### AspNetUserRoles（ユーザー・ロール関連）
```sql
- UserId: VARCHAR(450) PK, FK -- ユーザーID
- RoleId: VARCHAR(450) PK, FK -- ロールID
```

**制約**：
- 複合主キー: (UserId, RoleId)
- 外部キー: UserId → AspNetUsers.Id, RoleId → AspNetRoles.Id

### Projects（プロジェクト管理）
```sql
- ProjectId: BIGSERIAL PK -- プロジェクトID（主キー）
- ProjectName: VARCHAR(50) NOT NULL UNIQUE -- プロジェクト名
- Description: TEXT NULL -- プロジェクト説明
- UpdatedBy: VARCHAR(450) NOT NULL -- 最終更新者ID
- UpdatedAt: TIMESTAMPTZ NOT NULL DEFAULT NOW() -- 最終更新日時
- IsDeleted: BOOLEAN NOT NULL DEFAULT false -- 論理削除フラグ
```

**制約**：
- 一意制約: ProjectName（削除済み含む）
- インデックス: IX_Projects_ProjectName, IX_Projects_IsDeleted

### UserProjects（ユーザー・プロジェクト関連）
```sql
- UserProjectId: BIGSERIAL PK -- ユーザープロジェクトID（主キー）
- UserId: VARCHAR(450) NOT NULL FK -- ユーザーID
- ProjectId: BIGINT NOT NULL FK -- プロジェクトID
- UpdatedBy: VARCHAR(450) NOT NULL -- 最終更新者ID
- UpdatedAt: TIMESTAMPTZ NOT NULL DEFAULT NOW() -- 最終更新日時
```

**制約**：
- 複合一意制約: (UserId, ProjectId)
- 外部キー: UserId → AspNetUsers.Id, ProjectId → Projects.ProjectId
- インデックス: IX_UserProjects_UserProject, IX_UserProjects_ProjectId, IX_UserProjects_UserId

## 🔍 F#⇔C#型変換で注意すべき点

### 1. ID型の変換
- **AspNetUsers.Id**: VARCHAR(450) → C# string → F# UserId型
- **Projects.ProjectId**: BIGSERIAL → C# long → F# ProjectId型
- **UserProjects.UserProjectId**: BIGSERIAL → C# long

### 2. Null許容型の扱い
- **ASP.NET Core Identity標準カラム**: 多くがNULL許容（UserName, Email等）
- **カスタムカラム**: Name, UpdatedBy等はNOT NULL
- F#での扱い: NULL許容は option<'T>、NOT NULLは直接型

### 3. 日時型
- **PostgreSQL TIMESTAMPTZ** → C# DateTimeOffset → F# DateTimeOffset
- タイムゾーン対応が必要

### 4. 論理削除フラグ
- **IsDeleted**: BOOLEAN → C# bool → F# bool
- ドメイン層では削除済みユーザーを除外するロジック必要

### 5. データ型の誤記
- **AccessFailedCount**: INTEGER NOT NULL DEFAULT false（設計書の誤記）
  → 実際はDEFAULT 0であるべき

## 🎯 実装時の重要ポイント

### ASP.NET Core Identity統合
1. **ApplicationUser作成**: IdentityUser<string>継承
2. **カスタムプロパティ追加**: Name, IsFirstLogin
3. **UserManager<ApplicationUser>使用**: 標準的なユーザー操作

### 権限管理実装
1. **ロールベース**: AspNetRoles使用
2. **プロジェクトスコープ**: UserProjectsとの結合
3. **Claimsベース拡張**: 動的権限生成

### 検索・フィルタリング最適化
1. **Email部分一致**: NormalizedEmailカラムに対してpg_trgm GINインデックス推奨
2. **Name部分一致**: 同様にGINインデックス
3. **プロジェクトフィルタ**: UserProjectsテーブルとのJOINが必要
4. **既存インデックス活用**: IsDeleted, IsFirstLoginインデックスの活用

### データベース設計の修正必要箇所
1. **AccessFailedCount**: DEFAULT false → DEFAULT 0 への修正が必要
2. **BIGSERIAL**: PostgreSQL固有の型（BIGINT + SEQUENCE）
3. **UpdatedBy/UpdatedAt**: 全テーブルで統一された監査カラム

---

**確認完了**: データベース設計書の内容を正確に把握し、実装時の注意点を整理