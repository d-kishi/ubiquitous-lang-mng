# Step07 bUnit技術調査結果 (Part 1/3)

## 📋 調査概要
- **調査日**: 2025-10-05
- **調査目的**: Phase B1 Step7 Stage4（テストアーキテクチャ移行・bUnit UIテスト実装）準備
- **調査対象**: Blazor Server UIコンポーネントのbUnitテスト実装技術

---

## 1. bUnit基本情報

### 1.1 最新バージョン情報
- **最新版**: bUnit 1.40.0（2025年6月14日リリース）
- **.NET 8.0対応**: 完全サポート（.NET 5.0-9.0対応）
- **Blazor Server対応**: Blazor Server/WebAssembly両方サポート

### 1.2 NuGetパッケージ構成

**推奨インストールパッケージ**:
```bash
dotnet add package bunit --version 1.40.0
```

**パッケージ構成**:
- `bunit`: メインパッケージ（bunit.core + bunit.web統合）
- `bunit.core`: コア機能（TestContext等）
- `bunit.web`: Web UI固有機能（AuthorizeView等）

### 1.3 テストプロジェクト要件

**.csprojファイル設定**:
```xml
<Project Sdk="Microsoft.NET.Sdk.Razor">  <!-- 重要: Microsoft.NET.Sdk.Razor を使用 -->
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
  </PropertyGroup>
  
  <ItemGroup>
    <PackageReference Include="bunit" Version="1.40.0" />
    <PackageReference Include="xunit" Version="2.9.0" />
    <PackageReference Include="xunit.runner.visualstudio" Version="2.8.2" />
    <PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.11.0" />
    <PackageReference Include="Moq" Version="4.20.72" />
    <PackageReference Include="FluentAssertions" Version="6.12.1" />
  </ItemGroup>
  
  <ItemGroup>
    <ProjectReference Include="..\..\src\UbiquitousLanguageManager.Web\UbiquitousLanguageManager.Web.csproj" />
  </ItemGroup>
</Project>
```

**_Imports.razorファイル**:
```csharp
@using Bunit
@using Bunit.TestDoubles
@using Xunit
@using FluentAssertions
@using Moq
@using Microsoft.Extensions.DependencyInjection
@using Microsoft.AspNetCore.Components
@using Microsoft.AspNetCore.Components.Authorization
```

---

## 2. F#型システムとbUnit統合パターン

### 2.1 F# Result型のC#からの生成

**基本的な生成方法**:
```csharp
using Microsoft.FSharp.Core;

// 成功Result (Ok) の生成
var successResult = FSharpResult<ProjectListResultDto, string>.NewOk(new ProjectListResultDto
{
    IsSuccess = true,
    Projects = new List<ProjectDto>(),
    TotalCount = 0,
    PageNumber = 1,
    PageSize = 20
});

// 失敗Result (Error) の生成
var errorResult = FSharpResult<ProjectListResultDto, string>.NewError("プロジェクト取得に失敗しました");

// 別の生成方法
var result1 = FSharpResult.NewOk<ProjectListResultDto, string>(data);
var result2 = FSharpResult.NewError<ProjectListResultDto, string>("error");
```

**Resultのパターンマッチング**:
```csharp
public void ProcessResult(FSharpResult<ProjectListResultDto, string> result)
{
    if (result.IsOk)
    {
        var data = result.ResultValue;
        // 成功時の処理
    }
    else
    {
        var error = result.ErrorValue;
        // エラー時の処理
    }
}

// Switch式を使ったパターン
var message = result switch
{
    FSharpResult<ProjectListResultDto, string>.Ok ok => $"成功: {ok.Item.Projects.Count}件",
    FSharpResult<ProjectListResultDto, string>.Error err => $"エラー: {err.Item}",
    _ => "不明な結果"
};
```

### 2.2 F# Option型のC#からの生成

**基本的な生成方法**:
```csharp
using Microsoft.FSharp.Core;

// Some値の生成
var someValue = FSharpOption<string>.Some("プロジェクト説明");
var someValue2 = new FSharpOption<string>("プロジェクト説明");

// None値の生成
var noneValue = FSharpOption<string>.None;

// Option値のチェック
if (FSharpOption<string>.get_IsSome(optionValue))
{
    var value = optionValue.Value;
    // 値が存在する場合の処理
}

if (FSharpOption<string>.get_IsNone(optionValue))
{
    // 値が存在しない場合の処理
}
```

**拡張メソッドパターン（推奨）**:
```csharp
public static class FSharpOptionExtensions
{
    public static bool IsSome<T>(this FSharpOption<T> option)
        => FSharpOption<T>.get_IsSome(option);
    
    public static bool IsNone<T>(this FSharpOption<T> option)
        => FSharpOption<T>.get_IsNone(option);
    
    public static T GetValueOrDefault<T>(this FSharpOption<T> option, T defaultValue = default)
        => option.IsSome() ? option.Value : defaultValue;
}

// 使用例
var description = descriptionOption.GetValueOrDefault("説明なし");
```
