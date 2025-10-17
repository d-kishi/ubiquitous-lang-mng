namespace UbiquitousLanguageManager.Application.ProjectManagement

open System.Threading.Tasks
// F# Domain層namespace階層化対応: Bounded Context別にopen
open UbiquitousLanguageManager.Domain.Common                  // UserId, ProjectId, Role
open UbiquitousLanguageManager.Domain.Authentication          // User
open UbiquitousLanguageManager.Domain.ProjectManagement       // Project, Domain
open UbiquitousLanguageManager.Domain.UbiquitousLanguageManagement  // (使用なし)

// 🎯 Phase B1 Step3: プロジェクト管理サービスインターフェース
// Command/Query分離・権限制御マトリックス・Railway-oriented Programming統合
// 【F#初学者向け解説】
// Clean Architecture Application層のインターフェース定義です。
// IProjectManagementServiceは、プロジェクト管理の全ユースケースを抽象化し、
// Infrastructure層への依存を排除した純粋なApplication層契約を提供します。

type IProjectManagementService =

    // 📁 Command側: 状態変更操作（CRUD操作）
    // REQ-3.1.2準拠: プロジェクト作成・デフォルトドメイン自動作成・原子性保証

    /// <summary>
    /// プロジェクト作成（デフォルトドメイン自動作成付き）
    /// 【F#初学者向け解説】
    /// ProjectDomainService.createProjectWithDefaultDomainを活用し、
    /// Railway-oriented Programmingパターンでエラーハンドリングを実装します。
    /// Task&lt;Result&lt;T, string&gt;&gt;により、非同期処理と型安全なエラー処理を両立します。
    /// </summary>
    /// <param name="command">プロジェクト作成Command</param>
    /// <returns>作成結果（プロジェクト+デフォルトドメイン）またはエラーメッセージ</returns>
    abstract member CreateProjectAsync: command: CreateProjectCommand -> Task<ProjectCommandResult>

    /// <summary>
    /// プロジェクト編集（説明のみ変更可能）
    /// REQ-3.1.3・PROHIBITION-3.3.1-1準拠: プロジェクト名変更完全禁止
    /// 【F#初学者向け解説】
    /// プロジェクトの説明のみを更新できます。プロジェクト名の変更は
    /// 否定的仕様により完全に禁止されています。権限チェックも統合されています。
    /// </summary>
    /// <param name="command">プロジェクト更新Command</param>
    /// <returns>更新されたプロジェクトまたはエラーメッセージ</returns>
    abstract member UpdateProjectAsync: command: UpdateProjectCommand -> Task<UpdateCommandResult>

    /// <summary>
    /// プロジェクト削除（論理削除）
    /// REQ-3.1.4・PROHIBITION-3.3.1-2準拠: 関連データ確認・スーパーユーザーのみ実行可能
    /// 【F#初学者向け解説】
    /// 物理削除ではなく論理削除（IsActive=false）を実行します。
    /// 関連するドメインやユビキタス言語が存在する場合は削除を拒否します。
    /// </summary>
    /// <param name="command">プロジェクト削除Command</param>
    /// <returns>削除成功時はunit、失敗時はエラーメッセージ</returns>
    abstract member DeleteProjectAsync: command: DeleteProjectCommand -> Task<DeleteCommandResult>

    /// <summary>
    /// プロジェクト所有者変更
    /// プロジェクトの所有権を別のユーザーに移譲します
    /// </summary>
    /// <param name="command">所有者変更Command</param>
    /// <returns>更新されたプロジェクトまたはエラーメッセージ</returns>
    abstract member ChangeProjectOwnerAsync: command: ChangeProjectOwnerCommand -> Task<OwnerChangeCommandResult>

    /// <summary>
    /// プロジェクト有効化（論理削除の取り消し）
    /// </summary>
    /// <param name="command">プロジェクト有効化Command</param>
    /// <returns>有効化されたプロジェクトまたはエラーメッセージ</returns>
    abstract member ActivateProjectAsync: command: ActivateProjectCommand -> Task<ActivateCommandResult>

    // 📋 Query側: データ取得操作（読み取り専用・権限制御統合）
    // REQ-3.1.1・REQ-10.2.1準拠: 権限別表示制御・ページング対応

    /// <summary>
    /// プロジェクト一覧取得（権限制御・ページング対応）
    /// 【F#初学者向け解説】
    /// REQ-10.2.1の権限制御マトリックスに基づき、ユーザーロールに応じて
    /// 表示可能なプロジェクトを絞り込んで返します。
    /// - スーパーユーザー: 全プロジェクト
    /// - プロジェクト管理者: 担当プロジェクトのみ
    /// - ドメイン承認者・一般ユーザー: 非表示
    /// </summary>
    /// <param name="query">プロジェクト一覧取得Query</param>
    /// <returns>権限制御後のプロジェクト一覧とページング情報</returns>
    abstract member GetProjectsAsync: query: GetProjectsQuery -> Task<ProjectListResult>

    /// <summary>
    /// プロジェクト詳細取得（権限チェック統合）
    /// </summary>
    /// <param name="query">プロジェクト詳細取得Query</param>
    /// <returns>プロジェクト詳細情報または権限エラー</returns>
    abstract member GetProjectDetailAsync: query: GetProjectDetailQuery -> Task<ProjectDetailResult>

    /// <summary>
    /// プロジェクト参加ユーザー一覧取得
    /// </summary>
    /// <param name="query">プロジェクトユーザー一覧取得Query</param>
    /// <returns>参加ユーザー一覧またはエラーメッセージ</returns>
    abstract member GetProjectUsersAsync: query: GetProjectUsersQuery -> Task<ProjectUsersResult>

    /// <summary>
    /// プロジェクトドメイン一覧取得
    /// </summary>
    /// <param name="query">プロジェクトドメイン一覧取得Query</param>
    /// <returns>ドメイン一覧またはエラーメッセージ</returns>
    abstract member GetProjectDomainsAsync: query: GetProjectDomainsQuery -> Task<ProjectDomainsResult>

    /// <summary>
    /// ユーザー別プロジェクト一覧取得
    /// </summary>
    /// <param name="query">ユーザー別プロジェクト一覧取得Query</param>
    /// <returns>ユーザーが参加するプロジェクト一覧</returns>
    abstract member GetUserProjectsAsync: query: GetUserProjectsQuery -> Task<UserProjectsResult>

    /// <summary>
    /// プロジェクト検索（高度な検索機能）
    /// </summary>
    /// <param name="query">プロジェクト検索Query</param>
    /// <returns>検索条件に一致するプロジェクト一覧</returns>
    abstract member SearchProjectsAsync: query: SearchProjectsQuery -> Task<ProjectSearchResult>

    /// <summary>
    /// プロジェクト統計情報取得
    /// ダッシュボード表示用の統計データを取得します
    /// 【F#初学者向け解説】
    /// DomainService.calculateProjectStatisticsを活用し、
    /// ビジネスインテリジェンス機能の一部として統計情報を提供します。
    /// </summary>
    /// <param name="query">プロジェクト統計情報取得Query</param>
    /// <returns>統計情報またはエラーメッセージ</returns>
    abstract member GetProjectStatisticsAsync: query: GetProjectStatisticsQuery -> Task<ProjectStatisticsResult>

    // 👥 Phase B2: UserProjects多対多関連管理メソッド

    /// <summary>
    /// プロジェクトメンバー追加
    /// 【Phase B2: ユーザー・プロジェクト関連管理】
    /// - UserProjectsレコードINSERT
    /// - 重複追加チェック（複合一意制約）
    /// - SuperUser/ProjectManager権限のみ実行可能
    /// 【F#初学者向け解説】
    /// Railway-oriented Programmingパターンで実装され、
    /// 権限チェック→重複チェック→永続化の各ステップをResult型で連鎖処理します。
    /// </summary>
    /// <param name="command">メンバー追加Command</param>
    /// <returns>成功時はunit、失敗時はエラーメッセージ</returns>
    abstract member AddMemberToProjectAsync: command: AddMemberToProjectCommand -> Task<AddMemberCommandResult>

    /// <summary>
    /// プロジェクトメンバー削除
    /// 【Phase B2: ユーザー・プロジェクト関連管理】
    /// - UserProjectsレコードDELETE（物理削除）
    /// - 最後の管理者削除防止チェック（AspNetUserRoles参照）
    /// - SuperUser/ProjectManager権限のみ実行可能
    /// 【F#初学者向け解説】
    /// 最後のProjectManagerを削除しようとした場合はエラーを返します。
    /// これにより、プロジェクトに必ず管理者が存在することを保証します。
    /// </summary>
    /// <param name="command">メンバー削除Command</param>
    /// <returns>成功時はunit、失敗時はエラーメッセージ</returns>
    abstract member RemoveMemberFromProjectAsync: command: RemoveMemberFromProjectCommand -> Task<RemoveMemberCommandResult>

    /// <summary>
    /// プロジェクトメンバー一覧取得
    /// 【Phase B2: ユーザー・プロジェクト関連管理】
    /// - UserProjectsテーブル経由でメンバー一覧取得
    /// - 権限制御マトリックス準拠（SuperUser/ProjectManager/所属メンバーのみ表示可能）
    /// 【F#初学者向け解説】
    /// Infrastructure層GetProjectMembersAsync（UserIdのみ取得）を活用し、
    /// UserIdリストを返却します。User詳細情報取得はWeb層で実施します。
    /// </summary>
    /// <param name="query">メンバー一覧取得Query</param>
    /// <returns>UserIdリストまたはエラーメッセージ</returns>
    abstract member GetProjectMembersAsync: query: GetProjectMembersQuery -> Task<ProjectMembersResult>

    /// <summary>
    /// プロジェクトメンバー判定
    /// 【Phase B2: ユーザー・プロジェクト関連管理】
    /// - UserProjectsテーブル存在チェック
    /// - 権限制御マトリックス統合用のヘルパーメソッド
    /// 【F#初学者向け解説】
    /// 指定されたユーザーが指定されたプロジェクトのメンバーかどうかを判定します。
    /// Infrastructure層IsUserProjectMemberAsync活用で効率的な存在チェックを実現します。
    /// </summary>
    /// <param name="userId">判定対象ユーザーID</param>
    /// <param name="projectId">判定対象プロジェクトID</param>
    /// <returns>メンバー判定結果（true/false）またはエラーメッセージ</returns>
    abstract member IsUserProjectMemberAsync: userId: UserId * projectId: ProjectId -> Task<Result<bool, string>>

