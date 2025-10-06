namespace UbiquitousLanguageManager.Domain.ProjectManagement

open System

// 🎯 ProjectErrors.fs: プロジェクト管理エラー型
// Bounded Context: ProjectManagement
// このファイルはプロジェクト管理に関するドメインエラーを定義します
//
// 【F#初学者向け解説】
// 判別共用体（Discriminated Union）を使用してエラーを型安全に表現します。
// パターンマッチングにより、全てのエラーケースの処理を強制し、エラー処理漏れを防ぎます。
// C#のExceptionと異なり、Result型と組み合わせることで、エラーを値として扱い、
// コンパイル時にエラーハンドリングを強制できます。

// 📁 プロジェクト作成エラー判別共用体（Phase B1 完全版）
// 【F#初学者向け解説】
// 判別共用体の各ケースは異なる情報を持つことができます。
// - DuplicateProjectName of string: 文字列（重複プロジェクト名情報）を持つケース
// - SystemError of exn: 例外情報を持つケース
// これにより、エラーの種類に応じた詳細情報を型安全に保持できます。
//
// Railway-oriented Programming対応:
// - Result<'T, ProjectCreationError>型と組み合わせて使用
// - エラー時は即座に処理を中断し、エラー情報を伝播
// - 成功時のみ次の処理へ進む
type ProjectCreationError =
    | DuplicateProjectName of string           // プロジェクト名重複
    | InvalidProjectName of string             // 無効なプロジェクト名
    | InvalidProjectDescription of string      // 無効なプロジェクト説明
    | InsufficientPermissions of string        // 権限不足（要求仕様追加）
    | DomainCreationFailed of string          // デフォルトドメイン作成失敗
    | DatabaseError of string                  // データベースエラー
    | SystemError of exn                       // システムエラー（例外情報付き）

// 🏷️ ドメイン作成エラー判別共用体（Phase B1）
// 【F#初学者向け解説】
// ドメイン作成時に発生する可能性のあるエラーを型安全に表現します。
// プロジェクト作成エラーと分離することで、エラーの発生源を明確にします。
//
// 用途:
// - ドメイン単独作成時のエラーハンドリング
// - プロジェクト作成時のデフォルトドメイン作成エラー（ProjectCreationError.DomainCreationFailedに変換）
type DomainCreationError =
    | DuplicateDomainName of string            // ドメイン名重複（同一プロジェクト内）
    | InvalidDomainName of string              // 無効なドメイン名
    | InvalidDomainDescription of string       // 無効なドメイン説明
    | ProjectNotFound of int64                 // プロジェクトが存在しない（ProjectId）
    | InsufficientPermissions of string        // 権限不足
    | DatabaseError of string                  // データベースエラー
    | SystemError of exn                       // システムエラー（例外情報付き）

// 📊 プロジェクト更新エラー判別共用体（Phase B1 拡張）
// 【F#初学者向け解説】
// プロジェクトの更新操作（名前変更・説明変更・無効化等）で発生するエラーです。
// 作成エラーとは別に定義することで、操作の種類によるエラーハンドリングを明確にします。
type ProjectUpdateError =
    | ProjectNotFound of int64                 // プロジェクトが存在しない（ProjectId）
    | DuplicateProjectName of string           // プロジェクト名重複（名前変更時）
    | InvalidProjectName of string             // 無効なプロジェクト名（名前変更時）
    | InvalidProjectDescription of string      // 無効なプロジェクト説明（説明変更時）
    | ProjectNotActive of int64                // プロジェクトが非アクティブ
    | InsufficientPermissions of string        // 権限不足
    | HasActiveDomains of int64 * int          // アクティブなドメインが存在（ProjectId, ドメイン数）
    | DatabaseError of string                  // データベースエラー
    | ConcurrencyError of string               // 並行性エラー（楽観的排他制御）
    | SystemError of exn                       // システムエラー（例外情報付き）

