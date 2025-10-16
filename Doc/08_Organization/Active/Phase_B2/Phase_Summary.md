# Phase B2 組織設計・総括

## 📊 Phase概要

- **Phase名**: Phase B2 (ユーザー・プロジェクト関連管理)
- **Phase規模**: 🟢中規模（マスタープランから自動取得）
- **Phase段階数**: 5段階（Step1, Step2, Step4, Step5, Step6）**※Step3スキップ決定**
- **Phase特性**: 新機能実装（多対多関連・権限制御拡張）
- **推定期間**: 5セッション（Playwright統合効果: 12-15時間削減）
- **開始予定日**: 2025-10-15
- **完了予定日**: 2025-10-19（推定）

## 🎯 Phase成功基準

- **機能要件**: UserProjects多対多関連実装・権限制御拡張（6→16パターン）・プロジェクトメンバー管理UI実装
- **品質要件**: 仕様準拠度95点以上維持・0 Warning/0 Error達成・テスト成功率100%達成
- **技術基盤**: Clean Architecture 96-97点品質維持・Phase B1確立基盤活用・Playwright MCP + Agents統合

## 📋 段階構成詳細（Step1分析結果反映版）

### 基本実装段階（1-2）
- **段階1 (Step1)**: 要件詳細分析・技術調査（Playwright調査含む）✅ 完了
- **段階2 (Step2)**: Playwright MCP統合（1 Stage）✅ 完了

### 機能実装段階（4）
- **段階4 (Step4)**: Application層・Infrastructure層実装（多対多関連）
  - **重要決定**: Step3（Domain層拡張）スキップ
  - **理由**: UserProjectsテーブル既存完了（Phase A）・ドメインロジックなし

### 品質保証段階（5-6）
- **段階5 (Step5)**: Web層実装・Phase B1技術負債解消・統合テスト
- **段階6 (Step6)**: Playwright E2Eテスト実装・統合効果検証（4 Stage構成）
  - **前提条件**: Step5（UI実装）完了必須

## 🏢 Phase組織設計方針

- **基本方針**: SubAgentプール活用・並列実行による効率化
- **主要SubAgent**: spec-analysis, tech-research, design-review, dependency-analysis, fsharp-domain, fsharp-application, csharp-infrastructure, csharp-web-ui, integration-test, spec-compliance
- **Step別組織構成概要**: Pattern A（新機能実装）適用・Domain層優先アプローチ・Step1は4Agent並列実行

## 🔧 技術基盤継承確認

### Phase B-F1完了成果（テストアーキテクチャ基盤）
- ✅ **7プロジェクト構成確立**（ADR_020準拠）
- ✅ **テストアーキテクチャ設計書作成**
- ✅ **新規テストプロジェクト作成ガイドライン作成**
- ✅ **Playwright MCP + Agents統合計画完成**（統合推奨度10/10点）

### Phase B1完了成果（プロジェクト基本CRUD）
- ✅ **Clean Architecture 96-97点品質確立**
- ✅ **F#↔C# Type Conversion 4パターン確立**（Result/Option/DU/Record）
- ✅ **bUnitテスト基盤構築**（BlazorComponentTestBase・FSharpTypeHelpers・ProjectManagementServiceMockBuilder）
- ✅ **Bounded Context分離**（4境界文脈: Common/Authentication/ProjectManagement/UbiquitousLanguageManagement）
- ✅ **namespace階層化**（ADR_019確立）

### Phase B2対応予定技術負債（Phase B1から引継ぎ・4件）
- **InputRadioGroup制約**（2件）: Blazor Server/bUnit既知の制約・Step5で対応
- **フォーム送信詳細テスト**（1件）: フォーム送信ロジック未トリガー・Step5で対応
- **Null参照警告**（1件）: ProjectManagementServiceMockBuilder.cs:206・Step5で対応

## 📋 全Step実行プロセス

### Step1: 要件詳細分析・技術調査（Playwright調査含む）
**推定時間**: 2-3時間

**実施内容**:
1. **仕様詳細分析（spec-analysis担当）**
   - UserProjects多対多関連の要件詳細分析
   - 権限制御マトリックス拡張計画（6→16パターン）
   - プロジェクトメンバー管理UI仕様確認
   - Phase B1技術負債4件の解消計画

