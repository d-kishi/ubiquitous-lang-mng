# Project Overview - ユビキタス言語管理システム

## プロジェクト基本情報
- **名称**: ユビキタス言語管理システム (Ubiquitous Language Manager)
- **目的**: DDD開発における用語統一・管理を支援するWebアプリケーション
- **開発方式**: スクラム開発（1-2週間スプリント）
- **アーキテクチャ**: Clean Architecture + DDD

## 技術スタック

### アーキテクチャ構成
```
Web (C# Blazor Server) → Contracts (C# DTOs/TypeConverters) → Application (F# UseCases) → Domain (F# Models)
                      ↘ Infrastructure (C# EF Core/Repository) ↗
```

### 主要技術
- **Frontend**: Blazor Server + Bootstrap 5
- **Backend**: ASP.NET Core 8.0 + Entity Framework Core
- **Domain/Application**: F# 8.0（関数型プログラミング）
- **Database**: PostgreSQL 16 (Docker)
- **認証**: ASP.NET Core Identity（カスタム拡張）
- **開発環境**: Docker Compose + VS Code

## 現在の開発フェーズ

### Phase A8 (認証・セキュリティ) - 95%完了
- **期間**: 2025-08-26 〜 2025-08-31
- **主要成果**:
  - 初期ユーザーパスワード不整合解決（TECH-002完全解決）
  - ルートパス競合問題解決（Pages/Index vs Components/Pages/Home）
  - 仕様準拠認証フロー実装（平文初期パスワード対応）
  - SubAgent組み合わせパターンC成功実行
- **残作業**: ログイン動作検証（admin@ubiquitous-lang.com / su）
- **次回**: ログイン確認→Phase A8完了承認→Phase A9開始準備

### 完了済みフェーズ
- **Phase A1-A6**: 基本機能・認証基盤（2025-08完了）
- **Phase A7**: 要件準拠・アーキテクチャ統一（2025-08-26完了）

## 技術負債管理状況

### 解決済み
- **TECH-002**: 初期パスワード不整合 → 完全解決（2025-08-31）

### 対応予定
- **TECH-006**: Headers read-only error → GitHub Issue #17で対応計画済み
- **Issue #16**: ポート設定不整合 → 解決済み
- **Issue #17**: 実行エラー自動修正機構 → 次回対応予定

## データベース設計

### 認証関連テーブル
- **AspNetUsers**: ASP.NET Core Identity拡張
  - InitialPassword: 平文初期パスワード（初回ログイン用）
  - PasswordHash: ハッシュ化パスワード（初回ログイン後設定）
- **ユビキタス言語**: 用語定義・管理テーブル群

### 初期データ
- **スーパーユーザー**: admin@ubiquitous-lang.com
- **初期パスワード**: "su"（平文、PasswordHash=NULL）
- **仕様**: 初回ログイン→パスワード変更→ハッシュ値設定

## 開発・運用コマンド

### ビルド・実行
```bash
docker-compose up -d                                   # PostgreSQL起動
dotnet run --project src/UbiquitousLanguageManager.Web # アプリ起動
```

### テスト
```bash
dotnet test                                            # 全テスト実行
dotnet test --collect:"XPlat Code Coverage"           # カバレッジ測定
```

### アクセスURL
- **アプリ**: https://localhost:5001
- **PgAdmin**: http://localhost:8080
- **Smtp4dev**: http://localhost:5080

## コード品質状況

### 最新品質指標（2025-08-31）
- **ビルド**: 0 Warning, 0 Error
- **統合テスト**: 98/100点合格
- **仕様準拠度**: 98%達成
- **アーキテクチャ**: Clean Architecture完全準拠

### 開発手法
- **TDD**: Red-Green-Refactor サイクル
- **SubAgent**: 並列・順次実行による効率化
- **継続的品質改善**: Phase毎の品質評価・改善

## 組織・管理体制

### SubAgent組み合わせパターン
- **パターンA**: 機能拡張（新機能開発）
- **パターンB**: 問題解決（バグ修正・技術負債対応）
- **パターンC**: 品質改善（リファクタリング・最適化）

### ドキュメント管理
- **ADR**: 技術意思決定記録（16件）
- **日次記録**: セッション毎の作業記録・学習内容
- **技術負債**: GitHub Issues連携管理

## 今後の開発計画

### Phase A9 (予定)
- **フォーカス**: ユビキタス言語CRUD機能実装
- **期間**: 2025-09-01 〜 2025-09-07
- **主要機能**: 用語登録・検索・編集・削除

### 長期目標
- **Phase B**: 高度検索・分析機能
- **Phase C**: チーム協業機能
- **Phase D**: API・統合機能

## 学習・改善領域

### 技術スキル向上
- **Blazor Server**: ライフサイクル・SignalR最適化
- **F#**: 関数型プログラミングパターン深化
- **Clean Architecture**: 層間結合度最適化

### プロセス改善
- **SubAgent制御**: 大量起動防止機構強化
- **品質保証**: 自動化テスト拡充
- **ドキュメント**: リアルタイム更新機構導入