// 🏷️ ドメイン更新エラー判別共用体（Phase B1）
// 【F#初学者向け解説】
// ドメインの更新操作で発生するエラーです。
// ユビキタス言語との関連チェックなど、ドメイン固有のビジネスルールを表現します。
type DomainUpdateError =
    | DomainNotFound of int64                  // ドメインが存在しない（DomainId）
    | DuplicateDomainName of string            // ドメイン名重複（名前変更時・同一プロジェクト内）
    | InvalidDomainName of string              // 無効なドメイン名（名前変更時）
    | InvalidDomainDescription of string       // 無効なドメイン説明（説明変更時）
    | DomainNotActive of int64                 // ドメインが非アクティブ
    | CannotDeleteDefaultDomain of int64       // デフォルトドメインは削除不可（DomainId）
    | HasActiveLanguages of int64 * int        // アクティブなユビキタス言語が存在（DomainId, 言語数）
    | InsufficientPermissions of string        // 権限不足
    | DatabaseError of string                  // データベースエラー
    | ConcurrencyError of string               // 並行性エラー（楽観的排他制御）
    | SystemError of exn                       // システムエラー（例外情報付き）

// 🔍 プロジェクト検証エラー判別共用体（Phase B1）
// 【F#初学者向け解説】
// プロジェクト管理のビジネスルール検証で発生するエラーです。
// DomainService層での複雑なビジネスロジック検証時に使用します。
//
// 使用例:
// - 複数プロジェクト間の整合性チェック
// - プロジェクト統計情報の妥当性検証
// - プロジェクト削除時の依存関係チェック
type ProjectValidationError =
    | ProjectNameConflict of string * string list  // プロジェクト名衝突（名前, 衝突しているProjectId一覧）
    | InvalidProjectStructure of string            // 無効なプロジェクト構造
    | InvalidProjectStatistics of string           // 無効なプロジェクト統計情報
    | CircularDependency of string                 // 循環依存検出
    | ValidationFailed of string                   // 一般的な検証失敗
    | SystemError of exn                           // システムエラー（例外情報付き）

