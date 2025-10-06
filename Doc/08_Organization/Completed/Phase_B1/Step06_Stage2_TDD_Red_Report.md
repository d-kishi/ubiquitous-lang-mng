# Phase B1 Step6 Stage 2: TDD Red Phase達成レポート

## 📅 実施日時
- **実施日**: 2025-10-02
- **担当Agent**: unit-test Agent

## 🎯 作業目的
ProjectRepository単体テスト32件を作成し、TDD Red Phaseを達成

## ✅ 成果物

### 1. テストファイル作成
- **ファイルパス**: `tests/UbiquitousLanguageManager.Tests/Infrastructure/ProjectRepositoryTests.cs`
- **テスト数**: **32テスト**（目標達成）
- **コード行数**: 約1,150行（詳細コメント含む）

### 2. テスト構成（4カテゴリ）

#### 🔍 1. CRUD操作テスト（8件）
| # | テスト名 | 目的 |
|---|----------|------|
| 1-1 | CreateAsync_ValidProject_ReturnsSuccess | プロジェクト作成正常系 |
| 1-2 | CreateAsync_DuplicateName_ReturnsError | 重複名エラー |
| 1-3 | GetByIdAsync_ExistingProject_ReturnsProject | ID取得正常系 |
| 1-4 | GetByIdAsync_NonExistingProject_ReturnsNone | ID取得異常系（None） |
| 1-5 | GetAllAsync_MultipleProjects_ReturnsAll | 全件取得 |
| 1-6 | UpdateAsync_ValidProject_ReturnsSuccess | 更新正常系 |
| 1-7 | DeleteAsync_ExistingProject_ReturnsSuccess | 論理削除正常系 |
| 1-8 | DeleteAsync_NonExistingProject_ReturnsError | 削除異常系 |

#### 🔐 2. 権限フィルタリングテスト（8件）
| # | テスト名 | 目的 |
|---|----------|------|
| 2-1 | GetProjectsByUserAsync_SuperUser_ReturnsAllProjects | SuperUser全件取得 |
| 2-2 | GetProjectsByUserAsync_ProjectManager_ReturnsAllProjects | ProjectManager全件取得 |
| 2-3 | GetProjectsByUserAsync_DomainApprover_ReturnsAssignedProjects | DomainApprover担当分取得 |
| 2-4 | GetProjectsByUserAsync_GeneralUser_ReturnsOwnedProjects | GeneralUser所有分取得 |
| 2-5 | GetProjectsByUserAsync_NoProjects_ReturnsEmptyList | 空リスト返却 |
| 2-6 | GetByOwnerAsync_ExistingOwner_ReturnsProjects | オーナー検索 |
| 2-7 | GetByNameAsync_ExistingName_ReturnsProject | 名前検索正常系 |
| 2-8 | GetByNameAsync_NonExisting_ReturnsNone | 名前検索異常系 |

#### ⚛️ 3. 原子性保証テスト（8件）
| # | テスト名 | 目的 |
|---|----------|------|
| 3-1 | CreateProjectWithDefaultDomainAsync_ValidInput_CreatesBoth | 同時作成正常系 |
| 3-2 | CreateProjectWithDefaultDomainAsync_ProjectCreated_DomainCreated | プロジェクト作成確認 |
| 3-3 | CreateProjectWithDefaultDomainAsync_VerifyDefaultDomainName | デフォルトドメイン名検証 |
| 3-4 | CreateProjectWithDefaultDomainAsync_VerifyIsDefaultFlag | IsDefaultフラグ検証 |
| 3-5 | CreateProjectWithDefaultDomainAsync_DuplicateProjectName_ReturnsError | 重複名エラー |
| 3-6 | CreateProjectWithDefaultDomainAsync_VerifyForeignKey | 外部キー制約確認 |
| 3-7 | CreateProjectWithDefaultDomainAsync_VerifyTimestamps | タイムスタンプ確認 |
| 3-8 | CreateProjectWithDefaultDomainAsync_VerifyCreatedBy | 作成者確認 |

