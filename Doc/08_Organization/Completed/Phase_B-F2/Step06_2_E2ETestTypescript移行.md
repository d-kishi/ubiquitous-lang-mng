# Step06_2: E2EテストTypeScript移行計画

**作成日**: 2025-11-17
**対象**: Phase B-F2 Step6完了後の技術的改善
**目的**: Playwright Generator Agent完全活用のためのE2EテストTypeScript移行

---

## 📋 Executive Summary

### 移行の背景

**現状**:
- E2EテストはC#で実装（AuthenticationTests.cs, UserProjectsTests.cs）
- Playwright Test Agents（Generator/Healer/Planner）はTypeScript専用
- Generator AgentによるTypeScriptテスト生成 → contracts-bridge AgentによるC#変換という2段階プロセス

**課題**:
- Playwright Generator AgentがTypeScriptにしか対応していない
- C# E2Eテストでは Generator Agentの恩恵を直接受けられない
- TypeScript → C# 変換による品質劣化リスク

**提案**:
- **既存C# E2Eテストを完全削除**
- **TypeScriptでE2Eテストを再構築**
- Playwright Generator Agentをフル活用

### 実現可能性評価

**結論**: ✅ **実現可能** ⭐⭐⭐⭐☆ (8/10点)

**根拠**:
- ✅ TypeScript実行環境確立済み（Step6で構築）
- ✅ 技術的実現可能性確認済み（実際に実行成功）
- ✅ Generator Agent活用パターン確立済み（Step6実績）
- ⚠️ TypeScriptテスト品質改善が必要
- ⚠️ ドキュメント更新作業が広範囲

**合計推定時間**: 4-7時間

---

## 🔍 現状調査結果

### 既存C# E2Eテスト

| ファイル | 行数 | 内容 |
|---------|------|------|
| `AuthenticationTests.cs` | 422行 | Phase A認証機能（6シナリオ） |
| `UserProjectsTests.cs` | 279行 | Phase B2ユーザープロジェクト管理機能 |
| `*.csproj` | - | C#プロジェクトファイル |
| **合計** | **701行** | - |

**使用フレームワーク**:
- xUnit（テストフレームワーク）
- Microsoft.Playwright 1.56.0（ブラウザ自動化）
- Microsoft.AspNetCore.Mvc.Testing（WebApplicationFactory）

**参照関係**（現状）:
- Web層（Blazor Server）
- Infrastructure層（EF Core）
- Application層（F# UseCases）
- Domain層（F# Models）
- Contracts層（DTOs/TypeConverters）

### 既存TypeScript E2E環境（Step6で構築済み）

| ファイル | サイズ/内容 | 状態 |
|---------|-----------|------|
| `playwright.config.ts` | 915 bytes | ✅ Playwright設定（Full HD 1920x1080、DevContainer対応） |
| `package.json` | 345 bytes | ✅ npm scripts（test, test:ui, test:debug） |
| `authentication.spec.ts` | 7.5KB | ⚠️ Generator Agent生成・改善必要 |
| `seed.spec.ts` | 1.6KB | ⚠️ Seed Test・改善必要 |

**認識されたテスト**（`npx playwright test --list`）:
- `authentication.spec.ts`: 9シナリオ
- `seed.spec.ts`: 1シナリオ
- **合計**: 10テスト（2ファイル）

**実行結果**:
- ✅ TypeScript Playwrightテストが DevContainer内で実行可能
- ✅ Webアプリケーションへの接続成功
- ⚠️ テスト失敗（logout-button が見つからない）→ テストロジック改善必要

---

## ✅ 技術的実現可能性

### 検証結果

**DevContainer内TypeScriptテスト実行**:
```bash
# Webアプリケーション起動（DevContainer内）
dotnet run --project src/UbiquitousLanguageManager.Web

# TypeScriptテスト実行
cd tests/UbiquitousLanguageManager.E2E.Tests
npx playwright test seed.spec.ts
```

**結果**:
- ✅ TypeScriptテストが正常に認識される
- ✅ Webアプリケーションへの接続成功（https://localhost:5001）
- ⚠️ テストエラー（実行環境の問題ではなく、テストロジックの問題）

