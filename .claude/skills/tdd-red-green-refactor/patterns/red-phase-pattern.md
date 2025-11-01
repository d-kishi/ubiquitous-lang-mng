# Red Phase実践パターン

## 概要

Red Phaseは、TDDサイクルの最初のフェーズで、**失敗するテストを書く**段階です。

## 目的

- 実装すべき機能仕様を明確化する
- テストケース（正常系・異常系・境界値）を洗い出す
- テストが最初に失敗する（Red状態）ことを確認する

## 実施手順

### 1. 要件理解

**実施内容**:
- 実装すべき機能仕様を明確に理解する
- 仕様書（`Doc/01_Requirements/`）を参照する
- ユーザーストーリー・受け入れ基準を確認する

**チェックポイント**:
- [ ] 機能仕様を文章で説明できる
- [ ] 入力・出力が明確である
- [ ] 制約条件・バリデーションルールが明確である

### 2. テストケース設計

**実施内容**:
- 正常系テストケース洗い出し
- 異常系テストケース洗い出し
- 境界値テストケース洗い出し

**正常系**:
```
例: ユーザー登録
- 有効なメールアドレス・パスワードでユーザー登録成功
- 登録後、ユーザーIDが発行される
```

**異常系**:
```
例: ユーザー登録
- 空のメールアドレスでユーザー登録失敗
- 無効なメールアドレスでユーザー登録失敗
- 短すぎるパスワードでユーザー登録失敗
- 既存ユーザーと重複するメールアドレスでユーザー登録失敗
```

**境界値**:
```
例: ユーザー登録
- パスワード最小長（8文字）でユーザー登録成功
- パスワード最大長（100文字）でユーザー登録成功
- パスワード7文字でユーザー登録失敗
- パスワード101文字でユーザー登録失敗
```

**チェックポイント**:
- [ ] 正常系テストケースが1個以上ある
- [ ] 異常系テストケースが2個以上ある
- [ ] 境界値テストケースがある

### 3. テストコード作成

**F# Domain層テスト**:
```fsharp
module UserValidationTests =
    open Xunit
    open FsUnit.Xunit

    [<Fact>]
    let ``ValidateEmail_ValidEmail_ReturnsOk`` () =
        // Arrange
        let email = "test@example.com"

        // Act
        let result = UserValidation.validateEmail email

        // Assert
        result |> should be (instanceOfType<Result<Email, ValidationError>>)
        match result with
        | Ok _ -> ()
        | Error _ -> failwith "Expected Ok"

    [<Fact>]
    let ``ValidateEmail_EmptyEmail_ReturnsError`` () =
        // Arrange
        let email = ""

        // Act
        let result = UserValidation.validateEmail email

        // Assert
        match result with
        | Error EmptyEmail -> ()
        | _ -> failwith "Expected Error EmptyEmail"
```

**C# Application層テスト**:
```csharp
using Xunit;
using FluentAssertions;
using NSubstitute;

public class UserServiceTests
{
    [Fact]
    public async Task RegisterUserAsync_ValidInput_ReturnsUserId()
    {
        // Arrange
        var userRepository = Substitute.For<IUserRepository>();
        var userService = new UserService(userRepository);
        var input = new RegisterUserInput("test@example.com", "Password123!");

        // Act
        var result = await userService.RegisterUserAsync(input);

        // Assert
        result.Should().NotBeNull();
        result.UserId.Should().NotBeEmpty();
    }

    [Fact]
    public async Task RegisterUserAsync_EmptyEmail_ThrowsValidationException()
    {
        // Arrange
        var userRepository = Substitute.For<IUserRepository>();
        var userService = new UserService(userRepository);
        var input = new RegisterUserInput("", "Password123!");

        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(
            async () => await userService.RegisterUserAsync(input)
        );
    }
}
```

**テストメソッド命名規則**:
```
[TestMethod]_[Scenario]_[ExpectedResult]

例:
- ValidateEmail_ValidEmail_ReturnsOk
- ValidateEmail_EmptyEmail_ReturnsError
- RegisterUserAsync_ValidInput_ReturnsUserId
- RegisterUserAsync_EmptyEmail_ThrowsValidationException
```

**チェックポイント**:
- [ ] テストメソッド名が明確である
- [ ] Arrange-Act-Assert構造になっている
- [ ] テストが1つの責務のみをテストしている

### 4. テスト実行（Red状態確認）

**実行コマンド**:
```bash
# 全テスト実行
dotnet test

# 特定テストクラスのみ実行
dotnet test --filter "FullyQualifiedName~UserValidationTests"

# 詳細出力
dotnet test --logger "console;verbosity=detailed"
```

**期待する結果**:
```
❌ Failed: ValidateEmail_ValidEmail_ReturnsOk
   Expected: Ok
   Actual: (実装が未作成のため例外)

❌ Failed: ValidateEmail_EmptyEmail_ReturnsError
   Expected: Error EmptyEmail
   Actual: (実装が未作成のため例外)
```

**チェックポイント**:
- [ ] テストが失敗している（Red状態）
- [ ] 失敗理由が「実装が未作成」である
- [ ] テストコードに構文エラーがない

### 5. 失敗理由確認

**確認事項**:
- テストが失敗している理由が「期待する実装が未作成」であることを確認
- テストコード自体にバグがないことを確認

**典型的な問題**:
- ❌ テストが最初からGreen（実装が先行している）
  - **対策**: 実装を削除してRed状態にする
- ❌ テストコードに構文エラー
  - **対策**: テストコードを修正する
- ❌ テストが何もアサーションしていない
  - **対策**: アサーションを追加する

---

## Red Phase完了チェックリスト

- [ ] 要件仕様を明確に理解した
- [ ] テストケース（正常系・異常系・境界値）を洗い出した
- [ ] テストコードを作成した
- [ ] テストメソッド名が明確である（`[TestMethod]_[Scenario]_[ExpectedResult]`形式）
- [ ] テストが失敗する（Red状態）ことを確認した
- [ ] 失敗理由が「実装が未作成」であることを確認した
- [ ] テストコードに構文エラーがない

---

**次のPhase**: [Green Phase](./green-phase-pattern.md) - テストを通す最小実装
