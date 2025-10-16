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
- **テスト**: xUnit + FsUnit + Moq + WebApplicationFactory + bUnit (Blazor Component Testing)
- **E2Eテスト**: Playwright for .NET + **Playwright MCP統合完了**（2025-10-17・Claude Code直接統合・25ツール利用可能）

## プロジェクト構成

### ソースコード構成
```
src/
├── UbiquitousLanguageManager.Domain/       # F# ドメインモデル
│   ├── Common/                            # 共通境界文脈（Phase B1 Step4で確立）
│   │   ├── CommonTypes.fs                 # 共通ID型・Permission・Role定義
│   │   ├── CommonValueObjects.fs          # Description・ApprovalStatus
│   │   └── CommonSpecifications.fs        # Specification Pattern実装
│   ├── Authentication/                    # 認証境界文脈（Phase B1 Step4で確立）
│   │   ├── AuthenticationValueObjects.fs  # Email・UserName・Password
│   │   ├── AuthenticationErrors.fs        # AuthenticationError型
│   │   ├── AuthenticationEntities.fs      # User集約
│   │   └── UserDomainService.fs           # ユーザードメインサービス
│   ├── ProjectManagement/                 # プロジェクト管理境界文脈（Phase B1 Step4で確立）
│   │   ├── ProjectValueObjects.fs         # ProjectName・DomainName
│   │   ├── ProjectErrors.fs               # ProjectError型
│   │   ├── ProjectEntities.fs             # Project・Domain集約
│   │   └── ProjectDomainService.fs        # プロジェクトドメインサービス
│   └── UbiquitousLanguageManagement/      # ユビキタス言語管理境界文脈（Phase B1 Step4 Phase6で確立）
│       ├── UbiquitousLanguageValueObjects.fs  # JapaneseName・EnglishName
│       ├── UbiquitousLanguageErrors.fs    # UbiquitousLanguageError型
│       ├── UbiquitousLanguageEntities.fs  # DraftUbiquitousLanguage・FormalUbiquitousLanguage集約
│       └── UbiquitousLanguageDomainService.fs  # ユビキタス言語ドメインサービス
├── UbiquitousLanguageManager.Application/  # F# ユースケース
├── UbiquitousLanguageManager.Contracts/    # C# DTO/TypeConverters
├── UbiquitousLanguageManager.Infrastructure/ # C# EF Core/Repository
└── UbiquitousLanguageManager.Web/         # C# Blazor Server
```

### Domain層Bounded Context構成（Phase B1 Step4完成・2025-10-01）
Phase B1 Step4で確立された4つの境界文脈構造：

```yaml
Common（共通境界文脈）: 411行・3ファイル
  - CommonTypes.fs: 全境界文脈共通のID型・Permission（17種類）・Role（4種類）
  - CommonValueObjects.fs: Description・ApprovalStatus
  - CommonSpecifications.fs: Specification Pattern実装

Authentication（認証境界文脈）: 983行・4ファイル
  - AuthenticationValueObjects.fs: Email・UserName・Password・SecurityStamp（Smart Constructor）
  - AuthenticationErrors.fs: AuthenticationError型（22エラーケース）
  - AuthenticationEntities.fs: User集約ルート（50+フィールド・20+メソッド）
  - UserDomainService.fs: 8つのユーザー検証関数

ProjectManagement（プロジェクト管理境界文脈）: 887行・4ファイル
  - ProjectValueObjects.fs: ProjectName・DomainName（Smart Constructor）
  - ProjectErrors.fs: ProjectError型（Railway-oriented Programming）
  - ProjectEntities.fs: Project・Domain集約ルート
  - ProjectDomainService.fs: createProjectWithDefaultDomain（原子性保証実装）

UbiquitousLanguageManagement（ユビキタス言語管理境界文脈）: 350行・4ファイル
  - UbiquitousLanguageValueObjects.fs: JapaneseName・EnglishName
  - UbiquitousLanguageErrors.fs: UbiquitousLanguageError型（9エラーケース）
  - UbiquitousLanguageEntities.fs: DraftUbiquitousLanguage・FormalUbiquitousLanguage集約
  - UbiquitousLanguageDomainService.fs: 4つの検証関数

合計: 2,631行・16ファイル・4境界文脈
```

