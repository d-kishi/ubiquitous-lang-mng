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

**最終更新**: 2025-11-04（**Claude Code実行環境・DevContainer + Sandboxモード統合環境構成追加**）
**重要変更**: 開発環境構成セクション追加（Claude Code実行環境・DevContainer環境仕様・接続文字列調整・クロスプラットフォーム対応）
