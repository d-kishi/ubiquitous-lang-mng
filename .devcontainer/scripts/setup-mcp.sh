#!/bin/bash
set -e

echo "=================================================="
echo "🔌 MCP Server Setup for Claude Code CLI"
echo "=================================================="
echo ""

MCP_CONFIG_FILE="/workspace/.mcp.json"
SERENA_PROJECT_CONFIG="/workspace/.serena/project.yml"
SERENA_USER_CONFIG="$HOME/.serena/serena_config.yml"

# カラー定義
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

# エラーハンドリング
error_exit() {
  echo -e "${RED}❌ ERROR: $1${NC}" >&2
  exit 1
}

warning() {
  echo -e "${YELLOW}⚠️  WARNING: $1${NC}"
}

success() {
  echo -e "${GREEN}✅ $1${NC}"
}

# 1. .mcp.json存在確認
echo "📋 Checking MCP configuration file..."
if [ -f "$MCP_CONFIG_FILE" ]; then
  success "MCP configuration found: $MCP_CONFIG_FILE"

  # JSON構文チェック（jqがインストールされている場合）
  if command -v jq &> /dev/null; then
    if jq empty "$MCP_CONFIG_FILE" 2>/dev/null; then
      success "JSON syntax is valid"
    else
      error_exit "Invalid JSON syntax in $MCP_CONFIG_FILE"
    fi
  fi

  # 設定内容表示（デバッグ用）
  echo ""
  echo "📄 MCP Server Configuration:"
  cat "$MCP_CONFIG_FILE"
  echo ""
else
  error_exit "MCP configuration file not found: $MCP_CONFIG_FILE"
fi

# 2. uvコマンド確認・インストール（Serena用）
echo "🐍 Checking uv (Python package manager)..."
if command -v uvx &> /dev/null; then
  success "uv is installed"
  uvx --version
else
  warning "uv is not installed. Installing..."
  curl -LsSf https://astral.sh/uv/install.sh | sh || error_exit "Failed to install uv"
  export PATH="$HOME/.local/bin:$PATH"

  # .bashrcに追加（永続化）
  if ! grep -q 'export PATH="$HOME/.local/bin:$PATH"' ~/.bashrc; then
    echo 'export PATH="$HOME/.local/bin:$PATH"' >> ~/.bashrc
  fi

  success "uv installed successfully"
  uvx --version
fi

# 3. npxコマンド確認（Playwright用）
echo ""
echo "📦 Checking npx (Node.js package runner)..."
if command -v npx &> /dev/null; then
  success "npx is installed"
  npx --version
else
  error_exit "npx is not installed. Please install Node.js."
fi

# 4. Serenaユーザー設定ファイル作成
echo ""
echo "📝 Configuring Serena user settings..."
mkdir -p "$HOME/.serena"

if [ ! -f "$SERENA_USER_CONFIG" ]; then
  cat > "$SERENA_USER_CONFIG" <<EOF
# Serenaユーザー設定
record_tool_usage_stats: true
included_optional_tools: []
EOF
  success "Serena user configuration created: $SERENA_USER_CONFIG"
else
  success "Serena user configuration already exists"
fi

# 5. Serenaプロジェクト設定ファイル作成
echo ""
echo "📝 Configuring Serena project settings..."
mkdir -p /workspace/.serena

if [ ! -f "$SERENA_PROJECT_CONFIG" ]; then
  cat > "$SERENA_PROJECT_CONFIG" <<EOF
# Serenaプロジェクト設定
read_only: true
project_name: ubiquitous-lang-mng

# 除外パターン（必要に応じて追加）
exclude_patterns:
  - "*.log"
  - "node_modules/**"
  - "bin/**"
  - "obj/**"
  - ".git/**"
EOF
  success "Serena project configuration created: $SERENA_PROJECT_CONFIG"
else
  success "Serena project configuration already exists"
fi

# 6. MCP Serverテスト接続（オプション）
echo ""
echo "🔌 Testing MCP server connections..."

# Serenaテスト
echo "  Testing Serena MCP..."
if timeout 5s uvx --from git+https://github.com/oraios/serena serena-mcp-server --help &>/dev/null; then
  success "Serena MCP server is accessible"
else
  warning "Serena MCP server test timed out (this is normal on first run)"
fi

# Playwrightテスト
echo "  Testing Playwright MCP..."
if npx -y @playwright/mcp@latest --help &>/dev/null; then
  success "Playwright MCP server is accessible"
else
  warning "Playwright MCP server test failed"
fi

# 7. セットアップ完了
echo ""
echo "=================================================="
success "MCP Server Setup Complete!"
echo ""
echo "📌 Available MCP Servers:"
echo "   - Serena: Semantic code retrieval and editing"
echo "   - Playwright: Browser automation"
echo ""
echo "🚀 To start Claude Code CLI:"
echo "   cd /workspace && claude"
echo ""
echo "📚 Useful commands:"
echo "   /mcp          - List available MCP servers"
echo "   /mcp serena   - Test Serena connection"
echo "   /mcp playwright - Test Playwright connection"
echo ""
echo "=================================================="
