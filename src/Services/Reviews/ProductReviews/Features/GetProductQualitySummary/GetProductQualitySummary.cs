using BuildingBlocks.AI.AgentFramework;
using BuildingBlocks.Extensions;
using GenAIEshop.Reviews.Shared.Contracts;
using Mediator;
using Microsoft.Agents.AI;
using Microsoft.Extensions.Options;

namespace GenAIEshop.Reviews.ProductReviews.Features.GetProductQualitySummary;

public sealed record GetProductQualitySummary(Guid ProductId) : IQuery<GetProductQualitySummaryResult>
{
    public static GetProductQualitySummary Of(Guid productId)
    {
        productId.NotBeEmpty();
        return new GetProductQualitySummary(productId);
    }
}

public sealed class GetProductQualitySummaryHandler(
    [FromKeyedServices(GenAIEshop.Shared.Constants.Agents.ReviewsAgent)] AIAgent reviewsAgent,
    ICatalogServiceClient catalogServiceClient,
    IOptions<AgentFrameworkOptions> agentFrameworkOptions,
    ILogger<GetProductQualitySummaryHandler> logger
) : IQueryHandler<GetProductQualitySummary, GetProductQualitySummaryResult>
{
    public async ValueTask<GetProductQualitySummaryResult> Handle(GetProductQualitySummary query, CancellationToken ct)
    {
        logger.LogInformation("Getting quality summary for product {ProductId}", query.ProductId);

        var product =
            await catalogServiceClient.GetProductByIdAsync(query.ProductId, ct)
            ?? throw new InvalidOperationException($"Product {query.ProductId} not found in catalog.");

        if (!product.IsAvailable)
            throw new InvalidOperationException($"Product '{product.Name}' is currently unavailable.");

        var summaryRequest =
            @$"Provide a concise quality summary and classification for product id `{query.ProductId}` based on review analysis.";

        var agentResponse = await reviewsAgent.RunAsync(
            message: summaryRequest,
            options: ChatOptionsDefaults.GetDefaultAgentRunOptions(agentFrameworkOptions.Value),
            cancellationToken: ct
        );
        var message = agentResponse.Text;

        return new GetProductQualitySummaryResult(
            ProductId: query.ProductId,
            QualitySummary: message,
            GeneratedAt: DateTime.UtcNow
        );
    }
}

public sealed record GetProductQualitySummaryResult(Guid ProductId, string QualitySummary, DateTime GeneratedAt);
