using System.ComponentModel.DataAnnotations;

namespace UbiquitousLanguageManager.Infrastructure.Services;

/// <summary>
/// SMTP メール送信設定クラス
/// 【初学者向け解説】
/// アプリケーション設定（appsettings.json）から読み込まれるメール送信に関する設定を保持します。
/// Options パターンを使用してDIコンテナから設定を注入し、MailKitを使ったメール送信で使用されます。
/// 開発環境では Smtp4dev、本番環境では実際のSMTPサーバーの設定を切り替えて使用できます。
/// </summary>
/// <remarks>
/// Phase A3: NotificationService基盤構築で導入
/// Clean Architectureの原則に従い、Infrastructure層で外部システム（SMTPサーバー）への接続設定を管理
/// </remarks>
public class SmtpSettings
{
    /// <summary>
    /// SMTP サーバーホスト名
    /// 【設定例】
    /// - 開発環境: "localhost" (Smtp4dev)
    /// - 本番環境: "smtp.gmail.com", "smtp.sendgrid.net" など
    /// </summary>
    [Required]
    public string Server { get; set; } = string.Empty;

    /// <summary>
    /// SMTP サーバーポート番号
    /// 【設定例】
    /// - Smtp4dev: 1025 (認証なし)
    /// - Gmail: 587 (TLS)
    /// - SendGrid: 587 (TLS)
    /// </summary>
    [Range(1, 65535)]
    public int Port { get; set; }

    /// <summary>
    /// 送信者名（表示名）
    /// メールクライアントで "差出人" として表示される名前
    /// </summary>
    [Required]
    public string SenderName { get; set; } = string.Empty;

    /// <summary>
    /// 送信者メールアドレス
    /// 実際にメールを送信するメールアドレス（From アドレス）
    /// </summary>
    [Required]
    [EmailAddress]
    public string SenderEmail { get; set; } = string.Empty;

    /// <summary>
    /// SMTP 認証用ユーザー名
    /// 開発環境（Smtp4dev）では空文字列、本番環境では実際のユーザー名を設定
    /// </summary>
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// SMTP 認証用パスワード
    /// 開発環境（Smtp4dev）では空文字列、本番環境では実際のパスワードを設定
    /// 【セキュリティ重要】
    /// 本番環境では Azure Key Vault や AWS Secrets Manager などのシークレット管理サービス使用推奨
    /// </summary>
    public string Password { get; set; } = string.Empty;

    /// <summary>
    /// TLS/SSL 使用設定
    /// 【設定例】
    /// - 開発環境 (Smtp4dev): false
    /// - 本番環境: true (セキュリティのため必須)
    /// </summary>
    public bool EnableSsl { get; set; }

    /// <summary>
    /// SMTP認証が必要かどうか
    /// Username/Password が設定されている場合は自動的に true として扱われる
    /// </summary>
    public bool RequireAuthentication => !string.IsNullOrEmpty(Username);

    /// <summary>
    /// 設定の妥当性を検証
    /// 【初学者向け解説】
    /// アプリケーション起動時やメール送信前に設定が正しいかチェックするためのメソッド
    /// </summary>
    /// <returns>妥当性チェック結果</returns>
    public bool IsValid()
    {
        return !string.IsNullOrWhiteSpace(Server)
            && Port > 0 && Port <= 65535
            && !string.IsNullOrWhiteSpace(SenderName)
            && !string.IsNullOrWhiteSpace(SenderEmail)
            && IsValidEmail(SenderEmail);
    }

    /// <summary>
    /// メールアドレス形式の妥当性チェック
    /// </summary>
    private static bool IsValidEmail(string email)
    {
        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// 設定内容を安全な形で文字列として出力（デバッグ用）
    /// パスワードなどの機密情報はマスクして出力
    /// </summary>
    public override string ToString()
    {
        var passwordMask = string.IsNullOrEmpty(Password) ? "<empty>" : "<masked>";
        return $"SMTP Settings: {Server}:{Port}, From: {SenderName} <{SenderEmail}>, " +
               $"Auth: {RequireAuthentication}, SSL: {EnableSsl}, Password: {passwordMask}";
    }
}