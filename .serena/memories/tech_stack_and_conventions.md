# 技術スタック・規約・実装パターン

## 認証システム技術詳細（2025-01-03更新）

### ASP.NET Core Identity実装状況
- **統合レベル**: 良好（標準パターン準拠）
- **UserManager/SignInManager**: 適切な設定・実装済み
- **Cookie認証**: Remember Me機能実装済み
- **問題点**: F# Domain層未活用・C#のみ実装

### 初期データ設定パターン
```csharp
// InitialDataService.cs実装パターン
var user = new ApplicationUser
{
    Email = "admin@ubiquitous-lang.com",
    UserName = "admin@ubiquitous-lang.com",
    InitialPassword = "su",  // 機能仕様書2.0.1準拠
    IsFirstLogin = true
};
```

### TestWebApplicationFactory設定パターン
```csharp
// テスト環境設定標準パターン
public class CustomWebApplicationFactory<TProgram> : WebApplicationFactory<TProgram> 
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services => {
            services.Configure<InitialUserSettings>(opts => {
                opts.Email = "admin@ubiquitous-lang.com";
                opts.Password = "su";  // 仕様書準拠必須
            });
            services.AddDbContext<UbiquitousLanguageDbContext>(options =>
                options.UseInMemoryDatabase("TestDb"));
        });
    }
}
```

## Clean Architecture実装状況

### 現状評価（68/100点）
- **Domain層（F#）**: 12/25点（User定義あるが未活用）
- **Application層（F#）**: 10/20点（IAuthenticationService未実装）
- **Contracts層（C#）**: 18/15点（TypeConverter優秀・150%達成）
- **Infrastructure層（C#）**: 15/20点（ASP.NET Core Identity統合良好）
- **Web層（C#）**: 13/20点（AuthApiController複雑化）

### F#型システム活用パターン（未実装・Phase A9対応）
```fsharp
// Domain/Entities.fs - 定義済みだが未使用
type User = {
    Id: Guid
    Email: Email
    PasswordHash: string option
    Role: UserRole
}

type Email = private Email of string
type Password = private Password of string
```

### 認証処理重複問題
1. **AuthenticationService.cs**: Blazor Server認証処理
2. **AuthApiController.cs**: API認証処理
→ **Phase A9対応**: 単一認証基盤への統一化

## テスト実装パターン・規約

### 仕様準拠テストパターン
```csharp
[Fact]
public async Task MultipleLoginFailures_ShouldNotLockAccount()
{
    // 機能仕様書2.1.1「ロックアウト機構は設けない」準拠確認
    // 10回失敗後も正しいパスワードでログイン可能確認
}

[Fact]
public async Task InitialLogin_WithSuPassword_ShouldSucceed()
{
    // 機能仕様書2.0.1「初期パスワード:"su"」準拠確認
    var loginRequest = new LoginRequest 
    { 
        Email = "admin@ubiquitous-lang.com", 
        Password = "su" 
    };
    var result = await _authService.LoginAsync(loginRequest);
    Assert.True(result.IsSuccess);
}
```

### テスト命名・実装規約
- **Phase A3コメント禁止**: 「Phase A3スタブ実装のため〜」等の削除必須
- **成功期待テスト**: Assert.True(result.IsSuccess) - 実装成功前提
- **初期パスワード統一**: "su"使用・"TempPass123!"使用禁止

## 開発環境・ツール設定

### ログ管理標準化
```bash
# 作業ディレクトリクリーン化
mkdir -p .log/{作業名}
echo ".log/" >> .gitignore

# テスト実行結果保存パターン
dotnet test --logger "console;verbosity=detailed" > .log/{作業名}/{段階}.log
```

### 品質測定コマンド
```bash
# ビルド品質確認
dotnet build --verbosity normal > .log/{作業名}/build.log
grep -i "warning\|error" .log/{作業名}/build.log | wc -l

# テストカバレッジ測定
dotnet test --collect:"XPlat Code Coverage" > .log/{作業名}/coverage.log
```

## TECH負債・Issue管理パターン

### GitHub Issue作成パターン（Issue #21参考）
- **タイトル形式**: [TECH-XXX] {課題概要}（Phase XX: {詳細}）
- **工数見積もり**: 分単位での詳細工数・複数セッション分割計画
- **優先度設定**: Phase完了前対応・Phase間移行前解決

### 技術負債分類
- **TECH-XXX**: 技術的実装問題
- **ARCH-XXX**: アーキテクチャ設計問題
- **SPEC-XXX**: 仕様準拠問題