2. **技術調査（tech-research担当）**
   - Playwright MCP + Agents技術調査
   - Phase B2申し送り事項の詳細確認
   - Playwright MCP最新状況確認（導入手順・.NET対応状況）
   - Playwright Agents最新状況確認（VS Code安定版対応・.NET実績）
   - セキュリティ・クレデンシャル管理方針策定

3. **設計整合性確認（design-review担当）**
   - Clean Architecture 96-97点品質維持確認
   - Phase B1/B-F1設計基盤との整合性確認
   - namespace階層化（ADR_019）準拠確認
   - テストアーキテクチャ（ADR_020）準拠確認

4. **依存関係・実装順序分析（dependency-analysis担当）**
   - UserProjectsテーブル設計確認・依存関係特定
   - Domain層・Application層への影響範囲特定
   - Infrastructure層・Web層への影響範囲特定
   - Step2-5実装順序の最適化

**SubAgent**: spec-analysis + tech-research + design-review + dependency-analysis（4Agent並列実行）

**成果物**:
- Phase B2要件詳細分析レポート
- Playwright統合実装計画（Step2用）
- 設計整合性レビューレポート
- 依存関係分析結果・実装順序計画
- Phase B2全体実装計画（統合版）

---

### Step2: Playwright MCP統合
**推定時間**: 5分
**実施内容**: Playwright MCP統合のみ（1 Stage）
**完了状況**: ✅ 完了（2025-10-16）

**実施内容**:
1. **Stage 1: Playwright MCP統合**（5分）
   - `claude mcp add playwright npx '@playwright/mcp@latest'`
   - Claude Code再起動・25ツール利用可能確認

**重要決定**: Stage 2-5（E2Eテスト作成・Agents統合・効果検証・ADR作成）は**Step6に移動**
- **理由**: E2Eテスト実装にはUI実装完了（Step5）が前提条件
- **移動先**: Phase B2 Step6（UI実装後に実施）

**SubAgent**: なし（MainAgentが直接実施）

**成果物**:
- ✅ Playwright MCP統合完了（~/.claude.json設定）
- ✅ 25ツール利用可能状態確立

---

### Step4: Application層・Infrastructure層実装（Step3統合版）
**推定時間**: 3-4時間

**実施内容**:
- **Infrastructure層実装**（1.5-2時間）
  - ProjectRepository拡張（6メソッド追加）
  - UserProjects多対多関連実装
  - ❌ EF Core Migration作成不要（UserProjectsテーブル既存）
- **Application層実装**（1.5-2時間）
  - IProjectManagementService拡張（4メソッド追加 + 4メソッド修正）
  - 権限制御マトリックス拡張（DomainApprover/GeneralUser追加）
  - Railway-oriented Programming適用
- **TDD Green Phase達成**

**SubAgent**: fsharp-application + csharp-infrastructure + unit-test（並列実行）

**重要決定**:
- Step3（Domain層拡張）をスキップし、本Stepに統合
- 理由: UserProjectsテーブル既存・ドメインロジックなし

---

### Step5: Web層実装・Phase B1技術負債解消
**推定時間**: 3-4時間

**実施内容**:
- プロジェクトメンバー管理UI実装（Blazor Server）
- **data-testid属性付与（E2Eテスト対応・Step6前提条件）**
  - **Phase B2新規画面**: メンバー管理画面（7要素）
  - **Phase A/B1実装済み画面**: E2Eテスト経路対応（Login画面3要素・Project一覧画面2要素）
- Phase B1技術負債4件解消（InputRadioGroup制約・フォーム送信詳細・Null警告）
- bUnitテスト追加（Phase B1基盤活用）
- 統合テスト・品質確認

**SubAgent**: csharp-web-ui + integration-test + spec-compliance（並列実行）

---

### Step6: Playwright E2Eテスト実装・統合効果検証
**推定時間**: 1.5-2時間
**前提条件**: Step5（UI実装）完了必須

**実施内容**（4 Stage構成）:
1. **Stage 1: E2Eテスト作成**（30分・MCPツール活用）
   - E2E.Testsプロジェクトにテスト作成
   - Claude CodeがMCPツールでブラウザ操作
   - UserProjectsシナリオE2Eテスト作成（3シナリオ）

2. **Stage 2: Playwright Agents統合**（15分）
   - Planner/Generator/Healer設定
   - 自動修復機能有効化
   - セキュリティ設定（.gitignore・テスト専用アカウント）

