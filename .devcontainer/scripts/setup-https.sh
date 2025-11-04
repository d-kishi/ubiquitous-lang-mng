#!/bin/bash
set -e

CERT_PATH="/home/vscode/.aspnet/https/aspnetapp.pfx"

echo "=================================================="
echo "🔐 HTTPS Certificate Setup for DevContainer"
echo "=================================================="
echo ""

if [ ! -f "$CERT_PATH" ]; then
  echo "⚠️  ERROR: HTTPS certificate not found!"
  echo ""
  echo "📝 Please run the following commands on your HOST machine (Windows):"
  echo ""
  echo "   mkdir -p \$USERPROFILE/.aspnet/https"
  echo "   dotnet dev-certs https --clean"
  echo "   dotnet dev-certs https -ep \$USERPROFILE/.aspnet/https/aspnetapp.pfx -p DevPassword123"
  echo "   dotnet dev-certs https --trust"
  echo ""
  echo "Then rebuild the DevContainer:"
  echo "   VS Code: Ctrl+Shift+P → 'Dev Containers: Rebuild Container'"
  echo ""
  exit 1
else
  echo "✅ HTTPS certificate found: $CERT_PATH"

  # 証明書情報表示（デバッグ用）
  echo "📋 Certificate details:"
  ls -lh "$CERT_PATH"

  echo ""
  echo "✅ HTTPS setup complete. You can now run the app with HTTPS support."
  echo "   - HTTPS: https://localhost:5001"
  echo "   - HTTP:  http://localhost:5000"
  echo ""
fi

echo "=================================================="
