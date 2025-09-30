# Step05 Domain層namespace階層化 - 組織設計・実装記録

## 📋 Step概要
- **Step名**: Step05 Domain層namespace階層化（**新規追加・GitHub Issue #42**）
- **作業特性**: アーキテクチャ整合性確保・F#ベストプラクティス準拠・再発防止策
- **推定期間**: 1セッション（3.5-4.5時間）
- **実施予定日**: Step4完了後即座実施
- **SubAgent組み合わせ**: fsharp-domain + fsharp-application + contracts-bridge + csharp-infrastructure並列実行

## 🎯 Step目的・成果目標
- **Application層との整合性確保**: Application層は既にサブnamespace使用・Domain層も階層化
- **F#ベストプラクティス準拠**: Bounded Context別namespace分離推奨パターン適用
- **Bounded Context明確化の効果最大化**: ディレクトリ構造とnamespace構造の一致
- **Phase C/D拡張性向上**: 最適なnamespace構造での実装開始

## 🚨 実施タイミングの重要性

### なぜ今実施するのか？
1. **Step4完了後が最適**
   - Domain層Bounded Context別ディレクトリ分離完了
   - ディレクトリ構造とnamespace構造を一致させる最適タイミング

2. **Application層不整合の解消**
   - Application層: `UbiquitousLanguageManager.Application.ProjectManagement`使用中
   - Domain層: `UbiquitousLanguageManager.Domain`のみ（フラット）
   - 不整合状態の早期解消

3. **Infrastructure層実装前が最適**
   - Infrastructure層未実装状態での修正が最も影響範囲小
   - Infrastructure層実装後の修正は工数1.5-2倍増加

### 実施しない場合のリスク
- アーキテクチャ不整合の継続
- F#ベストプラクティス不準拠の継続
- Phase C/D実装時のリファクタリング再発
- 将来的なnamespace階層化工数増大（3-4時間 → 6-8時間）

## 🏢 組織設計

### SubAgent構成（4SubAgent並列実行）
- **fsharp-domain**: Domain層namespace変更（12ファイル）
- **fsharp-application**: Application層open文修正（5-8ファイル）
- **contracts-bridge**: Contracts層using文修正（3-5ファイル）
- **csharp-infrastructure**: Infrastructure層open文修正（10-15ファイル）

### 並列実行理由
- 各SubAgent独立作業可能（Domain層namespace確定後）
- 時間効率最大化（順次実行の場合4時間 → 並列実行で2.5-3時間）
- 専門性活用（各層の専門SubAgentが最適修正実施）

## 📚 前提条件・必須参照

### 🔴 実装前必須確認事項

#### 1. GitHub Issue #42確認
**参照**: GitHub Issue #42 - Domain層namespace階層化対応

**必須確認セクション**:
- **問題の背景**: Application層サブnamespace使用中・Domain層フラット
- **実装フェーズ**: 6フェーズ実装計画（合計3-4時間）
- **影響範囲**: 40-50ファイル修正
- **品質保証計画**: 0 Warning/0 Error維持・52テスト100%成功継続

#### 2. Step4完了確認 ✅
**参照ファイル**: `/Doc/08_Organization/Active/Phase_B1/Step04_Domain層リファクタリング.md`