**技術的制約**:
- Webアプリケーションも DevContainer内部で起動する必要がある
- `run-e2e-tests.sh`スクリプトが自動起動・自動停止を実現している

**実現可能性**: ✅ **確認完了**

---

## 🏗️ Clean Architecture整合性

### ADR_020との整合性

**現状のADR_020記載**（言語別分離原則）:
```
- F# プロジェクト: Domain.Unit.Tests, Application.Unit.Tests
- C# プロジェクト: Contracts.Unit.Tests, Infrastructure.Unit.Tests,
                  Infrastructure.Integration.Tests, Web.UI.Tests, E2E.Tests
```

**E2E.Tests は C#プロジェクトとして定義されている**。

### 移行後の整合性

**必要な更新**:

| ドキュメント | 更新内容 |
|------------|---------|
| **ADR_020** | 言語別分離原則に**TypeScript**を追加（E2E.Tests専用） |
| **テストアーキテクチャ設計書** | 言語選択ガイドライン更新・実行方法変更（dotnet test → npx playwright test） |
| **新規テストプロジェクト作成ガイドライン** | TypeScript E2Eテスト作成手順追加 |

**更新後のADR_020記載案**:
```
- F# プロジェクト: Domain.Unit.Tests, Application.Unit.Tests
- C# プロジェクト: Contracts.Unit.Tests, Infrastructure.Unit.Tests,
                  Infrastructure.Integration.Tests, Web.UI.Tests
- TypeScript プロジェクト: E2E.Tests（Playwright Generator Agent活用のため）
```

**整合性評価**: ⚠️ **条件付き整合性**
- ADR_020更新により整合性確保可能
- E2EテストのみTypeScript採用は技術的に合理的（Generator Agent活用）
- Clean Architectureの層分離原則には影響なし（E2Eテストは全層横断）

---

## 📊 影響範囲分析

### 影響を受けるファイル一覧

| カテゴリ | ファイル | 更新内容 |
|---------|---------|---------|
| **CI/CDスクリプト** | `tests/run-e2e-tests.sh` | `dotnet test` → `npx playwright test` |
| **Agent定義** | `.claude/agents/e2e-test.md` | TypeScript Playwright前提に書き換え |
| **Skills** | `.claude/skills/playwright-e2e-patterns/SKILL.md` | TypeScript E2Eパターンに更新 |
| | `.claude/skills/playwright-e2e-patterns/patterns/data-testid-design.md` | TypeScript例に更新 |
| | `.claude/skills/playwright-e2e-patterns/patterns/mcp-tools-usage.md` | TypeScript例に更新 |
| | `.claude/skills/playwright-e2e-patterns/patterns/blazor-signalr-e2e.md` | TypeScript例に更新 |
| **ADR** | `Doc/07_Decisions/ADR_020_テストアーキテクチャ決定.md` | 言語別分離原則にTypeScript追加 |
| **設計書** | `Doc/02_Design/テストアーキテクチャ設計書.md` | E2E.Tests言語選択更新・実行方法変更 |
| **ガイドライン** | `Doc/08_Organization/Rules/Playwright_運用統合ガイドライン.md` | TypeScript E2E標準に更新 |
| **開発手順** | `CLAUDE.md` | 開発コマンド説明更新（dotnet test → npm test） |
| **削除対象** | `tests/UbiquitousLanguageManager.E2E.Tests/AuthenticationTests.cs` | C# E2Eテスト完全削除 |
| | `tests/UbiquitousLanguageManager.E2E.Tests/UserProjectsTests.cs` | C# E2Eテスト完全削除 |
| | `tests/UbiquitousLanguageManager.E2E.Tests/UbiquitousLanguageManager.E2E.Tests.csproj` | C#プロジェクトファイル削除 |
| **保持・改善** | `tests/UbiquitousLanguageManager.E2E.Tests/authentication.spec.ts` | TypeScript E2Eテスト改善 |
| | `tests/UbiquitousLanguageManager.E2E.Tests/seed.spec.ts` | TypeScript E2Eテスト改善 |
| | `tests/UbiquitousLanguageManager.E2E.Tests/playwright.config.ts` | 設定見直し |
| | `tests/UbiquitousLanguageManager.E2E.Tests/package.json` | npm scripts整備 |

