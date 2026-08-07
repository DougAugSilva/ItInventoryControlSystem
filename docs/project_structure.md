# Project structure

Directory tree of `ItInventoryControl` **with only what is versioned and goes to GitHub**
(generated from `git ls-files`). Items ignored by `.gitignore` (builds, installed dependencies,
secrets, uploads) don't appear here — the full list of them is in
["Not versioned"](#not-versioned), at the end of this document.

Detailed documentation by area: [`doc_backend.md`](doc_backend.md),
[`doc_database.md`](doc_database.md), [`doc_docker.md`](doc_docker.md),
[`doc_frontend.md`](doc_frontend.md), [`doc_security.md`](doc_security.md).

```
ItInventoryControl/
├── backend/                             # .NET API (ASP.NET Core 10) — see doc_backend.md
│   ├── Inventory.slnx                   # .NET solution (references the Inventory.Api project)
│   └── Inventory.Api/
│       ├── Controllers/                 # API HTTP endpoints
│       │   ├── AuthController.cs        # POST /api/auth/login, GET /api/auth/me
│       │   ├── DashboardController.cs   # GET /api/dashboard/statistics (chart + filters)
│       │   ├── LoansController.cs       # GET /api/loans (feeds the Loans page)
│       │   ├── ItemsController.cs       # Item CRUD, photo upload, loan lifecycle
│       │   ├── ItemTypesController.cs   # GET /api/item-types (fixed list from section 1.1)
│       │   └── UsersController.cs       # User CRUD (Admin only); protects admin.besttechti
│       ├── Data/
│       │   ├── SeedItemJson.cs          # DTO mirroring the format of the local seed JSON file
│       │   ├── DataSeeder.cs            # Seeds the test user and imports the seed JSON
│       │   ├── InventoryDbContext.cs    # EF Core DbContext + ItemTypes table seed
│       │   ├── InventoryDbContextFactory.cs # Factory used by the `dotnet ef` tools (design-time)
│       │   └── Migrations/              # EF Core migrations — source of truth for the schema
│       ├── Dtos/                        # Request/response objects (never expose Models directly)
│       │   ├── AuthDtos.cs
│       │   ├── DashboardStatisticsDto.cs
│       │   ├── LoanDto.cs
│       │   ├── ItemDtos.cs
│       │   ├── ItemTypeDto.cs
│       │   └── UserDtos.cs
│       ├── Models/                      # Domain entities (mapped by EF Core)
│       │   ├── Loan.cs                  # An "outbound" record (who took it, for whom, when it's due back)
│       │   ├── ItemCondition.cs         # Enum: New / Used / Defective / Broken
│       │   ├── Item.cs
│       │   ├── AvailabilityStatus.cs    # Enum: Available / Loaned / Unavailable
│       │   ├── ItemType.cs
│       │   └── User.cs
│       ├── Services/
│       │   ├── PhotoStorageService.cs   # Validates (jpg/png binary signature, up to 10MB) and saves the photo
│       │   ├── JwtService.cs            # Generates the JWT token on login
│       │   └── PasswordPolicy.cs        # Minimum password rule (12+ characters, upper/lowercase/number)
│       ├── Properties/launchSettings.json # `dotnet run` / Visual Studio launch profiles
│       ├── appsettings.json             # Public config (e.g., allowed CORS origins)
│       ├── appsettings.Development.json # Overrides for the development environment only
│       ├── Inventory.Api.csproj         # Project definition and NuGet packages
│       ├── Inventory.Api.http           # Sample requests to test the API in the editor
│       └── Program.cs                   # Bootstrap: DI, JWT auth, CORS, rate limiting, HTTPS/HSTS, static files, migrations+seed
│
├── frontend/                            # React SPA, served by Vite — see doc_frontend.md
│   ├── public/
│   │   └── favicon.png                  # Logo used as the favicon
│   ├── src/
│   │   ├── components/                  # UI pieces shared across pages
│   │   │   ├── AdminRoute.jsx           # Redirects to "/" if the logged-in user isn't Admin
│   │   │   ├── Layout.jsx / .css        # "Fixed chrome" (tabs, logo, search) + side wallpaper
│   │   │   ├── ProtectedRoute.jsx       # Redirects to /login if there's no session
│   │   │   ├── SearchBar.jsx / .css     # Search with filters (type/status/condition) in the header
│   │   │   └── SquareImage.jsx          # Crops any photo into a square using CSS only
│   │   ├── constants/
│   │   │   └── items.js                 # Mirrors the backend's AvailabilityStatus/ItemCondition enums
│   │   ├── context/
│   │   │   └── AuthContext.jsx          # Login/logout state, JWT token, session
│   │   ├── pages/                       # One page per route
│   │   │   ├── Login.jsx / .css         # Login screen (logo, username/password, error)
│   │   │   ├── Home.jsx / .css          # "Register or Edit Item" (creates and edits items)
│   │   │   ├── Dashboard.jsx / .css     # Bar chart by type + filters
│   │   │   ├── Loans.jsx / .css         # List of loans (items checked out)
│   │   │   ├── Items.jsx / .css         # List of registered items, most recent first
│   │   │   └── Users.jsx / .css         # "User Management" (Admin only)
│   │   ├── services/
│   │   │   └── api.js                   # fetch wrapper (JSON and multipart) with Authorization Bearer
│   │   ├── styles/
│   │   │   ├── variables.css            # Project color palette
│   │   │   └── page-box.css             # Shared white-box-with-title-and-divider pattern
│   │   ├── App.jsx                      # Route definitions (React Router)
│   │   ├── main.jsx                     # Entry point (mounts React onto the page)
│   │   └── index.css                    # Global reset/styles
│   ├── .env                             # VITE_API_URL — not a secret, stays versioned
│   ├── .gitignore
│   ├── .oxlintrc.json                   # Linter configuration (`npm run lint`)
│   ├── README.md                        # Default README generated by Vite
│   ├── index.html                       # Root HTML served by Vite
│   ├── vite.config.js                   # Bundler/dev server configuration
│   └── package.json / package-lock.json # Frontend dependencies
│
├── docker/                              # See doc_docker.md
│   ├── docker-compose.yml               # Brings up SQL Server 2022 in a container for the database
│   ├── Dockerfile.api                   # Packages the API for production
│   └── .env.example                     # Environment variable template (SQL Server password)
│
├── database/                            # See doc_database.md
│   ├── README.md                        # Explains that the real schema comes from EF Core migrations
│   └── schema.sql                       # Reference export of the migrations (documentation only)
│
├── docs/                                # Project documentation (this directory)
│   ├── project_structure.md             # This file
│   ├── future_features.md
│   ├── execution_manual.md
│   ├── doc_backend.md
│   ├── doc_database.md
│   ├── doc_docker.md
│   ├── doc_frontend.md
│   └── doc_security.md
│
├── img/                                 # Images used by the site (served statically by the API at /img)
│   ├── desing/                          # Logos and design images
│   │   ├── logo_1.png                   # Logo used on the Login screen and as the favicon
│   │   ├── logo_2.png                   # Logo used in the fixed top bar (Layout)
│   │   └── data_center_wallpaper.jpg    # Wallpaper shown on the sides of the content (80% width)
│   └── itens/                           # Sample photos for the 7 test items
│
├── .gitignore
├── LICENSE
└── README.md
```

## Overview by area

- **`backend/`** — REST API in ASP.NET Core 10 + Entity Framework Core, JWT authentication,
  photo upload, and the business logic (items, types, loans, users, Dashboard statistics). Runs
  natively with `dotnet run` (inside WSL, not via `\\wsl.localhost\...` from Windows — see
  `doc_backend.md`), serving HTTPS by default (see `doc_security.md`).
- **`frontend/`** — React SPA (plain Vite, no UI framework), consuming the backend API via
  `fetch`. Runs with `npm run dev`. The pages' visual pattern (white box with title + divider,
  80%-width layout with side wallpaper) lives in `src/styles/page-box.css` and
  `src/components/Layout.css`.
