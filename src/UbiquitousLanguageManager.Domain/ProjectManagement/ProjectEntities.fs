namespace UbiquitousLanguageManager.Domain.ProjectManagement

open System
open UbiquitousLanguageManager.Domain.Common
open UbiquitousLanguageManager.Domain.Authentication

// 🎯 ProjectEntities.fs: プロジェクト管理エンティティ
// Bounded Context: ProjectManagement
// このファイルはプロジェクト管理に関するエンティティを定義します
//
// 【F#初学者向け解説】
// Clean Architecture Aggregate Rootパターンを適用したエンティティ定義です。
// レコード型の不変性により、データの整合性を保ちながら安全に管理ができます。
// Smart Constructorパターンと組み合わせることで、不正な状態のエンティティを作成できません。

// 📁 プロジェクトエンティティ: ドメイン領域の管理単位（Phase B1 拡張版）
// 【F#初学者向け解説】
// Clean Architecture Aggregate Rootパターンを適用したプロジェクトエンティティです。
// レコード型の不変性により、データの整合性を保ちながら安全にプロジェクト管理ができます。
// Smart Constructorパターンと組み合わせることで、不正な状態のProjectを作成できません。
//
// ビジネスルール:
// - プロジェクトは必ず所有者（OwnerId）を持つ
// - アクティブなプロジェクトのみ編集可能
// - 非アクティブなプロジェクトは論理削除（IsActive = false）
// - プロジェクト作成時にデフォルトドメインを自動作成（ProjectDomainServiceで実装）
type Project = {
    Id: ProjectId
    Name: ProjectName               // Phase B1: 専用のProjectName値オブジェクト使用
    Description: ProjectDescription  // Phase B1: 専用のProjectDescription値オブジェクト使用
    OwnerId: UserId                 // Phase B1: プロジェクト所有者の追加
    CreatedAt: DateTime             // Phase B1: 作成日時の追加（監査情報）
    UpdatedAt: DateTime option      // Phase B1: 更新日時をOption型で表現
    IsActive: bool
} with
    // 🔧 プロジェクト作成（Phase B1 拡張版）
    // 【F#初学者向け解説】
    // ファクトリーメソッドパターンにより、適切に初期化されたProjectを作成します。
    // TDD Green Phase: テスト成功のため一意なID生成ロジック実装
    // IDは一意性を保つため、現在時刻のTicksとランダム値を組み合わせて生成します。
    //
    // Railway-oriented Programming対応:
    // - Application層ではResult<Project, ProjectCreationError>で使用
    // - Domain層では単純なProject返却（バリデーションは値オブジェクトで完了）
    static member create (name: ProjectName) (description: ProjectDescription) (ownerId: UserId) : Project =
        // TDD Green Phase: テスト成功のため一意なID生成ロジック実装
        let uniqueId =
            let random = System.Random()
            let ticks = System.DateTime.UtcNow.Ticks
            let randomValue = random.Next(1000, 9999) |> int64
            ticks + randomValue

        {
            Id = ProjectId uniqueId  // 🔄 一意なIDを生成して設定
            Name = name
            Description = description
            OwnerId = ownerId
            CreatedAt = DateTime.UtcNow
            UpdatedAt = None    // 作成時は更新日時なし
            IsActive = true
        }

    // 🔧 ID付きプロジェクト作成（テスト用）
    // 【F#初学者向け解説】
    // テストやApplication層での型変換確認のため、IDを明示的に指定できるメソッドです。
    // F#のメソッドオーバーロード制限により、異なる名前を使用します。
    //
    // 用途:
    // - Unit Test: 既知のIDでテストデータを作成
    // - TypeConverter Test: F#↔C#境界での型変換整合性確認
    // - Infrastructure層: データベースから取得したIDで復元
    static member createWithId (id: ProjectId) (name: ProjectName) (description: ProjectDescription) (ownerId: UserId) : Project = {
        Id = id
        Name = name
        Description = description
        OwnerId = ownerId
        CreatedAt = DateTime.UtcNow
        UpdatedAt = None
        IsActive = true
    }

    // 📝 プロジェクト名変更: ビジネスルールを適用した更新
    // 【F#初学者向け解説】
    // Result型を使用することで、エラーハンドリングをコンパイル時に強制します。
    // プロジェクトの状態変更は新しいインスタンスを返すことで不変性を保ちます。
    //
    // ビジネスルール:
    // - 非アクティブなプロジェクトは名前変更不可
    // - 名前変更時は更新日時を自動更新
    member this.changeName (newName: ProjectName) (updatedBy: UserId) : Result<Project, string> =
        if not this.IsActive then
            Error "非アクティブなプロジェクトの名前は変更できません"
        else
            Ok { this with
                    Name = newName
                    UpdatedAt = Some DateTime.UtcNow }

    // 📝 プロジェクト説明変更: 説明の更新処理
    // 【F#初学者向け解説】
    // Option型のProjectDescriptionを受け取り、説明を更新します。
    // None（説明なし）も正当な値として扱います。
    member this.changeDescription (newDescription: ProjectDescription) (updatedBy: UserId) : Result<Project, string> =
        if not this.IsActive then
            Error "非アクティブなプロジェクトの説明は変更できません"
        else
            Ok { this with
                    Description = newDescription
                    UpdatedAt = Some DateTime.UtcNow }

    // 🚫 プロジェクト無効化: 論理削除によるプロジェクト無効化
    // 【F#初学者向け解説】
    // 物理削除ではなく論理削除を行います。これにより、関連するドメインや
    // ユビキタス言語との関係を保ちながら、新規作業を防ぐことができます。
    //
    // ビジネスルール:
    // - 削除権限チェック（DeleteProjects権限）
    // - 既に無効化済みの場合はエラー
    // - アクティブなドメインが存在する場合は削除不可（DomainServiceで実装）
    member this.deactivate (operatorUser: User) (updatedBy: UserId) : Result<Project, string> =
        if not (PermissionMappings.hasPermission operatorUser.Role DeleteProjects) then
            Error "プロジェクト削除の権限がありません"
        elif not this.IsActive then
            Error "既に無効化されているプロジェクトです"
        else
            Ok { this with
                    IsActive = false
                    UpdatedAt = Some DateTime.UtcNow }

    // ✅ プロジェクト有効化: 無効化されたプロジェクトの再有効化
    // 【F#初学者向け解説】
    // 論理削除されたプロジェクトを再度有効化する機能です。
    // プロジェクト管理権限が必要です。
    member this.activate (operatorUser: User) (updatedBy: UserId) : Result<Project, string> =
        if not (PermissionMappings.hasPermission operatorUser.Role ManageProjects) then
            Error "プロジェクト管理の権限がありません"
        elif this.IsActive then
            Error "既に有効化されているプロジェクトです"
        else
            Ok { this with
                    IsActive = true
                    UpdatedAt = Some DateTime.UtcNow }

    // 👤 プロジェクト所有者変更: 所有権移譲の処理
    // 【F#初学者向け解説】
    // プロジェクトの所有者を変更します。
    // プロジェクト管理権限が必要で、現在の所有者と同じユーザーは指定できません。
    member this.changeOwner (newOwnerId: UserId) (operatorUser: User) (updatedBy: UserId) : Result<Project, string> =
        if not (PermissionMappings.hasPermission operatorUser.Role ManageProjects) then
            Error "プロジェクト管理の権限がありません"
        elif not this.IsActive then
            Error "非アクティブなプロジェクトの所有者は変更できません"
        elif this.OwnerId = newOwnerId then
            Error "現在の所有者と同じユーザーが指定されています"
        else
            Ok { this with
                    OwnerId = newOwnerId
                    UpdatedAt = Some DateTime.UtcNow }

