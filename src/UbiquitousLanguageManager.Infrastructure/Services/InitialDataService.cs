using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using UbiquitousLanguageManager.Infrastructure.Data;
using UbiquitousLanguageManager.Infrastructure.Data.Entities;

namespace UbiquitousLanguageManager.Infrastructure.Services;

/// <summary>
/// 初期データ投入サービス
/// appsettings.jsonから初期スーパーユーザー設定を読み込み、
/// 機能仕様書で定義された"su"パスワードによる初期ユーザーを作成
/// 
/// 【ASP.NET Core Identity統合】
/// UserManager を使用して、Identity 統合されたユーザー作成を行います。
/// これにより、認証・承認機能が自動的に利用可能になります。
/// </summary>
public class InitialDataService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly ILogger<InitialDataService> _logger;
    private readonly InitialSuperUserSettings _settings;

    /// <summary>
    /// コンストラクタ: 依存関係の注入
    /// </summary>
    /// <param name="userManager">ASP.NET Core Identity のユーザー管理</param>
    /// <param name="roleManager">ASP.NET Core Identity のロール管理</param>
    /// <param name="logger">ログ出力</param>
    /// <param name="settings">初期スーパーユーザー設定（appsettings.jsonから読み込み）</param>
    public InitialDataService(
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager,
        ILogger<InitialDataService> logger,
        IOptions<InitialSuperUserSettings> settings)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _logger = logger;
        _settings = settings.Value;
    }

    /// <summary>
    /// 初期データの投入処理
    /// システム初期化時に自動実行される
    /// </summary>
    public virtual async Task SeedInitialDataAsync()
    {
        try
        {
            // 🎭 ロールの作成
            await CreateRolesAsync();

            // 🔍 既存のスーパーユーザーの存在確認
            var existingSuperUser = await _userManager.FindByEmailAsync(_settings.Email);

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
    /// システムロールの作成
    /// SuperUser, ProjectManager, DomainApprover, GeneralUser の4種類
    /// </summary>
    private async Task CreateRolesAsync()
    {
        var roles = new[] { "SuperUser", "ProjectManager", "DomainApprover", "GeneralUser" };

        foreach (var roleName in roles)
        {
            if (!await _roleManager.RoleExistsAsync(roleName))
            {
                await _roleManager.CreateAsync(new IdentityRole(roleName));
                _logger.LogInformation("🎭 ロールを作成しました: {RoleName}", roleName);
            }
        }
    }

    /// <summary>
    /// 初期スーパーユーザーの作成処理
    /// 機能仕様書の仕様に従い、設定ファイルから情報を読み込んで作成
    /// 
    /// 【F#初学者向け解説】
    /// ASP.NET Core Identity の UserManager を使用することで、
    /// パスワードハッシュ化、検証、セキュリティスタンプ生成等が自動的に行われます。
    /// </summary>
    private async Task CreateInitialSuperUserAsync()
    {
        // 🔧 設定値のデバッグログ
        _logger.LogInformation("設定値確認: Email={Email}, Name={Name}, Password設定有無={HasPassword}", 
            _settings?.Email ?? "null", 
            _settings?.Name ?? "null", 
            !string.IsNullOrWhiteSpace(_settings?.Password));

        // 🔧 設定値の検証
        if (_settings == null || string.IsNullOrWhiteSpace(_settings.Email))
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

        // 👤 ApplicationUser（カスタムプロパティ対応）の作成
        var superUser = new ApplicationUser
        {
            UserName = _settings.Email,  // Identity ではメールアドレスをユーザー名として使用
            Email = _settings.Email,
            EmailConfirmed = true,  // 初期スーパーユーザーは確認済み
            LockoutEnabled = false,  // スーパーユーザーはロックアウトしない
            Name = _settings.Name,  // カスタムプロパティ：ユーザー氏名
            IsFirstLogin = _settings.IsFirstLogin,  // カスタムプロパティ：初回ログインフラグ
            UpdatedAt = DateTime.UtcNow,  // カスタムプロパティ：更新日時
            UpdatedBy = "System",  // カスタムプロパティ：更新者（システム初期化）
            IsDeleted = false  // カスタムプロパティ：削除フラグ
        };

        // 💾 UserManager を使用したユーザー作成
        // パスワードハッシュ化は自動的に行われる
        var result = await _userManager.CreateAsync(superUser, _settings.Password);

        if (result.Succeeded)
        {
            // 🎭 SuperUser ロールの割り当て
            await _userManager.AddToRoleAsync(superUser, "SuperUser");

            _logger.LogInformation("👤 初期スーパーユーザーを作成しました: {Email}", _settings.Email);
            _logger.LogInformation("🔑 初期パスワード: {Password}", _settings.Password);
            _logger.LogWarning("⚠️ セキュリティ注意: 初回ログイン後、必ずパスワードを変更してください");
        }
        else
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            throw new InvalidOperationException($"ユーザー作成に失敗しました: {errors}");
        }
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