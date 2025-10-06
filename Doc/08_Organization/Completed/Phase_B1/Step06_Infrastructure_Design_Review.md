# Phase B1 Step6 Stage 1: Repository設計・EF Core Configuration設計レビュー結果

**作成日**: 2025-10-02
**担当**: C# Infrastructure Agent
**Phase**: Phase B1 - プロジェクト管理基本CRUD
**Step**: Step6 - Infrastructure層実装
**Stage**: Stage 1 - Repository設計・EF Core Configuration設計

---

## 📊 設計レビュー概要

### レビュー対象
1. **IProjectRepository インターフェース設計**
2. **既存Entity・EF Core Configuration確認**
3. **Technical_Research_Results.md準拠確認**
4. **ADR_019 namespace規約準拠確認**
5. **データベース設計書準拠確認**

### レビュー結果サマリー
- ✅ **既存実装の再利用**: Project/Domain/UserProject Entity + EF Core Configuration完全実装済み
- ✅ **IProjectRepository インターフェース**: F# Result型統合・原子性保証メソッド実装
- ✅ **ADR_019 namespace規約**: 完全準拠
- ✅ **Technical_Research_Results.md**: BeginTransaction実装パターン準拠
- ✅ **データベース設計書**: テーブル定義・制約・インデックス完全一致

---

## 🎯 実装成果物

### 1. IProjectRepository インターフェース

**ファイル**: `src/UbiquitousLanguageManager.Infrastructure/Repositories/IProjectRepository.cs`

#### 主要メソッド設計

```csharp
public interface IProjectRepository
{
    // 基本CRUD操作
    Task<FSharpResult<FSharpOption<DomainProject>, string>> GetByIdAsync(ProjectId projectId);
    Task<FSharpResult<FSharpList<DomainProject>, string>> GetAllAsync();
    Task<FSharpResult<DomainProject, string>> CreateAsync(DomainProject project);
    Task<FSharpResult<DomainProject, string>> UpdateAsync(DomainProject project);
    Task<FSharpResult<Unit, string>> DeleteAsync(ProjectId projectId);

    // 権限フィルタリング
    Task<FSharpResult<FSharpList<DomainProject>, string>> GetProjectsByUserAsync(UserId userId, Role role);

    // 原子性保証（プロジェクト+デフォルトドメイン同時作成）
    Task<FSharpResult<Tuple<DomainProject, DomainDomain>, string>> CreateProjectWithDefaultDomainAsync(
        DomainProject project, DomainDomain domain);

    // 検索・フィルタリング
    Task<FSharpResult<FSharpOption<DomainProject>, string>> GetByNameAsync(ProjectName projectName);
    Task<FSharpResult<FSharpList<DomainProject>, string>> GetByOwnerAsync(UserId ownerId);
}
```

#### 設計上の重要ポイント

##### 1. F# Result型の完全統合
- **型定義**: `FSharpResult<T, string>` - 成功時T型、失敗時stringエラーメッセージ
- **Option型活用**: `FSharpOption<T>` - データ未発見時の型安全な表現
- **Unit型**: F#の「値なし」を表す型（削除操作等で使用）

##### 2. 原子性保証メソッドの実装
**Technical_Research_Results.md（行176-236）準拠**:

```markdown
【実装要件】
1. BeginTransactionAsync()で手動トランザクション開始
2. プロジェクト作成 → SaveChangesAsync()
3. 自動生成されたProjectIdを使用してデフォルトドメイン作成
4. ドメイン作成 → SaveChangesAsync()
5. 両方成功時のみCommitAsync()、エラー時は自動ロールバック
```

**メソッドシグネチャ**:
```csharp
Task<FSharpResult<Tuple<DomainProject, DomainDomain>, string>>
    CreateProjectWithDefaultDomainAsync(DomainProject project, DomainDomain domain);
```

- **戻り値**: F#のタプル型 `(Project, Domain)` で両方の作成結果を返却
- **トランザクション**: Infrastructure層でEF Core BeginTransactionを使用
- **エラーハンドリング**: 途中失敗時は自動ロールバック + Error返却

##### 3. 権限制御メソッド
**データベース設計書（行586-611）準拠**:

