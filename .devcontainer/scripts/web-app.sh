#!/bin/bash
set -e

# DevContainer Webアプリ起動/停止管理スクリプト
# Purpose: 開発中のUI確認のためのWebアプリ起動管理
# Usage: bash .devcontainer/scripts/web-app.sh {start|stop|restart|status}
#
# Examples:
#   bash .devcontainer/scripts/web-app.sh start    # ホットリロード有効で起動
#   bash .devcontainer/scripts/web-app.sh stop     # 停止
#   bash .devcontainer/scripts/web-app.sh restart  # 再起動
#   bash .devcontainer/scripts/web-app.sh status   # 状態確認

# 色定義
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# ヘルパー関数
error_exit() {
    echo -e "${RED}ERROR: $1${NC}" >&2
    exit 1
}

warning() {
    echo -e "${YELLOW}WARNING: $1${NC}"
}

success() {
    echo -e "${GREEN}$1${NC}"
}

info() {
    echo -e "${BLUE}$1${NC}"
}

# 設定
WEB_PROJECT="src/UbiquitousLanguageManager.Web"
LOG_FILE="/tmp/web-app.log"
PID_FILE="/tmp/web-app.pid"
PORT=5001
TIMEOUT=60

# 環境変数設定（docker exec経由でも動作するよう、未設定の場合のみ設定）
# これにより、VS Codeターミナル/docker exec両方で動作可能
export ASPNETCORE_ENVIRONMENT="${ASPNETCORE_ENVIRONMENT:-Development}"
export ASPNETCORE_URLS="${ASPNETCORE_URLS:-https://+:5001;http://+:5000}"
export DOTNET_USE_POLLING_FILE_WATCHER="${DOTNET_USE_POLLING_FILE_WATCHER:-1}"

# HTTPS証明書設定
export ASPNETCORE_Kestrel__Certificates__Default__Password="${ASPNETCORE_Kestrel__Certificates__Default__Password:-DevPassword123}"
export ASPNETCORE_Kestrel__Certificates__Default__Path="${ASPNETCORE_Kestrel__Certificates__Default__Path:-/home/vscode/.aspnet/https/aspnetapp.pfx}"

# PostgreSQL接続文字列（DevContainer内ではpostgresホスト名を使用）
export ConnectionStrings__DefaultConnection="${ConnectionStrings__DefaultConnection:-Host=postgres;Port=5432;Database=ubiquitous_lang_db;Username=ubiquitous_lang_user;Password=ubiquitous_lang_password}"

# 作業ディレクトリ
cd /workspace

# ポート5001が応答しているか確認（curlベース）
is_port_responding() {
    curl -k -s --connect-timeout 2 https://localhost:$PORT > /dev/null 2>&1
}

# dotnetプロセスのPIDを/procから取得
get_web_pid() {
    # /proc から UbiquitousLanguageManager.Web を実行中のdotnetプロセスを検索
    for pid_dir in /proc/[0-9]*; do
        if [ -f "$pid_dir/cmdline" ]; then
            if grep -q "UbiquitousLanguageManager.Web" "$pid_dir/cmdline" 2>/dev/null; then
                basename "$pid_dir"
                return 0
            fi
        fi
    done
    echo ""
}

# Webアプリが起動中か確認
is_running() {
    # ポート応答またはプロセス存在で判定
    if is_port_responding; then
        return 0  # 起動中（ポート応答あり）
    fi
    local pid=$(get_web_pid)
    if [ -n "$pid" ]; then
        return 0  # 起動中（プロセス存在）
    fi
    return 1  # 停止中
}

# 起動待機
wait_for_ready() {
    local elapsed=0
    echo "Waiting for application to be ready on port $PORT..."

    while ! curl -k -s https://localhost:$PORT > /dev/null 2>&1; do
        if [ $elapsed -ge $TIMEOUT ]; then
            echo ""
            warning "Timeout waiting for port $PORT (${TIMEOUT}s)"
            echo "Application logs (last 30 lines):"
            echo "========================================="
            tail -30 "$LOG_FILE" 2>/dev/null || echo "(no logs available)"
            return 1
        fi

        sleep 2
        elapsed=$((elapsed + 2))
        printf "."
    done

    echo ""
    return 0
}

# 停止待機
wait_for_stop() {
    local pid=$1
    local elapsed=0
    local max_wait=10

    while kill -0 "$pid" 2>/dev/null; do
        if [ $elapsed -ge $max_wait ]; then
            return 1  # タイムアウト
        fi
        sleep 1
        elapsed=$((elapsed + 1))
    done

    return 0
}

# === コマンド実装 ===

