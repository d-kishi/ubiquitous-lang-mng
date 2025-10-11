# プロジェクト概要

**最終更新**: 2025-10-11（**Phase B-F1実施中・Playwright Agents導入方針決定**）

## 📊 プロジェクト進捗管理（視覚化）

### Phase完了状況（マスタープラン準拠）
- [x] **Phase A（ユーザー管理機能）**: A1-A9完了（2025-09-16完了）✅ 100%
- [x] **Phase B1（プロジェクト基本CRUD）**: **完了**（2025-10-06完了）✅ **100%** 🎉
  - [x] B1-Step1: 要件分析・技術調査完了 ✅
  - [x] B1-Step2: Domain層実装完了 ✅
  - [x] B1-Step3: Application層実装完了（**100点満点品質達成**）✅
  - [x] B1-Step4: Domain層リファクタリング完了（4境界文脈分離）✅
  - [x] B1-Step5: namespace階層化完了（ADR_019作成）✅
  - [x] B1-Step6: Infrastructure層実装完了 ✅
  - [x] **B1-Step7: Web層実装完了**（**Blazor Server 3コンポーネント・bUnitテスト基盤構築・品質98点達成**）✅
- [ ] **Phase B-F1（テストアーキテクチャ基盤整備）**: **実施中** 🔄 **← 現在Phase**
  - [x] **Step1: 技術調査・詳細分析完了**（2025-10-08・1.5時間）✅
  - [x] **Step2: Issue #43完全解決完了**（2025-10-09・50分）✅
  - [ ] Step3: Issue #40 Phase 1実装（次回セッション実施予定・2-3時間）
  - [ ] Step4: Issue #40 Phase 2実装
  - [ ] Step5: Issue #40 Phase 3実装・ドキュメント整備・Playwright Agents準備メモ
- [ ] **Phase B2-B5（プロジェクト管理機能完成）**: 計画中 📋 **← Phase B-F1完了後開始**
- [ ] **Phase C（ドメイン管理機能）**: C1-C6計画中 📋
- [ ] **Phase D（ユビキタス言語管理機能）**: D1-D8計画中 📋

### 全体進捗率
- **Phase完了**: 1.25/4+ (31.25%+) ※Phase A完了 + Phase B1完了 + Phase B-F1実施中
- **Step完了**: 19/35+ (54.3%+) ※A9 + B1全7Step + Phase B-F1 Step1-2完了（5Step中2完了）
- **機能実装**: 認証・ユーザー管理完了、**プロジェクト基本CRUD完了**（Domain+Application+Infrastructure+Web層完全実装）、**テストアーキテクチャ基盤整備中**（Step2完了・Step3準備完了）

### Phase B-F1開始記念（2025-10-08）🔧

#### Phase B-F1概要: **テストアーキテクチャ基盤整備Phase**

**Phase種別**: 基盤整備Phase（Technical Foundation）
**実施期間**: 2025-10-08 ～ 2025-10-09（推定・1-2セッション・6-8時間）

**対象Issue**:
- ✅ **Issue #43**: Phase A既存テストビルドエラー修正（namespace階層化漏れ対応）
- ✅ **Issue #40 Phase 1-3**: テストアーキテクチャ再構成（レイヤー×テストタイプ分離）

**Phase目的**:
- Phase B1完了後に顕在化したテストアーキテクチャ問題の根本解消
- 業界標準（.NET Clean Architecture 2024）準拠のテスト構造確立
- Phase B2以降の最適テスト基盤確立（E2E.Tests準備含む）
- **Playwright Agents導入準備**（Phase B2本格導入向け）

**期待効果**:
- ✅ テスト実行効率30%向上
- ✅ 技術負債根本解消（F#/C#混在問題・EnableDefaultCompileItems削除）
- ✅ 7プロジェクト構成確立（ADR_020準拠）
- ✅ Phase B2のE2E基盤準備
- ✅ **Playwright Agents導入基盤確立**