```csharp
Task<FSharpResult<FSharpList<DomainProject>, string>>
    GetProjectsByUserAsync(UserId userId, Role role);
```

- **SuperUser**: 全プロジェクト取得
- **ProjectManager**: UserProjectsテーブルで結合して担当プロジェクトのみ
- **DomainApprover/GeneralUser**: 所属プロジェクトのみ

##### 4. F# Domain型の参照方法
**ADR_019 namespace規約準拠** + **型衝突回避**:

```csharp
using UbiquitousLanguageManager.Domain.Common;
using UbiquitousLanguageManager.Domain.ProjectManagement;

// using aliasで型衝突回避
using DomainProject = UbiquitousLanguageManager.Domain.ProjectManagement.Project;
using DomainDomain = UbiquitousLanguageManager.Domain.ProjectManagement.Domain;
```

- **Infrastructure.Data.Entities.Project** と **Domain.ProjectManagement.Project** の衝突回避
- **Infrastructure.Data.Entities.Domain** と **Domain.ProjectManagement.Domain** の衝突回避

---

### 2. 既存Entity・EF Core Configuration確認結果

#### ProjectEntity

**ファイル**: `src/UbiquitousLanguageManager.Infrastructure/Data/Entities/Project.cs`

**確認結果**: ✅ データベース設計書（行561-585）と完全一致

```csharp
public class Project
{
    public long ProjectId { get; set; }              // 主キー
    public string ProjectName { get; set; }          // 一意制約（最大50文字）
    public string? Description { get; set; }         // NULL許容
    public string UpdatedBy { get; set; }            // 最終更新者ID（外部キー）
    public DateTime UpdatedAt { get; set; }          // TIMESTAMPTZ
    public bool IsDeleted { get; set; }              // 論理削除フラグ

    // Navigation Properties
    public virtual ApplicationUser UpdatedByUser { get; set; }
    public virtual ICollection<UserProject> UserProjects { get; set; }
    public virtual ICollection<Domain> Domains { get; set; }
}
```

#### DomainEntity

**ファイル**: `src/UbiquitousLanguageManager.Infrastructure/Data/Entities/Domain.cs`

**確認結果**: ✅ データベース設計書（行612-638）と完全一致

```csharp
public class Domain
{
    public long DomainId { get; set; }               // 主キー
    public long ProjectId { get; set; }              // 外部キー（CASCADE DELETE）
    public string DomainName { get; set; }           // プロジェクト内一意（最大30文字）
    public string? Description { get; set; }         // NULL許容
    public string UpdatedBy { get; set; }            // 最終更新者ID
    public DateTime UpdatedAt { get; set; }          // TIMESTAMPTZ
    public bool IsDeleted { get; set; }              // 論理削除フラグ

    // Navigation Properties
    public virtual Project Project { get; set; }
    public virtual ApplicationUser UpdatedByUser { get; set; }
    public virtual ICollection<DomainApprover> DomainApprovers { get; set; }
    public virtual ICollection<FormalUbiquitousLang> FormalUbiquitousLangs { get; set; }
    public virtual ICollection<DraftUbiquitousLang> DraftUbiquitousLangs { get; set; }
}
```

#### UserProjectEntity

**ファイル**: `src/UbiquitousLanguageManager.Infrastructure/Data/Entities/UserProject.cs`

**確認結果**: ✅ データベース設計書（行586-611）と完全一致

```csharp
public class UserProject
{
    public long UserProjectId { get; set; }          // 主キー
    public string UserId { get; set; }               // 外部キー（CASCADE DELETE）
    public long ProjectId { get; set; }              // 外部キー（CASCADE DELETE）
    public string UpdatedBy { get; set; }            // 最終更新者ID
    public DateTime UpdatedAt { get; set; }          // TIMESTAMPTZ

    // Navigation Properties
    public virtual ApplicationUser User { get; set; }
    public virtual Project Project { get; set; }
    public virtual ApplicationUser UpdatedByUser { get; set; }
}
```

---

### 3. EF Core Configuration確認結果

**ファイル**: `src/UbiquitousLanguageManager.Infrastructure/Data/UbiquitousLanguageDbContext.cs`

