#!/bin/bash

# 文脈復元時の標準プロセス Hook
# 使用方法: ./context_restore_hook.sh <phase_name> <step_number>
# 例: ./context_restore_hook.sh A1 2

PHASE_NAME=$1
STEP_NUMBER=$2

if [ -z "$PHASE_NAME" ] || [ -z "$STEP_NUMBER" ]; then
    echo "使用方法: $0 <phase_name> <step_number>"
    echo "例: $0 A1 2"
    exit 1
fi

RESEARCH_DIR="Doc/05_Research/Phase_${PHASE_NAME}"
ORGANIZATION_FILE="Doc/08_Organization/Active/Phase_${PHASE_NAME}_Organization.md"

echo "=========================================="
echo "文脈復元プロセス開始"
echo "Phase: ${PHASE_NAME}, Step: ${STEP_NUMBER}"
echo "=========================================="

# 1. 研究結果ディレクトリの存在確認
echo ""
echo "【1. 研究結果ディレクトリ確認】"
if [ ! -d "$RESEARCH_DIR" ]; then
    echo "❌ 研究結果ディレクトリが見つかりません: $RESEARCH_DIR"
    echo "Phase ${PHASE_NAME} のStep1分析結果記録が実施されていない可能性があります。"
    echo "Step1から実施する必要があります。"
    exit 1
fi

echo "✅ 研究結果ディレクトリ確認完了: $RESEARCH_DIR"

# 2. 必須ファイルの存在確認
echo ""
echo "【2. 必須ファイル存在確認】"
required_files=(
    "$RESEARCH_DIR/Step1_Analysis_Results.md"
    "$RESEARCH_DIR/Database_Design_Review.md"
)

missing_files=()
for file in "${required_files[@]}"; do
    if [ -f "$file" ]; then
        echo "✅ $file"
    else
        echo "❌ $file が見つかりません"
        missing_files+=("$file")
    fi
done

