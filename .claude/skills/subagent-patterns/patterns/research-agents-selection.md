# 調査分析系Agent選択パターン（4Agent）

## 概要

調査分析系Agentは、Phase・Step開始時の知見収集・仕様理解・設計確認・依存関係分析を担当する4つのSubAgentで構成されます。実装前の準備・判断材料収集に特化しています。

---

## Agent一覧

### 1. tech-research Agent

**責務**: 技術調査・最新情報収集・ベストプラクティス調査

**主要ツール**:
- Bash (gemini連携)
- WebSearch
- WebFetch
- Grep / Glob

**適用場面**:
```yaml
新技術導入検討時:
  - 新ライブラリ・フレームワーク選定
  - バージョンアップグレード調査
  - セキュリティパッチ適用可否判断

ベストプラクティス調査時:
  - Blazor Server SignalR接続パターン
  - Entity Framework Core最適化手法
  - F# + C#相互運用推奨パターン

技術課題解決時:
  - パフォーマンス問題の解決策調査
  - 技術的制約の回避方法調査
  - エラー・例外対処方法調査
```

**選択判断基準**:
- ✅ Gemini連携が必要（最新情報・AIアシスト）
- ✅ 外部Web情報収集が必要
- ✅ 技術ドキュメント・公式ドキュメント参照が必要
- ❌ 仕様書分析のみ（spec-analysisの責務）

**Phase B1実績**:
- Blazor Server SignalR再接続パターン調査
- Entity Framework Core N+1問題解決策調査
- F#型推論制約回避パターン調査

---

### 2. spec-analysis Agent

**責務**: 仕様分析・要件抽出・仕様準拠マトリックス作成

**主要ツール**:
- Read (仕様書・設計書読み込み)
- Grep (仕様パターン検索)
- WebFetch (外部仕様参照)
- mcp__serena__search_for_pattern

**適用場面**:
```yaml
Step開始時の仕様確認:
  - 要求仕様書（Doc/01_Requirements/）分析
  - 機能仕様書分析
  - 非機能要件抽出

仕様準拠マトリックス作成:
  - 実装項目と仕様の対応表作成
  - 仕様カバレッジ確認
  - 未実装要件の特定

テスト要件抽出:
  - テストケース要件分析
  - 受け入れ基準明確化
  - Edge Case特定
```

**選択判断基準**:
- ✅ 仕様書（.md, .pdf）からの要件抽出が必要
- ✅ 仕様準拠マトリックス作成が必要
- ✅ テスト要件明確化が必要
- ❌ 設計整合性確認のみ（design-reviewの責務）

**成果物例**:
```markdown
## 仕様準拠マトリックス

| 仕様ID | 仕様内容 | 実装ファイル | 実装状況 | テストケース |
|--------|---------|-------------|---------|-------------|
| REQ-001 | ユーザー登録 | UserService.fs | ✅完了 | UserServiceTests.fs:L42 |
| REQ-002 | メール検証 | EmailValidator.fs | ⏳実装中 | - |
```

**Phase B1実績**:
- Phase A1-A6仕様準拠マトリックス作成（95%準拠率達成）
- 認証要件・ユーザー管理要件抽出

---

### 3. design-review Agent

**責務**: 設計整合性確認・Clean Architecture準拠確認

**主要ツール**:
- Read
- mcp__serena__get_symbols_overview
- mcp__serena__find_symbol
- Grep

**適用場面**:
```yaml
設計整合性確認:
  - レイヤー間依存関係確認
  - namespace階層確認（ADR_019準拠）
  - Bounded Context境界確認

Clean Architecture準拠確認:
  - 依存方向ルール確認（Domain ← Application ← Infrastructure/Web）
  - 循環参照検出
  - レイヤー責務違反検出

データベース設計確認:
  - Entity構造とドメインモデル整合性
  - 正規化・制約確認
  - マイグレーション整合性
```

**選択判断基準**:
- ✅ Clean Architecture準拠確認が必要
- ✅ レイヤー間依存関係確認が必要
- ✅ 設計書と実装の整合性確認が必要
- ❌ 実装コード品質評価（code-reviewの責務）

**チェック観点**:
```yaml
レイヤー依存確認:
  - Domain層: 外部依存なし
  - Application層: Domain層のみ参照
  - Infrastructure層: Domain, Application参照可
  - Web層: すべての層参照可

namespace階層確認:
  - 3階層推奨: <Project>.<Layer>.<BoundedContext>
  - 4階層許容: <Project>.<Layer>.<BoundedContext>.<Feature>
  - 5階層以上禁止

F# Compilation Order確認:
  - Common → Authentication → ProjectManagement → UbiquitousLanguageManagement
  - ValueObjects → Errors → Entities → DomainServices
```

**Phase B1実績**:
- Phase B1完了時97点品質達成に貢献
- レイヤー依存違反0件維持

---

### 4. dependency-analysis Agent

**責務**: 依存関係特定・実装順序決定・制約リスク分析

