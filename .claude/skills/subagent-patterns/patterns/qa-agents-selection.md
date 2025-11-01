# 品質保証系Agent選択パターン（4Agent）

## 概要

品質保証系Agentは、テスト実装・コード品質評価・仕様準拠監査を担当する4つのSubAgentで構成されます。**実装コード修正禁止**が共通原則であり、品質確認・提案のみを行います（unit-test/integration-testを除く）。

---

## 🔴 CRITICAL: 品質保証系Agent共通原則

### 実装コード修正禁止原則

**原則**:
```yaml
読み取り専用Agent（修正禁止）:
  - code-review Agent
  - spec-compliance Agent

実装可能Agent（テストコードのみ）:
  - unit-test Agent（tests/ 配下のみ）
  - integration-test Agent（tests/ 配下のみ）
```

**違反例**（Phase B1で検出・修正）:
- ❌ code-review AgentがDomain層実装を直接修正
- ❌ spec-compliance Agentが仕様逸脱箇所を直接修正

**正しい対応**:
- ✅ 改善提案を提示
- ✅ 該当実装系Agentに修正委託
- ✅ Fix-Mode活用指示

---

## Agent一覧

### 1. unit-test Agent

**責務**: TDD実践・単体テスト設計実装・Red-Green-Refactorサイクル・テストカバレッジ管理・テスタブルコード設計

**実行範囲**: `tests/` 配下のすべてのテストプロジェクト**のみ**

**主要ツール**:
- mcp__serena__find_symbol
- mcp__serena__replace_symbol_body
- mcp__serena__get_symbols_overview
- Read
- Write
- Edit
- MultiEdit
- Grep
- Glob
- Bash (dotnet test等)

**適用場面**:
```yaml
TDD実践:
  - Red Phase: 失敗するテスト作成
  - Green Phase: 最小限の実装で合格
  - Refactor Phase: コード改善・リファクタリング

単体テスト実装:
  - Domain層テスト（ValueObjects/Entities/DomainServices）
  - Application層テスト（UseCase/ApplicationService）
  - Contracts層テスト（TypeConverter/Mapper）

テストカバレッジ管理:
  - カバレッジ測定（dotnet test --collect:"XPlat Code Coverage"）
  - 未カバー箇所の特定
  - テストケース追加
```

**✅ 実行可能な作業**:
```yaml
ファイル作成・編集:
  - tests/UbiquitousLanguageManager.Domain.Unit.Tests/**/*.cs
  - tests/UbiquitousLanguageManager.Application.Unit.Tests/**/*.cs
  - tests/UbiquitousLanguageManager.Contracts.Unit.Tests/**/*.cs
  - tests/UbiquitousLanguageManager.Infrastructure.Unit.Tests/**/*.cs
  - tests/UbiquitousLanguageManager.Web.Unit.Tests/**/*.cs

読み取り専用参照:
  - src/ 配下の実装コード（テスト対象の理解）
  - 設計書・仕様書
  - ADR（テスト方針確認）
```

**❌ 禁止範囲**:
```yaml
絶対禁止:
  - src/ 配下の実装コード修正（テスト対象の修正禁止）
  - テスト対象の実装変更
  - ビジネスロジック実装
```

**TDD Red-Green-Refactorサイクル（Phase B1確立）**:
```yaml
Red Phase:
  - 失敗するテスト作成
  - テストケース明確化
  - 期待値定義

Green Phase:
  - 最小限の実装で合格
  - テスト合格優先
  - リファクタリングは後回し

Refactor Phase:
  - コード改善
  - 重複除去
  - 可読性向上
  - テスト合格維持
```

**Phase B1実績**:
- UserEntityTests実装（97%カバレッジ達成）
- TDD Red-Green-Refactorサイクル実践
- tdd-red-green-refactor Skill確立

---

### 2. integration-test Agent

**責務**: WebApplicationFactory統合テスト・E2E・APIテスト・データベース統合テスト・テスト環境管理

**実行範囲**: `tests/` 配下の統合テストプロジェクト**のみ**

**主要ツール**:
- mcp__serena__find_symbol
- mcp__serena__replace_symbol_body
- mcp__serena__get_symbols_overview
- Bash (dotnet test/docker-compose等)
- Read
- Write
- Edit
- MultiEdit

**適用場面**:
```yaml
WebApplicationFactory統合テスト:
  - ASP.NET Core統合テスト
  - HttpClient活用
  - インメモリデータベース活用

E2Eテスト:
  - Playwright活用（Playwright MCP連携）
  - UI操作自動化
  - data-testid属性活用

データベース統合テスト:
  - PostgreSQL Dockerコンテナ活用
  - Migration適用確認
  - トランザクション分離
```

