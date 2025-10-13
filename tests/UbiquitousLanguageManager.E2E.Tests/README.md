# E2E.Tests

## 概要
エンドツーエンドテストプロジェクト

## Phase B2実装予定
- Playwright MCP統合（Claude Code統合推奨度: 9/10点）
- Playwright Agents統合（自律的修復・7/10点）
- UserProjects E2Eテスト実装

## Playwright MCP + Agents統合戦略
**統合推奨度**: ⭐⭐⭐⭐⭐ 10/10点（最強の相乗効果）

### Playwright MCP（優先度: 高）
- **目的**: AI Agentのブラウザ操作能力付与
- **効果**: テスト作成効率75-85%向上
- **導入**: 5秒（1コマンド: `claude mcp add playwright`）

### Playwright Agents（優先度: 中）
- **目的**: テストの自律的生成・修復
- **効果**: メンテナンス効率50-70%削減
- **導入**: 15-30分（VS Code Insiders設定）

### 期待効果（Phase B2以降）
- 作成効率75%↑ + メンテナンス効率75%↑ = **総合85%効率化**
- Phase B全体で10-15時間削減

## 実装状況
- [x] プロジェクト作成（Phase B-F1 Step4）
- [ ] Playwright MCP統合（Phase B2実施予定）
- [ ] Playwright Agents統合（Phase B2実施予定）
- [ ] UserProjects E2Eテスト実装（Phase B2実施予定）

## テスト対象
- Web層（Blazor Server UI）
- 全層統合（フルスタックE2Eテスト）
- ユーザーシナリオテスト

## 技術スタック
- xUnit
- Microsoft.Playwright（Phase B2統合予定）
- Microsoft.AspNetCore.Mvc.Testing
- Playwright MCP（Phase B2統合予定）
- Playwright Agents（Phase B2統合予定）

## 参照関係
- Web層（全層参照可）
- Infrastructure層
- Application層
- Domain層
- Contracts層

## 参考ドキュメント
- `Doc/08_Organization/Active/Phase_B-F1/Research/Playwright_MCP_評価レポート.md`
- `Doc/08_Organization/Active/Phase_B-F1/Research/Playwright_Agents_評価レポート.md`
- `Doc/08_Organization/Rules/Phase_B2_Playwright_統合戦略.md`
