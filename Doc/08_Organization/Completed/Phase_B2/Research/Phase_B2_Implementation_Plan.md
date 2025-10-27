# Phase B2 全体実装計画（統合版）

**作成日**: 2025-10-15
**作成基盤**: Step1 4Agent並列分析結果統合
**分析担当**: spec-analysis + tech-research + design-review + dependency-analysis
**計画承認**: 要ユーザー承認

---

## 🎯 1. Phase B2全体目標・成功基準

### Phase B2目標
- **機能要件**: UserProjects多対多関連実装・権限制御拡張（6→16パターン）・プロジェクトメンバー管理UI実装
- **品質要件**: 仕様準拠度95点以上維持・0 Warning/0 Error達成・テスト成功率100%達成
- **技術基盤**: Clean Architecture 96-97点品質維持・Phase B1確立基盤活用・Playwright MCP + Agents統合

### Phase B2成功基準（定量）
- ✅ **仕様準拠度**: 95点以上（Phase B1実績: 98-100点）
- ✅ **Clean Architecture品質**: 96-97点維持（Phase B1実績: 96-97点）
- ✅ **ビルド品質**: 0 Warning / 0 Error（製品コード）
- ✅ **テスト成功率**: 100%（Phase B1範囲内: 99.1% → 100%）
- ✅ **権限制御マトリックス**: 16パターン完全実装（Phase B1: 6パターン）
- ✅ **技術負債解消**: Phase B1技術負債4件完全解消

---

## 🔍 2. 4Agent分析結果統合・重要発見事項

### 2.1 重大な技術決定（3件）

#### 決定1: Step3（Domain層拡張）スキップ確定

**根拠** (dependency-analysis Agent):
- UserProjectsテーブルは**Phase Aで完全実装済み**
  - EF Core Entity: UserProject.cs (60行)
  - DbContext設定: 行328-372
  - Migration完了: 20250729153117_FinalInitMigrationWithComments.cs
  - 外部キー制約3件・インデックス4件設定済み
- UserProject集約は**多対多関連の中間テーブルのみ**
  - ドメインロジックなし
  - ビジネスルールはApplication層で実装
  - 既存Project集約に変更なし

**影響**:
- Step3をスキップし、Step4（Application/Infrastructure層実装）に統合
- 実装期間: 2-3時間削減
- Phase B2段階構成: 5段階 → **4段階**（Step1, Step2, Step4, Step5）

**リスク**: なし（UserProjectsテーブル完全実装確認済み）

#### 決定2: Playwright Agents推奨度向上（7/10 → 9/10）

**根拠** (tech-research Agent):
- **VS Code 1.105.0安定版リリース**（2025-10-10）
  - Insiders依存リスク完全解消
  - Playwright Agents Extensionの安定版対応確認
- **Playwright MCP最新版**: 0.0.42（2025-10-09）
  - .NET 8.0完全対応
  - 25ツール利用可能
  - セキュリティリスク: 低レベル
- **統合推奨度**: 10/10点維持

**期待効果** (Phase B2実績予測):
- **総合効率化**: 85%（MCP: 70% + Agents: 15%）
- **E2Eテスト作成時間**: 2-3時間 → 30分（85%削減）
- **E2Eテスト保守時間**: 1-2時間 → 15分（90%削減）
- **Phase B2総時間削減**: 12-15時間削減見込み

**リスク**:
- Agents自動修復精度: 80-85%（手動検証必須15-20%）
- CI/CD環境での実行: GitHub Actions対応必要

#### 決定3: Clean Architecture 96-97点品質維持確定

**根拠** (design-review Agent):
- **Phase B1品質レベル**: 96-97点 (A+ Excellent)
- **Phase B2品質維持戦略**:
  - Domain層品質維持: 25点満点（Phase B2影響なし）
  - Application層品質維持: 20点満点（軽微な拡張のみ）
  - Contracts層品質維持: 15点満点（既存4パターンで対応可能）
  - Infrastructure層品質維持: 20点満点（UserProjects既存Entity活用）
  - Web層品質維持: 20点満点（bUnit基盤95%再利用）

