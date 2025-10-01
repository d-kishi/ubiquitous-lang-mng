# ADR_019: namespace設計規約（Bounded Context別サブnamespace規約）

**ステータス**: 承認済み
**決定日**: 2025-10-01
**決定者**: プロジェクトオーナー

## 背景・課題

### 発生した問題
Phase B1 Step4完了後、以下のアーキテクチャ不整合が発覚：
- **Application層**: `UbiquitousLanguageManager.Application.ProjectManagement`（サブnamespace使用）
- **Domain層**: `UbiquitousLanguageManager.Domain`（フラットnamespace）
- **結果**: Step5（namespace階層化）で3.5-4.5時間の追加作業が必要

### 根本原因
1. **規約の不在**: ADR_010に「レイヤー構造を反映した階層化」の記載はあるが、Bounded Context別サブnamespace使用の明示的ルールなし
2. **文書化不足**: 実装例は存在するが、規約として文書化されず
3. **検証プロセス不足**: namespace構造妥当性チェックプロセスなし
4. **段階的実装の弊害**: Phase A完了時にnamespace規約を確立しなかったため、Phase B実装時に不整合が顕在化

## 決定事項

### 1. Bounded Context別サブnamespace必須化

#### 基本テンプレート
```
<ProjectName>.<Layer>.<BoundedContext>[.<Feature>]
```

#### 具体的namespace規約

**Domain層**（Phase B1 Step5で確立）:
```fsharp
namespace UbiquitousLanguageManager.Domain.Common          // 共通定義
namespace UbiquitousLanguageManager.Domain.Authentication  // 認証境界文脈
namespace UbiquitousLanguageManager.Domain.ProjectManagement  // プロジェクト管理境界文脈
namespace UbiquitousLanguageManager.Domain.UbiquitousLanguageManagement  // ユビキタス言語管理境界文脈
```

**Application層**（Phase B1 Step3で確立）:
```fsharp
namespace UbiquitousLanguageManager.Application.ProjectManagement
namespace UbiquitousLanguageManager.Application.Interfaces
```

**Infrastructure層**（将来実装・推奨構造）:
```csharp
namespace UbiquitousLanguageManager.Infrastructure.Data
namespace UbiquitousLanguageManager.Infrastructure.Repositories
namespace UbiquitousLanguageManager.Infrastructure.Identity
```

**Contracts層**（Phase B1 Step3で確立）:
```csharp
namespace UbiquitousLanguageManager.Contracts.DTOs
namespace UbiquitousLanguageManager.Contracts.Converters
namespace UbiquitousLanguageManager.Contracts.Interfaces
```

**Web層**（Phase A完了・部分的使用）:
```csharp
namespace UbiquitousLanguageManager.Web.Components
namespace UbiquitousLanguageManager.Web.Pages
namespace UbiquitousLanguageManager.Web.Services
```

### 2. 階層構造ルール

#### Common特別扱い
- **Common**: 全Bounded Contextで使用する共通定義
- **配置**: 各層のルート直下または`.Common`サブnamespace
- **依存関係**: 他のBounded Contextに依存しない
- **例**: `Domain.Common`（UserId, ProjectId, Role, Permission等）

#### Bounded Context別分離
- **Common**: 全境界文脈共通定義（ID型・Permission・Role等）
- **Authentication**: ユーザー・認証・権限管理
- **ProjectManagement**: プロジェクト管理
- **UbiquitousLanguageManagement**: ユビキタス言語管理（Phase D拡張予定）
- **DomainManagement**: ドメイン管理（Phase C実装予定）

#### 最大階層制限
- **推奨**: 3階層まで（`<Project>.<Layer>.<BoundedContext>`）
- **許容**: 4階層（`<Project>.<Layer>.<BoundedContext>.<Feature>`）
- **例**: `UbiquitousLanguageManager.Domain.ProjectManagement.Specifications`
- **理由**: 深すぎる階層は可読性低下・保守性悪化

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
type ProjectName = private ProjectName of string

// Smart Constructor
module ProjectName =
    let create (value: string) : Result<ProjectName, string> = ...
    let value (ProjectName name) = name

// ドメインサービス
module ProjectDomainService =
    let validateProjectName name = ...
    let createProject name = ...
```

#### F#コンパイル順序考慮（Phase B1 Step4で確立）
- **前方参照不可**: `.fsproj`でのファイル順序が重要
- **Bounded Context内依存順**: ValueObjects → Errors → Entities → DomainServices
- **Bounded Context間依存順**: Common → Authentication → ProjectManagement → UbiquitousLanguageManagement
- **理由**: F#は宣言順にコンパイル・前方参照不可の制約

### 4. C#特別考慮事項

#### using文推奨パターン
```csharp
// Repository実装
using UbiquitousLanguageManager.Domain.Common;
using UbiquitousLanguageManager.Domain.ProjectManagement;

