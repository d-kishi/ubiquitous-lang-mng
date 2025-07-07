namespace UbiquitousLanguageManager.Domain

// 🎯 Value Objects: ドメイン駆動設計の基本構成要素
// 値によって同一性が決まるオブジェクト（IDを持たない）

// 📧 メールアドレス値オブジェクト
type Email = 
    private Email of string
with
    // 🔧 静的ファクトリーメソッド: 検証を伴う安全な生成
    static member create (emailStr: string) =
        if System.String.IsNullOrWhiteSpace(emailStr) then
            Error "メールアドレスが入力されていません"
        elif not (emailStr.Contains("@")) then
            Error "有効なメールアドレス形式ではありません"
        else
            Ok (Email emailStr)
    
    // 📤 値の取得: プライベートコンストラクタのため専用メソッドで値を取得
    member this.Value = 
        let (Email email) = this
        email

// 👤 ユーザー名値オブジェクト
type UserName = 
    private UserName of string
with
    static member create (nameStr: string) =
        if System.String.IsNullOrWhiteSpace(nameStr) then
            Error "ユーザー名が入力されていません"
        elif nameStr.Length > 50 then
            Error "ユーザー名は50文字以内で入力してください"
        else
            Ok (UserName nameStr)
    
    member this.Value = 
        let (UserName name) = this
        name

// 🌍 日本語名値オブジェクト
type JapaneseName = 
    private JapaneseName of string
with
    static member create (nameStr: string) =
        if System.String.IsNullOrWhiteSpace(nameStr) then
            Error "日本語名が入力されていません"
        elif nameStr.Length > 100 then
            Error "日本語名は100文字以内で入力してください"
        else
            Ok (JapaneseName nameStr)
    
    member this.Value = 
        let (JapaneseName name) = this
        name

// 🔤 英語名値オブジェクト
type EnglishName = 
    private EnglishName of string
with
    static member create (nameStr: string) =
        if System.String.IsNullOrWhiteSpace(nameStr) then
            Error "英語名が入力されていません"
        elif nameStr.Length > 100 then
            Error "英語名は100文字以内で入力してください"
        else
            Ok (EnglishName nameStr)
    
    member this.Value = 
        let (EnglishName name) = this
        name

// 📝 説明文値オブジェクト
type Description = 
    private Description of string
with
    static member create (descStr: string) =
        if System.String.IsNullOrWhiteSpace(descStr) then
            Error "説明が入力されていません"
        elif descStr.Length > 1000 then
            Error "説明は1000文字以内で入力してください"
        else
            Ok (Description descStr)
    
    member this.Value = 
        let (Description desc) = this
        desc

// 🎭 ユーザーロール: 権限管理のための列挙型
type UserRole =
    | SuperUser      // システム管理者
    | ProjectManager // プロジェクト管理者
    | DomainApprover // ドメイン承認者
    | GeneralUser    // 一般ユーザー

// 🔄 承認状態: ワークフロー管理のための列挙型
type ApprovalStatus =
    | Draft     // 下書き
    | Submitted // 承認申請中
    | Approved  // 承認済み
    | Rejected  // 却下

// 🆔 識別子型: 型安全なIDの実装
type UserId = UserId of int64
type ProjectId = ProjectId of int64
type DomainId = DomainId of int64
type UbiquitousLanguageId = UbiquitousLanguageId of int64