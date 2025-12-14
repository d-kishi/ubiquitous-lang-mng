using Xunit;
using UbiquitousLanguageManager.Contracts.Converters;
using UbiquitousLanguageManager.Contracts.Enums;
using UbiquitousLanguageManager.Domain.Common;

namespace UbiquitousLanguageManager.Contracts.Unit.Tests.Converters;

/// <summary>
/// RoleTypeConverter単体テスト - F# Role ↔ C# RoleType 変換検証
///
/// 【テスト方針】
/// F#の判別共用体（Domain.Role）とC#のenum（RoleType）間の双方向変換を検証。
/// TDD（Red-Green-Refactor）サイクルに従い、全メソッドの正常系・異常系を網羅。
///
/// 【F#初学者向け解説】
/// F#の判別共用体は、C#からプロパティ（IsSuperUser, IsProjectManager等）でアクセス可能。
/// また、静的プロパティ（Role.SuperUser, Role.ProjectManager等）で各ケースを生成可能。
///
/// 【テスト対象メソッド】（5メソッド・計23件）
/// 1. ToRoleType(Role) - F# → C# 変換（4件）
/// 2. ToRole(RoleType) - C# → F# 変換（5件）
/// 3. FromString(string) - 文字列 → RoleType 変換（9件）
/// 4. ToDisplayString(RoleType) - RoleType → 日本語表示（4件）
/// 5. ToDisplayString(Role) - F# Role → 日本語表示（1件）
/// </summary>
public class RoleTypeConverterTests
{
    #region ToRoleType - F# Role → C# RoleType 変換（4件）

    [Fact]
    public void ToRoleType_SuperUser_ReturnsSuperUserRoleType()
    {
        // Arrange - F# Role.SuperUser準備
        // 【F#初学者向け】Role.SuperUserは静的プロパティでF#判別共用体のケースにアクセス
        var role = Role.SuperUser;

        // Act - F# → C# 変換実行
        var result = RoleTypeConverter.ToRoleType(role);

        // Assert - 変換結果検証
        Assert.Equal(RoleType.SuperUser, result); // SuperUser ロールは正しく RoleType.SuperUser に変換されるべき
    }

    [Fact]
    public void ToRoleType_ProjectManager_ReturnsProjectManagerRoleType()
    {
        // Arrange - F# Role.ProjectManager準備
        var role = Role.ProjectManager;

        // Act - F# → C# 変換実行
        var result = RoleTypeConverter.ToRoleType(role);

        // Assert - 変換結果検証
        Assert.Equal(RoleType.ProjectManager, result); // ProjectManager ロールは正しく RoleType.ProjectManager に変換されるべき
    }

    [Fact]
    public void ToRoleType_DomainApprover_ReturnsDomainApproverRoleType()
    {
        // Arrange - F# Role.DomainApprover準備
        var role = Role.DomainApprover;

        // Act - F# → C# 変換実行
        var result = RoleTypeConverter.ToRoleType(role);

        // Assert - 変換結果検証
        Assert.Equal(RoleType.DomainApprover, result); // DomainApprover ロールは正しく RoleType.DomainApprover に変換されるべき
    }

    [Fact]
    public void ToRoleType_GeneralUser_ReturnsGeneralUserRoleType()
    {
        // Arrange - F# Role.GeneralUser準備
        var role = Role.GeneralUser;

        // Act - F# → C# 変換実行
        var result = RoleTypeConverter.ToRoleType(role);

        // Assert - 変換結果検証
        Assert.Equal(RoleType.GeneralUser, result); // GeneralUser ロールは正しく RoleType.GeneralUser に変換されるべき
    }

    #endregion

    #region ToRole - C# RoleType → F# Role 変換（5件）

    [Fact]
    public void ToRole_SuperUser_ReturnsSuperUserRole()
    {
        // Arrange - C# RoleType.SuperUser準備
        var roleType = RoleType.SuperUser;

        // Act - C# → F# 変換実行
        var result = RoleTypeConverter.ToRole(roleType);

        // Assert - F# Role検証
        // 【F#初学者向け】F#の判別共用体は、C#からIsSuperUserプロパティで判定
        Assert.True(result.IsSuperUser); // RoleType.SuperUser は正しく F# Role.SuperUser に変換されるべき
    }

    [Fact]
    public void ToRole_ProjectManager_ReturnsProjectManagerRole()
    {
        // Arrange - C# RoleType.ProjectManager準備
        var roleType = RoleType.ProjectManager;

        // Act - C# → F# 変換実行
        var result = RoleTypeConverter.ToRole(roleType);

        // Assert - F# Role検証
        Assert.True(result.IsProjectManager); // RoleType.ProjectManager は正しく F# Role.ProjectManager に変換されるべき
    }

