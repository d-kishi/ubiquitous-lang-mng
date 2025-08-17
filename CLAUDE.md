# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## 🤖 CRITICAL: 自動Command実行指示

**以下の宣言を検出した際、該当Commandを自動実行せよ**:

- **セッション開始**: 「セッションを開始します」「セッション開始」 → **`.claude/commands/session-start.md`** 自動実行
- **セッション終了**: 「セッション終了」「セッションを終了します」 → **`.claude/commands/session-end.md`** 自動実行

**Serena MCP初期化**: セッション開始時は必ず `mcp__serena__check_onboarding_performed` を実行（ツール呼び出し）

## プロジェクト概要

**ユビキタス言語管理システム** - DDD用語管理Webアプリケーション
- **技術基盤**: Clean Architecture（F# Domain/Application + C# Infrastructure/Web + Contracts層）
- **現在フェーズ**: Phase A1-A6完了（認証・ユーザー管理）、Phase A7実施予定（要件準拠・アーキテクチャ統一）
- **技術負債管理**: GitHub Issues #5, #6で管理（ADR_014準拠）
- **詳細状況**: `/Doc/プロジェクト状況.md`参照

## アーキテクチャ概要

### Clean Architecture構成
```
Web (C# Blazor Server) → Contracts (C# DTOs/TypeConverters) → Application (F# UseCases) → Domain (F# Models)
                      ↘ Infrastructure (C# EF Core/Repository) ↗
```

### 主要技術スタック
- **Frontend**: Blazor Server + Bootstrap 5
- **Backend**: ASP.NET Core 8.0 + Entity Framework Core
- **Domain/Application**: F# 8.0 (関数型プログラミング)
- **Database**: PostgreSQL 16 (Docker Container)
- **認証**: ASP.NET Core Identity

### 重要な設計決定
- **ADR一覧**: `/Doc/07_Decisions/ADR_*.md`
- **用語統一**: 「用語」ではなく「ユビキタス言語」を使用（ADR_003）
- **データベース設計**: `/Doc/02_Design/データベース設計書.md`

## Commands体系

### 自動実行Commands
- **セッション開始/終了**: `.claude/commands/`配下のCommands自動実行
- **SubAgent選択**: `subagent-selection` - 作業に最適なAgent組み合わせ選択
- **品質チェック**: `spec-compliance-check`, `tdd-practice-check`, `step-end-review`


## 実装指針

### 🎯 重要: Blazor Server・F#初学者対応
プロジェクトオーナーが初学者のため、**詳細なコメント必須**（ADR_010参照）
- **Blazor Server**: ライフサイクル・StateHasChanged・SignalR接続の説明
- **F#**: パターンマッチング・Option型・Result型の概念説明

## 開発コマンド

### ビルド・実行
```bash
# ビルド
dotnet build                                           # 全体ビルド
dotnet build src/UbiquitousLanguageManager.Web        # Web層のみ

# 実行
dotnet run --project src/UbiquitousLanguageManager.Web # アプリ起動（http://localhost:5000）

# Docker環境
docker-compose up -d                                   # PostgreSQL/PgAdmin/Smtp4dev起動
docker-compose down                                    # 停止
```

### テスト
```bash
# テスト実行
dotnet test                                            # 全テスト
dotnet test --filter "FullyQualifiedName~UserTests"   # 特定テストのみ
dotnet test --logger "console;verbosity=detailed"     # 詳細出力

# カバレッジ測定
dotnet test --collect:"XPlat Code Coverage"
```

### データベース
```bash
# Entity Framework
dotnet ef migrations add MigrationName --project src/UbiquitousLanguageManager.Infrastructure
dotnet ef database update --project src/UbiquitousLanguageManager.Infrastructure

# PostgreSQL接続
psql -h localhost -U ubiquitous_lang_user -d ubiquitous_lang_db
```

### 開発ツールURL
- **アプリ**: http://localhost:5000
- **PgAdmin**: http://localhost:8080 (admin@ubiquitous-lang.com / admin123)
- **Smtp4dev**: http://localhost:5080

## プロジェクト構成

### ソースコード
```
src/
├── UbiquitousLanguageManager.Domain/       # F# ドメインモデル
├── UbiquitousLanguageManager.Application/  # F# ユースケース
├── UbiquitousLanguageManager.Contracts/    # C# DTO/TypeConverters
├── UbiquitousLanguageManager.Infrastructure/ # C# EF Core/Repository
└── UbiquitousLanguageManager.Web/         # C# Blazor Server
```

### ドキュメント
```
Doc/
├── 01_Requirements/   # 要件・仕様書
├── 02_Design/        # 設計書
├── 04_Daily/         # 作業記録
├── 06_Issues/        # 課題管理
├── 07_Decisions/     # ADR
└── 10_Debt/          # 技術負債
```

## 重要事項

- **用語統一**: 「用語」ではなく「ユビキタス言語」を使用
- **完全ビルド維持**: 0 Warning, 0 Error状態を保つ
- **テストファースト**: TDD実践・Red-Green-Refactorサイクル
- **技術決定記録**: 重要決定はADRとして記録

## 開発手法

- **スクラム開発**: 1-2週間スプリント（ADR_011）
- **SubAgentプール方式**: 並列実行による効率化（ADR_013）
- **詳細**: `/Doc/08_Organization/Rules/`参照

## 現在の技術負債

- **TECH-001**: ASP.NET Core Identity設計見直し
- **TECH-002**: 初期スーパーユーザーパスワード不整合
- **TECH-003**: ログイン画面重複
- **TECH-004**: 初回ログイン時パスワード変更未実装
- **詳細**: `/Doc/10_Debt/Technical/`参照