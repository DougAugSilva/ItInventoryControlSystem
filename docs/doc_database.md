# Database

SQL Server, with the schema defined **Code First** by Entity Framework Core — meaning the source
of truth for the schema is the classes in `backend/Inventory.Api/Models/` plus the migrations
generated from them, not a hand-written SQL script.

## Where SQL Server actually runs today

The project has a `docker/docker-compose.yml` ready to bring up SQL Server in a container — that
is the intended setup for the project (see `docs/doc_docker.md`). **But, in the current
development environment, the API points to a SQL Server instance installed natively on this
machine's Windows**, not the container. This works for development, but it's a configuration
specific to this machine (port, authentication mode, `sa` password) that doesn't exist anywhere
else — including the production Linux server. Details on why this matters in
[`doc_docker.md`](doc_docker.md).

The connection string itself (`ConnectionStrings:DefaultConnection`) is never versioned — it
lives in the backend's user-secrets (see `docs/doc_backend.md`).

## Tables

| Table | Model | What it represents |
|---|---|---|
| `Users` | `User.cs` | System login. `Username` has a unique index; `IsAdmin` controls access to user management; `PasswordHash` stores a BCrypt hash, never the plain-text password |
| `ItemTypes` | `ItemType.cs` | Fixed list of item types (Phone, Laptop, etc.). `Name` has a unique index. Populated via a migration seed (`HasData`), not manual registration |
| `Items` | `Item.cs` | An inventory item: asset number, photo, type, model/brand, availability status, and condition |
| `Loans` | `Loan.cs` | An "outbound" record: item loaned out, by whom (`RegisteredByUserId`), to whom (`BorrowedBy`), when it goes out and when it's due back (`ReturnedAt` is null while still on loan) |

Relationships (all configured in `InventoryDbContext.OnModelCreating`):
- `Item → ItemType` (N:1, `DeleteBehavior.Restrict` — won't allow deleting a type that has items)
- `Item → User` (N:1, who registered the item, `Restrict`)
- `Loan → Item` (N:1, `DeleteBehavior.Cascade` — deleting the item deletes its loan history)
- `Loan → User` (N:1, who registered the checkout, `Restrict`)

## Migrations

Live in `backend/Inventory.Api/Data/Migrations/`:

1. `InitialCreate` — creates the 4 tables above, the unique indexes (`ItemTypes.Name`,
   `Users.Username`) and seeds the 21 fixed item types.

### Creating a new migration

Run from inside `backend/Inventory.Api`:

```bash
dotnet ef migrations add MigrationName
dotnet ef database update   # only if you want to apply it manually; in dev, Program.cs already applies it automatically
```

`InventoryDbContextFactory.cs` exists just for this: it lets the `dotnet ef` tool discover the
`DbContext` without needing to initialize the whole host (Kestrel, DI, etc.) — without it, the
`dotnet ef migrations add` command used to take minutes before timing out.

## `database/schema.sql`

This is an **export generated from the migrations**, kept only as a quick-reference copy of the
schema (see `database/README.md`) — it's never applied directly against a database. It reflects
the state of the migrations at the time it was generated; if new migrations are created, this
file can become stale. If you need an up-to-date schema, prefer regenerating it from the
migrations rather than hand-editing this file.
