# Claude Code Hooks

**目的**:
1. Issue #55実現（ADR_016違反検出自動化・SubAgent成果物実体確認自動化）
2. Skills自動発動問題対策（Forced Eval Hook導入）

---

## 📦 セットアップ

### 1. 依存関係インストール

```bash
cd .claude/hooks
npm install
```

### 2. ビルド（トリガー自動生成 + TypeScriptコンパイル）

```bash
npm run build
```

**実行内容**:
1. `generate-triggers` - SKILL.mdからトリガーキーワードを自動抽出
2. `tsc` - TypeScriptコンパイル

**ビルド成果物**:
- `dist/index.js` - Hooks実行ファイル
- `skills-triggers.json` - 自動生成されたトリガー設定

---

## 🎯 実装されたHooks

### 1. PreToolUse Hook

**Task tool監視**: ADR_016違反検出
- step-start Command未実行検出
- SubAgent選択妥当性検証（簡易）

### 2. PostToolUse Hook

**SubAgent成果物実体確認**: Issue #55対応
- SubAgent応答からファイルパス抽出
- ファイル存在確認・サイズ確認
- 検証結果フィードバック

### 3. UserPromptSubmit Hook（Skills Forced Eval）

**Skills自動発動問題対策**: GitHub Issue #81・#9716対応
- メッセージからトリガーキーワード検出
- マッチしたSkillsの評価・活性化を強制指示
- 3ステッププロセス: EVALUATE → ACTIVATE → IMPLEMENT

> ⚠️ **ワークアラウンド**: この機能はClaude Code本体のSkills自動発動問題（Issue #9716）に対する暫定対応です。
> Anthropic公式がIssue #9716を修正した場合、このHookは不要となり削除可能です。

---

## 🔄 Skills Triggers 自動生成（B+C両対応）

### アーキテクチャ

```
.claude/skills/*/SKILL.md
    ↓ (ビルド時に自動抽出)
scripts/generate-triggers.ts
    ↓
skills-triggers.json (自動生成)
    ↓
src/index.ts (JSON読み込み)
```

### 自動検出（C方式）

各SKILL.mdのdescriptionから「」で囲まれたキーワードを自動抽出:

```yaml
# SKILL.md例
description: ADR知見を参照・適用する。「技術決定」「設計判断」「ADR確認」の際に使用する。
```

↓ 自動抽出

```json
{
  "name": "adr-knowledge-base",
  "triggers": ["技術決定", "設計判断", "ADR確認"],
  "source": "auto"
}
```

### 手動オーバーライド（B方式）

自動抽出がうまくいかない場合、`skills-triggers-manual.json`で上書き可能:

```json
{
  "skills": [
    {
      "name": "skill-name",
      "triggers": ["追加キーワード1", "追加キーワード2"]
    }
  ]
}
```

---

## 🆕 新規Skill追加時の手順

1. **SKILL.md作成**: `.claude/skills/{skill-name}/SKILL.md`
   - descriptionに「」でトリガーキーワードを記載

2. **ビルド実行**:
   ```bash
   cd .claude/hooks
   npm run build
   ```

3. **確認**: `skills-triggers.json`に新規Skillが追加されていることを確認

**重要**: SKILL.mdのdescriptionに「」でトリガーキーワードを記載すれば、自動的にHookに反映されます。

---

## 🔧 Claude Code統合設定

`.claude/settings.local.json`に以下の設定が追加済み:

```json
{
  "hooks": {
    "UserPromptSubmit": [
      {
        "hooks": [
          {
            "type": "command",
            "command": "node .claude/hooks/dist/index.js userPromptSubmit"
          }
        ]
      }
    ]
  }
}
```

---

## 🧪 動作確認

### UserPromptSubmit Hookテスト

```bash
cd .claude/hooks
echo '{"user_message": "テスト駆動開発でE2Eテスト実装を開始します", "transcript_path": "dummy.txt"}' | node dist/index.js userPromptSubmit
```

**期待結果**:
- JSONから12個のSkillsを読み込み
- `playwright-e2e-patterns`と`tdd-red-green-refactor`がマッチ
- Skills評価指示が出力される

### トリガー生成テスト

```bash
npm run generate-triggers
```

**期待結果**:
- 全Skillsのdescriptionからトリガーキーワードを抽出
- `skills-triggers.json`を生成/更新

---

## 📊 成果サマリー

**実装完了**:
- ✅ PreToolUse Hook（ADR_016違反検出）
- ✅ PostToolUse Hook（SubAgent成果物実体確認）
- ✅ UserPromptSubmit Hook（Skills Forced Eval）
- ✅ CLIエントリーポイント（標準入出力処理）
- ✅ settings.local.json統合
- ✅ Skills Triggers自動生成（B+C両対応）

---

## 📁 ファイル構成

```
.claude/hooks/
├── dist/                       # ビルド出力
│   └── index.js
├── scripts/
│   └── generate-triggers.ts    # トリガー自動生成スクリプト
├── src/
│   └── index.ts                # Hooks本体
├── skills-triggers.json        # 自動生成されるトリガー設定
├── skills-triggers-manual.json # 手動オーバーライド（オプション）
├── package.json
├── tsconfig.json
└── README.md
```

---

## 📚 参考リソース

- **ADR_016**: `Doc/07_Decisions/ADR_016_プロセス遵守違反防止策.md`
- **Skills自動発動問題**: GitHub Issue #81
- **Claude Code Skills Issue**: https://github.com/anthropics/claude-code/issues/9716
- **Forced Eval Hook参考**: https://scottspence.com/posts/how-to-make-claude-code-skills-activate-reliably

---

**作成日**: 2025-11-18
**最終更新**: 2025-12-08
**Status**: B+C両対応完了・新規Skill追加時の自動反映対応
