using BuildingBlocks.AI.AgentFramework;
using BuildingBlocks.Env;
using BuildingBlocks.OpenApi;
using BuildingBlocks.ProblemDetails;
using BuildingBlocks.Serialization;
using BuildingBlocks.Versioning;
using GenAIEshop.Reviews.Shared.Agents;
using GenAIEshop.Reviews.Shared.Tools;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Hosting;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;

namespace GenAIEshop.Reviews.Shared.Extensions.HostApplicationBuilderExtensions;

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

        // https://github.com/martinothamar/Mediator

        builder.Services.AddMediator(options =>
        {
            //options.Assemblies = handlerScanAssemblies;
            options.ServiceLifetime = ServiceLifetime.Scoped;
            options.Namespace = "Reviews";
        });

        AddAIServices(builder);

        return builder;
    }

    private static void AddAIServices(IHostApplicationBuilder builder)
    {
        builder.AddAgentFramework();

        AddAgents(builder);
    }

    private static void AddAgents(IHostApplicationBuilder builder)
    {
        builder.Services.AddScoped<ReviewsTool>();

        builder.AddAIAgent(
            GenAIEshop.Shared.Constants.Agents.ReviewsAgent,
            (sp, key) =>
            {
                var chatClient = sp.GetRequiredService<IChatClient>();

                var sentimentAgent = sp.GetRequiredKeyedService<AIAgent>(
                    GenAIEshop.Shared.Constants.Agents.SentimentAgent
                );
                var languageAgent = sp.GetRequiredKeyedService<AIAgent>(
                    GenAIEshop.Shared.Constants.Agents.LanguageAgent
                );
                var summarizeAgent = sp.GetRequiredKeyedService<AIAgent>(
                    GenAIEshop.Shared.Constants.Agents.SummarizeAgent
                );

                var scope = sp.CreateScope();
                var reviewsTool = scope.ServiceProvider.GetRequiredService<ReviewsTool>();

                return ReviewsAgent.CreateAgent(chatClient, languageAgent, sentimentAgent, summarizeAgent, reviewsTool);
            }
        );

        builder.AddAIAgent(
            GenAIEshop.Shared.Constants.Agents.LanguageAgent,
            (sp, key) =>
            {
                var chatClient = sp.GetRequiredService<IChatClient>();

                return LanguageAgent.CreateAgent(chatClient);
            }
        );

        builder.AddAIAgent(
            GenAIEshop.Shared.Constants.Agents.SentimentAgent,
            (sp, key) =>
            {
                var chatClient = sp.GetRequiredService<IChatClient>();

                return SentimentAgent.CreateAgent(chatClient);
            }
        );

        builder.AddAIAgent(
            GenAIEshop.Shared.Constants.Agents.SummarizeAgent,
            (sp, key) =>
            {
                var chatClient = sp.GetRequiredService<IChatClient>();

                return SummerizeAgent.CreateAgent(chatClient);
            }
        );

        builder.AddAIAgent(
            GenAIEshop.Shared.Constants.Agents.ReviewsCollectorAgent,
            (sp, key) =>
            {
                var chatClient = sp.GetRequiredService<IChatClient>();

                var scope = sp.CreateScope();
                var reviewsTool = scope.ServiceProvider.GetRequiredService<ReviewsTool>();

                return ReviewsCollectorAgent.CreateAgent(chatClient, reviewsTool);
            }
        );

        builder.AddAIAgent(
            GenAIEshop.Shared.Constants.Agents.InsightsSynthesizerAgent,
            (sp, key) =>
            {
                var chatClient = sp.GetRequiredService<IChatClient>();
                return InsightsSynthesizerAgent.CreateAgent(chatClient);
            }
        );

        builder.AddA2AServer(GenAIEshop.Shared.Constants.Agents.ReviewsAgent, _ => { });
        builder.AddA2AServer(GenAIEshop.Shared.Constants.Agents.SummarizeAgent, _ => { });
        builder.AddA2AServer(GenAIEshop.Shared.Constants.Agents.SentimentAgent, _ => { });

        // builder.Services.AddSingleton<ReviewsSequentialOrchestrationAgent>();
        // builder.Services.AddSingleton<ReviewsChatOrchestrationAgent>();
        // builder.Services.AddSingleton<IReviewsOrchestrationService, ReviewsOrchestrationService>();
        // builder.Services.AddSingleton<IntelligentReviewsChatManager>();
    }
}