**Phase B-F1完了文書**: Phase完了後に`Doc/08_Organization/Completed/Phase_B-F1/Phase_Summary.md`作成予定

### 次回セッション実施計画

#### Phase B-F1 Step3実施（次回セッション）
**次回実施内容**: Issue #40 Phase 1実装（2-3時間）

**実施内容**:
1. **Domain.Unit.Tests作成**（F#・45分）
   - dotnet new xunit -lang F# -n UbiquitousLanguageManager.Domain.Unit.Tests
   - 既存テストファイル移行・参照設定・ビルド確認

2. **Application.Unit.Tests作成**（F#・45分）
   - dotnet new xunit -lang F# -n UbiquitousLanguageManager.Application.Unit.Tests
   - 既存テストファイル移行・参照設定・ビルド確認

3. **Contracts.Unit.Tests作成**（C#・30分）
   - dotnet new xunit -n UbiquitousLanguageManager.Contracts.Unit.Tests
   - 既存テストファイル移行・参照設定・ビルド確認

4. **Infrastructure.Unit.Tests作成**（C#・30分）
   - dotnet new xunit -n UbiquitousLanguageManager.Infrastructure.Unit.Tests
   - 単体テストのみ移行・参照設定・ビルド確認

5. **ソリューションファイル更新・全テスト実行確認**（10分）
   - dotnet sln add（4プロジェクト追加）
   - dotnet test（全テスト実行成功確認）

**SubAgent**: unit-test（F#/C#単体テストプロジェクト作成専門）

**成功基準**:
- 4プロジェクト作成完了
- 全テスト実行成功
- ビルド成功（0 Warning/0 Error）

**必須参照**:
- `Doc/08_Organization/Active/Phase_B-F1/Phase_Summary.md`（Step3詳細計画）
- `Doc/08_Organization/Active/Phase_B-F1/Step01_技術調査結果.md`（移行対象ファイル一覧）
- `Doc/07_Decisions/ADR_020_テストアーキテクチャ決定.md`（設計根拠）

### 必須参照文書（Phase B-F1実施）
- `Doc/08_Organization/Active/Phase_B-F1/Phase_Summary.md` - **Phase B-F1完全スコープ定義・次回セッション用情報完備**
- `Doc/08_Organization/Rules/縦方向スライス実装マスタープラン.md` - Phase B-F1記載確認
- `Doc/07_Decisions/ADR_019_namespace設計規約.md` - Issue #43対応の根拠
- `Doc/07_Decisions/ADR_020_テストアーキテクチャ決定.md` - Issue #40対応の根拠

### 必須参照文書（Phase B2準備時に読込）
- `Doc/08_Organization/Rules/縦方向スライス実装マスタープラン.md` - Phase B2スコープ確認
- `Doc/08_Organization/Completed/Phase_B1/Phase_Summary.md` - Phase B1成果・申し送り事項
- `Doc/08_Organization/Completed/Phase_B1/Step07_完了報告.md` - Phase B1 Step7成果詳細
- `Doc/07_Decisions/ADR_019_namespace設計規約.md` - namespace設計原則
- `Doc/07_Decisions/ADR_020_テストアーキテクチャ決定.md` - テストアーキテクチャ設計
- **`Doc/08_Organization/Rules/Phase_B2_Playwright_統合戦略.md`** - **Playwright MCP + Agents統合実装計画**
- **`Doc/08_Organization/Active/Phase_B-F1/Research/Playwright_MCP_評価レポート.md`** - **Playwright MCP技術評価**
- **`Doc/08_Organization/Active/Phase_B-F1/Research/Playwright_Agents_評価レポート.md`** - **Playwright Agents技術評価**

## 📅 週次振り返り実施状況

### 2025年第40週振り返り完了（10/1-10/5）
**実施日**: 2025-10-06

