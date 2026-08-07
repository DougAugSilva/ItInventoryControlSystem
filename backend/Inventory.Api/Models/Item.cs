namespace Inventory.Api.Models;

public class Item
{
    public int Id { get; set; }

    // Nullable because some items don't have an asset tag yet.
    public double? AssetNumber { get; set; }

    public string? PhotoUrl { get; set; }

    public int ItemTypeId { get; set; }
    public ItemType? ItemType { get; set; }

    public required string ModelBrand { get; set; }
    public string? AdditionalInfo { get; set; }

    public AvailabilityStatus AvailabilityStatus { get; set; } = AvailabilityStatus.Available;
    public ItemCondition Condition { get; set; } = ItemCondition.New;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public int CreatedByUserId { get; set; }
    public User? CreatedByUser { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public ICollection<Loan> Loans { get; set; } = [];
}
