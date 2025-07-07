using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using UbiquitousLanguageManager.Infrastructure.Data.Entities;

namespace UbiquitousLanguageManager.Infrastructure.Data;

/// <summary>
/// ユビキタス言語管理システムのデータベースコンテキスト
/// Entity Framework Core + PostgreSQL の設定
/// ASP.NET Core Identity との統合
/// </summary>
public class UbiquitousLanguageDbContext : IdentityDbContext<IdentityUser>
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
    /// システム独自のユーザー管理（ASP.NET Core Identity とは別管理）
    /// </summary>
    public DbSet<UserEntity> Users { get; set; } = null!;

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

        // 👤 ユーザーテーブルの詳細設定
        ConfigureUserEntity(modelBuilder);
    }

    /// <summary>
    /// ユーザーエンティティの詳細設定
    /// PostgreSQL 固有の最適化と制約設定
    /// </summary>
    /// <param name="modelBuilder">モデルビルダー</param>
    private void ConfigureUserEntity(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<UserEntity>(entity =>
        {
            // 📧 メールアドレスにユニーク制約
            entity.HasIndex(e => e.Email)
                  .IsUnique()
                  .HasDatabaseName("IX_Users_Email_Unique");

            // 🎭 ユーザーロールにインデックス（検索性能向上）
            entity.HasIndex(e => e.UserRole)
                  .HasDatabaseName("IX_Users_UserRole");

            // 🔍 アクティブフラグにインデックス（論理削除対応）
            entity.HasIndex(e => e.IsActive)
                  .HasDatabaseName("IX_Users_IsActive");

            // 📊 更新日時にインデックス（監査ログ対応）
            entity.HasIndex(e => e.UpdatedAt)
                  .HasDatabaseName("IX_Users_UpdatedAt");

            // 🎯 PostgreSQL 固有設定
            entity.Property(e => e.Id)
                  .HasComment("ユーザーID（主キー、自動採番）");

            entity.Property(e => e.Email)
                  .HasComment("メールアドレス（ログインID、ユニーク制約）");

            entity.Property(e => e.PasswordHash)
                  .HasComment("BCryptでハッシュ化されたパスワード");

            entity.Property(e => e.Name)
                  .HasComment("ユーザー名（表示名）");

            entity.Property(e => e.UserRole)
                  .HasComment("ユーザーロール（SuperUser/ProjectManager/DomainApprover/GeneralUser）");

            entity.Property(e => e.IsActive)
                  .HasComment("アクティブ状態フラグ（論理削除用）")
                  .HasDefaultValue(true);

            entity.Property(e => e.IsFirstLogin)
                  .HasComment("初回ログインフラグ（パスワード変更必須状態）")
                  .HasDefaultValue(true);

            entity.Property(e => e.UpdatedAt)
                  .HasComment("最終更新日時（UTC、PostgreSQL の TIMESTAMPTZ）")
                  .HasColumnType("timestamptz")
                  .HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.Property(e => e.UpdatedBy)
                  .HasComment("最終更新者ID（循環参照回避のため外部キー制約なし）");
        });
    }
}