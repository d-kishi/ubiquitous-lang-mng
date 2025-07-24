# Phase A3 Identity技術調査チーム 調査結果

**調査日**: 2025-07-24  
**調査者**: Identity技術調査チーム  
**調査時間**: 30分  

## 📋 ASP.NET Core Identity拡張調査

### パスワードリセット機能

#### GeneratePasswordResetTokenAsync の仕組み
1. **DataProtectorTokenProvider使用**
   - ASP.NET Core データ保護APIによる暗号化
   - ユーザーID + セキュリティスタンプを含む
   - ステートレス（DBに保存されない）

2. **トークン検証メカニズム**
   - 同一パラメータでトークン再生成・比較
   - セキュリティスタンプ変更で自動無効化
   - 有効期限設定可能（デフォルト1日）

#### 実装パターン
```csharp
// トークン生成
var token = await _userManager.GeneratePasswordResetTokenAsync(user);

// トークン検証・リセット
var result = await _userManager.ResetPasswordAsync(user, token, newPassword);
```

### Remember Me機能

#### isPersistentパラメータ
- `false`: セッションCookie（ブラウザ終了で削除）
- `true`: 永続化Cookie（有効期限まで保持）

#### 設定方法
```csharp
// Program.cs
builder.Services.ConfigureApplicationCookie(options =>
{
    options.ExpireTimeSpan = TimeSpan.FromDays(7); // 7日間
    options.SlidingExpiration = true; // 期限自動延長
});
```

#### セキュリティ考慮事項
1. **HttpOnly Cookie**（デフォルト有効）
2. **Secure属性**（HTTPS環境で自動付与）
3. **セキュリティスタンプ**による無効化
4. **公共端末での注意喚起**必要

### セッション管理強化

#### セッション固定攻撃対策
```csharp
// ログイン成功時の自動実行
HttpContext.Session.Remove("SessionId");
HttpContext.Session.SetString("SessionId", Guid.NewGuid().ToString());
```

#### タイムアウト設定
```csharp
builder.Services.ConfigureApplicationCookie(options =>
{
    options.ExpireTimeSpan = TimeSpan.FromHours(2);
    options.SlidingExpiration = true;
});
```

## 🔧 カスタムSignInManager実装

### 監査ログ統合パターン
```csharp
public class AuditSignInManager<TUser> : SignInManager<TUser> where TUser : class
{
    private readonly IAuditLogger _auditLogger;
    
    public override async Task<SignInResult> PasswordSignInAsync(
        string userName, string password, bool isPersistent, bool lockoutOnFailure)
    {
        var result = await base.PasswordSignInAsync(userName, password, isPersistent, lockoutOnFailure);
        
        if (result.Succeeded)
            await _auditLogger.LogAuthenticationSuccessAsync(userName, GetClientIp());
        else
            await _auditLogger.LogAuthenticationFailureAsync(userName, GetClientIp(), GetFailureReason(result));
            
        return result;
    }
}
```

### DI登録
```csharp
builder.Services.AddDefaultIdentity<IdentityUser>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddSignInManager<AuditSignInManager<IdentityUser>>();
```

## 📊 ベストプラクティス

### トークン管理
1. **短い有効期限**（3-24時間）
2. **一度きりの使用**（使用後無効化）
3. **HTTPS必須**
4. **ユーザー存在の秘匿**

### エラーハンドリング
1. **具体的な例外キャッチ**
2. **ユーザーフレンドリーメッセージ**
3. **詳細ログ記録**（個人情報除外）

### パフォーマンス
1. **非同期処理**徹底
2. **キャッシュ活用**（セキュリティスタンプ等）
3. **DB接続最適化**

## 🚨 実装時の注意点

### 避けるべきパターン
1. トークンのDB保存
2. 平文パスワード処理
3. 同期的なメール送信
4. セッションIDの固定使用

### 推奨実装順序
1. 基本的な認証フロー確立
2. セキュリティ機能追加
3. 監査・ログ統合
4. パフォーマンス最適化

---

**関連技術**:
- ASP.NET Core Identity
- データ保護API
- Cookie認証
- セッション管理