3. **Stage 3: 統合効果検証**（30分）
   - 作成効率測定（MCP使用 vs 従来手法）
   - メンテナンス効率測定（Agents活用）
   - 総合85%効率化検証

4. **Stage 4: ADR記録作成**（20分）
   - ADR_021: Playwright MCP + Agents統合戦略作成
   - 技術決定の永続化

**SubAgent**: integration-test + tech-research

**成果物**:
- E2E.Testsプロジェクト初期実装（3シナリオ）
- Playwright Agents統合完了
- ADR_021作成
- 効果測定レポート

---

## 📊 Step間成果物参照マトリックス

### Step1成果物の後続Step活用計画
| Step | 作業内容 | 必須参照（Step1成果物） | 重点参照セクション | 活用目的 |
|------|---------|----------------------|-------------------|---------|
| **Step2** | Playwright MCP統合 | `Tech_Research_Playwright_2025-10.md` | MCP統合手順（2章） | Playwright MCP統合実装 |
| **Step6** | E2Eテスト作成 | `Spec_Analysis_UserProjects.md` | UserProjects操作フロー（3.2節） | E2Eテストシナリオ作成 |
| **Step6** | Playwright Agents統合 | `Tech_Research_Playwright_2025-10.md` | Agents統合手順（3章） | Playwright Agents統合実装 |
| **Step6** | ADR_021作成 | `Tech_Research_Playwright_2025-10.md` | 技術決定根拠・効果測定 | 技術決定永続化 |
| **Step4** | Infrastructure層実装 | `Dependency_Analysis_UserProjects.md` | ProjectRepository拡張（3.1節） | Repository実装指針 |
| **Step4** | Infrastructure層実装 | `Spec_Analysis_UserProjects.md` | UserProjectsテーブル設計（1.1節） | テーブル構造・制約確認 |
| **Step4** | Application層実装 | `Spec_Analysis_UserProjects.md` | 権限制御マトリックス拡張（2.2節） | 権限判定ロジック実装 |
| **Step4** | Application層実装 | `Phase_B2_Implementation_Plan.md` | Step4実施内容詳細（3章） | 実装範囲・工数確認 |
| **Step5** | Web層実装 | `Spec_Analysis_UserProjects.md` | プロジェクトメンバー管理UI仕様（3章） | UI実装指針 |
| **Step5** | 技術負債解消 | `Spec_Analysis_UserProjects.md` | Phase B1技術負債解消計画（4章） | 技術負債対応方針 |
| **Step5** | 技術負債解消 | `Tech_Research_Playwright_2025-10.md` | Playwright E2Eテスト活用 | フォーム送信詳細テスト |
| **Step5** | 品質確認 | `Design_Review_PhaseB2.md` | Clean Architecture品質維持（1章） | 品質基準・評価指標 |
| **全Step** | 全体計画参照 | `Phase_B2_Implementation_Plan.md` | リスク管理計画（4章） | リスク要因・対策確認 |

### 成果物ファイル所在
**出力ディレクトリ**: `/Doc/08_Organization/Active/Phase_B2/Research/`
- `Spec_Analysis_UserProjects.md` - 要件・仕様詳細分析（UserProjects多対多関連）
- `Tech_Research_Playwright_2025-10.md` - Playwright MCP + Agents技術調査（2025年10月版）
- `Design_Review_PhaseB2.md` - 設計整合性レビュー（Clean Architecture品質維持）
- `Dependency_Analysis_UserProjects.md` - 依存関係分析・実装順序計画
- `Phase_B2_Implementation_Plan.md` - Phase B2全体実装計画（統合版）

**Phase B-F1成果物**（Playwright統合計画）:
- `/Doc/08_Organization/Completed/Phase_B-F1/Phase_B2_申し送り事項.md` - Step2実施時必須参照
- `/Doc/08_Organization/Completed/Phase_B-F1/Research/Playwright_MCP_評価レポート.md` - MCP統合技術詳細
- `/Doc/08_Organization/Completed/Phase_B-F1/Research/Playwright_Agents_評価レポート.md` - Agents統合技術詳細

---

## 📊 Phase総括レポート（Phase完了時記録）

[Phase完了時に更新予定]

---

**Phase作成日**: 2025-10-15
**Phase開始承認**: 取得予定
**Phase完了日**: 未定
**Phase責任者**: Claude Code
**Phase監督**: プロジェクトオーナー
