using System.Reflection;
using BuildingBlocks.AI.MCP;
using BuildingBlocks.OpenApi;
using BuildingBlocks.ProblemDetails;
using BuildingBlocks.Serialization;
using BuildingBlocks.Versioning;
using GenAIEshop.Shared.Constants;

namespace GenAIEshop.Carts.Shared.Extensions.HostApplicationBuilderExtensions;

public static class HostApplicationBuilderExtensions
{
    public static IHostApplicationBuilder AddInfrastructure(this IHostApplicationBuilder builder)
    {
        builder.AddCustomProblemDetails();

        // Apply to other places rather than controller response like openapi document generation, and customizes the default JSON serialization behavior for Minimal APIs
        builder.Services.Configure<Microsoft.AspNetCore.Http.Json.JsonOptions>(options =>
        {
            SystemTextJsonSerializerOptions.SetDefaultOptions(options.SerializerOptions);
        });

        builder.AddVersioning();
        builder.AddAspnetMcpOpenApi(["v1"]);

        builder.AddCustomMcpServer(
            name: AspireApplicationResources.Api.McpServerApi,
            toolAssembly: Assembly.GetExecutingAssembly()
        );

        return builder;
    }
}
