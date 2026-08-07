using Inventory.Api.Data;
using Inventory.Api.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Inventory.Api.Controllers;

[ApiController]
[Route("api/item-types")]
[Authorize]
public class ItemTypesController(InventoryDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ItemTypeDto>>> List()
    {
        var types = await db.ItemTypes
            .OrderBy(t => t.Name)
            .Select(t => new ItemTypeDto(t.Id, t.Name))
            .ToListAsync();

        return Ok(types);
    }
}
