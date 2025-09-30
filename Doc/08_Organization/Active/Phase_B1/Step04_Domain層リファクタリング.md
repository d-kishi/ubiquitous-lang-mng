# Step04 Domain層リファクタリング - 組織設計・実装記録

## 📋 Step概要
- **Step名**: Step04 Domain層リファクタリング（**新規追加**）
- **作業特性**: リファクタリング・品質改善（Phase C/D準備）
- **推定期間**: 1セッション（3-4時間）
- **実施予定日**: 2025-09-30以降
- **SubAgent組み合わせ**: fsharp-domain単独実行・リファクタリング特化

## 🎯 Step目的・成果目標
- **Bounded Context別ディレクトリ分離**: 認証・プロジェクト管理・共通定義の明確化
- **Phase C/D成長予測対応**: 1,000行超ファイル問題の事前回避
- **可読性・保守性向上**: 初学者適切な構造（100-200行/ファイル）
- **並列開発基盤確立**: SubAgent競合リスク低減

## 🚨 実施タイミングの重要性

### なぜ今実施するのか？
1. **Infrastructure層実装前が最適**
   - Domain層変更の影響範囲が最小（Application層のみ）
   - Infrastructure層・Web層実装後のリファクタリングは工数3-5倍増加

2. **TDD基盤確立状態**
   - 52テスト100%成功状態で品質保証可能
   - リファクタリング後の動作確認が即座可能

3. **Phase C/D前準備**
   - Phase C: +530行（ValueObjects 150行・Entities 200行・DomainServices 180行）
   - Phase D: +950行（ValueObjects 250行・Entities 400行・DomainServices 300行）
   - 現在実施で将来リスク回避

### 実施しない場合のリスク
- **Phase C実装時**: 500-700行ファイルでの作業（可読性低下）
- **Phase D実装時**: 1,000行超ファイルでの作業（保守性重大問題）
- **リファクタリング工数**: 3-4時間 → 10-15時間（3-5倍増加）
- **テスト修正範囲**: 52テスト → 100+テスト（Phase C/D追加後）

## 🏢 組織設計

### SubAgent構成（fsharp-domain単独実行）
- **fsharp-domain**: Bounded Context別ディレクトリ分離・ファイル移行・コンパイル順序調整

### 単独実行理由
- リファクタリングは単一責務（Domain層のみ変更）
- 他層への影響なし（namespace変更なし・後方互換性維持）
- 段階的実装により品質確保容易

## 📚 Step1・Step3成果物活用（必須参照）

### 🔴 実装前必須確認事項

#### 1. リファクタリング調査結果確認
**参照ファイル**: `/Doc/08_Organization/Active/Phase_B1/Domain層リファクタリング調査結果.md`

**必須確認セクション**:
- **現状分析**: Domain層4ファイル1,289行の詳細構成
- **Phase C/D成長予測**: ValueObjects 754行・Entities 1,145行・DomainServices 770行予測
- **リスク評価**: 可読性・保守性・F#コンパイル順序制約リスク
- **推奨構造**: Bounded Context別ディレクトリ分離計画

#### 2. GitHub Issue #41確認
**参照**: GitHub Issue #41 - Domain層リファクタリング提案

**必須確認セクション**:
- **実装計画**: 5フェーズ実装（合計3-4時間）
- **品質保証計画**: 0 Warning/0 Error維持・52テスト100%成功継続
- **後方互換性**: namespace変更なし・Application層影響最小化

#### 3. Step3完了確認
**参照ファイル**: `/Doc/08_Organization/Active/Phase_B1/Step03_Application.md`

**必須確認事項**:
- **Domain層・Application層実装完了**: Infrastructure層未実装（影響範囲最小化）
- **TDD基盤確立**: 52テスト100%成功状態（品質保証可能）
- **仕様準拠度100点満点達成**: Step3で史上最高品質達成済み

## 🎯 Step成功基準

### リファクタリング完了基準
- [ ] **Bounded Context別ディレクトリ構造完成**: Common/Authentication/ProjectManagement
- [ ] **全ファイル分割完了**: ValueObjects/Entities/DomainServices/Errors（各境界文脈別）
- [ ] **.fsprojコンパイル順序調整完了**: Common → Authentication → ProjectManagement順
- [ ] **0 Warning/0 Error維持**: 全プロジェクトビルド成功
- [ ] **52テスト100%成功継続**: TDD基盤維持・品質保証確認

### 品質基準（必須）
- [ ] **Clean Architecture 97点維持**: 循環依存なし・層責務分離遵守
- [ ] **後方互換性維持**: namespace変更なし・Application層影響なし
- [ ] **Phase C/D準備完了**: 最適構造での実装開始可能状態

## 🔧 5フェーズ実装計画

### Phase 1: ディレクトリ・ファイル作成（30分）

#### 作業内容
1. **ディレクトリ構造作成**
```
src/UbiquitousLanguageManager.Domain/
├── Common/
├── Authentication/
└── ProjectManagement/
```

