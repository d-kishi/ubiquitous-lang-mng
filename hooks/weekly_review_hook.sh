#!/bin/bash

# 週次振り返りでの体系見直し Hook
# 使用方法: ./weekly_review_hook.sh
# 実行タイミング: 週次総括時（週末セッション完了時）

echo "=========================================="
echo "週次振り返りでの体系見直し"
echo "実行日時: $(date '+%Y-%m-%d %H:%M:%S')"
echo "=========================================="

# 週末判定
current_day=$(date '+%w')  # 0=日曜日, 6=土曜日
if [ "$current_day" != "0" ] && [ "$current_day" != "6" ]; then
    echo "⚠️  現在は週末ではありません（$(date '+%A')）。"
    echo "週次振り返りは週末に実施することを推奨します。"
    echo "それでも実行しますか？ (y/n): "
    read -r force_execute
    if [ "$force_execute" != "y" ]; then
        echo "週末に再度実行してください。"
        exit 0
    fi
fi

# 1. CLAUDE.md妥当性チェック
echo ""
echo "【1. CLAUDE.md妥当性チェック】"
echo "CLAUDE.mdの内容を確認し、以下の問題がないかチェックしてください:"
echo ""
echo "チェック項目:"
echo "- 論理的矛盾（矛盾する記述・整合性の問題）"
echo "- 古い情報（更新されていない情報・廃止された手法）"
echo "- 不要セクション（使われていない・重複している内容）"
echo "- 肥大化（過度に長い・複雑すぎる記述）"
echo "- 役割逸脱（Why以外の内容が含まれている）"
echo ""
echo "CLAUDE.md妥当性チェックを実施しましたか？ (y/n): "
read -r claude_md_checked
if [ "$claude_md_checked" != "y" ]; then
    echo "CLAUDE.md妥当性チェックを実施してから再度実行してください。"
    exit 1
fi

echo "CLAUDE.mdで発見された問題を入力してください（なければ「なし」）: "
read -r claude_md_issues

# 2. ドキュメント運用評価
echo ""
echo "【2. ドキュメント運用評価】"
echo "プロジェクト全体のドキュメント運用を評価してください:"
echo ""
echo "評価観点:"
echo "- ファイル配置（適切なディレクトリ・命名規則）"
echo "- 命名規則（一貫性・分かりやすさ・規則遵守）"
echo "- 役割分担（各文書の責任範囲・重複排除）"
echo "- アクセス性（見つけやすさ・参照しやすさ）"
echo "- 更新頻度（適切な更新・陳腐化防止）"
echo ""
echo "ドキュメント運用評価を実施しましたか？ (y/n): "
read -r document_evaluation_done
if [ "$document_evaluation_done" != "y" ]; then
    echo "ドキュメント運用評価を実施してから再度実行してください。"
    exit 1
fi

echo "ドキュメント運用で発見された問題を入力してください（なければ「なし」）: "
read -r document_issues

# 3. セッション管理最適化
echo ""
echo "【3. セッション管理最適化】"
echo "セッション管理の効率性を確認してください:"
echo ""
echo "確認項目:"
echo "- 読み込みパターン（効率的な情報取得・無駄な読み込み排除）"
echo "- チェックリスト（網羅性・実用性・簡潔性）"
echo "- Hooks実行（自動化効果・エラー発生状況）"
echo "- セッション時間（開始・終了時間の効率化）"
echo "- 情報継承（セッション間の情報伝達品質）"
echo ""
echo "セッション管理最適化確認を実施しましたか？ (y/n): "
read -r session_management_checked
if [ "$session_management_checked" != "y" ]; then
    echo "セッション管理最適化確認を実施してから再度実行してください。"
    exit 1
fi

echo "セッション管理で発見された問題を入力してください（なければ「なし」）: "
read -r session_issues

