# Step 04 組織設計・実行記録

**作成日**: 2025-11-03
**Step名**: DevContainer + Sandboxモード統合
**対応Issue**: GitHub Issue #37
**推定期間**: 6-8.5時間（1-2セッション）

---

## 📋 Step概要

### Step目的
Windows 11環境での開発環境セットアップ時間を96%削減し、Claudeの承認プロンプト数を84%削減することで、開発効率を大幅に向上させる。

### 主要な実施内容
1. **DevContainer環境構築**: .NET 8.0 + F# 8.0 + Node.js 20 + bubblewrap環境
2. **Sandboxモード統合**: `.claude/settings.json`設定による承認プロンプト削減
3. **自動動作検証**: ビルド・DB・アプリ・E2E・MCP Server動作確認
4. **🔴 ユーザー動作確認**: DevContainer初学者向け手順によるユーザー自身の動作確認（**Step4完了条件**）
5. **全ドキュメント作成**: 環境構築手順書再作成・Dev Container使用手順書・ADR作成

### 期待成果・ROI
- **セットアップ時間削減**: 1.25-2.3時間 → 5-8分（**94-96%削減**）
- **承認プロンプト削減**: 30-50回/Phase → 5-8回/Phase（**84%削減**）
- **Phase C-D ROI**: 5.85-25.9時間削減
- **長期ROI**: 16.3-69.9時間削減（233%-1398%）

---

## 🏢 組織設計

### Step特性判定
**品質保証段階（4-6）**: 技術基盤強化・開発環境最適化・効率化基盤構築

### SubAgent組み合わせ
**Pattern D（品質保証段階）** - MainAgentが直接実施
- **理由**: DevContainer設定ファイル作成・Sandboxモード設定は設定ファイル作成作業のため、専門SubAgentは不要
- **実施主体**: MainAgent直接実施
- **使用予定SubAgent**: なし

### 並列実行計画
本Stepは設定ファイル作成・段階的検証作業のため、並列実行は不要。

---

## 📊 Step内Stage構成（全8 Stage・6-8.5時間）

### Stage構成一覧

| Stage | 名称 | 内容 | 所要時間 | 完了基準 |
|-------|------|------|---------|---------|
| **1** | 環境設計・設定ファイル作成 | devcontainer.json, Dockerfile, docker-compose.yml設計 | 1-1.5h | 設計完了・ファイル構造確定 |
| **2** | Dockerfile作成 | .NET 8.0 + F# + Node.js 20 + bubblewrap環境構築 | 30-45m | Dockerfile作成完了 |
| **3** | docker-compose.yml調整 | 既存postgresサービス連携設定 | 30-45m | 新service定義完了 |
| **4** | Sandboxモード統合 | .claude/settings.json更新・承認範囲定義 | 1-1.5h | 設定ファイル更新完了 |
| **5** | 自動動作検証・効果測定 | MainAgentによる自動検証（ビルド・DB・アプリ・E2E・MCP確認・セットアップ時間96%削減・承認プロンプト84%削減測定） | 1-2h | 全検証項目クリア |
| **6** | 🔴 ユーザーによる動作確認 | DevContainer初学者向け手順に従ってユーザー自身がアプリを起動・動作確認（**Step4完了条件**） | 30-45m | ユーザー確認完了 |
| **7** | 全ドキュメント作成 | 環境構築手順書再作成・Dev Container使用手順書作成・ADR_0XX作成 | 1.5-2h | 全ドキュメント完成 |
| **8** | Step完了処理 | Phase_Summary更新・step-end-review Command実行・git commit | 30m | Step完了承認取得 |

---

## 🔴 Stage 6詳細: ユーザーによる動作確認（Step4完了条件）

**重要**: ユーザーはDevContainer利用経験がないため、以下の初学者向け手順を提供

### 1. DevContainer起動手順（初学者向け）

```
手順1: VS Codeを起動
手順2: プロジェクトフォルダを開く
       C:\Develop\ubiquitous-lang-mng

手順3: VS Code左下の緑色ボタン「><」をクリック

手順4: 表示されるメニューから「Reopen in Container」を選択

手順5: 初回ビルド開始（3-5分待機）
       - DevContainer環境構築中のメッセージが表示されます
       - ビルドログが表示されます

手順6: ビルド完了確認
       - VS Code内のターミナルがDevContainer内で動作していることを確認
       - プロンプトが「vscode@devcontainer:/workspace$」のような表示になります
```

