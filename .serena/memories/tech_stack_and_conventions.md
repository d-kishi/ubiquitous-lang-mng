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

## F# 実装規約・パターン（2025-09-25拡張）

### Railway-oriented Programming（ROP）実装パターン
Phase B1 Domain層実装において、以下のROPパターンを標準適用：

```fsharp
// Result型活用・エラーハンドリング
type CreateProjectResult = 
    | Success of Project * Domain
    | InvalidProjectName of string
    | DuplicateProject of string
    | DomainCreationFailed of string

// パイプライン処理・関数合成
let createProjectWithDomain projectName =
    validateProjectName projectName
    |> Result.bind createProject
    |> Result.bind createDefaultDomain
    |> Result.bind saveWithTransaction
```

### デフォルトドメイン自動作成パターン（2025-09-25新設）
ProjectDomainService実装において、原子性保証・失敗時ロールバックを実装：

```fsharp
// ProjectDomainService
module ProjectDomainService =
    let createProjectWithDefaultDomain (projectName: ProjectName) =
        use transaction = beginTransaction()
        projectName
        |> Project.create
        |> Result.bind (fun project ->
            Domain.createDefault project.Id
            |> Result.map (fun domain -> project, domain))
        |> Result.bind (fun (project, domain) ->
            Repository.saveProject project
            |> Result.bind (fun _ -> Repository.saveDomain domain)
            |> Result.map (fun _ -> project, domain))
        |> Result.bind (fun result ->
            transaction.Commit()
            Success result)
        |> Result.mapError (fun error ->
            transaction.Rollback()
            error)
```

### Smart Constructor・制約実装パターン
```fsharp
// Project型・Smart Constructor
type ProjectName = private ProjectName of string
type ProjectId = ProjectId of Guid

module ProjectName =
    let create (value: string) =
        if String.IsNullOrWhiteSpace(value) then
            Error "Project name cannot be empty"
        elif value.Length > 100 then
            Error "Project name cannot exceed 100 characters"
        else
            Ok (ProjectName value)
    
    let value (ProjectName name) = name

type Project = {
    Id: ProjectId
    Name: ProjectName
    CreatedAt: DateTime
    UpdatedAt: DateTime option
}

module Project =
    let create (name: ProjectName) =
        {
            Id = ProjectId (Guid.NewGuid())
            Name = name
            CreatedAt = DateTime.UtcNow
            UpdatedAt = None
        }
```

### ドメインモデル設計
- **不変データ**: Record型・判別共用体活用
- **純粋関数**: 副作用排除・参照透明性維持
- **Result型**: エラーハンドリング・鉄道指向プログラミング
- **Option型**: Null参照排除・安全な値表現

## C# 実装規約

### Blazor Server実装
- **ライフサイクル**: OnInitializedAsync・OnAfterRenderAsync活用
- **状態管理**: StateHasChanged明示的呼び出し
- **エラーハンドリング**: ErrorBoundary・例外ログ記録
- **パフォーマンス**: PreRender対応・SignalR最適化

### Entity Framework規約・EF Core BeginTransaction実装（2025-09-25追加）
```csharp
// Repository実装・原子性保証
public class ProjectRepository : IProjectRepository
{
    private readonly AppDbContext _context;
    
    public async Task<Result<Project>> CreateProjectWithDomainAsync(CreateProjectCommand command)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
        
        try
        {
            // Project作成
            var projectEntity = new ProjectEntity 
            { 
                Name = command.ProjectName,
                CreatedAt = DateTime.UtcNow 
            };
            _context.Projects.Add(projectEntity);
            await _context.SaveChangesAsync();
            
            // デフォルトDomain作成
            var domainEntity = new DomainEntity
            {
                ProjectId = projectEntity.Id,
                Name = "Default Domain",
                IsDefault = true,
                CreatedAt = DateTime.UtcNow
            };
            _context.Domains.Add(domainEntity);
            await _context.SaveChangesAsync();
            
            await transaction.CommitAsync();
            return Result.Success(projectEntity.ToDomainModel());
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return Result.Failure($"Transaction failed: {ex.Message}");
        }
    }
}
```

