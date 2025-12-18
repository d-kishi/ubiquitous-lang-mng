---
paths:
  - src/**
  - tests/**
  - .devcontainer/**
---

# 開発コマンド（DevContainer環境）

## 🔴 CRITICAL: DevContainer環境必須

本プロジェクトはDevContainer環境で開発します。以下のコマンドは全てDevContainer内で実行してください。

### Claude Code使用時の必須ルール

- Claude CodeはWindowsホスト環境で実行されるため、`dotnet`コマンドを直接実行すると**ホスト環境で実行されてしまう**
- **必ず方法Bの`docker exec`形式でDevContainer内実行を明示すること**
- ホスト環境で実行すると、ビルド成果物が混在しDevContainer環境でのビルド/実行が失敗する
- **違反時の対処**: ホスト環境のbin/objディレクトリをすべて削除し、DevContainer内で再ビルドが必要

---

## コマンド実行方法

### 方法A: VS Code統合ターミナル（推奨）

VS CodeでDevContainerを開いた状態で、統合ターミナル（Ctrl+`）から直接実行：

```bash
# ビルド
dotnet build
dotnet build src/UbiquitousLanguageManager.Web

# 実行
dotnet run --project src/UbiquitousLanguageManager.Web

# テスト
dotnet test
dotnet test --filter "FullyQualifiedName~UserTests"

# データベース
dotnet ef migrations add MigrationName --project src/UbiquitousLanguageManager.Infrastructure
dotnet ef database update --project src/UbiquitousLanguageManager.Infrastructure
```

### 方法B: ホスト環境から明示的実行（Claude Code用）

**暫定対応**: Windows環境ではClaude Code Sandboxモードが非対応のため、以下の形式でDevContainer内実行を明示：

```bash
# ビルド
docker exec ubiquitous-lang-mng_devcontainer-devcontainer-1 dotnet build
docker exec ubiquitous-lang-mng_devcontainer-devcontainer-1 dotnet build src/UbiquitousLanguageManager.Web

# 実行
docker exec ubiquitous-lang-mng_devcontainer-devcontainer-1 dotnet run --project src/UbiquitousLanguageManager.Web

# テスト
docker exec ubiquitous-lang-mng_devcontainer-devcontainer-1 dotnet test
docker exec ubiquitous-lang-mng_devcontainer-devcontainer-1 dotnet test --filter "FullyQualifiedName~UserTests"

# データベース
docker exec ubiquitous-lang-mng_devcontainer-devcontainer-1 dotnet ef migrations add MigrationName --project src/UbiquitousLanguageManager.Infrastructure
docker exec ubiquitous-lang-mng_devcontainer-devcontainer-1 dotnet ef database update --project src/UbiquitousLanguageManager.Infrastructure

# E2Eテスト（TypeScript/Playwright Test）
docker exec ubiquitous-lang-mng_devcontainer-devcontainer-1 bash tests/run-e2e-tests.sh
docker exec ubiquitous-lang-mng_devcontainer-devcontainer-1 bash tests/run-e2e-tests.sh authentication.spec.ts
```

---

## E2Eテスト自動実行（TypeScript/Playwright Test）

### 一括実行スクリプト（推奨）

`tests/run-e2e-tests.sh`は、E2Eテスト実行を自動化するスクリプトです：
- Webアプリケーションをバックグラウンド起動
- ポート5001の応答待機（最大60秒）
- E2Eテスト実行（npx playwright test）
- プロセスクリーンアップ

**実行時間**: 約30秒（手動実行3-5分 → 83-93%削減）

### VS Code統合ターミナル

```bash
# 全E2Eテスト実行
bash tests/run-e2e-tests.sh

# 特定テストファイルのみ実行
bash tests/run-e2e-tests.sh authentication.spec.ts
bash tests/run-e2e-tests.sh user-projects.spec.ts
```

### ホスト環境から（Claude Code用）

```bash
# 全E2Eテスト実行
docker exec ubiquitous-lang-mng_devcontainer-devcontainer-1 bash tests/run-e2e-tests.sh

# 特定テストファイルのみ実行
docker exec ubiquitous-lang-mng_devcontainer-devcontainer-1 bash tests/run-e2e-tests.sh authentication.spec.ts
```

**終了コード**:
- `0`: テスト成功
- `1`: テスト失敗またはエラー

---

## Docker環境管理

```bash
# PostgreSQL/PgAdmin/Smtp4dev起動（ホスト環境で実行）
docker-compose up -d

# 停止
docker-compose down

# DevContainer確認
docker ps --filter "name=devcontainer"
```

---

## 開発ツールURL

- **アプリ**: https://localhost:5001
- **PgAdmin**: http://localhost:8080 (admin@ubiquitous-lang.com / admin123)
- **Smtp4dev**: http://localhost:5080

---

## 暫定対応について

**注意**: 現在、Windows環境ではClaude Code Sandboxモードが非対応のため、方法Bを使用しています。

将来Sandboxモードが対応された際は、`docker exec`プレフィックスを省略して直接実行可能になります。

**関連情報**:
- GitHub Issue #63「Windows環境でのClaude Code Sandboxモード非対応に伴うDevContainer手動実行対応」
- ADR_025「DevContainer + Sandboxモード統合採用」

---

**作成日**: 2025-12-17
**Phase**: Issue #83 ルール管理基盤改善
**抽出元**: CLAUDE.md 開発コマンド（DevContainer環境）セクション
