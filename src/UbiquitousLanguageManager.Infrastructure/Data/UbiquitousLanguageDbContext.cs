using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using UbiquitousLanguageManager.Infrastructure.Data.Entities;

namespace UbiquitousLanguageManager.Infrastructure.Data;

/// <summary>
/// ユビキタス言語管理システムのデータベースコンテキスト
/// Entity Framework Core + PostgreSQL の設定
/// ASP.NET Core Identity との統合
/// 
/// 【アーキテクチャ説明】
/// ApplicationUser を使用した IdentityDbContext により、
/// ASP.NET Core Identity の認証機能と独自の業務ロジックを統合しています。
/// </summary>
public class UbiquitousLanguageDbContext : IdentityDbContext<ApplicationUser>
{
    /// <summary>
    /// コンストラクタ: Entity Framework の設定を受け取る
    /// </summary>
    /// <param name="options">DbContext オプション</param>
    public UbiquitousLanguageDbContext(DbContextOptions<UbiquitousLanguageDbContext> options) 
        : base(options)
    {
    }

    // 🎯 アプリケーション固有のテーブル定義
    
    /// <summary>
    /// ユーザーテーブル
    /// ASP.NET Core Identity のApplicationUser
    /// </summary>
    public new DbSet<ApplicationUser> Users { get; set; } = null!;

    // 🏢 プロジェクト管理テーブル
    /// <summary>
    /// プロジェクトテーブル
    /// ユビキタス言語管理の対象となるプロジェクトの集合
    /// </summary>
    public DbSet<Project> Projects { get; set; } = null!;
    
    /// <summary>
    /// ユーザー・プロジェクト関係テーブル
    /// ユーザーとプロジェクトの多対多関係を管理
    /// </summary>
    public DbSet<UserProject> UserProjects { get; set; } = null!;

    // 🌐 ドメイン管理テーブル
    /// <summary>
    /// ドメインテーブル
    /// 各プロジェクト配下の業務ドメイン定義の集合
    /// </summary>
    public DbSet<Entities.Domain> Domains { get; set; } = null!;
    
    /// <summary>
    /// ドメイン承認者テーブル
    /// 各ドメインに対する承認権限を持つユーザーの管理
    /// </summary>
    public DbSet<DomainApprover> DomainApprovers { get; set; } = null!;

    // 📚 ユビキタス言語管理テーブル
    /// <summary>
    /// 正式ユビキタス言語テーブル
    /// 承認済みの正式なユビキタス言語定義の集合
    /// </summary>
    public DbSet<FormalUbiquitousLang> FormalUbiquitousLanguages { get; set; } = null!;
    
    /// <summary>
    /// 下書きユビキタス言語テーブル
    /// 申請中・レビュー中の下書き状態のユビキタス言語定義の集合
    /// </summary>
    public DbSet<DraftUbiquitousLang> DraftUbiquitousLanguages { get; set; } = null!;
    
    /// <summary>
    /// 関連ユビキタス言語テーブル
    /// 正式なユビキタス言語間の関連性（類義語・対義語等）の管理
    /// </summary>
    public DbSet<RelatedUbiquitousLang> RelatedUbiquitousLanguages { get; set; } = null!;
    
    /// <summary>
    /// 下書きユビキタス言語関係テーブル
    /// 下書き状態のユビキタス言語と正式なユビキタス言語の関連性管理
    /// </summary>
    public DbSet<DraftUbiquitousLangRelation> DraftUbiquitousLanguageRelations { get; set; } = null!;
    
    /// <summary>
    /// 正式ユビキタス言語履歴テーブル
    /// 正式なユビキタス言語の変更履歴管理
    /// </summary>
    public DbSet<FormalUbiquitousLangHistory> FormalUbiquitousLanguageHistories { get; set; } = null!;

    /// <summary>
    /// モデル設定: Entity Framework の詳細設定
    /// テーブル構造、制約、インデックスの定義
    /// </summary>
    /// <param name="modelBuilder">モデルビルダー</param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // 🔐 ASP.NET Core Identity のテーブル設定
        base.OnModelCreating(modelBuilder);
        
