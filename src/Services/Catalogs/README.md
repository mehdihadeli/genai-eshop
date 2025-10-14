## Migration

```bash
dotnet ef migrations add InitialMigration -o Shared/Data/Migrations -c CatalogsDbContext
dotnet ef database update -c CatalogsDbContext
```