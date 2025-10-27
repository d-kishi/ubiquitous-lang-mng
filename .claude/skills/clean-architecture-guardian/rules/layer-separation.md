# Clean Architecture レイヤー分離原則

## 概要

F# + C# Clean Architecture実装における、レイヤー責務分離と依存関係の原則。ADR_010から抽出。

## 言語別責務分離

### F#の責務（Domain/Application層）

**Domain層**:
- ドメインロジック
- 型定義（Value Objects, Entities, Aggregates）
- ビジネスルール
- 計算処理
- Smart Constructor実装
- Specification Pattern

**Application層**:
- ユースケース実装（IService定義）
- Command/Query分離
- Railway-oriented Programming
- 権限制御ロジック
- トランザクション境界定義

**特性**:
- ✅ **純粋関数**: 副作用排除・参照透明性
- ✅ **不変データ**: Record型・判別共用体
- ✅ **型安全**: Result型・Option型によるエラーハンドリング

### C#の責務（Infrastructure/Web層）

**Infrastructure層**:
- データベースアクセス（Entity Framework Core）
- Repository実装
- 外部システム統合
- I/O処理
- Transaction制御

**Web層（Blazor Server）**:
- UIコンポーネント
- ユーザー入力処理
- 画面遷移
- SignalRリアルタイム通信
- 認証・認可UI

**Contracts層（F#↔C#境界）**:
- DTOs（Data Transfer Objects）
- TypeConverter実装
- F#↔C#型変換

**特性**:
- ✅ **副作用カプセル化**: I/O・DB操作をInfrastructure層に集約
- ✅ **UI責務**: ユーザーインタラクションのみ
- ✅ **境界変換**: F#型とC#型の適切な変換

## 依存関係の原則

### 許可される依存方向

```
Web (C#)
  ↓
Contracts (C#)
  ↓
Application (F#)
  ↓
Domain (F#)
  ↑
Infrastructure (C#)  ← 依存関係逆転（DIP）
```

### 具体的な依存ルール

#### ✅ 許可される依存

1. **C# → F#**: Application層・UI層からDomain層呼び出し
   ```csharp
   // Web層からApplication層呼び出し（許可）
   using UbiquitousLanguageManager.Application.ProjectManagement;
   var result = await projectService.CreateProjectAsync(command);
   ```

2. **Infrastructure → Domain**: Repository実装がDomain型を使用
   ```csharp
   // Infrastructure層がDomain型を参照（許可）
   using UbiquitousLanguageManager.Domain.ProjectManagement;
   public async Task<Project> GetByIdAsync(ProjectId id) { ... }
   ```

3. **レイヤー間のInterface**: Domain層でInterface定義、Infrastructure層で実装
   ```fsharp
   // Domain層（Interface定義）
   type IProjectRepository =
       abstract member GetByIdAsync: ProjectId -> Async<Project option>
   ```
   ```csharp
   // Infrastructure層（実装）
   public class ProjectRepository : IProjectRepository { ... }
   ```

#### ❌ 禁止される依存

1. **F# → C#**: Domain層・Application層からInfrastructure層への直接参照
   ```fsharp
   // ❌ 誤り: Domain層からInfrastructure層参照
   open UbiquitousLanguageManager.Infrastructure.Repositories
   ```

2. **下位層 → 上位層**: InfrastructureからWebへの参照
   ```csharp
   // ❌ 誤り: Infrastructure層からWeb層参照
   using UbiquitousLanguageManager.Web.Components;
   ```

3. **循環依存**: いかなる層間での循環参照も禁止
   ```
   ❌ 誤り: Application ⇄ Infrastructure
   ```

## レイヤー別責務詳細

### Domain層（F#）

**責務**:
- ビジネスルールの定義
- エンティティ・値オブジェクトの定義
- ドメインサービスの実装
- Specification Patternの実装

**禁止事項**:
- ❌ データベースアクセス
- ❌ UI処理
- ❌ 外部システム呼び出し
- ❌ I/O処理

**実装例**:
```fsharp
// Domain層: 純粋なビジネスロジック
module ProjectDomainService =
    let createProjectWithDefaultDomain (projectName: ProjectName) =
        projectName
        |> Project.create
        |> Result.bind (fun project ->
            Domain.createDefault project.Id
            |> Result.map (fun domain -> project, domain))
```

