# 技術スタック・規約

## アーキテクチャ構成

### Clean Architecture全体構成
```
Web (C# Blazor Server) → Contracts (C# DTOs/TypeConverters) → Application (F# UseCases) → Domain (F# Models)
                      ↘ Infrastructure (C# EF Core/Repository) ↗
```

### 現在品質スコア（Phase A8完了時点）
- **総合Clean Architectureスコア**: 89/100点
- **Domain層**: 20/20点（480行F#・完全実装）
- **Application層**: 18/20点（Phase A9で20点達成予定）
- **Infrastructure層**: 16/20点（18-19点が現実的最適解）
- **Web層**: 15/20点（認証統合により改善）
- **Contracts層**: 20/20点（580行TypeConverter基盤）

## 主要技術スタック

### Frontend技術
- **Blazor Server**: ASP.NET Core 8.0
- **Bootstrap 5**: レスポンシブUI・カード形式統一
- **SignalR**: リアルタイム通信（Blazor Server標準）
- **JavaScript相互運用**: トースト通知・DOM操作

### Backend技術
- **ASP.NET Core 8.0**: Web API・認証・依存注入
- **Entity Framework Core**: PostgreSQL統合・Code-First移行
- **ASP.NET Core Identity**: 認証・認可・ユーザー管理
- **FluentValidation**: バリデーション統一

### 関数型言語（F# 8.0）
- **Domain層**: 480行・ドメインモデル・ビジネスルール
- **Application層**: ユースケース・ビジネスロジック
- **Result/Option型**: エラーハンドリング・null安全
- **Railway-oriented Programming**: 関数型エラー処理パターン

### データベース・インフラ
- **PostgreSQL 16**: メインデータベース（Docker Container）
- **PgAdmin**: データベース管理UI（http://localhost:8080）
- **Smtp4dev**: 開発用メールサーバー（http://localhost:5080）
- **Docker Compose**: 開発環境統一

## 開発環境・ツール

### 実行環境
- **アプリケーション**: https://localhost:5001
- **HTTPS統一**: launchSettings.json・VS Code設定統一（Issue #16解決済み）
- **ホットリロード**: Blazor Server標準機能活用

### ビルド・テストコマンド
```bash
# ビルド
dotnet build                                           # 全体ビルド（0警告0エラー必須）
dotnet run --project src/UbiquitousLanguageManager.Web # アプリ起動

# テスト
dotnet test                                            # 全テスト（220テスト・95%カバレッジ）
dotnet test --collect:"XPlat Code Coverage"           # カバレッジ測定

# データベース
dotnet ef migrations add MigrationName --project src/UbiquitousLanguageManager.Infrastructure
dotnet ef database update --project src/UbiquitousLanguageManager.Infrastructure
```

### 品質管理ツール
- **TestWebApplicationFactory**: 統合テストパターン確立
- **FluentAssertions**: テスト可読性向上
- **Microsoft.AspNetCore.Mvc.Testing**: Web層テスト
- **F# NUnit**: F#層単体テスト

## コーディング規約・パターン

### F# Domain/Application層規約
```fsharp
// Smart Constructor Pattern
type Email = private Email of string
module Email =
    let create (input: string) : Result<Email, string> =
        if String.IsNullOrWhiteSpace(input) then Error "Email cannot be empty"
        elif not (input.Contains("@")) then Error "Invalid email format"
        else Ok (Email input)

// Railway-oriented Programming
type AuthenticationResult<'T> =
    | Success of 'T
    | InvalidCredentials
    | UserNotFound of Email
    | ValidationError of string

// Async by Design
let authenticateAsync (email: Email) (password: Password) : Async<AuthenticationResult<User>> =
    async {
        // 非同期認証処理
    }
```

### Blazor Server規約
```csharp
// ページライフサイクル
protected override async Task OnInitializedAsync()
{
    // 初期化処理
    await LoadDataAsync();
    StateHasChanged(); // 明示的UI更新
}

// エラーハンドリング・UI統合
private async Task HandleSubmitAsync()
{
    try
    {
        isSubmitting = true;
        StateHasChanged();
        
        var result = await Service.ProcessAsync(model);
        if (result.IsSuccess)
        {
            await JSRuntime.InvokeVoidAsync("showToast", "成功", "処理が完了しました");
        }
    }
    finally
    {
        isSubmitting = false;
        StateHasChanged();
    }
}
```

### TypeConverter基盤（580行）
```csharp
// F#↔C#境界型変換
public static class AuthenticationConverter
{
    public static AuthenticationRequestDto ToDto(this FSharpAuthenticationRequest request) =>
        new AuthenticationRequestDto
        {
            Email = request.Email.Value,
            Password = request.Password.Value
        };

    public static FSharpResult<User, AuthenticationError> FromDto(AuthenticationResultDto dto) =>
        dto.IsSuccess 
            ? FSharpResult<User, AuthenticationError>.NewSuccess(dto.User.ToFSharpUser())
            : FSharpResult<User, AuthenticationError>.NewFailure(dto.Error.ToFSharpError());
}
```

## 認証・セキュリティ

### ASP.NET Core Identity統合（Phase A8確立）
- **UserManager<ApplicationUser>**: ユーザー管理・パスワード処理
- **SignInManager<ApplicationUser>**: 認証・セッション管理
- **初期ユーザー**: admin@ubiquitous-lang.com / su
- **パスワード変更**: 初回ログイン時強制変更実装済み
- **パスワードリセット**: メール送信・トークン検証実装済み

