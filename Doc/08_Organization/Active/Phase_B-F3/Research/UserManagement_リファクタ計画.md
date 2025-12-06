# ユーザー管理機能 リファクタ計画

**作成日**: 2025-11-30
**最終更新**: 2025-11-30
**対象Phase**: Phase B-F3 Step1.5 Stage2-5
**方針**: 最初から作り直すつもりで下位層から積み上げ実装

---

## 🔴 重要発見事項（2025-11-30追記）

### IUserRepository実装ファイルの誤認識を修正

| ファイル | 状態 | DI登録 |
|---------|------|--------|
| **UserRepositoryAdapter.cs** (780行) | ✅ 本番使用中 | `AddScoped<IUserRepository, UserRepositoryAdapter>` |
| UserRepository.cs (1220行) | ⚠️ レガシー・未使用 | なし |

**調査根拠**:
- Program.cs 210行目: `builder.Services.AddScoped<IUserRepository, UserRepositoryAdapter>()`
- UserRepository.csはDI登録されていない

**結論**:
- Stage2の作業対象は **UserRepositoryAdapter.cs**
- **UserRepository.csは削除する**（テストファイル修正後）

### UserRepository.cs 削除判断

**参照箇所**:
- `DependencyInjectionUnitTests.cs`: 2箇所（テスト内DI登録）→ 修正必要
- ドキュメント: 複数のMDファイル → Stage完了後に更新

**削除理由**:
1. Program.csでDI登録されていない
2. UserRepositoryAdapterが全機能を提供
3. 1220行のレガシーコードがメンテナンス負担

---

## 1. リファクタ対象範囲

### 対象
| 層 | 対象ファイル | 作業内容 | 状態 |
|---|-------------|----------|------|
| **Infrastructure層** | `UserRepository.cs` | 6メソッド修正 + DbContext追加 + リネーム | ✅ Stage2完了 |
| **Infrastructure層** | ~~旧UserRepository.cs~~ | 削除（レガシー・未使用） | ✅ 削除完了 |
| **Application層** | `UserManagementServices.fs` | 権限フィルタ・プロジェクト割り当て | Stage3予定 |
| **Web層** | `Index.razor`, `Create.razor`, `Edit.razor` | 全画面書き換え | Stage4予定 |

**注**: UserRepositoryAdapter.cs → UserRepository.cs にリネーム完了（命名一貫性のため）

### 対象外
- ログイン、ログアウト、パスワード変更機能
- DB定義変更
- Domain層、Contracts層

---

## 2. Step1.5 新Stage構成

```
Stage 1: セキュリティ問題修正 ← ✅完了
Stage 2: Infrastructure層 UserRepositoryAdapter修正 ← ✅完了（2025-11-30）
Stage 3: Application層 権限フィルタ・プロジェクト割り当て ← 次回実施
Stage 4: Web層 全画面リファクタ
Stage 5: テスト（単体/統合/E2E）
```

---

## 3. Stage 2: Infrastructure層 UserRepositoryAdapter修正

**推定時間**: 7-8時間
**SubAgent**: `csharp-infrastructure`
**対象ファイル**: `UserRepositoryAdapter.cs`（本番使用中の実装）

### 正常動作しているメソッド（変更不要）
- `GetByEmailAsync` (58-102行) - UserManager.FindByEmailAsync使用
- `GetByIdentityIdAsync` (167-209行) - Identity ID直接検索
- `GetAllActiveUsersAsync` (428-471行) - IsDeleted=false フィルタ
- `GetAllUsersAsync` (476-519行) - 論理削除除外
- `SearchUsersAsync` (548-600行) - 部分一致検索

### Task 2-1: DeleteAsync バグ修正 🔴

**現状のバグ** (411行):
```csharp
// SUCCESS時に ERROR を返している！
return FSharpResult<Unit, string>.NewError("Delete completed successfully");
```

**修正内容**:
```csharp
return FSharpResult<Unit, string>.NewOk(default(Unit));
```

**完了基準**: 論理削除成功時に `Result.Ok(unit)` を返す