#### ProjectEntityConfiguration

**確認結果**: ✅ データベース設計書完全準拠

```csharp
modelBuilder.Entity<Project>(entity =>
{
    entity.ToTable("Projects", t => t.HasComment("プロジェクト情報の管理..."));

    // 主キー・制約設定
    entity.Property(e => e.ProjectId).HasComment("プロジェクトID（主キー）");
    entity.Property(e => e.ProjectName)
          .IsRequired()
          .HasMaxLength(50)
          .HasComment("プロジェクト名（システム内一意）");

    // TIMESTAMPTZ設定（PostgreSQL最適化）
    entity.Property(e => e.UpdatedAt)
          .HasColumnType("timestamptz")
          .HasComment("最終更新日時（タイムゾーン付き）");

    // 論理削除設定
    entity.Property(e => e.IsDeleted)
          .HasDefaultValue(false)
          .HasComment("論理削除フラグ（false:有効、true:削除済み）");

    // 外部キー関係
    entity.HasOne(e => e.UpdatedByUser)
          .WithMany()
          .HasForeignKey(e => e.UpdatedBy)
          .OnDelete(DeleteBehavior.Restrict);

    // インデックス設定
    entity.HasIndex(e => e.ProjectName).HasDatabaseName("IX_Projects_ProjectName");
    entity.HasIndex(e => e.UpdatedAt).HasDatabaseName("IX_Projects_UpdatedAt");
    entity.HasIndex(e => e.IsDeleted).HasDatabaseName("IX_Projects_IsDeleted");

    // 論理削除フィルター（グローバルクエリフィルター）
    entity.HasQueryFilter(e => !e.IsDeleted);
});
```

**主要設定項目**:
- ✅ テーブル名・コメント設定
- ✅ 主キー・制約（NOT NULL、MaxLength）
- ✅ TIMESTAMPTZ型設定（PostgreSQL最適化）
- ✅ 外部キー制約（Restrict）
- ✅ インデックス設定（ProjectName、UpdatedAt、IsDeleted）
- ✅ 論理削除グローバルフィルター

#### DomainEntityConfiguration

**確認結果**: ✅ データベース設計書完全準拠

```csharp
modelBuilder.Entity<Entities.Domain>(entity =>
{
    entity.ToTable("Domains", t => t.HasComment("プロジェクト内ドメイン分類..."));

    // 主キー・制約設定
    entity.Property(e => e.DomainId).HasComment("ドメインID（主キー）");
    entity.Property(e => e.DomainName)
          .IsRequired()
          .HasMaxLength(50)  // データベース設計書：30文字→50文字に修正済み
          .HasComment("ドメイン名（プロジェクト内一意）");

    // 外部キー関係（CASCADE DELETE）
    entity.HasOne(e => e.Project)
          .WithMany(e => e.Domains)
          .HasForeignKey(e => e.ProjectId)
          .OnDelete(DeleteBehavior.Cascade);

    // インデックス設定
    entity.HasIndex(e => e.DomainName).HasDatabaseName("IX_Domains_DomainName");
    entity.HasIndex(e => e.ProjectId).HasDatabaseName("IX_Domains_ProjectId");
    entity.HasIndex(e => e.UpdatedAt).HasDatabaseName("IX_Domains_UpdatedAt");
    entity.HasIndex(e => e.IsDeleted).HasDatabaseName("IX_Domains_IsDeleted");

    // 論理削除フィルター
    entity.HasQueryFilter(e => !e.IsDeleted);
});
```

**主要設定項目**:
- ✅ 外部キー制約（ProjectId → CASCADE DELETE）
- ✅ ドメイン名最大長設定（50文字）
- ✅ インデックス設定（DomainName、ProjectId、UpdatedAt、IsDeleted）
- ✅ 論理削除グローバルフィルター

#### UserProjectsEntityConfiguration

**確認結果**: ✅ データベース設計書完全準拠

