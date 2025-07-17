#!/bin/bash

# セッション終了時記録チェックリスト Hook
# 使用方法: ./session_record_hook.sh
# 実行タイミング: セッション終了時確認プロセス完了後

echo "=========================================="
echo "セッション終了時記録チェックリスト"
echo "実行日時: $(date '+%Y-%m-%d %H:%M:%S')"
echo "=========================================="

# 日次記録ファイルの設定
CURRENT_DATE=$(date '+%Y-%m-%d')
CURRENT_YEAR_MONTH=$(date '+%Y-%m')
DAILY_DIR="Doc/04_Daily/${CURRENT_YEAR_MONTH}"
RECORD_FILE="${DAILY_DIR}/${CURRENT_DATE}.md"

# ディレクトリ作成
if [ ! -d "$DAILY_DIR" ]; then
    echo "日次記録ディレクトリを作成しています: $DAILY_DIR"
    mkdir -p "$DAILY_DIR"
fi

echo ""
echo "記録ファイル: $RECORD_FILE"
echo ""

# 1. 成果・実績記録
echo "【1. 成果・実績記録】"
echo "以下の項目を記録してください:"
echo ""
echo "□ 今回セッションの主要成果整理（具体的な完了項目・創出物）"
echo "□ 完了・未完了事項の明確化（ToDoリスト状況・継続課題）"
echo "□ 技術的知見・学習事項の記録（新発見・ベストプラクティス）"
echo "□ 問題解決の経緯記録（エラー対応・解決手法・回避策）"
echo ""
echo "成果・実績記録が完了しましたか？ (y/n): "
read -r achievements_recorded
if [ "$achievements_recorded" != "y" ]; then
    echo "成果・実績記録を完了してから再度実行してください。"
    exit 1
fi

echo "主要成果を簡潔に入力してください: "
read -r main_achievements

# 2. 品質・効率評価
echo ""
echo "【2. 品質・効率評価】"
echo "以下の項目を評価・記録してください:"
echo ""
echo "□ セッション目的達成度評価（100%/80%/60%等の定量評価）"
echo "□ 時間効率測定（予定時間vs実際時間・効率化要因）"
echo "□ 適用手法の効果検証（Phase適応型組織・Gemini連携等の効果）"
echo "□ 品質評価（成果物品質・プロセス品質・満足度）"
echo ""
echo "品質・効率評価が完了しましたか？ (y/n): "
read -r quality_evaluated
if [ "$quality_evaluated" != "y" ]; then
    echo "品質・効率評価を完了してから再度実行してください。"
    exit 1
fi

echo "セッション目的達成度を入力してください（%）: "
read -r achievement_rate

# 3. 課題・改善管理
echo ""
echo "【3. 課題・改善管理】"
echo "以下の項目を確認・記録してください:"
echo ""
echo "□ 発見された課題・問題点の記録（技術的・プロセス的・コミュニケーション）"
echo "□ 継続課題の更新（解決済みマーク・新規追加・優先度変更）"
echo "□ コミュニケーション改善事項の確認（COM-XXX課題の状況更新）"
echo "□ プロセス改善提案（次回セッション以降の改善案）"
echo ""
echo "課題・改善管理が完了しましたか？ (y/n): "
read -r issues_managed
if [ "$issues_managed" != "y" ]; then
    echo "課題・改善管理を完了してから再度実行してください。"
    exit 1
fi

echo "発見された主要課題があれば入力してください（なければ「なし」）: "
read -r main_issues

# 4. 次回準備・引き継ぎ
echo ""
echo "【4. 次回準備・引き継ぎ】"
echo "以下の項目を設定・記録してください:"
echo ""
echo "□ 次回セッション予定事項の設定（具体的作業内容・準備事項）"
echo "□ 申し送り事項の更新（重要な制約・前提・注意点）"
echo "□ 技術的前提条件の整理（開発環境・依存関係・設定状況）"
echo "□ Phase状況の更新（現在Phase・次Phase・実装計画）"
echo ""
echo "次回準備・引き継ぎが完了しましたか？ (y/n): "
read -r next_prep_completed
if [ "$next_prep_completed" != "y" ]; then
    echo "次回準備・引き継ぎを完了してから再度実行してください。"
    exit 1
fi

echo "次回セッション最優先事項を入力してください: "
read -r next_priority

