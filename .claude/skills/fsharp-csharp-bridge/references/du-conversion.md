# F# Discriminated Union ↔ C# 統合パターン

## 目次

- [概要](#概要)
- [パターン1: switch式によるパターンマッチング（推奨）](#パターン1-switch式によるパターンマッチング推奨)
- [パターン2: Discriminated Unionの値生成](#パターン2-discriminated-unionの値生成)
- [パターン3: パラメータ付きDiscriminated Union](#パターン3-パラメータ付きdiscriminated-union)
- [Enum vs Discriminated Union](#enum-vs-discriminated-union)
- [よくあるエラーと解決方法](#よくあるエラーと解決方法)
- [実用的な使用例](#実用的な使用例)
- [Phase B1での実証データ](#phase-b1での実証データ)
- [参考情報](#参考情報)

---

## 概要

F# Discriminated Union（判別共用体）をC#で使用する際の変換パターン。EnumとDUの違いを理解し、適切なパターンマッチングを実装。

## パターン1: switch式によるパターンマッチング（推奨）

### Role型（Discriminated Union）のC#統合

```csharp
// F# Discriminated Union定義（Domain層）
type Role =
    | SuperUser
    | ProjectManager
    | DomainApprover
    | GeneralUser

// C#でのパターンマッチング（Phase B1 Step7確立）
currentUserRole = roleClaim.Value switch
{
    "SuperUser" => Role.SuperUser,
    "ProjectManager" => Role.ProjectManager,
    "DomainApprover" => Role.DomainApprover,
    "GeneralUser" => Role.GeneralUser,
    _ => Role.GeneralUser  // デフォルト値
};
```

### switch式のポイント

1. **文字列比較**: 判別共用体のケース名を文字列で比較
2. **静的プロパティアクセス**: `Role.SuperUser`形式
3. **デフォルトケース**: `_`パターンで未知の値に対応
4. **型安全**: コンパイル時に全ケースチェック可能

## パターン2: Discriminated Unionの値生成

### C#からDUを生成

```csharp
// Discriminated Unionの値生成
Role role = Role.SuperUser;
Role projectManager = Role.ProjectManager;

// 関数からの返却
public Role GetUserRole(string roleString)
{
    return roleString switch
    {
        "SuperUser" => Role.SuperUser,
        "ProjectManager" => Role.ProjectManager,
        _ => Role.GeneralUser
    };
}
```

## パターン3: パラメータ付きDiscriminated Union

### パラメータを持つDUのC#統合

```fsharp
// F# 定義（パラメータ付きDU）
type ProjectError =
    | InvalidProjectName of string
    | DuplicateProject of projectId: Guid
    | ProjectNotFound of projectId: Guid
    | DatabaseError of message: string
```

```csharp
// C#でのパターンマッチング（Phase B1 Step7確立）
if (error.IsInvalidProjectName)
{
    var errorMessage = error as ProjectError.InvalidProjectName;
    Console.WriteLine($"Invalid name: {errorMessage.Item}");
}
else if (error.IsDuplicateProject)
{
    var errorData = error as ProjectError.DuplicateProject;
    Console.WriteLine($"Duplicate ID: {errorData.projectId}");
}
```

### パラメータアクセス

- **Itemプロパティ**: 単一パラメータの場合
- **名前付きフィールド**: 名前付きパラメータの場合
- **Is{ケース名}プロパティ**: ケース判定用プロパティ

## Enum vs Discriminated Union

### 重要な違い

| 観点 | Enum | Discriminated Union |
|------|------|---------------------|
| **値の型** | 整数 | 任意の型（パラメータ可能） |
| **パターンマッチング** | switch文 | switch式 + 型チェック |
| **型安全性** | ❌ キャスト可能 | ✅ 完全な型安全 |
| **パラメータ** | ❌ 不可 | ✅ 可能 |
| **C#互換性** | ✅ ネイティブ | 🟡 F#ライブラリ経由 |

### よくある誤り（Enumと誤認）

```csharp
// ❌ 誤り（Roleは値型Enumではない）
if (Enum.TryParse<Role>(roleClaim.Value, out var role))
{
    // Error: Roleは値型ではない
}

// ✅ 正しい（switch式でパターンマッチング）
currentUserRole = roleClaim.Value switch
{
    "SuperUser" => Role.SuperUser,
    "ProjectManager" => Role.ProjectManager,
    _ => Role.GeneralUser
};
```

## よくあるエラーと解決方法

### エラー1: Enum.TryParseの誤用

```
Error CS0452: The type 'Role' must be a non-nullable value type
```

**原因**: Discriminated UnionをEnumとして扱おうとした

**解決**:
```csharp
// ❌ 誤り
Enum.TryParse<Role>(value, out var role)

// ✅ 正しい
var role = value switch
{
    "SuperUser" => Role.SuperUser,
    _ => Role.GeneralUser
};
```

### エラー2: ケース名の完全修飾

```
Error CS0246: The type or namespace name 'Role' could not be found
```

**原因**: using文不足または名前空間の問題

**解決**:
```csharp
using UbiquitousLanguageManager.Domain.Common;

// または完全修飾名
var role = UbiquitousLanguageManager.Domain.Common.Role.SuperUser;
```

### エラー3: パラメータアクセスエラー

```
Error CS1061: 'ProjectError' does not contain a definition for 'Message'
```

**原因**: パラメータ名の誤り（F#定義確認必要）

**解決**:
```csharp
// F#定義確認
// type ProjectError = DatabaseError of message: string

// ✅ 正しい（名前付きパラメータ）
if (error.IsDatabaseError)
{
    var dbError = error as ProjectError.DatabaseError;
    Console.WriteLine(dbError.message); // 小文字の'message'
}
```

## 実用的な使用例

### 権限チェック処理

```csharp
// 権限チェック（Phase B1 Step7確立）
bool CanDeleteProject(Role role, Guid ownerId, Guid userId)
{
    return role switch
    {
        var r when r.IsSuperUser => true,
        var r when r.IsProjectManager => true,
        var r when r.IsGeneralUser => ownerId == userId,
        _ => false
    };
}
```

### エラーメッセージ生成

```csharp
// エラーメッセージ生成（ProjectError DU使用）
string GetErrorMessage(ProjectError error)
{
    if (error.IsInvalidProjectName)
    {
        var e = error as ProjectError.InvalidProjectName;
        return $"プロジェクト名が無効です: {e.Item}";
    }
    else if (error.IsDuplicateProject)
    {
        var e = error as ProjectError.DuplicateProject;
        return $"プロジェクトID {e.projectId} は既に存在します";
    }
    else if (error.IsProjectNotFound)
    {
        var e = error as ProjectError.ProjectNotFound;
        return $"プロジェクトID {e.projectId} が見つかりません";
    }
    else if (error.IsDatabaseError)
    {
        var e = error as ProjectError.DatabaseError;
        return $"データベースエラー: {e.message}";
    }

    return "不明なエラー";
}
```

## Phase B1での実証データ

- **適用箇所**: Web層3コンポーネント・Contracts層5ファイル
- **エラー修正**: 8件のDUパターンマッチングエラー完全解決
- **成功率**: 100%（0 Warning/0 Error達成）
- **Enum誤用件数**: ゼロ件（正しいパターン確立）

## 参考情報

- **F# Discriminated Union**: `type DU = Case1 | Case2 of 'T`
- **Phase B1実装記録**: `Doc/08_Organization/Completed/Phase_B1/Step07_完了報告.md`
- **Domain層Role定義**: `src/UbiquitousLanguageManager.Domain/Common/CommonTypes.fs`
