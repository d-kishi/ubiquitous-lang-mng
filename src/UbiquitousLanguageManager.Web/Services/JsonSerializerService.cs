using System.Text.Json;

namespace UbiquitousLanguageManager.Web.Services;

/// <summary>
/// Blazor Server用JSON一括管理サービスインターフェース
/// 
/// 【技術負債予防・DRY原則準拠】
/// ConfigureHttpJsonOptionsはWeb API専用のため、Blazor Component内での
/// 直接JsonSerializer使用には適用されない問題を解決するための統一サービス。
/// 
/// 【Blazor Server初学者向け解説】
/// Blazor ComponentでJSON処理を行う際、各Componentで個別に
/// JsonSerializerOptionsを設定すると設定の重複・不整合が発生します。
/// このサービスにより、全体で統一されたJSON設定を提供します。
/// </summary>
public interface IJsonSerializerService
{
    /// <summary>
    /// JSONデシリアライズ処理
    /// </summary>
    /// <typeparam name="T">デシリアライズ対象の型</typeparam>
    /// <param name="json">JSON文字列</param>
    /// <returns>デシリアライズされたオブジェクト</returns>
    T? Deserialize<T>(string json);

    /// <summary>
    /// JSONシリアライズ処理
    /// </summary>
    /// <typeparam name="T">シリアライズ対象の型</typeparam>
    /// <param name="value">シリアライズ対象オブジェクト</param>
    /// <returns>JSON文字列</returns>
    string Serialize<T>(T value);
}

/// <summary>
/// Blazor Server用JSON一括管理サービス実装
/// 
/// 【実装方針】
/// Program.cs ConfigureHttpJsonOptionsと完全同一の設定を提供し、
/// Web APIとBlazor Componentでの一貫性を保証。
/// 
/// 【設定詳細】
/// - PropertyNameCaseInsensitive: JavaScript {success: true} ↔ C# {Success: true} 統一
/// - PropertyNamingPolicy.CamelCase: JSON出力時のcamelCase統一
/// </summary>
public class JsonSerializerService : IJsonSerializerService
{
    private readonly JsonSerializerOptions _options;
    
    /// <summary>
    /// コンストラクター - 統一JSON設定の初期化
    /// 
    /// 【重要】Program.cs ConfigureHttpJsonOptionsと完全同一設定
    /// 将来的にConfigureHttpJsonOptionsの設定変更時は、この設定も同期更新必要
    /// </summary>
    public JsonSerializerService()
    {
        _options = new JsonSerializerOptions
        {
            // 【JavaScript ↔ C# 統合標準化】
            // JavaScript側: {success: true, message: "OK"}
            // C#側: {Success: true, Message: "OK"}
            // この設定により、プロパティ名の大文字小文字を区別せず自動マッピング
            PropertyNameCaseInsensitive = true,
            
            // 【JSON出力標準化】
            // C# {Success: true} → JSON {"success": true}
            // JavaScript標準のcamelCase形式に統一
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
    }
    
    /// <summary>
    /// JSONデシリアライズ処理
    /// 
    /// 【Blazor Server初学者向け解説】
    /// JavaScript API呼び出しの戻り値や、外部システムからの
    /// JSON データをC#オブジェクトに変換する際に使用します。
    /// </summary>
    /// <typeparam name="T">デシリアライズ対象の型</typeparam>
    /// <param name="json">JSON文字列</param>
    /// <returns>デシリアライズされたオブジェクト（失敗時はnull）</returns>
    public T? Deserialize<T>(string json)
    {
        // 統一設定を使用したデシリアライズ実行
        // Program.cs ConfigureHttpJsonOptionsと同一の挙動を保証
        return JsonSerializer.Deserialize<T>(json, _options);
    }
    
    /// <summary>
    /// JSONシリアライズ処理
    /// 
    /// 【Blazor Server初学者向け解説】
    /// C#オブジェクトをJSONに変換し、JavaScript側に送信したり
    /// 外部システムへのAPI呼び出しで使用する際に活用します。
    /// </summary>
    /// <typeparam name="T">シリアライズ対象の型</typeparam>
    /// <param name="value">シリアライズ対象オブジェクト</param>
    /// <returns>JSON文字列</returns>
    public string Serialize<T>(T value)
    {
        // 統一設定を使用したシリアライズ実行
        // JavaScript標準のcamelCase形式で出力
        return JsonSerializer.Serialize(value, _options);
    }
}