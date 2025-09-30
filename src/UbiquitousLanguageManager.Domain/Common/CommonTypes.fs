namespace UbiquitousLanguageManager.Domain

// 🎯 CommonTypes.fs: 共通型定義
// Bounded Context: Common
// このファイルは全Bounded Contextで共有される共通型を定義します
//
// 【F#初学者向け解説】
// 判別共用体（Discriminated Union）を使用してID型を型安全に表現します。
// これにより、int64型のIDを直接扱う場合に起こりうる誤代入を防ぎます。
// 例: UserId と ProjectId を誤って入れ替えることをコンパイル時に防止

// 🆔 識別子型: 型安全なIDの実装
// 【F#初学者向け解説】
// | (バーティカルライン) は判別共用体のケース定義です。
// UserId型は内部的にint64を保持しますが、外部から見ると独立した型として扱われます。
// これを「Wrapper型」と呼び、型の誤用を防ぐ重要なパターンです。

type UserId =
    | UserId of int64
with
    // Valueプロパティ: 内部のint64値を取得
    // 【F#初学者向け解説】
    // パターンマッチングで内部値を取り出します。
    // let (UserId id) = this は「分解（Deconstruction）」と呼ばれる操作です。
    member this.Value =
        let (UserId id) = this
        id

    // 静的ファクトリーメソッド: int64からUserIdを作成
    static member create(id: int64) = UserId id

type ProjectId =
    | ProjectId of int64
with
    member this.Value =
        let (ProjectId id) = this
        id
    static member create(id: int64) = ProjectId id

type DomainId =
    | DomainId of int64
with
    member this.Value =
        let (DomainId id) = this
        id
    static member create(id: int64) = DomainId id

type UbiquitousLanguageId =
    | UbiquitousLanguageId of int64
with
    member this.Value =
        let (UbiquitousLanguageId id) = this
        id
    static member create(id: int64) = UbiquitousLanguageId id

// 🎭 権限システム：階層的権限とプロジェクトスコープ権限
// 【F#初学者向け解説】
// Discriminated Unionを使用して権限を型安全に表現します。
// これにより、コンパイル時に権限チェックの漏れを防ぎ、
// パターンマッチングで全ての権限ケースを確実に処理できます。

// 個別権限定義（最小権限単位）
// 【F#初学者向け解説】
// | で区切られた各行が「ケース」です。C#のenumに似ていますが、
// F#の判別共用体はより強力で、各ケースに異なる型の値を持たせることができます。
// ここでは単純なケース（値を持たない）を列挙しています。
type Permission =
    | ViewUsers                       // ユーザー閲覧
    | CreateUsers                     // ユーザー作成
    | EditUsers                       // ユーザー編集
    | DeleteUsers                     // ユーザー削除（無効化）
    | ManageUserRoles                 // ユーザーロール管理
    | ViewProjects                    // プロジェクト閲覧
    | CreateProjects                  // プロジェクト作成
    | ManageProjects                  // プロジェクト管理
    | DeleteProjects                  // プロジェクト削除
    | ViewDomains                     // ドメイン閲覧
    | ManageDomains                   // ドメイン管理
    | ApproveDomains                  // ドメイン承認
    | ViewUbiquitousLanguages         // ユビキタス言語閲覧
    | CreateUbiquitousLanguages       // ユビキタス言語作成
    | EditUbiquitousLanguages         // ユビキタス言語編集
    | ApproveUbiquitousLanguages      // ユビキタス言語承認
    | ManageSystemSettings            // システム設定管理

// ロール定義（権限の集合）
// 【F#初学者向け解説】
// ロールは権限の集合として定義されます。
// 後述のPermissionMappingsモジュールで各ロールが持つ権限を定義します。
type Role =
    | SuperUser                       // システム管理者（全権限）
    | ProjectManager                  // プロジェクト管理者
    | DomainApprover                  // ドメイン承認者
    | GeneralUser                     // 一般ユーザー

