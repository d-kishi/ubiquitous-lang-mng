# Step 07 組織設計・実行記録

## Step目的（Why）

**このStepで達成すべきこと**:
- F# Application層のUserId関連「余計な処理」を削除
- Guid型フィールド → string型変更（22箇所）
- GetHashCode()変換削除（22箇所）

**Phase全体における位置づけ**:
- **Phase全体の課題**: ID体系統一リファクタリング（GetHashCode()問題の根本解決）
- **このStepの役割**: Application層の「余計な処理」削除（Domain層変更の後続対応）

**関連Issue**: #79
- GitHub Issue #79「ID体系統一リファクタリング」

---

## Step概要

| 項目 | 内容 |
|------|------|
| **Step名** | Step07 Application層対応 |
| **作業特性** | 実装・型変更・GetHashCode削除 |
| **推定工数** | 2-3時間 |
| **ゴール寄与率** | 15% |
| **開始日** | 2025-12-07 |
| **完了日** | 2025-12-07 |

---

## 必須参照ファイル確認（Step間成果物参照マトリックス準拠）

| 必須参照ドキュメント | 重点参照セクション | 活用目的 | 確認状態 |
|---------------------|-------------------|---------|---------|
| `Research/GetHashCode使用箇所一覧.md` | 2.1, 2.2（UserId関連のみ） | UserId関連箇所の特定 | ✅確認済 |

---

## 正しいスコープの確認

**変更対象（UserId関連のみ）**:
- `OwnerId: Guid` → `OwnerId: string`
- `OperatorUserId: Guid` → `OperatorUserId: string`
- `UserId: Guid` → `UserId: string`
- `NewOwnerId: Guid` → `NewOwnerId: string`
- `TargetUserId: Guid` → `TargetUserId: string`
- `RequestUserId: Guid` → `RequestUserId: string`

**変更しない（Guid維持）**:
- `ProjectId: Guid` - DBがbigint、int64への変換を維持
- `ProjectId: Guid option` - 同上

---

## 組織設計

### SubAgent構成

| SubAgent | 担当Task | 実行内容 |
|----------|----------|----------|
| fsharp-application | 7-1, 7-2 | Commands.fs/Queries.fs修正 |

### 修正対象ファイル

#### 1. Commands.fs
**パス**: `src/UbiquitousLanguageManager.Application/ProjectManagement/Commands.fs`

**Guid型フィールド → string型（12箇所）**:

| 行 | 修正前 | 修正後 | 型名 |
|----|--------|--------|------|
| L22 | `OwnerId: Guid` | `OwnerId: string` | CreateProjectCommand |
| L23 | `OperatorUserId: Guid` | `OperatorUserId: string` | CreateProjectCommand |
| L48 | `OperatorUserId: Guid` | `OperatorUserId: string` | UpdateProjectCommand |
| L63 | `OperatorUserId: Guid` | `OperatorUserId: string` | DeleteProjectCommand |
| L73 | `NewOwnerId: Guid` | `NewOwnerId: string` | ChangeProjectOwnerCommand |
| L74 | `OperatorUserId: Guid` | `OperatorUserId: string` | ChangeProjectOwnerCommand |
| L83 | `OperatorUserId: Guid` | `OperatorUserId: string` | ActivateProjectCommand |
| L114 | `OperatorUserId: Guid` | `OperatorUserId: string` | GetProjectStatisticsCommand |
| L129 | `UserId: Guid` | `UserId: string` | AddMemberToProjectCommand |
| L130 | `OperatorUserId: Guid` | `OperatorUserId: string` | AddMemberToProjectCommand |
| L150 | `UserId: Guid` | `UserId: string` | RemoveMemberFromProjectCommand |
| L151 | `OperatorUserId: Guid` | `OperatorUserId: string` | RemoveMemberFromProjectCommand |

**GetHashCode変換削除（12箇所）**:

| 行 | 修正前 | 修正後 |
|----|--------|--------|
| L39 | `UserId(int64(this.OwnerId.GetHashCode()))` | `UserId this.OwnerId` |
| L40 | `UserId(int64(this.OperatorUserId.GetHashCode()))` | `UserId this.OperatorUserId` |
| L56 | `UserId(int64(this.OperatorUserId.GetHashCode()))` | `UserId this.OperatorUserId` |
| L67 | `UserId(int64(this.OperatorUserId.GetHashCode()))` | `UserId this.OperatorUserId` |
| L77 | `UserId(int64(this.NewOwnerId.GetHashCode()))` | `UserId this.NewOwnerId` |
| L77 | `UserId(int64(this.OperatorUserId.GetHashCode()))` | `UserId this.OperatorUserId` |
| L86 | `UserId(int64(this.OperatorUserId.GetHashCode()))` | `UserId this.OperatorUserId` |
| L118 | `UserId(int64(this.OperatorUserId.GetHashCode()))` | `UserId this.OperatorUserId` |
| L139 | `UserId(int64(this.UserId.GetHashCode()))` | `UserId this.UserId` |
| L140 | `UserId(int64(this.OperatorUserId.GetHashCode()))` | `UserId this.OperatorUserId` |
| L157 | `UserId(int64(this.UserId.GetHashCode()))` | `UserId this.UserId` |
| L158 | `UserId(int64(this.OperatorUserId.GetHashCode()))` | `UserId this.OperatorUserId` |

**維持（ProjectId関連）**: L52, L67, L77, L86, L138, L156

#### 2. Queries.fs
**パス**: `src/UbiquitousLanguageManager.Application/ProjectManagement/Queries.fs`

**Guid型フィールド → string型（10箇所）**:

| 行 | 修正前 | 修正後 | 型名 |
|----|--------|--------|------|
| L20 | `UserId: Guid` | `UserId: string` | GetProjectsQuery |
| L41 | `UserId: Guid` | `UserId: string` | GetProjectDetailQuery |
| L51 | `UserId: Guid` | `UserId: string` | GetProjectUsersQuery |
| L63 | `UserId: Guid` | `UserId: string` | GetProjectMembersQuery |
| L73 | `UserId: Guid` | `UserId: string` | GetProjectDomainsQuery |
| L83 | `TargetUserId: Guid` | `TargetUserId: string` | GetUserProjectsQuery |
| L84 | `RequestUserId: Guid` | `RequestUserId: string` | GetUserProjectsQuery |
| L96 | `UserId: Guid` | `UserId: string` | SearchProjectsQuery |
| L99 | `OwnerId: Guid option` | `OwnerId: string option` | SearchProjectsQuery |
| L119 | `UserId: Guid` | `UserId: string` | GetProjectStatisticsQuery |

**GetHashCode変換削除（10箇所）**:

| 行 | 修正前 | 修正後 |
|----|--------|--------|
| L34 | `UserId(int64(this.UserId.GetHashCode()))` | `UserId this.UserId` |
| L45 | `UserId(int64(this.UserId.GetHashCode()))` | `UserId this.UserId` |
| L55 | `UserId(int64(this.UserId.GetHashCode()))` | `UserId this.UserId` |
| L67 | `UserId(int64(this.UserId.GetHashCode()))` | `UserId this.UserId` |
| L78 | `UserId(int64(this.UserId.GetHashCode()))` | `UserId this.UserId` |
| L89 | `UserId(int64(this.TargetUserId.GetHashCode()))` | `UserId this.TargetUserId` |
| L90 | `UserId(int64(this.RequestUserId.GetHashCode()))` | `UserId this.RequestUserId` |
| L112 | `UserId(int64(this.UserId.GetHashCode()))` | `UserId this.UserId` |
| L113 | `Option.map (fun guid -> UserId(int64(guid.GetHashCode())))` | `Option.map UserId` |
| L124 | `UserId(int64(this.UserId.GetHashCode()))` | `UserId this.UserId` |

**維持（ProjectId関連）**: L45, L55, L67, L78, L125

---

## 完了基準

- [x] Commands.fs: UserId関連Guid型12箇所がstringに変更
- [x] Commands.fs: UserId関連GetHashCode12箇所が削除
- [x] Commands.fs: ProjectId関連6箇所は維持
- [x] Queries.fs: UserId関連Guid型10箇所がstringに変更
- [x] Queries.fs: UserId関連GetHashCode10箇所が削除
- [x] Queries.fs: ProjectId関連5箇所は維持
- [x] Application層ビルド成功（0 Warning, 0 Error）

