# ADR_023: DB初期化方針決定（EF Migrations主体・Code First方式）

**Status**: Accepted
**Date**: 2025-10-27
**決定者**: プロジェクトオーナー + Claude Code
**関連Issue**: GitHub Issue #58
**関連Phase**: Phase B2 Step7

---

## Context（背景）

### 問題状況
Phase B2 Step6完了時点で、データベース初期化に**二重管理問題**が発生：

1. **SQL Scripts方式**（`init/01_create_schema.sql`, `init/02_initial_data.sql`）
   - PostgreSQL固有機能活用（TIMESTAMPTZ、JSONB、COMMENT文）
   - 手動管理によるメンテナンスコスト
   - 全識別子Quote済み（大文字小文字保持）

2. **EF Migrations方式**（4つのPending Migrations）
   - .NETエコシステム統合
   - マイグレーション履歴自動管理
   - スキーマ変更のバージョン管理

### Source of Truth不明確
- どちらを優先すべきか不明
- スキーマ変更時に両方を更新する必要がある
- 不整合リスクが高い

### Phase B3以降の課題
- Phase B3以降でスキーマ変更が頻繁に発生する見込み
- スキーマ変更管理手順の確立が必須

---

## Decision（決定）

**Option A: EF Migrations主体（Code First方式）を採用**

### Source of Truth確立
- **Entity定義（C#コード）**: スキーマ定義のSource of Truth
- **EF Migrations**: スキーマ変更の履歴管理・バージョン管理

### 初期化フロー
- **開発環境**: `dotnet ef database update` → DbInitializer.cs自動実行
- **本番環境**: `dotnet ef database update` → InitialDataService.cs実行

### スキーマ変更手順（5ステップ）
1. Entity定義変更（C#コード）
2. `dotnet ef migrations add MigrationName`
3. Migrationファイル確認・必要に応じて手動編集（CHECK制約、GINインデックス等）
4. `dotnet ef database update`
5. データベース設計書更新（型定義・制約同期）

---

## Consequences（結果）

### Pros（利点）

#### ✅ PostgreSQL固有機能完全サポート
- **検証済み**: TIMESTAMPTZ、JSONB、COMMENT文が完全サポート
- **Migration実装例**: Stage 2-3で実証完了
  - TIMESTAMPTZ: `type: "timestamptz"` パラメータ指定
  - COMMENT文: `comment:` パラメータ指定
  - CHECK制約: `HasCheckConstraint()` メソッド使用

#### ✅ .NET統合・マイグレーション履歴自動管理
- `__EFMigrationsHistory`テーブルで履歴管理
- `dotnet ef`コマンドで一元管理

#### ✅ データベース設計書との乖離リスク解消
- **根本原因**: データベース設計書がSQL標準型名を使用（VARCHAR）、PostgreSQL実際型名は異なる（character varying）
- **解決策**: データベース設計書をPostgreSQL標準型名に統一（Stage 5-1で実施）
- **継続的整合性**: スキーマ変更手順（5ステップ）の最後にデータベース設計書更新を組み込み

### Cons（欠点・リスク）

#### ⚠️ 初期移行コスト
- **実績**: Phase B2 Step7で2.8-3.5時間（推定）→ 実際約2時間（対応完了）
- **Stage構成**: 5 Stages（バックアップ、Migrations実行、CHECK制約追加、Scripts削除、ドキュメント整備）

#### ⚠️ GINインデックス手動SQL必要
- **現状**: Phase B2では不要（検索機能未実装）
- **Phase C-D対応予定**: ユビキタス言語検索機能実装時にGINインデックス追加Migration作成
- **実装方法確立済み**: `migrationBuilder.Sql()` メソッドで手動SQL追加

---

## Alternatives Considered（検討した代替案）

### Option B: SQL Scripts主体（Database First方式）
**却下理由**:
- 手動管理のメンテナンスコスト
- .NETエコシステムとの統合が弱い
- マイグレーション履歴管理が手動

### Option C: ハイブリッド方式
**却下理由**:
- 運用複雑化
- 責任分界点不明確
- Option Aで全て対応可能

---

## Risks（リスク）

### ✅ 対応済みリスク
1. **PostgreSQL固有機能制限**: Stage 2-3で完全サポート確認済み
2. **データベース設計書乖離**: Stage 5-1でPostgreSQL標準型名に統一

### ⚠️ 継続監視リスク
1. **GINインデックス**: Phase C-D実装時に手動SQL追加必要
2. **パフォーマンス最適化**: PostgreSQL固有インデックス（BRIN等）は手動SQL必要

---

## References（参照）

### 関連ドキュメント
- **GitHub Issue #58**: DB初期化二重管理問題
- **データベース設計書**: `Doc/02_Design/データベース設計書.md`（Stage 5-1で更新）
- **Step07組織設計**: `Doc/08_Organization/Active/Phase_B2/Step07_組織設計.md`

### 関連Agent Skills
- **db-schema-management**: スキーマ変更パターンSkill（Stage 5-2で作成）

### 技術参考
- **EF Core Documentation**: [Migrations Overview](https://learn.microsoft.com/en-us/ef/core/managing-schemas/migrations/)
- **Npgsql Documentation**: [PostgreSQL Type Mapping](https://www.npgsql.org/doc/types/basic.html)

---

## Implementation Notes（実装メモ）

### Phase B2 Step7実装実績
- **Stage 1**: バックアップ・準備（10分→3分）
- **Stage 2**: EF Migrations実行・検証（60-90分→70分）
- **Stage 3**: CHECK制約追加Migration作成（30-40分→15分）
- **Stage 4**: SQL Scripts削除・クリーンアップ（10-15分→5分）
- **Stage 5**: ドキュメント整備（60-80分・実施中）

### 成果物
- `__EFMigrationsHistory`: 5レコード
- DbInitializer.cs: 開発環境初期データ投入
- InitialDataService.cs: 本番環境スーパーユーザー作成
- データベース設計書更新: PostgreSQL標準型名統一

---

**最終更新**: 2025-10-27
