# プロジェクト概要 - 2025年8月22日更新

## プロジェクト基本情報
**プロジェクト名**: ユビキタス言語管理システム  
**技術構成**: Clean Architecture（F# Domain/Application + C# Infrastructure/Web + Contracts層）  
**開発手法**: スクラム開発サイクル・SubAgentプール方式・TDD実践  
**品質基準**: 0 Warning, 0 Error状態維持・本番品質確保  

## 現在の状況（2025-08-22）
### Phase A7 Step3完了 - アーキテクチャ完全統一達成
- **Step1完了**: 包括的監査・課題分析（66項目逸脱特定・4Agent並列実行）
- **Step2完了**: 緊急対応・基盤整備（404エラー解消・Blazor基盤確立）
- **Step3完了**: アーキテクチャ完全統一（MVC削除・Pure Blazor Server実現・仕様準拠95%）

### Step3完了成果（2025-08-22完了）
- **MVC要素完全削除**: Controllers/・Views/ディレクトリ物理削除完了
- **Pure Blazor Server実現**: 要件定義4.2.1項100%準拠達成
- **Pages/Index.razor実装**: 認証分岐ルーティング（認証済み→/admin/users・未認証→/login）
- **F#/C#境界統一**: ResultMapper・DomainException実装によるエラーハンドリング統一
- **品質達成**: ビルド成功（0 Warning, 0 Error）・仕様準拠監査95%達成

### 技術負債状況（Step3完了時点）
- **TECH-002**: 初期スーパーユーザーパスワード仕様不整合 → **完全解決**
- **TECH-003**: ログイン画面重複 → **完全解決**（Pure Blazor統一により）
- **TECH-004**: 初回ログイン時パスワード変更機能未実装 → **完全解決**（統合実装済み）
- **CTRL-001**: AccountController未実装404エラー → **完全解決**（削除により根本解決）
- **ARCH-001**: MVC/Blazor混在アーキテクチャ → **完全解決**（Pure Blazor Server実現）

### GitHub Issues管理
- **Issue #5**: [COMPLIANCE-001] Phase A1-A6成果の要件準拠・品質監査 → **重要進展**（仕様準拠95%達成）
- **Issue #6**: [ARCH-001] MVC/Blazor混在アーキテクチャの要件逸脱解消 → **完全解決**

## アーキテクチャ概要
### Clean Architecture構成
```
Web (C# Pure Blazor Server) → Contracts (C# DTOs/TypeConverters) → Application (F# UseCases) → Domain (F# Models)
                            ↘ Infrastructure (C# EF Core/Repository) ↗
```

### 主要技術スタック
- **Frontend**: Pure Blazor Server（MVC完全削除）+ Bootstrap 5
- **Backend**: ASP.NET Core 8.0 + Entity Framework Core
- **Domain/Application**: F# 8.0 (関数型プログラミング)
- **Database**: PostgreSQL 16 (Docker Container)
- **認証**: ASP.NET Core Identity

## 完了フェーズ概要
### Phase A1-A6完了（2025-08-17）
- **A1-A5**: 認証システム・ユーザー管理・認証機能拡張・品質保証・技術負債解消
- **A6**: 認証システム統合・課題発見（MVC/Blazor混在・要件逸脱発見）
- **品質達成**: テスト基盤220テスト・95%カバレッジ・本番品質確立

### Phase A7 Step1-3完了（2025-08-22）
- **Step1**: 包括的監査・課題分析（66項目逸脱特定）
- **Step2**: 緊急対応・基盤整備（404エラー解消・認証基盤確立）
- **Step3**: アーキテクチャ完全統一（Pure Blazor Server実現・仕様準拠95%）

## 実装順序（縦方向スライス）
### A. ユーザー管理機能（Phase A1-A7） 🔄 実施中
- A1-A6完了、A7 Step1-3完了、Step4-6実施予定

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

## 次回セッション（Step4実施）
### 必須読み込みファイル
1. `/CLAUDE.md` - プロセス遵守絶対原則・セッション開始Command自動実行
2. `/Doc/08_Organization/Active/Phase_A7/Step04_詳細実装カード.md` - Step4詳細仕様
3. `/Doc/08_Organization/Active/Phase_A7/Step間依存関係マトリックス.md` - Step4前提条件確認
4. Serena MCP memory `phase_a7_technical_details` - 技術実装詳細

### Step4実施内容（Contracts層・型変換完全実装）
1. **TypeConverter完全実装**: F#↔C#型変換未実装部分
2. **DTO拡張**: Application層インターフェース対応DTO追加
3. **FirstLoginRedirectMiddleware統合**: パス統合完成

## プロセス遵守重要事項（ADR_016）
### 絶対遵守原則
- **コマンド = 契約**: 一字一句を法的契約として遵守
- **承認 = 必須**: 「ユーザー承認」表記は例外なく取得
- **手順 = 聖域**: 定められた順序の変更禁止

### 重要学習事項（Step3経験）
- **物理確認重要性**: SubAgent報告と実際の物理状態確認の必要性
- **並列実行成功**: 3Agent完全並列実行の実証成功
- **仕様準拠監査**: spec-compliance Agentによる定量的品質評価の有効性