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

（以下、既存の tech_stack_and_conventions 内容を維持）

## プロジェクト構成
...
（既存内容省略）
...

---

**最終更新**: 2025-10-21（**Agent Skills Phase 1導入完了・Skills参照方法追記**）
**重要変更**: F#↔C#型変換パターンの詳細を`.claude/skills/fsharp-csharp-bridge/`に移行
