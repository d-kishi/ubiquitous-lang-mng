#!/bin/bash

# セッション終了時確認プロセス Hook
# 使用方法: ./session_end_hook.sh
# 実行タイミング: セッション目的達成時

echo "=========================================="
echo "セッション終了時確認プロセス"
echo "実行日時: $(date '+%Y-%m-%d %H:%M:%S')"
echo "=========================================="

# 1. 目的達成確認
echo ""
echo "【1. 目的達成確認】"
echo "セッション開始時に設定した目的が達成されたかClaudeが判断・報告してください。"
echo ""
echo "セッション目的の達成状況を確認しましたか？ (y/n): "
read -r purpose_achieved_check
if [ "$purpose_achieved_check" != "y" ]; then
    echo "セッション目的の達成状況を確認してから再度実行してください。"
    exit 1
fi

echo "セッション目的は達成されましたか？ (y/n): "
read -r purpose_achieved
if [ "$purpose_achieved" != "y" ]; then
    echo ""
    echo "セッション目的が未達成の場合："
    echo "1. 継続して作業を行う"
    echo "2. 別のアプローチを検討する"
    echo "3. 次回セッションに持ち越す"
    echo ""
    echo "どのように対応しますか？ (continue/reconsider/postpone): "
    read -r approach
    case $approach in
        "continue")
            echo "作業を継続してください。完了後に再度このHookを実行してください。"
            exit 0
            ;;
        "reconsider")
            echo "アプローチを再検討してください。検討完了後に再度このHookを実行してください。"
            exit 0
            ;;
        "postpone")
            echo "次回セッションに持ち越しとして記録します。"
            ;;
        *)
            echo "無効な選択です。continue/reconsider/postpone のいずれかを選択してください。"
            exit 1
            ;;
    esac
fi

# 2. 終了確認
echo ""
echo "【2. 終了確認】"
echo "目的達成をユーザーに報告し、セッション終了するかどうか確認を取ってください。"
echo ""
echo "================================================"
echo "セッション終了確認"
echo "================================================"
echo ""
echo "今回のセッションの目的が達成されました。"
echo ""
echo "主な成果:"
echo "- [ここに具体的な成果を記載してください]"
echo "- [完了した作業項目を記載してください]"
echo "- [作成・修正したファイルを記載してください]"
echo ""
echo "このセッションを終了しますか？"
echo "継続する場合は新たな目的を設定いたします。"
echo "================================================"
echo ""
echo "上記の内容でユーザーに終了確認を取りましたか？ (y/n): "
read -r end_confirmation_sent
if [ "$end_confirmation_sent" != "y" ]; then
    echo "ユーザーに終了確認を取ってから再度実行してください。"
    exit 1
fi

echo "ユーザーはセッション終了を選択しましたか？ (y/n): "
read -r user_wants_to_end
if [ "$user_wants_to_end" != "y" ]; then
    # 継続判断
    echo ""
    echo "【3. 継続判断】"
    echo "ユーザーが継続を希望する場合は新たな目的設定を依頼してください。"
    echo ""
    echo "新たな目的設定を依頼しましたか？ (y/n): "
    read -r new_purpose_requested
    if [ "$new_purpose_requested" != "y" ]; then
        echo "新たな目的設定を依頼してから再度実行してください。"
        exit 1
    fi
    
    echo "新たな目的が設定されましたか？ (y/n): "
    read -r new_purpose_set
    if [ "$new_purpose_set" != "y" ]; then
        echo "新たな目的が設定されるまでお待ちください。"
        exit 1
    fi
    
    echo ""
    echo "新たな目的でセッションを継続します。"
    echo "必要に応じて dynamic_loading_hook.sh を再実行してください。"
    exit 0
fi

# 4. プロジェクト状況更新
echo ""
echo "【4. プロジェクト状況更新】"
echo "Doc/プロジェクト状況.md の以下の項目を更新してください:"
echo ""
echo "□ 次回セッション最優先事項"
echo "□ 次回Session読み込み推奨範囲"
echo "□ 今回完了した作業項目"
echo "□ 継続課題・未完了事項"
echo ""
echo "プロジェクト状況.md の更新が完了しましたか？ (y/n): "
read -r project_status_updated
if [ "$project_status_updated" != "y" ]; then
    echo "プロジェクト状況.md を更新してから再度実行してください。"
    exit 1
fi

# 5. セッション終了時記録の実行確認
echo ""
echo "【5. セッション終了時記録実行確認】"
echo "session_record_hook.sh を実行してセッション終了時記録を作成しますか？ (y/n): "
read -r execute_record_hook
if [ "$execute_record_hook" = "y" ]; then
    echo ""
    echo "session_record_hook.sh を実行してください。"
    echo "完了後、このプロセスを再開してください。"
    echo ""
    echo "session_record_hook.sh の実行が完了しましたか？ (y/n): "
    read -r record_hook_completed
    if [ "$record_hook_completed" != "y" ]; then
        echo "session_record_hook.sh を完了してから再度実行してください。"
        exit 1
    fi
else
    echo "⚠️  記録作成をスキップしました。後で必要に応じて実行してください。"
fi

# 6. 完了メッセージ
echo ""
echo "=========================================="
echo "セッション終了時確認プロセス完了"
echo "=========================================="
echo ""
echo "✅ 目的達成確認完了"
echo "✅ ユーザー終了確認完了"
echo "✅ プロジェクト状況更新完了"
if [ "$execute_record_hook" = "y" ]; then
    echo "✅ セッション終了時記録作成完了"
fi
echo ""
echo "セッション終了時確認プロセスが完了しました。"
echo ""

# 7. 次回セッション準備の確認
echo "【7. 次回セッション準備】"
echo "次回セッション開始時に session_start_hook.sh を実行してください。"
echo ""
echo "次回セッションの準備状況:"
echo "- プロジェクト状況.md 更新済み"
echo "- 継続課題・未完了事項 記録済み"
echo "- 次回最優先事項 設定済み"
echo ""

# 8. 重要な注意事項
echo "【重要な注意事項】"
echo "- AutoCompact対策: 重要な情報はファイルに記録済み"
echo "- 品質保証: 最終的な品質確認を実施してください"
echo "- 継続性確保: 次回セッションでの円滑な再開が可能"
echo ""

# 9. セッション終了ログ
echo "【セッション終了ログ】"
echo "セッション終了時刻: $(date '+%Y-%m-%d %H:%M:%S')"
echo "セッション終了理由: 目的達成"
echo "プロジェクト状況更新: 完了"
echo "次回セッション準備: 完了"
echo ""
echo "セッション終了処理が正常に完了しました。"
echo ""