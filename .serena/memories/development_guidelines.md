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

## コマンド駆動開発プロセス

### 自動実行Commands
- **セッション管理**: session-start.md / session-end.md
- **Phase管理**: phase-start.md / phase-end.md
- **Step管理**: step-start.md / step-end-review Commands
- **品質管理**: spec-compliance-check / tdd-practice-check

### セッション終了時必須更新（自動化完了）
- **daily_sessions**: 30日保持・自動削除・重要情報構造化
- **project_overview**: Phase進捗・技術負債・完了事項
- **development_guidelines**: 方針変更・プロセス改善
- **tech_stack_and_conventions**: 技術発見・規約変更
- **task_completion_checklist**: タスク状況・継続課題

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

## SubAgent活用戦略

### 主要SubAgent
- **csharp-web-ui**: Blazor Server・認証UI・リアルタイム機能
- **fsharp-domain**: ドメインモデル・ビジネスロジック・関数型パターン
- **contracts-bridge**: F#↔C#型変換・相互運用・TypeConverter
- **integration-test**: WebApplicationFactory・E2E・API・DB統合
- **spec-compliance**: 仕様準拠監査・マトリックス検証・受け入れ基準

### 並列実行パターン
- **実装時**: fsharp-domain + csharp-web-ui + contracts-bridge 並列実行
- **テスト時**: unit-test + integration-test 並列実行
- **品質確認**: spec-compliance + code-review 並列実行

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
**最終更新**: 2025-09-24（Dev Container移行計画追加）  
**次回更新**: Phase B1開始時または重要な方針変更時