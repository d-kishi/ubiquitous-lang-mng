# Web認証UXチーム 専門分析結果

**Phase**: A1 - 基本認証システム  
**分析日**: 2025-07-16  
**チーム**: Web認証UXチーム  

## 技術調査結果

### 調査対象技術・パターン

1. **Blazor Serverでの認証状態管理とSignalR連携**
2. **ASP.NET Core認証でのセキュリティベストプラクティス**
3. **Blazor認証UIコンポーネントの実装パターン**
4. **既存UI設計書に基づく実装戦略**

### 発見事項・ベストプラクティス

#### 1. Blazor Server認証状態管理

**AuthenticationStateProvider実装パターン**:
```csharp
public class CustomAuthenticationStateProvider : AuthenticationStateProvider
{
    private readonly IUserService _userService;
    private ClaimsPrincipal _currentUser = new ClaimsPrincipal(new ClaimsIdentity());

    public override Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        return Task.FromResult(new AuthenticationState(_currentUser));
    }

    public async Task<bool> LoginAsync(string email, string password)
    {
        var user = await _userService.ValidateUserAsync(email, password);
        if (user != null)
        {
            var claims = new List<Claim>
            {
                new(ClaimTypes.Name, user.Name),
                new(ClaimTypes.Email, user.Email),
                new(ClaimTypes.Role, user.Role.ToString())
            };

            _currentUser = new ClaimsPrincipal(new ClaimsIdentity(claims, "Custom"));
            NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(_currentUser)));
            return true;
        }
        return false;
    }
}
```

#### 2. セキュリティベストプラクティス

**CSRF対策**:
```csharp
// Program.cs
builder.Services.AddAntiforgery(options =>
{
    options.HeaderName = "X-CSRF-TOKEN";
    options.SuppressXFrameOptionsHeader = false;
});

// ログインコンポーネント
private async Task HandleLoginAsync()
{
    var tokens = Antiforgery.GetAndStoreTokens(HttpContext);
    await JSRuntime.InvokeVoidAsync("blazorApp.setAntiforgeryToken", tokens.RequestToken);
    await AuthStateProvider.LoginAsync(loginModel.Email, loginModel.Password);
}
```

**XSS対策**:
- Blazor Server標準の自動エスケープ機能活用
- 明示的HTMLエスケープ: `@((MarkupString)Html.Encode(userInput))`

**セキュアなパスワード管理**:
```csharp
public class UserService : IUserService
{
    public async Task<UserDto?> ValidateUserAsync(string email, string password)
    {
        var user = await _userRepository.GetByEmailAsync(email);
        if (user != null && BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
        {
            return user.ToDto();
        }
        return null;
    }
}
```

#### 3. Blazor認証UIコンポーネント

**ログイン画面実装**:
```razor
@page "/login"
@inject CustomAuthenticationStateProvider AuthStateProvider
@inject NavigationManager Navigation

<div class="login-container">
    <div class="login-form">
        <h2>ログイン</h2>
        
        <EditForm Model="@loginModel" OnValidSubmit="@HandleLoginAsync">
            <DataAnnotationsValidator />
            <ValidationSummary />
            
            <div class="form-group">
                <label>メールアドレス</label>
                <InputText @bind-Value="loginModel.Email" class="form-control" />
                <ValidationMessage For="@(() => loginModel.Email)" />
            </div>
            
            <div class="form-group">
                <label>パスワード</label>
                <InputText @bind-Value="loginModel.Password" type="password" class="form-control" />
                <ValidationMessage For="@(() => loginModel.Password)" />
            </div>
            
            <button type="submit" class="btn btn-primary" disabled="@isLoading">
                @if (isLoading)
                {
                    <span class="spinner-border spinner-border-sm"></span>
                }
                ログイン
            </button>
        </EditForm>
    </div>
</div>
```

#### 4. 既存UI設計書準拠実装

**段階的実装戦略**:
1. 最小限のログイン画面（Email/Password）
2. 基本UI機能（Remember Me、ローディング状態）
3. 拡張機能（セッションタイムアウト、エラーハンドリング）

### 潜在的リスク・注意点

