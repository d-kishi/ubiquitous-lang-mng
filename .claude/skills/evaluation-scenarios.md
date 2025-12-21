# Skills評価シナリオ

**作成日**: 2025-12-21
**目的**: 各Skillの活性化条件検証用シナリオ
**準拠**: https://platform.claude.com/docs/en/agents-and-tools/agent-skills/best-practices#evaluation-and-iteration

---

## シナリオ形式

```json
{
  "skills": ["skill-name"],
  "query": "ユーザー入力例",
  "expected_behavior": ["期待される動作1", "期待される動作2"]
}
```

---

## 1. adr-knowledge-base（3シナリオ）

### シナリオ1.1: ポジティブ（基本トリガー）
```json
{
  "skills": ["adr-knowledge-base"],
  "query": "新しい認証方式を技術決定したいのですが、参考になるADRはありますか？",
  "expected_behavior": [
    "adr-knowledge-base Skillが活性化",
    "関連ADR一覧を参照",
    "ADR_016（プロセス遵守）等の関連ADRを提示"
  ]
}
```

### シナリオ1.2: エッジケース（複合トリガー）
```json
{
  "skills": ["adr-knowledge-base"],
  "query": "アーキテクチャ選定でClean Architectureを採用した理由を確認したい",
  "expected_behavior": [
    "adr-knowledge-base Skillが活性化",
    "ADR_019（namespace設計）を参照",
    "設計判断の根拠を説明"
  ]
}
```

### シナリオ1.3: ネガティブ（非トリガー）
```json
{
  "skills": [],
  "query": "ログイン画面のCSSを修正してください",
  "expected_behavior": [
    "adr-knowledge-base Skillは活性化しない",
    "CSS修正の実装を直接実施"
  ]
}
```

---

## 2. clean-architecture-guardian（3シナリオ）

### シナリオ2.1: ポジティブ（基本トリガー）
```json
{
  "skills": ["clean-architecture-guardian"],
  "query": "新規クラスUserServiceをApplication層に作成します",
  "expected_behavior": [
    "clean-architecture-guardian Skillが活性化",
    "レイヤー分離原則を確認",
    "namespace階層化ルールを適用"
  ]
}
```

### シナリオ2.2: エッジケース（複合トリガー）
```json
{
  "skills": ["clean-architecture-guardian"],
  "query": "ビルドエラーが出ています。循環依存かもしれません",
  "expected_behavior": [
    "clean-architecture-guardian Skillが活性化",
    "循環依存チェックを実施",
    "違反パターンを特定"
  ]
}
```

### シナリオ2.3: ネガティブ（非トリガー）
```json
{
  "skills": [],
  "query": "データベースにカラムを追加してください",
  "expected_behavior": [
    "clean-architecture-guardian Skillは活性化しない",
    "db-schema-management Skillが候補"
  ]
}
```

---

## 3. db-schema-management（3シナリオ）

### シナリオ3.1: ポジティブ（基本トリガー）
```json
{
  "skills": ["db-schema-management"],
  "query": "Projectsテーブルに新しい列を追加したい",
  "expected_behavior": [
    "db-schema-management Skillが活性化",
    "EF Migrations workflowを参照",
    "Migration作成手順を案内"
  ]
}
```

### シナリオ3.2: エッジケース（複合トリガー）
```json
{
  "skills": ["db-schema-management"],
  "query": "CHECK制約を追加してデータベース設計書も更新する",
  "expected_behavior": [
    "db-schema-management Skillが活性化",
    "check-constraint-pattern.mdを参照",
    "db-doc-sync-checklist.mdで同期確認"
  ]
}
```

### シナリオ3.3: ネガティブ（非トリガー）
```json
{
  "skills": [],
  "query": "ユーザー一覧画面のテストを書いてください",
  "expected_behavior": [
    "db-schema-management Skillは活性化しない",
    "テスト関連Skillが候補"
  ]
}
```

---

## 4. devcontainer-web-app（3シナリオ）