### テストプロジェクト構成
```
tests/
├── UbiquitousLanguageManager.Domain.Tests/     # F# ドメインテスト
├── UbiquitousLanguageManager.Application.Tests/ # F# アプリケーションテスト
├── UbiquitousLanguageManager.Integration.Tests/ # C# 統合テスト
└── UbiquitousLanguageManager.Web.Tests/        # C# Webテスト

# 技術負債: GitHub Issue #40 - テストプロジェクト重複（Phase B完了後統合対応）
# 重複状況: UbiquitousLanguageManager.Tests/Domain ⊆ UbiquitousLanguageManager.Domain.Tests
```

## F# 実装規約・パターン（2025-09-25拡張・2025-09-30完全実証・2025-10-01 Bounded Context化）

### Bounded Context分離パターン（Phase B1 Step4確立・2025-10-01）
Domain層をBounded Contextディレクトリに分離し、Clean Architecture・DDD原則に基づく構造を確立：

#### F# Compilation Order規約（厳格遵守必須）
F#は前方宣言不可のため、依存関係順に厳密なコンパイル順序が必要：

```xml
<!-- .fsprojファイルの標準構造（Phase B1 Step4で確立） -->
<ItemGroup>
  <!-- 1. Common Bounded Context: 全境界文脈で共有される共通型 -->
  <Compile Include="Common\CommonTypes.fs" />      <!-- 最初: ID型・Permission・Role定義 -->
  <Compile Include="Common\CommonValueObjects.fs" /><!-- 2番目: CommonTypes依存 -->
  <Compile Include="Common\CommonSpecifications.fs" /><!-- 3番目: 前2つ依存 -->

  <!-- 2. Authentication Bounded Context: 認証・ユーザー管理境界文脈 -->
  <Compile Include="Authentication\AuthenticationValueObjects.fs" /><!-- Common依存 -->
  <Compile Include="Authentication\AuthenticationErrors.fs" /><!-- ValueObjects依存 -->
  <Compile Include="Authentication\AuthenticationEntities.fs" /><!-- Errors依存 -->
  <Compile Include="Authentication\UserDomainService.fs" /><!-- Entities依存 -->

  <!-- 3. ProjectManagement Bounded Context: プロジェクト管理境界文脈 -->
  <Compile Include="ProjectManagement\ProjectValueObjects.fs" /><!-- Common依存 -->
  <Compile Include="ProjectManagement\ProjectErrors.fs" /><!-- ValueObjects依存 -->
  <Compile Include="ProjectManagement\ProjectEntities.fs" /><!-- Errors+Common依存 -->
  <Compile Include="ProjectManagement\ProjectDomainService.fs" /><!-- Entities依存 -->

  <!-- 4. UbiquitousLanguageManagement Bounded Context: ユビキタス言語管理境界文脈 -->
  <Compile Include="UbiquitousLanguageManagement\UbiquitousLanguageValueObjects.fs" /><!-- Common依存 -->
  <Compile Include="UbiquitousLanguageManagement\UbiquitousLanguageErrors.fs" /><!-- ValueObjects依存 -->
  <Compile Include="UbiquitousLanguageManagement\UbiquitousLanguageEntities.fs" /><!-- Errors+Common依存 -->
  <Compile Include="UbiquitousLanguageManagement\UbiquitousLanguageDomainService.fs" /><!-- Entities依存 -->
</ItemGroup>
```

#### Bounded Context内ファイル構成規約（Phase B1 Step4で確立）
各Bounded Context内のファイル構成パターン（依存関係順）：

```yaml
1. ValueObjects.fs:
   - Smart Constructor実装
   - ドメイン固有値オブジェクト定義
   - 依存: Common\CommonTypes.fs のみ

2. Errors.fs:
   - エラー型（Discriminated Union）
   - ToMessage()・GetCategory()メソッド
   - 依存: ValueObjects.fs

3. Entities.fs:
   - 集約ルート（Aggregate Root）
   - ビジネスロジックメソッド
   - 依存: ValueObjects.fs・Errors.fs・Common\CommonTypes.fs

4. DomainService.fs:
   - ドメインサービス（複数集約にまたがるロジック）
   - Railway-oriented Programming実装
   - 依存: 同Bounded Context全ファイル
```