### 影響を受けるCommands

**直接的な影響なし**:
- Commands自体は`dotnet test`を直接呼び出していない
- `run-e2e-tests.sh`スクリプト経由でテスト実行

**確認済みCommands**:
- `step-end-review.md`
- `session-end.md`
- `phase-end.md`
- `weekly-retrospective.md`

---

## 🗺️ 移行計画（全5 Phase）

### Phase 1: C# E2Eテスト削除・基盤整備（30分）

**目的**: 既存C# E2Eテストの完全削除・TypeScript環境確認

**作業内容**:
1. C# E2Eテストファイル削除
   ```bash
   rm tests/UbiquitousLanguageManager.E2E.Tests/AuthenticationTests.cs
   rm tests/UbiquitousLanguageManager.E2E.Tests/UserProjectsTests.cs
   ```

2. C#プロジェクトファイル削除
   ```bash
   rm tests/UbiquitousLanguageManager.E2E.Tests/UbiquitousLanguageManager.E2E.Tests.csproj
   ```

3. .slnファイル更新
   ```bash
   dotnet sln remove tests/UbiquitousLanguageManager.E2E.Tests/UbiquitousLanguageManager.E2E.Tests.csproj
   ```

4. TypeScriptテスト環境確認
   - `package.json` 確認
   - `playwright.config.ts` 確認
   - `authentication.spec.ts` 保持
   - `seed.spec.ts` 保持

**完了基準**:
- ✅ C# E2Eテストファイルが存在しない
- ✅ C#プロジェクトファイルが存在しない
- ✅ TypeScript環境ファイルが保持されている
- ✅ `dotnet build` 成功（0 Warning / 0 Error）

---

### Phase 2: TypeScript E2Eテスト改善（1-2時間）

**目的**: TypeScript E2Eテストの品質改善・User Projects機能追加

**作業内容**:

#### 2.1 authentication.spec.ts 品質改善（必須）

**現状の品質問題**:

1. **❌ テストアカウント不一致**
   - **現状**: TypeScript = `admin@example.com` / `Admin123!`
   - **C#**: `e2e-test@ubiquitous-lang.local` / `E2ETest#2025!Secure`
   - **影響**: テスト失敗の原因（認証情報が異なる）
   - **対策**: C#版と統一（`e2e-test@ubiquitous-lang.local`へ変更）

2. **❌ セレクタ差異**
   - **現状**: TypeScript = `.navbar-collapse`（誤り）
   - **C#**: `.nav-scrollable`（正しい）
   - **影響**: NavMenu表示確認テストが失敗する
   - **対策**: `.nav-scrollable`へ修正

3. **❌ パスワード変更ロジック不完全**
   - **現状**: パスワード変更後、元に戻す処理が不完全
   - **C#**: 変更後、再度`/change-password`へ遷移してパスワードを元に戻す
   - **TypeScript**: 再遷移処理が欠落
   - **影響**: テストデータ整合性が維持されない
   - **対策**: C#版と同様の再遷移処理を追加

**改善作業内容**:

1. テストアカウント統一（3箇所修正）
   ```typescript
   const TEST_EMAIL = 'e2e-test@ubiquitous-lang.local';
   const TEST_PASSWORD = 'E2ETest#2025!Secure';
   ```

2. セレクタ修正（1箇所修正）
   ```typescript
   // 修正前
   const navMenu = page.locator('.navbar-collapse');

   // 修正後
   const navMenu = page.locator('.nav-scrollable');
   ```

3. パスワード変更ロジック改善（1テスト修正）
   ```typescript
   // 成功メッセージ表示確認後に追加
   await page.goto(`${BASE_URL}/change-password`);
   await page.waitForLoadState('networkidle');
   await page.waitForTimeout(1000); // Blazor Server SignalR接続完了待機

   await page.fill('#currentPassword', 'NewAdmin123!');
   await page.fill('#newPassword', TEST_PASSWORD);
   await page.fill('#confirmPassword', TEST_PASSWORD);
   await page.click('button[type="submit"]');
   await page.waitForLoadState('networkidle');
   ```

4. テスト実行・全テストパス確認

#### 2.2 seed.spec.ts 品質改善（必須）

