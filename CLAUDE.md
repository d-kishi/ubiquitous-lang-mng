# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## 🔴 CRITICAL: プロセス遵守絶対原則（ADR_016）

**違反は一切許容されない重要遵守事項**:

### 絶対遵守原則
- **コマンド = 契約**: 一字一句を法的契約として遵守・例外なし
- **承認 = 必須**: 「ユーザー承認」表記は例外なく取得・勝手な判断禁止
- **手順 = 聖域**: 定められた順序の変更禁止・先回り作業禁止

### 禁止行為（重大違反）
- ❌ **承認前の作業開始**: いかなる理由でも禁止
- ❌ **独断での判断**: 「効率化」を理由とした勝手な作業
- ❌ **成果物の虚偽報告**: 実体のない成果物の報告
- ❌ **コマンド手順の無視**: phase-start/step-start等の手順飛ばし

### 必須実行事項
- ✅ **実体確認**: SubAgent成果物の物理的存在確認
- ✅ **承認記録**: 取得した承認の明示的記録  
- ✅ **チェックリスト実行**: 組織管理運用マニュアルのプロセス遵守チェック

**詳細**: `/Doc/07_Decisions/ADR_016_プロセス遵守違反防止策.md`

## 🤖 CRITICAL: 自動Command実行指示

**以下の宣言を検出した際、該当Commandを自動実行せよ**:

- **セッション開始**: 「セッションを開始します」「セッション開始」 → **`.claude/commands/session-start.md`** 自動実行
- **セッション終了**: 「セッション終了」「セッションを終了します」 → **`.claude/commands/session-end.md`** 自動実行
  - **🔴 必須**: 日次記録作成・プロジェクト状況更新・Serenaメモリー5種類更新の実行証跡確認必須
- **週次振り返り**: 「週次振り返り」「振り返り実施」「今週の振り返り」「振り返りを実施」 → **`.claude/commands/weekly-retrospective.md`** 自動実行
- **Phase開始準備**: 「Phase開始準備」「新Phase準備」「PhaseXXの実行を開始してください」「PhaseXXを開始してください」「PhaseXX開始準備」「PhaseXXを準備」 → **`.claude/commands/phase-start.md`** 自動実行
- **Step開始準備**: 「Step開始」「次Step開始」「StepXX開始」「Step開始準備」「次のStep準備」「PhaseXX StepYYを開始します」「PhaseXX StepYY開始」「StepYYを開始」「StepYY準備」 → **`.claude/commands/step-start.md`** 自動実行
- **Step終了確認**: 「Step完了」「Step終了」「StepXX完了」「StepXX終了」「PhaseXX StepYY完了」「Step終了確認」 → **`.claude/commands/step-end-review.md`** 自動実行
- **Phase終了処理**: 「Phase完了」「Phase終了」「フェーズ完了」「PhaseXX完了」「PhaseXX終了」「Phase総括実施」 → **`.claude/commands/phase-end.md`** 自動実行
- **仕様準拠確認**: 「仕様準拠確認」「仕様チェック」「spec-compliance実行」 → **`.claude/commands/spec-compliance-check.md`** 自動実行
- **SubAgent選択**: 「SubAgent選択」「Agent選択」「subagent-selection実行」 → **`.claude/commands/subagent-selection.md`** 自動実行

**Serena MCP初期化**: セッション開始時は必ず `mcp__serena__check_onboarding_performed` を実行（ツール呼び出し）

## プロジェクト概要

**ユビキタス言語管理システム** - DDD用語管理Webアプリケーション
- **技術基盤**: Clean Architecture（F# Domain/Application + C# Infrastructure/Web + Contracts層）
- **現在フェーズ**: Phase A1-A6完了（認証・ユーザー管理）、Phase A7実施予定（要件準拠・アーキテクチャ統一）
- **技術負債管理**: GitHub Issues #5, #6で管理（ADR_015準拠）
- **詳細状況**: Serenaメモリー`project_overview`参照

## アーキテクチャ概要

### Clean Architecture構成
```
Web (C# Blazor Server) → Contracts (C# DTOs/TypeConverters) → Application (F# UseCases) → Domain (F# Models)
                      ↘ Infrastructure (C# EF Core/Repository) ↗
```

### 主要技術スタック
- **Frontend**: Blazor Server + Bootstrap 5
- **Backend**: ASP.NET Core 8.0 + Entity Framework Core
- **Domain/Application**: F# 8.0 (関数型プログラミング)
- **Database**: PostgreSQL 16 (Docker Container)
- **認証**: ASP.NET Core Identity

