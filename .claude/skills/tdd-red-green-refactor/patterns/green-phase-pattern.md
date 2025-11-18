# Green Phase実践パターン

## 概要

Green Phaseは、TDDサイクルの2番目のフェーズで、**テストを通す最小実装**を作成する段階です。

## 目的

- テストを通すための最小限のコードを作成する
- テストが成功する（Green状態）ことを確認する
- 過剰実装を避ける

## 実施手順

### 1. 最小実装作成

**実施内容**:
- Red Phaseで作成したテストを通すための最小限のコードを作成
- **過剰実装を避ける**（テストされていないコードは書かない）
- 必要最小限の機能のみ実装

**F# Domain層実装例**:
```fsharp
module UserValidation =
    type Email = Email of string
    type ValidationError = EmptyEmail | InvalidEmailFormat

    let validateEmail (email: string) : Result<Email, ValidationError> =
        // 最小実装: 空文字チェックのみ
        if String.IsNullOrWhiteSpace(email) then
            Error EmptyEmail
        else
            // 最小実装: 有効とみなす（詳細バリデーションは後のテストで追加）
            Ok (Email email)
```

**C# Application層実装例**:
```csharp
public class UserService
{
    private readonly IUserRepository _userRepository;

    public UserService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<RegisterUserResult> RegisterUserAsync(RegisterUserInput input)
    {
        // 最小実装: バリデーションチェック
        if (string.IsNullOrWhiteSpace(input.Email))
        {
            throw new ValidationException("Email is required");
        }

        // 最小実装: ユーザー作成
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = input.Email,
            PasswordHash = HashPassword(input.Password)
        };

        await _userRepository.AddAsync(user);

        return new RegisterUserResult { UserId = user.Id };
    }

    private string HashPassword(string password)
    {
        // 最小実装: パスワードハッシュ化（本格実装は後で）
        return password; // ⚠️ 本番環境では適切なハッシュ化必須
    }
}
```

**チェックポイント**:
- [ ] テストを通すための最小限のコードである
- [ ] 過剰実装していない（テストされていないコードがない）
- [ ] コードが読みやすい（変数名・メソッド名が明確）

### 2. テスト実行（Green状態確認）

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
✅ Passed: ValidateEmail_ValidEmail_ReturnsOk
✅ Passed: ValidateEmail_EmptyEmail_ReturnsError

Total tests: 2
     Passed: 2
     Failed: 0
```

**チェックポイント**:
- [ ] テストが成功している（Green状態）
- [ ] 新規作成したテストが全て成功している
- [ ] 既存テストが全て成功している（回帰なし）

### 3. テストカバレッジ確認

**実行コマンド**:
```bash
# カバレッジ測定
dotnet test --collect:"XPlat Code Coverage"

# カバレッジレポート生成（reportgenerator使用）
reportgenerator \
  -reports:**/coverage.cobertura.xml \
  -targetdir:coverage-report \
  -reporttypes:Html
```

**期待するカバレッジ**:
- **Domain層（F#）**: 100%（最優先）
- **Application層（C#）**: 90%以上
- **新規作成コード**: 100%

**チェックポイント**:
- [ ] 新規コードのテストカバレッジが100%である
- [ ] Domain層のテストカバレッジが100%である
- [ ] 全体のテストカバレッジが80%以上である

### 4. ビルド確認

**実行コマンド**:
```bash
# 全体ビルド
dotnet build

# Warningを詳細表示
dotnet build --verbosity detailed
```

**期待する結果**:
```
Build succeeded.
    0 Warning(s)
    0 Error(s)

Time Elapsed 00:00:05.12
```

**チェックポイント**:
- [ ] ビルドが成功している
- [ ] 0 Warning / 0 Error状態である
- [ ] コンパイラ警告が出ていない

### 5. 既存テスト実行（回帰確認）

**実行コマンド**:
```bash
# 全テスト実行
dotnet test

# テスト結果サマリー表示
dotnet test --logger "console;verbosity=normal"
```

**期待する結果**:
```
Total tests: 150
     Passed: 150
     Failed: 0
   Skipped: 0
