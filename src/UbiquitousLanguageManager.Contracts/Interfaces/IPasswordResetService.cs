using System.Threading.Tasks;
using UbiquitousLanguageManager.Contracts.DTOs;
using UbiquitousLanguageManager.Contracts.DTOs.Common;

namespace UbiquitousLanguageManager.Contracts.Interfaces
{
    /// <summary>
    /// パスワードリセットサービスのインターフェース
    /// 仕様書2.1.3準拠: パスワードリセット機能
    /// </summary>
    public interface IPasswordResetService
    {
        /// <summary>
        /// パスワードリセットを申請します
        /// </summary>
        /// <param name="email">メールアドレス</param>
        /// <returns>申請結果（成功/失敗）</returns>
        /// <remarks>
        /// 仕様書2.1.3: リセット申請
        /// - 登録済みメールアドレスの場合、リセットメールを送信
        /// - 未登録メールアドレスの場合、エラーメッセージを返す
        /// </remarks>
        Task<ResultDto> RequestPasswordResetAsync(string email);

        /// <summary>
        /// パスワードリセットトークンを検証します
        /// </summary>
        /// <param name="email">メールアドレス</param>
        /// <param name="token">リセットトークン</param>
        /// <returns>検証結果（有効/無効）</returns>
        /// <remarks>
        /// 仕様書2.1.3: リセットリンク有効期限（24時間）
        /// </remarks>
        Task<ResultDto<bool>> ValidateResetTokenAsync(string email, string token);

        /// <summary>
        /// パスワードをリセットします
        /// </summary>
        /// <param name="email">メールアドレス</param>
        /// <param name="token">リセットトークン</param>
        /// <param name="newPassword">新しいパスワード</param>
        /// <returns>リセット結果（成功/失敗）</returns>
        /// <remarks>
        /// 仕様書2.1.3: リセット実行
        /// - トークンが有効な場合、パスワードを更新
        /// - リセット後は自動でログイン画面に遷移
        /// </remarks>
        Task<ResultDto> ResetPasswordAsync(string email, string token, string newPassword);
    }
}