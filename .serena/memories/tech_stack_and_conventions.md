# 技術スタックとコーディング規約

## コーディング規約とスタイル

### F# (Domain/Application層)
- **型定義**: 値オブジェクトとエンティティの明確な分離
- **命名規則**: PascalCase for types, camelCase for values
- **コメント**: 初学者向け詳細説明必須 (ADR_010)
- **Option型**: null安全性のために積極的に使用
- **Result型**: エラーハンドリングに使用
- **パターンマッチング**: 網羅的なケース処理

### C# (Infrastructure/Web/Contracts層)  
- **命名規則**: Microsoft C# 標準規約に準拠
- **コメント**: Blazor Server初学者向け詳細説明必須
- **非同期処理**: async/await パターン
- **依存注入**: ASP.NET Core標準DIコンテナ使用

### 設計パターン
- **Clean Architecture**: 依存関係逆転の原則
- **Repository パターン**: データアクセス抽象化
- **Factory パターン**: エンティティ作成
- **TypeConverter パターン**: F#↔C#境界での型変換

### 品質基準
- **ビルド**: 0 Warning, 0 Error状態維持必須
- **テスト**: TDD実践・95%以上のカバレッジ目標
- **用語統一**: 「用語」ではなく「ユビキタス言語」使用

## 特記事項
- **初学者対応**: Blazor Server・F#初学者のため詳細コメント必須
- **技術決定**: 重要な決定はADR (Architecture Decision Record) として記録
- **セキュリティ**: CSRF・XSS対策・認証セキュリティ強化済み