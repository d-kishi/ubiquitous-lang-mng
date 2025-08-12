# ASP.NET Core Identity設計見直し技術調査結果

**調査実施日**: 2025-08-12  
**対象技術負債**: TECH-001 - ASP.NET Core Identity設計見直し  
**調査範囲**: ベストプラクティス・マイグレーション戦略・カスタム実装最適化・将来拡張対応  

## 技術調査結果

### 調査対象
ASP.NET Core Identity標準実装への回帰とベストプラクティス適用による技術負債解消手法

### 主要な発見

#### 1. ASP.NET Core Identity 標準実装のベストプラクティス

**推奨実装パターン**
- AddEntityFrameworkStores<TContext>()による標準実装使用
- Microsoft公式サポート、コミュニティ知識の活用、将来のアップデート対応
- 現状のカスタムUserStore/RoleStore実装が保守性を阻害

**Claimsテーブル活用の重要性**
- AspNetUserClaims: ユーザー固有の権限・属性管理
- AspNetRoleClaims: ロール継承による権限管理効率化
- 活用例: 組織・プロジェクト固有権限、機能別アクセス制御

**EntityFrameworkStores適切設定**
- Scopedライフタイム: DBコンテキストとの整合性確保
- 全Interface実装: IUserStore、IUserPasswordStore、IUserClaimStore等
- グローバルクエリフィルター: マルチテナント対応時の自動データ分離

#### 2. マイグレーション戦略

**安全なマイグレーション手順**
段階的実装により既存データへの影響を最小化：

Phase 1: Claimテーブル追加
```sql
CREATE TABLE "AspNetUserClaims" (
    "Id" SERIAL PRIMARY KEY,
    "UserId" VARCHAR(450) NOT NULL,
    "ClaimType" TEXT,
    "ClaimValue" TEXT,
    CONSTRAINT "FK_AspNetUserClaims_AspNetUsers_UserId" 
        FOREIGN KEY ("UserId") REFERENCES "AspNetUsers"("Id") ON DELETE CASCADE
);
```

Phase 2: インデックス追加（パフォーマンス最適化）
```sql
CREATE INDEX "IX_AspNetUserClaims_UserId" ON "AspNetUserClaims" ("UserId");
CREATE INDEX "IX_AspNetRoleClaims_RoleId" ON "AspNetRoleClaims" ("RoleId");
```

**ダウンタイム削減手法**
- 段階的マイグレーション: テーブル追加→コード変更→カスタム実装削除
- Idempotent Scripts使用: 複数環境への安全な適用
- SQL Script Generation: 本番環境での手動レビュー・適用

#### 3. カスタム実装の標準化

**ClaimsPrincipalFactory最適化**
最小限のカスタマイゼーションに留める：

```csharp
public class CustomClaimsPrincipalFactory : UserClaimsPrincipalFactory<ApplicationUser, ApplicationRole>
{
    public override async Task<ClaimsPrincipal> CreateAsync(ApplicationUser user)
    {
        var principal = await base.CreateAsync(user);
        var identity = (ClaimsIdentity)principal.Identity;
        
        // 必要最小限のカスタムClaim追加のみ
        identity.AddClaim(new Claim("organization_id", user.OrganizationId.ToString()));
        
        return principal;
    }
}
```

**DI設定最適化**
```csharp
services.AddDefaultIdentity<ApplicationUser>()
    .AddRoles<ApplicationRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddClaimsPrincipalFactory<CustomClaimsPrincipalFactory>();
```

#### 4. 将来拡張への対応

**OAuth/OpenID Connect統合準備**
- Backend for Frontend (BFF) Pattern: SPAでの安全な認証
- Authorization Code Flow + PKCE: 2025年推奨標準
- Claims Transformation: 外部IDプロバイダーとの統合

**JWT Token対応**
- 標準準拠: 自作Token禁止、OpenID Connect準拠Token使用
- セキュリティ: 非対称鍵使用、適切なToken有効期限設定

**マルチテナント拡張**
- データ分離パターン: 行レベル・スキーマレベル・データベースレベル
- テナント解決: サブドメイン・ヘッダー・JWT Claim
- EF Core統合: GlobalQueryFilter活用

### 推奨アプローチ

#### Phase B1着手前実施計画

**Step 1: データベーススキーマ更新**
```bash
dotnet ef migrations add AddIdentityClaimTables
dotnet ef database update
```

**Step 2: カスタム実装段階的削除**
1. CustomUserStore.cs → 削除
2. CustomRoleStore.cs → 削除  
3. CustomUserClaimsPrincipalFactory.cs → 最小限に簡略化

**Step 3: 標準実装への切り替え**
```csharp
services.AddDefaultIdentity<ApplicationUser>(options => {
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 8;
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
    options.SignIn.RequireConfirmedEmail = true;
})
.AddRoles<ApplicationRole>()
.AddEntityFrameworkStores<UbiquitousLanguageDbContext>()
.AddDefaultTokenProviders();
```

### 実装例・参考リンク

**参考実装パターン**
- Microsoft公式: Identity model customization
- Migration策: Safe Identity Migrations
- Claims最適化: Claims transformation best practices

**コード例集**
```csharp
public class MinimalIdentityConfiguration : IConfigureOptions<IdentityOptions>
{
    public void Configure(IdentityOptions options)
    {
        options.Password.RequireDigit = true;
        options.Password.RequiredLength = 8;
        options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
        options.Lockout.MaxFailedAccessAttempts = 5;
        options.SignIn.RequireConfirmedEmail = true;
    }
}
```

### リスク・考慮事項

**技術的リスク**
- 中リスク: マイグレーション実行時の一時的サービス停止
  - 対策: メンテナンス時間での実施、ロールバック計画準備
- 低リスク: 既存機能への影響
  - 理由: Phase A完了機能は標準テーブル使用

**運用考慮事項**
- 学習コスト削減: 一般的なASP.NET Core Identity知識で対応可能
- 保守性向上: Microsoft公式アップデート・セキュリティパッチの恩恵
- 拡張性確保: Claims機能による柔軟な権限管理実装

**パフォーマンス影響**
- 性能向上: 最適化されたEntityFrameworkStores使用
- スケーラビリティ: 標準実装によるキャッシュ・最適化機能活用

### 技術判断・推奨度

#### 強く推奨 (5/5)
- AspNetUserClaims/AspNetRoleClaimsテーブル追加
- カスタムUserStore/RoleStore削除  
- AddEntityFrameworkStores()標準実装使用

#### 推奨 (4/5)
- ClaimsPrincipalFactory最小限簡略化
- マイグレーション段階的実施

#### 将来検討 (3/5)
- OAuth/OpenID Connect統合準備
- マルチテナント機能拡張

## 結論

現在のカスタムUserStore/RoleStore実装は、ASP.NET Core Identityの標準的な使用方法から大きく逸脱し、保守性・拡張性・学習コストの観点で技術負債となっている。

AspNetUserClaims/AspNetRoleClaimsテーブルを追加し、標準的なEntityFrameworkStores実装に回帰することで、以下の利益が得られる：

1. 保守性向上: 一般的な知識での対応可能性
2. 拡張性確保: Claims機能による柔軟な権限管理
3. 将来対応: OAuth/OpenID Connect/マルチテナント拡張への準備
4. セキュリティ: Microsoft公式サポート・アップデートの恩恵

**Phase B1着手前の早期対応を強く推奨する。**

---

Generated with Claude Code
Co-Authored-By: Claude <noreply@anthropic.com>
ENDFILE < /dev/null
