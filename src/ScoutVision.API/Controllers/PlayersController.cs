using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ScoutVision.Core.Entities;
using ScoutVision.Infrastructure.Data;
using Swashbuckle.AspNetCore.Annotations;

namespace ScoutVision.API.Controllers;

using Microsoft.AspNetCore.Authorization;

[ApiController]
[Route("api/[controller]")]
public class PlayersController : ControllerBase
{
    private readonly ScoutVisionDbContext _context;

    public PlayersController(ScoutVisionDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    [Authorize(Roles = "Admin,Coach,Analyst,Club")]
    [SwaggerOperation(Summary = "Get all players", Description = "Retrieves a paginated list of all players")]
    public async Task<ActionResult<IEnumerable<Player>>> GetPlayers(
        [FromQuery] int page = 1, 
        [FromQuery] int pageSize = 10, 
        [FromQuery] string? search = null)
    {
        var query = _context.Players
            .Include(p => p.ContactInfo)
            .Include(p => p.Tags).ThenInclude(pt => pt.Tag)
            .AsQueryable();

        if (!string.IsNullOrEmpty(search))
        {
            query = query.Where(p => p.FirstName.Contains(search) || 
                                   p.LastName.Contains(search) || 
                                   p.CurrentTeam.Contains(search));
        }

        var totalCount = await query.CountAsync();
        var players = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        Response.Headers.Append("X-Total-Count", totalCount.ToString());
        return Ok(players);
    }

    [HttpGet("{id}")]
    [Authorize(Roles = "Admin,Coach,Analyst,Club")]
    [SwaggerOperation(Summary = "Get player by ID", Description = "Retrieves a specific player with all related data")]
    public async Task<ActionResult<Player>> GetPlayer(int id)
    {
        var player = await _context.Players
            .Include(p => p.ContactInfo)
            .Include(p => p.PerformanceMetrics)
            .Include(p => p.VideoAnalyses).ThenInclude(va => va.Timestamps)
            .Include(p => p.ScoutingReports)
            .Include(p => p.TalentPredictions)
            .Include(p => p.MindsetProfiles)
            .Include(p => p.Tags).ThenInclude(pt => pt.Tag)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (player == null)
        {
            return NotFound();
        }

        return player;
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Coach")]
    [SwaggerOperation(Summary = "Create new player", Description = "Creates a new player profile")]
    public async Task<ActionResult<Player>> PostPlayer(Player player)
    {
    player.CreatedAt = DateTime.UtcNow;
    player.CreatedBy = User?.Identity?.Name ?? "Unknown";
        
        _context.Players.Add(player);
        await _context.SaveChangesAsync();

        return CreatedAtAction("GetPlayer", new { id = player.Id }, player);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin,Coach")]
    [SwaggerOperation(Summary = "Update player", Description = "Updates an existing player profile")]
    public async Task<IActionResult> PutPlayer(int id, Player player)
    {
        if (id != player.Id)
        {
            return BadRequest();
        }

    player.UpdatedAt = DateTime.UtcNow;
    player.UpdatedBy = User?.Identity?.Name ?? "Unknown";

        _context.Entry(player).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!PlayerExists(id))
            {
                return NotFound();
            }
            else
            {
                throw;
            }
        }

        return NoContent();
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    [SwaggerOperation(Summary = "Delete player", Description = "Soft deletes a player profile")]
    public async Task<IActionResult> DeletePlayer(int id)
    {
        var player = await _context.Players.FindAsync(id);
        if (player == null)
        {
            return NotFound();
        }

        player.IsDeleted = true;
        player.UpdatedAt = DateTime.UtcNow;
    player.UpdatedBy = User?.Identity?.Name ?? "Unknown";
        
        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpGet("{id}/dashboard")]
    [Authorize(Roles = "Admin,Coach,Analyst,Club")]
    [SwaggerOperation(Summary = "Get player dashboard data", Description = "Gets comprehensive dashboard data for a player")]
    public async Task<ActionResult<object>> GetPlayerDashboard(int id)
    {
        var player = await _context.Players
            .Include(p => p.PerformanceMetrics)
            .Include(p => p.VideoAnalyses)
            .Include(p => p.ScoutingReports)
            .Include(p => p.TalentPredictions)
            .Include(p => p.MindsetProfiles)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (player == null)
        {
            return NotFound();
        }

        var dashboard = new
        {
            Player = new
            {
                player.Id,
                player.FullName,
                player.Age,
                player.Position,
                player.CurrentTeam,
                player.Status,
                player.Priority
            },
            Stats = new
            {
                TotalVideos = player.VideoAnalyses.Count,
                TotalReports = player.ScoutingReports.Count,
                TotalMetrics = player.PerformanceMetrics.Count,
                LatestPrediction = player.TalentPredictions.OrderByDescending(tp => tp.PredictionDate).FirstOrDefault(),
                LatestMindsetScore = player.MindsetProfiles.OrderByDescending(mp => mp.AssessmentDate).FirstOrDefault()?.OverallMindsetScore,
                AverageScoutRating = player.ScoutingReports.Any() ? player.ScoutingReports.Average(sr => sr.OverallRating) : 0
            },
            RecentActivity = new
            {
                LatestVideo = player.VideoAnalyses.OrderByDescending(va => va.AnalyzedAt).FirstOrDefault(),
                LatestReport = player.ScoutingReports.OrderByDescending(sr => sr.ReportDate).FirstOrDefault(),
                RecentMetrics = player.PerformanceMetrics.OrderByDescending(pm => pm.MeasuredAt).Take(5).ToList()
            }
        };

        return Ok(dashboard);
    }

    private bool PlayerExists(int id)
    {
        return _context.Players.Any(e => e.Id == id);
    }
}