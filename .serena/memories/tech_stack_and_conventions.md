# Tech Stack and Conventions - 技術スタック・規約

## 🏗 アーキテクチャ構成

### Clean Architecture構成
```
Web (C# Blazor Server) → Contracts (C# DTOs/TypeConverters) → Application (F# UseCases) → Domain (F# Models)
                      ↘ Infrastructure (C# EF Core/Repository) ↗
```

### 技術スタック詳細
- **Frontend**: Blazor Server + Bootstrap 5 + SignalR（リアルタイム）
- **Backend**: ASP.NET Core 8.0 + Entity Framework Core 8.0
- **Domain/Application**: F# 8.0（関数型プログラミング・Railway-oriented Programming）
- **Database**: PostgreSQL 16（Docker Container）
- **認証**: ASP.NET Core Identity + Cookie認証
- **開発ツール**: Docker Compose + PgAdmin + Smtp4dev

## 🔐 セキュリティ実装規約（2025-09-11確立）

### 認証情報表示禁止規約
**絶対遵守事項**:
```razor
@* ❌ セキュリティリスク - 絶対禁止 *@
<label>現在のパスワード <small>(初期パスワード: su)</small></label>
<input placeholder="初期パスワード 'su' を入力してください" />

@* ✅ セキュリティ安全 - 推奨実装 *@
<label>現在のパスワード</label>
<input placeholder="現在のパスワードを入力してください" />
```

### エラーメッセージ安全規約
```csharp
// ❌ 認証情報漏洩リスク
if (baseMessage.Contains("初期パスワード")) {
    errorMessage = $"初期パスワード 'su' が正しくありません。";
}

// ✅ セキュリティ安全実装
if (baseMessage.Contains("現在のパスワード") || baseMessage.Contains("初期パスワード")) {
    errorMessage = "パスワード変更に失敗しました: 現在のパスワードを正しく入力してください。";
}
```

### UI設計セキュリティ原則
1. **認証情報非表示**: パスワード・トークン・キー等の画面表示禁止
2. **一般的メッセージ**: 具体的認証情報を含まない汎用メッセージ使用
3. **ヒント制限**: placeholderに認証情報含有禁止

## 🎯 実装規約・パターン

### JsonSerializerService一括管理（2025-09-10確立）
**問題**: ConfigureHttpJsonOptionsはWeb API専用・Blazor Component適用不可
**解決**: JsonSerializerService DI統一管理

```csharp
// Program.cs DI登録
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.PropertyNameCaseInsensitive = true;
    options.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
});
builder.Services.AddScoped<IJsonSerializerService, JsonSerializerService>();

// JsonSerializerService実装
public class JsonSerializerService : IJsonSerializerService
{
    private readonly JsonSerializerOptions _options = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };
    
    public T? Deserialize<T>(string json) => JsonSerializer.Deserialize<T>(json, _options);
    public string Serialize<T>(T value) => JsonSerializer.Serialize(value, _options);
}

// Blazor Component利用
@inject IJsonSerializerService JsonSerializer
var parsedResult = JsonSerializer.Deserialize<PasswordChangeApiResponse>(resultJson);
```

**効果**: DRY原則準拠・技術負債予防・新規Component自動適用・保守性向上

### F# Application層パターン（Railway-oriented Programming）
```fsharp
// AuthenticationError判別共用体
type AuthenticationError = 
  | InvalidCredentials
  | UserNotFound
  | PasswordExpired
  | AccountLocked
  | PasswordRequired
  | TooManyAttempts
  | SystemError

// Result型によるエラーハンドリング
type IAuthenticationService =
  abstract member AuthenticateAsync : email:string -> password:string -> Task<Result<AuthenticationResult, AuthenticationError>>

// 実装例
let authenticateUser email password =
    async {
        match! validateCredentials email password with
        | Ok user -> 
            match user.IsFirstLogin with
            | true -> return Ok { User = user; RequiresPasswordChange = true }
            | false -> return Ok { User = user; RequiresPasswordChange = false }
        | Error InvalidCredentials -> return Error InvalidCredentials
        | Error error -> return Error error
    }
```

