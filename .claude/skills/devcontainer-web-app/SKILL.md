---
name: devcontainer-web-app
description: DevContainer環境でWebアプリケーションを起動・停止・再起動する。「アプリ起動」「動作確認」「画面確認」「ログイン確認」「UI確認」の作業時、またはStep完了後の検証時に使用する。
allowed-tools: Bash, Read
---

# DevContainer Webアプリ管理 Skill

DevContainer環境でのWebアプリケーション起動・停止を管理。UI確認・動作確認の前提となるアプリ起動を支援。

**確立**: Phase B-F3（Issue #77）

## 使用タイミング

1. **動作確認フェーズ開始時** - Step完了後のUI確認・ユーザーからの「動作確認」リクエスト
2. **UI変更確認時** - Razor/CSS変更後の見た目確認・レイアウト議論
3. **デバッグ・調査時** - バグ再現・ログ確認・状態調査

---

## スクリプト呼び出し方法

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

**起動成功**: `Web Application is ready!` → https://localhost:5001

---

## ホットリロード vs 再起動の判断

**詳細**: [`references/hot-reload-decision.md`](./references/hot-reload-decision.md)

| 変更内容 | 対応 |
|----------|------|
| Razor (.razor) | ホットリロード（自動反映） |
| CSS / 静的ファイル | ホットリロード（自動反映） |
| C# コード変更 | Rude Edit → 自動再起動 |
| F# コード変更 | `restart` コマンド実行 |
| Program.cs / DI設定 | `restart` コマンド実行 |

---

## 標準フロー

```
1. status で現在の状態確認
2. 停止中なら start で起動
3. 起動中なら、変更内容に応じて restart または継続
4. https://localhost:5001 の応答確認
5. Playwright MCP で画面確認（playwright-ui-verification Skill参照）
```

## 関連Skills/ファイル

- [`playwright-ui-verification`](../playwright-ui-verification/SKILL.md): UI確認フロー
- `.devcontainer/scripts/web-app.sh`: 起動管理スクリプト

---

**作成日**: 2025-12-06