# 5. 記録・文書化
echo ""
echo "【5. 記録・文書化】"
echo "以下の項目を実行・確認してください:"
echo ""
echo "□ 日次作業記録ファイル作成・更新"
echo "□ 重要な決定事項のADR化検討（技術選定・アーキテクチャ決定等）"
echo "□ プロジェクト状況.md の次回セッション情報更新"
echo "□ 週次総括への反映事項確認（週末セッションの場合）"
echo ""

# 記録ファイルの自動生成
echo "日次作業記録ファイルを自動生成しています..."
cat > "$RECORD_FILE" << EOF
# ${CURRENT_DATE} セッション記録

## 📋 セッション概要

**セッション開始**: ${CURRENT_DATE}
**セッション目的**: [セッション開始時に設定した目的]
**セッション結果**: 目的達成度 ${achievement_rate}%

## 🎯 成果・実績記録

### 主要成果
${main_achievements}

### 完了・未完了事項
- ✅ [完了した作業項目]
- ⏳ [未完了・継続課題]

### 技術的知見・学習事項
- [新発見・ベストプラクティス]
- [技術的改善点]

### 問題解決の経緯
- [エラー対応・解決手法・回避策]

## 🔍 品質・効率評価

### セッション目的達成度
**${achievement_rate}%** - [達成度の詳細説明]

### 時間効率測定
- **予定時間**: [予定していた時間]
- **実際時間**: [実際にかかった時間]
- **効率化要因**: [効率化に寄与した要素]

### 適用手法の効果検証
- **Phase適応型組織**: [効果・改善点]
- **Gemini連携**: [効果・改善点]
- **Hooks自動化**: [効果・改善点]

### 品質評価
- **成果物品質**: [品質レベル・改善点]
- **プロセス品質**: [プロセスの効率性・改善点]
- **満足度**: [全体的な満足度]

## 📋 課題・改善管理

### 発見された課題・問題点
${main_issues}

### 継続課題の更新
- [解決済み課題]
- [新規追加課題]
- [優先度変更課題]

### コミュニケーション改善事項
- [COM-XXX課題の状況更新]

### プロセス改善提案
- [次回セッション以降の改善案]

## 📈 継続的改善の実施

### 改善点の記録
${main_improvements}

### 改善案の策定
${priority_improvements}

### 効果測定の設定
- 定量的指標・定性的指標による改善効果測定

### 知見蓄積
${important_knowledge}

## 🚀 次回準備・引き継ぎ

### 次回セッション予定事項
${next_priority}

### 申し送り事項
- **重要な制約**: [制約・前提・注意点]
- **前提**: [技術的前提条件]
- **注意点**: [特別な注意事項]

### 技術的前提条件
- **開発環境**: [開発環境の状況]
- **依存関係**: [依存関係の状況]
- **設定状況**: [設定・構成の状況]

### Phase状況
- **現在Phase**: [現在のPhase]
- **次Phase**: [次のPhase]
- **実装計画**: [実装計画の状況]

## 📄 記録・文書化

### 重要な決定事項
- [技術選定・アーキテクチャ決定等]

### ADR化検討
- [ADR化が必要な決定事項]

### プロジェクト状況.md更新
- [次回セッション情報更新の内容]

### 週次総括への反映事項
- [週次総括に反映すべき事項]

---

**記録者**: Claude Code
**次回予定**: [次回セッション予定]
**状態**: [現在の状態]
**改善効果期待**: [期待される改善効果]
EOF

echo "✅ 日次作業記録ファイルを作成しました: $RECORD_FILE"
echo ""

echo "重要な決定事項のADR化が必要ですか？ (y/n): "
read -r need_adr
if [ "$need_adr" = "y" ]; then
    echo "⚠️  ADR化が必要な決定事項があります。後で作成してください。"
fi

echo "プロジェクト状況.md の次回セッション情報を更新しましたか？ (y/n): "
read -r project_status_updated
if [ "$project_status_updated" != "y" ]; then
    echo "⚠️  プロジェクト状況.md の更新を忘れずに行ってください。"
fi

# 週末セッションの場合の確認
current_day=$(date '+%w')  # 0=日曜日, 6=土曜日
if [ "$current_day" = "0" ] || [ "$current_day" = "6" ]; then
    echo "週末セッションです。週次総括への反映事項を確認してください。"
fi

