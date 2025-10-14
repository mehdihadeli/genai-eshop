## Migration

```bash
dotnet ef migrations add InitialMigration -o Shared/Data/Migrations -c ReviewsDbContext
dotnet ef database update -c ReviewsDbContext
```