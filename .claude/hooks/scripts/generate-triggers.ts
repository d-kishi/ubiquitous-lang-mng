/**
 * Skills Triggers 自動生成スクリプト
 *
 * 各SKILL.mdのdescriptionから「」で囲まれたトリガーキーワードを自動抽出し、
 * skills-triggers.json を生成する。
 *
 * 使用方法:
 *   npx ts-node scripts/generate-triggers.ts
 *   または
 *   npm run generate-triggers
 *
 * @author Ubiquitous Language Manager Project
 * @date 2025-12-08
 */

import * as fs from 'fs';
import * as path from 'path';

// 型定義
interface SkillTrigger {
  name: string;
  triggers: string[];
  source: 'auto' | 'manual';  // 自動抽出 or 手動オーバーライド
}

interface TriggersConfig {
  generatedAt: string;
  skills: SkillTrigger[];
}

// パス設定
// scripts/ -> hooks/ -> .claude/ -> skills/
const SKILLS_DIR = path.resolve(__dirname, '../../skills');
const OUTPUT_FILE = path.resolve(__dirname, '../skills-triggers.json');
const MANUAL_OVERRIDES_FILE = path.resolve(__dirname, '../skills-triggers-manual.json');

/**
 * SKILL.mdのdescriptionからトリガーキーワードを抽出
 *
 * 「」で囲まれた部分をトリガーキーワードとして抽出
 * 例: 「技術決定」「設計判断」「ADR確認」 → ["技術決定", "設計判断", "ADR確認"]
 */
function extractTriggersFromDescription(description: string): string[] {
  const triggers: string[] = [];

  // 「」で囲まれた部分を抽出
  const japaneseQuotePattern = /「([^」]+)」/g;
  let match;
  while ((match = japaneseQuotePattern.exec(description)) !== null) {
    const trigger = match[1].trim();
    if (trigger && !triggers.includes(trigger)) {
      triggers.push(trigger);
    }
  }

  return triggers;
}

/**
 * SKILL.mdファイルからdescriptionを抽出
 *
 * YAML frontmatter形式:
 * ---
 * name: skill-name
 * description: ...
 * ---
 */
function extractDescriptionFromSkillMd(content: string): string | null {
  // 改行コードを正規化（CRLF -> LF）
  const normalizedContent = content.replace(/\r\n/g, '\n').replace(/\r/g, '\n');

  // YAML frontmatterを抽出
  const frontmatterMatch = normalizedContent.match(/^---\n([\s\S]*?)\n---/);
  if (!frontmatterMatch) {
    return null;
  }

  const frontmatter = frontmatterMatch[1];

  // descriptionを抽出
  const descriptionMatch = frontmatter.match(/^description:\s*(.+)$/m);
  if (!descriptionMatch) {
    return null;
  }

  return descriptionMatch[1].trim();
}

/**
 * 全Skillsディレクトリを走査してトリガーを収集
 */
function collectSkillTriggers(): SkillTrigger[] {
  const skills: SkillTrigger[] = [];

  if (!fs.existsSync(SKILLS_DIR)) {
    console.error(`[ERROR] Skills directory not found: ${SKILLS_DIR}`);
    return skills;
  }

  const skillDirs = fs.readdirSync(SKILLS_DIR, { withFileTypes: true })
    .filter(dirent => dirent.isDirectory())
    .map(dirent => dirent.name);

  for (const skillName of skillDirs) {
    const skillMdPath = path.join(SKILLS_DIR, skillName, 'SKILL.md');

    if (!fs.existsSync(skillMdPath)) {
      console.warn(`[WARN] SKILL.md not found for: ${skillName}`);
      continue;
    }

    try {
      const content = fs.readFileSync(skillMdPath, 'utf-8');
      const description = extractDescriptionFromSkillMd(content);

      if (!description) {
        console.warn(`[WARN] No description found in: ${skillName}/SKILL.md`);
        continue;
      }

      const triggers = extractTriggersFromDescription(description);

      if (triggers.length === 0) {
        console.warn(`[WARN] No triggers extracted from: ${skillName} (description: ${description.substring(0, 50)}...)`);
      }

      skills.push({
        name: skillName,
        triggers,
        source: 'auto'
      });

      console.log(`[OK] ${skillName}: ${triggers.length} triggers extracted`);

    } catch (error) {
      console.error(`[ERROR] Failed to process ${skillName}: ${error}`);
    }
  }

  return skills;
}

/**
 * 手動オーバーライド設定を読み込んでマージ
 */
function applyManualOverrides(skills: SkillTrigger[]): SkillTrigger[] {
  if (!fs.existsSync(MANUAL_OVERRIDES_FILE)) {
    console.log('[INFO] No manual overrides file found, using auto-detected triggers only');
    return skills;
  }

  try {
    const overridesContent = fs.readFileSync(MANUAL_OVERRIDES_FILE, 'utf-8');
    const overrides: { skills: SkillTrigger[] } = JSON.parse(overridesContent);

    for (const override of overrides.skills) {
      const existingIndex = skills.findIndex(s => s.name === override.name);

      if (existingIndex >= 0) {
        // 既存スキルのトリガーをマージまたは置換
        if (override.triggers && override.triggers.length > 0) {
          // オーバーライドトリガーを追加（重複排除）
          const mergedTriggers = [...new Set([...skills[existingIndex].triggers, ...override.triggers])];
          skills[existingIndex].triggers = mergedTriggers;
          skills[existingIndex].source = 'manual';
          console.log(`[OVERRIDE] ${override.name}: merged with manual triggers`);
        }
      } else {
        // 新規スキル追加（自動検出できなかった場合）
        skills.push({
          ...override,
          source: 'manual'
        });
        console.log(`[MANUAL] ${override.name}: added from manual overrides`);
      }
    }

  } catch (error) {
    console.error(`[ERROR] Failed to load manual overrides: ${error}`);
  }

  return skills;
}

/**
 * メイン処理
 */
function main(): void {
  console.log('=== Skills Triggers Generator ===\n');
  console.log(`Skills directory: ${SKILLS_DIR}`);
  console.log(`Output file: ${OUTPUT_FILE}\n`);

  // 1. 自動抽出
  let skills = collectSkillTriggers();

  // 2. 手動オーバーライド適用
  skills = applyManualOverrides(skills);

  // 3. 結果をJSONに出力
  const config: TriggersConfig = {
    generatedAt: new Date().toISOString(),
    skills: skills.sort((a, b) => a.name.localeCompare(b.name))
  };

  fs.writeFileSync(OUTPUT_FILE, JSON.stringify(config, null, 2), 'utf-8');

  console.log(`\n=== Summary ===`);
  console.log(`Total skills: ${skills.length}`);
  console.log(`Auto-detected: ${skills.filter(s => s.source === 'auto').length}`);
  console.log(`Manual overrides: ${skills.filter(s => s.source === 'manual').length}`);
  console.log(`Output: ${OUTPUT_FILE}`);
}

// 実行
main();
