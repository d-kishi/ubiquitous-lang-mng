# 組織管理運用マニュアル（圧縮版）

**目的**: SubAgentプール活用による組織運用の実行ガイド
**適用範囲**: 全Phase・全Step

---

## 🔴 プロセス遵守チェックリスト（ADR_016準拠・統合版）

### 必須確認事項
- **コマンド = 契約**: 一字一句を法的契約として遵守
- **承認 = 必須**: 「ユーザー承認」表記があれば例外なく取得
- **手順 = 聖域**: 定められた順序の勝手な変更を絶対禁止

### 統合チェックリスト（Phase/Step/SubAgent共通）
```markdown
## 🔴 必須確認（全タイミング共通）
- [ ] コマンドの完全読み込み・理解完了
- [ ] 承認取得前の作業開始禁止確認
- [ ] Task/Agent Toolの実際の呼び出し実行確認
- [ ] 成果物ファイルの物理的存在確認完了
- [ ] 報告内容と実体ファイルの完全一致確認完了
```

---

## 🚀 SubAgent活用実行ガイド

### Phase開始準備
1. **phase-start Command実行** → Phase全体準備・ディレクトリ作成・Phase_Summary.md作成
2. **Phase開始承認** → ユーザーによるPhase実行開始の承認

### Step実行サイクル（全Step共通）

**開始時**:
1. step-start Command実行 → Step準備・組織設計・SubAgent選択
2. SubAgent並列実行 → ユーザー承認後のSubAgent専門作業並列実行

**実行中（TDDサイクル）**:
- Red → Green → Refactor の各Phase確認
- 設計変更・技術負債発見時はユーザー承認取得

**終了時**:
1. step-end-review Command実行 → 包括的品質確認
2. spec-compliance-check Command実行 → 仕様準拠監査
3. テストアーキテクチャ整合性確認（ADR_020準拠）
4. ユーザー承認取得

### Phase完了時
1. phase-end Command実行 → Phase総括・品質確認・次Phase移行準備
2. ユーザーレビュー・承認 → Active → Completed移動

---

## 🔧 SubAgent並列実行ガイド

### 必須実行方法
- 同一メッセージ内で複数Task tool呼び出し（並列実行）
- 依存関係のないタスクのみ並列化

### SubAgent選択ガイドライン

| レイヤー | SubAgent |
|---------|----------|
| F# Domain層 | fsharp-domain |
| F# Application層 | fsharp-application |
| F#/C#境界 | contracts-bridge |
| C# Infrastructure層 | csharp-infrastructure |
| C# Web UI層 | csharp-web-ui |
| TypeScript E2Eテスト | e2e-test |
| 単体テスト | unit-test |
| 統合テスト | integration-test |

---

## 🔧 エラー修正時の責務分担原則

**基本原則**: 「エラーが発生した場所」ではなく「エラーの内容」で責務を判定

### MainAgent責務

**✅ 実行可能**: 全体調整・SubAgentへの作業委託・品質確認・プロセス管理・ドキュメント統合

**❌ 禁止**: 実装コードの直接修正・ビジネスロジック追加・型変換ロジック実装・テストコード作成

**例外（直接修正可能）**: 単純なtypo・import文追加・コメント追加・空白調整

### エラー修正フロー
1. エラー内容を分析（影響範囲・修正規模判定）
2. 責務マッピングでSubAgent選定
3. Fix-Mode（軽量実行）での修正委託
4. 修正後品質確認・ビルド確認

---

## 🎭 Playwright Test Agents統合運用

### 技術制約
**SubAgentは他のSubAgentを呼び出せない**（Claude Code公式仕様）

### 統合パターン

**パターンA: MainAgentオーケストレーション型**（新規E2Eテスト・複雑シナリオ時）
```
MainAgent → playwright-test-planner → playwright-test-generator → e2e-test → playwright-test-healer（失敗時）
```

**パターンB: e2e-test Agent単独実行型**（既存テストメンテナンス・単一シナリオ時）
```
MainAgent → e2e-test
```

**詳細**: ADR_024、`.claude/skills/playwright-e2e-patterns/`

---

## 🧠 セッション定義

**セッション**: session-start実行からsession-end実行までの作業全体
- **Context継続**: 同一セッション内の会話継続（新セッションではない）
- **次のセッション**: session-end後に新たに開始されるセッション

### セッション継続判断基準

| Context使用率 | 判断 |
|--------------|------|
| < 80% | 継続（AutoCompact活用） |
| 80-85% | 手動compact検討 |
| ≥ 85% | セッション終了推奨 |

**重要**: Phase/Step境界ではセッション終了を検討

---

## 🔧 Command一覧

| Command | 用途 |
|---------|------|
| phase-start | Phase開始準備 |
| step-start | Step開始準備・SubAgent選択 |
| step-end-review | Step終了時レビュー |
| phase-end | Phase終了総括 |
| spec-compliance-check | 仕様準拠確認 |
| subagent-selection | SubAgent選択・並列実行計画 |

**詳細**: `.claude/commands/` 配下
