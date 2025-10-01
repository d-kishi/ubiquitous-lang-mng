namespace UbiquitousLanguageManager.Domain.ProjectManagement

open System

// 🎯 ProjectValueObjects.fs: プロジェクト管理値オブジェクト
// Bounded Context: ProjectManagement
// このファイルはプロジェクト管理に関する値オブジェクトを定義します
//
// 【F#初学者向け解説】
// Smart Constructor パターンを適用した値オブジェクトです。
// privateコンストラクタにより、不正な値での作成を防ぎ、createメソッドを通じてのみ
// 適切に検証された値でインスタンスを作成できます。これにより型安全性を確保します。

// 📁 プロジェクト名値オブジェクト（Phase B1）
// 【F#初学者向け解説】
// Smart Constructor パターンを適用したプロジェクト名の値オブジェクトです。
// - private 修飾子: 外部から直接 ProjectName(値) を作れないようにします
// - create 静的メソッド: 検証を伴う安全な生成を提供します
// - Result型: 成功(Ok)・失敗(Error)を型安全に表現します
//
// ビジネスルール:
// - 必須項目（空文字列・null・空白のみ禁止）
// - 最小3文字以上
// - 最大100文字以内
type ProjectName =
    private ProjectName of string
with
    // 🔧 静的ファクトリーメソッド: 検証を伴う安全な生成
    // 【F#初学者向け解説】
    // Result<ProjectName, string> は成功時にProjectName型、失敗時にstring型を返します。
    // C#の try-catch と異なり、エラーハンドリングをコンパイル時に強制します。
    static member create (nameStr: string) : Result<ProjectName, string> =
        // パターンマッチングによる段階的バリデーション
        // 【F#初学者向け解説】
        // if-elif-else構文で複数の条件を順次チェックします。
        // 最初に当てはまった条件で処理が確定し、それ以降の条件は評価されません。
        if System.String.IsNullOrWhiteSpace(nameStr) then
            Error "プロジェクト名は必須です"
        elif nameStr.Length > 100 then
            Error "プロジェクト名は100文字以内で入力してください"
        elif nameStr.Length < 3 then
            Error "プロジェクト名は3文字以上で入力してください"
        else
            Ok (ProjectName nameStr)

    // 📤 値の取得: プライベートコンストラクタのため専用メソッドで値を取得
    // 【F#初学者向け解説】
    // パターンマッチングで内部値を取り出します。
    // let (ProjectName name) = this は「分解（Deconstruction）」と呼ばれる操作です。
    // これにより、Wrapper型の内部値に安全にアクセスできます。
    member this.Value =
        let (ProjectName name) = this
        name

// 📝 プロジェクト説明値オブジェクト（Phase B1）
// 【F#初学者向け解説】
// Option型を活用してプロジェクト説明の任意性を表現します。
// - None = 説明なし
// - Some description = 説明あり
// これにより、空文字列とnullによる混乱を防ぎ、型安全に任意項目を表現します。
//
// ビジネスルール:
// - 任意項目（None許可）
// - 空文字列・空白のみの場合はNoneとして扱う
// - 最大1000文字以内
type ProjectDescription =
    private ProjectDescription of string option
with
    // 🔧 Option型から作成
    // 【F#初学者向け解説】
    // match式はF#のパターンマッチングの基本構文です。
    // Option型は None | Some value の2つのケースを持つ判別共用体です。
    // すべてのケースを処理することをコンパイラが強制するため、null参照例外を防げます。
    static member create (descStr: string option) : Result<ProjectDescription, string> =
        match descStr with
        | None -> Ok (ProjectDescription None)  // 説明なしは正常
        | Some desc when System.String.IsNullOrWhiteSpace(desc) ->
            // 空文字列・空白のみはNoneとして扱う（正規化）
            Ok (ProjectDescription None)
        | Some desc when desc.Length > 1000 ->
            Error "プロジェクト説明は1000文字以内で入力してください"
        | Some desc ->
            // Trim()で前後の空白を除去して保存
            Ok (ProjectDescription (Some (desc.Trim())))

    // 🔧 文字列から作成するヘルパーメソッド
    // 【F#初学者向け解説】
    // C#のstring型から直接作成できるヘルパーメソッドです。
    // Application層やContracts層で使いやすくするために提供します。
    static member createFromString (descStr: string) : Result<ProjectDescription, string> =
        if System.String.IsNullOrWhiteSpace(descStr) then
            Ok (ProjectDescription None)
        else
            ProjectDescription.create (Some descStr)

    // 📤 値の取得
    // 【F#初学者向け解説】
    // Option<string>型を返すため、呼び出し側でNoneチェックが必要です。
    // これにより、説明が存在しない場合の処理漏れをコンパイル時に防げます。
    member this.Value =
        let (ProjectDescription desc) = this
        desc

// 🏷️ ドメイン名値オブジェクト（Phase B1）
// 【F#初学者向け解説】
// ドメイン名のためのSmart Constructor実装です。
// プロジェクト名と同様のバリデーションルールを適用します。
//
// ビジネスルール:
// - 必須項目
// - 最小3文字以上
// - 最大100文字以内
// - デフォルトドメイン自動生成対応（"{ProjectName}_Default"形式）
type DomainName =
    private DomainName of string
with
    // 🔧 静的ファクトリーメソッド
    // 【F#初学者向け解説】
    // プロジェクト名と同じバリデーションロジックを適用します。
    // ビジネスルール上、ドメイン名とプロジェクト名は同等の制約を持ちます。
    static member create (nameStr: string) : Result<DomainName, string> =
        if System.String.IsNullOrWhiteSpace(nameStr) then
            Error "ドメイン名は必須です"
        elif nameStr.Length > 100 then
            Error "ドメイン名は100文字以内で入力してください"
        elif nameStr.Length < 3 then
            Error "ドメイン名は3文字以上で入力してください"
        else
            Ok (DomainName nameStr)

    // 📤 値の取得
    member this.Value =
        let (DomainName name) = this
        name