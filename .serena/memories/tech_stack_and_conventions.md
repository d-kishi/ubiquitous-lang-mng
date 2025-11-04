# 技術スタック・規約

## 🤖 Agent Skills参照方法（2025-10-21新設・Phase 1導入完了）

### F#↔C#型変換パターンの参照

**従来**: tech_stack_and_conventionsメモリーから参照

**Phase 1以降**: `.claude/skills/fsharp-csharp-bridge/` から自律的に参照

**変更理由**: 
- Agent SkillsによりClaude Codeが自律的に適用
- 効果測定の正確性確保

**詳細ファイル**:
1. `.claude/skills/fsharp-csharp-bridge/patterns/result-conversion.md` - Result型変換パターン
2. `.claude/skills/fsharp-csharp-bridge/patterns/option-conversion.md` - Option型変換パターン
3. `.claude/skills/fsharp-csharp-bridge/patterns/du-conversion.md` - Discriminated Union変換パターン
4. `.claude/skills/fsharp-csharp-bridge/patterns/record-conversion.md` - Record型変換パターン

### Clean Architecture準拠性の参照

**従来**: ADR_010・ADR_019から参照

**Phase 1以降**: `.claude/skills/clean-architecture-guardian/` から自律的に参照

**変更理由**: 
- Agent SkillsによりClaude Codeが自律的にチェック
- Phase B1で97点品質を達成した知見の自動維持

**詳細ファイル**:
1. `.claude/skills/clean-architecture-guardian/rules/layer-separation.md` - レイヤー分離原則
2. `.claude/skills/clean-architecture-guardian/rules/namespace-design.md` - namespace設計規約

---

## アーキテクチャ構成

### Clean Architecture構成
```
Web (C# Blazor Server) → Contracts (C# DTOs/TypeConverters) → Application (F# UseCases) → Domain (F# Models)
                      ↘ Infrastructure (C# EF Core/Repository) ↗
```

### 技術スタック
- **Frontend**: Blazor Server + Bootstrap 5 + SignalR
- **Backend**: ASP.NET Core 8.0 + Entity Framework Core 8.0
- **Domain/Application**: F# 8.0 + 関数型プログラミング
- **Database**: PostgreSQL 16 (Docker Container)
- **認証**: ASP.NET Core Identity
- **テスト**: xUnit + FsUnit + Moq + WebApplicationFactory + bUnit (Blazor Component Testing)
- **E2Eテスト**: Playwright for .NET + **Playwright MCP統合完了**（2025-10-17・Claude Code直接統合・25ツール利用可能）
- **⭐Agent Skills**: Phase 1導入完了（2025-10-21・fsharp-csharp-bridge + clean-architecture-guardian）

---

## 開発環境構成（2025-11-04確定）

### 🔴 CRITICAL: Claude Code実行環境

**Claude Code実行環境**: Windows 11ホスト環境（WSL2上ではない）
**DevContainer**: Sandboxモード環境として機能（セキュリティ分離）
**設定ファイル**: `.claude/settings.local.json`（sandbox.enabled: true）
**方針**: A方針（ホスト実行 + DevContainer Sandbox）採用

**重要な理解**:
- Claude Code CLIはホスト環境で実行
- dotnet/docker等のコマンドは自動的にDevContainer内で実行される
- bubblewrap/psql等のLinux専用ツールはホスト環境では直接確認不要

### DevContainer + Sandboxモード統合

**効果**: 
- セットアップ時間96%削減（75-140分 → 5-8分）
- 承認プロンプト84%削減（30-50回/Phase → 5-8回/Phase）

**詳細**: `Doc/99_Others/Claude_Code_Sandbox_DevContainer技術解説.md`  
**決定記録**: ADR_025（Doc/07_Decisions/ADR_025_DevContainer_Sandboxモード統合.md）

### DevContainer環境仕様

- **ベースイメージ**: mcr.microsoft.com/dotnet/sdk:8.0
- **.NET SDK**: 8.0.415
- **F# Runtime**: .NET SDK同梱（バージョン8.0）
- **Node.js**: 24.x Active LTS（ホスト環境と統一）
- **bubblewrap**: Sandboxセキュリティツール
- **PostgreSQL Client**: psql 16

### VS Code拡張機能自動インストール（15個）

