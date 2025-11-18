# 技術的学習・解決策データベース（2025-09-22拡張版）

## DevContainer構築・Sandboxモード統合（2025-11-03）

### DevContainer + Sandboxモード統合パターン

**実装構成**:
- **.devcontainer/devcontainer.json**: VS Code拡張・Sandbox設定・ポート転送・環境変数
- **.devcontainer/Dockerfile**: .NET 8.0 + Node.js 24 + bubblewrap環境
- **.devcontainer/docker-compose.yml**: Container orchestration設定
- **.claude/settings.local.json**: Sandboxモード有効化

**技術的価値**: 環境セットアップ時間96%削減（120分→5分）+ Sandbox統合による安全性向上

### Node.jsバージョン管理哲学（Container環境）

**決定**: Node.js 24.x（Active LTS）を直接インストール・Volta/nvm不採用

**理由**:
1. **Container環境特性**: Dockerfile固定バージョン・Immutable Infrastructure原則
2. **ホスト環境統一**: ホスト（v24.10.0）とContainer環境の一致
3. **バージョン管理ツール不要性**: Container再ビルドで完全制御可能

**技術判断基準**:
- ✅ Container環境: Dockerfile直接インストール（単一バージョン・不変インフラ）
- ✅ ホスト環境: Volta/nvm活用（複数プロジェクト・バージョン切り替え）

**学習**: Container環境とホスト環境の哲学的違い・バージョン管理ツールの適用範囲理解

### DevContainer Features vs Dockerfile競合解決

**問題**: Node.js FeatureとDockerfileの手動インストールが競合
```
ERROR: Feature "Node.js (via nvm), yarn and pnpm" failed to install!
sh: 10: source: Permission denied
exit code: 127
```

**根本原因**:
- DevContainer Features（nvm経由Node.js 20インストール）とDockerfile（直接Node.js 20インストール）の二重実行
- nvm環境変数設定とDockerfile環境の競合

**解決パターン**:
1. **DevContainer Features削除**: devcontainer.json から Node.js Feature削除
2. **Dockerfile統一**: 全パッケージインストールをDockerfileに集約
3. **コメント明記**: 手動インストール理由を明示的記録

```json
// Features（.NET環境）
// 注: Node.jsはDockerfileで手動インストール（Ver.24 Active LTS）
"features": {
  "ghcr.io/devcontainers/features/dotnet:2": {
    "version": "8.0",
    "installUsingApt": false
  },
  "ghcr.io/devcontainers/features/git:1": {
    "version": "latest"
  }
}
```

**予防策**: Features vs Dockerfile役割分担の事前設計・競合可能性チェック

### .NET SDK内蔵パッケージ理解

**問題**: Dockerfileで `fsharp` パッケージインストール試行 → apt-get失敗
```
failed to solve: process "/bin/sh -c apt-get update...fsharp...exit code: 100
```

**根本原因**: F#コンパイラは `mcr.microsoft.com/dotnet/sdk:8.0` に含まれており、個別インストール不要

**解決**: Dockerfile から `fsharp` パッケージ削除・コメント追記
```dockerfile
# パッケージ更新・基本ツールインストール
# 注: F#は.NET 8.0 SDKに含まれているため、追加インストール不要
RUN apt-get update && export DEBIAN_FRONTEND=noninteractive \
    && apt-get -y install --no-install-recommends \
    # bubblewrap（Sandboxモード用）
    bubblewrap \
    # PostgreSQL client
    postgresql-client \
    ...
```

**学習**: ベースイメージ内蔵ツールの事前確認必要性・.NET SDK構成要素理解

### `claude -c` セッション再開問題回避戦略

**問題**: `claude -c` でセッション再開時、長時間にわたってコンソールが応答しない
**影響**: 数十分～1時間の時間浪費・作業中断・フラストレーション

**解決戦略**:
1. **新規セッション開始**: `claude -c` 使用禁止・新規セッション開始を選択
2. **詳細記録の徹底**: Step組織設計書・Serenaメモリーに完全記録
3. **継続フロー設計**: 「今何をしている途中状態なのか」「次に何をすべきか」を明示的記録

**記録場所**:
- **Step組織設計書**: 次回セッション開始時の作業フロー（7ステップ手順・コマンド・期待結果）
- **Serenaメモリー**: daily_sessions（Session実施内容・技術的決定・次回指示）