#### 技術的課題
1. **SignalR接続の認証状態同期問題**
   - SignalR接続が切断された場合の認証状態同期
   - 接続復旧時の認証状態再確認機能が必要

2. **セッションタイムアウト処理**
   - 長時間操作がない場合のセッション無効化
   - JavaScript Timerでアクティビティ監視が必要

3. **Blazor Server特有の問題**
   - StateHasChanged()の適切な呼び出しタイミング
   - コンポーネントライフサイクルの理解

#### セキュリティリスク
1. **CSRF攻撃対策**
   - AntiforgeryTokenの適切な実装
   - フォーム送信時のトークン検証

2. **XSS攻撃対策**
   - ユーザー入力の適切なエスケープ
   - HTMLコンテンツのサニタイゼーション

## 実装方針

### 推奨実装アプローチ

#### Phase A1段階的実装戦略

**Phase A1-1: 基本認証機能**
- CustomAuthenticationStateProvider実装
- シンプルなログイン画面
- 基本的なセッション管理

**Phase A1-2: セキュリティ強化**
- CSRF対策実装
- セッションタイムアウト処理
- パスワード強度チェック

**Phase A1-3: UI/UX改善**
- Remember Me機能
- ログイン状態の視覚的フィードバック
- エラーハンドリングの改善

### Clean Architecture統合

**F# Domain層での認証ロジック**:
```fsharp
module AuthenticationDomain =
    type AuthenticationResult =
        | Success of User
        | InvalidCredentials
        | UserNotFound
        | AccountLocked
    
    let validateCredentials (email: string) (password: string) (userRepository: IUserRepository) =
        async {
            match! userRepository.GetByEmailAsync(email) with
            | Some user when BCrypt.Net.BCrypt.Verify(password, user.PasswordHash) ->
                return Success user
            | Some _ ->
                return InvalidCredentials
            | None ->
                return UserNotFound
        }
```

### 技術選択の理由

- **CustomAuthenticationStateProvider**: Blazor Server標準パターン
- **Cookie認証**: セッション管理のシンプルさ
- **BCrypt**: パスワードハッシュ化の業界標準
- **EditForm**: Blazor標準のフォーム検証機能

### 他チームとの連携ポイント

1. **F#ドメイン認証チーム**
   - 認証ロジックの実装共有
   - 認証結果の型変換協調

2. **Infrastructure統合チーム**
   - UserManager/SignInManagerの共有
   - 認証状態の一貫性確保

3. **Contracts境界チーム**
   - 認証DTO仕様の共有
   - エラー表示の一貫性確保

## 課題・懸念事項

### 発見された技術的課題

1. **Blazor Server初学者対応**
   - StateHasChanged、SignalR、コンポーネントライフサイクルの詳細コメント必須
   - ADR_010に従った実装規約の徹底

2. **F#↔C#境界の型変換**
   - 既存のContracts層TypeConvertersの活用
   - F#のResult型をC#で適切に処理

3. **UI設計書準拠**
   - 既存UI設計書の仕様に厳密に従う実装
   - 段階的にUI機能を追加（最小限→基本機能→拡張機能）

### 解決が必要な事項

1. セッションタイムアウト処理の実装方法
2. SignalR接続切断時の認証状態同期
3. エラーハンドリングの統一化

### 次Stepでの検証項目

1. CustomAuthenticationStateProvider動作確認
2. ログイン画面の基本機能テスト
3. セキュリティ対策の有効性確認

## Gemini連携結果

### 実施した技術調査内容

1. "Blazor Serverでの認証状態管理とSignalR連携"
2. "ASP.NET Core認証でのセキュリティベストプラクティス"
3. "Blazor認証UIコンポーネントの実装パターン"
4. "ASP.NET Core Identity Blazor Server統合"

### 得られた技術知見

- **CustomAuthenticationStateProvider**がBlazor Server認証の標準パターン
- **Cookie認証**が最もシンプルで効果的なセッション管理
- **BCrypt**がパスワードハッシュ化の業界標準
- **EditForm**による標準的なフォーム検証が推奨

### 実装への適用方針

1. Blazor Server標準パターンの採用による安全性確保
2. 段階的実装による品質確保
3. 既存アーキテクチャとの整合性維持
4. セキュリティベストプラクティスの徹底