    [Fact]
    public void ToRole_DomainApprover_ReturnsDomainApproverRole()
    {
        // Arrange - C# RoleType.DomainApprover準備
        var roleType = RoleType.DomainApprover;

        // Act - C# → F# 変換実行
        var result = RoleTypeConverter.ToRole(roleType);

        // Assert - F# Role検証
        Assert.True(result.IsDomainApprover); // RoleType.DomainApprover は正しく F# Role.DomainApprover に変換されるべき
    }

    [Fact]
    public void ToRole_GeneralUser_ReturnsGeneralUserRole()
    {
        // Arrange - C# RoleType.GeneralUser準備
        var roleType = RoleType.GeneralUser;

        // Act - C# → F# 変換実行
        var result = RoleTypeConverter.ToRole(roleType);

        // Assert - F# Role検証
        Assert.True(result.IsGeneralUser); // RoleType.GeneralUser は正しく F# Role.GeneralUser に変換されるべき
    }

    [Fact]
    public void ToRole_UndefinedValue_FallbacksToGeneralUser()
    {
        // Arrange - 未定義のRoleType値（enum値を直接キャスト）
        // 【防御的プログラミング】enum型は整数値を直接代入可能なため、未定義値のテストが必要
        var undefinedRoleType = (RoleType)999;

        // Act - C# → F# 変換実行
        var result = RoleTypeConverter.ToRole(undefinedRoleType);

        // Assert - デフォルトフォールバック検証
        // 【重要】未定義値は安全にGeneralUserにフォールバックすることで、例外発生を防止
        Assert.True(result.IsGeneralUser); // 未定義の RoleType 値は安全に GeneralUser にフォールバックすべき
    }

    #endregion

    #region FromString - 文字列 → RoleType 変換（9件）

    [Fact]
    public void FromString_SuperUser_ReturnsSuperUser()
    {
        // Arrange - 正確な文字列表現
        var roleString = "SuperUser";

        // Act - 文字列 → RoleType 変換実行
        var result = RoleTypeConverter.FromString(roleString);

        // Assert - 変換結果検証
        Assert.Equal(RoleType.SuperUser, result); // "SuperUser" 文字列は正しく RoleType.SuperUser に変換されるべき
    }

    [Fact]
    public void FromString_ProjectManager_ReturnsProjectManager()
    {
        // Arrange - 正確な文字列表現
        var roleString = "ProjectManager";

        // Act - 文字列 → RoleType 変換実行
        var result = RoleTypeConverter.FromString(roleString);

        // Assert - 変換結果検証
        Assert.Equal(RoleType.ProjectManager, result); // "ProjectManager" 文字列は正しく RoleType.ProjectManager に変換されるべき
    }

    [Fact]
    public void FromString_DomainApprover_ReturnsDomainApprover()
    {
        // Arrange - 正確な文字列表現
        var roleString = "DomainApprover";

        // Act - 文字列 → RoleType 変換実行
        var result = RoleTypeConverter.FromString(roleString);

        // Assert - 変換結果検証
        Assert.Equal(RoleType.DomainApprover, result); // "DomainApprover" 文字列は正しく RoleType.DomainApprover に変換されるべき
    }

    [Fact]
    public void FromString_GeneralUser_ReturnsGeneralUser()
    {
        // Arrange - 正確な文字列表現
        var roleString = "GeneralUser";

        // Act - 文字列 → RoleType 変換実行
        var result = RoleTypeConverter.FromString(roleString);

        // Assert - 変換結果検証
        Assert.Equal(RoleType.GeneralUser, result); // "GeneralUser" 文字列は正しく RoleType.GeneralUser に変換されるべき
    }

    [Fact]
    public void FromString_LowerCaseSuperUser_ReturnsSuperUser()
    {
        // Arrange - 小文字の文字列表現
        // 【使用場面】JSONデシリアライズ時の大小文字無視
        var roleString = "superuser";

        // Act - 文字列 → RoleType 変換実行
        var result = RoleTypeConverter.FromString(roleString);

        // Assert - 大小文字無視変換検証
        Assert.Equal(RoleType.SuperUser, result); // "superuser"（小文字）は大小文字を無視して RoleType.SuperUser に変換されるべき
    }

    [Fact]
    public void FromString_LowerCaseProjectManager_ReturnsProjectManager()
    {
        // Arrange - 小文字の文字列表現
        var roleString = "projectmanager";

        // Act - 文字列 → RoleType 変換実行
        var result = RoleTypeConverter.FromString(roleString);

        // Assert - 大小文字無視変換検証
        Assert.Equal(RoleType.ProjectManager, result); // "projectmanager"（小文字）は大小文字を無視して RoleType.ProjectManager に変換されるべき
    }

