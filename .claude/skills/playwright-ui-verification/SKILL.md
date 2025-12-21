---
name: playwright-ui-verification
description: Playwright MCPでブラウザを操作しUI状態を確認する。「画面確認」「UI確認」「デザイン確認」「バグ調査」「動作検証」「スクリーンショット取得」の際に使用する。
allowed-tools: mcp__playwright__browser_navigate, mcp__playwright__browser_snapshot, mcp__playwright__browser_click, mcp__playwright__browser_type, mcp__playwright__browser_take_screenshot, mcp__playwright__browser_wait_for, mcp__playwright__browser_close, Bash, Read
---

# Playwright UI確認 Skill

Playwright MCPを活用した**開発中のリアルタイムUI確認**。ユーザーとClaudeが視覚的にUI確認・議論を行う。

**確立**: Phase B-F3（Issue #78）

## 前提条件

1. **Webアプリ起動済み** - `devcontainer-web-app` Skillでアプリ起動・https://localhost:5001 応答確認
2. **Playwright MCP利用可能** - `mcp__playwright__*` ツール使用可能状態

## 3つのシーン

| シーン | 頻度 | 用途 |
|--------|------|------|
| A) UI/デザイン変更確認 | 高 | 変更後のUI確認・デザイン議論 |
| B) E2Eテスト作成前検証 | 中 | テスト作成前の動作確認 |
| C) バグ再現・調査 | 低 | 問題の再現・原因調査 |

**詳細**:
- シーンA: [`references/scene-a-ui-verification.md`](./references/scene-a-ui-verification.md)
- シーンB: [`references/scene-b-pre-e2e.md`](./references/scene-b-pre-e2e.md)
- シーンC: [`references/scene-c-bug-investigation.md`](./references/scene-c-bug-investigation.md)

---

## 基本フロー

### 1. 前提確認
```bash
docker exec ubiquitous-lang-mng_devcontainer-devcontainer-1 bash .devcontainer/scripts/web-app.sh status
```

### 2. ブラウザ起動・ナビゲート
```
mcp__playwright__browser_navigate url: https://localhost:5001
```

### 3. ログイン（必要な場合）
```
# E2Eテスト用アカウント: e2e-test@ubiquitous-lang.local / E2ETest#2025!Secure
mcp__playwright__browser_type ref: [emailフィールドref] text: e2e-test@ubiquitous-lang.local
mcp__playwright__browser_click ref: [ログインボタンref]
```

### 4. スナップショット取得・確認
```
mcp__playwright__browser_snapshot  # アクセシビリティスナップショット（推奨）
mcp__playwright__browser_take_screenshot filename: [画面名]_[確認項目]_[YYYYMMDD_HHMMSS].png
```

---

## スクリーンショット命名規則

```
{画面名}_{確認項目}_{YYYYMMDD_HHMMSS}.png
```

**例**: `UserIndex_全ユーザー表示_20251206_143000.png`

## テストアカウント

| 用途 | メールアドレス | パスワード |
|------|---------------|------------|
| E2Eテスト用 | e2e-test@ubiquitous-lang.local | E2ETest#2025!Secure |

## 関連Skills

- [`devcontainer-web-app`](../devcontainer-web-app/SKILL.md): Webアプリ起動管理
- [`playwright-e2e-patterns`](../playwright-e2e-patterns/SKILL.md): E2Eテスト作成パターン

---

**作成日**: 2025-12-06
