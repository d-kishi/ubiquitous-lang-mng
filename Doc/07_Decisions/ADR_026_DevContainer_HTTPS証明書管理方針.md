# ADR_026: DevContainer HTTPS証明書管理方針

**作成日**: 2025-11-04
**決定日**: 2025-11-04
**ステータス**: Accepted
**対応Phase**: Phase B-F2
**対応Step**: Step 4 Stage 6
**対応Issue**: GitHub Issue #37（DevContainer + Sandboxモード統合）

---

## 概要

DevContainer環境でのHTTPS開発証明書管理にMicrosoft推奨の「ボリュームマウント + 環境変数方式」を採用し、DevContainer再構築時の証明書永続化と環境再現性を確保する。

---

## 背景・課題

### 課題1: DevContainer再構築時の証明書消失

**問題発覚**: Phase B-F2 Step4 Stage 6（ユーザー動作確認）実施時

ASP.NET Core Webアプリケーションをhttps://localhost:5001でデバッグ実行した際、以下のエラーが発生：

```
System.InvalidOperationException: 'Unable to configure HTTPS endpoint. No server certificate was specified, and the default developer certificate could not be found or is out of date.'
```

**一時対応の試み**: DevContainer内で`dotnet dev-certs https`コマンドを実行し証明書を生成

```bash
dotnet dev-certs https -ep /home/vscode/.aspnet/https/aspnetapp.pfx -p DevPassword123
dotnet dev-certs https --trust
```

**問題の本質**: ユーザーからの指摘
> "おそらく今、DevContainer内に直接証明書を配置したのではないかと思いますが、それだけではDevContainerを一から再作成した際は毎回DDL証明書を作成しなければなりませんよね？そうなると、DevContainerを採用する事で可能になる開発環境の横展開に弊害が出るのではないでしょうか？"

**影響範囲**:
- DevContainer再構築のたびに証明書手動生成が必要（5-10分）
- 新規開発者の環境セットアップ時に追加手順発生
- DevContainerの「環境再現性」の利点が損なわれる
- ADR_025で目指した「セットアップ時間94-96%削減」の効果が減少

### 課題2: 環境再現性の損失

**DevContainer採用の本来の目的**（ADR_025より）:
- ✅ セットアップ時間94-96%削減（75-140分 → 5-8分）
- ✅ 環境一貫性確保（Windows/macOS/Linux統一）
- ✅ 横展開可能性（新規開発者即座に参加可能）

**一時対応での問題**:
- ❌ 証明書手動生成手順の追加（5-10分）
- ❌ 手順書の複雑化（トラブルシューティング項目増加）
- ❌ DevContainer再構築時のエラーリスク（証明書未作成状態でアプリ起動不可）

### 課題3: 開発ワークフローへの影響

**DevContainer再構築が必要なシナリオ**:
1. .devcontainer/devcontainer.json設定変更時
2. .devcontainer/Dockerfile修正時
3. VS Code拡張機能の追加時
4. Docker Desktopトラブル時のクリーン再起動時

**頻度**: Phase C-D（推定10-15週間）で5-10回程度発生見込み

**一時対応のコスト**:
- 再構築毎に5-10分 × 5-10回 = 25-100分（0.42-1.67時間）
- ADR_025で削減した時間（6.51-25.5時間）の2-6%が失われる

---

## 決定内容

### 採用方式: ボリュームマウント + 環境変数方式

**アーキテクチャ概要**:

