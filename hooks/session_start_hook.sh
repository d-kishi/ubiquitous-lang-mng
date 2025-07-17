#!/bin/bash

# セッション開始時の必読プロセス Hook
# 使用方法: ./session_start_hook.sh
# 実行タイミング: Claude Code起動後の最初のセッション開始時

echo "=========================================="
echo "セッション開始時必読プロセス"
echo "実行日時: $(date '+%Y-%m-%d %H:%M:%S')"
echo "=========================================="

# 必読ファイルの定義
declare -A REQUIRED_FILES=(
    ["CLAUDE.md"]="プロジェクト概要・技術構成・フェーズ状況"
    ["Doc/06_Issues/コミュニケーション改善課題.md"]="コミュニケーション課題と改善策"
    ["Doc/プロジェクト状況.md"]="最新状況・次回予定・重要な制約"
)

# 直近3日の作業記録ディレクトリ
DAILY_DIR="Doc/04_Daily"

# 1. 必読ファイルの存在確認
echo ""
echo "【1. 必読ファイル存在確認】"
missing_files=()
for file in "${!REQUIRED_FILES[@]}"; do
    if [ -f "$file" ]; then
        echo "✅ $file"
    else
        echo "❌ $file が見つかりません"
        missing_files+=("$file")
    fi
done

if [ ${#missing_files[@]} -gt 0 ]; then
    echo ""
    echo "❌ 必読ファイルが不足しています。以下を確認してください："
    for file in "${missing_files[@]}"; do
        echo "  - $file"
    done
    exit 1
fi

# 2. 必読ファイルの読み込み指示
echo ""
echo "【2. 必読ファイル読み込み】"
echo "以下の順序で必読ファイルを読み込んでください（5分以内）:"
echo ""

file_counter=1
for file in "CLAUDE.md" "Doc/06_Issues/コミュニケーション改善課題.md" "Doc/プロジェクト状況.md"; do
    echo "${file_counter}. $file"
    echo "   目的: ${REQUIRED_FILES[$file]}"
    echo ""
    ((file_counter++))
done

echo "上記の必読ファイルを読み込みましたか？ (y/n): "
read -r required_files_read
if [ "$required_files_read" != "y" ]; then
    echo "必読ファイルを読み込んでから再度実行してください。"
    exit 1
fi

# 3. 直近3日の作業記録確認
echo ""
echo "【3. 直近3日の作業記録確認】"
if [ -d "$DAILY_DIR" ]; then
    echo "作業記録ディレクトリ: $DAILY_DIR"
    
    # 直近3日分のファイルを検索
    recent_files=$(find "$DAILY_DIR" -name "*.md" -type f -mtime -3 | head -10 | sort -r)
    
    if [ -n "$recent_files" ]; then
        echo ""
        echo "直近3日の作業記録ファイル:"
        echo "$recent_files" | while read -r file; do
            echo "  - $file"
        done
        echo ""
        echo "直近3日の作業記録を確認しましたか？ (y/n): "
        read -r daily_records_read
        if [ "$daily_records_read" != "y" ]; then
            echo "直近3日の作業記録を確認してから再度実行してください。"
            exit 1
        fi
    else
        echo "⚠️  直近3日の作業記録が見つかりません。"
        echo "新規プロジェクトまたは長期間の中断後の可能性があります。"
    fi
else
    echo "❌ 作業記録ディレクトリが見つかりません: $DAILY_DIR"
    exit 1
fi

# 4. Phase開始時の追加確認（該当する場合）
echo ""
echo "【4. Phase開始時の追加確認】"
echo "現在のセッションでPhaseを開始する予定がありますか？ (y/n): "
read -r phase_starting
if [ "$phase_starting" = "y" ]; then
    echo ""
    echo "Phase開始時の追加必読ファイル:"
    echo "- Doc/07_Decisions/ADR_012_階層構造統一ルール.md"
    echo ""
    echo "Phase開始時の追加必読ファイルを確認しましたか？ (y/n): "
    read -r phase_files_read
    if [ "$phase_files_read" != "y" ]; then
        echo "Phase開始時の追加必読ファイルを確認してから再度実行してください。"
        exit 1
    fi
fi

# 5. 進捗状況・優先事項の整理
echo ""
echo "【5. 進捗状況・優先事項の整理】"
echo "読み込んだ情報から以下を整理してください:"
echo ""
echo "□ 現在の進捗状況の把握"
echo "□ 今回セッションの優先事項の特定"
echo "□ 推奨作業内容の確認"
echo "□ 重要な制約・前提条件の確認"
echo ""
echo "進捗状況・優先事項の整理が完了しましたか？ (y/n): "
read -r progress_organized
if [ "$progress_organized" != "y" ]; then
    echo "進捗状況・優先事項の整理を完了してから再度実行してください。"
    exit 1
fi

# 6. セッション目的の確認
echo ""
echo "【6. セッション目的の確認】"
echo "ユーザーに今回のセッション目的を確認してください:"
echo ""
echo "「今回のセッションでは何を行いますか？」"
echo ""
echo "ユーザーからセッション目的の回答を得ましたか？ (y/n): "
read -r session_purpose_confirmed
if [ "$session_purpose_confirmed" != "y" ]; then
    echo "ユーザーからセッション目的の確認を取ってから再度実行してください。"
    exit 1
fi

# 7. 完了メッセージ
echo ""
echo "=========================================="
echo "セッション開始時必読プロセス完了"
echo "=========================================="
echo ""
echo "✅ 必読ファイル読み込み完了（5分以内）"
echo "✅ 直近3日の作業記録確認完了"
if [ "$phase_starting" = "y" ]; then
    echo "✅ Phase開始時の追加確認完了"
fi
echo "✅ 進捗状況・優先事項整理完了"
echo "✅ セッション目的確認完了"
echo ""
echo "セッション開始準備が完了しました。"
echo "dynamic_loading_hook.sh を実行して、動的読み込み決定を行ってください。"
echo ""

# 8. 次ステップの案内
echo "【次ステップ】"
echo "1. dynamic_loading_hook.sh を実行"
echo "2. 作業範囲の特定と必要ファイルの選択的読み込み"
echo "3. セッション目的に基づく作業開始"
echo ""

# 9. 重要な注意事項
echo "【重要な注意事項】"
echo "- AutoCompact対策: 定型手順はHooksで自動実行"
echo "- 品質基準: 0 Warning, 0 Error状態の維持"
echo "- セッション終了時: session_end_hook.sh を実行"
echo ""