# 技術スタック・規約

## アーキテクチャ構成

### Clean Architecture構成
```
Web (C# Blazor Server) → Contracts (C# DTOs/TypeConverters) → Application (F# UseCases) → Domain (F# Models)
                      ↘ Infrastructure (C# EF Core/Repository) ↗
```

### 技術スタック
- **Frontend**: Blazor Server + Bootstrap 5 + SignalR
- **Backend**: ASP.NET Core 8.0 + Entity Framework Core 8.0
- **Domain/Application**: F# 8.0 + 関数型プログラミング
- **Database**: PostgreSQL 16 (Docker Container)
- **認証**: ASP.NET Core Identity
- **テスト**: xUnit + FsUnit + Moq + WebApplicationFactory

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
├── UbiquitousLanguageManager.Integration.Tests/ # C# 統合テスト
└── UbiquitousLanguageManager.Web.Tests/        # C# Webテスト
```

## F# 実装規約

### ドメインモデル設計
- **不変データ**: Record型・判別共用体活用
- **純粋関数**: 副作用排除・参照透明性維持
- **Result型**: エラーハンドリング・鉄道指向プログラミング
- **Option型**: Null参照排除・安全な値表現

### コーディング規約
```fsharp
// 型定義
type UserId = UserId of Guid
type EmailAddress = EmailAddress of string

// Result型活用
type CreateUserResult = 
    | Success of User
    | InvalidEmail of string
    | DuplicateUser of string

// パターンマッチング
let processUser user =
    match user.Status with
    | Active -> activateUser user
    | Inactive -> deactivateUser user
    | Suspended reason -> suspendUser user reason
```

## C# 実装規約

### Blazor Server実装
- **ライフサイクル**: OnInitializedAsync・OnAfterRenderAsync活用
- **状態管理**: StateHasChanged明示的呼び出し
- **エラーハンドリング**: ErrorBoundary・例外ログ記録
- **パフォーマンス**: PreRender対応・SignalR最適化

### Entity Framework規約
```csharp
// Entity設計
public class UserEntity
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

// Repository実装
public class UserRepository : IUserRepository
{
    private readonly AppDbContext _context;
    
    public async Task<User?> GetByIdAsync(Guid id)
    {
        var entity = await _context.Users.FindAsync(id);
        return entity?.ToDomainModel();
    }
}
```

## TypeConverter実装規約

### F#↔C#変換パターン
```csharp
// C# Contracts層
public static class UserTypeConverter
{
    public static UserDto ToDto(this FSharpDomain.User user)
    {
        return new UserDto
        {
            Id = user.Id.Value,
            Email = user.Email.Value,
            CreatedAt = user.CreatedAt
        };
    }
    
    public static FSharpDomain.User ToDomainModel(this UserDto dto)
    {
        return FSharpDomain.User.Create(
            new UserId(dto.Id),
            new EmailAddress(dto.Email),
            dto.CreatedAt
        );
    }
}
```

## データベース設計規約

### PostgreSQL設計指針
- **主キー**: UUID(Guid)使用・シーケンシャル避ける
- **インデックス**: 検索頻度・パフォーマンス重視設計
- **制約**: NOT NULL・UNIQUE・CHECK制約活用
- **監査**: CreatedAt・UpdatedAt・CreatedBy・UpdatedBy必須

### Migration規約
```bash
# Migration作成
dotnet ef migrations add MigrationName --project src/UbiquitousLanguageManager.Infrastructure

# Migration適用
dotnet ef database update --project src/UbiquitousLanguageManager.Infrastructure
```

## テスト実装規約

### 単体テスト（F#）
```fsharp
[<Test>]
let ``CreateUser_ValidInput_ReturnsSuccess`` () =
    // Arrange
    let email = EmailAddress "test@example.com"
    
    // Act
    let result = User.create email
    
    // Assert
    match result with
    | Success user -> 
        user.Email |> should equal email
    | _ -> 
        failtest "Expected Success"
```

### 統合テスト（C#）
```csharp
[Fact]
public async Task GetUser_ValidId_ReturnsUser()
{
    // Arrange
    await using var app = new WebApplicationFactory<Program>();
    var client = app.CreateClient();
    
    // Act
    var response = await client.GetAsync("/api/users/123");
    
    // Assert
    response.StatusCode.Should().Be(HttpStatusCode.OK);
}
```

## 開発環境・ツール

### 現在の開発環境
- **基本構成**: ローカル環境 + Docker Compose
- **IDE**: VS Code/Cursor + 28個推奨拡張機能
- **データベース**: PostgreSQL 16 (Docker Container)
- **開発補助**: PgAdmin, Smtp4dev (Docker Container)

### Dev Container移行計画（GitHub Issue #37）
- **移行予定**: 後日実施・詳細計画策定完了
- **期待効果**: 環境構築時間90%短縮（1-2時間 → 5分）
- **技術要件**: .NET 8.0 + F# + PostgreSQL完全対応確認済み
- **ROI分析**: 新規メンバー2名参加で投資回収・開発効率10-20%向上

### VS Code拡張機能（自動設定予定）
```json
// Dev Container移行時の自動インストール拡張機能
[
  "ms-dotnettools.csharp",           // C#開発
  "ms-dotnettools.csdevkit",         // C# Dev Kit
  "ionide.ionide-fsharp",            // F#開発
  "formulahendry.dotnet-test-explorer", // テスト実行
  "ms-azuretools.vscode-docker",     // Docker管理
  "mtxr.sqltools",                   // データベース接続
  "mtxr.sqltools-driver-pg",         // PostgreSQL Driver
  "eamodio.gitlens",                 // Git拡張
  "christian-kohler.path-intellisense", // パス補完
  "streetsidesoftware.code-spell-checker", // スペルチェック
  "shardulm94.trailing-spaces",      // 末尾スペース管理
  "editorconfig.editorconfig"        // EditorConfig
]
```

## 開発コマンド

### ビルド・実行コマンド
```bash
# 全体ビルド
dotnet build