// 🏷️ ドメインエンティティ: プロジェクト内の特定領域（Phase B1 拡張版）
// 【F#初学者向け解説】
// プロジェクト内のドメイン境界を表現するエンティティです。
// 新しいDomainName値オブジェクトを使用し、デフォルトドメイン自動作成に対応しています。
//
// ビジネスルール:
// - ドメインは必ずプロジェクト（ProjectId）に所属
// - 各プロジェクトに1つのデフォルトドメインが自動作成される（IsDefault = true）
// - デフォルトドメインは削除不可
// - アクティブなユビキタス言語が存在するドメインは削除不可
type Domain = {
    Id: DomainId
    ProjectId: ProjectId
    Name: DomainName                // Phase B1: 専用のDomainName値オブジェクト使用
    Description: ProjectDescription // Phase B1: プロジェクトと統一したDescription使用
    OwnerId: UserId                 // Phase B1: ドメイン所有者の追加
    IsDefault: bool                 // Phase B1: デフォルトドメインフラグの追加
    CreatedAt: DateTime             // Phase B1: 作成日時の追加（監査情報）
    UpdatedAt: DateTime option      // Phase B1: 更新日時をOption型で表現
    IsActive: bool
} with
    // 🔧 ドメイン作成（Phase B1 拡張版）
    // 【F#初学者向け解説】
    // プロジェクト作成時のデフォルトドメイン自動作成に対応したファクトリーメソッドです。
    // TDD Green Phase: テスト成功のため一意なID生成ロジック実装
    //
    // 通常のドメイン作成では IsDefault = false として作成します。
    // デフォルトドメインの作成には createDefault メソッドを使用します。
    static member create (name: DomainName) (projectId: ProjectId) (ownerId: UserId) : Domain =
        // TDD Green Phase: テスト成功のため一意なID生成ロジック実装
        let uniqueId =
            let random = System.Random()
            let ticks = System.DateTime.UtcNow.Ticks
            let randomValue = random.Next(2000, 8999) |> int64  // ProjectIdと重複を避けるため異なる範囲
            ticks + randomValue

        {
            Id = DomainId uniqueId  // 🔄 一意なIDを生成して設定
            ProjectId = projectId
            Name = name
            Description = ProjectDescription.create None |> function
                         | Ok desc -> desc
                         | Error _ -> failwith "空の説明作成に失敗"
            OwnerId = ownerId
            IsDefault = false
            CreatedAt = DateTime.UtcNow
            UpdatedAt = None
            IsActive = true
        }

    // 🔧 デフォルトドメイン作成（Phase B1 新規追加）
    // 【F#初学者向け解説】
    // プロジェクト作成時に自動的に作成されるデフォルトドメインのファクトリーメソッドです。
    // IsDefault = true により、このドメインがデフォルトであることを示します。
    //
    // デフォルトドメイン命名規則:
    // - "{ProjectName}_Default" 形式でドメイン名を自動生成
    // - 説明: "プロジェクト作成時に自動生成されたデフォルトドメインです"
    //
    // Railway-oriented Programming対応:
    // - Result<Domain, string>型を返却
    // - DomainName作成失敗時はエラーを伝播
    static member createDefault (projectId: ProjectId) (projectName: ProjectName) (ownerId: UserId) : Result<Domain, string> =
        let defaultDomainName = $"{projectName.Value}_Default"
        match DomainName.create defaultDomainName with
        | Ok domainName ->
            let description =
                ProjectDescription.create (Some "プロジェクト作成時に自動生成されたデフォルトドメインです")
                |> function
                   | Ok desc -> desc
                   | Error _ -> ProjectDescription.create None |> function
                                | Ok desc -> desc
                                | Error _ -> failwith "デフォルトドメイン説明作成に失敗"
            // TDD Green Phase: テスト成功のため一意なID生成ロジック実装
            let uniqueId =
                let random = System.Random()
                let ticks = System.DateTime.UtcNow.Ticks
                let randomValue = random.Next(3000, 7999) |> int64  // 他のIDと重複を避けるため異なる範囲
                ticks + randomValue

            Ok {
                Id = DomainId uniqueId  // 🔄 一意なIDを生成して設定
                ProjectId = projectId
                Name = domainName
                Description = description
                OwnerId = ownerId
                IsDefault = true   // デフォルトドメインフラグ
                CreatedAt = DateTime.UtcNow
                UpdatedAt = None
                IsActive = true
            }
        | Error err -> Error $"デフォルトドメイン作成失敗: {err}"

    // 🔧 ID付きドメイン作成（テスト用）
    // 【F#初学者向け解説】
    // テスト用にIDを明示的に指定してドメインを作成します。
    // Project.createWithIdと同様の用途です。
    static member createWithId (id: DomainId) (name: DomainName) (projectId: ProjectId) (ownerId: UserId) : Domain = {
        Id = id
        ProjectId = projectId
        Name = name
        Description = ProjectDescription.create None |> function
                     | Ok desc -> desc
                     | Error _ -> failwith "空の説明作成に失敗"
        OwnerId = ownerId
        IsDefault = false
        CreatedAt = DateTime.UtcNow
        UpdatedAt = None
        IsActive = true
    }

// 🚨 Note: DraftUbiquitousLanguage と FormalUbiquitousLanguage は
// ユビキタス言語管理境界文脈に属するため、Phase 5以降で別途移行します。
// Phase 4ではProjectManagement境界文脈のProject・Domainのみを移行対象とします。