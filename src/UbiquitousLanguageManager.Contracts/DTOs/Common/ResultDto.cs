namespace UbiquitousLanguageManager.Contracts.DTOs.Common;

/// <summary>
/// 汎用結果DTO
/// 
/// F#のResult型をC#で表現したDTOです。
/// F#のパターンマッチングの代わりに、IsSuccessフラグで成功・失敗を判定します。
/// Application層からWeb層への境界で、F#の型をC#の型に変換する際に使用されます。
/// </summary>
/// <typeparam name="TSuccess">成功時のデータ型</typeparam>
/// <typeparam name="TError">エラー時のデータ型</typeparam>
public class ResultDto<TSuccess, TError>
{
    /// <summary>
    /// 成功フラグ
    /// </summary>
    public bool IsSuccess { get; }

    /// <summary>
    /// 成功時のデータ
    /// </summary>
    public TSuccess? Value { get; }

    /// <summary>
    /// エラー時のデータ
    /// </summary>
    public TError? Error { get; }

    /// <summary>
    /// プロテクトコンストラクタ
    /// ファクトリーメソッドと継承クラスからのみ作成可能
    /// </summary>
    protected ResultDto(bool isSuccess, TSuccess? value, TError? error)
    {
        IsSuccess = isSuccess;
        Value = value;
        Error = error;
    }

    /// <summary>
    /// 成功結果の作成
    /// </summary>
    public static ResultDto<TSuccess, TError> Success(TSuccess value)
    {
        return new ResultDto<TSuccess, TError>(true, value, default(TError));
    }

    /// <summary>
    /// エラー結果の作成
    /// </summary>
    public static ResultDto<TSuccess, TError> Failure(TError error)
    {
        return new ResultDto<TSuccess, TError>(false, default(TSuccess), error);
    }
}

/// <summary>
/// シンプルな結果DTO（エラーは文字列固定）
/// </summary>
/// <typeparam name="TSuccess">成功時のデータ型</typeparam>
public class ResultDto<TSuccess> : ResultDto<TSuccess, string>
{
    /// <summary>
    /// プロテクトコンストラクタ
    /// ファクトリーメソッドと継承クラスからのみ作成可能
    /// </summary>
    /// <param name="isSuccess">成功フラグ</param>
    /// <param name="value">成功時のデータ</param>
    /// <param name="error">エラー時のメッセージ</param>
    protected ResultDto(bool isSuccess, TSuccess? value, string? error) 
        : base(isSuccess, value, error)
    {
    }

    /// <summary>
    /// 成功結果の作成
    /// </summary>
    public static new ResultDto<TSuccess> Success(TSuccess value)
    {
        return new ResultDto<TSuccess>(true, value, null);
    }

    /// <summary>
    /// エラー結果の作成
    /// </summary>
    public static new ResultDto<TSuccess> Failure(string error)
    {
        return new ResultDto<TSuccess>(false, default(TSuccess), error);
    }
}

/// <summary>
/// 結果DTO（データなし版）
/// </summary>
public class ResultDto : ResultDto<object, string>
{
    /// <summary>
    /// プロテクトコンストラクタ
    /// ファクトリーメソッドと継承クラスからのみ作成可能
    /// </summary>
    /// <param name="isSuccess">成功フラグ</param>
    /// <param name="error">エラー時のメッセージ</param>
    protected ResultDto(bool isSuccess, string? error) 
        : base(isSuccess, null, error)
    {
    }

    /// <summary>
    /// 成功結果の作成
    /// </summary>
    public static ResultDto Success()
    {
        return new ResultDto(true, null);
    }

    /// <summary>
    /// エラー結果の作成
    /// </summary>
    public static new ResultDto Failure(string error)
    {
        return new ResultDto(false, error);
    }
}