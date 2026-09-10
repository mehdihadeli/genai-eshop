using A2A.AspNetCore;
using BuildingBlocks.AI.A2A;
using BuildingBlocks.AI.AgentFramework;
using BuildingBlocks.OpenApi;
using GenAIEshop.Reviews.Shared.Agents;
using Microsoft.Agents.AI;

namespace GenAIEshop.Reviews.Shared.Extensions.WebApplicationExtensions;

public static class InfrastructureExtensions
{
    public static WebApplication UseInfrastructure(this WebApplication app)
    {
        app.UseExceptionHandler(new ExceptionHandlerOptions { AllowStatusCode404Response = true });
        // Handles non-exceptional status codes (e.g., 404 from Results.NotFound(), 401 from unauthorized access) and returns standardized ProblemDetails responses
        app.UseStatusCodePages();

        app.UseAspnetOpenApi();

        if (app.Environment.IsProduction())
        {
            app.UseHttpsRedirection();
        }

        // https://localhost:8001/.well-known/agent.json
        // https://a2aprotocol.ai/docs/guide/a2a-dotnet-sdk
        // https://github.com/microsoft/semantic-kernel/issues/13189
        // https://github.com/a2aproject/a2a-dotnet/tree/main/samples
        // https://github.com/microsoft/semantic-kernel/tree/main/dotnet/samples/Demos/A2AClientServer
        // Current Agent Framework A2A hosting maps HTTP+JSON and JSON-RPC endpoints directly from AIAgent.
        // https://learn.microsoft.com/en-us/agent-framework/hosting/self-hosting/a2a/server
        app.MapHostReviewA2AAgent();
        app.MapHostSummarizeA2AAgent();
        app.MapHostSentimentA2AAgent();

        app.MapAgentDiscovery("/agents");

        return app;
    }

    // https://github.com/microsoft/semantic-kernel/blob/90d158cbf8bd4598159a6fe64df745e56d9cbdf4/dotnet/samples/Demos/A2AClientServer/A2AServer/HostAgentFactory.cs#L30
    // https://github.com/microsoft/semantic-kernel/blob/90d158cbf8bd4598159a6fe64df745e56d9cbdf4/dotnet/samples/Demos/A2AClientServer/A2AServer/Program.cs#L99
    private static void MapHostReviewA2AAgent(this WebApplication app)
    {
        var reviewAgent = app.Services.GetRequiredKeyedService<AIAgent>(
            GenAIEshop.Shared.Constants.Agents.ReviewsAgent
        );
        app.MapA2AJsonRpc(reviewAgent, "/reviews").WithTags(GenAIEshop.Shared.Constants.Agents.ReviewsAgent);
        app.MapA2AHttpJson(reviewAgent, "/reviews").WithTags(GenAIEshop.Shared.Constants.Agents.ReviewsAgent);
        app.MapGet("/reviews/.well-known/agent-card.json", () => Results.Ok(ReviewsAgent.GetAgentCard()));
    }

    private static void MapHostSummarizeA2AAgent(this WebApplication app)
    {
        var summarizeAgent = app.Services.GetRequiredKeyedService<AIAgent>(
            GenAIEshop.Shared.Constants.Agents.SummarizeAgent
        );
        app.MapA2AJsonRpc(summarizeAgent, "/summarize").WithTags(GenAIEshop.Shared.Constants.Agents.SummarizeAgent);
        app.MapA2AHttpJson(summarizeAgent, "/summarize").WithTags(GenAIEshop.Shared.Constants.Agents.SummarizeAgent);
    }

    private static void MapHostSentimentA2AAgent(this WebApplication app)
    {
        var sentimentAgent = app.Services.GetRequiredKeyedService<AIAgent>(
            GenAIEshop.Shared.Constants.Agents.SentimentAgent
        );
        app.MapA2AJsonRpc(sentimentAgent, "/sentiment").WithTags(GenAIEshop.Shared.Constants.Agents.SentimentAgent);
        app.MapA2AHttpJson(sentimentAgent, "/sentiment").WithTags(GenAIEshop.Shared.Constants.Agents.SentimentAgent);
    }
}