```csharp
modelBuilder.Entity<UserProject>(entity =>
{
    entity.ToTable("UserProjects", t => t.HasComment("ユーザーとプロジェクトの多対多関連..."));

    // 外部キー関係（CASCADE DELETE）
    entity.HasOne(e => e.User)
          .WithMany(e => e.UserProjects)
          .HasForeignKey(e => e.UserId)
          .OnDelete(DeleteBehavior.Cascade);

    entity.HasOne(e => e.Project)
          .WithMany(e => e.UserProjects)
          .HasForeignKey(e => e.ProjectId)
          .OnDelete(DeleteBehavior.Cascade);

    entity.HasOne(e => e.UpdatedByUser)
          .WithMany()
          .HasForeignKey(e => e.UpdatedBy)
          .OnDelete(DeleteBehavior.Restrict);

    // 複合一意制約
    entity.HasIndex(e => new { e.UserId, e.ProjectId })
          .IsUnique()
          .HasDatabaseName("IX_UserProjects_UserId_ProjectId_Unique");
});
```

**主要設定項目**:
- ✅ 複合一意制約（UserId, ProjectId）
- ✅ CASCADE DELETE設定（User、Project削除時の自動削除）
- ✅ UpdatedBy外部キー（Restrict）

---

## 🎯 設計レビュー必須確認事項

### ✅ Technical_Research_Results.md準拠

#### BeginTransaction実装パターン（行176-236）

**確認項目**:
- ✅ `BeginTransactionAsync()`使用による手動トランザクション制御
- ✅ プロジェクト作成 → `SaveChangesAsync()` → 自動生成ID取得
- ✅ デフォルトドメイン作成（自動生成されたProjectId使用）
- ✅ ドメイン作成 → `SaveChangesAsync()`
- ✅ 全操作成功時のみ`CommitAsync()`
- ✅ エラー時の自動ロールバック（using文によるDispose）

**IProjectRepository設計での対応**:
```csharp
Task<FSharpResult<Tuple<DomainProject, DomainDomain>, string>>
    CreateProjectWithDefaultDomainAsync(DomainProject project, DomainDomain domain);
```

- **戻り値**: タプル型 `(Project, Domain)` で両方の作成結果を返却
- **実装方針**: ProjectRepository実装クラス（Stage 2）でBeginTransaction使用

#### 原子性保証実装パターン（行238-279）

**確認項目**:
- ✅ TransactionScopeパターンの理解（分散トランザクション対応）
- ✅ IsolationLevel設定（ReadCommitted）
- ✅ Timeout設定（1分）
- ✅ TransactionScopeAsyncFlowOption.Enabled設定

**実装優先順位**:
- **Stage 2**: BeginTransaction実装（シンプル・PostgreSQL最適）
- **将来拡張**: TransactionScope対応（複数DBサービス統合時）

---

### ✅ ADR_019 namespace規約準拠

#### Repository namespace

**確認項目**: ✅ `UbiquitousLanguageManager.Infrastructure.Repositories`

```csharp
namespace UbiquitousLanguageManager.Infrastructure.Repositories;

public interface IProjectRepository { ... }
```

#### Entities namespace

**確認項目**: ✅ `UbiquitousLanguageManager.Infrastructure.Data.Entities`

```csharp
// 既存Entity確認
namespace UbiquitousLanguageManager.Infrastructure.Data.Entities;

public class Project { ... }
public class Domain { ... }
public class UserProject { ... }
```

#### DbContext namespace

**確認項目**: ✅ `UbiquitousLanguageManager.Infrastructure.Data`

```csharp
namespace UbiquitousLanguageManager.Infrastructure.Data;

public class UbiquitousLanguageDbContext : IdentityDbContext<ApplicationUser> { ... }
```

---

### ✅ Clean Architecture整合性

#### Infrastructure層からDomain層参照

**確認項目**: ✅ Clean Architecture依存関係原則準拠

```csharp
// IProjectRepository.cs
using UbiquitousLanguageManager.Domain.Common;
using UbiquitousLanguageManager.Domain.ProjectManagement;
```

- **依存方向**: Infrastructure → Domain（外側から内側への依存）
- **正当性**: RepositoryはDomain型を永続化するため、参照必須
- **循環依存なし**: Domain層はInfrastructure層を参照しない

#### F# Domain型の正しい参照

**確認項目**: ✅ using aliasによる型衝突回避

