# 開発ガイドライン

## プロセス遵守絶対原則（ADR_016）

### 🔴 重大違反禁止事項
- ❌ **承認前の作業開始**: いかなる理由でも禁止
- ❌ **独断での判断**: 「効率化」を理由とした勝手な作業
- ❌ **コマンド手順の無視**: phase-start/step-start等の手順飛ばし
- ❌ **成果物の虚偽報告**: 実体のない成果物の報告

### ✅ 必須実行事項
- **実体確認**: SubAgent成果物の物理的存在確認
- **承認記録**: 取得した承認の明示的記録
- **チェックリスト実行**: 組織管理運用マニュアルのプロセス遵守チェック

## 🎯 動作確認タイミング戦略（2025-10-06確立・マスタープラン記録済み）

### 2段階動作確認アプローチ（全Phase共通原則）
**策定根拠**: Phase B1完了時点での機能不完全（権限6/16パターン・UserProjects未実装）、UI未最適化のため、機能完成後＋UI最適化後の2段階確認が効率的と判断

#### Phase B動作確認タイミング
1. **Phase B3完了後**: 中間確認（必須）
   - **目的**: ビジネスロジック・機能完全性の確認
   - **確認項目**: 全4ロール動作・UserProjects・権限16パターン完全実装
   - **フィードバック活用**: Phase B4（品質改善）・B5（UI/UX最適化）に反映

2. **Phase B5完了後**: 本格確認（必須）
   - **目的**: UI/UX・品質・パフォーマンスの最終確認・Phase B完了承認
   - **確認項目**: Phase B3確認内容再確認＋UI/UX改善確認＋E2E検証
   - **成果物**: Phase B完了承認・Phase C移行判断

#### 動作確認不実施原則
- **Phase B1-B2完了時**: 機能不完全（実装率50%未満）のため動作確認不実施
- **理由**: 不完全な機能での確認は非効率・誤った印象を与えるリスク

**詳細**: `/Doc/08_Organization/Rules/縦方向スライス実装マスタープラン.md`（動作確認戦略セクション）

## 🔧 namespace設計原則（ADR_019準拠・2025-10-01確立）

### 必須遵守事項
**基本テンプレート**: `<ProjectName>.<Layer>.<BoundedContext>[.<Feature>]`

#### 具体的namespace規約
- **Domain層**:
  ```fsharp
  namespace UbiquitousLanguageManager.Domain.Common
  namespace UbiquitousLanguageManager.Domain.Authentication
  namespace UbiquitousLanguageManager.Domain.ProjectManagement
  namespace UbiquitousLanguageManager.Domain.UbiquitousLanguageManagement
  ```
- **Application層**: `UbiquitousLanguageManager.Application.<BoundedContext>`
- **Infrastructure層**: `UbiquitousLanguageManager.Infrastructure.<Feature>`
- **Contracts層**: `UbiquitousLanguageManager.Contracts.<Feature>`
- **Web層**: `UbiquitousLanguageManager.Web.<Feature>`

#### 階層構造ルール
- **Common特別扱い**: 全Bounded Contextで使用する共通定義（ID型・Permission・Role等）
- **Bounded Context分離**: Authentication/ProjectManagement/UbiquitousLanguageManagement/DomainManagement
- **最大階層制限**: 3階層推奨・4階層許容（深すぎる階層は可読性低下）

#### F#特別考慮事項
- **Module設計**: Module = Bounded Context推奨だが強制しない・保守性優先
- **コンパイル順序**: Common→Authentication→ProjectManagement→UbiquitousLanguageManagement（前方参照不可制約）
- **namespace + module組み合わせ活用**: 型定義・Smart Constructor・ドメインサービスの整理

#### C#特別考慮事項
- **using文推奨パターン**: Bounded Context別にusing文を明示的に記載
- **using alias使用**: 型名衝突回避時に活用（例: `using DomainModel = UbiquitousLanguageManager.Domain.ProjectManagement.Domain;`）

### 検証プロセス（必須実行）

