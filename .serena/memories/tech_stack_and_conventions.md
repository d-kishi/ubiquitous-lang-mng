# 技術スタック・規約（2025-09-16更新）

## 🔧 確立済み技術基盤

### **フロントエンド（Blazor Server）**
- **Pure Blazor Server**: 統一アーキテクチャ確立
- **JavaScript interop**: auth-api.js適正化済み・責任分離実装
- **API統合**: 個別API関数（login・changePassword・logout）
- **SignalR制約対応**: Cookie削除・認証処理のAPI経由実装

### **バックエンド（ASP.NET Core 8.0）**
- **Clean Architecture**: 97点達成・健全アーキテクチャ基盤確立
- **F# Domain/Application層**: 85%活用・Railway-oriented Programming
- **C# Infrastructure/Web層**: ASP.NET Core Identity統合
- **TypeConverter基盤**: 1,539行・F#↔C#境界最適化完成

### **データベース**
- **PostgreSQL 16**: Docker Container運用
- **Entity Framework Core**: Infrastructure層統合
- **初期データ**: admin@ubiquitous-lang.com / su 確立

## 📋 実装規約・パターン

### **F# Domain層規約（2025-09-16確立）**

#### **AuthenticationError型拡張**
```fsharp
type AuthenticationError =
    | InvalidCredentials                           // 認証情報が正しくない
    | UserNotFound of Email                        // ユーザーが見つからない
    | ValidationError of string                    // バリデーションエラー
    | AccountLocked of Email * DateTime            // アカウントロックアウト
    | SystemError of exn                           // システムエラー
    | PasswordExpired of Email                     // パスワード期限切れ
    | TwoFactorRequired of Email                   // 二要素認証が必要
    // 🔐 Phase A9: パスワードリセット関連エラー（4種類）
    | PasswordResetTokenExpired of Email
    | PasswordResetTokenInvalid of Email
    | PasswordResetNotRequested of Email
    | PasswordResetAlreadyUsed of Email
    // 🔒 Phase A9: トークン関連エラー（4種類）
    | TokenGenerationFailed of string
    | TokenValidationFailed of string
    | TokenExpired of string
    | TokenRevoked of string
    // 👮 Phase A9: 管理者操作関連エラー（3種類）
    | InsufficientPermissions of string
    | OperationNotAllowed of string
    | ConcurrentOperationDetected of string
    // 🔮 Phase A9: 将来拡張用エラー（4種類）
    | TwoFactorAuthFailed of Email
    | ExternalAuthenticationFailed of string
    | AuditLogError of string
    | AccountDeactivated
```

#### **Railway-oriented Programming規約**
- **継続適用**: Result型・Option型活用
- **型安全性**: エラーハンドリングの明確化・予測可能性向上
- **ビジネスロジック集約**: 85%以上F#層実装

### **TypeConverter基盤規約（1,539行）**

#### **基盤構成**
- **TypeConverters.cs**: 726行（基盤統合メソッド）
- **AuthenticationConverter.cs**: 689行（認証特化拡張）
- **AuthenticationMapper.cs**: 124行（レガシー互換）

#### **認証特化変換**
```csharp
// ✅ 推奨: 21種類AuthenticationError完全対応
public static AuthenticationErrorDto ToDto(AuthenticationError error)
{
    // F# 判別共用体の安全なパターンマッチング
    if (error.IsInvalidCredentials)
        return AuthenticationErrorDto.InvalidCredentials();
    // ... 21種類の完全対応
}

// ✅ 推奨: 双方向変換・null安全性
public static FSharpResult<User, AuthenticationError> FromDto(UserDto dto)
{
    // C# → F# 安全な型変換
}
```

### **Clean Architecture規約（97点）**
- **依存方向**: Web→Application→Domain（厳格遵守）
- **インターフェース**: 全層間でインターフェース経由
- **循環依存**: 完全禁止・Infrastructure層基盤サービス化
- **薄いラッパー設計**: Web層責務特化・Infrastructure層委譲

## 🎯 品質基準・メトリクス

### **必達品質基準**
- **Clean Architecture**: 97点以上（Phase A9確立済み）
- **ビルド品質**: 0警告0エラー継続
- **テスト品質**: 全テスト成功維持
- **動作品質**: E2E認証フロー完全動作

### **コード品質指標**
- **責任分離**: 単一責任原則適用率95%以上
- **F# Domain層活用**: 85%以上（Phase A9確立済み）
- **可読性**: 複雑度削減・コメント適切配置
- **保守性**: 変更影響範囲限定・修正容易性確保
- **テスタビリティ**: モック注入可能・単体テスト容易

## 🔄 技術的発見・パターン（2025-09-16）

### **F# Domain層拡張パターン**
- **判別共用体拡張**: 7種類→21種類・業務要件に応じた体系的拡張
- **Railway-oriented Programming**: 型安全・エラーハンドリング明確化
- **F#↔C#統合**: TypeConverter基盤による効率的境界処理

### **Clean Architecture実践パターン**
- **97点達成手法**: 依存関係適正化・循環依存解消・インターフェース依存統一
- **薄いラッパー設計**: Web層責務特化・Infrastructure層基盤委譲
- **健全アーキテクチャ**: 各層責務明確・単一責任原則適用

### **技術負債解消パターン**
- **設計債務**: 複雑性削減→理解しやすさ向上
- **保守債務**: 責任分離→長期保守負荷軽減（50%削減達成）
- **アーキテクチャ債務**: Clean Architecture準拠による健全性確保