**✅ Step4完了事項（2025-09-30）**:
- ✅ **4境界文脈分離完了**: Common/Authentication/ProjectManagement/**UbiquitousLanguageManagement**
- ✅ **16ファイル分割完了**: ValueObjects/Entities/DomainServices/Errors×4境界文脈
- ✅ **.fsprojコンパイル順序確認**: Common → Authentication → ProjectManagement → UbiquitousLanguageManagement順
- ✅ **Phase 6追加実施**: UbiquitousLanguageManagement境界文脈分離完了（当初計画外の改善）
- ✅ **型安全性向上**: UbiquitousLanguageError型新規作成（93行）

**🔴 Step5での重要な変更点**:
- **namespace階層化対象ファイル数**: 12ファイル→**16ファイル**（UbiquitousLanguageManagement追加）
- **UbiquitousLanguageErrors.fs**: 新規作成ファイル（Step4で追加）
- **4境界文脈すべて**: namespace階層化対象

#### 3. Application層namespace構造確認
**確認コマンド**: `grep "^namespace" src/UbiquitousLanguageManager.Application/**/*.fs`

**確認ポイント**:
- `UbiquitousLanguageManager.Application` （ルート）
- `UbiquitousLanguageManager.Application.ProjectManagement` （サブ）
- `UbiquitousLanguageManager.Application.Interfaces` （サブ）

### 🎯 Step4からの申し送り事項

#### 完了事項
1. **Bounded Context完全分離達成**
   - Common/Authentication/ProjectManagement/UbiquitousLanguageManagement
   - 合計2,631行・16ファイル・4境界文脈

2. **Phase 6追加実施による品質向上**
   - UbiquitousLanguageManagement境界文脈の事前分離
   - Step5実施時の整合性確保
   - 「雛型の名残」問題の解消

3. **namespace階層化の前提条件完全達成**
   - ディレクトリ構造とnamespace構造の一致準備完了
   - F#コンパイル順序の最適化完了

#### 未完了事項（Step5で実施）
1. **namespace階層化**: すべて `UbiquitousLanguageManager.Domain` のまま
2. **Application層open文**: まだフラットnamespace参照
3. **Contracts層using文**: まだフラットnamespace参照
4. **Infrastructure層**: まだフラットnamespace参照

#### 既存問題（別Issue化予定）
- **テストプロジェクト**: `.csproj`なのにF#ファイル（`.fs`）を含む
- **影響**: テスト実行不可（C#コンパイラでF#コードを解析してエラー）
- **Step4との関連**: 無関係（既存の構造問題）

## 🎯 Step成功基準

### namespace階層化完了基準
- [ ] **Domain層サブnamespace導入完了**: `.Domain.Common`, `.Domain.Authentication`, `.Domain.ProjectManagement`
- [ ] **Application層open文修正完了**: 5-8ファイル修正
- [ ] **Contracts層using文修正完了**: 3-5ファイル修正
- [ ] **Infrastructure層open文修正完了**: 10-15ファイル修正
- [ ] **テストコード修正完了**: 6-8ファイル修正
- [ ] **0 Warning/0 Error維持**: 全プロジェクトビルド成功
- [ ] **52テスト100%成功継続**: TDD基盤維持・品質保証確認

### 品質基準（必須）
- [ ] **Application層整合性確保**: Domain層もサブnamespace使用・階層構造統一
- [ ] **F#ベストプラクティス準拠**: Bounded Context別namespace分離パターン適用
- [ ] **Clean Architecture 97点維持**: 循環依存なし・層責務分離遵守

## 🔧 7フェーズ実装計画

### Phase 1: Domain層namespace変更（60分）

#### 作業内容
1. **Common層namespace変更**（3ファイル）
```fsharp
// 変更前
namespace UbiquitousLanguageManager.Domain

// 変更後
namespace UbiquitousLanguageManager.Domain.Common
```
- CommonTypes.fs
- CommonValueObjects.fs
- CommonSpecifications.fs

2. **Authentication層namespace変更**（4ファイル）
```fsharp
namespace UbiquitousLanguageManager.Domain.Authentication
```
- AuthenticationValueObjects.fs
- AuthenticationEntities.fs
- AuthenticationErrors.fs
- UserDomainService.fs

3. **ProjectManagement層namespace変更**（4ファイル）
```fsharp
namespace UbiquitousLanguageManager.Domain.ProjectManagement
```
- ProjectValueObjects.fs
- ProjectEntities.fs
- ProjectErrors.fs
- ProjectDomainService.fs

4. **.fsproj確認**（変更不要）
   - コンパイル順序は維持（Common → Authentication → ProjectManagement）
   - namespace変更のみでファイル順序変更なし

#### 完了確認
- [ ] 12ファイルnamespace変更完了
- [ ] .fsprojコンパイル順序確認
- [ ] `dotnet build src/UbiquitousLanguageManager.Domain` 成功確認

---

### Phase 2: Application層修正（30分）

#### 作業内容
1. **ProjectManagement配下修正**（5ファイル）
```fsharp
// 変更前
open UbiquitousLanguageManager.Domain

