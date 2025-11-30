# ユーザー管理機能 リファクタ計画

**作成日**: 2025-11-30
**対象Phase**: Phase B-F3 Step1.5 Stage2-5
**方針**: 最初から作り直すつもりで下位層から積み上げ実装

---

## 1. リファクタ対象範囲

### 対象
| 層 | 対象ファイル | 作業内容 |
|---|-------------|----------|
| **Infrastructure層** | `UserRepository.cs` | 全メソッド完全実装 |
| **Application層** | `UserManagementServices.fs` | 権限フィルタ・プロジェクト割り当て |
| **Web層** | `Index.razor`, `Create.razor`, `Edit.razor` | 全画面書き換え |

### 対象外
- ログイン、ログアウト、パスワード変更機能
- DB定義変更
- Domain層、Contracts層

---

## 2. Step1.5 新Stage構成

```
Stage 1: セキュリティ問題修正 ← ✅完了（変更なし）
Stage 2: Infrastructure層 UserRepository完全実装 ← 新規
Stage 3: Application層 権限フィルタ・プロジェクト割り当て ← 新規
Stage 4: Web層 全画面リファクタ ← 元Stage 2
Stage 5: テスト（単体/統合/E2E） ← 元Stage 3-4統合
```

---

## 3. Stage 2: Infrastructure層 UserRepository完全実装

**推定時間**: 3-4時間
**SubAgent**: `csharp-infrastructure`

### Task 2-1: ID変換問題の解決

**現状**:
```csharp
var entityHashId = (long)entity.Id.GetHashCode();  // ハッシュ衝突リスク
```

**対策**:
- `GetHashCode()`廃止
- `GetByIdentityIdAsync`を主軸検索メソッドに統一
- `GetByIdAsync`は`GetByIdentityIdAsync`へのラッパーとして再実装

**実装方針**:
1. ApplicationUser.Id（GUID文字列）を直接使用
2. 既存の`GetByIdentityIdAsync`（動作確認済み）を活用
3. 内部マッピング不要（Identity ID直接参照）

### Task 2-2: GetByEmailAsync完全実装

**現状**: ハードコードでダミーユーザー返却

**実装内容**:
```csharp
public async Task<FSharpResult<FSharpOption<User>, string>> GetByEmailAsync(Email email)
{
    var normalizedEmail = email.Value.ToUpperInvariant();
    var entity = await _context.Users
        .Include(u => u.Roles)
        .Include(u => u.UserProjects)
        .FirstOrDefaultAsync(u => u.NormalizedEmail == normalizedEmail);

    if (entity == null)
        return FSharpResult<FSharpOption<User>, string>.NewOk(FSharpOption<User>.None);

    var user = ToDomainUser(entity);
    return FSharpResult<FSharpOption<User>, string>.NewOk(FSharpOption<User>.Some(user));
}
```

### Task 2-3: SaveAsync完全実装

**現状**: `Task.Delay(1)`のみ、永続化なし

**実装内容**:
1. **新規ユーザー作成**:
   - `UserManager.CreateAsync()`でIdentityユーザー作成
   - `UserManager.AddToRoleAsync()`でロール割り当て
   - UserProjectsテーブルへのINSERT

2. **既存ユーザー更新**:
   - 変更検出（Name, Role, IsActive等）
   - `UserManager.UpdateAsync()`で更新
   - ロール変更時は`RemoveFromRoleAsync` + `AddToRoleAsync`

### Task 2-4: DeleteAsync実装（論理削除）

**実装内容**:
```csharp
public async Task<FSharpResult<Unit, string>> DeleteAsync(UserId id)
{
    var entity = await GetEntityByIdAsync(id);
    if (entity == null)
        return FSharpResult<Unit, string>.NewError("User not found");

    entity.IsDeleted = true;
    entity.UpdatedAt = DateTime.UtcNow;
    entity.UpdatedBy = /* 操作者ID */;

    await _context.SaveChangesAsync();
    return FSharpResult<Unit, string>.NewOk(default);
}
```

### Task 2-5: GetByRoleAsync実装

**実装内容**:
```csharp
public async Task<FSharpResult<List<User>, string>> GetByRoleAsync(Role role)
{
    var roleName = role.ToString();
    var userIds = await _context.UserRoles
        .Where(ur => ur.Role.Name == roleName)
        .Select(ur => ur.UserId)
        .ToListAsync();

    var entities = await _context.Users
        .Where(u => userIds.Contains(u.Id) && !u.IsDeleted)
        .Include(u => u.UserProjects)
        .ToListAsync();

    var users = entities.Select(ToDomainUser).ToList();
    return FSharpResult<List<User>, string>.NewOk(users);
}
```

### 完了基準
- [ ] 全メソッドがDB操作を正しく実行
- [ ] GetByEmailAsync: 既存メールで正しくユーザー取得
- [ ] SaveAsync: INSERT/UPDATE動作確認
- [ ] DeleteAsync: 論理削除動作確認
- [ ] dotnet build成功（0 Error）

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

| Stage | 推定時間 | セッション |
|-------|----------|-----------|
| Stage 2 | 3-4h | 次回セッション |
| Stage 3 | 2-3h | 次々回前半 |
| Stage 4 | 4-5h | 次々回後半〜3回目 |
| Stage 5 | 2-3h | 3回目 |

**合計**: 11-15時間（2-3セッション）

---

## 8. Critical Files

### 実装対象（優先度順）
1. `src/UbiquitousLanguageManager.Infrastructure/Repositories/UserRepository.cs`
2. `src/UbiquitousLanguageManager.Infrastructure/Repositories/IUserRepository.cs`（メソッド追加時）
3. `src/UbiquitousLanguageManager.Application/UserManagementServices.fs`
4. `src/UbiquitousLanguageManager.Web/Components/Pages/Admin/Users/Index.razor`
5. `src/UbiquitousLanguageManager.Web/Components/Pages/Admin/Users/Create.razor`
6. `src/UbiquitousLanguageManager.Web/Components/Pages/Admin/Users/Edit.razor`

### 参照必須
- `Doc/02_Design/UI設計/01_認証・ユーザー管理画面設計.md`（3.6-3.8章）
- `src/UbiquitousLanguageManager.Infrastructure/Data/Entities/ApplicationUser.cs`
- `src/UbiquitousLanguageManager.Infrastructure/Data/Entities/UserProject.cs`
- `src/UbiquitousLanguageManager.Domain/Authentication/AuthenticationEntities.fs`

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
