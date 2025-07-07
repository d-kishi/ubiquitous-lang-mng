namespace UbiquitousLanguageManager.Domain

// 🎯 Result型: F#の強力なエラーハンドリング機能
// 成功・失敗の両方を型安全に表現し、例外を使わない関数型プログラミングを実現
type Result<'TSuccess, 'TError> =
    | Success of 'TSuccess
    | Error of 'TError

// 🔧 Result型操作モジュール: 関数型プログラミングのコンビネーター
module Result =
    
    // ✅ 成功値を作成
    let success value = Success value
    
    // ❌ エラー値を作成
    let error errorValue = Error errorValue
    
    // 🎭 パターンマッチング: F#の制御構文でResult型を安全に処理
    let map f result =
        match result with
        | Success value -> Success (f value)
        | Error error -> Error error
    
    // 🔄 bind操作: モナディックな合成（複数のResult操作を連鎖）
    let bind f result =
        match result with
        | Success value -> f value
        | Error error -> Error error
    
    // 🎯 isSuccess: 成功判定（boolean値を返す）
    let isSuccess result =
        match result with
        | Success _ -> true
        | Error _ -> false
    
    // 🎯 isError: エラー判定（boolean値を返す）
    let isError result =
        match result with
        | Success _ -> false
        | Error _ -> true

// 🎯 計算式表現: F#のコンピュテーション式でResult型を直感的に操作
type ResultBuilder() =
    member _.Return(value) = Success value
    member _.ReturnFrom(result) = result
    member _.Bind(result, f) = Result.bind f result
    member _.Zero() = Success ()

// 🔧 計算式のインスタンス: resultブロック内でdo!、let!を使用可能
let result = ResultBuilder()