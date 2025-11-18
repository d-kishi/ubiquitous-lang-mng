# SubAgent責務境界判定ルール

## 概要

SubAgent責務境界は、**Clean Architectureのレイヤー別責務**に厳格に対応しています。責務境界違反は、レイヤー依存方向違反・技術負債増加・テスト失敗の原因となるため、**絶対遵守**が必要です。

---

## 🔴 CRITICAL: 責務境界絶対原則

### 絶対原則

```yaml
1 Agent = 1 Layer（厳格1対1対応）:
  - fsharp-domain → Domain層のみ
  - fsharp-application → Application層のみ
  - contracts-bridge → Contracts層のみ
  - csharp-infrastructure → Infrastructure層のみ
  - csharp-web-ui → Web層のみ

他層の実装修正禁止:
  - 読み取り専用参照のみ許可
  - 実装修正は該当層Agentに委託

tests/配下への参照禁止（実装系Agent）:
  - unit-test/integration-test Agentの責務
  - 実装系Agentはテストコード参照・修正禁止
```

### 違反例（Phase B1で検出・修正）

```yaml
❌ 違反例1: fsharp-domain AgentがContracts層を修正
  - 発生: Domain層実装時にDTO修正を試みた
  - 影響: レイヤー依存方向違反・Clean Architecture崩壊
  - 修正: contracts-bridge Agentに委託

❌ 違反例2: contracts-bridge AgentがDomain層を修正
  - 発生: 型変換実装時にDomain層型を修正しようとした
  - 影響: 境界層がドメインロジックに介入
  - 修正: fsharp-domain Agentに委託

❌ 違反例3: csharp-web-ui AgentがInfrastructure層を修正
  - 発生: UI実装時にRepository修正を試みた
  - 影響: UI層がデータアクセス層に介入
  - 修正: csharp-infrastructure Agentに委託
```

---

## 実装系Agent責務境界（5Agent）

### 1. fsharp-domain Agent

**✅ 実行範囲**:
```yaml
ディレクトリ:
  - src/UbiquitousLanguageManager.Domain/ 配下のみ

ファイル作成・編集:
  - ValueObjects.fs
  - Entities.fs
  - DomainServices.fs
  - Errors.fs

読み取り専用参照:
  - Domain層内の他ファイル
  - 設計書（Doc/02_Design/）
  - 仕様書（Doc/01_Requirements/）
  - ADR（Doc/07_Decisions/）
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
  - ビジネスロジック以外の実装（UI/データアクセス等）
```

**判定基準**:
```yaml
Domain層責務に該当:
  - ValueObjects実装（Email, Password等）
  - Entities実装（User, Project等）
  - DomainServices実装（複数Entity横断ロジック）
  - ドメインルール実装
  → ✅ fsharp-domain Agent選択

上記以外:
  → ❌ 他Agentに委託
```

**例**:
```yaml
✅ 正しい使用:
  - User Entity実装
  - Email ValueObject実装
  - UserDomainService実装（パスワード検証等）

❌ 誤った使用:
  - UserDto実装（contracts-bridgeの責務）
  - UserRepository実装（csharp-infrastructureの責務）
  - UserTests実装（unit-testの責務）
```

---

### 2. fsharp-application Agent

**✅ 実行範囲**:
```yaml
ディレクトリ:
  - src/UbiquitousLanguageManager.Application/ 配下のみ

ファイル作成・編集:
  - UseCases/*.fs
  - ApplicationServices/*.fs
  - DTOs/*.fs（F#型としてのDTO）

読み取り専用参照:
  - Application層内の他ファイル
  - Domain層ファイル（依存許可）
  - 設計書・仕様書・ADR
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
  - Infrastructure実装（Repository実装等）
```

**判定基準**:
```yaml
Application層責務に該当:
  - UseCase実装（RegisterUserUseCase等）
  - ApplicationService実装
  - ドメインロジックオーケストレーション
  - トランザクション境界定義
  → ✅ fsharp-application Agent選択

上記以外:
  → ❌ 他Agentに委託
```