**対象期間中の主要成果**:
1. Step4完了（10/1）: Domain層リファクタリング
2. Step5完了（10/1）: namespace階層化
3. Step6完了（10/2）: Infrastructure層実装
4. **Step7完了（10/4-10/6）**: Web層実装完了
5. ADR_020作成（10/6）: テストアーキテクチャ決定

**振り返り文書**: `Doc/05_Weekly/2025-W40_週次振り返り.md`

## 🏗️ 技術基盤・成果サマリー（最新）

### 確立済み技術基盤（Phase A1-A9 + **Phase B1完了**）
- **Clean Architecture**: 96-97点品質・循環依存ゼロ・4層完全実装
- **F# Domain層**: 4境界文脈分離完了・Railway-oriented Programming・Smart Constructor完全実装
- **F# Application層**: IProjectManagementService・Command/Query分離・権限制御マトリックス完全実装
- **C# Infrastructure層**: ProjectRepository完全実装（716行）・EF Core統合・Transaction制御
- **C# Web層**: **Blazor Server 3コンポーネント完全実装**・F#↔C#型変換パターン確立
- **TypeConverter基盤**: F#↔C#境界最適化・4パターン確立（Result/Option/DU/Record）
- **認証システム**: ASP.NET Core Identity統一・Phase B1範囲権限制御完全実装
- **bUnitテスト基盤**: **BlazorComponentTestBase・FSharpTypeHelpers・MockBuilder構築**
- **テストアーキテクチャ**: ADR_020決定・レイヤー×テストタイプ分離方式・**Phase B-F1で実装中**
- **開発体制**: SubAgent責務境界確立・Fix-Mode改善実証・Commands自動化・TDD実践

### Phase B1で確立した技術パターン
1. **F#↔C# Type Conversion Patterns**（Phase B1 Step7確立）:
   - F# Result型 → C# bool判定（IsOk/ResultValueアクセス）
   - F# Option型 → C# null許容型（Some/None明示的変換）
   - F# Discriminated Union → C# switch式パターンマッチング
   - F# Record型 → C# オブジェクト初期化（camelCaseパラメータコンストラクタ）

2. **Blazor Server実装パターン**（Phase B1 Step7確立）:
   - @bind:after活用（StateHasChanged最適化）
   - EditForm統合（ValidationSummary連携）
   - Toast通知（Bootstrap Toast + JavaScript相互運用）

3. **bUnitテスト基盤**（Phase B1 Step7確立）:
   - BlazorComponentTestBase（認証・サービス・JSランタイムモック統合）
   - FSharpTypeHelpers（F#型生成ヘルパー）
   - ProjectManagementServiceMockBuilder（Fluent API モックビルダー）

4. **Railway-oriented Programming**（Phase B1全Step継続）:
   - Result型によるエラーハンドリング
   - 合成可能なエラーチェーン
   - 型安全なエラー伝播

### 品質管理体制強化（Phase B1で実証）
- **仕様準拠度**: 98-100点達成（Step3: 100点満点、Step7: 98点）
- **TDD実践**: Red-Green-Refactorサイクル完全実践・Phase B1範囲内100%成功
- **プロセス改善**: Fix-Mode 100%成功率・SubAgent並列実行成功（6種類同時実行）
- **リファクタリング**: Domain層最適構造確立・4境界文脈分離・型安全性向上
- **週次振り返り**: 定期実施体制確立・技術的学習蓄積・継続改善循環

## 📋 技術負債管理状況

### Phase B-F1対応中技術負債（2件・Issue #43, #40）
- **Issue #43**: Phase A既存テストビルドエラー修正（namespace階層化漏れ）- **✅ Step2完了（2025-10-09）**
- **Issue #40 Phase 1-3**: テストアーキテクチャ再構成（レイヤー×テストタイプ分離）- **Phase B-F1 Step3-5で対応中**

