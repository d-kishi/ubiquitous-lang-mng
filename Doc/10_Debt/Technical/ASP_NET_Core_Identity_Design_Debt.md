# ASP.NET Core Identity設計技術負債

**発生日**: 2025-08-05  
**優先度**: High  
**カテゴリ**: アーキテクチャ設計・保守性  
**Phase**: Phase A4完了後に発見、Phase B1着手前修正予定  

## 問題概要

ASP.NET Core IdentityでClaim/RoleClaimテーブルを除外した設計により、実装が複雑化している。

## 現状の実装

### 対象ファイル
- `/src/UbiquitousLanguageManager.Infrastructure/Identity/CustomUserStore.cs`
- `/src/UbiquitousLanguageManager.Infrastructure/Identity/CustomRoleStore.cs` 
- `/src/UbiquitousLanguageManager.Infrastructure/Identity/CustomUserClaimsPrincipalFactory.cs`
- `/src/UbiquitousLanguageManager.Web/Program.cs` (DI登録部分)

### 現在の設計問題
1. **データベース設計**: IdentityUserClaim/IdentityRoleClaimテーブルを意図的に除外
2. **カスタム実装**: UserStore/RoleStoreをカスタム実装してClaimテーブルアクセスを無効化
3. **実装複雑性**: ASP.NET Core Identity標準から大きく逸脱

## 問題点詳細

### 保守性の問題
- ASP.NET Core Identityの標準実装から外れる
- 一般的でない実装パターン
- Microsoft公式サポートの恩恵を受けにくい

### 拡張性の問題
- 将来的なClaim機能追加が困難
- 権限管理の柔軟性が制限される
- 外部システム連携時のClaim活用が困難

### 学習コストの問題
- 開発者が一般的なIdentity知識で対応できない
- カスタム実装の理解・保守に専門知識が必要

## 推奨解決策

### Phase B1着手前修正計画

#### 1. データベース設計見直し
```sql
-- 必要最小限のIdentityテーブルを追加
CREATE TABLE "AspNetUserClaims" (
    "Id" SERIAL PRIMARY KEY,
    "UserId" VARCHAR(450) NOT NULL,
    "ClaimType" TEXT,
    "ClaimValue" TEXT,
    CONSTRAINT "FK_AspNetUserClaims_AspNetUsers_UserId" 
        FOREIGN KEY ("UserId") REFERENCES "AspNetUsers"("Id") ON DELETE CASCADE
);

CREATE TABLE "AspNetRoleClaims" (
    "Id" SERIAL PRIMARY KEY,
    "RoleId" VARCHAR(450) NOT NULL,
    "ClaimType" TEXT,
    "ClaimValue" TEXT,
    CONSTRAINT "FK_AspNetRoleClaims_AspNetRoles_RoleId" 
        FOREIGN KEY ("RoleId") REFERENCES "AspNetRoles"("Id") ON DELETE CASCADE
);
```

#### 2. カスタム実装削除
- `CustomUserStore.cs` 削除
- `CustomRoleStore.cs` 削除
- `CustomUserClaimsPrincipalFactory.cs` 削除（必要に応じて簡略化して残す）

#### 3. Program.cs修正
```csharp
// 標準的な実装に戻す
.AddEntityFrameworkStores<UbiquitousLanguageDbContext>()
.AddDefaultTokenProviders();
```

#### 4. マイグレーション実行
```bash
dotnet ef migrations add AddIdentityClaimTables --startup-project ../UbiquitousLanguageManager.Web
dotnet ef database update --startup-project ../UbiquitousLanguageManager.Web
```

## 影響範囲

### ✅ 影響なし
- Phase A完了済み機能（ログイン・ユーザー管理等）
- 既存のユーザーデータ

### 📈 改善される項目
- 実装の保守性向上
- 将来の機能拡張容易性
- 開発者の学習コスト削減

## 実施スケジュール

- **Phase B1着手前**: データベース設計書更新
- **Phase B1開始時**: マイグレーション実行・カスタム実装削除
- **Phase B1 Step1**: 動作確認・テスト実行

## 関連文書

- `/Doc/02_Design/データベース設計書.md` - 修正対象
- `/Doc/07_Decisions/` - 必要に応じてADR作成検討
- `/Doc/06_Issues/課題一覧.md` - 関連課題管理

## 備考

現在の実装は動作するため、Phase A4完了には影響しない。
Phase B1以降の開発効率向上のため、早期の解決を推奨。