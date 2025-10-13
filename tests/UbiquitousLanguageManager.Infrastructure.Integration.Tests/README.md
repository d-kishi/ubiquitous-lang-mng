# Infrastructure.Integration.Tests

## 概要
Infrastructure層の統合テストプロジェクト

## Phase B2実装予定
- Repository統合テスト（Testcontainers.PostgreSql使用）
- EF Core統合テスト
- データベーストランザクションテスト

## 実装状況
- [x] プロジェクト作成（Phase B-F1 Step4）
- [ ] 統合テスト実装（Phase B2実施予定）

## テスト対象
- Infrastructure層（EF Core Repository実装）
- データベースアクセス層
- トランザクション制御

## 技術スタック
- xUnit
- Microsoft.AspNetCore.Mvc.Testing
- Microsoft.EntityFrameworkCore.InMemory
- Testcontainers.PostgreSql

## 参照関係
- Infrastructure層（テスト対象）
- Application層（サービス統合テスト用）
- Domain層（ドメインモデル使用）
- Web層（WebApplicationFactory使用のため）
- Contracts層（DTO変換用）
