# F# Option型 ↔ C# 統合パターン

## 概要

F# Option型（`FSharpOption<'T>`）をC#で使用する際の変換パターン。null参照の安全な代替として機能。

## パターン1: Option型の生成

### C#からF# Option型を生成

```csharp
// Some/None生成（Phase B1 Step7確立）
FSharpOption<string>.Some("値あり")
FSharpOption<string>.None

// 条件分岐での生成（推奨パターン）
string.IsNullOrEmpty(description)
    ? FSharpOption<string>.None
    : FSharpOption<string>.Some(description)
```

### Blazor Serverコンポーネントでの実用例

```csharp
// プロジェクト作成時の説明（オプショナル）
var query = new GetProjectsQuery(
    userId: currentUser.Id,
    userRole: currentUserRole,
    pageNumber: currentPage,
    pageSize: pageSize,
    includeInactive: showDeleted,
    searchKeyword: string.IsNullOrWhiteSpace(searchTerm)
        ? FSharpOption<string>.None
        : FSharpOption<string>.Some(searchTerm)
);
```

### ポイント

- **Some**: 値が存在する場合
- **None**: 値が存在しない場合（nullの安全な代替）
- **条件演算子**: 三項演算子でシンプルに生成

## パターン2: Option型の値取得

### IsSomeチェック後の値取得

```csharp
// Option型の値取得（Phase B1 Step7確立）
var descriptionOption = project.Description.Value;

if (descriptionOption != null && FSharpOption<string>.get_IsSome(descriptionOption))
{
    string description = descriptionOption.Value;
    // descriptionを使用
}
```

### 簡潔なパターン（推奨）

```csharp
// より簡潔なパターン
if (project.Description.IsSome)
{
    var description = project.Description.Value;
    // descriptionを使用
}
else
{
    // Noneの場合の処理
}
```

### Null合体演算子との組み合わせ

```csharp
// デフォルト値提供パターン
var description = project.Description.IsSome
    ? project.Description.Value
    : "説明なし";
```

## パターン3: null許容型 ↔ Option型変換

### null許容型からOption型へ

```csharp
// C# null許容型 → F# Option型
string? nullableDescription = GetDescription(); // C# null許容型

var optionDescription = nullableDescription != null
    ? FSharpOption<string>.Some(nullableDescription)
    : FSharpOption<string>.None;
```

### Option型からnull許容型へ

```csharp
// F# Option型 → C# null許容型
var optionDescription = project.Description; // F# Option<string>

string? nullableDescription = optionDescription.IsSome
    ? optionDescription.Value
    : null;
```

## よくあるエラーと解決方法

### エラー1: Valueプロパティ直接アクセス

```
System.NullReferenceException: Object reference not set to an instance of an object.
```

**原因**: IsSomeチェック前にValueアクセス

**解決**:
```csharp
// ❌ 誤り
var description = project.Description.Value; // IsSomeチェックなし

// ✅ 正しい
if (project.Description.IsSome)
{
    var description = project.Description.Value;
}
```

### エラー2: nullとの比較

```csharp
// ❌ 誤り（Option型はnullではない）
if (project.Description == null)
{
    // このパターンは動作しない
}

// ✅ 正しい
if (!project.Description.IsSome) // または project.Description.IsNone
{
    // Noneの場合の処理
}
```

### エラー3: Option<Option<T>>（ネスト）

```
Error: Unexpected nested Option type
```

**原因**: Option型のネスト（通常は設計の問題）

**解決**:
```csharp
// ❌ 避けるべき
FSharpOption<FSharpOption<string>> nested;

// ✅ 推奨（ネストを避ける）
FSharpOption<string> flat;
```

## F# Option型の利点

### null参照との違い

| 観点 | F# Option型 | C# null |
|------|-------------|---------|
| **型安全性** | ✅ コンパイル時チェック | ❌ 実行時エラー可能 |
| **明示性** | ✅ Some/Noneで明示 | ❌ nullは暗黙的 |
| **パターンマッチング** | ✅ F#で強力 | ❌ 限定的 |
| **デフォルト値** | ✅ Noneケース処理強制 | ❌ null参照例外リスク |

### 使用推奨ケース

1. **オプショナルな値**: 説明・コメント・備考等
2. **検索結果**: 見つからない可能性がある値
3. **設定値**: デフォルト値がある設定項目
4. **ユーザー入力**: 入力されない可能性がある項目

## Phase B1での実証データ

- **適用箇所**: Domain層ValueObjects 8ファイル・Blazor Server 3コンポーネント
- **エラー修正**: 6件のOption型null参照エラー完全解決
- **成功率**: 100%（0 Warning/0 Error達成）
- **null参照例外**: ゼロ件（完全な型安全性達成）

## 参考情報

- **F# Option型定義**: `Microsoft.FSharp.Core.FSharpOption<'T>`
- **Phase B1実装記録**: `Doc/08_Organization/Completed/Phase_B1/Step07_完了報告.md`
- **tech_stack_and_conventionsメモリー**: F# Option型セクション
