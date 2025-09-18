# ログ出力実装ガイドライン

**作成日**: 2025-09-19
**対象**: ユビキタス言語管理システム開発チーム
**関連ADR**: ADR_008_ログ出力指針・ADR_017_ログ管理実装戦略

## 🎯 基本方針

### Clean Architecture層別責務（ADR_008準拠）
- **Domain層 (F#)**: ログ出力完全禁止・Result型でエラー表現
- **Application層 (F#)**: ユースケース開始/終了・重要分岐点
- **Infrastructure層 (C#)**: データアクセス・外部システム連携・パフォーマンス
- **Web層 (C#)**: ユーザー操作・画面遷移・認証状態変更

### ログレベル選択基準
- **LogCritical**: システム停止・データ破損レベル
- **LogError**: 処理失敗・例外発生・回復不可能エラー
- **LogWarning**: 注意が必要な状態・性能劣化・非推奨機能使用
- **LogInformation**: 重要なビジネスイベント・状態変更・正常処理結果
- **LogDebug**: 開発時詳細情報（本番環境では出力されない）

## 📝 実装パターン

### 1. 基本的なログ出力

#### ✅ 推奨（構造化ログ）
```csharp
// 正常処理
_logger.LogInformation("ユーザーログイン成功 UserId: {UserId}, LoginTime: {LoginTime}",
    userId, DateTime.UtcNow);

// エラー処理
_logger.LogError(ex, "パスワード変更処理でエラーが発生 UserId: {UserId}, Error: {ErrorMessage}",
    userId, ex.Message);

// パフォーマンス計測
using var scope = _logger.BeginScope("DatabaseOperation UserId: {UserId}", userId);
_logger.LogDebug("データベース処理開始 Query: {Query}", query);
// ... データベース処理
_logger.LogDebug("データベース処理完了 Duration: {Duration}ms", stopwatch.ElapsedMilliseconds);
```

#### ❌ 禁止（文字列補間・Console.WriteLine）
```csharp
// 禁止: 文字列補間（パフォーマンス悪化・検索性低下）
_logger.LogInformation($"ユーザーログイン成功 {userId} {DateTime.UtcNow}");

// 禁止: Console.WriteLine（統一性なし・本番環境不適切）
Console.WriteLine($"デバッグ: {value}");

// 禁止: Exception詳細の不適切な出力
_logger.LogError(ex.ToString()); // セキュリティリスク
```

### 2. 層別実装例

#### Application層 (F#)
```fsharp
// ユースケース開始/終了
logger.LogInformation("認証処理開始 Email: {Email}", email)
match result with
| Success user ->
    logger.LogInformation("認証成功 UserId: {UserId}", user.Id)
    result
| Failure error ->
    logger.LogWarning("認証失敗 Email: {Email}, Error: {Error}", email, error.ToString())
    result
```

#### Infrastructure層 (C#)
```csharp
// データアクセス
_logger.LogDebug("ユーザー検索開始 Email: {Email}", email);
var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
if (user != null)
{
    _logger.LogInformation("ユーザー検索成功 UserId: {UserId}", user.Id);
}
else
{
    _logger.LogInformation("ユーザーが見つかりません Email: {Email}", email);
}
```

#### Web層 (C#)
```csharp
// 認証状態変更
_logger.LogInformation("ログアウト処理開始 UserId: {UserId}", userId);
await _signInManager.SignOutAsync();
_logger.LogInformation("ログアウト完了 UserId: {UserId}", userId);

// エラーハンドリング（ミドルウェア）
_logger.LogError(ex, "未処理例外が発生 Path: {Path}, Method: {Method}",
    context.Request.Path, context.Request.Method);
```

### 3. セキュリティ・個人情報配慮

#### ✅ 安全な情報
- ユーザーID（数値・GUID）
- 操作種別・処理名
- 実行時間・パフォーマンス指標
- HTTP ステータスコード・リクエストパス

#### ⚠️ 注意が必要な情報
- メールアドレス：仮名化推奨 `user***@example.com`
- パスワード：絶対にログ出力禁止
- 個人名：イニシャル化推奨 `田中太郎` → `T.T.`

#### ❌ 出力禁止
```csharp
// 禁止: パスワード・機密情報
_logger.LogDebug("パスワード: {Password}", password); // 絶対禁止

// 禁止: 詳細な例外スタック（本番環境）
_logger.LogError(ex.ToString()); // 内部実装露出リスク

// 禁止: SQL文に含まれるパラメータ値
_logger.LogDebug("SQL: {Sql}", sqlWithParameters); // SQLインジェクション情報漏洩
```

## 🔧 開発環境での活用

### Visual Studio / VS Code デバッグ出力
```csharp
// デバッグ時の詳細情報
_logger.LogDebug("Blazor Component初期化 ComponentName: {ComponentName}, Parameters: {@Parameters}",
    GetType().Name, parameters);

// StateHasChangedタイミング
_logger.LogDebug("UI状態更新 Component: {ComponentName}, Reason: {Reason}",
    GetType().Name, "DataLoaded");
```

### Entity Framework Core クエリログ
開発環境では`Microsoft.EntityFrameworkCore`を`Debug`レベルに設定済み：
- SQL クエリの自動出力
- パフォーマンス分析用データ
- N+1問題の検出支援

## 🚀 パフォーマンス考慮事項

### 高頻度処理での注意点
```csharp
// ✅ ログレベル事前チェック（高頻度処理）
if (_logger.IsEnabled(LogLevel.Debug))
{
    _logger.LogDebug("高頻度処理 Data: {@Data}", complexObject);
}

// ✅ シンプルなログ（高頻度処理）
_logger.LogDebug("処理完了 Id: {Id}", id);
```

### スコープ活用
```csharp
// リクエストスコープ（ミドルウェア）
using var scope = _logger.BeginScope("RequestId: {RequestId}", requestId);

// 操作スコープ
using var scope = _logger.BeginScope("UserOperation UserId: {UserId} Operation: {Operation}",
    userId, operationName);
```

## 📊 ログ分析・運用

### 重要なログパターン検索
```bash
# 認証エラー
grep "認証失敗" logs/*.log

# パフォーマンス問題
grep "Duration.*[5-9][0-9][0-9][0-9]" logs/*.log  # 5秒以上

# エラー発生状況
grep -E "LogError|LogCritical" logs/*.log | head -20
```

### 監視すべきログパターン
- **認証失敗の頻発**: セキュリティ攻撃の可能性
- **データベース接続エラー**: インフラ問題
- **未処理例外の増加**: アプリケーション品質問題
- **処理時間の増大**: パフォーマンス劣化

## 🔄 継続的改善

### ログ品質向上のポイント
1. **検索性**: 構造化パラメータの一貫した命名
2. **可読性**: 適切なログレベル・メッセージの明確性
3. **完全性**: エラーケースを含む全パターンのログ出力
4. **効率性**: 本番環境での適切なレベル設定

### 将来の拡張（Serilog移行準備）
現在のMicrosoft.Extensions.Loggingパターンは、将来のSerilog移行時にもそのまま活用可能：
- 構造化ログテンプレートの継続活用
- ログレベル・カテゴリ設計の継承
- Sink設定による出力先拡張（File・Database・クラウド）

---

**実装支援**: このガイドラインに沿ったログ実装について不明点がある場合は、ADR_008・ADR_017を参照するか、開発チーム内で相談してください。