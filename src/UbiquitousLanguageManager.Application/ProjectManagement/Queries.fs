namespace UbiquitousLanguageManager.Application.ProjectManagement

open System
// F# Domain層namespace階層化対応: Bounded Context別にopen
open UbiquitousLanguageManager.Domain.Common                  // UserId, ProjectId, Role
open UbiquitousLanguageManager.Domain.Authentication          // User
open UbiquitousLanguageManager.Domain.ProjectManagement       // Project, Domain
open UbiquitousLanguageManager.Domain.UbiquitousLanguageManagement  // (使用なし)

// 🎯 Phase B1 Step3: プロジェクト管理Query定義
// Command/Query分離実装 - クエリ側（データ取得操作）
// 【F#初学者向け解説】
// CQRSパターンのQuery側実装です。データの読み取り専用操作を明確に分離し、
// 権限制御マトリックス（REQ-10.2.1）に基づく表示制御を実現します。
// 4ロール×4機能の権限マトリックスを Query レベルで適用します。

// 📋 プロジェクト一覧取得Query
// REQ-3.1.1準拠: 権限別表示制御・ページング対応
type GetProjectsQuery = {
    UserId: Guid                   // 要求ユーザーID
    UserRole: Role               // ユーザーロール（権限制御用）
    PageNumber: int               // ページ番号（1から開始）
    PageSize: int                 // 1ページあたりの項目数
    IncludeInactive: bool         // 非アクティブプロジェクト含有フラグ
    SearchKeyword: string option  // 検索キーワード（プロジェクト名・説明）
} with
    // 🔧 Domain型変換とバリデーション
    member this.toDomainTypes() : Result<UserId * Role, string> =
        if this.PageNumber < 1 then
            Error "ページ番号は1以上である必要があります"
        elif this.PageSize < 1 || this.PageSize > 100 then
            Error "ページサイズは1-100の範囲で指定してください"
        else
            let userId = UserId(int64(this.UserId.GetHashCode()))
            Ok (userId, this.UserRole)

// 🔍 プロジェクト詳細取得Query
// REQ-10.2.1準拠: 権限チェック統合
type GetProjectDetailQuery = {
    ProjectId: Guid               // 取得対象プロジェクトID
    UserId: Guid                 // 要求ユーザーID
    UserRole: Role               // ユーザーロール（権限制御用）
} with
    member this.toDomainTypes() : ProjectId * UserId * Role =
        (ProjectId(int64(this.ProjectId.GetHashCode())), UserId(int64(this.UserId.GetHashCode())), this.UserRole)

// 👥 プロジェクトユーザー一覧取得Query
// プロジェクトに参加しているユーザーの一覧を取得
type GetProjectUsersQuery = {
    ProjectId: Guid               // 対象プロジェクトID
    UserId: Guid                 // 要求ユーザーID
    UserRole: Role               // ユーザーロール（権限制御用）
} with
    member this.toDomainTypes() : ProjectId * UserId * Role =
        (ProjectId(int64(this.ProjectId.GetHashCode())), UserId(int64(this.UserId.GetHashCode())), this.UserRole)

// 👥 Phase B2: プロジェクトメンバー一覧取得Query
// 【Phase B2: ユーザー・プロジェクト関連管理】
// - UserProjectsテーブル経由でメンバー一覧取得
// - 権限制御マトリックス準拠（SuperUser/ProjectManager/所属メンバーのみ表示可能）
type GetProjectMembersQuery = {
    ProjectId: Guid               // 対象プロジェクトID
    UserId: Guid                 // 要求ユーザーID
    UserRole: Role               // ユーザーロール（権限制御用）
} with
    member this.toDomainTypes() : ProjectId * UserId * Role =
        (ProjectId(int64(this.ProjectId.GetHashCode())), UserId(int64(this.UserId.GetHashCode())), this.UserRole)

// 🏷️ プロジェクトドメイン一覧取得Query
// プロジェクト内のドメイン一覧を取得
type GetProjectDomainsQuery = {
    ProjectId: Guid               // 対象プロジェクトID
    UserId: Guid                 // 要求ユーザーID
    UserRole: Role               // ユーザーロール（権限制御用）
    IncludeInactive: bool        // 非アクティブドメイン含有フラグ
} with
    member this.toDomainTypes() : ProjectId * UserId * Role =
        (ProjectId(int64(this.ProjectId.GetHashCode())), UserId(int64(this.UserId.GetHashCode())), this.UserRole)

