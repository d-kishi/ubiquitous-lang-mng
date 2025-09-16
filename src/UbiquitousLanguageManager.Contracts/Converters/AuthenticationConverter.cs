using System;
using Microsoft.FSharp.Core;
using UbiquitousLanguageManager.Contracts.DTOs;
using UbiquitousLanguageManager.Contracts.DTOs.Authentication;
using UbiquitousLanguageManager.Domain;

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
    public static AuthenticationResultDto ToDto(this FSharpResult<User, AuthenticationError> result)
    {
        // F#のResult型はnull非許可型なので、ArgumentNullExceptionは不要

        // F#のResult型のパターンマッチング（C#版）
        if (result.IsOk)
        {
            // 成功ケース: F#のUserエンティティをDTOに変換
            var user = result.ResultValue;
            var userDto = TypeConverters.ToDto(user);
            
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
            
            return AuthenticationResultDto.Failure(errorDto);
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
        if (error == null)
            throw new ArgumentNullException(nameof(error), "認証エラーがnullです");

        // F#判別共用体の各ケースをC#でパターンマッチング
        // F#コンパイラが生成するIsBxxxプロパティを使用
        
        if (error.IsInvalidCredentials)
        {
            return AuthenticationErrorDto.InvalidCredentials();
        }
        else if (error.IsUserNotFound)
        {
            // UserNotFound of Email パターンの安全な処理
            try
            {
                // F#の判別共用体から動的にEmailを取得
                dynamic dynamicError = error;
                var email = dynamicError.Item.Value as string;
                return AuthenticationErrorDto.UserNotFound(email ?? "unknown@example.com");
            }
            catch (Exception ex)
            {
                // フォールバック処理：エラー詳細をログに残しつつ安全に処理
                return AuthenticationErrorDto.SystemError(
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
                return AuthenticationErrorDto.ValidationError(message ?? "不明なバリデーションエラー");
            }
            catch (Exception ex)
            {
                return AuthenticationErrorDto.SystemError(
                    new InvalidOperationException($"ValidationErrorケースの処理に失敗: {ex.Message}", ex));
            }
        }
        else if (error.IsAccountLocked)
        {
            // AccountLocked of Email * DateTime パターンの安全な処理
            try
            {
                dynamic dynamicError = error;
                var tuple = dynamicError.Item;  // F#のタプル
                var email = tuple.Item1.Value as string;
                var lockoutEnd = (DateTime)tuple.Item2;
                
                return AuthenticationErrorDto.AccountLocked(
                    email ?? "unknown@example.com", 
                    lockoutEnd);
            }
            catch (Exception ex)
            {
                return AuthenticationErrorDto.SystemError(
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
                return AuthenticationErrorDto.SystemError(exception ?? new Exception("不明なシステムエラー"));
            }
            catch (Exception ex)
            {
                return AuthenticationErrorDto.SystemError(ex);
            }
        }
        else if (error.IsPasswordExpired)
        {
            // PasswordExpired of Email パターンの安全な処理
            try
            {
                dynamic dynamicError = error;
                var email = dynamicError.Item.Value as string;
                return AuthenticationErrorDto.PasswordExpired(email ?? "unknown@example.com");
            }
            catch (Exception ex)
            {
                return AuthenticationErrorDto.SystemError(
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
                return AuthenticationErrorDto.TwoFactorRequired(email ?? "unknown@example.com");
            }
            catch (Exception ex)
            {
                return AuthenticationErrorDto.SystemError(
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
                return AuthenticationErrorDto.PasswordResetTokenExpired(email ?? "unknown@example.com");
            }
            catch (Exception ex)
            {
                return AuthenticationErrorDto.SystemError(
                    new InvalidOperationException($"PasswordResetTokenExpiredケースの処理に失敗: {ex.Message}", ex));
            }
        }
        else if (error.IsPasswordResetTokenInvalid)
        {
            try
            {
                dynamic dynamicError = error;
                var email = dynamicError.Item.Value as string;
                return AuthenticationErrorDto.PasswordResetTokenInvalid(email ?? "unknown@example.com");
            }
            catch (Exception ex)
            {
                return AuthenticationErrorDto.SystemError(
                    new InvalidOperationException($"PasswordResetTokenInvalidケースの処理に失敗: {ex.Message}", ex));
            }
        }
        else if (error.IsPasswordResetNotRequested)
        {
            try
            {
                dynamic dynamicError = error;
                var email = dynamicError.Item.Value as string;
                return AuthenticationErrorDto.PasswordResetNotRequested(email ?? "unknown@example.com");
            }
            catch (Exception ex)
            {
                return AuthenticationErrorDto.SystemError(
                    new InvalidOperationException($"PasswordResetNotRequestedケースの処理に失敗: {ex.Message}", ex));
            }
        }
        else if (error.IsPasswordResetAlreadyUsed)
        {
            try
            {
                dynamic dynamicError = error;
                var email = dynamicError.Item.Value as string;
                return AuthenticationErrorDto.PasswordResetAlreadyUsed(email ?? "unknown@example.com");
            }
            catch (Exception ex)
            {
                return AuthenticationErrorDto.SystemError(
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
                return AuthenticationErrorDto.TokenGenerationFailed(message ?? "トークン生成に失敗しました");
            }
            catch (Exception ex)
            {
                return AuthenticationErrorDto.SystemError(
                    new InvalidOperationException($"TokenGenerationFailedケースの処理に失敗: {ex.Message}", ex));
            }
        }
        else if (error.IsTokenValidationFailed)
        {
            try
            {
                dynamic dynamicError = error;
                var message = dynamicError.Item as string;
                return AuthenticationErrorDto.TokenValidationFailed(message ?? "トークン検証に失敗しました");
            }
            catch (Exception ex)
            {
                return AuthenticationErrorDto.SystemError(
                    new InvalidOperationException($"TokenValidationFailedケースの処理に失敗: {ex.Message}", ex));
            }
        }
        else if (error.IsTokenExpired)
        {
            try
            {
                dynamic dynamicError = error;
                var message = dynamicError.Item as string;
                return AuthenticationErrorDto.TokenExpired(message ?? "トークンの有効期限が切れています");
            }
            catch (Exception ex)
            {
                return AuthenticationErrorDto.SystemError(
                    new InvalidOperationException($"TokenExpiredケースの処理に失敗: {ex.Message}", ex));
            }
        }
        else if (error.IsTokenRevoked)
        {
            try
            {
                dynamic dynamicError = error;
                var message = dynamicError.Item as string;
                return AuthenticationErrorDto.TokenRevoked(message ?? "トークンは取り消されています");
            }
            catch (Exception ex)
            {
                return AuthenticationErrorDto.SystemError(
                    new InvalidOperationException($"TokenRevokedケースの処理に失敗: {ex.Message}", ex));
            }
        }
        // 👮 Phase A9: 管理者操作関連エラーの完全対応
        else if (error.IsInsufficientPermissions)
        {
            try
            {
                dynamic dynamicError = error;
                // F#のタプル (Role * Permission) の処理
                var tuple = dynamicError.Item;
                var role = tuple.Item1.ToString();  // Role型の文字列化
                var permission = tuple.Item2.ToString();  // Permission型の文字列化
                return AuthenticationErrorDto.InsufficientPermissions(role, permission);
            }
            catch (Exception ex)
            {
                return AuthenticationErrorDto.SystemError(
                    new InvalidOperationException($"InsufficientPermissionsケースの処理に失敗: {ex.Message}", ex));
            }
        }
        else if (error.IsOperationNotAllowed)
        {
            try
            {
                dynamic dynamicError = error;
                var message = dynamicError.Item as string;
                return AuthenticationErrorDto.OperationNotAllowed(message ?? "この操作は許可されていません");
            }
            catch (Exception ex)
            {
                return AuthenticationErrorDto.SystemError(
                    new InvalidOperationException($"OperationNotAllowedケースの処理に失敗: {ex.Message}", ex));
            }
        }
        else if (error.IsConcurrentOperationDetected)
        {
            try
            {
                dynamic dynamicError = error;
                var message = dynamicError.Item as string;
                return AuthenticationErrorDto.ConcurrentOperationDetected(message ?? "並行操作が検出されました");
            }
            catch (Exception ex)
            {
                return AuthenticationErrorDto.SystemError(
                    new InvalidOperationException($"ConcurrentOperationDetectedケースの処理に失敗: {ex.Message}", ex));
            }
        }
        // 🔮 Phase A9: 将来拡張用エラーの完全対応
        else if (error.IsTwoFactorRequired)
        {
            try
            {
                dynamic dynamicError = error;
                var email = dynamicError.Item.Value as string;
                return AuthenticationErrorDto.TwoFactorAuthRequired(email ?? "unknown@example.com");
            }
            catch (Exception ex)
            {
                return AuthenticationErrorDto.SystemError(
                    new InvalidOperationException($"TwoFactorAuthRequiredケースの処理に失敗: {ex.Message}", ex));
            }
        }
        else if (error.IsTwoFactorAuthFailed)
        {
            try
            {
                dynamic dynamicError = error;
                var email = dynamicError.Item.Value as string;
                return AuthenticationErrorDto.TwoFactorAuthFailed(email ?? "unknown@example.com");
            }
            catch (Exception ex)
            {
                return AuthenticationErrorDto.SystemError(
                    new InvalidOperationException($"TwoFactorAuthFailedケースの処理に失敗: {ex.Message}", ex));
            }
        }
        else if (error.IsExternalAuthenticationFailed)
        {
            try
            {
                dynamic dynamicError = error;
                var message = dynamicError.Item as string;
                return AuthenticationErrorDto.ExternalAuthenticationFailed(message ?? "外部認証に失敗しました");
            }
            catch (Exception ex)
            {
                return AuthenticationErrorDto.SystemError(
                    new InvalidOperationException($"ExternalAuthenticationFailedケースの処理に失敗: {ex.Message}", ex));
            }
        }
        else if (error.IsAuditLogError)
        {
            try
            {
                dynamic dynamicError = error;
                var message = dynamicError.Item as string;
                return AuthenticationErrorDto.AuditLogError(message ?? "監査ログの記録に失敗しました");
            }
            catch (Exception ex)
            {
                return AuthenticationErrorDto.SystemError(
                    new InvalidOperationException($"AuditLogErrorケースの処理に失敗: {ex.Message}", ex));
            }
        }
        else if (error.IsAccountDeactivated)
        {
            // AccountDeactivated ケースの追加
            return AuthenticationErrorDto.AccountDeactivated();
        }
        else
        {
            // 予期しないケース（将来の拡張対応）
            return AuthenticationErrorDto.SystemError(
                new ArgumentOutOfRangeException(nameof(error),
                    $"未知のAuthenticationErrorタイプです。F#コンパイラバージョンの違いやケース追加の可能性があります。"));
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
    public static FSharpResult<User, AuthenticationError> ToFSharpResult(AuthenticationResultDto dto)
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
            return FSharpResult<User, AuthenticationError>.NewError(authError);
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
    public static FSharpResult<Tuple<Email, string>, string> ToFSharpLoginParams(LoginRequestDto loginDto)
    {
        if (loginDto == null)
            return FSharpResult<Tuple<Email, string>, string>.NewError("ログインリクエストがnullです");

        if (string.IsNullOrWhiteSpace(loginDto.Email))
            return FSharpResult<Tuple<Email, string>, string>.NewError("メールアドレスが入力されていません");

        if (string.IsNullOrWhiteSpace(loginDto.Password))
            return FSharpResult<Tuple<Email, string>, string>.NewError("パスワードが入力されていません");

        // F#のEmail値オブジェクトによる検証
        var emailResult = Email.create(loginDto.Email);
        if (emailResult.IsError)
            return FSharpResult<Tuple<Email, string>, string>.NewError(emailResult.ErrorValue);

        // 成功: F#のタプルとして返す
        var loginParams = new Tuple<Email, string>(emailResult.ResultValue, loginDto.Password);
        return FSharpResult<Tuple<Email, string>, string>.NewOk(loginParams);
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
    public static FSharpResult<Email, string> ToFSharpPasswordResetParams(PasswordResetRequestDto resetDto)
    {
        if (resetDto == null)
            return FSharpResult<Email, string>.NewError("パスワードリセット要求がnullです");

        if (string.IsNullOrWhiteSpace(resetDto.Email))
            return FSharpResult<Email, string>.NewError("メールアドレスが入力されていません");

        // F#のEmail値オブジェクトによる検証
        var emailResult = Email.create(resetDto.Email);
        if (emailResult.IsError)
            return FSharpResult<Email, string>.NewError(emailResult.ErrorValue);

        return FSharpResult<Email, string>.NewOk(emailResult.ResultValue);
    }

    /// <summary>
    /// C# PasswordResetTokenDto を F# パスワードリセット実行パラメータに変換
    /// パスワードリセット実行時のデータ変換
    /// </summary>
    /// <param name="tokenDto">C#のパスワードリセットトークンDTO</param>
    /// <returns>F#のResult型（成功時はEmail*string*string、失敗時はvalidationエラー）</returns>
    public static FSharpResult<Tuple<Email, string, string>, string> ToFSharpPasswordResetExecuteParams(PasswordResetTokenDto tokenDto)
    {
        if (tokenDto == null)
            return FSharpResult<Tuple<Email, string, string>, string>.NewError("パスワードリセットトークンがnullです");

        if (string.IsNullOrWhiteSpace(tokenDto.Email))
            return FSharpResult<Tuple<Email, string, string>, string>.NewError("メールアドレスが入力されていません");

        if (string.IsNullOrWhiteSpace(tokenDto.Token))
            return FSharpResult<Tuple<Email, string, string>, string>.NewError("リセットトークンが入力されていません");

        if (string.IsNullOrWhiteSpace(tokenDto.NewPassword))
            return FSharpResult<Tuple<Email, string, string>, string>.NewError("新しいパスワードが入力されていません");

        // F#のEmail値オブジェクトによる検証
        var emailResult = Email.create(tokenDto.Email);
        if (emailResult.IsError)
            return FSharpResult<Tuple<Email, string, string>, string>.NewError(emailResult.ErrorValue);

        // F#のPassword値オブジェクトによる検証
        var passwordResult = Password.create(tokenDto.NewPassword);
        if (passwordResult.IsError)
            return FSharpResult<Tuple<Email, string, string>, string>.NewError(passwordResult.ErrorValue);

        // 成功: F#のタプルとして返す (Email, Token, NewPassword)
        var resetParams = new Tuple<Email, string, string>(emailResult.ResultValue, tokenDto.Token, tokenDto.NewPassword);
        return FSharpResult<Tuple<Email, string, string>, string>.NewOk(resetParams);
    }

    /// <summary>
    /// F# パスワードリセット結果 を C# PasswordResetResultDto に変換
    /// Application層からWeb層への結果変換
    /// </summary>
    /// <param name="result">F#のパスワードリセット結果</param>
    /// <param name="userEmail">対象ユーザーのメールアドレス</param>
    /// <returns>C#のPasswordResetResultDto</returns>
    public static PasswordResetResultDto ToPasswordResetResultDto<T>(FSharpResult<T, AuthenticationError> result, string userEmail)
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
}

/// <summary>
/// Phase A9: ログインリクエストDTO
/// Web層からのログイン要求を型安全に表現
/// </summary>
public class LoginRequestDto
{
    /// <summary>
    /// ログイン用メールアドレス
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// ログイン用パスワード（平文）
    /// セキュリティ上、ハッシュ化前の一時的な保持のみ
    /// </summary>
    public string Password { get; set; } = string.Empty;

    /// <summary>
    /// ログイン状態を保持するかどうか（Remember Me機能）
    /// </summary>
    public bool RememberMe { get; set; } = false;

    /// <summary>
    /// 二要素認証コード（二要素認証有効ユーザーの場合）
    /// </summary>
    public string? TwoFactorCode { get; set; }
}