# Refactor Phase実践パターン

## 概要

Refactor Phaseは、TDDサイクルの3番目のフェーズで、**コード品質改善**を行う段階です。

## 目的

- コード品質を改善する（可読性・保守性向上）
- テストが成功する（Green状態）ことを維持する
- Clean Architecture準拠を確認する

## 実施手順

### 1. リファクタリング対象特定

**チェック項目**:
- [ ] **重複コード**: 同じロジックが複数箇所にある
- [ ] **長いメソッド**: メソッドが50行以上ある
- [ ] **長いパラメータリスト**: パラメータが4個以上ある
- [ ] **命名改善**: 変数名・メソッド名が不明確
- [ ] **マジックナンバー**: 定数化すべき数値がハードコードされている
- [ ] **コメント依存**: コメントなしでは理解できないコード

**リファクタリング対象例**:
```csharp
// ❌ Bad: 重複コード・長いメソッド・マジックナンバー
public class UserService
{
    public async Task<User?> GetUserByIdAsync(Guid userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null) return null;

        // 重複コード（他のメソッドでも同じロジック）
        if (user.IsFirstLogin && user.CreatedAt < DateTime.UtcNow.AddDays(-30))
        {
            user.IsFirstLogin = false;
            await _userRepository.UpdateAsync(user);
        }

        return user;
    }

    public async Task<List<User>> GetActiveUsersAsync()
    {
        var users = await _userRepository.GetAllAsync();

        // 重複コード（上のメソッドと同じロジック）
        foreach (var user in users)
        {
            if (user.IsFirstLogin && user.CreatedAt < DateTime.UtcNow.AddDays(-30))
            {
                user.IsFirstLogin = false;
                await _userRepository.UpdateAsync(user);
            }
        }

        return users.Where(u => u.IsActive).ToList();
    }
}
```

### 2. リファクタリング実行

**実施内容**:
- 重複コードを抽出してメソッド化
- 長いメソッドを分割
- 命名を改善
- マジックナンバーを定数化

**リファクタリング後**:
```csharp
// ✅ Good: リファクタリング後
public class UserService
{
    private const int FirstLoginExpirationDays = 30;

    public async Task<User?> GetUserByIdAsync(Guid userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null) return null;

        // 重複コードをメソッド抽出
        await UpdateFirstLoginStatusIfExpiredAsync(user);

        return user;
    }

    public async Task<List<User>> GetActiveUsersAsync()
    {
        var users = await _userRepository.GetAllAsync();

        // 重複コードをメソッド抽出
        foreach (var user in users)
        {
            await UpdateFirstLoginStatusIfExpiredAsync(user);
        }

        return users.Where(u => u.IsActive).ToList();
    }

    // リファクタリング: 重複コードを抽出
    private async Task UpdateFirstLoginStatusIfExpiredAsync(User user)
    {
        if (IsFirstLoginExpired(user))
        {
            user.IsFirstLogin = false;
            await _userRepository.UpdateAsync(user);
        }
    }

    // リファクタリング: 判定ロジックを抽出
    private bool IsFirstLoginExpired(User user)
    {
        return user.IsFirstLogin &&
               user.CreatedAt < DateTime.UtcNow.AddDays(-FirstLoginExpirationDays);
    }
}
```

**チェックポイント**:
- [ ] 重複コードが削除されている
- [ ] メソッドが適切な長さである（50行以内）
- [ ] 命名が明確である
- [ ] マジックナンバーが定数化されている

### 3. テスト再実行（Green状態維持確認）

**実行コマンド**:
```bash
# 全テスト実行
dotnet test

# 詳細出力
dotnet test --logger "console;verbosity=detailed"
```

**期待する結果**:
```
Total tests: 150
     Passed: 150
     Failed: 0
   Skipped: 0
```

**チェックポイント**:
- [ ] テストが成功している（Green状態維持）
- [ ] リファクタリング前と同じテスト結果
- [ ] テストカバレッジが維持されている

**重要**: リファクタリング中にテストが失敗した場合は、動作を変更してしまっている証拠です。即座にロールバックして再度慎重に実施してください。

### 4. ビルド確認

**実行コマンド**:
```bash
# 全体ビルド
dotnet build

# Warning詳細表示
dotnet build --verbosity detailed
```

**期待する結果**:
```
Build succeeded.
    0 Warning(s)
    0 Error(s)
```

**チェックポイント**:
- [ ] ビルドが成功している
- [ ] 0 Warning / 0 Error状態である
- [ ] リファクタリングでWarningが増えていない

### 5. Clean Architecture準拠確認

**確認方法**:
- `clean-architecture-guardian` Skill使用
- レイヤー分離原則確認
- 循環依存チェック

**実行例**:
```
clean-architecture-guardian Skill適用:
- ✅ レイヤー分離原則準拠
- ✅ 循環依存なし
- ✅ namespace階層化ルール準拠
- ✅ F# Compilation Order準拠
```

**チェックポイント**:
- [ ] レイヤー分離原則を守っている
- [ ] 循環依存がない
- [ ] namespace階層化ルールを守っている
- [ ] F# Compilation Orderを守っている（F#コードの場合）

### 6. コードレビュー（品質改善確認）

**チェック項目**:
- [ ] **可読性**: コードが読みやすくなった
- [ ] **保守性**: 将来の変更が容易になった
- [ ] **拡張性**: 新機能追加が容易になった
- [ ] **テスト容易性**: テストが書きやすくなった