2. **空ファイル作成**
```
Common/
├── CommonTypes.fs
├── CommonValueObjects.fs
└── CommonSpecifications.fs

Authentication/
├── AuthenticationValueObjects.fs
├── AuthenticationEntities.fs
├── AuthenticationErrors.fs
└── UserDomainService.fs

ProjectManagement/
├── ProjectValueObjects.fs
├── ProjectEntities.fs
├── ProjectErrors.fs
└── ProjectDomainService.fs
```

#### 完了確認
- [ ] 3ディレクトリ作成完了
- [ ] 12空ファイル作成完了
- [ ] ディレクトリ構造確認

---

### Phase 2: Common層移行（45分）

#### 作業内容
1. **CommonTypes.fs移行**
   - 既存ValueObjects.fsから抽出:
     - UserId・ProjectId・DomainId・UbiquitousLanguageId
     - Permission・Role・ProjectPermission

2. **CommonValueObjects.fs移行**
   - 既存ValueObjects.fsから抽出:
     - Description（共通説明型）
     - 共通バリデーション関数

3. **CommonSpecifications.fs移行**
   - 既存Specifications.fsの内容を全て移行
   - 仕様パターン実装

#### 完了確認
- [ ] CommonTypes.fs移行完了（型定義・Smart Constructor）
- [ ] CommonValueObjects.fs移行完了
- [ ] CommonSpecifications.fs移行完了
- [ ] `dotnet build` 成功確認

---

### Phase 3: Authentication層移行（60分）

#### 作業内容
1. **AuthenticationValueObjects.fs移行**
   - 既存ValueObjects.fsから抽出:
     - Email・StrongEmail・UserName
     - Password・PasswordHash・SecurityStamp

2. **AuthenticationEntities.fs移行**
   - 既存Entities.fsから抽出:
     - User（200+行）
     - UserProfile

3. **AuthenticationErrors.fs移行**
   - 既存Entities.fs・DomainServices.fsから抽出:
     - AuthenticationError判別共用体

4. **UserDomainService.fs移行**
   - 既存DomainServices.fsから抽出:
     - UserDomainService全体

#### 完了確認
- [ ] AuthenticationValueObjects.fs移行完了
- [ ] AuthenticationEntities.fs移行完了（User 200+行）
- [ ] AuthenticationErrors.fs移行完了
- [ ] UserDomainService.fs移行完了
- [ ] `dotnet build` 成功・`dotnet test` 成功確認

---

### Phase 4: ProjectManagement層移行（45分）

#### 作業内容
1. **ProjectValueObjects.fs移行**
   - 既存ValueObjects.fsから抽出:
     - ProjectName・ProjectDescription
     - DomainName（ドメイン関連も含む）

2. **ProjectEntities.fs移行**
   - 既存Entities.fsから抽出:
     - Project
     - Domain
     - DraftUbiquitousLanguage
     - FormalUbiquitousLanguage

3. **ProjectErrors.fs移行**
   - 既存Entities.fs・DomainServices.fsから抽出:
     - ProjectCreationError判別共用体

4. **ProjectDomainService.fs移行**
   - 既存DomainServices.fsから抽出:
     - DomainService（汎用）
     - ProjectDomainService

#### 完了確認
- [ ] ProjectValueObjects.fs移行完了
- [ ] ProjectEntities.fs移行完了（Project・Domain・DraftUbiquitousLanguage・FormalUbiquitousLanguage）
- [ ] ProjectErrors.fs移行完了
- [ ] ProjectDomainService.fs移行完了
- [ ] `dotnet build` 成功・`dotnet test` 成功確認

---

### Phase 5: 品質保証・検証（30分）

#### 作業内容
1. **.fsprojファイルコンパイル順序調整**
```xml
<ItemGroup>
  <!-- Common: 共通定義（最優先） -->
  <Compile Include="Common/CommonTypes.fs" />
  <Compile Include="Common/CommonValueObjects.fs" />
  <Compile Include="Common/CommonSpecifications.fs" />

  <!-- Authentication: 認証境界文脈 -->
  <Compile Include="Authentication/AuthenticationValueObjects.fs" />
  <Compile Include="Authentication/AuthenticationErrors.fs" />
  <Compile Include="Authentication/AuthenticationEntities.fs" />
  <Compile Include="Authentication/UserDomainService.fs" />

  <!-- ProjectManagement: プロジェクト管理境界文脈 -->
  <Compile Include="ProjectManagement/ProjectValueObjects.fs" />
  <Compile Include="ProjectManagement/ProjectErrors.fs" />
  <Compile Include="ProjectManagement/ProjectEntities.fs" />
  <Compile Include="ProjectManagement/ProjectDomainService.fs" />
</ItemGroup>
```

2. **全プロジェクトビルド・テスト実行**
   - `dotnet build`（0 Warning/0 Error確認）
   - `dotnet test`（52テスト100%成功確認）

3. **Application層・Contracts層参照確認**
   - Application層からのDomain層参照動作確認
   - Contracts層からのDomain層型変換動作確認

4. **既存ファイル削除（オプション）**
   - ValueObjects.fs削除（バックアップ推奨）
   - Entities.fs削除（バックアップ推奨）
   - DomainServices.fs削除（バックアップ推奨）
   - Specifications.fs削除 or 空ファイル化

