# edit_memory vs 直接ファイル編集 詳細検証計画

**作成日**: 2025-11-16
**目的**: `edit_memory`と「Grep+Read+Edit」方式のContext効率・応答性を公平に比較
**実施予定**: 2025-11-16（Compact後）

---

## 🎯 検証目的

### 主要評価指標

1. **Context効率**：同じ操作でのContext使用量増加の比較
2. **応答性**：体感的な実行速度・待ち時間
3. **正確性**：ファイル変更の正確性
4. **透明性**：処理内容の可視性

### 期待される成果

- session-end/weekly-retrospective Commandsで`edit_memory`採用の可否判断
- Context効率87.5%削減の維持または改善確認
- 応答性向上の定量的確認

---

## 📋 検証計画

### Phase 1: セッション1（方式A: edit_memory）

#### 事前準備
1. ✅ 現在のセッションをCompact（/compact実行）
2. ✅ 本ファイルを読み込み（Read tool）
3. ✅ Context使用量ベースライン記録

#### 実行手順

**Step 1: ベースライン測定**
```bash
# 現在のContext使用量を記録
# system_warningの"Token usage: XXX/200000"を記録
# 記録例：ベースラインA = 50,000 tokens
```

**Step 2: テスト操作実行（edit_memory）**

対象ファイル：`.serena/memories/daily_sessions.md`

操作内容：最後にテストセクション追加
```markdown
## 📅 2025-11-16（土）- 検証テスト方式A

### セッション1: edit_memory検証セッション

**実施環境**: 💻 **ローカル環境（Windows・Claude Code CLI）**

**目的**: edit_memory Context効率測定

**完了事項**:
- Context効率測定
- 応答性測定
- 正確性確認

**測定結果**:
- ベースラインContext: [記録]
- 実行後Context: [記録]
- 増加量: [計算]
- 実行時刻（開始）: [記録]
- 実行時刻（完了）: [記録]
- 所要時間: [計算]

---

**次回記録開始**: 2025-11-17以降のセッション
```

**実行コマンド**:
```
mcp__serena__edit_memory:
  memory_file_name: daily_sessions.md
  regex: (---\n\n\$1)$
  repl: [上記の内容]
```

**Step 3: 実行後測定**
```bash
# 実行完了後のContext使用量を記録
# system_warningの"Token usage: XXX/200000"を記録
# 記録例：実行後A = 52,500 tokens
# 増加量A = 実行後A - ベースラインA = 2,500 tokens
```

**Step 4: 応答性測定**
```bash
# 実行開始時刻を記録（例：14:30:15）
# 実行完了時刻を記録（例：14:30:17）
# 所要時間A = 完了時刻 - 開始時刻 = 2秒
```

**Step 5: 正確性確認**
```bash
# tail -20 .serena/memories/daily_sessions.md
# 追加内容が正しく反映されているか確認
```

**Step 6: git restore**
```bash
git restore .serena/memories/daily_sessions.md
```

#### 測定結果記録フォーマット

```markdown
### Phase 1結果（方式A: edit_memory）

**Context効率**:
- ベースライン: [XXX,XXX] tokens
- 実行後: [XXX,XXX] tokens
- 増加量: **[X,XXX] tokens**

**応答性**:
- 開始時刻: [HH:MM:SS]
- 完了時刻: [HH:MM:SS]
- 所要時間: **[X]秒**

**正確性**: ✅ 正確 / ❌ エラー発生

**備考**: [気づいた点・特記事項]
```

---

### Phase 2: セッション2（方式B: Grep+Read+Edit）

#### 事前準備
1. ✅ Phase 1完了確認
2. ✅ daily_sessions.mdが元の状態に戻っていることを確認
3. ✅ 新しいContext使用量ベースライン記録

#### 実行手順

**Step 1: ベースライン測定**
```bash
# Phase 1のgit restore直後のContext使用量を記録
# 記録例：ベースラインB = 53,000 tokens
```

**Step 2: テスト操作実行（Grep+Read+Edit）**

対象ファイル：`.serena/memories/daily_sessions.md`

操作内容：方式Aと**完全に同じ内容**を追加（方式Bバージョン）
```markdown
## 📅 2025-11-16（土）- 検証テスト方式B

### セッション1: Grep+Read+Edit検証セッション

**実施環境**: 💻 **ローカル環境（Windows・Claude Code CLI）**

**目的**: 直接ファイル編集 Context効率測定

**完了事項**:
- Context効率測定
- 応答性測定
- 正確性確認

**測定結果**:
- ベースラインContext: [記録]
- 実行後Context: [記録]
- 増加量: [計算]
- 実行時刻（開始）: [記録]
- 実行時刻（完了）: [記録]
- 所要時間: [計算]

---

**次回記録開始**: 2025-11-17以降のセッション
```

