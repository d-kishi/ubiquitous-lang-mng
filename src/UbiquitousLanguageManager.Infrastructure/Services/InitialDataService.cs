using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using UbiquitousLanguageManager.Infrastructure.Data;

namespace UbiquitousLanguageManager.Infrastructure.Services;

/// <summary>
/// 初期データ投入サービス
/// appsettings.jsonから初期スーパーユーザー設定を読み込み、
/// 機能仕様書で定義された"su"パスワードによる初期ユーザーを作成
/// </summary>
public class InitialDataService
{
    private readonly UbiquitousLanguageDbContext _context;
    private readonly ILogger<InitialDataService> _logger;
    private readonly InitialSuperUserSettings _settings;

    /// <summary>
    /// コンストラクタ: 依存関係の注入
    /// </summary>
    /// <param name="context">データベースコンテキスト</param>
    /// <param name="logger">ログ出力</param>
    /// <param name="settings">初期スーパーユーザー設定（appsettings.jsonから読み込み）</param>
    public InitialDataService(
        UbiquitousLanguageDbContext context,
        ILogger<InitialDataService> logger,
        IOptions<InitialSuperUserSettings> settings)
    {
        _context = context;
        _logger = logger;
        _settings = settings.Value;
    }

    /// <summary>
    /// 初期データの投入処理
    /// システム初期化時に自動実行される
    /// </summary>
    public async Task SeedInitialDataAsync()
    {
        try
        {
            // 🔍 既存のスーパーユーザーの存在確認
            var existingSuperUser = _context.Users
                .FirstOrDefault(u => u.Email == _settings.Email);

            if (existingSuperUser != null)
            {
                _logger.LogInformation("✅ 初期スーパーユーザーは既に存在します: {Email}", _settings.Email);
                return;
            }

            // 👤 初期スーパーユーザーの作成
            await CreateInitialSuperUserAsync();

            _logger.LogInformation("✅ 初期データ投入が正常に完了しました");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ 初期データ投入中にエラーが発生しました: {Message}", ex.Message);
            throw;
        }
    }

    /// <summary>
    /// 初期スーパーユーザーの作成処理
    /// 機能仕様書の仕様に従い、設定ファイルから情報を読み込んで作成
    /// </summary>
    private async Task CreateInitialSuperUserAsync()
    {
        // 🔧 設定値の検証
        if (string.IsNullOrWhiteSpace(_settings.Email))
        {
            throw new InvalidOperationException("初期スーパーユーザーのメールアドレスが設定されていません");
        }

        if (string.IsNullOrWhiteSpace(_settings.Name))
        {
            throw new InvalidOperationException("初期スーパーユーザーの名前が設定されていません");
        }

        if (string.IsNullOrWhiteSpace(_settings.Password))
        {
            throw new InvalidOperationException("初期スーパーユーザーのパスワードが設定されていません");
        }

        // 🔐 パスワードハッシュ化: BCrypt使用
        var passwordHash = BCrypt.Net.BCrypt.HashPassword(_settings.Password);

        // 👤 スーパーユーザーエンティティの作成
        var superUser = new Data.Entities.UserEntity
        {
            Email = _settings.Email,
            PasswordHash = passwordHash,
            Name = _settings.Name,
            UserRole = "SuperUser",  // 🎖️ スーパーユーザー権限
            IsActive = true,
            IsFirstLogin = _settings.IsFirstLogin,  // 🔑 初回ログイン時のパスワード変更必須
            UpdatedAt = DateTime.UtcNow,
            UpdatedBy = 1  // 🔧 システム作成（自分自身のIDを参照）
        };

        // 💾 データベースへの保存
        _context.Users.Add(superUser);
        await _context.SaveChangesAsync();

        _logger.LogInformation("👤 初期スーパーユーザーを作成しました: {Email}", _settings.Email);
        _logger.LogInformation("🔑 初期パスワード: {Password}", _settings.Password);
        _logger.LogWarning("⚠️ セキュリティ注意: 初回ログイン後、必ずパスワードを変更してください");

        // 🔧 作成したユーザーのIDで UpdatedBy を更新
        superUser.UpdatedBy = superUser.Id;
        await _context.SaveChangesAsync();
    }
}

/// <summary>
/// 初期スーパーユーザー設定クラス
/// appsettings.jsonの"InitialSuperUser"セクションにバインド
/// 機能仕様書の仕様: 初期パスワード"su"、設定ファイル読み込み方式
/// </summary>
public class InitialSuperUserSettings
{
    /// <summary>
    /// 初期スーパーユーザーのメールアドレス
    /// 設定ファイルから読み込み
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// 初期スーパーユーザーの名前
    /// 設定ファイルから読み込み
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 初期パスワード
    /// 機能仕様書の仕様: 固定値"su"
    /// </summary>
    public string Password { get; set; } = string.Empty;

    /// <summary>
    /// 初回ログインフラグ
    /// 機能仕様書の仕様: 初回ログイン時のパスワード変更必須
    /// </summary>
    public bool IsFirstLogin { get; set; } = true;
}