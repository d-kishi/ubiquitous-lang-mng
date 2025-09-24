# Phase XX 仕様準拠実装証跡

**生成日時**: {GENERATED_DATE}
**対象フェーズ**: Phase {PHASE_NAME}
**準拠度スコア**: {COMPLIANCE_SCORE} / 100点
**証跡収集方法**: spec-compliance-check-enhanced Command自動生成

## 📊 総合準拠度サマリー

### 加重スコア内訳
```yaml
肯定的仕様準拠度: {POSITIVE_SPEC_SCORE}/50点
  必須機能実装: {REQUIRED_FEATURES}/30点
  推奨機能実装: {RECOMMENDED_FEATURES}/15点
  拡張機能実装: {EXTENDED_FEATURES}/5点

否定的仕様遵守度: {NEGATIVE_SPEC_SCORE}/30点
  禁止事項遵守: {PROHIBITED_COMPLIANCE}/20点
  制約条件遵守: {CONSTRAINT_COMPLIANCE}/10点

実行可能性・品質: {QUALITY_SCORE}/20点
  テストカバレッジ: {TEST_COVERAGE}/8点
  パフォーマンス: {PERFORMANCE_SCORE}/6点
  保守性: {MAINTAINABILITY}/6点
```

### 品質判定
**総合判定**: {QUALITY_JUDGMENT}
- ✅ 98-100点: 最優秀品質
- ✅ 95-97点: 優秀品質
- ⚠️ 90-94点: 良好品質
- ⚠️ 85-89点: 改善必要
- ❌ 85点未満: 品質不適格

## 🎯 仕様項目別実装証跡

### 【必須機能実装証跡】

#### 仕様項目 {SPEC_ID_1}: {SPEC_TITLE_1}
**準拠スコア**: {SPEC_SCORE_1}/5点
**重要度**: {IMPORTANCE_LEVEL_1}

**仕様内容**:
```
{SPEC_DESCRIPTION_1}
```

**実装箇所**:
- **ファイル**: `{FILE_PATH_1}`
- **行番号**: {LINE_START_1}-{LINE_END_1}
- **コミットハッシュ**: `{COMMIT_HASH_1}`

**実装コードスニペット**:
```csharp
// 仕様{SPEC_ID_1}準拠実装
{CODE_SNIPPET_1}
```

**テストケース証跡**:
```csharp
// 仕様{SPEC_ID_1}検証テスト
{TEST_CODE_1}
```

**動作確認証跡**:
- **テスト実行結果**: ✅ 成功
- **スクリーンショット**: {SCREENSHOT_PATH_1} ※概念のみ
- **実行ログ**: {LOG_EXTRACT_1}

---

#### 仕様項目 {SPEC_ID_2}: {SPEC_TITLE_2}
（以下、実装された各仕様項目について同様の証跡記録）

## 🚫 否定的仕様遵守証跡

### 【禁止事項遵守確認】

#### 禁止項目 {PROHIBITION_ID_1}: {PROHIBITION_TITLE_1}
**遵守状況**: {PROHIBITION_STATUS_1}

**禁止内容**:
```
{PROHIBITION_DESCRIPTION_1}
```

**遵守確認方法**:
- **確認箇所**: {PROHIBITION_CHECK_LOCATION_1}
- **確認方法**: {PROHIBITION_CHECK_METHOD_1}
- **確認結果**: ✅ 遵守確認 / ❌ 違反検出

**コード確認**:
```csharp
// 禁止事項遵守確認コード
{PROHIBITION_CHECK_CODE_1}
```

## 📈 品質メトリクス証跡

### テストカバレッジ証跡
```yaml
単体テスト:
  カバレッジ: {UNIT_TEST_COVERAGE}%
  成功テスト: {UNIT_TEST_PASSED}/{UNIT_TEST_TOTAL}
  実行時間: {UNIT_TEST_DURATION}秒

統合テスト:
  カバレッジ: {INTEGRATION_TEST_COVERAGE}%
  成功テスト: {INTEGRATION_TEST_PASSED}/{INTEGRATION_TEST_TOTAL}
  実行時間: {INTEGRATION_TEST_DURATION}秒

E2Eテスト:
  成功シナリオ: {E2E_TEST_PASSED}/{E2E_TEST_TOTAL}
  実行時間: {E2E_TEST_DURATION}秒
```