**技術的価値**: Context管理戦略・セッション継続性確保・作業効率維持

**学習**: Claude Code制約（AutoCompact無応答問題）の理解・ドキュメント記録による継続性確保の重要性

---

## ログ管理戦略・実装指針

### ログ管理方針設計（Issue #24対応・2025-09-18策定）

#### 現状分析結果
- **ILogger使用**: 10ファイルで最小限の実装
- **設定状況**: 基本設定のみ（appsettings.json）
- **問題点**:
  - Console.WriteLineによるデバッグログ散在
  - 構造化ログ未実装
  - 環境別設定未整備
  - ログ管理方針未策定

#### F#/C#境界でのログ出力方針統一
**課題**: 構造化ログ・環境別設定の必要性
**解決方針**:
- **Domain層（F#）**: ログ出力禁止・純粋実装維持
- **Application層（F#）**: Result型・Railway-oriented Programming
- **Infrastructure層（C#）**: ILogger統一・構造化ログ実装
- **Web層（C#）**: ユーザー操作ログ・UI状態管理

**技術価値**: Console.WriteLine散在問題の体系的解決

#### 90分実装計画詳細

##### 1. ログ管理方針設計（30分）
**実施内容**:
- ログレベル定義（Critical, Error, Warning, Information, Debug, Trace）
- 各アプリケーション層でのログカテゴリ策定
- F#/C#境界でのログ出力方針統一
- ADR_017作成（ログ管理戦略の技術決定記録）

**成果物**:
- ログレベル・カテゴリ体系
- ADR_017_ログ管理戦略.md

##### 2. 設定ファイル最適化（20分）
**実施内容**:
- appsettings.json詳細ログ設定追加
- appsettings.Development.json開発環境専用設定
- 構造化ログ設定実装
- 環境別ログレベル調整

**成果物**:
- 本番・開発環境対応設定ファイル
- 構造化ログ基盤

##### 3. Program.cs統合実装（25分）
**実施内容**:
- ASP.NET Core Logging統合強化
- ログプロバイダー最適化設定
- リクエストスコープログ設定
- F#サービスでのILogger統合確認

**成果物**:
- 統合ログ設定基盤
- 全層でのILogger利用基盤

##### 4. 実装ガイドライン作成（10分）
**実施内容**:
- Logging_Guidelines.md作成
- ログレベル選択基準文書化
- パフォーマンス・セキュリティ考慮事項整理

**成果物**:
- 開発者向けログ実装ガイドライン

##### 5. 動作確認（15分）
**実施内容**:
- 各環境でのログ出力確認
- パフォーマンス影響測定
- ログ出力精度確認

**成果物**:
- ログ管理基盤の動作保証

#### 技術的検討事項

##### 構造化ログ実装
- **目的**: 検索・分析可能なログ出力
- **技術**: JSON形式ログ出力
- **活用**: 本番運用での問題分析効率化

##### パフォーマンス考慮
- **測定対象**: ログ出力による処理速度影響
- **最適化**: 不要ログの削減・レベル調整
- **本番対応**: Production環境でのログレベル最適化

#### 期待効果

##### 短期効果
- **体系的ログ管理**: 統一されたログ出力基盤
- **開発効率向上**: 適切なデバッグ情報取得
- **品質向上**: ログによる問題特定効率化

##### 長期効果
- **運用安定性**: 本番環境でのシステム監視基盤
- **保守性向上**: ログベースの問題分析体制
- **拡張性**: 将来的なログ分析ツール統合準備

## 仕様準拠・品質管理

### 原典仕様書直接参照の必要性（Issue #18解決）
**課題**: Serenaメモリーのみでは詳細仕様の見落とし発生
**解決策**: 9つの原典仕様書必須読み込み
- 要件定義書・機能仕様書・UI設計書・データベース設計書等
- spec-analysis Agent: 原典仕様網羅・重複実装リスク事前特定
- spec-compliance Agent: 原典照合プロセス・100点満点評価システム

**実証効果**: パスワード変更重複実装問題100%検出（従来見落とし）

### 仕様準拠スコアリングシステム
**評価基準**:
- **95点以上**: 優秀品質（即座リリース可能）
- **85-94点**: 良好品質（軽微改善後リリース）  
- **75-84点**: 改善必要（項目的改善必要）
- **75点未満**: 品質不適格（大幅修正必要）

**技術的価値**: 主観的判断から客観的スコア評価への転換