// 🔄 エラー変換ヘルパーモジュール
// 【F#初学者向け解説】
// 異なるエラー型間の変換を行うヘルパー関数を提供します。
// 例: DomainCreationError → ProjectCreationError
//     下位層のエラーを上位層のエラーに変換する際に使用します。
module ErrorConversions =

    // DomainCreationError → ProjectCreationError 変換
    // 【F#初学者向け解説】
    // パターンマッチングですべてのケースを網羅的に処理します。
    // F#コンパイラは、すべてのケースが処理されているか確認するため、
    // エラー変換の漏れを防ぐことができます。
    let domainCreationErrorToProjectCreationError (error: DomainCreationError) : ProjectCreationError =
        match error with
        | DomainCreationError.DuplicateDomainName msg -> ProjectCreationError.DomainCreationFailed msg
        | DomainCreationError.InvalidDomainName msg -> ProjectCreationError.DomainCreationFailed msg
        | DomainCreationError.InvalidDomainDescription msg -> ProjectCreationError.DomainCreationFailed msg
        | DomainCreationError.ProjectNotFound projectId ->
            ProjectCreationError.DatabaseError $"プロジェクト（ID: {projectId}）が見つかりません"
        | DomainCreationError.InsufficientPermissions msg -> ProjectCreationError.InsufficientPermissions msg
        | DomainCreationError.DatabaseError msg -> ProjectCreationError.DatabaseError msg
        | DomainCreationError.SystemError ex -> ProjectCreationError.SystemError ex

    // エラーメッセージ取得ヘルパー
    // 【F#初学者向け解説】
    // 判別共用体からユーザー向けのエラーメッセージを生成します。
    // UI層でエラー表示する際に使用します。

    let getProjectCreationErrorMessage (error: ProjectCreationError) : string =
        match error with
        | ProjectCreationError.DuplicateProjectName msg -> msg
        | ProjectCreationError.InvalidProjectName msg -> msg
        | ProjectCreationError.InvalidProjectDescription msg -> msg
        | ProjectCreationError.InsufficientPermissions msg -> msg
        | ProjectCreationError.DomainCreationFailed msg -> $"デフォルトドメイン作成失敗: {msg}"
        | ProjectCreationError.DatabaseError msg -> $"データベースエラー: {msg}"
        | ProjectCreationError.SystemError ex -> $"システムエラー: {ex.Message}"

    let getDomainCreationErrorMessage (error: DomainCreationError) : string =
        match error with
        | DomainCreationError.DuplicateDomainName msg -> msg
        | DomainCreationError.InvalidDomainName msg -> msg
        | DomainCreationError.InvalidDomainDescription msg -> msg
        | DomainCreationError.ProjectNotFound projectId -> $"プロジェクト（ID: {projectId}）が見つかりません"
        | DomainCreationError.InsufficientPermissions msg -> msg
        | DomainCreationError.DatabaseError msg -> $"データベースエラー: {msg}"
        | DomainCreationError.SystemError ex -> $"システムエラー: {ex.Message}"

    let getProjectUpdateErrorMessage (error: ProjectUpdateError) : string =
        match error with
        | ProjectUpdateError.ProjectNotFound projectId -> $"プロジェクト（ID: {projectId}）が見つかりません"
        | ProjectUpdateError.DuplicateProjectName msg -> msg
        | ProjectUpdateError.InvalidProjectName msg -> msg
        | ProjectUpdateError.InvalidProjectDescription msg -> msg
        | ProjectUpdateError.ProjectNotActive projectId -> $"プロジェクト（ID: {projectId}）は非アクティブです"
        | ProjectUpdateError.InsufficientPermissions msg -> msg
        | ProjectUpdateError.HasActiveDomains (projectId, count) ->
            $"プロジェクト（ID: {projectId}）にはアクティブなドメインが{count}件存在します"
        | ProjectUpdateError.DatabaseError msg -> $"データベースエラー: {msg}"
        | ProjectUpdateError.ConcurrencyError msg -> $"並行性エラー: {msg}"
        | ProjectUpdateError.SystemError ex -> $"システムエラー: {ex.Message}"

    let getDomainUpdateErrorMessage (error: DomainUpdateError) : string =
        match error with
        | DomainUpdateError.DomainNotFound domainId -> $"ドメイン（ID: {domainId}）が見つかりません"
        | DomainUpdateError.DuplicateDomainName msg -> msg
        | DomainUpdateError.InvalidDomainName msg -> msg
        | DomainUpdateError.InvalidDomainDescription msg -> msg
        | DomainUpdateError.DomainNotActive domainId -> $"ドメイン（ID: {domainId}）は非アクティブです"
        | DomainUpdateError.CannotDeleteDefaultDomain domainId ->
            $"デフォルトドメイン（ID: {domainId}）は削除できません"
        | DomainUpdateError.HasActiveLanguages (domainId, count) ->
            $"ドメイン（ID: {domainId}）にはアクティブなユビキタス言語が{count}件存在します"
        | DomainUpdateError.InsufficientPermissions msg -> msg
        | DomainUpdateError.DatabaseError msg -> $"データベースエラー: {msg}"
        | DomainUpdateError.ConcurrencyError msg -> $"並行性エラー: {msg}"
        | DomainUpdateError.SystemError ex -> $"システムエラー: {ex.Message}"

    let getProjectValidationErrorMessage (error: ProjectValidationError) : string =
        match error with
        | ProjectNameConflict (name, conflictingIds) ->
            // 【F#初学者向け解説】
            // 文字列補間内では複雑な式（関数呼び出し等）を直接使用できないため、
            // 事前にlet束縛で値を作成してから文字列補間に埋め込みます。
            let ids = String.concat ", " conflictingIds
            $"プロジェクト名「{name}」は既に使用されています（衝突ID: {ids}）"
        | InvalidProjectStructure msg -> $"無効なプロジェクト構造: {msg}"
        | InvalidProjectStatistics msg -> $"無効なプロジェクト統計情報: {msg}"
        | CircularDependency msg -> $"循環依存検出: {msg}"
        | ValidationFailed msg -> $"検証失敗: {msg}"
        | ProjectValidationError.SystemError ex -> $"システムエラー: {ex.Message}"