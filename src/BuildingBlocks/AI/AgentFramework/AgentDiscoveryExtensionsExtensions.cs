using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Hosting;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace BuildingBlocks.AI.AgentFramework;

public static class AgentDiscoveryExtensionsExtensions
{
    // Agent Framework exposes registered agents through AIAgent instances; AgentCatalog is no longer available.
    // https://learn.microsoft.com/en-us/agent-framework/integrations/by-component/agent-services/a2a
    public static void MapAgentDiscovery(this IEndpointRouteBuilder endpoints, [StringSyntax("Route")] string path)
    {
        var routeGroup = endpoints.MapGroup(path);

        routeGroup
            .MapGet(
                "/",
                (IEnumerable<AIAgent> agents) =>
                {
                    var results = agents
                        .Select(agent => new AgentDiscoveryCard
                        {
                            Name = agent.Name ?? string.Empty,
                            Description = agent.Description,
                        })
                        .ToList();

                    return TypedResults.Ok(results);
                }
            )
            .WithName("GetAgents");
    }

    private sealed record AgentDiscoveryCard
    {
        public required string Name { get; init; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Description { get; init; }
    }
}