**✅ 実行可能な作業**:
```yaml
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

**WebApplicationFactory活用パターン（Phase B2確立）**:
```csharp
public class CustomWebApplicationFactory<TProgram> : WebApplicationFactory<TProgram> where TProgram : class
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // インメモリデータベース設定
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseInMemoryDatabase("TestDb"));

            // テスト用サービス置き換え
        });
    }
}
```

**Playwright E2Eテストパターン（Phase B2確立）**:
```yaml
data-testid属性設計:
  - 命名規則: {page}-{component}-{action}
  - 例: login-email-input, login-submit-button

Playwright MCP活用:
  - browser_navigate: ページ遷移
  - browser_snapshot: UI状態確認
  - browser_click: クリック操作
  - browser_type: テキスト入力
```

**Phase B2実績**:
- E2Eテスト基盤構築（Playwright MCP統合）
- WebApplicationFactory統合テスト実装
- 93.3%効率化達成（playwright-e2e-patterns Skill確立）

---

### 3. code-review Agent

**責務**: コード品質・保守性評価・Clean Architecture準拠確認・パフォーマンス・セキュリティレビュー・ベストプラクティス適用

**実行範囲**: 全プロジェクトの読み込み・品質評価**のみ**

**主要ツール**:
- mcp__serena__find_symbol
- mcp__serena__get_symbols_overview
- mcp__serena__find_referencing_symbols
- Read
- Edit（改善提案のみ・実装修正禁止）
- Grep
- Bash (dotnet build/test等)

**適用場面**:
```yaml
コード品質評価:
  - 可読性・保守性評価
  - 命名規則確認
  - コメント充実度確認
  - 複雑度評価

Clean Architecture準拠確認:
  - レイヤー依存方向確認
  - 循環参照検出
  - 責務境界確認
  - namespace階層確認（ADR_019準拠）

パフォーマンスレビュー:
  - N+1問題検出
  - 不要なメモリ確保検出
  - LINQ最適化確認

セキュリティレビュー:
  - SQL Injection脆弱性確認
  - XSS脆弱性確認
  - 認証・認可漏れ確認
  - 機密情報漏洩リスク確認
```

**✅ 実行可能な作業**:
```yaml
読み取り専用参照:
  - src/ 配下の全実装コード
  - tests/ 配下の全テストコード
  - 設計書・仕様書
  - ADR

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

**Clean Architecture準拠確認観点（Phase B1で97点達成）**:
```yaml
レイヤー依存確認:
  - Domain層: 外部依存なし ✅
  - Application層: Domain層のみ参照 ✅
  - Infrastructure層: Domain, Application参照可 ✅
  - Web層: すべての層参照可 ✅

循環参照検出:
  - mcp__serena__find_referencing_symbols活用
  - 依存グラフ作成
  - 循環参照検出

namespace階層確認（ADR_019準拠）:
  - 3階層推奨: <Project>.<Layer>.<BoundedContext> ✅
  - 4階層許容: <Project>.<Layer>.<BoundedContext>.<Feature> ⚠️
  - 5階層以上禁止 ❌
```

**セキュリティレビュー観点（Phase B1確立）**:
```yaml
OWASP Top 10確認:
  - SQL Injection: Parameterized Query使用確認
  - XSS: エスケープ処理確認
  - CSRF: Anti-Forgeryトークン確認
  - 認証・認可: [Authorize]属性確認

機密情報漏洩リスク:
  - パスワード平文保存禁止
  - 接続文字列ハードコード禁止
  - APIキー埋め込み禁止
```

**Phase B1実績**:
- Phase B1完了時97点品質達成に貢献
- レイヤー依存違反0件維持
- セキュリティレビュー実施

---

### 4. spec-compliance Agent

**責務**: 仕様準拠監査・受け入れ基準確認・仕様準拠マトリックス検証・仕様逸脱リスク特定対策

**実行範囲**: 仕様書・実装コードの照合確認**のみ**

**主要ツール**:
- Read（仕様書・実装コード読み込み）
- Grep
- WebFetch（外部仕様参照）
- mcp__serena__find_symbol
- mcp__serena__get_symbols_overview
- mcp__serena__search_for_pattern

**適用場面**:
```yaml
仕様準拠確認:
  - 機能仕様書との照合
  - 非機能要件確認
  - データ整合性確認
  - UI/UX要件確認

仕様準拠マトリックス検証:
  - spec-analysis Agentが作成したマトリックス検証
  - 仕様カバレッジ確認
  - 未実装要件特定

受け入れ基準確認:
  - 受け入れ基準達成度確認
  - Edge Case対応確認
  - エラーハンドリング確認
```

**✅ 実行可能な作業**:
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

