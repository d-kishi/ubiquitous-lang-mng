# Clean Architecture レイヤー分離原則

## 概要

F# + C# Clean Architecture実装における、レイヤー責務分離と依存関係の原則。ADR_010から抽出。

---

## 🔴 CRITICAL: 依存関係図

```
Web (C#)
  ↓
Contracts (C#)
  ↓
Application (F#)
  ↓
Domain (F#)
  ↑
Infrastructure (C#)  ← 依存関係逆転（DIP）
```

---

## 言語別責務分離

### F#の責務（Domain/Application層）

| 層 | 責務 | 特性 |
|---|------|------|
| **Domain** | ドメインロジック・型定義（VO/Entity/Aggregate）・ビジネスルール・Smart Constructor・Specification Pattern | 純粋関数・不変データ・型安全 |
| **Application** | ユースケース実装・Command/Query分離・Railway-oriented・権限制御・トランザクション境界 | Result型・Option型エラーハンドリング |

### C#の責務（Infrastructure/Web/Contracts層）

| 層 | 責務 | 特性 |
|---|------|------|
| **Infrastructure** | DB操作（EF Core）・Repository実装・外部システム統合・I/O処理・Transaction制御 | 副作用カプセル化 |
| **Web（Blazor Server）** | UIコンポーネント・入力処理・画面遷移・SignalR通信・認証UI | UI責務のみ |
| **Contracts** | DTOs・TypeConverter実装・F#↔C#型変換 | 境界変換 |

---

## 依存ルール

### ✅ 許可される依存

| パターン | 説明 | 例 |
|---------|------|-----|
| **C# → F#** | Web/Infrastructure層からDomain層呼び出し | `using ...Application.ProjectManagement;` |
| **Infrastructure → Domain** | Repository実装がDomain型を使用 | `public async Task<Project> GetByIdAsync(ProjectId id)` |
| **Interface経由** | Domain層でInterface定義、Infrastructure層で実装 | `type IProjectRepository = abstract member GetByIdAsync...` |

### ❌ 禁止される依存

| パターン | 説明 |
|---------|------|
| **F# → C#** | Domain/Application層からInfrastructure層への直接参照 |
| **下位層 → 上位層** | InfrastructureからWebへの参照 |
| **循環依存** | いかなる層間での循環参照も禁止 |

---

## レイヤー別責務マトリックス

| 層 | 責務 | ❌ 禁止事項 |
|----|------|-----------|
| **Domain (F#)** | ビジネスルール定義・Entity/VO定義・DomainService | DB操作・UI処理・外部呼び出し・I/O |
| **Application (F#)** | ユースケース調整・トランザクション境界・権限制御 | 具体的DB操作・UI実装・ビジネスルール実装 |
| **Infrastructure (C#)** | DB操作実装・Repository実装・外部サービス統合 | ビジネスルール実装・UI処理 |
| **Web (C#)** | ユーザーインタラクション・画面表示・フォーム処理 | ビジネスルール実装・DB直接アクセス |

### 実装例（Domain層・純粋なビジネスロジック）

```fsharp
module ProjectDomainService =
    let createProjectWithDefaultDomain (projectName: ProjectName) =
        projectName
        |> Project.create
        |> Result.bind (fun project ->
            Domain.createDefault project.Id
            |> Result.map (fun domain -> project, domain))
```

---

## 循環依存の検出と解決

### 検出方法

| 方法 | コマンド/確認内容 |
|------|-----------------|
| **ビルドエラー** | `Error: Circular dependency detected between projects` |
| **参照確認** | `grep -r "ProjectReference" src/` |
| **依存グラフ** | Domain ← Application ← Infrastructure/Web |

### 解決方法

| 方法 | 説明 |
|------|------|
| **DIP適用** | Domain層でInterface定義、Infrastructure層で実装 |
| **中間層導入** | Contracts層による境界分離 |
| **イベント駆動** | 層間をイベントで疎結合化（将来拡張） |

---

## 検証チェックリスト

### Step開始時（必須）

- [ ] プロジェクト参照確認（循環依存なし）
- [ ] namespace構造確認（レイヤー別分離）
- [ ] using/open文確認（不正な依存なし）

### Phase完了時（必須）

- [ ] 全層レイヤー責務遵守確認
- [ ] 循環依存ゼロ確認
- [ ] Clean Architecture 97点以上確認
- [ ] 0 Warning/0 Error確認

---

## 参考情報

- **Clean Architecture**: Robert C. Martin著
- **Domain Modeling Made Functional**: Scott Wlaschin著
- **Phase B1実装記録**: `Doc/08_Organization/Completed/Phase_B1/Phase_Summary.md`