// 変更後
open UbiquitousLanguageManager.Domain.Common
open UbiquitousLanguageManager.Domain.ProjectManagement
```
- ProjectManagementService.fs
- IProjectManagementService.fs
- Commands.fs
- Queries.fs

2. **AuthenticationServices.fs修正**
```fsharp
open UbiquitousLanguageManager.Domain.Common
open UbiquitousLanguageManager.Domain.Authentication
```

3. **ApplicationServices.fs等確認**
   - 使用している型に応じてopen文追加

#### 完了確認
- [ ] 5-8ファイル修正完了
- [ ] `dotnet build src/UbiquitousLanguageManager.Application` 成功確認

---

### Phase 3: Contracts層修正（20分）

#### 作業内容
1. **TypeConverters.cs修正**
```csharp
// 変更前
using UbiquitousLanguageManager.Domain;

// 変更後
using UbiquitousLanguageManager.Domain.Common;
using UbiquitousLanguageManager.Domain.Authentication;
using UbiquitousLanguageManager.Domain.ProjectManagement;
```

2. **ApplicationDtos.cs・DTOs.cs確認**
   - 使用している型に応じてusing文追加

#### 完了確認
- [ ] 3-5ファイル修正完了
- [ ] `dotnet build src/UbiquitousLanguageManager.Contracts` 成功確認

---

### Phase 4: Infrastructure層修正（40分）

#### 作業内容
1. **Repository実装修正**（10-15ファイル）
```csharp
// ProjectRepository.cs等
using UbiquitousLanguageManager.Domain.Common;
using UbiquitousLanguageManager.Domain.ProjectManagement;

// UserRepository.cs等
using UbiquitousLanguageManager.Domain.Common;
using UbiquitousLanguageManager.Domain.Authentication;
```

2. **EF Core Configurations修正**
   - ProjectConfiguration.cs等
   - UserConfiguration.cs等

#### 完了確認
- [ ] 10-15ファイル修正完了
- [ ] `dotnet build src/UbiquitousLanguageManager.Infrastructure` 成功確認

---

### Phase 5: テストコード修正（30分）

#### 作業内容
1. **Domain.Tests修正**（3ファイル）
```fsharp
// 変更前
open UbiquitousLanguageManager.Domain

// 変更後
open UbiquitousLanguageManager.Domain.Common
open UbiquitousLanguageManager.Domain.ProjectManagement
```
- ProjectTests.fs
- ProjectDomainServiceTests.fs
- ProjectErrorHandlingTests.fs

2. **Application.Tests修正**（2-3ファイル）
```fsharp
open UbiquitousLanguageManager.Domain.Common
open UbiquitousLanguageManager.Domain.ProjectManagement
```
- ProjectManagementServiceTests.fs等

3. **Infrastructure.Tests修正**（1-2ファイル）

#### 完了確認
- [ ] 6-8ファイル修正完了
- [ ] `dotnet test` 成功（52テスト100%成功）

---

### Phase 6: 統合ビルド・テスト検証（30分）

#### 作業内容
1. **全プロジェクトビルド**
   - `dotnet build`（0 Warning/0 Error確認）
   - 全プロジェクト同時ビルド成功確認

2. **全テスト実行**
   - `dotnet test`（52テスト100%成功確認）
   - TDD基盤維持・品質保証確認

3. **Clean Architecture整合性確認**
   - Application層との整合性確保確認
   - Bounded Context別namespace分離確認
   - F#ベストプラクティス準拠確認

4. **最終確認**
   - namespace階層構造の確認
   - 循環依存なし確認
   - 層責務分離遵守確認

#### 完了確認
- [ ] `dotnet build` 成功（0 Warning, 0 Error）
- [ ] `dotnet test` 成功（52テスト100%成功）
- [ ] Application層整合性確保完了
- [ ] F#ベストプラクティス準拠確認

---

### Phase 7: 完了処理・再発防止策（ADR作成）（40-55分）

#### 🆕 作業内容

##### 1. ADR_019作成: namespace設計規約（40-55分）

**目的**: namespace規約不在が今回の問題根本原因・再発防止のための明文化

**作成内容**:
```markdown
# ADR_019: namespace設計規約（Bounded Context別サブnamespace規約）

**ステータス**: 承認済み
**決定日**: 2025-09-30
**決定者**: プロジェクトオーナー

## 背景・課題