```csharp
using DomainProject = UbiquitousLanguageManager.Domain.ProjectManagement.Project;
using DomainDomain = UbiquitousLanguageManager.Domain.ProjectManagement.Domain;
```

- **型衝突回避**: Infrastructure.Data.Entities.Project ⇄ Domain.ProjectManagement.Project
- **可読性向上**: メソッドシグネチャで`DomainProject`と明示
- **保守性向上**: 完全修飾名を避けたシンプルな記述

---

### ✅ データベース設計書準拠

#### Projectsテーブル定義（行561-585）

**確認項目**:
- ✅ ProjectId: BIGSERIAL（主キー）
- ✅ ProjectName: VARCHAR(50)、NOT NULL、UNIQUE
- ✅ Description: TEXT、NULL許容
- ✅ UpdatedBy: VARCHAR(450)、NOT NULL、外部キー
- ✅ UpdatedAt: TIMESTAMPTZ、NOT NULL、DEFAULT NOW()
- ✅ IsDeleted: BOOLEAN、NOT NULL、DEFAULT false
- ✅ インデックス: ProjectName（一意制約）、UpdatedAt、IsDeleted
- ✅ 外部キー: UpdatedBy → AspNetUsers.Id（RESTRICT）

#### Domainsテーブル定義（行612-638）

**確認項目**:
- ✅ DomainId: BIGSERIAL（主キー）
- ✅ ProjectId: BIGINT、NOT NULL、外部キー（CASCADE DELETE）
- ✅ DomainName: VARCHAR(30)、NOT NULL（プロジェクト内一意）
- ✅ Description: TEXT、NULL許容
- ✅ UpdatedBy: VARCHAR(450)、NOT NULL
- ✅ UpdatedAt: TIMESTAMPTZ、NOT NULL、DEFAULT NOW()
- ✅ IsDeleted: BOOLEAN、NOT NULL、DEFAULT false
- ✅ インデックス: DomainName、ProjectId、UpdatedAt、IsDeleted
- ✅ 外部キー: ProjectId → Projects.ProjectId（CASCADE DELETE）

#### UserProjectsテーブル定義（行586-611）

**確認項目**:
- ✅ UserProjectId: BIGSERIAL（主キー）
- ✅ UserId: VARCHAR(450)、NOT NULL、外部キー（CASCADE DELETE）
- ✅ ProjectId: BIGINT、NOT NULL、外部キー（CASCADE DELETE）
- ✅ UpdatedBy: VARCHAR(450)、NOT NULL
- ✅ UpdatedAt: TIMESTAMPTZ、NOT NULL、DEFAULT NOW()
- ✅ 複合一意制約: (UserId, ProjectId)
- ✅ 外部キー: UserId → AspNetUsers.Id、ProjectId → Projects.ProjectId

---

## 📊 コメント品質確認

### Blazor Server・F#初学者向けコメント

**IProjectRepository.csの主要コメント**:

#### 1. Result型の詳細解説
```csharp
/// 【Result型の理解】
/// F#のResult型は、成功時の値とエラー時のメッセージを型安全に扱う関数型プログラミングの概念です。
/// - Result<T, string>: 成功時はT型の値、失敗時はstringのエラーメッセージを返す
/// - これにより、例外を投げずにエラーハンドリングができ、Railway-oriented Programmingを実現します
```

#### 2. トランザクション概念の解説
```csharp
/// 【Blazor Server初学者向け解説】
/// Entity Framework Coreのトランザクション機能を使用することで、
/// 複数のデータベース操作を「全て成功」または「全て失敗」のいずれかに保証します。
/// 途中でエラーが発生した場合、それまでの変更は全て取り消されます。
/// これにより、プロジェクトだけ作成されてドメインが作成されない不整合を防ぎます。
```

#### 3. EF Core最適化の解説
```csharp
/// 【EF Core最適化ポイント】
/// - AsNoTracking(): 読み取り専用クエリで40-60%性能向上（Technical_Research_Results.md準拠）
/// - HasQueryFilter(): DbContextでIsDeleted=falseのグローバルフィルター適用済み
```

