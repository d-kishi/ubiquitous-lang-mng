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

    // 🔧 今後追加予定のテーブル:
    // public DbSet<ProjectEntity> Projects { get; set; } = null!;
    // public DbSet<DomainEntity> Domains { get; set; } = null!;
    // public DbSet<UbiquitousLanguageEntity> UbiquitousLanguages { get; set; } = null!;

    /// <summary>
    /// モデル設定: Entity Framework の詳細設定
    /// テーブル構造、制約、インデックスの定義
    /// </summary>
    /// <param name="modelBuilder">モデルビルダー</param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // 🔐 ASP.NET Core Identity のテーブル設定
        base.OnModelCreating(modelBuilder);

        // 🔐 ApplicationUser（Identity統合ユーザー）の詳細設定
        ConfigureApplicationUser(modelBuilder);

        // 👤 ApplicationUser の詳細設定（Identity基底で自動設定）
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
            // 📧 業務固有プロパティの設定
            entity.Property(e => e.Name)
                  .IsRequired()
                  .HasMaxLength(50)
                  .HasComment("ユーザー氏名（表示名）");

            entity.Property(e => e.UserRole)
                  .IsRequired()
                  .HasMaxLength(20)
                  .HasDefaultValue("GeneralUser")
                  .HasComment("ユーザーロール（SuperUser/ProjectManager/DomainApprover/GeneralUser）");

            entity.Property(e => e.IsFirstLogin)
                  .HasDefaultValue(true)
                  .HasComment("初回ログインフラグ（パスワード変更必須状態）");

            entity.Property(e => e.UpdatedAt)
                  .HasColumnType("timestamptz")
                  .HasDefaultValueSql("CURRENT_TIMESTAMP")
                  .HasComment("最終更新日時（UTC、PostgreSQL の TIMESTAMPTZ）");

            entity.Property(e => e.IsDeleted)
                  .HasDefaultValue(false)
                  .HasComment("論理削除フラグ");

            entity.Property(e => e.InitialPassword)
                  .HasMaxLength(100)
                  .HasComment("初期パスワード（一時的保存用、初回ログイン後NULL化）");

            entity.Property(e => e.DomainUserId)
                  .HasComment("F# Domain層のUser.Idとの連携用");

            // 🔍 インデックス設定（検索性能向上）
            entity.HasIndex(e => e.UserRole)
                  .HasDatabaseName("IX_ApplicationUsers_UserRole");

            entity.HasIndex(e => e.IsDeleted)
                  .HasDatabaseName("IX_ApplicationUsers_IsDeleted");

            entity.HasIndex(e => e.UpdatedAt)
                  .HasDatabaseName("IX_ApplicationUsers_UpdatedAt");

            entity.HasIndex(e => e.DomainUserId)
                  .HasDatabaseName("IX_ApplicationUsers_DomainUserId");

            // 🔐 論理削除されたユーザーを除外するグローバルフィルター
            entity.HasQueryFilter(e => !e.IsDeleted);
        });
    }

}