#### Step開始時検証
- [ ] namespace構造レビュー実施
- [ ] Bounded Context境界確認
- [ ] 循環依存なし確認
- [ ] ADR_019規約準拠確認

#### Phase完了時検証
- [ ] 全層namespace整合性確認
- [ ] ADR_019規約準拠確認
- [ ] F#/C#ベストプラクティス準拠確認
- [ ] Clean Architecture 97点以上維持確認

### 重要性・リスク
**違反時の影響**: Phase完了後の大規模手戻り（3.5-4.5時間・42ファイル修正の実績あり）

**詳細**: `/Doc/07_Decisions/ADR_019_namespace設計規約.md`（247行・業界標準準拠）

## 🔧 SubAgent責務境界原則（2025-09-28新設・2025-09-30改善実証・セッション終了処理対応）

### エラー修正時の必須遵守原則（タイミング問わず適用）
**基本方針**: 「エラーが発生した場所」ではなく「エラーの内容」で責務を判定

#### MainAgent責務（厳格定義）
**✅ 実行可能な作業**:
- 全体調整・オーケストレーション
- SubAgentへの作業委託・指示
- 品質確認・統合テスト実行
- プロセス管理・進捗管理
- ドキュメント統合・レポート作成
- セッション終了処理・メモリー差分更新

**❌ 禁止事項（例外を除く）**:
- 実装コードの直接修正
- ビジネスロジックの追加・変更
- 型変換ロジックの実装
- テストコードの作成・修正
- データベーススキーマの変更

**例外（直接修正可能）**:
- 単純なtypo（1-2文字）
- import文の追加のみ
- コメントの追加・修正
- 空白・インデントの調整

#### エラー修正の責務判定フロー（必須実行）
1. **エラー内容を分析**（エラーメッセージ・影響範囲・修正規模の判定）
2. **責務マッピングでSubAgent選定**:
   - F# Domain/Application層 → fsharp-domain/fsharp-application
   - F#↔C#境界・型変換 → contracts-bridge
   - C# Infrastructure/Web層 → csharp-infrastructure/csharp-web-ui
   - テストエラー → unit-test/integration-test
3. **修正規模による判定**（小規模でも対象SubAgentに委託）
4. **SubAgent修正実行**（Fix-Mode活用・修正完了後品質確認）

## 🚀 Fix-Mode活用体系（Phase B1 Step3実証完了・2025-09-30・永続化完了）

### Phase B1 Step3での実証結果（完全成功・永続化完了）
**修正対象**: Contracts層構文エラー9件（8件 + XMLコメント1件）
**実行時間**: 15分で全修正完了（従来手法60-90分 → 75%短縮）
**成功要因**: contracts-bridge Agent責務分担・専門性活用・標準テンプレート適用
**品質向上**: 構文チェック精度向上・C#規約完全準拠・0 Warning/0 Error達成
**永続化**: ADR_018・SubAgent実行ガイドライン策定完了・継続改善循環確立

### 確立された標準テンプレート（実証済み・ADR_018準拠）
```markdown
[SubAgent名] Agent, Fix-Mode: [エラー種別]エラーを修正してください。

## 修正対象エラー詳細
**ファイル**: [ファイルパス]:[行番号]
**エラーコード**: [CS1234など]
**エラーメッセージ**: [完全なエラーメッセージ]

## 修正指示
```csharp/fsharp
// 修正前（エラー）
[具体的なエラーコード]

// 修正後（正しい）
[期待される正しいコード]
```

## 参考実装
[既存の類似正常コードの例]

## 重要な制約事項
- **ロジック変更禁止**: 構文エラーの修正のみ実施
- **既存パターン準拠**: 他の同種実装の命名規則に従う
- **構文規約遵守**: C#/F#の言語仕様・プロジェクト規約完全準拠

修正完了後、[N]件のエラーが解消されることを確認してください。
```