// 📊 ユーザー別プロジェクト一覧取得Query
// 特定ユーザーが参加しているプロジェクトの一覧を取得
type GetUserProjectsQuery = {
    TargetUserId: Guid           // 対象ユーザーID
    RequestUserId: Guid          // 要求ユーザーID（権限チェック用）
    UserRole: Role               // 要求ユーザーのロール
    IncludeInactive: bool        // 非アクティブプロジェクト含有フラグ
} with
    member this.toDomainTypes() : Result<UserId * UserId * Role, string> =
        let targetUserId = UserId(int64(this.TargetUserId.GetHashCode()))
        let requestUserId = UserId(int64(this.RequestUserId.GetHashCode()))
        Ok (targetUserId, requestUserId, this.UserRole)

// 🎯 プロジェクト検索Query
// 高度な検索機能対応
type SearchProjectsQuery = {
    UserId: Guid                 // 検索ユーザーID
    UserRole: Role               // ユーザーロール（権限制御用）
    SearchKeyword: string option // 検索キーワード
    OwnerId: Guid option         // 所有者ID絞り込み
    CreatedDateFrom: DateTime option  // 作成日FROM
    CreatedDateTo: DateTime option    // 作成日TO
    IsActive: bool option        // アクティブ状態絞り込み
    PageNumber: int              // ページ番号
    PageSize: int                // ページサイズ
} with
    member this.toDomainTypes() : Result<UserId * Role * UserId option, string> =
        if this.PageNumber < 1 then
            Error "ページ番号は1以上である必要があります"
        elif this.PageSize < 1 || this.PageSize > 100 then
            Error "ページサイズは1-100の範囲で指定してください"
        else
            let userId = UserId(int64(this.UserId.GetHashCode()))
            let ownerIdOpt = this.OwnerId |> Option.map (fun guid -> UserId(int64(guid.GetHashCode())))
            Ok (userId, this.UserRole, ownerIdOpt)

// 📊 プロジェクト統計情報Query
// ダッシュボード表示用の統計データ
type GetProjectStatisticsQuery = {
    UserId: Guid                 // 要求ユーザーID
    UserRole: Role               // ユーザーロール（権限制御用）
    ProjectId: Guid option       // 特定プロジェクトの統計（Noneの場合は全体統計）
} with
    member this.toDomainTypes() : UserId * Role * ProjectId option =
        let userId = UserId(int64(this.UserId.GetHashCode()))
        let projectIdOpt = this.ProjectId |> Option.map (fun guid -> ProjectId(int64(guid.GetHashCode())))
        (userId, this.UserRole, projectIdOpt)

// 🎯 プロジェクト一覧結果DTO
// 【F#初学者向け解説】
// Query結果を格納するためのData Transfer Object（DTO）型です。
// ページング情報と権限制御後のプロジェクト一覧を含みます。
type ProjectListResultDto = {
    Projects: Project list        // 権限制御後のプロジェクト一覧
    TotalCount: int              // 全体件数
    PageNumber: int              // 現在ページ番号
    PageSize: int                // ページサイズ
    HasNextPage: bool            // 次ページ存在フラグ
    HasPreviousPage: bool        // 前ページ存在フラグ
}

// 🔍 プロジェクト詳細結果DTO
type ProjectDetailResultDto = {
    Project: Project             // プロジェクト詳細情報
    UserCount: int               // 参加ユーザー数
    DomainCount: int             // ドメイン数
    UbiquitousLanguageCount: int // ユビキタス言語数
    CanEdit: bool                // 編集権限フラグ
    CanDelete: bool              // 削除権限フラグ
}

// 📊 プロジェクト統計結果DTO
type ProjectStatisticsResultDto = {
    TotalProjects: int           // 総プロジェクト数
    ActiveProjects: int          // アクティブプロジェクト数
    InactiveProjects: int        // 非アクティブプロジェクト数
    ProjectsWithDomains: int     // ドメインを持つプロジェクト数
    AverageDomainsPerProject: float  // プロジェクトあたり平均ドメイン数
    TotalUbiquitousLanguages: int    // 総ユビキタス言語数
    ProjectsByOwner: (string * int) list  // 所有者別プロジェクト数
}

// 🔄 クエリ実行結果: Railway-oriented Programming統合
// 【F#初学者向け解説】
// Query実行の結果を型安全に表現します。Result型により、
// 成功時の結果データと失敗時のエラーメッセージを明確に分離します。
type QueryResult<'TResult> = Result<'TResult, string>

// 🎯 プロジェクト管理Query結果型定義
type ProjectListResult = QueryResult<ProjectListResultDto>
type ProjectDetailResult = QueryResult<ProjectDetailResultDto>
type ProjectUsersResult = QueryResult<User list>
type ProjectDomainsResult = QueryResult<Domain list>
type UserProjectsResult = QueryResult<Project list>
type ProjectSearchResult = QueryResult<ProjectListResultDto>
type ProjectStatisticsResult = QueryResult<ProjectStatisticsResultDto>