**Phase B1/B-F1設計基盤整合性**:
- ✅ Bounded Context分離（4境界文脈）維持
- ✅ F#↔C#型変換システム: 既存4パターンで全要件対応可能
- ✅ bUnitテスト基盤: 95%再利用可能
- ✅ namespace階層化（ADR_019）完全準拠
- ✅ テストアーキテクチャ（ADR_020）完全準拠
- ✅ 新規テストプロジェクト作成不要

**リスク**: 低（Phase B1確立基盤を完全活用）

### 2.2 実装範囲詳細確認（spec-analysis Agent）

#### UserProjects多対多関連実装
| 項目 | 内容 | 実装レイヤー |
|------|------|------------|
| テーブル設計 | 既存完了（Phase A） | - |
| Repository拡張 | 6メソッド追加 | Infrastructure層 |
| Application Service拡張 | 4メソッド追加 + 4メソッド修正 | Application層 |
| UI実装 | 3コンポーネント新規 | Web層 |

#### 権限制御マトリックス拡張（6 → 16パターン）
| ロール | Phase B1 | Phase B2追加 | 合計 |
|--------|---------|------------|------|
| SuperUser | 4パターン | 2パターン | 6パターン |
| ProjectManager | 2パターン | 2パターン | 4パターン |
| DomainApprover | 0パターン | 3パターン | 3パターン |
| GeneralUser | 0パターン | 3パターン | 3パターン |
| **合計** | **6パターン** | **10パターン** | **16パターン** |

#### Phase B1技術負債解消（4件）
| 負債項目 | Phase B2対応方針 | 推定工数 |
|---------|----------------|---------|
| InputRadioGroup制約（2件） | カスタムラジオボタンコンポーネント実装 | 1.5時間 |
| フォーム送信詳細テスト（1件） | Playwright E2Eテスト + bUnit部分統合 | 1.5時間 |
| Null参照警告（1件） | Null許容参照型 + デフォルト値明示 | 0.5時間 |

---

## 📋 3. Step2-5詳細作業内容確定版

### Step2: Playwright MCP + Agents統合実装（1.5-2時間）

**5 Stage構成** (Phase B-F1申し送り事項準拠):

#### Stage 1: Playwright MCP統合（5分・最優先）
```bash
claude mcp add playwright npx '@playwright/mcp@latest'
```
- Claude Code再起動
- 25ツール利用可能確認
- mcp__playwright__ プレフィックスツール確認

#### Stage 2: E2Eテスト作成（30分・MCPツール活用）
- E2E.Testsプロジェクトにテスト作成
  - UserProjects追加シナリオ
  - UserProjects削除シナリオ
  - 権限制御マトリックス検証
- Claude CodeがMCPツールでブラウザ操作

#### Stage 3: Playwright Agents統合（15分）
- Planner/Generator/Healer設定
- VS Code設定ファイル更新
- 自動修復機能有効化確認

#### Stage 4: 統合効果検証（30分）
- 作成効率測定（MCP使用 vs 従来手法）
- メンテナンス効率測定（Agents活用）
- 総合85%効率化検証

#### Stage 5: ADR記録作成（20分）
- ADR_021: Playwright MCP + Agents統合戦略作成
- 技術決定の永続化

**SubAgent**: integration-test + tech-research（並列実行）

**成果物**:
- Playwright MCP統合完了
- E2E.Testsプロジェクト初期実装（3-5テストケース）
- ADR_021作成
- 効果測定レポート

---

### Step4: Application層・Infrastructure層実装（3-4時間）

**実施内容** (Step3スキップによる統合版):

#### Infrastructure層実装（1.5-2時間）
**ProjectRepository拡張** (6メソッド追加):
1. `AddUserToProjectAsync` - UserProjects追加
2. `RemoveUserFromProjectAsync` - UserProjects削除
3. `GetProjectMembersAsync` - メンバー一覧取得（Eager Loading徹底）
4. `IsUserProjectMemberAsync` - メンバー判定
5. `GetProjectMemberCountAsync` - メンバー数取得
6. `SaveProjectWithDefaultDomainAndOwnerAsync` - プロジェクト作成時Owner自動追加

