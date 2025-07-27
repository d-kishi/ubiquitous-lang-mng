# Phase A3 Step5: エラー分類マトリックス

**作成日**: 2025-07-27  
**目的**: 121個テストコンパイルエラーの体系的分類・修正優先度付け  

## 📊 エラー分類サマリー

| カテゴリ | 件数 | 割合 | 優先度 | 修正方針 |
|---------|------|------|--------|----------|
| **基盤構造** | 64件 | 53% | 最高 | 一括修正 |
| **API変更** | 40件 | 33% | 高 | 段階修正 |
| **型境界** | 17件 | 14% | 中 | パターン修正 |
| **合計** | **121件** | **100%** | - | - |

## 🔍 詳細分類

### 1. 基盤構造エラー (64件 - 最優先)

#### **1.1 DbContext参照エラー (27件)**
**エラーパターン**: `ApplicationDbContext` → `UbiquitousLanguageDbContext`

**影響ファイル**:
- `DatabaseFixture.cs` (4箇所)
- 各種統合テストファイル (23箇所)

**修正方法**: 一括置換
```csharp
// 修正前
services.AddDbContext<ApplicationDbContext>
var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

// 修正後  
services.AddDbContext<UbiquitousLanguageDbContext>
var context = scope.ServiceProvider.GetRequiredService<UbiquitousLanguageDbContext>();
```

#### **1.2 User型参照エラー (31件)**
**エラーパターン**: `User` → `ApplicationUser`

**影響ファイル**:
- Identity設定 (8箇所)
- UserManager/SignInManager参照 (23箇所)

**修正方法**: 型統一
```csharp
// 修正前
services.AddIdentity<User, IdentityRole>
UserManager<User> _userManager;

// 修正後
services.AddIdentity<ApplicationUser, IdentityRole>  
UserManager<ApplicationUser> _userManager;
```

#### **1.3 SmtpSettings API変更 (6件)**
**エラーパターン**: プロパティ名変更

**影響ファイル**:
- `DatabaseFixture.cs`
- SmtpSettings関連テスト

**修正方法**: プロパティ名統一
```csharp
// 修正前
options.UseSsl = false;
options.From = "test@example.com";
options.FromName = "Test System";

// 修正後
options.EnableSsl = false;
options.SenderEmail = "test@example.com";  
options.SenderName = "Test System";
```

### 2. API変更エラー (40件 - 高優先)

#### **2.1 AuthenticationServiceコンストラクタ (15件)**
**エラーパターン**: `CS1729` - 引数5個→1個

**影響ファイル**:
- `AuditLoggingTests.cs`
- `AuthenticationServiceAutoLoginTests.cs`
- `AuthenticationServicePasswordResetTests.cs`
- `IdentityLockoutTests.cs`  
- `RememberMeFunctionalityTests.cs`

**修正方法**: コンストラクタ引数変更
```csharp
// 修正前
new AuthenticationService(
    _loggerMock.Object,
    _userManagerMock.Object,
    _signInManagerMock.Object,
    _notificationServiceMock.Object,
    _userRepositoryMock.Object);

// 修正後
new AuthenticationService(_loggerMock.Object);
```

#### **2.2 削除メソッド呼び出し (25件)**
**エラーパターン**: `CS1061` - メソッドが見つからない

**削除されたメソッド**:
- `RequestPasswordResetAsync` (8件)
- `ResetPasswordAsync` (6件)  
- `AutoLoginAfterPasswordResetAsync` (5件)
- `RecordLoginAttemptAsync` (4件)
- `ValidatePasswordResetTokenAsync` (2件)

**修正方法**: 拡張メソッド作成
```csharp
// TemporaryStubs.cs
public static class AuthenticationServiceExtensions
{
    public static Task<FSharpResult<string, string>> RequestPasswordResetAsync(
        this AuthenticationService service, Email email)
    {
        return Task.FromResult(FSharpResult<string, string>.NewError("Phase A3で削除"));
    }
    // ... 他のメソッドも同様
}
```

### 3. 型境界エラー (17件 - 中優先)

#### **3.1 F# Unit型衝突 (10件)**
**エラーパターン**: `CS0118` - 'Unit' は名前空間

**影響箇所**:
- `FSharpResult<Unit, string>` 使用箇所

**修正方法**: 完全修飾名使用
```csharp
// 修正前
FSharpResult<Unit, string>.NewOk(null!)

// 修正後
FSharpResult<Microsoft.FSharp.Core.Unit, string>.NewOk(null!)
```

#### **3.2 ISmtpClient名前空間衝突 (7件)**
**エラーパターン**: `CS0104` - あいまいな参照

**競合**: 
- `UbiquitousLanguageManager.Infrastructure.Emailing.ISmtpClient`
- `MailKit.Net.Smtp.ISmtpClient`

**修正方法**: 完全修飾名または using alias
```csharp
// 修正方法1: 完全修飾名
UbiquitousLanguageManager.Infrastructure.Emailing.ISmtpClient

// 修正方法2: using alias
using InfraISmtpClient = UbiquitousLanguageManager.Infrastructure.Emailing.ISmtpClient;
```

## 🛠️ 修正実行順序

### **Phase 2-1: 基盤修正 (64件)**
1. DbContext参照修正 (27件)
2. User型統一 (31件)
3. SmtpSettings修正 (6件)

### **Phase 2-2: API修正 (40件)**  
1. AuthenticationServiceコンストラクタ (15件)
2. 削除メソッド対応 (25件)

### **Phase 2-3: 型境界修正 (17件)**
1. Unit型衝突解決 (10件)
2. ISmtpClient衝突解決 (7件)

## 📋 修正完了チェックリスト

### **基盤修正完了確認**
- [ ] DatabaseFixture.cs修正完了
- [ ] 全Identity設定修正完了  
- [ ] SmtpSettings修正完了
- [ ] ビルドエラー64件→0件確認

### **API修正完了確認**
- [ ] 全AuthenticationServiceコンストラクタ修正完了
- [ ] TemporaryStubs.cs作成・適用完了
- [ ] 削除メソッド拡張適用完了
- [ ] ビルドエラー40件→0件確認

### **型境界修正完了確認**  
- [ ] 全Unit型完全修飾名適用完了
- [ ] ISmtpClient衝突解決完了
- [ ] ビルドエラー17件→0件確認

### **最終確認**
- [ ] 全テストプロジェクトビルド成功
- [ ] エラー0件・警告のみ
- [ ] TDD実行環境確認完了

---

**記録者**: テストインフラアーキテクト + 組織管理・進捗統括専門家  
**承認状況**: Phase A3 Step5実行準備完了  
**次回更新**: Phase 2実行完了時