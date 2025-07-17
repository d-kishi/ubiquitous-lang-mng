#!/bin/bash

# Phase終了時のファイル管理 Hook
# 使用方法: ./phase_completion_hook.sh <phase_name>
# 例: ./phase_completion_hook.sh A1

PHASE_NAME=$1

if [ -z "$PHASE_NAME" ]; then
    echo "使用方法: $0 <phase_name>"
    echo "例: $0 A1"
    exit 1
fi

ACTIVE_DIR="Doc/08_Organization/Active"
COMPLETED_DIR="Doc/08_Organization/Completed"
ORGANIZATION_FILE="Phase_${PHASE_NAME}_Organization.md"
RESEARCH_DIR="Doc/05_Research/Phase_${PHASE_NAME}"

echo "=========================================="
echo "Phase ${PHASE_NAME} 終了時処理開始"
echo "=========================================="

# 1. 必要なディレクトリの存在確認
echo ""
echo "【1. ディレクトリ構造確認】"
if [ ! -d "$ACTIVE_DIR" ]; then
    echo "❌ Activeディレクトリが見つかりません: $ACTIVE_DIR"
    exit 1
fi

if [ ! -d "$COMPLETED_DIR" ]; then
    echo "Completedディレクトリが存在しないため作成します: $COMPLETED_DIR"
    mkdir -p "$COMPLETED_DIR"
fi

echo "✅ ディレクトリ構造確認完了"

# 2. 組織ファイルの存在確認
echo ""
echo "【2. 組織ファイル確認】"
ACTIVE_ORG_FILE="$ACTIVE_DIR/$ORGANIZATION_FILE"
COMPLETED_ORG_FILE="$COMPLETED_DIR/$ORGANIZATION_FILE"

if [ ! -f "$ACTIVE_ORG_FILE" ]; then
    echo "❌ Active配下に組織ファイルが見つかりません: $ACTIVE_ORG_FILE"
    exit 1
fi

echo "✅ 組織ファイル確認完了: $ACTIVE_ORG_FILE"

# 3. Step統合レビュー実施確認
echo ""
echo "【3. Step統合レビュー実施確認】"
echo "Phase ${PHASE_NAME} の全Step統合レビューを実施しましたか？ (y/n): "
echo "（各Stepの効果統合評価・Phase全体の目標達成度確認・品質確認）"
read -r integrated_review_done
if [ "$integrated_review_done" != "y" ]; then
    echo "Step統合レビューを実施してから再度実行してください。"
    exit 1
fi

# 4. 統合レビュー結果の記録確認
echo ""
echo "【4. 統合レビュー結果記録確認】"
echo "統合レビュー結果を組織ファイルに記録しましたか？ (y/n): "
read -r review_recorded
if [ "$review_recorded" != "y" ]; then
    echo "統合レビュー結果を記録してから再度実行してください。"
    exit 1
fi

# 5. Phase完了判定
echo ""
echo "【5. Phase完了判定】"
echo "Phase ${PHASE_NAME} の完了判定を行います:"
echo "- 全Step完了確認済み"
echo "- 品質基準達成確認済み"
echo "- 成果物完成確認済み"
echo "- 統合テスト完了確認済み"
echo ""
echo "Phase ${PHASE_NAME} を完了済みとしてマークしますか？ (y/n): "
read -r phase_completed
if [ "$phase_completed" != "y" ]; then
    echo "Phase完了判定を完了してから再度実行してください。"
    exit 1
fi

# 6. 既存ファイルの重複確認
echo ""
echo "【6. ファイル移動準備】"
if [ -f "$COMPLETED_ORG_FILE" ]; then
    echo "⚠️  Completed配下に同名ファイルが既に存在します: $COMPLETED_ORG_FILE"
    echo "上書きしますか？ (y/n): "
    read -r overwrite_confirmed
    if [ "$overwrite_confirmed" != "y" ]; then
        echo "ファイル移動をキャンセルしました。"
        exit 1
    fi
