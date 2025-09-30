namespace UbiquitousLanguageManager.Domain

open System

// 🎯 UbiquitousLanguageErrors.fs: ユビキタス言語管理エラー型
// Bounded Context: UbiquitousLanguageManagement
// このファイルはユビキタス言語管理に関するドメインエラーを定義します
//
// 【F#初学者向け解説】
// 判別共用体（Discriminated Union）を使用してエラーを型安全に表現します。
// パターンマッチングにより、全てのエラーケースの処理を強制し、エラー処理漏れを防ぎます。
// C#のExceptionと異なり、Result型と組み合わせることで、エラーを値として扱い、
// コンパイル時にエラーハンドリングを強制できます。

// 🌍 ユビキタス言語管理エラー判別共用体
// 【F#初学者向け解説】
// 判別共用体により、ユビキタス言語管理のエラー種類を型安全に表現します。
// パターンマッチングによる網羅的なエラーハンドリングが可能になります。
//
// ケースの種類:
// - 値を持たないケース: 単純な列挙値として扱う
// - 値を持つケース: DuplicateJapaneseName of string（関連データを保持）
// - 複数の値を持つケース: タプルで複数値を保持
type UbiquitousLanguageError =
    // 🔍 重複エラー: 同一ドメイン内での用語名重複
    | DuplicateJapaneseName of string              // 日本語名が既に使用されている
    | DuplicateEnglishName of string               // 英語名が既に使用されている
    | DuplicateBothNames of string * string        // 日本語名・英語名ともに重複

    // 📋 承認ステータス関連エラー
    | InvalidApprovalStatus of string              // 無効な承認ステータス
    | UnauthorizedApproval of string               // 承認権限がない
    | InvalidStatusTransition of string * string   // 無効なステータス遷移（現在ステータス, 目標ステータス）

    // 🚫 ドメイン関連エラー
    | InactiveDomainError of string                // 非アクティブなドメインでの操作不可
    | DomainNotFound of int64                      // ドメインが存在しない（DomainId）

    // 🔐 権限・バリデーションエラー
    | InsufficientPermissions of string            // 権限不足
    | ValidationError of string                    // バリデーションエラー（型変換エラー等）

    // 💾 データベース関連エラー
    | DatabaseError of string                      // データベースエラー
    | ConcurrencyError of string                   // 並行性エラー（楽観的排他制御）

    // 🔧 システムエラー
    | SystemError of exn                           // システムエラー（例外情報付き）
with
    // 📝 エラーメッセージの生成: ユーザーフレンドリーなメッセージに変換
    // 【F#初学者向け解説】
    // パターンマッチングを使用して、各エラーケースに対応するメッセージを生成します。
    // match式は、すべてのケースを網羅しているかコンパイラがチェックします。
    member this.ToMessage() : string =
        match this with
        | DuplicateJapaneseName name -> $"日本語名「{name}」は既に使用されています"
        | DuplicateEnglishName name -> $"英語名「{name}」は既に使用されています"
        | DuplicateBothNames (japaneseName, englishName) ->
            $"日本語名「{japaneseName}」・英語名「{englishName}」ともに既に使用されています"
        | InvalidApprovalStatus status -> $"無効な承認ステータスです: {status}"
        | UnauthorizedApproval msg -> $"承認権限がありません: {msg}"
        | InvalidStatusTransition (current, target) ->
            $"ステータス「{current}」から「{target}」への変更は許可されていません"
        | InactiveDomainError msg -> $"非アクティブなドメインでは操作できません: {msg}"
        | DomainNotFound domainId -> $"ドメイン（ID: {domainId}）が見つかりません"
        | InsufficientPermissions msg -> $"権限が不足しています: {msg}"
        | ValidationError msg -> $"入力値が不正です: {msg}"
        | DatabaseError msg -> $"データベースエラーが発生しました: {msg}"
        | ConcurrencyError msg -> $"並行性エラー: {msg}"
        | SystemError ex -> $"システムエラーが発生しました: {ex.Message}"

    // 🔍 エラーカテゴリの判定: エラーの種類による分類
    // 【F#初学者向け解説】
    // エラーの種類により、適切な処理を行うためのカテゴリ分類を行います。
    // これにより、上位レイヤーでエラーの扱いを統一できます。
    member this.GetCategory() : string =
        match this with
        | DuplicateJapaneseName _ | DuplicateEnglishName _ | DuplicateBothNames _ -> "Duplication"
        | InvalidApprovalStatus _ | UnauthorizedApproval _ | InvalidStatusTransition _ -> "ApprovalWorkflow"
        | InactiveDomainError _ | DomainNotFound _ -> "Domain"
        | InsufficientPermissions _ -> "Authorization"
        | ValidationError _ -> "Validation"
        | DatabaseError _ -> "Database"
        | ConcurrencyError _ -> "Concurrency"
        | SystemError _ -> "System"

    // 🔒 セキュリティ上のエラーか判定: ログ記録の重要度判定に使用
    // 【F#初学者向け解説】
    // セキュリティに関連するエラーは、監査ログに記録する必要があります。
    // この関数により、どのエラーを重点的にログ記録すべきかを判定できます。
    member this.IsSecurityRelated() : bool =
        match this with
        | UnauthorizedApproval _ | InsufficientPermissions _ -> true
        | _ -> false