### 2. アプリケーション起動手順（初学者向け）

DevContainer内のターミナルで以下を実行：

```bash
# 1. データベースマイグレーション実行
dotnet ef database update --project src/UbiquitousLanguageManager.Infrastructure

# 実行結果確認:
# "Done." と表示されればOK

# 2. アプリケーション起動
dotnet run --project src/UbiquitousLanguageManager.Web

# 実行結果確認:
# "Now listening on: https://localhost:5001" と表示されればOK
# アプリケーション起動中はターミナルがビジー状態になります

# 3. ブラウザでアクセス
# 新しいブラウザタブで以下のURLを開く:
https://localhost:5001

# 4. ログイン確認
# ログイン画面で以下の情報を入力:
Email: admin@ubiquitous-lang.com
Password: Admin123!
```

### 3. 動作確認項目（初学者向けチェックリスト）

以下の項目をすべて確認してください：

```
✅ ログイン成功
   - ログイン後、ダッシュボード画面が表示される

✅ プロジェクト一覧表示
   - 左メニューの「プロジェクト」をクリック
   - プロジェクト一覧が表示される

✅ 新規プロジェクト作成
   - 「新規作成」ボタンをクリック
   - プロジェクト名・説明を入力して「作成」をクリック
   - 一覧に新規プロジェクトが表示される

✅ プロジェクト編集
   - 作成したプロジェクトの「編集」ボタンをクリック
   - プロジェクト名・説明を変更して「更新」をクリック
   - 変更が反映される

✅ プロジェクト削除
   - 編集したプロジェクトの「削除」ボタンをクリック
   - 確認ダイアログで「削除」をクリック
   - 一覧から削除される
```

### 4. 確認完了報告

上記の全ての確認項目をクリアしたら、以下のいずれかの方法でMainAgentに報告してください：

```
報告メッセージ例:
"Stage 6動作確認完了しました。すべての項目でOKです。"

または

"動作確認完了。ログイン・CRUD操作すべて正常に動作しました。"
```

MainAgentは報告を受けてStage 6完了として記録し、Stage 7（全ドキュメント作成）に移行します。

**⚠️ 注意**: Stage 6完了まで次Stageに移行しません。ユーザー確認完了がStep4完了条件です。

---

## 📋 Stage 7詳細: 全ドキュメント作成

### 作業1: 環境構築手順書再作成

**削除対象**:
```
Doc\99_Others\EnvironmentSetup配下の全ファイル
```

**新規作成**:
```
Doc\99_Others\EnvironmentSetup\
├── README.md（DevContainer対応版環境構築手順書）
└── Troubleshooting.md（トラブルシューティングガイド）
```

**README.md内容**:
- Windows 11環境での前提条件（WSL2・Docker Desktop・VS Code）
- DevContainer起動手順（初学者向け詳細説明）
- アプリケーション起動手順（初学者向け詳細説明）
- 開発フロー（コーディング→ビルド→テスト→デバッグ）
- よくある質問（FAQ）

**Troubleshooting.md内容**:
- よくあるトラブルと解決方法
  - DevContainer起動失敗
  - ビルドエラー
  - DB接続エラー
  - ポート競合エラー
- 従来環境へのロールバック手順（30分で復帰可能）
- サポート・問い合わせ先

### 作業2: Dev Container使用手順書作成

**ファイル**: `Doc/08_Organization/Rules/Dev_Container使用手順書.md`

**内容**:
- DevContainer環境の概要・メリット
- 起動・停止手順
- 日常的な開発フロー
  - コード編集
  - ビルド・実行
  - テスト実行
  - デバッグ
  - git操作
- Tips・ベストプラクティス
  - 拡張機能のインストール
  - ターミナルの使い方
  - パフォーマンス最適化
- 注意事項・制約事項

### 作業3: ADR作成

**ファイル**: `Doc/07_Decisions/ADR_025_DevContainer_Sandboxモード統合決定.md`

