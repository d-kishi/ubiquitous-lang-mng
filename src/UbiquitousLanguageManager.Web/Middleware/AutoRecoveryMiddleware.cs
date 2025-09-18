using System.Text.Json;
using System.Text.RegularExpressions;

namespace UbiquitousLanguageManager.Web.Middleware;

/// <summary>
/// 【GitHub Issue #17】自動リカバリミドルウェア
/// 実行時エラーを自動検知し、可能な場合は自動修正を試みます
/// </summary>
public class AutoRecoveryMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<AutoRecoveryMiddleware> _logger;
    private readonly ErrorPatternMatcher _errorMatcher;

    /// <summary>
    /// コンストラクタ
    /// </summary>
    public AutoRecoveryMiddleware(RequestDelegate next, ILogger<AutoRecoveryMiddleware> logger)
    {
        _next = next;
        _logger = logger;
        _errorMatcher = new ErrorPatternMatcher(logger);
    }

    /// <summary>
    /// ミドルウェアの実行
    /// </summary>
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        _logger.LogError(exception, "自動リカバリミドルウェアでエラーを検知: {Message}", exception.Message);
        
        try
        {
            var errorInfo = _errorMatcher.MatchError(exception);
            
            if (errorInfo != null && errorInfo.AutoFixable)
            {
                _logger.LogInformation("自動修正可能なエラーを検出: {Pattern}", errorInfo.PatternName);
                
                var recovered = await AttemptAutoRecovery(context, errorInfo, exception);
                
                if (recovered)
                {
                    _logger.LogInformation("エラーの自動修正に成功: {Pattern}", errorInfo.PatternName);
                    
                    // 修正後にリトライ（ただし無限ループを防ぐため制限）
                    if (!context.Items.ContainsKey("AutoRecoveryAttempted"))
                    {
                        context.Items["AutoRecoveryAttempted"] = true;
                        await _next(context);
                        return;
                    }
                }
            }
        }
        catch (Exception recoveryEx)
        {
            _logger.LogError(recoveryEx, "自動修正中にエラーが発生: {Message}", recoveryEx.Message);
        }

        // 自動修正できない場合、またはリトライ制限に達した場合の処理
        await HandleUnrecoverableError(context, exception);
    }

    private async Task<bool> AttemptAutoRecovery(HttpContext context, ErrorInfo errorInfo, Exception exception)
    {
        foreach (var action in errorInfo.Actions)
        {
            try
            {
                _logger.LogInformation("自動修正アクション実行中: {ActionType}", action.Type);
                
                var success = action.Type switch
                {
                    "powershell_script" => await ExecutePowerShellScript(action),
                    "middleware_recovery" => await HandleMiddlewareRecovery(context, action),
                    "restart_application" => await HandleApplicationRestart(action),
                    "dotnet_command" => await ExecuteDotNetCommand(action),
                    _ => false
                };

                if (success)
                {
                    _logger.LogInformation("自動修正アクション成功: {ActionType}", action.Type);
                    return true;
                }
            }
            catch (Exception actionEx)
            {
                _logger.LogWarning(actionEx, "自動修正アクション失敗: {ActionType} - {Message}", 
                    action.Type, actionEx.Message);
            }
        }

        return false;
    }

    private async Task<bool> ExecutePowerShellScript(ErrorAction action)
    {
        try
        {
            var startInfo = new System.Diagnostics.ProcessStartInfo
            {
                FileName = "powershell.exe",
                Arguments = $"-ExecutionPolicy Bypass -File {action.Command} {string.Join(" ", action.Args ?? Array.Empty<string>())}",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            using var process = System.Diagnostics.Process.Start(startInfo);
            if (process != null)
            {
                await process.WaitForExitAsync();
                
                if (process.ExitCode <= 1) // 0=成功、1=エラー検出なし
                {
                    _logger.LogInformation("PowerShellスクリプト実行成功: {Command}", action.Command);
                    return true;
                }
                else
                {
                    _logger.LogWarning("PowerShellスクリプト実行失敗: ExitCode={ExitCode}", process.ExitCode);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "PowerShellスクリプト実行中にエラー: {Command}", action.Command);
        }

        return false;
    }

    private async Task<bool> HandleMiddlewareRecovery(HttpContext context, ErrorAction action)
    {
        try
        {
            switch (action.Command)
            {
                case "restart_with_api_endpoint":
                    // Headers read-onlyエラーの場合、レスポンスをリセット
                    if (!context.Response.HasStarted)
                    {
                        context.Response.Clear();
                        context.Response.StatusCode = 500;
                        context.Response.ContentType = "application/json";
                        
                        var errorResponse = new
                        {
                            error = "Internal server error - automatically recovered",
                            recovered = true,
                            timestamp = DateTimeOffset.UtcNow
                        };
                        
                        await context.Response.WriteAsync(JsonSerializer.Serialize(errorResponse));
                        return true;
                    }
                    break;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "ミドルウェアリカバリ中にエラー: {Command}", action.Command);
        }

        return false;
    }

    private async Task<bool> HandleApplicationRestart(ErrorAction action)
    {
        // 本格的なアプリケーション再起動は実装しない（安全性のため）
        // ログ記録のみ
        _logger.LogWarning("アプリケーション再起動が必要ですが、安全性のため実行しません: {Action}", action.Description);
        await Task.CompletedTask;
        return false;
    }

    private async Task<bool> ExecuteDotNetCommand(ErrorAction action)
    {
        try
        {
            var startInfo = new System.Diagnostics.ProcessStartInfo
            {
                FileName = "dotnet",
                Arguments = action.Command.Replace("dotnet ", ""),
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            using var process = System.Diagnostics.Process.Start(startInfo);
            if (process != null)
            {
                await process.WaitForExitAsync();
                
                if (process.ExitCode == 0)
                {
                    _logger.LogInformation("dotnetコマンド実行成功: {Command}", action.Command);
                    return true;
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "dotnetコマンド実行中にエラー: {Command}", action.Command);
        }

        return false;
    }

    private async Task HandleUnrecoverableError(HttpContext context, Exception exception)
    {
        try
        {
            if (!context.Response.HasStarted)
            {
                context.Response.StatusCode = 500;
                context.Response.ContentType = "application/json";
                
                var errorResponse = new
                {
                    error = "Internal server error - recovery attempted but failed",
                    message = exception.Message,
                    recovered = false,
                    timestamp = DateTimeOffset.UtcNow,
                    suggestion = "Please try refreshing the page or contact support if the problem persists."
                };
                
                await context.Response.WriteAsync(JsonSerializer.Serialize(errorResponse));
            }
        }
        catch (Exception responseEx)
        {
            _logger.LogError(responseEx, "エラーレスポンス送信中にエラー: {Message}", responseEx.Message);
        }
    }
}

/// <summary>
/// エラーパターンマッチャー
/// </summary>
internal class ErrorPatternMatcher
{
    private readonly ErrorPatterns _patterns;
    private readonly ILogger<AutoRecoveryMiddleware> _logger;

    public ErrorPatternMatcher(ILogger<AutoRecoveryMiddleware> logger)
    {
        _logger = logger;
        _patterns = LoadErrorPatterns();
    }

    public ErrorInfo? MatchError(Exception exception)
    {
        var errorMessage = exception.ToString();

        foreach (var pattern in _patterns.Patterns)
        {
            foreach (var regexPattern in pattern.Value.Patterns)
            {
                try
                {
                    if (Regex.IsMatch(errorMessage, regexPattern, RegexOptions.IgnoreCase))
                    {
                        return new ErrorInfo
                        {
                            PatternName = pattern.Key,
                            Description = pattern.Value.Description,
                            Severity = pattern.Value.Severity,
                            AutoFixable = pattern.Value.AutoFix,
                            Actions = pattern.Value.Actions?.Select(a => new ErrorAction
                            {
                                Type = a.Type,
                                Command = a.Command,
                                Args = a.Args,
                                Description = a.Description
                            }).ToList() ?? new List<ErrorAction>()
                        };
                    }
                }
                catch (ArgumentException ex)
                {
                    // 正規表現の問題は無視してログ記録
                    _logger.LogWarning(ex, "正規表現エラー: {RegexPattern} - {Message}", regexPattern, ex.Message);
                }
            }
        }

        return null;
    }

    private ErrorPatterns LoadErrorPatterns()
    {
        try
        {
            var jsonPath = Path.Combine(Directory.GetCurrentDirectory(), ".claude", "scripts", "error-patterns.json");
            if (File.Exists(jsonPath))
            {
                var jsonContent = File.ReadAllText(jsonPath);
                return JsonSerializer.Deserialize<ErrorPatterns>(jsonContent, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
                }) ?? new ErrorPatterns();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "エラーパターン読み込み失敗: {Message}", ex.Message);
        }

        // フォールバック: デフォルトパターン
        return new ErrorPatterns
        {
            Patterns = new Dictionary<string, PatternDefinition>
            {
                ["process_lock"] = new()
                {
                    Description = "プロセスロックエラー",
                    Patterns = new[] { "The process cannot access the file", "being used by another process" },
                    Severity = "high",
                    AutoFix = true,
                    Actions = new[]
                    {
                        new ActionDefinition
                        {
                            Type = "powershell_script",
                            Command = ".claude/scripts/auto-fix-errors.ps1",
                            Args = new[] { "-Action", "process" },
                            Description = "プロセス終了"
                        }
                    }
                }
            }
        };
    }
}

// データモデル
internal class ErrorInfo
{
    public string PatternName { get; set; } = "";
    public string Description { get; set; } = "";
    public string Severity { get; set; } = "";
    public bool AutoFixable { get; set; }
    public List<ErrorAction> Actions { get; set; } = new();
}

internal class ErrorAction
{
    public string Type { get; set; } = "";
    public string Command { get; set; } = "";
    public string[]? Args { get; set; }
    public string Description { get; set; } = "";
}

internal class ErrorPatterns
{
    public Dictionary<string, PatternDefinition> Patterns { get; set; } = new();
}

internal class PatternDefinition
{
    public string Description { get; set; } = "";
    public string[] Patterns { get; set; } = Array.Empty<string>();
    public string Severity { get; set; } = "";
    public bool AutoFix { get; set; }
    public ActionDefinition[]? Actions { get; set; }
}

internal class ActionDefinition
{
    public string Type { get; set; } = "";
    public string Command { get; set; } = "";
    public string[]? Args { get; set; }
    public string Description { get; set; } = "";
}