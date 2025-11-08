using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ScoutVision.Infrastructure.Reporting;

namespace ScoutVision.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ReportController : ControllerBase
{
    private readonly IReportService _reportService;
    private readonly IReportAnalyticsService _analyticsService;

    public ReportController(
        IReportService reportService,
        IReportAnalyticsService analyticsService)
    {
        _reportService = reportService;
        _analyticsService = analyticsService;
    }

    /// <summary>
    /// Generate player scouting report in PDF format
    /// </summary>
    [HttpPost("player/{playerId}/pdf")]
    public async Task<IActionResult> GeneratePlayerReportPdf(
        int playerId,
        [FromBody] ReportOptions? options = null)
    {
        var reportOptions = options ?? new ReportOptions();
        var pdfBytes = await _reportService.GeneratePlayerReportPdfAsync(playerId, reportOptions);
        
        return File(pdfBytes, "application/pdf", $"player-report-{playerId}.pdf");
    }

    /// <summary>
    /// Generate team analysis report in PDF format
    /// </summary>
    [HttpPost("team/{teamId}/pdf")]
    public async Task<IActionResult> GenerateTeamReportPdf(
        int teamId,
        [FromBody] ReportOptions? options = null)
    {
        var reportOptions = options ?? new ReportOptions();
        var pdfBytes = await _reportService.GenerateTeamReportPdfAsync(teamId, reportOptions);
        
        return File(pdfBytes, "application/pdf", $"team-report-{teamId}.pdf");
    }

    /// <summary>
    /// Generate injury report in PDF format
    /// </summary>
    [HttpPost("injury/{clubId}/pdf")]
    public async Task<IActionResult> GenerateInjuryReportPdf(
        int clubId,
        [FromQuery] DateTime from,
        [FromQuery] DateTime to)
    {
        var pdfBytes = await _reportService.GenerateInjuryReportPdfAsync(clubId, from, to);
        
        return File(pdfBytes, "application/pdf", $"injury-report-{clubId}.pdf");
    }

    /// <summary>
    /// Generate transfer market report in PDF format
    /// </summary>
    [HttpPost("transfer/{clubId}/pdf")]
    public async Task<IActionResult> GenerateTransferReportPdf(
        int clubId,
        [FromBody] ReportOptions? options = null)
    {
        var reportOptions = options ?? new ReportOptions();
        var pdfBytes = await _reportService.GenerateTransferReportPdfAsync(clubId, reportOptions);
        
        return File(pdfBytes, "application/pdf", $"transfer-report-{clubId}.pdf");
    }

    /// <summary>
    /// Generate player statistics in Excel format
    /// </summary>
    [HttpPost("player/excel")]
    public async Task<IActionResult> GeneratePlayerStatsExcel(
        [FromBody] PlayerStatsExcelRequest request)
    {
        var excelBytes = await _reportService.GeneratePlayerStatsExcelAsync(
            request.PlayerIds,
            request.Options ?? new ExcelReportOptions());
        
        return File(excelBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", 
            "player-statistics.xlsx");
    }

    /// <summary>
    /// Generate transfer market data in Excel format
    /// </summary>
    [HttpPost("transfer-market/excel")]
    public async Task<IActionResult> GenerateTransferMarketExcel(
        [FromBody] TransferMarketFilter filter)
    {
        var excelBytes = await _reportService.GenerateTransferMarketExcelAsync(filter);
        
        return File(excelBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", 
            "transfer-market.xlsx");
    }

    /// <summary>
    /// Generate scouting presentation in PowerPoint format
    /// </summary>
    [HttpPost("player/{playerId}/presentation")]
    public async Task<IActionResult> GenerateScoutingPresentation(
        int playerId,
        [FromBody] PresentationOptions? options = null)
    {
        var presentationOptions = options ?? new PresentationOptions();
        var pptBytes = await _reportService.GenerateScoutingPresentationAsync(playerId, presentationOptions);
        
        return File(pptBytes, "application/vnd.openxmlformats-officedocument.presentationml.presentation", 
            $"scouting-presentation-{playerId}.pptx");
    }

    /// <summary>
    /// Get available report templates
    /// </summary>
    [HttpGet("templates")]
    public async Task<ActionResult<List<ReportTemplate>>> GetTemplates(
        [FromQuery] ReportType? type = null)
    {
        var templates = type.HasValue
            ? await _reportService.GetAvailableTemplatesAsync(type.Value)
            : await _reportService.GetAvailableTemplatesAsync(ReportType.Custom);
        
        return Ok(templates);
    }

    /// <summary>
    /// Create custom report template
    /// </summary>
    [HttpPost("templates")]
    public async Task<ActionResult<string>> CreateTemplate([FromBody] ReportTemplate template)
    {
        var templateId = await _reportService.CreateCustomTemplateAsync(template);
        return Ok(new { templateId });
    }

    /// <summary>
    /// Schedule a recurring report
    /// </summary>
    [HttpPost("schedule")]
    public async Task<ActionResult<string>> ScheduleReport([FromBody] ScheduledReport report)
    {
        var userId = User.FindFirst("sub")?.Value ?? User.Identity?.Name ?? "";
        report.UserId = userId;
        
        var scheduleId = await _reportService.ScheduleReportAsync(report);
        return Ok(new { scheduleId });
    }

    /// <summary>
    /// Get scheduled reports
    /// </summary>
    [HttpGet("schedule")]
    public async Task<ActionResult<List<ScheduledReport>>> GetScheduledReports()
    {
        var userId = User.FindFirst("sub")?.Value ?? User.Identity?.Name ?? "";
        var schedules = await _reportService.GetScheduledReportsAsync(userId);
        return Ok(schedules);
    }

    /// <summary>
    /// Cancel scheduled report
    /// </summary>
    [HttpDelete("schedule/{scheduleId}")]
    public async Task<IActionResult> CancelScheduledReport(string scheduleId)
    {
        await _reportService.CancelScheduledReportAsync(scheduleId);
        return NoContent();
    }

    /// <summary>
    /// Get report generation history
    /// </summary>
    [HttpGet("history")]
    public async Task<ActionResult<List<GeneratedReport>>> GetReportHistory(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var userId = User.FindFirst("sub")?.Value ?? User.Identity?.Name ?? "";
        var history = await _reportService.GetReportHistoryAsync(userId, page, pageSize);
        return Ok(history);
    }

    /// <summary>
    /// Download historical report
    /// </summary>
    [HttpGet("history/{reportId}/download")]
    public async Task<IActionResult> DownloadHistoricalReport(string reportId)
    {
        var reportBytes = await _reportService.DownloadHistoricalReportAsync(reportId);
        return File(reportBytes, "application/octet-stream", $"report-{reportId}.pdf");
    }

    /// <summary>
    /// Get report usage statistics
    /// </summary>
    [HttpGet("stats")]
    public async Task<ActionResult<ReportUsageStats>> GetReportStats(
        [FromQuery] DateTime? from = null,
        [FromQuery] DateTime? to = null)
    {
        var userId = User.FindFirst("sub")?.Value ?? User.Identity?.Name ?? "";
        var fromDate = from ?? DateTime.UtcNow.AddMonths(-1);
        var toDate = to ?? DateTime.UtcNow;
        
        var stats = await _analyticsService.GetReportUsageStatsAsync(userId, fromDate, toDate);
        return Ok(stats);
    }
}

public class PlayerStatsExcelRequest
{
    public List<int> PlayerIds { get; set; } = new();
    public ExcelReportOptions? Options { get; set; }
}