### Task 2-2: GetByRoleAsync 完全実装 🔴

**現状** (525-542行): 空リスト返却

**実装内容**:
```csharp
public async Task<FSharpResult<FSharpList<User>, string>> GetByRoleAsync(Role role)
{
    var roleName = role.ToString();
    var usersInRole = await _userManager.GetUsersInRoleAsync(roleName);
    var activeUsers = usersInRole.Where(u => !u.IsDeleted).ToList();
    // F# User変換...
}
```

### Task 2-3: SaveAsync ロール同期追加 🟡

**現状** (219-276行): ユーザー属性は更新されるがロール変更は永続化されない

**追加実装**:
- ロール同期: 現在ロール取得 → 変更があれば削除・追加
- ヘルパーメソッド `ConvertRoleToString(Role role)` 追加

### Task 2-4: GetUsersByProjectIdsAsync 実装 🟡

**現状** (315-333行): 空リスト返却

**実装内容**:
- DbContext依存追加が必要
- UserProjectsテーブルをクエリしてプロジェクト所属ユーザー取得

### Task 2-5: GetProjectIdsByUserIdAsync 実装 🟡

**現状** (346-363行): 空リスト返却

**実装内容**:
- UserProjectsテーブルからユーザーの所属プロジェクトID取得

### Task 2-6: ID変換問題のドキュメント化 🟢

**現状の問題** (645-648行):
```csharp
return new Guid((int)(userId.Item % int.MaxValue), 0, 0, 0, 0, 0, 0, 0, 0, 0, 0).ToString();
```

**対応方針**:
1. `GetByIdAsync` に警告ログ追加
2. `GetByIdentityIdAsync` を推奨する旨のコメント追加
3. 根本解決は将来Phase（DB設計変更が必要）

### 前提条件: DbContext依存追加
Task 2-4, 2-5 実装のため、コンストラクタに `UbiquitousLanguageDbContext` 追加

### UserRepository.cs 削除 ✅完了
- DependencyInjectionUnitTests.cs修正後に削除
- 1220行のレガシーコードを完全削除

### Task 2-7: UserRepositoryAdapter → UserRepository リネーム ✅完了（2025-11-30追加）

**背景**:
- Stage2でUserRepository.cs（レガシー）を削除
- ProjectRepositoryとの命名一貫性のため、UserRepositoryAdapterをUserRepositoryにリネーム

**実施内容**:
1. クラス名変更: `UserRepositoryAdapter` → `UserRepository`
2. ファイル名変更: `UserRepositoryAdapter.cs` → `UserRepository.cs`
3. DI登録変更: `Program.cs`
4. テストファイル変更: `DependencyInjectionUnitTests.cs`
5. コメント更新: `TypeConverters.cs`

### 完了基準 ✅全達成
- [x] dotnet build成功（0 Error）
- [x] DeleteAsync: 成功時 `Result.Ok(unit)` を返す
- [x] GetByRoleAsync: 指定ロールのユーザーを返す
- [x] SaveAsync: ロール変更が永続化される
- [x] GetUsersByProjectIdsAsync: プロジェクトユーザーを返す
- [x] GetProjectIdsByUserIdAsync: ユーザーのプロジェクトIDを返す
- [x] 既存テスト全Pass（Domain/Application/Contracts/Infrastructure）
- [x] UserRepository.cs削除完了
- [x] UserRepositoryAdapter → UserRepository リネーム完了

---

## 4. Stage 3: Application層 権限フィルタ・プロジェクト割り当て

**推定時間**: 2-3時間
**SubAgent**: `fsharp-application` + `csharp-infrastructure`

### Task 3-1: GetUserByIdAsync権限フィルタ

**現状**: TODOコメントのまま