**現状の品質問題**:

1. **❌ テストアカウント不一致**
   - **現状**: TypeScript = `admin@example.com` / `Admin123!`
   - **C#**: `e2e-test@ubiquitous-lang.local` / `E2ETest#2025!Secure`
   - **影響**: テスト失敗の原因（認証情報が異なる）
   - **対策**: C#版と統一（`e2e-test@ubiquitous-lang.local`へ変更）

2. **❌ セレクタ差異**
   - **現状**: `.navbar-collapse`（誤り）
   - **正しい**: `.nav-scrollable`
   - **対策**: `.nav-scrollable`へ修正

**改善作業内容**:

1. テストアカウント統一（2箇所修正）
2. セレクタ修正（1箇所修正）
3. テスト実行・パス確認

#### 2.3 User Projects機能のTypeScript E2Eテスト作成
1. Generator Agent呼び出し
   ```
   🎭 generator, generate Playwright tests for User Projects feature
   ```
2. `user-projects.spec.ts` 生成
3. テスト実行・修正

#### 2.4 playwright.config.ts 設定見直し
1. ViewportSize確認（1920x1080維持）
2. タイムアウト設定確認
3. レポート設定確認

#### 2.5 データベース初期化・クリーンアップ処理実装（必要に応じて）
1. C#実装（AuthenticationTests.cs）を参考にTypeScript実装
2. テストデータ作成・削除処理
3. テスト分離確保

**完了基準**:
- ✅ `authentication.spec.ts` 全テストパス（6/6テスト成功）
- ✅ `seed.spec.ts` 全テストパス（1/1テスト成功）
- ✅ `user-projects.spec.ts` 作成・全テストパス（3/3テスト成功）
- ✅ テストアカウント・セレクタの統一完了
- ✅ TypeScript E2Eテスト品質確保

---

### Phase 3: 実行スクリプト更新（30分）

**目的**: run-e2e-tests.sh をTypeScript版に書き換え

**作業内容**:

#### 3.1 run-e2e-tests.sh 更新
```bash
# 変更前
dotnet test tests/UbiquitousLanguageManager.E2E.Tests \
    --filter "FullyQualifiedName~$TEST_FILTER" \
    --logger "console;verbosity=detailed"

# 変更後
cd tests/UbiquitousLanguageManager.E2E.Tests
npx playwright test ${TEST_FILTER:+--grep "$TEST_FILTER"}
```

#### 3.2 package.json npm scripts整備
```json
{
  "scripts": {
    "test": "npx playwright test",
    "test:ui": "npx playwright test --ui",
    "test:debug": "npx playwright test --debug",
    "test:headed": "npx playwright test --headed",
    "test:report": "npx playwright show-report"
  }
}
```

**完了基準**:
- ✅ `bash tests/run-e2e-tests.sh` 成功
- ✅ `bash tests/run-e2e-tests.sh AuthenticationTests` フィルタ実行成功
- ✅ npm scripts動作確認

---

### Phase 4: Agent/Skills更新（1-2時間）

**目的**: Agent定義・Skillsの TypeScript Playwright対応

**作業内容**:

#### 4.1 e2e-test Agent定義更新
**ファイル**: `.claude/agents/e2e-test.md`

**変更内容**:
- **削除**: Serena MCPツール使用（C#コード操作不要）
- **追加**: Playwright Test MCPツール活用
- **変更**: TypeScript Playwright前提の説明

**更新後の構成**:
```yaml
---
name: e2e-test
description: "TypeScript Playwright E2Eテスト実装・Playwright Test Agents統合・Generator/Healer/Planner活用の専門Agent"
tools: mcp__playwright-test__*, mcp__playwright__*, Bash, Read, Write, Edit, MultiEdit
---
```

#### 4.2 playwright-e2e-patterns Skill更新
**ファイル**: `.claude/skills/playwright-e2e-patterns/SKILL.md`

**変更内容**:
- TypeScript Playwright前提のパターン説明
- Generator Agent活用パターン追加
- C#例 → TypeScript例に変更

**更新対象パターンファイル**:
1. `patterns/data-testid-design.md` - TypeScript例に更新
2. `patterns/mcp-tools-usage.md` - TypeScript例に更新
3. `patterns/blazor-signalr-e2e.md` - TypeScript例に更新

