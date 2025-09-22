# 開発ガイドライン（2025-09-22更新・コマンド更新対応）

## コーディング標準

### ログ実装基準（確立済み）
1. **構造化ログ必須**: `Logger.LogInformation("処理完了 Email: {Email}", email)`
2. **セキュリティ配慮**: パスワード非出力・メールマスク実装必須
3. **Console.WriteLine禁止**: 全てILogger経由・技術負債防止
4. **パフォーマンス測定**: Stopwatch + ログによる処理時間記録推奨

### Clean Architecture遵守
- **Domain層**: F#純粋実装・外部依存禁止・ログ出力禁止
- **Application層**: F# Railway-oriented Programming・Result型活用
- **Infrastructure層**: C# EF Core・Repository・Service・構造化ログ実装
- **Web層**: C# Blazor Server・UI状態管理・ユーザー操作ログ
- **Contracts層**: TypeConverter・境界値変換・変換ログ実装

### 品質基準
- **必須条件**: 0 Warning, 0 Error状態維持
- **テスト**: TDD実践・Red-Green-Refactorサイクル
- **E2E確認**: 実装後は実際の認証フローで動作確認必須

## SubAgent活用パターン（更新済み・2025-09-22）

### 新規追加Pattern（コマンド更新完了）
1. **Pattern D（品質保証段階）**: Phase B4-B5, C5-C6, D7等
   - Phase1: 技術負債特定・分析（code-review・dependency-analysis・tech-research）
   - Phase2: 品質改善実装（対象層Agent・unit-test・code-review）
   - Phase3: 統合検証・品質確認（integration-test・spec-compliance）

2. **Pattern E（拡張段階）**: Phase D7-D8等
   - Phase1: 外部連携設計・調査（tech-research・design-review・spec-analysis）
   - Phase2: 拡張機能実装（csharp-infrastructure・contracts-bridge・csharp-web-ui）
   - Phase3: 運用準備・統合確認（integration-test・code-review・spec-compliance）

### 効果実証済みパターン
1. **並列実装**: csharp-web-ui + csharp-infrastructure + contracts-bridge
2. **効率化実績**: 40-50%時間短縮・品質向上効果
3. **専門性活用**: 各Agentの得意領域での高品質実装
4. **TDD必須化**: 全実装系SubAgentにRed-Green-Refactorサイクル組み込み

### 開発プロセス（段階対応・更新済み）
1. **段階的ビルド**: 各段階完了後にdotnet build実行
2. **段階種別判定**: 基本実装（1-3）・品質保証（4-6）・拡張（7-8）段階の自動判定
3. **Phase規模対応**: 🟢中規模・🟡大規模・🔴超大規模の自動判定・適切なリソース配分
4. **E2Eテスト**: 実装完了後は実際のユーザーフローで検証
5. **KPT管理**: 三者協議方式・改善施策バックログ管理

## Phase実装戦略（改訂版・コマンド対応完了）

### 段階的実装アプローチ（5-8段階）
- **基本実装段階（1-3）**: 基本CRUD・関連機能・機能完成
- **品質保証段階（4-6）**: 技術負債解消・UI/UX最適化・統合テスト
- **拡張段階（7-8）**: 高度機能・外部連携・運用最適化

### Phase別特性（更新済み）
- **Phase B**: 5段階（プロジェクト管理・🟢中規模・5-7セッション）
- **Phase C**: 6段階（ドメイン管理・承認ワークフロー・🟡大規模・7-9セッション）
- **Phase D**: 7-8段階（ユビキタス言語管理・Excel風UI・🔴超大規模・10-12セッション）

## 計画管理方針

### 段階的詳細化アプローチ
- **Phase B**: 詳細計画実行・実績収集・コマンド更新効果検証
- **Phase C・D**: 概要計画・B実績による継続調整
- **実績ベース見直し**: 各Phase完了毎の計画精度向上

### コマンド活用（更新済み）
- **phase-start**: Phase規模自動判定・段階数自動取得
- **subagent-selection**: Pattern A～E選択・段階種別判定
- **step-start**: 段階種別対応・Stage構成拡張