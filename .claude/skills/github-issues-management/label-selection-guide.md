# ラベル選択ガイド

**目的**: GitHub Issue作成時のラベル判断基準・選択ロジックを提供

**参照元**: `Doc/08_Organization/Rules/GitHub_Issues運用規則.md`

## 📊 ラベル選択の3段階プロセス

```
1. 種別ラベル判断（必須・1つ選択）
   ↓
2. 優先度ラベル判断（必須・1つ選択）
   ↓
3. 影響範囲ラベル判断（任意・複数選択可）
```

---

## 1️⃣ 種別ラベル判断（必須・1つ選択）

### ラベル体系

| ラベル | 説明 | 判断基準 |
|--------|------|---------|
| `tech-debt` | 技術的負債 | コード重複、密結合、保守性低下 |
| `architecture` | アーキテクチャ問題 | 層間依存、設計原則違反、Clean Architecture逸脱 |
| `security` | セキュリティ問題 | 脆弱性、セキュリティ要件未達、認証・認可問題 |
| `performance` | パフォーマンス問題 | 処理速度、メモリ使用量、データベースパフォーマンス |
| `maintainability` | 保守性問題 | 可読性、テスト性、ドキュメント不足 |
| `compliance` | 仕様準拠問題 | 要件・設計書からの逸脱、仕様不整合 |

### 判断フロー

```
問題の性質を特定
  ↓
┌─────────────────────────────────────────┐
│ Q1: 設計原則・Clean Architectureに違反？ │
└─────────────────────────────────────────┘
  YES → `architecture`
  NO  ↓
┌─────────────────────────────────────────┐
│ Q2: セキュリティリスク・脆弱性が存在？   │
└─────────────────────────────────────────┘
  YES → `security`
  NO  ↓
┌─────────────────────────────────────────┐
│ Q3: パフォーマンス劣化・処理速度問題？   │
└─────────────────────────────────────────┘
  YES → `performance`
  NO  ↓
┌─────────────────────────────────────────┐
│ Q4: 要件・設計書との不整合が存在？       │
└─────────────────────────────────────────┘
  YES → `compliance`
  NO  ↓
┌─────────────────────────────────────────┐
│ Q5: 可読性・テスト性・ドキュメント問題？ │
└─────────────────────────────────────────┘
  YES → `maintainability`
  NO  ↓
┌─────────────────────────────────────────┐
│ Q6: コード重複・密結合・保守性低下？     │
└─────────────────────────────────────────┘
  YES → `tech-debt`
  NO  → 種別ラベル不要（bug等の標準ラベル使用）
```

### 具体例

#### 例1: Clean Architecture違反

**問題**: Web層が直接Domain層を参照している

**判断**:
- Q1: 設計原則違反 → YES
- **ラベル**: `architecture`

#### 例2: コード重複

**問題**: 同一ロジックが3箇所に散在

**判断**:
- Q1～Q5: NO
- Q6: コード重複 → YES
- **ラベル**: `tech-debt`

#### 例3: セキュリティ脆弱性

**問題**: SQLインジェクション脆弱性

**判断**:
- Q1: NO
- Q2: セキュリティリスク → YES
- **ラベル**: `security`

#### 例4: 仕様逸脱

**問題**: UI設計書と実装が不一致

**判断**:
- Q1～Q3: NO
- Q4: 設計書との不整合 → YES
- **ラベル**: `compliance`

---

## 2️⃣ 優先度ラベル判断（必須・1つ選択）

### ラベル体系

| ラベル | 説明 | 対応目安 | 判断基準 |
|--------|------|---------|---------|
| `priority/critical` | 緊急対応必要 | 24時間以内 | システム停止・重大セキュリティリスク |
| `priority/high` | 高優先度 | 1週間以内 | 機能不全・ユーザー影響大 |
| `priority/medium` | 中優先度 | 1ヶ月以内 | 保守性低下・将来的リスク |
| `priority/low` | 低優先度 | 次Phase以降 | 改善余地・技術的負債 |

### 判断基準マトリックス

| 評価項目 | Critical | High | Medium | Low |
|---------|----------|------|--------|-----|
| **ユーザー影響** | システム使用不可 | 主要機能不全 | 副次機能影響 | 影響なし |
| **セキュリティリスク** | 重大脆弱性 | 中程度脆弱性 | 軽微リスク | リスクなし |
| **ビジネス影響** | 事業継続不可 | 売上・評判影響 | 運用効率低下 | 影響軽微 |
| **技術的影響** | システム全体 | 複数機能 | 単一機能 | 局所的 |

### 判断フロー

```
影響度を評価
  ↓
┌─────────────────────────────────────────┐
│ システム停止・重大セキュリティリスク？   │
└─────────────────────────────────────────┘
  YES → `priority/critical`
  NO  ↓
┌─────────────────────────────────────────┐
│ 機能不全・ユーザー影響大・1週間以内？   │
└─────────────────────────────────────────┘
  YES → `priority/high`
  NO  ↓
┌─────────────────────────────────────────┐
│ 保守性低下・将来的リスク・1ヶ月以内？   │
└─────────────────────────────────────────┘
  YES → `priority/medium`
  NO  ↓
  `priority/low`（次Phase以降）
```