**実装内容**:
```fsharp
member this.GetUserByIdAsync(userId: UserId, operatorIdentityId: string) =
    task {
        let! operatorResult = userRepository.GetByIdentityIdAsync(operatorIdentityId)
        match operatorResult with
        | Error err -> return Error err
        | Ok operatorOpt ->
            match operatorOpt with
            | None -> return Error "操作者が見つかりません"
            | Some operator ->
                match operator.Role with
                | SuperUser ->
                    // SuperUser: 全ユーザー参照可能
                    let! userResult = userRepository.GetByIdAsync(userId)
                    return userResult |> Result.bind (fun opt ->
                        match opt with
                        | Some u -> Ok u
                        | None -> Error "ユーザーが見つかりません")
                | ProjectManager ->
                    // ProjectManager: 担当プロジェクトのユーザーのみ
                    let! projectIdsResult = userRepository.GetProjectIdsByUserIdAsync(operator.Id)
                    match projectIdsResult with
                    | Error err -> return Error err
                    | Ok projectIds ->
                        let! usersResult = userRepository.GetUsersByProjectIdsAsync(projectIds)
                        match usersResult with
                        | Error err -> return Error err
                        | Ok users ->
                            match users |> List.tryFind (fun u -> u.Id = userId) with
                            | Some user -> return Ok user
                            | None -> return Error "このユーザーを参照する権限がありません"
                | _ ->
                    return Error "ユーザー参照の権限がありません"
    }
```

### Task 3-2: CreateUserAsync プロジェクト割り当て

**現状**: `assignedProjectIds`パラメータが未使用

**実装内容**:
1. ユーザー作成後、`assignedProjectIds`をUserProjectsテーブルにINSERT
2. ProjectManager権限チェック（自分の担当プロジェクトのみ割り当て可能）
3. IUserRepositoryに`AssignProjectsToUserAsync`メソッド追加

### Task 3-3: UpdateUserAsync プロジェクト割り当て更新

**実装内容**:
1. 既存の割り当てを削除
2. 新しい割り当てをINSERT
3. ProjectManager権限チェック

### 完了基準
- [ ] SuperUser: 全ユーザー参照可能
- [ ] ProjectManager: 担当プロジェクトユーザーのみ参照可能
- [ ] プロジェクト割り当てがDB永続化
- [ ] dotnet build成功（0 Error）

---

## 5. Stage 4: Web層 全画面リファクタ

**推定時間**: 4-5時間
**SubAgent**: `csharp-web-ui`

### Task 4-1: Index.razor（UI設計書3.6章準拠）

**実装要件**:
| 要件 | 実装内容 |
|------|----------|
| 権限制御 | `[Authorize(Roles = "SuperUser,ProjectManager")]` |
| 表示項目 | 氏名、メールアドレス、権限レベル、所属プロジェクト |
| 検索 | 氏名部分一致 |
| フィルタ | プロジェクト別ドロップダウン |
| 削除済み表示 | チェックボックス切替 |
| ページング | 50/100/200件選択 |
| レイアウト | FullHD対応（Bootstrap 5） |
| data-testid | 全要素に付与 |

**data-testid命名規則**:
```
user-list-table
user-search-input
user-project-filter
user-show-deleted-checkbox
user-page-size-select
user-create-button
user-edit-button-{id}
user-delete-button-{id}
```

### Task 4-2: Create.razor（UI設計書3.7章準拠）

**実装要件**:
| 要件 | 実装内容 |
|------|----------|
| 権限制御 | `[Authorize(Roles = "SuperUser,ProjectManager")]` |
| 入力項目 | メールアドレス、氏名、初期パスワード、ロール、所属プロジェクト |
| ロール制限 | ProjectManager: 一般ユーザー/ドメイン承認者のみ |
| プロジェクト制限 | ProjectManager: 担当プロジェクトのみ表示 |
| レイアウト | FullHD対応 |
| data-testid | 全要素に付与 |

**data-testid命名規則**:
```
user-create-form
user-email-input
user-name-input
user-password-input
user-role-select
user-project-checkboxes
user-submit-button
user-cancel-button
```

### Task 4-3: Edit.razor（UI設計書3.8章準拠）

**実装要件**:
| 要件 | 実装内容 |
|------|----------|
| 権限制御 | `[Authorize(Roles = "SuperUser,ProjectManager")]` |
| メールアドレス | 表示のみ（変更不可） |
| ステータス変更 | アクティブ/非アクティブ |
| パスワードリセット | 管理者による新パスワード設定 |
| ロール・プロジェクト | Create.razorと同様の制限 |
| レイアウト | FullHD対応 |
| data-testid | 全要素に付与 |