**例**:
```yaml
✅ 正しい使用:
  - RegisterUserUseCase実装
  - UserApplicationService実装
  - トランザクション境界定義

❌ 誤った使用:
  - User Entity実装（fsharp-domainの責務）
  - UserRepository実装（csharp-infrastructureの責務）
  - UserDto（C#）実装（contracts-bridgeの責務）
```

---

### 3. contracts-bridge Agent

**✅ 実行範囲**:
```yaml
ディレクトリ:
  - src/UbiquitousLanguageManager.Contracts/ 配下のみ

ファイル作成・編集:
  - DTOs/*.cs（C# DTO）
  - TypeConverters/*.cs
  - Mappers/*.cs

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

**判定基準**:
```yaml
Contracts層責務に該当:
  - F# → C# 型変換
  - C# → F# 型変換
  - TypeConverter実装
  - DTO定義（C#）
  → ✅ contracts-bridge Agent選択

上記以外:
  → ❌ 他Agentに委託
```

**例**:
```yaml
✅ 正しい使用:
  - UserDto（C#）実装
  - UserEntityConverter実装（User Entity ↔ UserDto）
  - F# Option<'T> → C# nullable変換

❌ 誤った使用:
  - User Entity実装（fsharp-domainの責務）
  - UserRepository実装（csharp-infrastructureの責務）
  - ビジネスロジック実装（変換ロジック以外）
```

---

### 4. csharp-infrastructure Agent

**✅ 実行範囲**:
```yaml
ディレクトリ:
  - src/UbiquitousLanguageManager.Infrastructure/ 配下のみ

ファイル作成・編集:
  - Repositories/*.cs
  - Data/ApplicationDbContext.cs
  - Data/Configurations/*.cs
  - Services/*.cs（外部サービス連携）
  - Migrations/*.cs

読み取り専用参照:
  - Infrastructure層内の他ファイル
  - Domain層（Interface定義確認）
  - Application層（Interface定義確認）
  - Contracts層（DTO型確認）
  - データベース設計書
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

**判定基準**:
```yaml
Infrastructure層責務に該当:
  - Repository実装
  - DbContext実装
  - Entity Configuration実装
  - Migration生成・適用
  - 外部サービス連携実装
  → ✅ csharp-infrastructure Agent選択

上記以外:
  → ❌ 他Agentに委託
```

**例**:
```yaml
✅ 正しい使用:
  - UserRepository実装
  - ApplicationDbContext実装
  - UserConfiguration実装（Entity Configuration）
  - EmailService実装（SMTP連携）

❌ 誤った使用:
  - User Entity実装（fsharp-domainの責務）
  - RegisterUserUseCase実装（fsharp-applicationの責務）
  - UserDto実装（contracts-bridgeの責務）
```

---

### 5. csharp-web-ui Agent

**✅ 実行範囲**:
```yaml
ディレクトリ:
  - src/UbiquitousLanguageManager.Web/ 配下のみ

ファイル作成・編集:
  - Pages/*.razor
  - Components/*.razor
  - Shared/*.razor
  - Services/*.cs（UIサービス）
  - Program.cs

読み取り専用参照:
  - Web層内の他ファイル
  - すべての層（全層参照許可）
  - 設計書・仕様書・ADR
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

**判定基準**:
```yaml
Web層責務に該当:
  - Blazor Serverコンポーネント実装
  - Razorページ実装
  - UIロジック実装
  - SignalR Hub実装
  - 認証UI統合
  → ✅ csharp-web-ui Agent選択

上記以外:
  → ❌ 他Agentに委託
```

**例**:
```yaml
✅ 正しい使用:
  - Login.razor実装
  - UserManagement.razor実装
  - UserTableComponent.razor実装
  - AuthenticationStateProvider実装

❌ 誤った使用:
  - User Entity実装（fsharp-domainの責務）
  - UserRepository実装（csharp-infrastructureの責務）
  - ビジネスロジック実装（UIロジック以外）
```

---

## 品質保証系Agent責務境界（4Agent）

### 1. unit-test Agent

**✅ 実行範囲**:
```yaml
ディレクトリ:
  - tests/ 配下のすべてのテストプロジェクト

ファイル作成・編集:
  - tests/UbiquitousLanguageManager.Domain.Unit.Tests/**/*.cs
  - tests/UbiquitousLanguageManager.Application.Unit.Tests/**/*.cs
  - tests/UbiquitousLanguageManager.Contracts.Unit.Tests/**/*.cs
  - tests/UbiquitousLanguageManager.Infrastructure.Unit.Tests/**/*.cs
  - tests/UbiquitousLanguageManager.Web.Unit.Tests/**/*.cs