- **基本開発環境（4個）**: C#, F#, Playwright, Remote Containers
- **.NET開発必須（4個）**: C# Dev Kit, .NET Runtime, Test Explorer, EditorConfig
- **開発効率向上（5個）**: GitLens, Docker, Path Intellisense, Markdown All in One, 日本語言語パック
- **AI支援（2個）**: GitHub Copilot, GitHub Copilot Chat

### 接続文字列調整

- **ホスト環境**: `Host=localhost;Port=5432;...`
- **DevContainer環境**: `Host=postgres;Port=5432;...`（docker-compose service名参照）
- **自動設定**: devcontainer.jsonのremoteEnv環境変数で自動設定済み

### クロスプラットフォーム対応

- **改行コード統一**: `.gitattributes`作成（リポジトリ内LF統一、作業ディレクトリOS標準）
- **git設定**: `core.autocrlf`の差異をgitattributesで吸収
- **重要発見**: 改行コード混在（CRLF vs LF）がC#コンパイラのnullable reference type解析に影響する

### 技術負債

- **CS8600/CS8625等78 warnings**: DevContainer環境特有のnullable reference type警告（GitHub Issue #62記録済み）

---

## DevContainer開発環境規約（2025-11-03確立・Phase B-F2 Step4）

### VSCode拡張機能標準セット（15個）

**設定場所**: `.devcontainer/devcontainer.json` の `extensions` 配列

**基本開発環境（4個）**:
- `ms-dotnettools.csharp` - C#言語サポート
- `ionide.ionide-fsharp` - F#言語サポート
- `ms-playwright.playwright` - Playwright E2Eテスト統合
- `ms-vscode-remote.remote-containers` - DevContainer統合

**.NET開発必須（4個）**:
- `ms-dotnettools.csdevkit` - C# Dev Kit（包括的C#開発ツール）
- `ms-dotnettools.vscode-dotnet-runtime` - .NET Runtimeマネージャー
- `formulahendry.dotnet-test-explorer` - テストエクスプローラー
- `editorconfig.editorconfig` - EditorConfig対応（コーディング規約統一）

**開発効率向上（5個）**:
- `eamodio.gitlens` - Git履歴・Blame可視化
- `ms-azuretools.vscode-docker` - Docker統合
- `christian-kohler.path-intellisense` - パス補完
- `yzhang.markdown-all-in-one` - Markdownプレビュー・編集支援
- `ms-ceintl.vscode-language-pack-ja` - 日本語言語パック

**AI支援（2個）**:
- `github.copilot` - GitHub Copilot（AI ペアプログラミング）
- `github.copilot-chat` - GitHub Copilot Chat（AI 対話支援）

**重要**: DevContainer内で拡張機能を手動インストールしても `devcontainer.json` には自動記録されない。必ず手動で追加すること。

### クロスプラットフォーム改行コード規約

**設定場所**: `.gitattributes` (2025-11-03追加)

**背景**:
- Windows（CRLF）とLinux（LF）の改行コード混在により、C# nullable reference type解析が影響を受ける
- Phase B-F2 Step4で78個の警告（CS8600, CS8625, CS8602, CS8604, CS8620）が発生したが、`.gitattributes` 追加後に0件に解消

**重要発見**: 改行コード混在（CRLF vs LF）がC#コンパイラのnullable reference type解析に影響する

**適用方法**:
```bash
# .gitattributes作成後、既存ファイルに適用
git add --renormalize .
```

**設定内容**: テキストファイルは全てLF改行、バイナリファイルは変更なし

**効果**:
- クロスプラットフォーム開発環境でのビルド一貫性確保
- コンパイラ警告の排除（78件 → 0件）
- Git差異問題解決（676件 → 15件）

---

## PostgreSQL 識別子規約（2025-10-26確立・重要）

### 🔴 必須ルール: 全識別子Quote必須

**背景**: PostgreSQL識別子正規化動作（Unquoted識別子 → 小文字変換）

**問題事例**（Phase B2で発見）:
- `CREATE TABLE AspNetUsers` → `aspnetusers`テーブル作成（意図しない重複テーブル発生）
- `INSERT INTO AspNetUsers` → `aspnetusers`テーブルへ挿入（既存`"AspNetUsers"`テーブルは未使用）
- 結果: 27テーブル作成（15正常 + 12重複小文字）

**解決策**: 全識別子を`""`でQuote

