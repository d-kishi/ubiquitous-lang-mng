---
name: devcontainer-web-app
description: DevContainer環境でWebアプリケーションを起動・停止・再起動する。「アプリ起動」「動作確認」「画面確認」「ログイン確認」「UI確認」の作業時、またはStep完了後の検証時に使用する。
allowed-tools: Bash, Read
---

# DevContainer Webアプリ管理 Skill

## 概要

DevContainer環境でのWebアプリケーション起動・停止を管理するSkill。
ユーザーとの視覚的コミュニケーション（UI確認・動作確認）の前提となるアプリ起動を支援する。

**確立**: Phase B-F3（Issue #77）
**目的**: 開発中のUI確認効率化・ホットリロード有効化

## 使用タイミング

以下の場面でこのSkillを適用する：

1. **動作確認フェーズ開始時**
   - Step完了後のUI確認
   - ユーザーからの「動作確認」「画面確認」リクエスト

2. **UI変更確認時**
   - Razor/CSS変更後の見た目確認
   - レイアウト・デザイン議論

3. **デバッグ・調査時**
   - バグ再現のためのアプリ起動
   - ログ確認・状態調査

## スクリプト呼び出し方法

### 基本コマンド

```bash
# 起動（ホットリロード有効）
docker exec ubiquitous-lang-mng_devcontainer-devcontainer-1 bash .devcontainer/scripts/web-app.sh start

# 停止
docker exec ubiquitous-lang-mng_devcontainer-devcontainer-1 bash .devcontainer/scripts/web-app.sh stop

# 再起動
docker exec ubiquitous-lang-mng_devcontainer-devcontainer-1 bash .devcontainer/scripts/web-app.sh restart

# 状態確認
docker exec ubiquitous-lang-mng_devcontainer-devcontainer-1 bash .devcontainer/scripts/web-app.sh status
```

### 実行結果の確認

- **起動成功**: `Web Application is ready!` メッセージ表示
- **URL**: https://localhost:5001
- **ログ確認**: `tail -f /tmp/web-app.log`（DevContainer内）

## ホットリロード vs 再起動の判断

**詳細**: [`patterns/hot-reload-decision.md`](./patterns/hot-reload-decision.md)

### 簡易判断フロー

| 変更内容 | 対応 |
|----------|------|
| Razor (.razor) | ホットリロード（自動反映） |
| CSS / 静的ファイル | ホットリロード（自動反映） |
| C# コード変更 | Rude Edit → 自動再起動 |
| F# コード変更 | `restart` コマンド実行 |
| Program.cs / DI設定 | `restart` コマンド実行 |
| NuGetパッケージ追加 | `restart` コマンド実行 |

## 標準フロー

### 1. 動作確認開始時

```
1. status で現在の状態確認
2. 停止中なら start で起動
3. 起動中なら、変更内容に応じて restart または そのまま継続
4. https://localhost:5001 の応答確認
5. Playwright MCP で画面確認（playwright-ui-verification Skill参照）
```

### 2. 動作確認終了時

```
1. 必要に応じて stop で停止
2. または、次の作業のために起動したまま維持
```

## トラブルシューティング

### 起動タイムアウト

**症状**: `Timeout waiting for port 5001` エラー

**対処**:
1. ログ確認: `docker exec ... cat /tmp/web-app.log`
2. ポート競合確認: `docker exec ... lsof -i :5001`
3. 強制停止後再起動: `stop` → `start`

### プロセス残存

**症状**: 停止したはずなのにポート5001が使用中

**対処**:
1. `status` で状態確認
2. `stop` で停止（SIGKILL含む）
3. 手動確認: `docker exec ... pkill -f "dotnet.*UbiquitousLanguageManager"`

### ホットリロードが効かない

**症状**: Razor変更が反映されない

**対処**:
1. `DOTNET_USE_POLLING_FILE_WATCHER=1` 環境変数確認
2. DevContainer再ビルド（設定反映）
3. `restart` で再起動

## 関連Skills

- [`playwright-ui-verification`](../playwright-ui-verification/SKILL.md): UI確認フロー

## 関連ファイル

- `.devcontainer/scripts/web-app.sh`: 起動管理スクリプト
- `.devcontainer/devcontainer.json`: 環境変数設定（DOTNET_USE_POLLING_FILE_WATCHER）

---

**作成日**: 2025-12-06
**更新履歴**:
- 2025-12-06: 初版作成（Issue #77）
