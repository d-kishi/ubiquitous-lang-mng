using System;
using System.Diagnostics;
using System.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.FSharp.Core;
using UbiquitousLanguageManager.Contracts.DTOs;
using UbiquitousLanguageManager.Contracts.DTOs.Authentication;
using UbiquitousLanguageManager.Domain.Common;
using UbiquitousLanguageManager.Domain.Authentication;
// Microsoft.FSharp.Core.FSharpResult型は使用箇所で直接指定

namespace UbiquitousLanguageManager.Contracts.Converters;

/// <summary>
/// Phase A9: 認証専用TypeConverter
/// F# Application層の認証結果とC# Infrastructure/Web層の完全な型安全境界を確立
/// Railway-oriented Programming・Result型・AuthenticationError判別共用体対応
/// 【Blazor Server初学者向け解説】
/// このコンバーターは、F#の型安全な認証処理の結果を、C#の各層で安全に扱えるように変換します。
/// F#のResult型・判別共用体の恩恵を、C#側でも最大限活用できるよう設計されています。
/// </summary>
public static class AuthenticationConverter
{
    private static ILogger? _logger;

    /// <summary>
    /// ロガーを設定します（依存性注入で設定）
    /// Contracts層でのログ出力を有効化
    /// </summary>
    /// <param name="logger">ILoggerインスタンス</param>
    public static void SetLogger(ILogger logger)
    {
        _logger = logger;
    }
    // =================================================================
    // 🔄 F# → C# 変換メソッド（認証結果・エラー変換）
    // =================================================================

