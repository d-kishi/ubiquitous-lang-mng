# Step06 Infrastructure層実装 - 組織設計・実行記録

## 📋 Step概要
- **Step名**: Step06 Infrastructure層実装（**Phase B1最終実装Step**）
- **作業特性**: Infrastructure層実装・Repository・EF Core・権限制御統合
- **推定期間**: 1セッション（4-5時間）
- **実施日**: 2025-10-02（予定）
- **SubAgent組み合わせ**: csharp-infrastructure + fsharp-application + unit-test + integration-test（並列実行）
- **実施ステータス**: 🚀 **準備完了・実施開始待機**

## 🎯 Step目的・成果目標
- **ProjectRepository完全実装**: CRUD操作・権限フィルタリング・原子性保証
- **EF Core統合完成**: Configuration・Migration・トランザクション管理
- **Application層統合**: IProjectManagementService実装完成・F#↔C#境界統合
- **Phase B1完成**: プロジェクト管理機能完全実装・最高品質達成

## 🏢 組織設計

### SubAgent構成（Pattern A適用・並列実行）
- **csharp-infrastructure**: ProjectRepository・EF Core Configuration実装（中心）
- **fsharp-application**: Application層統合・IProjectManagementService実装連携
- **unit-test**: Repository単体テスト・TDD Red/Green実践
- **integration-test**: EF Core統合テスト・トランザクションテスト・E2Eテスト

### 並列実行計画
**Stage 1-2（設計・TDD Red）**: csharp-infrastructure + unit-test 並列実行
**Stage 3（TDD Green）**: csharp-infrastructure + fsharp-application + unit-test 並列実行
**Stage 4-5（品質確認）**: integration-test + spec-compliance 並列実行

### 並列実行理由
- 各SubAgent独立作業可能（Repository設計・テスト作成・Application層統合）
- 時間効率最大化（順次実行5時間 → 並列実行3.5-4時間）
- 専門性活用（Infrastructure・Application・Test各専門SubAgent最適作業）

## 📚 前提条件・必須参照

### 🔴 実装前必須確認事項

#### 1. Step5完了確認 ✅
**参照ファイル**: `/Doc/08_Organization/Active/Phase_B1/Step05_namespace階層化.md`

**✅ Step5完了事項（2025-10-01）**:
- ✅ **42ファイル修正完了**: Domain 15・Application 12・Contracts 7・Infrastructure 4・Web 2・Tests 2
- ✅ **namespace階層化100%達成**: 4境界文脈完全実装（Common/Authentication/ProjectManagement/UbiquitousLanguageManagement）
- ✅ **ADR_019作成**: namespace設計規約明文化（247行・業界標準準拠・再発防止策確立）
- ✅ **0 Warning/0 Error達成**: 全層ビルド成功・32テスト100%成功
- ✅ **Infrastructure層実装準備完了**: namespace基盤完成・即座実装可能状態

#### 2. Step1成果物必須参照（📊 Step間成果物参照マトリックスより）

##### 📄 Technical_Research_Results.md
**参照セクション**:
- **セクション2**: デフォルトドメイン自動作成の技術手法
  - EF Core BeginTransaction実装パターン（行176-236）
  - 原子性保証の具体的実装（行238-279）
  - トランザクションスコープ活用方法
- **セクション4**: EF Core多対多関連の最適実装
  - UserProjects中間テーブル設計
  - ProjectRole管理実装パターン

**活用目的**:
- ProjectRepository実装の技術的指針
- 原子性保証・失敗時ロールバック実装
- EF Core Configuration設計

##### 📄 Design_Review_Results.md（確認：ファイル不在・Step1で未作成）
**代替参照**: Phase A完了時の既存システム設計資料
- `src/UbiquitousLanguageManager.Infrastructure/` 既存実装パターン
- ASP.NET Core Identity統合実装（AuthenticationRepository等）

#### 3. 現在のビルド・テスト状況確認 ✅
**確認コマンド実行済み**:
- ✅ `dotnet build`: 0 Warning/0 Error（全5プロジェクト成功）
- ✅ `dotnet test`: テスト実行正常（Phase A基盤テスト継続成功）

#### 4. GitHub Issue確認 ✅
**確認結果**: phase-B1ラベル未解決Issue 0件
- GitHub Issue #41（Domain層リファクタリング）: ✅ クローズ済み（2025-09-30）
- GitHub Issue #42（namespace階層化）: ✅ クローズ済み（2025-10-01）
- **新規Issue作成不要**: 標準Step実装・技術負債なし