### 実証済み改善ポイント（Phase B1 Step3成功要因・永続化完了）
1. **指示の具体性向上**: エラー箇所行番号明示・修正前後コード例提示
2. **参考情報提供**: 既存正常コードの具体例提示・プロジェクト規約参照
3. **段階的確認**: 各SubAgent完了後の個別ビルド確認実施
4. **エラー本質把握**: 表面的修正ではなく根本原因の理解・解決
5. **制約事項明確化**: ロジック変更禁止・構文規約遵守の明示的指示

### エラー修正責務判定（実証済みマッピング・効果測定済み・継続活用推奨）
| エラー種別 | 対象SubAgent | 実証事例 | 修正時間 | 成功確認 |
|----------|-------------|---------|----------|----------|
| C#構文エラー（CS0246等） | contracts-bridge | Step3で6件修正成功 | 10分 | ✅ 100% |
| using alias構文エラー（CS0305） | contracts-bridge | Step3で2件修正成功 | 3分 | ✅ 100% |
| XMLコメント構文エラー（CS1587） | contracts-bridge | Step3で1件修正成功 | 2分 | ✅ 100% |
| F#構文エラー（FS0597等） | fsharp-domain/fsharp-application | Phase B1 Step5-7で活用 | - | ✅ 100% |
| テスト関連エラー | unit-test/integration-test | Phase B1 Step7で活用 | - | ✅ 100% |

### 効果測定指標（Phase B1 Step3実績・継続測定確立・目標達成）
- **Fix-Mode成功率**: 100%（9/9件成功・目標95%以上大幅達成）
- **修正時間効率**: 15分/9件（1.67分/件平均・目標15分以下達成）
- **ビルド成功率**: 100%（修正後即座ビルド成功・目標100%達成）
- **SubAgent責務違反件数**: 0件（完全遵守・効率性より責務優先徹底）

## 🚀 強化されたCommand体系（2025-09-25更新・セッション終了処理強化）

### 仕様駆動開発Command群
- **spec-validate**: Phase/Step開始前事前検証（100点満点・3カテゴリ）
- **spec-compliance-check**: 加重スコアリング仕様準拠確認（50/30/20点配分）
- **task-breakdown**: 自動タスク分解・TodoList連携・Clean Architecture層別分解

### 統合workflow（step-start強化版）
```
step-start Command実行
↓
task-breakdown自動実行（新機能）
├─ GitHub Issue読み込み（高優先度・phase-B1）
├─ Clean Architecture層別タスク分解
├─ TodoList自動生成・工数見積もり
└─ ユーザー承認
↓
SubAgent並列実行（Pattern A/B/C/D/E選択）
```

### session-end改善（差分更新方式・2025-09-25・実証完了・品質確認済み）
- **既存内容読み込み**: 各メモリー更新前にmcp__serena__read_memory実行必須
- **差分更新方式**: 全面書き換え禁止・既存内容保持・必要部分のみ更新
- **履歴管理**: daily_sessions 30日保持・task_completion_checklist状態更新
- **品質確認**: 破壊的変更防止・既存重要情報維持・次回セッション参照可能状態確保

## 🎯 Phase B1完了成果（2025-10-06・Phase完了承認取得）

### Phase B1総合評価: ✅ **完了承認**（品質スコア 98/100点・A+ Excellent）

**実行期間**: 2025-09-25 ～ 2025-10-06（12日間・9セッション）

**達成事項**:
- ✅ **全7Step完了**: 100%完了
- ✅ **機能要件**: プロジェクト基本CRUD完全実装（Phase B1範囲100%達成）
- ✅ **品質要件**: 仕様準拠度98点・Clean Architecture 96点・テスト100%成功
- ✅ **技術基盤**: F#/C# 4層統合・Bounded Context分離・namespace階層化完了
- ✅ **技術パターン**: F#↔C#変換4パターン・bUnitテスト基盤・Blazor Server実装パターン確立
- ✅ **プロセス改善**: Fix-Mode・SubAgent並列実行・ADR記録による知見永続化

