# 技術スタック・実装規約（2025-01-19更新）

## 技術スタック

### フロントエンド
- **Blazor Server**: リアルタイムUI・SignalR接続・ステート管理
- **Bootstrap 5**: レスポンシブデザイン・アクセシビリティ配慮
- **JavaScript interop**: 最小限・auth-api.js認証API連携のみ

### バックエンド
- **ASP.NET Core 8.0**: Web API・認証・ミドルウェア
- **Entity Framework Core**: PostgreSQL ORM・マイグレーション管理
- **F# Application層**: 関数型プログラミング・型安全性
- **F# Domain層**: DDD・ユビキタス言語・不変性

### データベース
- **PostgreSQL 16**: メインDB・Docker Container運用
- **PgAdmin**: 管理ツール（http://localhost:8080）
- **Smtp4dev**: 開発用メールサーバー（http://localhost:5080）

### 開発ツール
- **Docker Compose**: 開発環境統一・依存サービス管理
- **Claude Code**: AI支援開発・SubAgent活用・並列実行

## 実装規約

### ログ実装規約（2025-01-19確立）
```csharp
// ✅ 正しい実装
Logger.LogInformation("ユーザーログイン成功 Email: {Email}, Duration: {ElapsedMs}ms", 
    MaskEmail(email), stopwatch.ElapsedMilliseconds);

// ❌ 禁止パターン
Console.WriteLine($"ユーザーログイン: {email}"); // セキュリティリスク
```

### Clean Architecture層責務
- **Web層**: UI状態・ユーザー入力・画面遷移・操作ログ
- **Infrastructure層**: DB操作・外部API・Repository・パフォーマンスログ
- **Application層**: ビジネスロジック・F# Result型・エラーハンドリング
- **Domain層**: ドメインモデル・F#純粋実装・ログ出力禁止
- **Contracts層**: DTO・TypeConverter・境界変換ログ

### TypeConverter規約
```fsharp
// F# → C# 変換パターン
static member ToDto(model: AuthenticationResult) : AuthenticatedUserDto =
    Logger.LogDebug("TypeConverter実行 Type: {Type}", "AuthenticationResult->AuthenticatedUserDto")
    // 変換ロジック実装
```

### セキュリティ実装規約
1. **パスワード**: ログ出力絶対禁止・Hashのみ記録
2. **メールアドレス**: MaskEmail()関数使用・個人情報保護
3. **認証Cookie**: HttpOnly・Secure・SameSite設定
4. **セッション管理**: ASP.NET Core Identity標準準拠

## パフォーマンス規約

### 測定パターン
```csharp
var stopwatch = Stopwatch.StartNew();
// 処理実行
Logger.LogDebug("処理完了 Method: {Method}, Duration: {ElapsedMs}ms", 
    nameof(MethodName), stopwatch.ElapsedMilliseconds);
```

### 最適化基準
- **DB Query**: EF Core実行時間監視・N+1問題防止
- **Blazor Rendering**: StateHasChanged()最小化・レンダリング最適化
- **Memory**: IDisposable適切実装・リソース解放確実実行

## 開発環境設定
```bash
# 必須起動順序
docker-compose up -d                    # PostgreSQL/PgAdmin/Smtp4dev
dotnet run --project src/UbiquitousLanguageManager.Web  # アプリ起動
# https://localhost:5001 でアクセス
```