### パフォーマンス証跡
```yaml
レスポンス時間:
  平均応答時間: {AVERAGE_RESPONSE_TIME}ms
  最大応答時間: {MAX_RESPONSE_TIME}ms
  95パーセンタイル: {P95_RESPONSE_TIME}ms

メモリ使用量:
  平均使用量: {AVERAGE_MEMORY_USAGE}MB
  最大使用量: {MAX_MEMORY_USAGE}MB

同時接続:
  最大同時接続数: {MAX_CONCURRENT_CONNECTIONS}
  エラー率: {ERROR_RATE}%
```

## 🔍 仕様・実装対応マッピング

### ファイル別実装サマリー
```yaml
Domain層 (F#):
  - {DOMAIN_FILE_1}: 仕様{SPEC_LIST_DOMAIN_1}
  - {DOMAIN_FILE_2}: 仕様{SPEC_LIST_DOMAIN_2}

Application層 (F#):
  - {APP_FILE_1}: 仕様{SPEC_LIST_APP_1}
  - {APP_FILE_2}: 仕様{SPEC_LIST_APP_2}

Infrastructure層 (C#):
  - {INFRA_FILE_1}: 仕様{SPEC_LIST_INFRA_1}
  - {INFRA_FILE_2}: 仕様{SPEC_LIST_INFRA_2}

Web層 (C#):
  - {WEB_FILE_1}: 仕様{SPEC_LIST_WEB_1}
  - {WEB_FILE_2}: 仕様{SPEC_LIST_WEB_2}

Contracts層 (C#):
  - {CONTRACTS_FILE_1}: 仕様{SPEC_LIST_CONTRACTS_1}
```

### 仕様項番・実装行番号対応表
| 仕様ID | 仕様項目 | 実装ファイル | 行番号 | 実装完了度 |
|-------|---------|-------------|-------|-----------|
| {SPEC_ID_1} | {SPEC_TITLE_1} | {FILE_PATH_1} | {LINE_RANGE_1} | ✅ 完了 |
| {SPEC_ID_2} | {SPEC_TITLE_2} | {FILE_PATH_2} | {LINE_RANGE_2} | ✅ 完了 |
| {SPEC_ID_3} | {SPEC_TITLE_3} | {FILE_PATH_3} | {LINE_RANGE_3} | ⚠️ 部分完了 |

## 🎯 改善提案・次回改善点

### 検出された改善点
```yaml
高優先度改善点:
  - {HIGH_PRIORITY_IMPROVEMENT_1}
  - {HIGH_PRIORITY_IMPROVEMENT_2}

中優先度改善点:
  - {MEDIUM_PRIORITY_IMPROVEMENT_1}
  - {MEDIUM_PRIORITY_IMPROVEMENT_2}

低優先度改善点:
  - {LOW_PRIORITY_IMPROVEMENT_1}
```

### 次回Phase向け推奨事項
- **プロセス改善**: {PROCESS_IMPROVEMENT_RECOMMENDATION}
- **品質向上**: {QUALITY_IMPROVEMENT_RECOMMENDATION}
- **効率化**: {EFFICIENCY_IMPROVEMENT_RECOMMENDATION}

## 📚 参照・関連文書

### 参照した原典仕様
- `/Doc/01_Requirements/要件定義書.md` - Section {REQ_SECTIONS}
- `/Doc/01_Requirements/機能仕様書.md` - Section {FUNC_SECTIONS}
- `/Doc/02_Design/データベース設計書.md` - Table {DB_TABLES}

### 生成された関連文書
- `/Doc/05_Research/Phase_{PHASE_NAME}/Spec_Compliance_Matrix.md`
- `/Doc/05_Research/Phase_{PHASE_NAME}/Implementation_Requirements.md`
- `/Doc/05_Research/Phase_{PHASE_NAME}/Spec_Analysis_Results.md`

### 実行コマンド履歴
```bash
# 実行されたコマンド履歴
{COMMAND_HISTORY}
```

---

**このテンプレートの使用方法**:
1. spec-compliance-check-enhanced Command実行時に自動生成
2. {PLACEHOLDER}部分は実際の値で自動置換
3. 証跡として長期保存（30日以上）
4. 監査・品質レビュー時の根拠資料として活用

**生成コマンド**: `.claude/commands/spec-compliance-check-enhanced.md`
**更新頻度**: Phase完了時・重要修正時