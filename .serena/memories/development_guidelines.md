# 開発ガイドライン（2025-01-19更新）

## コーディング標準

### ログ実装基準（2025-01-19確立）
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

## SubAgent活用パターン

### 効果実証済みパターン
1. **並列実装**: csharp-web-ui + csharp-infrastructure + contracts-bridge
2. **効率化実績**: 40-50%時間短縮・品質向上効果
3. **専門性活用**: 各Agentの得意領域での高品質実装

### 開発プロセス
1. **段階的ビルド**: 各Phase完了後にdotnet build実行
2. **早期エラー検出**: ビルドエラー即座修正・問題拡大防止
3. **E2Eテスト**: 実装完了後は実際のユーザーフローで検証

## 技術選択基準

### Microsoft.Extensions.Logging採用理由（ADR_017）
- **既存投資活用**: 309箇所実装済みログ継続
- **.NET完全統合**: ASP.NET Core・EF Core・Blazor Server統合
- **学習コスト削減**: 新技術（Serilog）移行コスト回避

### F#活用基準
- **Domain層**: 100% F#実装・型安全性・不変性重視
- **Application層**: Railway-oriented Programming・関数型パラダイム
- **境界層**: TypeConverter活用・F#↔C#相互運用

## 次回セッション準備事項
- **SubAgent最適化**: Command体系見直し・効率化検討
- **レトロスペクティブ**: Phase A8-A9実行記録分析・改善提案