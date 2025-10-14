using GenAIEshop.Catalogs.Products.Dtos;
using GenAIEshop.Catalogs.Shared.Data;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace GenAIEshop.Catalogs.Products.Features.GettingProductById;

public sealed record GetProductById(Guid ProductId) : IQuery<GetProductByIdResult>;

public sealed class GetProductByIdHandler(CatalogsDbContext dbContext, ILogger<GetProductByIdHandler> logger)
    : IQueryHandler<GetProductById, GetProductByIdResult>
{
    public async ValueTask<GetProductByIdResult> Handle(GetProductById query, CancellationToken cancellationToken)
    {
        logger.LogInformation("Fetching product {ProductId}", query.ProductId);

        var product = await dbContext.Products.FirstOrDefaultAsync(p => p.Id == query.ProductId, cancellationToken);

        if (product == null)
            return new GetProductByIdResult(null);

        var productDto = product.ToDto();

        return new GetProductByIdResult(productDto);
    }
}

public sealed record GetProductByIdResult(ProductDto? Product);