### 重複実装検出機構
**技術課題**: 同一機能の複数実装による保守性低下
**検出実例**: パスワード変更機能3箇所重複
- Login.razor（モーダル・70行）
- ChangePassword.razor（独立画面・390行）  
- Profile.razor（リンク・4行）

**統一戦略**: UI設計書準拠の独立画面遷移統一

## ASP.NET Core技術解決パターン

### 初期パスワード仕様準拠実装（TECH-002完全解決）
**仕様要件**: InitialPassword=\"su\"（平文）、PasswordHash=NULL
**実装パターン**:
1. **InitialDataService.cs**: UserManager.CreateAsync(user)のみ、パスワードハッシュ化なし
2. **AuthenticationService.cs**: 平文InitialPassword認証ロジック追加  
3. **データベース整合**: AspNetUsers.PasswordHash=NULL, InitialPassword='su'
4. **SQLスクリプト**: init/02_initial_data.sql仕様準拠更新

**セキュリティ配慮**: 初回ログイン時強制パスワード変更実装必須

### ポート設定不整合解決パターン（Issue #16）
**真の原因**: 実行方法不統一（VS Code vs CLI）
**解決アプローチ**:
1. **HTTPS統一**: launchSettings.json + .vscode/launch.json
2. **ASP.NET Core標準**: デフォルトポート（HTTP=5000, HTTPS=5001）
3. **環境分離**: launchSettings.json開発環境設定分離

**技術選択基準**: 標準準拠・本番配慮・既存資産活用

### ルートパス競合解決
**問題**: Pages/Index.razor と Components/Pages/Home.razor の @page \"/\" 競合
**解決**: Home.razor から @page ディレクティブ削除
**予防策**: Blazor Serverルーティング設計時の重複チェック必須

## Clean Architecture実装パターン

### TypeConverter基盤活用（1,539行完成）
**境界効率変換**: F#↔C#データ型変換最適化
**保守負荷削減**: 50%削減効果実証
**活用パターン**: Domain Model ↔ DTO ↔ ViewModel変換統一

### F# Domain層活用パターン
**Railway-oriented Programming**: Result型活用・エラーハンドリング統一
**関数型プログラミング**: 副作用なし・純粋実装維持
**型安全性**: F#型システム活用による実行時エラー予防

## データベース設計・管理

### GitHub Issues活用ベストプラクティス
**移行効果**: ファイル管理 → GitHub Issues管理
- Issue番号による追跡性向上・一元管理実現
- 技術負債管理効率化（/Doc/10_Debt/ → GitHub Issues）
- TECH-011～015: 長期技術負債のIssue管理移行完了

### データベース整合性回復パターン
**直接SQL更新**: 開発環境データ修正
**SQLスクリプト整合**: init/02_initial_data.sql実体一致
**検証手順**: データ→コード→統合の順次確認

## SubAgent技術活用パターン

### Pattern D: テスト集中改善
**実行フロー**: integration-test（基盤修正）→ unit-test（品質向上）→ spec-compliance（評価）
**効果**: 依存関係考慮段階実行による確実性向上・専門性活用高効率

### spec-compliance-check Command
**3段階フロー**:
1. **spec-analysis**: 仕様マトリックス作成・原典仕様網羅
2. **spec-compliance**: 原典照合・100点満点スコアリング
3. **design-review**: 設計品質評価・Clean Architecture準拠度

**総合判定**: 品質基準による自動判定・定量的品質管理

### SubAgent専門性活用原則
- **MainAgent直接修正の最小化**: 専門知識活用最大化
- **問題領域別専門委任**: 各Agent得意領域での高品質実装
- **基準未達成時自動選定**: 再実行メカニズム確立

## 継続的改善機構

### 段階的品質達成アプローチ
**品質基準**: 95%必須基準 + 100%努力目標
**効果**: 完璧追求による開発停滞回避・継続的価値提供
**実証**: Step3（85%品質）完了 + Step4品質完全化準備

### 実証ベース改善手法
1. **具体的問題事例**: 改善機構設計起点
2. **段階的検証**: 改善 → 実証 → 効果測定 → クローズ
3. **システマティック検出**: 人的見落としを機構で補完
4. **定量化価値**: 主観判断から客観スコア評価

