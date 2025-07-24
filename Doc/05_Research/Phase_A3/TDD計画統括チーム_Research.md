# Phase A3 TDD計画統括チーム 調査結果

**調査日**: 2025-07-24  
**調査者**: TDD計画統括チーム  
**調査時間**: 30分  

## 📋 TDD実践計画調査

### Red-Green-Refactorサイクル設計

#### 各フェーズの責任と成果物
1. **Red Phase（テスト失敗）**
   - 仕様ベーステスト作成
   - 失敗確認（重要：テストが確実に失敗することを確認）
   - テストの妥当性検証

2. **Green Phase（最小実装）**
   - テストを成功させる最小限の実装
   - オーバーエンジニアリング回避
   - 動作確認・テスト成功確認

3. **Refactor Phase（品質向上）**
   - コード品質改善
   - 設計最適化
   - テスト継続成功確認

## 🎯 機能別TDD戦略

### Step2: メール送信基盤
#### Red Phase戦略
```csharp
[Fact]
public async Task SendEmailAsync_ShouldSendEmail_WithValidParameters()
{
    // Arrange
    var mockSmtpClient = new Mock<ISmtpClient>();
    var emailSender = new SmtpEmailSender(mockSmtpClient.Object);
    
    // Act & Assert
    await emailSender.SendEmailAsync("test@example.com", "Subject", "Body");
    
    // この時点では実装がないため失敗することを確認
    mockSmtpClient.Verify(x => x.SendAsync(It.IsAny<MimeMessage>(), default), Times.Once);
}
```

#### Green Phase実装
```csharp
public async Task SendEmailAsync(string to, string subject, string body, bool isBodyHtml = true, CancellationToken cancellationToken = default)
{
    var message = new MimeMessage();
    // 最小限の実装でテストを成功させる
    await _smtpClient.SendAsync(message, cancellationToken);
}
```

### Step3: パスワードリセット機能
#### 仕様ベーステスト設計
```csharp
// 仕様書2.1.3準拠: パスワードリセット機能
[Fact]
public async Task GeneratePasswordResetToken_ShouldCreateValidToken()
{
    // Red: トークン生成テスト
}

[Fact]
public async Task ResetPassword_WithValidToken_ShouldUpdatePassword()
{
    // Red: リセット実行テスト
}

[Fact]
public async Task ResetPassword_WithExpiredToken_ShouldFail()
{
    // Red: 無効トークン検証テスト
}
```

### Step4: Remember Me・ログアウト
#### 否定的仕様の検証テスト
```csharp
// 仕様書2.1.1準拠: ロックアウト機構なし
[Fact]
public async Task Login_AfterMultipleFailures_ShouldNotLockAccount()
{
    // Arrange
    var loginAttempts = 10;
    
    // Act: 複数回ログイン失敗
    for (int i = 0; i < loginAttempts; i++)
    {
        await _signInManager.PasswordSignInAsync("user@test.com", "wrongpassword", false, false);
    }
    
    // Assert: 正しいパスワードでログイン可能
    var result = await _signInManager.PasswordSignInAsync("user@test.com", "correctpassword", false, false);
    Assert.True(result.Succeeded);
    Assert.False(result.IsLockedOut);
}
```

## 📊 テストシナリオ設計

### 統合テストシナリオ
1. **エンドツーエンド認証フロー**
   - ユーザー登録 → 初回パスワード変更 → ログイン → ログアウト

2. **パスワードリセットフロー**
   - リセット申請 → メール送信 → トークン検証 → パスワード更新

3. **Remember Me機能**
   - チェックボックス選択 → 永続化Cookie確認 → セッション復元

### 単体テストカバレッジ目標
- **Application層**: 95%以上
- **Infrastructure層**: 80%以上
- **Web層**: 70%以上（UI集約）

## 🔄 フィーチャーチーム別TDD計画

### Step2組織設計とTDDマッピング

#### フィーチャーチーム1: Application層インターフェース
**Red Phase（20分）**:
```csharp
// IEmailSender使用テスト
[Fact]
public async Task UserRegistration_ShouldSendWelcomeEmail()
{
    // ユースケーステスト作成
}
```

**Green Phase（15分）**:
```csharp
public interface IEmailSender
{
    Task SendEmailAsync(string to, string subject, string body, bool isBodyHtml = true, CancellationToken cancellationToken = default);
}
```

#### フィーチャーチーム2: Infrastructure層実装
**Red Phase（20分）**:
```csharp
// SMTP送信テスト（モック使用）
[Fact]
public async Task SmtpEmailSender_ShouldCallSmtpClient()
```

**Green Phase（25分）**:
```csharp
public class SmtpEmailSender : IEmailSender
{
    // 基本実装
}
```

## 📋 TDD品質保証

### チェックポイント
#### Red Phase確認事項
- [ ] テストが確実に失敗することを確認
- [ ] 失敗理由が期待通り（実装不足）
- [ ] テスト自体にバグがないことを確認

#### Green Phase確認事項
- [ ] すべてのテストが成功
- [ ] 最小限の実装であることを確認
- [ ] オーバーエンジニアリングなし

#### Refactor Phase確認事項
- [ ] コード品質向上
- [ ] テスト継続成功
- [ ] 設計原則準拠

### テスト命名規約
```csharp
// パターン: [Method]_[Scenario]_[ExpectedResult]
[Fact]
public async Task SendEmailAsync_WithValidParameters_ShouldSendSuccessfully()

// 仕様書参照含む
[Fact] // 仕様書2.1.1準拠
public async Task Login_WithRememberMe_ShouldCreatePersistentCookie()
```

## 🚀 継続的改善

### TDD実践記録
各Stepで以下を記録：
1. **Red Phase実行時間**と成果
2. **Green Phase実装効率**
3. **Refactor Phase品質向上**内容
4. **発見した問題**と改善策

### Phase全体でのTDD効果測定
- **開発速度**（従来手法との比較）
- **バグ発見率**（各フェーズでの発見）
- **テストカバレッジ**達成度
- **リファクタリング**の容易さ

## 📊 技術的課題と対策

### モック・スタブ戦略
1. **外部依存のモック化**
   - SMTP接続
   - データベースアクセス
   - 時間依存処理

2. **インメモリテスト**
   - EF Core InMemory Provider
   - テスト用設定

### 非同期処理のテスト
```csharp
[Fact]
public async Task AsyncMethod_ShouldCompleteSuccessfully()
{
    // 非同期処理の適切なテスト方法
    var result = await _service.ProcessAsync();
    Assert.NotNull(result);
}
```

---

**TDD成功のキーポイント**:
- 仕様理解を最優先
- Red-Green-Refactorサイクル厳守
- 最小実装から開始
- 継続的なリファクタリング