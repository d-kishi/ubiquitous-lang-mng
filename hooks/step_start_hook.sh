#!/bin/bash

# Step開始時チェックリスト Hook
# 使用方法: ./step_start_hook.sh <phase_name> <step_number>
# 例: ./step_start_hook.sh A1 2

PHASE_NAME=$1
STEP_NUMBER=$2

if [ -z "$PHASE_NAME" ] || [ -z "$STEP_NUMBER" ]; then
    echo "使用方法: $0 <phase_name> <step_number>"
    echo "例: $0 A1 2"
    exit 1
fi

ORGANIZATION_FILE="Doc/08_Organization/Active/Phase_${PHASE_NAME}_Organization.md"
ADR_013_FILE="Doc/07_Decisions/ADR_013_組織管理サイクル運用規則.md"
CHECKLIST_FILE="Doc/08_Organization/組織レビューチェックリスト.md"

echo "=========================================="
echo "Step開始時チェックリスト"
echo "Phase: ${PHASE_NAME}, Step: ${STEP_NUMBER}"
echo "=========================================="

# 1. ADR_013確認
echo ""
echo "【1. ADR_013（組織管理サイクル運用規則）確認】"
if [ -f "$ADR_013_FILE" ]; then
    echo "✅ ADR_013ファイル存在確認: $ADR_013_FILE"
    echo "ADR_013の内容を確認しましたか？ (y/n): "
    read -r adr_confirmed
    if [ "$adr_confirmed" != "y" ]; then
        echo "ADR_013を確認してから再度実行してください。"
        exit 1
    fi
else
    echo "❌ ADR_013ファイルが見つかりません: $ADR_013_FILE"
    exit 1
fi

# 2. 前Step組織レビュー結果確認
echo ""
echo "【2. 前Step組織レビュー結果確認】"
if [ -f "$ORGANIZATION_FILE" ]; then
    echo "✅ 組織ファイル存在確認: $ORGANIZATION_FILE"
    
    if [ "$STEP_NUMBER" -gt 1 ]; then
        echo "前Step $((STEP_NUMBER - 1)) のレビュー結果を確認しましたか？ (y/n): "
        read -r prev_review_confirmed
        if [ "$prev_review_confirmed" != "y" ]; then
            echo "前Stepレビュー結果を確認してから再度実行してください。"
            exit 1
        fi
    else
        echo "✅ Step 1のため、前Stepレビュー結果確認をスキップします。"
    fi
else
    echo "❌ 組織ファイルが見つかりません: $ORGANIZATION_FILE"
    exit 1
fi

# 3. 現Step組織設計の妥当性確認
echo ""
echo "【3. 現Step組織設計の妥当性確認】"
echo "現Step ${STEP_NUMBER} の組織設計の妥当性を確認しましたか？ (y/n): "
read -r current_org_confirmed
if [ "$current_org_confirmed" != "y" ]; then
    echo "現Step組織設計を確認してから再度実行してください。"
    exit 1
fi

# 4. Step終了時必須プロセスの再確認
echo ""
echo "【4. Step終了時必須プロセスの再確認】"
echo "Step終了時に実行すべき必須プロセスを再確認しましたか？ (y/n): "
echo "（組織レビュー → ユーザー確認 → 次Step組織設計 → ユーザー承認 → 次Step開始）"
read -r end_process_confirmed
if [ "$end_process_confirmed" != "y" ]; then
    echo "Step終了時必須プロセスを確認してから再度実行してください。"
    exit 1
fi

# 5. 実装フェーズ特有の確認事項
echo ""
echo "【5. 実装フェーズ特有の確認事項】"
echo "以下の実装フェーズ特有の確認事項を確認しましたか？ (y/n): "
echo "- ビルド成功状態の維持（0 Warning, 0 Error）"
echo "- Clean Architecture依存関係の遵守"
echo "- F#初学者対応コメントの徹底"
read -r impl_phase_confirmed
if [ "$impl_phase_confirmed" != "y" ]; then
    echo "実装フェーズ特有の確認事項を確認してから再度実行してください。"
    exit 1
fi

# 6. 関連ファイルの存在確認
echo ""
echo "【6. 関連ファイルの存在確認】"
echo "関連ファイルの存在確認:"

files_to_check=(
    "CLAUDE.md"
    "Doc/プロジェクト状況.md"
    "Doc/06_Issues/コミュニケーション改善課題.md"
    "$CHECKLIST_FILE"
)

all_files_exist=true
for file in "${files_to_check[@]}"; do
    if [ -f "$file" ]; then
        echo "✅ $file"
    else
        echo "❌ $file が見つかりません"
        all_files_exist=false
    fi