### TypeConverter基盤（F#↔C#境界統合）
```csharp
// AuthenticationConverter実装例
public static class AuthenticationConverter
{
    public static AuthenticationResultDto ToDto(FSharpAuthenticationResult fsResult)
    {
        return new AuthenticationResultDto
        {
            Success = fsResult.IsSuccess,
            Message = fsResult.Message,
            RequiresPasswordChange = fsResult.RequiresPasswordChange
        };
    }
    
    // 66テストケース成功実証済み
}
```

## 🗄 データベース規約

### Entity Framework Core規約
```csharp
// User Entity例
public class User
{
    public int Id { get; set; }
    [Required] [MaxLength(255)] public string Email { get; set; } = string.Empty;
    public string? PasswordHash { get; set; }
    public bool IsFirstLogin { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}

// DbContext設定
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    modelBuilder.Entity<User>()
        .HasIndex(u => u.Email)
        .IsUnique();
}
```

### DB初期化・復元
```sql
-- scripts/restore-admin-user.sql
UPDATE "AspNetUsers" 
SET 
    "PasswordHash" = NULL,
    "SecurityStamp" = LOWER(REPLACE(gen_random_uuid()::text, '-', '')),
    "ConcurrencyStamp" = LOWER(REPLACE(gen_random_uuid()::text, '-', ''))
WHERE "Email" = 'admin@ubiquitous-lang.com';

UPDATE "Users" 
SET 
    "IsFirstLogin" = true,
    "PasswordHash" = NULL
WHERE "Email" = 'admin@ubiquitous-lang.com';
```

## 🎨 Blazor Server規約

### Component構造規約（セキュリティ対応）
```razor
@page "/change-password"
@using Microsoft.AspNetCore.Authorization
@using Microsoft.AspNetCore.Components.Authorization
@using Microsoft.AspNetCore.Components.Forms
@layout EmptyLayout
@attribute [Authorize]
@inject IJsonSerializerService JsonSerializer

@* 【Blazor Server初学者向け解説】 *@
@* セキュリティ原則: 認証情報の画面表示禁止 *@
@* パスワード変更コンポーネントの安全な状態管理 *@

<PageTitle>パスワード変更 - ユビキタス言語管理システム</PageTitle>

<!-- 安全な現在パスワード入力欄 -->
<div class="mb-3">
    <label for="currentPassword" class="form-label">
        <i class="fas fa-key me-1"></i>
        現在のパスワード
    </label>
    <InputText id="currentPassword" 
              @bind-Value="changePasswordRequest.CurrentPassword" 
              type="password" 
              class="form-control form-control-lg"
              placeholder="現在のパスワードを入力してください"
              disabled="@isSubmitting" />
</div>

@code {
    // 【Blazor Server初学者向け解説】
    // セキュリティ安全な状態管理・認証情報非表示原則遵守
    
    private async Task HandleValidSubmit()
    {
        try
        {
            // セキュリティ安全なエラーハンドリング
            if (baseMessage.Contains("現在のパスワード") || baseMessage.Contains("初期パスワード"))
            {
                errorMessage = "パスワード変更に失敗しました: 現在のパスワードを正しく入力してください。";
            }
        }
        catch (Exception ex)
        {
            // ログに認証情報含有禁止
            Console.WriteLine($"Password change process failed: {ex.Message}");
        }
    }
}
```

### CSS規約（Bootstrap 5準拠）
```css
/* カスタムスタイル例 */
.btn-primary {
    background: linear-gradient(45deg, #007bff, #0056b3);
    border: none;
    border-radius: 10px;
    transition: all 0.3s ease;
}

.btn-primary:hover {
    transform: translateY(-2px);
    box-shadow: 0 8px 25px rgba(0, 123, 255, 0.4);
}
```

## 🧪 テスト規約

### 単体テスト規約
```csharp
[TestClass]
public class AuthenticationServiceTests
{
    [TestMethod]
    public async Task AuthenticateAsync_ValidCredentials_ReturnsSuccess()
    {
        // Arrange
        var service = new AuthenticationService();
        
        // Act
        var result = await service.AuthenticateAsync("admin@ubiquitous-lang.com", "validPassword");
        
        // Assert
        Assert.IsTrue(result.IsSuccess);
    }
}
```

### 統合テスト規約
```csharp
[TestClass]
public class AuthenticationIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    
    [TestMethod]
    public async Task Login_ValidCredentials_RedirectsToHome()
    {
        // WebApplicationFactory利用パターン
    }
}
```

