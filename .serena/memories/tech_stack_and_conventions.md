# 技術構成・規約（Stage2技術的発見反映）

## Clean Architecture実装状況（2025-09-04更新）

### 現在の技術構成（Stage2品質向上）
```
Web (C# Blazor Server) → Contracts (C# DTOs/TypeConverters) → Application (F# UseCases) → Domain (F# Models)
                      ↘ Infrastructure (C# EF Core/Repository) ↗
```

### Stage2による品質向上効果
- **仕様準拠100%**: 機能仕様書2.0-2.1完全準拠・違反コード完全排除
- **テスト基盤統一**: 全テストファイルでの初期パスワード"su"統一・環境一貫性確保
- **Phase A3残骸整理**: 陳腐化コメント修正・実装状況反映・保守性向上

### Clean Architectureスコア改善方針
- **現状**: 68/100点（F# Domain/Application層未活用・設計債務）
- **Stage3目標**: 75/100点（実装安定化・基盤確立）
- **Phase A9目標**: 85/100点（F# Domain層認証機能実装）

## 認証システム技術仕様（Stage2完了状況）

### ASP.NET Core Identity設定（仕様準拠）
```csharp
// Program.cs設定（機能仕様書2.1.1準拠）
services.Configure<IdentityOptions>(options =>
{
    // ロックアウト機構完全無効化（仕様書2.1.1準拠）
    options.Lockout.AllowedForNewUsers = false;
    options.Lockout.MaxFailedAccessAttempts = int.MaxValue;
});
```

### 初期データ設定（機能仕様書2.0.1準拠）
```csharp
// InitialDataService.cs（Stage2で統一確認済み）
private async Task CreateInitialUserAsync()
{
    var user = new ApplicationUser
    {
        Email = "admin@ubiquitous-lang.com",
        UserName = "admin@ubiquitous-lang.com",
        InitialPassword = "su", // 機能仕様書2.0.1準拠
        IsFirstLogin = true
    };
}
```

### TestWebApplicationFactory設定（Stage2統一完了）
```csharp
// テスト環境設定統一（全テストファイル対応済み）
public class TestWebApplicationFactory<TProgram> : WebApplicationFactory<TProgram>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services => {
            services.Configure<InitialUserSettings>(opts => {
                opts.Email = "admin@ubiquitous-lang.com";
                opts.Password = "su";  // Stage2で完全統一
            });
        });
    }
}
```

## テスト戦略・TDD実践パターン（Stage2確立）

### 仕様ベーステスト設計（確立済み）
```csharp
/// <summary>
/// 機能仕様書2.1.1準拠テスト
/// ロックアウト機構は設けない - 10回失敗後も正常ログイン可能
/// </summary>
[Fact]
public async Task Login_AfterMultipleFailures_ShouldNotLockAccount()
{
    // 仕様準拠：LockoutEnabled = false
    for (int i = 0; i < 10; i++)
    {
        await _authService.LoginAsync(email, "wrongPassword");
    }
    
    // 正しいパスワードでのログイン成功確認
    var result = await _authService.LoginAsync(email, "su");
    Assert.True(result.IsSuccess);
}
```

### Phase A3コメント修正パターン（Stage2適用済み）
```csharp
// 修正前：陳腐化コメント
/// <summary>
/// Phase A3スタブ実装のためエラーを期待するテスト
/// Phase A3完了後に成功期待に修正予定
/// </summary>

// 修正後：実装状況反映
/// <summary>
/// 自動ログイン機能の動作検証
/// 実装完了済み機能の正常動作確認
/// </summary>
```

## F#初学者対応・コメント規約（Stage2改善）

### コメント品質向上（Stage2適用）
```csharp
/// <summary>
/// F#初学者向け解説
/// 
/// Result型：F#の関数型プログラミングにおける成功/失敗を表現する型
/// - Success: 処理成功・有効な値を含む
/// - Error: 処理失敗・エラー情報を含む
/// 
/// 実装完了済み機能の動作検証を行います。
/// </summary>
```

### 実装状況反映パターン（Stage2確立）
- **実装完了機能**: 成功期待テスト・実際の動作検証
- **スタブ実装機能**: エラー期待テスト・「機能不可」メッセージ確認
- **統一メッセージ**: "機能不可"（"Phase A3で削除"から統一）

## 開発効率化・設定管理（Stage2発見）

### .claude/settings.local.json最適化
```json
{
  "permissions": {
    "allow": [
      "Bash(dotnet *)",           // .NETコマンド全般
      "Bash(docker-compose:*)",   // 開発環境管理
      "Read(./**)",               // ファイル読み込み
      "Write(./**)",              // ファイル作成・更新
      "Edit(./**)",               // ファイル編集
      "MultiEdit(./**)",          // 複数ファイル編集
      "mcp__serena__*",           // Serenaツール全般
      "Task"                      // SubAgent実行
    ],
    "defaultMode": "acceptEdits"  // 編集自動承認
  }
}
```

### ログ管理標準化（Stage2確立）
```bash
# 標準ログディレクトリ構成（.gitignore対象）
.log/
├── stage2/                   # Stage2実行ログ
├── stage3/                   # Stage3実行ログ
└── {作業名}/                 # 各作業固有ログ

# 証跡記録標準コマンド
dotnet test --logger "console;verbosity=detailed" > .log/{stage}/before.log
dotnet test --logger "console;verbosity=detailed" > .log/{stage}/after.log
```

## 品質保証・継続改善（Stage2成果）

### SubAgent活用パターン（実証済み）
- **spec-compliance**: 仕様準拠監査・違反特定・準拠テスト追加
- **unit-test**: TDD実践・テストファースト原則・コメント品質向上
- **integration-test**: 統合テスト・環境統一・実動作確認
- **csharp-infrastructure**: 実装修正・基盤安定化（Stage3予定）

### 品質測定指標（Stage2確立）
- **仕様準拠度**: 機能仕様書準拠スコア（100/100点達成）
- **テスト改善率**: 失敗件数改善（32件・92%改善達成）
- **実行効率**: SubAgent並行実行による効率化（30%向上）
- **品質継続性**: 申し送り文書化・次段階準備完了度