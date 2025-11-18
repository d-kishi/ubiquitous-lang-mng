/**
 * Claude Code Hooks - Agent SDK Phase 1 (Issue #55)
 *
 * PreToolUse Hook: ADR_016違反検出自動化
 * - Task tool監視（matcher: "Task"）
 * - step-start Command未実行検出
 * - SubAgent選択妥当性検証
 *
 * @author Ubiquitous Language Manager Project
 * @date 2025-11-18
 */

import * as fs from 'fs/promises';

// ============================================================================
// 型定義（define-claude-code-hooks パッケージより）
// ============================================================================

/**
 * PreToolUse Hook入力型
 *
 * Task tool実行前に呼び出され、実行を許可・拒否・確認できる
 */
interface PreToolUseHookInput {
  /**
   * 実行されるツール名（例: "Task", "Bash", "Write"）
   */
  tool_name: string;

  /**
   * ツールへの入力パラメータ（JSON形式）
   */
  tool_input: any;

  /**
   * 会話トランスクリプトファイルのパス
   * step-start Command実行確認に使用
   */
  transcript_path: string;

  /**
   * ユーザーメッセージ
   */
  user_message?: string;
}

/**
 * PreToolUse Hook出力型
 *
 * decision: "approve" - 実行を許可
 * decision: "block" - 実行を拒否
 */
interface PreToolUseHookOutput {
  /**
   * 実行許可・拒否の判断
   */
  decision: "approve" | "block";

  /**
   * Claudeへの追加コンテキスト（エラーメッセージ・ガイダンス）
   */
  additionalContext?: string;
}

/**
 * PostToolUse Hook入力型
 *
 * Task tool実行後に呼び出され、SubAgent成果物実体確認を実施
 */
interface PostToolUseHookInput {
  /**
   * 実行されたツール名（例: "Task"）
   */
  tool_name: string;

  /**
   * ツールへの入力パラメータ（JSON形式）
   */
  tool_input: any;

  /**
   * ツールからの応答（SubAgent応答）
   */
  tool_response: string;

  /**
   * 会話トランスクリプトファイルのパス
   */
  transcript_path: string;
}

/**
 * PostToolUse Hook出力型
 *
 * SubAgent成果物実体確認結果をClaude応答に追加
 */
interface PostToolUseHookOutput {
  /**
   * Claudeへの追加コンテキスト（成果物確認結果）
   */
  additionalContext?: string;
}

// ============================================================================
// ユーティリティ関数
// ============================================================================

/**
 * トランスクリプトファイルからstep-start Command実行確認
 *
 * ADR_016: SubAgent起動前にstep-start Command実行が必須
 *
 * @param transcriptPath - トランスクリプトファイルのパス
 * @returns step-start Command実行済みならtrue
 */
async function checkStepStartExecuted(transcriptPath: string): Promise<boolean> {
  try {
    // トランスクリプトファイル読み込み
    const transcriptContent = await fs.readFile(transcriptPath, 'utf-8');

    // step-start Command実行パターン検出
    // SlashCommandツール呼び出しで "/step-start" が含まれるかチェック
    const stepStartPatterns = [
      /SlashCommand.*\/step-start/i,
      /command.*step-start/i,
      /<command-name>\/step-start<\/command-name>/i
    ];

    for (const pattern of stepStartPatterns) {
      if (pattern.test(transcriptContent)) {
        return true;
      }
    }

    return false;
  } catch (error) {
    // ファイル読み込みエラー時はログ出力して false 返却
    console.error(`[PreToolUse] トランスクリプト読み込みエラー: ${error}`);
    return false;
  }
}

/**
 * SubAgent選択妥当性検証
 *
 * ADR_016: SubAgent選択は subagent-selection Command実行が推奨
 * （Phase 1では簡易チェックのみ実施）
 *
 * @param subagentType - SubAgentタイプ（例: "tech-research"）
 * @returns 妥当性チェック結果メッセージ（問題なければundefined）
 */
