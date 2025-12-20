---
name: error-logging-patterns
description: エラーハンドリング・ログ出力パターンを適用する。「エラー処理実装」「ログ設計」「例外処理」「Result型活用」の際に使用する。
allowed-tools: Read, Grep, mcp__serena__find_symbol
---

# Error Logging Patterns Skill

## 概要

このSkillは、Clean ArchitectureとF#関数型プログラミングを活かした層別エラーハンドリング・ログ出力パターンの自律的適用を提供します。ADR_007（エラーハンドリング）・ADR_008（ログ出力指針）に基づく実装パターンを定義します。

---

## 使用タイミング

Claudeは以下の状況でこのSkillを自律的に使用すべきです：

### 1. エラー処理実装時（最重要）

**タイミング**:
- Domain層でのビジネスエラー定義
- Application層でのエラー変換・統合
- Infrastructure層での例外処理実装
- Presentation層でのユーザー向けエラー表示

**必須確認事項**:
- 層別エラー処理原則の遵守
- Result型・Option型の適切な使用
- 例外使用禁止原則（Domain層）

### 2. ログ出力設計時

**タイミング**:
- 新規機能実装時のログ設計
- 既存機能へのログ追加
- ログレベル設定の判断

**必須確認事項**:
- 層別ログ責務の遵守
- Domain層ログ出力禁止原則
- 構造化ログ形式の適用

### 3. F#↔C#境界実装時

**タイミング**:
- Result型からException型への変換
- Exception型からResult型への変換
- 境界でのエラー情報伝播

---

## 層別エラー処理原則

### Domain層 (F#)

- **原則**: Result型による明示的エラー処理・例外使用禁止
- **エラー型**: discriminated unionによる型安全なエラー表現
- **ログ**: 出力禁止（純粋関数維持）

### Application層 (F#)

- **原則**: Domainエラーの伝播・技術エラーの例外変換
- **エラー型**: DomainエラーからApplicationエラーへの変換
- **ログ**: ユースケース開始・終了・重要分岐点

### Infrastructure層 (C#)

- **原則**: 外部システムエラーの適切なキャッチ・変換
- **エラー型**: 技術例外からドメイン例外への変換
- **ログ**: パフォーマンス情報・接続状態・エラー詳細

### Presentation層 (C#)

- **原則**: ユーザーフレンドリーなエラー表示
- **エラー型**: UI状態管理・エラーメッセージ表示
- **ログ**: 画面遷移・重要操作・認証状態

---

## ログレベル戦略

| レベル | 用途 |
|--------|------|
| **Critical** | システム停止レベルの致命的エラー |
| **Error** | 機能レベルの問題・例外処理 |
| **Warning** | 潜在的問題・ビジネスエラー |
| **Information** | 重要な業務処理・状態変更 |
| **Debug** | 開発時詳細情報（本番環境無効） |

---

## 詳細ルール

- **エラーハンドリング**: [`./rules/error-handling.md`](./rules/error-handling.md)
- **ログ出力指針**: [`./rules/logging-guidelines.md`](./rules/logging-guidelines.md)

---

## 関連ADR・Skills

- **ADR_007**: エラーハンドリング統一方針
- **ADR_008**: ログ出力指針
- **fsharp-csharp-bridge Skill**: F#↔C#境界の型変換パターン
- **clean-architecture-guardian Skill**: Clean Architecture準拠性チェック

---

**作成日**: 2025-12-20
**Phase**: Issue #83 Rules最適化実行