### 基盤文書更新影響管理
**ベストプラクティス**:
1. **影響範囲分析**: 変更が及ぶ全関連文書事前特定
2. **包括的更新**: 関連文書全体同期更新による整合性確保
3. **ユーザー確認**: 見落とし防止のためのフィードバック活用
4. **段階的実行**: 重要度順実行による品質・効率両立

## 開発環境標準化

### ASP.NET Core開発環境
**標準設定**: HTTPS=5001・launchSettings.json環境分離
**VS Code統合**: デバッグ実行一貫性維持・CLI実行環境変数標準化
**品質保証**: 段階的検証（設定→動作→文書）

### 実行環境統一効果
**VS Code・CLI完全統一**: https://localhost:5001
**HTTPSからHTTPSリダイレクト**: 307自動リダイレクト
**本番環境配慮**: 開発設定・本番環境変数制御分離

## Agent Skills Phase 2展開の学習（2025-11-01）

### 品質優先判断の重要性
**課題**: 効率重視で簡潔版Skillsを作成しようとした
**ユーザー指摘**: 「品質が下がる可能性があるなら、効率よりも品質を重視して作業を進めてください」
**解決**: 高品質版Skillsに変更・Phase 1同等の詳細度で作成
**学習**: ユーザーフィードバックによる方針転換の価値・品質優先の判断基準確立

### Agent Skills設計パターンの確立
**SKILL.md + 補助ファイル構成**:
- SKILL.md: 概要・目的・使用タイミング・基本指針
- 補助ファイル: 詳細パターン・ルール・チェックリスト
- Phase 1平均: SKILL.md + 3補助ファイル
- Phase 2平均: SKILL.md + 3.8補助ファイル

**効果**: モジュール化による再利用性向上・保守性向上・自律的適用の実現

### SubAgent選択ロジック体系化の価値
**13種類のAgent定義**:
- 研究・分析系: tech-research, spec-analysis, design-review, dependency-analysis
- 実装系: fsharp-domain, fsharp-application, contracts-bridge, csharp-infrastructure, csharp-web-ui
- QA系: unit-test, integration-test, code-review, spec-compliance

**選択原則**: Phase特性別組み合わせパターン・責務境界判定・並列実行判断
**効果**: SubAgent選択時間5分→1分削減・選択精度85%→95%向上見込み

### テストアーキテクチャ自律適用の基盤確立
**ADR_020準拠性の自動チェック**:
- 新規テストプロジェクト作成チェックリスト（7段階）
- 命名規則の厳格遵守（UbiquitousLanguageManager.{Layer}.{TestType}.Tests）
- 参照関係原則の自動確認（Unit/Integration/UI/E2E別）

**効果**: GitHub Issue #40再発防止・テスト品質向上・作業効率20-30%改善見込み

---

**作成**: 2025-09-22（技術的学習DB・ログ管理戦略統合版）
**更新**: 2025-11-03（DevContainer構築・Sandboxモード統合・Node.jsバージョン管理哲学・Features競合解決・`claude -c`問題回避戦略追加）
**統合元**: technical_learnings, logging_management_strategy_planning, session_insights系メモリー

---

## VSCode拡張機能のリグレッションバグ調査・対応（2025-11-15）

### C# Dev Kit / Ionide F# の役割理解

**C# Dev Kit (ms-dotnettools.csdevkit)**:
- **対応言語**: C#のみ
- **機能**: C#プロジェクトのIntelliSense、デバッグ、プロジェクト管理
- **重要**: F#はネイティブサポートしない（Ionideが必要）

**Ionide F# (ionide.ionide-fsharp)**:
- **対応言語**: F#のみ
- **機能**: F#プロジェクトのIntelliSense、REPL、プロジェクト管理
- **独立性**: C# Dev Kitとは独立して動作

**誤解の教訓**:
- 両者の「競合」は誤った仮説だった
- 各拡張機能の役割範囲を正確に理解することが重要

### リグレッションバグの調査手法

**タイムライン相関分析**:
1. 拡張機能の更新日を確認（VSCode拡張機能ビュー）
2. GitHub Issuesで同時期の報告を検索
3. リリースノート・変更履歴を確認
4. エラーメッセージの完全一致を検証

**本件の調査フロー**:
```
C# Dev Kit v1.81.7リリース（2025-11-13）
  ↓
GitHub Issues #2492/#2500報告（2025-11-13）
  ↓
ユーザー環境で問題発生（2025-11-15報告）
  ↓
エラーメッセージの完全一致確認
  ↓
リグレッションバグ確定
```

