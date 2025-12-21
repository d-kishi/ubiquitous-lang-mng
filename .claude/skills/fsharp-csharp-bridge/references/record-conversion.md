# F# Record型 ↔ C# 統合パターン

## 目次

- [概要](#概要)
- [重要な原則: F# Record型は不変型](#重要な原則-f-record型は不変型)
- [パターン1: コンストラクタベース初期化（必須）](#パターン1-コンストラクタベース初期化必須)
- [パターン2: camelCaseパラメータ使用](#パターン2-camelcaseパラメータ使用)
- [パターン3: Option型フィールドの初期化](#パターン3-option型フィールドの初期化)
- [パターン4: Record型からDTOへの変換](#パターン4-record型からdtoへの変換)
- [よくあるエラーと解決方法](#よくあるエラーと解決方法)
- [C# Record型との違い](#c-record型との違い)
- [Phase B1での実証データ](#phase-b1での実証データ)
- [ベストプラクティス](#ベストプラクティス)

---

## 概要

F# Record型をC#で使用する際の変換パターン。**F# Recordは不変型**であり、C#のオブジェクト初期化子パターンは使用不可。コンストラクタベース初期化が必須。

## 重要な原則: F# Record型は不変型

F# Record型の特性：
- **イミュータブル**: 一度作成したら変更不可
- **Read-onlyプロパティ**: すべてのプロパティが読み取り専用
- **コンストラクタ必須**: 初期化時にすべてのフィールドを指定

## パターン1: コンストラクタベース初期化（必須）

### 正しいパターン（Phase B1 Step7確立）

```csharp
// F# Record型定義（Domain層）
type GetProjectsQuery = {
    UserId: Guid
    UserRole: Role
    PageNumber: int
    PageSize: int
    IncludeInactive: bool
    SearchKeyword: string option
}

// ✅ 正しいパターン（コンストラクタ使用）
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

### 誤ったパターン（Phase B1 Step7で36件のエラー発生）

```csharp
// ❌ 誤り（オブジェクト初期化子は使用不可）
var query = new GetProjectsQuery
{
    UserId = currentUser.Id,          // Error: Read-only property
    UserRole = currentUserRole,       // Error: Read-only property
    PageNumber = currentPage,         // Error: Read-only property
    PageSize = pageSize,              // Error: Read-only property
    IncludeInactive = showDeleted,    // Error: Read-only property
    SearchKeyword = /* ... */         // Error: Read-only property
};

// エラーメッセージ:
// Error CS0200: Property or indexer cannot be assigned to -- it is read only
```

## パターン2: camelCaseパラメータ使用

### F# Record → C# コンストラクタ

F# Recordのフィールドは通常PascalCaseですが、C#からのコンストラクタ呼び出しではcamelCaseパラメータを使用します。

```fsharp
// F# Record定義
type CreateProjectCommand = {
    Name: string           // PascalCase
    Description: string option
    OwnerId: Guid
}
```

```csharp
// C#からの呼び出し（camelCaseパラメータ）
var command = new CreateProjectCommand(
    name: "新規プロジェクト",        // camelCase
    description: FSharpOption<string>.Some("説明"),  // camelCase
    ownerId: currentUser.Id         // camelCase
);
```

### なぜcamelCaseか

- **F#コンパイラ生成**: F#コンパイラが自動生成するコンストラクタはcamelCaseパラメータ
- **C#規約との整合性**: C#のメソッドパラメータ規約と一致
- **IntelliSense**: Visual StudioのIntelliSenseでcamelCaseで表示

## パターン3: Option型フィールドの初期化

### Option型を含むRecord初期化

```csharp
// Option型フィールドを含むRecord（Phase B1 Step7確立）
var command = new CreateProjectCommand(
    name: projectName,
    description: string.IsNullOrWhiteSpace(descriptionInput)
        ? FSharpOption<string>.None
        : FSharpOption<string>.Some(descriptionInput),
    ownerId: currentUser.Id
);
```

### デフォルト値パターン

```csharp
// デフォルト値を使用（通常はNone）
var command = new CreateProjectCommand(
    name: projectName,
    description: FSharpOption<string>.None, // デフォルト: 説明なし
    ownerId: currentUser.Id
);
```

## パターン4: Record型からDTOへの変換

### TypeConverter実装での変換

```csharp
// F# Record → C# DTO（Contracts層）
public static class ProjectQueryConverters
{
    public static GetProjectsQuery ToFSharpQuery(this GetProjectsQueryDto dto)
    {
        var userRole = AuthenticationMapper.StringToRole(dto.UserRole)
            .GetValueOrThrow(); // 認証済み前提

        return new GetProjectsQuery(
            userId: dto.UserId,
            userRole: userRole,
            pageNumber: dto.PageNumber,
            pageSize: dto.PageSize,
            includeInactive: dto.IncludeInactive,
            searchKeyword: string.IsNullOrWhiteSpace(dto.SearchKeyword)
                ? FSharpOption<string>.None
                : FSharpOption<string>.Some(dto.SearchKeyword)
        );
    }
}
```

## よくあるエラーと解決方法

### エラー1: Read-onlyプロパティへの代入

```
Error CS0200: Property or indexer 'GetProjectsQuery.UserId' cannot be assigned to -- it is read only
```

**原因**: オブジェクト初期化子を使用

**解決**:
```csharp
// ❌ 誤り
var query = new GetProjectsQuery { UserId = id };

// ✅ 正しい
var query = new GetProjectsQuery(userId: id, ...);
```

### エラー2: コンストラクタパラメータ不足

```
Error CS7036: There is no argument given that corresponds to the required formal parameter
```

**原因**: すべてのフィールドを指定していない

**解決**:
```csharp
// F# Recordのすべてのフィールドを指定
var query = new GetProjectsQuery(
    userId: id,
    userRole: role,
    pageNumber: 1,      // 必須
    pageSize: 10,       // 必須
    includeInactive: false,  // 必須
    searchKeyword: FSharpOption<string>.None  // 必須
);
```

### エラー3: パラメータ名の大文字小文字

```
Error CS1739: The best overload for 'GetProjectsQuery' does not have a parameter named 'UserId'
```

**原因**: PascalCaseを使用（正しくはcamelCase）

**解決**:
```csharp
// ❌ 誤り
var query = new GetProjectsQuery(UserId: id); // PascalCase

// ✅ 正しい
var query = new GetProjectsQuery(userId: id); // camelCase
```

## C# Record型との違い

### C# 9.0+ Record vs F# Record

| 観点 | C# Record | F# Record |
|------|-----------|-----------|
| **初期化** | ✅ オブジェクト初期化子可能 | ❌ コンストラクタのみ |
| **with式** | ✅ `with { Prop = value }` | ✅ F#でのみ使用可能 |
| **不変性** | 🟡 initプロパティ | ✅ 完全な不変性 |
| **パラメータ名** | PascalCase | camelCase（C#から使用時） |

**重要**: C# RecordとF# Recordは**異なる概念**。混同しないこと。

## Phase B1での実証データ

- **適用箇所**: Blazor Server 3コンポーネント・Contracts層7ファイル
- **エラー修正**: 36件のRead-onlyプロパティエラー完全解決
- **成功率**: 100%（0 Warning/0 Error達成）
- **オブジェクト初期化子誤用**: ゼロ件（正しいパターン確立）

## ベストプラクティス

### 1. 名前付きパラメータ使用（推奨）

```csharp
// ✅ 推奨（可読性高い）
var query = new GetProjectsQuery(
    userId: id,
    userRole: role,
    pageNumber: 1,
    pageSize: 10,
    includeInactive: false,
    searchKeyword: FSharpOption<string>.None
);

// 🟡 許容（短いRecord）
var simple = new SimpleRecord(value1, value2);
```

### 2. Option型の明示的処理

```csharp
// ✅ 推奨（Option型の明示的生成）
description: string.IsNullOrWhiteSpace(input)
    ? FSharpOption<string>.None
    : FSharpOption<string>.Some(input)

// ❌ 避ける（nullを直接使用）
description: input  // コンパイルエラー
```

### 3. TypeConverter活用

```csharp
// ✅ 推奨（TypeConverterでカプセル化）
public static CreateProjectCommand ToFSharpCommand(this CreateProjectCommandDto dto)
{
    return new CreateProjectCommand(
        name: dto.Name,
        description: dto.Description.ToFSharpOption(),
        ownerId: dto.OwnerId
    );
}

// Blazor Serverからはシンプルに使用
var command = dto.ToFSharpCommand();
```

## 参考情報

- **F# Record型定義**: `type RecordName = { Field1: Type1; Field2: Type2 }`
- **Phase B1実装記録**: `Doc/08_Organization/Completed/Phase_B1/Step07_完了報告.md`
- **contracts-bridge Agent**: `.claude/agents/contracts-bridge.md`
- **tech_stack_and_conventionsメモリー**: F# Record型セクション
