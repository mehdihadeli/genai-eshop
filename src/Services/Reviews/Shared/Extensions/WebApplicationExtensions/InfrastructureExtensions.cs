using A2A.AspNetCore;
using BuildingBlocks.AI.A2A;
using BuildingBlocks.AI.AgentFramework;
using BuildingBlocks.OpenApi;
using GenAIEshop.Reviews.Shared.Agents;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.A2A;

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
        var hostAgent = new A2AHostAgent(reviewAgent, ReviewsAgent.GetAgentCard());

        // json-rpc endpoint
        app.MapA2A(hostAgent.TaskManager!, "/reviews").WithTags(GenAIEshop.Shared.Constants.Agents.ReviewsAgent);
        app.MapHttpA2A(hostAgent.TaskManager!, "/reviews").WithTags(GenAIEshop.Shared.Constants.Agents.ReviewsAgent);
        app.MapCustomWellKnownAgentCard(hostAgent.TaskManager!, agentPath: "/reviews");
    }

    private static void MapHostSummarizeA2AAgent(this WebApplication app)
    {
        var summarizeAgent = app.Services.GetRequiredKeyedService<AIAgent>(
            GenAIEshop.Shared.Constants.Agents.SummarizeAgent
        );
        var hostAgent = new A2AHostAgent(summarizeAgent, SummerizeAgent.GetAgentCard());

        app.MapA2A(hostAgent.TaskManager!, "/summarize").WithTags(GenAIEshop.Shared.Constants.Agents.SummarizeAgent);
        app.MapHttpA2A(hostAgent.TaskManager!, "/summarize")
            .WithTags(GenAIEshop.Shared.Constants.Agents.SummarizeAgent);
        app.MapCustomWellKnownAgentCard(hostAgent.TaskManager!, agentPath: "/summarize");
    }

    private static void MapHostSentimentA2AAgent(this WebApplication app)
    {
        var sentimentAgent = app.Services.GetRequiredKeyedService<AIAgent>(
            GenAIEshop.Shared.Constants.Agents.SentimentAgent
        );
        var hostAgent = new A2AHostAgent(sentimentAgent, SentimentAgent.GetAgentCard());

        app.MapA2A(hostAgent.TaskManager!, "/sentiment").WithTags(GenAIEshop.Shared.Constants.Agents.SentimentAgent);
        app.MapHttpA2A(hostAgent.TaskManager!, "/sentiment")
            .WithTags(GenAIEshop.Shared.Constants.Agents.SentimentAgent);
        app.MapCustomWellKnownAgentCard(hostAgent.TaskManager!, agentPath: "/sentiment");
    }
}
