using BuildingBlocks.Extensions;
using BuildingBlocks.VectorDB.Contracts;
using GenAIEshop.Catalogs.Products.Dtos;
using GenAIEshop.Catalogs.Products.Models;
using GenAIEshop.Catalogs.Shared.Data;
using Mediator;

namespace GenAIEshop.Catalogs.Products.Features.CreatingProduct;

public sealed record CreateProduct(string Name, string Description, decimal Price, string ImageUrl, bool IsAvailable)
    : ICommand<CreateProductResult>
{
    public static CreateProduct Of(string? name, string? description, decimal price, string? imageUrl, bool isAvailable)
    {
        name.NotBeNullOrWhiteSpace();
        price.NotBeNegativeOrZero();
        description.NotBeNullOrWhiteSpace();
        imageUrl.NotBeNullOrWhiteSpace();

        return new CreateProduct(name, description, price, imageUrl, isAvailable);
    }
}

public sealed class CreateProductHandler(
    CatalogsDbContext dbContext,
    IDataIngestor<Product> productVectorIngestor,
    ILogger<CreateProductHandler> logger
) : ICommandHandler<CreateProduct, CreateProductResult>
{
    public async ValueTask<CreateProductResult> Handle(CreateProduct command, CancellationToken cancellationToken)
    {
        logger.LogInformation("Creating product '{ProductName}'", command.Name);

        var product = new Product
        {
            Name = command.Name,
            Description = command.Description,
            Price = command.Price,
            IsAvailable = command.IsAvailable,
            ImageUrl = command.ImageUrl,
        };

        await dbContext.Products.AddAsync(product, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        var productDto = product.ToDto();

        // add product to vector db for semantic search
        await productVectorIngestor.IngestDataAsync(product, cancellationToken);

        logger.LogInformation("Product {ProductId} has been created.", product.Id);

        return new CreateProductResult(productDto);
    }
}

public sealed record CreateProductResult(ProductDto Product);
