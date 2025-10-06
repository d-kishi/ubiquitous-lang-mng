namespace UbiquitousLanguageManager.Domain.Common

// 🎯 CommonValueObjects.fs: 共通値オブジェクト定義
// Bounded Context: Common
// このファイルは全Bounded Contextで共有される共通値オブジェクトを定義します
//
// 【F#初学者向け解説】
// 値オブジェクト（Value Object）は、ドメイン駆動設計（DDD）の基本構成要素です。
// エンティティ（Entity）と異なり、IDを持たず、値によって同一性が決まります。
// 例: "説明文A" と "説明文A" は同じ値オブジェクトとして扱われます。

open System

// 📝 説明文値オブジェクト（共通基底型）
// 【F#初学者向け解説】
// Smart Constructor パターンを適用した説明文の値オブジェクトです。
// privateコンストラクタにより、不正な値での作成を防ぎます。
//
// Smart Constructorとは:
// 1. コンストラクタをprivateにする
// 2. createメソッド（静的ファクトリーメソッド）で検証してから作成
// 3. Result<T, string>型で成功/失敗を型安全に表現
// これにより、無効な値を持つインスタンスの存在を防ぎます。
type Description =
    private Description of string
with
    // 静的ファクトリーメソッド: 検証を伴う安全な生成
    // 【F#初学者向け解説】
    // Result<Description, string> は以下の2つの状態を表現します:
    // - Ok description: 成功時、有効なDescriptionインスタンスを返す
    // - Error "エラーメッセージ": 失敗時、エラーメッセージを返す
    //
    // これにより、例外を使わずに安全にエラーを伝播できます（Railway-oriented Programming）
    static member create (descStr: string) : Result<Description, string> =
        // String.IsNullOrWhiteSpace: null・空文字列・空白文字のみをチェック
        if String.IsNullOrWhiteSpace(descStr) then
            Error "説明が入力されていません"
        elif descStr.Length > 1000 then
            Error "説明は1000文字以内で入力してください"
        else
            // 検証成功: 有効なDescriptionインスタンスを作成
            Ok (Description descStr)

    // Valueプロパティ: 内部の文字列値を取得
    // 【F#初学者向け解説】
    // パターンマッチングで内部値を取り出します。
    // let (Description desc) = this は「分解（Deconstruction）」と呼ばれ、
    // プライベートコンストラクタで包まれた値を取り出す操作です。
    member this.Value =
        let (Description desc) = this
        desc

// 🔄 承認状態: ワークフロー管理のための列挙型
// 【F#初学者向け解説】
// 判別共用体でワークフローの状態を型安全に表現します。
// C#のenumと似ていますが、F#の判別共用体はより強力で、
// パターンマッチングによる網羅的な状態処理が可能です。
//
// 状態遷移:
// Draft → Submitted → Approved/Rejected
type ApprovalStatus =
    | Draft     // 下書き: 編集中・まだ承認申請していない状態
    | Submitted // 承認申請中: 承認者のレビュー待ち状態
    | Approved  // 承認済み: 承認され、正式に採用された状態
    | Rejected  // 却下: 承認が却下された状態（再編集可能）