### Phase B1で確立した技術パターン（Phase B2継承予定）
1. **F#↔C# Type Conversion Patterns**（Phase B1 Step7確立）:
   - F# Result型 → C# bool判定（IsOk/ResultValueアクセス）
   - F# Option型 → C# null許容型（Some/None明示的変換）
   - F# Discriminated Union → C# switch式パターンマッチング
   - F# Record型 → C# オブジェクト初期化（camelCaseパラメータコンストラクタ）

2. **Blazor Server実装パターン**（Phase B1 Step7確立）:
   - @bind:after活用（StateHasChanged最適化）
   - EditForm統合（ValidationSummary連携）
   - Toast通知（Bootstrap Toast + JavaScript相互運用）

3. **bUnitテスト基盤**（Phase B1 Step7確立・Phase B2活用予定）:
   - BlazorComponentTestBase（認証・サービス・JSランタイムモック統合）
   - FSharpTypeHelpers（F#型生成ヘルパー）
   - ProjectManagementServiceMockBuilder（Fluent API モックビルダー）

### Phase B2申し送り事項
**Phase B2対応予定技術負債（4件）**:
- InputRadioGroup制約（2件）: Blazor Server/bUnit既知の制約・Phase B2実装パターン確立
- フォーム送信詳細テスト（1件）: フォーム送信ロジック未トリガー・Phase B2対応
- Null参照警告（1件）: ProjectManagementServiceMockBuilder.cs:206・Phase B2 Null安全性向上

**Phase B2スコープ**:
- UserProjects多対多関連実装
- DomainApprover/GeneralUser権限実装（10パターン追加）
- プロジェクトメンバー管理UI実装

**Phase B1確立基盤活用**:
- Clean Architecture基盤（96-97点品質）
- F#↔C# Type Conversion 4パターン
- bUnitテスト基盤（Phase B2で継続活用）
- Blazor Server実装パターン（Phase B2で継続活用）

## 🎯 Step1成果活用体制（2025-09-25新設）

### Step間成果物参照マトリックス
Phase B1において、Step1分析成果を後続Step2-5で確実活用するため、以下の仕組みを確立：

**参照マトリックス構造**:
```
| Step | 作業内容 | 必須参照（Step1成果物） | 重点参照セクション | 活用目的 |
|------|---------|----------------------|-------------------|---------|
| Step2 | Domain層実装 | Technical_Research_Results.md | F# Railway-oriented Programming | ProjectDomainService実装 |
```

### step-start Command自動参照機能（2025-09-25追加）
- **Step1成果物参照準備**: Phase_Summary.mdの参照マトリックスから当該Step必須参照ファイル特定
- **自動参照リスト追加**: 当該Step組織設計記録にStep1分析成果の必須参照リスト自動追加
- **SubAgent指示連携**: 参照ファイルパスをSubAgent実行指示に埋め込み

## 🤖 SubAgent並列実行効率化（Phase B1 Step3完全実証・2025-09-30・技術価値確立）

### 実証された効果（Phase B1 Step3成功実績・継続活用推奨）
**並列実行Agent**: fsharp-application + contracts-bridge + unit-test 同時実行
**品質達成**: 仕様準拠度100点満点・TDD実践⭐⭐⭐⭐⭐ 5/5優秀評価
**責務分担成功**: F#実装・C#境界・テスト基盤の専門性完全活用
**時間効率**: 従来手法比50%効率改善・実装85%・エラー修正15%の時間配分実現
**技術価値**: Clean Architecture 97点品質継続・Railway-oriented Programming完全適用

### SubAgent並列実行パターン（実証済み効果・継続活用推奨）
- **Pattern A実装時**: fsharp-application + fsharp-domain + contracts-bridge 並列実行（Step3実証）
- **テスト並列**: unit-test + integration-test 並列実行
- **品質確認並列**: spec-compliance + code-review 並列実行
- **設計並列**: spec-analysis + design-review 並列実行
- **Fix-Mode特化**: contracts-bridge単独（エラー種別特化・最高効率・15分/9件実証）