### E2Eテスト手順（実証済み）
1. **DB復元**: `/scripts/restore-admin-user.sql`実行
2. **シナリオ1**: 初回ログイン→パスワード変更
3. **シナリオ2**: パスワード変更後通常ログイン
4. **シナリオ3**: F# Authentication Service統合確認・エラーハンドリング確認

## 🔧 開発環境規約

### 必須環境構成
```bash
# Docker環境起動
docker-compose up -d    # PostgreSQL/PgAdmin/Smtp4dev起動

# アプリ起動
dotnet clean
dotnet build            # 0警告0エラー必須
dotnet test            # 106/106テスト成功必須
dotnet run --project src/UbiquitousLanguageManager.Web

# 開発ツールURL
# アプリ: https://localhost:5001
# PgAdmin: http://localhost:8080 (admin@ubiquitous-lang.com / admin123)  
# Smtp4dev: http://localhost:5080
```

### VS Code設定
```json
// .vscode/launch.json
{
    "version": "0.2.0",
    "configurations": [
        {
            "name": ".NET Core Launch (web)",
            "type": "coreclr",
            "request": "launch",
            "preLaunchTask": "build",
            "program": "${workspaceFolder}/src/UbiquitousLanguageManager.Web/bin/Debug/net8.0/UbiquitousLanguageManager.Web.dll",
            "args": [],
            "cwd": "${workspaceFolder}/src/UbiquitousLanguageManager.Web",
            "stopAtEntry": false,
            "serverReadyAction": {
                "action": "openExternally",
                "pattern": "\\bNow listening on:\\s+(https?://\\S+)"
            },
            "env": {
                "ASPNETCORE_ENVIRONMENT": "Development"
            },
            "sourceFileMap": {
                "/Views": "${workspaceFolder}/Views"
            }
        }
    ]
}
```

## 📋 品質基準・チェックリスト

### 必須品質基準
- ✅ **0 Warning, 0 Error**: dotnet build結果
- ✅ **106/106テスト成功**: dotnet test結果
- ✅ **Clean Architectureスコア**: 94/100点維持
- ✅ **E2E動作確認**: admin@ubiquitous-lang.com初期パスワード変更フロー
- ✅ **セキュリティ確認**: 認証情報画面表示禁止・UI安全性確保

### コード品質チェック
- ✅ **初学者コメント**: Blazor Server・F#概念説明必須
- ✅ **ADR準拠**: 重要技術決定参照・記録
- ✅ **用語統一**: 「ユビキタス言語」使用（「用語」禁止）
- ✅ **DRY原則**: 重複実装排除・設定一元管理
- ✅ **セキュリティ原則**: 認証情報表示禁止・安全なUI設計

## 🚀 セキュリティ修正成功パターン（2025-09-11確立）

### UI修正成功事例
1. **問題特定**: パスワード変更画面・初期パスワード情報表示によるセキュリティリスク
2. **適切な修正範囲**: ラベル・placeholder・警告メッセージ・バリデーションメッセージ
3. **段階的修正**: 各箇所の段階的修正・品質確認
4. **成果確認**: ビルド・動作確認・ユーザー確認・100%達成

### 技術実装パターン
- **現在のパスワード関連エラー**: 統一メッセージ（セキュリティ情報削除）
- **パスワード強度・その他エラー**: 元のbaseMessage保持（詳細情報維持）
- **UI表示**: 認証情報完全削除・一般的メッセージ統一

## ⚠️ 重要制約・注意点

### 開発制約
- **HTTPS必須**: https://localhost:5001（HTTP非対応）
- **Docker依存**: PostgreSQL・PgAdmin・Smtp4dev要起動
- **DB復元必須**: E2Eテスト後`/scripts/restore-admin-user.sql`実行

### セキュリティ制約（2025-09-11確立）
- **認証情報表示禁止**: UI・エラーメッセージ・ログ出力全面禁止
- **一般的メッセージ**: 具体的認証情報を含まない汎用表現使用
- **設計時安全性**: 新規UI実装時の認証情報表示リスク事前確認

### 技術制約
- **ConfigureHttpJsonOptions**: Web API専用・Blazor Component適用不可→JsonSerializerService利用
- **F#/C#境界**: TypeConverter基盤経由・直接型変換禁止
- **ASP.NET Core Identity**: Cookie認証・SecurityStamp更新必須