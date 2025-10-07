# Phase B-F1 組織設計・総括

## 📊 Phase概要

- **Phase名**: Phase B-F1 (Phase B - Foundation 1)
- **Phase種別**: 基盤整備Phase（Technical Foundation）
- **Phase規模**: 🟢中規模相当（1-2セッション・5Step構成）
- **Phase特性**: テストアーキテクチャ整合性確保・技術負債解消・Phase B2準備
- **実施タイミング**: Phase B1完了後・Phase B2開始前
- **推定期間**: 1-2セッション（6-8時間）
- **開始予定日**: 2025-10-08
- **完了予定日**: 2025-10-09（推定）

## 🎯 対象Issue詳細

### Issue #43: Phase A既存テストビルドエラー修正

**背景**:
- Phase B1 Step5（namespace階層化）実施時のテストファイル修正対象の判断ミス
- Phase A既存C#テストファイル（約35件）のusing文が古いnamespace記法のまま残存

**根本原因**:
```csharp
// 修正前（Step5で修正すべきだったが漏れ）
using UbiquitousLanguageManager.Domain;

// 修正後（ADR_019準拠）
using UbiquitousLanguageManager.Domain.Common;
using UbiquitousLanguageManager.Domain.Authentication;
using UbiquitousLanguageManager.Domain.ProjectManagement;
```

**影響範囲**:
- UserDomainServiceTests.cs
- AuthenticationConverterTests.cs
- AuthenticationServiceTests.cs
- NotificationServiceTests.cs
- 他約31ファイル

**推定工数**: 30-45分

### Issue #40 Phase 1-3: テストアーキテクチャ再構成

**背景**:
- Phase B1 Step7（Web層bUnit UIテスト実装）完了時点で顕在化
- 現状の問題点：
  1. テストタイプ混在（Unit/Integration同一プロジェクト）
  2. レイヤー混在（全5層が1プロジェクトに集約）
  3. 言語混在による技術負債（F#/C#混在・EnableDefaultCompileItems=false）
  4. Phase B2以降の拡張性不足（E2Eテスト追加時に更なる混在）

**ADR_020決定事項**:
- レイヤー別×テストタイプ別分離方式採用
- 7プロジェクト構成確立
- 業界標準（.NET Clean Architecture 2024）準拠

**7プロジェクト構成**:
```
tests/
├── UbiquitousLanguageManager.Domain.Unit.Tests/              # F# Domain単体テスト
├── UbiquitousLanguageManager.Application.Unit.Tests/         # F# Application単体テスト
├── UbiquitousLanguageManager.Contracts.Unit.Tests/           # C# Contracts単体テスト
├── UbiquitousLanguageManager.Infrastructure.Unit.Tests/      # C# Infrastructure単体テスト
├── UbiquitousLanguageManager.Infrastructure.Integration.Tests/ # C# 統合テスト
├── UbiquitousLanguageManager.Web.UI.Tests/                   # C# Blazor bUnit UIテスト
└── UbiquitousLanguageManager.E2E.Tests/                      # C# E2Eテスト（Phase B2で実装）
```

**推定工数**:
- Phase 1（レイヤー別単体テスト作成）: 2-3時間
- Phase 2（テストタイプ別プロジェクト作成）: 1-2時間
- Phase 3（旧プロジェクト削除・ドキュメント整備）: 1-1.5時間

## 📋 全Step実行プロセス

### Step1: 技術調査・詳細分析（1-1.5時間）

**目的**: Issue #43, #40の詳細調査・リスク分析・実装方針確定

**調査内容**:

#### 1. Issue #43詳細調査（30分）
- Phase A既存テストファイルの正確な件数・配置確認
  - `Glob("tests/UbiquitousLanguageManager.Tests/**/*.cs")` 実行
  - using文パターン検索（`Grep("using UbiquitousLanguageManager.Domain;")`）
- ビルドエラー詳細分析
  - `dotnet build tests/UbiquitousLanguageManager.Tests` 実行
  - エラーコード・行番号特定（CS0246等）
- 修正対象using文パターン特定
  - 古いnamespace使用箇所の完全リスト作成
