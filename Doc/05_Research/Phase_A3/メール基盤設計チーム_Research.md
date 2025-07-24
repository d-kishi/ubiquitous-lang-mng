# Phase A3 メール基盤設計チーム 調査結果

**調査日**: 2025-07-24  
**調査者**: メール基盤設計チーム  
**調査時間**: 30分  

## 📋 メール送信アーキテクチャ調査

### Clean Architecture準拠設計

#### 層別責任分担
1. **Application層**
   - `IEmailSender`インターフェース定義
   - メール送信ユースケース実装
   - ドメインロジックからの分離

2. **Infrastructure層**
   - `SmtpEmailSender`具体実装
   - SMTP接続・送信処理
   - 設定管理・エラーハンドリング

3. **Web層**
   - DI設定・サービス登録
   - 環境別設定管理

### 推奨ライブラリ選定

#### MailKit採用理由
1. **System.Net.Mail.SmtpClient非推奨**
   - .NET Framework時代の古いAPI
   - 非同期サポート不完全
   - セキュリティ更新停止

2. **MailKit優位性**
   - 完全非同期サポート
   - 現代的なSMTP実装
   - セキュリティ機能充実
   - アクティブ開発継続

## 🛠️ 実装設計

### IEmailSenderインターフェース
```csharp
public interface IEmailSender
{
    Task SendEmailAsync(
        string to,
        string subject,
        string body,
        bool isBodyHtml = true,
        CancellationToken cancellationToken = default);
}
```

### SmtpSettings設定クラス
```csharp
public class SmtpSettings
{
    public string Host { get; set; } = string.Empty;
    public int Port { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string SenderEmail { get; set; } = string.Empty;
    public string SenderName { get; set; } = string.Empty;
    public bool EnableSsl { get; set; }
}
```

### SmtpEmailSender実装
```csharp
public class SmtpEmailSender : IEmailSender
{
    private readonly SmtpSettings _settings;
    private readonly ILogger<SmtpEmailSender> _logger;

    public async Task SendEmailAsync(string to, string subject, string body, 
        bool isBodyHtml = true, CancellationToken cancellationToken = default)
    {
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(_settings.SenderName, _settings.SenderEmail));
        message.To.Add(new MailboxAddress("", to));
        message.Subject = subject;

        var bodyBuilder = new BodyBuilder 
        { 
            HtmlBody = isBodyHtml ? body : null, 
            TextBody = !isBodyHtml ? body : null 
        };
        message.Body = bodyBuilder.ToMessageBody();

        using var client = new SmtpClient();
        await client.ConnectAsync(_settings.Host, _settings.Port, _settings.EnableSsl, cancellationToken);
        await client.AuthenticateAsync(_settings.Username, _settings.Password, cancellationToken);
        await client.SendAsync(message, cancellationToken);
        await client.DisconnectAsync(true, cancellationToken);
    }
}
```

## 📧 開発環境構築

### smtp4dev Docker統合

#### docker-compose.yml追加
```yaml
services:
  smtp4dev:
    image: rnwood/smtp4dev:v3
    ports:
      - "5000:80"    # Web UI
      - "2525:25"    # SMTP Port
    restart: unless-stopped
```

#### 開発環境設定
```json
// appsettings.Development.json
{
  "SmtpSettings": {
    "Host": "localhost",
    "Port": 2525,
    "Username": "",
    "Password": "",
    "EnableSsl": false,
    "SenderEmail": "noreply@ubiquitous-lang-mng.local",
    "SenderName": "Ubiquitous Language Manager (Dev)"
  }
}
```

### 本番環境設定
```json
// appsettings.json
{
  "SmtpSettings": {
    "Host": "smtp.example.com",
    "Port": 587,
    "Username": "${SMTP_USERNAME}",
    "Password": "${SMTP_PASSWORD}",
    "EnableSsl": true,
    "SenderEmail": "noreply@your-domain.com",
    "SenderName": "Ubiquitous Language Manager"
  }
}
```

## 🔧 エラーハンドリング設計

### 例外分類と対処
1. **SmtpCommandException**
   - 認証エラー、サーバー拒否
   - 設定確認・再試行不要

2. **SocketException**
   - ネットワーク接続エラー
   - リトライ対象

3. **TimeoutException**
   - 送信タイムアウト
   - リトライ・タイムアウト延長検討

### Pollyを使用したリトライ戦略
```csharp
var retryPolicy = Policy
    .Handle<SocketException>()
    .Or<TimeoutException>()
    .WaitAndRetryAsync(
        retryCount: 3,
        sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));

await retryPolicy.ExecuteAsync(async () =>
{
    await client.SendAsync(message, cancellationToken);
});
```

## 📊 テンプレートエンジン検討

### 選択肢比較
1. **単純な文字列置換**
   - 軽量・依存関係なし
   - 複雑なレイアウト不可

2. **RazorLight**
   - Razor構文使用可能
   - 学習コスト低
   - 複雑なHTML対応

3. **Scriban**
   - 高パフォーマンス
   - セキュア（サンドボックス）
   - 構文学習必要

### 推奨実装（段階的）
1. **Phase A3**: 単純文字列置換で開始
2. **将来拡張**: RazorLight導入検討

## 🚨 セキュリティ考慮事項

### 設定情報保護
1. **環境変数使用**（本番環境）
2. **設定ファイル暗号化**
3. **ログからの認証情報除外**

### メール内容検証
1. **HTMLサニタイゼーション**
2. **XSS対策**
3. **リンク検証**

---

**必要NuGetパッケージ**:
- MailKit
- Microsoft.Extensions.Options.ConfigurationExtensions
- Polly（リトライ用）