        // 🚫 不要なIdentityテーブルを除外（LoginとTokenのみ - 使用しない機能）
        modelBuilder.Ignore<Microsoft.AspNetCore.Identity.IdentityUserLogin<string>>();
        modelBuilder.Ignore<Microsoft.AspNetCore.Identity.IdentityUserToken<string>>();

        // 🔐 ASP.NET Core Identity テーブルの詳細設定（Claims含む標準実装）
        ConfigureIdentityTables(modelBuilder);

        // 🔐 ApplicationUser（Identity統合ユーザー）の詳細設定
        ConfigureApplicationUser(modelBuilder);

        // 🏢 プロジェクト管理エンティティの設定
        ConfigureProjectEntities(modelBuilder);

        // 🌐 ドメイン管理エンティティの設定
        ConfigureDomainEntities(modelBuilder);

        // 📚 ユビキタス言語管理エンティティの設定
        ConfigureUbiquitousLanguageEntities(modelBuilder);
    }

    /// <summary>
    /// ASP.NET Core Identity テーブルの基本設定とコメント
    /// initスキーマのコメント定義に準拠
    /// 
    /// 【F#初学者向け解説】
    /// ASP.NET Core Identityは.NET標準の認証・認可システムです。
    /// Claimsテーブルを含めることで、将来的な権限管理の拡張性を確保します。
    /// </summary>
    /// <param name="modelBuilder">モデルビルダー</param>
    private void ConfigureIdentityTables(ModelBuilder modelBuilder)
    {
        // AspNetUsers テーブルコメント設定
        modelBuilder.Entity<ApplicationUser>()
                   .ToTable("AspNetUsers", t => t.HasComment("ASP.NET Core Identity ユーザー情報とカスタムプロフィール"));

        // AspNetRoles テーブルコメント設定
        modelBuilder.Entity<Microsoft.AspNetCore.Identity.IdentityRole>()
                   .ToTable("AspNetRoles", t => t.HasComment("ASP.NET Core Identity ロール管理"));

        // AspNetUserRoles テーブルコメント設定
        modelBuilder.Entity<Microsoft.AspNetCore.Identity.IdentityUserRole<string>>()
                   .ToTable("AspNetUserRoles", t => t.HasComment("ASP.NET Core Identity ユーザー・ロール関連"));

        // AspNetUserClaims テーブルコメント設定
        modelBuilder.Entity<Microsoft.AspNetCore.Identity.IdentityUserClaim<string>>()
                   .ToTable("AspNetUserClaims", t => t.HasComment("ASP.NET Core Identity ユーザークレーム管理"));

        // AspNetRoleClaims テーブルコメント設定
        modelBuilder.Entity<Microsoft.AspNetCore.Identity.IdentityRoleClaim<string>>()
                   .ToTable("AspNetRoleClaims", t => t.HasComment("ASP.NET Core Identity ロールクレーム管理"));
    }

    /// <summary>
    /// ApplicationUser（ASP.NET Core Identity）の詳細設定
    /// PostgreSQL 固有の最適化と制約設定
    ///
    /// 【Blazor Server初学者向け解説】
    /// ここで設定したインデックスや制約は、データベースの性能とデータ整合性を保証します。
    /// 特に、Email のユニーク制約により、同じメールアドレスでの重複登録を防ぎます。
    /// </summary>
    /// <param name="modelBuilder">モデルビルダー</param>
    private void ConfigureApplicationUser(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ApplicationUser>(entity =>
        {
            // ASP.NET Core Identity 標準列のコメント設定
            entity.Property(e => e.Id)
                  .HasComment("ユーザーID（主キー、GUID形式）");

            entity.Property(e => e.UserName)
                  .HasComment("ユーザー名（ログイン用）");

            entity.Property(e => e.NormalizedUserName)
                  .HasComment("正規化ユーザー名（検索用）");

            entity.Property(e => e.Email)
                  .HasComment("メールアドレス");

            entity.Property(e => e.NormalizedEmail)
                  .HasComment("正規化メールアドレス（検索用）");

            entity.Property(e => e.EmailConfirmed)
                  .HasComment("メール確認済みフラグ");

            entity.Property(e => e.PasswordHash)
                  .HasComment("パスワードハッシュ値（Identity管理）");

            entity.Property(e => e.SecurityStamp)
                  .HasComment("セキュリティスタンプ（パスワード変更時更新）");

            entity.Property(e => e.ConcurrencyStamp)
                  .HasComment("同時実行制御スタンプ");

            entity.Property(e => e.PhoneNumber)
                  .HasComment("電話番号");

            entity.Property(e => e.PhoneNumberConfirmed)
                  .HasComment("電話番号確認済みフラグ");

            entity.Property(e => e.TwoFactorEnabled)
                  .HasComment("二要素認証有効フラグ");

            entity.Property(e => e.LockoutEnd)
                  .HasComment("ロックアウト終了時間");

            entity.Property(e => e.LockoutEnabled)
                  .HasComment("ロックアウト有効フラグ");

            entity.Property(e => e.AccessFailedCount)
                  .HasComment("アクセス失敗回数");

            // 📧 業務固有プロパティの設定
            entity.Property(e => e.Name)
                  .IsRequired()
                  .HasMaxLength(50)
                  .HasComment("ユーザー氏名（カスタムフィールド）");

            entity.Property(e => e.IsFirstLogin)
                  .HasDefaultValue(true)
                  .HasComment("初回ログインフラグ（カスタムフィールド）");

            entity.Property(e => e.UpdatedAt)
                  .HasColumnType("timestamptz")
                  .HasDefaultValueSql("CURRENT_TIMESTAMP")
                  .HasComment("最終更新日時（タイムゾーン付き）");

            entity.Property(e => e.IsDeleted)
                  .HasDefaultValue(false)
                  .HasComment("論理削除フラグ（false:有効、true:削除済み）");

            entity.Property(e => e.InitialPassword)
                  .HasMaxLength(100)
                  .HasComment("初期パスワード（初回ログイン時まで保持）");

            // Phase A3必須プロパティ（パスワードリセット機能）
            entity.Property(e => e.PasswordResetToken)
                  .HasColumnType("text")
                  .HasComment("パスワードリセットトークン（Phase A3機能）");

            entity.Property(e => e.PasswordResetExpiry)
                  .HasColumnType("timestamptz")
                  .HasComment("リセットトークン有効期限（Phase A3機能）");

            entity.Property(e => e.UpdatedBy)
                  .IsRequired()
                  .HasMaxLength(450)
                  .HasComment("最終更新者ID");

            // DomainUserIdプロパティは設計書にない余計な実装のため削除

            // UserRoleインデックスは削除（ASP.NET Core Identity標準のRoles機能使用）

            // Phase A3機能関連インデックス追加
            entity.HasIndex(e => e.PasswordResetToken)
                  .HasDatabaseName("IX_ApplicationUsers_PasswordResetToken");

            entity.HasIndex(e => e.PasswordResetExpiry)
                  .HasDatabaseName("IX_ApplicationUsers_PasswordResetExpiry");

            entity.HasIndex(e => e.IsDeleted)
                  .HasDatabaseName("IX_ApplicationUsers_IsDeleted");

            entity.HasIndex(e => e.UpdatedAt)
                  .HasDatabaseName("IX_ApplicationUsers_UpdatedAt");

            // DomainUserIdインデックスは削除（設計書にない実装）

            // 🔐 ASP.NET Core Identity ロール関連のナビゲーションプロパティ設定
            // 【F#初学者向け解説】
            // ApplicationUser.Roles プロパティと AspNetUserRoles テーブル（IdentityUserRole<string>）の関連付けを定義します。
            // これにより、Include(u => u.Roles) で効率的にロール情報を取得できるようになります。
            // N+1問題（複数ユーザー取得時に毎回ロールクエリが発行される問題）を回避します。
            entity.HasMany(u => u.Roles)
                  .WithOne()
                  .HasForeignKey(ur => ur.UserId)
                  .IsRequired();

            // 🔐 論理削除されたユーザーを除外するグローバルフィルター
            entity.HasQueryFilter(e => !e.IsDeleted);
        });
    }

    /// <summary>
    /// プロジェクト管理エンティティの詳細設定
    /// </summary>
    /// <param name="modelBuilder">モデルビルダー</param>
    private void ConfigureProjectEntities(ModelBuilder modelBuilder)
    {
        // Project エンティティ設定
        modelBuilder.Entity<Project>(entity =>
        {
            // テーブルコメント設定
            entity.ToTable("Projects", t => t.HasComment("プロジェクト情報の管理とユーザー・ドメインとの関連制御"));
            entity.Property(e => e.ProjectId)
                  .HasComment("プロジェクトID（主キー）");

            entity.Property(e => e.ProjectName)
                  .IsRequired()
                  .HasMaxLength(50)
                  .HasComment("プロジェクト名（システム内一意）");

            entity.Property(e => e.Description)
                  .HasComment("プロジェクト説明");

            entity.Property(e => e.UpdatedBy)
                  .IsRequired()
                  .HasMaxLength(450)
                  .HasComment("最終更新者ユーザーID");

            entity.Property(e => e.UpdatedAt)
                  .HasColumnType("timestamptz")
                  .HasComment("最終更新日時（タイムゾーン付き）");

            entity.Property(e => e.IsDeleted)
                  .HasDefaultValue(false)
                  .HasComment("論理削除フラグ（false:有効、true:削除済み）");

            // 外部キー関係
            entity.HasOne(e => e.UpdatedByUser)
                  .WithMany()
                  .HasForeignKey(e => e.UpdatedBy)
                  .OnDelete(DeleteBehavior.Restrict);

            // インデックス
            entity.HasIndex(e => e.ProjectName)
                  .HasDatabaseName("IX_Projects_ProjectName");
            entity.HasIndex(e => e.UpdatedAt)
                  .HasDatabaseName("IX_Projects_UpdatedAt");
            entity.HasIndex(e => e.IsDeleted)
                  .HasDatabaseName("IX_Projects_IsDeleted");

            // 論理削除フィルター
            entity.HasQueryFilter(e => !e.IsDeleted);
        });

        // UserProject エンティティ設定
        modelBuilder.Entity<UserProject>(entity =>
        {
            // テーブルコメント設定
            entity.ToTable("UserProjects", t => t.HasComment("ユーザーとプロジェクトの多対多関連を管理、権限制御の基盤"));
            
            entity.Property(e => e.UserProjectId)
                  .HasComment("ユーザープロジェクトID（主キー）");

            entity.Property(e => e.UserId)
                  .HasComment("ユーザーID（外部キー）");

            entity.Property(e => e.ProjectId)
                  .HasComment("プロジェクトID（外部キー）");

            entity.Property(e => e.UpdatedBy)
                  .IsRequired()
                  .HasMaxLength(450)
                  .HasComment("最終更新者ユーザーID");

            entity.Property(e => e.UpdatedAt)
                  .HasColumnType("timestamptz")
                  .HasComment("最終更新日時（タイムゾーン付き）");

            // 外部キー関係
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

            // ユニーク制約
            entity.HasIndex(e => new { e.UserId, e.ProjectId })
                  .IsUnique()
                  .HasDatabaseName("IX_UserProjects_UserId_ProjectId_Unique");
        });
    }

    /// <summary>
    /// ドメイン管理エンティティの詳細設定
    /// </summary>
    /// <param name="modelBuilder">モデルビルダー</param>
    private void ConfigureDomainEntities(ModelBuilder modelBuilder)
    {
        // Domain エンティティ設定
        modelBuilder.Entity<Entities.Domain>(entity =>
        {
            // テーブルコメント設定
            entity.ToTable("Domains", t => t.HasComment("プロジェクト内ドメイン分類と承認権限の管理単位"));
            
            entity.Property(e => e.DomainId)
                  .HasComment("ドメインID（主キー）");

            entity.Property(e => e.ProjectId)
                  .HasComment("所属プロジェクトID");

            entity.Property(e => e.DomainName)
                  .IsRequired()
                  .HasMaxLength(50)
                  .HasComment("ドメイン名（プロジェクト内一意）");

            entity.Property(e => e.Description)
                  .HasComment("ドメイン説明");

            entity.Property(e => e.UpdatedBy)
                  .IsRequired()
                  .HasMaxLength(450)
                  .HasComment("最終更新者ユーザーID");

            entity.Property(e => e.UpdatedAt)
                  .HasColumnType("timestamptz")
                  .HasComment("最終更新日時（タイムゾーン付き）");

            entity.Property(e => e.IsDeleted)
                  .HasDefaultValue(false)
                  .HasComment("論理削除フラグ（false:有効、true:削除済み）");

            // 外部キー関係
            entity.HasOne(e => e.Project)
                  .WithMany(e => e.Domains)
                  .HasForeignKey(e => e.ProjectId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.UpdatedByUser)
                  .WithMany()
                  .HasForeignKey(e => e.UpdatedBy)
                  .OnDelete(DeleteBehavior.Restrict);

            // インデックス
            entity.HasIndex(e => e.DomainName)
                  .HasDatabaseName("IX_Domains_DomainName");
            entity.HasIndex(e => e.ProjectId)
                  .HasDatabaseName("IX_Domains_ProjectId");
            entity.HasIndex(e => e.UpdatedAt)
                  .HasDatabaseName("IX_Domains_UpdatedAt");
            entity.HasIndex(e => e.IsDeleted)
                  .HasDatabaseName("IX_Domains_IsDeleted");

            // 論理削除フィルター
            entity.HasQueryFilter(e => !e.IsDeleted);
        });

        // DomainApprover エンティティ設定
        modelBuilder.Entity<DomainApprover>(entity =>
        {
            // テーブルコメント設定
            entity.ToTable("DomainApprovers", t => t.HasComment("ドメイン別承認権限の管理、承認者とドメインの多対多関連"));
            
            entity.Property(e => e.DomainApproverId)
                  .HasComment("ドメイン承認者ID（主キー）");

            entity.Property(e => e.DomainId)
                  .HasComment("ドメインID（外部キー）");

            entity.Property(e => e.ApproverId)
                  .HasComment("承認者ユーザーID（外部キー）");

            entity.Property(e => e.UpdatedBy)
                  .IsRequired()
                  .HasMaxLength(450)
                  .HasComment("最終更新者ユーザーID");

            entity.Property(e => e.UpdatedAt)
                  .HasColumnType("timestamptz")
                  .HasComment("最終更新日時（タイムゾーン付き）");

            // 外部キー関係
            entity.HasOne(e => e.Domain)
                  .WithMany(e => e.DomainApprovers)
                  .HasForeignKey(e => e.DomainId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Approver)
                  .WithMany(e => e.DomainApprovers)
                  .HasForeignKey(e => e.ApproverId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.UpdatedByUser)
                  .WithMany()
                  .HasForeignKey(e => e.UpdatedBy)
                  .OnDelete(DeleteBehavior.Restrict);

            // ユニーク制約
            entity.HasIndex(e => new { e.DomainId, e.ApproverId })
                  .IsUnique()
                  .HasDatabaseName("IX_DomainApprovers_DomainId_ApproverId_Unique");
        });
    }

    /// <summary>
    /// ユビキタス言語管理エンティティの詳細設定
    /// </summary>
    /// <param name="modelBuilder">モデルビルダー</param>
    private void ConfigureUbiquitousLanguageEntities(ModelBuilder modelBuilder)
    {
        // FormalUbiquitousLang エンティティ設定
        modelBuilder.Entity<FormalUbiquitousLang>(entity =>
        {
            // テーブルコメント設定
            entity.ToTable("FormalUbiquitousLang", t => t.HasComment("承認済み正式ユビキタス言語の管理、Claude Code出力対象データ"));
            
            entity.Property(e => e.FormalUbiquitousLangId)
                  .HasComment("正式ユビキタス言語ID（主キー）");

            entity.Property(e => e.DomainId)
                  .HasComment("所属ドメインID（外部キー）");

            entity.Property(e => e.JapaneseName)
                  .IsRequired()
                  .HasMaxLength(30)
                  .HasComment("和名（ドメイン内一意）");

            entity.Property(e => e.EnglishName)
                  .IsRequired()
                  .HasMaxLength(50)
                  .HasComment("英名");

            entity.Property(e => e.Description)
                  .IsRequired()
                  .HasComment("意味・説明（改行可能）");

            entity.Property(e => e.OccurrenceContext)
                  .HasMaxLength(50)
                  .HasComment("発生機会");

            entity.Property(e => e.Remarks)
                  .HasComment("備考（改行可能）");

            entity.Property(e => e.UpdatedBy)
                  .IsRequired()
                  .HasMaxLength(450)
                  .HasComment("最終更新者ユーザーID");

            entity.Property(e => e.UpdatedAt)
                  .HasColumnType("timestamptz")
                  .HasComment("最終更新日時（タイムゾーン付き）");

            entity.Property(e => e.IsDeleted)
                  .HasDefaultValue(false)
                  .HasComment("論理削除フラグ（false:有効、true:削除済み）");

            // 外部キー関係
            entity.HasOne(e => e.Domain)
                  .WithMany(e => e.FormalUbiquitousLangs)
                  .HasForeignKey(e => e.DomainId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.UpdatedByUser)
                  .WithMany()
                  .HasForeignKey(e => e.UpdatedBy)
                  .OnDelete(DeleteBehavior.Restrict);

            // インデックス
            entity.HasIndex(e => e.JapaneseName)
                  .HasDatabaseName("IX_FormalUbiquitousLang_JapaneseName");
            entity.HasIndex(e => e.EnglishName)
                  .HasDatabaseName("IX_FormalUbiquitousLang_EnglishName");
            entity.HasIndex(e => e.DomainId)
                  .HasDatabaseName("IX_FormalUbiquitousLang_DomainId");
            entity.HasIndex(e => e.UpdatedAt)
                  .HasDatabaseName("IX_FormalUbiquitousLang_UpdatedAt");
            entity.HasIndex(e => e.IsDeleted)
                  .HasDatabaseName("IX_FormalUbiquitousLang_IsDeleted");

            // 論理削除フィルター
            entity.HasQueryFilter(e => !e.IsDeleted);
        });

        // DraftUbiquitousLang エンティティ設定
        modelBuilder.Entity<DraftUbiquitousLang>(entity =>
        {
            // テーブルコメント設定
            entity.ToTable("DraftUbiquitousLang", t => t.HasComment("編集中・承認申請中のドラフトユビキタス言語管理"));
            
            entity.Property(e => e.DraftUbiquitousLangId)
                  .HasComment("ドラフトユビキタス言語ID（主キー）");

            entity.Property(e => e.DomainId)
                  .HasComment("所属ドメインID（外部キー）");

            entity.Property(e => e.JapaneseName)
                  .IsRequired()
                  .HasMaxLength(30)
                  .HasComment("和名");

            entity.Property(e => e.EnglishName)
                  .HasMaxLength(50)
                  .HasComment("英名");

            entity.Property(e => e.Description)
                  .HasComment("意味・説明（改行可能）");

            entity.Property(e => e.OccurrenceContext)
                  .HasMaxLength(50)
                  .HasComment("発生機会");

            entity.Property(e => e.Remarks)
                  .HasComment("備考（改行可能）");

            entity.Property(e => e.Status)
                  .IsRequired()
                  .HasMaxLength(20)
                  .HasDefaultValue("Draft")
                  .HasComment("ステータス（Draft/PendingApproval）");

            entity.Property(e => e.ApplicantId)
                  .HasMaxLength(450)
                  .HasComment("申請者ユーザーID");

            entity.Property(e => e.ApplicationDate)
                  .HasColumnType("timestamptz")
                  .HasComment("申請日時");

            entity.Property(e => e.RejectionReason)
                  .HasComment("却下理由");

            entity.Property(e => e.SourceFormalUbiquitousLangId)
                  .HasComment("編集元正式ユビキタス言語ID");

            entity.Property(e => e.UpdatedBy)
                  .IsRequired()
                  .HasMaxLength(450)
                  .HasComment("最終更新者ユーザーID");

            entity.Property(e => e.UpdatedAt)
                  .HasColumnType("timestamptz")
                  .HasComment("最終更新日時（タイムゾーン付き）");

            // 外部キー関係
            entity.HasOne(e => e.Domain)
                  .WithMany(e => e.DraftUbiquitousLangs)
                  .HasForeignKey(e => e.DomainId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Applicant)
                  .WithMany()
                  .HasForeignKey(e => e.ApplicantId)
                  .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(e => e.UpdatedByUser)
                  .WithMany()
                  .HasForeignKey(e => e.UpdatedBy)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.SourceFormalUbiquitousLang)
                  .WithMany()
                  .HasForeignKey(e => e.SourceFormalUbiquitousLangId)
                  .OnDelete(DeleteBehavior.SetNull);

            // インデックス
            entity.HasIndex(e => e.JapaneseName)
                  .HasDatabaseName("IX_DraftUbiquitousLang_JapaneseName");
            entity.HasIndex(e => e.Status)
                  .HasDatabaseName("IX_DraftUbiquitousLang_Status");
            entity.HasIndex(e => e.DomainId)
                  .HasDatabaseName("IX_DraftUbiquitousLang_DomainId");
            entity.HasIndex(e => e.ApplicantId)
                  .HasDatabaseName("IX_DraftUbiquitousLang_ApplicantId");
            entity.HasIndex(e => e.UpdatedAt)
                  .HasDatabaseName("IX_DraftUbiquitousLang_UpdatedAt");
        });

        // RelatedUbiquitousLang エンティティ設定
        modelBuilder.Entity<RelatedUbiquitousLang>(entity =>
        {
            // テーブルコメント設定
            entity.ToTable("RelatedUbiquitousLang", t => t.HasComment("ユビキタス言語間の関連性管理、多対多関連"));
            
            entity.Property(e => e.RelatedUbiquitousLangId)
                  .HasComment("関連ユビキタス言語ID（主キー）");

            entity.Property(e => e.SourceUbiquitousLangId)
                  .HasComment("関連元ユビキタス言語ID");

            entity.Property(e => e.TargetUbiquitousLangId)
                  .HasComment("関連先ユビキタス言語ID");

            entity.Property(e => e.UpdatedBy)
                  .IsRequired()
                  .HasMaxLength(450)
                  .HasComment("最終更新者ユーザーID");

            entity.Property(e => e.UpdatedAt)
                  .HasColumnType("timestamptz")
                  .HasComment("最終更新日時（タイムゾーン付き）");

            // 外部キー関係
            entity.HasOne(e => e.SourceUbiquitousLang)
                  .WithMany(e => e.SourceRelations)
                  .HasForeignKey(e => e.SourceUbiquitousLangId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.TargetUbiquitousLang)
                  .WithMany(e => e.TargetRelations)
                  .HasForeignKey(e => e.TargetUbiquitousLangId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.UpdatedByUser)
                  .WithMany()
                  .HasForeignKey(e => e.UpdatedBy)
                  .OnDelete(DeleteBehavior.Restrict);

            // ユニーク制約
            entity.HasIndex(e => new { e.SourceUbiquitousLangId, e.TargetUbiquitousLangId })
                  .IsUnique()
                  .HasDatabaseName("IX_RelatedUbiquitousLang_Source_Target_Unique");
        });

        // DraftUbiquitousLangRelation エンティティ設定
        modelBuilder.Entity<DraftUbiquitousLangRelation>(entity =>
        {
            // テーブルコメント設定
            entity.ToTable("DraftUbiquitousLangRelations", t => t.HasComment("ドラフトユビキタス言語と正式ユビキタス言語間の関連性管理"));
            
            entity.Property(e => e.DraftUbiquitousLangRelationId)
                  .HasComment("ドラフト関連ID（主キー）");

            entity.Property(e => e.DraftUbiquitousLangId)
                  .HasComment("ドラフトユビキタス言語ID");

            entity.Property(e => e.FormalUbiquitousLangId)
                  .HasComment("関連正式ユビキタス言語ID");

            entity.Property(e => e.UpdatedBy)
                  .IsRequired()
                  .HasMaxLength(450)
                  .HasComment("最終更新者ユーザーID");

            entity.Property(e => e.UpdatedAt)
                  .HasColumnType("timestamptz")
                  .HasComment("最終更新日時（タイムゾーン付き）");

            // 外部キー関係
            entity.HasOne(e => e.DraftUbiquitousLang)
                  .WithMany(e => e.DraftRelations)
                  .HasForeignKey(e => e.DraftUbiquitousLangId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.FormalUbiquitousLang)
                  .WithMany(e => e.DraftRelations)
                  .HasForeignKey(e => e.FormalUbiquitousLangId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.UpdatedByUser)
                  .WithMany()
                  .HasForeignKey(e => e.UpdatedBy)
                  .OnDelete(DeleteBehavior.Restrict);

            // ユニーク制約
            entity.HasIndex(e => new { e.DraftUbiquitousLangId, e.FormalUbiquitousLangId })
                  .IsUnique()
                  .HasDatabaseName("IX_DraftUbiquitousLangRelation_Draft_Formal_Unique");
        });

        // FormalUbiquitousLangHistory エンティティ設定
        modelBuilder.Entity<FormalUbiquitousLangHistory>(entity =>
        {
            // テーブルコメント設定
            entity.ToTable("FormalUbiquitousLangHistory", t => t.HasComment("正式ユビキタス言語の変更履歴管理、JSONB活用でスナップショット保存"));
            
            entity.Property(e => e.HistoryId)
                  .HasComment("履歴ID（主キー）");

            entity.Property(e => e.FormalUbiquitousLangId)
                  .HasComment("元の正式ユビキタス言語ID");

            entity.Property(e => e.DomainId)
                  .HasComment("所属ドメインID（外部キー）");

            entity.Property(e => e.JapaneseName)
                  .IsRequired()
                  .HasMaxLength(30)
                  .HasComment("和名");

            entity.Property(e => e.EnglishName)
                  .IsRequired()
                  .HasMaxLength(50)
                  .HasComment("英名");

            entity.Property(e => e.Description)
                  .IsRequired()
                  .HasComment("意味・説明（改行可能）");

            entity.Property(e => e.OccurrenceContext)
                  .HasMaxLength(50)
                  .HasComment("発生機会");

            entity.Property(e => e.Remarks)
                  .HasComment("備考（改行可能）");

            entity.Property(e => e.RelatedUbiquitousLangSnapshot)
                  .HasColumnType("jsonb")
                  .HasComment("関連ユビキタス言語スナップショット（JSONB、GINインデックス対応）");

            entity.Property(e => e.UpdatedBy)
                  .IsRequired()
                  .HasMaxLength(450)
                  .HasComment("最終更新者ユーザーID");

            entity.Property(e => e.UpdatedAt)
                  .HasColumnType("timestamptz")
                  .HasComment("最終更新日時（タイムゾーン付き）");

            entity.Property(e => e.IsDeleted)
                  .HasDefaultValue(false)
                  .HasComment("論理削除フラグ（false:有効、true:削除済み）");

            // 外部キー関係
            entity.HasOne(e => e.Domain)
                  .WithMany()
                  .HasForeignKey(e => e.DomainId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.UpdatedByUser)
                  .WithMany()
                  .HasForeignKey(e => e.UpdatedBy)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.FormalUbiquitousLang)
                  .WithMany(e => e.Histories)
                  .HasForeignKey(e => e.FormalUbiquitousLangId)
                  .OnDelete(DeleteBehavior.Cascade);

            // インデックス
            entity.HasIndex(e => e.FormalUbiquitousLangId)
                  .HasDatabaseName("IX_FormalUbiquitousLangHistory_FormalUbiquitousLangId");
            entity.HasIndex(e => e.DomainId)
                  .HasDatabaseName("IX_FormalUbiquitousLangHistory_DomainId");
            entity.HasIndex(e => e.UpdatedAt)
                  .HasDatabaseName("IX_FormalUbiquitousLangHistory_UpdatedAt");
            entity.HasIndex(e => e.IsDeleted)
                  .HasDatabaseName("IX_FormalUbiquitousLangHistory_IsDeleted");

            // 論理削除フィルター
            entity.HasQueryFilter(e => !e.IsDeleted);
        });
    }

}