---

## Step実行記録（随時更新）

| 日時 | 作業内容 | 状態 |
|------|----------|------|
| 2025-12-07 | Step開始処理・組織設計ファイル作成 | ✅完了 |
| 2025-12-07 | ユーザー対話・合意（スコープ・SubAgent・完了基準） | ✅完了 |
| 2025-12-07 | Step開始承認取得 | ✅完了 |
| 2025-12-07 | fsharp-application SubAgent実行 | ✅完了 |
| 2025-12-07 | 成果物物理的存在確認 | ✅完了 |
| 2025-12-07 | Application層ビルド確認（0 Warning, 0 Error） | ✅完了 |
| 2025-12-07 | Step終了処理 | ✅完了 |

---

## Step終了時レビュー

### 仕様準拠確認
- [x] UserId関連のみ変更、ProjectId関連は維持

### TDD実践確認
- [x] Application層の型変更は即時確認可能なためテスト先行不要
- [x] ビルド成功により型定義の正当性確認

### 技術負債記録
- [x] 新規技術負債なし

### Phaseゴール達成率
- [x] Phase_Summary.md「完成度マトリックス」更新
- [x] 累積達成率: Step 7完了 = Application層 30%達成

### 成果物確認（ADR_016準拠）

**修正ファイル一覧**:

| ファイル | 修正内容 | 確認状態 |
|---------|---------|---------|
| `Commands.fs` L22-23 | `OwnerId/OperatorUserId: string` | ✅物理確認済 |
| `Commands.fs` L39-40 | `UserId this.OwnerId/OperatorUserId` | ✅物理確認済 |
| `Commands.fs` L48, L56 | `OperatorUserId: string`, `UserId this.OperatorUserId` | ✅物理確認済 |
| `Commands.fs` L63, L67 | `OperatorUserId: string`, `UserId this.OperatorUserId` | ✅物理確認済 |
| `Commands.fs` L73-74, L77 | `NewOwnerId/OperatorUserId: string` | ✅物理確認済 |
| `Commands.fs` L83, L86 | `OperatorUserId: string`, `UserId this.OperatorUserId` | ✅物理確認済 |
| `Commands.fs` L114, L118 | `OperatorUserId: string`, `UserId this.OperatorUserId` | ✅物理確認済 |
| `Commands.fs` L129-130, L139-140 | AddMember修正 | ✅物理確認済 |
| `Commands.fs` L150-151, L157-158 | RemoveMember修正 | ✅物理確認済 |
| `Queries.fs` L20, L34 | `UserId: string`, `UserId this.UserId` | ✅物理確認済 |
| `Queries.fs` L41, L45 | `UserId: string`, `UserId this.UserId` | ✅物理確認済 |
| `Queries.fs` L51, L55 | `UserId: string`, `UserId this.UserId` | ✅物理確認済 |
| `Queries.fs` L63, L67 | `UserId: string`, `UserId this.UserId` | ✅物理確認済 |
| `Queries.fs` L73, L78 | `UserId: string`, `UserId this.UserId` | ✅物理確認済 |
| `Queries.fs` L83-84, L89-90 | `TargetUserId/RequestUserId: string` | ✅物理確認済 |
| `Queries.fs` L96, L99, L112-113 | SearchProjects修正 | ✅物理確認済 |
| `Queries.fs` L119, L124 | GetProjectStatistics修正 | ✅物理確認済 |
| `UseCases.fs` L89, L201 | ChangePasswordCommand修正 | ✅物理確認済 |
| `UseCases.fs` L110, L225 | CreateUbiquitousLanguageCommand修正 | ✅物理確認済 |
| `UseCases.fs` L116, L247 | SubmitForApprovalCommand修正 | ✅物理確認済 |
| `UseCases.fs` L123, L258 | ApprovalCommand修正 | ✅物理確認済 |

**ビルド結果**:
```
Build succeeded.
    0 Warning(s)
    0 Error(s)
```

---

**作成日**: 2025-12-07
**作成者**: MainAgent
