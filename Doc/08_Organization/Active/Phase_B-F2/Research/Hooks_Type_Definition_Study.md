# Hooks型定義理解まとめ

**作成日**: 2025-11-18
**調査者**: 技術調査Agent  
**対象**: GitHub Issue #55 Agent SDK Phase 1技術検証

---

## 1. PreToolUse Hook型定義

### 型定義（TypeScript）

```typescript
interface PreToolUseHookInput {
  hook_event_name: "PreToolUse";
  session_id: string;
  transcript_path: string;
  cwd: string;
  tool_name: string;
  tool_input: ToolInput;
}

interface PreToolUseHookOutput {
  decision?: "approve" | "block";
  reason?: string;
  continue?: boolean;
  suppressOutput?: boolean;
}
```

### パラメータ詳細

- **tool_name**: 実行しようとしているツール名（"Task", "Read", "Write", "Bash"等）
- **tool_input**: ツール固有パラメータ（Task: prompt, Read: file_path等）
- **transcript_path**: セッションのトランスクリプトファイルパス（コマンド履歴確認用）
- **cwd**: カレントワーキングディレクトリ

### decision種類（allow/deny/confirm の使い分け）

1. **"approve"** (デフォルト): ツール実行を許可
2. **"block"**: ツール実行を拒否、reasonをユーザー表示
3. **"ask"** (未確認): ユーザー確認プロンプト表示の可能性（Phase 1で検証要）

### matcher機能（Task tool監視の実装方法）

```typescript
{
  matcher: "Task",  // Task toolのみマッチ
  handler: async (input) => { ... }
}

{
  matcher: "Bash|Write|Edit",  // 複数ツール（パイプ区切り）
  handler: async (input) => { ... }
}

{
  matcher: "*",  // 全ツールマッチ
  handler: async (input) => { ... }
}
```

---

## 2. PostToolUse Hook型定義

### 型定義（TypeScript）

```typescript
interface PostToolUseHookInput {
  hook_event_name: "PostToolUse";
  session_id: string;
  transcript_path: string;
  cwd: string;
  tool_name: string;
  tool_input: ToolInput;
  tool_response: ToolOutput;
}

interface PostToolUseHookOutput {
  additionalContext?: string;
  continue?: boolean;
  suppressOutput?: boolean;
}
```

### パラメータ詳細

- **tool_response**: ツール実行結果（Task: SubAgent応答、Read: ファイル内容等）

### 返り値詳細

- **additionalContext**: Claude応答に追加するコンテキスト（成果物確認結果等）

### 検証・フィードバック機能

**用途**:
- SubAgent成果物実体確認（ファイル存在確認）
- 自動フォーマット（Prettier実行）
- ビルド・テスト実行
- ログ記録

---

## 3. Issue #55実現に必要な型定義理解

### ADR_016違反検出（PreToolUseでどう実装するか）

**実装例**:
```typescript
export default defineHooks({
  PreToolUse: [{
    matcher: "Task",
    handler: async (input) => {
      const transcript = await fs.readFile(input.transcript_path, "utf-8");
      if (!transcript.includes("step-start Command実行完了")) {
        return {
          decision: "block",
          reason: "ADR_016違反: step-start Command未実行"
        };
      }
      return { decision: "approve" };
    }
  }]
});
```

### SubAgent成果物実体確認（PostToolUseでどう実装するか）

**実装例**:
```typescript
export default defineHooks({
  PostToolUse: [{
    matcher: "Task",
    handler: async (input) => {
      const responseText = input.tool_response?.content || "";
      const filePaths = extractFilePathsFromResponse(responseText);
      
      const verificationResults = await Promise.all(
        filePaths.map(async (path) => ({
          path,
          exists: await fileExists(path)
        }))
      );
      
      const missingFiles = verificationResults.filter(r => !r.exists);
      if (missingFiles.length > 0) {
        return {
          additionalContext: `ADR_016違反: 以下のファイルが存在しません\n${missingFiles.map(f => f.path).join('\n')}`
        };
      }
      
      return { additionalContext: "成果物実体確認完了" };
    }
  }]
});
```

---

## 4. TypeScript初学者向けの注意事項

### async/await の必須理解

```typescript
// ✅ 正しい
handler: async (input) => {
  const result = await someAsyncOperation();
  return { decision: "approve" };
}

// ❌ 誤り: async がない
handler: (input) => {
  const result = await someAsyncOperation();  // エラー
  return { decision: "approve" };
}
```

### Promise の理解

```typescript
// Promise.all() で並列実行
const results = await Promise.all(
  filePaths.map(async (path) => await fileExists(path))
);
```

### エラーハンドリング（try-catch）

```typescript
handler: async (input) => {
  try {
    const result = await validateStep();
    return { decision: "approve" };
  } catch (error) {
    return {
      decision: "block",
      reason: `内部エラー: ${error.message}`
    };
  }
}
```

---

## 参考情報

- define-claude-code-hooks: https://github.com/timoconnellaus/define-claude-code-hooks
- TypeScript SDK reference: https://docs.claude.com/en/api/agent-sdk/typescript
