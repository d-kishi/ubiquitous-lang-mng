# DevContainer使用ガイド

**最終更新**: 2025-11-04
**対象Phase**: Phase B-F2以降
**関連ADR**: ADR_025（DevContainer + Sandboxモード統合）、ADR_026（HTTPS証明書管理方針）

---

## 📖 目次

1. [DevContainerとは](#1-devcontainerとは)
2. [環境構築（前提条件）](#2-環境構築前提条件)
3. [DevContainerの起動・停止・再構築](#3-devcontainerの起動停止再構築)
4. [HTTPS証明書管理（重要）](#4-https証明書管理重要)
5. [開発ワークフロー](#5-開発ワークフロー)
6. [トラブルシューティング](#6-トラブルシューティング)
7. [よくある質問（FAQ）](#7-よくある質問faq)
8. [参考資料](#8-参考資料)

---

## 1. DevContainerとは

### 1.1 基本概念

**Development Container（DevContainer）** は、VS Codeの拡張機能「Remote - Containers」を使用して、Dockerコンテナ内で開発を行う仕組みです。

**アーキテクチャ**:
```
┌──────────────────────────────────────────────┐
│ Windows 11 ホスト環境                         │
│                                              │
│  📁 プロジェクトファイル（C:\Develop\...）    │
│  🔧 Docker Desktop                           │
│  💻 VS Code（Remote - Containers拡張）       │
│                                              │
│         │                                    │
│         │ Volume Mount + Remote Connection   │
│         ↓                                    │
│  ┌──────────────────────────────────────┐   │
│  │ DevContainer (Docker)                │   │
│  │                                      │   │
│  │  ✅ .NET SDK 8.0.415                 │   │
│  │  ✅ F# 8.0                           │   │
│  │  ✅ Node.js 24.x LTS                 │   │
│  │  ✅ PostgreSQL Client 16             │   │
│  │  ✅ VS Code拡張機能15個（自動）       │   │
│  │  ✅ 環境変数（自動設定）              │   │
│  │                                      │   │
│  │  📂 /workspace (マウント)            │   │
│  │  🔨 dotnet build/test/run           │   │
│  └──────────────────────────────────────┘   │
│                                              │
└──────────────────────────────────────────────┘
```

### 1.2 メリット

#### ✅ 環境セットアップ時間の大幅削減

**従来環境** (Phase A-B1):
- .NET SDK, F# Runtime, Node.js, Docker Desktop, VS Code拡張機能15個を手動インストール
- **セットアップ時間**: 75-140分（1.25-2.3時間）

**DevContainer環境** (Phase B-F2以降):
- DevContainer起動のみで全て自動セットアップ
- **セットアップ時間**: 5-8分（0.08-0.13時間）

**削減率**: **94-96%削減**（ADR_025効果測定）

#### ✅ 環境一貫性の保証

- Windows, macOS, Linux問わず同じ開発環境
- 改行コード混在問題の解消（CRLF vs LF）
- コンパイラ警告78件 → 0件（ADR_025実測値）

#### ✅ 環境再現性の向上

- 新規開発者が即座に参加可能（DevContainer起動のみ）
- 環境構築手順書の簡素化
- PC入れ替え時の移行コスト削減

#### ✅ セキュリティの強化

- ホスト環境とコンテナ環境の分離
- Docker + bubblewrap二重隔離（ADR_025）
- Sandboxモードによる安全なコマンド実行（Windows未対応、GitHub Issue #63）

---

## 2. 環境構築（前提条件）

### 2.1 必須ツール

以下のツールがホスト環境にインストール済みであること：

1. **Docker Desktop for Windows**
   - バージョン: 4.30以降推奨
   - WSL2バックエンド有効化
   - インストール手順: `Doc/99_Others/EnvironmentSetup/03_Docker_Setup.md`

2. **Visual Studio Code**
   - バージョン: 1.85以降推奨
   - インストール手順: `Doc/99_Others/EnvironmentSetup/04_VSCode_Setup.md`

3. **VS Code拡張機能: Remote - Containers**
   - 拡張機能ID: `ms-vscode-remote.remote-containers`
   - DevContainer起動に必須

4. **Git for Windows**
   - バージョン: 2.40以降推奨
   - インストール手順: `Doc/99_Others/EnvironmentSetup/01_Git_Setup.md`

### 2.2 HTTPS開発証明書の準備（重要）

**初回のみ実施**:

DevContainerでHTTPSアプリケーションを実行するため、ホスト環境で開発用SSL証明書を生成します。

**Windows環境**:
```bash
# PowerShellまたはGit Bashで実行
mkdir -p $USERPROFILE/.aspnet/https
dotnet dev-certs https --clean
dotnet dev-certs https -ep $USERPROFILE/.aspnet/https/aspnetapp.pfx -p DevPassword123
dotnet dev-certs https --trust
```

**macOS環境**:
```bash
mkdir -p ~/.aspnet/https
dotnet dev-certs https --clean
dotnet dev-certs https -ep ~/.aspnet/https/aspnetapp.pfx -p DevPassword123
dotnet dev-certs https --trust
```

**Linux環境**:
```bash
mkdir -p ~/.aspnet/https
dotnet dev-certs https --clean
dotnet dev-certs https -ep ~/.aspnet/https/aspnetapp.pfx -p DevPassword123
# Linuxでは --trust オプション非対応（ブラウザで手動承認）
```

**証明書情報**:
- **パスワード**: `DevPassword123`（開発環境専用）
- **有効期限**: 1年間（生成日から365日）
- **用途**: localhost専用（本番環境使用禁止）

**詳細**: [4. HTTPS証明書管理](#4-https証明書管理重要) 参照

---

## 3. DevContainerの起動・停止・再構築

### 3.1 DevContainerの起動

#### 初回起動手順

1. **VS Codeでプロジェクトを開く**
   ```
   File → Open Folder → C:\Develop\ubiquitous-lang-mng
   ```

2. **DevContainer起動プロンプト表示**
   - VS Code右下に通知表示: "Folder contains a Dev Container configuration file. Reopen in Container?"
   - 「Reopen in Container」をクリック

   または、手動起動:
   ```
   Ctrl+Shift+P → "Dev Containers: Reopen in Container" を選択
   ```

3. **DevContainer構築・起動（初回は5-8分）**
   - Dockerイメージビルド（3-5分）
   - VS Code拡張機能15個自動インストール（1-2分）
   - `postCreateCommand`実行（setup-https.sh + dotnet restore、1-2分）

4. **起動完了確認**
   - VS Code左下に「Dev Container: Ubiquitous Language Manager」表示
   - ターミナルでコマンド実行可能:
     ```bash
     dotnet --version  # 8.0.415
     node --version    # v24.x
     ```

#### 2回目以降の起動

1. VS Codeでプロジェクトを開く
2. 自動的にDevContainer起動（1-2分）
   - Dockerイメージは再利用（ビルド不要）
   - 拡張機能も再利用（インストール不要）

### 3.2 DevContainerの停止

#### 方法1: ローカルに戻る（推奨）

```
VS Code左下の緑色ボタン「><」をクリック
→ 「Reopen Folder Locally」を選択
```

**効果**:
- DevContainerから抜けてホスト環境に戻る
- Dockerコンテナは停止しない（次回起動が高速）

#### 方法2: DevContainerを完全停止

```bash
# ホスト環境（PowerShell）で実行
docker-compose -f .devcontainer/docker-compose.yml down
```

**効果**:
- DevContainerのDockerコンテナを停止・削除
- 次回起動時は起動処理が必要（1-2分）

### 3.3 DevContainerの再構築

**再構築が必要なケース**:
1. `.devcontainer/devcontainer.json`設定変更時
2. `.devcontainer/Dockerfile`修正時
3. VS Code拡張機能の追加時
4. Docker Desktopトラブル時のクリーン再起動時

#### 再構築手順

```
VS Code左下の緑色ボタン「><」をクリック
→ 「Rebuild Container」を選択
```

**所要時間**: 3-5分（Dockerイメージ再ビルド含む）

**注意**: HTTPS証明書はホスト環境にあるため、再構築後も自動的に利用可能（[4. HTTPS証明書管理](#4-https証明書管理重要) 参照）

---

## 4. HTTPS証明書管理（重要）

### 4.1 証明書の仕組み

**採用方式**: ボリュームマウント + 環境変数方式（ADR_026）

**アーキテクチャ**:
```
┌──────────────────────────────────────────────┐
│ Windows 11 ホスト環境                         │
│                                              │
│  📁 C:\Users\<username>\.aspnet\https\       │
│     └── aspnetapp.pfx (2.6KB, 1年有効)       │
│                                              │
│         │ Volume Mount (Read-Only)           │
│         │ ホスト証明書をコンテナに共有        │
│         ↓                                    │
│  ┌──────────────────────────────────────┐   │
│  │ DevContainer                         │   │
│  │                                      │   │
│  │  📁 /home/vscode/.aspnet/https/      │   │
│  │     └── aspnetapp.pfx (マウント)     │   │
│  │                                      │   │
│  │  🔐 環境変数（自動設定）              │   │
│  │  ASPNETCORE_Kestrel__Certificates__  │   │
│  │    Default__Path                     │   │
│  │  ASPNETCORE_Kestrel__Certificates__  │   │
│  │    Default__Password                 │   │
│  │                                      │   │
│  │  🚀 ASP.NET Core Kestrel             │   │
│  │     └── 起動時に証明書自動読み込み    │   │
│  └──────────────────────────────────────┘   │
│                                              │
└──────────────────────────────────────────────┘
```

**メリット**:
- ✅ DevContainer再構築で証明書が失われない（永続化）
- ✅ 環境再現性の確保（新規開発者も同じ手順）
- ✅ 自動化（postCreateCommandで検証）
- ✅ Microsoft公式推奨アプローチ（ADR_026参照）

### 4.2 証明書有効期限と更新

#### 有効期限

**証明書有効期限**: 1年間（生成日から365日）

**有効期限確認方法**（Windows）:
```bash
# PowerShellで実行
$cert = New-Object System.Security.Cryptography.X509Certificates.X509Certificate2("$env:USERPROFILE\.aspnet\https\aspnetapp.pfx", "DevPassword123")
$cert.NotAfter  # 有効期限日を表示
```

#### 証明書更新手順

**有効期限切れ時の症状**:
```
System.InvalidOperationException: 'Unable to configure HTTPS endpoint. The certificate is expired.'
```

**更新手順**（2-3分）:

1. **ホスト環境で証明書再生成**
   ```bash
   # Windows環境（PowerShellまたはGit Bash）
   dotnet dev-certs https --clean
   dotnet dev-certs https -ep $USERPROFILE/.aspnet/https/aspnetapp.pfx -p DevPassword123
   dotnet dev-certs https --trust
   ```

2. **DevContainer再構築**（オプション）
   - 既に起動中の場合は、アプリ再起動で証明書再読み込み
   - 確実に反映させる場合は「Rebuild Container」実行

**更新タイミングの目安**:
- 証明書生成から11ヶ月後に更新（余裕を持って）
- 環境構築手順書に生成日をメモしておく

### 4.3 setup-https.shスクリプト

**役割**: DevContainer起動時（`postCreateCommand`）に証明書の存在を自動確認

**実行タイミング**: DevContainer初回起動・再構築時

**成功時の出力例**:
```
==================================================
🔐 HTTPS Certificate Setup for DevContainer
==================================================

✅ HTTPS certificate found: /home/vscode/.aspnet/https/aspnetapp.pfx
📋 Certificate details:
-r--r--r-- 1 vscode vscode 2.6K Nov  4 12:34 /home/vscode/.aspnet/https/aspnetapp.pfx

✅ HTTPS setup complete. You can now run the app with HTTPS support.
   - HTTPS: https://localhost:5001
   - HTTP:  http://localhost:5000

==================================================
```

**証明書未作成時の出力例**:
```
==================================================
🔐 HTTPS Certificate Setup for DevContainer
==================================================

⚠️  ERROR: HTTPS certificate not found!

📝 Please run the following commands on your HOST machine (Windows):

   mkdir -p $USERPROFILE/.aspnet/https
   dotnet dev-certs https --clean
   dotnet dev-certs https -ep $USERPROFILE/.aspnet/https/aspnetapp.pfx -p DevPassword123
   dotnet dev-certs https --trust

Then rebuild the DevContainer:
   VS Code: Ctrl+Shift+P → 'Dev Containers: Rebuild Container'

==================================================
```

**エラー発生時の対処**:
1. ホスト環境で証明書生成コマンド実行
2. DevContainer再構築（「Rebuild Container」）
3. 再度エラーが出る場合は [6. トラブルシューティング](#6-トラブルシューティング) 参照

### 4.4 証明書セキュリティ

#### 開発環境専用証明書

**重要**: この証明書は開発環境専用です。

- ✅ **用途**: localhost専用（https://localhost:5001）
- ✅ **パスワード**: `DevPassword123`（開発環境専用）
- ❌ **本番環境使用**: 禁止（別の証明書管理方式を使用）

#### 証明書パスワード平文保存

`.devcontainer/devcontainer.json`に証明書パスワードが平文で記載されています:

```json
{
  "remoteEnv": {
    "ASPNETCORE_Kestrel__Certificates__Default__Password": "DevPassword123"
  }
}
```

**リスク評価**: 極めて低（ADR_026リスク評価）
- 開発環境専用証明書（本番環境使用不可）
- localhost専用（外部アクセス不可）

**本番環境では**: Azure Key Vault、User Secrets等の暗号化機能を使用

---

## 5. 開発ワークフロー

### 5.1 日常的な開発フロー

1. **VS Codeでプロジェクトを開く**
   - DevContainer自動起動（1-2分）

2. **コード編集**
   - ホスト環境のファイルを直接編集
   - DevContainer内のVS Codeに自動反映

3. **ビルド・テスト実行**

   **方法A: VS Code統合ターミナル（推奨）**
   ```bash
   # DevContainer内のターミナルで直接実行
   dotnet build
   dotnet test
   dotnet run --project src/UbiquitousLanguageManager.Web
   ```

   **方法B: ホスト環境から明示的実行**（Windows Sandbox非対応のため暫定対応）
   ```bash
   # Windows PowerShellで実行
   docker exec ubiquitous-lang-mng_devcontainer-devcontainer-1 dotnet build
   docker exec ubiquitous-lang-mng_devcontainer-devcontainer-1 dotnet test
   ```

   **詳細**: ADR_025「Windows Sandbox非対応と暫定対応」、GitHub Issue #63

4. **デバッグ実行**
   - F5キーでデバッグ起動
   - ブレークポイント設定・ステップ実行可能
   - https://localhost:5001 で動作確認

5. **Git操作**
   - ホスト環境・DevContainer内どちらでも可能
   - `git add`, `git commit`, `git push` 等

6. **終了**
   - VS Code左下「><」→「Reopen Folder Locally」
   - またはVS Codeを閉じる（次回自動再起動）

### 5.2 データベースマイグレーション

**マイグレーション追加**:
```bash
# DevContainer内で実行
dotnet ef migrations add MigrationName --project src/UbiquitousLanguageManager.Infrastructure
```

**マイグレーション適用**:
```bash
dotnet ef database update --project src/UbiquitousLanguageManager.Infrastructure
```

**PostgreSQL接続**:
- DevContainer内から: `Host=postgres` (Docker Compose service名)
- 接続文字列: `appsettings.Development.json`に設定済み

### 5.3 パッケージ管理

**NuGetパッケージ追加**:
```bash
dotnet add src/UbiquitousLanguageManager.Web package PackageName
dotnet restore
```

**npm パッケージ追加**:
```bash
cd src/UbiquitousLanguageManager.Web
npm install package-name
```

### 5.4 VS Code拡張機能の追加

1. `.devcontainer/devcontainer.json`の`extensions`配列に拡張機能IDを追加:
   ```json
   {
     "customizations": {
       "vscode": {
         "extensions": [
           "新しい拡張機能ID"
         ]
       }
     }
   }
   ```

2. DevContainer再構築（「Rebuild Container」）

---

## 6. トラブルシューティング

### 6.1 HTTPS証明書関連

#### 問題1: 証明書エラー（Unable to configure HTTPS endpoint）

**症状**:
```
System.InvalidOperationException: 'Unable to configure HTTPS endpoint. No server certificate was specified, and the default developer certificate could not be found or is out of date.'
```

**原因1**: ホスト環境で証明書未作成

**対処法**:
1. ホスト環境で証明書生成コマンド実行:
   ```bash
   mkdir -p $USERPROFILE/.aspnet/https
   dotnet dev-certs https --clean
   dotnet dev-certs https -ep $USERPROFILE/.aspnet/https/aspnetapp.pfx -p DevPassword123
   dotnet dev-certs https --trust
   ```
2. DevContainer再構築（「Rebuild Container」）

**原因2**: 証明書有効期限切れ（1年経過）

**対処法**: [4.2 証明書有効期限と更新](#42-証明書有効期限と更新) 参照

---

#### 問題2: setup-https.shエラー（改行コード問題）

**症状**:
```
: invalid optionripts/setup-https.sh: line 2: set: -
.devcontainer/scripts/setup-https.sh: line 3: \r': command not found
```

**原因**: スクリプトがCRLF改行コード（Windows）になっている

**対処法**:

**方法1: Git再正規化**（推奨）
```bash
# ホスト環境で実行
git add --renormalize .
git status
# setup-https.shが変更されていることを確認
git commit -m "Fix: Normalize line endings for setup-https.sh"
```

**方法2: スクリプト再作成**
```bash
# DevContainer内で実行
cat > .devcontainer/scripts/setup-https.sh <<'EOF'
#!/bin/bash
set -e
（スクリプト内容をコピー）
EOF
```

---

#### 問題3: ブラウザ証明書警告

**症状**: https://localhost:5001 にアクセス時、ブラウザで「この接続ではプライバシーが保護されません」警告表示

**原因**: ホスト環境で証明書の信頼設定未実施

**対処法**:
```bash
# ホスト環境で実行
dotnet dev-certs https --trust
```

**手動承認**（Linux環境のみ）:
- ブラウザで「詳細設定」→「localhost にアクセスする（安全ではありません）」をクリック
- 開発環境専用証明書のため、セキュリティリスクなし

---

### 6.2 DevContainer起動関連

#### 問題1: DevContainer起動失敗

**症状**: DevContainer起動時にエラーメッセージ表示・起動中断

**原因1**: Docker Desktop未起動

**対処法**:
1. Docker Desktopを起動
2. Docker Desktop右下が緑色（Running）であることを確認
3. DevContainer再起動

**原因2**: Dockerディスク容量不足

**対処法**:
1. Docker Desktop → Settings → Resources → Disk image location
2. 使用容量確認（推奨: 50GB以上空き）
3. 不要なDockerイメージ削除:
   ```bash
   docker system prune -a
   ```

**原因3**: .devcontainer/devcontainer.json構文エラー

**対処法**:
1. VS Codeで`.devcontainer/devcontainer.json`を開く
2. JSON構文エラーがないか確認（VS Codeが赤波線で表示）
3. 構文エラー修正後、DevContainer再起動

---

#### 問題2: VS Code拡張機能がインストールされない

**症状**: DevContainer起動後、期待した拡張機能が表示されない

**原因**: `.devcontainer/devcontainer.json`の`extensions`設定不備

**対処法**:
1. `.devcontainer/devcontainer.json`の`extensions`配列確認
2. 拡張機能IDが正しいか確認（例: `ms-dotnettools.csharp`）
3. DevContainer再構築（「Rebuild Container」）

---

#### 問題3: ポートフォワーディング失敗

**症状**: https://localhost:5001 にアクセスできない

**原因**: VS Codeポートフォワーディング未設定

**対処法**:
1. VS Code下部の「PORTS」タブを確認
2. 5001番ポートが表示されていない場合、手動追加:
   - 「PORTS」タブで「Add Port」→ 5001 入力
3. ブラウザで https://localhost:5001 にアクセス

---

### 6.3 データベース接続関連

#### 問題1: PostgreSQL接続エラー

**症状**:
```
Npgsql.NpgsqlException (0x80004005): Failed to connect to 127.0.0.1:5432
```

**原因**: 接続文字列が`Host=localhost`になっている

**対処法**:
1. `src/UbiquitousLanguageManager.Web/appsettings.Development.json`確認
2. 接続文字列の`Host`を`postgres`に変更:
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Host=postgres;Port=5432;Database=ubiquitous_lang_db;Username=ubiquitous_lang_user;Password=ubiquitous_lang_password"
     }
   }
   ```
3. アプリ再起動

**理由**: DevContainerからPostgreSQLコンテナへの接続は、Docker Composeサービス名（`postgres`）を使用

---

## 7. よくある質問（FAQ）

### Q1. DevContainerとDockerの違いは？

**A**: DevContainerは「開発環境特化型のDockerコンテナ」です。

- **Docker**: 汎用的なコンテナ技術（本番環境でも使用）
- **DevContainer**: 開発環境に最適化されたDockerコンテナ
  - VS Code拡張機能の自動インストール
  - ソースコードのボリュームマウント
  - デバッグ・ステップ実行のサポート
  - ポートフォワーディングの自動設定

---

### Q2. DevContainerの起動に時間がかかるのはなぜ？

**A**: 初回起動時（5-8分）のみ時間がかかります。

**初回起動時の処理**:
1. Dockerイメージビルド（3-5分）
   - .NET SDK, F# Runtime, Node.js等のインストール
2. VS Code拡張機能15個インストール（1-2分）
3. `postCreateCommand`実行（1-2分）
   - setup-https.sh実行
   - dotnet restore実行

**2回目以降**（1-2分）:
- Dockerイメージ再利用（ビルド不要）
- VS Code拡張機能再利用（インストール不要）

---

### Q3. DevContainer内で作成したファイルは保存される？

**A**: はい、保存されます。

**仕組み**:
- プロジェクトフォルダ（`C:\Develop\ubiquitous-lang-mng`）はホスト環境とDevContainerでボリュームマウント共有
- DevContainer内でファイルを作成・編集すると、ホスト環境にも即座に反映
- DevContainerを削除してもファイルは残る

---

### Q4. ホスト環境とDevContainer、どちらでgit操作すべき？

**A**: どちらでも可能です。推奨は「ホスト環境」。

**ホスト環境での操作**（推奨）:
- ✅ Git for Windows（Git Bash）の高速性能
- ✅ Git GUI（SourceTree, GitKraken等）使用可能
- ✅ GPG署名設定が簡単

**DevContainer内での操作**:
- ✅ VS Code統合ターミナルで完結
- ⚠️ Git設定（user.name, user.email）がDevContainer独立
- ⚠️ GPG署名設定が複雑

---

### Q5. DevContainer再構築で証明書は消える？

**A**: いいえ、消えません。

**理由**: HTTPS証明書はホスト環境（`C:\Users\<username>\.aspnet\https\`）に保存され、ボリュームマウントでDevContainerに共有されているため、再構築後も自動的に利用可能です。

**詳細**: [4. HTTPS証明書管理](#4-https証明書管理重要) 参照

---

### Q6. DevContainerからホスト環境に戻るには？

**A**: VS Code左下の緑色ボタン「><」→「Reopen Folder Locally」

**効果**:
- DevContainerから抜けてホスト環境に戻る
- Dockerコンテナは停止しない（次回起動が高速）

---

### Q7. DevContainerのDockerイメージサイズは？

**A**: 約3-4GB（初回ビルド時）

**内訳**:
- ベースイメージ（Ubuntu）: 1GB
- .NET SDK 8.0: 1-1.5GB
- Node.js 24.x: 500MB
- VS Code拡張機能: 500MB-1GB

**ディスク容量推奨**: 50GB以上空き

---

### Q8. 複数のDevContainerを同時起動できる？

**A**: はい、可能です。

**例**: 別のプロジェクトでもDevContainerを使用している場合
- プロジェクトAのDevContainer起動（ポート: 5001）
- プロジェクトBのDevContainer起動（ポート: 6001）
- 両方同時に動作可能

**注意**: ポート番号の重複に注意（`.devcontainer/devcontainer.json`の`forwardPorts`設定）

---

## 8. 参考資料

### プロジェクト内ドキュメント

- **ADR_025**: DevContainer + Sandboxモード統合採用（`Doc/07_Decisions/ADR_025_DevContainer_Sandboxモード統合.md`）
- **ADR_026**: DevContainer HTTPS証明書管理方針（`Doc/07_Decisions/ADR_026_DevContainer_HTTPS証明書管理方針.md`）
- **技術解説**: Claude Code Sandbox + DevContainer技術解説（`Doc/99_Others/Claude_Code_Sandbox_DevContainer技術解説.md`）
- **環境構築手順書**: HTTPS証明書セットアップ（`Doc/99_Others/EnvironmentSetup/07_Development_Settings.md`）
- **トラブルシューティングガイド**: DevContainer・開発環境問題（`Doc/10_Guide/Troubleshooting_Guide.md`）
- **GitHub Issue #63**: Windows環境でのClaude Code Sandboxモード非対応に伴うDevContainer手動実行対応
- **GitHub Issue #37**: DevContainer + Sandboxモード統合（Phase B-F2）

### Microsoft公式ドキュメント

- [Developing inside a Container](https://code.visualstudio.com/docs/devcontainers/containers) - VS Code DevContainers公式ガイド
- [Dev Container metadata reference](https://containers.dev/implementors/json_reference/) - devcontainer.json設定リファレンス
- [Hosting ASP.NET Core Images with Docker over HTTPS](https://learn.microsoft.com/en-us/aspnet/core/security/docker-https) - Docker環境でのHTTPS証明書管理
- [dotnet dev-certs command](https://learn.microsoft.com/en-us/dotnet/core/tools/dotnet-dev-certs) - 開発用SSL証明書生成コマンド

### 外部リソース

- [Dev Containers Tutorial](https://code.visualstudio.com/docs/devcontainers/tutorial) - VS Code公式チュートリアル
- [awesome-devcontainer](https://github.com/manekinekko/awesome-devcontainer) - DevContainer設定例集

---

**最終更新**: 2025-11-04
**次回更新予定**: 証明書有効期限管理機能追加（将来改善）