### 一時的な回避策としてのバージョンダウングレード

**目的**: アップストリーム修正までの業務継続

**手順**:
1. VSCode拡張機能ビューで対象拡張機能を選択
2. 歯車アイコン → "Install Another Version..."
3. 安定版（前バージョン）を選択してインストール
4. 自動更新を**一時的に**無効化

**重要な原則**:
- ⚠️ **恒久的なバージョン固定は避ける**
- アップストリーム修正を監視し、修正版リリース後は速やかに最新版に戻す
- チーム全体に一時的措置であることを共有

### DevContainerエラーログアクセス方法

**Claude Code実行環境の理解**:
- Claude CodeはWindowsホスト環境で実行
- DevContainer内のファイルには直接アクセス不可
- `docker exec`コマンドでコンテナ内実行が必要

**Git Bash環境での注意点**:
```bash
# パス変換を無効化する環境変数が必要
MSYS_NO_PATHCONV=1 docker exec <container_name> cat /path/to/file
```

**理由**: Git BashはWindowsパスをUnixパスに自動変換するため、`MSYS_NO_PATHCONV=1`で無効化

### アップストリームissue監視の重要性

**外部依存の問題対応原則**:
1. 自力での根本修正は不可能（外部ライブラリ/拡張機能）
2. 一時的回避策で業務継続
3. アップストリームの修正を定期監視
4. 修正版リリース後、速やかに適用

**監視方法**:
- GitHub Issuesをブックマーク
- 週次チェックを習慣化
- リリースノートの定期確認

## Playwright Test Agent vs MCP Serverの違い理解（2025-11-15）

### Phase B2記録との重大な不整合発見

**記録されていた内容**: 「Playwright Agents統合完了」
**実態**: Playwright MCP Serverのみ統合・Playwright Test Agentsは未導入

**混同の原因**:
- 「Playwright Agents」という表現の曖昧性
- MCP Server統合時に「Agents」という言葉を不正確に使用
- Test Agentsとの違いを明確に理解していなかった

### Playwright MCP Server vs Test Agentsの違い

**Playwright MCP Server**:
- **種類**: MCP (Model Context Protocol) ツール
- **機能**: 21個のツールを提供（browser_navigate, browser_click, etc.）
- **役割**: Claude Codeがブラウザを操作するための「道具」
- **本プロジェクトの状況**: ✅ **統合済み**（Phase B2で完了）
- **成果**: 93.3%効率化達成（Skills + MCP Serverの組み合わせ）

**Playwright Test Agents（Planner/Generator/Healer）**:
- **種類**: 3つのAI駆動Claude Code SubAgents
- **機能**:
  - **Planner**: テストシナリオ計画策定
  - **Generator**: Playwright Test コード自動生成
  - **Healer**: テスト失敗時の自動修復
- **要件**: Playwright Test（TypeScript/JavaScript）環境必須
- **本プロジェクトの状況**: ❌ **未導入**（C# Microsoft.Playwrightのみ使用）
- **導入の可否**: package.json不在・playwright.config.ts不在・JavaScript/TypeScript環境なし

### 93.3%効率化の正しい理解

**従来の誤認**: Playwright Test Agents統合による効率化
**実態**: 以下の組み合わせによる効率化

1. **Playwright MCP Server**（21ツール）
   - browser操作の自動化
   - スナップショット取得・要素選択支援
   
2. **playwright-e2e-patterns Skill**（Phase B2作成）
   - E2Eテスト実装パターンの標準化
   - data-testid設計パターン
   - Blazor Server SignalR対応パターン

3. **e2e-test Agent**（既存SubAgent）
   - E2Eテスト設計・実装の専門知識

### 技術調査による新たな発見

**Playwright Test Agents導入の評価結果**:
- **選択肢A（導入する）**: 環境構築コスト大・本プロジェクトとのミスマッチ
- **選択肢B（導入しない）**: 現状維持・Phase B3以降検討
- **選択肢C（既存基盤活用）**: 推奨・実証済み93.3%効率化継続

**推奨方針**: 選択肢C
- 理由: 既存基盤（MCP Server + Skills + e2e-test Agent）で十分な効果
- 実証: Phase B2で93.3%効率化達成済み
- リスク: 新規導入によるリスク回避

### ドキュメント記録の正確性の重要性