### 具体例

#### 例1: SQLインジェクション脆弱性

**評価**:
- ユーザー影響: システム使用不可（データ漏洩リスク）
- セキュリティリスク: 重大脆弱性
- **優先度**: `priority/critical`

#### 例2: Clean Architecture違反

**評価**:
- ユーザー影響: なし（内部実装問題）
- 技術的影響: 複数機能（保守性低下）
- **優先度**: `priority/medium`

#### 例3: コード重複

**評価**:
- ユーザー影響: なし
- 技術的影響: 局所的
- **優先度**: `priority/low`

---

## 3️⃣ 影響範囲ラベル判断（任意・複数選択可）

### ラベル体系

| ラベル | 説明 | 選択基準 |
|--------|------|---------|
| `scope/domain` | F# ドメイン層 | Domain層のコード・ロジックに影響 |
| `scope/application` | F# アプリケーション層 | Application層のユースケースに影響 |
| `scope/contracts` | C# Contracts層 | F#↔C#境界・型変換に影響 |
| `scope/infrastructure` | C# Infrastructure層 | データベース・外部サービスに影響 |
| `scope/web` | C# Web層 | UI・Blazor Serverコンポーネントに影響 |
| `scope/tests` | テスト関連 | テストコード・テスト基盤に影響 |
| `scope/docs` | ドキュメント | ドキュメント・設計書に影響 |

### 選択ルール

**原則**: 影響を受けるすべての層を選択（複数選択可）

**例**:
- Domain層のロジック変更 → `scope/domain`
- Domain層変更がApplication層に波及 → `scope/domain`, `scope/application`
- F#↔C#境界の型変換問題 → `scope/contracts`（必要に応じて `scope/domain`, `scope/infrastructure` も）

### 判断フロー

```
影響ファイルを特定
  ↓
┌─────────────────────────────────────────┐
│ 各層への影響を確認（複数選択可）         │
└─────────────────────────────────────────┘
  ├─ src/UbiquitousLanguageManager.Domain/ → `scope/domain`
  ├─ src/UbiquitousLanguageManager.Application/ → `scope/application`
  ├─ src/UbiquitousLanguageManager.Contracts/ → `scope/contracts`
  ├─ src/UbiquitousLanguageManager.Infrastructure/ → `scope/infrastructure`
  ├─ src/UbiquitousLanguageManager.Web/ → `scope/web`
  ├─ tests/ → `scope/tests`
  └─ Doc/ → `scope/docs`
```

### 具体例

#### 例1: Domain層のロジック変更

**影響ファイル**:
- `src/UbiquitousLanguageManager.Domain/Models/User.fs`
- `src/UbiquitousLanguageManager.Application/Users/UserService.fs`

**ラベル**: `scope/domain`, `scope/application`

#### 例2: Web層のUI変更

**影響ファイル**:
- `src/UbiquitousLanguageManager.Web/Components/Pages/Login.razor`

**ラベル**: `scope/web`

#### 例3: F#↔C#境界の型変換問題

**影響ファイル**:
- `src/UbiquitousLanguageManager.Contracts/Users/UserDtoConverter.cs`
- `src/UbiquitousLanguageManager.Domain/Models/User.fs`

**ラベル**: `scope/contracts`, `scope/domain`

---

## 🎯 ラベル組み合わせパターン（よくある例）

### パターン1: Clean Architecture違反

```bash
--label "architecture,priority/medium,scope/domain,scope/application"
```

### パターン2: セキュリティ脆弱性

```bash
--label "security,priority/critical,scope/web,scope/infrastructure"
```

### パターン3: コード重複

```bash
--label "tech-debt,priority/low,scope/application"
```

### パターン4: 仕様逸脱

```bash
--label "compliance,priority/high,scope/web,scope/docs"
```

### パターン5: パフォーマンス問題

```bash
--label "performance,priority/high,scope/infrastructure"
```

---

## ⚠️ 注意事項

### 1. 種別ラベルは必ず1つ選択

**誤**: `--label "tech-debt,architecture"`（複数種別）

**正**: `--label "architecture"`（最も適切な1つ）

### 2. 優先度ラベルは必ず1つ選択

**誤**: `--label "priority/high,priority/medium"`（複数優先度）

**正**: `--label "priority/high"`（最も適切な1つ）

### 3. 影響範囲ラベルは複数選択可

**正**: `--label "scope/domain,scope/application,scope/contracts"`（複数影響範囲）

### 4. 標準ラベル（bug, enhancement等）との併用

運用規則の種別ラベルが適用されない場合、標準ラベルを使用：

**例**: 外部依存のバグ（C# Dev Kit）
```bash
--label "bug"
```

運用規則は「技術的負債・課題管理」用のため、外部依存のバグは対象外。

---

**最終更新**: 2025-11-15
