using Microsoft.AspNetCore.Mvc;
using UbiquitousLanguageManager.Infrastructure.Services;

namespace UbiquitousLanguageManager.Web.Controllers;

/// <summary>
/// 初期データ投入用コントローラー
/// Phase A8 Step5 Stage3補完: admin@ubiquitous-lang.com確実作成用
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class SeedController : ControllerBase
{
    private readonly InitialDataService _initialDataService;
    private readonly ILogger<SeedController> _logger;

    /// <summary>
    /// SeedControllerのコンストラクタ
    /// </summary>
    /// <param name="initialDataService">初期データサービス</param>
    /// <param name="logger">ロガー</param>
    public SeedController(
        InitialDataService initialDataService,
        ILogger<SeedController> logger)
    {
        _initialDataService = initialDataService;
        _logger = logger;
    }

    /// <summary>
    /// 初期データ投入API
    /// GET /api/seed/initial-data
    /// </summary>
    [HttpGet("initial-data")]
    public async Task<IActionResult> SeedInitialData()
    {
        try
        {
            _logger.LogInformation("🚀 手動初期データ投入API呼び出し");
            
            await _initialDataService.SeedInitialDataAsync();
            
            _logger.LogInformation("✅ 初期データ投入完了");
            
            return Ok(new { 
                success = true, 
                message = "初期データ投入が正常に完了しました",
                timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ 初期データ投入中にエラーが発生");
            
            return StatusCode(500, new {
                success = false,
                message = "初期データ投入中にエラーが発生しました",
                error = ex.Message,
                timestamp = DateTime.UtcNow
            });
        }
    }
}