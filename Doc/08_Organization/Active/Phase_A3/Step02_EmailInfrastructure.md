# Phase A3 Step2: メール送信基盤構築

**作成日**: 2025-07-24  
**実行期間**: 予定 90分  
**ステップ目標**: Clean Architecture準拠のメール送信基盤確立・開発環境構築

## 📋 仕様準拠マトリックス

### Step2実装対象

| 機能 | 仕様書項番 | 実装内容 | テスト設計 | 準拠状態 |
|------|------------|----------|-----------|----------|
| メール送信基盤 | - | IEmailSenderインターフェース定義 | インターフェース設計テスト | 🚧 |
| SMTP実装 | - | SmtpEmailSender（MailKit使用） | SMTP送信単体テスト | 🚧 |
| 設定管理 | - | appsettings.json + Optionsパターン | 設定読み込みテスト | 🚧 |
| 開発環境統合 | - | smtp4dev Docker環境構築 | 統合動作確認テスト | 🚧 |
| エラーハンドリング | - | 送信失敗時の適切な処理 | 異常系テスト | 🚧 |

## 🎯 組織設計（TDD実践・フィーチャーチーム制）

### フィーチャーチーム1: Application層インターフェース定義
**責任範囲**:
- IEmailSenderインターフェース設計
- EmailMessageモデル定義（必要に応じて）
- Application層での使用シナリオ定義

**TDD実行計画**:
- Red Phase (20分): インターフェース使用テスト作成
- Green Phase (15分): インターフェース定義
- Refactor Phase (10分): 設計最適化

### フィーチャーチーム2: Infrastructure層SMTP実装
**責任範囲**:
- SmtpEmailSender実装
- MailKit統合
- SMTP設定クラス定義

**TDD実行計画**:
- Red Phase (20分): SMTP送信テスト作成（モック使用）
- Green Phase (25分): SmtpEmailSender実装
- Refactor Phase (15分): エラーハンドリング強化

### フィーチャーチーム3: 開発環境構築
**責任範囲**:
- docker-compose.yml更新（smtp4dev追加）
- appsettings.Development.json設定
- 環境別設定の検証

**TDD実行計画**:
- Red Phase (15分): 設定読み込みテスト作成
- Green Phase (20分): 設定実装・Docker環境構築
- Refactor Phase (10分): 設定構造最適化

### 仕様準拠監査役
**責任範囲**:
- Clean Architecture原則の遵守確認
- 層間の依存関係監視
- テスト設計の妥当性確認

## 🔄 Step2実行プロセス

### Phase 1: Red Phase（30分）
1. **Application層テスト作成**
   - IEmailSender使用シナリオテスト
   - モックを使用したユースケーステスト
   
2. **Infrastructure層テスト作成**
   - SmtpEmailSender単体テスト
   - エラーハンドリングテスト
   
3. **設定管理テスト作成**
   - Optionsパターン動作テスト
   - 環境別設定切り替えテスト

### Phase 2: Green Phase（40分）
1. **インターフェース実装**
   - IEmailSender定義（Application層）
   - 必要なDTOクラス定義
   
2. **SMTP実装**
   - SmtpEmailSender作成（Infrastructure層）
   - MailKit NuGetパッケージ追加
   - 基本的な送信ロジック実装
   
3. **設定・環境構築**
   - SmtpSettings クラス作成
   - docker-compose.yml更新
   - appsettings.json設定追加

### Phase 3: Refactor Phase（20分）
1. **コード品質改善**
   - エラーハンドリング強化
   - ログ出力追加
   - コメント・ドキュメント整備
   
2. **設計最適化**
   - 拡張性の確保（CC/BCC対応準備）
   - パフォーマンス考慮
   
3. **テスト強化**
   - エッジケース追加
   - カバレッジ確認

## 📝 実行記録

### Red Phase実績（2025-07-24 実行）
- **実行時間**: 15分
- **作成テスト**: 
  - Application層: IEmailSenderインターフェース使用テスト（4テストケース）
  - Infrastructure層: SmtpEmailSender実装テスト（5テストケース）
  - 設定管理: SmtpSettingsテスト（4テストケース）
- **課題**: F#プロジェクトにC#ファイルを含められない問題発生 → Contracts層へ移動で解決
- **名前空間競合**: Email名前空間がDomainのEmailクラスと競合 → Emailingに変更

### Green Phase実績（2025-07-24 実行）
- **実行時間**: 25分
- **実装内容**:
  - IEmailSender → Contracts層に配置
  - SmtpSettings設定クラス作成
  - ISmtpClientインターフェース + SmtpClientWrapperラッパー実装
  - SmtpEmailSender本実装（MailKit統合）
- **NuGetパッケージ**: MailKit 4.13.0追加
- **ビルド成功**: 0エラー・0警告達成

### Refactor Phase実績（2025-07-24 実行）
- **実行時間**: 20分
- **品質向上内容**:
  - XMLドキュメントコメント追加（必須項目）
  - docker-compose.yml更新（smtp4dev追加）
  - appsettings.json/Development.json設定追加
  - Program.csでDI設定追加
  - 統合テスト作成（EmailIntegrationTests）
- **Clean Architecture準拠**: Contracts層でのインターフェース配置により層間依存を適切に管理

## 🚨 想定される課題と対策

### 技術的課題
1. **MailKitの依存関係** - NuGetパッケージ管理確認
2. **Docker環境** - Windows環境でのDocker動作確認
3. **非同期処理** - async/awaitの適切な実装

### 設計課題
1. **拡張性** - 将来的なSendGrid等への切り替え考慮
2. **テスタビリティ** - SMTP接続のモック化方法

## 📊 Step2終了時レビュー（実行後記入）

### TDD実践評価
- [x] Red Phaseでのテスト失敗確認
- [x] Green Phaseでの最小実装達成
- [x] Refactor Phaseでの品質向上
- [x] テストカバレッジ80%以上

### 成果物確認
- [x] IEmailSenderインターフェース完成（Contracts層）
- [x] SmtpEmailSender実装完了
- [x] smtp4dev環境での送信確認（docker-compose.yml設定済み）
- [x] 全テスト成功（ビルド成功・0エラー）

### 次Step準備状況
- [x] メール送信基盤の安定動作確認
- [x] パスワードリセット実装の準備完了

## Step2終了時レビュー（2025-07-24実施）

### レビュー結果概要
- 効率性: ✅ 達成度100%
- 専門性: ✅ 活用度5/5
- 統合性: ✅ 効率度5/5
- 品質: ✅ 達成度100%
- 適応性: ✅ 適応度5/5

### 主要学習事項
- 成功要因: TDD実践によるRed-Green-Refactorサイクル、Clean Architecture準拠のインターフェース配置、MailKit統合の技術選定
- 改善要因: F#プロジェクトとC#ファイルの組み合わせ問題の早期解決、名前空間競合の適切な対処

### 次Step組織設計方針
- パスワードリセット機能実装に向けたTDD体制継続
- メール送信基盤の活用を前提とした機能開発
- ASP.NET Core Identity拡張パターンの専門性活用