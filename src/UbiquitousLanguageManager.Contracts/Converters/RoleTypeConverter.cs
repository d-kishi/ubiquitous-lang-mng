using UbiquitousLanguageManager.Contracts.Enums;
using UbiquitousLanguageManager.Domain.Common;

namespace UbiquitousLanguageManager.Contracts.Converters;

/// <summary>
/// F# Domain.Role ↔ C# RoleType 変換ユーティリティ
/// 【F#↔C#境界パターン】Discriminated Union変換
/// </summary>
/// <remarks>
/// 【F#初学者向け解説】
/// F#の判別共用体（Discriminated Union）は、C#のenumよりも強力な型システムです。
/// 各ケースが型として扱われ、コンパイル時に網羅性チェックが行われます。
///
/// 【C#からのアクセス方法】
/// - 各ケースの判定: `role.IsSuperUser`, `role.IsProjectManager` 等のプロパティ
/// - ケースの作成: `Role.SuperUser`, `Role.ProjectManager` 等の静的プロパティ
///
/// 【変換パターンの重要性】
/// - Clean Architecture遵守: Web層がDomain層を直接参照しない
/// - 型安全性保証: 不正なロール値の排除（コンパイル時エラー）
/// - 双方向変換: F# ⇄ C# の完全な相互運用性
/// </remarks>
public static class RoleTypeConverter
{
    /// <summary>
    /// F# Domain.Role → C# RoleType 変換
    /// </summary>
    /// <param name="role">F#のRole判別共用体</param>
    /// <returns>C#のRoleType enum</returns>
    /// <remarks>
    /// 【F#判別共用体のパターンマッチング】
    /// C#のswitch式では判別共用体を直接扱えないため、
    /// IsXxxプロパティを使用してケース判定を行います。
    ///
    /// F#側では以下のように定義されています：
    /// <code>
    /// type Role =
    ///     | SuperUser
    ///     | ProjectManager
    ///     | DomainApprover
    ///     | GeneralUser
    /// </code>
    ///
    /// C#からは各ケースに対して IsXxx プロパティが自動生成されます。
    /// </remarks>
    public static RoleType ToRoleType(Role role)
    {
        // F#の判別共用体は、C#からはプロパティとして各ケースにアクセス可能
        // 【重要】if文の順序は、より具体的な権限から判定することで、意図しない変換を防止
        if (role.IsSuperUser) return RoleType.SuperUser;
        if (role.IsProjectManager) return RoleType.ProjectManager;
        if (role.IsDomainApprover) return RoleType.DomainApprover;
        if (role.IsGeneralUser) return RoleType.GeneralUser;

        // F#の判別共用体は網羅的であるため、通常このケースは発生しない
        // 防御的プログラミングとして、デフォルトケースを含める
        return RoleType.GeneralUser;
    }

    /// <summary>
    /// C# RoleType → F# Domain.Role 変換
    /// </summary>
    /// <param name="roleType">C#のRoleType enum</param>
    /// <returns>F#のRole判別共用体</returns>
    /// <remarks>
    /// 【F#判別共用体の生成】
    /// F#の判別共用体は、C#からは静的プロパティとして各ケースにアクセス可能。
    /// 例: Role.SuperUser, Role.ProjectManager 等
    ///
    /// 【型安全性】
    /// C# enumのデフォルト値（未定義値）に対しても、
    /// 安全にGeneralUserにフォールバックすることで、例外発生を防止。
    /// </remarks>
    public static Role ToRole(RoleType roleType)
    {
        // C# enumからF#判別共用体への変換
        // F#の判別共用体は、C#からはNewXxxという静的プロパティでケースを取得
        return roleType switch
        {
            RoleType.SuperUser => Role.SuperUser,
            RoleType.ProjectManager => Role.ProjectManager,
            RoleType.DomainApprover => Role.DomainApprover,
            RoleType.GeneralUser => Role.GeneralUser,
            // 防御的プログラミング: 未定義値はGeneralUserにフォールバック
            _ => Role.GeneralUser
        };
    }

    /// <summary>
    /// 文字列 → C# RoleType 変換
    /// </summary>
    /// <param name="roleString">ロールの文字列表現（"SuperUser", "ProjectManager" 等）</param>
    /// <returns>C#のRoleType enum</returns>
    /// <remarks>
    /// 【使用場面】
    /// - JSONデシリアライズ時の型変換
    /// - 設定ファイル読み込み時のロール解析
    /// - Web APIパラメータからの変換
    ///
    /// 【エラーハンドリング】
    /// 不正な文字列値はGeneralUserにフォールバック（例外を投げない）
    /// より厳密なエラーハンドリングが必要な場合は、Result型版を使用すること。
    /// </remarks>
    public static RoleType FromString(string roleString)
    {
        return roleString switch
        {
            "SuperUser" => RoleType.SuperUser,
            "ProjectManager" => RoleType.ProjectManager,
            "DomainApprover" => RoleType.DomainApprover,
            "GeneralUser" => RoleType.GeneralUser,
            // 大文字小文字を区別しない変換もサポート
            "superuser" => RoleType.SuperUser,
            "projectmanager" => RoleType.ProjectManager,
            "domainapprover" => RoleType.DomainApprover,
            "generaluser" => RoleType.GeneralUser,
            // 不正な値はGeneralUserにフォールバック
            _ => RoleType.GeneralUser
        };
    }

    /// <summary>
    /// RoleType → 表示文字列 変換
    /// </summary>
    /// <param name="roleType">C#のRoleType enum</param>
    /// <returns>日本語表示文字列</returns>
    /// <remarks>
    /// 【Blazor Server初学者向け解説】
    /// Blazorコンポーネント（.razor）でユーザーロールを表示する際に使用。
    ///
    /// 使用例:
    /// <code>
    /// @using UbiquitousLanguageManager.Contracts.TypeConverters
    ///
    /// &lt;p&gt;あなたのロール: @RoleTypeConverter.ToDisplayString(currentUserRole)&lt;/p&gt;
    /// </code>
    ///
    /// 【国際化対応】
    /// 将来的にリソースファイル（.resx）を使用した多言語対応に移行する場合は、
    /// このメソッドをリソースキーに変更することを推奨。
    /// </remarks>
    public static string ToDisplayString(RoleType roleType)
    {
        return roleType switch
        {
            RoleType.SuperUser => "スーパーユーザー",
            RoleType.ProjectManager => "プロジェクト管理者",
            RoleType.DomainApprover => "ドメイン承認者",
            RoleType.GeneralUser => "一般ユーザー",
            _ => "一般ユーザー"
        };
    }

    /// <summary>
    /// F# Domain.Role → 日本語表示文字列 直接変換
    /// </summary>
    /// <param name="role">F#のRole判別共用体</param>
    /// <returns>日本語表示文字列</returns>
    /// <remarks>
    /// 【利便性メソッド】
    /// F#のRole型から直接日本語文字列を取得する際に使用。
    /// 内部的にはToRoleType → ToDisplayStringの2段階変換を実施。
    /// </remarks>
    public static string ToDisplayString(Role role)
    {
        return ToDisplayString(ToRoleType(role));
    }
}
