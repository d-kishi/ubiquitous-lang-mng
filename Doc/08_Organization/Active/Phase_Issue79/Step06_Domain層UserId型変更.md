# Step 06 組織設計・実行記録

## Step目的（Why）

**このStepで達成すべきこと**:
- F# Domain層のUserId型のみをint64からstringに変更
- GetHashCode()問題解消の第一歩（UserId型定義レベルでの対応）

**Phase全体における位置づけ**:
- **Phase全体の課題**: ID体系統一リファクタリング（GetHashCode()問題の根本解決）
- **このStepの役割**: Domain層の型定義変更（後続層修正の基盤確立）

**関連Issue**: #79
- GitHub Issue #79「ID体系統一リファクタリング」

---

## Step概要

| 項目 | 内容 |
|------|------|
| **Step名** | Step06 Domain層 UserId型変更 |
| **作業特性** | 実装・型変更 |
| **推定工数** | 30分-1時間 |
| **ゴール寄与率** | 10% |
| **開始日** | 2025-12-07 |
| **完了日** | 2025-12-07 |

---

## 必須参照ファイル確認（Step間成果物参照マトリックス準拠）

| 必須参照ドキュメント | 重点参照セクション | 活用目的 | 確認状態 |
|---------------------|-------------------|---------|---------|
| `Research/ID変換フロー図.md` | 6. InitialDataとの関連, 7. 理想的なID変換フロー | UserId型変換方針の理解 | ✅確認済 |

---

## 正しいスコープの確認

**変更対象（UserIdのみ）**:
- `UserId of int64` → `UserId of string`

**変更しない（int64維持）**:
- `ProjectId of int64` - DBがbigint、変換不要
- `DomainId of int64` - DBがbigint、変換不要
- `UbiquitousLanguageId of int64` - DBがbigint、変換不要

---

## 組織設計

### SubAgent構成

| SubAgent | 担当Task | 実行内容 |
|----------|----------|----------|
| fsharp-domain | 6-1, 6-2 | UserId型定義変更・UserId.create呼び出し修正 |

### 修正対象ファイル

#### 1. CommonTypes.fs
**パス**: `src/UbiquitousLanguageManager.Domain/Common/CommonTypes.fs`

**修正箇所（3箇所）**:
- L19: `UserId of int64` → `UserId of string`
- L25-27: `member this.Value` 戻り値型自動変更
- L30: `static member create(id: int64)` → `static member create(id: string)`

#### 2. AuthenticationEntities.fs
**パス**: `src/UbiquitousLanguageManager.Domain/Authentication/AuthenticationEntities.fs`

**修正箇所（5箇所）**:

| 行 | 修正前 | 修正後 | 用途 |
|----|--------|--------|------|
| L56 | `UserId.create 0L` | `UserId.create ""` | User.create |
| L130 | `UserId.create 0L` | `UserId.create ""` | User.createWithAuthentication |
| L428 | `UserId.create 1L` | `UserId.create "system"` | User.createSystemAdmin |
| L447 | `UserId.create 1L` | `UserId.create "system"` | createSystemAdmin - CreatedBy |
| L449 | `UserId.create 1L` | `UserId.create "system"` | createSystemAdmin - UpdatedBy |

---

## 完了基準

- [x] CommonTypes.fs: UserId型がstringに変更
- [x] CommonTypes.fs: ProjectId/DomainId/UbiquitousLanguageIdはint64のまま
- [x] AuthenticationEntities.fs: UserId.create呼び出し5箇所がstring引数に変更
- [x] Domain層ビルド成功（後続層でエラー発生は想定内）

---

## Step実行記録（随時更新）

| 日時 | 作業内容 | 状態 |
|------|----------|------|
| 2025-12-07 | Step開始処理・組織設計ファイル作成 | ✅完了 |
| 2025-12-07 | ユーザー対話・合意（スコープ・SubAgent・完了基準） | ✅完了 |
| 2025-12-07 | Phase_Summary更新（Issue#79経緯・本質・方針追記、Step7/8/9修正） | ✅完了 |
| 2025-12-07 | Step開始承認取得 | ✅完了 |
| 2025-12-07 | fsharp-domain SubAgent実行 | ✅完了 |
| 2025-12-07 | 成果物物理的存在確認 | ✅完了 |
| 2025-12-07 | Domain層ビルド確認（0 Warning, 0 Error） | ✅完了 |
| 2025-12-07 | Step終了処理 | ✅完了 |

---

## Step終了時レビュー

### 仕様準拠確認
- [x] UserId型: `int64` → `string`に変更完了
- [x] ProjectId/DomainId/UbiquitousLanguageId: `int64`維持（変更なし）
- [x] UserId.create呼び出し5箇所: string引数に修正完了

### TDD実践確認
- [x] Domain層の型変更は即時確認可能なためテスト先行不要
- [x] ビルド成功により型定義の正当性確認

### 技術負債記録
- [x] 新規技術負債なし

### Phaseゴール達成率
- [x] Phase_Summary.md「完成度マトリックス」更新
- [x] 累積達成率: Step 6完了 = Domain層 10%達成

### 成果物確認（ADR_016準拠）

**修正ファイル一覧**:

| ファイル | 修正内容 | 確認状態 |
|---------|---------|---------|
| `CommonTypes.fs` L20 | `UserId of string` | ✅物理確認済 |
| `CommonTypes.fs` L31 | `static member create(id: string)` | ✅物理確認済 |
| `AuthenticationEntities.fs` L56 | `UserId.create ""` | ✅物理確認済 |
| `AuthenticationEntities.fs` L130 | `UserId.create ""` | ✅物理確認済 |
| `AuthenticationEntities.fs` L428 | `UserId.create "system"` | ✅物理確認済 |
| `AuthenticationEntities.fs` L447 | `UserId.create "system"` | ✅物理確認済 |
| `AuthenticationEntities.fs` L449 | `UserId.create "system"` | ✅物理確認済 |

**ビルド結果**:
```
Build succeeded.
    0 Warning(s)
    0 Error(s)
```

---

**作成日**: 2025-12-07
**完了日**: 2025-12-07
**作成者**: MainAgent