**完了基準**:
- ✅ e2e-test Agent定義がTypeScript Playwright対応
- ✅ playwright-e2e-patterns SkillがTypeScript対応
- ✅ 全パターンファイルにTypeScript例を記載

---

### Phase 5: ドキュメント更新（1-2時間）

**目的**: プロジェクトドキュメント全体のTypeScript E2E対応

**作業内容**:

#### 5.1 Playwright_運用統合ガイドライン.md 更新
**ファイル**: `Doc/08_Organization/Rules/Playwright_運用統合ガイドライン.md`

**更新内容**:
- TypeScript E2E標準に変更
- Generator Agent活用方法追加
- C# Playwright記載削除

#### 5.2 ADR_020 更新
**ファイル**: `Doc/07_Decisions/ADR_020_テストアーキテクチャ決定.md`

**更新内容**:
- 言語別分離原則にTypeScript追加
- E2E.TestsをTypeScriptプロジェクトとして定義
- 技術的根拠追加（Generator Agent活用）

**更新後の記載**:
```
### 言語別分離原則

- **F# プロジェクト**: Domain.Unit.Tests, Application.Unit.Tests
- **C# プロジェクト**: Contracts.Unit.Tests, Infrastructure.Unit.Tests,
                      Infrastructure.Integration.Tests, Web.UI.Tests
- **TypeScript プロジェクト**: E2E.Tests（Playwright Generator Agent活用のため）

#### TypeScript採用の技術的根拠

1. **Playwright Generator AgentがTypeScript専用**
   - 公式仕様によりTypeScriptのみ対応
   - C# Playwrightとの直接連携不可

2. **E2Eテスト作成効率化**
   - Generator Agentによる自動生成（60-70%時間削減実績）
   - TypeScript → C# 変換の中間工程削除

3. **Clean Architecture整合性**
   - E2Eテストは全層横断のため、特定言語に依存しない
   - テストコード言語とプロダクションコード言語の分離は許容
```

#### 5.3 テストアーキテクチャ設計書 更新
**ファイル**: `Doc/02_Design/テストアーキテクチャ設計書.md`

**更新内容**:
- E2E.Tests言語選択ガイドライン更新
- 実行方法変更（dotnet test → npx playwright test）
- TypeScript E2Eテストの参照関係説明削除（プロダクションコード参照不要）

#### 5.4 CLAUDE.md 更新
**ファイル**: `CLAUDE.md`

**更新内容**:
- 開発コマンド説明更新
- E2Eテスト実行方法変更
- TypeScript E2Eテスト実装手順追加

**更新箇所**:
```markdown
## E2Eテスト自動実行

**一括実行スクリプト**（推奨）:

`tests/run-e2e-tests.sh`は、E2Eテスト実行を自動化するスクリプトです：
- Webアプリケーションをバックグラウンド起動
- ポート5001の応答待機（最大60秒）
- **TypeScript E2Eテスト実行**（`npx playwright test`）
- プロセスクリーンアップ

#### 方法A: VS Code統合ターミナル（推奨）

```bash
# 全E2Eテスト実行
bash tests/run-e2e-tests.sh

# 特定テストファイルのみ実行
bash tests/run-e2e-tests.sh authentication.spec.ts
bash tests/run-e2e-tests.sh user-projects.spec.ts
```

#### 方法B: ホスト環境から明示的実行（Claude Code用）

```bash
# 全E2Eテスト実行
docker exec ubiquitous-lang-mng_devcontainer-devcontainer-1 bash tests/run-e2e-tests.sh

# 特定テストファイルのみ実行
docker exec ubiquitous-lang-mng_devcontainer-devcontainer-1 bash tests/run-e2e-tests.sh authentication.spec.ts
```
```

#### 5.5 組織管理運用マニュアル.md 更新
**ファイル**: `Doc/08_Organization/Rules/組織管理運用マニュアル.md`

**更新内容**:

1. **E2Eテスト実行方法の変更**
   - `dotnet test tests/UbiquitousLanguageManager.E2E.Tests` → `bash tests/run-e2e-tests.sh`
   - TypeScript Playwright実行コマンド説明追加

