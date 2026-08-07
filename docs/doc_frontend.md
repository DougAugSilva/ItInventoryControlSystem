# Frontend

SPA in **React 19** (plain JavaScript, no TypeScript), bundled with **Vite**, no UI framework
(hand-written CSS, with CSS variables for the color palette). Talks to the backend only via
`fetch` — no SSR, no direct database calls.

For anything security-related (where the token is stored, password policy, etc.), see
[`doc_security.md`](doc_security.md).

## Stack

- React 19 + `react-router-dom` 7 (client-side routing)
- `recharts` (Dashboard's bar chart)
- Vite 8 (dev server + build), `@vitejs/plugin-react`
- `oxlint` for linting (`npm run lint`)

## Running it

```bash
cd frontend
npm install
npm run dev      # dev server at http://localhost:5173
npm run build    # generates frontend/dist (static, for production)
```

Needs the backend running (`https://localhost:5443` by default — see
[`doc_security.md`](doc_security.md) about HTTPS) — the API URL comes from `VITE_API_URL` in
`.env` (this file **is** versioned: it's not a secret, just points to where the API is). If the
backend runs on a different port/host, just change that variable.

Since the backend uses ASP.NET Core's development certificate (self-signed), the browser will
warn about an "insecure connection" the first time — that's expected locally; open
`https://localhost:5443` directly once and accept the warning before using the frontend, or the
pages' `fetch` calls will silently fail, blocked by the browser.

**Environment note:** if developing in WSL, run `npm run dev` from inside WSL, on the native
Linux path (not via `\\wsl.localhost\...` from Windows) — Node is already installed natively
there, and accessing the project files over the network instead of local disk is much slower
(same reason detailed in [`doc_backend.md`](doc_backend.md) for `dotnet`).

## Folder structure

```
src/
├── components/   # UI shared across pages (route guards, fixed layout, search)
├── constants/    # Mirrors backend enums (AvailabilityStatus, ItemCondition)
├── context/      # AuthContext (session/login)
├── pages/        # One page per route
├── services/     # api.js — the only place that fetches from the backend
├── styles/       # Color palette and layout patterns shared across pages
├── App.jsx       # Route definitions
└── main.jsx      # Entry point
```

## Routes and authentication (`App.jsx`)

```
/login                → Login (public)
/                     → Home ("Register or Edit Item")
/dashboard            → Dashboard
/loans                → Loans
/items                → Items
/users                → Users (Admin only)
```

Everything except `/login` lives inside `<ProtectedRoute><Layout /></ProtectedRoute>` — without a
valid session, it redirects to `/login`. The `/users` route has a second layer, `<AdminRoute>`,
which redirects to `/` if the logged-in user isn't an administrator (`user.isAdmin`). **This
frontend check exists only to avoid needlessly exposing the UI** — what actually enforces access
is `[Authorize(Roles = "Admin")]` on the backend's `UsersController`; never rely solely on a
frontend check to protect sensitive data.

`AuthContext` keeps the logged-in user in memory (React state) and the JWT token in
`localStorage` (via `services/api.js`). On page reload, `AuthContext` calls `GET /api/auth/me` to
restore the session from the saved token; if the token is invalid/expired, it clears it and sends
the user to `/login`.

## `services/api.js`

A thin wrapper over `fetch`, used by every page — avoids repeating the `Authorization` header and
error handling in every call:

- `apiJson(path, { method, body })` — JSON requests, automatically injects
  `Authorization: Bearer <token>` if one is saved.
- `apiForm(path, { method, formData })` — for photo uploads (multipart), same token injection.
- `fileUrl(path)` — builds the full URL (`VITE_API_URL` + path) to display an image served by the
  backend (item photo, logo, wallpaper).
- Every non-2xx response becomes an `Error` with the message the backend sent (`message` or
  `title`) — that's what shows up in the pages' error "toasts".

## Shared visual pattern (`styles/page-box.css`)

Every internal page (Home, Dashboard, Loans, Items, Users) follows the same "skeleton": a single
white box (`.page-box`) with the title on top (`.page-box-header`, with a thin divider line) and
the content below (`.page-box-body`), stretching to fill the available height — a LinkedIn
card-like style. When creating a new page, reuse these three classes instead of styling the
title/box from scratch.

`Layout.jsx`/`Layout.css` takes care of the rest of the fixed visuals: the top bar (logo, tabs,
search, user — intentionally the same on every page, avoid changing it without a reason) and the
side wallpaper (content centered at 80% width, wallpaper visible only on the sides).

## Adding a new page

1. Create `src/pages/NewPage.jsx` + `.css`, using the `page-box` pattern above.
2. Register the route in `App.jsx` (inside `<Layout />` if it needs the fixed bar and a session).
3. If the page should only be accessible to admins, wrap it with `<AdminRoute>` like `/users`
   does — and remember to protect the corresponding backend endpoint too.
4. If it should appear in the top tabs, add it to `TABS` (`Layout.jsx`).
