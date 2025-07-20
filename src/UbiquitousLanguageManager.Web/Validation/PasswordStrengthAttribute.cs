using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace UbiquitousLanguageManager.Web.Validation;

/// <summary>
/// パスワード強度検証属性
/// 
/// 【Blazor Server初学者向け解説】
/// カスタムバリデーション属性を作成することで、複雑なバリデーションロジックを
/// 再利用可能な形で実装できます。この属性はEditFormのバリデーションで自動的に使用されます。
/// </summary>
public class PasswordStrengthAttribute : ValidationAttribute
{
    private readonly int _minimumLength;
    private readonly bool _requireUppercase;
    private readonly bool _requireLowercase;
    private readonly bool _requireDigit;
    private readonly bool _requireSpecialCharacter;

    /// <summary>
    /// パスワード強度検証属性のコンストラクタ
    /// </summary>
    /// <param name="minimumLength">最小文字数</param>
    /// <param name="requireUppercase">大文字必須フラグ</param>
    /// <param name="requireLowercase">小文字必須フラグ</param>
    /// <param name="requireDigit">数字必須フラグ</param>
    /// <param name="requireSpecialCharacter">特殊文字必須フラグ</param>
    public PasswordStrengthAttribute(
        int minimumLength = 8,
        bool requireUppercase = true,
        bool requireLowercase = true,
        bool requireDigit = true,
        bool requireSpecialCharacter = true)
    {
        _minimumLength = minimumLength;
        _requireUppercase = requireUppercase;
        _requireLowercase = requireLowercase;
        _requireDigit = requireDigit;
        _requireSpecialCharacter = requireSpecialCharacter;
        
        // デフォルトエラーメッセージ
        ErrorMessage = BuildErrorMessage();
    }

    /// <summary>
    /// パスワード強度を検証します
    /// </summary>
    /// <param name="value">検証対象の値</param>
    /// <param name="validationContext">検証コンテキスト</param>
    /// <returns>検証結果</returns>
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        // nullまたは空文字の場合は、Requiredアトリビュートに委譲
        if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
        {
            return ValidationResult.Success;
        }

        var password = value.ToString()!;
        var errors = new List<string>();

        // 長さチェック
        if (password.Length < _minimumLength)
        {
            errors.Add($"{_minimumLength}文字以上");
        }

        // 大文字チェック
        if (_requireUppercase && !Regex.IsMatch(password, @"[A-Z]"))
        {
            errors.Add("大文字");
        }

        // 小文字チェック
        if (_requireLowercase && !Regex.IsMatch(password, @"[a-z]"))
        {
            errors.Add("小文字");
        }

        // 数字チェック
        if (_requireDigit && !Regex.IsMatch(password, @"\d"))
        {
            errors.Add("数字");
        }

        // 特殊文字チェック
        if (_requireSpecialCharacter && !Regex.IsMatch(password, @"[!@#$%^&*()_+\-=\[\]{};':""\\|,.<>\/?]"))
        {
            errors.Add("記号");
        }

        if (errors.Any())
        {
            var errorMessage = $"パスワードは{string.Join("、", errors)}を含む必要があります";
            return new ValidationResult(errorMessage, new[] { validationContext.MemberName ?? string.Empty });
        }

        return ValidationResult.Success;
    }

    private string BuildErrorMessage()
    {
        var requirements = new List<string>();
        
        requirements.Add($"{_minimumLength}文字以上");
        
        if (_requireUppercase) requirements.Add("大文字");
        if (_requireLowercase) requirements.Add("小文字");
        if (_requireDigit) requirements.Add("数字");
        if (_requireSpecialCharacter) requirements.Add("記号");

        return $"パスワードは{string.Join("、", requirements)}を含む必要があります";
    }
}

/// <summary>
/// パスワード確認一致検証属性
/// </summary>
public class PasswordMatchAttribute : ValidationAttribute
{
    private readonly string _passwordPropertyName;

    /// <summary>
    /// パスワード確認一致検証属性のコンストラクタ
    /// </summary>
    /// <param name="passwordPropertyName">パスワードプロパティ名</param>
    public PasswordMatchAttribute(string passwordPropertyName)
    {
        _passwordPropertyName = passwordPropertyName;
        ErrorMessage = "パスワードと確認用パスワードが一致しません";
    }

    /// <summary>
    /// パスワード確認一致を検証します
    /// </summary>
    /// <param name="value">検証対象の値</param>
    /// <param name="validationContext">検証コンテキスト</param>
    /// <returns>検証結果</returns>
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value == null)
        {
            return ValidationResult.Success;
        }

        var passwordProperty = validationContext.ObjectType.GetProperty(_passwordPropertyName);
        if (passwordProperty == null)
        {
            throw new ArgumentException($"Property {_passwordPropertyName} not found");
        }

        var passwordValue = passwordProperty.GetValue(validationContext.ObjectInstance);
        
        if (!value.Equals(passwordValue))
        {
            return new ValidationResult(ErrorMessage, new[] { validationContext.MemberName ?? string.Empty });
        }

        return ValidationResult.Success;
    }
}