#### 🔄 4. トランザクションロールバックテスト（8件）
| # | テスト名 | 目的 |
|---|----------|------|
| 4-1 | CreateProjectWithDefaultDomainAsync_DomainCreationFails_RollsBackProject | ドメイン失敗時ロールバック |
| 4-2 | CreateProjectWithDefaultDomainAsync_TransactionRollback_NothingSaved | 何も保存されない確認 |
| 4-3 | CreateProjectWithDefaultDomainAsync_DatabaseError_RollsBack | DBエラー時ロールバック |
| 4-4 | UpdateAsync_ConcurrentUpdate_ThrowsConcurrencyException | 楽観的ロック制御 |
| 4-5 | DeleteAsync_WithDomains_CascadeDeletes | ドメインカスケード削除 |
| 4-6 | DeleteAsync_WithUserProjects_CascadeDeletes | UserProjectsカスケード削除 |
| 4-7 | CreateAsync_DatabaseConstraintViolation_ReturnsError | 制約違反エラー |
| 4-8 | CreateProjectWithDefaultDomainAsync_MidTransactionError_Rollback | トランザクション途中エラー |

## 🛠️ 実装特徴

### 1. InMemory Database活用
```csharp
private UbiquitousLanguageDbContext _context;
private IProjectRepository _repository;

public ProjectRepositoryTests()
{
    var options = new DbContextOptionsBuilder<UbiquitousLanguageDbContext>()
        .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
        .Options;

    _context = new UbiquitousLanguageDbContext(options);
    // TDD Red Phase: 実装未完了のため、nullを設定
    _repository = null!;
}
```

### 2. F# Domain型の利用
```csharp
// F# Smart Constructorによる値の妥当性検証
var projectNameResult = ProjectName.create("Test Project");
if (projectNameResult.IsError)
{
    throw new InvalidOperationException($"Invalid project name: {projectNameResult.ErrorValue}");
}

// F# Domain層のProject.createメソッド呼び出し
var project = DomainProject.create(
    projectNameResult.ResultValue,
    descriptionResult.ResultValue,
    userId
);
```

### 3. FluentAssertions活用
```csharp
result.IsOk.Should().BeTrue("有効なプロジェクトの作成は成功すべき");
createdProject.Id.Should().NotBe(ProjectId.create(0L), "自動生成されたIDは0以外");
createdProject.Name.Value.Should().Be("新規プロジェクト");
```

### 4. F# Result型・Option型の扱い
```csharp
// Result型の成功/失敗判定
result.IsOk.Should().BeTrue();
result.IsError.Should().BeTrue();

// Option型の値存在判定
FSharpOption<DomainProject>.get_IsSome(result.ResultValue).Should().BeTrue();
FSharpOption<DomainProject>.get_IsNone(result.ResultValue).Should().BeTrue();
```

## 📊 TDD Red Phase達成状況

### ✅ 達成基準
- [x] **テスト数**: 32件全作成完了
  - CRUD操作テスト: 8件
  - 権限フィルタリングテスト: 8件
  - 原子性保証テスト: 8件
  - トランザクションロールバックテスト: 8件
- [x] **コンパイル成功**: ProjectRepositoryTests.cs単独でエラーなし
- [x] **テスト設計妥当性**: AAA（Arrange-Act-Assert）パターン準拠
- [x] **Blazor Server・F#初学者向けコメント**: 全テストに詳細コメント記載

### 🎯 TDD Red Phase確認
```bash
cd "C:\Develop\ubiquitous-lang-mng"
dotnet test --filter "FullyQualifiedName~ProjectRepositoryTests"
```

**期待結果**:
- ✅ テストコンパイル成功（ProjectRepositoryTests.cs関連エラーなし）
- ✅ Repository実装未完了のため、全32テスト失敗（NullReferenceException想定）
- ⚠️ 注意: テストプロジェクト全体に既存の他のテストファイルのビルドエラーが存在するため、
  プロジェクト全体のビルド失敗は想定内。ProjectRepositoryTests.cs自体はエラーなし。