- ADR_019準拠の正しいnamespace構造確認
  - 4 Bounded Context別using文パターン確認

#### 2. Issue #40現状分析（30分）
- 現在のテストプロジェクト構成の完全把握
  - **UbiquitousLanguageManager.Tests**: 構成・ファイル数・参照関係
  - **UbiquitousLanguageManager.Domain.Tests**: 構成・ファイル数・参照関係
  - **UbiquitousLanguageManager.Web.Tests**: 構成・ファイル数・参照関係
- 各プロジェクトの.csproj/.fsproj分析
  - 参照関係（ProjectReference）確認
  - 使用NuGetパッケージ確認
- テストファイル配置・命名規則の現状確認
  - テストファイルのレイヤー別・テストタイプ別分類
- 移行対象ファイル数の正確な把握
  - Domain層: X件
  - Application層: X件
  - Contracts層: X件
  - Infrastructure層: X件（Unit/Integration分離必要）
  - Web層: X件

#### 3. 移行リスク・影響範囲分析（15分）
- テスト実行失敗リスク評価
  - 移行時のテストコード修正範囲特定
  - 依存関係変更によるテスト失敗可能性評価
- CI/CD破損リスク評価
  - GitHub Actions設定確認
  - テストプロジェクト変更による影響範囲特定
- 依存関係エラーリスク評価
  - プロジェクト参照の段階的移行計画
- ロールバック手順確認
  - Git commitポイント設定

#### 4. 実装方針・SubAgent選定（15分）
- Step2-5の詳細手順確定
- 各StepのSubAgent選定理由明確化
- 並列実行可能性評価（Step3-4）
- チェックポイント設定
  - 各Step完了後のビルド・テスト実行確認内容

**SubAgent**:
- spec-analysis（Issue分析・仕様確認）
- dependency-analysis（依存関係分析・影響範囲特定）

**成果物**:
- `Doc/08_Organization/Active/Phase_B-F1/Step01_技術調査結果.md`
- Issue #43修正対象ファイルリスト（完全版）
- Issue #40移行計画詳細（ファイル単位・移行順序）
- リスク分析マトリックス

---

### Step2: Issue #43完全解決（45分-1時間）

**目的**: Phase A既存テストのnamespace階層化完全適用

**実装手順**:

#### 1. 修正対象ファイル特定（5分）
- Step1調査結果に基づく正確なファイルリスト確認
- 修正優先順位設定（ビルドエラー発生順）

#### 2. using文一括修正（30分）
**対象**: Phase A既存テスト約35件

**修正パターン**:
```csharp
// 削除
using UbiquitousLanguageManager.Domain;

// 追加（必要なBounded Contextのみ）
using UbiquitousLanguageManager.Domain.Common;           // CommonTypes・Permission・Role
using UbiquitousLanguageManager.Domain.Authentication;   // User・Email・UserName・Password
using UbiquitousLanguageManager.Domain.ProjectManagement; // Project・ProjectName
// UbiquitousLanguageManagement は Phase A では未使用のため追加不要
```

**修正方法**:
- unit-test Agent使用（テストコード修正専門）
- ファイル単位での段階的修正
- 各ファイル修正後の個別ビルド確認推奨

#### 3. EnableDefaultCompileItems除外設定削除（5分）
**対象ファイル**: `tests/UbiquitousLanguageManager.Tests/UbiquitousLanguageManager.Tests.csproj`

**削除内容**:
```xml
<!-- 削除対象 -->
<PropertyGroup>
  <EnableDefaultCompileItems>false</EnableDefaultCompileItems>
</PropertyGroup>

<ItemGroup Label="Phase A既存テスト一時除外">
  <Compile Remove="Application/**/*.cs" />
  <Compile Remove="Contracts/**/*.cs" />
  <Compile Remove="Domain/**/*.cs" />
  <Compile Remove="Integration/**/*.cs" />
  <Compile Remove="Web/**/*.cs" />
  <Compile Remove="Stubs/TemporaryStubs.cs" />
</ItemGroup>
```

