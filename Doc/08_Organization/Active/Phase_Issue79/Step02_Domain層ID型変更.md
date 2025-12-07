# Step 2: Domain層ID型変更

## Step概要

- **Step番号**: 2/8
- **Step名**: Domain層ID型変更
- **推定工数**: 1-2時間
- **ゴール寄与率**: 10%（累積: 20%）
- **開始日**: 2025-12-07
- **完了日**: 2025-12-07
- **ユーザー承認**: ✅取得済み

---

## 目標

F# Domain層のID型を`int64`から`string`に変更し、GUID文字列を直接格納できるようにする。

---

## 修正方針（ユーザー合意済み・2025-12-07）

**採用方針**: A: ID型をstring化（根本解決）

```fsharp
// Before
type UserId = UserId of int64
type ProjectId = ProjectId of int64
type DomainId = DomainId of int64
type UbiquitousLanguageId = UbiquitousLanguageId of int64

// After
type UserId = UserId of string
type ProjectId = ProjectId of string
type DomainId = DomainId of string
type UbiquitousLanguageId = UbiquitousLanguageId of string
```

**選定理由**:
- GUID（128bit）をint64（64bit）に変換不可能
- GetHashCode()の不安定性・衝突リスク排除
- ASP.NET Core IdentityのGUID文字列をそのまま使用可能
- 全層での一貫したID体系を実現

---

## 対象ファイル

### 主要修正対象

| # | ファイル | パス | 修正内容 |
|---|----------|------|----------|
| 1 | CommonTypes.fs | `src/UbiquitousLanguageManager.Domain/Common/` | 4つのID型定義変更 |
| 2 | AuthenticationEntities.fs | `src/UbiquitousLanguageManager.Domain/Authentication/` | UserId使用箇所確認 |
| 3 | ProjectEntities.fs | `src/UbiquitousLanguageManager.Domain/Projects/` | ProjectId使用箇所確認 |
| 4 | UbiquitousLanguageEntities.fs | `src/UbiquitousLanguageManager.Domain/UbiquitousLanguages/` | UbiquitousLanguageId/DomainId使用箇所確認 |
| 5 | DomainEntities.fs | `src/UbiquitousLanguageManager.Domain/Domains/` | DomainId使用箇所確認 |

### 確認対象（コンパイルエラー発生予想）

- Application層: Queries.fs, Commands.fs（Step 3で対応）
- Infrastructure層: Repository実装（Step 4で対応）
- Contracts層: TypeConverters.cs（Step 5で対応）

---

## SubAgent構成

### 使用SubAgent

| Agent | 役割 | 担当Task |
|-------|------|----------|
| **fsharp-domain** | Domain層修正 | Task 2-1: ID型定義変更 |
| **fsharp-domain** | エンティティ確認 | Task 2-2: エンティティ影響確認 |

### SubAgent実行手順

1. **fsharp-domain Agent起動**
2. **Task 2-1実行**: CommonTypes.fsのID型定義変更
3. **Task 2-2実行**: 各エンティティファイルの影響確認
4. **ビルド確認**: `dotnet build`でエラー箇所特定（Step 3以降で修正）

---

## 作業内容

### Task 2-1: ID型定義変更

**対象**: `src/UbiquitousLanguageManager.Domain/Common/CommonTypes.fs`

**修正内容**:

```fsharp
// 修正前
type UserId =
    | UserId of int64

type ProjectId =
    | ProjectId of int64

type DomainId =
    | DomainId of int64

type UbiquitousLanguageId =
    | UbiquitousLanguageId of int64

// 修正後
type UserId =
    | UserId of string

type ProjectId =
    | ProjectId of string

type DomainId =
    | DomainId of string

type UbiquitousLanguageId =
    | UbiquitousLanguageId of string
```

**ヘルパー関数の修正**:

```fsharp
// NewXxxId関数の削除または修正
// Before: UserId.NewUserId(int64 value) → UserId(value)
// After: UserId.NewUserId(string value) → UserId(value)
```

### Task 2-2: エンティティ影響確認

各エンティティファイルでID型の使用箇所を確認し、型変更による影響を特定する。

**確認ポイント**:
- ID型のパターンマッチング箇所
- ID型のコンストラクタ呼び出し箇所
- ID型の比較・演算箇所

---

## 完了基準

- [x] CommonTypes.fsのID型定義が全てstring型に変更されている
- [x] Domain層のビルドが通る（Application層以降はエラー許容）
- [x] 各エンティティファイルでの影響箇所が特定されている
- [x] Step 3（Application層対応）への引き継ぎ情報が整理されている

---

## 参照ドキュメント

| ドキュメント | 参照セクション | 活用目的 |
|-------------|---------------|---------|
| `Research/ID変換フロー図.md` | 6. ID型定義（参考） | 現状の型定義理解 |
| `Research/GetHashCode使用箇所一覧.md` | 全体 | 影響範囲の把握 |

---

## 進捗記録

| 日時 | 作業内容 | 状態 |
|------|----------|------|
| 2025-12-07 | Step 2開始・組織設計ファイル作成 | ✅完了 |
| 2025-12-07 | Task 2-1: CommonTypes.fs ID型変更（4型） | ✅完了 |
| 2025-12-07 | Task 2-2: エンティティ影響確認・修正（9箇所） | ✅完了 |
| 2025-12-07 | Domain層ビルド確認（0 Warning, 0 Error） | ✅完了 |

---

## リスク・注意事項

1. **ビルドエラー多発**: ID型変更により全層でコンパイルエラー発生（想定内）
2. **Step 3-5で対応**: Application/Infrastructure/Contracts層のエラーは後続Stepで修正
3. **ヘルパー関数**: NewXxxId関数がある場合は引数型も変更必要

---

## Step 3への引き継ぎ情報

### 修正完了内容

**修正ファイル（4ファイル・13箇所）**:

| ファイル | 修正箇所数 | 修正内容 |
|----------|-----------|----------|
| CommonTypes.fs | 4箇所 | 4つのID型定義（UserId/ProjectId/DomainId/UbiquitousLanguageId）をint64→stringに変更 |
| AuthenticationEntities.fs | 5箇所 | User.create等のID初期化をstring対応 |
| ProjectEntities.fs | 3箇所 | Project.create等でGuid.NewGuid().ToString()使用 |
| UbiquitousLanguageEntities.fs | 1箇所 | DraftUbiquitousLanguage.createのID初期化 |

### ID生成方式の変更

- **変更前**: `DateTime.UtcNow.Ticks + Random値`による`int64`生成
- **変更後**: `System.Guid.NewGuid().ToString()`によるGUID文字列生成

### 未対応エラー（Step 3以降で対応）

**Application層（Step 3対象）**:
- `Queries.fs`: GetHashCode()使用箇所13箇所
- `Commands.fs`: GetHashCode()使用箇所14箇所

**Infrastructure層（Step 4対象）**:
- `AuthenticationService.cs`: GetHashCode()使用箇所2箇所
- `UserRepository.cs`: GetHashCode()使用箇所1箇所

**Tests層（Step 7対象）**:
- Domain.Unit.Tests内のエラー

---

**作成日**: 2025-12-07
**完了日**: 2025-12-07
**作成者**: MainAgent
