using Microsoft.FSharp.Core;

namespace UbiquitousLanguageManager.Web.Tests.Infrastructure;

/// <summary>
/// F#型システムとC#の統合を簡潔にする拡張メソッド集
///
/// 【対象型】
/// - FSharpResult&lt;T, string&gt;: F# Result型（成功/失敗）
/// - FSharpOption&lt;T&gt;: F# Option型（値あり/なし）
///
/// 【使用例】
/// // Result型生成
/// var okResult = projectDto.ToOkResult();
/// var errorResult = "エラーメッセージ".ToErrorResult&lt;ProjectDto&gt;();
///
/// // Option型生成
/// var someValue = "検索キーワード".ToSome();
/// var noneValue = FSharpTypeHelpers.ToNone&lt;string&gt;();
///
/// // Result型検証
/// result.IsOk.Should().BeTrue();
/// result.ErrorValue.Should().Be("予期されるエラー");
/// </summary>
public static class FSharpTypeHelpers
{
    #region FSharpResult 生成ヘルパー

    /// <summary>
    /// C#オブジェクトをF# Result型のOk値に変換
    /// </summary>
    /// <typeparam name="T">成功値の型</typeparam>
    /// <param name="value">成功値</param>
    /// <returns>FSharpResult&lt;T, string&gt;.NewOk</returns>
    public static FSharpResult<T, string> ToOkResult<T>(this T value)
        => FSharpResult<T, string>.NewOk(value);

    /// <summary>
    /// エラーメッセージをF# Result型のError値に変換
    /// </summary>
    /// <typeparam name="T">成功値の型（Errorには含まれない）</typeparam>
    /// <param name="errorMessage">エラーメッセージ</param>
    /// <returns>FSharpResult&lt;T, string&gt;.NewError</returns>
    public static FSharpResult<T, string> ToErrorResult<T>(this string errorMessage)
        => FSharpResult<T, string>.NewError(errorMessage);

    #endregion

    #region FSharpOption 生成ヘルパー

    /// <summary>
    /// C#オブジェクトをF# Option型のSome値に変換
    /// </summary>
    /// <typeparam name="T">値の型</typeparam>
    /// <param name="value">値</param>
    /// <returns>FSharpOption&lt;T&gt;.Some</returns>
    public static FSharpOption<T> ToSome<T>(this T value)
        => FSharpOption<T>.Some(value);

    /// <summary>
    /// F# Option型のNone値を生成
    ///
    /// 【重要】
    /// 型推論が効かない場合は明示的に型指定必須
    /// 例: var noneValue = FSharpTypeHelpers.ToNone&lt;string&gt;();
    /// </summary>
    /// <typeparam name="T">値の型</typeparam>
    /// <returns>FSharpOption&lt;T&gt;.None</returns>
    public static FSharpOption<T> ToNone<T>()
        => FSharpOption<T>.None;

    #endregion

    #region FSharpOption 検証ヘルパー

    /// <summary>
    /// Option型がSome値か判定
    /// </summary>
    public static bool IsSome<T>(this FSharpOption<T> option)
        => FSharpOption<T>.get_IsSome(option);

    /// <summary>
    /// Option型がNone値か判定
    /// </summary>
    public static bool IsNone<T>(this FSharpOption<T> option)
        => FSharpOption<T>.get_IsNone(option);

    /// <summary>
    /// Option型から値を取得（None時はデフォルト値）
    /// </summary>
    public static T GetValueOrDefault<T>(this FSharpOption<T> option, T defaultValue = default!)
        => option.IsSome() ? option.Value : defaultValue;

    #endregion
}