```
┌──────────────────────────────────────────────┐
│ Windows 11 ホスト環境                         │
│                                              │
│  📁 C:\Users\<username>\.aspnet\https\       │
│     └── aspnetapp.pfx (2.6KB, 1年有効)       │
│                                              │
│         │                                    │
│         │ Volume Mount (Read-Only)           │
│         │ ${localEnv:USERPROFILE}/.aspnet/https │
│         ↓                                    │
│  ┌──────────────────────────────────────┐   │
│  │ VSCode DevContainer (Docker)         │   │
│  │                                      │   │
│  │  📁 /home/vscode/.aspnet/https/      │   │
│  │     └── aspnetapp.pfx (マウント)     │   │
│  │                                      │   │
│  │  🔐 環境変数                         │   │
│  │  ASPNETCORE_Kestrel__Certificates__  │   │
│  │    Default__Path                     │   │
│  │  ASPNETCORE_Kestrel__Certificates__  │   │
│  │    Default__Password                 │   │
│  │                                      │   │
│  │  🔧 postCreateCommand:               │   │
│  │  setup-https.sh（証明書検証）        │   │
│  └──────────────────────────────────────┘   │
│                                              │
└──────────────────────────────────────────────┘
```

---

## 実装内容

### Phase 1: 問題発見・技術調査（2025-11-04）

