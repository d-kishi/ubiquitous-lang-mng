using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace UbiquitousLanguageManager.Infrastructure.Data;

/// <summary>
/// EF Core Migration作成時に使用されるDbContextファクトリ
/// 
/// 【F#初学者向け解説】
/// このクラスは、dotnet ef コマンドでMigrationを作成する際に必要です。
/// アプリケーション実行時ではなく、開発時のツールとして使用されます。
/// </summary>
public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<UbiquitousLanguageDbContext>
{
    /// <summary>
    /// Migration作成時にDbContextインスタンスを生成
    /// </summary>
    /// <param name="args">コマンドライン引数（通常は使用しない）</param>
    /// <returns>設定済みのDbContextインスタンス</returns>
    public UbiquitousLanguageDbContext CreateDbContext(string[] args)
    {
        // 設定ファイルの読み込み
        // Web プロジェクトの appsettings.json を参照
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Path.Combine(Directory.GetCurrentDirectory(), "../UbiquitousLanguageManager.Web"))
            .AddJsonFile("appsettings.json", optional: false)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .Build();

        var optionsBuilder = new DbContextOptionsBuilder<UbiquitousLanguageDbContext>();
        
        // PostgreSQL接続文字列の設定
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        optionsBuilder.UseNpgsql(connectionString);

        return new UbiquitousLanguageDbContext(optionsBuilder.Options);
    }
}