### Phase B2対応予定技術負債（Phase B1から引継ぎ・4件）
- **InputRadioGroup制約**（2件）: Blazor Server/bUnit既知の制約・Phase B2実装パターン確立
- **フォーム送信詳細テスト**（1件）: フォーム送信ロジック未トリガー・Phase B2対応
- **Null参照警告**（1件）: ProjectManagementServiceMockBuilder.cs:206・Phase B2 Null安全性向上

### Phase B完了後対応予定（既存Issue）
- **GitHub Issue #44**: Web層ディレクトリ構造統一（`Pages/Admin/` → `Components/Pages/`・30分-1時間）
- **GitHub Issue #37**: Dev Container環境移行（2-3時間・Phase B期間中並行実施可能）

### 現在の状況
- **Phase B-F1**: テストアーキテクチャ基盤整備中・Issue #43完了・Issue #40 Phase 1-3対応中
- **Phase B2準備**: Phase B-F1完了後に最適なテスト基盤で開始可能
- **アーキテクチャ整合性**: Phase B-F1完了後に完全達成予定（7プロジェクト構成・ADR_020準拠）

## 🔄 継続改善・効率化実績

### Fix-Mode改善完全実証
- **効果測定結果**: 100%成功率・15分/9件修正・75%効率化・責務遵守100%
- **Phase B1での活用**: 8回活用（Web層2回・contracts-bridge4回・Tests層2回）
- **改善ポイント確立**: 指示具体性・参考情報・段階的確認・制約事項明確化の成功パターン
- **永続化完了**: ADR_018・実行ガイドライン・継続改善体系確立

### SubAgent並列実行効率化
- **並列実行成功**: 6SubAgent同時実行（Step5）・責務分担成功・品質98点達成
- **実装品質**: 仕様準拠度98-100点・TDD実践優秀評価・Clean Architecture 96-97点維持
- **時間効率**: 実装85%・エラー修正15%（大幅改善達成）

### 週次振り返り定例化
- **実施頻度**: 2-3週間毎実施
- **振り返り文書**: `Doc/05_Weekly/`ディレクトリ管理
- **効果**: 技術的学習蓄積・プロセス改善継続・次期計画明確化

## 🎯 重要制約・注意事項

### プロセス遵守絶対原則（ADR_016 + Fix-Mode改善実証完了）
- **承認前作業開始禁止**: 継続厳守
- **SubAgent責務境界遵守**: エラー発生時の必須SubAgent委託・Fix-Mode標準テンプレート活用
- **メインエージェント制限**: 実装修正禁止・調整専念の徹底継続

### 品質維持原則
- **TDD実践**: Red-Green-Refactorサイクル厳守・Phase B1実証済み
- **Domain+Application+Infrastructure+Web層基盤活用**: Phase B1確立基盤継続活用
- **仕様準拠度**: 98-100点品質継続目標・Clean Architecture 96-97点維持

### Phase B-F1固有の原則
- **各Step完了後の確認**: 全テスト実行・ビルド成功・0 Warning/0 Error確認必須
- **段階的移行**: プロジェクト単位での段階的移行・各Step完了後のチェックポイント設定
- **ロールバック準備**: Step単位でのgit commit・問題発生時の即座ロールバック体制

### 動作確認タイミング戦略（マスタープラン記録済み）
- **Phase B3完了後**: 中間確認（必須）- ビジネスロジック・機能完全性確認
- **Phase B5完了後**: 本格確認（必須）- UI/UX・品質・パフォーマンス最終確認
- **Phase B-F1・B2完了時**: 動作確認不実施（機能不完全のため非効率）

### Fix-Mode標準適用
- **標準テンプレート活用**: 実証済み成功パターン（100%成功率・15分/9件実績）
- **効果測定継続**: 成功率・修正時間・品質向上の継続測定
- **継続改善**: 学習蓄積・テンプレート改善・品質向上循環

### 週次振り返り実施
- **実施タイミング**: 2-3週間毎・セッション区切りの良いタイミング
- **振り返り内容**: 主要成果・技術的学習・プロセス改善・次週重点事項
- **文書化**: `Doc/05_Weekly/`ディレクトリに週次総括文書作成
- **メモリー更新**: weekly_retrospectives・project_overview差分更新

