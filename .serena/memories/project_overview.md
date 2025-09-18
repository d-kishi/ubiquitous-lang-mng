# プロジェクト概要（2025-01-19更新）

## プロジェクト基本情報
**ユビキタス言語管理システム** - DDD用語管理Webアプリケーション

### 技術基盤
- **Clean Architecture**: F# Domain/Application + C# Infrastructure/Web + Contracts層
- **品質レベル**: Clean Architecture 97点達成・0 Warning 0 Error維持
- **技術スタック**: ASP.NET Core 8.0 + Blazor Server + F# 8.0 + PostgreSQL 16

### 現在状況（2025-01-19）
- **Phase A1-A9**: 認証システム・ユーザー管理・要件準拠・アーキテクチャ統一**完全完了**
- **GitHub Issue #24**: ログ管理方針設計・実装**完全完了**
- **次回セッション推奨**: SubAgentとCommandの見直し・適正化（60-90分）

### 確立済み技術基盤
1. **認証システム統一**: F# Application層・TypeConverter 1,539行完成・保守負荷50%削減
2. **ログ管理基盤**: Microsoft.Extensions.Logging・構造化ログ・セキュリティ配慮実装
3. **開発体制**: SubAgentプール方式・TDD実践・並列実行40-50%効率化
4. **品質管理**: 0警告0エラー・E2Eテスト・実稼働品質達成

### Phase B1準備状況
- **技術基盤**: 完全確立・即座にプロジェクト管理機能実装可能
- **準備完了事項**: Clean Architecture・認証・ログ・TypeConverter基盤
- **推奨前準備**: SubAgent・Command最適化による開発効率さらなる向上

### 重要な制約・前提
- **開発環境**: docker-compose up -d → dotnet run（https://localhost:5001）
- **認証情報**: admin@ubiquitous-lang.com / su（動作確認済み）
- **DB復元**: E2Eテスト後は/scripts/restore-admin-user.sql実行必須