2. **e2e-test SubAgent活用ガイド更新**
   - TypeScript Playwright前提の説明に変更
   - Generator/Healer/Planner Agents活用プロセス追加
   - C# Playwright関連記載削除

3. **E2Eテスト作成標準プロセス更新**
   - Generator Agent呼び出しパターン追加
   - TypeScript E2Eテスト作成手順追加
   - data-testid属性設計パターン（TypeScript版）

4. **Commands実行時のE2E確認手順更新**
   - step-end-review時のE2Eテスト実行方法
   - phase-end時のE2Eテスト全実行方法
   - TypeScript環境でのテスト実行確認

**更新セクション例**:
```markdown
### E2Eテスト実行方法（TypeScript Playwright）

#### 標準実行方法
```bash
# 全E2Eテスト実行
bash tests/run-e2e-tests.sh

# 特定テストファイルのみ実行
bash tests/run-e2e-tests.sh authentication.spec.ts
```

#### e2e-test SubAgent活用
```
e2e-test Agent, create E2E tests for [feature name]

実施内容:
- [機能概要]
- [テストシナリオ概要]
- Generator Agent活用（TypeScript自動生成）
```

#### E2Eテスト作成標準プロセス（Generator Agent活用）
1. 機能要件確認
2. Generator Agent呼び出し
   ```
   🎭 generator, generate Playwright tests for [feature] at https://localhost:5001/[path]
   ```
3. 生成されたTypeScriptテスト確認・改善
4. テスト実行・パス確認
5. data-testid属性設計パターン適用
6. Blazor Server SignalR対応パターン適用
```

