# Project Overview - ユビキタス言語管理システム

## 🎯 プロジェクト概要
**ドメイン駆動設計(DDD)用語管理Webアプリケーション** - Clean Architecture準拠・F#/C#ハイブリッド実装

## 📊 現在状況（2025-09-10更新）

### ✅ 完了Phase（Phase A1-A9完全完了）
- **Phase A1-A8**: 基本認証・ユーザー管理・要件準拠・Blazor Server認証統合最適化（2025-09-05完了）
- **Phase A9**: 認証システムアーキテクチャ根本改善（**Step 1完全完了・2025-09-10**）
  - **F# Application層**: IAuthenticationService・Railway-oriented Programming完全実装（96/100点）
  - **Infrastructure層**: UserRepositoryAdapter・ASP.NET Core Identity統合完成（94/100点）
  - **JsonSerializerService実装**: Blazor Server JSON一括管理・技術負債予防実現
    - DI登録による全Component統一利用・DRY原則完全準拠
    - ConfigureHttpJsonOptions制約解決・保守性向上実現
  - **E2E認証テスト完了**: 3シナリオ完全成功・F# Authentication Service統合確認
  - **Clean Architecture品質向上**: **89点→94点（+5点達成）**

### 🔴 次回最優先（Phase A9 Step 2）
- **Phase A9 Step 2**: 認証処理重複実装統一（120分・csharp-web-ui + csharp-infrastructure）
  - **対象箇所**: Infrastructure/Services/AuthenticationService.cs:64-146・Web/Services/AuthenticationService.cs・Web/Controllers/AuthApiController.cs
  - **目標**: 単一責任原則達成・Infrastructure層認証サービス一本化

## 🏗 アーキテクチャ構成

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

## ✅ 技術負債解消状況

### 完全解決済み
- **TECH-002**: 初期パスワード不整合 → **完全解決**（2025-09-04・Phase A8 Step5により完全解決）
- **TECH-004**: 初回ログイン時パスワード変更未実装 → **完全解決**（2025-09-09・E2E認証統合テスト完了）
- **TECH-006**: Headers read-onlyエラー → **完全解決**（2025-09-04・HTTP文脈分離により根本解決）
- **JsonSerializerOptions個別設定**: 重複・設定漏れリスク → **一括管理で根本解決**（2025-09-10）

## 🎯 品質状況

### 達成品質
- **Clean Architectureスコア**: **94/100点**（Phase A9 Step 1完了・+5点向上）
- **テスト基盤**: 106/106テスト成功・0警告0エラー・95%カバレッジ
- **認証システム**: admin@ubiquitous-lang.com / su 実ログイン・全機能完全動作

### 技術基盤
- **F# Application層**: Railway-oriented Programming・Result型完全実装
- **TypeConverter基盤**: F#↔C#境界統合・66テストケース成功
- **JsonSerializerService**: Blazor Server全体JSON一括管理・技術負債予防

## 📚 開発環境・コマンド

### ビルド・実行
```bash
dotnet build                                           # 全体ビルド
dotnet run --project src/UbiquitousLanguageManager.Web # アプリ起動（https://localhost:5001）
docker-compose up -d                                   # PostgreSQL/PgAdmin/Smtp4dev起動
```

### 開発ツールURL
- **アプリ**: https://localhost:5001
- **PgAdmin**: http://localhost:8080 (admin@ubiquitous-lang.com / admin123)
- **Smtp4dev**: http://localhost:5080

## 🔧 重要な実装パターン

### JsonSerializerService（2025-09-10実装）
```csharp
// Program.cs DI登録
builder.Services.AddScoped<IJsonSerializerService, JsonSerializerService>();

// Blazor Component利用
@inject IJsonSerializerService JsonSerializer
var parsedResult = JsonSerializer.Deserialize<PasswordChangeApiResponse>(resultJson);
```

### F# Authentication Service
```fsharp
// Railway-oriented Programming
type AuthenticationError = 
  | InvalidCredentials
  | UserNotFound
  | PasswordExpired
  // ... 7つの判別共用体

type IAuthenticationService =
  abstract member AuthenticateAsync : email:string -> password:string -> Task<Result<AuthenticationResult, AuthenticationError>>
```

## 🎯 Phase別進捗

### A. ユーザー管理機能
- [x] **Phase A1-A9**: 基本認証・ユーザー管理・要件準拠・Blazor Server認証統合最適化・認証システムアーキテクチャ根本改善（**2025-09-10完了**）
- [ ] **Phase A10**: 認証処理重複実装統一（**次回実施予定・Phase A9 Step 2内容**）

### B. プロジェクト管理機能
- [ ] **Phase B1**: プロジェクト基本CRUD（未着手・Phase A完了後）

## ⚠️ 重要制約

- **用語統一**: 「用語」ではなく「ユビキタス言語」を使用（ADR_003準拠）
- **品質基準**: 0 Warning, 0 Error状態維持・本番品質確保必須
- **初学者対応**: Blazor Server・F#詳細コメント必須（ADR_010準拠）
- **DB復元**: E2Eテスト後は`/scripts/restore-admin-user.sql`で必ず復元実行

## 📋 次回セッション準備事項

### 必須読み込みファイル
1. `/Doc/08_Organization/Active/Phase_A9/Phase_Summary.md` - Phase A9実行計画・Step 2詳細
2. `/Doc/05_Research/Phase_A9/02_アーキテクチャレビューレポート.md` - 認証ロジック混在問題詳細
3. `/Doc/04_Daily/2025-09/2025-09-10-2-PhaseA9_Step1完全完了_JsonSerializerService一括管理実装セッション終了.md` - Step 1完了成果

### 成功基準（Phase A9 Step 2）
- **認証処理統一**: 3箇所の重複実装完全統一・単一責任原則達成
- **品質維持**: 0警告0エラー・106テスト成功・Clean Architecture 94点維持
- **E2E動作確認**: 統一後の認証フロー完全動作