#### 4. ビルド・テスト実行確認（5分）
```bash
# 全体ビルド確認
dotnet build

# 成功基準: 0 Warning/0 Error

# Phase Aテスト実行確認
dotnet test tests/UbiquitousLanguageManager.Tests

# 成功基準: Phase A既存テスト35件 100%成功
```

**SubAgent**: unit-test（テストコード修正専門）

**成果物**:
- Phase A既存テスト35件修正完了
- ビルド成功確認記録（0 Warning/0 Error）
- テスト実行成功確認記録（35件100%成功）
- `Doc/08_Organization/Active/Phase_B-F1/Step02_Issue43完了報告.md`

---

### Step3: Issue #40 Phase 1実装（2-3時間）

**目的**: レイヤー別単体テストプロジェクト作成

**実装内容**:

#### 1. Domain.Unit.Tests作成（F#・45分）
**プロジェクト作成**:
```bash
dotnet new xunit -lang F# -n UbiquitousLanguageManager.Domain.Unit.Tests -o tests/UbiquitousLanguageManager.Domain.Unit.Tests
```

**移行対象**:
- 既存 `UbiquitousLanguageManager.Domain.Tests` の全ファイル
- 既存 `UbiquitousLanguageManager.Tests/Domain` の全ファイル
- 重複テストファイル統合

**参照設定**:
```xml
<ItemGroup>
  <ProjectReference Include="..\..\src\UbiquitousLanguageManager.Domain\UbiquitousLanguageManager.Domain.fsproj" />
</ItemGroup>
```

**NuGetパッケージ**:
- xUnit
- FsUnit.xUnit
- Microsoft.NET.Test.Sdk

#### 2. Application.Unit.Tests作成（F#・45分）
**プロジェクト作成**:
```bash
dotnet new xunit -lang F# -n UbiquitousLanguageManager.Application.Unit.Tests -o tests/UbiquitousLanguageManager.Application.Unit.Tests
```

**移行対象**:
- 既存 `UbiquitousLanguageManager.Tests/Application` の全ファイル

**参照設定**:
```xml
<ItemGroup>
  <ProjectReference Include="..\..\src\UbiquitousLanguageManager.Application\UbiquitousLanguageManager.Application.fsproj" />
  <ProjectReference Include="..\..\src\UbiquitousLanguageManager.Domain\UbiquitousLanguageManager.Domain.fsproj" />
</ItemGroup>
```

#### 3. Contracts.Unit.Tests作成（C#・30分）
**プロジェクト作成**:
```bash
dotnet new xunit -n UbiquitousLanguageManager.Contracts.Unit.Tests -o tests/UbiquitousLanguageManager.Contracts.Unit.Tests
```

**移行対象**:
- 既存 `UbiquitousLanguageManager.Tests/Contracts` の全ファイル

**参照設定**:
```xml
<ItemGroup>
  <ProjectReference Include="..\..\src\UbiquitousLanguageManager.Contracts\UbiquitousLanguageManager.Contracts.csproj" />
  <ProjectReference Include="..\..\src\UbiquitousLanguageManager.Domain\UbiquitousLanguageManager.Domain.fsproj" />
  <ProjectReference Include="..\..\src\UbiquitousLanguageManager.Application\UbiquitousLanguageManager.Application.fsproj" />
</ItemGroup>
```

#### 4. Infrastructure.Unit.Tests作成（C#・30分）
**プロジェクト作成**:
```bash
dotnet new xunit -n UbiquitousLanguageManager.Infrastructure.Unit.Tests -o tests/UbiquitousLanguageManager.Infrastructure.Unit.Tests
```

**移行対象**:
- 既存 `UbiquitousLanguageManager.Tests/Infrastructure` から**単体テストのみ**移行
- 統合テスト（DB接続・WebApplicationFactory使用）はPhase 2で対応

**参照設定**:
```xml
<ItemGroup>
  <ProjectReference Include="..\..\src\UbiquitousLanguageManager.Infrastructure\UbiquitousLanguageManager.Infrastructure.csproj" />
</ItemGroup>
```