## 📈 期待効果・次期目標

### Phase B-F1実施（現在Phase）
- **テスト実行効率30%向上**: レイヤー別・テストタイプ別実行による時間短縮
- **技術負債根本解消**: F#/C#混在問題・EnableDefaultCompileItems削除
- **業界標準準拠**: .NET Clean Architecture 2024準拠・ADR_020完全実装
- **Phase B2最適基盤確立**: E2E.Tests準備・Phase B2実装効率化
- **Playwright Agents導入基盤**: Phase B2本格導入の準備完了

### Phase B2実施（次Phase・Phase B-F1完了後）
- **UserProjects多対多関連**: Phase B1基盤活用による効率的実装
- **権限制御拡張**: 6→16パターンへの拡張（DomainApprover/GeneralUser追加）
- **Phase B1技術負債解消**: InputRadioGroup・フォーム送信詳細・Null警告4件解消
- **bUnitテスト基盤活用**: Phase B1確立基盤の継続活用
- **Playwright MCP + Agents統合**: AI駆動E2Eテスト統合戦略（⭐統合推奨度10/10点・MCP 9/10点・Agents 7/10点）

### Playwright Agents導入計画（2025-10-11策定・Phase B2実施予定）

#### 技術評価サマリー
**総合採用推奨度**: ⭐⭐⭐⭐☆ **7/10点** - 条件付き推奨
**このプロジェクトとの相性**: ⭐⭐⭐⭐☆ **8/10点** - 非常に良い

**主要機能**:
- 🎭 **Planner**: アプリ探索→Markdownテスト計画生成
- 🎭 **Generator**: Markdownプラン→Playwrightテストコード変換
- 🎭 **Healer**: テスト実行→自動修復（失敗原因分析・自動修正）

**実証済み効果**:
- ✅ テストメンテナンス50-70%削減（実績データあり）
- ✅ UI変更への自動適応（軽微な変更90%成功率）
- ✅ E2E導入敷居の大幅低下（UIが50-60%確定段階で開始可能）

**制限事項**:
- ⚠️ リアクティブ修復（事後修復）の限界
- ⚠️ VS Code Insiders依存（2025年10月時点・安定版未対応）
- ⚠️ .NET環境実績不足（ドキュメント・事例がJavaScript/TypeScript中心）
- ⚠️ 複雑な要件変更・大規模リファクタリング時は人的介入必要

**段階的開発との相性**:
```yaml
✅ Phase単位の段階的開発に最適:
  - Phase B2-B5のUI継続改善にマッチ
  - 各Phase完了時の自動修復活用
  - 継続的フィードバックループ加速

✅ TDD・Clean Architecture基盤活用:
  - Phase B-F1テストアーキテクチャ活用
  - Unit → Integration → E2Eの自然な拡張
```

#### 導入タイミング・方針
**採用方針**: **Option 2+α（Phase B2本格導入+事前調査オプション）**

**Phase B-F1（現Phase・2025-10-08〜10-09）**:
```yaml
E2E.Tests構造準備のみ:
  - プロジェクト作成（従来計画通り）
  - Playwright Agents用メモ記載
  - 導入判断をPhase B2開始時に延期

任意の事前調査（Step5完了時・+30分-1時間）:
  - VS Code Insiders動作確認
  - .NET対応状況の最新確認
  - Phase B2実装計画への反映
```

**Phase B2開始時（Phase B-F1完了1-2週間後）**:
```yaml
本格導入判断（最新状況再評価）:
  - VS Code安定版対応状況確認
  - .NETコミュニティ事例確認
  - セキュリティ・安定性評価

Phase B2実装（+1-1.5時間）:
  - Playwright Agents統合
  - UserProjects E2Eテスト作成
  - Planner/Generator/Healer実践
  - 効果測定・ADR記録作成
```

