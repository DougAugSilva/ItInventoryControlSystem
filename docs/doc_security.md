# Security

This document records: the current state of the project's security, and what I identified as
still missing to improve going forward.

## What's in place today

### Database and HTTPS certificates

- User passwords stored with a **BCrypt** hash, never in plain text.
- No SQL injection risk — every data access goes through Entity Framework Core's LINQ
  (parameterized), with no raw SQL string concatenation in any controller.
- Role-based authorization (Admin/User) enforced on the **backend**
  (`[Authorize(Roles = "Admin")]` on `UsersController`), not just hidden in the UI — the frontend
  also hides the tab, but the server is what actually guarantees it.
- Protection of the `admin.besttechti` account (cannot be edited/removed) and username
  uniqueness are guaranteed in the controller and reinforced by a unique index in the database.
- Real secrets (connection string, JWT key, SQL Server password) never lived in the
  repository — always in user-secrets, outside version control.
- CORS restricted by configuration to a specific origin, not `*`.

- `DataSeeder.cs` doesn't hardcode any password. `SeedAdminUserAsync` and `SeedTestUserAsync` now
  take an `IConfiguration` and read the password from `Seed:SenhaAdmin` / `Seed:SenhaUsuarioTeste`
  (user-secrets in dev; environment variable in other environments) — if the configuration is
  missing, the application fails loudly and early (`InvalidOperationException`) instead of
  seeding an insecure default password.

- `Properties/launchSettings.json`: the default profile now brings up Kestrel on
  `https://localhost:5443` **and** `http://localhost:5080` (previously it only used the ports via
  `--urls` on the command line, with HTTPS not configured anywhere).
- `Program.cs`: added `app.UseHsts()` outside the Development environment (HSTS doesn't make
  sense in dev, where the certificate is the development one, not a real one).
  `UseHttpsRedirection()` already existed and now actually has an HTTPS endpoint to redirect
  to — tested: a request to `http://localhost:5080/...` responds `307` pointing to
  `https://localhost:5443/...`.
- `frontend/.env`: `VITE_API_URL` changed from `http://localhost:5080` to
  `https://localhost:5443`.
- **Local development notice:** the certificate in use is ASP.NET Core's development certificate
  (self-signed). The browser will show an "insecure connection" warning the first time it
  accesses `https://localhost:5443` directly — this is expected for a development certificate and
  **does not affect the validity of the change**: in production, the server would use a real
  certificate (Let's Encrypt or the company's own), issued by a trusted authority, and this
  warning wouldn't appear. If the warning is bothersome in dev, you can manually trust the
  certificate in the browser (accepting the risk, locally only).

### Password policy

- `Services/PasswordPolicy.cs` class: requires at least 12 characters, at least one uppercase
  letter, one lowercase letter, and one digit (symbols are allowed but not required, and don't
  get in the way).
- Applied in `UsersController` — both when creating a user (`POST /api/users`) and when changing
  an existing user's password (`PUT /api/users/{id}`, only when a new password is sent). Returns
  `400` with the specific message about what's missing.
- Frontend (`Users.jsx`) has a visual hint of the requirements and `minLength={12}` on the field,
  purely for UX — the backend is what actually enforces the rule.
- **Does not apply to login** (`AuthController`), which only verifies the existing password.

### Rate limiting

- Uses ASP.NET Core's built-in rate limiter (`Microsoft.AspNetCore.RateLimiting`, already part of
  the framework, no new package needed).
- Two fixed-window policies, 15 requests per minute each:
  - **`Login`**, partitioned by IP address (there's no session yet at that point) — applied to
    `POST /api/auth/login`.
  - **`SensitiveActions`**, partitioned by logged-in user (`uid` claim) — applied to:
    - `DELETE /api/users/{id}` (record deletion)
    - `PUT /api/users/{id}` (user update)
    - `PUT /api/items/{id}` (item update — this project has no price field; this is the closest
      thing to a "record update" operation that exists in the domain)
    - `GET /api/dashboard/statistics` (the most expensive report to calculate in the system)
  - Exceeding the limit responds `429 Too Many Requests`.
- Manually tested: 17 consecutive login attempts started responding `429` from the 16th onward
  (the window already had 4 requests from earlier tests counting against the same IP).

### Photo upload validation

- Validation used to trust the `Content-Type` sent by the browser — a header the client itself
  controls. Any file could be uploaded with a fake `Content-Type: image/jpeg` and pass right
  through.
- `PhotoStorageService` now reads the file's first bytes and checks the **real binary signature**
  (`FF D8 FF` for JPEG, `89 50 4E 47 0D 0A 1A 0A` for PNG) — the client's `Content-Type` is no
  longer used to decide anything.
- `ItemsController` (`Create` and `Update`) now catches `InvalidPhotoException` and returns `400`
  with the error message — previously this wasn't handled and became a generic `500`.
- Manually tested: a text file renamed with `Content-Type: image/jpeg` was rejected (`400`); a
  real JPEG kept working (`200`).

## What's still missing to improve security

Not every original risk was resolved in this round — what's still pending:

- **JWT token in `localStorage`** — still vulnerable to theft via XSS, if such a vulnerability
  ever shows up. Migrating to an `httpOnly` + `Secure` + `SameSite` cookie would remove this
  attack surface, but requires changing the frontend's authentication model (it would stop
  handling the token directly in JS).
- **No token revocation** — a valid JWT cannot be invalidated before it expires (8h). A denylist
  of revoked tokens (or shortening the duration + a refresh token) would fix this.
- **`Encrypt=False` in the SQL Server connection string** — still the case; it's a workaround
  setting for the native Windows SQL Server behind the WSL network (see `doc_docker.md`). When
  migrating to SQL Server in a container (`docker-compose.yml`) or to production, revisit whether
  the database connection's encryption should be re-enabled.
- **Development HTTPS certificate (self-signed)** — works for local testing, but production
  needs a certificate issued by a trusted authority (Let's Encrypt or the company's own),
  typically terminated at a reverse proxy (nginx, IIS, etc.) in front of the API.
- **No additional security headers** (`Content-Security-Policy`, `X-Content-Type-Options`,
  `X-Frame-Options`) — not implemented; they reduce attack surfaces like clickjacking and some
  types of XSS, but weren't requested in this round.
- **No account lockout on failed login attempts** — IP-based rate limiting helps against simple
  brute force, but doesn't stop a distributed attacker (multiple IPs) from trying passwords
  against a specific account. A temporary per-account lockout (not just per-IP) would cover this
  case.
- **No logging/auditing of sensitive actions** (who deleted what and when) — today there's no
  audit trail beyond each record's own `CreatedAt`/`UpdatedAt`.
