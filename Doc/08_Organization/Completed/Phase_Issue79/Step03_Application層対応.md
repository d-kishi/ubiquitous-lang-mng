# Step 3: Application層対応

## Step概要

- **Step番号**: 3/8
- **Step名**: Application層対応
- **推定工数**: 1-2時間
- **ゴール寄与率**: 15%（累積: 35%）
- **開始日**: 2025-12-07
- **完了日**: 2025-12-07
- **ユーザー承認**: ✅取得済み

---

## 目標

F# Application層のGetHashCode()を全て排除し、ID型のstring化に対応する。

---

## 修正対象ファイル

### 1. Commands.fs（18箇所）

**パス**: `src/UbiquitousLanguageManager.Application/ProjectManagement/Commands.fs`

| 行番号 | コマンド型 | 修正箇所数 |
|--------|-----------|-----------|
| 39-40 | CreateProjectCommand | 2箇所 |
| 52, 56 | UpdateProjectCommand | 2箇所 |
| 67 | DeleteProjectCommand | 2箇所 |
| 77 | ChangeProjectOwnerCommand | 3箇所 |
| 86 | ActivateProjectCommand | 2箇所 |
| 118 | GetProjectStatisticsCommand | 1箇所 |
| 138-140 | AddMemberToProjectCommand | 3箇所 |
| 156-158 | RemoveMemberFromProjectCommand | 3箇所 |

### 2. Queries.fs（15箇所）

**パス**: `src/UbiquitousLanguageManager.Application/ProjectManagement/Queries.fs`

| 行番号 | クエリ型 | 修正箇所数 |
|--------|---------|-----------|
| 34 | GetProjectsQuery | 1箇所 |
| 45 | GetProjectDetailQuery | 2箇所 |
| 55 | GetProjectUsersQuery | 2箇所 |
| 67 | GetProjectMembersQuery | 2箇所 |
| 78 | GetProjectDomainsQuery | 2箇所 |
| 89-90 | GetUserProjectsQuery | 2箇所 |
| 112-113 | SearchProjectsQuery | 2箇所（Option型含む） |
| 124-125 | GetProjectStatisticsQuery | 2箇所（Option型含む） |

### 3. UserManagementServices.fs - 修正不要

既にGetHashCode回避策が導入済み（コメントで問題認識済み）

---

## 修正パターン

### パターン1: 単純変換

```fsharp
// Before
UserId(int64(this.UserId.GetHashCode()))
ProjectId(int64(this.ProjectId.GetHashCode()))

// After
UserId(this.UserId.ToString())
ProjectId(this.ProjectId.ToString())
```

### パターン2: Option型変換

```fsharp
// Before
Option.map (fun guid -> UserId(int64(guid.GetHashCode())))

// After
Option.map (fun guid -> UserId(guid.ToString()))
```

---

## SubAgent構成

### 使用SubAgent

| Agent | 役割 | 担当Task |
|-------|------|----------|
| **fsharp-application** | Application層修正 | Task 3-1, 3-3 |

---

## 作業内容

### Task 3-1: Commands.fs修正（18箇所）

**対象**: `src/UbiquitousLanguageManager.Application/ProjectManagement/Commands.fs`

**修正箇所詳細**:

1. **CreateProjectCommand.toDomainTypes()** (Line 39-40)
   - `UserId(int64(this.OwnerId.GetHashCode()))` → `UserId(this.OwnerId.ToString())`
   - `UserId(int64(this.OperatorUserId.GetHashCode()))` → `UserId(this.OperatorUserId.ToString())`

2. **UpdateProjectCommand.toDomainTypes()** (Line 52, 56)
   - `ProjectId(int64(this.ProjectId.GetHashCode()))` → `ProjectId(this.ProjectId.ToString())`
   - `UserId(int64(this.OperatorUserId.GetHashCode()))` → `UserId(this.OperatorUserId.ToString())`

3. **DeleteProjectCommand.toDomainTypes()** (Line 67)
   - 2箇所同時修正

4. **ChangeProjectOwnerCommand.toDomainTypes()** (Line 77)
   - 3箇所同時修正

5. **ActivateProjectCommand.toDomainTypes()** (Line 86)
   - 2箇所同時修正

6. **GetProjectStatisticsCommand.toDomainTypes()** (Line 118)
   - 1箇所修正

