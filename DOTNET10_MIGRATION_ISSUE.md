# .NET 10への移行

## 📋 概要

本プロジェクトは現在.NET 8で構築されていますが、.NET 10がリリースされたため、最新バージョンへの移行を検討します。

**現在の状況**:
- 主要プロジェクト（src/配下）: .NET 8.0
- 一部テストプロジェクト（tests/配下）: .NET 9.0と.NET 8.0が混在
- 利用可能なSDK: .NET 10.0.100

## 🎯 移行の目的

- 最新の.NETランタイムによるパフォーマンス向上
- セキュリティアップデートの適用
- 最新のC# 13/F# 9機能の活用
- 長期サポート（LTS）への対応（該当する場合）

## 📊 影響範囲

### 主要プロジェクト（.NET 8.0 → .NET 10.0）

**src/配下**:
- `UbiquitousLanguageManager.Domain` (F#)
- `UbiquitousLanguageManager.Application` (F#)
- `UbiquitousLanguageManager.Contracts` (C#)
- `UbiquitousLanguageManager.Infrastructure` (C#)
- `UbiquitousLanguageManager.Web` (C#)

**一部テスト**:
- `UbiquitousLanguageManager.Web.UI.Tests`
- `UbiquitousLanguageManager.Domain.Unit.Tests` (F#)
- `integration-test-standalone/InitialPasswordIntegrationTests`

### テストプロジェクト（.NET 9.0 → .NET 10.0）

**tests/配下**:
- `UbiquitousLanguageManager.Infrastructure.Unit.Tests`
- `UbiquitousLanguageManager.Infrastructure.Integration.Tests`
- `UbiquitousLanguageManager.E2E.Tests`
- `UbiquitousLanguageManager.Contracts.Unit.Tests`
- `UbiquitousLanguageManager.Application.Unit.Tests` (F#)

## 📝 移行作業内容

### Phase 1: 調査・準備
- [ ] .NET 10の破壊的変更（Breaking Changes）調査
- [ ] 使用中のNuGetパッケージの.NET 10互換性確認
- [ ] Entity Framework Core、ASP.NET Core Identityの互換性確認
- [ ] F#コンパイラの.NET 10対応状況確認
- [ ] 移行リスク評価

### Phase 2: テスト環境での移行
- [ ] 開発環境のDockerイメージ更新（.NET 10 SDK）
- [ ] global.json作成/更新（SDKバージョン固定）
- [ ] テストプロジェクトのTargetFramework更新（net9.0 → net10.0）
- [ ] 全テスト実行・互換性確認

### Phase 3: 本体プロジェクトの移行
- [ ] src/配下プロジェクトのTargetFramework更新（net8.0 → net10.0）
- [ ] NuGetパッケージバージョン更新
  - Microsoft.AspNetCore.Identity.EntityFrameworkCore
  - Microsoft.EntityFrameworkCore.*
  - その他依存パッケージ
- [ ] ビルド確認（0 Warning, 0 Error維持）
- [ ] 全テスト実行（既存テスト100%成功維持）

### Phase 4: CI/CD・デプロイ環境更新
- [ ] GitHub Actions workflows更新（.NET 10 SDK使用）
- [ ] DevContainer設定更新
- [ ] 本番環境Dockerイメージ更新

### Phase 5: ドキュメント更新
- [ ] README.md更新
- [ ] CLAUDE.md更新（開発コマンドセクション）
- [ ] 環境構築ドキュメント更新
- [ ] ADR作成（.NET 10移行決定記録）

## ⚠️ リスク・考慮事項

### 技術的リスク
- **破壊的変更**: .NET 8 → .NET 10で非互換の可能性
- **NuGetパッケージ**: 一部パッケージが.NET 10未対応の可能性
- **F#互換性**: F#コンパイラの.NET 10対応状況
- **Blazor Server**: SignalR等の動作変更の可能性

### 運用リスク
- **開発環境**: チーム全員のSDK更新が必要
- **ビルド時間**: 初回ビルド時のパッケージダウンロード
- **デバッグ**: 既存のトラブルシューティング知見が通用しない可能性

### 軽減策
- ✅ Phase 1で詳細調査を実施（Breaking Changes確認）
- ✅ Phase 2でテストプロジェクトから段階的移行
- ✅ 各Phaseで全テスト実行・品質維持確認
- ✅ 問題発生時は.NET 8へロールバック可能な体制

## 📅 対応時期

**要件等**（実施タイミングは別途検討）

以下の要因を考慮して実施タイミングを決定：
- 現在進行中のPhaseとの優先度
- .NET 10の安定性（RTM後の安定期待ち）
- チームのリソース状況
- 他の技術負債との優先順位

**推奨タイミング候補**:
- Phase B完了後（Phase C開始前）
- Phase C-D間の準備期間
- 大きな機能開発のない安定期

## 🔗 関連情報

### 公式ドキュメント
- [.NET 10リリースノート](https://learn.microsoft.com/ja-jp/dotnet/core/whats-new/dotnet-10/overview)
- [.NET 10 Breaking Changes](https://learn.microsoft.com/ja-jp/dotnet/core/compatibility/10.0)
- [ASP.NET Core 10.0の新機能](https://learn.microsoft.com/ja-jp/aspnet/core/release-notes/aspnetcore-10.0)

### 関連Issue
- Issue #62: テストコードのNullable参照型警告（.NET 9移行時発生）
- Issue #63: DevContainer環境対応

### 参考ADR
- ADR_025: DevContainer + Sandboxモード統合採用
- 今後作成: ADR_0XX（.NET 10移行決定記録）

## ✅ 完了条件

- [ ] 全プロジェクトが.NET 10.0をターゲット
- [ ] ビルド成功（0 Warning, 0 Error）
- [ ] 全テスト成功（既存テスト100%維持）
- [ ] 開発環境・CI/CD環境が.NET 10で動作
- [ ] ドキュメント更新完了
- [ ] ADR作成（移行決定記録）

---

**作成日**: 2025-11-14  
**優先度**: Medium（要件等）  
**カテゴリ**: 技術基盤・インフラ更新  
**推奨ラベル**: `enhancement`
