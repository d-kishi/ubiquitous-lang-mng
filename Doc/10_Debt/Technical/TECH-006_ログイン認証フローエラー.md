# TECH-006: ログイン認証フローエラー

**作成日**: 2025-08-25  
**発見箇所**: Phase A7 Step5完了時  
**影響度**: 中  
**優先度**: 高（Step6で解決必須）  

## 問題概要

ログイン処理実行時に以下のエラーが発生する：

```
System.InvalidOperationException: Headers are read-only, response has already started.
   at Microsoft.AspNetCore.Server.Kestrel.Core.Internal.Http.HttpHeaders.ThrowHeadersReadOnlyException()
   at Microsoft.AspNetCore.Server.Kestrel.Core.Internal.Http.HttpResponseHeaders.Microsoft.AspNetCore.Http.IHeaderDictionary.set_SetCookie(StringValues value)
   at Microsoft.AspNetCore.Http.ResponseCookies.Append(String key, String value, CookieOptions options)
   at Microsoft.AspNetCore.Authentication.Cookies.ChunkingCookieManager.AppendResponseCookie(HttpContext context, String key, String value, CookieOptions options)
   at Microsoft.AspNetCore.Authentication.Cookies.CookieAuthenticationHandler.HandleSignInAsync(ClaimsPrincipal user, AuthenticationProperties properties)
   at Microsoft.AspNetCore.Authentication.AuthenticationService.SignInAsync(HttpContext context, String scheme, ClaimsPrincipal principal, AuthenticationProperties properties)
   at Microsoft.AspNetCore.Identity.SignInManager`1.SignInWithClaimsAsync(TUser user, AuthenticationProperties authenticationProperties, IEnumerable`1 additionalClaims)
   at Microsoft.AspNetCore.Identity.SignInManager`1.SignInOrTwoFactorAsync(TUser user, Boolean isPersistent, String loginProvider, Boolean bypassTwoFactor)
   at Microsoft.AspNetCore.Identity.SignInManager`1.PasswordSignInAsync(TUser user, String password, Boolean isPersistent, Boolean lockoutOnFailure)
   at UbiquitousLanguageManager.Web.Services.AuthenticationService.LoginAsync(LoginRequestDto request) in C:\Develop\ubiquitous-lang-mng\src\UbiquitousLanguageManager.Web\Services\AuthenticationService.cs:line 125
```

## 原因分析

### 直接原因
Blazor ServerコンポーネントでHTTPレスポンスが既に開始された後に、ASP.NET Core IdentityがCookieを設定しようとしてエラー発生。

### 問題箇所
- **ファイル**: `src/UbiquitousLanguageManager.Web/Components/Pages/Auth/Login.razor`
- **問題行**: 256行目の`StateHasChanged()`呼び出し後、266行目で`AuthService.LoginAsync()`実行
- **問題**: HTTPレスポンス開始後のCookie認証処理

## 技術的詳細

### エラー発生シーケンス
1. `Login.razor`の`HandleLoginAsync()`メソッド実行
2. 256行目：`StateHasChanged()`でHTTPレスポンス開始
3. 266行目：`AuthService.LoginAsync()`でCookie認証処理
4. エラー発生：Cookieヘッダー設定不可

### Blazor Server特有の問題
- **SignalR通信**: StateHasChanged()でサーバー→クライアント通信開始
- **HTTPコンテキスト**: レスポンス開始後のヘッダー変更制約
- **認証フロー**: ASP.NET Core Identity Cookie設定タイミング

## 影響範囲

### 機能影響
- **ログイン機能**: 完全に機能しない
- **認証フロー**: 全認証処理に影響
- **ユーザー体験**: アプリケーション利用不可

### システム影響
- **重要度**: 高（基本機能の完全停止）
- **範囲**: 全ユーザー・全認証機能
- **回避策**: なし

## 解決方針

### 修正方向性
1. **`StateHasChanged()`タイミング調整**: 認証処理完了後に移動
2. **認証フロー最適化**: Blazor Server・ASP.NET Core Identity統合改善
3. **エラーハンドリング**: 認証エラー時の適切な処理実装

### 推定修正内容
- `Login.razor`の認証処理フロー見直し
- `AuthenticationService.cs`のHTTPコンテキスト処理改善
- 認証状態更新タイミングの最適化

## 対応計画

### 解決予定Step
**Phase A7 Step6（統合品質保証・完了確認）**で解決

### 担当SubAgent
- **integration-test**: 認証統合テスト・エラー修正
- **csharp-web-ui**: Login.razorフロー修正
- **code-review**: 認証処理品質確認

### 推定工数
- **修正作業**: 30-45分
- **テスト確認**: 15-30分
- **合計**: Step6想定時間内（60-90分）

## 技術負債分類

- **分類**: 実装品質問題
- **緊急度**: 高（基本機能停止）
- **技術的複雑度**: 中（Blazor Server・ASP.NET Core Identity統合）
- **ビジネス影響**: 高（アプリケーション利用不可）

## 関連情報

### 関連技術負債
- なし（新規発見）

### 関連課題
- [UI-001]: プロフィール変更画面（90%解決・残10%は本エラー修正）

### 参考資料
- Blazor Server認証ベストプラクティス
- ASP.NET Core Identity Cookie認証ガイド

**記録者**: MainAgent  
**確認者**: Phase A7 Step5終了時レビュー  
**次回アクション**: Step6で修正実施