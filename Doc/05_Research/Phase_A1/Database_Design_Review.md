# データベース設計書確認結果

**Phase**: A1 - 基本認証システム  
**確認日**: 2025-07-16  
**確認者**: Claude Code  

## 該当Phase関連テーブル構造

### 実装対象テーブル一覧

#### ASP.NET Core Identity関連
1. **AspNetUsers** - ユーザー基本情報・認証情報
2. **AspNetRoles** - システムロール定義
3. **AspNetUserRoles** - ユーザー・ロール関連

#### プロジェクト関連（最小限）
4. **Projects** - プロジェクト基本情報（初期データ用）
5. **UserProjects** - ユーザー・プロジェクト関連（将来用）

### 各テーブルのカラム定義詳細

#### AspNetUsers
- **認証関連カラム**:
  - Id (VARCHAR(450), PK) - GUID文字列
  - Email (VARCHAR(256)) - ログインID兼用
  - PasswordHash (TEXT) - パスワードハッシュ
  - SecurityStamp (TEXT) - セキュリティスタンプ
  - IsFirstLogin (BOOLEAN) - 初回ログインフラグ
  - InitialPassword (VARCHAR(100)) - 初期パスワード（一時的）
  
- **追加業務カラム**:
  - Name (VARCHAR(50), NOT NULL) - ユーザー氏名
  - UpdatedAt (TIMESTAMPTZ) - 最終更新日時
  - IsDeleted (BOOLEAN) - 論理削除フラグ

### F#↔C#型変換マッピング

#### PostgreSQL → F#型変換
```
VARCHAR(450) → string
VARCHAR(256) → string  
VARCHAR(50) → string
VARCHAR(100) → string option
TEXT → string
BOOLEAN → bool
TIMESTAMPTZ → DateTime
BIGSERIAL → int64
```

#### F#型 → C#型変換
```
string → string
string option → string? (nullable)
bool → bool
DateTime → DateTime
int64 → long
```

### ASP.NET Core Identity統合ポイント

#### Identity関連テーブルとの関連性
- **AspNetUsers**: IdentityUser<string>を継承したApplicationUserクラス使用
- **AspNetRoles**: IdentityRole<string>使用（標準）
- **AspNetUserRoles**: IdentityUserRole<string>使用（標準）

#### 認証・承認に関わるテーブル設計
- **主キー型**: string（GUID文字列）をIdentityで使用
- **Email認証**: Emailカラムをユーザー名として使用
- **ロール管理**: 4種類の固定ロール（SuperUser, ProjectManager, DomainApprover, GeneralUser）

#### カスタムフィールドの実装詳細
- **Name**: ユーザー表示名（必須）
- **IsFirstLogin**: 初回ログイン判定用
- **InitialPassword**: 管理者による初期パスワード設定用（セキュリティ注意）
- **UpdatedAt/IsDeleted**: 監査・論理削除用

### 実装時の重要事項

#### データベースアクセス時の注意点
1. **GUID生成**: C#側でGuid.NewGuid().ToString()使用
2. **TIMESTAMPTZ**: UTCとして扱い、表示時にローカル変換
3. **論理削除**: IsDeleted=trueのユーザーは認証不可にする
4. **初期パスワード**: ログイン成功後に必ずNULL化

#### 性能に関わる設計事項
- **インデックス**: Email, NormalizedEmail, IsDeletedに設定済み
- **複合インデックス**: UserProjectsの(UserId, ProjectId)
- **N+1問題**: UserProjects取得時はInclude使用推奨

#### 制約違反の可能性がある操作
1. **Email重複**: 一意制約によりエラー（事前チェック必須）
2. **外部キー違反**: UserProjects挿入時のユーザー/プロジェクト存在確認
3. **NULL制約**: Name必須、その他Identity標準カラムはNULL許可

### Phase A1での実装スコープ

#### 必須実装
- AspNetUsers, AspNetRoles, AspNetUserRolesの基本CRUD
- 初期スーパーユーザー自動生成
- ログイン・ログアウト機能

#### 将来実装準備
- Projects, UserProjectsテーブル作成（データ投入は後Phase）
- ロールベース認証の基盤構築

#### 除外事項
- Domains以降のテーブル（Phase B以降）
- パスワードリセット機能（Phase A3）
- 複雑な権限制御（Phase B2以降）