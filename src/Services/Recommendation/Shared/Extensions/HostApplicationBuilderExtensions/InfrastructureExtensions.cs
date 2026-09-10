using BuildingBlocks.AI.A2A;
using BuildingBlocks.AI.AgentFramework;
using BuildingBlocks.AI.MCP;
using BuildingBlocks.Env;
using BuildingBlocks.OpenApi;
using BuildingBlocks.ProblemDetails;
using BuildingBlocks.Serialization;
using BuildingBlocks.Versioning;
using GenAIEshop.Recommendation.Shared.Agents;
using GenAIEshop.Shared.Constants;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Hosting;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;

namespace GenAIEshop.Recommendation.Shared.Extensions.HostApplicationBuilderExtensions;

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
            options.Namespace = "Recommendation";
        });

        return builder;
    }

    private static void AddAIServices(IHostApplicationBuilder builder)
    {
        builder.AddAgentFramework();

        builder.AddHttpMcpClient(
            mcpClientName: Mcp.SharedMcpTools,
            mcpServerHostUrl: $"https+http://{AspireApplicationResources.Api.McpServerApi}"
        );

        // ref: https://github.com/modelcontextprotocol/servers/tree/main/src/time
        builder.AddStdioMcpClient(
            mcpClientName: Mcp.DateTimeMcpTools,
            command: "docker",
            arguments: ["run", "-i", "--rm", "-e", "LOCAL_TIMEZONE", "mcp/time"]
        );

        AddAgents(builder);
    }

    private static void AddAgents(IHostApplicationBuilder builder)
    {
        builder.AddA2AClient(
            agentName: GenAIEshop.Shared.Constants.Agents.ReviewsAgent,
            agentHostUrl: $"https+http://{AspireApplicationResources.Api.ReviewsApi}",
            agentPath: "/reviews"
        );

        builder.AddAIAgent(
            GenAIEshop.Shared.Constants.Agents.ProductRecommendationAgent,
            (sp, key) =>
            {
                var chatClient = sp.GetRequiredService<IChatClient>();
                using var scope = sp.CreateScope();

                var reviewA2AAgent = scope.ServiceProvider.GetRequiredKeyedService<AIAgent>(
                    GenAIEshop.Shared.Constants.Agents.ReviewsAgent
                );

                var sharedMcpClient = scope.ServiceProvider.GetMcpClientByName(Mcp.SharedMcpTools);
                var timeMcpClient = scope.ServiceProvider.GetMcpClientByName(Mcp.DateTimeMcpTools);

                return ProductRecommendationAgent.CreateAgent(
                    chatClient,
                    sharedMcpClient,
                    timeMcpClient,
                    reviewA2AAgent
                );
            }
        );
    }
}
