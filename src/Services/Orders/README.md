## Migration

```bash
dotnet ef migrations add InitialMigration -o Shared/Data/Migrations -c OrdersDbContext
dotnet ef database update -c OrdersDbContext
```