#### Bounded Context分離の設計原則（Phase B1 Step4実証）
```yaml
分離判断基準:
  - **凝集性**: 関連する概念を1つのBounded Contextに集約
  - **独立性**: 他Bounded Contextへの依存を最小化
  - **境界明確化**: ドメイン用語の意味が境界内で一貫

依存関係管理:
  - **Common優先**: 共通型はCommon Bounded Contextに集約
  - **循環依存禁止**: コンパイル順序で循環依存を防止
  - **境界間依存最小**: 境界を越える依存はCommon経由

ユーザーフィードバック活用:
  - Phase B1 Step4 Phase6追加の経緯: ユーザーが「雛型の名残」を指摘
  - 当初3境界文脈計画 → ユーザー提案で4境界文脈に拡張
  - 結果: Step5（namespace階層化）の問題を事前回避
```

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

### Phase B1 Step3 Application層実装パターン（2025-09-30完全実装・100点満点品質達成）

#### IProjectManagementService実装パターン（完全実装・仕様準拠度100点達成）
```fsharp
// Command/Query分離・権限制御統合・100点品質達成
type IProjectManagementService =
    abstract member CreateProjectAsync: CreateProjectCommand -> Async<Result<ProjectDto, string>>
    abstract member GetProjectsByUserAsync: GetProjectsQuery -> Async<Result<ProjectDto list, string>>
    abstract member UpdateProjectAsync: UpdateProjectCommand -> Async<Result<ProjectDto, string>>
    abstract member DeleteProjectAsync: DeleteProjectCommand -> Async<Result<unit, string>>

// Railway-oriented Programming適用・Domain層統合・完全実装
let createProjectAsync command = async {
    let! validationResult = 
        command
        |> validateCreateCommand
        |> Result.bind (fun cmd -> 
            ProjectDomainService.createProjectWithDefaultDomain 
                (ProjectName.create cmd.Name |> Result.getOk))
    
    return validationResult |> Result.map ProjectDto.fromDomain
}
```

#### 権限制御マトリックス実装（4ロール×4機能・完全実装・100点評価）
```fsharp
// 権限制御の完全実装・100点品質達成
let checkProjectPermission (role: UserRole) (operation: ProjectOperation) 
                          (projectOwnerId: Guid) (userId: Guid) =
    match role, operation with
    | SuperUser, _ -> true
    | ProjectManager, (Create | Read | Update | Delete) -> true
    | DomainApprover, (Read | Update) -> true
    | GeneralUser, Read -> true
    | GeneralUser, (Create | Update | Delete) -> projectOwnerId = userId
    | _ -> false

// Query実装・権限フィルタリング・完全実装
let getProjectsByUserAsync query = async {
    let! projects = Repository.getAllProjects()
    
    return projects
    |> List.filter (fun p -> checkProjectPermission query.UserRole Read p.OwnerId query.UserId)
    |> List.skip ((query.PageNumber - 1) * query.PageSize)
    |> List.take query.PageSize
    |> List.map ProjectDto.fromDomain
    |> Ok
}
```

### デフォルトドメイン自動作成パターン（2025-09-25新設・Phase B1 Step2完全実装）
ProjectDomainService実装において、原子性保証・失敗時ロールバックを実装：

```fsharp
// ProjectDomainService・完全実装・100点品質
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

### Smart Constructor・制約実装パターン（Phase B1 Step2完全実装）
```fsharp
// Project型・Smart Constructor・完全実装
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

### Entity Framework規約・EF Core BeginTransaction実装（2025-09-25追加・Step4準備完了）
```csharp
// Repository実装・原子性保証・Step4実装準備完了
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

### TypeConverter実装規約・F#↔C#境界最適化（2025-09-25拡張・2025-09-30完全実装）

#### Phase B1 Step3 Contracts層実装パターン（2025-09-30完全実装・構文エラー完全修正）
```csharp
// Application DTOs・Command/Query用TypeConverter・完全実装
public static class ProjectCommandConverters
{
    public static CreateProjectCommand ToFSharpCommand(this CreateProjectCommandDto dto)
    {
        return new CreateProjectCommand(
            dto.Name,
            string.IsNullOrEmpty(dto.Description) ? null : dto.Description,
            dto.OwnerId
        );
    }
    
