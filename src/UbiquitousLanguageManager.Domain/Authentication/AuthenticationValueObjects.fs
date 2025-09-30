namespace UbiquitousLanguageManager.Domain

open System

// 🎯 AuthenticationValueObjects.fs: 認証関連値オブジェクト定義
// Bounded Context: Authentication
// このファイルは認証・ユーザー管理に関する値オブジェクトを定義します
//
// 【F#初学者向け解説】
// Value Object（値オブジェクト）は、IDを持たず値によって同一性が決まるオブジェクトです。
// Smart Constructorパターンを適用し、不正な値での作成を防ぎます。

// 📧 メールアドレス値オブジェクト
// 【F#初学者向け解説】
// private コンストラクタにより、外部から直接 Email を作成できません。
// createメソッドを通じてのみ、バリデーションを経た適切な値でインスタンスを作成できます。
// これが Smart Constructor パターンです。
type Email =
    private Email of string
with
    // 🔧 静的ファクトリーメソッド: 検証を伴う安全な生成
    // 【F#初学者向け解説】
    // Result<Email, string>型を返すことで、成功時はOk(Email)、失敗時はError(エラーメッセージ)を返します。
    // これにより、エラーハンドリングをコンパイル時に強制し、例外を使わない安全なコードが書けます。
    static member create (emailStr: string) : Result<Email, string> =
        if System.String.IsNullOrWhiteSpace(emailStr) then
            Error "メールアドレスが入力されていません"
        elif emailStr.Length > 254 then
            Error "メールアドレスは254文字以内で入力してください"
        elif not (emailStr.Contains("@")) then
            Error "有効なメールアドレス形式ではありません"
        elif emailStr.StartsWith("@") || emailStr.EndsWith("@") then
            Error "有効なメールアドレス形式ではありません"
        elif emailStr.IndexOf("@") <> emailStr.LastIndexOf("@") then
            Error "有効なメールアドレス形式ではありません"
        elif not (emailStr.Contains(".")) then
            Error "有効なメールアドレス形式ではありません"
        else
            let atIndex = emailStr.IndexOf("@")
            let localPart = emailStr.Substring(0, atIndex)
            let domainPart = emailStr.Substring(atIndex + 1)
            if System.String.IsNullOrWhiteSpace(localPart) || System.String.IsNullOrWhiteSpace(domainPart) then
                Error "有効なメールアドレス形式ではありません"
            elif domainPart.IndexOf(".") = -1 then
                Error "有効なメールアドレス形式ではありません"
            else
                Ok (Email emailStr)

    // 📤 値の取得: プライベートコンストラクタのため専用メソッドで値を取得
    // 【F#初学者向け解説】
    // パターンマッチングで内部値を取り出します。
    // let (Email email) = this は「分解（Deconstruction）」と呼ばれる操作です。
    member this.Value =
        let (Email email) = this
        email

// 👤 ユーザー名値オブジェクト
// 【F#初学者向け解説】
// ユーザー名のビジネスルール（必須・50文字以内）を型レベルで保証します。
type UserName =
    private UserName of string
with
    static member create (nameStr: string) : Result<UserName, string> =
        if System.String.IsNullOrWhiteSpace(nameStr) then
            Error "ユーザー名が入力されていません"
        elif nameStr.Length > 50 then
            Error "ユーザー名は50文字以内で入力してください"
        else
            Ok (UserName nameStr)

    member this.Value =
        let (UserName name) = this
        name

// 🔐 パスワード値オブジェクト（平文パスワード用）
// 【F#初学者向け解説】
// これは平文パスワードを一時的に保持するためのValue Objectです。
// セキュリティの観点から、パスワードは即座にハッシュ化され、
// この値オブジェクトはInfrastructure層でのハッシュ化処理のみで使用されます。
type Password =
    private Password of string
with
    // パスワード強度バリデーションを含む安全な生成
    // 【F#初学者向け解説】
    // 正規表現を使用してパスワードの複雑性要件をチェックします。
    // - 8文字以上
    // - 大文字を含む
    // - 小文字を含む
    // - 数字を含む
    static member create (passwordStr: string) : Result<Password, string> =
        if System.String.IsNullOrWhiteSpace(passwordStr) then
            Error "パスワードが入力されていません"
        elif passwordStr.Length < 8 then
            Error "パスワードは8文字以上で入力してください"
        elif passwordStr.Length > 100 then
            Error "パスワードは100文字以内で入力してください"
        elif not (System.Text.RegularExpressions.Regex.IsMatch(passwordStr, "[A-Z]")) then
            Error "パスワードには大文字を含めてください"
        elif not (System.Text.RegularExpressions.Regex.IsMatch(passwordStr, "[a-z]")) then
            Error "パスワードには小文字を含めてください"
        elif not (System.Text.RegularExpressions.Regex.IsMatch(passwordStr, "[0-9]")) then
            Error "パスワードには数字を含めてください"
        else
            Ok (Password passwordStr)

    member this.Value =
        let (Password pwd) = this
        pwd

// 🔐 パスワードハッシュ値オブジェクト
// 【F#初学者向け解説】
// パスワードは平文で保存せず、必ずハッシュ化して保存します。
// このValue Objectは、ハッシュ化されたパスワードのみを受け入れ、
// ドメイン層でのパスワード処理の安全性を保証します。
type PasswordHash =
    private PasswordHash of string
