namespace UbiquitousLanguageManager.Domain

open System

// 🎯 AuthenticationErrors.fs: 認証関連エラー定義
// Bounded Context: Authentication
// このファイルは認証・ユーザー管理に関するドメインエラーを定義します
//
// 【F#初学者向け解説】
// 判別共用体（Discriminated Union）により、エラーの種類を型安全に表現します。
// パターンマッチングによる網羅的なエラーハンドリングが可能になります。

// 🔐 Phase A9: 認証エラー判別共用体
// Railway-oriented Programming対応・F#↔C#境界TypeConverter拡張
// 【F#初学者向け解説】
// 判別共用体により、認証エラーの種類を型安全に表現します。
// パターンマッチングによる網羅的なエラーハンドリングが可能になります。
//
// ケースの種類:
// - 値を持たないケース: InvalidCredentials（単純な列挙値）
// - 値を持つケース: UserNotFound of Email（関連データを保持）
// - 複数の値を持つケース: AccountLocked of Email * DateTime（タプルで複数値）
type AuthenticationError =
    | InvalidCredentials                           // 認証情報が正しくない
    | UserNotFound of Email                        // ユーザーが見つからない
    | ValidationError of string                    // バリデーションエラー（型変換エラー等）
    | AccountLocked of Email * DateTime            // アカウントロックアウト
    | SystemError of exn                           // システムエラー（例外情報付き）
    | PasswordExpired of Email                     // パスワード期限切れ
    | TwoFactorRequired of Email                   // 二要素認証が必要
    // 🔐 Phase A9: パスワードリセット関連エラー（4種類）
    | PasswordResetTokenExpired of Email           // パスワードリセットトークン期限切れ
    | PasswordResetTokenInvalid of Email           // 無効なパスワードリセットトークン
    | PasswordResetNotRequested of Email           // パスワードリセット未要求
    | PasswordResetAlreadyUsed of Email            // パスワードリセットトークン使用済み
    // 🔒 Phase A9: トークン関連エラー（4種類）
    | TokenGenerationFailed of string              // トークン生成失敗
    | TokenValidationFailed of string              // トークン検証失敗
    | TokenExpired of string                       // トークン期限切れ
    | TokenRevoked of string                       // トークン無効化
    // 👮 Phase A9: 管理者操作関連エラー（3種類）
    | InsufficientPermissions of string            // 権限不足（ロール・権限情報）
    | OperationNotAllowed of string                // 操作不許可
    | ConcurrentOperationDetected of string        // 並行操作検出
    // 🔮 Phase A9: 将来拡張用エラー（4種類）
    | TwoFactorAuthFailed of Email                 // 二要素認証失敗
    | ExternalAuthenticationFailed of string       // 外部認証失敗
    | AuditLogError of string                      // 監査ログエラー
    | AccountDeactivated                           // アカウント無効化
with
    // 📝 エラーメッセージの生成: ユーザーフレンドリーなメッセージに変換
    // 【F#初学者向け解説】
    // パターンマッチングを使用して、各エラーケースに対応するメッセージを生成します。
    // match式は、すべてのケースを網羅しているかコンパイラがチェックします。
    member this.ToMessage() : string =
        match this with
        | InvalidCredentials -> "メールアドレスまたはパスワードが正しくありません"
        | UserNotFound email -> $"ユーザー '{email.Value}' が見つかりません"
        | ValidationError msg -> $"入力値が不正です: {msg}"
        | AccountLocked (email, lockoutEnd) ->
            let formattedDate = lockoutEnd.ToString("yyyy/MM/dd HH:mm")
            $"アカウント '{email.Value}' はロックされています（解除予定: {formattedDate}）"
        | SystemError ex -> $"システムエラーが発生しました: {ex.Message}"
        | PasswordExpired email -> $"アカウント '{email.Value}' のパスワードの有効期限が切れています"
        | TwoFactorRequired email -> $"アカウント '{email.Value}' には二要素認証が必要です"
        | PasswordResetTokenExpired email -> $"パスワードリセットトークンの有効期限が切れています: {email.Value}"
        | PasswordResetTokenInvalid email -> $"無効なパスワードリセットトークンです: {email.Value}"
        | PasswordResetNotRequested email -> $"パスワードリセットが要求されていません: {email.Value}"
        | PasswordResetAlreadyUsed email -> $"このパスワードリセットトークンは既に使用されています: {email.Value}"
        | TokenGenerationFailed msg -> $"トークン生成に失敗しました: {msg}"
        | TokenValidationFailed msg -> $"トークン検証に失敗しました: {msg}"
        | TokenExpired msg -> $"トークンの有効期限が切れています: {msg}"
        | TokenRevoked msg -> $"トークンは無効化されています: {msg}"
        | InsufficientPermissions msg -> $"権限が不足しています: {msg}"
        | OperationNotAllowed msg -> $"この操作は許可されていません: {msg}"
        | ConcurrentOperationDetected msg -> $"並行操作が検出されました: {msg}"
        | TwoFactorAuthFailed email -> $"二要素認証に失敗しました: {email.Value}"
        | ExternalAuthenticationFailed msg -> $"外部認証に失敗しました: {msg}"
        | AuditLogError msg -> $"監査ログ記録に失敗しました: {msg}"
        | AccountDeactivated -> "アカウントは無効化されています"

    // 🔍 エラーカテゴリの判定: エラーの種類による分類
    // 【F#初学者向け解説】
    // エラーの種類により、適切な処理を行うためのカテゴリ分類を行います。
    // これにより、上位レイヤーでエラーの扱いを統一できます。
    member this.GetCategory() : string =
        match this with
        | InvalidCredentials | UserNotFound _ | PasswordExpired _ -> "Authentication"
        | ValidationError _ -> "Validation"
        | AccountLocked _ | AccountDeactivated -> "AccountStatus"
        | SystemError _ -> "System"
        | TwoFactorRequired _ | TwoFactorAuthFailed _ -> "TwoFactor"
        | PasswordResetTokenExpired _ | PasswordResetTokenInvalid _
        | PasswordResetNotRequested _ | PasswordResetAlreadyUsed _ -> "PasswordReset"
        | TokenGenerationFailed _ | TokenValidationFailed _
        | TokenExpired _ | TokenRevoked _ -> "Token"
        | InsufficientPermissions _ | OperationNotAllowed _ -> "Authorization"
        | ConcurrentOperationDetected _ -> "Concurrency"
        | ExternalAuthenticationFailed _ -> "ExternalAuth"
        | AuditLogError _ -> "Audit"

    // 🔒 セキュリティ上のエラーか判定: ログ記録の重要度判定に使用
    // 【F#初学者向け解説】
    // セキュリティに関連するエラーは、監査ログに記録する必要があります。
    // この関数により、どのエラーを重点的にログ記録すべきかを判定できます。
    member this.IsSecurityRelated() : bool =
        match this with
        | InvalidCredentials | AccountLocked _ | TwoFactorRequired _
        | TwoFactorAuthFailed _ | PasswordResetTokenInvalid _
        | InsufficientPermissions _ | OperationNotAllowed _ -> true
        | _ -> false