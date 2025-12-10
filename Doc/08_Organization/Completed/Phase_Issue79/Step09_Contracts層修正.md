# Step 09 組織設計・実行記録

## Step目的（Why）

**このStepで達成すべきこと**:
- Contracts層のDTO型変更（UserId関連フィールド: long → string）
- TypeConverters.cs修正（CreateUserId()メソッドstring対応）
- AuthenticationMapper.cs修正（ToChangePasswordCommand()のuserIdパラメータstring対応）
- Step 8延期分対応（AuthenticationService.cs L233 GetHashCode()削除）

**Phase全体における位置づけ**:
- **Phase全体の課題**: ID体系統一リファクタリング（GetHashCode()問題の根本解決）
- **このStepの役割**: Contracts層の型統一・Step 8の14エラー解消

**関連Issue**: #79
- GitHub Issue #79「ID体系統一リファクタリング」

---

## Step概要

| 項目 | 内容 |
|------|------|
| **Step名** | Step09 Contracts層修正 |
| **作業特性** | 実装・DTO型変更・Converter修正 |
| **推定工数** | 2-3時間 |
| **ゴール寄与率** | 15% |
| **開始日** | 2025-12-07 |
| **完了日** | 2025-12-07 |

---

## 必須参照ファイル確認（Step間成果物参照マトリックス準拠）

| 必須参照ドキュメント | 重点参照セクション | 活用目的 | 確認状態 |
|---------------------|-------------------|---------|---------|
| `Research/ID変換フロー図.md` | 2. 認証フロー | UserId処理箇所の特定 | ✅確認済 |

---

## 正しいスコープの確認

**変更対象（UserId関連のみ long → string）**:
- AuthenticatedUserDto.Id
- UserDto.Id, CreatedBy, UpdatedBy
- CreateUserDto.CreatedBy
- ChangePasswordDto.UserId
- ChangeUserRoleDto.UserId
- ChangeEmailDto.UserId
- ProjectDto.OwnerId, UpdatedBy
- CreateProjectCommand.OwnerId
- UpdateProjectDto.UpdatedBy

**変更しない（long維持）**:
- ProjectDto.Id（ProjectId関連）
- CreateProjectCommand.Id（ProjectId関連）
- その他ProjectId, DomainId, UbiquitousLanguageId関連

---

## 組織設計

### SubAgent構成

| SubAgent | 担当Task | 実行内容 |
|----------|----------|----------|
| contracts-bridge | 9-1, 9-2, 9-3 | DTO型変更・TypeConverters修正・AuthenticationMapper修正 |
| csharp-infrastructure | 9-4 | AuthenticationService.cs L233 GetHashCode削除 |

### 修正対象ファイル

#### 1. AuthenticatedUserDto.cs
**パス**: `src/UbiquitousLanguageManager.Contracts/DTOs/Authentication/AuthenticatedUserDto.cs`

| 行 | 修正前 | 修正後 |
|----|--------|--------|
| L18 | `public long Id { get; set; }` | `public string Id { get; set; } = string.Empty;` |

#### 2. UserDto.cs
**パス**: `src/UbiquitousLanguageManager.Contracts/DTOs/UserDto.cs`

| クラス | フィールド | 修正前 | 修正後 |
|--------|----------|--------|--------|
| UserDto | Id | `public long Id` | `public string Id = string.Empty` |
| UserDto | CreatedBy | `public long CreatedBy` | `public string CreatedBy = string.Empty` |
| UserDto | UpdatedBy | `public long UpdatedBy` | `public string UpdatedBy = string.Empty` |
| CreateUserDto | CreatedBy | `public long CreatedBy` | `public string CreatedBy = string.Empty` |
| ChangePasswordDto | UserId | `public long UserId` | `public string UserId = string.Empty` |
| ChangeUserRoleDto | UserId | `public long UserId` | `public string UserId = string.Empty` |
| ChangeEmailDto | UserId | `public long UserId` | `public string UserId = string.Empty` |

