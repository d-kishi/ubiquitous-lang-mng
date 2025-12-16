#!/usr/bin/env node
/**
 * Claude Code ステータスライン スクリプト
 * Windows/Linux両対応 - コンテキストウィンドウ使用率表示
 *
 * 設置場所: .claude/statusline.js（プロジェクト配下）
 * 設定: .claude/settings.local.json の statusLine に追加
 *
 * 表示内容:
 * - モデル名
 * - Gitブランチ名
 * - コンテキストウィンドウ使用率（色分け表示）
 * - セッションコスト
 */

// ANSIカラーコード定義
const colors = {
  reset: '\x1b[0m',    // リセット
  red: '\x1b[31m',     // 赤（警告: 85%以上）
  yellow: '\x1b[33m',  // 黄（注意: 70-84%）
  green: '\x1b[32m',   // 緑（正常: 70%未満）
  cyan: '\x1b[36m',    // シアン
  dim: '\x1b[2m'       // 薄暗い表示
};

// 標準入力からJSONを読み込み
let input = '';

process.stdin.setEncoding('utf8');
process.stdin.on('data', chunk => { input += chunk; });
process.stdin.on('end', () => {
  try {
    const data = JSON.parse(input);
    const output = formatStatusLine(data);
    console.log(output);
  } catch (e) {
    // JSON解析失敗時のフォールバック
    console.log('[Status] Ready');
  }
});

/**
 * ステータスライン文字列を生成
 * @param {Object} data - Claude Codeから渡されるJSON データ
 * @returns {string} フォーマット済みステータスライン
 */
function formatStatusLine(data) {
  const parts = [];

  // モデル名表示
  const model = data.model?.display_name || 'Claude';
  parts.push(`[${model}]`);

  // Gitブランチ名表示（ワークスペース情報から取得）
  const projectDir = data.workspace?.project_dir || '';
  if (projectDir) {
    try {
      const { execSync } = require('child_process');
      const branch = execSync('git branch --show-current', {
        cwd: projectDir,
        encoding: 'utf8',
        timeout: 1000,  // タイムアウト: 1秒
        stdio: ['pipe', 'pipe', 'ignore']  // stderrを無視（Windows互換）
      }).trim();
      if (branch) {
        parts.push(`🌿 ${branch}`);
      }
    } catch (e) {
      // Gitが利用不可、またはリポジトリでない場合は無視
    }
  }

  // コンテキストウィンドウ使用率表示
  const contextWindow = data.context_window;
  if (contextWindow) {
    const contextSize = contextWindow.context_window_size || 200000;
    const currentUsage = contextWindow.current_usage;

    let usedTokens = 0;
    if (currentUsage) {
      // v2.0.70以降: current_usageで正確な使用量を取得
      usedTokens = (currentUsage.input_tokens || 0) +
        (currentUsage.cache_creation_input_tokens || 0) +
        (currentUsage.cache_read_input_tokens || 0);
    } else {
      // フォールバック: 累積トークン数を使用
      usedTokens = contextWindow.total_input_tokens || 0;
    }

    const percentUsed = Math.round((usedTokens / contextSize) * 100);

    // 使用率に応じた色分け（プロジェクトの80%ルールに準拠）
    let contextColor = colors.green;
    let indicator = '✓';

    if (percentUsed >= 85) {
      // 85%以上: 赤（セッション終了推奨）
      contextColor = colors.red;
      indicator = '⚠️';
    } else if (percentUsed >= 70) {
      // 70-84%: 黄（手動compact検討）
      contextColor = colors.yellow;
      indicator = '⚡';
    }

    // 表示形式: 使用率% (使用量k/最大量k)
    const usedK = Math.round(usedTokens / 1000);
    const totalK = Math.round(contextSize / 1000);
    parts.push(`${indicator} ${contextColor}Context: ${percentUsed}%${colors.reset} ${colors.dim}(${usedK}k/${totalK}k)${colors.reset}`);
  }

  // セッションコスト表示（オプション）
  // ※Maxプラン利用のため無効化（必要時にコメント解除）
  // const cost = data.cost?.total_cost_usd;
  // if (cost !== undefined && cost > 0) {
  //   parts.push(`${colors.dim}$${cost.toFixed(4)}${colors.reset}`);
  // }

  return parts.join(' | ');
}
