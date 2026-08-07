namespace Inventory.Api.Models;

// Represents an "outbound" record: an item on loan, who took it, who it's for, and when it's due back.
public class Loan
{
    public int Id { get; set; }

    public int ItemId { get; set; }
    public Item? Item { get; set; }

    public DateTime CheckoutDate { get; set; } = DateTime.UtcNow;
    public DateTime? ReturnDueDate { get; set; }
    public required string Technician { get; set; }
    public required string BorrowedBy { get; set; }

    public DateTime? ReturnedAt { get; set; }

    public int RegisteredByUserId { get; set; }
    public User? RegisteredByUser { get; set; }
}