with
    // パスワードハッシュは外部（Infrastructure層）で生成されたものを受け取る
    // 【F#初学者向け解説】
    // ドメイン層ではパスワードのハッシュ化処理は行いません（関心の分離）。
    // Infrastructure層で生成されたハッシュ値を受け取り、ドメインモデルに組み込みます。
    static member create (hashStr: string) : Result<PasswordHash, string> =
        if System.String.IsNullOrWhiteSpace(hashStr) then
            Error "パスワードハッシュが入力されていません"
        else
            Ok (PasswordHash hashStr)

    member this.Value =
        let (PasswordHash hash) = this
        hash

// 🔒 セキュリティスタンプ値オブジェクト
// ASP.NET Core Identityで使用される、ユーザーの認証状態変更を追跡する値
// 【F#初学者向け解説】
// SecurityStampは、パスワード変更・メール変更・ロール変更などの
// セキュリティに影響する操作が行われた際に更新されます。
// これにより、既存のセッションを無効化し、セキュリティを確保します。
type SecurityStamp =
    private SecurityStamp of string
with
    static member create (stampStr: string) : Result<SecurityStamp, string> =
        if System.String.IsNullOrWhiteSpace(stampStr) then
            // 新規作成時は自動生成
            Ok (SecurityStamp (System.Guid.NewGuid().ToString("N")))
        else
            Ok (SecurityStamp stampStr)

    // 新規スタンプの生成（静的メソッド）
    // 【F#初学者向け解説】
    // 「N」フォーマット指定子は、ハイフンなしの32文字の16進数文字列を生成します。
    // 例: "00000000000000000000000000000000"
    static member createNew () =
        SecurityStamp (System.Guid.NewGuid().ToString("N"))

    member this.Value =
        let (SecurityStamp stamp) = this
        stamp

// 🔄 並行性スタンプ値オブジェクト
// Entity Frameworkの楽観的並行性制御で使用
// 【F#初学者向け解説】
// ConcurrencyStampは、データベースの楽観的並行性制御に使用されます。
// 2つのユーザーが同時に同じデータを更新しようとした際、
// 後から更新しようとした操作がエラーになり、データの整合性を保ちます。
type ConcurrencyStamp =
    private ConcurrencyStamp of string
with
    static member create (stampStr: string) : Result<ConcurrencyStamp, string> =
        if System.String.IsNullOrWhiteSpace(stampStr) then
            Ok (ConcurrencyStamp (System.Guid.NewGuid().ToString()))
        else
            Ok (ConcurrencyStamp stampStr)

    static member createNew () =
        ConcurrencyStamp (System.Guid.NewGuid().ToString())

    member this.Value =
        let (ConcurrencyStamp stamp) = this
        stamp

// 📧 強化版Email値オブジェクト（より厳密なバリデーション）
// 【F#初学者向け解説】
// 既存のEmailを拡張し、より厳密なメールアドレス検証を行います。
// System.Net.Mail.MailAddressを使用してRFC準拠の検証を実行します。
type StrongEmail =
    private StrongEmail of string
with
    static member create (emailStr: string) : Result<StrongEmail, string> =
        if System.String.IsNullOrWhiteSpace(emailStr) then
            Error "メールアドレスが入力されていません"
        elif emailStr.Length > 254 then
            Error "メールアドレスは254文字以内で入力してください"
        else
            try
                // RFC準拠のメールアドレス検証
                // 【F#初学者向け解説】
                // try-with式はF#の例外ハンドリングです。
                // :? は型のパターンマッチング（型チェック）を行います。
                let mailAddress = System.Net.Mail.MailAddress(emailStr)
                if mailAddress.Address = emailStr then
                    Ok (StrongEmail emailStr)
                else
                    Error "有効なメールアドレス形式ではありません"
            with
            | :? System.FormatException -> Error "有効なメールアドレス形式ではありません"
            | :? System.ArgumentException -> Error "有効なメールアドレス形式ではありません"

    member this.Value =
        let (StrongEmail email) = this
        email

// 👤 ユーザープロフィール値オブジェクト
// 【F#初学者向け解説】
// ユーザーの詳細プロフィール情報を表現するValue Objectです。
// レコード型として不変な構造で定義し、プロフィール更新時は
// 新しいインスタンスを作成することで不変性を保ちます。
//
// Option型の活用:
// - None = 値が設定されていない（nullではない）
// - Some value = 値が設定されている
// Option型により、nullによる不具合を型安全に防ぎます。
type UserProfile = {
    DisplayName: string option        // 表示名（任意）
    Department: string option         // 所属部署（任意）
    PhoneNumber: string option        // 電話番号（任意）
    Notes: string option              // 備考（任意）
} with
    // ファクトリーメソッド: 値の正規化を行いながらプロフィールを作成
    // 【F#初学者向け解説】
    // 空文字列やnullをNoneに変換し、Option型の意味を明確にします。
    // Trim()により前後の空白を除去し、データを正規化します。
    static member create displayName department phoneNumber notes = {
        DisplayName =
            if System.String.IsNullOrWhiteSpace(displayName) then None
            else Some (displayName.Trim())
        Department =
            if System.String.IsNullOrWhiteSpace(department) then None
            else Some (department.Trim())
        PhoneNumber =
            if System.String.IsNullOrWhiteSpace(phoneNumber) then None
            else Some (phoneNumber.Trim())
        Notes =
            if System.String.IsNullOrWhiteSpace(notes) then None
            else Some (notes.Trim())
    }

    // 空のプロフィール: 初期状態の作成
    static member empty = {
        DisplayName = None
        Department = None
        PhoneNumber = None
        Notes = None
    }