cmd_start() {
    echo "========================================="
    echo "Starting Web Application (Hot Reload)"
    echo "========================================="

    if is_running; then
        local pid=$(get_web_pid)
        warning "Web application is already running (PID: $pid)"
        echo "Use 'restart' to restart, or 'stop' to stop first."
        return 0
    fi

    info "Starting with dotnet watch run..."
    info "Hot Reload: ENABLED (DOTNET_USE_POLLING_FILE_WATCHER=1)"
    echo ""

    # dotnet watch runでホットリロード有効起動
    nohup dotnet watch run --project "$WEB_PROJECT" > "$LOG_FILE" 2>&1 &
    local new_pid=$!
    echo "$new_pid" > "$PID_FILE"

    echo "Process started (Initial PID: $new_pid)"
    echo "Log file: $LOG_FILE"
    echo ""

    # 起動待機
    if wait_for_ready; then
        success "Web Application is ready!"
        echo ""
        echo "  URL: https://localhost:$PORT"
        echo "  Log: tail -f $LOG_FILE"
        echo ""
        success "Hot Reload is active. Razor/CSS changes will auto-refresh."
        echo "For C#/F# changes, the app will auto-restart (Rude Edit)."
    else
        error_exit "Failed to start Web Application"
    fi
}

cmd_stop() {
    echo "========================================="
    echo "Stopping Web Application"
    echo "========================================="

    # 全関連プロセスのPIDを収集
    local pids=""
    for pid_dir in /proc/[0-9]*; do
        if [ -f "$pid_dir/cmdline" ]; then
            if grep -q "UbiquitousLanguageManager.Web" "$pid_dir/cmdline" 2>/dev/null; then
                local found_pid=$(basename "$pid_dir")
                pids="$pids $found_pid"
            fi
        fi
    done

    if [ -z "$pids" ]; then
        info "Web application is not running."
        rm -f "$PID_FILE"
        return 0
    fi

    echo "Found processes:$pids"
    echo "Sending SIGTERM..."

    # SIGTERM送信（graceful shutdown）
    for pid in $pids; do
        kill "$pid" 2>/dev/null || true
    done

    # 停止待機（最大10秒）
    local elapsed=0
    local max_wait=10
    local all_stopped=false

    while [ $elapsed -lt $max_wait ]; do
        sleep 1
        elapsed=$((elapsed + 1))

        # 残存プロセス確認
        local remaining=""
        for pid in $pids; do
            if [ -d "/proc/$pid" ]; then
                remaining="$remaining $pid"
            fi
        done

        if [ -z "$remaining" ]; then
            all_stopped=true
            break
        fi
    done

    if [ "$all_stopped" = true ]; then
        success "Web Application stopped gracefully."
    else
        warning "Some processes did not stop gracefully, sending SIGKILL..."
        for pid in $pids; do
            if [ -d "/proc/$pid" ]; then
                kill -9 "$pid" 2>/dev/null || true
            fi
        done
        sleep 1
        success "Web Application stopped (forced)."
    fi

    rm -f "$PID_FILE"
}

cmd_restart() {
    echo "========================================="
    echo "Restarting Web Application"
    echo "========================================="

    cmd_stop
    echo ""
    sleep 2
    cmd_start
}

cmd_status() {
    echo "========================================="
    echo "Web Application Status"
    echo "========================================="

    local pid=$(get_web_pid)

    if [ -n "$pid" ]; then
        success "Status: RUNNING"
        echo ""
        echo "  PID: $pid"
        echo "  URL: https://localhost:$PORT"
        echo "  Log: $LOG_FILE"
        echo ""

        # ポートの応答確認
        if curl -k -s https://localhost:$PORT > /dev/null 2>&1; then
            success "  Health: Responding"
        else
            warning "  Health: Not responding (may be starting up)"
        fi

        echo ""
        echo "Related processes:"
        # /procからdotnet関連プロセスを検索
        local found_any=false
        for pid_dir in /proc/[0-9]*; do
            if [ -f "$pid_dir/cmdline" ]; then
                local cmdline=$(cat "$pid_dir/cmdline" 2>/dev/null | tr '\0' ' ')
                if echo "$cmdline" | grep -qE "dotnet.*(watch|UbiquitousLanguageManager)"; then
                    local proc_pid=$(basename "$pid_dir")
                    echo "  PID $proc_pid: $cmdline"
                    found_any=true
                fi
            fi
        done
        if [ "$found_any" = false ]; then
            echo "  (none)"
        fi
    else
        info "Status: STOPPED"
        echo ""
        echo "  Use 'start' to start the application."
    fi
}

# === メイン処理 ===

case "${1:-}" in
    start)
        cmd_start
        ;;
    stop)
        cmd_stop
        ;;
    restart)
        cmd_restart
        ;;
    status)
        cmd_status
        ;;
    *)
        echo "Usage: $0 {start|stop|restart|status}"
        echo ""
        echo "Commands:"
        echo "  start   - Start with hot reload enabled (dotnet watch run)"
        echo "  stop    - Stop the application gracefully"
        echo "  restart - Stop and start the application"
        echo "  status  - Show current status"
        exit 1
        ;;
esac