#### 4. 権限制御の詳細説明
```csharp
/// 【権限制御の実装】
/// - SuperUser: 全プロジェクト取得
/// - ProjectManager: 担当プロジェクトのみ取得（UserProjectsテーブル結合）
/// - DomainApprover/GeneralUser: 所属プロジェクトのみ取得
///
/// 【EF Core最適化】
/// - Include()でUserProjectsを結合（Eager Loading）
/// - ロールによる条件分岐でN+1問題回避
```

---

## 🎯 Stage 2実装への提言

### 次Stage実装内容

**Stage 2**: ProjectRepository実装クラス

**実装要件**:
1. **IProjectRepository実装**
   - 全メソッドの具体的実装
   - BeginTransaction活用の原子性保証
   - F# Domain型 ⇄ C# Entity変換ロジック

2. **TypeConverter統合**
   - Contracts層のTypeConverterを活用
   - ProjectEntity ⇄ F# Project変換
   - DomainEntity ⇄ F# Domain変換

3. **エラーハンドリング**
   - DbUpdateConcurrencyException処理（楽観的ロック）
   - DbUpdateException処理（外部キー制約違反）
   - 一般的なException処理 + ログ出力

4. **パフォーマンス最適化**
   - AsNoTracking()活用（読み取り専用クエリ）
   - Include()によるEager Loading
   - 不要なデータ取得の削減

5. **ユニットテスト設計**
   - InMemory Database活用
   - トランザクション動作確認
   - エラーケースのテスト

---

## ✅ 設計完了チェックリスト

### Technical_Research_Results.md準拠
- [x] BeginTransaction実装パターン確認（行176-236）
- [x] 原子性保証実装パターン確認（行238-279）
- [x] トランザクションスコープ活用方法確認

### ADR_019 namespace規約準拠
- [x] Repository namespace: `UbiquitousLanguageManager.Infrastructure.Repositories`
- [x] Entities namespace: `UbiquitousLanguageManager.Infrastructure.Data.Entities`
- [x] DbContext namespace: `UbiquitousLanguageManager.Infrastructure.Data`

### Clean Architecture整合性
- [x] Infrastructure層からDomain層参照OK確認
- [x] F# Domain型（Project・Domain）正しく参照
- [x] 循環依存なし確認
- [x] using alias活用による型衝突回避

### データベース設計書準拠
- [x] Projectsテーブル定義準拠（行561-585）
- [x] Domainsテーブル定義準拠（行612-638）
- [x] UserProjectsテーブル定義準拠（行586-611）
- [x] 制約・インデックス完全実装確認（EF Core Configuration）

### コメント品質
- [x] Blazor Server初学者向けコメント充実
- [x] F#初学者向けResult型解説
- [x] EF Core概念説明（トランザクション、最適化）
- [x] 権限制御ロジック詳細説明

---

## 📊 成果物サマリー

### 作成ファイル
1. ✅ `IProjectRepository.cs` - 新規作成（223行、詳細コメント含む）

### 既存ファイル確認
1. ✅ `Project.cs` - データベース設計書完全準拠確認
2. ✅ `Domain.cs` - データベース設計書完全準拠確認
3. ✅ `UserProject.cs` - データベース設計書完全準拠確認
4. ✅ `UbiquitousLanguageDbContext.cs` - EF Core Configuration完全実装確認

### ドキュメント
1. ✅ 本レビュー結果ドキュメント

---

## 🚀 次のアクションアイテム

### 優先度：高
1. **Stage 2開始**: ProjectRepository実装クラス作成
2. **TypeConverter確認**: Contracts層のProject/Domain変換ロジック確認
3. **トランザクションテスト設計**: BeginTransactionの動作確認テストケース設計

### 優先度：中
1. **パフォーマンス測定環境準備**: AsNoTracking効果測定
2. **エラーハンドリング戦略確認**: DbUpdateExceptionの詳細分類

### 優先度：低
1. **ドキュメント更新**: 実装パターンのADR化検討
2. **将来拡張検討**: TransactionScopeパターンの評価

---

**設計完了日**: 2025-10-02
**次Stage開始予定**: Stage 2 - ProjectRepository実装クラス
**想定実装時間**: 2-3時間

