using BuildingBlocks.Extensions;
using BuildingBlocks.VectorDB.Contracts;
using GenAIEshop.Catalogs.Products.Models;
using GenAIEshop.Catalogs.Shared.Data;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace GenAIEshop.Catalogs.Products.Features.DeletingProduct;

public sealed record DeleteProduct(Guid ProductId) : ICommand<Unit>
{
    public static DeleteProduct Of(Guid productId)
    {
        productId.NotBeEmpty();
        return new DeleteProduct(productId);
    }
}

public sealed class DeleteProductHandler(
    CatalogsDbContext dbContext,
    IDataIngestor<Product> productVectorIngestor,
    ILogger<DeleteProductHandler> logger
) : ICommandHandler<DeleteProduct, Unit>
{
    public async ValueTask<Unit> Handle(DeleteProduct command, CancellationToken cancellationToken)
    {
        logger.LogInformation("Deleting product {ProductId}", command.ProductId);

        var product =
            await dbContext.Products.FirstOrDefaultAsync(p => p.Id == command.ProductId, cancellationToken)
            ?? throw new InvalidOperationException("Product not found.");

        dbContext.Products.Remove(product);
        await dbContext.SaveChangesAsync(cancellationToken);

        // delete vector data
        await productVectorIngestor.DeleteDataAsync(product, cancellationToken);

        return Unit.Value;
    }
}
