# SubAgent実行ガイドライン

## 概要

SubAgent活用効率化と品質向上のための実行ガイドライン。Phase B1 Step3で実証済み。

**基盤ADR**: ADR_016（プロセス遵守）・ADR_018（指示改善とFix-Mode活用）

---

## 🔴 CRITICAL: SubAgent選択マトリックス

### 作業種別による選択

| 作業種別 | 実行パターン |
|---------|-------------|
| **新機能実装** | Domain→Application→Infrastructure→Web |
| **機能拡張** | 影響分析→実装統合→品質保証 |
| **品質改善** | 課題分析→改善実装→検証完成 |
| **エラー修正** | エラー種別判定→Fix-Mode実行 |

### エラー修正時のSubAgent選定

| エラー種別 | 対象SubAgent | 適用条件 |
|----------|-------------|----------|
| F#構文エラー（FS0597等） | fsharp-domain/fsharp-application | Domain/Application層のF#コード |
| C#構文エラー（CS1234等） | contracts-bridge/csharp-infrastructure/csharp-web-ui | Contracts/Infrastructure/Web層 |
| F#↔C#境界エラー | contracts-bridge | TypeConverter・DTO変換関連 |
| テスト関連エラー | unit-test/integration-test | テストプロジェクト全般 |
| ビルド・設定エラー | general-purpose | .csproj/.fsproj・パッケージ関連 |

---

## Fix-Mode実行手順

### Step1: エラー分析（必須）

- [ ] エラーコード（CS1234、FS0597等）の特定
- [ ] エラーメッセージの完全な読み取り
- [ ] 発生ファイル・行番号の確認
- [ ] 影響範囲・修正規模の見積もり

### Step2: SubAgent選定（必須）

上記マトリックスに従い適切なSubAgentを選定。

### Step3: Fix-Mode実行（標準テンプレート）

```markdown
[SubAgent名] Agent, Fix-Mode: [エラー種別]エラーを修正してください。

## 修正対象エラー詳細
**ファイル**: [完全ファイルパス]:[行番号]
**エラーコード**: [CS1234/FS0597等]
**エラーメッセージ**: [ビルド出力からの完全コピー]

## 修正指示
// 修正前（エラー）
[具体的なエラーコード]

// 修正後（正しい）
[期待される正しいコード]

## 重要な制約事項
- **ロジック変更禁止**: 構文エラーの修正のみ実施
- **既存パターン準拠**: 他の同種実装の命名規則に従う

修正完了後、[N]件のエラーが解消されることを確認してください。
```

---

## Fix-Modeチェックリスト

### 実行前（必須）

- [ ] エラー種別の正確な判定完了
- [ ] 適切なSubAgentの選定完了
- [ ] 標準テンプレートの準備完了

### 実行後（必須）

- [ ] SubAgent修正実行の完了確認
- [ ] 修正内容の妥当性確認
- [ ] 全体ビルド成功確認（0 Warning/0 Error）

---

## 効果測定指標

| 指標 | 目標 |
|------|------|
| **Fix-Mode成功率** | 95%以上（一回の指示で完了） |
| **修正時間効率** | 15分以下/9件基準 |
| **ビルド成功率** | 100% |
| **SubAgent責務違反件数** | 0件 |

---

## 関連文書

- **ADR_016**: プロセス遵守違反防止策
- **ADR_018**: SubAgent指示改善とFix-Mode活用
- **Commands**: task-breakdown, step-start, spec-compliance-check
- **Serenaメモリー**: development_guidelines, tech_stack_and_conventions

---

**作成日**: 2025-09-30（Phase B1 Step3実証結果）