### 発生した問題
- Application層: `UbiquitousLanguageManager.Application.ProjectManagement`（サブnamespace使用）
- Domain層: `UbiquitousLanguageManager.Domain`（フラットnamespace）
- アーキテクチャ不整合発生・Step5（namespace階層化）対応必要

### 根本原因
- ADR_010に「レイヤー構造を反映した階層化」記載あり
- Bounded Context別サブnamespace使用の明示的ルールなし
- 実装例あるも規約として文書化されず
- namespace構造妥当性チェックプロセスなし

## 決定事項

### 1. Bounded Context別サブnamespace必須化

#### 基本テンプレート
```
<ProjectName>.<Layer>.<BoundedContext>[.<Feature>]
```

#### 具体的namespace規約

**Domain層**:
```fsharp
namespace UbiquitousLanguageManager.Domain.Common          // 共通定義
namespace UbiquitousLanguageManager.Domain.Authentication  // 認証境界文脈
namespace UbiquitousLanguageManager.Domain.ProjectManagement  // プロジェクト管理境界文脈
```

**Application層**:
```fsharp
namespace UbiquitousLanguageManager.Application.ProjectManagement
namespace UbiquitousLanguageManager.Application.Interfaces
```

**Infrastructure層**:
```csharp
namespace UbiquitousLanguageManager.Infrastructure.Data
namespace UbiquitousLanguageManager.Infrastructure.Repositories
namespace UbiquitousLanguageManager.Infrastructure.Identity
```

**Contracts層**:
```csharp
namespace UbiquitousLanguageManager.Contracts.DTOs
namespace UbiquitousLanguageManager.Contracts.Converters
namespace UbiquitousLanguageManager.Contracts.Interfaces
```

### 2. 階層構造ルール

#### Common特別扱い
- **Common**: 全Bounded Contextで使用する共通定義
- **配置**: 各層のルート直下または`.Common`サブnamespace
- **依存関係**: 他のBounded Contextに依存しない

#### Bounded Context別分離
- **Authentication**: ユーザー・認証・権限管理
- **ProjectManagement**: プロジェクト管理
- **DomainManagement**: ドメイン管理（Phase C実装予定）
- **LanguageManagement**: ユビキタス言語管理（Phase D実装予定）

#### 最大階層制限
- **推奨**: 3階層まで（`<Project>.<Layer>.<BoundedContext>`）
- **許容**: 4階層（`<Project>.<Layer>.<BoundedContext>.<Feature>`）
- **例**: `UbiquitousLanguageManager.Domain.ProjectManagement.Specifications`

### 3. F#特別考慮事項

#### Module設計との関係
- **Module = Bounded Context推奨だが強制しない**
- **保守性優先**: 500行超は複数ファイル・moduleに分割
- **namespace + moduleの組み合わせ活用**

**推奨パターン**:
```fsharp
namespace UbiquitousLanguageManager.Domain.ProjectManagement

// 型定義
type ProjectId = ProjectId of Guid
type ProjectName = ...

// ドメインサービス
module ProjectDomainService =
    let validateProjectName = ...
    let createProject = ...
```

#### F#コンパイル順序考慮
- `.fsproj`でのファイル順序重要（前方参照不可）
- Bounded Context内でも依存関係順に配置
- Common → Authentication → ProjectManagement順推奨

### 4. C#特別考慮事項

#### using文推奨パターン
```csharp
// Repository実装
using UbiquitousLanguageManager.Domain.Common;
using UbiquitousLanguageManager.Domain.ProjectManagement;

// TypeConverter実装
using DomainCommon = UbiquitousLanguageManager.Domain.Common;
using DomainProject = UbiquitousLanguageManager.Domain.ProjectManagement;
```

### 5. 検証プロセス

#### Step開始時検証
- [ ] namespace構造レビュー実施
- [ ] Bounded Context境界確認
- [ ] 循環依存なし確認

#### Phase完了時検証
- [ ] 全層namespace整合性確認
- [ ] ADR_019規約準拠確認
- [ ] F#/C#ベストプラクティス準拠確認

## 技術的根拠

### 業界標準実践（2024年調査）

#### F# namespace規約
- **出典**: "Domain Modeling Made Functional", F# for fun and profit
- **推奨**: Bounded Context別namespace分離
- **実用**: 保守性優先・namespace + module組み合わせ