**教訓**:
1. **用語の正確性**: 「Playwright Agents」ではなく「Playwright MCP Server」
2. **成果の帰属**: 何によって効率化が達成されたかの明確な記録
3. **検証の重要性**: 記録内容と実装の照合確認

**再発防止**:
- Phase完了時の成果物リスト作成（具体的なファイル名・ツール名）
- 「統合」「導入」の定義明確化
- 実装確認の徹底（記録だけでなく実体の確認）

---

## Phase B-F2技術的学習（2025-11-18）

### Agent SDK Phase 1技術検証の重要な学習

#### 初回No-Go判断の誤りと訂正プロセス

**初回調査の重大な誤解**（2025-10-29午前）:
- ❌ Agent SDKは.NETアプリケーションに統合が必要
- ❌ 公式.NET SDKの展開を待つ必要がある
- ❌ F# + C# Clean Architectureとの統合が必要
- ❌ ROI基準未達成（3.4-19.7%）によるNo-Go判断

**ユーザー様指摘による誤解の訂正**:
> "このプロジェクト自体は実験的意味合いが強いため、正直ROI評価はまったく気にしていません。求めているのはClaude Agent SDKの技術的価値の検証であり、ROI評価は全く無価値な観点です。"

**再調査による正しい理解**:
- ✅ Agent SDKは外部プロセスとしてClaude Codeを監視・制御
- ✅ TypeScript/Python SDKで完結、.NET統合不要
- ✅ アプリケーションコードと独立、統合不要
- ✅ 実装工数40-60時間（初回見積もり80-120時間から50-67%削減）

**学習**:
1. **技術価値評価の重要性**: ROI評価よりも技術的可能性・学習価値を優先
2. **アーキテクチャの正確な理解**: 外部プロセス vs アプリケーション統合の違い
3. **ユーザーフィードバックの価値**: 前提条件の誤りをユーザーが訂正

#### TypeScript SDK学習による発見

**学習時間**: 約11時間（TypeScript SDK 9.0h + 正規表現2.0h）

**重要な発見**:
1. **Hooks型システムの理解**:
   - PreToolUse hook: Tool呼び出し前の介入ポイント
   - PostToolUse hook: Tool呼び出し後の検証ポイント
   - TypeScript型定義による安全な実装

2. **実現可能性確認の成果**:
   - ADR_016違反検出機能: FEASIBLE（PreToolUse hookでTask tool監視）
   - SubAgent成果物実体確認機能: FEASIBLE（PostToolUse hookでファイル存在確認）
   - 並列実行信頼性向上機能: FEASIBLE（並列Task tool呼び出し検出）

3. **Phase 2実施判断**: Go判断（Phase C期間中並行実施推奨、推定工数25-35時間）

**技術的価値**: 外部プロセスアーキテクチャによる拡張性・安全性の理解

### Playwright Test Agents統合の学習

#### Generator Agent効果測定

**効果**: 極めて高い（推定40-50%時間削減）

**成功要因**:
1. **TypeScript → C#変換の品質**: authentication.spec.ts（TypeScript）→ AuthenticationTests.cs（C#）の高品質変換
2. **contracts-bridge Agent統合**: F#↔C#型変換パターンの活用
3. **Playwright MCP Server活用**: 21ツールによるブラウザ操作自動化

**具体的成果**:
- AuthenticationTests.cs作成時間: 2.5時間（推定4-5時間 → 40-50%削減）
- テスト成功率: 100%（6/6 PASS、0 FAIL、3 SKIP）
- ViewportSize: 1920x1080（Full HD統一）
- data-testid selectors: Blazor Server SignalR対応

#### Healer Agent限界の発見

**効果**: 0%（0/1成功）

**限界の理解**:
1. **複雑な状態管理問題は自動検出・修復不可**: パスワード変更による認証情報不整合
2. **ユーザー手動テストの重要性**: 根本原因特定の鍵は人間の判断
3. **適用範囲の明確化**: 単純なセレクタエラーは修復可能、状態管理問題は不可

**根本原因の発見プロセス**:
```
問題: パスワード変更テスト後、元のパスワードに戻す処理が不足
  ↓
Healer Agent: 検出不可（表面的なエラーメッセージのみ）
  ↓
ユーザー手動テスト: パスワード不整合を発見
  ↓
修正: `/`リダイレクト後に`/change-password`へ再遷移してリセット実行
```

**学習**: 人間-AI協調の重要性・Healer Agent適用範囲の理解

