using Inventory.Api.Data;
using Inventory.Api.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Inventory.Api.Controllers;

[ApiController]
[Route("api/loans")]
[Authorize]
public class LoansController(InventoryDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<LoanDto>>> List()
    {
        var loans = await db.Loans
            .Include(e => e.Item).ThenInclude(i => i!.ItemType)
            .Include(e => e.RegisteredByUser)
            .OrderByDescending(e => e.CheckoutDate)
            .Select(e => new LoanDto(
                e.Id,
                e.ItemId,
                e.Item!.ModelBrand,
                e.Item.ItemType!.Name,
                e.Item.PhotoUrl,
                e.CheckoutDate,
                e.ReturnDueDate,
                e.Technician,
                e.BorrowedBy,
                e.ReturnedAt,
                e.RegisteredByUser!.FullName))
            .ToListAsync();

        return Ok(loans);
    }
}