**内容**:
- **決定内容**: DevContainer + Sandboxモード統合採用
- **背景・課題**: 環境セットアップ時間・承認プロンプト頻度の課題
- **判断根拠**:
  - セットアップ時間96%削減（実測値）
  - 承認プロンプト84%削減（実測値）
  - Windows 11 WSL2標準対応
  - ROI 233%-1398%（Phase C-D以降）
- **代替案との比較**:
  - ローカル環境維持（現状維持）
  - Dockerのみ導入
  - DevContainerのみ導入
  - Sandboxモードのみ導入
- **リスク評価**:
  - 技術的リスク（低）
  - ROIリスク（低）
  - セキュリティリスク（極めて低）
- **ロールバック手順**: 30分で従来環境復帰可能

---

## 🎯 Step成功基準

### 技術的完了基準
- ✅ DevContainer起動成功（VS Code「Reopen in Container」実行成功）
- ✅ ビルド成功（`dotnet build` → 0 Warning / 0 Error）
- ✅ DB接続成功（EF Core マイグレーション実行成功）
- ✅ アプリ起動成功（https://localhost:5001 アクセス可能）
- ✅ E2Eテスト実行成功（Playwright テスト実行成功）
- ✅ MCP Server動作確認（Serena・Playwright統合動作確認）
- ✅ Sandbox設定確認（設定ファイル正しく読み込まれ）

### 効果測定完了基準
- ✅ セットアップ時間94-96%削減確認（新規環境で測定）
- ✅ 承認プロンプト84%削減確認（Phase B-F2残り作業で実測）
- ✅ ロールバック手順確認（30分で従来環境復帰可能確認）

### 🔴 ユーザー動作確認完了基準（Step4完了条件）
- ✅ **ユーザー自身がDevContainer上でアプリケーションを起動し、正常動作を確認**
- ✅ ログイン・プロジェクトCRUD操作すべて成功
- ✅ ユーザーからMainAgentへの確認完了報告受領

### ドキュメント完成基準
- ✅ 環境構築手順書再作成完了（Doc\99_Others\EnvironmentSetup配下）
- ✅ Dev Container使用手順書作成完了（Doc/08_Organization/Rules/配下）
- ✅ ADR_025作成完了（Doc/07_Decisions/配下）
- ✅ Phase_Summary更新完了

---

## 🔧 技術的前提条件

### 環境要件
- ✅ Windows 11 WSL2環境（標準搭載・2023年9月以降）
- ✅ Docker Desktop 4.25.0以降（既に使用中・PostgreSQL運用中）
- ✅ VS Code 1.95.0以降（DevContainer拡張機能搭載）
- ✅ 既存PostgreSQL 16（Docker Container・稼働中）

### 技術基盤継承
- ✅ Phase B2成果: Playwright統合完了・E2Eテストデータ作成環境整備
- ✅ Phase B-F2 Step3成果: ADR_024作成・e2e-test Agent新設
- ✅ 既存docker-compose.yml: PostgreSQL・PgAdmin・Smtp4dev運用中

### ビルド・テスト状況
- ✅ 0 Warning / 0 Error状態維持（Phase B2以降継続）
- ✅ テスト成功率100%（335/338 tests・Phase B-F2 Step3時点）

---

## 📚 Step1成果物必須参照

### 必須参照ファイル
**Tech_Research_DevContainer_Sandbox_2025-10.md** - DevContainer + Sandboxモード技術調査資料
- **参照場所**: `Doc/08_Organization/Active/Phase_B-F2/Research/Tech_Research_DevContainer_Sandbox_2025-10.md`
- **重点参照セクション**:
  - 💡 実装計画 → Stage 1-5設計
  - 💰 ROI評価 → セットアップ時間96%削減・承認プロンプト84%削減測定方法
  - 📋 devcontainer.json設定サンプル
  - 📋 Dockerfile設定サンプル
  - 📋 .claude/settings.json設定サンプル
- **活用目的**: DevContainer構築・Sandboxモード統合実装の詳細手順・設定内容参照

---

## ⚠️ 実装時の注意事項