### 重要な設計決定
- **ADR一覧**: `/Doc/07_Decisions/ADR_*.md`
- **用語統一**: 「用語」ではなく「ユビキタス言語」を使用（ADR_003）
- **データベース設計**: `/Doc/02_Design/データベース設計書.md`

## Commands体系

### 自動実行Commands
- **セッション開始/終了**: `.claude/commands/`配下のCommands自動実行
- **SubAgent選択**: `subagent-selection` - 作業に最適なAgent組み合わせ選択
- **品質チェック**: `spec-compliance-check`, `step-end-review`

---

## 🤖 Agent Skills

**目的**: プロジェクト固有の知見・パターン・判断基準をモジュール化し、Claudeが自律的に適用

**配置**: `.claude/skills/`

**Phase 1導入完了**（2025-10-21）:
- **fsharp-csharp-bridge**: F#↔C#型変換パターンの自律的適用
- **clean-architecture-guardian**: Clean Architecture準拠性の自動チェック
- **playwright-e2e-patterns**: Playwright MCP活用によるE2Eテスト作成パターン

### Skills作成判断基準

ADRとAgent Skillsの使い分けについては、以下のガイドラインを参照：

**詳細**: `Doc/08_Organization/Rules/ADRとAgent_Skills判断ガイドライン.md`

**簡潔な判断フロー**（30秒チェック）:
1. **歴史的記録が必要か？**（なぜこの決定をしたか） → ADR作成
2. **Claudeが自律的に適用すべきか？**（実装時に自動適用） → Skills作成
3. **技術選定の根拠か？**（代替案との比較・リスク評価） → ADR作成
4. **実装パターン・チェックリストか？**（繰り返し使うパターン） → Skills作成

**関連ドキュメント**:
- Skills一覧: `.claude/skills/README.md`
- 効果測定: `Doc/08_Organization/Active/AgentSkills_Phase1_効果測定.md`

---

## 実装指針

### 🎯 重要: Blazor Server・F#初学者対応
プロジェクトオーナーが初学者のため、**詳細なコメント必須**（ADR_010参照）
- **Blazor Server**: ライフサイクル・StateHasChanged・SignalR接続の説明
- **F#**: パターンマッチング・Option型・Result型の概念説明

### 🔴 メインエージェント必須遵守事項（ADR_016準拠）
**エラー修正時の責務分担原則** - タイミング問わず適用

#### MainAgent責務定義
```markdown
✅ 実行可能な作業:
- 全体調整・オーケストレーション
- SubAgentへの作業委託・指示
- 品質確認・統合テスト実行
- プロセス管理・進捗管理
- ドキュメント統合・レポート作成

❌ 禁止事項（例外を除く）:
- 実装コードの直接修正
- ビジネスロジックの追加・変更
- 型変換ロジックの実装
- テストコードの作成・修正
- データベーススキーマの変更
```

#### エラー発生時の必須対応原則
1. **エラー内容で責務判定**（発生場所・タイミング問わず）
2. **責務マッピングでSubAgent選定**：
   - F# Domain/Application層 → fsharp-domain/fsharp-application
   - F#↔C#境界・型変換 → contracts-bridge
   - C# Infrastructure/Web層 → csharp-infrastructure/csharp-web-ui
   - テストエラー → unit-test/integration-test
3. **Fix-Mode活用**：`"[SubAgent名] Agent, Fix-Mode: [修正内容]"`
4. **効率性より責務遵守を優先**

#### 例外（直接修正可能）
- 単純なtypo（1-2文字）
- import文の追加のみ
- コメントの追加・修正
- 空白・インデントの調整

### 🧪 新規テストプロジェクト作成時の必須確認事項

**確認タイミング**（以下のいずれか）:
1. **unit-test/integration-test Agent選択時**（step-start Command実行時）
2. **新規テストプロジェクト作成指示を受けた時**（MainAgent/SubAgent問わず）
3. **tests/配下に新規ディレクトリ・プロジェクトファイル作成前**

**新規テストプロジェクト作成前に以下を必ず確認すること**（GitHub Issue #40再発防止策）：

1. **ADR_020**: テストアーキテクチャ決定
   - `/Doc/07_Decisions/ADR_020_テストアーキテクチャ決定.md`
   - レイヤー×テストタイプ分離方式の理解

2. **テストアーキテクチャ設計書**: `/Doc/02_Design/テストアーキテクチャ設計書.md`
   - プロジェクト構成図・命名規則・参照関係原則の確認
   - （Issue #40 Phase 3完了後に作成予定）

3. **新規プロジェクト作成チェックリスト**: `/Doc/08_Organization/Rules/新規テストプロジェクト作成ガイドライン.md`
   - 事前確認・プロジェクト作成・参照関係設定・ビルド確認・ドキュメント更新の全手順実施
   - （Issue #40 Phase 3完了後に作成予定）

