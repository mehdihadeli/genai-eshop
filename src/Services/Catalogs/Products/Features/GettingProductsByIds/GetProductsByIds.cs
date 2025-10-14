using GenAIEshop.Catalogs.Products.Dtos;
using GenAIEshop.Catalogs.Shared.Data;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace GenAIEshop.Catalogs.Products.Features.GettingProductsByIds;

public sealed record GetProductsByIds(Guid[] ProductIds) : IQuery<GetProductsByIdsResult>;

public sealed class GetProductsByIdsHandler(CatalogsDbContext dbContext)
    : IQueryHandler<GetProductsByIds, GetProductsByIdsResult>
{
    public async ValueTask<GetProductsByIdsResult> Handle(GetProductsByIds query, CancellationToken cancellationToken)
    {
        var products = await dbContext
            .Products.Where(p => query.ProductIds.Contains(p.Id))
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        var productsDto = products.Select(x => x.ToDto());

        return new GetProductsByIdsResult(productsDto);
    }
}

public sealed record GetProductsByIdsResult(IEnumerable<ProductDto> Products);