```sql
-- ❌ 誤り（小文字化される）
CREATE TABLE AspNetUsers (
    Id VARCHAR(450),
    UserName VARCHAR(256)
);

-- ✅ 正しい（大文字小文字保持）
CREATE TABLE "AspNetUsers" (
    "Id" VARCHAR(450),
    "UserName" VARCHAR(256)
);
```

### 必須適用箇所

1. **CREATE TABLE**: テーブル名・全列名
2. **INSERT INTO**: テーブル名・全列名
3. **FOREIGN KEY**: 参照テーブル名・参照列名
4. **CREATE INDEX**: テーブル名・列名
5. **COMMENT ON**: テーブル名・列名（`"TableName"."ColumnName"`形式）

### COMMENT文の正しい形式

```sql
-- ❌ 誤り
COMMENT ON TABLE AspNetUsers IS 'ユーザー情報';
COMMENT ON COLUMN AspNetUsers.Id IS 'ユーザーID';

-- ✅ 正しい
COMMENT ON TABLE "AspNetUsers" IS 'ASP.NET Core Identity ユーザー情報';
COMMENT ON COLUMN "AspNetUsers"."Id" IS 'ユーザーID（主キー、GUID形式）';
```

### 参考ファイル

- `init/01_create_schema.sql` - 全識別子Quote済み（2025-10-26修正）
- `init/02_initial_data.sql` - 全INSERT文Quote済み（2025-10-26修正）

---

## F#↔C# 型変換パターン（Phase B1 Step7確立・2025-10-05）

**重要**: 詳細は`.claude/skills/fsharp-csharp-bridge/`に移行（Phase 1・2025-10-21）

### F# Result型のC#統合パターン

**詳細**: `.claude/skills/fsharp-csharp-bridge/patterns/result-conversion.md`

**概要**:
- **IsOk/ResultValueアクセスパターン**（推奨）
- NewOk/NewError生成パターン
- Railway-oriented Programming統合

### F# Option型のC#統合パターン

**詳細**: `.claude/skills/fsharp-csharp-bridge/patterns/option-conversion.md`

**概要**:
- Some/None生成パターン
- IsSome/Valueアクセスパターン
- null許容型変換パターン

### F# Discriminated Union ↔ C# 統合パターン

**詳細**: `.claude/skills/fsharp-csharp-bridge/patterns/du-conversion.md`

**概要**:
- switch式パターンマッチング
- Role型（Discriminated Union）のC#統合
- Enumとの違い（重要）

### F# Record型 ↔ C# 統合パターン

**詳細**: `.claude/skills/fsharp-csharp-bridge/patterns/record-conversion.md`

**概要**:
- コンストラクタベース初期化パターン（必須）
- camelCaseパラメータ使用
- Read-onlyプロパティ対応

---

## MCP（Model Context Protocol）仕様・メンテナンスパターン（2025-11-02確立）

**確立日**: 2025-11-02（Phase B-F2 Step3 MCPメンテナンス機能追加時）

### MCP仕様理解

#### JSON-RPC活用（ツール一覧取得）

**ツール一覧取得方法**:
```bash
# Playwright MCP
echo '{"jsonrpc": "2.0", "id": 1, "method": "tools/list"}' \
  | npx @playwright/mcp@latest \
  | jq '.result.tools[].name'
```

**目的**:
- MCPツール数の正確な確認
- ツール追加/廃止の検出
- SubAgent定義の正確性維持

**実績**:
- Phase B-F2 Step3で Playwright MCP 21ツール確認（当初25ツール想定→21ツールに修正）
- e2e-test Agent定義の完全版実装

#### Claude SubAgent仕様（ワイルドカード非対応）

**仕様**:
- SubAgent定義の`tools`セクションでワイルドカード非対応
- 全ツールを明示的に列挙する必要あり

**影響**:
- e2e-test Agent定義: 9ツール記載 → 21ツール完全版に修正
- integration-test Agent定義: 同様の対応必要

**推奨対応**:
- `tools/list`メソッドで全ツール取得
- SubAgent定義に完全版リスト記載
- 週次振り返り時にツール変更確認

### MCPメンテナンスパターン（半自動推奨）

#### 完全自動 vs 半自動の判断

