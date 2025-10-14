using BuildingBlocks.AI.AgentFramework;
using BuildingBlocks.Exceptions;
using BuildingBlocks.Extensions;
using Mediator;
using Microsoft.Agents.AI;
using Microsoft.Extensions.Options;

namespace GenAIEshop.Recommendation.Recommendations.Features.ComparingProducts;

public sealed record CompareProducts(Guid[] ProductIds, string? Criteria) : ICommand<CompareProductsResult>
{
    public static CompareProducts Of(Guid[] productIds, string? criteria)
    {
        foreach (Guid productId in productIds)
        {
            productId.NotBeEmpty();
        }

        return productIds.Length < 2
            ? throw new ValidationException("At least two products are required for comparison")
            : new CompareProducts(productIds, criteria);
    }
}

public sealed record CompareProductsResult(string Comparison, DateTime GeneratedAt);

public sealed class CompareProductsHandler(
    [FromKeyedServices(GenAIEshop.Shared.Constants.Agents.ProductRecommendationAgent)]
        AIAgent productRecommendationAgent,
    IOptions<AgentFrameworkOptions> agentFrameworkOptions,
    ILogger<CompareProductsHandler> logger
) : ICommandHandler<CompareProducts, CompareProductsResult>
{
    public async ValueTask<CompareProductsResult> Handle(CompareProducts query, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Comparing {ProductCount} products with criteria: {Criteria}",
            query.ProductIds.Length,
            query.Criteria ?? "default comparison"
        );

        var comparisonRequest = BuildComparisonRequest(query);

        var agentResponse = await productRecommendationAgent.RunAsync(
            message: comparisonRequest,
            options: ChatOptionsDefaults.GetDefaultAgentRunOptions(agentFrameworkOptions.Value),
            cancellationToken: cancellationToken
        );
        var comparison = agentResponse.Text;

        logger.LogInformation("Successfully generated comparison for {ProductCount} products", query.ProductIds.Length);

        return new CompareProductsResult(Comparison: comparison, GeneratedAt: DateTime.UtcNow);
    }

    private static string BuildComparisonRequest(CompareProducts query)
    {
        var productIdsString = string.Join(", ", query.ProductIds);
        var request = $"Please compare these products: {productIdsString}";

        if (!string.IsNullOrWhiteSpace(query.Criteria))
        {
            request += $". Comparison criteria: {query.Criteria}";
        }

        return request;
    }
}
