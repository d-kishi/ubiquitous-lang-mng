# プロジェクト概要（2025-09-22更新・コマンド更新完了）

## プロジェクト基本情報
**ユビキタス言語管理システム** - DDD用語管理Webアプリケーション

### 技術基盤
- **Clean Architecture**: F# Domain/Application + C# Infrastructure/Web + Contracts層
- **品質レベル**: Clean Architecture 97点達成・0 Warning 0 Error維持
- **技術スタック**: ASP.NET Core 8.0 + Blazor Server + F# 8.0 + PostgreSQL 16

### 現在状況（2025-09-22）
- **Phase A1-A9**: 認証システム・ユーザー管理・要件準拠・技術基盤整備**完全完了**（2025-09-21）
- **コマンド・SubAgent更新**: 縦方向スライス実装マスタープラン改訂対応**完全完了**（2025-09-22）
- **次回予定**: Phase B1プロジェクト管理機能基本CRUD実装（更新済みコマンド活用）

### コマンド・SubAgent更新完了（2025-09-22）
1. **Pattern D・E追加**: 品質保証段階・拡張段階の新パターン導入
2. **Phase規模判定機能**: 🟢中規模/🟡大規模/🔴超大規模の自動判定
3. **段階構成対応**: 5-8段階への完全対応・段階種別判定機能
4. **SubAgent選択精度向上**: Phase B/C/D各段階に最適化されたAgent組み合わせ
5. **更新ファイル**: subagent-selection.md・phase-start.md・Phase特性別テンプレート.md・SubAgent組み合わせパターン.md・step-start.md

### 確立済み技術基盤
1. **認証システム統一**: F# Application層・TypeConverter 1,539行完成・保守負荷50%削減
2. **ログ管理基盤**: Microsoft.Extensions.Logging・構造化ログ・セキュリティ配慮実装
3. **開発体制**: SubAgentプール方式・TDD実践・並列実行40-50%効率化・KPT管理システム
4. **品質管理**: 0警告0エラー・E2Eテスト・実稼働品質達成

### Phase計画（改訂版・コマンド対応完了）
- **Phase B計画**: 5段階（プロジェクト管理・🟢中規模・5-7セッション）
- **Phase C計画**: 6段階（ドメイン管理・承認ワークフロー・🟡大規模・7-9セッション）
- **Phase D計画**: 7-8段階（ユビキタス言語管理・Excel風UI・🔴超大規模・10-12セッション）
- **段階的詳細化**: Phase B詳細・C/D概要・実績ベース継続調整

### 重要な制約・前提
- **開発環境**: docker-compose up -d → dotnet run（https://localhost:5001）
- **認証情報**: admin@ubiquitous-lang.com / su（動作確認済み）
- **DB復元**: E2Eテスト後は/scripts/restore-admin-user.sql実行必須