### Pattern選択ガイドライン（2025-09-25追加・2025-09-30実証・技術価値確立）
- **Pattern A**: 新機能実装（Domain→Application→Infrastructure→Web）**←Step3で100点満点実証**
- **Pattern B**: 機能拡張（影響分析→実装統合→品質保証）
- **Pattern C**: 品質改善（課題分析→改善実装→検証完成）
- **Pattern D**: 品質保証段階（技術負債→品質改善→統合検証）
- **Pattern E**: 拡張段階（外部連携→拡張実装→運用準備）

**Phase B1 Step3実証結果（Pattern A適用成功・技術価値確立）**:
- **実行効果**: Domain層基盤活用→Application層実装→Contracts層統合→TDD Green Phase達成
- **品質結果**: 仕様準拠度100点・TDD実践優秀評価・0 Warning/0 Error達成
- **プロセス価値**: Fix-Mode・SubAgent責務分担の成功パターン確立・永続化完了
- **技術基盤価値**: Infrastructure層実装準備完了・4層統合基盤確立

### 実行時間効率化実績（実証済み・継続効果期待）
- **3-4SubAgent並列実行**: 効率的作業分散・同時実行による時間短縮
- **実行時間効率化**: 従来の50%効率改善実績・継続改善実現
- **品質と効率の両立**: 仕様準拠度100点・TDD実践優秀評価との同時達成実証済み

## コマンド駆動開発プロセス

### 自動実行Commands
- **セッション管理**: session-start.md / session-end.md（差分更新方式・品質確認済み）
- **Phase管理**: phase-start.md / phase-end.md
- **Step管理**: step-start.md（task-breakdown統合） / step-end-review Commands
- **品質管理**: spec-validate・spec-compliance-check・tdd-practice-check

### セッション終了時必須更新（差分更新自動化・品質確認済み・破壊的変更防止）
- **daily_sessions**: 30日保持・自動削除・重要情報構造化・既存履歴保持
- **project_overview**: Phase進捗・技術負債・完了事項・該当セクションのみ更新
- **development_guidelines**: 方針変更・プロセス改善・新規追記のみ
- **tech_stack_and_conventions**: 技術発見・規約変更・新規追記のみ
- **task_completion_checklist**: タスク状況・継続課題・状態更新のみ

## 🎯 品質管理強化体制（2025-09-25強化・2025-09-30満点達成実証・Phase B1継続維持）

### 加重スコアリング体系（Phase B1 Step3で100点満点達成・Step7で98点達成）
- **肯定的仕様準拠度**: 50点満点（最高重要度）- Phase B1 Step3で50/50点・Step7で48/50点達成
- **否定的仕様遵守度**: 30点満点（高重要度）- Phase B1 Step3・Step7で30/30点達成（完璧）
- **実行可能性・品質**: 20点満点（中重要度）- Phase B1 Step3・Step7で20/20点達成（完璧）
- **実績**: 95点以上目標→98-100点達成（Phase B1全体で高品質維持・Phase B2継続目標）

### 自動証跡記録（Step3実装完了・継続活用）
- **実装箇所自動検出**: 仕様項番コメントからの逆引き実装完了
- **コードスニペット収集**: 重要実装部分の自動抽出実装完了
- **実装行番号マッピング**: 仕様項番 ↔ ソースコード行番号対応実装完了

### 事前検証体制（継続強化・Phase B2適用推奨）
- **spec-validate**: Phase/Step開始前の仕様完全性検証
- **品質ゲート**: 95点未満時の開始禁止・改善必須
- **継続監視**: Phase B1 Step3で100点・Step7で98点達成・継続品質保証体制確立

## 技術実装方針

### Clean Architecture遵守（96-97点品質継続維持・Phase B1全体達成）
- **Domain**: F# 純粋関数・不変データ・Result型活用
- **Application**: F# UseCases・外部依存排除
- **Contracts**: C# DTOs・TypeConverters・F#↔C#変換
- **Infrastructure**: C# EF Core・Repository・外部サービス
- **Web**: C# Blazor Server・Bootstrap 5・SignalR

