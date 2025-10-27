# F# Result型 ↔ C# 統合パターン

## 概要

F# Result型（`FSharpResult<'T, 'TError>`）をC#で使用する際の変換パターン。Phase B1 Step7で確立・実証済み。

## パターン1: IsOkプロパティ直接アクセス（推奨）

### C#からF# Result型を処理

```csharp
// Blazor Serverコンポーネント内での使用例（Phase B1 Step7確立）
var result = await ProjectManagementService.GetProjectsAsync(query);

// ✅ 正しいパターン（推奨）
if (result.IsOk)
{
    var listResult = result.ResultValue;
    projects = listResult.Projects.ToList();
}
else
{
    errorMessage = result.ErrorValue;
}
```

### なぜこのパターンが推奨されるか

1. **型安全**: `IsOk`はboolプロパティ、コンパイル時チェック
2. **シンプル**: 追加の変換関数不要
3. **パフォーマンス**: プロパティアクセスのみ、オーバーヘッドなし
4. **Phase B1実証済み**: 36件の実装で100%成功率

## パターン2: C#からF# Result型を生成

### TypeConverter実装でのResult生成

```csharp
// Contracts層TypeConverter実装（Phase B1 Step3確立）
public static FSharpResult<User, AuthenticationError> ToFSharpResult(this AuthenticationResultDto dto)
{
    // ✅ 正しいパターン
    return dto.IsSuccess
        ? FSharpResult<User, AuthenticationError>.NewOk(dto.User.ToDomainModel())
        : FSharpResult<User, AuthenticationError>.NewError(dto.Error);
}
```

### ポイント

- **NewOk**: 成功値を持つResult生成（静的メソッド）
- **NewError**: エラー値を持つResult生成（静的メソッド）
- **完全修飾名**: `Microsoft.FSharp.Core.FSharpResult<T, TError>`が必要な場合あり

## パターン3: Railway-oriented Programming統合

### Blazor ServerでのROP統合

```csharp
// Application層呼び出し（Railway-oriented Programming）
var result = await ProjectManagementService.CreateProjectAsync(command);

// Result型のパターンマッチング処理
if (result.IsOk)
{
    var creationResult = result.ResultValue;

    // 成功時: Toast表示してリダイレクト
    await ShowToast("success", "プロジェクトとデフォルトドメイン「共通」を作成しました");
    NavigationManager.NavigateTo("/projects");
}
else
{
    // エラー時: エラーメッセージ表示
    errorMessage = result.ErrorValue;
    await ShowToast("danger", errorMessage);
}
```

### Railway-oriented Programmingの利点

1. **エラー伝播の明示性**: 例外ではなくResult型でエラー管理
2. **型安全なエラーハンドリング**: コンパイル時にエラー処理を強制
3. **合成可能**: 複数のResult型を合成して処理チェーン構築

## よくあるエラーと解決方法

### エラー1: IsOkが見つからない

```
Error CS1061: 'FSharpResult<ProjectDto, string>' does not contain a definition for 'IsOk'
```

**原因**:
- F# Result型の完全修飾名が必要
- using文が不足

**解決**:
```csharp
using Microsoft.FSharp.Core;

// または完全修飾名使用
if (result.IsOk) { ... }
```

### エラー2: ResultValueアクセスエラー

```
Error CS1061: 'FSharpResult<T, E>' does not contain a definition for 'ResultValue'
```

**原因**: IsOkチェック前にResultValueアクセス

**解決**:
```csharp
// ❌ 誤り
var value = result.ResultValue; // IsOkチェックなし

// ✅ 正しい
if (result.IsOk)
{
    var value = result.ResultValue;
}
```

## Phase B1での実証データ

- **適用箇所**: Blazor Server 3コンポーネント・Contracts層7ファイル
- **エラー修正**: 12件のIsOk/ResultValueアクセスエラー完全解決
- **成功率**: 100%（0 Warning/0 Error達成）
- **品質評価**: 仕様準拠度98/100点

## 参考情報

- **F# Result型定義**: `Microsoft.FSharp.Core.FSharpResult<'T, 'TError>`
- **Phase B1実装記録**: `Doc/08_Organization/Completed/Phase_B1/Step07_完了報告.md`
- **contracts-bridge Agent**: `.claude/agents/contracts-bridge.md`