#### 5. ソリューションファイル更新（10分）
```bash
dotnet sln add tests/UbiquitousLanguageManager.Domain.Unit.Tests
dotnet sln add tests/UbiquitousLanguageManager.Application.Unit.Tests
dotnet sln add tests/UbiquitousLanguageManager.Contracts.Unit.Tests
dotnet sln add tests/UbiquitousLanguageManager.Infrastructure.Unit.Tests
```

#### 6. 全テスト実行確認（10分）
```bash
# 新規プロジェクト個別実行
dotnet test tests/UbiquitousLanguageManager.Domain.Unit.Tests
dotnet test tests/UbiquitousLanguageManager.Application.Unit.Tests
dotnet test tests/UbiquitousLanguageManager.Contracts.Unit.Tests
dotnet test tests/UbiquitousLanguageManager.Infrastructure.Unit.Tests

# 全体実行
dotnet test
```

**SubAgent**:
- unit-test（F#/C# 単体テストプロジェクト作成）
- integration-test（プロジェクト構成確認・参照関係検証）

**並列実行可能性**: unit-test + integration-test 並列実行可

**成果物**:
- 4つの新規テストプロジェクト作成完了
- 全テスト実行成功確認（Phase A + Phase B1 + 新規4プロジェクト）
- `Doc/08_Organization/Active/Phase_B-F1/Step03_Phase1完了報告.md`

---

### Step4: Issue #40 Phase 2実装（1-2時間）

**目的**: テストタイプ別プロジェクト作成・リネーム

**実装内容**:

#### 1. Infrastructure.Integration.Tests作成（C#・45分）
**プロジェクト作成**:
```bash
dotnet new xunit -n UbiquitousLanguageManager.Infrastructure.Integration.Tests -o tests/UbiquitousLanguageManager.Infrastructure.Integration.Tests
```

**移行対象**:
- 既存 `UbiquitousLanguageManager.Tests/Integration` の全ファイル
- WebApplicationFactory使用テスト
- DB接続テスト

**参照設定**:
```xml
<ItemGroup>
  <ProjectReference Include="..\..\src\UbiquitousLanguageManager.Infrastructure\UbiquitousLanguageManager.Infrastructure.csproj" />
  <ProjectReference Include="..\..\src\UbiquitousLanguageManager.Application\UbiquitousLanguageManager.Application.fsproj" />
  <ProjectReference Include="..\..\src\UbiquitousLanguageManager.Domain\UbiquitousLanguageManager.Domain.fsproj" />
  <ProjectReference Include="..\..\src\UbiquitousLanguageManager.Web\UbiquitousLanguageManager.Web.csproj" />
</ItemGroup>
```

**NuGetパッケージ**:
- Microsoft.AspNetCore.Mvc.Testing（WebApplicationFactory）
- Microsoft.EntityFrameworkCore.InMemory
- Testcontainers.PostgreSql

#### 2. Web.UI.Testsリネーム（C#・30分）
**リネーム**:
```bash
# プロジェクトディレクトリ名変更
mv tests/UbiquitousLanguageManager.Web.Tests tests/UbiquitousLanguageManager.Web.UI.Tests

# .csprojファイル名変更
mv tests/UbiquitousLanguageManager.Web.UI.Tests/UbiquitousLanguageManager.Web.Tests.csproj tests/UbiquitousLanguageManager.Web.UI.Tests/UbiquitousLanguageManager.Web.UI.Tests.csproj
```

**namespace更新**:
- テストファイル内の`namespace UbiquitousLanguageManager.Web.Tests`を`namespace UbiquitousLanguageManager.Web.UI.Tests`に変更

#### 3. CI/CDパイプライン更新（15分）
**対象ファイル**: `.github/workflows/*.yml`（該当ファイルがあれば）

**更新内容**:
```yaml
# 新規テストプロジェクト追加
- name: Run Tests
  run: |
    dotnet test tests/UbiquitousLanguageManager.Domain.Unit.Tests
    dotnet test tests/UbiquitousLanguageManager.Application.Unit.Tests
    dotnet test tests/UbiquitousLanguageManager.Contracts.Unit.Tests
    dotnet test tests/UbiquitousLanguageManager.Infrastructure.Unit.Tests
    dotnet test tests/UbiquitousLanguageManager.Infrastructure.Integration.Tests
    dotnet test tests/UbiquitousLanguageManager.Web.UI.Tests
```

