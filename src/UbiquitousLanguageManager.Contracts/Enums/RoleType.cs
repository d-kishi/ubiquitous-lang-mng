namespace UbiquitousLanguageManager.Contracts.Enums;

/// <summary>
/// ユーザーロールタイプ
/// F# Domain層のRole型（Discriminated Union）のC#側契約型
/// 【Clean Architecture】Web層がDomain層を直接参照しないための境界型
/// </summary>
/// <remarks>
/// 【F#↔C#境界パターン】
/// - F#のDiscriminated Union（判別共用体）は型安全な列挙型として機能
/// - C#からF#の判別共用体を直接参照するとClean Architectureの依存関係が破綻
/// - このenumを使用することで、Web層はDomain層への直接参照を回避
///
/// 【使用場面】
/// - Blazor ServerコンポーネントでのRole選択UI
/// - Web層からApplication層への型変換境界
/// - セッション管理・認可処理でのロール判定
/// </remarks>
public enum RoleType
{
    /// <summary>システム全体の管理者（全権限保持）</summary>
    SuperUser = 0,

    /// <summary>プロジェクト管理者（プロジェクト作成・管理権限）</summary>
    ProjectManager = 1,

    /// <summary>ドメイン承認者（ユビキタス言語承認権限）</summary>
    DomainApprover = 2,

    /// <summary>一般ユーザー（参照・編集権限のみ）</summary>
    GeneralUser = 3
}
