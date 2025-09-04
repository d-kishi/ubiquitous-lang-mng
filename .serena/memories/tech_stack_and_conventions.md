# 技術スタック・規約 - 2025-09-04更新

## 技術スタック（本番対応済み）

### フロントエンド技術
- **Blazor Server**: ASP.NET Core 8.0サーバーサイドレンダリング
- **Bootstrap 5**: レスポンシブデザイン用UIフレームワーク
- **SignalR**: リアルタイム通信（WebSocketフォールバック）
- **認証**: ASP.NET Core Identity統合完了

### バックエンド技術
- **ASP.NET Core 8.0**: Webフレームワーク + APIエンドポイント
- **Entity Framework Core**: PostgreSQL ORM統合
- **ASP.NET Core Identity**: 認証・認可システム
- **HTTP文脈分離**: AuthApiControllerパターンによるAPI分離

### Domain/Application層（F#統合完了）
- **F# 8.0**: ドメインロジック関数型プログラミング
- **TypeConverter統合**: 580行C#/F#境界管理
- **Clean Architecture**: Domain/Application層F#実装準備完了
- **ビジネスルール**: パターンマッチング + Option型 + Result型

### データベース技術
- **PostgreSQL 16**: 本番データベース（Dockerコンテナ化）
- **Entity Framework Core**: Code-Firstマイグレーションアプローチ
- **Docker Compose**: 開発環境自動化
- **pgAdmin 4**: データベース管理インターフェース

### 開発環境
- **Dockerコンテナ**: PostgreSQL + pgAdmin + Smtp4dev
- **VS Code**: 主要開発環境
- **Git**: 従来的コミット規約によるバージョン管理
- **GitHub Issues**: 技術負債 + 機能追跡

## コード規約・基準

### C#規約
- **ファイル構成**: 機能ベースフォルダ構造
- **命名**: クラス・メソッドPascalCase、変数camelCase
- **コメント**: F#初学者向け詳細説明（ADR_010）
- **認証**: UserManagerパターン必須（直接DbContext禁止）

### F#規約（Domain層）
- **モジュール構成**: ドメイン駆動設計モジュール構造
- **型定義**: ドメインモデリング用判別共用体
- **関数合成**: データ変換用パイプライン演算子|>
- **エラーハンドリング**: 明示的エラーハンドリング用Result<'T, 'Error>型

### TypeConverterパターン（重要統合）
```csharp
// C# Contracts層 - F#/C#境界管理
public class EntityTypeConverter 
{
    public static CSharpDto FromFSharpType(FSharpDomainType fsType) { /* ... */ }
    public static FSharpDomainType ToFSharpType(CSharpDto dto) { /* ... */ }
}
```

### データベース規約
- **Entity Framework**: 明示的設定によるCode-First
- **マイグレーション**: タイムスタンプ付き説明的命名
- **シード**: 一貫した初期データ用InitialDataService
- **接続**: 標準認証情報によるDocker PostgreSQL

## テスト基準（100%成功達成）

### 統合テスト
- **TestWebApplicationFactory**: Webアプリテスト標準パターン
- **データベース**: クリーンアップ付き独立テストデータベース
- **認証**: 信頼できるテスト用UserManager同一スコープパターン
- **カバレッジ**: 106/106テスト成功維持

### 単体テスト基準
- **Arrange-Act-Assert**: 明確テスト構造
- **説明的名前**: メソッド名でテスト意図明示
- **F#テスト**: プロパティベーステスト考慮関数型アプローチ
- **モック戦略**: 最小モック・統合テスト優先

### テスト構成
```
test/
├── UnitTests/           # ドメインロジック単体テスト
├── IntegrationTests/    # Web API + データベース統合
└── FunctionalTests/     # エンドツーエンドシナリオ
```

## Clean Architecture実装

