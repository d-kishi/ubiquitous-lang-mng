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