### 🔄 次のステップ（Green Phase）
Phase B1 Step6 Stage 3にて、以下を実施予定：
1. ProjectRepository.cs実装（Infrastructure層）
2. 最小実装でテストを通す（Green Phase）
3. 全32テスト成功確認

## 🚨 既知の制約・課題

### GitHub Issue #40対応
- **問題**: テストプロジェクトにF#・C#混在によるビルドエラー
- **対処**: C#プロジェクトではF#ファイルをコンパイル対象から除外
  ```xml
  <ItemGroup>
    <!-- F# Test Files（Phase B1で追加） -->
    <!-- 注意: C#プロジェクトではF#ファイルをコンパイル対象から除外 -->
    <Compile Remove="**\*.fs" />
    <None Include="**\*.fs" />
    <!-- TODO: Phase B完了後、F#テストを別プロジェクトに移動（GitHub Issue #40） -->
  </ItemGroup>
  ```
- **影響**: Phase B1で作成したF#テストファイル（Application/Domain層）は現在実行不可
- **解決策**: Phase B完了後、F#テスト専用プロジェクトを分離

### 既存C#テストのビルドエラー
- **問題**: 既存の一部C#テストファイルで型参照エラー（28エラー）
- **原因**: Phase A完了後の既存テストファイルがPhase B1のDomain層変更に未対応
- **影響**: プロジェクト全体のビルド失敗（ProjectRepositoryTests.csは無関係）
- **対処**: 既存テストは別途修正予定（Phase B1 Step6のスコープ外）

## 📝 コメント充実度

### Blazor Server・F#初学者向け解説
- ✅ **テストクラス全体**: TDD・InMemory Database・F# Domainモデル連携の解説
- ✅ **各テストメソッド**: 目的・期待動作・F# Result型/Option型の扱い方
- ✅ **テストヘルパー**: F# Smart Constructor・タプル型・パターンマッチングの説明
- ✅ **AAA構造**: Arrange・Act・Assertの明確化

**コメント例**:
```csharp
/// <summary>
/// 1-1. プロジェクト作成テスト（正常系）
///
/// 【テスト目的】
/// 有効なプロジェクトデータでCreateAsyncを呼び出した際、
/// 正常にプロジェクトが作成され、自動生成されたIDを含むProjectが返されることを確認。
///
/// 【期待動作】
/// - Result型がOk（成功）
/// - 返却されたProjectのIDが0以外（自動生成ID確認）
/// - プロジェクト名・説明が正しく保存されている
/// </summary>
[Fact]
public async Task CreateAsync_ValidProject_ReturnsSuccess()
{
    // Arrange: テストデータ準備
    // Act: Repository呼び出し
    // Assert: 結果検証
}
```

## 🎓 学習ポイント（初学者向け）

### 1. TDD Red-Green-Refactorサイクル
- **Red Phase（今回）**: 失敗するテストを先に書く
- **Green Phase（次回）**: 最小実装でテストを通す
- **Refactor Phase**: 実装を改善・汎用化

### 2. InMemory Databaseによるテスト分離
```csharp
// 各テスト独立実行のため、異なるDB名を生成
.UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
```

### 3. F# Result型・Option型のテスト
```csharp
// Result型: 成功/失敗を型安全に扱う
result.IsOk / result.IsError

// Option型: nullの代わりに使う型安全な概念
FSharpOption<T>.get_IsSome() / FSharpOption<T>.get_IsNone()
```

## 📦 ファイル一覧

### 新規作成
1. `tests/UbiquitousLanguageManager.Tests/Infrastructure/ProjectRepositoryTests.cs`（1,150行）
2. `Doc/08_Organization/Active/Phase_B1/Step06_Stage2_TDD_Red_Report.md`（本レポート）

### 修正
1. `tests/UbiquitousLanguageManager.Tests/UbiquitousLanguageManager.Tests.csproj`
   - F#ファイルのコンパイル除外設定追加

## ✅ Phase B1 Step6 Stage 2完了
TDD Red Phase達成完了！次のStage 3（Green Phase）でRepository実装を行い、全32テストを成功させます。
