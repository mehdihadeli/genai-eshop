using BuildingBlocks.EF.Extensions;
using BuildingBlocks.Extensions;
using GenAIEshop.Catalogs.Products;
using GenAIEshop.Catalogs.Shared.Data;
using GenAIEshop.Shared.Constants;

namespace GenAIEshop.Catalogs.Shared;

public static class ApplicationConfig
{
    public static WebApplicationBuilder AddApplicationServices(this WebApplicationBuilder builder)
    {
        builder.AddProductsServices();

        AddDatabase(builder);

        return builder;
    }

    private static void AddDatabase(WebApplicationBuilder builder)
    {
        builder.AddPostgresDbContext<CatalogsDbContext>(
            connectionStringName: AspireApplicationResources.PostgresDatabase.Catalogs,
            action: app =>
            {
                if (app.Environment.IsDevelopment() || app.Environment.IsAspireRun())
                {
                    // apply migration and seed data for dev environment
                    app.AddMigration<CatalogsDbContext, CatalogsDataSeeder>();
                }
                else
                {
                    // just apply migration for production without seeding
                    app.AddMigration<CatalogsDbContext>();
                }
            }
        );
    }

    public static IEndpointRouteBuilder MapApplicationEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapProductsEndpoints();

        return endpoints;
    }
}
