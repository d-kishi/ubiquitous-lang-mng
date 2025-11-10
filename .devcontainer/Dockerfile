# ベースイメージ: .NET 8.0 SDK
FROM mcr.microsoft.com/dotnet/sdk:8.0

# 非rootユーザー作成（セキュリティベストプラクティス）
ARG USERNAME=vscode
ARG USER_UID=1000
ARG USER_GID=$USER_UID

# パッケージ更新・基本ツールインストール
# 注: F#は.NET 8.0 SDKに含まれているため、追加インストール不要
RUN apt-get update && export DEBIAN_FRONTEND=noninteractive \
    && apt-get -y install --no-install-recommends \
    # bubblewrap（Sandboxモード用）
    bubblewrap \
    # PostgreSQL client
    postgresql-client \
    # 開発ツール
    git \
    curl \
    wget \
    unzip \
    # ユーティリティ
    ca-certificates \
    gnupg \
    lsb-release

# Node.js 24.x（Active LTS）インストール（公式リポジトリから）
# ホスト環境（v24.10.0）に合わせて最新LTSを使用
RUN curl -fsSL https://deb.nodesource.com/setup_24.x | bash - \
    && apt-get install -y nodejs

# グローバルnpmパッケージインストール
RUN npm install -g \
    # Playwright（E2Eテスト）
    @playwright/test \
    playwright

# Claude Code CLIインストール（GitHub Codespaces統合）
ARG CLAUDE_CODE_VERSION=latest
RUN npm install -g @anthropic-ai/claude-code@${CLAUDE_CODE_VERSION}

# Playwrightブラウザインストール（オプション・E2Eテスト用）
RUN npx playwright install --with-deps chromium

# 非rootユーザー作成
RUN groupadd --gid $USER_GID $USERNAME \
    && useradd --uid $USER_UID --gid $USER_GID -m $USERNAME \
    # sudoユーザー追加（DevContainer内で権限が必要な場合）
    && apt-get install -y sudo \
    && echo $USERNAME ALL=\(root\) NOPASSWD:ALL > /etc/sudoers.d/$USERNAME \
    && chmod 0440 /etc/sudoers.d/$USERNAME

# クリーンアップ（イメージサイズ削減）
RUN apt-get autoremove -y \
    && apt-get clean -y \
    && rm -rf /var/lib/apt/lists/*

# 作業ディレクトリ設定
WORKDIR /workspace

# 非rootユーザーに切り替え
USER $USERNAME

# .NET Global ToolsのPATH追加（非rootユーザー用）
ENV PATH="${PATH}:/home/${USERNAME}/.dotnet/tools"

# .NET Global Toolsインストール（開発ツール）
# 注: USER $USERNAME の後に実行することで、非rootユーザーのホームディレクトリにインストール
RUN dotnet tool install -g dotnet-ef \
    && dotnet tool install -g dotnet-format

# コンテナ起動時のデフォルトコマンド
CMD ["/bin/bash"]