# 6. 完了メッセージ
echo ""
echo "=========================================="
echo "セッション終了時記録チェックリスト完了"
echo "=========================================="
echo ""
echo "✅ 成果・実績記録完了"
echo "✅ 品質・効率評価完了"
echo "✅ 課題・改善管理完了"
echo "✅ 次回準備・引き継ぎ完了"
echo "✅ 記録・文書化完了"
echo ""
echo "📄 作成されたファイル: $RECORD_FILE"
echo ""
echo "セッション終了時記録が完了しました。"
echo ""

# 7. 次回セッションに向けた準備状況確認
echo "【7. 次回セッション準備状況】"
echo "次回セッション開始時に必要な準備:"
echo "- session_start_hook.sh の実行"
echo "- 作成された記録ファイルの確認"
echo "- 継続課題の把握"
echo "- 次回最優先事項の実施"
echo ""
echo "次回セッションの準備が整いました。"
echo ""

# 8. 継続的改善の実施
echo "【8. 継続的改善の実施】"
echo "手法の継続的改善を実施してください:"
echo ""

# 8.1. 改善点の記録
echo "8.1. 改善点の記録"
echo "実施中に発見された問題点・非効率な部分を記録してください:"
echo "- 技術的問題（エラー・パフォーマンス・設計課題）"
echo "- プロセス的問題（手順・効率性・品質保証）"
echo "- コミュニケーション問題（理解齟齬・情報伝達・確認不足）"
echo "- ツール・環境問題（開発環境・依存関係・設定）"
echo ""
echo "改善点を記録しましたか？ (y/n): "
read -r improvements_recorded
if [ "$improvements_recorded" != "y" ]; then
    echo "改善点を記録してから継続してください。"
fi

echo "主要な改善点を入力してください（なければ「なし」）: "
read -r main_improvements

# 8.2. 改善案の策定
echo ""
echo "8.2. 改善案の策定"
echo "次回セッション以降での改善策を策定してください:"
echo "- 即座に実行可能な改善策"
echo "- 中期的な改善策"
echo "- 長期的な改善策"
echo ""
echo "改善案を策定しましたか？ (y/n): "
read -r improvement_plans_created
if [ "$improvement_plans_created" != "y" ]; then
    echo "改善案を策定してから継続してください。"
fi

echo "優先度の高い改善案を入力してください（なければ「なし」）: "
read -r priority_improvements

# 8.3. 効果測定の設定
echo ""
echo "8.3. 効果測定の設定"
echo "改善策の実施効果を測定する方法を設定してください:"
echo "- 定量的指標（時間短縮・エラー削減・品質向上）"
echo "- 定性的指標（満足度・理解度・使いやすさ）"
echo ""
echo "効果測定の設定を行いましたか？ (y/n): "
read -r effect_measurement_set
if [ "$effect_measurement_set" != "y" ]; then
    echo "効果測定の設定を行ってから継続してください。"
fi

# 8.4. 知見蓄積
echo ""
echo "8.4. 知見蓄積"
echo "成功パターン・失敗パターンの体系化を行ってください:"
echo "- 成功パターン（うまくいった手法・アプローチ）"
echo "- 失敗パターン（避けるべき手法・陥りやすいミス）"
echo "- 教訓（学んだこと・気づき・ベストプラクティス）"
echo ""
echo "知見蓄積を行いましたか？ (y/n): "
read -r knowledge_accumulated
if [ "$knowledge_accumulated" != "y" ]; then
    echo "知見蓄積を行ってから継続してください。"
fi

echo "重要な知見を入力してください（なければ「なし」）: "
read -r important_knowledge

# 9. 記録品質の確認
echo ""
echo "【9. 記録品質確認】"
echo "作成された記録ファイルの品質を確認してください:"
echo "- 具体的な成果が記録されているか"
echo "- 継続課題が明確に記載されているか"
echo "- 次回セッションの方向性が明確か"
echo "- 技術的な詳細が適切に記録されているか"
echo "- 継続的改善の内容が記録されているか"
echo ""
echo "記録品質の確認が完了しましたか？ (y/n): "
read -r record_quality_checked
if [ "$record_quality_checked" != "y" ]; then
    echo "⚠️  記録品質を確認し、必要に応じて修正してください。"
fi

echo ""
echo "セッション終了時記録チェックリストの処理が完了しました。"
echo ""