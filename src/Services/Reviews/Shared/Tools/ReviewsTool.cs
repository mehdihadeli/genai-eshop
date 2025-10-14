using System.ComponentModel;
using BuildingBlocks.AI.AgentFramework;
using GenAIEshop.Reviews.ProductReviews.Dtos;
using GenAIEshop.Reviews.Shared.Contracts;
using Mediator;
using Microsoft.Extensions.AI;

namespace GenAIEshop.Reviews.Shared.Tools;

// https://github.com/microsoft/agent-framework/blob/main/dotnet/samples/SemanticKernelMigration/AzureOpenAI/Step02_ToolCall/Program.cs
// https://learn.microsoft.com/en-us/dotnet/ai/microsoft-extensions-ai#tool-calling
// https://learn.microsoft.com/en-us/agent-framework/migration-guide/from-semantic-kernel/?pivots=programming-language-csharp
// https://learn.microsoft.com/en-us/agent-framework/tutorials/agents/function-tools?pivots=programming-language-csharp
// https://learn.microsoft.com/en-us/agent-framework/tutorials/agents/function-tools-approvals
// https://github.com/microsoft/agent-framework/blob/9148392d00dafa47a95178622f3800f89bbf0425/dotnet/samples/GettingStarted/Agents/Agent_Step15_Plugins/Program.cs#L86

/// <summary>
/// ReviewsTool Function tool is a custom code that we want the agent to be able to call when needed for getting reviews.
/// </summary>
/// <param name="sender"></param>
/// <param name="catalogServiceClient"></param>
public class ReviewsTool
{
    private readonly ISender _sender;
    private readonly ICatalogServiceClient _catalogServiceClient;

    public ReviewsTool(ISender sender, ICatalogServiceClient catalogServiceClient)
    {
        _sender = sender;
        _catalogServiceClient = catalogServiceClient;
    }

    [Description("Retrieves all reviews for a specific product through product `id` which is guid")]
    public async Task<IReadOnlyCollection<ReviewDetailsDto>> GetReviewsByProductId(
        [Description("Product `id` of type guid to get reviews for this product")] Guid productId
    )
    {
        var reviewsByProductResult = await _sender.Send(
            ProductReviews.Features.GettingReviewsByProduct.GetReviewsByProduct.Of(productId, 1, int.MaxValue)
        );

        var product =
            await _catalogServiceClient.GetProductByIdAsync(productId)
            ?? throw new InvalidOperationException($"Product {productId} not found in catalog.");

        var items = reviewsByProductResult
            .Reviews.Select(x => new ReviewDetailsDto(
                x.Id,
                x.ProductId,
                product.Name,
                product.Description,
                x.UserId,
                x.Rating,
                x.Comment,
                x.CreatedAt
            ))
            .ToList();

        return items;
    }

    [Description("Retrieves all reviews for a list of product ids (guids). Returns reviews grouped by product.")]
    public async Task<Dictionary<Guid, IReadOnlyCollection<ReviewDetailsDto>>> GetReviewsByProductIds(
        [Description("List of product ids (guids) to get reviews for")] IReadOnlyCollection<Guid> productIds
    )
    {
        if (productIds == null || productIds.Count == 0)
            throw new ArgumentException("At least one productId must be provided.", nameof(productIds));

        var products = await _catalogServiceClient.GetProductsByIdAsync(productIds);

        var productMap = products.ToDictionary(p => p.Id);

        var result = new Dictionary<Guid, IReadOnlyCollection<ReviewDetailsDto>>();

        foreach (var productId in productIds)
        {
            if (!productMap.TryGetValue(productId, out var product))
                continue;

            var reviewsByProductResult = await _sender.Send(
                ProductReviews.Features.GettingReviewsByProduct.GetReviewsByProduct.Of(productId, 1, int.MaxValue)
            );

            var items = reviewsByProductResult
                .Reviews.Select(x => new ReviewDetailsDto(
                    x.Id,
                    x.ProductId,
                    product.Name,
                    product.Description,
                    x.UserId,
                    x.Rating,
                    x.Comment,
                    x.CreatedAt
                ))
                .ToList();

            result[productId] = items;
        }

        return result;
    }

    /// <summary>
    /// Returns the functions provided by this plugin.
    /// </summary>
    public IEnumerable<AITool> AsAITools()
    {
        // https://learn.microsoft.com/en-us/agent-framework/tutorials/agents/function-tools?pivots=programming-language-csharp
        yield return AgentFrameworkExtensions.CreateFunctionToolFromMethod(
            GetReviewsByProductId,
            nameof(GetReviewsByProductId)
        );
        yield return AgentFrameworkExtensions.CreateFunctionToolFromMethod(
            GetReviewsByProductIds,
            nameof(GetReviewsByProductIds)
        );
    }

    // [Description(
    //     "Retrieves recent reviews for a product within a specified time frame through product name, description and id"
    // )]
    // public async Task<IReadOnlyCollection<ReviewDetailsDto>> GetRecentReviews(
    //     [Description("Product id to get reviews for this product")] Guid productId,
    //     [Description("Number of days to look back for recent reviews")] int daysBack = 30
    // )
    // {
    //     var reviews = (
    //         await sender.Send(
    //             ProductReviews.Features.GettingReviewsByProduct.GetReviewsByProduct.Of(productId, 1, int.MaxValue)
    //         )
    //     ).Reviews;
    //
    //     var cutoffDate = DateTime.UtcNow.AddDays(-daysBack);
    //
    //     var product =
    //         await catalogServiceClient.GetProductByIdAsync(productId)
    //         ?? throw new InvalidOperationException($"Product {productId} not found in catalog.");
    //
    //     var items = reviews
    //         .Where(r => r.CreatedAt >= cutoffDate)
    //         .Select(x => new ReviewDetailsDto(
    //             x.Id,
    //             x.ProductId,
    //             product.Name,
    //             product.Description,
    //             x.UserId,
    //             x.Rating,
    //             x.Comment,
    //             x.CreatedAt
    //         ))
    //         .ToList();
    //
    //     return items;
    // }
}
