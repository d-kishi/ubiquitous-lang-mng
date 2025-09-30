namespace UbiquitousLanguageManager.Domain

// 🎯 UbiquitousLanguageValueObjects.fs: ユビキタス言語管理の値オブジェクト
// Bounded Context: UbiquitousLanguageManagement
// このファイルはユビキタス言語管理に関する値オブジェクトを定義します
//
// 【F#初学者向け解説】
// 値オブジェクト（Value Object）は、IDではなく値そのもので同一性が決まる不変なオブジェクトです。
// Smart Constructor パターンを適用し、不正な値の作成を防ぎます。

// 🌍 日本語名型（ユビキタス言語用）
// 【F#初学者向け解説】
// Smart Constructor パターンを適用した日本語名の値オブジェクトです。
// privateコンストラクタにより、不正な値での作成を防ぎ、createメソッドを通じてのみ
// 適切に検証された値でインスタンスを作成できます。これにより型安全性を確保します。
type JapaneseName =
    private JapaneseName of string
with
    // 🔧 静的ファクトリーメソッド: 検証を伴う安全な生成
    // Result型を使用することで、エラーケースを明示的に扱えるようにします
    static member create (nameStr: string) : Result<JapaneseName, string> =
        if System.String.IsNullOrWhiteSpace(nameStr) then
            Error "日本語名が入力されていません"
        elif nameStr.Length > 100 then
            Error "日本語名は100文字以内で入力してください"
        else
            Ok (JapaneseName nameStr)

    // 📤 値の取得: プライベートコンストラクタのため専用プロパティで値を取得
    // パターンマッチングを使用して、ラップされた文字列値を取り出します
    member this.Value =
        let (JapaneseName name) = this
        name

// 🔤 英語名型（ユビキタス言語用）
// 【F#初学者向け解説】
// Smart Constructor パターンを適用した英語名の値オブジェクトです。
// 日本語名と同様に、型安全な生成とバリデーションを提供します。
type EnglishName =
    private EnglishName of string
with
    // 🔧 静的ファクトリーメソッド: 検証を伴う安全な生成
    // Result型によるエラーハンドリングで、エラーケースを型レベルで管理
    static member create (nameStr: string) : Result<EnglishName, string> =
        if System.String.IsNullOrWhiteSpace(nameStr) then
            Error "英語名が入力されていません"
        elif nameStr.Length > 100 then
            Error "英語名は100文字以内で入力してください"
        else
            Ok (EnglishName nameStr)

    // 📤 値の取得: プライベートコンストラクタのため専用プロパティで値を取得
    member this.Value =
        let (EnglishName name) = this
        name