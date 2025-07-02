# ADR_005: PostgreSQL Docker Container採用

**日付**: 2025-07-02  
**状態**: 採用決定  
**決定者**: プロジェクトオーナー  
**記録者**: Claude Code  

## 背景・課題

### 従来の計画
- **開発環境**: SQLite（軽量セットアップ）
- **本番環境**: PostgreSQL（部門承認後移行）
- **課題**: SQLite → PostgreSQL の**データ移行**が必要

### 技術的課題
- データ型互換性の問題（DATETIME vs TIMESTAMP等）
- SQL方言の違い（AUTOINCREMENT vs SERIAL等）
- 移行時のデータ整合性保証
- 移行スクリプト開発・テスト・運用コスト

## 決定内容

### PostgreSQL Docker Container採用
- **開発環境**: PostgreSQL（Docker Container）
- **本番環境**: クラウドDBサービス（AWS RDS/Azure Database/GCP Cloud SQL等）
- **効果**: **データ移行が一切不要**

### 技術構成
- **開発**: `docker-compose.yml` による PostgreSQL Container
- **本番**: クラウドキャリアのマネージドDBサービス
- **DB接続**: 環境変数による接続文字列切り替え

## 採用理由

### 1. データ移行コスト削除（主要理由）
- SQLite → PostgreSQL移行スクリプト開発が不要
- 移行時のデータ整合性テストが不要
- 本番リリース時の移行リスクが完全に排除

### 2. 開発効率向上
- 開発環境と本番環境のSQL方言統一
- PostgreSQL固有機能（JSONB、配列型等）の開発時活用
- 本番環境と同一のクエリ実行計画・パフォーマンス特性

### 3. 運用安全性向上
- 開発・テスト・本番環境の完全統一
- SQL互換性問題の事前排除
- 本番デプロイ時の予期しない動作の防止

## 技術的詳細

### Docker Container仕様
```yaml
# docker-compose.yml（予定）
version: '3.8'
services:
  postgres:
    image: postgres:15
    environment:
      POSTGRES_DB: ubiquitous_lang_db
      POSTGRES_USER: dev_user
      POSTGRES_PASSWORD: dev_password
    ports:
      - "5432:5432"
    volumes:
      - postgres_data:/var/lib/postgresql/data
      - ./init.sql:/docker-entrypoint-initdb.d/init.sql
volumes:
  postgres_data:
```

### 接続文字列管理
```json
// appsettings.Development.json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Port=5432;Database=ubiquitous_lang_db;User Id=dev_user;Password=dev_password;"
  }
}

// appsettings.Production.json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=${DB_HOST};Port=${DB_PORT};Database=${DB_NAME};User Id=${DB_USER};Password=${DB_PASSWORD};"
  }
}
```

## 代替案検討

### 案1: SQLite → PostgreSQL移行（従来案）
- **メリット**: 初期セットアップの軽量性
- **デメリット**: データ移行コスト、SQL互換性問題
- **結論**: 移行コストが過大

### 案2: PostgreSQL Docker Container（採用案）
- **メリット**: 移行コスト削除、環境統一、開発効率向上
- **デメリット**: Docker環境構築が必要
- **結論**: メリットが圧倒的

### 案3: PostgreSQL直接インストール
- **メリット**: Docker不要
- **デメリット**: 環境構築複雑性、チーム間環境差異
- **結論**: Docker Containerの方が管理しやすい

## 影響範囲

### 変更対象
- ✅ **データベース設計書**: 既にPostgreSQL前提で作成済み
- ✅ **DDLファイル**: 既にPostgreSQL対応済み
- 🔄 **システム設計書**: PostgreSQL Docker前提で作成予定
- 📋 **開発環境構築**: Docker Compose設定が必要

### 変更不要
- 要件定義書・機能仕様書・ユーザーストーリー（DB技術非依存）
- UI設計（DB技術非依存）
- Clean Architectureレイヤー設計（Infrastructure層で抽象化）

## 実装タスク

### 高優先度
1. **Docker Compose設定作成**
   - PostgreSQL Container定義
   - 初期データセットアップ
   - 永続化ボリューム設定

2. **接続文字列管理**
   - 開発・本番環境設定分離
   - 環境変数による設定切り替え

### 中優先度
3. **開発環境構築手順書**
   - Docker環境セットアップガイド
   - 初回セットアップ手順
   - トラブルシューティング

## リスク・対策

### リスク1: Docker環境構築の複雑性
- **対策**: 詳細な手順書作成、自動化スクリプト提供

### リスク2: 開発チームのDocker習熟度
- **対策**: 基本的なDocker操作（up/down/logs）のみで運用可能な設計

### リスク3: 本番環境接続設定
- **対策**: 環境変数による抽象化、設定検証機能

## 成功指標

### 技術的成功
- [ ] Docker Container起動・接続成功
- [ ] EF Core Migration正常実行
- [ ] 開発・本番環境でのSQL動作一致

### 運用的成功
- [ ] 開発環境セットアップ時間短縮
- [ ] 本番デプロイ時のDB関連問題ゼロ
- [ ] データ移行作業の完全削除

## 参考情報

### 関連決定
- **ADR_002**: MermaidのER図記法統一
- **ADR_003**: 用語統一（ユビキタス言語）
- **ADR_004**: コミュニケーション課題状態管理システム

### 関連ファイル
- `/Doc/02_Design/データベース設計書.md`
- `/Doc/02_Design/database_schema.sql`
- `/Doc/04_Daily/2025-07/2025-07-02.md`（本決定記録）

---

**決定効果**: データ移行コスト削除、開発効率向上、運用安全性確保  
**次回アクション**: Docker Compose設定作成、システム設計書への反映  
**承認**: プロジェクトオーナー承認済み（2025-07-02）