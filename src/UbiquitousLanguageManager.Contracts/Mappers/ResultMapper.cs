using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.FSharp.Core;
using Microsoft.FSharp.Control;
using UbiquitousLanguageManager.Contracts.Exceptions;

namespace UbiquitousLanguageManager.Contracts.Mappers
{
    /// <summary>
    /// F# Result型をC#例外処理に変換するマッパー
    /// Clean Architecture境界での統一エラーハンドリングを実現
    /// </summary>
    public static class ResultMapper
    {
        private static ILogger? _logger;

        /// <summary>
        /// ロガーを設定します（依存性注入で設定）
        /// ResultMapper でのログ出力を有効化
        /// </summary>
        /// <param name="logger">ILoggerインスタンス</param>
        public static void SetLogger(ILogger logger)
        {
            _logger = logger;
        }
        /// <summary>
        /// F# Result&lt;T, string&gt; を C# 値に変換
        /// エラーの場合はDomainExceptionをスロー
        /// </summary>
        /// <typeparam name="T">結果型</typeparam>
        /// <param name="result">F# Result</param>
        /// <returns>成功時の値</returns>
        /// <exception cref="DomainException">F# Result がエラーの場合</exception>
        public static T MapResult<T>(FSharpResult<T, string> result)
        {
            var stopwatch = Stopwatch.StartNew();

            try
            {
                _logger?.LogDebug("F# Result→C#値変換開始 ResultType: {ResultType}", typeof(T).Name);

                if (result.IsOk)
                {
                    _logger?.LogDebug("F# Result→C#値変換成功 ResultType: {ResultType}, ConversionTime: {ConversionTime}ms",
                        typeof(T).Name, stopwatch.ElapsedMilliseconds);
                    return result.ResultValue;
                }
                else
                {
                    _logger?.LogWarning("F# Result→C#例外変換 ErrorMessage: {ErrorMessage}, ConversionTime: {ConversionTime}ms",
                        result.ErrorValue, stopwatch.ElapsedMilliseconds);
                    throw new DomainException(result.ErrorValue);
                }
            }
            catch (DomainException)
            {
                // DomainExceptionはそのまま再スロー（ログは上で出力済み）
                throw;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "F# Result→C#値変換で予期しないエラーが発生 ResultType: {ResultType}, ConversionTime: {ConversionTime}ms",
                    typeof(T).Name, stopwatch.ElapsedMilliseconds);
                throw;
            }
            finally
            {
                stopwatch.Stop();
            }
        }

        /// <summary>
        /// F# Async&lt;Result&lt;T, string&gt;&gt; を C# Task&lt;T&gt; に変換
        /// 非同期処理でのエラーハンドリング統一
        /// </summary>
        /// <typeparam name="T">結果型</typeparam>
        /// <param name="asyncResult">F# Async Result</param>
        /// <returns>成功時の値</returns>
        /// <exception cref="DomainException">F# Result がエラーの場合</exception>
        public static async Task<T> MapResultAsync<T>(FSharpAsync<FSharpResult<T, string>> asyncResult)
        {
            var result = await FSharpAsync.StartAsTask(asyncResult, null, null);
            return MapResult(result);
        }

        /// <summary>
        /// F# Option型をnull許容型に変換
        /// </summary>
        /// <typeparam name="T">値型</typeparam>
        /// <param name="option">F# Option</param>
        /// <returns>値またはnull</returns>
        public static T? MapOption<T>(FSharpOption<T> option) where T : class
        {
            return FSharpOption<T>.get_IsSome(option) ? option.Value : null;
        }
    }
}