**命名規則（厳守）**: `UbiquitousLanguageManager.{Layer}.{TestType}.Tests`
- **Layer**: Domain / Application / Contracts / Infrastructure / Web
- **TestType**: Unit / Integration / UI / E2E

**参照関係原則**:
- **Unit Tests**: テスト対象レイヤーのみ参照
- **Integration Tests**: 必要な依存層のみ参照
- **E2E Tests**: 全層参照可

## 開発コマンド（DevContainer環境）

**🔴 重要**: 本プロジェクトはDevContainer環境で開発します。以下のコマンドは全てDevContainer内で実行してください。

### コマンド実行方法

#### 方法A: VS Code統合ターミナル（推奨）

VS CodeでDevContainerを開いた状態で、統合ターミナル（Ctrl+`）から直接実行：

```bash
# ビルド
dotnet build
dotnet build src/UbiquitousLanguageManager.Web

# 実行
dotnet run --project src/UbiquitousLanguageManager.Web

# テスト
dotnet test
dotnet test --filter "FullyQualifiedName~UserTests"

# データベース
dotnet ef migrations add MigrationName --project src/UbiquitousLanguageManager.Infrastructure
dotnet ef database update --project src/UbiquitousLanguageManager.Infrastructure
```

#### 方法B: ホスト環境から明示的実行（Claude Code用）

**暫定対応**: Windows環境ではClaude Code Sandboxモードが非対応のため、以下の形式でDevContainer内実行を明示：

```bash
# ビルド
docker exec ubiquitous-lang-mng_devcontainer-devcontainer-1 dotnet build
docker exec ubiquitous-lang-mng_devcontainer-devcontainer-1 dotnet build src/UbiquitousLanguageManager.Web

# 実行
docker exec ubiquitous-lang-mng_devcontainer-devcontainer-1 dotnet run --project src/UbiquitousLanguageManager.Web

# テスト
docker exec ubiquitous-lang-mng_devcontainer-devcontainer-1 dotnet test
docker exec ubiquitous-lang-mng_devcontainer-devcontainer-1 dotnet test --filter "FullyQualifiedName~UserTests"

# データベース
docker exec ubiquitous-lang-mng_devcontainer-devcontainer-1 dotnet ef migrations add MigrationName --project src/UbiquitousLanguageManager.Infrastructure
docker exec ubiquitous-lang-mng_devcontainer-devcontainer-1 dotnet ef database update --project src/UbiquitousLanguageManager.Infrastructure
```

### Docker環境管理

```bash
# PostgreSQL/PgAdmin/Smtp4dev起動（ホスト環境で実行）
docker-compose up -d

# 停止
docker-compose down

