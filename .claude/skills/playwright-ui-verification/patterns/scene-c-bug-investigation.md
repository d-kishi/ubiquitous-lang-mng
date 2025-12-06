# シーンC: バグ再現・調査

## 概要

ユーザー報告またはログから問題を特定し、Playwrightで再現・調査するフロー。

**頻度**: 低
**トリガー**: ユーザーからのバグ報告、ログでのエラー発見

## 情報収集パターン

### パターン1: ユーザー報告ベース

ユーザーからの報告を受けて調査：

```
【ユーザー報告例】
「ユーザー編集画面で保存ボタンを押しても反応がない」

【収集する情報】
1. どの画面で発生？ → /Admin/Users/Edit/{id}
2. どのような操作？ → フォーム編集後、保存ボタンクリック
3. 期待動作は？ → 保存されて一覧画面に戻る
4. 実際の動作は？ → 何も起きない
5. 発生条件は？ → 特定のユーザーのみ？全員？
```

### パターン2: ログ探索ベース

アプリケーションログから問題を特定：

```bash
# DevContainer内でログ確認
docker exec ubiquitous-lang-mng_devcontainer-devcontainer-1 cat /tmp/web-app.log | grep -i error

# または tail でリアルタイム監視
docker exec ubiquitous-lang-mng_devcontainer-devcontainer-1 tail -f /tmp/web-app.log
```

**ログから抽出する情報**:
- エラーメッセージ
- スタックトレース
- 発生時刻
- 関連するリクエスト情報

## 再現フロー

### Step 1: 再現条件の整理

```
【再現条件チェックリスト】
□ 対象画面/URL
□ ログインユーザー（ロール）
□ 操作手順（ステップバイステップ）
□ 入力データ
□ 発生タイミング（常時？特定条件？）
```

### Step 2: Playwrightで再現

```
1. mcp__playwright__browser_navigate
   url: https://localhost:5001

2. 問題発生条件でログイン

3. 問題画面へ移動

4. 操作を一つずつ実行
   - 各ステップでbrowser_snapshot
   - コンソールエラー確認（必要に応じて）

5. 問題発生時点でスナップショット
```

### Step 3: コンソール・ネットワーク確認

```
# コンソールメッセージ確認
mcp__playwright__browser_console_messages
  onlyErrors: true

# ネットワークリクエスト確認
mcp__playwright__browser_network_requests
```

### Step 4: 原因特定・報告

確認結果から：
1. 問題の再現可否
2. 再現した場合の状況
3. 推定される原因
4. 追加調査が必要な箇所

## 具体例

### 例: 保存ボタンが反応しない問題

```
【ユーザー報告】
「ユーザー編集画面で保存ボタンが効かない」

【Step 1: 条件整理】
- 画面: /Admin/Users/Edit/1
- ユーザー: SuperUser
- 操作: 名前変更 → 保存ボタンクリック
- 期待: 保存されて一覧に戻る
- 実際: 反応なし

【Step 2: 再現操作】
1. SuperUserでログイン
2. /Admin/Users/Edit/1 へ移動
3. browser_snapshot → フォーム確認
4. 名前を「テスト変更」に編集
5. 保存ボタンクリック
6. browser_snapshot → 状態確認

【Step 3: コンソール確認】
browser_console_messages
  onlyErrors: true

結果:
- JavaScript Error: "Uncaught TypeError: Cannot read property 'submit' of null"

【Step 4: 報告】
「問題を再現しました。
 JavaScript エラーが発生しています：
 'Cannot read property submit of null'

 推定原因：
 - フォーム要素のIDまたはref指定の問題
 - 動的レンダリングのタイミング問題

 調査推奨箇所：
 - Edit.razor の @onclick ハンドラ
 - フォーム要素のID/ref定義」
```

## ログ探索コマンド

### Webアプリログ

```bash
# 全ログ確認
docker exec ubiquitous-lang-mng_devcontainer-devcontainer-1 cat /tmp/web-app.log

# エラーのみ
docker exec ubiquitous-lang-mng_devcontainer-devcontainer-1 cat /tmp/web-app.log | grep -i error

# 直近50行
docker exec ubiquitous-lang-mng_devcontainer-devcontainer-1 tail -50 /tmp/web-app.log

# リアルタイム監視
docker exec ubiquitous-lang-mng_devcontainer-devcontainer-1 tail -f /tmp/web-app.log
```

### データベースログ

```bash
# PostgreSQLログ確認
docker logs postgres 2>&1 | tail -50
```

## 調査結果の記録

問題調査後は以下を記録：

1. **GitHub Issue作成**（未解決の場合）
   - 再現手順
   - 期待動作
   - 実際の動作
   - 調査結果
   - 推定原因

2. **修正実施**（解決可能な場合）
   - 原因特定
   - 修正実施
   - 再テスト確認

---

**作成日**: 2025-12-06
