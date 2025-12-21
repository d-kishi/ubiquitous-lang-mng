---
name: fsharp-csharp-bridge
description: F#とC#間の型変換パターンを適用する。「F#↔C#境界実装」「型変換エラー」「Option/Result型変換」「Contracts層実装」の際に使用する。
allowed-tools: Read, Grep
---

# F#↔C# Type Conversion Patterns

F# Domain/Application層とC# Infrastructure/Web層の境界で発生する型変換パターン。Phase B1 Step7で確立・実証。

## 使用タイミング

1. **F#↔C#境界コード実装時** - Contracts層TypeConverter・Blazor→F# Application呼び出し
2. **型変換エラー発生時** - IsOkアクセス・Read-onlyプロパティ・Option型null参照エラー
3. **契約層作業時** - contracts-bridge Agent作業・DTO↔Domainモデル変換

---

## 4つの型変換パターン

### 1. F# Result型 ↔ C# 統合パターン

**詳細**: [`references/result-conversion.md`](./references/result-conversion.md)

**概要**: IsOk/ResultValueアクセス・NewOk/NewError生成・Railway-oriented Programming統合

**典型エラー**: `'FSharpResult<T, E>' does not contain a definition for 'IsOk'`

### 2. F# Option型 ↔ C# 統合パターン

**詳細**: [`references/option-conversion.md`](./references/option-conversion.md)

**概要**: Some/None生成・IsSome/Valueアクセス・null許容型変換

### 3. F# Discriminated Union ↔ C# 統合パターン

**詳細**: [`references/du-conversion.md`](./references/du-conversion.md)

**概要**: switch式パターンマッチング・Role型統合・Enumとの違い

**典型エラー**: `The type or namespace name 'Role' could not be found`

### 4. F# Record型 ↔ C# 統合パターン

**詳細**: [`references/record-conversion.md`](./references/record-conversion.md)

**概要**: コンストラクタベース初期化（必須）・camelCaseパラメータ・Read-only対応

**典型エラー**: `Property or indexer cannot be assigned to -- it is read only`

---

## 品質基準

- ✅ **型安全性**: コンパイル時型チェック完全通過
- ✅ **実行時安全性**: null参照例外ゼロ
- ✅ **可読性**: F#/C#それぞれの慣用句に準拠

## Phase B1実証結果

- 36ファイル修正・36件型変換エラー完全解決
- 97/100点（Clean Architecture準拠）・0 Warning/0 Error達成

## 関連Agents

- **contracts-bridge**: F#↔C#境界実装専門（このSkillの知見を活用）
- **csharp-web-ui**: Blazor Server実装時に参照
- **csharp-infrastructure**: Infrastructure層実装時に参照

---

**作成日**: 2025-10-21
