namespace UbiquitousLanguageManager.Infrastructure.Emailing
{
    /// <summary>
    /// SMTP設定クラス
    /// Phase A3 Step2: メール送信基盤の設定管理
    /// </summary>
    public class SmtpSettings
    {
        /// <summary>
        /// SMTPサーバーのホスト名
        /// </summary>
        public string Host { get; set; } = string.Empty;

        /// <summary>
        /// SMTPサーバーのポート番号
        /// </summary>
        public int Port { get; set; }

        /// <summary>
        /// SMTP認証用のユーザー名
        /// </summary>
        public string Username { get; set; } = string.Empty;

        /// <summary>
        /// SMTP認証用のパスワード
        /// </summary>
        public string Password { get; set; } = string.Empty;

        /// <summary>
        /// SSL/TLSを使用するかどうか
        /// </summary>
        public bool EnableSsl { get; set; }

        /// <summary>
        /// 送信元メールアドレス
        /// </summary>
        public string SenderEmail { get; set; } = string.Empty;

        /// <summary>
        /// 送信元の表示名
        /// </summary>
        public string SenderName { get; set; } = string.Empty;
    }
}