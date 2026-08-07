using System.Text.Json;
using Inventory.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Inventory.Api.Data;

public static class DataSeeder
{
    // Seed user passwords are never hardcoded in source (that would leak a real admin
    // account's password to anyone with repository access). They come from configuration
    // (user-secrets in dev; environment variable in other environments) — see
    // "Seed:SenhaUsuarioTeste" and "Seed:SenhaAdmin" in docs/doc_backend.md.
    public static async Task SeedTestUserAsync(InventoryDbContext db, IConfiguration configuration)
    {
        var alreadyExists = await db.Users.AnyAsync(u => u.Username == "john.doe");
        if (alreadyExists)
        {
            return;
        }

        var password = configuration["Seed:SenhaUsuarioTeste"]
            ?? throw new InvalidOperationException("Seed:SenhaUsuarioTeste not configured.");

        db.Users.Add(new User
        {
            Username = "john.doe",
            FullName = "John Doe",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
        });

        await db.SaveChangesAsync();
    }

    public static async Task SeedAdminUserAsync(InventoryDbContext db, IConfiguration configuration)
    {
        var alreadyExists = await db.Users.AnyAsync(u => u.Username == "admin.besttechti");
        if (alreadyExists)
        {
            return;
        }

        var password = configuration["Seed:SenhaAdmin"]
            ?? throw new InvalidOperationException("Seed:SenhaAdmin not configured.");

        db.Users.Add(new User
        {
            Username = "admin.besttechti",
            FullName = "Administrador BestTechTI",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
            IsAdmin = true,
        });

        await db.SaveChangesAsync();
    }

    // Imports the local seed file (user-provided test data) on first run. Idempotent: does
    // nothing if any item already exists.
    public static async Task SeedTestItemsAsync(InventoryDbContext db, string jsonPath)
    {
        if (await db.Items.AnyAsync())
        {
            return;
        }

        if (!File.Exists(jsonPath))
        {
            return;
        }

        var user = await db.Users.FirstAsync(u => u.Username == "john.doe");
        var types = await db.ItemTypes.ToListAsync();

        var json = await File.ReadAllTextAsync(jsonPath);
        var itemsJson = JsonSerializer.Deserialize<List<SeedItemJson>>(json)
            ?? throw new InvalidOperationException("Seed JSON file is empty or invalid.");

        foreach (var itemJson in itemsJson)
        {
            var itemType = FindItemType(types, itemJson.ItemType);
            if (itemType is null)
            {
                continue;
            }

            var item = new Item
            {
                AssetNumber = double.TryParse(itemJson.AssetNumber, out var number) ? number : null,
                PhotoUrl = itemJson.Photo.Replace('\\', '/'),
                ItemTypeId = itemType.Id,
                ModelBrand = itemJson.ModelBrand,
                AdditionalInfo = itemJson.AdditionalInfo,
                Condition = MapCondition(itemJson.Condition),
                CreatedByUserId = user.Id,
            };

            var loan = MapAvailability(itemJson.Availability, item, user.Id);

            db.Items.Add(item);
            await db.SaveChangesAsync();

            if (loan is not null)
            {
                loan.ItemId = item.Id;
                db.Loans.Add(loan);
            }
        }

        await db.SaveChangesAsync();
    }

    private static ItemType? FindItemType(List<ItemType> types, string name)
    {
        var target = name.Trim();

        var found = types.FirstOrDefault(t => string.Equals(t.Name, target, StringComparison.OrdinalIgnoreCase));
        if (found is not null) return found;

        // Tolerates singular/plural mismatches between the seed JSON and the fixed catalog
        // (e.g. "Notebook" in the JSON vs "Laptops" in the catalog).
        found = types.FirstOrDefault(t => string.Equals(t.Name, target + "s", StringComparison.OrdinalIgnoreCase));
        if (found is not null) return found;

        return types.FirstOrDefault(t =>
            target.EndsWith('s') && string.Equals(t.Name, target[..^1], StringComparison.OrdinalIgnoreCase));
    }

    // The seed JSON's "estado" values are in Portuguese (matches the local, gitignored data file).
    private static ItemCondition MapCondition(string condition) => condition.Trim().ToLowerInvariant() switch
    {
        "novo" => ItemCondition.New,
        "usado" => ItemCondition.Used,
        "com defeito" => ItemCondition.Defective,
        "quebrado" => ItemCondition.Broken,
        _ => ItemCondition.Used,
    };

    // The seed JSON's "disponibilidade" values are in Portuguese (matches the local, gitignored data file).
    private static Loan? MapAvailability(string availability, Item item, int userId)
    {
        var text = availability.Trim();

        if (text.StartsWith("emprestado", StringComparison.OrdinalIgnoreCase))
        {
            item.AvailabilityStatus = AvailabilityStatus.Loaned;

            var detail = text.Contains(':') ? text[(text.IndexOf(':') + 1)..].Trim() : string.Empty;
            var parts = detail.Split(" para ", 2, StringSplitOptions.TrimEntries);

            return new Loan
            {
                Technician = parts.ElementAtOrDefault(0) ?? "Not informed",
                BorrowedBy = parts.ElementAtOrDefault(1) ?? "Not informed",
                RegisteredByUserId = userId,
            };
        }

        item.AvailabilityStatus = text.Equals("indisponivel", StringComparison.OrdinalIgnoreCase)
            ? AvailabilityStatus.Unavailable
            : AvailabilityStatus.Available;

        return null;
    }
}