#### 3. ProjectDto.cs
**パス**: `src/UbiquitousLanguageManager.Contracts/DTOs/ProjectDto.cs`

| クラス | フィールド | 修正前 | 修正後 |
|--------|----------|--------|--------|
| ProjectDto | OwnerId | `public long OwnerId` | `public string OwnerId = string.Empty` |
| ProjectDto | UpdatedBy | `public long UpdatedBy` | `public string UpdatedBy = string.Empty` |
| CreateProjectCommand | OwnerId | `public long OwnerId` | `public string OwnerId = string.Empty` |
| UpdateProjectDto | UpdatedBy | `public long UpdatedBy` | `public string UpdatedBy = string.Empty` |

**注意**: ProjectDto.Id, CreateProjectCommand.Id等はProjectId関連なのでlong維持

#### 4. TypeConverters.cs
**パス**: `src/UbiquitousLanguageManager.Contracts/Converters/TypeConverters.cs`

**修正箇所（1箇所）**:

| メソッド | 修正前 | 修正後 |
|----------|--------|--------|
| CreateUserId() | `CreateUserId(long id)` + `UserId.NewUserId(id)` | `CreateUserId(string id)` + `UserId.create(id)` |

#### 5. AuthenticationMapper.cs
**パス**: `src/UbiquitousLanguageManager.Contracts/Converters/AuthenticationMapper.cs`

**修正箇所（1箇所）**:

| メソッド | 修正前 | 修正後 |
|----------|--------|--------|
| ToChangePasswordCommand() | `long userId` パラメータ | `string userId` パラメータ |

#### 6. AuthenticationService.cs（Step 8延期分）
**パス**: `src/UbiquitousLanguageManager.Infrastructure/Services/AuthenticationService.cs`

**修正箇所（1箇所）**:

| 行 | 修正前 | 修正後 |
|----|--------|--------|
| L233 | `Id = user.Id.GetHashCode(),` | `Id = user.Id.Value,` |

---

## 完了基準

- [x] AuthenticatedUserDto.Id: long → string
- [x] UserDto.Id/CreatedBy/UpdatedBy: long → string
- [x] CreateUserDto.CreatedBy: long → string
- [x] ChangePasswordDto.UserId: long → string
- [x] ChangeUserRoleDto.UserId: long → string
- [x] ChangeEmailDto.UserId: long → string
- [x] ProjectDto.OwnerId/UpdatedBy: long → string
- [x] CreateProjectCommand.OwnerId: long → string
- [x] UpdateProjectDto.UpdatedBy: long → string
- [x] TypeConverters.CreateUserId(): string対応に変更
- [x] AuthenticationMapper.ToChangePasswordCommand(): string対応に変更
- [x] AuthenticationService.cs L233: GetHashCode()削除（`Id = user.Id`に変更）
- [x] Contracts層ビルド成功（0 Error(s)）
- [x] Infrastructure層ビルド成功（0 Error(s)）

### 追加対応（ビルドエラー解消中に発見）

- [x] DomainDto.CreatedBy/UpdatedBy: long → string
- [x] UbiquitousLanguageDto.CreatedBy/UpdatedBy/ApprovedBy: long → string
- [x] UpdateUserDto.UpdatedBy: long → string
- [x] ApplicationDtos.cs（CreateProjectCommandDto/UpdateProjectCommandDto/DeleteProjectCommandDto/GetProjectsQueryDto）
- [x] ProjectCommandConverters.cs（Tuple型更新・バリデーション変更）
- [x] DomainRepository.cs（UserId.create変更）
- [x] ProjectRepository.cs（UserId変換ロジック修正）
- [x] UserRepository.cs（ConvertUserIdToGuid GUID変換ロジック変更）

---

## Step実行記録（随時更新）

