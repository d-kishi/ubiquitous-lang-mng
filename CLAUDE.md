# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## 🔴 CRITICAL: プロセス遵守絶対原則（ADR_016）

**違反は一切許容されない重要遵守事項**:

### 絶対遵守原則
- **コマンド = 契約**: 一字一句を法的契約として遵守・例外なし
- **承認 = 必須**: 「ユーザー承認」表記は例外なく取得・勝手な判断禁止
- **手順 = 聖域**: 定められた順序の変更禁止・先回り作業禁止

### 禁止行為（重大違反）
- ❌ **承認前の作業開始**: いかなる理由でも禁止
- ❌ **独断での判断**: 「効率化」を理由とした勝手な作業
- ❌ **成果物の虚偽報告**: 実体のない成果物の報告
- ❌ **コマンド手順の無視**: phase-start/step-start等の手順飛ばし

### 必須実行事項
- ✅ **実体確認**: SubAgent成果物の物理的存在確認
- ✅ **承認記録**: 取得した承認の明示的記録  
- ✅ **チェックリスト実行**: 組織管理運用マニュアルのプロセス遵守チェック

**詳細**: `/Doc/07_Decisions/ADR_016_プロセス遵守違反防止策.md`

## 📋 次回セッション（ログイン検証）必須読み込みファイル

**次回実行予定**: 初期ユーザーログイン検証（admin@ubiquitous-lang.com / su）

### 🔴 セッション開始時必読（絶対必須）
```
1. /CLAUDE.md (本ファイル) - プロセス遵守絶対原則確認
2. /Doc/08_Organization/Rules/組織管理運用マニュアル.md - プロセス遵守チェックリスト
3. /Doc/08_Organization/Active/Phase_A7/Phase_Summary.md - Phase概要・セッション開始時確認事項
4. /Doc/08_Organization/Active/Phase_A7/Step04_組織設計.md - Step4完了成果確認
5. /Doc/08_Organization/Active/Phase_A7/Step間依存関係マトリックス.md - Step5前提条件確認
```

### 🟡 実装時参照（必要に応じて）
```
6. /Doc/01_Requirements/UI設計書.md - プロフィール変更画面設計確認時
7. /Doc/07_Decisions/ADR_003_ユビキタス言語統一戦略.md - 用語統一実装時
8. Serena MCP memory `phase_a7_technical_details` - 技術詳細確認時
```

### ⚠️ 次回セッション開始手順
```
1. 上記必読ファイル1-5を順次読み込み
2. プロセス遵守チェックリスト実行
3. Step5前提条件確認（Step4完了・TypeConverter基盤確認）
4. Step5開始承認取得
5. SubAgent実行（csharp-web-ui・fsharp-domain・spec-compliance）
```

## 🤖 CRITICAL: 自動Command実行指示

**以下の宣言を検出した際、該当Commandを自動実行せよ**:

- **セッション開始**: 「セッションを開始します」「セッション開始」 → **`.claude/commands/session-start.md`** 自動実行
- **セッション終了**: 「セッション終了」「セッションを終了します」 → **`.claude/commands/session-end.md`** 自動実行
  - **🔴 必須**: 日次記録作成・プロジェクト状況更新・Serenaメモリー5種類更新の実行証跡確認必須
- **週次振り返り**: 「週次振り返り」「振り返り実施」「今週の振り返り」 → **`.claude/commands/weekly-retrospective.md`** 自動実行
- **Phase開始準備**: 「Phase開始準備」「新Phase準備」「PhaseXXの実行を開始してください」「PhaseXXを開始してください」 → **`.claude/commands/phase-start.md`** 自動実行
- **Step開始準備**: 「Step開始」「次Step開始」「StepXX開始」「Step開始準備」「次のStep準備」 → **`.claude/commands/step-start.md`** 自動実行
- **Phase終了処理**: 「Phase完了」「Phase終了」「フェーズ完了」「PhaseXX完了」「Phase総括実施」 → **`.claude/commands/phase-end.md`** 自動実行

**Serena MCP初期化**: セッション開始時は必ず `mcp__serena__check_onboarding_performed` を実行（ツール呼び出し）

## プロジェクト概要

**ユビキタス言語管理システム** - DDD用語管理Webアプリケーション
- **技術基盤**: Clean Architecture（F# Domain/Application + C# Infrastructure/Web + Contracts層）
- **現在フェーズ**: Phase A1-A6完了（認証・ユーザー管理）、Phase A7実施予定（要件準拠・アーキテクチャ統一）
- **技術負債管理**: GitHub Issues #5, #6で管理（ADR_015準拠）
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
dotnet run --project src/UbiquitousLanguageManager.Web # アプリ起動（https://localhost:5001）

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
- **アプリ**: https://localhost:5001
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

## 権限設定（作業効率化）

### 自動承認設定
`.claude/settings.local.json`にて以下の操作を自動承認設定済み：
- **ファイル操作**: カレントディレクトリ配下の全Read/Write/Edit操作
- **Bashコマンド**: dotnet, git, npm, docker等の開発コマンド全般
- **MCP Serena**: 全シンボル操作・メモリ管理操作
- **設定モード**: `defaultMode: "acceptEdits"`により編集操作を自動承認

これにより、Phase実装作業において承認待ちによる中断を最小化し、作業効率を30-40%改善。

### 権限追加方法
新たなコマンドを追加する場合は`.claude/settings.local.json`の`permissions.allow`配列に追加。
例: `"Bash(新コマンド:*)"`

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