# DevContainer確認
docker ps --filter "name=devcontainer"
```

### 開発ツールURL
- **アプリ**: https://localhost:5001
- **PgAdmin**: http://localhost:8080 (admin@ubiquitous-lang.com / admin123)
- **Smtp4dev**: http://localhost:5080

### 暫定対応について

**注意**: 現在、Windows環境ではClaude Code Sandboxモードが非対応のため、方法Bを使用しています。

将来Sandboxモードが対応された際は、`docker exec`プレフィックスを省略して直接実行可能になります。

**関連情報**:
- GitHub Issue #63「Windows環境でのClaude Code Sandboxモード非対応に伴うDevContainer手動実行対応」
- ADR_025「DevContainer + Sandboxモード統合採用」

## プロジェクト構成

### ソースコード
```
src/
├── UbiquitousLanguageManager.Domain/       # F# ドメインモデル
├── UbiquitousLanguageManager.Application/  # F# ユースケース
├── UbiquitousLanguageManager.Contracts/    # C# DTO/TypeConverters
├── UbiquitousLanguageManager.Infrastructure/ # C# EF Core/Repository
└── UbiquitousLanguageManager.Web/         # C# Blazor Server
```

### ドキュメント
```
Doc/
├── 01_Requirements/   # 要件・仕様書
├── 02_Design/        # 設計書
├── 04_Daily/         # 作業記録
├── 06_Issues/        # 課題管理
├── 07_Decisions/     # ADR
└── 10_Debt/          # 技術負債
```

## 重要事項

- **用語統一**: 「用語」ではなく「ユビキタス言語」を使用
- **完全ビルド維持**: 0 Warning, 0 Error状態を保つ
- **テストファースト**: TDD実践・Red-Green-Refactorサイクル
- **技術決定記録**: 重要決定はADRとして記録

## 権限設定（作業効率化）

### 自動承認設定
`.claude/settings.local.json`にて以下の操作を自動承認設定済み：
- **ファイル操作**: カレントディレクトリ配下の全Read/Write/Edit操作
- **Bashコマンド**: dotnet, git, npm, docker等の開発コマンド全般
- **MCP Serena**: 全シンボル操作・メモリ管理操作
- **設定モード**: `defaultMode: "acceptEdits"`により編集操作を自動承認

これにより、Phase実装作業において承認待ちによる中断を最小化し、作業効率を30-40%改善。

### 権限追加方法
新たなコマンドを追加する場合は`.claude/settings.local.json`の`permissions.allow`配列に追加。
例: `"Bash(新コマンド:*)"`

## 開発手法

- **スクラム開発**: 1-2週間スプリント（ADR_011）
- **SubAgentプール方式**: 並列実行による効率化（ADR_013）
- **詳細**: `/Doc/08_Organization/Rules/`参照

## Claude Code実行環境・Sandboxモード統合（2025-11-03確定）

### 🔴 CRITICAL: Claude Code実行場所の理解

**標準構成（A方針・本プロジェクト採用）**: Claude Codeはホスト環境で実行
- **Claude Code CLI**: Windows 11ホスト環境で起動（WSL2上ではない）
- **DevContainer**: Sandboxモード環境として機能（セキュリティ分離）
- **Sandboxモード**: Claude Codeがコマンド実行時にDevContainer内で安全に実行
- **設定ファイル**: `.claude/settings.local.json`でSandbox有効化済み

### Sandboxモード動作フロー
```
1. ユーザーがホスト環境でClaude Code CLIを起動
2. `.claude/settings.local.json`の`sandbox.enabled: true`を読み込み
3. dotnet/docker等のコマンド実行時、DevContainer内で自動実行
4. ファイル操作・ビルド・テスト実行は全てSandbox環境で分離実行
```

### 非標準構成（B方針・非推奨）
DevContainer内でClaude Code CLIを実行する構成は技術的に可能だが：
- ❌ 複雑性増加（MCP Server設定・権限管理）
- ❌ Sandbox二重化（意味なし）
- ❌ Windows環境では非標準・サポート不足
- ⚠️ **本プロジェクトでは採用しない**

### 環境確認方法
```bash
# ホスト環境で実施（Windows PowerShell/Git Bash）
dotnet --version       # .NET SDK確認
docker --version       # Docker Desktop確認
node --version         # Node.js確認

# DevContainer内で実施（VS Code Terminal）
# "Reopen in Container"後に実行
dotnet --version       # .NET SDK 8.0.415確認
dotnet build           # ビルド確認（Sandbox環境）
dotnet test            # テスト実行（Sandbox環境）
```

### ロールバック手順
DevContainer導入前の環境に戻す場合：
1. VS Code左下の緑色ボタン「><」をクリック
2. 「Reopen Folder Locally」を選択
3. ホスト環境に復帰（30分以内）

**詳細技術解説**: `Doc/99_Others/Claude_Code_Sandbox_DevContainer技術解説.md`
**決定記録**: ADR_025（Doc/07_Decisions/ADR_025_DevContainer_Sandboxモード統合.md）

## Context管理・セッション継続判断（2025-10-13策定）

### 🧠 AutoCompact buffer理解

**AutoCompact buffer**: 45,000トークン（22.5%）の予約領域
- **目的**: 応答生成余裕・自動圧縮ワーキングスペース・中断防止
- **v2.0改善**: 重要情報の保持精度向上（コード変更・技術決定保持）
- **既知問題**: `/context`表示のバッファ二重カウント（Issue #8479）

### 📊 セッション継続判断（80%ルール）

```yaml
継続（AutoCompact活用）:
  - Context使用率 < 80% (160k/200k未満)
  - 実装作業中（Step実行中）
  - 60分毎に /context 確認

手動compact:
  - 80% ≤ Context < 85%
  - /compact コマンド実行

終了（セッション分割）:
  - Phase/Step境界
  - 重要な技術決定直後
  - Context使用率 ≥ 85%
```

### ⚠️ ADR_016整合性

**AutoCompact使用時の必須確認**:
- 🔴 SubAgent成果物の物理的存在確認（必須）
- 🔴 プロセス遵守チェックリスト実行
- ✅ 重要決定はADR/Phase_Summary.mdに文書化
- ✅ CLAUDE.mdルールの定期的再確認

**詳細**: `/Doc/08_Organization/Rules/組織管理運用マニュアル.md` - Context管理セクション

## 現在の技術負債

- **TECH-001**: ASP.NET Core Identity設計見直し
- **TECH-002**: 初期スーパーユーザーパスワード不整合
- **TECH-003**: ログイン画面重複
- **TECH-004**: 初回ログイン時パスワード変更未実装
- **詳細**: `/Doc/10_Debt/Technical/`参照