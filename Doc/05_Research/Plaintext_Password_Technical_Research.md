# 平文パスワード管理・初回ログイン認証技術調査

## 調査概要

ASP.NET Core Identityでの平文パスワード一時保存と初回ログイン認証のベストプラクティス調査結果。セキュリティリスク分析と実装方針を含む。

---

## 1. ASP.NET Core Identity平文パスワード管理手法

### 1.1 PasswordHash NULL作成手法

#### CreateAsync(TUser user)メソッド使用
```csharp
// パスワードなしでユーザー作成（PasswordHashがNULLになる）
var user = new ApplicationUser { UserName = "username", Email = "email@example.com" };
var result = await userManager.CreateAsync(user);
```

#### カスタムPasswordValidator実装
```csharp
internal class CustomPasswordValidator : PasswordValidator
{
    public override async Task<IdentityResult> ValidateAsync(string item)
    {
        if (string.IsNullOrEmpty(item))
            return IdentityResult.Success;
        return await base.ValidateAsync(item);
    }
}
```

#### カスタムPasswordHasher実装
```csharp
internal class CustomPasswordHasher : PasswordHasher
{
    public override PasswordVerificationResult VerifyHashedPassword(
        string hashedPassword, string providedPassword)
    {
        if (hashedPassword == null && string.IsNullOrEmpty(providedPassword))
            return PasswordVerificationResult.Success;
        return base.VerifyHashedPassword(hashedPassword, providedPassword);
    }
}
```

### 1.2 設定方法
```csharp
services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders()
    .AddPasswordValidator<CustomPasswordValidator>()
    .AddPasswordValidator<CustomPasswordHasher>();
```

---

## 2. 初回ログイン認証・パスワード変更強制実装パターン

### 2.1 データベース拡張アプローチ

#### ApplicationUserモデル拡張
```csharp
public class ApplicationUser : IdentityUser
{
    public bool RequirePasswordChange { get; set; } = false;
    public string? InitialPassword { get; set; } // 一時的な平文パスワード保存
    public DateTime? LastPasswordChangeDate { get; set; }
}
```

### 2.2 カスタム認証フロー実装

#### カスタムSignInManager実装
```csharp
public class CustomSignInManager : SignInManager<ApplicationUser> 
{
    public CustomSignInManager(
        UserManager<ApplicationUser> userManager,
        IHttpContextAccessor contextAccessor,
        IUserClaimsPrincipalFactory<ApplicationUser> claimsFactory,
        IOptions<IdentityOptions> optionsAccessor,
        ILogger<SignInManager<ApplicationUser>> logger,
        IAuthenticationSchemeProvider schemes)
        : base(userManager, contextAccessor, claimsFactory, optionsAccessor, logger, schemes)
    {
    }

    public override async Task<SignInResult> CheckPasswordSignInAsync(
        ApplicationUser user, string password, bool lockoutOnFailure)
    {
        // 初回ログイン判定
        if (user.RequirePasswordChange && !string.IsNullOrEmpty(user.InitialPassword))
        {
            // 平文パスワード比較
            if (user.InitialPassword == password)
            {
                return SignInResult.Success;
            }
        }
        
        // 通常のハッシュ認証
        return await base.CheckPasswordSignInAsync(user, password, lockoutOnFailure);
    }
}
```

---

## 3. セキュリティ考慮事項

### 3.1 平文パスワード保持リスク

#### 🔴 重大リスク
- **データベース漏洩時の即座のアカウント侵害**
- **内部アクセス権限者による不正利用**
- **ログファイルへの意図しない記録**
- **メモリダンプ時の情報漏洩**

#### 🟡 軽減可能リスク  
- **一時的保存による時間限定リスク**
- **初回ログイン後の即座削除による最小化**

### 3.2 セキュリティ対策実装

#### 暗号化保存アプローチ
```csharp
public class InitialPasswordService
{
    private readonly IDataProtector _protector;
    
    public InitialPasswordService(IDataProtectionProvider provider)
    {
        _protector = provider.CreateProtector("InitialPassword");
    }
    
    public string EncryptPassword(string password)
    {
        return _protector.Protect(password);
    }
    
    public string DecryptPassword(string encryptedPassword)
    {
        return _protector.Unprotect(encryptedPassword);
    }
}
```

---

## 4. 推奨実装方針

### 4.1 セキュリティ重視アプローチ（推奨）

#### Email確認トークンベース実装
```csharp
public async Task<IActionResult> CreateUserAsync(CreateUserRequest request)
{
    var user = new ApplicationUser 
    { 
        UserName = request.UserName,
        Email = request.Email,
        RequirePasswordChange = true
    };
    
    // パスワードなしでユーザー作成
    var createResult = await _userManager.CreateAsync(user);
    
    if (createResult.Succeeded)
    {
        // パスワードリセットトークン生成
        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        
        // Email送信（トークン付きURL）
        var resetUrl = Url.Action("SetInitialPassword", "Account", 
            new { userId = user.Id, token }, Request.Scheme);
            
        await _emailService.SendInitialPasswordEmailAsync(user.Email, resetUrl);
    }
    
    return Ok();
}
```

---

## 5. 結論・推奨事項

### 5.1 技術選択指針
- **❌ 平文パスワード保存**: セキュリティリスク過大
- **⭐ Email確認トークン**: 最もセキュアなアプローチ
- **✅ 暗号化一時保存**: セキュリティとUXのバランス

### 5.2 本プロジェクト推奨実装
1. **Phase A8での実装**: Email確認トークンベースの初回ログイン
2. **Clean Architecture統合**: F# Domain/Application層との適切な分離
3. **段階的移行**: 既存Identity実装からの無停止移行

### 5.3 重要注意事項
- **絶対原則**: 本格運用で平文パスワード長期保存は禁止
- **監査要件**: 全認証イベントのログ記録必須
- **定期見直し**: セキュリティ対策の継続的改善

---

**調査実施日**: 2025-08-31  
**技術調査Agent**: Claude Code  
**対象プロジェクト**: ユビキタス言語管理システム