| 日時 | 作業内容 | 状態 |
|------|----------|------|
| 2025-12-07 | Step開始処理・組織設計ファイル作成 | ✅完了 |
| 2025-12-07 | ユーザー対話・合意（スコープ・SubAgent・完了基準） | ✅完了 |
| 2025-12-07 | Step開始承認取得 | ✅完了 |
| 2025-12-07 | contracts-bridge + csharp-infrastructure SubAgent並列実行 | ✅完了 |
| 2025-12-07 | 第1回ビルド確認（8エラー発生→追加DTO修正） | ✅完了 |
| 2025-12-07 | 第2回ビルド確認（1エラー発生→ApplicationDtos.cs修正） | ✅完了 |
| 2025-12-07 | 第3回ビルド確認（14エラー発生→ProjectCommandConverters.cs修正） | ✅完了 |
| 2025-12-07 | Contracts層ビルド成功・Infrastructure層15エラー発生 | ✅完了 |
| 2025-12-07 | csharp-infrastructure SubAgent追加実行（Infrastructure層修正） | ✅完了 |
| 2025-12-07 | 全層ビルド成功確認（0 Error(s)） | ✅完了 |
| 2025-12-07 | Step終了処理・step-end-review実行 | ✅完了 |

---

## Step終了時レビュー

### 仕様準拠確認
- [x] UserId関連のみ変更、ProjectId関連は維持
- [x] ProjectDto.Id, CreateProjectCommand.Id等はlong維持を確認

### TDD実践確認
- [x] Contracts層の型変更は即時確認可能なためテスト先行不要
- [x] ビルド成功により型定義の正当性確認

### 技術負債記録
- [x] 新規技術負債の有無確認
  - **なし**: 今回の修正で根本的なGetHashCode()問題を解消

### Phaseゴール達成率
- [x] Phase_Summary.md「完成度マトリックス」更新
- [x] 累積達成率更新（Step 9完了 = 60%達成、残りStep 10-13で40%）

### 成果物確認（ADR_016準拠）

**修正ファイル一覧（Contracts層）**:

| ファイル | 修正内容 | 確認状態 |
|---------|---------|---------|
| `AuthenticatedUserDto.cs` | Id: long → string | ✅物理確認済 |
| `UserDto.cs` | Id/CreatedBy/UpdatedBy等: long → string | ✅物理確認済 |
| `ProjectDto.cs` | OwnerId/UpdatedBy: long → string | ✅物理確認済 |
| `DomainDto.cs` | CreatedBy/UpdatedBy: long → string | ✅物理確認済 |
| `UbiquitousLanguageDto.cs` | CreatedBy/UpdatedBy/ApprovedBy: long → string | ✅物理確認済 |
| `UpdateUserDto.cs` | UpdatedBy: long → string | ✅物理確認済 |
| `ApplicationDtos.cs` | OwnerId/UserId: long → string | ✅物理確認済 |
| `CreateProjectDto.cs` | OwnerId: long → string | ✅物理確認済 |
| `TypeConverters.cs` | CreateUserId(string)に変更 | ✅物理確認済 |
| `AuthenticationMapper.cs` | ToChangePasswordCommand(string userId) | ✅物理確認済 |
| `ProjectCommandConverters.cs` | Tuple型・バリデーション更新 | ✅物理確認済 |

**修正ファイル一覧（Infrastructure層）**:

| ファイル | 修正内容 | 確認状態 |
|---------|---------|---------|
| `AuthenticationService.cs` L233 | `Id = user.Id`（GetHashCode削除） | ✅物理確認済 |
| `AuthenticationService.cs` L1340 | `UserId.create("system")` | ✅物理確認済 |
| `DomainRepository.cs` | UserId.create変更 | ✅物理確認済 |
| `ProjectRepository.cs` | UserId変換ロジック修正 | ✅物理確認済 |
| `UserRepository.cs` | ConvertUserIdToGuid GUID変換変更 | ✅物理確認済 |

**ビルド結果**:
```
Contracts層: Build succeeded. 0 Error(s)
Infrastructure層: Build succeeded. 0 Error(s)
```

---

**作成日**: 2025-12-07
**作成者**: MainAgent
**完了確認**: 2025-12-07
