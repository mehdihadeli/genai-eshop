using BuildingBlocks.Extensions;
using GenAIEshop.Catalogs.Products.Dtos;
using GenAIEshop.Catalogs.Shared.Data;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace GenAIEshop.Catalogs.Products.Features.GettingProducts;

public sealed record GetProducts(int PageNumber, int PageSize) : IQuery<GetProductsResult>
{
    public static GetProducts Of(int pageNumber = 1, int pageSize = 10)
    {
        return new GetProducts(pageNumber.NotBeNegativeOrZero(), pageSize.NotBeNegativeOrZero());
    }

    public int Skip => (PageNumber - 1) * PageSize;
}

public sealed class GetProductsHandler(CatalogsDbContext dbContext, ILogger<GetProductsHandler> logger)
    : IQueryHandler<GetProducts, GetProductsResult>
{
    public async ValueTask<GetProductsResult> Handle(GetProducts query, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Fetching products page {PageNumber} with size {PageSize}",
            query.PageNumber,
            query.PageSize
        );

        var totalCount = await dbContext.Products.CountAsync(cancellationToken);

        var productDtos = await dbContext
            .Products.Where(x => x.IsAvailable == true)
            .OrderBy(p => p.Name)
            .Skip(query.Skip)
            .Take(query.PageSize)
            .Select(p => p.ToDto())
            .ToListAsync(cancellationToken: cancellationToken);

        return new GetProductsResult(
            Products: productDtos.AsReadOnly(),
            PageSize: query.PageSize,
            TotalCount: totalCount
        );
    }
}

public sealed record GetProductsResult(IReadOnlyCollection<ProductDto> Products, int PageSize, int TotalCount)
{
    public int PageCount => (int)Math.Ceiling(TotalCount / (double)PageSize);
};
