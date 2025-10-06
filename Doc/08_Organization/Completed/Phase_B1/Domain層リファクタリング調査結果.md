# Domain層リファクタリング調査結果

**調査実施日**：2025-09-30
**調査目的**：Phase C/D実装に向けたDomain層構造の適切性評価・リファクタリング必要性判断
**調査範囲**：全レイヤー（Domain・Application・Contracts・Infrastructure・Web）

---

## 📊 調査結果サマリー

| レイヤー | 現状評価 | リファクタリング必要性 | 優先度 | 推奨タイミング |
|---------|---------|-------------------|-------|--------------|
| Domain（F#） | 🔴 問題あり | **必須** | 高 | Phase B1 Step4前（今すぐ） |
| Application（F#） | 🟢 良好 | 不要 | - | - |
| Contracts（C#） | 🟡 監視必要 | Phase C前検討 | 中 | Phase B2完了後 |
| Infrastructure（C#） | 🟢 良好 | 不要 | - | - |
| Web（C#） | 🟢 良好 | 不要 | - | - |

## 🔴 Domain層（F#）：リファクタリング必須

### 現状分析

**ファイル構成**（4ファイル・1,289行）：
```
ValueObjects.fs    354行  ← 認証・プロジェクト・ドメイン・共通型が混在
Entities.fs        545行  ← User・Project・Domain・UbiquitousLanguageが混在
DomainServices.fs  290行  ← UserDomainService・ProjectDomainServiceが混在
Specifications.fs  100行  ← 仕様パターン
```

### Phase C/D成長予測

| Phase | ValueObjects | Entities | DomainServices |
|-------|-------------|----------|----------------|
| **現在（B1）** | 354行 | 545行 | 290行 |
| Phase C追加 | +150行 | +200行 | +180行 |
| Phase D追加 | +250行 | +400行 | +300行 |
| **Phase D完了時** | **754行** | **1,145行** | **770行** |

### リスク評価

#### 🔴 可読性リスク（高）
- 単一ファイル1,000行超は初学者に不適切
- 境界文脈混在で関連コード探索困難
- F#関数型パラダイムとの相乗効果で学習困難

#### 🔴 保守性リスク（高）
- 境界文脈混在で並列開発困難（SubAgent競合）
- 影響範囲特定困難
- Railway-oriented Programming等の複雑パターンとの組み合わせで理解困難

#### 🟡 F#コンパイル順序制約リスク（中）
- F#は宣言順序依存（前方参照不可）
- Phase C/D実装後のリファクタリングは工数3-5倍増加

#### 🟡 テスト保守性リスク（中）
- 現在52テスト全成功
- 早期実施で影響範囲最小化可能

### 推奨リファクタリング内容

**Bounded Context別ディレクトリ分離**：
```
Domain/
├── Common/                # 共通定義（ID型・Permission・Role等）
├── Authentication/        # 認証境界文脈（User・Email・Password等）
├── ProjectManagement/     # プロジェクト管理境界文脈（Project・ProjectName等）
└── (Phase C/D追加予定)
    ├── DomainManagement/      # ドメイン管理境界文脈
    └── LanguageManagement/    # ユビキタス言語管理境界文脈
```

**実装工数**：
- **Step4（ディレクトリ分離）**：3.5-4.5時間
- **Step5（namespace階層化）**：3-4時間
**最適タイミング**：Phase B1 Step4-5（分割実施）

### 実施しない場合の影響

- Phase C実装時：500+行ファイルでの作業
- Phase D実装時：1,000+行ファイルでの作業
- リファクタリング工数：3-4時間 → 10-15時間（3-5倍）
- テスト修正範囲：52テスト → 100+テスト

## 🟢 Application層（F#）：現状良好

### 現状構造

**既にBounded Context分離実施中**：
```
Application/
├── Interfaces/
├── ProjectManagement/          ← Bounded Context分離済み
│   ├── Commands.fs            (113行)
│   ├── Queries.fs
│   ├── IProjectManagementService.fs
│   └── ProjectManagementService.fs (432行)
├── AuthenticationServices.fs
└── ApplicationServices.fs
```

### 評価
- ✅ ProjectManagementディレクトリでBounded Context分離済み
- ✅ Phase C/D追加時も同様パターン適用予定
- ✅ ファイルサイズ適切（最大432行）

**リファクタリング不要**

## 🟡 Contracts層（C#）：将来的監視必要

### 現状構造

