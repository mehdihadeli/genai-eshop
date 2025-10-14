using System.Diagnostics.CodeAnalysis;
using A2A;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace BuildingBlocks.AI.A2A;

public static class EndpointRouteBuilderExtensions
{
    public const string AgentCardPath = ".well-known/agent-card.json";

    // https://github.com/microsoft/semantic-kernel/issues/13189
    // https://github.com/a2aproject/a2a-dotnet/blob/main/src/A2A.AspNetCore/A2AEndpointRouteBuilderExtensions.cs#L52
    // https://github.com/a2aproject/a2a-dotnet/blob/main/src/A2A/Client/A2ACardResolver.cs#L24
    public static IEndpointConventionBuilder MapCustomWellKnownAgentCard(
        this IEndpointRouteBuilder endpoints,
        ITaskManager taskManager,
        [StringSyntax("Route")] string agentPath
    )
    {
        ArgumentNullException.ThrowIfNull(endpoints);
        ArgumentNullException.ThrowIfNull(taskManager);
        ArgumentException.ThrowIfNullOrEmpty(agentPath);

        var routeGroup = endpoints.MapGroup("");

        var agentPathWithoutSlash = agentPath.TrimStart('/');
        var cardPath = $"{agentPathWithoutSlash}/{AgentCardPath}";
        routeGroup.MapGet(
            cardPath,
            async (HttpRequest request, CancellationToken cancellationToken) =>
            {
                var agentUrl = $"{request.Scheme}://{request.Host}/{agentPathWithoutSlash}";
                var agentCard = await taskManager.OnAgentCardQuery(agentUrl, cancellationToken);
                return Results.Ok(agentCard);
            }
        );

        return routeGroup;
    }
}