using BuildingBlocks.AI.AgentFramework;
using BuildingBlocks.Extensions;
using GenAIEshop.Reviews.Shared.Contracts;
using Mediator;
using Microsoft.Agents.AI;
using Microsoft.Extensions.Options;

namespace GenAIEshop.Reviews.ProductReviews.Features.AnalyzeReviewTrends;

public sealed record AnalyzeReviewTrends(Guid ProductId, int DaysBack = 30) : IQuery<AnalyzeReviewTrendsResult>
{
    public static AnalyzeReviewTrends Of(Guid productId, int daysBack = 30)
    {
        productId.NotBeEmpty();
        daysBack.NotBeNegativeOrZero();
        return new AnalyzeReviewTrends(productId, daysBack);
    }
}

public sealed class AnalyzeReviewTrendsHandler(
    [FromKeyedServices(GenAIEshop.Shared.Constants.Agents.ReviewsAgent)] AIAgent reviewsAgent,
    ICatalogServiceClient catalogServiceClient,
    IOptions<AgentFrameworkOptions> agentFrameworkOptions,
    ILogger<AnalyzeReviewTrendsHandler> logger
) : IQueryHandler<AnalyzeReviewTrends, AnalyzeReviewTrendsResult>
{
    public async ValueTask<AnalyzeReviewTrendsResult> Handle(AnalyzeReviewTrends query, CancellationToken ct)
    {
        logger.LogInformation(
            "Analyzing review trends for product {ProductId} over {DaysBack} days",
            query.ProductId,
            query.DaysBack
        );

        var product =
            await catalogServiceClient.GetProductByIdAsync(query.ProductId, ct)
            ?? throw new InvalidOperationException($"Product {query.ProductId} not found in catalog.");

        if (!product.IsAvailable)
            throw new InvalidOperationException($"Product '{product.Name}' is currently unavailable.");

        var trendsRequest =
            $"Analyze review trends and patterns for product id `{query.ProductId}` over the past {query.DaysBack} days. Identify any significant changes in sentiment or common themes.";

        var agentResponse = await reviewsAgent.RunAsync(
            message: trendsRequest,
            options: ChatOptionsDefaults.GetDefaultAgentRunOptions(agentFrameworkOptions.Value),
            cancellationToken: ct
        );
        var message = agentResponse.Text;

        return new AnalyzeReviewTrendsResult(
            ProductId: query.ProductId,
            TrendsAnalysis: message,
            PeriodInDays: query.DaysBack,
            GeneratedAt: DateTime.UtcNow
        );
    }
}

public sealed record AnalyzeReviewTrendsResult(
    Guid ProductId,
    string TrendsAnalysis,
    int PeriodInDays,
    DateTime GeneratedAt
);
