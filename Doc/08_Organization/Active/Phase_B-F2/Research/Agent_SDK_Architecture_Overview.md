# Agent SDKアーキテクチャ概要

**作成日**: 2025-11-18
**調査者**: 技術調査Agent  
**対象**: GitHub Issue #55 Agent SDK Phase 1技術検証  
**参照**: Tech_Research_Agent_SDK_2025-10.md（既存調査レポート）

---

## 1. 外部プロセスとしての独立性

### 重要な理解

Agent SDKは**完全に独立した外部プロセス**として動作します。

```
アプリケーション（F# + C# / .NET）
    ↓ 監視対象
Claude Code Process（監視・ツール実行）
    ↓ イベント通知・制御
Agent SDK Process（TypeScript / Python）
    └─ Hooks実行（PreToolUse / PostToolUse）
```

**重要なポイント**:
- ✅ **.NET統合不要**: Agent SDKはTypeScript/Pythonで完結
- ✅ **公式SDK待ち不要**: 現時点で実装可能
- ✅ **アプリケーションコードと無関係**: F#/C#コードへの影響なし
- ✅ **プロセス分離**: Agent SDKクラッシュがアプリに波及しない

### 言語非依存性

Agent SDKは**Claude Codeの動作を監視・制御**するため、開発対象アプリケーションの実装言語に依存しません。

- 本プロジェクト: F# Domain + C# Infrastructure/Web → **関係なし**
- Agent SDK: TypeScript（Node.js 24.x）で実装 → **独立動作**

---

## 2. Hooksの実行タイミング・ライフサイクル

### Hooksライフサイクル全体図

```
1. SessionStart Hook（セッション開始時・初回のみ）
   ↓
2. UserPromptSubmit Hook（ユーザープロンプト送信時）
   ↓
3. Claude モデル応答生成
   ↓
4. PreToolUse Hook（ツール実行前・繰り返し可能）★重要
   ├─ decision: "approve" → 5へ
   ├─ decision: "block" → ツール実行スキップ
   └─ decision: "ask" → ユーザー確認（未確認）
   ↓
5. Tool実行（Read, Write, Bash, Task等）
   ↓
6. PostToolUse Hook（ツール実行後・繰り返し可能）★重要
   ↓
7. Stop Hook（Claude応答完了時）
   ↓
8. PreCompact Hook（AutoCompact実行前・該当時のみ）
   ↓
9. SessionEnd Hook（セッション終了時）
```

### Issue #55実現における重要Hooks

#### PreToolUse Hook（実行前の制御）

**用途**:
- ✅ **ADR_016違反検出**: step-start実行確認、SubAgent選択妥当性検証
- ✅ **危険操作防止**: 本番デプロイ防止、機密ファイル編集防止
- ✅ **並列実行制御**: 同一ファイルへの競合検出

**実行タイミング**: Claude Codeがツールを実行する**直前**

**返り値による制御**:
- `decision: "approve"` → ツール実行許可
- `decision: "block"` → ツール実行拒否（reason表示）

#### PostToolUse Hook（実行後の処理）

**用途**:
- ✅ **SubAgent成果物実体確認**: Task tool完了後のファイル存在確認
- ✅ **自動フォーマット**: TypeScript編集後のPrettier実行
- ✅ **検証・テスト**: ビルド成功確認、テスト実行

**実行タイミング**: Claude Codeがツールを実行した**直後**

**返り値による追加情報**:
- `additionalContext` → Claude応答に追加するコンテキスト

---

## 3. Claude Code本体とHooksの通信方式

### 通信フロー（PreToolUse例）

```
Claude: "Read file example.txt を実行したい"
  ↓
[PreToolUse Hook 発火]
  ↓
Claude Code → Agent SDK Process
  │ JSON送信（IPC経由）
  │ {
  │   "hook_event_name": "PreToolUse",
  │   "tool_name": "Read",
  │   "tool_input": { "file_path": "example.txt" },
  │   "session_id": "abc123"
  │ }
  ↓
Agent SDK Process（hooks.ts実行）
  │ handler(input) 実行
  ↓
Agent SDK Process → Claude Code
  │ JSON返却（IPC経由）
  │ {
  │   "decision": "approve" | "block",
  │   "reason": "..."
  │ }
  ↓
Claude Code: ツール実行判断
  ├─ "approve" → Read file実行
  └─ "block" → 実行スキップ、reasonをユーザーに表示
```

### 通信プロトコル（推定）

- **データ形式**: JSON
- **転送方式**: stdin/stdout（標準入出力）またはUnix Domain Socket
- **非同期処理**: Hooks内でasync/await使用可能

---

## 4. TypeScript実装における重要ポイント

### 型安全性の活用

**推奨パッケージ**:
```bash
npm install --save-dev define-claude-code-hooks
```

**型定義の利点**:
- IntelliSense完全サポート（VSCode）
- コンパイル時エラー検出
- リファクタリング安全性

### Matcher機能の活用

**Matcher構文**:
- `"Task"` → Task toolのみ
- `"Bash|Write|Edit"` → 複数ツール（パイプ区切り）
- `"*"` または `".*"` → 全ツール

---

## 参考情報

### 公式ドキュメント
- Claude Code Hooks Guide: https://code.claude.com/docs/en/hooks-guide
- Agent SDK TypeScript Reference: https://docs.claude.com/en/api/agent-sdk/typescript

### TypeScriptパッケージ
- define-claude-code-hooks: https://github.com/timoconnellaus/define-claude-code-hooks

### コミュニティ実装例
- johnlindquist/claude-hooks: https://github.com/johnlindquist/claude-hooks
- bartolli/claude-code-typescript-hooks: https://github.com/bartolli/claude-code-typescript-hooks
