namespace UbiquitousLanguageManager.Application.ProjectManagement

open System
// F# Domain層namespace階層化対応: Bounded Context別にopen
open UbiquitousLanguageManager.Domain.Common                  // UserId, ProjectId
open UbiquitousLanguageManager.Domain.Authentication          // (使用なし)
open UbiquitousLanguageManager.Domain.ProjectManagement       // Project, ProjectName, ProjectDescription, Domain
open UbiquitousLanguageManager.Domain.UbiquitousLanguageManagement  // (使用なし)

// 🎯 Phase B1 Step3: プロジェクト管理Command定義
// Command/Query分離実装 - コマンド側（状態変更操作）
// 【F#初学者向け解説】
// Commandパターンにより、プロジェクトへの状態変更操作を明確に表現します。
// レコード型を使用することで、不変性とスレッドセーフ性を確保し、
// Railway-oriented Programmingパターンによる型安全なエラーハンドリングを実現します。

// 📁 プロジェクト作成Command
// REQ-3.1.2準拠: デフォルトドメイン自動作成・原子性保証
type CreateProjectCommand = {
    Name: string                    // プロジェクト名（Smart Constructor適用前の生値）
    Description: string option      // プロジェクト説明（任意項目）
    OwnerId: string                // プロジェクト所有者ID
    OperatorUserId: string         // 操作実行者ID（権限チェック用）
} with
    // 🔧 Smart Constructorパターン: 型安全な変換処理
    // 【F#初学者向け解説】
    // C#のstring型からF#のDomain値オブジェクトへの変換を行います。
    // Result型を使用することで、変換エラーをコンパイル時に安全に処理できます。
    member this.toDomainTypes() : Result<ProjectName * ProjectDescription * UserId * UserId, string> =
        // プロジェクト名変換: Smart Constructor適用
        match ProjectName.create this.Name with
        | Error err -> Error $"プロジェクト名エラー: {err}"
        | Ok projectName ->
            // プロジェクト説明変換: Option型対応
            match ProjectDescription.create this.Description with
            | Error err -> Error $"プロジェクト説明エラー: {err}"
            | Ok projectDescription ->
                // ID変換: UserId型への変換
                let ownerId = UserId this.OwnerId
                let operatorId = UserId this.OperatorUserId
                Ok (projectName, projectDescription, ownerId, operatorId)

// 📝 プロジェクト編集Command
// REQ-3.1.3準拠: 説明のみ編集可能・プロジェクト名変更禁止
type UpdateProjectCommand = {
    ProjectId: Guid                // 更新対象プロジェクトID
    Description: string option     // 新しいプロジェクト説明
    OperatorUserId: string        // 操作実行者ID（権限チェック用）
} with
    // 🔧 Domain型変換: 権限チェック統合
    member this.toDomainTypes() : Result<ProjectId * ProjectDescription * UserId, string> =
        let projectId = ProjectId(int64(this.ProjectId.GetHashCode()))  // TDD: 一時的な変換
        match ProjectDescription.create this.Description with
        | Error err -> Error $"プロジェクト説明エラー: {err}"
        | Ok projectDescription ->
            let operatorId = UserId this.OperatorUserId
            Ok (projectId, projectDescription, operatorId)

// 🗑️ プロジェクト削除Command
// REQ-3.1.4準拠: 論理削除・関連データ影響確認・スーパーユーザーのみ
type DeleteProjectCommand = {
    ProjectId: Guid                // 削除対象プロジェクトID
    OperatorUserId: string        // 操作実行者ID（スーパーユーザー権限必須）
    ConfirmRelatedDataDeletion: bool  // 関連データ削除確認フラグ
} with
    member this.toDomainTypes() : ProjectId * UserId =
        (ProjectId(int64(this.ProjectId.GetHashCode())), UserId this.OperatorUserId)

// 👤 プロジェクト所有者変更Command
// プロジェクト所有権の移譲処理
type ChangeProjectOwnerCommand = {
    ProjectId: Guid                // 対象プロジェクトID
    NewOwnerId: string            // 新しい所有者ID
    OperatorUserId: string        // 操作実行者ID
} with
    member this.toDomainTypes() : ProjectId * UserId * UserId =
        (ProjectId(int64(this.ProjectId.GetHashCode())), UserId this.NewOwnerId, UserId this.OperatorUserId)