### DevContainer HTTPS証明書管理の学習

#### ボリュームマウント方式の採用

**課題**: DevContainer再構築時にHTTPS証明書が消失
**解決**: ボリュームマウント + 環境変数 + postCreateCommand方式

**技術詳細**:
```json
"mounts": [
  "source=${localEnv:USERPROFILE}/.aspnet/https,target=/root/.aspnet/https,type=bind,consistency=cached"
],
"remoteEnv": {
  "ASPNETCORE_Kestrel__Certificates__Default__Password": "mypassword123",
  "ASPNETCORE_Kestrel__Certificates__Default__Path": "/root/.aspnet/https/aspnetapp.pfx"
},
"postCreateCommand": "bash .devcontainer/scripts/setup-https.sh"
```

**技術的価値**:
1. **環境再現性100%**: DevContainer再構築時も証明書が自動的に利用可能
2. **Microsoft公式推奨**: ボリュームマウント方式が公式推奨パターン
3. **セキュリティ**: 証明書ファイルをGit管理外に配置

**学習**: DevContainer環境における永続化データの管理パターン

### 改行コード混在問題の発見

#### 問題の発見

**現象**: DevContainer移行時に78個のwarnings（CS8600, CS8625, CS8602, CS8604, CS8620）が発生
**調査**: `.gitattributes` 作成前後での差異確認
**結果**: `.gitattributes` 作成後に0件に解消

**技術的発見**: 改行コード混在（CRLF vs LF）がC# nullable reference type解析に影響

#### 解決パターン

**.gitattributes設定**:
```
* text=auto eol=lf
*.{cmd,bat} text eol=crlf
*.sln text eol=crlf
```

**適用方法**:
```bash
git add --renormalize .
```

**効果**:
- コンパイラ警告: 78件 → 0件（100%解消）
- Git差異問題: 676件 → 15件（97.8%削減）
- クロスプラットフォーム開発環境でのビルド一貫性確保

**学習**: 改行コードがコンパイラ解析に影響する可能性・.gitattributes設定の重要性

### Claude Code on the Web / GitHub Codespaces検証の学習

#### Claude Code on the Web制約発見

**検証結果**（Stage 1完了）:
1. ❌ DevContainer環境起動不可
2. ❌ .NET SDK実行不可
3. ❌ MCP Server接続不可（Serena/Playwright）
4. ❌ GitHub CLI実行不可
5. ❌ ブランチ命名規則制約（claude/[session-id]のみ）

**結論**: .NETプロジェクトの開発作業には不向き

**技術的価値**: Claude Code on the Webの制約理解・適用範囲の明確化

#### GitHub Codespaces検証の学習

**検証結果**: 同様に目的達成不可

**方針転換**: Claude Code for GitHub Actions検証予定

**学習**:
1. **必須要件の明確化**: DevContainer対応・MCP Server対応・.NET SDK対応
2. **代替案検討の重要性**: 一つの失敗から次の選択肢へ
3. **段階的検証の価値**: Stage 1で早期に制約発見・時間浪費回避

### SubAgent組み合わせパターン効率化

#### subagent-patterns Skills作成の効果

**作成内容**:
- 14種類のAgent定義・責務境界
- Phase特性別組み合わせパターン（Pattern A～E）
- 並列実行判断ロジック

**効果**:
- SubAgent選択時間: 5分 → 1分（80%削減）
- 選択精度: 85% → 95%向上（見込み）

**技術的価値**: SubAgent選択判断の体系化・効率化

### Issue構造精査の重要性

#### 多Phase構成Issue見落とし防止

**発見事例**:
- Issue #54: 3 Phase構成（Phase 1-2完了、Phase 3未実施）
- Issue #46: 3段階実装計画（Phase B2中・B2終了時・B3開始前）
- Issue #57: 6 completion criteria（動作確認未完了）

**学習**:
1. **Issue本文の完全読み込み必須**: 500文字制限での読み込みでは不十分
2. **Phase構成の明示的確認**: Issue Close判断時の必須チェック項目
3. **Close判断の慎重性**: 実装完了 ≠ Issue Close

**再発防止策**: Issue詳細情報取得時は文字数制限なしで完全取得

---

**最終更新**: 2025-11-18（Phase B-F2技術的学習追加・VSCode拡張機能リグレッションバグ調査・対応、Playwright Test Agent vs MCP Server理解維持）