7. **AddMemberToProjectCommand.toDomainTypes()** (Line 138-140)
   - 3箇所修正

8. **RemoveMemberFromProjectCommand.toDomainTypes()** (Line 156-158)
   - 3箇所修正

### Task 3-2: 中間ビルド確認

```bash
docker exec ubiquitous-lang-mng_devcontainer-devcontainer-1 dotnet build
```

### Task 3-3: Queries.fs修正（15箇所）

**対象**: `src/UbiquitousLanguageManager.Application/ProjectManagement/Queries.fs`

修正パターンは Commands.fs と同様。Option型変換箇所に注意。

### Task 3-4: 最終ビルド確認

```bash
docker exec ubiquitous-lang-mng_devcontainer-devcontainer-1 dotnet build
```

---

## 完了基準

- [x] Commands.fs: 18箇所のGetHashCode()排除
- [x] Queries.fs: 15箇所のGetHashCode()排除
- [x] UserManagementServices.fs: 4箇所の型不整合修正
- [x] UseCases.fs: 4箇所の型不整合修正
- [x] Application層ビルド成功（0 Warning, 0 Error）
- [x] Step 4（Infrastructure層）への引き継ぎ情報整理

---

## 進捗記録

| 日時 | 作業内容 | 状態 |
|------|----------|------|
| 2025-12-07 | Step 3開始・組織設計ファイル作成 | ✅完了 |
| 2025-12-07 | Task 3-1: Commands.fs修正（18箇所） | ✅完了 |
| 2025-12-07 | Task 3-2: Queries.fs修正（15箇所） | ✅完了 |
| 2025-12-07 | Task 3-3: UserManagementServices.fs修正（4箇所） | ✅完了 |
| 2025-12-07 | Task 3-4: UseCases.fs修正（4箇所） | ✅完了 |
| 2025-12-07 | Task 3-5: 最終ビルド確認（0 Warning, 0 Error） | ✅完了 |

---

## リスク・注意点

1. **Option型処理**: Option.map内のラムダ式変換に注意
2. **後続Step影響**: Infrastructure層（Step 4）も同様のパターン適用予定
3. **ビルドエラー**: エラー発生時は型定義（CommonTypes.fs）を再確認

---

## 参照ドキュメント

| ドキュメント | 参照セクション | 活用目的 |
|-------------|---------------|---------|
| `Research/GetHashCode使用箇所一覧.md` | 2.1, 2.2 | 修正箇所特定 |
| `Step02_Domain層ID型変更.md` | Step 3への引き継ぎ情報 | 前Step成果確認 |

---

## Step 4への引き継ぎ情報

### 修正完了内容

**修正ファイル（4ファイル・41箇所）**:

| ファイル | 修正箇所数 | 修正内容 |
|----------|-----------|----------|
| Commands.fs | 18箇所 | GetHashCode() → ToString() 変換 |
| Queries.fs | 15箇所 | GetHashCode() → ToString() 変換（Option型含む） |
| UserManagementServices.fs | 4箇所 | 型不整合修正（string ↔ int64 変換） |
| UseCases.fs | 4箇所 | ID型コンストラクタ引数のstring変換 |

### 修正パターン

**パターン1: GetHashCode → ToString**
```fsharp
// Before
UserId(int64(xxx.GetHashCode()))
// After
UserId(xxx.ToString())
```

**パターン2: C#インターフェース互換性維持**
```fsharp
// ProjectId.Valueがstringになったが、C#側がint64を期待する場合
System.Int64.Parse(projectId.Value)
```

### 未対応エラー（Step 4以降で対応）

**Infrastructure層（Step 4対象）**:
- AuthenticationService.cs: GetHashCode()使用箇所2箇所
- UserRepository.cs: GetHashCode()使用箇所1箇所

**Contracts/Web層（Step 5対象）**:
- TypeConverters.cs: DTO⇔Domain変換

**テスト層（Step 7対象）**:
- Domain.Unit.Tests内の型不整合エラー

### 注意事項

1. **C#側インターフェース**: `IUserManagementService`などはまだ`int64`を返す設計のため、F#側で`Int64.Parse()`による変換が必要な箇所がある
2. **Step 4での統一**: Infrastructure層修正時に、C#側もstring型に統一することを推奨

---

**作成日**: 2025-12-07
**完了日**: 2025-12-07
**作成者**: MainAgent
