# 技術スタック・規約

**最終更新**: 2025-11-18（**Serenaメモリスリム化実施・詳細内容移行完了**）

---

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
- **E2Eテスト**: TypeScript/Playwright Test（Phase B-F2 Step6でTypeScript移行完了）
- **⭐Agent Skills**: Phase 1-3導入完了（計8個Skills確立）

---

## 開発環境構成（2025-11-04確定）

### 🔴 CRITICAL: Claude Code実行環境

**原則**: Claude Code CLIはホスト環境で実行、DevContainerはSandboxモード環境として機能

**例**: dotnet/dockerコマンド実行時、自動的にDevContainer内で実行される（`.claude/settings.local.json` の `sandbox.enabled: true`）

**詳細**: `CLAUDE.md` - DevContainer環境仕様詳細（コンテナ仕様・VS Code拡張機能15個・接続文字列・クロスプラットフォーム対応）

### DevContainer + Sandboxモード統合効果

**原則**: セットアップ時間96%削減・承認プロンプト84%削減

**例**: セットアップ時間 75-140分 → 5-8分、承認プロンプト 30-50回/Phase → 5-8回/Phase

**詳細**:
- `Doc/99_Others/Claude_Code_Sandbox_DevContainer技術解説.md` - 技術解説
- ADR_025（`Doc/07_Decisions/ADR_025_DevContainer_Sandboxモード統合.md`） - 決定記録

---

## PostgreSQL 識別子規約（2025-10-26確立・重要）

### 🔴 必須ルール: 全識別子Quote必須

**原則**: PostgreSQL識別子正規化動作により、Unquoted識別子は小文字に変換される

**例**:
```sql
-- ❌ 誤り: CREATE TABLE AspNetUsers → aspnetusersテーブル作成
-- ✅ 正しい: CREATE TABLE "AspNetUsers" → AspNetUsersテーブル作成（大文字小文字保持）
```

**詳細**: `Doc/02_Design/データベース設計書.md` - Section 9（PostgreSQL識別子規約）

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

## MCP（Model Context Protocol）仕様・メンテナンス（2025-11-02確立）

### MCP仕様理解の原則

**原則**: JSON-RPC `tools/list`メソッドでツール一覧を取得し、SubAgent定義の正確性を維持

**例**:
```bash
# Playwright MCP ツール一覧取得
echo '{"jsonrpc": "2.0", "id": 1, "method": "tools/list"}' \
  | npx @playwright/mcp@latest | jq '.result.tools[].name'
```

**詳細**: `Doc/08_Organization/Rules/開発手法詳細ガイド.md` - MCP仕様・メンテナンス

### MCPメンテナンスパターン（半自動推奨）

**原則**: 週次振り返り時に半自動メンテナンス（5-10分/週の運用負荷で安全性確保）

**例**: バージョン確認 → ツール変更検出 → 変更レポート作成 → SubAgent定義更新判断

**詳細**:
- `Doc/08_Organization/Rules/開発手法詳細ガイド.md` - 週次メンテナンスフロー
- `Doc/07_Decisions/ADR_024_Playwright専用SubAgent新設決定.md` - 5段階手順・トラブルシューティング

---

## ADR vs Skills判断基準の実証（2025-11-02確立）

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

---

## Playwright Test Agents統合パターン（2025-11-17追加）

**目的**: SubAgent間呼び出し制限を遵守したPlaywright Test Agents統合運用

**技術制約**: Claude Code公式仕様
- **SubAgent制限**: "subagents cannot spawn other subagents"
- **理由**: 無限ネスティング防止
- **参照**: https://code.claude.com/docs/en/sub-agents

### パターンA: MainAgentオーケストレーション型（推奨）

**適用条件**:
- 新規E2Eテストスイート作成
- 複雑なシナリオ（5シナリオ以上）
- 大幅なテスト変更が必要

**フロー**:
```
MainAgent
  ├─ Task(playwright-test-planner) → テスト計画生成（該当時）
  ├─ Task(playwright-test-generator) → TypeScriptテスト生成（該当時）
  ├─ Task(e2e-test) → テスト実行・統合検証
  └─ Task(playwright-test-healer) → 失敗時の修復（該当時）
```

**効率化効果**: 60-70%（Generator/Healer活用時）

**責務分担**:
- **MainAgent**: オーケストレーション・Agent間調整・統合判断
- **playwright-test-generator**: TypeScriptテストコード生成
- **e2e-test**: テスト実行・検証・品質確認
- **playwright-test-healer**: 失敗テストの修復

### パターンB: e2e-testスタンドアロン型

**適用条件**:
- 既存E2Eテストのメンテナンス
- 小規模修正（1-2箇所）
- テスト実行・検証のみ

**フロー**:
```
MainAgent → Task(e2e-test) → テスト実装/実行/検証
```

**効率化効果**: 基本（Playwright MCP 21ツール直接使用）

### 関連定義ファイル

1. **e2e-test.md**: e2e-test SubAgent定義（責務・統合パターン記載）
2. **subagent-selection.md**: Pattern D（品質保証段階）に統合パターン記載
3. **subagent-patterns SKILL.md**: e2e-test責務境界・統合パターン詳細
4. **ADR_024**: SubAgent制限・統合パターン技術決定記録
5. **組織管理運用マニュアル.md**: 統合運用ガイド

### 実装時の注意点

1. **e2e-test SubAgentから直接呼び出し不可**:
   - ❌ e2e-test内で`Task(playwright-test-generator)`実行
   - ✅ MainAgentが調整・e2e-testは実行担当

2. **MainAgent責務**:
   - Agent選択・実行順序決定
   - 成果物引継ぎ・統合判断
   - 品質確認・承認取得

3. **パターン選択基準**:
   - 新規/大規模 → パターンA
   - メンテナンス/小規模 → パターンB

---

**最終更新**: 2025-11-18（**Serenaメモリスリム化実施・詳細内容移行完了・-69%削減**）
**前回更新**: 2025-11-04（Week 44週次振り返り完了・MCP仕様/ADR vs Skills判断基準追加）
**重要変更**: 詳細内容を既存ファイルに移行（CLAUDE.md・データベース設計書.md・開発手法詳細ガイド.md）、基本原則 + 例示 + 参照形式に再構成
