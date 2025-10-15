# UserProjects依存関係分析結果・実装順序計画

**作成日**: 2025-10-15
**分析対象**: Phase B2 UserProjects多対多関連実装
**分析担当**: dependency-analysis Agent

---

## 🎯 核心的判断・発見事項

### 1. Domain層拡張: ❌ **不要**（Phase B2確定）

**理由**:
- UserProject集約は**多対多関連の中間テーブル**のみ
- ドメインロジックなし（ビジネスルールはApplication層で実装）
- 既存Phase B1 Project集約に変更なし

**重要な決定**: **Step3（Domain層拡張）をスキップ → Step4に統合**

### 2. UserProjectsテーブル: ✅ **完全実装済み**（Phase A既存）

**既存実装確認**:
- EF Core Entity完全定義（UserProject.cs: 60行）
- DbContext設定完了（行328-372）
- Migration完了（20250729153117_FinalInitMigrationWithComments.cs）
- 外部キー制約3件・インデックス4件設定済み

**Phase B2での作業**:
- ❌ テーブル定義変更不要
- ✅ Repository実装のみ必要（UserProjectsアクセスロジック）

### 3. 実装順序最適化（Step2-5）

```
Step2（Playwright統合・1.5-2h）
  ↓ 並列実行可能
Step4（Application/Infrastructure層・3-4h）
  Infrastructure層（1.5-2h）→ Application層（1.5-2h）: シーケンシャル必須
  ↓ Application層完了後
Step5（Web層・Phase B1技術負債解消・3-4h）
  3コンポーネント並列実装可能
```

---

## 📋 1. UserProjectsテーブル依存関係

### 1.1 テーブル設計詳細

**UserProjectsテーブル構造**（既存確認済み）:
- Primary Key: UserProjectId (BIGSERIAL)
- Foreign Keys: UserId (VARCHAR(450)), ProjectId (BIGINT)
- Audit Fields: UpdatedBy, UpdatedAt
- 複合一意制約: (UserId, ProjectId)

### 1.2 Users/Projectsテーブルとの関連

**既存関連**:
- Users (AspNetUsers) ← UserProjects → Projects
- CASCADE DELETE設定済み（両外部キー）

### 1.3 EF Core Migration影響範囲

**結論**: ❌ **Migration作成不要**
- UserProjectsテーブルは Phase A で完全実装済み
- Phase B2 では Repository実装のみ必要

---

## 📋 2. Domain層・Application層影響範囲

### 2.1 Domain層（F#）

**結論**: ❌ **Domain層実装不要**（Phase B2確定）

**理由**:
- UserProject集約は多対多関連の中間テーブルのみ
- ビジネスルールはApplication層で実装
- 既存Project集約に変更なし

### 2.2 Application層（F#）

**IProjectManagementService追加メソッド（4メソッド）**:
1. `AddMemberToProjectAsync`
2. `RemoveMemberFromProjectAsync`
3. `GetProjectMembersAsync`
4. `IsUserProjectMemberAsync`

**既存メソッド修正（4メソッド）**:
1. `CreateProjectAsync`: Owner自動UserProjects追加
2. `DeleteProjectAsync`: UserProjects関連データ追加
3. `GetProjectsAsync`: DomainApprover/GeneralUser権限拡張
4. `GetProjectDetailAsync`: UserCount実装

---

## 📋 3. Infrastructure層・Web層影響範囲

### 3.1 Infrastructure層（C#）

**ProjectRepository追加メソッド（6メソッド）**:
1. `AddUserToProjectAsync`
2. `RemoveUserFromProjectAsync`
3. `GetProjectMembersAsync`
4. `IsUserProjectMemberAsync`
5. `GetProjectMemberCountAsync`
6. `SaveProjectWithDefaultDomainAndOwnerAsync`

**既存メソッド修正（2メソッド）**:
- `GetProjectsByUserAsync`
- `GetRelatedDataCountAsync`

### 3.2 Web層（C# Blazor Server）

**新規コンポーネント（3コンポーネント）**:
1. `ProjectMembers.razor`
2. `ProjectMemberSelector.razor`
3. `ProjectMemberCard.razor`

**既存コンポーネント拡張（1コンポーネント）**:
- `ProjectEdit.razor`

**Phase B1技術負債解消（4件）**:
1. InputRadioGroup制約（2件）
2. フォーム送信詳細テスト（1件）
3. Null参照警告（1件）

---

## 📋 4. Step2-5実装順序最適化

### 4.1 依存関係マッピング

```
Infrastructure層（ProjectRepository） → Application層（IProjectManagementService） → Web層（Blazor Server）

並列実行可能:
- Step2（Playwright統合）独立実施
- Step5内部（3コンポーネント並列実装）
```

### 4.2 ボトルネック・リスク要因

| リスク要因 | 影響度 | 発生確率 | 対策 |
|----------|-------|---------|-----|
| N+1問題 | 大 | 中 | Include()パターン活用 |
| CASCADE DELETE | 中 | 低 | 論理削除実装活用 |
| 複合一意制約違反 | 中 | 中 | 既存チェック実装 |
| 権限判定複雑化 | 中 | 中 | ヘルパーメソッド実装 |

### 4.3 最適実装順序（Step2-5）

**Step2: Playwright統合（1.5-2時間）**
- 独立実施可能
- E2E.Testsプロジェクト初期実装

**Step4: Application/Infrastructure層実装（3-4時間）**
- Infrastructure層実装 → Application層実装（シーケンシャル）
- TDD Green Phase達成

**Step5: Web層実装・技術負債解消（3-4時間）**
- 3コンポーネント並列実装可能
- Phase B1技術負債4件解消

---

## 📋 5. 実装推奨事項

### Step4重点事項

**Infrastructure層**:
1. SaveProjectWithDefaultDomainAndOwnerAsync実装
2. GetProjectMembersAsync実装（Eager Loading徹底）

**Application層**:
1. AddMemberToProjectAsync実装
2. CreateProjectAsync拡張

### Step5重点事項

**ProjectMembers.razor実装**:
1. 権限制御UI
2. 状態管理
3. エラーハンドリング

---

## 📊 6. Phase B2完了後の達成状態

- ✅ UserProjects多対多関連完全実装
- ✅ 権限制御マトリックス16パターン完全実装
- ✅ プロジェクトメンバー管理UI完全実装
- ✅ Phase B1技術負債4件完全解消
- ✅ Playwright E2E基盤確立
- ✅ Clean Architecture 96-97点品質維持

---

**依存関係分析完了**
**重要な決定事項**:
- Step3（Domain層拡張）スキップ確定
- UserProjectsテーブルMigration不要確定
- Phase B1トランザクションパターン拡張方針確定
