#!/bin/bash

# Step終了時の必須プロセス Hook
# 使用方法: ./step_completion_hook.sh <phase_name> <step_number>
# 例: ./step_completion_hook.sh A1 1

PHASE_NAME=$1
STEP_NUMBER=$2

if [ -z "$PHASE_NAME" ] || [ -z "$STEP_NUMBER" ]; then
    echo "使用方法: $0 <phase_name> <step_number>"
    echo "例: $0 A1 1"
    exit 1
fi

ORGANIZATION_FILE="Doc/08_Organization/Active/Phase_${PHASE_NAME}_Organization.md"
CHECKLIST_FILE="Doc/08_Organization/組織レビューチェックリスト.md"

echo "=========================================="
echo "Step ${STEP_NUMBER} 終了時プロセス開始"
echo "Phase: ${PHASE_NAME}"
echo "=========================================="

# 1. 組織レビューチェックリスト実施確認
echo ""
echo "【1. 組織レビューチェックリスト実施】"
echo "以下のチェックリストを実施してください:"
echo "- チェックリストファイル: ${CHECKLIST_FILE}"
echo ""
echo "チェックリストを実施しましたか？ (y/n): "
read -r checklist_done
if [ "$checklist_done" != "y" ]; then
    echo "チェックリストを実施してから再度実行してください。"
    exit 1
fi

# 2. 組織ファイルの存在確認
if [ ! -f "$ORGANIZATION_FILE" ]; then
    echo "エラー: 組織ファイルが見つかりません: $ORGANIZATION_FILE"
    exit 1
fi

# 3. レビュー結果の記録確認
echo ""
echo "【2. Step ${STEP_NUMBER} レビュー結果記録】"
echo "組織ファイルにStep ${STEP_NUMBER}のレビュー結果を記録しましたか？ (y/n): "
read -r review_recorded
if [ "$review_recorded" != "y" ]; then
    echo "レビュー結果を記録してから再度実行してください。"
    exit 1
fi

# 4. ユーザー確認プロンプト生成
echo ""
echo "【3. ユーザー確認依頼】"
echo "以下の内容をユーザーに確認してください:"
echo ""
echo "================================================"
echo "Step ${STEP_NUMBER} 完了レビュー結果確認"
echo "================================================"
echo ""
echo "Phase ${PHASE_NAME} Step ${STEP_NUMBER} が完了しました。"
echo "組織レビューチェックリストを実施し、結果を記録しました。"
echo ""
echo "レビュー結果の詳細は以下をご確認ください:"
echo "- 組織ファイル: ${ORGANIZATION_FILE}"
echo "- チェックリスト: ${CHECKLIST_FILE}"
echo ""
echo "このStep ${STEP_NUMBER}の完了を承認し、次のStep ${STEP_NUMBER}+1の組織設計に進みますか？"
echo ""
echo "承認後、次Step組織設計を実施いたします。"
echo "================================================"
echo ""

echo "上記の内容でユーザーに確認しましたか？ (y/n): "
read -r user_confirmed
if [ "$user_confirmed" != "y" ]; then
    echo "ユーザー確認後に再度実行してください。"
    exit 1
fi

# 5. 次Step組織設計実施確認
echo ""
echo "【4. 次Step組織設計実施】"
echo "ユーザーの承認を得て、次Step ${STEP_NUMBER}+1の組織設計を実施しましたか？ (y/n): "
read -r next_org_designed
if [ "$next_org_designed" != "y" ]; then
    echo "次Step組織設計を実施してから再度実行してください。"
    exit 1
fi

# 6. 組織設計結果のユーザー確認
echo ""
echo "【5. 組織設計結果ユーザー確認】"
echo "以下の内容をユーザーに確認してください:"
echo ""
echo "================================================"
echo "Step ${STEP_NUMBER}+1 組織設計結果確認"
echo "================================================"
echo ""
echo "Phase ${PHASE_NAME} Step $((STEP_NUMBER + 1)) の組織設計が完了しました。"
echo ""
echo "組織設計結果の詳細は以下をご確認ください:"
echo "- 組織ファイル: ${ORGANIZATION_FILE}"
echo ""
echo "この組織設計を承認し、Step $((STEP_NUMBER + 1)) の作業着手を開始しますか？"
echo ""
echo "承認後、Step $((STEP_NUMBER + 1)) の作業を開始いたします。"
echo "================================================"
echo ""

echo "上記の内容でユーザーに確認しましたか？ (y/n): "
read -r org_design_confirmed
if [ "$org_design_confirmed" != "y" ]; then
    echo "ユーザー確認後に再度実行してください。"
    exit 1
fi

# 7. 組織ファイル更新
echo ""
echo "【6. 組織ファイル更新】"
echo "組織ファイルにStep $((STEP_NUMBER + 1)) の組織設計結果を更新しましたか？ (y/n): "
read -r org_file_updated
if [ "$org_file_updated" != "y" ]; then
    echo "組織ファイルを更新してから再度実行してください。"
    exit 1
fi

# 8. 完了メッセージ
echo ""
echo "=========================================="
echo "Step ${STEP_NUMBER} 終了時プロセス完了"
echo "=========================================="
echo ""
echo "✅ Step ${STEP_NUMBER} 組織レビュー完了"
echo "✅ ユーザー確認・承認完了"
echo "✅ Step $((STEP_NUMBER + 1)) 組織設計完了"
echo "✅ 組織設計ユーザー確認・承認完了"
echo "✅ 組織ファイル更新完了"
echo ""
echo "Step $((STEP_NUMBER + 1)) の作業を開始してください。"
echo ""