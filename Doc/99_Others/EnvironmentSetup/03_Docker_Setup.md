# Docker Desktop インストール手順

## 1. 前提条件の確認

### システム要件確認
PowerShell（管理者権限）で以下を実行：

```powershell
# Windows バージョン確認
winver
```
- Windows 11またはWindows 10 バージョン 2004以降が必要

### WSL2の有効化

1. PowerShell（管理者権限）で以下を実行：

```powershell
# WSL有効化
dism.exe /online /enable-feature /featurename:Microsoft-Windows-Subsystem-Linux /all /norestart

# 仮想マシンプラットフォーム有効化
dism.exe /online /enable-feature /featurename:VirtualMachinePlatform /all /norestart
```

2. **PCを再起動**

3. WSL2 Linux カーネル更新プログラムのインストール：
   - https://aka.ms/wsl2kernel にアクセス
   - 「x64 マシン用 WSL2 Linux カーネル更新プログラム パッケージ」をダウンロード
   - インストーラーを実行

4. WSL2をデフォルトに設定：

```powershell
wsl --set-default-version 2
```

## 2. Docker Desktop のダウンロード

1. 公式サイトにアクセス: https://www.docker.com/products/docker-desktop/
2. 「Download for Windows」をクリック
3. 「Docker Desktop Installer.exe」をダウンロード

## 3. インストール

1. ダウンロードした「Docker Desktop Installer.exe」を実行
2. インストールオプション：
   - ☑ Use WSL 2 instead of Hyper-V (推奨)
   - ☑ Add shortcut to desktop
3. 「OK」をクリックしてインストール開始
4. インストール完了後、「Close and restart」をクリック
5. **PCを再起動**

## 4. 初期設定

1. PC再起動後、Docker Desktopが自動起動
2. 利用規約に同意
3. Docker アカウントへのサインイン（スキップ可能）

## 5. インストール確認

PowerShellを開いて以下のコマンドを実行：

```powershell
docker --version
docker-compose --version
```

バージョン情報が表示されれば成功です。

## 6. 動作確認

```powershell
# Dockerの動作確認
docker run hello-world
```

「Hello from Docker!」というメッセージが表示されれば正常です。

## 7. Docker Compose ファイルの配置

プロジェクトルートに以下の内容で `docker-compose.yml` を作成：

```yaml
version: '3.8'

services:
  postgresql:
    image: postgres:15-alpine
    container_name: ubiquitous-lang-postgres
    environment:
      POSTGRES_USER: postgres
      POSTGRES_PASSWORD: postgres
      POSTGRES_DB: UbiquitousLanguageDB
    ports:
      - "5432:5432"
    volumes:
      - postgres_data:/var/lib/postgresql/data

  smtp4dev:
    image: rnwood/smtp4dev:latest
    container_name: ubiquitous-lang-smtp4dev
    ports:
      - "5025:25"
      - "5080:80"
    environment:
      - ServerOptions__Urls=http://+:80

volumes:
  postgres_data:
```

## 8. コンテナの起動

プロジェクトルートで以下を実行：

```powershell
# コンテナ起動
docker-compose up -d

# 起動確認
docker ps
```

postgresql と smtp4dev のコンテナが表示されれば成功です。

---
作成日: 2025-08-09