# 4. プロセス改善提案
echo ""
echo "【4. プロセス改善提案】"
echo "発見された非効率な部分の改善策を検討してください:"
echo ""
echo "改善提案の観点:"
echo "- 手順簡略化（不要な手順・重複する確認の排除）"
echo "- 自動化拡大（Hooks化可能な手順・定型作業）"
echo "- 品質向上（エラー防止・見落とし防止）"
echo "- 効率化（時間短縮・負荷軽減）"
echo "- 使いやすさ向上（直感的な操作・分かりやすい手順）"
echo ""
echo "プロセス改善提案を策定しましたか？ (y/n): "
read -r process_improvement_proposed
if [ "$process_improvement_proposed" != "y" ]; then
    echo "プロセス改善提案を策定してから再度実行してください。"
    exit 1
fi

echo "優先度の高いプロセス改善提案を入力してください（なければ「なし」）: "
read -r priority_process_improvements

# 5. 文書合理化チェック
echo ""
echo "【5. 文書合理化チェック】"
echo "重要文書の合理化状況を確認してください:"
echo ""
echo "対象文書:"
echo "- CLAUDE.md（プロジェクト指針・基本方針）"
echo "- プロジェクト状況.md（進捗管理・次回予定）"
echo "- ADR文書（アーキテクチャ決定記録）"
echo "- 組織設計文書（Phase適応型組織）"
echo ""
echo "確認項目:"
echo "- 肥大化（過度に長い・複雑すぎる）"
echo "- 重複（同じ内容の繰り返し・類似記述）"
echo "- 役割逸脱（本来の目的から外れた内容）"
echo "- 陳腐化（古い情報・使われていない内容）"
echo ""
echo "文書合理化チェックを実施しましたか？ (y/n): "
read -r document_rationalization_checked
if [ "$document_rationalization_checked" != "y" ]; then
    echo "文書合理化チェックを実施してから再度実行してください。"
    exit 1
fi

echo "文書合理化で発見された問題を入力してください（なければ「なし」）: "
read -r rationalization_issues

# 6. 合理化提案の策定
echo ""
echo "【6. 合理化提案の策定】"
echo "発見された問題に対する具体的な合理化提案を策定してください:"
echo ""
echo "合理化提案の種類:"
echo "- 内容削減（不要な記述・重複の排除）"
echo "- 構造改善（章立て・見出し・整理）"
echo "- 役割明確化（各文書の責任範囲）"
echo "- Hooks化（定型手順の自動化）"
echo "- 統合・分離（文書の最適な分割・統合）"
echo ""
echo "合理化提案を策定しましたか？ (y/n): "
read -r rationalization_proposed
if [ "$rationalization_proposed" != "y" ]; then
    echo "合理化提案を策定してから再度実行してください。"
    exit 1
fi

echo "最優先の合理化提案を入力してください（なければ「なし」）: "
read -r priority_rationalization

# 7. 週次レビュー記録の生成
echo ""
echo "【7. 週次レビュー記録の生成】"
echo "週次振り返りの記録を生成しています..."

CURRENT_DATE=$(date '+%Y-%m-%d')
WEEK_NUMBER=$(date '+%Y-W%U')
REVIEW_RECORD="週次振り返り記録_${WEEK_NUMBER}.md"

cat > "/tmp/$REVIEW_RECORD" << EOF
# 週次振り返り記録 - ${WEEK_NUMBER}

## 📋 実施日時
**日時**: ${CURRENT_DATE}
**週**: ${WEEK_NUMBER}
**実施者**: Claude Code

## 🔍 CLAUDE.md妥当性チェック

### 発見された問題
${claude_md_issues}

### チェック項目
- 論理的矛盾
- 古い情報
- 不要セクション
- 肥大化
- 役割逸脱

## 📄 ドキュメント運用評価

### 発見された問題
${document_issues}

### 評価観点
- ファイル配置
- 命名規則
- 役割分担
- アクセス性
- 更新頻度

## ⚙️ セッション管理最適化

### 発見された問題
${session_issues}

### 確認項目
- 読み込みパターン
- チェックリスト
- Hooks実行
- セッション時間
- 情報継承

## 💡 プロセス改善提案

### 優先度の高い改善提案
${priority_process_improvements}

### 改善提案の観点
- 手順簡略化
- 自動化拡大
- 品質向上
- 効率化
- 使いやすさ向上

