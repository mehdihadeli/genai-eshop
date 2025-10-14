using BuildingBlocks.AI.AgentFramework;
using BuildingBlocks.Extensions;
using Mediator;
using Microsoft.Agents.AI;
using Microsoft.Extensions.Options;

namespace GenAIEshop.Recommendation.Recommendations.Features.GettingRecommendation;

public sealed record GetProductRecommendations(string Query) : IQuery<GetProductRecommendationsResult>
{
    public static GetProductRecommendations Of(string query)
    {
        query.NotBeNullOrWhiteSpace();
        return new GetProductRecommendations(query);
    }
}

public sealed class GetProductRecommendationsHandler(
    [FromKeyedServices(GenAIEshop.Shared.Constants.Agents.ProductRecommendationAgent)]
        AIAgent productRecommendationAgent,
    IOptions<AgentFrameworkOptions> agentFrameworkOptions,
    ILogger<GetProductRecommendationsHandler> logger
) : IQueryHandler<GetProductRecommendations, GetProductRecommendationsResult>
{
    public async ValueTask<GetProductRecommendationsResult> Handle(
        GetProductRecommendations query,
        CancellationToken cancellationToken
    )
    {
        logger.LogInformation("Generating product recommendations for query: {Query}", query.Query);

        var recommendationRequest = $"Please provide product recommendations for: {query.Query}";

        var agentResponse = await productRecommendationAgent.RunAsync(
            message: recommendationRequest,
            options: ChatOptionsDefaults.GetDefaultAgentRunOptions(agentFrameworkOptions.Value),
            cancellationToken: cancellationToken
        );
        var recommendations = agentResponse.Text;

        return new GetProductRecommendationsResult(Recommendations: recommendations, GeneratedAt: DateTime.UtcNow);
    }
}

public sealed record GetProductRecommendationsResult(string Recommendations, DateTime GeneratedAt);
