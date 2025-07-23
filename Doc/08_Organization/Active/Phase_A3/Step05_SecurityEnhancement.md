# Phase A3: Step5 - セキュリティ強化機能実装（TDD実践版）

**Step種別**: セキュリティ強化実装  
**作成日**: 2025-07-23  
**実施予定**: 次回セッション  
**Step責任者**: Claude Code  
**想定所要時間**: 120分  

## 📋 Step概要

### Step目的
Phase A3の中核ステップとして、監査ログの永続化、セキュリティイベント通知、Remember Me UIの実装を通じて、エンタープライズレベルのセキュリティ機能を完成させる。**テストファースト開発（TDD）**を厳格に適用し、高品質なセキュリティ機能を実現する。

### 主要タスク
1. AuditEventsテーブル・エンティティ追加（DB永続化）
2. IAuditLogger完全実装（EF Core統合）
3. セキュリティ通知メール機能（アカウントロック・設定変更通知）
4. Remember Me UI実装（チェックボックス・Cookie管理）
5. セキュリティポリシー設定機能

## 🏢 組織設計（TDD実践体制）

### フィーチャーチーム構成

#### Feature Team 1: 監査ログ永続化機能
**責任範囲**: AuditEventsテーブル設計・エンティティ実装・IAuditLogger実装
**TDD実践計画**:
- Red Phase: 
  - AuditEvent作成・保存テスト（境界値・異常系含む）
  - IAuditLogger各メソッドのテスト（非同期処理・エラーハンドリング）
  - EF Core統合テスト（トランザクション・並行性）
- Green Phase: 
  - AuditEventエンティティ・DbContext設定
  - AuditLogger実装（非同期保存・バッチ処理）
  - マイグレーション作成・実行
- Refactor Phase: 
  - パフォーマンス最適化（バッチ保存・インデックス）
  - エラーハンドリング強化
  - ログフォーマット標準化
**技術スタック**: EF Core、PostgreSQL、F# Domain層

#### Feature Team 2: セキュリティ通知機能
**責任範囲**: セキュリティイベント検知・通知メール送信・通知設定管理
**TDD実践計画**:
- Red Phase:
  - アカウントロック通知テスト（閾値・条件）
  - 設定変更通知テスト（パスワード・権限変更）
  - 通知設定管理テスト（有効/無効・通知先）
- Green Phase:
  - ISecurityNotificationService実装
  - 各種通知メールテンプレート作成
  - 通知設定管理機能実装
- Refactor Phase:
  - 通知ロジックの統一化
  - テンプレートエンジン導入検討
  - 非同期通知処理最適化
**技術スタック**: IEmailSender（Step2基盤）、BackgroundService

#### Feature Team 3: Remember Me UI・統合
**責任範囲**: ログイン画面UI改修・Cookie管理・セキュリティポリシー統合
**TDD実践計画**:
- Red Phase:
  - Remember Meチェックボックス動作テスト
  - Cookie設定・読み取りテスト（有効期限・セキュア属性）
  - ポリシー適用テスト（強制ログアウト・セッション管理）
- Green Phase:
  - Login.razor改修（Remember Me UI追加）
  - Cookie管理ロジック実装
  - セキュリティポリシー適用機能
- Refactor Phase:
  - UI/UXの改善（アクセシビリティ対応）
  - Cookie管理の抽象化
  - ポリシー設定の外部化（appsettings.json）
**技術スタック**: Blazor Server、ASP.NET Core Identity、Cookie認証

## TDDサイクル実行計画

### 0:00-0:30 [Red Phase - テスト作成]
├── Team 1: 監査ログ永続化テストスイート作成
│   ├── AuditEventエンティティ単体テスト
│   ├── IAuditLogger実装テスト
│   └── EF Core統合テスト
├── Team 2: セキュリティ通知テストスイート作成
│   ├── 通知条件判定テスト
│   ├── メール送信テスト
│   └── 通知設定管理テスト
├── Team 3: Remember Me UIテストスイート作成
│   ├── UIコンポーネントテスト（bUnit）
│   ├── Cookie管理テスト
│   └── セキュリティポリシーテスト
└── 🔴 必須チェックポイント: 全テスト失敗の確認

### 0:30-0:70 [Green Phase - 最小実装]
├── Team 1: 監査ログ永続化実装
│   ├── エンティティ・DbContext実装
│   ├── AuditLogger最小実装
│   └── マイグレーション実行
├── Team 2: セキュリティ通知実装
│   ├── 通知サービス最小実装
│   ├── メールテンプレート作成
│   └── 通知設定基本機能
├── Team 3: Remember Me UI実装
│   ├── ログイン画面UI改修
│   ├── Cookie管理基本実装
│   └── ポリシー適用基本機能
└── 🟢 必須チェックポイント: 全テスト成功の確認

