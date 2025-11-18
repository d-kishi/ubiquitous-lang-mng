# Phase B2 Step7・Step8 次回セッション準備メモ

**作成日**: 2025-10-26
**対象Step**: Step7（DB初期化方針決定）、Step8（E2E完全動作検証）
**Phase**: Phase B2（ユーザー・プロジェクト関連管理）

---

## 📋 セッション開始時の必須確認事項

### 1. Phase_Summary.md確認
- **ファイル**: `Doc/08_Organization/Active/Phase_B2/Phase_Summary.md`
- **確認内容**: Step7とStep8の実施内容・成果物・SubAgent構成

### 2. GitHub Issue #58確認
- **Issue番号**: #58
- **タイトル**: DB初期化二重管理問題（EF Migrations vs SQL Scripts）
- **ラベル**: tech-debt
- **状態**: OPEN
- **対応期限**: Phase B3着手前

---

## 🎯 Step7: DB初期化方針決定（GitHub Issue #58対応）

### 対応すべき問題

#### 現状の二重管理状態
1. **SQL Scripts方式**:
   - `init/01_create_schema.sql` - テーブル定義（14テーブル、全識別子Quote済み）
   - `init/02_initial_data.sql` - 初期データ投入（4ユーザー、4ロール、2プロジェクト）
   - PostgreSQL固有機能活用（TIMESTAMPTZ、JSONB、COMMENT文）

2. **EF Migrations方式**:
   - 4つのMigrationファイル（全てPending状態）:
     - `20250729153117_FinalInitMigrationWithComments`
     - `20250812070606_AddIdentityClaimTables`
     - `20250812071836_Phase_A5_StandardIdentityMigration`
     - `20251002152530_PhaseB1_AddProjectAndDomainFields`
   - `__EFMigrationsHistory`テーブル（EF自動作成）
   - AspNetUserClaims、AspNetRoleClaimsテーブル（EF自動作成可能）

#### 問題点
1. **二重管理によるメンテナンスコスト**
   - スキーマ変更時に両方を更新する必要がある
   - 不整合リスクが高い

2. **init/01_create_schema.sqlの過去の不完全性**（現在は修正済み）
   - AspNetUserClaims、AspNetRoleClaimsが定義されていない（→ 2025-10-26修正完了）
   - DB設計書との不整合（→ 2025-10-26修正完了）

3. **初期化フローが不明確**
   - どちらを優先すべきか不明
   - 開発環境・本番環境での扱いが未定義

### 技術的考察（Option A/B/C）

#### Option A: EF Migrations主体（Code First）
**メリット**:
- .NETエコシステムとの親和性が高い
- マイグレーション履歴管理が自動化
- スキーマ変更のバージョン管理が容易

**デメリット**:
- PostgreSQL固有機能の制御が制限される
- TIMESTAMPTZ、JSONB等の最適化が困難
- データベース設計書との乖離リスク

#### Option B: SQL Scripts主体（Database First）
**メリット**:
- PostgreSQL固有機能を完全制御可能
- データベース設計書と直接対応
- パフォーマンス最適化が容易
- COMMENT文による自己文書化

**デメリット**:
- 手動管理のメンテナンスコスト
- .NETエコシステムとの統合が弱い
- マイグレーション履歴管理が手動

#### Option C: ハイブリッド方式
**内容**:
- スキーマ定義: SQL Scripts（PostgreSQL最適化重視）
- データ投入: InitialDataService（.NET統合重視）
- マイグレーション: EF Migrations（変更管理）

**メリット**:
- 両者の利点活用

**デメリット**:
- 運用複雑化
- 責任分界点不明確

### Step7実施時の参照ドキュメント

1. **GitHub Issue #58本文**:
   ```bash
   gh issue view 58
   ```

2. **データベース設計書**:
   - `Doc/02_Design/データベース設計書.md`
   - セクション1.1: 設計方針
   - セクション1.2: システム構成

3. **現状のSQLファイル**:
   - `init/01_create_schema.sql` - 2025-10-26修正版（全識別子Quote済み）
   - `init/02_initial_data.sql` - 2025-10-26修正版（全識別子Quote済み）

4. **EF Migrations状態確認**:
   ```bash
   dotnet ef migrations list --project src/UbiquitousLanguageManager.Infrastructure
   ```

### Step7成果物

- **ADR_023**: DB初期化方針決定（Option A/B/C選択理由・運用方針）
- **統一DB初期化スクリプト/Migrations**
- **データベース設計書更新**（初期化方針セクション追加）
- **GitHub Issue #58クローズ**

---

## 🎯 Step8: E2Eテスト実行環境整備・Phase B2完全動作検証

### 前提条件

**Step7完了必須**: DB初期化方針が確定していること

### E2Eテストユーザ仕様

#### 必須要件
- **Email**: `e2e-test@ubiquitous-lang.local`
- **UserName**: `e2e-test@ubiquitous-lang.local`（Emailと同じ）
- **Name**: `E2Eテストユーザー`
- **IsFirstLogin**: **false**（初回ログイン済み状態）
- **PasswordHash**: BCryptハッシュ（平文: `E2ETest#2025!Secure`）
- **InitialPassword**: `E2ETest#2025!Secure`（平文保存）
- **Role**: SuperUser（全機能アクセス権限）

#### 作成方法（Step7決定方式による）
- **Option A選択時**: EF Seeding or InitialDataService
- **Option B選択時**: init/03_test_data.sql作成
- **Option C選択時**: 選択した方式に準拠

