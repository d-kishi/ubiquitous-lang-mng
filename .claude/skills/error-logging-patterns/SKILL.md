---
name: error-logging-patterns
description: エラーハンドリング・ログ出力パターンを適用する。「エラー処理実装」「ログ設計」「例外処理」「Result型活用」の際に使用する。
allowed-tools: Read, Grep, mcp__serena__find_symbol
---

# Error Logging Patterns Skill

Clean ArchitectureとF#関数型プログラミングを活かした層別エラーハンドリング・ログ出力パターン。ADR_007・ADR_008に基づく。

---

## 使用タイミング

1. **エラー処理実装時** - Domain/Application/Infrastructure/Presentation層でのエラー処理
2. **ログ出力設計時** - 新規機能・既存機能へのログ追加・ログレベル判断
3. **F#↔C#境界実装時** - Result型↔Exception型変換

---

## 層別エラー処理・ログ原則（概要）

| 層 | エラー処理 | ログ出力 |
|----|-----------|---------|
| **Domain (F#)** | Result型必須・例外禁止 | **禁止**（純粋関数維持） |
| **Application (F#)** | エラー伝播・変換 | ユースケース開始・終了 |
| **Infrastructure (C#)** | 例外キャッチ・変換 | パフォーマンス・エラー詳細 |
| **Presentation (C#)** | ユーザー向け表示 | 操作・認証状態 |

---

## 必須確認事項

### エラー処理
- Result型・Option型の適切な使用
- 層別エラー処理原則の遵守
- Domain層での例外使用禁止

### ログ出力
- Domain層ログ出力禁止原則
- 構造化ログ形式（{PropertyName}形式）
- Microsoft.Extensions.Logging + Serilog使用

---

## 詳細ルール

- **エラーハンドリング**: [`./references/error-handling.md`](./references/error-handling.md)
- **ログ出力指針**: [`./references/logging-guidelines.md`](./references/logging-guidelines.md)

---

## 関連ADR・Skills

- **ADR_007**: エラーハンドリング統一方針
- **ADR_008**: ログ出力指針
- **fsharp-csharp-bridge Skill**: F#↔C#境界の型変換パターン
- **clean-architecture-guardian Skill**: Clean Architecture準拠性チェック

---

**作成日**: 2025-12-20
**Phase**: Issue #83 Rules最適化実行