### 0:70-0:120 [Refactor Phase - 品質向上]
├── Team 1: 監査ログ最適化
│   ├── パフォーマンスチューニング
│   ├── エラーハンドリング強化
│   └── コード整理・ドキュメント化
├── Team 2: 通知機能改善
│   ├── 非同期処理最適化
│   ├── テンプレート管理改善
│   └── 通知ロジック統一化
├── Team 3: UI/UX・統合改善
│   ├── アクセシビリティ対応
│   ├── Cookie管理抽象化
│   └── 全体統合テスト
├── 全チーム統合・品質レビュー
└── 🔵 必須チェックポイント: リファクタリング後のテスト成功確認

## 🎯 期待成果

### 実装成果
- [ ] AuditEventsテーブル・エンティティ（マイグレーション完了）
- [ ] IAuditLogger完全実装（非同期保存・バッチ処理対応）
- [ ] セキュリティ通知メール機能（3種類以上の通知パターン）
- [ ] Remember Me UI（ログイン画面統合）
- [ ] セキュリティポリシー設定機能（設定可能・永続化）

### TDD実践成果
- [ ] 各機能のテストケース先行作成（Red Phase完了証跡）
- [ ] テスト失敗→成功のサイクル記録
- [ ] テストカバレッジ80%以上達成
- [ ] リファクタリング前後のテスト成功維持

### セキュリティ基準
- [ ] 監査ログの改ざん防止（ハッシュ値記録）
- [ ] 通知メールのセキュア送信（TLS必須）
- [ ] Cookie属性適切設定（HttpOnly、Secure、SameSite）
- [ ] OWASP Top 10準拠確認
- [ ] セキュリティポリシー強制適用

## 📊 技術的詳細

### 主要コンポーネント設計

#### AuditEventエンティティ（Domain層）
```fsharp
namespace UbiquitousLanguageManager.Domain.Entities

type AuditEventType =
    | LoginSuccess
    | LoginFailure
    | PasswordChanged
    | AccountLocked
    | AccountUnlocked
    | PermissionChanged
    | SecurityPolicyViolation

type AuditEvent = {
    Id: Guid
    UserId: Guid option
    EventType: AuditEventType
    EventTime: DateTimeOffset
    IpAddress: string option
    UserAgent: string option
    Details: string
    Severity: string // Information, Warning, Error
    Hash: string // 改ざん防止用
}
```

#### IAuditLogger拡張（Application層）
```fsharp
type IAuditLogger =
    // 既存メソッド...
    
    // Step5追加メソッド
    abstract member LogSecurityEventAsync: eventType: AuditEventType -> userId: Guid option -> details: string -> Task<Result<unit, string>>
    abstract member GetAuditEventsAsync: userId: Guid -> startDate: DateTimeOffset -> endDate: DateTimeOffset -> Task<Result<AuditEvent list, string>>
    abstract member PurgeOldAuditEventsAsync: retentionDays: int -> Task<Result<int, string>>
```

#### セキュリティ通知サービス（Application層）
```fsharp
type SecurityNotificationType =
    | AccountLockedNotification
    | PasswordChangedNotification
    | SuspiciousActivityNotification
    | SecurityPolicyChangeNotification

type ISecurityNotificationService =
    abstract member SendSecurityNotificationAsync: userId: Guid -> notificationType: SecurityNotificationType -> details: string -> Task<Result<unit, string>>
    abstract member GetNotificationSettingsAsync: userId: Guid -> Task<Result<NotificationSettings, string>>
    abstract member UpdateNotificationSettingsAsync: userId: Guid -> settings: NotificationSettings -> Task<Result<unit, string>>
```

### Remember Me UI実装
- `/Pages/Auth/Login.razor` - チェックボックス追加
- Cookie設定: 30日間有効、HttpOnly、Secure必須
- ポリシー連携: 強制ログアウト時のCookie無効化

## 🚨 リスク・前提条件

### 技術的前提
- Step1-4が正常完了している
- データベースマイグレーション実行環境が整備済み
- メール送信基盤（Step2）が正常稼働中
- ASP.NET Core Identity統合完了

### 識別されたリスク
1. **データベース負荷**: 監査ログの大量書き込みによるパフォーマンス影響
2. **通知の過剰送信**: セキュリティイベントの頻発による通知疲れ
3. **Cookie管理複雑性**: 複数認証方式との整合性確保
4. **TDD実践の時間超過**: テスト作成に時間がかかりすぎる可能性

### リスク対策
- 監査ログのバッチ処理・非同期保存実装
- 通知頻度制限・集約機能の実装
- Cookie管理の抽象化レイヤー導入
- タイムボックス管理・必要に応じた調整

### Step4からの申し送り事項
- SignInManager統合パターン確立済み
- ASP.NET Core Identity Lockout基本実装完了
- 既存テストのコンストラクタエラーは解決済み
- F#/C#相互運用パターン確立済み

---

**次回更新**: Step5実行時（実行記録・TDD実践記録追加）