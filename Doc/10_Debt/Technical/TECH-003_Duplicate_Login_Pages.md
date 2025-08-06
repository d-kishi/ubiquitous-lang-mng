# TECH-003: ログイン画面の重複と統合

**発見日**: 2025-08-06  
**報告者**: プロジェクトオーナー  
**優先度**: High（Phase B1着手前修正）  
**ステータス**: 未対応  

## 問題概要

ログイン画面が2つ存在し、Blazor Server版が正常に機能していない。

## 詳細

### 現状の実装
1. **MVC版ログイン画面**
   - URL: `http://localhost:5001/Account/Login`
   - 状態: 正常に機能している
   - 技術: ASP.NET Core MVC

2. **Blazor Server版ログイン画面**
   - URL: `http://localhost:5001/login`
   - 状態: 画面は表示されるが、ログイン機能が動作していない
   - 技術: Blazor Server

### 方針
- **残す**: Blazor Server版（`/login`）
- **削除**: MVC版（`/Account/Login`）
- **理由**: プロジェクト全体でBlazor Serverを活用する方針のため

## 影響範囲

1. **削除対象**
   - `/Controllers/AccountController.cs`のLoginアクション
   - `/Views/Account/Login.cshtml`
   - 関連するViewModelやPartialView

2. **修正対象**
   - `/Components/Pages/Login.razor`のバックエンド処理
   - 認証サービスとの連携
   - ログイン成功後のリダイレクト処理

3. **更新対象**
   - ルーティング設定
   - 認証ミドルウェア設定
   - ログアウト時のリダイレクト先

## 修正方針

### 実装手順
1. **Blazor Serverログイン機能の実装**
   - `IAuthenticationService`を使用した認証処理
   - `AuthenticationStateProvider`の適切な更新
   - Cookie認証の設定

2. **MVC版の削除**
   - AccountControllerからLoginアクション削除
   - Viewファイルの削除
   - ルーティング設定の調整

3. **統合テスト**
   - ログイン機能の動作確認
   - セッション管理の確認
   - リダイレクトの動作確認

### 注意事項
- パスワードリセット機能との連携確認
- Remember Me機能の動作確認
- エラーメッセージの表示確認

## 関連Issue
- TECH-001: ASP.NET Core Identity設計見直し
- TECH-002: 初期スーパーユーザーパスワード仕様不整合

## 対応予定
Phase B1着手前（2025-08-06～08-07予定）