# Backend

REST API in **ASP.NET Core 10** (`backend/Inventory.Api`), with **Entity Framework Core** on top
of SQL Server, **JWT** authentication, and photo uploads to disk. It's the only source of access
to the database — the frontend never talks to SQL Server directly.

For anything specifically about security (password policy, rate limiting, HTTPS, risk/fix
history), see [`doc_security.md`](doc_security.md).

## Stack

- .NET 10 / ASP.NET Core Web API (`Microsoft.NET.Sdk.Web`)
- Entity Framework Core 10 + `Microsoft.EntityFrameworkCore.SqlServer` (Code First)
- `Microsoft.AspNetCore.Authentication.JwtBearer` to validate the token on requests
- `BCrypt.Net-Next` for password hashing
- OpenAPI (`Microsoft.AspNetCore.OpenApi` + `Microsoft.OpenApi`) — spec exposed at
  `/openapi/v1.json` only in the Development environment (see `Program.cs`)

## Running it

```bash
cd backend/Inventory.Api
dotnet run    # uses the "https" profile (default) from launchSettings.json:
              # https://localhost:5443 and http://localhost:5080 (redirects to https)
```

The first time, generate the local HTTPS development certificate (if it doesn't exist yet):

```bash
dotnet dev-certs https --trust
```

Requires a SQL Server connection string, the JWT key, and the seed users' passwords, configured
via [user-secrets](https://learn.microsoft.com/aspnet/core/security/app-secrets) — never
versioned, each environment (each dev machine) has its own:

```bash
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=...;Database=InventoryDb;User Id=sa;Password=...;TrustServerCertificate=True"
dotnet user-secrets set "Jwt:Key" "..."
dotnet user-secrets set "Jwt:Issuer" "InventoryApi"
dotnet user-secrets set "Jwt:Audience" "InventoryFrontend"
dotnet user-secrets set "Seed:SenhaAdmin" "..."            # password for the admin.besttechti user
dotnet user-secrets set "Seed:SenhaUsuarioTeste" "..."     # password for the john.doe user
```

The last two exist so the real password for those accounts never ends up written in plain text
in the source code (`DataSeeder.cs`) — without them configured, the application fails to start
instead of seeding an insecure default password.

In the Development environment (`Program.cs`), the application runs `Database.MigrateAsync()`
automatically on startup and seeds test data (`DataSeeder`) — no need to run migrations manually
to develop locally.

**Important if developing in WSL:** run `dotnet` from inside WSL, on the native Linux path
(`/home/.../ItInventoryControl`), never through the network path `\\wsl.localhost\...` from
Windows' `dotnet.exe`. Every file the Windows `dotnet.exe` reads/writes on that path goes through
the 9P network protocol that bridges Windows↔WSL2 — orders of magnitude slower than local access
for workloads with lots of small files (MSBuild, NuGet restore). Builds that take seconds running
natively can take minutes or hang when run over the network path.

## Folder structure

```
Inventory.Api/
├── Controllers/    # HTTP endpoints
├── Data/           # DbContext, migrations, test data seeding
├── Dtos/           # Request/response records (never expose Models directly)
├── Models/         # EF Core entities
├── Services/       # Rules that aren't purely HTTP (JWT, photo storage, password policy)
└── Program.cs      # Bootstrap: DI, authentication, CORS, rate limiting, static files, migrations+seed
```

## Authentication and authorization

- `AuthController.Login` validates username/password (BCrypt hash) and returns a JWT
  (`JwtService`). The token carries `sub` (username), the custom claim `uid` (numeric user id),
  and a `role` claim (`Admin` or `User`).
- `Program.cs` sets `options.MapInboundClaims = false` — this prevents the `JwtBearerHandler`
  from automatically remapping short claims (`sub` → a WS-* claim URI), which would break reading
  `uid` in the controllers. If a custom claim ever "disappears" when reading
  `User.FindFirstValue(...)`, this is the first place to check.
- Most controllers use plain `[Authorize]` (any logged-in user). `UsersController` uses
  `[Authorize(Roles = "Admin")]` — only administrators manage users. On the frontend, the same
  rule is mirrored in `AdminRoute.jsx`, but the controller is what actually enforces security
  (the frontend just avoids exposing the UI needlessly).
- `CurrentUserId()` (repeated in `ItemsController` and `UsersController`) reads the `uid` claim
  to know who is making the request — used to record who registered an item or to prevent
  someone from removing their own account.

## Controllers

| Controller | Route | Access | Rate limit | Summary |
|---|---|---|---|---|
| `AuthController` | `/api/auth` | `login` public, `me` authenticated | `login`: 15/min per IP | Login (issues a JWT) and current session data |
| `ItemsController` | `/api/items` | Authenticated | `PUT`: 15/min per user | CRUD for inventory items; search with filters (type, status, condition, text); switching status to "Loaned" opens a `Loan`, switching back closes it (`ReturnedAt`) |
| `LoansController` | `/api/loans` | Authenticated | — | List of outbound loans (loan history), most recent first — feeds the Loans page |
| `ItemTypesController` | `/api/item-types` | Authenticated | — | Fixed list of item types (seeded in `InventoryDbContext.DefaultItemTypes`) |
| `DashboardController` | `/api/dashboard/statistics` | Authenticated | 15/min per user | Item count grouped by type, with optional status/condition filters |
| `UsersController` | `/api/users` | Admin only | `PUT`/`DELETE`: 15/min per user | User CRUD; blocks editing/removing the `admin.besttechti` account, prevents duplicate usernames, and enforces the password policy (see `doc_security.md`) |

Rate limiting uses ASP.NET Core's built-in middleware (`Microsoft.AspNetCore.RateLimiting`,
configured in `Program.cs`) — exceeding the limit responds with `429 Too Many Requests`.

## Photo upload

`PhotoStorageService` validates the size (up to 10MB) and the **real file type via its binary
signature** (magic bytes — `FF D8 FF` for JPEG, `89 50 4E 47 0D 0A 1A 0A` for PNG), not the
`Content-Type` the browser sends (that's just a client-side claim, easy to spoof). If the photo
doesn't pass validation, `ItemsController` responds `400` with the error message
(`InvalidPhotoException`). Valid files are written to `wwwroot/uploads/itens/<guid>.<ext>`
(gitignored folder — uploaded photos aren't versioned). The relative URL (`/uploads/itens/...`)
is saved in `Item.PhotoUrl` and served statically by `Program.cs` via `app.UseStaticFiles()`.

*Sample* photos for test items and the site's design images (logos, wallpaper) live in `img/` at
the repository root (outside `backend/`) and are served separately by a second
`UseStaticFiles` pointed there, mounted at `/img`.

## CORS

`Program.cs` defines a named policy (`FrontendCorsPolicy`) that only allows the origins listed in
`Cors:AllowedOrigins` in `appsettings.json` (today, just `http://localhost:5173`, the Vite dev
server). If the frontend's domain changes (production, for example), this is the place to
update — without it, the browser blocks every `fetch` call from the frontend due to CORS policy,
even if the API responds normally.

## Adding a new endpoint

1. If it involves a new table/field: add the `Model`, register it in `InventoryDbContext`
   (`DbSet` + `OnModelCreating` if it needs an index/relationship) and generate the migration
   (see `docs/doc_database.md`).
2. Create the request/response `Dto`s in `Dtos/` — never return the EF Core `Model` directly from
   the API (avoids leaking internal fields and circular-reference serialization issues).
3. Create the `Controller` (or add a method to an existing one), with the appropriate
   `[Authorize]`.
4. Run `dotnet build` before considering it done — the build fails fast if something doesn't compile.