if [ ${#missing_files[@]} -gt 0 ]; then
    echo ""
    echo "必須ファイルが不足しています："
    for file in "${missing_files[@]}"; do
        echo "  - $file"
    done
    echo "Step1分析結果記録が不完全な可能性があります。"
    exit 1
fi

# 3. 研究結果ファイル一覧表示
echo ""
echo "【3. 研究結果ファイル一覧】"
echo "Phase ${PHASE_NAME} の研究結果ファイル："
ls -la "$RESEARCH_DIR"
echo ""

# 4. データベース設計書確認結果の確認
echo ""
echo "【4. データベース設計書確認結果確認】"
DB_REVIEW_FILE="$RESEARCH_DIR/Database_Design_Review.md"
echo "データベース設計書確認結果を確認しています..."
echo "ファイル: $DB_REVIEW_FILE"
echo ""

if [ -f "$DB_REVIEW_FILE" ]; then
    echo "データベース設計書確認結果の概要："
    echo "----------------------------------------"
    # ファイルの最初の10行を表示
    head -n 10 "$DB_REVIEW_FILE"
    echo "----------------------------------------"
    echo ""
    echo "データベース設計書確認結果を確認しましたか？ (y/n): "
    read -r db_review_confirmed
    if [ "$db_review_confirmed" != "y" ]; then
        echo "データベース設計書確認結果を確認してから再度実行してください。"
        exit 1
    fi
else
    echo "❌ データベース設計書確認結果が見つかりません"
    exit 1
fi

# 5. 各チーム分析結果の確認
echo ""
echo "【5. 各チーム分析結果確認】"
echo "各チーム分析結果を確認しています..."

# チーム調査ファイルを自動検出
team_files=($(find "$RESEARCH_DIR" -name "*_Research.md" -type f))

if [ ${#team_files[@]} -eq 0 ]; then
    echo "❌ チーム分析結果ファイルが見つかりません"
    exit 1
fi

echo "検出されたチーム分析結果ファイル："
for file in "${team_files[@]}"; do
    filename=$(basename "$file")
    team_name=$(echo "$filename" | sed 's/_Research.md$//')
    echo "  - $team_name: $file"
done
echo ""

echo "各チーム分析結果を確認しましたか？ (y/n): "
read -r team_results_confirmed
if [ "$team_results_confirmed" != "y" ]; then
    echo "各チーム分析結果を確認してから再度実行してください。"
    exit 1
fi

# 6. 統合分析結果の確認
echo ""
echo "【6. 統合分析結果確認】"
ANALYSIS_FILE="$RESEARCH_DIR/Step1_Analysis_Results.md"
echo "統合分析結果を確認しています..."
echo "ファイル: $ANALYSIS_FILE"
echo ""

if [ -f "$ANALYSIS_FILE" ]; then
    echo "統合分析結果の概要："
    echo "----------------------------------------"
    # ファイルの最初の15行を表示
    head -n 15 "$ANALYSIS_FILE"
    echo "----------------------------------------"
    echo ""
    echo "統合分析結果を確認しましたか？ (y/n): "
    read -r analysis_confirmed
    if [ "$analysis_confirmed" != "y" ]; then
        echo "統合分析結果を確認してから再度実行してください。"
        exit 1
    fi
else
    echo "❌ 統合分析結果が見つかりません"
    exit 1
fi

# 7. 計画詳細化結果の確認
echo ""
echo "【7. 計画詳細化結果確認】"
echo "Step2以降の具体的作業計画を確認しています..."
echo ""

# 統合分析結果から計画部分を抽出表示
if grep -q "Step2以降" "$ANALYSIS_FILE"; then
    echo "Step2以降の作業計画："
    echo "----------------------------------------"
    grep -A 10 "Step2以降" "$ANALYSIS_FILE"
    echo "----------------------------------------"
else
    echo "⚠️  Step2以降の作業計画が見つかりません"
fi
echo ""

echo "計画詳細化結果を確認しましたか？ (y/n): "
read -r plan_confirmed
if [ "$plan_confirmed" != "y" ]; then
    echo "計画詳細化結果を確認してから再度実行してください。"
    exit 1
fi

# 8. 組織設計の妥当性確認
echo ""
echo "【8. 組織設計妥当性確認】"
if [ -f "$ORGANIZATION_FILE" ]; then
    echo "組織設計ファイル: $ORGANIZATION_FILE"
    echo "現在の組織設計とStep1分析結果の整合性を確認しましたか？ (y/n): "
    read -r org_consistency_confirmed
    if [ "$org_consistency_confirmed" != "y" ]; then
        echo "組織設計と分析結果の整合性を確認してから再度実行してください。"
        exit 1
    fi
else
    echo "❌ 組織設計ファイルが見つかりません: $ORGANIZATION_FILE"
    exit 1
fi

# 9. 技術的前提条件の確認
echo ""
echo "【9. 技術的前提条件確認】"
echo "以下の技術的前提条件を確認してください："
echo "- 開発環境の準備状況"
echo "- 依存関係の解決状況"
echo "- ビルド環境の正常性"
echo "- PostgreSQL接続設定の確認"
echo ""
echo "技術的前提条件を確認しましたか？ (y/n): "
read -r tech_prereq_confirmed
if [ "$tech_prereq_confirmed" != "y" ]; then
    echo "技術的前提条件を確認してから再度実行してください。"
    exit 1
fi

# 10. 品質保証の確認
echo ""
echo "【10. 品質保証確認】"
echo "文脈復元の品質保証を確認しています："
echo ""

# 品質保証チェックリスト
quality_checks=(
    "Step1記録内容の完全性"
    "データベース設計書確認結果の妥当性"
    "各チーム分析結果の技術的妥当性"
    "統合分析結果の実装可能性"
    "計画詳細化結果の実現性"
)

echo "品質保証チェックリスト："
for check in "${quality_checks[@]}"; do
    echo "  ✓ $check"
done
echo ""

echo "上記の品質保証項目を確認しましたか？ (y/n): "
read -r quality_confirmed
if [ "$quality_confirmed" != "y" ]; then
    echo "品質保証項目を確認してから再度実行してください。"
    exit 1
fi

# 11. 完了メッセージと次ステップ案内
echo ""
echo "=========================================="
echo "文脈復元プロセス完了"
echo "=========================================="
echo ""
echo "✅ 研究結果ディレクトリ確認完了"
echo "✅ 必須ファイル存在確認完了"
echo "✅ データベース設計書確認結果確認完了"
echo "✅ 各チーム分析結果確認完了"
echo "✅ 統合分析結果確認完了"
echo "✅ 計画詳細化結果確認完了"
echo "✅ 組織設計妥当性確認完了"
echo "✅ 技術的前提条件確認完了"
echo "✅ 品質保証確認完了"
echo ""
echo "文脈復元が正常に完了しました。"
echo "Phase ${PHASE_NAME} Step ${STEP_NUMBER} の作業を開始してください。"
echo ""

# 12. 作業開始時の重要事項リマインダー
echo "【作業開始時の重要事項】"
echo "1. 確認した計画詳細化結果に従って作業を進めてください"
echo "2. 品質基準（0 Warning, 0 Error）を常に維持してください"
echo "3. 不明点が発生した場合は研究結果ファイルを参照してください"
echo "4. Step完了時には step_completion_hook.sh を実行してください"
echo ""

# 13. 参照先情報
echo "【参照先情報】"
echo "- 研究結果ディレクトリ: $RESEARCH_DIR"
echo "- 組織設計ファイル: $ORGANIZATION_FILE"
echo "- 統合分析結果: $ANALYSIS_FILE"
echo "- データベース設計書確認結果: $DB_REVIEW_FILE"
echo ""