done

if [ "$all_files_exist" = false ]; then
    echo "必要なファイルが不足しています。"
    exit 1
fi

# 7. Step1専用：構造化分析プロセス（Step1の場合のみ実行）
if [ "$STEP_NUMBER" = "1" ]; then
    echo ""
    echo "【7. Step1専用：構造化分析プロセス】"
    echo "Step1では構造化分析プロセスを実施します。"
    echo "Phase適応型組織による専門役割分析を実行しますか？ (y/n): "
    read -r execute_structured_analysis
    if [ "$execute_structured_analysis" = "y" ]; then
        echo ""
        echo "構造化分析プロセスを実施してください："
        echo ""
        echo "Phase 1: 専門役割による課題発見（30分）"
        echo "├── 各専門役割を順次実行"
        echo "├── 役割毎のGemini技術調査"
        echo "├── 課題・リスク・ベストプラクティス抽出"
        echo "└── 実装アプローチ候補の列挙"
        echo ""
        echo "Phase 1（30分）が完了しましたか？ (y/n): "
        read -r phase1_completed
        if [ "$phase1_completed" != "y" ]; then
            echo "Phase 1を完了してから再度実行してください。"
            exit 1
        fi
        
        echo ""
        echo "Phase 2: 集約・優先順位付け（30分）"
        echo "├── 各役割結果の統合・重複排除"
        echo "├── 相互依存関係の整理"
        echo "├── 影響度・緊急度による優先順位付け"
        echo "└── 実装順序決定"
        echo ""
        echo "Phase 2（30分）が完了しましたか？ (y/n): "
        read -r phase2_completed
        if [ "$phase2_completed" != "y" ]; then
            echo "Phase 2を完了してから再度実行してください。"
            exit 1
        fi
        
        echo ""
        echo "Phase 3: 統合解決策立案（60分）"
        echo "├── 高優先課題の統合解決策検討"
        echo "├── 各専門観点の実装パターン統合"
        echo "├── Clean Architecture実装順序確定"
        echo "└── 次Session具体的作業計画策定"
        echo ""
        echo "Phase 3（60分）が完了しましたか？ (y/n): "
        read -r phase3_completed
        if [ "$phase3_completed" != "y" ]; then
            echo "Phase 3を完了してから再度実行してください。"
            exit 1
        fi
        
        echo ""
        echo "✅ 構造化分析プロセス完了（合計120分）"
        echo "- Phase 1: 専門役割による課題発見"
        echo "- Phase 2: 集約・優先順位付け"
        echo "- Phase 3: 統合解決策立案"
    else
        echo "構造化分析プロセスをスキップしました。"
    fi
fi

# 8. Phase A1 特有の確認事項（Phase A1の場合のみ）
if [ "$PHASE_NAME" = "A1" ]; then
    echo ""
    echo "【8. Phase A1 特有の確認事項】"
    echo "Phase A1 基本認証システム実装の確認事項:"
    echo "- データベース設計書PostgreSQL互換性確認済み"
    echo "- ASP.NET Core Identity統合方針確認済み"
    echo "- F#↔C#認証情報変換方針確認済み"
    echo "上記を確認しましたか？ (y/n): "
    read -r phase_a1_confirmed
    if [ "$phase_a1_confirmed" != "y" ]; then
        echo "Phase A1特有の確認事項を確認してから再度実行してください。"
        exit 1
    fi
fi

# 9. 完了メッセージ
echo ""
echo "=========================================="
echo "Step開始時チェックリスト完了"
echo "=========================================="
echo ""
echo "✅ ADR_013（組織管理サイクル運用規則）確認完了"
echo "✅ 前Step組織レビュー結果確認完了"
echo "✅ 現Step組織設計妥当性確認完了"
echo "✅ Step終了時必須プロセス再確認完了"
echo "✅ 実装フェーズ特有確認事項完了"
echo "✅ 関連ファイル存在確認完了"
if [ "$PHASE_NAME" = "A1" ]; then
    echo "✅ Phase A1特有確認事項完了"
fi
echo ""
echo "Phase ${PHASE_NAME} Step ${STEP_NUMBER} の作業を開始してください。"
echo ""

# 10. Step作業開始時のリマインダー
echo "【Step作業開始時のリマインダー】"
echo "- Step終了時には step_completion_hook.sh を実行してください"
echo "- 問題が発生した場合は組織レビューチェックリストを参照してください"
echo "- 品質基準（0 Warning, 0 Error）を常に維持してください"
echo ""