# TDD実践詳細ガイド

## 概要

unit-test Agent活用・テスタブルコード設計・テストカバレッジ管理・チェックリストの詳細ガイド。

## 目次

- [unit-test Agent活用パターン](#unit-test-agent活用パターン)
- [テスタブルコード設計原則](#テスタブルコード設計原則)
- [テストカバレッジ管理方法](#テストカバレッジ管理方法)
- [TDDサイクル実践チェックリスト](#tddサイクル実践チェックリスト)

---

## unit-test Agent活用パターン

### unit-test Agent起動判断

**起動すべき状況**:
- 新規テストクラス作成時（3個以上のテストメソッド必要時）
- 既存テストクラス拡張時（5個以上のテストメソッド追加時）
- テストカバレッジ不足時（80%未満）

**MainAgentで直接実施すべき状況**:
- 単純なテストメソッド1-2個追加時
- テストメソッド名修正のみ
- アサーション修正のみ

### unit-test Agentへの指示テンプレート

```markdown
unit-test Agent, 以下のテスト作成をお願いします：

**対象**: [テスト対象クラス・メソッド名]
**テストケース**:
1. 正常系: [シナリオ]
2. 異常系: [シナリオ]
3. 境界値: [シナリオ]

**期待するテストカバレッジ**: [%]
**参照ADR**: ADR_009テスト指針
**参照Skill**: tdd-red-green-refactor
```

---

## テスタブルコード設計原則

### 1. 依存性注入（DI）

**F# Domain層**:
```fsharp
// ✅ Good: 純粋関数（外部依存なし）
module UserValidation =
    let validateEmail (email: string) : Result<Email, ValidationError> =
        if String.IsNullOrWhiteSpace(email) then
            Error EmptyEmail
        else
            Ok (Email email)
```

**C# Application層**:
```csharp
// ✅ Good: コンストラクタインジェクション
public class UserService
{
    private readonly IUserRepository _userRepository;

    public UserService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }
}
```

### 2. インターフェース分離

```csharp
// ✅ Good: インターフェース定義
public interface IUserRepository
{
    Task<User?> GetByIdAsync(Guid id);
    Task AddAsync(User user);
}

// ✅ Good: テスト時はNSubstituteでモック作成
```

### 3. F#純粋関数の活用

```fsharp
// ✅ Good: 純粋関数（副作用なし・テスト容易）
module UserDomain =
    let createUser (name: string) (email: Email) : Result<User, DomainError> =
        if String.IsNullOrWhiteSpace(name) then
            Error InvalidUserName
        else
            Ok { Name = name; Email = email }
```

---

## テストカバレッジ管理方法

### カバレッジ目標（ADR_009準拠）

| レイヤー | 目標 |
|---------|------|
| **全体** | 80%以上 |
| **Domain層（F#）** | 100%（最優先） |
| **Application層（C#）** | 90%以上 |
| **Infrastructure層（C#）** | 70%以上 |
| **Web層（Blazor Server）** | 50%以上（UI統合テストで補完） |

### カバレッジ測定コマンド

```bash
# カバレッジ測定
dotnet test --collect:"XPlat Code Coverage"

# カバレッジレポート生成（reportgenerator使用）
reportgenerator \
  -reports:**/coverage.cobertura.xml \
  -targetdir:coverage-report \
  -reporttypes:Html
```

### カバレッジ不足時の対応

| カバレッジ不足 | 対応 |
|--------------|------|
| **Domain層 < 100%** | 🔴 重大: 即座にテスト追加（最優先）|
| **Application層 < 90%** | 🟡 警告: 次Step開始前にテスト追加 |
| **Infrastructure層 < 70%** | 🟡 警告: Phase完了前にテスト追加 |

---

## TDDサイクル実践チェックリスト

### Red Phase チェックリスト

- [ ] 要件仕様を明確に理解した
- [ ] テストケース（正常系・異常系・境界値）を洗い出した
- [ ] テストメソッド名が明確である（`[TestMethod]_[Scenario]_[ExpectedResult]`形式）
- [ ] テストが失敗する（Red状態）ことを確認した
- [ ] 失敗理由が「実装が未作成」であることを確認した

### Green Phase チェックリスト

- [ ] テストを通すための最小実装を作成した
- [ ] テストが成功する（Green状態）ことを確認した
- [ ] 新規コードのテストカバレッジが100%である
- [ ] ビルドが成功した（0 Warning / 0 Error）
- [ ] 既存テストが全て成功した（回帰なし）

### Refactor Phase チェックリスト

- [ ] リファクタリング対象を特定した
- [ ] リファクタリングを実行した
- [ ] テストが成功する（Green状態維持）ことを確認した
- [ ] ビルドが成功した（0 Warning / 0 Error）
- [ ] Clean Architecture準拠を確認した（clean-architecture-guardian Skill使用）
- [ ] コード品質が改善した（可読性・保守性向上）

---

**作成日**: 2025-12-21
**抽出元**: SKILL.md Phase B-F2 Step2