**仕様準拠確認4観点（Phase B1確立）**:
```yaml
1. 機能要件確認:
   - 仕様書記載の全機能実装確認
   - 機能動作確認
   - 例外処理確認

2. 非機能要件確認:
   - パフォーマンス要件確認
   - セキュリティ要件確認
   - 可用性要件確認

3. データ整合性確認:
   - データベーススキーマと仕様の整合性
   - バリデーションルール確認
   - 制約条件確認

4. UI/UX要件確認:
   - 画面レイアウト確認
   - ユーザビリティ確認
   - アクセシビリティ確認
```

**仕様準拠マトリックス検証例**:
```markdown
## 仕様準拠マトリックス検証結果

| 仕様ID | 仕様内容 | 実装状況 | 準拠度 | 逸脱内容 | 対策 |
|--------|---------|---------|-------|---------|-----|
| REQ-001 | ユーザー登録 | ✅完了 | 100% | なし | - |
| REQ-002 | メール検証 | ⚠️部分実装 | 80% | リトライ未実装 | fsharp-application Agent修正依頼 |
| REQ-003 | パスワード変更 | ❌未実装 | 0% | 全体未実装 | Phase A7で実装予定 |

**総合準拠度**: 60%（3項目中1.8項目準拠）
**目標準拠度**: 95%
**改善必要項目**: 2項目
```

**Phase B1実績**:
- Phase A1-A6仕様準拠度95%達成
- 仕様逸脱リスク特定・対策実施
- spec-compliance-auto Skill確立

---

## 品質保証系Agent組み合わせパターン

### Pattern A: 新機能実装時の品質保証

**組み合わせ**: unit-test + integration-test → code-review + spec-compliance

**理由**:
1. **unit-test + integration-test**: テスト実装（並列可能）
2. **code-review + spec-compliance**: 品質・仕様準拠確認（並列可能・テスト完了後）

**Phase B2実績**: Phase B2 Step7-8で適用

---

### Pattern B: テスト強化Phase

**組み合わせ**: unit-test → integration-test → code-review

**理由**:
1. **unit-test**: 単体テスト拡充（TDDサイクル）
2. **integration-test**: 統合テスト・E2Eテスト拡充
3. **code-review**: テスト品質確認

**Phase B2実績**: Phase B2 Step1-3で適用

---

### Pattern C: 仕様準拠監査Phase

**組み合わせ**: spec-compliance → code-review

**理由**:
1. **spec-compliance**: 仕様準拠度評価・逸脱箇所特定
2. **code-review**: コード品質改善提案

**Phase A7想定**: 要件準拠・アーキテクチャ統一Phase

---

## 並列実行判断

### ✅ 並列実行可能な組み合わせ

**unit-test + integration-test**（条件付き）:
```yaml
並列可能な条件:
  - 異なるテストプロジェクト対象
  - 同一テストプロジェクト操作なし

並列不可な条件:
  - 同一テストプロジェクト操作可能性
  - テストプロジェクト参照関係競合リスク
```

**code-review + spec-compliance**:
```yaml
並列可能な理由:
  - 両方とも読み取り専用（実装修正なし）
  - ファイル競合なし
  - 同時実行リスクなし
```

**実装系 + テスト系**:
```yaml
並列可能な理由:
  - 責務分離（src/ と tests/ 分離）
  - ファイル競合なし

組み合わせ:
  - fsharp-domain + unit-test
  - fsharp-application + unit-test
  - csharp-infrastructure + integration-test
  - csharp-web-ui + integration-test
```

### ❌ 並列実行不可能な組み合わせ

**テスト系同士（同一プロジェクト操作可能性）**:
```yaml
並列不可な理由:
  - 同一テストプロジェクトへの同時書き込みリスク
  - テストプロジェクト参照関係競合リスク

推奨しない組み合わせ:
  - unit-test + integration-test（同一テストプロジェクト操作可能性）
```

---

## 選択チェックリスト

### Step開始時

- [ ] TDD実践が必要か？ → unit-test
- [ ] 統合テスト・E2Eテストが必要か？ → integration-test
- [ ] コード品質確認が必要か？ → code-review
- [ ] 仕様準拠確認が必要か？ → spec-compliance

### Agent選択迷い時

- [ ] Red-Green-Refactorサイクル実践か？ → unit-test
- [ ] WebApplicationFactory/Playwrightテストか？ → integration-test
- [ ] Clean Architecture準拠確認か？ → code-review
- [ ] 仕様準拠マトリックス検証か？ → spec-compliance

### 並列実行判断

- [ ] テスト系同士の並列実行は同一プロジェクト操作なしか確認
- [ ] code-review + spec-complianceは常に並列可能

---

**作成日**: 2025-11-01
**Phase B-F2 Step2**: Agent Skills Phase 2展開
**参照**: SubAgent組み合わせパターン.md、ADR_013、tdd-red-green-refactor Skill、spec-compliance-auto Skill
