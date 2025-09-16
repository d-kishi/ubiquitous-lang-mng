# 技術スタック・規約 - 最新状況（2025-09-16更新）

## 🏗️ アーキテクチャ構成

### Clean Architecture完成形（97/100点達成）
```
Web (C# Blazor Server) → Contracts (C# DTOs/TypeConverters) → Application (F# UseCases) → Domain (F# Models)
                      ↘ Infrastructure (C# EF Core/Repository) ↗
```

### 層別技術選択
- **Domain層**: F# 8.0・純粋関数・ビジネスロジック・型安全性85%活用
- **Application層**: F# 8.0・ユースケース・Railway-oriented Programming・Result型
- **Contracts層**: C# 12.0・DTO・TypeConverter基盤（1,539行完成）
- **Infrastructure層**: C# 12.0・ASP.NET Core Identity・Entity Framework Core 8.0
- **Web層**: C# 12.0・Blazor Server・Bootstrap 5・SignalR

## 🔧 主要技術スタック

### フロントエンド
- **Blazor Server**: サーバーサイドレンダリング・リアルタイム更新
- **Bootstrap 5**: レスポンシブデザイン・コンポーネントライブラリ
- **SignalR**: リアルタイム通信・セッション管理

### バックエンド・データベース
- **ASP.NET Core 8.0**: Web API・依存注入・ミドルウェアパイプライン
- **Entity Framework Core 8.0**: ORM・Code First・Migration管理
- **PostgreSQL 16**: RDBMS・Docker Container稼働
- **ASP.NET Core Identity**: 認証・認可・ユーザー管理統合

### 開発・テスト環境
- **Docker Compose**: PostgreSQL・PgAdmin・Smtp4dev統合環境
- **xUnit**: 単体テスト・統合テスト・テストカバレッジ95%
- **WebApplicationFactory**: 統合テストパターン・E2E確認

## 💎 確立済み実装パターン

### F# Domain層パターン
- **Railway-oriented Programming**: Result型・エラーハンドリング・関数合成
- **判別共用体**: AuthenticationError 7種類・型安全エラー分類
- **レコード型**: イミュータブル・型安全・値オブジェクト
- **Option型**: null安全・存在確認・関数型プログラミング

### TypeConverter基盤（1,539行完成）
- **F#↔C#境界最適化**: 双方向データ変換・パフォーマンス最適化
- **認証特化拡張**: AuthenticationConverter 689行・認証ドメイン完全対応
- **DRY原則準拠**: 共通変換ロジック・保守性向上
- **単一責任原則**: 各Converter特化・拡張容易性

### Blazor Server実装パターン
- **JsonSerializerService**: DI統一管理・技術負債予防・ConfigureHttpJsonOptions制約解決
- **ComponentBase継承**: ライフサイクル管理・StateHasChanged適切利用
- **Parameter Binding**: 型安全・双方向バインディング・バリデーション統合

### 認証システム統一パターン（保守負荷50%削減）
- **Infrastructure層一本化**: ASP.NET Core Identity統合・重複実装解消
- **F# Authentication Service**: Application層・型安全・エラーハンドリング統合
- **E2E認証フロー**: admin@ubiquitous-lang.com・完全動作確認済み

## 📋 開発規約・制約

### 必須品質基準
- **0警告0エラー**: 継続維持必須・例外なし
- **テスト成功**: 106/106テスト・95%カバレッジ・継続維持
- **Clean Architecture点数**: 97/100点維持・依存方向遵守

### コード品質規約
- **F# Domain/Application**: 純粋関数・イミューダブル・副作用分離
- **C# Infrastructure/Web**: SOLID原則・DI活用・非同期パターン
- **TypeConverter**: 単一責任・型安全・パフォーマンス最適化

### データベース規約
- **Migration管理**: Code First・段階的変更・後方互換性
- **命名規約**: スネークケース・複数形テーブル・単数形カラム
- **インデックス戦略**: パフォーマンス最適化・クエリ分析基準

## 🚀 パフォーマンス最適化

### Blazor Server最適化
- **SignalR接続管理**: 適切なライフサイクル・リソース管理
- **StateHasChanged最適化**: 必要最小限実行・レンダリング効率化
- **Component分割**: 責任分離・再利用性・保守性向上

### データアクセス最適化
- **Repositoryパターン**: 抽象化・テスト容易性・DI統合
- **非同期処理**: async/await・スレッドプール効率活用
- **クエリ最適化**: EF Core適切利用・N+1問題回避

### F#パフォーマンス
- **型安全コンパイル時最適化**: 実行時エラー削減・パフォーマンス向上
- **イミューダブル最適化**: 構造共有・メモリ効率・並行性
- **関数合成**: 処理効率・コード簡潔性・保守性

## 🔄 技術負債管理

### 解決済み技術負債
- **TECH-002**: 初期パスワード不整合・完全解決（2025-09-04）
- **TECH-004**: 初回ログイン時パスワード変更・完全解決（2025-09-09）
- **TECH-006**: Headers read-onlyエラー・完全解決（2025-09-04）
- **JsonSerializerOptions個別設定**: 一括管理実装・技術負債予防（2025-09-10）

### 技術負債予防パターン
- **JsonSerializerService**: DI一括管理・設定統一・保守性向上
- **TypeConverter基盤**: F#↔C#境界最適化・拡張容易性
- **Clean Architecture**: 依存方向遵守・層責務明確化・保守性

## 📊 品質メトリクス・監視

### 自動化品質チェック
- **dotnet build**: 0警告0エラー継続確認
- **dotnet test**: 106テスト成功・95%カバレッジ継続
- **Clean Architecture**: 依存関係チェック・97点品質維持

### 継続的品質改善
- **週次総括**: 技術品質振り返り・改善点特定
- **SubAgent専門活用**: 各層最適化・品質向上効果測定
- **レトロスペクティブ**: フィードバック収集・次期改善サイクル

## 🔗 外部連携・統合

### 開発ツール連携
- **GitHub Issues**: 課題管理・進捗追跡・品質管理
- **Docker環境**: 統合開発環境・本番環境整合性
- **PgAdmin**: データベース管理・クエリ分析・パフォーマンス確認

### 監視・ログ管理（Issue #24検討中）
- **ILogger統合設計**: 統一ログ管理・問題分析・運用監視
- **エラーハンドリング**: F# Result型・C# Exception・統合エラー管理
- **パフォーマンス監視**: レスポンス時間・リソース使用量・品質維持

**最終更新**: 2025-09-16
**状態**: Clean Architecture 97点確立・技術基盤完成・継続改善準備完了
**重点**: 確立基盤継続活用・技術負債予防・品質メトリクス継続監視