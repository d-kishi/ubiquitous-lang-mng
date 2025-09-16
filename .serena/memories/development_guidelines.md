# 開発ガイドライン - 最新状況（2025-09-16更新）

## 🎯 現在の開発方針

### ✅ 確立済み開発プリンシプル
- **Clean Architecture 97点品質**: 健全な依存関係・層責務分離徹底
- **F# Domain層85%活用**: Railway-oriented Programming・型安全性重視
- **0警告0エラー原則**: 継続維持必須・品質基準確立
- **ADR_016プロセス遵守**: 絶対遵守・違反防止・SubAgent完全活用

### 🔄 レトロスペクティブ機能強化（新規確立）
- **週次総括からのフィードバック活用**: SubAgent・Command改善サイクル
- **継続的改善パターン**: 週次総括→改善施策→次期Phase実験→効果測定
- **スクラムプロセス段階導入**: Phase B1実験開始・GitHub Issue #25管理

## 🏗️ アーキテクチャ指針

### Clean Architecture完成品質（97/100点）
- **Domain層**: F# 純粋関数・ビジネスロジック集約
- **Application層**: F# ユースケース・Railway-oriented Programming
- **Infrastructure層**: C# ASP.NET Core Identity・EF Core統合
- **Web層**: C# Blazor Server・型安全UI実装
- **Contracts層**: F#↔C# TypeConverter基盤（1,539行完成）

### 技術選択基準
- **F# Domain/Application**: 型安全性・関数型プログラミング活用
- **C# Infrastructure/Web**: .NET エコシステム・既存資産活用
- **TypeConverter**: 境界明確化・双方向データ変換最適化

## 🔧 実装プラクティス

### SubAgent活用パターン（確立済み）
- **csharp-web-ui**: Blazor Server UI・20分高品質実装実証
- **fsharp-application**: F# Application層・Railway-oriented Programming
- **csharp-infrastructure**: ASP.NET Core Identity・EF Core統合
- **contracts-bridge**: F#↔C# TypeConverter・境界最適化
- **design-review**: アーキテクチャ健全性確認
- **spec-compliance**: 仕様準拠監査・品質保証

### Command活用パターン（確立済み）
- **session-start**: 必須プロセス・Serena初期化・目的明確化
- **session-end**: 記録完全性・継続性確保・品質評価
- **phase-start/phase-end**: Phase管理・計画策定・完了処理
- **weekly-retrospective**: 改善サイクル・学習記録・次期計画

## 🎯 品質保証体制

### 必須品質基準
- **0警告0エラー**: 継続維持・例外許可なし
- **テスト成功率**: 106/106テスト成功・95%カバレッジ維持
- **E2E動作確認**: admin@ubiquitous-lang.com認証フロー完全動作
- **Clean Architecture点数**: 97点維持・依存方向遵守

### TDD実践プロセス
- **Red-Green-Refactor**: 厳格遵守・品質確保
- **テスト設計**: 仕様準拠・エラーケース網羅
- **リファクタリング**: 継続的改善・技術負債予防

## 🚀 プロセス改善・継続改善

### Step統合効率化（確立済み）
- **重複作業回避**: 実質完了済み判断・30分短縮効果
- **品質維持**: 一貫性確保・作業漏れ防止
- **効率化判断**: 統合可能性評価・品質影響確認

### 課題管理最適化（確立済み）
- **情報源明記**: GitHub Issue番号・ファイルパス・完了後処理記載
- **継続性確保**: 解決済み記録削除・未解決記録適切移行
- **一元管理**: GitHub Issues中心・Doc/10_Debt段階的移行

### 失敗回避パターン（教訓活用）
- **要件集中**: Phase要件100%集中・対症療法回避
- **SubAgent完全活用**: メインエージェント直接作業禁止
- **根本解決重視**: 一時的対応禁止・系統的改善重視

## 📊 メトリクス・評価基準

### 技術品質指標
- **Clean Architecture点数**: 97/100点（要件95点超過）
- **F# Domain層活用率**: 85%（要件80%超過）
- **TypeConverter規模**: 1,539行（580行から165%拡張）
- **保守負荷削減**: 50%削減達成

### プロセス品質指標
- **Command遵守率**: ADR_016完全準拠
- **SubAgent活用効果**: 専門性発揮・並列実行効率化
- **時間効率**: 計画vs実績・効率化要因特定
- **改善サイクル**: 週次総括→施策→実験→測定

## 🔄 継続的改善方針

### レトロスペクティブ活用（新規）
- **週次総括**: 体系的振り返り・改善点抽出
- **SubAgent・Command改善**: フィードバック収集・最適化実施
- **Phase実験**: 改善施策試行・効果測定・次期適用

### スクラム開発プロセス段階導入（検討開始）
- **GitHub Issue #25**: 実装可能性検討・段階計画
- **Phase B1実験**: プロジェクト管理機能実装並行実験
- **メトリクス自動化**: Velocity・Quality・Process指標収集

**最終更新**: 2025-09-16
**状態**: Phase A9完了・技術基盤確立・継続改善サイクル開始準備
**重点**: レトロスペクティブ機能強化・スクラムプロセス段階導入・品質基準維持