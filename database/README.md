# Database

The schema is defined by code (EF Core Code First) in `backend/Inventory.Api/Data`. The generated
migrations live in `backend/Inventory.Api/Data/Migrations` and are the source of truth.

`schema.sql` in this directory (added at database creation time) is an export generated from the
migrations, kept only as a quick reference — it is never applied directly.
