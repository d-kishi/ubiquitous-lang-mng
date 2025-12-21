# 仕様準拠ワークフロー詳細

## 概要

仕様準拠チェックの詳細手順・仕様準拠率維持・仕様逸脱リスク特定のためのガイドライン。

## 目次

- [spec-compliance-check Command活用パターン](#spec-compliance-check-command活用パターン)
- [仕様書参照方法](#仕様書参照方法)
- [仕様準拠率95%維持手順](#仕様準拠率95維持手順)
- [仕様逸脱リスク特定方法](#仕様逸脱リスク特定方法)
- [仕様準拠チェックリスト](#仕様準拠チェックリスト)

---

## spec-compliance-check Command活用パターン

### Command実行タイミング

**必須実行**:
- Step完了時（step-end-review Command内で自動実行）
- Phase完了時（phase-end Command内で自動実行）
- 仕様書更新直後

**推奨実行**:
- 新機能実装完了時
- バグ修正完了時
- リファクタリング完了時

### Command実行方法

```bash
# spec-compliance-check Command実行
/spec-compliance-check

# 実行内容:
# 1. Doc/01_Requirements/配下の仕様書全ファイル読み込み
# 2. 実装コードと仕様書の照合
# 3. 仕様逸脱リスクの特定
# 4. 仕様準拠率算出
# 5. 改善提案レポート作成
```

---

## 仕様書参照方法

### 仕様書の構成（Doc/01_Requirements/）

```
Doc/01_Requirements/
├── 機能仕様書.md           # 機能要件（最重要）
├── 非機能要件定義書.md      # 非機能要件
├── データベース設計書.md    # データ整合性要件
└── UI_UX仕様書.md          # UI/UX要件
```

### 仕様書の読み方

**手順**:
1. ✅ **機能特定**: 実装する機能の仕様書項番を特定
2. ✅ **仕様精読**: 肯定的仕様・否定的仕様を抽出
3. ✅ **ビジネスルール理解**: 制約条件・バリデーションルールを理解
4. ✅ **テストケース設計**: 仕様に基づくテストケース洗い出し

### 仕様書項番の記録ルール

```csharp
// 仕様書2.1.1準拠: ログイン機能
// ビジネスルール: ログイン失敗によるロックアウト機構は設けない
public async Task<IActionResult> Login(LoginModel model)
{
    // 実装内容
}
```

```fsharp
// 仕様書3.2.1準拠: プロジェクト作成機能
// 必須項目: プロジェクト名、説明
let createProject (name: ProjectName) (description: Description) =
    // 実装内容
```

---

## 仕様準拠率95%維持手順

### 仕様準拠率算出方法

```
仕様準拠率 = (実装済み仕様項目数 / 全仕様項目数) × 100

例:
- 全仕様項目数: 100項目
- 実装済み仕様項目数: 97項目
- 仕様準拠率: 97%
```

### 仕様準拠率低下時の対応

**95%未満の場合**:
1. 🔴 **緊急対応**: 即座に仕様逸脱箇所を特定
2. 🔴 **修正実施**: 仕様に準拠するよう修正
3. 🔴 **再測定**: 仕様準拠率が95%以上になることを確認

**90%未満の場合**:
1. 🔴 **重大**: Phase進行停止
2. 🔴 **全面見直し**: 全実装を仕様書と照合
3. 🔴 **再設計検討**: 必要に応じて再設計

### 仕様準拠率維持のベストプラクティス

1. **実装前の仕様精読**（最重要）
   - 仕様書を3回読む
   - 肯定的仕様・否定的仕様を明確化
   - 不明点は即座に質問

2. **仕様書項番の記録**
   - すべての実装に仕様書項番をコメント
   - トレーサビリティ確保

3. **定期的な仕様準拠確認**
   - Step完了時に必ず確認
   - Phase完了時に監査

---

## 仕様逸脱リスク特定方法

### 仕様逸脱の典型パターン

#### パターン1: 仕様書に記載のない独自機能追加

**例**:
```csharp
// ❌ 仕様逸脱: 仕様書に記載のないキャッシュ機能を追加
public async Task<User?> GetUserByIdAsync(Guid userId)
{
    // 仕様書に記載なし
    if (_cache.TryGetValue(userId, out User cachedUser))
    {
        return cachedUser;
    }

    var user = await _userRepository.GetByIdAsync(userId);
    _cache.Set(userId, user);
    return user;
}
```

**対策**:
- 仕様書を再確認
- キャッシュ機能が本当に必要か検討
- 必要であれば仕様書を更新してから実装

#### パターン2: 否定的仕様の無視

**例**:
```csharp
// ❌ 仕様逸脱: 「ログイン失敗によるロックアウト機構は設けない」を無視
public async Task<LoginResult> LoginAsync(string email, string password)
{
    var user = await _userRepository.GetByEmailAsync(email);

    if (user.FailedLoginCount >= 5) // 仕様書で禁止されている
    {
        return LoginResult.LockedOut();
    }

    // ...
}
```

**対策**:
- 否定的仕様を明確に理解
- テストで否定的仕様違反を検出

#### パターン3: ビジネスルールの誤実装

**例**:
```csharp
// ❌ 仕様逸脱: パスワード最小長が仕様と異なる
public bool IsPasswordValid(string password)
{
    return password.Length >= 6; // 仕様書では8文字以上
}
```

**対策**:
- ビジネスルールを定数化
- 仕様書項番をコメントで明記

---

## 仕様準拠チェックリスト

### Step開始時

- [ ] 該当機能の仕様書項番を特定した
- [ ] 仕様書を3回精読した
- [ ] 肯定的仕様を抽出した
- [ ] 否定的仕様を抽出した
- [ ] ビジネスルールを理解した
- [ ] テストケースを仕様に基づいて設計した

### 実装中

- [ ] 仕様書項番をコメントで記録した
- [ ] ビジネスルールを定数化した
- [ ] 仕様に記載のない独自機能を追加していない
- [ ] 否定的仕様を遵守している

### Step完了時

- [ ] spec-compliance-check Command実行完了
- [ ] 仕様準拠率95%以上達成
- [ ] 仕様逸脱箇所を全て修正
- [ ] 全テストが成功している

---

**作成日**: 2025-12-21
**抽出元**: SKILL.md Phase B-F2 Step2
