# 技術スタック・規約 - 2025年8月20日更新

## 技術スタック構成

### Frontend - Blazor Server
- **ASP.NET Core 8.0**: Blazor Server基盤
- **Bootstrap 5**: UIフレームワーク
- **SignalR**: リアルタイム通信（Blazor Server標準）
- **JavaScript**: 最小限利用（Blazor優先）

### Backend - ASP.NET Core
- **ASP.NET Core 8.0**: Webアプリケーション基盤
- **ASP.NET Core Identity**: 認証・認可システム
- **Entity Framework Core**: ORM
- **Dependency Injection**: 標準DIコンテナ

### Domain/Application層 - F#
- **F# 8.0**: 関数型プログラミング
- **Result型**: エラーハンドリング
- **Option型**: null安全性
- **パターンマッチング**: 条件分岐・データ処理

### Database
- **PostgreSQL 16**: メインデータベース（Docker Container）
- **Entity Framework Core**: Code First Migrations
- **Connection Pooling**: パフォーマンス最適化

### 開発環境
- **Docker Compose**: PostgreSQL + PgAdmin + Smtp4dev
- **PgAdmin**: データベース管理UI
- **Smtp4dev**: 開発用メールサーバー

## プロジェクト構成

### ソースコード構成
```
src/
├── UbiquitousLanguageManager.Domain/       # F# ドメインモデル
├── UbiquitousLanguageManager.Application/  # F# ユースケース
├── UbiquitousLanguageManager.Contracts/    # C# DTO/TypeConverters
├── UbiquitousLanguageManager.Infrastructure/ # C# EF Core/Repository
└── UbiquitousLanguageManager.Web/         # C# Blazor Server
```

### テストプロジェクト構成
```
tests/
├── UbiquitousLanguageManager.Domain.Tests/     # F# ドメインテスト
├── UbiquitousLanguageManager.Application.Tests/ # F# アプリケーションテスト
├── UbiquitousLanguageManager.Infrastructure.Tests/ # C# インフラテスト
└── UbiquitousLanguageManager.Tests/            # C# 統合テスト
```

## Clean Architecture実装規約

### 依存関係ルール（厳守）
```
Web → Contracts → Application → Domain
  ↘ Infrastructure ↗
```
- **Domain**: 他層に依存しない
- **Application**: Domainのみ依存
- **Infrastructure**: Application・Domain依存
- **Contracts**: Application・Domain依存（F#↔C#境界）
- **Web**: 全層に依存可能

### レイヤー責任分離
#### Domain層（F#）
- **ドメインモデル**: エンティティ・値オブジェクト
- **ドメインサービス**: 複雑なビジネスロジック
- **仕様**: ビジネスルール・制約

#### Application層（F#）
- **ユースケース**: アプリケーションサービス
- **インターフェース**: Infrastructure・Web層抽象化
- **DTO**: データ転送オブジェクト（F#型）

#### Contracts層（C#）
- **TypeConverter**: F#↔C#型変換
- **DTO**: C#側データ転送オブジェクト
- **Mapper**: Result型↔例外変換

#### Infrastructure層（C#）
- **Repository**: データアクセス実装
- **Entity Framework**: DBコンテキスト・エンティティ
- **外部サービス**: メール送信・外部API連携

#### Web層（C#）
- **Blazor Server**: UI・ページ・コンポーネント
- **認証**: ASP.NET Core Identity統合
- **Middleware**: 横断的処理

## F#/C#境界設計規約

### Result型変換パターン（必須）
```csharp
public static class ResultMapper
{
    public static T MapResult<T>(FSharpResult<T, string> result)
    {
        if (FSharpResult.IsOk(result))
            return result.ResultValue;
        else
            throw new DomainException(result.ErrorValue);
    }
}
```

### TypeConverter実装パターン
```csharp
public class EntityTypeConverter
{
    // Domain → DTO変換（常に成功）
    public static EntityDto ToDto(Entity domain) => new()
    {
        Id = domain.Id.Value,
        Name = domain.Name.Value
    };

    // DTO → Domain変換（検証あり）
    public static Result<Entity, string> FromDto(EntityDto dto)
    {
        // バリデーション・ドメインオブジェクト生成
    }
}
```

### エラーハンドリング統一
```csharp
// 統一例外クラス
public class DomainException : Exception
{
    public DomainException(string message) : base(message) { }
}

// Blazor ErrorBoundary
<ErrorBoundary>
    <ChildContent>@ChildContent</ChildContent>
    <ErrorContent>
        <div class="alert alert-danger">@context.Message</div>
    </ErrorContent>
</ErrorBoundary>
```

