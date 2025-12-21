# 実装系Agent選択パターン（5Agent）

## 概要

実装系Agentは、Clean Architectureの5層（Domain/Application/Contracts/Infrastructure/Web）に対応する5つのSubAgentで構成されます。**レイヤー別責務境界が最も重要**であり、責務違反は厳禁です。

## 目次

- [レイヤー別責務境界](#-critical-レイヤー別責務境界厳格遵守)
- [Agent一覧](#agent一覧)
- [実装系Agent組み合わせパターン](#実装系agent組み合わせパターン)
- [並列実行判断](#並列実行判断)
- [選択チェックリスト](#選択チェックリスト)

---

## 🔴 CRITICAL: レイヤー別責務境界（厳格遵守）

### 責務境界違反の重大性

**違反例**（Phase B1で検出・修正）:
- ❌ fsharp-domain AgentがContracts層を修正
- ❌ contracts-bridge AgentがDomain層を修正
- ❌ csharp-web-ui AgentがInfrastructure層を修正

**違反の影響**:
- レイヤー依存方向違反
- Clean Architecture崩壊
- 技術負債増加
- テスト失敗（参照関係エラー）

**遵守原則**:
```yaml
絶対原則:
  - 1 Agent = 1 Layer（厳格1対1対応）
  - 他層の実装修正禁止（読み取り専用参照のみ許可）
  - tests/配下への参照禁止（実装系Agentの責務外）
```

---

## Agent一覧

### 1. fsharp-domain Agent

**責務**: F#ドメインモデル・ビジネスロジック実装

**実行範囲**: `src/UbiquitousLanguageManager.Domain/` 配下**のみ**

**主要ツール**:
- Read
- Write
- Edit
- MultiEdit
- Grep
- Glob
- Bash (dotnet build等)

**適用場面**:
```yaml
ValueObjects実装:
  - Email, Password, ProjectName等
  - バリデーションロジック実装
  - F#型制約活用（private constructor等）

Entities実装:
  - User, Project, UbiquitousLanguageTerm等
  - 集約ルート定義
  - ドメインイベント実装

DomainServices実装:
  - 複数Entityにまたがるビジネスロジック
  - ドメインルール実装
  - 純粋関数型実装
```

**✅ 実行可能な作業**:
```yaml
ファイル作成・編集:
  - src/UbiquitousLanguageManager.Domain/ValueObjects.fs
  - src/UbiquitousLanguageManager.Domain/Entities.fs
  - src/UbiquitousLanguageManager.Domain/DomainServices.fs
  - src/UbiquitousLanguageManager.Domain/Errors.fs

読み取り専用参照:
  - Domain層内の他ファイル
  - 設計書（Doc/02_Design/）
  - 仕様書（Doc/01_Requirements/）
```

**❌ 禁止範囲**:
```yaml
絶対禁止:
  - tests/ 配下のファイル読み込み・参照
  - Application層ファイルの修正
  - Contracts層ファイルの修正
  - Infrastructure層ファイルの修正
  - Web層ファイルの修正
  - テスト実装・TDD実践（unit-testの責務）
```

**F#固有の注意事項**:
```yaml
Compilation Order制約:
  - 前方参照禁止
  - ファイル順序: ValueObjects → Errors → Entities → DomainServices
  - namespace順序: Common → Authentication → ProjectManagement → UbiquitousLanguageManagement

型推論制約:
  - 型注釈明示（初学者対応）
  - Option型・Result型の説明コメント必須
  - パターンマッチング詳細コメント
```

**Phase B1実績**:
- User Entity実装（Authentication Bounded Context）
- Email, Password ValueObjects実装
- 97点品質達成（Clean Architecture準拠）

---

### 2. fsharp-application Agent

**責務**: F#アプリケーションサービス・ユースケース実装・ドメインロジックオーケストレーション

**実行範囲**: `src/UbiquitousLanguageManager.Application/` 配下**のみ**

**主要ツール**:
- Read
- Write
- Edit
- MultiEdit
- Grep
- Glob
- Bash (dotnet build等)

**適用場面**:
```yaml
UseCase実装:
  - RegisterUserUseCase（ユーザー登録ユースケース）
  - CreateProjectUseCase（プロジェクト作成ユースケース）
  - トランザクション境界定義
  - ドメインロジックオーケストレーション

ApplicationService実装:
  - 複数UseCaseの調整
  - 外部サービス連携調整
  - イベント発行

Input/Output DTO定義:
  - F#型としてのDTO定義
  - バリデーション実装
  - Domain型との変換ロジック
```

**✅ 実行可能な作業**:
```yaml
ファイル作成・編集:
  - src/UbiquitousLanguageManager.Application/UseCases/*.fs
  - src/UbiquitousLanguageManager.Application/ApplicationServices/*.fs
  - src/UbiquitousLanguageManager.Application/DTOs/*.fs

読み取り専用参照:
  - Application層内の他ファイル
  - Domain層ファイル（依存許可）
  - 設計書・仕様書
```

**❌ 禁止範囲**:
```yaml
絶対禁止:
  - tests/ 配下のファイル読み込み・参照
  - Domain層ファイルの修正（参照のみ許可）
  - Contracts層ファイルの修正
  - Infrastructure層ファイルの修正
  - Web層ファイルの修正
  - ビジネスロジック実装（Domain層の責務）
```

**Application層特有の注意事項**:
```yaml
依存方向:
  - Domain層への依存: ✅ 許可
  - Infrastructure層への依存: ❌ 禁止（Interfaceのみ定義・実装は禁止）
  - Web層への依存: ❌ 禁止

トランザクション境界:
  - UseCaseがトランザクション境界
  - Repository操作の調整
  - ドメインイベント発行タイミング管理
```

**Phase B1実績**:
- RegisterUserUseCase実装
- CreateProjectUseCase実装
- ドメインロジックオーケストレーション適用

---

### 3. contracts-bridge Agent

**責務**: F#↔C#型変換・TypeConverter実装・境界DTO実装

**実行範囲**: `src/UbiquitousLanguageManager.Contracts/` 配下**のみ**

**主要ツール**:
- mcp__serena__find_symbol
- mcp__serena__replace_symbol_body
- Read
- Write
- Edit
- MultiEdit

**適用場面**:
```yaml
F# → C# 型変換:
  - F# Option<'T> → C# nullable参照型
  - F# Result<'T, 'TError> → C# custom Result<T, TError>
  - F# Discriminated Union → C# class階層
  - F# Record → C# record

C# → F# 型変換:
  - C# nullable → F# Option
  - C# custom Result → F# Result
  - C# class階層 → F# Discriminated Union
  - C# record → F# Record

TypeConverter実装:
  - UserEntityConverter（User Entity ↔ UserDto）
  - ProjectEntityConverter（Project Entity ↔ ProjectDto）
  - 双方向変換実装
```

**✅ 実行可能な作業**:
```yaml
ファイル作成・編集:
  - src/UbiquitousLanguageManager.Contracts/DTOs/*.cs
  - src/UbiquitousLanguageManager.Contracts/TypeConverters/*.cs
  - src/UbiquitousLanguageManager.Contracts/Mappers/*.cs

読み取り専用参照:
  - Contracts層内の他ファイル
  - Domain層（F#型定義確認）
  - Infrastructure層（C#型定義確認）
```

**❌ 禁止範囲**:
```yaml
絶対禁止:
  - tests/ 配下のファイル読み込み・参照
  - Domain層ファイルの修正
  - Application層ファイルの修正
  - Infrastructure層ファイルの修正
  - Web層ファイルの修正
  - ビジネスロジック実装（変換ロジックのみ許可）
```

**F#↔C#境界の4つの変換パターン（Phase B1確立）**:
```yaml
Pattern 1: F# Option<'T> → C# nullable:
  F#: Some(value) / None
  C#: value / null
  注意: null安全性確保

Pattern 2: F# Result<'T, 'TError> → C# Result<T, TError>:
  F#: Ok(value) / Error(error)
  C#: Result<T, TError>.Success(value) / Result<T, TError>.Failure(error)
  注意: カスタムResult型実装必要

Pattern 3: F# Discriminated Union → C# class階層:
  F#: type UserStatus = Active | Inactive | Suspended
  C#: abstract class UserStatus / class Active : UserStatus / ...
  注意: パターンマッチング再現

Pattern 4: F# Record → C# record:
  F#: type UserDto = { Id: Guid; Email: string }
  C#: record UserDto(Guid Id, string Email)
  注意: immutability保持
```

**Phase B1実績**:
- UserEntityConverter実装（4パターン適用）
- ProjectEntityConverter実装
- 型変換パターン確立（fsharp-csharp-bridge Skill化）

---

### 4. csharp-infrastructure Agent

**責務**: Entity Framework Repository実装・データベースアクセス・外部サービス連携・インフラ設定

**実行範囲**: `src/UbiquitousLanguageManager.Infrastructure/` 配下**のみ**

**主要ツール**:
- mcp__serena__find_symbol
- mcp__serena__replace_symbol_body
- mcp__serena__get_symbols_overview
- mcp__serena__find_referencing_symbols
- Read
- Write
- Edit
- MultiEdit
- Bash (dotnet ef等)

**適用場面**:
```yaml
Repository実装:
  - IUserRepository実装（UserRepository.cs）
  - IProjectRepository実装（ProjectRepository.cs）
  - Entity Framework Core活用
  - LINQ to Entities実装

DbContext実装:
  - ApplicationDbContext実装
  - Entity Configuration実装
  - Migration生成・適用

外部サービス連携:
  - EmailService実装（SMTP連携）
  - FileStorageService実装
  - API Client実装
```

**✅ 実行可能な作業**:
```yaml
ファイル作成・編集:
  - src/UbiquitousLanguageManager.Infrastructure/Repositories/*.cs
  - src/UbiquitousLanguageManager.Infrastructure/Data/ApplicationDbContext.cs
  - src/UbiquitousLanguageManager.Infrastructure/Data/Configurations/*.cs
  - src/UbiquitousLanguageManager.Infrastructure/Services/*.cs
  - src/UbiquitousLanguageManager.Infrastructure/Migrations/*.cs

読み取り専用参照:
  - Infrastructure層内の他ファイル
  - Domain層（Interface定義確認）
  - Application層（Interface定義確認）
  - Contracts層（DTO型確認）
  - データベース設計書（Doc/02_Design/データベース設計書.md）
```

**❌ 禁止範囲**:
```yaml
絶対禁止:
  - tests/ 配下のファイル読み込み・参照
  - Domain層ファイルの修正
  - Application層ファイルの修正
  - Contracts層ファイルの修正
  - Web層ファイルの修正
  - ビジネスロジック実装（データアクセスのみ許可）
```

**Entity Framework固有の注意事項**:
```yaml
N+1問題対策:
  - Include/ThenInclude活用
  - AsNoTracking活用（読み取り専用）
  - Projection活用

トランザクション管理:
  - DbContext.SaveChangesAsync()
  - トランザクションスコープ管理
  - 楽観的同時実行制御

Migration管理:
  - dotnet ef migrations add MigrationName
  - dotnet ef database update
  - Migration履歴管理
```

**Phase B1実績**:
- UserRepository実装（Identity統合）
- ApplicationDbContext実装
- Migration生成・適用自動化

---

### 5. csharp-web-ui Agent

**責務**: Blazor Serverコンポーネント・Razor・フロントエンドUI・認証UI統合・リアルタイム機能実装

**実行範囲**: `src/UbiquitousLanguageManager.Web/` 配下**のみ**

**主要ツール**:
- mcp__serena__find_symbol
- mcp__serena__replace_symbol_body
- mcp__serena__get_symbols_overview
- mcp__serena__find_referencing_symbols
- Read
- Write
- Edit
- MultiEdit

**適用場面**:
```yaml
Blazor Serverコンポーネント実装:
  - Pages/Login.razor（ログインページ）
  - Pages/UserManagement.razor（ユーザー管理ページ）
  - Components/UserTable.razor（ユーザー一覧コンポーネント）
  - StateHasChanged()活用・ライフサイクル管理

認証UI統合:
  - ASP.NET Core Identity統合
  - AuthenticationStateProvider活用
  - [Authorize]属性活用

リアルタイム機能実装:
  - SignalR Hub実装
  - リアルタイム更新
  - 再接続ロジック
```

**✅ 実行可能な作業**:
```yaml
ファイル作成・編集:
  - src/UbiquitousLanguageManager.Web/Pages/*.razor
  - src/UbiquitousLanguageManager.Web/Components/*.razor
  - src/UbiquitousLanguageManager.Web/Shared/*.razor
  - src/UbiquitousLanguageManager.Web/Services/*.cs
  - src/UbiquitousLanguageManager.Web/Program.cs

読み取り専用参照:
  - Web層内の他ファイル
  - すべての層（全層参照許可）
  - 設計書・仕様書
```

**❌ 禁止範囲**:
```yaml
絶対禁止:
  - tests/ 配下のファイル読み込み・参照
  - Domain層ファイルの修正
  - Application層ファイルの修正
  - Contracts層ファイルの修正
  - Infrastructure層ファイルの修正
  - ビジネスロジック実装（UIロジックのみ許可）
```

**Blazor Server固有の注意事項（初学者対応）**:
```yaml
ライフサイクル:
  - OnInitializedAsync(): 初期化処理
  - OnParametersSetAsync(): パラメータ変更時
  - OnAfterRenderAsync(): レンダリング後
  - Dispose(): クリーンアップ
  - 各ライフサイクルの説明コメント必須

StateHasChanged():
  - UIの再レンダリングトリガー
  - 非同期処理後の呼び出し必須
  - 呼び出しタイミングの説明コメント必須

SignalR接続:
  - HubConnectionBuilder活用
  - 再接続ロジック実装
  - 接続状態管理
  - タイムアウト対策
```

**Phase B1実績**:
- Login.razor実装（認証UI）
- UserManagement.razor実装（CRUD UI）
- SignalR再接続ロジック実装

---

## 実装系Agent組み合わせパターン

### Pattern A: 新機能実装（ボトムアップ）

**組み合わせ**: fsharp-domain → fsharp-application → contracts-bridge → csharp-infrastructure → csharp-web-ui

**理由**: Clean Architecture依存方向に従う実装順序

**並列実行判断**:
```yaml
✅ 並列可能:
  - fsharp-domain + unit-test（責務分離・src/とtests/分離）
  - fsharp-application + unit-test
  - csharp-infrastructure + integration-test
  - csharp-web-ui + integration-test

❌ 並列不可:
  - fsharp-domain + fsharp-application（依存関係あり）
  - contracts-bridge + csharp-infrastructure（同一ファイル操作可能性）
```

**Phase B2実績**: Phase B2 Step4-7で適用（E2Eテスト基盤実装）

---

### Pattern B: 技術基盤整備（レイヤー別）

**組み合わせ**: 該当層Agent + unit-test/integration-test

**理由**: 特定レイヤーの改善・リファクタリング

**例**:
```yaml
Domain層リファクタリング:
  - fsharp-domain Agent + unit-test Agent（並列）

Infrastructure層最適化:
  - csharp-infrastructure Agent + integration-test Agent（並列）

Web層UI改善:
  - csharp-web-ui Agent + integration-test Agent（並列）
```

---

### Pattern C: F#↔C#境界修正（contracts-bridge単独）

**組み合わせ**: contracts-bridge Agent のみ

**理由**: 型変換ロジック修正は境界層のみで完結

**注意事項**:
- Domain層・Infrastructure層の読み取り専用参照は許可
- 実装修正は禁止

---

## 並列実行判断

### ✅ 並列実行可能な組み合わせ

**実装系 + テスト系**:
```yaml
並列可能な理由:
  - 責務分離: 実装系は src/ 配下、テスト系は tests/ 配下
  - ファイル競合なし
  - 同時書き込みリスクなし

組み合わせ:
  - fsharp-domain + unit-test
  - fsharp-application + unit-test
  - csharp-infrastructure + integration-test
  - csharp-web-ui + integration-test
```

### ❌ 並列実行不可能な組み合わせ

**実装系同士**:
```yaml
並列不可な理由:
  - 同一ファイルへの同時書き込みリスク
  - 依存関係による順序制約
  - ビルドエラーリスク

組み合わせ:
  - fsharp-domain + fsharp-application（依存関係）
  - fsharp-application + contracts-bridge（依存関係）
  - contracts-bridge + csharp-infrastructure（同一ファイル可能性）
  - csharp-infrastructure + csharp-web-ui（同一ファイル可能性）
```

---

## 選択チェックリスト

### Step開始時

- [ ] Domain層実装が必要か？ → fsharp-domain
- [ ] Application層実装が必要か？ → fsharp-application
- [ ] F#↔C#境界実装が必要か？ → contracts-bridge
- [ ] Infrastructure層実装が必要か？ → csharp-infrastructure
- [ ] Web層実装が必要か？ → csharp-web-ui

### Agent選択迷い時

- [ ] ValueObjects/Entities/DomainServices実装か？ → fsharp-domain
- [ ] UseCase/ApplicationService実装か？ → fsharp-application
- [ ] 型変換・DTO実装か？ → contracts-bridge
- [ ] Repository/DbContext/外部サービス実装か？ → csharp-infrastructure
- [ ] Blazor/Razor/UI実装か？ → csharp-web-ui

### 責務境界確認

- [ ] 該当Agentの実行範囲を確認した
- [ ] 禁止範囲に該当しないことを確認した
- [ ] 並列実行可能性を判断した

---

**作成日**: 2025-11-01
**Phase B-F2 Step2**: Agent Skills Phase 2展開
**参照**: SubAgent組み合わせパターン.md、ADR_013、fsharp-csharp-bridge Skill
