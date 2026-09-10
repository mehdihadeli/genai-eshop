using BuildingBlocks.AI.AgentFramework;
using BuildingBlocks.Extensions;
using GenAIEshop.Reviews.Shared.Contracts;
using Mediator;
using Microsoft.Agents.AI;
using Microsoft.Extensions.Options;

namespace GenAIEshop.Reviews.ProductReviews.Features.AnalyzeProductReviews;

public sealed record AnalyzeProductReviews(Guid ProductId, AgentOrchestrationType AgentOrchestrationType)
    : ICommand<AnalyzeProductReviewsResult>
{
    public static AnalyzeProductReviews Of(Guid productId, AgentOrchestrationType agentOrchestrationType)
    {
        productId.NotBeEmpty();
        return new AnalyzeProductReviews(productId, agentOrchestrationType);
    }
}

public sealed class AnalyzeProductReviewsHandler(
    [FromKeyedServices(GenAIEshop.Shared.Constants.Agents.ReviewsAgent)] AIAgent reviewsAgent,
    // IReviewsOrchestrationService reviewsOrchestrationService,
    ICatalogServiceClient catalogServiceClient,
    IOptions<AgentFrameworkOptions> agentFrameworkOptions,
    ILogger<AnalyzeProductReviewsHandler> logger
) : ICommandHandler<AnalyzeProductReviews, AnalyzeProductReviewsResult>
{
    public async ValueTask<AnalyzeProductReviewsResult> Handle(AnalyzeProductReviews command, CancellationToken ct)
    {
        logger.LogInformation("Analyzing reviews for product {ProductId}", command.ProductId);

        var product =
            await catalogServiceClient.GetProductByIdAsync(command.ProductId, ct)
            ?? throw new InvalidOperationException($"Product {command.ProductId} not found in catalog.");

        if (!product.IsAvailable)
            throw new InvalidOperationException($"Product '{product.Name}' is currently unavailable.");

        var analysisRequest =
            $"Please analyze all reviews for product with id `{command.ProductId}` and perform sentiment analysis, summarization and provide comprehensive quality assessment with sentiment analysis and key insights.";

        string message;
        switch (command.AgentOrchestrationType)
        {
            case AgentOrchestrationType.Normal:
                // RunAsync creates a session when none is supplied; GetNewThread was removed from the current API.
                // https://learn.microsoft.com/en-us/agent-framework/concepts/agents/custom-agents
                var agentResponse = await reviewsAgent.RunAsync(
                    message: analysisRequest,
                    options: ChatOptionsDefaults.GetDefaultAgentRunOptions(agentFrameworkOptions.Value),
                    cancellationToken: ct
                );
                message = agentResponse.Text;
                break;
            // case AgentOrchestrationType.Sequential:
            //     message = await reviewsOrchestrationService.AnalyzeReviewsUsingSequentialOrchestrationAsync(
            //         analysisRequest
            //     );
            //     break;
            // case AgentOrchestrationType.GroupChat:
            //     message = await reviewsOrchestrationService.AnalyzeReviewsUsingChatGroupOrchestrationAsync(
            //         analysisRequest
            //     );
            //     break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        return new AnalyzeProductReviewsResult(
            ProductId: command.ProductId,
            Analysis: message,
            GeneratedAt: DateTime.UtcNow
        );
    }
}

public sealed record AnalyzeProductReviewsResult(Guid ProductId, string Analysis, DateTime GeneratedAt);