    /// <summary>
    /// F# Result&lt;User, AuthenticationError&gt; を C# AuthenticationResultDto に変換
    /// Railway-oriented Programmingの結果を型安全にマッピング
    /// 【F#初学者向け解説】
    /// F#のResult型は、成功(Ok)と失敗(Error)を型レベルで区別する強力な仕組みです。
    /// この変換により、その型安全性をC#側でも活用できます。
    /// </summary>
    /// <param name="result">F#の認証結果（Result型）</param>
    /// <returns>C#のAuthenticationResultDTO</returns>
    /// <exception cref="ArgumentNullException">resultがnullの場合</exception>
    public static AuthenticationResultDto ToDto(this Microsoft.FSharp.Core.FSharpResult<User, AuthenticationError> result)
    {
        var stopwatch = Stopwatch.StartNew();

        try
        {
            _logger?.LogDebug("F#認証結果→C# DTO変換開始");

            // F#のResult型はnull非許可型なので、ArgumentNullExceptionは不要

            // F#のResult型のパターンマッチング（C#版）
            if (result.IsOk)
            {
                // 成功ケース: F#のUserエンティティをDTOに変換
                var user = result.ResultValue;
                var userDto = TypeConverters.ToDto(user);

                _logger?.LogInformation("認証結果変換成功 UserId: {UserId}, Email: {Email}, ConversionTime: {ConversionTime}ms",
                    user.Id.Value, user.Email.Value, stopwatch.ElapsedMilliseconds);

                return AuthenticationResultDto.Success(
                    user: userDto,
                    token: null,  // トークン生成は別途実装
                    tokenExpires: null
                );
            }
            else
            {
                // 失敗ケース: F#のAuthenticationErrorをDTOに変換
                var error = result.ErrorValue;
                var errorDto = ToDto(error);

                _logger?.LogWarning("認証結果変換(失敗) ErrorType: {ErrorType}, ConversionTime: {ConversionTime}ms",
                    GetErrorTypeName(error), stopwatch.ElapsedMilliseconds);

                return AuthenticationResultDto.Failure(errorDto);
            }
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "F#認証結果→C# DTO変換でエラーが発生 ConversionTime: {ConversionTime}ms",
                stopwatch.ElapsedMilliseconds);
            throw;
        }
        finally
        {
            stopwatch.Stop();
        }
    }

    /// <summary>
    /// F# AuthenticationError判別共用体 を C# AuthenticationErrorDto に変換
    /// 判別共用体の各ケースを安全にマッピング
    /// 【F#初学者向け解説】
    /// F#の判別共用体は、エラーの種類を型レベルで区別できます。
    /// C#では、プロパティチェックによるパターンマッチングで同様の型安全性を実現します。
    /// </summary>
    /// <param name="error">F#のAuthenticationError判別共用体</param>
    /// <returns>C#のAuthenticationErrorDto</returns>
    /// <exception cref="ArgumentNullException">errorがnullの場合</exception>
    /// <exception cref="ArgumentOutOfRangeException">未知のエラータイプの場合</exception>
    public static AuthenticationErrorDto ToDto(AuthenticationError error)
    {
        var stopwatch = Stopwatch.StartNew();

        try
        {
            if (error == null)
            {
                _logger?.LogError("AuthenticationErrorが null で渡されました");
                throw new ArgumentNullException(nameof(error), "認証エラーがnullです");
            }

            var errorType = GetErrorTypeName(error);
            _logger?.LogDebug("F#認証エラー→C# DTO変換開始 ErrorType: {ErrorType}", errorType);

            // F#判別共用体の各ケースをC#でパターンマッチング
            // F#コンパイラが生成するIsBxxxプロパティを使用

            AuthenticationErrorDto resultDto;

            if (error.IsInvalidCredentials)
            {
                resultDto = AuthenticationErrorDto.InvalidCredentials();
            }
            else if (error.IsUserNotFound)
            {
                // UserNotFound of Email パターンの安全な処理
                try
                {
                    // F#の判別共用体から動的にEmailを取得
                    dynamic dynamicError = error;
                    var email = dynamicError.Item.Value as string;
                    resultDto = AuthenticationErrorDto.UserNotFound(email ?? "unknown@example.com");
                }
                catch (Exception ex)
                {
                    _logger?.LogError(ex, "UserNotFoundケースの処理に失敗 ErrorDetail: {ErrorDetail}", ex.Message);
                    // フォールバック処理：エラー詳細をログに残しつつ安全に処理
                    resultDto = AuthenticationErrorDto.SystemError(
                        new InvalidOperationException($"UserNotFoundケースの処理に失敗: {ex.Message}", ex));
                }
            }
            else if (error.IsValidationError)
            {
                // ValidationError of string パターンの安全な処理
                try
                {
                    dynamic dynamicError = error;
                    var message = dynamicError.Item as string;
                    resultDto = AuthenticationErrorDto.ValidationError(message ?? "不明なバリデーションエラー");
                }
                catch (Exception ex)
                {
                    resultDto = AuthenticationErrorDto.SystemError(
                        new InvalidOperationException($"ValidationErrorケースの処理に失敗: {ex.Message}", ex));
                }
            }
            else if (error.IsAccountLocked)
            {
                // AccountLocked of Email * DateTime パターンの安全な処理
                // Phase B-F1 修正: dynamic型の完全活用
                try
                {
                    dynamic dynamicError = error;
                    dynamic tuple = dynamicError.Item;  // F#のタプル（dynamic型で保持）
                    // Phase B-F1 修正: dynamic型でEmail型のValueプロパティにアクセス
                    dynamic emailObj = tuple.Item1;  // Email型オブジェクト（dynamic）
                    string email = (string)emailObj.Value;  // Email.Valueを明示的にstringにキャスト
                    DateTime lockoutEnd = (DateTime)tuple.Item2;  // DateTime型を明示的にキャスト

                    resultDto = AuthenticationErrorDto.AccountLocked(
                        email ?? "unknown@example.com",
                        lockoutEnd);
                }
                catch (Exception ex)
                {
                    _logger?.LogError(ex, "AccountLockedケースの処理に失敗 ErrorDetail: {ErrorDetail}, ErrorType: {ErrorType}",
                        ex.Message, ex.GetType().Name);
                    resultDto = AuthenticationErrorDto.SystemError(
                        new InvalidOperationException($"AccountLockedケースの処理に失敗: {ex.Message}", ex));
                }
            }
            else if (error.IsSystemError)
            {
                // SystemError of exn パターンの安全な処理
                try
                {
                    dynamic dynamicError = error;
                    var exception = dynamicError.Item as Exception;
                    resultDto = AuthenticationErrorDto.SystemError(exception ?? new Exception("不明なシステムエラー"));
                }
                catch (Exception ex)
                {
                    resultDto = AuthenticationErrorDto.SystemError(ex);
                }
            }
            else if (error.IsPasswordExpired)
            {
                // PasswordExpired of Email パターンの安全な処理
                try
                {
                    dynamic dynamicError = error;
                    var email = dynamicError.Item.Value as string;
                    resultDto = AuthenticationErrorDto.PasswordExpired(email ?? "unknown@example.com");
                }
                catch (Exception ex)
                {
                    resultDto = AuthenticationErrorDto.SystemError(
                        new InvalidOperationException($"PasswordExpiredケースの処理に失敗: {ex.Message}", ex));
                }
            }
            else if (error.IsTwoFactorRequired)
            {
                // TwoFactorRequired of Email パターンの安全な処理
                try
                {
                    dynamic dynamicError = error;
                    var email = dynamicError.Item.Value as string;
                    resultDto = AuthenticationErrorDto.TwoFactorRequired(email ?? "unknown@example.com");
                }
                catch (Exception ex)
                {
                    resultDto = AuthenticationErrorDto.SystemError(
                        new InvalidOperationException($"TwoFactorRequiredケースの処理に失敗: {ex.Message}", ex));
                }
            }
            // 🔐 Phase A9: パスワードリセット関連エラーの完全対応
            else if (error.IsPasswordResetTokenExpired)
            {
                try
                {
                    dynamic dynamicError = error;
                    var email = dynamicError.Item.Value as string;
                    resultDto = AuthenticationErrorDto.PasswordResetTokenExpired(email ?? "unknown@example.com");
                }
                catch (Exception ex)
                {
                    resultDto = AuthenticationErrorDto.SystemError(
                        new InvalidOperationException($"PasswordResetTokenExpiredケースの処理に失敗: {ex.Message}", ex));
                }
            }
            else if (error.IsPasswordResetTokenInvalid)
            {
                try
                {
                    dynamic dynamicError = error;
                    var email = dynamicError.Item.Value as string;
                    resultDto = AuthenticationErrorDto.PasswordResetTokenInvalid(email ?? "unknown@example.com");
                }
                catch (Exception ex)
                {
                    resultDto = AuthenticationErrorDto.SystemError(
                        new InvalidOperationException($"PasswordResetTokenInvalidケースの処理に失敗: {ex.Message}", ex));
                }
            }
            else if (error.IsPasswordResetNotRequested)
            {
                try
                {
                    dynamic dynamicError = error;
                    var email = dynamicError.Item.Value as string;
                    resultDto = AuthenticationErrorDto.PasswordResetNotRequested(email ?? "unknown@example.com");
                }
                catch (Exception ex)
                {
                    resultDto = AuthenticationErrorDto.SystemError(
                        new InvalidOperationException($"PasswordResetNotRequestedケースの処理に失敗: {ex.Message}", ex));
                }
            }
            else if (error.IsPasswordResetAlreadyUsed)
            {
                try
                {
                    dynamic dynamicError = error;
                    var email = dynamicError.Item.Value as string;
                    resultDto = AuthenticationErrorDto.PasswordResetAlreadyUsed(email ?? "unknown@example.com");
                }
                catch (Exception ex)
                {
                    resultDto = AuthenticationErrorDto.SystemError(
                        new InvalidOperationException($"PasswordResetAlreadyUsedケースの処理に失敗: {ex.Message}", ex));
                }
            }
            // 🔒 Phase A9: トークン関連エラーの完全対応
            else if (error.IsTokenGenerationFailed)
            {
                try
                {
                    dynamic dynamicError = error;
                    var message = dynamicError.Item as string;
                    resultDto = AuthenticationErrorDto.TokenGenerationFailed(message ?? "トークン生成に失敗しました");
                }
                catch (Exception ex)
                {
                    resultDto = AuthenticationErrorDto.SystemError(
                        new InvalidOperationException($"TokenGenerationFailedケースの処理に失敗: {ex.Message}", ex));
                }
            }
            else if (error.IsTokenValidationFailed)
            {
                try
                {
                    dynamic dynamicError = error;
                    var message = dynamicError.Item as string;
                    resultDto = AuthenticationErrorDto.TokenValidationFailed(message ?? "トークン検証に失敗しました");
                }
                catch (Exception ex)
                {
                    resultDto = AuthenticationErrorDto.SystemError(
                        new InvalidOperationException($"TokenValidationFailedケースの処理に失敗: {ex.Message}", ex));
                }
            }
            else if (error.IsTokenExpired)
            {
                try
                {
                    dynamic dynamicError = error;
                    var message = dynamicError.Item as string;
                    resultDto = AuthenticationErrorDto.TokenExpired(message ?? "トークンの有効期限が切れています");
                }
                catch (Exception ex)
                {
                    resultDto = AuthenticationErrorDto.SystemError(
                        new InvalidOperationException($"TokenExpiredケースの処理に失敗: {ex.Message}", ex));
                }
            }
            else if (error.IsTokenRevoked)
            {
                try
                {
                    dynamic dynamicError = error;
                    var message = dynamicError.Item as string;
                    resultDto = AuthenticationErrorDto.TokenRevoked(message ?? "トークンは取り消されています");
                }
                catch (Exception ex)
                {
                    resultDto = AuthenticationErrorDto.SystemError(
                        new InvalidOperationException($"TokenRevokedケースの処理に失敗: {ex.Message}", ex));
                }
            }
            // 👮 Phase A9: 管理者操作関連エラーの完全対応
            else if (error.IsInsufficientPermissions)
            {
                try
                {
                    dynamic dynamicError = error;
                    // Phase B-F1 修正: InsufficientPermissions of string (ロール・権限情報を文字列で保持)
                    var message = dynamicError.Item as string;
                    // メッセージをパースして role と permission に分割（"Role:Permission"形式を想定）
                    var parts = message?.Split(':') ?? Array.Empty<string>();
                    var role = parts.Length > 0 ? parts[0] : "Unknown";
                    var permission = parts.Length > 1 ? parts[1] : "Unknown";
                    resultDto = AuthenticationErrorDto.InsufficientPermissions(role, permission);
                }
                catch (Exception ex)
                {
                    resultDto = AuthenticationErrorDto.SystemError(
                        new InvalidOperationException($"InsufficientPermissionsケースの処理に失敗: {ex.Message}", ex));
                }
            }
            else if (error.IsOperationNotAllowed)
            {
                try
                {
                    dynamic dynamicError = error;
                    var message = dynamicError.Item as string;
                    resultDto = AuthenticationErrorDto.OperationNotAllowed(message ?? "この操作は許可されていません");
                }
                catch (Exception ex)
                {
                    resultDto = AuthenticationErrorDto.SystemError(
                        new InvalidOperationException($"OperationNotAllowedケースの処理に失敗: {ex.Message}", ex));
                }
            }
            else if (error.IsConcurrentOperationDetected)
            {
                try
                {
                    dynamic dynamicError = error;
                    var message = dynamicError.Item as string;
                    resultDto = AuthenticationErrorDto.ConcurrentOperationDetected(message ?? "並行操作が検出されました");
                }
                catch (Exception ex)
                {
                    resultDto = AuthenticationErrorDto.SystemError(
                        new InvalidOperationException($"ConcurrentOperationDetectedケースの処理に失敗: {ex.Message}", ex));
                }
            }
            // 🔮 Phase A9: 将来拡張用エラーの完全対応
            else if (error.IsTwoFactorAuthFailed)
            {
                try
                {
                    dynamic dynamicError = error;
                    var email = dynamicError.Item.Value as string;
                    resultDto = AuthenticationErrorDto.TwoFactorAuthFailed(email ?? "unknown@example.com");
                }
                catch (Exception ex)
                {
                    resultDto = AuthenticationErrorDto.SystemError(
                        new InvalidOperationException($"TwoFactorAuthFailedケースの処理に失敗: {ex.Message}", ex));
                }
            }
            else if (error.IsExternalAuthenticationFailed)
            {
                try
                {
                    dynamic dynamicError = error;
                    var message = dynamicError.Item as string;
                    resultDto = AuthenticationErrorDto.ExternalAuthenticationFailed(message ?? "外部認証に失敗しました");
                }
                catch (Exception ex)
                {
                    resultDto = AuthenticationErrorDto.SystemError(
                        new InvalidOperationException($"ExternalAuthenticationFailedケースの処理に失敗: {ex.Message}", ex));
                }
            }
            else if (error.IsAuditLogError)
            {
                try
                {
                    dynamic dynamicError = error;
                    var message = dynamicError.Item as string;
                    resultDto = AuthenticationErrorDto.AuditLogError(message ?? "監査ログの記録に失敗しました");
                }
                catch (Exception ex)
                {
                    resultDto = AuthenticationErrorDto.SystemError(
                        new InvalidOperationException($"AuditLogErrorケースの処理に失敗: {ex.Message}", ex));
                }
            }
            else if (error.IsAccountDeactivated)
            {
                // AccountDeactivated ケースの追加
                resultDto = AuthenticationErrorDto.AccountDeactivated();
            }
            else
            {
                // 予期しないケース（将来の拡張対応）
                resultDto = AuthenticationErrorDto.SystemError(
                    new ArgumentOutOfRangeException(nameof(error),
                        $"未知のAuthenticationErrorタイプです。F#コンパイラバージョンの違いやケース追加の可能性があります。"));
            }

            _logger?.LogInformation("認証エラー変換完了 ErrorType: {ErrorType}, ConversionTime: {ConversionTime}ms",
                errorType, stopwatch.ElapsedMilliseconds);

            return resultDto;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "F#認証エラー→C# DTO変換でエラーが発生 ConversionTime: {ConversionTime}ms",
                stopwatch.ElapsedMilliseconds);
            throw;
        }
        finally
        {
            stopwatch.Stop();
        }
    }

    // =================================================================
    // 🔄 C# → F# 変換メソッド（DTOからドメイン型への逆変換）
    // =================================================================

    /// <summary>
    /// C# AuthenticationResultDto を F# Result&lt;User, AuthenticationError&gt; に変換
    /// DTOからF#ドメイン型への逆変換（双方向変換の実現）
    /// 【重要】この変換は、C#側で作成されたDTOをF#側で処理する際に使用
    /// </summary>
    /// <param name="dto">C#のAuthenticationResultDTO</param>
    /// <returns>F#のResult型（成功時はUser、失敗時はAuthenticationError）</returns>
    /// <exception cref="ArgumentNullException">dtoがnullの場合</exception>
    /// <exception cref="InvalidOperationException">DTOの状態が不整合の場合</exception>
    public static Microsoft.FSharp.Core.FSharpResult<User, AuthenticationError> ToFSharpResult(AuthenticationResultDto dto)
    {
        if (dto == null)
            throw new ArgumentNullException(nameof(dto), "AuthenticationResultDtoがnullです");

        if (dto.IsSuccess)
        {
            // 成功ケース: UserDtoからF# Userエンティティに変換
            if (dto.User == null)
                throw new InvalidOperationException("成功結果にUserが設定されていません");

            // UserDTOからF# Userへの変換（既存のTypeConverters使用想定）
            // 実際の実装では、UserDto→User変換メソッドが必要
            throw new NotImplementedException("UserDto → F# User 変換は将来実装予定");
        }
        else
        {
            // 失敗ケース: AuthenticationErrorDtoからF# AuthenticationErrorに変換
            if (dto.Error == null)
                throw new InvalidOperationException("失敗結果にErrorが設定されていません");

            var authError = ToFSharpAuthenticationError(dto.Error);
            return Microsoft.FSharp.Core.FSharpResult<User, AuthenticationError>.NewError(authError);
        }
    }

    /// <summary>
    /// C# AuthenticationErrorDto を F# AuthenticationError判別共用体 に変換
    /// DTOから判別共用体への型安全な逆変換
    /// </summary>
    /// <param name="errorDto">C#のAuthenticationErrorDto</param>
    /// <returns>F#のAuthenticationError判別共用体</returns>
    /// <exception cref="ArgumentNullException">errorDtoがnullの場合</exception>
    /// <exception cref="ArgumentOutOfRangeException">未知のエラータイプの場合</exception>
    public static AuthenticationError ToFSharpAuthenticationError(AuthenticationErrorDto errorDto)
    {
        if (errorDto == null)
            throw new ArgumentNullException(nameof(errorDto), "AuthenticationErrorDtoがnullです");

        // エラータイプによるパターンマッチング（C#→F#変換）
        return errorDto.Type switch
        {
            "InvalidCredentials" => AuthenticationError.InvalidCredentials,
            
            "UserNotFound" => errorDto.Email != null
                ? AuthenticationError.NewUserNotFound(Email.create(errorDto.Email).ResultValue)
                : throw new InvalidOperationException("UserNotFoundエラーにEmailが設定されていません"),
            
            "ValidationError" => AuthenticationError.NewValidationError(errorDto.Message),
            
            "AccountLocked" => (errorDto.Email != null && errorDto.LockoutEnd.HasValue)
                ? AuthenticationError.NewAccountLocked(
                    Email.create(errorDto.Email).ResultValue, 
                    errorDto.LockoutEnd.Value)
                : throw new InvalidOperationException("AccountLockedエラーにEmail/LockoutEndが設定されていません"),
            
            "SystemError" => AuthenticationError.NewSystemError(
                new Exception(errorDto.Message)),
            
            "PasswordExpired" => errorDto.Email != null
                ? AuthenticationError.NewPasswordExpired(Email.create(errorDto.Email).ResultValue)
                : throw new InvalidOperationException("PasswordExpiredエラーにEmailが設定されていません"),
            
            "TwoFactorRequired" => errorDto.Email != null
                ? AuthenticationError.NewTwoFactorRequired(Email.create(errorDto.Email).ResultValue)
                : throw new InvalidOperationException("TwoFactorRequiredエラーにEmailが設定されていません"),
            
            _ => throw new ArgumentOutOfRangeException(nameof(errorDto), 
                $"未知のAuthenticationErrorタイプです: {errorDto.Type}")
        };
    }

    // =================================================================
    // 🔄 認証リクエスト・レスポンス変換（追加機能）
    // =================================================================

    /// <summary>
    /// C# LoginRequestDto を F# 認証パラメータに変換
    /// Web層からApplication層への安全なデータ変換
    /// </summary>
    /// <param name="loginDto">C#のログインリクエストDTO</param>
    /// <returns>F#のResult型（成功時はEmail*string、失敗時はvalidationエラー）</returns>
    public static Microsoft.FSharp.Core.FSharpResult<Tuple<Email, string>, string> ToFSharpLoginParams(LoginRequestDto loginDto)
    {
        var stopwatch = Stopwatch.StartNew();

        try
        {
            _logger?.LogDebug("C#ログインDTO→F#パラメータ変換開始 Email: {Email}",
                loginDto?.Email?.Length > 0 ? $"{loginDto.Email[0]}***@{loginDto.Email.Split('@').LastOrDefault()}" : "null");

            if (loginDto == null)
            {
                _logger?.LogWarning("ログインDTO変換失敗: DTOがnull ConversionTime: {ConversionTime}ms", stopwatch.ElapsedMilliseconds);
                return Microsoft.FSharp.Core.FSharpResult<Tuple<Email, string>, string>.NewError("ログインリクエストがnullです");
            }

            if (string.IsNullOrWhiteSpace(loginDto.Email))
            {
                _logger?.LogWarning("ログインDTO変換失敗: メールアドレス未入力 ConversionTime: {ConversionTime}ms", stopwatch.ElapsedMilliseconds);
                return Microsoft.FSharp.Core.FSharpResult<Tuple<Email, string>, string>.NewError("メールアドレスが入力されていません");
            }

            if (string.IsNullOrWhiteSpace(loginDto.Password))
            {
                _logger?.LogWarning("ログインDTO変換失敗: パスワード未入力 ConversionTime: {ConversionTime}ms", stopwatch.ElapsedMilliseconds);
                return Microsoft.FSharp.Core.FSharpResult<Tuple<Email, string>, string>.NewError("パスワードが入力されていません");
            }

            // F#のEmail値オブジェクトによる検証
            var emailResult = Email.create(loginDto.Email);
            if (emailResult.IsError)
            {
                _logger?.LogWarning("ログインDTO変換失敗: Email値オブジェクト検証エラー Error: {ValidationError}, ConversionTime: {ConversionTime}ms",
                    emailResult.ErrorValue, stopwatch.ElapsedMilliseconds);
                return Microsoft.FSharp.Core.FSharpResult<Tuple<Email, string>, string>.NewError(emailResult.ErrorValue);
            }

            // 成功: F#のタプルとして返す
            var loginParams = new Tuple<Email, string>(emailResult.ResultValue, loginDto.Password);

            _logger?.LogInformation("ログインDTO変換成功 Email: {MaskedEmail}, ConversionTime: {ConversionTime}ms",
                $"{loginDto.Email[0]}***@{loginDto.Email.Split('@').LastOrDefault()}", stopwatch.ElapsedMilliseconds);

            return Microsoft.FSharp.Core.FSharpResult<Tuple<Email, string>, string>.NewOk(loginParams);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "ログインDTO変換で予期しないエラーが発生 ConversionTime: {ConversionTime}ms", stopwatch.ElapsedMilliseconds);
            return Microsoft.FSharp.Core.FSharpResult<Tuple<Email, string>, string>.NewError($"変換処理中にエラーが発生しました: {ex.Message}");
        }
        finally
        {
            stopwatch.Stop();
        }
    }

    // =================================================================
    // 🔄 Phase A9: パスワードリセット関連変換（拡張機能）
    // =================================================================

    /// <summary>
    /// C# PasswordResetRequestDto を F# パスワードリセット要求パラメータに変換
    /// Web層からApplication層への安全なデータ変換
    /// </summary>
    /// <param name="resetDto">C#のパスワードリセット要求DTO</param>
    /// <returns>F#のResult型（成功時はEmail、失敗時はvalidationエラー）</returns>
    public static Microsoft.FSharp.Core.FSharpResult<Email, string> ToFSharpPasswordResetParams(PasswordResetRequestDto resetDto)
    {
        if (resetDto == null)
            return Microsoft.FSharp.Core.FSharpResult<Email, string>.NewError("パスワードリセット要求がnullです");

        if (string.IsNullOrWhiteSpace(resetDto.Email))
            return Microsoft.FSharp.Core.FSharpResult<Email, string>.NewError("メールアドレスが入力されていません");

        // F#のEmail値オブジェクトによる検証
        var emailResult = Email.create(resetDto.Email);
        if (emailResult.IsError)
            return Microsoft.FSharp.Core.FSharpResult<Email, string>.NewError(emailResult.ErrorValue);

        return Microsoft.FSharp.Core.FSharpResult<Email, string>.NewOk(emailResult.ResultValue);
    }

    /// <summary>
    /// C# PasswordResetTokenDto を F# パスワードリセット実行パラメータに変換
    /// パスワードリセット実行時のデータ変換
    /// </summary>
    /// <param name="tokenDto">C#のパスワードリセットトークンDTO</param>
    /// <returns>F#のResult型（成功時はEmail*string*string、失敗時はvalidationエラー）</returns>
    public static Microsoft.FSharp.Core.FSharpResult<Tuple<Email, string, string>, string> ToFSharpPasswordResetExecuteParams(PasswordResetTokenDto tokenDto)
    {
        if (tokenDto == null)
            return Microsoft.FSharp.Core.FSharpResult<Tuple<Email, string, string>, string>.NewError("パスワードリセットトークンがnullです");

        if (string.IsNullOrWhiteSpace(tokenDto.Email))
            return Microsoft.FSharp.Core.FSharpResult<Tuple<Email, string, string>, string>.NewError("メールアドレスが入力されていません");

        if (string.IsNullOrWhiteSpace(tokenDto.Token))
            return Microsoft.FSharp.Core.FSharpResult<Tuple<Email, string, string>, string>.NewError("リセットトークンが入力されていません");

        if (string.IsNullOrWhiteSpace(tokenDto.NewPassword))
            return Microsoft.FSharp.Core.FSharpResult<Tuple<Email, string, string>, string>.NewError("新しいパスワードが入力されていません");

        // F#のEmail値オブジェクトによる検証
        var emailResult = Email.create(tokenDto.Email);
        if (emailResult.IsError)
            return Microsoft.FSharp.Core.FSharpResult<Tuple<Email, string, string>, string>.NewError(emailResult.ErrorValue);

        // パスワードバリデーション（簡易チェック）
        if (tokenDto.NewPassword.Length < 8)
            return Microsoft.FSharp.Core.FSharpResult<Tuple<Email, string, string>, string>.NewError("パスワードは8文字以上である必要があります");

        // 成功: F#のタプルとして返す (Email, Token, NewPassword)
        var resetParams = new Tuple<Email, string, string>(emailResult.ResultValue, tokenDto.Token, tokenDto.NewPassword);
        return Microsoft.FSharp.Core.FSharpResult<Tuple<Email, string, string>, string>.NewOk(resetParams);
    }

    /// <summary>
    /// F# パスワードリセット結果 を C# PasswordResetResultDto に変換
    /// Application層からWeb層への結果変換
    /// </summary>
    /// <param name="result">F#のパスワードリセット結果</param>
    /// <param name="userEmail">対象ユーザーのメールアドレス</param>
    /// <returns>C#のPasswordResetResultDto</returns>
    public static PasswordResetResultDto ToPasswordResetResultDto<T>(Microsoft.FSharp.Core.FSharpResult<T, AuthenticationError> result, string userEmail)
    {
        if (result.IsOk)
        {
            return PasswordResetResultDto.Success(userEmail, "パスワードが正常にリセットされました。新しいパスワードでログインしてください。");
        }
        else
        {
            var error = result.ErrorValue;
            var errorDto = ToDto(error);
            return PasswordResetResultDto.Failure(errorDto);
        }
    }

    // =================================================================
    // 🔄 Phase A9: 拡張認証エラー変換（将来のF#拡張対応）
    // =================================================================

    /// <summary>
    /// 拡張AuthenticationErrorDto を F# AuthenticationError判別共用体 に変換
    /// 新規追加されたエラー型に対応（将来のF#拡張準備）
    /// </summary>
    /// <param name="errorDto">C#のAuthenticationErrorDto（拡張版）</param>
    /// <returns>F#のAuthenticationError判別共用体</returns>
    /// <exception cref="ArgumentNullException">errorDtoがnullの場合</exception>
    /// <exception cref="ArgumentOutOfRangeException">未知のエラータイプの場合</exception>
    public static AuthenticationError ToFSharpAuthenticationErrorExtended(AuthenticationErrorDto errorDto)
    {
        if (errorDto == null)
            throw new ArgumentNullException(nameof(errorDto), "AuthenticationErrorDtoがnullです");

        // 拡張エラータイプの変換（既存変換も含む）
        return errorDto.Type switch
        {
            // 既存エラー（基本型）
            "InvalidCredentials" => AuthenticationError.InvalidCredentials,
            "UserNotFound" => errorDto.Email != null
                ? AuthenticationError.NewUserNotFound(Email.create(errorDto.Email).ResultValue)
                : throw new InvalidOperationException("UserNotFoundエラーにEmailが設定されていません"),
            "ValidationError" => AuthenticationError.NewValidationError(errorDto.Message),
            "AccountLocked" => (errorDto.Email != null && errorDto.LockoutEnd.HasValue)
                ? AuthenticationError.NewAccountLocked(
                    Email.create(errorDto.Email).ResultValue,
                    errorDto.LockoutEnd.Value)
                : throw new InvalidOperationException("AccountLockedエラーにEmail/LockoutEndが設定されていません"),
            "SystemError" => AuthenticationError.NewSystemError(new Exception(errorDto.Message)),
            "PasswordExpired" => errorDto.Email != null
                ? AuthenticationError.NewPasswordExpired(Email.create(errorDto.Email).ResultValue)
                : throw new InvalidOperationException("PasswordExpiredエラーにEmailが設定されていません"),
            "TwoFactorRequired" => errorDto.Email != null
                ? AuthenticationError.NewTwoFactorRequired(Email.create(errorDto.Email).ResultValue)
                : throw new InvalidOperationException("TwoFactorRequiredエラーにEmailが設定されていません"),

            // 拡張エラー（将来F#側実装予定）
            // 現在はSystemErrorとして変換し、詳細情報を保持
            "PasswordResetTokenExpired" or "PasswordResetTokenInvalid" or
            "PasswordResetNotRequested" or "PasswordResetAlreadyUsed" or
            "TokenGenerationFailed" or "TokenValidationFailed" or
            "TokenExpired" or "TokenRevoked" or
            "InsufficientPermissions" or "OperationNotAllowed" or
            "ConcurrentOperationDetected" =>
                AuthenticationError.NewSystemError(new InvalidOperationException($"拡張エラータイプ: {errorDto.Type} - {errorDto.Message}")),

            _ => throw new ArgumentOutOfRangeException(nameof(errorDto),
                $"未知のAuthenticationErrorタイプです: {errorDto.Type}")
        };
    }

    // =================================================================
    // 🔧 ログ用ヘルパーメソッド（ADR_017準拠）
    // =================================================================

    /// <summary>
    /// F# AuthenticationErrorの型名を取得（ログ出力用）
    /// 構造化ログのパラメータとして使用
    /// </summary>
    /// <param name="error">F#のAuthenticationError</param>
    /// <returns>エラータイプ名</returns>
    private static string GetErrorTypeName(AuthenticationError error)
    {
        if (error.IsInvalidCredentials) return "InvalidCredentials";
        if (error.IsUserNotFound) return "UserNotFound";
        if (error.IsValidationError) return "ValidationError";
        if (error.IsAccountLocked) return "AccountLocked";
        if (error.IsSystemError) return "SystemError";
        if (error.IsPasswordExpired) return "PasswordExpired";
        if (error.IsTwoFactorRequired) return "TwoFactorRequired";
        if (error.IsPasswordResetTokenExpired) return "PasswordResetTokenExpired";
        if (error.IsPasswordResetTokenInvalid) return "PasswordResetTokenInvalid";
        if (error.IsPasswordResetNotRequested) return "PasswordResetNotRequested";
        if (error.IsPasswordResetAlreadyUsed) return "PasswordResetAlreadyUsed";
        if (error.IsTokenGenerationFailed) return "TokenGenerationFailed";
        if (error.IsTokenValidationFailed) return "TokenValidationFailed";
        if (error.IsTokenExpired) return "TokenExpired";
        if (error.IsTokenRevoked) return "TokenRevoked";
        if (error.IsInsufficientPermissions) return "InsufficientPermissions";
        if (error.IsOperationNotAllowed) return "OperationNotAllowed";
        if (error.IsConcurrentOperationDetected) return "ConcurrentOperationDetected";
        if (error.IsTwoFactorAuthFailed) return "TwoFactorAuthFailed";
        if (error.IsExternalAuthenticationFailed) return "ExternalAuthenticationFailed";
        if (error.IsAuditLogError) return "AuditLogError";
        if (error.IsAccountDeactivated) return "AccountDeactivated";
        return "Unknown";
    }
}