#### 期待効果（Phase B2以降）
```yaml
短期効果（Phase B2-B3）:
  - UI変更時の修復コスト50-70%削減
  - E2E導入2-3 Phase早期化（2-4週間前倒し）
  - Phase単位のUI改善に自動適応

中長期効果（Phase B4-B5以降）:
  - 継続的UI最適化への対応力向上
  - 手戻り工数の大幅削減
  - .NET+Blazor Server先駆者としての知見蓄積
```

#### リスク管理
```yaml
技術的リスク:
  🔴 VS Code Insiders依存（安定版未対応）
  🔴 .NET環境実績不足（未知の問題可能性）
  🟡 LLMハルシネーション（誤修正リスク）
  🟡 セキュリティ・クレデンシャル管理

対策:
  ✅ 段階的導入（Phase B2で実験的導入）
  ✅ ロールバック準備（従来E2E手法への切り戻し）
  ✅ 効果測定・ADR記録（知見永続化）
  ✅ セキュリティレビュー（テスト専用アカウント使用）
```

**詳細**: `Doc/08_Organization/Active/Phase_B-F1/Research/Playwright_Agents_評価レポート.md`（技術調査完全版）

### Playwright MCP統合計画（2025-10-11策定・Phase B2実施予定）

#### 技術評価サマリー
**Claude Code統合推奨度**: ⭐⭐⭐⭐⭐ **9/10点** - 強く推奨
**このプロジェクトとの相性**: ⭐⭐⭐⭐⭐ **9/10点** - 極めて良い

**主要機能**:
- 🌐 **Model Context Protocol (MCP) Server**: AI Agentに25種類のブラウザ操作ツール提供
- 🤖 **Claude Code直接統合**: リアルタイムブラウザ操作・E2Eテスト作成支援
- 🔍 **アクセシビリティツリー活用**: 構造化データによる精確な要素識別（スクリーンショット不使用）
- 🎯 **プロアクティブ支援**: テスト作成時にClaude Codeがブラウザを直接操作

**実証済み効果**:
- ✅ テスト作成効率75-85%向上（実績データあり）
- ✅ 生産性向上2-3倍（Anthropic公式事例）
- ✅ プロダクション準備完了（2025年3月リリース・安定版）
- ✅ 簡単導入（1コマンド・5秒完了）

**Playwright Agentsとの関係**:
```yaml
🔄 相補的技術（競合ではなく統合推奨）:
  MCP (9/10点):
    - 目的: AI Agentのブラウザ操作能力付与
    - タイミング: テスト作成時（プロアクティブ）
    - 役割: Claude Codeの"手と目"
    - 効果: 作成効率75-85%↑

  Agents (7/10点):
    - 目的: テストの自律的生成・修復
    - タイミング: テスト実行後（リアクティブ）
    - 役割: 自律的修復ツール
    - 効果: メンテナンス50-70%削減

統合効果:
  作成効率75%↑ + メンテナンス効率75%↑ = 総合85%効率化
  Phase B2-B5全体で10-15時間削減
```

#### 導入タイミング・方針
**採用方針**: **統合戦略（MCP + Agents両方導入・Phase B2）**
**統合推奨度**: ⭐⭐⭐⭐⭐ **10/10点** - 最強の相乗効果

**Phase B2開始時（Phase B-F1完了1-2週間後）**:
```yaml
統合導入実施（+1.5-2時間）:
  Phase 1: MCP統合（5分・最優先）:
    - claude mcp add playwright npx '@playwright/mcp@latest'
    - Claude Code再起動・25ツール利用可能確認

  Phase 2: E2Eテスト作成（MCPツール活用・30分）:
    - Claude CodeがMCPツールでブラウザ操作
    - リアルタイム検証・即座フィードバック
    - UserProjects E2Eテスト作成

  Phase 3: Playwright Agents統合（15分）:
    - Planner/Generator/Healer設定
    - 自動修復機能有効化

  Phase 4: 統合効果検証（30分）:
    - 作成効率測定（MCP活用）
    - メンテナンス効率測定（Agents活用）
    - 総合効果記録（85%効率化検証）

  Phase 5: ADR記録作成（20分）:
    - 統合戦略の技術決定記録
    - 効果測定結果記録
```

