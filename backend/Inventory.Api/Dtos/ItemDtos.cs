using Inventory.Api.Models;
using Microsoft.AspNetCore.Http;

namespace Inventory.Api.Dtos;

public record LoanInfoDto(DateTime? ReturnDueDate, string Technician, string BorrowedBy);

public record ItemDto(
    int Id,
    double? AssetNumber,
    string? PhotoUrl,
    int ItemTypeId,
    string ItemTypeName,
    string ModelBrand,
    string? AdditionalInfo,
    AvailabilityStatus AvailabilityStatus,
    ItemCondition Condition,
    DateTime CreatedAt,
    string CreatedByName,
    LoanInfoDto? ActiveLoan);

public class ItemFormDto
{
    public double? AssetNumber { get; set; }
    public IFormFile? Photo { get; set; }
    public int ItemTypeId { get; set; }
    public required string ModelBrand { get; set; }
    public string? AdditionalInfo { get; set; }
    public AvailabilityStatus AvailabilityStatus { get; set; }
    public ItemCondition Condition { get; set; }

    // Only used when AvailabilityStatus == Loaned
    public DateTime? ReturnDueDate { get; set; }
    public string? Technician { get; set; }
    public string? BorrowedBy { get; set; }
}
