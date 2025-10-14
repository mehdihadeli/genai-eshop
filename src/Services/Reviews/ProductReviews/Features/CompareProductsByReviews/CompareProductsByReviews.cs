using BuildingBlocks.AI.AgentFramework;
using BuildingBlocks.Exceptions;
using BuildingBlocks.Extensions;
using GenAIEshop.Reviews.Shared.Contracts;
using GenAIEshop.Reviews.Shared.Dtos;
using Mediator;
using Microsoft.Agents.AI;
using Microsoft.Extensions.Options;

namespace GenAIEshop.Reviews.ProductReviews.Features.CompareProductsByReviews;

public sealed record CompareProductsByReviews(List<Guid> ProductIds) : IQuery<CompareProductsByReviewsResult>
{
    public static CompareProductsByReviews Of(params Guid[] productIds)
    {
        if (productIds.Length < 2)
            throw new ValidationException("At least two product IDs are required for comparison.");

        foreach (var productId in productIds)
            productId.NotBeEmpty();

        return new CompareProductsByReviews(productIds.ToList());
    }
}

public sealed class CompareProductsByReviewsHandler(
    [FromKeyedServices(GenAIEshop.Shared.Constants.Agents.ReviewsAgent)] AIAgent reviewsAgent,
    ICatalogServiceClient catalogServiceClient,
    IOptions<AgentFrameworkOptions> agentFrameworkOptions,
    ILogger<CompareProductsByReviewsHandler> logger
) : IQueryHandler<CompareProductsByReviews, CompareProductsByReviewsResult>
{
    public async ValueTask<CompareProductsByReviewsResult> Handle(CompareProductsByReviews query, CancellationToken ct)
    {
        logger.LogInformation("Comparing {ProductCount} products by reviews", query.ProductIds.Count);

        var products =
            await catalogServiceClient.GetProductsByIdAsync(query.ProductIds, ct)
            ?? throw new InvalidOperationException("One or more products not found in catalog.");

        foreach (ProductDto product in products)
        {
            if (!product.IsAvailable)
                throw new InvalidOperationException($"Product '{product.Name}' is currently unavailable.");
        }

        var comparisonRequest =
            @$"Compare products with ids `{string.Join(',', query.ProductIds)}` based on their reviews and provide a comparative analysis.";

        var agentResponse = await reviewsAgent.RunAsync(
            message: comparisonRequest,
            options: ChatOptionsDefaults.GetDefaultAgentRunOptions(agentFrameworkOptions.Value),
            cancellationToken: ct
        );
        var message = agentResponse.Text;

        return new CompareProductsByReviewsResult(
            ProductIds: query.ProductIds,
            ComparisonAnalysis: message,
            GeneratedAt: DateTime.UtcNow
        );
    }
}

public sealed record CompareProductsByReviewsResult(
    List<Guid> ProductIds,
    string ComparisonAnalysis,
    DateTime GeneratedAt
);
