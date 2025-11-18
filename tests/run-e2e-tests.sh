#!/bin/bash
set -e

# E2Eテスト自動実行スクリプト
# Purpose: Webアプリ起動 → E2Eテスト実行 → Webアプリ停止を自動化
# Usage: bash tests/run-e2e-tests.sh [test-filter]
# Example: bash tests/run-e2e-tests.sh AuthenticationTests

echo "========================================="
echo "E2E Test Automation Script"
echo "========================================="

# 作業ディレクトリ移動
cd /workspace

# テストフィルタ（引数で指定可能、デフォルトは全テスト実行）
# 使用例: bash tests/run-e2e-tests.sh authentication.spec.ts
# 使用例: bash tests/run-e2e-tests.sh user-projects.spec.ts
TEST_FILTER="${1:-}"

echo ""
echo "Step 1: Starting Web Application..."
echo "========================================="

# Webアプリケーションをバックグラウンドで起動
dotnet run --project src/UbiquitousLanguageManager.Web > /tmp/web-app.log 2>&1 &
WEB_PID=$!

echo "Web Application started (PID: $WEB_PID)"

echo ""
echo "Step 2: Waiting for application to be ready..."
echo "========================================="

# ポート5001がリスニング状態になるまで待機（最大60秒）
TIMEOUT=60
ELAPSED=0

while ! curl -k -s https://localhost:5001 > /dev/null 2>&1; do
    if [ $ELAPSED -ge $TIMEOUT ]; then
        echo "ERROR: Timeout waiting for port 5001"
        echo "Web Application logs:"
        cat /tmp/web-app.log
        kill $WEB_PID 2>/dev/null || true
        exit 1
    fi

    sleep 2
    ELAPSED=$((ELAPSED + 2))
    echo "Waiting... ($ELAPSED/$TIMEOUT seconds)"
done

echo "Application ready on port 5001!"

# 追加待機（アプリケーション完全初期化）
echo "Additional wait for full initialization..."
sleep 5

echo ""
echo "Step 3: Running E2E Tests..."
echo "========================================="

# E2Eテスト実行（TypeScript/Playwright）
TEST_EXIT_CODE=0
cd tests/UbiquitousLanguageManager.E2E.Tests

if [ -z "$TEST_FILTER" ]; then
    # 全テスト実行
    npx playwright test || TEST_EXIT_CODE=$?
else
    # 特定テストファイル実行
    npx playwright test "$TEST_FILTER" || TEST_EXIT_CODE=$?
fi

cd /workspace

echo ""
echo "Step 4: Stopping Web Application..."
echo "========================================="

# Webアプリケーション停止
kill $WEB_PID 2>/dev/null || true
wait $WEB_PID 2>/dev/null || true

echo "Web Application stopped (PID: $WEB_PID)"

echo ""
echo "========================================="
echo "E2E Test Automation Complete"
echo "========================================="
echo "Test Exit Code: $TEST_EXIT_CODE"

# テスト結果に応じた終了コード
exit $TEST_EXIT_CODE
