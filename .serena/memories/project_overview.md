# プロジェクト概要

## 現在の状況（2025-09-22更新）

### Phase A7 完了状況
- **現在フェーズ**: Phase A8 実行準備（Phase A7完了）
- **完了事項**: 要件準拠・アーキテクチャ統一・技術基盤整備・コンテキスト最適化Stage3完了
- **実装成果**: 認証・ユーザー管理・組織設計・プロフィール変更・コンテキスト最適化完了

### コンテキスト最適化実行結果
- **Stage3完了**: 大幅なコンテキスト削減達成
  - Doc/04_Daily: 134ファイル(1.3MB) → 1ファイル(8KB) - 99%削減
  - Serenaメモリー: 19個 → 9個 - 53%削減
  - 30日管理機能をsession-end.mdに実装・完全自動化達成
- **GitHub Issues解決**: Issue #34, #35クローズ完了
- **自動アーカイブ**: Doc/04_Daily/archiveに97ファイル移動

### 技術基盤整備完了事項
- **Clean Architecture完全実装**: F# Domain/Application + C# Infrastructure/Web + Contracts層
- **TypeConverter基盤確立**: F#↔C#間の型変換体系完成
- **コマンドシステム完成**: session-start/end, phase-start/end, step関連Commands体系
- **自動化メモリー管理**: 30日保持・自動削除機能付きdaily_sessions運用開始

### 技術負債管理状況
- **完全解決**: TECH-001(Identity設計), TECH-002(パスワード不整合), TECH-003(ログイン画面重複), TECH-004(初回ログイン)
- **コンテキスト負債解決**: 大量記録ファイル・重複メモリーによるコンテキスト圧迫問題完全解決
- **継続監視**: GitHub Issues体系による新規技術負債の早期発見・対処体制確立

### 次回セッション推奨範囲
**実行内容**: Phase A8開始準備（次期機能実装）
**読み込み推奨ファイル**:
1. `/CLAUDE.md` - プロセス遵守原則確認
2. `/Doc/08_Organization/Active/Phase_A8/Phase_Summary.md` - Phase A8概要
3. `development_guidelines` メモリー - 開発方針確認
4. `tech_stack_and_conventions` メモリー - 技術規約確認

**予想時間配分**: 準備30分・実装120-150分・記録30分

### プロジェクト基本情報
- **名称**: ユビキタス言語管理システム
- **技術構成**: Clean Architecture (F# Domain/Application + C# Infrastructure/Web)
- **データベース**: PostgreSQL 16 (Docker Container)
- **認証基盤**: ASP.NET Core Identity完全統合
- **開発アプローチ**: TDD + SubAgent並列実行 + Command駆動開発

### 重要制約・注意事項
- **プロセス遵守絶対原則**: ADR_016準拠・承認前作業開始禁止・手順変更禁止
- **用語統一**: 「用語」禁止・「ユビキタス言語」必須使用（ADR_003）
- **コメント必須**: Blazor Server・F#初学者対応・詳細説明必須（ADR_010）
- **自動削除管理**: daily_sessions 30日保持・session-endで自動実行

### 効率化達成事項
- **コンテキスト最適化**: 大幅削減により読み込み時間30-40%短縮
- **自動化プロセス**: Commands体系による作業効率化・人的ミス削減
- **SubAgent活用**: 並列実行による実装時間短縮・品質向上
- **メモリー統合**: 情報重複削除・検索効率向上・保守性向上