### E2Eテストデータ仕様

#### テストプロジェクト
- **ProjectName**: `E2Eテストプロジェクト`
- **Description**: `Playwright E2Eテスト専用プロジェクト`
- **Owner**: `e2e-test@ubiquitous-lang.local`

#### テストドメイン
- **DomainName**: `E2Eテストドメイン`
- **Description**: `Playwright E2Eテスト専用ドメイン`
- **ProjectId**: E2Eテストプロジェクトの自動採番ID

#### UserProjects関連
- **UserId**: `e2e-test@ubiquitous-lang.local`
- **ProjectId**: E2Eテストプロジェクトの自動採番ID

#### ドラフトユビキタス言語サンプル（オプション）
- **JapaneseName**: `テスト用語`
- **EnglishName**: `TestTerm`
- **Description**: `E2Eテスト用のサンプル用語`
- **DomainId**: E2Eテストドメインの自動採番ID

### E2Eテスト実行対象

#### UserProjectsTests.cs（3シナリオ）
- **場所**: `tests/UbiquitousLanguageManager.Web.E2E.Tests/UserProjectsTests.cs`
- **シナリオ**:
  1. ログイン → プロジェクト詳細 → メンバー管理画面表示
  2. メンバー追加操作
  3. メンバー削除操作

#### Playwright環境確認
- **Browser確認**: `powershell playwright.ps1 install` 実行済み（2025-10-26）
- **再確認コマンド**: `npx playwright --version`

### Step8実施時の動作確認項目

#### Phase B2機能完全動作検証
1. **UserProjects多対多関連**:
   - プロジェクトメンバー追加・削除が正常動作
   - UserProjectsテーブルへの正常挿入・削除

2. **権限制御16パターン**:
   - E2Eテストユーザー（SuperUser）で全機能アクセス可能確認

3. **Phase B1技術負債解消確認**:
   - InputRadioGroup正常動作
   - EditForm送信ロジック正常動作
   - Null参照警告なし

4. **data-testid属性15要素**:
   - 全要素がPlaywrightで取得可能確認

### Step8成果物

- **E2Eテストユーザ・データ作成完了**（init/03_test_data.sql or Seedingメソッド）
- **UserProjectsTests.cs実行成功レポート**（3シナリオ全成功）
- **Phase B2機能動作確認完了レポート**
- **Phase B2完全完了宣言**

---

## 📂 関連ファイル・ディレクトリ

### ドキュメント
- `Doc/08_Organization/Active/Phase_B2/Phase_Summary.md` - Phase B2総括
- `Doc/02_Design/データベース設計書.md` - DB設計詳細
- `Doc/07_Decisions/ADR_021_Playwright統合戦略.md` - Playwright技術決定

### コード
- `init/01_create_schema.sql` - スキーマ定義（2025-10-26修正版）
- `init/02_initial_data.sql` - 初期データ投入（2025-10-26修正版）
- `src/UbiquitousLanguageManager.Infrastructure/Migrations/` - EF Migrationsディレクトリ
- `tests/UbiquitousLanguageManager.Web.E2E.Tests/UserProjectsTests.cs` - E2Eテスト

### GitHub
- GitHub Issue #58: DB初期化二重管理問題
- GitHub Issue #57: Playwright実装責任の検討（Phase B3着手前対応）

---

## 🔍 セッション開始時の確認コマンド

### EF Migrations状態確認
```bash
dotnet ef migrations list --project src/UbiquitousLanguageManager.Infrastructure
```

### データベース状態確認
```bash
docker-compose exec postgres psql -U ubiquitous_lang_user -d ubiquitous_lang_db -c "\dt"
```

### Playwright環境確認
```bash
npx playwright --version
```

### GitHub Issue #58確認
```bash
gh issue view 58
```

---

## 💡 重要な技術的前提知識

### PostgreSQL識別子の取扱い
- **Quote必須**: テーブル名・列名は全て`""`で囲む（大文字小文字保持）
- **Unquoted**: 小文字に正規化される（aspnetusers等の重複テーブル作成リスク）

### ASP.NET Core Identity初期パスワード
- **InitialPassword**: 平文保存（AspNetUsers.InitialPassword列）
- **PasswordHash**: BCryptハッシュ（AspNetUsers.PasswordHash列）
- **パスワードポリシー**: PasswordHash作成時のみ適用

### E2Eテストユーザー要件
- **IsFirstLogin = false**: 初回ログイン済み状態
- **理由**: 初回パスワード変更フローをスキップし、即座にログイン可能にする

---

## 📝 補足事項

### 2025-10-26セッション成果（Phase B2 Step6完了時点）
- ✅ init/01_create_schema.sql修正完了（全識別子Quote済み）
- ✅ init/02_initial_data.sql修正完了（全識別子Quote済み）
- ✅ 重複小文字テーブル12件削除完了
- ✅ AspNetUserClaims、AspNetRoleClaims追加完了
- ✅ データベース再初期化成功（0エラー）

### Git状態（2025-10-26時点）
- **Staged変更**: Playwright関連ファイル（Step6成果物）
- **Unstaged変更**: なし（仕切り直しでロールバック完了）
- **Untracked変更**: なし

### バックグラウンドプロセス
- **dotnet run**: 複数のバックグラウンドプロセスが実行中
- **セッション開始時**: 不要なプロセスは終了推奨（`KillShell`使用）

---

**メモ作成者**: Claude Code
**次回セッション担当者**: Claude Code
**監督**: プロジェクトオーナー
