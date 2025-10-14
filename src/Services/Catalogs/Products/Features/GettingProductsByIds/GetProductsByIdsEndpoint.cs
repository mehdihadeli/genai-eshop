using GenAIEshop.Catalogs.Products.Dtos;
using GenAIEshop.Catalogs.Products.Features.GettingProductsByIds;
using Mediator;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace GenAIEshop.Catalogs.Products.Features.GettingProductById;

public static class GetProductsByIdsEndpoint
{
    public static RouteHandlerBuilder MapGetProductsByIdsEndpoint(this IEndpointRouteBuilder app)
    {
        return app.MapGet("/by-ids", HandleAsync)
            .WithName(nameof(GetProductsByIds))
            .WithDisplayName("Get Products By IDs")
            .WithSummary("Retrieves multiple products by their unique identifiers.")
            .WithDescription("Returns a list of products matching the provided IDs.")
            .Produces<IEnumerable<ProductDto>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest);
    }

    static async Task<Results<Ok<IEnumerable<ProductDto>>, BadRequest<string>>> HandleAsync(
        [AsParameters] GetProductsByIdsRequestParameters parameters
    )
    {
        var (ids, sender, ct) = parameters;

        if (ids is null || ids.Length == 0)
        {
            return TypedResults.BadRequest(
                "Query parameter 'ids' is required (repeatable), e.g. ?ids={guid}&ids={guid2}"
            );
        }

        var result = await sender.Send(new GetProductsByIds(ids), ct);
        return TypedResults.Ok(result.Products);
    }
}

public sealed record GetProductsByIdsRequestParameters(
    [FromQuery(Name = "ids")] Guid[]? ProductIds,
    ISender Sender,
    CancellationToken CancellationToken
);