### 🎯 Step5からの申し送り事項

#### 完了事項（Infrastructure層実装準備完了）
1. **namespace整合性100%達成**
   - 全層namespace階層化完了・Bounded Context明確化
   - Infrastructure層: 既に `UbiquitousLanguageManager.Infrastructure.*` 使用中
   - 新規実装時はnamespace規約（ADR_019）完全準拠

2. **ADR_019 namespace設計規約確立**
   - Infrastructure層namespace: `UbiquitousLanguageManager.Infrastructure.<Feature>`
   - Repository: `UbiquitousLanguageManager.Infrastructure.Repositories`
   - Configurations: `UbiquitousLanguageManager.Infrastructure.Configurations`

3. **Domain/Application層基盤完成**
   - ProjectDomainService: Railway-oriented Programming完全実装
   - IProjectManagementService: Command/Query分離・権限制御統合
   - ApplicationDtos: F#↔C#境界最適化完了

#### Infrastructure層実装時の重要制約
1. **namespace規約遵守**: ADR_019準拠・階層構造統一
2. **using文整理**: Bounded Context別に明示的記載
3. **F#型参照**: 完全修飾名使用推奨（型衝突回避）

## 🎯 Step成功基準

### Infrastructure層実装完了基準
- [ ] **ProjectRepository完全実装**: CRUD操作・権限フィルタリング・原子性保証
- [ ] **EF Core Configuration実装**: ProjectEntityConfiguration・DomainEntityConfiguration
- [ ] **Migration作成**: AddProjectManagementTables Migration適用成功
- [ ] **Application層統合**: IProjectManagementService実装完成・Repository統合
- [ ] **TDD実践**: Repository単体テスト32件追加・100%成功
- [ ] **統合テスト**: EF Core統合テスト・トランザクションテスト成功

### 品質基準（必須）
- [ ] **0 Warning/0 Error維持**: 全プロジェクトビルド成功
- [ ] **全テスト100%成功**: Domain32 + Application20 + Repository32 = 84テスト成功
- [ ] **Clean Architecture 97点維持**: 循環依存なし・層責務分離遵守
- [ ] **仕様準拠度100点維持**: 原子性保証・権限制御・否定的仕様完全遵守

## 🔧 5段階実装計画（Stage構成）

### Stage 1: Repository設計・EF Core Configuration設計（60分）

#### SubAgent: csharp-infrastructure
**作業内容**:
1. **ProjectRepository設計**
   - IProjectRepository インターフェース設計
   - CRUD操作メソッド定義
   - 権限フィルタリングメソッド定義
   - 原子性保証メソッド定義

2. **EF Core Configuration設計**
   - ProjectEntityConfiguration設計
   - DomainEntityConfiguration設計
   - UserProjects中間テーブル設計

3. **設計レビュー**
   - Technical_Research_Results.md参照確認
   - Clean Architecture整合性確認
   - namespace規約（ADR_019）準拠確認

**参照**:
- `/Doc/08_Organization/Active/Phase_B1/Research/Technical_Research_Results.md`（行176-279）
- `/Doc/07_Decisions/ADR_019_namespace設計規約.md`

**成果物**:
- `src/UbiquitousLanguageManager.Infrastructure/Repositories/IProjectRepository.cs`（設計案）
- `src/UbiquitousLanguageManager.Infrastructure/Configurations/ProjectEntityConfiguration.cs`（設計案）

### Stage 2: TDD Red（Repository単体テスト作成）（60分）

#### SubAgent: unit-test + csharp-infrastructure（並列実行）
**作業内容**:
1. **Repository単体テスト作成**（unit-test）
   - ProjectRepositoryTests.cs作成
   - CRUD操作テスト作成（8件）
   - 権限フィルタリングテスト作成（8件）
   - 原子性保証テスト作成（8件）
   - トランザクションロールバックテスト作成（8件）

2. **テスト実行・失敗確認**（unit-test）
   - `dotnet test` 実行
   - 32件テスト失敗確認（TDD Red Phase達成）

**参照**:
- `/Doc/08_Organization/Active/Phase_B1/Step02_Domain.md`（TDD Red実践事例）