読み取り専用参照:
  - src/ 配下の実装コード（テスト対象の理解）
  - 設計書・仕様書・ADR
```

**❌ 禁止範囲**:
```yaml
絶対禁止:
  - src/ 配下の実装コード修正（テスト対象の修正禁止）
  - テスト対象の実装変更
  - ビジネスロジック実装
```

**判定基準**:
```yaml
unit-test責務に該当:
  - TDD実践・Red-Green-Refactorサイクル
  - 単体テスト実装
  - テストカバレッジ管理
  → ✅ unit-test Agent選択

上記以外:
  → ❌ 他Agentに委託
```

---

### 2. integration-test Agent

**✅ 実行範囲**:
```yaml
ディレクトリ:
  - tests/ 配下の統合テストプロジェクト

ファイル作成・編集:
  - tests/UbiquitousLanguageManager.Application.Integration.Tests/**/*.cs
  - tests/UbiquitousLanguageManager.Infrastructure.Integration.Tests/**/*.cs
  - tests/UbiquitousLanguageManager.Web.Integration.Tests/**/*.cs
  - tests/UbiquitousLanguageManager.Web.E2E.Tests/**/*.cs

読み取り専用参照:
  - src/ 配下の実装コード
  - docker-compose.yml
  - 設計書・仕様書
```

**❌ 禁止範囲**:
```yaml
絶対禁止:
  - src/ 配下の実装コード修正
  - テスト対象の実装変更
  - 本番環境への影響
```

**判定基準**:
```yaml
integration-test責務に該当:
  - WebApplicationFactory統合テスト
  - E2Eテスト（Playwright）
  - データベース統合テスト
  → ✅ integration-test Agent選択

上記以外:
  → ❌ 他Agentに委託
```

---

### 3. code-review Agent

**✅ 実行範囲**:
```yaml
読み取り専用参照:
  - src/ 配下の全実装コード
  - tests/ 配下の全テストコード
  - 設計書・仕様書・ADR

改善提案:
  - コード改善提案作成
  - リファクタリング提案
  - セキュリティ対策提案
  - パフォーマンス改善提案
```

**❌ 禁止範囲**:
```yaml
絶対禁止:
  - 実装コードの直接修正（改善提案のみ）
  - テストコードの直接修正（改善提案のみ）
  - ビジネスロジック実装
```

**判定基準**:
```yaml
code-review責務に該当:
  - コード品質評価
  - Clean Architecture準拠確認
  - パフォーマンス・セキュリティレビュー
  - ベストプラクティス適用確認
  → ✅ code-review Agent選択

実装修正が必要:
  → ❌ 該当実装系Agentに委託
```

---

### 4. spec-compliance Agent

**✅ 実行範囲**:
```yaml
読み取り専用参照:
  - Doc/01_Requirements/ 配下の全仕様書
  - src/ 配下の全実装コード
  - tests/ 配下の全テストコード
  - 設計書・ADR

仕様準拠評価:
  - 仕様準拠度評価（95%目標）
  - 仕様逸脱箇所の特定
  - 改善提案作成
```

**❌ 禁止範囲**:
```yaml
絶対禁止:
  - 実装コードの直接修正（準拠度評価のみ）
  - テストコードの直接修正
  - 仕様書の修正
```

**判定基準**:
```yaml
spec-compliance責務に該当:
  - 仕様準拠度評価
  - 仕様準拠マトリックス検証
  - 受け入れ基準確認
  → ✅ spec-compliance Agent選択