### 層責任
- **Domain（F#）**: ビジネスルール・エンティティ・ドメインサービス
- **Application（F#）**: ユースケース・アプリケーションサービス
- **Contracts（C#）**: DTO・TypeConverter・インターフェース定義
- **Infrastructure（C#）**: データベース・外部サービス・リポジトリ
- **Web（C#）**: コントローラー・Blazorコンポーネント・UIロジック

### 依存方向（厳密実施）
```
Web → Contracts → Application → Domain
   ↘ Infrastructure ↗
```

### TypeConverter統合ポイント
- **Web → Contracts**: UIバインディング用DTO変換
- **Contracts → Application**: F#型変換
- **Application → Domain**: 純粋F#ドメイン操作
- **Infrastructure → Contracts**: データベースエンティティ変換

## セキュリティ基準（本番対応）

### 認証実装
- **ASP.NET Core Identity**: UserManagerによる完全統合
- **パスワード管理**: ハッシュ化保存・平文禁止
- **初期ユーザー**: admin@ubiquitous-lang.com「su」パスワード
- **セッション管理**: セキュアクッキーによるASP.NET Coreセッション

### HTTPS強制
- **開発**: https://localhost:5001必須
- **本番**: HTTPS リダイレクト有効
- **証明書**: ローカルHTTPS用開発証明書

## パフォーマンス基準

### データベースパフォーマンス
- **接続プール**: Entity Frameworkデフォルトプール
- **クエリ最適化**: LINQ to Entitiesベストプラクティス
- **インデックス**: キークエリ用戦略的データベースインデックス
- **マイグレーション**: ノンブロッキングマイグレーション戦略

### アプリケーションパフォーマンス
- **Blazor Server**: サーバーサイドレンダリング最適化
- **SignalR**: 効率的リアルタイム通信
- **キャッシュ**: 頻繁アクセスデータ用インメモリキャッシュ
- **Async/Await**: 一貫した非同期プログラミング

## 開発ワークフロー基準

### Gitワークフロー
- **ブランチ戦略**: mainからのフィーチャーブランチ
- **コミットメッセージ**: 従来的コミット仕様
- **コードレビュー**: mainブランチプルリクエスト必須
- **継続的統合**: コミット時自動テスト

### 品質ゲート
- **ビルド**: 0警告・0エラー必須
- **テスト**: 100%成功率必要
- **カバレッジ**: 95%以上テストカバレッジ維持
- **仕様準拠**: 95点以上目標

## Docker環境基準

### コンテナ設定
```yaml
# PostgreSQL設定
POSTGRES_DB: ubiquitous_lang_db
POSTGRES_USER: ubiquitous_lang_user  
POSTGRES_PASSWORD: ubiquitous_lang_password
Port: 5432

# pgAdmin設定
PGADMIN_DEFAULT_EMAIL: admin@ubiquitous-lang.com
PGADMIN_DEFAULT_PASSWORD: admin123
Port: 8080

# Smtp4dev設定
Port: 5080 (Web UI), 2525 (SMTP)
```

### ボリューム管理
- **データベース永続化**: PostgreSQLデータボリューム
- **開発**: ファイル監視によるホットリロード
- **クリーンアップ**: 新規開始用コンテナクリーンアップスクリプト

## エラーハンドリング基準

### アプリケーションエラー
- **グローバル例外ハンドラー**: 一貫したエラーレスポンス形式
- **ログ**: Serilog統合による構造化ログ
- **ユーザーメッセージ**: 日本語ユーザーフレンドリーエラーメッセージ
- **開発者情報**: 開発モード詳細エラー情報

### F#エラーハンドリング
```fsharp
// 明示的エラーハンドリング用Result型
type ValidationResult<'T> = 
    | Success of 'T
    | ValidationError of string list
    | BusinessError of string
```

### データベースエラーハンドリング
- **接続復旧**: 一時的障害用再試行ポリシー
- **トランザクション管理**: 明示的トランザクション境界
- **制約違反**: ビジネスルール用意味的エラーメッセージ
- **マイグレーション障害**: ロールバック戦略・検証