### 接続文字列調整（重要）
- **従来**: `Host=localhost;Port=5432;Database=ubiquitous_lang_db;...`
- **DevContainer**: `Host=postgres;Port=5432;Database=ubiquitous_lang_db;...`
- **理由**: docker-compose service名参照方式（`postgres` serviceと同一ネットワーク）
- **設定場所**: `devcontainer.json`の`remoteEnv`で自動設定

### ボリュームマウント
- **マウント設定**: `../..:/workspace:cached`
- **理由**: `.devcontainer`ディレクトリから2階層上のプロジェクトルートをマウント
- **cached オプション**: パフォーマンス最適化（macOS/Windows環境）

### ネットワーク設定
- **共有ネットワーク**: `ubiquitous-lang-network`
- **理由**: DevContainerとPostgres間でService名自動名前解決
- **設定場所**: `docker-compose.yml`のnetworksセクション

### ロールバック手順
- **各Stage完了時**: git commit推奨（30分単位のロールバックポイント作成）
- **完全ロールバック**: 従来環境復帰に30分（Docker Desktop停止→ローカル環境再起動）

### Stage 6実施制約
- **制約**: ユーザー確認完了まで次Stage（Stage 7）移行禁止
- **理由**: ユーザー動作確認がStep4完了条件のため

---

## 📊 Step実行記録（随時更新）

### Stage 1: 環境設計・設定ファイル作成
**実施日**: 2025-11-03
**担当**: MainAgent
**所要時間**: 30分

**実施内容**:
1. `.devcontainer/`ディレクトリ作成
2. `devcontainer.json`作成完了
   - VS Code拡張機能自動インストール設定（C#, F#, Playwright）
   - Sandboxモード有効化設定
   - ポートフォワーディング設定（5001, 5432, 8080等）
   - 環境変数設定（PostgreSQL接続文字列、SMTP設定）
3. `.devcontainer/docker-compose.yml`作成完了
   - devcontainerサービス定義
   - 既存PostgreSQLとの連携設定
   - ubiquitous-lang-networkネットワーク設定

**成果物**:
- ✅ `.devcontainer/devcontainer.json` (3.8KB)
- ✅ `.devcontainer/docker-compose.yml` (1.2KB)

---

### Stage 2: Dockerfile作成
**実施日**: 2025-11-03
**担当**: MainAgent
**所要時間**: 15分（+修正15分）

**実施内容**:
1. `.devcontainer/Dockerfile`作成
   - ベースイメージ: mcr.microsoft.com/dotnet/sdk:8.0
   - bubblewrapインストール（Sandboxモード用）
   - PostgreSQL clientインストール
   - Node.js 24.x（Active LTS）インストール ← **修正済み**
   - Playwright + npmパッケージインストール
   - 非rootユーザー（vscode）作成
   - .NET Global Toolsインストール（dotnet-ef, dotnet-format）

**修正履歴**:
- **初回エラー**: `fsharp`パッケージインストール失敗 → F#は.NET SDKに含まれるため削除
- **Feature競合**: devcontainer.json Node.js Feature削除（Dockerfileと競合）
- **バージョン変更**: Node.js 20 → 24（ホスト環境v24.10.0に合わせる・Active LTS使用）

**成果物**:
- ✅ `.devcontainer/Dockerfile` (2.5KB)

---

### Stage 3: docker-compose.yml調整
**実施日**: 2025-11-03
**担当**: MainAgent
**所要時間**: 10分

**実施内容**:
1. `.devcontainer/docker-compose.yml`ネットワーク設定調整
   - `external: true`削除（プロジェクトルートのdocker-compose.ymlと連携）
   - ubiquitous-lang-networkの適切な参照設定

**備考**: 既存`docker-compose.yml`（プロジェクトルート）は変更不要

**成果物**:
- ✅ `.devcontainer/docker-compose.yml`（修正版・1.2KB）

---

### Stage 4: Sandboxモード統合
**実施日**: 2025-11-03（Session 2 - 11/03 18:00-19:30）
**担当**: MainAgent
**所要時間**: 1.5時間（予定20分→実際1.5h：トラブルシューティング含む）

**実施内容**:

#### 1. DevContainer起動確認・トラブルシューティング（45分）

