# 開発ガイドライン

## セッション情報源選択ガイドライン（重要）

### セッション目的に応じた情報源選択
- **Phase作業**: `/Doc/08_Organization/Active/Phase_XX/` を必ず確認
- **技術負債対応**: `/Doc/10_Debt/Technical/` の該当文書確認
- **通常開発**: 要件・設計書を確認
- **原則**: セッション目的から適切な情報源を導出する

## 重要な設計パターンとガイドライン

### Clean Architecture実装パターン
- **Domain層**: F#でビジネスロジック・エンティティ・値オブジェクト
- **Application層**: F#でユースケース・サービス
- **Infrastructure層**: C#でデータアクセス・外部システム連携
- **Web層**: C#でBlazor Server・MVC共存
- **Contracts層**: C#でDTO・TypeConverters（F#↔C#境界）

### F#↔C#境界設計
- **TypeConverters**: Domain ↔ DTO変換の責務
- **Option型**: C#でnullable参照型に変換
- **Result型**: C#でカスタム結果型に変換
- **非同期**: F# Async ↔ C# Task変換

### Blazor Server設計パターン
- **認証**: CustomAuthenticationStateProvider使用
- **状態管理**: StateHasChanged()の適切な使用
- **DI**: DbContextFactory使用（マルチスレッド対応）
- **ライフサイクル**: コンポーネントのDisposeパターン

### テスト設計パターン
- **単体テスト**: 各層独立したテスト
- **統合テスト**: TestWebApplicationFactoryパターン
- **TDD**: Red-Green-Refactorサイクル厳守
- **カバレッジ**: 95%以上維持

### 品質管理
- **エラーハンドリング**: グローバル例外処理 + 業務例外
- **セキュリティ**: CSRF・XSS対策・認証強化
- **パフォーマンス**: 適切なキャッシュ・非同期処理
- **ログ**: 構造化ログ・監査ログ

### 開発プロセス
- **スクラム**: 1-2週間スプリント
- **SubAgentプール方式**: 並列実行・専門化
- **継続的品質改善**: 各Phase終了時のレトロスペクティブ