### Application層（F#）

**責務**:
- ユースケースの調整
- トランザクション境界の定義
- 権限制御ロジック
- Command/Queryの処理

**禁止事項**:
- ❌ 具体的なデータベース操作
- ❌ UI実装
- ❌ ビジネスルール実装（Domain層の責務）

**実装例**:
```fsharp
// Application層: ユースケース調整
let createProjectAsync (command: CreateProjectCommand) = async {
    // 権限チェック
    do! checkPermission command.UserId CreateProject

    // Domain層呼び出し
    let! result = ProjectDomainService.createProjectWithDefaultDomain command.Name

    // Repository呼び出し（Interface経由）
    return! Repository.saveAsync result
}
```

### Infrastructure層（C#）

**責務**:
- データベース操作実装
- Repository実装
- 外部サービス統合
- Transaction制御

**禁止事項**:
- ❌ ビジネスルール実装
- ❌ UI処理

**実装例**:
```csharp
// Infrastructure層: Repository実装
public class ProjectRepository : IProjectRepository
{
    private readonly AppDbContext _context;

    public async Task<Project> GetByIdAsync(ProjectId id)
    {
        var entity = await _context.Projects.FindAsync(id.Value);
        return entity?.ToDomainModel();
    }
}
```

### Web層（Blazor Server, C#）

**責務**:
- ユーザーインタラクション
- 画面表示
- フォーム処理
- ナビゲーション

**禁止事項**:
- ❌ ビジネスルール実装
- ❌ データベース直接アクセス

**実装例**:
```csharp
// Web層: UIコンポーネント
protected override async Task OnInitializedAsync()
{
    // Application層呼び出しのみ
    var result = await ProjectService.GetProjectsAsync(query);

    if (result.IsOk)
    {
        projects = result.ResultValue.ToList();
    }
}
```

## 循環依存の検出と解決

### 検出方法

1. **ビルドエラー確認**:
   ```
   Error: Circular dependency detected between projects
   ```

2. **プロジェクト参照確認**:
   ```bash
   # すべての.csproj/.fsprojファイルでProjectReferenceを確認
   grep -r "ProjectReference" src/
   ```

3. **依存関係グラフ作成**:
   ```
   Domain (F#)
     ↑
   Application (F#)
     ↑
   Infrastructure (C#) → Domain (許可)
     ↑
   Web (C#) → Application (許可)
   ```

### 解決方法

1. **依存関係逆転の原則（DIP）適用**:
   ```fsharp
   // Domain層でInterface定義
   type IProjectRepository =
       abstract member GetByIdAsync: ProjectId -> Async<Project option>
   ```

2. **中間層導入**（Contracts層）:
   ```
   Web → Contracts → Application → Domain
              ↑
        Infrastructure
   ```

3. **イベント駆動アーキテクチャ**（将来拡張）:
   ```
   層間をイベントで疎結合化
   ```

## Phase B1での実証データ

### 達成した品質

- **循環依存**: 0件
- **不正な依存**: 0件
- **レイヤー責務違反**: 0件
- **Clean Architecture準拠度**: 97/100点

### 確立したパターン

1. **Domain層純粋性**: 100%純粋関数・副作用ゼロ
2. **DIP適用**: すべてのRepository・外部サービスでInterface使用
3. **Contracts層活用**: F#↔C#境界の明確化

## 検証チェックリスト

### Step開始時（必須）

- [ ] プロジェクト参照確認（循環依存なし）
- [ ] namespace構造確認（レイヤー別分離）
- [ ] using/open文確認（不正な依存なし）

### Phase完了時（必須）

- [ ] 全層レイヤー責務遵守確認
- [ ] 循環依存ゼロ確認
- [ ] Clean Architecture 97点以上確認
- [ ] 0 Warning/0 Error確認

## 参考情報

- **Clean Architecture**: Robert C. Martin著
- **Domain Modeling Made Functional**: Scott Wlaschin著
- **Phase B1実装記録**: `Doc/08_Organization/Completed/Phase_B1/Phase_Summary.md`
