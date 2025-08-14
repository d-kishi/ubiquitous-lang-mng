# プロジェクト概要

## プロジェクトの目的
**ユビキタス言語管理システム** - Domain-Driven Design (DDD) における用語管理を行うWebアプリケーション

## 技術スタック

### アーキテクチャ
- **Clean Architecture** 採用
- F# (Domain/Application層) + C# (Infrastructure/Web/Contracts層) のハイブリッド構成

### 技術構成
- **フロントエンド**: Blazor Server + Bootstrap 5
- **バックエンド**: ASP.NET Core 8.0 + Entity Framework Core
- **データベース**: PostgreSQL 16 (Docker Container)
- **認証**: ASP.NET Core Identity
- **関数型言語**: F# 8.0 (ドメイン層・アプリケーション層)
- **オブジェクト指向言語**: C# (インフラ層・Web層・契約層)

### プロジェクト構造
```
src/
├── UbiquitousLanguageManager.Domain/       # F# ドメインモデル
├── UbiquitousLanguageManager.Application/  # F# ユースケース
├── UbiquitousLanguageManager.Contracts/    # C# DTO/TypeConverters
├── UbiquitousLanguageManager.Infrastructure/ # C# EF Core/Repository
└── UbiquitousLanguageManager.Web/         # C# Blazor Server
```

## アーキテクチャ図
```
Web (C# Blazor Server) → Contracts (C# DTOs/TypeConverters) → Application (F# UseCases) → Domain (F# Models)
                      ↘ Infrastructure (C# EF Core/Repository) ↗
```

## 現在の状況
- **完了フェーズ**: Phase A1-A4 (認証・ユーザー管理システム完成)
- **現在フェーズ**: Phase A5 (技術負債解消・ASP.NET Core Identity設計見直し)
- **次期フェーズ**: Phase B1 (プロジェクト管理機能)

## 開発手法
- **スクラム開発**: 1-2週間スプリント
- **TDD実践**: Red-Green-Refactorサイクル
- **Clean Architecture**: 層間依存関係の厳格管理
- **SubAgentプール方式**: 並列実行による効率化