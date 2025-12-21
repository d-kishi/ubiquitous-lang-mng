---
name: tdd-red-green-refactor
description: TDD Red-Green-Refactorサイクルを実践する。「テスト駆動開発」「単体テスト作成」「テストファースト」「リファクタリング」の際に使用する。
allowed-tools: Read, Write, Edit, Bash, Grep, Glob
---

# TDD Red-Green-Refactor Skill

TDD (Test-Driven Development) のRed-Green-Refactorサイクル実践パターン。ADR_009テスト指針準拠（カバレッジ80%以上・Domain層100%）。

## 使用タイミング

1. **新機能実装時** - 新規ドメインモデル・ユースケース・APIエンドポイント
2. **バグ修正時** - バグ再現テスト作成・回帰テスト
3. **リファクタリング時** - テスト網羅確認・構造変更
4. **unit-test Agent起動時** - 指示作成・カバレッジ確認

## Red-Green-Refactorサイクル概要

### Phase: Red（失敗するテストを書く）

**手順**: 要件理解 → テストケース設計 → テストコード作成 → テスト実行（失敗確認）

**チェック**:
- ❌ テストが失敗している（Red状態）
- ✅ テストが実装仕様を明確に表現
- ✅ テストが1つの責務のみをテスト

**詳細**: [`references/red-phase-pattern.md`](./references/red-phase-pattern.md)

---

### Phase: Green（テストを通す最小実装）

**手順**: 最小実装 → テスト実行（Green確認） → カバレッジ確認 → ビルド確認

**チェック**:
- ✅ テストが成功（Green状態）
- ✅ 実装が必要最小限
- ✅ ビルド成功（0 Warning / 0 Error）

**詳細**: [`references/green-phase-pattern.md`](./references/green-phase-pattern.md)

---

### Phase: Refactor（コード品質改善）

**手順**: リファクタリング対象特定 → 実行 → テスト再実行 → Clean Architecture確認

**チェック**:
- ✅ テスト成功維持（Green状態維持）
- ✅ コード品質改善（可読性・保守性）
- ✅ Clean Architecture準拠

**詳細**: [`references/refactor-phase-pattern.md`](./references/refactor-phase-pattern.md)

## カバレッジ目標（ADR_009）

| レイヤー | 目標 |
|---------|------|
| **Domain層（F#）** | 100%（最優先） |
| **Application層（C#）** | 90%以上 |
| **Infrastructure層（C#）** | 70%以上 |
| **全体** | 80%以上 |

## 参照ファイル

| ファイル | 内容 |
|---------|------|
| [`red-phase-pattern.md`](./references/red-phase-pattern.md) | Red Phase詳細パターン |
| [`green-phase-pattern.md`](./references/green-phase-pattern.md) | Green Phase詳細パターン |
| [`refactor-phase-pattern.md`](./references/refactor-phase-pattern.md) | Refactor Phase詳細パターン |
| [`tdd-practices.md`](./references/tdd-practices.md) | unit-test Agent活用・テスタブルコード設計・チェックリスト |

## 関連Skills

- **unit-test SubAgent**: 単体テスト設計・実装・実行
- **clean-architecture-guardian**: Clean Architecture準拠性チェック（Refactor Phase使用）

## 参照元

- **ADR_009**: テスト指針（テストピラミッド・品質基準）