// 階層的権限マッピング関数
// 【F#初学者向け解説】
// Set<Permission>を使用することで、権限の重複を自動的に排除し、
// 効率的な権限チェックを実現します。Set.unionで権限を合成し、
// 階層的な権限継承を表現しています。
//
// 「再帰関数」を使用してロール継承を実現：
// - ProjectManagerはDomainApproverの権限を継承
// - DomainApproverはGeneralUserの権限を継承
// - SuperUserはProjectManagerの権限を継承
module PermissionMappings =
    // rec キーワード: 再帰関数（自分自身を呼び出せる関数）を定義
    // 【F#初学者向け解説】
    // この関数は自分自身を呼び出して階層的な権限を取得します。
    // 例: ProjectManagerの権限を取得する際、DomainApproverの権限も再帰的に取得
    let rec getPermissionsForRole (role: Role) : Set<Permission> =
        match role with
        | GeneralUser ->
            // set [...] は Set<T> を作成するリテラル構文
            // 【F#初学者向け解説】
            // Setはリスト（list）と異なり、重複を許さない集合です。
            // 権限チェックが高速で、順序も気にしない場合に最適です。
            set [
                ViewProjects; ViewDomains; ViewUbiquitousLanguages
                CreateUbiquitousLanguages; EditUbiquitousLanguages
            ]
        | DomainApprover ->
            // 基底ロールの権限を取得（再帰呼び出し）
            let basePermissions = getPermissionsForRole GeneralUser
            // Set.union: 2つのSetを結合（重複は自動除去）
            Set.union basePermissions (set [
                ApproveDomains; ApproveUbiquitousLanguages
            ])
        | ProjectManager ->
            let basePermissions = getPermissionsForRole DomainApprover
            Set.union basePermissions (set [
                ViewUsers; CreateUsers; EditUsers; ManageUserRoles
                CreateProjects; ManageProjects; ManageDomains
            ])
        | SuperUser ->
            let basePermissions = getPermissionsForRole ProjectManager
            Set.union basePermissions (set [
                DeleteUsers; DeleteProjects; ManageSystemSettings
            ])

    // 特定権限の保有確認
    // 【F#初学者向け解説】
    // bool を返す関数です。コロン(:)の後がこの関数の戻り値の型です。
    // Set.contains は Set内に特定の要素が存在するか確認します。
    let hasPermission (role: Role) (permission: Permission) : bool =
        let permissions = getPermissionsForRole role
        Set.contains permission permissions

    // 複数権限の一括確認
    // 【F#初学者向け解説】
    // List.forallは「全ての要素が条件を満たすか」をチェックします。
    // fun p -> ... はラムダ式（無名関数）です。C#の p => ... に相当します。
    let hasAllPermissions (role: Role) (requiredPermissions: Permission list) : bool =
        let userPermissions = getPermissionsForRole role
        requiredPermissions |> List.forall (fun p -> Set.contains p userPermissions)

// プロジェクトスコープ権限
// 【F#初学者向け解説】
// ユーザーが特定のプロジェクトに対して持つ権限を表現します。
// レコード型 {...} を使用してプロジェクトIDと権限の組み合わせを定義します。
// withキーワード以降はメンバー定義（静的メソッド・インスタンスメソッド）です。
type ProjectPermission = {
    ProjectId: ProjectId
    Permissions: Set<Permission>
} with
    // 静的ファクトリーメソッド
    // 【F#初学者向け解説】
    // Listで受け取った権限をSet.ofListでSetに変換します。
    // これにより重複する権限が自動的に除去されます。
    static member create projectId permissions = {
        ProjectId = projectId
        Permissions = Set.ofList permissions
    }

    // インスタンスメソッド: 特定権限の保有確認
    // 【F#初学者向け解説】
    // member this.xxx は C# のインスタンスメソッドに相当します。
    // this は現在のインスタンスを指します。
    member this.hasPermission permission =
        Set.contains permission this.Permissions