#### 4. ソリューションファイル更新（5分）
```bash
dotnet sln add tests/UbiquitousLanguageManager.Infrastructure.Integration.Tests
dotnet sln remove tests/UbiquitousLanguageManager.Web.Tests
dotnet sln add tests/UbiquitousLanguageManager.Web.UI.Tests
```

#### 5. 全テスト実行確認（10分）
```bash
dotnet test
```

**SubAgent**:
- integration-test（統合テストプロジェクト作成）
- csharp-web-ui（Web UIテストプロジェクトリネーム）

**並列実行可能性**: integration-test + csharp-web-ui 並列実行可

**成果物**:
- 2つのテストプロジェクト整理完了
- CI/CD設定更新完了（該当ファイルがあれば）
- 全テスト実行成功確認
- `Doc/08_Organization/Active/Phase_B-F1/Step04_Phase2完了報告.md`

---

### Step5: Issue #40 Phase 3実装・ドキュメント整備（1-1.5時間）

**目的**: 旧プロジェクト削除・ドキュメント整備・Phase完了

**実装内容**:

#### 1. 旧プロジェクト削除（15分）
**削除対象**:
```bash
# プロジェクトディレクトリ削除
rm -rf tests/UbiquitousLanguageManager.Domain.Tests
rm -rf tests/UbiquitousLanguageManager.Tests

# ソリューションファイルから削除
dotnet sln remove tests/UbiquitousLanguageManager.Domain.Tests
dotnet sln remove tests/UbiquitousLanguageManager.Tests
```

#### 2. 最終確認（15分）
**ビルド確認**:
```bash
dotnet build

# 成功基準: 0 Warning/0 Error
```

**全テスト実行確認**:
```bash
dotnet test --verbosity normal

# 成功基準: Phase A + Phase B1 全テスト100%成功
```

**テストカバレッジ確認**:
```bash
dotnet test --collect:"XPlat Code Coverage"

# 成功基準: カバレッジ95%以上維持
```

#### 3. ドキュメント整備（30-45分）

##### 3.1 テストアーキテクチャ設計書作成（15分）
**ファイル**: `/Doc/02_Design/テストアーキテクチャ設計書.md`

**記載内容**:
- プロジェクト構成図（Mermaid図）
- 命名規則: `{ProjectName}.{Layer}.{TestType}.Tests`
- 参照関係原則（Unit/Integration/E2E別）
- 配置ルール・判断基準
- ADR_020参照

##### 3.2 新規テストプロジェクト作成ガイドライン作成（15分）
**ファイル**: `/Doc/08_Organization/Rules/新規テストプロジェクト作成ガイドライン.md`

**記載内容**:
```markdown
## 新規テストプロジェクト作成チェックリスト

### 事前確認
- [ ] テストアーキテクチャ設計書を確認
- [ ] 既存テストプロジェクトとの重複確認
- [ ] レイヤー・テストタイプの分類明確化

### プロジェクト作成
- [ ] 命名規則準拠: `{ProjectName}.{Layer}.{TestType}.Tests`
- [ ] 言語選択: F# (Domain/Application) / C# (その他)
- [ ] SDK選択: Sdk="Microsoft.NET.Sdk" / "Microsoft.NET.Sdk.Razor"

### 参照関係設定
- [ ] テスト対象レイヤーのみ参照（Unitテスト）
- [ ] 必要な依存層のみ参照（Integrationテスト）
- [ ] 不要な参照の削除確認

### ビルド・実行確認
- [ ] `dotnet build` 成功
- [ ] `dotnet test` 成功
- [ ] ソリューションファイル (.sln) 更新

### ドキュメント更新
- [ ] テストアーキテクチャ設計書に追記
- [ ] README.md のテスト実行手順更新
```

##### 3.3 組織管理運用マニュアル更新（10分）
**ファイル**: `/Doc/08_Organization/Rules/組織管理運用マニュアル.md`

