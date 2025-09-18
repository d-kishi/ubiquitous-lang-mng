using System.Net;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;

namespace UbiquitousLanguageManager.Web.Middleware;

/// <summary>
/// グローバル例外ハンドリングミドルウェア
/// アプリケーション全体で発生した例外を適切なHTTPレスポンスに変換
/// Blazor Server・F#初学者向けに詳細なコメントを記載
/// </summary>
public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;

    /// <summary>
    /// コンストラクタ
    /// ASP.NET Coreのミドルウェアパイプラインで使用
    /// </summary>
    /// <param name="next">次のミドルウェアへの委譲</param>
    /// <param name="logger">ログ出力</param>
    public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    /// <summary>
    /// ミドルウェアのメイン処理
    /// リクエストの処理中に発生した例外をキャッチし、適切なレスポンスを返す
    /// </summary>
    /// <param name="context">HTTPコンテキスト</param>
    /// <returns>非同期処理タスク</returns>
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            // 次のミドルウェアまたはエンドポイントを実行
            await _next(context);
        }
        catch (Exception ex)
        {
            // 例外をログに記録（構造化ログで詳細情報を記録）
            _logger.LogError(ex, "アプリケーション未処理例外が発生 Path: {Path}, Method: {Method}, StatusCode: {StatusCode}, Error: {ErrorMessage}",
                context.Request.Path, context.Request.Method, GetStatusCodeFromException(ex), ex.Message);

            // 適切なHTTPレスポンスを返す
            await HandleExceptionAsync(context, ex);
        }
    }

    /// <summary>
    /// 例外を適切なHTTPレスポンスに変換
    /// 例外の種類に応じて適切なステータスコードとメッセージを返す
    /// </summary>
    /// <param name="context">HTTPコンテキスト</param>
    /// <param name="exception">発生した例外</param>
    /// <returns>非同期処理タスク</returns>
    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        // レスポンスの基本設定
        context.Response.ContentType = "application/json";

        // 例外の種類に応じたステータスコードとメッセージの決定
        var (statusCode, message) = GetErrorResponse(exception);
        context.Response.StatusCode = (int)statusCode;

        // エラーレスポンスの作成
        var response = new
        {
            error = new
            {
                message = message,
                statusCode = (int)statusCode,
                timestamp = DateTime.UtcNow.ToString("O"), // ISO 8601形式
                path = context.Request.Path.Value
            }
        };

        // JSON形式でエラーレスポンスを返す
        var jsonResponse = JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await context.Response.WriteAsync(jsonResponse);
    }

    /// <summary>
    /// 例外の種類に応じた適切なHTTPステータスコードとメッセージを決定
    /// </summary>
    /// <param name="exception">発生した例外</param>
    /// <returns>HTTPステータスコードとエラーメッセージのタプル</returns>
    private static (HttpStatusCode statusCode, string message) GetErrorResponse(Exception exception)
    {
        return exception switch
        {
            // ArgumentNullException, ArgumentException: 400 Bad Request
            ArgumentNullException => (HttpStatusCode.BadRequest, "必須パラメータが指定されていません"),
            ArgumentException => (HttpStatusCode.BadRequest, "無効なパラメータが指定されました"),

            // Entity Framework Core関連の例外
            DbUpdateException dbEx => HandleDbUpdateException(dbEx),

            // 認証・認可関連の例外
            UnauthorizedAccessException => (HttpStatusCode.Unauthorized, "認証が必要です"),
            InvalidOperationException invalidEx when invalidEx.Message.Contains("権限") => 
                (HttpStatusCode.Forbidden, "このリソースにアクセスする権限がありません"),

            // データが見つからない場合
            KeyNotFoundException => (HttpStatusCode.NotFound, "指定されたリソースが見つかりません"),

            // タイムアウト関連
            TimeoutException => (HttpStatusCode.RequestTimeout, "リクエストがタイムアウトしました"),

            // その他の例外: 500 Internal Server Error
            _ => (HttpStatusCode.InternalServerError, "サーバー内部エラーが発生しました")
        };
    }

    /// <summary>
    /// Entity Framework Coreのデータベース更新例外を処理
    /// PostgreSQL固有のエラーコードを解析して適切なメッセージを返す
    /// </summary>
    /// <param name="dbEx">データベース更新例外</param>
    /// <returns>HTTPステータスコードとエラーメッセージのタプル</returns>
    private static (HttpStatusCode statusCode, string message) HandleDbUpdateException(DbUpdateException dbEx)
    {
        // PostgreSQL固有のエラーコードを確認
        var innerException = dbEx.InnerException?.Message ?? string.Empty;

        // 一意制約違反 (PostgreSQLエラーコード: 23505)
        if (innerException.Contains("23505"))
        {
            return (HttpStatusCode.Conflict, "指定されたデータは既に存在します");
        }

        // 外部キー制約違反 (PostgreSQLエラーコード: 23503)
        if (innerException.Contains("23503"))
        {
            return (HttpStatusCode.BadRequest, "関連データが存在しないため、操作を実行できません");
        }

        // NOT NULL制約違反 (PostgreSQLエラーコード: 23502)
        if (innerException.Contains("23502"))
        {
            return (HttpStatusCode.BadRequest, "必須フィールドが設定されていません");
        }

        // チェック制約違反 (PostgreSQLエラーコード: 23514)
        if (innerException.Contains("23514"))
        {
            return (HttpStatusCode.BadRequest, "データの形式が正しくありません");
        }

        // その他のデータベース更新エラー
        return (HttpStatusCode.InternalServerError, "データベースの更新に失敗しました");
    }

    /// <summary>
    /// 例外からHTTPステータスコードを取得（ログ用）
    /// </summary>
    /// <param name="exception">発生した例外</param>
    /// <returns>HTTPステータスコード</returns>
    private static int GetStatusCodeFromException(Exception exception)
    {
        var (statusCode, _) = GetErrorResponse(exception);
        return (int)statusCode;
    }
}

/// <summary>
/// GlobalExceptionMiddlewareの拡張メソッド
/// Program.csでのミドルウェア登録を簡素化
/// </summary>
public static class GlobalExceptionMiddlewareExtensions
{
    /// <summary>
    /// GlobalExceptionMiddlewareをミドルウェアパイプラインに登録
    /// </summary>
    /// <param name="app">アプリケーションビルダー</param>
    /// <returns>アプリケーションビルダー</returns>
    public static IApplicationBuilder UseGlobalExceptionHandling(this IApplicationBuilder app)
    {
        return app.UseMiddleware<GlobalExceptionMiddleware>();
    }
}