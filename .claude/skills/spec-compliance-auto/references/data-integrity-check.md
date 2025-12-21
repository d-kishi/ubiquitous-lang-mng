# データ整合性準拠チェック

## 概要

データベース設計がデータベース設計書通りであるかをチェックします。

## チェック項目

### 1. 主キー・外部キー制約

**チェックリスト**:
- [ ] 主キーが正しく設定されている
- [ ] 外部キーが正しく設定されている
- [ ] 参照整合性制約が設定されている
- [ ] カスケード削除設定が仕様通り

**例**:
```csharp
// Entity Framework設定例
modelBuilder.Entity<UserProject>()
    .HasOne(up => up.User)
    .WithMany(u => u.UserProjects)
    .HasForeignKey(up => up.UserId)
    .OnDelete(DeleteBehavior.Cascade); // 仕様書準拠
```

### 2. NULL許容・NOT NULL制約

**チェックリスト**:
- [ ] 必須項目がNOT NULLである
- [ ] オプション項目がNULL許容である
- [ ] デフォルト値が仕様通り

**例**:
```csharp
public class User
{
    public Guid Id { get; set; } // NOT NULL（主キー）
    public string Email { get; set; } = null!; // NOT NULL（必須）
    public string? PhoneNumber { get; set; } // NULL許容（オプション）
}
```

### 3. 一意制約

**チェックリスト**:
- [ ] 一意制約が仕様通りである
- [ ] インデックスが適切に設定されている

**例**:
```csharp
modelBuilder.Entity<User>()
    .HasIndex(u => u.Email)
    .IsUnique(); // メールアドレスは一意
```

### 4. データ型・長さ

**チェックリスト**:
- [ ] データ型が仕様通り
- [ ] 文字列長が仕様通り
- [ ] 数値範囲が仕様通り

**例**:
```csharp
[MaxLength(256)] // 仕様書準拠
public string Email { get; set; } = null!;

[MaxLength(1000)] // 仕様書準拠
public string Description { get; set; } = null!;
```

---

**参照**: データベース設計書.md（Doc/02_Design/データベース設計書.md）
