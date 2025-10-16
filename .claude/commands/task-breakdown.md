# タスク分解・自動管理Command

**目的**: Phase/Step作業の自動分解とTodoList連携管理
**対象**: Phase開始時・Step開始時・複雑作業時
**連携先**: TodoWriteツール・GitHub Issues・仕様書・設計書

## 🎯 Command機能概要

### 自動タスク分解機能
```yaml
分解対象:
  - Phase/Step実装作業
  - GitHub Issue対応事項
  - 仕様改善作業
  - 品質改善作業

出力:
  - 構造化タスクリスト
  - TodoWriteツール自動更新
  - 工数見積もり（概算）
  - 依存関係マッピング
```

### Phase B1特化機能
```yaml
Phase B1対応:
  - プロジェクト基本CRUD分解
  - 権限制御テストマトリックス分解
  - デフォルトドメイン設計分解
  - Clean Architecture層別分解
```

## コマンド実行内容

### 1. 作業情報収集・分析
```bash
echo "📋 作業情報の収集・分析を開始..."
```

**情報収集対象**:
- [ ] **現在Phase状況**: Serenaメモリー`project_overview`から進捗確認
- [ ] **Phase計画情報**: `/Doc/08_Organization/Active/Phase_XX/Phase_Summary.md`
- [ ] **GitHub Issues**: 未完了Issue（特にphase-B1, 高優先度ラベル）
- [ ] **仕様書・設計書**: 該当Phase対象の仕様・設計文書
- [ ] **技術負債情報**: GitHub Issues（TECH-XXXラベル付き）から収集

### 2. Phase B1専用タスク分解（SpecKit inspired）

#### A. Issue #38対応タスク（高優先度）
```bash
echo "🔴 Issue #38: Phase B1開始前必須対応の分解..."
```

**自動生成タスク例**:
```yaml
高優先度タスク群:
  1. デフォルトドメイン自動作成設計:
     - F# ProjectDomainServiceモジュール設計書作成
     - トランザクション境界仕様書作成
     - 失敗時ロールバック戦略定義

  2. 権限制御テストマトリックス設計:
     - 4×4=16通りテストケース表作成
     - 各組み合わせ期待動作定義書作成
     - テスト実装仕様書作成

  3. 否定的仕様補強:
     - 機能仕様書に否定的仕様セクション追加
     - 禁止事項5項目の詳細記述
     - エラーメッセージ仕様定義
```

#### B. プロジェクト基本CRUD実装タスク
```bash
echo "🏗️ プロジェクト基本CRUD実装の層別分解..."
```

**Clean Architecture層別分解**:
```yaml
Domain層（F#）:
  - Project型定義・Smart Constructor実装
  - ProjectId型・ProjectName型実装
  - プロジェクト作成ビジネスルール実装
  - プロジェクト編集制約ルール実装

Application層（F#）:
  - CreateProjectCommand/Query定義
  - UpdateProjectCommand/Query定義
  - DeleteProjectCommand定義
  - GetProjectsQuery実装

Contracts層（C#）:
  - ProjectDto/CreateProjectDto実装
  - ProjectTypeConverter実装（F#↔C#変換）
  - バリデーション属性定義

Infrastructure層（C#）:
  - ProjectRepository実装
  - ProjectEntity定義
  - EF Core設定（ProjectConfig）
  - データベースマイグレーション

Web層（C#/Blazor Server）:
  - プロジェクト一覧画面（Projects/Index.razor）
  - プロジェクト作成画面（Projects/Create.razor）
  - プロジェクト編集画面（Projects/Edit.razor）
  - プロジェクト削除機能実装
```

### 3. 権限制御実装タスク分解
```bash
echo "🔐 権限制御システムの実装分解..."
```

**権限制御マトリックス実装**:
```yaml
権限制御実装タスク:
  認証・認可基盤:
    - プロジェクト権限Policy定義
    - ProjectManagerAuthorizationHandler実装
    - 権限チェック属性実装

  テスト実装:
    - SuperUser権限テスト（4機能×成功パターン）
    - ProjectManager権限テスト（編集OK・登録削除NG）
    - DomainApprover権限テスト（全機能NG）
    - GeneralUser権限テスト（全機能NG）

  UI権限制御:
    - メニュー表示制御（権限別）
    - ボタン表示制御（作成・編集・削除）
    - データフィルタ制御（担当プロジェクトのみ表示）
```

### 4. テスト実装タスク分解
```bash
echo "🧪 テスト実装の体系的分解..."
```