**既存メソッド修正** (2メソッド):
- `GetProjectsByUserAsync` - UserProjects JOIN追加
- `GetRelatedDataCountAsync` - UserProjectsカウント追加

**EF Core Migration**:
- ❌ **新規Migration作成不要**（UserProjectsテーブル既存）

#### Application層実装（1.5-2時間）
**IProjectManagementService拡張** (4メソッド追加):
1. `AddMemberToProjectAsync` - メンバー追加
2. `RemoveMemberFromProjectAsync` - メンバー削除
3. `GetProjectMembersAsync` - メンバー一覧取得
4. `IsUserProjectMemberAsync` - メンバー判定

**既存メソッド修正** (4メソッド):
1. `CreateProjectAsync` - Owner自動UserProjects追加
2. `DeleteProjectAsync` - UserProjects関連データ追加
3. `GetProjectsAsync` - DomainApprover/GeneralUser権限拡張
4. `GetProjectDetailAsync` - UserCount実装

**権限制御マトリックス拡張**:
- DomainApprover権限追加（3パターン）
- GeneralUser権限追加（3パターン）
- Railway-oriented Programming適用

**SubAgent**: fsharp-application + csharp-infrastructure + unit-test（並列実行）

**成果物**:
- IProjectManagementService拡張完了
- ProjectRepository拡張完了
- 権限制御マトリックス16パターン実装完了
- TDD Green Phase達成
- 単体テスト追加（10-15件）

**重点事項**:
- N+1問題防止（Include()パターン徹底）
- CASCADE DELETE活用（論理削除実装活用）
- 複合一意制約違反チェック（既存チェック実装活用）

---

### Step5: Web層実装・Phase B1技術負債解消（3-4時間）

**実施内容** (3 Phase構成):

#### Phase 1: プロジェクトメンバー管理UI実装（1.5-2時間）

**新規コンポーネント** (3コンポーネント):
1. `ProjectMembers.razor` - メンバー管理画面メイン
   - 権限制御UI（SuperUser/ProjectManager専用）
   - 状態管理（メンバー追加・削除）
   - エラーハンドリング（重複追加・最後の管理者削除防止）
2. `ProjectMemberSelector.razor` - メンバー選択ドロップダウン
3. `ProjectMemberCard.razor` - メンバー情報カード

**既存コンポーネント拡張**:
- `ProjectEdit.razor` - メンバー管理画面へのリンク追加

#### Phase 2: Phase B1技術負債解消（1-1.5時間）

**InputRadioGroup制約解消** (2件・1時間):
- カスタムラジオボタンコンポーネント実装（30分）
- 既存画面への適用（20分）
- bUnitテスト実装（10分）

**フォーム送信詳細テスト** (1件・30分):
- Playwright E2Eテスト実装（20分）
- bUnit部分統合テスト実装（10分）

**Null参照警告** (1件・15分):
- Null許容参照型有効化（5分）
- Null警告解消（10分）

#### Phase 3: 統合テスト・品質確認（30-45分）

- bUnitテスト追加（10-15件）
- E2Eテスト実行（Playwright Agents活用）
- 統合テスト実行
- ビルド品質確認（0 Warning / 0 Error）
- テスト成功率確認（100%）

**SubAgent**: csharp-web-ui + integration-test + spec-compliance（並列実行）

**成果物**:
- プロジェクトメンバー管理UI完成
- Phase B1技術負債4件完全解消
- bUnitテスト追加（10-15件）
- E2Eテスト追加（5-10件）
- 統合テスト完了
- 品質確認完了（96-97点維持）

---

## 🎯 4. リスク管理計画

### 4.1 技術的リスク

