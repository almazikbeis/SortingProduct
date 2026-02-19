# SortingProduct

ASP.NET Core (.NET 8) Web API.

## Environment variables (hosting)

Set database connection string via:

- `ConnectionStrings__Db` (recommended)

Example (Npgsql):

`Host=...;Port=5432;Database=...;Username=...;Password=...;SSL Mode=Require;Trust Server Certificate=true`

## Docker

The repository contains a `Dockerfile` for container hosting (Railway, etc.).

## Startup migrations

On startup the app automatically applies EF Core migrations (`Database.Migrate()`).

## Endpoints

- `POST /api/import/xlsx` - import products from Excel (.xlsx)
- `POST /api/admin/grouping/run` - run grouping immediately
- `GET /api/admin/batches` - view all imported batches with remaining quantity
- `GET /api/groups` - list groups
- `GET /api/groups/{groupId}/items` - list items in a group
- `GET /health` - health check
