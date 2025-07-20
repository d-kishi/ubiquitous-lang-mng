# トラブルシューティングガイド - Phase A2

**作成日**: 2025-07-20  
**対象**: システム管理者・開発者  
**Phase**: A2ユーザー管理機能  

## 🚨 緊急度別対応フロー

### 🔴 緊急（システム停止）
**対応時間**: 即座（5分以内）
1. **エラーログ確認**
2. **データベース状態確認** 
3. **緊急復旧手順実行**
4. **影響範囲把握・報告**

### 🟡 重要（機能制限）
**対応時間**: 30分以内
1. **問題特定・分離**
2. **回避策提供**
3. **根本解決計画**

### 🟢 通常（軽微な問題）
**対応時間**: 2時間以内
1. **調査・解決**
2. **再発防止策検討**

## 📋 症状別診断フローチャート

### ログイン問題

#### 「ログインできない」
```
1. メールアドレス・パスワード確認
   ├─ 正しい → 2へ
   └─ 間違い → ユーザーに正しい情報確認依頼

2. ユーザーアカウント状態確認
   ├─ アクティブ → 3へ
   └─ 無効 → 管理者にアカウント有効化依頼

3. データベース接続確認
   ├─ 正常 → 4へ
   └─ 異常 → 「データベース問題」セクション参照

4. ASP.NET Core Identity確認
   ├─ 正常 → サポート連絡
   └─ 異常 → アプリケーション再起動
```

#### 「ログイン後に権限エラー」
```
1. ユーザーロール確認
   ├─ 適切 → 2へ
   └─ 不適切 → 管理者にロール変更依頼

2. 操作対象との権限関係確認
   ├─ 問題なし → 3へ
   └─ 権限不足 → 権限説明・代替手段提案

3. セッション状態確認
   ├─ 有効 → アプリケーション問題として調査
   └─ 無効 → 再ログイン依頼
```

### データベース問題

#### 「アプリケーション起動時エラー」
```
System.InvalidOperationException: Cannot use table 'AspNetUsers'
```

**診断手順**:
```bash
# 1. PostgreSQLコンテナ状態確認
docker ps | grep postgres
# 期待値: UP状態

# 2. コンテナログ確認
docker logs ubiquitous-lang-mng_postgresql_1
# 確認項目: エラーメッセージ、接続拒否

# 3. 接続テスト
docker exec -it [postgres_container] psql -U ubiquitous_user -d ubiquitous_db
```

**解決手順**:
```bash
# 手順1: コンテナ再起動
docker-compose restart postgresql
sleep 10

# 手順2: マイグレーション確認・実行
dotnet ef database update --project src/UbiquitousLanguageManager.Infrastructure

# 手順3: アプリケーション再起動
dotnet run --project src/UbiquitousLanguageManager.Web
```

#### 「データベース接続タイムアウト」
**症状**: ページ読み込みが非常に遅い、タイムアウトエラー

**診断手順**:
```bash
# 1. 接続数確認
docker exec [postgres_container] psql -U ubiquitous_user -d ubiquitous_db -c "SELECT count(*) FROM pg_stat_activity;"

# 2. 長時間実行クエリ確認
docker exec [postgres_container] psql -U ubiquitous_user -d ubiquitous_db -c "SELECT pid, now() - pg_stat_activity.query_start AS duration, query FROM pg_stat_activity WHERE (now() - pg_stat_activity.query_start) > interval '5 minutes';"
```

**解決手順**:
```bash
# 1. 長時間クエリ終了（必要に応じて）
docker exec [postgres_container] psql -U ubiquitous_user -d ubiquitous_db -c "SELECT pg_terminate_backend([pid]);"

# 2. 接続プール再初期化
# アプリケーション再起動

# 3. インデックス最適化（定期メンテナンス時）
docker exec [postgres_container] psql -U ubiquitous_user -d ubiquitous_db -c "REINDEX DATABASE ubiquitous_db;"
```

### パフォーマンス問題

#### 「ユーザー一覧表示が遅い」
**症状**: 1000人以上のユーザーで一覧表示に10秒以上

**診断手順**:
```bash
# 1. ユーザー数確認
# 管理画面で確認、または直接データベース
docker exec [postgres_container] psql -U ubiquitous_user -d ubiquitous_db -c "SELECT COUNT(*) FROM \"AspNetUsers\";"

# 2. 実行プラン確認（開発環境）
# クエリの実行プラン分析
```

**解決手順**:
1. **ページングサイズ調整**
   - デフォルト20件 → 10件に変更
   - 管理画面設定で調整

2. **フィルタリング利用推奨**
   - ロール絞り込み
   - アクティブ状態絞り込み
   - 名前・メール部分検索

