# 技術スタック・実装規約

## 技術基盤構成
### Clean Architecture実装
```
Web (C# Blazor Server) → Contracts (C# DTOs/TypeConverters) → Application (F# UseCases) → Domain (F# Models)
                      ↘ Infrastructure (C# EF Core/Repository) ↗
```

### 主要技術スタック
- **Frontend**: Blazor Server + Bootstrap 5
- **Backend**: ASP.NET Core 8.0 + Entity Framework Core
- **Domain/Application**: F# 8.0 (関数型プログラミング)
- **Database**: PostgreSQL 16 (Docker Container)
- **認証**: ASP.NET Core Identity（24時間トークン・セキュリティ強化）
- **Email**: MailKit/MimeKit + Smtp4dev統合

### 開発環境
- **アプリ**: https://localhost:5001
- **PgAdmin**: http://localhost:8080
- **Smtp4dev**: http://localhost:5080

## 実装パターン・規約
### F#/C# 相互運用パターン
- **Contracts層**: TypeConverterパターンでF#ドメインモデル↔C# DTO変換
- **境界明確化**: Domain層（F#）とInfrastructure層（C#）の分離
- **Result型活用**: F# Resultパターンでエラーハンドリング統一

### 認証システム実装パターン（Phase A8完成）
- **ASP.NET Core Identity統合**: Blazor Server Cookie認証
- **パスワード機能**: 変更・リセット完全動作・機能仕様書2.1.3準拠
- **セキュリティ**: 24時間トークン・暗号化・HTTPS強制

### 設定管理パターン
```json
// appsettings.json
{
  "App": {
    "BaseUrl": "https://localhost:5001"
  }
}
```
- **Configuration注入**: `_configuration["App:BaseUrl"]`パターン活用
- **ハードコード排除**: URL・パス等の外部設定化必須

### URL・ルーティングパターン
- **一貫性**: `/forgot-password` ↔ `/reset-password`対応関係明確化
- **仕様準拠**: 機能仕様書との完全一致確保

## テスト・品質保証
### テスト基盤（Phase A8完成状態）
- **テスト数**: 106テスト・100%成功継続
- **カバレッジ**: 95%達成・TestWebApplicationFactoryパターン
- **品質基準**: 0警告0エラー・品質スコア98/100点

### 品質確認コマンド
```bash
dotnet build                    # 0警告0エラー確認
dotnet test                     # 106/106テスト成功確認
docker-compose up -d            # 環境起動
```

## Email・SMTP統合（Phase A8完成）
### パスワードリセットフロー
1. `/forgot-password` → メール送信
2. Smtp4dev → メール受信・リンク確認
3. `/reset-password` → パスワード変更
4. `/login` → 新パスワードログイン

### SMTP設定パターン
```csharp
// SmtpEmailSender.cs実装パターン
var baseUrl = _configuration["App:BaseUrl"] ?? "https://localhost:5001";
var resetUrl = $"{baseUrl}/reset-password?token={Uri.EscapeDataString(resetToken)}&email={Uri.EscapeDataString(email)}";
```

## SubAgent活用パターン
### 専門Agent活用基準
- **csharp-infrastructure**: C# Infrastructure層・Repository・設定変更
- **fsharp-domain**: F# Domain層・ビジネスロジック・ドメインモデル
- **spec-compliance**: 仕様準拠確認・品質監査・準拠度評価

### 並列 vs 単一実行判断
- **単一Agent推奨**: 関連性高い・短時間・設定変更等
- **並列実行推奨**: 独立性高い・技術調査・多角的分析

## 次回Phase A9準備事項
### GitHub Issue #21対応技術準備
- **F# Domain層**: 認証ドメインモデル設計準備
- **リファクタリング**: 段階的実装・既存機能保護パターン
- **品質継承**: Phase A8の98/100点品質基盤活用

### 技術負債完全解消状態（継承）
- **TECH-002**: 初期パスワード不整合 → 完全解決
- **TECH-006**: Headers read-only → HTTP文脈分離解決
- **新規技術負債**: ゼロ維持継続