**実行コマンド**:
```
Step 2-1: Read（部分読み込み）
  file_path: C:\Develop\ubiquitous-lang-mng\.serena\memories\daily_sessions.md
  offset: 534
  limit: 20

Step 2-2: Edit（差分更新）
  file_path: C:\Develop\ubiquitous-lang-mng\.serena\memories\daily_sessions.md
  old_string: ---\n\n$1
  new_string: [上記の内容]
```

**Step 3: 実行後測定**
```bash
# 実行完了後のContext使用量を記録
# 記録例：実行後B = 54,200 tokens
# 増加量B = 実行後B - ベースラインB = 1,200 tokens
```

**Step 4: 応答性測定**
```bash
# 実行開始時刻を記録（例：14:32:45）
# 実行完了時刻を記録（例：14:32:49）
# 所要時間B = 完了時刻 - 開始時刻 = 4秒
```

**Step 5: 正確性確認**
```bash
# tail -20 .serena/memories/daily_sessions.md
# 追加内容が正しく反映されているか確認
```

**Step 6: git restore**
```bash
git restore .serena/memories/daily_sessions.md
```

#### 測定結果記録フォーマット

```markdown
### Phase 2結果（方式B: Grep+Read+Edit）

**Context効率**:
- ベースライン: [XXX,XXX] tokens
- 実行後: [XXX,XXX] tokens
- 増加量: **[X,XXX] tokens**

**応答性**:
- 開始時刻: [HH:MM:SS]
- 完了時刻: [HH:MM:SS]
- 所要時間: **[X]秒**

**正確性**: ✅ 正確 / ❌ エラー発生

**備考**: [気づいた点・特記事項]
```

---

## 📊 結果分析フレームワーク

### Context効率比較

**計算式**:
```
Context効率改善率 = (増加量B - 増加量A) / 増加量B × 100%

例：
- 増加量A（edit_memory）= 2,500 tokens
- 増加量B（Grep+Read+Edit）= 1,200 tokens
- 改善率 = (1,200 - 2,500) / 1,200 × 100% = -108%（方式Aが悪化）

または

- 増加量A（edit_memory）= 1,200 tokens
- 増加量B（Grep+Read+Edit）= 2,500 tokens
- 改善率 = (2,500 - 1,200) / 2,500 × 100% = 52%（方式Aが改善）
```

**判定基準**:
- 改善率 > 50%: edit_memory採用強く推奨
- 改善率 30-50%: edit_memory採用推奨
- 改善率 10-30%: 応答性を考慮して判断
- 改善率 0-10%: 応答性重視ならedit_memory採用
- 改善率 < 0%: 現行方式（Grep+Read+Edit）維持

### 応答性比較

**計算式**:
```
応答性改善率 = (所要時間B - 所要時間A) / 所要時間B × 100%

例：
- 所要時間A（edit_memory）= 2秒
- 所要時間B（Grep+Read+Edit）= 4秒
- 改善率 = (4 - 2) / 4 × 100% = 50%（方式Aが高速）
```

**判定基準**:
- 改善率 > 50%: edit_memory応答性優位
- 改善率 30-50%: edit_memory応答性やや優位
- 改善率 10-30%: 応答性差は小さい
- 改善率 < 10%: 応答性はほぼ同等

### 総合評価マトリックス

| Context効率 | 応答性 | 総合判定 | 推奨方針 |
|------------|--------|----------|----------|
| edit_memory優位 | edit_memory優位 | ⭐⭐⭐ 強く推奨 | session-end/weekly-retrospective全面移行 |
| edit_memory優位 | 同等 | ⭐⭐ 推奨 | session-end/weekly-retrospective全面移行 |
| 同等 | edit_memory優位 | ⭐⭐ 推奨 | session-end/weekly-retrospective全面移行 |
| 同等 | 同等 | ⭐ どちらでも可 | ユーザー好み・保守性で判断 |
| Grep+Read+Edit優位 | edit_memory優位 | △ 要検討 | Context効率vs応答性のトレードオフ判断 |
| edit_memory優位 | Grep+Read+Edit優位 | △ 要検討 | Context効率重視ならedit_memory |
| Grep+Read+Edit優位 | 同等 | ❌ 不採用 | 現行方式維持 |
| Grep+Read+Edit優位 | Grep+Read+Edit優位 | ❌ 不採用 | 現行方式維持 |

---

## 🎯 期待される結果パターン

### パターン1: edit_memory完全勝利
```markdown
Context効率: edit_memory優位（改善率40%）
応答性: edit_memory優位（改善率50%）
→ 結論: session-end/weekly-retrospectiveを即座にedit_memory方式に移行
```

### パターン2: edit_memory応答性優位・Context同等
```markdown
Context効率: 同等（改善率5%）
応答性: edit_memory優位（改善率40%）
→ 結論: 応答性重視でedit_memory採用推奨
```

### パターン3: トレードオフ
```markdown
Context効率: Grep+Read+Edit優位（-30%）
応答性: edit_memory優位（改善率60%）
→ 結論: ユーザー判断（Context効率87.5%削減の維持 vs 応答性向上）
```

