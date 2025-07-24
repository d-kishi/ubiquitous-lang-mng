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

### Red Phase実績
（実行時に記入）

### Green Phase実績
（実行時に記入）

### Refactor Phase実績
（実行時に記入）

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
- [ ] Red Phaseでのテスト失敗確認
- [ ] Green Phaseでの最小実装達成
- [ ] Refactor Phaseでの品質向上
- [ ] テストカバレッジ80%以上

### 成果物確認
- [ ] IEmailSenderインターフェース完成
- [ ] SmtpEmailSender実装完了
- [ ] smtp4dev環境での送信確認
- [ ] 全テスト成功

### 次Step準備状況
- [ ] メール送信基盤の安定動作確認
- [ ] パスワードリセット実装の準備完了