### セキュリティ実装パターン
- **HTTPS統一**: 5001ポート・SSL証明書自動
- **CSRF保護**: Blazor Server標準機能
- **認証状態管理**: AuthenticationStateProvider統合
- **権限制御**: [Authorize]属性・ロールベースアクセス

## Phase A9技術実装詳細

### F# Application層実装予定（Step 1・180分）
```fsharp
// IAuthenticationService F#実装
type IAuthenticationService =
    abstract member AuthenticateAsync: Email * Password -> Async<Result<AuthenticatedUser, AuthenticationError>>
    abstract member ChangePasswordAsync: UserId * Password * Password -> Async<Result<unit, AuthenticationError>>

// UserRepositoryAdapter（Infrastructure層改修）
type UserRepositoryAdapter(userManager: UserManager<ApplicationUser>) =
    interface IUserRepository with
        member this.FindByEmailAsync(email: Email) = 
            async {
                let! user = userManager.FindByEmailAsync(email.Value) |> Async.AwaitTask
                return Option.ofObj user |> Option.map ApplicationUserConverter.toFSharp
            }
```

### Railway-oriented Programming実装パターン
```fsharp
// エラー型定義
type AuthenticationError =
    | InvalidCredentials
    | UserNotFound of Email  
    | ValidationError of string
    | SystemError of exn

// 合成可能な認証フロー
let authenticateUser authService email password =
    email
    |> Email.create
    |> Result.bind (fun validEmail ->
        password
        |> Password.create  
        |> Result.bind (fun validPassword ->
            authService.AuthenticateAsync(validEmail, validPassword)))
```

## データベース設計・Entity Framework

### ApplicationUser拡張（Identity統合）
```csharp
public class ApplicationUser : IdentityUser
{
    public string FullName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public bool IsInitialPassword { get; set; } = true;
}
```

### Entity Framework構成
```csharp
// ApplicationDbContext
public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public DbSet<Project> Projects { get; set; }
    public DbSet<Domain> Domains { get; set; }
    public DbSet<UbiquitousLanguage> UbiquitousLanguages { get; set; }
    
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        // Entity構成・シード実行
    }
}
```

## UI・フロントエンド規約

### Bootstrap 5統一パターン（Phase A8確立・92/100点）
```html
<!-- カード形式統一 -->
<div class="card shadow-sm">
    <div class="card-header bg-primary text-white">
        <h5 class="card-title mb-0"><i class="fas fa-lock me-2"></i>ログイン</h5>
    </div>
    <div class="card-body p-4">
        <!-- フォーム内容 -->
    </div>
</div>

<!-- ローディング状態統一 -->
<button type="submit" class="btn btn-primary" disabled="@isSubmitting">
    @if (isSubmitting)
    {
        <span class="spinner-border spinner-border-sm me-2"></span>
    }
    <i class="fas fa-sign-in-alt me-2"></i>ログイン
</button>
```

### レスポンシブ対応統一
```html
<div class="row justify-content-center">
    <div class="col-lg-4 col-md-6 col-sm-8">
        <!-- フォーム内容 -->
    </div>
</div>
```

## テスト戦略・品質保証

### TDD実践パターン（Red-Green-Refactor）
```csharp
[Test]
public async Task AuthenticateAsync_ValidCredentials_ReturnsSuccess()
{
    // Arrange
    var email = Email.Create("test@example.com").Value;
    var password = Password.Create("ValidPassword123").Value;
    
    // Act
    var result = await authenticationService.AuthenticateAsync(email, password);
    
    // Assert
    result.Should().BeOfType<Success<AuthenticatedUser>>();
}
```

### 統合テストパターン（TestWebApplicationFactory）
```csharp
public class AuthenticationIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient client;
    
    [Test]
    public async Task Login_ValidCredentials_RedirectsToDashboard()
    {
        // 実際のHTTPリクエスト・レスポンステスト
    }
}
```

## 技術制約・現実的判断（Phase A9確立）

### ASP.NET Core Identity制約の受容
- **UserManager依存**: パスワードハッシュ化・検証の内部実装
- **SignInManager依存**: クッキー認証・セッション管理
- **セキュリティトークン**: パスワードリセット・メール確認
- **現実的最適解**: 完全自作より統合活用が投資対効果最大

### Infrastructure層スコア制約（16→18-19点）
- **Entity Framework依存**: Repository抽象化の限界
- **Identity統合**: ドメイン純粋性vs実用性トレードオフ
- **保守性重視**: フレームワーク活用による長期安定性

## 用語統一・ドメイン言語（ADR_003）

### 統一用語
- **「ユビキタス言語」**: 「用語」ではなく必須使用
- **「承認者」**: 「管理者」との区別明確化
- **「ドメイン」**: ビジネス領域・技術領域での使い分け明確化
- **「プロジェクト」**: 開発プロジェクト・業務プロジェクト区別

### 技術用語統一
- **「TypeConverter」**: F#↔C#境界変換コンポーネント
- **「Railway-oriented Programming」**: F#関数型エラー処理パターン
- **「Clean Architecture」**: レイヤー依存関係の統一呼称

---

**最終更新**: 2025-09-07（Phase A9計画策定・技術実装詳細追加）  
**品質基準**: 0警告0エラー・220テスト成功・95%カバレッジ維持  
**次期重点**: F# Application層実装・Railway-oriented Programming導入