| リスク要因 | 影響度 | 発生確率 | 対策 | 担当SubAgent |
|-----------|-------|---------|------|------------|
| N+1問題 | 大 | 中 | Include()パターン活用・Eager Loading徹底 | csharp-infrastructure |
| CASCADE DELETE | 中 | 低 | 論理削除実装活用・関連データ確認 | csharp-infrastructure |
| 複合一意制約違反 | 中 | 中 | 既存チェック実装活用・事前検証 | fsharp-application |
| 権限判定複雑化 | 中 | 中 | ヘルパーメソッド実装・Railway-oriented Programming | fsharp-application |
| Playwright Agents自動修復精度 | 中 | 低 | 手動検証15-20%・レビュープロセス | integration-test |
| bUnit InputRadioGroup制約 | 低 | 低 | カスタムコンポーネント実装 | csharp-web-ui |

### 4.2 プロセスリスク

| リスク要因 | 影響度 | 発生確率 | 対策 |
|-----------|-------|---------|------|
| Step3スキップによる見落とし | 中 | 低 | dependency-analysis Agent再確認・Step4冒頭で確認 |
| Playwright統合効果測定不足 | 低 | 中 | Step2 Stage 4で定量測定必須 |
| 技術負債解消漏れ | 中 | 低 | spec-compliance Agent最終チェック |
| Phase B1基盤理解不足 | 低 | 低 | design-review Agent成果物参照必須 |

### 4.3 品質リスク

| リスク要因 | 影響度 | 発生確率 | 対策 |
|-----------|-------|---------|------|
| Clean Architecture品質低下 | 大 | 低 | code-review Agent定期実行・96-97点維持確認 |
| テスト成功率100%未達 | 中 | 低 | unit-test/integration-test Agent TDD徹底 |
| 仕様準拠度95点未達 | 中 | 低 | spec-compliance Agent Step5最終確認 |

---

## 📅 5. Phase B2全体スケジュール・工数見積もり

### 5.1 Step別工数見積もり（確定版）

| Step | 実施内容 | 推定時間 | SubAgent構成 |
|------|---------|---------|------------|
| Step1 | 要件詳細分析・技術調査 | ✅ 2-3時間 | spec-analysis + tech-research + design-review + dependency-analysis |
| Step2 | Playwright MCP + Agents統合 | 1.5-2時間 | integration-test + tech-research |
| ~~Step3~~ | ~~Domain層拡張~~ | **スキップ** | - |
| Step4 | Application/Infrastructure層実装 | 3-4時間 | fsharp-application + csharp-infrastructure + unit-test |
| Step5 | Web層実装・技術負債解消 | 3-4時間 | csharp-web-ui + integration-test + spec-compliance |
| **合計** | **Phase B2全体** | **10-13時間** | - |

### 5.2 セッション別実施計画（推奨）

| セッション | 実施Step | 推定時間 | 実施日（推定） |
|-----------|---------|---------|--------------|
| セッション1 | Step1 | 2-3時間 | 2025-10-15 ✅ |
| セッション2 | Step2 | 1.5-2時間 | 2025-10-16 |
| セッション3 | Step4 (前半) | 2時間 | 2025-10-17 |
| セッション4 | Step4 (後半) | 2時間 | 2025-10-18 |
| セッション5 | Step5 | 3-4時間 | 2025-10-19 |

**Phase B2完了予定日**: 2025-10-19（推定）

### 5.3 Playwright統合効果反映（削減見込み）

**従来手法の場合**: 22-28時間
**Playwright統合後**: 10-13時間
**削減時間**: **12-15時間**（約55%削減）

---

## 📊 6. Phase B2完了後の達成状態

### 機能要件
- ✅ UserProjects多対多関連完全実装
- ✅ 権限制御マトリックス16パターン完全実装
- ✅ プロジェクトメンバー管理UI完全実装
- ✅ Phase B1技術負債4件完全解消

### 品質要件
- ✅ Clean Architecture 96-97点品質維持
- ✅ 仕様準拠度95点以上達成
- ✅ 0 Warning / 0 Error達成
- ✅ テスト成功率100%達成

### 技術基盤
- ✅ Playwright MCP + Agents統合完了
- ✅ Playwright E2E基盤確立
- ✅ ADR_021作成（技術決定永続化）
- ✅ Phase B1確立基盤100%活用

---

## 📋 7. Step間成果物参照マトリックス

### Step1成果物（本計画含む）