### シナリオ4.1: ポジティブ（基本トリガー）
```json
{
  "skills": ["devcontainer-web-app"],
  "query": "アプリ起動して動作確認したい",
  "expected_behavior": [
    "devcontainer-web-app Skillが活性化",
    "web-app.sh startを案内",
    "https://localhost:5001の応答確認"
  ]
}
```

### シナリオ4.2: エッジケース（複合トリガー）
```json
{
  "skills": ["devcontainer-web-app"],
  "query": "Step完了したので画面確認をお願いします",
  "expected_behavior": [
    "devcontainer-web-app Skillが活性化",
    "アプリ状態確認",
    "playwright-ui-verificationとの連携を示唆"
  ]
}
```

### シナリオ4.3: ネガティブ（非トリガー）
```json
{
  "skills": [],
  "query": "F#のドメインモデルを修正してください",
  "expected_behavior": [
    "devcontainer-web-app Skillは活性化しない",
    "fsharp-domain Agentが候補"
  ]
}
```

---

## 5. error-logging-patterns（3シナリオ）

### シナリオ5.1: ポジティブ（基本トリガー）
```json
{
  "skills": ["error-logging-patterns"],
  "query": "エラー処理実装をDomain層に追加したい",
  "expected_behavior": [
    "error-logging-patterns Skillが活性化",
    "層別エラー処理原則を参照",
    "Domain層ではResult型必須を案内"
  ]
}
```

### シナリオ5.2: エッジケース（複合トリガー）
```json
{
  "skills": ["error-logging-patterns"],
  "query": "ログ設計でResult型活用のベストプラクティスを教えて",
  "expected_behavior": [
    "error-logging-patterns Skillが活性化",
    "logging-guidelines.mdを参照",
    "Result型変換パターンを提示"
  ]
}
```

### シナリオ5.3: ネガティブ（非トリガー）
```json
{
  "skills": [],
  "query": "GitHub Issueを作成してください",
  "expected_behavior": [
    "error-logging-patterns Skillは活性化しない",
    "github-issues-management Skillが候補"
  ]
}
```

---

## 6. fsharp-csharp-bridge（3シナリオ）

### シナリオ6.1: ポジティブ（基本トリガー）
```json
{
  "skills": ["fsharp-csharp-bridge"],
  "query": "F#↔C#境界実装でOption型変換エラーが出ています",
  "expected_behavior": [
    "fsharp-csharp-bridge Skillが活性化",
    "option-conversion.mdを参照",
    "IsSome/Valueアクセスパターンを提示"
  ]
}
```

### シナリオ6.2: エッジケース（複合トリガー）
```json
{
  "skills": ["fsharp-csharp-bridge"],
  "query": "Contracts層でTypeConverterを実装する際のResult型変換方法",
  "expected_behavior": [
    "fsharp-csharp-bridge Skillが活性化",
    "result-conversion.mdを参照",
    "NewOk/NewError生成パターンを提示"
  ]
}
```

### シナリオ6.3: ネガティブ（非トリガー）
```json
{
  "skills": [],
  "query": "E2Eテストを作成してください",
  "expected_behavior": [
    "fsharp-csharp-bridge Skillは活性化しない",
    "playwright-e2e-patterns Skillが候補"
  ]
}
```

---

## 7. github-issues-management（3シナリオ）

### シナリオ7.1: ポジティブ（基本トリガー）
```json
{
  "skills": ["github-issues-management"],
  "query": "技術的負債を記録するIssue作成をお願いします",
  "expected_behavior": [
    "github-issues-management Skillが活性化",
    "運用規則を参照",
    "ラベル選択ガイドを適用"
  ]
}
```

### シナリオ7.2: エッジケース（複合トリガー）
```json
{
  "skills": ["github-issues-management"],
  "query": "バグ報告のIssueにラベル設定をしたい",
  "expected_behavior": [
    "github-issues-management Skillが活性化",
    "label-selection-guide.mdを参照",
    "種別・優先度・影響範囲ラベルを提案"
  ]
}
```

