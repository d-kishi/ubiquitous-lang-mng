# Issueテンプレート活用パターン

**目的**: GitHub Issue作成時のテンプレート活用パターンを提供

**参照元**: `Doc/08_Organization/Rules/GitHub_Issues運用規則.md`

## 📋 基本テンプレート構造

運用規則で定義されている基本テンプレート:

```markdown
# [ISSUE-TYPE-XXX] 課題タイトル

## 📋 基本情報
- **発見日**: 2025-08-17
- **発見Phase**: Phase A6レビュー
- **発見者**: ユーザー/Claude Code
- **種別**: [tech-debt/architecture/security/performance/maintainability/compliance]
- **優先度**: [critical/high/medium/low]

## 🔍 問題詳細

### 現状
現在の問題状況を具体的に記述

### 期待される状態
あるべき姿・目標状態

### 発生原因
根本原因の分析結果

## 📊 影響範囲

### 影響ファイル
- `src/path/to/file1.cs`
- `src/path/to/file2.razor`

### 関連機能
- 機能A
- 機能B

### リスク
対応しない場合のリスク評価

## 🛠️ 対応方針

### 解決アプローチ
具体的な解決方法・手順

### 必要なリソース
- 作業時間見積もり
- 必要な技術調査
- 外部依存

## 📚 Claude Code対応時の必須読み込み情報

### 必須ファイル
- [ ] `/Doc/01_Requirements/要件定義書.md`
- [ ] `/Doc/02_Design/システム設計書.md`
- [ ] `/Doc/07_Decisions/ADR_XXX.md`

### 推奨ファイル
- [ ] `/Doc/02_Design/UI設計/*.md`
- [ ] `/src/UbiquitousLanguageManager.*/Program.cs`

### 調査コマンド
```bash
# 影響範囲調査
find src -name "*.cs" | xargs grep "PatternToSearch"

# 依存関係確認
dotnet list package --include-transitive
```

## ✅ 完了チェックリスト

### 調査・分析
- [ ] 問題の根本原因特定
- [ ] 影響範囲の詳細調査
- [ ] 解決方針の検討・決定

### 実装
- [ ] コード修正・リファクタリング
- [ ] テストケース追加・修正
- [ ] ドキュメント更新

### 検証
- [ ] 修正内容の動作確認
- [ ] 回帰テスト実行
- [ ] コードレビュー完了

### 完了処理
- [ ] 関連PRのマージ
- [ ] ADR更新（必要に応じて）
- [ ] 次Phaseへの申し送り事項整理
```

---

## 🎨 種別ごとのカスタマイズパターン

### 1. tech-debt（技術的負債）

**特徴**: コード重複・密結合・保守性低下

**カスタマイズポイント**:

```markdown
## 🔍 問題詳細

### 現状
- コード重複箇所: 3ファイル（具体的なファイルパス）
- 重複行数: 約150行
- 保守性への影響: 修正時に3箇所すべての更新が必要

### 期待される状態
- 共通ロジックの抽出・統一
- 単一責任の原則に基づく設計

### 発生原因
- 初期実装時の時間的制約
- リファクタリング機会の不足
```

### 2. architecture（アーキテクチャ問題）

**特徴**: 層間依存・設計原則違反・Clean Architecture逸脱

**カスタマイズポイント**:

```markdown
## 🔍 問題詳細

### 現状
- 違反内容: Web層が直接Domain層を参照
- Clean Architecture原則: Dependency Rule違反
- 影響: 依存関係の逆転・テスタビリティ低下

### 期待される状態
- Web層 → Contracts層 → Application層 → Domain層の依存関係
- Dependency Ruleの厳格遵守

### 発生原因
- Clean Architecture理解不足
- ADR_019（namespace設計規約）の不徹底
```

**必須参照ADR**:
- ADR_019（namespace設計規約）
- ADR_020（テストアーキテクチャ決定）

### 3. security（セキュリティ問題）

**特徴**: 脆弱性・セキュリティ要件未達・認証/認可問題

**カスタマイズポイント**:

```markdown
## 🔍 問題詳細

### 現状
- 脆弱性タイプ: SQLインジェクション
- 影響範囲: ユーザー管理機能
- CVE番号（該当時）: CVE-2025-XXXXX
- 脆弱性スコア（CVSS）: 7.5（High）

### 期待される状態
- パラメータ化クエリの使用
- 入力値の適切なサニタイゼーション

### 発生原因
- 動的SQL生成の使用
- セキュリティレビュー不足
```

**優先度**: 常に `priority/critical` または `priority/high`

### 4. performance（パフォーマンス問題）

**特徴**: 処理速度・メモリ使用量・データベースパフォーマンス

**カスタマイズポイント**:

```markdown
## 🔍 問題詳細

### 現状
- 問題箇所: ユーザー一覧取得API
- 処理時間: 5秒（目標: 1秒以内）
- 原因: N+1クエリ問題（1 + 100クエリ実行）

### 期待される状態
- 処理時間: 1秒以内
- クエリ数: 1クエリ（Eager Loading活用）

### 発生原因
- Entity Framework Coreの遅延読み込み
- Includeメソッド未使用
```

**測定データ**: 処理時間・メモリ使用量・クエリ数を具体的に記載

### 5. maintainability（保守性問題）

**特徴**: 可読性・テスト性・ドキュメント不足

**カスタマイズポイント**:

