# Phase A3以降実装計画・技術負債管理

**作成日**: 2025-07-20  
**更新日**: Phase A2 Step 3-1完了時点  
**対象**: Infrastructure層スタブメソッド（27メソッド）

## 📊 技術負債概要

### 実装済み基本機能
- ✅ UserRepository（CRUD・検索・統計）
- ✅ AuthenticationService（Login・ChangePassword）
- ✅ NotificationService（基本ログ出力）

### 未実装機能（スタブ実装済み）
- 🚧 認証機能拡張：15メソッド
- 🚧 通知機能拡張：9メソッド
- 🚧 ユーザー管理高度機能：3メソッド

## 🔐 認証機能拡張（AuthenticationService）

### 🔴 高優先度（Phase A3必須）
1. **CreateUserWithPasswordAsync**
   - **機能**: 管理者によるユーザー作成（パスワード付き）
   - **ユーザー価値**: 管理者がユーザーを効率的に作成可能
   - **技術的依存**: ASP.NET Core Identity UserManager
   - **工数見積**: 30-40分

2. **LockUserAsync / UnlockUserAsync** 
   - **機能**: セキュリティ違反時のアカウントロック・解除
   - **ユーザー価値**: セキュリティインシデント対応
   - **技術的依存**: Identity LockoutEnd管理
   - **工数見積**: 20-30分

### 🟡 中優先度（Phase A3-A4）
3. **GenerateTokenAsync / ValidateTokenAsync**
   - **機能**: API認証・セッション管理トークン
   - **ユーザー価値**: API連携・セッション継続
   - **技術的依存**: JWT・Claims管理
   - **工数見積**: 40-60分

4. **HashPasswordAsync / VerifyPasswordAsync**
   - **機能**: パスワードハッシュ化・検証の独立実装
   - **ユーザー価値**: セキュリティ強化・独自認証
   - **技術的依存**: BCrypt.Net統合
   - **工数見積**: 20-30分

5. **RecordFailedLoginAsync / RecordSuccessfulLoginAsync**
   - **機能**: ログイン試行の詳細記録・分析
   - **ユーザー価値**: セキュリティ監査・分析機能
   - **技術的依存**: 監査ログシステム
   - **工数見積**: 30-40分

### 🟢 低優先度（Phase A4以降）
6. **二要素認証系（3メソッド）**
   - EnableTwoFactorAsync / DisableTwoFactorAsync / VerifyTwoFactorCodeAsync
   - **機能**: 2FA管理・バックアップコード
   - **ユーザー価値**: 高度セキュリティ
   - **工数見積**: 60-90分

7. **メール確認系（2メソッド）**
   - SendEmailConfirmationAsync / ConfirmEmailAsync
   - **機能**: メールアドレス確認フロー
   - **ユーザー価値**: アカウント信頼性向上
   - **工数見積**: 40-60分

8. **UpdateSecurityStampAsync**
   - **機能**: セキュリティスタンプ更新（強制ログアウト）
   - **ユーザー価値**: セキュリティインシデント対応
   - **工数見積**: 15-20分

## 📧 通知機能拡張（NotificationService）

### 🔴 高優先度（Phase A3必須）
1. **SendWelcomeEmailAsync**
   - **機能**: 新規ユーザー歓迎メール・初期設定案内
   - **ユーザー価値**: オンボーディング体験向上
   - **技術的依存**: メール送信システム（SMTP/SendGrid）
   - **工数見積**: 30-40分

2. **SendSecurityAlertAsync**
   - **機能**: セキュリティイベント（ロック・異常ログイン）通知
   - **ユーザー価値**: セキュリティ状況の即座把握
   - **技術的依存**: メールテンプレート・緊急通知
   - **工数見積**: 25-35分

### 🟡 中優先度（Phase A3-A4）
3. **ユーザー管理通知（4メソッド）**
   - SendRoleChangeNotificationAsync
   - SendPasswordChangeNotificationAsync
   - SendAccountDeactivationNotificationAsync
   - SendAccountActivationNotificationAsync
   - **機能**: ユーザー状態変更の通知・履歴
   - **ユーザー価値**: 管理透明性・監査対応
   - **工数見積**: 60-80分

4. **SendEmailChangeConfirmationAsync**
   - **機能**: メールアドレス変更時の新旧確認通知
   - **ユーザー価値**: アカウント乗っ取り防止
   - **工数見積**: 20-30分

### 🟢 低優先度（Phase A4以降）
5. **承認フロー通知（2メソッド）**
   - SendApprovalRequestAsync / SendApprovalResultAsync
   - **機能**: ユビキタス言語承認プロセス通知
   - **ユーザー価値**: 承認ワークフロー効率化
   - **技術的依存**: Phase A4承認機能
   - **工数見積**: 40-60分

## 🛠️ 技術的依存関係・制約

### Phase A3で必要な技術基盤
1. **メール送信システム**
   - SMTP設定またはSendGrid統合
   - メールテンプレートシステム
   - 非同期メール送信キュー

2. **監査ログシステム**
   - セキュリティイベントログ
   - ユーザー操作履歴
   - ADR_008ログ出力規約拡張

3. **設定管理システム**
   - メール設定・SMTP認証情報
   - セキュリティポリシー設定
   - 通知設定（オン・オフ切替）

### Phase A4以降の高度機能
- JWT・API認証システム
- 二要素認証プロバイダー
- 承認ワークフローエンジン

## 📋 Phase A3実装推奨計画

### Sprint 1: 基本ユーザー管理完成（120-150分）
```
1. CreateUserWithPasswordAsync (30-40分)
2. SendWelcomeEmailAsync (30-40分)
3. LockUserAsync/UnlockUserAsync (20-30分)
4. SendSecurityAlertAsync (25-35分)
5. 統合テスト・品質確認 (15-25分)
```

### Sprint 2: セキュリティ・監査強化（100-130分）
```
1. RecordFailedLoginAsync/RecordSuccessfulLoginAsync (30-40分)
2. ユーザー管理通知4メソッド (60-80分)
3. 統合テスト・監査ログ確認 (10-15分)
```

### Sprint 3: 高度認証機能（80-120分）
```
1. GenerateTokenAsync/ValidateTokenAsync (40-60分)
2. HashPasswordAsync/VerifyPasswordAsync (20-30分)
3. SendEmailChangeConfirmationAsync (20-30分)
```

## 🔄 継続的管理プロセス

### Phase完了時
- 実装済み機能の✅マーク更新
- 新規発見技術負債の追加
- 優先度の見直し・調整

### 実装判断基準
- **即時実装**: ユーザー価値が高く、技術的依存が少ない
- **計画実装**: ユーザー価値は高いが、技術基盤準備が必要
- **将来実装**: nice-to-have機能、高度な技術的依存

---

**管理者**: Claude Code（自動更新）  
**レビュー**: Phase完了時にユーザー確認