# Phase B-F1 Step2 組織設計・実行記録

**作成日**: 2025-10-09
**Step名**: Step2 - Issue #43完全解決
**作業特性**: 修正・技術負債解消
**推定期間**: 45分-1時間

---

## 📋 Step概要

### Step目的
Phase A既存テスト17件のnamespace階層化適用・EnableDefaultCompileItems技術負債解消

### 対象Issue
- **Issue #43**: Phase A既存テストビルドエラー修正（namespace階層化漏れ対応）

### 成功基準
- ✅ 17件全修正完了（ADR_019準拠のnamespace階層化適用）
- ✅ EnableDefaultCompileItems除外設定削除完了
- ✅ ビルド成功（0 Warning/0 Error）
- ✅ Phase Aテスト実行成功率100%
- ✅ git commit作成（"Phase B-F1 Step2: Issue #43完全解決"）

---

## 🏢 組織設計

### SubAgent構成
- **unit-test**: テストコード修正専門
  - 役割: using文一括修正・.csproj修正
  - 責務: Phase A既存テスト17件の修正・技術負債解消

### 実施戦略
- **段階的修正**: パターン別に段階的修正（Authentication→Common→複数境界文脈）
- **個別ビルド確認**: 各ファイル修正後の個別ビルド確認推奨
- **最終統合確認**: 全修正完了後のビルド・テスト実行

---

## 🎯 Step1成果物活用

### 必須参照文書
- **Step01_技術調査結果.md**: 修正対象17件の完全リスト・3パターン確立
- **Spec_Analysis_Issue43_40.md**: 詳細な修正対象ファイル一覧（Line 22-42）
- **次回セッション準備.md**: 実施手順詳細・ロールバック手順
- **ADR_019**: namespace設計規約の根拠確認

### 活用内容
- 修正対象ファイル17件の完全リスト活用
- 3パターン修正方法の適用
- ADR_019準拠のnamespace構造確認

---

## 📊 実施タスク（TodoList）

### 1. using文一括修正（30分・17件・3パターン）

#### パターン1: Authentication境界文脈中心（12件）
```csharp
// 削除
using UbiquitousLanguageManager.Domain;

// 追加
using UbiquitousLanguageManager.Domain.Common;
using UbiquitousLanguageManager.Domain.Authentication;
```

**対象ファイル**:
1. Domain/UserDomainServiceTests.cs
2. Domain/UserProfileValueObjectTests.cs
3. Domain/PasswordValueObjectTests.cs
4. Contracts/AuthenticationConverterTests.cs
5. Contracts/AuthenticationMapperTests.cs
6. Infrastructure/AuthenticationServiceTests.cs
7. Infrastructure/AuthenticationServiceAutoLoginTests.cs
8. Infrastructure/AuthenticationServicePasswordResetTests.cs
9. Infrastructure/RememberMeFunctionalityTests.cs
10. Integration/AutoLoginIntegrationTests.cs
11. Integration/FSharpAuthenticationIntegrationTests.cs

#### パターン2: Common境界文脈中心（3件）
```csharp
// 削除
using UbiquitousLanguageManager.Domain;

// 追加
using UbiquitousLanguageManager.Domain.Common;
```

**対象ファイル**:
1. Domain/ValueObjectsTests.cs
2. Infrastructure/NotificationServiceTests.cs
3. Infrastructure/AuditLoggingTests.cs

#### パターン3: 複数境界文脈（2件）
```csharp
// 削除
using UbiquitousLanguageManager.Domain;

// 追加
using UbiquitousLanguageManager.Domain.Common;
using UbiquitousLanguageManager.Domain.Authentication;
using UbiquitousLanguageManager.Domain.ProjectManagement;
```

**対象ファイル**:
1. Contracts/TypeConvertersExtensionsTests.cs
2. Contracts/TypeConvertersTests.cs

### 2. EnableDefaultCompileItems削除（5分・3箇所）

**対象ファイル**: `tests/UbiquitousLanguageManager.Tests/UbiquitousLanguageManager.Tests.csproj`

**削除箇所**:
- Line 9付近: `<EnableDefaultCompileItems>false</EnableDefaultCompileItems>`
- Line 31-36付近: F#ファイル除外設定
- Line 59-74付近: Phase B1テスト明示的Include

### 3. ビルド・テスト実行確認（5分）

```bash
# 全体ビルド確認
dotnet build
# 成功基準: 0 Warning/0 Error

# Phase Aテスト実行確認
dotnet test tests/UbiquitousLanguageManager.Tests --verbosity normal
# 成功基準: Phase A既存テスト 100%成功
```

### 4. git commit作成

**コミットメッセージ**: "Phase B-F1 Step2: Issue #43完全解決 - namespace階層化適用・技術負債解消"

---

## 🚨 リスク管理

### 想定リスク・対策

| リスク | 影響度 | 対策 | ロールバック時間 |
|-------|-------|------|----------------|
| typo・namespace誤り | 中 | 各ファイル修正後の個別ビルド確認 | 5分 |
| テスト失敗 | 中 | エラーメッセージ確認・即座修正 | 5-10分 |
| ビルドエラー長期化 | 高 | 10分以上解決不可の場合ロールバック | 5-10分 |

### ロールバック実行基準
- ビルドエラーが10分以上解決不可
- テスト成功率が95%未満に低下

### ロールバック手順
```bash
# Step2 commit特定
git log -5 --oneline

# revert実行
git revert [Step2 commit-hash]

# 確認
dotnet build
dotnet test
```

---

## 📝 実行記録（随時更新）

### 技術的前提条件確認

**実施日時**: 2025-10-09

#### ビルド状況
```
ビルドに成功しました。
    0 個の警告
    0 エラー
経過時間 00:00:20.89
```
✅ **確認結果**: Phase B1完了時状態維持（0 Warning/0 Error）

#### git状況
```
On branch feature/PhaseB-F1
nothing to commit, working tree clean
```
✅ **確認結果**: クリーンな状態・作業開始準備完了

---

## ✅ Step終了時レビュー
*Step完了時に更新予定*

### 実績記録
- 実施時間:
- 修正ファイル数:
- 成功率:

### 品質確認
- ビルド結果:
- テスト結果:
- 技術負債解消:

---

**Step開始**: 2025-10-09
**Step責任者**: Claude Code
**SubAgent**: unit-test