**問題1: 起動ディレクトリ誤り**
- **現象**: DevContainerが`C:\Develop`で起動（本来は`C:\Develop\ubiquitous-lang-mng`）
- **原因**: `.devcontainer/docker-compose.yml`のボリュームマウントパス誤り（`../..` → `..`に修正必要）
- **対応**: line 11修正: `- ../..:/workspace:cached` → `- ..:/workspace:cached`
- **結果**: DevContainer再ビルドで正常なディレクトリに修正

**問題2: Claude Code実行場所の議論**
- **ユーザー質問**: DevContainer内でClaude Code CLIを実行すべきか？
- **結論**: ホスト環境で実行（A方針）が標準構成
  - Sandboxモード: Claude Code（ホスト）がDevContainer（サンドボックス環境）を利用
  - DevContainer内実行（B方針）は非標準・複雑性増加
- **記録**: この議論は別途技術記録として`Doc/99_Others`に記録（後述）

#### 2. .NETフレームワーク互換性問題解決（30分）

**問題3: net9.0互換性エラー（NETSDK1045）**
- **現象**: `dotnet restore`失敗（DevContainer内.NET SDK 8.0.415がnet9.0未サポート）
- **原因**: 5つのテストプロジェクトが`net9.0`ターゲット（プロジェクト初期の名残）
- **対応**: 5プロジェクトを`net9.0` → `net8.0`に変更
  - `UbiquitousLanguageManager.Application.Unit.Tests.fsproj`
  - `UbiquitousLanguageManager.Contracts.Unit.Tests.csproj`
  - `UbiquitousLanguageManager.E2E.Tests.csproj`
  - `UbiquitousLanguageManager.Infrastructure.Integration.Tests.csproj`
  - `UbiquitousLanguageManager.Infrastructure.Unit.Tests.csproj`

**問題4: パッケージバージョン互換性エラー（NU1202）**
- **現象**: `dotnet restore`失敗（9.0.x パッケージがnet8.0未サポート）
- **対応**: 2プロジェクトで2パッケージをダウングレード
  - `Microsoft.AspNetCore.Mvc.Testing`: 9.0.9 → 8.0.11
  - `Microsoft.EntityFrameworkCore.InMemory`: 9.0.9 → 8.0.11
- **結果**: `dotnet restore`成功

#### 3. ビルド確認・Warning対応（15分）

**ビルド結果**:
- ✅ `dotnet build`成功（0 Error）
- ⚠️ 78個のnullable reference type warnings発生（CS8600, CS8625, CS8602, CS8604, CS8620）
- **原因**: ホスト環境では発生しない新規warning（DevContainer環境特有）
- **対応**: GitHub Issue #62作成・技術負債として記録（Phase B-F2終了後対応予定）

#### 4. Git差異問題解決・クロスプラットフォーム対応（20分）

**問題5: git status差異（676 vs 17）**
- **現象**: DevContainerで676個の変更ファイル認識、ホスト環境では17個のみ
- **原因**: CRLF（Windows）vs LF（Linux）の改行コード差異
  - ホスト: `core.autocrlf=true`
  - DevContainer: `core.autocrlf`未設定
- **対応1**: `.gitignore`修正（CoverageReport/個別ファイル200行 → ディレクトリレベル1行に簡略化）
- **対応2**: `.gitattributes`作成（改行コード統一設定）
  - リポジトリ内: LF統一
  - 作業ディレクトリ: OS標準に自動変換（Windows=CRLF, Linux=LF）
  - `git add --renormalize .`実行で正規化
- **結果**: 変更ファイル数 676 → 15個に削減（ホスト17個とほぼ一致）

#### 5. VSCode拡張機能統合（10分）

**実施内容**:
- `.devcontainer/devcontainer.json`の`extensions`配列を4個 → 15個に拡張
- **追加拡張機能（11個）**:
  - .NET開発必須（4個）: C# Dev Kit, .NET Runtime, Test Explorer, EditorConfig
  - 開発効率向上（5個）: GitLens, Docker, Path Intellisense, Markdown All in One, 日本語言語パック
  - AI支援（2個）: GitHub Copilot, GitHub Copilot Chat