### コメント・説明方針（ADR_010）
**Blazor Server初学者対応**:
- ライフサイクル詳細説明
- StateHasChanged呼び出しタイミング
- SignalR接続・切断処理説明

**F#初学者対応**:
- パターンマッチング詳細説明
- Option型・Result型の概念説明
- 関数合成・パイプライン演算子説明

### TDD実践指針（Phase B1全Step実践・継続実践目標）
- **Red-Green-Refactor**: 必須サイクル（Step2 Red・Step3 Green・Step7 Refactor完全実践）
- **単体テスト**: xUnit + FsUnit + Moq（Phase B1: 52テスト100%成功維持）
- **統合テスト**: bUnit UIテスト（Phase B1 Step7: 10テスト・Phase B1範囲内100%成功）
- **カバレッジ**: 95%以上維持（Phase B1達成済み・Phase B2継続目標）

## 🤖 SubAgent活用戦略（2025-09-30改善実証・技術価値確立・Phase B1全Step活用）

### 主要SubAgent（実証済み効果・専門性確認・継続活用推奨）
- **csharp-web-ui**: Blazor Server・認証UI・リアルタイム機能（Phase B1 Step7完全活用）
- **fsharp-domain**: ドメインモデル・ビジネスロジック・関数型パターン（Step2完全実装）
- **fsharp-application**: F# Application層・IService実装・Command/Query分離（Step3満点実装・100点品質）
- **contracts-bridge**: F#↔C#型変換・相互運用・TypeConverter（Fix-Mode実証成功・15分で9件修正・100%成功率）
- **integration-test**: WebApplicationFactory・E2E・API・DB統合（Phase B2適用予定）
- **spec-compliance**: 仕様準拠監査・加重スコアリング・証跡記録（98-100点満点評価実績）
- **spec-analysis**: 仕様書分析・原典仕様確認・完全性検証
- **unit-test**: TDD実践・Red-Green-Refactor支援（52テスト100%成功実績・⭐⭐⭐⭐⭐優秀評価）

## 環境管理・改善方針（2025-09-24追加）

### Dev Container移行計画
- **GitHub Issue**: #37で管理・後日実施予定
- **期待効果**: 環境構築時間90%短縮（1-2時間 → 5分）
- **技術要件**: .NET 8.0 + F# + PostgreSQL + VS Code拡張機能完全対応
- **ROI評価**: 新規メンバー2名参加で投資回収・開発効率10-20%向上

### 環境標準化指針
- **現状**: ローカル環境・Docker Compose（PostgreSQL/PgAdmin/Smtp4dev）
- **移行後**: Dev Container統合環境・VS Code拡張機能自動設定
- **利点**: 環境差異解消・オンボーディング簡易化・Issue #29根本解決

## 📋 GitHub Issues管理強化（2025-09-28更新・Phase B1完了時更新）

### 技術負債管理統一（GitHub Issues完全移行）
- **管理方法**: `/Doc/10_Debt/`廃止 → GitHub Issues TECH-XXX番号体系確立
- **step-end-review**: 新規技術負債のGitHub Issue作成（TECH-XXX番号付与）
- **task-breakdown**: 技術負債情報をGitHub Issues（TECH-XXXラベル）から収集

### Issue作成・管理規約
- **高優先度**: Phase開始前必須対応事項（即座対応）
- **低優先度**: 将来実装・研究開発的要素（詳細記録・再開可能状態）
- **技術負債**: TECH-XXX形式・系統管理

### Issue種別・ラベル体系
- **phase-B1**: Phase B1関連作業（✅完了）
- **phase-B2**: Phase B2関連作業（次Phase）
- **spec-driven**: 仕様駆動開発関連
- **quality**: 品質改善・技術負債
- **enhancement**: 機能拡張・改善提案