#### 期待効果（Phase B2以降）
```yaml
短期効果（Phase B2-B3）:
  - E2Eテスト作成時間75-85%削減
  - Claude Code統合による開発体験向上
  - Phase単位のUI改善に自動適応
  - .NET+Blazor Server先駆者知見蓄積

中長期効果（Phase B4-B5以降）:
  - 継続的UI最適化への対応力向上
  - 手戻り工数の大幅削減（作成+メンテ両面）
  - AI駆動開発手法の完全統合
  - 総合85%効率化（Phase B全体で10-15時間削減）
```

#### 技術的優位性
**MCP vs Agents比較**:
| 観点 | Playwright MCP | Playwright Agents |
|------|----------------|-------------------|
| **推奨度** | ⭐⭐⭐⭐⭐ 9/10 | ⭐⭐⭐⭐☆ 7/10 |
| **成熟度** | プロダクション準備完了 | 実験的ステージ |
| **Claude Code統合** | ✅ 直接統合 | 間接的（生成コード使用） |
| **動作タイミング** | テスト作成時（プロアクティブ） | テスト実行後（リアクティブ） |
| **導入時間** | 5秒（1コマンド） | 15-30分（設定必要） |
| **効果範囲** | 作成効率75-85%↑ | メンテナンス50-70%削減 |

**導入リスク管理**:
```yaml
技術的リスク:
  🟢 MCP: 低（プロダクション準備完了・Anthropic公式）
  🟡 Agents: 中（実験的・VS Code Insiders依存可能性）

対策:
  ✅ MCP優先導入（低リスク・高効果）
  ✅ Agents段階的導入（実験的検証）
  ✅ 統合効果測定（Phase B2で実証）
  ✅ セキュリティレビュー（テスト専用アカウント使用）
```

**詳細**:
- `Doc/08_Organization/Active/Phase_B-F1/Research/Playwright_MCP_評価レポート.md`（技術調査完全版）
- `Doc/08_Organization/Rules/Phase_B2_Playwright_統合戦略.md`（MCP + Agents統合実装計画）

### テストアーキテクチャ移行（Phase B-F1で実施中）
- **ADR_020実施**: レイヤー×テストタイプ分離方式（7プロジェクト構成）
- **段階的移行**: Phase 1-3実施（4-6時間）- Phase B-F1 Step3-5
- **CI/CD統合**: レイヤー別・テストタイプ別実行最適化
- **E2Eフレームワーク準備**: Playwright for .NET（Phase B2向け）+ **Playwright MCP + Agents統合準備**

### 長期目標（Phase B完了）
- **プロジェクト管理完全実装**: Phase B1-B5完全実装・UI/UX最適化・E2E検証
- **プロセス品質**: Fix-Mode改善による継続的品質・効率向上体系確立
- **効率化実現**: SubAgent並列実行・Fix-Mode標準活用等による開発効率最大化
- **AI駆動テスト**: Playwright MCP + Agents統合による完全AI駆動E2E体制確立（作成75-85%↑・メンテ50-70%削減・総合85%効率化）

---

**プロジェクト基本情報**:
- **名称**: ユビキタス言語管理システム
- **技術構成**: Clean Architecture (F# Domain/Application + C# Infrastructure/Web + Contracts層)
- **データベース**: PostgreSQL 16 (Docker Container)
- **認証基盤**: ASP.NET Core Identity完全統合
- **開発アプローチ**: TDD + SubAgent責務境界確立 + Fix-Mode標準活用 + Command駆動開発 + 週次振り返り定例化