実装修正が必要:
  → ❌ 該当実装系Agentに委託
```

---

## 調査分析系Agent責務境界（4Agent）

### 1. tech-research Agent

**✅ 実行範囲**:
```yaml
ツール:
  - Bash (gemini連携)
  - WebSearch
  - WebFetch
  - Grep / Glob

活動:
  - 技術調査・最新情報収集
  - ベストプラクティス調査
  - 技術ドキュメント参照
```

**❌ 禁止範囲**:
```yaml
絶対禁止:
  - 実装コードの直接修正
  - 仕様書分析のみ（spec-analysisの責務）
```

---

### 2. spec-analysis Agent

**✅ 実行範囲**:
```yaml
ツール:
  - Read（仕様書・設計書）
  - Grep
  - WebFetch

活動:
  - 仕様分析・要件抽出
  - 仕様準拠マトリックス作成
  - テスト要件抽出
```

**❌ 禁止範囲**:
```yaml
絶対禁止:
  - 実装コードの直接修正
  - 設計整合性確認のみ（design-reviewの責務）
```

---

### 3. design-review Agent

**✅ 実行範囲**:
```yaml
ツール:
  - Read
  - mcp__serena__get_symbols_overview
  - mcp__serena__find_symbol
  - Grep

活動:
  - 設計整合性確認
  - Clean Architecture準拠確認
  - レイヤー間依存関係確認
```

**❌ 禁止範囲**:
```yaml
絶対禁止:
  - 実装コードの直接修正
  - コード品質評価のみ（code-reviewの責務）
```

---

### 4. dependency-analysis Agent

**✅ 実行範囲**:
```yaml
ツール:
  - mcp__serena__find_referencing_symbols
  - mcp__serena__find_symbol
  - Grep / Read
  - Bash (dotnet list package等)

活動:
  - 依存関係特定・実装順序決定
  - 制約リスク分析
  - NuGetパッケージ依存確認
```

**❌ 禁止範囲**:
```yaml
絶対禁止:
  - 実装コードの直接修正
  - 設計レベルの依存確認のみ（design-reviewの責務）
```

---

## 責務境界判定フローチャート

```yaml
実装修正が必要か？:
  YES:
    ファイル場所は？:
      tests/配下:
        TDD実践・単体テストか？:
          YES: → unit-test Agent
          NO: → integration-test Agent

      src/UbiquitousLanguageManager.Domain/配下:
        → fsharp-domain Agent

      src/UbiquitousLanguageManager.Application/配下:
        → fsharp-application Agent

      src/UbiquitousLanguageManager.Contracts/配下:
        → contracts-bridge Agent

      src/UbiquitousLanguageManager.Infrastructure/配下:
        → csharp-infrastructure Agent

      src/UbiquitousLanguageManager.Web/配下:
        → csharp-web-ui Agent

  NO:
    読み取り専用作業か？:
      技術調査・Web情報収集:
        → tech-research Agent

      仕様分析・要件抽出:
        → spec-analysis Agent

      設計整合性確認:
        → design-review Agent

      依存関係分析:
        → dependency-analysis Agent

      コード品質評価:
        → code-review Agent

      仕様準拠度評価:
        → spec-compliance Agent
```

---

## 責務境界チェックリスト

### Step開始時

- [ ] 実装修正が必要か確認した
- [ ] 修正対象ファイル場所を確認した
- [ ] 該当Agentの実行範囲に含まれることを確認した
- [ ] 禁止範囲に該当しないことを確認した

### Agent選択迷い時

- [ ] 責務境界判定フローチャートを参照した
- [ ] 複数Agentが候補の場合、責務マトリックスを確認した
- [ ] 並列実行可能性を判断した

### 実装修正時

- [ ] 該当Agentの実行範囲を厳守している
- [ ] 他層のファイル修正を行っていない
- [ ] tests/配下の参照・修正を行っていない（実装系Agent）

---

**作成日**: 2025-11-01
**Phase B-F2 Step2**: Agent Skills Phase 2展開
**参照**: SubAgent組み合わせパターン.md、ADR_013、ADR_016
