using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using UbiquitousLanguageManager.Infrastructure.Data.Entities;
using Microsoft.Extensions.Logging;
using UbiquitousLanguageManager.Contracts.DTOs;
using UbiquitousLanguageManager.Contracts.DTOs.Common;
using UbiquitousLanguageManager.Contracts.Interfaces;

namespace UbiquitousLanguageManager.Infrastructure.Services
{
    /// <summary>
    /// パスワードリセットサービスの実装
    /// 仕様書2.1.3準拠: パスワードリセット機能
    /// </summary>
    public class PasswordResetService : IPasswordResetService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEmailSender _emailSender;
        private readonly ILogger<PasswordResetService> _logger;

        /// <summary>
        /// PasswordResetServiceのコンストラクタ
        /// </summary>
        /// <param name="userManager">ASP.NET Core Identity UserManager</param>
        /// <param name="emailSender">メール送信サービス</param>
        /// <param name="logger">ロガー</param>
        public PasswordResetService(
            UserManager<ApplicationUser> userManager,
            IEmailSender emailSender,
            ILogger<PasswordResetService> logger)
        {
            _userManager = userManager;
            _emailSender = emailSender;
            _logger = logger;
        }

        /// <summary>
        /// パスワードリセットを申請します
        /// </summary>
        /// <param name="email">メールアドレス</param>
        /// <returns>申請結果</returns>
        /// <remarks>
        /// 仕様書2.1.3: リセット申請
        /// - 登録済みメールアドレスの場合、リセットメールを送信
        /// - 未登録メールアドレスの場合、エラーメッセージを返す
        /// </remarks>
        public async Task<ResultDto> RequestPasswordResetAsync(string email)
        {
            var startTime = DateTime.UtcNow;
            try
            {
                // 📧 入力検証: メールアドレスの妥当性確認
                if (string.IsNullOrWhiteSpace(email))
                {
                    _logger.LogWarning("Password reset requested with empty email address");
                    return ResultDto.Failure("メールアドレスを入力してください");
                }

                _logger.LogInformation("Starting password reset request for email: {Email}", email);

                // 🔍 ユーザー検索: メールアドレスの存在確認
                var user = await _userManager.FindByEmailAsync(email);
                if (user == null)
                {
                    // 🚫 仕様書2.1.3: 未登録メールアドレス時はエラー表示
                    _logger.LogWarning("Password reset requested for non-existent email: {Email}", email);
                    return ResultDto.Failure("メールアドレスが見つかりません");
                }

                // 🎯 トークン生成: DataProtectorTokenProviderによるセキュアトークン生成
                var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);
                _logger.LogDebug("Password reset token generated for user: {UserId}", user.Id);

                // 📤 メール送信: リセットリンク付きメール送信
                _logger.LogDebug("Sending password reset email to: {Email}", email);
                var emailSent = await _emailSender.SendPasswordResetEmailAsync(email, resetToken);
                if (!emailSent)
                {
                    var duration = DateTime.UtcNow - startTime;
                    _logger.LogError("Failed to send password reset email to: {Email} after {Duration}ms",
                        email, duration.TotalMilliseconds);
                    return ResultDto.Failure("メール送信に失敗しました。しばらく待ってから再試行してください");
                }

                var successDuration = DateTime.UtcNow - startTime;
                _logger.LogInformation("Password reset email sent successfully to: {Email} in {Duration}ms",
                    email, successDuration.TotalMilliseconds);
                return ResultDto.Success();
            }
            catch (Exception ex)
            {
                var duration = DateTime.UtcNow - startTime;
                _logger.LogError(ex, "Unexpected error during password reset request for: {Email} after {Duration}ms",
                    email, duration.TotalMilliseconds);
                return ResultDto.Failure("パスワードリセット申請中にエラーが発生しました");
            }
        }

        /// <summary>
        /// パスワードリセットトークンを検証します
        /// </summary>
        /// <param name="email">メールアドレス</param>
        /// <param name="token">リセットトークン</param>
        /// <returns>検証結果（有効/無効）</returns>
        /// <remarks>
        /// 仕様書2.1.3: リセットリンク有効期限（24時間）
        /// </remarks>
        public async Task<ResultDto<bool>> ValidateResetTokenAsync(string email, string token)
        {
            try
            {
                // 📧 入力検証
                if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(token))
                {
                    _logger.LogWarning("Token validation requested with invalid parameters");
                    return ResultDto<bool>.Failure("無効なパラメータです");
                }

                // 🔍 ユーザー検索
                var user = await _userManager.FindByEmailAsync(email);
                if (user == null)
                {
                    _logger.LogWarning("Token validation requested for non-existent email: {Email}", email);
                    return ResultDto<bool>.Failure("ユーザーが見つかりません");
                }

                // 🎯 トークン検証: 有効期限・セキュリティスタンプ確認
                var isValidToken = await _userManager.VerifyUserTokenAsync(
                    user,
                    _userManager.Options.Tokens.PasswordResetTokenProvider,
                    "ResetPassword",
                    token);

                _logger.LogDebug("Token validation result for user {UserId}: {IsValid}", user.Id, isValidToken);
                return ResultDto<bool>.Success(isValidToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during token validation for: {Email}", email);
                return ResultDto<bool>.Failure("トークン検証中にエラーが発生しました");
            }
        }

        /// <summary>
        /// パスワードをリセットします
        /// </summary>
        /// <param name="email">メールアドレス</param>
        /// <param name="token">リセットトークン</param>
        /// <param name="newPassword">新しいパスワード</param>
        /// <returns>リセット結果</returns>
        /// <remarks>
        /// 仕様書2.1.3: リセット実行
        /// - トークンが有効な場合、パスワードを更新
        /// - リセット後は自動でログイン画面に遷移
        /// </remarks>
        public async Task<ResultDto> ResetPasswordAsync(string email, string token, string newPassword)
        {
            try
            {
                // 📧 入力検証
                if (string.IsNullOrWhiteSpace(email) ||
                    string.IsNullOrWhiteSpace(token) ||
                    string.IsNullOrWhiteSpace(newPassword))
                {
                    _logger.LogWarning("Password reset requested with invalid parameters");
                    return ResultDto.Failure("必要な情報が不足しています");
                }

                _logger.LogInformation("Password reset execution requested for email: {Email}", email);

                // 🔍 ユーザー検索
                var user = await _userManager.FindByEmailAsync(email);
                if (user == null)
                {
                    _logger.LogWarning("Password reset requested for non-existent email: {Email}", email);
                    return ResultDto.Failure("ユーザーが見つかりません");
                }

                // 🔄 パスワードリセット実行: ASP.NET Core Identity標準機能
                var resetResult = await _userManager.ResetPasswordAsync(user, token, newPassword);
                if (!resetResult.Succeeded)
                {
                    var errors = string.Join(", ", resetResult.Errors.Select(e => e.Description));
                    _logger.LogWarning("Password reset failed for user {UserId}: {Errors}", user.Id, errors);

                    // 🎯 エラーメッセージの日本語化
                    if (resetResult.Errors.Any(e => e.Code.Contains("InvalidToken")))
                    {
                        return ResultDto.Failure("リセットトークンが無効です。期限切れか、既に使用済みの可能性があります");
                    }
                    if (resetResult.Errors.Any(e => e.Code.Contains("Password")))
                    {
                        return ResultDto.Failure("パスワードが要件を満たしていません。8文字以上で英数字を含めてください");
                    }

                    return ResultDto.Failure($"パスワードリセットに失敗しました: {errors}");
                }

                _logger.LogInformation("Password reset completed successfully for user: {UserId}", user.Id);
                return ResultDto.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during password reset for: {Email}", email);
                return ResultDto.Failure("パスワードリセット中にエラーが発生しました");
            }
        }
    }
}