#!/bin/bash

# 動的読み込み決定プロセス Hook
# 使用方法: ./dynamic_loading_hook.sh
# 実行タイミング: セッション開始時必読プロセス完了後

echo "=========================================="
echo "動的読み込み決定プロセス"
echo "実行日時: $(date '+%Y-%m-%d %H:%M:%S')"
echo "=========================================="

# Phase 1: 作業範囲特定（1-2分）
echo ""
echo "【Phase 1: 作業範囲特定（1-2分）】"
echo "ユーザーとの対話で以下を決定してください:"
echo ""

# 1. 作業内容・目的確認
echo "1. 今回の作業内容・目的確認"
echo "   ユーザーに今回の作業内容と目的を確認してください。"
echo ""
echo "作業内容・目的の確認が完了しましたか？ (y/n): "
read -r work_content_confirmed
if [ "$work_content_confirmed" != "y" ]; then
    echo "作業内容・目的の確認を完了してから再度実行してください。"
    exit 1
fi

# 2. 必要ADR特定
echo ""
echo "2. 必要ADR特定"
echo "   作業内容に応じて必要なADRを特定してください:"
echo ""
echo "   【ADRカテゴリ】"
echo "   - ADR_001-003: 基本記法・用語統一"
echo "   - ADR_007-009: エラーハンドリング・ログ・テスト"
echo "   - ADR_010: 実装規約（Blazor Server・F#コメント）"
echo "   - ADR_011: スクラム開発サイクル"
echo "   - ADR_012: 階層構造統一ルール（Phase開始時必読）"
echo "   - ADR_013: 組織管理サイクル運用規則（実装フェーズ必読）"
echo ""
echo "必要なADRを特定しましたか？ (y/n): "
read -r adr_identified
if [ "$adr_identified" != "y" ]; then
    echo "必要なADRを特定してから再度実行してください。"
    exit 1
fi

echo "特定したADRをカンマ区切りで入力してください（例: ADR_007,ADR_010,ADR_013）: "
read -r selected_adrs

# 3. 必要設計書特定
echo ""
echo "3. 必要設計書特定"
echo "   作業対象に応じて必要な設計書を特定してください:"
echo ""
echo "   【設計書カテゴリ】"
echo "   - システム設計書.md: アーキテクチャ全体"
echo "   - データベース設計書.md: DB関連作業"
echo "   - Application層インターフェース設計書.md: F#↔C#境界"
echo "   - UI設計/01_認証・ユーザー管理画面設計.md: 認証UI"
echo "   - UI設計/02_プロジェクト管理画面設計.md: プロジェクト管理UI"
echo "   - UI設計/03_ドメイン管理画面設計.md: ドメイン管理UI"
echo "   - UI設計/04_ユビキタス言語管理画面設計.md: 用語管理UI"
echo ""
echo "必要な設計書を特定しましたか？ (y/n): "
read -r design_docs_identified
if [ "$design_docs_identified" != "y" ]; then
    echo "必要な設計書を特定してから再度実行してください。"
    exit 1
fi

echo "特定した設計書をカンマ区切りで入力してください: "
read -r selected_design_docs

# 4. 組織設計適用有無
echo ""
echo "4. 組織設計適用有無"
echo "   Phase適応型組織を使用しますか？"
echo "   - Step実装時: 通常は使用"
echo "   - 軽微な修正: 使用しない場合もあり"
echo ""
echo "Phase適応型組織を使用しますか？ (y/n): "
read -r use_phase_org
if [ "$use_phase_org" = "y" ]; then
    echo "組織設計ファイルの場所: Doc/08_Organization/Active/"
    echo "組織レビューチェックリスト: Doc/08_Organization/組織レビューチェックリスト.md"
fi

# 5. 環境・課題情報の必要性確認
echo ""
echo "5. 環境・課題情報の必要性確認"
echo "   以下の情報が必要ですか？"
echo ""
echo "   【環境・課題情報】"
echo "   - Doc/09_Environment/: 環境設定・DB接続作業時"
echo "   - Doc/06_Issues/課題一覧.md: 問題解決・技術調査時"
echo ""
echo "環境・課題情報が必要ですか？ (y/n): "
read -r need_env_issues
if [ "$need_env_issues" = "y" ]; then
    echo "環境・課題情報を読み込み対象に追加します。"
fi

