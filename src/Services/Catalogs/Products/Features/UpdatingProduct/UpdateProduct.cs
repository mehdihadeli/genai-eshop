using BuildingBlocks.Extensions;
using BuildingBlocks.VectorDB.Contracts;
using GenAIEshop.Catalogs.Products.Dtos;
using GenAIEshop.Catalogs.Products.Models;
using GenAIEshop.Catalogs.Shared.Data;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace GenAIEshop.Catalogs.Products.Features.UpdatingProduct;

public sealed record UpdateProduct(
    Guid ProductId,
    string Name,
    string Description,
    decimal Price,
    string ImageUrl,
    bool IsAvailable
) : ICommand<UpdateProductResult>
{
    public static UpdateProduct Of(
        Guid productId,
        string? name,
        string? description,
        decimal price,
        string? imageUrl,
        bool isAvailable
    )
    {
        productId.NotBeEmpty();
        name.NotBeNullOrWhiteSpace();
        description.NotBeNullOrWhiteSpace();
        imageUrl.NotBeNullOrWhiteSpace();
        price.NotBeNegativeOrZero();

        return new UpdateProduct(productId, name, description, price, imageUrl, isAvailable);
    }
}

public sealed class UpdateProductHandler(
    CatalogsDbContext dbContext,
    IDataIngestor<Product> productVectorIngestor,
    ILogger<UpdateProductHandler> logger
) : ICommandHandler<UpdateProduct, UpdateProductResult>
{
    public async ValueTask<UpdateProductResult> Handle(UpdateProduct command, CancellationToken cancellationToken)
    {
        logger.LogInformation("Updating product {ProductId}", command.ProductId);

        var product =
            await dbContext.Products.FirstOrDefaultAsync(p => p.Id == command.ProductId, cancellationToken)
            ?? throw new InvalidOperationException("Product not found.");

        product.Name = command.Name;
        product.Description = command.Description;
        product.Price = command.Price;
        product.ImageUrl = command.ImageUrl;
        product.IsAvailable = command.IsAvailable;

        await dbContext.SaveChangesAsync(cancellationToken);

        var productDto = product.ToDto();

        // add product to vector db for semantic search
        await productVectorIngestor.IngestDataAsync(product, cancellationToken);

        logger.LogInformation("Product {ProductId} has been updated.", product.Id);

        return new UpdateProductResult(productDto);
    }
}

public sealed record UpdateProductResult(ProductDto Product);
