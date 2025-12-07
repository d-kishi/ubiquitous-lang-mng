# ID変換フロー図

**作成日**: 2025-12-07
**目的**: GitHub Issue #79「ID体系統一リファクタリング」準備

---

## 1. 現状のID体系概要

```mermaid
graph TB
    subgraph "ASP.NET Core Identity層"
        A[ApplicationUser.Id<br/>string GUID]
    end

    subgraph "Infrastructure層 C#"
        B[AuthenticatedUser.Id<br/>int]
        C[UserRepository<br/>Guid → long変換]
    end

    subgraph "Domain層 F#"
        D[UserId<br/>int64 Single-Case DU]
        E[ProjectId<br/>int64 Single-Case DU]
    end

    A -->|GetHashCode| B
    A -->|GetHashCode| C
    C -->|int64()| D

    style A fill:#f9f,stroke:#333
    style B fill:#bbf,stroke:#333
    style C fill:#bbf,stroke:#333
    style D fill:#bfb,stroke:#333
    style E fill:#bfb,stroke:#333
```

---

## 2. 認証フロー（Login → UserId取得 → 権限判定）

```mermaid
sequenceDiagram
    participant User as ユーザー
    participant Login as Login.razor
    participant Auth as AuthenticationService
    participant Identity as ASP.NET Core Identity
    participant Domain as Domain層

    User->>Login: ログイン情報入力
    Login->>Auth: SignInAsync(email, password)
    Auth->>Identity: FindByEmailAsync(email)
    Identity-->>Auth: ApplicationUser (Id = GUID string)

    Note over Auth: GetHashCode()変換<br/>L233: Id = user.Id.GetHashCode()

    Auth-->>Login: AuthenticatedUser (Id = int)

    Note over Login,Domain: この時点でGUIDとint間の<br/>対応関係が失われる

    Login->>Domain: 権限判定リクエスト
    Domain-->>Login: 権限情報
```

### 2.1 問題点

1. **GetHashCode()変換（L233）**: GUID → int変換でハッシュ衝突リスク
2. **対応関係喪失**: 変換後、元のGUIDへの逆算不可能
3. **プロセス間不整合**: GetHashCode()は.NETランタイム毎に異なる値を返す可能性

---

## 3. ユーザー作成フロー

```mermaid
sequenceDiagram
    participant Admin as 管理者
    participant Create as Create.razor
    participant Service as UserManagementService
    participant Auth as AuthenticationService
    participant Identity as ASP.NET Core Identity
    participant DB as PostgreSQL

    Admin->>Create: ユーザー情報入力
    Create->>Service: CreateUserAsync(dto)
    Service->>Auth: RegisterUserAsync(email, password)
    Auth->>Identity: CreateAsync(ApplicationUser)

    Note over Identity: 新規GUID生成<br/>Id = Guid.NewGuid().ToString()

    Identity->>DB: INSERT AspNetUsers
    Identity-->>Auth: IdentityResult + ApplicationUser

    Note over Auth: GetHashCode()変換<br/>L1333: UserId.NewUserId((long)Id.GetHashCode())

    Auth-->>Service: F# User (UserId = int64)
    Service-->>Create: 作成結果
```

### 3.1 問題点

1. **L1333**: 新規ユーザー作成時にGetHashCode()でUserId生成
2. **衝突リスク**: 大量ユーザー作成時にハッシュ衝突確率上昇
3. **追跡困難**: ユーザーIDからGUIDへの逆引き不可

---

## 4. プロジェクト参照フロー

```mermaid
sequenceDiagram
    participant User as ユーザー
    participant List as ProjectList.razor
    participant Query as GetUserProjectsQuery
    participant Service as ProjectManagementService
    participant Repo as ProjectRepository
    participant DB as PostgreSQL

    User->>List: プロジェクト一覧表示
    List->>Query: new GetUserProjectsQuery(userId: Guid)

    Note over Query: GetHashCode()変換<br/>Queries.fs L34:<br/>UserId(int64(this.UserId.GetHashCode()))

    Query->>Service: Execute(query)
    Service->>Repo: GetProjectsByUserAsync(UserId)
    Repo->>DB: SELECT * FROM UserProjects WHERE UserId = ?

    Note over DB: DBにはint64で格納<br/>GetHashCode()変換済みの値

    DB-->>Repo: プロジェクト一覧
    Repo-->>Service: F# Project list
    Service-->>List: プロジェクト一覧DTO
```

