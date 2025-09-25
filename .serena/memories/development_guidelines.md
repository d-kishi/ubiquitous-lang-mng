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

## 🚀 強化されたCommand体系（2025-09-25更新）

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

### session-end改善（差分更新方式・2025-09-25）
- **既存内容読み込み**: 各メモリー更新前にmcp__serena__read_memory実行必須
- **差分更新方式**: 全面書き換え禁止・既存内容保持・必要部分のみ更新
- **履歴管理**: daily_sessions 30日保持・task_completion_checklist状態更新

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

### SubAgent並列実行効率化（2025-09-25実績）
- **4SubAgent並列実行**: spec-analysis, tech-research, design-review, dependency-analysis
- **実行時間**: 90分→45分（50%効率改善達成）
- **成果物品質**: 包括的分析による実装準備完了度向上

## コマンド駆動開発プロセス

### 自動実行Commands
- **セッション管理**: session-start.md / session-end.md（差分更新方式）
- **Phase管理**: phase-start.md / phase-end.md
- **Step管理**: step-start.md（task-breakdown統合） / step-end-review Commands
- **品質管理**: spec-validate・spec-compliance-check・tdd-practice-check

### セッション終了時必須更新（差分更新自動化）
- **daily_sessions**: 30日保持・自動削除・重要情報構造化・既存履歴保持
- **project_overview**: Phase進捗・技術負債・完了事項・該当セクションのみ更新
- **development_guidelines**: 方針変更・プロセス改善・新規追記のみ
- **tech_stack_and_conventions**: 技術発見・規約変更・新規追記のみ
- **task_completion_checklist**: タスク状況・継続課題・状態更新のみ

## 🎯 品質管理強化体制（2025-09-25強化）

### 加重スコアリング体系
- **肯定的仕様準拠度**: 50点満点（最高重要度）
- **否定的仕様遵守度**: 30点満点（高重要度）  
- **実行可能性・品質**: 20点満点（中重要度）
- **目標**: 95点以上達成・維持

### 自動証跡記録
- **実装箇所自動検出**: 仕様項番コメントからの逆引き
- **コードスニペット収集**: 重要実装部分の自動抽出
- **実装行番号マッピング**: 仕様項番 ↔ ソースコード行番号対応

### 事前検証体制
- **spec-validate**: Phase/Step開始前の仕様完全性検証
- **品質ゲート**: 95点未満時の開始禁止・改善必須

## 技術実装方針

### Clean Architecture遵守
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

### TDD実践指針
- **Red-Green-Refactor**: 必須サイクル
- **単体テスト**: xUnit + FsUnit + Moq
- **統合テスト**: WebApplicationFactory
- **カバレッジ**: 95%以上維持

## 🤖 SubAgent活用戦略

### 主要SubAgent
- **csharp-web-ui**: Blazor Server・認証UI・リアルタイム機能
- **fsharp-domain**: ドメインモデル・ビジネスロジック・関数型パターン
- **contracts-bridge**: F#↔C#型変換・相互運用・TypeConverter
- **integration-test**: WebApplicationFactory・E2E・API・DB統合
- **spec-compliance**: 仕様準拠監査・加重スコアリング・証跡記録
- **spec-analysis**: 仕様書分析・原典仕様確認・完全性検証

### 並列実行パターン
- **実装時**: fsharp-domain + csharp-web-ui + contracts-bridge 並列実行
- **テスト時**: unit-test + integration-test 並列実行
- **品質確認**: spec-compliance + code-review 並列実行
- **設計時**: spec-analysis + design-review 並列実行

### Pattern選択ガイドライン（2025-09-25追加）
- **Pattern A**: 新機能実装（Domain→Application→Infrastructure→Web）
- **Pattern B**: 機能拡張（影響分析→実装統合→品質保証）
- **Pattern C**: 品質改善（課題分析→改善実装→検証完成）
- **Pattern D**: 品質保証段階（技術負債→品質改善→統合検証）
- **Pattern E**: 拡張段階（外部連携→拡張実装→運用準備）

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

## 📋 GitHub Issues管理強化（2025-09-25追加）

### Issue作成・管理規約
- **高優先度**: Phase開始前必須対応事項（即座対応）
- **低優先度**: 将来実装・研究開発的要素（詳細記録・再開可能状態）
- **技術負債**: TECH-XXX形式・系統管理

### Issue種別・ラベル体系
- **phase-B1**: Phase B1関連作業
- **spec-driven**: 仕様駆動開発関連
- **quality**: 品質改善・技術負債
- **enhancement**: 機能拡張・改善提案

### 現在のIssue状況
- **Issue #38**: Phase B1開始前必須対応事項（🔴高優先度・次回セッション対応）
- **Issue #39**: 仕様駆動開発強化Phase 2・3（🔵低優先度・将来実装）

## 情報管理・選択指針

### 必要最小限読み込み原則
- **セッション開始時**: 必読ファイル（5-6個）のみ読み込み
- **実装時**: 該当モジュール・関連ファイルのみ対象化
- **SubAgent指示**: 具体的作業範囲・対象ファイル明示

### メモリー活用方針
- **project_overview**: プロジェクト状況・Phase進捗確認
- **development_guidelines**: 開発方針・プロセス確認
- **tech_stack_and_conventions**: 技術規約・実装パターン確認
- **daily_sessions**: 過去30日セッション履歴・学習事項確認

### コンテキスト最適化
- **自動アーカイブ**: 30日経過記録の自動削除
- **情報統合**: 重複排除・関連情報の集約
- **構造化記録**: 検索効率・保守性重視の記録形式

## 品質保証体系

### 0 Warning, 0 Error維持
- **ビルド時**: 全プロジェクト警告・エラー0維持
- **実行時**: 例外・ログエラー0維持
- **テスト**: 全テスト成功・カバレッジ95%以上

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

### 作業時間短縮
- **Commands活用**: 定型作業の自動化
- **SubAgent並列**: 同時実行による時間短縮
- **情報絞り込み**: 必要最小限読み込み
- **テンプレート活用**: 定型作業の効率化

### 品質向上
- **自動化チェック**: Commands による品質確認
- **継続監視**: GitHub Issues による課題管理
- **学習蓄積**: daily_sessions による知見共有
- **プロセス改善**: 週次振り返りによる改善循環

---
**最終更新**: 2025-09-25（Step1成果活用体制確立・SubAgent並列実行50%効率改善・session-end差分更新方式）  
**重要変更**: Step間成果物参照マトリックス・step-start自動参照機能・4SubAgent並列実行パターン確立