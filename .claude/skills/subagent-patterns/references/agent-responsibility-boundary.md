# SubAgent責務境界判定ルール

## 概要

SubAgent責務境界は、**Clean Architectureのレイヤー別責務**に厳格に対応。

## 目次

- [責務境界絶対原則](#-critical-責務境界絶対原則)
- [実装系Agent責務境界](#実装系agent責務境界5agent)
- [品質保証系Agent責務境界](#品質保証系agent責務境界)
- [調査分析系Agent責務境界](#調査分析系agent責務境界)

---

## 🔴 CRITICAL: 責務境界絶対原則

### 絶対原則

| 原則 | 内容 |
|------|------|
| **1 Agent = 1 Layer** | fsharp-domain→Domain層のみ、csharp-infrastructure→Infrastructure層のみ等 |
| **他層の実装修正禁止** | 読み取り専用参照のみ許可、実装修正は該当層Agentに委託 |
| **tests/配下への参照禁止** | 実装系Agentはテストコード参照・修正禁止（unit-test/integration-testの責務） |

### 違反例（Phase B1で検出）

| 違反 | 内容 | 修正方法 |
|------|------|----------|
| fsharp-domain→Contracts層修正 | Domain実装時にDTO修正を試みた | contracts-bridge Agentに委託 |
| contracts-bridge→Domain層修正 | 型変換時にDomain型を修正しようとした | fsharp-domain Agentに委託 |
| csharp-web-ui→Infrastructure層修正 | UI実装時にRepository修正を試みた | csharp-infrastructure Agentに委託 |

---

## 実装系Agent責務境界（5Agent）

| Agent | 対象ディレクトリ | 作成・編集ファイル | 読み取り参照 | 禁止 |
|-------|-----------------|-------------------|-------------|------|
| **fsharp-domain** | `src/.../Domain/` | ValueObjects.fs, Entities.fs, DomainServices.fs, Errors.fs | Domain層内, 設計書, 仕様書, ADR | tests/配下, 他層修正, UI/データアクセス実装 |
| **fsharp-application** | `src/.../Application/` | UseCases/*.fs, ApplicationServices/*.fs | Application層内, Domain層, 設計書 | tests/配下, Domain層修正, 他層修正 |
| **contracts-bridge** | `src/.../Contracts/` | DTOs/*.cs, TypeConverters/*.cs, Mappers/*.cs | Contracts層内, Domain層, Infrastructure層 | tests/配下, 他層修正, ビジネスロジック |
| **csharp-infrastructure** | `src/.../Infrastructure/` | Repositories/*.cs, DbContext, Configurations, Migrations | Infrastructure層内, Domain層, Application層, DB設計書 | tests/配下, 他層修正, ビジネスロジック |
| **csharp-web-ui** | `src/.../Web/` | Pages/*.razor, Components/*.razor, Services/*.cs, Program.cs | 全層参照可, 設計書 | tests/配下, 他層修正, ビジネスロジック |

### 実装系Agent判定基準

| Agent | 責務に該当する作業 |
|-------|-------------------|
| **fsharp-domain** | ValueObjects/Entities/DomainServices実装、ドメインルール実装 |
| **fsharp-application** | UseCase/ApplicationService実装、オーケストレーション、トランザクション境界 |
| **contracts-bridge** | F#↔C#型変換、TypeConverter実装、DTO定義（C#） |
| **csharp-infrastructure** | Repository/DbContext/Entity Configuration実装、Migration、外部サービス連携 |
| **csharp-web-ui** | Blazor Serverコンポーネント、Razorページ、UIロジック、SignalR Hub、認証UI |

---

## 品質保証系Agent責務境界（4Agent）

| Agent | 実行範囲 | 読み取り参照 | 禁止 |
|-------|---------|-------------|------|
| **unit-test** | tests/*.Unit.Tests/**/*.cs | src/配下（テスト対象理解）, 設計書 | src/配下の実装コード修正 |
| **integration-test** | tests/*.Integration.Tests/**/*.cs, tests/*.E2E.Tests/**/*.cs | src/配下, docker-compose.yml | src/配下の実装コード修正 |
| **code-review** | 改善提案作成のみ（読み取り専用） | src/配下全コード, tests/配下全コード, 設計書 | 実装・テストコードの直接修正 |
| **spec-compliance** | 仕様準拠評価のみ（読み取り専用） | Doc/01_Requirements/, src/, tests/, ADR | 実装・テストコードの直接修正 |

### 品質保証系Agent判定基準

| Agent | 責務に該当する作業 |
|-------|-------------------|
| **unit-test** | TDD実践・Red-Green-Refactor、単体テスト実装、カバレッジ管理 |
| **integration-test** | WebApplicationFactory統合テスト、E2Eテスト（Playwright）、DB統合テスト |
| **code-review** | コード品質評価、Clean Architecture準拠確認、パフォーマンス・セキュリティレビュー |
| **spec-compliance** | 仕様準拠度評価（95%目標）、仕様逸脱特定、受け入れ基準確認 |

---

## 調査分析系Agent責務境界（4Agent）

| Agent | ツール | 活動 | 禁止 |
|-------|-------|------|------|
| **tech-research** | Bash, WebSearch, WebFetch, Grep/Glob | 技術調査・最新情報収集、ベストプラクティス調査 | 実装コードの直接修正 |
| **spec-analysis** | Read, Grep, WebFetch | 仕様分析・要件抽出、仕様準拠マトリックス作成 | 実装コードの直接修正 |
| **design-review** | Read, mcp__serena__*, Grep | 設計整合性確認、Clean Architecture準拠確認、レイヤー間依存確認 | 実装コードの直接修正 |
| **dependency-analysis** | mcp__serena__*, Grep/Read, Bash | 依存関係特定・実装順序決定、制約リスク分析、NuGet依存確認 | 実装コードの直接修正 |

---

## 責務境界判定フローチャート

### 実装修正が必要な場合

| ファイル場所 | 選択Agent |
|-------------|-----------|
| `tests/` 配下（TDD・単体テスト） | unit-test |
| `tests/` 配下（統合・E2E） | integration-test |
| `src/.../Domain/` | fsharp-domain |
| `src/.../Application/` | fsharp-application |
| `src/.../Contracts/` | contracts-bridge |
| `src/.../Infrastructure/` | csharp-infrastructure |
| `src/.../Web/` | csharp-web-ui |

### 読み取り専用作業の場合

| 作業内容 | 選択Agent |
|----------|-----------|
| 技術調査・Web情報収集 | tech-research |
| 仕様分析・要件抽出 | spec-analysis |
| 設計整合性確認 | design-review |
| 依存関係分析 | dependency-analysis |
| コード品質評価 | code-review |
| 仕様準拠度評価 | spec-compliance |

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

**作成日**: 2025-12-18
**参照**: ADR_013、ADR_016、SubAgent組み合わせパターン