- **`docker/`** — runs SQL Server in a container (`docker-compose.yml`) for development and
  packages the API for production (`Dockerfile.api`) — see `doc_docker.md`.
- **`database/`** — schema documentation/export; the real schema is generated by the migrations
  in `backend/Inventory.Api/Data/Migrations`.
- **`docs/`** — the project's technical documentation (this file and the five `doc_*.md` files).
- **`img/`** — visual assets (logos, wallpaper, and sample item photos), served as static files
  by the API itself at `/img/...`.

## Not versioned

These exist on a developer's disk, but never go to GitHub — that's why they don't appear in the
tree above:

| Path | Why |
|---|---|
| `backend/**/bin/`, `backend/**/obj/` | .NET build output, generated on every `dotnet build` |
| `backend/**/wwwroot/uploads/` | Photos uploaded through the registration form — runtime data, not code |
| `*.user`, `.vs/`, `.vscode/`, `*.suo` | Local IDE configuration |
| `frontend/node_modules/` | Dependencies installed via `npm install` |
| `frontend/dist/`, `frontend/.vite/` | Output of `npm run build` / Vite cache |
| `docker/.env` | Real SQL Server password used by `docker-compose` (only `.env.example` is versioned) |
| `Thumbs.db` | Windows Explorer junk |
| Backend user-secrets (connection string, JWT key) | Stay entirely outside the repository (not even on disk inside the project folder) — see `doc_backend.md` |