**追加内容**:
```markdown
### Step完了時チェックリスト（追加項目）

#### テストアーキテクチャ整合性確認
- [ ] 新規テストプロジェクト作成時、設計書との整合性確認
- [ ] テストプロジェクト命名規則準拠確認
- [ ] 不要な参照関係の追加がないか確認
- [ ] EnableDefaultCompileItems等の技術負債が増加していないか確認

### Phase完了時チェックリスト（追加項目）

#### テストアーキテクチャレビュー
- [ ] テストアーキテクチャ設計書の最新性確認
- [ ] 全テストプロジェクトの構成妥当性確認
- [ ] Phase中に発生した技術負債の記録・Issue化
- [ ] 次Phase向けテストアーキテクチャ改善提案
```

**SubAgent**:
- spec-compliance（品質確認・成功基準達成確認）
- design-review（ドキュメントレビュー・整合性確認）

**成果物**:
- 旧プロジェクト削除完了
- 3つの新規ドキュメント作成完了
- 全テスト実行成功確認記録
- テストカバレッジ95%維持確認記録
- `Doc/08_Organization/Active/Phase_B-F1/Step05_Phase3完了報告.md`
- `Doc/08_Organization/Completed/Phase_B-F1/Phase_Summary.md`（Phase完了総括・Active→Completedへ移動）

---

## 🎯 Phase B-F1成功基準

### 必須達成項目

#### 1. Issue #43完全解決
- ✅ Phase A既存テスト35件修正完了（ADR_019準拠のnamespace階層化適用）
- ✅ EnableDefaultCompileItems除外設定削除完了
- ✅ 全テストビルド成功（0 Warning/0 Error）
- ✅ Phase Aテスト実行成功率100%

#### 2. Issue #40 Phase 1-3完全実装
- ✅ 7プロジェクト構成確立（ADR_020準拠）
  - Domain.Unit.Tests（F#）
  - Application.Unit.Tests（F#）
  - Contracts.Unit.Tests（C#）
  - Infrastructure.Unit.Tests（C#）
  - Infrastructure.Integration.Tests（C#）
  - Web.UI.Tests（C#）
  - E2E.Tests準備（Phase B2で実施）
- ✅ 旧テストプロジェクト削除完了（Domain.Tests / Tests）
- ✅ CI/CD設定更新完了（該当ファイルがあれば）

#### 3. 品質維持
- ✅ 全テスト実行成功（Phase A + Phase B1 + 新規6プロジェクト）
- ✅ テストカバレッジ95%以上維持
- ✅ 0 Warning/0 Error維持
- ✅ ビルド時間増加なし（テストプロジェクト分離による影響なし）

#### 4. ドキュメント整備
- ✅ テストアーキテクチャ設計書作成完了
- ✅ 新規テストプロジェクト作成ガイドライン作成完了
- ✅ 組織管理運用マニュアル更新完了（Phase/Step完了チェックリスト追加）

### 期待効果

#### 短期効果（Phase B-F1完了時）
- **テスト実行効率30%向上**: レイヤー別・テストタイプ別実行による時間短縮
- **技術負債解消**: F#/C#混在問題・EnableDefaultCompileItems削除
- **業界標準準拠**: .NET Clean Architecture 2024ベストプラクティス準拠

#### 中長期効果（Phase B2以降）
- **Phase B2最適基盤確立**: E2E.Tests追加の自然な拡張
- **CI/CD最適化**: レイヤー別・テストタイプ別実行・並列実行
- **保守性向上**: テスト失敗時の影響範囲即座特定・メンテナンス容易性向上

---

## 📚 関連ドキュメント・ADR参照リスト

### 必読ADR
- **ADR_019**: namespace設計規約（Phase B1 Step5作成・Phase A既存テスト修正の根拠）
- **ADR_020**: テストアーキテクチャ決定（Phase B1完了時作成・Issue #40の根拠）

### 関連設計書
- `/Doc/02_Design/データベース設計書.md`: 統合テストのDB接続仕様確認用
- `/Doc/08_Organization/Completed/Phase_B1/Phase_Summary.md`: Phase B1成果・申し送り事項

