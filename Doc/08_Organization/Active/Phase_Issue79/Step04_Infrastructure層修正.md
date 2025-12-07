# Step 4+5: Infrastructure層修正 + Contracts/Web層統合

## Step概要

- **Step番号**: 4+5/8（統合実施）
- **Step名**: Infrastructure層修正 + Contracts/Web層統合
- **推定工数**: 2-3時間 → 実績: 約4時間
- **ゴール寄与率**: 35%（累積: 55%）
- **開始日**: 2025-12-07
- **完了日**: 2025-12-07
- **ユーザー承認**: ✅取得済み（Step 4+5統合を承認）

---

## 目標

C# Infrastructure層のGetHashCode()を全て排除し、ID型のstring化に対応する。
当初Step 5で計画していたContracts/Web層の修正も、依存関係により統合して実施。

---

## 修正ファイル一覧（実績）

### Contracts層（5ファイル）

| ファイル | 修正内容 |
|----------|----------|
| AuthenticatedUserDto.cs | Id: long → string |
| TypeConverters.cs | CreateUserId/ProjectId/DomainId: long → string |
| ProjectCommandConverters.cs | ToFSharp/FromFSharp変換: long → string |
| ProjectDto.cs, UserDto.cs, DomainDto.cs, 他 | 全DTO ID型: long → string |

### Infrastructure層（5ファイル）

| ファイル | 修正内容 |
|----------|----------|
| AuthenticationService.cs | L233, L1332: GetHashCode()排除 |
| UserRepository.cs | L1530: GetHashCode()排除、インターフェース実装修正 |
| ProjectRepository.cs | Entity⇔Domain変換: long.ToString() / long.Parse() |
| DomainRepository.cs | Entity⇔Domain変換: long.ToString() / long.Parse() |

### Application層（2ファイル）

| ファイル | 修正内容 |
|----------|----------|
| UserManagementServices.fs | assignedProjectIds: int64 list → string list |
| IUserManagementService.fs | インターフェース型定義変更 |

### Web層（7ファイル）

| ファイル | 修正内容 |
|----------|----------|
| BlazorAuthenticationService.cs | GetCurrentDomainUserId: long? → string? |
| CustomAuthenticationStateProvider.cs | GetCurrentDomainUserId: long? → string? |
| Index.razor (Admin/Users) | Dictionary<long, string> → Dictionary<string, string> |
| Create.razor, Edit.razor (Admin/Users) | AssignedProjectIds: List<long> → List<string> |
| ProjectList.razor, ProjectEdit.razor | ProjectDto変換処理修正 |

---

## 完了基準

- [x] AuthenticatedUserDto.Idがstring型に変更
- [x] AuthenticationService.cs L233のGetHashCode()排除
- [x] AuthenticationService.cs L1332のGetHashCode()排除
- [x] UserRepository.cs L1530のGetHashCode()排除
- [x] TypeConverters.csのID変換ロジック修正
- [x] 全DTO（ProjectDto, UserDto, DomainDto等）のID型変更
- [x] Repository層のEntity⇔Domain変換処理修正
- [x] Web層のBlazorコンポーネント修正
- [x] 本番コードビルド成功（0 Warning, 0 Error）
- [x] Step 6/7への引き継ぎ情報整理

---

## 進捗記録

| 日時 | 作業内容 | 状態 |
|------|----------|------|
| 2025-12-07 | Step 4開始・組織設計ファイル作成 | ✅完了 |
| 2025-12-07 | Task 4-1: AuthenticatedUserDto.Id型変更 | ✅完了 |
| 2025-12-07 | Task 4-2〜4-5: GetHashCode排除 | ✅完了 |
| 2025-12-07 | ビルドエラー発覚・Step 4+5統合決定 | ✅承認済 |
| 2025-12-07 | Task 4-6: TypeConverters.cs修正 | ✅完了 |
| 2025-12-07 | Task 4-7: DTO型変更（全DTO） | ✅完了 |
| 2025-12-07 | Task 4-8: ProjectCommandConverters修正 | ✅完了 |
| 2025-12-07 | Task 4-9: Repository層追加修正 | ✅完了 |
| 2025-12-07 | Task 4-10: Web層修正 | ✅完了 |
| 2025-12-07 | Task 4-11: F# Application層修正 | ✅完了 |
| 2025-12-07 | 本番コードビルド成功確認 | ✅完了 |

---

## 使用SubAgent

| Agent | 担当Task | 実行内容 |
|-------|----------|----------|
| contracts-bridge | 4-1, 4-2, 4-6, 4-7, 4-8 | DTO型変更・TypeConverters修正 |
| csharp-infrastructure | 4-3〜4-5, 4-9 | GetHashCode排除・Repository修正 |
| csharp-web-ui | 4-10 | Blazorコンポーネント修正 |
| fsharp-application | 4-11 | UserManagementServices修正 |

---

## Step 6/7への引き継ぎ情報

### 修正完了内容

**本番コード（src/）**: ✅ビルド成功（0 Warning, 0 Error）

| 層 | 修正ファイル数 | 状態 |
|----|---------------|------|
| Domain層 | 4ファイル | ✅Step 2完了 |
| Application層 | 6ファイル | ✅Step 3+4完了 |
| Contracts層 | 10ファイル | ✅Step 4+5完了 |
| Infrastructure層 | 5ファイル | ✅Step 4+5完了 |
| Web層 | 7ファイル | ✅Step 4+5完了 |

### 残作業（Step 6/7対象）

**Step 6: InitialData対応**
- 02_initial_data.sqlのユーザーID形式確認
- 既存データのマイグレーション検討

**Step 7: テストコード修正**
- Domain.Unit.Tests: F#テスト（~20箇所）
- Contracts.Unit.Tests: C#テスト（~25箇所）
- Application.Unit.Tests: F#テスト（~10箇所）
- Infrastructure.Unit.Tests: C#テスト（~5箇所）

---

## 技術的決定事項

### Entity層のID型維持

- **決定**: Entity層（DBテーブル対応）のIDはlong型のまま維持
- **理由**: DBスキーマ変更・マイグレーションを避けるため
- **影響**: Repository層でEntity⇔Domain変換時にlong.ToString()/long.Parse()使用

### 変換パターン

```csharp
// Entity → Domain変換
var projectId = ProjectId.NewProjectId(entity.ProjectId.ToString());

// Domain → Entity変換（比較時）
entity.ProjectId.ToString() == projectId.Item

// Domain → Entity変換（代入時）
entity.ProjectId = long.Parse(domain.ProjectId.Value);
```

---

## 参照ドキュメント

| ドキュメント | 参照セクション | 活用目的 |
|-------------|---------------|---------|
| `Research/GetHashCode使用箇所一覧.md` | 1.1, 1.2 | 修正箇所特定 |
| `Research/ID変換フロー図.md` | 2. 認証フロー | 修正影響範囲把握 |
| `Step03_Application層対応.md` | Step 4への引き継ぎ情報 | 前Step成果確認 |

---

**作成日**: 2025-12-07
**完了日**: 2025-12-07
**作成者**: MainAgent