## 📋 文書合理化チェック

### 発見された問題
${rationalization_issues}

### 確認項目
- 肥大化
- 重複
- 役割逸脱
- 陳腐化

## 🎯 合理化提案

### 最優先の合理化提案
${priority_rationalization}

### 合理化提案の種類
- 内容削減
- 構造改善
- 役割明確化
- Hooks化
- 統合・分離

## 🔄 定型業務Hooks化候補評価

### Hooks化候補
${hooks_candidates}

### 評価基準
- 効果が実証された定型手順
- AutoCompactで失われると重大な影響
- 頻繁に実行される重要プロセス
- 手順が安定化・標準化された

## 🚀 次週実施事項

### 改善実施計画
- プロセス改善の実施
- 文書合理化の実施
- Hooks機能の改善
- 品質向上の取り組み
- 定型業務Hooks化候補の実装

### 継続監視事項
- CLAUDE.md妥当性の継続確認
- ドキュメント運用の継続評価
- セッション管理の継続最適化

---

**記録者**: Claude Code
**次回実施**: 次週末
**継続的改善**: 日次セッションでの継続実施
EOF

echo "✅ 週次振り返り記録を生成しました: /tmp/$REVIEW_RECORD"

# 8. 完了メッセージ
echo ""
echo "=========================================="
echo "週次振り返りでの体系見直し完了"
echo "=========================================="
echo ""
echo "✅ CLAUDE.md妥当性チェック完了"
echo "✅ ドキュメント運用評価完了"
echo "✅ セッション管理最適化完了"
echo "✅ プロセス改善提案完了"
echo "✅ 文書合理化チェック完了"
echo "✅ 合理化提案策定完了"
echo "✅ 週次レビュー記録生成完了"
echo ""
echo "週次振り返りでの体系見直しが完了しました。"
echo ""

# 9. 定型業務Hooks化候補評価
echo ""
echo "【9. 定型業務Hooks化候補評価】"
echo "ADR_014（定型業務管理3段階成熟化プロセス）に基づいて評価してください:"
echo ""
echo "評価対象の定型業務を確認してください:"
echo "- 現在CLAUDE.mdやADRで管理されている定型手順"
echo "- 試験運用中の新しいプロセス"
echo "- 効果測定が完了した手法"
echo ""
echo "Hooks化候補の評価基準:"
echo "✅ 効果が実証された定型手順"
echo "✅ AutoCompactで失われると重大な影響"
echo "✅ 頻繁に実行される重要プロセス"
echo "✅ 手順が安定化・標準化された"
echo ""
echo "定型業務Hooks化候補の評価を実施しましたか？ (y/n): "
read -r hooks_candidate_evaluated
if [ "$hooks_candidate_evaluated" != "y" ]; then
    echo "定型業務Hooks化候補の評価を実施してから再度実行してください。"
    exit 1
fi

echo "Hooks化すべき候補があれば入力してください（なければ「なし」）: "
read -r hooks_candidates

# 10. 改善実施の推奨
echo ""
echo "【10. 改善実施の推奨】"
echo "以下の改善を次週から実施することを推奨します:"
echo "- 発見された問題の解決"
echo "- 提案された改善策の実施"
echo "- 文書合理化の実行"
echo "- プロセス最適化の適用"
echo "- 定型業務Hooks化候補の実装"
echo ""

# 11. 次回週次振り返り準備
echo "【11. 次回週次振り返り準備】"
echo "次回の週次振り返りまでに以下を実施してください:"
echo "- 今回の改善提案の実施"
echo "- 改善効果の測定・記録"
echo "- 新たな問題の発見・記録"
echo "- 継続的改善の実施"
echo ""

# 12. 重要な注意事項
echo "【12. 重要な注意事項】"
echo "- 週次振り返りは毎週実施"
echo "- 改善提案は必ず実施"
echo "- 体系見直しは継続的に実施"
echo "- 品質向上を常に意識"
echo ""
echo "週次振り返りでの体系見直しが正常に完了しました。"
echo ""