// 🎯 プロジェクト管理Repository抽象化
// 【F#初学者向け解説】
// Infrastructure層への依存を抽象化するためのRepository契約です。
// ProjectManagementServiceは、このインターフェースを通じて永続化処理を行い、
// 具体的なデータベース技術（EF Core等）から独立性を保ちます。
type IProjectRepository =

    /// <summary>
    /// プロジェクト保存（作成・更新共通）
    /// </summary>
    /// <param name="project">保存するプロジェクト</param>
    /// <returns>保存されたプロジェクトまたはエラーメッセージ</returns>
    abstract member SaveAsync: project: Project -> Task<Result<Project, string>>

    /// <summary>
    /// プロジェクトとデフォルトドメインの原子性保証保存
    /// REQ-3.1.2-2準拠: 失敗時完全ロールバック
    /// </summary>
    /// <param name="project">保存するプロジェクト</param>
    /// <param name="defaultDomain">自動作成されたデフォルトドメイン</param>
    /// <returns>保存結果（両方）またはエラーメッセージ</returns>
    abstract member SaveProjectWithDefaultDomainAsync: project: Project * defaultDomain: Domain -> Task<Result<Project * Domain, string>>

    /// <summary>
    /// ID によるプロジェクト取得
    /// </summary>
    /// <param name="id">プロジェクトID</param>
    /// <returns>見つかったプロジェクト（option型）またはエラーメッセージ</returns>
    abstract member GetByIdAsync: id: ProjectId -> Task<Result<Project option, string>>

    /// <summary>
    /// 所有者別プロジェクト一覧取得（重複チェック用）
    /// </summary>
    /// <param name="ownerId">所有者ID</param>
    /// <returns>プロジェクト一覧またはエラーメッセージ</returns>
    abstract member GetByOwnerAsync: ownerId: UserId -> Task<Result<Project list, string>>

    /// <summary>
    /// 権限制御済みプロジェクト一覧取得（ページング対応）
    /// </summary>
    /// <param name="userId">要求ユーザーID</param>
    /// <param name="userRole">ユーザーロール</param>
    /// <param name="pageNumber">ページ番号</param>
    /// <param name="pageSize">ページサイズ</param>
    /// <param name="includeInactive">非アクティブプロジェクト含有フラグ</param>
    /// <returns>権限制御後のプロジェクト一覧</returns>
    abstract member GetProjectsWithPermissionAsync:
        userId: UserId * userRole: Role * pageNumber: int * pageSize: int * includeInactive: bool ->
        Task<Result<ProjectListResultDto, string>>

    /// <summary>
    /// 全有効プロジェクト取得（スーパーユーザー用）
    /// </summary>
    /// <returns>全ての有効なプロジェクトまたはエラーメッセージ</returns>
    abstract member GetAllActiveAsync: unit -> Task<Result<Project list, string>>

    /// <summary>
    /// 関連データ数取得（削除前確認用）
    /// PROHIBITION-3.3.1-2準拠: 参照中プロジェクト削除禁止
    /// </summary>
    /// <param name="projectId">プロジェクトID</param>
    /// <returns>関連ドメイン数・ユビキタス言語数の合計</returns>
    abstract member GetRelatedDataCountAsync: projectId: ProjectId -> Task<Result<int, string>>

    /// <summary>
    /// プロジェクト検索（高度な検索条件対応）
    /// </summary>
    /// <param name="searchQuery">検索条件</param>
    /// <returns>検索結果一覧</returns>
    abstract member SearchProjectsAsync: searchQuery: SearchProjectsQuery -> Task<Result<ProjectListResultDto, string>>

    // 👥 Phase B2: UserProjects多対多関連管理Repository拡張

    /// <summary>
    /// UserProjectsレコード追加（プロジェクトメンバー追加）
    /// </summary>
    /// <param name="userId">追加するユーザーID</param>
    /// <param name="projectId">対象プロジェクトID</param>
    /// <param name="updatedBy">更新者ID</param>
    /// <returns>成功時はunit、失敗時はエラーメッセージ</returns>
    abstract member AddUserToProjectAsync: userId: UserId * projectId: ProjectId * updatedBy: UserId -> Task<Result<unit, string>>

    /// <summary>
    /// UserProjectsレコード削除（プロジェクトメンバー削除）
    /// </summary>
    /// <param name="userId">削除するユーザーID</param>
    /// <param name="projectId">対象プロジェクトID</param>
    /// <returns>成功時はunit、失敗時はエラーメッセージ</returns>
    abstract member RemoveUserFromProjectAsync: userId: UserId * projectId: ProjectId -> Task<Result<unit, string>>

    /// <summary>
    /// プロジェクトメンバー一覧取得（UserProjects JOIN）
    /// </summary>
    /// <param name="projectId">対象プロジェクトID</param>
    /// <returns>UserIdリストまたはエラーメッセージ</returns>
    abstract member GetProjectMembersAsync: projectId: ProjectId -> Task<Result<UserId list, string>>

    /// <summary>
    /// プロジェクトメンバー判定（UserProjects存在チェック）
    /// </summary>
    /// <param name="userId">判定対象ユーザーID</param>
    /// <param name="projectId">判定対象プロジェクトID</param>
    /// <returns>メンバー判定結果（true/false）またはエラーメッセージ</returns>
    abstract member IsUserProjectMemberAsync: userId: UserId * projectId: ProjectId -> Task<Result<bool, string>>

    /// <summary>
    /// プロジェクトメンバー数取得（UserProjects COUNT）
    /// </summary>
    /// <param name="projectId">対象プロジェクトID</param>
    /// <returns>メンバー数またはエラーメッセージ</returns>
    abstract member GetProjectMemberCountAsync: projectId: ProjectId -> Task<Result<int, string>>

    /// <summary>
    /// プロジェクト作成 + デフォルトドメイン作成 + Owner自動追加（トランザクション保証）
    /// 【Phase B2: Phase B1トランザクションパターン拡張】
    /// </summary>
    /// <param name="project">保存するプロジェクト</param>
    /// <param name="defaultDomain">自動作成されたデフォルトドメイン</param>
    /// <param name="ownerId">Owner ID（UserProjects追加用）</param>
    /// <returns>保存結果（両方）またはエラーメッセージ</returns>
    abstract member SaveProjectWithDefaultDomainAndOwnerAsync: project: Project * defaultDomain: Domain * ownerId: UserId -> Task<Result<Project * Domain, string>>

// 🏷️ ドメインRepository抽象化
// デフォルトドメイン自動作成で必要
type IDomainRepository =

    /// <summary>
    /// ドメイン保存
    /// </summary>
    /// <param name="domain">保存するドメイン</param>
    /// <returns>保存されたドメインまたはエラーメッセージ</returns>
    abstract member SaveAsync: domain: Domain -> Task<Result<Domain, string>>

    /// <summary>
    /// プロジェクト別ドメイン一覧取得
    /// </summary>
    /// <param name="projectId">プロジェクトID</param>
    /// <returns>ドメイン一覧またはエラーメッセージ</returns>
    abstract member GetByProjectIdAsync: projectId: ProjectId -> Task<Result<Domain list, string>>

// 👥 ユーザーRepository抽象化（権限チェック用）
type IUserRepository =

    /// <summary>
    /// ID によるユーザー取得
    /// </summary>
    /// <param name="id">ユーザーID</param>
    /// <returns>見つかったユーザー（option型）またはエラーメッセージ</returns>
    abstract member GetByIdAsync: id: UserId -> Task<Result<User option, string>>