#### 5.6 GitHub Issue #46 更新
**Issue**: [#46 Playwright統合後のCommands/SubAgent刷新](https://github.com/d-kishi/ubiquitous-lang-mng/issues/46)

**更新内容**:

1. **コメント修正**（[コメント #3538861969](https://github.com/d-kishi/ubiquitous-lang-mng/issues/46#issuecomment-3538861969)）
   - **誤**: "generator Agent: C# Playwright非対応 → 本プロジェクトでは使用不可"
   - **正**: "generator Agent: TypeScript専用 → E2EテストをTypeScriptへ完全移行により使用可能"

2. **新規コメント追加**
   - TypeScript E2E移行完了報告
   - Generator/Healer/Planner Agents完全活用可能になったこと
   - 移行計画ドキュメントへのリンク

**コメント例**:
```markdown
## TypeScript E2E移行完了（2025-11-18）

Phase B-F2 Step6_2にて、E2EテストをC#からTypeScriptへ完全移行しました。

### 移行結果
- ✅ C# E2Eテスト完全削除（AuthenticationTests.cs, UserProjectsTests.cs）
- ✅ TypeScript E2Eテスト品質改善・新規作成（authentication.spec.ts, user-projects.spec.ts）
- ✅ Generator/Healer/Planner Agents完全活用可能

### Generator Agent活用可能化
従来「C# Playwright非対応のため使用不可」としていましたが、TypeScript移行により**完全活用可能**になりました。

### 効果
- E2Eテスト作成効率: 97-98%削減（Phase B2: 93.3% → Phase B-F2: 97-98%）
- Generator Agent: 60-70%時間削減
- Healer Agent: 修復成功率50-70%期待

### 関連ドキュメント
- [移行計画](../Active/Phase_B-F2/Step06_2_E2ETestTypescript移行.md)
- [Playwright運用統合ガイドライン](../Rules/Playwright_運用統合ガイドライン.md)
```

**完了基準**:
- ✅ Playwright_運用統合ガイドライン.md がTypeScript E2E標準
- ✅ ADR_020 がTypeScript E2E対応
- ✅ テストアーキテクチャ設計書 がTypeScript E2E対応
- ✅ CLAUDE.md がTypeScript E2E対応
- ✅ 組織管理運用マニュアル.md がTypeScript E2E対応
- ✅ GitHub Issue #46 更新完了

---

## ⚠️ リスク評価

### 高リスク

| リスク | 影響度 | 発生確率 | 対策 |
|-------|-------|---------|------|
| **TypeScriptテスト品質** | 高 | 高 | Generator Agent活用・段階的改善・テスト実行確認の徹底 |
| **DB初期化処理移行** | 高 | 中 | C#実装（AuthenticationTests.cs）を参考にTypeScript実装・動作確認徹底 |

### 中リスク

| リスク | 影響度 | 発生確率 | 対策 |
|-------|-------|---------|------|
| **Generator Agent習得コスト** | 中 | 中 | Step6経験活用・段階的学習・playwright-test MCPツール活用 |
| **CI/CDパイプライン影響** | 中 | 低 | 将来対応（現在CI/CD未実装）・GitHub Actions整備時に対応 |

### 低リスク

| リスク | 影響度 | 発生確率 | 対策 |
|-------|-------|---------|------|
| **技術スタック増加（F# + C# + TypeScript）** | 低 | 高 | DevContainer環境で吸収済み・E2EテストのみTypeScript |
| **技術的実現可能性** | 低 | 低 | 既に確認済み（実際に実行成功） |

---

## 📈 期待される効果

### Positive

1. **E2Eテスト作成効率向上（推定60-70%）**
   - Generator Agent直接活用による自動生成
   - TypeScript → C# 変換工程の削減
   - Step6実績（1-2時間削減）の再現

2. **テスト品質向上**
   - Generator Agentによる標準的なテストパターン適用
   - Playwright TypeScript公式ドキュメント準拠
   - Healer Agentによる自動修復機能活用

3. **技術的整合性向上**
   - Playwright Generator AgentのTypeScript専用仕様に準拠
   - 中間変換工程削除による品質劣化リスク排除

4. **長期的保守性向上**
   - Playwright公式ツールとの完全統合
   - TypeScript E2Eテストのエコシステム活用
   - Generator/Healer/Planner Agentsの継続的活用

### Negative

1. **初期移行コスト**
   - C# E2Eテスト削除による一時的なカバレッジ低下
   - TypeScript E2Eテスト品質改善作業（推定1-2時間）
   - ドキュメント更新作業（推定1-2時間）

2. **技術スタック増加**
   - F# + C# + TypeScript の3言語体制
   - 学習コスト増加（TypeScript E2Eテスト作成方法）

3. **移行期間中のリスク**
   - E2Eテストカバレッジ低下（移行完了まで）
   - 回帰テスト実行不可（移行作業中）

---

## ✅ 移行完了基準

### 必須基準（Must）

- ✅ C# E2Eテストファイル完全削除
- ✅ TypeScript E2Eテスト全テストパス
- ✅ `bash tests/run-e2e-tests.sh` 成功
- ✅ ドキュメント更新完了（ADR_020, テストアーキテクチャ設計書, CLAUDE.md等）
- ✅ `dotnet build` 成功（0 Warning / 0 Error）

### 推奨基準（Should）

- 📊 TypeScript E2Eテストカバレッジ ≥ C# E2Eテストカバレッジ
- 📊 Generator Agent活用パターン文書化
- 📝 移行後の効果測定レポート作成

### 希望基準（Could）

- 🎭 Healer Agent実用評価実施
- 🎭 Planner Agent実用評価実施
- 📊 E2Eテスト作成効率測定（移行前後比較）

---

## 🔗 関連ドキュメント

### ADR（技術決定記録）
- [ADR_020_テストアーキテクチャ決定](../../../07_Decisions/ADR_020_テストアーキテクチャ決定.md)
- [ADR_021_Playwright統合戦略](../../../07_Decisions/ADR_021_Playwright統合戦略.md)

### 設計書
- [テストアーキテクチャ設計書](../../../02_Design/テストアーキテクチャ設計書.md)

### Agent Skills
- [playwright-e2e-patterns](../../../../.claude/skills/playwright-e2e-patterns/SKILL.md)

### 組織・運用ドキュメント
- [Playwright_運用統合ガイドライン](../../../08_Organization/Rules/Playwright_運用統合ガイドライン.md)

### 技術調査
- [Tech_Research_Playwright_Test_Agent_2025-11](../Research/Tech_Research_Playwright_Test_Agent_2025-11.md)

---

**作成日**: 2025-11-17
**作成者**: Claude Code
**ステータス**: 計画策定完了・実装未着手