### 4.1 問題点

1. **Query層変換**: 毎回GetHashCode()でGUID → int64変換
2. **DB格納値**: 既にGetHashCode()変換済みの値が格納
3. **一貫性**: 同じGUIDでも異なるプロセスで異なる値になる可能性

---

## 5. 変換箇所マッピング

```mermaid
graph LR
    subgraph "Web層 C#"
        W1[Index.razor<br/>Guid.TryParse]
        W2[Create.razor<br/>Guid.TryParse]
        W3[Edit.razor<br/>Guid.TryParse]
    end

    subgraph "Application層 F#"
        A1[Queries.fs<br/>GetHashCode x13]
        A2[Commands.fs<br/>GetHashCode x14]
    end

    subgraph "Infrastructure層 C#"
        I1[AuthenticationService.cs<br/>GetHashCode x2]
        I2[UserRepository.cs<br/>GetHashCode x1]
    end

    subgraph "Domain層 F#"
        D1[UserId<br/>int64]
        D2[ProjectId<br/>int64]
    end

    W1 --> A1
    W2 --> A2
    W3 --> A2
    A1 --> D1
    A2 --> D1
    A2 --> D2
    I1 --> D1
    I2 --> D1

    style A1 fill:#fbb,stroke:#333
    style A2 fill:#fbb,stroke:#333
    style I1 fill:#fbb,stroke:#333
    style I2 fill:#fbb,stroke:#333
```

---

## 6. InitialDataとの関連

```mermaid
graph TB
    subgraph "InitialData 02_initial_data.sql"
        ID1["admin-001<br/>(人間可読ID)"]
        ID2["pm-001<br/>(人間可読ID)"]
        ID3["user-001<br/>(人間可読ID)"]
    end

    subgraph "AspNetUsers"
        DB1["Id: admin-001<br/>(string)"]
        DB2["Id: pm-001<br/>(string)"]
        DB3["Id: user-001<br/>(string)"]
    end

    subgraph "変換処理"
        C1["Guid.TryParse('admin-001')<br/>→ 失敗"]
        C2["GetHashCode()<br/>→ 不安定なint"]
    end

    ID1 --> DB1
    ID2 --> DB2
    ID3 --> DB3

    DB1 --> C1
    C1 -->|失敗| C2

    style C1 fill:#fbb,stroke:#333
    style C2 fill:#fbb,stroke:#333
```

### 6.1 問題点

1. **人間可読ID**: `admin-001`等はGUID形式ではない
2. **Guid.TryParse失敗**: Web層でGUIDとしてパースできない
3. **GetHashCode依存**: 結果としてGetHashCode()での変換に依存

---

## 7. 理想的なID変換フロー（修正後）

```mermaid
sequenceDiagram
    participant Web as Web層 (C#)
    participant App as Application層 (F#)
    participant Infra as Infrastructure層 (C#)
    participant DB as PostgreSQL

    Note over Web,DB: 統一ID体系: GUID文字列

    Web->>App: Query/Command (userId: string GUID)

    Note over App: GUID文字列をそのまま使用<br/>GetHashCode()廃止

    App->>Infra: Service呼び出し (userId: string)
    Infra->>DB: WHERE AspNetUsers.Id = userId
    DB-->>Infra: ユーザーデータ
    Infra-->>App: F# User (IdentityId: string)
    App-->>Web: DTO
```

### 7.1 修正方針

1. **GUID文字列統一**: 全層でGUID文字列をそのまま使用
2. **GetHashCode廃止**: 全30箇所から排除
3. **InitialData修正**: 人間可読ID → GUID文字列に変更
4. **型定義見直し**: `UserId of int64` → `UserId of string` 検討

---

## 8. 修正影響範囲

```mermaid
pie title GetHashCode使用箇所分布
    "Queries.fs" : 13
    "Commands.fs" : 14
    "AuthenticationService.cs" : 2
    "UserRepository.cs" : 1
```

---

**作成日**: 2025-12-07
**作成者**: MainAgent