**コードレビュー観点**:
```
1. DRY原則（Don't Repeat Yourself）
   - [ ] 重複コードが削除されている

2. SOLID原則
   - [ ] 単一責任の原則（SRP）: 1クラス1責務
   - [ ] 開放閉鎖の原則（OCP）: 拡張に開き、修正に閉じている
   - [ ] リスコフの置換原則（LSP）: 継承が適切
   - [ ] インターフェース分離の原則（ISP）: インターフェースが最小
   - [ ] 依存性逆転の原則（DIP）: 抽象に依存

3. 命名規則
   - [ ] クラス名・メソッド名が明確
   - [ ] 変数名が意図を表現している
   - [ ] bool変数はis/has/canで始まる
```

---

## リファクタリングパターン集

### パターン1: メソッド抽出（Extract Method）

**Before**:
```csharp
public async Task ProcessOrderAsync(Order order)
{
    // 長いメソッド
    if (order.Items.Count == 0)
        throw new ValidationException("Order must have at least one item");

    decimal total = 0;
    foreach (var item in order.Items)
    {
        total += item.Price * item.Quantity;
    }

    if (total > 10000)
    {
        total *= 0.9m; // 10%割引
    }

    order.Total = total;
    await _orderRepository.AddAsync(order);
}
```

**After**:
```csharp
public async Task ProcessOrderAsync(Order order)
{
    ValidateOrder(order);
    var total = CalculateTotal(order.Items);
    var discountedTotal = ApplyDiscount(total);

    order.Total = discountedTotal;
    await _orderRepository.AddAsync(order);
}

private void ValidateOrder(Order order)
{
    if (order.Items.Count == 0)
        throw new ValidationException("Order must have at least one item");
}

private decimal CalculateTotal(List<OrderItem> items)
{
    return items.Sum(item => item.Price * item.Quantity);
}

private decimal ApplyDiscount(decimal total)
{
    const decimal DiscountThreshold = 10000;
    const decimal DiscountRate = 0.9m;

    return total > DiscountThreshold ? total * DiscountRate : total;
}
```

### パターン2: マジックナンバー定数化

**Before**:
```csharp
public bool IsPasswordValid(string password)
{
    return password.Length >= 8 && password.Length <= 100;
}
```

**After**:
```csharp
private const int MinPasswordLength = 8;
private const int MaxPasswordLength = 100;

public bool IsPasswordValid(string password)
{
    return password.Length >= MinPasswordLength &&
           password.Length <= MaxPasswordLength;
}
```

### パターン3: F#パイプライン活用

**Before**:
```fsharp
let processUsers users =
    let activeUsers = List.filter (fun u -> u.IsActive) users
    let sortedUsers = List.sortBy (fun u -> u.Name) activeUsers
    List.map (fun u -> { u with LastProcessed = DateTime.UtcNow }) sortedUsers
```

**After**:
```fsharp
let processUsers users =
    users
    |> List.filter (fun u -> u.IsActive)
    |> List.sortBy (fun u -> u.Name)
    |> List.map (fun u -> { u with LastProcessed = DateTime.UtcNow })
```

### パターン4: 条件分岐の早期リターン

**Before**:
```csharp
public async Task<User?> GetUserAsync(Guid userId)
{
    var user = await _userRepository.GetByIdAsync(userId);

    if (user != null)
    {
        if (user.IsActive)
        {
            if (!user.IsDeleted)
            {
                return user;
            }
        }
    }

    return null;
}
```

**After**:
```csharp
public async Task<User?> GetUserAsync(Guid userId)
{
    var user = await _userRepository.GetByIdAsync(userId);

    if (user == null) return null;
    if (!user.IsActive) return null;
    if (user.IsDeleted) return null;

    return user;
}
```

---

## よくある問題と対策

### 問題1: リファクタリング中にテストが失敗

**症状**:
```
❌ Failed: GetUserByIdAsync_ExistingUser_ReturnsUser
   (リファクタリングで動作が変わってしまった)
```

**対策**:
1. 即座にリファクタリングをロールバック
2. テストが成功することを確認
3. より慎重に段階的にリファクタリング

### 問題2: リファクタリングでWarningが増えた

**症状**:
```
warning CS8600: Converting null literal or possible null value to non-nullable type
```

**対策**:
1. Warningを修正
2. null許容参照型を適切に使用
3. ビルドが0 Warning / 0 Errorになることを確認

### 問題3: 過剰なリファクタリング

**症状**:
```csharp
// ❌ Bad: 過剰に細分化しすぎ
public class UserService
{
    public async Task<User?> GetUserAsync(Guid id)
    {
        return await GetUserFromRepositoryAsync(id);
    }

    private async Task<User?> GetUserFromRepositoryAsync(Guid id)
    {
        return await FetchUserAsync(id);
    }

    private async Task<User?> FetchUserAsync(Guid id)
    {
        return await _userRepository.GetByIdAsync(id);
    }
}
```

**対策**:
- 適度な粒度でリファクタリング
- 1メソッドは1責務だが、過剰な細分化は避ける

---

## Refactor Phase完了チェックリスト

- [ ] リファクタリング対象を特定した
- [ ] リファクタリングを実行した
- [ ] テストが成功している（Green状態維持）
- [ ] ビルドが成功した（0 Warning / 0 Error）
- [ ] Clean Architecture準拠を確認した（clean-architecture-guardian Skill使用）
- [ ] コード品質が改善した（可読性・保守性向上）
- [ ] 重複コードが削除されている
- [ ] 命名が明確である
- [ ] マジックナンバーが定数化されている

---

**次のサイクル**: [Red Phase](./red-phase-pattern.md) - 次の機能のテスト作成
