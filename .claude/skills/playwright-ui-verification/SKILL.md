---
name: playwright-ui-verification
description: Playwright MCPでブラウザを操作しUI状態を確認する。「画面確認」「UI確認」「デザイン確認」「バグ調査」「動作検証」「スクリーンショット取得」の際に使用する。
allowed-tools: mcp__playwright__browser_navigate, mcp__playwright__browser_snapshot, mcp__playwright__browser_click, mcp__playwright__browser_type, mcp__playwright__browser_take_screenshot, mcp__playwright__browser_wait_for, mcp__playwright__browser_close, Bash, Read
---

# Playwright UI確認 Skill

## 概要

Playwright MCPを活用して、ユーザーとClaudeが視覚的にUI確認・議論を行うためのSkill。
E2Eテスト実装とは異なり、**開発中のリアルタイムUI確認**に特化。

**確立**: Phase B-F3（Issue #78）
**目的**: ユーザーとの視覚的コミュニケーション品質向上

## 前提条件

1. **Webアプリ起動済み**
   - `devcontainer-web-app` Skillでアプリ起動
   - https://localhost:5001 応答確認

2. **Playwright MCP利用可能**
   - `mcp__playwright__*` ツール使用可能状態

## 3つのシーン

このSkillは以下の3シーンに対応：

| シーン | 頻度 | 用途 |
|--------|------|------|
| A) UI/デザイン変更確認 | 高 | 変更後のUI確認・デザイン議論 |
| B) E2Eテスト作成前検証 | 中 | テスト作成前の動作確認 |
| C) バグ再現・調査 | 低 | 問題の再現・原因調査 |

**詳細**:
- シーンA: [`patterns/scene-a-ui-verification.md`](./patterns/scene-a-ui-verification.md)
- シーンB: [`patterns/scene-b-pre-e2e.md`](./patterns/scene-b-pre-e2e.md)
- シーンC: [`patterns/scene-c-bug-investigation.md`](./patterns/scene-c-bug-investigation.md)

## 基本フロー

### 1. 前提確認

```bash
# Webアプリ状態確認
docker exec ubiquitous-lang-mng_devcontainer-devcontainer-1 bash .devcontainer/scripts/web-app.sh status

# 起動していない場合
docker exec ubiquitous-lang-mng_devcontainer-devcontainer-1 bash .devcontainer/scripts/web-app.sh start
```

### 2. ブラウザ起動・ナビゲート

```
mcp__playwright__browser_navigate
  url: https://localhost:5001
```

### 3. ログイン（必要な場合）

```
# ログインページへ移動（自動リダイレクトされる場合あり）

# スナップショット取得
mcp__playwright__browser_snapshot

# 認証情報入力（E2Eテスト用アカウント）
mcp__playwright__browser_type
  ref: [emailフィールドref]
  text: e2e-test@ubiquitous-lang.local

mcp__playwright__browser_type
  ref: [passwordフィールドref]
  text: E2ETest#2025!Secure

mcp__playwright__browser_click
  ref: [ログインボタンref]
```

### 4. 目的画面へ移動

```
# ナビゲーションメニュー操作
mcp__playwright__browser_click
  ref: [メニュー項目ref]

# ページ読み込み待機
mcp__playwright__browser_wait_for
  text: [期待するテキスト]
```

### 5. スナップショット取得・確認

```
# アクセシビリティスナップショット（推奨）
mcp__playwright__browser_snapshot

# 視覚的スクリーンショット（必要に応じて）
mcp__playwright__browser_take_screenshot
  filename: [画面名]_[確認項目]_[YYYYMMDD_HHMMSS].png
```

### 6. ユーザーへの報告

```
スナップショット結果をユーザーに報告:
- 現在の画面状態
- 確認できた項目
- 問題点・改善提案
```

## スクリーンショット命名規則

```
{画面名}_{確認項目}_{YYYYMMDD_HHMMSS}.png
```

**例**:
- `UserIndex_全ユーザー表示_20251206_143000.png`
- `UserCreate_バリデーション_20251206_143500.png`
- `Login_エラーメッセージ_20251206_144000.png`

## テストアカウント情報

| 用途 | メールアドレス | パスワード |
|------|---------------|------------|
| E2Eテスト用 | e2e-test@ubiquitous-lang.local | E2ETest#2025!Secure |

**注意**: 上記アカウントはDBに登録済みのE2Eテスト専用アカウントです。

## 関連Skills

- [`devcontainer-web-app`](../devcontainer-web-app/SKILL.md): Webアプリ起動管理
- [`playwright-e2e-patterns`](../playwright-e2e-patterns/SKILL.md): E2Eテスト作成パターン（ログイン・画面遷移の基本パターン参照）

## トラブルシューティング

### ブラウザが起動しない

**対処**:
1. Playwright MCPが有効か確認
2. `mcp__playwright__browser_navigate` でエラー内容確認

### ログインできない

**対処**:
1. アプリが起動しているか確認（`web-app.sh status`）
2. データベース接続確認
3. テストアカウントの存在確認

### スナップショットが空

**対処**:
1. ページ読み込み待機（`browser_wait_for`）
2. JavaScript実行完了待機
3. `browser_snapshot` 再実行

### 要素が見つからない

**対処**:
1. `browser_snapshot` で現在のDOM構造確認
2. ref値が正しいか確認
3. 動的読み込み要素の場合は待機

---

**作成日**: 2025-12-06
**更新履歴**:
- 2025-12-06: 初版作成（Issue #78）