**成果物**:
- `tests/UbiquitousLanguageManager.Infrastructure.Tests/Repositories/ProjectRepositoryTests.cs`（32テスト）

### Stage 3: TDD Green（Repository実装・Application層統合）（90分）

#### SubAgent: csharp-infrastructure + fsharp-application + unit-test（並列実行）
**作業内容**:
1. **ProjectRepository実装**（csharp-infrastructure）
   - CRUD操作実装
   - BeginTransaction・Commit・Rollback実装
   - 権限フィルタリング実装
   - 原子性保証実装

2. **EF Core Configuration実装**（csharp-infrastructure）
   - ProjectEntityConfiguration実装
   - DomainEntityConfiguration実装
   - UserProjects中間テーブル設定

3. **Application層統合**（fsharp-application）
   - IProjectManagementService実装修正
   - Repository DI統合
   - F#↔C#境界TypeConverter確認

4. **テスト実行・成功確認**（unit-test）
   - `dotnet test` 実行
   - 84テスト（Domain32 + Application20 + Repository32）100%成功確認（TDD Green Phase達成）

**参照**:
- `/Doc/08_Organization/Active/Phase_B1/Research/Technical_Research_Results.md`（行176-279）
- `/Doc/08_Organization/Active/Phase_B1/Step03_Application.md`（Application層統合事例）

**成果物**:
- `src/UbiquitousLanguageManager.Infrastructure/Repositories/ProjectRepository.cs`（完全実装）
- `src/UbiquitousLanguageManager.Infrastructure/Configurations/*.cs`（完全実装）
- Migration: `AddProjectManagementTables`

### Stage 4: 品質チェック＆統合テスト（60分）

#### SubAgent: integration-test + spec-compliance（並列実行）
**作業内容**:
1. **統合テスト実装**（integration-test）
   - WebApplicationFactory統合テスト
   - E2Eテスト（プロジェクト作成・取得・更新・削除）
   - トランザクション統合テスト
   - 権限制御統合テスト

2. **仕様準拠確認**（spec-compliance）
   - 機能仕様書3.1章完全準拠確認
   - 原子性保証実装確認
   - 権限制御マトリックス（4ロール×4機能）完全実装確認
   - 否定的仕様完全遵守確認

**成果物**:
- `tests/UbiquitousLanguageManager.Integration.Tests/ProjectManagementIntegrationTests.cs`
- 仕様準拠度評価レポート（100点満点目標）

### Stage 5: 統合確認・0 Warning/0 Error確認（30分）

#### SubAgent: csharp-infrastructure（単独実行）
**作業内容**:
1. **全ビルド確認**
   - `dotnet build` 実行
   - 0 Warning/0 Error確認

2. **全テスト確認**
   - `dotnet test` 実行
   - 全テスト100%成功確認

3. **Migration適用確認**
   - `dotnet ef database update` 実行
   - データベーススキーマ確認

4. **Clean Architecture品質確認**
   - 循環依存なし確認
   - 層責務分離遵守確認
   - namespace規約準拠確認

**成果物**:
- Phase B1 Step6完了確認レポート
- Infrastructure層実装完了・Phase B1完成宣言

## 📊 推定時間・実施計画

### Stage別推定時間
- **Stage 1**: Repository設計・EF Core Configuration設計（60分）
- **Stage 2**: TDD Red（60分）
- **Stage 3**: TDD Green（90分）
- **Stage 4**: 品質チェック＆統合テスト（60分）
- **Stage 5**: 統合確認（30分）

**合計**: 約300分（5時間）

### 効率化要因
- Step1技術調査成果活用（実装パターン明確）
- Step5 namespace整合性完了（型参照問題なし）
- Domain/Application層基盤完成（統合設計明確）
- SubAgent並列実行（時間効率30%向上）

## 🎯 技術的前提条件

### 確立済み技術基盤（継承活用）
- **F# Domain層**: ProjectDomainService・Railway-oriented Programming完全実装
- **F# Application層**: IProjectManagementService・Command/Query分離・権限制御統合
- **Contracts層**: ApplicationDtos・TypeConverter拡張完了
- **namespace整合性**: ADR_019準拠・4境界文脈完全分離

### Infrastructure層実装の技術方針
1. **EF Core BeginTransaction活用**: 原子性保証・失敗時ロールバック
2. **権限フィルタリング**: Repository層での多重チェック実装
3. **UserProjects中間テーブル**: 多対多関連最適実装
4. **F#↔C#境界**: TypeConverter活用・型安全性確保

