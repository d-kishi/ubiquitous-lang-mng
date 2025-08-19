# Phase A7 Step3: MVC削除対象マスターリスト

## 📋 削除対象概要
- **対象**: Phase A1-A6で残存したMVC関連ファイル・設定
- **目的**: Pure Blazor Server要件実現・アーキテクチャ統一
- **実施Step**: Step3（アーキテクチャ完全統一）
- **完了基準**: MVC要素0件・Pure Blazor Server実現

## 🗂️ Controllers削除チェックリスト

### 削除対象Controllers
```
src/UbiquitousLanguageManager.Web/Controllers/
```

- [ ] **HomeController.cs**
  - **機能**: ルート（/）認証分岐制御
  - **逸脱レベル**: 軽微
  - **削除可能性**: 高
  - **対応優先度**: 中
  - **代替実装**: App.razor + Pages/Index.razor

- [ ] **AccountController.cs** (Step2で作成後削除)
  - **機能**: MVC版パスワード変更（暫定実装）
  - **逸脱レベル**: 高
  - **削除可能性**: 高（Step2完了後）
  - **対応優先度**: 最高
  - **代替実装**: Pages/Auth/ChangePassword.razor

### 削除後確認事項
- [ ] Controllers/ディレクトリ完全削除
- [ ] Controller名前空間参照削除

## 📁 Views削除チェックリスト

### 削除対象Views
```
src/UbiquitousLanguageManager.Web/Views/
```

#### Home関連
- [ ] **Views/Home/Index.cshtml**
  - **機能**: ホーム画面MVC版
  - **逸脱レベル**: 軽微
  - **削除可能性**: 高
  - **対応優先度**: 中
  - **代替実装**: Pages/Index.razor

- [ ] **Views/Home/Error.cshtml**  
  - **機能**: エラー表示MVC版
  - **逸脱レベル**: 軽微
  - **削除可能性**: 高
  - **対応優先度**: 低
  - **代替実装**: Shared/ErrorBoundary.razor

#### Account関連
- [ ] **Views/Account/ChangePassword.cshtml**
  - **機能**: パスワード変更MVC版
  - **逸脱レベル**: **高**
  - **削除可能性**: **Step2完了後のみ可能**
  - **対応優先度**: **最高**
  - **代替実装**: Pages/Auth/ChangePassword.razor
  - **⚠️注意**: Step2でBlazor版実装完了まで削除禁止

- [ ] **Views/Account/AccessDenied.cshtml**
  - **機能**: アクセス拒否MVC版
  - **逸脱レベル**: 軽微
  - **削除可能性**: 中
  - **対応優先度**: 低
  - **代替実装**: Pages/Auth/Unauthorized.razor

#### Shared関連
- [ ] **Views/Shared/_Layout.cshtml**
  - **機能**: MVC共通レイアウト
  - **逸脱レベル**: 軽微
  - **削除可能性**: 中
  - **対応優先度**: 低
  - **代替実装**: Shared/MainLayout.razor（既存）

- [ ] **Views/Shared/_ValidationScriptsPartial.cshtml**
  - **機能**: クライアントサイドバリデーション
  - **逸脱レベル**: 軽微
  - **削除可能性**: 中
  - **対応優先度**: 低
  - **代替実装**: Blazor組み込みバリデーション

#### Views設定ファイル
- [ ] **Views/_ViewImports.cshtml**
  - **機能**: MVC View共通設定
  - **削除可能性**: 高
  - **影響**: MVC Viewがすべて削除されれば不要

- [ ] **Views/_ViewStart.cshtml**
  - **機能**: MVC View開始設定
  - **削除可能性**: 高  
  - **影響**: MVC Viewがすべて削除されれば不要

### 削除後確認事項
- [ ] Views/ディレクトリ完全削除
- [ ] MVC View参照削除
- [ ] Razor View Engine依存削除

## ⚙️ Program.cs設定削除チェックリスト

### Services設定削除
```csharp
// 削除対象（具体的行番号は実装時確認）
```

- [ ] **AddControllersWithViews()**
  - **場所**: Program.cs（サービス設定部分）
  - **影響**: MVC Controller/View機能無効化
  - **削除可能性**: Controllers/Views完全削除後
  - **確認事項**: AddRazorPages()は保持