### GitHub Issues
- **Issue #43**: Phase A既存テストビルドエラー修正（本Phase対象）
- **Issue #40**: テストアーキテクチャ全面見直し（Phase 1-3を本Phaseで実施・Phase 4はPhase B2）

### プロセス管理文書
- `/Doc/08_Organization/Rules/縦方向スライス実装マスタープラン.md`: Phase B-F1記載確認
- `/Doc/08_Organization/Rules/組織管理運用マニュアル.md`: Step/Phase完了チェックリスト

---

## 🚨 リスク管理マトリックス

| リスク | 影響度 | 発生確率 | 対策 | ロールバックプラン |
|-------|-------|---------|------|-----------------|
| テスト実行失敗 | 高 | 中 | 各Step完了後に全テスト実行・カバレッジ維持確認 | Step単位でのgit revert |
| CI/CD破損 | 高 | 低 | Step4完了後に統合的更新・段階的確認 | CI/CD設定のgit revert |
| 依存関係エラー | 中 | 中 | プロジェクト参照の段階的移行・ビルド確認徹底 | 参照関係のみgit revert |
| 移行漏れ | 中 | 低 | Step1詳細調査・移行前後のテスト件数比較 | 旧プロジェクト一時復元 |
| テストカバレッジ低下 | 中 | 低 | Step5カバレッジ測定・95%維持確認 | 不足テストの追加実装 |

### ロールバック実行基準
以下のいずれかに該当する場合、即座にロールバック実行：
- 全テスト成功率が95%未満に低下
- ビルドエラーが10分以上解決不可
- テストカバレッジが90%未満に低下
- CI/CDパイプラインが30分以上復旧不可

---

## 🔄 次回セッション開始時の必須確認事項

### 1. プロジェクト状況確認（5分）
```bash
# 現在のブランチ・コミット状況確認
git status
git log -5 --oneline

# ビルド健全性確認
dotnet build

# 成功基準: 0 Warning/0 Error（Phase B1完了時状態維持）
```

### 2. Phase B-F1開始準備確認（5分）
- [ ] 本Phase_Summary.md内容確認
- [ ] マスタープランにPhase B-F1記載確認
- [ ] Active/Phase_B-F1ディレクトリ存在確認

### 3. Step1実施開始（ユーザー承認必須）
- [ ] Step1: 技術調査・詳細分析実施開始の最終承認
- [ ] 推定時間1-1.5時間の確認
- [ ] SubAgent選定確認（spec-analysis + dependency-analysis）

### 4. TodoList初期化
Step1開始時に以下のTodoList作成推奨：
```markdown
- [ ] Step1: 技術調査・詳細分析（1-1.5時間）
  - [ ] Issue #43詳細調査（30分）
  - [ ] Issue #40現状分析（30分）
  - [ ] 移行リスク・影響範囲分析（15分）
  - [ ] 実装方針・SubAgent選定（15分）
- [ ] Step2: Issue #43完全解決（45分-1時間）
- [ ] Step3: Issue #40 Phase 1実装（2-3時間）
- [ ] Step4: Issue #40 Phase 2実装（1-2時間）
- [ ] Step5: Issue #40 Phase 3実装・ドキュメント整備（1-1.5時間）
```

---

## 📊 Phase総括レポート（Phase完了時記録）

*Phase完了時に更新予定*

### 実績記録
- 実施期間:
- 実施セッション数:
- 実施時間:

### 品質達成状況
- Issue #43解決状況:
- Issue #40 Phase 1-3完了状況:
- 全テスト実行成功率:
- テストカバレッジ:

### 技術的成果
- 確立した技術パターン:
- 解消した技術負債:
- Phase B2への申し送り事項:

### プロセス改善
- SubAgent活用実績:
- 効率化達成度:
- 継続改善提案:

---

**Phase作成日**: 2025-10-08
**Phase開始承認**: 取得済み
**次回アクション**: Step1技術調査・詳細分析実施
**Phase責任者**: Claude Code
**Phase監督**: プロジェクトオーナー
