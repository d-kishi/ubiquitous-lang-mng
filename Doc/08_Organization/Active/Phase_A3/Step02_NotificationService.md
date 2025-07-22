# Phase A3: Step2 - NotificationService基盤構築

**Step種別**: 基盤実装  
**作成日**: 2025-07-22  
**実施予定**: 次回セッション  
**Step責任者**: Claude Code  
**想定所要時間**: 90分  

## 📋 Step概要

### Step目的
Phase A3の中核となるメール送信基盤（NotificationService）を構築する。Clean Architectureに準拠したインターフェース定義と、MailKitを使用した実装、開発環境でのSMTP設定を完成させる。

### 主要タスク
1. IEmailSenderインターフェース定義（Application層）
2. MailKitEmailSender実装（Infrastructure層）
3. BackgroundEmailQueueサービス実装
4. Smtp4dev環境設定・動作確認

## 🏢 組織設計

### チーム構成（2チーム体制）

#### Team 1: インターフェース設計・アーキテクチャチーム
**役割**: Clean Architecture準拠のインターフェース設計、DI設定
**専門領域**: F#、Application層設計、依存性注入
**主要タスク**:
- IEmailSenderインターフェース定義（F# Application層）
- IBackgroundEmailQueueインターフェース定義
- DIコンテナ登録設計（Program.cs）
- Clean Architecture準拠確認

#### Team 2: 実装・インフラチーム
**役割**: 具体的な実装とインフラ設定
**専門領域**: C#、Infrastructure層、SMTP、非同期処理
**主要タスク**:
- MailKitEmailSender実装（C# Infrastructure層）
- BackgroundEmailQueue実装（Channel<T>使用）
- EmailSendingBackgroundService実装
- SmtpSettings設定管理
- Smtp4dev環境構築・テスト

### 実行計画
```
0:00-0:30 [並行作業]
├── Team 1: インターフェース定義・アーキテクチャ設計
└── Team 2: 実装準備・Smtp4dev環境構築

0:30-0:60 [統合実装]
├── MailKitEmailSender実装
├── BackgroundService実装
└── DI設定・統合

0:60-0:90 [動作確認・テスト]
├── Smtp4devでの送信テスト
├── 非同期処理動作確認
└── 単体テスト作成
```

## 🎯 期待成果

### 実装成果
- [ ] IEmailSenderインターフェース完成（Application層）
- [ ] MailKitEmailSender実装完成（Infrastructure層）
- [ ] BackgroundEmailQueueサービス稼働
- [ ] Smtp4dev環境での送信確認

### 品質基準
- [ ] Clean Architecture準拠（依存方向確認）
- [ ] 非同期処理の適切な実装
- [ ] エラーハンドリング実装
- [ ] 単体テスト作成（モック可能な設計）

### ドキュメント成果
- [ ] 実装コードのコメント（初学者向け詳細）
- [ ] Smtp4devセットアップガイド
- [ ] 技術的決定事項の記録

## 📊 技術的詳細

### 実装予定コンポーネント

#### IEmailSender（Application層）
```fsharp
namespace UbiquitousLanguageManager.Application.Interfaces

type IEmailSender =
    abstract member SendEmailAsync: email: string -> subject: string -> htmlMessage: string -> Task
```

#### SmtpSettings（Infrastructure層）
```csharp
public class SmtpSettings
{
    public string Server { get; set; }
    public int Port { get; set; }
    public string SenderName { get; set; }
    public string SenderEmail { get; set; }
    public string Username { get; set; }
    public string Password { get; set; }
}
```

### 開発環境設定
```json
// appsettings.Development.json
"SmtpSettings": {
    "Server": "localhost",
    "Port": 1025,
    "SenderName": "Ubiquitous Language Manager",
    "SenderEmail": "noreply@ubiquitous-lang.local",
    "Username": "",
    "Password": ""
}
```

## 🚨 リスク・前提条件

### 技術的前提
- MailKit 4.0以上インストール済み
- .NET 8.0環境
- Smtp4devインストール可能な環境

### 識別されたリスク
1. **F#でのインターフェース定義**: Task型の適切な扱い
2. **非同期処理**: デッドロック回避、適切なキャンセレーション
3. **開発環境差異**: Windows/Mac/Linuxでの動作確認

## 🔄 Step実行記録

### 実行開始時刻: 2025-07-22 20:30（推定）
### 実行終了時刻: 2025-07-22 21:06