### シナリオ7.3: ネガティブ（非トリガー）
```json
{
  "skills": [],
  "query": "テストカバレッジを確認してください",
  "expected_behavior": [
    "github-issues-management Skillは活性化しない",
    "tdd-red-green-refactor Skillが候補"
  ]
}
```

---

## 8. playwright-e2e-patterns（3シナリオ）

### シナリオ8.1: ポジティブ（基本トリガー）
```json
{
  "skills": ["playwright-e2e-patterns"],
  "query": "E2Eテスト実装でログインフローのテストを書きたい",
  "expected_behavior": [
    "playwright-e2e-patterns Skillが活性化",
    "blazor-signalr-e2e.mdを参照",
    "SignalR待機パターンを提示"
  ]
}
```

### シナリオ8.2: エッジケース（複合トリガー）
```json
{
  "skills": ["playwright-e2e-patterns"],
  "query": "data-testid設計でローディング状態テストに対応したい",
  "expected_behavior": [
    "playwright-e2e-patterns Skillが活性化",
    "data-testid-design.mdを参照",
    "CDP Network Throttlingパターンを提示"
  ]
}
```

### シナリオ8.3: ネガティブ（非トリガー）
```json
{
  "skills": [],
  "query": "単体テストを作成してください",
  "expected_behavior": [
    "playwright-e2e-patterns Skillは活性化しない",
    "tdd-red-green-refactor Skillが候補"
  ]
}
```

---

## 9. playwright-ui-verification（3シナリオ）

### シナリオ9.1: ポジティブ（基本トリガー）
```json
{
  "skills": ["playwright-ui-verification"],
  "query": "画面確認でユーザー一覧画面の表示を確認したい",
  "expected_behavior": [
    "playwright-ui-verification Skillが活性化",
    "前提条件（アプリ起動）を確認",
    "snapshot取得フローを案内"
  ]
}
```

### シナリオ9.2: エッジケース（複合トリガー）
```json
{
  "skills": ["playwright-ui-verification"],
  "query": "バグ調査でスクリーンショット取得をお願いします",
  "expected_behavior": [
    "playwright-ui-verification Skillが活性化",
    "シーンC（バグ再現・調査）を参照",
    "スクリーンショット命名規則を適用"
  ]
}
```

### シナリオ9.3: ネガティブ（非トリガー）
```json
{
  "skills": [],
  "query": "マイグレーションを実行してください",
  "expected_behavior": [
    "playwright-ui-verification Skillは活性化しない",
    "db-schema-management Skillが候補"
  ]
}
```

---

## 10. spec-compliance-auto（3シナリオ）

### シナリオ10.1: ポジティブ（基本トリガー）
```json
{
  "skills": ["spec-compliance-auto"],
  "query": "新機能実装前に仕様確認をしたい",
  "expected_behavior": [
    "spec-compliance-auto Skillが活性化",
    "仕様書の該当セクション特定",
    "要件理解を支援"
  ]
}
```

### シナリオ10.2: エッジケース（複合トリガー）
```json
{
  "skills": ["spec-compliance-auto"],
  "query": "仕様逸脱リスク確認で要件検証を行いたい",
  "expected_behavior": [
    "spec-compliance-auto Skillが活性化",
    "compliance-workflow.mdを参照",
    "仕様準拠率チェック項目を適用"
  ]
}
```

### シナリオ10.3: ネガティブ（非トリガー）
```json
{
  "skills": [],
  "query": "Dockerコンテナを再起動してください",
  "expected_behavior": [
    "spec-compliance-auto Skillは活性化しない",
    "devcontainer-web-app Skillが候補"
  ]
}
```

---

## 11. subagent-patterns（3シナリオ）

### シナリオ11.1: ポジティブ（基本トリガー）
```json
{
  "skills": ["subagent-patterns"],
  "query": "Step開始でSubAgent選択をお願いします",
  "expected_behavior": [
    "subagent-patterns Skillが活性化",
    "14種類のAgent一覧を参照",
    "最適な組み合わせを提案"
  ]
}
```