## Blazor Server実装規約

### コンポーネント構成
```
Pages/
├── Auth/                    # 認証関連ページ
│   ├── Login.razor
│   ├── ChangePassword.razor
│   └── Profile.razor
├── Admin/                   # 管理画面
│   └── UserManagement.razor
└── Index.razor              # トップページ

Shared/
├── MainLayout.razor         # メインレイアウト
├── LoginLayout.razor        # ログイン専用レイアウト
├── NavMenu.razor           # ナビゲーションメニュー
└── ErrorBoundary.razor     # エラーハンドリング
```

### 認証実装パターン
```razor
@page "/admin/users"
@using Microsoft.AspNetCore.Components.Authorization
@attribute [Authorize(Roles = "Admin,SuperAdmin")]
@inject AuthenticationStateProvider AuthenticationStateProvider
@inject UserManager<ApplicationUser> UserManager

@code {
    protected override async Task OnInitializedAsync()
    {
        var authState = await AuthenticationStateProvider.GetAuthenticationStateAsync();
        // 認証処理・初期化
    }
}
```

### 初学者対応コメント規約（ADR_010）
```razor
@code {
    // Blazor Server ライフサイクル解説
    protected override async Task OnInitializedAsync()
    {
        // コンポーネント初期化時に1回実行
        // サーバーサイドで実行されSignalR経由でクライアント更新
    }

    private async Task UpdateData()
    {
        // データ更新後は必ずStateHasChanged()呼び出し
        // これによりUI再レンダリングが実行される
        StateHasChanged();
    }
}
```

## URL設計規約

### 統一ルール（厳守）
- **Blazor Server形式**: 小文字・ハイフン区切り
- **例**: `/change-password`, `/admin/users`, `/profile`

### 認証関連URL
```
/login              → Login.razor
/logout             → Logout.razor
/change-password    → ChangePassword.razor
/profile            → Profile.razor
```

### 管理画面URL
```
/                   → Index.razor（認証分岐）
/admin/users        → UserManagement.razor
/admin/projects     → ProjectManagement.razor（将来実装）
```

## テスト実装規約

### 単体テスト（F# Domain/Application）
```fsharp
[<Test>]
let ``ユビキタス言語作成時_有効な値の場合_成功する`` () =
    // Arrange
    let name = "テスト用語"
    let description = "テスト説明"
    
    // Act
    let result = UbiquitousLanguage.create name description
    
    // Assert
    result |> should be (ofCase <@ Result<_, _>.Ok @>)
```

### 統合テスト（C# WebApplicationFactory）
```csharp
[Test]
public async Task ログイン機能_有効な認証情報_成功()
{
    // Arrange
    await using var application = new TestWebApplicationFactory();
    var client = application.CreateClient();
    
    // Act & Assert
    var response = await client.PostAsync("/login", formData);
    response.StatusCode.Should().Be(HttpStatusCode.Redirect);
}
```

## コーディング規約

### F#規約
- **関数名**: lowerCamelCase
- **型名**: PascalCase
- **Result型**: 必須使用（例外禁止）
- **Option型**: null代替として使用

### C#規約
- **PascalCase**: クラス・メソッド・プロパティ
- **camelCase**: フィールド・ローカル変数
- **async/await**: 非同期処理必須
- **ConfigureAwait(false)**: ライブラリコードで使用

## 用語統一規約（ADR_003）

### 必須用語統一
- ❌ **「用語」** → ✅ **「ユビキタス言語」**
- ❌ **「Term」** → ✅ **「UbiquitousLanguage」**
- ❌ **「語彙」** → ✅ **「ユビキタス言語」**

### 適用範囲
- 全ソースコード（F#・C#・Razor）
- 全ドキュメント（README・CLAUDE.md・ADR）
- UI表示（画面・メッセージ・エラー表示）

## パフォーマンス・セキュリティ規約

### セキュリティ実装必須
- **ValidateAntiForgeryToken**: 全POST/PUT/DELETE
- **Authorize属性**: 認証必要ページ
- **Role-Based認可**: 管理機能アクセス制御
- **HTTPS**: 本番環境強制

### パフォーマンス最適化
- **Connection Pooling**: EF Core設定
- **非同期処理**: 全DB・外部API操作
- **メモリ効率**: IEnumerable活用・using文使用