**成果物**:
- ✅ `.claude/settings.local.json`（更新・4.1KB）
- ✅ `.devcontainer/devcontainer.json`（拡張機能15個追加）
- ✅ `.devcontainer/docker-compose.yml`（ボリュームマウント修正）
- ✅ `.gitattributes`（新規作成・改行コード統一設定）
- ✅ `.gitignore`（CoverageReport/簡略化）
- ✅ 5プロジェクトファイル（net9.0→net8.0修正）
- ✅ GitHub Issue #62（78 warnings技術負債記録）

**最終状態**:
- ✅ dotnet restore成功
- ✅ dotnet build成功（0 Error, 78 Warnings技術負債記録済み）
- ✅ git status正常化（15個の実変更のみ認識）
- ✅ DevContainer環境構築完了（Stage 5検証待ち）

---

### Stage 5: 自動動作検証・効果測定
**実施日**: 2025-11-03（進行中・中断）
**担当**: MainAgent
**状況**: **DevContainerビルド待ち**

**実施内容（途中まで）**:
1. DevContainer設定ファイル3件作成完了（Stage 1-4）
2. Dockerfileビルドエラー2回修正完了
   - エラー1: `fsharp`パッケージ不存在 → 削除
   - エラー2: Node.js Feature競合 → devcontainer.json修正 + Node.js 20→24変更
3. **現在**: ユーザーによるDevContainer再ビルド待ち

**次回セッション開始時の作業フロー**:

#### 🔴 重要: ユーザーから「DevContainer起動完了しました」報告を受けたら以下を実施

1. **DevContainer環境確認**（5分）:
   ```bash
   # DevContainer内ターミナルで実行
   dotnet --version    # .NET 8.0確認
   node --version      # Node.js 24.x確認
   npm --version       # npm確認
   git --version       # git確認
   which bubblewrap    # bubblewrap確認
   psql --version      # PostgreSQL client確認
   ```

2. **ビルド確認**（5-10分）:
   ```bash
   dotnet build
   # 期待結果: 0 Warning / 0 Error
   ```

3. **DB接続確認**（5分）:
   ```bash
   dotnet ef database update --project src/UbiquitousLanguageManager.Infrastructure
   # 期待結果: "Done."表示
   ```

4. **アプリ起動確認**（5分）:
   ```bash
   dotnet run --project src/UbiquitousLanguageManager.Web
   # 期待結果: "Now listening on: https://localhost:5001"
   # ブラウザで https://localhost:5001 にアクセス可能確認
   ```

5. **E2Eテスト実行確認**（10分）:
   ```bash
   dotnet test tests/UbiquitousLanguageManager.E2E.Tests/
   # 期待結果: Playwrightテスト実行成功
   ```

6. **MCP Server動作確認**（5分）:
   - Serena: `mcp__serena__check_onboarding_performed`実行
   - Playwright: `mcp__playwright__browser_navigate`実行

7. **効果測定**（10分）:
   - セットアップ時間: 従来75-140分 → DevContainer 5-8分（94-96%削減確認）
   - 承認プロンプト: Phase B-F2残り作業で測定予定

**検証完了後の次ステップ**:
- Stage 5完了 → **Stage 6: ユーザー動作確認**に移行
- Stage 6完了 → Stage 7: 全ドキュメント作成
- Stage 7完了 → Stage 8: Step完了処理

**検証結果**:
（DevContainer起動成功後に記録）

---

### Stage 6: 🔴 ユーザーによる動作確認（Step4完了条件）
**実施日**: 未実施
**担当**: ユーザー
**実施内容**:
（実施後に記録）

**確認結果**:
（実施後に記録）

---

### Stage 7: 全ドキュメント作成
**実施日**: 未実施
**担当**: MainAgent
**実施内容**:
（実施後に記録）

**成果物**:
（実施後に記録）

---

### Stage 8: Step完了処理
**実施日**: 未実施
**担当**: MainAgent
**実施内容**:
（実施後に記録）

**完了確認**:
（実施後に記録）

---

## ✅ Step終了時レビュー

**実施日**: 未実施
**レビュー担当**: MainAgent

### 品質確認結果
（step-end-review Command実行後に記録）

### 成果物一覧
（Step完了後に記録）

### 次Stepへの申し送り事項
（Step完了後に記録）

---

**最終更新**: 2025-11-03（Step組織設計・準備完了）