// TypeConverter実装（Phase B1 Step3で確立）
using UbiquitousLanguageManager.Domain.Common;
using UbiquitousLanguageManager.Domain.Authentication;
using UbiquitousLanguageManager.Domain.ProjectManagement;
using UbiquitousLanguageManager.Domain.UbiquitousLanguageManagement;
```

#### using alias使用（型名衝突回避）
```csharp
// Infrastructure層のDomain Entity vs Domain層のDomain型
using DomainModel = UbiquitousLanguageManager.Domain.ProjectManagement.Domain;
using DomainEntity = UbiquitousLanguageManager.Infrastructure.Data.Entities.Domain;
```

### 5. 検証プロセス

#### Step開始時検証（必須）
- [ ] namespace構造レビュー実施
- [ ] Bounded Context境界確認
- [ ] 循環依存なし確認
- [ ] ADR_019規約準拠確認

#### Phase完了時検証（必須）
- [ ] 全層namespace整合性確認
- [ ] ADR_019規約準拠確認
- [ ] F#/C#ベストプラクティス準拠確認
- [ ] Clean Architecture 97点以上維持確認

## 技術的根拠

### 業界標準実践（2024年調査）

#### F# namespace規約
- **出典**: "Domain Modeling Made Functional" (Scott Wlaschin), F# for fun and profit
- **推奨**: Bounded Context別namespace分離
- **実用**: 保守性優先・namespace + module組み合わせ
- **理由**: DDDの境界文脈をコードレベルで明確化

#### C# namespace規約
- **出典**: Microsoft Learn, Clean Architecture実践 (Robert C. Martin)
- **推奨**: `<Company>.<Product>.<Layer>.<BoundedContext>`
- **理由**:
  - エンティティ名衝突回避
  - 依存関係制御
  - 境界明確化
  - IntelliSense効率向上

### Clean Architecture準拠
- **層責務分離**: namespace階層でレイヤー・境界文脈明確化
- **依存関係原則**: namespace構造で依存方向制御（外側→内側のみ）
- **テスタビリティ**: Bounded Context単位でのテスト容易性向上
- **拡張性**: 新しいBounded Context追加が容易

### DDD（ドメイン駆動設計）準拠
- **Bounded Context境界**: namespaceで物理的に分離
- **ユビキタス言語**: namespace名がドメイン用語と一致
- **集約ルート**: Entities.fs配下に配置・namespace で整理
- **ドメインサービス**: DomainService.fs配下に配置・namespace で整理

## 実装影響

### 既存コードへの影響
- **Phase B1 Step5**: namespace階層化で対応完了（実施時間: 約4時間）
  - Domain層: 15ファイル修正
  - Application層: 12ファイル修正
  - Contracts層: 7ファイル修正
  - Infrastructure層: 4ファイル修正
  - Web層: 2ファイル修正
  - Tests層: 2ファイル修正（型衝突解決含む）
  - **合計**: 42ファイル修正・0 Warning/0 Error達成・全32テスト成功

### 将来Phaseへの影響
- **Phase C（ドメイン管理）**: `Domain.DomainManagement` namespace新規作成
- **Phase D（ユビキタス言語管理）**: 既存`Domain.UbiquitousLanguageManagement`拡張
- **Phase E以降**: 本ADR準拠でnamespace設計・不整合発生防止

### 開発効率への影響
- **初期コスト**: namespace設計時間（Phase設計時10-15分）
- **長期メリット**:
  - コード探索容易性向上（IntelliSenseで境界文脈別表示）
  - 保守性向上（変更影響範囲が明確）
  - Phase C/D拡張性確保（新規Bounded Context追加が容易）
  - 並列開発効率向上（境界文脈単位での独立作業）

## Phase B1 Step5実装記録

### 実施内容（2025-10-01）
1. **Domain層namespace階層化**: 15ファイル・4境界文脈（Common/Authentication/ProjectManagement/UbiquitousLanguageManagement）
2. **Application層open文修正**: 12ファイル・Bounded Context別open文追加
3. **Contracts層using文修正**: 7ファイル・C#境界参照更新
4. **Infrastructure層using文修正**: 4ファイル・認証系中心
5. **Web層using文修正**: 2ファイル・@using形式対応
6. **Tests層修正**: 2ファイル・型衝突解決（完全修飾名使用）
7. **統合ビルド・テスト**: 全層0 Warning/0 Error・32テスト100%成功

### 発見された課題と対応
- **課題**: `ProjectCreationError.DuplicateProjectName` と `ProjectUpdateError.DuplicateProjectName` の型名衝突
- **対応**: テストコードで完全修飾名使用（12箇所修正）
- **教訓**: 同一namespace内で同名コンストラクタを持つ判別共用体は型衝突リスクあり

## 関連文書

- **ADR_010**: 実装規約（Line 74「レイヤー構造を反映した階層化」基本方針）
- **ADR_012**: 階層構造統一ルール（ドキュメント階層構造・本ADRはコード構造）
- **Step05_namespace階層化.md**: 本ADR適用の実装記録
- **GitHub Issue #42**: namespace階層化対応Issue

## レビュー履歴

| 日付 | レビュー者 | 結果 | コメント |
|------|------------|------|----------|
| 2025-10-01 | プロジェクトオーナー | 承認 | Phase B1 Step5完了・namespace規約明文化により再発防止確立 |

---

**承認者**: プロジェクトオーナー
**承認日**: 2025-10-01
**有効期間**: プロジェクト実装フェーズ全体
**次回レビュー**: Phase C実装前（新規Bounded Context追加時）