**テスト実装計画**:
```yaml
単体テスト:
  - F# Domain層テスト（xUnit + FsUnit）
  - F# Application層テスト（Moq連携）
  - C# Repository層テスト（InMemoryDatabase）
  - TypeConverter相互変換テスト

統合テスト:
  - WebApplicationFactory統合テスト
  - データベース統合テスト（TestContainer）
  - 権限制御統合テスト

E2Eテスト:
  - プロジェクト管理業務フローテスト
  - 権限制御UIテスト
  - エラーハンドリングテスト
```

### 5. TodoWriteツール自動更新
```bash
echo "📝 TodoWriteツールへの自動反映..."
```

**TodoList自動生成**:
- [ ] **高優先度タスクの自動設定**: Issue #38関連は`in_progress`
- [ ] **依存関係の考慮**: Domain→Application→Infrastructure→Web順序
- [ ] **工数見積もり付与**: 各タスクに概算時間（30分/1時間/2時間等）
- [ ] **進捗状況の自動管理**: 完了タスクの`completed`自動更新

**TodoList出力例**:
```yaml
自動生成Todo例:
  1. [高優先度] デフォルトドメイン設計書作成 (1時間)
  2. [高優先度] 権限制御テストマトリックス作成 (1時間)
  3. [高優先度] 否定的仕様セクション追加 (30分)
  4. Project Domain型定義・実装 (1時間)
  5. ProjectRepository実装 (1時間)
  6. プロジェクト一覧画面実装 (1時間)
  7. 16通り権限テスト実装 (2時間)
  8. 統合テスト実装 (1時間)
```

### 6. 作業依存関係・実行順序の自動決定
```bash
echo "🔗 タスク依存関係の分析・実行順序決定..."
```

**依存関係マッピング**:
```yaml
実行順序（並列実行考慮）:
  Stage 1（並列可能）:
    - Issue #38の3項目対応
    - F# Domain型定義

  Stage 2（Domain依存）:
    - F# Application層実装
    - TypeConverter実装

  Stage 3（Application依存）:
    - C# Repository実装
    - 権限制御Policy実装

  Stage 4（Infrastructure依存）:
    - Blazor UI実装
    - 統合テスト実装

  Stage 5（全体統合）:
    - E2Eテスト実装
    - 品質確認・仕様準拠チェック
```

## 📊 出力成果物

### 自動生成ファイル
```yaml
生成ファイル:
  - /Doc/08_Organization/Active/Phase_XX/Task_Breakdown.md
  - /Doc/08_Organization/Active/Phase_XX/Dependencies_Matrix.md
  - TodoWriteツール直接更新
```

### 進捗追跡機能
```yaml
進捗管理:
  - タスク完了率の自動計算
  - Stage進捗の可視化
  - 予定vs実績の追跡
  - ボトルネック特定・改善提案
```

## 🎯 Phase B1特化機能

### GitHub Issue連携
- [ ] **Issue #38自動読み込み**: 3項目の詳細タスク化
- [ ] **新規Issue自動作成**: 必要に応じてサブタスクIssue作成
- [ ] **Issue完了連携**: タスク完了時の自動Issue更新

### 仕様改善支援
- [ ] **spec-validate連携**: 95点達成のための改善タスク生成
- [ ] **spec-compliance連携**: 品質確認タスク自動追加

### SubAgent連携最適化
- [ ] **並列実行計画**: 効率的なSubAgent組み合わせ提案
- [ ] **Pattern自動選択**: Phase B1に最適なPattern B適用

## 🔄 継続改善・学習機能

### タスク分解精度向上
- [ ] **過去実績学習**: 実際の作業時間vs見積もりの学習
- [ ] **分解パターン改善**: 効果的な分解粒度の自動最適化
- [ ] **依存関係精度向上**: 実際の依存関係パターンの学習

### プロジェクト適応
- [ ] **Phase特性学習**: B/C/D各Phaseの特性に応じた分解最適化
- [ ] **技術スタック対応**: F#/C#/Clean Architecture特化分解

## 実行方法・使用例

### 基本実行
```markdown
Command: task-breakdown
Purpose: 現在Phase/Stepの作業を自動分解してTodoList生成

実行例:
User: "Phase B1のタスク分解をお願いします"
→ Phase B1情報収集 → Issue #38読み込み → CRUD分解 → TodoList自動生成
```

### 特定作業分解
```markdown
Command: task-breakdown --target="Issue #38"
Purpose: 特定Issue/作業の詳細分解

Command: task-breakdown --layer="Domain"
Purpose: 特定層（Domain/Application等）の分解のみ実行
```

---

**作成日**: 2025-09-25
**目的**: Phase B1効率化・SpecKit的作業管理・TodoWriteツール活用
**効果**: 作業の構造化・進捗の可視化・効率的な並列実行計画