**主要ツール**:
- mcp__serena__find_referencing_symbols
- mcp__serena__find_symbol
- Grep
- Read
- Bash (dotnet list package等)

**適用場面**:
```yaml
実装順序決定:
  - 依存グラフ作成
  - ボトムアップ実装順序決定
  - 並列実装可能性判断

技術的依存関係分析:
  - NuGetパッケージ依存関係確認
  - バージョン競合リスク分析
  - 循環参照検出

制約リスク分析:
  - F# Compilation Order制約分析
  - Entity Framework制約分析
  - Blazor Server制約分析
```

**選択判断基準**:
- ✅ 実装順序決定が必要
- ✅ 依存関係グラフ作成が必要
- ✅ NuGetパッケージ依存確認が必要
- ❌ 設計レベルの依存確認のみ（design-reviewの責務）

**成果物例**:
```markdown
## 実装順序決定

### Phase 1: 基盤実装（並列可能）
1. Domain層ValueObjects（依存なし）
2. Domain層Errors（依存なし）

### Phase 2: ドメインモデル実装（順次）
3. Domain層Entities（ValueObjects依存）
4. Domain層DomainServices（Entities依存）

### Phase 3: アプリケーション層（並列可能・Domain層完了後）
5. Application層UseCases
6. Application層ApplicationServices

### Phase 4: 境界・インフラ実装（並列可能）
7. Contracts層（F#↔C#境界）
8. Infrastructure層（Repository・DB）

### Phase 5: Web層実装
9. Web層（全層依存）
```

**制約リスク分析例**:
```yaml
F# Compilation Order制約:
  - リスク: Authentication → ProjectManagement前方参照
  - 対策: ファイル順序調整・共通型Common配下移動

Entity Framework制約:
  - リスク: Navigation Property循環参照
  - 対策: 単方向Navigation・DTO分離

Blazor Server制約:
  - リスク: SignalR接続タイムアウト
  - 対策: 再接続ロジック実装・セッション管理
```

**Phase B1実績**:
- Phase A1-A6実装順序決定（6 Phase並列実装判断）
- F# Compilation Order制約回避（Common配下共通型分離）

---

## 調査分析系Agent組み合わせパターン

### Pattern 1: 新機能実装Phase開始時

**組み合わせ**: spec-analysis → design-review → dependency-analysis

**理由**:
1. **spec-analysis**: 仕様要件抽出・仕様準拠マトリックス作成
2. **design-review**: 既存設計との整合性確認・レイヤー配置決定
3. **dependency-analysis**: 実装順序決定・並列実装判断

**Phase B2実績**: Phase B2 Step1-3で適用（E2Eテスト基盤構築）

---

### Pattern 2: 技術基盤刷新Phase開始時

**組み合わせ**: tech-research → dependency-analysis → design-review

**理由**:
1. **tech-research**: 新技術調査・ベストプラクティス収集
2. **dependency-analysis**: 既存依存関係への影響分析
3. **design-review**: アーキテクチャ整合性確認

**Phase B-F2想定**: 技術基盤整備・最適化パターン

---

### Pattern 3: テスト強化Phase開始時

**組み合わせ**: spec-analysis → tech-research

**理由**:
1. **spec-analysis**: テスト要件抽出・受け入れ基準明確化
2. **tech-research**: テストフレームワーク・パターン調査

**Phase B2実績**: Phase B2 Step1（Playwright調査・E2E要件抽出）

---

## 並列実行判断

### ✅ 並列実行可能な組み合わせ

**tech-research + spec-analysis**:
- **理由**: 技術調査と仕様分析は独立作業
- **条件**: 仕様書が技術選定に依存しない場合

**design-review + dependency-analysis**:
- **理由**: 両方とも読み取り専用（実装修正なし）
- **条件**: 依存関係分析が設計確認に影響しない場合

### ❌ 並列実行不可能な組み合わせ

**spec-analysis → design-review** (順次実行推奨):
- **理由**: 仕様要件が設計判断に影響

**tech-research → dependency-analysis** (順次実行推奨):
- **理由**: 技術選定が依存関係構造に影響

---

## 選択チェックリスト

### Step開始時

- [ ] 仕様書分析が必要か？ → spec-analysis
- [ ] 新技術調査が必要か？ → tech-research
- [ ] 設計整合性確認が必要か？ → design-review
- [ ] 実装順序決定が必要か？ → dependency-analysis

### Agent選択迷い時

- [ ] Gemini連携・Web情報収集が必要 → tech-research
- [ ] 仕様書（.md/.pdf）からの要件抽出が必要 → spec-analysis
- [ ] Clean Architecture準拠確認が必要 → design-review
- [ ] 依存グラフ・実装順序決定が必要 → dependency-analysis

---

**作成日**: 2025-11-01
**Phase B-F2 Step2**: Agent Skills Phase 2展開
**参照**: SubAgent組み合わせパターン.md、ADR_013