#### C# namespace規約
- **出典**: Microsoft Learn, Clean Architecture実践
- **推奨**: `<Company>.<Product>.<Layer>.<BoundedContext>`
- **理由**: エンティティ名衝突回避・依存関係制御・境界明確化

### Clean Architecture準拠
- **層責務分離**: namespace階層でレイヤー・境界文脈明確化
- **依存関係原則**: namespace構造で依存方向制御
- **テスタビリティ**: Bounded Context単位でのテスト容易性向上

## 実装影響

### 既存コードへの影響
- **Phase B1 Step5**: namespace階層化で対応（3.5-4.5時間）
- **将来Phase**: 本ADR準拠でnamespace設計

### 開発効率への影響
- **初期コスト**: namespace設計時間（Phase設計時10-15分）
- **長期メリット**: コード探索容易性・保守性向上・Phase C/D拡張性確保

## 関連文書

- **ADR_010**: 実装規約（Line 74「レイヤー構造を反映した階層化」基本方針）
- **ADR_012**: 階層構造統一ルール（ドキュメント階層構造・本ADRはコード構造）
- **Step05_namespace階層化.md**: 本ADR適用の実装記録
- **GitHub Issue #42**: namespace階層化対応Issue

## レビュー履歴

| 日付 | レビュー者 | 結果 | コメント |
|------|------------|------|----------|
| 2025-09-30 | プロジェクトオーナー | 承認 | namespace規約明文化により再発防止 |

---

**承認者**: プロジェクトオーナー
**承認日**: 2025-09-30
**有効期間**: プロジェクト実装フェーズ全体
```

##### 2. ADR_010更新: namespace規約参照追加（5分）

**更新箇所**: `/Doc/07_Decisions/ADR_010_実装規約.md` Line 74

**変更前**:
```markdown
- **名前空間**: レイヤー構造を反映した階層化
```

**変更後**:
```markdown
- **名前空間**: レイヤー構造を反映した階層化（詳細: ADR_019参照）
```

##### 3. 関連ドキュメント更新（5分）

**更新対象**:
- `Phase_Summary.md`: Step5完了成果にADR_019追加記載
- `Step間依存関係マトリックス.md`: Step5完了判定にADR作成追加

#### 完了確認
- [ ] ADR_019作成完了（markdown形式・承認済みステータス）
- [ ] ADR_010更新完了（Line 74にADR_019参照追加）
- [ ] 関連ドキュメント更新完了（2文書）
- [ ] namespace規約明文化完了（再発防止策確立）

---

## 📊 実装対象詳細

### 修正ファイル一覧

#### Domain層（12ファイル）
```
src/UbiquitousLanguageManager.Domain/
├── Common/ (3ファイル)
│   ├── CommonTypes.fs
│   ├── CommonValueObjects.fs
│   └── CommonSpecifications.fs
├── Authentication/ (4ファイル)
│   ├── AuthenticationValueObjects.fs
│   ├── AuthenticationEntities.fs
│   ├── AuthenticationErrors.fs
│   └── UserDomainService.fs
└── ProjectManagement/ (4ファイル)
    ├── ProjectValueObjects.fs
    ├── ProjectEntities.fs
    ├── ProjectErrors.fs
    └── ProjectDomainService.fs
```

#### Application層（5-8ファイル）
```
src/UbiquitousLanguageManager.Application/
├── ProjectManagement/ (5ファイル)
│   ├── ProjectManagementService.fs
│   ├── IProjectManagementService.fs
│   ├── Commands.fs
│   └── Queries.fs
├── AuthenticationServices.fs
└── ApplicationServices.fs等
```

#### Contracts層（3-5ファイル）
```
src/UbiquitousLanguageManager.Contracts/
├── Converters/
│   └── TypeConverters.cs
├── DTOs/
│   ├── ApplicationDtos.cs
│   └── DTOs.cs等
```

#### Infrastructure層（10-15ファイル）
```
src/UbiquitousLanguageManager.Infrastructure/
├── Repositories/
│   ├── ProjectRepository.cs
│   ├── UserRepository.cs
│   └── 等
└── Data/Configurations/
    ├── ProjectConfiguration.cs
    ├── UserConfiguration.cs
    └── 等