**技術調査実施**: Microsoft公式ドキュメント調査
- [Hosting ASP.NET Core Images with Docker over HTTPS](https://learn.microsoft.com/en-us/aspnet/core/security/docker-https)
- [dotnet dev-certs command](https://learn.microsoft.com/en-us/dotnet/core/tools/dotnet-dev-certs)

**調査結論**: Microsoft公式推奨アプローチは「ボリュームマウント方式」

---

### Phase 2: ホスト環境でHTTPS証明書生成（初回のみ）

**実行環境**: Windows 11ホスト環境（PowerShellまたはGit Bash）

**実行コマンド**:
```bash
# 証明書保存ディレクトリ作成
mkdir -p $USERPROFILE/.aspnet/https

# 既存証明書クリーンアップ
dotnet dev-certs https --clean

# 証明書生成（PFX形式、パスワード付き）
dotnet dev-certs https -ep $USERPROFILE/.aspnet/https/aspnetapp.pfx -p DevPassword123

# ホスト環境での信頼設定（ブラウザ証明書警告回避）
dotnet dev-certs https --trust
```

**生成結果**:
- **ファイルパス**: `C:\Users\<username>\.aspnet\https\aspnetapp.pfx`
- **ファイルサイズ**: 2.6KB
- **証明書パスワード**: `DevPassword123`（開発環境専用）
- **有効期限**: 1年間（生成日から365日）
- **証明書用途**: localhost専用（本番環境使用禁止）
- **信頼設定**: ホスト環境のWindowsルート証明書ストアに登録

**macOS環境での実行コマンド**:
```bash
mkdir -p ~/.aspnet/https
dotnet dev-certs https --clean
dotnet dev-certs https -ep ~/.aspnet/https/aspnetapp.pfx -p DevPassword123
dotnet dev-certs https --trust
```

**Linux環境での実行コマンド**:
```bash
mkdir -p ~/.aspnet/https
dotnet dev-certs https --clean
dotnet dev-certs https -ep ~/.aspnet/https/aspnetapp.pfx -p DevPassword123
# Linuxでは --trust オプション非対応（ブラウザで手動承認）
```

---

### Phase 3: DevContainer設定修正

#### 3-1. devcontainer.json修正

**ファイル**: `.devcontainer/devcontainer.json`

**追加内容1: 証明書ボリュームマウント**（Line 11-13）

```json
{
  "mounts": [
    "source=${localEnv:USERPROFILE}${localEnv:HOME}/.aspnet/https,target=/home/vscode/.aspnet/https,readonly,type=bind"
  ]
}
```

**設定詳細**:
- `source`: ホスト環境のパス（Windows: `%USERPROFILE%`、macOS/Linux: `$HOME`）
- `target`: DevContainer内のマウント先（`/home/vscode/.aspnet/https`）
- `readonly`: 読み取り専用マウント（誤って証明書削除防止）
- `type=bind`: バインドマウント（証明書ファイルの直接共有）

**追加内容2: 環境変数設定**（Line 103-106）

```json
{
  "remoteEnv": {
    "ASPNETCORE_Kestrel__Certificates__Default__Password": "DevPassword123",
    "ASPNETCORE_Kestrel__Certificates__Default__Path": "/home/vscode/.aspnet/https/aspnetapp.pfx"
  }
}
```

**設定詳細**:
- `ASPNETCORE_Kestrel__Certificates__Default__Path`: 証明書ファイルパス
- `ASPNETCORE_Kestrel__Certificates__Default__Password`: 証明書パスワード
- ASP.NET Core Kestrelサーバーが起動時に自動読み込み

**追加内容3: postCreateCommand修正**（Line 62）

```json
{
  "postCreateCommand": "bash .devcontainer/scripts/setup-https.sh && dotnet restore"
}
```

**変更内容**: 証明書検証スクリプト追加

---

#### 3-2. setup-https.shスクリプト作成

**ファイル**: `.devcontainer/scripts/setup-https.sh`（新規作成）

**実装内容**:
```bash
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
```

**スクリプト機能**:
1. **証明書存在チェック**: `/home/vscode/.aspnet/https/aspnetapp.pfx`が存在するか確認
2. **エラー時のガイダンス表示**: 証明書未作成時、ホスト環境での生成コマンドを表示
3. **証明書情報表示**: デバッグ用にファイルサイズ・権限を表示
4. **成功メッセージ**: HTTPS/HTTPのURL表示

**重要: 改行コード形式**
- **必須**: LF改行コード（Linux環境で実行可能）
- **禁止**: CRLF改行コード（Windows）
- **理由**: DevContainerはLinuxベースのため、CRLFで実行すると以下のエラー発生
  ```
  : invalid optionripts/setup-https.sh: line 2: set: -
  .devcontainer/scripts/setup-https.sh: line 3: \r': command not found
  ```

**作成方法**: Gitが自動的にLF変換（.gitattributesで制御済み）

---

### Phase 4: DevContainer再構築

**実行手順**:
1. VS Code左下の緑色ボタン「><」をクリック
2. 「Rebuild Container」を選択
3. DevContainer再構築開始（約3-5分）
4. `postCreateCommand`でsetup-https.sh自動実行
5. 証明書存在確認・成功メッセージ表示

**再構築ログ例**:
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

---

## 判断根拠

### Microsoft公式推奨アプローチ

**公式ドキュメント**: [Hosting ASP.NET Core Images with Docker over HTTPS](https://learn.microsoft.com/en-us/aspnet/core/security/docker-https)

**推奨方式**:
> "For development purposes, you can use a volume mount to share your local development certificate with the container."

**推奨理由**（Microsoft公式）:
1. **証明書永続化**: コンテナ再構築で証明書が失われない
2. **環境再現性**: 複数開発者・複数環境で同じ手順
3. **セキュリティ**: ホスト環境で証明書を集中管理
4. **運用効率**: 自動化（postCreateCommandで検証）

### 環境再現性の確保

**DevContainer再構築時の動作**:

| フェーズ | 一時対応（非推奨） | 採用案: ボリュームマウント方式 |
|---------|------------------|-----------------------------|
| **1. DevContainer起動** | ❌ 証明書なし | ✅ ホスト証明書を自動マウント |
| **2. postCreateCommand実行** | ⚠️ スクリプトなし | ✅ setup-https.sh実行・検証 |
| **3. アプリ起動** | ❌ HTTPS証明書エラー | ✅ https://localhost:5001 で起動 |
| **4. デバッグ実行** | ❌ 手動証明書生成必要（5-10分） | ✅ 即座に実行可能 |

**環境横展開時の手順**:

**一時対応**:
1. DevContainer起動
2. 証明書エラー発生
3. DevContainer内で`dotnet dev-certs https`実行
4. アプリ再起動
5. **合計**: 10-15分

**採用案**:
1. ホスト環境で証明書生成（初回のみ・2-3分）
2. DevContainer起動
3. setup-https.sh自動実行・成功
4. アプリ即座に起動
5. **合計**: 2-3分（初回）、0分（2回目以降）

### 証明書永続化のメリット

**証明書有効期限**: 1年間（365日）

**メンテナンスコスト**:
- **一時対応**: DevContainer再構築毎に証明書生成（5-10分 × 5-10回/Phase = 25-100分）
- **採用案**: 1年に1回のみ更新（2-3分）

**Phase C-D（10-15週間）でのコスト削減**:
- DevContainer再構築: 5-10回
- 削減時間: 22.5-97分（0.38-1.62時間）

**長期運用（Phase E以降・30週間）でのコスト削減**:
- DevContainer再構築: 15-30回
- 削減時間: 67.5-292分（1.13-4.87時間）

---

## 代替案との比較

| 観点 | 案1: postCreateCommand自動生成 | 案2: User Secrets方式 | **採用案: ボリュームマウント方式** |
|-----|---------------------|---------------|--------------------------|
| **証明書永続化** | ❌ 再構築で消失 | ⭐ 永続化（User Secrets保存） | **⭐⭐ 永続化（ホスト環境保存）** ✅ |
| **環境再現性** | ⭐ 中（自動生成だが毎回実行） | ⭐ 中（User Secrets共有必要） | **⭐⭐⭐ 高（ホスト環境のみ準備）** ✅ |
| **セットアップ複雑度** | ⭐⭐⭐ 簡単（スクリプトのみ） | ⭐⭐ 中（User Secrets設定） | **⭐ やや複雑（ホスト+DevContainer設定）** |
| **Microsoft推奨** | ❌ 非推奨（毎回生成は非効率） | ⭐ 推奨（本番環境向け） | **⭐⭐⭐ 最推奨（開発環境向け）** ✅ |
| **証明書有効期限管理** | ⚠️ 不要（毎回生成） | ⭐ 必要（1年毎） | **⭐⭐ 必要だが明確（1年毎）** ✅ |
| **DevContainer再構築時間** | ⚠️ +2-3分（証明書生成） | ⭐ 追加なし | **⭐⭐ 追加なし（即座に利用可能）** ✅ |
| **トラブルシューティング** | ⚠️ 生成失敗時の対処複雑 | ⭐ User Secrets権限エラー | **⭐⭐ setup-https.shでガイダンス表示** ✅ |
| **セキュリティ** | ⭐ 中（コンテナ内生成） | ⭐⭐ 高（User Secrets暗号化） | **⭐⭐ 高（ホスト環境集中管理）** ✅ |
| **本番環境適用可能性** | ❌ 不可（開発証明書のみ） | ⭐⭐⭐ 可（本番証明書にも対応） | **⭐ 不可（開発環境専用）** |

**結論**: 採用案（ボリュームマウント方式）が開発環境において最優位

---

### 案1: postCreateCommand自動生成方式（非採用）

**実装例**:
```json
{
  "postCreateCommand": "dotnet dev-certs https -ep /home/vscode/.aspnet/https/aspnetapp.pfx -p DevPassword123 && dotnet restore"
}
```

**メリット**:
- ✅ 設定が簡単（1行追加のみ）
- ✅ ホスト環境での事前準備不要

**デメリット（非採用理由）**:
- ❌ DevContainer再構築毎に証明書生成（2-3分のオーバーヘッド）
- ❌ 証明書永続化されない（再構築で消失）
- ❌ Microsoft非推奨（毎回生成は非効率）
- ❌ 証明書生成失敗時のトラブルシューティング複雑

### 案2: User Secrets方式（非採用）

**実装例**:
```bash
dotnet user-secrets set "Kestrel:Certificates:Default:Path" "/path/to/cert.pfx"
dotnet user-secrets set "Kestrel:Certificates:Default:Password" "password"
```

**メリット**:
- ✅ 証明書パスワードの暗号化保存
- ✅ 本番環境にも適用可能
- ✅ Microsoft推奨（本番環境向け）

**デメリット（非採用理由）**:
- ⚠️ User Secrets共有の複雑さ（新規開発者毎に設定）
- ⚠️ DevContainer環境でのUser Secrets設定が追加作業
- ⚠️ 証明書ファイル自体の共有方法が別途必要
- ⚠️ 開発環境には過剰な仕組み

---

## リスク評価

### 技術的リスク: **低**

#### リスク1: 証明書有効期限切れ

**確率**: 高（1年に1回必ず発生）
**影響**: 中（アプリ起動不可）
**発生タイミング**: 証明書生成から365日後

**症状**:
```
System.InvalidOperationException: 'Unable to configure HTTPS endpoint. The certificate is expired.'
```

**対策**:
1. **予防**: 証明書有効期限を環境構築手順書に明記
2. **検出**: setup-https.shで有効期限チェック機能追加（将来改善）
3. **復旧**: ホスト環境で証明書再生成（2-3分）
   ```bash
   dotnet dev-certs https --clean
   dotnet dev-certs https -ep $USERPROFILE/.aspnet/https/aspnetapp.pfx -p DevPassword123
   dotnet dev-certs https --trust
   ```
4. **ドキュメント**: トラブルシューティングガイドに詳細手順記載

**エスカレーション基準**: 証明書再生成コマンドがエラーの場合

#### リスク2: ホスト環境証明書未作成

**確率**: 中（新規開発者の初回DevContainer起動時）
**影響**: 中（setup-https.shでエラー・ガイダンス表示）
**発生タイミング**: DevContainer初回起動時

**症状**:
```
⚠️  ERROR: HTTPS certificate not found!
📝 Please run the following commands on your HOST machine (Windows):
   ...
```

**対策**:
1. **予防**: 環境構築手順書に証明書生成を必須手順として記載
2. **検出**: setup-https.shで自動検出・ガイダンス表示
3. **復旧**: ホスト環境で証明書生成後、DevContainer再構築

**エスカレーション基準**: 証明書生成コマンドがエラーの場合

#### リスク3: 改行コード問題（CRLF vs LF）

**確率**: 低（.gitattributesで制御済み）
**影響**: 中（setup-https.sh実行エラー）
**発生タイミング**: Windows環境でスクリプト編集時

**症状**:
```
: invalid optionripts/setup-https.sh: line 2: set: -
.devcontainer/scripts/setup-https.sh: line 3: \r': command not found
```

**対策**:
1. **予防**: .gitattributesで`*.sh`をLF改行に強制設定
2. **検出**: DevContainer起動時のエラーメッセージで即座に判明
3. **復旧**: Git再正規化（`git add --renormalize .`）またはスクリプト再作成

**エスカレーション基準**: Git再正規化でも解決しない場合

### セキュリティリスク: **極めて低**

#### リスク: 証明書パスワード平文保存

**確率**: -（仕様上該当）
**影響**: 極めて小（開発環境専用証明書）

**リスク評価**:
- ✅ 証明書用途: localhost専用（本番環境使用不可）
- ✅ 証明書パスワード: `DevPassword123`（開発環境専用）
- ✅ 保存場所: `.devcontainer/devcontainer.json`（Gitリポジトリ管理下）
- ⚠️ 平文保存: 環境変数として記述

**対策**: 本番環境では別の証明書管理方式（User Secrets、Azure Key Vault等）を使用
**ドキュメント**: devcontainer.jsonに「開発環境専用」コメント記載

### 運用リスク: **低**

#### リスク: ボリュームマウント失敗

**確率**: 低（Windows環境でDocker Desktop正常動作が前提）
**影響**: 中（DevContainer起動失敗）

**対策**:
1. **予防**: Docker Desktop最新版維持
2. **検出**: setup-https.shで証明書存在チェック
3. **復旧**: Docker Desktop再起動・設定確認

**エスカレーション基準**: Docker Desktop設定正常でもマウント失敗の場合

---

## ロールバック手順

HTTPS証明書対応を無効化し、HTTP-only構成に戻す場合、以下の手順で10分以内に復帰可能：

### 1. devcontainer.json修正（5分）

**ファイル**: `.devcontainer/devcontainer.json`

**変更内容**: 証明書関連設定をコメントアウト

```json
{
  // "mounts": [
  //   "source=${localEnv:USERPROFILE}${localEnv:HOME}/.aspnet/https,target=/home/vscode/.aspnet/https,readonly,type=bind"
  // ],

  "remoteEnv": {
    // "ASPNETCORE_Kestrel__Certificates__Default__Password": "DevPassword123",
    // "ASPNETCORE_Kestrel__Certificates__Default__Path": "/home/vscode/.aspnet/https/aspnetapp.pfx",
    "ASPNETCORE_URLS": "http://+:5000"  // HTTPのみ有効化
  },

  "postCreateCommand": "dotnet restore"  // setup-https.sh削除
}
```

### 2. launch.json修正（2分）

**ファイル**: `.vscode/launch.json`

**変更内容**: HTTPS URLをHTTPに変更

```json
{
  "env": {
    "ASPNETCORE_ENVIRONMENT": "Development",
    "ASPNETCORE_URLS": "http://localhost:5000"  // HTTPS → HTTP
  }
}
```

### 3. DevContainer再構築（3分）

```
VS Code左下の緑色ボタン「><」をクリック
→ 「Rebuild Container」を選択
```

### 4. 動作確認（2分）

- アプリ起動: http://localhost:5000 でアクセス可能
- HTTPS無効化: https://localhost:5001 は無効

**ロールバック実測時間**: 7-12分（目標10分以内 ✅）

---

## 参考資料

### Microsoft公式ドキュメント
- [Hosting ASP.NET Core Images with Docker over HTTPS](https://learn.microsoft.com/en-us/aspnet/core/security/docker-https) - Microsoft公式HTTPS証明書管理ガイド
- [dotnet dev-certs command](https://learn.microsoft.com/en-us/dotnet/core/tools/dotnet-dev-certs) - 開発用SSL証明書生成コマンドリファレンス
- [Kestrel web server in ASP.NET Core](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/servers/kestrel) - Kestrel証明書設定ドキュメント

### プロジェクト内ドキュメント
- **技術解説**: `Doc/99_Others/Claude_Code_Sandbox_DevContainer技術解説.md` - DevContainer + Sandboxモードアーキテクチャ詳細
- **実装記録**: `Doc/08_Organization/Active/Phase_B-F2/Step04_DevContainer_Sandboxモード統合.md` - Stage 6実施記録・申し送り事項
- **環境構築手順**: `Doc/99_Others/EnvironmentSetup/07_Development_Settings.md` - HTTPS証明書セットアップ手順（Stage 7で追記）
- **DevContainerガイド**: `Doc/99_Others/DevContainer使用ガイド.md` - 証明書管理詳細・トラブルシューティング（Stage 7で作成）

---

## 関連ADR

- **ADR_025: DevContainer + Sandboxモード統合採用** - 本ADRの前提となる開発環境統合決定
- **ADR_015: GitHub Issues連携による技術負債管理** - GitHub Issue #37として環境整備管理
- **ADR_016: プロセス遵守違反防止策** - Stage 6実施における厳格なプロセス遵守

---

## 承認記録

**決定者**: プロジェクトオーナー
**承認日**: 2025-11-04（Phase B-F2 Step4 Stage 6実施時）
**承認コメント**: "おそらく今、DevContainer内に直接証明書を配置したのではないかと思いますが、それだけではDevContainerを一から再作成した際は毎回DDL証明書を作成しなければなりませんよね？そうなると、DevContainerを採用する事で可能になる開発環境の横展開に弊害が出るのではないでしょうか？今の手法がDevContainer環境でSSLのWebアプリケーションを開発するベストプラクティスだとは思えないのですが、いかがでしょうか？" → 技術調査・恒久的対応実施 → "選択肢Aを採ります。対応お願いします。"

---

**最終更新**: 2025-11-04
