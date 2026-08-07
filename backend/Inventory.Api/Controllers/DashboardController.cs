using Inventory.Api.Data;
using Inventory.Api.Dtos;
using Inventory.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;

namespace Inventory.Api.Controllers;

[ApiController]
[Route("api/dashboard")]
[Authorize]
public class DashboardController(InventoryDbContext db) : ControllerBase
{
    [HttpGet("statistics")]
    [EnableRateLimiting("SensitiveActions")]
    public async Task<ActionResult<IEnumerable<DashboardStatisticsDto>>> Statistics(
        [FromQuery] AvailabilityStatus? status,
        [FromQuery] ItemCondition? condition)
    {
        var query = db.Items.Include(i => i.ItemType).AsQueryable();

        if (status is not null)
        {
            query = query.Where(i => i.AvailabilityStatus == status);
        }

        if (condition is not null)
        {
            query = query.Where(i => i.Condition == condition);
        }

        var grouped = await query
            .GroupBy(i => new { i.ItemTypeId, i.ItemType!.Name })
            .Select(g => new { g.Key.Name, Count = g.Count() })
            .OrderByDescending(g => g.Count)
            .ToListAsync();

        var statistics = grouped.Select(g => new DashboardStatisticsDto(g.Name, g.Count));

        return Ok(statistics);
    }
}