```

**チェックポイント**:
- [ ] 既存テストが全て成功している
- [ ] 新規実装が既存機能に影響していない
- [ ] 回帰なし

---

## Green Phase実践例

### 例1: F# Domain層（純粋関数）

**Red Phase**:
```fsharp
[<Fact>]
let ``CreateUser_ValidInput_ReturnsUser`` () =
    // Arrange
    let name = "Test User"
    let email = Email "test@example.com"

    // Act
    let result = UserDomain.createUser name email

    // Assert
    match result with
    | Ok user ->
        user.Name |> should equal name
        user.Email |> should equal email
    | Error _ -> failwith "Expected Ok"
```

**Green Phase（最小実装）**:
```fsharp
module UserDomain =
    type User = { Name: string; Email: Email }
    type DomainError = InvalidUserName

    let createUser (name: string) (email: Email) : Result<User, DomainError> =
        // 最小実装: 空文字チェックのみ
        if String.IsNullOrWhiteSpace(name) then
            Error InvalidUserName
        else
            // 最小実装: ユーザー作成
            Ok { Name = name; Email = email }
```

### 例2: C# Application層（DI使用）

**Red Phase**:
```csharp
[Fact]
public async Task GetUserByIdAsync_ExistingUser_ReturnsUser()
{
    // Arrange
    var userId = Guid.NewGuid();
    var expectedUser = new User { Id = userId, Email = "test@example.com" };
    var userRepository = Substitute.For<IUserRepository>();
    userRepository.GetByIdAsync(userId).Returns(expectedUser);
    var userService = new UserService(userRepository);

    // Act
    var result = await userService.GetUserByIdAsync(userId);

    // Assert
    result.Should().NotBeNull();
    result.Id.Should().Be(userId);
}
```

**Green Phase（最小実装）**:
```csharp
public class UserService
{
    private readonly IUserRepository _userRepository;

    public UserService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<User?> GetUserByIdAsync(Guid userId)
    {
        // 最小実装: リポジトリから取得して返すのみ
        return await _userRepository.GetByIdAsync(userId);
    }
}
```

---

## よくある問題と対策

### 問題1: 過剰実装

**症状**:
```csharp
// ❌ Bad: テストされていない機能を追加している
public async Task<User?> GetUserByIdAsync(Guid userId)
{
    var user = await _userRepository.GetByIdAsync(userId);

    // ❌ テストされていないキャッシュ機能
    _cache.Set(userId, user);

    // ❌ テストされていないロギング機能
    _logger.LogInformation($"User {userId} retrieved");

    return user;
}
```

**対策**:
```csharp
// ✅ Good: テストを通す最小実装
public async Task<User?> GetUserByIdAsync(Guid userId)
{
    // 最小実装: リポジトリから取得して返すのみ
    return await _userRepository.GetByIdAsync(userId);
}

// キャッシュ・ロギング機能は、それぞれのテストを書いてから追加
```

### 問題2: テストが最初からGreen

**症状**:
```
✅ Passed: ValidateEmail_ValidEmail_ReturnsOk
   (実装が既に存在していた)
```

**対策**:
1. 実装を削除してRed状態にする
2. テストが失敗することを確認
3. 最小実装を再作成

### 問題3: 既存テストが失敗（回帰）

**症状**:
```
❌ Failed: ExistingUserTest_SomeScenario_ExpectedResult
   (新規実装が既存機能に影響)
```

**対策**:
1. 新規実装をロールバック
2. 既存テストが成功することを確認
3. 慎重に最小実装を再作成

---

## Green Phase完了チェックリスト

- [ ] テストを通すための最小実装を作成した
- [ ] テストが成功する（Green状態）ことを確認した
- [ ] 新規コードのテストカバレッジが100%である
- [ ] ビルドが成功した（0 Warning / 0 Error）
- [ ] 既存テストが全て成功した（回帰なし）
- [ ] 過剰実装していない（テストされていないコードがない）

---

**次のPhase**: [Refactor Phase](./refactor-phase-pattern.md) - コード品質改善