function validateSubAgentSelection(subagentType: string): string | undefined {
  // Phase 1では簡易チェックのみ: 未知のSubAgentタイプを警告
  const knownSubAgents = [
    "tech-research",
    "code-review",
    "csharp-web-ui",
    "csharp-infrastructure",
    "contracts-bridge",
    "fsharp-application",
    "fsharp-domain",
    "e2e-test",
    "design-review",
    "dependency-analysis",
    "integration-test",
    "unit-test",
    "spec-analysis",
    "spec-compliance",
    "playwright-test-generator",
    "playwright-test-planner",
    "playwright-test-healer"
  ];

  if (!knownSubAgents.includes(subagentType)) {
    return `⚠️ 未知のSubAgentタイプ: ${subagentType}\n` +
           `既知のSubAgents: ${knownSubAgents.join(", ")}`;
  }

  return undefined;
}

// ============================================================================
// PreToolUse Hook実装
// ============================================================================

/**
 * PreToolUse Hook: Task tool監視
 *
 * ADR_016違反検出:
 * 1. step-start Command未実行検出
 * 2. SubAgent選択妥当性検証（簡易）
 *
 * @param input - PreToolUse Hook入力
 * @returns PreToolUse Hook出力（decision: "approve" | "block"）
 */
async function preToolUseHook(input: PreToolUseHookInput): Promise<PreToolUseHookOutput> {
  try {
    // Task toolのみ監視（matcher: "Task"）
    if (input.tool_name !== "Task") {
      return { decision: "approve" };
    }

    console.log(`[PreToolUse] Task tool実行前チェック開始`);
    console.log(`[PreToolUse] SubAgent: ${input.tool_input.subagent_type}`);

    // 1. step-start Command実行確認
    const stepStartExecuted = await checkStepStartExecuted(input.transcript_path);

    if (!stepStartExecuted) {
      // ADR_016違反: step-start Command未実行
      const errorMessage =
        `❌ ADR_016違反検出: step-start Command未実行\n\n` +
        `SubAgent起動前に step-start Command の実行が必須です。\n\n` +
        `**修正手順**:\n` +
        `1. /step-start Command を実行してください\n` +
        `2. Step目的・成果物を明確化してください\n` +
        `3. SubAgent選択を実施してください\n\n` +
        `**参照**: ADR_016プロセス遵守絶対原則（Doc/07_Decisions/ADR_016_プロセス遵守違反防止策.md）`;

      console.error(`[PreToolUse] ADR_016違反検出: step-start Command未実行`);

      return {
        decision: "block",
        additionalContext: errorMessage
      };
    }

    // 2. SubAgent選択妥当性検証（簡易）
    const validationError = validateSubAgentSelection(input.tool_input.subagent_type);

    if (validationError) {
      console.warn(`[PreToolUse] SubAgent選択警告: ${validationError}`);

      return {
        decision: "approve", // Phase 1では警告のみ（block しない）
        additionalContext: validationError
      };
    }

    // 3. すべてのチェックをパス
    console.log(`[PreToolUse] チェック完了: すべてのチェックをパス`);

    return { decision: "approve" };

  } catch (error) {
    // エラーハンドリング: エラー発生時は approve（実行を妨げない）
    console.error(`[PreToolUse] エラー発生: ${error}`);

    return {
      decision: "approve",
      additionalContext: `⚠️ PreToolUse Hookでエラーが発生しましたが、実行を継続します。\nエラー: ${error}`
    };
  }
}

// ============================================================================
// PostToolUse Hook実装
// ============================================================================

/**
 * SubAgent応答からファイルパス抽出
 *
 * 正規表現パターンでファイルパスを抽出
 * （TypeScript_Learning_Notes.md Section 5参照）
 *
 * @param toolResponse - SubAgent応答
 * @returns 抽出されたファイルパスの配列
 */