### 実施内容
1. **IEmailSenderインターフェース定義**（F# Application層）完成
   - F# Result型を使用した関数型エラーハンドリング実装
   - 3つのメール送信メソッド定義（HTML、プレーンテキスト、添付ファイル付き）
   - IBackgroundEmailQueue併せて定義、C# Func<T>型使用で相互運用性確保

2. **MailKitEmailSender実装**（C# Infrastructure層）完成
   - MailKit 4.13.0使用、SmtpSettings設定クラス実装
   - 詳細なBlazor Server・F#初学者向けコメント追加（ADR_010準拠）
   - F#/C#型変換問題解決（FSharpResult<Unit, string>使用）

3. **BackgroundEmailQueue実装**完成
   - Channel<T>を使用したプロデューサー・コンシューマーパターン
   - EmailSendingBackgroundServiceによるバックグラウンド処理実装
   - 1000件キューの bounded channel設計

4. **DIコンテナ統合**（Program.cs）完成
   - SmtpSettings設定バインディング、サービス登録
   - Microsoft.Extensions.Options バージョン競合解決

5. **Smtp4dev環境構築**完成
   - 全グローバルインストール、appsettings.Development.json設定
   - localhost:1025 SMTP、localhost:5000 Web UI設定

### 成果物
**F# Application層:**
- `/src/UbiquitousLanguageManager.Application/Interfaces.fs` - IEmailSender, IBackgroundEmailQueue定義

**C# Infrastructure層:**
- `/src/UbiquitousLanguageManager.Infrastructure/Services/SmtpSettings.cs` - SMTP設定クラス
- `/src/UbiquitousLanguageManager.Infrastructure/Services/MailKitEmailSender.cs` - MailKit実装
- `/src/UbiquitousLanguageManager.Infrastructure/Services/BackgroundEmailQueue.cs` - キューサービス
- `/src/UbiquitousLanguageManager.Infrastructure/Services/EmailSendingBackgroundService.cs` - バックグラウンドサービス

**テスト:**
- `/tests/UbiquitousLanguageManager.Tests/Infrastructure/MailKitEmailSenderTests.cs` - 単体テスト（9テストケース）
- `/tests/UbiquitousLanguageManager.Tests/Infrastructure/BackgroundEmailQueueTests.cs` - 単体テスト（10テストケース）
- `/tests/UbiquitousLanguageManager.Tests/Integration/EmailServiceIntegrationTests.cs` - 統合テスト（4テストケース）

**設定・統合:**
- `/src/UbiquitousLanguageManager.Web/Program.cs` - DI登録
- `/src/UbiquitousLanguageManager.Web/appsettings.Development.json` - SMTP設定

### 発見事項・課題
1. **F#/C#型変換の複雑性**
   - F# Result型 → FSharpResult<Unit, string>に変更して相互運用性確保
   - F# Function型 → System.Func<T>直接使用でF#/C#間の変換問題回避

2. **パッケージバージョン競合**
   - Microsoft.Extensions.Options 8.0.0 → 9.0.7 アップグレードで解決
   - Microsoft.Extensions.Hosting.Abstractions 9.0.7 追加

3. **XML コメント処理**
   - ジェネリック型のエスケープ（`<T>` → `&lt;T&gt;`）
   - 初学者向け詳細コメントの体系的実装

4. **テスト環境の依存関係**
   - Smtp4dev起動チェック機能実装
   - CI/ローカル環境での適応的テスト実行

## 📋 Step2終了時レビュー（2025-07-22 21:10実施）
詳細項目は `/Doc/08_Organization/Rules/組織管理運用マニュアル.md` を参照

### レビュー結果概要
- 効率性: ✅ 達成度100%
- 専門性: ✅ 活用度5
- 統合性: ✅ 効率度5  
- 品質: ✅ 達成度5
- 適応性: ✅ 適応度5

### 主要学習事項
- 成功要因: F#/C#相互運用性問題の体系的解決、テストファースト開発の徹底実施
- 改善要因: パッケージバージョン管理の事前確認、開発環境依存テストの適応性向上

### 次Step組織設計方針
Phase A3 Step3（パスワードリセット機能実装）に向けて：
- Step2で構築したメール送信基盤の活用に重点
- UI実装（Blazor Server）とメール送信統合の専門性分離
- パスワードリセット token生成・検証ロジックの独立実装
- 既存認証システムとの統合における境界設計

---

**次回更新**: Step2実行時（実行記録・レビュー結果追加）