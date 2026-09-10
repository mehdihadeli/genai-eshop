using System.Diagnostics.CodeAnalysis;
using A2A;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace BuildingBlocks.AI.A2A;

public static class EndpointRouteBuilderExtensions
{
    public const string AgentCardPath = ".well-known/agent-card.json";

    // IA2ARequestHandler replaced the removed ITaskManager API in the current A2A SDK.
    // https://learn.microsoft.com/en-us/agent-framework/hosting/self-hosting/a2a/server
    // https://github.com/microsoft/semantic-kernel/issues/13189
    // https://github.com/a2aproject/a2a-dotnet/blob/main/src/A2A.AspNetCore/A2AEndpointRouteBuilderExtensions.cs#L52
    // https://github.com/a2aproject/a2a-dotnet/blob/main/src/A2A/Client/A2ACardResolver.cs#L24
    public static IEndpointConventionBuilder MapCustomWellKnownAgentCard(
        this IEndpointRouteBuilder endpoints,
        IA2ARequestHandler requestHandler,
        [StringSyntax("Route")] string agentPath
    )
    {
        ArgumentNullException.ThrowIfNull(endpoints);
        ArgumentNullException.ThrowIfNull(requestHandler);
        ArgumentException.ThrowIfNullOrEmpty(agentPath);

        var routeGroup = endpoints.MapGroup("");

        var agentPathWithoutSlash = agentPath.TrimStart('/');
        var cardPath = $"{agentPathWithoutSlash}/{AgentCardPath}";
        routeGroup.MapGet(
            cardPath,
            async (HttpRequest request, CancellationToken cancellationToken) =>
            {
                var agentCard = await requestHandler.GetExtendedAgentCardAsync(
                    new GetExtendedAgentCardRequest(),
                    cancellationToken
                );
                return Results.Ok(agentCard);
            }
        );

        return routeGroup;
    }
}
