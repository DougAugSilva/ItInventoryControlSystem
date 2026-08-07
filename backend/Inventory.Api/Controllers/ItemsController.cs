using System.Security.Claims;
using Inventory.Api.Data;
using Inventory.Api.Dtos;
using Inventory.Api.Models;
using Inventory.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;

namespace Inventory.Api.Controllers;

[ApiController]
[Route("api/items")]
[Authorize]
public class ItemsController(InventoryDbContext db, PhotoStorageService photoStorage) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ItemDto>>> List(
        [FromQuery] string? search,
        [FromQuery] int? itemTypeId,
        [FromQuery] AvailabilityStatus? status,
        [FromQuery] ItemCondition? condition,
        [FromQuery] string? sort)
    {
        var query = db.Items
            .Include(i => i.ItemType)
            .Include(i => i.CreatedByUser)
            .Include(i => i.Loans)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(i =>
                i.ModelBrand.Contains(term) ||
                (i.AdditionalInfo != null && i.AdditionalInfo.Contains(term)) ||
                (i.ItemType != null && i.ItemType.Name.Contains(term)) ||
                (i.AssetNumber != null && i.AssetNumber.ToString()!.Contains(term)));
        }

        if (itemTypeId is not null)
        {
            query = query.Where(i => i.ItemTypeId == itemTypeId);
        }

        if (status is not null)
        {
            query = query.Where(i => i.AvailabilityStatus == status);
        }

        if (condition is not null)
        {
            query = query.Where(i => i.Condition == condition);
        }

        query = sort == "recent"
            ? query.OrderByDescending(i => i.CreatedAt)
            : query.OrderBy(i => i.ModelBrand);

        var items = await query.ToListAsync();
        return Ok(items.Select(MapToDto));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ItemDto>> GetById(int id)
    {
        var item = await db.Items
            .Include(i => i.ItemType)
            .Include(i => i.CreatedByUser)
            .Include(i => i.Loans)
            .FirstOrDefaultAsync(i => i.Id == id);

        return item is null ? NotFound() : Ok(MapToDto(item));
    }

    [HttpPost]
    public async Task<ActionResult<ItemDto>> Create([FromForm] ItemFormDto form)
    {
        var validationError = await ValidateAsync(form, currentCheckoutDate: null);
        if (validationError is not null)
        {
            return BadRequest(new { message = validationError });
        }

        var item = new Item
        {
            AssetNumber = form.AssetNumber,
            ItemTypeId = form.ItemTypeId,
            ModelBrand = form.ModelBrand,
            AdditionalInfo = form.AdditionalInfo,
            AvailabilityStatus = form.AvailabilityStatus,
            Condition = form.Condition,
            CreatedByUserId = CurrentUserId(),
        };

        if (form.Photo is not null)
        {
            try
            {
                item.PhotoUrl = await photoStorage.SaveAsync(form.Photo);
            }
            catch (InvalidPhotoException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        db.Items.Add(item);
        await db.SaveChangesAsync();

        if (form.AvailabilityStatus == AvailabilityStatus.Loaned)
        {
            db.Loans.Add(NewLoan(item.Id, form));
            await db.SaveChangesAsync();
        }

        return await GetById(item.Id);
    }

    [HttpPut("{id:int}")]
    [EnableRateLimiting("SensitiveActions")]
    public async Task<ActionResult<ItemDto>> Update(int id, [FromForm] ItemFormDto form)
    {
        var item = await db.Items.Include(i => i.Loans).FirstOrDefaultAsync(i => i.Id == id);
        if (item is null)
        {
            return NotFound();
        }

        var currentOpenLoan = item.Loans.FirstOrDefault(e => e.ReturnedAt == null);

        var validationError = await ValidateAsync(form, currentCheckoutDate: currentOpenLoan?.CheckoutDate);
        if (validationError is not null)
        {
            return BadRequest(new { message = validationError });
        }

        var previousStatus = item.AvailabilityStatus;

        item.AssetNumber = form.AssetNumber;
        item.ItemTypeId = form.ItemTypeId;
        item.ModelBrand = form.ModelBrand;
        item.AdditionalInfo = form.AdditionalInfo;
        item.AvailabilityStatus = form.AvailabilityStatus;
        item.Condition = form.Condition;
        item.UpdatedAt = DateTime.UtcNow;

        if (form.Photo is not null)
        {
            try
            {
                item.PhotoUrl = await photoStorage.SaveAsync(form.Photo);
            }
            catch (InvalidPhotoException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        if (form.AvailabilityStatus == AvailabilityStatus.Loaned)
        {
            if (currentOpenLoan is not null)
            {
                currentOpenLoan.ReturnDueDate = form.ReturnDueDate;
                currentOpenLoan.Technician = form.Technician!;
                currentOpenLoan.BorrowedBy = form.BorrowedBy!;
            }
            else
            {
                db.Loans.Add(NewLoan(item.Id, form));
            }
        }
        else if (previousStatus == AvailabilityStatus.Loaned && currentOpenLoan is not null)
        {
            currentOpenLoan.ReturnedAt = DateTime.UtcNow;
        }

        await db.SaveChangesAsync();
        return await GetById(item.Id);
    }

    private Loan NewLoan(int itemId, ItemFormDto form) => new()
    {
        ItemId = itemId,
        ReturnDueDate = form.ReturnDueDate,
        Technician = form.Technician!,
        BorrowedBy = form.BorrowedBy!,
        RegisteredByUserId = CurrentUserId(),
    };

    private async Task<string?> ValidateAsync(ItemFormDto form, DateTime? currentCheckoutDate)
    {
        if (!await db.ItemTypes.AnyAsync(t => t.Id == form.ItemTypeId))
        {
            return "Invalid item type.";
        }

        if (string.IsNullOrWhiteSpace(form.ModelBrand))
        {
            return "Model and brand are required.";
        }

        if (form.AvailabilityStatus == AvailabilityStatus.Loaned &&
            (string.IsNullOrWhiteSpace(form.Technician) || string.IsNullOrWhiteSpace(form.BorrowedBy)))
        {
            return "For loaned items, inform the responsible technician and who it was loaned to.";
        }

        if (form.AvailabilityStatus == AvailabilityStatus.Loaned && form.ReturnDueDate is not null)
        {
            var checkoutDate = currentCheckoutDate ?? DateTime.UtcNow;
            if (form.ReturnDueDate.Value.Date < checkoutDate.Date)
            {
                return "The return due date cannot be earlier than the date the item was loaned out.";
            }
        }

        return null;
    }

    private int CurrentUserId() => int.Parse(User.FindFirstValue("uid")!);

    private static ItemDto MapToDto(Item item)
    {
        var openLoan = item.Loans?.FirstOrDefault(e => e.ReturnedAt == null);

        return new ItemDto(
            item.Id,
            item.AssetNumber,
            item.PhotoUrl,
            item.ItemTypeId,
            item.ItemType?.Name ?? string.Empty,
            item.ModelBrand,
            item.AdditionalInfo,
            item.AvailabilityStatus,
            item.Condition,
            item.CreatedAt,
            item.CreatedByUser?.FullName ?? string.Empty,
            openLoan is null
                ? null
                : new LoanInfoDto(openLoan.ReturnDueDate, openLoan.Technician, openLoan.BorrowedBy));
    }
}