```

#### Tests（6-8ファイル）
```
tests/
├── UbiquitousLanguageManager.Domain.Tests/ (3ファイル)
│   ├── ProjectTests.fs
│   ├── ProjectDomainServiceTests.fs
│   └── ProjectErrorHandlingTests.fs
└── UbiquitousLanguageManager.Tests/
    ├── Application/ (2-3ファイル)
    └── Infrastructure/ (1-2ファイル)
```

### 修正パターン例

#### F#ファイル修正パターン
```fsharp
// Domain層
namespace UbiquitousLanguageManager.Domain.ProjectManagement
// ↑ namespace行のみ変更

// Application層・Tests
open UbiquitousLanguageManager.Domain.Common
open UbiquitousLanguageManager.Domain.ProjectManagement
// ↑ open文を必要な分追加
```

#### C#ファイル修正パターン
```csharp
// Contracts層・Infrastructure層
using UbiquitousLanguageManager.Domain.Common;
using UbiquitousLanguageManager.Domain.Authentication;
using UbiquitousLanguageManager.Domain.ProjectManagement;
// ↑ using文を必要な分追加
```

## 📋 品質保証計画

### ビルド品質
- **0 Warning/0 Error維持**: 全プロジェクトビルド成功
- **namespace階層化完了**: Domain層サブnamespace導入完了
- **Clean Architecture 97点維持**: 循環依存なし・層責務分離遵守

### テスト品質
- **52テスト100%成功継続**: Domain層32テスト・Application層20テスト
- **TDD基盤維持**: Red-Green-Refactorサイクル継続可能状態
- **回帰テストなし**: 既存機能の動作変更なし

### アーキテクチャ品質
- **Application層整合性**: Domain層もサブnamespace使用・階層構造統一
- **F#ベストプラクティス準拠**: Bounded Context別namespace分離
- **ディレクトリ・namespace一致**: Step4ディレクトリ構造とnamespace構造の一致

## 🔄 次Step準備状況

### Step6（Infrastructure層実装）準備
- **namespace整合性確立**: Domain層サブnamespace完成・参照整合性確保
- **Repository統合準備**: 最適化されたnamespace構造での実装開始
- **EF Core Configurations**: Bounded Context別Configuration実装準備

### Phase C/D準備
- **Phase C追加時**: DomainManagement/配下に`UbiquitousLanguageManager.Domain.DomainManagement`追加
- **Phase D追加時**: LanguageManagement/配下に`UbiquitousLanguageManager.Domain.LanguageManagement`追加
- **拡張性確保**: 最適なnamespace構造での実装開始

## ⚠️ 重要な注意事項

### SubAgent並列実行必須
- **fsharp-domain**: Domain層namespace変更完了後、他SubAgent開始
- **並列実行**: Application・Contracts・Infrastructure・Tests同時修正
- **時間効率**: 順次実行4時間 → 並列実行2.5-3時間

### 段階的実装必須
1. **Phase 1完了後**: Domain層ビルド成功確認
2. **Phase 2-4完了後**: 各層個別ビルド成功確認
3. **Phase 5完了後**: 全テスト成功確認
4. **Phase 6完了後**: 全体統合・品質保証完了
5. **Phase 7完了後**: ADR_019作成・再発防止策確立

### テスト確認タイミング
- **Phase 5完了後**: `dotnet test`実行必須（52テスト100%成功確認）
- **Phase 6完了後**: 最終統合テスト・E2E動作確認
- **失敗時**: 前Phaseへのロールバック検討

## 📈 期待効果

### 短期効果（Step5完了時）
- Application層との整合性確保
- F#ベストプラクティス準拠
- Bounded Context明確化の効果最大化
- **namespace規約明文化**（ADR_019作成）

### 長期効果（Phase C/D実装時）
- Phase C/D実装時の拡張性向上
- 並列開発効率向上（Bounded Context別namespace明確化）
- 保守性・可読性向上
- **再発防止策確立**（同様問題の未然防止）

---

**Step作成日**: 2025-09-30
**最終更新日**: 2025-09-30（Phase 7: ADR作成追加）
**実施予定日**: Step4完了後即座実施
**推定時間**: 3.5-4.5時間（7フェーズ実装・ADR作成含む）
**SubAgent**: fsharp-domain + fsharp-application + contracts-bridge + csharp-infrastructure並列実行
**GitHub Issue**: #42