using System.Threading;
using System.Threading.Tasks;

namespace UbiquitousLanguageManager.Contracts.Interfaces
{
    /// <summary>
    /// メール送信サービスのインターフェース
    /// Phase A3 Step2: Clean Architecture準拠のメール送信基盤
    /// </summary>
    public interface IEmailSender
    {
        /// <summary>
        /// メールを非同期で送信します
        /// </summary>
        /// <param name="to">宛先メールアドレス</param>
        /// <param name="subject">件名</param>
        /// <param name="body">本文</param>
        /// <param name="isBodyHtml">本文がHTMLかどうか（デフォルト: true）</param>
        /// <param name="cancellationToken">キャンセレーショントークン</param>
        /// <returns>非同期タスク</returns>
        Task SendEmailAsync(
            string to,
            string subject,
            string body,
            bool isBodyHtml = true,
            CancellationToken cancellationToken = default);
    }
}