fi

# 7. ファイル移動実行
echo ""
echo "【7. ファイル移動実行】"
echo "組織ファイルを移動しています..."
echo "移動元: $ACTIVE_ORG_FILE"
echo "移動先: $COMPLETED_ORG_FILE"

if mv "$ACTIVE_ORG_FILE" "$COMPLETED_ORG_FILE"; then
    echo "✅ ファイル移動完了"
else
    echo "❌ ファイル移動に失敗しました"
    exit 1
fi

# 8. 移動後の確認
echo ""
echo "【8. 移動後確認】"
if [ -f "$COMPLETED_ORG_FILE" ]; then
    echo "✅ 移動先でファイル確認完了: $COMPLETED_ORG_FILE"
else
    echo "❌ 移動先でファイルが見つかりません"
    exit 1
fi

if [ -f "$ACTIVE_ORG_FILE" ]; then
    echo "❌ 移動元にファイルが残っています: $ACTIVE_ORG_FILE"
    exit 1
else
    echo "✅ 移動元からファイルが削除されました"
fi

# 9. 研究結果ファイルの確認
echo ""
echo "【9. 研究結果ファイル確認】"
if [ -d "$RESEARCH_DIR" ]; then
    echo "✅ Phase ${PHASE_NAME} の研究結果ディレクトリ: $RESEARCH_DIR"
    echo "研究結果ファイルは参照用として保持されます。"
    
    # 研究結果ファイルの概要表示
    echo ""
    echo "研究結果ファイル一覧:"
    ls -la "$RESEARCH_DIR"
else
    echo "⚠️  研究結果ディレクトリが見つかりません: $RESEARCH_DIR"
    echo "Phase ${PHASE_NAME} でStep1分析結果記録が実施されていない可能性があります。"
fi

# 10. 知識蓄積のためのサマリー生成
echo ""
echo "【10. 知識蓄積サマリー生成】"
echo "Phase ${PHASE_NAME} の知識蓄積サマリーを生成しました:"
echo ""
echo "================================================"
echo "Phase ${PHASE_NAME} 完了サマリー"
echo "================================================"
echo ""
echo "【Phase情報】"
echo "- Phase名: ${PHASE_NAME}"
echo "- 完了日: $(date '+%Y-%m-%d %H:%M:%S')"
echo "- 組織ファイル: $COMPLETED_ORG_FILE"
echo "- 研究結果: $RESEARCH_DIR"
echo ""
echo "【次Phase計画時の参考事項】"
echo "- 組織構成の効果測定結果を参考にしてください"
echo "- 技術的知見は研究結果ファイルを参照してください"
echo "- 品質基準と達成方法は組織ファイルを参考にしてください"
echo "================================================"
echo ""

# 11. 次Phase準備の案内
echo ""
echo "【11. 次Phase準備】"
echo "Phase ${PHASE_NAME} の次Phaseを開始する際は、以下を実施してください:"
echo "1. 完了したPhaseの知識蓄積結果を参照"
echo "2. 新しいPhaseの組織設計を実施"
echo "3. phase_start_hook.sh を実行（作成予定）"
echo ""

# 12. 完了メッセージ
echo ""
echo "=========================================="
echo "Phase ${PHASE_NAME} 終了時処理完了"
echo "=========================================="
echo ""
echo "✅ Step統合レビュー実施確認完了"
echo "✅ 統合レビュー結果記録完了"
echo "✅ Phase完了判定完了"
echo "✅ 組織ファイル移動完了 (Active → Completed)"
echo "✅ 研究結果ファイル確認完了"
echo "✅ 知識蓄積サマリー生成完了"
echo ""
echo "Phase ${PHASE_NAME} は正常に完了しました。"
echo "次Phaseの計画時に、完了したPhaseの知識蓄積結果を活用してください。"
echo ""