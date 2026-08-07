namespace Inventory.Api.Models;

public class User
{
    public int Id { get; set; }
    public required string Username { get; set; }
    public required string FullName { get; set; }
    public required string PasswordHash { get; set; }
    public bool IsAdmin { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