## 📋 SubAgent実行指示準備

### csharp-infrastructure Agent指示（準備完了）
```markdown
**作業内容**: Phase B1 Step6 Infrastructure層実装

**必須参照**:
- `/Doc/08_Organization/Active/Phase_B1/Research/Technical_Research_Results.md`（行176-279）
- `/Doc/07_Decisions/ADR_019_namespace設計規約.md`

**実装対象**:
1. ProjectRepository完全実装（CRUD・権限フィルタ・原子性保証）
2. EF Core Configuration実装
3. Migration作成・適用

**重要制約**:
- namespace規約（ADR_019）完全準拠
- Railway-oriented Programming統合
- 0 Warning/0 Error維持
```

### fsharp-application Agent指示（準備完了）
```markdown
**作業内容**: Phase B1 Step6 Application層Repository統合

**実装対象**:
1. IProjectManagementService実装修正
2. ProjectRepository DI統合
3. F#↔C#境界TypeConverter確認

**重要制約**:
- IProjectManagementService既存実装保持
- Railway-oriented Programming継続適用
- 権限制御マトリックス完全維持
```

### unit-test Agent指示（準備完了）
```markdown
**作業内容**: Phase B1 Step6 Repository単体テスト作成

**実装対象**:
1. ProjectRepositoryTests.cs作成（32テスト）
2. TDD Red Phase達成（失敗確認）
3. TDD Green Phase達成（成功確認）

**テスト種別**:
- CRUD操作テスト（8件）
- 権限フィルタリングテスト（8件）
- 原子性保証テスト（8件）
- トランザクションロールバックテスト（8件）
```

### integration-test Agent指示（準備完了）
```markdown
**作業内容**: Phase B1 Step6 統合テスト作成

**実装対象**:
1. WebApplicationFactory統合テスト
2. E2Eテスト（プロジェクト管理機能完全動作）
3. トランザクション統合テスト
4. 権限制御統合テスト

**重要確認**:
- 機能仕様書3.1章完全準拠
- 原子性保証動作確認
- 権限制御マトリックス完全動作
```

## ✅ Step開始前チェックリスト

### プロセス遵守確認（ADR_016）
- [x] **Step情報収集完了**: Phase状況・Step5完了・Step6内容確認完了
- [x] **Step1成果物参照準備完了**: Technical_Research_Results.md確認完了
- [x] **SubAgent選択完了**: Pattern A適用・4SubAgent選定完了
- [x] **並列実行計画策定完了**: Stage別並列実行計画明確化完了
- [x] **Step06組織設計記録作成完了**: 本ファイル作成完了
- [ ] **ユーザー承認取得**: Step6開始・SubAgent並列実行の最終承認（**実施待機**）

### 技術的前提条件確認
- [x] **ビルド状況**: 0 Warning/0 Error（確認済み）
- [x] **テスト状況**: 既存テスト100%成功（確認済み）
- [x] **namespace整合性**: Step5完了・ADR_019準拠（確認済み）
- [x] **GitHub Issue**: phase-B1未解決Issue 0件（確認済み）

### 必須参照文書確認
- [x] **Technical_Research_Results.md**: EF Core実装パターン確認完了
- [x] **Step05完了記録**: namespace階層化完了確認完了
- [x] **ADR_019**: namespace設計規約確認完了
- [x] **組織管理運用マニュアル**: Step開始プロセス確認完了

## 📊 Step実行記録（随時更新）

### 2025-10-02 Step開始準備完了
- ✅ Step情報収集・確認完了
- ✅ Step1成果物参照準備完了
- ✅ SubAgent選択・並列実行計画策定完了
- ✅ Step06組織設計記録ファイル作成完了
- 🚀 **ユーザー承認待機**: Step6実施開始承認待ち

---

## ✅ Step終了時レビュー（Step完了時に更新）

**Phase B1 Step6完了後に記載**:
- Infrastructure層実装完了確認
- 全テスト成功確認
- 仕様準拠度評価
- Phase B1完成宣言
- 次Phase（Phase B2 or Phase C）移行準備

---

**最終更新**: 2025-10-02（Step開始準備完了・ユーザー承認待機）
**次のアクション**: ユーザー承認取得後、SubAgent並列実行開始
