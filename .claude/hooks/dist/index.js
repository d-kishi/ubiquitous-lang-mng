"use strict";
var __createBinding = (this && this.__createBinding) || (Object.create ? (function(o, m, k, k2) {
    if (k2 === undefined) k2 = k;
    var desc = Object.getOwnPropertyDescriptor(m, k);
    if (!desc || ("get" in desc ? !m.__esModule : desc.writable || desc.configurable)) {
      desc = { enumerable: true, get: function() { return m[k]; } };
    }
    Object.defineProperty(o, k2, desc);
}) : (function(o, m, k, k2) {
    if (k2 === undefined) k2 = k;
    o[k2] = m[k];
}));
var __setModuleDefault = (this && this.__setModuleDefault) || (Object.create ? (function(o, v) {
    Object.defineProperty(o, "default", { enumerable: true, value: v });
}) : function(o, v) {
    o["default"] = v;
});
var __importStar = (this && this.__importStar) || (function () {
    var ownKeys = function(o) {
        ownKeys = Object.getOwnPropertyNames || function (o) {
            var ar = [];
            for (var k in o) if (Object.prototype.hasOwnProperty.call(o, k)) ar[ar.length] = k;
            return ar;
        };
        return ownKeys(o);
    };
    return function (mod) {
        if (mod && mod.__esModule) return mod;
        var result = {};
        if (mod != null) for (var k = ownKeys(mod), i = 0; i < k.length; i++) if (k[i] !== "default") __createBinding(result, mod, k[i]);
        __setModuleDefault(result, mod);
        return result;
    };
})();
Object.defineProperty(exports, "__esModule", { value: true });
const fs = __importStar(require("fs/promises"));
async function checkStepStartExecuted(transcriptPath) {
    try {
        const transcriptContent = await fs.readFile(transcriptPath, 'utf-8');
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
    }
    catch (error) {
        console.error(`[PreToolUse] トランスクリプト読み込みエラー: ${error}`);
        return false;
    }
}
function validateSubAgentSelection(subagentType) {
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
async function preToolUseHook(input) {
    try {
        if (input.tool_name !== "Task") {
            return { decision: "approve" };
        }
        console.log(`[PreToolUse] Task tool実行前チェック開始`);
        console.log(`[PreToolUse] SubAgent: ${input.tool_input.subagent_type}`);
        const stepStartExecuted = await checkStepStartExecuted(input.transcript_path);
        if (!stepStartExecuted) {
            const errorMessage = `❌ ADR_016違反検出: step-start Command未実行\n\n` +
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
        const validationError = validateSubAgentSelection(input.tool_input.subagent_type);
        if (validationError) {
            console.warn(`[PreToolUse] SubAgent選択警告: ${validationError}`);
            return {
                decision: "approve",
                additionalContext: validationError
            };
        }
        console.log(`[PreToolUse] チェック完了: すべてのチェックをパス`);
        return { decision: "approve" };
    }
    catch (error) {
        console.error(`[PreToolUse] エラー発生: ${error}`);
        return {
            decision: "approve",
            additionalContext: `⚠️ PreToolUse Hookでエラーが発生しましたが、実行を継続します。\nエラー: ${error}`
        };
    }
}
function extractFilePaths(toolResponse) {
    const filePaths = [];
    const patterns = [
        /(?:作成|更新|生成)(?:しました|完了)?[:\s]+([^\s]+)/g,
        /(?:ファイル|成果物)[:\s]+`([^`]+)`/g,
        /(?:出力先|保存先)[:\s]+([^\s]+)/g,
        /([a-zA-Z0-9_\-/.\\]+\.(?:md|ts|js|json|txt))/g
    ];
    for (const pattern of patterns) {
        const matches = toolResponse.matchAll(pattern);
        for (const match of matches) {
            if (match[1]) {
                if (!filePaths.includes(match[1])) {
                    filePaths.push(match[1]);
                }
            }
        }
    }
    return filePaths;
}
async function checkFileExists(filePath) {
    try {
        await fs.access(filePath);
        return true;
    }
    catch (error) {
        return false;
    }
}
async function getFileSize(filePath) {
    try {
        const stats = await fs.stat(filePath);
        return stats.size;
    }
    catch (error) {
        return -1;
    }
}
async function postToolUseHook(input) {
    try {
        if (input.tool_name !== "Task") {
            return {};
        }
        console.log(`[PostToolUse] Task tool実行後チェック開始`);
        console.log(`[PostToolUse] SubAgent: ${input.tool_input.subagent_type}`);
        const filePaths = extractFilePaths(input.tool_response);
        if (filePaths.length === 0) {
            console.log(`[PostToolUse] ファイルパス未検出（成果物なしまたはパターン不一致）`);
            return {};
        }
        console.log(`[PostToolUse] 抽出ファイルパス: ${filePaths.join(", ")}`);
        const fileChecks = await Promise.all(filePaths.map(async (filePath) => {
            const exists = await checkFileExists(filePath);
            const size = exists ? await getFileSize(filePath) : -1;
            return { filePath, exists, size };
        }));
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
    }
    catch (error) {
        console.error(`[PostToolUse] エラー発生: ${error}`);
        return {
            additionalContext: `\n\n⚠️ PostToolUse Hookでエラーが発生しましたが、処理を継続します。\nエラー: ${error}`
        };
    }
}
exports.default = {
    preToolUse: {
        matcher: "Task",
        handler: preToolUseHook
    },
    postToolUse: {
        matcher: "Task",
        handler: postToolUseHook
    }
};
//# sourceMappingURL=index.js.map