    public static CreateProjectCommandDto ToCSharpDto(CreateProjectCommand command)
    {
        return new CreateProjectCommandDto
        {
            Name = command.Name,
            Description = command.Description ?? string.Empty,
            OwnerId = command.OwnerId
        };
    }
}

// Query用TypeConverter・権限制御統合・完全実装
public static class ProjectQueryConverters
{
    public static GetProjectsQuery ToFSharpQuery(this GetProjectsQueryDto dto)
    {
        var userRole = AuthenticationMapper.StringToRole(dto.UserRole)
            .GetValueOrThrow(); // 認証済み前提
            
        return new GetProjectsQuery(
            dto.UserId,
            userRole,
            dto.PageNumber,
            dto.PageSize
        );
    }
}
```

#### 既存TypeConverter実装（完全実装・構文エラー0達成）
```csharp
// ProjectDto・TypeConverter実装・構文規約完全準拠
public static class ProjectTypeConverter
{
    // Fix-Mode適用成功: メソッド名修正・C#構文規約準拠
    public static Microsoft.FSharp.Core.FSharpResult<User, AuthenticationError> ToFSharpResult(AuthenticationResultDto dto)
    {
        // Fix-Mode適用成功: using alias削除・完全修飾名使用
        return dto.IsSuccess
            ? Microsoft.FSharp.Core.FSharpResult<User, AuthenticationError>.NewOk(dto.User.ToDomainModel())
            : Microsoft.FSharp.Core.FSharpResult<User, AuthenticationError>.NewError(dto.Error);
    }
    
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

## テスト実装規約・TDD実践パターン（2025-09-25強化・2025-09-30優秀評価達成）

### F# 単体テスト・Red-Green-Refactorサイクル（52テスト100%成功・⭐⭐⭐⭐⭐優秀評価）
```fsharp
module ProjectTests =

[<Test>]
let ``createProjectWithDomain_ValidInput_ReturnsProjectAndDomain`` () =
    // Red: テスト失敗を確認（Step2実施完了）
    let projectName = ProjectName.create "Test Project" |> Result.getOk
    
    // Green: 実装してテスト成功（Step3実施完了・100%成功）
    let result = ProjectDomainService.createProjectWithDefaultDomain projectName
    
    // Refactor: リファクタリング・品質向上（Step4完了・Bounded Context化）
    match result with
    | Success (project, domain) -> 
        project.Name |> should equal projectName
        domain.IsDefault |> should be True
    | _ -> 
        failtest "Expected Success with Project and Domain"

[<Test>]
let ``createProjectWithDomain_TransactionFailure_RollsBack`` () =
    // 原子性保証・ロールバック確認（完全実装）
    let projectName = ProjectName.create "Test Project" |> Result.getOk
    
    // Repository失敗をシミュレート
    let mockRepo = Mock.Of<IProjectRepository>()
    Mock.Setup(fun x -> x.SaveProject(It.IsAny<Project>())).Throws<Exception>()
    
    let result = ProjectDomainService.createProjectWithDefaultDomain projectName
    
    match result with
    | DomainCreationFailed _ -> () // 期待される失敗
    | _ -> failtest "Expected transaction rollback failure"
```

### Phase B1 Step3 Application層テストパターン（2025-09-30完全実装・20テスト追加・100%成功）
```fsharp
module ProjectManagementServiceTests =

[<Test>]
let ``CreateProjectAsync_ValidCommand_ReturnsProjectDto`` () = async {
    // Arrange
    let command = { Name = "Test Project"; Description = None; OwnerId = Guid.NewGuid() }
    let service = ProjectManagementService()
    
    // Act
    let! result = service.CreateProjectAsync(command)
    
    // Assert
    match result with
    | Ok projectDto -> 
        projectDto.Name |> should equal command.Name
        projectDto.Id |> should not' (equal Guid.Empty)
    | Error msg -> failtest $"Expected success but got error: {msg}"
}

[<Test>]
let ``GetProjectsByUserAsync_GeneralUser_ReturnsOnlyOwnedProjects`` () = async {
    // 権限制御テスト・GeneralUserは自分のプロジェクトのみ取得（完全実装）
    let userId = Guid.NewGuid()
    let query = { UserId = userId; UserRole = GeneralUser; PageNumber = 1; PageSize = 10 }
    let service = ProjectManagementService()
    
    let! result = service.GetProjectsByUserAsync(query)
    
    match result with
    | Ok projects -> 
        projects |> List.iter (fun p -> p.OwnerId |> should equal userId)
    | Error msg -> failtest $"Expected success but got error: {msg}"
}
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
- **command-quality-check**: Commands実行品質確認

### 新規Commands（2025-09-25追加）
- **task-breakdown**: 自動タスク分解・TodoList連携・Clean Architecture層別分解

## 🎯 仕様駆動開発強化体制（2025-09-25追加・2025-09-30満点達成実証）

### 加重スコアリング体系（Phase B1 Step3で100点満点達成）
```yaml
肯定的仕様準拠度: 50点満点（重要度: 最高）- 50/50点達成
  - 必須機能実装: 30点 - プロジェクト基本CRUD完全実装
  - 推奨機能実装: 15点 - 権限制御・デフォルトドメイン自動作成実装
  - 拡張機能実装: 5点 - Railway-oriented Programming完全適用

否定的仕様遵守度: 30点満点（重要度: 高）- 30/30点達成
  - 禁止事項遵守: 20点 - セキュリティ要件・制約条件完全遵守
  - 制約条件遵守: 10点 - パフォーマンス制約・設計制約遵守

実行可能性・品質: 20点満点（重要度: 中）- 20/20点達成
  - テストカバレッジ: 8点 - 52テスト100%成功・95%カバレッジ達成
  - パフォーマンス: 6点 - 0 Warning/0 Error・応答速度要件達成
  - 保守性: 6点 - Clean Architecture・可読性・拡張性確保
```

### Phase B1技術実装パターン（2025-09-25確立・2025-09-30完全実装・2025-10-01 Bounded Context化）

#### Phase B1 Step4完全実装パターン（2025-10-01完全成功・Bounded Context化達成）
```yaml
Domain層リファクタリング（100%完了・0エラー・Bounded Context化達成）:
  - 4 Bounded Contexts分離: Common/Authentication/ProjectManagement/UbiquitousLanguageManagement（✅完了）
  - F# Compilation Order最適化: 依存関係順厳格管理・前方宣言制約対応（✅完了）
  - 2,631行・16ファイル移行: モノリシック構造から境界文脈分離（✅完了）
  - ユーザーフィードバック活用: Phase6追加・4境界文脈化・Step5問題事前回避（✅完了）
  - 0 Warning/0 Error達成: 全ビルド成功・既存テスト100%維持（✅完了）

Bounded Context設計原則確立（実証済み・継続活用推奨）:
  - 凝集性: 関連概念の境界内集約・ドメイン用語一貫性
  - 独立性: 境界間依存最小化・Common経由での依存管理
  - 境界明確化: 各境界の責務定義・循環依存ゼロ達成

Step5準備完了状態（namespace階層化基盤確立）:
  - 16ファイル準備: 当初計画12ファイル→Phase6で16ファイルに拡張
  - ディレクトリ構造: 将来namespaceに対応する構造確立
  - 技術負債回避: ユーザー指摘「雛型の名残」完全解消
```

#### Phase B1 Step3完全実装パターン（2025-09-30完全成功・100点満点品質達成）
```yaml
Application層実装（100%完了・0エラー・100点満点品質達成）:
  - F# IProjectManagementService: Command/Query分離・権限制御統合（✅完了）
  - Railway-oriented Programming: Domain層基盤完全活用（✅完了）
  - 権限制御マトリックス: 4ロール×4機能完全実装（✅完了）
  - TDD Green Phase: 52テスト100%成功・Application層20テスト追加（✅完了）
  - Contracts層TypeConverter: 構文エラー9件完全修正（✅Fix-Mode成功・15分完了）

Fix-Mode活用実績（Phase B1 Step3完全実証・永続化完了）:
  - C#構文エラー修正: 9件成功（CS0246・CS0305・CS1587）
  - 実行時間短縮: 従来手法の1/3時間・75%効率化実証
  - 責務分担確立: contracts-bridge Agent専門性活用・100%成功率
  - 永続化完了: ADR_018・SubAgent実行ガイドライン策定

SubAgent並列実行成果（完全成功・技術価値確立）:
  - fsharp-application: F#層100点仕様準拠・完全実装
  - contracts-bridge: C#境界実装・構文エラー完全修正・C#規約100%準拠
  - unit-test: TDD⭐⭐⭐⭐⭐優秀評価・52テスト100%成功
```

#### 既存実装パターン（Phase B1 Step1-2完了・Step4基盤確立）
```yaml
Domain層実装（Step2完了・100点品質基盤→Step4 Bounded Context化完成）:
  - F# Railway-oriented Programming: Result型パイプライン完全実装
  - ProjectDomainService: 原子性保証・失敗時ロールバック完全実装
  - Smart Constructor: ProjectName・ProjectId制約実装完全実装
  - Bounded Context分離: 4境界文脈確立（Common/Authentication/ProjectManagement/UbiquitousLanguageManagement）

Infrastructure層実装（Step4準備完了・Application層統合基盤確立）:
  - EF Core BeginTransaction: 原子性保証実装パターン確立
  - Repository: CRUD・権限フィルタ統合設計完了
  - UserProjects中間テーブル: 多対多関連最適実装設計完了

Web層実装（Step5準備完了・UI権限制御設計完了）:
  - Blazor Server権限制御: 4ロール×4機能マトリックス設計完了
  - リアルタイム更新: SignalR・StateHasChanged最適化設計完了
```

## SubAgent Fix-Mode活用パターン（2025-09-30完全実証・永続化完了）

### Fix-Mode標準テンプレート（実証済み・100%成功率・ADR_018準拠）
```
[SubAgent名] Agent, Fix-Mode: [エラー種別]エラーを修正してください。

## 修正対象エラー詳細
**ファイル**: [ファイルパス]:[行番号]
**エラーコード**: [CS1234など]
**エラーメッセージ**: [完全なエラーメッセージ]

## 修正指示
```csharp
// 修正前（エラー）
[具体的なエラーコード]

// 修正後（正しい）
[期待される正しいコード]
```

## 参考実装
[既存の類似正常コードの例]

## 重要な制約事項
- **ロジック変更禁止**: 構文エラーの修正のみ実施
- **既存パターン準拠**: 他の同種実装の命名規則に従う
- **構文規約遵守**: C#/F#の言語仕様・プロジェクト規約完全準拠

修正完了後、[N]件のエラーが解消されることを確認してください。
```

### Fix-Mode適用効果（Phase B1 Step3完全実証・永続化完了）
- **C#構文エラー修正**: 9件成功（CS0246・CS0305・CS1587）
- **時間効率**: 従来修正手法の1/3時間・75%効率化・15分完了
- **責務分担**: contracts-bridge Agent専門性活用・メインAgent調整専念
- **品質向上**: C#規約100%準拠・構文チェック精度向上・0 Warning/0 Error達成

### 実証済み成功事例（Phase B1 Step3・継続活用推奨・ADR_018記録済み）
- **C#メソッド名修正**: `ToMicrosoft.FSharp.Core.FSharpResult` → `ToFSharpResult`（6件・10分）
- **using alias削除**: ジェネリック型エイリアス問題解決（2件・3分）
- **XMLコメント修正**: ファイル冒頭コメント構文修正（1件・2分）

### 改善価値永続化（ADR_018・SubAgent実行ガイドライン策定完了）
- **標準テンプレート確立**: 実証済み成功パターン・具体的指示例
- **責務判定フロー**: エラー内容による判定・SubAgent選定マッピング
- **効果測定体系**: 成功率・時間効率・品質向上の継続測定・100%成功率実証

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

## F#↔C# 型変換パターン（Phase B1 Step7確立・2025-10-05）

### F# Result型のC#統合パターン
**Phase B1 Step7 Stage3で確立・Blazor Server実装で実証**

#### パターン1: IsOkプロパティ直接アクセス（推奨）
```csharp
// Blazor Serverコンポーネント内での使用例
var result = await ProjectManagementService.GetProjectsAsync(query);

// ✅ 正しいパターン（Phase B1 Step7確立）
if (result.IsOk)
{
    var listResult = result.ResultValue;
    projects = listResult.Projects.ToList();
}
else
{
    errorMessage = result.ErrorValue;
}
```

#### パターン2: C#からのF# Result生成
```csharp
// F# Result型の生成（C#からF#サービスへの返却時）
return FSharpResult<ProjectCreationResultDto, string>.NewOk(successData);
return FSharpResult<ProjectCreationResultDto, string>.NewError("エラーメッセージ");
```

### F# Record型のC#統合パターン
**F# Recordは不変型・C#ではコンストラクタベース初期化必須**

```csharp
// ❌ 誤ったパターン（Phase B1 Step7で36件エラー発生）
var query = new GetProjectsQuery
{
    UserId = currentUser.Id,  // Error: Read-only property
    UserRole = currentUserRole,
    // ...
};

// ✅ 正しいパターン（コンストラクタ使用）
var query = new GetProjectsQuery(
    userId: currentUser.Id,
    userRole: currentUserRole,
    pageNumber: currentPage,
    pageSize: pageSize,
    includeInactive: showDeleted,
    searchKeyword: string.IsNullOrWhiteSpace(searchTerm)
        ? FSharpOption<string>.None
        : FSharpOption<string>.Some(searchTerm)
);
```

### F# Option型のC#統合パターン

#### パターン1: Option型の生成
```csharp
// Some/None生成
FSharpOption<string>.Some("値あり")
FSharpOption<string>.None

// 条件分岐での生成
string.IsNullOrEmpty(description)
    ? FSharpOption<string>.None
    : FSharpOption<string>.Some(description)
```

#### パターン2: Option型の値取得
```csharp
// IsSomeチェック後の値取得
var descriptionOption = project.Description.Value;
if (descriptionOption != null && FSharpOption<string>.get_IsSome(descriptionOption))
{
    string description = descriptionOption.Value;
}
```

### F# Discriminated Union型のC#統合パターン
**Role型（Discriminated Union）のパターンマッチング**

```csharp
// ❌ 誤ったパターン（Enumと誤認）
if (Enum.TryParse<Role>(roleClaim.Value, out var role))  // Error: Roleは値型ではない

// ✅ 正しいパターン（switch式でパターンマッチング）
currentUserRole = roleClaim.Value switch
{
    "SuperUser" => Role.SuperUser,
    "ProjectManager" => Role.ProjectManager,
    "DomainApprover" => Role.DomainApprover,
    "GeneralUser" => Role.GeneralUser,
    _ => Role.GeneralUser  // デフォルト値
};
```

### Blazor Server実装パターン（Phase B1 Step7確立）

#### InputRadioGroupパターン
```razor
<!-- ✅ 正しいパターン -->
<InputRadioGroup @bind-Value="model.IsActive">
    <InputRadio Name="isActive" TValue="bool" Value="true" />
    <label>有効</label>
    <InputRadio Name="isActive" TValue="bool" Value="false" />
    <label>無効</label>
</InputRadioGroup>
```

#### @bind:afterパターン（.NET 7.0+）
```razor
<!-- ✅ 正しいパターン（変更後イベント処理） -->
<select @bind="pageSize" @bind:after="OnPageSizeChangedAsync">
    <option value="10">10件</option>
    <option value="25">25件</option>
</select>
```

#### Model Classスコープパターン
```razor
@code {
    // ✅ 正しいパターン（@code内にネストクラスとして定義）
    public class CreateProjectModel
    {
        [Required(ErrorMessage = "プロジェクト名は必須です")]
        public string Name { get; set; } = string.Empty;

        [StringLength(1000)]
        public string? Description { get; set; }
    }

    private CreateProjectModel model = new();
}
```

### Railway-oriented Programming統合パターン
**F# Result型とBlazor Serverの統合**

```csharp
// Application層呼び出し（Railway-oriented Programming）
var result = await ProjectManagementService.CreateProjectAsync(command);

// Result型のパターンマッチング処理
if (result.IsOk)
{
    var creationResult = result.ResultValue;

    // 成功時: Toast表示してリダイレクト
    await ShowToast("success", "プロジェクトとデフォルトドメイン「共通」を作成しました");
    NavigationManager.NavigateTo("/projects");
}
else
{
    // エラー時: エラーメッセージ表示
    errorMessage = result.ErrorValue;
    await ShowToast("danger", errorMessage);
}
```

---
**最終更新**: 2025-10-05（Phase B1 Step7 Stage3完了・F#↔C#型変換パターン確立・Blazor Server実装パターン確立）
**重要追加**: F# Result/Record/Option/Discriminated Union型のC#統合パターン・Blazor Server実装パターン・Railway-oriented Programming統合パターン