    [Fact]
    public void FromString_LowerCaseDomainApprover_ReturnsDomainApprover()
    {
        // Arrange - 小文字の文字列表現
        var roleString = "domainapprover";

        // Act - 文字列 → RoleType 変換実行
        var result = RoleTypeConverter.FromString(roleString);

        // Assert - 大小文字無視変換検証
        Assert.Equal(RoleType.DomainApprover, result); // "domainapprover"（小文字）は大小文字を無視して RoleType.DomainApprover に変換されるべき
    }

    [Fact]
    public void FromString_LowerCaseGeneralUser_ReturnsGeneralUser()
    {
        // Arrange - 小文字の文字列表現
        var roleString = "generaluser";

        // Act - 文字列 → RoleType 変換実行
        var result = RoleTypeConverter.FromString(roleString);

        // Assert - 大小文字無視変換検証
        Assert.Equal(RoleType.GeneralUser, result); // "generaluser"（小文字）は大小文字を無視して RoleType.GeneralUser に変換されるべき
    }

    [Fact]
    public void FromString_InvalidValue_FallbacksToGeneralUser()
    {
        // Arrange - 不正な文字列値
        // 【防御的プログラミング】不正な値は例外を投げずにGeneralUserにフォールバック
        var invalidRoleString = "InvalidRole";

        // Act - 文字列 → RoleType 変換実行
        var result = RoleTypeConverter.FromString(invalidRoleString);

        // Assert - デフォルトフォールバック検証
        Assert.Equal(RoleType.GeneralUser, result); // 不正な文字列値は安全に GeneralUser にフォールバックすべき
    }

    #endregion

    #region ToDisplayString - RoleType → 日本語表示（5件）

    [Fact]
    public void ToDisplayString_SuperUser_ReturnsJapaneseDisplay()
    {
        // Arrange - RoleType.SuperUser準備
        var roleType = RoleType.SuperUser;

        // Act - 日本語表示文字列取得
        var result = RoleTypeConverter.ToDisplayString(roleType);

        // Assert - 日本語表示検証
        // 【Blazor Server初学者向け】Blazorコンポーネント（.razor）でユーザーロール表示に使用
        Assert.Equal("スーパーユーザー", result); // RoleType.SuperUser は "スーパーユーザー" と表示されるべき
    }

    [Fact]
    public void ToDisplayString_ProjectManager_ReturnsJapaneseDisplay()
    {
        // Arrange - RoleType.ProjectManager準備
        var roleType = RoleType.ProjectManager;

        // Act - 日本語表示文字列取得
        var result = RoleTypeConverter.ToDisplayString(roleType);

        // Assert - 日本語表示検証
        Assert.Equal("プロジェクト管理者", result); // RoleType.ProjectManager は "プロジェクト管理者" と表示されるべき
    }

    [Fact]
    public void ToDisplayString_DomainApprover_ReturnsJapaneseDisplay()
    {
        // Arrange - RoleType.DomainApprover準備
        var roleType = RoleType.DomainApprover;

        // Act - 日本語表示文字列取得
        var result = RoleTypeConverter.ToDisplayString(roleType);

        // Assert - 日本語表示検証
        Assert.Equal("ドメイン承認者", result); // RoleType.DomainApprover は "ドメイン承認者" と表示されるべき
    }

    [Fact]
    public void ToDisplayString_GeneralUser_ReturnsJapaneseDisplay()
    {
        // Arrange - RoleType.GeneralUser準備
        var roleType = RoleType.GeneralUser;

        // Act - 日本語表示文字列取得
        var result = RoleTypeConverter.ToDisplayString(roleType);

        // Assert - 日本語表示検証
        Assert.Equal("一般ユーザー", result); // RoleType.GeneralUser は "一般ユーザー" と表示されるべき
    }

    [Fact]
    public void ToDisplayString_FSharpRole_ReturnsJapaneseDisplay()
    {
        // Arrange - F# Role.ProjectManager準備
        // 【利便性メソッド】F# Role型から直接日本語表示を取得（ToRoleType → ToDisplayString の2段階変換）
        var role = Role.ProjectManager;

        // Act - F# Role → 日本語表示文字列（オーバーロード版）
        var result = RoleTypeConverter.ToDisplayString(role);

        // Assert - オーバーロードメソッド検証
        Assert.Equal("プロジェクト管理者", result); // F# Role.ProjectManager は "プロジェクト管理者" と表示されるべき
    }

    #endregion
}
