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

// 🔐 パスワードハッシュ値オブジェクト
// 【F#初学者向け解説】
// パスワードは平文で保存せず、必ずハッシュ化して保存します。
// このValue Objectは、ハッシュ化されたパスワードのみを受け入れ、
// ドメイン層でのパスワード処理の安全性を保証します。
type PasswordHash = 
    private PasswordHash of string
with
    // パスワードハッシュは外部（Infrastructure層）で生成されたものを受け取る
    static member create (hashStr: string) =
        if System.String.IsNullOrWhiteSpace(hashStr) then
            Error "パスワードハッシュが入力されていません"
        else
            Ok (PasswordHash hashStr)
    
    member this.Value = 
        let (PasswordHash hash) = this
        hash

// 🔒 セキュリティスタンプ値オブジェクト
// ASP.NET Core Identityで使用される、ユーザーの認証状態変更を追跡する値
type SecurityStamp = 
    private SecurityStamp of string
with
    static member create (stampStr: string) =
        if System.String.IsNullOrWhiteSpace(stampStr) then
            // 新規作成時は自動生成
            Ok (SecurityStamp (System.Guid.NewGuid().ToString("N")))
        else
            Ok (SecurityStamp stampStr)
    
    static member createNew () =
        SecurityStamp (System.Guid.NewGuid().ToString("N"))
    
    member this.Value = 
        let (SecurityStamp stamp) = this
        stamp

// 🔄 並行性スタンプ値オブジェクト
// Entity Frameworkの楽観的並行性制御で使用
type ConcurrencyStamp = 
    private ConcurrencyStamp of string
with
    static member create (stampStr: string) =
        if System.String.IsNullOrWhiteSpace(stampStr) then
            Ok (ConcurrencyStamp (System.Guid.NewGuid().ToString()))
        else
            Ok (ConcurrencyStamp stampStr)
    
    static member createNew () =
        ConcurrencyStamp (System.Guid.NewGuid().ToString())
    
    member this.Value = 
        let (ConcurrencyStamp stamp) = this
        stamp

// 🆔 識別子型: 型安全なIDの実装
type UserId = 
    | UserId of int64
with
    member this.Value = 
        let (UserId id) = this
        id
    static member create(id: int64) = UserId id

type ProjectId = 
    | ProjectId of int64
with
    member this.Value = 
        let (ProjectId id) = this
        id
    static member create(id: int64) = ProjectId id

type DomainId = 
    | DomainId of int64
with
    member this.Value = 
        let (DomainId id) = this
        id
    static member create(id: int64) = DomainId id

type UbiquitousLanguageId = 
    | UbiquitousLanguageId of int64
with
    member this.Value = 
        let (UbiquitousLanguageId id) = this
        id
    static member create(id: int64) = UbiquitousLanguageId id