**data-testid命名規則**:
```
user-edit-form
user-email-display
user-name-input
user-role-select
user-status-toggle
user-project-checkboxes
user-password-reset-button
user-password-reset-input
user-submit-button
user-cancel-button
```

### 完了基準
- [ ] UI設計書3.6-3.8章全要件満足
- [ ] 全画面data-testid付与
- [ ] FullHDレイアウト確認（1920x1080）
- [ ] dotnet build成功（0 Error）

---

## 6. Stage 5: テスト

**推定時間**: 2-3時間

### Task 5-1: 単体テスト
**SubAgent**: `unit-test`
- UserRepository各メソッドのテスト
- UserManagementApplicationServiceのテスト

### Task 5-2: 統合テスト
**SubAgent**: `integration-test`
- WebApplicationFactory使用
- 全層を通した動作確認

### Task 5-3: E2Eテスト
**SubAgent**: `e2e-test`
- Playwright Test使用
- data-testid活用
- ユーザー一覧/作成/編集フロー

### 完了基準
- [ ] 全テストPASS
- [ ] 手動動作確認完了

---

## 7. 推定時間・セッション分割

| Stage | 推定時間 | 状態 |
|-------|----------|------|
| Stage 1 | - | ✅完了 |
| Stage 2 | 3-4h | ✅完了（2025-11-30） |
| Stage 3 | 2-3h | 次回セッション |
| Stage 4 | 4-5h | 次々回 |
| Stage 5 | 2-3h | 次々回〜3回目 |

**残り合計**: 8-11時間（1-2セッション）

---

## 8. Critical Files

### 実装対象（優先度順）
1. `src/UbiquitousLanguageManager.Infrastructure/Repositories/UserRepository.cs` ← **✅ Stage2完了（リネーム後）**
2. ~~旧UserRepository.cs（レガシー）~~ ← **✅ 削除完了（Stage2で1220行削除）**
3. `src/UbiquitousLanguageManager.Application/UserManagementServices.fs` ← **Stage3対象**
4. `src/UbiquitousLanguageManager.Web/Components/Pages/Admin/Users/Index.razor` ← **Stage4対象**
5. `src/UbiquitousLanguageManager.Web/Components/Pages/Admin/Users/Create.razor` ← **Stage4対象**
6. `src/UbiquitousLanguageManager.Web/Components/Pages/Admin/Users/Edit.razor` ← **Stage4対象**

**リネーム実施**: UserRepositoryAdapter.cs → UserRepository.cs（命名一貫性のため）

### 参照必須
- `Doc/02_Design/UI設計/01_認証・ユーザー管理画面設計.md`（3.6-3.8章）
- `src/UbiquitousLanguageManager.Infrastructure/Data/Entities/ApplicationUser.cs`
- `src/UbiquitousLanguageManager.Infrastructure/Data/Entities/UserProject.cs`
- `src/UbiquitousLanguageManager.Domain/Authentication/AuthenticationEntities.fs`
- `src/UbiquitousLanguageManager.Web/Program.cs` (210行目: DI登録確認)

---

## 9. リスク・対策

| リスク | 対策 |
|--------|------|
| UserId変換複雑化 | GetByIdentityIdAsyncベースに統一、GetHashCode完全廃止 |
| DB変更禁止制約 | 既存テーブル（UserProjects等）活用、新規テーブル作成なし |
| 既存テストへの影響 | IUserRepositoryインターフェース変更最小化 |
| F#⇔C#型変換 | Contracts層の既存TypeConverter活用 |

---

## 10. プロセス遵守（ADR_016準拠）

- [ ] 各Stage開始前にユーザー承認取得
- [ ] SubAgent責務分離（MainAgent直接修正禁止）
- [ ] 各Stage完了時に成果物存在確認
- [ ] dotnet build成功確認（0 Error）

---

**計画作成完了**: 2025-11-30