| 成果物 | 参照先Step | 参照目的 |
|-------|-----------|---------|
| Spec_Analysis_UserProjects.md | Step4, Step5 | 要件詳細確認 |
| Tech_Research_Playwright_2025-10.md | Step2 | Playwright統合手順 |
| Design_Review_PhaseB2.md | Step4, Step5 | 設計整合性確認 |
| Dependency_Analysis_UserProjects.md | Step4 | 依存関係・実装順序 |
| Phase_B2_Implementation_Plan.md | Step2-5全体 | 全体計画参照 |

### Step2-5成果物（予定）

| Step | 成果物 | 参照先 | 参照目的 |
|------|-------|-------|---------|
| Step2 | ADR_021 | Step4-5 | Playwright統合戦略 |
| Step2 | E2E.Tests初期実装 | Step5 | E2Eテスト拡張基盤 |
| Step4 | IProjectManagementService拡張 | Step5 | Web層実装基盤 |
| Step4 | ProjectRepository拡張 | Step5 | Web層実装基盤 |
| Step5 | 統合テスト完了レポート | Phase総括 | Phase B2完了確認 |

---

## 🎯 8. Step1完了後の次アクション

### Step1 Stage 2-5（残り作業）

#### Stage 3: Step1成果活用体制確立（30-45分）
- ✅ Step間成果物参照マトリックス作成（本計画 7章完了）
- ⏳ 後続Step組織設計記録への参照リスト準備
- ⏳ Phase_Summary.md更新

#### Stage 4: 品質確認・レビュー（15-30分）
- ⏳ 5つの分析・計画レポートの品質確認
- ⏳ 仕様準拠度確認
- ⏳ 成果物の完全性確認

#### Stage 5: Step完了処理（15分）
- ⏳ Step01_Analysis.md更新
- ⏳ ユーザー報告・Step完了承認取得

### Step2実行準備
- Step2組織設計記録作成（step-start Command実行）
- Playwright MCP統合実行（Stage 1: 5分）

---

## 📚 参照文書一覧

### Step1分析レポート
- `/Doc/08_Organization/Active/Phase_B2/Research/Spec_Analysis_UserProjects.md`
- `/Doc/08_Organization/Active/Phase_B2/Research/Tech_Research_Playwright_2025-10.md`
- `/Doc/08_Organization/Active/Phase_B2/Research/Design_Review_PhaseB2.md`
- `/Doc/08_Organization/Active/Phase_B2/Research/Dependency_Analysis_UserProjects.md`

### Phase B-F1成果物
- `/Doc/08_Organization/Completed/Phase_B-F1/Phase_B2_申し送り事項.md`
- `/Doc/08_Organization/Completed/Phase_B-F1/Research/Playwright_MCP_評価レポート.md`
- `/Doc/08_Organization/Completed/Phase_B-F1/Research/Playwright_Agents_評価レポート.md`

### Phase B1成果物
- `/Doc/08_Organization/Completed/Phase_B1/Phase_Summary.md`

### 要件・仕様書
- `/Doc/01_Requirements/機能仕様書.md` - 4.2.1「プロジェクトメンバー管理」
- `/Doc/02_Design/UI設計/02_プロジェクト・ドメイン管理画面設計.md`
- `/Doc/02_Design/データベース設計書.md` - UserProjectsテーブル

### アーキテクチャ決定記録
- `/Doc/07_Decisions/ADR_019_namespace階層化標準.md`
- `/Doc/07_Decisions/ADR_020_テストアーキテクチャ決定.md`

---

**Phase B2全体実装計画作成完了**
**重要な決定事項**:
- ✅ Step3（Domain層拡張）スキップ確定
- ✅ UserProjectsテーブルMigration不要確定
- ✅ Playwright Agents推奨度向上確定（7/10 → 9/10）
- ✅ Clean Architecture 96-97点品質維持確定
- ✅ Phase B2段階構成: 5段階 → 4段階（Step1, Step2, Step4, Step5）
- ✅ Phase B2推定工数: 10-13時間（Playwright統合効果反映済み）

**次のアクション**: Step1 Stage 3-5実施・ユーザー承認取得