```markdown
## 🔍 問題詳細

### 現状
- 問題箇所: UserService.fs（400行）
- 可読性問題: 関数が長すぎる（100行超）・コメント不足
- テスト性問題: 副作用が多く単体テスト困難

### 期待される状態
- 関数サイズ: 30行以内（単一責任の原則）
- コメント: 複雑ロジックに詳細コメント
- 副作用分離: Railway-oriented Programming活用

### 発生原因
- リファクタリング機会の不足
- ADR_010（実装規約）の不徹底
```

### 6. compliance（仕様準拠問題）

**特徴**: 要件・設計書からの逸脱・仕様不整合

**カスタマイズポイント**:

```markdown
## 🔍 問題詳細

### 現状
- 逸脱内容: ログイン画面のUI構成がUI設計書と不一致
- 設計書記載: パスワード変更モーダル形式
- 実装状態: 独立画面遷移形式

### 期待される状態
- UI設計書（Doc/02_Design/UI設計/ログイン画面.md）完全準拠
- モーダル形式でのパスワード変更実装

### 発生原因
- UI設計書の確認不足
- 仕様準拠チェック（spec-compliance-check）未実施
```

**必須参照**:
- 要件定義書・機能仕様書・UI設計書

---

## 🎯 必須セクション vs 任意セクション

### 必須セクション（すべてのIssueで記載）

- ✅ **基本情報**: 発見日・Phase・種別・優先度
- ✅ **問題詳細**: 現状・期待される状態・発生原因
- ✅ **影響範囲**: 影響ファイル・関連機能・リスク
- ✅ **対応方針**: 解決アプローチ

### 任意セクション（必要に応じて記載）

- 📋 **Claude Code対応時の必須読み込み情報**: Claude Code対応時のみ
- 📋 **調査コマンド**: 技術調査が必要な場合のみ
- 📋 **完了チェックリスト**: 複雑な対応が必要な場合のみ

---

## 🚀 GitHub CLI活用パターン

### パターン1: HEREDOCでテンプレート記述

```bash
gh issue create \
  --title "[ARCH-001] Web層が直接Domain層を参照" \
  --label "architecture,priority/medium,scope/domain,scope/web" \
  --body "$(cat <<'EOF'
## 📋 基本情報
- **発見日**: 2025-11-15
- **発見Phase**: Phase B-F2 Step6
- **発見者**: Claude Code
- **種別**: architecture
- **優先度**: medium

## 🔍 問題詳細

### 現状
Web層が直接Domain層を参照している（Dependency Rule違反）

### 期待される状態
Web層 → Contracts層 → Application層 → Domain層の依存関係

### 発生原因
Clean Architecture理解不足・ADR_019不徹底

## 📊 影響範囲

### 影響ファイル
- `src/UbiquitousLanguageManager.Web/Components/Pages/Login.razor`
- `src/UbiquitousLanguageManager.Domain/Models/User.fs`

### リスク
依存関係の逆転・テスタビリティ低下

## 🛠️ 対応方針

### 解決アプローチ
1. Contracts層にUserDtoConverter追加
2. Web層の参照をContracts層に変更
3. Application層経由でDomain層にアクセス

EOF
)"
```

### パターン2: Markdownファイルからテンプレート読み込み

```bash
# テンプレートファイル作成
cat > issue_template.md <<'EOF'
## 📋 基本情報
...
EOF

# Issue作成
gh issue create \
  --title "[ISSUE-TITLE]" \
  --label "architecture,priority/medium" \
  --body "$(cat issue_template.md)"
```

---

## 📚 Claude Code対応時の必須読み込み情報（詳細）

### 必須ファイルの選択基準

| 種別 | 必須ファイル |
|------|-------------|
| `tech-debt` | ADR_010（実装規約）・該当層の設計書 |
| `architecture` | ADR_019（namespace設計）・ADR_020（テストアーキテクチャ）・システム設計書 |
| `security` | セキュリティ要件書・該当機能の仕様書 |
| `performance` | パフォーマンス要件書・データベース設計書 |
| `maintainability` | ADR_010（実装規約）・コーディング規約 |
| `compliance` | 要件定義書・機能仕様書・UI設計書 |

### 調査コマンドのパターン

#### パターン1: 影響範囲調査

```bash
# ファイル内検索
find src -name "*.cs" | xargs grep "PatternToSearch"

# シンボル参照検索（Serena MCP）
mcp__serena__find_referencing_symbols --name_path "SymbolName" --relative_path "path/to/file"
```

#### パターン2: 依存関係確認

```bash
# NuGetパッケージ依存関係
dotnet list package --include-transitive

# プロジェクト参照確認
dotnet list reference
```

#### パターン3: パフォーマンス測定

```bash
# ビルド時間測定
time dotnet build

# テスト実行時間測定
time dotnet test
```

---

## ⚠️ 注意事項

### 1. テンプレートの柔軟な活用

**原則**: 基本テンプレートを必ず使用

**カスタマイズ**: 種別ごとの最適化パターンを適用

**省略禁止**: 必須セクションは必ず記載

### 2. 具体性の重視

**NG**: 「コードが汚い」「動作が遅い」（抽象的）

**OK**: 「UserService.fsが400行で可読性低下」「処理時間5秒（目標: 1秒以内）」（具体的）

### 3. 影響範囲の明確化

**必須**: 影響ファイル・関連機能の具体的な列挙

**推奨**: ファイルパスの完全記載（相対パス）

---

**最終更新**: 2025-11-15