### シナリオ11.2: エッジケース（複合トリガー）
```json
{
  "skills": ["subagent-patterns"],
  "query": "並列実行判断でAgent責務の境界を確認したい",
  "expected_behavior": [
    "subagent-patterns Skillが活性化",
    "agent-responsibility-boundary.mdを参照",
    "並列実行可否を判定"
  ]
}
```

### シナリオ11.3: ネガティブ（非トリガー）
```json
{
  "skills": [],
  "query": "ログ出力のレベルを変更してください",
  "expected_behavior": [
    "subagent-patterns Skillは活性化しない",
    "error-logging-patterns Skillが候補"
  ]
}
```

---

## 12. tdd-red-green-refactor（3シナリオ）

### シナリオ12.1: ポジティブ（基本トリガー）
```json
{
  "skills": ["tdd-red-green-refactor"],
  "query": "テスト駆動開発でドメインモデルの単体テストを作成したい",
  "expected_behavior": [
    "tdd-red-green-refactor Skillが活性化",
    "Red Phase（失敗するテスト）から開始",
    "カバレッジ目標を確認"
  ]
}
```

### シナリオ12.2: エッジケース（複合トリガー）
```json
{
  "skills": ["tdd-red-green-refactor"],
  "query": "テストファーストでリファクタリングの準備をしたい",
  "expected_behavior": [
    "tdd-red-green-refactor Skillが活性化",
    "Refactor Phase詳細を参照",
    "Green状態維持を確認"
  ]
}
```

### シナリオ12.3: ネガティブ（非トリガー）
```json
{
  "skills": [],
  "query": "ADRを新規作成してください",
  "expected_behavior": [
    "tdd-red-green-refactor Skillは活性化しない",
    "adr-knowledge-base Skillが候補"
  ]
}
```

---

## 13. test-architecture（3シナリオ）

### シナリオ13.1: ポジティブ（基本トリガー）
```json
{
  "skills": ["test-architecture"],
  "query": "テストプロジェクト作成でInfrastructure層の統合テストを追加したい",
  "expected_behavior": [
    "test-architecture Skillが活性化",
    "命名規則を確認",
    "参照関係原則を適用"
  ]
}
```

### シナリオ13.2: エッジケース（複合トリガー）
```json
{
  "skills": ["test-architecture"],
  "query": "テスト参照関係でテスト配置を確認したい",
  "expected_behavior": [
    "test-architecture Skillが活性化",
    "reference-relationships.mdを参照",
    "レイヤー×テストタイプ分離を確認"
  ]
}
```

### シナリオ13.3: ネガティブ（非トリガー）
```json
{
  "skills": [],
  "query": "Blazorコンポーネントを修正してください",
  "expected_behavior": [
    "test-architecture Skillは活性化しない",
    "csharp-web-ui Agentが候補"
  ]
}
```

---

## サマリー

| Skill名 | ポジティブ | エッジケース | ネガティブ |
|---------|-----------|-------------|-----------|
| adr-knowledge-base | ✅ | ✅ | ✅ |
| clean-architecture-guardian | ✅ | ✅ | ✅ |
| db-schema-management | ✅ | ✅ | ✅ |
| devcontainer-web-app | ✅ | ✅ | ✅ |
| error-logging-patterns | ✅ | ✅ | ✅ |
| fsharp-csharp-bridge | ✅ | ✅ | ✅ |
| github-issues-management | ✅ | ✅ | ✅ |
| playwright-e2e-patterns | ✅ | ✅ | ✅ |
| playwright-ui-verification | ✅ | ✅ | ✅ |
| spec-compliance-auto | ✅ | ✅ | ✅ |
| subagent-patterns | ✅ | ✅ | ✅ |
| tdd-red-green-refactor | ✅ | ✅ | ✅ |
| test-architecture | ✅ | ✅ | ✅ |

**合計**: 39シナリオ（13 Skills × 3シナリオ）