### TypeConverter実装規約・F#↔C#境界最適化（2025-09-25拡張）
```csharp
// ProjectDto・TypeConverter実装
public static class ProjectTypeConverter
{
    public static ProjectDto ToDto(this FSharpDomain.Project project)
    {
        return new ProjectDto
        {
            Id = project.Id.Value,
            Name = project.Name |> ProjectName.value,
            CreatedAt = project.CreatedAt,
            UpdatedAt = project.UpdatedAt
        };
    }
    
    public static FSharpDomain.Project ToDomainModel(this ProjectDto dto)
    {
        var projectName = ProjectName.create(dto.Name)
            .GetValueOrThrow(); // 検証済み前提
            
        return new FSharpDomain.Project(
            new ProjectId(dto.Id),
            projectName,
            dto.CreatedAt,
            dto.UpdatedAt
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

## テスト実装規約・TDD実践パターン（2025-09-25強化）

### F# 単体テスト・Red-Green-Refactorサイクル
```fsharp
module ProjectTests =

[<Test>]
let ``createProjectWithDomain_ValidInput_ReturnsProjectAndDomain`` () =
    // Red: テスト失敗を確認
    let projectName = ProjectName.create "Test Project" |> Result.getOk
    
    // Green: 実装してテスト成功
    let result = ProjectDomainService.createProjectWithDefaultDomain projectName
    
    // Refactor: リファクタリング・品質向上
    match result with
    | Success (project, domain) -> 
        project.Name |> should equal projectName
        domain.IsDefault |> should be True
    | _ -> 
        failtest "Expected Success with Project and Domain"

[<Test>]
let ``createProjectWithDomain_TransactionFailure_RollsBack`` () =
    // 原子性保証・ロールバック確認
    let projectName = ProjectName.create "Test Project" |> Result.getOk
    
    // Repository失敗をシミュレート
    let mockRepo = Mock.Of<IProjectRepository>()
    Mock.Setup(fun x -> x.SaveProject(It.IsAny<Project>())).Throws<Exception>()
    
    let result = ProjectDomainService.createProjectWithDefaultDomain projectName
    
    match result with
    | DomainCreationFailed _ -> () // 期待される失敗
    | _ -> failtest "Expected transaction rollback failure"
```

### 統合テスト（C#）
```csharp
[Fact]
public async Task CreateProject_ValidInput_CreatesProjectAndDomain()
{
    // Arrange
    await using var app = new WebApplicationFactory<Program>();
    var client = app.CreateClient();
    
    var command = new CreateProjectCommand 
    { 
        ProjectName = "Integration Test Project" 
    };
    
    // Act
    var response = await client.PostAsJsonAsync("/api/projects", command);
    
    // Assert
    response.StatusCode.Should().Be(HttpStatusCode.Created);
    
    // データベース確認
    using var scope = app.Services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    
    var project = await context.Projects.FirstOrDefaultAsync(p => p.Name == command.ProjectName);
    project.Should().NotBeNull();
    
    var domain = await context.Domains.FirstOrDefaultAsync(d => d.ProjectId == project.Id && d.IsDefault);
    domain.Should().NotBeNull();
}
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

### Phase B1技術実装パターン（2025-09-25確立）
```yaml
Domain層実装:
  - F# Railway-oriented Programming: Result型パイプライン
  - ProjectDomainService: 原子性保証・失敗時ロールバック
  - Smart Constructor: ProjectName・ProjectId制約実装

Application層実装:
  - IProjectManagementService: Command/Query分離
  - CreateProjectCommand: バリデーション・ビジネスルール
  - ProjectQuery: 権限制御・フィルタリング

Infrastructure層実装:
  - EF Core BeginTransaction: 原子性保証実装
  - Repository: CRUD・権限フィルタ統合
  - UserProjects中間テーブル: 多対多関連最適実装

Web層実装:
  - Blazor Server権限制御: 4ロール×4機能マトリックス実装
  - リアルタイム更新: SignalR・StateHasChanged最適化
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

### 開発コマンド
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

## 環境設定

### 開発環境URL
- **アプリケーション**: https://localhost:5001
- **PgAdmin**: http://localhost:8080 (admin@ubiquitous-lang.com / admin123)
- **Smtp4dev**: http://localhost:5080

### 認証情報
- **スーパーユーザー**: admin@ubiquitous-lang.com / su
- **一般ユーザー**: user@ubiquitous-lang.com / password123

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
**最終更新**: 2025-09-25（Phase B1技術実装パターン確立・Railway-oriented Programming・デフォルトドメイン自動作成・TDD実践強化）  
**重要追加**: F# ROP実装パターン・EF Core原子性保証・TypeConverter最適化・加重スコアリング体系・Phase B1特化実装方針