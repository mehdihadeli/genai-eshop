using BuildingBlocks.AI.AgentFramework;
using BuildingBlocks.Env;
using BuildingBlocks.OpenApi;
using BuildingBlocks.ProblemDetails;
using BuildingBlocks.Serialization;
using BuildingBlocks.VectorDB;
using BuildingBlocks.VectorDB.Contracts;
using BuildingBlocks.Versioning;
using GenAIEshop.Catalogs.Products.Data;
using GenAIEshop.Catalogs.Products.Models;
using Microsoft.Extensions.Configuration;

namespace GenAIEshop.Catalogs.Shared.Extensions.HostApplicationBuilderExtensions;

public static class HostApplicationBuilderExtensions
{
    public static IHostApplicationBuilder AddInfrastructure(this IHostApplicationBuilder builder)
    {
        DotEnv.Load();
        builder.Configuration.AddEnvironmentVariables();
        builder.AddCustomProblemDetails();

        // Apply to other places rather than controller response like openapi document generation, and customizes the default JSON serialization behavior for Minimal APIs
        builder.Services.Configure<Microsoft.AspNetCore.Http.Json.JsonOptions>(options =>
        {
            SystemTextJsonSerializerOptions.SetDefaultOptions(options.SerializerOptions);
        });

        builder.AddVersioning();
        builder.AddAspnetOpenApi(["v1"]);

        AddAIServices(builder);

        // https://github.com/martinothamar/Mediator
        builder.Services.AddMediator(options =>
        {
            //options.Assemblies = handlerScanAssemblies;
            options.ServiceLifetime = ServiceLifetime.Scoped;
            options.Namespace = "Catalogs";
        });

        return builder;
    }

    private static void AddAIServices(IHostApplicationBuilder builder)
    {
        builder.AddAgentFramework();

        builder.AddVectorDB(VectorDBType.Qdrant);
        builder.Services.AddScoped<IDataIngestor<Product>, ProductVectorIngestor>();
    }
}