#### 完了確認
- [ ] .fsprojコンパイル順序調整完了
- [ ] `dotnet build` 成功（0 Warning, 0 Error）
- [ ] `dotnet test` 成功（52テスト100%成功）
- [ ] Application層・Contracts層参照動作確認
- [ ] 既存ファイル削除 or バックアップ完了

---

## 📊 実装対象詳細

### 移行前後のファイル構成

#### 移行前（現在）
```
src/UbiquitousLanguageManager.Domain/
├── ValueObjects.fs (354行) - 全境界文脈混在
├── Entities.fs (545行) - 全境界文脈混在
├── DomainServices.fs (290行) - 全境界文脈混在
└── Specifications.fs (100行) - 仕様パターン
```

#### 移行後（目標）
```
src/UbiquitousLanguageManager.Domain/
├── Common/
│   ├── CommonTypes.fs (~100行) - ID型・Permission・Role
│   ├── CommonValueObjects.fs (~50行) - Description等
│   └── CommonSpecifications.fs (~100行) - 仕様パターン
├── Authentication/
│   ├── AuthenticationValueObjects.fs (~150行) - Email・Password等
│   ├── AuthenticationEntities.fs (~250行) - User (200+行)
│   ├── AuthenticationErrors.fs (~20行) - AuthenticationError
│   └── UserDomainService.fs (~100行) - UserDomainService
└── ProjectManagement/
    ├── ProjectValueObjects.fs (~100行) - ProjectName・DomainName
    ├── ProjectEntities.fs (~250行) - Project・Domain・UbiquitousLanguage
    ├── ProjectErrors.fs (~20行) - ProjectCreationError
    └── ProjectDomainService.fs (~170行) - ProjectDomainService・DomainService
```

### 行数削減効果
- **最大ファイル**: 545行 → 250行（54%削減）
- **平均ファイル**: 322行 → 123行（62%削減）
- **ファイル数**: 4ファイル → 12ファイル（3倍分散）

## 📋 品質保証計画

### ビルド品質
- **0 Warning/0 Error維持**: 全プロジェクトビルド成功
- **F#コンパイル順序**: Common → Authentication → ProjectManagement順厳守
- **Clean Architecture 97点維持**: 循環依存なし・層責務分離遵守

### テスト品質
- **52テスト100%成功継続**: Domain層32テスト・Application層20テスト
- **TDD基盤維持**: Red-Green-Refactorサイクル継続可能状態
- **回帰テストなし**: 既存機能の動作変更なし

### 後方互換性
- **namespace変更なし**: `UbiquitousLanguageManager.Domain`統一維持
- **Application層影響なし**: IProjectManagementService実装変更不要
- **Contracts層影響なし**: TypeConverter実装変更不要

## 🔄 次Step準備状況

### Step5（Infrastructure層実装）準備
- **Domain層最適構造確立**: Bounded Context明確化
- **Repository統合準備**: 最適化されたDomain層との統合容易
- **EF Core Configurations**: 境界文脈別Configuration実装準備

### Phase C/D準備
- **Phase C追加時**: DomainManagement/配下に追加（100-200行/ファイル維持）
- **Phase D追加時**: LanguageManagement/配下に追加（100-200行/ファイル維持）
- **成長予測対応**: 1,000行超ファイル問題の事前回避完了

## ⚠️ 重要な注意事項

### F#コンパイル順序制約
- **前方参照不可**: F#は宣言順序依存（後方の型を参照不可）
- **依存関係順**: Common → Authentication → ProjectManagement順厳守
- **循環参照禁止**: Bounded Context間の循環参照は設計エラー

### 段階的実装必須
1. **Phase 1完了後**: ディレクトリ・ファイル構造確認
2. **Phase 2完了後**: Common層動作確認・ビルド成功
3. **Phase 3完了後**: Authentication層動作確認・テスト成功
4. **Phase 4完了後**: ProjectManagement層動作確認・テスト成功
5. **Phase 5完了後**: 全体統合・品質保証完了

### テスト確認タイミング
- **Phase 2-4各完了後**: `dotnet build`・`dotnet test`実行必須
- **Phase 5完了後**: 最終統合テスト・E2E動作確認
- **失敗時**: 前Phaseへのロールバック検討

## 📈 期待効果

### 短期効果（Phase B1完了時）
- **可読性向上**: 単一ファイル100-200行・境界文脈明確分離
- **保守性向上**: 並列開発容易・影響範囲特定容易
- **品質維持**: 0 Warning/0 Error・52テスト100%成功継続

### 長期効果（Phase C/D実装時）
- **Phase C実装効率**: 最適構造での実装開始・並列開発可能
- **Phase D実装効率**: 成長予測対応完了・リスク事前回避
- **技術基盤発展**: Bounded Context明確化・F#コンパイル順序最適化

---

**Step作成日**: 2025-09-30
**実施予定日**: 2025-09-30以降（Step3完了後即座実行可能）
**推定時間**: 3-4時間（5フェーズ実装）
**SubAgent**: fsharp-domain単独実行・リファクタリング特化