# Phase 2: 選択的読み込み実行（2-3分）
echo ""
echo "【Phase 2: 選択的読み込み実行（2-3分）】"
echo "決定した範囲のファイルを読み込んでください:"
echo ""

# 読み込み対象ファイル一覧の生成
echo "📋 読み込み対象ファイル一覧"
echo "================================"

# ADR読み込み
if [ -n "$selected_adrs" ]; then
    echo ""
    echo "【ADR】"
    IFS=',' read -ra ADR_ARRAY <<< "$selected_adrs"
    for adr in "${ADR_ARRAY[@]}"; do
        adr_file="Doc/07_Decisions/${adr// /}_*.md"
        echo "  - $adr_file"
    done
fi

# 設計書読み込み
if [ -n "$selected_design_docs" ]; then
    echo ""
    echo "【設計書】"
    IFS=',' read -ra DESIGN_ARRAY <<< "$selected_design_docs"
    for design in "${DESIGN_ARRAY[@]}"; do
        design_file="Doc/02_Design/${design// /}"
        echo "  - $design_file"
    done
fi

# 組織設計読み込み
if [ "$use_phase_org" = "y" ]; then
    echo ""
    echo "【組織設計】"
    echo "  - Doc/08_Organization/Active/ (該当Phase組織ファイル)"
    echo "  - Doc/08_Organization/組織レビューチェックリスト.md"
fi

# 環境・課題情報読み込み
if [ "$need_env_issues" = "y" ]; then
    echo ""
    echo "【環境・課題情報】"
    echo "  - Doc/09_Environment/ (該当ファイル)"
    echo "  - Doc/06_Issues/課題一覧.md"
fi

echo ""
echo "================================"
echo ""
echo "上記の読み込み対象ファイルを読み込みましたか？ (y/n): "
read -r files_loaded
if [ "$files_loaded" != "y" ]; then
    echo "読み込み対象ファイルを読み込んでから再度実行してください。"
    exit 1
fi

# 6. 読み込み完了確認
echo ""
echo "【6. 読み込み完了確認】"
echo "すべての必要ファイルの読み込みが完了しましたか？ (y/n): "
read -r all_loading_completed
if [ "$all_loading_completed" != "y" ]; then
    echo "すべてのファイル読み込みを完了してから再度実行してください。"
    exit 1
fi

# 7. 作業着手準備確認
echo ""
echo "【7. 作業着手準備確認】"
echo "読み込んだ情報を基に、作業着手の準備が完了しましたか？ (y/n): "
read -r work_ready
if [ "$work_ready" != "y" ]; then
    echo "作業着手の準備を完了してから再度実行してください。"
    exit 1
fi

# 8. 完了メッセージ
echo ""
echo "=========================================="
echo "動的読み込み決定プロセス完了"
echo "=========================================="
echo ""
echo "✅ Phase 1: 作業範囲特定完了（1-2分）"
echo "✅ Phase 2: 選択的読み込み実行完了（2-3分）"
echo ""
echo "📋 読み込み完了サマリー"
echo "- 選択したADR: $selected_adrs"
echo "- 選択した設計書: $selected_design_docs"
echo "- 組織設計使用: $use_phase_org"
echo "- 環境・課題情報: $need_env_issues"
echo ""
echo "動的読み込み決定プロセスが完了しました。"
echo "作業を開始してください。"
echo ""

# 9. 次ステップの案内
echo "【次ステップ】"
echo "1. 確認した作業内容・目的に基づく作業開始"
echo "2. 品質基準（0 Warning, 0 Error）の維持"
echo "3. 作業完了後は session_end_hook.sh を実行"
echo ""

# 10. 読み込み結果の記録
echo "【読み込み結果記録】"
echo "今回の読み込み結果を記録しています..."
{
    echo "# 動的読み込み決定結果 - $(date '+%Y-%m-%d %H:%M:%S')"
    echo ""
    echo "## 選択したADR"
    echo "$selected_adrs"
    echo ""
    echo "## 選択した設計書"
    echo "$selected_design_docs"
    echo ""
    echo "## 組織設計使用"
    echo "$use_phase_org"
    echo ""
    echo "## 環境・課題情報"
    echo "$need_env_issues"
    echo ""
} > "/tmp/dynamic_loading_result_$(date '+%Y%m%d_%H%M%S').md"
echo "読み込み結果を記録しました。"
echo ""