- [ ] **AddMvc()関連設定**
  - **場所**: Program.cs（サービス設定部分） 
  - **影響**: MVC全般無効化
  - **削除可能性**: 高
  - **確認事項**: Blazor設定に影響なし

### ルーティング設定削除
```csharp
// 削除対象（具体的行番号は実装時確認）
```

- [ ] **MapControllerRoute()**
  - **場所**: Program.cs（ルーティング設定部分）
  - **設定内容**: `"{controller=Home}/{action=Index}/{id?}"`
  - **影響**: MVC Controller ルーティング無効化
  - **削除可能性**: Controllers完全削除後
  - **確認事項**: Blazor ルーティングに影響なし

- [ ] **MapControllers()**
  - **場所**: Program.cs（ルーティング設定部分）
  - **影響**: API Controller ルーティング
  - **削除可能性**: 高（API Controller使用していない場合）
  - **⚠️注意**: 今後のAPI実装予定確認要

### 保持する設定（削除対象外）
```csharp
// 保持必須：Blazor Server設定
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();

// 保持必須：認証設定
builder.Services.AddDefaultIdentity<ApplicationUser>();

// 保持必須：ルーティング設定  
app.MapRazorPages();
app.MapBlazorHub();
app.MapFallbackToPage("/_Host");
```

### 削除後確認事項
- [ ] `dotnet build` 成功（0 Warning, 0 Error）
- [ ] MVC関連エラー・警告なし
- [ ] Blazor Server機能正常動作

## 🧹 その他削除対象

### 不要なNuGetパッケージ確認
```xml
<!-- 確認対象：削除可能性要検討 -->
<PackageReference Include="Microsoft.AspNetCore.Mvc" />
<PackageReference Include="Microsoft.AspNetCore.Mvc.Razor.RuntimeCompilation" />
```

### 不要なusingディレクティブ
```csharp
// 削除対象例
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
```

## 📊 削除完了確認マトリックス

| カテゴリ | 削除対象数 | 削除完了 | 代替実装確認 | ビルド確認 |
|---------|-----------|---------|-------------|-----------|
| Controllers | 2ファイル | [ ] | [ ] | [ ] |
| Views | 8ファイル | [ ] | [ ] | [ ] |
| Program.cs設定 | 3設定 | [ ] | [ ] | [ ] |
| その他 | 2項目 | [ ] | [ ] | [ ] |
| **合計** | **15項目** | [ ] | [ ] | [ ] |

## 🎯 削除完了基準

### 技術基準
- [ ] **MVC要素0件**: Controllers・Views完全削除
- [ ] **Pure Blazor Server実現**: MVC設定完全削除
- [ ] **代替実装完了**: 全機能Blazor版実装済み

### 動作基準
- [ ] **認証フロー完全動作**: ログイン・ログアウト・パスワード変更
- [ ] **画面遷移正常**: 全URL Blazor Server で正常表示
- [ ] **エラーハンドリング**: 統一されたエラー表示

### 品質基準
- [ ] **ビルド成功**: 0 Warning, 0 Error
- [ ] **テスト成功**: MVC削除影響なし
- [ ] **性能維持**: SignalR・認証応答時間正常

## ⚠️ 削除実施時の注意事項

### 削除順序（重要）
1. **Step2完了確認**: AccountController・/change-password実装完了
2. **代替機能確認**: 各削除対象の代替Blazor実装確認
3. **段階的削除**: Controllers→Views→Program.cs設定の順
4. **都度確認**: 各段階でビルド・動作確認実施

### 回避すべき作業
- ❌ **Step2完了前の削除**: 機能停止リスク
- ❌ **一括削除**: 問題特定困難化
- ❌ **テスト省略**: 回帰バグリスク

### 削除後の確認必須項目
- ✅ **全URL正常動作**: /・/login・/change-password・/profile・/admin/users
- ✅ **認証フロー完全動作**: 初回ログイン→パスワード変更フロー
- ✅ **エラーハンドリング**: F#→C#境界エラー表示

---

**削除実施**: Step3（アーキテクチャ完全統一）  
**完了基準**: MVC要素0件・Pure Blazor Server実現  
**次工程**: Step4（Contracts層・型変換完全実装）