function extractFilePaths(toolResponse: string): string[] {
  const filePaths: string[] = [];

  // ファイルパス抽出パターン（3種類）
  const patterns = [
    /(?:作成|更新|生成)(?:しました|完了)?[:\s]+([^\s]+)/g,
    /(?:ファイル|成果物)[:\s]+`([^`]+)`/g,
    /(?:出力先|保存先)[:\s]+([^\s]+)/g,
    // Markdownファイルパスパターン（`.md`等）
    /([a-zA-Z0-9_\-/.\\]+\.(?:md|ts|js|json|txt))/g
  ];

  for (const pattern of patterns) {
    const matches = toolResponse.matchAll(pattern);
    for (const match of matches) {
      if (match[1]) {
        // 重複排除
        if (!filePaths.includes(match[1])) {
          filePaths.push(match[1]);
        }
      }
    }
  }

  return filePaths;
}

/**
 * ファイル存在確認
 *
 * fs.access()によるファイル存在確認
 *
 * @param filePath - 確認対象ファイルパス
 * @returns ファイル存在すればtrue
 */
async function checkFileExists(filePath: string): Promise<boolean> {
  try {
    await fs.access(filePath);
    return true;
  } catch (error) {
    // ENOENTエラー: ファイル不存在
    return false;
  }
}

/**
 * ファイルサイズ取得
 *
 * fs.stat()によるファイルサイズ取得
 *
 * @param filePath - 確認対象ファイルパス
 * @returns ファイルサイズ（バイト）、エラー時は-1
 */
async function getFileSize(filePath: string): Promise<number> {
  try {
    const stats = await fs.stat(filePath);
    return stats.size;
  } catch (error) {
    return -1;
  }
}

/**
 * PostToolUse Hook: SubAgent成果物実体確認
 *
 * Issue #55実現:
 * 1. SubAgent応答からファイルパス抽出
 * 2. ファイル存在確認（fs.access()）
 * 3. ファイルサイズ確認（fs.stat()）
 * 4. 検証結果フィードバック
 *
 * @param input - PostToolUse Hook入力
 * @returns PostToolUse Hook出力（additionalContext）
 */
async function postToolUseHook(input: PostToolUseHookInput): Promise<PostToolUseHookOutput> {
  try {
    // Task toolのみ監視（matcher: "Task"）
    if (input.tool_name !== "Task") {
      return {};
    }

    console.log(`[PostToolUse] Task tool実行後チェック開始`);
    console.log(`[PostToolUse] SubAgent: ${input.tool_input.subagent_type}`);

    // 1. SubAgent応答からファイルパス抽出
    const filePaths = extractFilePaths(input.tool_response);

    if (filePaths.length === 0) {
      console.log(`[PostToolUse] ファイルパス未検出（成果物なしまたはパターン不一致）`);
      return {};
    }

    console.log(`[PostToolUse] 抽出ファイルパス: ${filePaths.join(", ")}`);

    // 2-3. ファイル存在確認・サイズ確認（並列実行）
    const fileChecks = await Promise.all(
      filePaths.map(async (filePath) => {
        const exists = await checkFileExists(filePath);
        const size = exists ? await getFileSize(filePath) : -1;
        return { filePath, exists, size };
      })
    );

    // 4. 検証結果フィードバック
    const existingFiles = fileChecks.filter((check) => check.exists);
    const missingFiles = fileChecks.filter((check) => !check.exists);

    let feedbackMessage = `\n\n📊 SubAgent成果物実体確認結果:\n`;

    if (existingFiles.length > 0) {
      feedbackMessage += `\n✅ **存在確認完了** (${existingFiles.length}ファイル):\n`;
      existingFiles.forEach((check) => {
        const sizeKB = (check.size / 1024).toFixed(2);
        feedbackMessage += `  - ${check.filePath} (${sizeKB} KB)\n`;
      });
    }

    if (missingFiles.length > 0) {
      feedbackMessage += `\n❌ **ファイル不存在** (${missingFiles.length}ファイル):\n`;
      missingFiles.forEach((check) => {
        feedbackMessage += `  - ${check.filePath}\n`;
      });
      feedbackMessage += `\n⚠️ ADR_016違反の可能性: SubAgentが成果物作成を報告したが、ファイルが存在しません。\n`;
    }

    console.log(`[PostToolUse] 成果物確認完了: ${existingFiles.length}/${fileChecks.length}ファイル存在`);

    return {
      additionalContext: feedbackMessage
    };

  } catch (error) {
    // エラーハンドリング: エラー発生時もフィードバック継続
    console.error(`[PostToolUse] エラー発生: ${error}`);

    return {
      additionalContext: `\n\n⚠️ PostToolUse Hookでエラーが発生しましたが、処理を継続します。\nエラー: ${error}`
    };
  }
}

// ============================================================================
// Hooks登録（define-claude-code-hooks パッケージ使用）
// ============================================================================

/**
 * Claude Code Hooks登録
 *
 * Phase 1実装:
 * - PreToolUse Hook: Task tool監視（ADR_016違反検出）
 * - PostToolUse Hook: SubAgent成果物実体確認
 */
export default {
  preToolUse: {
    matcher: "Task", // Task toolのみ監視
    handler: preToolUseHook
  },
  postToolUse: {
    matcher: "Task", // Task toolのみ監視
    handler: postToolUseHook
  }
};
