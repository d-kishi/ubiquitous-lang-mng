using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.FSharp.Core;
using UbiquitousLanguageManager.Application;

namespace UbiquitousLanguageManager.Infrastructure.Services;

/// <summary>
/// F# Application層ILogger&lt;T&gt;インターフェースのアダプター実装（Phase A4 Step2で追加）
/// 
/// 【F#初学者向け解説】
/// F#のApplication層で定義されたILogger&lt;T&gt;インターフェースを、
/// Microsoft.Extensions.LoggingのILogger&lt;T&gt;にアダプターするクラスです。
/// これにより、F#のコードからC#の標準的なロギング機能を利用できます。
/// </summary>
public class FSharpLoggerAdapter<T> : UbiquitousLanguageManager.Application.ILogger<T>
{
    private readonly Microsoft.Extensions.Logging.ILogger<T> _logger;

    /// <summary>
    /// FSharpLoggerAdapterのコンストラクタ
    /// </summary>
    /// <param name="logger">Microsoft.Extensions.LoggingのILogger実装</param>
    public FSharpLoggerAdapter(Microsoft.Extensions.Logging.ILogger<T> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Information レベルのログを非同期で出力
    /// </summary>
    public Task<Unit> LogInformationAsync(string message)
    {
        _logger.LogInformation(message);
        return Task.FromResult<Unit>(null!);  // 元の実装（動作確認済み）
    }

    /// <summary>
    /// Warning レベルのログを非同期で出力
    /// </summary>
    public Task<Unit> LogWarningAsync(string message)
    {
        _logger.LogWarning(message);
        return Task.FromResult<Unit>(null!);  // 元の実装（動作確認済み）
    }

    /// <summary>
    /// Error レベルのログを非同期で出力
    /// </summary>
    public Task<Unit> LogErrorAsync(string message, FSharpOption<Exception> exception)
    {
        if (exception != null && FSharpOption<Exception>.get_IsSome(exception))
        {
            _logger.LogError(exception.Value, message);
        }
        else
        {
            _logger.LogError(message);
        }
        return Task.FromResult<Unit>(null!);  // 元の実装（動作確認済み）
    }

    /// <summary>
    /// Debug レベルのログを非同期で出力
    /// </summary>
    public Task<Unit> LogDebugAsync(string message)
    {
        _logger.LogDebug(message);
        return Task.FromResult<Unit>(null!);  // 元の実装（動作確認済み）
    }
}