### パターン4: 現行方式維持
```markdown
Context効率: Grep+Read+Edit優位（-50%）
応答性: 同等（改善率3%）
→ 結論: 現行方式（Grep+Read+Edit）維持
```

---

## 📝 実施後アクション

### Phase 1, 2完了後

1. **結果記録**:
   - 本ファイルの最後に測定結果を追記
   - Phase 1, 2の結果を並べて記録

2. **分析実施**:
   - Context効率改善率計算
   - 応答性改善率計算
   - 総合評価マトリックス判定

3. **方針決定**:
   - edit_memory採用可否判断
   - session-end/weekly-retrospective Commands更新要否判断

4. **Commands更新（edit_memory採用時）**:
   - `.claude/commands/session-end.md` 更新
   - `.claude/commands/weekly-retrospective.md` 更新
   - 87.5%削減効果の維持確認

5. **ドキュメント記録**:
   - 本ファイルをアーカイブ（検証完了記録として保存）
   - development_guidelines.md に知見追加（必要に応じて）

---

## ⚠️ 注意事項

### 測定時の重要ルール

1. **Context増加のみ測定**:
   - git restoreなどの副次的操作は無視
   - 純粋に「edit_memory」または「Grep+Read+Edit」の増加量のみ測定

2. **同一内容で比較**:
   - 方式Aと方式Bで追加する内容は**完全に同一**にする
   - 文字数・行数・構造が同じであることを確認

3. **ベースラインの正確性**:
   - 各Phase開始前に必ずベースライン記録
   - system_warningの数値を正確にコピー

4. **応答性測定の公平性**:
   - 開始時刻は「ツール実行直前」
   - 完了時刻は「ツール実行結果確認直後」
   - 他の作業を挟まない

### トラブルシューティング

**問題1: regex マッチ失敗**
```bash
# 対処法：ファイルの最後の内容を確認
tail -10 .serena/memories/daily_sessions.md
# regex パターンを実際の内容に合わせて調整
```

**問題2: Context増加量が異常に大きい**
```bash
# 対処法：他のツール実行が混入していないか確認
# 純粋にedit_memory/Editツールのみの影響を測定
```

**問題3: ベースライン記録忘れ**
```bash
# 対処法：Phase最初からやり直す
# ベースラインなしでは正確な測定不可
```

---

## 📋 測定結果記録欄

### Phase 1結果（方式A: edit_memory）

**Context効率**:
- ベースライン: 59,319 tokens
- 実行後: 59,938 tokens
- 増加量: **619** tokens

**応答性**:
- 開始時刻: N/A（体感測定）
- 完了時刻: N/A（体感測定）
- 所要時間: **2-3秒**

**正確性**: ✅ 正確 / ⬜ エラー発生

**備考**:
単一ツール実行のため、応答が速く、Context増加量も最小限

---

### Phase 2結果（方式B: Grep+Read+Edit）

**Context効率**:
- ベースライン: 61,228 tokens
- 実行後: 63,011 tokens
- 増加量: **1,783** tokens

**応答性**:
- 開始時刻: N/A（体感測定）
- 完了時刻: N/A（体感測定）
- 所要時間: **3-4秒**

**正確性**: ✅ 正確 / ⬜ エラー発生

**備考**:
Read+Editの2ステップ実行のため、edit_memoryより若干時間がかかる。Readツールでファイル全体をContextに読み込むため、Context増加量が大きい

---

### 総合分析結果

**Context効率改善率**: **65.3%**
- 計算式: (1,783 - 619) / 1,783 × 100% = 65.3%
- 判定: ✅ edit_memory優位 / ⬜ 同等 / ⬜ Grep+Read+Edit優位

**応答性改善率**: **28.6%**
- 計算式: (3.5 - 2.5) / 3.5 × 100% ≈ 28.6%
- 判定: ⬜ edit_memory優位 / ✅ 同等（応答性差は小さい） / ⬜ Grep+Read+Edit優位

**総合評価**: ⬜ ⭐⭐⭐ 強く推奨 / ✅ ⭐⭐ 推奨 / ⬜ ⭐ どちらでも可 / ⬜ △ 要検討 / ⬜ ❌ 不採用

**最終判定**:
✅ edit_memory方式に移行
⬜ 現行方式（Grep+Read+Edit）維持
⬜ 追加検証が必要

**理由**:
Context効率で65.3%の大幅改善を達成。これは検証計画の「改善率 > 50%: edit_memory採用強く推奨」基準を満たす。応答性も28.6%改善しており、実質的に同等以上。session-end/weekly-retrospective Commandsにおいて、Context効率87.5%削減効果をさらに向上させる可能性が高い。ただし、Commands更新後は実運用での効果測定が必要。

---

**検証実施者**: Claude Code
**検証完了日**: 2025-11-16
**次回アクション**: Commands更新（session-end.md / weekly-retrospective.md）