# Web実行
dotnet run --project src/UbiquitousLanguageManager.Web

# テスト実行
dotnet test

# カバレッジ測定
dotnet test --collect:"XPlat Code Coverage"
```

### Docker環境コマンド
```bash
# 環境起動
docker-compose up -d

# 環境停止
docker-compose down

# ログ確認
docker-compose logs postgres
```

## 🚀 Commands一覧（2025-09-25更新）

### セッション管理Commands
- **session-start.md**: セッション開始プロセス・Serena初期化・目的設定
- **session-end.md**: セッション終了プロセス・差分更新・記録作成・メモリー30日管理

### Phase管理Commands
- **phase-start.md**: Phase開始準備・前提条件確認・SubAgent選択
- **phase-end.md**: Phase総括・成果確認・次Phase準備

### Step管理Commands
- **step-start.md**: Step開始・task-breakdown統合・並列実行計画
- **step-end-review.md**: Step品質確認・完了確認・継続判断

### 品質管理Commands（強化版）
- **spec-validate**: Phase/Step開始前事前検証（100点満点・3カテゴリ）
- **spec-compliance-check**: 加重スコアリング仕様準拠監査（50/30/20点配分）
- **tdd-practice-check**: TDD実践確認・テストカバレッジ
- **command-quality-check**: Commands実行品質確認

### 新規Commands（2025-09-25追加）
- **task-breakdown**: 自動タスク分解・TodoList連携・Clean Architecture層別分解

### SubAgent選択Commands
- **subagent-selection**: 作業特性・最適Agent組み合わせ選択

## 🎯 仕様駆動開発強化体制（2025-09-25追加）

### 加重スコアリング体系
```yaml
肯定的仕様準拠度: 50点満点（重要度: 最高）
  - 必須機能実装: 30点
  - 推奨機能実装: 15点
  - 拡張機能実装: 5点

否定的仕様遵守度: 30点満点（重要度: 高）
  - 禁止事項遵守: 20点
  - 制約条件遵守: 10点

実行可能性・品質: 20点満点（重要度: 中）
  - テストカバレッジ: 8点
  - パフォーマンス: 6点
  - 保守性: 6点
```

### 自動証跡記録機能
- **実装箇所自動検出**: 仕様項番コメントからの逆引き
- **コードスニペット収集**: 重要実装部分の自動抽出
- **実装行番号マッピング**: 仕様項番 ↔ ソースコード行番号対応表

### Phase B1特化機能
```yaml
GitHub Issue連携:
  - Issue #38自動読み込み: 3項目の詳細タスク化
  - 新規Issue自動作成: サブタスクIssue作成
  
Clean Architecture層別分解:
  - Domain層（F#）: Project型定義・Smart Constructor実装
  - Application層（F#）: CreateProjectCommand/Query定義
  - Contracts層（C#）: ProjectDto/CreateProjectDto実装
  - Infrastructure層（C#）: ProjectRepository実装
  - Web層（C#/Blazor Server）: プロジェクト管理画面実装

権限制御実装:
  - 4ロール×4機能=16通りテストケース
  - SuperUser/ProjectManager/DomainApprover/GeneralUser
  - 作成・編集・削除・参照機能の組み合わせ
```

## 環境設定

### 開発環境URL
- **アプリケーション**: https://localhost:5001
- **PgAdmin**: http://localhost:8080 (admin@ubiquitous-lang.com / admin123)
- **Smtp4dev**: http://localhost:5080

### 認証情報
- **スーパーユーザー**: admin@ubiquitous-lang.com / su
- **一般ユーザー**: user@ubiquitous-lang.com / password123

### 接続文字列（Dev Container移行時調整予定）
```json
// 現在（ローカル環境）
"DefaultConnection": "Host=localhost;Database=ubiquitous_lang_db;Username=ubiquitous_lang_user;Password=ubiquitous_lang_password;Port=5432"

// Dev Container移行後
"DefaultConnection": "Host=postgres;Database=ubiquitous_lang_db;Username=ubiquitous_lang_user;Password=ubiquitous_lang_password;Port=5432"
```

## パフォーマンス・監視

### ログ設定
- **Serilog**: 構造化ログ・レベル分離
- **Application Insights**: パフォーマンス監視（本番環境）
- **Debug出力**: 開発時詳細ログ・リアルタイム確認

### メトリクス監視
- **レスポンス時間**: 500ms以下維持
- **メモリ使用量**: 2GB以下維持
- **データベース接続**: 接続プール最適化
- **CPU使用率**: 70%以下維持

## セキュリティ実装規約

### 認証・認可
- **Identity Framework**: ASP.NET Core Identity準拠
- **JWT Token**: API認証・有効期限管理
- **Role管理**: Admin・User・ReadOnly階層管理
- **Session管理**: タイムアウト・同時ログイン制御

### 入力検証・サニタイゼーション
- **サーバーサイド検証**: 必須・信頼境界での検証
- **クライアント検証**: UX補助・即座フィードバック
- **SQL Injection**: Entity Framework・パラメーター化クエリ
- **XSS対策**: 自動エスケープ・CSP設定

---
**最終更新**: 2025-09-25（仕様駆動開発強化・Command体系統合・task-breakdown追加）  
**重要追加**: 加重スコアリング・自動証跡記録・Phase B1特化・GitHub Issue連携