| 方式 | メリット | デメリット | 推奨 |
|------|---------|-----------|------|
| **完全自動** | 運用負荷ゼロ | 破壊的変更リスク・意図しない変更 | ❌ 非推奨 |
| **半自動** | 安全性確保・意図的更新 | 5-10分/週の運用負荷 | ✅ 推奨 |
| **手動** | 完全なコントロール | 見逃しリスク・運用負荷大 | ❌ 非推奨 |

**半自動方式の詳細**: `.claude/commands/weekly-retrospective.md` - Section 11（MCP更新確認）

#### 週次メンテナンスフロー

**実施タイミング**: 週次振り返り時（weekly-retrospective Command実行時）

**手順**:
1. **バージョン確認**:
   ```bash
   # Playwright MCP
   npx @playwright/mcp@latest --version
   npm view @playwright/mcp version

   # Serena MCP
   gh api repos/oraios/serena/releases/latest
   ```

2. **ツール変更検出**:
   ```bash
   # Playwright MCP tools/list実行
   echo '{"jsonrpc": "2.0", "id": 1, "method": "tools/list"}' \
     | npx @playwright/mcp@latest \
     | jq '.result.tools[].name'
   ```

3. **変更レポート作成**:
   - 新規バージョンの有無
   - ツール追加/廃止/非推奨の検出
   - 影響範囲の評価（SubAgent定義への影響）

4. **SubAgent定義更新判断**:
   - ユーザーにレポート提示
   - 更新が必要な場合: 手動編集実施
   - 更新不要の場合: スキップ

**期待運用コスト**: 5-10分/週

### ADR_024参照

**MCPメンテナンス手順の完全版**: `Doc/07_Decisions/ADR_024_Playwright専用SubAgent新設決定.md`

**内容**:
- 5段階手順（バージョン確認・リリースノート確認・ツール変更検出・SubAgent定義更新判断・動作検証）
- 週次振り返り連携方法
- トラブルシューティング（ツール廃止時・新規ツール追加時）

---

## ADR vs Skills判断基準の実証（2025-11-02確立）

**確立日**: 2025-11-02（Phase B-F2 Step2-3実施時）

### 判断基準（30秒チェック）

**詳細**: `.serena/memories/development_guidelines.md` - Section「ADR vs Agent Skills 判断基準」

**簡潔版**:
1. **歴史的記録が必要か？**（なぜこの決定をしたか） → ADR作成
2. **Claudeが自律的に適用すべきか？**（実装時に自動適用） → Skills作成
3. **技術選定の根拠か？**（代替案との比較・リスク評価） → ADR作成
4. **実装パターン・チェックリストか？**（繰り返し使うパターン） → Skills作成

### Phase B-F2実証事例

#### Step2: ADR/Rules → Skills migration

**移行ファイル**:
1. `仕様準拠ガイド.md` → `spec-compliance-auto` Skill
2. `SubAgent組み合わせパターン.md` → `subagent-patterns` Skill

**評価結果**:
- ✅ 適切な判断（ADR vs Skills判断基準に準拠）
- ✅ "why"（判断根拠）はADR、"how"（適用方法）はSkillsの分離原則確認

#### Step3: ADR_024作成（簡潔版）

**作成方針**:
- **判断根拠のみ記載**（簡潔版）
- **詳細はSkillsに記載**（playwright-e2e-patterns, subagent-patterns）

**ADR_024内容**:
- E2E専用SubAgent新設決定
- 5点の判断根拠（ADR_020整合性・レイヤー分離・技術スタック・Skill参照・MCP連携）
- 詳細実装パターンはSkillsに記載

**効果**:
- ADR肥大化防止（簡潔版ADR）
- Skills自律適用の実現（詳細パターン）
- 重複記載の削減

### 期待効果

- ✅ **明確な役割分担**: ADR（判断根拠）vs Skills（適用方法）の分離
- ✅ **ADR肥大化防止**: 判断根拠のみ記載・詳細はSkills
- ✅ **Skills自律適用**: Claudeが詳細パターンを自動適用

---

**最終更新**: 2025-11-04（**Week 44週次振り返り完了・MCP仕様/ADR vs Skills判断基準追加**）
**重要変更**: MCP仕様・メンテナンスパターン追加（JSON-RPC活用・半自動メンテナンス・ADR_024参照）、ADR vs Skills判断基準実証追加（Phase B-F2実証事例）