// 👥 Phase B2: プロジェクトメンバー一覧結果型
type ProjectMembersResult = QueryResult<UserId list>

// 🔐 権限制御ヘルパーモジュール
// 【F#初学者向け解説】
// REQ-10.2.1の権限制御マトリックスを実装するヘルパーモジュールです。
// 4ロール×4機能の権限マトリックス判定をQuery側で活用します。
module ProjectQueryPermissions =

    // 📋 プロジェクト一覧表示権限チェック
    // 【Phase B2拡張】DomainApprover/GeneralUserも所属プロジェクト表示可能
    let canViewProjectList (userRole: Role) : bool =
        match userRole with
        | SuperUser | ProjectManager | DomainApprover | GeneralUser -> true  // 全ロールで表示可能（範囲は異なる）

    // 🔍 プロジェクト詳細表示権限チェック
    // 【Phase B2拡張】DomainApprover/GeneralUserも所属プロジェクト詳細表示可能
    // isMember: UserProjectsテーブルでメンバー判定済みフラグ
    let canViewProjectDetail (userRole: Role) (userId: UserId) (project: Project) (isMember: bool) : bool =
        match userRole with
        | SuperUser -> true  // スーパーユーザーは全プロジェクト表示可能
        | ProjectManager ->
            // プロジェクト管理者は担当プロジェクトのみ表示可能
            // 実際の実装では UserProjects テーブルをチェック
            project.OwnerId = userId  // 簡略化: 所有者のみチェック
        | DomainApprover | GeneralUser -> isMember  // 所属プロジェクトのみ表示可能

    // 📝 プロジェクト編集権限チェック
    let canEditProject (userRole: Role) (userId: UserId) (project: Project) : bool =
        match userRole with
        | SuperUser -> true  // 全プロジェクト編集可能
        | ProjectManager -> project.OwnerId = userId  // 担当プロジェクトのみ編集可能
        | DomainApprover | GeneralUser -> false  // 編集権限なし

    // 🗑️ プロジェクト削除権限チェック
    let canDeleteProject (userRole: Role) : bool =
        match userRole with
        | SuperUser -> true  // スーパーユーザーのみ削除可能
        | ProjectManager | DomainApprover | GeneralUser -> false  // 削除権限なし

    // 👥 Phase B2: プロジェクトメンバー管理権限チェック

    // メンバー追加権限チェック（基本権限のみ）
    // 【Phase B2: 権限制御マトリックス拡張】
    // - SuperUser: 全プロジェクトに対してメンバー追加可能
    // - ProjectManager: 担当プロジェクトのみメンバー追加可能（UserProjects判定は呼び出し側で実施）
    // - DomainApprover/GeneralUser: メンバー追加権限なし
    // 【F#初学者向け解説】
    // この関数はロールベースの基本権限チェックのみを行います。
    // ProjectManagerの場合、実際のメンバー判定はAddMemberToProjectAsyncで
    // IsUserProjectMemberAsyncを呼び出して行います。
    let canAddMember (userRole: Role) (userId: UserId) (project: Project) : bool =
        match userRole with
        | SuperUser -> true  // 全プロジェクトに追加可能
        | ProjectManager -> true  // 基本権限OK（メンバー判定は呼び出し側で実施）
        | DomainApprover | GeneralUser -> false  // 追加権限なし

    // メンバー削除権限チェック（基本権限のみ）
    // 【Phase B2: 権限制御マトリックス拡張】
    // - SuperUser: 全プロジェクトからメンバー削除可能
    // - ProjectManager: 担当プロジェクトのみメンバー削除可能（UserProjects判定は呼び出し側で実施）
    // - DomainApprover/GeneralUser: メンバー削除権限なし
    let canRemoveMember (userRole: Role) (userId: UserId) (project: Project) : bool =
        match userRole with
        | SuperUser -> true  // 全プロジェクトから削除可能
        | ProjectManager -> true  // 基本権限OK（メンバー判定は呼び出し側で実施）
        | DomainApprover | GeneralUser -> false  // 削除権限なし

    // プロジェクトメンバー一覧表示権限チェック
    // 【Phase B2: 権限制御マトリックス拡張】
    // - SuperUser/ProjectManager: 全プロジェクトのメンバー一覧表示可能
    // - DomainApprover/GeneralUser: 所属プロジェクトのみメンバー一覧表示可能
    let canViewProjectMembers (userRole: Role) (userId: UserId) (project: Project) (isMember: bool) : bool =
        match userRole with
        | SuperUser -> true  // 全プロジェクトのメンバー表示可能
        | ProjectManager -> project.OwnerId = userId  // 担当プロジェクトのみ表示可能
        | DomainApprover | GeneralUser -> isMember  // 所属プロジェクトのみ表示可能