```
Contracts/
├── DTOs/              18ファイル（境界文脈別分離なし）
├── Converters/
│   └── TypeConverters.cs    1,038行 ← 要注意
├── Mappers/
└── Interfaces/
```

### Phase C/D成長予測

**TypeConverters.cs**：
- 現在：1,038行
- Phase C追加予測：+300行
- Phase D追加予測：+500行
- **Phase D完了時**：1,838行

### 推奨事項

**Phase C前に境界文脈別分割検討**（優先度：中）：
```
Converters/
├── Common/
│   └── CommonTypeConverters.cs
├── Authentication/
│   └── AuthenticationTypeConverters.cs
├── ProjectManagement/
│   └── ProjectTypeConverters.cs
└── (Phase C/D追加)
```

**実装工数**：1-2時間
**推奨タイミング**：Phase B2完了後・Phase C開始前

## 🟢 Infrastructure層（C#）：現状良好

### 現状構造

```
Infrastructure/
├── Data/              EF Core設定
├── Identity/          ASP.NET Core Identity
├── Repositories/      Repository実装（31ファイル）
├── Services/          ドメインサービス実装
└── Emailing/          メール送信サービス
```

### 評価
- ✅ 機能別ディレクトリ分離済み
- ✅ ファイルサイズ適切（最大899行：UserRepository.cs）
- ✅ Phase C/D追加で自然に分散予測

**リファクタリング不要**

## 🟢 Web層（C#）：現状良好

### 現状構造

```
Web/Components/
├── Account/           アカウント関連UI
├── Authorization/     権限制御コンポーネント
├── Pages/            画面（.razor）
└── Security/         セキュリティコンポーネント
```

### 評価
- ✅ 機能別ディレクトリ分離済み
- ✅ 48ファイル（.razor + .cs）で適切に分散
- ✅ 最大ファイル509行（SecurityStatusIndicator.razor）
- ✅ Phase C/D追加で機能別ディレクトリ追加予定

**リファクタリング不要**

## 🎯 実施推奨事項

### 最優先：Domain層リファクタリング（2段階実施）

**実施タイミング**：Phase B1 Step4-5（連続実施）
**理由**：
1. Step3完了でDomain/Application層実装完了・Infrastructure層未実装
2. 影響範囲最小化（Infrastructure/Web層実装前）
3. テスト基盤確立（52テスト100%成功状態）
4. Phase C/D前準備（最適な構造で開始可能）

**実装工数**：
- **Step4（Bounded Context別ディレクトリ分離）**：3.5-4.5時間
- **Step5（全層namespace階層化）**：3-4時間（GitHub Issue #42）
**品質保証**：0 Warning/0 Error・52テスト100%成功継続

### 第2優先：Contracts層TypeConverters分割

**実施タイミング**：Phase B2完了後・Phase C開始前
**トリガー**：TypeConverters.cs 1,500行超予測時
**実装工数**：1-2時間

## 📋 関連ドキュメント

### Issue作成
- **Step4関連Issue文書**：`/Doc/06_Issues/ISSUE_Domain層リファクタリング提案.md`
- **Step5関連GitHub Issue**：#42 Domain層namespace階層化対応

### 参考文書
- Phase B1 Phase_Summary：`/Doc/08_Organization/Active/Phase_B1/Phase_Summary.md`
- Step04詳細設計：`/Doc/08_Organization/Active/Phase_B1/Step04_Domain層リファクタリング.md`
- Step05詳細設計：`/Doc/08_Organization/Active/Phase_B1/Step05_namespace階層化.md`
- 機能仕様書（Phase C/D定義）：`/Doc/01_Requirements/機能仕様書.md`
- Clean Architecture ADR：`/Doc/07_Decisions/ADR_001_アーキテクチャ構成決定.md`

## 📝 調査手法

### 実施内容
1. 各レイヤーのファイル構成・行数調査
2. Phase C/D機能仕様書確認・成長予測算出
3. Bounded Context観点での構造評価
4. リスク分析（可読性・保守性・技術制約）
5. 実装工数・タイミング検討

### 使用ツール
- Bash（wc・find・grep）：ファイル行数・構造調査
- Read：仕様書・Phase文書確認
- Serena MCP：プロジェクト概要・技術詳細確認

---

**調査実施者**：Claude Code
**調査完了日**：2025-09-30
**対応決定**：Step4（ディレクトリ分離）・Step5（namespace階層化）の2段階実施（GitHub Issue #42）