3. **インデックス確認**（開発者対応）
   - メールアドレス
   - ユーザー名
   - 作成日時

#### 「検索機能が遅い」
**症状**: 特定条件での検索に時間がかかる

**解決手順**:
1. **検索条件最適化**
   - 曖昧検索の制限
   - 必須フィルター追加

2. **データベース最適化**（週次メンテナンス）
```bash
# 統計情報更新
docker exec [postgres_container] psql -U ubiquitous_user -d ubiquitous_db -c "ANALYZE;"
```

### ユーザー管理問題

#### 「ユーザー作成ができない」
**症状**: 新規ユーザー作成時にエラー

**診断フロー**:
```
1. 作成者権限確認
   ├─ SuperUser/ProjectManager → 2へ
   └─ その他 → 権限不足エラー

2. 作成対象ロール確認
   ├─ 権限内 → 3へ
   └─ 権限外 → 権限制限エラー

3. メールアドレス重複確認
   ├─ 重複なし → 4へ
   └─ 重複あり → 重複エラー

4. バリデーションエラー確認
   ├─ 形式正常 → システムエラー調査
   └─ 形式異常 → 入力内容修正依頼
```

#### 「パスワード変更ができない」
**症状**: ユーザーがパスワード変更に失敗

**確認項目**:
1. **現在パスワード**の正確性
2. **新パスワード強度**
   - 8文字以上
   - 大文字・小文字・数字含む
3. **確認パスワード**の一致

**解決手順**:
1. **パスワード要件説明**
2. **強制リセット**（管理者権限必要）
3. **一時パスワード発行**

### セキュリティ問題

#### 「不正アクセス検知」
**症状**: 不審なログイン試行、権限外操作

**即座の対応**:
1. **該当ユーザーの一時停止**
```bash
# 管理画面でアカウント無効化
# または直接データベース更新（緊急時）
```

2. **アクセスログ確認**
3. **影響範囲調査**
4. **必要に応じてパスワード強制リセット**

### 統合テスト問題（Phase A3で解決予定）

#### 「テスト実行エラー」
**症状**: `dotnet test` でコンパイルエラー・実行時エラー

**既知の問題**:
1. **F# Option型問題**
   - C#テストでF#のOption型アクセス
   - `IsSome`プロパティエラー

2. **Command DTO問題**
   - F#レコード型の読み取り専用プロパティ
   - オブジェクト初期化子使用不可

3. **ApplicationUser問題**
   - UserEntityとテーブル競合
   - プロパティ不一致

**回避策**:
- 実アプリケーション動作確認に集中
- Phase A3での包括的解決待ち

## 🔧 復旧手順テンプレート

### 標準復旧手順
```bash
# 1. 現状把握
systemctl status ubiquitous-lang-mng  # または ps aux | grep dotnet
docker ps | grep postgres

# 2. ログ確認
tail -n 100 /var/log/ubiquitous-lang-mng/app.log
docker logs ubiquitous-lang-mng_postgresql_1

# 3. 基本復旧
docker-compose restart postgresql
sleep 10
systemctl restart ubiquitous-lang-mng

# 4. 動作確認
curl -I http://localhost:5000/health
# または管理画面アクセス確認
```

### データベース復旧手順
```bash
# 1. バックアップ確認
ls -la /backups/ubiquitous_db_*

# 2. 緊急復旧（最新バックアップから）
docker exec [postgres_container] pg_restore -U ubiquitous_user -d ubiquitous_db /backups/latest.backup

# 3. マイグレーション実行
dotnet ef database update --project src/UbiquitousLanguageManager.Infrastructure

# 4. データ整合性確認
# 管理画面でユーザー一覧確認
```

## 📞 エスカレーション基準

### レベル1（管理者対応）
- 設定変更
- ユーザーアカウント問題
- パフォーマンス調整

### レベル2（開発者対応）
- アプリケーションエラー
- データベーススキーマ問題
- セキュリティ問題

### レベル3（外部支援）
- インフラストラクチャ問題
- 大規模データ破損
- セキュリティインシデント

## 📊 問題分析・記録

### 問題記録テンプレート
```markdown
## 問題概要
- 発生日時: YYYY-MM-DD HH:MM
- 影響範囲: [全体/特定機能/特定ユーザー]
- 緊急度: [高/中/低]

## 症状
- [具体的な症状]
- [エラーメッセージ]

## 原因
- [判明した原因]

## 対応内容
- [実施した対応]
- [復旧時刻]

## 再発防止策
- [改善項目]
```

---

**緊急連絡先**: 開発チーム  
**エスカレーション**: システム管理者 → 開発チーム → 外部支援  
**最終更新**: 2025-07-20  