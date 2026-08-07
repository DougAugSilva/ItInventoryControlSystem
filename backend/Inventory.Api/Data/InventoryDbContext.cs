using Inventory.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Inventory.Api.Data;

public class InventoryDbContext(DbContextOptions<InventoryDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<ItemType> ItemTypes => Set<ItemType>();
    public DbSet<Item> Items => Set<Item>();
    public DbSet<Loan> Loans => Set<Loan>();

    // Fixed catalog of item types offered when registering a new item.
    public static readonly string[] DefaultItemTypes =
    [
        "Phone",
        "Phone Base",
        "Mono Headset",
        "Stereo Headset",
        "Mobile Phone",
        "HDMI Cables",
        "VGA Cables",
        "Wired Keyboard",
        "Wireless Keyboard",
        "Wired Mouse",
        "Wireless Mouse",
        "Wired Earphones",
        "Wireless Earphones",
        "HDMI-to-VGA Adapters",
        "DisplayPort Adapters",
        "Desktop Workstation",
        "Monitors",
        "Monitor Stand",
        "Laptop Stand",
        "Headphone Foam Pads",
        "Laptops",
    ];

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasIndex(u => u.Username).IsUnique();
            entity.Property(u => u.Username).HasMaxLength(50);
            entity.Property(u => u.FullName).HasMaxLength(100);
        });

        modelBuilder.Entity<ItemType>(entity =>
        {
            entity.HasIndex(t => t.Name).IsUnique();
            entity.Property(t => t.Name).HasMaxLength(50);

            entity.HasData(DefaultItemTypes.Select((name, index) => new ItemType
            {
                Id = index + 1,
                Name = name,
            }));
        });

        modelBuilder.Entity<Item>(entity =>
        {
            entity.Property(i => i.ModelBrand).HasMaxLength(100);
            entity.Property(i => i.AdditionalInfo).HasMaxLength(200);
            entity.Property(i => i.AssetNumber).HasColumnType("float");

            entity.HasOne(i => i.ItemType)
                .WithMany(t => t.Items)
                .HasForeignKey(i => i.ItemTypeId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(i => i.CreatedByUser)
                .WithMany()
                .HasForeignKey(i => i.CreatedByUserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Loan>(entity =>
        {
            entity.Property(e => e.Technician).HasMaxLength(100);
            entity.Property(e => e.BorrowedBy).HasMaxLength(100);

            entity.HasOne(e => e.Item)
                .WithMany(i => i.Loans)
                .HasForeignKey(e => e.ItemId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.RegisteredByUser)
                .WithMany()
                .HasForeignKey(e => e.RegisteredByUserId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