### 現在のIssue状況（2025-10-06更新）
- **Issue #38**: Phase B1開始前必須対応事項（✅完了クローズ）
- **Issue #39**: 仕様駆動開発強化Phase 2・3（🔵低優先度・将来実装）
- **Issue #40**: テストプロジェクト重複問題（🔵Phase B完了後対応・統合方式採用・1-2時間見積もり）
- **Issue #41**: Domain層リファクタリング（✅Phase B1 Step4完了クローズ）
- **Issue #42**: namespace階層化（✅Phase B1 Step5完了クローズ）
- **Issue #43**: Phase A既存テストビルドエラー（🔵Phase B完了後対応・1-2時間見積もり）
- **Issue #44**: ディレクトリ構造統一（🔵Phase B完了後対応・30分-1時間見積もり）

## 情報管理・選択指針

### 必要最小限読み込み原則
- **セッション開始時**: 必読ファイル（5-6個）のみ読み込み
- **実装時**: 該当モジュール・関連ファイルのみ対象化
- **SubAgent指示**: 具体的作業範囲・対象ファイル明示

### メモリー活用方針（差分更新方式・品質確認済み）
- **project_overview**: プロジェクト状況・Phase進捗確認・次回予定確認
- **development_guidelines**: 開発方針・プロセス確認・Fix-Mode指針確認
- **tech_stack_and_conventions**: 技術規約・実装パターン確認・新規技術情報確認
- **daily_sessions**: 過去30日セッション履歴・学習事項確認・継続課題確認
- **task_completion_checklist**: タスク状況・優先度・継続課題の状態管理

### コンテキスト最適化（継続改善・効率化）
- **自動アーカイブ**: 30日経過記録の自動削除・重要情報保持
- **情報統合**: 重複排除・関連情報の集約・検索効率向上
- **構造化記録**: 検索効率・保守性重視の記録形式・継続改善

## 品質保証体系

### 0 Warning, 0 Error維持（Phase B1全Step達成継続・Phase B2継続目標）
- **ビルド時**: 全プロジェクト警告・エラー0維持（Phase B1達成済み・Phase B2継続維持）
- **実行時**: 例外・ログエラー0維持
- **テスト**: 全テスト成功・カバレッジ95%以上（Phase B1達成済み・Phase B2継続維持）

### セキュリティ方針
- **シークレット管理**: appsettings・環境変数分離
- **認証・認可**: ASP.NET Core Identity準拠
- **入力検証**: サーバーサイド必須・クライアント補助
- **ログ管理**: 個人情報除外・セキュリティ事象記録

## 用語統一原則（ADR_003）

### 必須用語
- **「ユビキタス言語」**: Domain用語の正式名称
- **「Phase/Step」**: 開発フェーズ・ステップ
- **「SubAgent」**: 専門Agent・並列実行単位
- **「Command」**: 自動実行プロセス・.mdファイル

### 禁止用語
- **「用語」**: ユビキタス言語と呼ぶ
- **「タスク」**: 具体的作業内容明示
- **「機能」**: 具体的機能名明示

## 効率化・最適化指針

### 作業時間短縮（Phase B1実証済み・継続効果期待）
- **Commands活用**: 定型作業の自動化
- **SubAgent並列**: 同時実行による時間短縮（50%効率改善実証・継続活用推奨）
- **情報絞り込み**: 必要最小限読み込み
- **テンプレート活用**: 定型作業の効率化（Fix-Mode 75%効率化実証・継続活用）

### 品質向上（Phase B1で98-100点達成・Phase B2継続目標）
- **自動化チェック**: Commands による品質確認
- **継続監視**: GitHub Issues による課題管理
- **学習蓄積**: daily_sessions による知見共有
- **プロセス改善**: 週次振り返りによる改善循環

---
**最終更新**: 2025-10-06（**Phase B1完了・品質スコア98点達成・Phase B2準備開始・動作確認タイミング戦略確立**）  
**重要変更**: Phase B1完了記録追加・Phase B1成果総括・Phase B2申し送り事項記録・動作確認タイミング戦略追加（マスタープラン記録済み）・F#↔C#型変換4パターン確立記録・bUnitテスト基盤確立記録・Blazor Server実装パターン確立記録