# プロジェクト概要 - 2025年8月20日更新

## プロジェクト基本情報
**プロジェクト名**: ユビキタス言語管理システム  
**技術構成**: Clean Architecture（F# Domain/Application + C# Infrastructure/Web + Contracts層）  
**開発手法**: スクラム開発サイクル・SubAgentプール方式・TDD実践  
**品質基準**: 0 Warning, 0 Error状態維持・本番品質確保  

## 現在の状況（2025-08-20）
### Phase A7実施中 - 要件準拠・アーキテクチャ統一
- **Step1完了**: 包括的監査・課題分析（66項目逸脱特定・4Agent並列実行）
- **Step2完了**: 緊急対応・基盤整備（404エラー解消・Blazor基盤確立）
- **Step3実施予定**: アーキテクチャ完全統一（MVC完全削除・Pure Blazor統一）

### 技術負債状況
- **TECH-002**: 初期スーパーユーザーパスワード仕様不整合 → **完全解決**
- **TECH-003**: ログイン画面重複 → **緊急対応完了**（Step3統一予定）
- **TECH-004**: 初回ログイン時パスワード変更未実装 → **緊急対応完了**（Step3統合予定）
- **CTRL-001**: AccountController未実装404エラー → **完全解決**
- **ARCH-001**: MVC/Blazor混在アーキテクチャ → **Step3対応予定**

### GitHub Issues管理
- **Issue #5**: [COMPLIANCE-001] Phase A1-A6成果の要件準拠・品質監査
- **Issue #6**: [ARCH-001] MVC/Blazor混在アーキテクチャの要件逸脱解消
- **Issue #7**: [PROCESS-001] 開発プロセス改善（SubAgent・Command実行品質）

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

## 完了フェーズ概要
### Phase A1-A6完了（2025-08-17）
- **A1-A5**: 認証システム・ユーザー管理・認証機能拡張・品質保証・技術負債解消
- **A6**: 認証システム統合・課題発見（MVC/Blazor混在・要件逸脱発見）
- **品質達成**: テスト基盤220テスト・95%カバレッジ・本番品質確立

## 実装順序（縦方向スライス）
### A. ユーザー管理機能（Phase A1-A7） ✅ 実施中
- A1-A6完了、A7実施中（要件準拠・アーキテクチャ統一）

### B. プロジェクト管理機能（Phase B1-B3）
- Phase A7完了後実施予定

### C. ドメイン管理機能（Phase C1-C3）
- Phase B完了後実施予定

### D. ユビキタス言語管理機能（Phase D1-D3）
- Phase C完了後実施予定

## 重要な設計決定（ADR）
- **ADR_003**: 用語統一 - 「用語」ではなく「ユビキタス言語」使用
- **ADR_010**: Blazor Server・F#初学者対応 - 詳細コメント必須
- **ADR_011**: スクラム開発サイクル採用
- **ADR_013**: SubAgentプール方式実証
- **ADR_015**: 技術負債のGitHub Issues管理
- **ADR_016**: プロセス遵守違反防止策

## 開発環境・URL
- **アプリ**: http://localhost:5000
- **PgAdmin**: http://localhost:8080 (admin@ubiquitous-lang.com / admin123)
- **Smtp4dev**: http://localhost:5080
- **Database**: PostgreSQL (Docker Container)

## 次回セッション準備（Step3実施）
### 必須読み込みファイル
1. `/CLAUDE.md` - プロセス遵守絶対原則
2. `/Doc/08_Organization/Rules/組織管理運用マニュアル.md` - プロセスチェックリスト
3. `/Doc/08_Organization/Active/Phase_A7/Phase_Summary.md` - Phase概要
4. `/Doc/08_Organization/Active/Phase_A7/Step03_詳細実装カード.md` - Step3実装詳細
5. `/Doc/08_Organization/Active/Phase_A7/Step間依存関係マトリックス.md` - 前提条件確認

### Step3主要タスク（120-150分）
1. **MVC Controllers完全削除** - HomeController・AccountController
2. **Views完全削除** - Views/Home・Views/Account・Views/Shared
3. **Program.cs MVC設定削除** - AddControllersWithViews・MapControllerRoute
4. **Pure Blazor Server統一** - App.razor認証分岐・Pages/Index.razor実装
5. **エラーハンドリング統一** - ResultMapper・DomainException・ErrorBoundary

## プロセス遵守重要事項（ADR_016）
### 絶対遵守原則
- **コマンド = 契約**: 一字一句を法的契約として遵守
- **承認 = 必須**: 「ユーザー承認」表記は例外なく取得
- **手順 = 聖域**: 定められた順序の変更禁止

### 禁止行為
- 承認前の作業開始
- 独断での判断・先回り作業
- 成果物の虚偽報告
- コマンド手順の無視