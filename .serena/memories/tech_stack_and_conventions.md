# 技術スタック・開発規約（2025-09-22更新・コマンド更新対応）

## アーキテクチャ
- **Clean Architecture**: F# Domain/Application + C# Infrastructure/Web + Contracts層
- **スコア**: 97/100点達成（要件85-90点を大幅超過）
- **F# Domain層活用**: 85%達成（Railway-oriented Programming）
- **TypeConverter基盤**: 1,539行完成（F#↔C#境界効率変換）

## 技術スタック
- **Frontend**: Blazor Server + Bootstrap 5
- **Backend**: ASP.NET Core 8.0 + Entity Framework Core
- **Domain/Application**: F# 8.0（関数型プログラミング）
- **Database**: PostgreSQL 16（Docker Container）
- **認証**: ASP.NET Core Identity統合
- **ログ管理**: Microsoft.Extensions.Logging + 構造化ログ

## 開発規約・品質基準
- **品質基準**: 0 Warning, 0 Error状態維持必須
- **TDD実践**: Red-Green-Refactorサイクル組み込み
- **詳細コメント必須**: Blazor Server・F#初学者対応（ADR_010）
- **用語統一**: 「用語」ではなく「ユビキタス言語」使用（ADR_003）

## SubAgent活用パターン（更新完了・2025-09-22）

### 基本Pattern（確立済み）
- **Pattern A**: 新機能実装（基本実装段階・Domain→Application→Infrastructure→Web UI）
- **Pattern B**: 機能拡張（影響分析→実装・統合→品質保証）
- **Pattern C**: 品質改善（課題分析→改善実装→検証・完成）

### 新規Pattern（コマンド更新で追加）
- **Pattern D**: 品質保証段階（Phase B4-B5, C5-C6, D7等）
  - 技術負債特定→品質改善実装→統合検証・品質確認
- **Pattern E**: 拡張段階（Phase D7-D8等）
  - 外部連携設計→拡張機能実装→運用準備・統合確認

### Agent別専門領域
- **csharp-web-ui**: Blazor Server UI実装（TDD必須）
- **fsharp-domain**: F# ドメインモデル設計・Railway-oriented Programming
- **csharp-infrastructure**: EF Core Repository実装・構造化ログ統合
- **contracts-bridge**: F#↔C# TypeConverter拡張・境界最適化
- **並列実行効果**: 40-50%時間短縮・品質向上実証

## Phase規模・段階管理（新機能・2025-09-22）

### Phase規模判定（自動化）
- **🟢中規模**: Phase B（5段階・5-7セッション・標準SubAgentパターン）
- **🟡大規模**: Phase C（6段階・7-9セッション・専門性強化SubAgentパターン）
- **🔴超大規模**: Phase D（7-8段階・10-12セッション・複雑Phase対応SubAgentパターン）

### 段階種別判定（自動化）
- **基本実装段階（1-3）**: 基本CRUD・関連機能・機能完成
- **品質保証段階（4-6）**: 技術負債解消・UI/UX最適化・統合テスト
- **拡張段階（7-8）**: 高度機能・外部連携・運用最適化

## 開発コマンド
```bash
# ビルド・実行
dotnet build
dotnet run --project src/UbiquitousLanguageManager.Web
docker-compose up -d

# テスト
dotnet test
dotnet test --collect:"XPlat Code Coverage"

# データベース
dotnet ef migrations add MigrationName --project src/UbiquitousLanguageManager.Infrastructure
dotnet ef database update --project src/UbiquitousLanguageManager.Infrastructure
```

## 開発ツールURL
- **アプリ**: https://localhost:5001
- **PgAdmin**: http://localhost:8080
- **Smtp4dev**: http://localhost:5080

## 認証情報（動作確認済み）
- **管理者**: admin@ubiquitous-lang.com / su

## コマンド更新状況（2025-09-22完了）
- **subagent-selection.md**: Pattern D・E追加・段階判断機能追加
- **phase-start.md**: Phase規模判定・段階数自動取得
- **Phase特性別テンプレート.md**: 全面改訂・5-8段階対応
- **SubAgent組み合わせパターン.md**: Pattern D・E詳細追加
- **step-start.md**: 段階種別判定・Stage構成拡張