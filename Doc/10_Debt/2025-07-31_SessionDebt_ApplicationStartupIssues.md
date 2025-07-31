# 2025-07-31 セッション技術的負債記録

**作成日**: 2025-07-31  
**セッション目的**: Dockerポート変更後の動作確認  
**結果**: 場当たり的修正により基本動作は確保したが、複数の技術的負債が発生  
**Phase A4での扱い**: 参考情報として活用（体系的解決の参考）

## 🚨 **重要な前提**

本記録は**Phase A4対応時の参考情報**として作成されています。  
Phase A4では、ADR_013（組織管理サイクル運用規則）に従った体系的なアプローチにより、これらの問題を根本から解決することを推奨します。

## 📋 **発生した技術的負債一覧**

### 1. **依存関係注入設定の不完全修正**

**問題**: 
- `Program.cs`で複数の依存関係注入設定を一時的にコメントアウト
- 根本的な設計問題の解決ではなく、回避的対応

**具体的修正内容**:
```csharp
// 🔧 DbContextFactory設定: マルチスレッド環境でのEF Core最適化
// 一時的にコメントアウト: Singleton登録問題を回避
// builder.Services.AddDbContextFactory<UbiquitousLanguageDbContext>(options =>
//     options.UseNpgsql(connectionString));

// Application Service実装の登録
// 一時的にコメントアウト: IAuthenticationService依存関係の問題を解決するため
// builder.Services.AddScoped<UbiquitousLanguageManager.Application.UserApplicationService>();
```

**影響**:
- `DbContextFactory`が使用不可（Blazor Serverでの最適化機能喪失）
- `UserApplicationService`が使用不可（F#ドメイン層との連携不可）
- 本番環境での性能・機能制限

**Phase A4での対応方針**:
- Clean ArchitectureのF#/C#境界設計の見直し
- 依存関係注入の体系的設計・実装
- 適切な`IAuthenticationService`実装の確立

### 2. **GlobalExceptionMiddlewareの開発機能無効化**

**問題**:
- 開発環境でのエラー詳細表示機能を一時的に無効化
- デバッグ効率の低下

**具体的修正内容**:
```csharp
// 開発環境では詳細なエラー情報を含める（一時的にコメントアウト）
// details = context.RequestServices.GetRequiredService<IWebHostEnvironment>().IsDevelopment() 
//     ? new
//     {
//         type = exception.GetType().FullName,
//         message = exception.Message,
//         stackTrace = exception.StackTrace
//     }
//     : null
```

**影響**:
- 開発環境でのエラー調査効率低下
- デバッグ情報の不足

**Phase A4での対応方針**:
- 適切な環境別エラーハンドリング設定の確立
- 開発・本番環境での情報表示レベルの最適化

### 3. **ルーティング設定の根本的未解決**

**問題**:
- MVC/Blazorルーティング競合の根本解決未完了
- `MapFallbackToPage`削除による簡易的対応
- `HomeController.Index()`が呼び出されない問題未解決

**具体的修正内容**:
```csharp
// 🎯 Blazor設定: 管理画面用ルーティング（特定パスのみ）
app.MapGet("/admin", context => 
{
    context.Response.Redirect("/admin/users");
    return Task.CompletedTask;
});
app.MapGet("/admin/users", async context =>
{
    await context.Response.WriteAsync("Blazor Admin Page - Users");
});
// ... 他の簡易的ルート設定
```

**影響**:
- Blazor Serverの適切な動作不可
- ルートパス（`/`）でのMVCビュー表示不可
- 管理画面の不完全な動作

**Phase A4での対応方針**:
- MVC/Blazor共存アーキテクチャの体系的設計
- 適切なルーティング戦略の確立
- 認証フローとルーティングの統合設計

### 4. **テストコードの未対応**

**問題**:
- 依存関係注入修正に対応したテスト更新未実施
- 統合テストの整合性確認未完了
- リグレッションテスト未実施

**影響**:
- テストカバレッジの低下
- 継続的インテグレーションの品質保証機能低下
- 将来の修正時のリグレッション検出不可

**Phase A4での対応方針**:
- テストファースト開発の徹底適用
- 修正に対応したテストケース設計・実装
- 統合テスト・回帰テストの完全実施

### 5. **プロパティファイル設定の追加**

**問題**:
- `launchSettings.json`の新規作成による環境設定変更
- 開発環境設定の体系性確認未完了

**具体的修正内容**:
```json
{
  "profiles": {
    "UbiquitousLanguageManager.Web": {
      "commandName": "Project",
      "dotnetRunMessages": true,
      "launchBrowser": true,
      "applicationUrl": "http://localhost:5000",
      "environmentVariables": {
        "ASPNETCORE_ENVIRONMENT": "Development"
      }
    }
  }
}
```

**Phase A4での対応方針**:
- 環境設定の体系的整理
- 開発・本番環境設定の最適化

## 🎯 **Phase A4での推奨アプローチ**

### **ADR_013組織的解決の適用**

1. **Phase A4 Step1: 問題分析**
   - **Clean Architecture専門役割**: F#/C#境界・DI設計の根本分析
   - **Blazor Server専門役割**: ルーティング・認証統合の体系分析
   - **テスト基盤専門役割**: テスト戦略・品質保証の包括分析
   - **統合分析**: 各問題の相互依存関係・解決優先順位の決定

2. **Phase A4 Step2以降: 体系的実装**
   - **TDD適用**: Red-Green-Refactorサイクルによる段階的解決
   - **仕様準拠チェック**: 各修正の機能仕様書準拠確認
   - **統合テスト**: 全体動作の品質保証

### **成功基準**

- [ ] 全ての「一時的コメントアウト」の解消
- [ ] Clean Architectureの適切な依存関係注入設定
- [ ] MVC/Blazor共存の適切なルーティング設定
- [ ] 完全なテストカバレッジの確保
- [ ] 機能仕様書準拠の確認
- [ ] 本番品質での動作確認

## 📝 **教訓・学習事項**

### **今回の経験から得られた教訓**

1. **複雑な技術課題の早期判断**: 複数層・複数技術にわたる問題はADR_013適用を初期判断
2. **場当たり的修正の危険性**: 一時的解決は技術的負債の蓄積につながる
3. **組織的アプローチの価値**: Phase適応型組織化による体系的解決の重要性

### **Phase A4以降への提言**

- 技術課題の性質を早期に見極め、適切なアプローチを選択する
- 「動作する」と「本番品質」の区別を明確にする
- テストファースト開発を徹底し、品質を継続的に保証する

---

**注意**: この記録は参考情報です。Phase A4では、これらの問題を根本から解決するため、ADR_013に従った体系的なアプローチを採用することを強く推奨します。