// ✅ プロジェクト有効化Command
// 論理削除されたプロジェクトの再有効化
type ActivateProjectCommand = {
    ProjectId: Guid                // 有効化対象プロジェクトID
    OperatorUserId: string        // 操作実行者ID
} with
    member this.toDomainTypes() : ProjectId * UserId =
        (ProjectId(int64(this.ProjectId.GetHashCode())), UserId this.OperatorUserId)

// 🎯 プロジェクト作成結果DTO
// 【F#初学者向け解説】
// ProjectDomainService.createProjectWithDefaultDomainの結果を受け取るためのDTO型です。
// プロジェクトとデフォルトドメインの両方の情報を含み、C#側への型変換で使用されます。
type ProjectCreationResultDto = {
    Project: Project               // 作成されたプロジェクト
    DefaultDomain: Domain         // 自動作成されたデフォルトドメイン
    CreatedAt: DateTime           // 作成日時
}

// 🔄 コマンド実行結果: Railway-oriented Programming統合
// 【F#初学者向け解説】
// Command実行の結果を型安全に表現します。Result型により、
// 成功時の値と失敗時のエラーメッセージを明確に分離し、エラーハンドリングを強制します。
type CommandResult<'TSuccess> = Result<'TSuccess, string>

// 🎯 プロジェクト管理Command結果型定義
type ProjectCommandResult = CommandResult<ProjectCreationResultDto>
type UpdateCommandResult = CommandResult<Project>
type DeleteCommandResult = CommandResult<unit>
type OwnerChangeCommandResult = CommandResult<Project>
type ActivateCommandResult = CommandResult<Project>

// 📊 プロジェクト統計情報Command
// ビジネスインテリジェンス機能の一部
type GetProjectStatisticsCommand = {
    OperatorUserId: string        // 統計情報要求者ID
    IncludeInactiveProjects: bool // 非アクティブプロジェクト含有フラグ
} with
    member this.toDomainTypes() : UserId =
        UserId this.OperatorUserId

// 👥 Phase B2: UserProjects多対多関連管理Command

// プロジェクトメンバー追加Command
// 【Phase B2: ユーザー・プロジェクト関連管理】
// - UserProjectsレコードINSERT
// - 重複追加チェック（複合一意制約）
// - SuperUser/ProjectManager権限のみ実行可能
type AddMemberToProjectCommand = {
    ProjectId: Guid               // 対象プロジェクトID
    UserId: string               // 追加するユーザーID
    OperatorUserId: string       // 操作実行者ID（権限チェック用）
    OperatorRole: Role           // 操作実行者ロール（権限チェック用）
} with
    // 🔧 Domain型変換
    // 【F#初学者向け解説】
    // Command レコードの生値をF# Domain層の型に変換します。
    // Result型により、変換エラーを安全に処理します。
    member this.toDomainTypes() : Result<ProjectId * UserId * UserId * Role, string> =
        let projectId = ProjectId(int64(this.ProjectId.GetHashCode()))
        let userId = UserId this.UserId
        let operatorId = UserId this.OperatorUserId
        Ok (projectId, userId, operatorId, this.OperatorRole)

// プロジェクトメンバー削除Command
// 【Phase B2: ユーザー・プロジェクト関連管理】
// - UserProjectsレコードDELETE（物理削除）
// - 最後の管理者削除防止チェック（Application層で実施）
// - SuperUser/ProjectManager権限のみ実行可能
type RemoveMemberFromProjectCommand = {
    ProjectId: Guid               // 対象プロジェクトID
    UserId: string               // 削除するユーザーID
    OperatorUserId: string       // 操作実行者ID（権限チェック用）
    OperatorRole: Role           // 操作実行者ロール（権限チェック用）
} with
    // 🔧 Domain型変換
    member this.toDomainTypes() : Result<ProjectId * UserId * UserId * Role, string> =
        let projectId = ProjectId(int64(this.ProjectId.GetHashCode()))
        let userId = UserId this.UserId
        let operatorId = UserId this.OperatorUserId
        Ok (projectId, userId, operatorId, this.OperatorRole)

// 🎯 Command結果型追加
type AddMemberCommandResult = CommandResult<unit>
type RemoveMemberCommandResult = CommandResult<unit>